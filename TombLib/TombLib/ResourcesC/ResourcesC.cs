#nullable enable

using System;
using System.Drawing;
using System.IO;

namespace TombLib.ResourcesC;

/// <summary>
/// Loads the editor's embedded reference textures and binary resources.
/// </summary>
public static class ResourcesC
{
    private static readonly Lazy<Bitmap> ExtraTexture = new(() => LoadBitmap("Extra.Tr5.pc.png"));
    private static readonly Lazy<Bitmap> FontTexture = new(() => LoadBitmap("Font.pc.png"));
    private static readonly Lazy<Bitmap> SkyTexture = new(() => LoadBitmap("pcsky.raw.png"));
    private static readonly Lazy<byte[]> SpriteFont = new(() => LoadBytes("font.bin"));
    private static readonly Lazy<byte[]> Palette = new(() => LoadBytes("palette.bin"));

    /// <summary>Gets the default TR5 extra texture page.</summary>
    public static Bitmap Extra_Tr5_pc => ExtraTexture.Value;
    /// <summary>Gets the default font texture page.</summary>
    public static Bitmap Font_pc => FontTexture.Value;
    /// <summary>Gets the default sky texture page.</summary>
    public static Bitmap pcsky_raw => SkyTexture.Value;
    /// <summary>Gets the legacy renderer sprite font data.</summary>
    public static byte[] font => SpriteFont.Value;
    /// <summary>Gets the editor's default palette data.</summary>
    public static byte[] palette => Palette.Value;

    private static Stream Open(string name)
        => typeof(ResourcesC).Assembly.GetManifestResourceStream($"TombLib.ResourcesC.{name}")
            ?? throw new InvalidOperationException($"The embedded resource '{name}' is missing.");

    private static byte[] LoadBytes(string name)
    {
        using var source = Open(name);
        using var destination = new MemoryStream();
        source.CopyTo(destination);
        return destination.ToArray();
    }

    private static Bitmap LoadBitmap(string name)
    {
        using var source = Open(name);
        using var bitmap = new Bitmap(source);
        return new Bitmap(bitmap);
    }
}
