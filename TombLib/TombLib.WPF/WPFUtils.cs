using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using TombLib.Services;

namespace TombLib.WPF;

public static class WPFUtils
{
    private const uint RootAncestor = 2;

    internal static IEnumerable<Window> OpenWindows => Application.Current?.Windows.OfType<Window>()
        ?? PresentationSource.CurrentSources.OfType<HwndSource>().Select(source => source.RootVisual).OfType<Window>();

    /// <summary>Gets the active WPF window handle for native dialog ownership.</summary>
    public static IWindowOwner GetWindowOwner()
        => new WindowOwner(GetOwnerWindow(IntPtr.Zero) is { } window ? new WindowInteropHelper(window).Handle : IntPtr.Zero);

    /// <summary>Resolves a native owner handle, falling back to the active or main WPF window.</summary>
    public static Window? GetOwnerWindow(IntPtr handle)
    {
        var rootHandle = handle == IntPtr.Zero ? IntPtr.Zero : GetAncestor(handle, RootAncestor);
        Window? active = null;

        foreach (var window in OpenWindows)
        {
            if (rootHandle != IntPtr.Zero && new WindowInteropHelper(window).Handle == rootHandle)
                return window;

            if (window.IsActive)
                active = window;
        }

        return active ?? Application.Current?.MainWindow;
    }

    public static Color ToWPFColor(this Vector3 color) => Color.FromRgb((byte)(color.X * 255.0f), (byte)(color.Y * 255.0f), (byte)(color.Z * 255.0f));
    public static Color ToWPFColor(this Vector4 color) => Color.FromArgb((byte)(color.W * 255.0f), (byte)(color.X * 255.0f), (byte)(color.Y * 255.0f), (byte)(color.Z * 255.0f));

    public static Brush ToWPFBrush(this Vector3 color) => new Vector4(color, 1.0f).ToWPFBrush();
    public static Vector3 ToFloat3Color(this Color color) => new Vector3(color.R, color.G, color.B) / 255.0f;
    public static Vector4 ToFloat4Color(this Color color) => new Vector4(color.R, color.G, color.B, color.A) / 255.0f;

    public static Brush ToWPFBrush(this Vector4 color, float? alpha = null)
    {
        var brushColor = Color.FromArgb(
            (byte)Math.Max(0, Math.Min(255, Math.Round((alpha.HasValue ? Math.Clamp(alpha.Value, 0.0, 1.0) : color.W) * 255.0f))),
            (byte)Math.Max(0, Math.Min(255, Math.Round(color.X * 255.0f))),
            (byte)Math.Max(0, Math.Min(255, Math.Round(color.Y * 255.0f))),
            (byte)Math.Max(0, Math.Min(255, Math.Round(color.Z * 255.0f))));

        return BrushHelpers.CreateFrozenBrush(brushColor);
    }

    public static Brush ToWPFBrush(this Vector3 color, float alpha)
    {
        return ToWPFBrush(new Vector4(color.X, color.Y, color.Z, alpha));
    }

    public static float GetBrightness(this Brush b)
    {
        if (b is SolidColorBrush scb)
        {
            Color c = scb.Color;
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B).GetBrightness();
        }
        else
        {
            return 1.0f;
        }
    }

    public static IEnumerable<Control> AllSubControls(DependencyObject? depObj)
    {
        if (depObj is null)
            yield break;

        foreach (var child in LogicalTreeHelper.GetChildren(depObj))
        {
            if (child is DependencyObject dpo)
            {
                if (child is Control t)
                    yield return t;

                foreach (Control childOfChild in AllSubControls(dpo))
                    yield return childOfChild;
            }
        }
    }

    private sealed record WindowOwner(IntPtr Handle) : IWindowOwner;

    [DllImport("user32.dll")]
    private static extern IntPtr GetAncestor(IntPtr window, uint flags);
}
