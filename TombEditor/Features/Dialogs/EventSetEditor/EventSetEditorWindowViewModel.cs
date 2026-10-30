#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using TombLib.WPF.ViewModels;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>
    /// WPF port of <c>FormEventSetEditor</c> (step 1: event-set management shell + level-script mode).
    /// Edits the level's live global/volume <see cref="EventSet"/> list (Cancel restores a backup,
    /// mirroring the previous form). The per-event node graph is, for now, edited by the hosted previous
    /// <c>TriggerManager</c>; that area is replaced by a pure-WPF node editor in later steps.
    /// </summary>
    public partial class EventSetEditorWindowViewModel : ObservableObject, IModalDialogViewModel
    {
        private readonly Editor _editor;
        private readonly List<EventSet> _usedList;
        private VolumeInstance? _instance;

        private readonly List<EventSet> _backupList;
        private readonly Dictionary<VolumeInstance, int>? _backupVolumes;
        private readonly bool[] _backupVolumeState = new bool[2];

        private readonly IMessageService _messageService;
        private readonly IDialogService _dialogService;
        private readonly ILocalizationService _localizationService;

        private bool _lockUi;
        private bool _levelChanged;
        public bool Cancelled { get; private set; }

        [ObservableProperty] private bool? _dialogResult;

        public bool GlobalMode { get; }
        public bool GenericMode => GlobalMode || _instance == null;
        public bool ShowVolumeOptions => !GenericMode;

        public List<NodeFunction> NodeFunctions => ScriptingUtils.NodeFunctions;
        public List<string> ScriptFunctions { get; }

        private readonly ArgumentDataProvider _argumentProvider;

        /// <summary>Root rows of the event-set tree (folders + sets outside any folder).</summary>
        public ObservableCollection<SetTreeNode> TreeRoots { get; } = new();
        public IReadOnlyList<EventType> EventTypes { get; }

        private HashSet<string> _collapsedFolders => GlobalMode
            ? _editor.Level.Settings.CollapsedGlobalEventSetFolders
            : _editor.Level.Settings.CollapsedVolumeEventSetFolders;
        private readonly HashSet<string> _backupCollapsedFolders;

        /// <summary>Suppresses tree-event feedback (expansion sync, selection echo) while rebuilding.</summary>
        private bool _lockTree;

        public EventSetEditorWindowViewModel(Editor editor, bool global, VolumeInstance? instance = null)
        {
            _editor = editor;
            _messageService = ServiceLocator.ResolveService<IMessageService>();
            _dialogService = ServiceLocator.ResolveService<IDialogService>();
            _localizationService = ServiceLocator.ResolveService<ILocalizationService>().WithKeysFor(this);

            _usedList = global ? editor.Level.Settings.GlobalEventSets : editor.Level.Settings.VolumeEventSets;
            _instance = instance;
            GlobalMode = global;

            EventTypes = global ? Event.GlobalEventTypes : Event.VolumeEventTypes;
            ScriptFunctions = ScriptingUtils.GetAllFunctionNames(editor.Level.Settings.MakeAbsolute(editor.Level.Settings.TenLuaScriptFile));
            _argumentProvider = new ArgumentDataProvider(editor, ScriptFunctions);

            // Backup for Cancel.
            _backupList = _usedList.Select(s => s.Clone()).ToList();
            if (!GlobalMode)
            {
                _backupVolumes = new Dictionary<VolumeInstance, int>();
                foreach (var vol in editor.Level.GetAllObjects().OfType<VolumeInstance>())
                    _backupVolumes.Add(vol, editor.Level.Settings.VolumeEventSets.IndexOf(vol.EventSet));
                if (_instance != null)
                {
                    _backupVolumeState[0] = _instance.Enabled;
                    _backupVolumeState[1] = _instance.DetectInAdjacentRooms;
                }
            }

            _backupCollapsedFolders = new HashSet<string>(_collapsedFolders);
            BuildTree();

            // Like the previous dialog, generic mode does not preselect a set.
            if (!GenericMode)
                SelectedSet = _instance!.EventSet;

            _editor.EditorEventRaised += OnEditorEventRaised;
        }

        private void OnEditorEventRaised(IEditorEvent obj)
        {
            // The backup we hold belongs to the old level, so a level switch must close without restoring.
            if (obj is Editor.LevelChangedEvent)
            {
                _levelChanged = true;
                DialogResult = true;
            }
            else if (obj is Editor.SelectedObjectChangedEvent)
            {
                FollowVolume(_editor.SelectedObject as VolumeInstance);
            }
            else if (obj is Editor.EventSetsChangedEvent)
            {
                RepopulateSets();
            }
        }

        public bool HasSelectedSet => SelectedSet != null;

        public string Title => GenericMode
            ? _localizationService[GlobalMode ? "TitleGlobal" : "TitleVolume"]
            : _localizationService.Format("TitleVolumeSpecific", _instance!.ToShortString());

        /// <summary>Follows the editor's 3D selection to another volume (mirrors FormEventSetEditor.ChangeVolume).</summary>
        private void FollowVolume(VolumeInstance? instance)
        {
            if (GlobalMode)
                return;

            _instance = instance;
            OnPropertyChanged(nameof(GenericMode));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(EnableVolume));
            OnPropertyChanged(nameof(DetectInAdjacentRooms));
            SelectedSet = _instance?.EventSet ?? SelectedSet;
        }

        /// <summary>Rebuilds the set list after an external change (mirrors FormEventSetEditor on EventSetsChangedEvent).</summary>
        private void RepopulateSets()
        {
            if (_repopulating)
                return;

            _repopulating = true;
            var current = SelectedSet;

            BuildTree();

            if (!GenericMode)
                SelectedSet = _instance?.EventSet;
            else
                SelectedSet = current != null && _usedList.Contains(current) ? current : FirstAvailableSet();

            _repopulating = false;
        }

        private bool _repopulating;

        // Event-set folder tree (#1156 parity with the previous FormEventSetEditor).

        /// <summary>Rebuilds the whole tree from <see cref="_usedList"/> + the sets' Folder paths.</summary>
        private void BuildTree()
        {
            _lockTree = true;

            var collapsed = new HashSet<string>(_collapsedFolders);
            TreeRoots.Clear();

            foreach (var set in _usedList)
                GetOrCreateFolderChildren(set.Folder, out var parent).Add(Attach(SetTreeNode.Leaf(set), parent));

            foreach (var node in AllNodes().Where(n => n.IsFolder))
                node.IsExpanded = !collapsed.Contains(node.FolderPath);

            _lockTree = false;
        }

        private IEnumerable<SetTreeNode> AllNodes() => TreeRoots.SelectMany(r => r.SelfAndDescendants());

        private SetTreeNode? FindNodeBySet(EventSet set) => AllNodes().FirstOrDefault(n => n.Set == set);

        private EventSet? FirstAvailableSet() => AllNodes().FirstOrDefault(n => !n.IsFolder)?.Set;

        private SetTreeNode Attach(SetTreeNode node, SetTreeNode? parent)
        {
            node.Parent = parent;
            node.ExpansionChanged += OnNodeExpansionChanged;
            return node;
        }

        /// <summary>Returns the child collection for the given "/"-separated folder path, creating folder nodes as needed.</summary>
        private ObservableCollection<SetTreeNode> GetOrCreateFolderChildren(string? path, out SetTreeNode? parent)
        {
            parent = null;
            var collection = TreeRoots;

            if (string.IsNullOrEmpty(path))
                return collection;

            foreach (var part in path.Split(new[] { SetTreeNode.FolderSeparator }, StringSplitOptions.RemoveEmptyEntries))
            {
                var existing = collection.FirstOrDefault(n => n.IsFolder && n.DisplayName == part);
                if (existing == null)
                {
                    existing = Attach(SetTreeNode.Folder(part), parent);
                    collection.Add(existing);
                }
                parent = existing;
                collection = existing.Children;
            }

            return collection;
        }

        private void OnNodeExpansionChanged()
        {
            if (_lockTree)
                return;

            _collapsedFolders.Clear();
            foreach (var node in AllNodes())
                if (node.IsFolder && !node.IsExpanded)
                    _collapsedFolders.Add(node.FolderPath);
        }

        /// <summary>Writes the tree structure back to the sets' Folder paths and rebuilds the level list in tree order.</summary>
        public void SyncFoldersFromTree()
        {
            _usedList.Clear();

            foreach (var node in AllNodes())
            {
                if (node.Set is { } set)
                {
                    set.Folder = node.Parent?.FolderPath ?? string.Empty;
                    _usedList.Add(set);
                }
            }
        }

        /// <summary>Selection echo from the view's TreeView. Folder rows clear the set editor.</summary>
        [ObservableProperty] private SetTreeNode? _selectedNode;

        partial void OnSelectedNodeChanged(SetTreeNode? value)
        {
            if (_lockTree)
                return;

            _syncingTreeSelection = true;
            SelectedSet = value?.Set;
            _syncingTreeSelection = false;

            // Folder rows do not change SelectedSet, so refresh delete availability explicitly.
            RefreshCommandStates();
        }

        private bool _syncingTreeSelection;

        private void SelectNodeFor(EventSet? set)
        {
            if (_syncingTreeSelection)
                return;

            var node = set != null ? FindNodeBySet(set) : null;

            _lockTree = true;
            foreach (var n in AllNodes())
                n.IsSelected = n == node;
            for (var p = node?.Parent; p != null; p = p.Parent)
                p.IsExpanded = true;
            SelectedNode = node;
            _lockTree = false;

            OnNodeExpansionChanged();
        }

        /// <summary>Folder for newly created/cloned sets, derived from the current tree selection.</summary>
        private string TargetFolder()
        {
            if (SelectedNode == null)
                return string.Empty;
            if (SelectedNode.IsFolder)
                return SelectedNode.FolderPath;
            return SelectedNode.Parent?.FolderPath ?? string.Empty;
        }

        private bool FolderNameExists(IEnumerable<SetTreeNode> siblings, string name, SetTreeNode? exclude = null)
            => siblings.Any(n => n.IsFolder && n != exclude && string.Equals(n.DisplayName, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>Prompts until a unique sibling folder name is given; null on cancel/empty.</summary>
        private string? PromptUniqueFolderName(string title, string prompt, string initialValue, IEnumerable<SetTreeNode> siblings, SetTreeNode? exclude = null)
        {
            string current = initialValue;
            while (true)
            {
                var inputVm = new InputBoxWindowViewModel(title, prompt, current);
                if (_dialogService.ShowDialog(this, inputVm) != true)
                    return null;

                string name = inputVm.Value.Trim();
                if (string.IsNullOrEmpty(name))
                    return null;

                if (!FolderNameExists(siblings, name, exclude))
                    return name;

                _messageService.ShowError(_localizationService["FolderExists"]);
                current = name;
            }
        }

        [RelayCommand]
        private void NewFolder()
        {
            string parentFolder = TargetFolder();
            var siblings = GetOrCreateFolderChildren(parentFolder, out _);

            string? name = PromptUniqueFolderName(
                _localizationService["NewFolderTitle"], _localizationService["NewFolderPrompt"],
                _localizationService["NewFolderDefaultName"], siblings);
            if (name == null)
                return;

            string fullPath = string.IsNullOrEmpty(parentFolder) ? name : parentFolder + SetTreeNode.FolderSeparator + name;
            GetOrCreateFolderChildren(fullPath, out var folder);
            if (folder != null)
                SelectFolderNode(folder);
        }

        private void SelectFolderNode(SetTreeNode folder)
        {
            _lockTree = true;
            foreach (var n in AllNodes())
                n.IsSelected = n == folder;
            for (var p = folder.Parent; p != null; p = p.Parent)
                p.IsExpanded = true;
            SelectedNode = folder;
            _lockTree = false;

            _syncingTreeSelection = true;
            SelectedSet = null;
            _syncingTreeSelection = false;
        }

        /// <summary>Renames a folder node (double-click in the view), keeping sibling names unique.</summary>
        public void RenameFolder(SetTreeNode node)
        {
            if (!node.IsFolder)
                return;

            var siblings = node.Parent?.Children ?? TreeRoots;
            string? name = PromptUniqueFolderName(
                _localizationService["RenameFolderTitle"], _localizationService["RenameFolderPrompt"],
                node.DisplayName, siblings, node);
            if (name == null || name == node.DisplayName)
                return;

            node.DisplayName = name;
            SyncFoldersFromTree();
            OnNodeExpansionChanged();
        }

        /// <summary>Moves a node (set or folder) into a folder (or to the root when null). Used by drag-drop.</summary>
        public void MoveNode(SetTreeNode node, SetTreeNode? targetFolder)
        {
            if (targetFolder != null && (!targetFolder.IsFolder || node == targetFolder || targetFolder.IsDescendantOf(node)))
                return;

            var targetCollection = targetFolder?.Children ?? TreeRoots;
            if (targetCollection.Contains(node))
                return;

            if (node.IsFolder && FolderNameExists(targetCollection, node.DisplayName, node))
            {
                _messageService.ShowError(_localizationService["FolderExists"]);
                return;
            }

            var sourceCollection = node.Parent?.Children ?? TreeRoots;
            sourceCollection.Remove(node);
            node.Parent = targetFolder;
            targetCollection.Add(node);

            if (targetFolder != null)
                targetFolder.IsExpanded = true;

            SyncFoldersFromTree();
            OnNodeExpansionChanged();
        }

        // Selection.

        [ObservableProperty] private EventSet? _selectedSet;
        [ObservableProperty] private EventType _selectedEventType;
        [ObservableProperty] private Event? _currentEvent;

        partial void OnSelectedSetChanged(EventSet? value)
        {
            // Assigning null (Unassign) clears the volume's event set too.
            if (!GenericMode)
                _instance!.EventSet = value;

            SelectNodeFor(value);

            _lockUi = true;

            if (value != null)
            {
                _name = value.Name;
                OnPropertyChanged(nameof(Name));
                LoadActivators(value);
                SelectedEventType = value.LastUsedEvent;
                CurrentEvent = value.Events.TryGetValue(value.LastUsedEvent, out var evt) ? evt : null;
            }
            else
            {
                _name = string.Empty;
                OnPropertyChanged(nameof(Name));
                CurrentEvent = null;
            }

            _lockUi = false;
            OnPropertyChanged(nameof(HasSelectedSet));
            RefreshCommandStates();
        }

        partial void OnSelectedEventTypeChanged(EventType value)
        {
            if (_lockUi || SelectedSet == null)
                return;
            SelectedSet.LastUsedEvent = value;
            CurrentEvent = SelectedSet.Events.TryGetValue(value, out var evt) ? evt : null;
        }

        // Per-event editor (node graph + level-script modes).

        private const int NodeGridSize = 256;
        private const double NodeGridStep = 8.0;

        [ObservableProperty] private NodeEditorViewModel? _nodeEditor;

        public bool HasCurrentEvent => CurrentEvent != null;

        partial void OnCurrentEventChanged(Event? value)
        {
            NodeEditor = value != null
                ? new NodeEditorViewModel(value, SelectedEventType, NodeFunctions, _argumentProvider, NodeGridSize, NodeGridStep)
                : null;

            OnPropertyChanged(nameof(HasCurrentEvent));
            OnPropertyChanged(nameof(IsNodeEditorMode));
            OnPropertyChanged(nameof(IsLevelScriptMode));
            OnPropertyChanged(nameof(EventFunction));
            OnPropertyChanged(nameof(EventArgument));
        }

        public bool IsNodeEditorMode
        {
            get => CurrentEvent != null && CurrentEvent.Mode == EventSetMode.NodeEditor;
            set
            {
                if (CurrentEvent == null || !value)
                    return;
                CurrentEvent.Mode = EventSetMode.NodeEditor;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLevelScriptMode));
            }
        }

        public bool IsLevelScriptMode
        {
            get => CurrentEvent != null && CurrentEvent.Mode == EventSetMode.LevelScript;
            set
            {
                if (CurrentEvent == null || !value)
                    return;
                CurrentEvent.Mode = EventSetMode.LevelScript;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNodeEditorMode));
            }
        }

        public string EventFunction
        {
            get => CurrentEvent?.Function ?? string.Empty;
            set { if (CurrentEvent != null) { CurrentEvent.Function = value; OnPropertyChanged(); } }
        }

        public string EventArgument
        {
            get => CurrentEvent?.Argument ?? string.Empty;
            set { if (CurrentEvent != null) { CurrentEvent.Argument = value; OnPropertyChanged(); } }
        }

        // Name (with validation).

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_lockUi || SelectedSet == null || value == SelectedSet.Name)
                {
                    SetProperty(ref _name, value);
                    return;
                }

                if (string.IsNullOrEmpty(value))
                {
                    _messageService.ShowError(_localizationService["NameEmpty"]);
                    SetProperty(ref _name, SelectedSet.Name);
                    return;
                }

                if (_usedList.Any(s => s.Name == value))
                {
                    _messageService.ShowError(_localizationService["NameExists"]);
                    SetProperty(ref _name, SelectedSet.Name);
                    return;
                }

                EditorActions.ReplaceEventSetNames(_usedList, SelectedSet.Name, value);
                SelectedSet.Name = value;
                SetProperty(ref _name, value);

                if (FindNodeBySet(SelectedSet) is { } node)
                    node.DisplayName = value;
            }
        }

        // Volume options.

        [ObservableProperty] private bool _activatorLara;
        [ObservableProperty] private bool _activatorNpc;
        [ObservableProperty] private bool _activatorOtherMoveables;
        [ObservableProperty] private bool _activatorStatics;
        [ObservableProperty] private bool _activatorFlyBy;

        private void LoadActivators(EventSet set)
        {
            if (set is not VolumeEventSet volumeSet)
                return;
            ActivatorLara = (volumeSet.Activators & VolumeActivators.Player) != 0;
            ActivatorNpc = (volumeSet.Activators & VolumeActivators.NPCs) != 0;
            ActivatorOtherMoveables = (volumeSet.Activators & VolumeActivators.OtherMoveables) != 0;
            ActivatorStatics = (volumeSet.Activators & VolumeActivators.Statics) != 0;
            ActivatorFlyBy = (volumeSet.Activators & VolumeActivators.Flybys) != 0;
        }

        partial void OnActivatorLaraChanged(bool value) => ModifyActivators();
        partial void OnActivatorNpcChanged(bool value) => ModifyActivators();
        partial void OnActivatorOtherMoveablesChanged(bool value) => ModifyActivators();
        partial void OnActivatorStaticsChanged(bool value) => ModifyActivators();
        partial void OnActivatorFlyByChanged(bool value) => ModifyActivators();

        private void ModifyActivators()
        {
            if (GlobalMode || _lockUi || SelectedSet is not VolumeEventSet volumeSet)
                return;

            volumeSet.Activators =
                (ActivatorLara ? VolumeActivators.Player : 0) |
                (ActivatorNpc ? VolumeActivators.NPCs : 0) |
                (ActivatorOtherMoveables ? VolumeActivators.OtherMoveables : 0) |
                (ActivatorStatics ? VolumeActivators.Statics : 0) |
                (ActivatorFlyBy ? VolumeActivators.Flybys : 0);
            UpdateVolume();
        }

        public bool EnableVolume
        {
            get => _instance?.Enabled ?? false;
            set { if (_instance != null) { _instance.Enabled = value; OnPropertyChanged(); UpdateVolume(); } }
        }
        public bool DetectInAdjacentRooms
        {
            get => _instance?.DetectInAdjacentRooms ?? false;
            set { if (_instance != null) { _instance.DetectInAdjacentRooms = value; OnPropertyChanged(); UpdateVolume(); } }
        }

        private void UpdateVolume()
        {
            if (_instance?.Room != null)
                _editor.ObjectChange(_instance, ObjectChangeType.Change);
        }

        // Commands.

        [RelayCommand]
        private void NewSet()
        {
            string name = _localizationService.Format(GlobalMode ? "NewGlobalSetName" : "NewVolumeSetName", _usedList.Count + 1);
            string folder = TargetFolder();
            EventSet newSet = GlobalMode
                ? new GlobalEventSet { Name = name, Folder = folder, LastUsedEvent = Event.GlobalEventTypes[_editor.Configuration.NodeEditor_DefaultGlobalEventToEdit] }
                : new VolumeEventSet { Name = name, Folder = folder, LastUsedEvent = Event.VolumeEventTypes[_editor.Configuration.NodeEditor_DefaultEventToEdit] };

            foreach (var evt in newSet.Events)
                evt.Value.Mode = (EventSetMode)_editor.Configuration.NodeEditor_DefaultEventMode;

            _usedList.Add(newSet);
            BuildTree();
            SelectedSet = newSet;
        }

        [RelayCommand(CanExecute = nameof(HasSelectedSet))]
        private void CloneSet()
        {
            if (SelectedSet == null)
                return;
            var clone = SelectedSet.Clone();
            clone.Name = SelectedSet.Name + _localizationService["CopySuffix"];
            clone.Folder = SelectedSet.Folder;
            _usedList.Add(clone);
            BuildTree();
            SelectedSet = clone;
        }

        /// <summary>Allows deleting a selected folder row too, so the toolbar button covers both cases.</summary>
        public bool HasDeletableSelection => SelectedNode != null;

        [RelayCommand(CanExecute = nameof(HasDeletableSelection))]
        private void DeleteSet()
        {
            var node = SelectedNode;
            if (node == null)
                return;

            if (node.IsFolder)
            {
                var setsInFolder = node.SelfAndDescendants().Where(n => n.Set != null).Select(n => n.Set!).ToList();

                if (setsInFolder.Count > 0)
                {
                    bool proceed = _messageService.ShowConfirmation(
                        _localizationService.Format("DeleteFolderConfirm", setsInFolder.Count, node.DisplayName),
                        _localizationService["DeleteFolderTitle"],
                        defaultValue: false,
                        isRisky: true);
                    if (!proceed)
                        return;

                    foreach (var set in setsInFolder)
                        EditorActions.DeleteEventSet(set);
                }

                (node.Parent?.Children ?? TreeRoots).Remove(node);
                SyncFoldersFromTree();
                SelectedSet = FirstAvailableSet();
                return;
            }

            var toDelete = node.Set!;
            var nextSet = FindAdjacentSet(node);

            EditorActions.DeleteEventSet(toDelete);
            BuildTree();

            SelectedSet = nextSet != null && _usedList.Contains(nextSet) ? nextSet : FirstAvailableSet();
        }

        private EventSet? FindAdjacentSet(SetTreeNode current)
        {
            var all = AllNodes().ToList();
            int index = all.IndexOf(current);

            for (int i = index + 1; i < all.Count; i++)
                if (all[i].Set is { } set)
                    return set;
            for (int i = index - 1; i >= 0; i--)
                if (all[i].Set is { } set)
                    return set;
            return null;
        }

        [RelayCommand]
        private void UnassignSet()
        {
            if (!GenericMode)
                SelectedSet = null;
        }

        [RelayCommand]
        private void Ok() => DialogResult = true;

        [RelayCommand]
        private void Cancel() => DialogResult = false;

        /// <summary>
        /// Called by the view after the window has closed (any close path). Only OK keeps the changes;
        /// Cancel, the title-bar X and an external <c>Close()</c> restore the backup.
        /// </summary>
        public void OnWindowClosed()
        {
            _editor.EditorEventRaised -= OnEditorEventRaised;

            if (_levelChanged)
                return;

            // Like the previous dialog, anything but OK (Cancel, the window X, Escape) restores the backup.
            if (DialogResult != true)
            {
                Cancelled = true;
                RestoreState();
            }
            else
            {
                SyncFoldersFromTree();
            }
            _editor.EventSetsChange();
        }

        private void RestoreState()
        {
            _collapsedFolders.Clear();
            foreach (string path in _backupCollapsedFolders)
                _collapsedFolders.Add(path);

            if (GlobalMode)
            {
                _editor.Level.Settings.GlobalEventSets = _backupList;
            }
            else
            {
                _editor.Level.Settings.VolumeEventSets = _backupList;

                foreach (var vol in _editor.Level.GetAllObjects().OfType<VolumeInstance>())
                {
                    if (_backupVolumes != null && _backupVolumes.TryGetValue(vol, out int index) && index >= 0)
                        vol.EventSet = _backupList[index];
                }

                if (_instance != null)
                {
                    _instance.Enabled = _backupVolumeState[0];
                    _instance.DetectInAdjacentRooms = _backupVolumeState[1];
                }
            }
        }

        private void RefreshCommandStates()
        {
            OnPropertyChanged(nameof(HasDeletableSelection));
            CloneSetCommand.NotifyCanExecuteChanged();
            DeleteSetCommand.NotifyCanExecuteChanged();
        }
    }
}
