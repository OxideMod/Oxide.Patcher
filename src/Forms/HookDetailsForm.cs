using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Oxide.Patcher.Hooks;

namespace Oxide.Patcher
{
    /// <summary>
    /// Dialog used both to create a brand new hook and to edit every detail of an
    /// existing hook - assembly, type, signature, category, hook type, and every
    /// type-specific setting - without needing to hand-edit the .opj project file.
    /// </summary>
    public partial class HookDetailsForm : Form
    {
        private readonly Project _project;
        private readonly Hook _existingHook;

        /// <summary>
        /// Gets the hook that was created or updated once the dialog closes with DialogResult.OK.
        /// In edit mode, this is the same object passed in unless the hook type was changed, in
        /// which case it is a newly created hook of the new type.
        /// </summary>
        public Hook ResultHook { get; private set; }

        /// <summary>
        /// Creates a dialog for adding a brand new hook
        /// </summary>
        /// <param name="project">The current project, used to populate the assembly list</param>
        /// <param name="categoryHint">The category the new hook should be created in, if any</param>
        public HookDetailsForm(Project project, string categoryHint = null)
        {
            InitializeComponent();

            _project = project;
            _existingHook = null;
            Text = "Add New Hook";

            PopulateAssemblies();
            PopulateExposures();
            PopulateReturnAndArgumentBehaviors();
            PopulateHookTypes(null);

            categorytextbox.Text = categoryHint ?? string.Empty;
            exposuredropdown.SelectedItem = MethodExposure.Public.ToString();
        }

        /// <summary>
        /// Creates a dialog for editing every detail of an existing hook, including switching
        /// it to a different hook type
        /// </summary>
        /// <param name="project">The current project, used to populate the assembly list</param>
        /// <param name="hook">The hook to edit</param>
        public HookDetailsForm(Project project, Hook hook)
        {
            InitializeComponent();

            _project = project;
            _existingHook = hook ?? throw new ArgumentNullException(nameof(hook));
            Text = "Edit Hook Details";

            PopulateAssemblies();
            PopulateExposures();
            PopulateReturnAndArgumentBehaviors();

            categorytextbox.Text = hook.HookCategory ?? string.Empty;
            nametextbox.Text = hook.Name;
            hooknametextbox.Text = hook.HookName;
            hookdescriptiontextbox.Text = hook.HookDescription;
            typenametextbox.Text = hook.TypeName;
            msilhashtextbox.Text = hook.MSILHash;

            if (assemblydropdown.Items.Contains(hook.AssemblyName))
            {
                assemblydropdown.SelectedItem = hook.AssemblyName;
            }

            if (hook.Signature != null)
            {
                exposuredropdown.SelectedItem = hook.Signature.Exposure.ToString();
                returntypetextbox.Text = hook.Signature.ReturnType;
                methodnametextbox.Text = hook.Signature.Name;
                parameterstextbox.Text = string.Join(", ", hook.Signature.Parameters);
            }
            else
            {
                exposuredropdown.SelectedItem = MethodExposure.Public.ToString();
            }

            // Populated last: selecting the hook's current type fires SelectedIndexChanged,
            // which loads that type's settings from the hook - so everything else needs to
            // already be in place first
            PopulateHookTypes(hook.GetType());
        }

        private void PopulateHookTypes(Type preselect)
        {
            for (int i = 0; i < Hook.HookTypes.Length; i++)
            {
                string typeName = Hook.HookTypes[i].GetCustomAttribute<HookType>().Name;
                hooktypedropdown.Items.Add(typeName);

                bool shouldSelect = preselect != null ? Hook.HookTypes[i] == preselect : Hook.HookTypes[i] == Hook.DefaultHookType;
                if (shouldSelect)
                {
                    hooktypedropdown.SelectedIndex = i;
                }
            }

            if (hooktypedropdown.SelectedIndex < 0 && hooktypedropdown.Items.Count > 0)
            {
                hooktypedropdown.SelectedIndex = 0;
            }
        }

        private void PopulateAssemblies()
        {
            foreach (Manifest manifest in _project.Manifests)
            {
                assemblydropdown.Items.Add(manifest.AssemblyName);
            }

            if (assemblydropdown.Items.Count > 0 && assemblydropdown.SelectedIndex < 0)
            {
                assemblydropdown.SelectedIndex = 0;
            }
        }

        private void PopulateExposures()
        {
            foreach (string name in Enum.GetNames(typeof(MethodExposure)))
            {
                exposuredropdown.Items.Add(name);
            }
        }

        private void PopulateReturnAndArgumentBehaviors()
        {
            foreach (ReturnBehavior value in Enum.GetValues(typeof(ReturnBehavior)))
            {
                simplereturnbehavior.Items.Add(value);
            }

            foreach (ArgumentBehavior value in Enum.GetValues(typeof(ArgumentBehavior)))
            {
                simpleargumentbehavior.Items.Add(value);
            }
        }

        private void hooktypedropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (hooktypedropdown.SelectedIndex < 0)
            {
                return;
            }

            Type hookType = Hook.HookTypes[hooktypedropdown.SelectedIndex];
            ShowTypeSettingsPanel(hookType);

            if (_existingHook != null && hookType == _existingHook.GetType())
            {
                // Selected type matches the hook's actual type - show its real settings
                LoadTypeSettingsFromHook(_existingHook);
            }
            else
            {
                // A brand new hook, or switching to a different type - start fresh, since
                // settings from one hook type don't carry over to another
                ResetTypeSettingsToDefaults(hookType);
            }
        }

        private void ShowTypeSettingsPanel(Type hookType)
        {
            simplesettingspanel.Visible = hookType == typeof(Simple);
            initoxidesettingspanel.Visible = hookType == typeof(InitOxide);
            modifysettingspanel.Visible = hookType == typeof(Modify);
        }

        private void LoadTypeSettingsFromHook(Hook hook)
        {
            switch (hook)
            {
                case Simple simple:
                    simpleinjectionindex.Value = Math.Max(0, simple.InjectionIndex);
                    simplereturnbehavior.SelectedIndex = (int)simple.ReturnBehavior;
                    simpleargumentbehavior.SelectedIndex = (int)simple.ArgumentBehavior;
                    simpleargumentstring.Text = simple.ArgumentString ?? string.Empty;
                    simpledeprecatedcheckbox.Checked = simple.Deprecation != null;
                    simplereplacementhooktextbox.Text = simple.Deprecation?.ReplacementHook ?? string.Empty;
                    simpleremovaldatepicker.Value = simple.Deprecation?.RemovalDate ?? DateTime.Now.AddDays(60);
                    break;

                case InitOxide initOxide:
                    initoxideinjectionindex.Value = Math.Max(0, initOxide.InjectionIndex);
                    break;

                case Modify modify:
                    modifyinjectionindex.Value = Math.Max(0, modify.InjectionIndex);
                    modifyremovecount.Value = Math.Max(0, modify.RemoveCount);
                    break;
            }
        }

        private void ResetTypeSettingsToDefaults(Type hookType)
        {
            if (hookType == typeof(Simple))
            {
                simpleinjectionindex.Value = 0;
                simplereturnbehavior.SelectedIndex = (int)ReturnBehavior.Continue;
                simpleargumentbehavior.SelectedIndex = (int)ArgumentBehavior.None;
                simpleargumentstring.Text = string.Empty;
                simpledeprecatedcheckbox.Checked = false;
                simplereplacementhooktextbox.Text = string.Empty;
                simpleremovaldatepicker.Value = DateTime.Now.AddDays(60);
            }
            else if (hookType == typeof(InitOxide))
            {
                initoxideinjectionindex.Value = 0;
            }
            else if (hookType == typeof(Modify))
            {
                modifyinjectionindex.Value = 0;
                modifyremovecount.Value = 0;
            }
        }

        private void ApplyTypeSettings(Hook hook)
        {
            switch (hook)
            {
                case Simple simple:
                    simple.InjectionIndex = (int)simpleinjectionindex.Value;
                    simple.ReturnBehavior = (ReturnBehavior)simplereturnbehavior.SelectedIndex;
                    simple.ArgumentBehavior = (ArgumentBehavior)simpleargumentbehavior.SelectedIndex;
                    simple.ArgumentString = simpleargumentstring.Text;
                    simple.Deprecation = simpledeprecatedcheckbox.Checked
                        ? new Simple.DeprecatedStatus
                        {
                            ReplacementHook = simplereplacementhooktextbox.Text.Trim(),
                            RemovalDate = simpleremovaldatepicker.Value
                        }
                        : null;
                    break;

                case InitOxide initOxide:
                    initOxide.InjectionIndex = (int)initoxideinjectionindex.Value;
                    break;

                case Modify modify:
                    modify.InjectionIndex = (int)modifyinjectionindex.Value;
                    modify.RemoveCount = (int)modifyremovecount.Value;
                    break;
            }
        }

        private void simpledeprecatedcheckbox_CheckedChanged(object sender, EventArgs e)
        {
            bool isDeprecated = simpledeprecatedcheckbox.Checked;

            simplereplacementhooklabel.Enabled = isDeprecated;
            simplereplacementhooktextbox.Enabled = isDeprecated;
            simpleremovaldatelabel.Enabled = isDeprecated;
            simpleremovaldatepicker.Enabled = isDeprecated;

            if (isDeprecated && simpleremovaldatepicker.Value < DateTime.Now)
            {
                simpleremovaldatepicker.Value = DateTime.Now.AddDays(60);
            }
        }

        private void findmethodbutton_Click(object sender, EventArgs e)
        {
            using (FindMethodForm form = new FindMethodForm(_project))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                if (!assemblydropdown.Items.Contains(form.ResultAssemblyName))
                {
                    assemblydropdown.Items.Add(form.ResultAssemblyName);
                }
                assemblydropdown.SelectedItem = form.ResultAssemblyName;

                typenametextbox.Text = form.ResultTypeName;
                exposuredropdown.SelectedItem = form.ResultSignature.Exposure.ToString();
                returntypetextbox.Text = form.ResultSignature.ReturnType;
                methodnametextbox.Text = form.ResultSignature.Name;
                parameterstextbox.Text = string.Join(", ", form.ResultSignature.Parameters);

                if (form.ResultMsilHash != null)
                {
                    msilhashtextbox.Text = form.ResultMsilHash;
                }

                if (nametextbox.TextLength == 0)
                {
                    nametextbox.Text = form.ResultSignature.Name;
                }

                if (hooknametextbox.TextLength == 0)
                {
                    hooknametextbox.Text = form.ResultSignature.Name;
                }
            }
        }

        private void okbutton_Click(object sender, EventArgs e)
        {
            if (nametextbox.TextLength == 0)
            {
                MessageBox.Show(this, "Please enter a hook name.", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (hooknametextbox.TextLength == 0)
            {
                MessageBox.Show(this, "Please enter an Oxide hook name.", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (assemblydropdown.SelectedItem == null)
            {
                MessageBox.Show(this, "Please select an assembly. If none are listed, add one to the project first.", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (typenametextbox.TextLength == 0)
            {
                MessageBox.Show(this, "Please enter the fully qualified type name.", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (methodnametextbox.TextLength == 0)
            {
                MessageBox.Show(this, "Please enter the target method name.", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (hooktypedropdown.SelectedIndex < 0)
            {
                MessageBox.Show(this, "Please select a hook type.", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Type selectedHookType = Hook.HookTypes[hooktypedropdown.SelectedIndex];

            if (selectedHookType == typeof(Simple) && simpledeprecatedcheckbox.Checked && simplereplacementhooktextbox.TextLength == 0)
            {
                MessageBox.Show(this, "Please enter the name of the replacement hook, or uncheck Deprecated.", "Oxide Patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MethodExposure exposure = (MethodExposure)Enum.Parse(typeof(MethodExposure), (string)exposuredropdown.SelectedItem);

            string[] parameters = parameterstextbox.Text
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .ToArray();

            MethodSignature signature = new MethodSignature(exposure, returntypetextbox.Text.Trim(), methodnametextbox.Text.Trim(), parameters);

            Hook hook;
            if (_existingHook != null && selectedHookType == _existingHook.GetType())
            {
                // Same type as before - update the existing hook object in place
                hook = _existingHook;
            }
            else if (_existingHook != null)
            {
                // Switched to a different hook type - the object has to be recreated, so
                // carry over everything that isn't tied to the old type's settings
                hook = Activator.CreateInstance(selectedHookType) as Hook;
                hook.Flagged = _existingHook.Flagged;
                hook.FlagReason = _existingHook.FlagReason;
                hook.BaseHook = _existingHook.BaseHook;
                hook.BaseHookName = _existingHook.BaseHookName;
            }
            else
            {
                hook = Activator.CreateInstance(selectedHookType) as Hook;
            }

            hook.Name = nametextbox.Text.Trim();
            hook.HookName = hooknametextbox.Text.Trim();
            hook.HookDescription = hookdescriptiontextbox.Text;
            hook.AssemblyName = (string)assemblydropdown.SelectedItem;
            hook.TypeName = typenametextbox.Text.Trim();
            hook.Signature = signature;
            hook.MSILHash = msilhashtextbox.TextLength > 0 ? msilhashtextbox.Text.Trim() : null;
            hook.HookCategory = categorytextbox.TextLength > 0 ? categorytextbox.Text.Trim() : null;

            ApplyTypeSettings(hook);

            ResultHook = hook;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
