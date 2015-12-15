using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Mono.Cecil;
using Mono.Cecil.Cil;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using OxidePatcher.Patching;
using OxidePatcher.Views;

namespace OxidePatcher.Hooks
{
    [HookType("Modify")]
    public class Modify : Hook
    {
        public enum OpType { None, Byte, SByte, Int32, Int64, Single, Double, String, VerbatimString, Instruction, Variable, Parameter, Field, Method, Generic, Type }

        public class InstructionData
        {
            public string OpCode { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public OpType OpType { get; set; }
            public object Operand { get; set; }
            public override string ToString()
            {
                return $"{OpCode} {Operand}";
            }
        }
        /// <summary>
        /// Gets or sets the instruction index to inject the hook call at
        /// </summary>
        public int InjectionIndex { get; set; }
        public int RemoveCount { get; set; }
        public List<InstructionData> Instructions { get; set; } = new List<InstructionData>();

        private Dictionary<string, OpCode> opCodes = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public).ToDictionary(f => f.Name.ToLower(), f => (OpCode)f.GetValue(null));

        public override bool ApplyPatch(MethodDefinition original, ILWeaver weaver, AssemblyDefinition oxidemodule, bool console)
        {
            var insts = new List<Instruction>();
            foreach (var instructionData in Instructions)
            {
                var instruction = CreateInstruction(original, weaver, instructionData, insts, console);
                if (instruction == null) return false;
                insts.Add(instruction);
            }
            // Start injecting where requested
            weaver.Pointer = InjectionIndex;

            if (!weaver.RemoveAfter(RemoveCount))
            {
                if (!console) MessageBox.Show(string.Format("The remove count specified for {0} is invalid!", Name), "Invalid Remove Count", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (Instructions.Count == 0) return true;

            // Get the existing instruction we're going to inject behind
            Instruction existing;
            try
            {
                existing = weaver.Instructions[weaver.Pointer];
            }
            catch (ArgumentOutOfRangeException)
            {
                if (!console) MessageBox.Show(string.Format("The injection index specified for {0} is invalid!", Name), "Invalid Index", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            foreach (var inst in insts)
                weaver.Add(inst);
            // Find all instructions which pointed to the existing and redirect them
            for (int i = 0; i < weaver.Instructions.Count; i++)
            {
                Instruction ins = weaver.Instructions[i];
                if (ins.Operand != null && ins.Operand.Equals(existing))
                {
                    // Check if the instruction lies within our injection range
                    // If it does, it's an instruction we just injected so we don't want to edit it
                    if (i < InjectionIndex || i > weaver.Pointer)
                        ins.Operand = insts[0];
                }
            }
            return true;
        }

        public override HookSettingsControl CreateSettingsView()
        {
            return new ModifyHookSettingsControl { Hook = this };
        }

        private Instruction CreateInstruction(MethodDefinition method, ILWeaver weaver, InstructionData instructionData, List<Instruction> insts, bool console)
        {
            var opcode = opCodes[instructionData.OpCode];
            var optype = instructionData.OpType;
            Instruction Instruction = null;
            switch (optype)
            {
                case OpType.None:
                    Instruction = Instruction.Create(opcode);
                    break;
                case OpType.Byte:
                    Instruction = Instruction.Create(opcode, Convert.ToByte(instructionData.Operand));
                    break;
                case OpType.SByte:
                    Instruction = Instruction.Create(opcode, Convert.ToSByte(instructionData.Operand));
                    break;
                case OpType.Int32:
                    Instruction = Instruction.Create(opcode, Convert.ToInt32(instructionData.Operand));
                    break;
                case OpType.Int64:
                    Instruction = Instruction.Create(opcode, Convert.ToInt64(instructionData.Operand));
                    break;
                case OpType.Single:
                    Instruction = Instruction.Create(opcode, Convert.ToSingle(instructionData.Operand));
                    break;
                case OpType.Double:
                    Instruction = Instruction.Create(opcode, Convert.ToDouble(instructionData.Operand));
                    break;
                case OpType.String:
                    Instruction = Instruction.Create(opcode, Convert.ToString(instructionData.Operand));
                    break;
                case OpType.VerbatimString:
                    Instruction = Instruction.Create(opcode, Regex.Unescape(Convert.ToString(instructionData.Operand)));
                    break;
                case OpType.Instruction:
                    var index = Convert.ToInt32(instructionData.Operand);
                    Instruction = Instruction.Create(opcode, index < 1024 ? weaver.Instructions[index] : insts[index - 1024]);
                    break;
                case OpType.Variable:
                    Instruction = Instruction.Create(opcode, method.Body.Variables[Convert.ToInt32(instructionData.Operand)]);
                    break;
                case OpType.Parameter:
                    Instruction = Instruction.Create(opcode, method.Parameters[Convert.ToInt32(instructionData.Operand)]);
                    break;
                case OpType.Field:
                    var fieldData = Convert.ToString(instructionData.Operand).Split('|');
                    var fieldType = GetType(fieldData[0], fieldData[1], console);
                    if (fieldType == null) return null;
                    var fieldField = fieldType.Fields.FirstOrDefault(f => f.Name.Equals(fieldData[2]));
                    if (fieldField == null)
                    {
                        if (!console) MessageBox.Show(string.Format("The Field '{0}' for '{1}' could not be found!", fieldData[2], Name), "Missing Field", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                    Instruction = Instruction.Create(opcode, method.Module.Import(fieldField));
                    break;
                case OpType.Method:
                    var methodData = Convert.ToString(instructionData.Operand).Split('|');
                    var methodType = GetType(methodData[0], methodData[1], console);
                    if (methodType == null) return null;
                    var methodMethod = methodType.Methods.FirstOrDefault(f => f.Name.Equals(methodData[2]));
                    if (methodMethod == null)
                    {
                        if (!console) MessageBox.Show(string.Format("The Method '{0}' for '{1}' could not be found!", methodData[2], Name), "Missing Method", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                    Instruction = Instruction.Create(opcode, method.Module.Import(methodMethod));
                    break;
                case OpType.Generic:
                    break;
                case OpType.Type:
                    var typeData = Convert.ToString(instructionData.Operand).Split('|');
                    var typeType = GetType(typeData[0], typeData[1], console);
                    if (typeType == null) return null;
                    Instruction = Instruction.Create(opcode, method.Module.Import(typeType));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return Instruction;
        }

        private TypeDefinition GetType(string assemblyName, string typeName, bool console)
        {
            var assem = PatcherForm.MainForm.LoadAssembly(assemblyName.Replace(".dll", "") + ".dll");
            if (assem == null)
            {
                if (!console) MessageBox.Show(string.Format("The Assembly '{0}' for '{1}' could not be found!", assemblyName, Name), "Missing Assembly", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            var type = assem.MainModule.GetType(typeName);
            if (type == null)
            {
                if (!console) MessageBox.Show(string.Format("The Type '{0}' for '{1}' could not be found!", typeName, Name), "Missing Type", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return type;
        }
    }
}
