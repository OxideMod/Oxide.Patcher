using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OxidePatcher.Patching;

namespace OxidePatcher
{
    public partial class PatchProcessForm : Form
    {
        private delegate void WriteLogDelegate(string message);

        /// <summary>
        /// Gets or sets the project to patch
        /// </summary>
        public Project PatchProject { get; set; }

        private Task thetask;

        public PatchProcessForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            thetask = new Task(Worker);
            thetask.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!thetask.IsCompleted)
            {
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }

        private void WriteLog(string message)
        {
            if (message.Contains(Environment.NewLine))
            {
                string[] items = message.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                statuslabel.Text = items[0];
                for (int i = 0; i < items.Length; i++)
                {
                    patchlog.Items.Add(items[i]);
                }
            }
            else
            {
                statuslabel.Text = message;
                patchlog.Items.Add(message);
            }
        }

        private void OnWorkComplete()
        {
            copybutton.Enabled = true;
            closebutton.Enabled = true;
            progressbar.Value = 100;
        }

        #region Worker Thread

        private void WorkerWriteLog(string format, params object[] args)
        {
            Invoke(new WriteLogDelegate(WriteLog), string.Format(format, args));
        }

        private void WorkerCompleteWork()
        {
            Invoke(new Action(OnWorkComplete));
        }

        private void Worker()
        {
            WorkerWriteLog("Started patching.");
            try
            {
                Patcher patcher = new Patcher(PatchProject);
                patcher.OnLogMessage += (msg) => WorkerWriteLog(msg);
                patcher.Patch(false);
            }
            catch (Exception ex)
            {
                WorkerWriteLog("ERROR: {0}", ex.Message);
            }
            WorkerWriteLog("Patch complete.");
            WorkerCompleteWork();
        }

        #endregion

        private void closebutton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void copybutton_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < patchlog.Items.Count; i++)
            {
                sb.AppendLine((string)patchlog.Items[i]);
            }
            Clipboard.SetText(sb.ToString());
        }
    }
}
