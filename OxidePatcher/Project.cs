using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace OxidePatcher
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
        /// Initialises a new instance of the Project class with sensible defaults
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
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new Manifest.Converter());
            File.WriteAllText(filename, JsonConvert.SerializeObject(this, Formatting.Indented, settings));
        }

        /// <summary>
        /// Loads this project from file
        /// </summary>
        /// <returns></returns>
        public static Project Load(string filename)
        {
            if (!File.Exists(filename)) return new Project();
            string text = File.ReadAllText(filename);
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new Manifest.Converter());
            return JsonConvert.DeserializeObject<Project>(text, settings);
        }

        /// <summary>
        /// Adds an empty manifest with the given assembly name to the project
        /// </summary>
        /// <param name="assemblyname"></param>
        public void AddManifest(string assemblyname)
        {
            Manifest manifest = new Manifest();
            manifest.AssemblyName = assemblyname;
            Manifests.Add(manifest);
        }

        /// <summary>
        /// Removes the manifest that references the specified assembly name from the project
        /// </summary>
        /// <param name="assemblyname"></param>
        public void RemoveManifest(string assemblyname)
        {
            var manifest = GetManifest(assemblyname);
            if (manifest == null) return;
            Manifests.Remove(manifest);
        }

        /// <summary>
        /// Gets the manifest with the specified assembly name
        /// </summary>
        /// <param name="assemblyname"></param>
        /// <returns></returns>
        public Manifest GetManifest(string assemblyname)
        {
            foreach (var manifest in Manifests)
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
