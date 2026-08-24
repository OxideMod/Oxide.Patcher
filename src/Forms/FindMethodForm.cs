using Mono.Cecil;
using Oxide.Patcher.Common;
using Oxide.Patcher.Hooks;
using Oxide.Patcher.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Oxide.Patcher
{
    /// <summary>
    /// Dialog that lets the user browse every type and method across the project's
    /// assemblies (or search for one by partial name) and picks the details needed
    /// to fill in a hook: assembly, type name, signature, and MSIL hash.
    /// </summary>
    public partial class FindMethodForm : Form
    {
        private readonly Project _project;
        private readonly AssemblyLoader _loader;
        private readonly Dictionary<string, List<TypeDefinition>> _typesByAssembly = new Dictionary<string, List<TypeDefinition>>();

        private int _searchGeneration;

        /// <summary>Gets the assembly name of the selected method once the dialog closes with DialogResult.OK</summary>
        public string ResultAssemblyName { get; private set; }

        /// <summary>Gets the fully qualified declaring type name of the selected method</summary>
        public string ResultTypeName { get; private set; }

        /// <summary>Gets the signature of the selected method</summary>
        public MethodSignature ResultSignature { get; private set; }

        /// <summary>Gets the MSIL hash of the selected method's body, if it has one</summary>
        public string ResultMsilHash { get; private set; }

        public FindMethodForm(Project project)
        {
            InitializeComponent();

            _project = project;
            _loader = new AssemblyLoader(project, string.Empty, deferLoading: true);

            PopulateAssemblyNodes();
        }

        private void PopulateAssemblyNodes()
        {
            methodtree.BeginUpdate();
            methodtree.Nodes.Clear();

            foreach (Manifest manifest in _project.Manifests)
            {
                AssemblyDefinition assembly = _loader.LoadAssembly(manifest.AssemblyName);
                if (assembly == null)
                {
                    continue;
                }

                List<TypeDefinition> types = assembly.Modules.SelectMany(m => m.GetTypes())
                                                              .Where(t => t.HasMethods)
                                                              .OrderBy(t => t.FullName)
                                                              .ToList();

                _typesByAssembly[manifest.AssemblyName] = types;

                TreeNode assemblyNode = new TreeNode(manifest.AssemblyName)
                {
                    Tag = manifest.AssemblyName
                };
                assemblyNode.Nodes.Add(new TreeNode("..."));
                methodtree.Nodes.Add(assemblyNode);
            }

            methodtree.EndUpdate();

            statuslabel.Text = _typesByAssembly.Count > 0
                ? "Expand an assembly to browse its types, or type above to search by name."
                : "No assemblies are loaded in this project.";
        }

        private void methodtree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;

            if (node.Tag is string assemblyName && node.Nodes.Count == 1 && node.Nodes[0].Text == "...")
            {
                PopulateTypeNodes(node, assemblyName);
            }
            else if (node.Tag is TypeDefinition type && node.Nodes.Count == 1 && node.Nodes[0].Text == "...")
            {
                PopulateMethodNodes(node, type);
            }
        }

        private void PopulateTypeNodes(TreeNode assemblyNode, string assemblyName)
        {
            assemblyNode.Nodes.Clear();

            if (!_typesByAssembly.TryGetValue(assemblyName, out List<TypeDefinition> types))
            {
                return;
            }

            foreach (TypeDefinition type in types)
            {
                TreeNode typeNode = new TreeNode(type.FullName) { Tag = type };
                typeNode.Nodes.Add(new TreeNode("..."));
                assemblyNode.Nodes.Add(typeNode);
            }
        }

        private void PopulateMethodNodes(TreeNode typeNode, TypeDefinition type)
        {
            typeNode.Nodes.Clear();

            foreach (MethodDefinition method in type.Methods.OrderBy(m => m.Name))
            {
                TreeNode methodNode = new TreeNode(Utility.GetMethodDeclaration(method)) { Tag = method };
                typeNode.Nodes.Add(methodNode);
            }

            if (typeNode.Nodes.Count == 0)
            {
                typeNode.Nodes.Add(new TreeNode("(no methods)"));
            }
        }

        private void methodtree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            okbutton.Enabled = e.Node?.Tag is MethodDefinition;
        }

        private void methodtree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node?.Tag is MethodDefinition)
            {
                methodtree.SelectedNode = e.Node;
                AcceptSelection();
            }
        }

        private async void searchbox_TextChanged(object sender, EventArgs e)
        {
            string query = searchbox.Text.Trim();
            int generation = ++_searchGeneration;

            if (query.Length == 0)
            {
                PopulateAssemblyNodes();
                return;
            }

            if (query.Length < 2)
            {
                statuslabel.Text = "Keep typing to search...";
                return;
            }

            statuslabel.Text = "Searching...";

            List<TreeNode> results = await System.Threading.Tasks.Task.Run(() => Search(query));

            if (generation != _searchGeneration)
            {
                // A newer search has since started; discard these results
                return;
            }

            methodtree.BeginUpdate();
            methodtree.Nodes.Clear();
            methodtree.Nodes.AddRange(results.ToArray());
            methodtree.EndUpdate();

            statuslabel.Text = results.Count > 0
                ? $"{results.Count} matching type{(results.Count == 1 ? string.Empty : "s")} found."
                : "No matches found.";
        }

        private List<TreeNode> Search(string query)
        {
            List<TreeNode> results = new List<TreeNode>();

            foreach (KeyValuePair<string, List<TypeDefinition>> pair in _typesByAssembly)
            {
                foreach (TypeDefinition type in pair.Value)
                {
                    bool typeMatches = type.FullName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

                    List<MethodDefinition> matchingMethods = type.Methods
                        .Where(m => typeMatches || m.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        .OrderBy(m => m.Name)
                        .ToList();

                    if (matchingMethods.Count == 0)
                    {
                        continue;
                    }

                    TreeNode typeNode = new TreeNode($"{pair.Key} :: {type.FullName}") { Tag = type };
                    foreach (MethodDefinition method in matchingMethods)
                    {
                        typeNode.Nodes.Add(new TreeNode(Utility.GetMethodDeclaration(method)) { Tag = method });
                    }

                    typeNode.Expand();
                    results.Add(typeNode);

                    if (results.Count >= 200)
                    {
                        return results;
                    }
                }
            }

            return results;
        }

        private void okbutton_Click(object sender, EventArgs e)
        {
            AcceptSelection();
        }

        private void AcceptSelection()
        {
            if (!(methodtree.SelectedNode?.Tag is MethodDefinition method))
            {
                return;
            }

            string assemblyName = _loader.rassemblydict.TryGetValue(method.Module.Assembly, out string name)
                ? name
                : _typesByAssembly.FirstOrDefault(p => p.Value.Contains(method.DeclaringType)).Key;

            if (assemblyName == null)
            {
                MessageBox.Show(this, "Could not determine which assembly this method belongs to.", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ResultAssemblyName = assemblyName;
            ResultTypeName = method.DeclaringType.FullName;
            ResultSignature = Utility.GetMethodSignature(method);
            ResultMsilHash = method.Body != null ? new ILWeaver(method.Body).Hash : null;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
