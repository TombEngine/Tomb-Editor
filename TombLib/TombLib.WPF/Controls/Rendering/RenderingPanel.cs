#nullable enable

using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using TombLib.Input;
using TombLib.Rendering;
using TombLib.Services;
using TombLib.WPF.Input;

namespace TombLib.WPF.Controls.Rendering;

/// <summary>
/// Owns a native Direct3D child window within WPF, including swap-chain lifetime and input.
/// </summary>
public partial class RenderingPanel : HwndHost
{
    private const int ChildWindowStyle = 0x56000100;
    private IntPtr _windowHandle;
    private RenderingDevice? _device;
    private RenderingSwapChain? _swapChain;
    private bool _antialias;
    private bool _renderPending;
    private bool _disposing;

    /// <summary>Raised when the render surface draws a frame.</summary>
    public event EventHandler? Draw;

    /// <summary>Gets the device assigned to this surface.</summary>
    public RenderingDevice Device => _device ?? throw new InvalidOperationException("Rendering has not been initialized.");

    /// <summary>Gets the swap chain after WPF has created the native surface.</summary>
    public RenderingSwapChain SwapChain => _swapChain ?? throw new InvalidOperationException("The rendering surface has not been created.");

    /// <summary>Gets or sets whether pending frames may be rendered.</summary>
    public bool AllowRendering { get; set; } = true;

    /// <summary>Gets the client area in device pixels.</summary>
    public Size ClientSize
    {
        get
        {
            var dpi = VisualTreeHelper.GetDpi(this);
            return new Size(Math.Max(1, (int)Math.Round(ActualWidth * dpi.DpiScaleX)), Math.Max(1, (int)Math.Round(ActualHeight * dpi.DpiScaleY)));
        }
    }

    /// <summary>Gets the client bounds in device pixels.</summary>
    public Rectangle ClientRectangle => new(Point.Empty, ClientSize);

    /// <summary>Gets the surface width in device pixels for camera and picking calculations.</summary>
    public new int Width => ClientSize.Width;

    /// <summary>Gets the surface height in device pixels for camera and picking calculations.</summary>
    public new int Height => ClientSize.Height;

    /// <summary>Gets whether the owning WPF window is active.</summary>
    public bool IsOwnerActive => Window.GetWindow(this)?.IsActive == true;

    protected virtual Vector4 ClearColor => new(0.392f, 0.584f, 0.929f, 1.0f);

    public RenderingPanel()
    {
        Focusable = true;
        Loaded += (_, _) => Invalidate();
    }

    /// <summary>Assigns the rendering device and creates its swap chain when the HWND is ready.</summary>
    public virtual void InitializeRendering(RenderingDevice device, bool antialias = false,
        ObjectRenderingQuality objectQuality = ObjectRenderingQuality.Undefined)
    {
        _swapChain?.Dispose();
        _swapChain = null;
        _device = device;
        ClearRenderingError();
        _antialias = antialias;
        CreateSwapChain();
        Invalidate();
    }

    private void CreateSwapChain()
    {
        if (_windowHandle == IntPtr.Zero || _device is null || _swapChain is not null)
            return;

        _swapChain = _device.CreateSwapChain(new RenderingSwapChain.Description
        {
            Size = new VectorInt2(Width, Height),
            WindowHandle = _windowHandle,
            Antialias = _antialias
        });
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        _windowHandle = CreateWindowEx(0, "static", string.Empty, ChildWindowStyle,
            0, 0, Width, Height, hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (_windowHandle == IntPtr.Zero)
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

        try
        {
            RegisterDropTarget();
            CreateSwapChain();
            Invalidate();
            return new HandleRef(this, _windowHandle);
        }
        catch
        {
            RevokeDropTarget();
            DestroyWindow(_windowHandle);
            _windowHandle = IntPtr.Zero;
            throw;
        }
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        RevokeDropTarget();
        _swapChain?.Dispose();
        _swapChain = null;
        DestroyWindow(hwnd.Handle);
        _windowHandle = IntPtr.Zero;
    }

    /// <summary>Queues one frame on the owning dispatcher, combining repeated invalidations.</summary>
    public void Invalidate()
    {
        if (_disposing || _renderPending)
            return;

        _renderPending = true;
        Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => {
            _renderPending = false;
            RenderFrame();
        }));
    }

    private void RenderFrame()
    {
        if (_disposing || !AllowRendering || !IsVisible || _device is null || _swapChain is null || RenderingError is not null)
            return;

        if (_swapChain.RenderException is { } previousFailure)
        {
            ReportRenderingError(previousFailure);
            return;
        }

        try
        {
            _swapChain.Clear(ClearColor);
            OnDraw();
            _swapChain.Present();
            if (_swapChain.RenderException is { } failure)
                ReportRenderingError(failure);
        }
        catch (Exception exception)
        {
            ReportRenderingError(exception);
        }
    }

    protected virtual void OnDraw() => Draw?.Invoke(this, EventArgs.Empty);

    protected virtual void OnResize(EventArgs e)
    {
        if (_swapChain is null || _swapChain.RenderException is not null)
            return;

        try
        {
            _swapChain.Resize(new VectorInt2(Width, Height));
            Invalidate();
        }
        catch (Exception exception)
        {
            ReportRenderingError(exception);
        }
    }

    protected virtual void OnSizeChanged(EventArgs e) { }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        OnResize(EventArgs.Empty);
        OnSizeChanged(EventArgs.Empty);
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        OnResize(EventArgs.Empty);
    }

    /// <summary>Gets the mouse movement normalized by the viewport height.</summary>
    public Vector2 Delta(Point currentPosition, Point previousMousePosition)
        => new((currentPosition.X - previousMousePosition.X) / (float)Height, (currentPosition.Y - previousMousePosition.Y) / (float)Height);

    /// <summary>Wraps the cursor at the viewport edges while preserving camera movement.</summary>
    public Vector2 WarpMouseCursor(Point currentPosition, Point previousMousePosition)
    {
        var delta = Delta(currentPosition, previousMousePosition);
        var position = PointerState.ScreenPosition;
        if (currentPosition.X <= 0)
            position.X += Width - 2;
        else if (currentPosition.X >= Width - 1)
            position.X -= Width - 2;

        if (currentPosition.Y <= 0)
            position.Y += Height - 2;
        else if (currentPosition.Y >= Height - 1)
            position.Y -= Height - 2;

        PointerState.ScreenPosition = position;
        if (Math.Abs(currentPosition.X - previousMousePosition.X) >= Width / 2.0f)
            delta.X = 0.0f;
        if (Math.Abs(currentPosition.Y - previousMousePosition.Y) >= Height / 2.0f)
            delta.Y = 0.0f;

        return delta;
    }

    protected override void Dispose(bool disposing)
    {
        _disposing = true;
        if (disposing)
        {
            _swapChain?.Dispose();
            _swapChain = null;
        }

        base.Dispose(disposing);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(int extendedStyle, string className, string windowName, int style,
        int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr parameter);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(IntPtr window);
}
