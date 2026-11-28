#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLib.LevelData;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.Memo;

public partial class MemoWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly MemoInstance _memo;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _text;
    [ObservableProperty] private bool _alwaysDisplay;

    public MemoWindowViewModel(MemoInstance memo, ILocalizationService? localizationService = null)
    {
        _memo = memo;
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        _text = memo.Text ?? string.Empty;
        _alwaysDisplay = memo.AlwaysDisplay;
    }

    [RelayCommand]
    private void Confirm()
    {
        _memo.Text = Text;
        _memo.AlwaysDisplay = AlwaysDisplay;
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel() => DialogResult = false;
}
