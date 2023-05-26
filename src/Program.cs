using Oxide.Patcher.Hooks;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Mono.Cecil;
using Oxide.Patcher.Docs;

namespace Oxide.Patcher
{
    internal static class Program
    {
        public static Project PatchProject;

        public static AssemblyDefinition OxideAssembly { get; private set; }

        // defines for commandline output
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        private const int ATTACH_PARENT_PROCESS = -1;

        static Program()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args1) =>
            {
                string resourceName = $"Oxide.Patcher.Dependencies.{new AssemblyName(args1.Name).Name}.dll";
                if (resourceName.Contains("resources.dll"))
                {
                    return null;
                }

                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    byte[] assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            // Load oxide assembly
            string oxideFileName = Path.Combine(Application.StartupPath, "Oxide.Core.dll");
            if (!File.Exists(oxideFileName))
            {
                if (Array.Exists(args, x => x == "-c"))
                {
                    Console.WriteLine("Failed to locate Oxide.Core.dll!");
                }
                else
                {
                    MessageBox.Show("Failed to locate Oxide.Core.dll!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                Environment.Exit(0);
                return;
            }

            OxideAssembly = AssemblyDefinition.ReadAssembly(oxideFileName);

            if (Array.Exists(args, x => x == "-docs"))
            {
                GenerateDocsFile(args);
                return;
            }

            if (args.Length == 0 || !Array.Exists(args, x => x == "-c"))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new PatcherForm());
                return;
            }

            RunHeadless(args);
        }

        private static void RunHeadless(string[] args)
        {
            string fileName = "Rust.opj";
            bool unflagAll = false;
            string targetOverride = string.Empty;

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    // redirect console output to parent process;
                    // must be before any calls to Console.WriteLine()
                    AttachConsole(ATTACH_PARENT_PROCESS);
                    break;
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg.StartsWith("-c"))
                {
                    continue;
                }

                if (arg.Contains("-unflag"))
                {
                    unflagAll = true;
                    continue;
                }

                if (!arg.StartsWith("-") && arg.EndsWith(".opj"))
                {
                    fileName = arg;
                    continue;
                }

                if (arg.Contains("-p"))
                {
                    try
                    {
                        string nextArg = args[i + 1];
                        if (nextArg.StartsWith("-"))
                        {
                            throw new Exception();
                        }

                        if (!nextArg.EndsWith(".opj"))
                        {
                            targetOverride = nextArg;
                            i++;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("ERROR: -p requires a file path.");
                        return;
                    }

                    continue;
                }

                Console.WriteLine($"ERROR: Unknown or invalid option: {arg}");
                return;
            }

            if (!string.IsNullOrEmpty(targetOverride) && !Directory.Exists(targetOverride))
            {
                Console.WriteLine($"{targetOverride} does not exist!");
                return;
            }

            if (!File.Exists(fileName))
            {
                Console.WriteLine($"{fileName} does not exist!");
                return;
            }

            PatchProject = Project.Load(fileName, targetOverride);

            if (unflagAll)
            {
                UnflagAll(PatchProject, fileName);
            }

            try
            {
                Patching.Patcher patcher = new Patching.Patcher(PatchProject, true);
                patcher.Patch();
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error while patching: {0}", e);
            }

            Console.WriteLine("Press Enter to continue...");
        }

        private static void GenerateDocsFile(string[] args)
        {
            string fileName = null, targetOverride = "", docsOutput = null;

                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (!arg.StartsWith("-") && arg.EndsWith(".opj"))
                    {
                        fileName = arg;
                        continue;
                    }

                    if (arg.Contains("-p"))
                    {
                        try
                        {
                            string nextArg = args[i + 1];
                            if (nextArg.StartsWith("-"))
                            {
                                throw new Exception();
                            }

                            if (!nextArg.EndsWith(".opj"))
                            {
                                targetOverride = nextArg;
                                i++;
                            }
                        }
                        catch
                        {
                            Console.WriteLine("ERROR: -p requires a file path.");
                            return;
                        }

                        continue;
                    }

                    if (arg.StartsWith("-docsfile"))
                    {
                        try
                        {
                            string nextArg = args[i + 1];
                            if (nextArg.StartsWith("-"))
                            {
                                throw new Exception();
                            }

                            if (!nextArg.EndsWith(".opj"))
                            {
                                docsOutput = nextArg;
                                i++;
                            }
                        }
                        catch
                        {
                            Console.WriteLine("ERROR: -docsfile requires a file path.");
                            return;
                        }
                    }
                }

                if (fileName == null || docsOutput == null)
                {
                    throw new Exception("Target opj file name or output target was not set.");
                }

                try
                {
                    DocsGenerator.GenerateFile(fileName, docsOutput, targetOverride);
                    Console.WriteLine("Successfully generated docs data file.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to generate docs data file, {e}");
                }
        }

        private static void UnflagAll(Project project, string filename)
        {
            bool updated = false;
            foreach (Hook hook in project.Manifests.SelectMany(m => m.Hooks))
            {
                if (!hook.Flagged)
                {
                    continue;
                }

                hook.Flagged = false;
                updated = true;

                Console.WriteLine($"Hook '{hook.HookName}' has been unflagged.");
            }

            if (updated)
            {
                project.Save(filename);
            }
        }
    }
}
