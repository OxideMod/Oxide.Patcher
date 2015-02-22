using System;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

using Mono.Cecil;

namespace OxidePatcher.Views
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
        }

        private void PopulateDetails()
        {
            try
            {
                typenametextbox.Text = TypeDef.FullName;
                StringBuilder sb = new StringBuilder();
                if (TypeDef.IsPublic)
                    sb.Append("public ");
                else
                    sb.Append("private ");
                if (TypeDef.IsSealed)
                    sb.Append("sealed ");
                sb.Append("class ");
                sb.Append(TypeDef.Name);
                if (Utility.TransformType(TypeDef.BaseType.Name) != "object")
                    sb.AppendFormat(" : {0} ", TypeDef.BaseType.Name);
                declarationtextbox.Text = sb.ToString();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(PatcherForm.MainForm, "Error loading details for a class. It may be empty.", 
                    "Null Reference Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void PopulateTree()
        {
            // Clear tree
            objectview.Nodes.Clear();

            // Create root nodes
            TreeNode staticnode = new TreeNode("Static Members");
            TreeNode instancenode = new TreeNode("Instance Members");

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
                string name = string.Format("{0} {1} {2}", qualifier, Utility.TransformType(field.FieldType.Name), field.Name);
                TreeNode node = new TreeNode(name);
                string icon = SelectIcon(field);
                node.ImageKey = icon;
                node.SelectedImageKey = icon;
                node.Tag = field;
                if (field.IsStatic)
                    staticnode.Nodes.Add(node);
                else
                    instancenode.Nodes.Add(node);
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
                        sb.Append("private ");
                    else if (prop.GetMethod.IsPublic)
                        sb.Append("public ");
                    else
                        sb.Append("protected ");
                    sb.Append("getter, ");
                }
                if (prop.SetMethod != null)
                {
                    ignoremethods.Add(prop.SetMethod);
                    if (prop.SetMethod.IsPrivate)
                        sb.Append("private ");
                    else if (prop.SetMethod.IsPublic)
                        sb.Append("public ");
                    else
                        sb.Append("protected ");
                    sb.Append("setter");
                }
                sb.Append(")");
                string name = string.Format("{0} {1} {2}", Utility.TransformType(prop.PropertyType.Name), prop.Name, sb.ToString());
                TreeNode node = new TreeNode(name);
                string icon = SelectIcon(prop);
                node.ImageKey = icon;
                node.SelectedImageKey = icon;
                node.Tag = prop;
                bool propstatic = (prop.GetMethod != null ? prop.GetMethod.IsStatic : false) || (prop.SetMethod != null ? prop.SetMethod.IsStatic : false);
                if (propstatic)
                    staticnode.Nodes.Add(node);
                else
                    instancenode.Nodes.Add(node);
                
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
                        staticnode.Nodes.Add(node);
                    else
                        instancenode.Nodes.Add(node);
                }
            }

            // Add root nodes
            if (staticnode.Nodes.Count > 0)
                objectview.Nodes.Add(staticnode);
            if (instancenode.Nodes.Count > 0)
                objectview.Nodes.Add(instancenode);
            objectview.ExpandAll();
        }

        

        private string SelectIcon(FieldDefinition field)
        {
            if (field.IsPrivate)
                return "Field-Private_545.png";
            else if (field.IsPublic)
                return "FieldIcon.png";
            else
                return "Field-Protected_544.png";
        }

        private string SelectIcon(PropertyDefinition prop)
        {
            MethodDefinition getmethod = prop.GetMethod;
            MethodDefinition setmethod = prop.SetMethod;
            if (getmethod == null && setmethod == null)
                return "Property_501.png";
            else
            {
                if ((getmethod != null && getmethod.IsPublic) || (setmethod != null && setmethod.IsPublic))
                    return "Property_501.png";
                else
                    return "Property-Private_505.png";
            }
        }

        private string SelectIcon(MethodDefinition method)
        {
            if (method.IsPrivate)
                return "Method-Private_640.png";
            else if (method.IsPublic)
                return "Method_636.png";
            else
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
            if (selected == null) return;

            if (selected.Tag is MethodDefinition)
            {
                MethodViewControl methodview = new MethodViewControl();
                methodview.Dock = DockStyle.Fill;
                methodview.MethodDef = selected.Tag as MethodDefinition;
                methodview.MainForm = MainForm;
                splitter.Panel2.Controls.Add(methodview);
                currentview = methodview;
            }
        }
    }
}
