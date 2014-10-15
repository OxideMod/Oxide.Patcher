using System;
using System.Linq;

using Mono.Cecil;

using OxidePatcher.Hooks;

namespace OxidePatcher
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
                    return old;
                else
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
                qualifier = method.IsPublic ? "public static" : method.IsPrivate ? "private static" : "internal static";
            else if (method.IsVirtual)
                qualifier = method.IsPublic ? "public virtual" : method.IsPrivate ? "private virtual" : method.IsFamilyAndAssembly ? "protected virtual" : "internal virtual";
            else
                qualifier = method.IsPublic ? "public" : method.IsPrivate ? "private" : "protected";
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
            string[] args = method.Parameters.Select((x) => string.Format("{0} {1}", Utility.TransformType(x.ParameterType.Name), x.Name)).ToArray();
            string name;
            if (method.Name == ".ctor" || method.Name == ".cctor")
                name = string.Format("{0} {1}({2})", qualifier, method.DeclaringType.Name, string.Join(", ", args));
            else
                name = string.Format("{0} {1} {2}({3})", qualifier, Utility.TransformType(method.ReturnType.Name), method.Name, string.Join(", ", args));
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
                exposure = MethodExposure.Public;
            else if (method.IsPrivate)
                exposure = MethodExposure.Private;
            else if (method.IsFamilyAndAssembly)
                exposure = MethodExposure.Protected;
            else
                exposure = MethodExposure.Internal;
            string[] parameters = new string[method.Parameters.Count];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] = method.Parameters[i].ParameterType.FullName;
            }
            return new MethodSignature(exposure, method.ReturnType.FullName, method.Name, parameters);
        }
    }
}
