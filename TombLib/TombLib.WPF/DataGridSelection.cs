#nullable enable

using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TombLib.WPF;

/// <summary>Attached behavior that mirrors <see cref="DataGrid.SelectedItems"/> into a bindable list.</summary>
public static class DataGridSelection
{
    public static readonly DependencyProperty SelectedItemsListProperty = DependencyProperty.RegisterAttached(
        "SelectedItemsList", typeof(IList), typeof(DataGridSelection),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemsListChanged));

    public static IList? GetSelectedItemsList(DependencyObject element) => (IList?)element.GetValue(SelectedItemsListProperty);

    public static void SetSelectedItemsList(DependencyObject element, IList? value) => element.SetValue(SelectedItemsListProperty, value);

    private static void OnSelectedItemsListChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not DataGrid grid)
            return;

        // Subscribe once; the handler republishes a snapshot of the selection whenever it changes.
        grid.SelectionChanged -= OnGridSelectionChanged;
        grid.SelectionChanged += OnGridSelectionChanged;

        if (e.NewValue is null)
            Publish(grid);
    }

    private static void OnGridSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid grid)
            Publish(grid);
    }

    private static void Publish(DataGrid grid) => SetSelectedItemsList(grid, grid.SelectedItems.Cast<object>().ToList());
}
