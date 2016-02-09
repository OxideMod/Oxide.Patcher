using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using Mono.Cecil;
using Mono.Cecil.Cil;

using OxidePatcher.Hooks;

using Enum = System.Enum;

namespace OxidePatcher
{
    public partial class ModifyForm : Form
    {
        public Modify.InstructionData Instruction;
        public int Index;
        private readonly TextBox textBox;
        private readonly ComboBox comboBox;
        private readonly MethodDefinition method;
        private readonly Modify hook;

        private class ListData
        {
            public object Value { get; set; }
            public string Text { get; set; }
        }

        public ModifyForm(Modify hook, MethodDefinition method)
        {
            InitializeComponent();
            this.hook = hook;
            this.method = method;
            textBox = new TextBox();
            textBox.Dock = DockStyle.Fill;
            comboBox = new ComboBox();
            comboBox.Dock = DockStyle.Fill;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.DisplayMember = "Text";
            comboBox.ValueMember = "Value";
            var ops = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public).OrderBy(f => f.Name);
            foreach (var op in ops)
                opcodes.Items.Add(op.Name.ToLower());
            var optypevalues = Enum.GetValues(typeof (Modify.OpType));
            foreach (var op in optypevalues)
                optypes.Items.Add(op);
            SaveButton.Visible = false;
        }

        public ModifyForm(Modify hook, MethodDefinition method, Modify.InstructionData inst) : this(hook, method)
        {
            Instruction = inst;
            InsertBeforeButton.Visible = false;
            InsertAfterButton.Visible = false;
            SaveButton.Visible = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Instruction != null)
            {
                opcodes.SelectedItem = Instruction.OpCode;
                optypes.SelectedItem = Instruction.OpType;
            }
            else
            {
                opcodes.SelectedIndex = 0;
                optypes.SelectedIndex = 0;
            }
        }

        private void optypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            Control control = null;
            var optype = (Modify.OpType)optypes.SelectedItem;
            switch (optype)
            {
                case Modify.OpType.None:
                    break;
                case Modify.OpType.Byte:
                case Modify.OpType.SByte:
                case Modify.OpType.Int32:
                case Modify.OpType.Int64:
                case Modify.OpType.Single:
                case Modify.OpType.Double:
                case Modify.OpType.String:
                case Modify.OpType.VerbatimString:
                case Modify.OpType.Field:
                case Modify.OpType.Method:
                case Modify.OpType.Generic:
                case Modify.OpType.Type:
                    textBox.Text = Instruction?.Operand?.ToString() ?? string.Empty;
                    control = textBox;
                    break;
                case Modify.OpType.Instruction:
                    var instructions = new List<ListData>();
                    for (var i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        var instruction = method.Body.Instructions[i];
                        instructions.Add(new ListData {Text = $"({i}) {instruction.OpCode} {instruction.Operand}", Value = i});
                    }
                    for (int i = 0; i < hook.Instructions.Count; i++)
                    {
                        var instructionData = hook.Instructions[i];
                        instructions.Add(new ListData { Text = $"({i + 1024}) {instructionData.OpCode} {instructionData.Operand}", Value = i + 1024 });
                    }
                    comboBox.DataSource = instructions;
                    control = comboBox;
                    break;
                case Modify.OpType.Variable:
                    var variables = new List<ListData>();
                    foreach (var variable in method.Body.Variables)
                        variables.Add(new ListData {Text = $"({variable.Index}) ({variable.VariableType.FullName})", Value = variable.Index});
                    comboBox.DataSource = variables;
                    control = comboBox;
                    break;
                case Modify.OpType.Parameter:
                    var parameters = new List<ListData>();
                    foreach (var parameter in method.Parameters)
                        parameters.Add(new ListData {Text = $"({parameter.Index}) {parameter.Name} ({parameter.ParameterType.FullName})", Value = parameter.Index});
                    comboBox.DataSource = parameters;
                    control = comboBox;
                    break;
            }
            var current = tablepanel.GetControlFromPosition(1, 2);
            if (current != control)
            {
                if (current != null) tablepanel.Controls.Remove(current);
                if (control != null) tablepanel.Controls.Add(control, 1, 2);
                operandlabel.Visible = control != null;
            }
            if (control is ComboBox && Instruction?.Operand != null)
                comboBox.SelectedItem = ((List<ListData>) comboBox.DataSource).FirstOrDefault(i => Convert.ToInt32(i.Value) == Convert.ToInt32(Instruction.Operand));
        }

        private void InsertBeforeButton_Click(object sender, EventArgs e)
        {
            Instruction = new Modify.InstructionData();
            if (!UpdateInstruction()) return;
            Index = -1;
            DialogResult = DialogResult.OK;
        }

        private void InsertAfterButton_Click(object sender, EventArgs e)
        {
            Instruction = new Modify.InstructionData();
            if (!UpdateInstruction()) return;
            Index = 1;
            DialogResult = DialogResult.OK;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (UpdateInstruction())
                DialogResult = DialogResult.OK;
        }

        private bool UpdateInstruction()
        {
            Instruction.OpCode = (string) opcodes.SelectedItem;
            Instruction.OpType = (Modify.OpType) optypes.SelectedItem;
            string error = null;
            switch (Instruction.OpType)
            {
                case Modify.OpType.None:
                    Instruction.Operand = null;
                    break;
                case Modify.OpType.Byte:
                case Modify.OpType.SByte:
                case Modify.OpType.Int32:
                case Modify.OpType.Int64:
                case Modify.OpType.Single:
                case Modify.OpType.Double:
                case Modify.OpType.String:
                case Modify.OpType.VerbatimString:
                    Instruction.Operand = textBox.Text;
                    break;
                case Modify.OpType.Instruction:
                case Modify.OpType.Variable:
                case Modify.OpType.Parameter:
                    Instruction.Operand = Convert.ToInt32(((ListData) comboBox.SelectedItem).Value);
                    break;
                case Modify.OpType.Field:
                    var fieldData = textBox.Text.Split('|');
                    if (fieldData.Length < 3)
                    {
                        error = "OpType Field format: AssemblyName|TypeFullName|FieldName";
                        break;
                    }
                    var fieldAssem = GetAssembly(fieldData[0]);
                    if (fieldAssem == null)
                    {
                        error = $"Assembly '{fieldData[0]}' not found";
                        break;
                    }
                    var fieldType = fieldAssem.MainModule.GetType(fieldData[1]);
                    if (fieldType == null)
                    {
                        error = $"Type '{fieldData[1]}' not found";
                        break;
                    }
                    var fieldField = fieldType.Fields.FirstOrDefault(f => f.Name.Equals(fieldData[2]));
                    if (fieldField == null)
                    {
                        error = $"Field '{fieldData[2]}' not found";
                        break;
                    }
                    Instruction.Operand = textBox.Text;
                    break;
                case Modify.OpType.Method:
                    var methodData = textBox.Text.Split('|');
                    if (methodData.Length < 3)
                    {
                        error = "OpType Method format: AssemblyName|TypeFullName|MethodName";
                        break;
                    }
                    var methodAssem = GetAssembly(methodData[0]);
                    if (methodAssem == null)
                    {
                        error = $"Assembly '{methodData[0]}' not found";
                        break;
                    }
                    var methodType = methodAssem.MainModule.GetType(methodData[1]);
                    if (methodType == null)
                    {
                        error = $"Type '{methodData[1]}' not found";
                        break;
                    }
                    MethodDefinition methodMethod;
                    var start = methodData[2].IndexOf('(');
                    var end = methodData[2].IndexOf(')');
                    if (start >= 0 && end >= 0 && start < end)
                    {
                        var name = methodData[2].Substring(0, start).Trim();
                        var methodSig = methodData[2].Substring(start + 1, end - start - 1);
                        var sigData = methodSig.Split(',');
                        var sigTypes = new TypeDefinition[sigData.Length];
                        for (int i = 0; i < sigData.Length; i++)
                        {
                            var s = sigData[i];
                            var sigName = s.Trim();
                            var assem = "mscorlib";
                            if (sigName.Contains('|'))
                            {
                                var split = sigName.Split('|');
                                assem = split[0].Trim();
                                sigName = split[1].Trim();
                            }
                            var sigType = GetAssembly(assem).MainModule.GetType(sigName);
                            if (sigType == null)
                            {
                                error = $"SigType '{sigName}' not found";
                                break;
                            }
                            sigTypes[i] = sigType;
                        }
                        if (error != null) break;
                        methodMethod = null;
                        foreach (var methodDefinition in methodType.Methods)
                        {
                            if (!methodDefinition.Name.Equals(name) || methodDefinition.Parameters.Count != sigTypes.Length) continue;
                            var match = true;
                            for (int i = 0; i < methodDefinition.Parameters.Count; i++)
                            {
                                var parameter = methodDefinition.Parameters[i];
                                if (!parameter.ParameterType.FullName.Equals(sigTypes[i].FullName))
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (!match) continue;
                            methodMethod = methodDefinition;
                            break;
                        }
                    }
                    else
                        methodMethod = methodType.Methods.FirstOrDefault(f => f.Name.Equals(methodData[2]));
                    if (methodMethod == null)
                    {
                        error = $"Method '{methodData[2]}' not found";
                        break;
                    }
                    Instruction.Operand = textBox.Text;
                    break;
                case Modify.OpType.Generic:
                    break;
                case Modify.OpType.Type:
                    var typeData = textBox.Text.Split('|');
                    if (typeData.Length < 2)
                    {
                        error = "OpType Type format: AssemblyName|TypeFullName";
                        break;
                    }
                    var typeAssem = GetAssembly(typeData[0]);
                    if (typeAssem == null)
                    {
                        error = $"Assembly '{typeData[0]}' not found";
                        break;
                    }
                    var typeType = typeAssem.MainModule.GetType(typeData[1]);
                    if (typeType == null)
                    {
                        error = $"Type '{typeData[1]}' not found";
                        break;
                    }
                    Instruction.Operand = textBox.Text;
                    break;
                default:
                    error = $"Unknown OpType '{Instruction.OpType}'";
                    break;
            }
            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, "Instruction creation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private AssemblyDefinition GetAssembly(string assemblyName)
        {
            return PatcherForm.MainForm.LoadAssembly(assemblyName.Replace(".dll", "") + ".dll");
        }
    }
}
