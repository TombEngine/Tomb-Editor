#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.WPF.Features.AnimatedTextures;

/// <summary>
/// Draws every animated-texture set's frame outlines on a texture atlas, highlighting the selected
/// set and numbering its frames. Shared by the Tomb Editor and Wad Tool texture-map views (WPF
/// counterpart of the shared <c>PanelTextureMapForAnimations</c> painting).
/// </summary>
public static class AnimatedTextureSetsPainter
{
    private static readonly Pen _outlinePen = CreatePen(Color.FromArgb(80, 192, 192, 192));
    private static readonly Pen _activeOutlinePen = CreatePen(Color.FromArgb(200, 238, 82, 238));
    private static readonly Brush _textBrush = Freeze(new SolidColorBrush(Colors.Violet));
    private static readonly Brush _textShadowBrush = Freeze(new SolidColorBrush(Colors.Black));
    private static readonly Typeface _typeface = new("Segoe UI");

    private static Pen CreatePen(Color color)
    {
        var pen = new Pen(new SolidColorBrush(color), 2);
        pen.Freeze();
        return pen;
    }

    private static Brush Freeze(Brush brush)
    {
        brush.Freeze();
        return brush;
    }

    public static void Paint(DrawingContext drawingContext, IReadOnlyList<AnimatedTextureSet> sets,
        AnimatedTextureSet? selectedSet, Texture? visibleTexture, Func<Vector2, Point> toVisualCoord, double pixelsPerDip)
    {
        if (sets.Count == 0)
            return;

        foreach (AnimatedTextureSet set in sets)
            if (set != selectedSet)
                DrawSetOutlines(drawingContext, set, false, visibleTexture, toVisualCoord, pixelsPerDip);

        DrawSetOutlines(drawingContext, selectedSet, true, visibleTexture, toVisualCoord, pixelsPerDip);
    }

    private static void DrawSetOutlines(DrawingContext drawingContext, AnimatedTextureSet? set, bool current,
        Texture? visible, Func<Vector2, Point> toVisualCoord, double dpi)
    {
        if (set is null)
            return;

        for (int i = 0; i < set.Frames.Count; ++i)
        {
            AnimatedTextureFrame frame = set.Frames[i];

            if (visible is null || !Equals(frame.Texture, visible))
                continue;

            Point[] edges =
            {
                toVisualCoord(frame.TexCoord0),
                toVisualCoord(frame.TexCoord1),
                toVisualCoord(frame.TexCoord2),
                toVisualCoord(frame.TexCoord3)
            };

            double minX = Math.Min(Math.Min(edges[0].X, edges[1].X), Math.Min(edges[2].X, edges[3].X));
            double minY = Math.Min(Math.Min(edges[0].Y, edges[1].Y), Math.Min(edges[2].Y, edges[3].Y));
            double maxX = Math.Max(Math.Max(edges[0].X, edges[1].X), Math.Max(edges[2].X, edges[3].X));
            double maxY = Math.Max(Math.Max(edges[0].Y, edges[1].Y), Math.Max(edges[2].Y, edges[3].Y));

            if (current)
            {
                string counter = (i + 1) + "/" + set.Frames.Count;
                var text = new FormattedText(counter, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    _typeface, 12.0, _textBrush, dpi) { TextAlignment = TextAlignment.Left };
                var shadow = new FormattedText(counter, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    _typeface, 12.0, _textShadowBrush, dpi) { TextAlignment = TextAlignment.Left };

                if (maxX - minX > text.Width && maxY - minY > text.Height)
                {
                    var origin = new Point((minX + maxX) / 2.0 - text.Width / 2.0, (minY + maxY) / 2.0 - text.Height / 2.0);
                    drawingContext.DrawText(shadow, new Point(origin.X + 1, origin.Y + 1));
                    drawingContext.DrawText(text, origin);
                }
            }

            DrawClosedPolyline(drawingContext, current ? _activeOutlinePen : _outlinePen, edges);
        }
    }

    private static void DrawClosedPolyline(DrawingContext drawingContext, Pen pen, Point[] points)
    {
        if (points.Length == 0)
            return;

        var geometry = new StreamGeometry();
        using (StreamGeometryContext context = geometry.Open())
        {
            context.BeginFigure(points[0], false, true);
            for (int i = 1; i < points.Length; i++)
                context.LineTo(points[i], true, false);
        }
        geometry.Freeze();
        drawingContext.DrawGeometry(null, pen, geometry);
    }
}
