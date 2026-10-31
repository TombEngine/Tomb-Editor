#nullable enable

using System;
using System.Windows;
using System.Windows.Media;
using TombEditor.Features.DockableViews.SectorOptionsPanel;
using TombLib;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.ResizeRoom;

/// <summary>
/// Read-only WPF preview of the room being resized: the kept part of the old room is drawn
/// with the regular sector coloring (shared <see cref="SectorTileRenderer"/>), the new ring
/// as border wall and the added area as floor/wall fill on a native WPF drawing surface.
/// </summary>
public class ResizeRoomGridControl : FrameworkElement
{
    private const double BorderThickness = 1.0;

    private static readonly Pen GridPen = BrushHelpers.CreateFrozenPen(Color.FromArgb(140, 0, 0, 0), BorderThickness);
    private static readonly Pen BorderPen = BrushHelpers.CreateFrozenPen(Colors.Black, BorderThickness);

    public Room? Room { get; set; }
    public RectangleInt2 NewArea { get; set; }
    public bool UseFloor { get; set; }
    public ColorScheme? ColorScheme { get; set; }

    protected override void OnRender(DrawingContext dc)
    {
        var editor = Editor.Instance;
        var room = Room;
        var colorScheme = ColorScheme;

        if (editor is null || room is null || colorScheme is null)
            return;

        var gridDimensions = NewArea.Size + VectorInt2.One;
        if (gridDimensions.X <= 0 || gridDimensions.Y <= 0)
            return;

        double gridStep = Math.Min(ActualWidth / gridDimensions.X, ActualHeight / gridDimensions.Y);
        if (gridStep <= 0.0)
            return;

        var totalArea = new Rect(
            (ActualWidth - (gridStep * gridDimensions.X)) * 0.5,
            (ActualHeight - (gridStep * gridDimensions.Y)) * 0.5,
            gridStep * gridDimensions.X,
            gridStep * gridDimensions.Y);

        var resources = new SectorTileRenderer.Resources();
        dc.DrawRectangle(resources.GetBrush(colorScheme.Color2DBackground), null, totalArea);

        for (int x = 0; x < gridDimensions.X; x++)
        {
            for (int z = 0; z < gridDimensions.Y; z++)
            {
                var tileRect = new Rect(
                    totalArea.X + (x * gridStep),
                    totalArea.Y + ((gridDimensions.Y - 1 - z) * gridStep),
                    gridStep,
                    gridStep);

                PaintSectorTile(dc, editor, room, colorScheme, resources, tileRect, x, z);
            }
        }

        var gridArea = new Rect(
            totalArea.X + (BorderThickness / 2.0),
            totalArea.Y + (BorderThickness / 2.0),
            Math.Max(0.0, totalArea.Width - BorderThickness),
            Math.Max(0.0, totalArea.Height - BorderThickness));
        dc.DrawGridLines(GridPen, gridArea, gridDimensions.X, gridDimensions.Y);
        dc.DrawRectangleInside(BorderPen, totalArea);
    }

    private void PaintSectorTile(DrawingContext dc, Editor editor, Room room, ColorScheme colorScheme,
        SectorTileRenderer.Resources resources, Rect sectorArea, int x, int z)
    {
        RectangleInt2 newArea = NewArea;
        VectorInt2 old = new VectorInt2(x, z) + newArea.Start;

        // Draw new border wall
        if ((x == 0) || (z == 0) || (x == newArea.Width) || (z == newArea.Height))
        {
            if ((newArea.X0 == 0 && x == 0 && room.LocalArea.Inflate(0, -1).Contains(old) && 0 < z && z < newArea.Height) ||
                (newArea.Y0 == 0 && z == 0 && room.LocalArea.Inflate(-1, 0).Contains(old) && 0 < x && x < newArea.Width) ||
                (newArea.X1 == room.NumXSectors - 1 && x == newArea.Width && room.LocalArea.Inflate(0, -1).Contains(old) && 0 < z && z < newArea.Height) ||
                (newArea.Y1 == room.NumZSectors - 1 && z == newArea.Height && room.LocalArea.Inflate(-1, 0).Contains(old) && 0 < x && x < newArea.Width))
            {
                SectorTileRenderer.PaintSectorTile(dc, editor, room, resources, sectorArea, old.X, old.Y);
            }
            else
            {
                dc.DrawRectangle(resources.GetBrush(colorScheme.ColorBorderWall), null, sectorArea);
            }

            return;
        }

        // Draw inner parts of the old room
        if (old.X > 0 && old.Y > 0 && old.X < (room.NumXSectors - 1) && old.Y < (room.NumZSectors - 1))
        {
            SectorTileRenderer.PaintSectorTile(dc, editor, room, resources, sectorArea, old.X, old.Y);
            return;
        }

        // Draw new floor / wall fill
        dc.DrawRectangle(resources.GetBrush(UseFloor ? colorScheme.ColorFloor : colorScheme.ColorWall), null, sectorArea);
    }
}
