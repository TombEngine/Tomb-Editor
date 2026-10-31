#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TombEditor.Features.Dialogs.LevelSettings;

/// <summary>Removes and reorders all selected rows of a grid collection, like the multi-row buttons of the previous dialog.</summary>
public static class SelectionListOperations
{
    /// <summary>Removes every selected row from the list.</summary>
    public static void Remove<T>(ObservableCollection<T> list, IList? selection) where T : class
    {
        foreach (var item in GetSelectedItems<T>(selection))
            list.Remove(item);
    }

    /// <summary>Moves every selected row one position up; rows already at the top stay in place.</summary>
    public static void MoveUp<T>(ObservableCollection<T> list, IList? selection) where T : class
    {
        var indices = GetSelectedIndices(list, selection).OrderBy(index => index).ToList();
        int lastItemIndex = 0;

        foreach (int index in indices)
        {
            if (index == lastItemIndex++)
                continue;

            list.Move(index, index - 1);
        }
    }

    /// <summary>Moves every selected row one position down; rows already at the bottom stay in place.</summary>
    public static void MoveDown<T>(ObservableCollection<T> list, IList? selection) where T : class
    {
        var indices = GetSelectedIndices(list, selection).OrderByDescending(index => index).ToList();
        int lastItemIndex = list.Count - 1;

        foreach (int index in indices)
        {
            if (index == lastItemIndex--)
                continue;

            list.Move(index, index + 1);
        }
    }

    private static List<T> GetSelectedItems<T>(IList? selection) where T : class
        => selection?.OfType<T>().ToList() ?? [];

    private static IEnumerable<int> GetSelectedIndices<T>(ObservableCollection<T> list, IList? selection) where T : class
        => GetSelectedItems<T>(selection).Select(list.IndexOf).Where(index => index >= 0);
}
