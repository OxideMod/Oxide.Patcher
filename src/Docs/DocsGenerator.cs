using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Windows.Forms;

using Mono.Cecil;

using Newtonsoft.Json;
using Oxide.Patcher.Common;
using Oxide.Patcher.Hooks;
using Oxide.Patcher.Patching;

namespace Oxide.Patcher.Docs
{
    public static class DocsGenerator
    {
        internal static AssemblyLoader AssemblyLoader;
        internal static string TargetDirectory;

        public static void GenerateFile(Project project, AssemblyLoader assemblyLoader, string outputFile = "docs.json")
        {
            AssemblyLoader = new AssemblyLoader(project, string.Empty);
            TargetDirectory = project.TargetDirectory;
            DocsData docsData = new DocsData();
            List<DocsHook> hooks = new List<DocsHook>();

            foreach (Manifest manifest in project.Manifests)
            {
                AssemblyDefinition docsAssembly = AssemblyLoader.LoadAssembly(manifest.AssemblyName);
                if (docsAssembly == null)
                {
                    continue;
                }

                Dictionary<Hook, Hook> cloneHooks = manifest.Hooks.Where(h => h.BaseHook != null).ToDictionary(h => h.BaseHook);
                Dictionary<string, TypeDefinition> typesByName = docsAssembly.Modules
                    .SelectMany(m => m.GetTypes())
                    .GroupBy(t => t.FullName)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (Hook hook in manifest.Hooks)
                {
                    if (!ShouldApplyPatch(hook, cloneHooks)) continue;

                    try
                    {
                        MethodDefinition methodDef = GetMethod(typesByName, hook.TypeName, hook.Signature)
                            ?? throw new Exception($"Failed to find method definition for hook {hook.Name}");

                        ILWeaver weaver = new ILWeaver(methodDef.Body) { Module = methodDef.Module };
                        hook.PreparePatch(methodDef, weaver);
                        hook.ApplyPatch(methodDef, weaver);
                        weaver.Apply(methodDef.Body);
                    }
                    catch (Exception e)
                    {
                        ReportHookError(hook, e);
                    }
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    docsAssembly.Write(stream);
                    stream.Position = 0;
                    string searchDir = Path.Combine(project.TargetDirectory, docsAssembly.Name.Name);
                    using (PEFile peFile = new PEFile(docsAssembly.Name.Name, stream, PEStreamOptions.PrefetchEntireImage))
                    {
                        UniversalAssemblyResolver resolver = new UniversalAssemblyResolver(searchDir, true, peFile.DetectTargetFrameworkId(), peFile.DetectRuntimePack());
                        CSharpDecompiler decompiler = new CSharpDecompiler(peFile, resolver, new DecompilerSettings { UsingDeclarations = true });

                        foreach (Hook hook in manifest.Hooks)
                        {
                            if (hook.Flagged)
                            {
                                Console.WriteLine($"Skipping flagged hook {hook.Name}");
                                continue;
                            }
                            if (hook.BaseHook != null && hook.BaseHook.Flagged) continue;

                            try
                            {
                                MethodDefinition methodDef = GetMethod(typesByName, hook.TypeName, hook.Signature);
                                if (methodDef == null) continue;
                                hooks.Add(new DocsHook(hook, methodDef, decompiler, project.TargetDirectory));
                            }
                            catch (NotSupportedException) { }
                            catch (DecompilerException ex)
                            {
                                Console.WriteLine($"Failed to decompile method for hook {hook.Name}: {ex.Message}{(ex.InnerException != null ? " | " + ex.InnerException.Message : string.Empty)}");
                            }
                            catch (Exception e)
                            {
                                ReportHookError(hook, e);
                            }
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

        private static bool ShouldApplyPatch(Hook hook, Dictionary<Hook, Hook> cloneHooks)
        {
            if (hook.Flagged) return false;
            if (hook.BaseHook != null && hook.BaseHook.Flagged) return false;
            if (!cloneHooks.TryGetValue(hook, out Hook cloneHook)) return true;
            return cloneHook.Flagged;
        }

        private static void ReportHookError(Hook hook, Exception e)
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

        private static MethodDefinition GetMethod(Dictionary<string, TypeDefinition> typesByName, string typeName, MethodSignature signature)
        {
            if (!typesByName.TryGetValue(typeName, out TypeDefinition type)) return null;
            try
            {
                return type.Methods.Single(m => MethodSignatureMatches(Utility.GetMethodSignature(m), signature));
            }
            catch
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
