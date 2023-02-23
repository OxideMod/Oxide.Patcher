using Oxide.Patcher.Hooks;
using System;
using System.Windows.Forms;

namespace Oxide.Patcher.Views
{
    public partial class SimpleHookSettingsControl : HookSettingsControl<Simple>
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

            foreach (ReturnBehavior behavior in ReturnBehaviors)
            {
                returnbehavior.Items.Add(behavior);
            }

            foreach (ArgumentBehavior behavior in ArgumentBehaviors)
            {
                argumentbehavior.Items.Add(behavior);
            }

            injectionindex.Value = Hook.InjectionIndex;
            returnbehavior.SelectedIndex = (int)Hook.ReturnBehavior;
            argumentbehavior.SelectedIndex = (int)Hook.ArgumentBehavior;
            argumentstring.Text = string.IsNullOrEmpty(Hook.ArgumentString) ? string.Empty : Hook.ArgumentString;

            if (Hook.Deprecation != null)
            {
                chkIsDeprecated.Checked = true;
                txtTargetHook.Text = Hook.Deprecation.ReplacementHook;
                dtpRemovalDate.Value = Hook.Deprecation.RemovalDate;
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

            Hook.InjectionIndex = (int)injectionindex.Value;
            NotifyChanges();
        }

        private void returnbehavior_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges)
            {
                return;
            }

            Hook.ReturnBehavior = (ReturnBehavior)returnbehavior.SelectedIndex;
            NotifyChanges();
        }

        private void argumentbehavior_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges)
            {
                return;
            }

            Hook.ArgumentBehavior = (ArgumentBehavior)argumentbehavior.SelectedIndex;
            NotifyChanges();
        }

        private void argumentstring_TextChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges)
            {
                return;
            }

            Hook.ArgumentString = argumentstring.Text;
            NotifyChanges();
        }

        private void chkIsDeprecated_CheckedChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges || !(sender is CheckBox checkBox))
            {
                return;
            }

            _ignoreChanges = true;

            DateTime initialDeprecationDate = DateTime.Now.AddDays(60);
            dtpRemovalDate.Value = initialDeprecationDate;

            Hook.Deprecation = checkBox.Checked ? new Simple.DeprecatedStatus
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

            Hook.Deprecation.ReplacementHook = txtTargetHook.Text.Trim();

            NotifyChanges();
        }

        private void dtpRemovalDate_ValueChanged(object sender, EventArgs e)
        {
            if (_ignoreChanges)
            {
                return;
            }

            Hook.Deprecation.RemovalDate = dtpRemovalDate.Value;

            NotifyChanges();
        }
    }
}
