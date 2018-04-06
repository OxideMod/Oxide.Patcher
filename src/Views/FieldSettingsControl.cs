using Oxide.Patcher.Fields;
using System;
using System.Windows.Forms;

namespace Oxide.Patcher.Views
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
            using (ModifyForm form = new ModifyForm(Field, TargetTypeText.Text))
            {
                if (form.ShowDialog(PatcherForm.MainForm) != DialogResult.OK)
                {
                    return;
                }

                if (form.Instruction == null)
                {
                    return;
                }

                TargetTypeText.Text = form.Instruction.Operand.ToString();

                Field.FieldType = TargetTypeText.Text;
                Controller.ApplyButton.Enabled = true;
            }
        }
    }
}
