using Mono.Cecil;
using Oxide.Patcher.Deobfuscation;
using Oxide.Patcher.Fields;
using Oxide.Patcher.Hooks;
using Oxide.Patcher.Modifiers;
using Oxide.Patcher.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Oxide.Patcher.Common
{
    public class AssemblyLoader
    {
        private Project _project;
        private string _opjPath;

        private Dictionary<string, AssemblyDefinition> assemblydict;
        internal Dictionary<AssemblyDefinition, string> rassemblydict;

        private IAssemblyResolver _resolver;

        public AssemblyLoader(Project project, string opjPath)
        {
            _project = project;
            _opjPath = opjPath;

            _resolver = new PatcherAssemblyResolver(project.TargetDirectory);

            assemblydict = new Dictionary<string, AssemblyDefinition>();
            rassemblydict = new Dictionary<AssemblyDefinition, string>();

            LoadAssemblies();
        }

        internal void LoadAssemblies()
        {
            foreach (Manifest manifest in _project.Manifests)
            {
                LoadAssembly(manifest.AssemblyName);
            }
        }

        /// <summary>
        /// Verifies the project is still valid
        /// </summary>
        public void VerifyProject()
        {
            // Step 1: Check all included assemblies are intact
            // Step 2: Check all hooks are intact
            // Step 3: Check all modifiers are intact
            int missingAssemblies = 0, missingMethods = 0, changedMethods = 0, changedFields = 0, changedModMethods = 0, changedProperties = 0, changedNewFields = 0;
            foreach (Manifest manifest in _project.Manifests)
            {
                if (!assemblydict.TryGetValue(manifest.AssemblyName, out AssemblyDefinition _))
                {
                    missingAssemblies++;
                    foreach (Hook hook in manifest.Hooks)
                    {
                        hook.Flagged = true;
                    }
                }
                else
                {
                    foreach (Hook hook in manifest.Hooks)
                    {
                        MethodDefinition method = GetMethod(hook.AssemblyName, hook.TypeName, hook.Signature);
                        if (method == null)
                        {
                            missingMethods++;
                            hook.Flagged = true;
                        }
                        else
                        {
                            string hash = new ILWeaver(method.Body).Hash;
                            if (hash != hook.MSILHash)
                            {
                                changedMethods++;
                                hook.MSILHash = hash;
                                hook.Flagged = true;
                            }
                        }
                    }

                    foreach (Modifier modifier in manifest.Modifiers)
                    {
                        switch (modifier.Type)
                        {
                            case ModifierType.Field:
                                FieldDefinition fielddef = GetField(modifier.AssemblyName, modifier.TypeName, modifier.Name, modifier.Signature);
                                if (fielddef == null)
                                {
                                    changedFields++;
                                    modifier.Flagged = true;
                                }
                                break;

                            case ModifierType.Method:
                                MethodDefinition methoddef = GetMethod(modifier.AssemblyName, modifier.TypeName, modifier.Signature);
                                if (methoddef == null)
                                {
                                    changedModMethods++;
                                    modifier.Flagged = true;
                                }
                                break;

                            case ModifierType.Property:
                                PropertyDefinition propertydef = GetProperty(modifier.AssemblyName, modifier.TypeName, modifier.Name, modifier.Signature);
                                if (propertydef == null)
                                {
                                    changedProperties++;
                                    modifier.Flagged = true;
                                }
                                break;
                        }
                    }

                    foreach (Field field in manifest.Fields)
                    {
                        if (field.IsValid(_project))
                        {
                            continue;
                        }

                        changedNewFields++;
                        field.Flagged = true;
                    }
                }
            }

            if (missingAssemblies <= 0 && missingMethods <= 0 && changedMethods <= 0 && changedFields <= 0 &&
                changedModMethods <= 0 && changedProperties <= 0 && changedNewFields <= 0)
            {
                return;
            }

            _project.Save(_opjPath);

            if (PatcherForm.MainForm != null)
            {
                if (missingAssemblies >= 1)
                {
                    MessageBox.Show($"{missingAssemblies} assembl{(missingAssemblies > 1 ? "ies" : "ly")} are missing from the target directory!",
                                    "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (missingMethods >= 1)
                {
                    MessageBox.Show($"{missingMethods} method{(missingMethods > 1 ? "s" : string.Empty)} referenced by hooks no longer exist!",
                                    "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (changedMethods >= 1)
                {
                    MessageBox.Show($"{changedMethods} method{(changedMethods > 1 ? "s" : string.Empty)} referenced by hooks have changed!",
                                    "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (changedFields >= 1)
                {
                    MessageBox.Show($"{changedFields} field{(changedFields > 1 ? "s" : string.Empty)} with altered modifiers have changed!",
                                    "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (changedModMethods >= 1)
                {
                    MessageBox.Show($"{changedModMethods} method{(changedModMethods > 1 ? "s" : string.Empty)} with altered modifiers have changed!",
                                    "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (changedProperties >= 1)
                {
                    MessageBox.Show($"{changedProperties} propert{(changedProperties > 1 ? "ies" : "y")} with altered modifiers have changed!",
                                    "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (changedNewFields >= 1)
                {
                    MessageBox.Show($"{changedNewFields} new field{(changedNewFields > 1 ? "s" : string.Empty)} were flagged!",
                                    "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        internal AssemblyDefinition LoadAssembly(string name)
        {
            if (assemblydict.TryGetValue(name, out AssemblyDefinition assdef))
            {
                return assdef;
            }

            string file = $"{Path.GetFileNameWithoutExtension(name)}_Original{Path.GetExtension(name)}";
            string filename = Path.Combine(_project.TargetDirectory, file);
            if (!File.Exists(filename))
            {
                string oldfilename = Path.Combine(_project.TargetDirectory, name);
                if (!File.Exists(oldfilename))
                {
                    return null;
                }

                CreateOriginal(oldfilename, filename);
            }
            assdef = AssemblyDefinition.ReadAssembly(filename, new ReaderParameters { AssemblyResolver = _resolver });
            assemblydict.Add(name, assdef);
            rassemblydict.Add(assdef, name);
            return assdef;
        }

        public void CreateOriginal(string oldfile, string newfile)
        {
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(oldfile, new ReaderParameters { AssemblyResolver = _resolver });
            Deobfuscator deob = Deobfuscators.Find(assembly);
            if (deob != null)
            {
                //TODO: Make this work without UI
                if (PatcherForm.MainForm != null)
                {
                    DialogResult result = MessageBox.Show(
                            $"Assembly '{assembly.MainModule.Name}' appears to be obfuscated using '{deob.Name}', attempt to deobfuscate?", "Oxide Patcher", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                    if (result == DialogResult.Yes)
                    {
                        // Deobfuscate
                        if (deob.Deobfuscate(assembly))
                        {
                            // Success
                            if (File.Exists(newfile))
                            {
                                File.Delete(newfile);
                            }

                            assembly.Write(newfile);
                            return;
                        }

                        if (PatcherForm.MainForm != null)
                        {
                            MessageBox.Show("Deobfuscation failed!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            Console.WriteLine("ERROR: Deobfuscation failed!");
                        }
                    }
                }
            }
            File.Copy(oldfile, newfile);
        }

        /// <summary>
        /// Gets the method associated with the specified signature
        /// </summary>
        /// <param name="assemblyname"></param>
        /// <param name="typename"></param>
        /// <param name="signature"></param>
        public MethodDefinition GetMethod(string assemblyname, string typename, MethodSignature signature)
        {
            if (!assemblydict.TryGetValue(assemblyname, out AssemblyDefinition assdef))
            {
                return null;
            }

            try
            {
                TypeDefinition type = assdef.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == typename);

                return type.Methods.Single(m => Utility.GetMethodSignature(m).Equals(signature));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the method associated with the specified signature
        /// </summary>
        /// <param name="assemblyname"></param>
        /// <param name="typename"></param>
        /// <param name="signature"></param>
        public MethodDefinition GetMethod(string assemblyname, string typename, ModifierSignature signature)
        {
            if (!assemblydict.TryGetValue(assemblyname, out AssemblyDefinition assdef))
            {
                return null;
            }

            try
            {
                TypeDefinition type = assdef.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == typename);

                return type.Methods.Single(m => Utility.GetModifierSignature(m).Equals(signature));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the field associated with the specified signature
        /// </summary>
        /// <param name="assemblyname"></param>
        /// <param name="typename"></param>
        /// <param name="name"></param>
        /// <param name="signature"></param>
        public FieldDefinition GetField(string assemblyname, string typename, string name, ModifierSignature signature)
        {
            if (!assemblydict.TryGetValue(assemblyname, out AssemblyDefinition assdef))
            {
                return null;
            }

            try
            {
                TypeDefinition type = assdef.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == typename);

                return type.Fields.Single(m => Utility.GetModifierSignature(m).Equals(signature));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the property associated with the specified signature
        /// </summary>
        /// <param name="assemblyname"></param>
        /// <param name="typename"></param>
        /// <param name="name"></param>
        /// <param name="signature"></param>
        public PropertyDefinition GetProperty(string assemblyname, string typename, string name, ModifierSignature signature)
        {
            if (!assemblydict.TryGetValue(assemblyname, out AssemblyDefinition assdef))
            {
                return null;
            }

            try
            {
                TypeDefinition type = assdef.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == typename);

                return type.Properties.Single(m => Utility.GetModifierSignature(m).Equals(signature));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the type associated with the specified signature
        /// </summary>
        /// <param name="assemblyname"></param>
        /// <param name="typename"></param>
        public TypeDefinition GetType(string assemblyname, string typename)
        {
            if (!assemblydict.TryGetValue(assemblyname, out AssemblyDefinition assdef))
            {
                return null;
            }

            try
            {
                return assdef.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == typename);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
