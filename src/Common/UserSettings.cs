using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Oxide.Patcher.Common
{
    /// <summary>
    /// A set of persistent window settings
    /// </summary>
    public class UserSettings
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

        // The settings filename
        private const string FileName = "oxide-patcher-settings.json";

        private UserSettings Initialise()
        {
            // Fill in defaults
            Rectangle workingarea = Screen.GetWorkingArea(new Point(0, 0));
            FormPosition = new Point(workingarea.Left + workingarea.Width / 5, workingarea.Top + workingarea.Height / 5);
            FormSize = new Size(workingarea.Width * 3 / 5, workingarea.Height * 3 / 5);
            WindowState = FormWindowState.Normal;
            LastProjectDirectory = string.Empty;

            return this;
        }

        /// <summary>
        /// Saves these settings
        /// </summary>
        public void Save()
        {
            File.WriteAllText(FileName, JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        /// <summary>
        /// Loads the settings
        /// </summary>
        /// <returns></returns>
        public static UserSettings Load()
        {
            if (!File.Exists(FileName))
            {
                return new UserSettings().Initialise();
            }

            string text = File.ReadAllText(FileName);
            return JsonConvert.DeserializeObject<UserSettings>(text);
        }
    }
}
