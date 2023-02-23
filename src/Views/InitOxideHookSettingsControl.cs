using System;
using Oxide.Patcher.Hooks;

namespace Oxide.Patcher.Views
{
    public partial class InitOxideHookSettingsControl : HookSettingsControl<InitOxide>
    {
        private bool _loaded;

        public InitOxideHookSettingsControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            injectionindex.Value = Hook.InjectionIndex;

            _loaded = true;
        }

        private void injectionindex_ValueChanged(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            Hook.InjectionIndex = (int)injectionindex.Value;
            NotifyChanges();
        }
    }
}
