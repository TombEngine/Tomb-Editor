using Nickelony.IDEKit.AvalonEdit.IntelliSense.Completion;
using Nickelony.IDEKit.AvalonEdit.IntelliSense.Hover;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Hover;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TombLib.Scripting.UI.Hover;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	/// <inheritdoc/>
	protected override async Task HandleMouseHover(MouseEventArgs e)
		=> await _hoverController.HandleMouseHoverAsync(e).ConfigureAwait(true);

	/// <summary>
	/// Owns Lua hover request state, request eligibility checks, and hover-versus-diagnostic tooltip presentation.
	/// </summary>
	private sealed class LuaHoverController
	{
		private readonly LuaEditor _editor;
		private readonly TextHoverController _controller;

		internal LuaHoverController(LuaEditor editor)
		{
			_editor = editor;
			_controller = new(
				owner: editor,
				getOffsetFromPoint: editor.GetOffsetFromPoint,
				buildRequestState: BuildRequestState,
				requestHoverAsync: RequestAsync,
				getCurrentRequestOffset: TryGetCurrentRequestOffset,
				showDiagnosticToolTip: editor.ShowDiagnosticToolTip,
				showHoverToolTip: hoverInfo => HoverControllerFactory.ShowStandardHoverToolTip(_editor, hoverInfo),
				showCombinedToolTip: (hoverInfo, diagnosticInfo) =>
					HoverControllerFactory.ShowStandardCombinedToolTip(_editor, hoverInfo, diagnosticInfo),
				applyHoverState: _ => { },
				sessionGenerationProvider: () => _editor.SessionGeneration);
		}

		internal Task HandleMouseHoverAsync(MouseEventArgs e) => _controller.HandleMouseHoverAsync(e);

		internal void CancelPendingRequest() => _controller.CancelPendingRequest();

		internal void InvalidateRequests() => _controller.InvalidateRequests();

		internal void Dispose() => _controller.Dispose();

		private TextHoverRequestState BuildRequestState(int hoveredOffset)
		{
			_editor.TryGetDiagnosticInfo(hoveredOffset, out TextEditorDiagnostic? diagnosticInfo, allowLineFallback: false);
			bool canShowToolTip = TextPopupInteractionRules.CanShowHover(_editor.IsCompletionWindowOpen, _editor._signatureHelpController.IsVisible);
			int hoverOffset = 0;
			bool shouldRequestHover = false;

			if (_editor.IsIntelliSenseAvailable()
				&& canShowToolTip
				&& LuaEditorInteractionRules.TryGetHoverOffset(_editor.Document, hoveredOffset, out hoverOffset))
			{
				shouldRequestHover = !string.IsNullOrWhiteSpace(_editor.GetWordFromOffset(hoverOffset));
			}

			return new TextHoverRequestState(
				ShouldRequestHover: shouldRequestHover,
				RequestOffset: shouldRequestHover ? hoverOffset : 0,
				CanShowToolTip: canShowToolTip,
				CanShowDiagnosticFallback: canShowToolTip,
				DiagnosticInfo: diagnosticInfo);
		}

		private int? TryGetCurrentRequestOffset(int hoveredOffset)
		{
			if (!TextPopupInteractionRules.CanShowHover(_editor.IsCompletionWindowOpen, _editor._signatureHelpController.IsVisible))
				return null;

			if (!LuaEditorInteractionRules.TryGetHoverOffset(_editor.Document, hoveredOffset, out int hoverOffset))
				return null;

			return string.IsNullOrWhiteSpace(_editor.GetWordFromOffset(hoverOffset))
				? null
				: hoverOffset;
		}

		private async Task<TextHoverInfo?> RequestAsync(int offset, CancellationToken cancellationToken)
		{
			if (!_editor.IsIntelliSenseAvailable())
				return null;

			var intelliSenseProvider = _editor.IntelliSenseProvider;

			if (intelliSenseProvider is null)
				return null;

			(int line, int column) = _editor.GetPositionFromOffset(offset);

			return await intelliSenseProvider
				.GetHoverAsync(_editor.FilePath, _editor.Text, line, column, cancellationToken)
				.ConfigureAwait(true);
		}
	}
}
