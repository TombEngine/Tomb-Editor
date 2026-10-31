#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Services;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using TombLib.WPF.Dialogs;
using WadTool.Controls;
using WadTool.Features.Dialogs.MeshEditor;

namespace WadTool.Features.Dialogs.StaticEditor;

/// <summary>
/// WPF counterpart of the legacy <c>FormStaticEditor</c>. Edits a clone of a <see cref="WadStatic"/>
/// (visibility / collision boxes, transform, lights, shatter attributes) using the WPF
/// <see cref="PanelRenderingStaticEditor"/> 3D view, committing back to the wad on OK.
/// </summary>
public partial class StaticEditorWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly WadToolClass _tool;
    private readonly DeviceManager _deviceManager;
    private readonly Wad2 _wad;
    private readonly WadStatic _static;

    private PanelRenderingStaticEditor? _panel;
    private bool _initializing = true;
    private bool _suppressLightSelectionSync;

    [ObservableProperty] private bool? _dialogResult;

    // Transform
    [ObservableProperty] private double _positionX;
    [ObservableProperty] private double _positionY;
    [ObservableProperty] private double _positionZ;

    // Visibility box
    [ObservableProperty] private double _visBoxMinX, _visBoxMinY, _visBoxMinZ, _visBoxMaxX, _visBoxMaxY, _visBoxMaxZ;

    // Collision box
    [ObservableProperty] private double _colBoxMinX, _colBoxMinY, _colBoxMinZ, _colBoxMaxX, _colBoxMaxY, _colBoxMaxZ;

    // Lighting
    [ObservableProperty] private double _ambient;
    [ObservableProperty] private double _intensity;
    [ObservableProperty] private double _radius;
    [ObservableProperty] private bool _intensityEnabled;
    [ObservableProperty] private bool _radiusEnabled;
    [ObservableProperty] private bool _ambientEnabled = true;
    [ObservableProperty] private bool _lightsEnabled = true;
    [ObservableProperty] private bool _addLightEnabled = true;
    [ObservableProperty] private bool _deleteLightEnabled = true;
    [ObservableProperty] private int _lightingTypeIndex;
    [ObservableProperty] private StaticLightItem? _selectedLight;

    // Shatter
    [ObservableProperty] private bool _shatter;
    [ObservableProperty] private bool _shatterControlsEnabled;
    [ObservableProperty] private bool _soundEnabled;
    [ObservableProperty] private bool _playEnabled;
    [ObservableProperty] private int _soundIdIndex = -1;

    // Draw options
    [ObservableProperty] private bool _drawGrid = true;
    [ObservableProperty] private bool _drawGizmo = true;
    [ObservableProperty] private bool _drawLights = true;
    [ObservableProperty] private bool _drawNormals;
    [ObservableProperty] private bool _drawVisibilityBox;
    [ObservableProperty] private bool _drawCollisionBox;

    public ObservableCollection<StaticLightItem> Lights { get; } = new();
    public ObservableCollection<string> SoundIds { get; } = new();

    public string Title { get; }

    /// <summary>Raised for editor MessageEvents so the window can show a popup anchored to the 3D view.</summary>
    public event Action<string, PopupType>? MessageRaised;

    public StaticEditorWindowViewModel(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadStatic staticMesh)
    {
        _tool = tool;
        _deviceManager = deviceManager;
        _wad = wad;
        _static = staticMesh.Clone();

        Title = "Static editor - " + _static.Id.ToString(_wad.GameVersion);

        SoundIds.Add("Default");
        foreach (var entry in WadSounds.GetFormattedList(_tool.ReferenceLevel, _wad.GameVersion))
            SoundIds.Add(entry);
    }

    /// <summary>Called by the window once the rendering panel is hosted; mirrors the legacy ctor body.</summary>
    public void AttachPanel(PanelRenderingStaticEditor panel)
    {
        _panel = panel;

        panel.InitializeRendering(_tool, _deviceManager);
        panel.Configuration = _tool.Configuration;
        panel.Static = _static;
        panel.DrawGrid = true;
        panel.DrawGizmo = true;
        panel.DrawLights = true;

        _initializing = true;
        SyncVisibilityBoxFromModel();
        SyncCollisionBoxFromModel();
        SyncPositionFromPanel();
        RefreshLightsList();
        UpdateLightUI();
        UpdateShatterUI();
        Ambient = _static.AmbientLight;
        _initializing = false;

        _tool.EditorEventRaised += OnEditorEventRaised;
    }

    public void Detach() => _tool.EditorEventRaised -= OnEditorEventRaised;

    /// <summary>Called when the 3D view moved the static (gizmo drag); refreshes the position fields.</summary>
    public void RefreshPositionFromPanel() => SyncPositionFromPanel();

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        switch (obj)
        {
            case WadToolClass.StaticSelectedLightChangedEvent:
                UpdateLightUI();
                break;

            case WadToolClass.StaticLightsChangedEvent:
                RefreshLightsList();
                UpdateLightUI();
                _static.Version = DataVersion.GetNext();
                _panel?.Invalidate();
                break;

            case WadToolClass.MessageEvent message:
                MessageRaised?.Invoke(message.Message, message.Type);
                break;
        }
    }

    // ---- Transform ----

    partial void OnPositionXChanged(double value) => SetPanelPosition();
    partial void OnPositionYChanged(double value) => SetPanelPosition();
    partial void OnPositionZChanged(double value) => SetPanelPosition();

    private void SetPanelPosition()
    {
        if (_initializing || _panel is null)
            return;

        _panel.StaticPosition = new Vector3((float)PositionX, (float)PositionY, (float)PositionZ);
        _panel.Invalidate();
    }

    private void SyncPositionFromPanel()
    {
        if (_panel is null)
            return;

        bool previous = _initializing;
        _initializing = true;
        PositionX = _panel.StaticPosition.X;
        PositionY = _panel.StaticPosition.Y;
        PositionZ = _panel.StaticPosition.Z;
        _initializing = previous;
    }

    // ---- Visibility box ----

    partial void OnVisBoxMinXChanged(double value) => ApplyVisibilityBox();
    partial void OnVisBoxMinYChanged(double value) => ApplyVisibilityBox();
    partial void OnVisBoxMinZChanged(double value) => ApplyVisibilityBox();
    partial void OnVisBoxMaxXChanged(double value) => ApplyVisibilityBox();
    partial void OnVisBoxMaxYChanged(double value) => ApplyVisibilityBox();
    partial void OnVisBoxMaxZChanged(double value) => ApplyVisibilityBox();

    private void ApplyVisibilityBox()
    {
        if (_initializing || _panel is null)
            return;

        _static.VisibilityBox = new BoundingBox(
            new Vector3((float)VisBoxMinX, (float)VisBoxMinY, (float)VisBoxMinZ),
            new Vector3((float)VisBoxMaxX, (float)VisBoxMaxY, (float)VisBoxMaxZ));
        _panel.Invalidate();
    }

    private void SyncVisibilityBoxFromModel()
    {
        bool previous = _initializing;
        _initializing = true;
        VisBoxMinX = _static.VisibilityBox.Minimum.X;
        VisBoxMinY = _static.VisibilityBox.Minimum.Y;
        VisBoxMinZ = _static.VisibilityBox.Minimum.Z;
        VisBoxMaxX = _static.VisibilityBox.Maximum.X;
        VisBoxMaxY = _static.VisibilityBox.Maximum.Y;
        VisBoxMaxZ = _static.VisibilityBox.Maximum.Z;
        _initializing = previous;
    }

    // ---- Collision box ----

    partial void OnColBoxMinXChanged(double value) => ApplyCollisionBox();
    partial void OnColBoxMinYChanged(double value) => ApplyCollisionBox();
    partial void OnColBoxMinZChanged(double value) => ApplyCollisionBox();
    partial void OnColBoxMaxXChanged(double value) => ApplyCollisionBox();
    partial void OnColBoxMaxYChanged(double value) => ApplyCollisionBox();
    partial void OnColBoxMaxZChanged(double value) => ApplyCollisionBox();

    private void ApplyCollisionBox()
    {
        if (_initializing || _panel is null)
            return;

        _static.CollisionBox = new BoundingBox(
            new Vector3((float)ColBoxMinX, (float)ColBoxMinY, (float)ColBoxMinZ),
            new Vector3((float)ColBoxMaxX, (float)ColBoxMaxY, (float)ColBoxMaxZ));
        _panel.Invalidate();
    }

    private void SyncCollisionBoxFromModel()
    {
        bool previous = _initializing;
        _initializing = true;
        ColBoxMinX = _static.CollisionBox.Minimum.X;
        ColBoxMinY = _static.CollisionBox.Minimum.Y;
        ColBoxMinZ = _static.CollisionBox.Minimum.Z;
        ColBoxMaxX = _static.CollisionBox.Maximum.X;
        ColBoxMaxY = _static.CollisionBox.Maximum.Y;
        ColBoxMaxZ = _static.CollisionBox.Maximum.Z;
        _initializing = previous;
    }

    // ---- Lighting ----

    partial void OnAmbientChanged(double value)
    {
        if (_initializing || _panel is null)
            return;

        _static.AmbientLight = (short)value;
        _panel.UpdateLights();
    }

    partial void OnIntensityChanged(double value)
    {
        if (_initializing || _panel?.SelectedLight is null)
            return;

        _panel.SelectedLight.Intensity = (float)value;
        _panel.UpdateLights();
    }

    partial void OnRadiusChanged(double value)
    {
        if (_initializing || _panel?.SelectedLight is null)
            return;

        _panel.SelectedLight.Radius = (float)value;
        _panel.UpdateLights();
    }

    partial void OnSelectedLightChanged(StaticLightItem? value)
    {
        if (_suppressLightSelectionSync || _panel is null)
            return;

        _panel.SelectedLight = value?.Light;
        _panel.Invalidate();
    }

    partial void OnLightingTypeIndexChanged(int value)
    {
        if (_initializing || _panel is null)
            return;

        if (value == (int)WadMeshLightingType.Normals)
        {
            _static.Mesh.LightingType = WadMeshLightingType.Normals;
            _static.Lights.Clear();
            AddLightEnabled = false;
            DeleteLightEnabled = false;
            IntensityEnabled = false;
            RadiusEnabled = false;
            AmbientEnabled = false;
            LightsEnabled = false;
            RefreshLightsList();
            UpdateLightUI();
        }
        else
        {
            _static.Mesh.LightingType = WadMeshLightingType.VertexColors;
            AddLightEnabled = true;
            DeleteLightEnabled = true;
            IntensityEnabled = true;
            RadiusEnabled = true;
            AmbientEnabled = true;
            LightsEnabled = true;
            UpdateLightUI();
        }

        _panel.Invalidate();
    }

    private void RefreshLightsList()
    {
        var selectedLight = _panel?.SelectedLight;

        // Rebuilding the list clears its selection, which must not reset the panel selection.
        _suppressLightSelectionSync = true;

        try
        {
            Lights.Clear();

            foreach (var light in _static.Lights)
                Lights.Add(new StaticLightItem(light, "Light #" + _static.Lights.IndexOf(light)));

            SelectedLight = Lights.FirstOrDefault(item => ReferenceEquals(item.Light, selectedLight));
        }
        finally
        {
            _suppressLightSelectionSync = false;
        }
    }

    private void UpdateLightUI()
    {
        bool previous = _initializing;
        _initializing = true;

        if (_panel?.SelectedLight is not null)
        {
            IntensityEnabled = true;
            Intensity = _panel.SelectedLight.Intensity;
            RadiusEnabled = true;
            Radius = _panel.SelectedLight.Radius;
        }
        else
        {
            IntensityEnabled = false;
            RadiusEnabled = false;
        }

        // Keep the list selection in step with the panel selection without re-entering the setter.
        _suppressLightSelectionSync = true;
        SelectedLight = Lights.FirstOrDefault(item => ReferenceEquals(item.Light, _panel?.SelectedLight));
        _suppressLightSelectionSync = false;

        LightingTypeIndex = (int)_static.Mesh.LightingType;

        // The index setter is skipped while initializing, so apply the Normals lighting restrictions here.
        if (_static.Mesh.LightingType == WadMeshLightingType.Normals)
        {
            AddLightEnabled = false;
            DeleteLightEnabled = false;
            IntensityEnabled = false;
            RadiusEnabled = false;
            AmbientEnabled = false;
            LightsEnabled = false;
        }

        _initializing = previous;
    }

    // ---- Shatter ----

    partial void OnShatterChanged(bool value)
    {
        if (_initializing)
            return;

        _static.Shatter = value;
        UpdateShatterUI();
    }

    partial void OnSoundIdIndexChanged(int value)
    {
        if (_initializing || value == -1)
            return;

        _static.ShatterSoundID = value - 1;
    }

    private void UpdateShatterUI()
    {
        bool previous = _initializing;
        _initializing = true;

        if (_wad.GameVersion != TRVersion.Game.TombEngine)
        {
            ShatterControlsEnabled = false;
            SoundEnabled = false;
            PlayEnabled = false;
        }
        else
        {
            ShatterControlsEnabled = true;
            SoundEnabled = _static.Shatter;
            PlayEnabled = _tool.ReferenceLevel is not null && _static.Shatter;
        }

        Shatter = _static.Shatter;
        SoundIdIndex = SoundIds.Count - 1 > _static.ShatterSoundID ? _static.ShatterSoundID + 1 : -1;

        _initializing = previous;
    }

    // ---- Draw options ----

    partial void OnDrawGridChanged(bool value) => SetDrawFlag(() => _panel!.DrawGrid = value);
    partial void OnDrawGizmoChanged(bool value) => SetDrawFlag(() => _panel!.DrawGizmo = value);
    partial void OnDrawLightsChanged(bool value) => SetDrawFlag(() => _panel!.DrawLights = value);
    partial void OnDrawNormalsChanged(bool value) => SetDrawFlag(() => _panel!.DrawNormals = value);
    partial void OnDrawVisibilityBoxChanged(bool value) => SetDrawFlag(() => _panel!.DrawVisibilityBox = value);
    partial void OnDrawCollisionBoxChanged(bool value) => SetDrawFlag(() => _panel!.DrawCollisionBox = value);

    private void SetDrawFlag(Action apply)
    {
        if (_initializing || _panel is null)
            return;

        apply();
        _panel.Invalidate();
    }

    // ---- Commands ----

    [RelayCommand]
    private void Save()
    {
        if (_panel is null)
            return;

        // Bake the gizmo transform into the mesh, like the legacy butSaveChanges_Click.
        var transform = _panel.GizmoTransform;
        if (transform != Matrix4x4.Identity)
        {
            for (int i = 0; i < _static.Mesh.VertexPositions.Count; i++)
            {
                var position = MathC.HomogenousTransform(_static.Mesh.VertexPositions[i], transform);
                _static.Mesh.VertexPositions[i] = new Vector3(position.X, position.Y, position.Z);
            }

            for (int i = 0; i < _static.Mesh.VertexNormals.Count; i++)
            {
                var normal = MathC.HomogenousTransform(_static.Mesh.VertexNormals[i], transform);
                _static.Mesh.VertexNormals[i] = new Vector3(normal.X, normal.Y, normal.Z);
            }
        }

        _wad.Statics.Remove(_static.Id);
        _wad.Statics.Add(_static.Id, _static);
        _static.Version = DataVersion.GetNext();

        _tool.ToggleUnsavedChanges();

        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel() => DialogResult = false;

    [RelayCommand]
    private void CalculateVisibilityBox()
    {
        if (_panel is null)
            return;

        _static.VisibilityBox = _static.Mesh.CalculateBoundingBox(_panel.GizmoTransform);
        SyncVisibilityBoxFromModel();
        _panel.Invalidate();
    }

    [RelayCommand]
    private void ClearVisibilityBox()
    {
        _static.VisibilityBox = new BoundingBox();
        SyncVisibilityBoxFromModel();
        _panel?.Invalidate();
    }

    [RelayCommand]
    private void CalculateCollisionBox()
    {
        if (_panel is null)
            return;

        _static.CollisionBox = _static.Mesh.CalculateBoundingBox(_panel.GizmoTransform);
        SyncCollisionBoxFromModel();
        _panel.Invalidate();
    }

    [RelayCommand]
    private void ClearCollisionBox()
    {
        _static.CollisionBox = new BoundingBox();
        SyncCollisionBoxFromModel();
        _panel?.Invalidate();
    }

    [RelayCommand]
    private void ResetTranslation()
    {
        if (_panel is null)
            return;

        _panel.StaticPosition = Vector3.Zero;
        SyncPositionFromPanel();
        _panel.Invalidate();
    }

    [RelayCommand]
    private void ResetRotation()
    {
        if (_panel is null)
            return;

        _panel.StaticRotation = Vector3.Zero;
        _panel.Invalidate();
    }

    [RelayCommand]
    private void ResetScale()
    {
        if (_panel is null)
            return;

        _panel.StaticScale = 1.0f;
        _panel.Invalidate();
    }

    [RelayCommand]
    private void AddLight()
    {
        if (_panel is not null)
            _panel.Action = PanelRenderingStaticEditor.StaticEditorAction.PlaceLight;
    }

    [RelayCommand]
    private void DeleteLight()
    {
        if (_panel?.SelectedLight is null)
            return;

        WadLight light = _panel.SelectedLight;
        _panel.SelectedLight = null;
        _panel.DeleteLight(light);
        _panel.UpdateLights();
        RefreshLightsList();
        UpdateLightUI();
        _panel.Invalidate();
    }

    [RelayCommand]
    private void RecalculateNormals()
    {
        if (_static.Mesh is null || _panel is null)
            return;

        _static.Mesh.CalculateNormals();
        _panel.UpdateLights();
        _panel.Invalidate();
    }

    [RelayCommand]
    private void EditMesh()
    {
        if (_panel?.Static?.Mesh is null)
            return;

        var meshViewModel = new MeshEditorWindowViewModel(_tool, DeviceManager.DefaultDeviceManager, _tool.DestinationWad, _static.Mesh.Clone());
        var meshDialog = new MeshEditorWindow { DataContext = meshViewModel };
        meshDialog.SetOwner(Owner);
        meshDialog.ShowDialog();
        if (meshViewModel.DialogResult != true || meshViewModel.SelectedMesh is null)
            return;

        _static.Mesh = meshViewModel.SelectedMesh.Clone();
        _panel.UpdateMesh();
        _panel.Invalidate();
        UpdateLightUI();
    }

    [RelayCommand]
    private void ImportMesh()
    {
        if (_panel is null)
            return;

        var mesh = WadActions.ImportMesh(_tool, Owner);
        if (mesh is null)
            return;

        _static.Mesh = mesh;
        _static.VisibilityBox = _static.Mesh.CalculateBoundingBox(_panel.GizmoTransform);
        _static.CollisionBox = _static.Mesh.CalculateBoundingBox(_panel.GizmoTransform);
        _static.Version = DataVersion.GetNext();

        _panel.Invalidate();
        SyncPositionFromPanel();
        SyncCollisionBoxFromModel();
        SyncVisibilityBoxFromModel();
        UpdateLightUI();
    }

    [RelayCommand]
    private void ExportMesh() => WadActions.ExportMesh(_static.Mesh, _tool, Owner);

    [RelayCommand]
    private void ResetShatterAttributes()
    {
        _static.Shatter = TrCatalog.IsStaticShatterable(_wad.GameVersion, _static.Id.TypeId);
        _static.ShatterSoundID = -1;
        UpdateShatterUI();
    }

    [RelayCommand]
    private void PlaySound()
    {
        if (_tool.ReferenceLevel is null || _tool.ReferenceLevel.Settings.GlobalSoundMap.Count == 0)
            return;

        // HACK: Old shatter sound is hardcoded in both original engines and TEN.
        int soundID = SoundIdIndex > 0 ? SoundIdIndex - 1 : 347;

        var soundInfo = _tool.ReferenceLevel.Settings.GlobalSoundMap.FirstOrDefault(info => info.Id == soundID);
        if (soundInfo is not null)
            try { WadSoundPlayer.PlaySoundInfo(_tool.ReferenceLevel, soundInfo); }
            catch (Exception) { }
    }

    private static IWindowOwner Owner => WPFUtils.GetWindowOwner();
}
