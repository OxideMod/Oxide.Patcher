using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Mono.Cecil;
using Newtonsoft.Json;

using Oxide.Patcher.Hooks;

namespace Oxide.Patcher.Docs
{
    public static class DocsGenerator
    {
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
            DocsData docsData = new DocsData();
            List<DocsHook> hooks = new List<DocsHook>();

            Dictionary<string, AssemblyDefinition> assemblies = new Patching.Patcher(project).PatchInMemory();

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
                        DocsHook docsHook = new DocsHook(hook, assembly);
                        hooks.Add(docsHook);
                    }
                    catch
                    {

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
        }
    }
}
