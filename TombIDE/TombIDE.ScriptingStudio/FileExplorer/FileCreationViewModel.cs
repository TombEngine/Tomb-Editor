#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.FileExplorer;

/// <summary>
/// Represents a directory node in the file creation tree.
/// </summary>
public partial class FileCreationDirectoryNode : ObservableObject
{
	public string Name { get; }
	public string FullPath { get; }
	public ObservableCollection<FileCreationDirectoryNode> Children { get; } = [];

	[ObservableProperty]
	private bool _isExpanded;

	[ObservableProperty]
	private bool _isSelected;

	public FileCreationDirectoryNode(string name, string fullPath)
	{
		Name = name;
		FullPath = fullPath;
	}
}

/// <summary>
/// ViewModel for the File Creation / Save As dialog.
/// </summary>
public partial class FileCreationViewModel : ObservableObject
{
	private readonly string _scriptRootFolderPath;
	private readonly string _defaultFileExtension;
	private readonly string[] _ignoredNodePaths;

	[ObservableProperty]
	private string _title = "Creating New File...";

	[ObservableProperty]
	private string _actionLabel = "Where to Create:";

	[ObservableProperty]
	private string _actionButtonText = "Create";

	[ObservableProperty]
	private string _newFileName = "untitled";

	[ObservableProperty]
	private int _selectedFormatIndex;

	[ObservableProperty]
	private FileCreationDirectoryNode? _selectedNode;

	public ObservableCollection<FileCreationDirectoryNode> RootNodes { get; } = [];

	/// <summary>
	/// The resulting file path after the dialog is accepted.
	/// </summary>
	public string? NewFilePath { get; private set; }

	/// <summary>
	/// Raised when the dialog should be accepted (equivalent to DialogResult.OK).
	/// </summary>
	public event Action? RequestAccept;

	/// <summary>
	/// Raised when the dialog should be cancelled.
	/// </summary>
	public event Action? RequestCancel;

	public FileCreationViewModel(
		string scriptRootFolderPath,
		FileCreationMode mode,
		string defaultFileExtension,
		string? initialNodePath = null,
		string? initialFileName = null,
		params string[] ignoredNodePaths)
	{
		_scriptRootFolderPath = scriptRootFolderPath;
		_defaultFileExtension = defaultFileExtension;
		_ignoredNodePaths = ignoredNodePaths;

		SwitchMode(mode);
		PopulateTree();
		SelectInitialNode(initialNodePath);
		SetInitialFileName(initialFileName);

		SelectedFormatIndex = defaultFileExtension.ToLowerInvariant() switch
		{
			".json5" => 1,
			".lua" => 2,
			_ => 0
		};
	}

	private void SwitchMode(FileCreationMode mode)
	{
		switch (mode)
		{
			case FileCreationMode.New:
				Title = "Creating New File...";
				ActionLabel = "Where to Create:";
				ActionButtonText = "Create";
				break;

			case FileCreationMode.SavingAs:
				Title = "Saving As...";
				ActionLabel = "Where to Save:";
				ActionButtonText = "Save";
				break;
		}
	}

	private void PopulateTree()
	{
		RootNodes.Clear();

		if (!Directory.Exists(_scriptRootFolderPath))
			return;

		var rootDir = new DirectoryInfo(_scriptRootFolderPath);
		var rootNode = BuildDirectoryNode(rootDir);
		RootNodes.Add(rootNode);

		// Remove ignored nodes.
		foreach (string ignoredPath in _ignoredNodePaths.Where(p => !string.IsNullOrWhiteSpace(p)))
		{
			RemoveNodeByPath(rootNode, ignoredPath);
		}
	}

	private static FileCreationDirectoryNode BuildDirectoryNode(DirectoryInfo directory)
	{
		var node = new FileCreationDirectoryNode(directory.Name, directory.FullName);

		try
		{
			foreach (DirectoryInfo subDir in directory.GetDirectories())
			{
				FileCreationDirectoryNode childNode = BuildDirectoryNode(subDir);
				node.Children.Add(childNode);
			}
		}
		catch (UnauthorizedAccessException)
		{
			// Skip inaccessible directories.
		}

		return node;
	}

	private static bool RemoveNodeByPath(FileCreationDirectoryNode parent, string path)
	{
		for (int i = parent.Children.Count - 1; i >= 0; i--)
		{
			FileCreationDirectoryNode child = parent.Children[i];

			if (child.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase))
			{
				parent.Children.RemoveAt(i);
				return true;
			}

			if (RemoveNodeByPath(child, path))
				return true;
		}

		return false;
	}

	private void SelectInitialNode(string? initialNodePath)
	{
		if (RootNodes.Count == 0)
			return;

		FileCreationDirectoryNode root = RootNodes[0];

		if (string.IsNullOrEmpty(initialNodePath))
		{
			root.IsSelected = true;
			root.IsExpanded = true;
			SelectedNode = root;
			return;
		}

		FileCreationDirectoryNode? targetNode = FindNodeByPath(root, initialNodePath);

		if (targetNode is not null)
		{
			targetNode.IsSelected = true;
			targetNode.IsExpanded = true;
			SelectedNode = targetNode;
		}
		else
		{
			root.IsSelected = true;
			root.IsExpanded = true;
			SelectedNode = root;
		}
	}

	private static FileCreationDirectoryNode? FindNodeByPath(FileCreationDirectoryNode parent, string path)
	{
		if (parent.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase))
			return parent;

		foreach (FileCreationDirectoryNode child in parent.Children)
		{
			FileCreationDirectoryNode? result = FindNodeByPath(child, path);

			if (result is not null)
				return result;
		}

		return null;
	}

	private void SetInitialFileName(string? initialFileName)
	{
		if (!string.IsNullOrEmpty(initialFileName))
			NewFileName = initialFileName;
	}

	private string GetSelectedExtension() => SelectedFormatIndex switch
	{
		0 => ".txt",
		1 => ".json5",
		2 => ".lua",
		_ => ".txt"
	};

	// -- Commands --

	[RelayCommand]
	private void Accept()
	{
		try
		{
			string newFileName = RemoveIllegalPathSymbols(NewFileName).Trim();

			if (string.IsNullOrWhiteSpace(newFileName))
				throw new ArgumentException("Invalid file name.");

			newFileName += GetSelectedExtension();

			if (SelectedNode is null)
				throw new ArgumentException("No target directory selected.");

			string targetDirectory = SelectedNode.FullPath;

			if (Directory.GetFiles(targetDirectory, "*.*", SearchOption.TopDirectoryOnly)
				.Any(file => newFileName.Equals(Path.GetFileName(file), StringComparison.OrdinalIgnoreCase)))
			{
				throw new ArgumentException("A file with the same name already exists in that directory.");
			}

			NewFilePath = Path.Combine(targetDirectory, newFileName);

			RequestAccept?.Invoke();
		}
		catch (Exception)
		{
			// Errors are displayed by the caller (EditorDocumentController).
			// We just don't accept.
		}
	}

	[RelayCommand]
	private void Cancel()
	{
		NewFilePath = null;
		RequestCancel?.Invoke();
	}

	/// <summary>
	/// Removes illegal path symbols from a file name.
	/// </summary>
	private static string RemoveIllegalPathSymbols(string fileName)
	{
		char[] invalidChars = Path.GetInvalidFileNameChars();
		return new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
	}
}
