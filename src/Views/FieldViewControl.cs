using Oxide.Patcher.Fields;
using System;
using System.Windows.Forms;

namespace Oxide.Patcher.Views
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

        private bool _loaded;

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

            FieldSettingsControl settingsview = Field.CreateSettingsView();
            settingsview.Controller = this;
            settingsview.Dock = DockStyle.Fill;

            fieldsettingstab.Controls.Add(settingsview);

            assemblytextbox.Text = Field.AssemblyName;
            typenametextbox.Text = Field.TypeName;
            nametextbox.Text = Field.Name;

            flagbutton.Enabled = !Field.Flagged;
            unflagbutton.Enabled = Field.Flagged;

            _loaded = true;
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
            applybutton.Enabled = false;

            string previousName = Field.Name;
            Field.Name = nametextbox.Text;

            if (!Field.IsValid(MainForm.CurrentProject, true))
            {
                nametextbox.Text = previousName;
                Field.Name = previousName;
                return;
            }

            MainForm.UpdateField(Field, false);
        }

        private void nametextbox_TextChanged(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            applybutton.Enabled = true;
        }
    }
}
