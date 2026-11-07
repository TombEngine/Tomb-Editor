using System.Windows;
using System.Windows.Media;

namespace TombLib.WPF;

/// <summary>
/// Helpers for walking the WPF visual tree.
/// </summary>
public static class VisualTreeExtensions
{
    /// <summary>
    /// Returns the closest ancestor of the given type (the element itself is not considered), or null.
    /// </summary>
    public static T? FindAncestor<T>(this DependencyObject element) where T : DependencyObject
    {
        var current = VisualTreeHelper.GetParent(element);

        while (current is not null)
        {
            if (current is T match)
                return match;

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}