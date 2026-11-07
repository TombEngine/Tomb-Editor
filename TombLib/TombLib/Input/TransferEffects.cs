#nullable enable

using System;

namespace TombLib.Input;

/// <summary>
/// Describes TransferEffects values independently of the window framework.
/// </summary>
[Flags]
public enum TransferEffects
{
    None = 0,
    Copy = 1,
    Move = 2,
    Link = 4,
    Scroll = -2147483648,
    All = -2147483645,
}
