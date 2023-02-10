using System;

namespace Oxide.Patcher.Hooks
{
    /// <summary>
    /// Indicates details about the hook type for use with UI
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class HookType : Attribute
    {
        /// <summary>
        /// Gets a human-friendly name for this hook type
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets if this hook type should be used as the default
        /// </summary>
        public bool Default { get; set; }

        public HookType(string name)
        {
            Name = name;
        }
    }
}
