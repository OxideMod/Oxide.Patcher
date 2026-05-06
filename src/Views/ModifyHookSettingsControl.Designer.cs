namespace Oxide.Patcher.Views
{
    partial class ModifyHookSettingsControl
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.argumentOverride = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.injectionindex = new System.Windows.Forms.NumericUpDown();
            this.illist = new System.Windows.Forms.ListBox();
            this.listContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.NewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RemoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.removecount = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.injectionindex)).BeginInit();
            this.listContextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.removecount)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 269F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.argumentOverride, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.injectionindex, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.illist, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.removecount, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 5);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(833, 380);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(5, 343);
            this.label4.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(259, 37);
            this.label4.TabIndex = 12;
            this.label4.Text = "type var, type var:returntype";
            // 
            // argumentOverride
            // 
            this.argumentOverride.Dock = System.Windows.Forms.DockStyle.Fill;
            this.argumentOverride.Location = new System.Drawing.Point(274, 323);
            this.argumentOverride.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.argumentOverride.Name = "argumentOverride";
            this.argumentOverride.Size = new System.Drawing.Size(554, 22);
            this.argumentOverride.TabIndex = 11;
            this.argumentOverride.TextChanged += new System.EventHandler(this.argumentstring_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(5, 318);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(259, 25);
            this.label3.TabIndex = 5;
            this.label3.Text = "Argument override";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(261, 39);
            this.label1.TabIndex = 0;
            this.label1.Text = "Injection Index:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // injectionindex
            // 
            this.injectionindex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.injectionindex.Location = new System.Drawing.Point(273, 4);
            this.injectionindex.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.injectionindex.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.injectionindex.Name = "injectionindex";
            this.injectionindex.Size = new System.Drawing.Size(556, 22);
            this.injectionindex.TabIndex = 2;
            this.injectionindex.ValueChanged += new System.EventHandler(this.injectionindex_ValueChanged);
            // 
            // illist
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.illist, 2);
            this.illist.ContextMenuStrip = this.listContextMenuStrip;
            this.illist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.illist.FormattingEnabled = true;
            this.illist.ItemHeight = 16;
            this.illist.Location = new System.Drawing.Point(4, 82);
            this.illist.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.illist.Name = "illist";
            this.illist.Size = new System.Drawing.Size(825, 232);
            this.illist.TabIndex = 4;
            this.illist.SelectedIndexChanged += new System.EventHandler(this.illist_SelectedIndexChanged);
            // 
            // listContextMenuStrip
            // 
            this.listContextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.listContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewToolStripMenuItem,
            this.EditToolStripMenuItem,
            this.RemoveToolStripMenuItem});
            this.listContextMenuStrip.Name = "listContextMenuStrip";
            this.listContextMenuStrip.Size = new System.Drawing.Size(133, 76);
            // 
            // NewToolStripMenuItem
            // 
            this.NewToolStripMenuItem.Name = "NewToolStripMenuItem";
            this.NewToolStripMenuItem.Size = new System.Drawing.Size(132, 24);
            this.NewToolStripMenuItem.Text = "New";
            this.NewToolStripMenuItem.Click += new System.EventHandler(this.NewToolStripMenuItem_Click);
            // 
            // EditToolStripMenuItem
            // 
            this.EditToolStripMenuItem.Enabled = false;
            this.EditToolStripMenuItem.Name = "EditToolStripMenuItem";
            this.EditToolStripMenuItem.Size = new System.Drawing.Size(132, 24);
            this.EditToolStripMenuItem.Text = "Edit";
            this.EditToolStripMenuItem.Click += new System.EventHandler(this.EditToolStripMenuItem_Click);
            // 
            // RemoveToolStripMenuItem
            // 
            this.RemoveToolStripMenuItem.Enabled = false;
            this.RemoveToolStripMenuItem.Name = "RemoveToolStripMenuItem";
            this.RemoveToolStripMenuItem.Size = new System.Drawing.Size(132, 24);
            this.RemoveToolStripMenuItem.Text = "Remove";
            this.RemoveToolStripMenuItem.Click += new System.EventHandler(this.RemoveToolStripMenuItem_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(4, 39);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(261, 39);
            this.label2.TabIndex = 1;
            this.label2.Text = "Remove Count:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // removecount
            // 
            this.removecount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.removecount.Location = new System.Drawing.Point(273, 43);
            this.removecount.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.removecount.Name = "removecount";
            this.removecount.Size = new System.Drawing.Size(556, 22);
            this.removecount.TabIndex = 3;
            this.removecount.ValueChanged += new System.EventHandler(this.removecount_ValueChanged);
            // 
            // ModifyHookSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ModifyHookSettingsControl";
            this.Size = new System.Drawing.Size(859, 397);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.injectionindex)).EndInit();
            this.listContextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.removecount)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown injectionindex;
        private System.Windows.Forms.ListBox illist;
        private System.Windows.Forms.ContextMenuStrip listContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem NewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EditToolStripMenuItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown removecount;
        private System.Windows.Forms.ToolStripMenuItem RemoveToolStripMenuItem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox argumentOverride;
        private System.Windows.Forms.Label label4;
    }
}
