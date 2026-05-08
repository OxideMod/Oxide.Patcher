using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
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

        private static readonly Dictionary<string, (PEFile PeFile, CSharpDecompiler Decompiler)> DecompilerCache = new Dictionary<string, (PEFile, CSharpDecompiler)>();

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
                return "Error in creating source code from IL: " + ex;
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

            if (weaver != null || writeToStream)
            {
                weaver?.Apply(methodDefinition.Body);

                MemoryStream assemblyStream = new MemoryStream();
                methodDefinition.Module.Assembly.Write(assemblyStream);
                assemblyStream.Position = 0;

                string tempPath = Path.Combine(targetDirectory, "temporary");
                PEFile tempPeFile = new PEFile("temporary", assemblyStream);
                UniversalAssemblyResolver tempResolver = new UniversalAssemblyResolver(tempPath, true, tempPeFile.DetectTargetFrameworkId(), tempPeFile.DetectRuntimePack());

                return new DecompilerWrapper(new CSharpDecompiler(tempPeFile, tempResolver, DecompilerSettings), tempPeFile, assemblyStream, ownsPeFile: true);
            }
            else
            {
                string assemblyName = methodDefinition.Module.Assembly.Name.Name;
                string originalPath = Path.Combine(targetDirectory, $"{assemblyName}_Original.dll");
                string path = File.Exists(originalPath) ? originalPath : Path.Combine(targetDirectory, $"{assemblyName}.dll");

                if (!DecompilerCache.TryGetValue(path, out (PEFile PeFile, CSharpDecompiler Decompiler) cached))
                {
                    PEFile peFile;
                    using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    {
                        peFile = new PEFile(path, fs, PEStreamOptions.PrefetchEntireImage);
                    }
                    UniversalAssemblyResolver resolver = new UniversalAssemblyResolver(path, true, peFile.DetectTargetFrameworkId(), peFile.DetectRuntimePack());
                    cached = (peFile, new CSharpDecompiler(peFile, resolver, DecompilerSettings));
                    DecompilerCache[path] = cached;
                }

                return new DecompilerWrapper(cached.Decompiler, cached.PeFile, null, ownsPeFile: false);
            }
        }

        private readonly struct DecompilerWrapper : IDisposable
        {
            private readonly CSharpDecompiler _decompiler;
            private readonly PEFile _peFile;
            private readonly MemoryStream _assemblyStream;
            private readonly bool _ownsPeFile;

            public DecompilerWrapper(CSharpDecompiler decompiler, PEFile peFile, MemoryStream assemblyStream, bool ownsPeFile)
            {
                _decompiler = decompiler;
                _peFile = peFile;
                _assemblyStream = assemblyStream;
                _ownsPeFile = ownsPeFile;
            }

            public string DecompileAsString(EntityHandle handle) => _decompiler.DecompileAsString(handle);
            public SyntaxTree Decompile(EntityHandle handle) => _decompiler.Decompile(handle);

            public void Dispose()
            {
                if (_ownsPeFile)
                {
                    _peFile.Dispose();
                }
                _assemblyStream?.Dispose();
            }
        }
    }
}
