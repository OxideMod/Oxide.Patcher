using Oxide.Patcher.Hooks;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Oxide.Patcher.Views
{
    public partial class SimpleHookSettingsControl : HookSettingsControl
    {
        private bool ignorechanges;

        public SimpleHookSettingsControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ReturnBehavior[] allreturnbehaviors = Enum.GetValues(typeof(ReturnBehavior)) as ReturnBehavior[];
            Array.Sort(allreturnbehaviors, (a, b) => Comparer<int>.Default.Compare((int)a, (int)b));
            for (int i = 0; i < allreturnbehaviors.Length; i++)
            {
                returnbehavior.Items.Add(allreturnbehaviors[i]);
            }

            ArgumentBehavior[] allargumentbehaviors = Enum.GetValues(typeof(ArgumentBehavior)) as ArgumentBehavior[];
            Array.Sort(allargumentbehaviors, (a, b) => Comparer<int>.Default.Compare((int)a, (int)b));
            for (int i = 0; i < allargumentbehaviors.Length; i++)
            {
                argumentbehavior.Items.Add(allargumentbehaviors[i]);
            }

            Simple hook = Hook as Simple;

            ignorechanges = true;
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

            ignorechanges = false;
        }

        private void injectionindex_ValueChanged(object sender, EventArgs e)
        {
            if (ignorechanges)
            {
                return;
            }

            Simple hook = Hook as Simple;
            hook.InjectionIndex = (int)injectionindex.Value;
            NotifyChanges();
        }

        private void returnbehavior_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ignorechanges)
            {
                return;
            }

            Simple hook = Hook as Simple;
            hook.ReturnBehavior = (ReturnBehavior)returnbehavior.SelectedIndex;
            NotifyChanges();
        }

        private void argumentbehavior_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ignorechanges)
            {
                return;
            }

            Simple hook = Hook as Simple;
            hook.ArgumentBehavior = (ArgumentBehavior)argumentbehavior.SelectedIndex;
            NotifyChanges();
        }

        private void argumentstring_TextChanged(object sender, EventArgs e)
        {
            if (ignorechanges)
            {
                return;
            }

            Simple hook = Hook as Simple;
            hook.ArgumentString = argumentstring.Text;
            NotifyChanges();
        }

        private void chkIsDeprecated_CheckedChanged(object sender, EventArgs e)
        {
            if (ignorechanges)
            {
                return;
            }

            ignorechanges = true;

            CheckBox checkbox = sender as CheckBox;
            Simple hook = Hook as Simple;

            DateTime initialDeprecationDate = DateTime.Now.AddDays(60);
            dtpRemovalDate.Value = initialDeprecationDate;

            hook.Deprecation = checkbox.Checked ? new Simple.DeprecatedStatus
            {
                RemovalDate = initialDeprecationDate,
                ReplacementHook = txtTargetHook.Text
            } : null;
            HandleDeprecationFieldsVisibility(checkbox.Checked);

            ignorechanges = false;

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
            if (ignorechanges)
            {
                return;
            }

            Simple hook = Hook as Simple;
            hook.Deprecation.ReplacementHook = txtTargetHook.Text.Trim();

            NotifyChanges();
        }

        private void dtpRemovalDate_ValueChanged(object sender, EventArgs e)
        {
            if (ignorechanges)
            {
                return;
            }

            Simple hook = Hook as Simple;
            hook.Deprecation.RemovalDate = dtpRemovalDate.Value;

            NotifyChanges();
        }
    }
}
