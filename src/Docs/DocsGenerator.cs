using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using Mono.Cecil;

using Newtonsoft.Json;
using Oxide.Patcher.Common;
using Oxide.Patcher.Hooks;

namespace Oxide.Patcher.Docs
{
    public static class DocsGenerator
    {
        private static Task _worker;

        public static void GenerateFile(string targetOpj, string outputFile, string overrideDirectory = "")
        {
            Project project = Project.Load(targetOpj, overrideDirectory);
            if (project == null)
            {
                throw new Exception($"Failed to load opj {targetOpj}");
            }

            GenerateFile(project, outputFile);
        }

        public static void GenerateFile(Project project, string outputFile = "docs.json")
        {
            if (_worker?.IsCompleted == false)
            {
                if (PatcherForm.MainForm != null)
                {
                    MessageBox.Show("Docs data file is already being generated, please wait.", "Oxide Patcher",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                return;
            }

            _worker = Task.Run(() =>
            {
                DocsData docsData = new DocsData();
                List<DocsHook> hooks = new List<DocsHook>();

                Dictionary<string, AssemblyDefinition> assemblies = new Patching.Patcher(project).Patch(false, false);

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
                        catch (Exception e)
                        {
                            MessageBox.Show($"There was an error while generating docs data for '{hook.Name}'. ({e})", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            });
        }

        private static MethodDefinition GetMethod(AssemblyDefinition assemblyDefinition, string typeName, MethodSignature signature)
        {
            try
            {
                TypeDefinition type = assemblyDefinition.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == typeName);
                return type.Methods.Single(m => Utility.GetMethodSignature(m).Equals(signature));
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
