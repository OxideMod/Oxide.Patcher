using Mono.Cecil;
using Newtonsoft.Json;
using Oxide.Patcher.Patching;
using Oxide.Patcher.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Oxide.Patcher.Hooks
{
    public enum MethodExposure { Private, Protected, Public, Internal }

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

    /// <summary>
    /// Represents the signature of a method
    /// </summary>
    public sealed class MethodSignature
    {
        /// <summary>
        /// Gets the method exposure
        /// </summary>
        public MethodExposure Exposure { get; }

        /// <summary>
        /// Gets the method name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the method return type as a fully qualified type name
        /// </summary>
        public string ReturnType { get; }

        /// <summary>
        /// Gets the parameter list as fully qualified type names
        /// </summary>
        public string[] Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the MethodSignature class
        /// </summary>
        /// <param name="exposure"></param>
        /// <param name="returntype"></param>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        public MethodSignature(MethodExposure exposure, string returntype, string name, string[] parameters)
        {
            Exposure = exposure;
            ReturnType = returntype;
            Name = name;
            Parameters = parameters;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MethodSignature othersig))
            {
                return false;
            }

            if (Exposure != othersig.Exposure || Name != othersig.Name)
            {
                return false;
            }

            if (Parameters.Length != othersig.Parameters.Length)
            {
                return false;
            }

            for (int i = 0; i < Parameters.Length; i++)
            {
                if (Parameters[i] != othersig.Parameters[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int total = Exposure.GetHashCode() + Name.GetHashCode();
            foreach (string param in Parameters)
            {
                total += param.GetHashCode();
            }

            return total;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} {1} {2}(", Exposure.ToString().ToLower(), Utility.TransformType(ReturnType), Name);
            for (int i = 0; i < Parameters.Length; i++)
            {
                if (i > 0)
                {
                    sb.AppendFormat(", {0}", Parameters[i]);
                }
                else
                {
                    sb.Append(Parameters[i]);
                }
            }

            sb.Append(")");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Represents a hook that is applied to single method and calls a single Oxide hook
    /// </summary>
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
        /// Gets or sets the base hook
        /// </summary>
        [JsonIgnore]
        public Hook BaseHook { get; set; }

        /// <summary>
        /// Gets or sets the hook category
        /// </summary>
        public string HookCategory { get; set; }

        protected void ShowMsg(string msg, string header, Patching.Patcher patcher)
        {
            if (patcher != null)
            {
                patcher.Log(msg);
            }
            else
            {
                MessageBox.Show(msg, header, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// PrePatches this hook into the target weaver
        /// </summary>
        /// <param name="weaver"></param>
        /// <param name="oxidemodule"></param>
        /// <param name="original"></param>
        /// <param name="patcher"></param>
        public bool PreparePatch(MethodDefinition original, ILWeaver weaver, AssemblyDefinition oxidemodule, Patching.Patcher patcher = null)
        {
            if (BaseHook != null)
            {
                return BaseHook.PreparePatch(original, weaver, oxidemodule, patcher) && BaseHook.ApplyPatch(original, weaver, oxidemodule, patcher);
            }
            return true;
        }

        /// <summary>
        /// Patches this hook into the target weaver
        /// </summary>
        /// <param name="original"></param>
        /// <param name="weaver"></param>
        /// <param name="oxidemodule"></param>
        /// <param name="patcher"></param>
        public abstract bool ApplyPatch(MethodDefinition original, ILWeaver weaver, AssemblyDefinition oxidemodule, Patching.Patcher patcher = null);

        /// <summary>
        /// Creates the settings view control for this hook
        /// </summary>
        /// <returns></returns>
        public abstract HookSettingsControl CreateSettingsView();

        #region Static Interface

        private static Type[] hooktypes;
        private static Type defaulthooktype;

        static Hook()
        {
            Type basetype = typeof(Hook);
            hooktypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract)
                .Where(t => basetype.IsAssignableFrom(t))
                .Where(t => t.GetCustomAttribute<HookType>() != null)
                .ToArray();
            foreach (Type hooktype in hooktypes)
            {
                HookType type = hooktype.GetCustomAttribute<HookType>();
                if (type.Default)
                {
                    defaulthooktype = hooktype;
                    break;
                }
            }
        }

        /// <summary>
        /// Gets all hook types
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> GetHookTypes()
        {
            return hooktypes;
        }

        /// <summary>
        /// Gets a hook type by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Type GetHookType(string name)
        {
            foreach (Type type in hooktypes)
            {
                if (type.Name == name)
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the default hook type
        /// </summary>
        /// <returns></returns>
        public static Type GetDefaultHookType()
        {
            return defaulthooktype;
        }

        #endregion Static Interface
    }
}
