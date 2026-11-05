#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
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
using TombLib.Utils;
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

        /// <summary>Asks the view to scroll the given node into view (a purely visual concern).</summary>
        public event Action<NodeViewModel>? BringNodeIntoViewRequested;

        /// <summary>Raised to show a transient information toast next to the editor.</summary>
        public event Action<string>? MessageRequested;

        /// <summary>Supplied by the view: the visible part of the canvas, in canvas pixels (used to place new nodes).</summary>
        public Func<Rect>? ViewportProvider { get; set; }

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
        public double GridStep => _options.GridStep;
        public int GridSize => _options.GridSize;
        public bool LinksAsRopes => _options.LinksAsRopes;

        /// <summary>Gets or sets the world point at the center of the view (Y grows upwards), or null when the event never had one saved.</summary>
        public Vector2? SavedViewPosition
        {
            get => _event.NodePosition.X == float.MaxValue ? null : _event.NodePosition;
            set => _event.NodePosition = value ?? new Vector2(float.MaxValue);
        }

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
                var vm = new NodeViewModel(node, _options, function, _provider, _eventType)
                {
                    AvailableFunctions = Functions.Where(f => f.Conditional == (node is TriggerNodeCondition)).ToList(),
                    HasNodeFunctions = Functions.Count > 0
                };
                vm.FunctionChangeRequested += OnNodeFunctionChangeRequested;
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

        /// <summary>Replaces the function of a node (chosen in the node's combo) and resets its arguments to the defaults.</summary>
        private void OnNodeFunctionChangeRequested(NodeViewModel nodeViewModel, NodeFunction function)
        {
            nodeViewModel.Node.Function = function.Signature;
            nodeViewModel.Node.Arguments.Clear();
            nodeViewModel.Node.FixArguments(function);

            // Rebuilding inside the combo's own selection change would dispose the combo, so defer it.
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(Rebuild));
        }

        private void SyncRootsAndRebuild(IEnumerable<TriggerNode>? knownNodes = null)
        {
            var all = knownNodes ?? TriggerNode.LinearizeNodes(_event.Nodes);
            _event.Nodes = all.Where(n => n.Previous is null).ToList();
            Rebuild();
        }

        // Node creation.

        [RelayCommand]
        private void AddActionNode() => AddNodeFromToolbar(false);

        [RelayCommand]
        private void AddConditionNode() => AddNodeFromToolbar(true);

        /// <summary>Toolbar Action / Condition buttons: Ctrl skips linking to the selected node, Alt links through Else.</summary>
        private void AddNodeFromToolbar(bool condition)
        {
            var modifiers = Keyboard.Modifiers;
            AddNodeOfKind(condition, modifiers != ModifierKeys.Control, modifiers == ModifierKeys.Alt);
        }

        /// <summary>Adds an action or condition node with the first function of its kind (a blank node when there is none).</summary>
        public void AddNodeOfKind(bool condition, bool linkToPrevious, bool linkToElse)
            => CreateNode(condition, Functions.FirstOrDefault(f => f.Conditional == condition), linkToPrevious, linkToElse, null);

        /// <summary>Parameter of <see cref="AddNodeAtPositionCommand"/>: a function plus the canvas position to drop it at.</summary>
        public sealed record AddNodeRequest(NodeFunction Function, double CanvasX, double CanvasY);

        /// <summary>Creates a node for a function at a specific canvas position (right-click or double-click add); Alt links through Else.</summary>
        [RelayCommand]
        private void AddNodeAtPosition(AddNodeRequest? request)
        {
            if (request is null)
                return;

            CreateNode(request.Function.Conditional, request.Function, true, Keyboard.Modifiers == ModifierKeys.Alt, new Point(request.CanvasX, request.CanvasY));
        }

        private void CreateNode(bool condition, NodeFunction? function, bool linkToPrevious, bool linkToElse, Point? canvasPosition)
        {
            TriggerNode node = condition
                ? new TriggerNodeCondition { Name = _localizationService.Format("IfNodeName", Nodes.Count(n => n.IsCondition) + 1) }
                : new TriggerNodeAction { Name = _localizationService.Format("ActionNodeName", Nodes.Count(n => !n.IsCondition) + 1) };

            node.Color = NodeViewModel.GetDefaultColor(condition);
            node.Size = _options.DefaultNodeWidth;

            if (function is not null)
            {
                node.Function = function.Signature;
                node.FixArguments(function);
            }

            if (canvasPosition is { } position)
                node.ScreenPosition = ToGridPosition(position.X, position.Y);
            else
                PlaceNode(node);

            if (linkToPrevious)
                LinkToSelectedNode(node, linkToElse);

            _event.Nodes.Add(node);
            SyncRootsAndRebuild();
            SelectSingle(Nodes.FirstOrDefault(n => n.Node == node));

            if (canvasPosition is null && SelectedNode is { } created)
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

        // Canvas Y grows downwards while the stored Y grows upwards.
        private Vector2 ToGridPosition(double canvasX, double canvasY) => new(
            (float)Math.Clamp(canvasX / _options.GridStep, 0.0, _options.GridSize),
            (float)Math.Clamp(_options.GridSize - canvasY / _options.GridStep, 0.0, _options.GridSize));

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
                y = selected.CanvasTop + selected.Height;
            }
            else
            {
                // The previous editor mixed view coordinates with pixels here, so a new node lands near the view's top-left corner.
                var viewport = ViewportProvider?.Invoke() ?? new Rect(0.0, 0.0, 0.0, 0.0);
                double viewX = (viewport.Left + viewport.Width / 2.0) / step;
                double viewY = _options.GridSize - (viewport.Top + viewport.Height / 2.0) / step;
                x = viewport.Left + viewX - width / (2.0 * step);
                y = viewport.Top + viewY;
            }

            double limit = _options.CanvasExtent;
            double marginX = Math.Floor(step / 2.0);
            double marginY = Math.Floor(step) * 2.0;
            bool colliding = true;
            while (colliding)
            {
                y += Math.Floor(step * 4.0);
                colliding = false;

                var rect = new Rect(x, y, width, estimatedHeight);
                rect.Inflate(marginX, marginY);

                foreach (var other in Nodes)
                {
                    var otherRect = new Rect(other.CanvasLeft, other.CanvasTop, other.Width, other.Height);
                    otherRect.Inflate(marginX, marginY);

                    if (rect.IntersectsWith(otherRect))
                    {
                        colliding = true;
                        break;
                    }
                }

                if (y > limit)
                    break;
            }

            node.ScreenPosition = ToGridPosition(x, y);
        }

        // Clipboard (shared across events/instances within the session).

        private static List<TriggerNode>? _clipboard;

        public void CopySelected(bool cut)
        {
            // Only chain roots are cloned: Clone() already copies everything linked below a node.
            var selectedModels = _selectedNodes.Select(n => n.Node).ToHashSet();
            var copied = selectedModels
                .Where(n => n.Previous is null || !selectedModels.Contains(n.Previous))
                .Select(n =>
                {
                    var clone = n.Clone();
                    clone.Previous = null;
                    return clone;
                })
                .ToList();

            if (cut)
            {
                _clipboard = copied;
                DeleteSelectedNode();
            }
            else if (copied.Count > 0)
            {
                _clipboard = copied;
                MessageRequested?.Invoke(_localizationService["NodesCopied"]);
            }
        }

        public void Paste()
        {
            if (_clipboard is null || _clipboard.Count == 0)
                return;

            // A pasted copy that would land exactly on an existing chain is nudged right and down.
            var offset = new Vector2(_options.GridSize / 64.0f, -_options.GridSize / 64.0f);

            foreach (var source in _clipboard)
            {
                if (_event.Nodes.Any(n => n.ScreenPosition == source.ScreenPosition))
                    source.ScreenPosition += offset;

                var clone = source.Clone();
                clone.Previous = null;
                _event.Nodes.Add(clone);
            }

            SyncRootsAndRebuild();

            if (Nodes.FirstOrDefault(n => n.Node == _event.Nodes.FirstOrDefault()) is { } first)
                BringNodeIntoViewRequested?.Invoke(first);
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
            var current = (node.BodyBrush as SolidColorBrush)?.Color ?? Colors.Gray;
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

        /// <summary>Gets whether the event has any node (enables Clear and Export).</summary>
        public bool HasNodes => Nodes.Count > 0;

        /// <summary>Exports the whole node graph as a Lua function to the clipboard.</summary>
        [RelayCommand(CanExecute = nameof(HasNodes))]
        private void Export()
        {
            var script = ScriptingUtils.ParseNodes(_event.Nodes, "ExportedNodeFunction");
            if (string.IsNullOrEmpty(script))
                return;

            try
            {
                Clipboard.SetText(script);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Node graph could not be copied to the clipboard.");
                return;
            }

            MessageRequested?.Invoke(_localizationService["ExportDone"]);
        }

        /// <summary>Removes every node in the event after asking for confirmation (the "Clear" toolbar action).</summary>
        [RelayCommand(CanExecute = nameof(HasNodes))]
        private void ClearNodes()
        {
            if (_messageService.ShowConfirmation(_localizationService["ClearNodesMessage"], _localizationService["ClearNodesTitle"], isRisky: true))
                ClearAllNodes();
        }

        /// <summary>Removes every node in the event without asking (the Shift+Delete shortcut).</summary>
        public void ClearAllNodes()
        {
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

            // Nothing happens when no node matches, like the previous editor.
            var node = Nodes.FirstOrDefault(n => n.Title.Contains(inputBox.Value, StringComparison.OrdinalIgnoreCase));
            if (node is null)
                return;

            SelectSingle(node);
            BringNodeIntoViewRequested?.Invoke(node);
        }

        // Connections. A link is dragged from a grip: picking up a grip detaches its current link, and
        // dropping on an invalid target leaves the node detached (as in the previous editor).

        /// <summary>Detaches whatever is linked at the given grip of a node when a link drag starts from it.</summary>
        public void BeginLinkDrag(TriggerNode node, NodeConnection connection)
        {
            // Collected before detaching: the node that loses its link is no longer reachable from the roots and must stay in the event.
            var allNodes = TriggerNode.LinearizeNodes(_event.Nodes).ToList();

            switch (connection)
            {
                case NodeConnection.Previous:
                    DetachFromParent(node);
                    break;

                case NodeConnection.Next when node.Next is not null:
                    node.Next.Previous = null;
                    node.Next = null;
                    break;

                case NodeConnection.Else when node is TriggerNodeCondition { Else: not null } condition:
                    condition.Else.Previous = null;
                    condition.Else = null;
                    break;
            }

            SyncRootsAndRebuild(allNodes);
        }

        /// <summary>Gets whether a link dragged from <paramref name="dragMode"/> of a node may end on the given grip of the target.</summary>
        private static bool CanConnect(TriggerNode dragged, NodeConnection dragMode, TriggerNode target, NodeConnection targetMode)
        {
            if (dragged == target || targetMode == dragMode)
                return false;

            // Next and Else grips never connect to each other.
            if (targetMode != NodeConnection.Previous && dragMode != NodeConnection.Previous)
                return false;

            if (WouldCycle(dragged, target, targetMode != NodeConnection.Previous))
                return false;

            return targetMode switch
            {
                NodeConnection.Next => target.Next is null,
                NodeConnection.Previous => target.Previous is null,
                _ => target is TriggerNodeCondition { Else: null }
            };
        }

        /// <summary>Ends a link drag on another node: picks the grip nearest to the drop point that accepts the link and connects.</summary>
        public void CompleteLinkDrag(TriggerNode dragged, NodeConnection dragMode, NodeViewModel? target, double dropX)
        {
            if (target is null || !target.IsEditable)
                return;

            IEnumerable<NodeConnection> candidates = dragMode == NodeConnection.Previous
                ? new[] { NodeConnection.Next, NodeConnection.Else }.OrderBy(m => Math.Abs(dropX - target.GetGripSpan(m).Left - target.GetGrip(m).Width / 2.0))
                : [NodeConnection.Previous];

            foreach (var targetMode in candidates)
            {
                if (!CanConnect(dragged, dragMode, target.Node, targetMode))
                    continue;

                Connect(dragged, dragMode, target.Node, targetMode);
                return;
            }
        }

        private void Connect(TriggerNode dragged, NodeConnection dragMode, TriggerNode target, NodeConnection targetMode)
        {
            switch (targetMode)
            {
                case NodeConnection.Previous:
                    if (dragged is TriggerNodeCondition condition && dragMode == NodeConnection.Else)
                        condition.Else = target;
                    else
                        dragged.Next = target;

                    target.Previous = dragged;
                    break;

                case NodeConnection.Next:
                    dragged.Previous = target;
                    target.Next = dragged;
                    break;

                case NodeConnection.Else when target is TriggerNodeCondition targetCondition:
                    dragged.Previous = target;
                    targetCondition.Else = dragged;
                    break;
            }

            SyncRootsAndRebuild();
        }

        private static void DetachFromParent(TriggerNode node)
        {
            if (node.Previous is not { } parent)
                return;

            if (parent.Next == node)
                parent.Next = null;
            if (parent is TriggerNodeCondition parentCondition && parentCondition.Else == node)
                parentCondition.Else = null;

            node.Previous = null;
        }

        /// <summary>Walks the chain from <paramref name="target"/> (backwards, or forwards through Next and Else) looking for <paramref name="incoming"/>.</summary>
        private static bool WouldCycle(TriggerNode incoming, TriggerNode target, bool backwards)
        {
            if (incoming == target)
                return true;

            if (backwards)
                return target.Previous is not null && WouldCycle(incoming, target.Previous, backwards);

            return (target.Next is not null && WouldCycle(incoming, target.Next, backwards)) ||
                   (target is TriggerNodeCondition { Else: not null } condition && WouldCycle(incoming, condition.Else, backwards));
        }
    }
}
