using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;

namespace Oxide.Patcher.Patching
{
    /// <summary>
    /// Allows Mono.Cecil to locate assemblies when trying to build references
    /// </summary>
    public class PatcherAssemblyResolver : DefaultAssemblyResolver
    {
        private readonly Dictionary<string, AssemblyDefinition> _memoryCache = new Dictionary<string, AssemblyDefinition>();

        /// <summary>
        /// Initializes a new instance of the AssemblyResolver class
        /// </summary>
        public PatcherAssemblyResolver(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException("Directory not found: " + path);
            AddSearchDirectory(path);
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            if (_memoryCache.TryGetValue(name.FullName, out AssemblyDefinition cached))
            {
                return cached;
            }

            foreach (string dir in GetSearchDirectories())
            {
                foreach (string ext in new[] { ".dll", ".exe" })
                {
                    string path = Path.Combine(dir, name.Name + ext);
                    if (!File.Exists(path)) continue;
                    try
                    {
                        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(new MemoryStream(File.ReadAllBytes(path)), new ReaderParameters { AssemblyResolver = this });
                        _memoryCache[name.FullName] = assembly;
                        return assembly;
                    }
                    catch (BadImageFormatException) { }
                }
            }

            return base.Resolve(name);
        }
    }
}
