using Mono.Cecil;
using Oxide.Patcher.Modifiers;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Oxide.Patcher.Views
{
    public partial class ModifierSettingsControl : UserControl
    {
        private bool ignorechanges;

        internal MethodDefinition MethodDef;

        internal PropertyDefinition PropertyDef;

        internal FieldDefinition FieldDef;

        internal TypeDefinition TypeDef;

        internal ModifierViewControl Controller;

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
            {
                Modifier.TargetExposure = Modifier.Signature.Exposure;
            }

            isPublic.Checked = Modifier.TargetExposure[0] == Exposure.Public;
            isPrivate.Checked = Modifier.TargetExposure[0] == Exposure.Private;
            isProtected.Checked = Modifier.TargetExposure[0] == Exposure.Protected;
            isInternal.Checked = Modifier.TargetExposure[0] == Exposure.Internal;

            if (Modifier.Type == ModifierType.Property)
            {
                isPublic.Text += " getter";
                isPrivate.Text += " getter";
                isProtected.Text += " getter";
                isInternal.Text += " getter";

                if (PropertyDef.SetMethod != null)
                {
                    isPublicSetter.Visible = true;
                    isPrivateSetter.Visible = true;
                    isProtectedSetter.Visible = true;
                    isInternalSetter.Visible = true;
                    isPublicSetter.Checked = Modifier.TargetExposure[1] == Exposure.Public;
                    isPrivateSetter.Checked = Modifier.TargetExposure[1] == Exposure.Private;
                    isProtectedSetter.Checked = Modifier.TargetExposure[1] == Exposure.Protected;
                    isInternalSetter.Checked = Modifier.TargetExposure[1] == Exposure.Internal;
                    isPublicSetter.Text += " setter";
                    isPrivateSetter.Text += " setter";
                    isProtectedSetter.Text += " setter";
                    isInternalSetter.Text += " setter";
                    isStatic.Checked = Modifier.TargetExposure.Length == 3;
                }
                else
                {
                    isStatic.Checked = Modifier.TargetExposure.Length == 2;
                }
            }
            else
            {
                isStatic.Checked = Modifier.TargetExposure.Length == 2;
            }

            if (Modifier.Type == ModifierType.Type)
            {
                isProtected.Enabled = false;
                isInternal.Enabled = false;
            }

            ignorechanges = false;
        }

        private void ChangeExposure(object sender, EventArgs e)
        {
            if (ignorechanges)
            {
                return;
            }

            CheckBox checkbox = sender as CheckBox;
            ignorechanges = true;
            if (checkbox.Checked)
            {
                isPublic.Checked = checkbox == isPublic;
                isPrivate.Checked = checkbox == isPrivate;
                isProtected.Checked = checkbox == isProtected;
                isInternal.Checked = checkbox == isInternal;
            }
            else
            {
                isPublic.Checked = Modifier.Signature.Exposure[0] == Exposure.Public;
                isPrivate.Checked = Modifier.Signature.Exposure[0] == Exposure.Private;
                isProtected.Checked = Modifier.Signature.Exposure[0] == Exposure.Protected;
                isInternal.Checked = Modifier.Signature.Exposure[0] == Exposure.Internal;
            }
            ignorechanges = false;
            UpdateTargetExposure();
            Controller.ApplyButton.Enabled = true;
        }

        private void ChangeSetterExposure(object sender, EventArgs e)
        {
            if (ignorechanges)
            {
                return;
            }

            CheckBox checkbox = sender as CheckBox;
            ignorechanges = true;
            if (checkbox.Checked)
            {
                isPublicSetter.Checked = checkbox == isPublicSetter;
                isPrivateSetter.Checked = checkbox == isPrivateSetter;
                isProtectedSetter.Checked = checkbox == isProtectedSetter;
                isInternalSetter.Checked = checkbox == isInternalSetter;
            }
            else
            {
                isPublicSetter.Checked = Modifier.Signature.Exposure[1] == Exposure.Public;
                isPrivateSetter.Checked = Modifier.Signature.Exposure[1] == Exposure.Private;
                isProtectedSetter.Checked = Modifier.Signature.Exposure[1] == Exposure.Protected;
                isInternalSetter.Checked = Modifier.Signature.Exposure[1] == Exposure.Internal;
            }
            ignorechanges = false;
            UpdateTargetExposure();
            Controller.ApplyButton.Enabled = true;
        }

        private void isStatic_CheckedChanged(object sender, EventArgs e)
        {
            if (ignorechanges)
            {
                return;
            }

            UpdateTargetExposure();
            Controller.ApplyButton.Enabled = true;
        }

        private void UpdateTargetExposure()
        {
            List<Exposure> exposure = new List<Exposure>();
            if (isPublic.Checked)
            {
                exposure.Add(Exposure.Public);
            }
            else if (isPrivate.Checked)
            {
                exposure.Add(Exposure.Private);
            }
            else if (isProtected.Checked)
            {
                exposure.Add(Exposure.Protected);
            }
            else if (isInternal.Checked)
            {
                exposure.Add(Exposure.Internal);
            }

            if (Modifier.Type == ModifierType.Property)
            {
                if (PropertyDef.SetMethod != null)
                {
                    if (isPublicSetter.Checked)
                    {
                        exposure.Add(Exposure.Public);
                    }
                    else if (isPrivateSetter.Checked)
                    {
                        exposure.Add(Exposure.Private);
                    }
                    else if (isProtectedSetter.Checked)
                    {
                        exposure.Add(Exposure.Protected);
                    }
                    else if (isInternalSetter.Checked)
                    {
                        exposure.Add(Exposure.Internal);
                    }
                }
            }

            if (isStatic.Checked)
            {
                exposure.Add(Exposure.Static);
            }

            Modifier.TargetExposure = exposure.ToArray();
        }
    }
}
