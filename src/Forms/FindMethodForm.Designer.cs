namespace Oxide.Patcher
{
    partial class FindMethodForm
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
            this.searchpanel = new System.Windows.Forms.Panel();
            this.searchbox = new System.Windows.Forms.TextBox();
            this.searchlabel = new System.Windows.Forms.Label();
            this.methodtree = new System.Windows.Forms.TreeView();
            this.statuslabel = new System.Windows.Forms.Label();
            this.buttonpanel = new System.Windows.Forms.FlowLayoutPanel();
            this.cancelbutton = new System.Windows.Forms.Button();
            this.okbutton = new System.Windows.Forms.Button();
            this.searchpanel.SuspendLayout();
            this.buttonpanel.SuspendLayout();
            this.SuspendLayout();
            //
            // searchpanel
            //
            this.searchpanel.Controls.Add(this.searchbox);
            this.searchpanel.Controls.Add(this.searchlabel);
            this.searchpanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchpanel.Location = new System.Drawing.Point(0, 0);
            this.searchpanel.Name = "searchpanel";
            this.searchpanel.Padding = new System.Windows.Forms.Padding(8, 8, 8, 4);
            this.searchpanel.Size = new System.Drawing.Size(620, 40);
            this.searchpanel.TabIndex = 0;
            //
            // searchlabel
            //
            this.searchlabel.AutoSize = true;
            this.searchlabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.searchlabel.Location = new System.Drawing.Point(8, 8);
            this.searchlabel.Name = "searchlabel";
            this.searchlabel.Size = new System.Drawing.Size(46, 28);
            this.searchlabel.TabIndex = 0;
            this.searchlabel.Text = "Search:";
            this.searchlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // searchbox
            //
            this.searchbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchbox.Location = new System.Drawing.Point(54, 8);
            this.searchbox.Name = "searchbox";
            this.searchbox.Size = new System.Drawing.Size(558, 20);
            this.searchbox.TabIndex = 1;
            this.searchbox.TextChanged += new System.EventHandler(this.searchbox_TextChanged);
            //
            // methodtree
            //
            this.methodtree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.methodtree.HideSelection = false;
            this.methodtree.Location = new System.Drawing.Point(0, 40);
            this.methodtree.Name = "methodtree";
            this.methodtree.Size = new System.Drawing.Size(620, 434);
            this.methodtree.TabIndex = 1;
            this.methodtree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.methodtree_BeforeExpand);
            this.methodtree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.methodtree_AfterSelect);
            this.methodtree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.methodtree_NodeMouseDoubleClick);
            //
            // statuslabel
            //
            this.statuslabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statuslabel.Location = new System.Drawing.Point(0, 474);
            this.statuslabel.Name = "statuslabel";
            this.statuslabel.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.statuslabel.Size = new System.Drawing.Size(620, 22);
            this.statuslabel.TabIndex = 2;
            this.statuslabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // buttonpanel
            //
            this.buttonpanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonpanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttonpanel.Location = new System.Drawing.Point(0, 496);
            this.buttonpanel.Name = "buttonpanel";
            this.buttonpanel.Padding = new System.Windows.Forms.Padding(8);
            this.buttonpanel.Size = new System.Drawing.Size(620, 42);
            this.buttonpanel.TabIndex = 3;
            //
            // cancelbutton
            //
            this.cancelbutton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelbutton.Location = new System.Drawing.Point(533, 8);
            this.cancelbutton.Name = "cancelbutton";
            this.cancelbutton.Size = new System.Drawing.Size(79, 26);
            this.cancelbutton.TabIndex = 0;
            this.cancelbutton.Text = "Cancel";
            this.cancelbutton.UseVisualStyleBackColor = true;
            this.buttonpanel.Controls.Add(this.cancelbutton);
            //
            // okbutton
            //
            this.okbutton.Enabled = false;
            this.okbutton.Location = new System.Drawing.Point(448, 8);
            this.okbutton.Name = "okbutton";
            this.okbutton.Size = new System.Drawing.Size(79, 26);
            this.okbutton.TabIndex = 1;
            this.okbutton.Text = "Select";
            this.okbutton.UseVisualStyleBackColor = true;
            this.okbutton.Click += new System.EventHandler(this.okbutton_Click);
            this.buttonpanel.Controls.Add(this.okbutton);
            //
            // FindMethodForm
            //
            this.AcceptButton = this.okbutton;
            this.CancelButton = this.cancelbutton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 538);
            this.Controls.Add(this.methodtree);
            this.Controls.Add(this.statuslabel);
            this.Controls.Add(this.buttonpanel);
            this.Controls.Add(this.searchpanel);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ShowIcon = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(420, 320);
            this.Name = "FindMethodForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find Method";
            this.searchpanel.ResumeLayout(false);
            this.searchpanel.PerformLayout();
            this.buttonpanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel searchpanel;
        private System.Windows.Forms.TextBox searchbox;
        private System.Windows.Forms.Label searchlabel;
        private System.Windows.Forms.TreeView methodtree;
        private System.Windows.Forms.Label statuslabel;
        private System.Windows.Forms.FlowLayoutPanel buttonpanel;
        private System.Windows.Forms.Button okbutton;
        private System.Windows.Forms.Button cancelbutton;
    }
}
