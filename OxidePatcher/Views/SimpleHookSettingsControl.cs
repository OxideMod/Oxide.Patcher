using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OxidePatcher.Hooks;

namespace OxidePatcher.Views
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

            ReturnBehaviour[] allreturnbehaviours = Enum.GetValues(typeof(ReturnBehaviour)) as ReturnBehaviour[];
            Array.Sort(allreturnbehaviours, (a, b) => Comparer<int>.Default.Compare((int)a, (int)b));
            for (int i = 0; i < allreturnbehaviours.Length; i++)
                returnbehaviour.Items.Add(allreturnbehaviours[i]);

            ArgumentBehaviour[] allargumentbehaviours = Enum.GetValues(typeof(ArgumentBehaviour)) as ArgumentBehaviour[];
            Array.Sort(allargumentbehaviours, (a, b) => Comparer<int>.Default.Compare((int)a, (int)b));
            for (int i = 0; i < allargumentbehaviours.Length; i++)
                argumentbehaviour.Items.Add(allargumentbehaviours[i]);

            Simple hook = Hook as Simple;

            ignorechanges = true;
            injectionindex.Value = hook.InjectionIndex;
            returnbehaviour.SelectedIndex = (int)hook.ReturnBehaviour;
            argumentbehaviour.SelectedIndex = (int)hook.ArgumentBehaviour;
            argumentstring.Text = string.IsNullOrEmpty(hook.ArgumentString) ? string.Empty : hook.ArgumentString;
            ignorechanges = false;
        }

        private void injectionindex_ValueChanged(object sender, EventArgs e)
        {
            if (ignorechanges) return;

            Simple hook = Hook as Simple;
            hook.InjectionIndex = (int)injectionindex.Value;
            NotifyChanges();
        }

        private void returnbehaviour_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ignorechanges) return;

            Simple hook = Hook as Simple;
            hook.ReturnBehaviour = (ReturnBehaviour)returnbehaviour.SelectedIndex;
            NotifyChanges();
        }

        private void argumentbehaviour_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ignorechanges) return;

            Simple hook = Hook as Simple;
            hook.ArgumentBehaviour = (ArgumentBehaviour)argumentbehaviour.SelectedIndex;
            NotifyChanges();
        }

        private void argumentstring_TextChanged(object sender, EventArgs e)
        {
            if (ignorechanges) return;

            Simple hook = Hook as Simple;
            hook.ArgumentString = argumentstring.Text;
            NotifyChanges();
        }
    }
}
