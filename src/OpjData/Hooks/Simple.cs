using ICSharpCode.Decompiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Oxide.Patcher.Patching;
using Oxide.Patcher.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using Oxide.Patcher.Common;
using AssemblyDefinition = Mono.Cecil.AssemblyDefinition;
using TypeDefinition = Mono.Cecil.TypeDefinition;

namespace Oxide.Patcher.Hooks
{
    public enum ReturnBehavior { Continue, ExitWhenValidType, ModifyRefArg, UseArgumentString, ExitWhenNonNull }

    public enum ArgumentBehavior { None, JustThis, JustParams, All, UseArgumentString }

    /// <summary>
    /// A simple hook that injects at a certain point in the method, with a few options for handling arguments and return values
    /// </summary>
    [HookType("Simple", Default = true)]
    public class Simple : Hook
    {
        public class DeprecatedStatus
        {
            public string ReplacementHook { get; set; }
            public DateTime RemovalDate { get; set; }
        }

        /// <summary>
        /// Gets or sets the instruction index to inject the hook call at
        /// </summary>
        public int InjectionIndex { get; set; }

        /// <summary>
        /// Gets or sets the return behavior
        /// </summary>
        public ReturnBehavior ReturnBehavior { get; set; }

        /// <summary>
        /// Gets or sets the argument behavior
        /// </summary>
        public ArgumentBehavior ArgumentBehavior { get; set; }

        /// <summary>
        /// Gets or sets the argument string
        /// </summary>
        public string ArgumentString { get; set; }

        public DeprecatedStatus Deprecation { get; set; }

        public override bool ApplyPatch(MethodDefinition original, ILWeaver weaver, Patching.Patcher patcher = null)
        {
            bool isDeprecated = Deprecation != null;
            string targetMethodName = isDeprecated ? "CallDeprecatedHook" : "CallHook";

            // Get the call hook method (only grab object parameters: ignore the object[] hook)
            List<MethodDefinition> callhookmethods = Program.OxideAssembly.MainModule.Types
                                                                .Single(t => t.FullName == "Oxide.Core.Interface").Methods
                                                                .Where(m => m.IsStatic && m.Name == targetMethodName && m.HasParameters && m.Parameters.Any(p => p.ParameterType.IsArray) == false)
                                                                .OrderBy(x => x.Parameters.Count)
                                                                .ToList();

            // Start injecting where requested
            weaver.Pointer = InjectionIndex;
            weaver.OriginalPointer = InjectionIndex;

            // Get the existing instruction we're going to inject behind
            Instruction existing;
            try
            {
                existing = weaver.Instructions[weaver.Pointer];
            }
            catch (ArgumentOutOfRangeException)
            {
                ShowMessage($"The injection index specified for {Name} is invalid!", "Invalid Index", patcher);
                return false;
            }

            // Introduce new locals from stack before injecting anything
            if (ArgumentBehavior == ArgumentBehavior.UseArgumentString)
                IntroduceLocals(original, weaver, patcher);

            VariableDefinition hookExpireDate = null;
            Instruction hookExpireDateAssignment = null;
            if (isDeprecated)
            {
                hookExpireDate = weaver.AddVariable(original.Module.Import(Deprecation.RemovalDate.GetType()), "hookExpireDate");
                hookExpireDateAssignment = weaver.Add(Instruction.Create(OpCodes.Ldloca_S, hookExpireDate));
                weaver.Add(Instruction.Create(OpCodes.Ldc_I4, Deprecation.RemovalDate.Year));
                weaver.Add(Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)Deprecation.RemovalDate.Month));
                weaver.Add(Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)Deprecation.RemovalDate.Day));
                weaver.Add(Instruction.Create(OpCodes.Call, original.Module.Import(typeof(DateTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) }))));
            }

            // Load the hook name
            Instruction hookname = weaver.Add(Instruction.Create(OpCodes.Ldstr, HookName));
            if (isDeprecated)
            {
                weaver.Add(Instruction.Create(OpCodes.Ldstr, Deprecation.ReplacementHook));
                weaver.Add(Instruction.Create(OpCodes.Ldloc, hookExpireDate));
            }

            // Create an object array and load all arguments into it
            Instruction firstinjected = hookExpireDateAssignment ?? hookname;
            PushArgsArray(original, weaver, out int argCount, patcher);

            // Call the CallHook or CallDeprecatedHook method with the correct amount of arguments
            weaver.Add(Instruction.Create(OpCodes.Call, original.Module.Import(callhookmethods[argCount])));

            // Deal with the return value
            DealWithReturnValue(original, null, weaver);

            // Find all instructions which pointed to the existing and redirect them
            for (int i = 0; i < weaver.Instructions.Count; i++)
            {
                Instruction ins = weaver.Instructions[i];
                if (ins.Operand != null && ins.Operand.Equals(existing))
                {
                    // Check if the instruction lies within our injection range
                    // If it does, it's an instruction we just injected so we don't want to edit it
                    if (i < InjectionIndex || i > weaver.Pointer)
                    {
                        ins.Operand = firstinjected;
                    }
                }
            }

            return true;
        }

        private void IntroduceLocals(MethodDefinition method, ILWeaver weaver, Patching.Patcher patcher)
        {
            var s = Utility.ParseArgumentString(ArgumentString, out _);
            if (s == null)
                return;
            for (var i = 0; i < s.Length; i++)
            {
                var arg = s[i].ToLowerInvariant();
                if (string.IsNullOrEmpty(arg) || arg[0] != 'r')
                    continue;
                if (arg.Contains("."))
                    arg = arg.Split('.')[0];
                if (!int.TryParse(arg.Substring(1), out var index))
                    continue;
                if (index > 0)
                    index--; // A hidden trick. Because IL listing starts from 1.
                if (index < 0 || index >= method.Body.Instructions.Count)
                {
                    ShowMessage($"Invalid callsite index {arg} supplied for {HookName}", "Invalid callsite supplied",
                        patcher);
                    continue;
                }
                var ins = weaver.Instructions[index]; //method.Body.Instructions[index];
                var opVarType = ins.Operand is MethodDefinition mDef ? mDef.ReturnType :
                    ins.Operand is MethodReference mRef ? mRef.ReturnType : null;
                if (opVarType == null)
                {
                    ShowMessage($"Invalid callsite index {arg} supplied for {HookName}\nMethod call not found.",
                        "Invalid callsite supplied", patcher);
                    continue;
                }
                if (opVarType.IsVoid())
                {
                    ShowMessage($"Invalid callsite index {arg} supplied for {HookName}\nReturn type cannot be void.",
                        "Invalid callsite supplied", patcher);
                    continue;
                }
                if (opVarType is GenericParameter gp &&
                    ((MethodReference)ins.Operand).DeclaringType is GenericInstanceType git)
                    opVarType = git.GenericArguments[gp.Position];

                var varDef = weaver.IntroducedLocals.ContainsKey(index)
                    ? weaver.Variables[weaver.IntroducedLocals[index]]
                    : weaver.AddVariable(opVarType);
                var nextIndex = index + (weaver.Pointer - weaver.OriginalPointer);
                weaver.IntroducedLocals[index] = weaver.Variables.Count - 1;
                weaver.AddAfter(nextIndex++, Instruction.Create(OpCodes.Stloc_S, varDef));
                weaver.AddAfter(nextIndex, Instruction.Create(OpCodes.Ldloc_S, varDef));
            }
        }

        private Instruction PushArgsArray(MethodDefinition method, ILWeaver weaver, /*out VariableDefinition argsvar*/ out int argCount, Patching.Patcher patcher)
        {
            argCount = 0;
            // Are we going to use arguments?
            if (ArgumentBehavior == ArgumentBehavior.None)
            {
                return null;
            }

            // Create array variable
            Instruction firstInstruction = null;
            // Are we using the argument string?
            if (ArgumentBehavior == ArgumentBehavior.UseArgumentString)
            {
                string[] args = Utility.ParseArgumentString(ArgumentString, out string retvalue);
                if (args == null)
                {
                    return null;
                }

                // Populate it
                for (int i = 0; i < args.Length; i++)
                {
                    argCount++;
                    string arg = args[i].ToLowerInvariant();
                    string[] target = null;
                    if (!string.IsNullOrEmpty(arg) && args[i].Contains("."))
                    {
                        string[] split = args[i].Split('.');
                        arg = split[0];
                        target = split.Skip(1).ToArray();
                    }

                    if (string.IsNullOrEmpty(arg))
                    {
                        weaver.Add(Instruction.Create(OpCodes.Ldnull));
                    }
                    else if (arg == "this")
                    {
                        if (method.IsStatic)
                        {
                            weaver.Add(Instruction.Create(OpCodes.Ldnull));
                        }
                        else
                        {
                            weaver.Add(ILWeaver.Ldarg(null));
                        }

                        GetMember(weaver, method, method.DeclaringType.Resolve(), target, patcher);
                    }
                    else if (arg[0] == 'p' || arg[0] == 'a')
                    {
                        if (int.TryParse(arg.Substring(1), out int index))
                        {
                            ParameterDefinition pdef;

                            if (index < method.Parameters.Count)
                            {
                                pdef = method.Parameters[index];

                                weaver.Add(ILWeaver.Ldarg(pdef));

                                if (!GetMember(weaver, method, pdef.ParameterType.Resolve(), target, patcher))
                                {
                                    var typeRef = pdef.ParameterType is ByReferenceType byRefType
                                        ? byRefType.ElementType
                                        : pdef.ParameterType;
                                    if (pdef.ParameterType.IsByReference)
                                        weaver.Add(Instruction.Create(OpCodes.Ldobj, typeRef));
                                    if (pdef.ParameterType.IsValueType)
                                        weaver.Add(Instruction.Create(OpCodes.Box, typeRef));
                                }
                            }
                            else
                            {
                                ShowMessage($"Invalid argument `{arg}` supplied for {HookName}", "Invalid argument supplied", patcher);
                            }
                        }
                        else
                        {
                            weaver.Add(Instruction.Create(OpCodes.Ldnull));
                        }
                    }
                    else if (arg[0] == 'l' || arg[0] == 'v')
                    {
                        if (int.TryParse(arg.Substring(1), out int index))
                        {
                            if (index < weaver.Variables.Count)
                            {
                                VariableDefinition vdef = weaver.Variables[index];

                                weaver.Ldloc(vdef);

                                if (!GetMember(weaver, method, vdef.VariableType.Resolve(), target, patcher))
                                {
                                    var typeRef = vdef.VariableType is ByReferenceType byRefType
                                        ? byRefType.ElementType
                                        : vdef.VariableType;
                                    if (vdef.VariableType.IsByReference)
                                        weaver.Add(Instruction.Create(OpCodes.Ldobj, typeRef));
                                    if (vdef.VariableType.IsValueType)
                                        weaver.Add(Instruction.Create(OpCodes.Box, typeRef));
                                }
                            }
                            else
                            {
                                ShowMessage($"Invalid variable `{arg}` supplied for {HookName}", "Invalid variable supplied", patcher);
                            }
                        }
                        else
                        {
                            weaver.Add(Instruction.Create(OpCodes.Ldnull));
                        }
                    }
                    else if (arg[0] == 'r')
                    {

                        if (int.TryParse(arg.Substring(1), out int index)
                            && weaver.IntroducedLocals.ContainsKey(--index)) // A hidden trick. Because IL listing starts from 1.
                        {
                            VariableDefinition vdef = weaver.Variables[weaver.IntroducedLocals[index]];

                            weaver.Ldloc(vdef);

                            if (!GetMember(weaver, method, vdef.VariableType.Resolve(), target, patcher))
                            {
                                var typeRef = vdef.VariableType is ByReferenceType byRefType
                                    ? byRefType.ElementType
                                    : vdef.VariableType;
                                if (vdef.VariableType.IsByReference)
                                    weaver.Add(Instruction.Create(OpCodes.Ldobj, typeRef));
                                if (vdef.VariableType.IsValueType)
                                    weaver.Add(Instruction.Create(OpCodes.Box, typeRef));
                            }
                        }
                        else
                        {
                            weaver.Add(Instruction.Create(OpCodes.Ldnull));
                        }
                    }
                    else
                    {
                        weaver.Add(Instruction.Create(OpCodes.Ldnull));
                    }
                }
            }
            else
            {
                // Figure out what we're doing
                bool includeargs = ArgumentBehavior == ArgumentBehavior.All || ArgumentBehavior == ArgumentBehavior.JustParams;
                bool includethis = ArgumentBehavior == ArgumentBehavior.All || ArgumentBehavior == ArgumentBehavior.JustThis;
                if (method.IsStatic)
                {
                    includethis = false;
                }

                // Work out what arguments we're going to transmit
                List<ParameterDefinition> args = new List<ParameterDefinition>();
                if (includeargs)
                {
                    for (int i = 0; i < method.Parameters.Count; i++)
                    {
                        ParameterDefinition arg = method.Parameters[i];
                        if (!arg.IsOut)
                        {
                            args.Add(arg);
                        }
                    }
                }

                // Include this
                if (includethis)
                {
                    argCount++;
                    weaver.Add(ILWeaver.Ldarg(null));
                }

                // Loop each argument
                for (int i = 0; i < args.Count; i++)
                {
                    argCount++;
                    // Load array, load index load arg, store in array
                    ParameterDefinition arg = args[i];

                    weaver.Add(ILWeaver.Ldarg(args[i]));
                    if (arg.ParameterType.IsByReference)
                    {
                        weaver.Add(Instruction.Create(OpCodes.Ldobj, arg.ParameterType));
                        weaver.Add(Instruction.Create(OpCodes.Box, arg.ParameterType));
                    }
                    else if (arg.ParameterType.IsValueType)
                    {
                        weaver.Add(Instruction.Create(OpCodes.Box, arg.ParameterType));
                    }
                }
            }
            return firstInstruction;
        }

        private void DealWithReturnValue(MethodDefinition method, VariableDefinition argsvar, ILWeaver weaver)
        {
            // What return behavior do we use?
            switch (ReturnBehavior)
            {
                case ReturnBehavior.Continue:
                    // Just discard the return value
                    weaver.Add(Instruction.Create(OpCodes.Pop));
                    break;

                case ReturnBehavior.ExitWhenNonNull:
                case ReturnBehavior.ExitWhenValidType:
                    // Is there a return value or not?
                    if (method.ReturnType.FullName == "System.Void")
                    {
                        // If the hook returned something that was non-null, return
                        Instruction i = weaver.Add(Instruction.Create(OpCodes.Ldnull));
                        weaver.Add(Instruction.Create(OpCodes.Beq_S, i.Next));
                        weaver.Add(Instruction.Create(OpCodes.Ret));
                    }
                    else if (ReturnBehavior == ReturnBehavior.ExitWhenNonNull && method.ReturnType == method.Module.TypeSystem.Boolean)
                    {
                        // Create variable
                        VariableDefinition returnvar = weaver.AddVariable(method.Module.TypeSystem.Object, "returnvar");

                        // Store the return value in it
                        weaver.Stloc(returnvar);
                        Instruction i = weaver.Ldloc(returnvar);

                        // Continue if null
                        weaver.Add(Instruction.Create(OpCodes.Brfalse_S, i.Next));

                        // If it's boolean, return it - else return false
                        weaver.Ldloc(returnvar);
                        weaver.Add(Instruction.Create(OpCodes.Isinst, method.Module.TypeSystem.Boolean));
                        Instruction i2 = Instruction.Create(OpCodes.Ldloc, returnvar);
                        weaver.Add(Instruction.Create(OpCodes.Brtrue_S, i2));
                        weaver.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                        weaver.Add(Instruction.Create(OpCodes.Ret));
                        weaver.Add(i2);
                        weaver.Add(Instruction.Create(OpCodes.Unbox_Any, method.ReturnType));
                        weaver.Add(Instruction.Create(OpCodes.Ret));
                    }
                    else
                    {
                        // Create variable
                        VariableDefinition returnvar = weaver.AddVariable(method.Module.TypeSystem.Object, "returnvar");

                        // Store the return value in it
                        weaver.Stloc(returnvar);
                        weaver.Ldloc(returnvar);

                        // If it's non-null and matches the return type, return it - else continue
                        weaver.Add(Instruction.Create(OpCodes.Isinst, method.ReturnType));
                        Instruction i = weaver.Add(Instruction.Create(OpCodes.Ldnull));
                        weaver.Add(Instruction.Create(OpCodes.Beq_S, i.Next));
                        weaver.Ldloc(returnvar);
                        weaver.Add(Instruction.Create(OpCodes.Unbox_Any, method.ReturnType));
                        weaver.Add(Instruction.Create(OpCodes.Ret));
                    }
                    break;

                case ReturnBehavior.ModifyRefArg:
                    string[] args = Utility.ParseArgumentString(ArgumentString, out _);
                    if (args == null)
                    {
                        break;
                    }

                    for (int i = 0; i < args.Length; i++)
                    {
                        string arg = args[i].ToLowerInvariant();
                        if (arg[0] == 'p' || arg[0] == 'a')
                        {
                            if (int.TryParse(arg.Substring(1), out int index))
                            {
                                ParameterDefinition pdef = method.Parameters[index];
                                if (pdef.ParameterType.IsValueType)
                                {
                                    //weaver.Ldloc(argsvar);
                                    //weaver.Add(ILWeaver.Ldc_I4_n(i));
                                    weaver.Add(Instruction.Create(OpCodes.Ldelem_Ref));
                                    weaver.Add(Instruction.Create(OpCodes.Unbox_Any, pdef.ParameterType));
                                    weaver.Starg(pdef);
                                }
                            }
                        }
                        else if (arg[0] == 'l' || arg[0] == 'v')
                        {
                            if (int.TryParse(arg.Substring(1), out int index))
                            {
                                VariableDefinition vdef = weaver.Variables[index];
                                if (vdef.VariableType.IsValueType)
                                {
                                    weaver.Ldloc(argsvar);
                                    weaver.Add(ILWeaver.Ldc_I4_n(i));
                                    weaver.Add(Instruction.Create(OpCodes.Ldelem_Ref));
                                    weaver.Add(Instruction.Create(OpCodes.Unbox_Any, vdef.VariableType));
                                    weaver.Stloc(vdef);
                                }
                            }
                        }
                    }
                    weaver.Add(Instruction.Create(OpCodes.Pop));
                    break;

                case ReturnBehavior.UseArgumentString:
                    // Deal with it according to the retvalue of the arg string
                    string retvalue;
                    Utility.ParseArgumentString(ArgumentString, out retvalue);
                    if (!string.IsNullOrEmpty(retvalue))
                    {
                        if (retvalue[0] == 'l' && retvalue.Length > 1)
                        {
                            if (int.TryParse(retvalue.Substring(1), out int localindex))
                            {
                                // Create variable and get the target variable
                                VariableDefinition returnvar = weaver.AddVariable(method.Module.TypeSystem.Object, "returnvar");
                                VariableDefinition targetvar = weaver.Variables[localindex];

                                // Store the return value in it
                                weaver.Stloc(returnvar);
                                weaver.Ldloc(returnvar);

                                // If it's non-null and matches the variable type, store it in the target variable
                                weaver.Add(Instruction.Create(OpCodes.Isinst, targetvar.VariableType));
                                Instruction i = weaver.Add(Instruction.Create(OpCodes.Ldnull));
                                weaver.Add(Instruction.Create(OpCodes.Beq_S, i.Next));
                                weaver.Ldloc(returnvar);
                                weaver.Add(Instruction.Create(OpCodes.Unbox_Any, targetvar.VariableType));
                                weaver.Stloc(targetvar);

                                // Handled
                                return;
                            }
                        }
                        else if (retvalue[0] == 'a' && retvalue.Length > 1)
                        {
                            if (int.TryParse(retvalue.Substring(1), out int localindex))
                            {
                                // Create variable and get the target parameter
                                VariableDefinition returnvar = weaver.AddVariable(method.Module.TypeSystem.Object, "returnvar");
                                ParameterDefinition targetvar = method.Parameters[localindex];
                                TypeReference targettype = targetvar.ParameterType is ByReferenceType byReferenceType
                                    ? byReferenceType.ElementType
                                    : targetvar.ParameterType;

                                // Store the return value in it
                                weaver.Stloc(returnvar);
                                weaver.Ldloc(returnvar);

                                // If it's non-null and matches the variable type, store it in the target parameter variable
                                Instruction i = weaver.Add(Instruction.Create(OpCodes.Isinst, targettype));
                                weaver.Add(Instruction.Create(OpCodes.Brfalse_S, i.Next));
                                if (!targetvar.ParameterType.IsValueType)
                                {
                                    weaver.Add(ILWeaver.Ldarg(targetvar));
                                }

                                weaver.Ldloc(returnvar);
                                weaver.Add(Instruction.Create(OpCodes.Unbox_Any, targettype));
                                if (!targetvar.ParameterType.IsValueType)
                                {
                                    weaver.Add(Instruction.Create(OpCodes.Stobj, targettype));
                                }
                                else
                                {
                                    weaver.Starg(targetvar);
                                }

                                // Handled
                                return;
                            }
                        }
                        else if (retvalue == "ret" || retvalue == "return")
                        {
                            // Create variable
                            VariableDefinition returnvar = weaver.AddVariable(method.Module.TypeSystem.Object, "returnvar");

                            // Store the return value in it
                            weaver.Stloc(returnvar);
                            weaver.Ldloc(returnvar);

                            // If it's non-null and matches the return type, return it - else continue
                            weaver.Add(Instruction.Create(OpCodes.Isinst, method.ReturnType));
                            Instruction i = weaver.Add(Instruction.Create(OpCodes.Ldnull));
                            weaver.Add(Instruction.Create(OpCodes.Beq_S, i.Next));
                            weaver.Ldloc(returnvar);
                            weaver.Add(Instruction.Create(OpCodes.Unbox_Any, method.ReturnType));
                            weaver.Add(Instruction.Create(OpCodes.Ret));

                            // Handled
                            return;
                        }
                    }

                    // Not handled
                    weaver.Add(Instruction.Create(OpCodes.Pop));
                    break;
            }
        }

        private bool GetMember(ILWeaver weaver, MethodDefinition originalMethod, TypeDefinition currentArg, string[] target, Patching.Patcher patcher)
        {
            if (currentArg == null || target == null || target.Length == 0)
            {
                return false;
            }

            int i;
            TypeDefinition arg = currentArg;
            for (i = 0; i < target.Length; i++)
            {
                if (GetMember(weaver, originalMethod, ref arg, target[i], patcher))
                {
                    continue;
                }

                ShowMessage($"Could not find the member `{target[i]}` in any of the base classes or interfaces of `{currentArg.Name}`.", "Invalid member", patcher);
                return false;
            }

            if (arg.IsValueType || arg.IsByReference)
            {
                weaver.Add(arg.Module == originalMethod.Module
                    ? Instruction.Create(OpCodes.Box, arg.Resolve())
                    : Instruction.Create(OpCodes.Box, originalMethod.Module.Import(arg.Resolve())));
            }

            return i >= 1;
        }

        private bool GetMember(ILWeaver weaver, MethodDefinition originalMethod, ref TypeDefinition currentArg, string target, Patching.Patcher patcher)
        {
            if (currentArg == null || string.IsNullOrEmpty(target))
            {
                return false;
            }

            while (currentArg != null)
            {
                if (target.Contains('('))
                {
                    string[] methodname = target.Split('(');
                    string[] args = methodname[1].TrimEnd(')').Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    MethodDefinition method = currentArg.Methods.FirstOrDefault(m => m.Name == methodname[0] && m.Parameters.Count == args.Length);
                    if (method != null)
                    {
                        if (method.IsGenericInstance || method.HasGenericParameters || method.Parameters.Count > 0)
                        {
                            return false;
                        }

                        if (method.ReturnType.FullName != "System.Void")
                        {
                            if (method.ReturnType.IsByReference)
                            {
                                if (method.ReturnType.IsValueType)
                                {
                                    weaver.Add(Instruction.Create(OpCodes.Unbox_Any, method.ReturnType));
                                }
                            }
                            else
                            {
                                if (method.ReturnType.IsValueType)
                                {
                                    weaver.Add(Instruction.Create(OpCodes.Box, method.ReturnType));
                                }
                            }
                        }

                        weaver.Add(method.Module == originalMethod.Module
                            ? Instruction.Create(OpCodes.Callvirt, method)
                            : Instruction.Create(OpCodes.Callvirt, originalMethod.Module.Import(method)));


                        currentArg = method.ReturnType.Resolve();

                        return true;
                    }
                }

                if (currentArg.IsClass)
                {
                    if (currentArg.HasFields)
                    {
                        foreach (FieldDefinition field in currentArg.Fields)
                        {
                            if (!string.Equals(field.Name, target, StringComparison.CurrentCultureIgnoreCase))
                            {
                                continue;
                            }

                            weaver.Add(field.Module == originalMethod.Module
                                ? Instruction.Create(OpCodes.Ldfld, field)
                                : Instruction.Create(OpCodes.Ldfld, originalMethod.Module.Import(field)));
                            currentArg = field.FieldType.Resolve();

                            return true;
                        }
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

                        weaver.Add(property.GetMethod.Module == originalMethod.Module
                            ? Instruction.Create(OpCodes.Callvirt, property.GetMethod)
                            : Instruction.Create(OpCodes.Callvirt, originalMethod.Module.Import(property.GetMethod)));
                        currentArg = property.PropertyType.Resolve();

                        return true;
                    }
                }

                if (currentArg.HasInterfaces)
                {
                    foreach (TypeReference intf in currentArg.Interfaces)
                    {
                        TypeDefinition previousArg = currentArg;
                        currentArg = intf.Resolve();
                        if (GetMember(weaver, originalMethod, ref currentArg, target, patcher))
                        {
                            return true;
                        }

                        currentArg = previousArg;
                    }
                }

                if (currentArg.BaseType != null && originalMethod.Module.Assembly != currentArg.BaseType.Module.Assembly)
                {
                    TypeReference baseType = currentArg.BaseType;
                    AssemblyDefinition baseTypeAssembly = AssemblyDefinition.ReadAssembly($"{(patcher != null ? patcher.PatchProject.TargetDirectory : PatcherForm.MainForm.CurrentProject.TargetDirectory)}\\{baseType.Scope.Name}{(baseType.Scope.Name.EndsWith(".dll") ? "" : ".dll")}");
                    currentArg = baseTypeAssembly.MainModule.Types.Single(x => x.FullName == baseType.FullName);
                }
                else
                {
                    currentArg = currentArg.BaseType?.Resolve();
                }
            }

            return false;
        }

        public override IHookSettingsControl CreateSettingsView()
        {
            return new SimpleHookSettingsControl { Hook = this };
        }
    }
}
