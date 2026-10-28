#nullable enable

using System;
using System.Linq;

namespace TombLib.Icons;

/// <summary>
/// Identifies an icon by its case-sensitive category path and file name without an extension.
/// </summary>
public readonly record struct IconId(string Category, string Name)
{
    /// <summary>
    /// Gets the slash-separated icon path, including any category subdirectories.
    /// </summary>
    public string Path => $"{Category}/{Name}";

    /// <inheritdoc />
    public override string ToString() => Path;

    /// <summary>
    /// Parses a relative icon path such as "Actions/Play" or "Actions/Playback/Play".
    /// </summary>
    public static IconId Parse(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        var normalizedPath = path.Replace('\\', '/');
        var segments = normalizedPath.Split('/');

        if (segments.Length < 2 || segments.Any(IsInvalidSegment))
            throw new ArgumentException("Expected a relative category/name icon path with nonempty segments.", nameof(path));

        int slash = normalizedPath.LastIndexOf('/');
        return new IconId(normalizedPath[..slash], normalizedPath[(slash + 1)..]);
    }

    internal string ResourcePath => Parse(Path).Path.Replace('/', '.');

    private static bool IsInvalidSegment(string segment) =>
        string.IsNullOrWhiteSpace(segment) || segment is "." or ".." ||
        segment.Any(character => char.IsControl(character) || character is ':' or '*' or '?' or '"' or '<' or '>' or '|');
}
