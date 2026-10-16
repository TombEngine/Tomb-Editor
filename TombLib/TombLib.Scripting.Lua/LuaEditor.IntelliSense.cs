using ICSharpCode.AvalonEdit.Document;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using TombLib.Scripting.Lua.Parsing;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	private Window? _hostWindow;
	private bool _luaResourcesCleaned;
	private bool _luaControllersDisposed;

	private void BindLuaIntelliSenseEvents()
	{
		Document.Changed += LuaEditor_DocumentChanged;
		IsKeyboardFocusWithinChanged += LuaEditor_IsKeyboardFocusWithinChanged;
		Loaded += LuaEditor_Loaded;
		AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(TextEditor_PreviewMouseDown), true);
		Unloaded += LuaEditor_Unloaded;
	}

	private void LuaEditor_Loaded(object? sender, RoutedEventArgs e)
	{
		_luaResourcesCleaned = false;
		AttachHostWindowHandlers();
	}

	private void LuaEditor_DocumentChanged(object? sender, DocumentChangeEventArgs e)
		=> ClearDiagnostics();

	/// <inheritdoc/>
	protected override void OnLanguageTextChanged(EventArgs e)
	{
		Interlocked.Increment(ref _editorDocumentVersion);
		RebaseOpenCompletionItems();
	}

	private void LuaEditor_IsKeyboardFocusWithinChanged(object? sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue is bool hasKeyboardFocus && !hasKeyboardFocus)
		{
			CloseCompletionWindow();
			DismissTransientToolTips();
		}
	}

	private void AttachHostWindowHandlers()
	{
		Window? window = Window.GetWindow(this);

		if (window == _hostWindow)
			return;

		if (_hostWindow is not null)
			_hostWindow.Deactivated -= HostWindow_Deactivated;

		_hostWindow = window;

		if (_hostWindow is not null)
			_hostWindow.Deactivated += HostWindow_Deactivated;
	}

	private void HostWindow_Deactivated(object? sender, EventArgs e)
	{
		CloseCompletionWindow();
		DismissTransientToolTips();
	}

	private void LuaEditor_Unloaded(object? sender, RoutedEventArgs e)
		=> CleanupLuaResources();

	/// <inheritdoc/>
	protected override void DisposeEditorResources()
	{
		CleanupLuaResources();
		DisposeLuaControllers();
	}

	private void CleanupLuaResources()
	{
		if (_luaResourcesCleaned)
			return;

		_luaResourcesCleaned = true;

		_textMateHighlighting?.Dispose();
		_textMateHighlighting = null;
		InvalidateAsyncEditorRequests();

		CancelPendingCompletionRequest();
		_hoverController.CancelInFlightRequest();

		CompletionController.CancelTooltipUpdate();

		_definitionNavigationController.CancelPendingRequest();
		CloseCompletionWindow();

		DetachHostWindowHandlers();

		DismissTransientToolTips();
		ClearDiagnostics();
		ClearSemanticTokens();

		IntelliSenseProvider?.CloseDocument(FilePath);
	}

	private void DisposeLuaControllers()
	{
		if (_luaControllersDisposed)
			return;

		_luaControllersDisposed = true;
		_signatureHelpController.Dispose();
		_hoverController.Dispose();
	}

	private void DetachHostWindowHandlers()
	{
		if (_hostWindow is not null)
			_hostWindow.Deactivated -= HostWindow_Deactivated;

		_hostWindow = null;
	}

	/// <inheritdoc/>
	protected override void OnLanguageTextEntering(TextCompositionEventArgs e)
	{
		if (!CompletionEnabled || !IsIntelliSenseAvailable())
			return;

		if (e.Text == " " && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
		{
			e.Handled = true;
			CancelPendingCompletionRequest();

			// The completion request is asynchronous and exception-safe; the hook stays synchronous.
			if (LuaEditorInteractionRules.IsValidManualCompletionContext(Document, CaretOffset))
				_ = RequestCompletionAsync(CaretOffset, null);
		}
	}

	/// <inheritdoc/>
	protected override void OnLanguageTextEntered(TextCompositionEventArgs e)
	{
		if (!IsIntelliSenseAvailable())
			return;

		if (e.Text == "(" || e.Text == ",")
		{
			CancelPendingCompletionRequest();
			CloseCompletionWindow();

			if (!SignatureHelpPopupsEnabled)
			{
				DismissSignatureHelp();
				return;
			}

			_signatureHelpController.CancelScheduledRefresh();

			// The signature help request is asynchronous and exception-safe; the hook stays synchronous.
			_ = RequestSignatureHelpAsync(CaretOffset);
			return;
		}

		if (e.Text == ")")
		{
			CancelPendingCompletionRequest();
			CloseCompletionWindow();
			DismissSignatureHelp();
			return;
		}

		if (IsCompletionWindowOpen)
		{
			CancelPendingCompletionRequest();

			if (!ShouldKeepCompletionWindowOpen(e.Text))
			{
				CloseCompletionWindow();
			}
			else
			{
				ScheduleCloseIfEmpty();
				return;
			}
		}

		if (SignatureHelpPopupsEnabled && ShouldRefreshSignatureHelpAfterTextInput(e.Text))
			ScheduleSignatureHelpRefresh();

		if (!CompletionEnabled)
			return;

		if (TryGetCompletionTrigger(e.Text, out char? triggerCharacter)
			&& LuaEditorInteractionRules.IsValidCompletionContext(Document, CaretOffset, triggerCharacter))
		{
			if (triggerCharacter is null)
			{
				ScheduleCompletionRequest();
			}
			else
			{
				CancelPendingCompletionRequest();

				// The completion request is asynchronous and exception-safe; the hook stays synchronous.
				_ = RequestCompletionAsync(CaretOffset, triggerCharacter);
			}
		}
	}

	[MemberNotNullWhen(true, nameof(IntelliSenseProvider))]
	private bool IsIntelliSenseAvailable()
		=> IntelliSenseEnabled && IntelliSenseProvider?.IsAvailable == true && !string.IsNullOrWhiteSpace(FilePath);

	private static bool TryGetCompletionTrigger(string? inputText, out char? triggerCharacter)
	{
		triggerCharacter = null;

		if (string.IsNullOrEmpty(inputText) || inputText.Length != 1)
			return false;

		char typedChar = inputText[0];

		if (typedChar == '.' || typedChar == ':')
		{
			triggerCharacter = typedChar;
			return true;
		}

		return LuaLineParser.IsIdentifierTriggerCharacter(typedChar);
	}

	private static bool ShouldKeepCompletionWindowOpen(string? inputText)
		=> inputText?.Length == 1 && LuaLineParser.IsIdentifierCharacter(inputText[0]);

	private void DismissTransientToolTips()
	{
		CancelPendingCompletionRequest();
		_hoverController.CancelInFlightRequest();
		_hoverController.InvalidateRequests();
		DismissSignatureHelp();
		CloseDefinitionToolTip(true);
	}

	private (int Line, int Column) GetPositionFromOffset(int offset)
	{
		int safeOffset = Math.Max(0, Math.Min(offset, Document.TextLength));
		TextLocation location = Document.GetLocation(safeOffset);

		return (location.Line - 1, location.Column - 1);
	}

	private static void LogEditorFailure(string area, Exception exception)
		=> Log.Warn(exception, "Lua editor operation '{Area}' failed.", area);

	/// <inheritdoc/>
	protected override void OnAutoClosingElementSkipped(string element)
	{
		if (!ShouldDismissSignatureHelpOnAutoClosingSkip(element, ParenthesesClosingString))
			return;

		CancelPendingCompletionRequest();
		CloseCompletionWindow();
		DismissSignatureHelp();
	}

	private void InvalidateAsyncEditorRequests()
	{
		// The session generation is advanced by the base at load, replace, rename, and disposal
		// boundaries; this method only invalidates the language-service controllers. Canceling the
		// in-flight provider calls keeps them from running on when the results are already stale.
		CompletionController.Requests.CancelInFlightRequest();
		CompletionController.Requests.InvalidateRequests();
		_hoverController.CancelInFlightRequest();
		_hoverController.InvalidateRequests();
		_signatureHelpController.InvalidateRequests();
		_definitionNavigationController.InvalidateRequests();
	}

	/// <inheritdoc/>
	protected override void OnLoadReset()
	{
		base.OnLoadReset();
		InvalidateAsyncEditorRequests();
		DismissTransientToolTips();
		_definitionNavigationController.CancelPendingRequest();
	}

	private bool IsAsyncEditorResultCurrent(CancellationToken cancellationToken,
		long requestToken,
		long currentRequestToken,
		int requestDocumentVersion,
		int requestGeneration)
	{
		return IsAsyncEditorResultCurrent(
			cancellationToken.IsCancellationRequested,
			requestToken,
			currentRequestToken,
			requestDocumentVersion,
			_editorDocumentVersion,
			requestGeneration,
			SessionGeneration,
			IsLoaded,
			IsIntelliSenseAvailable());
	}

	private static bool IsAsyncEditorResultCurrent(bool isCancellationRequested,
		long requestToken,
		long currentRequestToken,
		int requestDocumentVersion,
		int currentDocumentVersion,
		int requestGeneration,
		int currentGeneration,
		bool isEditorLoaded,
		bool isIntelliSenseAvailable)
	{
		return !isCancellationRequested
			&& requestToken == currentRequestToken
			&& requestDocumentVersion == currentDocumentVersion
			&& requestGeneration == currentGeneration
			&& isEditorLoaded
			&& isIntelliSenseAvailable;
	}

	private bool ShouldRefreshSignatureHelpAfterTextInput(string? inputText)
		=> ShouldRefreshSignatureHelpAfterTextInput(inputText, _signatureHelpController.IsPresentationVisibleOrRequestPending);

	private static bool ShouldRefreshSignatureHelpAfterTextInput(string? inputText, bool isPresentationVisibleOrRequestPending)
		=> isPresentationVisibleOrRequestPending && inputText?.Length == 1;

	private static bool ShouldDismissSignatureHelpOnAutoClosingSkip(string element, string parenthesesClosingString)
		=> string.Equals(element, parenthesesClosingString, StringComparison.Ordinal);
}
