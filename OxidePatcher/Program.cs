using OxidePatcher.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                string filename = "RustExperimental.opj";
                bool unflagAll = false;
                foreach (string opt in args)
                {
                    if (opt.Contains("-unflag"))
                    {
                        unflagAll = true;
                    } else if (!opt.StartsWith("-") && opt.EndsWith(".opj"))
                    {
                        filename = opt;
                    }
                    else
                    {
                        Console.WriteLine("Unknown or invalid option: " + opt);
                        return;
                    }
                }
                if (!System.IO.File.Exists(filename))
                {
                    Console.WriteLine(filename + " does not exist!");
                    return;
                }
                Project PatchProject = Project.Load(filename);
                if (unflagAll)
                {
                    unflag(PatchProject, filename);
                }
                Patcher patcher = new Patcher(PatchProject);
                patcher.Patch(true);
                Console.WriteLine("Press Enter to continue...");
            }
        }

        private static void unflag(Project project, string filename)
        {
            bool updated = false;
            foreach (var hook in project.Manifests.SelectMany((m) => m.Hooks))
            {
                if (hook.Flagged)
                {
                    hook.Flagged = false;
                    updated = true;
                    Console.WriteLine("Hook " + hook.HookName + " has been unflagged.");
                }
            }
            if (updated)
            {
                project.Save(filename);
            }
        }
    }
}
