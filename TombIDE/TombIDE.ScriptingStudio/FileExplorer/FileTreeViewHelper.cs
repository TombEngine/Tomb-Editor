using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using TombIDE.ScriptingStudio.Properties;

namespace TombIDE.ScriptingStudio.FileExplorer;

internal static class FileTreeViewHelper
{
	#region Public methods

	public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath)
		=> CreateFullFileListNode(sourceDirectoryPath, string.Empty, false, null, false, string.Empty);

	public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, string fileSearchPattern)
		=> CreateFullFileListNode(sourceDirectoryPath, fileSearchPattern, false, null, false, string.Empty);

	public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, string fileSearchPattern, bool expandAllNodes)
		=> CreateFullFileListNode(sourceDirectoryPath, fileSearchPattern, false, null, expandAllNodes, string.Empty);

	public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, string fileSearchPattern, DarkTreeView expandedSourceTreeView)
		=> CreateFullFileListNode(sourceDirectoryPath, fileSearchPattern, false, expandedSourceTreeView, false, string.Empty);

	public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, bool directoriesOnly)
		=> CreateFullFileListNode(sourceDirectoryPath, string.Empty, directoriesOnly, null, false, string.Empty);

	public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, bool directoriesOnly, bool expandAllNodes)
		=> CreateFullFileListNode(sourceDirectoryPath, string.Empty, directoriesOnly, null, expandAllNodes, string.Empty);

	public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, bool directoriesOnly, DarkTreeView expandedSourceTreeView)
		=> CreateFullFileListNode(sourceDirectoryPath, string.Empty, directoriesOnly, expandedSourceTreeView, false, string.Empty);

	#endregion Public methods

	#region Private methods

	public static DarkTreeNode CreateFullFileListNode(string sourceDirectoryPath, string fileSearchPattern, bool directoriesOnly, DarkTreeView expandedSourceTreeView, bool expandAllNodes, string excludedDirectoryFilter)
	{
		var sourceDirectory = new DirectoryInfo(sourceDirectoryPath);

		var node = new DarkTreeNode(sourceDirectory.Name)
		{
			Icon = Resources.Folder_16,
			Tag = sourceDirectory
		};

		FillNodeWithItems(node, fileSearchPattern, directoriesOnly, expandedSourceTreeView, expandAllNodes, excludedDirectoryFilter);

		if (expandAllNodes)
			node.Expanded = true;

		return node;
	}

	private static void FillNodeWithItems(DarkTreeNode node, string fileSearchPattern, bool directoriesOnly, DarkTreeView expandedSourceTreeView, bool expandAllNodes, string excludedDirectoryFilter)
	{
		var stack = new Stack<DarkTreeNode>();
		stack.Push(node);

		while (stack.Count > 0)
		{
			DarkTreeNode currentNode = stack.Pop();
			var info = currentNode.Tag as DirectoryInfo;

			currentNode.Nodes.AddRange(GetDirectoryNodes(stack, info.GetDirectories(), expandedSourceTreeView, expandAllNodes, excludedDirectoryFilter));

			if (!directoriesOnly)
			{
				foreach (string filter in fileSearchPattern.Split('|'))
					currentNode.Nodes.AddRange(GetFileNodes(info.GetFiles(filter)));
			}

			if (expandedSourceTreeView != null)
			{
				DarkTreeNode expandedSourceNode = expandedSourceTreeView.FindNode(currentNode.FullPath);

				if (expandedSourceNode != null)
					currentNode.Expanded = expandedSourceNode.Expanded;
			}
			else if (expandAllNodes)
			{
				currentNode.Expanded = true;
			}
		}
	}

	private static IEnumerable<DarkTreeNode> GetDirectoryNodes(Stack<DarkTreeNode> loopStack, DirectoryInfo[] directories, DarkTreeView expandedSourceTreeView, bool expandAllNodes, string excludedDirectoryFilter)
	{
		foreach (DirectoryInfo directory in directories)
		{
			if (!string.IsNullOrEmpty(excludedDirectoryFilter) && directory.FullName.EndsWith(excludedDirectoryFilter, StringComparison.OrdinalIgnoreCase))
				continue;

			var childDirectoryNode = new DarkTreeNode(directory.Name)
			{
				Icon = Resources.Folder_16,
				Tag = directory
			};

			if (expandedSourceTreeView != null)
			{
				DarkTreeNode expandedSourceChildNode = expandedSourceTreeView.FindNode(childDirectoryNode.FullPath);

				if (expandedSourceChildNode != null)
					childDirectoryNode.Expanded = expandedSourceChildNode.Expanded;
			}
			else if (expandAllNodes)
			{
				childDirectoryNode.Expanded = true;
			}

			loopStack.Push(childDirectoryNode);
			yield return childDirectoryNode;
		}
	}

	private static IEnumerable<DarkTreeNode> GetFileNodes(FileInfo[] files)
	{
		foreach (FileInfo file in files)
		{
			yield return new DarkTreeNode(file.Name)
			{
				Icon = Resources.New_16,
				Tag = file
			};
		}
	}

	#endregion Private methods
}
