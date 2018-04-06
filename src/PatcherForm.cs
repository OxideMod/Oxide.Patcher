using Mono.Cecil;
using Oxide.Patcher.Deobfuscation;
using Oxide.Patcher.Fields;
using Oxide.Patcher.Hooks;
using Oxide.Patcher.Modifiers;
using Oxide.Patcher.Patching;
using Oxide.Patcher.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Oxide.Patcher
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
        internal Dictionary<AssemblyDefinition, string> rassemblydict;

        private IAssemblyResolver resolver;

        private Version version = Assembly.GetExecutingAssembly().GetName().Version;

        private MouseEventArgs mea;

        public static PatcherForm MainForm { get; private set; }

        private MRUManager mruManager;

        private int newCategoryCount;

        private TreeNode dragNode;

        private TreeNode tempDropNode;

        private TreeNode lastDragDestination;

        private DateTime lastDragDestinationTime;

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
            string title = string.Format(Text, version);
            Text = title.Slice(0, title.LastIndexOf("."));
            MainForm = this;
        }

        public PatcherForm(string filename)
        {
            InitializeComponent();
            string title = string.Format(Text, version);
            Text = title.Slice(0, title.LastIndexOf("."));
            if (File.Exists(filename))
            {
                CurrentProjectFilename = filename;
            }
            else
            {
                MessageBox.Show(filename + " does not exist!", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            MainForm = this;
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

            // Load MRU
            mruManager = new MRUManager(recentprojects, "Oxide.Patcher", 10, openrecentproject_Click);

            // Load settings
            // CurrentSettings = PatcherFormSettings.Load();
            // Location = CurrentSettings.FormPosition;
            // Size = CurrentSettings.FormSize;
            // WindowState = CurrentSettings.WindowState;

            assemblydict = new Dictionary<string, AssemblyDefinition>();
            rassemblydict = new Dictionary<AssemblyDefinition, string>();

            if (CurrentProjectFilename != null)
            {
                OpenProject(CurrentProjectFilename);
            }
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
            if (result == DialogResult.OK)
            {
                OpenProject(openprojectdialog.FileName);
            }
        }

        private void openrecentproject_Click(object sender, EventArgs e)
        {
            string file = (sender as ToolStripItem).Text;
            if (!File.Exists(file))
            {
                if (MessageBox.Show($"{file} doesn't exist. Do you want to remove it from the recent files list?", "File not found", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    mruManager.Remove(file);
                }

                return;
            }

            mruManager.AddOrUpdate(file);
            OpenProject(file);
        }

        /// <summary>
        /// Called when the new project menu item was clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newproject_Click(object sender, EventArgs e)
        {
            NewProjectForm form = new NewProjectForm { StartPosition = FormStartPosition.CenterParent };
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

        #endregion Menu Handlers

        #region Toolbar Handlers

        /// <summary>
        /// Called when the new project tool icon was clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newprojecttool_Click(object sender, EventArgs e)
        {
            NewProjectForm form = new NewProjectForm { StartPosition = FormStartPosition.CenterParent };
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
            if (result == DialogResult.OK)
            {
                OpenProject(openprojectdialog.FileName);
            }
        }

        /// <summary>
        /// Called when the patch tool icon was clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void patchtool_Click(object sender, EventArgs e)
        {
            PatchProcessForm patchprocess = new PatchProcessForm
            {
                StartPosition = FormStartPosition.CenterParent,
                PatchProject = CurrentProject
            };
            patchprocess.ShowDialog(this);
            UpdateAllHooks();
        }

        #endregion Toolbar Handlers

        #region Object View Handlers

        private void objectview_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode node = e.Node;
                if (node != null)
                {
                    objectview.SelectedNode = node;
                    string str = node.Tag as string ?? string.Empty;
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
                        if ((node.Tag as Hook).Flagged)
                        {
                            FlagMenuItem.Enabled = false;
                            UnflagMenuItem.Enabled = true;
                        }
                        else
                        {
                            FlagMenuItem.Enabled = true;
                            UnflagMenuItem.Enabled = false;
                        }
                        hooksmenu.Show(objectview, e.X, e.Y);
                    }
                    else if (str == "Hooks")
                    {
                        hookmenu.Show(objectview, e.X, e.Y);
                    }
                    else if (str == "Category")
                    {
                        categorymenu.Show(objectview, e.X, e.Y);
                    }
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
                    AddTab(typedef.FullName, classview, classview);
                }
            }
            else if (e.Node.Tag is Hook)
            {
                GotoHook(e.Node.Tag as Hook);
            }
            else if (e.Node.Tag is Modifier)
            {
                GotoModifier(e.Node.Tag as Modifier);
            }
            else if (e.Node.Tag is Field)
            {
                GotoField(e.Node.Tag as Field);
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

                string realfilename = Path.Combine(CurrentProject.TargetDirectory, data.AssemblyName);
                string origfilename = Path.Combine(CurrentProject.TargetDirectory, Path.GetFileNameWithoutExtension(data.AssemblyName) + "_Original" + Path.GetExtension(data.AssemblyName));
                if (!File.Exists(origfilename))
                {
                    CreateOriginal(realfilename, origfilename);
                }

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
                {
                    objectview.SelectedNode.Parent.Nodes.Remove(objectview.SelectedNode);
                }
                else
                {
                    objectview.SelectedNode.ImageKey = "cross.png";
                    objectview.SelectedNode.SelectedImageKey = "cross.png";
                    objectview.SelectedNode.Nodes.Clear();
                }
            }
        }

        private void flag_Click(object sender, EventArgs e)
        {
            if (CurrentProject != null)
            {
                Hook hook = objectview.SelectedNode.Tag as Hook;
                if (hook.Flagged == false)
                {
                    hook.Flagged = true;
                    UpdateHook(hook, false);
                }
            }
        }

        private void unflag_Click(object sender, EventArgs e)
        {
            if (CurrentProject != null)
            {
                Hook hook = objectview.SelectedNode.Tag as Hook;
                if (hook.Flagged)
                {
                    hook.Flagged = false;
                    UpdateHook(hook, false);
                }
            }
        }

        private void unflagall_Click(object sender, EventArgs e)
        {
            if (CurrentProject != null)
            {
                foreach (Hook hook in CurrentProject.Manifests.SelectMany(m => m.Hooks))
                {
                    if (hook.Flagged)
                    {
                        hook.Flagged = false;
                    }
                }
                UpdateAllHooks();
            }
        }

        private void flagall_Click(object sender, EventArgs e)
        {
            if (CurrentProject != null)
            {
                foreach (Hook hook in CurrentProject.Manifests.SelectMany(m => m.Hooks))
                {
                    if (hook.Flagged == false)
                    {
                        hook.Flagged = true;
                    }
                }
                UpdateAllHooks();
            }
        }

        private void FlagCategory_Click(object sender, EventArgs e)
        {
            if (CurrentProject != null)
            {
                foreach (TreeNode entry in objectview.SelectedNode.Nodes)
                {
                    Hook hook = entry.Tag as Hook;
                    if (!hook.Flagged)
                    {
                        hook.Flagged = true;
                        UpdateHook(hook, false);
                    }
                }
            }
        }

        private void UnflagCategory_Click(object sender, EventArgs e)
        {
            if (CurrentProject != null)
            {
                foreach (TreeNode entry in objectview.SelectedNode.Nodes)
                {
                    Hook hook = entry.Tag as Hook;
                    if (hook.Flagged)
                    {
                        hook.Flagged = false;
                        UpdateHook(hook, false);
                    }
                }
            }
        }

        private void addcategory_Click(object sender, EventArgs e)
        {
            TreeNode node = objectview.SelectedNode;
            if (node != null && (string)node.Tag == "Hooks")
            {
                TreeNode category = new TreeNode($"New Category {newCategoryCount++}")
                {
                    Tag = "Category",
                    ImageKey = "folder.png",
                    SelectedImageKey = "folder.png"
                };

                node.Nodes.Insert(0, category);
                objectview.LabelEdit = true;
                if (!node.Nodes[0].IsEditing)
                {
                    node.Nodes[0].BeginEdit();
                }
            }
        }

        private void renamecategory_Click(object sender, EventArgs e)
        {
            TreeNode node = objectview.SelectedNode;
            if (node == null || (string)node.Tag != "Category")
            {
                return;
            }

            objectview.LabelEdit = true;
            if (!node.IsEditing)
            {
                node.BeginEdit();
            }
        }

        private void objectview_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                if (e.Label.Length > 0)
                {
                    bool flag = CategoryExists(e.Label);
                    if (e.Label.IndexOfAny(new[] { '@', '.', ',', '!', '"' }) == -1 && !flag)
                    {
                        e.Node.EndEdit(false);
                        objectview.LabelEdit = false;
                        e.Node.Text = e.Label;
                        e.Node.Name = e.Label;

                        foreach (object node in e.Node.Nodes)
                        {
                            Hook hook = ((TreeNode)node).Tag as Hook;
                            if (hook == null)
                            {
                                continue;
                            }

                            hook.HookCategory = e.Label;
                            UpdateHook(hook, false);
                        }
                        objectview.BeginInvoke(new Action(() =>
                        {
                            Sort(objectview.Nodes["Hooks"].Nodes);
                            objectview.SelectedNode = objectview.Nodes["Hooks"].Nodes[e.Label];
                        }));
                    }
                    else if (flag)
                    {
                        e.CancelEdit = true;
                        MessageBox.Show("A category with this name already exists!", "Invalid Category",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Node.BeginEdit();
                    }
                    else
                    {
                        e.CancelEdit = true;
                        MessageBox.Show(
                            "Invalid category!\nThe category contains invalid characters!\nThese characters are: '@','.', ',', '!', '\"'",
                            "Invalid Category", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Node.BeginEdit();
                    }
                }
                else
                {
                    e.CancelEdit = true;
                    MessageBox.Show("Invalid category!\nThe category can't be empty!", "Invalid Category",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Node.BeginEdit();
                }
            }
            else
            {
                objectview.LabelEdit = false;
            }
        }

        private void removecategory_Click(object sender, EventArgs e)
        {
            TreeNode node = objectview.SelectedNode;
            if (node == null || (string)node.Tag != "Category")
            {
                return;
            }

            for (int i = node.Nodes.Count; i > 0; i--)
            {
                TreeNode child = node.Nodes[i - 1];
                node.Nodes.Remove(child);
                Hook hook = child.Tag as Hook;
                if (hook != null)
                {
                    hook.HookCategory = null;
                    UpdateHook(hook, false);
                }
                node.Parent.Nodes.Add(child);
            }
            Sort(node.Parent.Nodes, false);
            node.Remove();
        }

        private void objectview_MouseDown(object sender, MouseEventArgs e)
        {
            objectview.SelectedNode = objectview.GetNodeAt(e.X, e.Y);
        }

        private void objectview_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode node = e.Item as TreeNode;
            if (node?.Tag != null && !(node.Tag is Hook))
            {
                return;
            }

            dragNode = (TreeNode)e.Item;

            imagelistDragDrop.Images.Clear();
            imagelistDragDrop.ImageSize = new Size(dragNode.Bounds.Width + objectview.Indent, dragNode.Bounds.Height);

            Bitmap bmp = new Bitmap(dragNode.Bounds.Width + objectview.Indent + 5, dragNode.Bounds.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.CompositingQuality = CompositingQuality.HighQuality;
            gfx.DrawImage(imagelist.Images[dragNode.ImageKey], 0, 0);
            gfx.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            gfx.DrawString(dragNode.Text, objectview.Font, new SolidBrush(objectview.ForeColor), objectview.Indent, 0f);
            imagelistDragDrop.Images.Add(bmp);

            if (!DragDropHelper.ImageList_BeginDrag(imagelistDragDrop.Handle, 0, -8, 55))
            {
                return;
            }

            DoDragDrop(bmp, DragDropEffects.Move);
            DragDropHelper.ImageList_EndDrag();
        }

        private void objectview_DragOver(object sender, DragEventArgs e)
        {
            TreeNode targetNode = objectview.GetNodeAt(objectview.PointToClient(Cursor.Position));

            Point p = PointToClient(new Point(e.X, e.Y));
            DragDropHelper.ImageList_DragMove(p.X, p.Y);

            objectview.Scroll();

            e.Effect = DragDropEffects.None;

            if (targetNode == null)
            {
                return;
            }

            targetNode.EnsureVisible();
            if (targetNode != tempDropNode)
            {
                DragDropHelper.ImageList_DragShowNolock(false);
                objectview.SelectedNode = targetNode;
                DragDropHelper.ImageList_DragShowNolock(true);
                tempDropNode = targetNode;
            }

            string target = targetNode?.Tag as string;
            if (target == null || target != "Category")
            {
                return;
            }

            if (lastDragDestination != targetNode)
            {
                lastDragDestination = targetNode;
                lastDragDestinationTime = DateTime.Now;
            }
            else
            {
                TimeSpan hoverTime = DateTime.Now.Subtract(lastDragDestinationTime);
                if (!targetNode.IsExpanded && hoverTime.TotalSeconds >= 2)
                {
                    targetNode.Expand();
                }
            }

            e.Effect = DragDropEffects.Move;
        }

        private void objectview_DragDrop(object sender, DragEventArgs e)
        {
            DragDropHelper.ImageList_DragLeave(objectview.Handle);

            TreeNode targetNode = objectview.GetNodeAt(objectview.PointToClient(Cursor.Position));

            Hook hook = dragNode.Tag as Hook;
            if (hook == null)
            {
                return;
            }

            if (!targetNode.IsExpanded)
            {
                targetNode.Expand();
            }

            objectview.Nodes.Remove(dragNode);
            targetNode.Nodes.Add(dragNode);

            hook.HookCategory = targetNode.Text;
            UpdateHook(hook, false);
            Sort(targetNode.Nodes);
            objectview.SelectedNode = dragNode;
        }

        private void objectview_DragEnter(object sender, DragEventArgs e)
        {
            DragDropHelper.ImageList_DragEnter(objectview.Handle, e.X - objectview.Left, e.Y - objectview.Top);
        }

        private void objectview_DragLeave(object sender, EventArgs e)
        {
            DragDropHelper.ImageList_DragLeave(objectview.Handle);
        }

        #endregion Object View Handlers

        #region Tab View Handlers

        private void closetab_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < tabview.TabCount; ++i)
            {
                if (tabview.GetTabRect(i).Contains(mea.Location))
                {
                    (tabview.Controls[i] as TabPage).Dispose();
                }
            }
        }

        private void closeothertabs_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < tabview.TabCount; ++i)
            {
                if (tabview.GetTabRect(i).Contains(mea.Location))
                {
                    tabview.SelectedTab = tabview.Controls[i] as TabPage;
                }
            }
            while (tabview.Controls.Count > 1)
            {
                foreach (TabPage tab in tabview.Controls)
                {
                    if (tab != tabview.SelectedTab)
                    {
                        tab.Dispose();
                    }
                }
            }
        }

        private void tabview_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                mea = e;
                tabviewcontextmenu.Show(tabview, e.Location);
            }
        }

        #endregion Tab View Handlers

        internal AssemblyDefinition LoadAssembly(string name)
        {
            if (assemblydict.TryGetValue(name, out AssemblyDefinition assdef))
            {
                return assdef;
            }

            string file = $"{Path.GetFileNameWithoutExtension(name)}_Original{Path.GetExtension(name)}";
            string filename = Path.Combine(CurrentProject.TargetDirectory, file);
            if (!File.Exists(filename))
            {
                string oldfilename = Path.Combine(CurrentProject.TargetDirectory, name);
                if (!File.Exists(oldfilename))
                {
                    return null;
                }

                CreateOriginal(oldfilename, filename);
            }
            assdef = AssemblyDefinition.ReadAssembly(filename, new ReaderParameters { AssemblyResolver = resolver });
            assemblydict.Add(name, assdef);
            rassemblydict.Add(assdef, name);
            return assdef;
        }

        private void CreateOriginal(string oldfile, string newfile)
        {
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(oldfile, new ReaderParameters { AssemblyResolver = resolver });
            Deobfuscator deob = Deobfuscators.Find(assembly);
            if (deob != null)
            {
                DialogResult result = MessageBox.Show(this,
                    $"Assembly '{assembly.MainModule.Name}' appears to be obfuscated using '{deob.Name}', attempt to deobfuscate?", "Oxide Patcher", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Yes)
                {
                    // Deobfuscate
                    if (deob.Deobfuscate(assembly))
                    {
                        // Success
                        if (File.Exists(newfile))
                        {
                            File.Delete(newfile);
                        }

                        assembly.Write(newfile);
                        return;
                    }

                    MessageBox.Show(this, "Deobfuscation failed!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            File.Copy(oldfile, newfile);
        }

        private bool IsFileOriginal(string filename)
        {
            string name = Path.GetFileNameWithoutExtension(filename);
            const string postfix = "_Original";
            if (name.Length <= postfix.Length)
            {
                return false;
            }

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
            hooks.Name = "Hooks";
            hooks.SelectedImageKey = "lightning.png";
            hooks.Tag = "Hooks";
            objectview.Nodes.Add(hooks);

            foreach (Hook hook in CurrentProject.Manifests.SelectMany(m => m.Hooks))
            {
                TreeNode category = new TreeNode(hook.HookCategory);
                if (hook.HookCategory != null)
                {
                    category.ImageKey = "folder.png";
                    category.Name = hook.HookCategory;
                    category.SelectedImageKey = "folder.png";
                    category.Tag = "Category";
                    if (!hooks.Nodes.ContainsKey(hook.HookCategory))
                    {
                        hooks.Nodes.Add(category);
                    }
                    else
                    {
                        category = hooks.Nodes.Find(hook.HookCategory, true)[0];
                    }
                }

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

                if (hook.HookCategory == null)
                {
                    hooks.Nodes.Add(hooknode);
                }
                else
                {
                    category.Nodes.Add(hooknode);
                    if (!hook.Flagged)
                    {
                        continue;
                    }

                    category.ImageKey = "folder_flagged.png";
                    category.SelectedImageKey = "folder_flagged.png";
                }
            }

            // Add modifiers
            TreeNode modifiers = new TreeNode("Modifiers");
            modifiers.ImageKey = "lightning.png";
            modifiers.Name = "Modifiers";
            modifiers.SelectedImageKey = "lightning.png";
            modifiers.Tag = "Modifiers";
            objectview.Nodes.Add(modifiers);

            foreach (Modifier modifier in CurrentProject.Manifests.SelectMany(m => m.Modifiers))
            {
                TreeNode modifiernode = new TreeNode(modifier.Name);
                if (modifier.Flagged)
                {
                    modifiernode.ImageKey = "script_error.png";
                    modifiernode.SelectedImageKey = "script_error.png";
                }
                else
                {
                    modifiernode.ImageKey = "script_lightning.png";
                    modifiernode.SelectedImageKey = "script_lightning.png";
                }

                modifiernode.Tag = modifier;
                modifiers.Nodes.Add(modifiernode);
            }

            // Add fields
            TreeNode fields = new TreeNode("Fields");
            fields.ImageKey = "lightning.png";
            fields.Name = "Fields";
            fields.SelectedImageKey = "lightning.png";
            fields.Tag = "Fields";
            objectview.Nodes.Add(fields);

            foreach (Field field in CurrentProject.Manifests.SelectMany(m => m.Fields))
            {
                TreeNode fieldnode = new TreeNode($"{field.TypeName}::{field.Name}");
                if (field.Flagged)
                {
                    fieldnode.ImageKey = "script_error.png";
                    fieldnode.SelectedImageKey = "script_error.png";
                }
                else
                {
                    fieldnode.ImageKey = "script_lightning.png";
                    fieldnode.SelectedImageKey = "script_lightning.png";
                }

                fieldnode.Tag = field;
                fields.Nodes.Add(fieldnode);
            }

            // Add assemblies
            TreeNode assemblies = new TreeNode("Assemblies");
            assemblies.ImageKey = "folder.png";
            assemblies.SelectedImageKey = "folder.png";
            objectview.Nodes.Add(assemblies);

            List<TreeNode> assemblynodes = new List<TreeNode>();
            IEnumerable<string> files = Directory.GetFiles(CurrentProject.TargetDirectory).Where(f => f.EndsWith(".dll") || f.EndsWith(".exe") && !Path.GetFileName(f).StartsWith(typeof(Program).Assembly.GetName().Name));
            foreach (string file in files)
            {
                // Check if it's an original dll
                if (!IsFileOriginal(file))
                {
                    // See if it has a manifest
                    string assemblyname = Path.GetFileNameWithoutExtension(file);
                    string assemblyfile = Path.GetFileName(file);
                    if (CurrentProject.Manifests.Any(x => x.AssemblyName == assemblyfile))
                    {
                        // Get the manifest
                        // Manifest manifest = CurrentProject.Manifests.Single((x) => x.AssemblyName == assemblyname);

                        // Load the assembly
                        NodeAssemblyData data = new NodeAssemblyData();
                        data.Included = true;
                        data.AssemblyName = assemblyfile;
                        data.Loaded = true;
                        data.Definition = LoadAssembly(assemblyfile);

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
                        {
                            PopulateAssemblyNode(assembly, data.Definition);
                        }
                    }
                    else
                    {
                        // Nope, just make an empty node for it
                        TreeNode assembly = new TreeNode(assemblyname);
                        assembly.ImageKey = "cross.png";
                        assembly.SelectedImageKey = "cross.png";
                        assembly.Tag = new NodeAssemblyData { Included = false, AssemblyName = assemblyfile };
                        assemblynodes.Add(assembly);
                    }
                }
            }

            // Sort
            assemblynodes.Sort((a, b) =>
            {
                return Comparer<string>.Default.Compare(a.ImageKey, b.ImageKey);
            });

            // Sort Hooks and Modifiers
            Sort(objectview.Nodes["Hooks"].Nodes);
            Sort(objectview.Nodes["Modifiers"].Nodes);
            Sort(objectview.Nodes["Fields"].Nodes);

            // Add
            for (int i = 0; i < assemblynodes.Count; i++)
            {
                assemblies.Nodes.Add(assemblynodes[i]);
            }

            // Set status
            statuslabel.Text = "";
        }

        private sealed class NamespaceData
        {
            public string Name { get; }
            public List<NamespaceData> ChildNamespaces { get; }
            public List<TypeDefinition> ChildTypes { get; }
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
            HashSet<TypeDefinition> alltypes = new HashSet<TypeDefinition>(definition.Modules.SelectMany(x => x.GetTypes()));

            // Sort types into their namespaces
            Dictionary<string, NamespaceData> namespaces = new Dictionary<string, NamespaceData>();
            NamespaceData globalnamespace = new NamespaceData("");
            namespaces.Add("", globalnamespace);
            foreach (TypeDefinition typedef in alltypes)
            {
                if (!namespaces.TryGetValue(typedef.Namespace, out NamespaceData nspcdata))
                {
                    nspcdata = new NamespaceData(typedef.Namespace);
                    namespaces.Add(nspcdata.Name, nspcdata);
                }
                if (typedef.Namespace == "")
                {
                    globalnamespace = nspcdata;
                }

                nspcdata.ChildTypes.Add(typedef);
            }

            // Setup namespace hierarchy
            bool done = false;
            while (!done)
            {
                done = true;
                foreach (KeyValuePair<string, NamespaceData> pair in namespaces)
                {
                    if (pair.Value.Parent == null && pair.Value != globalnamespace)
                    {
                        if (pair.Key.Contains('.'))
                        {
                            string[] spl = pair.Key.Split('.');
                            string[] splm = new string[spl.Length - 1];
                            Array.Copy(spl, splm, splm.Length);
                            string parent = string.Concat(splm);
                            if (!namespaces.TryGetValue(parent, out NamespaceData parentnamespace))
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
            {
                typeparent = root;
            }

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
                {
                    return "Class-Private_493.png";
                }

                if (typedef.IsSealed)
                {
                    return "Class-Sealed_490.png";
                }

                return "Class_489.png";
            }

            if (typedef.IsInterface)
            {
                if (!typedef.IsPublic)
                {
                    return "Interface-Private_616.png";
                }

                if (typedef.IsSealed)
                {
                    return "Interface-Sealed_613.png";
                }

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

        private void VerifyProject()
        {
            // Step 1: Check all included assemblies are intact
            // Step 2: Check all hooks are intact
            // Step 3: Check all modifiers are intact
            int missingassemblies = 0, missingmethods = 0, changedmethods = 0, changedfields = 0, changedmodmethods = 0, changedproperties = 0, changednewfields = 0;
            foreach (Manifest manifest in CurrentProject.Manifests)
            {
                AssemblyDefinition assdef = LoadAssembly(manifest.AssemblyName);
                if (assdef == null)
                {
                    missingassemblies++;
                    foreach (Hook hook in manifest.Hooks)
                    {
                        hook.Flagged = true;
                    }
                }
                else
                {
                    foreach (Hook hook in manifest.Hooks)
                    {
                        MethodDefinition method = GetMethod(hook.AssemblyName, hook.TypeName, hook.Signature);
                        if (method == null)
                        {
                            missingmethods++;
                            hook.Flagged = true;
                        }
                        else
                        {
                            string hash = new ILWeaver(method.Body).Hash;
                            if (hash != hook.MSILHash)
                            {
                                changedmethods++;
                                hook.MSILHash = hash;
                                hook.Flagged = true;
                            }
                        }
                    }

                    foreach (Modifier modifier in manifest.Modifiers)
                    {
                        switch (modifier.Type)
                        {
                            case ModifierType.Field:
                                FieldDefinition fielddef = GetField(modifier.AssemblyName, modifier.TypeName, modifier.Name, modifier.Signature);
                                if (fielddef == null)
                                {
                                    changedfields++;
                                    modifier.Flagged = true;
                                }
                                break;

                            case ModifierType.Method:
                                MethodDefinition methoddef = GetMethod(modifier.AssemblyName, modifier.TypeName, modifier.Signature);
                                if (methoddef == null)
                                {
                                    changedmodmethods++;
                                    modifier.Flagged = true;
                                }
                                break;

                            case ModifierType.Property:
                                PropertyDefinition propertydef = GetProperty(modifier.AssemblyName, modifier.TypeName, modifier.Name, modifier.Signature);
                                if (propertydef == null)
                                {
                                    changedproperties++;
                                    modifier.Flagged = true;
                                }
                                break;
                        }
                    }

                    foreach (Field field in manifest.Fields)
                    {
                        if (field.IsValid())
                        {
                            continue;
                        }

                        changednewfields++;
                        field.Flagged = true;
                    }
                }
            }

            if (missingassemblies > 0 || missingmethods > 0 || changedmethods > 0 || changedfields > 0 || changedmodmethods > 0 || changedproperties > 0 || changednewfields > 0)
            {
                CurrentProject.Save(CurrentProjectFilename);
                if (missingassemblies > 1)
                {
                    MessageBox.Show(this, $"{missingassemblies} assemblies are missing from the target directory!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (missingassemblies == 1)
                {
                    MessageBox.Show(this, $"{missingassemblies} assembly is missing from the target directory!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (missingmethods > 1)
                {
                    MessageBox.Show(this, $"{missingmethods} methods referenced by hooks no longer exist!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (missingmethods == 1)
                {
                    MessageBox.Show(this, $"{missingmethods} method referenced by hooks no longer exists!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (changedmethods > 1)
                {
                    MessageBox.Show(this, $"{changedmethods} methods referenced by hooks have changed!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (changedmethods == 1)
                {
                    MessageBox.Show(this, $"{changedmethods} method referenced by hooks has changed!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (changedfields > 1)
                {
                    MessageBox.Show(this, $"{changedfields} fields with altered modifiers have changed!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (changedfields == 1)
                {
                    MessageBox.Show(this, $"{changedfields} field with altered modifiers has changed!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (changedmodmethods > 1)
                {
                    MessageBox.Show(this, $"{changedmodmethods} methods with altered modifiers have changed!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (changedmodmethods == 1)
                {
                    MessageBox.Show(this, $"{changedmodmethods} method with altered modifiers has changed!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (changedproperties > 1)
                {
                    MessageBox.Show(this, $"{changedproperties} properties with altered modifiers have changed!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (changedproperties == 1)
                {
                    MessageBox.Show(this, $"{changedproperties} property with altered modifiers has changed!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (changednewfields > 1)
                {
                    MessageBox.Show(this, $"{changednewfields} new fields were flagged!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (changednewfields == 1)
                {
                    MessageBox.Show(this, $"{changednewfields} new field was flagged!", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool CategoryExists(string label)
        {
            return objectview.Nodes["Hooks"].Nodes.Cast<TreeNode>().Any(node => node.Text == label);
        }

        private void Sort(TreeNodeCollection nodes, bool subNodes = true)
        {
            bool sorting = true;
            while (sorting)
            {
                sorting = false;
                int i;
                for (i = 1; i < nodes.Count; i++)
                {
                    if (subNodes)
                    {
                        if (nodes[i - 1].Nodes.Count > 0)
                        {
                            Sort(nodes[i - 1].Nodes);
                        }

                        if (nodes[i].Nodes.Count > 0)
                        {
                            Sort(nodes[i].Nodes);
                        }
                    }

                    if (CompareTreeNodes(nodes[i], nodes[i - 1]) >= 0)
                    {
                        continue;
                    }

                    SwapTreeNodes(nodes, i, i - 1);
                    sorting = true;
                }

                if (i == 1 && nodes.Count == 1 && subNodes)
                {
                    if (nodes[0].Nodes.Count > 0)
                    {
                        Sort(nodes[0].Nodes);
                    }
                }
            }
        }

        private int CompareTreeNodes(TreeNode a, TreeNode b)
        {
            if (a.Tag.GetType().BaseType == b.Tag.GetType().BaseType)
            {
                return string.CompareOrdinal(a.Text, b.Text);
            }

            return a.Tag is string ? -1 : 1;
        }

        private void SwapTreeNodes(TreeNodeCollection collection, int a, int b)
        {
            TreeNode aNode = collection[a];
            TreeNode bNode = collection[b];
            collection.Remove(aNode);
            collection.Remove(bNode);
            collection.Insert(a, bNode);
            collection.Insert(b, aNode);
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
            if (CurrentProject == null)
            {
                return;
            }
            if (!Directory.Exists(CurrentProject.TargetDirectory))
            {
                statuslabel.Text = "Target Directory specified in project file does not exist!";
                statuslabel.Invalidate();
                MessageBox.Show(this, CurrentProject.TargetDirectory + " does not exist!", "Directory Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Add project settings
                TreeNode projectsettings = new TreeNode("Project Settings");
                projectsettings.ImageKey = "cog_edit.png";
                projectsettings.SelectedImageKey = "cog_edit.png";
                projectsettings.Tag = "Project Settings";
                objectview.Nodes.Add(projectsettings);
                return;
            }
            resolver = new AssemblyResolver { TargetDirectory = CurrentProject.TargetDirectory };

            // Verify
            VerifyProject();

            // Populate tree
            PopulateInitialTree();

            // Enable the patch button
            patchtool.Enabled = true;

            // Add the file to the MRU
            mruManager.AddOrUpdate(filename);
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

            // Clear the assembly dictionary
            assemblydict.Clear();
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
        /// Opens or focuses the specified modifier view
        /// </summary>
        /// <param name="modifier"></param>
        public void GotoModifier(Modifier modifier)
        {
            // Check if it's already open somewhere
            foreach (TabPage tabpage in tabview.TabPages)
            {
                ModifierViewControl control = tabpage.Tag as ModifierViewControl;
                if (control != null && control.Modifier == modifier)
                {
                    tabview.SelectedTab = tabpage;
                    return;
                }
            }

            // Create
            ModifierViewControl view = new ModifierViewControl();
            view.Modifier = modifier;
            view.MainForm = this;
            view.Dock = DockStyle.Fill;
            AddTab(modifier.Name, view, view);
        }

        /// <summary>
        /// Opens or focuses the specified field view
        /// </summary>
        /// <param name="field"></param>
        public void GotoField(Field field)
        {
            // Check if it's already open somewhere
            foreach (TabPage tabpage in tabview.TabPages)
            {
                FieldViewControl control = tabpage.Tag as FieldViewControl;
                if (control != null && control.Field == field)
                {
                    tabview.SelectedTab = tabpage;
                    return;
                }
            }

            // Create
            FieldViewControl view = new FieldViewControl();
            view.Field = field;
            view.MainForm = this;
            view.Dock = DockStyle.Fill;
            AddTab($"{field.TypeName}::{field.Name}", view, view);
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
            foreach (object node in objectview.Nodes)
            {
                if ((node as TreeNode).Text == "Hooks")
                {
                    hooks = node as TreeNode;
                    break;
                }
            }

            if (hooks == null)
            {
                return;
            }

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
        /// Adds a modifier to the current project
        /// </summary>
        /// <param name="modifier"></param>
        public void AddModifier(Modifier modifier)
        {
            Manifest manifest = CurrentProject.GetManifest(modifier.AssemblyName);
            manifest.Modifiers.Add(modifier);
            CurrentProject.Save(CurrentProjectFilename);

            TreeNode modifiers = null;
            foreach (object node in objectview.Nodes)
            {
                if ((node as TreeNode).Text == "Modifiers")
                {
                    modifiers = node as TreeNode;
                    break;
                }
            }

            if (modifiers == null)
            {
                return;
            }

            TreeNode modifiernode = new TreeNode(modifier.Name);
            if (modifier.Flagged)
            {
                modifiernode.ImageKey = "script_error.png";
                modifiernode.SelectedImageKey = "script_error.png";
            }
            else
            {
                modifiernode.ImageKey = "script_lightning.png";
                modifiernode.SelectedImageKey = "script_lightning.png";
            }
            modifiernode.Tag = modifier;
            modifiers.Nodes.Add(modifiernode);
            Sort(modifiernode.Nodes);
        }

        /// <summary>
        /// Adds a modifier to the current project
        /// </summary>
        /// <param name="field"></param>
        public void AddField(Field field)
        {
            Manifest manifest = CurrentProject.GetManifest(field.AssemblyName);
            manifest.Fields.Add(field);
            CurrentProject.Save(CurrentProjectFilename);

            TreeNode fields = null;
            foreach (object node in objectview.Nodes)
            {
                if ((node as TreeNode).Text == "Fields")
                {
                    fields = node as TreeNode;
                    break;
                }
            }

            if (fields == null)
            {
                return;
            }

            TreeNode fieldnode = new TreeNode($"{field.TypeName}::{field.Name}");
            if (field.Flagged)
            {
                fieldnode.ImageKey = "script_error.png";
                fieldnode.SelectedImageKey = "script_error.png";
            }
            else
            {
                fieldnode.ImageKey = "script_lightning.png";
                fieldnode.SelectedImageKey = "script_lightning.png";
            }
            fieldnode.Tag = field;
            fields.Nodes.Add(fieldnode);
            Sort(fieldnode.Nodes);
        }

        /// <summary>
        /// Removes a hook from the current project
        /// </summary>
        /// <param name="hook"></param>
        public void RemoveHook(Hook hook)
        {
            Manifest manifest = CurrentProject.GetManifest(hook.AssemblyName);
            manifest.Hooks.Remove(hook);
            Dictionary<Hook, Hook> cloneHooks = manifest.Hooks.Where(h => h.BaseHook != null).ToDictionary(h => h.BaseHook);
            if (cloneHooks.ContainsKey(hook))
            {
                cloneHooks[hook].BaseHook = null;
                cloneHooks[hook].Flagged = true;
                UpdateHook(cloneHooks[hook], false);
            }
            CurrentProject.Save(CurrentProjectFilename);

            foreach (TabPage tabpage in tabview.TabPages)
            {
                if (tabpage.Tag is HookViewControl && (tabpage.Tag as HookViewControl).Hook == hook)
                {
                    tabview.TabPages.Remove(tabpage);
                    break;
                }
            }

            foreach (TreeNode node in objectview.Nodes["Hooks"].Nodes)
            {
                if (node.Tag == hook)
                {
                    node.Remove();
                    break;
                }

                string tag = node.Tag as string;
                if (string.IsNullOrEmpty(tag))
                {
                    continue;
                }

                if (tag != "Category")
                {
                    continue;
                }

                foreach (TreeNode subnode in node.Nodes)
                {
                    if (subnode.Tag != hook)
                    {
                        continue;
                    }

                    subnode.Remove();
                    break;
                }
            }
        }

        /// <summary>
        /// Removes a modifier from the current project
        /// </summary>
        /// <param name="modifier"></param>
        public void RemoveModifier(Modifier modifier)
        {
            Manifest manifest = CurrentProject.GetManifest(modifier.AssemblyName);
            manifest.Modifiers.Remove(modifier);
            CurrentProject.Save(CurrentProjectFilename);

            foreach (TabPage tabpage in tabview.TabPages)
            {
                if (tabpage.Tag is ModifierViewControl && (tabpage.Tag as ModifierViewControl).Modifier == modifier)
                {
                    tabview.TabPages.Remove(tabpage);
                    break;
                }
            }

            foreach (TreeNode node in objectview.Nodes["Modifiers"].Nodes)
            {
                if (node.Tag == modifier)
                {
                    node.Remove();
                    break;
                }
            }
        }

        /// <summary>
        /// Removes a field from the current project
        /// </summary>
        /// <param name="field"></param>
        public void RemoveField(Field field)
        {
            Manifest manifest = CurrentProject.GetManifest(field.AssemblyName);
            manifest.Fields.Remove(field);
            CurrentProject.Save(CurrentProjectFilename);

            foreach (TabPage tabpage in tabview.TabPages)
            {
                if (tabpage.Tag is FieldViewControl && (tabpage.Tag as FieldViewControl).Field == field)
                {
                    tabview.TabPages.Remove(tabpage);
                    break;
                }
            }

            foreach (TreeNode node in objectview.Nodes["Fields"].Nodes)
            {
                if (node.Tag == field)
                {
                    node.Remove();
                    break;
                }
            }
        }

        /// <summary>
        /// Updates the UI for a hook
        /// </summary>
        /// <param name="hook"></param>
        /// <param name="batchUpdate"></param>
        public void UpdateHook(Hook hook, bool batchUpdate)
        {
            Manifest manifest = CurrentProject.GetManifest(hook.AssemblyName);
            Dictionary<Hook, Hook> cloneHooks = manifest.Hooks.Where(h => h.BaseHook != null).ToDictionary(h => h.BaseHook);

            if (cloneHooks.ContainsKey(hook) && hook.Flagged)
            {
                cloneHooks[hook].Flagged = true;
                UpdateHook(cloneHooks[hook], false);
            }

            if (hook.BaseHook != null)
            {
                if (hook.BaseHook.Flagged && !hook.Flagged)
                {
                    hook.Flagged = true;
                    MessageBox.Show($"Can't unflag {hook.Name} because its base hook {hook.BaseHook.Name} is flagged", "Cannot unflag", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

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
            if (!batchUpdate)
            {
                CurrentProject.Save(CurrentProjectFilename);
            }

            TreeNode hooks = null;
            foreach (object node in objectview.Nodes)
            {
                if ((node as TreeNode).Text == "Hooks")
                {
                    hooks = node as TreeNode;
                    break;
                }
            }

            if (hooks == null)
            {
                return;
            }

            foreach (object node in hooks.Nodes)
            {
                string tag = (node as TreeNode).Tag as string;
                if (tag != null && tag == "Category")
                {
                    TreeNode category = node as TreeNode;
                    bool flagged = false;
                    foreach (object subnode in category.Nodes)
                    {
                        if ((subnode as TreeNode).Tag == hook)
                        {
                            TreeNode treenode = subnode as TreeNode;

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
                        }
                        if (((subnode as TreeNode).Tag as Hook).Flagged)
                        {
                            flagged = true;
                        }
                    }

                    if (flagged)
                    {
                        category.ImageKey = "folder_flagged.png";
                        category.SelectedImageKey = "folder_flagged.png";
                    }
                    else
                    {
                        category.ImageKey = "folder.png";
                        category.SelectedImageKey = "folder.png";
                    }

                    Sort(category.Nodes);
                }
                else if ((node as TreeNode).Tag == hook)
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
                    Sort(hooks.Nodes);
                    break;
                }
            }
        }

        /// <summary>
        /// Updates the UI for a modifier
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="batchUpdate"></param>
        public void UpdateModifier(Modifier modifier, bool batchUpdate)
        {
            foreach (TabPage tabpage in tabview.TabPages)
            {
                if (tabpage.Tag is ModifierViewControl && (tabpage.Tag as ModifierViewControl).Modifier == modifier)
                {
                    tabpage.Text = modifier.Name;
                    if (modifier.Flagged)
                    {
                        (tabpage.Tag as ModifierViewControl).UnflagButton.Enabled = true;
                        (tabpage.Tag as ModifierViewControl).FlagButton.Enabled = false;
                    }
                    else
                    {
                        (tabpage.Tag as ModifierViewControl).UnflagButton.Enabled = false;
                        (tabpage.Tag as ModifierViewControl).FlagButton.Enabled = true;
                    }
                }
            }
            if (!batchUpdate)
            {
                CurrentProject.Save(CurrentProjectFilename);
            }

            TreeNode modifiers = null;
            foreach (object node in objectview.Nodes)
            {
                if ((node as TreeNode).Text == "Modifiers")
                {
                    modifiers = node as TreeNode;
                    break;
                }
            }

            if (modifiers == null)
            {
                return;
            }

            foreach (object node in modifiers.Nodes)
            {
                if ((node as TreeNode).Tag == modifier)
                {
                    TreeNode treenode = node as TreeNode;

                    treenode.Text = modifier.Name;
                    if (modifier.Flagged)
                    {
                        treenode.ImageKey = "script_error.png";
                        treenode.SelectedImageKey = "script_error.png";
                    }
                    else
                    {
                        treenode.ImageKey = "script_lightning.png";
                        treenode.SelectedImageKey = "script_lightning.png";
                    }
                    Sort(modifiers.Nodes);
                    break;
                }
            }
        }

        /// <summary>
        /// Updates the UI for a modifier
        /// </summary>
        /// <param name="field"></param>
        /// <param name="batchUpdate"></param>
        public void UpdateField(Field field, bool batchUpdate)
        {
            foreach (TabPage tabpage in tabview.TabPages)
            {
                if (tabpage.Tag is FieldViewControl && (tabpage.Tag as FieldViewControl).Field == field)
                {
                    tabpage.Text = $"{field.TypeName}::{field.Name}";
                    if (field.Flagged)
                    {
                        (tabpage.Tag as FieldViewControl).UnflagButton.Enabled = true;
                        (tabpage.Tag as FieldViewControl).FlagButton.Enabled = false;
                    }
                    else
                    {
                        (tabpage.Tag as FieldViewControl).UnflagButton.Enabled = false;
                        (tabpage.Tag as FieldViewControl).FlagButton.Enabled = true;
                    }
                }
            }

            if (!batchUpdate)
            {
                CurrentProject.Save(CurrentProjectFilename);
            }

            TreeNode fields = null;
            foreach (object node in objectview.Nodes)
            {
                if ((node as TreeNode).Text == "Fields")
                {
                    fields = node as TreeNode;
                    break;
                }
            }

            if (fields == null)
            {
                return;
            }

            foreach (object node in fields.Nodes)
            {
                if ((node as TreeNode).Tag == field)
                {
                    TreeNode treenode = node as TreeNode;

                    treenode.Text = $"{field.TypeName}::{field.Name}";
                    if (field.Flagged)
                    {
                        treenode.ImageKey = "script_error.png";
                        treenode.SelectedImageKey = "script_error.png";
                    }
                    else
                    {
                        treenode.ImageKey = "script_lightning.png";
                        treenode.SelectedImageKey = "script_lightning.png";
                    }
                    Sort(fields.Nodes);
                    break;
                }
            }
        }

        public void UpdateAllHooks()
        {
            if (CurrentProject != null)
            {
                foreach (Hook hook in CurrentProject.Manifests.SelectMany(m => m.Hooks))
                {
                    UpdateHook(hook, true);
                }
                CurrentProject.Save(CurrentProjectFilename);
            }
        }

        /// <summary>
        /// Gets the method associated with the specified signature
        /// </summary>
        /// <param name="assemblyname"></param>
        /// <param name="typename"></param>
        /// <param name="signature"></param>
        public MethodDefinition GetMethod(string assemblyname, string typename, MethodSignature signature)
        {
            if (!assemblydict.TryGetValue(assemblyname, out AssemblyDefinition assdef))
            {
                return null;
            }

            try
            {
                TypeDefinition type = assdef.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == typename);

                return type.Methods.Single(m => Utility.GetMethodSignature(m).Equals(signature));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the method associated with the specified signature
        /// </summary>
        /// <param name="assemblyname"></param>
        /// <param name="typename"></param>
        /// <param name="signature"></param>
        public MethodDefinition GetMethod(string assemblyname, string typename, ModifierSignature signature)
        {
            if (!assemblydict.TryGetValue(assemblyname, out AssemblyDefinition assdef))
            {
                return null;
            }

            try
            {
                TypeDefinition type = assdef.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == typename);

                return type.Methods.Single(m => Utility.GetModifierSignature(m).Equals(signature));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the field associated with the specified signature
        /// </summary>
        /// <param name="assemblyname"></param>
        /// <param name="typename"></param>
        /// <param name="name"></param>
        /// <param name="signature"></param>
        public FieldDefinition GetField(string assemblyname, string typename, string name, ModifierSignature signature)
        {
            if (!assemblydict.TryGetValue(assemblyname, out AssemblyDefinition assdef))
            {
                return null;
            }

            try
            {
                TypeDefinition type = assdef.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == typename);

                return type.Fields.Single(m => Utility.GetModifierSignature(m).Equals(signature));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the property associated with the specified signature
        /// </summary>
        /// <param name="assemblyname"></param>
        /// <param name="typename"></param>
        /// <param name="name"></param>
        /// <param name="signature"></param>
        public PropertyDefinition GetProperty(string assemblyname, string typename, string name, ModifierSignature signature)
        {
            if (!assemblydict.TryGetValue(assemblyname, out AssemblyDefinition assdef))
            {
                return null;
            }

            try
            {
                TypeDefinition type = assdef.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == typename);

                return type.Properties.Single(m => Utility.GetModifierSignature(m).Equals(signature));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the type associated with the specified signature
        /// </summary>
        /// <param name="assemblyname"></param>
        /// <param name="typename"></param>
        public TypeDefinition GetType(string assemblyname, string typename)
        {
            if (!assemblydict.TryGetValue(assemblyname, out AssemblyDefinition assdef))
            {
                return null;
            }

            try
            {
                return assdef.Modules.SelectMany(m => m.GetTypes()).Single(t => t.FullName == typename);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion Code Interface
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
            int len = end - start; // Calculate length
            return source.Substring(start, len); // Return Substring of length
        }
    }

    public class DragDropHelper
    {
        [DllImport("comctl32.dll")]
        public static extern bool InitCommonControls();

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern bool ImageList_BeginDrag(IntPtr hWnd, int iTrack, int dxHotspot, int dyHotspot);

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern bool ImageList_DragMove(int x, int y);

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern void ImageList_EndDrag();

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern bool ImageList_DragEnter(IntPtr hWnd, int x, int y);

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern bool ImageList_DragLeave(IntPtr hWnd);

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern bool ImageList_DragShowNolock(bool fShow);

        static DragDropHelper()
        {
            InitCommonControls();
        }
    }

    public static class AutoScroll
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static void Scroll(this Control control)
        {
            Point pt = control.PointToClient(Cursor.Position);
            if (pt.Y + 20 > control.Height)
            {
                DragDropHelper.ImageList_DragShowNolock(false);
                SendMessage(control.Handle, 277, (IntPtr)1, (IntPtr)0);
                DragDropHelper.ImageList_DragShowNolock(true);
            }
            else if (pt.Y < 20)
            {
                DragDropHelper.ImageList_DragShowNolock(false);
                SendMessage(control.Handle, 277, (IntPtr)0, (IntPtr)0);
                DragDropHelper.ImageList_DragShowNolock(true);
            }
        }
    }
}
