using OxidePatcher.Projects;
using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace OxidePatcher
{
    public partial class NewProjectForm : Form
    {
        private const string CONFIG_FILE_FILTER = "Oxide Project Configuration|*.opj.config";
        private const string PROJECT_FILE_FILTER = "Oxide Project|*.opj";

        private Project projectToUpdate;

        public NewProjectForm() : this(null) { }

        public NewProjectForm(Project projectToUpdate)
        {
            InitializeComponent();

            this.projectToUpdate = projectToUpdate;
        }

        private void ProjectForm_Load(object sender, EventArgs e)
        {
            if (projectToUpdate != null)
            {
                this.Text = "Update Project File";
                this.createbutton.Text = "Update";

                ProjectNameTextbox.Text = projectToUpdate.Name;
                ProjectFileTextbox.Text = projectToUpdate.ProjectFilePath;
                ConfigFileTextbox.Text = projectToUpdate.ConfigurationPath;
                AssembliesDirectoryTextbox.Text = projectToUpdate.Configuration?.AssembliesSourceDirectory;

                return;
            }

            this.Text = "Create Project";
            this.createbutton.Text = "Create";
        }

        private void ProjectFileBrowseButton_Click(object sender, EventArgs eventArgs)
        {
            this.SelectFileDialog.Filter = PROJECT_FILE_FILTER;

            var dialogResult = SelectFileDialog.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                ProjectFileTextbox.Text = SelectFileDialog.FileName;
            }
        }

        private void ConfigFileBrowseButton_Click(object sender, EventArgs eventArgs)
        {
            this.SelectFileDialog.Filter = CONFIG_FILE_FILTER;

            var dialogResult = SelectFileDialog.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                ConfigFileTextbox.Text = SelectFileDialog.FileName;
            }
        }

        private void AssembliesDirectoryBrowseButton_Click(object sender, EventArgs e)
        {
            var dialogResult = SelectFolderDialog.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                if (!Directory.EnumerateFiles(SelectFolderDialog.SelectedPath).Any(x => x.EndsWith(".dll") || x.EndsWith(".exe")))
                {
                    var confirmResult = MessageBox.Show(this, "The specified directory does not contain any dll files. Continue anyway?", "Oxide Patcher", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirmResult == DialogResult.No)
                    {
                        return;
                    }
                }

                ProjectFileTextbox.Text = SelectFolderDialog.SelectedPath;
            }
        }

        private void cancelbutton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void createbutton_Click(object sender, EventArgs e)
        {
            // TODO: Dry up duplicate code with ProjectSettingsControl
            // Validate
            var validationMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(ProjectNameTextbox.Text))
            {
                validationMessage += $"A valid project name is required.{Environment.NewLine}";
            }

            if (string.IsNullOrWhiteSpace(ProjectFileTextbox.Text) || !Directory.Exists(Path.GetDirectoryName(ProjectFileTextbox.Text)))
            {
                validationMessage += $"The project file path is invalid.{Environment.NewLine}";
            }
            
            if (string.IsNullOrWhiteSpace(ConfigFileTextbox.Text) || !Directory.Exists(Path.GetDirectoryName(ConfigFileTextbox.Text)))
            {
                validationMessage += $"The config file path is invalid.{Environment.NewLine}";
            }


            if (!Directory.Exists(AssembliesDirectoryTextbox.Text))
            {
                validationMessage += $"The assemblies directory is invalid.{Environment.NewLine}";
            }

            if (!string.IsNullOrEmpty(validationMessage))
            {
                MessageBox.Show(this, validationMessage, "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Create new project if we don't already have one to update
            Project newProject = projectToUpdate ?? new Project();

            newProject.Name = ProjectNameTextbox.Text;
            newProject.ConfigurationPath = ConfigFileTextbox.Text;
            newProject.Configuration = new ProjectConfiguration();
            newProject.Configuration.AssembliesSourceDirectory = AssembliesDirectoryTextbox.Text;
            newProject.ProjectFilePath = ProjectFileTextbox.Text;
            newProject.OxidePatcherVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            // saving the project will also save the config file
            newProject.Save();

            // Set parent form to load / reload it
            PatcherForm owner = Owner as PatcherForm;
            owner.OpenProject(newProject.ProjectFilePath);

            // Close
            Close();
        }
    }
}
