using Mono.Cecil;

namespace Oxide.Patcher.Deobfuscation
{
    /// <summary>
    /// Represents a deobfuscation method for a specific type of obfuscation
    /// </summary>
    public abstract class Deobfuscator
    {
        /// <summary>
        /// Gets the name of this deobfuscator
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the priority of this deobfuscator
        /// </summary>
        public abstract int Priority { get; }

        /// <summary>
        /// Returns if this deobfuscator is capable of deobfuscating the specified assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public abstract bool CanDeobfuscate(AssemblyDefinition assembly);

        /// <summary>
        /// Deobfuscates the specified assembly, returning a success value
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public abstract bool Deobfuscate(AssemblyDefinition assembly);

    }
}
