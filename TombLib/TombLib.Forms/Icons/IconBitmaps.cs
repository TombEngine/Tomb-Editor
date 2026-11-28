#nullable enable

using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using DrawingGraphics = System.Drawing.Graphics;

namespace TombLib.Icons;

/// <summary>
/// Loads cached embedded PNG icons at the appropriate DPI scale.
/// </summary>
public static class IconBitmaps
{
    private const string AssemblyPrefix = "TombLib.Icons.Png.";
    private static readonly Assembly Assembly = typeof(IconId).Assembly;
    private static readonly ConcurrentDictionary<(IconId Id, int Scale), Bitmap> Cache = new();

    /// <summary>
    /// Loads the icon variant closest to the specified scale relative to 96 DPI.
    /// </summary>
    public static Bitmap Load(IconId id, float dpiScale) => LoadBucket(id, PickBucket(dpiScale));

    /// <summary>
    /// Loads an icon path at the variant closest to the specified DPI scale.
    /// </summary>
    public static Bitmap Load(string path, float dpiScale) => Load(IconId.Parse(path), dpiScale);

    /// <summary>
    /// Loads the 1x variant for a DPI-unaware hosting process.
    /// </summary>
    public static Bitmap Load(IconId id) => LoadBucket(id, 1);

    /// <summary>
    /// Loads an icon path at the 1x variant.
    /// </summary>
    public static Bitmap Load(string path) => Load(IconId.Parse(path));

    /// <summary>
    /// Loads at the control's DPI, falling back to the process DPI when no control is supplied.
    /// </summary>
    public static Bitmap LoadForControl(IconId id, Control? control)
        => Load(id, control is null ? CurrentDpiScale() : control.DeviceDpi / 96.0f);

    /// <summary>
    /// Loads a specific 1x, 2x, 3x or 4x variant. Cached bitmaps remain owned by this loader.
    /// </summary>
    public static Bitmap LoadBucket(IconId id, int bucket)
    {
        if (bucket is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(bucket), "Bucket must be 1..4.");

        var normalizedId = IconId.Parse(id.Path);
        return Cache.GetOrAdd((normalizedId, bucket), key => {
            // MSBuild rewrites the @Nx directory name to _Nx in manifest resources.
            var resourceName = $"{AssemblyPrefix}_{key.Scale}x.{key.Id.ResourcePath}.png";
            using var stream = Assembly.GetManifestResourceStream(resourceName)
                               ?? throw new FileNotFoundException($"Embedded icon not found: {resourceName}");
            using var source = new Bitmap(stream);
            var bitmap = new Bitmap(source) { Tag = key.Id };
            return bitmap;
        });
    }

    private static int PickBucket(float dpiScale) => dpiScale switch
    {
        < 1.30f => 1,
        < 1.85f => 2,
        < 2.85f => 3,
        _ => 4
    };

    /// <summary>
    /// Reads the current process DPI scale relative to 96 DPI.
    /// </summary>
    public static float CurrentDpiScale()
    {
        using var graphics = DrawingGraphics.FromHwnd(IntPtr.Zero);
        return graphics.DpiX / 96.0f;
    }
}
