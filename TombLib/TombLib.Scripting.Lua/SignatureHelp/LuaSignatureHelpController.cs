using Nickelony.IDEKit.AvalonEdit.LanguageFeatures.Signatures;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Signatures;
using System;
using System.Collections.Generic;
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

	private static TextBlock BuildSignatureBlock(TextSignatureHelp signatureInfo, LuaThemeBrushSet brushSet)
	{
		var textBlock = new TextBlock
		{
			FontFamily = new("Consolas"),
			FontSize = Math.Max(SystemFonts.MessageFontSize + 1.0, SignaturePopupFontSize),
			TextWrapping = TextWrapping.Wrap,
			Foreground = brushSet.SignatureForeground
		};

		TextSignatureInformation activeSignature = signatureInfo.ActiveSignature;
		string label = activeSignature.Label;

		if (activeSignature.Parameters.Count == 0
			|| !TryGetActiveParameterRange(label, activeSignature.Parameters, signatureInfo.ActiveParameterIndex, out int activeStart, out int activeEnd))
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

	private static bool TryGetActiveParameterRange(
		string label,
		IReadOnlyList<TextSignatureParameterInfo> parameters,
		int? activeParameterIndex,
		out int activeStart,
		out int activeEnd)
	{
		activeStart = 0;
		activeEnd = 0;

		if (activeParameterIndex is not int activeIndex)
			return false;

		string activeLabel = parameters[activeIndex].Label;

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
		private bool _disposed;

		internal LuaSignatureHelpController(LuaEditor editor)
		{
			_editor = editor;
			_popupPresenter = new(editor, editor.AttachHostWindowHandlers);
			_controller = new(
				new TextSignatureHelpControllerHooks
				{
					GetCurrentCaretOffset = () => _editor.CaretOffset,
					RequestSignatureHelpAsync = RequestSignatureHelpAsync,
					ShowSignatureHelp = ShowToolTip,
					DismissSignatureHelp = DismissPopup
				});
		}

		internal bool IsVisible => _controller.CurrentPresentation.IsVisible;

		internal bool IsPresentationVisibleOrRequestPending => _controller.CurrentPresentation.IsPresentationVisibleOrRequestPending;

		internal void Dismiss() => _controller.Dismiss();

		internal Task RequestAsync(int offset) => _controller.RequestAsync(offset);

		internal void ScheduleRefresh() => _controller.ScheduleRefresh();

		internal void CancelScheduledRefresh() => _controller.CancelScheduledRefresh();

		internal void InvalidateRequests()
		{
			_controller.CancelInFlightRequest();
			_controller.InvalidateRequests();
		}

		internal void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_controller.Dispose();
			_popupPresenter.Dispose();
		}

		private void DismissPopup() => _popupPresenter.Close();

		private void ShowToolTip(TextSignatureHelp signatureInfo)
			=> _popupPresenter.Show(contentMaxWidth => CreatePanel(signatureInfo, contentMaxWidth));

		private StackPanel CreatePanel(TextSignatureHelp signatureInfo, double contentMaxWidth)
		{
			LuaThemeBrushSet brushSet = _editor.GetThemeBrushSet();
			var panel = new StackPanel { MaxWidth = contentMaxWidth };
			panel.Children.Add(BuildSignatureBlock(signatureInfo, brushSet));

			TextSignatureInformation activeSignature = signatureInfo.ActiveSignature;
			string? documentation = BacktickFenceTextNormalizer.NormalizeForPlainText(activeSignature.Documentation, Environment.NewLine);

			if (documentation is not null)
				panel.Children.Add(CreateSignatureDocumentationBlock(documentation, brushSet));

			if (signatureInfo.ActiveParameterIndex is int activeParameterIndex
				&& activeParameterIndex >= 0
				&& activeParameterIndex < activeSignature.Parameters.Count)
			{
				TextSignatureParameterInfo activeParameter = activeSignature.Parameters[activeParameterIndex];
				string? parameterDocumentation = BacktickFenceTextNormalizer.NormalizeForPlainText(activeParameter.Documentation, Environment.NewLine);

				if (parameterDocumentation is not null)
					panel.Children.Add(CreateSignatureDocumentationBlock(activeParameter.Label + ": " + parameterDocumentation, brushSet));
			}

			return panel;
		}

		private async Task<TextSignatureHelp?> RequestSignatureHelpAsync(
			int offset,
			TextSignatureHelpContext context,
			CancellationToken cancellationToken)
		{
			// The shared controller performs the authoritative request-token check after the await, so only
			// the cancellation token needs to reach the provider. The document-version and request-generation
			// checks below additionally drop results computed for stale document state.
			if (!_editor.IsIntelliSenseAvailable())
				return null;

			var intelliSenseProvider = _editor.IntelliSenseProvider;

			if (intelliSenseProvider is null)
				return null;

			int requestDocumentVersion = _editor._editorDocumentVersion;
			int requestGeneration = _editor.SessionGeneration;

			try
			{
				(int line, int column) = _editor.GetPositionFromOffset(offset);

				TextSignatureHelp? signatureInfo = await intelliSenseProvider
					.GetSignatureHelpAsync(new LanguageServerSignatureHelpRequest(_editor.FilePath, _editor.Text, new TextPosition(line, column)), cancellationToken: cancellationToken)
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
