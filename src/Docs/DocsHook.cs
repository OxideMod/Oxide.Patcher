using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;

using Mono.Cecil;

using Oxide.Patcher.Hooks;

namespace Oxide.Patcher.Docs
{
    public class DocsHook
    {
        private static readonly Dictionary<string, AssemblyDefinition> _assemblies = new Dictionary<string, AssemblyDefinition>();

        public HookType Type { get; set; }
        public string Name { get; set; }
        public string HookName { get; set; }
        public ReturnBehavior ReturnBehavior { get; set; } = ReturnBehavior.Continue;
        public string TargetType { get; set; }
        public string Category { get; set; }
        public DocsMethodData MethodData { get; set; }
        public string CodeAfterInjection { get; set; }

        public DocsHook(Hook hook, AssemblyDefinition assemblyDefinition)
        {
            if (IsNeverCalledInPlugin(hook.HookName))
            {
                throw new NotSupportedException("This hook is never called in a plugin");
            }

            switch (hook)
            {
                case Simple simpleHook:
                    Type = HookType.Simple;
                    ReturnBehavior = simpleHook.ReturnBehavior;
                    break;

                case Modify modifyHook:
                    Type = HookType.Modify;
                    break;

                default:
                    throw new NotSupportedException("This hook type is not supported");
            }

            Name = hook.Name;
            HookName = hook.HookName;
            TargetType = hook.TypeName;
            Category = hook.HookCategory;

            MethodDefinition methodDef = GetMethod(assemblyDefinition, hook.TypeName, hook.Signature);
            if (methodDef == null)
            {
                throw new Exception($"Failed to find method definition for hook {hook.Name}");
            }

            MethodData = new DocsMethodData(methodDef);

            if (hook.Flagged)
            {
                return;
            }

            string methodSourceCode = GetSourceCode(hook, methodDef);

            string[] lines = Regex.Split(methodSourceCode, "\r\n|\r|\n");

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (!line.Contains("Interface.CallHook"))
                {
                    continue;
                }

                int startIndex = i - 5 < 0 ? 0 : i - 5;
                int endIndex = i + 6;

                StringBuilder sb = new StringBuilder();

                if (startIndex > 0)
                {
                    sb.AppendLine("...");
                }

                for (int x = startIndex; x < endIndex && x < lines.Length; x++)
                {
                    sb.AppendLine(lines[x]);
                }

                if (endIndex < lines.Length - 1)
                {
                    sb.AppendLine("...");
                }

                CodeAfterInjection = sb.ToString();

                break;
            }
        }

        //Doesn't work if I use the Decompiler class so just do this for now
        private static string GetSourceCode(Hook hook, MethodDefinition methodDefinition)
        {
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

        private static MethodDefinition GetMethod(AssemblyDefinition assemblyDefinition, string typeName, MethodSignature signature)
        {
            try
            {
                TypeDefinition type = assemblyDefinition.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == typeName);
                return type.Methods.Single(m => Utility.GetMethodSignature(m).Equals(signature));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool IsNeverCalledInPlugin(string hook)
        {
            return hook == "InitLogging";
        }

        public enum HookType
        {
            Simple,
            Modify
        }
    }
}
