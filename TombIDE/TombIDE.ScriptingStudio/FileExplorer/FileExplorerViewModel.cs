#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic.FileIO;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Interop;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.Shared.SharedClasses;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using Nickelony.IDEKit.Workspace.Documents;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.FileExplorer;

public sealed partial class FileExplorerViewModel : ObservableObject, IDisposable
{
	private readonly IDialogService _dialogService;
	private readonly IMessageService _messageService;
	private readonly ILocalizationService _localizationService;
	private readonly IWin32DialogOwnerProvider _dialogOwnerProvider;
	private readonly IWorkspaceDocumentManager? _documentManager;
	private readonly FileSystemWatcher _fileSystemWatcher;

	private string _commentPrefix = string.Empty;
	private string _excludedDirectoryFilter = string.Empty;
	private string _filter = "*.*";
	private NotifyFilters _notifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
	private string _rootDirectoryPath = string.Empty;

	public FileExplorerViewModel(
		IDialogService dialogService,
		IMessageService messageService,
		ILocalizationService localizationService,
		IWin32DialogOwnerProvider dialogOwnerProvider)
		: this(dialogService, messageService, localizationService, dialogOwnerProvider, null)
	{ }

	internal FileExplorerViewModel(
		IDialogService dialogService,
		IMessageService messageService,
		ILocalizationService localizationService,
		IWin32DialogOwnerProvider dialogOwnerProvider,
		IWorkspaceDocumentManager? documentManager)
	{
		ArgumentNullException.ThrowIfNull(dialogService);
		ArgumentNullException.ThrowIfNull(messageService);
		ArgumentNullException.ThrowIfNull(localizationService);
		ArgumentNullException.ThrowIfNull(dialogOwnerProvider);

		_dialogService = dialogService;
		_messageService = messageService;
		_localizationService = localizationService.WithKeysFor(this);
		_dialogOwnerProvider = dialogOwnerProvider;
		_documentManager = documentManager;

		_fileSystemWatcher = new FileSystemWatcher
		{
			EnableRaisingEvents = false,
			Filter = "*.*",
			IncludeSubdirectories = true,
			NotifyFilter = _notifyFilter
		};

		_fileSystemWatcher.Changed += FileSystemWatcher_Changed;
		_fileSystemWatcher.Created += FileSystemWatcher_Created;
		_fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
		_fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
	}

	public ObservableCollection<FileExplorerItemViewModel> RootNodes { get; } = [];

	public string Title => _localizationService["Title"];

	public bool IsEmpty => RootNodes.Count == 0;

	public string CommentPrefix
	{
		get => _commentPrefix;
		set => SetProperty(ref _commentPrefix, value ?? string.Empty);
	}

	public string ExcludedDirectoryFilter
	{
		get => _excludedDirectoryFilter;
		set
		{
			if (!SetProperty(ref _excludedDirectoryFilter, value ?? string.Empty))
				return;

			UpdateFileList();
		}
	}

	public string Filter
	{
		get => _filter;
		set
		{
			if (!SetProperty(ref _filter, string.IsNullOrWhiteSpace(value) ? "*.*" : value))
				return;

			UpdateFileList();
		}
	}

	public NotifyFilters NotifyFilter
	{
		get => _notifyFilter;
		set
		{
			if (!SetProperty(ref _notifyFilter, value))
				return;

			_fileSystemWatcher.NotifyFilter = value;
		}
	}

	public string RootDirectoryPath
	{
		get => _rootDirectoryPath;
		set
		{
			if (!SetProperty(ref _rootDirectoryPath, value ?? string.Empty))
				return;

			UpdateWatcherState();
			UpdateFileList();

			CreateNewFileRequestedCommand.NotifyCanExecuteChanged();
			CreateNewFolderRequestedCommand.NotifyCanExecuteChanged();
		}
	}

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(OpenSelectedItemCommand))]
	[NotifyCanExecuteChangedFor(nameof(OpenSourceViewCommand))]
	[NotifyCanExecuteChangedFor(nameof(RenameSelectedItemCommand))]
	[NotifyCanExecuteChangedFor(nameof(DeleteSelectedItemCommand))]
	[NotifyCanExecuteChangedFor(nameof(OpenInExplorerCommand))]
	private FileExplorerItemViewModel? _selectedItem;

	public event FileSystemEventHandler? FileChanged;
	public event FileSystemEventHandler? FileCreated;
	public event FileSystemEventHandler? FileDeleted;
	public event FileOpenedEventHandler? FileOpened;
	public event RenamedEventHandler? FileRenamed;

	public void Dispose()
	{
		_fileSystemWatcher.EnableRaisingEvents = false;
		_fileSystemWatcher.Changed -= FileSystemWatcher_Changed;
		_fileSystemWatcher.Created -= FileSystemWatcher_Created;
		_fileSystemWatcher.Deleted -= FileSystemWatcher_Deleted;
		_fileSystemWatcher.Renamed -= FileSystemWatcher_Renamed;
		_fileSystemWatcher.Dispose();
	}

	public void UpdateFileList()
	{
		string? selectedPath = SelectedItem?.FullPath;
		var expandedPaths = new HashSet<string>(EnumerateExpandedPaths(RootNodes), StringComparer.OrdinalIgnoreCase);

		RootNodes.Clear();

		if (string.IsNullOrWhiteSpace(RootDirectoryPath) || !Directory.Exists(RootDirectoryPath))
		{
			SelectedItem = null;
			OnPropertyChanged(nameof(IsEmpty));
			return;
		}

		var rootDirectory = new DirectoryInfo(RootDirectoryPath);
		FileExplorerItemViewModel rootNode = CreateDirectoryNode(rootDirectory, expandedPaths, true);
		RootNodes.Add(rootNode);
		OnPropertyChanged(nameof(IsEmpty));

		if (!string.IsNullOrWhiteSpace(selectedPath))
			SelectPath(selectedPath);
	}

}
