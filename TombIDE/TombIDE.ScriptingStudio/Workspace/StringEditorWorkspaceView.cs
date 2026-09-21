#nullable enable

using System;
using TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;
using TombLib.Scripting.ClassicScript.StringTables;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.Workspace.Documents;
using Nickelony.IDEKit.Workspace.Views;
using Nickelony.IDEKit.Core.Editing;

namespace TombIDE.ScriptingStudio.Workspace;

internal sealed class StringEditorWorkspaceView : IWorkspaceDocumentDeleteGuardView, ITextEditTarget, ITextEditTargetVersion, IWorkspaceScriptView, IWorkspaceViewPendingEdits
{
	private readonly StringEditorView _view;
	private readonly string _projectionId = Guid.NewGuid().ToString("N");
	private string _documentId = string.Empty;
	private WorkspaceDocumentKey? _documentKey;
	private TextFileFormat _fileFormat;
	private long _version;
	private bool _hasPendingEdits;
	private bool _hasConflict;
	private bool _deleteGuardActive;
	private bool _wasEnabled;

	public StringEditorWorkspaceView(StringEditorView view)
	{
		_view = view ?? throw new ArgumentNullException(nameof(view));
		_view.WorkspaceContentChanged += OnWorkspaceContentChanged;
	}

	public event EventHandler<WorkspaceDocumentViewApplyRequestedEventArgs>? ApplyRequested;

	public string ViewId => _projectionId;

	public string DocumentId => _documentId;

	public WorkspaceDocumentKey? DocumentKey => _documentKey;

	public bool HasPendingEdits => _hasPendingEdits;

	public bool HasConflict => _hasConflict;

	public string Text => _documentKey is null
		? string.Empty
		: _hasPendingEdits ? _view.WorkspacePendingContent : _view.WorkspaceCanonicalContent;

	public long Version => _version;

	public void Apply(PreparedTextEdits edits)
	{
		ArgumentNullException.ThrowIfNull(edits);
		if (_documentKey is null)
			throw new InvalidOperationException("The string-table view is not attached.");

		string updatedContent = Text;
		foreach (TextEditOperation operation in edits.Operations)
		{
			if (operation.StartOffset < 0
				|| operation.EndOffset < operation.StartOffset
				|| operation.EndOffset > updatedContent.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(edits));
			}

			updatedContent = updatedContent.Remove(operation.StartOffset, operation.Length)
				.Insert(operation.StartOffset, operation.NewText);
		}

		ClassicScriptStringTableParseResult parseResult = _view.ParseWorkspaceContent(
			updatedContent,
			GetParsedNewline(updatedContent));
		if (!parseResult.IsSuccess || parseResult.Model is null)
			throw new InvalidOperationException(parseResult.Diagnostics[0].Message);

		_view.ApplyWorkspaceTable(parseResult.Model);
		_view.SetWorkspacePendingContent(updatedContent);
		OnWorkspaceContentChanged(_view, EventArgs.Empty);
	}

	public WorkspaceDocumentViewOpenResult Open(WorkspaceDocumentSnapshot snapshot)
	{
		ArgumentNullException.ThrowIfNull(snapshot);

		if (_documentKey is not null)
			return new WorkspaceDocumentViewOpenResult(WorkspaceDocumentViewOpenOutcome.AlreadyOpen);

		if (_hasPendingEdits || _hasConflict)
			return new WorkspaceDocumentViewOpenResult(WorkspaceDocumentViewOpenOutcome.Unavailable);

		if (!TryParse(snapshot.Content, out ClassicScriptStringTableParseResult? parseResult))
			return new WorkspaceDocumentViewOpenResult(
				WorkspaceDocumentViewOpenOutcome.Unavailable,
				new WorkspaceOperationFailure("ParseFailed", parseResult.Diagnostics[0].Message));

		try
		{
			_view.ApplyWorkspaceSnapshot(snapshot, parseResult.Model!);
			SetViewState(snapshot);
			return new WorkspaceDocumentViewOpenResult(WorkspaceDocumentViewOpenOutcome.Opened);
		}
		catch (Exception exception)
		{
			return new WorkspaceDocumentViewOpenResult(
				WorkspaceDocumentViewOpenOutcome.Unavailable,
				new WorkspaceOperationFailure("OpenFailed", exception.Message, exception));
		}
	}

	public WorkspaceDocumentViewRefreshResult Refresh(WorkspaceDocumentSnapshot snapshot)
	{
		ArgumentNullException.ThrowIfNull(snapshot);

		if (_documentKey is null)
			return Failed("ViewNotAttached", "The view is not attached.");

		if (snapshot.DocumentKey != _documentKey
			|| !string.Equals(snapshot.DocumentId, _documentId, StringComparison.Ordinal))
		{
			_hasConflict = true;
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.MarkedStale);
		}

		if (snapshot.Version < _version || _hasPendingEdits || _hasConflict)
		{
			_hasConflict = true;
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.MarkedStale);
		}

		if (!TryParse(snapshot.Content, out ClassicScriptStringTableParseResult? parseResult))
		{
			_hasConflict = true;
			return Failed("ParseFailed", parseResult.Diagnostics[0].Message);
		}

		try
		{
			_view.ApplyWorkspaceSnapshot(snapshot, parseResult.Model!);
			SetViewState(snapshot);
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.Refreshed);
		}
		catch (Exception exception)
		{
			_hasConflict = true;
			return Failed("RefreshFailed", exception.Message, exception);
		}
	}

	public WorkspaceDocumentViewRefreshResult DiscardPendingEdits(WorkspaceDocumentSnapshot snapshot)
	{
		ArgumentNullException.ThrowIfNull(snapshot);

		if (_documentKey is null
			|| snapshot.DocumentKey != _documentKey
			|| !string.Equals(snapshot.DocumentId, _documentId, StringComparison.Ordinal))
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.MarkedStale);

		if (!TryParse(snapshot.Content, out ClassicScriptStringTableParseResult? parseResult))
			return Failed("ParseFailed", parseResult.Diagnostics[0].Message);

		try
		{
			_view.ApplyWorkspaceSnapshot(snapshot, parseResult.Model!);
			SetViewState(snapshot);
			_hasPendingEdits = false;
			_hasConflict = false;
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.Refreshed);
		}
		catch (Exception exception)
		{
			_hasConflict = true;
			return Failed("ViewDiscardFailed", exception.Message, exception);
		}
	}

	public WorkspaceDocumentViewIdentityResult AcknowledgeIdentity(WorkspaceDocumentViewIdentityChange change)
	{
		ArgumentNullException.ThrowIfNull(change);

		if (_documentKey is null
			|| _documentKey != change.OldDocumentKey
			|| !string.Equals(_documentId, change.OldDocumentId, StringComparison.Ordinal))
			return new WorkspaceDocumentViewIdentityResult(
				WorkspaceDocumentViewIdentityOutcome.Failed,
				new WorkspaceOperationFailure("ViewIdentityMismatch", "The view is not attached to the expected document identity."));

		if (!TryParse(change.Snapshot.Content, out ClassicScriptStringTableParseResult? parseResult))
			return new WorkspaceDocumentViewIdentityResult(
				WorkspaceDocumentViewIdentityOutcome.Failed,
				new WorkspaceOperationFailure("ParseFailed", parseResult.Diagnostics[0].Message));

		try
		{
			_view.ApplyWorkspaceSnapshot(change.Snapshot, parseResult.Model!);
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

	public WorkspaceDocumentViewDeleteGuardResult ApplyDeleteGuard()
	{
		if (_deleteGuardActive)
			return new WorkspaceDocumentViewDeleteGuardResult(WorkspaceDocumentViewDeleteGuardOutcome.Succeeded);

		try
		{
			_wasEnabled = _view.IsEnabled;
			_view.IsEnabled = false;
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

	public WorkspaceDocumentViewDeleteGuardResult ReleaseDeleteGuard()
	{
		if (!_deleteGuardActive)
			return new WorkspaceDocumentViewDeleteGuardResult(WorkspaceDocumentViewDeleteGuardOutcome.Succeeded);

		try
		{
			_view.IsEnabled = _wasEnabled;
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

	public WorkspaceDocumentViewRefreshResult AcknowledgeApply(WorkspaceDocumentMutationResult result)
	{
		ArgumentNullException.ThrowIfNull(result);

		if (_documentKey is null)
			return Failed("ViewNotAttached", "The view is not attached.");

		if (result.Outcome is not (WorkspaceDocumentMutationOutcome.Changed or WorkspaceDocumentMutationOutcome.NoChange)
			|| result.Snapshot is null)
		{
			_hasConflict = true;
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.MarkedStale);
		}

		if (result.RequestedIdentity.DocumentKey != _documentKey
			|| !string.Equals(result.RequestedIdentity.DocumentId, _documentId, StringComparison.Ordinal))
		{
			_hasConflict = true;
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.MarkedStale);
		}

		if (!TryParse(result.Snapshot.Content, out ClassicScriptStringTableParseResult? parseResult))
		{
			_hasConflict = true;
			return Failed("ParseFailed", parseResult.Diagnostics[0].Message);
		}

		try
		{
			_view.ApplyWorkspaceSnapshot(result.Snapshot, parseResult.Model!);
			SetViewState(result.Snapshot);
			_hasPendingEdits = false;
			_hasConflict = false;
			return new WorkspaceDocumentViewRefreshResult(WorkspaceDocumentViewRefreshOutcome.Refreshed);
		}
		catch (Exception exception)
		{
			_hasConflict = true;
			return Failed("AcknowledgeFailed", exception.Message, exception);
		}
	}

	public void Close()
	{
		if (_deleteGuardActive)
			ReleaseDeleteGuard();

		_view.WorkspaceContentChanged -= OnWorkspaceContentChanged;
		_view.DetachWorkspaceView();
		_documentId = string.Empty;
		_documentKey = null;
		_version = 0;
		_hasPendingEdits = false;
		_hasConflict = false;
	}

	private void OnWorkspaceContentChanged(object? sender, EventArgs e)
	{
		if (_documentKey is null)
			return;

		_hasPendingEdits = true;
		ApplyRequested?.Invoke(
			this,
			new WorkspaceDocumentViewApplyRequestedEventArgs(
				new WorkspaceDocumentReplaceRequest(
					new(_documentKey.Value, _documentId, _version),
					_view.WorkspacePendingContent,
					_fileFormat)));
	}

	private void SetViewState(WorkspaceDocumentSnapshot snapshot)
	{
		_documentId = snapshot.DocumentId;
		_documentKey = snapshot.DocumentKey;
		_fileFormat = snapshot.FileFormat;
		_version = snapshot.Version;
	}

	private bool TryParse(
		string content,
		out ClassicScriptStringTableParseResult result)
	{
		result = _view.ParseWorkspaceContent(content, GetParsedNewline(content));
		return result.IsSuccess && result.Model is not null;
	}

	private static string GetParsedNewline(string content)
	{
		int carriageReturnIndex = content.IndexOf('\r');
		int lineFeedIndex = content.IndexOf('\n');
		if (carriageReturnIndex >= 0 && carriageReturnIndex + 1 < content.Length && content[carriageReturnIndex + 1] == '\n')
			return "\r\n";

		if (carriageReturnIndex >= 0 && (lineFeedIndex < 0 || carriageReturnIndex < lineFeedIndex))
			return "\r";

		return "\n";
	}

	private static WorkspaceDocumentViewRefreshResult Failed(
		string code,
		string message,
		Exception? exception = null)
		=> new(
			WorkspaceDocumentViewRefreshOutcome.Failed,
			new WorkspaceOperationFailure(code, message, exception));
}
