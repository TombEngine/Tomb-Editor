#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.LevelData;
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
    /// dragging, deleting and connecting (Next/Else) nodes. Selection handling lives in the
    /// <c>Selection</c> part of this class.
    /// </summary>
    public partial class NodeEditorViewModel : ObservableObject
    {
        private readonly Event _event;
        private readonly EventType _eventType;
        private readonly NodeEditorOptions _options;
        private readonly ArgumentDataProvider _provider;
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
        private readonly IColorPickerService _colorPickerService;
        private readonly ILocalizationService _localizationService;

        public IReadOnlyList<NodeFunction> Functions { get; }
        public ObservableCollection<NodeViewModel> Nodes { get; } = new();
        public ObservableCollection<LinkViewModel> Links { get; } = new();

        [ObservableProperty] private NodeFunction? _functionToAdd;

        /// <summary>Asks the view to scroll the given node into view (a purely visual concern).</summary>
        public event Action<NodeViewModel>? BringNodeIntoViewRequested;

        /// <summary>How many times the event may fire (0 = unlimited), edited via the call-count box.</summary>
        public int CallCounter
        {
            get => _event.CallCounter;
            set
            {
                if (_event.CallCounter == value)
                    return;

                _event.CallCounter = value;
                OnPropertyChanged();
            }
        }

        public double CanvasWidth => _options.CanvasExtent;
        public double CanvasHeight => _options.CanvasExtent;

        public NodeEditorViewModel(Event evt, EventType eventType, IReadOnlyList<NodeFunction> functions, ArgumentDataProvider provider, NodeEditorOptions options,
            IDialogService? dialogService = null, IMessageService? messageService = null, IColorPickerService? colorPickerService = null,
            ILocalizationService? localizationService = null)
        {
            _event = evt;
            _eventType = eventType;
            Functions = functions;
            _provider = provider;
            _options = options;
            _dialogService = ServiceLocator.ResolveService(dialogService);
            _messageService = ServiceLocator.ResolveService(messageService);
            _colorPickerService = ServiceLocator.ResolveService(colorPickerService);
            _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

            // Group the function picker by Section (the default view is shared, so add only once).
            var functionsView = System.Windows.Data.CollectionViewSource.GetDefaultView(Functions);
            if (functionsView is not null && functionsView.GroupDescriptions.Count == 0)
                functionsView.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription(nameof(NodeFunction.Section)));

            Rebuild();
        }

        /// <summary>Re-reads the call counter after the other event-editor mode changed it.</summary>
        public void RefreshCallCounter() => OnPropertyChanged(nameof(CallCounter));

        private void Rebuild()
        {
            // The view-models are recreated, so the selection is carried over by model node.
            var previouslySelected = _selectedNodes.Select(n => n.Node).ToHashSet();

            foreach (var link in Links)
                link.Detach();
            Links.Clear();
            Nodes.Clear();

            var map = new Dictionary<TriggerNode, NodeViewModel>();
            foreach (var node in TriggerNode.LinearizeNodes(_event.Nodes))
            {
                var function = Functions.FirstOrDefault(f => f.Signature == node.Function);
                var vm = new NodeViewModel(node, _options, function, _provider, _eventType);
                map[node] = vm;
                Nodes.Add(vm);
            }

            foreach (var vm in Nodes)
            {
                if (vm.Node.Next is not null && map.TryGetValue(vm.Node.Next, out var next))
                    Links.Add(new LinkViewModel(vm, next, false, _options.LinksAsRopes));

                if (vm.Node is TriggerNodeCondition condition && condition.Else is not null && map.TryGetValue(condition.Else, out var elseVm))
                    Links.Add(new LinkViewModel(vm, elseVm, true, _options.LinksAsRopes));
            }

            SetSelection(Nodes.Where(n => previouslySelected.Contains(n.Node)));
        }

        private void SyncRootsAndRebuild()
        {
            var all = TriggerNode.LinearizeNodes(_event.Nodes);
            _event.Nodes = all.Where(n => n.Previous is null).ToList();
            Rebuild();
        }

        // Node creation.

        /// <summary>Adds the function chosen in the toolbar; Ctrl skips linking to the selected node, Alt links through Else.</summary>
        [RelayCommand]
        private void AddNode()
        {
            if (FunctionToAdd is null)
                return;

            var modifiers = Keyboard.Modifiers;
            CreateNode(FunctionToAdd, modifiers != ModifierKeys.Control, modifiers == ModifierKeys.Alt, null);
        }

        /// <summary>Adds an action or condition node (keyboard shortcuts), reusing the toolbar function when its kind matches.</summary>
        public void AddNodeOfKind(bool condition, bool linkToPrevious, bool linkToElse)
        {
            var function = FunctionToAdd is { } chosen && chosen.Conditional == condition
                ? chosen
                : Functions.FirstOrDefault(f => f.Conditional == condition);

            if (function is not null)
                CreateNode(function, linkToPrevious, linkToElse, null);
        }

        /// <summary>Parameter of <see cref="AddNodeAtPositionCommand"/>: a function plus the canvas position to drop it at.</summary>
        public sealed record AddNodeRequest(NodeFunction Function, double CanvasX, double CanvasY);

        /// <summary>Creates a node for a function at a specific canvas position (right-click add).</summary>
        [RelayCommand]
        private void AddNodeAtPosition(AddNodeRequest? request)
        {
            if (request is null)
                return;

            CreateNode(request.Function, false, false, new Point(request.CanvasX, request.CanvasY));
        }

        private void CreateNode(NodeFunction function, bool linkToPrevious, bool linkToElse, Point? canvasPosition)
        {
            TriggerNode node = function.Conditional
                ? new TriggerNodeCondition { Name = _localizationService.Format("IfNodeName", Nodes.Count(n => n.IsCondition) + 1) }
                : new TriggerNodeAction { Name = _localizationService.Format("ActionNodeName", Nodes.Count(n => !n.IsCondition) + 1) };

            node.Size = _options.DefaultNodeWidth;
            node.Function = function.Signature;
            node.FixArguments(function);

            if (canvasPosition is { } position)
                node.ScreenPosition = ToGridPosition(position.X, position.Y);
            else
                PlaceNode(node);

            if (linkToPrevious)
                LinkToSelectedNode(node, linkToElse);

            _event.Nodes.Add(node);
            SyncRootsAndRebuild();
            SelectSingle(Nodes.FirstOrDefault(n => n.Node == node));

            if (SelectedNode is { } created)
                BringNodeIntoViewRequested?.Invoke(created);
        }

        /// <summary>Connects a new node below the selected one (Else first for conditions when requested or when Next is taken).</summary>
        private void LinkToSelectedNode(TriggerNode node, bool elseIfPossible)
        {
            if (SelectedNode?.Node is not { } selected || node.Previous is not null)
                return;

            if (selected is TriggerNodeCondition condition && (elseIfPossible || condition.Next is not null) && condition.Else is null)
                condition.Else = node;
            else if (selected.Next is null)
                selected.Next = node;
            else
                return;

            node.Previous = selected;
        }

        private Vector2 ToGridPosition(double canvasX, double canvasY) => new(
            (float)Math.Clamp(canvasX / _options.GridStep, 0.0, _options.GridSize),
            (float)Math.Clamp(canvasY / _options.GridStep, 0.0, _options.GridSize));

        /// <summary>Picks a non-overlapping position below the selected node (mirrors NodeEditor.GetBestPosition).</summary>
        private void PlaceNode(TriggerNode node)
        {
            const double estimatedHeight = 120.0;
            double width = node.Size;
            double step = _options.GridStep;

            double x, y;
            if (SelectedNode is { } selected)
            {
                x = selected.CanvasLeft + selected.Width / 2.0 - width / 2.0;
                y = selected.CanvasTop + selected.Height + step * 4.0;
            }
            else
            {
                x = step * 40.0;
                y = step * 5.0;
            }

            double limit = _options.CanvasExtent;
            bool colliding = true;
            while (colliding)
            {
                colliding = false;
                var rect = new Rect(x, y, width, estimatedHeight);
                foreach (var other in Nodes)
                {
                    var otherRect = new Rect(other.CanvasLeft - step, other.CanvasTop - step * 2.0, other.Width + step * 2.0, other.Height + step * 4.0);
                    if (rect.IntersectsWith(otherRect))
                    {
                        colliding = true;
                        break;
                    }
                }

                if (colliding)
                    y += step * 4.0;
                if (y > limit)
                    break;
            }

            node.ScreenPosition = ToGridPosition(x, y);
        }

        // Clipboard (shared across events/instances within the session).

        private static List<TriggerNode>? _clipboard;

        public void CopySelected(bool cut)
        {
            if (!HasSelectedNode)
                return;

            // Only chain roots are cloned: Clone() already copies everything linked below a node.
            var selectedModels = _selectedNodes.Select(n => n.Node).ToHashSet();
            _clipboard = selectedModels
                .Where(n => n.Previous is null || !selectedModels.Contains(n.Previous))
                .Select(n =>
                {
                    var clone = n.Clone();
                    clone.Previous = null;
                    return clone;
                })
                .ToList();

            if (cut)
                DeleteSelectedNode();
        }

        public void Paste()
        {
            if (_clipboard is null || _clipboard.Count == 0)
                return;

            var pasted = new List<TriggerNode>();
            foreach (var source in _clipboard)
            {
                var clone = source.Clone();
                clone.Previous = null;

                var position = clone.ScreenPosition;
                clone.ScreenPosition = new Vector2(
                    Math.Clamp(position.X + 4.0f, 0.0f, _options.GridSize),
                    Math.Clamp(position.Y + 4.0f, 0.0f, _options.GridSize));

                _event.Nodes.Add(clone);
                pasted.Add(clone);
            }

            SyncRootsAndRebuild();

            var pastedAll = TriggerNode.LinearizeNodes(pasted).ToHashSet();
            SetSelection(Nodes.Where(n => pastedAll.Contains(n.Node)));
        }

        // Deleting.

        [RelayCommand(CanExecute = nameof(HasSelectedNode))]
        private void DeleteSelectedNode()
        {
            if (!HasSelectedNode)
                return;

            foreach (var node in _selectedNodes.Select(n => n.Node).ToList())
                RemoveFromGraph(node);

            ClearSelection();
            SyncRootsAndRebuild();
        }

        /// <summary>Detaches a node from its neighbours (children become roots) and removes it from the event.</summary>
        private void RemoveFromGraph(TriggerNode node)
        {
            if (node.Previous is not null)
            {
                if (node.Previous.Next == node)
                    node.Previous.Next = null;
                if (node.Previous is TriggerNodeCondition previousCondition && previousCondition.Else == node)
                    previousCondition.Else = null;

                node.Previous = null;
            }

            if (node.Next is not null)
            {
                node.Next.Previous = null;
                _event.Nodes.Add(node.Next);
                node.Next = null;
            }

            if (node is TriggerNodeCondition condition && condition.Else is not null)
            {
                condition.Else.Previous = null;
                _event.Nodes.Add(condition.Else);
                condition.Else = null;
            }

            _event.Nodes.Remove(node);
        }

        /// <summary>Deletes a specific node (context-menu action), selecting it first.</summary>
        [RelayCommand]
        private void DeleteNode(NodeViewModel? node)
        {
            if (node is null)
                return;

            SelectSingle(node);
            DeleteSelectedNode();
        }

        // Node-action toolbar commands (operate on the last selected node unless stated otherwise).

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
            if (node is not null)
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

        /// <summary>Toggles the lock of every selected node.</summary>
        [RelayCommand(CanExecute = nameof(HasSelectedNode))]
        private void LockSelected()
        {
            foreach (var node in _selectedNodes)
                node.IsLocked = !node.IsLocked;
        }

        [RelayCommand(CanExecute = nameof(HasSelectedNode))]
        private void ExportSelected() => CopySelected(false);

        /// <summary>Removes every node in the event (the "Clear" toolbar action).</summary>
        [RelayCommand]
        public void ClearNodes()
        {
            if (Nodes.Count == 0)
                return;

            if (!_messageService.ShowConfirmation(_localizationService["ClearNodesMessage"], _localizationService["ClearNodesTitle"], isRisky: true))
                return;

            _event.Nodes = new List<TriggerNode>();
            ClearSelection();
            SyncRootsAndRebuild();
        }

        /// <summary>Finds a node by its (case-insensitive) name (the "Find" toolbar action) and brings it into view.</summary>
        [RelayCommand]
        private void FindNode()
        {
            var inputBox = new InputBoxWindowViewModel(title: _localizationService["FindNodeTitle"], label: _localizationService["FindNodeLabel"], initialValue: SelectedNode?.Title ?? string.Empty);
            if (_dialogService.ShowDialog(this, inputBox) != true)
                return;

            var node = Nodes.FirstOrDefault(n => n.Title.Contains(inputBox.Value, StringComparison.OrdinalIgnoreCase));
            if (node is null)
            {
                _messageService.ShowInformation(_localizationService.Format("FindNodeNotFound", inputBox.Value), _localizationService["FindNodeTitle"]);
                return;
            }

            SelectSingle(node);
            BringNodeIntoViewRequested?.Invoke(node);
        }

        // Connections.

        /// <summary>Connects <paramref name="from"/>'s Next (or Else) output to <paramref name="to"/>.</summary>
        public void Connect(NodeViewModel from, NodeViewModel to, bool asElse)
        {
            if (from == to || to.Node.Previous is not null || WouldCycle(from.Node, to.Node))
                return;

            if (asElse && from.Node is TriggerNodeCondition condition)
            {
                if (condition.Else is not null)
                    condition.Else.Previous = null;
                condition.Else = to.Node;
            }
            else
            {
                if (from.Node.Next is not null)
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
                if (condition.Else is not null)
                {
                    condition.Else.Previous = null;
                    condition.Else = null;
                }
            }
            else if (from.Node.Next is not null)
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
            {
                if (node == from)
                    return true;
            }

            return false;
        }
    }
}
