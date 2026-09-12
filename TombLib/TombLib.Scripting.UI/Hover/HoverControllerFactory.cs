using Nickelony.IDEKit.AvalonEdit.IntelliSense.Hover;
using Nickelony.IDEKit.AvalonEdit.IntelliSense.Presentation;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Hover;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Rendering;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Hover;

/// <summary>
/// Creates <see cref="TextHoverController"/> instances with standard tooltip presentation wiring,
/// eliminating boilerplate across language-specific editor projects.
/// </summary>
public static class HoverControllerFactory
{
	/// <summary>
	/// Creates a <see cref="TextHoverController"/> wired with the standard hover and combined tooltip presentation.
	/// </summary>
	/// <param name="editor">The text editor that owns the controller.</param>
	/// <param name="buildRequestState">Produces the hover request state for a given offset.</param>
	/// <param name="requestHoverAsync">Resolves hover information asynchronously for a given offset.</param>
	/// <param name="applyHoverState">Optional callback invoked with the final presentation state after each hover resolution.</param>
	public static TextHoverController Create(
		TextEditorBase editor,
		Func<int, TextHoverRequestState> buildRequestState,
		Func<int, CancellationToken, Task<TextHoverInfo?>> requestHoverAsync,
		Action<TextHoverPresentationState>? applyHoverState = null)
	{
		return new TextHoverController(
			owner: editor,
			getOffsetFromPoint: editor.GetOffsetFromPoint,
			buildRequestState: buildRequestState,
			requestHoverAsync: requestHoverAsync,
			getCurrentRequestOffset: hoveredOffset => hoveredOffset,
			showDiagnosticToolTip: editor.ShowDiagnosticToolTip,
			showHoverToolTip: hoverInfo => ShowStandardHoverToolTip(editor, hoverInfo),
			showCombinedToolTip: (hoverInfo, diagnosticInfo) => ShowStandardCombinedToolTip(editor, hoverInfo, diagnosticInfo),
			applyHoverState: applyHoverState);
	}

	/// <summary>
	/// Shows a standard hover tooltip populated from <see cref="TextHoverToolTipContentFactory.CreateHoverContent"/>.
	/// </summary>
	public static void ShowStandardHoverToolTip(TextEditorBase editor, TextHoverInfo hoverInfo)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(hoverInfo);

		editor.ShowToolTip(
			content: TextHoverToolTipContentFactory.CreateHoverContent(
				hoverInfo: hoverInfo,
				foreground: TextEditorColorPalette.ToolTipForeground,
				background: TextEditorColorPalette.ToolTipBackground),

			border: TextEditorColorPalette.ToolTipBorder,
			background: TextEditorColorPalette.ToolTipBackground);
	}

	/// <summary>
	/// Shows a combined hover + diagnostic tooltip populated from <see cref="TextHoverToolTipContentFactory.CreateCombinedContent"/>.
	/// </summary>
	public static void ShowStandardCombinedToolTip(
		TextEditorBase editor,
		TextHoverInfo hoverInfo,
		TextEditorDiagnostic diagnosticInfo)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(hoverInfo);
		ArgumentNullException.ThrowIfNull(diagnosticInfo);

		editor.ShowToolTip(
			content: TextHoverToolTipContentFactory.CreateCombinedContent(
				hoverInfo: hoverInfo,
				diagnosticInfo: diagnosticInfo,
				foreground: TextEditorColorPalette.ToolTipForeground,
				background: TextEditorColorPalette.ToolTipBackground,
				maxWidth: ToolTipDefaults.TextMaxWidth,
				fontSize: ToolTipDefaults.TextFontSize,
				getDiagnosticColors: GetDiagnosticColors),

			border: TextEditorColorPalette.ToolTipBorder,
			background: TextEditorColorPalette.ToolTipBackground);
	}

	/// <summary>
	/// Resolves diagnostic tooltip border and background colors for the given severity.
	/// </summary>
	public static (SolidColorBrush Border, SolidColorBrush Background) GetDiagnosticColors(TextEditorDiagnosticSeverity severity)
	{
		TextEditorToolTipHelper.GetDiagnosticToolTipColors(severity, out SolidColorBrush border, out SolidColorBrush background);
		return (border, background);
	}
}
