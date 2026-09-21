using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace TombLib.WPF;

public static class DependencyObjectExtensions
{
	public static T? FindVisualAncestor<T>(this DependencyObject? dependencyObject) where T : DependencyObject
	{
		DependencyObject? ancestor = dependencyObject;

		do
			ancestor = GetVisualParent(ancestor);
		while (ancestor is not null and not T);

		return ancestor as T;
	}

	public static T? FindVisualAncestorOrSelf<T>(this DependencyObject? dependencyObject) where T : DependencyObject
	{
		if (dependencyObject is T self)
			return self;

		return dependencyObject.FindVisualAncestor<T>();
	}

	public static T? FindAncestor<T>(this DependencyObject? dependencyObject) where T : DependencyObject
	{
		DependencyObject? ancestor = dependencyObject;

		do
			ancestor = GetParentElement(ancestor);
		while (ancestor is not null and not T);

		return ancestor as T;
	}

	public static T? FindAncestorOrSelf<T>(this DependencyObject? dependencyObject) where T : DependencyObject
	{
		if (dependencyObject is T self)
			return self;

		return dependencyObject.FindAncestor<T>();
	}

	public static T? FindVisualDescendant<T>(this DependencyObject? dependencyObject) where T : DependencyObject
	{
		if (dependencyObject is null)
			return null;

		if (dependencyObject is T self)
			return self;

		if (!HasVisualChildren(dependencyObject))
			return null;

		int childCount = VisualTreeHelper.GetChildrenCount(dependencyObject);

		for (int i = 0; i < childCount; i++)
		{
			T? descendant = VisualTreeHelper.GetChild(dependencyObject, i).FindVisualDescendant<T>();

			if (descendant is not null)
				return descendant;
		}

		return null;
	}

	public static bool IsDescendantOf(this DependencyObject? dependencyObject, DependencyObject? ancestor)
	{
		while (dependencyObject is not null)
		{
			if (ReferenceEquals(dependencyObject, ancestor))
				return true;

			dependencyObject = GetParentElement(dependencyObject);
		}

		return false;
	}

	private static DependencyObject? GetParentElement(DependencyObject? dependencyObject)
	{
		if (dependencyObject is null)
			return null;

		if (dependencyObject is FrameworkContentElement contentElement)
			return contentElement.Parent;

		DependencyObject? visualParent = GetVisualParent(dependencyObject);

		if (visualParent is not null)
			return visualParent;

		return LogicalTreeHelper.GetParent(dependencyObject);
	}

	private static DependencyObject? GetVisualParent(DependencyObject? dependencyObject)
	{
		if (dependencyObject is not Visual && dependencyObject is not Visual3D)
			return null;

		return VisualTreeHelper.GetParent(dependencyObject);
	}

	private static bool HasVisualChildren(DependencyObject dependencyObject)
		=> dependencyObject is Visual || dependencyObject is Visual3D;
}