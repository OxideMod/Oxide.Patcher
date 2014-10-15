using System;
using System.IO;

using Mono.Cecil;

namespace OxidePatcher.Patching
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
        private DefaultAssemblyResolver fallback;

        /// <summary>
        /// Initialises a new instance of the AssemblyResolver class
        /// </summary>
        public AssemblyResolver()
        {
            fallback = new DefaultAssemblyResolver();
        }

        private static readonly string[] commasplit = new string[] { ", " };
        private AssemblyDefinition LocalResolve(string fullName)
        {
            string[] data = fullName.Split(commasplit, StringSplitOptions.RemoveEmptyEntries);
            string filename = Path.Combine(TargetDirectory, data[0] + ".dll");
            if (File.Exists(filename))
                return AssemblyDefinition.ReadAssembly(filename);
            else
                return null;
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            AssemblyDefinition result = LocalResolve(fullName);
            if (result == null) result = fallback.Resolve(fullName, parameters);
            return result;
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            AssemblyDefinition result = LocalResolve(fullName);
            if (result == null) result = fallback.Resolve(fullName);
            return result;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            AssemblyDefinition result = LocalResolve(name.FullName);
            if (result == null) result = fallback.Resolve(name, parameters);
            return result;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            AssemblyDefinition result = LocalResolve(name.FullName);
            if (result == null) result = fallback.Resolve(name);
            return result;
        }
    }
}