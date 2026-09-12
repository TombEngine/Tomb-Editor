using Nickelony.IDEKit.AvalonEdit.IntelliSense.Signatures;
using System;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombLib.Scripting.UI.Presentation;

/// <summary>
/// Builds the editor status presentation from an editor control.
/// </summary>
public sealed class TextEditorStatusPresentationService
{
	/// <summary>
	/// Creates the status presentation for the given editor control.
	/// </summary>
	/// <param name="editorControl">The editor control, or <c>null</c> when no editor is active.</param>
	/// <param name="showSyntaxPreviewWhenEmpty">Whether the syntax preview is shown when no editor is active.</param>
	/// <param name="text">The localized status text.</param>
	/// <returns>The status presentation.</returns>
	public TextEditorStatusPresentation Create(
		IEditorControl? editorControl,
		bool showSyntaxPreviewWhenEmpty,
		TextEditorStatusText text)
	{
		ArgumentNullException.ThrowIfNull(text);

		bool showSyntaxPreview = editorControl is ISyntaxPreviewSource || showSyntaxPreviewWhenEmpty;

		if (editorControl is null)
			return TextEditorStatusPresentation.Empty(showSyntaxPreview);

		return new TextEditorStatusPresentation(
			HasEditor: true,
			RowLabelText: BuildRowLabel(editorControl, text),
			ColumnLabelText: string.Format(text.ColumnFormatText, editorControl.CurrentColumn),
			SelectionLabelText: string.Format(text.SelectionFormatText, editorControl.SelectionLength),
			ZoomLabelText: string.Format(text.ZoomFormatText, editorControl.Zoom),
			CanResetZoom: editorControl.Zoom != 100,
			ResetZoomToolTipText: editorControl.Zoom != 100 ? text.ResetZoomToolTipText : string.Empty,
			ShowSyntaxPreview: showSyntaxPreview,
			SyntaxPreview: editorControl is ISyntaxPreviewSource syntaxPreviewSource
				? syntaxPreviewSource.GetSyntaxPreview()
				: null);
	}

	private static string BuildRowLabel(IEditorControl editorControl, TextEditorStatusText text)
	{
		if (editorControl.EditorType == EditorType.Strings)
			return string.Format(text.RowFormatText, editorControl.CurrentRow);

		if (editorControl is TextEditorBase)
			return string.Format(text.LineFormatText, editorControl.CurrentRow);

		return string.Empty;
	}
}
