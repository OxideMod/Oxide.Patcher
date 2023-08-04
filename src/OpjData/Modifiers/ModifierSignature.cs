using System.Text;
using Newtonsoft.Json;
using Oxide.Patcher.Common;

namespace Oxide.Patcher.Modifiers
{
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
}
