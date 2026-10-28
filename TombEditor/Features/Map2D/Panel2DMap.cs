using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad.Catalog;
using TombLib.WPF;

#nullable disable


namespace TombEditor.Features.Map2D
{
    /// <summary>
    /// WPF port of the 2D map view. Draws the level layout from the top down, hosts the
    /// <see cref="DepthBar"/> on the right edge, and supports room selection, dragging,
    /// depth probes and copy/paste of rooms. Replaces the previous <c>Panel2DMap</c>.
    /// </summary>
    public partial class Panel2DMap : FrameworkElement, IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Vector2 ViewPosition { get; set; } = new Vector2(60.0f, 60.0f);

        public float ViewScale
        {
            get { return _viewScale; }
            set
            {
                value = Math.Min(value, _editor.Configuration.Map2D_NavigationMaxZoom);
                value = Math.Max(value, _editor.Configuration.Map2D_NavigationMinZoom);
                _viewScale = value;
            }
        }
        private float _viewScale = 6.0f;

        private readonly DepthBar _depthBar;
        private readonly Editor _editor;
        private Room _roomMouseClicked;
        private HashSet<Room> _roomsToMove; // Set to a valid list only if room dragging is active
        private Vector2 _roomMouseOffset; // Relative vector to the position of the room for where it was clicked.
        private Vector2? _viewMoveMouseWorldCoord;
        private int? _currentlyEditedDepthProbeIndex;
        private Point _lastMousePosition;
        private IReadOnlyList<RoomClipboardData.ContourLine> _insertionContourLineData;
        private Vector2 _insertionDropPosition;
        private VectorInt2 _insertionCurrentOffset;
        private Point _startMousePosition;
        private VectorInt3 _overallDelta;
        private bool _viewInitialized;

        // Keyboard navigation (replicates the previous MovementTimer acceleration).
        private readonly DispatcherTimer _movementTimer;
        private const float _moveAcceleration = 0.02f;
        private System.Windows.Input.Key _moveKey = System.Windows.Input.Key.None;
        private float _moveMultiplier;

        private int _mapSize = 100;

        private class SelectionArea
        {
            public Rectangle2 _area;
            public HashSet<Room> _roomSelectionCache;
            public HashSet<Room> GetRoomSelection(Panel2DMap parent)
            {
                if (_roomSelectionCache == null && _area.Size.Length() > 0.5f)
                    _roomSelectionCache = new HashSet<Room>(
                        parent.BoolCombine(parent._editor.SelectedRooms,
                        parent._editor.Level.ExistingRooms
                        .Where(room => parent._depthBar.CheckRoom(room))
                        .Where(room =>
                            room.Position.X + room.NumXSectors > Math.Min(_area.Start.X, _area.End.X) &&
                            room.Position.Z + room.NumZSectors > Math.Min(_area.Start.Y, _area.End.Y) &&
                            room.Position.X < Math.Max(_area.Start.X, _area.End.X) &&
                            room.Position.Z < Math.Max(_area.Start.Y, _area.End.Y))));
                return _roomSelectionCache;
            }
        }
        private SelectionArea _selectionArea;

        private Brush _roomsNormalBrush;
        private Brush _roomsNormalAboveBrush;
        private Brush _roomsNormalBelowBrush;
        private Brush _roomsSelectedBrush;
        private Brush _roomsMovedBrush;
        private Color _roomsSelectedColor;

        private static readonly Brush _roomsLockedBrush = CreateHatchBrush(Color.FromArgb(50, 20, 20, 20));
        private static readonly Brush _selectionAreaBrush = BrushHelpers.CreateFrozenBrush(Color.FromArgb(50, 20, 20, 190));
        private static readonly Pen _selectionAreaPen = CreateDashedPen(Color.FromArgb(200, 20, 20, 190), 1.5);
        private static readonly Pen _roomBorderPen = BrushHelpers.CreateFrozenPen(Colors.Black, 1.0);
        private static readonly Pen _roomPortalPen = CreateDottedPen(Color.FromArgb(220, 7, 70, 70), 1.0);
        private static readonly Pen _gridPenThin = BrushHelpers.CreateFrozenPen(Colors.LightGray, 1.0);
        private static readonly Pen _gridPenThick = BrushHelpers.CreateFrozenPen(Colors.LightGray, 3.0);
        private static readonly Brush _designPlaceholderBrush = BrushHelpers.CreateFrozenBrush(Color.FromRgb(50, 50, 50));
        private const double _probeRadius = 18;

        private System.Windows.Controls.ContextMenu _currentContextMenu;
        private bool _disposed;

        public Panel2DMap()
        {
            ClipToBounds = true;
            Focusable = true;
            FocusVisualStyle = null;
            SnapsToDevicePixels = true;
            AllowDrop = true;

            _movementTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _movementTimer.Tick += MoveTimerTick;

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            if (Editor.Instance is not null)
            {
                _editor = Editor.Instance;
                _editor.EditorEventRaised += EditorEventRaised;

                _depthBar = new DepthBar(_editor);
                _depthBar.InvalidateParent += InvalidateVisual;
                _depthBar.SelectedRoom += rooms => _editor.SelectRoomsAndResetCamera(BoolCombine(_editor.SelectedRooms, rooms));

                UpdateBrushes();
            }

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_viewInitialized && ActualWidth > 0 && ActualHeight > 0)
            {
                ResetView();
                _viewInitialized = true;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
            => _movementTimer.Stop();

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_editor is not null)
                _editor.EditorEventRaised -= EditorEventRaised;

            _movementTimer.Stop();
            _movementTimer.Tick -= MoveTimerTick;
            _insertionContourLineData = null;
            if (_currentContextMenu != null)
                _currentContextMenu.IsOpen = false;

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (_disposed)
                return;

            if (_selectionArea != null)
                _selectionArea._roomSelectionCache = null;

            if (obj is Editor.InitEvent ||
                obj is Editor.LevelChangedEvent ||
                obj is Editor.GameVersionChangedEvent)
            {
                var newSize = TrCatalog.GetLimit(_editor.Level.Settings.GameVersion, Limit.WorldDimensions);
                _mapSize = newSize > 0 ? newSize : 100;

                var floorHeight = TrCatalog.GetLimit(_editor.Level.Settings.GameVersion, Limit.FloorHeight);
                _depthBar.MinDepth = -floorHeight;
                _depthBar.MaxDepth = floorHeight;

                InvalidateVisual();
            }

            // Update drawing
            if (obj is Editor.SelectedRoomsChangedEvent ||
                obj is Editor.RoomGeometryChangedEvent ||
                obj is Editor.RoomPositionChangedEvent ||
                obj is Editor.RoomSectorPropertiesChangedEvent ||
                obj is Editor.RoomPropertiesChangedEvent ||
                obj is Editor.RoomListChangedEvent ||
                obj is Editor.ConfigurationChangedEvent)
            {
                UpdateBrushes();

                if (_editor.Mode == EditorMode.Map2D)
                    InvalidateVisual();
            }

            // Show message when displaying 2D map for the first time.
            if (obj is Editor.ModeChangedEvent)
                if (_editor.Mode == EditorMode.Map2D && (_editor.Configuration.Map2D_ShowTimes < 3)) // Show up to 3 times to increase likelyhood of the user noticing.
                {
                    _editor.SendMessage("Double click or Alt + click on the map to add or remove depth probe.\n" +
                        "Click and drag on the emptiness to start selection by area or use the middle mouse button.\n" +
                        "Selection can be changed using Ctrl or Shift. To copy rooms, press Ctrl while moving.", PopupType.Info);

                    _editor.Configuration.Map2D_ShowTimes++;
                    _editor.ConfigurationChange();
                }
        }

        private void UpdateBrushes()
        {
            _roomsNormalBrush = _editor.Configuration.UI_ColorScheme.ColorFloor.ToWPFBrush(0.7f);
            _roomsNormalBelowBrush = _editor.Configuration.UI_ColorScheme.Color2DRoomsBelow.ToWPFBrush(0.7f);
            _roomsNormalAboveBrush = _editor.Configuration.UI_ColorScheme.Color2DRoomsAbove.ToWPFBrush(0.47f);
            _roomsSelectedBrush = _editor.Configuration.UI_ColorScheme.ColorSelection.ToWPFBrush(0.7f);
            _roomsMovedBrush = _editor.Configuration.UI_ColorScheme.Color2DRoomsMoved.ToWPFBrush(0.28f);
            _roomsSelectedColor = ((SolidColorBrush)_roomsSelectedBrush).Color;

            _depthBar?.UpdateBrushes();
        }

        /// <summary>
        /// WPF replacement for modifier-key selection that reads the live keyboard modifier state.
        /// </summary>
        private IEnumerable<Room> BoolCombine(IEnumerable<Room> oldObjects, IEnumerable<Room> newObjects)
        {
            bool control = System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control);
            bool shift = System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift);

            if (control && shift)
                return oldObjects.Except(newObjects).ToList(); // Difference
            else if (control)
                return oldObjects.Union(newObjects).Except(oldObjects.Intersect(newObjects)).ToList(); // Either-or
            else if (shift)
                return oldObjects.Union(newObjects); // Union
            else
                return newObjects;
        }

        private static Pen CreateDottedPen(Color color, double thickness)
        {
            var pen = new Pen(BrushHelpers.CreateFrozenBrush(color), thickness) { DashStyle = DashStyles.Dot };
            pen.Freeze();
            return pen;
        }

        private static Pen CreateDashedPen(Color color, double thickness)
        {
            var pen = new Pen(BrushHelpers.CreateFrozenBrush(color), thickness)
            {
                DashStyle = new DashStyle(new double[] { 3.0, 3.0 }, 0.0)
            };
            pen.Freeze();
            return pen;
        }

        private static Brush CreateHatchBrush(Color color)
        {
            var geometry = new LineGeometry(new Point(0, 8), new Point(8, 0));
            var brush = new DrawingBrush
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 8, 8),
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, 8, 8),
                ViewboxUnits = BrushMappingMode.Absolute,
                Drawing = new GeometryDrawing(null, new Pen(BrushHelpers.CreateFrozenBrush(color), 1.0), geometry)
            };
            brush.Freeze();
            return brush;
        }
    }
}
