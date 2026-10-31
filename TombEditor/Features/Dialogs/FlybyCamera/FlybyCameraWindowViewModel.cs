#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TombEditor.Features.FlybyTimeline;
using TombEditor.Features.FlybyTimeline.Sequence;
using TombLib;
using TombLib.LevelData;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.FlybyCamera;

public sealed record DofModeItem(DofMode Mode, string DisplayName);

public partial class FlybyCameraWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private const float ChangeComparisonEpsilon = 0.0001f;
    private const int FlagBitCount = 16;

    private readonly FlybyCameraInstance _flyByCamera;
    private readonly Editor _editor;
    private readonly ILocalizationService _localizationService;

    // Snapshot for cancel-restore + change detection.
    private readonly ushort _originalFlags;
    private readonly ushort _originalSequence;
    private readonly ushort _originalNumber;
    private readonly short _originalTimer;
    private readonly float _originalSpeed;
    private readonly float _originalFov;
    private readonly float _originalRoll;
    private readonly float _originalRotationX;
    private readonly float _originalRotationY;
    private readonly float _originalDofDistance;
    private readonly float _originalDofRange;
    private readonly float _originalDofStrength;
    private readonly DofMode _originalDofMode;

    private bool _loading = true;
    private bool _ownedPreview;
    private bool _restoreOriginalValues;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private decimal _sequence;
    [ObservableProperty] private decimal _number;
    [ObservableProperty] private decimal _timer;
    [ObservableProperty] private decimal _speed;
    [ObservableProperty] private decimal _fov;
    [ObservableProperty] private decimal _roll;
    [ObservableProperty] private decimal _rotationX;
    [ObservableProperty] private decimal _rotationY;
    [ObservableProperty] private decimal _dofDistance;
    [ObservableProperty] private decimal _dofRange;
    [ObservableProperty] private decimal _dofStrength;
    [ObservableProperty] private DofModeItem _selectedDofMode;

    public ObservableCollection<FlybyFlagBitViewModel> Flags { get; } = new();
    public IReadOnlyList<DofModeItem> DofModes { get; }

    public bool IsTombEngine { get; }
    public bool HasChanges { get; private set; }

    /// <summary>Upper bound for the FOV editor; a full 180 degrees would make the projection degenerate.</summary>
    public decimal MaxFov => (decimal)FlybyConstants.MaxFlybyFieldOfViewDegrees;

    public FlybyCameraWindowViewModel(
        FlybyCameraInstance flyByCamera,
        Editor? editor = null,
        ILocalizationService? localizationService = null)
    {
        _flyByCamera = flyByCamera;
        _editor = editor ?? Editor.Instance;
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        IsTombEngine = _editor.Level.IsTombEngine;

        _originalFlags = flyByCamera.Flags;
        _originalSequence = flyByCamera.Sequence;
        _originalNumber = flyByCamera.Number;
        _originalTimer = flyByCamera.Timer;
        _originalSpeed = flyByCamera.Speed;
        _originalFov = flyByCamera.Fov;
        _originalRoll = flyByCamera.Roll;
        _originalRotationX = flyByCamera.RotationX;
        _originalRotationY = flyByCamera.RotationY;
        _originalDofDistance = flyByCamera.DofDistance;
        _originalDofRange = flyByCamera.DofRange;
        _originalDofStrength = flyByCamera.DofStrength;
        _originalDofMode = flyByCamera.DofMode;

        DofModes = new List<DofModeItem>
        {
            new(DofMode.None, _localizationService["DofModeNone"]),
            new(DofMode.Full, _localizationService["DofModeFull"]),
            new(DofMode.Front, _localizationService["DofModeFront"]),
            new(DofMode.Back, _localizationService["DofModeBack"])
        };

        _selectedDofMode = FindDofMode(flyByCamera.DofMode);

        _sequence = flyByCamera.Sequence;
        _number = flyByCamera.Number;
        _timer = flyByCamera.Timer;
        _speed = (decimal)flyByCamera.Speed;
        _fov = (decimal)flyByCamera.Fov;
        _roll = (decimal)flyByCamera.Roll;
        _rotationX = (decimal)flyByCamera.RotationX;
        _rotationY = (decimal)flyByCamera.RotationY;
        _dofDistance = (decimal)flyByCamera.DofDistance;
        _dofRange = (decimal)flyByCamera.DofRange;
        _dofStrength = (decimal)flyByCamera.DofStrength;

        BuildFlagBits();

        _loading = false;
    }

    private DofModeItem FindDofMode(DofMode mode)
    {
        foreach (DofModeItem item in DofModes)
            if (item.Mode == mode)
                return item;
        return DofModes[0];
    }

    private void BuildFlagBits()
    {
        for (int i = 0; i < FlagBitCount; i++)
        {
            string label = GetBitLabel(i);
            bool isChecked = FlybySequenceHelper.GetFlagBit(_flyByCamera.Flags, i);

            Flags.Add(new FlybyFlagBitViewModel(i, label, isChecked));
        }
    }

    private string GetBitLabel(int index)
    {
        // Four flags swap meaning in TR5 / TombEngine. Mirrors the original FormFlybyCamera load logic.
        bool tr5OrTen = _editor.Level.Settings.GameVersion is TRVersion.Game.TR5 or TRVersion.Game.TombEngine;

        return index switch
        {
            1 when tr5OrTen => _localizationService["Bit1TR5"],
            4 when tr5OrTen => _localizationService["Bit4TR5"],
            12 when tr5OrTen => _localizationService["Bit12TR5"],
            13 when tr5OrTen => _localizationService["Bit13TR5"],
            _ => _localizationService[$"Bit{index}"]
        };
    }

    // Live-preview hooks for the parameters the editor renders in real time.

    partial void OnFovChanged(decimal value) => PreviewParameterChanged();
    partial void OnRollChanged(decimal value) => PreviewParameterChanged();
    partial void OnRotationXChanged(decimal value) => PreviewParameterChanged();
    partial void OnRotationYChanged(decimal value) => PreviewParameterChanged();
    partial void OnDofDistanceChanged(decimal value) => PreviewParameterChanged();
    partial void OnDofRangeChanged(decimal value) => PreviewParameterChanged();
    partial void OnDofStrengthChanged(decimal value) => PreviewParameterChanged();
    partial void OnSelectedDofModeChanged(DofModeItem value) => PreviewParameterChanged();

    private void PreviewParameterChanged()
    {
        if (_loading)
            return;

        _flyByCamera.Fov = (float)Fov;
        _flyByCamera.Roll = (float)Roll;
        _flyByCamera.RotationX = (float)RotationX;
        _flyByCamera.RotationY = (float)RotationY;
        _flyByCamera.DofDistance = (float)DofDistance;
        _flyByCamera.DofRange = (float)DofRange;
        _flyByCamera.DofStrength = (float)DofStrength;
        _flyByCamera.DofMode = SelectedDofMode.Mode;

        _editor.CameraPreviewUpdated(_flyByCamera);
    }

    /// <summary>
    /// Engages the camera preview if it is not already running for some other reason
    /// (flyby timeline, fly-mode). Mirrors the original FormFlybyCamera_Load behaviour.
    /// </summary>
    public void BeginPreview()
    {
        if (_editor.FlyMode)
            return;

        if (_editor.CameraPreviewMode == CameraPreviewType.None)
        {
            _editor.ToggleCameraPreview(true);
            _ownedPreview = true;
        }

        _editor.CameraPreviewUpdated(_flyByCamera);
    }

    /// <summary>
    /// Drops preview ownership and (if requested) rolls back the live-edited instance values.
    /// </summary>
    public void EndPreview()
    {
        if (_restoreOriginalValues)
        {
            RestoreOriginalValues();

            if (!_ownedPreview && _editor.CameraPreviewMode != CameraPreviewType.None)
                _editor.CameraPreviewUpdated(_flyByCamera);
        }

        if (_ownedPreview && _editor.CameraPreviewMode != CameraPreviewType.None)
            _editor.ToggleCameraPreview(false);
    }

    /// <summary>
    /// Invoked by the view when the window is being closed without an explicit OK/Cancel.
    /// The legacy form accepts pending changes when the user closes via the window chrome.
    /// </summary>
    public void OnWindowClosing()
    {
        if (_restoreOriginalValues || DialogResult is true)
            return;

        AcceptPendingChanges();
        DialogResult = true;
    }

    private void AcceptPendingChanges()
    {
        ApplyPendingValues(_flyByCamera);
        HasChanges = ComputeHasChanges();
    }

    private void ApplyPendingValues(FlybyCameraInstance camera)
    {
        camera.Flags = CollectFlags();
        camera.Sequence = (ushort)Sequence;
        camera.Number = (ushort)Number;
        camera.Timer = (short)Timer;
        camera.Speed = (float)Speed;
        camera.Fov = (float)Fov;
        camera.Roll = (float)Roll;
        camera.RotationX = (float)RotationX;
        camera.RotationY = (float)RotationY;
        camera.DofDistance = (float)DofDistance;
        camera.DofRange = (float)DofRange;
        camera.DofStrength = (float)DofStrength;
        camera.DofMode = SelectedDofMode.Mode;
    }

    private ushort CollectFlags()
    {
        ushort flags = 0;

        foreach (FlybyFlagBitViewModel bit in Flags)
            flags = FlybySequenceHelper.SetFlagBit(flags, bit.Index, bit.IsChecked);

        return flags;
    }

    private bool ComputeHasChanges()
    {
        var pending = new FlybyCameraInstance();
        ApplyPendingValues(pending);

        return pending.Flags != _originalFlags
            || pending.Sequence != _originalSequence
            || pending.Number != _originalNumber
            || pending.Timer != _originalTimer
            || !MathC.WithinEpsilon(pending.Speed, _originalSpeed, ChangeComparisonEpsilon)
            || !MathC.WithinEpsilon(pending.Fov, _originalFov, ChangeComparisonEpsilon)
            || !MathC.WithinEpsilon(pending.Roll, _originalRoll, ChangeComparisonEpsilon)
            || !MathC.WithinEpsilon(pending.RotationX, _originalRotationX, ChangeComparisonEpsilon)
            || !MathC.WithinEpsilon(pending.RotationY, _originalRotationY, ChangeComparisonEpsilon)
            || !MathC.WithinEpsilon(pending.DofDistance, _originalDofDistance, ChangeComparisonEpsilon)
            || !MathC.WithinEpsilon(pending.DofRange, _originalDofRange, ChangeComparisonEpsilon)
            || !MathC.WithinEpsilon(pending.DofStrength, _originalDofStrength, ChangeComparisonEpsilon)
            || pending.DofMode != _originalDofMode;
    }

    private void RestoreOriginalValues()
    {
        _flyByCamera.Flags = _originalFlags;
        _flyByCamera.Sequence = _originalSequence;
        _flyByCamera.Number = _originalNumber;
        _flyByCamera.Timer = _originalTimer;
        _flyByCamera.Speed = _originalSpeed;
        _flyByCamera.Fov = _originalFov;
        _flyByCamera.Roll = _originalRoll;
        _flyByCamera.RotationX = _originalRotationX;
        _flyByCamera.RotationY = _originalRotationY;
        _flyByCamera.DofDistance = _originalDofDistance;
        _flyByCamera.DofRange = _originalDofRange;
        _flyByCamera.DofStrength = _originalDofStrength;
        _flyByCamera.DofMode = _originalDofMode;
    }

    [RelayCommand]
    private void Confirm()
    {
        AcceptPendingChanges();
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        _restoreOriginalValues = true;
        DialogResult = false;
    }
}
