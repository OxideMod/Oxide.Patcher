using Mono.Cecil;
using Mono.Cecil.Cil;
using Oxide.Patcher.Fields;
using Oxide.Patcher.Hooks;
using Oxide.Patcher.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Oxide.Patcher
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
            IOrderedEnumerable<FieldInfo> ops = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public).OrderBy(f => f.Name);
            foreach (FieldInfo op in ops)
            {
                opcodes.Items.Add(op.Name.ToLower());
            }

            opcodes.AutoCompleteSource = AutoCompleteSource.ListItems;
            opcodes.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            Array optypevalues = Enum.GetValues(typeof(Modify.OpType));
            foreach (object op in optypevalues)
            {
                optypes.Items.Add(op);
            }

            SaveButton.Visible = false;
        }

        public ModifyForm(Modify hook, MethodDefinition method, Modify.InstructionData inst) : this(hook, method)
        {
            Instruction = inst;
            InsertBeforeButton.Visible = false;
            InsertAfterButton.Visible = false;
            SaveButton.Visible = true;
        }

        public ModifyForm(Field field, string type) : this(hook: null, method: null)
        {
            Instruction = new Modify.InstructionData { Operand = type, OpType = Modify.OpType.Type };

            opcodelabel.Visible = false;
            opcodes.Visible = false;

            optypes.Items.Clear();
            optypes.Items.Add(Modify.OpType.Type);

            tablepanel.RowStyles[0].Height = 0;

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
            Modify.OpType optype = (Modify.OpType)optypes.SelectedItem;
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
                    List<ListData> instructions = new List<ListData>();
                    IList<Instruction> instructionset = method.Body.Instructions;
                    if (hook.BaseHook != null)
                    {
                        MethodDefinition methoddef = PatcherForm.MainForm.GetMethod(hook.AssemblyName, hook.TypeName, hook.Signature);
                        ILWeaver weaver = new ILWeaver(methoddef.Body) { Module = methoddef.Module };
                        hook.BaseHook.ApplyPatch(methoddef, weaver, PatcherForm.MainForm.OxideAssembly);
                        instructionset = weaver.Instructions;
                    }
                    for (int i = 0; i < instructionset.Count; i++)
                    {
                        Instruction instruction = instructionset[i];
                        instructions.Add(new ListData { Text = $"({i}) {instruction.OpCode} {instruction.Operand}", Value = i });
                    }
                    for (int i = 0; i < hook.Instructions.Count; i++)
                    {
                        Modify.InstructionData instructionData = hook.Instructions[i];
                        instructions.Add(new ListData { Text = $"({i + 1024}) {instructionData.OpCode} {instructionData.Operand}", Value = i + 1024 });
                    }
                    comboBox.DataSource = instructions;
                    control = comboBox;
                    break;

                case Modify.OpType.Variable:
                    List<ListData> variables = new List<ListData>();
                    foreach (VariableDefinition variable in method.Body.Variables)
                    {
                        variables.Add(new ListData { Text = $"({variable.Index}) ({variable.VariableType.FullName})", Value = variable.Index });
                    }

                    comboBox.DataSource = variables;
                    control = comboBox;
                    break;

                case Modify.OpType.Parameter:
                    List<ListData> parameters = new List<ListData>();
                    foreach (ParameterDefinition parameter in method.Parameters)
                    {
                        parameters.Add(new ListData { Text = $"({parameter.Index}) {parameter.Name} ({parameter.ParameterType.FullName})", Value = parameter.Index });
                    }

                    comboBox.DataSource = parameters;
                    control = comboBox;
                    break;
            }
            Control current = tablepanel.GetControlFromPosition(1, 2);
            if (current != control)
            {
                if (current != null)
                {
                    tablepanel.Controls.Remove(current);
                }

                if (control != null)
                {
                    tablepanel.Controls.Add(control, 1, 2);
                }

                operandlabel.Visible = control != null;
            }
            if (control is ComboBox && Instruction?.Operand != null)
            {
                comboBox.SelectedItem = ((List<ListData>)comboBox.DataSource).FirstOrDefault(i => Convert.ToInt32(i.Value) == Convert.ToInt32(Instruction.Operand));
            }
        }

        private void InsertBeforeButton_Click(object sender, EventArgs e)
        {
            Instruction = new Modify.InstructionData();
            if (!UpdateInstruction())
            {
                return;
            }

            Index = -1;
            DialogResult = DialogResult.OK;
        }

        private void InsertAfterButton_Click(object sender, EventArgs e)
        {
            Instruction = new Modify.InstructionData();
            if (!UpdateInstruction())
            {
                return;
            }

            Index = 1;
            DialogResult = DialogResult.OK;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (UpdateInstruction())
            {
                DialogResult = DialogResult.OK;
            }
        }

        private bool UpdateInstruction()
        {
            Instruction.OpCode = (string)opcodes.SelectedItem;
            Instruction.OpType = (Modify.OpType)optypes.SelectedItem;
            string error = null;
            int start;
            int end;
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
                    Instruction.Operand = Convert.ToInt32(((ListData)comboBox.SelectedItem).Value);
                    break;

                case Modify.OpType.Field:
                    string[] fieldData = textBox.Text.Split('|');
                    if (fieldData.Length < 3)
                    {
                        error = "OpType Field format: AssemblyName|TypeFullName|FieldName";
                        break;
                    }
                    AssemblyDefinition fieldAssem = GetAssembly(fieldData[0]);
                    if (fieldAssem == null)
                    {
                        error = $"Assembly '{fieldData[0]}' not found";
                        break;
                    }
                    TypeDefinition fieldType = fieldAssem.MainModule.GetType(fieldData[1]);
                    if (fieldType == null)
                    {
                        error = $"Type '{fieldData[1]}' not found";
                        break;
                    }
                    FieldDefinition fieldField = fieldType.Fields.FirstOrDefault(f => f.Name.Equals(fieldData[2]));
                    if (fieldField == null)
                    {
                        error = $"Field '{fieldData[2]}' not found";
                        break;
                    }
                    Instruction.Operand = textBox.Text;
                    break;

                case Modify.OpType.Method:
                    string[] methodData = textBox.Text.Split('|');
                    if (methodData.Length < 3)
                    {
                        error = "OpType Method format: AssemblyName|TypeFullName|MethodName";
                        break;
                    }
                    if (methodData.Length > 3)
                    {
                        methodData[2] = string.Join("|", methodData.Skip(2).ToArray());
                    }
                    AssemblyDefinition methodAssem = GetAssembly(methodData[0]);
                    if (methodAssem == null)
                    {
                        error = $"Assembly '{methodData[0]}' not found";
                        break;
                    }
                    TypeDefinition methodType = methodAssem.MainModule.GetType(methodData[1]);
                    if (methodType == null)
                    {
                        error = $"Type '{methodData[1]}' not found";
                        break;
                    }
                    MethodDefinition methodMethod;
                    start = methodData[2].IndexOf('(');
                    end = methodData[2].IndexOf(')');
                    if (start >= 0 && end >= 0 && start < end)
                    {
                        string name = Modify.TagsRegex.Replace(methodData[2], string.Empty).Trim();
                        string methodSig = methodData[2].Substring(start + 1, end - start - 1);
                        string[] sigData = methodSig.Split(',');
                        TypeDefinition[] sigTypes = new TypeDefinition[sigData.Length];
                        for (int i = 0; i < sigData.Length; i++)
                        {
                            string s = sigData[i];
                            string sigName = s.Trim();
                            string assem = "mscorlib";
                            if (sigName.Contains('|'))
                            {
                                string[] split = sigName.Split('|');
                                assem = split[0].Trim();
                                sigName = split[1].Trim();
                            }
                            TypeDefinition sigType = GetAssembly(assem).MainModule.GetType(sigName);
                            if (sigType == null)
                            {
                                error = $"SigType '{sigName}' not found";
                                break;
                            }
                            sigTypes[i] = sigType;
                        }
                        if (error != null)
                        {
                            break;
                        }

                        methodMethod = null;
                        foreach (MethodDefinition methodDefinition in methodType.Methods)
                        {
                            if (!methodDefinition.Name.Equals(name) || methodDefinition.Parameters.Count != sigTypes.Length)
                            {
                                continue;
                            }

                            bool match = true;
                            for (int i = 0; i < methodDefinition.Parameters.Count; i++)
                            {
                                ParameterDefinition parameter = methodDefinition.Parameters[i];
                                if (!parameter.ParameterType.FullName.Equals(sigTypes[i].FullName))
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (!match)
                            {
                                continue;
                            }

                            methodMethod = methodDefinition;
                            break;
                        }
                    }
                    else
                    {
                        string methodName = methodData[2];
                        int position = methodName.IndexOf('[');
                        if (position > 0)
                        {
                            methodName = methodName.Substring(0, position);
                        }

                        methodMethod = methodType.Methods.FirstOrDefault(f => f.Name.Equals(methodName) && (position <= 0 || f.HasGenericParameters));
                    }
                    if (methodMethod == null)
                    {
                        error = $"Method '{methodData[2]}' not found";
                        break;
                    }
                    if (methodMethod.HasGenericParameters)
                    {
                        start = methodData[2].IndexOf('[');
                        end = methodData[2].IndexOf(']');
                        if (start >= 0 && end >= 0 && start < end)
                        {
                            string methodG = methodData[2].Substring(start + 1, end - start - 1);
                            string[] genData = methodG.Split(',');
                            TypeDefinition[] genTypes = new TypeDefinition[genData.Length];
                            for (int i = 0; i < genData.Length; i++)
                            {
                                string s = genData[i];
                                string genName = s.Trim();
                                string assem = "mscorlib";
                                if (genName.Contains('|'))
                                {
                                    string[] split = genName.Split('|');
                                    assem = split[0].Trim();
                                    genName = split[1].Trim();
                                }
                                TypeDefinition genType = GetAssembly(assem).MainModule.GetType(genName);
                                if (genType == null)
                                {
                                    error = $"GenericType '{genName}' not found";
                                    break;
                                }
                                genTypes[i] = genType;
                            }
                            if (error != null)
                            {
                                break;
                            }
                        }
                    }
                    Instruction.Operand = textBox.Text;
                    break;

                case Modify.OpType.Generic:
                    break;

                case Modify.OpType.Type:
                    string[] typeData = textBox.Text.Split('|');
                    if (typeData.Length < 2)
                    {
                        error = "OpType Type format: AssemblyName|TypeFullName";
                        break;
                    }
                    AssemblyDefinition typeAssem = GetAssembly(typeData[0]);
                    if (typeAssem == null)
                    {
                        error = $"Assembly '{typeData[0]}' not found";
                        break;
                    }
                    TypeDefinition typeType = typeAssem.MainModule.GetType(Modify.TagsRegex.Replace(typeData[1], string.Empty).Trim());
                    if (typeType == null)
                    {
                        error = $"Type '{typeData[1]}' not found";
                        break;
                    }
                    start = typeData[1].IndexOf('[');
                    end = typeData[1].IndexOf(']');
                    if (start >= 0 && end >= 0 && start < end)
                    {
                        string typeG = typeData[1].Substring(start + 1, end - start - 1);
                        string[] genData = typeG.Split(',');
                        TypeDefinition[] genTypes = new TypeDefinition[genData.Length];
                        for (int i = 0; i < genData.Length; i++)
                        {
                            string s = genData[i];
                            string genName = s.Trim();
                            string assem = "mscorlib";
                            if (genName.Contains('|'))
                            {
                                string[] split = genName.Split('|');
                                assem = split[0].Trim();
                                genName = split[1].Trim();
                            }
                            TypeDefinition genType = GetAssembly(assem).MainModule.GetType(genName);
                            if (genType == null)
                            {
                                error = $"GenericType '{genName}' not found";
                                break;
                            }
                            genTypes[i] = genType;
                        }
                        if (error != null)
                        {
                            break;
                        }
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

        private void opcodes_Leave(object sender, EventArgs e)
        {
            if (!opcodes.Items.Contains(opcodes.Text))
            {
                MessageBox.Show("Unknown OpCode specified, please select a valid value!", "Invalid OpCode", MessageBoxButtons.OK, MessageBoxIcon.Error);
                opcodes.Focus();
            }
        }
    }
}
