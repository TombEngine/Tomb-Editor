#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TombLib.LevelData;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>
    /// One row of the event-set tree: either a folder (grouping, <see cref="Set"/> is null)
    /// or an event-set leaf. Folder paths use "/" as separator, mirroring the previous
    /// FormEventSetEditor (#1156); the authoritative path lives on <see cref="EventSet.Folder"/>
    /// and is re-derived from the tree by the view-model's SyncFoldersFromTree.
    /// </summary>
    public partial class SetTreeNode : ObservableObject
    {
        public const string FolderSeparator = "/";

        public EventSet? Set { get; }
        public SetTreeNode? Parent { get; set; }
        public ObservableCollection<SetTreeNode> Children { get; } = new();

        [ObservableProperty] private string _displayName;
        [ObservableProperty] private bool _isExpanded = true;
        [ObservableProperty] private bool _isSelected;

        /// <summary>Raised when a folder is expanded/collapsed, so the VM can persist the state.</summary>
        public event Action? ExpansionChanged;

        private SetTreeNode(string displayName, EventSet? set)
        {
            _displayName = displayName;
            Set = set;
        }

        public static SetTreeNode Folder(string name) => new(name, null);
        public static SetTreeNode Leaf(EventSet set) => new(set.Name, set);

        public bool IsFolder => Set is null;

        partial void OnIsExpandedChanged(bool value) => ExpansionChanged?.Invoke();

        /// <summary>Path of this folder node ("A/B"); for leaves, the path of the containing folder.</summary>
        public string FolderPath
        {
            get
            {
                var parts = new List<string>();
                for (var node = IsFolder ? this : Parent; node is not null; node = node.Parent)
                    parts.Insert(0, node.DisplayName);
                return string.Join(FolderSeparator, parts);
            }
        }

        public bool IsDescendantOf(SetTreeNode other)
        {
            for (var node = Parent; node is not null; node = node.Parent)
                if (node == other)
                    return true;
            return false;
        }

        public IEnumerable<SetTreeNode> SelfAndDescendants()
        {
            yield return this;
            foreach (var child in Children)
                foreach (var node in child.SelfAndDescendants())
                    yield return node;
        }
    }
}
