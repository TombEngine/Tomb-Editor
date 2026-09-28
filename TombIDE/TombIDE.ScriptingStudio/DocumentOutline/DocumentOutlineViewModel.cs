#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using DarkUI.Controls;
using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.DocumentOutline;

public sealed partial class DocumentOutlineViewModel : ObservableObject, IDisposable
{
	private readonly ILocalizationService _localizationService;
	private readonly ContentNodesRefreshCoordinator _refreshCoordinator;

	private ScriptingDocumentContext? _documentContext;

	internal DocumentOutlineViewModel(
		ILocalizationService localizationService)
	{
		ArgumentNullException.ThrowIfNull(localizationService);

		_localizationService = localizationService.WithKeysFor(this);
		_refreshCoordinator = new ContentNodesRefreshCoordinator();
	}

	public ObservableCollection<DocumentOutlineNodeViewModel> Nodes { get; } = [];

	public ITextDocumentSymbolProvider? NodesProvider { get; private set; }

	public string Title => _localizationService["Title"];

	public bool IsEmpty => Nodes.Count == 0;

	/// <summary>
	/// Gets or sets the document context the outline is bound to; the outline provider and the
	/// observed editor are resolved from it.
	/// </summary>
	public ScriptingDocumentContext? DocumentContext
	{
		get => _documentContext;
		set
		{
			if (ReferenceEquals(_documentContext, value))
				return;

			if (_documentContext?.Editor is { } previousEditor)
				previousEditor.ContentChangedWorkerRunCompleted -= EditorControl_ContentChangedWorkerRunCompleted;

			_documentContext = value;

			if (_documentContext?.Editor is { } editor)
				editor.ContentChangedWorkerRunCompleted += EditorControl_ContentChangedWorkerRunCompleted;

			UpdateNodesProvider();
		}
	}

	[ObservableProperty]
	private string _searchText = string.Empty;

	[ObservableProperty]
	private DocumentOutlineNodeViewModel? _selectedNode;

	partial void OnSelectedNodeChanged(DocumentOutlineNodeViewModel? value)
	{
		// Clear IsSelected on all nodes except the newly selected one.
		foreach (DocumentOutlineNodeViewModel node in Nodes)
			ClearSelectionExcept(node, value);
	}

	private static void ClearSelectionExcept(DocumentOutlineNodeViewModel node, DocumentOutlineNodeViewModel? except)
	{
		if (ReferenceEquals(node, except))
			return;

		node.IsSelected = false;

		foreach (DocumentOutlineNodeViewModel child in node.Children)
			ClearSelectionExcept(child, except);
	}

	public void Dispose()
	{
		_refreshCoordinator.InvalidatePendingRequests();

		if (_documentContext?.Editor is { } editor)
			editor.ContentChangedWorkerRunCompleted -= EditorControl_ContentChangedWorkerRunCompleted;
	}

	public bool SelectNode(string nodeText)
	{
		foreach (DocumentOutlineNodeViewModel node in Nodes)
			node.ClearSelection();

		foreach (DocumentOutlineNodeViewModel node in Nodes)
		{
			if (!node.TrySelect(nodeText))
				continue;

			SelectedNode = FindSelectedNode(node);
			return true;
		}

		SelectedNode = null;
		return false;
	}

	partial void OnSearchTextChanged(string value)
		=> RefreshNodes();

	private void ApplyNodes(IReadOnlyList<DarkTreeNode> nodes)
	{
		string? selectedNodeText = SelectedNode?.Text;

		Nodes.Clear();

		foreach (DarkTreeNode node in nodes)
			Nodes.Add(DocumentOutlineNodeViewModel.FromDarkTreeNode(node));

		if (!string.IsNullOrWhiteSpace(selectedNodeText) && !SelectNode(selectedNodeText))
			SelectedNode = null;

		OnPropertyChanged(nameof(IsEmpty));
	}

	private bool CanApplyRefreshResult(ITextDocumentSymbolProvider nodesProvider)
		=> ReferenceEquals(nodesProvider, NodesProvider);

	private void EditorControl_ContentChangedWorkerRunCompleted(object? sender, EventArgs e)
		=> RefreshNodes();

	private static DocumentOutlineNodeViewModel? FindSelectedNode(DocumentOutlineNodeViewModel node)
	{
		if (node.IsSelected)
			return node;

		foreach (DocumentOutlineNodeViewModel child in node.Children)
		{
			DocumentOutlineNodeViewModel? selectedNode = FindSelectedNode(child);

			if (selectedNode is not null)
				return selectedNode;
		}

		return null;
	}

	private void RefreshNodes()
	{
		if (_documentContext?.Editor is not { } editorControl || NodesProvider is null)
		{
			_refreshCoordinator.InvalidatePendingRequests();
			ApplyNodes([]);
			return;
		}

		string filter = string.IsNullOrWhiteSpace(SearchText)
			? string.Empty
			: SearchText.Trim();

		ITextDocumentSymbolProvider nodesProvider = NodesProvider;
		string content = editorControl.Content;

		_refreshCoordinator.RequestRefresh(nodesProvider, content, filter, CanApplyRefreshResult, ApplyNodes);
	}

	private void UpdateNodesProvider()
	{
		NodesProvider = _documentContext?.Registration?.Contributions.OutlineProviderFactory?.Invoke(_documentContext);
		_refreshCoordinator.InvalidatePendingRequests();

		RefreshNodes();
	}
}
