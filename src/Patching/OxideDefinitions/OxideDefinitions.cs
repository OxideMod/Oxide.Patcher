using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Oxide.Patcher.Patching.OxideDefinitions
{
    public static class OxideDefinitions
    {
        private static readonly Regex TypeRegex = new Regex(@"\[(?>\[(?<DEPTH>)|\](?<-DEPTH>)|.?)*(?(DEPTH)(?!))\]", RegexOptions.Compiled);

        public static bool TryParseType(string input, out OxideTypeDefinition result, out string error)
        {
            result = null;
            error = string.Empty;

            if (!TypeRegex.IsMatch(input))
            {
                string[] typeData = input.Split('|');
                if (typeData.Length < 2)
                {
                    error = "OpType Type format: AssemblyName|TypeFullName";
                    return false;
                }

                AssemblyDefinition targetAssembly = GetAssembly(typeData[0]);
                if (targetAssembly == null)
                {
                    error = $"Assembly '{typeData[0]}' not found";
                    return false;
                }

                TypeDefinition targetType = targetAssembly.MainModule.GetType(typeData[1].Trim());
                if (targetType == null)
                {
                    error = $"Type '{typeData[1]}' not found";
                    return false;
                }

                result = new OxideTypeDefinition(targetType);
            }
            else
            {
                Match genericTypeMatch = TypeRegex.Match(input);
                string genericType = genericTypeMatch.Value.Substring(1, genericTypeMatch.Value.Length - 2);
                List<string> genericTypeInstances = GetGenericTypeInstances(genericType);
                string[] typeData = input.Split('[')[0].Split('|');
                if (typeData.Length < 2)
                {
                    error = "OpType Type format: AssemblyName|TypeFullName";
                    return false;
                }

                AssemblyDefinition targetAssembly = GetAssembly(typeData[0]);
                if (targetAssembly == null)
                {
                    error = $"Assembly '{typeData[0]}' not found";
                    return false;
                }

                TypeDefinition targetType = targetAssembly.MainModule.GetType($"{typeData[1]}`{genericTypeInstances.Count}");
                if (targetType == null)
                {
                    error = $"Type '{typeData[1]}' not found";
                    return false;
                }

                result = new OxideTypeDefinition(targetType);

                foreach (string genericTypeInstanceName in genericTypeInstances)
                {
                    if (TryParseType(genericTypeInstanceName, out OxideTypeDefinition genericTypeDefinition, out error))
                    {
                        result.AddGenericTypeInstance(genericTypeDefinition);
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
            }

            return true;
        }

        private static List<string> GetGenericTypeInstances(string typeName)
        {
            int depth = 0;
            int start = 0;
            List<string> genericTypes = new List<string>();

            for (int i = 0; i < typeName.Length; i++)
            {
                if (typeName[i] == '[')
                {
                    depth++;
                }
                else if (typeName[i] == ']')
                {
                    depth--;
                }
                else if (typeName[i] == ',' && depth == 0)
                {
                    genericTypes.Add(typeName.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }

            genericTypes.Add(typeName.Substring(start, typeName.Length - start).Trim());

            return genericTypes;
        }

        private static AssemblyDefinition GetAssembly(string assemblyName)
        {
            return PatcherForm.MainForm.LoadAssembly(assemblyName.Replace(".dll", "") + ".dll");
        }
    }
}
