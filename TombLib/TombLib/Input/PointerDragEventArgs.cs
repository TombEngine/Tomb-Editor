#nullable enable

using System;

namespace TombLib.Input;

/// <summary>
/// Describes a drag operation and its allowed transfer effects.
/// </summary>
public sealed class PointerDragEventArgs(IDataTransfer? data, int keyState, int x, int y, TransferEffects allowedEffect, TransferEffects effect) : EventArgs
{
    /// <summary>Gets the data offered by the drag source.</summary>
    public IDataTransfer? Data { get; } = data;
    /// <summary>Gets the native pointer button and modifier state.</summary>
    public int KeyState { get; } = keyState;
    /// <summary>Gets the horizontal screen coordinate.</summary>
    public int X { get; } = x;
    /// <summary>Gets the vertical screen coordinate.</summary>
    public int Y { get; } = y;
    /// <summary>Gets the effects accepted by the source.</summary>
    public TransferEffects AllowedEffect { get; } = allowedEffect;
    /// <summary>Gets or sets the effect selected by the target.</summary>
    public TransferEffects Effect { get; set; } = effect;
}
