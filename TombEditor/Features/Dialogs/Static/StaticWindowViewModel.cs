#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Numerics;
using System.Windows.Media;
using TombLib;
using TombLib.LevelData;
using TombLib.WPF;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.Static;

public partial class StaticWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly StaticInstance _staticMesh;
    private readonly Editor _editor;
    private readonly IMessageService _messageService;
    private readonly ILocalizationService _localizationService;

    private readonly Vector3 _originalColor;
    private short _newOcb;
    private bool _isApplying;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _ocbText = "0";

    // Standard flag bits (non-scalable mode).
    [ObservableProperty] private bool _burnLaraOnCollision;
    [ObservableProperty] private bool _damageLaraOnContact;
    [ObservableProperty] private bool _disableCollision;
    [ObservableProperty] private bool _explodeKillingOnCollision;
    [ObservableProperty] private bool _glassTransparency;
    [ObservableProperty] private bool _hardShatter;
    [ObservableProperty] private bool _heavyTriggerOnCollision;
    [ObservableProperty] private bool _hugeCollision;
    [ObservableProperty] private bool _iceTransparency;
    [ObservableProperty] private bool _poisonLaraOnCollision;

    // SpecificShatter (>= 4096) is available regardless of Scalable mode.
    [ObservableProperty] private bool _specificShatter;

    // When Scalable is on, the standard flag bits are disabled and ScalableValue 0..1023 is packed instead.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StandardFlagsEnabled))]
    private bool _scalable;

    [ObservableProperty] private int _scalableValue;

    [ObservableProperty] private Color _displayColor;

    public bool StandardFlagsEnabled => !Scalable;

    public bool CanBeColored { get; }

    public StaticWindowViewModel(
        StaticInstance staticMesh,
        Editor? editor = null,
        IMessageService? messageService = null,
        ILocalizationService? localizationService = null)
    {
        _staticMesh = staticMesh;
        _editor = editor ?? Editor.Instance;
        _newOcb = staticMesh.Ocb;
        _originalColor = staticMesh.Color;
        CanBeColored = staticMesh.CanBeColored();

        _messageService = ServiceLocator.ResolveService(messageService);
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        _displayColor = Vector3ToColor(staticMesh.Color * 0.5f);

        DecodeOcb();
    }

    partial void OnOcbTextChanged(string value)
    {
        if (_isApplying)
            return;

        if (short.TryParse(value, out short parsed))
        {
            _newOcb = parsed;
            DecodeOcb();
        }
    }

    partial void OnBurnLaraOnCollisionChanged(bool value) => EncodeOcbIfNotApplying();
    partial void OnDamageLaraOnContactChanged(bool value) => EncodeOcbIfNotApplying();
    partial void OnDisableCollisionChanged(bool value) => EncodeOcbIfNotApplying();
    partial void OnExplodeKillingOnCollisionChanged(bool value) => EncodeOcbIfNotApplying();
    partial void OnGlassTransparencyChanged(bool value) => EncodeOcbIfNotApplying();
    partial void OnHardShatterChanged(bool value) => EncodeOcbIfNotApplying();
    partial void OnHeavyTriggerOnCollisionChanged(bool value) => EncodeOcbIfNotApplying();
    partial void OnHugeCollisionChanged(bool value) => EncodeOcbIfNotApplying();
    partial void OnIceTransparencyChanged(bool value) => EncodeOcbIfNotApplying();
    partial void OnPoisonLaraOnCollisionChanged(bool value) => EncodeOcbIfNotApplying();
    partial void OnSpecificShatterChanged(bool value) => EncodeOcbIfNotApplying();
    partial void OnScalableChanged(bool value) => EncodeOcbIfNotApplying();
    partial void OnScalableValueChanged(int value) => EncodeOcbIfNotApplying();

    private void EncodeOcbIfNotApplying()
    {
        if (!_isApplying)
            EncodeOcb();
    }

    private void EncodeOcb()
    {
        ushort backupOcb = (ushort)(_newOcb & ~(ushort)StaticMeshFlags.All);
        ushort ocb = 0;

        if (SpecificShatter)
            ocb += (ushort)StaticMeshFlags.SpecificShatter;

        if (Scalable)
        {
            ocb += (ushort)((ushort)StaticMeshFlags.Scalable + 4 * ScalableValue);
        }
        else
        {
            if (BurnLaraOnCollision) ocb += (ushort)StaticMeshFlags.BurnLaraOnCollision;
            if (DamageLaraOnContact) ocb += (ushort)StaticMeshFlags.DamageLaraOnCollision;
            if (DisableCollision) ocb += (ushort)StaticMeshFlags.DisableCollision;
            if (ExplodeKillingOnCollision) ocb += (ushort)StaticMeshFlags.ExplodeKillingOnCollision;
            if (GlassTransparency) ocb += (ushort)StaticMeshFlags.GlassTrasparency;
            if (HardShatter) ocb += (ushort)StaticMeshFlags.HardShatter;
            if (HeavyTriggerOnCollision) ocb += (ushort)StaticMeshFlags.EnableHeavyTriggerOnCollision;
            if (HugeCollision) ocb += (ushort)StaticMeshFlags.HugeCollision;
            if (IceTransparency) ocb += (ushort)StaticMeshFlags.IceTrasparency;
            if (PoisonLaraOnCollision) ocb += (ushort)StaticMeshFlags.PoisonLaraOnCollision;
        }

        ushort encoded = (ushort)(ocb | backupOcb);
        _newOcb = (short)encoded;

        _isApplying = true;
        OcbText = encoded.ToString();
        _isApplying = false;
    }

    private void DecodeOcb()
    {
        _isApplying = true;

        BurnLaraOnCollision = (_newOcb & (ushort)StaticMeshFlags.BurnLaraOnCollision) != 0;
        DamageLaraOnContact = (_newOcb & (ushort)StaticMeshFlags.DamageLaraOnCollision) != 0;
        DisableCollision = (_newOcb & (ushort)StaticMeshFlags.DisableCollision) != 0;
        ExplodeKillingOnCollision = (_newOcb & (ushort)StaticMeshFlags.ExplodeKillingOnCollision) != 0;
        GlassTransparency = (_newOcb & (ushort)StaticMeshFlags.GlassTrasparency) != 0;
        HardShatter = (_newOcb & (ushort)StaticMeshFlags.HardShatter) != 0;
        HeavyTriggerOnCollision = (_newOcb & (ushort)StaticMeshFlags.EnableHeavyTriggerOnCollision) != 0;
        HugeCollision = (_newOcb & (ushort)StaticMeshFlags.HugeCollision) != 0;
        IceTransparency = (_newOcb & (ushort)StaticMeshFlags.IceTrasparency) != 0;
        PoisonLaraOnCollision = (_newOcb & (ushort)StaticMeshFlags.PoisonLaraOnCollision) != 0;
        SpecificShatter = (_newOcb & (ushort)StaticMeshFlags.SpecificShatter) != 0;
        Scalable = (_newOcb & (ushort)StaticMeshFlags.Scalable) != 0;

        if (Scalable)
            ScalableValue = (int)MathC.Clamp((_newOcb & 4095) / 4.0f, 0.0f, 1023.0f);

        _isApplying = false;
    }

    [RelayCommand]
    private void Confirm()
    {
        if (!short.TryParse(OcbText, out short ocb))
        {
            _messageService.ShowError(_localizationService["InvalidOcbMessage"]);
            return;
        }

        _editor.UndoManager.PushObjectPropertyChanged(_staticMesh);
        _staticMesh.Ocb = ocb;
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        _staticMesh.Color = _originalColor;
        DialogResult = false;
    }

    [RelayCommand]
    private void PickColor()
        => EditorActions.EditColor(null, _staticMesh, newColor => DisplayColor = newColor.ToWPFColor());

    private static Color Vector3ToColor(Vector3 value)
    {
        byte r = (byte)System.Math.Clamp(value.X * 255.0f, 0, 255);
        byte g = (byte)System.Math.Clamp(value.Y * 255.0f, 0, 255);
        byte b = (byte)System.Math.Clamp(value.Z * 255.0f, 0, 255);

        return Color.FromRgb(r, g, b);
    }
}
