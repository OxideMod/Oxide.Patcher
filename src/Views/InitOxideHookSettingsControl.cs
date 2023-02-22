using System;
using System.Windows.Forms;
using Oxide.Patcher.Hooks;

namespace Oxide.Patcher.Views
{
    public partial class InitOxideHookSettingsControl : HookSettingsControl
    {
        private bool _loaded;

        public InitOxideHookSettingsControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!(Hook is InitOxide hook))
            {
                MessageBox.Show("Invalid Hook Type", "Hook in init oxide view is not an InitOxide hook");
                return;
            }

            injectionindex.Value = hook.InjectionIndex;

            _loaded = true;
        }

        private void injectionindex_ValueChanged(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            if (!(Hook is InitOxide hook))
            {
                MessageBox.Show("Unexpected Error", "Hook in init oxide view is not an InitOxide hook");
                return;
            }

            hook.InjectionIndex = (int)injectionindex.Value;
            NotifyChanges();
        }
    }
}
