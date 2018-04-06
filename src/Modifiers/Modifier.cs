using Mono.Cecil;
using Newtonsoft.Json;
using Oxide.Patcher.Patching;
using Oxide.Patcher.Views;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Oxide.Patcher.Modifiers
{
    public enum Exposure { Private, Protected, Public, Internal, Static, Null }

    public enum ModifierType { Field, Method, Property, Type }

    /// <summary>
    /// Represents the signature of a method, field or property
    /// </summary>
    public sealed class ModifierSignature
    {
        /// <summary>
        /// Gets the exposure
        /// </summary>
        public Exposure[] Exposure { get; }

        /// <summary>
        /// Gets the name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the method return type or the field or property type as a fully qualified type name
        /// </summary>
        public string FullTypeName { get; }

        /// <summary>
        /// Gets the parameter list for methods as fully qualified type names
        /// </summary>
        public string[] Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the ModifierSignature class
        /// </summary>
        /// <param name="exposure"></param>
        /// <param name="fulltypename"></param>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        public ModifierSignature(Exposure exposure, string fulltypename, string name, string[] parameters)
        {
            Exposure = new[] { exposure };
            FullTypeName = fulltypename;
            Name = name;
            Parameters = parameters;
        }

        [JsonConstructor]
        public ModifierSignature(Exposure[] exposure, string fulltypename, string name, string[] parameters)
        {
            Exposure = exposure;
            FullTypeName = fulltypename;
            Name = name;
            Parameters = parameters;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ModifierSignature othersig))
            {
                return false;
            }

            if (Name != othersig.Name)
            {
                return false;
            }

            if (Exposure.Length != othersig.Exposure.Length)
            {
                return false;
            }

            for (int i = 0; i < Exposure.Length; i++)
            {
                if (Exposure[i] != othersig.Exposure[i])
                {
                    return false;
                }
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
            int total = Name.GetHashCode();
            foreach (Exposure exposure in Exposure)
            {
                total += exposure.GetHashCode();
            }

            foreach (string param in Parameters)
            {
                total += param.GetHashCode();
            }

            return total;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} {1} {2}(", Exposure.ToString().ToLower(), Utility.TransformType(FullTypeName), Name);
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
        public string MSILHash { get; set; }

        public Modifier()
        {
        }

        public Modifier(FieldDefinition field, string assembly)
        {
            Name = $"{field.DeclaringType}::{field.Name}";
            Type = ModifierType.Field;
            TypeName = field.DeclaringType.FullName;
            AssemblyName = assembly;
            Signature = Utility.GetModifierSignature(field);
            TargetExposure = Signature.Exposure;
            if (field.IsStatic)
            {
                List<Exposure> temp = TargetExposure.ToList();
                temp.Add(Exposure.Static);
                TargetExposure = temp.ToArray();
            }
            MSILHash = string.Empty;
        }

        public Modifier(MethodDefinition method, string assembly)
        {
            Name = $"{method.DeclaringType}::{method.Name}";
            Type = ModifierType.Method;
            TypeName = method.DeclaringType.FullName;
            AssemblyName = assembly;
            Signature = Utility.GetModifierSignature(method);
            TargetExposure = Signature.Exposure;
            if (method.IsStatic)
            {
                List<Exposure> temp = TargetExposure.ToList();
                temp.Add(Exposure.Static);
                TargetExposure = temp.ToArray();
            }
            MSILHash = new ILWeaver(method.Body).Hash;
        }

        public Modifier(PropertyDefinition property, string assembly)
        {
            Name = $"{property.DeclaringType}::{property.Name}";
            Type = ModifierType.Property;
            TypeName = property.DeclaringType.FullName;
            AssemblyName = assembly;
            Signature = Utility.GetModifierSignature(property);
            TargetExposure = Signature.Exposure;
            if ((property.GetMethod == null || property.GetMethod.IsStatic) && (property.SetMethod == null || property.SetMethod.IsStatic))
            {
                List<Exposure> temp = TargetExposure.ToList();
                temp.Add(Exposure.Static);
                TargetExposure = temp.ToArray();
            }
            MSILHash = string.Empty;
        }

        public Modifier(TypeDefinition type, string assembly)
        {
            Name = $"{type.FullName}";
            Type = ModifierType.Type;
            TypeName = type.FullName;
            AssemblyName = assembly;
            Signature = Utility.GetModifierSignature(type);
            TargetExposure = Signature.Exposure;
            if (type.IsAbstract && type.IsSealed)
            {
                List<Exposure> temp = TargetExposure.ToList();
                temp.Add(Exposure.Static);
                TargetExposure = temp.ToArray();
            }
            MSILHash = string.Empty;
        }

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
        /// Creates the settings view control for this hook
        /// </summary>
        /// <returns></returns>
        public ModifierSettingsControl CreateSettingsView()
        {
            return new ModifierSettingsControl { Modifier = this };
        }
    }
}
