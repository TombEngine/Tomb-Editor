using Nickelony.IDEKit.AvalonEdit.IntelliSense.Signatures;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Signatures;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.UI.Signatures;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	private const double SignaturePopupFontSize = 14.0;

	private void DismissSignatureHelp() => _signatureHelpController.Dismiss();

	private Task RequestSignatureHelpAsync(int offset) => _signatureHelpController.RequestAsync(offset);

	private void ScheduleSignatureHelpRefresh() => _signatureHelpController.ScheduleRefresh();

	private static TextBlock CreateSignatureDocumentationBlock(string text, LuaThemeBrushSet brushSet) => new()
	{
		Text = text,
		Foreground = brushSet.SignatureParamDocForeground,
		FontFamily = SystemFonts.MessageFontFamily,
		FontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, SignaturePopupFontSize),
		TextWrapping = TextWrapping.Wrap,
		Margin = new(0.0, 4.0, 0.0, 0.0)
	};

	private static TextBlock BuildSignatureBlock(TextSignatureHelpInfo signatureInfo, LuaThemeBrushSet brushSet)
	{
		var textBlock = new TextBlock
		{
			FontFamily = new("Consolas"),
			FontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, SignaturePopupFontSize),
			TextWrapping = TextWrapping.Wrap,
			Foreground = brushSet.SignatureForeground
		};

		string label = signatureInfo.Label;

		if (signatureInfo.Parameters.Count == 0
			|| !TryGetActiveParameterRange(label, signatureInfo, out int activeStart, out int activeEnd))
		{
			textBlock.Text = label;
			return textBlock;
		}

		if (activeStart > 0)
			textBlock.Inlines.Add(new Run(label[..activeStart]));

		textBlock.Inlines.Add(new Run(label[activeStart..activeEnd])
		{
			FontWeight = FontWeights.Bold,
			Foreground = brushSet.SignatureActiveParamForeground
		});

		if (activeEnd < label.Length)
			textBlock.Inlines.Add(new Run(label[activeEnd..]));

		return textBlock;
	}

	private static bool TryGetActiveParameterRange(string label, TextSignatureHelpInfo signatureInfo, out int activeStart, out int activeEnd)
	{
		activeStart = 0;
		activeEnd = 0;

		int activeIndex = Math.Min(signatureInfo.ActiveParameterIndex, signatureInfo.Parameters.Count - 1);

		if (activeIndex < 0)
			return false;

		string activeLabel = signatureInfo.Parameters[activeIndex].Label;

		if (string.IsNullOrEmpty(activeLabel))
			return false;

		int searchStart = label.IndexOf('(');
		searchStart = searchStart < 0 ? 0 : searchStart + 1;

		int matchIndex = label.IndexOf(activeLabel, searchStart, StringComparison.Ordinal);

		if (matchIndex < 0)
			return false;

		activeStart = matchIndex;
		activeEnd = matchIndex + activeLabel.Length;
		return true;
	}

	/// <summary>
	/// Owns signature help popup state, refresh scheduling, and provider request flow for Lua call-site assistance.
	/// </summary>
	private sealed class LuaSignatureHelpController
	{
		private readonly LuaEditor _editor;
		private readonly TextSignatureHelpController _controller;
		private readonly TextSignatureHelpPopupPresenter _popupPresenter;
		private CancellationTokenSource? _requestCancellation;
		private bool _disposed;

		internal LuaSignatureHelpController(LuaEditor editor)
		{
			_editor = editor;
			_popupPresenter = new(editor, editor.AttachHostWindowHandlers);
			_controller = new(
				getCurrentCaretOffset: () => _editor.CaretOffset,
				requestSignatureHelpAsync: RequestSignatureHelpAsync,
				showSignatureHelp: ShowToolTip,
				dismissSignatureHelp: DismissPopup,
				cancelInFlightRequest: CancelInFlightRequest,
				refreshDebounceDelay: TimeSpan.FromMilliseconds(50.0));
		}

		internal bool IsVisible => _controller.CurrentPresentation.IsVisible;

		internal bool IsActiveOrPending => _controller.CurrentPresentation.IsActiveOrPending;

		internal void Dismiss() => _controller.Dismiss();

		internal Task RequestAsync(int offset) => _controller.RequestAsync(offset);

		internal void ScheduleRefresh() => _controller.ScheduleRefresh();

		internal void CancelPendingRefresh() => _controller.CancelPendingRefresh();

		internal void InvalidateRequests()
		{
			CancelInFlightRequest();
			_controller.InvalidateRequests();
		}

		internal void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			CancelInFlightRequest();
			_controller.Dispose();
			_popupPresenter.Dispose();
		}

		private void CancelInFlightRequest()
		{
			if (_requestCancellation is null)
				return;

			_requestCancellation.Cancel();
			_requestCancellation.Dispose();
			_requestCancellation = null;
		}

		private void DismissPopup() => _popupPresenter.Close();

		private void ShowToolTip(TextSignatureHelpInfo signatureInfo)
			=> _popupPresenter.Show(contentMaxWidth => CreatePanel(signatureInfo, contentMaxWidth));

		private StackPanel CreatePanel(TextSignatureHelpInfo signatureInfo, double contentMaxWidth)
		{
			LuaThemeBrushSet brushSet = _editor.GetThemeBrushSet();
			var panel = new StackPanel { MaxWidth = contentMaxWidth };
			panel.Children.Add(BuildSignatureBlock(signatureInfo, brushSet));

			string? documentation = MarkupTextNormalizer.NormalizeForPlainText(signatureInfo.Documentation);

			if (documentation is not null)
				panel.Children.Add(CreateSignatureDocumentationBlock(documentation, brushSet));

			if (signatureInfo.ActiveParameterIndex >= 0 && signatureInfo.ActiveParameterIndex < signatureInfo.Parameters.Count)
			{
				TextSignatureParameterInfo activeParameter = signatureInfo.Parameters[signatureInfo.ActiveParameterIndex];
				string? parameterDocumentation = MarkupTextNormalizer.NormalizeForPlainText(activeParameter.Documentation);

				if (parameterDocumentation is not null)
					panel.Children.Add(CreateSignatureDocumentationBlock(activeParameter.Label + ": " + parameterDocumentation, brushSet));
			}

			return panel;
		}

		private async Task<TextSignatureHelpInfo?> RequestSignatureHelpAsync(int offset, int requestToken)
		{
			// The shared controller performs the authoritative request-token check after the await,
			// so the token parameter is not needed here. The document-version and request-generation
			// checks below additionally drop results computed for stale document state.
			if (!_editor.IsIntelliSenseAvailable())
				return null;

			var intelliSenseProvider = _editor.IntelliSenseProvider;

			if (intelliSenseProvider is null)
				return null;

			// A newer request supersedes the previous one, so its in-flight provider call is
			// cancelled rather than being allowed to complete and then be discarded.
			CancelInFlightRequest();
			_requestCancellation = new();
			CancellationToken cancellationToken = _requestCancellation.Token;
			int requestDocumentVersion = _editor._editorDocumentVersion;
			int requestGeneration = _editor.SessionGeneration;

			try
			{
				(int line, int column) = _editor.GetPositionFromOffset(offset);

				TextSignatureHelpInfo? signatureInfo = await intelliSenseProvider
					.GetSignatureHelpAsync(_editor.FilePath, _editor.Text, line, column, cancellationToken)
					.ConfigureAwait(true);

				return requestDocumentVersion == _editor._editorDocumentVersion
					&& requestGeneration == _editor.SessionGeneration
					? signatureInfo
					: null;
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}
	}
}
