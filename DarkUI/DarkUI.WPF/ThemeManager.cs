using DarkUI.WPF.Dictionaries;
using System;
using System.Collections.Generic;
using System.Windows;

namespace DarkUI.WPF;

/// <summary>
/// Swaps the active theme <see cref="ResourceDictionary"/> set in
/// <see cref="Application.Current"/>.Resources at runtime. Control templates
/// reference brushes via <c>DynamicResource</c>, so every control re-resolves
/// the new palette without a reload.
/// </summary>
public static class ThemeManager
{
    private static readonly List<ResourceDictionary> _current = new();

    public static event Action<Theme>? ThemeChanged;

    public static void Apply(Theme theme)
    {
        var app = Application.Current
            ?? throw new InvalidOperationException("ThemeManager.Apply requires Application.Current. Call after WPFInitializer.InitializeWPF().");

        foreach (var dict in _current)
            app.Resources.MergedDictionaries.Remove(dict);
        _current.Clear();

        foreach (var dict in Load(theme))
        {
            app.Resources.MergedDictionaries.Add(dict);
            _current.Add(dict);
        }

        Defaults.CurrentTheme = theme;
        // Light and WinRoomEdit both use light-background palettes; the SVG
        // icon set is white-on-transparent, so invert to render them black.
        Defaults.ShouldIconsInvert = theme is Theme.Light or Theme.WinRoomEdit;
        ThemeChanged?.Invoke(theme);
    }

    private static IEnumerable<ResourceDictionary> Load(Theme theme)
    {
        switch (theme)
        {
            case Theme.Dark:
                // DarkColors has codebehind that mirrors legacy palette colors
                // when the user changed brightness in the shared options.
                yield return new DarkColors();
                break;
            case Theme.Black:
                yield return new ResourceDictionary { Source = Pack("Dictionaries/BlackColors.xaml") };
                break;
            case Theme.Light:
                yield return new ResourceDictionary { Source = Pack("Dictionaries/LightColors.xaml") };
                break;
            case Theme.WinRoomEdit:
                // Single aggregator that pulls in ClassicColors + classic control templates.
                yield return new ResourceDictionary { Source = Pack("ClassicRoomEdit/ClassicRoomEdit.xaml") };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(theme), theme, null);
        }
    }

    private static Uri Pack(string relativePath)
        => new($"pack://application:,,,/DarkUI.WPF;component/{relativePath}", UriKind.Absolute);
}
