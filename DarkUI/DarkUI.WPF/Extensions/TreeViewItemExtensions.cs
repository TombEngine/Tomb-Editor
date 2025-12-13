using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DarkUI.WPF.Extensions;

public static class TreeViewItemExtensions
{
	public static int GetDepth(this TreeViewItem item)
	{
		try
		{
			TreeViewItem? parent = GetParentTreeViewItem(item);

			while (parent is not null)
				return GetDepth(parent) + 1;
		}
		catch { }

		return 0;
	}

	public static TreeViewItem? GetParentTreeViewItem(TreeViewItem item)
	{
		DependencyObject parent = VisualTreeHelper.GetParent(item);

		while (parent is not (TreeViewItem or TreeView))
			parent = VisualTreeHelper.GetParent(parent ?? throw new InvalidOperationException());

		return parent as TreeViewItem;
	}
}
