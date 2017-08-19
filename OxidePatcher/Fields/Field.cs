using System;
using System.Linq;
using System.Windows.Forms;

using Mono.Cecil;

using OxidePatcher.Patching;
using System.IO;

namespace OxidePatcher.Fields
{
    public enum Exposure { Private, Protected, Public, Internal, Static, Null }

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

        protected void ShowMsg(string msg, string header, Patcher patcher = null)
        {
            if (patcher != null)
                patcher.Log(msg);
            else
                MessageBox.Show(msg, header, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Creates the settings view control for this hook
        /// </summary>
        /// <returns></returns>
        public Views.FieldSettingsControl CreateSettingsView()
        {
            return new Views.FieldSettingsControl { Field = this };
        }

        internal bool IsValid(bool warn = false)
        {
            var fieldData = FieldType.Split('|');

            var targetAssemblyFile = Path.Combine(PatcherForm.MainForm.CurrentProject.TargetDirectory, $"{AssemblyName.Replace(".dll", "")}_Original.dll");
            var targetAssembly = AssemblyDefinition.ReadAssembly(targetAssemblyFile);
            var target = targetAssembly.MainModule.GetType(TypeName);
            if (target == null)
            {
                if (warn)
                    ShowMsg($"The type '{TypeName}' in '{AssemblyName}' to add the field '{Name}' into could not be found!", "Target type missing");

                return false;
            }

            if (target.Fields.Any(x => x.Name == Name))
            {
                if (warn)
                    ShowMsg($"A field with the name '{Name}' already exists in the targetted class!", "Duplicate name");

                return false;
            }

            foreach (var field in PatcherForm.MainForm.CurrentProject.Manifests.Find(x => x.AssemblyName == AssemblyName).Fields.Where(x => x != this))
            {
                if (field.Name != Name) continue;

                if (warn)
                    ShowMsg($"A field with the name '{Name}' is already being added to the targetted class!", "Duplicate name");

                return false;
            }

            if (string.IsNullOrEmpty(FieldType)) return true;

            var newFieldAssemblyFile = Path.Combine(PatcherForm.MainForm.CurrentProject.TargetDirectory, $"{fieldData[0].Replace(".dll", "")}.dll");
            var newFieldAssembly = AssemblyDefinition.ReadAssembly(newFieldAssemblyFile);
            var newFieldType = newFieldAssembly?.MainModule?.GetType(fieldData[1]);
            if (newFieldType == null)
            {
                if (warn)
                    ShowMsg($"The type '{fieldData[1]}' in '{fieldData[0]}' to add the field '{Name}' from could not be found!", "New field type missing");

                return false;
            }

            return true;
        }
    }
}
