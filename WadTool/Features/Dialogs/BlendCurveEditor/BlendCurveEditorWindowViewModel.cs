#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLib.Types;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace WadTool.Features.Dialogs.BlendCurveEditor;

public partial class BlendCurveEditorWindowViewModel : ObservableObject, IModalDialogViewModel
{
    [ObservableProperty] private bool? _dialogResult;

    /// <summary>Index into the preset combo; -1 means a custom curve (no preset selected).</summary>
    [ObservableProperty] private int _selectedPresetIndex = -1;

    /// <summary>
    /// Working copy of the curve passed to the constructor; the hosted editor mutates it in place.
    /// Callers read it back after <see cref="DialogResult"/> became <c>true</c>.
    /// </summary>
    public BezierCurve2 ResultCurve { get; }

    public BlendCurveEditorWindowViewModel(BezierCurve2 curve, ILocalizationService? localizationService = null)
    {
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        ResultCurve = curve.Clone();
    }

    [RelayCommand]
    private void Confirm() => DialogResult = true;

    [RelayCommand]
    private void Cancel() => DialogResult = false;
}
