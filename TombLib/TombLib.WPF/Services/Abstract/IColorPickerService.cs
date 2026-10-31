#nullable enable

using System;
using System.Numerics;

namespace TombLib.WPF.Services.Abstract;

/// <summary>
/// Provides a modal color picker dialog for view models,
/// abstracting away the concrete dialog implementation.
/// </summary>
public interface IColorPickerService
{
    /// <summary>
    /// Shows a modal color picker initialized with <paramref name="initialColor"/> (RGB, 0-1 range).
    /// <para><paramref name="onColorChanged"/> is invoked live while the user edits the color, enabling realtime previews.</para>
    /// </summary>
    /// <param name="initialColor">The color the picker starts with.</param>
    /// <param name="onColorChanged">Optional callback raised on every color change and restored to the initial color on cancel.</param>
    /// <returns>The chosen color, or <see langword="null"/> if the user cancelled.</returns>
    Vector3? PickColor(Vector3 initialColor, Action<Vector3>? onColorChanged = null);
}
