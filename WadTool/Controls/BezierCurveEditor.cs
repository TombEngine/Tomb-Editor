#nullable enable

using System;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Types;

namespace WadTool.Controls;

/// <summary>
/// WPF rewrite of the previous <c>BezierCurveEditor</c>: renders a blend curve with two
/// draggable handles. Double-clicking an empty spot resets the curve to linear. Like the
/// previous control, <see cref="Value"/> shares the caller's <see cref="BezierCurve2"/>
/// instance and mutates it in place while dragging.
/// </summary>
public class BezierCurveEditor : FrameworkElement
{
    private const double HandleRadius = 6.0;
    private const double HandleOutlineRadius = 9.0;

    public event EventHandler? ValueChanged;

    private BezierCurve2 _bezierCurve = BezierCurve2.Linear.Clone();
    private int _selectedPoint = -1;

    public BezierCurve2 Value
    {
        get => _bezierCurve;
        set
        {
            _bezierCurve = value ?? BezierCurve2.Linear.Clone();
            UpdateUI();
        }
    }

    public BezierCurveEditor()
    {
        IsEnabledChanged += (_, _) => InvalidateVisual();
    }

    /// <summary>Redraws after the curve was mutated externally (e.g. a preset was applied).</summary>
    public void UpdateUI() => InvalidateVisual();

    private Vector2 TransformToBezier(Point point)
        => new((float)(point.X / ActualWidth), (float)(1.0 - point.Y / ActualHeight));

    private Point TransformToScreen(Vector2 point)
        => new(point.X * ActualWidth, (1.0 - point.Y) * ActualHeight);

    /// <summary>
    /// Screen positions of the four control points. For an exactly linear curve the handles sit
    /// on the corners, so they are displayed at 1/3 and 2/3 of the diagonal to stay grabbable
    /// (same behaviour as the control).
    /// </summary>
    private Point[] GetScreenPoints()
    {
        if (_bezierCurve.StartHandle == _bezierCurve.Start && _bezierCurve.EndHandle == _bezierCurve.End)
        {
            double w = ActualWidth, h = ActualHeight;
            return new[]
            {
                new Point(0, h),
                new Point(w / 3.0, h * 2.0 / 3.0),
                new Point(2.0 * w / 3.0, h / 3.0),
                new Point(w, 0)
            };
        }

        return new[]
        {
            TransformToScreen(_bezierCurve.Start),
            TransformToScreen(_bezierCurve.StartHandle),
            TransformToScreen(_bezierCurve.EndHandle),
            TransformToScreen(_bezierCurve.End)
        };
    }

    private int HitTestHandle(Point position)
    {
        Point[] points = GetScreenPoints();

        for (int i = 1; i < 3; i++)
        {
            if (Math.Abs(position.X - points[i].X) < HandleOutlineRadius * 2 &&
                Math.Abs(position.Y - points[i].Y) < HandleOutlineRadius * 2)
                return i;
        }

        return -1;
    }

    private Color GetColor(string resourceKey, Color fallback)
        => TryFindResource(resourceKey) is Color color ? color : fallback;

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        double w = ActualWidth, h = ActualHeight;
        if (w <= 0 || h <= 0)
            return;

        Color background = GetColor("GreyBackgroundColor", Color.FromRgb(0x3C, 0x3F, 0x41));
        Color lightBorder = GetColor("LightBorderColor", Color.FromRgb(0x51, 0x51, 0x51));
        Color disabled = GetColor("DarkGreySelectionColor", Color.FromRgb(0x52, 0x52, 0x52));
        Color curveColor = IsEnabled ? GetColor("LightestBackgroundColor", Color.FromRgb(0xB2, 0xB2, 0xB2)) : disabled;
        Color handleLine = IsEnabled ? GetColor("GreyHighlightColor", Color.FromRgb(0x7A, 0x80, 0x84)) : disabled;

        var backgroundBrush = new SolidColorBrush(background);
        backgroundBrush.Freeze();
        drawingContext.DrawRectangle(backgroundBrush, null, new Rect(0, 0, w, h));

        Point[] points = GetScreenPoints();

        var curveGeometry = new StreamGeometry();
        using (StreamGeometryContext context = curveGeometry.Open())
        {
            context.BeginFigure(points[0], isFilled: false, isClosed: false);
            context.BezierTo(points[1], points[2], points[3], isStroked: true, isSmoothJoin: true);
        }
        curveGeometry.Freeze();

        var curvePen = new Pen(new SolidColorBrush(curveColor), 2.0);
        curvePen.Freeze();
        drawingContext.DrawGeometry(null, curvePen, curveGeometry);

        var handleLinePen = new Pen(new SolidColorBrush(handleLine), 1.0);
        handleLinePen.Freeze();
        drawingContext.DrawLine(handleLinePen, points[0], points[1]);
        drawingContext.DrawLine(handleLinePen, points[3], points[2]);

        var outlineBrush = new SolidColorBrush(background);
        outlineBrush.Freeze();

        for (int i = 1; i < 3; i++)
        {
            drawingContext.DrawEllipse(outlineBrush, null, points[i], HandleOutlineRadius, HandleOutlineRadius);

            Color handleColor = !IsEnabled ? disabled : (i == _selectedPoint ? curveColor : handleLine);
            var handleBrush = new SolidColorBrush(handleColor);
            handleBrush.Freeze();
            drawingContext.DrawEllipse(handleBrush, null, points[i], HandleRadius, HandleRadius);
        }

        var innerBorderPen = new Pen(outlineBrush, 3.0);
        innerBorderPen.Freeze();
        drawingContext.DrawRectangle(null, innerBorderPen, new Rect(1.5, 1.5, w - 3.0, h - 3.0));

        var borderPen = new Pen(new SolidColorBrush(lightBorder), 1.0);
        borderPen.Freeze();
        drawingContext.DrawRectangle(null, borderPen, new Rect(0.5, 0.5, w - 1.0, h - 1.0));
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        Point position = e.GetPosition(this);
        int hit = HitTestHandle(position);

        if (e.ClickCount == 2)
        {
            if (hit == -1)
            {
                Value.Set(BezierCurve2.Linear);
                ValueChanged?.Invoke(this, EventArgs.Empty);
                InvalidateVisual();
            }

            return;
        }

        _selectedPoint = hit;

        if (hit != -1)
            CaptureMouse();

        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_selectedPoint == -1 || e.LeftButton != MouseButtonState.Pressed)
            return;

        Point position = e.GetPosition(this);
        position.X = Math.Max(HandleRadius / 2.0, Math.Min(position.X, ActualWidth - HandleRadius / 2.0 - 1.0));
        position.Y = Math.Max(HandleRadius / 2.0, Math.Min(position.Y, ActualHeight - HandleRadius / 2.0 - 1.0));

        if (_selectedPoint == 1)
            _bezierCurve.StartHandle = TransformToBezier(position);
        else
            _bezierCurve.EndHandle = TransformToBezier(position);

        ValueChanged?.Invoke(this, EventArgs.Empty);
        InvalidateVisual();
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        _selectedPoint = -1;
        ReleaseMouseCapture();
        InvalidateVisual();
    }
}
