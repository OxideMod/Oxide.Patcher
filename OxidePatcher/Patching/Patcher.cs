using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Mono.Cecil;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

using OxidePatcher.Modifiers;

namespace OxidePatcher.Patching
{
    /// <summary>
    /// Responsible for performing the actual patch process
    /// </summary>
    public class Patcher
    {
        /// <summary>
        /// Gets the project that this patcher will patch
        /// </summary>
        public Project PatchProject { get; private set; }

        /// <summary>
        /// Is this a Console or Window Patcher?
        /// </summary>
        public bool IsConsole { get; private set; }

        /// <summary>
        /// Called when a log message has been written
        /// </summary>
        public event Action<string> OnLogMessage;

        /// <summary>
        /// Directory where the patcher resides
        /// </summary>
        public string Directory { get; private set; }

        /// <summary>
        /// Initializes a new instance of the Patcher class
        /// </summary>
        /// <param name="patchproject"></param>
        /// <param name="console"></param>
        public Patcher(Project patchproject, bool console = false)
        {
            PatchProject = patchproject;
            IsConsole = console;
            Directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Gets the correct Assembly FilePath
        /// </summary>
        /// <param name="assemblyname"></param>
        /// <param name="original"></param>
        private string GetAssemblyFilename(string assemblyname, bool original)
        {
            if (original)
                return Path.Combine(PatchProject.TargetDirectory, Path.GetFileNameWithoutExtension(assemblyname) + "_Original" + Path.GetExtension(assemblyname));
            return Path.Combine(PatchProject.TargetDirectory, assemblyname);
        }

        /// <summary>
        /// Logs to console / window output and to log file
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        internal void Log(string format, params object[] args)
        {
            var line = string.Format(format, args);
            WriteToLog(line);
            if (IsConsole) Console.WriteLine(line);
            else OnLogMessage?.Invoke(line);
        }

        /// <summary>
        /// Writes text to log file
        /// </summary>
        /// <param name="line"></param>
        private void WriteToLog(string line)
        {
            using (var file = new StreamWriter(Path.Combine(Directory, "log.txt"), true))
            {
                file.WriteLine(line);
            }
        }

        /// <summary>
        /// Performs the patch process
        /// </summary>
        public void Patch()
        {
            // Load oxide assembly
            string oxidefilename = Path.Combine(System.Windows.Forms.Application.StartupPath, "Oxide.Core.dll");
            if (!File.Exists(oxidefilename)) throw new FileNotFoundException("Failed to locate Oxide.Core.dll assembly");
            AssemblyDefinition oxideassembly = AssemblyDefinition.ReadAssembly(oxidefilename);
            if (PatchProject == null)
                return;
            // CReate reader params
            ReaderParameters readerparams = new ReaderParameters();
            readerparams.AssemblyResolver = new AssemblyResolver { TargetDirectory = PatchProject.TargetDirectory };
            DateTime now = DateTime.Now;
            WriteToLog("----------------------------------------");
            WriteToLog(now.ToShortDateString() + " " + now.ToString("hh:mm:ss tt zzz"));
            WriteToLog("----------------------------------------");
            // Loop each manifest
            foreach (var manifest in PatchProject.Manifests)
            {
                // Get the assembly filename
                string filename;
                if (!IsConsole)
                {
                    filename = GetAssemblyFilename(manifest.AssemblyName, true);
                    if (!File.Exists(filename))
                    {
                        WriteToLog(string.Format("Failed to locate target assembly {0}", manifest.AssemblyName));
                        throw new FileNotFoundException(string.Format("Failed to locate target assembly {0}", manifest.AssemblyName), filename);
                    }
                }
                else
                {
                    filename = GetAssemblyFilename(manifest.AssemblyName, true);
                    if (!File.Exists(filename))
                    {
                        filename = GetAssemblyFilename(manifest.AssemblyName, false);
                        if (!File.Exists(filename))
                        {
                            WriteToLog(string.Format("Failed to locate target assembly {0}", manifest.AssemblyName));
                            throw new FileNotFoundException(string.Format("Failed to locate target assembly {0}", manifest.AssemblyName), filename);
                        }
                        else
                        {
                            System.IO.File.Copy(filename, Path.GetFileNameWithoutExtension(filename) + "_Original" + Path.GetExtension(filename), true);
                            filename = Path.GetFileNameWithoutExtension(filename) + "_Original" + Path.GetExtension(filename);
                        }
                    }
                }

                // Load it
                Log("Loading assembly {0}", manifest.AssemblyName);
                AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(filename, readerparams);

                var baseHooks = (from hook in manifest.Hooks where hook.BaseHook != null select hook.BaseHook).ToList();
                var cloneHooks = manifest.Hooks.Where(hook => hook.BaseHook != null).ToDictionary(hook => hook.BaseHook);

                // Loop each hook
                foreach (var hook in manifest.Hooks)
                {
                    var cloneFlagged = false;
                    if (cloneHooks.ContainsKey(hook))
                        cloneFlagged = cloneHooks[hook].Flagged;

                    if (baseHooks.Contains(hook) && !hook.Flagged && !cloneFlagged) continue;
                    // Check if it's flagged
                    if (hook.BaseHook != null)
                    {
                        if (hook.BaseHook.Flagged)
                        {
                            Log("Ignored hook {0} as its base hook {1} is flagged", hook.Name, hook.BaseHook.Name);
                            continue;
                        }
                    }
                    if (hook.Flagged)
                    {
                        // Log
                        Log("Ignored hook {0} as it is flagged", hook.Name);
                    }
                    else
                    {
                        // Locate the method
                        MethodDefinition method;
                        try
                        {
                            var type = assembly.Modules
                                .SelectMany((m) => m.GetTypes())
                                .Single((t) => t.FullName == hook.TypeName);

                            method = type.Methods
                                .Single((m) => Utility.GetMethodSignature(m).Equals(hook.Signature));
                        }
                        catch (Exception)
                        {
                            WriteToLog(string.Format("Failed to locate method {0}::{1} in assembly {2}", hook.TypeName, hook.Signature.Name, manifest.AssemblyName));
                            throw new Exception(string.Format("Failed to locate method {0}::{1} in assembly {2}", hook.TypeName, hook.Signature.Name, manifest.AssemblyName));
                        }

                        // Let the hook do it's work
                        var weaver = new ILWeaver(method.Body) {Module = method.Module};
                        try
                        {
                            // Apply
                            bool patchApplied = hook.PreparePatch(method, weaver, oxideassembly, this) && hook.ApplyPatch(method, weaver, oxideassembly, this);
                            if (patchApplied)
                            {
                                weaver.Apply(method.Body);
                                var bhook = hook;
                                if (bhook.BaseHook != null)
                                {
                                    var patchedHooks = new List<string> { hook.Name };
                                    while (bhook.BaseHook != null)
                                    {
                                        bhook = hook.BaseHook;
                                        patchedHooks.Add(bhook.Name);
                                    }
                                    patchedHooks.Reverse();
                                    Log("Applied hooks {0} to {1}::{2}", string.Join(", ", patchedHooks), bhook.TypeName, bhook.Signature.Name);
                                }
                                else
                                {
                                    Log("Applied hook {0} to {1}::{2}", hook.Name, hook.TypeName, hook.Signature.Name);
                                }
                            }
                            else
                            {
                                Log("Failed to apply hook {0}, invalid injextion index specified!", hook.Name);
                                hook.Flagged = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("Failed to apply hook {0}", hook.Name);
                            Log(ex.ToString());
                        }
                    }
                }

                // Loop each access modifier
                foreach (var modifier in manifest.Modifiers)
                {
                    if (modifier.Flagged)
                    {
                        // Log
                        Log($"Ignored modifier changes to {modifier.Name} as it is flagged");
                    }
                    else
                    {
                        switch (modifier.Type)
                        {
                            case ModifierType.Field:
                                FieldDefinition field;
                                try
                                {
                                    var type = assembly.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == modifier.TypeName);
                                    field = type.Fields.Single(m => Utility.GetModifierSignature(m).Equals(modifier.Signature));
                                }
                                catch (Exception)
                                {
                                    WriteToLog($"Failed to locate field {modifier.TypeName}::{modifier.Signature.Name} in assembly {manifest.AssemblyName}");
                                    throw new Exception($"Failed to locate field {modifier.TypeName}::{modifier.Signature.Name} in assembly {manifest.AssemblyName}");
                                }

                                if (modifier.Signature.Exposure[0] != modifier.TargetExposure[0])
                                {
                                    switch (modifier.Signature.Exposure[0])
                                    {
                                        case Exposure.Private:
                                            field.Attributes -= FieldAttributes.Private;
                                            break;
                                        case Exposure.Protected:
                                            field.Attributes -= FieldAttributes.Family;
                                            break;
                                        case Exposure.Public:
                                            field.Attributes -= FieldAttributes.Public;
                                            break;
                                        case Exposure.Internal:
                                            field.Attributes -= FieldAttributes.Assembly;
                                            break;
                                    }

                                    switch (modifier.TargetExposure[0])
                                    {
                                        case Exposure.Private:
                                            field.Attributes |= FieldAttributes.Private;
                                            break;
                                        case Exposure.Protected:
                                            field.Attributes |= FieldAttributes.Family;
                                            break;
                                        case Exposure.Public:
                                            field.Attributes |= FieldAttributes.Public;
                                            break;
                                        case Exposure.Internal:
                                            field.Attributes |= FieldAttributes.Assembly;
                                            break;
                                    }
                                }

                                switch (modifier.TargetExposure.Length)
                                {
                                    case 1:
                                        if (field.IsStatic)
                                            field.Attributes -= FieldAttributes.Static;
                                        break;
                                    case 2:
                                        if (!field.IsStatic)
                                            field.Attributes |= FieldAttributes.Static;
                                        break;
                                }

                                Log($"Applied modifier changes to field {modifier.TypeName}::{modifier.Name}");
                                break;
                            case ModifierType.Method:
                                MethodDefinition method;
                                try
                                {
                                    var type = assembly.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == modifier.TypeName);
                                    method = type.Methods.Single(m => Utility.GetModifierSignature(m).Equals(modifier.Signature));
                                }
                                catch (Exception)
                                {
                                    WriteToLog($"Failed to locate method {modifier.TypeName}::{modifier.Signature.Name} in assembly {manifest.AssemblyName}");
                                    throw new Exception($"Failed to locate method {modifier.TypeName}::{modifier.Signature.Name} in assembly {manifest.AssemblyName}");
                                }

                                if (modifier.Signature.Exposure[0] != modifier.TargetExposure[0])
                                {
                                    switch (modifier.Signature.Exposure[0])
                                    {
                                        case Exposure.Private:
                                            method.Attributes -= MethodAttributes.Private;
                                            break;
                                        case Exposure.Protected:
                                            method.Attributes -= MethodAttributes.Family;
                                            break;
                                        case Exposure.Public:
                                            method.Attributes -= MethodAttributes.Public;
                                            break;
                                        case Exposure.Internal:
                                            method.Attributes -= MethodAttributes.Assembly;
                                            break;
                                    }

                                    switch (modifier.TargetExposure[0])
                                    {
                                        case Exposure.Private:
                                            method.Attributes |= MethodAttributes.Private;
                                            break;
                                        case Exposure.Protected:
                                            method.Attributes |= MethodAttributes.Family;
                                            break;
                                        case Exposure.Public:
                                            method.Attributes |= MethodAttributes.Public;
                                            break;
                                        case Exposure.Internal:
                                            method.Attributes |= MethodAttributes.Assembly;
                                            break;
                                    }
                                }

                                switch (modifier.TargetExposure.Length)
                                {
                                    case 1:
                                        if (method.IsStatic)
                                            method.Attributes -= MethodAttributes.Static;
                                        break;
                                    case 2:
                                        if (!method.IsStatic)
                                            method.Attributes |= MethodAttributes.Static;
                                        break;
                                }

                                Log($"Applied modifier changes to method {modifier.TypeName}::{modifier.Signature.Name}");
                                break;
                            case ModifierType.Property:
                                PropertyDefinition property;
                                try
                                {
                                    var type = assembly.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == modifier.TypeName);
                                    property = type.Properties.Single(m => Utility.GetModifierSignature(m).Equals(modifier.Signature));
                                }
                                catch (Exception)
                                {
                                    WriteToLog($"Failed to locate property {modifier.TypeName}::{modifier.Signature.Name} in assembly {manifest.AssemblyName}");
                                    throw new Exception($"Failed to locate property {modifier.TypeName}::{modifier.Signature.Name} in assembly {manifest.AssemblyName}");
                                }

                                if (property.GetMethod != null && modifier.Signature.Exposure[0] != modifier.TargetExposure[0])
                                {
                                    switch (modifier.Signature.Exposure[0])
                                    {
                                        case Exposure.Private:
                                            property.GetMethod.Attributes -= MethodAttributes.Private;
                                            break;
                                        case Exposure.Protected:
                                            property.GetMethod.Attributes -= MethodAttributes.Family;
                                            break;
                                        case Exposure.Public:
                                            property.GetMethod.Attributes -= MethodAttributes.Public;
                                            break;
                                        case Exposure.Internal:
                                            property.GetMethod.Attributes -= MethodAttributes.Assembly;
                                            break;
                                    }

                                    switch (modifier.TargetExposure[0])
                                    {
                                        case Exposure.Private:
                                            property.GetMethod.Attributes |= MethodAttributes.Private;
                                            break;
                                        case Exposure.Protected:
                                            property.GetMethod.Attributes |= MethodAttributes.Family;
                                            break;
                                        case Exposure.Public:
                                            property.GetMethod.Attributes |= MethodAttributes.Public;
                                            break;
                                        case Exposure.Internal:
                                            property.GetMethod.Attributes |= MethodAttributes.Assembly;
                                            break;
                                    }
                                }

                                if (property.SetMethod != null && modifier.Signature.Exposure[1] != modifier.TargetExposure[1])
                                {
                                    switch (modifier.Signature.Exposure[1])
                                    {
                                        case Exposure.Private:
                                            property.SetMethod.Attributes -= MethodAttributes.Private;
                                            break;
                                        case Exposure.Protected:
                                            property.SetMethod.Attributes -= MethodAttributes.Family;
                                            break;
                                        case Exposure.Public:
                                            property.SetMethod.Attributes -= MethodAttributes.Public;
                                            break;
                                        case Exposure.Internal:
                                            property.SetMethod.Attributes -= MethodAttributes.Assembly;
                                            break;
                                    }

                                    switch (modifier.TargetExposure[1])
                                    {
                                        case Exposure.Private:
                                            property.SetMethod.Attributes |= MethodAttributes.Private;
                                            break;
                                        case Exposure.Protected:
                                            property.SetMethod.Attributes |= MethodAttributes.Family;
                                            break;
                                        case Exposure.Public:
                                            property.SetMethod.Attributes |= MethodAttributes.Public;
                                            break;
                                        case Exposure.Internal:
                                            property.SetMethod.Attributes |= MethodAttributes.Assembly;
                                            break;
                                    }
                                }

                                switch (modifier.TargetExposure.Length)
                                {
                                    case 1:
                                        if (property.GetMethod != null && property.GetMethod.IsStatic)
                                            property.GetMethod.Attributes -= MethodAttributes.Static;
                                        if (property.SetMethod != null && property.SetMethod.IsStatic)
                                            property.SetMethod.Attributes -= MethodAttributes.Static;
                                        break;
                                    case 2:
                                        if (property.GetMethod != null && property.SetMethod == null && !property.GetMethod.IsStatic)
                                            property.GetMethod.Attributes |= MethodAttributes.Static;
                                        break;
                                    case 3:
                                        if (property.GetMethod != null && !property.GetMethod.IsStatic)
                                            property.GetMethod.Attributes |= MethodAttributes.Static;
                                        if (property.SetMethod != null && !property.SetMethod.IsStatic)
                                            property.SetMethod.Attributes |= MethodAttributes.Static;
                                        break;
                                }

                                Log($"Applied modifier changes to property {modifier.TypeName}::{modifier.Name}");
                                break;
                            case ModifierType.Type:
                                TypeDefinition typedef;
                                try
                                {
                                    typedef = assembly.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == modifier.TypeName);
                                }
                                catch (Exception)
                                {
                                    WriteToLog($"Failed to locate type {modifier.TypeName} in assembly {manifest.AssemblyName}");
                                    throw new Exception($"Failed to locate type {modifier.TypeName} in assembly {manifest.AssemblyName}");
                                }

                                if (modifier.Signature.Exposure[0] != modifier.TargetExposure[0])
                                {
                                    switch (modifier.Signature.Exposure[0])
                                    {
                                        case Exposure.Private:
                                            if (typedef.IsNested)
                                                typedef.Attributes -= TypeAttributes.NestedPrivate;
                                            else
                                                typedef.Attributes -= TypeAttributes.NotPublic;
                                            break;
                                        case Exposure.Public:
                                            if (typedef.IsNested)
                                                typedef.Attributes -= TypeAttributes.NestedPublic;
                                            else
                                                typedef.Attributes -= TypeAttributes.Public;
                                            break;
                                    }

                                    switch (modifier.TargetExposure[0])
                                    {
                                        case Exposure.Private:
                                            if (typedef.IsNested)
                                                typedef.Attributes |= TypeAttributes.NestedPrivate;
                                            else
                                                typedef.Attributes |= TypeAttributes.NotPublic;
                                            break;
                                        case Exposure.Public:
                                            if (typedef.IsNested)
                                                typedef.Attributes |= TypeAttributes.NestedPublic;
                                            else
                                                typedef.Attributes |= TypeAttributes.Public;
                                            break;
                                    }
                                }

                                switch (modifier.TargetExposure.Length)
                                {
                                    case 1:
                                        if (typedef.IsAbstract && typedef.IsSealed)
                                            typedef.Attributes -= TypeAttributes.Abstract | TypeAttributes.Sealed;
                                        break;
                                    case 2:
                                        if (!typedef.IsAbstract && !typedef.IsSealed)
                                            typedef.Attributes |= TypeAttributes.Abstract | TypeAttributes.Sealed;
                                        break;
                                }

                                Log($"Applied modifier changes to type {modifier.TypeName}");
                                break;
                        }
                    }
                }

                // Save it
                Log("Saving assembly {0}", manifest.AssemblyName);
                filename = GetAssemblyFilename(manifest.AssemblyName, false);
                assembly.Write(filename);
            }
        }
    }
}
