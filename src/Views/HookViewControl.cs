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

        private List<Type> hooktypes;

        private TextEditorControl msilbefore, msilafter, codebefore, codeafter;

        private MethodDefinition methoddef;

        private bool ignoretypechange;

        public HookViewControl()
        {
            InitializeComponent();
            FlagButton = flagbutton;
            UnflagButton = unflagbutton;
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            methoddef = MainForm.GetMethod(Hook.AssemblyName, Hook.TypeName, Hook.Signature);

            hooktypes = new List<Type>();
            int selindex = 0;
            int i = 0;
            foreach (Type hooktype in Hook.GetHookTypes())
            {
                string typename = hooktype.GetCustomAttribute<HookType>().Name;
                hooktypedropdown.Items.Add(typename);
                hooktypes.Add(hooktype);
                if (typename == Hook.HookTypeName)
                {
                    selindex = i;
                }

                i++;
            }

            List<Hook> hooks = MainForm.CurrentProject.GetManifest(Hook.AssemblyName).Hooks;
            List<Hook> baseHooks = (from hook in hooks where hook.BaseHook != null select hook.BaseHook).ToList();
            basehookdropdown.Items.Add("");
            int selindex2 = 0;
            i = 1;
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
                    selindex2 = i;
                }

                i++;
            }

            assemblytextbox.Text = Hook.AssemblyName;
            typenametextbox.Text = Hook.TypeName;

            if (methoddef != null)
            {
                methodnametextbox.Text = Hook.Signature.ToString();
            }
            else
            {
                methodnametextbox.Text = Hook.Signature + " (METHOD MISSING)";
            }

            nametextbox.Text = Hook.Name;
            hooknametextbox.Text = Hook.HookName;
            ignoretypechange = true;
            hooktypedropdown.SelectedIndex = selindex;
            basehookdropdown.SelectedIndex = selindex2;
            ignoretypechange = false;

            applybutton.Enabled = false;

            if (Hook.Flagged)
            {
                flagbutton.Enabled = false;
                unflagbutton.Enabled = true;
                unflagbutton.Focus();
            }
            else
            {
                flagbutton.Enabled = true;
                unflagbutton.Enabled = false;
                flagbutton.Focus();
            }

            HookSettingsControl settingsview = Hook.CreateSettingsView();
            if (settingsview == null)
            {
                Label tmp = new Label();
                tmp.TextAlign = ContentAlignment.MiddleCenter;
                tmp.AutoSize = false;
                tmp.Text = "No settings.";
                tmp.Dock = DockStyle.Fill;
                hooksettingstab.Controls.Add(tmp);
            }
            else
            {
                settingsview.Dock = DockStyle.Fill;
                settingsview.OnSettingsChanged += settingsview_OnSettingsChanged;
                hooksettingstab.Controls.Add(settingsview);
            }

            if (methoddef == null)
            {
                Label missinglabel1 = new Label();
                missinglabel1.Dock = DockStyle.Fill;
                missinglabel1.AutoSize = false;
                missinglabel1.Text = "METHOD MISSING";
                missinglabel1.TextAlign = ContentAlignment.MiddleCenter;
                beforetab.Controls.Add(missinglabel1);

                Label missinglabel2 = new Label();
                missinglabel2.Dock = DockStyle.Fill;
                missinglabel2.AutoSize = false;
                missinglabel2.Text = "METHOD MISSING";
                missinglabel2.TextAlign = ContentAlignment.MiddleCenter;
                aftertab.Controls.Add(missinglabel2);

                return;
            }

            ILWeaver weaver = new ILWeaver(methoddef.Body) { Module = methoddef.Module };

            Hook.PreparePatch(methoddef, weaver, MainForm.OxideAssembly);
            msilbefore = new TextEditorControl { Dock = DockStyle.Fill, Text = weaver.ToString(), IsReadOnly = true };
            codebefore = new TextEditorControl
            {
                Dock = DockStyle.Fill,
                Text = await Decompiler.GetSourceCode(methoddef, weaver),
                Document = { HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("C#") },
                IsReadOnly = true
            };

            Hook.ApplyPatch(methoddef, weaver, MainForm.OxideAssembly);
            msilafter = new TextEditorControl { Dock = DockStyle.Fill, Text = weaver.ToString(), IsReadOnly = true };
            codeafter = new TextEditorControl
            {
                Dock = DockStyle.Fill,
                Text = await Decompiler.GetSourceCode(methoddef, weaver),
                Document = { HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("C#") },
                IsReadOnly = true
            };

            beforetab.Controls.Add(msilbefore);
            aftertab.Controls.Add(msilafter);
            codebeforetab.Controls.Add(codebefore);
            codeaftertab.Controls.Add(codeafter);
        }

        private void settingsview_OnSettingsChanged(HookSettingsControl obj)
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
            if (hooktypedropdown.SelectedIndex < 0)
            {
                return;
            }

            if (ignoretypechange)
            {
                return;
            }

            Type t = hooktypes[hooktypedropdown.SelectedIndex];
            if (t == null)
            {
                return;
            }

            DialogResult result = MessageBox.Show(MainForm, "Are you sure you want to change the type of this hook? Any hook settings will be lost.", "Oxide Patcher", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                MainForm.RemoveHook(Hook);
                Hook newhook = Activator.CreateInstance(t) as Hook;
                newhook.Name = Hook.Name;
                newhook.HookName = Hook.HookName;
                newhook.AssemblyName = Hook.AssemblyName;
                newhook.TypeName = Hook.TypeName;
                newhook.Signature = Hook.Signature;
                newhook.Flagged = Hook.Flagged;
                newhook.MSILHash = Hook.MSILHash;
                newhook.BaseHook = Hook.BaseHook;
                newhook.BaseHookName = Hook.BaseHookName;
                newhook.HookCategory = Hook.HookCategory;
                MainForm.AddHook(newhook);
                MainForm.GotoHook(newhook);
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

            if (msilbefore != null && msilafter != null)
            {
                ILWeaver weaver = new ILWeaver(methoddef.Body) { Module = methoddef.Module };

                Hook.PreparePatch(methoddef, weaver, MainForm.OxideAssembly);
                msilbefore.Text = weaver.ToString();
                codebefore.Text = await Decompiler.GetSourceCode(methoddef, weaver);

                Hook.ApplyPatch(methoddef, weaver, MainForm.OxideAssembly);
                msilafter.Text = weaver.ToString();
                codeafter.Text = await Decompiler.GetSourceCode(methoddef, weaver);
            }

            applybutton.Enabled = false;
        }

        private void basehookdropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (basehookdropdown.SelectedIndex < 0)
            {
                return;
            }

            if (ignoretypechange)
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
                if (hook.Name.Equals(hookName))
                {
                    Hook.BaseHook = hook;
                    break;
                }
            }
            if (!Hook.BaseHook.Name.Equals(hookName))
            {
                MessageBox.Show(MainForm, "Base Hook not found!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void clonebutton_Click(object sender, EventArgs e)
        {
            Hook newhook = Activator.CreateInstance(Hook.GetType()) as Hook;
            newhook.Name = Hook.Name + "(Clone)";
            newhook.HookName = Hook.HookName + "(Clone)";
            newhook.AssemblyName = Hook.AssemblyName;
            newhook.TypeName = Hook.TypeName;
            newhook.Signature = Hook.Signature;
            newhook.Flagged = Hook.Flagged;
            newhook.MSILHash = Hook.MSILHash;
            newhook.BaseHook = Hook;
            MainForm.AddHook(newhook);
            MainForm.GotoHook(newhook);
            clonebutton.Enabled = false;
        }
    }
}
