#nullable enable

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using TombLib.Services;
using TombLib.Utils;

namespace TombLib.WPF;

/// <summary>Displays a non-activating toast anchored to a WPF control.</summary>
public class PopUpInfo : IDisposable
{
    private const float ShiftCoefficient = 0.25f;   // Intro animation push coefficient.
    private const float FinalOpacity = 0.85f; // Non-animated opacity.
    private const float AverageReadSpeed = 2.5f;  // Average reading speed.

    private static readonly Color WarningColor = Color.FromRgb(255, 192, 128);
    private static readonly Color ErrorColor = Color.FromRgb(255, 128, 128);
    private static readonly Color InfoColor = Color.FromRgb(192, 192, 255);

    private readonly DispatcherTimer _animTimer = new() { Interval = TimeSpan.FromMilliseconds(10) };
    private float _animProgress = 0.0f;
    private float _animTimeout = 1000.0f;

    private PopupAlignment _alignment;
    private int _padding;
    private FrameworkElement? _wpfParent;
    private System.Drawing.Point _parentPosition;

    private ToastWindow? _window;

    public PopUpInfo()
    {
        _animTimer.Tick += UpdateTimer;
    }

    /// <summary>Shows a toast anchored to a native WPF view or rendering surface.</summary>
    public static void Show(PopUpInfo popup, IWindowOwner? owner, FrameworkElement parent, string message, PopupType type)
    {
        if (owner is not null && GetRootHandle(owner.Handle) != GetActiveWindow())
            return;

        string title = type switch
        {
            PopupType.Info => "Information",
            PopupType.Warning => "Warning",
            PopupType.Error => "Error",
            _ => string.Empty
        };
        popup.Show(parent, PopupAlignment.BottomRight, message, title, type);
    }

    /// <summary>Shows a toast relative to a WPF anchor, in device independent coordinates.</summary>
    public void Show(FrameworkElement parent, PopupAlignment alignment, string message, string title = "",
        PopupType type = PopupType.Info, int timeout = 0, int padding = 10)
    {
        _wpfParent = parent;
        var owner = Window.GetWindow(parent);
        var handle = owner is null ? IntPtr.Zero : new WindowInteropHelper(owner).Handle;
        ShowCore(handle, parent.ActualWidth, parent.ActualHeight, alignment, message, title, type, timeout, padding);
    }

    private void ShowCore(IntPtr owner, double width, double height, PopupAlignment alignment, string message, string title,
        PopupType type, int timeout, int padding)
    {
        // No message means kill current pop-up
        if (message == "")
        {
            Hide();
            return;
        }

        _alignment = alignment;
        _padding = padding;

        if (_window is null || !_window.IsLoaded)
        {
            _window?.Close();
            var window = new ToastWindow();
            window.Closed += (_, _) => {
                if (ReferenceEquals(_window, window))
                {
                    _animTimer.Stop();
                    _window = null;
                }
            };
            _window = window;
        }

        _window.Setup(title, message, type,
            maxWidth: Math.Max(80.0, width * 0.7),
            maxHeight: Math.Max(40.0, height / 4.0));

        // Setup message timeout
        if (timeout > 0)
            _animTimeout = timeout * 10.0f;
        else
        {
            // Default timeout is based on average reading speed.
            int wordCount = 0, index = 0;
            while (index < message.Length)
            {
                while (index < message.Length && !char.IsWhiteSpace(message[index]))
                    index++;
                wordCount++;
                while (index < message.Length && char.IsWhiteSpace(message[index]))
                    index++;
            }
            _animTimeout = wordCount * AverageReadSpeed;
        }

        // Start intro animation
        _animProgress = 0.0f;
        _animTimer.Start();

        // Clear opacity to prevent flicker
        _window.Opacity = 0.0;

        if (!_window.IsVisible)
        {
            new WindowInteropHelper(_window) { Owner = owner };
            _window.Show();
        }
    }

    public void Hide()
    {
        _animTimer.Stop();
        if (_window is { IsLoaded: true })
            _window.Hide();
    }

    public void Dispose()
    {
        _animTimer.Stop();
        _animTimer.Tick -= UpdateTimer;
        _window?.Close();
        _window = null;
        _wpfParent = null;
    }

    private void UpdateTimer(object? sender, EventArgs e)
    {
        if (!TryGetAnchor(out var callbackControlLocation, out var parentSize, out double scale) || _window is null)
        {
            _animTimer.Stop();
            Hide();
            return;
        }

        _animProgress += 0.1f;
        _animProgress = (float)Math.Round(_animProgress, 1, MidpointRounding.AwayFromZero);

        bool updateLocation = _parentPosition != callbackControlLocation;
        int currentShift = 0;

        int popupWidth = (int)(_window.ActualWidth * scale);
        int popupHeight = (int)(_window.ActualHeight * scale);

        if (_animProgress <= 1.0f)
        {
            // Smoothly descend pop-up window from parent control using sine function
            var shiftDistance = popupHeight * ShiftCoefficient;
            currentShift = (int)(shiftDistance - (shiftDistance * Math.Sin(_animProgress * Math.PI / 2)));

            updateLocation = true; // Force location updating, as we are animating

            _window.Opacity = _animProgress * FinalOpacity;
        }
        else
        {
            var outroTime = _animTimeout - _animProgress;

            if (outroTime < 1.0f)
            {
                if (outroTime < 0.0f)
                    outroTime = 0.0f;
                _window.Opacity = outroTime * FinalOpacity;
                if (outroTime == 0.0f)
                {
                    _animProgress = 0.0f;
                    _animTimer.Stop();
                    _window.Hide();
                }
            }
        }

        if (updateLocation)
        {
            _parentPosition = callbackControlLocation;

            int x, y;
            switch (_alignment)
            {
                default:
                case PopupAlignment.BottomLeft:
                    x = callbackControlLocation.X + _padding;
                    y = callbackControlLocation.Y + parentSize.Height - popupHeight - _padding - currentShift;
                    break;
                case PopupAlignment.BottomRight:
                    x = callbackControlLocation.X + parentSize.Width - popupWidth - _padding;
                    y = callbackControlLocation.Y + parentSize.Height - popupHeight - _padding - currentShift;
                    break;
                case PopupAlignment.TopLeft:
                    x = callbackControlLocation.X + _padding;
                    y = callbackControlLocation.Y + _padding + currentShift;
                    break;
                case PopupAlignment.TopRight:
                    x = callbackControlLocation.X + parentSize.Width - popupWidth - _padding;
                    y = callbackControlLocation.Y + _padding + currentShift;
                    break;
                case PopupAlignment.Center:
                    x = callbackControlLocation.X + parentSize.Width / 2 - popupWidth / 2;
                    y = callbackControlLocation.Y + parentSize.Height / 2 - popupHeight / 2 + currentShift;
                    break;
            }

            _window.Left = x / scale;
            _window.Top = y / scale;
        }
    }

    private bool TryGetAnchor(out System.Drawing.Point position, out System.Drawing.Size size, out double scale)
    {
        if (_wpfParent is { IsVisible: true } visual && PresentationSource.FromVisual(visual) is not null)
        {
            var origin = visual.PointToScreen(new Point());
            scale = VisualTreeHelper.GetDpi(visual).DpiScaleX;
            position = new System.Drawing.Point((int)origin.X, (int)origin.Y);
            size = new System.Drawing.Size((int)(visual.ActualWidth * scale), (int)(visual.ActualHeight * scale));
            return true;
        }

        position = default;
        size = default;
        scale = 1.0;
        return false;
    }

    private static IntPtr GetRootHandle(IntPtr handle) => handle == IntPtr.Zero ? IntPtr.Zero : GetAncestor(handle, 2 /* GA_ROOT */);

    [DllImport("user32.dll")]
    private static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    /// <summary>The actual toast surface: borderless, transparent to clicks, never activated.</summary>
    private sealed class ToastWindow : Window
    {
        private const int ExtendedStyleIndex = -20;
        private const int TransparentStyle = 0x20;
        private const int NoActivateStyle = 0x8000000;
        private const int ToolWindowStyle = 0x80;

        private readonly TextBlock _title;
        private readonly Border _titlePanel;
        private readonly TextBlock _message;

        public ToastWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            ShowActivated = false;
            ShowInTaskbar = false;
            SizeToContent = SizeToContent.WidthAndHeight;
            IsHitTestVisible = false;
            Focusable = false;

            _title = new TextBlock { FontWeight = FontWeights.Bold, Margin = new Thickness(6, 3, 6, 3) };
            _titlePanel = new Border { Child = _title };
            _titlePanel.SetResourceReference(Border.BackgroundProperty, "Brush_Background_Low");
            _message = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(6, 4, 6, 4) };
            _message.SetResourceReference(TextBlock.ForegroundProperty, "Brush_Text");

            var stack = new StackPanel();
            stack.Children.Add(_titlePanel);
            stack.Children.Add(_message);

            var border = new Border { Child = stack, BorderThickness = new Thickness(1) };
            border.SetResourceReference(Border.BackgroundProperty, "Brush_Background");
            border.SetResourceReference(Border.BorderBrushProperty, "Brush_Border");

            Content = border;

            SourceInitialized += (_, _) =>
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                int exStyle = GetWindowLong(hwnd, ExtendedStyleIndex);
                SetWindowLong(hwnd, ExtendedStyleIndex, exStyle | TransparentStyle | NoActivateStyle | ToolWindowStyle);
            };
        }

        public void Setup(string title, string message, PopupType type, double maxWidth, double maxHeight)
        {
            bool titleIsVisible = type == PopupType.None || title != string.Empty;
            _titlePanel.Visibility = titleIsVisible ? Visibility.Visible : Visibility.Collapsed;
            _title.Text = title;
            _title.Foreground = new SolidColorBrush(type switch
            {
                PopupType.Warning => WarningColor,
                PopupType.Error => ErrorColor,
                _ => InfoColor
            });

            MaxWidth = Math.Max(maxWidth, 60.0);
            MaxHeight = Math.Max(maxHeight, 40.0);
            _message.Text = message;

            // Re-measure now so the animation immediately works with the final size.
            UpdateLayout();
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
    }
}
