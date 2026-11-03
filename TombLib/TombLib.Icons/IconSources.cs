#nullable enable

using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

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
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            buffer.Position = 0;
            var size = ReadViewportSize(buffer);
            buffer.Position = 0;

            var settings = new WpfDrawingSettings { IncludeRuntime = false, TextAsGeometry = true };
            using var reader = new FileSvgReader(settings);
            var bounds = new Rect(0, 0, size.Width, size.Height);
            var viewport = new DrawingGroup { ClipGeometry = new RectangleGeometry(bounds) };
            viewport.Children.Add(new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(bounds)));
            viewport.Children.Add(reader.Read(buffer));
            var image = new DrawingImage(viewport);
            image.Freeze();
            return image;
        });
    }

    /// <summary>
    /// Reads the size of the SVG view box. Icons are 16 by 16 unless the file declares another size, such as the 24 pixel transport icons.
    /// </summary>
    private static Size ReadViewportSize(Stream stream)
    {
        var viewBox = XDocument.Load(stream).Root?.Attribute("viewBox")?.Value;
        if (viewBox is null)
            return new Size(16, 16);

        var parts = viewBox.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var width) || !double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
            return new Size(16, 16);

        return new Size(width, height);
    }

    /// <summary>
    /// Loads a cached vector image from a relative category/name path.
    /// </summary>
    public static DrawingImage Load(string path) => Load(IconId.Parse(path));
}
