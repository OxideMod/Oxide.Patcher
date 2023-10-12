using Mono.Cecil;
using Oxide.Patcher.Modifiers;
using System;
using System.Text;
using System.Windows.Forms;
using Mono.CSharp;
using Oxide.Patcher.Common;

namespace Oxide.Patcher
{
    public partial class FieldAndPropertyViewControl : UserControl
    {
        /// <summary>
        /// Gets or sets the property definition to use
        /// </summary>
        public PropertyDefinition PropertyDef { get; set; }

        /// <summary>
        /// Gets or sets the field definition to use
        /// </summary>
        public FieldDefinition FieldDef { get; set; }

        /// <summary>
        /// Gets or sets the main patcher form
        /// </summary>
        public PatcherForm MainForm { get; set; }

        private Modifier _modifierView;

        public FieldAndPropertyViewControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            PopulateDetails();

            bool modifierFound = (FieldDef != null || PropertyDef != null) && FindModifier();

            editbutton.Enabled = !modifierFound;
            gotoeditbutton.Enabled = modifierFound;
        }

        private bool FindModifier()
        {
            foreach (Manifest manifest in MainForm.CurrentProject.Manifests)
            {
                foreach (Modifier modifier in manifest.Modifiers)
                {
                    if (FieldDef != null && modifier.Signature.Equals(Utility.GetModifierSignature(FieldDef)) && modifier.TypeName == FieldDef.DeclaringType.FullName)
                    {
                        _modifierView = modifier;
                        return true;
                    }

                    if (PropertyDef != null && modifier.Signature.Equals(Utility.GetModifierSignature(PropertyDef)) && modifier.TypeName == PropertyDef.DeclaringType.FullName)
                    {
                        _modifierView = modifier;
                        return true;
                    }
                }
            }

            return false;
        }

        private void PopulateDetails()
        {
            typenametextbox.Text = FieldDef?.FullName ?? PropertyDef?.FullName;

            if (FieldDef != null)
            {
                detailsgroup.Text = "Field Details";
                string qualifier = FieldDef.IsPublic ? "public" : FieldDef.IsPrivate ? "private" : "protected";
                declarationtextbox.Text = $"{qualifier} {Utility.TransformType(FieldDef.FieldType.Name)} {FieldDef.Name}";
                editbutton.Text = "Edit Field Modifier";
                gotoeditbutton.Text = "Goto Field Modifiers";
            }
            else if (PropertyDef != null)
            {
                detailsgroup.Text = "Property Details";
                StringBuilder sb = new StringBuilder();

                sb.Append("(");

                MethodDefinition propertyGetMethod = PropertyDef.GetMethod;
                if (propertyGetMethod != null)
                {
                    sb.Append(propertyGetMethod.IsPrivate ? "private " : propertyGetMethod.IsPublic ? "public " : "protected ");
                    sb.Append("get, ");
                }

                MethodDefinition propertySetMethod = PropertyDef.SetMethod;
                if (propertySetMethod != null)
                {
                    sb.Append(propertySetMethod.IsPrivate ? "private " : propertySetMethod.IsPublic ? "public " : "protected ");
                    sb.Append("set");
                }

                sb.Append(")");

                declarationtextbox.Text = $"{Utility.TransformType(PropertyDef.PropertyType.Name)} {PropertyDef.Name} {sb.Replace(", )", ")")}";
                editbutton.Text = "Edit Property Modifier";
                gotoeditbutton.Text = "Goto Property Modifiers";
            }
        }

        private void editbutton_Click(object sender, EventArgs e)
        {
            Modifier modifier = FieldDef != null
                ? new Modifier(FieldDef, MainForm.AssemblyLoader.rassemblydict[FieldDef.Module.Assembly])
                : new Modifier(PropertyDef, MainForm.AssemblyLoader.rassemblydict[PropertyDef.Module.Assembly]);

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
    }
}
