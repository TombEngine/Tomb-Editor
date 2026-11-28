#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLib;
using TombLib.LevelData;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.Sink;

public partial class SinkWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly SinkInstance _sink;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private short _strength;

    public SinkWindowViewModel(SinkInstance sink, ILocalizationService? localizationService = null)
    {
        _sink = sink;
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        // Strength is stored as 0..31 internally but shown as 1..32, matching legacy FormSink.
        _strength = (short)MathC.Clamp(sink.Strength + 1, 1, 32);
    }

    [RelayCommand]
    private void Confirm()
    {
        _sink.Strength = (short)(Strength - 1);
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel() => DialogResult = false;
}
