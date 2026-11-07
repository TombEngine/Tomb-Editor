using System;

namespace DarkUI.WPF.AvalonDock;

public sealed class DarkUIDockTheme : global::AvalonDock.Themes.Theme
{
    // One template serves every DarkUI palette. Its brushes are DynamicResource tokens,
    // so a palette change from ThemeManager re-skins open docks without swapping the theme.
    private static readonly Uri ResourceUri = new("/DarkUI.WPF;component/Styles/AvalonDock/Theme.xaml", UriKind.Relative);

    public override Uri GetResourceUri() => ResourceUri;
}
