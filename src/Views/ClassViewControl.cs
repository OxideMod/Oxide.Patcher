using Mono.Cecil;
using Oxide.Patcher.Fields;
using Oxide.Patcher.Modifiers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Oxide.Patcher.Common;

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

        private Control _currentView;

        private Modifier _modifierView;

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

            bool modifierFound = false;
            foreach (Manifest manifest in MainForm.CurrentProject.Manifests)
            {
                if (TypeDef == null)
                {
                    continue;
                }

                foreach (Modifier modifier in manifest.Modifiers)
                {
                    if (modifier.Signature.Equals(Utility.GetModifierSignature(TypeDef)) && modifier.TypeName == TypeDef.FullName)
                    {
                        modifierFound = true;
                        _modifierView = modifier;
                        break;
                    }
                }
            }

            editbutton.Enabled = !modifierFound;
            gotoeditbutton.Enabled = modifierFound;
        }

        private void PopulateDetails()
        {
            try
            {
                typenametextbox.Text = TypeDef.FullName;
                StringBuilder sb = new StringBuilder();

                sb.Append(TypeDef.IsPublic ? "public " : "private ");

                if (TypeDef.IsSealed)
                {
                    sb.Append("sealed ");
                }

                sb.Append("class ");
                sb.Append(TypeDef.Name);

                if (Utility.TransformType(TypeDef.BaseType.Name) != "object")
                {
                    sb.Append($" : {TypeDef.BaseType.Name} ");
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
            TreeNode staticMembersNode = new TreeNode("Static Members");
            TreeNode staticFieldsNode = new TreeNode("Fields");
            TreeNode staticPropertiesNode = new TreeNode("Properties");
            TreeNode staticMethodsNode = new TreeNode("Methods");

            TreeNode instanceMembersNode = new TreeNode("Instance Members");
            TreeNode instanceFieldsNode = new TreeNode("Fields");
            TreeNode instancePropertiesNode = new TreeNode("Properties");
            TreeNode instanceMethodsNode = new TreeNode("Methods");

            // Get all members and sort
            FieldDefinition[] fieldDefs = TypeDef.Fields.ToArray();
            Array.Sort(fieldDefs, (a, b) => Comparer<string>.Default.Compare(a.Name, b.Name));

            PropertyDefinition[] propDefs = TypeDef.Properties.ToArray();
            Array.Sort(propDefs, (a, b) => Comparer<string>.Default.Compare(a.Name, b.Name));

            MethodDefinition[] methodDefs = TypeDef.Methods.ToArray();
            Array.Sort(methodDefs, (a, b) => Comparer<string>.Default.Compare(a.Name, b.Name));

            // Add fields
            foreach (FieldDefinition fieldDef in fieldDefs)
            {
                string qualifier = fieldDef.IsPublic ? "public" : fieldDef.IsPrivate ? "private" : "protected";
                string name = $"{qualifier} {Utility.TransformType(fieldDef.FieldType.Name)} {fieldDef.Name}";
                string icon = SelectIcon(fieldDef);

                TreeNode node = new TreeNode(name)
                {
                    ImageKey = icon,
                    SelectedImageKey = icon,
                    Tag = fieldDef
                };

                if (fieldDef.IsStatic)
                {
                    staticFieldsNode.Nodes.Add(node);
                }
                else
                {
                    instanceFieldsNode.Nodes.Add(node);
                }
            }

            // Add properties
            HashSet<MethodDefinition> ignoreMethods = new HashSet<MethodDefinition>();
            foreach (PropertyDefinition propertyDef in propDefs)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("(");

                MethodDefinition propertyGetMethod = propertyDef.GetMethod;
                if (propertyGetMethod != null)
                {
                    ignoreMethods.Add(propertyGetMethod);

                    sb.Append(propertyGetMethod.IsPrivate ? "private " : propertyGetMethod.IsPublic ? "public " : "protected ");
                    sb.Append("getter, ");
                }

                MethodDefinition propertySetMethod = propertyDef.SetMethod;
                if (propertySetMethod != null)
                {
                    ignoreMethods.Add(propertySetMethod);

                    sb.Append(propertySetMethod.IsPrivate ? "private " : propertySetMethod.IsPublic ? "public " : "protected ");
                    sb.Append("setter");
                }

                sb.Append(")");

                string name = $"{Utility.TransformType(propertyDef.PropertyType.Name)} {propertyDef.Name} {sb}";
                string icon = SelectIcon(propertyDef);

                TreeNode node = new TreeNode(name)
                {
                    ImageKey = icon,
                    SelectedImageKey = icon,
                    Tag = propertyDef
                };

                //If static
                if ((propertyGetMethod?.IsStatic ?? false) || (propertySetMethod?.IsStatic ?? false))
                {
                    staticPropertiesNode.Nodes.Add(node);
                }
                else
                {
                    instancePropertiesNode.Nodes.Add(node);
                }
            }

            // Add methods
            foreach (MethodDefinition method in methodDefs)
            {
                if (ignoreMethods.Contains(method))
                {
                    continue;
                }

                string name = Utility.GetMethodDeclaration(method);
                string icon = SelectIcon(method);

                TreeNode node = new TreeNode(name)
                {
                    ImageKey = icon,
                    SelectedImageKey = icon,
                    Tag = method
                };

                if (method.IsStatic)
                {
                    staticMethodsNode.Nodes.Add(node);
                }
                else
                {
                    instanceMethodsNode.Nodes.Add(node);
                }
            }

            // Add all nodes
            if (instanceFieldsNode.Nodes.Count > 0)
            {
                instanceMembersNode.Nodes.Add(instanceFieldsNode);
            }

            if (instancePropertiesNode.Nodes.Count > 0)
            {
                instanceMembersNode.Nodes.Add(instancePropertiesNode);
            }

            if (instanceMethodsNode.Nodes.Count > 0)
            {
                instanceMembersNode.Nodes.Add(instanceMethodsNode);
                instanceMethodsNode.Expand();
            }

            if (instanceMembersNode.Nodes.Count > 0)
            {
                objectview.Nodes.Add(instanceMembersNode);
                instanceMembersNode.Expand();
            }

            if (staticFieldsNode.Nodes.Count > 0)
            {
                staticMembersNode.Nodes.Add(staticFieldsNode);
            }

            if (staticPropertiesNode.Nodes.Count > 0)
            {
                staticMembersNode.Nodes.Add(staticPropertiesNode);
            }

            if (staticMethodsNode.Nodes.Count > 0)
            {
                staticMembersNode.Nodes.Add(staticMethodsNode);
                staticMethodsNode.Expand();
            }

            if (staticMembersNode.Nodes.Count > 0)
            {
                objectview.Nodes.Add(staticMembersNode);
                staticMembersNode.Expand();
            }
        }

        private string SelectIcon(FieldDefinition field)
        {
            return field.IsPrivate ? "Field-Private_545.png" : field.IsPublic ? "FieldIcon.png" : "Field-Protected_544.png";
        }

        private string SelectIcon(PropertyDefinition prop)
        {
            MethodDefinition getMethod = prop.GetMethod;
            MethodDefinition setMethod = prop.SetMethod;
            if (getMethod == null && setMethod == null)
            {
                return "Property_501.png";
            }

            if (getMethod?.IsPublic == true || setMethod?.IsPublic == true)
            {
                return "Property_501.png";
            }

            return "Property-Private_505.png";
        }

        private string SelectIcon(MethodDefinition method)
        {
            return method.IsPrivate ? "Method-Private_640.png" : method.IsPublic ? "Method_636.png" : "Method-Protected_639.png";
        }

        private void objectview_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_currentView != null)
            {
                splitter.Panel2.Controls.Remove(_currentView);
                _currentView.Dispose();
                _currentView = null;
            }

            TreeNode selected = e.Node;
            if (selected == null)
            {
                return;
            }

            switch (selected.Tag)
            {
                case MethodDefinition methodDef:
                {
                    MethodViewControl methodView = new MethodViewControl()
                    {
                        Dock = DockStyle.Fill,
                        MethodDef = methodDef,
                        MainForm = MainForm
                    };

                    splitter.Panel2.Controls.Add(methodView);
                    _currentView = methodView;
                    break;
                }

                case PropertyDefinition _:
                case FieldDefinition _:
                {
                    FieldAndPropertyViewControl fieldPropertyView = new FieldAndPropertyViewControl()
                    {
                        Dock = DockStyle.Fill,
                        PropertyDef = selected.Tag as PropertyDefinition,
                        FieldDef = selected.Tag as FieldDefinition,
                        MainForm = MainForm
                    };

                    splitter.Panel2.Controls.Add(fieldPropertyView);
                    _currentView = fieldPropertyView;
                    break;
                }
            }
        }

        private void editbutton_Click(object sender, EventArgs e)
        {
            Modifier modifier = new Modifier(TypeDef, MainForm.AssemblyLoader.rassemblydict[TypeDef.Module.Assembly]);

            MainForm.AddModifier(modifier);
            MainForm.GotoModifier(modifier);

            _modifierView = modifier;
            editbutton.Enabled = false;
            gotoeditbutton.Enabled = true;
        }

        private void gotoeditbutton_Click(object sender, EventArgs e)
        {
            MainForm.GotoModifier(_modifierView);
        }

        private void injectfield_Click(object sender, EventArgs e)
        {
            Field field = new Field(TypeDef, MainForm.AssemblyLoader.rassemblydict[TypeDef.Module.Assembly]);

            MainForm.AddField(field);
            MainForm.GotoField(field);
        }
    }
}
