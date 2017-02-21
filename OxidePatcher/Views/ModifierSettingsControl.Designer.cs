namespace OxidePatcher.Views
{
    partial class ModifierSettingsControl
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.isPrivateSetter = new System.Windows.Forms.CheckBox();
            this.isPublicSetter = new System.Windows.Forms.CheckBox();
            this.isStatic = new System.Windows.Forms.CheckBox();
            this.isPrivate = new System.Windows.Forms.CheckBox();
            this.isPublic = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 135F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.isPrivateSetter, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.isPublicSetter, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.isStatic, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.isPrivate, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.isPublic, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(344, 78);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // isPrivateSetter
            // 
            this.isPrivateSetter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.isPrivateSetter.AutoSize = true;
            this.isPrivateSetter.Location = new System.Drawing.Point(138, 30);
            this.isPrivateSetter.Name = "isPrivateSetter";
            this.isPrivateSetter.Size = new System.Drawing.Size(59, 17);
            this.isPrivateSetter.TabIndex = 6;
            this.isPrivateSetter.Text = "Private";
            this.isPrivateSetter.UseVisualStyleBackColor = true;
            this.isPrivateSetter.Visible = false;
            this.isPrivateSetter.CheckedChanged += new System.EventHandler(this.isPrivateSetter_CheckedChanged);
            // 
            // isPublicSetter
            // 
            this.isPublicSetter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.isPublicSetter.AutoSize = true;
            this.isPublicSetter.Location = new System.Drawing.Point(138, 4);
            this.isPublicSetter.Name = "isPublicSetter";
            this.isPublicSetter.Size = new System.Drawing.Size(55, 17);
            this.isPublicSetter.TabIndex = 5;
            this.isPublicSetter.Text = "Public";
            this.isPublicSetter.UseVisualStyleBackColor = true;
            this.isPublicSetter.Visible = false;
            this.isPublicSetter.CheckedChanged += new System.EventHandler(this.isPublicSetter_CheckedChanged);
            // 
            // isStatic
            // 
            this.isStatic.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.isStatic.AutoSize = true;
            this.isStatic.Location = new System.Drawing.Point(3, 56);
            this.isStatic.Name = "isStatic";
            this.isStatic.Size = new System.Drawing.Size(53, 17);
            this.isStatic.TabIndex = 4;
            this.isStatic.Text = "Static";
            this.isStatic.UseVisualStyleBackColor = true;
            this.isStatic.CheckedChanged += new System.EventHandler(this.isStatic_CheckedChanged);
            // 
            // isPrivate
            // 
            this.isPrivate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.isPrivate.AutoSize = true;
            this.isPrivate.Location = new System.Drawing.Point(3, 30);
            this.isPrivate.Name = "isPrivate";
            this.isPrivate.Size = new System.Drawing.Size(59, 17);
            this.isPrivate.TabIndex = 2;
            this.isPrivate.Text = "Private";
            this.isPrivate.UseVisualStyleBackColor = true;
            this.isPrivate.CheckedChanged += new System.EventHandler(this.isPrivate_CheckedChanged);
            // 
            // isPublic
            // 
            this.isPublic.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.isPublic.AutoSize = true;
            this.isPublic.Location = new System.Drawing.Point(3, 4);
            this.isPublic.Name = "isPublic";
            this.isPublic.Size = new System.Drawing.Size(55, 17);
            this.isPublic.TabIndex = 0;
            this.isPublic.Text = "Public";
            this.isPublic.UseVisualStyleBackColor = true;
            this.isPublic.CheckedChanged += new System.EventHandler(this.isPublic_CheckedChanged);
            // 
            // ModifierSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ModifierSettingsControl";
            this.Size = new System.Drawing.Size(415, 198);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox isPrivateSetter;
        private System.Windows.Forms.CheckBox isPublicSetter;
        private System.Windows.Forms.CheckBox isStatic;
        private System.Windows.Forms.CheckBox isPrivate;
        private System.Windows.Forms.CheckBox isPublic;
    }
}
