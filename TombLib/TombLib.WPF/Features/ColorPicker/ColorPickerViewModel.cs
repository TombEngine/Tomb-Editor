using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace TombLib.WPF.Features.ColorPicker;

/// <summary>Maintains the selected color and a working copy of reusable custom slots.</summary>
public partial class ColorPickerViewModel : ObservableObject, IModalDialogViewModel
{
    [ObservableProperty] private Color _selectedColor;
    [ObservableProperty] private int _selectedSlot = -1;
    [ObservableProperty] private bool? _dialogResult;

    public ObservableCollection<SolidColorBrush> CustomColors { get; } = new();

    public ColorPickerViewModel(Color initialColor, IEnumerable<Color> customColors)
    {
        _selectedColor = initialColor;
        foreach (var color in customColors)
            CustomColors.Add(CreateBrush(color));
    }

    [RelayCommand]
    private void Accept() => DialogResult = true;

    [RelayCommand]
    private void SaveCustom()
    {
        int index = SelectedSlot < 0 ? 0 : SelectedSlot;
        if (index >= CustomColors.Count)
            return;

        var color = SelectedColor;
        CustomColors[index] = CreateBrush(color);
        SelectedSlot = index;
        SelectedColor = color;
    }

    partial void OnSelectedSlotChanged(int value)
    {
        if (value >= 0 && value < CustomColors.Count)
            SelectedColor = CustomColors[value].Color;
    }

    private static SolidColorBrush CreateBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
