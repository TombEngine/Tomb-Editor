#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>
    /// View-model for a single node on the WPF node canvas, wrapping a <see cref="TriggerNode"/>.
    /// Position is stored on the model as a 0-256 grid coordinate where Y grows upwards (as in the previous
    /// node editor, and as the script export orders orphaned chains by it); the canvas maps it to pixels via
    /// <see cref="_gridStep"/> and flips Y.
    /// </summary>
    public partial class NodeViewModel : ObservableObject
    {
        public const double GripWidth = 200.0;
        public const double GripHeight = 6.0;
        public const double ShownGripInset = 8.0;

        private const double ConditionGripWidth = GripWidth / 2.0;

        private static readonly Vector3 GreyBackground = new(60.0f / 255.0f, 63.0f / 255.0f, 65.0f / 255.0f);
        private static readonly Vector3 ConditionTint = new(0.8f, 1.0f, 0.8f);
        private static readonly Vector3 ActionTint = new(0.8f, 0.8f, 1.0f);

        private readonly NodeEditorOptions _options;
        private readonly double _gridStep;
        private readonly string _functionDisplay;
        private readonly NodeFunction? _function;

        public TriggerNode Node { get; }
        public ObservableCollection<ArgumentViewModel> Arguments { get; } = new();
        public event Action? Moved;

        public NodeViewModel(TriggerNode node, NodeEditorOptions options, NodeFunction? function, ArgumentDataProvider provider, EventType eventType)
        {
            Node = node;
            _options = options;
            _gridStep = options.GridStep;
            _functionDisplay = function?.Name ?? node.Function;
            _function = function;
            IsUnsupported = function?.IsUnsupported(eventType) ?? false;

            if (function != null)
            {
                node.FixArguments(function);
                foreach (var layout in function.Arguments)
                {
                    int index = node.Arguments.FindIndex(a => a.Name == layout.Name);
                    if (index >= 0)
                        Arguments.Add(new ArgumentViewModel(node.Arguments, index, layout, provider, node.Size));
                }
            }
        }

        /// <summary>Gets the color a new node starts with: the grey background tinted green for conditions and blue for actions.</summary>
        public static Vector3 GetDefaultColor(bool condition) => GreyBackground * (condition ? ConditionTint : ActionTint);

        public bool IsCondition => Node is TriggerNodeCondition;

        /// <summary>Functions that can be chosen for this node (same kind as the node), shown in the node's function combo.</summary>
        public System.Collections.Generic.IReadOnlyList<NodeFunction> AvailableFunctions { get; set; } = System.Array.Empty<NodeFunction>();

        /// <summary>Raised when the user picks another function in the node's combo.</summary>
        public event Action<NodeViewModel, NodeFunction>? FunctionChangeRequested;

        public NodeFunction? SelectedFunction
        {
            get => _function;
            set
            {
                if (value is null || ReferenceEquals(value, _function))
                    return;

                FunctionChangeRequested?.Invoke(this, value);
            }
        }
        public bool HasArguments => Arguments.Count > 0;

        /// <summary>The node's function is not usable with the event type being edited (#1156).</summary>
        public bool IsUnsupported { get; }

        /// <summary>Gets the text drawn above the node: its name, or a warning when its function is not supported by the event type.</summary>
        public string LabelText => IsUnsupported ? Localizer.Instance["TombEditor.NodeEditor.NotSupported"] : Node.Name;

        /// <summary>Gets the tooltip of the function combo: the function description with line breaks.</summary>
        public string FunctionDescription => TombLib.Utils.TextExtensions.SingleLineToMultiLine(_function?.Description ?? string.Empty);

        public bool ShowGrips => _options.ShowGrips;

        /// <summary>Gets or sets whether any node function exists at all; without one the node shows a notice instead of the combo.</summary>
        public bool HasNodeFunctions { get; set; } = true;

        /// <summary>Gets the margin of the node content, leaving room for the side grip strip when shown.</summary>
        public Thickness ContentMargin => new(8.0 + (_options.ShowGrips ? ShownGripInset : 0.0), 8.0, 8.0, 8.0);

        public bool IsLocked
        {
            get => Node.Locked;
            set
            {
                if (Node.Locked == value) return;
                Node.Locked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditable));
            }
        }

        public bool IsEditable => !Node.Locked;

        public Brush BodyBrush
        {
            get
            {
                var c = Node.Color == Vector3.Zero ? GetDefaultColor(IsCondition) : Node.Color;
                var brush = new SolidColorBrush(Color.FromRgb(ToByte(c.X), ToByte(c.Y), ToByte(c.Z)));
                brush.Freeze();
                return brush;
            }
        }

        public void SetColor(Color color)
        {
            Node.Color = new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            OnPropertyChanged(nameof(BodyBrush));
            Moved?.Invoke();
        }

        public string Title
        {
            get => Node.Name;
            set
            {
                if (Node.Name == value)
                    return;

                Node.Name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LabelText));
            }
        }

        public string FunctionName => string.IsNullOrEmpty(_functionDisplay) ? Localizer.Instance["TombEditor.NodeEditor.NoFunction"] : _functionDisplay;
        public double Width => Node.Size;

        [ObservableProperty] private double _height = 64;
        [ObservableProperty] private bool _isSelected;

        partial void OnHeightChanged(double value) => RaisePorts();

        public double CanvasLeft
        {
            get => Node.ScreenPosition.X * _gridStep;
            set => SetGridPosition(Math.Round(value) / _gridStep, Node.ScreenPosition.Y);
        }

        public double CanvasTop
        {
            get => (_options.GridSize - Node.ScreenPosition.Y) * _gridStep;
            set => SetGridPosition(Node.ScreenPosition.X, _options.GridSize - Math.Round(value) / _gridStep);
        }

        private void SetGridPosition(double gx, double gy)
        {
            gx = Math.Clamp(gx, 0, _options.GridSize);
            gy = Math.Clamp(gy, 0, _options.GridSize);
            Node.ScreenPosition = new Vector2((float)gx, (float)gy);
            OnPropertyChanged(nameof(CanvasLeft));
            OnPropertyChanged(nameof(CanvasTop));
            RaisePorts();
        }

        /// <summary>Gets the grip bar of a connection, relative to the node's top-left corner.</summary>
        public Rect GetGrip(NodeConnection connection)
        {
            double width = Width;
            double bottom = Height - GripHeight;

            return connection switch
            {
                NodeConnection.Previous => new Rect(width / 2.0 - GripWidth / 2.0, 0.0, GripWidth, GripHeight),
                NodeConnection.Next when IsCondition => new Rect(Math.Floor(width / 3.0) - ConditionGripWidth / 2.0, bottom, ConditionGripWidth, GripHeight),
                NodeConnection.Next => new Rect(width / 2.0 - GripWidth / 2.0, bottom, GripWidth, GripHeight),
                _ => new Rect(width - Math.Floor(width / 3.0) - ConditionGripWidth / 2.0, bottom, ConditionGripWidth, GripHeight)
            };
        }

        /// <summary>Gets the horizontal span through the middle of a grip bar, in canvas pixels.</summary>
        public NodeLinkRenderer.Span GetGripSpan(NodeConnection connection)
        {
            var grip = GetGrip(connection);
            return new NodeLinkRenderer.Span(CanvasLeft + grip.Left, CanvasLeft + grip.Right, CanvasTop + grip.Top + grip.Height / 2.0);
        }

        private static byte ToByte(float value) => (byte)Math.Clamp(Math.Round(value * 255.0), 0.0, 255.0);

        private void RaisePorts()
        {
            OnPropertyChanged(nameof(PreviousGrip));
            OnPropertyChanged(nameof(NextGrip));
            OnPropertyChanged(nameof(ElseGrip));
            Moved?.Invoke();
        }

        // Grip bars drawn over the node (relative to the node).

        public Rect PreviousGrip => GetGrip(NodeConnection.Previous);
        public Rect NextGrip => GetGrip(NodeConnection.Next);
        public Rect ElseGrip => GetGrip(NodeConnection.Else);
    }

    /// <summary>A rendered connection (<c>Next</c> or <c>Else</c>) between two nodes.</summary>
    public partial class LinkViewModel : ObservableObject
    {
        public NodeViewModel From { get; }
        public NodeViewModel To { get; }
        public bool IsElse { get; }
        private readonly bool _asRope;

        public LinkViewModel(NodeViewModel from, NodeViewModel to, bool isElse, bool asRope = true)
        {
            _asRope = asRope;
            From = from;
            To = to;
            IsElse = isElse;
            From.Moved += RaiseEndpoints;
            To.Moved += RaiseEndpoints;
        }

        public void Detach()
        {
            From.Moved -= RaiseEndpoints;
            To.Moved -= RaiseEndpoints;
        }

        public Geometry Geometry => NodeLinkRenderer.CreateGeometry(
            From.GetGripSpan(IsElse ? NodeConnection.Else : NodeConnection.Next), To.GetGripSpan(NodeConnection.Previous), _asRope);

        private Brush LinkBrush => NodeLinkRenderer.CreateBrush(GetColor(), IsElse, _asRope, NodeLinkRenderer.ConnectedOpacity);

        public Brush? Stroke => _asRope ? LinkBrush : null;
        public Brush? Fill => _asRope ? null : LinkBrush;
        public double StrokeThickness => NodeLinkRenderer.GetThickness(_asRope);

        private Vector3 GetColor() => From.Node.Color == Vector3.Zero ? NodeViewModel.GetDefaultColor(From.IsCondition) : From.Node.Color;

        private void RaiseEndpoints()
        {
            OnPropertyChanged(nameof(Geometry));
            OnPropertyChanged(nameof(Stroke));
            OnPropertyChanged(nameof(Fill));
        }
    }
}
