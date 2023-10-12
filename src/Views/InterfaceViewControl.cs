using Mono.Cecil;
using Oxide.Patcher.Modifiers;
using System;
using System.Text;
using System.Windows.Forms;
using Oxide.Patcher.Common;

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

        private Modifier _modifierView;

        public InterfaceViewControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            // Populate the details
            PopulateDetails();

            bool modifierfound = FindModifier();

            editbutton.Enabled = !modifierfound;
            gotoeditbutton.Enabled = modifierfound;
        }

        private bool FindModifier()
        {
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
                        _modifierView = modifier;
                        return true;
                    }
                }
            }

            return false;
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

                sb.Append("interface ");
                sb.Append(TypeDef.Name);

                declarationtextbox.Text = sb.ToString();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(PatcherForm.MainForm, "Error loading details for a interface.",
                    "Null Reference Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
