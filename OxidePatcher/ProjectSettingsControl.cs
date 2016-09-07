using System;
using System.IO;
using System.Windows.Forms;

namespace OxidePatcher
{
    public partial class ProjectSettingsControl : UserControl
    {
        public Project ProjectObject { get; set; }

        public ProjectSettingsControl()
        {
            InitializeComponent();
        }

        private void ProjectSettingsControl_Load(object sender, EventArgs e)
        {
            nametextbox.Text = ProjectObject.Name;
            ProjectFileTextbox.Text = ProjectObject.ProjectFilePath; 
            ConfigFileTextbox.Text = ProjectObject.ConfigurationPath;
        }

        private void savebutton_Click(object sender, EventArgs e)
        {
            // Validate
            var validationMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(nametextbox.Text))
            {
                validationMessage += $"A valid project name is required.{Environment.NewLine}";
            }

            if (!Directory.Exists(Path.GetDirectoryName(ProjectFileTextbox.Text)))
            {
                validationMessage += $"The project file path is invalid.{Environment.NewLine}";
            }

            if (!Directory.Exists(Path.GetDirectoryName(ConfigFileTextbox.Text)))
            {
                validationMessage += $"The config file path is invalid.{Environment.NewLine}";
            }


            if (!Directory.Exists(AssembliesDirectoryTextbox.Text))
            {
                validationMessage += $"The assemblies directory is invalid.{Environment.NewLine}";
            }

            if(!string.IsNullOrEmpty(validationMessage))
            {
                MessageBox.Show(this, validationMessage, "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Save
            ProjectObject.Name = nametextbox.Text;
            ProjectObject.ProjectFilePath = ProjectFileTextbox.Text;

            ProjectObject.ConfigurationPath = ConfigFileTextbox.Text;

            ProjectObject.Configuration = ProjectObject.Configuration ?? new ProjectConfiguration();
            ProjectObject.Configuration.AssembliesSourceDirectory = AssembliesDirectoryTextbox.Text;


            ProjectObject.Save();
        }
    }
}
