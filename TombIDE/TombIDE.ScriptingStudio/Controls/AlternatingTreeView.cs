#nullable enable

using System.Windows;
using System.Windows.Controls;

namespace TombIDE.ScriptingStudio.Controls;

public sealed class AlternatingTreeView : TreeView
{
	protected override DependencyObject GetContainerForItemOverride() => new AlternatingTreeViewItem();

	protected override bool IsItemItsOwnContainerOverride(object item)
		=> item is AlternatingTreeViewItem;
}
