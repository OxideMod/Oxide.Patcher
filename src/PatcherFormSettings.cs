using Newtonsoft.Json;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Oxide.Patcher
{
    /// <summary>
    /// A set of persistent window settings
    /// </summary>
    public class PatcherFormSettings
    {
        /// <summary>
        /// Gets or sets the form position
        /// </summary>
        public Point FormPosition { get; set; }

        /// <summary>
        /// Gets or sets the form size
        /// </summary>
        public Size FormSize { get; set; }

        /// <summary>
        /// Gets or sets the window state
        /// </summary>
        public FormWindowState WindowState { get; set; }

        /// <summary>
        /// Gets or sets the last directory used to open or save a project
        /// </summary>
        public string LastProjectDirectory { get; set; }

        /// <summary>
        /// Gets or sets the last target directory used
        /// </summary>
        public string LastTargetDirectory { get; set; }

        // The settings filename
        private const string filename = "formsettings.json";

        /// <summary>
        /// Initializes a new instance of the PatcherFormSettings class with sensible defaults
        /// </summary>
        public PatcherFormSettings()
        {
            // Fill in defaults
            Rectangle workingarea = Screen.GetWorkingArea(new Point(0, 0));
            FormPosition = new Point(workingarea.Left + workingarea.Width / 5, workingarea.Top + workingarea.Height / 5);
            FormSize = new Size(workingarea.Width * 3 / 5, workingarea.Height * 3 / 5);
            WindowState = FormWindowState.Normal;
            LastProjectDirectory = "";
            LastTargetDirectory = "";
        }

        /// <summary>
        /// Saves these settings
        /// </summary>
        public void Save()
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        /// <summary>
        /// Loads the settings
        /// </summary>
        /// <returns></returns>
        public static PatcherFormSettings Load()
        {
            if (!File.Exists(filename))
            {
                return new PatcherFormSettings();
            }

            string text = File.ReadAllText(filename);
            return JsonConvert.DeserializeObject<PatcherFormSettings>(text);
        }
    }
}
