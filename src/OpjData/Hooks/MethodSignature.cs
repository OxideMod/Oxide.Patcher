using System.Text;
using Oxide.Patcher.Common;

namespace Oxide.Patcher.Hooks
{
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
}
