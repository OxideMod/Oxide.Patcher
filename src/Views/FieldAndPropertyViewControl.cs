using Mono.Cecil;
using Oxide.Patcher.Modifiers;
using System;
using System.Text;
using System.Windows.Forms;

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

        private Modifier modifierview;

        public FieldAndPropertyViewControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            PopulateDetails();

            bool modifierfound = false;
            foreach (Manifest manifest in MainForm.CurrentProject.Manifests)
            {
                if (FieldDef != null)
                {
                    foreach (Modifier modifier in manifest.Modifiers)
                    {
                        if (modifier.Signature.Equals(Utility.GetModifierSignature(FieldDef)) && modifier.TypeName == FieldDef.DeclaringType.FullName)
                        {
                            modifierfound = true;
                            modifierview = modifier;
                            break;
                        }
                    }
                }

                if (PropertyDef != null)
                {
                    foreach (Modifier modifier in manifest.Modifiers)
                    {
                        if (modifier.Signature.Equals(Utility.GetModifierSignature(PropertyDef)) && modifier.TypeName == PropertyDef.DeclaringType.FullName)
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
            typenametextbox.Text = PropertyDef?.FullName ?? FieldDef.FullName;
            if (FieldDef != null)
            {
                detailsgroup.Text = "Field Details";
                string qualifier = FieldDef.IsPublic ? "public" : FieldDef.IsPrivate ? "private" : "protected";
                declarationtextbox.Text = $"{qualifier} {Utility.TransformType(FieldDef.FieldType.Name)} {FieldDef.Name}";
                editbutton.Text = "Edit Field Modifier";
                gotoeditbutton.Text = "Goto Field Modifiers";
            }
            if (PropertyDef != null)
            {
                detailsgroup.Text = "Property Details";
                StringBuilder sb = new StringBuilder();
                sb.Append("(");
                if (PropertyDef.GetMethod != null)
                {
                    if (PropertyDef.GetMethod.IsPrivate)
                    {
                        sb.Append("private ");
                    }
                    else if (PropertyDef.GetMethod.IsPublic)
                    {
                        sb.Append("public ");
                    }
                    else
                    {
                        sb.Append("protected ");
                    }

                    sb.Append("get, ");
                }
                if (PropertyDef.SetMethod != null)
                {
                    if (PropertyDef.SetMethod.IsPrivate)
                    {
                        sb.Append("private ");
                    }
                    else if (PropertyDef.SetMethod.IsPublic)
                    {
                        sb.Append("public ");
                    }
                    else
                    {
                        sb.Append("protected ");
                    }

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
            Modifier modifier = FieldDef != null ? new Modifier(FieldDef, MainForm.rassemblydict[FieldDef.Module.Assembly]) : new Modifier(PropertyDef, MainForm.rassemblydict[PropertyDef.Module.Assembly]);

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
    }
}
