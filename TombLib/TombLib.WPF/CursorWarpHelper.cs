#nullable enable

using DrawingPoint = System.Drawing.Point;
using System;
using System.Windows;
using TombLib.WPF.Input;

namespace TombLib.WPF;

/// <summary>
/// Provides helpers for wrapping the OS cursor across WPF control edges during captured mouse drags.
/// </summary>
public static class CursorWarpHelper
{
    private const double DefaultEdgeThreshold = 1.0;
    private const double DefaultEdgeInset = 2.0;

    /// <summary>
    /// Returns whether a stale pre-warp mouse event should be ignored.
    /// </summary>
    public static bool IsPendingWarpStale(double currentPosition, double targetPosition, double extent)
    {
        if (extent <= 0.0)
            return false;

        bool targetOnLowerHalf = targetPosition <= extent * 0.5;
        bool currentOnLowerHalf = currentPosition <= extent * 0.5;
        return targetOnLowerHalf != currentOnLowerHalf;
    }

    /// <summary>
    /// Returns whether a stale pre-warp mouse event should be ignored.
    /// </summary>
    public static bool IsPendingWarpStale(Point currentPosition, CursorWarpResult target, Size extent)
    {
        if (target.WarpedX && IsPendingWarpStale(currentPosition.X, target.Position.X, extent.Width))
            return true;

        if (target.WarpedY && IsPendingWarpStale(currentPosition.Y, target.Position.Y, extent.Height))
            return true;

        return false;
    }

    /// <summary>
    /// Warps the OS cursor horizontally when it reaches a control edge.
    /// </summary>
    public static bool TryWarpHorizontal(
        FrameworkElement element,
        Point currentPosition,
        bool allowLeftEdgeWarp,
        bool allowRightEdgeWarp,
        out CursorWarpResult result,
        double edgeThreshold = DefaultEdgeThreshold,
        double edgeInset = DefaultEdgeInset)
    {
        return TryWarp(
            element,
            currentPosition,
            allowLeftEdgeWarp,
            allowRightEdgeWarp,
            allowTopEdgeWarp: false,
            allowBottomEdgeWarp: false,
            out result,
            edgeThreshold,
            edgeInset);
    }

    /// <summary>
    /// Warps the OS cursor when it reaches a control edge.
    /// </summary>
    public static bool TryWarp(
        FrameworkElement element,
        Point currentPosition,
        bool allowLeftEdgeWarp,
        bool allowRightEdgeWarp,
        bool allowTopEdgeWarp,
        bool allowBottomEdgeWarp,
        out CursorWarpResult result,
        double edgeThreshold = DefaultEdgeThreshold,
        double edgeInset = DefaultEdgeInset)
    {
        ArgumentNullException.ThrowIfNull(element);

        var warpedPosition = currentPosition;
        bool warpedX = TryGetWarpedCoordinate(
            currentPosition.X, element.ActualWidth,
            allowLeftEdgeWarp, allowRightEdgeWarp,
            out double warpedXPosition,
            edgeThreshold, edgeInset);
        bool warpedY = TryGetWarpedCoordinate(
            currentPosition.Y, element.ActualHeight,
            allowTopEdgeWarp, allowBottomEdgeWarp,
            out double warpedYPosition,
            edgeThreshold, edgeInset);

        if (warpedX)
            warpedPosition.X = warpedXPosition;

        if (warpedY)
            warpedPosition.Y = warpedYPosition;

        warpedPosition = CoerceLocalPosition(element, warpedPosition);

        result = new CursorWarpResult(warpedPosition, warpedX, warpedY);

        if (!result.Warped)
            return false;

        SetCursorPosition(element, warpedPosition);
        return true;
    }

    /// <summary>
    /// Calculates a wrapped cursor position for a single axis when the pointer touches either edge.
    /// </summary>
    /// <param name="currentPosition">The current local cursor coordinate on the axis.</param>
    /// <param name="extent">The local size of the control on the axis.</param>
    /// <param name="allowLowerEdgeWarp">Whether hitting the lower edge should wrap to the opposite side.</param>
    /// <param name="allowUpperEdgeWarp">Whether hitting the upper edge should wrap to the opposite side.</param>
    /// <param name="warpedPosition">Receives the wrapped local coordinate when a warp is allowed.</param>
    /// <param name="edgeThreshold">The edge distance at which warping becomes eligible.</param>
    /// <param name="edgeInset">The inset used for the wrapped destination so the cursor stays inside the control.</param>
    /// <returns><see langword="true"/> when the coordinate was wrapped; otherwise <see langword="false"/>.</returns>
    private static bool TryGetWarpedCoordinate(
        double currentPosition,
        double extent,
        bool allowLowerEdgeWarp,
        bool allowUpperEdgeWarp,
        out double warpedPosition,
        double edgeThreshold,
        double edgeInset)
    {
        warpedPosition = currentPosition;

        if (extent <= edgeInset * 2.0)
            return false;

        if (currentPosition <= edgeThreshold)
        {
            if (!allowLowerEdgeWarp)
                return false;

            warpedPosition = extent - edgeInset;
            return true;
        }

        if (currentPosition >= extent - edgeThreshold)
        {
            if (!allowUpperEdgeWarp)
                return false;

            warpedPosition = edgeInset;
            return true;
        }

        return false;
    }

    private static Point CoerceLocalPosition(FrameworkElement element, Point localPosition)
    {
        return new Point(
            Math.Clamp(localPosition.X, 0.0, Math.Max(0.0, element.ActualWidth - 1.0)),
            Math.Clamp(localPosition.Y, 0.0, Math.Max(0.0, element.ActualHeight - 1.0)));
    }

    private static void SetCursorPosition(FrameworkElement element, Point localPosition)
    {
        var screenPoint = element.PointToScreen(localPosition);

        PointerState.ScreenPosition = new DrawingPoint(
            (int)Math.Round(screenPoint.X),
            (int)Math.Round(screenPoint.Y));
    }
}

/// <summary>
/// Describes the result of a cursor warp operation.
/// </summary>
public readonly record struct CursorWarpResult(Point Position, bool WarpedX, bool WarpedY)
{
    /// <summary>
    /// Gets a value indicating whether either axis was warped.
    /// </summary>
    public bool Warped => WarpedX || WarpedY;
}
