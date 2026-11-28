#nullable enable

namespace TombLib.Services;

/// <summary>
/// Provides the native owner handle for a desktop window.
/// </summary>
public interface IWindowOwner
{
    /// <summary>
    /// Gets the native window handle.
    /// </summary>
    nint Handle { get; }
}
