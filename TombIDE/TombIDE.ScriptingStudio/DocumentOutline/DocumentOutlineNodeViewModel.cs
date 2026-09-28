#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using DarkUI.Controls;
using System;
using System.Collections.ObjectModel;
using Nickelony.IDEKit.IntelliSense.Navigation;

namespace TombIDE.ScriptingStudio.DocumentOutline;

public sealed partial class DocumentOutlineNodeViewModel : ObservableObject
{
	public DocumentOutlineNodeViewModel(string text, TextDefinitionDiscriminator? identifyingObject)
	{
		Text = text;
		IdentifyingObject = identifyingObject;
	}

	public ObservableCollection<DocumentOutlineNodeViewModel> Children { get; } = [];

	public TextDefinitionDiscriminator? IdentifyingObject { get; }

	public string Text { get; }

	[ObservableProperty]
	private bool _isExpanded;

	[ObservableProperty]
	private bool _isSelected;

	public static DocumentOutlineNodeViewModel FromDarkTreeNode(DarkTreeNode node)
	{
		ArgumentNullException.ThrowIfNull(node);

		var viewModel = new DocumentOutlineNodeViewModel(node.Text, node.Tag as TextDefinitionDiscriminator)
		{
			IsExpanded = node.Expanded
		};

		foreach (DarkTreeNode childNode in node.Nodes)
			viewModel.Children.Add(FromDarkTreeNode(childNode));

		return viewModel;
	}

	public void ClearSelection()
	{
		IsSelected = false;

		foreach (DocumentOutlineNodeViewModel child in Children)
			child.ClearSelection();
	}

	public bool TrySelect(string nodeText)
	{
		string normalizedText = NormalizeNodeText(nodeText);

		if (string.Equals(Text, normalizedText, StringComparison.Ordinal))
		{
			IsSelected = true;
			return true;
		}

		foreach (DocumentOutlineNodeViewModel child in Children)
		{
			if (!child.TrySelect(normalizedText))
				continue;

			IsExpanded = true;
			return true;
		}

		return false;
	}

	private static string NormalizeNodeText(string nodeText)
		=> (nodeText ?? string.Empty).Trim().Trim('[').Trim(']');
}
