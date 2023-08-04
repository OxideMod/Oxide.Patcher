using Mono.Cecil;

using Oxide.Patcher.Patching;
using Oxide.Patcher.Views;

using System.Linq;
using Oxide.Patcher.Common;

namespace Oxide.Patcher.Modifiers
{
    /// <summary>
    /// Represents a hook that is applied to single method and calls a single Oxide hook
    /// </summary>
    public class Modifier
    {
        /// <summary>
        /// Gets or sets a name for this modifier
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the assembly in which the target resides
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified name for the type in which the target resides
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the type of the target
        /// </summary>
        public ModifierType Type { get; set; }

        /// <summary>
        /// Gets the target exposure
        /// </summary>
        public Exposure[] TargetExposure { get; set; }

        /// <summary>
        /// Gets or sets if this modifier has been flagged
        /// </summary>
        public bool Flagged { get; set; }

        /// <summary>
        /// Gets or sets the target signature
        /// </summary>
        public ModifierSignature Signature { get; set; }

        /// <summary>
        /// Gets or sets the MSIL hash of the target
        /// </summary>
        public string MSILHash { get; set; } = string.Empty;

        public Modifier() { }

        private Modifier(MemberReference memberRef, string assemblyName, string typeName)
        {
            Name = $"{memberRef.DeclaringType}::{memberRef.Name}";
            TypeName = typeName;
            AssemblyName = assemblyName;
            Signature = Utility.GetModifierSignature(memberRef);
            TargetExposure = Signature.Exposure;
        }

        public Modifier(FieldDefinition field, string assembly) : this(field, assembly, field.DeclaringType.FullName)
        {
            Type = ModifierType.Field;

            if (field.IsStatic)
            {
                AddStaticExposure();
            }
        }

        public Modifier(MethodDefinition method, string assembly) : this(method, assembly, method.DeclaringType.FullName)
        {
            Type = ModifierType.Method;

            if (method.IsStatic)
            {
                AddStaticExposure();
            }

            MSILHash = new ILWeaver(method.Body).Hash;
        }

        public Modifier(PropertyDefinition property, string assembly) : this(property, assembly, property.DeclaringType.FullName)
        {
            Type = ModifierType.Property;

            if (property.GetMethod?.IsStatic != false && (property.SetMethod == null || property.SetMethod.IsStatic))
            {
                AddStaticExposure();
            }
        }

        public Modifier(TypeDefinition type, string assembly) : this(type, assembly, type.FullName)
        {
            Name = type.FullName;
            Type = ModifierType.Type;

            if (type.IsAbstract && type.IsSealed)
            {
                AddStaticExposure();
            }
        }

        private void AddStaticExposure()
        {
            Exposure[] exposures = new Exposure[TargetExposure.Length + 1];
            exposures[TargetExposure.Length] = Exposure.Static;

            for (int i = 0; i < TargetExposure.Length; i++)
            {
                exposures[i] = TargetExposure[i];
            }

            TargetExposure = exposures;
        }

        public bool HasTargetExposure(Exposure exposure)
        {
            return TargetExposure?.Any(x => x == exposure) ?? false;
        }

        /// <summary>
        /// Creates the settings view control for this hook
        /// </summary>
        /// <returns></returns>
        public ModifierSettingsControl CreateSettingsView()
        {
            return new ModifierSettingsControl { Modifier = this };
        }
    }
}
