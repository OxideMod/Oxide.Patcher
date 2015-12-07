using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using OxidePatcher.Patching;
using OxidePatcher.Views;

using Mono.Cecil;
using Mono.Cecil.Cil;

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

        public override bool ApplyPatch(MethodDefinition original, ILWeaver weaver, AssemblyDefinition oxideassembly, bool console)
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
                if (console == false)
                {
                    MessageBox.Show(string.Format("The injection index specified for {0} is invalid!", this.Name), "Invalid Index", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            // Load the hook name

            // Push the arguments array to the stack and make the call
            VariableDefinition argsvar;
            var firstinjected = PushArgsArray(original, weaver, out argsvar);
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

        private Instruction PushArgsArray(MethodDefinition method, ILWeaver weaver, out VariableDefinition argsvar)
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
                    string field = string.Empty;
                    if (!string.IsNullOrEmpty(arg) && args[i].Contains("."))
                    {
                        string[] split = args[i].Split('.');
                        arg = split[0];
                        field = split[1];
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

                        SpecifyFieldOrProperty(weaver, method.DeclaringType, field);
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
                            pdef = method.Parameters[index];

                            weaver.Add(ILWeaver.Ldarg(pdef));
                            if (pdef.ParameterType.IsByReference)
                            {
                                weaver.Add(Instruction.Create(OpCodes.Ldobj, pdef.ParameterType));
                                weaver.Add(Instruction.Create(OpCodes.Box, pdef.ParameterType));
                            }
                            
                            if (!SpecifyFieldOrProperty(weaver, pdef.ParameterType as TypeDefinition, field) && pdef.ParameterType.IsValueType)
                                weaver.Add(Instruction.Create(OpCodes.Box, pdef.ParameterType));
                        }
                        else
                            weaver.Add(Instruction.Create(OpCodes.Ldnull));
                    }
                    else if (arg[0] == 'l' || arg[0] == 'v')
                    {
                        int index;
                        if (int.TryParse(arg.Substring(1), out index))
                        {
                            VariableDefinition vdef = weaver.Variables[index];

                            weaver.Ldloc(vdef);
                            if (vdef.VariableType.IsByReference)
                            {
                                weaver.Add(Instruction.Create(OpCodes.Ldobj, vdef.VariableType));
                                weaver.Add(Instruction.Create(OpCodes.Box, vdef.VariableType));
                            }
                            
                            if (!SpecifyFieldOrProperty(weaver, vdef.VariableType as TypeDefinition, field) && vdef.VariableType.IsValueType)
                                weaver.Add(Instruction.Create(OpCodes.Box, vdef.VariableType));
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
                                if(!targetvar.ParameterType.IsValueType)
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

        private bool SpecifyFieldOrProperty(ILWeaver weaver, TypeDefinition tdef, string name)
        {
            var fieldIncluded = false;

            if (tdef != null && tdef.IsClass && name != string.Empty)
            {
                while (tdef != null)
                {
                    if (tdef.HasFields)
                    {
                        foreach (var fld in tdef.Fields)
                        {
                            if (fieldIncluded) break;
                            if (!string.Equals(fld.Name, name, StringComparison.CurrentCultureIgnoreCase)) continue;
                            weaver.Add(Instruction.Create(OpCodes.Ldfld, fld));
                            if (fld.FieldType.IsByReference)
                                weaver.Add(Instruction.Create(OpCodes.Box, fld.FieldType));
                            fieldIncluded = true;
                        }
                    }

                    if (fieldIncluded) break;
                    if (tdef.HasProperties)
                    {
                        foreach (var property in tdef.Properties)
                        {
                            if (fieldIncluded) break;
                            if (!string.Equals(property.Name, name, StringComparison.CurrentCultureIgnoreCase)) continue;
                            weaver.Add(Instruction.Create(OpCodes.Call, property.GetMethod));
                            if (property.PropertyType.IsByReference)
                                weaver.Add(Instruction.Create(OpCodes.Box, property.PropertyType));
                            fieldIncluded = true;
                        }
                    }

                    tdef = tdef.BaseType as TypeDefinition;
                }
            }

            return fieldIncluded;
        }

        public override HookSettingsControl CreateSettingsView()
        {
            SimpleHookSettingsControl control = new SimpleHookSettingsControl();
            control.Hook = this;
            return control;
        }
    }
}
