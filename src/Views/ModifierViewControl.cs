using Oxide.Patcher.Modifiers;
using System;
using System.Windows.Forms;

namespace Oxide.Patcher.Views
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

            ModifierSettingsControl settingsView = Modifier.CreateSettingsView();
            settingsView.Controller = this;
            settingsView.Dock = DockStyle.Fill;

            switch (Modifier.Type)
            {
                case ModifierType.Field:
                    detailsgroup.Text = "Field Details";
                    namelabel.Text = "Field Name:";
                    settingsView.FieldDef = MainForm.AssemblyLoader.GetField(Modifier.AssemblyName, Modifier.TypeName, Modifier.Name, Modifier.Signature);
                    break;

                case ModifierType.Method:
                    detailsgroup.Text = "Method Details";
                    namelabel.Text = "Method Name:";
                    settingsView.MethodDef = MainForm.AssemblyLoader.GetMethod(Modifier.AssemblyName, Modifier.TypeName, Modifier.Signature);
                    break;

                case ModifierType.Property:
                    detailsgroup.Text = "Property Details";
                    namelabel.Text = "Property Name:";
                    settingsView.PropertyDef = MainForm.AssemblyLoader.GetProperty(Modifier.AssemblyName, Modifier.TypeName, Modifier.Name, Modifier.Signature);
                    break;

                case ModifierType.Type:
                    detailsgroup.Text = "Type Details";
                    namelabel.Text = "Type Name:";
                    settingsView.TypeDef = MainForm.AssemblyLoader.GetType(Modifier.AssemblyName, Modifier.TypeName);
                    break;
            }

            modifiersettingstab.Controls.Add(settingsView);

            assemblytextbox.Text = Modifier.AssemblyName;
            typenametextbox.Text = Modifier.TypeName;

            typenamelabel.Visible = Modifier.Type != ModifierType.Type;
            typenametextbox.Visible = Modifier.Type != ModifierType.Type;

            if (settingsView.FieldDef != null || settingsView.MethodDef != null || settingsView.PropertyDef != null)
            {
                nametextbox.Text = $"{Modifier.TypeName}::{Modifier.Name}";
            }
            else if (settingsView.TypeDef != null)
            {
                nametextbox.Text = Modifier.TypeName;
            }
            else
            {
                nametextbox.Text = Modifier.Type != ModifierType.Type ? $"{Modifier.TypeName}::{Modifier.Name} (MISSING)" : $"{Modifier.TypeName} (MISSING)";
            }

            flagbutton.Enabled = !Modifier.Flagged;
            unflagbutton.Enabled = Modifier.Flagged;
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
