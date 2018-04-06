using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using Mono.Cecil;
using Oxide.Patcher.Hooks;
using Oxide.Patcher.Modifiers;
using Oxide.Patcher.Patching;
using System;
using System.Windows.Forms;

namespace Oxide.Patcher
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

        private Hook methodhook;

        private Modifier methodmodifier;

        public MethodViewControl()
        {
            InitializeComponent();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            PopulateDetails();

            TextEditorControl msilEditor = new TextEditorControl
            {
                IsReadOnly = true,
                Dock = DockStyle.Fill,
                Text = Decompiler.DecompileToIL(MethodDef.Body)
            };
            msiltab.Controls.Add(msilEditor);

            TextEditorControl codeEditor = new TextEditorControl
            {
                IsReadOnly = true,
                Dock = DockStyle.Fill,
                Text = await Decompiler.GetSourceCode(MethodDef),
                Document = { HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("C#") }
            };
            codetab.Controls.Add(codeEditor);

            if (!MethodDef.HasBody)
            {
                hookbutton.Enabled = false;
                gotohookbutton.Enabled = false;
                return;
            }

            MethodSignature methodsig = Utility.GetMethodSignature(MethodDef);

            bool hookfound = false;
            foreach (Manifest manifest in MainForm.CurrentProject.Manifests)
            {
                foreach (Hook hook in manifest.Hooks)
                {
                    if (hook.Signature.Equals(methodsig) && hook.TypeName == MethodDef.DeclaringType.FullName)
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

            bool modifierfound = false;
            foreach (Manifest manifest in MainForm.CurrentProject.Manifests)
            {
                foreach (Modifier modifier in manifest.Modifiers)
                {
                    if (modifier.Signature.Equals(Utility.GetModifierSignature(MethodDef)) && modifier.TypeName == MethodDef.DeclaringType.FullName)
                    {
                        modifierfound = true;
                        methodmodifier = modifier;
                        break;
                    }
                }
            }
            if (modifierfound)
            {
                editbutton.Enabled = false;
                gotoeditbutton.Enabled = true;
            }
            else
            {
                editbutton.Enabled = true;
                gotoeditbutton.Enabled = false;
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
            if (t == null)
            {
                return;
            }

            Hook hook = Activator.CreateInstance(t) as Hook;
            hook.Name = MethodDef.Name;
            if (hook.Name.StartsWith("On"))
            {
                hook.HookName = hook.Name;
            }
            else
            {
                hook.HookName = "On" + hook.Name;
            }

            hook.TypeName = MethodDef.DeclaringType.FullName;
            hook.AssemblyName = MainForm.rassemblydict[MethodDef.Module.Assembly];
            hook.Signature = Utility.GetMethodSignature(MethodDef);
            hook.MSILHash = new ILWeaver(MethodDef.Body).Hash;

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

        private void editbutton_Click(object sender, EventArgs e)
        {
            Modifier modifier = new Modifier(MethodDef, MainForm.rassemblydict[MethodDef.Module.Assembly]);

            MainForm.AddModifier(modifier);
            MainForm.GotoModifier(modifier);

            editbutton.Enabled = false;
            gotoeditbutton.Enabled = true;
        }

        private void gotoeditbutton_Click(object sender, EventArgs e)
        {
            MainForm.GotoModifier(methodmodifier);
        }
    }
}
