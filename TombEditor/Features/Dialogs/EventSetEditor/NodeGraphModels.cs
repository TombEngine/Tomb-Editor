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
    /// Position is stored on the model as a 0-256 grid coordinate; the canvas maps it to pixels via
    /// <see cref="_gridStep"/>.
    /// </summary>
    public partial class NodeViewModel : ObservableObject
    {
        private readonly NodeEditorOptions _options;
        private readonly double _gridStep;
        private readonly string _functionDisplay;

        public TriggerNode Node { get; }
        public ObservableCollection<ArgumentViewModel> Arguments { get; } = new();
        public event Action? Moved;

        public NodeViewModel(TriggerNode node, NodeEditorOptions options, NodeFunction? function, ArgumentDataProvider provider, EventType eventType)
        {
            Node = node;
            _options = options;
            _gridStep = options.GridStep;
            _functionDisplay = function?.Name ?? node.Function;
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

        public bool IsCondition => Node is TriggerNodeCondition;
        public bool HasArguments => Arguments.Count > 0;

        /// <summary>The node's function is not usable with the event type being edited (#1156).</summary>
        public bool IsUnsupported { get; }

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

        public Brush HeaderBrush
        {
            get
            {
                var c = Node.Color;
                Color color = c == Vector3.Zero
                    ? (IsCondition ? Color.FromRgb(110, 84, 56) : Color.FromRgb(60, 84, 110))
                    : Color.FromRgb((byte)(c.X * 255), (byte)(c.Y * 255), (byte)(c.Z * 255));
                return new SolidColorBrush(color);
            }
        }

        public void SetColor(Color color)
        {
            Node.Color = new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
            OnPropertyChanged(nameof(HeaderBrush));
        }

        public string Title
        {
            get => Node.Name;
            set { if (Node.Name == value) return; Node.Name = value; OnPropertyChanged(); }
        }

        public string FunctionName => string.IsNullOrEmpty(_functionDisplay) ? Localizer.Instance["TombEditor.NodeEditor.NoFunction"] : _functionDisplay;
        public double Width => Node.Size;

        [ObservableProperty] private double _height = 64;
        [ObservableProperty] private bool _isSelected;

        partial void OnHeightChanged(double value) => RaisePorts();

        public double CanvasLeft
        {
            get => Node.ScreenPosition.X * _gridStep;
            set => SetGridPosition(value / _gridStep, Node.ScreenPosition.Y);
        }

        public double CanvasTop
        {
            get => Node.ScreenPosition.Y * _gridStep;
            set => SetGridPosition(Node.ScreenPosition.X, value / _gridStep);
        }

        private void SetGridPosition(double gx, double gy)
        {
            // Snap to the nearest grid unit.
            gx = Math.Clamp(Math.Round(gx), 0, _options.GridSize);
            gy = Math.Clamp(Math.Round(gy), 0, _options.GridSize);
            Node.ScreenPosition = new Vector2((float)gx, (float)gy);
            OnPropertyChanged(nameof(CanvasLeft));
            OnPropertyChanged(nameof(CanvasTop));
            RaisePorts();
        }

        // Connection ports (in canvas pixels).
        public double InX => CanvasLeft + Width / 2.0;
        public double InY => CanvasTop;
        public double OutX => CanvasLeft + Width / 2.0;
        public double OutY => CanvasTop + Height;
        public double ElseX => CanvasLeft + Width;
        public double ElseY => CanvasTop + Height / 2.0;

        private void RaisePorts()
        {
            OnPropertyChanged(nameof(InX));
            OnPropertyChanged(nameof(InY));
            OnPropertyChanged(nameof(OutX));
            OnPropertyChanged(nameof(OutY));
            OnPropertyChanged(nameof(ElseX));
            OnPropertyChanged(nameof(ElseY));
            Moved?.Invoke();
        }
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

        public double X1 => IsElse ? From.ElseX : From.OutX;
        public double Y1 => IsElse ? From.ElseY : From.OutY;
        public double X2 => To.InX;
        public double Y2 => To.InY;

        /// <summary>Cubic-bezier "rope" between the output and input ports.</summary>
        public Geometry Geometry
        {
            get
            {
                double dy = _asRope ? Math.Max(40.0, Math.Abs(Y2 - Y1) / 2.0) : 0.0;
                var figure = new PathFigure { StartPoint = new Point(X1, Y1) };
                figure.Segments.Add(new BezierSegment(new Point(X1, Y1 + dy), new Point(X2, Y2 - dy), new Point(X2, Y2), true));
                var geometry = new PathGeometry();
                geometry.Figures.Add(figure);
                geometry.Freeze();
                return geometry;
            }
        }

        private void RaiseEndpoints()
        {
            OnPropertyChanged(nameof(X1));
            OnPropertyChanged(nameof(Y1));
            OnPropertyChanged(nameof(X2));
            OnPropertyChanged(nameof(Y2));
            OnPropertyChanged(nameof(Geometry));
        }
    }
}
