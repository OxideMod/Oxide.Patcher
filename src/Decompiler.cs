using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Oxide.Patcher.Patching;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Oxide.Patcher
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
            if (body?.Instructions == null)
            {
                return null;
            }

            Collection<Instruction> instructions = body.Instructions;
            for (int i = 0; i < instructions.Count; i++)
            {
                Instruction inst = instructions[i];
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
                    DecompilerSettings settings = new DecompilerSettings { UsingDeclarations = true };
                    DecompilerContext context = new DecompilerContext(typeDefinition.Module)
                    {
                        CurrentType = typeDefinition,
                        Settings = settings
                    };
                    AstBuilder astBuilder = new AstBuilder(context);
                    PlainTextOutput textOutput = new PlainTextOutput();
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
                    if (weaver != null)
                    {
                        weaver.Apply(methodDefinition.Body);
                    }

                    DecompilerSettings settings = new DecompilerSettings { UsingDeclarations = false };
                    DecompilerContext context = new DecompilerContext(methodDefinition.Module)
                    {
                        CurrentType = methodDefinition.DeclaringType,
                        Settings = settings
                    };
                    AstBuilder astBuilder = new AstBuilder(context);
                    astBuilder.AddMethod(methodDefinition);
                    PlainTextOutput textOutput = new PlainTextOutput();
                    astBuilder.GenerateCode(textOutput);
                    return textOutput.ToString();
                }
                catch (Exception ex)
                {
                    return "Error in creating source code from IL: " + ex.Message + Environment.NewLine + ex.StackTrace;
                }
                finally
                {
                    if (weaver != null)
                    {
                        methodDefinition.Body = null;
                    }
                }
            });
        }
    }
}
