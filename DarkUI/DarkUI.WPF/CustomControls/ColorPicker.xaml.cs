using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DarkUI.WPF.CustomControls;

/// <summary>A native WPF RGB color editor with a hue and saturation/value selection surface.</summary>
public partial class ColorPicker : UserControl
{
    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
        nameof(SelectedColor), typeof(Color), typeof(ColorPicker),
        new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorChanged));
    public static readonly DependencyProperty HueProperty = DependencyProperty.Register(
        nameof(Hue), typeof(double), typeof(ColorPicker), new PropertyMetadata(0.0, HueChanged));
    public static readonly DependencyProperty RedProperty = DependencyProperty.Register(
        nameof(Red), typeof(int), typeof(ColorPicker), new PropertyMetadata(255, RgbChanged));
    public static readonly DependencyProperty GreenProperty = DependencyProperty.Register(
        nameof(Green), typeof(int), typeof(ColorPicker), new PropertyMetadata(255, RgbChanged));
    public static readonly DependencyProperty BlueProperty = DependencyProperty.Register(
        nameof(Blue), typeof(int), typeof(ColorPicker), new PropertyMetadata(255, RgbChanged));
    public static readonly DependencyProperty HexProperty = DependencyProperty.Register(
        nameof(Hex), typeof(string), typeof(ColorPicker), new PropertyMetadata("#FFFFFF", HexChanged));
    public static readonly DependencyProperty HueBrushProperty = DependencyProperty.Register(
        nameof(HueBrush), typeof(Brush), typeof(ColorPicker), new PropertyMetadata(Brushes.Red));
    public static readonly DependencyProperty PreviewBrushProperty = DependencyProperty.Register(
        nameof(PreviewBrush), typeof(Brush), typeof(ColorPicker), new PropertyMetadata(Brushes.White));

    private bool _updating;
    private double _saturation;
    private double _value = 1.0;

    public Color SelectedColor { get => (Color)GetValue(SelectedColorProperty); set => SetValue(SelectedColorProperty, value); }
    public double Hue { get => (double)GetValue(HueProperty); set => SetValue(HueProperty, value); }
    public int Red { get => (int)GetValue(RedProperty); set => SetValue(RedProperty, value); }
    public int Green { get => (int)GetValue(GreenProperty); set => SetValue(GreenProperty, value); }
    public int Blue { get => (int)GetValue(BlueProperty); set => SetValue(BlueProperty, value); }
    public string Hex { get => (string)GetValue(HexProperty); set => SetValue(HexProperty, value); }
    public Brush HueBrush { get => (Brush)GetValue(HueBrushProperty); private set => SetValue(HueBrushProperty, value); }
    public Brush PreviewBrush { get => (Brush)GetValue(PreviewBrushProperty); private set => SetValue(PreviewBrushProperty, value); }

    public ColorPicker()
    {
        InitializeComponent();
        UpdateComponents();
    }

    private static void ColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => ((ColorPicker)sender).UpdateComponents();

    private static void HueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var picker = (ColorPicker)sender;
        picker.HueBrush = CreateBrush(FromHsv(picker.Hue, 1.0, 1.0));
        if (!picker._updating)
            picker.SetCurrentValue(SelectedColorProperty, FromHsv(picker.Hue, picker._saturation, picker._value));
    }

    private static void RgbChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var picker = (ColorPicker)sender;
        if (!picker._updating)
        {
            picker.SetCurrentValue(SelectedColorProperty,
                Color.FromRgb((byte)Math.Clamp(picker.Red, 0, 255), (byte)Math.Clamp(picker.Green, 0, 255), (byte)Math.Clamp(picker.Blue, 0, 255)));
        }
    }

    private static void HexChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var picker = (ColorPicker)sender;
        if (picker._updating)
            return;

        var text = picker.Hex?.Trim().TrimStart('#');
        if (text?.Length == 6 && int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rgb))
            picker.SetCurrentValue(SelectedColorProperty, Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb));
        else
            picker.SetCurrentValue(HexProperty, $"#{picker.SelectedColor.R:X2}{picker.SelectedColor.G:X2}{picker.SelectedColor.B:X2}");
    }

    private void UpdateComponents()
    {
        _updating = true;
        try
        {
            var color = SelectedColor;
            double red = color.R / 255.0;
            double green = color.G / 255.0;
            double blue = color.B / 255.0;
            double maximum = Math.Max(red, Math.Max(green, blue));
            double minimum = Math.Min(red, Math.Min(green, blue));
            double delta = maximum - minimum;
            _value = maximum;
            _saturation = maximum == 0.0 ? 0.0 : delta / maximum;
            if (delta > 0.0)
            {
                double hue = maximum == red ? (green - blue) / delta : maximum == green ? (blue - red) / delta + 2.0 : (red - green) / delta + 4.0;
                SetCurrentValue(HueProperty, (hue * 60.0 + 360.0) % 360.0);
            }

            SetCurrentValue(RedProperty, (int)color.R);
            SetCurrentValue(GreenProperty, (int)color.G);
            SetCurrentValue(BlueProperty, (int)color.B);
            SetCurrentValue(HexProperty, $"#{color.R:X2}{color.G:X2}{color.B:X2}");
            PreviewBrush = CreateBrush(color);
            UpdateMarker();
        }
        finally
        {
            _updating = false;
        }
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static Color FromHsv(double hue, double saturation, double value)
    {
        double chroma = value * saturation;
        double intermediate = chroma * (1.0 - Math.Abs(hue / 60.0 % 2.0 - 1.0));
        double match = value - chroma;
        var (red, green, blue) = hue switch
        {
            < 60.0 => (chroma, intermediate, 0.0),
            < 120.0 => (intermediate, chroma, 0.0),
            < 180.0 => (0.0, chroma, intermediate),
            < 240.0 => (0.0, intermediate, chroma),
            < 300.0 => (intermediate, 0.0, chroma),
            _ => (chroma, 0.0, intermediate)
        };
        return Color.FromRgb((byte)Math.Round((red + match) * 255.0), (byte)Math.Round((green + match) * 255.0), (byte)Math.Round((blue + match) * 255.0));
    }

    private void SurfaceMouseDown(object sender, MouseButtonEventArgs e)
    {
        Surface.CaptureMouse();
        UpdateFromPosition(e.GetPosition(Surface));
        e.Handled = true;
    }

    private void SurfaceMouseMove(object sender, MouseEventArgs e)
    {
        if (Surface.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            UpdateFromPosition(e.GetPosition(Surface));
    }

    private void SurfaceMouseUp(object sender, MouseButtonEventArgs e) => Surface.ReleaseMouseCapture();

    private void SurfaceSizeChanged(object sender, SizeChangedEventArgs e) => UpdateMarker();

    private void UpdateFromPosition(Point position)
    {
        _saturation = Math.Clamp(position.X / Math.Max(1.0, Surface.ActualWidth), 0.0, 1.0);
        _value = 1.0 - Math.Clamp(position.Y / Math.Max(1.0, Surface.ActualHeight), 0.0, 1.0);
        SetCurrentValue(SelectedColorProperty, FromHsv(Hue, _saturation, _value));
        UpdateMarker();
    }

    private void UpdateMarker()
    {
        if (Marker is null || Surface is null)
            return;

        Canvas.SetLeft(Marker, _saturation * Surface.ActualWidth - 5.0);
        Canvas.SetTop(Marker, (1.0 - _value) * Surface.ActualHeight - 5.0);
    }
}
