using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace OxidePatcher.Patching
{
    /// <summary>
    /// Represents a set of modifiable instructions
    /// </summary>
    public sealed class ILWeaver : IEnumerable<Instruction>
    {
        /// <summary>
        /// Gets the instructions
        /// </summary>
        public IList<Instruction> Instructions { get; private set; }

        /// <summary>
        /// Gets the local variables
        /// </summary>
        public IList<VariableDefinition> Variables { get; private set; }

        /// <summary>
        /// Gets the exception handlers
        /// </summary>
        public IList<ExceptionHandler> ExceptionHandlers { get; private set; }

        /// <summary>
        /// Gets or sets the current instruction pointer
        /// </summary>
        public int Pointer { get; set; }

        /// <summary>
        /// Gets or sets the module to which this weaver belongs
        /// </summary>
        public ModuleDefinition Module { get; set; }

        /// <summary>
        /// Gets a SHA256 hash of this weaver's instructions
        /// </summary>
        public string Hash
        {
            get
            {
                string str = ToString();
                byte[] raw = Encoding.ASCII.GetBytes(str);
                SHA256 sha = SHA256.Create();
                byte[] hash = sha.ComputeHash(raw);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ILWeaver class with a empty data
        /// </summary>
        public ILWeaver()
        {
            Instructions = new List<Instruction>();
            Variables = new List<VariableDefinition>();
            ExceptionHandlers = new List<ExceptionHandler>();
        }

        /// <summary>
        /// Initializes a new instance of the ILWeaver class with a copy of the specified method's data
        /// </summary>
        /// <param name="body"></param>
        public ILWeaver(MethodBody body)
        {
            Instructions = new List<Instruction>();
            for (int i = 0; i < body.Instructions.Count; i++)
            {
                Instruction existing = body.Instructions[i];
                Instruction newinst = Instruction.Create(OpCodes.Pop); // Dummy instruction
                newinst.OpCode = existing.OpCode;
                newinst.Operand = existing.Operand;
                newinst.Offset = existing.Offset;
                Instructions.Add(newinst);
            }
            for (int i = 0; i < body.Instructions.Count; i++)
            {
                Instruction existing = body.Instructions[i];
                if (existing.Operand is Instruction)
                {
                    Instruction other = existing.Operand as Instruction;
                    int otherindex = body.Instructions.IndexOf(other);
                    Instructions[i].Operand = Instructions[otherindex];
                }
            }
            ExceptionHandlers = new List<ExceptionHandler>();
            for (int i = 0; i < body.ExceptionHandlers.Count; i++)
            {
                ExceptionHandler existing = body.ExceptionHandlers[i];
                ExceptionHandler newexhandler = new ExceptionHandler(ExceptionHandlerType.Catch); // Dummy handler
                newexhandler.HandlerType = existing.HandlerType;
                newexhandler.CatchType = existing.CatchType;
                if (existing.TryStart != null) newexhandler.TryStart = Instructions[body.Instructions.IndexOf(existing.TryStart)];
                if (existing.TryEnd != null) newexhandler.TryEnd = Instructions[body.Instructions.IndexOf(existing.TryEnd)];
                if (existing.FilterStart != null) newexhandler.FilterStart = Instructions[body.Instructions.IndexOf(existing.FilterStart)];
                if (existing.HandlerStart != null) newexhandler.HandlerStart = Instructions[body.Instructions.IndexOf(existing.HandlerStart)];
                if (existing.HandlerEnd != null) newexhandler.HandlerEnd = Instructions[body.Instructions.IndexOf(existing.HandlerEnd)];
                ExceptionHandlers.Add(newexhandler);
            }
            UpdateInstructions();
            Variables = new List<VariableDefinition>(body.Variables);
            Pointer = Instructions.Count - 1;
        }

        /// <summary>
        /// Initializes a new instance of the ILWeaver class with the specified instruction and variable sets
        /// </summary>
        /// <param name="body"></param>
        public ILWeaver(IEnumerable<Instruction> instructions, IEnumerable<VariableDefinition> variables)
        {
            Instructions = new List<Instruction>(instructions);
            Variables = new List<VariableDefinition>(variables);
            Pointer = Instructions.Count - 1;
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
            Pointer++;
            return instruction;
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
                    ins.Previous = null;
                else
                    ins.Previous = Instructions[i - 1];
                if (i == Instructions.Count - 1)
                    ins.Next = null;
                else
                    ins.Next = Instructions[i + 1];
                ins.Offset = curoffset;
                curoffset += ins.GetSize();
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
            VariableDefinition def = new VariableDefinition(name, type);
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
            var inst = Instruction.Create(OpCodes.Starg, parameter);
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
            if (n == 0) return Instruction.Create(OpCodes.Ldc_I4_0);
            if (n == 1) return Instruction.Create(OpCodes.Ldc_I4_1);
            if (n == 2) return Instruction.Create(OpCodes.Ldc_I4_2);
            if (n == 3) return Instruction.Create(OpCodes.Ldc_I4_3);
            if (n == 4) return Instruction.Create(OpCodes.Ldc_I4_4);
            if (n == 5) return Instruction.Create(OpCodes.Ldc_I4_5);
            if (n == 6) return Instruction.Create(OpCodes.Ldc_I4_6);
            if (n == 7) return Instruction.Create(OpCodes.Ldc_I4_7);
            if (n == 8) return Instruction.Create(OpCodes.Ldc_I4_8);
            return Instruction.Create(OpCodes.Ldc_I4_S, n);
        }

        public static Instruction Stloc_n(int n)
        {
            if (n == 0) return Instruction.Create(OpCodes.Stloc_0);
            if (n == 1) return Instruction.Create(OpCodes.Stloc_1);
            if (n == 2) return Instruction.Create(OpCodes.Stloc_2);
            if (n == 3) return Instruction.Create(OpCodes.Stloc_3);
            return Instruction.Create(OpCodes.Stloc_S, n);
        }

        public static Instruction Ldloc_n(int n)
        {
            if (n == 0) return Instruction.Create(OpCodes.Ldloc_0);
            if (n == 1) return Instruction.Create(OpCodes.Ldloc_1);
            if (n == 2) return Instruction.Create(OpCodes.Ldloc_2);
            if (n == 3) return Instruction.Create(OpCodes.Ldloc_3);
            return Instruction.Create(OpCodes.Ldloc_S, n);
        }

        public static Instruction Ldarg(ParameterDefinition pdef)
        {
            var n = pdef != null ? pdef.Sequence : 0;
            if (n == 0) return Instruction.Create(OpCodes.Ldarg_0);
            if (n == 1) return Instruction.Create(OpCodes.Ldarg_1);
            if (n == 2) return Instruction.Create(OpCodes.Ldarg_2);
            if (n == 3) return Instruction.Create(OpCodes.Ldarg_3);
            return Instruction.Create(OpCodes.Ldarg_S, pdef);
        }

        #endregion

        /// <summary>
        /// Applies this weaver to the specified method
        /// </summary>
        /// <param name="target"></param>
        public void Apply(MethodBody target)
        {
            target.Variables.Clear();
            for (int i = 0; i < Variables.Count; i++)
                target.Variables.Add(Variables[i]);
            target.Instructions.Clear();
            for (int i = 0; i < Instructions.Count; i++)
                target.Instructions.Add(Instructions[i]);
            target.ExceptionHandlers.Clear();
            for (int i = 0; i < ExceptionHandlers.Count; i++)
                target.ExceptionHandlers.Add(ExceptionHandlers[i]);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Instructions.Count; i++)
                sb.AppendLine(Instructions[i].ToString());
            return sb.ToString();
        }

        #region IEnumerator

        public struct Enumerator : IEnumerator<Instruction>
        {
            private int currentpointer;
            private IList<Instruction> list;

            private Instruction current;
            public Instruction Current { get { return current; } }

            object System.Collections.IEnumerator.Current
            {
                get { return current; }
            }

            public Enumerator(IList<Instruction> list)
            {
                this.list = list;
                currentpointer = -1;
                current = null;
            }



            public bool MoveNext()
            {
                currentpointer++;
                if (currentpointer > list.Count) return false;
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

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
