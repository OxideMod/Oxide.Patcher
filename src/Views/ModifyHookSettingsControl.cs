using Mono.Cecil;
using Oxide.Patcher.Hooks;
using System;
using System.Windows.Forms;

namespace Oxide.Patcher.Views
{
    public partial class ModifyHookSettingsControl : HookSettingsControl
    {
        private bool ignorechanges;
        private MethodDefinition method;
        public ModifyHookSettingsControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            method = PatcherForm.MainForm.GetMethod(Hook.AssemblyName, Hook.TypeName, Hook.Signature);

            Modify hook = (Modify)Hook;

            ignorechanges = true;
            injectionindex.Value = hook.InjectionIndex;
            removecount.Maximum = method.Body.Instructions.Count - 1;
            removecount.Value = hook.RemoveCount;
            illist.DataSource = new BindingSource { DataSource = hook.Instructions };
            ignorechanges = false;
        }

        private void injectionindex_ValueChanged(object sender, EventArgs e)
        {
            if (ignorechanges)
            {
                return;
            }

            Modify hook = (Modify)Hook;
            hook.InjectionIndex = (int)injectionindex.Value;
            NotifyChanges();
        }

        private void removecount_ValueChanged(object sender, EventArgs e)
        {
            if (ignorechanges)
            {
                return;
            }

            Modify hook = (Modify)Hook;
            hook.RemoveCount = (int)removecount.Value;
            NotifyChanges();
        }

        private void illist_SelectedIndexChanged(object sender, EventArgs e)
        {
            EditToolStripMenuItem.Enabled = illist.SelectedIndex >= 0;
            RemoveToolStripMenuItem.Enabled = illist.SelectedIndex >= 0;
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ModifyForm form = new ModifyForm((Modify)Hook, method))
            {
                if (form.ShowDialog(PatcherForm.MainForm) != DialogResult.OK)
                {
                    return;
                }

                if (form.Instruction == null)
                {
                    return;
                }

                int index = illist.SelectedIndex + form.Index;
                index = index >= 0 ? index > illist.Items.Count ? illist.Items.Count - 1 : index : 0;
                Modify hook = (Modify)Hook;
                hook.Instructions.Insert(index, form.Instruction);
                ((BindingSource)illist.DataSource).ResetBindings(false);
                NotifyChanges();
            }
        }

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Modify.InstructionData inst = (Modify.InstructionData)illist.SelectedItem;
            if (inst == null)
            {
                return;
            }

            using (ModifyForm form = new ModifyForm((Modify)Hook, method, inst))
            {
                if (form.ShowDialog(PatcherForm.MainForm) != DialogResult.OK)
                {
                    return;
                } ((BindingSource)illist.DataSource).ResetBindings(false);
                NotifyChanges();
            }
        }

        private void RemoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Modify hook = (Modify)Hook;
            hook.Instructions.RemoveAt(illist.SelectedIndex);
            ((BindingSource)illist.DataSource).ResetBindings(false);
            NotifyChanges();
        }
    }
}
