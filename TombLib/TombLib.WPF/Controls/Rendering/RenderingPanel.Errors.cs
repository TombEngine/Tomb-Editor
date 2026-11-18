#nullable enable

using NLog;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using TombLib.WPF;

namespace TombLib.WPF.Controls.Rendering;

public partial class RenderingPanel
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>Gets the failure that stopped rendering, or null while the viewport is healthy.</summary>
    public Exception? RenderingError { get; private set; }

    /// <summary>Raised once when rendering stops so the owning view can offer recovery actions.</summary>
    public event EventHandler? RenderingFailed;

    /// <summary>Stops drawing and displays WPF recovery guidance after a rendering failure.</summary>
    protected void ReportRenderingError(Exception exception)
    {
        if (RenderingError is not null)
            return;

        RenderingError = exception;
        Logger.Error(exception, "The native rendering surface stopped drawing.");
        if (_windowHandle != IntPtr.Zero)
            ShowWindow(_windowHandle, 0);

        InvalidateVisual();
        RenderingFailed?.Invoke(this, EventArgs.Empty);
    }

    private void ClearRenderingError()
    {
        RenderingError = null;
        if (_windowHandle != IntPtr.Zero && IsVisible)
            ShowWindow(_windowHandle, 5);

        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (RenderingError is null)
            return;

        var background = TryFindResource("Brush_Background") as Brush ?? Brushes.Black;
        var foreground = TryFindResource("Brush_Text") as Brush ?? Brushes.LightGray;
        drawingContext.DrawRectangle(background, null, new Rect(RenderSize));
        var message = new FormattedText(Localizer.Instance["TombLib.Rendering.FatalError"], CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight, new Typeface(SystemFonts.MessageFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            SystemFonts.MessageFontSize, foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip)
        {
            MaxTextWidth = Math.Max(1.0, ActualWidth - 24.0),
            TextAlignment = TextAlignment.Center,
            Trimming = TextTrimming.CharacterEllipsis
        };
        drawingContext.DrawText(message, new Point(12.0, Math.Max(12.0, (ActualHeight - message.Height) / 2.0)));
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr window, int command);
}
