namespace Oxide.Patcher
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
            this.selectfilenamebutton = new System.Windows.Forms.Button();
            this.filenamelabel = new System.Windows.Forms.Label();
            this.filenametextbox = new System.Windows.Forms.TextBox();
            this.directorylabel = new System.Windows.Forms.Label();
            this.namelabel = new System.Windows.Forms.Label();
            this.nametextbox = new System.Windows.Forms.TextBox();
            this.selectdirectorybutton = new System.Windows.Forms.Button();
            this.directorytextbox = new System.Windows.Forms.TextBox();
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
            this.settingsgroup.Size = new System.Drawing.Size(505, 94);
            this.settingsgroup.TabIndex = 1;
            this.settingsgroup.TabStop = false;
            this.settingsgroup.Text = "Project Settings";
            // 
            // tablepanel
            // 
            this.tablepanel.ColumnCount = 3;
            this.tablepanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tablepanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tablepanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tablepanel.Controls.Add(this.selectfilenamebutton, 2, 2);
            this.tablepanel.Controls.Add(this.filenamelabel, 0, 2);
            this.tablepanel.Controls.Add(this.filenametextbox, 0, 2);
            this.tablepanel.Controls.Add(this.directorylabel, 0, 1);
            this.tablepanel.Controls.Add(this.namelabel, 0, 0);
            this.tablepanel.Controls.Add(this.nametextbox, 1, 0);
            this.tablepanel.Controls.Add(this.selectdirectorybutton, 2, 1);
            this.tablepanel.Controls.Add(this.directorytextbox, 1, 1);
            this.tablepanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablepanel.Location = new System.Drawing.Point(3, 16);
            this.tablepanel.Name = "tablepanel";
            this.tablepanel.RowCount = 3;
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tablepanel.Size = new System.Drawing.Size(499, 75);
            this.tablepanel.TabIndex = 0;
            // 
            // selectfilenamebutton
            // 
            this.selectfilenamebutton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectfilenamebutton.Enabled = false;
            this.selectfilenamebutton.Location = new System.Drawing.Point(469, 53);
            this.selectfilenamebutton.Name = "selectfilenamebutton";
            this.selectfilenamebutton.Size = new System.Drawing.Size(27, 19);
            this.selectfilenamebutton.TabIndex = 8;
            this.selectfilenamebutton.Text = "...";
            this.selectfilenamebutton.UseVisualStyleBackColor = true;
            // 
            // filenamelabel
            // 
            this.filenamelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filenamelabel.Location = new System.Drawing.Point(3, 50);
            this.filenamelabel.Name = "filenamelabel";
            this.filenamelabel.Size = new System.Drawing.Size(94, 25);
            this.filenamelabel.TabIndex = 6;
            this.filenamelabel.Text = "Filename:";
            this.filenamelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // filenametextbox
            // 
            this.filenametextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filenametextbox.Enabled = false;
            this.filenametextbox.Location = new System.Drawing.Point(103, 53);
            this.filenametextbox.Name = "filenametextbox";
            this.filenametextbox.Size = new System.Drawing.Size(360, 20);
            this.filenametextbox.TabIndex = 7;
            // 
            // directorylabel
            // 
            this.directorylabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.directorylabel.Location = new System.Drawing.Point(3, 25);
            this.directorylabel.Name = "directorylabel";
            this.directorylabel.Size = new System.Drawing.Size(94, 25);
            this.directorylabel.TabIndex = 2;
            this.directorylabel.Text = "Target Directory:";
            this.directorylabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // namelabel
            // 
            this.namelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.namelabel.Location = new System.Drawing.Point(3, 0);
            this.namelabel.Name = "namelabel";
            this.namelabel.Size = new System.Drawing.Size(94, 25);
            this.namelabel.TabIndex = 0;
            this.namelabel.Text = "Name:";
            this.namelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nametextbox
            // 
            this.tablepanel.SetColumnSpan(this.nametextbox, 2);
            this.nametextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nametextbox.Location = new System.Drawing.Point(103, 3);
            this.nametextbox.MaxLength = 100;
            this.nametextbox.Name = "nametextbox";
            this.nametextbox.Size = new System.Drawing.Size(393, 20);
            this.nametextbox.TabIndex = 3;
            this.nametextbox.Text = "Untitled Oxide Project";
            // 
            // selectdirectorybutton
            // 
            this.selectdirectorybutton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectdirectorybutton.Enabled = false;
            this.selectdirectorybutton.Location = new System.Drawing.Point(469, 28);
            this.selectdirectorybutton.Name = "selectdirectorybutton";
            this.selectdirectorybutton.Size = new System.Drawing.Size(27, 19);
            this.selectdirectorybutton.TabIndex = 4;
            this.selectdirectorybutton.Text = "...";
            this.selectdirectorybutton.UseVisualStyleBackColor = true;
            // 
            // directorytextbox
            // 
            this.directorytextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.directorytextbox.Location = new System.Drawing.Point(103, 28);
            this.directorytextbox.Name = "directorytextbox";
            this.directorytextbox.Size = new System.Drawing.Size(360, 20);
            this.directorytextbox.TabIndex = 5;
            // 
            // savebutton
            // 
            this.savebutton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.savebutton.Location = new System.Drawing.Point(9, 202);
            this.savebutton.Name = "savebutton";
            this.savebutton.Size = new System.Drawing.Size(106, 23);
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
            this.Size = new System.Drawing.Size(505, 228);
            this.Load += new System.EventHandler(this.ProjectSettingsControl_Load);
            this.settingsgroup.ResumeLayout(false);
            this.tablepanel.ResumeLayout(false);
            this.tablepanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox settingsgroup;
        private System.Windows.Forms.TableLayoutPanel tablepanel;
        private System.Windows.Forms.Button selectfilenamebutton;
        private System.Windows.Forms.Label filenamelabel;
        private System.Windows.Forms.TextBox filenametextbox;
        private System.Windows.Forms.Label directorylabel;
        private System.Windows.Forms.Label namelabel;
        private System.Windows.Forms.TextBox nametextbox;
        private System.Windows.Forms.Button selectdirectorybutton;
        private System.Windows.Forms.TextBox directorytextbox;
        private System.Windows.Forms.Button savebutton;
    }
}
