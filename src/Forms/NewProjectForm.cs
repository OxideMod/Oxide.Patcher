using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Oxide.Patcher
{
    public partial class NewProjectForm : Form
    {
        public NewProjectForm()
        {
            InitializeComponent();
        }

        private void NewProjectForm_Load(object sender, EventArgs e)
        {
            // PatcherForm owner = Owner as PatcherForm;
            // PatcherFormSettings settings = owner.CurrentSettings;

            // selectdirectorydialog.SelectedPath = settings.LastTargetDirectory;
            directorytextbox.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            //selectfilenamedialog.FileName = settings.LastProjectDirectory;
        }

        private void selectdirectorybutton_Click(object sender, EventArgs e)
        {
            DialogResult result = selectdirectorydialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                if (!Directory.EnumerateFiles(selectdirectorydialog.SelectedPath).Any(x => x.EndsWith(".dll") || x.EndsWith(".exe")))
                {
                    if (MessageBox.Show(this, "The specified directory does not contain any dll files. Continue anyway?", "Oxide Patcher", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        return;
                    }
                }

                //PatcherForm owner = Owner as PatcherForm;

                //PatcherFormSettings settings = owner.CurrentSettings;
                //settings.LastTargetDirectory = selectdirectorydialog.SelectedPath;
                //settings.Save();

                directorytextbox.Text = selectdirectorydialog.SelectedPath;
            }
        }

        private void selectfilenamebutton_Click(object sender, EventArgs e)
        {
            DialogResult result = selectfilenamedialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                //PatcherForm owner = Owner as PatcherForm;

                //PatcherFormSettings settings = owner.CurrentSettings;
                //settings.LastProjectDirectory = Path.GetDirectoryName(selectfilenamedialog.FileName);
                //settings.Save();

                filenametextbox.Text = selectfilenamedialog.FileName;
            }
        }

        private void cancelbutton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void createbutton_Click(object sender, EventArgs e)
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

            // Create project
            Project newproj = new Project();
            newproj.Name = nametextbox.Text;
            newproj.TargetDirectory = directorytextbox.Text;
            newproj.Save(filenametextbox.Text);

            // Set parent form to load it
            PatcherForm owner = Owner as PatcherForm;
            owner.OpenProject(filenametextbox.Text);

            // Close
            Close();
        }
    }
}
