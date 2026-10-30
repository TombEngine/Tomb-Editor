#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Numerics;
using System.Windows.Media;
using TombLib.LevelData;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.Moveable;

public partial class MoveableWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly MoveableInstance _moveable;
    private readonly Editor _editor;
    private readonly IMessageService _messageService;
    private readonly ILocalizationService _localizationService;

    private readonly Vector3 _originalColor;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _ocbText;
    [ObservableProperty] private bool _bit1;
    [ObservableProperty] private bool _bit2;
    [ObservableProperty] private bool _bit3;
    [ObservableProperty] private bool _bit4;
    [ObservableProperty] private bool _bit5;
    [ObservableProperty] private bool _invisible;
    [ObservableProperty] private bool _clearBody;
    [ObservableProperty] private Color _displayColor;

    public bool IsOcbEnabled { get; }
    public bool CanBeColored { get; }
    public string ClearBodyLabel { get; }

    public MoveableWindowViewModel(
        MoveableInstance moveable,
        Editor? editor = null,
        IMessageService? messageService = null,
        ILocalizationService? localizationService = null)
    {
        _moveable = moveable;
        _editor = editor ?? Editor.Instance;
        _messageService = ServiceLocator.ResolveService(messageService);
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        // OCB is only meaningful past TR3 in the original engines.
        IsOcbEnabled = _editor.Level.Settings.GameVersion.Native() > TRVersion.Game.TR3;

        // TombEngine repurposes the "Clear body" flag as "No reflection".
        ClearBodyLabel = _editor.Level.IsTombEngine
            ? _localizationService["NoReflection"]
            : _localizationService["ClearBody"];

        _originalColor = moveable.Color;
        CanBeColored = moveable.CanBeColored();

        _ocbText = moveable.Ocb.ToString();
        _bit1 = (moveable.CodeBits & (1 << 0)) != 0;
        _bit2 = (moveable.CodeBits & (1 << 1)) != 0;
        _bit3 = (moveable.CodeBits & (1 << 2)) != 0;
        _bit4 = (moveable.CodeBits & (1 << 3)) != 0;
        _bit5 = (moveable.CodeBits & (1 << 4)) != 0;
        _invisible = moveable.Invisible;
        _clearBody = moveable.ClearBody;

        // Display swatch shows the colour halved to match the historical FormMoveable behaviour.
        _displayColor = Vector3ToColor(moveable.Color * 0.5f);
    }

    private bool _colorEdited;

    partial void OnDisplayColorChanged(Color value) => _colorEdited = true;

    [RelayCommand]
    private void ResetTint()
    {
        // Only update the swatch; Confirm applies DisplayColor to the moveable.
        DisplayColor = Colors.Gray;
    }

    [RelayCommand]
    private void Confirm()
    {
        if (!short.TryParse(OcbText, out short ocb))
        {
            _messageService.ShowError(_localizationService["InvalidOcbMessage"]);
            return;
        }

        _editor.UndoManager.PushObjectPropertyChanged(_moveable);

        byte codeBits = 0;
        if (Bit1) codeBits |= 1 << 0;
        if (Bit2) codeBits |= 1 << 1;
        if (Bit3) codeBits |= 1 << 2;
        if (Bit4) codeBits |= 1 << 3;
        if (Bit5) codeBits |= 1 << 4;

        _moveable.CodeBits = codeBits;
        _moveable.Invisible = Invisible;
        _moveable.ClearBody = ClearBody;
        _moveable.Ocb = ocb;
        // Like the previous dialog, the tint is only written when the user changed it (the 8-bit swatch round trip would alter it otherwise).
        if (CanBeColored && _colorEdited)
            _moveable.Color = ColorToVector3(DisplayColor) * 2.0f;

        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        _moveable.Color = _originalColor;
        DialogResult = false;
    }

    private static Color Vector3ToColor(Vector3 value)
    {
        byte r = (byte)System.Math.Clamp(value.X * 255.0f, 0, 255);
        byte g = (byte)System.Math.Clamp(value.Y * 255.0f, 0, 255);
        byte b = (byte)System.Math.Clamp(value.Z * 255.0f, 0, 255);

        return Color.FromRgb(r, g, b);
    }

    private static Vector3 ColorToVector3(Color color)
        => new(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
}
