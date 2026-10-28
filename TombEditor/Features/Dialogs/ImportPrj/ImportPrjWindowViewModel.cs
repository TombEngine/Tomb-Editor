#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.ImportPrj;

public partial class ImportPrjWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _soundsPath = string.Empty;
    [ObservableProperty] private bool _respectMousepatchOnFlybyHandling;
    [ObservableProperty] private bool _useHalfPixelCorrection;

    public string PrjPath { get; }
    public string PrjFileName { get; }

    public ImportPrjWindowViewModel(
        string prjPath,
        bool respectMousepatch,
        bool useHalfPixelCorrection,
        IDialogService? dialogService = null,
        ILocalizationService? localizationService = null)
    {
        _dialogService = ServiceLocator.ResolveService(dialogService);
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        PrjPath = prjPath;
        PrjFileName = Path.GetFileNameWithoutExtension(prjPath);
        _respectMousepatchOnFlybyHandling = respectMousepatch;
        _useHalfPixelCorrection = useHalfPixelCorrection;
    }

    [RelayCommand]
    private void BrowseSoundsCatalog()
    {
        var settings = new OpenFileDialogSettings
        {
            Title = _localizationService["BrowseSoundsTitle"],
            Filter = _localizationService["SoundCatalogsFilter"],
            CheckFileExists = true
        };

        if (_dialogService.ShowOpenFileDialog(this, settings) == true)
            SoundsPath = settings.FileName;
    }

    [RelayCommand]
    private void Confirm() => DialogResult = true;

    [RelayCommand]
    private void Cancel() => DialogResult = false;
}
