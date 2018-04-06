using Mono.Cecil;
using Oxide.Patcher.Fields;
using Oxide.Patcher.Modifiers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Oxide.Patcher.Views
{
    public partial class ClassViewControl : UserControl
    {
        /// <summary>
        /// Gets or sets the type definition to use
        /// </summary>
        public TypeDefinition TypeDef { get; set; }

        /// <summary>
        /// Gets or sets the main patcher form
        /// </summary>
        public PatcherForm MainForm { get; set; }

        private Control currentview;

        private Modifier modifierview;

        public ClassViewControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            // Populate the details
            PopulateDetails();

            // Populate the tree
            PopulateTree();

            bool modifierfound = false;
            foreach (Manifest manifest in MainForm.CurrentProject.Manifests)
            {
                if (TypeDef != null)
                {
                    foreach (Modifier modifier in manifest.Modifiers)
                    {
                        if (modifier.Signature.Equals(Utility.GetModifierSignature(TypeDef)) && modifier.TypeName == TypeDef.FullName)
                        {
                            modifierfound = true;
                            modifierview = modifier;
                            break;
                        }
                    }
                }
            }
            if (modifierfound)
            {
                editbutton.Enabled = false;
                gotoeditbutton.Enabled = true;
            }
            else
            {
                editbutton.Enabled = true;
                gotoeditbutton.Enabled = false;
            }
        }

        private void PopulateDetails()
        {
            try
            {
                typenametextbox.Text = TypeDef.FullName;
                StringBuilder sb = new StringBuilder();
                if (TypeDef.IsPublic)
                {
                    sb.Append("public ");
                }
                else
                {
                    sb.Append("private ");
                }

                if (TypeDef.IsSealed)
                {
                    sb.Append("sealed ");
                }

                sb.Append("class ");
                sb.Append(TypeDef.Name);
                if (Utility.TransformType(TypeDef.BaseType.Name) != "object")
                {
                    sb.AppendFormat(" : {0} ", TypeDef.BaseType.Name);
                }

                declarationtextbox.Text = sb.ToString();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(PatcherForm.MainForm, "Error loading details for a class. It may be empty.",
                    "Null Reference Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateTree()
        {
            // Clear tree
            objectview.Nodes.Clear();

            // Create root nodes
            TreeNode staticnode = new TreeNode("Static Members");
            TreeNode staticnodeFields = new TreeNode("Fields");
            TreeNode staticnodeProperties = new TreeNode("Properties");
            TreeNode staticnodeMethods = new TreeNode("Methods");
            TreeNode instancenode = new TreeNode("Instance Members");
            TreeNode instancenodeFields = new TreeNode("Fields");
            TreeNode instancenodeProperties = new TreeNode("Properties");
            TreeNode instancenodeMethods = new TreeNode("Methods");

            // Get all members and sort
            FieldDefinition[] fielddefs = TypeDef.Fields.ToArray();
            Array.Sort(fielddefs, (a, b) => Comparer<string>.Default.Compare(a.Name, b.Name));
            PropertyDefinition[] propdefs = TypeDef.Properties.ToArray();
            Array.Sort(propdefs, (a, b) => Comparer<string>.Default.Compare(a.Name, b.Name));
            MethodDefinition[] methoddefs = TypeDef.Methods.ToArray();
            Array.Sort(methoddefs, (a, b) => Comparer<string>.Default.Compare(a.Name, b.Name));

            // Add fields
            for (int i = 0; i < fielddefs.Length; i++)
            {
                FieldDefinition field = fielddefs[i];
                string qualifier = field.IsPublic ? "public" : field.IsPrivate ? "private" : "protected";
                string name = $"{qualifier} {Utility.TransformType(field.FieldType.Name)} {field.Name}";
                TreeNode node = new TreeNode(name);
                string icon = SelectIcon(field);
                node.ImageKey = icon;
                node.SelectedImageKey = icon;
                node.Tag = field;
                if (field.IsStatic)
                {
                    staticnodeFields.Nodes.Add(node);
                }
                else
                {
                    instancenodeFields.Nodes.Add(node);
                }
            }

            // Add properties
            HashSet<MethodDefinition> ignoremethods = new HashSet<MethodDefinition>();
            for (int i = 0; i < propdefs.Length; i++)
            {
                PropertyDefinition prop = propdefs[i];
                StringBuilder sb = new StringBuilder();
                sb.Append("(");
                if (prop.GetMethod != null)
                {
                    ignoremethods.Add(prop.GetMethod);
                    if (prop.GetMethod.IsPrivate)
                    {
                        sb.Append("private ");
                    }
                    else if (prop.GetMethod.IsPublic)
                    {
                        sb.Append("public ");
                    }
                    else
                    {
                        sb.Append("protected ");
                    }

                    sb.Append("getter, ");
                }
                if (prop.SetMethod != null)
                {
                    ignoremethods.Add(prop.SetMethod);
                    if (prop.SetMethod.IsPrivate)
                    {
                        sb.Append("private ");
                    }
                    else if (prop.SetMethod.IsPublic)
                    {
                        sb.Append("public ");
                    }
                    else
                    {
                        sb.Append("protected ");
                    }

                    sb.Append("setter");
                }
                sb.Append(")");
                string name = $"{Utility.TransformType(prop.PropertyType.Name)} {prop.Name} {sb}";
                TreeNode node = new TreeNode(name);
                string icon = SelectIcon(prop);
                node.ImageKey = icon;
                node.SelectedImageKey = icon;
                node.Tag = prop;
                bool propstatic = (prop.GetMethod != null ? prop.GetMethod.IsStatic : false) || (prop.SetMethod != null ? prop.SetMethod.IsStatic : false);
                if (propstatic)
                {
                    staticnodeProperties.Nodes.Add(node);
                }
                else
                {
                    instancenodeProperties.Nodes.Add(node);
                }
            }

            // Add methods
            for (int i = 0; i < methoddefs.Length; i++)
            {
                MethodDefinition method = methoddefs[i];
                if (!ignoremethods.Contains(method))
                {
                    string name = Utility.GetMethodDeclaration(method);
                    TreeNode node = new TreeNode(name);
                    string icon = SelectIcon(method);
                    node.ImageKey = icon;
                    node.SelectedImageKey = icon;
                    node.Tag = method;
                    if (method.IsStatic)
                    {
                        staticnodeMethods.Nodes.Add(node);
                    }
                    else
                    {
                        instancenodeMethods.Nodes.Add(node);
                    }
                }
            }

            // Add all nodes
            if (instancenodeFields.Nodes.Count > 0)
            {
                instancenode.Nodes.Add(instancenodeFields);
            }

            if (instancenodeProperties.Nodes.Count > 0)
            {
                instancenode.Nodes.Add(instancenodeProperties);
            }

            if (instancenodeMethods.Nodes.Count > 0)
            {
                instancenode.Nodes.Add(instancenodeMethods);
            }

            if (instancenode.Nodes.Count > 0)
            {
                objectview.Nodes.Add(instancenode);
            }

            if (staticnodeFields.Nodes.Count > 0)
            {
                staticnode.Nodes.Add(staticnodeFields);
            }

            if (staticnodeProperties.Nodes.Count > 0)
            {
                staticnode.Nodes.Add(staticnodeProperties);
            }

            if (staticnodeMethods.Nodes.Count > 0)
            {
                staticnode.Nodes.Add(staticnodeMethods);
            }

            if (staticnode.Nodes.Count > 0)
            {
                objectview.Nodes.Add(staticnode);
            }

            instancenode.Expand();
            instancenodeMethods.Expand();
            staticnode.Expand();
            staticnodeMethods.Expand();
        }

        private string SelectIcon(FieldDefinition field)
        {
            if (field.IsPrivate)
            {
                return "Field-Private_545.png";
            }

            if (field.IsPublic)
            {
                return "FieldIcon.png";
            }

            return "Field-Protected_544.png";
        }

        private string SelectIcon(PropertyDefinition prop)
        {
            MethodDefinition getmethod = prop.GetMethod;
            MethodDefinition setmethod = prop.SetMethod;
            if (getmethod == null && setmethod == null)
            {
                return "Property_501.png";
            }

            if (getmethod != null && getmethod.IsPublic || setmethod != null && setmethod.IsPublic)
            {
                return "Property_501.png";
            }

            return "Property-Private_505.png";
        }

        private string SelectIcon(MethodDefinition method)
        {
            if (method.IsPrivate)
            {
                return "Method-Private_640.png";
            }

            if (method.IsPublic)
            {
                return "Method_636.png";
            }

            return "Method-Protected_639.png";
        }

        private void objectview_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (currentview != null)
            {
                splitter.Panel2.Controls.Remove(currentview);
                currentview.Dispose();
                currentview = null;
            }

            TreeNode selected = e.Node;
            if (selected == null)
            {
                return;
            }

            if (selected.Tag is MethodDefinition)
            {
                MethodViewControl methodview = new MethodViewControl();
                methodview.Dock = DockStyle.Fill;
                methodview.MethodDef = selected.Tag as MethodDefinition;
                methodview.MainForm = MainForm;
                splitter.Panel2.Controls.Add(methodview);
                currentview = methodview;
            }

            if (selected.Tag is PropertyDefinition || selected.Tag is FieldDefinition)
            {
                FieldAndPropertyViewControl fieldpropertyview = new FieldAndPropertyViewControl();
                fieldpropertyview.Dock = DockStyle.Fill;
                fieldpropertyview.PropertyDef = selected.Tag as PropertyDefinition;
                fieldpropertyview.FieldDef = selected.Tag as FieldDefinition;
                fieldpropertyview.MainForm = MainForm;
                splitter.Panel2.Controls.Add(fieldpropertyview);
                currentview = fieldpropertyview;
            }
        }

        private void editbutton_Click(object sender, EventArgs e)
        {
            Modifier modifier = new Modifier(TypeDef, MainForm.rassemblydict[TypeDef.Module.Assembly]);

            MainForm.AddModifier(modifier);
            MainForm.GotoModifier(modifier);

            modifierview = modifier;
            editbutton.Enabled = false;
            gotoeditbutton.Enabled = true;
        }

        private void gotoeditbutton_Click(object sender, EventArgs e)
        {
            MainForm.GotoModifier(modifierview);
        }

        private void injectfield_Click(object sender, EventArgs e)
        {
            Field field = new Field(TypeDef, MainForm.rassemblydict[TypeDef.Module.Assembly]);

            MainForm.AddField(field);
            MainForm.GotoField(field);
        }
    }
}
