using Mono.Cecil;
using System;
using System.IO;

namespace Oxide.Patcher.Patching
{
    /// <summary>
    /// Allows Mono.Cecil to locate assemblies when trying to build references
    /// </summary>
    public class PatcherAssemblyResolver : DefaultAssemblyResolver
    {

        /// <summary>
        /// Initializes a new instance of the AssemblyResolver class
        /// </summary>
        public PatcherAssemblyResolver(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException("Directory not found: " + path);
            AddSearchDirectory(path);
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference assemblyName)
        {
            return base.Resolve(assemblyName);
        }
    }
}
