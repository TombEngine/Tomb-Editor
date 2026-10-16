using System;
using System.IO;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.FileExplorer;

namespace TombIDE.ScriptingStudio.Shell;

internal sealed class StudioFileExplorerDocumentSyncService
{
	public void ApplyChanged(IEditorDocumentController documentController, bool isMainWindowFocused, FileSystemEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(e);

		if (!isMainWindowFocused)
			documentController.AddFileToReloadQueue(e.FullPath);
	}

	public void ApplyDeleted(IEditorDocumentController documentController, FileSystemEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(e);

		documentController.AddFileToReloadQueue(e.FullPath);
	}

	public void ApplyOpened(IEditorDocumentController documentController, FileOpenedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(e);

		if (e.OpenSourceView)
			documentController.OpenSourceFile(e.FilePath);
		else
			documentController.OpenFile(e.FilePath, e.EditorType);
	}

	public void ApplyRenamed(IEditorDocumentController documentController, RenamedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(e);

		documentController.AddFileToReloadQueue(e.OldFullPath);
	}

	public void ApplyWindowFocus(IEditorDocumentController documentController, bool isMainWindowFocused)
	{
		ArgumentNullException.ThrowIfNull(documentController);

		if (isMainWindowFocused)
			documentController.TryRunFileReloadQueue();
	}
}
