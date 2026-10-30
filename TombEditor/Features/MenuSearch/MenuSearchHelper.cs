#nullable enable

using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TombEditor.Features.MenuSearch;

/// <summary>Finds a menu entry by its text and opens the menus leading to it.</summary>
public static class MenuSearchHelper
{
    /// <summary>
    /// Opens the submenu containing the first enabled entry whose text contains the request and
    /// highlights that entry. Returns the menu that was opened, or null when nothing matches.
    /// </summary>
    public static MenuItem? FindAndOpen(ItemsControl items, string request, MenuItem? parent = null)
    {
        if (string.IsNullOrEmpty(request))
            return null;

        MenuItem? result = null;
        MenuItem? match = null;

        foreach (var item in items.Items)
        {
            if (item is not MenuItem menuItem)
                continue;

            if (menuItem.HasItems)
            {
                result = FindAndOpen(menuItem, request, menuItem);
                if (result is not null)
                    break;
            }
            else if (menuItem.IsEnabled && GetText(menuItem).Contains(request, StringComparison.InvariantCultureIgnoreCase))
            {
                match = menuItem;
                result = parent;
                break;
            }
        }

        if (result is null)
            return null;

        result.IsSubmenuOpen = true;

        if (match is not null)
            match.Dispatcher.BeginInvoke(() => match.Focus(), DispatcherPriority.Input);

        return parent;
    }

    private static string GetText(MenuItem item) => (item.Header?.ToString() ?? string.Empty).Replace("_", string.Empty);
}