using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Oxide.Patcher.Common;
using Oxide.Patcher.Hooks;

using ILVariable = ICSharpCode.Decompiler.IL.ILVariable;
using MetadataTokens = System.Reflection.Metadata.Ecma335.MetadataTokens;
using MethodDefinitionHandle = System.Reflection.Metadata.MethodDefinitionHandle;
using VariableKind = ICSharpCode.Decompiler.IL.VariableKind;

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

                    // Auto generate hook argument from instructions list or use ArgumentOverride string to generate Docs. 
                    if (string.IsNullOrEmpty(modifyHook.ArgumentOverride))
                    {
                        // Analyse modify instructions to find arguments and return type
                        (ReturnTypeOverwrite,HookParameters) = GetHookArguments(modifyHook, methodDef);
                        if (ReturnTypeOverwrite != null) ReturnBehavior = ReturnBehavior.UseArgumentString;
                    }
                    else
                    {
                        // use override string for argument and return type
                        (ReturnTypeOverwrite, HookParameters) = GetHookArguments_override(modifyHook, methodDef);
                        if (ReturnTypeOverwrite != null) ReturnBehavior = ReturnBehavior.UseArgumentString;
                    }
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

        private (string, Dictionary<string, string>) GetHookArguments(Modify hook, MethodDefinition method)
        {
            Dictionary<string, string> hookArguments = new Dictionary<string, string>();
            bool foundHook = false;
            ArgSM procState = ArgSM.StartState;
            ReturnTypeOverwrite = null;

            foreach (var instr in hook.Instructions)
            {
                // Interpret IL between ldstr hookname  and the call callhook instruction
                var split = instr.OpCode.ToLowerInvariant().Split('_');
                if (procState== ArgSM.StartState && split[0].StartsWith("ldstr") && instr.Operand.Equals(hook.HookName))
                {
                    hookArguments.Clear();
                    procState = ArgSM.GetArgs;
                }
                else if (procState == ArgSM.GetArgs &&  split[0].StartsWith("ldstr"))
                {
                    hookArguments.Add("str", Utility.TransformType("string"));
                }
                else if (procState == ArgSM.GetArgs && split[0].StartsWith("ldc"))
                {
					// Its a constant, value is not important for argument list. use 'int condition'
                    //int cte = ((split.Count() == 3 && split[1].StartsWith("i4") && split[2] == "s") ? Convert.ToInt32(instr.Operand) : Convert.ToInt32(split[2]));
                    //hookArguments.Add(cte.ToString(), "int32"); // Code use constant, but form plugin POV, its a variable
                    hookArguments.Add("condition", Utility.TransformType("int"));
                }
                else if (procState == ArgSM.GetArgs && split[0].StartsWith("ldarg"))
                {
                    string operand;
                    int index = ((split.Count() == 1 || split[1] == "s") ? Convert.ToInt32(instr.Operand) : Convert.ToInt32(split[1]));
                    if (index == 0) operand = "this";
                    else operand = "a" + (index - 1).ToString();
                    AddHookArguments(operand, method, hookArguments);
                }
                else if (procState == ArgSM.GetArgs && split[0].StartsWith("ldloc"))
                {
                    string operand;
                    int index = ((split.Count() == 1 || split[1] == "s") ? Convert.ToInt32(instr.Operand) : Convert.ToInt32(split[1]));
                    operand = "l" + index.ToString();
                    AddHookArguments(operand, method, hookArguments);
                }
                else if (procState == ArgSM.GetArgs && split[0].StartsWith("ldfld"))
                {
                    if (instr.Operand is string str)
                    {
                        var opSplit = str.Split('|');
                        var opCount = opSplit.Count();
                        if (hookArguments.Count == 0 || opCount != 3) continue;

                        (var fieldName, var fieldType) = ResolveFieldType(opSplit[0], opSplit[1], opSplit[2]);
                        var type = Utility.TransformType(fieldType);
                        var key = hookArguments.Last().Key;
                        hookArguments.Remove(key);     // pop last and update arg
                        hookArguments.Add(fieldName, string.IsNullOrEmpty(type) ? "object" : type);
                    }
                }
                else if (procState == ArgSM.GetArgs && instr.OpCode.StartsWith("box"))
                {
                    if (instr.Operand is string str)
                    {
                        var opSplit = str.Split('|');
                        if (hookArguments.Count == 0) continue;
                        var key = hookArguments.Last().Key;
                        hookArguments[key] = Utility.TransformType(opSplit.Last());
                    }
                }
                else if (procState == ArgSM.GetArgs && instr.OpCode.StartsWith("callvirt"))
                {
                    if (instr.Operand is string str)
                    {
                        // No modify hooks use callvirt at this time
                        //Console.WriteLine($"hook: {hook.Name} use callvirt. Use Argument override string for correct info in doc ");
                    }
                }
                else if (procState == ArgSM.GetArgs && instr.OpCode.StartsWith("call"))
                {
                    if (instr.Operand is string str)
                    {
                        if (str.Contains("CallHook"))
                        {
                            foundHook = true;
                            procState = ArgSM.GetRetType;
                        }
                        else
                        {
                            // for now, support simple one param call for type convertion of data
                            // a call for data conversion should be followed by a box op to get the type.
                            var opSplit = str.Split('|');
                            string operand = string.Join(".", opSplit.Skip(1));
                            if (hookArguments.Count == 0) continue;
                            var key = hookArguments.Last().Key;
                            hookArguments.Remove(key);     // pop last
                            hookArguments.Add(key, Utility.TransformType(operand));
                            //Console.WriteLine($"hook: {hook.Name} use call. Check generated documentation ");
                        }
                    }
                }
                else if (procState== ArgSM.GetRetType && (instr.OpCode.StartsWith("brtrue") || instr.OpCode.StartsWith("brfalse") || instr.OpCode.StartsWith("ldnull")))
                {
                    ReturnTypeOverwrite = "object";
                    procState = ArgSM.Idle;
                    break;
                }
                else if (procState == ArgSM.GetRetType && (instr.OpCode.StartsWith("pop"))) // Callhook return value ignored
                {
                    ReturnTypeOverwrite = null;
                    procState = ArgSM.Idle;
                    break;
                }
            }
            return (ReturnTypeOverwrite, (foundHook ? hookArguments : null));
        }

        void AddHookArguments(string argument, MethodDefinition method, Dictionary<string, string> hookArguments)
        {
            if (string.IsNullOrEmpty(argument)) return;

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
            }
            else hookArguments.Add(argName, typeName);
        }

        private (string, Dictionary<string, string>) GetHookArguments_override(Modify hook, MethodDefinition method)
        {
            Dictionary<string, string> hookArguments = new Dictionary<string, string>();
            ReturnTypeOverwrite = null;
            if (string.IsNullOrEmpty(hook.ArgumentOverride)) return (null, null);
            string[] line = hook.ArgumentOverride.Split(':'); // split arg and return type
            if (!string.IsNullOrEmpty(line[0]))   // no arg to analyse
            {
                string[] arguments = line[0].Split(',');
                int index = 1;
                string arg;
                string type;
                foreach (string argument in arguments)
                {
                    string[] typearg = argument.Trim().Split(' ');
                    if (typearg.Length == 0) break;
                    if (typearg.Length == 1) arg = $"oxide_{index++}";
                    else arg = typearg[1];
                    type = typearg[0];
                    if (hookArguments.ContainsKey(arg)) arg = $"oxide_{index++}"; // check dup key typo
                    hookArguments.Add(arg, type);
                }
            }

            if (line.Length == 2 && !string.IsNullOrEmpty(line[1])) // use return type if defined
            {
                ReturnTypeOverwrite = line[1].Trim();
            }
            return (ReturnTypeOverwrite, hookArguments);
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
                    argName = GetFriendlyMemberName(target[target.Length - 1]);
                    return Utility.GetReadableTypeName(finalTypeRef);
                }

                argName = GetLocalVariableName(index, method);
                return variableType is Mono.Cecil.ByReferenceType byRefType
                           ? Utility.GetReadableTypeName(byRefType.ElementType)
                           : Utility.GetReadableTypeName(variableType);
            }

            if ((firstArg.StartsWith("a") || firstArg.StartsWith("p")) && int.TryParse(firstArg.Substring(1), out index))
            {
                ParameterDefinition parameter = method.Parameters[index];
                TypeReference parameterType = parameter.ParameterType;

                if (target != null && GetMember(method, parameterType.Resolve(), target, out TypeReference finalTypeRef))
                {
                    argName = GetFriendlyMemberName(target[target.Length - 1]);
                    return Utility.GetReadableTypeName(finalTypeRef);
                }

                argName = parameter.Name;
                return Utility.GetReadableTypeName(parameter.ParameterType);
            }

            if (firstArg.StartsWith("r") && int.TryParse(firstArg.Substring(1), out index) &&
                method.Body.Instructions[index - 1].Operand is MethodDefinition storedMethod)
            {
                TypeDefinition returnType = storedMethod.DeclaringType;
                argName = GetArgNameFromTypeName(returnType.Name);
                return Utility.GetReadableTypeName(returnType);
            }

            if (firstArg == "this")
            {
                if (target != null && GetMember(method, method.DeclaringType, target, out TypeReference finalTypeRef))
                {
                    argName = GetFriendlyMemberName(target[target.Length - 1]);
                    return Utility.GetReadableTypeName(finalTypeRef);
                }

                argName = "instance";
                return Utility.GetReadableTypeName(method.DeclaringType);
            }

            if (firstArg == "true" || firstArg == "false")
            {
                argName = "flag";
                return "bool";
            }

            argName = "Unknown";
            return "object";
        }

        private string GetLocalVariableName(int index, MethodDefinition method)
        {
            if (index < 0 || index >= method.Body.Variables.Count) return $"V_{index}";

            if (_syntaxTree == null)
            {
                _syntaxTree = _decompiler.Decompile((MethodDefinitionHandle)MetadataTokens.EntityHandle(method.MetadataToken.ToInt32()));
            }

            ILVariable ilVariable = _syntaxTree?.Descendants
                .Select(x => x.Annotation<ILVariableResolveResult>()?.Variable)
                .FirstOrDefault(x => x != null && x.Index == index
                    && (x.Kind == VariableKind.Local || x.Kind == VariableKind.PinnedLocal
                        || x.Kind == VariableKind.UsingLocal || x.Kind == VariableKind.ForeachLocal));

            if (!string.IsNullOrEmpty(ilVariable?.Name))
            {
                return ilVariable.Name;
            }

            string ilTypeName = method.Body.Variables[index].VariableType.Name;
            return string.IsNullOrEmpty(ilTypeName) ? $"V_{index}" : GetArgNameFromTypeName(ilTypeName);
        }

        private static string GetFriendlyMemberName(string memberName)
        {
            Match hoistedLocal = Regex.Match(memberName, @"^<(\w+)>");
            if (hoistedLocal.Success)
            {
                return hoistedLocal.Groups[1].Value;
            }

            return memberName.EndsWith("()")
                ? GetArgNameFromTypeName(Regex.Replace(memberName.Substring(0, memberName.Length - 2), "^Get(?=[A-Z])", string.Empty))
                : memberName;
        }

        private static string GetArgNameFromTypeName(string typeName)
        {
            int backtickIndex = typeName.IndexOf('`');
            if (backtickIndex > 0)
            {
                typeName = typeName.Substring(0, backtickIndex);
            }

            return char.ToLower(typeName[0]) + typeName.Substring(1);
        }

        // Probably should be in a different file like decompiler
        public (string,string) ResolveFieldType(string assemblyPath, string fullTypeName, string fieldName)
        {
            string fullpath = Path.Combine(_targetDirectory, $"{assemblyPath}.dll");
            string typename = fullTypeName.Replace("/", "+");

            var resolver = new UniversalAssemblyResolver(fullpath, false, null);
            var module = new PEFile(fullpath);
            var typeSystem = new DecompilerTypeSystem(module, resolver);

            ITypeDefinition typeDef = typeSystem.MainModule.Compilation
                .FindType(new FullTypeName(typename))
                .GetDefinition();

            if (typeDef != null)
            {
                var field = typeDef.GetFields().FirstOrDefault(f => f.Name == fieldName);                
                if (field != null)
                {
                    if (field.ReturnType is ITypeParameter typeParam)
                    {
                        //int index = typeParam.Index;
                        return (field.Name, typeParam.EffectiveBaseClass.Name);
                    }
                    else
                    {
                        return (field.Name, field.Type.ReflectionName);
                    }
                }
            }
            return ("unknown", "object");
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
                    parameterName = GetArgNameFromTypeName(parameter.ParameterType.Name);
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

        public enum ArgSM
        {
            StartState,
            GetArgs,
            GetRetType,
            Idle
        }
    }
}
