using System;
using System.IO;
using System.Windows.Forms;

namespace Oxide.Patcher
{
    public partial class ProjectSettingsControl : UserControl
    {
        public Project ProjectObject { get; set; }

        public string ProjectFilename { get; set; }

        public ProjectSettingsControl()
        {
            InitializeComponent();
        }

        private void ProjectSettingsControl_Load(object sender, EventArgs e)
        {
            nametextbox.Text = ProjectObject.Name;
            directorytextbox.Text = ProjectObject.TargetDirectory;
            filenametextbox.Text = ProjectFilename;
            docspathtextbox.Text = PatcherForm.MainForm?.Settings?.DocsPath ?? string.Empty;
        }

        private void selectdocspathbutton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                ofd.Title = "Select Docs JSON File";
                if (!string.IsNullOrEmpty(docspathtextbox.Text) && File.Exists(docspathtextbox.Text))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(docspathtextbox.Text);
                    ofd.FileName = Path.GetFileName(docspathtextbox.Text);
                }
                else if (!string.IsNullOrEmpty(ProjectObject?.TargetDirectory) && Directory.Exists(ProjectObject.TargetDirectory))
                {
                    ofd.InitialDirectory = ProjectObject.TargetDirectory;
                }

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    docspathtextbox.Text = ofd.FileName;
                    if (PatcherForm.MainForm?.Settings != null)
                    {
                        PatcherForm.MainForm.Settings.DocsPath = ofd.FileName;
                        PatcherForm.MainForm.Settings.Save();
                    }
                }
            }
        }

        private void savebutton_Click(object sender, EventArgs e)
        {
            // Verify
            if (!Directory.Exists(directorytextbox.Text))
            {
                MessageBox.Show(this, "The target directory is invalid.", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Directory.Exists(Path.GetDirectoryName(filenametextbox.Text)))
            {
                MessageBox.Show(this, "The filename is invalid.", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (nametextbox.TextLength == 0)
            {
                MessageBox.Show(this, "The project name is invalid.", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Save
            ProjectObject.Name = nametextbox.Text;
            ProjectObject.TargetDirectory = directorytextbox.Text;
            ProjectObject.Save(ProjectFilename);
        }
    }
}
