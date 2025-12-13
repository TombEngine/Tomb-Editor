using System.Windows;
using System.Windows.Media;

namespace DarkUI.WPF.Extensions;

public static class DependencyObjectExtensions
{
	public static T? FindVisualAncestor<T>(this DependencyObject dependencyObject) where T : class
	{
		DependencyObject ancestor = dependencyObject;

		do
			ancestor = VisualTreeHelper.GetParent(ancestor);
		while (ancestor is not null and not T);

		return ancestor as T;
	}

	public static T? FindLogicalAncestor<T>(this DependencyObject dependencyObject) where T : class
	{
		DependencyObject ancestor = dependencyObject;

		do
		{
			DependencyObject reference = ancestor;
			ancestor = LogicalTreeHelper.GetParent(ancestor) ?? VisualTreeHelper.GetParent(reference);
		}
		while (ancestor is not null and not T);

		return ancestor as T;
	}
}
