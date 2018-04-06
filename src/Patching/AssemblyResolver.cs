using Mono.Cecil;
using System;
using System.IO;

namespace Oxide.Patcher.Patching
{
    /// <summary>
    /// Allows Mono.Cecil to locate assemblies when trying to build references
    /// </summary>
    public class AssemblyResolver : IAssemblyResolver
    {
        /// <summary>
        /// Gets or sets the target directory
        /// </summary>
        public string TargetDirectory { get; set; }

        // The fallback resolver
        private readonly DefaultAssemblyResolver fallback;

        /// <summary>
        /// Initializes a new instance of the AssemblyResolver class
        /// </summary>
        public AssemblyResolver()
        {
            fallback = new DefaultAssemblyResolver();
        }

        private static readonly string[] commasplit = { ", " };
        private AssemblyDefinition LocalResolve(string fullName)
        {
            string[] data = fullName.Split(commasplit, StringSplitOptions.RemoveEmptyEntries);
            string filename = Path.Combine(TargetDirectory, data[0] + ".dll");
            if (!File.Exists(filename))
            {
                filename = Path.Combine(TargetDirectory, data[0] + ".exe");
            }

            return File.Exists(filename) ? AssemblyDefinition.ReadAssembly(filename) : null;
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            AssemblyDefinition result = LocalResolve(fullName) ?? fallback.Resolve(AssemblyNameReference.Parse(fullName), parameters);
            return result;
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            AssemblyDefinition result = LocalResolve(fullName) ?? fallback.Resolve(AssemblyNameReference.Parse(fullName));
            return result;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            AssemblyDefinition result = LocalResolve(name.FullName) ?? fallback.Resolve(name, parameters);
            return result;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            AssemblyDefinition result = LocalResolve(name.FullName) ?? fallback.Resolve(name);
            return result;
        }
    }
}
