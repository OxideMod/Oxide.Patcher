using System;
using System.Collections.Generic;
using System.IO;
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

        public override bool ApplyPatch(MethodDefinition original, ILWeaver weaver, AssemblyDefinition oxidemodule, Patcher patcher = null)
        {
            var insts = new List<Instruction>();
            foreach (var instructionData in Instructions)
            {
                var instruction = CreateInstruction(original, weaver, instructionData, insts, patcher);
                if (instruction == null) return false;
                insts.Add(instruction);
            }
            // Start injecting where requested
            weaver.Pointer = InjectionIndex;

            if (!weaver.RemoveAfter(RemoveCount))
            {
                ShowMsg(string.Format("The remove count specified for {0} is invalid!", Name), "Invalid Remove Count", patcher);
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
                ShowMsg(string.Format("The injection index specified for {0} is invalid!", Name), "Invalid Index", patcher);
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

        private Instruction CreateInstruction(MethodDefinition method, ILWeaver weaver, InstructionData instructionData, List<Instruction> insts, Patcher patcher)
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
                    var fieldType = GetType(fieldData[0], fieldData[1], patcher);
                    if (fieldType == null) return null;
                    var fieldField = fieldType.Fields.FirstOrDefault(f => f.Name.Equals(fieldData[2]));
                    if (fieldField == null)
                    {
                        ShowMsg(string.Format("The Field '{0}' for '{1}' could not be found!", fieldData[2], Name), "Missing Field", patcher);
                        return null;
                    }
                    Instruction = Instruction.Create(opcode, method.Module.Import(fieldField));
                    break;
                case OpType.Method:
                    var methodData = Convert.ToString(instructionData.Operand).Split('|');
                    var methodType = GetType(methodData[0], methodData[1], patcher);
                    if (methodType == null) return null;
                    var methodMethod = methodType.Methods.FirstOrDefault(f => f.Name.Equals(methodData[2]));
                    if (methodMethod == null)
                    {
                        ShowMsg(string.Format("The Method '{0}' for '{1}' could not be found!", methodData[2], Name), "Missing Method", patcher);
                        return null;
                    }
                    Instruction = Instruction.Create(opcode, method.Module.Import(methodMethod));
                    break;
                case OpType.Generic:
                    break;
                case OpType.Type:
                    var typeData = Convert.ToString(instructionData.Operand).Split('|');
                    var typeType = GetType(typeData[0], typeData[1], patcher);
                    if (typeType == null) return null;
                    Instruction = Instruction.Create(opcode, method.Module.Import(typeType));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return Instruction;
        }

        private TypeDefinition GetType(string assemblyName, string typeName, Patcher patcher)
        {
            var targetDir = patcher != null ? patcher.PatchProject.TargetDirectory : PatcherForm.MainForm.CurrentProject.TargetDirectory;
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(targetDir);
            string filename = Path.Combine(targetDir, assemblyName.Replace(".dll", "") + ".dll");
            var assem = AssemblyDefinition.ReadAssembly(filename, new ReaderParameters { AssemblyResolver = resolver });
            if (assem == null)
            {
                ShowMsg(string.Format("The Assembly '{0}' for '{1}' could not be found!", assemblyName, Name), "Missing Assembly", patcher);
                return null;
            }
            var type = assem.MainModule.GetType(typeName);
            if (type == null)
            {
                ShowMsg(string.Format("The Type '{0}' for '{1}' could not be found!", typeName, Name), "Missing Type", patcher);
                return null;
            }
            return type;
        }
    }
}
