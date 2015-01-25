using System;
using System.IO;
using System.Linq;

using Mono.Cecil;

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
        /// Called when a log message has been written
        /// </summary>
        public event Action<string> OnLogMessage;

        /// <summary>
        /// Initialises a new instance of the Patcher class
        /// </summary>
        /// <param name="patchproject"></param>
        public Patcher(Project patchproject)
        {
            PatchProject = patchproject;
        }

        private void Log(string format, params object[] args)
        {
            if (OnLogMessage != null) OnLogMessage(string.Format(format, args));
        }

        /// <summary>
        /// Performs the patch process
        /// </summary>
        public void Patch(bool console)
        {
            // Load oxide assembly
            string oxidefilename = Path.Combine(System.Windows.Forms.Application.StartupPath, "Oxide.Core.dll");
            if (!File.Exists(oxidefilename)) throw new FileNotFoundException(string.Format("Failed to locate Oxide.dll assembly"));
            AssemblyDefinition oxideassembly = AssemblyDefinition.ReadAssembly(oxidefilename);

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
                if (!console)
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
                            System.IO.File.Copy(filename, filename.Replace(".dll", "_Original.dll"), true);
                            filename = filename.Replace(".dll", "_Original.dll");
                        }
                    }
                }

                // Load it
                if (console)
                {
                    Console.WriteLine(string.Format("Loading assembly {0}", manifest.AssemblyName));
                }
                else
                {
                    Log("Loading assembly {0}", manifest.AssemblyName);
                }
                WriteToLog(string.Format("Loading assembly {0}", manifest.AssemblyName));
                AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(filename, readerparams);

                // Loop each hook
                foreach (var hook in manifest.Hooks)
                {
                    // Check if it's flagged
                    if (hook.Flagged)
                    {
                        // Log
                        if (console)
                        {
                            Console.WriteLine(string.Format("Ignored hook {0} as it is flagged", hook.Name));
                        }
                        else
                        {
                            Log("Ignored hook {0} as it is flagged", hook.Name);
                        }
                        WriteToLog(string.Format("Ignored hook {0} as it is flagged", hook.Name));
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
                        ILWeaver weaver = new ILWeaver(method.Body);
                        weaver.Module = method.Module;
                        try
                        {
                            // Apply
                            bool patchApplied = hook.ApplyPatch(method, weaver, oxideassembly, console);
                            if (patchApplied)
                            {
                                weaver.Apply(method.Body);
                            }
                            else
                            {
                                if (console)
                                {
                                    Console.WriteLine(string.Format("The injection index specified for {0} is invalid!", hook.Name));
                                }
                                WriteToLog(string.Format("The injection index specified for {0} is invalid!", hook.Name));
                                hook.Flagged = true;
                            }

                            // Log
                            if (console)
                            {
                                if (patchApplied)
                                {
                                    Console.WriteLine(string.Format("Applied hook {0} to {1}::{2}", hook.Name, hook.TypeName, hook.Signature.Name));
                                    WriteToLog(string.Format("Applied hook {0} to {1}::{2}", hook.Name, hook.TypeName, hook.Signature.Name));
                                }
                                else
                                {
                                    Console.WriteLine(string.Format("Failed to apply hook {0}", hook.Name));
                                    WriteToLog(string.Format("Failed to apply hook {0}", hook.Name));
                                }
                            }
                            else
                            {
                                if (patchApplied)
                                {
                                    Log("Applied hook {0} to {1}::{2}", hook.Name, hook.TypeName, hook.Signature.Name);
                                    WriteToLog(string.Format("Applied hook {0} to {1}::{2}", hook.Name, hook.TypeName, hook.Signature.Name));
                                }
                                else
                                {
                                    Log("Failed to apply hook {0}", hook.Name);
                                    WriteToLog(string.Format("Failed to apply hook {0}", hook.Name));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (console)
                            {
                                Console.WriteLine(string.Format("Failed to apply hook {0}", hook.Name));
                                Console.WriteLine(ex.ToString());
                            }
                            else
                            {
                                Log("Failed to apply hook {0}", hook.Name);
                                Log("{0}", ex.ToString());
                            }
                            WriteToLog(string.Format("Failed to apply hook {0}", hook.Name));
                            WriteToLog(ex.ToString());
                        }
                    }
                }

                // Save it
                if (console)
                {
                    Console.WriteLine(string.Format("Saving assembly {0}", manifest.AssemblyName));
                }
                else
                {
                    Log("Saving assembly {0}", manifest.AssemblyName);
                }
                WriteToLog(string.Format("Saving assembly {0}", manifest.AssemblyName));
                filename = GetAssemblyFilename(manifest.AssemblyName, false);
                assembly.Write(filename);
            }
        }

        private string GetAssemblyFilename(string assemblyname, bool original)
        {
            if (original)
                return Path.Combine(PatchProject.TargetDirectory, assemblyname + "_Original.dll");
            else
                return Path.Combine(PatchProject.TargetDirectory, assemblyname + ".dll");
        }

        private void WriteToLog(string line)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("log.txt", true))
            {
                file.WriteLine(line);
            }
        }
    }
}
