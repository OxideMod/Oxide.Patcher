using System;
using System.Linq;
using System.Windows.Forms;

using OxidePatcher.Patching;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace OxidePatcher.Hooks
{
    /// <summary>
    /// The initialisation hook that loads Oxide
    /// </summary>
    [HookType("Initialise Oxide")]
    public class InitOxide : Hook
    {

        public override void ApplyPatch(MethodDefinition original, ILWeaver weaver, AssemblyDefinition oxideassembly)
        {
            MethodDefinition initoxidemethod = oxideassembly.MainModule.Types
                .Single((t) => t.FullName == "Oxide.Core.Interface")
                .Methods.Single((m) => m.IsStatic && m.Name == "Initialise");

            weaver.Pointer = 0;
            weaver.Add(Instruction.Create(OpCodes.Call, weaver.Module.Import(initoxidemethod)));
        }

        public override Views.HookSettingsControl CreateSettingsView()
        {
            return null;
        }
    }
}
