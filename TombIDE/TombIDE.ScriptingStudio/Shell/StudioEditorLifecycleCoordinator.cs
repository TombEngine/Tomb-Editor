#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using Nickelony.IDEKit.KeyBindings;
using System;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Shell;

internal sealed class StudioEditorLifecycleCoordinator : IEditorLifecycleService
{
	private readonly IEditorDocumentController _documentController;
	private readonly IMessenger _messenger;
	private readonly Action<IEditorControl> _applyUserSettings;
	private readonly KeyBindingDispatcher<UICommand> _shortcutDispatcher;

	public StudioEditorLifecycleCoordinator(
		IEditorDocumentController documentController,
		IMessenger messenger,
		Action<IEditorControl> applyUserSettings,
		Action<UICommand> executeCommand,
		Func<UICommand, bool> canExecuteCommand,
		IKeyBindingService<UICommand> shortcutBindings)
	{
		_documentController = documentController ?? throw new ArgumentNullException(nameof(documentController));
		_messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
		_applyUserSettings = applyUserSettings ?? throw new ArgumentNullException(nameof(applyUserSettings));
		_shortcutDispatcher = new KeyBindingDispatcher<UICommand>(
			shortcutBindings ?? throw new ArgumentNullException(nameof(shortcutBindings)),
			canExecuteCommand ?? throw new ArgumentNullException(nameof(canExecuteCommand)),
			executeCommand ?? throw new ArgumentNullException(nameof(executeCommand)));
	}

	public void Attach()
	{
		Detach();
		_documentController.FileOpened += DocumentController_FileOpened;
		_documentController.EditorClosed += DocumentController_EditorClosed;
	}

	public void Detach()
	{
		_documentController.FileOpened -= DocumentController_FileOpened;
		_documentController.EditorClosed -= DocumentController_EditorClosed;

		foreach (IEditorControl editor in _documentController.GetOpenEditors())
			DetachEditor(editor);
	}

	public void Dispose()
		=> Detach();

	private void DocumentController_FileOpened(object? sender, EventArgs e)
	{
		if (sender is not IEditorControl editor)
			return;

		AttachEditor(editor);
		_applyUserSettings(editor);
		_messenger.Send(new ShellUiRefreshMessage());
	}

	private void DocumentController_EditorClosed(object? sender, EditorControlEventArgs e)
		=> DetachEditor(e.Editor);

	private void AttachEditor(IEditorControl editor)
	{
		editor.ContentChangedWorkerRunCompleted -= Editor_ContentChangedWorkerRunCompleted;
		editor.ContentChangedWorkerRunCompleted += Editor_ContentChangedWorkerRunCompleted;

		if (editor is not TextEditorBase textEditor)
			return;

		textEditor.KeyDown -= TextEditor_KeyDown;
		textEditor.KeyDown += TextEditor_KeyDown;
		textEditor.TextChanged -= TextEditor_TextChanged;
		textEditor.TextChanged += TextEditor_TextChanged;
	}

	private void DetachEditor(IEditorControl editor)
	{
		editor.ContentChangedWorkerRunCompleted -= Editor_ContentChangedWorkerRunCompleted;

		if (editor is not TextEditorBase textEditor)
			return;

		textEditor.KeyDown -= TextEditor_KeyDown;
		textEditor.TextChanged -= TextEditor_TextChanged;
	}

	private void Editor_ContentChangedWorkerRunCompleted(object? sender, EventArgs e)
		=> _messenger.Send(new CommandStateRefreshMessage());

	private void TextEditor_TextChanged(object? sender, EventArgs e)
		=> _messenger.Send(new CommandStateRefreshMessage());

	private void TextEditor_KeyDown(object? sender, System.Windows.Input.KeyEventArgs e)
	{
		if (_shortcutDispatcher.TryHandleKeyDown(e))
			e.Handled = true;
	}
}
