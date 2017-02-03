namespace OxidePatcher
{
    partial class ProjectForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.createbutton = new System.Windows.Forms.Button();
            this.cancelbutton = new System.Windows.Forms.Button();
            this.SelectFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SelectFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.tablepanel = new System.Windows.Forms.TableLayoutPanel();
            this.AssembliesDirectoryBrowseButton = new System.Windows.Forms.Button();
            this.AssembliesDirectoryTextbox = new System.Windows.Forms.TextBox();
            this.AssembliesDirectoryLabel = new System.Windows.Forms.Label();
            this.ProjectFileLabel = new System.Windows.Forms.Label();
            this.ProjectNameLabel = new System.Windows.Forms.Label();
            this.ProjectNameTextbox = new System.Windows.Forms.TextBox();
            this.ProjectFileBrowseButton = new System.Windows.Forms.Button();
            this.ProjectFileTextbox = new System.Windows.Forms.TextBox();
            this.settingsgroup = new System.Windows.Forms.GroupBox();
            this.tablepanel.SuspendLayout();
            this.settingsgroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // createbutton
            // 
            this.createbutton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.createbutton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.createbutton.Location = new System.Drawing.Point(12, 174);
            this.createbutton.Name = "createbutton";
            this.createbutton.Size = new System.Drawing.Size(219, 35);
            this.createbutton.TabIndex = 1;
            this.createbutton.Text = "Create";
            this.createbutton.UseVisualStyleBackColor = true;
            this.createbutton.Click += new System.EventHandler(this.createbutton_Click);
            // 
            // cancelbutton
            // 
            this.cancelbutton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelbutton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelbutton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancelbutton.Location = new System.Drawing.Point(350, 174);
            this.cancelbutton.Name = "cancelbutton";
            this.cancelbutton.Size = new System.Drawing.Size(227, 35);
            this.cancelbutton.TabIndex = 2;
            this.cancelbutton.Text = "Cancel";
            this.cancelbutton.UseVisualStyleBackColor = true;
            this.cancelbutton.Click += new System.EventHandler(this.cancelbutton_Click);
            // 
            // SelectFileDialog
            // 
            this.SelectFileDialog.DefaultExt = "opj";
            this.SelectFileDialog.Filter = "Oxide project|*.opj";
            // 
            // tablepanel
            // 
            this.tablepanel.ColumnCount = 3;
            this.tablepanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tablepanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tablepanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tablepanel.Controls.Add(this.AssembliesDirectoryBrowseButton, 2, 2);
            this.tablepanel.Controls.Add(this.AssembliesDirectoryTextbox, 1, 2);
            this.tablepanel.Controls.Add(this.AssembliesDirectoryLabel, 0, 2);
            this.tablepanel.Controls.Add(this.ProjectFileLabel, 0, 1);
            this.tablepanel.Controls.Add(this.ProjectNameLabel, 0, 0);
            this.tablepanel.Controls.Add(this.ProjectNameTextbox, 1, 0);
            this.tablepanel.Controls.Add(this.ProjectFileBrowseButton, 2, 1);
            this.tablepanel.Controls.Add(this.ProjectFileTextbox, 1, 1);
            this.tablepanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablepanel.Location = new System.Drawing.Point(3, 16);
            this.tablepanel.Name = "tablepanel";
            this.tablepanel.RowCount = 3;
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tablepanel.Size = new System.Drawing.Size(559, 75);
            this.tablepanel.TabIndex = 0;
            // 
            // AssembliesDirectoryBrowseButton
            // 
            this.AssembliesDirectoryBrowseButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AssembliesDirectoryBrowseButton.Location = new System.Drawing.Point(529, 53);
            this.AssembliesDirectoryBrowseButton.Name = "AssembliesDirectoryBrowseButton";
            this.AssembliesDirectoryBrowseButton.Size = new System.Drawing.Size(27, 19);
            this.AssembliesDirectoryBrowseButton.TabIndex = 11;
            this.AssembliesDirectoryBrowseButton.Text = "...";
            this.AssembliesDirectoryBrowseButton.UseVisualStyleBackColor = true;
            this.AssembliesDirectoryBrowseButton.Click += new System.EventHandler(this.AssembliesDirectoryBrowseButton_Click);
            // 
            // AssembliesDirectoryTextbox
            // 
            this.AssembliesDirectoryTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AssembliesDirectoryTextbox.Location = new System.Drawing.Point(123, 53);
            this.AssembliesDirectoryTextbox.Name = "AssembliesDirectoryTextbox";
            this.AssembliesDirectoryTextbox.Size = new System.Drawing.Size(400, 20);
            this.AssembliesDirectoryTextbox.TabIndex = 10;
            // 
            // AssembliesDirectoryLabel
            // 
            this.AssembliesDirectoryLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AssembliesDirectoryLabel.Location = new System.Drawing.Point(3, 50);
            this.AssembliesDirectoryLabel.Name = "AssembliesDirectoryLabel";
            this.AssembliesDirectoryLabel.Size = new System.Drawing.Size(114, 25);
            this.AssembliesDirectoryLabel.TabIndex = 9;
            this.AssembliesDirectoryLabel.Text = "Assemblies Directory:";
            this.AssembliesDirectoryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ProjectFileLabel
            // 
            this.ProjectFileLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProjectFileLabel.Location = new System.Drawing.Point(3, 25);
            this.ProjectFileLabel.Name = "ProjectFileLabel";
            this.ProjectFileLabel.Size = new System.Drawing.Size(114, 25);
            this.ProjectFileLabel.TabIndex = 2;
            this.ProjectFileLabel.Text = "Project File:";
            this.ProjectFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ProjectNameLabel
            // 
            this.ProjectNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProjectNameLabel.Location = new System.Drawing.Point(3, 0);
            this.ProjectNameLabel.Name = "ProjectNameLabel";
            this.ProjectNameLabel.Size = new System.Drawing.Size(114, 25);
            this.ProjectNameLabel.TabIndex = 0;
            this.ProjectNameLabel.Text = "Project Name:";
            this.ProjectNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ProjectNameTextbox
            // 
            this.tablepanel.SetColumnSpan(this.ProjectNameTextbox, 2);
            this.ProjectNameTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProjectNameTextbox.Location = new System.Drawing.Point(123, 3);
            this.ProjectNameTextbox.MaxLength = 100;
            this.ProjectNameTextbox.Name = "ProjectNameTextbox";
            this.ProjectNameTextbox.Size = new System.Drawing.Size(433, 20);
            this.ProjectNameTextbox.TabIndex = 3;
            this.ProjectNameTextbox.Text = "Untitled Oxide Project";
            // 
            // ProjectFileBrowseButton
            // 
            this.ProjectFileBrowseButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProjectFileBrowseButton.Location = new System.Drawing.Point(529, 28);
            this.ProjectFileBrowseButton.Name = "ProjectFileBrowseButton";
            this.ProjectFileBrowseButton.Size = new System.Drawing.Size(27, 19);
            this.ProjectFileBrowseButton.TabIndex = 4;
            this.ProjectFileBrowseButton.Text = "...";
            this.ProjectFileBrowseButton.UseVisualStyleBackColor = true;
            this.ProjectFileBrowseButton.Click += new System.EventHandler(this.ProjectFileBrowseButton_Click);
            // 
            // ProjectFileTextbox
            // 
            this.ProjectFileTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProjectFileTextbox.Location = new System.Drawing.Point(123, 28);
            this.ProjectFileTextbox.Name = "ProjectFileTextbox";
            this.ProjectFileTextbox.Size = new System.Drawing.Size(400, 20);
            this.ProjectFileTextbox.TabIndex = 5;
            // 
            // settingsgroup
            // 
            this.settingsgroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsgroup.Controls.Add(this.tablepanel);
            this.settingsgroup.Location = new System.Drawing.Point(12, 12);
            this.settingsgroup.Name = "settingsgroup";
            this.settingsgroup.Size = new System.Drawing.Size(565, 94);
            this.settingsgroup.TabIndex = 0;
            this.settingsgroup.TabStop = false;
            this.settingsgroup.Text = "Project Settings";
            // 
            // ProjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelbutton;
            this.ClientSize = new System.Drawing.Size(589, 221);
            this.Controls.Add(this.cancelbutton);
            this.Controls.Add(this.createbutton);
            this.Controls.Add(this.settingsgroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProjectForm";
            this.Text = "Create Project";
            this.Load += new System.EventHandler(this.ProjectForm_Load);
            this.tablepanel.ResumeLayout(false);
            this.tablepanel.PerformLayout();
            this.settingsgroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button createbutton;
        private System.Windows.Forms.Button cancelbutton;
        private System.Windows.Forms.SaveFileDialog SelectFileDialog;
        private System.Windows.Forms.FolderBrowserDialog SelectFolderDialog;
        private System.Windows.Forms.TableLayoutPanel tablepanel;
        private System.Windows.Forms.Label ProjectFileLabel;
        private System.Windows.Forms.Label ProjectNameLabel;
        private System.Windows.Forms.TextBox ProjectNameTextbox;
        private System.Windows.Forms.Button ProjectFileBrowseButton;
        private System.Windows.Forms.TextBox ProjectFileTextbox;
        private System.Windows.Forms.GroupBox settingsgroup;
        private System.Windows.Forms.Label AssembliesDirectoryLabel;
        private System.Windows.Forms.TextBox AssembliesDirectoryTextbox;
        private System.Windows.Forms.Button AssembliesDirectoryBrowseButton;
    }
}