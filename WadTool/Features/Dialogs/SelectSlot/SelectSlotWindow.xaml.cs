#nullable enable

using System;
using System.ComponentModel;
using System.Windows;
using TombLib.WPF;

namespace WadTool.Features.Dialogs.SelectSlot;

public partial class SelectSlotWindow : Window
{
    public SelectSlotWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    // Match the legacy DarkListView.SelectItem() behaviour, which also ensured visibility:
    // scroll the current selection into view on first show and on each selection change
    // (e.g. when the user types an ID into the numeric field).
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ScrollToSelection();
        if (DataContext is INotifyPropertyChanged vm)
        {
            // Unsubscribe first so a repeated Loaded never double-subscribes.
            vm.PropertyChanged -= OnVmPropertyChanged;
            vm.PropertyChanged += OnVmPropertyChanged;
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is INotifyPropertyChanged vm)
            vm.PropertyChanged -= OnVmPropertyChanged;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectSlotWindowViewModel.SelectedSlot))
            ScrollToSelection();
    }

    private void ScrollToSelection()
    {
        if (slotsList.SelectedItem is not null)
            slotsList.ScrollIntoView(slotsList.SelectedItem);
    }

    // Like the legacy lstSlots_MouseDoubleClick: a double-click anywhere in the list confirms.
    private void OnSlotsListMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is SelectSlotWindowViewModel vm)
            vm.ConfirmCommand.Execute(null);
    }
}
