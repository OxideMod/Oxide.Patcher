namespace Oxide.Patcher.Views
{
    partial class FieldViewControl
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
            this.detailsgroup = new System.Windows.Forms.GroupBox();
            this.detailstable = new System.Windows.Forms.TableLayoutPanel();
            this.typenametextbox = new System.Windows.Forms.TextBox();
            this.assemblytextbox = new System.Windows.Forms.TextBox();
            this.typenamelabel = new System.Windows.Forms.Label();
            this.assemblylabel = new System.Windows.Forms.Label();
            this.namelabel = new System.Windows.Forms.Label();
            this.nametextbox = new System.Windows.Forms.TextBox();
            this.buttonholder = new System.Windows.Forms.FlowLayoutPanel();
            this.flagbutton = new System.Windows.Forms.Button();
            this.unflagbutton = new System.Windows.Forms.Button();
            this.applybutton = new System.Windows.Forms.Button();
            this.deletebutton = new System.Windows.Forms.Button();
            this.fieldsettingstab = new System.Windows.Forms.TabPage();
            this.tabview = new System.Windows.Forms.TabControl();
            this.detailsgroup.SuspendLayout();
            this.detailstable.SuspendLayout();
            this.buttonholder.SuspendLayout();
            this.tabview.SuspendLayout();
            this.SuspendLayout();
            // 
            // detailsgroup
            // 
            this.detailsgroup.Controls.Add(this.detailstable);
            this.detailsgroup.Dock = System.Windows.Forms.DockStyle.Top;
            this.detailsgroup.Location = new System.Drawing.Point(5, 5);
            this.detailsgroup.Name = "detailsgroup";
            this.detailsgroup.Size = new System.Drawing.Size(623, 140);
            this.detailsgroup.TabIndex = 2;
            this.detailsgroup.TabStop = false;
            this.detailsgroup.Text = "Details";
            // 
            // detailstable
            // 
            this.detailstable.ColumnCount = 2;
            this.detailstable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.detailstable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.detailstable.Controls.Add(this.typenametextbox, 1, 1);
            this.detailstable.Controls.Add(this.assemblytextbox, 1, 0);
            this.detailstable.Controls.Add(this.typenamelabel, 0, 1);
            this.detailstable.Controls.Add(this.assemblylabel, 0, 0);
            this.detailstable.Controls.Add(this.namelabel, 0, 2);
            this.detailstable.Controls.Add(this.nametextbox, 1, 2);
            this.detailstable.Controls.Add(this.buttonholder, 0, 7);
            this.detailstable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailstable.Location = new System.Drawing.Point(3, 16);
            this.detailstable.Name = "detailstable";
            this.detailstable.RowCount = 8;
            this.detailstable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.detailstable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.detailstable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.detailstable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.detailstable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.detailstable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.detailstable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.detailstable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.detailstable.Size = new System.Drawing.Size(617, 121);
            this.detailstable.TabIndex = 0;
            // 
            // typenametextbox
            // 
            this.typenametextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.typenametextbox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.typenametextbox.Location = new System.Drawing.Point(135, 32);
            this.typenametextbox.Name = "typenametextbox";
            this.typenametextbox.ReadOnly = true;
            this.typenametextbox.Size = new System.Drawing.Size(479, 23);
            this.typenametextbox.TabIndex = 1;
            this.typenametextbox.TabStop = false;
            // 
            // assemblytextbox
            // 
            this.assemblytextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.assemblytextbox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.assemblytextbox.Location = new System.Drawing.Point(135, 3);
            this.assemblytextbox.Name = "assemblytextbox";
            this.assemblytextbox.ReadOnly = true;
            this.assemblytextbox.Size = new System.Drawing.Size(479, 23);
            this.assemblytextbox.TabIndex = 1;
            this.assemblytextbox.TabStop = false;
            // 
            // typenamelabel
            // 
            this.typenamelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.typenamelabel.Location = new System.Drawing.Point(3, 29);
            this.typenamelabel.Name = "typenamelabel";
            this.typenamelabel.Size = new System.Drawing.Size(126, 29);
            this.typenamelabel.TabIndex = 1;
            this.typenamelabel.Text = "Type Name:";
            this.typenamelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // assemblylabel
            // 
            this.assemblylabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.assemblylabel.Location = new System.Drawing.Point(3, 0);
            this.assemblylabel.Name = "assemblylabel";
            this.assemblylabel.Size = new System.Drawing.Size(126, 29);
            this.assemblylabel.TabIndex = 1;
            this.assemblylabel.Text = "Assembly:";
            this.assemblylabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // namelabel
            // 
            this.namelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.namelabel.Location = new System.Drawing.Point(3, 58);
            this.namelabel.Name = "namelabel";
            this.namelabel.Size = new System.Drawing.Size(126, 29);
            this.namelabel.TabIndex = 0;
            this.namelabel.Text = "Field Name:";
            this.namelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // nametextbox
            // 
            this.nametextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nametextbox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nametextbox.Location = new System.Drawing.Point(135, 61);
            this.nametextbox.Name = "nametextbox";
            this.nametextbox.Size = new System.Drawing.Size(479, 23);
            this.nametextbox.TabIndex = 0;
            this.nametextbox.TabStop = false;
            this.nametextbox.TextChanged += new System.EventHandler(this.nametextbox_TextChanged);
            // 
            // buttonholder
            // 
            this.detailstable.SetColumnSpan(this.buttonholder, 2);
            this.buttonholder.Controls.Add(this.flagbutton);
            this.buttonholder.Controls.Add(this.unflagbutton);
            this.buttonholder.Controls.Add(this.applybutton);
            this.buttonholder.Controls.Add(this.deletebutton);
            this.buttonholder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonholder.Location = new System.Drawing.Point(3, 90);
            this.buttonholder.Name = "buttonholder";
            this.buttonholder.Size = new System.Drawing.Size(611, 28);
            this.buttonholder.TabIndex = 5;
            // 
            // flagbutton
            // 
            this.flagbutton.Location = new System.Drawing.Point(3, 3);
            this.flagbutton.Name = "flagbutton";
            this.flagbutton.Size = new System.Drawing.Size(55, 23);
            this.flagbutton.TabIndex = 5;
            this.flagbutton.Text = "Flag";
            this.flagbutton.UseVisualStyleBackColor = true;
            this.flagbutton.Click += new System.EventHandler(this.flagbutton_Click);
            // 
            // unflagbutton
            // 
            this.unflagbutton.Location = new System.Drawing.Point(64, 3);
            this.unflagbutton.Name = "unflagbutton";
            this.unflagbutton.Size = new System.Drawing.Size(62, 23);
            this.unflagbutton.TabIndex = 6;
            this.unflagbutton.Text = "Unflag";
            this.unflagbutton.UseVisualStyleBackColor = true;
            this.unflagbutton.Click += new System.EventHandler(this.unflagbutton_Click);
            // 
            // applybutton
            // 
            this.applybutton.Enabled = false;
            this.applybutton.Location = new System.Drawing.Point(132, 3);
            this.applybutton.Name = "applybutton";
            this.applybutton.Size = new System.Drawing.Size(96, 23);
            this.applybutton.TabIndex = 9;
            this.applybutton.Text = "Apply Changes";
            this.applybutton.UseVisualStyleBackColor = true;
            this.applybutton.Click += new System.EventHandler(this.applybutton_Click);
            // 
            // deletebutton
            // 
            this.deletebutton.Location = new System.Drawing.Point(234, 3);
            this.deletebutton.Name = "deletebutton";
            this.deletebutton.Size = new System.Drawing.Size(87, 23);
            this.deletebutton.TabIndex = 10;
            this.deletebutton.Text = "Remove";
            this.deletebutton.UseVisualStyleBackColor = true;
            this.deletebutton.Click += new System.EventHandler(this.deletebutton_Click);
            // 
            // fieldsettingstab
            // 
            this.fieldsettingstab.Location = new System.Drawing.Point(4, 22);
            this.fieldsettingstab.Name = "fieldsettingstab";
            this.fieldsettingstab.Padding = new System.Windows.Forms.Padding(3);
            this.fieldsettingstab.Size = new System.Drawing.Size(615, 303);
            this.fieldsettingstab.TabIndex = 0;
            this.fieldsettingstab.Text = "Field Settings";
            this.fieldsettingstab.UseVisualStyleBackColor = true;
            // 
            // tabview
            // 
            this.tabview.Controls.Add(this.fieldsettingstab);
            this.tabview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabview.Location = new System.Drawing.Point(5, 145);
            this.tabview.Name = "tabview";
            this.tabview.SelectedIndex = 0;
            this.tabview.Size = new System.Drawing.Size(623, 329);
            this.tabview.TabIndex = 12;
            // 
            // FieldViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabview);
            this.Controls.Add(this.detailsgroup);
            this.Name = "FieldViewControl";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Size = new System.Drawing.Size(633, 479);
            this.detailsgroup.ResumeLayout(false);
            this.detailstable.ResumeLayout(false);
            this.detailstable.PerformLayout();
            this.buttonholder.ResumeLayout(false);
            this.tabview.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox detailsgroup;
        private System.Windows.Forms.TableLayoutPanel detailstable;
        private System.Windows.Forms.Label namelabel;
        private System.Windows.Forms.TextBox nametextbox;
        private System.Windows.Forms.FlowLayoutPanel buttonholder;
        private System.Windows.Forms.Button flagbutton;
        private System.Windows.Forms.Button unflagbutton;
        private System.Windows.Forms.Label typenamelabel;
        private System.Windows.Forms.Label assemblylabel;
        private System.Windows.Forms.TextBox typenametextbox;
        private System.Windows.Forms.TextBox assemblytextbox;
        private System.Windows.Forms.TabPage fieldsettingstab;
        private System.Windows.Forms.TabControl tabview;
        private System.Windows.Forms.Button applybutton;
        private System.Windows.Forms.Button deletebutton;
    }
}
