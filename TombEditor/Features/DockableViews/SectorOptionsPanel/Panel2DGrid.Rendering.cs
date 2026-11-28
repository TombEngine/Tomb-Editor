#nullable enable

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using TombLib;
using TombLib.LevelData;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.SectorOptionsPanel;

// Rendering. The per-sector shape painting is shared with the resize-room preview
// through SectorTileRenderer.
public partial class Panel2DGrid
{
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        CoerceViewOffset();
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        var editor = _editor;
        var room = Room;

        if (editor is null || room is null)
        {
            DrawDesignPlaceholder(dc);
            return;
        }

        try
        {
            var totalArea = GetVisualAreaTotal();
            var roomArea = GetVisualAreaRoom();
            var gridDimensions = GetGridDimensions();
            double gridStep = GetGridStep();
            var roomSize = room.SectorSize;

            if (gridStep <= 0.0)
                return;

            var resources = new SectorTileRenderer.Resources();
            dc.DrawRectangle(resources.GetBrush(editor.Configuration.UI_ColorScheme.Color2DBackground), null, totalArea);

            for (int x = 0; x < roomSize.X; x++)
            {
                for (int z = 0; z < roomSize.Y; z++)
                {
                    var tileRect = new Rect(
                        roomArea.X + (x * gridStep),
                        roomArea.Y + ((roomSize.Y - 1 - z) * gridStep),
                        gridStep,
                        gridStep);

                    SectorTileRenderer.PaintSectorTile(dc, editor, room, resources, tileRect, x, z);
                }
            }

            DrawGridLines(dc, totalArea, gridDimensions);
            DrawSelection(dc, editor, room, resources);
        }
        catch (Exception exc)
        {
            logger.Error(exc, "An exception occurred while drawing the 2D grid.");
        }
    }

    private static void DrawGridLines(DrawingContext dc, Rect totalArea, VectorInt2 gridDimensions)
    {
        var gridArea = GetGridLineArea(totalArea);
        dc.DrawGridLines(GridPen, gridArea, gridDimensions.X, gridDimensions.Y);
        dc.DrawRectangleInside(BorderPen, totalArea);
    }

    private void DrawSelection(DrawingContext dc, Editor editor, Room room, SectorTileRenderer.Resources resources)
    {
        if (editor.SelectedSectors.Valid)
        {
            var selectionPen = resources.GetPen(editor.Configuration.UI_ColorScheme.ColorSelection, 2.0);
            dc.DrawRectangleInside(selectionPen, ToVisualCoord(editor.SelectedSectors.Area));
        }

        if (editor.SelectedObject is SectorBasedObjectInstance instance && instance.Room == room)
        {
            var pen = instance is PortalInstance ? SelectedPortalPen : SelectedTriggerPen;
            dc.DrawRectangleInside(pen, ToVisualCoord(instance.Area));
        }
    }

    private void DrawDesignPlaceholder(DrawingContext dc)
    {
        dc.DrawRectangle(DesignPlaceholderBrush, BorderPen,
            new Rect(0.0, 0.0, ActualWidth, ActualHeight));

        dc.DrawCenteredText(
            "2D Grid",
            new Rect(0.0, 0.0, ActualWidth, ActualHeight),
            new Typeface("Segoe UI"),
            12.0,
            Brushes.Gray,
            VisualTreeHelper.GetDpi(this).PixelsPerDip,
            CultureInfo.InvariantCulture);
    }
}
