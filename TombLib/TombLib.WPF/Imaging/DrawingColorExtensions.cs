using System;
using System.Drawing;
using System.Numerics;

namespace TombLib.WPF;

/// <summary>Converts editor colors to drawing primitives used by the rendering pipeline.</summary>
public static class DrawingColorExtensions
{
    /// <summary>Converts normalized RGB channels to an opaque drawing color.</summary>
    public static Color ToDrawingColor(this Vector3 color) => new Vector4(color, 1.0f).ToDrawingColor();

    /// <summary>Converts normalized RGB channels with an explicit alpha channel.</summary>
    public static Color ToDrawingColor(this Vector3 color, float alpha) => new Vector4(color, alpha).ToDrawingColor();

    /// <summary>Converts normalized RGBA channels, optionally overriding alpha.</summary>
    public static Color ToDrawingColor(this Vector4 color, float? alpha = null)
        => Color.FromArgb(ToChannel(alpha ?? color.W), ToChannel(color.X), ToChannel(color.Y), ToChannel(color.Z));

    /// <summary>Converts drawing RGB channels to the editor's normalized color representation.</summary>
    public static Vector3 ToFloat3Color(this Color color)
        => IsDefaultGray(color) ? new Vector3(0.5f) : new Vector3(color.R, color.G, color.B) / 255.0f;

    /// <summary>Converts drawing RGBA channels to the editor's normalized color representation.</summary>
    public static Vector4 ToFloat4Color(this Color color)
        => IsDefaultGray(color) ? new Vector4(0.5f) : new Vector4(color.R, color.G, color.B, color.A) / 255.0f;

    /// <summary>Checks whether all drawing color channels have equal intensity.</summary>
    public static bool IsGrayscale(this Color color) => color.R == color.G && color.G == color.B;

    private static int ToChannel(float value) => Math.Clamp((int)Math.Round(value * 255.0f), 0, 255);

    private static bool IsDefaultGray(Color color) => color.R == 128 && color.G == 128 && color.B == 128;
}
