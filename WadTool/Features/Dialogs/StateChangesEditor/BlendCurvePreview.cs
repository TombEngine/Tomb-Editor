#nullable enable

using System;
using System.Windows;
using System.Windows.Media;
using TombLib.Types;

namespace WadTool.Features.Dialogs.StateChangesEditor;

/// <summary>
/// Lightweight blend curve preview (ported from the legacy previous
/// <c>BezierCurveEditor.DrawPreview</c>): renders a curve inside the blend curve grid cells.
/// </summary>
public sealed class BlendCurvePreview : FrameworkElement
{
    private const double EdgePadding = 2.0;

    public static readonly DependencyProperty CurveProperty = DependencyProperty.Register(
        nameof(Curve), typeof(BezierCurve2), typeof(BlendCurvePreview),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
        nameof(Stroke), typeof(Brush), typeof(BlendCurvePreview),
        new FrameworkPropertyMetadata(Brushes.Gainsboro, FrameworkPropertyMetadataOptions.AffectsRender));

    public BezierCurve2? Curve
    {
        get => (BezierCurve2?)GetValue(CurveProperty);
        set => SetValue(CurveProperty, value);
    }

    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        BezierCurve2? curve = Curve;
        double w = ActualWidth - EdgePadding * 2.0;
        double h = ActualHeight - EdgePadding * 2.0;

        if (curve is null || w <= 0.0 || h <= 0.0)
            return;

        int steps = Math.Max((int)(w / 2.0), 8);
        var points = new Point[steps + 1];
        for (int i = 0; i <= steps; i++)
        {
            float alpha = (float)i / steps;
            var p = curve.GetPoint(alpha);
            points[i] = new Point(EdgePadding + p.X * w, EdgePadding + (1.0f - p.Y) * h);
        }

        var geometry = new StreamGeometry();
        using (StreamGeometryContext context = geometry.Open())
        {
            context.BeginFigure(points[0], isFilled: false, isClosed: false);
            context.PolyLineTo(points[1..], isStroked: true, isSmoothJoin: true);
        }
        geometry.Freeze();

        var pen = new Pen(Stroke, 1.0)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round
        };
        if (pen.CanFreeze)
            pen.Freeze();

        drawingContext.DrawGeometry(null, pen, geometry);
    }
}
