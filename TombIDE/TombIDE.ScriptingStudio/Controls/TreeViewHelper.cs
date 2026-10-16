#nullable enable

using System.Windows;
using System.Windows.Controls;

namespace TombIDE.ScriptingStudio.Controls;

/// <summary>
/// Provides an attached property that enables two-way binding to <see cref="TreeView.SelectedItem"/>.
/// </summary>
public static class TreeViewHelper
{
	public static readonly DependencyProperty SelectedItemProperty =
		DependencyProperty.RegisterAttached(
			"SelectedItem",
			typeof(object),
			typeof(TreeViewHelper),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

	public static object GetSelectedItem(TreeView treeView)
		=> treeView.GetValue(SelectedItemProperty);

	public static void SetSelectedItem(TreeView treeView, object value)
		=> treeView.SetValue(SelectedItemProperty, value);

	private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not TreeView treeView)
			return;

		treeView.SelectedItemChanged -= TreeView_SelectedItemChanged;
		treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
	}

	private static void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		if (sender is TreeView treeView)
			SetSelectedItem(treeView, e.NewValue);
	}
}
