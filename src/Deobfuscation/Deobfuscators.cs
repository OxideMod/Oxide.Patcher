using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Patcher.Deobfuscation
{
    /// <summary>
    /// Contains methods for finding a deobfuscator
    /// </summary>
    public static class Deobfuscators
    {
        private static Deobfuscator[] deobfuscators;

        static Deobfuscators()
        {
            List<Deobfuscator> list = new List<Deobfuscator>();
            Type deobfuscator = typeof(Deobfuscator);
            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => deobfuscator.IsAssignableFrom(t) && !t.IsAbstract))
            {
                list.Add(Activator.CreateInstance(type) as Deobfuscator);
            }
            list.Sort((a, b) => Comparer<int>.Default.Compare(a.Priority, b.Priority));
            deobfuscators = list.ToArray();
        }

        /// <summary>
        /// Finds a deobfuscator that can deobfuscate the specified assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static Deobfuscator Find(AssemblyDefinition assembly)
        {
            for (int i = 0; i < deobfuscators.Length; i++)
            {
                if (deobfuscators[i].CanDeobfuscate(assembly))
                {
                    return deobfuscators[i];
                }
            }
            return null;
        }
    }
}
