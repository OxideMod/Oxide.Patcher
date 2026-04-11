using System;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Oxide.Patcher.Hooks;
using Oxide.Patcher.Modifiers;

namespace Oxide.Patcher.Common
{
    /// <summary>
    /// Contains helpful utility methods
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Gets a human readable type name from a Mono.Cecil TypeReference
        /// </summary>
        public static string GetReadableTypeName(TypeReference typeRef)
        {
            if (typeRef == null)
            {
                return "Unknown";
            }

            if (typeRef is ByReferenceType byRef)
            {
                return GetReadableTypeName(byRef.ElementType);
            }

            if (typeRef is ArrayType arrayType)
            {
                return GetReadableTypeName(arrayType.ElementType) + "[]";
            }

            if (typeRef is GenericInstanceType genericType)
            {
                string baseName = StripBacktick(genericType.ElementType.Name);
                if (genericType.ElementType.DeclaringType != null)
                {
                    baseName = GetReadableTypeName(genericType.ElementType.DeclaringType) + "." + baseName;
                }

                string args = string.Join(", ", genericType.GenericArguments.Select(GetReadableTypeName));
                return $"{baseName}<{args}>";
            }

            string name = StripBacktick(typeRef.Name);
            string primitive = MapPrimitive(name);
            if (primitive != name)
            {
                return primitive;
            }

            if (typeRef.DeclaringType != null)
            {
                return GetReadableTypeName(typeRef.DeclaringType) + "." + name;
            }

            if (!string.IsNullOrEmpty(typeRef.Namespace))
            {
                return typeRef.Namespace + "." + name;
            }

            return name;
        }

        /// <summary>
        /// Transforms a type name string into a human readable form, handling
        /// generic types, nested types, and primitive mapping
        /// </summary>
        public static string TransformType(string old)
        {
            if (string.IsNullOrEmpty(old))
            {
                return old;
            }

            // Strip backtick+arity (e.g., List`1 → List)
            old = Regex.Replace(old, @"`\d+", "");

            // Transform each type name token: strip namespace, map primitives, convert / to .
            old = Regex.Replace(old, @"[\w./]+", m =>
            {
                // Split by / for nested types, strip namespace from the outermost part
                string[] parts = m.Value.Split('/');
                int lastDot = parts[0].LastIndexOf('.');
                if (lastDot >= 0)
                {
                    parts[0] = parts[0].Substring(lastDot + 1);
                }

                return MapPrimitive(string.Join(".", parts));
            });

            // Normalize comma spacing in generic arguments
            old = old.Replace(",", ", ");

            return old;
        }

        private static string StripBacktick(string name)
        {
            int index = name.IndexOf('`');
            return index > 0 ? name.Substring(0, index) : name;
        }

        private static string MapPrimitive(string name)
        {
            switch (name)
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
                    return name;
            }
        }

        /// <summary>
        /// Gets the C# qualifier string for the specified method
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private static string GetMethodQualifier(MethodDefinition method)
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

        public static ModifierSignature GetModifierSignature(MemberReference memberRef)
        {
            switch (memberRef)
            {
                case MethodDefinition methodDefinition:
                    return GetModifierSignature(methodDefinition);

                case FieldDefinition fieldDefinition:
                    return GetModifierSignature(fieldDefinition);

                case PropertyDefinition propertyDefinition:
                    return GetModifierSignature(propertyDefinition);

                case TypeDefinition typeDefinition:
                    return GetModifierSignature(typeDefinition);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets a signature for the specified field
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private static ModifierSignature GetModifierSignature(FieldDefinition field)
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

            return new ModifierSignature(exposure, field.FullName, field.Name, Array.Empty<string>());
        }

        /// <summary>
        /// Gets a signature for the specified method
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private static ModifierSignature GetModifierSignature(MethodDefinition method)
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
        private static ModifierSignature GetModifierSignature(PropertyDefinition property)
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

            return new ModifierSignature(new[] { getExposure, setExposure }, property.FullName, property.Name, Array.Empty<string>());
        }

        /// <summary>
        /// Gets a signature for the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ModifierSignature GetModifierSignature(TypeDefinition type)
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

            return new ModifierSignature(exposure, type.FullName, type.Name, Array.Empty<string>());
        }

        public static string[] ParseArgumentString(string argumentString, out string returnValue)
        {
            // Check arg string for null
            if (string.IsNullOrEmpty(argumentString))
            {
                returnValue = null;
                return null;
            }

            // Strip whitespace
            string argString = argumentString.Replace(" ", string.Empty);

            // Split by return value indicator
            string[] argsReturnSplit = argString.Split(new[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);
            if (argsReturnSplit.Length == 0)
            {
                returnValue = null;
                return null;
            }

            // Split by comma
            string[] args = argsReturnSplit[0].Split(',');

            // Set the return value
            returnValue = argsReturnSplit.Length > 1 ? argsReturnSplit[1] : null;

            // Return
            return args;
        }
    }
}
