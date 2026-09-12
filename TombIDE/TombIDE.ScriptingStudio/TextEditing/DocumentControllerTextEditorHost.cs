#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombIDE.ScriptingStudio.Controls;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.AvalonEdit.Documents;
using Nickelony.IDEKit.Workspace.Documents;
using Nickelony.IDEKit.Workspace.Views;

namespace TombIDE.ScriptingStudio.TextEditing;

internal sealed class DocumentControllerTextEditorHost : ITextEditorHost, IEditorViewHost
{
	private readonly IEditorDocumentController _documentController;
	private readonly IWorkspaceDocumentManager? _documentManager;

	public DocumentControllerTextEditorHost(
		IEditorDocumentController documentController,
		IWorkspaceDocumentManager? documentManager = null)
	{
		_documentController = documentController ?? throw new ArgumentNullException(nameof(documentController));
		_documentManager = documentManager;
	}

	public TEditor? OpenEditor<TEditor>(string filePath, EditorType editorType = EditorType.Default, bool openSourceView = false)
		where TEditor : class, IEditorControl
		=> OpenEditorControl(filePath, editorType, openSourceView) as TEditor;

	public ITextSnapshot? TryGetTextSnapshot(string filePath)
	{
		if (_documentController.FindEditorsOfFile(filePath)
			.OfType<TextEditorBase>()
			.FirstOrDefault() is { } openEditor)
		{
			return new TextDocumentSnapshot(openEditor.Document);
		}

		if (_documentManager is null)
			throw new InvalidOperationException("Canonical document snapshots require a workspace manager.");

		WorkspaceDocumentOpenResult result = _documentManager
			.OpenAsync(filePath, DefaultWorkspaceOpenOptions)
			.GetAwaiter()
			.GetResult();

		return result.Snapshot?.Text;
	}

	public TextEditorBase OpenTextEditor(string filePath, EditorType editorType = EditorType.Default, bool openSourceView = false)
	{
		if (!_documentController.FindEditorsOfFile(filePath).Any() && !File.Exists(filePath))
			throw new FileNotFoundException("Unable to apply a workspace edit because the target file could not be found.", filePath);

		IEditorControl editor = OpenEditorControl(filePath, editorType, openSourceView);

		if (editor is not TextEditorBase textEditor)
			throw new InvalidOperationException($"Unable to apply workspace edits to '{filePath}'.");

		return textEditor;
	}

	public EditorSessionOpenResult Open(
		WorkspaceDocumentSnapshot snapshot,
		EditorSessionOptions options)
	{
		ArgumentNullException.ThrowIfNull(snapshot);

		IEditorControl? previousEditor = _documentController.CurrentEditor;
		IEditorControl? editor = _documentController.FindEditorsOfFile(snapshot.DisplayPath).FirstOrDefault();
		bool acquiredView = false;

		if (editor is null)
		{
			_documentController.OpenSourceFile(snapshot.DisplayPath);
			editor = _documentController.FindEditorsOfFile(snapshot.DisplayPath).FirstOrDefault();
			acquiredView = editor is not null;
		}

		if (editor is null)
			return new EditorSessionOpenResult(EditorSessionOpenStatus.Unavailable, null);

		if (!ReferenceEquals(_documentController.CurrentEditor, editor))
			_documentController.ActivateEditor(editor);

		return new EditorSessionOpenResult(
			acquiredView ? EditorSessionOpenStatus.Opened : EditorSessionOpenStatus.AlreadyOpen,
			new EditorSession(
				snapshot,
				options.Mode,
				acquiredView,
				() => _documentController.ContainsEditor(editor) && _documentController.TryCloseEditor(editor),
				() =>
				{
					if (previousEditor is not null && _documentController.ContainsEditor(previousEditor))
						_documentController.ActivateEditor(previousEditor);
				}));
	}

	public IReadOnlyList<IEditorControl> GetOpenEditors(string filePath)
		=> [.. _documentController.FindEditorsOfFile(filePath)];

	public TResult ExecutePreservingSelection<TResult>(Func<TResult> action)
	{
		ArgumentNullException.ThrowIfNull(action);

		IEditorControl? previouslySelectedEditor = _documentController.CurrentEditor;

		try
		{
			return action();
		}
		finally
		{
			if (previouslySelectedEditor is not null && _documentController.ContainsEditor(previouslySelectedEditor))
				_documentController.ActivateEditor(previouslySelectedEditor);
		}
	}

	private IEditorControl OpenEditorControl(string filePath, EditorType editorType, bool openSourceView)
	{
		if (openSourceView)
			_documentController.OpenSourceFile(filePath);
		else
			_documentController.OpenFile(filePath, editorType);

		return _documentController.CurrentEditor
			?? throw new InvalidOperationException($"Unable to open '{filePath}'.");
	}

	private static readonly WorkspaceDocumentOpenOptions DefaultWorkspaceOpenOptions = new(
		TextEncodingKind.Utf8,
		new TextFileFormat(TextEncodingKind.Utf8, false, TextNewlineStyle.Lf));
}
