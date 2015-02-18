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
    public partial class InitOxideHookSettingsControl : HookSettingsControl
    {
        private bool ignorechanges;

        public InitOxideHookSettingsControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            InitOxide hook = Hook as InitOxide;

            ignorechanges = true;
            injectionindex.Value = hook.InjectionIndex;
            ignorechanges = false;
        }

        private void injectionindex_ValueChanged(object sender, EventArgs e)
        {
            if (ignorechanges) return;

            InitOxide hook = Hook as InitOxide;
            hook.InjectionIndex = (int)injectionindex.Value;
            NotifyChanges();
        }
    }
}
