using System;
using System.Windows.Forms;

using OxidePatcher.Modifiers;

namespace OxidePatcher.Views
{
    public partial class ModifierViewControl : UserControl
    {
        /// <summary>
        /// Gets or sets the modifier to use
        /// </summary>
        public Modifier Modifier { get; set; }

        /// <summary>
        /// Gets or sets the main patcher form
        /// </summary>
        public PatcherForm MainForm { get; set; }

        public Button FlagButton { get; set; }

        public Button UnflagButton { get; set; }

        public Button ApplyButton { get; set; }

        public ModifierViewControl()
        {
            InitializeComponent();
            FlagButton = flagbutton;
            UnflagButton = unflagbutton;
            ApplyButton = applybutton;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var settingsview = Modifier.CreateSettingsView();
            settingsview.controller = this;
            settingsview.Dock = DockStyle.Fill;

            switch (Modifier.Type)
            {
                case ModifierType.Field:
                    detailsgroup.Text = "Field Details";
                    namelabel.Text = "Field Name:";
                    settingsview.fielddef = MainForm.GetField(Modifier.AssemblyName, Modifier.TypeName, Modifier.Name, Modifier.Signature);
                    break;
                case ModifierType.Method:
                    detailsgroup.Text = "Method Details";
                    namelabel.Text = "Method Name:";
                    settingsview.methoddef = MainForm.GetMethod(Modifier.AssemblyName, Modifier.TypeName, Modifier.Signature);
                    break;
                case ModifierType.Property:
                    detailsgroup.Text = "Property Details";
                    namelabel.Text = "Property Name:";
                    settingsview.propertydef = MainForm.GetProperty(Modifier.AssemblyName, Modifier.TypeName, Modifier.Name, Modifier.Signature);
                    break;
            }

            modifiersettingstab.Controls.Add(settingsview);

            assemblytextbox.Text = Modifier.AssemblyName;
            typenametextbox.Text = Modifier.TypeName;

            if (settingsview.fielddef != null || settingsview.methoddef != null || settingsview.propertydef != null)
                nametextbox.Text = $"{Modifier.TypeName}::{Modifier.Name}";
            else
                nametextbox.Text = $"{Modifier.TypeName}::{Modifier.Name} (MISSING)";

            nametextbox.Text = Modifier.Name;

            if (Modifier.Flagged)
            {
                flagbutton.Enabled = false;
                unflagbutton.Enabled = true;
                unflagbutton.Focus();
            }
            else
            {
                flagbutton.Enabled = true;
                unflagbutton.Enabled = false;
                flagbutton.Focus();
            }
        }

        private void deletebutton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(MainForm, "Are you sure you want to remove these modifier changes?", "Oxide Patcher", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                MainForm.RemoveModifier(Modifier);
            }
        }

        private void flagbutton_Click(object sender, EventArgs e)
        {
            Modifier.Flagged = true;
            MainForm.UpdateModifier(Modifier, false);
            flagbutton.Enabled = false;
            unflagbutton.Enabled = true;
        }

        private void unflagbutton_Click(object sender, EventArgs e)
        {
            Modifier.Flagged = false;
            MainForm.UpdateModifier(Modifier, false);
            flagbutton.Enabled = true;
            unflagbutton.Enabled = false;
        }

        private void applybutton_Click(object sender, EventArgs e)
        {
            MainForm.UpdateModifier(Modifier, false);
            applybutton.Enabled = false;
        }
    }
}
