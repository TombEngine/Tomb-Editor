#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TombIDE.ScriptingStudio.FileExplorer;

/// <summary>
/// Builds the file explorer node tree and handles file-system watcher events.
/// </summary>
public sealed partial class FileExplorerViewModel
{
	private FileExplorerItemViewModel CreateDirectoryNode(DirectoryInfo directoryInfo, ISet<string> expandedPaths, bool isRoot = false)
	{
		var directoryNode = new FileExplorerItemViewModel(directoryInfo.Name, directoryInfo.FullName, true)
		{
			IsExpanded = isRoot || expandedPaths.Contains(directoryInfo.FullName)
		};

		foreach (DirectoryInfo childDirectory in GetDirectories(directoryInfo))
			directoryNode.Children.Add(CreateDirectoryNode(childDirectory, expandedPaths));

		foreach (FileInfo file in GetFiles(directoryInfo))
			directoryNode.Children.Add(new FileExplorerItemViewModel(file.Name, file.FullName, false));

		return directoryNode;
	}

	private IEnumerable<string> EnumerateExpandedPaths(IEnumerable<FileExplorerItemViewModel> nodes)
	{
		foreach (FileExplorerItemViewModel node in nodes)
		{
			if (node.IsExpanded)
				yield return node.FullPath;

			foreach (string childPath in EnumerateExpandedPaths(node.Children))
				yield return childPath;
		}
	}

	private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
	{
		if (!ShouldHandlePath(e.FullPath))
			return;

		RunOnUiThread(() => FileChanged?.Invoke(this, e));
	}

	private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
	{
		if (!ShouldHandlePath(e.FullPath))
			return;

		RunOnUiThread(() =>
		{
			UpdateFileList();
			FileCreated?.Invoke(this, e);
		});
	}

	private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
	{
		if (!ShouldHandlePath(e.FullPath))
			return;

		RunOnUiThread(() =>
		{
			UpdateFileList();
			FileDeleted?.Invoke(this, e);
		});
	}

	private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
	{
		if (!ShouldHandlePath(e.FullPath) && !ShouldHandlePath(e.OldFullPath))
			return;

		RunOnUiThread(() =>
		{
			UpdateFileList();
			FileRenamed?.Invoke(this, e);
		});
	}

	private IEnumerable<DirectoryInfo> GetDirectories(DirectoryInfo directoryInfo)
	{
		DirectoryInfo[] directories;

		try
		{
			directories = directoryInfo.GetDirectories();
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			yield break;
		}

		foreach (DirectoryInfo directory in directories.OrderBy(static directory => directory.Name, StringComparer.OrdinalIgnoreCase))
		{
			if (IsExcludedDirectory(directory.FullName))
				continue;

			yield return directory;
		}
	}

	private IEnumerable<FileInfo> GetFiles(DirectoryInfo directoryInfo)
	{
		if (string.Equals(Filter, "*.*", StringComparison.Ordinal))
		{
			FileInfo[] allFiles;

			try
			{
				allFiles = directoryInfo.GetFiles();
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				yield break;
			}

			foreach (FileInfo file in allFiles.OrderBy(static file => file.Name, StringComparer.OrdinalIgnoreCase))
				yield return file;

			yield break;
		}

		var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (string pattern in GetFilterPatterns())
		{
			FileInfo[] files;

			try
			{
				files = directoryInfo.GetFiles(pattern);
			}
			catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
			{
				continue;
			}

			foreach (FileInfo file in files.OrderBy(static file => file.Name, StringComparer.OrdinalIgnoreCase))
			{
				if (seenPaths.Add(file.FullName))
					yield return file;
			}
		}
	}

	private IEnumerable<string> GetFilterPatterns()
	{
		return (string.IsNullOrWhiteSpace(Filter) ? "*.*" : Filter)
			.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
	}

	private bool IsExcludedDirectory(string fullPath)
	{
		return !string.IsNullOrWhiteSpace(ExcludedDirectoryFilter)
			&& fullPath.EndsWith(ExcludedDirectoryFilter, StringComparison.OrdinalIgnoreCase);
	}

	private bool IsExcludedPath(string fullPath)
	{
		return !string.IsNullOrWhiteSpace(ExcludedDirectoryFilter)
			&& fullPath.Contains(ExcludedDirectoryFilter, StringComparison.OrdinalIgnoreCase);
	}

	private bool IsSupportedFileFormat(string fullPath)
	{
		if (string.Equals(Filter, "*.*", StringComparison.Ordinal))
			return true;

		string extension = Path.GetExtension(fullPath);

		foreach (string pattern in GetFilterPatterns())
		{
			if (string.Equals(pattern, "*.*", StringComparison.Ordinal))
				return true;

			if (pattern.StartsWith("*.", StringComparison.Ordinal)
				&& string.Equals(extension, pattern[1..], StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	private void SelectPath(string fullPath)
	{
		foreach (FileExplorerItemViewModel node in RootNodes)
			node.ClearSelection();

		foreach (FileExplorerItemViewModel node in RootNodes)
		{
			if (!node.TrySelectPath(fullPath))
				continue;

			SelectedItem = FindSelectedItem(node);
			return;
		}

		SelectedItem = null;
	}

	private bool ShouldHandlePath(string fullPath)
	{
		if (IsExcludedPath(fullPath))
			return false;

		if (Directory.Exists(fullPath))
			return true;

		return !Path.HasExtension(fullPath) || IsSupportedFileFormat(fullPath);
	}

	private void UpdateWatcherState()
	{
		_fileSystemWatcher.EnableRaisingEvents = false;

		if (string.IsNullOrWhiteSpace(RootDirectoryPath) || !Directory.Exists(RootDirectoryPath))
			return;

		_fileSystemWatcher.Path = RootDirectoryPath;
		_fileSystemWatcher.NotifyFilter = NotifyFilter;
		_fileSystemWatcher.EnableRaisingEvents = true;
	}

	private static FileExplorerItemViewModel? FindSelectedItem(FileExplorerItemViewModel node)
	{
		if (node.IsSelected)
			return node;

		foreach (FileExplorerItemViewModel child in node.Children)
		{
			FileExplorerItemViewModel? selectedItem = FindSelectedItem(child);

			if (selectedItem is not null)
				return selectedItem;
		}

		return null;
	}
}
