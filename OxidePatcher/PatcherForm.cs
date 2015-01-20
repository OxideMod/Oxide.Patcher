using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

using OxidePatcher.Views;
using OxidePatcher.Hooks;
using OxidePatcher.Deobfuscation;

using Mono.Cecil;

namespace OxidePatcher
{

    public partial class PatcherForm : Form
    {
        /// <summary>
        /// Gets the currently open project
        /// </summary>
        public Project CurrentProject { get; private set; }

        /// <summary>
        /// Gets the filename of the currently open project
        /// </summary>
        public string CurrentProjectFilename { get; private set; }

        /// <summary>
        /// Gets the current settings
        /// </summary>
        public PatcherFormSettings CurrentSettings { get; private set; }

        /// <summary>
        /// Gets the oxide assembly
        /// </summary>
        public AssemblyDefinition OxideAssembly { get; private set; }

        private Dictionary<string, AssemblyDefinition> assemblydict;

        private IAssemblyResolver resolver;

        private Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        private class NodeAssemblyData
        {
            public bool Included { get; set; }
            public bool Loaded { get; set; }
            public string AssemblyName { get; set; }
            public AssemblyDefinition Definition { get; set; }
        }

        public PatcherForm()
        {
            InitializeComponent();
            string title = String.Format(this.Text, version);
            this.Text = title.Slice(0, title.LastIndexOf("."));
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Load oxide
            string oxidefilename = Path.Combine(Application.StartupPath, "Oxide.Core.dll");
            if (!File.Exists(oxidefilename))
            {
                MessageBox.Show("Failed to locate Oxide.Core.dll!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                return;
            }
            OxideAssembly = AssemblyDefinition.ReadAssembly(oxidefilename);

            // Load settings
            // CurrentSettings = PatcherFormSettings.Load();
            // Location = CurrentSettings.FormPosition;
            // Size = CurrentSettings.FormSize;
            // WindowState = CurrentSettings.WindowState;

            assemblydict = new Dictionary<string, AssemblyDefinition>();


        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // base.OnFormClosing(e);

            // Save settings
            // CurrentSettings.FormPosition = Location;
            // CurrentSettings.FormSize = Size;
            // CurrentSettings.WindowState = WindowState;
            // CurrentSettings.Save();
        }

        #region Menu Handlers

        /// <summary>
        /// Called when the open project menu item was clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openproject_Click(object sender, EventArgs e)
        {
            DialogResult result = openprojectdialog.ShowDialog(this);
            if (result == System.Windows.Forms.DialogResult.OK)
                OpenProject(openprojectdialog.FileName);
        }

        /// <summary>
        /// Called when the new project menu item was clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newproject_Click(object sender, EventArgs e)
        {
            NewProjectForm form = new NewProjectForm();
            form.ShowDialog(this);
        }

        /// <summary>
        /// Called when the exit menu item was clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void closetab_Click(object sender, EventArgs e)
        {
            tabview.SelectedTab.Dispose();
        }

        private void unflagall_Click(object sender, EventArgs e)
        {
            if (CurrentProject != null)
            {
                foreach (var hook in CurrentProject.Manifests.SelectMany((m) => m.Hooks))
                {
                    if (hook.Flagged)
                    {
                        hook.Flagged = false;
                        UpdateHook(hook);
                    }
                }
            }
        }
        private void flagall_Click(object sender, EventArgs e)
        {
            if (CurrentProject != null)
            {
                foreach (var hook in CurrentProject.Manifests.SelectMany((m) => m.Hooks))
                {
                    if (hook.Flagged == false)
                    {
                        hook.Flagged = true;
                        UpdateHook(hook);
                    }
                }
            }
        }

        #endregion

        #region Toolbar Handlers

        /// <summary>
        /// Called when the new project tool icon was clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newprojecttool_Click(object sender, EventArgs e)
        {
            NewProjectForm form = new NewProjectForm();
            form.ShowDialog(this);
        }

        /// <summary>
        /// Called when the open project tool icon was clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openprojecttool_Click(object sender, EventArgs e)
        {
            DialogResult result = openprojectdialog.ShowDialog(this);
            if (result == System.Windows.Forms.DialogResult.OK)
                OpenProject(openprojectdialog.FileName);
        }

        /// <summary>
        /// Called when the patch tool icon was clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void patchtool_Click(object sender, EventArgs e)
        {
            PatchProcessForm patchprocess = new PatchProcessForm();
            patchprocess.PatchProject = CurrentProject;
            patchprocess.ShowDialog(this);
            UpdateAllHooks();
        }

        #endregion

        #region Object View Handlers

        private void objectview_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                TreeNode node = e.Node;
                if (node != null)
                {
                    if (node.Tag is NodeAssemblyData)
                    {
                        NodeAssemblyData data = (NodeAssemblyData)node.Tag;
                        if (!data.Included)
                        {
                            unloadedassemblymenu.Show(objectview, e.X, e.Y);

                        }
                        else
                        {
                            loadedassemblymenu.Show(objectview, e.X, e.Y);
                        }
                    }
                    else if (node.Tag is Hook)
                    {
                        hooksmenu.Show(objectview, e.X, e.Y);
                    }
                    objectview.SelectedNode = node;
                }
            }
        }

        private void objectview_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {

            // Check if the tab is already open somewhere
            foreach (TabPage tabpage in tabview.TabPages)
            {
                ProjectSettingsControl psControl = tabpage.Tag as ProjectSettingsControl;
                ClassViewControl cvControl = tabpage.Tag as ClassViewControl;

                if ((psControl != null || cvControl != null) && e.Node.Text == tabpage.Text)
                {
                    tabview.SelectedTab = tabpage;
                    return;
                }
            }

            if (e.Node.Tag is string)
            {
                string str = (string)e.Node.Tag;
                switch (str)
                {
                    case "Project Settings":
                        ProjectSettingsControl projectsettings = new ProjectSettingsControl();
                        projectsettings.ProjectFilename = CurrentProjectFilename;
                        projectsettings.ProjectObject = CurrentProject;
                        AddTab("Project Settings", projectsettings, projectsettings);
                        break;
                }
            }
            else if (e.Node.Tag is TypeDefinition)
            {
                TypeDefinition typedef = e.Node.Tag as TypeDefinition;
                if (typedef.IsClass)
                {
                    ClassViewControl classview = new ClassViewControl();
                    classview.TypeDef = typedef;
                    classview.MainForm = this;
                    AddTab(typedef.Name, classview, classview);
                }
            }
            else if (e.Node.Tag is Hook)
            {
                GotoHook(e.Node.Tag as Hook);
            }
        }

        private void unloadedassemblymenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Name == "addtoproject")
            {
                NodeAssemblyData data = (NodeAssemblyData)objectview.SelectedNode.Tag;
                CurrentProject.AddManifest(data.AssemblyName);
                CurrentProject.Save(CurrentProjectFilename);
                data.Included = true;
                data.Loaded = true;
                data.Definition = LoadAssembly(data.AssemblyName);
                objectview.SelectedNode.ImageKey = "accept.png";
                objectview.SelectedNode.SelectedImageKey = "accept.png";
                objectview.SelectedNode.Nodes.Clear();

                string realfilename = Path.Combine(CurrentProject.TargetDirectory, data.AssemblyName + ".dll");
                string origfilename = Path.Combine(CurrentProject.TargetDirectory, data.AssemblyName + "_Original.dll");
                if (!File.Exists(origfilename)) CreateOriginal(realfilename, origfilename);

                // Populate
                PopulateAssemblyNode(objectview.SelectedNode, data.Definition);
            }
        }

        private void loadedassemblymenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Name == "removefromproject")
            {
                NodeAssemblyData data = (NodeAssemblyData)objectview.SelectedNode.Tag;
                CurrentProject.RemoveManifest(data.AssemblyName);
                CurrentProject.Save(CurrentProjectFilename);
                data.Included = false;
                data.Loaded = false;
                data.Definition = null;
                if (objectview.SelectedNode.Tag == null)
                    objectview.SelectedNode.Parent.Nodes.Remove(objectview.SelectedNode);
                else
                {
                    objectview.SelectedNode.ImageKey = "cross.png";
                    objectview.SelectedNode.SelectedImageKey = "cross.png";
                    objectview.SelectedNode.Nodes.Clear();
                }
            }
        }

        #endregion

        private AssemblyDefinition LoadAssembly(string name)
        {
            AssemblyDefinition assdef;
            if (assemblydict.TryGetValue(name, out assdef)) return assdef;

            string file = string.Format("{0}_Original.dll", name);
            string filename = Path.Combine(CurrentProject.TargetDirectory, file);
            if (!File.Exists(filename))
            {
                string oldfile = string.Format("{0}.dll", name);
                string oldfilename = Path.Combine(CurrentProject.TargetDirectory, oldfile);
                if (!File.Exists(oldfilename))
                    return null;
                CreateOriginal(oldfilename, filename);
            }
            assdef = AssemblyDefinition.ReadAssembly(filename, new ReaderParameters { AssemblyResolver = resolver });
            assemblydict.Add(name, assdef);
            return assdef;
        }

        private void CreateOriginal(string oldfile, string newfile)
        {
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(oldfile, new ReaderParameters { AssemblyResolver = resolver });
            Deobfuscator deob = Deobfuscators.Find(assembly);
            if (deob != null)
            {
                DialogResult result = MessageBox.Show(this, string.Format("Assembly '{0}' appears to be obfuscated using '{1}', attempt to deobfuscate?", assembly.MainModule.Name, deob.Name), "Oxide Patcher", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Yes)
                {
                    // Deobfuscate
                    if (deob.Deobfuscate(assembly))
                    {
                        // Success
                        if (File.Exists(newfile)) File.Delete(newfile);
                        assembly.Write(newfile);
                        return;
                    }
                    else
                    {
                        MessageBox.Show(this, "Deobfuscation failed!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            File.Copy(oldfile, newfile);
        }

        private bool IsFileOriginal(string filename)
        {
            string name = Path.GetFileNameWithoutExtension(filename);
            const string postfix = "_Original";
            if (name.Length <= postfix.Length) return false;
            return name.Substring(name.Length - postfix.Length) == postfix;
        }

        private void PopulateInitialTree()
        {
            // Set status
            statuslabel.Text = "Loading project...";
            statuslabel.Invalidate();

            // Add project settings
            TreeNode projectsettings = new TreeNode("Project Settings");
            projectsettings.ImageKey = "cog_edit.png";
            projectsettings.SelectedImageKey = "cog_edit.png";
            projectsettings.Tag = "Project Settings";
            objectview.Nodes.Add(projectsettings);

            // Add hooks
            TreeNode hooks = new TreeNode("Hooks");
            hooks.ImageKey = "lightning.png";
            hooks.SelectedImageKey = "lightning.png";
            objectview.Nodes.Add(hooks);
            foreach (var hook in CurrentProject.Manifests.SelectMany((m) => m.Hooks))
            {
                TreeNode hooknode = new TreeNode(hook.Name);
                if (hook.Flagged)
                {
                    hooknode.ImageKey = "script_error.png";
                    hooknode.SelectedImageKey = "script_error.png";
                }
                else
                {
                    hooknode.ImageKey = "script_lightning.png";
                    hooknode.SelectedImageKey = "script_lightning.png";
                }
                hooknode.Tag = hook;
                hooks.Nodes.Add(hooknode);
            }

            // Add assemblies
            TreeNode assemblies = new TreeNode("Assemblies");
            assemblies.ImageKey = "folder.png";
            assemblies.SelectedImageKey = "folder.png";
            objectview.Nodes.Add(assemblies);
            List<TreeNode> assemblynodes = new List<TreeNode>();
            foreach (string file in Directory.EnumerateFiles(CurrentProject.TargetDirectory, "*.dll"))
            {
                // Check if it's an original dll
                if (!IsFileOriginal(file))
                {

                    // See if it has a manifest
                    string assemblyname = Path.GetFileNameWithoutExtension(file);
                    if (CurrentProject.Manifests.Any((x) => x.AssemblyName == assemblyname))
                    {
                        // Get the manifest
                        // Manifest manifest = CurrentProject.Manifests.Single((x) => x.AssemblyName == assemblyname);

                        // Load the assembly
                        NodeAssemblyData data = new NodeAssemblyData();
                        data.Included = true;
                        data.AssemblyName = assemblyname;
                        data.Loaded = true;
                        data.Definition = LoadAssembly(assemblyname);

                        // Create a node for it
                        TreeNode assembly = new TreeNode(assemblyname);
                        if (data.Definition == null)
                        {
                            assembly.ImageKey = "error.png";
                            assembly.SelectedImageKey = "error.png";
                        }
                        else
                        {
                            assembly.ImageKey = "accept.png";
                            assembly.SelectedImageKey = "accept.png";
                        }
                        assembly.Tag = data;
                        assemblynodes.Add(assembly);

                        // Populate
                        if (data.Definition != null)
                            PopulateAssemblyNode(assembly, data.Definition);
                    }
                    else
                    {
                        // Nope, just make an empty node for it
                        TreeNode assembly = new TreeNode(assemblyname);
                        assembly.ImageKey = "cross.png";
                        assembly.SelectedImageKey = "cross.png";
                        assembly.Tag = new NodeAssemblyData() { Included = false, AssemblyName = assemblyname };
                        assemblynodes.Add(assembly);
                    }
                }
            }

            // Sort
            assemblynodes.Sort((a, b) =>
            {
                return Comparer<string>.Default.Compare(a.ImageKey, b.ImageKey);
            });

            // Add
            for (int i = 0; i < assemblynodes.Count; i++)
                assemblies.Nodes.Add(assemblynodes[i]);

            // Set status
            statuslabel.Text = "";
        }

        private sealed class NamespaceData
        {
            public string Name { get; private set; }
            public List<NamespaceData> ChildNamespaces { get; private set; }
            public List<TypeDefinition> ChildTypes { get; private set; }
            public NamespaceData Parent { get; set; }

            public NamespaceData(string name)
            {
                Name = name;
                ChildNamespaces = new List<NamespaceData>();
                ChildTypes = new List<TypeDefinition>();
            }
        }

        private void PopulateAssemblyNode(TreeNode root, AssemblyDefinition definition)
        {
            // Build collection of all types
            HashSet<TypeDefinition> alltypes = new HashSet<TypeDefinition>(definition.Modules.SelectMany((x) => x.GetTypes()));

            // Sort types into their namespaces
            Dictionary<string, NamespaceData> namespaces = new Dictionary<string, NamespaceData>();
            NamespaceData globalnamespace = new NamespaceData("");
            namespaces.Add("", globalnamespace);
            foreach (TypeDefinition typedef in alltypes)
            {
                NamespaceData nspcdata;
                if (!namespaces.TryGetValue(typedef.Namespace, out nspcdata))
                {
                    nspcdata = new NamespaceData(typedef.Namespace);
                    namespaces.Add(nspcdata.Name, nspcdata);
                }
                if (typedef.Namespace == "") globalnamespace = nspcdata;
                nspcdata.ChildTypes.Add(typedef);
            }

            // Setup namespace hierarchy
            bool done = false;
            while (!done)
            {
                done = true;
                foreach (var pair in namespaces)
                {
                    if (pair.Value.Parent == null && pair.Value != globalnamespace)
                    {
                        if (pair.Key.Contains('.'))
                        {
                            string[] spl = pair.Key.Split('.');
                            string[] splm = new string[spl.Length - 1];
                            Array.Copy(spl, splm, splm.Length);
                            string parent = string.Concat(splm);
                            NamespaceData parentnamespace;
                            if (!namespaces.TryGetValue(parent, out parentnamespace))
                            {
                                parentnamespace = new NamespaceData(parent);
                                namespaces.Add(parent, parentnamespace);
                                parentnamespace.ChildNamespaces.Add(pair.Value);
                                pair.Value.Parent = parentnamespace;
                                done = false;
                                break;
                            }
                            parentnamespace.ChildNamespaces.Add(pair.Value);
                            pair.Value.Parent = parentnamespace;
                        }
                        else
                        {
                            globalnamespace.ChildNamespaces.Add(pair.Value);
                            pair.Value.Parent = globalnamespace;
                        }
                    }
                }
            }

            // Populate tree
            PopulateAssemblyNode(root, globalnamespace, true);
        }

        private void PopulateAssemblyNode(TreeNode root, NamespaceData namespacedata, bool isglobal = false)
        {
            // Add global namespace node if needed
            TreeNode typeparent;
            if (isglobal)
            {
                typeparent = new TreeNode("global");
                typeparent.ImageKey = "namespace.png";
                typeparent.SelectedImageKey = "namespace.png";
                root.Nodes.Add(typeparent);
            }
            else
                typeparent = root;

            // Add all namespaces
            NamespaceData[] namespaces = namespacedata.ChildNamespaces.ToArray();
            Array.Sort(namespaces, (a, b) => Comparer<string>.Default.Compare(a.Name, b.Name));
            for (int i = 0; i < namespaces.Length; i++)
            {
                // Get namespace
                NamespaceData data = namespaces[i];
                string[] spl = data.Name.Split('.');
                string subname = spl[spl.Length - 1];

                // Create a node for it
                TreeNode namespacenode = new TreeNode(subname);
                namespacenode.ImageKey = "namespace.png";
                namespacenode.SelectedImageKey = "namespace.png";
                root.Nodes.Add(namespacenode);

                // Recursively fill in children
                PopulateAssemblyNode(namespacenode, data);
            }

            // Add all types
            TypeDefinition[] typedefs = namespacedata.ChildTypes.ToArray();
            Array.Sort(typedefs, (a, b) => Comparer<string>.Default.Compare(a.Name, b.Name));
            for (int i = 0; i < typedefs.Length; i++)
            {
                // Get type
                TypeDefinition typedef = typedefs[i];
                if (!typedef.IsNested)
                {
                    // Create a node for it
                    TreeNode typenode = new TreeNode(typedef.Name);
                    string img = SelectIcon(typedef);
                    typenode.ImageKey = img;
                    typenode.SelectedImageKey = img;
                    typenode.Tag = typedef;
                    typeparent.Nodes.Add(typenode);

                    // Populate any nested types
                    PopulateAssemblyNode(typenode, typedef);
                }
            }
        }

        private void PopulateAssemblyNode(TreeNode root, TypeDefinition roottype)
        {
            // Add all types
            TypeDefinition[] typedefs = roottype.NestedTypes.ToArray();
            Array.Sort(typedefs, (a, b) => Comparer<string>.Default.Compare(a.Name, b.Name));
            for (int i = 0; i < typedefs.Length; i++)
            {
                // Get type
                TypeDefinition typedef = typedefs[i];
                
                // Create a node for it
                TreeNode typenode = new TreeNode(typedef.Name);
                string img = SelectIcon(typedef);
                typenode.ImageKey = img;
                typenode.SelectedImageKey = img;
                typenode.Tag = typedef;
                root.Nodes.Add(typenode);

                // Populate any nested types
                PopulateAssemblyNode(typenode, typedef);
            }
        }

        private string SelectIcon(TypeDefinition typedef)
        {
            if (typedef.IsClass)
            {
                if (!typedef.IsPublic)
                    return "Class-Private_493.png";
                else if (typedef.IsSealed)
                    return "Class-Sealed_490.png";
                else
                    return "Class_489.png";
            }
            else if (typedef.IsInterface)
            {
                if (!typedef.IsPublic)
                    return "Interface-Private_616.png";
                else if (typedef.IsSealed)
                    return "Interface-Sealed_613.png";
                else
                    return "Interface_612.png";
            }
            return "script_error.png";
        }

        private void AddTab(string name, Control control, object tag)
        {
            // Create tab
            TabPage tab = new TabPage();
            tab.Text = name;
            tab.Tag = tag;
            
            // Measure text size
            //var size = tab.CreateGraphics().MeasureString(tab.Text, tab.Font);
            //tab.Width = (int)size.Width + 100;
            

            // Setup child control
            if (control != null)
            {
                tab.Controls.Add(control);
                control.Dock = DockStyle.Fill;
            }

            // Add tab and select
            tabview.TabPages.Add(tab);
            tabview.SelectedTab = tab;
        }

        private void tabview_MouseDown(object sender, MouseEventArgs e)
        {
            /*for (int i = 0; i < tabview.Controls.Count; i++)
            {
                var r = tabview.GetTabRect(i);
                var closebutton = new System.Drawing.Rectangle(r.Right - 15, r.Top + 4, 9, 7);
                if (closebutton.Contains(e.Location))
                {
                    tabview.Controls.Remove(tabview.Controls[i]);
                }
            }*/
        }

        private void tabview_DrawItem(object sender, DrawItemEventArgs e)
        {
            //e.Graphics.DrawString("x", e.Font, System.Drawing.Brushes.Black, e.Bounds.Right - 15, e.Bounds.Top + 4);
            //e.Graphics.DrawString(tabview.TabPages[e.Index].Text, e.Font, System.Drawing.Brushes.Black, e.Bounds.Left + 12, e.Bounds.Top + 4);
            //e.DrawFocusRectangle();
        }

        private void VerifyProject()
        {
            // Step 1: Check all included assemblies are intact
            // Step 2: Check all hooks are intact
            int missingassemblies = 0, missingmethods = 0, changedmethods = 0;
            foreach (Manifest manifest in CurrentProject.Manifests)
            {
                AssemblyDefinition assdef = LoadAssembly(manifest.AssemblyName);
                if (assdef == null)
                {
                    missingassemblies++;
                    foreach (var hook in manifest.Hooks)
                        hook.Flagged = true;
                }
                else
                {
                    foreach (var hook in manifest.Hooks)
                    {
                        var method = GetMethod(hook.AssemblyName, hook.TypeName, hook.Signature);
                        if (method == null)
                        {
                            missingmethods++;
                            hook.Flagged = true;
                        }
                        else
                        {
                            string hash = new Patching.ILWeaver(method.Body).Hash;
                            if (hash != hook.MSILHash)
                            {
                                changedmethods++;
                                hook.MSILHash = hash;
                                hook.Flagged = true;
                            }
                        }
                    }
                }
            }

            if (missingassemblies > 0 || missingmethods > 0 || changedmethods > 0)
            {
                CurrentProject.Save(CurrentProjectFilename);
                if (missingassemblies > 1)
                    MessageBox.Show(this, string.Format("{0} assemblies are missing from the target directory!", missingassemblies), "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (missingassemblies == 1)
                    MessageBox.Show(this, string.Format("{0} assembly is missing from the target directory!", missingassemblies), "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (missingmethods > 0)
                    MessageBox.Show(this, string.Format("{0} method(s) referenced by hooks no longer exist!", missingmethods), "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (changedmethods > 0)
                    MessageBox.Show(this, string.Format("{0} method(s) referenced by hooks have changed!", changedmethods), "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        

        #region Code Interface

        /// <summary>
        /// Opens the specified project
        /// </summary>
        /// <param name="filename"></param>
        public void OpenProject(string filename)
        {
            // Close current project
            CloseProject();

            // Open new project data
            CurrentProjectFilename = filename;
            CurrentProject = Project.Load(filename);
            resolver = new Patching.AssemblyResolver { TargetDirectory = CurrentProject.TargetDirectory };

            // Verify
            VerifyProject();

            // Populate tree
            PopulateInitialTree();

            // Enable the patch button
            patchtool.Enabled = true;
        }

        /// <summary>
        /// Closes the current project
        /// </summary>
        public void CloseProject()
        {
            // Clear the tree and tabs
            objectview.Nodes.Clear();
            tabview.Controls.Clear();

            // Set project to null
            CurrentProject = null;
            CurrentProjectFilename = null;
        }

        /// <summary>
        /// Opens or focuses the specified hook view
        /// </summary>
        /// <param name="hook"></param>
        public void GotoHook(Hook hook)
        {
            // Check if it's already open somewhere
            foreach (TabPage tabpage in tabview.TabPages)
            {
                HookViewControl control = tabpage.Tag as HookViewControl;
                if (control != null && control.Hook == hook)
                {
                    tabview.SelectedTab = tabpage;
                    return;
                }
            }

            // Create
            HookViewControl view = new HookViewControl();
            view.Hook = hook;
            view.MainForm = this;
            view.Dock = DockStyle.Fill;
            AddTab(hook.Name, view, view);
        }

        /// <summary>
        /// Adds a hook to the current project
        /// </summary>
        /// <param name="hook"></param>
        public void AddHook(Hook hook)
        {
            Manifest manifest = CurrentProject.GetManifest(hook.AssemblyName);
            manifest.Hooks.Add(hook);
            CurrentProject.Save(CurrentProjectFilename);

            TreeNode hooks = null;
            foreach (var node in objectview.Nodes)
                if ((node as TreeNode).Text == "Hooks")
                {
                    hooks = node as TreeNode;
                    break;
                }
            if (hooks == null) return;

            TreeNode hooknode = new TreeNode(hook.Name);
            if (hook.Flagged)
            {
                hooknode.ImageKey = "script_error.png";
                hooknode.SelectedImageKey = "script_error.png";
            }
            else
            {
                hooknode.ImageKey = "script_lightning.png";
                hooknode.SelectedImageKey = "script_lightning.png";
            }
            hooknode.Tag = hook;
            hooks.Nodes.Add(hooknode);
        }

        /// <summary>
        /// Removes a hook from the current project
        /// </summary>
        /// <param name="hook"></param>
        public void RemoveHook(Hook hook)
        {
            Manifest manifest = CurrentProject.GetManifest(hook.AssemblyName);
            manifest.Hooks.Remove(hook);
            CurrentProject.Save(CurrentProjectFilename);

            foreach (TabPage tabpage in tabview.TabPages)
            {
                if (tabpage.Tag is HookViewControl && (tabpage.Tag as HookViewControl).Hook == hook)
                {
                    tabview.TabPages.Remove(tabpage);
                    break;
                }
            }

            TreeNode hooks = null;
            foreach (var node in objectview.Nodes)
                if ((node as TreeNode).Text == "Hooks")
                {
                    hooks = node as TreeNode;
                    break;
                }
            if (hooks == null) return;

            foreach (var node in hooks.Nodes)
            {
                if ((node as TreeNode).Tag == hook)
                {
                    hooks.Nodes.Remove(node as TreeNode);
                    break;
                }
            }
        }

        /// <summary>
        /// Updates the UI for a hook
        /// </summary>
        /// <param name="hook"></param>
        public void UpdateHook(Hook hook)
        {
            foreach (TabPage tabpage in tabview.TabPages)
            {
                if (tabpage.Tag is HookViewControl && (tabpage.Tag as HookViewControl).Hook == hook)
                {
                    tabpage.Text = hook.Name;
                    if (hook.Flagged)
                    {
                        (tabpage.Tag as HookViewControl).UnflagButton.Enabled = true;
                        (tabpage.Tag as HookViewControl).FlagButton.Enabled = false;
                    }
                    else
                    {
                        (tabpage.Tag as HookViewControl).UnflagButton.Enabled = false;
                        (tabpage.Tag as HookViewControl).FlagButton.Enabled = true;
                    }
                }
            }

            CurrentProject.Save(CurrentProjectFilename);

            TreeNode hooks = null;
            foreach (var node in objectview.Nodes)
                if ((node as TreeNode).Text == "Hooks")
                {
                    hooks = node as TreeNode;
                    break;
                }
            if (hooks == null) return;

            foreach (var node in hooks.Nodes)
            {
                if ((node as TreeNode).Tag == hook)
                {
                    TreeNode treenode = node as TreeNode;

                    treenode.Text = hook.Name;
                    if (hook.Flagged)
                    {
                        treenode.ImageKey = "script_error.png";
                        treenode.SelectedImageKey = "script_error.png";
                    }
                    else
                    {
                        treenode.ImageKey = "script_lightning.png";
                        treenode.SelectedImageKey = "script_lightning.png";
                    }


                    break;
                }
            }
        }

        public void UpdateAllHooks()
        {
            if (CurrentProject != null)
            {
                foreach (var hook in CurrentProject.Manifests.SelectMany((m) => m.Hooks))
                {
                    UpdateHook(hook);
                }
            }
        }

        /// <summary>
        /// Gets the method associated with the specified signature
        /// </summary>
        /// <param name="hook"></param>
        /// <returns></returns>
        public MethodDefinition GetMethod(string assemblyname, string typename, MethodSignature signature)
        {
            AssemblyDefinition assdef;
            if (!assemblydict.TryGetValue(assemblyname, out assdef)) return null;

            try
            {
                var type = assdef.Modules
                    .SelectMany((m) => m.GetTypes())
                    .Single((t) => t.FullName == typename);

                return type.Methods
                    .Single((m) => Utility.GetMethodSignature(m).Equals(signature));
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        

        




    }

    public static class Extensions
    {
        /// <summary>
        /// Get the string slice between the two indexes.
        /// Inclusive for start index, exclusive for end index.
        /// </summary>
        public static string Slice(this string source, int start, int end)
        {
            if (end < 0) // Keep this for negative end support
            {
                end = source.Length + end;
            }
            int len = end - start;               // Calculate length
            return source.Substring(start, len); // Return Substring of length
        }
    }
}
