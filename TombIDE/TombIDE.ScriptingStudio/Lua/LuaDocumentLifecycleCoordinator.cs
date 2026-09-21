#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Nickelony.IDEKit.IntelliSense.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Shell;

namespace TombIDE.ScriptingStudio.Lua;

internal sealed class LuaDocumentLifecycleCoordinator : ILuaEditorLifecycleService
{
	private readonly IEditorDocumentController _documentController;
	private readonly IMessenger _messenger;
	private readonly ILuaLanguageServerIntelliSenseProvider _intellisenseProvider;
	private readonly LuaTrackedDocumentStateService _trackedDocumentStateService;
	private readonly HashSet<LuaEditor> _trackedEditors = [];

	public LuaDocumentLifecycleCoordinator(
		IEditorDocumentController documentController,
		IMessenger messenger,
		ILuaLanguageServerIntelliSenseProvider intellisenseProvider,
		LuaTrackedDocumentStateService trackedDocumentStateService)
	{
		_documentController = documentController ?? throw new ArgumentNullException(nameof(documentController));
		_messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
		_intellisenseProvider = intellisenseProvider ?? throw new ArgumentNullException(nameof(intellisenseProvider));
		_trackedDocumentStateService = trackedDocumentStateService ?? throw new ArgumentNullException(nameof(trackedDocumentStateService));
	}

	public void Attach()
	{
		Detach();

		_documentController.FileOpened += DocumentController_FileOpened;
		_documentController.CurrentEditorChanged += DocumentController_CurrentEditorChanged;
		_documentController.EditorClosed += DocumentController_EditorClosed;
		_documentController.DocumentRenamed += DocumentController_DocumentRenamed;
	}

	public void Detach()
	{
		_documentController.FileOpened -= DocumentController_FileOpened;
		_documentController.CurrentEditorChanged -= DocumentController_CurrentEditorChanged;
		_documentController.EditorClosed -= DocumentController_EditorClosed;
		_documentController.DocumentRenamed -= DocumentController_DocumentRenamed;

		foreach (LuaEditor editor in _documentController.GetOpenEditors().OfType<LuaEditor>())
			DetachEditor(editor);
	}

	public void Dispose()
		=> Detach();

	private void DocumentController_FileOpened(object? sender, EventArgs e)
	{
		if (sender is not LuaEditor editor)
			return;

		AttachEditor(editor);
		if (_trackedEditors.Add(editor))
			_trackedDocumentStateService.OpenDocument(editor);
		_messenger.Send(new ShellUiRefreshMessage());
	}

	private void DocumentController_CurrentEditorChanged(object? sender, EventArgs e)
		=> _messenger.Send(new ShellUiRefreshMessage());

	private void DocumentController_EditorClosed(object? sender, EditorControlEventArgs e)
	{
		if (e.Editor is LuaEditor editor)
		{
			DetachEditor(editor);
			_trackedEditors.Remove(editor);
		}
	}

	private void DocumentController_DocumentRenamed(object? sender, DocumentRenamedEventArgs e)
	{
		LuaEditor? editor = null;

		editor = _documentController.FindEditorsOfFile(e.NewFilePath).OfType<LuaEditor>().FirstOrDefault();

		if (editor is null)
			return;

		_trackedDocumentStateService.RenameDocument(e.OldFilePath, e.NewFilePath, editor);

		if (ReferenceEquals(_documentController.CurrentEditor, editor))
			_messenger.Send(new ShellUiRefreshMessage());
	}

	private void Editor_TextChangedDelayed(object? sender, EventArgs e)
	{
		if (sender is LuaEditor editor)
			_trackedDocumentStateService.UpdateDocument(editor);
	}

	private void AttachEditor(LuaEditor editor)
	{
		editor.IntelliSenseProvider = _intellisenseProvider;
		editor.DefinitionNavigationRequested -= Editor_DefinitionNavigationRequested;
		editor.DefinitionNavigationRequested += Editor_DefinitionNavigationRequested;
		editor.StatusChanged -= Editor_StatusChanged;
		editor.StatusChanged += Editor_StatusChanged;
		editor.TextChanged -= Editor_TextChanged;
		editor.TextChanged += Editor_TextChanged;
		editor.TextChangedDelayed -= Editor_TextChangedDelayed;
		editor.TextChangedDelayed += Editor_TextChangedDelayed;
	}

	private void DetachEditor(LuaEditor editor)
	{
		editor.DefinitionNavigationRequested -= Editor_DefinitionNavigationRequested;
		editor.StatusChanged -= Editor_StatusChanged;
		editor.TextChanged -= Editor_TextChanged;
		editor.TextChangedDelayed -= Editor_TextChangedDelayed;
	}

	private void Editor_DefinitionNavigationRequested(TextDefinitionLocation location)
		=> _messenger.Send(new LuaDefinitionNavigationMessage(location));

	private void Editor_StatusChanged(object? sender, EventArgs e)
		=> _messenger.Send(new CommandStateRefreshMessage());

	private void Editor_TextChanged(object? sender, EventArgs e)
		=> _messenger.Send(new CommandStateRefreshMessage());
}
