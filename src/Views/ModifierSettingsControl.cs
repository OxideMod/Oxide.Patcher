using Mono.Cecil;
using Oxide.Patcher.Modifiers;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Oxide.Patcher.Views
{
    public partial class ModifierSettingsControl : UserControl
    {
        private bool _ignoreChanges;

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

            _ignoreChanges = true;

            if (Modifier.TargetExposure == null)
            {
                Modifier.TargetExposure = Modifier.Signature.Exposure;
            }

            Exposure exposure = Modifier.TargetExposure[0];

            isPublic.Checked = exposure == Exposure.Public;
            isPrivate.Checked = exposure == Exposure.Private;
            isProtected.Checked = exposure == Exposure.Protected;
            isInternal.Checked = exposure == Exposure.Internal;

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

                    exposure = Modifier.TargetExposure[1];
                    isPublicSetter.Checked = exposure == Exposure.Public;
                    isPrivateSetter.Checked = exposure == Exposure.Private;
                    isProtectedSetter.Checked = exposure == Exposure.Protected;
                    isInternalSetter.Checked = exposure == Exposure.Internal;

                    isPublicSetter.Text += " setter";
                    isPrivateSetter.Text += " setter";
                    isProtectedSetter.Text += " setter";
                    isInternalSetter.Text += " setter";
                }
            }

            isStatic.Checked = Modifier.HasTargetExposure(Exposure.Static);

            if (Modifier.Type == ModifierType.Type)
            {
                isProtected.Enabled = false;
                isInternal.Enabled = false;
            }

            _ignoreChanges = false;
        }

        private void ChangeExposure(object sender, EventArgs e)
        {
            if (_ignoreChanges || !(sender is CheckBox checkbox))
            {
                return;
            }

            _ignoreChanges = true;

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

            _ignoreChanges = false;

            UpdateTargetExposure();

            Controller.ApplyButton.Enabled = true;
        }

        private void ChangeSetterExposure(object sender, EventArgs e)
        {
            if (_ignoreChanges || !(sender is CheckBox checkbox))
            {
                return;
            }

            _ignoreChanges = true;

            if (checkbox.Checked)
            {
                isPublicSetter.Checked = checkbox == isPublicSetter;
                isPrivateSetter.Checked = checkbox == isPrivateSetter;
                isProtectedSetter.Checked = checkbox == isProtectedSetter;
                isInternalSetter.Checked = checkbox == isInternalSetter;
            }
            else
            {
                Exposure exposure = Modifier.Signature.Exposure[1];
                isPublicSetter.Checked = exposure == Exposure.Public;
                isPrivateSetter.Checked = exposure == Exposure.Private;
                isProtectedSetter.Checked = exposure == Exposure.Protected;
                isInternalSetter.Checked = exposure == Exposure.Internal;
            }

            _ignoreChanges = false;

            UpdateTargetExposure();

            Controller.ApplyButton.Enabled = true;
        }

        private void isStatic_CheckedChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges)
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
