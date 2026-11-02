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

        /// <summary>
        /// Applies a click on a node: a plain click selects only that node, Ctrl adds it (or removes it when already
        /// selected) and Shift only adds it, so a selected group stays intact for dragging. Locked nodes ignore clicks.
        /// </summary>
        public void ClickNode(NodeViewModel node, bool ctrl, bool shift)
        {
            if (!node.IsEditable)
                return;

            var selection = ctrl || shift ? _selectedNodes.ToList() : [];

            if (!selection.Contains(node))
                selection.Add(node);
            else if (!shift)
                selection.Remove(node);

            SetSelection(selection);
        }

        public void SelectAll() => SetSelection(Nodes);

        [RelayCommand]
        public void ClearSelection()
        {
            if (HasSelectedNode)
                SetSelection([]);
        }

        /// <summary>Selects exactly the nodes intersecting the rubber-band area (canvas coordinates).</summary>
        public void SelectInArea(Rect area)
        {
            if (area.Width == 0.0 || area.Height == 0.0)
                return;

            SetSelection(Nodes.Where(n => area.IntersectsWith(new Rect(n.CanvasLeft, n.CanvasTop, n.Width, n.Height))));
        }

        /// <summary>Starts moving the dragged node together with the rest of the selection (locked ones too) from the current positions.</summary>
        public void BeginMove(NodeViewModel anchor)
        {
            var group = _selectedNodes.Contains(anchor) ? _selectedNodes : [anchor];
            _moveOrigins = group.Select(n => (n, n.CanvasLeft, n.CanvasTop)).ToList();
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
            // The stored Y grows upwards, so the topmost node has the highest Y.
            var ordered = _selectedNodes.Select(n => n.Node)
                                        .OrderByDescending(n => n.ScreenPosition.Y)
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
            LinkSelectedCommand.NotifyCanExecuteChanged();
            ExportCommand.NotifyCanExecuteChanged();
            ClearNodesCommand.NotifyCanExecuteChanged();
        }
    }
}
