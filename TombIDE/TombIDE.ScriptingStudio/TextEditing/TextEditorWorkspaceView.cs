#nullable enable

using System;
using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.AvalonEdit.Editing;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.Workspace.Documents;
using TombIDE.ScriptingStudio.Workspace;
using TombLib.Scripting.UI.Editing;
using Nickelony.IDEKit.Core.Editing;

namespace TombIDE.ScriptingStudio.TextEditing;

/// <summary>
/// Projects a workspace document into an AvalonEdit text editor, publishing edits and
/// acknowledging store mutations while keeping the editor's document as the local view.
/// </summary>
internal sealed class TextEditorWorkspaceView : IWorkspaceDocumentDeleteGuardView, ITextEditTarget, ITextEditTargetVersion, IWorkspaceScriptView, IWorkspaceViewPendingEdits
{
	private readonly ICSharpCode.AvalonEdit.TextEditor _editor;
	private readonly IAvalonEditWorkspaceViewHost _host;
	private readonly AvalonEditTextEditTarget _editTarget;
	private readonly string _projectionId = Guid.NewGuid().ToString("N");
	private string _text;
	private string _documentId = string.Empty;
	private WorkspaceDocumentKey? _documentKey;
	private TextFileFormat _fileFormat;
	private long _version;
	private int _suppressionDepth;
	private bool _hasPendingEdits;
	private bool _hasConflict;
	private bool _deleteGuardActive;
	private bool _wasReadOnly;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextEditorWorkspaceView"/> class.
	/// </summary>
	/// <param name="editor">The AvalonEdit editor to project into.</param>
	/// <param name="host">The host adapter exposing the editor's workspace state.</param>
	public TextEditorWorkspaceView(ICSharpCode.AvalonEdit.TextEditor editor, IAvalonEditWorkspaceViewHost host)
	{
		_editor = editor ?? throw new ArgumentNullException(nameof(editor));
		_host = host ?? throw new ArgumentNullException(nameof(host));
		_editTarget = new AvalonEditTextEditTarget(editor);
		_text = editor.Text;
		_host.ActiveEditTarget = this;
		_editor.Document.Changed += EditorDocument_Changed;
	}

	/// <inheritdoc/>
	public event EventHandler<WorkspaceDocumentViewApplyRequestedEventArgs>? ApplyRequested;

	/// <inheritdoc/>
	public string ViewId => _projectionId;

	/// <inheritdoc/>
	public string DocumentId => _documentId;

	/// <inheritdoc/>
	public WorkspaceDocumentKey? DocumentKey => _documentKey;

	/// <inheritdoc/>
	public bool HasPendingEdits => _hasPendingEdits;

	/// <inheritdoc/>
	public bool HasConflict => _hasConflict;

	/// <inheritdoc/>
	public string Text => _text;

	/// <inheritdoc/>
	public long Version => _version;

	/// <inheritdoc/>
	public void Apply(PreparedTextEdits edits)
	{
		ArgumentNullException.ThrowIfNull(edits);
		if (edits.Operations.Count == 0)
			return;

		using (IDisposable suppression = BeginSuppression())
			_editTarget.Apply(edits);

		_host.ProcessContentChange(_text);
		PublishCurrentContent(_text);
	}

	/// <inheritdoc/>
	public WorkspaceDocumentViewOpenResult Open(WorkspaceDocumentSnapshot snapshot)
	{
		ArgumentNullException.ThrowIfNull(snapshot);

		if (_documentKey is not null)
			return new WorkspaceDocumentViewOpenResult(WorkspaceDocumentViewOpenOutcome.AlreadyOpen);

		if (_hasPendingEdits || _hasConflict)
			return new WorkspaceDocumentViewOpenResult(WorkspaceDocumentViewOpenOutcome.Unavailable);

		try
		{
			ApplyAttachSnapshot(snapshot);
			SetViewState(snapshot);
			return new WorkspaceDocumentViewOpenResult(WorkspaceDocumentViewOpenOutcome.Opened);
		}
		catch (Exception exception)
		{
			ClearActiveEditTarget();
			return new WorkspaceDocumentViewOpenResult(
				WorkspaceDocumentViewOpenOutcome.Unavailable,
				new WorkspaceOperationFailure("OpenFailed", exception.Message, exception));
		}
	}

	/// <inheritdoc/>
	public WorkspaceDocumentViewRefreshResult Refresh(WorkspaceDocumentSnapshot snapshot)
	{
		ArgumentNullException.ThrowIfNull(snapshot);

		if (_documentKey is null)
			return new WorkspaceDocumentViewRefreshResult(
				WorkspaceDocumentViewRefreshOutcome.Failed,
				new WorkspaceOperationFailure("ViewNotAttached", "The view is not attached."));

		if (snapshot.DocumentKey != _documentKey
			|| !string.Equals(snapshot.DocumentId, _documentId, StringComparison.Ordinal))
		{
			_hasConflict = true;
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.MarkedStale);
		}

		if (snapshot.Version < _version)
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.MarkedStale);

		if (_hasPendingEdits || _hasConflict)
		{
			_hasConflict = true;
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.MarkedStale);
		}

		try
		{
			ApplyRefreshedSnapshot(snapshot);
			SetViewState(snapshot);
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.Refreshed);
		}
		catch (Exception exception)
		{
			_hasConflict = true;
			return new WorkspaceDocumentViewRefreshResult(
				WorkspaceDocumentViewRefreshOutcome.Failed,
				new WorkspaceOperationFailure("ViewRefreshFailed", exception.Message, exception));
		}
	}

	/// <inheritdoc/>
	public WorkspaceDocumentViewRefreshResult DiscardPendingEdits(WorkspaceDocumentSnapshot snapshot)
	{
		ArgumentNullException.ThrowIfNull(snapshot);

		if (_documentKey is null
			|| snapshot.DocumentKey != _documentKey
			|| !string.Equals(snapshot.DocumentId, _documentId, StringComparison.Ordinal))
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.MarkedStale);

		try
		{
			if (!string.Equals(Text, snapshot.Content, StringComparison.Ordinal))
				_host.ApplyAuthoritativeContent(snapshot.DisplayPath, snapshot.Content);

			_text = snapshot.Content;
			_fileFormat = snapshot.FileFormat;
			_version = snapshot.Version;
			_hasPendingEdits = false;
			_hasConflict = false;
			_host.RecordPersistedContent(snapshot.Content);
			_host.IsContentChanged = snapshot.IsDirty;
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.Refreshed);
		}
		catch (Exception exception)
		{
			_hasConflict = true;
			return new WorkspaceDocumentViewRefreshResult(
				WorkspaceDocumentViewRefreshOutcome.Failed,
				new WorkspaceOperationFailure("ViewDiscardFailed", exception.Message, exception));
		}
	}

	/// <inheritdoc/>
	public WorkspaceDocumentViewIdentityResult AcknowledgeIdentity(WorkspaceDocumentViewIdentityChange change)
	{
		ArgumentNullException.ThrowIfNull(change);

		if (_documentKey is null
			|| _documentKey != change.OldDocumentKey
			|| !string.Equals(_documentId, change.OldDocumentId, StringComparison.Ordinal))
			return new WorkspaceDocumentViewIdentityResult(
				WorkspaceDocumentViewIdentityOutcome.Failed,
				new WorkspaceOperationFailure("ViewIdentityMismatch", "The view is not attached to the expected document identity."));

		try
		{
			_host.FilePath = change.Snapshot.DisplayPath;
			SetViewState(change.Snapshot);
			_hasPendingEdits = false;
			_hasConflict = false;
			return new WorkspaceDocumentViewIdentityResult(WorkspaceDocumentViewIdentityOutcome.Updated);
		}
		catch (Exception exception)
		{
			_hasConflict = true;
			return new WorkspaceDocumentViewIdentityResult(
				WorkspaceDocumentViewIdentityOutcome.Failed,
				new WorkspaceOperationFailure("ViewIdentityUpdateFailed", exception.Message, exception));
		}
	}

	/// <inheritdoc/>
	public WorkspaceDocumentViewDeleteGuardResult ApplyDeleteGuard()
	{
		if (_deleteGuardActive)
			return new WorkspaceDocumentViewDeleteGuardResult(WorkspaceDocumentViewDeleteGuardOutcome.Succeeded);

		try
		{
			_wasReadOnly = _editor.IsReadOnly;
			_editor.IsReadOnly = true;
			_deleteGuardActive = true;
			return new WorkspaceDocumentViewDeleteGuardResult(WorkspaceDocumentViewDeleteGuardOutcome.Succeeded);
		}
		catch (Exception exception)
		{
			return new WorkspaceDocumentViewDeleteGuardResult(
				WorkspaceDocumentViewDeleteGuardOutcome.Failed,
				new WorkspaceOperationFailure("DeleteBarrierUpdateFailed", exception.Message, exception));
		}
	}

	/// <inheritdoc/>
	public WorkspaceDocumentViewDeleteGuardResult ReleaseDeleteGuard()
	{
		if (!_deleteGuardActive)
			return new WorkspaceDocumentViewDeleteGuardResult(WorkspaceDocumentViewDeleteGuardOutcome.Succeeded);

		try
		{
			_editor.IsReadOnly = _wasReadOnly;
			_deleteGuardActive = false;
			return new WorkspaceDocumentViewDeleteGuardResult(WorkspaceDocumentViewDeleteGuardOutcome.Succeeded);
		}
		catch (Exception exception)
		{
			return new WorkspaceDocumentViewDeleteGuardResult(
				WorkspaceDocumentViewDeleteGuardOutcome.Failed,
				new WorkspaceOperationFailure("DeleteBarrierUpdateFailed", exception.Message, exception));
		}
	}

	/// <inheritdoc/>
	public WorkspaceDocumentViewRefreshResult AcknowledgeApply(WorkspaceDocumentMutationResult result)
	{
		ArgumentNullException.ThrowIfNull(result);

		if (_documentKey is null)
			return new WorkspaceDocumentViewRefreshResult(
				WorkspaceDocumentViewRefreshOutcome.Failed,
				new WorkspaceOperationFailure("ViewNotAttached", "The view is not attached."));

		if (result.Outcome is not (WorkspaceDocumentMutationOutcome.Changed or WorkspaceDocumentMutationOutcome.NoChange)
			|| result.Snapshot is null)
		{
			_hasConflict = true;
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.MarkedStale);
		}

		try
		{
			if (result.RequestedIdentity.DocumentKey != _documentKey
				|| !string.Equals(result.RequestedIdentity.DocumentId, _documentId, StringComparison.Ordinal))
			{
				_hasConflict = true;
				return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.MarkedStale);
			}

			if (!string.Equals(Text, result.Snapshot.Content, StringComparison.Ordinal))
				ApplyRefreshedSnapshot(result.Snapshot);

			SetViewState(result.Snapshot);
			_hasPendingEdits = false;
			_hasConflict = false;
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.Refreshed);
		}
		catch (Exception exception)
		{
			_hasConflict = true;
			return new WorkspaceDocumentViewRefreshResult(
				WorkspaceDocumentViewRefreshOutcome.Failed,
				new WorkspaceOperationFailure("ViewAcknowledgeFailed", exception.Message, exception));
		}
	}

	/// <inheritdoc/>
	public void Close()
	{
		if (_deleteGuardActive)
			ReleaseDeleteGuard();

		_editor.Document.Changed -= EditorDocument_Changed;
		ClearActiveEditTarget();

		_documentId = string.Empty;
		_documentKey = null;
		_version = 0;
		_hasPendingEdits = false;
		_hasConflict = false;
	}

	private void EditorDocument_Changed(object? sender, DocumentChangeEventArgs e)
	{
		_text = ApplyDocumentChange(_text, e);

		if (_suppressionDepth > 0 || _documentKey is null)
			return;

		_host.ProcessContentChange(_text);
		PublishCurrentContent(_text);
	}

	private void PublishCurrentContent(string content)
	{
		if (_documentKey is null)
			return;

		_hasPendingEdits = true;
		_host.IsContentChanged = true;
		ApplyRequested?.Invoke(
			this,
			new WorkspaceDocumentViewApplyRequestedEventArgs(
				new WorkspaceDocumentReplaceRequest(
					new(_documentKey.Value, _documentId, _version),
					content,
					_fileFormat)));
	}

	private void ApplyAttachSnapshot(WorkspaceDocumentSnapshot snapshot)
	{
		using IDisposable suppression = BeginSuppression();
		TextWorkspaceEditSelectionState selectionState = TextWorkspaceEditSelectionState.Capture(_editor, _host.FilePath ?? snapshot.DisplayPath);

		if (string.Equals(Text, snapshot.Content, StringComparison.Ordinal))
			_host.ApplyAuthoritativeBaseline(snapshot.DisplayPath, snapshot.Content);
		else
			_host.ApplyAuthoritativeContent(snapshot.DisplayPath, snapshot.Content);

		RestoreSelectionState(selectionState);
		_text = snapshot.Content;
	}

	private void ApplyRefreshedSnapshot(WorkspaceDocumentSnapshot snapshot)
	{
		using IDisposable suppression = BeginSuppression();
		TextWorkspaceEditSelectionState selectionState = TextWorkspaceEditSelectionState.Capture(_editor, _host.FilePath ?? snapshot.DisplayPath);

		_host.FilePath = snapshot.DisplayPath;
		if (!string.Equals(Text, snapshot.Content, StringComparison.Ordinal))
			_editor.Text = snapshot.Content;
		_host.ApplyAuthoritativeBaseline(snapshot.DisplayPath, snapshot.Content);

		RestoreSelectionState(selectionState);
		_text = snapshot.Content;
	}

	private void SetViewState(WorkspaceDocumentSnapshot snapshot)
	{
		_documentId = snapshot.DocumentId;
		_documentKey = snapshot.DocumentKey;
		_fileFormat = snapshot.FileFormat;
		_version = snapshot.Version;
		_host.RecordPersistedContent(snapshot.Content);
		_host.IsContentChanged = snapshot.IsDirty;
	}

	private void RestoreSelectionState(TextWorkspaceEditSelectionState selectionState)
	{
		int documentLength = _editor.Document.TextLength;
		int selectionStart = Math.Clamp(selectionState.SelectionStart, 0, documentLength);
		int selectionEnd = Math.Clamp(selectionState.SelectionEnd, 0, documentLength);
		int caretOffset = Math.Clamp(selectionState.CaretOffset, 0, documentLength);

		if (selectionEnd < selectionStart)
			(selectionStart, selectionEnd) = (selectionEnd, selectionStart);

		_editor.Select(selectionStart, selectionEnd - selectionStart);
		_editor.CaretOffset = caretOffset;
	}

	private void ClearActiveEditTarget()
	{
		if (ReferenceEquals(_host.ActiveEditTarget, this))
			_host.ActiveEditTarget = null;
	}

	private static string ApplyDocumentChange(string text, DocumentChangeEventArgs change)
	{
		string insertedText = change.InsertedText.Text;
		int newLength = text.Length - change.RemovalLength + insertedText.Length;

		return string.Create(
			newLength,
			(text, change.Offset, change.RemovalLength, insertedText),
			static (destination, state) =>
			{
				state.text.AsSpan(0, state.Offset).CopyTo(destination);
				state.insertedText.AsSpan().CopyTo(destination[state.Offset..]);
				state.text.AsSpan(state.Offset + state.RemovalLength).CopyTo(
					destination[(state.Offset + state.insertedText.Length)..]);
			});
	}

	private SuppressionScope BeginSuppression()
	{
		_suppressionDepth++;
		return new SuppressionScope(this);
	}

	private sealed class SuppressionScope : IDisposable
	{
		private readonly TextEditorWorkspaceView _projection;
		private bool _disposed;

		public SuppressionScope(TextEditorWorkspaceView view)
		{
			_projection = view;
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_projection._suppressionDepth--;
		}
	}
}
