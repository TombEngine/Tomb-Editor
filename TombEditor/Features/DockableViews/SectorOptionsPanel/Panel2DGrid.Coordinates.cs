#nullable enable

using System;
using System.Windows;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.Features.DockableViews.SectorOptionsPanel;

// Grid coordinate calculations.
public partial class Panel2DGrid
{
    private VectorInt2 RoomSize => Room?.SectorSize ?? new VectorInt2(Room.DefaultRoomDimensions, Room.DefaultRoomDimensions);

    private VectorInt2 GetGridDimensions()
        => VectorInt2.Max(RoomSize, new VectorInt2(Room.DefaultRoomDimensions, Room.DefaultRoomDimensions));

    private double GetFitGridStep()
    {
        var gridDimensions = GetGridDimensions();
        double w = Math.Max(0.0, ActualWidth - BorderThickness);
        double h = Math.Max(0.0, ActualHeight - BorderThickness);

        if (w <= 0.0 || h <= 0.0)
            return 0.0;

        if (w * gridDimensions.Y < h * gridDimensions.X)
            return w / gridDimensions.X;
        else
            return h / gridDimensions.Y;
    }

    private double GetGridStep()
        => GetFitGridStep() * _viewScale;

    private Rect GetVisualAreaTotal()
    {
        var area = GetCenteredVisualAreaTotal(GetGridStep());

        return new Rect(
            area.X + _viewOffsetX,
            area.Y + _viewOffsetY,
            area.Width,
            area.Height);
    }

    private Rect GetCenteredVisualAreaTotal(double gridStep)
    {
        double w = ActualWidth;
        double h = ActualHeight;
        var gridDimensions = GetGridDimensions();
        double gridW = (gridDimensions.X * gridStep) + BorderThickness;
        double gridH = (gridDimensions.Y * gridStep) + BorderThickness;

        return new Rect(
            (w - gridW) * 0.5,
            (h - gridH) * 0.5,
            gridW,
            gridH);
    }

    private Rect GetVisualAreaRoom()
    {
        var totalArea = GetGridLineArea(GetVisualAreaTotal());
        double gridStep = GetGridStep();
        var gridDimensions = GetGridDimensions();
        var roomSize = RoomSize;

        return new Rect(
            totalArea.X + (gridStep * ((gridDimensions.X - roomSize.X) / 2)),
            totalArea.Y + (gridStep * ((gridDimensions.Y - roomSize.Y) / 2)),
            gridStep * roomSize.X,
            gridStep * roomSize.Y);
    }

    private Point ToVisualCoord(VectorInt2 sectorCoord)
    {
        var roomArea = GetVisualAreaRoom();
        double gridStep = GetGridStep();

        return new Point(
            (sectorCoord.X * gridStep) + roomArea.X,
            roomArea.Bottom - ((sectorCoord.Y + 1) * gridStep));
    }

    private Rect ToVisualCoord(RectangleInt2 sectorArea)
    {
        var p0 = ToVisualCoord(sectorArea.Start);
        var p1 = ToVisualCoord(sectorArea.End);
        double gridStep = GetGridStep();

        return new Rect(
            Math.Min(p0.X, p1.X), Math.Min(p0.Y, p1.Y),
            Math.Abs(p1.X - p0.X) + gridStep, Math.Abs(p1.Y - p0.Y) + gridStep);
    }

    /// <summary>
    /// Converts a viewport-space point to a sector coordinate.
    /// </summary>
    /// <param name="point">The mouse position in control coordinates.</param>
    /// <param name="sectorCoord">Receives the resolved sector coordinate when the conversion succeeds.</param>
    /// <returns><see langword="true"/> when a sector coordinate could be resolved; otherwise <see langword="false"/>.</returns>
    private bool TryGetSectorFromVisualCoord(Point point, out VectorInt2 sectorCoord)
    {
        sectorCoord = default;

        var room = Room;

        if (room is null)
            return false;

        double gridStep = GetGridStep();

        if (gridStep <= 0.0)
            return false;

        var roomArea = GetVisualAreaRoom();
        var roomSize = room.SectorSize;

        if (roomSize.X <= 0 || roomSize.Y <= 0)
            return false;

        sectorCoord = new VectorInt2(
            (int)Math.Clamp((point.X - roomArea.X) / gridStep, 0.0, roomSize.X - 1),
            (int)Math.Clamp((roomArea.Bottom - point.Y) / gridStep, 0.0, roomSize.Y - 1));

        return true;
    }

    private static Rect GetGridLineArea(Rect totalArea)
    {
        return new Rect(
            totalArea.X + (BorderThickness / 2.0),
            totalArea.Y + (BorderThickness / 2.0),
            Math.Max(0.0, totalArea.Width - BorderThickness),
            Math.Max(0.0, totalArea.Height - BorderThickness));
    }
}
