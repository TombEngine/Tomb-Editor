#nullable enable

using System;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TombEditor.Controls;

namespace TombEditor.Features.Dialogs.TextureRemap;

/// <summary>
/// Texture map specialization used by the texture-remap dialog. Replaces the standard quad
/// selection with a Start/End rectangle and routes input through the shared
/// <see cref="TextureRemapWindowViewModel"/>.
/// </summary>
public sealed class TextureRemapMapView : WpfTextureMapView
{
    private static readonly Brush FillBrush = CreateFrozenBrush(Color.FromArgb(30, 220, 210, 20));
    private static readonly Pen OutlinePen = CreateFrozenPen(Color.FromArgb(255, 220, 210, 20), 3.0);

    public static readonly DependencyProperty IsDestinationProperty = DependencyProperty.Register(
        nameof(IsDestination), typeof(bool), typeof(TextureRemapMapView),
        new PropertyMetadata(false));

    public bool IsDestination
    {
        get => (bool)GetValue(IsDestinationProperty);
        set => SetValue(IsDestinationProperty, value);
    }

    private TextureRemapWindowViewModel? Vm => DataContext as TextureRemapWindowViewModel;

    protected override void OnPaintSelection(DrawingContext drawingContext)
    {
        if (Vm is null)
            return;

        Vector2 start, end;

        if (IsDestination)
        {
            Vector2 size = Vector2.Abs(Vm.SourceEnd - Vm.SourceStart) * Vm.Scaling;
            start = Vm.DestinationStart;
            end = Vm.DestinationStart + size;
        }
        else
        {
            start = Vm.SourceStart;
            end = Vm.SourceEnd;
        }

        Point pStart = ToVisualCoord(start);
        Point pEnd = ToVisualCoord(end);

        var rect = new Rect(
            Math.Min(pStart.X, pEnd.X),
            Math.Min(pStart.Y, pEnd.Y),
            Math.Abs(pEnd.X - pStart.X),
            Math.Abs(pEnd.Y - pStart.Y));

        drawingContext.DrawRectangle(FillBrush, OutlinePen, rect);
    }

    private Vector2 Quantize(Vector2 texCoord, bool endSnap)
    {
        SelectionPrecisionType precision = GetSelectionPrecision(true);

        if (precision.Precision == 0.0f)
            return texCoord;

        texCoord /= precision.Precision;
        texCoord = endSnap
            ? new Vector2((float)Math.Ceiling(texCoord.X), (float)Math.Ceiling(texCoord.Y))
            : new Vector2((float)Math.Floor(texCoord.X), (float)Math.Floor(texCoord.Y));
        return texCoord * precision.Precision;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        if (Vm is null || !(base.VisibleTexture is { IsAvailable: true }))
        {
            base.OnMouseDown(e);
            return;
        }

        if (!IsKeyboardFocused)
            Focus();

        CaptureMouse();

        Point pos = e.GetPosition(this);

        if (e.ChangedButton == MouseButton.Left)
        {
            if (IsDestination)
            {
                ApplyDestinationStart(pos);
            }
            else
            {
                Vector2 newStart = Quantize(FromVisualCoord(pos, false), false);
                Vector2 size = base.VisibleTexture!.Image.Size;

                if (newStart.X >= 0 && newStart.Y >= 0 && newStart.X < size.X && newStart.Y < size.Y)
                {
                    Vm.SourceStart = newStart;
                    Vm.NotifyRemapRectanglesChanged();
                }

                ApplySourceEnd(pos);
            }
        }
        else
        {
            // Right-button pan handled by the base class.
            base.OnMouseDown(e);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (Vm is null || !(base.VisibleTexture is { IsAvailable: true }))
        {
            base.OnMouseMove(e);
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            Point pos = e.GetPosition(this);

            if (IsDestination)
                ApplyDestinationStart(pos);
            else
                ApplySourceEnd(pos);
        }
        else if (e.RightButton == MouseButtonState.Pressed)
        {
            base.OnMouseMove(e);
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        if (Vm is null)
        {
            base.OnMouseUp(e);
            return;
        }

        if (e.ChangedButton == MouseButton.Left)
        {
            Point pos = e.GetPosition(this);

            if (IsDestination)
                ApplyDestinationStart(pos);
            else
                ApplySourceEnd(pos);

            ReleaseMouseCapture();
        }
        else
        {
            base.OnMouseUp(e);
        }
    }

    private void ApplySourceEnd(Point pos)
    {
        if (Vm is null || base.VisibleTexture is null)
            return;

        Vector2 newEnd = Quantize(FromVisualCoord(pos, true), true);
        Vm.SourceEnd = newEnd;
        Vm.UpdateScaling();
        Vm.NotifyRemapRectanglesChanged();
    }

    private void ApplyDestinationStart(Point pos)
    {
        if (Vm is null || base.VisibleTexture is null)
            return;

        Vector2 newStart = Quantize(FromVisualCoord(pos, false), false);
        Vector2 size = (Vm.SourceEnd - Vm.SourceStart) * Vm.Scaling;
        Vector2 imageSize = base.VisibleTexture.Image.Size;

        Vector2 finalStart = Vm.DestinationStart;

        if (newStart.X >= 0 && newStart.X + size.X <= imageSize.X)
            finalStart.X = newStart.X;

        if (newStart.Y >= 0 && newStart.Y + size.Y <= imageSize.Y)
            finalStart.Y = newStart.Y;

        Vm.DestinationStart = finalStart;
        Vm.NotifyRemapRectanglesChanged();
    }

    private static Brush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static Pen CreateFrozenPen(Color color, double thickness)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        var pen = new Pen(brush, thickness);
        pen.Freeze();
        return pen;
    }
}
