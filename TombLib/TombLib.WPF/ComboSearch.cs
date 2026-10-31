#nullable enable

using CommunityToolkit.Mvvm.Input;
using DarkUI.WPF.CustomControls;
using System.Windows.Input;

namespace TombLib.WPF;

/// <summary>
/// Commands that let a plain button drive a searchable combo box (the search icon next to texture combos).
/// </summary>
public static class ComboSearch
{
    /// <summary>
    /// Opens the drop-down of the combo box passed as command parameter and focuses its search box.
    /// </summary>
    public static ICommand OpenCommand { get; } = new RelayCommand<SearchableComboBox>(combo => combo?.OpenAndFocusSearch());
}