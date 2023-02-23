using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using Mono.Cecil;
using Oxide.Patcher.Hooks;
using Oxide.Patcher.Patching;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Oxide.Patcher.Views
{
    public partial class HookViewControl : UserControl
    {
        /// <summary>
        /// Gets or sets the hook to use
        /// </summary>
        public Hook Hook { get; set; }

        /// <summary>
        /// Gets or sets the main patcher form
        /// </summary>
        public PatcherForm MainForm { get; set; }

        public Button FlagButton { get; set; }

        public Button UnflagButton { get; set; }

        private TextEditorControl _msilBefore, _msilAfter, _codeBefore, _codeAfter;

        private MethodDefinition _methodDef;

        private bool _loaded;

        public HookViewControl()
        {
            InitializeComponent();
            FlagButton = flagbutton;
            UnflagButton = unflagbutton;
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _methodDef = MainForm.GetMethod(Hook.AssemblyName, Hook.TypeName, Hook.Signature);

            InitialiseDropdowns();

            assemblytextbox.Text = Hook.AssemblyName;
            typenametextbox.Text = Hook.TypeName;

            methodnametextbox.Text = _methodDef != null ? Hook.Signature.ToString() : $"{Hook.Signature} (METHOD MISSING)";

            nametextbox.Text = Hook.Name;
            hooknametextbox.Text = Hook.HookName;

            applybutton.Enabled = false;

            flagbutton.Enabled = !Hook.Flagged;
            unflagbutton.Enabled = Hook.Flagged;

            LoadSettings();

            await LoadCodeViews();

            _loaded = true;
        }

        #region -Loading-

        private void InitialiseDropdowns()
        {
            for (int i = 0; i < Hook.HookTypes.Length; i++)
            {
                string typeName = Hook.HookTypes[i].GetCustomAttribute<HookType>().Name;

                hooktypedropdown.Items.Add(typeName);

                if (typeName == Hook.HookTypeName)
                {
                    hooktypedropdown.SelectedIndex = i;
                }
            }

            List<Hook> hooks = MainForm.CurrentProject.GetManifest(Hook.AssemblyName).Hooks;

            List<Hook> baseHooks = (from hook in hooks where hook.BaseHook != null select hook.BaseHook).ToList();

            //Add 'None' option
            basehookdropdown.Items.Add(string.Empty);

            foreach (Hook hook in hooks)
            {
                if (hook.BaseHook == Hook)
                {
                    clonebutton.Enabled = false;
                }

                if (hook != Hook.BaseHook && baseHooks.Contains(hook))
                {
                    continue;
                }

                basehookdropdown.Items.Add(hook.Name);

                if (hook == Hook.BaseHook)
                {
                    basehookdropdown.SelectedIndex = basehookdropdown.Items.Count - 1;
                }
            }
        }

        private void LoadSettings()
        {
            IHookSettingsControl settingsView = Hook.CreateSettingsView();
            if (settingsView == null)
            {
                Label label = new Label
                {
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = false,
                    Text = "No settings.",
                    Dock = DockStyle.Fill
                };

                hooksettingstab.Controls.Add(label);
            }
            else
            {
                settingsView.Dock = DockStyle.Fill;
                settingsView.OnSettingsChanged += settingsview_OnSettingsChanged;
                hooksettingstab.Controls.Add((Control)settingsView);
            }
        }

        private async Task LoadCodeViews()
        {
            if (_methodDef == null)
            {
                beforetab.Controls.Add(new Label
                {
                    Dock = DockStyle.Fill,
                    AutoSize = false,
                    Text = "METHOD MISSING",
                    TextAlign = ContentAlignment.MiddleCenter
                });

                aftertab.Controls.Add(new Label
                {
                    Dock = DockStyle.Fill,
                    AutoSize = false,
                    Text = "METHOD MISSING",
                    TextAlign = ContentAlignment.MiddleCenter
                });

                _loaded = true;
                return;
            }

            ILWeaver weaver = new ILWeaver(_methodDef.Body) { Module = _methodDef.Module };

            Hook.PreparePatch(_methodDef, weaver);

            _msilBefore = new TextEditorControl { Dock = DockStyle.Fill, Text = weaver.ToString(), IsReadOnly = true };
            _codeBefore = new TextEditorControl
            {
                Dock = DockStyle.Fill,
                Text = await Decompiler.GetSourceCode(_methodDef, weaver),
                Document = { HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("C#") },
                IsReadOnly = true
            };

            Hook.ApplyPatch(_methodDef, weaver);

            _msilAfter = new TextEditorControl { Dock = DockStyle.Fill, Text = weaver.ToString(), IsReadOnly = true };
            _codeAfter = new TextEditorControl
            {
                Dock = DockStyle.Fill,
                Text = await Decompiler.GetSourceCode(_methodDef, weaver),
                Document = { HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("C#") },
                IsReadOnly = true
            };

            beforetab.Controls.Add(_msilBefore);
            aftertab.Controls.Add(_msilAfter);
            codebeforetab.Controls.Add(_codeBefore);
            codeaftertab.Controls.Add(_codeAfter);
        }

        #endregion

        #region -Actions-

        private void settingsview_OnSettingsChanged()
        {
            applybutton.Enabled = true;
        }

        private void deletebutton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(MainForm, "Are you sure you want to remove this hook?", "Oxide Patcher", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                MainForm.RemoveHook(Hook);
            }
        }

        private void flagbutton_Click(object sender, EventArgs e)
        {
            Hook.Flagged = true;
            MainForm.UpdateHook(Hook, false);
            flagbutton.Enabled = false;
            unflagbutton.Enabled = true;
        }

        private void unflagbutton_Click(object sender, EventArgs e)
        {
            Hook.Flagged = false;
            MainForm.UpdateHook(Hook, false);
            if (Hook.Flagged)
            {
                return;
            }

            flagbutton.Enabled = true;
            unflagbutton.Enabled = false;
        }

        private void hooktypedropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loaded || hooktypedropdown.SelectedIndex < 0)
            {
                return;
            }

            Type hookType = Hook.HookTypes[hooktypedropdown.SelectedIndex];
            if (hookType == null)
            {
                return;
            }

            DialogResult result = MessageBox.Show(MainForm, "Are you sure you want to change the type of this hook? Any hook settings will be lost.",
                                                  "Oxide Patcher", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                MainForm.RemoveHook(Hook);

                Hook newHook = Activator.CreateInstance(hookType) as Hook;
                newHook.Name = Hook.Name;
                newHook.HookName = Hook.HookName;
                newHook.AssemblyName = Hook.AssemblyName;
                newHook.TypeName = Hook.TypeName;
                newHook.Signature = Hook.Signature;
                newHook.Flagged = Hook.Flagged;
                newHook.MSILHash = Hook.MSILHash;
                newHook.BaseHook = Hook.BaseHook;
                newHook.BaseHookName = Hook.BaseHookName;
                newHook.HookCategory = Hook.HookCategory;

                MainForm.AddHook(newHook);
                MainForm.GotoHook(newHook);
            }
        }

        private void nametextbox_TextChanged(object sender, EventArgs e)
        {
            applybutton.Enabled = true;
        }

        private void hooknametextbox_TextChanged(object sender, EventArgs e)
        {
            applybutton.Enabled = true;
        }

        private async void applybutton_Click(object sender, EventArgs e)
        {
            Hook.Name = nametextbox.Text;
            Hook.HookName = hooknametextbox.Text;

            MainForm.UpdateHook(Hook, false);

            if (_msilBefore != null && _msilAfter != null)
            {
                ILWeaver weaver = new ILWeaver(_methodDef.Body) { Module = _methodDef.Module };

                Hook.PreparePatch(_methodDef, weaver);
                _msilBefore.Text = weaver.ToString();
                _codeBefore.Text = await Decompiler.GetSourceCode(_methodDef, weaver);

                Hook.ApplyPatch(_methodDef, weaver);
                _msilAfter.Text = weaver.ToString();
                _codeAfter.Text = await Decompiler.GetSourceCode(_methodDef, weaver);
            }

            applybutton.Enabled = false;
        }

        private void basehookdropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (basehookdropdown.SelectedIndex < 0)
            {
                return;
            }

            string hookName = (string)basehookdropdown.SelectedItem;
            if (string.IsNullOrWhiteSpace(hookName))
            {
                Hook.BaseHook = null;
                return;
            }

            List<Hook> hooks = MainForm.CurrentProject.GetManifest(Hook.AssemblyName).Hooks;
            foreach (Hook hook in hooks)
            {
                if (!hook.Name.Equals(hookName))
                {
                    continue;
                }

                Hook.BaseHook = hook;
                break;
            }

            if (!Hook.BaseHook.Name.Equals(hookName))
            {
                MessageBox.Show(MainForm, "Base Hook not found!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void clonebutton_Click(object sender, EventArgs e)
        {
            Hook newHook = Activator.CreateInstance(Hook.GetType()) as Hook;
            newHook.Name = Hook.Name + "(Clone)";
            newHook.HookName = Hook.HookName + "(Clone)";
            newHook.AssemblyName = Hook.AssemblyName;
            newHook.TypeName = Hook.TypeName;
            newHook.Signature = Hook.Signature;
            newHook.Flagged = Hook.Flagged;
            newHook.MSILHash = Hook.MSILHash;
            newHook.BaseHook = Hook;

            MainForm.AddHook(newHook);
            MainForm.GotoHook(newHook);

            clonebutton.Enabled = false;
        }

        #endregion
    }
}
