using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Oxide.Patcher
{
    public partial class PatchProcessForm : Form
    {
        private delegate void WriteLogDelegate(string message);

        private delegate void WriteProgressDelegate(string message);

        /// <summary>
        /// Gets or sets the project to patch
        /// </summary>
        public Project PatchProject { get; set; }

        private Task thetask;

        private int errors;

        public PatchProcessForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            errors = 0;

            foreach (Manifest manifest in PatchProject.Manifests)
            {
                progressbar.Maximum += manifest.Hooks.Count(h => h.BaseHook == null || h.BaseHook != null && h.Flagged) + manifest.Modifiers.Count + manifest.Fields.Count + 2;
            }

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
            statuslabel.Text = message;
            patchlog.Items.Add(message);

            patchlog.SelectedIndex = patchlog.Items.Count - 1;
        }

        private void ReportProgress(string message)
        {
            if (message.Contains("Failed to apply"))
            {
                errors++;
                if (errors == 1)
                {
                    progressbar.SetState(2);
                }
            }

            progressbar.Value++;
            progressbar.Refresh();

            if (message.Contains(Environment.NewLine))
            {
                string[] items = message.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
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

            patchlog.SelectedIndex = patchlog.Items.Count - 1;
        }

        private void OnWorkComplete()
        {
            copybutton.Enabled = true;
            closebutton.Enabled = true;
        }

        #region Worker Thread

        private void WorkerWriteLog(string format, params object[] args)
        {
            Invoke(new WriteLogDelegate(WriteLog), string.Format(format, args));
        }

        private void WorkerReportProgress(string format, params object[] args)
        {
            Invoke(new WriteProgressDelegate(ReportProgress), string.Format(format, args));
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
                Patching.Patcher patcher = new Patching.Patcher(PatchProject);
                patcher.OnLogMessage += msg => WorkerReportProgress(msg);
                patcher.Patch();
            }
            catch (Exception ex)
            {
                WorkerWriteLog("ERROR: {0}", ex.Message);
            }

            if (errors > 0)
            {
                WorkerWriteLog($"Failed to apply {errors} {(errors == 1 ? "hook" : "hooks")}");
            }
            else
            {
                WorkerWriteLog("Patch complete.");
            }

            WorkerCompleteWork();
        }

        #endregion Worker Thread

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

    public static class ModifyProgressBarColor
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);
        public static void SetState(this ProgressBar pBar, int state)
        {
            SendMessage(pBar.Handle, 1040, (IntPtr)state, IntPtr.Zero);
        }
    }
}
