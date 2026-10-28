#nullable enable

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using DrawingPoint = System.Drawing.Point;
using WinForms = System.Windows.Forms;
using WpfDragDrop = System.Windows.DragDrop;
using WpfDragDropEffects = System.Windows.DragDropEffects;
using WpfDragEventArgs = System.Windows.DragEventArgs;
using WpfPoint = System.Windows.Point;

namespace TombLib.Controls.Wpf;

public partial class RenderingPanel : WinForms.IWin32Window
{
    /// <summary>Gets whether the native surface owns keyboard focus.</summary>
    public bool Focused => GetFocus() == _windowHandle;

    /// <summary>Gets or sets native mouse capture for a viewport gesture.</summary>
    public bool Capture
    {
        get => GetCapture() == _windowHandle;
        set
        {
            if (value)
                SetCapture(_windowHandle);
            else
                ReleaseCapture();
        }
    }

    /// <summary>Converts a device pixel screen point to viewport pixels.</summary>
    public DrawingPoint PointToClient(DrawingPoint point)
    {
        var native = new NativePoint { X = point.X, Y = point.Y };
        ScreenToClient(_windowHandle, ref native);
        return new DrawingPoint(native.X, native.Y);
    }

    /// <summary>Converts viewport pixels to a device pixel screen point.</summary>
    public DrawingPoint PointToScreen(DrawingPoint point)
    {
        var native = new NativePoint { X = point.X, Y = point.Y };
        ClientToScreen(_windowHandle, ref native);
        return new DrawingPoint(native.X, native.Y);
    }

    /// <summary>Converts viewport bounds to device pixel screen bounds.</summary>
    public Rectangle RectangleToScreen(Rectangle rectangle) => new(PointToScreen(rectangle.Location), rectangle.Size);

    /// <summary>Draws a pending frame synchronously.</summary>
    public void Update() => RenderFrame();

    /// <summary>Queues a viewport redraw.</summary>
    public void Invalidate(bool invalidateChildren) => Invalidate();

    /// <summary>Runs a delegate on the WPF dispatcher.</summary>
    public object Invoke(Delegate method, params object[] arguments) => Dispatcher.Invoke(method, arguments);

    /// <summary>Queues a delegate on the WPF dispatcher.</summary>
    public DispatcherOperation BeginInvoke(Delegate method, params object[] arguments) => Dispatcher.BeginInvoke(method, arguments);

    /// <summary>Starts an OLE drag operation using WPF's native data transfer.</summary>
    public WinForms.DragDropEffects DoDragDrop(object data, WinForms.DragDropEffects allowedEffects)
        => (WinForms.DragDropEffects)WpfDragDrop.DoDragDrop(this, data, (WpfDragDropEffects)allowedEffects);

    protected virtual void OnPreviewKeyDown(WinForms.PreviewKeyDownEventArgs e) { }
    protected virtual void OnLostFocus(EventArgs e) { }
    protected virtual void OnDragEnter(WinForms.DragEventArgs e) { }
    protected virtual void OnDragOver(WinForms.DragEventArgs e) { }
    protected virtual void OnDragDrop(WinForms.DragEventArgs e) { }
    protected virtual void OnDragLeave(EventArgs e) { }

    protected override void OnDragEnter(WpfDragEventArgs e)
    {
        base.OnDragEnter(e);
        var args = CreateDragArgs(e);
        OnDragEnter(args);
        e.Effects = (WpfDragDropEffects)args.Effect;
        e.Handled = true;
    }

    protected override void OnDragOver(WpfDragEventArgs e)
    {
        base.OnDragOver(e);
        var args = CreateDragArgs(e);
        OnDragOver(args);
        e.Effects = (WpfDragDropEffects)args.Effect;
        e.Handled = true;
    }

    protected override void OnDrop(WpfDragEventArgs e)
    {
        base.OnDrop(e);
        var args = CreateDragArgs(e);
        OnDragDrop(args);
        e.Effects = (WpfDragDropEffects)args.Effect;
        e.Handled = true;
    }

    protected override void OnDragLeave(WpfDragEventArgs e)
    {
        base.OnDragLeave(e);
        OnDragLeave(EventArgs.Empty);
    }

    private WinForms.DragEventArgs CreateDragArgs(WpfDragEventArgs e)
    {
        var data = new WinForms.DataObject();
        foreach (var format in e.Data.GetFormats())
        {
            var value = e.Data.GetData(format);
            if (value is not null)
                data.SetData(format, value);
        }

        WpfPoint screen = base.PointToScreen(e.GetPosition(this));
        return new WinForms.DragEventArgs(data, (int)e.KeyStates, (int)screen.X, (int)screen.Y,
            (WinForms.DragDropEffects)e.AllowedEffects, (WinForms.DragDropEffects)e.Effects);
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetFocus();

    [DllImport("user32.dll")]
    private static extern IntPtr GetCapture();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ClientToScreen(IntPtr window, ref NativePoint point);
}
