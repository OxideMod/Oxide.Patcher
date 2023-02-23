using System;
using System.Windows.Forms;

using Oxide.Patcher.Hooks;

namespace Oxide.Patcher.Views
{
    public class HookSettingsControl<T> : UserControl, IHookSettingsControl where T : Hook
    {
        /// <summary>
        /// Gets or sets the hook to use
        /// </summary>
        public T Hook { get; set; }

        /// <summary>
        /// Called when the settings have been changed by the user
        /// </summary>
        public event Action OnSettingsChanged;

        /// <summary>
        /// Raises the OnSettingsChanged event
        /// </summary>
        protected void NotifyChanges()
        {
            OnSettingsChanged?.Invoke();
        }
    }

    public interface IHookSettingsControl
    {
        DockStyle Dock { get; set; }
        event Action OnSettingsChanged;
    }
}
