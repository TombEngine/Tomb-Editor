#nullable enable

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace TombLib.WPF;

/// <summary>
/// Provides reusable helpers for low-level <see cref="DrawingContext"/> rendering.
/// </summary>
public static class DrawingContextExtensions
{
    /// <summary>
    /// Draws centered text inside the specified bounds.
    /// </summary>
    public static void DrawCenteredText(
        this DrawingContext drawingContext,
        string text,
        Rect bounds,
        Typeface typeface,
        double emSize,
        Brush foreground,
        double pixelsPerDip,
        CultureInfo? culture = null,
        FlowDirection flowDirection = FlowDirection.LeftToRight)
    {
        ArgumentNullException.ThrowIfNull(drawingContext);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(typeface);
        ArgumentNullException.ThrowIfNull(foreground);

        if (bounds.Width <= 0.0 || bounds.Height <= 0.0)
            return;

        var formattedText = new FormattedText(
            text,
            culture ?? CultureInfo.CurrentUICulture,
            flowDirection,
            typeface,
            emSize,
            foreground,
            pixelsPerDip);

        drawingContext.DrawText(formattedText, new Point(
            bounds.X + ((bounds.Width - formattedText.Width) / 2.0),
            bounds.Y + ((bounds.Height - formattedText.Height) / 2.0)));
    }

    /// <summary>
    /// Draws internal grid lines within the specified bounds.
    /// </summary>
    public static void DrawGridLines(this DrawingContext drawingContext, Pen pen, Rect bounds, int columns, int rows)
    {
        ArgumentNullException.ThrowIfNull(drawingContext);
        ArgumentNullException.ThrowIfNull(pen);

        if (bounds.Width <= 0.0 || bounds.Height <= 0.0 || columns <= 0 || rows <= 0)
            return;

        var geometry = CreateGridLineGeometry(bounds, columns, rows);

        if (geometry is null)
            return;

        var guidelines = CreateGridLineGuidelines(bounds, columns, rows, pen.Thickness);
        drawingContext.PushGuidelineSet(guidelines);

        try
        {
            drawingContext.DrawGeometry(null, pen, geometry);
        }
        finally
        {
            drawingContext.Pop();
        }
    }

    /// <summary>
    /// Draws a rectangle while keeping the stroke inside the specified bounds.
    /// </summary>
    public static void DrawRectangleInside(this DrawingContext drawingContext, Pen pen, Rect bounds)
    {
        ArgumentNullException.ThrowIfNull(drawingContext);
        ArgumentNullException.ThrowIfNull(pen);

        if (bounds.Width <= 0.0 || bounds.Height <= 0.0)
            return;

        var adjustedBounds = bounds;

        if (adjustedBounds.Width > pen.Thickness && adjustedBounds.Height > pen.Thickness)
            adjustedBounds.Inflate(-pen.Thickness / 2.0, -pen.Thickness / 2.0);

        drawingContext.DrawRectangle(null, pen, adjustedBounds);
    }

    private static void AddLine(StreamGeometryContext context, Point start, Point end)
    {
        context.BeginFigure(start, false, false);
        context.LineTo(end, true, false);
    }

    private static StreamGeometry? CreateGridLineGeometry(Rect bounds, int columns, int rows)
    {
        var geometry = new StreamGeometry();
        bool hasLines = false;

        using (var context = geometry.Open())
        {
            for (int x = 1; x < columns; x++)
            {
                double xPos = bounds.X + ((bounds.Width * x) / columns);
                AddLine(context, new Point(xPos, bounds.Y), new Point(xPos, bounds.Bottom));
                hasLines = true;
            }

            for (int y = 1; y < rows; y++)
            {
                double yPos = bounds.Y + ((bounds.Height * y) / rows);
                AddLine(context, new Point(bounds.X, yPos), new Point(bounds.Right, yPos));
                hasLines = true;
            }
        }

        if (!hasLines)
            return null;

        geometry.Freeze();
        return geometry;
    }

    /// <summary>
    /// Builds pixel-snapping guidelines for the grid lines inside the provided bounds.
    /// </summary>
    private static GuidelineSet CreateGridLineGuidelines(Rect bounds, int columns, int rows, double penThickness)
    {
        var guidelines = new GuidelineSet();
        double halfThickness = penThickness / 2.0;

        guidelines.GuidelinesX.Add(bounds.X);

        for (int x = 1; x < columns; x++)
        {
            double xPos = bounds.X + ((bounds.Width * x) / columns);
            guidelines.GuidelinesX.Add(xPos - halfThickness);
            guidelines.GuidelinesX.Add(xPos + halfThickness);
        }

        guidelines.GuidelinesX.Add(bounds.Right);
        guidelines.GuidelinesY.Add(bounds.Y);

        for (int y = 1; y < rows; y++)
        {
            double yPos = bounds.Y + ((bounds.Height * y) / rows);
            guidelines.GuidelinesY.Add(yPos - halfThickness);
            guidelines.GuidelinesY.Add(yPos + halfThickness);
        }

        guidelines.GuidelinesY.Add(bounds.Bottom);
        guidelines.Freeze();
        return guidelines;
    }
}
