namespace Oxide.Patcher
{
    partial class PatchProcessForm
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
            this.statuslabel = new System.Windows.Forms.Label();
            this.progressbar = new System.Windows.Forms.ProgressBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.patchlog = new System.Windows.Forms.ListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.copybutton = new System.Windows.Forms.Button();
            this.closebutton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statuslabel
            // 
            this.statuslabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.statuslabel.Location = new System.Drawing.Point(5, 5);
            this.statuslabel.Name = "statuslabel";
            this.statuslabel.Size = new System.Drawing.Size(584, 23);
            this.statuslabel.TabIndex = 1;
            this.statuslabel.Text = "Idle";
            this.statuslabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressbar
            // 
            this.progressbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.progressbar.Location = new System.Drawing.Point(5, 28);
            this.progressbar.Maximum = 0;
            this.progressbar.Name = "progressbar";
            this.progressbar.Size = new System.Drawing.Size(584, 23);
            this.progressbar.Step = 1;
            this.progressbar.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.patchlog);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(5, 51);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.panel1.Size = new System.Drawing.Size(584, 225);
            this.panel1.TabIndex = 3;
            // 
            // patchlog
            // 
            this.patchlog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patchlog.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.patchlog.FormattingEnabled = true;
            this.patchlog.HorizontalScrollbar = true;
            this.patchlog.ItemHeight = 15;
            this.patchlog.Location = new System.Drawing.Point(0, 5);
            this.patchlog.Name = "patchlog";
            this.patchlog.Size = new System.Drawing.Size(584, 186);
            this.patchlog.TabIndex = 6;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.flowLayoutPanel1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 191);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(584, 34);
            this.panel2.TabIndex = 5;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.copybutton);
            this.flowLayoutPanel1.Controls.Add(this.closebutton);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(211, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(162, 29);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // copybutton
            // 
            this.copybutton.Enabled = false;
            this.copybutton.Location = new System.Drawing.Point(3, 3);
            this.copybutton.Name = "copybutton";
            this.copybutton.Size = new System.Drawing.Size(75, 23);
            this.copybutton.TabIndex = 0;
            this.copybutton.Text = "Copy Log";
            this.copybutton.UseVisualStyleBackColor = true;
            this.copybutton.Click += new System.EventHandler(this.copybutton_Click);
            // 
            // closebutton
            // 
            this.closebutton.Enabled = false;
            this.closebutton.Location = new System.Drawing.Point(84, 3);
            this.closebutton.Name = "closebutton";
            this.closebutton.Size = new System.Drawing.Size(75, 23);
            this.closebutton.TabIndex = 1;
            this.closebutton.Text = "Done";
            this.closebutton.UseVisualStyleBackColor = true;
            this.closebutton.Click += new System.EventHandler(this.closebutton_Click);
            // 
            // PatchProcessForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(594, 281);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.progressbar);
            this.Controls.Add(this.statuslabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PatchProcessForm";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Text = "Oxide Patch Process";
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label statuslabel;
        private System.Windows.Forms.ProgressBar progressbar;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox patchlog;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button copybutton;
        private System.Windows.Forms.Button closebutton;
    }
}