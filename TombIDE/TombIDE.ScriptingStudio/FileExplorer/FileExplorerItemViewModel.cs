#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace TombIDE.ScriptingStudio.FileExplorer;

public sealed partial class FileExplorerItemViewModel : ObservableObject
{
	public FileExplorerItemViewModel(string name, string fullPath, bool isDirectory)
	{
		Name = name;
		FullPath = fullPath;
		IsDirectory = isDirectory;
	}

	public ObservableCollection<FileExplorerItemViewModel> Children { get; } = [];

	public string FullPath { get; }

	public bool IsDirectory { get; }

	public bool IsFile => !IsDirectory;

	public string Name { get; }

	[ObservableProperty]
	private bool _isExpanded;

	[ObservableProperty]
	private bool _isSelected;

	public void ClearSelection()
	{
		IsSelected = false;

		foreach (FileExplorerItemViewModel child in Children)
			child.ClearSelection();
	}

	public bool TrySelectPath(string fullPath)
	{
		if (string.Equals(FullPath, fullPath, StringComparison.OrdinalIgnoreCase))
		{
			IsSelected = true;
			return true;
		}

		foreach (FileExplorerItemViewModel child in Children)
		{
			if (!child.TrySelectPath(fullPath))
				continue;

			IsExpanded = true;
			return true;
		}

		return false;
	}
}
