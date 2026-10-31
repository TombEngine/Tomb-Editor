#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using TombLib.LevelData;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace WadTool.Features.Dialogs.NewWad2;

public partial class NewWad2WindowViewModel : ObservableObject, IModalDialogViewModel
{
    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private TRVersion.Game _selectedGameVersion = TRVersion.Game.TR4;

    public IReadOnlyList<TRVersion.Game> GameVersions { get; }

    public NewWad2WindowViewModel(ILocalizationService? localizationService = null)
    {
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        GameVersions = new List<TRVersion.Game>(TRVersion.NativeVersions);
    }

    [RelayCommand]
    private void Confirm() => DialogResult = true;

    [RelayCommand]
    private void Cancel() => DialogResult = false;
}
