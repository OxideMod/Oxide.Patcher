using OxidePatcher.Patching;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
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
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args1) =>
            {
                String resourceName = "OxidePatcher.Dependencies." +
                   new AssemblyName(args1.Name).Name + ".dll";
                if (resourceName.Contains("resources.dll"))
                {
                    return null;
                }
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    Byte[] assemblyData = new Byte[stream.Length];

                    stream.Read(assemblyData, 0, assemblyData.Length);

                    return Assembly.Load(assemblyData);

                }

            }; 

            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new PatcherForm());
            }
            else
            {
                bool console = false;
                string filename = "RustExperimental.opj";
                bool unflagAll = false;
                string targetOverride = "";
                string error = "";
                int n = 0;

                while (n < args.Length)
                {

                    if (args[n].Contains("-unflag"))
                    {
                        unflagAll = true;
                    }
                    else if (!args[n].StartsWith("-") && args[n].EndsWith(".opj"))
                    {
                        filename = args[n];
                    }
                    else if (args[n].Contains("-c"))
                    {
                        console = true;
                    }
                    else if (args[n].Contains("-p"))
                    {
                        try
                        {
                            if (!args[n + 1].StartsWith("-") && !(args[n + 1].EndsWith(".opj")))
                            {
                                targetOverride = args[n + 1];
                                n++;
                            }
                            else if (args[n + 1].StartsWith("-"))
                            {
                                error = "-p requires a file path.";
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            error = "-p requires a file path.";
                        }
                    }
                    else
                    {
                        error = "Unknown or invalid option: " + args[n];
                    }
                    n++;
                }
                if (console)
                {
                    // redirect console output to parent process;
                    // must be before any calls to Console.WriteLine()
                    AttachConsole(ATTACH_PARENT_PROCESS);
                }
                if (error != "" && console)
                {
                    Console.WriteLine("ERROR: " + error);
                    return;
                }
                else if (error != "")
                {
                    MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (console && !Directory.Exists(targetOverride) && targetOverride != "")
                {
                    Console.WriteLine(targetOverride + " does not exist!");
                    return;
                }
                else if (!Directory.Exists(targetOverride) && targetOverride != "")
                {
                    MessageBox.Show(targetOverride + " does not exist!", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (console && !System.IO.File.Exists(filename))
                {
                    Console.WriteLine(filename + " does not exist!");
                    return;
                }
                else if (!System.IO.File.Exists(filename))
                {
                    MessageBox.Show(filename + " does not exist!", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Project PatchProject = null;
                if (targetOverride == "")
                {
                    PatchProject = Project.Load(filename);
                }
                else
                {
                    PatchProject = Project.Load(filename, targetOverride);
                }
                if (unflagAll)
                {
                    unflag(PatchProject, filename, console);
                }
                if (!console)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new PatcherForm(filename));
                }
                else
                {
                    Patcher patcher = new Patcher(PatchProject);
                    patcher.Patch(true);
                    Console.WriteLine("Press Enter to continue...");
                }
            }
        }

        private static void unflag(Project project, string filename, bool console)
        {
            bool updated = false;
            foreach (var hook in project.Manifests.SelectMany((m) => m.Hooks))
            {
                if (hook.Flagged)
                {
                    hook.Flagged = false;
                    updated = true;
                    if (console)
                    {
                        Console.WriteLine("Hook " + hook.HookName + " has been unflagged.");
                    }
                }
            }
            if (updated)
            {
                project.Save(filename);
            }
        }
    }
}
