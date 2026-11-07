using System.Windows;
using System.Windows.Controls;

namespace DarkUI.WPF.CustomControls;

/// <summary>
/// A <see cref="TreeView"/> that generates <see cref="AlternatingTreeViewItem"/> containers,
/// so data-bound trees (ItemsSource + HierarchicalDataTemplate) pick up the DarkUI item style.
/// </summary>
public class AlternatingTreeView : TreeView
{
	protected override DependencyObject GetContainerForItemOverride() => new AlternatingTreeViewItem();
	protected override bool IsItemItsOwnContainerOverride(object item) => item is AlternatingTreeViewItem;
}
