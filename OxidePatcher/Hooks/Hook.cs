using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

using OxidePatcher.Patching;

using Newtonsoft.Json;

using Mono.Cecil;

namespace OxidePatcher.Hooks
{
    public enum MethodExposure { Private, Protected, Public, Internal }

    /// <summary>
    /// Indicates details about the hook type for use with UI
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class HookType : Attribute
    {
        /// <summary>
        /// Gets a human-friendly name for this hook type
        /// </summary>
        public string Name { get; private set; }

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
        public MethodExposure Exposure { get; private set; }

        /// <summary>
        /// Gets the method name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the method return type as a fully qualified type name
        /// </summary>
        public string ReturnType { get; private set; }

        /// <summary>
        /// Gets the parameter list as fully qualified type names
        /// </summary>
        public string[] Parameters { get; private set; }

        /// <summary>
        /// Initialises a new instance of the MethodSignature class
        /// </summary>
        /// <param name="exposure"></param>
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
            MethodSignature othersig = obj as MethodSignature;
            if (othersig == null) return false;
            if (Exposure != othersig.Exposure || Name != othersig.Name) return false;
            if (Parameters.Length != othersig.Parameters.Length) return false;
            for (int i = 0; i < Parameters.Length; i++)
                if (Parameters[i] != othersig.Parameters[i])
                    return false;
            return true;
        }

        public override int GetHashCode()
        {
            int total = Exposure.GetHashCode() + Name.GetHashCode();
            foreach (var param in Parameters)
                total += param.GetHashCode();
            return total;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} {1} {2}(", Exposure.ToString().ToLower(), Utility.TransformType(ReturnType),  Name);
            for (int i = 0; i < Parameters.Length; i++)
                if (i > 0)
                    sb.AppendFormat(", {0}", Parameters[i]);
                else
                    sb.Append(Parameters[i]);
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
        public string HookTypeName
        {
            get
            {
                return GetType().GetCustomAttribute<HookType>().Name;
            }
        }

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
        /// Patches this hook into the target weaver
        /// </summary>
        /// <param name="targetmethod"></param>
        /// <param name="oxidemodule"></param>
        public abstract void ApplyPatch(MethodDefinition original, ILWeaver weaver, AssemblyDefinition oxidemodule);

        /// <summary>
        /// Creates the settings view control for this hook
        /// </summary>
        /// <returns></returns>
        public abstract Views.HookSettingsControl CreateSettingsView();

        #region Static Interface

        private static Type[] hooktypes;
        private static Type defaulthooktype;

        static Hook()
        {
            Type basetype = typeof(Hook);
            hooktypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany((a) => a.GetTypes())
                .Where((t) => !t.IsAbstract)
                .Where((t) => basetype.IsAssignableFrom(t))
                .Where((t) => t.GetCustomAttribute<HookType>() != null)
                .ToArray();
            foreach (var hooktype in hooktypes)
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
            foreach (var type in hooktypes)
                if (type.Name == name)
                    return type;
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

        #endregion
    }
}
