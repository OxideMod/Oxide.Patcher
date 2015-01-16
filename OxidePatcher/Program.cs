using OxidePatcher.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OxidePatcher
{
    static class Program
    {
        // defines for commandline output
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // redirect console output to parent process;
            // must be before any calls to Console.WriteLine()
            AttachConsole(ATTACH_PARENT_PROCESS);

            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new PatcherForm());
            }
            else if (args.Length == 1)
            {
                Project PatchProject = Project.Load(args[0]);
                Patcher patcher = new Patcher(PatchProject);
                patcher.Patch(true);
                Console.WriteLine("Press Enter to continue...");
            }
            else
            {
                Console.WriteLine("SYNTAX: 'OxidePatcher.exe <PATH TO OPJ FILE>'");
                Console.WriteLine("Press Enter to continue...");
            }
        }
    }
}
