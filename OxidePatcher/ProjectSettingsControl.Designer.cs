namespace OxidePatcher
{
    partial class ProjectSettingsControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.settingsgroup = new System.Windows.Forms.GroupBox();
            this.tablepanel = new System.Windows.Forms.TableLayoutPanel();
            this.AssembliesDirectoryBrowseButton = new System.Windows.Forms.Button();
            this.AssembliesDirectoryTextbox = new System.Windows.Forms.TextBox();
            this.ProjectFileLabel = new System.Windows.Forms.Label();
            this.namelabel = new System.Windows.Forms.Label();
            this.nametextbox = new System.Windows.Forms.TextBox();
            this.ProjectFileBrowseButton = new System.Windows.Forms.Button();
            this.ProjectFileTextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.savebutton = new System.Windows.Forms.Button();
            this.settingsgroup.SuspendLayout();
            this.tablepanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // settingsgroup
            // 
            this.settingsgroup.Controls.Add(this.tablepanel);
            this.settingsgroup.Dock = System.Windows.Forms.DockStyle.Top;
            this.settingsgroup.Location = new System.Drawing.Point(0, 0);
            this.settingsgroup.Name = "settingsgroup";
            this.settingsgroup.Size = new System.Drawing.Size(778, 94);
            this.settingsgroup.TabIndex = 1;
            this.settingsgroup.TabStop = false;
            this.settingsgroup.Text = "Project Settings";
            // 
            // tablepanel
            // 
            this.tablepanel.ColumnCount = 3;
            this.tablepanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 116F));
            this.tablepanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tablepanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tablepanel.Controls.Add(this.AssembliesDirectoryBrowseButton, 2, 2);
            this.tablepanel.Controls.Add(this.AssembliesDirectoryTextbox, 1, 2);
            this.tablepanel.Controls.Add(this.ProjectFileLabel, 0, 1);
            this.tablepanel.Controls.Add(this.namelabel, 0, 0);
            this.tablepanel.Controls.Add(this.nametextbox, 1, 0);
            this.tablepanel.Controls.Add(this.ProjectFileBrowseButton, 2, 1);
            this.tablepanel.Controls.Add(this.ProjectFileTextbox, 1, 1);
            this.tablepanel.Controls.Add(this.label1, 0, 2);
            this.tablepanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablepanel.Location = new System.Drawing.Point(3, 16);
            this.tablepanel.Name = "tablepanel";
            this.tablepanel.RowCount = 3;
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tablepanel.Size = new System.Drawing.Size(772, 75);
            this.tablepanel.TabIndex = 0;
            // 
            // AssembliesDirectoryBrowseButton
            // 
            this.AssembliesDirectoryBrowseButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AssembliesDirectoryBrowseButton.Enabled = false;
            this.AssembliesDirectoryBrowseButton.Location = new System.Drawing.Point(742, 53);
            this.AssembliesDirectoryBrowseButton.Name = "AssembliesDirectoryBrowseButton";
            this.AssembliesDirectoryBrowseButton.Size = new System.Drawing.Size(27, 19);
            this.AssembliesDirectoryBrowseButton.TabIndex = 10;
            this.AssembliesDirectoryBrowseButton.Text = "...";
            this.AssembliesDirectoryBrowseButton.UseVisualStyleBackColor = true;
            // 
            // AssembliesDirectoryTextbox
            // 
            this.AssembliesDirectoryTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AssembliesDirectoryTextbox.Location = new System.Drawing.Point(119, 53);
            this.AssembliesDirectoryTextbox.Name = "AssembliesDirectoryTextbox";
            this.AssembliesDirectoryTextbox.Size = new System.Drawing.Size(617, 20);
            this.AssembliesDirectoryTextbox.TabIndex = 9;
            // 
            // ProjectFileLabel
            // 
            this.ProjectFileLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProjectFileLabel.Location = new System.Drawing.Point(3, 25);
            this.ProjectFileLabel.Name = "ProjectFileLabel";
            this.ProjectFileLabel.Size = new System.Drawing.Size(110, 25);
            this.ProjectFileLabel.TabIndex = 2;
            this.ProjectFileLabel.Text = "Project File:";
            this.ProjectFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // namelabel
            // 
            this.namelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.namelabel.Location = new System.Drawing.Point(3, 0);
            this.namelabel.Name = "namelabel";
            this.namelabel.Size = new System.Drawing.Size(110, 25);
            this.namelabel.TabIndex = 0;
            this.namelabel.Text = "Name:";
            this.namelabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nametextbox
            // 
            this.tablepanel.SetColumnSpan(this.nametextbox, 2);
            this.nametextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nametextbox.Location = new System.Drawing.Point(119, 3);
            this.nametextbox.MaxLength = 100;
            this.nametextbox.Name = "nametextbox";
            this.nametextbox.Size = new System.Drawing.Size(650, 20);
            this.nametextbox.TabIndex = 3;
            this.nametextbox.Text = "Untitled Oxide Project";
            // 
            // ProjectFileBrowseButton
            // 
            this.ProjectFileBrowseButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProjectFileBrowseButton.Enabled = false;
            this.ProjectFileBrowseButton.Location = new System.Drawing.Point(742, 28);
            this.ProjectFileBrowseButton.Name = "ProjectFileBrowseButton";
            this.ProjectFileBrowseButton.Size = new System.Drawing.Size(27, 19);
            this.ProjectFileBrowseButton.TabIndex = 4;
            this.ProjectFileBrowseButton.Text = "...";
            this.ProjectFileBrowseButton.UseVisualStyleBackColor = true;
            // 
            // ProjectFileTextbox
            // 
            this.ProjectFileTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProjectFileTextbox.Location = new System.Drawing.Point(119, 28);
            this.ProjectFileTextbox.Name = "ProjectFileTextbox";
            this.ProjectFileTextbox.Size = new System.Drawing.Size(617, 20);
            this.ProjectFileTextbox.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 25);
            this.label1.TabIndex = 3;
            this.label1.Text = "Assemblies Directory:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // savebutton
            // 
            this.savebutton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.savebutton.Location = new System.Drawing.Point(9, 146);
            this.savebutton.Name = "savebutton";
            this.savebutton.Size = new System.Drawing.Size(240, 44);
            this.savebutton.TabIndex = 2;
            this.savebutton.Text = "Save Changes";
            this.savebutton.UseVisualStyleBackColor = true;
            this.savebutton.Click += new System.EventHandler(this.savebutton_Click);
            // 
            // ProjectSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.savebutton);
            this.Controls.Add(this.settingsgroup);
            this.Name = "ProjectSettingsControl";
            this.Size = new System.Drawing.Size(778, 203);
            this.Load += new System.EventHandler(this.ProjectSettingsControl_Load);
            this.settingsgroup.ResumeLayout(false);
            this.tablepanel.ResumeLayout(false);
            this.tablepanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox settingsgroup;
        private System.Windows.Forms.TableLayoutPanel tablepanel;
        private System.Windows.Forms.Label ProjectFileLabel;
        private System.Windows.Forms.Label namelabel;
        private System.Windows.Forms.TextBox nametextbox;
        private System.Windows.Forms.Button ProjectFileBrowseButton;
        private System.Windows.Forms.TextBox ProjectFileTextbox;
        private System.Windows.Forms.Button savebutton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox AssembliesDirectoryTextbox;
        private System.Windows.Forms.Button AssembliesDirectoryBrowseButton;
    }
}
