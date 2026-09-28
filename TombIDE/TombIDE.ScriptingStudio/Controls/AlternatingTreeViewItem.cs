#nullable enable

using System.Windows;

namespace TombIDE.ScriptingStudio.Controls;

public class AlternatingTreeViewItem : DarkUI.WPF.CustomControls.AlternatingTreeViewItem
{
	protected override DependencyObject GetContainerForItemOverride() => new AlternatingTreeViewItem();

	protected override bool IsItemItsOwnContainerOverride(object item)
		=> item is AlternatingTreeViewItem;
}
