namespace TombLib.Scripting.UI.Editors;

/// <summary>
/// Describes the zoom bounds, step size, and reference font size used to apply a zoom step.
/// </summary>
/// <remarks>
/// The values are validated by <see cref="TextEditorViewStateCoordinator.TryApplyZoomStep"/>, so the host
/// keeps them in its runtime-mutable settings model and passes the current values with each step.
/// </remarks>
/// <param name="MinZoom">The lower bound used when zooming out, in percentage points. Must be positive.</param>
/// <param name="MaxZoom">
/// The upper bound used when zooming in, in percentage points. Must not be less than <paramref name="MinZoom"/>.
/// </param>
/// <param name="StepSize">The size of one zoom step, in percentage points. Must be positive.</param>
/// <param name="ReferenceFontSize">
/// The font size, in device-independent pixels, that a zoom of <c>100</c>% applies. Must be a finite
/// positive number.
/// </param>
internal readonly record struct ZoomOptions(int MinZoom, int MaxZoom, int StepSize, double ReferenceFontSize);
