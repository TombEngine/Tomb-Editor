using Nickelony.IDEKit.IntelliSense.Diagnostics;
	using Nickelony.IDEKit.Core.Infrastructure;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using TombLib.Scripting.UI.Bases;
	using TombLib.Scripting.UI.Editors;
namespace TombLib.Scripting.UI.Diagnostics;

/// <summary>
/// Coordinates background diagnostics detection for an editor. The worker is single-flight and
/// coalesces to the latest content; while the editor is in a suppressed processing scope, new
/// checks are not started and completed results are discarded, so diagnostics are never replaced
/// with content captured during the scope. Each admitted run is classified with a
/// <see cref="TextEditorRequestOutcome"/>: completed and failed runs are handled here, while
/// cancelled, superseded, and stale runs never reach the editor.
/// </summary>
public sealed class TextDiagnosticsCoordinator : IDisposable
{
	private static readonly TimeSpan DefaultIdleDelay = TimeSpan.FromMilliseconds(500.0);

	private readonly TextEditorBase _editor;
	private readonly ErrorDetectionWorker _worker;
	private bool _isDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextDiagnosticsCoordinator"/> class.
	/// </summary>
	/// <param name="editor">The editor whose diagnostics are coordinated.</param>
	/// <param name="engineVersion">The engine version used for error detection.</param>
	/// <param name="diagnosticsProvider">The optional diagnostics provider.</param>
	/// <param name="idleDelayInterval">The optional idle debounce interval.</param>
	public TextDiagnosticsCoordinator(
		TextEditorBase editor,
		Version engineVersion,
		ITextDiagnosticsProvider? diagnosticsProvider = null,
		TimeSpan? idleDelayInterval = null)
	{
		ArgumentNullException.ThrowIfNull(editor);

		_editor = editor;
		_worker = new ErrorDetectionWorker(
			diagnosticsProvider,
			engineVersion,
			idleDelayInterval ?? DefaultIdleDelay,
			() => _editor.ProcessingMode == EditorProcessingMode.Suppressed,
			() => _editor.SessionGeneration,
			() => _editor.FilePath);

		_worker.RunWorkerCompleted += ErrorDetectionWorker_RunWorkerCompleted;
	}

	/// <summary>
	/// Gets whether a diagnostics run is currently in progress.
	/// </summary>
	public bool IsBusy => _worker.IsBusy;

	/// <summary>
	/// Stops queued diagnostics work and invalidates any in-flight result without disposing the coordinator.
	/// </summary>
	public void Reset()
	{
		if (_isDisposed)
			return;

		_worker.Reset();
	}

	/// <summary>
	/// Schedules a diagnostics run after the idle debounce interval. No-op while the editor is in a suppressed processing scope.
	/// </summary>
	/// <param name="editorContent">The editor content to check.</param>
	public void RunOnIdle(string? editorContent)
	{
		if (_isDisposed || _editor.ProcessingMode == EditorProcessingMode.Suppressed)
			return;

		_worker.RunErrorCheckOnIdle(editorContent);
	}

	/// <summary>
	/// Runs a diagnostics check now, coalescing to the latest content when a check is already in
	/// progress. No-op while the editor is in a suppressed processing scope.
	/// </summary>
	/// <param name="editorContent">The editor content to check.</param>
	public void RunErrorCheck(string? editorContent)
	{
		if (_isDisposed || _editor.ProcessingMode == EditorProcessingMode.Suppressed)
			return;

		_worker.RunErrorCheck(editorContent);
	}

	// IDisposable

	/// <summary>
	/// Stops background detection and unsubscribes completion callbacks so no diagnostics mutate the editor afterwards.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;
		_worker.RunWorkerCompleted -= ErrorDetectionWorker_RunWorkerCompleted;
		_worker.Dispose();
	}

	private void ErrorDetectionWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
	{
		// Keep the last known diagnostics when the provider failed; the worker already logged the failure.
		// While processing is suppressed, discard the completed result instead of publishing it.
		if (!_isDisposed && _editor.ProcessingMode != EditorProcessingMode.Suppressed && !e.Cancelled && e.Error is null && e.Result is IReadOnlyList<TextEditorDiagnostic> diagnostics)
			_editor.SetDiagnostics(diagnostics);
	}
}
