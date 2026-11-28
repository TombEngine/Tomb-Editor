using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TombLib;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.WPF;

#nullable disable


namespace TombEditor.Features.Map2D
{
    /// <summary>
    /// WPF port of the 2D map depth bar. Renders the vertical depth scale on the right edge of
    /// <see cref="Panel2DMap"/> and handles the slider/room-drag interaction on top of it.
    /// Drawing happens through a <see cref="DrawingContext"/> and input is fed from the hosting
    /// element's mouse handlers.
    /// </summary>
    public class DepthBar
    {
        public float MinDepth
        {
            get { return _minDepth; }
            set { _minDepth = _selectedLimit0 = value; InvalidateParent?.Invoke(); }
        }
        private float _minDepth;

        public float MaxDepth
        {
            get { return _maxDepth; }
            set { _maxDepth = _selectedLimit1 = value; InvalidateParent?.Invoke(); }
        }
        private float _maxDepth;

        public readonly List<DepthProbe> DepthProbes = new();
        public event Action InvalidateParent;
        public event Action<IEnumerable<Room>> SelectedRoom;

        private const int _snappingMargin = 4;
        private const float _marginX = 10.0f;
        private const float _marginYUp = 32.0f;
        private const float _marginYDown = 32.0f;
        private const float _barWidth = 36.0f;
        private const int _heightStringCount = 16;
        private const float _heightStringLineLength = 4.0f;
        private const float _heightStringLineDistance = 6.0f;
        private const float _heightStringFadeDistance = 12.0f;
        private const float _heightStringArrowSize = 3.0f;
        private const float _selectionMaxPixelDistanceForMove = 8.0f;
        private const float _minDepthDifferenceBetweenIndependentlyMergedSequences = 12.0f;

        private readonly Editor _editor;

        private float _selectedLimit0 { get; set; }
        private float _selectedLimit1 { get; set; }
        private Room _roomMouseClicked;
        private int _groupMouseClicked;
        private HashSet<Room> _roomsToMove; // Set to a valid list only if room dragging is active
        private float _roomMouseOffset; // Relative depth difference to where it was clicked.
        private float _barMouseOffset;
        private int _overallDelta;

        public static readonly Color[] ProbeColors =
        {
            Colors.Crimson,
            Colors.Purple,
            Colors.DarkGreen,
            Colors.Maroon,
            Colors.DarkOrange,
            Colors.MidnightBlue
        };

        public static readonly Typeface ProbeTypeface = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        public const double ProbeFontSize = 14.0;

        private static readonly Typeface _heightStringTypeface = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        private const double _heightStringFontSize = 11.0;
        private static readonly double _heightStringFontHeight = _heightStringTypeface.FontFamily.LineSpacing * _heightStringFontSize;

        private static readonly Brush _backgroundBrush = BrushHelpers.CreateFrozenBrush(Color.FromRgb(245, 245, 245));
        private static readonly Pen _outlinePen = BrushHelpers.CreateFrozenPen(Color.FromArgb(245, 80, 80, 80), 1.0);
        private static readonly Brush _outlineBrush = BrushHelpers.CreateFrozenBrush(Color.FromArgb(245, 80, 80, 80));
        private static readonly Pen _heightLinesPen = BrushHelpers.CreateFrozenPen(Color.FromRgb(220, 220, 220), 1.0);
        private static readonly Pen _heightLinesBigPen = BrushHelpers.CreateFrozenPen(Color.FromRgb(200, 200, 200), 3.0);
        private static readonly Pen _sequenceSeperatorPen = _outlinePen;
        private static readonly Pen _portalPen = CreateDottedPen(Colors.Black, 1.0);
        private static readonly Pen _roomBoundsPen = _outlinePen;
        private static readonly Pen _selectionPen = BrushHelpers.CreateFrozenPen(Color.FromArgb(220, 40, 0, 120), 4.0);
        private static readonly Brush _selectionBrush = BrushHelpers.CreateFrozenBrush(Color.FromArgb(40, 40, 0, 120));
        private static readonly Brush _roomsOutsideOverdraw = BrushHelpers.CreateFrozenBrush(Color.FromArgb(180, 240, 240, 240));
        private static readonly Brush _roomsLockedBrush = CreateHatchBrush(Color.FromArgb(50, 20, 20, 20));

        private Brush _roomsNormalBrush;
        private Brush _roomsWallBrush;

        public class DepthProbe
        {
            public Vector2 Position { get; set; }
            public Color Color { get; set; }

            public DepthProbe(DepthBar parent)
            {
                Color = parent.GetProbeColor();
            }
        }

        private struct RelevantRoom
        {
            public Room Room;
            public Sector Sector;
            public float MinDepth;
            public float MaxDepth;
        }

        private enum SelectionMode
        {
            None,
            SelectedLimit0,
            SelectedLimit1,
            SelectedLimitBoth,
            RoomMove
        }
        private SelectionMode _selectionMode = SelectionMode.None;

        public DepthBar(Editor editor)
        {
            _editor = editor;
            UpdateBrushes();
        }

        public void UpdateBrushes()
        {
            _roomsNormalBrush = BrushHelpers.CreateFrozenBrush(_editor.Configuration.UI_ColorScheme.ColorFloor.ToWPFColor());
            _roomsWallBrush = BrushHelpers.CreateFrozenBrush(_editor.Configuration.UI_ColorScheme.ColorWall.ToWPFColor());
        }

        public Rect GetBarArea(Size parentControlSize)
        {
            float barsWidth = _barWidth * (DepthProbes.Count + 1);
            return new Rect(
                parentControlSize.Width - barsWidth - _marginX, _marginYUp,
                barsWidth, Math.Max(parentControlSize.Height - (_marginYUp + _marginYDown), 64.0));
        }

        private Color GetProbeColor()
        {
            return DepthProbes
                .Select(probe => probe.Color)
                .Concat(ProbeColors) // Make a list of *all* colors
                .GroupBy(color => color) // Group same colors
                .OrderBy(group => group.Count()) // Sort color groups after how often they appeared
                .First().First(); // Use first color of the least occurring color group.
        }

        private float FromVisualY(Rect barArea, double y)
        {
            float depth = MaxDepth - (float)(y - barArea.Y) / (float)barArea.Height * (MaxDepth - MinDepth);
            depth = Math.Max(Math.Min(depth, MaxDepth), MinDepth);
            return depth;
        }

        private double ToVisualY(Rect barArea, float depth)
        {
            return (MaxDepth - depth) / (MaxDepth - MinDepth) * barArea.Height + barArea.Y;
        }

        /// <returns>true, if the selection should continue in the background of the bar.</returns>
        public bool MouseDown(Point location, MouseButton button, Size parentControlSize, Vector2 clickPos)
        {
            _selectionMode = SelectionMode.None;
            _overallDelta = 0;

            // check if the mouse click was in the bar area
            Rect barArea = GetBarArea(parentControlSize);
            Rect selectionArea = barArea;
            selectionArea.Inflate(10.0, _selectionMaxPixelDistanceForMove * 0.8);
            if (!selectionArea.Contains(location))
                return true;

            switch (button)
            {
                case MouseButton.Left:
                    // check if the mouse click was on one of the two sliders
                    double distanceToSelectedLimit0 = Math.Abs(location.Y - ToVisualY(barArea, _selectedLimit0));
                    double distanceToSelectedLimit1 = Math.Abs(location.Y - ToVisualY(barArea, _selectedLimit1));
                    if (distanceToSelectedLimit0 < distanceToSelectedLimit1)
                    {
                        if (distanceToSelectedLimit0 < _selectionMaxPixelDistanceForMove)
                            _selectionMode = SelectionMode.SelectedLimit0;
                    }
                    else
                    {
                        if (distanceToSelectedLimit1 < _selectionMaxPixelDistanceForMove)
                            _selectionMode = SelectionMode.SelectedLimit1;
                    }

                    // check if a the click happend on a room
                    if (barArea.Contains(location) && _selectionMode == SelectionMode.None)
                    {
                        for (int groupIndex = 0; groupIndex < GroupCount; ++groupIndex)
                        {
                            Rect groupArea = groupGetArea(barArea, groupIndex);
                            if (groupArea.Contains(location))
                            {
                                _groupMouseClicked = groupIndex;

                                float mouseDepth = FromVisualY(barArea, location.Y);
                                List<List<RelevantRoom>> roomSequences = groupBuildRoomSequences(clickPos, groupIndex);
                                double sequenceWidth = groupArea.Width / roomSequences.Count;
                                for (int i = 0; i < roomSequences.Count; ++i)
                                {
                                    double posX0 = groupArea.X + sequenceWidth * i;
                                    double posX1 = groupArea.X + sequenceWidth * (i + 1);
                                    if (location.X >= posX0 && location.X <= posX1)
                                        for (int j = roomSequences[i].Count - 1; j >= 0; --j)
                                            if (mouseDepth <= roomSequences[i][j].MaxDepth && mouseDepth >= roomSequences[i][j].MinDepth)
                                            {
                                                _roomMouseClicked = roomSequences[i][j].Room;
                                                _roomMouseOffset = mouseDepth - Clicks.FromWorld(_roomMouseClicked.Position.Y);

                                                // If multiple rooms are selected, don't reset selection.
                                                if (_editor.SelectedRooms.Count <= 1 || !_editor.SelectedRooms.Contains(_roomMouseClicked))
                                                    SelectedRoom?.Invoke(new[] { _roomMouseClicked });

                                                InvalidateParent?.Invoke();
                                                _selectionMode = SelectionMode.RoomMove;
                                                return false;
                                            }
                                }
                                break;
                            }
                        }

                        selectionArea.Y = ToVisualY(barArea, Math.Max(_selectedLimit0, _selectedLimit1));
                        selectionArea.Height = Math.Abs(ToVisualY(barArea, _selectedLimit0) - ToVisualY(barArea, _selectedLimit1));

                        if (selectionArea.Contains(location))
                        {
                            _barMouseOffset = (float)distanceToSelectedLimit0;
                            _selectionMode = SelectionMode.SelectedLimitBoth;
                        }
                    }
                    break;

                case MouseButton.Middle:
                case MouseButton.XButton1:
                case MouseButton.XButton2:
                    for (int groupIndex = 0; groupIndex < DepthProbes.Count; ++groupIndex)
                    {
                        Rect groupArea = groupGetArea(barArea, groupIndex);
                        if (groupArea.Contains(location))
                        {
                            DepthProbes.RemoveAt(groupIndex);
                            InvalidateParent?.Invoke();
                            break;
                        }
                    }
                    break;
            }

            return false;
        }

        public void MouseMove(Point location, Size parentControlSize)
        {
            Rect barArea = GetBarArea(parentControlSize);

            switch (_selectionMode)
            {
                case SelectionMode.SelectedLimit0:
                    _selectedLimit0 = FromVisualY(barArea, location.Y);
                    InvalidateParent?.Invoke();
                    break;

                case SelectionMode.SelectedLimit1:
                    _selectedLimit1 = FromVisualY(barArea, location.Y);
                    InvalidateParent?.Invoke();
                    break;

                case SelectionMode.SelectedLimitBoth:
                    float barHeight = _selectedLimit1 - _selectedLimit0;
                    float newBarPos = FromVisualY(barArea, _barMouseOffset + location.Y);
                    newBarPos = (float)Math.Max(Math.Min(newBarPos + (double)barHeight, MaxDepth), MinDepth) - barHeight;

                    _selectedLimit0 = newBarPos;
                    _selectedLimit1 = newBarPos + barHeight;
                    InvalidateParent?.Invoke();
                    break;

                case SelectionMode.RoomMove:
                    float destinationHeight = FromVisualY(barArea, location.Y) - _roomMouseOffset;

                    if (_roomsToMove == null)
                    {
                        HashSet<Room> roomsToMove = new();

                        // If multiple rooms are selected, build a list of rooms to move based on that.
                        // Otherwise, use only room which was clicked before.
                        if (_editor.SelectedRooms.Count > 1)
                            foreach (var room in _editor.SelectedRooms)
                                roomsToMove.UnionWith(_editor.Level.GetConnectedRooms(room));
                        else
                            roomsToMove = _editor.Level.GetConnectedRooms(_roomMouseClicked);


                        if (EditorActions.CheckForLockedRooms(WPFUtils.GetWindowOwner(), roomsToMove))
                        {
                            _roomsToMove = null;
                            _selectionMode = SelectionMode.None;
                            break;
                        }
                        _roomsToMove = roomsToMove;
                        InvalidateParent?.Invoke();
                    }

                    // limit room movement to valid range
                    float maxHeight = MaxDepth;
                    float minHeight = MinDepth;
                    foreach (Room room in _roomsToMove)
                    {
                        float roomUpperLimit = MaxDepth - (Clicks.FromWorld(room.Position.Y) - Clicks.FromWorld(_roomMouseClicked.Position.Y) + Clicks.FromWorld(room.GetHighestCorner()));
                        float roomLowerLimit = MinDepth - (Clicks.FromWorld(room.Position.Y) - Clicks.FromWorld(_roomMouseClicked.Position.Y) + Clicks.FromWorld(room.GetLowestCorner()));
                        maxHeight = Math.Min(maxHeight, roomUpperLimit);
                        minHeight = Math.Max(minHeight, roomLowerLimit);
                    }

                    destinationHeight = Math.Max(Math.Min(destinationHeight, maxHeight), minHeight);
                    int delta = (int)(Math.Ceiling(destinationHeight) - Clicks.FromWorld(_roomMouseClicked.Position.Y));

                    // Snapping
                    if (!(Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)))
                    {
                        HashSet<Room> roomsInGroup = new();
                        List<List<RelevantRoom>> roomSequences = groupBuildRoomSequences(Vector2.Zero, _groupMouseClicked);

                        for (int i = 0; i < roomSequences.Count; ++i)
                            for (int j = 0; j < roomSequences[i].Count; ++j)
                                roomsInGroup.Add(roomSequences[i][j].Room);

                        int highestGroupPoint = _editor.Level.GetHighestRoomGroupPoint(_roomsToMove);
                        int lowestGroupPoint = _editor.Level.GetLowestRoomGroupPoint(_roomsToMove);

                        Room nearbyRoom = _editor.Level.GetNearbyRoomBelow(_roomsToMove, roomsInGroup, lowestGroupPoint, _snappingMargin);

                        if (nearbyRoom != null)
                        {
                            if (Math.Abs(delta) <= _snappingMargin)
                            {
                                int newDelta = -(lowestGroupPoint - (Clicks.FromWorld(nearbyRoom.Position.Y) + Clicks.FromWorld(nearbyRoom.GetHighestCorner())));
                                if (newDelta + highestGroupPoint > MaxDepth)
                                    break; // Do not push room out of bounds
                                delta = newDelta;
                            }
                            else if (Math.Abs(delta) <= _snappingMargin + 1)
                                break; // Noise reduction
                        }
                        else
                        {
                            nearbyRoom = _editor.Level.GetNearbyRoomAbove(_roomsToMove, roomsInGroup, highestGroupPoint, 5);
                            if (nearbyRoom != null)
                            {
                                if (Math.Abs(delta) <= _snappingMargin)
                                {
                                    int newDelta = Clicks.FromWorld(nearbyRoom.Position.Y) + Clicks.FromWorld(nearbyRoom.GetLowestCorner()) - highestGroupPoint;
                                    if (newDelta + lowestGroupPoint < MinDepth)
                                        break; // Do not push room out of bounds
                                    delta = newDelta;
                                }
                                else if (Math.Abs(delta) <= _snappingMargin + 1)
                                    break; // Noise reduction
                            }
                        }
                    }

                    // do movement
                    if (delta != 0)
                    {
                        _overallDelta += delta;
                        EditorActions.MoveRooms(new VectorInt3(0, Clicks.ToWorld(delta), 0), _roomsToMove, true);
                    }
                    break;
            }
        }

        public void MouseUp()
        {
            _selectionMode = SelectionMode.None;
            if (_roomsToMove != null)
            {
                _editor.UndoManager.PushRoomsMoved(_roomsToMove.ToList(), new VectorInt3(0, _overallDelta, 0));
                InvalidateParent?.Invoke();
                _roomsToMove = null;
            }
            _roomMouseClicked = null;
        }

        public void Draw(DrawingContext dc, Size parentControlSize, Vector2 cursorPos, Func<Room, Brush, Brush> getRoomBrush, double pixelsPerDip)
        {
            Rect barArea = GetBarArea(parentControlSize);
            double selectedLimit0PosY = ToVisualY(barArea, _selectedLimit0);
            double selectedLimit1PosY = ToVisualY(barArea, _selectedLimit1);

            // Draw box
            dc.DrawRectangle(_backgroundBrush, null, barArea);

            // Draw height lines
            for (int depth = (int)MinDepth; depth <= (int)MaxDepth; ++depth)
            {
                double posY = ToVisualY(barArea, depth);
                dc.DrawLine(_heightLinesPen, new Point(barArea.Left, posY), new Point(barArea.Right, posY));
            }

            // Draw height strings
            for (int i = 0; i <= _heightStringCount; ++i)
            {
                float depth = MaxDepth - (MaxDepth - MinDepth) / _heightStringCount * i;
                double posY = ToVisualY(barArea, depth);

                // Hide height string when close to selection limits
                double distanceToSelectionLimit0 = Math.Abs(posY - selectedLimit0PosY);
                double distanceToSelectionLimit1 = Math.Abs(posY - selectedLimit1PosY);
                double distance = Math.Min(distanceToSelectionLimit0, distanceToSelectionLimit1) - _heightStringFontHeight * 0.85;

                if (distance > _heightStringFadeDistance)
                {
                    DrawHeightString(dc, barArea, _outlineBrush, (float)Math.Round(depth), pixelsPerDip);

                    if (i > 0 && i < _heightStringCount)
                        dc.DrawLine(_heightLinesBigPen, new Point(barArea.Left, posY), new Point(barArea.Right, posY));
                }
                else if (distance > 0.0)
                {
                    double alpha = distance / _heightStringFadeDistance;
                    Color baseColor = ((SolidColorBrush)_outlineBrush).Color;
                    Brush alphaBrush = BrushHelpers.CreateFrozenBrush(Color.FromArgb((byte)(baseColor.A * alpha), baseColor.R, baseColor.G, baseColor.B));
                    DrawHeightString(dc, barArea, alphaBrush, (float)Math.Round(depth), pixelsPerDip);

                    if (i > 0 && i < _heightStringCount)
                    {
                        Color bigColor = ((SolidColorBrush)_heightLinesBigPen.Brush).Color;
                        Pen alphaPen = BrushHelpers.CreateFrozenPen(Color.FromArgb((byte)(bigColor.A * alpha), bigColor.R, bigColor.G, bigColor.B), _heightLinesBigPen.Thickness);
                        dc.DrawLine(alphaPen, new Point(barArea.Left, posY), new Point(barArea.Right, posY));
                    }
                }
            }

            // Draw common selection range
            DrawHeightString(dc, barArea, _selectionPen.Brush, _selectedLimit0, pixelsPerDip, true);
            DrawHeightString(dc, barArea, _selectionPen.Brush, _selectedLimit1, pixelsPerDip, true);

            Rect selectionRect = new(
                new Point(barArea.Left, Math.Min(selectedLimit1PosY, selectedLimit0PosY)),
                new Size(_barWidth, Math.Abs(selectedLimit0PosY - selectedLimit1PosY)));
            dc.DrawRectangle(_selectionBrush, null, selectionRect);

            // Draw probe selection ranges
            for (int groupIndex = 0; groupIndex < DepthProbes.Count; ++groupIndex)
            {
                Rect groupArea = groupGetArea(barArea, groupIndex);
                selectionRect = new Rect(
                    new Point(groupArea.Left, Math.Min(selectedLimit1PosY, selectedLimit0PosY)),
                    new Size(groupArea.Width, Math.Abs(selectedLimit0PosY - selectedLimit1PosY)));

                Color pc = DepthProbes[groupIndex].Color;
                Brush b = BrushHelpers.CreateFrozenBrush(Color.FromArgb(60, pc.R, pc.G, pc.B));
                dc.DrawRectangle(b, null, selectionRect);
            }

            // Draw depth bar content
            for (int groupIndex = 0; groupIndex < GroupCount; ++groupIndex)
            {
                Rect groupArea = groupGetArea(barArea, groupIndex);

                // Draw sequences
                List<List<RelevantRoom>> roomSequences = groupBuildRoomSequences(cursorPos, groupIndex);
                double sequenceWidth = groupArea.Width / roomSequences.Count;
                for (int roomSequenceIndex = 0; roomSequenceIndex < roomSequences.Count; ++roomSequenceIndex)
                {
                    var roomSequence = roomSequences[roomSequenceIndex];
                    double posX0 = groupArea.X + sequenceWidth * roomSequenceIndex;
                    double posX1 = groupArea.X + sequenceWidth * (roomSequenceIndex + 1);
                    for (int i = 0; i < roomSequence.Count; ++i)
                    {
                        RelevantRoom room = roomSequence[i];
                        double posY0 = ToVisualY(groupArea, room.MaxDepth);
                        double posY1 = ToVisualY(groupArea, room.MinDepth);

                        // HACK: if a rooms is full of walls (but why a designer should do this???) then
                        // MaxDepth or MinDepth can be int.MinValue or int.MaxValue and posY0 or posY1 get fucked...
                        // However I'm solving the issue elsewhere, this hack is here as a really last chance for
                        // the 0.0000000001% of the cases that could be wrong
                        if (Math.Abs(posY0) >= short.MaxValue || Math.Abs(posY1) >= short.MaxValue)
                            continue;

                        // Draw fill color for room
                        Brush colorBrush = _roomsNormalBrush;
                        Rect roomRect = new(posX0, posY0, posX1 - posX0, posY1 - posY0);
                        if (room.Sector != null && room.Sector.Type != SectorType.Floor)
                            colorBrush = _roomsWallBrush;
                        Brush colorBrush2 = getRoomBrush(room.Room, colorBrush);
                        dc.DrawRectangle(colorBrush2, null, roomRect);
                        if (!CheckRoom(room.MinDepth, room.MaxDepth))
                            dc.DrawRectangle(_roomsOutsideOverdraw, null, roomRect);
                        if (room.Room.Properties.Locked)
                            dc.DrawRectangle(_roomsLockedBrush, null, roomRect);

                        // Find portals on the selected sector
                        Pen belowPen = _roomBoundsPen;
                        if (room.Sector != null && room.Sector.FloorPortal != null)
                        {
                            Room portalRoom = room.Sector.FloorPortal.AdjoiningRoom;
                            if (i - 1 >= 0 && roomSequence[i - 1].Room == portalRoom)
                                belowPen = _portalPen;
                        }
                        Pen abovePen = _roomBoundsPen;
                        if (room.Sector != null && room.Sector.CeilingPortal != null)
                        {
                            Room portalRoom = room.Sector.CeilingPortal.AdjoiningRoom;
                            if (i + 1 < roomSequence.Count && roomSequence[i + 1].Room == portalRoom)
                                abovePen = _portalPen;
                        }

                        //Draw room borders
                        dc.DrawLine(belowPen, new Point(posX0, posY1), new Point(posX1, posY1));
                        dc.DrawLine(abovePen, new Point(posX0, posY0), new Point(posX1, posY0));
                        dc.DrawLine(_sequenceSeperatorPen, new Point(posX0, posY0), new Point(posX0, posY1));
                        dc.DrawLine(_sequenceSeperatorPen, new Point(posX1, posY0), new Point(posX1, posY1));
                    }
                }
            }

            // Draw outline around the groups
            for (int groupIndex = 0; groupIndex < GroupCount; ++groupIndex)
            {
                Rect groupArea = groupGetArea(barArea, groupIndex);
                dc.DrawRectangle(null, _outlinePen, groupArea);
            }
        }

        private void DrawHeightString(DrawingContext dc, Rect barArea, Brush brush, float depth, double pixelsPerDip, bool selection = false)
        {
            double screenPosY = ToVisualY(barArea, depth);

            if (selection)
            {
                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    ctx.BeginFigure(new Point(barArea.X - _heightStringArrowSize - 4, screenPosY + _heightStringArrowSize / 1.5), false, true);
                    ctx.LineTo(new Point(barArea.X - _heightStringArrowSize - 4, screenPosY - _heightStringArrowSize / 1.5), true, false);
                    ctx.LineTo(new Point(barArea.X - 4, screenPosY), true, false);
                }
                geometry.Freeze();
                Pen pen = brush == _selectionPen.Brush ? _selectionPen : BrushHelpers.CreateFrozenPen(brush, 1.0);
                dc.DrawGeometry(null, pen, geometry);
            }
            else
                dc.DrawLine(BrushHelpers.CreateFrozenPen(brush, 1.0), new Point(barArea.X, screenPosY), new Point(barArea.X - _heightStringLineLength, screenPosY));

            string value = Math.Abs(depth) < 1000 ? string.Format("{0:F0}", depth) : string.Format("{0:F0}", depth / 1000) + "k";
            string text = selection ? "y = " + value : value;

            var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                _heightStringTypeface, _heightStringFontSize, brush, pixelsPerDip)
            {
                TextAlignment = TextAlignment.Right,
                MaxTextWidth = Math.Max(0.0, barArea.X - (_heightStringLineDistance + _heightStringLineLength))
            };

            dc.DrawText(formatted, new Point(0.0, screenPosY - formatted.Height / 2.0));
        }

        private int GroupCount
        {
            get { return DepthProbes.Count + 1; }
        }

        public Rect groupGetArea(Rect barArea, int groupIndex)
        {
            return new Rect(barArea.Right - (groupIndex + 1) * _barWidth, barArea.Y, _barWidth, barArea.Height);
        }

        private List<List<RelevantRoom>> groupBuildRoomSequences(Vector2 cursorPos, int groupIndex)
        {
            // Decide what bar is at the given index
            Vector2 probePos = groupIndex == DepthProbes.Count ? cursorPos : DepthProbes[groupIndex].Position;
            bool shouldCheckRoomsToMove = groupIndex == DepthProbes.Count && _roomsToMove != null;

            // Iterate over all rooms under the cursor and add them to the room sequences
            IEnumerable<Room> sortedRoomList = _editor.Level.GetVerticallyAscendingRoomList(room =>
            {
                Vector2 roomLocal = probePos - room.SectorPos;
                bool CollidesWithProbe = roomLocal.X >= 1 && roomLocal.Y >= 1 && roomLocal.X < room.NumXSectors - 1 && roomLocal.Y < room.NumZSectors - 1;
                return shouldCheckRoomsToMove ? _roomsToMove.Contains(room) : CollidesWithProbe;
            });

            var roomSequences = new List<List<RelevantRoom>>();
            foreach (Room room in sortedRoomList)
            {
                Vector2 roomLocal = probePos - room.SectorPos;
                bool CollidesWithProbe = roomLocal.X >= 1 && roomLocal.Y >= 1 && roomLocal.X < room.NumXSectors - 1 && roomLocal.Y < room.NumZSectors - 1;

                Sector sector = CollidesWithProbe ? room.Sectors[(int)roomLocal.X, (int)roomLocal.Y] : null;
                RelevantRoom relevantRoom = new()
                {
                    Room = room,
                    Sector = sector,
                    MinDepth = Clicks.FromWorld(room.Position.Y) + Clicks.FromWorld(room.GetLowestCorner()),
                    MaxDepth = Clicks.FromWorld(room.Position.Y) + Clicks.FromWorld(room.GetHighestCorner())
                };

                // Search for a fit in the sequence for rooms it the current room is connected to on this sector
                if (sector != null && sector.FloorPortal != null)
                {
                    var portal = sector.FloorPortal;
                    var roomAbove = portal.AdjoiningRoom;
                    foreach (var roomSequence in roomSequences)
                        if (roomSequence.Last().Room == roomAbove)
                        {
                            roomSequence.Add(relevantRoom);
                            goto AddedRoomSucessfully;
                        }
                }
                roomSequences.Add(new List<RelevantRoom>());
                roomSequences.Last().Add(relevantRoom);
                AddedRoomSucessfully:
                ;
            }

            // Also try moving rooms into the same sequence that are connected directly, just not on the square that is selected
            for (int i = 1; i < roomSequences.Count; ++i) // triangular iteration
            {
                var roomSequenceAbove = roomSequences[i];
                foreach (var portal in roomSequenceAbove[0].Room.Portals)
                {
                    var connectedRoom = portal.AdjoiningRoom;

                    for (int j = 0; j < i; ++j)
                    {
                        var roomSequenceBelow = roomSequences[j];
                        if (roomSequences[j].Last().Room != connectedRoom)
                            continue;

                        float distanceBetweenSequences = roomSequenceAbove[0].MinDepth - roomSequenceBelow.Last().MaxDepth;
                        if (!(distanceBetweenSequences >= 0.0))
                            continue;

                        roomSequences[j].AddRange(roomSequences[i]);
                        roomSequences.RemoveAt(i);
                        --i;
                        goto NextRoom;
                    }
                }
                NextRoom:
                ;
            }

            // Try to merge independent sequences if they are sufficently far apart
            for (int i = 0; i < roomSequences.Count; ++i)
            {
                var roomSequenceAbove = roomSequences[i];
                for (int j = 0; j < roomSequences.Count; ++j)
                {
                    var roomSequenceBelow = roomSequences[j];
                    float distanceBetweenSequences = roomSequenceAbove[0].MinDepth - roomSequenceBelow.Last().MaxDepth;
                    if (distanceBetweenSequences >= _minDepthDifferenceBetweenIndependentlyMergedSequences)
                    {
                        roomSequenceBelow.AddRange(roomSequenceAbove);
                        roomSequences.RemoveAt(i);
                        --i;
                        goto NextRoom;
                    }
                }
                NextRoom:
                ;
            }

            return roomSequences;
        }

        public float SelectedMin => Math.Min(_selectedLimit0, _selectedLimit1);
        public float SelectedMax => Math.Max(_selectedLimit0, _selectedLimit1);
        public HashSet<Room> RoomsToMove => _roomsToMove;
        public bool CheckRoom(float roomMinDepth, float roomMaxDepth) => roomMinDepth >= SelectedMin && roomMaxDepth <= SelectedMax;
        public bool CheckRoom(Room room) => CheckRoom(
            Clicks.FromWorld(room.Position.Y) + Clicks.FromWorld(room.GetLowestCorner()),
            Clicks.FromWorld(room.Position.Y) + Clicks.FromWorld(room.GetHighestCorner()));

        private static Pen CreateDottedPen(Color color, double thickness)
        {
            var pen = new Pen(BrushHelpers.CreateFrozenBrush(color), thickness)
            {
                DashStyle = DashStyles.Dot
            };
            pen.Freeze();
            return pen;
        }

        private static Brush CreateHatchBrush(Color color)
        {
            // Approximates the previous WideUpwardDiagonal hatch over a transparent background.
            var geometry = new GeometryGroup();
            geometry.Children.Add(new LineGeometry(new Point(0, 8), new Point(8, 0)));

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
