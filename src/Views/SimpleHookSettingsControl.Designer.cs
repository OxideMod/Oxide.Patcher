namespace Oxide.Patcher.Views
{
    partial class SimpleHookSettingsControl
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
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.injectionindex = new System.Windows.Forms.NumericUpDown();
            this.returnbehavior = new System.Windows.Forms.ComboBox();
            this.argumentbehavior = new System.Windows.Forms.ComboBox();
            this.argumentstring = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.injectionindex)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.injectionindex, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.returnbehavior, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.argumentbehavior, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.argumentstring, 1, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(344, 101);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 26);
            this.label2.TabIndex = 9;
            this.label2.Text = "Argument String:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(3, 52);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(114, 26);
            this.label6.TabIndex = 5;
            this.label6.Text = "Argument Behavior:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(114, 26);
            this.label3.TabIndex = 2;
            this.label3.Text = "Return Behavior:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Injection Index:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // injectionindex
            // 
            this.injectionindex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.injectionindex.Location = new System.Drawing.Point(123, 3);
            this.injectionindex.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.injectionindex.Name = "injectionindex";
            this.injectionindex.Size = new System.Drawing.Size(218, 20);
            this.injectionindex.TabIndex = 6;
            this.injectionindex.ValueChanged += new System.EventHandler(this.injectionindex_ValueChanged);
            // 
            // returnbehavior
            // 
            this.returnbehavior.Dock = System.Windows.Forms.DockStyle.Fill;
            this.returnbehavior.FormattingEnabled = true;
            this.returnbehavior.Location = new System.Drawing.Point(123, 29);
            this.returnbehavior.Name = "returnbehavior";
            this.returnbehavior.Size = new System.Drawing.Size(218, 21);
            this.returnbehavior.TabIndex = 7;
            this.returnbehavior.SelectedIndexChanged += new System.EventHandler(this.returnbehavior_SelectedIndexChanged);
            // 
            // argumentbehavior
            // 
            this.argumentbehavior.Dock = System.Windows.Forms.DockStyle.Fill;
            this.argumentbehavior.FormattingEnabled = true;
            this.argumentbehavior.Location = new System.Drawing.Point(123, 55);
            this.argumentbehavior.Name = "argumentbehavior";
            this.argumentbehavior.Size = new System.Drawing.Size(218, 21);
            this.argumentbehavior.TabIndex = 8;
            this.argumentbehavior.SelectedIndexChanged += new System.EventHandler(this.argumentbehavior_SelectedIndexChanged);
            // 
            // argumentstring
            // 
            this.argumentstring.Dock = System.Windows.Forms.DockStyle.Fill;
            this.argumentstring.Location = new System.Drawing.Point(123, 81);
            this.argumentstring.Name = "argumentstring";
            this.argumentstring.Size = new System.Drawing.Size(218, 20);
            this.argumentstring.TabIndex = 10;
            this.argumentstring.TextChanged += new System.EventHandler(this.argumentstring_TextChanged);
            // 
            // SimpleHookSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SimpleHookSettingsControl";
            this.Size = new System.Drawing.Size(415, 198);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.injectionindex)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown injectionindex;
        private System.Windows.Forms.ComboBox returnbehavior;
        private System.Windows.Forms.ComboBox argumentbehavior;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox argumentstring;
    }
}
