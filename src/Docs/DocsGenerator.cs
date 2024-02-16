using ICSharpCode.Decompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Mono.Cecil;

using Newtonsoft.Json;
using Oxide.Patcher.Common;
using Oxide.Patcher.Hooks;

namespace Oxide.Patcher.Docs
{
    public static class DocsGenerator
    {
        internal static AssemblyLoader AssemblyLoader;

        public static void GenerateFile(Project project, string outputFile = "docs.json", Dictionary<string, AssemblyDefinition> patchedAssemblies = null, bool isConsole = false)
        {
            AssemblyLoader = PatcherForm.MainForm != null ? PatcherForm.MainForm.AssemblyLoader : new AssemblyLoader(project, string.Empty);
            DocsData docsData = new DocsData();
            List<DocsHook> hooks = new List<DocsHook>();

            Dictionary<string, AssemblyDefinition> assemblies = patchedAssemblies ?? new Patching.Patcher(project, isConsole).Patch(false, false);

            foreach (Manifest manifest in project.Manifests)
            {
                if (!assemblies.TryGetValue(manifest.AssemblyName, out AssemblyDefinition assembly))
                {
                    continue;
                }

                foreach (Hook hook in manifest.Hooks)
                {
                    try
                    {
                        MethodDefinition methodDef = GetMethod(assembly, hook.TypeName, hook.Signature);
                        if (methodDef == null)
                        {
                            throw new Exception($"Failed to find method definition for hook {hook.Name}");
                        }

                        DocsHook docsHook = new DocsHook(hook, methodDef, project.TargetDirectory);
                        hooks.Add(docsHook);
                    }
                    catch (NotSupportedException) { }
                    catch (DecompilerException)
                    {
                        Console.WriteLine($"Failed to decompile method for hook {hook.Name}");
                    }
                    catch (Exception e)
                    {
                        if (PatcherForm.MainForm != null)
                        {
                            MessageBox.Show($"There was an error while generating docs data for '{hook.Name}'. ({e})", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            Console.WriteLine($"There was an error while generating docs data for '{hook.Name}'. ({e})");
                        }
                    }
                }
            }

            docsData.Hooks = hooks.ToArray();

            //Save file
            File.WriteAllText(outputFile, JsonConvert.SerializeObject(docsData, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            }));

            if (PatcherForm.MainForm != null)
            {
                MessageBox.Show("Successfully generated docs data file.", "Oxide Patcher",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                PatcherForm.MainForm.Invoke((MethodInvoker)delegate
                {
                    PatcherForm.MainForm.SetDocsButtonEnabled(true);
                });
            }
        }

        private static MethodDefinition GetMethod(AssemblyDefinition assemblyDefinition, string typeName, MethodSignature signature)
        {
            try
            {
                TypeDefinition type = assemblyDefinition.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == typeName);
                return type.Methods.Single(m => MethodSignatureMatches(Utility.GetMethodSignature(m), signature));
            }
            catch (Exception e)
            {
                return null;
            }
        }

        // Ignore exposure for now
        private static bool MethodSignatureMatches(MethodSignature obj1, MethodSignature othersig)
        {
            if (obj1.Name != othersig.Name)
            {
                return false;
            }

            if (obj1.Parameters.Length != othersig.Parameters.Length)
            {
                return false;
            }

            for (int i = 0; i < obj1.Parameters.Length; i++)
            {
                if (obj1.Parameters[i] != othersig.Parameters[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
