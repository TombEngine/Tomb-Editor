namespace TombLib.Scripting.UI.Presentation;

/// <summary>
/// Carries the localized format strings used by the editor status presentation.
/// </summary>
/// <param name="RowFormatText">The format string for the row value.</param>
/// <param name="LineFormatText">The format string for the line value.</param>
/// <param name="ColumnFormatText">The format string for the column value.</param>
/// <param name="SelectionFormatText">The format string for the selection value.</param>
/// <param name="ZoomFormatText">The format string for the zoom value.</param>
/// <param name="ResetZoomToolTipText">The tooltip text for the reset zoom action.</param>
public sealed record class TextEditorStatusText(
	string RowFormatText,
	string LineFormatText,
	string ColumnFormatText,
	string SelectionFormatText,
	string ZoomFormatText,
	string ResetZoomToolTipText);
