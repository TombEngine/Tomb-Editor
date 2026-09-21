#nullable enable

using System;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Shell;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.Core.Pathing;
using Nickelony.IDEKit.Workspace.Documents;
using Nickelony.IDEKit.Workspace.Editing;
using Nickelony.IDEKit.Workspace.Views;

namespace TombIDE.ScriptingStudio.TextEditing;

internal readonly record struct SilentActionFileState(
	string FilePath,
	EditorType EditorType,
	bool OpenSourceView,
	bool WasContentChanged,
	IEditorSession? Session);

internal readonly record struct SilentActionCompletion(
	IEditorControl? Editor,
	bool SaveAffectedFile,
	IEditorSession? Session);

internal sealed class StudioSilentActionService
{
	private readonly IEditorDocumentController _documentController;
	private readonly IScriptingHostOperations _hostOperations;
	private readonly IEditorViewHost? _viewHost;
	private readonly IWorkspaceDocumentManager? _documentManager;
	private readonly WorkspaceEditApplier? _workspaceEditApplier;

	public StudioSilentActionService(
		IEditorDocumentController documentController,
		IScriptingHostOperations hostOperations,
		IEditorViewHost? viewHost = null,
		IWorkspaceDocumentManager? documentManager = null)
	{
		_documentController = documentController ?? throw new ArgumentNullException(nameof(documentController));
		_hostOperations = hostOperations ?? throw new ArgumentNullException(nameof(hostOperations));
		_viewHost = viewHost;
		_documentManager = documentManager;
		// Host flavor: target ids are Windows file paths, so deduplicate them case-insensitively.
		_workspaceEditApplier = documentManager is null
			? null
			: new WorkspaceEditApplier(
				request => documentManager.ReplaceAsync(request).GetAwaiter().GetResult().StoreResult,
				LocalPathComparisonPolicy.CaseInsensitive);
	}

	public SilentActionFileState CaptureFileState(string filePath, EditorType editorType = EditorType.Default)
	{
		IEditorControl? editor = _documentController.FindEditor(filePath, editorType);
		bool wasContentChanged = editor is not null && editor.IsContentChanged;
		IEditorSession? session = AcquireSession(filePath);

		return new SilentActionFileState(filePath, editorType, false, wasContentChanged, session);
	}

	public SilentActionFileState CaptureSourceFileState(string filePath)
	{
		IEditorControl? editor = _documentController.FindSourceEditor(filePath);
		bool wasContentChanged = editor is not null && editor.IsContentChanged;
		IEditorSession? session = AcquireSession(filePath);

		return new SilentActionFileState(filePath, EditorType.Default, true, wasContentChanged, session);
	}

	public SilentActionCompletion CreateCompletion(
		SilentActionFileState fileState,
		bool saveAffectedFile = true)
	{
		IEditorControl? editor = fileState.OpenSourceView
			? _documentController.FindSourceEditor(fileState.FilePath)
			: _documentController.FindEditor(fileState.FilePath, fileState.EditorType);

		return new SilentActionCompletion(
			editor,
			saveAffectedFile && !fileState.WasContentChanged,
			fileState.Session);
	}

	public void Complete(bool indicateChange, params SilentActionCompletion[] completions)
	{
		if (indicateChange && _documentController.CurrentEditor is { } currentEditor)
		{
			currentEditor.LastModified = DateTime.Now;
			_hostOperations.IndicateExternalChange();
		}

		foreach (SilentActionCompletion completion in completions)
		{
			if (!completion.SaveAffectedFile
				|| completion.Editor is not { } editor
				|| !_documentController.ContainsEditor(editor))
				continue;

			if (_workspaceEditApplier is not null
				&& editor is TextEditorBase textEditor
				&& textEditor.WorkspaceEditTarget is null)
			{
				ApplyThroughWorkspace(textEditor);
				continue;
			}

			_documentController.SaveFile(editor);
		}

		for (int completionIndex = completions.Length - 1; completionIndex >= 0; completionIndex--)
			completions[completionIndex].Session?.Dispose();

		_documentController.EnsureTabFileSynchronization();
	}

	private void ApplyThroughWorkspace(TextEditorBase editor)
	{
			WorkspaceDocumentManagerOpenResult openResult = _documentManager!
			.OpenAsync(editor.FilePath, DefaultWorkspaceOpenOptions)
			.GetAwaiter()
			.GetResult();

		if (openResult.Snapshot is not WorkspaceDocumentSnapshot snapshot)
		{
			_documentController.SaveFile(editor);
			return;
		}

		_workspaceEditApplier!.Apply([
			new WorkspaceEditTargetPreparation
			{
				TargetId = editor.FilePath,
				Identity = new(snapshot.DocumentKey, snapshot.DocumentId, snapshot.Version),
				BeforeContent = snapshot.Content,
				AfterContent = editor.Text,
				BeforeFileFormat = snapshot.FileFormat,
				AfterFileFormat = snapshot.FileFormat
			}]);

		_documentController.SaveFile(editor);
	}

	private IEditorSession? AcquireSession(string filePath)
	{
		if (_viewHost is null || _documentManager is null)
			return null;

WorkspaceDocumentManagerOpenResult result = _documentManager
			.OpenAsync(filePath, DefaultWorkspaceOpenOptions)
			.GetAwaiter()
			.GetResult();

		WorkspaceDocumentSnapshot snapshot = result.Snapshot
			?? throw new InvalidOperationException($"Unable to resolve the canonical document for '{filePath}'.");

		return _viewHost.Open(snapshot, new(EditorSessionMode.Transient)).Session;
	}

	private static readonly WorkspaceDocumentOpenOptions DefaultWorkspaceOpenOptions = new(
		TextEncodingKind.Utf8,
		new TextFileFormat(TextEncodingKind.Utf8, false, TextNewlineStyle.Lf));
}
