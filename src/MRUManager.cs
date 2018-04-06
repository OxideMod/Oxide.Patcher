using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Oxide.Patcher
{
    public class MRUManager
    {
        /// <summary>
        /// Gets the current registry key that is being used to store the MRU list
        /// </summary>
        public string SubKeyName { get; }

        /// <summary>
        /// Gets the parent menu item
        /// </summary>
        public ToolStripMenuItem ParentMenuItem { get; }

        /// <summary>
        /// Gets the callback when a MRU entry is clicked
        /// </summary>
        public Action<object, EventArgs> OnRecentFileClick { get; }

        /// <summary>
        /// Gets the maximum amount of entries
        /// </summary>
        public int MaxEntries { get; }

        public MRUManager(ToolStripMenuItem parent, string appName, int entries, Action<object, EventArgs> onRecentFileClick = null)
        {
            ParentMenuItem = parent;
            SubKeyName = $"Software\\{appName}\\MRU";
            MaxEntries = entries;
            OnRecentFileClick = onRecentFileClick;

            Refresh();
        }

        /// <summary>
        /// Refreshes the MRU list
        /// </summary>
        private void Refresh()
        {
            RegistryKey registryKey;
            ToolStripItem toolStripItem;

            try
            {
                registryKey = Registry.CurrentUser.OpenSubKey(SubKeyName, false);
                if (registryKey == null)
                {
                    ParentMenuItem.Enabled = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open the recent files history:\n{ex}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ParentMenuItem.DropDownItems.Clear();
            string[] recentFiles = registryKey.GetValueNames();
            foreach (string file in recentFiles.Select(entry => (string)registryKey.GetValue(entry)).Where(file => file != null))
            {
                toolStripItem = ParentMenuItem.DropDownItems.Add(file);
                toolStripItem.Click += new EventHandler(OnRecentFileClick);
            }

            if (ParentMenuItem.DropDownItems.Count == 0)
            {
                ParentMenuItem.Enabled = false;
                return;
            }

            ParentMenuItem.DropDownItems.Add("-");
            toolStripItem = ParentMenuItem.DropDownItems.Add("Clear recent files");
            toolStripItem.Click += Clear;
            ParentMenuItem.Enabled = true;
        }

        /// <summary>
        /// Updates the MRU list order and adds the file when not present
        /// </summary>
        /// <param name="filename"></param>
        public void AddOrUpdate(string filename)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(SubKeyName,
                    RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (registryKey == null)
                {
                    return;
                }

                string[] recentFiles = registryKey.GetValueNames();
                List<string> newRecentFiles = new List<string> { filename };
                newRecentFiles.AddRange(
                    recentFiles.Select(file => (string)registryKey.GetValue(file, null))
                        .Where(sFile => sFile != filename && sFile != null));

                foreach (string file in recentFiles)
                {
                    registryKey.DeleteValue(file, true);
                }

                for (int i = 0; i < newRecentFiles.Count; i++)
                {
                    if (i >= MaxEntries)
                    {
                        break;
                    }

                    registryKey.SetValue(i.ToString(), newRecentFiles[i]);
                }

                registryKey.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update the recent files history:\n{ex}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Refresh();
        }

        /// <summary>
        /// Removes a file from the MRU list
        /// </summary>
        /// <param name="filename"></param>
        public void Remove(string filename)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(SubKeyName, true);
                if (registryKey == null)
                {
                    return;
                }

                string[] recentFiles = registryKey.GetValueNames();
                foreach (string file in recentFiles.Where(file => (string)registryKey.GetValue(file, null) == filename))
                {
                    registryKey.DeleteValue(file, true);
                    registryKey.Close();
                    break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove file from the recent files history:\n{ex}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Refresh();
        }

        /// <summary>
        /// Clears the MRU list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clear(object sender, EventArgs e)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(SubKeyName, true);
                if (registryKey == null)
                {
                    return;
                }

                string[] recentFiles = registryKey.GetValueNames();
                foreach (string file in recentFiles)
                {
                    registryKey.DeleteValue(file, true);
                }

                ParentMenuItem.DropDownItems.Clear();
                ParentMenuItem.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to clear the recent files history:\n{ex}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
