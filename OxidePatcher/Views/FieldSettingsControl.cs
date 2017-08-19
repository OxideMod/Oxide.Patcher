using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Mono.Cecil;

using OxidePatcher.Fields;

namespace OxidePatcher.Views
{
    public partial class FieldSettingsControl : UserControl
    {
        internal FieldViewControl Controller;

        /// <summary>
        /// Gets or sets the hook to use
        /// </summary>
        public Field Field { get; set; }

        public FieldSettingsControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            TargetTypeText.Text = Field.FieldType;
        }

        private void ModifyButton_Click(object sender, EventArgs e)
        {
            using (var form = new ModifyForm(Field, TargetTypeText.Text))
            {
                if (form.ShowDialog(PatcherForm.MainForm) != DialogResult.OK) return;
                if (form.Instruction == null) return;
                TargetTypeText.Text = form.Instruction.Operand.ToString();

                Field.FieldType = TargetTypeText.Text;
                Controller.ApplyButton.Enabled = true;
            }
        }
    }
}
