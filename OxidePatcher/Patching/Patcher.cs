using System;
using System.IO;
using System.Linq;

using Mono.Cecil;

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
        /// Initializes a new instance of the Patcher class
        /// </summary>
        /// <param name="patchproject"></param>
        /// <param name="console"></param>
        public Patcher(Project patchproject, bool console = false)
        {
            PatchProject = patchproject;
            IsConsole = console;
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
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("log.txt", true))
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
            if (!File.Exists(oxidefilename)) throw new FileNotFoundException("Failed to locate Oxide.dll assembly");
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
                                Log("Applied hook {0} to {1}::{2}", hook.Name, hook.TypeName, hook.Signature.Name);
                                weaver.Apply(method.Body);
                            }
                            else
                            {
                                Log("Failed to apply hook {0}", hook.Name);
                                Log("The injection index specified for {0} is invalid!", hook.Name);
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
                                    var type = assembly.Modules
                                        .SelectMany((m) => m.GetTypes())
                                        .Single((t) => t.FullName == modifier.TypeName);

                                    field = type.Fields
                                        .Single(m => Utility.GetModifierSignature(m).Equals(modifier.Signature));
                                }
                                catch (Exception)
                                {
                                    Log($"Failed to locate method {modifier.TypeName}::{modifier.Signature.Name} in assembly {manifest.AssemblyName}");
                                    throw new Exception($"Failed to locate method {modifier.TypeName}::{modifier.Signature.Name} in assembly {manifest.AssemblyName}");
                                }

                                field.Attributes -= modifier.Signature.Exposure[0] == Exposure.Public ? FieldAttributes.Public : FieldAttributes.Private;
                                field.Attributes |= modifier.TargetExposure[0] == Exposure.Public ? FieldAttributes.Public : FieldAttributes.Private;
                                if (modifier.TargetExposure.Length == 2)
                                {
                                    if (!field.IsStatic)
                                        field.Attributes |= FieldAttributes.Static;
                                    else if (field.IsStatic)
                                        field.Attributes -= FieldAttributes.Static;
                                }
                                Log($"Applied modifier changes to {modifier.TypeName}::{modifier.Name}");
                                break;
                            case ModifierType.Method:
                                MethodDefinition method;
                                try
                                {
                                    var type = assembly.Modules
                                        .SelectMany((m) => m.GetTypes())
                                        .Single((t) => t.FullName == modifier.TypeName);

                                    method = type.Methods
                                        .Single(m => Utility.GetModifierSignature(m).Equals(modifier.Signature));
                                }
                                catch (Exception)
                                {
                                    Log($"Failed to locate method {modifier.TypeName}::{modifier.Signature.Name} in assembly {manifest.AssemblyName}");
                                    throw new Exception($"Failed to locate method {modifier.TypeName}::{modifier.Signature.Name} in assembly {manifest.AssemblyName}");
                                }

                                method.Attributes -= modifier.Signature.Exposure[0] == Exposure.Public ? MethodAttributes.Public : MethodAttributes.Private;
                                method.Attributes |= modifier.TargetExposure[0] == Exposure.Public ? MethodAttributes.Public : MethodAttributes.Private;
                                if (modifier.TargetExposure.Length == 2)
                                {
                                    if (!method.IsStatic)
                                        method.Attributes |= MethodAttributes.Static;
                                    else if (method.IsStatic)
                                        method.Attributes-= MethodAttributes.Static;
                                }
                                Log($"Applied modifier changes to {modifier.TypeName}::{modifier.Signature.Name}");
                                break;
                            case ModifierType.Property:
                                PropertyDefinition property;
                                try
                                {
                                    var type = assembly.Modules
                                        .SelectMany((m) => m.GetTypes())
                                        .Single((t) => t.FullName == modifier.TypeName);

                                    property = type.Properties
                                        .Single(m => Utility.GetModifierSignature(m).Equals(modifier.Signature));
                                }
                                catch (Exception)
                                {
                                    Log($"Failed to locate method {modifier.TypeName}::{modifier.Signature.Name} in assembly {manifest.AssemblyName}");
                                    throw new Exception($"Failed to locate method {modifier.TypeName}::{modifier.Signature.Name} in assembly {manifest.AssemblyName}");
                                }

                                if (property.GetMethod != null)
                                {
                                    property.GetMethod.Attributes -= modifier.Signature.Exposure[0] == Exposure.Public ? MethodAttributes.Public : MethodAttributes.Private;
                                    property.GetMethod.Attributes |= modifier.TargetExposure[0] == Exposure.Public ? MethodAttributes.Public : MethodAttributes.Private;
                                }
                                if (property.SetMethod != null)
                                {
                                    property.SetMethod.Attributes -= modifier.Signature.Exposure[1] == Exposure.Public ? MethodAttributes.Public : MethodAttributes.Private;
                                    property.SetMethod.Attributes |= modifier.TargetExposure[1] == Exposure.Public ? MethodAttributes.Public : MethodAttributes.Private;
                                }

                                if ((property.SetMethod != null && modifier.TargetExposure.Length == 3) || (property.SetMethod == null && modifier.TargetExposure.Length == 2))
                                {
                                    if (property.GetMethod != null && !property.GetMethod.IsStatic)
                                        property.GetMethod.Attributes |= MethodAttributes.Static;
                                    else if (property.GetMethod != null && property.GetMethod.IsStatic)
                                        property.GetMethod.Attributes -= MethodAttributes.Static;

                                    if (property.SetMethod != null && !property.SetMethod.IsStatic)
                                        property.SetMethod.Attributes |= MethodAttributes.Static;
                                    else if (property.SetMethod != null && property.SetMethod.IsStatic)
                                        property.SetMethod.Attributes -= MethodAttributes.Static;
                                }
                                Log($"Applied modifier changes to {modifier.TypeName}::{modifier.Name}");
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
