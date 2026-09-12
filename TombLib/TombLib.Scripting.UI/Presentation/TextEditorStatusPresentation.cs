using Nickelony.IDEKit.IntelliSense.Signatures;

namespace TombLib.Scripting.UI.Presentation;

/// <summary>
/// Describes the current state of the editor status presentation.
/// </summary>
/// <param name="HasEditor">Whether an editor is currently active.</param>
/// <param name="RowLabelText">The localized label for the row value.</param>
/// <param name="ColumnLabelText">The localized label for the column value.</param>
/// <param name="SelectionLabelText">The localized label for the selection value.</param>
/// <param name="ZoomLabelText">The localized label for the zoom value.</param>
/// <param name="CanResetZoom">Whether the zoom can be reset.</param>
/// <param name="ResetZoomToolTipText">The tooltip text for the reset zoom action.</param>
/// <param name="ShowSyntaxPreview">Whether the syntax preview is visible.</param>
/// <param name="SyntaxPreview">The signature help info for the syntax preview, when visible.</param>
public readonly record struct TextEditorStatusPresentation(
	bool HasEditor,
	string RowLabelText,
	string ColumnLabelText,
	string SelectionLabelText,
	string ZoomLabelText,
	bool CanResetZoom,
	string ResetZoomToolTipText,
	bool ShowSyntaxPreview,
	TextSignatureHelpInfo? SyntaxPreview)
{
	/// <summary>
	/// Creates an empty status presentation with no active editor.
	/// </summary>
	/// <param name="showSyntaxPreview">Whether the syntax preview should be shown.</param>
	/// <returns>The empty status presentation.</returns>
	public static TextEditorStatusPresentation Empty(bool showSyntaxPreview) => new(
		HasEditor: false,
		RowLabelText: string.Empty,
		ColumnLabelText: string.Empty,
		SelectionLabelText: string.Empty,
		ZoomLabelText: string.Empty,
		CanResetZoom: false,
		ResetZoomToolTipText: string.Empty,
		ShowSyntaxPreview: showSyntaxPreview,
		SyntaxPreview: null);
}
