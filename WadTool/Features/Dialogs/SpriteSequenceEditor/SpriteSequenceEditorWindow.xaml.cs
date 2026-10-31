#nullable enable

using System;
using System.ComponentModel;
using System.Windows;
using TombLib.WPF;

namespace WadTool.Features.Dialogs.SpriteSequenceEditor;

public partial class SpriteSequenceEditorWindow : Window
{
    public SpriteSequenceEditorWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    // The legacy DarkDataGridView kept the active row visible (e.g. when sprites are added at the
    // end of the list); mirror that by scrolling the selection into view on each selection change.
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
        if (e.PropertyName == nameof(SpriteSequenceEditorWindowViewModel.SelectedSprite))
            ScrollToSelection();
    }

    private void ScrollToSelection()
    {
        if (spritesGrid.SelectedItem is not null)
            spritesGrid.ScrollIntoView(spritesGrid.SelectedItem);
    }
}
