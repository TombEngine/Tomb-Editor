#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace TombIDE.ScriptingStudio.DocumentOutline;

public partial class DocumentOutlineView : UserControl
{
	private bool _isSynchronizingSelection;
	private DocumentOutlineNodeViewModel? _pendingNodeInvoke;

	public DocumentOutlineView()
		=> InitializeComponent();

	public event EventHandler<DocumentOutlineNodeViewModel>? NodeInvoked;

	public void SelectNode(string nodeText)
	{
		if (!TrySynchronizeSelection(nodeText))
			return;

		Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(SynchronizeSelectedNodeContainer));
	}

	private void BringSelectedNodeIntoView()
	{
		if (TryFindSelectedContainer(treeView, out TreeViewItem? container))
			container.BringIntoView();
	}

	private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		if (e.NewValue is DocumentOutlineNodeViewModel node)
		{
			if (!_isSynchronizingSelection && ViewModel is not null)
			{
				ViewModel.SelectedNode = node;
				SynchronizeSelectedNodeContainer();
			}

			// Defer NodeInvoked to the next dispatch frame so that rapid
			// parent-then-child selection changes only trigger the final
			// (child) node, preventing spurious navigation to the parent.
			_pendingNodeInvoke = node;
			Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(FlushNodeInvoke));
		}
	}

	private void FlushNodeInvoke()
	{
		DocumentOutlineNodeViewModel? node = _pendingNodeInvoke;
		_pendingNodeInvoke = null;

		if (node is not null)
			NodeInvoked?.Invoke(this, node);
	}

	private DocumentOutlineViewModel? ViewModel => DataContext as DocumentOutlineViewModel;

	private bool TrySynchronizeSelection(string nodeText)
	{
		if (ViewModel is null)
			return false;

		if (_isSynchronizingSelection)
			return false;

		_isSynchronizingSelection = true;

		try
		{
			return ViewModel.SelectNode(nodeText);
		}
		finally
		{
			_isSynchronizingSelection = false;
		}
	}

	private void SynchronizeSelectedNodeContainer()
	{
		if (ViewModel?.SelectedNode is not DocumentOutlineNodeViewModel selectedNode)
			return;

		if (!TryFindContainerForItem(treeView, selectedNode, out TreeViewItem? container))
			return;

		_isSynchronizingSelection = true;

		try
		{
			DeselectAncestorContainers(container);

			if (!container.IsSelected)
				container.IsSelected = true;

			container.BringIntoView();
		}
		finally
		{
			_isSynchronizingSelection = false;
		}
	}

	private static void DeselectAncestorContainers(TreeViewItem container)
	{
		DependencyObject? current = VisualTreeHelper.GetParent(container);

		while (current is not null)
		{
			if (current is TreeViewItem ancestor)
				ancestor.IsSelected = false;

			current = VisualTreeHelper.GetParent(current);
		}
	}

	private static bool TryFindContainerForItem(ItemsControl parent, object item, [NotNullWhen(true)] out TreeViewItem? container)
	{
		if (parent.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem directContainer)
		{
			container = directContainer;
			return true;
		}

		foreach (object childItem in parent.Items)
		{
			if (parent.ItemContainerGenerator.ContainerFromItem(childItem) is not TreeViewItem childContainer)
				continue;

			if (TryFindContainerForItem(childContainer, item, out container))
				return true;
		}

		container = null;
		return false;
	}

	private static bool TryFindSelectedContainer(ItemsControl parent, [NotNullWhen(true)] out TreeViewItem? selectedContainer)
	{
		foreach (object item in parent.Items)
		{
			if (parent.ItemContainerGenerator.ContainerFromItem(item) is not TreeViewItem container)
				continue;

			if (container.IsSelected)
			{
				selectedContainer = container;
				return true;
			}

			if (TryFindSelectedContainer(container, out selectedContainer))
				return true;
		}

		selectedContainer = null;
		return false;
	}
}
