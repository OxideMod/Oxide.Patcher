using Mono.Cecil;
using Oxide.Patcher.Modifiers;
using System;
using System.Text;
using System.Windows.Forms;

namespace Oxide.Patcher.Views
{
    public partial class InterfaceViewControl : UserControl
    {
        /// <summary>
        /// Gets or sets the type definition to use
        /// </summary>
        public TypeDefinition TypeDef { get; set; }

        /// <summary>
        /// Gets or sets the main patcher form
        /// </summary>
        public PatcherForm MainForm { get; set; }

        private Modifier modifierview;

        public InterfaceViewControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            // Populate the details
            PopulateDetails();

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

                sb.Append("interface ");
                sb.Append(TypeDef.Name);
               
                declarationtextbox.Text = sb.ToString();
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(PatcherForm.MainForm, "Error loading details for a interface.",
                    "Null Reference Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void declarationtextbox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
