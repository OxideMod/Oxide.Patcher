using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace OxidePatcher
{
    /// <summary>
    /// An Oxide patcher project
    /// </summary>
    public class Project
    {
        /// <summary>
        /// The minimum required patcher version for a project. Projects with a patcher version below this will need to be converted to the latest project format.
        /// </summary>
        public static readonly Version MINIMUM_PATCHER_VERSION = new Version(2, 1);

        /// <summary>
        /// Gets or sets the project name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The configuration values for the project.
        /// </summary>
        [JsonIgnore]
        public ProjectConfiguration Configuration { get; set; }

        /// <summary>
        /// The path to the project configuration file.
        /// </summary>
        public string ConfigurationPath { get; set; }

        /// <summary>
        /// The oxide patcher version for the file. Used to allow for automatic project updates.
        /// </summary>
        public Version OxidePatcherVersion { get; set; }

        [JsonIgnore]
        public string ProjectFilePath { get; set; }

        /// <summary>
        /// Gets or sets all the manifests contained in this project
        /// </summary>
        public List<Manifest> Manifests { get; set; }

        /// <summary>
        /// Used by the UI to indicate to the user that the project needs to be updated/resaved to match the expected format of the latest patcher version.
        /// </summary>
        [JsonIgnore]
        public bool IsLegacyVersion { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the Project class with sensible defaults
        /// </summary>
        public Project()
        {
            // Fill in defaults
            Name = "Untitled Project";
            Configuration = null;
            ConfigurationPath = string.Empty;
            Manifests = new List<Manifest>();
        }

        /// <summary>
        /// Saves this project to file
        /// </summary>
        public void Save()
        {
            File.WriteAllText(ProjectFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
            File.WriteAllText(ConfigurationPath, JsonConvert.SerializeObject(Configuration, Formatting.Indented));
        }

        /// <summary>
        /// Loads this project from file
        /// </summary>
        /// <returns></returns>
        public static Project Load(string filename)
        {
            if (!File.Exists(filename)) return new Project();
            string text = File.ReadAllText(filename);

            Project project = null;
            try
            {
                project = JsonConvert.DeserializeObject<Project>(text);
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                if (PatcherForm.MainForm != null)
                {
                    MessageBox.Show("There was a problem loading the project file!" + 
                        Environment.NewLine + "Are all file paths properly escaped?", "JSON Exception", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Console.WriteLine("ERROR: There was a problem loading the project file!" + 
                        " Are all file paths properly escaped?");
                }
                return null;
            }

            // set filename
            project.ProjectFilePath = filename;

            // add a patcher version if one wasn't already deserialized
            if(project.OxidePatcherVersion == null)
            {
                project.OxidePatcherVersion = new Version(1, 0);
            }
            
            if(project.OxidePatcherVersion < MINIMUM_PATCHER_VERSION)
            {
                project.IsLegacyVersion = true;
            }

            if(!string.IsNullOrWhiteSpace(project.ConfigurationPath) && File.Exists(project.ConfigurationPath))
            {
                try
                {
                    var configurationText = File.ReadAllText(project.ConfigurationPath);
                    project.Configuration = JsonConvert.DeserializeObject<ProjectConfiguration>(configurationText);
                }
                catch (FileNotFoundException)
                {
                    // ignore file not found
                }
                catch (JsonReaderException)
                {
                    if (PatcherForm.MainForm != null)
                    {
                        MessageBox.Show("There was a problem deserializing the project's configuration file.",
                            "JSON Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Console.WriteLine(@"ERROR: There was a problem deserializing the project file!
                        Are all file paths properly escaped?");
                    }
                }
            }

            // attempt to deserialize legacy TargetDirectory property
            if (string.IsNullOrEmpty(project.Configuration?.AssembliesSourceDirectory))
            {
                try
                {
                    var projectJson = JObject.Parse(text);
                    var targetDirectory = projectJson.Value<string>("TargetDirectory");
                    if (!string.IsNullOrEmpty(targetDirectory))
                    {
                        project.Configuration = project.Configuration ?? new ProjectConfiguration();
                        project.Configuration.AssembliesSourceDirectory = targetDirectory;
                    }
                }
                catch (Exception)
                {
                    //no need to worry about errors trying to parse this property since it may not exist 
                }
            }
            
            return project;
        }

        public static Project Load(string filename, string overrideTarget)
        {
            var project = Load(filename);
            if(project?.Configuration != null)
            {
                project.Configuration.AssembliesSourceDirectory = overrideTarget;
            }

            return project;
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
