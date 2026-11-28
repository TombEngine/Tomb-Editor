#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using TombLib.LevelData;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.Camera;

public sealed record CameraModeItem(CameraInstanceMode Mode, string DisplayName);

public partial class CameraWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly CameraInstance _instance;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty] private CameraModeItem _selectedMode;
    [ObservableProperty] private byte _moveTimer;
    [ObservableProperty] private bool _glideOut;

    public IReadOnlyList<CameraModeItem> Modes { get; }

    public bool IsMoveTimerEnabled { get; }
    public bool IsGlideOutEnabled { get; }

    public CameraWindowViewModel(CameraInstance instance, ILocalizationService? localizationService = null)
    {
        _instance = instance;
        _localizationService = ServiceLocator.ResolveService(localizationService)
            .WithKeysFor(this);

        var settings = instance.Room.Level.Settings;

        // Filter the camera-mode list by engine support
        var modes = new List<CameraModeItem>
        {
            new(CameraInstanceMode.Default, _localizationService["ModeDefault"])
        };

        if (settings.GameVersion.SupportsLockedCameras())
            modes.Add(new CameraModeItem(CameraInstanceMode.Locked, _localizationService["ModeLocked"]));

        if (settings.GameVersion == TRVersion.Game.TR5)
            modes.Add(new CameraModeItem(CameraInstanceMode.Sniper, _localizationService["ModeSniper"]));

        Modes = modes;

        _selectedMode = Modes.FirstOrDefault(m => m.Mode == instance.CameraMode) ?? Modes[0];
        _moveTimer = instance.MoveTimer;
        _glideOut = instance.GlideOut;

        IsGlideOutEnabled = settings.GameVersion == TRVersion.Game.TRNG;

        IsMoveTimerEnabled = settings.GameVersion is not (TRVersion.Game.TR3 or TRVersion.Game.TR4 or TRVersion.Game.TR5);
    }

    [RelayCommand]
    private void Confirm()
    {
        _instance.CameraMode = SelectedMode.Mode;
        _instance.MoveTimer = MoveTimer;
        _instance.GlideOut = GlideOut;
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel() => DialogResult = false;
}
