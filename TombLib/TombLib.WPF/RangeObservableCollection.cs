using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace TombLib.WPF;

/// <summary>
/// An observable collection that can be replaced in one step, raising a single reset notification
/// instead of one event per item.
/// </summary>
public class RangeObservableCollection<T> : ObservableCollection<T>
{
    /// <summary>Replaces the whole content of the collection with the given items.</summary>
    public void ReplaceAll(IEnumerable<T> items)
    {
        CheckReentrancy();

        Items.Clear();

        foreach (var item in items)
            Items.Add(item);

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
