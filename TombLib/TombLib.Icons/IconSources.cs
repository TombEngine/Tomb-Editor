#nullable enable

using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
    private static readonly ConcurrentDictionary<(IconId Id, bool Inverted), DrawingImage> Cache = new();
    private static readonly Regex HexColor = new(@"((?:fill|stroke|stop-color|flood-color|color)\s*[:=]\s*""?\s*)#([0-9a-fA-F]{6}|[0-9a-fA-F]{3})\b", RegexOptions.Compiled);

    /// <summary>
    /// Gets or sets whether icon colors are inverted, so white glyphs stay visible on light palettes.
    /// Set this before icons are loaded.
    /// </summary>
    public static bool InvertColors { get; set; }

    /// <summary>
    /// Loads a cached vector image for the specified icon.
    /// </summary>
    public static DrawingImage Load(IconId id)
    {
        var normalizedId = IconId.Parse(id.Path);
        var inverted = InvertColors;
        return Cache.GetOrAdd((normalizedId, inverted), key => {
            var resourceName = $"{AssemblyPrefix}{key.Id.ResourcePath}.svg";
            using var stream = Assembly.GetManifestResourceStream(resourceName)
                               ?? throw new FileNotFoundException($"Embedded SVG not found: {resourceName}");
            using var buffer = new MemoryStream();
            if (key.Inverted)
                WriteInvertedSvg(stream, buffer);
            else
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
    /// Copies an SVG while inverting its hex colors.
    /// </summary>
    private static void WriteInvertedSvg(Stream source, Stream destination)
    {
        using var reader = new StreamReader(source);
        var svg = HexColor.Replace(reader.ReadToEnd(), InvertColor);
        var bytes = Encoding.UTF8.GetBytes(svg);
        destination.Write(bytes, 0, bytes.Length);
    }

    private static string InvertColor(Match match)
    {
        var hex = match.Groups[2].Value;
        if (hex.Length == 3)
            hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);

        var inverted = Convert.ToInt32(hex, 16) ^ 0xFFFFFF;
        return $"{match.Groups[1].Value}#{inverted:X6}";
    }

    /// <summary>
    /// Loads a cached vector image from a relative category/name path.
    /// </summary>
    public static DrawingImage Load(string path) => Load(IconId.Parse(path));
}
