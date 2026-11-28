#nullable enable

using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.SectorOptionsPanel;

/// <summary>
/// Sector-tile painting shared by the WPF 2D grids (<see cref="Panel2DGrid"/> and the
/// resize-room preview): renders the <see cref="SectorColoringManager"/> shapes for one
/// sector into a <see cref="DrawingContext"/>.
/// </summary>
internal static class SectorTileRenderer
{
    private const double OutlineSectorColoringInfoWidth = 3.0;
    private const double HatchPenThickness = 3.5;
    private const double HatchSpacing = 8.0;

    internal static void PaintSectorTile(DrawingContext dc, Editor editor, Room room, Resources resources, Rect sectorArea, int x, int z)
    {
        var coloringInfos = editor.SectorColoringManager.ColoringInfo.GetColors(
            editor.Configuration.UI_ColorScheme, room, x, z,
            editor.Configuration.UI_ProbeAttributesThroughPortals);

        if (coloringInfos is null)
            return;

        for (int i = 0; i < coloringInfos.Count; i++)
        {
            var info = coloringInfos[i];

            switch (info.Shape)
            {
                case SectorColoringShape.Rectangle:
                    dc.DrawRectangle(resources.GetBrush(info.Color), null, sectorArea);
                    break;

                case SectorColoringShape.Frame:
                    DrawFrame(dc, resources, sectorArea, info.Color);
                    break;

                case SectorColoringShape.Hatch:
                    DrawHatch(dc, resources, sectorArea, info.Color);
                    break;

                case SectorColoringShape.EdgeZp:
                case SectorColoringShape.EdgeZn:
                case SectorColoringShape.EdgeXp:
                case SectorColoringShape.EdgeXn:
                    DrawEdge(dc, sectorArea, info.Shape, resources.GetBrush(info.Color));
                    break;

                case SectorColoringShape.TriangleXnZn:
                case SectorColoringShape.TriangleXnZp:
                case SectorColoringShape.TriangleXpZn:
                case SectorColoringShape.TriangleXpZp:
                    DrawTriangle(dc, sectorArea, info.Shape, resources.GetBrush(info.Color));
                    break;
            }
        }
    }

    private static void DrawFrame(DrawingContext dc, Resources resources, Rect sectorArea, Vector4 color)
    {
        if (sectorArea.Width <= OutlineSectorColoringInfoWidth || sectorArea.Height <= OutlineSectorColoringInfoWidth)
            return;

        var pen = resources.GetPen(color, OutlineSectorColoringInfoWidth);
        dc.DrawRectangleInside(pen, sectorArea);
    }

    private static void DrawHatch(DrawingContext dc, Resources resources, Rect sectorArea, Vector4 color)
    {
        var pen = resources.GetPen(color, HatchPenThickness);
        var clip = new RectangleGeometry(sectorArea);
        clip.Freeze();

        dc.PushClip(clip);

        try
        {
            double startOffset = -sectorArea.Height / 2.0;
            double totalRange = sectorArea.Width + sectorArea.Height;
            double margin = (HatchPenThickness / 2.0) + 0.5;

            for (double offset = startOffset; offset < totalRange; offset += HatchSpacing)
            {
                dc.DrawLine(pen,
                    new Point(sectorArea.X - sectorArea.Height + offset - margin, sectorArea.Bottom + margin),
                    new Point(sectorArea.X + offset + margin, sectorArea.Top - margin));
            }
        }
        finally
        {
            dc.Pop();
        }
    }

    private static void DrawEdge(DrawingContext dc, Rect sectorArea, SectorColoringShape shape, Brush brush)
    {
        const double w = OutlineSectorColoringInfoWidth;

        var edgeRect = shape switch
        {
            SectorColoringShape.EdgeZp => new Rect(sectorArea.X, sectorArea.Y, sectorArea.Width, w),
            SectorColoringShape.EdgeZn => new Rect(sectorArea.X, sectorArea.Bottom - w, sectorArea.Width, w),
            SectorColoringShape.EdgeXp => new Rect(sectorArea.Right - w, sectorArea.Y, w, sectorArea.Height),
            _ => new Rect(sectorArea.X, sectorArea.Y, w, sectorArea.Height)
        };

        dc.DrawRectangle(brush, null, edgeRect);
    }

    private static void DrawTriangle(DrawingContext dc, Rect sectorArea, SectorColoringShape shape, Brush brush)
    {
        var (p0, p1, p2) = shape switch
        {
            SectorColoringShape.TriangleXnZn => (
                new Point(sectorArea.Left, sectorArea.Top),
                new Point(sectorArea.Left, sectorArea.Bottom),
                new Point(sectorArea.Right, sectorArea.Bottom)),
            SectorColoringShape.TriangleXnZp => (
                new Point(sectorArea.Left, sectorArea.Bottom),
                new Point(sectorArea.Left, sectorArea.Top),
                new Point(sectorArea.Right, sectorArea.Top)),
            SectorColoringShape.TriangleXpZn => (
                new Point(sectorArea.Left, sectorArea.Bottom),
                new Point(sectorArea.Right, sectorArea.Top),
                new Point(sectorArea.Right, sectorArea.Bottom)),
            _ => (
                new Point(sectorArea.Left, sectorArea.Top),
                new Point(sectorArea.Right, sectorArea.Top),
                new Point(sectorArea.Right, sectorArea.Bottom))
        };

        var geometry = new StreamGeometry();

        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(p0, true, true);
            ctx.LineTo(p1, false, false);
            ctx.LineTo(p2, false, false);
        }

        geometry.Freeze();
        dc.DrawGeometry(brush, null, geometry);
    }

    /// <summary>Per-render brush/pen cache (frozen resources, keyed by color).</summary>
    internal sealed class Resources
    {
        private readonly Dictionary<Color, Brush> _brushes = [];
        private readonly Dictionary<PenKey, Pen> _pens = [];

        public Brush GetBrush(Vector4 color)
            => GetBrush(color.ToWPFColor());

        public Brush GetBrush(Color color)
        {
            if (_brushes.TryGetValue(color, out var brush))
                return brush;

            brush = BrushHelpers.CreateFrozenBrush(color);
            _brushes.Add(color, brush);
            return brush;
        }

        public Pen GetPen(Vector4 color, double thickness)
            => GetPen(color.ToWPFColor(), thickness);

        private Pen GetPen(Color color, double thickness)
        {
            var key = new PenKey(color, thickness);

            if (_pens.TryGetValue(key, out var pen))
                return pen;

            pen = BrushHelpers.CreateFrozenPen(color, thickness);
            _pens.Add(key, pen);
            return pen;
        }

        private readonly record struct PenKey(Color Color, double Thickness);
    }
}
