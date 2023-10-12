using Mono.Cecil;
using Oxide.Patcher.Patching.OxideDefinitions;
using Oxide.Patcher.Views;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Oxide.Patcher.Fields
{
    /// <summary>
    /// Represents a hook that is applied to single method and calls a single Oxide hook
    /// </summary>
    public class Field
    {
        /// <summary>
        /// Gets or sets a name for this field
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
        /// Gets or sets the field to be added
        /// </summary>
        public string FieldType { get; set; }

        /// <summary>
        /// Gets or sets if this modifier has been flagged
        /// </summary>
        public bool Flagged { get; set; }

        public Field()
        {
        }

        public Field(TypeDefinition type, string assembly)
        {
            Name = "NewField";
            TypeName = type.FullName;
            FieldType = string.Empty;
            AssemblyName = assembly;
        }

        private void ShowMessage(string message, string header, Patching.Patcher patcher = null)
        {
            if (patcher != null)
            {
                patcher.Log(message);
            }
            else if (PatcherForm.MainForm != null)
            {
                MessageBox.Show(message, header, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Console.WriteLine($"{header}: {message}");
            }
        }

        /// <summary>
        /// Creates the settings view control for this hook
        /// </summary>
        /// <returns></returns>
        public FieldSettingsControl CreateSettingsView()
        {
            return new FieldSettingsControl { Field = this };
        }

        internal bool IsValid(Project project, bool warn = false)
        {
            string targetAssemblyFile = Path.Combine(project.TargetDirectory, $"{AssemblyName.Replace(".dll", "")}_Original.dll");
            AssemblyDefinition targetAssembly = AssemblyDefinition.ReadAssembly(targetAssemblyFile);
            TypeDefinition target = targetAssembly.MainModule.GetType(TypeName);
            if (target == null)
            {
                if (warn)
                {
                    ShowMessage($"The type '{TypeName}' in '{AssemblyName}' to add the field '{Name}' into could not be found!", "Target type missing");
                }

                return false;
            }

            if (target.Fields.Any(x => x.Name == Name))
            {
                if (warn)
                {
                    ShowMessage($"A field with the name '{Name}' already exists in the targetted class!", "Duplicate name");
                }

                return false;
            }

            foreach (Field field in project.Manifests.Find(x => x.AssemblyName == AssemblyName).Fields)
            {
                if (field == this || field.Name != Name)
                {
                    continue;
                }

                if (warn)
                {
                    ShowMessage($"A field with the name '{Name}' is already being added to the targetted class!", "Duplicate name");
                }

                return false;
            }

            if (string.IsNullOrEmpty(FieldType))
            {
                return true;
            }

            if (!OxideDefinitions.TryParseType(FieldType, out _, out string error))
            {
                if (warn)
                {
                    ShowMessage($"Couldn't resolve the field type: {error}", "Error resolving field type");
                }

                return false;
            }

            return true;
        }
    }
}
