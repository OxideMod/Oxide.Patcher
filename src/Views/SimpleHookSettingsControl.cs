using System;
using System.Collections.Generic;

using Oxide.Patcher.Hooks;

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
    }
}
