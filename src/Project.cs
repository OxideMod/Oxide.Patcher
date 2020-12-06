using Newtonsoft.Json;
using Oxide.Patcher.Fields;
using Oxide.Patcher.Hooks;
using Oxide.Patcher.Modifiers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Oxide.Patcher
{
    /// <summary>
    /// An Oxide patcher project
    /// </summary>
    public class Project
    {
        private const string HooksFolder = "Hooks";
        private const string ModifiersFolder = "Modifiers";
        private const string FieldsFolder = "Field";

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
            if (filename.EndsWith($"{Name}.opj"))
            {
                filename = filename.Replace($"{Name}.opj", "");
            }

            Dictionary<string, List<Hook>> hookFiles = new Dictionary<string, List<Hook>>();
            Dictionary<string, List<Modifier>> modifierFiles = new Dictionary<string, List<Modifier>>();
            Dictionary<string, List<Field>> fieldFiles = new Dictionary<string, List<Field>>();

            foreach (Manifest manifest in Manifests)
            {
                foreach(Hook hook in manifest.Hooks)
                {
                    string dirName = $"{filename}{Path.DirectorySeparatorChar}{HooksFolder}{Path.DirectorySeparatorChar}{hook.HookCategory}";
                    string hookPath = $"{dirName}{Path.DirectorySeparatorChar}{hook.HookName.Replace("/", "_").Replace("\\", "_")}.json";

                    if (hookFiles.ContainsKey(hookPath))
                    {
                        hookFiles[hookPath].Add(hook);
                    }
                    else
                    {
                        hookFiles[hookPath] = new List<Hook> { hook };
                    }
                }

                foreach (Modifier modifier in manifest.Modifiers)
                {
                    string dirName = $"{filename}{Path.DirectorySeparatorChar}{ModifiersFolder}{Path.DirectorySeparatorChar}";
                    string modifierPath = $"{dirName}{Path.DirectorySeparatorChar}{modifier.TypeName.Replace("/", "_").Replace("\\", "_")}_{modifier.Signature.Name}.json";

                    if (modifierFiles.ContainsKey(modifierPath))
                    {
                        modifierFiles[modifierPath].Add(modifier);
                    }
                    else
                    {
                        modifierFiles[modifierPath] = new List<Modifier> { modifier };
                    }
                }

                foreach (Field field in manifest.Fields)
                {
                    string dirName = $"{filename}{Path.DirectorySeparatorChar}{FieldsFolder}{Path.DirectorySeparatorChar}";
                    string fieldPath = $"{dirName}{Path.DirectorySeparatorChar}{field.AssemblyName.Replace("/", "_").Replace("\\", "_").Replace(".dll", "").Replace(".exe", "")}.json";

                    if (fieldFiles.ContainsKey(fieldPath))
                    {
                        fieldFiles[fieldPath].Add(field);
                    }
                    else
                    {
                        fieldFiles[fieldPath] = new List<Field> { field };
                    }
                }
            }

            foreach (KeyValuePair<string, List<Hook>> hookFile in hookFiles)
            {
                //Create directory if it doesn't exist
                FileInfo file = new FileInfo(hookFile.Key);
                file.Directory.Create();

                //Write to file
                File.WriteAllText(file.FullName, JsonConvert.SerializeObject(hookFile.Value, Formatting.Indented));
            }

            foreach (KeyValuePair<string, List<Modifier>> hookFile in modifierFiles)
            {
                //Create directory if it doesn't exist
                FileInfo file = new FileInfo(hookFile.Key);
                file.Directory.Create();

                //Write to file
                File.WriteAllText(file.FullName, JsonConvert.SerializeObject(hookFile.Value, Formatting.Indented));
            }

            foreach (KeyValuePair<string, List<Field>> hookFile in fieldFiles)
            {
                //Create directory if it doesn't exist
                FileInfo file = new FileInfo(hookFile.Key);
                file.Directory.Create();

                //Write to file
                File.WriteAllText(file.FullName, JsonConvert.SerializeObject(hookFile.Value, Formatting.Indented));
            }
            //System.IO.FileInfo file = new System.IO.FileInfo(filePath);
            //file.Directory.Create(); // If the directory already exists, this method does nothing.
            //System.IO.File.WriteAllText(file.FullName, content);

            //Directory.CreateDirectory(dirName);

            ////Write to file
            //File.WriteAllText(hookName, JsonConvert.SerializeObject(new List<Hook> { hook }, Formatting.Indented));


            //File.WriteAllText(filename, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        /// <summary>
        /// Loads this project from file
        /// </summary>
        /// <returns></returns>
        public static Project Load(string filename)
        {
            //if (filename.EndsWith($".opj"))
            //{
            //if (File.Exists(filename))
            //{
            //    // Old structure
            //    string text = File.ReadAllText(filename);
            //    try
            //    {
            //        return JsonConvert.DeserializeObject<Project>(text);
            //    }
            //    catch (JsonReaderException)
            //    {
            //        if (PatcherForm.MainForm != null)
            //        {
            //            MessageBox.Show("There was a problem loading the project file!" +
            //                Environment.NewLine + "Are all file paths properly escaped?", "JSON Exception",
            //                MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        }
            //        else
            //        {
            //            Console.WriteLine("ERROR: There was a problem loading the project file! Are all file paths properly escaped?");
            //        }
            //        return null;
            //    }
            //}
            //else
            //{
            //    //New structure
            //}
            //}
            //else
            //{
            Project project = new Project();

                var directory = Path.GetDirectoryName(filename);

                string hooksDir = $"{directory}{Path.DirectorySeparatorChar}{HooksFolder}";

                List<Hook> hooks = new List<Hook>();

                //Load Hooks
                if (Directory.Exists(hooksDir))
                {
                    foreach(string dir in Directory.GetDirectories(hooksDir))
                    {
                        foreach(string file in Directory.GetFiles(dir))
                        {
                            string text = File.ReadAllText(file);
                            try
                            {
                                List<Hook> deserialized = JsonConvert.DeserializeObject<List<Hook>>(text, new JsonSerializerSettings { Converters = new List<JsonConverter> { new Hook.Convertor() } });
                                hooks.AddRange(deserialized);
                                //TODO: Update category name
                            }
                            catch (Exception e)
                            {
                                throw new InvalidOperationException("Something went wrong");
                            }
                        }
                    }

                    foreach (Hook hook in hooks)
                    {
                        Manifest manifest = project.Manifests.FirstOrDefault(m => m.AssemblyName == hook.AssemblyName);

                        if (manifest == null)
                        {
                            manifest = new Manifest { AssemblyName = hook.AssemblyName };
                        }

                        manifest.Hooks.Add(hook);
                    }

                    return project;
                }
            //}
            return null;
        }

        public static Project Load(string filename, string overrideTarget)
        {
            Project project = Load(filename);

            if (project == null)
            {
                return null;
            }

            project.TargetDirectory = overrideTarget;
            return project;
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
