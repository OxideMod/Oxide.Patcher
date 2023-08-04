using Mono.Cecil;

using Newtonsoft.Json;

using Oxide.Patcher.Patching;
using Oxide.Patcher.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Oxide.Patcher.Common.JsonHelpers;

namespace Oxide.Patcher.Hooks
{
    /// <summary>
    /// Represents a hook that is applied to single method and calls a single Oxide hook
    /// </summary>
    [JsonConverter(typeof(HookConverter))]
    public abstract class Hook
    {
        /// <summary>
        /// Gets a human friendly type name for this hook
        /// </summary>
        public string HookTypeName => GetType().GetCustomAttribute<HookType>().Name;

        /// <summary>
        /// Gets or sets a name for this hook
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the Oxide hook to call
        /// </summary>
        public string HookName { get; set; }

        /// <summary>
        /// Gets or sets the description of the Oxide hook to call
        /// </summary>
        public string HookDescription { get; set; }

        /// <summary>
        /// Gets or sets the name of the assembly in which the target type resides
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified name for the type in which the target method resides
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets if this hook has been flagged
        /// </summary>
        public bool Flagged { get; set; }

        /// <summary>
        /// Gets or sets the target method signature
        /// </summary>
        public MethodSignature Signature { get; set; }

        /// <summary>
        /// Gets or sets the MSIL hash of the target method
        /// </summary>
        public string MSILHash { get; set; }

        /// <summary>
        /// Gets or sets the base hook name
        /// </summary>
        public string BaseHookName { get; set; }

        /// <summary>
        /// Gets or sets the hook category
        /// </summary>
        public string HookCategory { get; set; }

        /// <summary>
        /// Gets or sets the base hook
        /// </summary>
        [JsonIgnore]
        public Hook BaseHook { get; set; }

        /// <summary>
        /// Gets or sets the hook cloned from this one
        /// </summary>
        [JsonIgnore]
        public Hook ChildHook { get; set; }

        protected void ShowMessage(string message, string header, Patching.Patcher patcher)
        {
            if (patcher != null)
            {
                patcher.Log(message);
            }
            else
            {
                MessageBox.Show(message, header, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Prepares this hook for patching, ensuring all base hooks are patched first
        /// </summary>
        /// <param name="weaver"></param>
        /// <param name="original"></param>
        /// <param name="patcher"></param>
        public bool PreparePatch(MethodDefinition original, ILWeaver weaver, Patching.Patcher patcher = null)
        {
            if (BaseHook != null)
            {
                return BaseHook.PreparePatch(original, weaver, patcher) && BaseHook.ApplyPatch(original, weaver, patcher);
            }

            return true;
        }

        /// <summary>
        /// Patches this hook into the target weaver
        /// </summary>
        /// <param name="original"></param>
        /// <param name="weaver"></param>
        /// <param name="patcher"></param>
        public abstract bool ApplyPatch(MethodDefinition original, ILWeaver weaver, Patching.Patcher patcher = null);

        /// <summary>
        /// Creates the settings view control for this hook
        /// </summary>
        /// <returns></returns>
        public abstract IHookSettingsControl CreateSettingsView();

        #region Static Interface

        public static readonly Type[] HookTypes;
        public static readonly Type DefaultHookType;

        static Hook()
        {
            Type baseType = typeof(Hook);
            HookTypes = AppDomain.CurrentDomain.GetAssemblies()
                                 .SelectMany(GetAllTypesFromAssembly)
                                 .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t) && t.GetCustomAttribute<HookType>() != null)
                                 .ToArray();

            foreach (Type hookType in HookTypes)
            {
                HookType type = hookType.GetCustomAttribute<HookType>();
                if (type.Default)
                {
                    DefaultHookType = hookType;
                    break;
                }
            }
        }

        /// <summary>
        /// Gets a hook type by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Type GetHookType(string name)
        {
            foreach (Type type in HookTypes)
            {
                if (type.Name == name)
                {
                    return type;
                }
            }

            return null;
        }

        #endregion Static Interface

        private static IEnumerable<Type> GetAllTypesFromAssembly(Assembly asm)
        {
            foreach (Module module in asm.GetModules())
            {
                Type[] moduleTypes;

                try
                {
                    moduleTypes = module.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    moduleTypes = e.Types;
                }
                catch (Exception)
                {
                    moduleTypes = Type.EmptyTypes;
                }

                foreach (Type type in moduleTypes)
                {
                    if (type != null)
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}
