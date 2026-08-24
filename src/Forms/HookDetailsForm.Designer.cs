namespace Oxide.Patcher
{
    partial class HookDetailsForm
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
            this.formtabs = new System.Windows.Forms.TabControl();
            this.generaltab = new System.Windows.Forms.TabPage();
            this.detailstable = new System.Windows.Forms.TableLayoutPanel();
            this.hooktypelabel = new System.Windows.Forms.Label();
            this.hooktypedropdown = new System.Windows.Forms.ComboBox();
            this.categorylabel = new System.Windows.Forms.Label();
            this.categorytextbox = new System.Windows.Forms.TextBox();
            this.namelabel = new System.Windows.Forms.Label();
            this.nametextbox = new System.Windows.Forms.TextBox();
            this.hooknamelabel = new System.Windows.Forms.Label();
            this.hooknametextbox = new System.Windows.Forms.TextBox();
            this.hookdescriptionlabel = new System.Windows.Forms.Label();
            this.hookdescriptiontextbox = new System.Windows.Forms.TextBox();
            this.assemblylabel = new System.Windows.Forms.Label();
            this.assemblydropdown = new System.Windows.Forms.ComboBox();
            this.typenamelabel = new System.Windows.Forms.Label();
            this.typenametextbox = new System.Windows.Forms.TextBox();
            this.exposurelabel = new System.Windows.Forms.Label();
            this.exposuredropdown = new System.Windows.Forms.ComboBox();
            this.returntypelabel = new System.Windows.Forms.Label();
            this.returntypetextbox = new System.Windows.Forms.TextBox();
            this.methodnamelabel = new System.Windows.Forms.Label();
            this.methodnametextbox = new System.Windows.Forms.TextBox();
            this.parameterslabel = new System.Windows.Forms.Label();
            this.parameterstextbox = new System.Windows.Forms.TextBox();
            this.msilhashlabel = new System.Windows.Forms.Label();
            this.msilhashtextbox = new System.Windows.Forms.TextBox();
            this.typesettingstab = new System.Windows.Forms.TabPage();
            this.typesettingspanel = new System.Windows.Forms.Panel();
            this.simplesettingspanel = new System.Windows.Forms.Panel();
            this.simpletable = new System.Windows.Forms.TableLayoutPanel();
            this.simpleinjectionindexlabel = new System.Windows.Forms.Label();
            this.simpleinjectionindex = new System.Windows.Forms.NumericUpDown();
            this.simplereturnbehaviorlabel = new System.Windows.Forms.Label();
            this.simplereturnbehavior = new System.Windows.Forms.ComboBox();
            this.simpleargumentbehaviorlabel = new System.Windows.Forms.Label();
            this.simpleargumentbehavior = new System.Windows.Forms.ComboBox();
            this.simpleargumentstringlabel = new System.Windows.Forms.Label();
            this.simpleargumentstring = new System.Windows.Forms.TextBox();
            this.simpledeprecatedlabel = new System.Windows.Forms.Label();
            this.simpledeprecatedcheckbox = new System.Windows.Forms.CheckBox();
            this.simplereplacementhooklabel = new System.Windows.Forms.Label();
            this.simplereplacementhooktextbox = new System.Windows.Forms.TextBox();
            this.simpleremovaldatelabel = new System.Windows.Forms.Label();
            this.simpleremovaldatepicker = new System.Windows.Forms.DateTimePicker();
            this.initoxidesettingspanel = new System.Windows.Forms.Panel();
            this.initoxidetable = new System.Windows.Forms.TableLayoutPanel();
            this.initoxideinjectionindexlabel = new System.Windows.Forms.Label();
            this.initoxideinjectionindex = new System.Windows.Forms.NumericUpDown();
            this.modifysettingspanel = new System.Windows.Forms.Panel();
            this.modifytable = new System.Windows.Forms.TableLayoutPanel();
            this.modifyinjectionindexlabel = new System.Windows.Forms.Label();
            this.modifyinjectionindex = new System.Windows.Forms.NumericUpDown();
            this.modifyremovecountlabel = new System.Windows.Forms.Label();
            this.modifyremovecount = new System.Windows.Forms.NumericUpDown();
            this.modifynotelabel = new System.Windows.Forms.Label();
            this.buttonpanel = new System.Windows.Forms.FlowLayoutPanel();
            this.okbutton = new System.Windows.Forms.Button();
            this.cancelbutton = new System.Windows.Forms.Button();
            this.findmethodbutton = new System.Windows.Forms.Button();
            this.formtabs.SuspendLayout();
            this.generaltab.SuspendLayout();
            this.detailstable.SuspendLayout();
            this.typesettingstab.SuspendLayout();
            this.typesettingspanel.SuspendLayout();
            this.simplesettingspanel.SuspendLayout();
            this.simpletable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.simpleinjectionindex)).BeginInit();
            this.initoxidesettingspanel.SuspendLayout();
            this.initoxidetable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.initoxideinjectionindex)).BeginInit();
            this.modifysettingspanel.SuspendLayout();
            this.modifytable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modifyinjectionindex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.modifyremovecount)).BeginInit();
            this.buttonpanel.SuspendLayout();
            this.SuspendLayout();
            //
            // formtabs
            //
            this.formtabs.Controls.Add(this.generaltab);
            this.formtabs.Controls.Add(this.typesettingstab);
            this.formtabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formtabs.Location = new System.Drawing.Point(0, 0);
            this.formtabs.Name = "formtabs";
            this.formtabs.SelectedIndex = 0;
            this.formtabs.Size = new System.Drawing.Size(524, 429);
            this.formtabs.TabIndex = 0;
            //
            // generaltab
            //
            this.generaltab.Controls.Add(this.detailstable);
            this.generaltab.Location = new System.Drawing.Point(4, 22);
            this.generaltab.Name = "generaltab";
            this.generaltab.Padding = new System.Windows.Forms.Padding(3);
            this.generaltab.Size = new System.Drawing.Size(516, 403);
            this.generaltab.TabIndex = 0;
            this.generaltab.Text = "General";
            this.generaltab.UseVisualStyleBackColor = true;
            //
            // detailstable
            //
            this.detailstable.ColumnCount = 2;
            this.detailstable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.detailstable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.detailstable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailstable.Location = new System.Drawing.Point(3, 3);
            this.detailstable.Name = "detailstable";
            this.detailstable.Padding = new System.Windows.Forms.Padding(8);
            this.detailstable.RowCount = 13;
            for (int i = 0; i < 12; i++)
            {
                this.detailstable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            }
            this.detailstable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.detailstable.Size = new System.Drawing.Size(510, 397);
            this.detailstable.TabIndex = 0;
            //
            // hooktypelabel
            //
            this.hooktypelabel.AutoSize = true;
            this.hooktypelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hooktypelabel.Location = new System.Drawing.Point(11, 8);
            this.hooktypelabel.Name = "hooktypelabel";
            this.hooktypelabel.Size = new System.Drawing.Size(122, 23);
            this.hooktypelabel.TabIndex = 0;
            this.hooktypelabel.Text = "Hook Type:";
            this.hooktypelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailstable.Controls.Add(this.hooktypelabel, 0, 0);
            //
            // hooktypedropdown
            //
            this.hooktypedropdown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hooktypedropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.hooktypedropdown.Location = new System.Drawing.Point(141, 11);
            this.hooktypedropdown.Name = "hooktypedropdown";
            this.hooktypedropdown.Size = new System.Drawing.Size(358, 21);
            this.hooktypedropdown.TabIndex = 1;
            this.hooktypedropdown.SelectedIndexChanged += new System.EventHandler(this.hooktypedropdown_SelectedIndexChanged);
            this.detailstable.Controls.Add(this.hooktypedropdown, 1, 0);
            //
            // categorylabel
            //
            this.categorylabel.AutoSize = true;
            this.categorylabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.categorylabel.Name = "categorylabel";
            this.categorylabel.Size = new System.Drawing.Size(122, 23);
            this.categorylabel.TabIndex = 2;
            this.categorylabel.Text = "Category:";
            this.categorylabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailstable.Controls.Add(this.categorylabel, 0, 1);
            //
            // categorytextbox
            //
            this.categorytextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.categorytextbox.Name = "categorytextbox";
            this.categorytextbox.Size = new System.Drawing.Size(358, 20);
            this.categorytextbox.TabIndex = 3;
            this.detailstable.Controls.Add(this.categorytextbox, 1, 1);
            //
            // namelabel
            //
            this.namelabel.AutoSize = true;
            this.namelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.namelabel.Name = "namelabel";
            this.namelabel.Size = new System.Drawing.Size(122, 23);
            this.namelabel.TabIndex = 4;
            this.namelabel.Text = "Name:";
            this.namelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailstable.Controls.Add(this.namelabel, 0, 2);
            //
            // nametextbox
            //
            this.nametextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nametextbox.Name = "nametextbox";
            this.nametextbox.Size = new System.Drawing.Size(358, 20);
            this.nametextbox.TabIndex = 5;
            this.detailstable.Controls.Add(this.nametextbox, 1, 2);
            //
            // hooknamelabel
            //
            this.hooknamelabel.AutoSize = true;
            this.hooknamelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hooknamelabel.Name = "hooknamelabel";
            this.hooknamelabel.Size = new System.Drawing.Size(122, 23);
            this.hooknamelabel.TabIndex = 6;
            this.hooknamelabel.Text = "Oxide Hook Name:";
            this.hooknamelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailstable.Controls.Add(this.hooknamelabel, 0, 3);
            //
            // hooknametextbox
            //
            this.hooknametextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hooknametextbox.Name = "hooknametextbox";
            this.hooknametextbox.Size = new System.Drawing.Size(358, 20);
            this.hooknametextbox.TabIndex = 7;
            this.detailstable.Controls.Add(this.hooknametextbox, 1, 3);
            //
            // hookdescriptionlabel
            //
            this.hookdescriptionlabel.AutoSize = true;
            this.hookdescriptionlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hookdescriptionlabel.Name = "hookdescriptionlabel";
            this.hookdescriptionlabel.Size = new System.Drawing.Size(122, 44);
            this.hookdescriptionlabel.TabIndex = 8;
            this.hookdescriptionlabel.Text = "Hook Description:";
            this.hookdescriptionlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailstable.Controls.Add(this.hookdescriptionlabel, 0, 4);
            //
            // hookdescriptiontextbox
            //
            this.hookdescriptiontextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hookdescriptiontextbox.MaxLength = 255;
            this.hookdescriptiontextbox.Multiline = true;
            this.hookdescriptiontextbox.Name = "hookdescriptiontextbox";
            this.hookdescriptiontextbox.Size = new System.Drawing.Size(358, 44);
            this.hookdescriptiontextbox.TabIndex = 9;
            this.detailstable.Controls.Add(this.hookdescriptiontextbox, 1, 4);
            //
            // assemblylabel
            //
            this.assemblylabel.AutoSize = true;
            this.assemblylabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.assemblylabel.Name = "assemblylabel";
            this.assemblylabel.Size = new System.Drawing.Size(122, 23);
            this.assemblylabel.TabIndex = 10;
            this.assemblylabel.Text = "Assembly:";
            this.assemblylabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailstable.Controls.Add(this.assemblylabel, 0, 5);
            //
            // assemblydropdown
            //
            this.assemblydropdown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.assemblydropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.assemblydropdown.Name = "assemblydropdown";
            this.assemblydropdown.Size = new System.Drawing.Size(358, 21);
            this.assemblydropdown.TabIndex = 11;
            this.detailstable.Controls.Add(this.assemblydropdown, 1, 5);
            //
            // typenamelabel
            //
            this.typenamelabel.AutoSize = true;
            this.typenamelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.typenamelabel.Name = "typenamelabel";
            this.typenamelabel.Size = new System.Drawing.Size(122, 23);
            this.typenamelabel.TabIndex = 12;
            this.typenamelabel.Text = "Full Type Name:";
            this.typenamelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailstable.Controls.Add(this.typenamelabel, 0, 6);
            //
            // typenametextbox
            //
            this.typenametextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.typenametextbox.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.typenametextbox.Name = "typenametextbox";
            this.typenametextbox.Size = new System.Drawing.Size(358, 20);
            this.typenametextbox.TabIndex = 13;
            this.detailstable.Controls.Add(this.typenametextbox, 1, 6);
            //
            // exposurelabel
            //
            this.exposurelabel.AutoSize = true;
            this.exposurelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exposurelabel.Name = "exposurelabel";
            this.exposurelabel.Size = new System.Drawing.Size(122, 23);
            this.exposurelabel.TabIndex = 14;
            this.exposurelabel.Text = "Method Exposure:";
            this.exposurelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailstable.Controls.Add(this.exposurelabel, 0, 7);
            //
            // exposuredropdown
            //
            this.exposuredropdown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exposuredropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.exposuredropdown.Name = "exposuredropdown";
            this.exposuredropdown.Size = new System.Drawing.Size(358, 21);
            this.exposuredropdown.TabIndex = 15;
            this.detailstable.Controls.Add(this.exposuredropdown, 1, 7);
            //
            // returntypelabel
            //
            this.returntypelabel.AutoSize = true;
            this.returntypelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.returntypelabel.Name = "returntypelabel";
            this.returntypelabel.Size = new System.Drawing.Size(122, 23);
            this.returntypelabel.TabIndex = 16;
            this.returntypelabel.Text = "Return Type:";
            this.returntypelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailstable.Controls.Add(this.returntypelabel, 0, 8);
            //
            // returntypetextbox
            //
            this.returntypetextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.returntypetextbox.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.returntypetextbox.Name = "returntypetextbox";
            this.returntypetextbox.Size = new System.Drawing.Size(358, 20);
            this.returntypetextbox.TabIndex = 17;
            this.detailstable.Controls.Add(this.returntypetextbox, 1, 8);
            //
            // methodnamelabel
            //
            this.methodnamelabel.AutoSize = true;
            this.methodnamelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.methodnamelabel.Name = "methodnamelabel";
            this.methodnamelabel.Size = new System.Drawing.Size(122, 23);
            this.methodnamelabel.TabIndex = 18;
            this.methodnamelabel.Text = "Method Name:";
            this.methodnamelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailstable.Controls.Add(this.methodnamelabel, 0, 9);
            //
            // methodnametextbox
            //
            this.methodnametextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.methodnametextbox.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.methodnametextbox.Name = "methodnametextbox";
            this.methodnametextbox.Size = new System.Drawing.Size(358, 20);
            this.methodnametextbox.TabIndex = 19;
            this.detailstable.Controls.Add(this.methodnametextbox, 1, 9);
            //
            // parameterslabel
            //
            this.parameterslabel.AutoSize = true;
            this.parameterslabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parameterslabel.Name = "parameterslabel";
            this.parameterslabel.Size = new System.Drawing.Size(122, 23);
            this.parameterslabel.TabIndex = 20;
            this.parameterslabel.Text = "Parameters (CSV):";
            this.parameterslabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailstable.Controls.Add(this.parameterslabel, 0, 10);
            //
            // parameterstextbox
            //
            this.parameterstextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parameterstextbox.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.parameterstextbox.Name = "parameterstextbox";
            this.parameterstextbox.Size = new System.Drawing.Size(358, 20);
            this.parameterstextbox.TabIndex = 21;
            this.detailstable.Controls.Add(this.parameterstextbox, 1, 10);
            //
            // msilhashlabel
            //
            this.msilhashlabel.AutoSize = true;
            this.msilhashlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.msilhashlabel.Name = "msilhashlabel";
            this.msilhashlabel.Size = new System.Drawing.Size(122, 23);
            this.msilhashlabel.TabIndex = 22;
            this.msilhashlabel.Text = "MSIL Hash:";
            this.msilhashlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailstable.Controls.Add(this.msilhashlabel, 0, 11);
            //
            // msilhashtextbox
            //
            this.msilhashtextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.msilhashtextbox.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.msilhashtextbox.Name = "msilhashtextbox";
            this.msilhashtextbox.Size = new System.Drawing.Size(358, 20);
            this.msilhashtextbox.TabIndex = 23;
            this.detailstable.Controls.Add(this.msilhashtextbox, 1, 11);
            //
            // typesettingstab
            //
            this.typesettingstab.Controls.Add(this.typesettingspanel);
            this.typesettingstab.Location = new System.Drawing.Point(4, 22);
            this.typesettingstab.Name = "typesettingstab";
            this.typesettingstab.Padding = new System.Windows.Forms.Padding(3);
            this.typesettingstab.Size = new System.Drawing.Size(516, 403);
            this.typesettingstab.TabIndex = 1;
            this.typesettingstab.Text = "Type Settings";
            this.typesettingstab.UseVisualStyleBackColor = true;
            //
            // typesettingspanel
            //
            this.typesettingspanel.Controls.Add(this.simplesettingspanel);
            this.typesettingspanel.Controls.Add(this.initoxidesettingspanel);
            this.typesettingspanel.Controls.Add(this.modifysettingspanel);
            this.typesettingspanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.typesettingspanel.Location = new System.Drawing.Point(3, 3);
            this.typesettingspanel.Name = "typesettingspanel";
            this.typesettingspanel.Size = new System.Drawing.Size(510, 397);
            this.typesettingspanel.TabIndex = 0;
            //
            // simplesettingspanel
            //
            this.simplesettingspanel.Controls.Add(this.simpletable);
            this.simplesettingspanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simplesettingspanel.Location = new System.Drawing.Point(0, 0);
            this.simplesettingspanel.Name = "simplesettingspanel";
            this.simplesettingspanel.Size = new System.Drawing.Size(510, 397);
            this.simplesettingspanel.TabIndex = 0;
            //
            // simpletable
            //
            this.simpletable.ColumnCount = 2;
            this.simpletable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.simpletable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.simpletable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpletable.Location = new System.Drawing.Point(0, 0);
            this.simpletable.Name = "simpletable";
            this.simpletable.Padding = new System.Windows.Forms.Padding(8);
            this.simpletable.RowCount = 8;
            for (int i = 0; i < 7; i++)
            {
                this.simpletable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            }
            this.simpletable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.simpletable.Size = new System.Drawing.Size(510, 397);
            this.simpletable.TabIndex = 0;
            //
            // simpleinjectionindexlabel
            //
            this.simpleinjectionindexlabel.AutoSize = true;
            this.simpleinjectionindexlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleinjectionindexlabel.Location = new System.Drawing.Point(11, 8);
            this.simpleinjectionindexlabel.Name = "simpleinjectionindexlabel";
            this.simpleinjectionindexlabel.Size = new System.Drawing.Size(122, 23);
            this.simpleinjectionindexlabel.TabIndex = 0;
            this.simpleinjectionindexlabel.Text = "Injection Index:";
            this.simpleinjectionindexlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.simpletable.Controls.Add(this.simpleinjectionindexlabel, 0, 0);
            //
            // simpleinjectionindex
            //
            this.simpleinjectionindex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleinjectionindex.Location = new System.Drawing.Point(141, 11);
            this.simpleinjectionindex.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            this.simpleinjectionindex.Name = "simpleinjectionindex";
            this.simpleinjectionindex.Size = new System.Drawing.Size(358, 20);
            this.simpleinjectionindex.TabIndex = 1;
            this.simpletable.Controls.Add(this.simpleinjectionindex, 1, 0);
            //
            // simplereturnbehaviorlabel
            //
            this.simplereturnbehaviorlabel.AutoSize = true;
            this.simplereturnbehaviorlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simplereturnbehaviorlabel.Name = "simplereturnbehaviorlabel";
            this.simplereturnbehaviorlabel.Size = new System.Drawing.Size(122, 23);
            this.simplereturnbehaviorlabel.TabIndex = 2;
            this.simplereturnbehaviorlabel.Text = "Return Behavior:";
            this.simplereturnbehaviorlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.simpletable.Controls.Add(this.simplereturnbehaviorlabel, 0, 1);
            //
            // simplereturnbehavior
            //
            this.simplereturnbehavior.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simplereturnbehavior.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.simplereturnbehavior.Name = "simplereturnbehavior";
            this.simplereturnbehavior.Size = new System.Drawing.Size(358, 21);
            this.simplereturnbehavior.TabIndex = 3;
            this.simpletable.Controls.Add(this.simplereturnbehavior, 1, 1);
            //
            // simpleargumentbehaviorlabel
            //
            this.simpleargumentbehaviorlabel.AutoSize = true;
            this.simpleargumentbehaviorlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleargumentbehaviorlabel.Name = "simpleargumentbehaviorlabel";
            this.simpleargumentbehaviorlabel.Size = new System.Drawing.Size(122, 23);
            this.simpleargumentbehaviorlabel.TabIndex = 4;
            this.simpleargumentbehaviorlabel.Text = "Argument Behavior:";
            this.simpleargumentbehaviorlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.simpletable.Controls.Add(this.simpleargumentbehaviorlabel, 0, 2);
            //
            // simpleargumentbehavior
            //
            this.simpleargumentbehavior.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleargumentbehavior.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.simpleargumentbehavior.Name = "simpleargumentbehavior";
            this.simpleargumentbehavior.Size = new System.Drawing.Size(358, 21);
            this.simpleargumentbehavior.TabIndex = 5;
            this.simpletable.Controls.Add(this.simpleargumentbehavior, 1, 2);
            //
            // simpleargumentstringlabel
            //
            this.simpleargumentstringlabel.AutoSize = true;
            this.simpleargumentstringlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleargumentstringlabel.Name = "simpleargumentstringlabel";
            this.simpleargumentstringlabel.Size = new System.Drawing.Size(122, 23);
            this.simpleargumentstringlabel.TabIndex = 6;
            this.simpleargumentstringlabel.Text = "Argument String:";
            this.simpleargumentstringlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.simpletable.Controls.Add(this.simpleargumentstringlabel, 0, 3);
            //
            // simpleargumentstring
            //
            this.simpleargumentstring.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleargumentstring.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.simpleargumentstring.Name = "simpleargumentstring";
            this.simpleargumentstring.Size = new System.Drawing.Size(358, 20);
            this.simpleargumentstring.TabIndex = 7;
            this.simpletable.Controls.Add(this.simpleargumentstring, 1, 3);
            //
            // simpledeprecatedlabel
            //
            this.simpledeprecatedlabel.AutoSize = true;
            this.simpledeprecatedlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpledeprecatedlabel.Name = "simpledeprecatedlabel";
            this.simpledeprecatedlabel.Size = new System.Drawing.Size(122, 23);
            this.simpledeprecatedlabel.TabIndex = 8;
            this.simpledeprecatedlabel.Text = "Deprecated:";
            this.simpledeprecatedlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.simpletable.Controls.Add(this.simpledeprecatedlabel, 0, 4);
            //
            // simpledeprecatedcheckbox
            //
            this.simpledeprecatedcheckbox.AutoSize = true;
            this.simpledeprecatedcheckbox.Location = new System.Drawing.Point(141, 11);
            this.simpledeprecatedcheckbox.Name = "simpledeprecatedcheckbox";
            this.simpledeprecatedcheckbox.Size = new System.Drawing.Size(15, 14);
            this.simpledeprecatedcheckbox.TabIndex = 9;
            this.simpledeprecatedcheckbox.UseVisualStyleBackColor = true;
            this.simpledeprecatedcheckbox.CheckedChanged += new System.EventHandler(this.simpledeprecatedcheckbox_CheckedChanged);
            this.simpletable.Controls.Add(this.simpledeprecatedcheckbox, 1, 4);
            //
            // simplereplacementhooklabel
            //
            this.simplereplacementhooklabel.AutoSize = true;
            this.simplereplacementhooklabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simplereplacementhooklabel.Name = "simplereplacementhooklabel";
            this.simplereplacementhooklabel.Size = new System.Drawing.Size(122, 23);
            this.simplereplacementhooklabel.TabIndex = 10;
            this.simplereplacementhooklabel.Text = "Replacement Hook:";
            this.simplereplacementhooklabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.simpletable.Controls.Add(this.simplereplacementhooklabel, 0, 5);
            //
            // simplereplacementhooktextbox
            //
            this.simplereplacementhooktextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simplereplacementhooktextbox.Name = "simplereplacementhooktextbox";
            this.simplereplacementhooktextbox.Size = new System.Drawing.Size(358, 20);
            this.simplereplacementhooktextbox.TabIndex = 11;
            this.simpletable.Controls.Add(this.simplereplacementhooktextbox, 1, 5);
            //
            // simpleremovaldatelabel
            //
            this.simpleremovaldatelabel.AutoSize = true;
            this.simpleremovaldatelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleremovaldatelabel.Name = "simpleremovaldatelabel";
            this.simpleremovaldatelabel.Size = new System.Drawing.Size(122, 23);
            this.simpleremovaldatelabel.TabIndex = 12;
            this.simpleremovaldatelabel.Text = "Removal Date:";
            this.simpleremovaldatelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.simpletable.Controls.Add(this.simpleremovaldatelabel, 0, 6);
            //
            // simpleremovaldatepicker
            //
            this.simpleremovaldatepicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleremovaldatepicker.Name = "simpleremovaldatepicker";
            this.simpleremovaldatepicker.Size = new System.Drawing.Size(358, 20);
            this.simpleremovaldatepicker.TabIndex = 13;
            this.simpletable.Controls.Add(this.simpleremovaldatepicker, 1, 6);
            //
            // initoxidesettingspanel
            //
            this.initoxidesettingspanel.Controls.Add(this.initoxidetable);
            this.initoxidesettingspanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.initoxidesettingspanel.Location = new System.Drawing.Point(0, 0);
            this.initoxidesettingspanel.Name = "initoxidesettingspanel";
            this.initoxidesettingspanel.Size = new System.Drawing.Size(510, 397);
            this.initoxidesettingspanel.TabIndex = 1;
            //
            // initoxidetable
            //
            this.initoxidetable.ColumnCount = 2;
            this.initoxidetable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.initoxidetable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.initoxidetable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.initoxidetable.Location = new System.Drawing.Point(0, 0);
            this.initoxidetable.Name = "initoxidetable";
            this.initoxidetable.Padding = new System.Windows.Forms.Padding(8);
            this.initoxidetable.RowCount = 2;
            this.initoxidetable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.initoxidetable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.initoxidetable.Size = new System.Drawing.Size(510, 397);
            this.initoxidetable.TabIndex = 0;
            //
            // initoxideinjectionindexlabel
            //
            this.initoxideinjectionindexlabel.AutoSize = true;
            this.initoxideinjectionindexlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.initoxideinjectionindexlabel.Location = new System.Drawing.Point(11, 8);
            this.initoxideinjectionindexlabel.Name = "initoxideinjectionindexlabel";
            this.initoxideinjectionindexlabel.Size = new System.Drawing.Size(122, 23);
            this.initoxideinjectionindexlabel.TabIndex = 0;
            this.initoxideinjectionindexlabel.Text = "Injection Index:";
            this.initoxideinjectionindexlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.initoxidetable.Controls.Add(this.initoxideinjectionindexlabel, 0, 0);
            //
            // initoxideinjectionindex
            //
            this.initoxideinjectionindex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.initoxideinjectionindex.Location = new System.Drawing.Point(141, 11);
            this.initoxideinjectionindex.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            this.initoxideinjectionindex.Name = "initoxideinjectionindex";
            this.initoxideinjectionindex.Size = new System.Drawing.Size(358, 20);
            this.initoxideinjectionindex.TabIndex = 1;
            this.initoxidetable.Controls.Add(this.initoxideinjectionindex, 1, 0);
            //
            // modifysettingspanel
            //
            this.modifysettingspanel.Controls.Add(this.modifytable);
            this.modifysettingspanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modifysettingspanel.Location = new System.Drawing.Point(0, 0);
            this.modifysettingspanel.Name = "modifysettingspanel";
            this.modifysettingspanel.Size = new System.Drawing.Size(510, 397);
            this.modifysettingspanel.TabIndex = 2;
            //
            // modifytable
            //
            this.modifytable.ColumnCount = 2;
            this.modifytable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.modifytable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.modifytable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modifytable.Location = new System.Drawing.Point(0, 0);
            this.modifytable.Name = "modifytable";
            this.modifytable.Padding = new System.Windows.Forms.Padding(8);
            this.modifytable.RowCount = 4;
            this.modifytable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.modifytable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.modifytable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.modifytable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.modifytable.Size = new System.Drawing.Size(510, 397);
            this.modifytable.TabIndex = 0;
            //
            // modifyinjectionindexlabel
            //
            this.modifyinjectionindexlabel.AutoSize = true;
            this.modifyinjectionindexlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modifyinjectionindexlabel.Location = new System.Drawing.Point(11, 8);
            this.modifyinjectionindexlabel.Name = "modifyinjectionindexlabel";
            this.modifyinjectionindexlabel.Size = new System.Drawing.Size(122, 23);
            this.modifyinjectionindexlabel.TabIndex = 0;
            this.modifyinjectionindexlabel.Text = "Injection Index:";
            this.modifyinjectionindexlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.modifytable.Controls.Add(this.modifyinjectionindexlabel, 0, 0);
            //
            // modifyinjectionindex
            //
            this.modifyinjectionindex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modifyinjectionindex.Location = new System.Drawing.Point(141, 11);
            this.modifyinjectionindex.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            this.modifyinjectionindex.Name = "modifyinjectionindex";
            this.modifyinjectionindex.Size = new System.Drawing.Size(358, 20);
            this.modifyinjectionindex.TabIndex = 1;
            this.modifytable.Controls.Add(this.modifyinjectionindex, 1, 0);
            //
            // modifyremovecountlabel
            //
            this.modifyremovecountlabel.AutoSize = true;
            this.modifyremovecountlabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modifyremovecountlabel.Name = "modifyremovecountlabel";
            this.modifyremovecountlabel.Size = new System.Drawing.Size(122, 23);
            this.modifyremovecountlabel.TabIndex = 2;
            this.modifyremovecountlabel.Text = "Remove Count:";
            this.modifyremovecountlabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.modifytable.Controls.Add(this.modifyremovecountlabel, 0, 1);
            //
            // modifyremovecount
            //
            this.modifyremovecount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modifyremovecount.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            this.modifyremovecount.Name = "modifyremovecount";
            this.modifyremovecount.Size = new System.Drawing.Size(358, 20);
            this.modifyremovecount.TabIndex = 3;
            this.modifytable.Controls.Add(this.modifyremovecount, 1, 1);
            //
            // modifynotelabel
            //
            this.modifynotelabel.AutoSize = false;
            this.modifynotelabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modifynotelabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.modifynotelabel.Name = "modifynotelabel";
            this.modifynotelabel.Size = new System.Drawing.Size(358, 40);
            this.modifynotelabel.TabIndex = 4;
            this.modifynotelabel.Text = "This hook type also has a generated instruction list. Save this hook first, the" +
    "n edit its instructions from the hook\'s own tab.";
            this.modifytable.Controls.Add(this.modifynotelabel, 1, 2);
            //
            // buttonpanel
            //
            this.buttonpanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonpanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttonpanel.Location = new System.Drawing.Point(0, 429);
            this.buttonpanel.Name = "buttonpanel";
            this.buttonpanel.Padding = new System.Windows.Forms.Padding(8);
            this.buttonpanel.Size = new System.Drawing.Size(524, 42);
            this.buttonpanel.TabIndex = 1;
            //
            // cancelbutton
            //
            this.cancelbutton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelbutton.Location = new System.Drawing.Point(437, 8);
            this.cancelbutton.Name = "cancelbutton";
            this.cancelbutton.Size = new System.Drawing.Size(79, 26);
            this.cancelbutton.TabIndex = 0;
            this.cancelbutton.Text = "Cancel";
            this.cancelbutton.UseVisualStyleBackColor = true;
            this.buttonpanel.Controls.Add(this.cancelbutton);
            //
            // okbutton
            //
            this.okbutton.Location = new System.Drawing.Point(352, 8);
            this.okbutton.Name = "okbutton";
            this.okbutton.Size = new System.Drawing.Size(79, 26);
            this.okbutton.TabIndex = 1;
            this.okbutton.Text = "OK";
            this.okbutton.UseVisualStyleBackColor = true;
            this.okbutton.Click += new System.EventHandler(this.okbutton_Click);
            this.buttonpanel.Controls.Add(this.okbutton);
            //
            // findmethodbutton
            //
            this.findmethodbutton.Location = new System.Drawing.Point(267, 8);
            this.findmethodbutton.Name = "findmethodbutton";
            this.findmethodbutton.Size = new System.Drawing.Size(97, 26);
            this.findmethodbutton.TabIndex = 2;
            this.findmethodbutton.Text = "Find Method...";
            this.findmethodbutton.UseVisualStyleBackColor = true;
            this.findmethodbutton.Click += new System.EventHandler(this.findmethodbutton_Click);
            this.buttonpanel.Controls.Add(this.findmethodbutton);
            //
            // HookDetailsForm
            //
            this.AcceptButton = this.okbutton;
            this.CancelButton = this.cancelbutton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 471);
            this.Controls.Add(this.formtabs);
            this.Controls.Add(this.buttonpanel);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ShowIcon = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "HookDetailsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Hook Details";
            this.formtabs.ResumeLayout(false);
            this.generaltab.ResumeLayout(false);
            this.detailstable.ResumeLayout(false);
            this.detailstable.PerformLayout();
            this.typesettingstab.ResumeLayout(false);
            this.typesettingspanel.ResumeLayout(false);
            this.simplesettingspanel.ResumeLayout(false);
            this.simpletable.ResumeLayout(false);
            this.simpletable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.simpleinjectionindex)).EndInit();
            this.initoxidesettingspanel.ResumeLayout(false);
            this.initoxidetable.ResumeLayout(false);
            this.initoxidetable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.initoxideinjectionindex)).EndInit();
            this.modifysettingspanel.ResumeLayout(false);
            this.modifytable.ResumeLayout(false);
            this.modifytable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modifyinjectionindex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.modifyremovecount)).EndInit();
            this.buttonpanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl formtabs;
        private System.Windows.Forms.TabPage generaltab;
        private System.Windows.Forms.TableLayoutPanel detailstable;
        private System.Windows.Forms.Label hooktypelabel;
        private System.Windows.Forms.ComboBox hooktypedropdown;
        private System.Windows.Forms.Label categorylabel;
        private System.Windows.Forms.TextBox categorytextbox;
        private System.Windows.Forms.Label namelabel;
        private System.Windows.Forms.TextBox nametextbox;
        private System.Windows.Forms.Label hooknamelabel;
        private System.Windows.Forms.TextBox hooknametextbox;
        private System.Windows.Forms.Label hookdescriptionlabel;
        private System.Windows.Forms.TextBox hookdescriptiontextbox;
        private System.Windows.Forms.Label assemblylabel;
        private System.Windows.Forms.ComboBox assemblydropdown;
        private System.Windows.Forms.Label typenamelabel;
        private System.Windows.Forms.TextBox typenametextbox;
        private System.Windows.Forms.Label exposurelabel;
        private System.Windows.Forms.ComboBox exposuredropdown;
        private System.Windows.Forms.Label returntypelabel;
        private System.Windows.Forms.TextBox returntypetextbox;
        private System.Windows.Forms.Label methodnamelabel;
        private System.Windows.Forms.TextBox methodnametextbox;
        private System.Windows.Forms.Label parameterslabel;
        private System.Windows.Forms.TextBox parameterstextbox;
        private System.Windows.Forms.Label msilhashlabel;
        private System.Windows.Forms.TextBox msilhashtextbox;
        private System.Windows.Forms.TabPage typesettingstab;
        private System.Windows.Forms.Panel typesettingspanel;
        private System.Windows.Forms.Panel simplesettingspanel;
        private System.Windows.Forms.TableLayoutPanel simpletable;
        private System.Windows.Forms.Label simpleinjectionindexlabel;
        private System.Windows.Forms.NumericUpDown simpleinjectionindex;
        private System.Windows.Forms.Label simplereturnbehaviorlabel;
        private System.Windows.Forms.ComboBox simplereturnbehavior;
        private System.Windows.Forms.Label simpleargumentbehaviorlabel;
        private System.Windows.Forms.ComboBox simpleargumentbehavior;
        private System.Windows.Forms.Label simpleargumentstringlabel;
        private System.Windows.Forms.TextBox simpleargumentstring;
        private System.Windows.Forms.Label simpledeprecatedlabel;
        private System.Windows.Forms.CheckBox simpledeprecatedcheckbox;
        private System.Windows.Forms.Label simplereplacementhooklabel;
        private System.Windows.Forms.TextBox simplereplacementhooktextbox;
        private System.Windows.Forms.Label simpleremovaldatelabel;
        private System.Windows.Forms.DateTimePicker simpleremovaldatepicker;
        private System.Windows.Forms.Panel initoxidesettingspanel;
        private System.Windows.Forms.TableLayoutPanel initoxidetable;
        private System.Windows.Forms.Label initoxideinjectionindexlabel;
        private System.Windows.Forms.NumericUpDown initoxideinjectionindex;
        private System.Windows.Forms.Panel modifysettingspanel;
        private System.Windows.Forms.TableLayoutPanel modifytable;
        private System.Windows.Forms.Label modifyinjectionindexlabel;
        private System.Windows.Forms.NumericUpDown modifyinjectionindex;
        private System.Windows.Forms.Label modifyremovecountlabel;
        private System.Windows.Forms.NumericUpDown modifyremovecount;
        private System.Windows.Forms.Label modifynotelabel;
        private System.Windows.Forms.FlowLayoutPanel buttonpanel;
        private System.Windows.Forms.Button okbutton;
        private System.Windows.Forms.Button cancelbutton;
        private System.Windows.Forms.Button findmethodbutton;
    }
}
