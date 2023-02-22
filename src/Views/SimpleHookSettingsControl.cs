using Oxide.Patcher.Hooks;
using System;
using System.Windows.Forms;

namespace Oxide.Patcher.Views
{
    public partial class SimpleHookSettingsControl : HookSettingsControl
    {
        private bool _ignoreChanges = true;

        private static readonly ReturnBehavior[] ReturnBehaviors;
        private static readonly ArgumentBehavior[] ArgumentBehaviors;

        static SimpleHookSettingsControl()
        {
            ReturnBehaviors = Enum.GetValues(typeof(ReturnBehavior)) as ReturnBehavior[];
            Array.Sort(ReturnBehaviors, (a, b) => a.CompareTo(b));

            ArgumentBehaviors = Enum.GetValues(typeof(ArgumentBehavior)) as ArgumentBehavior[];
            Array.Sort(ArgumentBehaviors, (a, b) => a.CompareTo(b));
        }

        public SimpleHookSettingsControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!(Hook is Simple hook))
            {
                MessageBox.Show("Hook in modify hook settings was not a modify hook", "Invalid Hook Type",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (ReturnBehavior behavior in ReturnBehaviors)
            {
                returnbehavior.Items.Add(behavior);
            }

            foreach (ArgumentBehavior behavior in ArgumentBehaviors)
            {
                argumentbehavior.Items.Add(behavior);
            }

            injectionindex.Value = hook.InjectionIndex;
            returnbehavior.SelectedIndex = (int)hook.ReturnBehavior;
            argumentbehavior.SelectedIndex = (int)hook.ArgumentBehavior;
            argumentstring.Text = string.IsNullOrEmpty(hook.ArgumentString) ? string.Empty : hook.ArgumentString;
            if (hook.Deprecation != null)
            {
                chkIsDeprecated.Checked = true;
                txtTargetHook.Text = hook.Deprecation.ReplacementHook;
                dtpRemovalDate.Value = hook.Deprecation.RemovalDate;
                HandleDeprecationFieldsVisibility(true);
            }

            _ignoreChanges = false;
        }

        private void injectionindex_ValueChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges)
            {
                return;
            }

            if (!(Hook is Simple hook))
            {
                MessageBox.Show("Hook in modify hook settings was not a modify hook", "Invalid Hook Type",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            hook.InjectionIndex = (int)injectionindex.Value;
            NotifyChanges();
        }

        private void returnbehavior_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges)
            {
                return;
            }

            if (!(Hook is Simple hook))
            {
                MessageBox.Show("Hook in modify hook settings was not a modify hook", "Invalid Hook Type",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            hook.ReturnBehavior = (ReturnBehavior)returnbehavior.SelectedIndex;
            NotifyChanges();
        }

        private void argumentbehavior_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges)
            {
                return;
            }

            if (!(Hook is Simple hook))
            {
                MessageBox.Show("Hook in modify hook settings was not a modify hook", "Invalid Hook Type",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            hook.ArgumentBehavior = (ArgumentBehavior)argumentbehavior.SelectedIndex;
            NotifyChanges();
        }

        private void argumentstring_TextChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges)
            {
                return;
            }

            if (!(Hook is Simple hook))
            {
                MessageBox.Show("Hook in modify hook settings was not a modify hook", "Invalid Hook Type",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            hook.ArgumentString = argumentstring.Text;
            NotifyChanges();
        }

        private void chkIsDeprecated_CheckedChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges || !(sender is CheckBox checkBox) || !(Hook is Simple hook))
            {
                return;
            }

            _ignoreChanges = true;

            DateTime initialDeprecationDate = DateTime.Now.AddDays(60);
            dtpRemovalDate.Value = initialDeprecationDate;

            hook.Deprecation = checkBox.Checked ? new Simple.DeprecatedStatus
            {
                RemovalDate = initialDeprecationDate,
                ReplacementHook = txtTargetHook.Text
            } : null;

            HandleDeprecationFieldsVisibility(checkBox.Checked);

            _ignoreChanges = false;

            NotifyChanges();
        }

        private void HandleDeprecationFieldsVisibility(bool shouldShow)
        {
            lblTargetHook.Visible = shouldShow;
            txtTargetHook.Visible = shouldShow;
            lblRemovalDate.Visible = shouldShow;
            dtpRemovalDate.Visible = shouldShow;
        }

        private void txtTargetHook_TextChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges)
            {
                return;
            }

            if (!(Hook is Simple hook))
            {
                MessageBox.Show("Hook in modify hook settings was not a modify hook", "Invalid Hook Type",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            hook.Deprecation.ReplacementHook = txtTargetHook.Text.Trim();

            NotifyChanges();
        }

        private void dtpRemovalDate_ValueChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges)
            {
                return;
            }

            if (!(Hook is Simple hook))
            {
                MessageBox.Show("Hook in modify hook settings was not a modify hook", "Invalid Hook Type",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            hook.Deprecation.RemovalDate = dtpRemovalDate.Value;

            NotifyChanges();
        }
    }
}
