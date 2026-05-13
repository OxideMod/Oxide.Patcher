using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Oxide.Patcher.Common;
using Oxide.Patcher.Hooks;

using MetadataTokens = System.Reflection.Metadata.Ecma335.MetadataTokens;
using MethodDefinitionHandle = System.Reflection.Metadata.MethodDefinitionHandle;

namespace Oxide.Patcher.Docs
{
    public class DocsHook
    {
        public HookType Type { get; set; }
        public string Name { get; set; }
        public string HookName { get; set; }
        public string HookDescription { get; set; }
        public Dictionary<string, string> HookParameters { get; set; }
        public string ReturnTypeOverwrite { get; set; }
        public ReturnBehavior ReturnBehavior { get; set; } = ReturnBehavior.Continue;
        public string TargetType { get; set; }
        public string Category { get; set; }
        public DocsMethodData MethodData { get; set; }
        public string CodeAfterInjection { get; set; }

        private readonly string _targetDirectory;
        private readonly CSharpDecompiler _decompiler;
        private SyntaxTree _syntaxTree;

        public DocsHook(Hook hook, MethodDefinition methodDef, CSharpDecompiler decompiler, string targetDirectory)
        {
            _decompiler = decompiler;
            if (IsNeverCalledInPlugin(hook.HookName))
            {
                throw new NotSupportedException("This hook is never called in a plugin");
            }

            _targetDirectory = targetDirectory;

            switch (hook)
            {
                case Simple simpleHook:
                    Type = HookType.Simple;
                    ReturnBehavior = simpleHook.ReturnBehavior;
                    HookParameters = GetHookArguments(simpleHook, methodDef);
                    ReturnTypeOverwrite = GetReturnType(simpleHook, methodDef);
                    break;

                case Modify modifyHook:
                    Type = HookType.Modify;
                    break;

                default:
                    throw new NotSupportedException("This hook type is not supported");
            }

            Name = hook.Name;
            HookName = hook.HookName;
            HookDescription = hook.HookDescription;
            string targetType = hook.TypeName;
            int backtickIndex = targetType.IndexOf('`');
            TargetType = backtickIndex > 0 ? targetType.Substring(0, backtickIndex) : targetType;
            Category = hook.HookCategory;

            MethodData = new DocsMethodData(methodDef);

            string methodSourceCode = _decompiler.DecompileAsString(MetadataTokens.EntityHandle(methodDef.MetadataToken.ToInt32()));
            methodSourceCode = Regex.Replace(methodSourceCode, @"^(?:\s*using\s+[\w\.]+;\s*)+", string.Empty);

            string[] lines = Regex.Split(methodSourceCode, "\r\n|\r|\n");

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (!line.Contains($"Interface.CallHook(\"{hook.HookName}\"") && !line.Contains($"Interface.CallDeprecatedHook(\"{hook.HookName}\""))
                {
                    continue;
                }

                int startIndex = i - 5 < 0 ? 0 : i - 5;
                int endIndex = i + 6;

                StringBuilder sb = new StringBuilder();

                if (startIndex > 0)
                {
                    sb.AppendLine("//---");
                }

                for (int x = startIndex; x < endIndex && x < lines.Length; x++)
                {
                    string current = lines[x];
                    string trimmed = current.TrimStart();
                    if (trimmed.StartsWith("[") && current.TrimEnd().EndsWith("]"))
                    {
                        continue;
                    }

                    sb.AppendLine(current);
                }

                if (endIndex < lines.Length - 1)
                {
                    sb.AppendLine("//---");
                }

                CodeAfterInjection = sb.ToString();

                break;
            }
        }

        private Dictionary<string, string> GetHookArguments(Simple hook, MethodDefinition method)
        {
            Dictionary<string, string> hookArguments = new Dictionary<string, string>();

            switch (hook?.ArgumentBehavior)
            {
                case ArgumentBehavior.All:
                {
                    AddThisArg(hook, hookArguments);
                    AddMethodArgs(method, hookArguments);

                    break;
                }

                case ArgumentBehavior.JustThis:
                {
                    AddThisArg(hook, hookArguments);
                    break;
                }

                case ArgumentBehavior.JustParams:
                {
                    AddMethodArgs(method, hookArguments);
                    break;
                }

                case ArgumentBehavior.UseArgumentString:
                {
                    string[] args = Utility.ParseArgumentString(hook.ArgumentString, out string returnValue);

                    foreach (string argument in args)
                    {
                        string typeName = GetArgStringType(argument, method, out string argName);

                        //TODO: think of a better way to handle if there are two args that have the same name
                        if (hookArguments.ContainsKey(argName))
                        {
                            string newArgName = argName;

                            int index = 2;
                            while (hookArguments.ContainsKey(newArgName))
                            {
                                newArgName = $"{argName}{index}";
                                index++;
                            }

                            hookArguments.Add(newArgName, typeName);

                            continue;
                        }

                        hookArguments.Add(argName, typeName);
                    }

                    break;
                }
            }

            return hookArguments;
        }

        private string GetReturnType(Simple hook, MethodDefinition method)
        {
            switch (hook?.ReturnBehavior)
            {
                case ReturnBehavior.Continue:
                    return null;

                case ReturnBehavior.ExitWhenNonNull:
                case ReturnBehavior.ExitWhenValidType:
                case ReturnBehavior.ModifyRefArg:
                    return hook?.Signature.ReturnType == "System.Void" ? null : Utility.TransformType(hook?.Signature.ReturnType);

                case ReturnBehavior.UseArgumentString:
                    Utility.ParseArgumentString(hook.ArgumentString, out string returnValue);
                    return GetArgStringType(returnValue, method, out string _);
            }

            return null;
        }

        //Doesn't work if I use the Decompiler class so just do this for now
        // private static string GetSourceCode(MethodDefinition methodDefinition)
        // {
        //     DecompilerSettings settings = new DecompilerSettings { UsingDeclarations = false };
        //     DecompilerContext context = new DecompilerContext(methodDefinition.Module)
        //     {
        //         CurrentType = methodDefinition.DeclaringType,
        //         Settings = settings
        //     };
        //
        //     AstBuilder astBuilder = new AstBuilder(context);
        //     astBuilder.AddMethod(methodDefinition);
        //     PlainTextOutput textOutput = new PlainTextOutput();
        //     astBuilder.GenerateCode(textOutput);
        //     return textOutput.ToString();
        // }

        #region -Arg Helpers-

        private string GetArgStringType(string arg, MethodDefinition method, out string argName)
        {
            string firstArg = arg.ToLowerInvariant();

            string[] target = null;
            if (!string.IsNullOrEmpty(firstArg) && arg.Contains("."))
            {
                string[] split = arg.Split('.');
                firstArg = split[0];
                target = split.Skip(1).ToArray();
            }

            if ((firstArg.StartsWith("l") || firstArg.StartsWith("v")) && int.TryParse(firstArg.Substring(1), out int index))
            {
                VariableDefinition variable = method.Body.Variables[index];
                TypeReference variableType = variable.VariableType;

                if (target != null && GetMember(method, variableType.Resolve(), target, out TypeReference finalTypeRef))
                {
                    argName = target[target.Length - 1];
                    return Utility.GetReadableTypeName(finalTypeRef);
                }

                argName = GetLocalVariableName(index, method);
                return variableType is ByReferenceType byRefType
                           ? Utility.GetReadableTypeName(byRefType.ElementType)
                           : Utility.GetReadableTypeName(variableType);
            }

            if ((firstArg.StartsWith("a") || firstArg.StartsWith("p")) && int.TryParse(firstArg.Substring(1), out index))
            {
                ParameterDefinition parameter = method.Parameters[index];
                TypeReference parameterType = parameter.ParameterType;

                if (target != null && GetMember(method, parameterType.Resolve(), target, out TypeReference finalTypeRef))
                {
                    argName = target[target.Length - 1];
                    return Utility.GetReadableTypeName(finalTypeRef);
                }

                argName = parameter.Name;
                return Utility.GetReadableTypeName(parameter.ParameterType);
            }

            if (firstArg.StartsWith("r") && int.TryParse(firstArg.Substring(1), out index) &&
                method.Body.Instructions[index - 1].Operand is MethodDefinition storedMethod)
            {
                TypeDefinition returnType = storedMethod.DeclaringType;
                string typeName = returnType.Name;

                char firstChar = char.ToLower(typeName[0]);
                argName = $"{firstChar}{typeName.Substring(1)}";
                return Utility.GetReadableTypeName(returnType);
            }

            if (firstArg == "this")
            {
                if (target != null && GetMember(method, method.DeclaringType, target, out TypeReference finalTypeRef))
                {
                    argName = target[target.Length - 1];
                    return Utility.GetReadableTypeName(finalTypeRef);
                }

                argName = "instance";
                return Utility.GetReadableTypeName(method.DeclaringType);
            }

            if (firstArg == "true" || firstArg == "false")
            {
                argName = firstArg;
                return "bool";
            }

            argName = "Unknown";
            return "Unknown";
        }

        private string GetLocalVariableName(int index, MethodDefinition method)
        {
            if (index < 0 || index >= method.Body.Variables.Count) return $"V_{index}";

            if (_syntaxTree == null)
            {
                _syntaxTree = _decompiler.Decompile((MethodDefinitionHandle)MetadataTokens.EntityHandle(method.MetadataToken.ToInt32()));
            }

            string ilTypeName = method.Body.Variables[index].VariableType.Name;
            VariableInitializer initializer = _syntaxTree?.Descendants.OfType<VariableInitializer>().ElementAtOrDefault(index);
            if (initializer?.Parent is VariableDeclarationStatement decl
                && !string.IsNullOrEmpty(initializer.Name)
                && Utility.TransformType(decl.Type.ToString()) == Utility.TransformType(ilTypeName))
            {
                return initializer.Name;
            }

            return string.IsNullOrEmpty(ilTypeName) ? $"V_{index}" : char.ToLower(ilTypeName[0]) + ilTypeName.Substring(1);
        }

        private bool GetMember(MethodDefinition originalMethod, TypeDefinition currentArg, string[] target, out TypeReference finalTypeRef)
        {
            finalTypeRef = null;
            if (currentArg == null || target == null || target.Length == 0)
            {
                return false;
            }

            int i;
            TypeDefinition arg = currentArg;
            TypeReference lastTypeRef = currentArg;
            for (i = 0; i < target.Length; i++)
            {
                if (GetMember(originalMethod, ref arg, target[i], out lastTypeRef))
                {
                    continue;
                }

                return false;
            }

            finalTypeRef = lastTypeRef;
            return i >= 1;
        }

        private bool GetMember(MethodDefinition originalMethod, ref TypeDefinition currentArg, string target, out TypeReference unresolvedTypeRef)
        {
            unresolvedTypeRef = null;
            if (currentArg == null || string.IsNullOrEmpty(target))
            {
                return false;
            }

            while (currentArg != null)
            {
                if (target.Contains('('))
                {
                    string[] methodName = target.Split('(');
                    string[] args = methodName[1].TrimEnd(')').Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    MethodDefinition method = currentArg.Methods.FirstOrDefault(m => m.Name == methodName[0] && m.Parameters.Count == args.Length);
                    if (method != null)
                    {
                        if (method.IsGenericInstance || method.HasGenericParameters || method.Parameters.Count > 0)
                        {
                            return false;
                        }

                        unresolvedTypeRef = method.ReturnType;
                        currentArg = method.ReturnType.Resolve();

                        return true;
                    }
                }

                if (currentArg.IsClass && currentArg.HasFields)
                {
                    foreach (FieldDefinition field in currentArg.Fields)
                    {
                        if (!string.Equals(field.Name, target, StringComparison.CurrentCultureIgnoreCase))
                        {
                            continue;
                        }

                        unresolvedTypeRef = field.FieldType;
                        currentArg = field.FieldType.Resolve();

                        return true;
                    }
                }

                if (currentArg.HasProperties)
                {
                    foreach (PropertyDefinition property in currentArg.Properties)
                    {
                        if (!string.Equals(property.Name, target, StringComparison.CurrentCultureIgnoreCase))
                        {
                            continue;
                        }

                        unresolvedTypeRef = property.PropertyType;
                        currentArg = property.PropertyType.Resolve();

                        return true;
                    }
                }

                if (currentArg.HasInterfaces)
                {
                    foreach (TypeReference interfaceType in currentArg.Interfaces)
                    {
                        TypeDefinition previous = currentArg;
                        currentArg = interfaceType.Resolve();

                        if (GetMember(originalMethod, ref currentArg, target, out unresolvedTypeRef))
                        {
                            return true;
                        }

                        currentArg = previous;
                    }
                }

                if (currentArg.BaseType != null && originalMethod.Module.Assembly != currentArg.BaseType.Module.Assembly)
                {
                    TypeReference baseType = currentArg.BaseType;
                    string scopeName = baseType.Scope.Name;

                    string baseTypePath = $"{_targetDirectory}\\{scopeName}{(scopeName.EndsWith(".dll") ? "" : ".dll")}";
                    AssemblyDefinition baseTypeAssembly = AssemblyDefinition.ReadAssembly(new MemoryStream(File.ReadAllBytes(baseTypePath)));

                    currentArg = baseTypeAssembly.MainModule.Types.Single(x => x.FullName == baseType.FullName);
                }
                else
                {
                    currentArg = currentArg.BaseType?.Resolve();
                }
            }

            return false;
        }

        #endregion

        private void AddThisArg(Hook hook, Dictionary<string, string> dict)
        {
            TypeDefinition type = DocsGenerator.AssemblyLoader.GetType(hook.AssemblyName, hook.TypeName);
            if (type == null)
            {
                return;
            }

            dict.Add("instance", Utility.GetReadableTypeName(type));
        }

        private void AddMethodArgs(MethodDefinition method, Dictionary<string, string> dict)
        {
            foreach (ParameterDefinition parameter in method.Parameters)
            {
                string parameterName = parameter.Name;

                if (parameterName == "instance" && dict.ContainsKey("instance"))
                {
                    string parameterTypeName = parameter.ParameterType.Name;
                    parameterName = $"{char.ToLower(parameterTypeName[0])}{parameterTypeName.Substring(1)}";
                }

                dict.Add(parameterName, Utility.GetReadableTypeName(parameter.ParameterType));
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
