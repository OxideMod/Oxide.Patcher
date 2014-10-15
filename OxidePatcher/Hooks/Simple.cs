using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;

using OxidePatcher.Patching;
using OxidePatcher.Views;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace OxidePatcher.Hooks
{
    public enum ReturnBehaviour { Continue, ExitWhenValidType, ModifyRefArg, UseArgumentString }

    public enum ArgumentBehaviour { None, JustThis, JustParams, All, UseArgumentString }

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
        /// Gets or sets the return behaviour
        /// </summary>
        public ReturnBehaviour ReturnBehaviour { get; set; }

        /// <summary>
        /// Gets or sets the argument behaviour
        /// </summary>
        public ArgumentBehaviour ArgumentBehaviour { get; set; }

        /// <summary>
        /// Gets or sets the argument string
        /// </summary>
        public string ArgumentString { get; set; }

        public override void ApplyPatch(MethodDefinition original, ILWeaver weaver, AssemblyDefinition oxideassembly)
        {
            // Get the call hook method
            MethodDefinition callhookmethod = oxideassembly.MainModule.Types
                .Single((t) => t.FullName == "Oxide.Core.Interface")
                .Methods.Single((m) => m.IsStatic && m.Name == "CallHook");

            // Start injecting where requested
            weaver.Pointer = InjectionIndex;

            // Get the existing instruction we're going to inject behind
            Instruction existing = weaver.Instructions[weaver.Pointer];

            // Load the hook name
            Instruction firstinjected = weaver.Add(Instruction.Create(OpCodes.Ldstr, HookName));

            // Push the arguments array to the stack and make the call
            PushArgsArray(original, weaver);
            weaver.Add(Instruction.Create(OpCodes.Call, original.Module.Import(callhookmethod)));

            // Deal with the return value
            DealWithReturnValue(original, weaver);

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
        }

        private void PushArgsArray(MethodDefinition method, ILWeaver weaver)
        {
            // Are we going to use arguments?
            if (ArgumentBehaviour == Hooks.ArgumentBehaviour.None)
            {
                // Push null and we're done
                weaver.Add(Instruction.Create(OpCodes.Ldnull));
                return;
            }

            // Create array variable
            VariableDefinition argsvar = weaver.AddVariable(new ArrayType(method.Module.TypeSystem.Object), "args");

            // Are we using the argument string?
            if (ArgumentBehaviour == Hooks.ArgumentBehaviour.UseArgumentString)
            {
                string retvalue;
                string[] args = ParseArgumentString(out retvalue);
                if (args == null)
                {
                    // Silently fail, but at least produce valid IL
                    weaver.Add(Instruction.Create(OpCodes.Ldnull));
                    return;
                }

                // Create the array
                weaver.Add(ILWeaver.Ldc_I4_n(args.Length));
                weaver.Add(Instruction.Create(OpCodes.Newarr, method.Module.TypeSystem.Object));
                weaver.Stloc(argsvar);

                // Populate it
                for (int i = 0; i < args.Length; i++)
                {
                    weaver.Ldloc(argsvar);
                    string arg = args[i].ToLowerInvariant();
                    weaver.Add(ILWeaver.Ldc_I4_n(i));
                    if (string.IsNullOrEmpty(arg))
                        weaver.Add(Instruction.Create(OpCodes.Ldnull));
                    else if (arg == "this")
                    {
                        if (method.IsStatic)
                            weaver.Add(Instruction.Create(OpCodes.Ldnull));
                        else
                            weaver.Add(ILWeaver.Ldarg_n(0));
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

                            weaver.Add(ILWeaver.Ldarg_n(pdef.Sequence));
                            if (pdef.ParameterType.IsByReference)
                            {
                                weaver.Add(Instruction.Create(OpCodes.Ldobj, pdef.ParameterType));
                                weaver.Add(Instruction.Create(OpCodes.Box, pdef.ParameterType));
                            }
                            else if (pdef.ParameterType.IsValueType)
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

                            if (vdef.VariableType.IsByReference)
                            {
                                weaver.Add(Instruction.Create(OpCodes.Ldobj, vdef.VariableType));
                                weaver.Add(Instruction.Create(OpCodes.Box, vdef.VariableType));
                            }
                            else if (vdef.VariableType.IsValueType)
                                weaver.Add(Instruction.Create(OpCodes.Box, vdef.VariableType));

                            weaver.Ldloc(vdef);
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
                bool includeargs = ArgumentBehaviour == Hooks.ArgumentBehaviour.All || ArgumentBehaviour == Hooks.ArgumentBehaviour.JustParams;
                bool includethis = ArgumentBehaviour == Hooks.ArgumentBehaviour.All || ArgumentBehaviour == Hooks.ArgumentBehaviour.JustThis;
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

                // Load arg count, create array, store
                if (includethis)
                    weaver.Add(ILWeaver.Ldc_I4_n(args.Count + 1));
                else
                    weaver.Add(ILWeaver.Ldc_I4_n(args.Count));
                weaver.Add(Instruction.Create(OpCodes.Newarr, method.Module.TypeSystem.Object));
                weaver.Stloc(argsvar);

                // Include this
                if (includethis)
                {
                    weaver.Ldloc(argsvar);
                    weaver.Add(ILWeaver.Ldc_I4_n(0));
                    weaver.Add(ILWeaver.Ldarg_n(0));
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
                    weaver.Add(ILWeaver.Ldarg_n(args[i].Sequence));
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

            // Finally, load it
            weaver.Ldloc(argsvar);
        }

        private void DealWithReturnValue(MethodDefinition method, ILWeaver weaver)
        {
            // What return behaviour do we use?
            switch (ReturnBehaviour)
            {
                case Hooks.ReturnBehaviour.Continue:
                    // Just discard the return value
                    weaver.Add(Instruction.Create(OpCodes.Pop));
                    break;
                case Hooks.ReturnBehaviour.ExitWhenValidType:
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
                case Hooks.ReturnBehaviour.ModifyRefArg:
                    // TODO: This
                    weaver.Add(Instruction.Create(OpCodes.Pop));
                    break;
                case Hooks.ReturnBehaviour.UseArgumentString:
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


        public override HookSettingsControl CreateSettingsView()
        {
            SimpleHookSettingsControl control = new SimpleHookSettingsControl();
            control.Hook = this;
            return control;
        }
    }
}
