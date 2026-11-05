#nullable enable

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using TombLib.WPF;
using TombLib.WPF.Input;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>
    /// Pure-WPF visual-scripting node canvas: draggable action/condition nodes, Next/Else links drawn as ribbons or
    /// ropes, create-from-function-list, delete, and grip-drag to (re)connect nodes.
    /// </summary>
    public partial class NodeEditorView : UserControl
    {
        private const double WheelScrollFactor = 0.04;
        private const double ContextMenuDragThreshold = 4.0;

        private readonly RubberBandSelection _rubberBand;
        private Vector _dragTotal;

        // Link drag started from a node grip.
        private TombLib.LevelData.VisualScripting.TriggerNode? _linkNode;
        private NodeConnection _linkMode;
        private Point _linkStart;
        private bool _linkMoved;
        private Path? _linkVisual;

        // Right-button panning.
        private bool _panning;
        private Point _panStart;
        private Point _panLast;
        private double _panStartHorizontal;
        private double _panStartVertical;

        private bool _viewPositionTracked;

        public NodeEditorView()
        {
            InitializeComponent();
            _rubberBand = new RubberBandSelection(rootCanvas, selectionRectangle);
            DataContextChanged += OnDataContextChanged;
            canvasScroll.ScrollChanged += OnScrollChanged;
        }

        private NodeEditorViewModel? ViewModel => DataContext as NodeEditorViewModel;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewPositionTracked = false;

            // Scrolling a node into view is the only visual concern the view-model needs from us.
            if (e.OldValue is NodeEditorViewModel oldViewModel)
            {
                oldViewModel.BringNodeIntoViewRequested -= ScrollToNode;
                oldViewModel.ViewportProvider = null;
            }

            if (e.NewValue is NodeEditorViewModel newViewModel)
            {
                newViewModel.BringNodeIntoViewRequested += ScrollToNode;
                newViewModel.ViewportProvider = GetViewport;
                UpdateGridBrush(newViewModel);

                // Start the view where the event was left, or over the event's existing nodes. Deferred so
                // the ScrollViewer has a measured viewport (a new editor is created for every event switch).
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, RestoreView);
            }
        }

        /// <summary>Draws the grid lines one cell apart, over the grid area only.</summary>
        private void UpdateGridBrush(NodeEditorViewModel viewModel)
        {
            double step = viewModel.GridStep;

            // WinForms draws the canvas in DarkBackground with grid lines 15% brighter.
            var background = TryFindResource("DarkBackgroundColor") is Color themeColor ? themeColor : Color.FromRgb(0x2B, 0x2B, 0x2B);
            var lineColor = Color.FromRgb((byte)(background.R * 1.15), (byte)(background.G * 1.15), (byte)(background.B * 1.15));
            rootCanvas.Background = new SolidColorBrush(background);
            gridRectangle.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

            var drawing = new GeometryDrawing
            {
                Geometry = new RectangleGeometry(new Rect(0.0, 0.0, step, step)),
                Pen = new Pen(new SolidColorBrush(lineColor), 1.0)
            };

            var brush = new DrawingBrush(drawing)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0.0, 0.0, step, step),
                ViewportUnits = BrushMappingMode.Absolute
            };
            brush.Freeze();

            gridRectangle.Fill = brush;
        }

        private Rect GetViewport()
        {
            double scale = zoomTransform.ScaleX;
            return new Rect(canvasScroll.HorizontalOffset / scale, canvasScroll.VerticalOffset / scale,
                            canvasScroll.ViewportWidth / scale, canvasScroll.ViewportHeight / scale);
        }

        /// <summary>Restores the saved view position of the event, or centers on the grid when it has none (as WinForms does).</summary>
        private void RestoreView()
        {
            var viewModel = ViewModel;
            if (viewModel is null)
                return;

            if (canvasScroll.ViewportWidth <= 0.0 || canvasScroll.ViewportHeight <= 0.0)
            {
                canvasScroll.UpdateLayout();
                if (canvasScroll.ViewportWidth <= 0.0)
                    return;
            }

            if (viewModel.SavedViewPosition is { } saved)
                CenterOn(saved.X * viewModel.GridStep, (viewModel.GridSize - saved.Y) * viewModel.GridStep);
            else
                CenterOn(viewModel.GridSize * viewModel.GridStep / 2.0, viewModel.GridSize * viewModel.GridStep / 2.0);

            // Only scrolling after the initial positioning is remembered, like the previous editor did.
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => _viewPositionTracked = true));
        }

        /// <summary>Scrolls so the bounding box of all nodes is centered in the viewport.</summary>
        private void CenterOnNodes(NodeEditorViewModel viewModel)
        {
            if (viewModel.Nodes.Count == 0)
            {
                CenterOn(viewModel.GridSize * viewModel.GridStep / 2.0, viewModel.GridSize * viewModel.GridStep / 2.0);
                return;
            }

            double minX = double.PositiveInfinity, minY = double.PositiveInfinity;
            double maxX = double.NegativeInfinity, maxY = double.NegativeInfinity;

            foreach (var node in viewModel.Nodes)
            {
                minX = Math.Min(minX, node.CanvasLeft);
                minY = Math.Min(minY, node.CanvasTop);
                maxX = Math.Max(maxX, node.CanvasLeft + node.Width);
                maxY = Math.Max(maxY, node.CanvasTop + node.Height);
            }

            CenterOn((minX + maxX) / 2.0, (minY + maxY) / 2.0);
        }

        private void CenterOn(double canvasX, double canvasY)
        {
            double scale = zoomTransform.ScaleX;
            canvasScroll.ScrollToHorizontalOffset(canvasX * scale - canvasScroll.ViewportWidth / 2.0);
            canvasScroll.ScrollToVerticalOffset(canvasY * scale - canvasScroll.ViewportHeight / 2.0);
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!_viewPositionTracked || ViewModel is not { } viewModel)
                return;

            // The view position is the world point at the center of the view (Y grows upwards).
            var viewport = GetViewport();
            viewModel.SavedViewPosition = new System.Numerics.Vector2(
                (float)((viewport.Left + viewport.Width / 2.0) / viewModel.GridStep),
                (float)(viewModel.GridSize - (viewport.Top + viewport.Height / 2.0) / viewModel.GridStep));
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = GetDroppedLuaObject(e) is not null ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (ViewModel is not { } viewModel || GetDroppedLuaObject(e) is not { } item)
                return;

            viewModel.CreateNodeForObject(item, e.GetPosition(rootCanvas));
            e.Handled = true;
        }

        private static TombLib.LevelData.PositionAndScriptBasedObjectInstance? GetDroppedLuaObject(DragEventArgs e)
        {
            var formats = e.Data.GetFormats();
            return formats.Length > 0 ? e.Data.GetData(formats[0]) as TombLib.LevelData.PositionAndScriptBasedObjectInstance : null;
        }
        private void OnArgDragOver(object sender, DragEventArgs e)
        {
            bool accepts = sender is FrameworkElement fe && fe.DataContext is ArgumentViewModel arg && arg.AcceptsDrop;
            e.Effects = accepts ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnArgDrop(object sender, DragEventArgs e)
        {
            if (sender is not FrameworkElement fe || fe.DataContext is not ArgumentViewModel arg)
                return;

            var formats = e.Data.GetFormats();
            if (formats.Length > 0)
                arg.HandleDrop(e.Data.GetData(formats[0]));

            e.Handled = true;
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                ZoomAtCursor(e);
                return;
            }

            // The wheel scrolls vertically, or horizontally with Shift. Over a node it keeps its normal job.
            if (ViewModel is not { } viewModel || FindNodeAt(e.GetPosition(rootCanvas)) is not null)
                return;

            double delta = e.Delta * WheelScrollFactor * viewModel.GridStep;

            if (Keyboard.Modifiers == ModifierKeys.Shift)
                canvasScroll.ScrollToHorizontalOffset(canvasScroll.HorizontalOffset + delta);
            else
                canvasScroll.ScrollToVerticalOffset(canvasScroll.VerticalOffset - delta);

            e.Handled = true;
        }

        private void ZoomAtCursor(MouseWheelEventArgs e)
        {
            e.Handled = true;

            double oldScale = zoomTransform.ScaleX;
            double factor = e.Delta > 0 ? 1.1 : 1.0 / 1.1;
            double newScale = Math.Clamp(oldScale * factor, 0.3, 2.0);
            if (newScale == oldScale)
                return;

            // Anchor the zoom on the cursor: keep the canvas point under the mouse stationary
            // by compensating the scroll offsets for the scale change.
            Point viewportPos = e.GetPosition(canvasScroll);
            double anchorX = (canvasScroll.HorizontalOffset + viewportPos.X) / oldScale;
            double anchorY = (canvasScroll.VerticalOffset + viewportPos.Y) / oldScale;

            zoomTransform.ScaleX = newScale;
            zoomTransform.ScaleY = newScale;

            // The new extent must be measured before the offsets can reach it.
            canvasScroll.UpdateLayout();
            canvasScroll.ScrollToHorizontalOffset((anchorX * newScale) - viewportPos.X);
            canvasScroll.ScrollToVerticalOffset((anchorY * newScale) - viewportPos.Y);
        }

        /// <summary>Lets the host window route its keys here, as WinForms processes them form-wide (N, A, C, L, Ctrl+A, Esc ...).</summary>
        public bool TryHandleWindowKey(KeyEventArgs e)
        {
            if (ViewModel is null || KeyboardInput.IsEditingText(e.OriginalSource as DependencyObject))
                return false;

            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            return NodeEditorKeyMap.TryHandle(ViewModel, key, Keyboard.Modifiers);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Shortcuts must not fire while a node argument or the call counter is being edited.
            if (ViewModel is null || KeyboardInput.IsEditingText(e.OriginalSource as DependencyObject))
                return;

            // Alt+key arrives as Key.System with the real key in SystemKey.
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (NodeEditorKeyMap.TryHandle(ViewModel, key, Keyboard.Modifiers))
                e.Handled = true;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel is null)
                return;

            var position = e.GetPosition(rootCanvas);

            // Clicks on nodes are handled by the nodes themselves.
            if (FindNodeAt(position) is not null || _linkNode is not null)
                return;

            Focus();

            // Double-clicking an empty place opens the function list to create a node there.
            if (e.ClickCount == 2)
            {
                OpenNodeMenu(position);
                e.Handled = true;
                return;
            }

            _rubberBand.Begin(ViewModel, position);
            e.Handled = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_panning)
            {
                PanTo(e);
                return;
            }

            if (ViewModel is not null && _rubberBand.IsActive)
                _rubberBand.Update(ViewModel, e.GetPosition(rootCanvas));
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_rubberBand.IsActive)
                _rubberBand.End();
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Right-click on a node falls through to that node's own context menu.
            if (ViewModel is null || FindNodeAt(e.GetPosition(rootCanvas)) is not null)
                return;

            Focus();

            _panning = true;
            _panStart = _panLast = e.GetPosition(canvasScroll);
            _panStartHorizontal = canvasScroll.HorizontalOffset;
            _panStartVertical = canvasScroll.VerticalOffset;

            rootCanvas.CaptureMouse();
            e.Handled = true;
        }

        private void PanTo(MouseEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed)
            {
                EndPan();
                return;
            }

            // The canvas follows the cursor, as if grabbed.
            _panLast = e.GetPosition(canvasScroll);
            canvasScroll.ScrollToHorizontalOffset(_panStartHorizontal - (_panLast.X - _panStart.X));
            canvasScroll.ScrollToVerticalOffset(_panStartVertical - (_panLast.Y - _panStart.Y));
        }

        private void EndPan()
        {
            _panning = false;
            rootCanvas.ReleaseMouseCapture();
        }

        private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_panning)
                return;

            // A right-click without dragging opens the function list; dragging only pans.
            bool clicked = (_panLast - _panStart).Length < ContextMenuDragThreshold;
            EndPan();

            if (clicked)
                OpenNodeMenu(e.GetPosition(rootCanvas));

            e.Handled = true;
        }

        private void OpenNodeMenu(Point position)
        {
            if (ViewModel is not { } viewModel)
                return;

            // The menu is composed here because it depends on the click position; every item
            // delegates the actual work to the view-model's AddNodeAtPositionCommand.
            var menu = new ContextMenu();
            foreach (var group in viewModel.Functions.GroupBy(f => f.Section))
            {
                // "__" so underscores in script-defined names display literally (not access keys).
                var section = new MenuItem { Header = string.IsNullOrEmpty(group.Key) ? Localizer.Instance["TombEditor.NodeEditor.Misc"] : group.Key.Replace("_", "__") };

                // Conditions are listed before actions inside every section.
                foreach (var function in group.OrderBy(f => !f.Conditional))
                {
                    section.Items.Add(new MenuItem
                    {
                        Header = function.Name?.Replace("_", "__"),
                        Command = viewModel.AddNodeAtPositionCommand,
                        CommandParameter = new NodeEditorViewModel.AddNodeRequest(function, position.X, position.Y)
                    });
                }

                menu.Items.Add(section);
            }

            menu.IsOpen = true;
        }

        private void Node_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            if (sender is not FrameworkElement { DataContext: NodeViewModel node } || ViewModel is null)
                return;

            // Clicking an argument editor inside the node leaves the selection alone; only the node surface selects it.
            if (IsInsideEditorControl(e.OriginalSource as DependencyObject, sender))
                return;

            var modifiers = Keyboard.Modifiers;
            ViewModel.ClickNode(node, modifiers.HasFlag(ModifierKeys.Control), modifiers.HasFlag(ModifierKeys.Shift));
        }

        private static bool IsInsideEditorControl(DependencyObject? source, object nodeElement)
        {
            for (var element = source; element is not null && element != nodeElement; element = VisualTreeHelper.GetParent(element))
            {
                if (element is Thumb)
                    return false;

                if (element is Control)
                    return true;
            }

            return false;
        }

        /// <summary>Scrolls a node into view when it is not fully visible, putting the middle of its top edge at the view center.</summary>
        private void ScrollToNode(NodeViewModel node)
        {
            // Deferred so the node has been measured and its real height is known.
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                var bounds = new Rect(node.CanvasLeft, node.CanvasTop, node.Width, node.Height);
                if (GetViewport().Contains(bounds))
                    return;

                CenterOn(node.CanvasLeft + node.Width / 2.0, node.CanvasTop);
            }));
        }

        /// <summary>Right-click on a node opens the searchable function list of that node.</summary>
        private void Node_PreviewMouseRightUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DependencyObject root || FindDescendant<DarkUI.WPF.CustomControls.SearchableComboBox>(root, "functionCombo") is not { } combo || !combo.IsEnabled)
                return;

            // Opened after the right button is released, otherwise the popup is dismissed by that same click.
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => TombLib.WPF.ComboSearch.OpenCommand.Execute(combo)));
            e.Handled = true;
        }

        private static T? FindDescendant<T>(DependencyObject root, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match && match.Name == name)
                    return match;

                if (FindDescendant<T>(child, name) is { } found)
                    return found;
            }

            return null;
        }
        private void Node_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is NodeViewModel node)
                node.Height = e.NewSize.Height;
        }

        private void NodeThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            _dragTotal = new Vector();

            if (sender is FrameworkElement { DataContext: NodeViewModel node } && ViewModel is not null)
                ViewModel.BeginMove(node);
        }

        private void NodeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // The total offset is applied to the starting positions, so slow drags are not lost to rounding.
            _dragTotal += new Vector(e.HorizontalChange, e.VerticalChange);
            ViewModel?.MoveBy(_dragTotal.X, _dragTotal.Y);
        }

        private void Grip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: NodeViewModel node, Tag: string tag } || !node.IsEditable)
                return;

            e.Handled = true;
            _linkNode = node.Node;
            _linkMode = Enum.Parse<NodeConnection>(tag);
            _linkStart = e.GetPosition(rootCanvas);
            _linkMoved = false;

            // Capture on the canvas (reliable) rather than the tiny grip bar.
            rootCanvas.CaptureMouse();
            rootCanvas.MouseMove += Canvas_LinkMove;
            rootCanvas.MouseLeftButtonUp += Canvas_LinkUp;
        }

        private void Canvas_LinkMove(object sender, MouseEventArgs e)
        {
            if (_linkNode is null || ViewModel is not { } viewModel)
                return;

            var position = e.GetPosition(rootCanvas);

            // The link is only picked up once the mouse actually moves, so a plain click on a grip changes nothing.
            if (!_linkMoved)
            {
                if (position == _linkStart)
                    return;

                _linkMoved = true;
                viewModel.BeginLinkDrag(_linkNode, _linkMode);

                // Inserted below the rubber band and the nodes, since links are drawn under the nodes.
                _linkVisual = new Path { IsHitTestVisible = false, StrokeThickness = NodeLinkRenderer.GetThickness(viewModel.LinksAsRopes) };
                rootCanvas.Children.Insert(rootCanvas.Children.IndexOf(selectionRectangle), _linkVisual);
            }

            if (viewModel.Nodes.FirstOrDefault(n => n.Node == _linkNode) is not { } source)
                return;

            var end = new NodeLinkRenderer.Span(position.X, position.X, position.Y);
            var brush = NodeLinkRenderer.CreateBrush(source.Node.Color == System.Numerics.Vector3.Zero ? NodeViewModel.GetDefaultColor(source.IsCondition) : source.Node.Color,
                _linkMode == NodeConnection.Else, viewModel.LinksAsRopes, NodeLinkRenderer.PendingOpacity);

            _linkVisual!.Data = NodeLinkRenderer.CreateGeometry(source.GetGripSpan(_linkMode), end, viewModel.LinksAsRopes);
            _linkVisual.Stroke = viewModel.LinksAsRopes ? brush : null;
            _linkVisual.Fill = viewModel.LinksAsRopes ? null : brush;
        }

        private void Canvas_LinkUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(rootCanvas);

            if (ViewModel is { } viewModel && _linkNode is not null && _linkMoved)
                viewModel.CompleteLinkDrag(_linkNode, _linkMode, FindNodeAt(position), position);

            EndLinkDrag();
        }

        private void EndLinkDrag()
        {
            if (_linkVisual is not null)
                rootCanvas.Children.Remove(_linkVisual);

            _linkVisual = null;
            _linkNode = null;

            rootCanvas.ReleaseMouseCapture();
            rootCanvas.MouseMove -= Canvas_LinkMove;
            rootCanvas.MouseLeftButtonUp -= Canvas_LinkUp;
        }

        private NodeViewModel? FindNodeAt(Point position)
        {
            if (ViewModel == null)
                return null;

            // Geometric search in canvas coordinates. Visual-tree hit-testing is unreliable here because
            // the nodes live in a zero-sized items panel. Iterate back-to-front so the topmost node wins.
            for (int i = ViewModel.Nodes.Count - 1; i >= 0; i--)
            {
                var node = ViewModel.Nodes[i];
                if (position.X >= node.CanvasLeft && position.X <= node.CanvasLeft + node.Width &&
                    position.Y >= node.CanvasTop && position.Y <= node.CanvasTop + node.Height)
                    return node;
            }
            return null;
        }
    }
}
