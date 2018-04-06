using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Oxide.Patcher.Patching;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Patcher.Deobfuscation
{
    /// <summary>
    /// A deobfuscator capable of deobfuscating assemblies obfsucated by CodeGuard for Unity
    /// </summary>
    public class UnityCodeGuard : Deobfuscator
    {
        /// <summary>
        /// Gets the name of this deobfuscator
        /// </summary>
        public override string Name => "Unity CodeGuard";

        /// <summary>
        /// Gets the priority of this deobfuscator
        /// </summary>
        public override int Priority => 0;

        // The current reference count map
        private Dictionary<MethodReference, int> refcounts;

        /// <summary>
        /// Gets all relevant types for the specified assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private IEnumerable<TypeDefinition> GetTypes(AssemblyDefinition assembly)
        {
            //TypeReference objectref = assembly.MainModule.TypeSystem.Object;
            return assembly.MainModule.Types;
            /*.Where((t) =>
                {
                    TypeDefinition curbase = t;
                    while (curbase != objectref)
                    {
                        if (curbase.BaseType == null) return false;
                        try
                        {
                            curbase = curbase.BaseType.Resolve();
                        }
                        catch (AssemblyResolutionException)
                        {
                            return false;
                        }
                        if (curbase.FullName == "UnityEngine.MonoBehaviour")
                            return true;
                    }
                    return false;
                })*/
        }

        /// <summary>
        /// Returns if this deobfuscator is capable of deobfuscating the specified assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public override bool CanDeobfuscate(AssemblyDefinition assembly)
        {
            // Search the assembly for field and method names which have been made into symbols
            // We're only going to look through types that inherit MonoBehaviour at some point in the inheritance chain
            int count = 0;
            TypeReference objectref = assembly.MainModule.TypeSystem.Object;
            foreach (TypeDefinition type in GetTypes(assembly))
            {
                // Search all members
                foreach (MethodDefinition method in type.Methods)
                {
                    if (IdentifyObfuscatedName(method.Name))
                    {
                        count++;
                    }
                }

                foreach (FieldDefinition field in type.Fields)
                {
                    if (IdentifyObfuscatedName(field.Name))
                    {
                        count++;
                    }
                }

                foreach (PropertyDefinition property in type.Properties)
                {
                    if (IdentifyObfuscatedName(property.Name))
                    {
                        count++;
                    }
                }
            }

            // If we have more than a certain number of obfuscated methods, return true
            // We do this to allow for a few false positives
            const int threshold = 5;
            return count >= threshold;
        }

        /// <summary>
        /// Returns if the specified variable, field or method name has been obfuscated or not
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool IdentifyObfuscatedName(string name)
        {
            // Symbols obfuscated by CodeGuard consist of a single unicode character
            if (name.Length != 1)
            {
                return false;
            }

            char c = name[0];

            // "Normal" names are anything from a-z, A-Z or an underscore
            // These names might still be obfuscated but probably not by CodeGuard

            if (c >= 'a' && c <= 'z')
            {
                return false;
            }

            if (c >= 'A' && c <= 'Z')
            {
                return false;
            }

            if (c == '_')
            {
                return false;
            }

            // CodeGuard probably renamed this
            return true;
        }

        /// <summary>
        /// Deobfuscates the specified assembly, returning a success value
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public override bool Deobfuscate(AssemblyDefinition assembly)
        {
            // Build the refmap
            refcounts = new Dictionary<MethodReference, int>();
            foreach (Instruction inst in GetTypes(assembly)
                .SelectMany(t => t.Methods)
                .Where(m => m.HasBody)
                .SelectMany(m => m.Body.Instructions)
                )
            {
                // Get method
                MethodReference method = inst.Operand as MethodReference;
                if (method != null)
                {
                    // Increment refcounter
                    if (refcounts.TryGetValue(method, out int curcount))
                    {
                        refcounts[method] = curcount + 1;
                    }
                    else
                    {
                        refcounts.Add(method, 1);
                    }
                }
            }

            // Search the assembly for field and method names which have been made into symbols
            // We're only going to look through types that inherit MonoBehaviour at some point in the inheritance chain
            foreach (TypeDefinition type in GetTypes(assembly))
            {
                // Deobfuscate
                DeobfuscateType(type);
            }
            return true;
        }

        /// <summary>
        /// Attempts to deobfuscate the specified type
        /// </summary>
        /// <param name="typedef"></param>
        protected virtual void DeobfuscateType(TypeDefinition typedef)
        {
            // Deal with method parameters
            foreach (MethodDefinition method in typedef.Methods
                .Where(m => m.HasParameters)
                )
            {
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    ParameterDefinition paramdef = method.Parameters[i];
                    if (IdentifyObfuscatedName(paramdef.Name))
                    {
                        string name = $"arg{i}";
                        paramdef.Name = name;
                    }
                }
            }

            // Deal with field names
            FieldDefinition[] fields = typedef.Fields
                .Where(f => IdentifyObfuscatedName(f.Name))
                .ToArray();
            Array.Sort(fields, (a, b) =>
                {
                    // Sort firstly by type
                    // Then sort by offset
                    // Finally, sort by obfuscated name
                    int tmp = Comparer<string>.Default.Compare(a.FieldType.FullName, b.FieldType.FullName);
                    if (tmp != 0)
                    {
                        return tmp;
                    }

                    tmp = Comparer<int>.Default.Compare(a.Offset, b.Offset);
                    if (tmp != 0)
                    {
                        return tmp;
                    }

                    return Comparer<string>.Default.Compare(a.Name, b.Name);
                });
            for (int i = 0; i < fields.Length; i++)
            {
                FieldDefinition field = fields[i];
                string name;
                if (field.IsPublic)
                {
                    name = $"Field{i + 1}";
                }
                else
                {
                    name = $"field{i + 1}";
                }

                field.Name = name;
            }

            // Deal with property names
            PropertyDefinition[] properties = typedef.Properties
                .Where(f => IdentifyObfuscatedName(f.Name))
                .ToArray();
            Array.Sort(properties, (a, b) =>
            {
                // Sort firstly by type, then by obfuscated name
                int tmp = Comparer<string>.Default.Compare(a.PropertyType.FullName, b.PropertyType.FullName);
                if (tmp != 0)
                {
                    return tmp;
                }

                return Comparer<string>.Default.Compare(a.Name, b.Name);
            });
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyDefinition property = properties[i];
                string name = $"property{i + 1}";
                // NOTE: Do we need to rename the get and set methods too?
                property.Name = name;
            }

            // Deal with method names
            MethodDefinition[] methods = typedef.Methods
                .Where(f => IdentifyObfuscatedName(f.Name))
                .ToArray();
            Array.Sort(methods, (a, b) =>
            {
                // Sort by the following in order: return type, parameter count, parameter types, obfuscated name
                int tmp = Comparer<string>.Default.Compare(a.ReturnType.FullName, b.ReturnType.FullName);
                if (tmp != 0)
                {
                    return tmp;
                }

                tmp = Comparer<int>.Default.Compare(a.Parameters.Count, b.Parameters.Count);
                if (tmp != 0)
                {
                    return tmp;
                }
                // TODO: Sort by parameter types
                return Comparer<string>.Default.Compare(a.Name, b.Name);
            });
            for (int i = 0; i < methods.Length; i++)
            {
                MethodDefinition method = methods[i];
                string name;
                if (method.IsPublic)
                {
                    name = $"Method{i + 1}";
                }
                else
                {
                    name = $"method{i + 1}";
                }

                method.Name = name;
            }

            // Deal with proxy methods
            HashSet<MethodDefinition> toremove = new HashSet<MethodDefinition>();
            foreach (MethodDefinition method in typedef.Methods
                .Where(m => m.HasBody)
                )
            {
                // Identify a proxy call via IL
                Collection<Instruction> instructions = method.Body.Instructions;
                if (instructions.Count != 3)
                {
                    continue;
                }

                if (instructions[0].OpCode.Code != Code.Ldarg_0)
                {
                    continue;
                }

                if (instructions[1].OpCode.Code != Code.Callvirt)
                {
                    continue;
                }

                if (instructions[2].OpCode.Code != Code.Ret)
                {
                    continue;
                }

                // Check that it's calling an obfuscated method in our type
                MethodReference proxymethod = instructions[1].Operand as MethodReference;
                if (proxymethod.DeclaringType.FullName != typedef.FullName)
                {
                    continue;
                }

                if (!methods.Any(m => m.FullName == proxymethod.FullName))
                {
                    continue;
                }

                // Check that the target method is not referenced by anything else
                if (!refcounts.TryGetValue(proxymethod, out int refcount))
                {
                    refcount = 0;
                }

                if (refcount > 1)
                {
                    continue;
                }

                // Resolve it
                MethodDefinition proxymethoddef = proxymethod.Resolve();
                if (!proxymethoddef.HasBody)
                {
                    continue;
                }

                // It passed, collapse the proxy method's IL into this method and remove it
                ILWeaver weaver = new ILWeaver(proxymethoddef.Body);
                weaver.Apply(method.Body);
                toremove.Add(proxymethoddef);
            }

            // Remove any proxy methods
            foreach (MethodDefinition method in toremove)
            {
                typedef.Methods.Remove(method);
            }
        }
    }
}
