using System;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Metadata;

using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Oxide.Patcher.Patching;

using MethodDefinition = Mono.Cecil.MethodDefinition;

namespace Oxide.Patcher.Common
{
    /// <summary>
    /// Contains code decompiling utility methods
    /// </summary>
    public static class Decompiler
    {
        private static readonly DecompilerSettings DecompilerSettings = new DecompilerSettings
        {
            UsingDeclarations = false
        };

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

        public static SyntaxTree GetSyntaxTree(MethodDefinition methodDefinition)
        {
            using (DecompilerWrapper decompiler = GetDecompiler(methodDefinition))
            {
                MethodDefinitionHandle handle = (MethodDefinitionHandle)MetadataTokens.EntityHandle(methodDefinition.MetadataToken.ToInt32());
                return decompiler.Decompile(handle);
            }
        }

        public static async Task<string> GetSourceCode(MethodDefinition methodDefinition, ILWeaver weaver = null)
        {
            try
            {
                return await Task.Run(() =>
                {
                    EntityHandle handle = MetadataTokens.EntityHandle(methodDefinition.MetadataToken.ToInt32());

                    using (DecompilerWrapper decompiler = GetDecompiler(methodDefinition, weaver))
                    {
                        return decompiler.DecompileAsString(handle);
                    }
                });
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
        }

        private static DecompilerWrapper GetDecompiler(MethodDefinition methodDefinition, ILWeaver weaver = null,
                                                                 bool writeToStream = false)
        {
            string targetDirectory = PatcherForm.MainForm?.CurrentProject.TargetDirectory ?? Program.PatchProject.TargetDirectory;

            MemoryStream assemblyStream = null;
            PEFile peFile;
            string path;

            if (weaver != null || writeToStream)
            {
                weaver?.Apply(methodDefinition.Body);

                assemblyStream = new MemoryStream();

                methodDefinition.Module.Assembly.Write(assemblyStream);
                assemblyStream.Position = 0;

                path = Path.Combine(targetDirectory, "temporary");
                peFile = new PEFile("temporary", assemblyStream);
            }
            else
            {
                path = Path.Combine(targetDirectory, $"{methodDefinition.Module.Assembly.Name.Name}.dll");
                peFile = new PEFile(path);
            }

            UniversalAssemblyResolver resolver = new UniversalAssemblyResolver(path, true, peFile.DetectTargetFrameworkId(),
                                                                               peFile.DetectRuntimePack());

            return new DecompilerWrapper(new CSharpDecompiler(peFile, resolver, DecompilerSettings), peFile, assemblyStream);
        }

        private readonly struct DecompilerWrapper : IDisposable
        {
            public CSharpDecompiler Decompiler { get; }
            public PEFile PeFile { get; }
            public MemoryStream AssemblyStream { get; }

            public DecompilerWrapper(CSharpDecompiler decompiler, PEFile peFile, MemoryStream assemblyStream = null)
            {
                Decompiler = decompiler;
                PeFile = peFile;
                AssemblyStream = assemblyStream;
            }

            public string DecompileAsString(EntityHandle handle) => Decompiler.DecompileAsString(handle);
            public SyntaxTree Decompile(EntityHandle handle) => Decompiler.Decompile(handle);

            public void Dispose()
            {
                PeFile.Dispose();
                AssemblyStream?.Dispose();
            }
        }
    }
}
