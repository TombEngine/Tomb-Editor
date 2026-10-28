#nullable enable

using System;
using System.ComponentModel;
using System.Windows;
using TombLib.Types;
using TombLib.WPF;
using WadTool.Controls;

namespace WadTool.Features.Dialogs.BlendCurveEditor;

/// <summary>
/// WPF shell replacing the WinForms <c>FormBlendCurveEditor</c>; the curve canvas is the WPF
/// <see cref="BezierCurveEditor"/> control.
/// </summary>
public partial class BlendCurveEditorWindow : Window
{
    private readonly BezierCurveEditor _curveEditor;
    private BlendCurveEditorWindowViewModel? _viewModel;

    public BlendCurveEditorWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        _curveEditor = curveEditor;
        _curveEditor.ValueChanged += CurveEditor_ValueChanged;

        DataContextChanged += OnDataContextChanged;
    }

    protected override void OnClosed(EventArgs e)
    {
        _curveEditor.ValueChanged -= CurveEditor_ValueChanged;
        DataContextChanged -= OnDataContextChanged;
        if (_viewModel is not null)
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;

        base.OnClosed(e);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;

        _viewModel = e.NewValue as BlendCurveEditorWindowViewModel;

        if (_viewModel is null)
            return;

        // The hosted editor works directly on the view model's curve clone, exactly like the
        // legacy form worked on its ResultCurve.
        _curveEditor.Value = _viewModel.ResultCurve;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    // Mirrors the legacy cbBlendPreset_SelectedIndexChanged handler.
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_viewModel is null || e.PropertyName != nameof(BlendCurveEditorWindowViewModel.SelectedPresetIndex))
            return;

        switch (_viewModel.SelectedPresetIndex)
        {
            case 0:
                _curveEditor.Value.Set(BezierCurve2.Linear);
                break;

            case 1:
                _curveEditor.Value.Set(BezierCurve2.EaseIn);
                break;

            case 2:
                _curveEditor.Value.Set(BezierCurve2.EaseOut);
                break;

            case 3:
                _curveEditor.Value.Set(BezierCurve2.EaseInOut);
                break;
        }

        _curveEditor.UpdateUI();
    }

    // Manually editing the curve deselects the preset, like in the legacy form.
    private void CurveEditor_ValueChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.SelectedPresetIndex = -1;
    }
}
