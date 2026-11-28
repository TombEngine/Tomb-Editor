#nullable enable

using System;

namespace TombLib.Input;

/// <summary>
/// Describes PointerButtons values independently of the window framework.
/// </summary>
[Flags]
public enum PointerButtons
{
    Left = 1048576,
    None = 0,
    Right = 2097152,
    Middle = 4194304,
    XButton1 = 8388608,
    XButton2 = 16777216,
}
