#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Windows;
using TombLib.Services;
using TombLib.WPF.Dialogs;

namespace TombLib.WPF.Features.Pickers;

/// <summary>Themed modal prompt with a combo box (WPF counterpart of list-selection dialogs like the shared <c>AnimationImportDialog</c>).</summary>
public partial class ComboPickerDialog : Window
{
    public ComboPickerDialog()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}

public partial class ComboPickerDialogViewModel : ObservableObject, IModalDialogViewModel
{
    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private int _selectedIndex;

    public ComboPickerDialogViewModel(string title, string label, IReadOnlyList<string> items, int selectedIndex)
    {
        Title = title;
        Label = label;
        Items = items;
        _selectedIndex = items.Count > 0 ? selectedIndex : -1;
    }

    public string Title { get; }
    public string Label { get; }
    public IReadOnlyList<string> Items { get; }

    [RelayCommand]
    private void Ok() => DialogResult = true;
}

/// <summary>Shows a modal combo-box picker and returns the selected index, or null if cancelled.</summary>
public static class ComboPickerBox
{
    public static int? Show(object? owner, string title, string label, IReadOnlyList<string> items, int selectedIndex = 0)
    {
        var viewModel = new ComboPickerDialogViewModel(title, label, items, selectedIndex);
        var dialog = new ComboPickerDialog { DataContext = viewModel };

        if (owner is Window window)
            dialog.Owner = window;
        else if (owner is IWindowOwner nativeOwner)
            dialog.SetOwner(nativeOwner);

        dialog.ShowDialog();
        return viewModel.DialogResult == true && viewModel.SelectedIndex >= 0 ? viewModel.SelectedIndex : null;
    }
}
