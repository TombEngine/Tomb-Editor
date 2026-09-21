#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.Workspace.Documents;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using TombIDE.ScriptingStudio.Workspace;
using Nickelony.IDEKit.Workspace.Documents.FileSystem;
using Nickelony.IDEKit.Workspace.Documents.Reloading;

namespace TombIDE.ScriptingStudio.Controls;

internal sealed partial class EditorDocumentController : IEditorDocumentController
{
	private readonly EditorDocumentControllerCore _documentController;
	private readonly FileReloadCoordinator<DialogResult> _fileReloadCoordinator = new();
	private readonly IMessageService _messageService;
	private readonly IWorkspaceDocumentManager? _documentManager;
	private readonly IWorkspaceFileSystem? _workspaceFileSystem;
	private readonly Dictionary<IEditorControl, IWorkspaceDocumentView> _workspaceViews = [];
	private IEditorControl? _currentEditor;
	private ScriptingDocumentContext _currentDocumentContext = ScriptingDocumentContext.Empty;
	private long _documentContextGeneration;
	private string _scriptRootDirectoryPath;

	public EditorDocumentController(
		Version currentEngineVersion,
		string scriptRootDirectoryPath,
		IMessageService? messageService = null,
		IWorkspaceDocumentManager? documentManager = null,
		IWorkspaceFileSystem? workspaceFileSystem = null)
	{
		ArgumentNullException.ThrowIfNull(currentEngineVersion);

		_documentController = new EditorDocumentControllerCore(currentEngineVersion);
		_documentManager = documentManager;
		_workspaceFileSystem = workspaceFileSystem;
		_messageService = messageService
			?? ServiceLocator.GetService<IMessageService>()
			?? new MessageBoxService();
		_scriptRootDirectoryPath = string.Empty;
		ScriptRootDirectoryPath = scriptRootDirectoryPath;
	}

	public string ScriptRootDirectoryPath
	{
		get => _scriptRootDirectoryPath;
		set
		{
			if (!AskSaveAll())
				return;

			CloseAllEditors();
			_scriptRootDirectoryPath = value ?? string.Empty;
		}
	}

	public IEditorControl? CurrentEditor => _currentEditor;

	public ScriptingDocumentContext CurrentDocumentContext => _currentDocumentContext;

	public ScriptingDocumentRegistration? GetDocumentRegistration(IEditorControl? editor)
		=> _documentController.GetDocumentRegistration(editor);

	public IEditorControl? FindEditor(string filePath, EditorType editorType = EditorType.Default)
		=> _documentController.FindEditor(filePath, editorType);

	public IEditorControl? FindSourceEditor(string filePath)
		=> _documentController.FindSourceEditor(filePath);

	public IEnumerable<IEditorControl> GetOpenEditors()
		=> _documentController.GetOpenEditors();

	public IEnumerable<IEditorControl> FindEditorsOfFile(string filePath)
		=> _documentController.FindEditorsOfFile(filePath);

	public bool ContainsEditor(IEditorControl editor)
		=> _documentController.ContainsEditor(editor);

	public void RegisterDocument(ScriptingDocumentRegistration registration)
		=> _documentController.RegisterDocument(registration);

	public void OpenFile(string filePath, EditorType editorType = EditorType.Default, DocumentLoadOptions options = default)
	{
		IEditorControl? existingEditor = FindEditor(filePath, editorType);

		if (existingEditor is not null)
		{
			ActivateEditor(existingEditor);
			return;
		}

		EditorOpenResult openResult = _documentController.CreateEditor(filePath, editorType);

		if (openResult.Editor is null || !openResult.IsNewDocument)
			return;

		if (!TryAttachWorkspaceView(openResult.Editor, filePath, options))
			return;

		_documentController.AddEditor(openResult.Editor);
		AttachEditor(openResult.Editor);
		ActivateEditor(openResult.Editor);
		OnFileOpened(EventArgs.Empty);
	}

	public void OpenSourceFile(string filePath, DocumentLoadOptions options = default)
		=> OpenFile(filePath, _documentController.GetSourceViewEditorType(filePath), options);

	public string GetDocumentTitle(IEditorControl editor)
	{
		if (editor is null)
			return string.Empty;

		string title = _documentController.GetDocumentTitle(editor);
		return editor.IsContentChanged ? title + "*" : title;
	}

	public void RenameDocument(string oldFilePath, string newFilePath)
	{
		if (string.IsNullOrWhiteSpace(oldFilePath)
			|| string.IsNullOrWhiteSpace(newFilePath)
			|| oldFilePath.Equals(newFilePath, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		List<IEditorControl> editors = FindEditorsOfFile(oldFilePath).ToList();

		if (editors.Count == 0)
			return;

		foreach (IEditorControl editor in editors)
		{
			editor.FilePath = newFilePath;
			RaiseEditorTitleChanged(editor);
		}

		OnDocumentRenamed(new DocumentRenamedEventArgs(oldFilePath, newFilePath));
	}

	public bool IsEveryDocumentSaved()
	{
		return _documentController.GetFilePaths()
			.Select(path => FindEditorsOfFile(path).FirstOrDefault())
			.Where(static editor => editor is not null)
			.All(editor => !IsDocumentDirty(editor!));
	}

	public void EnsureTabFileSynchronization()
	{
		foreach (string filePath in _documentController.GetFilePaths())
			SynchronizeEditorsOfFile(filePath);
	}

	public event EventHandler? FileOpened;

	public event EventHandler<ScriptingDocumentContextChangedEventArgs>? CurrentEditorChanged;

	public event EventHandler<EditorControlEventArgs>? EditorClosed;

	public event EventHandler<EditorControlEventArgs>? EditorTitleChanged;

	public event EventHandler<DocumentRenamedEventArgs>? DocumentRenamed;

}
