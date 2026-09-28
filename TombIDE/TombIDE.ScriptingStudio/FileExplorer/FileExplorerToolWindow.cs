#nullable enable

using System;
using System.IO;
using System.Windows;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.FileExplorer;

public sealed class FileExplorerToolWindow : StudioDockPane
{
	private readonly FileExplorerView _view;
	private readonly FileExplorerViewModel _viewModel;

	public FileExplorerToolWindow(FileExplorerViewModel viewModel)
		: base(Strings.Default.FileExplorer, "FileExplorer", StudioDockPaneLocation.Left, new Size(280, 320))
	{
		ArgumentNullException.ThrowIfNull(viewModel);

		_viewModel = viewModel;
		_view = new FileExplorerView
		{
			DataContext = _viewModel
		};

		_viewModel.FileOpened += ViewModel_FileOpened;
		_viewModel.FileChanged += ViewModel_FileChanged;
		_viewModel.FileCreated += ViewModel_FileCreated;
		_viewModel.FileDeleted += ViewModel_FileDeleted;
		_viewModel.FileRenamed += ViewModel_FileRenamed;
	}

	public override UIElement Content => _view;

	public string CommentPrefix
	{
		get => _viewModel.CommentPrefix;
		set => _viewModel.CommentPrefix = value;
	}

	public string ExcludedDirectoryFilter
	{
		get => _viewModel.ExcludedDirectoryFilter;
		set => _viewModel.ExcludedDirectoryFilter = value;
	}

	public string Filter
	{
		get => _viewModel.Filter;
		set => _viewModel.Filter = value;
	}

	public System.IO.NotifyFilters NotifyFilter
	{
		get => _viewModel.NotifyFilter;
		set => _viewModel.NotifyFilter = value;
	}

	public string RootDirectoryPath
	{
		get => _viewModel.RootDirectoryPath;
		set => _viewModel.RootDirectoryPath = value;
	}

	public event FileSystemEventHandler? FileChanged;
	public event FileSystemEventHandler? FileCreated;
	public event FileSystemEventHandler? FileDeleted;
	public event FileOpenedEventHandler? FileOpened;
	public event RenamedEventHandler? FileRenamed;

	public string? CreateNewFile()
		=> _viewModel.CreateNewFile();

	public void CreateNewFolder()
		=> _viewModel.CreateNewFolder();

	public void UpdateFileList()
		=> _viewModel.UpdateFileList();

	public override void Dispose()
	{
		_viewModel.FileOpened -= ViewModel_FileOpened;
		_viewModel.FileChanged -= ViewModel_FileChanged;
		_viewModel.FileCreated -= ViewModel_FileCreated;
		_viewModel.FileDeleted -= ViewModel_FileDeleted;
		_viewModel.FileRenamed -= ViewModel_FileRenamed;
		FileChanged = null;
		FileCreated = null;
		FileDeleted = null;
		FileOpened = null;
		FileRenamed = null;
		_viewModel.Dispose();
	}

	private void ViewModel_FileChanged(object sender, FileSystemEventArgs e)
		=> FileChanged?.Invoke(this, e);

	private void ViewModel_FileCreated(object sender, FileSystemEventArgs e)
		=> FileCreated?.Invoke(this, e);

	private void ViewModel_FileDeleted(object sender, FileSystemEventArgs e)
		=> FileDeleted?.Invoke(this, e);

	private void ViewModel_FileOpened(object sender, FileOpenedEventArgs e)
		=> FileOpened?.Invoke(this, e);

	private void ViewModel_FileRenamed(object sender, RenamedEventArgs e)
		=> FileRenamed?.Invoke(this, e);
}
