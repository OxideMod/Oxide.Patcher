using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Oxide.Patcher
{
    /// <summary>
    /// An Oxide patcher project
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Gets or sets the project name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the directory of the dlls
        /// </summary>
        public string TargetDirectory { get; set; }

        /// <summary>
        /// Gets or sets all the manifests contained in this project
        /// </summary>
        public List<Manifest> Manifests { get; set; }

        /// <summary>
        /// Initializes a new instance of the Project class with sensible defaults
        /// </summary>
        public Project()
        {
            // Fill in defaults
            Name = "Untitled Project";
            TargetDirectory = "";
            Manifests = new List<Manifest>();
        }

        /// <summary>
        /// Saves this project to file
        /// </summary>
        public void Save(string filename)
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        /// <summary>
        /// Loads this project from file
        /// </summary>
        /// <returns></returns>
        public static Project Load(string filename)
        {
            if (!File.Exists(filename))
            {
                return new Project();
            }

            string text = File.ReadAllText(filename);
            try
            {
                return JsonConvert.DeserializeObject<Project>(text);
            }
            catch (JsonReaderException)
            {
                if (PatcherForm.MainForm != null)
                {
                    MessageBox.Show("There was a problem loading the project file!" +
                        Environment.NewLine + "Are all file paths properly escaped?", "JSON Exception",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Console.WriteLine("ERROR: There was a problem loading the project file! Are all file paths properly escaped?");
                }
                return null;
            }
        }

        public static Project Load(string filename, string overrideTarget)
        {
            if (!File.Exists(filename))
            {
                return new Project();
            }

            string text = File.ReadAllText(filename);
            try
            {
                Project project = JsonConvert.DeserializeObject<Project>(text);
                project.TargetDirectory = overrideTarget;
                return project;
            }
            catch (JsonReaderException)
            {
                if (PatcherForm.MainForm != null)
                {
                    MessageBox.Show("There was a problem loading the project file!" +
                        Environment.NewLine + "Are all file paths properly escaped?", "JSON Exception",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Console.WriteLine("ERROR: There was a problem loading the project file! Are all file paths properly escaped?");
                }
                return null;
            }
        }

        /// <summary>
        /// Adds an empty manifest with the given assembly name to the project
        /// </summary>
        /// <param name="assemblyname"></param>
        public void AddManifest(string assemblyname)
        {
            Manifest manifest = new Manifest { AssemblyName = assemblyname };
            Manifests.Add(manifest);
        }

        /// <summary>
        /// Removes the manifest that references the specified assembly name from the project
        /// </summary>
        /// <param name="assemblyname"></param>
        public void RemoveManifest(string assemblyname)
        {
            Manifest manifest = GetManifest(assemblyname);
            if (manifest != null)
            {
                Manifests.Remove(manifest);
            }
        }

        /// <summary>
        /// Gets the manifest with the specified assembly name
        /// </summary>
        /// <param name="assemblyname"></param>
        /// <returns></returns>
        public Manifest GetManifest(string assemblyname)
        {
            foreach (Manifest manifest in Manifests)
            {
                if (manifest.AssemblyName == assemblyname)
                {
                    return manifest;
                }
            }
            return null;
        }
    }
}
