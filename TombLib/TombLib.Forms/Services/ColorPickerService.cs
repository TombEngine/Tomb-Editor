#nullable enable

using System;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using TombLib.Rendering;
using TombLib.WPF.Features.ColorPicker;
using TombLib.WPF.Services.Abstract;

namespace TombLib.Forms.Services;

/// <summary>Shows the shared WPF color picker and preserves custom color slots in the editor color scheme.</summary>
public class ColorPickerService : IColorPickerService
{
    private readonly Func<ColorScheme?>? _colorSchemeProvider;

    /// <param name="colorSchemeProvider">Provides the color scheme whose custom slots are read and persisted on confirmation.</param>
    public ColorPickerService(Func<ColorScheme?>? colorSchemeProvider = null) => _colorSchemeProvider = colorSchemeProvider;

    public Vector3? PickColor(Vector3 initialColor, Action<Vector3>? onColorChanged = null)
    {
        var scheme = _colorSchemeProvider?.Invoke();
        var colors = scheme?.CustomColors.Select(color => ToColor(new Vector3(color.X, color.Y, color.Z)))
            ?? Enumerable.Repeat(Colors.White, 16);
        var viewModel = new ColorPickerViewModel(ToColor(initialColor), colors);
        var dialog = new ColorPickerWindow { DataContext = viewModel };
        var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive);
        if (owner is not null)
            dialog.Owner = owner;

        viewModel.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(ColorPickerViewModel.SelectedColor))
                onColorChanged?.Invoke(ToVector(viewModel.SelectedColor));
        };

        if (dialog.ShowDialog() != true)
        {
            onColorChanged?.Invoke(initialColor);
            return null;
        }

        if (scheme is not null)
        {
            for (int i = 0; i < scheme.CustomColors.Length && i < viewModel.CustomColors.Count; i++)
                scheme.CustomColors[i] = new Vector4(ToVector(viewModel.CustomColors[i].Color), 1.0f);
        }

        return ToVector(viewModel.SelectedColor);
    }

    private static Color ToColor(Vector3 color) => Color.FromRgb(ToChannel(color.X), ToChannel(color.Y), ToChannel(color.Z));

    private static byte ToChannel(float value) => (byte)Math.Clamp((int)Math.Round(value * 255.0f), 0, 255);

    private static Vector3 ToVector(Color color) => new Vector3(color.R, color.G, color.B) / 255.0f;
}
