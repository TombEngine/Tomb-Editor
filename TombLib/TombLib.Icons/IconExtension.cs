#nullable enable

using System;
using System.Windows.Markup;

namespace TombLib.Icons;

/// <summary>
/// Provides vector icons to XAML from relative paths such as Actions/Playback/Play.
/// </summary>
public sealed class IconExtension : MarkupExtension
{
    /// <summary>
    /// Gets or sets the category/name path of the icon.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Creates an icon extension whose path can be assigned in XAML.
    /// </summary>
    public IconExtension()
    {
    }

    /// <summary>
    /// Creates an icon extension for the specified relative path.
    /// </summary>
    public IconExtension(string path)
    {
        Path = path;
    }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider) => IconSources.Load(Path);
}
