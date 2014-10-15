using System;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace OxidePatcher
{
    /// <summary>
    /// Contains code decompiling utility methods
    /// </summary>
    public static class Decompiler
    {
        /// <summary>
        /// Decompiles the specified method body to MSIL
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static string DecompileToIL(MethodBody body)
        {
            StringBuilder sb = new StringBuilder();
            var instructions = body.Instructions;
            for (int i = 0; i < instructions.Count; i++)
            {
                var inst = instructions[i];
                sb.AppendLine(inst.ToString());
            }
            return sb.ToString();
        }
    }
}
