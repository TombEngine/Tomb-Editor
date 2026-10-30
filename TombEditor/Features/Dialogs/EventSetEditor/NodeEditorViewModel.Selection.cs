#nullable enable

using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    // Multi-selection, group moving and selection-based commands of the node editor.
    public partial class NodeEditorViewModel
    {
        private readonly List<NodeViewModel> _selectedNodes = [];
        private List<(NodeViewModel Node, double Left, double Top)> _moveOrigins = [];

        /// <summary>All selected nodes in selection order.</summary>
        public IReadOnlyList<NodeViewModel> SelectedNodes => _selectedNodes;

        /// <summary>The last selected node, target of single-node actions (rename, colour).</summary>
        public NodeViewModel? SelectedNode => _selectedNodes.Count > 0 ? _selectedNodes[^1] : null;

        public bool HasSelectedNode => _selectedNodes.Count > 0;

        public bool HasMultipleSelectedNodes => _selectedNodes.Count > 1;

        /// <summary>Replaces the selection with the given nodes and refreshes everything depending on it.</summary>
        public void SetSelection(IEnumerable<NodeViewModel> nodes)
        {
            var selection = nodes.Distinct().ToList();

            foreach (var node in Nodes)
                node.IsSelected = selection.Contains(node);

            _selectedNodes.Clear();
            _selectedNodes.AddRange(selection);

            OnPropertyChanged(nameof(SelectedNode));
            OnPropertyChanged(nameof(HasSelectedNode));
            OnPropertyChanged(nameof(HasMultipleSelectedNodes));
            RefreshSelectionCommands();
        }

        public void SelectSingle(NodeViewModel? node) => SetSelection(node is null ? [] : [node]);

        /// <summary>Adds the node to the selection, or removes it when <paramref name="toggle"/> is set and it is already selected.</summary>
        public void SelectNode(NodeViewModel node, bool toggle)
        {
            if (!_selectedNodes.Contains(node))
                SetSelection([.. _selectedNodes, node]);
            else if (toggle)
                SetSelection(_selectedNodes.Where(n => n != node));
        }

        public void SelectAll() => SetSelection(Nodes);

        [RelayCommand]
        public void ClearSelection()
        {
            if (HasSelectedNode)
                SetSelection([]);
        }

        /// <summary>Selects the baseline plus every node intersecting the rubber-band area (canvas coordinates).</summary>
        public void SelectInArea(Rect area, IReadOnlyCollection<NodeViewModel> baseline)
        {
            var inside = Nodes.Where(n => area.IntersectsWith(new Rect(n.CanvasLeft, n.CanvasTop, n.Width, n.Height)));
            SetSelection(baseline.Concat(inside));
        }

        /// <summary>Starts moving the selection (or just the node, when it is not selected) from the current positions.</summary>
        public void BeginMove(NodeViewModel anchor)
        {
            var group = _selectedNodes.Contains(anchor) ? _selectedNodes : [anchor];
            _moveOrigins = group.Where(n => n.IsEditable).Select(n => (n, n.CanvasLeft, n.CanvasTop)).ToList();
        }

        /// <summary>Moves the nodes registered by <see cref="BeginMove"/> by a total offset since the move began.</summary>
        public void MoveBy(double deltaX, double deltaY)
        {
            foreach (var (node, left, top) in _moveOrigins)
            {
                node.CanvasLeft = left + deltaX;
                node.CanvasTop = top + deltaY;
            }
        }

        /// <summary>Links the selected nodes top to bottom where both ends are still free.</summary>
        [RelayCommand(CanExecute = nameof(HasMultipleSelectedNodes))]
        private void LinkSelected()
        {
            var ordered = _selectedNodes.Select(n => n.Node)
                                        .OrderBy(n => n.ScreenPosition.Y)
                                        .ThenBy(n => n.ScreenPosition.X)
                                        .ToList();

            bool linked = false;
            for (int i = 0; i < ordered.Count - 1; i++)
            {
                var node = ordered[i];
                var nextNode = ordered[i + 1];

                if (node.Next is not null || nextNode.Previous is not null)
                    continue;

                node.Next = nextNode;
                nextNode.Previous = node;
                linked = true;
            }

            if (linked)
                SyncRootsAndRebuild();
        }

        private void RefreshSelectionCommands()
        {
            RenameSelectedCommand.NotifyCanExecuteChanged();
            ColorSelectedCommand.NotifyCanExecuteChanged();
            LockSelectedCommand.NotifyCanExecuteChanged();
            DeleteSelectedNodeCommand.NotifyCanExecuteChanged();
            ExportSelectedCommand.NotifyCanExecuteChanged();
            LinkSelectedCommand.NotifyCanExecuteChanged();
        }
    }
}
