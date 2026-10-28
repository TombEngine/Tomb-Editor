#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using TombLib.LevelData.VisualScripting;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using TombLib.WPF.ViewModels;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>
    /// Drives the pure-WPF node canvas for one <see cref="Event"/>: builds node/link view-models from
    /// the event's <see cref="TriggerNode"/> graph, and supports creating (from the function catalog),
    /// dragging, deleting and connecting (Next/Else) nodes. Argument editing follows in step 3.
    /// </summary>
    public partial class NodeEditorViewModel : ObservableObject
    {
        private readonly TombLib.LevelData.Event _event;
        private readonly TombLib.LevelData.EventType _eventType;
        private readonly int _gridSize;
        private readonly double _gridStep;
        private readonly ArgumentDataProvider _provider;
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
        private readonly IColorPickerService _colorPickerService;
        private readonly ILocalizationService _localizationService;

        public IReadOnlyList<NodeFunction> Functions { get; }
        public ObservableCollection<NodeViewModel> Nodes { get; } = new();
        public ObservableCollection<LinkViewModel> Links { get; } = new();

        [ObservableProperty] private NodeViewModel? _selectedNode;
        [ObservableProperty] private NodeFunction? _functionToAdd;

        /// <summary>Drives the enabled state of the node-action toolbar buttons.</summary>
        public bool HasSelectedNode => SelectedNode != null;

        partial void OnSelectedNodeChanged(NodeViewModel? value)
        {
            // Keep each node's IsSelected flag (drives the selection border) in sync with the selection.
            foreach (var node in Nodes)
                node.IsSelected = node == value;

            OnPropertyChanged(nameof(HasSelectedNode));
            RefreshCommandStates();
        }

        private void RefreshCommandStates()
        {
            RenameSelectedCommand.NotifyCanExecuteChanged();
            ColorSelectedCommand.NotifyCanExecuteChanged();
            LockSelectedCommand.NotifyCanExecuteChanged();
            DeleteSelectedNodeCommand.NotifyCanExecuteChanged();
            ExportSelectedCommand.NotifyCanExecuteChanged();
        }

        /// <summary>Asks the view to scroll the given node into view (a purely visual concern).</summary>
        public event Action<NodeViewModel>? BringNodeIntoViewRequested;

        /// <summary>How many times the event may fire (0 = unlimited), edited via the call-count box.</summary>
        public int CallCounter
        {
            get => _event.CallCounter;
            set { if (_event.CallCounter == value) return; _event.CallCounter = value; OnPropertyChanged(); }
        }

        public double CanvasWidth => _gridSize * _gridStep;
        public double CanvasHeight => _gridSize * _gridStep;

        public NodeEditorViewModel(TombLib.LevelData.Event evt, TombLib.LevelData.EventType eventType, IReadOnlyList<NodeFunction> functions, ArgumentDataProvider provider, int gridSize, double gridStep,
            IDialogService? dialogService = null, IMessageService? messageService = null, IColorPickerService? colorPickerService = null,
            ILocalizationService? localizationService = null)
        {
            _event = evt;
            _eventType = eventType;
            Functions = functions;
            _provider = provider;
            _gridSize = gridSize;
            _gridStep = gridStep;
            _dialogService = ServiceLocator.ResolveService(dialogService);
            _messageService = ServiceLocator.ResolveService(messageService);
            _colorPickerService = ServiceLocator.ResolveService(colorPickerService);
            _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

            // Group the function picker by Section (the default view is shared, so add only once).
            var functionsView = System.Windows.Data.CollectionViewSource.GetDefaultView(Functions);
            if (functionsView != null && functionsView.GroupDescriptions.Count == 0)
                functionsView.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription(nameof(NodeFunction.Section)));

            Rebuild();
        }

        private void Rebuild()
        {
            foreach (var link in Links)
                link.Detach();
            Links.Clear();
            Nodes.Clear();

            var map = new Dictionary<TriggerNode, NodeViewModel>();
            foreach (var node in TriggerNode.LinearizeNodes(_event.Nodes))
            {
                var function = Functions.FirstOrDefault(f => f.Signature == node.Function);
                var vm = new NodeViewModel(node, _gridStep, function, _provider, _eventType);
                map[node] = vm;
                Nodes.Add(vm);
            }

            foreach (var vm in Nodes)
            {
                if (vm.Node.Next != null && map.TryGetValue(vm.Node.Next, out var next))
                    Links.Add(new LinkViewModel(vm, next, false));

                if (vm.Node is TriggerNodeCondition condition && condition.Else != null && map.TryGetValue(condition.Else, out var elseVm))
                    Links.Add(new LinkViewModel(vm, elseVm, true));
            }
        }

        private void SyncRootsAndRebuild()
        {
            var all = TriggerNode.LinearizeNodes(_event.Nodes);
            _event.Nodes = all.Where(n => n.Previous == null).ToList();
            Rebuild();
        }

        [RelayCommand]
        private void AddNode()
        {
            if (FunctionToAdd == null)
                return;

            TriggerNode node = FunctionToAdd.Conditional
                ? new TriggerNodeCondition { Name = _localizationService.Format("IfNodeName", Nodes.Count(n => n.IsCondition) + 1) }
                : new TriggerNodeAction { Name = _localizationService.Format("ActionNodeName", Nodes.Count(n => !n.IsCondition) + 1) };

            node.Size = TriggerNode.DefaultSize;
            node.Function = FunctionToAdd.Signature;
            node.FixArguments(FunctionToAdd);

            PlaceNode(node);

            _event.Nodes.Add(node);
            SyncRootsAndRebuild();
            SelectedNode = Nodes.FirstOrDefault(n => n.Node == node);
        }

        /// <summary>Parameter of <see cref="AddNodeAtPositionCommand"/>: a function plus the canvas position to drop it at.</summary>
        public sealed record AddNodeRequest(NodeFunction Function, double CanvasX, double CanvasY);

        /// <summary>Creates a node for a function at a specific canvas position (right-click add).</summary>
        [RelayCommand]
        private void AddNodeAtPosition(AddNodeRequest? request)
        {
            if (request == null)
                return;

            NodeFunction func = request.Function;
            TriggerNode node = func.Conditional
                ? new TriggerNodeCondition { Name = _localizationService.Format("IfNodeName", Nodes.Count(n => n.IsCondition) + 1) }
                : new TriggerNodeAction { Name = _localizationService.Format("ActionNodeName", Nodes.Count(n => !n.IsCondition) + 1) };

            node.Size = TriggerNode.DefaultSize;
            node.Function = func.Signature;
            node.FixArguments(func);
            node.ScreenPosition = new Vector2(
                (float)Math.Clamp(request.CanvasX / _gridStep, 0, 256),
                (float)Math.Clamp(request.CanvasY / _gridStep, 0, 256));

            _event.Nodes.Add(node);
            SyncRootsAndRebuild();
            SelectedNode = Nodes.FirstOrDefault(n => n.Node == node);
        }

        /// <summary>Picks a non-overlapping position below the selected node (mirrors NodeEditor.GetBestPosition).</summary>
        private void PlaceNode(TriggerNode node)
        {
            const double estimatedHeight = 120.0;
            double width = node.Size;

            double x, y;
            if (SelectedNode != null)
            {
                x = SelectedNode.CanvasLeft + SelectedNode.Width / 2.0 - width / 2.0;
                y = SelectedNode.CanvasTop + SelectedNode.Height + _gridStep * 4.0;
            }
            else
            {
                x = _gridStep * 40.0;
                y = _gridStep * 5.0;
            }

            double limit = _gridSize * _gridStep;
            bool colliding = true;
            while (colliding)
            {
                colliding = false;
                var rect = new Rect(x, y, width, estimatedHeight);
                foreach (var other in Nodes)
                {
                    var otherRect = new Rect(other.CanvasLeft - _gridStep, other.CanvasTop - _gridStep * 2.0,
                        other.Width + _gridStep * 2.0, other.Height + _gridStep * 4.0);
                    if (rect.IntersectsWith(otherRect))
                    {
                        colliding = true;
                        break;
                    }
                }

                if (colliding)
                    y += _gridStep * 4.0;
                if (y > limit)
                    break;
            }

            node.ScreenPosition = new Vector2(
                (float)Math.Clamp(x / _gridStep, 0, 256),
                (float)Math.Clamp(y / _gridStep, 0, 256));
        }

        // Clipboard (shared across events/instances within the session).

        private static List<TriggerNode>? _clipboard;

        public void CopySelected(bool cut)
        {
            if (SelectedNode == null)
                return;

            var clone = SelectedNode.Node.Clone();
            clone.Previous = null;
            _clipboard = new List<TriggerNode> { clone };

            if (cut)
                DeleteSelectedNode();
        }

        public void Paste()
        {
            if (_clipboard == null || _clipboard.Count == 0)
                return;

            foreach (var source in _clipboard)
            {
                var clone = source.Clone();
                clone.Previous = null;
                var p = clone.ScreenPosition;
                clone.ScreenPosition = new Vector2(Math.Clamp(p.X + 4, 0, 256), Math.Clamp(p.Y + 4, 0, 256));
                _event.Nodes.Add(clone);
            }

            SyncRootsAndRebuild();
            SelectedNode = Nodes.LastOrDefault();
        }

        [RelayCommand(CanExecute = nameof(HasSelectedNode))]
        private void DeleteSelectedNode()
        {
            if (SelectedNode == null)
                return;

            var node = SelectedNode.Node;

            // Detach from previous.
            if (node.Previous != null)
            {
                if (node.Previous.Next == node)
                    node.Previous.Next = null;
                if (node.Previous is TriggerNodeCondition prevCondition && prevCondition.Else == node)
                    prevCondition.Else = null;
            }

            // Promote children to roots.
            if (node.Next != null)
            {
                node.Next.Previous = null;
                _event.Nodes.Add(node.Next);
                node.Next = null;
            }
            if (node is TriggerNodeCondition condition && condition.Else != null)
            {
                condition.Else.Previous = null;
                _event.Nodes.Add(condition.Else);
                condition.Else = null;
            }

            _event.Nodes.Remove(node);
            SelectedNode = null;
            SyncRootsAndRebuild();
        }

        /// <summary>Deletes a specific node (context-menu action), selecting it first.</summary>
        [RelayCommand]
        private void DeleteNode(NodeViewModel? node)
        {
            if (node == null)
                return;

            SelectedNode = node;
            DeleteSelectedNode();
        }

        // --- Node-action toolbar commands (operate on the last selected node) ---

        [RelayCommand(CanExecute = nameof(HasSelectedNode))]
        private void RenameSelected()
        {
            if (SelectedNode is not { } node)
                return;

            var inputBox = new InputBoxWindowViewModel(title: _localizationService["RenameNodeTitle"], label: _localizationService["RenameNodeLabel"], initialValue: node.Title);
            if (_dialogService.ShowDialog(this, inputBox) == true)
                node.Title = inputBox.Value;
        }

        [RelayCommand(CanExecute = nameof(HasSelectedNode))]
        private void ColorSelected()
        {
            if (SelectedNode is { } node)
                PickNodeColor(node);
        }

        /// <summary>Picks a color for a specific node (context-menu action).</summary>
        [RelayCommand]
        private void SetColor(NodeViewModel? node)
        {
            if (node != null)
                PickNodeColor(node);
        }

        private void PickNodeColor(NodeViewModel node)
        {
            var current = (node.HeaderBrush as SolidColorBrush)?.Color ?? Colors.Gray;
            var oldColor = new Vector3(current.R, current.G, current.B) / 255.0f;

            // Live-preview while picking; restore the old color on cancel.
            Vector3? picked = _colorPickerService.PickColor(oldColor, c => node.SetColor(ToMediaColor(c)));
            node.SetColor(ToMediaColor(picked ?? oldColor));
        }

        private static Color ToMediaColor(Vector3 color) => Color.FromRgb(
            (byte)Math.Clamp(Math.Round(color.X * 255.0), 0.0, 255.0),
            (byte)Math.Clamp(Math.Round(color.Y * 255.0), 0.0, 255.0),
            (byte)Math.Clamp(Math.Round(color.Z * 255.0), 0.0, 255.0));

        [RelayCommand(CanExecute = nameof(HasSelectedNode))]
        private void LockSelected()
        {
            if (SelectedNode is { } node)
                node.IsLocked = !node.IsLocked;
        }

        [RelayCommand(CanExecute = nameof(HasSelectedNode))]
        private void ExportSelected() => CopySelected(false);

        /// <summary>Removes every node in the event (the "Clear" toolbar action).</summary>
        [RelayCommand]
        private void ClearNodes()
        {
            if (Nodes.Count == 0)
                return;

            if (!_messageService.ShowConfirmation(_localizationService["ClearNodesMessage"], _localizationService["ClearNodesTitle"], isRisky: true))
                return;

            _event.Nodes = new List<TriggerNode>();
            SelectedNode = null;
            SyncRootsAndRebuild();
        }

        /// <summary>Finds a node by its (case-insensitive) name (the "Find" toolbar action) and brings it into view.</summary>
        [RelayCommand]
        private void FindNode()
        {
            var inputBox = new InputBoxWindowViewModel(title: _localizationService["FindNodeTitle"], label: _localizationService["FindNodeLabel"]);
            if (_dialogService.ShowDialog(this, inputBox) != true)
                return;

            var node = Nodes.FirstOrDefault(n => string.Equals(n.Title, inputBox.Value, StringComparison.OrdinalIgnoreCase));
            if (node is null)
            {
                _messageService.ShowInformation(_localizationService.Format("FindNodeNotFound", inputBox.Value), _localizationService["FindNodeTitle"]);
                return;
            }

            SelectedNode = node;
            BringNodeIntoViewRequested?.Invoke(node);
        }

        /// <summary>Connects <paramref name="from"/>'s Next (or Else) output to <paramref name="to"/>.</summary>
        public void Connect(NodeViewModel from, NodeViewModel to, bool asElse)
        {
            if (from == to || to.Node.Previous != null || WouldCycle(from.Node, to.Node))
                return;

            if (asElse && from.Node is TriggerNodeCondition condition)
            {
                if (condition.Else != null)
                    condition.Else.Previous = null;
                condition.Else = to.Node;
            }
            else
            {
                if (from.Node.Next != null)
                    from.Node.Next.Previous = null;
                from.Node.Next = to.Node;
            }

            to.Node.Previous = from.Node;
            SyncRootsAndRebuild();
        }

        /// <summary>Clears <paramref name="from"/>'s Next (or Else) connection.</summary>
        public void Disconnect(NodeViewModel from, bool asElse)
        {
            if (asElse && from.Node is TriggerNodeCondition condition)
            {
                if (condition.Else != null)
                {
                    condition.Else.Previous = null;
                    condition.Else = null;
                }
            }
            else if (from.Node.Next != null)
            {
                from.Node.Next.Previous = null;
                from.Node.Next = null;
            }

            SyncRootsAndRebuild();
        }

        private static bool WouldCycle(TriggerNode from, TriggerNode candidate)
        {
            // Walk down from candidate; if we reach 'from', linking would create a cycle.
            foreach (var node in TriggerNode.LinearizeNodes(new List<TriggerNode> { candidate }))
                if (node == from)
                    return true;
            return false;
        }
    }
}
