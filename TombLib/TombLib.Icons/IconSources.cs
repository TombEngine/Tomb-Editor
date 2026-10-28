#nullable enable

using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace TombLib.Icons;

/// <summary>
/// Loads embedded SVG icons as frozen WPF vector images.
/// </summary>
public static class IconSources
{
    private const string AssemblyPrefix = "TombLib.Icons.Svg.";
    private static readonly Assembly Assembly = typeof(IconSources).Assembly;
    private static readonly ConcurrentDictionary<IconId, DrawingImage> Cache = new();

    /// <summary>
    /// Loads a cached vector image for the specified icon.
    /// </summary>
    public static DrawingImage Load(IconId id)
    {
        var normalizedId = IconId.Parse(id.Path);
        return Cache.GetOrAdd(normalizedId, key => {
            var resourceName = $"{AssemblyPrefix}{key.ResourcePath}.svg";
            using var stream = Assembly.GetManifestResourceStream(resourceName)
                               ?? throw new FileNotFoundException($"Embedded SVG not found: {resourceName}");
            var settings = new WpfDrawingSettings { IncludeRuntime = false, TextAsGeometry = true };
            using var reader = new FileSvgReader(settings);
            var viewport = new DrawingGroup { ClipGeometry = new RectangleGeometry(new Rect(0, 0, 16, 16)) };
            viewport.Children.Add(new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(new Rect(0, 0, 16, 16))));
            viewport.Children.Add(reader.Read(stream));
            var image = new DrawingImage(viewport);
            image.Freeze();
            return image;
        });
    }

    /// <summary>
    /// Loads a cached vector image from a relative category/name path.
    /// </summary>
    public static DrawingImage Load(string path) => Load(IconId.Parse(path));
}
