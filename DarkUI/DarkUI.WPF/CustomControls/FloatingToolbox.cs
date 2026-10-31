#nullable enable

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DarkUI.WPF.CustomControls;

/// <summary>Displays draggable WPF tools over a viewport, including native rendering surfaces.</summary>
public sealed class FloatingToolbox : Popup, IDisposable
{
    private readonly FrameworkElement _viewport;
    private readonly Border _border;
    private Window? _owner;
    private Point _position;
    private Point _lastScreenPosition;
    private bool _isShown;
    private bool _disposed;
    private Size _previousViewportSize;

    /// <summary>Gets or sets the toolbox position in viewport coordinates.</summary>
    public Point Position
    {
        get => _position;
        set
        {
            _position = value;
            UpdatePosition();
        }
    }

    /// <summary>Gets or sets whether the toolbox should appear while its viewport is visible.</summary>
    public bool IsShown
    {
        get => _isShown;
        set
        {
            _isShown = value;
            UpdateVisibility();
        }
    }

    /// <summary>Creates a toolbox whose drag grip appears above or beside its content.</summary>
    public FloatingToolbox(FrameworkElement viewport, UIElement content, bool horizontalGrip = true)
    {
        _viewport = viewport;
        PlacementTarget = viewport;
        Placement = PlacementMode.Relative;
        AllowsTransparency = true;
        StaysOpen = true;
        Focusable = false;

        var grip = new Thumb { Cursor = Cursors.SizeAll, Focusable = false, Margin = new Thickness(3) };
        grip.SetResourceReference(Control.BackgroundProperty, "Brush_Border");
        grip.DragDelta += OnDragDelta;
        grip.DragCompleted += (_, _) => SnapToEdges();
        DockPanel.SetDock(grip, horizontalGrip ? Dock.Top : Dock.Left);

        if (horizontalGrip)
            grip.Height = 9;
        else
            grip.Width = 9;

        var panel = new DockPanel();
        panel.Children.Add(grip);
        panel.Children.Add(new ScrollViewer
        {
            Content = content,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        });
        _border = new Border { BorderThickness = new Thickness(1), Child = panel };
        _border.SetResourceReference(Border.BackgroundProperty, "Brush_Background");
        _border.SetResourceReference(Border.BorderBrushProperty, "Brush_Border");
        _border.SizeChanged += OnContentSizeChanged;
        Child = _border;

        viewport.Loaded += OnViewportLoaded;
        viewport.Unloaded += OnViewportUnloaded;
        viewport.IsVisibleChanged += OnViewportVisibilityChanged;
        viewport.SizeChanged += OnViewportSizeChanged;
        viewport.LayoutUpdated += OnViewportLayoutUpdated;
        Opened += (_, _) => UpdatePosition();
    }

    private void OnViewportLoaded(object sender, RoutedEventArgs e)
    {
        _owner = Window.GetWindow(_viewport);

        if (_owner is not null)
        {
            _owner.Activated += OnOwnerActivationChanged;
            _owner.Deactivated += OnOwnerActivationChanged;
            _owner.LocationChanged += OnOwnerLocationChanged;
            _owner.StateChanged += OnOwnerLocationChanged;
        }

        _previousViewportSize = _viewport.RenderSize;
        UpdateVisibility();
    }

    private void OnViewportUnloaded(object sender, RoutedEventArgs e)
    {
        IsOpen = false;
        DetachOwner();
    }

    private void DetachOwner()
    {
        if (_owner is null)
            return;

        _owner.Activated -= OnOwnerActivationChanged;
        _owner.Deactivated -= OnOwnerActivationChanged;
        _owner.LocationChanged -= OnOwnerLocationChanged;
        _owner.StateChanged -= OnOwnerLocationChanged;
        _owner = null;
    }

    private void OnOwnerActivationChanged(object? sender, EventArgs e) => UpdateVisibility();

    private void OnOwnerLocationChanged(object? sender, EventArgs e) => UpdatePosition();

    private void OnViewportVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e) => UpdateVisibility();

    private void UpdateVisibility()
    {
        IsOpen = !_disposed && _isShown && _viewport.IsVisible && _owner is { IsActive: true, WindowState: not WindowState.Minimized };

        if (IsOpen)
            UpdatePosition();
    }

    private void OnViewportLayoutUpdated(object? sender, EventArgs e)
    {
        if (!IsOpen || PresentationSource.FromVisual(_viewport) is null)
            return;

        var screenPosition = _viewport.PointToScreen(new Point());

        if (screenPosition == _lastScreenPosition)
            return;

        _lastScreenPosition = screenPosition;
        UpdatePosition();
    }

    private void OnViewportSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_previousViewportSize.Width > 0 && _position.X + _border.ActualWidth / 2 > _previousViewportSize.Width / 2)
            _position.X += e.NewSize.Width - _previousViewportSize.Width;

        if (_previousViewportSize.Height > 0 && _position.Y + _border.ActualHeight / 2 > _previousViewportSize.Height / 2)
            _position.Y += e.NewSize.Height - _previousViewportSize.Height;

        _previousViewportSize = e.NewSize;
        UpdatePosition();
    }

    private void OnContentSizeChanged(object sender, SizeChangedEventArgs e) => UpdatePosition();

    private void OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        _position.Offset(e.HorizontalChange, e.VerticalChange);
        UpdatePosition();
    }

    private void SnapToEdges()
    {
        const double snappingDistance = 10;
        double right = Math.Max(0, _viewport.ActualWidth - _border.ActualWidth);
        double bottom = Math.Max(0, _viewport.ActualHeight - _border.ActualHeight);
        _position.X = _position.X < snappingDistance ? 0 : right - _position.X < snappingDistance ? right : _position.X;
        _position.Y = _position.Y < snappingDistance ? 0 : bottom - _position.Y < snappingDistance ? bottom : _position.Y;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (_viewport.ActualWidth <= 0 || _viewport.ActualHeight <= 0)
            return;

        _border.MaxWidth = _viewport.ActualWidth;
        _border.MaxHeight = _viewport.ActualHeight;
        _position.X = Math.Clamp(_position.X, 0, Math.Max(0, _viewport.ActualWidth - _border.ActualWidth));
        _position.Y = Math.Clamp(_position.Y, 0, Math.Max(0, _viewport.ActualHeight - _border.ActualHeight));
        HorizontalOffset = _position.X + 0.01;
        HorizontalOffset = _position.X;
        VerticalOffset = _position.Y;
    }

    /// <summary>Closes the toolbox and releases viewport and owner subscriptions.</summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        IsOpen = false;
        DetachOwner();
        _viewport.Loaded -= OnViewportLoaded;
        _viewport.Unloaded -= OnViewportUnloaded;
        _viewport.IsVisibleChanged -= OnViewportVisibilityChanged;
        _viewport.SizeChanged -= OnViewportSizeChanged;
        _viewport.LayoutUpdated -= OnViewportLayoutUpdated;
        _border.SizeChanged -= OnContentSizeChanged;
        Child = null;
        PlacementTarget = null;
    }
}
