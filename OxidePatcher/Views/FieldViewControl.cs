using System;
using System.Windows.Forms;

using OxidePatcher.Fields;

namespace OxidePatcher.Views
{
    public partial class FieldViewControl : UserControl
    {
        /// <summary>
        /// Gets or sets the modifier to use
        /// </summary>
        public Field Field { get; set; }

        /// <summary>
        /// Gets or sets the main patcher form
        /// </summary>
        public PatcherForm MainForm { get; set; }

        public Button FlagButton { get; set; }

        public Button UnflagButton { get; set; }

        public Button ApplyButton { get; set; }

        private bool loaded = false;

        public FieldViewControl()
        {
            InitializeComponent();
            FlagButton = flagbutton;
            UnflagButton = unflagbutton;
            ApplyButton = applybutton;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var settingsview = Field.CreateSettingsView();
            settingsview.Controller = this;
            settingsview.Dock = DockStyle.Fill;

            fieldsettingstab.Controls.Add(settingsview);

            assemblytextbox.Text = Field.AssemblyName;
            typenametextbox.Text = Field.TypeName;
            nametextbox.Text = Field.Name;

            if (Field.Flagged)
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

            loaded = true;
        }

        private void deletebutton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(MainForm, "Are you sure you want to remove this field?", "Oxide Patcher", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                MainForm.RemoveField(Field);
            }
        }

        private void flagbutton_Click(object sender, EventArgs e)
        {
            Field.Flagged = true;
            MainForm.UpdateField(Field, false);
            flagbutton.Enabled = false;
            unflagbutton.Enabled = true;
        }

        private void unflagbutton_Click(object sender, EventArgs e)
        {
            Field.Flagged = false;
            MainForm.UpdateField(Field, false);
            flagbutton.Enabled = true;
            unflagbutton.Enabled = false;
        }

        private void applybutton_Click(object sender, EventArgs e)
        {
            var temp = Field.Name;
            Field.Name = nametextbox.Text;
            applybutton.Enabled = false;

            if (!Field.IsValid(true))
            {
                nametextbox.Text = temp;
                Field.Name = temp;
                return;
            }

            MainForm.UpdateField(Field, false);
        }

        private void nametextbox_TextChanged(object sender, EventArgs e)
        {
            if (!loaded) return;
            applybutton.Enabled = true;
        }
    }
}
