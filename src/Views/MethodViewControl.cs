using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using Mono.Cecil;
using Oxide.Patcher.Hooks;
using Oxide.Patcher.Modifiers;
using Oxide.Patcher.Patching;
using System;
using System.Windows.Forms;
using Oxide.Patcher.Common;

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

        private Hook _methodHook;

        private Modifier _methodModifier;

        public MethodViewControl()
        {
            InitializeComponent();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            PopulateDetails();

            msiltab.Controls.Add(new TextEditorControl
            {
                IsReadOnly = true,
                Dock = DockStyle.Fill,
                Text = Decompiler.DecompileToIL(MethodDef.Body)
            });

            codetab.Controls.Add(new TextEditorControl
            {
                IsReadOnly = true,
                Dock = DockStyle.Fill,
                Text = await Decompiler.GetSourceCode(MethodDef),
                Document = { HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("C#") }
            });

            bool modifierFound = FindModifier();

            editbutton.Enabled = !modifierFound;
            gotoeditbutton.Enabled = modifierFound;

            if (!MethodDef.HasBody)
            {
                hookbutton.Enabled = false;
                gotohookbutton.Enabled = false;
                return;
            }

            bool hookFound = FindHook();

            hookbutton.Enabled = !hookFound;
            gotohookbutton.Enabled = hookFound;
        }

        private void PopulateDetails()
        {
            typenametextbox.Text = MethodDef.FullName;
            declarationtextbox.Text = Utility.GetMethodDeclaration(MethodDef);
        }

        private bool FindHook()
        {
            MethodSignature methodsig = Utility.GetMethodSignature(MethodDef);

            foreach (Manifest manifest in MainForm.CurrentProject.Manifests)
            {
                foreach (Hook hook in manifest.Hooks)
                {
                    if (hook.Signature.Equals(methodsig) && hook.TypeName == MethodDef.DeclaringType.FullName)
                    {
                        _methodHook = hook;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool FindModifier()
        {
            foreach (Manifest manifest in MainForm.CurrentProject.Manifests)
            {
                foreach (Modifier modifier in manifest.Modifiers)
                {
                    if (modifier.Signature.Equals(Utility.GetModifierSignature(MethodDef)) && modifier.TypeName == MethodDef.DeclaringType.FullName)
                    {
                        _methodModifier = modifier;
                        return true;
                    }
                }
            }

            return false;
        }

        #region -Actions-

        private void hookbutton_Click(object sender, EventArgs e)
        {
            Type defaultHookType = Hook.DefaultHookType;
            if (defaultHookType == null)
            {
                return;
            }

            Hook hook = Activator.CreateInstance(defaultHookType) as Hook;
            hook.Name = MethodDef.Name;
            hook.HookName = hook.Name.StartsWith("On") ? hook.Name : $"On{hook.Name}";
            hook.TypeName = MethodDef.DeclaringType.FullName;
            hook.AssemblyName = MainForm.AssemblyLoader.rassemblydict[MethodDef.Module.Assembly];
            hook.Signature = Utility.GetMethodSignature(MethodDef);
            hook.MSILHash = new ILWeaver(MethodDef.Body).Hash;

            MainForm.AddHook(hook);
            MainForm.GotoHook(hook);

            _methodHook = hook;

            hookbutton.Enabled = false;
            gotohookbutton.Enabled = true;
        }

        private void gotohookbutton_Click(object sender, EventArgs e)
        {
            MainForm.GotoHook(_methodHook);
        }

        private void editbutton_Click(object sender, EventArgs e)
        {
            Modifier modifier = new Modifier(MethodDef, MainForm.AssemblyLoader.rassemblydict[MethodDef.Module.Assembly]);

            MainForm.AddModifier(modifier);
            MainForm.GotoModifier(modifier);

            editbutton.Enabled = false;
            gotoeditbutton.Enabled = true;
        }

        private void gotoeditbutton_Click(object sender, EventArgs e)
        {
            MainForm.GotoModifier(_methodModifier);
        }

        #endregion
    }
}
