using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using OxidePatcher.Patching;
using OxidePatcher.Views;

using Mono.Cecil;
using Mono.Cecil.Cil;

using AssemblyDefinition = Mono.Cecil.AssemblyDefinition;
using TypeDefinition = Mono.Cecil.TypeDefinition;

namespace OxidePatcher.Hooks
{
    public enum ReturnBehavior { Continue, ExitWhenValidType, ModifyRefArg, UseArgumentString }

    public enum ArgumentBehavior { None, JustThis, JustParams, All, UseArgumentString }

    /// <summary>
    /// A simple hook that injects at a certain point in the method, with a few options for handling arguments and return values
    /// </summary>
    [HookType("Simple", Default = true)]
    public class Simple : Hook
    {
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

        /// <summary>
        /// Resolves assembly dependencies
        /// </summary>
        private DefaultAssemblyResolver resolver;

        public override bool ApplyPatch(MethodDefinition original, ILWeaver weaver, AssemblyDefinition oxideassembly, Patcher patcher = null)
        {
            // Get the call hook method
            MethodDefinition callhookmethod = oxideassembly.MainModule.Types
                .Single((t) => t.FullName == "Oxide.Core.Interface")
                .Methods.Single((m) => m.IsStatic && m.Name == "CallHook");

            // Start injecting where requested
            weaver.Pointer = InjectionIndex;

            // Get the existing instruction we're going to inject behind
            Instruction existing;
            try
            {
                existing = weaver.Instructions[weaver.Pointer];
            }
            catch (ArgumentOutOfRangeException)
            {
                ShowMsg(string.Format("The injection index specified for {0} is invalid!", Name), "Invalid Index", patcher);
                return false;
            }

            // Load the hook name

            // Push the arguments array to the stack and make the call
            VariableDefinition argsvar;
            var firstinjected = PushArgsArray(original, weaver, out argsvar, patcher);
            var hookname = weaver.Add(Instruction.Create(OpCodes.Ldstr, HookName));
            if (firstinjected == null) firstinjected = hookname;
            if (argsvar != null)
                weaver.Ldloc(argsvar);
            else
                weaver.Add(Instruction.Create(OpCodes.Ldnull));
            weaver.Add(Instruction.Create(OpCodes.Call, original.Module.Import(callhookmethod)));

            // Deal with the return value
            DealWithReturnValue(original, argsvar, weaver);

            // Find all instructions which pointed to the existing and redirect them
            for (int i = 0; i < weaver.Instructions.Count; i++)
            {
                Instruction ins = weaver.Instructions[i];
                if (ins.Operand != null && ins.Operand.Equals(existing))
                {
                    // Check if the instruction lies within our injection range
                    // If it does, it's an instruction we just injected so we don't want to edit it
                    if (i < InjectionIndex || i > weaver.Pointer)
                        ins.Operand = firstinjected;
                }
            }
            return true;
        }

        private Instruction PushArgsArray(MethodDefinition method, ILWeaver weaver, out VariableDefinition argsvar, Patcher patcher)
        {
            // Are we going to use arguments?
            if (ArgumentBehavior == Hooks.ArgumentBehavior.None)
            {
                // Push null and we're done
                argsvar = null;
                return null;
            }

            // Create array variable
            Instruction firstInstruction;
            // Are we using the argument string?
            if (ArgumentBehavior == Hooks.ArgumentBehavior.UseArgumentString)
            {
                string retvalue;
                string[] args = ParseArgumentString(out retvalue);
                if (args == null)
                {
                    // Silently fail, but at least produce valid IL
                    argsvar = null;
                    return null;
                }

                // Create the array
                argsvar = weaver.AddVariable(new ArrayType(method.Module.TypeSystem.Object), "args");
                firstInstruction = weaver.Add(ILWeaver.Ldc_I4_n(args.Length));
                weaver.Add(Instruction.Create(OpCodes.Newarr, method.Module.TypeSystem.Object));
                weaver.Stloc(argsvar);

                // Populate it
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i].ToLowerInvariant();
                    string[] target = null;
                    if (!string.IsNullOrEmpty(arg) && args[i].Contains("."))
                    {
                        string[] split = args[i].Split('.');
                        arg = split[0];
                        target = split.Skip(1).ToArray();
                    }

                    weaver.Ldloc(argsvar);
                    weaver.Add(ILWeaver.Ldc_I4_n(i));
                    if (string.IsNullOrEmpty(arg))
                        weaver.Add(Instruction.Create(OpCodes.Ldnull));
                    else if (arg == "this")
                    {
                        if (method.IsStatic)
                            weaver.Add(Instruction.Create(OpCodes.Ldnull));
                        else
                            weaver.Add(ILWeaver.Ldarg(null));

                        GetFieldOrProperty(weaver, method, method.DeclaringType.Resolve(), target, patcher);
                    }
                    else if (arg[0] == 'p' || arg[0] == 'a')
                    {
                        int index;
                        if (int.TryParse(arg.Substring(1), out index))
                        {
                            ParameterDefinition pdef;

                            /*if (method.IsStatic)
                                pdef = method.Parameters[index];
                            else
                                pdef = method.Parameters[index + 1];*/
                            if (index < method.Parameters.Count)
                            {
                                pdef = method.Parameters[index];

                                weaver.Add(ILWeaver.Ldarg(pdef));
                                if (pdef.ParameterType.IsByReference)
                                {
                                    weaver.Add(Instruction.Create(OpCodes.Ldobj, pdef.ParameterType));
                                    weaver.Add(Instruction.Create(OpCodes.Box, pdef.ParameterType));
                                }

                                if (
                                    !GetFieldOrProperty(weaver, method, pdef.ParameterType.Resolve(), target, patcher) &&
                                    pdef.ParameterType.IsValueType)
                                    weaver.Add(Instruction.Create(OpCodes.Box, pdef.ParameterType));
                            }
                            else
                                ShowMsg($"Invalid argument `{arg}` supplied for {HookName}", "Invalid argument supplied", patcher);
                        }
                        else
                            weaver.Add(Instruction.Create(OpCodes.Ldnull));
                    }
                    else if (arg[0] == 'l' || arg[0] == 'v')
                    {
                        int index;
                        if (int.TryParse(arg.Substring(1), out index))
                        {
                            if (index < method.Body.Variables.Count)
                            {
                                VariableDefinition vdef = weaver.Variables[index];

                                weaver.Ldloc(vdef);
                                if (vdef.VariableType.IsByReference)
                                {
                                    weaver.Add(Instruction.Create(OpCodes.Ldobj, vdef.VariableType));
                                    weaver.Add(Instruction.Create(OpCodes.Box, vdef.VariableType));
                                }

                                if (!GetFieldOrProperty(weaver, method, vdef.VariableType.Resolve(), target, patcher) && vdef.VariableType.IsValueType)
                                    weaver.Add(Instruction.Create(OpCodes.Box, vdef.VariableType));
                            }
                            else
                                ShowMsg($"Invalid variable `{arg}` supplied for {HookName}", "Invalid variable supplied", patcher);
                        }
                        else
                            weaver.Add(Instruction.Create(OpCodes.Ldnull));
                    }
                    else
                        weaver.Add(Instruction.Create(OpCodes.Ldnull));

                    weaver.Add(Instruction.Create(OpCodes.Stelem_Ref));
                }
            }
            else
            {
                // Figure out what we're doing
                bool includeargs = ArgumentBehavior == Hooks.ArgumentBehavior.All || ArgumentBehavior == Hooks.ArgumentBehavior.JustParams;
                bool includethis = ArgumentBehavior == Hooks.ArgumentBehavior.All || ArgumentBehavior == Hooks.ArgumentBehavior.JustThis;
                if (method.IsStatic) includethis = false;

                // Work out what arguments we're going to transmit
                List<ParameterDefinition> args = new List<ParameterDefinition>();
                if (includeargs)
                {
                    for (int i = 0; i < method.Parameters.Count; i++)
                    {
                        ParameterDefinition arg = method.Parameters[i];
                        if (!arg.IsOut)
                            args.Add(arg);
                    }
                }

                argsvar = weaver.AddVariable(new ArrayType(method.Module.TypeSystem.Object), "args");

                // Load arg count, create array, store
                if (includethis)
                    firstInstruction = weaver.Add(ILWeaver.Ldc_I4_n(args.Count + 1));
                else
                    firstInstruction = weaver.Add(ILWeaver.Ldc_I4_n(args.Count));
                weaver.Add(Instruction.Create(OpCodes.Newarr, method.Module.TypeSystem.Object));
                weaver.Stloc(argsvar);

                // Include this
                if (includethis)
                {
                    weaver.Ldloc(argsvar);
                    weaver.Add(ILWeaver.Ldc_I4_n(0));
                    weaver.Add(ILWeaver.Ldarg(null));
                    weaver.Add(Instruction.Create(OpCodes.Stelem_Ref));
                }

                // Loop each argument
                for (int i = 0; i < args.Count; i++)
                {
                    // Load array, load index load arg, store in array
                    ParameterDefinition arg = args[i];
                    weaver.Ldloc(argsvar);
                    if (includethis)
                        weaver.Add(ILWeaver.Ldc_I4_n(i + 1));
                    else
                        weaver.Add(ILWeaver.Ldc_I4_n(i));
                    weaver.Add(ILWeaver.Ldarg(args[i]));
                    if (arg.ParameterType.IsByReference)
                    {
                        weaver.Add(Instruction.Create(OpCodes.Ldobj, arg.ParameterType));
                        weaver.Add(Instruction.Create(OpCodes.Box, arg.ParameterType));
                    }
                    else if (arg.ParameterType.IsValueType)
                        weaver.Add(Instruction.Create(OpCodes.Box, arg.ParameterType));
                    weaver.Add(Instruction.Create(OpCodes.Stelem_Ref));
                }
            }
            return firstInstruction;
        }

        private void DealWithReturnValue(MethodDefinition method, VariableDefinition argsvar, ILWeaver weaver)
        {
            // What return behavior do we use?
            switch (ReturnBehavior)
            {
                case Hooks.ReturnBehavior.Continue:
                    // Just discard the return value
                    weaver.Add(Instruction.Create(OpCodes.Pop));
                    break;
                case Hooks.ReturnBehavior.ExitWhenValidType:
                    // Is there a return value or not?
                    if (method.ReturnType.FullName == "System.Void")
                    {
                        // If the hook returned something that was non-null, return
                        Instruction i = weaver.Add(Instruction.Create(OpCodes.Ldnull));
                        weaver.Add(Instruction.Create(OpCodes.Beq_S, i.Next));
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
                case Hooks.ReturnBehavior.ModifyRefArg:
                    string wayne;
                    var args = ParseArgumentString(out wayne);
                    if (args == null)
                    {
                        break;
                    }
                    for (int i = 0; i < args.Length; i++)
                    {
                        string arg = args[i].ToLowerInvariant();
                        if (arg[0] == 'p' || arg[0] == 'a')
                        {
                            int index;
                            if (int.TryParse(arg.Substring(1), out index))
                            {
                                var pdef = method.Parameters[index];
                                if (pdef.ParameterType.IsValueType)
                                {
                                    weaver.Ldloc(argsvar);
                                    weaver.Add(ILWeaver.Ldc_I4_n(i));
                                    weaver.Add(Instruction.Create(OpCodes.Ldelem_Ref));
                                    weaver.Add(Instruction.Create(OpCodes.Unbox_Any, pdef.ParameterType));
                                    weaver.Starg(pdef);
                                }
                            }
                        }
                        else if (arg[0] == 'l' || arg[0] == 'v')
                        {
                            int index;
                            if (int.TryParse(arg.Substring(1), out index))
                            {
                                var vdef = weaver.Variables[index];
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
                case Hooks.ReturnBehavior.UseArgumentString:
                    // Deal with it according to the retvalue of the arg string
                    string retvalue;
                    ParseArgumentString(out retvalue);
                    if (!string.IsNullOrEmpty(retvalue))
                    {
                        if (retvalue[0] == 'l' && retvalue.Length > 1)
                        {
                            int localindex;
                            if (int.TryParse(retvalue.Substring(1), out localindex))
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
                            int localindex;
                            if (int.TryParse(retvalue.Substring(1), out localindex))
                            {
                                // Create variable and get the target parameter
                                VariableDefinition returnvar = weaver.AddVariable(method.Module.TypeSystem.Object, "returnvar");
                                ParameterDefinition targetvar = method.Parameters[localindex];
                                var byReferenceType = targetvar.ParameterType as ByReferenceType;
                                TypeReference targettype = byReferenceType != null
                                    ? byReferenceType.ElementType
                                    : targetvar.ParameterType;

                                // Store the return value in it
                                weaver.Stloc(returnvar);
                                weaver.Ldloc(returnvar);

                                // If it's non-null and matches the variable type, store it in the target parameter variable
                                Instruction i = weaver.Add(Instruction.Create(OpCodes.Isinst, targettype));
                                weaver.Add(Instruction.Create(OpCodes.Brfalse_S, i.Next));
                                if (!targetvar.ParameterType.IsValueType)
                                    weaver.Add(ILWeaver.Ldarg(targetvar));
                                weaver.Ldloc(returnvar);
                                weaver.Add(Instruction.Create(OpCodes.Unbox_Any, targettype));
                                if (!targetvar.ParameterType.IsValueType)
                                    weaver.Add(Instruction.Create(OpCodes.Stobj, targettype));
                                else
                                    weaver.Starg(targetvar);

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

        private string[] ParseArgumentString(out string retvalue)
        {
            // Check arg string for null
            if (string.IsNullOrEmpty(ArgumentString))
            {
                retvalue = null;
                return null;
            }

            // Strip whitespace
            string argstr = new string(ArgumentString.Where((c) => !char.IsWhiteSpace(c)).ToArray());

            // Split by return value indicator
            string[] leftright = argstr.Split(new string[1] { "=>" }, StringSplitOptions.RemoveEmptyEntries);
            if (leftright.Length == 0)
            {
                retvalue = null;
                return null;
            }

            // Split by comma
            string[] args = leftright[0].Split(',');

            // Set the return value
            if (leftright.Length > 1)
                retvalue = leftright[1];
            else
                retvalue = null;

            // Return
            return args;
        }

        private bool GetFieldOrProperty(ILWeaver weaver, MethodDefinition originalMethod, TypeDefinition currentArg, string[] target, Patcher patcher)
        {
            if (resolver == null)
            {
                resolver = new DefaultAssemblyResolver();
                resolver.AddSearchDirectory(patcher?.PatchProject?.Configuration?.AssembliesSourceDirectory ?? PatcherForm.MainForm.CurrentProject.Configuration.AssembliesSourceDirectory);
            }

            if (currentArg == null || target == null || target.Length == 0) return false;

            int i;
            var arg = currentArg;
            for (i = 0; i < target.Length; i++)
            {
                if (GetFieldOrProperty(weaver, originalMethod, ref arg, target[i])) continue;
                ShowMsg($"Could not find the field or property `{target[i]}` in any of the base classes or interfaces of `{currentArg.Name}`.", "Invalid field or property", patcher);
                return false;
            }

            if (arg.IsValueType || arg.IsByReference)
                weaver.Add(arg.Module == originalMethod.Module
                    ? Instruction.Create(OpCodes.Box, arg.Resolve())
                    : Instruction.Create(OpCodes.Box, originalMethod.Module.Import(arg.Resolve())));

            return i >= 1;
        }

        private bool GetFieldOrProperty(ILWeaver weaver, MethodDefinition originalMethod, ref TypeDefinition currentArg, string target)
        {
            if (currentArg == null || string.IsNullOrEmpty(target)) return false;

            while (currentArg != null)
            {
                if (currentArg.IsClass)
                {
                    if (currentArg.HasFields)
                    {
                        foreach (var field in currentArg.Fields)
                        {
                            if (!string.Equals(field.Name, target, StringComparison.CurrentCultureIgnoreCase)) continue;

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
                    foreach (var property in currentArg.Properties)
                    {
                        if (!string.Equals(property.Name, target, StringComparison.CurrentCultureIgnoreCase)) continue;

                        weaver.Add(property.GetMethod.Module == originalMethod.Module
                            ? Instruction.Create(OpCodes.Callvirt, property.GetMethod)
                            : Instruction.Create(OpCodes.Callvirt, originalMethod.Module.Import(property.GetMethod)));
                        currentArg = property.PropertyType.Resolve();

                        return true;
                    }
                }

                if (currentArg.HasInterfaces)
                {
                    foreach (var intf in currentArg.Interfaces)
                    {
                        var previousArg = currentArg;
                        currentArg = intf.Resolve();
                        if (GetFieldOrProperty(weaver, originalMethod, ref currentArg, target)) return true;
                        currentArg = previousArg;
                    }
                }

                currentArg = currentArg.BaseType?.Resolve();
            }

            return false;
        }

        public override HookSettingsControl CreateSettingsView()
        {
            return new SimpleHookSettingsControl { Hook = this };
        }
    }
}
