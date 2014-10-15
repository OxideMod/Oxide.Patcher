using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

using Mono.Cecil;

using ICSharpCode.TextEditor;

using OxidePatcher.Hooks;

namespace OxidePatcher
{
    public partial class MethodViewControl : UserControl
    {
        /// <summary>
        /// Gets or sets the method definition to use
        /// </summary>
        public MethodDefinition MethodDef { get; set; }

        /// <summary>
        /// Gets or sets the main patcher form
        /// </summary>
        public PatcherForm MainForm { get; set; }

        private TextEditorControl texteditor;

        private Hook methodhook;

        public MethodViewControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            PopulateDetails();

            texteditor = new TextEditorControl();
            texteditor.IsReadOnly = true;
            texteditor.Dock = DockStyle.Fill;
            texteditor.Text = Decompiler.DecompileToIL(MethodDef.Body);
            codepanel.Controls.Add(texteditor);

            if (!MethodDef.HasBody)
            {
                hookbutton.Enabled = false;
                gotohookbutton.Enabled = false;
                return;
            }

            MethodSignature methodsig = Utility.GetMethodSignature(MethodDef);

            bool hookfound = false;
            foreach (var manifest in MainForm.CurrentProject.Manifests)
            {
                foreach (var hook in manifest.Hooks)
                {
                    if (hook.Signature.Equals(methodsig))
                    {
                        hookfound = true;
                        methodhook = hook;
                        break;
                    }
                }
            }
            if (hookfound)
            {
                hookbutton.Enabled = false;
                gotohookbutton.Enabled = true;
            }
            else
            {
                hookbutton.Enabled = true;
                gotohookbutton.Enabled = false;
            }
        }

        private void PopulateDetails()
        {
            typenametextbox.Text = MethodDef.FullName;
            declarationtextbox.Text = Utility.GetMethodDeclaration(MethodDef);
        }

        private void hookbutton_Click(object sender, EventArgs e)
        {
            Type t = Hook.GetDefaultHookType();
            if (t == null) return;

            Hook hook = Activator.CreateInstance(t) as Hook;
            hook.Name = MethodDef.Name;
            if (hook.Name.Substring(0, 2) == "On")
                hook.HookName = hook.Name;
            else
                hook.HookName = "On" + hook.Name;
            hook.TypeName = MethodDef.DeclaringType.FullName;
            hook.AssemblyName = MethodDef.Module.Assembly.Name.Name;
            hook.Signature = Utility.GetMethodSignature(MethodDef);
            hook.MSILHash = new Patching.ILWeaver(MethodDef.Body).Hash;

            MainForm.AddHook(hook);
            MainForm.GotoHook(hook);

            methodhook = hook;

            hookbutton.Enabled = false;
            gotohookbutton.Enabled = true;
        }

        private void gotohookbutton_Click(object sender, EventArgs e)
        {
            MainForm.GotoHook(methodhook);
        }
    }
}
