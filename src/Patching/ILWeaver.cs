using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Oxide.Patcher.Patching
{
    /// <summary>
    /// Represents a set of modifiable instructions
    /// </summary>
    public sealed class ILWeaver : IEnumerable<Instruction>
    {
        private static readonly Dictionary<OpCode, OpCode> BranchSubstitutionTable = new Dictionary<OpCode, OpCode>
        {
            { OpCodes.Brfalse_S, OpCodes.Brfalse },
            { OpCodes.Brtrue_S, OpCodes.Brtrue },
            { OpCodes.Leave_S, OpCodes.Leave },
            { OpCodes.Br_S, OpCodes.Br },
            { OpCodes.Bge_S, OpCodes.Bge },
            { OpCodes.Bgt_S, OpCodes.Bgt },
            { OpCodes.Ble_S, OpCodes.Ble },
            { OpCodes.Ble_Un_S, OpCodes.Ble_Un },
            { OpCodes.Bne_Un_S, OpCodes.Bne_Un },
            { OpCodes.Beq_S, OpCodes.Beq },
            { OpCodes.Blt_S, OpCodes.Blt },
            { OpCodes.Blt_Un_S, OpCodes.Blt_Un },
            { OpCodes.Bgt_Un_S, OpCodes.Bgt_Un },
            { OpCodes.Bge_Un_S, OpCodes.Bge_Un },
        };

        /// <summary>
        /// Gets the instructions
        /// </summary>
        public List<Instruction> Instructions { get; } = new List<Instruction>();

        /// <summary>
        /// Gets the local variables
        /// </summary>
        public List<VariableDefinition> Variables { get; } = new List<VariableDefinition>();

        /// <summary>
        /// Gets the exception handlers
        /// </summary>
        public List<ExceptionHandler> ExceptionHandlers { get; } = new List<ExceptionHandler>();

        /// <summary>
        /// Gets the created local variables for data on the stack (instruction index : variable id)
        /// </summary>
        public Dictionary<int, int> IntroducedLocals { get; } = new Dictionary<int, int>();

        /// <summary>
        /// Gets or sets the current instruction pointer
        /// </summary>
        public int Pointer { get; set; }

        /// <summary>
        /// Gets or sets the original instruction pointer (initial injection index)
        /// </summary>
        public int OriginalPointer { get; set; }

        /// <summary>
        /// Gets or sets the module to which this weaver belongs
        /// </summary>
        public ModuleDefinition Module { get; set; }

        /// <summary>
        /// Gets a SHA256 hash of this weaver's instructions
        /// </summary>
        public string Hash => Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(ToString())));

        /// <summary>
        /// Initializes a new instance of the ILWeaver class with a copy of the specified method's data
        /// </summary>
        /// <param name="body"></param>
        public ILWeaver(MethodBody body)
        {
            foreach (Instruction existing in body.Instructions)
            {
                Instruction newinst = Instruction.Create(OpCodes.Pop); // Dummy instruction
                newinst.OpCode = existing.OpCode;
                newinst.Operand = existing.Operand;
                newinst.Offset = existing.Offset;
                Instructions.Add(newinst);
            }

            for (int i = 0; i < body.Instructions.Count; i++)
            {
                Instruction existing = body.Instructions[i];

                if (existing.Operand is Instruction operand)
                {
                    int otherindex = body.Instructions.IndexOf(operand);
                    Instructions[i].Operand = Instructions[otherindex];
                }

                Instruction[] instructions = existing.Operand as Instruction[];
                if (instructions == null)
                {
                    continue;
                }

                Instruction[] newOperand = new Instruction[instructions.Length];
                Instructions[i].Operand = newOperand;
                for (int index = 0; index < instructions.Length; index++)
                {
                    int otherindex = body.Instructions.IndexOf(instructions[index]);
                    newOperand[index] = Instructions[otherindex];
                }
            }

            foreach (ExceptionHandler existing in body.ExceptionHandlers)
            {
                ExceptionHandler newexhandler = new ExceptionHandler(ExceptionHandlerType.Catch) // Dummy handler
                {
                    HandlerType = existing.HandlerType,
                    CatchType = existing.CatchType
                };

                if (existing.TryStart != null)
                {
                    newexhandler.TryStart = Instructions[body.Instructions.IndexOf(existing.TryStart)];
                }

                if (existing.TryEnd != null)
                {
                    newexhandler.TryEnd = Instructions[body.Instructions.IndexOf(existing.TryEnd)];
                }

                if (existing.FilterStart != null)
                {
                    newexhandler.FilterStart = Instructions[body.Instructions.IndexOf(existing.FilterStart)];
                }

                if (existing.HandlerStart != null)
                {
                    newexhandler.HandlerStart = Instructions[body.Instructions.IndexOf(existing.HandlerStart)];
                }

                if (existing.HandlerEnd != null)
                {
                    newexhandler.HandlerEnd = Instructions[body.Instructions.IndexOf(existing.HandlerEnd)];
                }

                ExceptionHandlers.Add(newexhandler);
            }

            UpdateInstructions();
            Variables.AddRange(body.Variables);
            Pointer = Instructions.Count - 1;
            OriginalPointer = Pointer;
        }

        public ILWeaver(MethodBody body, ModuleDefinition module) : this(body)
        {
            Module = module;
        }

        /// <summary>
        /// Initializes a new instance of the ILWeaver class with the specified instruction and variable sets
        /// </summary>
        /// <param name="instructions"></param>
        /// <param name="variables"></param>
        public ILWeaver(IEnumerable<Instruction> instructions, IEnumerable<VariableDefinition> variables)
        {
            Instructions = new List<Instruction>(instructions);
            Variables = new List<VariableDefinition>(variables);
            Pointer = Instructions.Count - 1;
            OriginalPointer = Pointer;
        }

        /// <summary>
        /// Adds an instruction at the pointer
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public Instruction Add(Instruction instruction)
        {
            Instructions.Insert(Pointer, instruction);
            UpdateInstructions();
            AdjustBranches();
            UpdateExceptionHandlers();
            Pointer++;
            return instruction;
        }

        /// <summary>
        /// Adds an instruction after specified position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public Instruction AddAfter(int position, Instruction Instruction)
        {
            Instructions.Insert(position + 1, Instruction);
            UpdateInstructions();
            AdjustBranches();
            UpdateExceptionHandlers();
            Pointer++;
            return Instruction;
        }

        /// <summary>
        /// Removes an instruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public bool Remove(Instruction instruction)
        {
            if (Instructions.Remove(instruction))
            {
                UpdateInstructions();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes instructions after the pointer
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool RemoveAfter(int count)
        {
            if (Pointer + count >= Instructions.Count)
            {
                return false;
            }

            List<Instruction> toRemove = new List<Instruction>();
            for (int i = 0; i < count; i++)
            {
                toRemove.Add(Instructions[Pointer + i]);
            }

            for (int j = ExceptionHandlers.Count - 1; j >= 0; j--)
            {
                ExceptionHandler handler = ExceptionHandlers[j];
                if (!toRemove.Contains(handler.TryStart) && !toRemove.Contains(handler.TryEnd) && !toRemove.Contains(handler.HandlerStart) && !toRemove.Contains(handler.HandlerEnd))
                {
                    continue;
                }

                int start = Instructions.IndexOf(handler.TryStart);
                int end = Instructions.IndexOf(handler.TryEnd);
                for (int i = start; i < end; i++)
                {
                    toRemove.Add(Instructions[i]);
                }

                ExceptionHandlers.RemoveAt(j);
            }

            foreach (Instruction instruction in toRemove)
            {
                Instructions.Remove(instruction);
            }

            if (toRemove.Count > 0)
            {
                //Fix branches
                Instruction firstRemove = toRemove[0];
                foreach (Instruction instruction in Instructions)
                {
                    if (instruction.Operand == firstRemove)
                    {
                        instruction.Operand = Instructions[Pointer];
                    }
                }
            }

            UpdateInstructions();
            return true;
        }

        /// <summary>
        /// Updates the offset and "linkedness" of all instructions
        /// </summary>
        private void UpdateInstructions()
        {
            int curoffset = 0;
            for (int i = 0; i < Instructions.Count; i++)
            {
                Instruction ins = Instructions[i];
                if (i == 0)
                {
                    ins.Previous = null;
                }
                else
                {
                    ins.Previous = Instructions[i - 1];
                }

                if (i == Instructions.Count - 1)
                {
                    ins.Next = null;
                }
                else
                {
                    ins.Next = Instructions[i + 1];
                }

                ins.Offset = curoffset;
                curoffset += ins.GetSize();
            }
        }

        private static OpCode LongSubstituteOf(OpCode opCode)
            => !BranchSubstitutionTable.TryGetValue(opCode, out var newOpCode) ? opCode : newOpCode;

        private void AdjustBranches()
        {
            var needsUpdate = false;
            for (var i = 0; i < Instructions.Count; i++)
            {
                var ins = Instructions[i];
                if (!(ins.Operand is Instruction operand) || !BranchSubstitutionTable.ContainsKey(ins.OpCode))
                    continue;
                {
                    var isFwd = operand.Offset >= ins.Offset;
                    var offsetTest = isFwd
                        ? operand.Offset - ins.Offset <= 129
                        : ins.Offset - operand.Offset <= 126;
                    if (!offsetTest)
                    {
                        ins.OpCode = LongSubstituteOf(ins.OpCode);
                        needsUpdate = true;
                    }
                }
            }
            if (needsUpdate)
                UpdateInstructions();
        }

        private void UpdateExceptionHandlers()
        {
            if (Pointer == 0 || ExceptionHandlers.Count == 0 || Instructions[Pointer - 1].OpCode != OpCodes.Endfinally)
                return;
            var oldHandlerEnd = Instructions[Pointer + 1];
            var newHandlerEnd = Instructions[Pointer];
            for (var i = 0; i < ExceptionHandlers.Count; i++)
            {
                var handler = ExceptionHandlers[i];
                if (handler.HandlerStart == null || handler.HandlerEnd != oldHandlerEnd)
                    continue;
                handler.HandlerEnd = newHandlerEnd;
                break;
            }
        }

        /// <summary>
        /// Adds a variable
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public VariableDefinition AddVariable(TypeReference type, string name = "")
        {
            VariableDefinition def = new VariableDefinition(type)
            {
                Name = string.IsNullOrEmpty(name) ? $"OxideGen_{Variables.Count}" : name
            };
            Variables.Add(def);
            return def;
        }

        public Instruction Stloc(VariableDefinition variable)
        {
            int n = Variables.IndexOf(variable);
            Instruction inst;
            switch (n)
            {
                case 0:
                    inst = Instruction.Create(OpCodes.Stloc_0);
                    break;

                case 1:
                    inst = Instruction.Create(OpCodes.Stloc_1);
                    break;

                case 2:
                    inst = Instruction.Create(OpCodes.Stloc_2);
                    break;

                case 3:
                    inst = Instruction.Create(OpCodes.Stloc_3);
                    break;

                default:
                    inst = Instruction.Create(OpCodes.Stloc_S, variable);
                    break;
            }
            Add(inst);
            return inst;
        }

        public Instruction Starg(ParameterDefinition parameter)
        {
            Instruction inst = Instruction.Create(OpCodes.Starg, parameter);
            Add(inst);
            return inst;
        }

        public Instruction Ldloc(VariableDefinition variable)
        {
            int n = Variables.IndexOf(variable);
            Instruction inst;
            switch (n)
            {
                case 0:
                    inst = Instruction.Create(OpCodes.Ldloc_0);
                    break;

                case 1:
                    inst = Instruction.Create(OpCodes.Ldloc_1);
                    break;

                case 2:
                    inst = Instruction.Create(OpCodes.Ldloc_2);
                    break;

                case 3:
                    inst = Instruction.Create(OpCodes.Ldloc_3);
                    break;

                default:
                    inst = Instruction.Create(OpCodes.Ldloc_S, variable);
                    break;
            }
            Add(inst);
            return inst;
        }

        #region Utility

        public static Instruction Ldc_I4_n(int n)
        {
            if (n == 0)
            {
                return Instruction.Create(OpCodes.Ldc_I4_0);
            }

            if (n == 1)
            {
                return Instruction.Create(OpCodes.Ldc_I4_1);
            }

            if (n == 2)
            {
                return Instruction.Create(OpCodes.Ldc_I4_2);
            }

            if (n == 3)
            {
                return Instruction.Create(OpCodes.Ldc_I4_3);
            }

            if (n == 4)
            {
                return Instruction.Create(OpCodes.Ldc_I4_4);
            }

            if (n == 5)
            {
                return Instruction.Create(OpCodes.Ldc_I4_5);
            }

            if (n == 6)
            {
                return Instruction.Create(OpCodes.Ldc_I4_6);
            }

            if (n == 7)
            {
                return Instruction.Create(OpCodes.Ldc_I4_7);
            }

            if (n == 8)
            {
                return Instruction.Create(OpCodes.Ldc_I4_8);
            }

            if (n >= -128 && n <= 127)
            {
                return Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)n);
            }

            return Instruction.Create(OpCodes.Ldc_I4, n);
        }

        public static Instruction Stloc_n(int n)
        {
            if (n == 0)
            {
                return Instruction.Create(OpCodes.Stloc_0);
            }

            if (n == 1)
            {
                return Instruction.Create(OpCodes.Stloc_1);
            }

            if (n == 2)
            {
                return Instruction.Create(OpCodes.Stloc_2);
            }

            if (n == 3)
            {
                return Instruction.Create(OpCodes.Stloc_3);
            }

            return Instruction.Create(OpCodes.Stloc_S, n);
        }

        public static Instruction Ldloc_n(int n)
        {
            if (n == 0)
            {
                return Instruction.Create(OpCodes.Ldloc_0);
            }

            if (n == 1)
            {
                return Instruction.Create(OpCodes.Ldloc_1);
            }

            if (n == 2)
            {
                return Instruction.Create(OpCodes.Ldloc_2);
            }

            if (n == 3)
            {
                return Instruction.Create(OpCodes.Ldloc_3);
            }

            return Instruction.Create(OpCodes.Ldloc_S, n);
        }

        public static Instruction Ldarg(ParameterDefinition pdef)
        {
            int n = pdef != null ? pdef.Sequence : 0;
            if (n == 0)
            {
                return Instruction.Create(OpCodes.Ldarg_0);
            }

            if (n == 1)
            {
                return Instruction.Create(OpCodes.Ldarg_1);
            }

            if (n == 2)
            {
                return Instruction.Create(OpCodes.Ldarg_2);
            }

            if (n == 3)
            {
                return Instruction.Create(OpCodes.Ldarg_3);
            }

            return Instruction.Create(OpCodes.Ldarg_S, pdef);
        }

        #endregion Utility

        /// <summary>
        /// Applies this weaver to the specified method
        /// </summary>
        /// <param name="target"></param>
        public void Apply(MethodBody target)
        {
            target.Variables.Clear();
            for (int i = 0; i < Variables.Count; i++)
            {
                target.Variables.Add(Variables[i]);
            }

            target.Instructions.Clear();
            for (int i = 0; i < Instructions.Count; i++)
            {
                target.Instructions.Add(Instructions[i]);
            }

            target.ExceptionHandlers.Clear();
            for (int i = 0; i < ExceptionHandlers.Count; i++)
            {
                target.ExceptionHandlers.Add(ExceptionHandlers[i]);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Instructions.Count; i++)
            {
                sb.AppendLine(Instructions[i].ToString().Replace("\n", "\\n"));
            }

            return sb.ToString();
        }

        #region IEnumerator

        public struct Enumerator : IEnumerator<Instruction>
        {
            private int currentpointer;
            private IList<Instruction> list;

            private Instruction current;
            public Instruction Current => current;

            object IEnumerator.Current => current;

            public Enumerator(IList<Instruction> list)
            {
                this.list = list;
                currentpointer = -1;
                current = null;
            }

            public bool MoveNext()
            {
                currentpointer++;
                if (currentpointer > list.Count)
                {
                    return false;
                }

                current = list[currentpointer];
                return true;
            }

            public void Reset()
            {
                currentpointer = -1;
                current = null;
            }

            public void Dispose()
            {
            }
        }

        public IEnumerator<Instruction> GetEnumerator()
        {
            return new Enumerator(Instructions);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerator
    }
}
