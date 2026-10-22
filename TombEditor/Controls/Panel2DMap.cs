using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombEditor.Controls.ContextMenus;
using TombLib;
using TombLib.Controls;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.Utils;
using TombLib.Wad.Catalog;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;
using RectangleF = System.Drawing.RectangleF;

namespace TombEditor.Controls
{
    public class Panel2DMap : Panel
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector2 ViewPosition { get; set; } = new Vector2(60.0f, 60.0f);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        private readonly MovementTimer _movementTimer;
        private IReadOnlyList<RoomClipboardData.ContourLine> _insertionContourLineData;
        private Vector2 _insertionDropPosition;
        private VectorInt2 _insertionCurrentOffset;
        private Point _startMousePosition;
        private VectorInt3 _overallDelta;

        private int _mapSize = 100;

        private class SelectionArea
        {
            public Rectangle2 _area;
            public HashSet<Room> _roomSelectionCache;
            public HashSet<Room> GetRoomSelection(Panel2DMap parent)
            {
                if (_roomSelectionCache == null && _area.Size.Length() > 0.5f)
                    _roomSelectionCache = new HashSet<Room>(
                        WinFormsUtils.BoolCombine(parent._editor.SelectedRooms,
                        parent._editor.Level.ExistingRooms
                        .Where(room => parent._depthBar.CheckRoom(room))
                        .Where(room =>
                            room.Position.X + room.NumXSectors > Math.Min(_area.Start.X, _area.End.X) &&
                            room.Position.Z + room.NumZSectors > Math.Min(_area.Start.Y, _area.End.Y) &&
                            room.Position.X < Math.Max(_area.Start.X, _area.End.X) &&
                            room.Position.Z < Math.Max(_area.Start.Y, _area.End.Y)), ModifierKeys));
                return _roomSelectionCache;
            }
        }
        private SelectionArea _selectionArea;

        private SolidBrush _roomsNormalBrush;
        private SolidBrush _roomsNormalAboveBrush;
        private SolidBrush _roomsNormalBelowBrush;
        private SolidBrush _roomsSelectedBrush;
        private SolidBrush _roomsMovedBrush;

        private static readonly Brush _roomsLockedBrush = new HatchBrush(HatchStyle.WideUpwardDiagonal, Color.Transparent, Color.FromArgb(50, 20, 20, 20));
        private static readonly Brush _selectionAreaBrush = new HatchBrush(HatchStyle.SmallConfetti, Color.FromArgb(90, 20, 20, 190), Color.FromArgb(50, 20, 20, 190));
        private static readonly Pen _selectionAreaPen = new Pen(Color.FromArgb(200, 20, 20, 190), 1.5f) { DashPattern = new[] { 3.0f, 3.0f } };
        private static readonly Pen _roomBorderPen = new Pen(Color.Black, 1);
        private static readonly Pen _roomPortalPen = new Pen(Color.FromArgb(220, 7, 70, 70), 1) { DashStyle = DashStyle.Dot };
        private static readonly Pen _gridPenThin = new Pen(Color.LightGray, 1);
        private static readonly Pen _gridPenThick = new Pen(Color.LightGray, 3);
        private const float _probeRadius = 18;

        private BaseContextMenu _currentContextMenu;

        public Panel2DMap()
        {
            DoubleBuffered = true;
            AllowDrop = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
            UpdateStyles();

            if (Editor.Instance is not null)
            {
                _editor = Editor.Instance;
                _editor.EditorEventRaised += EditorEventRaised;

                _depthBar = new DepthBar(_editor);
                _depthBar.InvalidateParent += Invalidate;
                _depthBar.GetParent += () => this;
                _depthBar.SelectedRoom += rooms => _editor.SelectRoomsAndResetCamera(WinFormsUtils.BoolCombine(_editor.SelectedRooms, rooms, ModifierKeys));

                _movementTimer = new MovementTimer(MoveTimerTick);

                UpdateBrushes();
                ResetView();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _editor is not null)
                _editor.EditorEventRaised -= EditorEventRaised;

            _movementTimer?.Dispose();
            _insertionContourLineData = null;
            _currentContextMenu?.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
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
                _depthBar.MaxDepth =  floorHeight;

                Invalidate();
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
                    Invalidate();
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
            if (_roomsNormalBrush != null) _roomsNormalBrush.Dispose();
            if (_roomsNormalAboveBrush != null) _roomsNormalAboveBrush.Dispose();
            if (_roomsNormalBelowBrush != null) _roomsNormalBelowBrush.Dispose();
            if (_roomsSelectedBrush != null) _roomsSelectedBrush.Dispose();
            if (_roomsMovedBrush != null) _roomsMovedBrush.Dispose();

            _roomsNormalBrush = new SolidBrush(_editor.Configuration.UI_ColorScheme.ColorFloor.ToWinFormsColor(0.7f));
            _roomsNormalBelowBrush = new SolidBrush(_editor.Configuration.UI_ColorScheme.Color2DRoomsBelow.ToWinFormsColor(0.7f));
            _roomsNormalAboveBrush = new SolidBrush(_editor.Configuration.UI_ColorScheme.Color2DRoomsAbove.ToWinFormsColor(0.47f));
            _roomsSelectedBrush = new SolidBrush(_editor.Configuration.UI_ColorScheme.ColorSelection.ToWinFormsColor(0.7f));
            _roomsMovedBrush = new SolidBrush(_editor.Configuration.UI_ColorScheme.Color2DRoomsMoved.ToWinFormsColor(0.28f));

            _depthBar.UpdateBrushes();
        }

        public void ResetView()
        {
            ViewPosition = (new Vector2(Width, Height) * 0.5f - new Vector2(16.0f)) / _viewScale;
        }

        public Vector2 FromVisualCoord(PointF pos)
        {
            return new Vector2((pos.X - Width * 0.5f) / _viewScale + ViewPosition.X, (Height * 0.5f - pos.Y) / _viewScale + ViewPosition.Y);
        }

        public Rectangle2 FromVisualCoord(RectangleF area)
        {
            Vector2 visibleAreaStart = FromVisualCoord(new PointF(area.Left, area.Top));
            Vector2 visibleAreaEnd = FromVisualCoord(new PointF(area.Right, area.Bottom));
            return new Rectangle2(Vector2.Min(visibleAreaStart, visibleAreaEnd), Vector2.Max(visibleAreaStart, visibleAreaEnd));
        }

        public PointF ToVisualCoord(Vector2 pos)
        {
            return new PointF((pos.X - ViewPosition.X) * _viewScale + Width * 0.5f, Height * 0.5f - (pos.Y - ViewPosition.Y) * _viewScale);
        }

        public RectangleF ToVisualCoord(Rectangle2 area)
        {
            PointF start = ToVisualCoord(area.Start);
            PointF end = ToVisualCoord(area.End);
            return RectangleF.FromLTRB(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Max(start.X, end.X), Math.Max(start.Y, end.Y));
        }

        private void MoveToFixedPoint(PointF visualPoint, Vector2 worldPoint, bool limitPosition = false)
        {
            // Adjust ViewPosition in such a way, that the FixedPoint does not move visually
            ViewPosition = -worldPoint;
            ViewPosition = -FromVisualCoord(visualPoint);
            if (limitPosition)
                LimitPosition();
            Invalidate();
        }

        private void LimitPosition()
        {
            ViewPosition = Vector2.Clamp(ViewPosition, new Vector2(), new Vector2(_mapSize));
        }

        private int? FindClosestProbe(Vector2 clickPos)
        {
            if (_depthBar.DepthProbes.Count == 0)
                return null;

            int currentProbeIndex = 0;

            for (int i = 0; i < _depthBar.DepthProbes.Count; ++i)
                if ((clickPos - _depthBar.DepthProbes[i].Position).Length() < (clickPos - _depthBar.DepthProbes[currentProbeIndex].Position).Length())
                    currentProbeIndex = i;

            if ((clickPos - _depthBar.DepthProbes[currentProbeIndex].Position).Length() < _probeRadius / 2 / ViewScale)
                return currentProbeIndex;
            else
                return null;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            var clickPos = FromVisualCoord(e.Location);
            if (!_depthBar.MouseDown(e, Size, clickPos))
                return;

            _lastMousePosition = e.Location;

            //https://stackoverflow.com/questions/14191219/receive-mouse-move-even-cursor-is-outside-control
            Capture = true; // Capture mouse for zoom and panning

            switch (e.Button)
            {
                case MouseButtons.Left:
                    // Check for depth probe
                    int? currentProbeIndex = FindClosestProbe(clickPos);

                    if (ModifierKeys.HasFlag(Keys.Alt))
                    {
                        if (currentProbeIndex.HasValue)
                        {
                            // Remove depth probe closest to mouse pointer
                            _depthBar.DepthProbes.RemoveAt(currentProbeIndex.Value);
                        }
                        else
                        {
                            // Add depth probe under mouse pointer
                            _currentlyEditedDepthProbeIndex = _depthBar.DepthProbes.Count;
                            _depthBar.DepthProbes.Add(new DepthBar.DepthProbe(_depthBar) { Position = clickPos });
                        }
                        Invalidate();
                    }
                    else if (currentProbeIndex.HasValue)
                    {
                        // Depth probe normally selected
                        _currentlyEditedDepthProbeIndex = FindClosestProbe(clickPos);
                    }
                    else
                    {
                        _overallDelta = VectorInt3.Zero;
                        _roomMouseClicked = DoPicking(clickPos);
                        if (_roomMouseClicked == null)
                        {
                            _editor.SelectedRooms = new[] { _editor.SelectedRoom };
                            _selectionArea = new SelectionArea { _area = new Rectangle2(clickPos, clickPos) };
                        }
                        else
                        {
                            if (ModifierKeys != Keys.None || !_editor.SelectedRooms.Contains(_roomMouseClicked))
                            {
                                _editor.SelectRoomsAndResetCamera(WinFormsUtils.BoolCombine(_editor.SelectedRooms,
                                    new[] { _roomMouseClicked }, ModifierKeys));
                            }
                            _roomMouseOffset = clickPos - _roomMouseClicked.SectorPos;
                        }
                    }
                    break;

                case MouseButtons.Right:
                    _startMousePosition = e.Location;

                    // Move view with mouse cursor
                    // Mouse cursor is a fixed point
                    _viewMoveMouseWorldCoord = clickPos;
                    break;

                case MouseButtons.Middle:
                    _selectionArea = new SelectionArea { _area = new Rectangle2(clickPos, clickPos) };
                    break;

                case MouseButtons.XButton1:
                    // Remove depth probe closest to mouse pointer
                    currentProbeIndex = FindClosestProbe(clickPos);
                    if (currentProbeIndex.HasValue)
                    {
                        _depthBar.DepthProbes.RemoveAt(currentProbeIndex.Value);
                        Invalidate();
                    }
                    break;

                case MouseButtons.XButton2:
                    // Add depth probe under mouse pointer
                    _currentlyEditedDepthProbeIndex = _depthBar.DepthProbes.Count;
                    _depthBar.DepthProbes.Add(new DepthBar.DepthProbe(_depthBar) { Position = clickPos });
                    Invalidate();
                    break;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Vector2 clickPos = FromVisualCoord(e.Location);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (!ModifierKeys.HasFlag(Keys.Shift))
                    {
                        int? currentProbeIndex = FindClosestProbe(clickPos);
                        if (currentProbeIndex.HasValue)
                            _depthBar.DepthProbes.RemoveAt(currentProbeIndex.Value);
                        else
                            _depthBar.DepthProbes.Add(new DepthBar.DepthProbe(_depthBar) { Position = clickPos });
                        Invalidate();
                    }
                    break;

                case MouseButtons.Middle:
                    Room clickedRoom = DoPicking(clickPos);
                    Keys modifierKeys = ModifierKeys;
                    if (clickedRoom != null)
                    {
                        IEnumerable<Room> connectedRooms = _editor.Level.GetConnectedRooms(clickedRoom);
                        connectedRooms = WinFormsUtils.BoolCombine(_editor.SelectedRooms, _editor.Level.GetConnectedRooms(clickedRoom), ModifierKeys);
                        connectedRooms = // Don't use the currently clicked room because it was already processed with the previous single click.
                            _editor.SelectedRooms.Where(room => room == clickedRoom || room == clickedRoom.AlternateOpposite)
                            .Concat(
                                connectedRooms.Where(room => room != clickedRoom && room != clickedRoom.AlternateOpposite));
                        _editor.SelectRoomsAndResetCamera(connectedRooms);
                    }
                    _selectionArea = null;
                    break;
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!Focused)
                Focus(); // Enable keyboard interaction
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Update depth bar...
            _depthBar.MouseMove(e, Size);
            RectangleF area = _depthBar.groupGetArea(_depthBar.GetBarArea(Size), _depthBar.DepthProbes.Count); // Only redraw the depth bar group for the cursor.
            Invalidate(Rectangle.FromLTRB((int)Math.Floor(area.X), (int)Math.Floor(area.Y), (int)Math.Ceiling(area.Right), (int)Math.Ceiling(area.Bottom)));

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (_currentlyEditedDepthProbeIndex.HasValue)
                    {
                        // Move depth probe around
                        _depthBar.DepthProbes[_currentlyEditedDepthProbeIndex.Value].Position = FromVisualCoord(e.Location);
                        Invalidate();
                    }
                    else if (_roomMouseClicked != null)
                    {
                        bool copyMode = ModifierKeys.HasFlag(Keys.Control);

                        if (copyMode &&((Vector3)_overallDelta).Length() >= 1 * ViewScale)
                        {
                            _roomsToMove = null;
                            DoDragDrop(new RoomClipboardData(_editor, FromVisualCoord(e.Location)), DragDropEffects.Copy);
                            break;
                        }

                        if (_roomsToMove == null)
                            _roomsToMove = _editor.Level.GetConnectedRooms(_editor.SelectedRooms.Concat(new[] { _roomMouseClicked }));

                        if (_roomsToMove != null && UpdateRoomPosition(FromVisualCoord(e.Location) - _roomMouseOffset, _roomMouseClicked, !copyMode))
                        {
                            // Move rooms around
                            foreach (Room room in _roomsToMove)
                                _editor.RoomPositionChange(room);
                            _editor.ResetCamera();
                            Invalidate();
                        }
                    }
                    else if (_selectionArea != null)
                        goto case MouseButtons.Middle;
                    break;

                case MouseButtons.Middle:
                    _selectionArea._area.End = FromVisualCoord(e.Location);
                    _selectionArea._roomSelectionCache = null;
                    Invalidate();
                    break;

                case MouseButtons.Right:
                    if (_viewMoveMouseWorldCoord != null)
                        if (ModifierKeys.HasFlag(Keys.Control))
                        { // Zoom
                            float relativeDeltaY = (e.Location.Y - _lastMousePosition.Y) / (float)Height;
                            ViewScale *= (float)Math.Exp(_editor.Configuration.Map2D_NavigationSpeedMouseZoom * relativeDeltaY);
                            Invalidate();
                        }
                        else
                        { // Panning
                            MoveToFixedPoint(e.Location, _viewMoveMouseWorldCoord.Value, true);
                        }
                    break;
            }

            _lastMousePosition = e.Location;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Capture = false;
            _depthBar.MouseUp(e, Size);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (_roomMouseClicked != null)
                    {
                        if(_roomsToMove != null)
                        {
                            _editor.UndoManager.PushRoomsMoved(_roomsToMove.ToList(), _overallDelta);
                            _roomsToMove = null;
                        }

                        _roomMouseClicked = null;
                        Invalidate();
                    }
                    break;
                case MouseButtons.Right:
                    var distance = new Vector2(_startMousePosition.X, _startMousePosition.Y) - new Vector2(e.Location.X, e.Location.Y);
                    if (distance.Length() < 4.0f)
                    {
                        _currentContextMenu?.Dispose();
                        Vector2 clickPos = FromVisualCoord(e.Location);
                        if (_editor.SelectedRooms.Contains(DoPicking(clickPos)))
                            _currentContextMenu = new SelectedRoomContextMenu(_editor, this, clickPos);
                        else
                            _currentContextMenu = new Space2DMapContextMenu(_editor, this, clickPos);
                        _currentContextMenu.Show(PointToScreen(e.Location));
                    }

                    _viewMoveMouseWorldCoord = null;
                    break;
            }
            _currentlyEditedDepthProbeIndex = null;
            _roomMouseClicked = null;
            _roomsToMove = null;

            if (_selectionArea != null)
            { // Change room selection
                _editor.SelectRoomsAndResetCamera(_selectionArea.GetRoomSelection(this));
                _selectionArea = null;
                Invalidate();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Vector2 FixedPointInWorld = FromVisualCoord(e.Location);
            ViewScale *= (float)Math.Exp(e.Delta * _editor.Configuration.Map2D_NavigationSpeedMouseWheelZoom);
            MoveToFixedPoint(e.Location, FixedPointInWorld);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            // Make control receive key events as suggested here...
            // https://stackoverflow.com/questions/20079373/trouble-creating-keydown-event-in-panel
            if ((ModifierKeys & (Keys.Control | Keys.Alt | Keys.Shift)) == Keys.None)
                _movementTimer.Engage(e.KeyCode);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            KeyUpdateSelectionAreaPreview(e);
            if (e.KeyCode == Keys.Escape)
            {
                _selectionArea = null;
                Invalidate();
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _movementTimer.Stop();
            KeyUpdateSelectionAreaPreview(e);
        }

        private void KeyUpdateSelectionAreaPreview(KeyEventArgs e)
        {
            if ((e.KeyCode & Keys.ShiftKey) != 0 || (e.KeyCode & Keys.ControlKey) != 0)
            {
                if (_selectionArea != null)
                {
                    _selectionArea._roomSelectionCache = null;
                    Invalidate();
                }
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _movementTimer.Stop();
            _insertionContourLineData = null;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Don't paint the background
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                if (LicenseManager.UsageMode != LicenseUsageMode.Runtime || _editor?.Level == null)
                {
                    e.Graphics.Clear(Parent.BackColor);
                    e.Graphics.DrawString("2D Room Rendering: Not Available!", Font, Brushes.DarkGray, ClientRectangle,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    return;
                }

                RectangleF barArea = _depthBar.GetBarArea(Size);
                RectangleF barAreaWithSpace = new RectangleF(barArea.X - 1, barArea.Y, barArea.Width, barArea.Height);

                // Draw 2d map if necessary and not occluded by 2d bar
                if (!barAreaWithSpace.Contains(e.ClipRectangle))
                {
                    Rectangle2 visibleArea = FromVisualCoord(e.ClipRectangle);
                    e.Graphics.Clear(_editor.Configuration.UI_ColorScheme.Color2DBackground.ToWinFormsColor());

                    // Draw hidden rooms
                    float currentRangeMin = Clicks.FromWorld(_editor.SelectedRoom.Position.Y) + Clicks.FromWorld(_editor.SelectedRoom.GetLowestCorner());
                    float currentRangeMax = Clicks.FromWorld(_editor.SelectedRoom.Position.Y) + Clicks.FromWorld(_editor.SelectedRoom.GetHighestCorner());
                    List<Room> sortedRoomList = _editor.Level.GetVerticallyAscendingRoomList(room =>
                        room.Position.X + room.NumXSectors >= visibleArea.Start.X && room.Position.X <= visibleArea.End.X &&
                        room.Position.Z + room.NumZSectors >= visibleArea.Start.Y && room.Position.Z <= visibleArea.End.Y).ToList();

                    bool drewAny = false;
                    foreach (Room room in sortedRoomList)
                        if (!_depthBar.CheckRoom(room)) // Check if the room fits the depth bar criterion
                        {
                            drewAny = true;
                            DrawRoom(e, room, currentRangeMin, currentRangeMax, true, false);
                        }
                    if (drewAny)
                        using (var b = new SolidBrush(_editor.Configuration.UI_ColorScheme.Color2DBackground.ToWinFormsColor(0.7f)))
                            e.Graphics.FillRectangle(b, e.ClipRectangle); // Make the rooms in the background appear faded
                        

                    // Draw grid lines
                    Vector2 GridLines0 = FromVisualCoord(new PointF());
                    Vector2 GridLines1 = FromVisualCoord(new PointF() + Size);
                    Vector2 GridLinesStart = Vector2.Min(GridLines0, GridLines1);
                    Vector2 GridLinesEnd = Vector2.Max(GridLines0, GridLines1);
                    GridLinesStart = Vector2.Clamp(GridLinesStart, new Vector2(0.0f), new Vector2(_mapSize));
                    GridLinesEnd = Vector2.Clamp(GridLinesEnd, new Vector2(0.0f), new Vector2(_mapSize));
                    Point GridLinesStartInt = new Point((int)Math.Floor(GridLinesStart.X), (int)Math.Floor(GridLinesStart.Y));
                    Point GridLinesEndInt = new Point((int)Math.Ceiling(GridLinesEnd.X), (int)Math.Ceiling(GridLinesEnd.Y));

                    for (int x = GridLinesStartInt.X; x <= GridLinesEndInt.X; ++x)
                        e.Graphics.DrawLine(x % 10 == 0 ? _gridPenThick : _gridPenThin,
                            ToVisualCoord(new Vector2(x, 0)), ToVisualCoord(new Vector2(x, _mapSize)));

                    for (int y = GridLinesStartInt.Y; y <= GridLinesEndInt.Y; ++y)
                        e.Graphics.DrawLine(y % 10 == 0 ? _gridPenThick : _gridPenThin,
                            ToVisualCoord(new Vector2(0, y)), ToVisualCoord(new Vector2(_mapSize, y)));

                    // Draw visible rooms
                    foreach (Room room in sortedRoomList)
                        if (_depthBar.CheckRoom(room)) // Check if the room fits the depth bar criterion
                            DrawRoom(e, room, currentRangeMin, currentRangeMax, true, true);

                    // Draw probe positions with digits
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    for (int i = 0; i < _depthBar.DepthProbes.Count; ++i)
                    {
                        PointF depthProbeVisualPos = ToVisualCoord(_depthBar.DepthProbes[i].Position);
                        RectangleF depthProbeRect = new RectangleF(depthProbeVisualPos.X - _probeRadius / 2, depthProbeVisualPos.Y - _probeRadius / 2, _probeRadius, _probeRadius);

                        Color probeColor = _depthBar.DepthProbes[i].Color;
                        using (var probeBrush = new SolidBrush(probeColor.MixWith(Color.White, 0.765)))
                            e.Graphics.FillEllipse(probeBrush, depthProbeRect);
                        using (var probePen = new Pen(probeColor.MixWith(Color.White, 0.05), 2))
                            e.Graphics.DrawEllipse(probePen, depthProbeRect);

                        SizeF textRectSize = e.Graphics.MeasureString(i.ToString(), DepthBar.ProbeFont);
                        PointF textRextPos = new PointF(depthProbeVisualPos.X - textRectSize.Width / 2, depthProbeVisualPos.Y - textRectSize.Height / 2);
                        using (var probeTextBrush = new SolidBrush(probeColor.MixWith(Color.White, 0.05)))
                        {
                            e.Graphics.DrawString(i.ToString(), DepthBar.ProbeFont, probeTextBrush, new RectangleF(textRextPos, textRectSize), DepthBar.ProbeStringLayout);

                            // Draw depth bar numbers
                            if (!barAreaWithSpace.Contains(e.ClipRectangle))
                            {
                                RectangleF groupArea = _depthBar.groupGetArea(barArea, i);
                                e.Graphics.DrawString(i.ToString(), DepthBar.ProbeFont, probeTextBrush, new RectangleF(groupArea.X, 0, groupArea.Width, groupArea.Y), DepthBar.ProbeStringLayout);
                            }
                        }
                    }
                    e.Graphics.SmoothingMode = SmoothingMode.Default;

                    // Draw selection area
                    if (_selectionArea != null)
                    {
                        e.Graphics.FillRectangle(_selectionAreaBrush, ToVisualCoord(_selectionArea._area));
                        e.Graphics.DrawRectangle(_selectionAreaPen, ToVisualCoord(_selectionArea._area));
                    }
                }

                // Draw insertion contour data
                if (_insertionContourLineData != null)
                    foreach (var contourLineSegment in _insertionContourLineData)
                    {
                        e.Graphics.DrawLine(_roomBorderPen,
                            ToVisualCoord(contourLineSegment.Start + _insertionCurrentOffset),
                            ToVisualCoord(contourLineSegment.End + _insertionCurrentOffset));
                    }

                // Draw depth bar
                Vector2 cursorPos = FromVisualCoord(PointToClient(MousePosition));
                _depthBar.Draw(e, ClientSize, cursorPos, GetRoomBrush);

                // Invalidation debugger
                //Random r = new Random();
                //byte[] color = new byte[3];
                //r.NextBytes(color);
                //e.Graphics.Clear(Color.FromArgb(color[0], color[1], color[1]));
            }
            catch (Exception exc)
            {
                logger.Error(exc, "An exception occured while drawing the 2D map.");
            }
        }

        private void MoveTimerTick(object sender, EventArgs e)
        {
            switch (_movementTimer.MoveKey)
            {
                case Keys.Down:
                    ViewPosition += new Vector2(0.0f, -_editor.Configuration.Map2D_NavigationSpeedKeyMove / ViewScale * _movementTimer.MoveMultiplier);
                    LimitPosition();
                    Invalidate();
                    break;
                case Keys.Up:
                    ViewPosition += new Vector2(0.0f, _editor.Configuration.Map2D_NavigationSpeedKeyMove / ViewScale * _movementTimer.MoveMultiplier);
                    LimitPosition();
                    Invalidate();
                    break;
                case Keys.Left:
                    ViewPosition += new Vector2(-_editor.Configuration.Map2D_NavigationSpeedKeyMove / ViewScale * _movementTimer.MoveMultiplier, 0.0f);
                    LimitPosition();
                    Invalidate();
                    break;
                case Keys.Right:
                    ViewPosition += new Vector2(_editor.Configuration.Map2D_NavigationSpeedKeyMove / ViewScale * _movementTimer.MoveMultiplier, 0.0f);
                    LimitPosition();
                    Invalidate();
                    break;
                case Keys.PageDown:
                    ViewScale *= (float)Math.Exp(-_editor.Configuration.Map2D_NavigationSpeedKeyZoom * _movementTimer.MoveMultiplier);
                    Invalidate();
                    break;
                case Keys.PageUp:
                    ViewScale *= (float)Math.Exp(_editor.Configuration.Map2D_NavigationSpeedKeyZoom * _movementTimer.MoveMultiplier);
                    Invalidate();
                    break;
            }
        }

        private void DrawRoom(PaintEventArgs e, Room room, float currentRangeMin, float currentRangeMax, bool drawFilled, bool drawOutline)
        {
            int width = room.NumXSectors;
            int height = room.NumZSectors;
            if (drawFilled)
            {
                // Fill area of room with rectangular stripes
                List<RectangleF> rectangles = new List<RectangleF>();
                for (int z = 1; z < height - 1; ++z)
                {
                    int previousRectangleCount = rectangles.Count;
                    for (int x = 1; x < width - 1; ++x)
                        if (room.Sectors[x, z].Type == SectorType.Floor)
                        {
                            int xBegin = x;
                            // Search for the next sector without a wall
                            for (; x < width - 1; ++x)
                                if (room.Sectors[x, z].Type != SectorType.Floor)
                                    break;
                            rectangles.Add(new RectangleF(xBegin, z, x - xBegin, 1));
                        }

                    // Try to combine rectangle with the rectangle of the previous row
                    if (rectangles.Count >= 2 && previousRectangleCount + 1 == rectangles.Count)
                    {
                        RectangleF previousRectangle = rectangles[rectangles.Count - 2];
                        RectangleF thisRectangle = rectangles[rectangles.Count - 1];
                        if (thisRectangle.X == previousRectangle.X &&
                            thisRectangle.Width == previousRectangle.Width &&
                            thisRectangle.Top == previousRectangle.Bottom)
                        {
                            rectangles.RemoveAt(rectangles.Count - 1);
                            previousRectangle.Height += 1;
                            rectangles[rectangles.Count - 1] = previousRectangle;
                        }
                    }
                }

                // Transform coordinates
                for (int j = 0; j < rectangles.Count; ++j)
                    rectangles[j] = new RectangleF(
                        ToVisualCoord(new Vector2(rectangles[j].X + room.SectorPos.X, rectangles[j].Bottom + room.SectorPos.Y)),
                        new SizeF(rectangles[j].Width * _viewScale, rectangles[j].Height * _viewScale));

                // Draw the rectangular stripes
                if (rectangles.Count > 0)
                {
                    Brush brush = _roomsNormalBrush;
                    if (Clicks.FromWorld(room.Position.Y) + Clicks.FromWorld(room.GetHighestCorner()) <= currentRangeMin)
                        brush = _roomsNormalBelowBrush;
                    if (Clicks.FromWorld(room.Position.Y) + Clicks.FromWorld(room.GetLowestCorner()) >= currentRangeMax)
                        brush = _roomsNormalAboveBrush;
                    using (var brush2 = GetRoomBrush(room, brush))
                        e.Graphics.FillRectangles(brush2, rectangles.ToArray());
                    if(room.Properties.Locked)
                        e.Graphics.FillRectangles(_roomsLockedBrush, rectangles.ToArray());

                }
            }

            if (drawOutline)
            {
                // Determine outlines of room
                for (int z = 1; z < height; ++z)
                    for (int x = 1; x < width; ++x)
                    {
                        Sector thisSector = room.Sectors[x, z];
                        Sector aboveSector = room.Sectors[x, z - 1];
                        Sector leftSector = room.Sectors[x - 1, z];
                        if (aboveSector.IsAnyWall != thisSector.IsAnyWall)
                            e.Graphics.DrawLine(aboveSector.WallPortal != null || thisSector.WallPortal != null ? _roomPortalPen : _roomBorderPen,
                                ToVisualCoord(new Vector2(x + room.SectorPos.X, z + room.SectorPos.Y)),
                                ToVisualCoord(new Vector2(x + 1 + room.SectorPos.X, z + room.SectorPos.Y)));
                        if (leftSector.IsAnyWall != thisSector.IsAnyWall)
                            e.Graphics.DrawLine(leftSector.WallPortal != null || thisSector.WallPortal != null ? _roomPortalPen : _roomBorderPen,
                                ToVisualCoord(new Vector2(x + room.SectorPos.X, z + room.SectorPos.Y)),
                                ToVisualCoord(new Vector2(x + room.SectorPos.X, z + 1 + room.SectorPos.Y)));
                    }
            }
        }

        private ConditionallyOwned<Brush> GetRoomBrush(Room room, Brush baseBrush)
        {
            // Handle room movement
            bool isBeingMoved =
                _roomsToMove != null && _roomsToMove.Contains(room) ||
                _depthBar.RoomsToMove != null && _depthBar.RoomsToMove.Contains(room);
            if (isBeingMoved)
                baseBrush = _roomsMovedBrush;

            // Handle room selection
            HashSet<Room> currentlySelectedRooms = _selectionArea?.GetRoomSelection(this);
            bool isSelected = _editor.SelectedRoomsContains(room);
            bool willBeSelected = currentlySelectedRooms?.Contains(room) ?? isSelected;
            if (isSelected != willBeSelected)
                if (baseBrush is SolidBrush)
                    return new ConditionallyOwned<Brush>( // HatchStyle.ZigZag
                        new HatchBrush(isSelected ? HatchStyle.LightUpwardDiagonal : HatchStyle.DarkDownwardDiagonal,
                        _roomsSelectedBrush.Color, ((SolidBrush)baseBrush).Color), true);
                else
                    return baseBrush;
            else if (isSelected)
                return _roomsSelectedBrush;
            else
                return baseBrush;
        }

        private bool UpdateRoomPosition(Vector2 newRoomPos, Room roomReference, bool moveRooms)
        {
            VectorInt2 newRoomPosInt = VectorInt2.FromRounded(newRoomPos);
            VectorInt2 roomMovement = newRoomPosInt - roomReference.SectorPos;

            if (roomMovement != new VectorInt2())
            {
                if (EditorActions.CheckForLockedRooms(this, _roomsToMove))
                    _roomMouseClicked = null;
                else
                {
                    var delta = new VectorInt3(roomMovement.X, 0, roomMovement.Y);

                    if (roomMovement.X != 0 || roomMovement.Y != 0)
                    {
                        _overallDelta += delta;
                        if (moveRooms)
                        {
                            EditorActions.MoveRooms(delta, _roomsToMove, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private Room DoPicking(Vector2 pos)
        {
            IEnumerable<Room> roomList = _editor.Level.GetVerticallyAscendingRoomList(room =>
            {
                int roomLocalX = (int)pos.X - room.SectorPos.X;
                int roomLocalZ = (int)pos.Y - room.SectorPos.Y;
                if (roomLocalX < 1 || roomLocalZ < 1 || roomLocalX >= room.NumXSectors - 1 || roomLocalZ >= room.NumZSectors - 1)
                    return false;

                if (room.Sectors[roomLocalX, roomLocalZ].IsAnyWall)
                    return false;

                // Check if the room fits the depth bar criterion
                return _depthBar.CheckRoom(room);
            });

            return roomList.LastOrDefault();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Invalidate();
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);

            RoomClipboardData clipboardData = drgevent.Data.GetData(typeof(RoomClipboardData)) as RoomClipboardData;
            if (clipboardData != null)
            {
                drgevent.Effect = DragDropEffects.Copy;
                _insertionContourLineData = clipboardData.ContourLines;
                _insertionDropPosition = clipboardData.DropPosition;
                _insertionCurrentOffset = GetDragDropOffset(drgevent);
                Invalidate();
            }
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);

            if (_insertionContourLineData != null)
            {
                VectorInt2 newCurrentOffset = GetDragDropOffset(drgevent);
                if (newCurrentOffset != _insertionCurrentOffset)
                {
                    _insertionCurrentOffset = newCurrentOffset;
                    Invalidate();
                }
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);

            if (_insertionContourLineData != null)
            {
                _insertionContourLineData = null;
                Invalidate();
            }
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);

            _insertionContourLineData = null;
            RoomClipboardData clipboardData = drgevent.Data.GetData(typeof(RoomClipboardData)) as RoomClipboardData;
            if (clipboardData != null)
                clipboardData.MergeInto(_editor, GetDragDropOffset(drgevent));
        }

        private VectorInt2 GetDragDropOffset(DragEventArgs drgevent)
        {
            var newPos = FromVisualCoord(PointToClient(new Point(drgevent.X, drgevent.Y)));
            var result = newPos - _insertionDropPosition;
            return VectorInt2.FromRounded(result);
        }

        [DefaultValue(true)]
        public override bool AllowDrop
        {
            get { return base.AllowDrop; }
            set { base.AllowDrop = value; }
        }
    }
}
