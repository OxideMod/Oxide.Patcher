namespace Oxide.Patcher
{
    partial class ModifyForm
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
            this.settingsgroup = new System.Windows.Forms.GroupBox();
            this.tablepanel = new System.Windows.Forms.TableLayoutPanel();
            this.operandlabel = new System.Windows.Forms.Label();
            this.optypelabel = new System.Windows.Forms.Label();
            this.optypes = new System.Windows.Forms.ComboBox();
            this.opcodelabel = new System.Windows.Forms.Label();
            this.opcodes = new System.Windows.Forms.ComboBox();
            this.InsertBeforeButton = new System.Windows.Forms.Button();
            this.InsertAfterButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.settingsgroup.SuspendLayout();
            this.tablepanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // settingsgroup
            // 
            this.settingsgroup.Controls.Add(this.tablepanel);
            this.settingsgroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsgroup.Location = new System.Drawing.Point(0, 0);
            this.settingsgroup.Name = "settingsgroup";
            this.settingsgroup.Size = new System.Drawing.Size(484, 98);
            this.settingsgroup.TabIndex = 1;
            this.settingsgroup.TabStop = false;
            this.settingsgroup.Text = "Instruction Settings";
            // 
            // tablepanel
            // 
            this.tablepanel.ColumnCount = 3;
            this.tablepanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
            this.tablepanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tablepanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 122F));
            this.tablepanel.Controls.Add(this.operandlabel, 0, 2);
            this.tablepanel.Controls.Add(this.optypelabel, 0, 1);
            this.tablepanel.Controls.Add(this.optypes, 1, 1);
            this.tablepanel.Controls.Add(this.opcodelabel, 0, 0);
            this.tablepanel.Controls.Add(this.opcodes, 1, 0);
            this.tablepanel.Controls.Add(this.InsertBeforeButton, 2, 0);
            this.tablepanel.Controls.Add(this.InsertAfterButton, 2, 1);
            this.tablepanel.Controls.Add(this.SaveButton, 2, 2);
            this.tablepanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablepanel.Location = new System.Drawing.Point(3, 16);
            this.tablepanel.Name = "tablepanel";
            this.tablepanel.RowCount = 4;
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tablepanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tablepanel.Size = new System.Drawing.Size(478, 79);
            this.tablepanel.TabIndex = 0;
            // 
            // operandlabel
            // 
            this.operandlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.operandlabel.Location = new System.Drawing.Point(3, 52);
            this.operandlabel.Name = "operandlabel";
            this.operandlabel.Size = new System.Drawing.Size(80, 26);
            this.operandlabel.TabIndex = 4;
            this.operandlabel.Text = "Operand:";
            this.operandlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.operandlabel.Visible = false;
            // 
            // optypelabel
            // 
            this.optypelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optypelabel.Location = new System.Drawing.Point(3, 26);
            this.optypelabel.Name = "optypelabel";
            this.optypelabel.Size = new System.Drawing.Size(80, 26);
            this.optypelabel.TabIndex = 3;
            this.optypelabel.Text = "OpType:";
            this.optypelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // optypes
            // 
            this.optypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.optypes.FormattingEnabled = true;
            this.optypes.Location = new System.Drawing.Point(89, 29);
            this.optypes.Name = "optypes";
            this.optypes.Size = new System.Drawing.Size(264, 21);
            this.optypes.TabIndex = 2;
            this.optypes.SelectedIndexChanged += new System.EventHandler(this.optypes_SelectedIndexChanged);
            // 
            // opcodelabel
            // 
            this.opcodelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.opcodelabel.Location = new System.Drawing.Point(3, 0);
            this.opcodelabel.Name = "opcodelabel";
            this.opcodelabel.Size = new System.Drawing.Size(80, 26);
            this.opcodelabel.TabIndex = 0;
            this.opcodelabel.Text = "OpCode:";
            this.opcodelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // opcodes
            // 
            this.opcodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.opcodes.FormattingEnabled = true;
            this.opcodes.Location = new System.Drawing.Point(89, 3);
            this.opcodes.Name = "opcodes";
            this.opcodes.Size = new System.Drawing.Size(264, 21);
            this.opcodes.TabIndex = 1;
            this.opcodes.Leave += new System.EventHandler(this.opcodes_Leave);
            // 
            // InsertBeforeButton
            // 
            this.InsertBeforeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InsertBeforeButton.Location = new System.Drawing.Point(359, 3);
            this.InsertBeforeButton.Name = "InsertBeforeButton";
            this.InsertBeforeButton.Size = new System.Drawing.Size(116, 20);
            this.InsertBeforeButton.TabIndex = 5;
            this.InsertBeforeButton.Text = "Insert before";
            this.InsertBeforeButton.UseVisualStyleBackColor = true;
            this.InsertBeforeButton.Click += new System.EventHandler(this.InsertBeforeButton_Click);
            // 
            // InsertAfterButton
            // 
            this.InsertAfterButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InsertAfterButton.Location = new System.Drawing.Point(359, 29);
            this.InsertAfterButton.Name = "InsertAfterButton";
            this.InsertAfterButton.Size = new System.Drawing.Size(116, 20);
            this.InsertAfterButton.TabIndex = 6;
            this.InsertAfterButton.Text = "Insert after";
            this.InsertAfterButton.UseVisualStyleBackColor = true;
            this.InsertAfterButton.Click += new System.EventHandler(this.InsertAfterButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SaveButton.Location = new System.Drawing.Point(359, 55);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(116, 20);
            this.SaveButton.TabIndex = 7;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // ModifyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 98);
            this.Controls.Add(this.settingsgroup);
            this.Name = "ModifyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ModifyForm";
            this.settingsgroup.ResumeLayout(false);
            this.tablepanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox settingsgroup;
        private System.Windows.Forms.TableLayoutPanel tablepanel;
        private System.Windows.Forms.Label optypelabel;
        private System.Windows.Forms.ComboBox optypes;
        private System.Windows.Forms.Label opcodelabel;
        private System.Windows.Forms.ComboBox opcodes;
        private System.Windows.Forms.Label operandlabel;
        private System.Windows.Forms.Button InsertBeforeButton;
        private System.Windows.Forms.Button InsertAfterButton;
        private System.Windows.Forms.Button SaveButton;
    }
}