using Nickelony.IDEKit.IntelliSense.Diagnostics;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Nickelony.IDEKit.Core.Infrastructure;


/// <summary>
/// Runs error detection in the background, debounced by an idle timer, and publishes the result
/// through <see cref="RunWorkerCompleted"/>. Detection runs are single-flight: at most one run is
/// active, and content captured while a run is active is coalesced so only the latest content is
/// checked when the active run completes. A newer request cancels the in-flight run's cancellation
/// token, and a result is never published after it has been superseded or the worker disposed.
/// Each admitted run is classified with a <see cref="TextEditorRequestOutcome"/>: completed and
/// failed runs surface through the completion event, while cancelled, superseded, and stale runs
/// never publish. When a suppression provider is supplied, checks are not started while processing
/// is suppressed. The worker is created on and confined to the UI thread; the full-document
/// provider call runs on the thread pool because error detection is CPU-bound and the provider
/// contract explicitly permits background execution. New detection work cannot be admitted after
/// the worker is disposed: <see cref="RunErrorCheckOnIdle"/>, <see cref="RunErrorCheck"/>, and
/// <see cref="Reset"/> throw <see cref="ObjectDisposedException"/> once disposed, while work
/// already admitted before disposal completes with its normal cancellation outcome.
/// </summary>
public sealed class ErrorDetectionWorker : IDisposable
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	// Properties

	/// <summary>
	/// Gets whether a detection run is currently in progress.
	/// </summary>
	public bool IsBusy => _isBusy;

	/// <summary>
	/// Gets the outcome of the most recently completed detection run.
	/// </summary>
	internal TextEditorRequestOutcome LastRequestOutcome { get; private set; }

	/// <summary>
	/// Gets or sets the idle debounce interval before a queued check runs.
	/// </summary>
	public TimeSpan IdleDelayInterval
	{
		get => _errorUpdateTimer.Interval;
		set => _errorUpdateTimer.Interval = value;
	}

	/// <summary>
	/// Gets the engine version used for error detection.
	/// </summary>
	public Version EngineVersion { get; }

	// Fields

	private readonly ITextDiagnosticsProvider? _diagnosticsProvider;
	private readonly Dispatcher _dispatcher;
	private readonly DispatcherTimer _errorUpdateTimer = new();
	private readonly Func<bool>? _suppressedProvider;
	private readonly Func<int>? _sessionGenerationProvider;
	private readonly Func<string?>? _logicalDocumentIdProvider;
	private readonly RequestTokenSource _requestTokens = new();

	private CancellationTokenSource? _requestCancellation;
	private volatile bool _isBusy;
	private string _editorContent = string.Empty;
	private string? _pendingContent;
	private bool _hasPendingContent;
	private bool _isDisposed;

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="ErrorDetectionWorker"/> class on the current dispatcher thread.
	/// </summary>
	/// <param name="diagnosticsProvider">The diagnostics provider used to detect errors (optional).</param>
	/// <param name="engineVersion">The engine version used for error detection.</param>
	/// <param name="idleDelayInterval">The idle debounce interval.</param>
	/// <param name="suppressedProvider">The callback that reports whether editor processing is currently suppressed (optional).</param>
	/// <param name="sessionGenerationProvider">The callback that reports the editor's current session generation (optional).</param>
	/// <param name="logicalDocumentIdProvider">The callback that reports the editor's current logical document identity (optional).</param>
	public ErrorDetectionWorker(
		ITextDiagnosticsProvider? diagnosticsProvider,
		Version engineVersion,
		TimeSpan idleDelayInterval,
		Func<bool>? suppressedProvider = null,
		Func<int>? sessionGenerationProvider = null,
		Func<string?>? logicalDocumentIdProvider = null)
	{
		ArgumentNullException.ThrowIfNull(engineVersion);

		_diagnosticsProvider = diagnosticsProvider;
		_dispatcher = Dispatcher.CurrentDispatcher;
		EngineVersion = engineVersion;
		IdleDelayInterval = idleDelayInterval;
		_suppressedProvider = suppressedProvider;
		_sessionGenerationProvider = sessionGenerationProvider;
		_logicalDocumentIdProvider = logicalDocumentIdProvider;

		_errorUpdateTimer.Tick += ErrorUpdateTimer_Tick;
	}

	// Events

	/// <summary>
	/// Raised when a detection run completes with the resulting diagnostics.
	/// </summary>
	public event RunWorkerCompletedEventHandler? RunWorkerCompleted;

	// Public methods

	/// <summary>
	/// Schedules a detection run after the idle debounce interval. No-op while processing is suppressed.
	/// </summary>
	/// <param name="editorContent">The editor content to check.</param>
	/// <exception cref="ObjectDisposedException">The worker has been disposed.</exception>
	public void RunErrorCheckOnIdle(string? editorContent)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		if (IsProcessingSuppressed())
			return;

		if (_errorUpdateTimer.IsEnabled)
			_errorUpdateTimer.Stop();

		_editorContent = editorContent ?? string.Empty;
		_errorUpdateTimer.Start();
	}

	/// <summary>
	/// Runs a detection check now with the given content, coalescing to the latest content when a
	/// run is already in progress. No-op while processing is suppressed.
	/// </summary>
	/// <param name="editorContent">The editor content to check.</param>
	/// <exception cref="ObjectDisposedException">The worker has been disposed.</exception>
	public void RunErrorCheck(string? editorContent)
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		if (IsProcessingSuppressed())
			return;

		if (_diagnosticsProvider is null)
			return;

		_editorContent = editorContent ?? string.Empty;

		// Single-flight policy: an active run is never overlapped; the latest content is retained
		// and checked when the active run completes.
		if (_isBusy)
		{
			_pendingContent = _editorContent;
			_hasPendingContent = true;
			return;
		}

		StartErrorCheck(_editorContent);
	}

	/// <summary>
	/// Stops queued diagnostics work and invalidates any in-flight result without disposing the worker.
	/// </summary>
	/// <exception cref="ObjectDisposedException">The worker has been disposed.</exception>
	public void Reset()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		_errorUpdateTimer.Stop();
		_hasPendingContent = false;
		_pendingContent = null;
		_requestCancellation?.Cancel();
		_requestTokens.Invalidate();
	}

	private void ErrorUpdateTimer_Tick(object? sender, EventArgs e)
	{
		_errorUpdateTimer.Stop();

		// Do not start a check when processing became suppressed while the timer was pending.
		if (IsProcessingSuppressed())
			return;

		RunErrorCheck(_editorContent);
	}

	private bool IsProcessingSuppressed()
		=> _suppressedProvider?.Invoke() ?? false;

	// Private methods

	private void StartErrorCheck(string editorContent)
	{
		_requestCancellation?.Cancel();
		_requestCancellation = new CancellationTokenSource();

		int requestId = _requestTokens.Begin();
		var requestIdentity = new TextEditorRequestIdentity(
			LogicalDocumentId: _logicalDocumentIdProvider?.Invoke(),
			DocumentVersion: 0,
			SessionGeneration: _sessionGenerationProvider?.Invoke() ?? 0);
		_isBusy = true;

		_ = RunErrorCheckCoreAsync(editorContent, requestId, requestIdentity, _requestCancellation.Token);
	}

	private async Task RunErrorCheckCoreAsync(string editorContent, int requestId, TextEditorRequestIdentity requestIdentity, CancellationToken cancellationToken)
	{
		Exception? error = null;
		IReadOnlyList<TextEditorDiagnostic> result = [];

		try
		{
			// Full-document error detection is CPU-bound and the provider contract
			// (ITextDiagnosticsProvider) explicitly permits background execution, so the provider
			// runs on the thread pool. Cancellation is cooperative at the run boundary: the token
			// prevents a superseded run from starting and marks a run cancelled while in flight.
			result = await Task.Run(() => GetDiagnostics(editorContent), cancellationToken).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Cancelled by a newer request, reset, or disposal; the owner owns completion.
			await _dispatcher.InvokeAsync(() => CompleteCanceledRequest(requestId));
			return;
		}
		catch (Exception ex)
		{
			Log.Warn(ex, "Error detection failed for request '{RequestId}'.", requestId);
			error = ex;
		}

		await _dispatcher.InvokeAsync(() => CompleteRequest(requestId, requestIdentity, result, error));
	}

	private IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string editorContent)
	{
		ITextDiagnosticsProvider? diagnosticsProvider = _diagnosticsProvider;

		return diagnosticsProvider is null
			? []
			: diagnosticsProvider.GetDiagnostics(new TextDiagnosticsRequest(editorContent, EngineVersion));
	}

	private void CompleteRequest(int requestId, TextEditorRequestIdentity requestIdentity, object result, Exception? error)
	{
		if (_isDisposed)
			return;

		// A newer request or an owner invalidation replaced this run before its work completed; the
		// completed result is dropped rather than published.
		if (!_requestTokens.IsCurrent(requestId))
		{
			LastRequestOutcome = TextEditorRequestOutcome.Superseded;
			CompleteInvalidatedRequest();
			return;
		}

		// The run is still the latest request, but the operation ownership it was admitted under has
		// ended: the session generation advanced or the logical document identity changed (for
		// example a rename). Pending content captured for the prior generation is dropped too.
		if (!IsRequestIdentityCurrent(requestIdentity))
		{
			LastRequestOutcome = TextEditorRequestOutcome.Stale;
			_isBusy = false;
			_hasPendingContent = false;
			_pendingContent = null;
			return;
		}

		LastRequestOutcome = error is null ? TextEditorRequestOutcome.Completed : TextEditorRequestOutcome.Failed;
		_isBusy = false;
		RunWorkerCompleted?.Invoke(this, new RunWorkerCompletedEventArgs(result, error, false));

		// Run the coalesced latest content after the active run, unless processing became suppressed
		// while the run was in flight.
		if (_hasPendingContent && !IsProcessingSuppressed())
			RunPendingCheck();
	}

	private void CompleteCanceledRequest(int requestId)
	{
		if (_isDisposed || _requestTokens.IsCurrent(requestId))
			return;

		LastRequestOutcome = TextEditorRequestOutcome.Cancelled;
		CompleteInvalidatedRequest();
	}

	private void CompleteInvalidatedRequest()
	{
		_isBusy = false;

		if (_hasPendingContent && !IsProcessingSuppressed())
			RunPendingCheck();
	}

	private void RunPendingCheck()
	{
		_hasPendingContent = false;
		string? content = _pendingContent;
		_pendingContent = null;
		StartErrorCheck(content ?? string.Empty);
	}

	private bool IsRequestIdentityCurrent(TextEditorRequestIdentity requestIdentity)
	{
		if (_sessionGenerationProvider is not null && requestIdentity.SessionGeneration != _sessionGenerationProvider())
			return false;

		if (_logicalDocumentIdProvider is not null
			&& !string.Equals(requestIdentity.LogicalDocumentId, _logicalDocumentIdProvider(), StringComparison.Ordinal))
			return false;

		return true;
	}

	// IDisposable

	/// <summary>
	/// Stops error detection and cancels any in-flight run so no completion callback fires afterwards.
	/// </summary>
	public void Dispose()
	{
		_isDisposed = true;
		_errorUpdateTimer.Stop();
		_errorUpdateTimer.Tick -= ErrorUpdateTimer_Tick;
		_isBusy = false;
		_hasPendingContent = false;
		_pendingContent = null;
		_requestCancellation?.Cancel();
		_requestTokens.Invalidate();
	}
}
