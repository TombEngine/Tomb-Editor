#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLib.WPF.Dialogs;

namespace SoundTool.ViewModels;

/// <summary>Edits a copy of the application preferences until confirmed.</summary>
public partial class OptionsViewModel : ObservableObject, IModalDialogViewModel
{
    [ObservableProperty] private bool _allowMultipleInstances;
    [ObservableProperty] private float _brightness;
    [ObservableProperty] private bool? _dialogResult;

    public OptionsViewModel(Configuration configuration)
    {
        _allowMultipleInstances = configuration.SoundTool_AllowMultipleInstances;
        _brightness = configuration.UI_FormColor_Brightness;
    }

    [RelayCommand]
    private void Accept() => DialogResult = true;
}
