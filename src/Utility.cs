using Mono.Cecil;
using Oxide.Patcher.Hooks;
using Oxide.Patcher.Modifiers;
using System.Linq;

namespace Oxide.Patcher
{
    /// <summary>
    /// Contains helpful utility methods
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Transforms the specified type name into a more human readable name
        /// </summary>
        /// <param name="old"></param>
        /// <returns></returns>
        public static string TransformType(string old)
        {
            const string prefix = "System.";
            if (old.Length >= prefix.Length && old.Substring(0, prefix.Length) == prefix)
            {
                string smallertype = old.Substring(prefix.Length);
                string newtype = TransformType(smallertype);
                if (newtype == smallertype)
                {
                    return old;
                }

                return newtype;
            }

            switch (old)
            {
                case "String":
                    return "string";

                case "Integer":
                    return "int";

                case "Boolean":
                    return "bool";

                case "Object":
                    return "object";

                case "UInt16":
                    return "ushort";

                case "UInt32":
                    return "uint";

                case "UInt64":
                    return "ulong";

                case "Int16":
                    return "short";

                case "Int32":
                    return "int";

                case "Int64":
                    return "long";

                case "Byte":
                    return "byte";

                case "Void":
                    return "void";

                case "Single":
                    return "float";

                case "Double":
                    return "double";

                default:
                    return old;
            }
        }

        /// <summary>
        /// Gets the C# qualifier string for the specified method
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string GetMethodQualifier(MethodDefinition method)
        {
            string qualifier;
            if (method.IsStatic)
            {
                qualifier = method.IsPublic ? "public static" : method.IsPrivate ? "private static" : "internal static";
            }
            else if (method.IsAbstract)
            {
                qualifier = method.IsPublic ? "public abstract" : method.IsPrivate ? "private abstract" : method.IsFamilyOrAssembly || method.IsFamily ? "protected abstract" : "internal abstract";
            }
            else if (method.IsVirtual)
            {
                qualifier = method.IsPublic ? "public virtual" : method.IsPrivate ? "private virtual" : method.IsFamilyOrAssembly || method.IsFamily ? "protected virtual" : "internal virtual";
            }
            else
            {
                qualifier = method.IsPublic ? "public" : method.IsPrivate ? "private" : "protected";
            }

            return qualifier;
        }

        /// <summary>
        /// Gets the C# method string for the specified method
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string GetMethodDeclaration(MethodDefinition method)
        {
            string qualifier = GetMethodQualifier(method);
            string[] args = method.Parameters.Select(x => $"{TransformType(x.ParameterType.Name)} {x.Name}").ToArray();
            string name;
            if (method.Name == ".ctor" || method.Name == ".cctor")
            {
                name = $"{qualifier} {method.DeclaringType.Name}({string.Join(", ", args)})";
            }
            else
            {
                name = $"{qualifier} {TransformType(method.ReturnType.Name)} {method.Name}({string.Join(", ", args)})";
            }

            return name;
        }

        /// <summary>
        /// Gets a method signature for the specified method
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static MethodSignature GetMethodSignature(MethodDefinition method)
        {
            MethodExposure exposure;
            if (method.IsPublic)
            {
                exposure = MethodExposure.Public;
            }
            else if (method.IsPrivate)
            {
                exposure = MethodExposure.Private;
            }
            else if (method.IsFamilyOrAssembly || method.IsFamily)
            {
                exposure = MethodExposure.Protected;
            }
            else
            {
                exposure = MethodExposure.Internal;
            }

            string[] parameters = new string[method.Parameters.Count];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] = method.Parameters[i].ParameterType.FullName;
            }
            return new MethodSignature(exposure, method.ReturnType.FullName, method.Name, parameters);
        }

        /// <summary>
        /// Gets a signature for the specified field
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static ModifierSignature GetModifierSignature(FieldDefinition field)
        {
            Exposure exposure;
            if (field.IsPublic)
            {
                exposure = Exposure.Public;
            }
            else if (field.IsPrivate)
            {
                exposure = Exposure.Private;
            }
            else if (field.IsFamilyOrAssembly || field.IsFamily)
            {
                exposure = Exposure.Protected;
            }
            else
            {
                exposure = Exposure.Internal;
            }

            return new ModifierSignature(exposure, field.FullName, field.Name, new string[0]);
        }

        /// <summary>
        /// Gets a signature for the specified method
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static ModifierSignature GetModifierSignature(MethodDefinition method)
        {
            Exposure exposure;
            if (method.IsPublic)
            {
                exposure = Exposure.Public;
            }
            else if (method.IsPrivate)
            {
                exposure = Exposure.Private;
            }
            else if (method.IsFamilyOrAssembly || method.IsFamily)
            {
                exposure = Exposure.Protected;
            }
            else
            {
                exposure = Exposure.Internal;
            }

            string[] parameters = new string[method.Parameters.Count];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] = method.Parameters[i].ParameterType.FullName;
            }

            return new ModifierSignature(exposure, method.ReturnType.FullName, method.Name, parameters);
        }

        /// <summary>
        /// Gets a signature for the specified property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static ModifierSignature GetModifierSignature(PropertyDefinition property)
        {
            Exposure getExposure = Exposure.Null;
            Exposure setExposure = Exposure.Null;

            if (property.GetMethod != null)
            {
                if (property.GetMethod.IsPublic)
                {
                    getExposure = Exposure.Public;
                }
                else if (property.GetMethod.IsPrivate)
                {
                    getExposure = Exposure.Private;
                }
                else if (property.GetMethod.IsFamilyOrAssembly || property.GetMethod.IsFamily)
                {
                    getExposure = Exposure.Protected;
                }
                else
                {
                    getExposure = Exposure.Protected;
                }
            }

            if (property.SetMethod != null)
            {
                if (property.SetMethod.IsPublic)
                {
                    setExposure = Exposure.Public;
                }
                else if (property.SetMethod.IsPrivate)
                {
                    setExposure = Exposure.Private;
                }
                else if (property.SetMethod.IsFamilyOrAssembly || property.SetMethod.IsFamily)
                {
                    setExposure = Exposure.Protected;
                }
                else
                {
                    setExposure = Exposure.Protected;
                }
            }

            return new ModifierSignature(new[] { getExposure, setExposure }, property.FullName, property.Name, new string[0]);
        }

        /// <summary>
        /// Gets a signature for the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ModifierSignature GetModifierSignature(TypeDefinition type)
        {
            Exposure exposure = Exposure.Null;
            if (type.IsPublic || type.IsNestedPublic)
            {
                exposure = Exposure.Public;
            }
            else if (type.IsNotPublic || type.IsNestedPrivate)
            {
                exposure = Exposure.Private;
            }

            return new ModifierSignature(exposure, type.FullName, type.Name, new string[0]);
        }
    }
}
