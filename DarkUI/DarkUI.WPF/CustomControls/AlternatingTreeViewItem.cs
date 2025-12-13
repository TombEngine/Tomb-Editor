using DarkUI.WPF.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace DarkUI.WPF.CustomControls;

public class AlternatingTreeViewItem : TreeViewItem
{
	public AlternatingTreeViewItem()
	{
		Loaded += TreeViewItemEx_Changed;
		Expanded += TreeViewItemEx_Changed;
		Collapsed += TreeViewItemEx_Changed;
	}

	private void TreeViewItemEx_Changed(object sender, RoutedEventArgs e)
	{
		TreeView? treeViewAncestor = this.FindVisualAncestor<TreeView>();
		treeViewAncestor?.SetAlternationIndexRecursively(0);
	}
}
