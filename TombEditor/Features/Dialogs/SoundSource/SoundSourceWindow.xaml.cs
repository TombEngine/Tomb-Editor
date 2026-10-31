#nullable enable

using System;
using System.ComponentModel;
using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.SoundSource;

public partial class SoundSourceWindow : Window
{
    public SoundSourceWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    // Match legacy OnShown lstSounds.EnsureVisible(): scroll the current selection
    // into view on first show, plus on each subsequent selection change.
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
        if (e.PropertyName == nameof(SoundSourceWindowViewModel.SelectedSound))
            ScrollToSelection();
    }

    private void ScrollToSelection()
    {
        if (soundList.SelectedItem is not null)
            soundList.ScrollIntoView(soundList.SelectedItem);
    }
}
