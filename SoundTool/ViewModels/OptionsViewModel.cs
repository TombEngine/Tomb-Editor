#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using TombLib.WPF.Dialogs;

namespace SoundTool.ViewModels;

/// <summary>Edits a copy of the application preferences until applied.</summary>
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

    /// <summary>Raised when the edited values should be written to the configuration (Apply or OK).</summary>
    public event EventHandler? Applied;

    [RelayCommand]
    private void Apply() => Applied?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void Accept()
    {
        Applied?.Invoke(this, EventArgs.Empty);
        DialogResult = true;
    }

    [RelayCommand]
    private void ResetPage()
    {
        var defaults = new Configuration();
        AllowMultipleInstances = defaults.SoundTool_AllowMultipleInstances;
        Brightness = defaults.UI_FormColor_Brightness;
    }
}