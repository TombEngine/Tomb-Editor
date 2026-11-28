#nullable enable

using System;
using System.Drawing;

namespace TombLib.Input;

/// <summary>
/// Describes a pointer action in client coordinates.
/// </summary>
public sealed class PointerEventArgs(PointerButtons button, int clicks, int x, int y, int delta) : EventArgs
{
    /// <summary>Gets the pointer button associated with this action.</summary>
    public PointerButtons Button { get; } = button;
    /// <summary>Gets the click count.</summary>
    public int Clicks { get; } = clicks;
    /// <summary>Gets the horizontal client coordinate.</summary>
    public int X { get; } = x;
    /// <summary>Gets the vertical client coordinate.</summary>
    public int Y { get; } = y;
    /// <summary>Gets the wheel movement in native wheel units.</summary>
    public int Delta { get; } = delta;
    /// <summary>Gets the pointer location in client coordinates.</summary>
    public Point Location => new(X, Y);
}
