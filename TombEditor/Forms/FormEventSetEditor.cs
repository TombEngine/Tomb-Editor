using DarkUI.Collections;
using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormEventSetEditor : DarkForm
    {
        private const string _folderNodeType = "Folder";
        private const string _folderSeparator = "/";

        private VolumeInstance _instance;
        private readonly Editor _editor;

        private bool _lockUI = false;
        private bool _lockSelectionChange = true;

        private List<EventSet> _usedList;

        private List<EventSet> _backupEventSetList;
        private Dictionary<VolumeInstance, int> _backupVolumes;
        private bool[] _backupVolumeState = new bool[2];

        private List<TriggerNode> _clipboard;

        private readonly PopUpInfo _popup = new PopUpInfo();
        private readonly List<string> _scriptFuncs;

        private string _mode => GlobalMode ? "global" : "volume";

        public bool GlobalMode => _usedList == _editor.Level.Settings.GlobalEventSets;
        public bool GenericMode => GlobalMode || _instance == null;

        private HashSet<string> _backupCollapsedFolders;
        private HashSet<string> _collapsedFolders => GlobalMode ? _editor.Level.Settings.CollapsedGlobalEventSetFolders : _editor.Level.Settings.CollapsedVolumeEventSetFolders;

        public EventSet SelectedSet
        {
            get
            {
                return _selectedSet;
            }

            set
            {
                if (value != null && value == _selectedSet)
                    return;

                _selectedSet = value;

                if (_selectedSet == null)
                {
                    ClearSelection();
                    ClearEventSetFromUI();
                }
                else
                {
                    var node = FindNodeByEventSet(_selectedSet);
                    if (node != null)
                    {
                        ClearSelection();
                        treeEvents.SelectNode(node);
                        treeEvents.EnsureVisible();
                        LoadEventSetIntoUI(_selectedSet);
                    }
                }

                if (!GenericMode)
                    _instance.EventSet = _selectedSet;
            }
        }
        private EventSet _selectedSet;

        public FormEventSetEditor(bool global, VolumeInstance instance = null)
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            _usedList = global ? _editor.Level.Settings.GlobalEventSets : _editor.Level.Settings.VolumeEventSets;
            _instance = instance;

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Configuration);

            // Backup event set list and volume state
            BackupState();

            // Populate function lists
            _scriptFuncs = ScriptingUtils.GetAllFunctionNames(_editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.TenLuaScriptFile));
            triggerManager.Initialize(_editor, ScriptingUtils.NodeFunctions, _scriptFuncs);

            // Populate event type list (needs to be done only once and before all other UI updates)
            PopulateEventTypeList();

            // Determine editing mode
            SetupUI();

            // Populate and select event set list
            PopulateEventSetList();

            // Only folder nodes can receive drag-drop children.
            treeEvents.CanDropIntoNode = n => IsFolderNode(n);

            // Sync folder paths when user drags nodes in the tree.
            treeEvents.NodesMoved += (s, args) => SyncFoldersFromTree();

            // Persist folder expansion state to the level file.
            treeEvents.AfterNodeExpand += (s, args) => SyncCollapsedFolders();
            treeEvents.AfterNodeCollapse += (s, args) => SyncCollapsedFolders();

            // Gray out UI by default, if event set list is empty
            if (_usedList.Count == 0)
                UpdateUI();

            // Don't select first event set if window is opened in generic mode
            _lockSelectionChange = !GenericMode;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Resize splitter
            splitContainer.SplitterDistance = _editor.Configuration.Window_FormEventSetEditor_SplitterDistance;

            if (!GenericMode)
                SelectedSet = _instance.EventSet;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // HACK: When trigger manager is invisible, control cleanup happens much faster.
            triggerManager.Visible = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editor.EditorEventRaised -= EditorEventRaised;

                if (DialogResult != DialogResult.OK)
                    RestoreState();
                else
                    SyncFoldersFromTree();

                _editor.EventSetsChange();
            }

            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.MessageEvent)
            {
                var msg = (Editor.MessageEvent)obj;
                PopUpInfo.Show(_popup, msg.ForceInMainWindow ? null : FindForm(), triggerManager, msg.Message, msg.Type);
            }

            if (obj is Editor.LevelChangedEvent)
                Close();

            if (obj is Editor.SelectedObjectChangedEvent)
                ChangeVolume(_editor.SelectedObject as VolumeInstance);

            if (obj is Editor.EventSetsChangedEvent)
            {
                PopulateEventSetList();
                SelectedSet = (_instance?.EventSet ?? null);
            }
        }

        public void ChangeVolume(VolumeInstance instance)
        {
            if (GlobalMode)
            {
                if (instance != null)
                    _popup.ShowInfo(triggerManager, "To edit volumes in realtime, please close this window");
                return;
            }

            _instance = instance;

            SetupUI();
            SelectedSet = (_instance?.EventSet ?? _selectedSet);
        }

        public void UpdateVolume()
        {
            // Don't update dummy or yet not placed volumes.
            if (_instance == null || _instance.Room == null)
                return;

            _editor.ObjectChange(_instance, ObjectChangeType.Change);
        }

        public void ClearSelection()
        {
            _lockSelectionChange = true;
            treeEvents.SelectNodes(new List<DarkTreeNode>());
            _lockSelectionChange = false;
        }

        private void SetupUI()
        {
            if (GenericMode)
            {
                butSearch.Location = butUnassignEventSet.Location;
                butUnassignEventSet.Visible = cbEnableVolume.Visible = cbAdjacentRooms.Visible = false;
                Text = "Edit " + _mode + " event sets";
            }
            else
            {
                butSearch.Location = new Point(butUnassignEventSet.Location.X - butSearch.Width - 6, butSearch.Location.Y);
                butUnassignEventSet.Visible = cbEnableVolume.Visible = cbAdjacentRooms.Visible = true;
                cbEnableVolume.Checked = _instance.Enabled;
                cbAdjacentRooms.Checked = _instance.DetectInAdjacentRooms;
                Text = "Edit volume: " + _instance.ToShortString();
            }

            if (GlobalMode)
            {
                panelActivators.Visible = false;
                panelEditor.Height = panelActivators.Location.Y + panelActivators.Height - panelEditor.Location.Y;
            }
        }

        private void UpdateUI()
        {
            bool eventSetSelected = SelectedSet != null;
            bool folderSelected = treeEvents.SelectedNodes.Count > 0 && IsFolderNode(treeEvents.SelectedNodes[0]);

            tbName.Enabled =
            triggerManager.Enabled =
            cbEvents.Enabled =
            butUnassignEventSet.Enabled =
            butCloneEventSet.Enabled = eventSetSelected;
            butDeleteEventSet.Enabled = eventSetSelected || folderSelected;

            cbActivatorLara.Enabled =
            cbActivatorNPC.Enabled =
            cbActivatorOtherMoveables.Enabled =
            cbActivatorStatics.Enabled =
            cbActivatorFlyBy.Enabled =
            lblActivators.Enabled = eventSetSelected && !GlobalMode;

            butSearch.Enabled = treeEvents.Nodes.Count > 0;
        }

        private void SetEventTooltip()
        {
            switch (cbEvents.SelectedItem)
            {
                case EventType.OnVolumeEnter:
                    toolTip.SetToolTip(cbEvents, "Occurs when something enters assigned volume. \nThis event performs once.");
                    break;

                case EventType.OnVolumeInside:
                    toolTip.SetToolTip(cbEvents, "Occurs when something resides inside assigned volume. \nThis event performs continuously.");
                    break;

                case EventType.OnVolumeLeave:
                    toolTip.SetToolTip(cbEvents, "Occurs when something leaves assigned volume. \nThis event performs once.");
                    break;

                case EventType.OnLevelEnd:
                    toolTip.SetToolTip(cbEvents, "Occurs when level was finished. \nThis event performs once.");
                    break;

                case EventType.OnLevelStart:
                    toolTip.SetToolTip(cbEvents, "Occurs when new level starts. \nThis event performs once.");
                    break;

                case EventType.OnLoadGame:
                    toolTip.SetToolTip(cbEvents, "Occurs when game was just loaded from savegame. \nThis event performs once.");
                    break;

                case EventType.OnSaveGame:
                    toolTip.SetToolTip(cbEvents, "Occurs when game was just saved. \nThis event performs once.");
                    break;

                case EventType.OnLoop:
                    toolTip.SetToolTip(cbEvents, "Occurs every game frame, except menus and freeze mode. \nThis event performs continuously.");
                    break;

                case EventType.OnUseItem:
                    toolTip.SetToolTip(cbEvents, "Occurs when an item was selected and used in inventory.");
                    break;

                case EventType.OnPickup:
                    toolTip.SetToolTip(cbEvents, "Occurs when an item was picked up by player.");
                    break;

                case EventType.OnVehicleEnter:
                    toolTip.SetToolTip(cbEvents, "Occurs when player enters a vehicle.");
                    break;

                case EventType.OnVehicleLeave:
                    toolTip.SetToolTip(cbEvents, "Occurs when player leaves a vehicle.");
                    break;

                case EventType.OnFreeze:
                    toolTip.SetToolTip(cbEvents, "Occurs when game is running in a freeze mode. \nThis event performs continuously.");
                    break;
            }
        }

        private void BackupState()
        {
            if (!GlobalMode)
            {
                if (!GenericMode)
                {
                    _backupVolumeState[0] = _instance.Enabled;
                    _backupVolumeState[1] = _instance.DetectInAdjacentRooms;
                }

                _backupVolumes = new Dictionary<VolumeInstance, int>();
                foreach (var vol in _editor.Level.GetAllObjects().OfType<VolumeInstance>())
                    _backupVolumes.Add(vol, _editor.Level.Settings.VolumeEventSets.IndexOf(vol.EventSet));
            }

            _backupEventSetList = new List<EventSet>();
            foreach (var evtSet in _usedList)
                _backupEventSetList.Add(evtSet.Clone());

            _backupCollapsedFolders = new HashSet<string>(_collapsedFolders);
        }

        private void RestoreState()
        {
            _collapsedFolders.Clear();
            foreach (var path in _backupCollapsedFolders)
                _collapsedFolders.Add(path);

            if (GlobalMode)
            {
                _editor.Level.Settings.GlobalEventSets = _backupEventSetList;
            }
            else
            {
                _editor.Level.Settings.VolumeEventSets = _backupEventSetList;

                var volumes = _editor.Level.GetAllObjects().OfType<VolumeInstance>().ToList();

                foreach (var vol in volumes)
                {
                    if (!_backupVolumes.ContainsKey(vol))
                        continue;

                    int index = -1;
                    var entry = _backupVolumes.TryGetValue(vol, out index);
                    if (index >= 0)
                        vol.EventSet = _backupEventSetList[index];
                }

                if (!GenericMode)
                {
                    _instance.Enabled = _backupVolumeState[0];
                    _instance.DetectInAdjacentRooms = _backupVolumeState[1];
                }
            }
        }

        private void PopulateEventTypeList()
        {
            cbEvents.Items.Clear();
            cbEvents.Items.AddRange((GlobalMode ? Event.GlobalEventTypes : Event.VolumeEventTypes).Cast<object>().ToArray());
        }

        private void PopulateEventSetList()
        {
            _lockSelectionChange = true;

            var collapsed = new HashSet<string>(_collapsedFolders);
            treeEvents.Nodes.Clear();

            foreach (var evtSet in _usedList)
            {
                var parentCollection = GetOrCreateFolderNodes(evtSet.Folder);
                parentCollection.Add(new DarkTreeNode(evtSet.Name) { Tag = evtSet });
            }

            foreach (var node in treeEvents.GetAllNodes().Where(n => IsFolderNode(n)))
                node.Expanded = !collapsed.Contains(GetFolderPath(node));

            _lockSelectionChange = false;
        }

        private DarkTreeNode FindFirstEventSetNode() => treeEvents.GetAllNodes().FirstOrDefault(n => n.Tag is EventSet);
        private DarkTreeNode FindNodeByEventSet(EventSet evtSet) => treeEvents.GetAllNodes().FirstOrDefault(n => n.Tag == evtSet);
        private bool IsFolderNode(DarkTreeNode node) => node != null && node.NodeType as string == _folderNodeType;
        private bool FolderNameExists(ObservableList<DarkTreeNode> collection, string name, DarkTreeNode exclude = null) =>
            collection.Any(n => IsFolderNode(n) && n != exclude && string.Equals(n.Text, name, StringComparison.OrdinalIgnoreCase));

        private ObservableList<DarkTreeNode> GetOrCreateFolderNodes(string path)
        {
            if (string.IsNullOrEmpty(path))
                return treeEvents.Nodes;

            var parts = path.Split(new[] { _folderSeparator }, StringSplitOptions.RemoveEmptyEntries);
            var currentCollection = treeEvents.Nodes;

            foreach (var part in parts)
            {
                var existing = currentCollection.FirstOrDefault(n => IsFolderNode(n) && n.Text == part);
                if (existing == null)
                {
                    existing = new DarkTreeNode(part) { NodeType = _folderNodeType };
                    currentCollection.Add(existing);
                }
                currentCollection = existing.Nodes;
            }

            return currentCollection;
        }

        private string GetFolderPath(DarkTreeNode node)
        {
            var parts = new List<string>();
            var current = node;

            while (current != null)
            {
                if (IsFolderNode(current))
                    parts.Insert(0, current.Text);
                current = current.ParentNode;
            }

            return string.Join(_folderSeparator, parts);
        }

        private void SyncFoldersFromTree()
        {
            _usedList.Clear();

            foreach (var node in treeEvents.GetAllNodes())
            {
                if (node.Tag is EventSet evtSet)
                {
                    evtSet.Folder = node.ParentNode != null ? GetFolderPath(node.ParentNode) : string.Empty;
                    _usedList.Add(evtSet);
                }
            }
        }

        private void SyncCollapsedFolders()
        {
            _collapsedFolders.Clear();

            foreach (var node in treeEvents.GetAllNodes())
            {
                if (IsFolderNode(node) && !node.Expanded)
                    _collapsedFolders.Add(GetFolderPath(node));
            }
        }

        private void RemoveEmptyFolderNodes()
        {
            bool removed;
            do
            {
                removed = false;
                foreach (var node in treeEvents.GetAllNodes())
                {
                    if (IsFolderNode(node) && node.Nodes.Count == 0)
                    {
                        var parent = node.ParentNode;
                        if (parent != null)
                            parent.Nodes.Remove(node);
                        else
                            treeEvents.Nodes.Remove(node);

                        removed = true;
                        break;
                    }
                }
            }
            while (removed);
        }

        private void ClearEventSetFromUI()
        {
            UpdateUI();

            _lockUI = true;

            cbActivatorLara.Checked =
            cbActivatorNPC.Checked =
            cbActivatorOtherMoveables.Checked =
            cbActivatorStatics.Checked =
            cbActivatorFlyBy.Checked = false;

            cbEvents.SelectedItem = null;
            triggerManager.Event = null;

            tbName.Text = string.Empty;

            _lockUI = false;
        }

        private void LoadEventSetIntoUI(EventSet newEventSet)
        {
            if (!GenericMode)
                _instance.EventSet = newEventSet;

            UpdateUI();

            _lockUI = true;

            if (!GlobalMode)
            {
                var evtSet = newEventSet as VolumeEventSet;

                cbActivatorLara.Checked = (evtSet.Activators & VolumeActivators.Player) != 0;
                cbActivatorNPC.Checked = (evtSet.Activators & VolumeActivators.NPCs) != 0;
                cbActivatorOtherMoveables.Checked = (evtSet.Activators & VolumeActivators.OtherMoveables) != 0;
                cbActivatorStatics.Checked = (evtSet.Activators & VolumeActivators.Statics) != 0;
                cbActivatorFlyBy.Checked = (evtSet.Activators & VolumeActivators.Flybys) != 0;
            }

            cbEvents.SelectedItem = newEventSet.LastUsedEvent;
            triggerManager.EventType = newEventSet.LastUsedEvent;
            triggerManager.Event = newEventSet.Events[newEventSet.LastUsedEvent];

            tbName.Text = newEventSet.Name;

            _lockUI = false;
        }

        private void ModifyActivators()
        {
            if (GlobalMode || SelectedSet == null || _lockUI)
                return;

            (SelectedSet as VolumeEventSet).Activators = 0 |
                                            (cbActivatorLara.Checked ? VolumeActivators.Player : 0) |
                                            (cbActivatorNPC.Checked ? VolumeActivators.NPCs : 0) |
                                            (cbActivatorOtherMoveables.Checked ? VolumeActivators.OtherMoveables : 0) |
                                            (cbActivatorStatics.Checked ? VolumeActivators.Statics : 0) |
                                            (cbActivatorFlyBy.Checked ? VolumeActivators.Flybys : 0);
            UpdateVolume();
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void treeEvents_SelectedNodesChanged(object sender, EventArgs e)
        {
            if (_lockSelectionChange)
                return;

            if (treeEvents.SelectedNodes.Count == 0)
            {
                SelectedSet = null;
                UpdateUI();
                return;
            }

            var selectedNode = treeEvents.SelectedNodes[0];
            if (selectedNode.Tag is EventSet evtSet)
            {
                _selectedSet = evtSet;
                LoadEventSetIntoUI(evtSet);
            }
            else
            {
                _selectedSet = null;
                ClearEventSetFromUI();
            }

            UpdateUI();
        }

        private void butNewEventSet_Click(object sender, EventArgs e)
        {
            var name = "New " + _mode + " event set " + (_usedList.Count + 1).ToString();

            // Determine folder from selected node.
            var folder = string.Empty;
            if (treeEvents.SelectedNodes.Count > 0)
            {
                var selected = treeEvents.SelectedNodes[0];
                if (IsFolderNode(selected))
                    folder = GetFolderPath(selected);
                else if (selected.ParentNode != null && IsFolderNode(selected.ParentNode))
                    folder = GetFolderPath(selected.ParentNode);
            }

            EventSet newSet;

            if (GlobalMode)
            {
                newSet = new GlobalEventSet()
                {
                    Name = name,
                    Folder = folder,
                    LastUsedEvent = Event.GlobalEventTypes[_editor.Configuration.NodeEditor_DefaultGlobalEventToEdit]
                };
            }
            else
            {
                newSet = new VolumeEventSet()
                {
                    Name = name,
                    Folder = folder,
                    LastUsedEvent = Event.VolumeEventTypes[_editor.Configuration.NodeEditor_DefaultEventToEdit]
                };
            }

            foreach (var evt in newSet.Events)
                evt.Value.Mode = (EventSetMode)_editor.Configuration.NodeEditor_DefaultEventMode;

            _usedList.Add(newSet);

            PopulateEventSetList();
            SelectedSet = newSet;

            tbName.Focus();
        }

        private void butCloneEventSet_Click(object sender, EventArgs e)
        {
            if (SelectedSet == null)
                return;

            var clonedSet = SelectedSet.Clone();
            clonedSet.Name = SelectedSet.Name + " (copy)";
            clonedSet.Folder = SelectedSet.Folder;
            _usedList.Add(clonedSet);

            PopulateEventSetList();
            SelectedSet = clonedSet;
        }

        private void butDeleteEventSet_Click(object sender, EventArgs e)
        {
            if (treeEvents.SelectedNodes.Count == 0)
                return;

            var selectedNode = treeEvents.SelectedNodes[0];

            if (IsFolderNode(selectedNode))
            {
                var eventSetsInFolder = GetAllEventSetsUnder(selectedNode).ToList();

                if (eventSetsInFolder.Count > 0)
                {
                    var result = DarkMessageBox.Show(this,
                        "Delete " + eventSetsInFolder.Count + " event set" + (eventSetsInFolder.Count > 1 ? "s" : string.Empty) + " in folder '" + selectedNode.Text + "'?",
                        "Delete folder", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                    if (result == DialogResult.Cancel)
                        return;

                    foreach (var evtSet in eventSetsInFolder)
                        EditorActions.DeleteEventSet(evtSet);
                }

                // Remove the folder node from the tree.
                if (selectedNode.ParentNode != null)
                    selectedNode.ParentNode.Nodes.Remove(selectedNode);
                else
                    treeEvents.Nodes.Remove(selectedNode);

                SyncFoldersFromTree();
                SelectFirstAvailableEventSet();
            }
            else if (selectedNode.Tag is EventSet)
            {
                var nextSet = FindAdjacentEventSet(selectedNode);

                EditorActions.DeleteEventSet(SelectedSet);
                PopulateEventSetList();

                if (nextSet != null && _usedList.Contains(nextSet))
                    SelectedSet = nextSet;
                else
                    SelectFirstAvailableEventSet();
            }
        }

        private void SelectFirstAvailableEventSet()
        {
            if (_usedList.Count > 0)
            {
                var firstNode = FindFirstEventSetNode();
                if (firstNode != null)
                    SelectedSet = firstNode.Tag as EventSet;
                else
                    SelectedSet = null;
            }
            else
            {
                SelectedSet = null;
            }
        }

        private EventSet FindAdjacentEventSet(DarkTreeNode current)
        {
            var allNodes = treeEvents.GetAllNodes();
            int index = allNodes.IndexOf(current);

            // Look forward first, then backward.
            for (int i = index + 1; i < allNodes.Count; i++)
            {
                if (allNodes[i].Tag is EventSet evtSet)
                    return evtSet;
            }

            for (int i = index - 1; i >= 0; i--)
            {
                if (allNodes[i].Tag is EventSet evtSet)
                    return evtSet;
            }

            return null;
        }

        private IEnumerable<EventSet> GetAllEventSetsUnder(DarkTreeNode node)
        {
            if (node.Tag is EventSet evtSet)
                yield return evtSet;

            foreach (var child in node.Nodes)
                foreach (var set in GetAllEventSetsUnder(child))
                    yield return set;
        }

        private void butUnassignEventSet_Click(object sender, EventArgs e)
        {
            if (GenericMode)
                return;

            SelectedSet = null;
        }

        private void cbActivators_CheckedChanged(object sender, EventArgs e)
        {
            ModifyActivators();
        }

        private void butSearch_Click(object sender, EventArgs e)
        {
            var searchPopUp = new PopUpSearch(treeEvents) { ShowAboveControl = true };
            searchPopUp.Show(this);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Don't process reserved camera keys
            if (WinFormsUtils.DirectionalCameraKeys.Contains(keyData))
                return base.ProcessCmdKey(ref msg, keyData);

            // Don't process one-key and shift hotkeys if we're focused on control which allows text input
            if (WinFormsUtils.CurrentControlSupportsInput(this, keyData))
                return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Delete:
                case Keys.Back:
                    if (treeEvents.ContainsFocus)
                    {
                        butDeleteEventSet_Click(butDeleteEventSet, EventArgs.Empty);
                        return true;
                    }
                    else
                    {
                        triggerManager.ProcessKey(keyData);
                    }
                    break;

                case (Keys.Control | Keys.C):
                    var copiedNodes = triggerManager.CopyNodes(false);
                    if (copiedNodes.Count > 0)
                    {
                        _clipboard = copiedNodes;
                        _editor.SendMessage("Selected nodes are copied to clipboard.", PopupType.Info);
                    }
                    break;

                case (Keys.Control | Keys.X):
                    _clipboard = triggerManager.CopyNodes(true);
                    break;

                case (Keys.Control | Keys.V):
                    triggerManager.PasteNodes(_clipboard);
                    break;

                default:
                    triggerManager.ProcessKey(keyData);
                    break;

            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void cbEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedSet == null)
                return;

            SetEventTooltip();

            if (!_lockUI)
            {
                SelectedSet.LastUsedEvent = (EventType)cbEvents.SelectedItem;
                triggerManager.EventType = SelectedSet.LastUsedEvent;
                triggerManager.Event = SelectedSet.Events[SelectedSet.LastUsedEvent];
            }
        }

        private void cbEvents_Format(object sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem != null && e.ListItem is EventType)
                e.Value = e.ListItem.ToString().SplitCamelcase();
        }

        private void tbName_Validated(object sender, EventArgs e)
        {
            if (SelectedSet == null || _lockUI)
                return;

            if (SelectedSet.Name == tbName.Text)
                return;

            if (string.IsNullOrEmpty(tbName.Text))
            {
                _popup.ShowWarning(triggerManager, "Event set name can't be empty");
                tbName.Text = SelectedSet.Name;
                return;
            }

            if (_usedList.Any(s => s.Name == tbName.Text))
            {
                _popup.ShowWarning(triggerManager, "An event set with same name already exists");
                tbName.Text = SelectedSet.Name;
                return;
            }

            EditorActions.ReplaceEventSetNames(_usedList, SelectedSet.Name, tbName.Text);
            SelectedSet.Name = tbName.Text;

            var node = FindNodeByEventSet(SelectedSet);
            if (node != null)
                node.Text = tbName.Text;
        }

        private void butNewFolder_Click(object sender, EventArgs e)
        {
            var parentFolder = string.Empty;

            if (treeEvents.SelectedNodes.Count > 0)
            {
                var selected = treeEvents.SelectedNodes[0];
                if (IsFolderNode(selected))
                    parentFolder = GetFolderPath(selected);
                else if (selected.ParentNode != null && IsFolderNode(selected.ParentNode))
                    parentFolder = GetFolderPath(selected.ParentNode);
            }

            var parentCollection = GetOrCreateFolderNodes(parentFolder);
            var newFolderName = PromptUniqueFolderName("New folder", "Enter folder name:", "New folder", parentCollection);
            if (newFolderName == null)
                return;

            var fullPath = string.IsNullOrEmpty(parentFolder) ? newFolderName : parentFolder + _folderSeparator + newFolderName;
            GetOrCreateFolderNodes(fullPath);

            var newNode = treeEvents.GetAllNodes().LastOrDefault(n => IsFolderNode(n) && n.Text == newFolderName);
            if (newNode != null)
            {
                treeEvents.SelectNode(newNode);
                treeEvents.EnsureVisible();
            }
        }

        private void RenameFolder(DarkTreeNode node)
        {
            if (!IsFolderNode(node))
                return;

            var siblings = node.ParentNode?.Nodes ?? treeEvents.Nodes;
            var newName = PromptUniqueFolderName("Rename folder", "Enter new folder name:", node.Text, siblings, node);
            if (newName == null || newName == node.Text)
                return;

            node.Text = newName;
            SyncFoldersFromTree();
        }

        // Shows a name-input dialog in a loop until the user enters a name that doesn't conflict with
        // an existing folder in the given collection, or cancels. Returns null on cancel or empty input.
        private string PromptUniqueFolderName(string title, string prompt, string initialValue, ObservableList<DarkTreeNode> collection, DarkTreeNode exclude = null)
        {
            var current = initialValue;
            while (true)
            {
                using (var inputBox = new FormInputBox(title, prompt, current))
                {
                    if (inputBox.ShowDialog(this) != DialogResult.OK)
                        return null;

                    var name = inputBox.Result.Trim();
                    if (string.IsNullOrEmpty(name))
                        return null;

                    if (!FolderNameExists(collection, name, exclude))
                        return name;

                    DarkMessageBox.Show(this, "A folder with that name already exists. Specify a different name.", title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    current = name;
                }
            }
        }

        private void treeEvents_DoubleClick(object sender, EventArgs e)
        {
            if (treeEvents.SelectedNodes.Count == 0)
                return;

            var node = treeEvents.SelectedNodes[0];
            if (!IsFolderNode(node))
                return;

            // DarkTreeView already toggled Expanded via OnMouseDoubleClick, undo that.
            node.Expanded = !node.Expanded;
            RenameFolder(node);
        }

        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (Visible)
                _editor.Configuration.Window_FormEventSetEditor_SplitterDistance = splitContainer.SplitterDistance;
        }

        private void cbEnableVolume_CheckedChanged(object sender, EventArgs e)
        {
            _instance.Enabled = cbEnableVolume.Checked;
            UpdateVolume();
        }

        private void cbAdjacentRooms_CheckedChanged(object sender, EventArgs e)
        {
            _instance.DetectInAdjacentRooms = cbAdjacentRooms.Checked;
            UpdateVolume();
        }
    }
}
