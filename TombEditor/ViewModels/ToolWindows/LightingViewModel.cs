using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Numerics;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.LevelData;

namespace TombEditor.ViewModels.ToolWindows;

public partial class LightingViewModel : ObservableObject
{
    private readonly Editor _editor;

    // Light type items matching LightType enum order: Point, Shadow, Spot, Effect, Sun, FogBulb.
    public int SelectedLightType
    {
        get => (int)(_editor.SelectedObject is LightInstance light ? light.Type : (LightType)_lastSelectedLightType);
        set
        {
            _lastSelectedLightType = value;
            EditorActions.UpdateLightType((LightType)value);
        }
    }

    public int SelectedLightQuality
    {
        get => (int)(_editor.SelectedObject is LightInstance light ? light.Quality : 0);
        set => EditorActions.UpdateLightQuality((LightQuality)value);
    }

    public Color LightColor
    {
        get
        {
            if (_editor.SelectedObject is LightInstance light)
                return new Vector4(light.Color * 0.5f, 1.0f).ToWPFColor();

            return Colors.Transparent;
        }
    }

    public decimal Intensity
    {
        get => (decimal)(_editor.SelectedObject is LightInstance light ? light.Intensity : 0);
        set => UpdateLightFloat(
            (l, v) => CompareRounded(l.Intensity, v, 2),
            (l, v) => l.Intensity = v,
            (float)value);
    }

    public decimal InnerRange
    {
        get => (decimal)(_editor.SelectedObject is LightInstance light && HasInRange(light) ? light.InnerRange : 0);
        set => UpdateLightFloat(
            (l, v) => CompareRounded(l.InnerRange, v, 2),
            (l, v) => l.InnerRange = v,
            (float)value);
    }

    public decimal OuterRange
    {
        get => (decimal)(_editor.SelectedObject is LightInstance light && HasOutRange(light) ? light.OuterRange : 0);
        set => UpdateLightFloat(
            (l, v) => CompareRounded(l.OuterRange, v, 2),
            (l, v) => l.OuterRange = v,
            (float)value);
    }

    public decimal InnerAngle
    {
        get => (decimal)(_editor.SelectedObject is LightInstance light && HasInOutAngle(light) ? light.InnerAngle : 0);
        set => UpdateLightFloat(
            (l, v) => CompareRounded(l.InnerAngle, v, 2),
            (l, v) => l.InnerAngle = v,
            (float)value);
    }

    public decimal OuterAngle
    {
        get => (decimal)(_editor.SelectedObject is LightInstance light && HasInOutAngle(light) ? light.OuterAngle : 0);
        set => UpdateLightFloat(
            (l, v) => CompareRounded(l.OuterAngle, v, 2),
            (l, v) => l.OuterAngle = v,
            (float)value);
    }

    public decimal DirectionY
    {
        get => (decimal)(_editor.SelectedObject is LightInstance light && HasDirection(light) ? light.RotationY : 0);
        set => UpdateLightFloat(
            (l, v) => CompareRounded(l.RotationY, v, 2),
            (l, v) => l.RotationY = v,
            (float)value);
    }

    public decimal DirectionX
    {
        get => (decimal)(_editor.SelectedObject is LightInstance light && HasDirection(light) ? light.RotationX : 0);
        set => UpdateLightFloat(
            (l, v) => CompareRounded(l.RotationX, v, 2),
            (l, v) => l.RotationX = v,
            (float)value);
    }

    public bool LightEnabled
    {
        get => _editor.SelectedObject is LightInstance light && light.Enabled;
        set => UpdateLightBool(
            (l, v) => l.Enabled == v,
            (l, v) => l.Enabled = v,
            value);
    }

    public bool ObstructedByGeometry
    {
        get => _editor.SelectedObject is LightInstance light && light.IsObstructedByRoomGeometry;
        set => UpdateLightBool(
            (l, v) => l.IsObstructedByRoomGeometry == v,
            (l, v) => l.IsObstructedByRoomGeometry = v,
            value);
    }

    public bool CastsShadow
    {
        get => _editor.SelectedObject is LightInstance light && light.CastDynamicShadows;
        set => UpdateLightBool(
            (l, v) => l.CastDynamicShadows == v,
            (l, v) => l.CastDynamicShadows = v,
            value);
    }

    public bool StaticallyUsed
    {
        get => _editor.SelectedObject is LightInstance light && light.IsStaticallyUsed;
        set => UpdateLightBool(
            (l, v) => l.IsStaticallyUsed == v,
            (l, v) => l.IsStaticallyUsed = v,
            value);
    }

    public bool DynamicallyUsed
    {
        get => _editor.SelectedObject is LightInstance light && light.IsDynamicallyUsed;
        set => UpdateLightBool(
            (l, v) => l.IsDynamicallyUsed == v,
            (l, v) => l.IsDynamicallyUsed = v,
            value);
    }

    public bool UsedForImportedGeometry
    {
        get => _editor.SelectedObject is LightInstance light && light.IsUsedForImportedGeometry;
        set => UpdateLightBool(
            (l, v) => l.IsUsedForImportedGeometry == v,
            (l, v) => l.IsUsedForImportedGeometry = v,
            value);
    }

    // Enabled state for controls.
    [ObservableProperty] private bool _hasLight;
    [ObservableProperty] private bool _canEditInRange;
    [ObservableProperty] private bool _canEditOutRange;
    [ObservableProperty] private bool _canEditInOutAngle;
    [ObservableProperty] private bool _canEditDirection;
    [ObservableProperty] private bool _canCastShadows;
    [ObservableProperty] private bool _canCastDynamicShadows;
    [ObservableProperty] private bool _canIlluminateGeometry;
    [ObservableProperty] private bool _canEditQuality;

    public ICommand AddLightCommand { get; }

    private int _lastSelectedLightType;

    public LightingViewModel(Editor editor)
    {
        _editor = editor;
        _editor.EditorEventRaised += EditorEventRaised;

        AddLightCommand = new RelayCommand(PlaceSelectedLightType);

        RefreshLightState();
    }

    public void Cleanup()
    {
        _editor.EditorEventRaised -= EditorEventRaised;
    }

    [RelayCommand]
    private void EditLightColor()
    {
        EditorActions.EditLightColor(WPFUtils.GetWin32WindowFromCaller(this));
    }

    private void EditorEventRaised(IEditorEvent obj)
    {
        if (obj is Editor.InitEvent or
            Editor.GameVersionChangedEvent or
            Editor.LevelChangedEvent or
            Editor.ObjectChangedEvent or
            Editor.SelectedObjectChangedEvent)
            RefreshLightState();
    }

    private void RefreshLightState()
    {
        var isTEN = _editor.Level.Settings.GameVersion == TRVersion.Game.TombEngine;
        var light = _editor.SelectedObject as LightInstance;

        HasLight = light != null;
        CanEditInRange = light != null && HasInRange(light);
        CanEditOutRange = light != null && HasOutRange(light);
        CanEditInOutAngle = light != null && HasInOutAngle(light);
        CanEditDirection = light != null && HasDirection(light);
        CanCastShadows = light != null && CanObstruct(light);
        CanCastDynamicShadows = light != null && CanDynamicShadow(light, isTEN);
        CanIlluminateGeometry = light != null && CanIlluminate(light);
        CanEditQuality = light != null;

        OnPropertyChanged(nameof(SelectedLightType));
        OnPropertyChanged(nameof(SelectedLightQuality));
        OnPropertyChanged(nameof(LightColor));
        OnPropertyChanged(nameof(Intensity));
        OnPropertyChanged(nameof(InnerRange));
        OnPropertyChanged(nameof(OuterRange));
        OnPropertyChanged(nameof(InnerAngle));
        OnPropertyChanged(nameof(OuterAngle));
        OnPropertyChanged(nameof(DirectionY));
        OnPropertyChanged(nameof(DirectionX));
        OnPropertyChanged(nameof(LightEnabled));
        OnPropertyChanged(nameof(ObstructedByGeometry));
        OnPropertyChanged(nameof(CastsShadow));
        OnPropertyChanged(nameof(StaticallyUsed));
        OnPropertyChanged(nameof(DynamicallyUsed));
        OnPropertyChanged(nameof(UsedForImportedGeometry));
    }

    private void PlaceSelectedLightType()
    {
        EditorActions.PlaceLight((LightType)_lastSelectedLightType);
    }

    private void UpdateLightFloat(Func<LightInstance, float, bool> compare, Action<LightInstance, float> setter, float value)
    {
        EditorActions.UpdateLight(compare, setter, light => (float?)value);
    }

    private void UpdateLightBool(Func<LightInstance, bool, bool> compare, Action<LightInstance, bool> setter, bool value)
    {
        EditorActions.UpdateLight(compare, setter, light => (bool?)value);
    }

    private static bool CompareRounded(float first, float second, int decimalPlaces)
    {
        float multiplier = (float)Math.Pow(10, decimalPlaces);
        return Math.Round(first * multiplier) == Math.Round(second * multiplier);
    }

    private static bool HasInRange(LightInstance light)
        => light.Type is LightType.Point or LightType.Shadow or LightType.Effect or LightType.Spot;

    private static bool HasOutRange(LightInstance light)
        => light.Type is LightType.Point or LightType.Shadow or LightType.Effect or LightType.FogBulb or LightType.Spot;

    private static bool HasInOutAngle(LightInstance light)
        => light.Type is LightType.Spot;

    private static bool HasDirection(LightInstance light)
        => light.Type is LightType.Spot or LightType.Sun;

    private static bool CanObstruct(LightInstance light)
        => light.Type is LightType.Point or LightType.Shadow or LightType.Spot or LightType.Sun;

    private static bool CanDynamicShadow(LightInstance light, bool isTEN)
        => isTEN && light.Type is LightType.Point or LightType.Spot;

    private static bool CanIlluminate(LightInstance light)
        => light.Type is LightType.Point or LightType.Shadow or LightType.Spot or LightType.Sun;
}
