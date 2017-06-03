using System;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;

using Mono.Cecil;
using Mono.Cecil.Cil;

using OxidePatcher.Patching;

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
            if (body?.Instructions == null) return null;
            var instructions = body.Instructions;
            for (int i = 0; i < instructions.Count; i++)
            {
                var inst = instructions[i];
                sb.AppendLine(inst.ToString().Replace("\n", "\\n"));
            }
            return sb.ToString();
        }

        public static async Task<string> GetSourceCode(TypeDefinition typeDefinition)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var settings = new DecompilerSettings { UsingDeclarations = true };
                    var context = new DecompilerContext(typeDefinition.Module)
                    {
                        CurrentType = typeDefinition,
                        Settings = settings
                    };
                    var astBuilder = new AstBuilder(context);
                    var textOutput = new PlainTextOutput();
                    astBuilder.GenerateCode(textOutput);
                    return textOutput.ToString();
                }
                catch (Exception ex)
                {
                    return "Error in creating source code from Type: " + ex.Message + ex.Message + Environment.NewLine + ex.StackTrace;
                }
            });
        }

        public static async Task<string> GetSourceCode(MethodDefinition methodDefinition, ILWeaver weaver = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (weaver != null) weaver.Apply(methodDefinition.Body);
                    var settings = new DecompilerSettings { UsingDeclarations = false };
                    var context = new DecompilerContext(methodDefinition.Module)
                    {
                        CurrentType = methodDefinition.DeclaringType,
                        Settings = settings
                    };
                    var astBuilder = new AstBuilder(context);
                    astBuilder.AddMethod(methodDefinition);
                    var textOutput = new PlainTextOutput();
                    astBuilder.GenerateCode(textOutput);
                    return textOutput.ToString();
                }
                catch (Exception ex)
                {
                    return "Error in creating source code from IL: " + ex.Message + Environment.NewLine + ex.StackTrace;
                }
                finally
                {
                    if (weaver != null) methodDefinition.Body = null;
                }
            });
        }
    }
}
