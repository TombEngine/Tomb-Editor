#nullable disable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using TombLib;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.WPF;

namespace TombEditor.Features.Map2D
{
    // Rendering.
    public partial class Panel2DMap
    {
        private readonly Dictionary<(Color, Color, bool), Brush> _hatchBrushCache = new();

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (!_viewInitialized && ActualWidth > 0 && ActualHeight > 0)
            {
                ResetView();
                _viewInitialized = true;
            }

            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (_editor?.Level == null || _editor.SelectedRoom == null)
            {
                DrawDesignPlaceholder(dc);
                return;
            }

            try
            {
                double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
                var fullRect = new Rect(0, 0, ActualWidth, ActualHeight);
                Rect barArea = _depthBar.GetBarArea(ControlSize);

                Rectangle2 visibleArea = FromVisualCoord(fullRect);
                dc.DrawRectangle(_editor.Configuration.UI_ColorScheme.Color2DBackground.ToWPFBrush(), null, fullRect);

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
                        DrawRoom(dc, room, currentRangeMin, currentRangeMax, true, false);
                    }
                if (drewAny)
                    dc.DrawRectangle(_editor.Configuration.UI_ColorScheme.Color2DBackground.ToWPFBrush(0.7f), null, fullRect); // Make the rooms in the background appear faded

                // Draw grid lines
                Vector2 GridLines0 = FromVisualCoord(new Point());
                Vector2 GridLines1 = FromVisualCoord(new Point(ActualWidth, ActualHeight));
                Vector2 GridLinesStart = Vector2.Min(GridLines0, GridLines1);
                Vector2 GridLinesEnd = Vector2.Max(GridLines0, GridLines1);
                GridLinesStart = Vector2.Clamp(GridLinesStart, new Vector2(0.0f), new Vector2(_mapSize));
                GridLinesEnd = Vector2.Clamp(GridLinesEnd, new Vector2(0.0f), new Vector2(_mapSize));
                System.Drawing.Point GridLinesStartInt = new((int)Math.Floor(GridLinesStart.X), (int)Math.Floor(GridLinesStart.Y));
                System.Drawing.Point GridLinesEndInt = new((int)Math.Ceiling(GridLinesEnd.X), (int)Math.Ceiling(GridLinesEnd.Y));

                for (int x = GridLinesStartInt.X; x <= GridLinesEndInt.X; ++x)
                    dc.DrawLine(x % 10 == 0 ? _gridPenThick : _gridPenThin,
                        ToVisualCoord(new Vector2(x, 0)), ToVisualCoord(new Vector2(x, _mapSize)));

                for (int y = GridLinesStartInt.Y; y <= GridLinesEndInt.Y; ++y)
                    dc.DrawLine(y % 10 == 0 ? _gridPenThick : _gridPenThin,
                        ToVisualCoord(new Vector2(0, y)), ToVisualCoord(new Vector2(_mapSize, y)));

                // Draw visible rooms
                foreach (Room room in sortedRoomList)
                    if (_depthBar.CheckRoom(room)) // Check if the room fits the depth bar criterion
                        DrawRoom(dc, room, currentRangeMin, currentRangeMax, true, true);

                // Draw probe positions with digits
                for (int i = 0; i < _depthBar.DepthProbes.Count; ++i)
                {
                    Point depthProbeVisualPos = ToVisualCoord(_depthBar.DepthProbes[i].Position);

                    Color probeColor = _depthBar.DepthProbes[i].Color;
                    Brush probeBrush = BrushHelpers.CreateFrozenBrush(MixWith(probeColor, Colors.White, 0.765));
                    Pen probePen = BrushHelpers.CreateFrozenPen(MixWith(probeColor, Colors.White, 0.05), 2.0);
                    dc.DrawEllipse(probeBrush, probePen, depthProbeVisualPos, _probeRadius / 2, _probeRadius / 2);

                    Color textColor = MixWith(probeColor, Colors.White, 0.05);
                    Brush probeTextBrush = BrushHelpers.CreateFrozenBrush(textColor);

                    var formatted = new FormattedText(i.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        DepthBar.ProbeTypeface, DepthBar.ProbeFontSize, probeTextBrush, pixelsPerDip);
                    dc.DrawText(formatted, new Point(depthProbeVisualPos.X - formatted.Width / 2, depthProbeVisualPos.Y - formatted.Height / 2));

                    // Draw depth bar numbers
                    Rect groupArea = _depthBar.groupGetArea(barArea, i);
                    dc.DrawCenteredText(i.ToString(), new Rect(groupArea.X, 0, groupArea.Width, groupArea.Y),
                        DepthBar.ProbeTypeface, DepthBar.ProbeFontSize, probeTextBrush, pixelsPerDip, CultureInfo.CurrentCulture);
                }

                // Draw selection area
                if (_selectionArea != null)
                {
                    dc.DrawRectangle(_selectionAreaBrush, _selectionAreaPen, ToVisualCoord(_selectionArea._area));
                }

                // Draw insertion contour data
                if (_insertionContourLineData != null)
                    foreach (var contourLineSegment in _insertionContourLineData)
                    {
                        dc.DrawLine(_roomBorderPen,
                            ToVisualCoord(contourLineSegment.Start + _insertionCurrentOffset),
                            ToVisualCoord(contourLineSegment.End + _insertionCurrentOffset));
                    }

                // Draw depth bar
                Vector2 cursorPos = FromVisualCoord(_lastMousePosition);
                _depthBar.Draw(dc, ControlSize, cursorPos, GetRoomBrush, pixelsPerDip);
            }
            catch (Exception exc)
            {
                logger.Error(exc, "An exception occured while drawing the 2D map.");
            }
        }

        private void DrawRoom(DrawingContext dc, Room room, float currentRangeMin, float currentRangeMax, bool drawFilled, bool drawOutline)
        {
            int width = room.NumXSectors;
            int height = room.NumZSectors;
            if (drawFilled)
            {
                // Fill area of room with rectangular stripes
                List<Rect> rectangles = new();
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
                            rectangles.Add(new Rect(xBegin, z, x - xBegin, 1));
                        }

                    // Try to combine rectangle with the rectangle of the previous row
                    if (rectangles.Count >= 2 && previousRectangleCount + 1 == rectangles.Count)
                    {
                        Rect previousRectangle = rectangles[rectangles.Count - 2];
                        Rect thisRectangle = rectangles[rectangles.Count - 1];
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
                    rectangles[j] = new Rect(
                        ToVisualCoord(new Vector2((float)rectangles[j].X + room.SectorPos.X, (float)rectangles[j].Bottom + room.SectorPos.Y)),
                        new Size(rectangles[j].Width * _viewScale, rectangles[j].Height * _viewScale));

                // Draw the rectangular stripes
                if (rectangles.Count > 0)
                {
                    Brush brush = _roomsNormalBrush;
                    if (Clicks.FromWorld(room.Position.Y) + Clicks.FromWorld(room.GetHighestCorner()) <= currentRangeMin)
                        brush = _roomsNormalBelowBrush;
                    if (Clicks.FromWorld(room.Position.Y) + Clicks.FromWorld(room.GetLowestCorner()) >= currentRangeMax)
                        brush = _roomsNormalAboveBrush;
                    Brush brush2 = GetRoomBrush(room, brush);
                    foreach (var rect in rectangles)
                        dc.DrawRectangle(brush2, null, rect);
                    if (room.Properties.Locked)
                        foreach (var rect in rectangles)
                            dc.DrawRectangle(_roomsLockedBrush, null, rect);
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
                            dc.DrawLine(aboveSector.WallPortal != null || thisSector.WallPortal != null ? _roomPortalPen : _roomBorderPen,
                                ToVisualCoord(new Vector2(x + room.SectorPos.X, z + room.SectorPos.Y)),
                                ToVisualCoord(new Vector2(x + 1 + room.SectorPos.X, z + room.SectorPos.Y)));
                        if (leftSector.IsAnyWall != thisSector.IsAnyWall)
                            dc.DrawLine(leftSector.WallPortal != null || thisSector.WallPortal != null ? _roomPortalPen : _roomBorderPen,
                                ToVisualCoord(new Vector2(x + room.SectorPos.X, z + room.SectorPos.Y)),
                                ToVisualCoord(new Vector2(x + room.SectorPos.X, z + 1 + room.SectorPos.Y)));
                    }
            }
        }

        private Brush GetRoomBrush(Room room, Brush baseBrush)
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
            {
                if (baseBrush is SolidColorBrush solid)
                    return GetSelectionHatchBrush(_roomsSelectedColor, solid.Color, isSelected);
                else
                    return baseBrush;
            }
            else if (isSelected)
                return _roomsSelectedBrush;
            else
                return baseBrush;
        }

        private Brush GetSelectionHatchBrush(Color lineColor, Color backgroundColor, bool upward)
        {
            var key = (lineColor, backgroundColor, upward);
            if (_hatchBrushCache.TryGetValue(key, out var cached))
                return cached;

            var line = upward
                ? new LineGeometry(new Point(0, 6), new Point(6, 0))
                : new LineGeometry(new Point(0, 0), new Point(6, 6));

            var group = new DrawingGroup();
            group.Children.Add(new GeometryDrawing(BrushHelpers.CreateFrozenBrush(backgroundColor), null, new RectangleGeometry(new Rect(0, 0, 6, 6))));
            group.Children.Add(new GeometryDrawing(null, new Pen(BrushHelpers.CreateFrozenBrush(lineColor), 1.0), line));

            var brush = new DrawingBrush(group)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 6, 6),
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, 6, 6),
                ViewboxUnits = BrushMappingMode.Absolute
            };
            brush.Freeze();

            _hatchBrushCache[key] = brush;
            return brush;
        }

        private void DrawDesignPlaceholder(DrawingContext dc)
        {
            dc.DrawRectangle(_designPlaceholderBrush, _roomBorderPen, new Rect(0.0, 0.0, ActualWidth, ActualHeight));
            dc.DrawCenteredText("2D Room Rendering: Not Available!",
                new Rect(0.0, 0.0, ActualWidth, ActualHeight),
                new Typeface("Segoe UI"), 12.0, Brushes.DarkGray,
                VisualTreeHelper.GetDpi(this).PixelsPerDip, CultureInfo.CurrentCulture);
        }

        private static Color MixWith(Color firstColor, Color secondColor, double mixFactor)
        {
            if (mixFactor > 1)
                mixFactor = 1;
            if (!(mixFactor >= 0))
                mixFactor = 0;
            return Color.FromArgb(
                (byte)Math.Round(firstColor.A * (1 - mixFactor) + secondColor.A * mixFactor),
                (byte)Math.Round(firstColor.R * (1 - mixFactor) + secondColor.R * mixFactor),
                (byte)Math.Round(firstColor.G * (1 - mixFactor) + secondColor.G * mixFactor),
                (byte)Math.Round(firstColor.B * (1 - mixFactor) + secondColor.B * mixFactor));
        }
    }
}
