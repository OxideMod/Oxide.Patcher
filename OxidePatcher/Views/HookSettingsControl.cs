using System;
using System.Windows.Forms;

using OxidePatcher.Hooks;

namespace OxidePatcher.Views
{
    public class HookSettingsControl : UserControl
    {
        /// <summary>
        /// Gets or sets the hook to use
        /// </summary>
        public Hook Hook { get; set; }

        /// <summary>
        /// Called when the settings have been changed by the user
        /// </summary>
        public event Action<HookSettingsControl> OnSettingsChanged;

        /// <summary>
        /// Raises the OnSettingsChanged event
        /// </summary>
        protected void NotifyChanges()
        {
            if (OnSettingsChanged != null)
                OnSettingsChanged(this);
        }
    }
}
