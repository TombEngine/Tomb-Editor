#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class FileExplorerPaneProvider : IStudioPaneContributionProvider
{
	private readonly ScriptingWorkspaceProfile _profile;
	private readonly IEditorDocumentController _documentController;
	private readonly FileExplorerViewModel _viewModel;
	private readonly StudioFileExplorerDocumentSyncService _fileSyncService;

	public FileExplorerPaneProvider(
		ScriptingWorkspaceProfile profile,
		IEditorDocumentController documentController,
		FileExplorerViewModel viewModel,
		StudioFileExplorerDocumentSyncService fileSyncService)
	{
		ArgumentNullException.ThrowIfNull(profile);
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(viewModel);
		ArgumentNullException.ThrowIfNull(fileSyncService);

		_profile = profile;
		_documentController = documentController;
		_viewModel = viewModel;
		_fileSyncService = fileSyncService;
	}

	public IReadOnlyList<StudioPaneContribution> GetPaneContributions()
	{
		_viewModel.CommentPrefix = _profile.CommentPrefix;
		_viewModel.ExcludedDirectoryFilter = _profile.FileExplorerExcludedDirectoryFilter;
		_viewModel.Filter = _profile.FileExplorerFilter;
		_viewModel.RootDirectoryPath = _documentController.ScriptRootDirectoryPath;

		var pane = new FileExplorerToolWindow(_viewModel);

		pane.FileOpened += FileExplorer_FileOpened;
		pane.FileChanged += FileExplorer_FileChanged;
		pane.FileDeleted += FileExplorer_FileDeleted;
		pane.FileRenamed += FileExplorer_FileRenamed;

		return [new StudioPaneContribution(UICommand.FileExplorer, pane.SerializationKey, () => pane)];
	}

	public void ApplyWindowFocus(bool isFocused)
		=> _fileSyncService.ApplyWindowFocus(_documentController, isFocused);

	private void FileExplorer_FileChanged(object? sender, FileSystemEventArgs e)
		=> _fileSyncService.ApplyChanged(_documentController, System.Windows.Forms.Form.ActiveForm?.Focused == true, e);

	private void FileExplorer_FileDeleted(object? sender, FileSystemEventArgs e)
		=> _fileSyncService.ApplyDeleted(_documentController, e);

	private void FileExplorer_FileOpened(object? sender, FileOpenedEventArgs e)
		=> _fileSyncService.ApplyOpened(_documentController, e);

	private void FileExplorer_FileRenamed(object? sender, RenamedEventArgs e)
		=> _fileSyncService.ApplyRenamed(_documentController, e);
}
