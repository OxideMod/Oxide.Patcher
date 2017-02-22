using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Mono.Cecil;

using OxidePatcher.Modifiers;

namespace OxidePatcher.Views
{
    public partial class ModifierSettingsControl : UserControl
    {
        private bool ignorechanges;

        internal MethodDefinition methoddef;

        internal PropertyDefinition propertydef;

        internal FieldDefinition fielddef;

        internal ModifierViewControl controller;

        /// <summary>
        /// Gets or sets the hook to use
        /// </summary>
        public Modifier Modifier { get; set; }

        public ModifierSettingsControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            ignorechanges = true;

            if (Modifier.TargetExposure == null)
                Modifier.TargetExposure = Modifier.Signature.Exposure;

            isPublic.Checked = Modifier.TargetExposure[0] == Exposure.Public;
            isPrivate.Checked = !isPublic.Checked;

            if (Modifier.Type == ModifierType.Property)
            {
                if (propertydef.SetMethod != null)
                {
                    isPublicSetter.Visible = true;
                    isPrivateSetter.Visible = true;
                    isPublicSetter.Checked = Modifier.TargetExposure[1] == Exposure.Public;
                    isPrivateSetter.Checked = !isPublicSetter.Checked;
                    isPublicSetter.Text += " setter";
                    isPrivateSetter.Text += " setter";
                    isStatic.Checked = Modifier.TargetExposure.Length == 3;
                }
                else
                {
                    isStatic.Checked = Modifier.TargetExposure.Length == 2;
                }
                isPublic.Text += " getter";
                isPrivate.Text += " getter";
            }
            else
            {
                isStatic.Checked = Modifier.TargetExposure.Length == 2;
            }

            ignorechanges = false;
        }

        private void isPublic_CheckedChanged(object sender, EventArgs e)
        {
            if (ignorechanges) return;
            isPrivate.Checked = !isPublic.Checked;
            UpdateTargetExposure();
            controller.ApplyButton.Enabled = true;
        }

        private void isPublicSetter_CheckedChanged(object sender, EventArgs e)
        {
            if (ignorechanges) return;
            isPrivateSetter.Checked = !isPublicSetter.Checked;
            UpdateTargetExposure();
            controller.ApplyButton.Enabled = true;
        }

        private void isPrivate_CheckedChanged(object sender, EventArgs e)
        {
            if (ignorechanges) return;
            isPublic.Checked = !isPrivate.Checked;
            UpdateTargetExposure();
            controller.ApplyButton.Enabled = true;
        }

        private void isPrivateSetter_CheckedChanged(object sender, EventArgs e)
        {
            if (ignorechanges) return;
            isPublicSetter.Checked = !isPrivateSetter.Checked;
            UpdateTargetExposure();
            controller.ApplyButton.Enabled = true;
        }

        private void isStatic_CheckedChanged(object sender, EventArgs e)
        {
            if (ignorechanges) return;
            isPublicSetter.Checked = !isPrivateSetter.Checked;
            UpdateTargetExposure();
            controller.ApplyButton.Enabled = true;
        }

        private void UpdateTargetExposure()
        {
            var exposure = new List<Exposure>();
            if (isPublic.Checked)
                exposure.Add(Exposure.Public);
            else if (isPrivate.Checked)
                exposure.Add(Exposure.Private);
            if (Modifier.Type == ModifierType.Property)
            {
                if (propertydef.SetMethod != null)
                {
                    if (isPublicSetter.Checked)
                        exposure.Add(Exposure.Public);
                    else if (isPrivateSetter.Checked)
                        exposure.Add(Exposure.Private);
                }
            }
            if (isStatic.Checked)
                exposure.Add(Exposure.Static);

            Modifier.TargetExposure = exposure.ToArray();
        }
    }
}
