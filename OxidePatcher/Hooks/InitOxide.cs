using System;
using System.Linq;
using System.Windows.Forms;

using OxidePatcher.Patching;
using OxidePatcher.Views;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace OxidePatcher.Hooks
{
    /// <summary>
    /// The initialisation hook that loads Oxide
    /// </summary>
    [HookType("Initialize Oxide")]
    public class InitOxide : Hook
    {
        /// <summary>
        /// Gets or sets the instruction index to inject the hook call at
        /// </summary>
        public int InjectionIndex { get; set; }

        public override bool ApplyPatch(MethodDefinition original, ILWeaver weaver, AssemblyDefinition oxideassembly, bool console)
        {
            MethodDefinition initoxidemethod = oxideassembly.MainModule.Types
                .Single((t) => t.FullName == "Oxide.Core.Interface")
                .Methods.Single((m) => m.IsStatic && m.Name == "Initialize");

            // Start injecting where requested
            weaver.Pointer = InjectionIndex;

            // Get the existing instruction we're going to inject behind
            Instruction existing;
            try
            {
                existing = weaver.Instructions[weaver.Pointer];
            }
            catch (ArgumentOutOfRangeException)
            {
                if (console == false)
                {
                    MessageBox.Show(string.Format("The injection index specified for {0} is invalid!", this.Name), "Invalid Index", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            // Load the hook name
            Instruction firstinjected = weaver.Add(Instruction.Create(OpCodes.Call, weaver.Module.Import(initoxidemethod)));

            // Find all instructions which pointed to the existing and redirect them
            for (int i = 0; i < weaver.Instructions.Count; i++)
            {
                Instruction ins = weaver.Instructions[i];
                if (ins.Operand != null && ins.Operand.Equals(existing))
                {
                    // Check if the instruction lies within our injection range
                    // If it does, it's an instruction we just injected so we don't want to edit it
                    if (i < InjectionIndex || i > weaver.Pointer)
                        ins.Operand = firstinjected;
                }
            }
            return true;
        }

        public override Views.HookSettingsControl CreateSettingsView()
        {
            InitOxideHookSettingsControl control = new InitOxideHookSettingsControl();
            control.Hook = this;
            return control;
        }
    }
}
