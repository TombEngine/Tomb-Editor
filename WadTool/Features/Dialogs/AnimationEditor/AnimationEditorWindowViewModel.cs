#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LevelFileDialog = TombLib.WPF.Services.LevelFileDialog;
using PopUpInfo = TombLib.WPF.PopUpInfo;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Threading;
using TombLib.GeometryIO;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Services;
using TombLib.Types;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using TombLib.WPF.CustomControls;
using TombLib.WPF.Dialogs;
using WadTool.Controls;
using WadTool.Features.Dialogs.AnimationFixer;
using WadTool.Features.Dialogs.AnimCommandsEditor;
using WadTool.Features.Dialogs.StateChangesEditor;

namespace WadTool.Features.Dialogs.AnimEditor;

/// <summary>Coordinates animation editing, playback and native WPF viewport tools.</summary>
public partial class AnimationEditorWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly DeviceManager _deviceManager;
    private readonly WadMoveableId _moveableId;
    private readonly AnimationEditor _editor;

    private PanelRenderingAnimationEditor? _panel;
    private AnimationTrackBar? _timeline;
    private BezierCurveEditor? _bezier;
    private DispatcherTimer? _playTimer;
    private bool _allowUpdate = true;
    private bool _rebuildingList;
    private bool _undoPossible;
    private bool _redoPossible;
    private int _frameCount;

    private readonly PopUpInfo _popup = new();

    // Chained playback / blend / sound state (slice 3b), mirroring the legacy fields.
    private static readonly int _materialIndexSwitchInterval = 30 * 3;
    private static readonly int _gridRecoveryWaitInterval = 30 * 2;
    private static readonly float _gridRecoveryStep = 1 / (30 * 0.5f);

    private int _chainedIncomingAnimation = -1;
    private int _chainedIncomingFrame = -1;
    private TombLib.VectorInt2 _chainedIncomingFrameRange = -TombLib.VectorInt2.One;
    private int _chainedSetPosRecoveryCount;
    private float _gridRecoveryCount;
    private int _chainedInitialAnim;
    private int _chainedInitialCursorPos;
    private TombLib.VectorInt2 _chainedInitialSelection;
    private readonly AnimBlendPreviewState _blendState = new();
    private int _overallPlaybackCount = 30 * 3;
    private int _currentMaterialIndex;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _title;

    private bool _isSaved = true;
    [ObservableProperty] private string _statusText = string.Empty;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _isTombEngine;
    [ObservableProperty] private AnimListItem? _selectedAnim;

    // Editable animation properties (slice 2).
    [ObservableProperty] private string _animName = string.Empty;
    [ObservableProperty] private double _frameRate;
    [ObservableProperty] private double _endFrame;
    [ObservableProperty] private double _nextAnimation;
    [ObservableProperty] private double _nextFrame;
    [ObservableProperty] private double _startVertVel;
    [ObservableProperty] private double _endVertVel;
    [ObservableProperty] private double _startHorVel;
    [ObservableProperty] private double _endHorVel;
    [ObservableProperty] private double _blendFrameCount;
    [ObservableProperty] private string _stateIdText = string.Empty;

    // Per-bone transform + bounding box (slice 2).
    [ObservableProperty] private string _transformHeader = "Transform";
    [ObservableProperty] private int _transformModeIndex;
    [ObservableProperty] private double _rotationX, _rotationY, _rotationZ;
    [ObservableProperty] private double _translationX, _translationY, _translationZ;
    [ObservableProperty] private double _bBoxMinX, _bBoxMinY, _bBoxMinZ, _bBoxMaxX, _bBoxMaxY, _bBoxMaxZ;
    [ObservableProperty] private MeshBoneItem? _selectedBoneItem;

    // Root motion (TEN), blend preset, transport toggles (slice 3a).
    [ObservableProperty] private bool _rootPosX, _rootPosY, _rootPosZ, _rootRotX, _rootRotY, _rootRotZ;
    [ObservableProperty] private int _blendPresetIndex = -1;
    [ObservableProperty] private bool _chainPlayback;
    [ObservableProperty] private bool _soundPreview;
    [ObservableProperty] private string _soundConditionLabel = "Land";

    // View / config toggles (slice 3e).
    [ObservableProperty] private bool _scrollGrid;
    [ObservableProperty] private bool _restoreGridHeight;
    [ObservableProperty] private bool _smoothAnimation;
    [ObservableProperty] private bool _showGizmo;
    [ObservableProperty] private bool _showGrid;
    [ObservableProperty] private bool _showCollisionBox;
    [ObservableProperty] private bool _showSkin;

    [ObservableProperty] private bool _roomsEnabled;
    [ObservableProperty] private object? _selectedRoomItem;
    [ObservableProperty] private double _growX, _growY, _growZ;

    public ObservableCollection<AnimListItem> Animations { get; } = new();
    public ObservableCollection<MeshBoneItem> Bones { get; } = new();
    public ObservableCollection<object> Rooms { get; } = new();

    /// <summary>Gets the application configuration (used by the window to persist its placement).</summary>
    public WadTool.Configuration Configuration => _editor.Tool.Configuration;

    public AnimationEditorWindowViewModel(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadMoveableId id)
    {
        _deviceManager = deviceManager;
        _moveableId = id;
        _editor = new AnimationEditor(tool, wad, id);
        _title = "Animation editor - " + _editor.Moveable.Id.ToString(_editor.Wad.GameVersion);
    }

    /// <summary>Called by the window once the hosted controls exist; mirrors the legacy ctor.</summary>
    public void AttachControls(PanelRenderingAnimationEditor panel, AnimationTrackBar timeline, BezierCurveEditor bezier)
    {
        _panel = panel;
        _timeline = timeline;
        _bezier = bezier;

        panel.Configuration = _editor.Tool.Configuration;

        WadMoveable skin;
        var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(_editor.Wad.GameVersion, _moveableId.TypeId));
        skin = _editor.Wad.Moveables.ContainsKey(skinId) ? _editor.Wad.Moveables[skinId] : _editor.Wad.Moveables[_moveableId];
        panel.InitializeRendering(_editor, _deviceManager, skin);

        _playTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
        _playTimer.Tick += (_, _) => OnPlayTick();

        _editor.Tool.EditorEventRaised += OnEditorEventRaised;

        IsTombEngine = _editor.Wad.GameVersion == TRVersion.Game.TombEngine;
        _allowUpdate = false;
        ChainPlayback = _editor.Tool.Configuration.AnimationEditor_ChainPlayback;
        SoundPreview = _editor.Tool.Configuration.AnimationEditor_SoundPreview;
        SoundConditionLabel = _editor.Tool.Configuration.AnimationEditor_SoundPreviewType.ToString();
        ScrollGrid = _editor.Tool.Configuration.AnimationEditor_ScrollGrid;
        RestoreGridHeight = _editor.Tool.Configuration.AnimationEditor_RecoverGridAfterPositionChange;
        SmoothAnimation = _editor.Tool.Configuration.AnimationEditor_SmoothAnimation;
        ShowGizmo = _editor.Tool.Configuration.AnimationEditor_ShowGizmo;
        ShowGrid = _editor.Tool.Configuration.AnimationEditor_ShowGrid;
        ShowCollisionBox = _editor.Tool.Configuration.AnimationEditor_ShowCollisionBox;
        ShowSkin = _editor.Tool.Configuration.AnimationEditor_ShowSkin;
        _allowUpdate = true;

        Bones.Clear();
        for (int i = 0; i < panel.Model.Bones.Count; i++)
            Bones.Add(new MeshBoneItem(i, panel.Model.Bones[i].Name));

        UpdateReferenceLevel();

        RebuildAnimationsList();
        if (Animations.Count > 0)
            SelectedAnim = Animations[0];
    }

    public void Detach()
    {
        if (_playTimer is not null) _playTimer.Stop();
        _editor.Tool.EditorEventRaised -= OnEditorEventRaised;
        _popup.Dispose();
    }

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        if (obj is WadToolClass.AnimationEditorMeshSelectedEvent ||
            obj is WadToolClass.AnimationEditorGizmoPickedEvent ||
            obj is WadToolClass.AnimationEditorAnimationChangedEvent ||
            obj is WadToolClass.AnimationEditorCurrentAnimationChangedEvent)
        {
            _editor.MadeChanges = false;
            UpdateTransformUI();
        }

        if (obj is WadToolClass.UndoStackChangedEvent stackEvent)
        {
            _undoPossible = stackEvent.UndoPossible;
            _redoPossible = stackEvent.RedoPossible;
            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();
        }

        if (obj is WadToolClass.UndoStackChangedEvent || obj is WadToolClass.AnimationEditorGizmoPickedEvent)
            SetSaved(false);

        if (obj is WadToolClass.AnimationEditorAnimationChangedEvent ||
            obj is WadToolClass.AnimationEditorCurrentAnimationChangedEvent ||
            obj is WadToolClass.AnimationEditorAnimcommandChangedEvent)
            _timeline?.Invalidate();

        if (obj is WadToolClass.AnimationEditorMeshSelectedEvent meshSelected && _panel is not null)
        {
            // Sync the bone list selection to the 3D-picked mesh.
            int index = meshSelected.Mesh is null ? -1 : meshSelected.Model.Meshes.IndexOf(meshSelected.Mesh);
            var item = Bones.FirstOrDefault(b => b.Index == index);
            if (item is not null && !ReferenceEquals(item, SelectedBoneItem))
            {
                bool prev = _allowUpdate;
                _allowUpdate = false; // avoid re-driving panel selection
                SelectedBoneItem = item;
                _allowUpdate = prev;
            }
        }

        if (obj is WadToolClass.ReferenceLevelChangedEvent)
            UpdateReferenceLevel();

        if (obj is WadToolClass.AnimationEditorStateChangeEvent stateChange &&
            _playTimer is not null && _playTimer.IsEnabled && _editor.Tool.Configuration.AnimationEditor_ChainPlayback)
        {
            _chainedIncomingAnimation = stateChange.NextAnimation;
            _chainedIncomingFrame = stateChange.NextFrame;
            _chainedIncomingFrameRange = stateChange.FrameRange;
            _blendState.SetPendingBlend(stateChange.BlendFrames, stateChange.BlendCurve);
        }

        if (obj is WadToolClass.MessageEvent message)
            ShowPopup(message.Message, message.Type);
    }

    /// <summary>Legacy <c>lstAnimations_Click</c>: a click on the list resets the chained playback start and the grid position.</summary>
    public void ResetChainedPlaybackStart()
    {
        if (_editor.CurrentAnim is null || SelectedAnim is null)
            return;

        _chainedInitialAnim = _editor.CurrentAnim.Index;
        _chainedInitialCursorPos = 0;
        _chainedInitialSelection = new TombLib.VectorInt2();

        _panel.GridPosition = System.Numerics.Vector3.Zero;
    }

    partial void OnSelectedAnimChanged(AnimListItem? value)
    {
        // Like the previous list, an empty selection keeps the current animation.
        if (_rebuildingList || value is null)
            return;

        SelectAnimation(value.Node);
        _editor.CheckAnimationIntegrity(value.Node.WadAnimation);
    }

    private void SelectAnimation(AnimationNode? node)
    {
        if (_timeline is null || _panel is null)
            return;

        bool sameAnimSameSize = _editor.CurrentAnim is not null && node is not null && _editor.CurrentAnim.Index == node.Index &&
            _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == node.DirectXAnimation.KeyFrames.Count;

        _editor.CurrentAnim = node;

        _editor.MadeChanges = false;
        _allowUpdate = false;
        if (node is not null)
        {
            AnimName = node.WadAnimation.Name;
            FrameRate = node.WadAnimation.FrameRate;
            EndFrame = node.WadAnimation.EndFrame;
            NextAnimation = node.WadAnimation.NextAnimation;
            NextFrame = node.WadAnimation.NextFrame;
            StartVertVel = node.WadAnimation.StartVelocity;
            EndVertVel = node.WadAnimation.EndVelocity;
            StartHorVel = node.WadAnimation.StartLateralVelocity;
            EndHorVel = node.WadAnimation.EndLateralVelocity;
            BlendFrameCount = node.WadAnimation.BlendFrameCount;
            StateIdText = node.WadAnimation.StateId.ToString();
            if (_bezier is not null) _bezier.Value = node.WadAnimation.BlendCurve;
            BlendPresetIndex = -1;

            RootPosX = node.WadAnimation.RootMotion.TranslationX;
            RootPosY = node.WadAnimation.RootMotion.TranslationY;
            RootPosZ = node.WadAnimation.RootMotion.TranslationZ;
            RootRotX = node.WadAnimation.RootMotion.RotationX;
            RootRotY = node.WadAnimation.RootMotion.RotationY;
            RootRotZ = node.WadAnimation.RootMotion.RotationZ;
        }

        _timeline.Animation = node;

        if (_editor.ValidAnimationAndFrames)
        {
            _timeline.Minimum = 0;
            _timeline.Maximum = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;
        }

        if (!sameAnimSameSize)
        {
            if (!IsPlaying)
                _timeline.Value = 0;
            SelectFrame();
            _timeline.ResetSelection();
        }
        _allowUpdate = true;

        if (node?.DirectXAnimation.KeyFrames.Count > 0)
            _panel.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
        _panel.Invalidate();

        UpdateStatusLabel();
    }

    /// <summary>Called by the window when the timeline cursor changes.</summary>
    public void OnTimelineValueChanged() => SelectFrame();

    private void SelectFrame(float k = -1)
    {
        if (_panel is null || _timeline is null || !_editor.ValidAnimationAndFrames)
            return;

        int frameIndex = _timeline.Value;
        var keyFrames = _editor.CurrentAnim.DirectXAnimation.KeyFrames;
        if (frameIndex >= keyFrames.Count)
            frameIndex = keyFrames.Count - 1;

        _editor.CurrentFrameIndex = frameIndex;

        if (k > 0)
        {
            int nextIndex = frameIndex < keyFrames.Count - 1 ? frameIndex + 1 : frameIndex;
            k = Math.Min(k, 1);
            _panel.Model.BuildAnimationPose(keyFrames[frameIndex], keyFrames[nextIndex], k);
        }
        else
        {
            _panel.Model.BuildAnimationPose(keyFrames[frameIndex]);
            UpdateTransformUI();
        }

        _panel.Invalidate();
        PreviewSounds();
        UpdateStatusLabel();
    }

    /// <summary>Reads the current keyframe's per-bone rotation/translation + bounding box into the UI (legacy UpdateTransformUI).</summary>
    private void UpdateTransformUI()
    {
        if (_panel is null || _editor.CurrentKeyFrame is null)
            return;

        _allowUpdate = false;
        int meshIndex = _panel.SelectedMesh is null ? 0 : _panel.Model.Meshes.IndexOf(_panel.SelectedMesh);

        RotationX = TombLib.MathC.RadToDeg(_editor.CurrentKeyFrame.Rotations[meshIndex].X);
        RotationY = TombLib.MathC.RadToDeg(_editor.CurrentKeyFrame.Rotations[meshIndex].Y);
        RotationZ = TombLib.MathC.RadToDeg(_editor.CurrentKeyFrame.Rotations[meshIndex].Z);
        TranslationX = _editor.CurrentKeyFrame.Translations[0].X;
        TranslationY = _editor.CurrentKeyFrame.Translations[0].Y;
        TranslationZ = _editor.CurrentKeyFrame.Translations[0].Z;
        if (TransformModeIndex < 0) TransformModeIndex = 0;

        BBoxMinX = _editor.CurrentKeyFrame.BoundingBox.Minimum.X;
        BBoxMinY = _editor.CurrentKeyFrame.BoundingBox.Minimum.Y;
        BBoxMinZ = _editor.CurrentKeyFrame.BoundingBox.Minimum.Z;
        BBoxMaxX = _editor.CurrentKeyFrame.BoundingBox.Maximum.X;
        BBoxMaxY = _editor.CurrentKeyFrame.BoundingBox.Maximum.Y;
        BBoxMaxZ = _editor.CurrentKeyFrame.BoundingBox.Maximum.Z;

        var boneName = _panel.Model.Bones[meshIndex].Name;
        if (string.IsNullOrEmpty(boneName)) boneName = "Bone " + meshIndex;
        TransformHeader = "Transform (" + boneName + ")";
        _allowUpdate = true;
    }

    /// <summary>Called by the window when the timeline selection changes (drives bounding-box range edits).</summary>
    public void OnTimelineSelectionChanged()
    {
        if (_timeline is null) return;
        _editor.Selection = _timeline.SelectionIsEmpty ? new TombLib.VectorInt2(-1, -1) : _timeline.Selection;
        UpdateStatusLabel();
    }

    private bool ValidAnim() => _allowUpdate && _editor.ValidAnimationAndFrames;

    private void PushUndoOnce()
    {
        if (!_editor.MadeChanges)
        {
            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
            _editor.MadeChanges = true;
        }
    }

    private void AfterParameterChange()
    {
        _timeline?.Invalidate();
        _panel?.Invalidate();
    }

    // ---- Editable animation properties ----

    partial void OnAnimNameChanged(string value)
    {
        if (!_allowUpdate || _editor.CurrentAnim is null) return;
        var newName = value.Trim();
        if (string.IsNullOrEmpty(newName) || newName == _editor.CurrentAnim.WadAnimation.Name) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.Name = newName;
        if (SelectedAnim is not null) SelectedAnim.Label = "(" + _editor.CurrentAnim.Index + ") " + newName;
    }

    partial void OnFrameRateChanged(double value)
    {
        if (!ValidAnim() || (byte)value == _editor.CurrentAnim.WadAnimation.FrameRate) return;
        PushUndoOnce();
        byte rate = (byte)TombLib.MathC.Clamp((float)value, 1, 255);
        _allowUpdate = false;
        EndFrame = Math.Round(_editor.CurrentAnim.WadAnimation.EndFrame / (_editor.CurrentAnim.WadAnimation.FrameRate / (float)rate));
        _allowUpdate = true;
        _editor.CurrentAnim.WadAnimation.FrameRate = rate;
        _editor.CurrentAnim.WadAnimation.EndFrame = (ushort)EndFrame;
        AfterParameterChange();
    }

    partial void OnEndFrameChanged(double value)
    {
        if (!ValidAnim() || (ushort)value == _editor.CurrentAnim.WadAnimation.EndFrame) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.EndFrame = (ushort)value;
        AfterParameterChange();
    }

    partial void OnNextAnimationChanged(double value)
    {
        if (!ValidAnim() || (ushort)value == _editor.CurrentAnim.WadAnimation.NextAnimation) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.NextAnimation = (ushort)value;
        AfterParameterChange();
    }

    partial void OnNextFrameChanged(double value)
    {
        if (!ValidAnim() || (ushort)value == _editor.CurrentAnim.WadAnimation.NextFrame) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.NextFrame = (ushort)value;
        AfterParameterChange();
    }

    partial void OnStartVertVelChanged(double value) => SetVelocity(v => _editor.CurrentAnim.WadAnimation.StartVelocity = v, _editor.CurrentAnim?.WadAnimation.StartVelocity, value);
    partial void OnEndVertVelChanged(double value) => SetVelocity(v => _editor.CurrentAnim.WadAnimation.EndVelocity = v, _editor.CurrentAnim?.WadAnimation.EndVelocity, value);
    partial void OnStartHorVelChanged(double value) => SetVelocity(v => _editor.CurrentAnim.WadAnimation.StartLateralVelocity = v, _editor.CurrentAnim?.WadAnimation.StartLateralVelocity, value);
    partial void OnEndHorVelChanged(double value) => SetVelocity(v => _editor.CurrentAnim.WadAnimation.EndLateralVelocity = v, _editor.CurrentAnim?.WadAnimation.EndLateralVelocity, value);

    private void SetVelocity(Action<float> setter, float? current, double value)
    {
        if (!ValidAnim() || current is null || Math.Abs(current.Value - (float)value) < 1e-6f) return;
        PushUndoOnce();
        setter((float)value);
        AfterParameterChange();
    }

    partial void OnBlendFrameCountChanged(double value)
    {
        if (!ValidAnim() || (ushort)value == _editor.CurrentAnim.WadAnimation.BlendFrameCount) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.BlendFrameCount = (ushort)value;
        AfterParameterChange();
    }

    /// <summary>Commits the state-id text box (legacy UpdateStateChange). Called by the window on commit.</summary>
    public void CommitStateId()
    {
        if (!_allowUpdate || _editor.CurrentAnim is null) return;

        ushort oldValue = _editor.CurrentAnim.WadAnimation.StateId;
        if (!ushort.TryParse(StateIdText.Trim(), out ushort newValue))
        {
            string searchString = StateIdText.Trim();
            int possibleID = -1;
            for (int i = 0; i < 2; i++)
            {
                possibleID = TrCatalog.TryToGetStateID(_editor.Wad.GameVersion, _editor.Moveable.Id.TypeId, searchString);
                if (possibleID >= 0) break;
                searchString = searchString.Replace(' ', '_');
            }
            if (possibleID < 0)
                return;
            newValue = (ushort)possibleID;
        }

        var possibleName = TrCatalog.GetStateName(_editor.Wad.GameVersion, _editor.Moveable.Id.TypeId, newValue);
        _allowUpdate = false;
        StateIdText = possibleName.Contains("Unknown") ? newValue.ToString() : possibleName;
        _allowUpdate = true;

        if (oldValue == newValue) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.StateId = newValue;
    }

    // ---- Bounding box ----

    partial void OnBBoxMinXChanged(double value) => SetBoundingBox();
    partial void OnBBoxMinYChanged(double value) => SetBoundingBox();
    partial void OnBBoxMinZChanged(double value) => SetBoundingBox();
    partial void OnBBoxMaxXChanged(double value) => SetBoundingBox();
    partial void OnBBoxMaxYChanged(double value) => SetBoundingBox();
    partial void OnBBoxMaxZChanged(double value) => SetBoundingBox();

    private void SetBoundingBox()
    {
        if (_timeline is null || !ValidAnim()) return;
        var bb = _editor.CurrentAnim.DirectXAnimation.KeyFrames[_timeline.Value].BoundingBox;
        var newMin = new System.Numerics.Vector3((float)BBoxMinX, (float)BBoxMinY, (float)BBoxMinZ);
        var newMax = new System.Numerics.Vector3((float)BBoxMaxX, (float)BBoxMaxY, (float)BBoxMaxZ);
        if (newMin == bb.Minimum && newMax == bb.Maximum) return;

        PushUndoOnce();
        var deltaMin = newMin - bb.Minimum;
        var deltaMax = newMax - bb.Maximum;

        int start = _timeline.Value, end = _timeline.Value;
        if (!_editor.SelectionIsEmpty) { start = _editor.Selection.X; end = _editor.Selection.Y; }

        for (int i = start; i <= end; i++)
        {
            var bb2 = _editor.CurrentAnim.DirectXAnimation.KeyFrames[i].BoundingBox;
            bb2.Minimum += deltaMin;
            bb2.Maximum += deltaMax;
            _editor.CurrentAnim.DirectXAnimation.KeyFrames[i].BoundingBox = bb2;
        }
        AfterParameterChange();
    }

    // ---- Per-bone transform ----

    partial void OnTransformModeIndexChanged(int value)
    {
        _editor.TransformMode = (AnimTransformMode)value;
        if (_allowUpdate && _editor.MadeChanges) UpdateTransform();
    }

    partial void OnRotationXChanged(double value) => UpdateTransform();
    partial void OnRotationYChanged(double value) => UpdateTransform();
    partial void OnRotationZChanged(double value) => UpdateTransform();
    partial void OnTranslationXChanged(double value) => UpdateTransform();
    partial void OnTranslationYChanged(double value) => UpdateTransform();
    partial void OnTranslationZChanged(double value) => UpdateTransform();

    private void UpdateTransform()
    {
        if (_panel is null || !_allowUpdate || _editor.CurrentKeyFrame is null)
            return;

        PushUndoOnce();
        int meshIndex = _panel.SelectedMesh is null ? 0 : _panel.Model.Meshes.IndexOf(_panel.SelectedMesh);
        _editor.UpdateTransform(meshIndex,
            new System.Numerics.Vector3(TombLib.MathC.DegToRad((float)RotationX), TombLib.MathC.DegToRad((float)RotationY), TombLib.MathC.DegToRad((float)RotationZ)),
            new System.Numerics.Vector3((float)TranslationX, (float)TranslationY, (float)TranslationZ));
        _panel.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
        _panel.Invalidate();
    }

    // ---- Bone / mesh selection ----

    partial void OnSelectedBoneItemChanged(MeshBoneItem? value)
    {
        if (!_allowUpdate || _panel is null || value is null) return;
        _panel.SelectedMesh = value.Index < 0 ? null : _panel.Model.Meshes[value.Index];
        UpdateTransformUI();
        _panel.Invalidate();
    }

    // ---- Root motion (TEN) ----

    partial void OnRootPosXChanged(bool value) => ApplyRootMotion();
    partial void OnRootPosYChanged(bool value) => ApplyRootMotion();
    partial void OnRootPosZChanged(bool value) => ApplyRootMotion();
    partial void OnRootRotXChanged(bool value) => ApplyRootMotion();
    partial void OnRootRotYChanged(bool value) => ApplyRootMotion();
    partial void OnRootRotZChanged(bool value) => ApplyRootMotion();

    private void ApplyRootMotion()
    {
        if (!_allowUpdate || _editor.CurrentAnim is null) return;
        PushUndoOnce();
        var rootMotion = _editor.CurrentAnim.WadAnimation.RootMotion;
        rootMotion.TranslationX = RootPosX;
        rootMotion.TranslationY = RootPosY;
        rootMotion.TranslationZ = RootPosZ;
        rootMotion.RotationX = RootRotX;
        rootMotion.RotationY = RootRotY;
        rootMotion.RotationZ = RootRotZ;
        _editor.CurrentAnim.WadAnimation.RootMotion = rootMotion;
    }

    // ---- Blend curve preset ----

    partial void OnBlendPresetIndexChanged(int value)
    {
        if (_bezier is null || value < 0) return;
        _bezier.Value.Set(value switch
        {
            0 => BezierCurve2.Linear,
            1 => BezierCurve2.EaseIn,
            2 => BezierCurve2.EaseOut,
            _ => BezierCurve2.EaseInOut
        });
        _bezier.UpdateUI();
    }

    /// <summary>Called by the window when the blend-curve editor value changes (legacy resets the preset combo).</summary>
    public void OnBlendCurveEdited()
    {
        _allowUpdate = false;
        BlendPresetIndex = -1;
        _allowUpdate = true;
    }

    // ---- Transport toggles ----

    partial void OnChainPlaybackChanged(bool value) => _editor.Tool.Configuration.AnimationEditor_ChainPlayback = value;

    [RelayCommand]
    private void ToggleSound()
    {
        if (_editor.Tool.ReferenceLevel is null && !WadActions.LoadReferenceLevel(_editor.Tool, Owner)) return;
        _editor.Tool.Configuration.AnimationEditor_SoundPreview = !_editor.Tool.Configuration.AnimationEditor_SoundPreview;
        SoundPreview = _editor.Tool.Configuration.AnimationEditor_SoundPreview;
    }

    [RelayCommand]
    private void CycleSoundCondition()
    {
        if (_editor.Tool.ReferenceLevel is null && !WadActions.LoadReferenceLevel(_editor.Tool, Owner)) return;
        var t = _editor.Tool.Configuration.AnimationEditor_SoundPreviewType;
        bool isTEN = IsTombEngine;
        _editor.Tool.Configuration.AnimationEditor_SoundPreviewType = t switch
        {
            SoundPreviewType.Land => SoundPreviewType.LandWithMaterial,
            SoundPreviewType.LandWithMaterial => SoundPreviewType.Water,
            SoundPreviewType.Water => isTEN ? SoundPreviewType.Quicksand : SoundPreviewType.Land,
            SoundPreviewType.Quicksand when isTEN => SoundPreviewType.Underwater,
            _ => SoundPreviewType.Land
        };
        SoundConditionLabel = _editor.Tool.Configuration.AnimationEditor_SoundPreviewType.ToString();
    }

    private static IWindowOwner Owner => WPFUtils.GetWindowOwner();

    private void UpdateStatusLabel()
    {
        if (_timeline is null || _editor.CurrentAnim is null || _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == 0)
        {
            StatusText = string.Empty;
            return;
        }

        _frameCount = _timeline.Value * _editor.CurrentAnim.WadAnimation.FrameRate;
        StatusText = "Frame: " + _frameCount + " / " + Math.Max(_editor.GetRealNumberOfFrames() - 1, 0) +
                     "   Keyframe: " + _timeline.Value + " / " + Math.Max(_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1, 0);

        if (!_timeline.SelectionIsEmpty)
            StatusText += "   Selected keyframes: " + _timeline.Selection.X + " to " + _timeline.Selection.Y;
    }

    [RelayCommand]
    private void Play()
    {
        if (_playTimer is null || _panel is null || _timeline is null) return;

        IsPlaying = !IsPlaying;
        if (IsPlaying)
        {
            _playTimer.Start();
            if (_editor.CurrentAnim?.WadAnimation is not null)
            {
                _chainedInitialAnim = _editor.CurrentAnim.Index;
                _chainedInitialCursorPos = _timeline.Value;
                _chainedInitialSelection = _timeline.Selection;
                if (_editor.CurrentAnim.WadAnimation.KeyFrames.Count > 1)
                    _frameCount = _timeline.Value * _editor.CurrentAnim.WadAnimation.FrameRate;
            }
        }
        else
        {
            _playTimer.Stop();
            if (_editor.Tool.Configuration.AnimationEditor_ChainPlayback &&
                _editor.Tool.Configuration.AnimationEditor_RewindAfterChainPlayback)
            {
                var origNode = _editor.Animations.FirstOrDefault(item => item.Index == _chainedInitialAnim);
                if (origNode is not null && origNode != _editor.CurrentAnim)
                {
                    if (!SelectAnimByIndex(_chainedInitialAnim))
                        SelectAnimation(origNode);
                    else
                    {
                        _timeline.Value = _chainedInitialCursorPos;
                        _timeline.SelectionStart = _chainedInitialSelection.X;
                        _timeline.SelectionEnd = _chainedInitialSelection.Y;
                    }
                }
                else if (origNode == _editor.CurrentAnim &&
                         _editor.CurrentAnim.WadAnimation.NextFrame >= (_editor.GetRealNumberOfFrames() - 1) &&
                         _editor.GetRealFrameNumber() >= _editor.CurrentAnim.WadAnimation.NextFrame)
                    _timeline.Value = _chainedInitialCursorPos;
            }

            _blendState.Clear();
            _panel.DisablePicking = false;
        }

        _panel.GridPosition = System.Numerics.Vector3.Zero;
        _panel.Invalidate();
        _editor.Tool.TogglePlayback(IsPlaying, _editor.Tool.Configuration.AnimationEditor_ChainPlayback);
    }

    private bool SelectAnimByIndex(int index)
    {
        var item = Animations.FirstOrDefault(a => a.Node.Index == index);
        if (item is null) return false;
        if (!ReferenceEquals(item, SelectedAnim)) SelectedAnim = item;
        return true;
    }

    private void OnPlayTick()
    {
        if (_panel is null || _timeline is null ||
            _editor.CurrentAnim?.WadAnimation is null || _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count < 1)
            return;

        int realFrameNumber = _editor.GetRealNumberOfFrames();
        int realRangeNumber = _editor.CurrentAnim.WadAnimation.EndFrame > realFrameNumber - 1 ? realFrameNumber : _editor.CurrentAnim.WadAnimation.EndFrame + 1;

        _frameCount++;

        var nextIndex = _editor.CurrentAnim.WadAnimation.NextAnimation;
        var nextFrame = _editor.CurrentAnim.WadAnimation.NextFrame;
        var nextRange = new TombLib.VectorInt2(realRangeNumber);

        bool chain = _editor.Tool.Configuration.AnimationEditor_ChainPlayback;

        if (chain && _chainedIncomingAnimation >= 0)
        {
            if (_chainedIncomingAnimation < _editor.Animations.Count &&
                _chainedIncomingFrame >= 0 &&
                _chainedIncomingFrame < _editor.GetRealNumberOfFrames(_chainedIncomingAnimation) &&
                _chainedIncomingFrameRange.X >= 0 &&
                _chainedIncomingFrameRange.Y >= _chainedIncomingFrameRange.X &&
                _chainedIncomingFrameRange.Y <= realFrameNumber)
            {
                nextIndex = (ushort)_chainedIncomingAnimation;
                nextFrame = (ushort)_chainedIncomingFrame;
                nextRange = _chainedIncomingFrameRange;
            }
            else
            {
                ShowPopup("Pending state change to animation #" + _chainedIncomingAnimation + " had incorrect data and was ignored.", PopupType.Error);
                _chainedIncomingAnimation = -1;
                _blendState.ClearPendingBlend();
            }
        }

        if (_frameCount >= nextRange.X && _frameCount <= nextRange.Y)
        {
            if (chain)
            {
                bool blendStarted = _blendState.TryBegin(_editor.CurrentAnim, _frameCount, _editor.Tool.Configuration.AnimationEditor_SmoothAnimation);
                _panel.DisablePicking = blendStarted;
                _chainedIncomingAnimation = -1;

                var nextNode = _editor.Animations.FirstOrDefault(item => item.Index == nextIndex);
                if (nextNode is not null)
                {
                    if (_editor.Tool.Configuration.AnimationEditor_ScrollGrid && _editor.CurrentAnim.WadAnimation.AnimCommands.Count > 0 &&
                        _editor.CurrentAnim.WadAnimation.AnimCommands.Any(cmd => cmd.Type == WadAnimCommandType.SetPosition))
                        _chainedSetPosRecoveryCount = 0;

                    _editor.CurrentAnim.WadAnimation.AnimCommands
                        .Where(cmd => cmd.Type == WadAnimCommandType.SetPosition)
                        .ToList()
                        .ForEach(cmd => _panel.GridPosition += new System.Numerics.Vector3(cmd.Parameter1, cmd.Parameter2, cmd.Parameter3));

                    if (nextNode != _editor.CurrentAnim)
                        SelectAnimByIndex(nextIndex);

                    var maxFrameNumber = _editor.GetRealNumberOfFrames(nextIndex);
                    if (nextFrame >= maxFrameNumber)
                    {
                        _frameCount = 0;
                        ShowPopup("No frame " + nextFrame + " in animation " + nextIndex + ". Using first frame.", PopupType.Warning);
                    }
                    else
                        _frameCount = nextFrame;
                }
                else
                    ShowPopup("Animation " + nextIndex + " wasn't found. Chain is broken.", PopupType.Warning);
            }
            else
                _frameCount = 0;
        }

        if (_editor.Tool.Configuration.AnimationEditor_ScrollGrid)
        {
            if (!chain && _frameCount >= (realRangeNumber - 1) &&
                _editor.CurrentAnim.WadAnimation.NextAnimation != _editor.CurrentAnim.Index)
                _panel.GridPosition = System.Numerics.Vector3.Zero;

            var startVel = new Vector3(_editor.CurrentAnim.WadAnimation.StartLateralVelocity, 0.0f, _editor.CurrentAnim.WadAnimation.StartVelocity);
            var endVel = new Vector3(_editor.CurrentAnim.WadAnimation.EndLateralVelocity, 0.0f, _editor.CurrentAnim.WadAnimation.EndVelocity);

            if (_editor.Moveable.Id.TypeId == 0)
            {
                startVel = GetPreviewVelocity(startVel);
                endVel = GetPreviewVelocity(endVel);
            }

            var shiftX = Vector3.Lerp(startVel, endVel, (float)_frameCount / realRangeNumber);
            _panel.GridPosition += shiftX;

            if (_editor.Tool.Configuration.AnimationEditor_RecoverGridAfterPositionChange)
            {
                if (_panel.GridPosition.Y != 0 &&
                    !_editor.CurrentAnim.WadAnimation.AnimCommands.Any(cmd => cmd.Type == WadAnimCommandType.SetPosition) &&
                    _editor.CurrentAnim.WadAnimation.NextAnimation == _editor.CurrentAnim.Index)
                    _chainedSetPosRecoveryCount++;
                else
                    _chainedSetPosRecoveryCount = 0;

                if (_chainedSetPosRecoveryCount >= _gridRecoveryWaitInterval)
                {
                    _gridRecoveryCount += _gridRecoveryStep;
                    var shiftY = new System.Numerics.Vector3(_panel.GridPosition.X, 0.0f, _panel.GridPosition.Z);
                    if (_gridRecoveryCount < 1.0f)
                        shiftY.Y = (float)TombLib.MathC.SmoothStep(_panel.GridPosition.Y, 0.0f, _gridRecoveryCount);
                    else { _gridRecoveryCount = 0.0f; _chainedSetPosRecoveryCount = 0; }
                    _panel.GridPosition = shiftY;
                }
            }
        }

        byte frameRate = _editor.CurrentAnim.WadAnimation.FrameRate == 0 ? (byte)1 : _editor.CurrentAnim.WadAnimation.FrameRate;
        bool isKeyFrame = _frameCount % frameRate == 0;

        if (isKeyFrame)
        {
            int newFrameNumber = (int)Math.Round((double)(_frameCount / frameRate));
            _timeline.Value = newFrameNumber > _timeline.Maximum ? 0 : newFrameNumber;
        }
        else if (_editor.Tool.Configuration.AnimationEditor_SmoothAnimation)
        {
            float k = (float)_frameCount / frameRate;
            k = _frameCount == realFrameNumber - 1 ? 1.0f : k - (float)Math.Floor(k);
            SelectFrame(k);
        }

        if (_blendState.IsActive)
        {
            _blendState.BuildPose(_panel.Model, _editor.CurrentAnim, _frameCount, _editor.Tool.Configuration.AnimationEditor_SmoothAnimation);
            _panel.Invalidate();
            if (!_blendState.Advance())
                _panel.DisablePicking = false;
        }

        UpdateStatusLabel();
    }

    private Vector3 GetPreviewVelocity(Vector3 velocity) => _editor.CurrentAnim.WadAnimation.StateId switch
    {
        5 or 16 or 23 or 25 or 32 or 47 => new Vector3(velocity.X, velocity.Y, -velocity.Z),
        86 or 163 when _editor.Wad.GameVersion == TRVersion.Game.TombEngine => new Vector3(velocity.X, velocity.Y, -velocity.Z),
        21 or 26 or 31 or 49 or 60 or 78 => new Vector3(velocity.Z, 0.0f, velocity.X),
        22 or 27 or 30 or 48 or 58 or 77 => new Vector3(-velocity.Z, 0.0f, -velocity.X),
        _ => velocity
    };

    private void ShowPopup(string message, PopupType type) => PopUpInfo.Show(_popup, null, _panel, message, type);

    private void PreviewSounds()
    {
        if (!_editor.Tool.Configuration.AnimationEditor_SoundPreview || _editor.Tool.ReferenceLevel is null)
            return;

        _overallPlaybackCount++;
        if (_overallPlaybackCount > _materialIndexSwitchInterval)
        {
            _overallPlaybackCount = 0;
            var materialSounds = _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap
                .Where(s => s.Name.IndexOf("FOOTSTEPS_", StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
            if (materialSounds.Count > 1)
                while (true)
                {
                    var newMaterialIndex = (new Random()).Next(0, materialSounds.Count - 1);
                    if (materialSounds.Count == 1 || newMaterialIndex != _currentMaterialIndex)
                    {
                        _currentMaterialIndex = materialSounds[newMaterialIndex].Id;
                        break;
                    }
                }
        }

        var previewSoundType = _editor.Tool.Configuration.AnimationEditor_SoundPreviewType;
        foreach (var ac in _editor.CurrentAnim.WadAnimation.AnimCommands)
        {
            int idToPlay = -1;
            if (ac.Type == WadAnimCommandType.PlaySound)
                idToPlay = ac.Parameter2;
            else if (ac.Type == WadAnimCommandType.FlipEffect && previewSoundType == SoundPreviewType.LandWithMaterial &&
                     _editor.Wad.GameVersion.Native() >= TRVersion.Game.TR3)
            {
                var flipID = ac.Parameter2;
                if (flipID == 32 || flipID == 33) idToPlay = _currentMaterialIndex;
            }

            if (idToPlay == -1 || ac.Parameter1 != _frameCount)
                continue;

            var soundType = ac.Type == WadAnimCommandType.FlipEffect ? WadSoundEnvironmentType.Land : (WadSoundEnvironmentType)ac.Parameter3;

            if (ac.Type == WadAnimCommandType.FlipEffect &&
                (previewSoundType == SoundPreviewType.Water || previewSoundType == SoundPreviewType.Quicksand || previewSoundType == SoundPreviewType.Underwater))
                continue;
            if (soundType == WadSoundEnvironmentType.Land && !(previewSoundType == SoundPreviewType.Land || previewSoundType == SoundPreviewType.LandWithMaterial))
                continue;
            if (soundType == WadSoundEnvironmentType.Water && previewSoundType != SoundPreviewType.Water) continue;
            if (soundType == WadSoundEnvironmentType.Quicksand && previewSoundType != SoundPreviewType.Quicksand) continue;
            if (soundType == WadSoundEnvironmentType.Underwater && previewSoundType != SoundPreviewType.Underwater) continue;

            var soundInfo = _editor.Tool.ReferenceLevel.Settings.GlobalSoundMap.FirstOrDefault(s => s.Id == idToPlay);
            if (soundInfo is null)
            {
                ShowPopup("Sound info " + idToPlay + " missing in reference project", PopupType.Warning);
                continue;
            }

            if (!_editor.Tool.ReferenceLevel.Settings.SelectedSounds.Contains(idToPlay))
                ShowPopup("Sound info " + idToPlay + " is disabled in level settings.", PopupType.Warning);
            else
                try { WadSoundPlayer.PlaySoundInfo(_editor.Tool.ReferenceLevel, soundInfo); }
                catch (Exception exc) { ShowPopup("Unable to play sound info " + idToPlay + ". Exception: \n" + exc.Message, PopupType.Warning); }
        }
    }

    private void UpdateReferenceLevel()
    {
        if (_panel is null) return;
        _panel.Level = _editor.Tool.ReferenceLevel;
        _panel.Invalidate();

        _allowUpdate = false;
        Rooms.Clear();
        if (_editor.Tool.ReferenceLevel is not null)
        {
            RoomsEnabled = true;
            Rooms.Add("(select room)");
            foreach (var room in _editor.Tool.ReferenceLevel.Rooms)
                if (room is not null) Rooms.Add(room);
            SelectedRoomItem = Rooms.Count > 0 ? Rooms[0] : null;
        }
        else
            RoomsEnabled = false;
        _allowUpdate = true;
    }

    partial void OnSelectedRoomItemChanged(object? value)
    {
        if (!_allowUpdate || _panel is null) return;
        if (value is Room room)
        {
            _panel.Room = room;
            _panel.RoomPosition = room.GetLocalCenter();
        }
        else
        {
            _panel.Room = null;
            _panel.RoomPosition = System.Numerics.Vector3.Zero;
        }
        _panel.Invalidate();
    }

    [RelayCommand]
    private void GotoStart() { if (_timeline is not null) _timeline.Value = _timeline.Minimum; }
    [RelayCommand]
    /// <summary>Steps one frame and wraps around at the ends (keyboard navigation of the previous form).</summary>
    public void StepFrameLooping(bool forward)
    {
        if (_timeline is null)
            return;

        if (forward)
            _timeline.ValueLoopInc();
        else
            _timeline.ValueLoopDec();
    }

    /// <summary>Marks the start (in) or end (out) of the frame selection at the current frame.</summary>
    public void MarkSelection(bool start)
    {
        if (_timeline is null)
            return;

        if (start)
            _timeline.SelectionStart = _timeline.Value;
        else
            _timeline.SelectionEnd = _timeline.Value;
    }

    public void SelectAllFrames() => _timeline?.SelectAll();

    /// <summary>Clears the mesh, bone list and timeline selections (Escape).</summary>
    public void ClearSelections()
    {
        SelectedBoneItem = null;
        _timeline?.ResetSelection();
    }

    private void GotoPrev() { if (_timeline is not null && _timeline.Value > _timeline.Minimum) _timeline.Value--; }
    [RelayCommand]
    private void GotoNext() { if (_timeline is not null && _timeline.Value < _timeline.Maximum) _timeline.Value++; }
    [RelayCommand]
    private void GotoEnd() { if (_timeline is not null) _timeline.Value = _timeline.Maximum; }

    [RelayCommand]
    private void ResetCamera() => _panel?.ResetCamera();

    // ---- Keyframe & animation operations (slice 3c) ----

    private void OnKeyframesListChanged()
    {
        if (_timeline is null) return;
        if (_editor.ValidAnimationAndFrames)
        {
            _timeline.Minimum = 0;
            _timeline.Maximum = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;
            _allowUpdate = false;
            EndFrame = _editor.CurrentAnim.WadAnimation.EndFrame;
            _allowUpdate = true;
            UpdateStatusLabel();
        }
        else
            StatusText = string.Empty;
    }

    private void FixEndFrame(int delta)
    {
        var v = Math.Max(0, _editor.CurrentAnim.WadAnimation.EndFrame + delta);
        _editor.CurrentAnim.WadAnimation.EndFrame = (ushort)v;
    }

    private bool ValidAndSelected(bool prompt = true)
    {
        if (_timeline is null) return false;
        if (_timeline.SelectionIsEmpty)
        { if (prompt) ShowPopup("No frames selected. Please select at least 1 frame.", PopupType.Error); }
        else if (_editor.CurrentAnim is null)
        { if (prompt) ShowPopup("No animation selected. Select animation to work with.", PopupType.Error); }
        else if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == 0)
        { if (prompt) ShowPopup("Current animation contains no frames.", PopupType.Error); }
        else
            return true;
        return false;
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo() => _editor.Tool.UndoManager.Undo();
    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo() => _editor.Tool.UndoManager.Redo();
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        _editor.SaveChanges();
        SetSaved(true);
    }

    private void SetSaved(bool saved)
    {
        if (_isSaved == saved)
            return;

        _isSaved = saved;
        SaveCommand.NotifyCanExecuteChanged();
        Title = "Animation editor - " + _editor.Moveable.Id.ToString(_editor.Wad.GameVersion) + (saved ? "" : "*");
    }

    [RelayCommand]
    private void AddAnimation()
    {
        var wadAnimation = new WadAnimation { FrameRate = 1, Name = "New Animation " + _editor.Animations.Count };
        var keyFrame = new WadKeyFrame();
        foreach (var bone in _editor.Moveable.Bones)
            keyFrame.Angles.Add(new WadKeyFrameRotation());
        wadAnimation.KeyFrames.Add(keyFrame);

        var dxAnimation = Animation.FromWad2(_editor.Moveable.Bones, wadAnimation);
        var node = new AnimationNode(wadAnimation, dxAnimation, _editor.Animations.Count);

        _editor.Animations.Add(node);
        var item = new AnimListItem(node, "(" + node.Index + ") " + node.WadAnimation.Name);
        Animations.Add(item);
        SelectedAnim = item;
    }

    [RelayCommand]
    private void AddFrame()
    {
        if (_timeline is null) return;
        AddNewFrame(_timeline.Value + 1, true);
    }

    private void AddNewFrame(int index, bool undo)
    {
        if (_editor.CurrentAnim is null || _timeline is null) return;
        if (undo) _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        KeyFrame keyFrame;
        if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == 0)
        {
            index = 0;
            keyFrame = new KeyFrame();
            foreach (var bone in _editor.Moveable.Bones)
            {
                keyFrame.Rotations.Add(System.Numerics.Vector3.Zero);
                keyFrame.Quaternions.Add(System.Numerics.Quaternion.Identity);
                keyFrame.Translations.Add(bone.Translation);
                keyFrame.TranslationsMatrices.Add(System.Numerics.Matrix4x4.CreateTranslation(bone.Translation));
            }
        }
        else
            keyFrame = _editor.CurrentKeyFrame.Clone();

        _editor.CurrentAnim.DirectXAnimation.KeyFrames.Insert(index, keyFrame);
        OnKeyframesListChanged();
        FixEndFrame(1);
    }

    [RelayCommand]
    private void DeleteFrame() => DeleteFrames(true, true, true);

    private void DeleteFrames(bool prompt, bool undo, bool updateGUI)
    {
        if (_timeline is null || _panel is null || !ValidAndSelected(prompt)) return;

        if (prompt &&
            UserDialogs.Show(Owner, "Do you really want to delete frame" +
                (_timeline.SelectionSize == 1 ? " " + _timeline.Value : "s " + _timeline.Selection.X + "-" + _timeline.Selection.Y) + "?",
                "Confirm", DialogButtons.YesNo, DialogIcon.Question) == DialogChoice.No)
            return;

        if (undo) _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        int selectionStart = _timeline.Selection.X;
        int selectionEnd = _timeline.Selection.Y;
        int cursorPos = (_timeline.Value < _timeline.Selection.X || _timeline.Value > _timeline.Selection.Y) ? _timeline.Value : -1;

        _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveRange(_timeline.Selection.X, _timeline.SelectionSize);

        if (!updateGUI) return;

        FixEndFrame(-_timeline.SelectionSize);
        _timeline.ResetSelection();
        OnKeyframesListChanged();

        if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count != 0)
            _timeline.Value = cursorPos != -1
                ? (cursorPos < selectionStart ? cursorPos : selectionStart + (cursorPos - selectionEnd - 1))
                : selectionStart;
        else
        {
            _timeline.Value = 0;
            StatusText = string.Empty;
        }
        _panel.Invalidate();
    }

    [RelayCommand]
    private void CutFrame()
    {
        if (!ValidAndSelected()) return;
        CopyFramesInternal(false);
        DeleteFrames(false, true, true);
    }

    [RelayCommand]
    private void CopyFrame() => CopyFramesInternal(true);

    private void CopyFramesInternal(bool updateGUI)
    {
        if (_timeline is null || !ValidAndSelected()) return;
        _editor.ClipboardKeyFrames.Clear();
        for (int i = _timeline.Selection.X; i <= _timeline.Selection.Y; i++)
            _editor.ClipboardKeyFrames.Add(_editor.CurrentAnim.DirectXAnimation.KeyFrames[i].Clone());
        if (updateGUI)
            _timeline.Highlight(_timeline.Selection.X, _timeline.Selection.Y);
    }

    [RelayCommand]
    private void PasteFrame()
    {
        if (_timeline is null || _panel is null || _editor.CurrentAnim is null) return;
        if (_editor.ClipboardKeyFrames is null || _editor.ClipboardKeyFrames.Count <= 0)
        {
            ShowPopup("Nothing to paste!", PopupType.Warning);
            return;
        }

        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        int startIndex = _timeline.SelectionIsEmpty ? _timeline.Value : _timeline.Selection.X;
        int endIndex = _timeline.SelectionIsEmpty ? _timeline.Value : _timeline.Selection.Y;
        int cursorPos = (_timeline.SelectionIsEmpty || _timeline.Value < _timeline.Selection.X || _timeline.Value > _timeline.Selection.Y) ? _timeline.Value : -1;

        if (!_timeline.SelectionIsEmpty)
            DeleteFrames(false, false, false);

        var pastedFrames = new System.Collections.Generic.List<KeyFrame>();
        _editor.ClipboardKeyFrames.ForEach(frame => pastedFrames.Add(frame.Clone()));
        _editor.CurrentAnim.DirectXAnimation.KeyFrames.InsertRange(startIndex, pastedFrames);
        OnKeyframesListChanged();
        FixEndFrame(pastedFrames.Count - _timeline.SelectionSize);

        int insertEnd = startIndex + _editor.ClipboardKeyFrames.Count - 1;
        if (cursorPos != -1 && cursorPos <= _timeline.Maximum)
            _timeline.Value = cursorPos < startIndex ? cursorPos : insertEnd + (cursorPos - endIndex) + (_timeline.SelectionIsEmpty ? 1 : 0);
        else
            _timeline.Value = insertEnd;

        _timeline.ResetSelection();
        _timeline.Highlight(startIndex, insertEnd);
        _panel.Invalidate();
    }

    [RelayCommand]
    private void EditAnimCommands() => EditAnimCommands(null);

    /// <summary>Opens the anim commands editor, optionally selecting the given command.</summary>
    public void EditAnimCommands(WadAnimCommand? command)
    {
        if (_editor.CurrentAnim is null) return;
        var vm = new AnimCommandsEditorWindowViewModel(_editor, _editor.CurrentAnim, command);
        var dialog = new AnimCommandsEditorWindow { DataContext = vm };
        dialog.SetOwner(Owner);
        dialog.ShowDialog();
        _editor.Tool.AnimationEditorAnimationChanged(_editor.CurrentAnim, false);
    }

    [RelayCommand]
    private void EditStateChanges() => EditStateChanges(null);

    private void EditStateChanges(WadStateChange? newStateChange)
    {
        if (_editor.CurrentAnim is null) return;
        var vm = new StateChangesEditorWindowViewModel(_editor, _editor.CurrentAnim, newStateChange);
        var dialog = new StateChangesEditorWindow { DataContext = vm };
        dialog.SetOwner(Owner);
        dialog.ShowDialog();
    }

    [RelayCommand]
    private void FixAnimation() => FixAnimations(AnimationFixScope.Current);

    // ---- Animation operations (slice 3d) ----

    [RelayCommand]
    private void DeleteAnimation()
    {
        if (_editor.CurrentAnim is null) return;
        if (UserDialogs.Show(Owner, "Do you really want to delete '" + _editor.CurrentAnim.WadAnimation.Name + "'?",
                "Confirm", DialogButtons.YesNo, DialogIcon.Question) == DialogChoice.No)
            return;
        DeleteAnimationCore(_editor.CurrentAnim);
    }

    private void DeleteAnimationCore(AnimationNode animToDelete, bool updateUi = true)
    {
        if (animToDelete is null || !_editor.Animations.Contains(animToDelete)) return;

        int currentIndex = _editor.Animations.IndexOf(animToDelete);
        for (int i = 0; i < _editor.Animations.Count; i++)
        {
            if (i == currentIndex) continue;
            var animation = _editor.Animations[i];
            if (animation.Index > currentIndex) animation.Index--;
            if (animation.WadAnimation.NextAnimation > currentIndex) animation.WadAnimation.NextAnimation--;
            foreach (var stateChange in animation.WadAnimation.StateChanges)
                foreach (var dispatch in stateChange.Dispatches)
                    if (dispatch.NextAnimation > currentIndex) dispatch.NextAnimation--;
        }

        _editor.Animations.Remove(animToDelete);

        if (!updateUi)
            return;

        RebuildAnimationsList();
        SetSaved(false);

        if (Animations.Count > 0)
            SelectedAnim = Animations[Math.Min(currentIndex, Animations.Count - 1)];
        else
            SelectAnimation(null);

        _panel?.Invalidate();
        _timeline?.Invalidate();
    }

    [RelayCommand]
    private void CutAnimation()
    {
        if (_editor.CurrentAnim is null) { ShowPopup("No animation to cut!", PopupType.Warning); return; }
        _editor.ClipboardNode = _editor.CurrentAnim;
        DeleteAnimationCore(_editor.CurrentAnim);
    }

    [RelayCommand]
    private void CopyAnimation()
    {
        if (_editor.CurrentAnim is null) { ShowPopup("No animation to copy!", PopupType.Warning); return; }
        _editor.ClipboardNode = _editor.CurrentAnim.Clone();
    }

    [RelayCommand]
    private void PasteAnimation()
    {
        if (_editor.ClipboardNode is null || _editor.CurrentAnim is null) { ShowPopup("No animation to paste!", PopupType.Warning); return; }
        int animationIndex = _editor.Animations.Count;
        var pastedAnim = _editor.ClipboardNode.Clone(animationIndex);
        _editor.Animations.Add(pastedAnim);
        pastedAnim.WadAnimation.Name += " - Copy";
        RebuildAnimationsList();
        SelectAnimByIndex(animationIndex);
    }

    [RelayCommand]
    private void ReplaceAnimation()
    {
        if (_editor.ClipboardNode is null || _editor.CurrentAnim is null) { ShowPopup("No animation to replace!", PopupType.Warning); return; }
        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
        int animationIndex = _editor.CurrentAnim.Index;
        var pastedAnim = _editor.ClipboardNode.Clone(animationIndex);
        _editor.Animations[animationIndex] = pastedAnim;
        pastedAnim.WadAnimation.Name += " - Copy";
        RebuildAnimationsList();
        SelectAnimByIndex(animationIndex);
    }

    [RelayCommand]
    private void SplitAnimation()
    {
        if (_timeline is null || _editor.CurrentAnim is null) { ShowPopup("No animation to split!", PopupType.Warning); return; }

        if (_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count < 3)
        { ShowPopup("You must have at least 3 frames for splitting the animation", PopupType.Error); return; }

        int numFrames = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
        if (_timeline.Value == 0 || _timeline.Value == numFrames - 1)
        { ShowPopup("You can't set the first or the last frame for splitting the animation", PopupType.Error); return; }

        var newWadAnimation = _editor.CurrentAnim.WadAnimation.Clone();
        var newDirectXAnimation = _editor.CurrentAnim.DirectXAnimation.Clone();
        int numFrames2 = numFrames - _timeline.Value;

        _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveRange(_timeline.Value + 1, numFrames2 - 1);
        newDirectXAnimation.KeyFrames.RemoveRange(0, _timeline.Value);

        newWadAnimation.Name += " - splitted";
        _editor.Animations.Add(new AnimationNode(newWadAnimation, newDirectXAnimation, _editor.Animations.Count));

        RebuildAnimationsList();
        SelectAnimByIndex(_editor.Animations.Count - 1);
    }

    // ---- Bounding box (slice 3d) ----

    private System.Collections.Generic.List<int> GetSelectedMeshList()
        => Bones.Where(b => b.Checked).Select(b => b.Index).ToList();

    [RelayCommand]
    private void SelectAllMeshes() { foreach (var b in Bones) b.Checked = true; }
    [RelayCommand]
    private void SelectNoMeshes() { foreach (var b in Bones) b.Checked = false; }

    [RelayCommand]
    private void CalcBoundingBox() => CalculateAnimationBoundingBox(false);
    [RelayCommand]
    private void ClearBoundingBox() => CalculateAnimationBoundingBox(true);

    private void CalculateAnimationBoundingBox(bool clear)
    {
        if (_panel is null || _timeline is null || !_editor.ValidAnimationAndFrames) return;
        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        int start = 0, end = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
        if (!_timeline.SelectionIsEmpty) { start = _timeline.Selection.X; end = _timeline.Selection.Y + 1; }

        for (int i = start; i < end; i++)
            CalculateKeyframeBoundingBox(i, clear);

        _panel.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
        _panel.Invalidate();
    }

    private void CalculateKeyframeBoundingBox(int index, bool clear, bool undo = false)
    {
        if (_panel is null || _timeline is null || !_editor.ValidAnimationAndFrames) return;
        if (undo) _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        var meshList = GetSelectedMeshList();
        var keyFrame = _editor.CurrentAnim.DirectXAnimation.KeyFrames[index];
        _panel.Model.BuildAnimationPose(keyFrame);

        if (clear || meshList.Count == 0)
            keyFrame.BoundingBox = new TombLib.BoundingBox();
        else
            keyFrame.CalculateBoundingBox(_panel.Model, _panel.Skin, meshList);

        if (index == _timeline.Value)
        {
            UpdateTransformUI();
            _panel.Invalidate();
        }

        SetSaved(false);
    }

    [RelayCommand]
    private void GrowBoundingBox() => InflateAnimationBoundingBox(new System.Numerics.Vector3((float)GrowX, (float)GrowY, (float)GrowZ));
    [RelayCommand]
    private void ShrinkBoundingBox() => InflateAnimationBoundingBox(new System.Numerics.Vector3((float)-GrowX, (float)-GrowY, (float)-GrowZ));

    private void InflateAnimationBoundingBox(System.Numerics.Vector3 value)
    {
        if (_timeline is null || !_editor.ValidAnimationAndFrames || value.Length() == 0f) return;
        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        int start = 0, end = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
        if (!_timeline.SelectionIsEmpty) { start = _timeline.Selection.X; end = _timeline.Selection.Y + 1; }

        for (int i = start; i < end; i++)
        {
            var kf = _editor.CurrentAnim.DirectXAnimation.KeyFrames[i];
            kf.BoundingBox = kf.BoundingBox.Inflate(value);
        }

        if (_panel is not null)
        {
            UpdateTransformUI();
            _panel.Invalidate();
        }
    }

    // ---- View / config toggles (slice 3e) ----

    partial void OnScrollGridChanged(bool value) => _editor.Tool.Configuration.AnimationEditor_ScrollGrid = value;
    partial void OnRestoreGridHeightChanged(bool value) => _editor.Tool.Configuration.AnimationEditor_RecoverGridAfterPositionChange = value;
    partial void OnSmoothAnimationChanged(bool value) => _editor.Tool.Configuration.AnimationEditor_SmoothAnimation = value;
    partial void OnShowGizmoChanged(bool value) { _editor.Tool.Configuration.AnimationEditor_ShowGizmo = value; _panel?.Invalidate(); }
    partial void OnShowGridChanged(bool value) { _editor.Tool.Configuration.AnimationEditor_ShowGrid = value; _panel?.Invalidate(); }
    partial void OnShowCollisionBoxChanged(bool value) { _editor.Tool.Configuration.AnimationEditor_ShowCollisionBox = value; _panel?.Invalidate(); }
    partial void OnShowSkinChanged(bool value) { _editor.Tool.Configuration.AnimationEditor_ShowSkin = value; _panel?.Invalidate(); }

    // ---- Interpolate / reverse / mirror (slice 3e) ----

    [RelayCommand]
    private void Interpolate()
    {
        if (_timeline is null || !ValidAndSelected()) return;

        int numFrames;
        var selection = TombLib.MathC.Clamp(_timeline.Selection.Y - _timeline.Selection.X - 1, 0, int.MaxValue).ToString();
        string input = InputBox.Show(Owner, "Interpolation", "Enter number of interpolated frames:", selection);
        if (input is null) return;
        if (!int.TryParse(input, out numFrames)) numFrames = 3;
        if (numFrames == 0) { ShowPopup("Interpolation requires at least 1 frame to insert", PopupType.Error); return; }

        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
        int start = _timeline.Selection.X;
        int end = (_timeline.SelectionSize == 1 && _timeline.Selection.Y == _timeline.Maximum) ? _timeline.Minimum : _timeline.Selection.Y;
        InterpolateFrames(start, end, numFrames);
    }

    private void InterpolateFrames(int frameIndex1, int frameIndex2, int numFrames, bool updateGui = true)
    {
        if (_timeline is null || _panel is null) return;
        var frame1 = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex1];
        var frame2 = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex2];

        if (frameIndex2 - frameIndex1 > 1)
            _editor.CurrentAnim.DirectXAnimation.KeyFrames.RemoveRange(frameIndex1 + 1, frameIndex2 - frameIndex1 - 1);

        for (int i = 0; i < numFrames; i++)
        {
            var keyFrame = new KeyFrame();
            foreach (var bone in _editor.Moveable.Bones)
            {
                keyFrame.Rotations.Add(System.Numerics.Vector3.Zero);
                keyFrame.Quaternions.Add(System.Numerics.Quaternion.Identity);
                keyFrame.Translations.Add(frame1.Translations[0] + bone.Translation);
                keyFrame.TranslationsMatrices.Add(System.Numerics.Matrix4x4.CreateTranslation(frame1.Translations[0] + bone.Translation));
            }
            _editor.CurrentAnim.DirectXAnimation.KeyFrames.Insert(frameIndex1 + 1 + i, keyFrame);
        }

        float k = 1.0f / (numFrames + 1);
        for (int i = 0; i < numFrames; i++)
        {
            var keyframe = _editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex1 + i + 1];
            keyframe.Translations[0] = System.Numerics.Vector3.Lerp(frame1.Translations[0], frame2.Translations[0], k * (i + 1));
            keyframe.TranslationsMatrices[0] = System.Numerics.Matrix4x4.CreateTranslation(keyframe.Translations[0]);
            for (int j = 0; j < keyframe.Quaternions.Count; j++)
            {
                keyframe.Quaternions[j] = System.Numerics.Quaternion.Slerp(frame1.Quaternions[j], frame2.Quaternions[j], k * (i + 1));
                keyframe.Rotations[j] = TombLib.MathC.QuaternionToEuler(keyframe.Quaternions[j]);
            }
            var bbMin = System.Numerics.Vector3.Lerp(frame1.BoundingBox.Minimum, frame2.BoundingBox.Minimum, k * (i + 1));
            var bbMax = System.Numerics.Vector3.Lerp(frame1.BoundingBox.Maximum, frame2.BoundingBox.Maximum, k * (i + 1));
            keyframe.BoundingBox = new TombLib.BoundingBox(bbMin, bbMax);
        }

        FixEndFrame(numFrames);

        if (!updateGui)
            return;

        OnKeyframesListChanged();
        _timeline.Highlight(frameIndex1, frameIndex1 + numFrames + 1);
        _timeline.ResetSelection();
        _timeline.Value = frameIndex1 + numFrames + 1;
        ShowPopup("Successfully inserted " + numFrames + " interpolated frames between frames " + frameIndex1 + " and " + frameIndex2, PopupType.Info);
    }

    [RelayCommand]
    private void Reverse()
    {
        if (_timeline is null || _panel is null || !_editor.ValidAnimationAndFrames) return;
        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        int start = 0, end = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
        if (!_editor.SelectionIsEmpty) { start = _editor.Selection.X; end = _editor.Selection.Y; }

        _editor.CurrentAnim.DirectXAnimation.KeyFrames.Reverse(start, end - start);
        foreach (var ac in _editor.CurrentAnim.WadAnimation.AnimCommands)
            if (ac.FrameBased && ac.Parameter1 >= start && ac.Parameter1 <= end)
                ac.Parameter1 = (short)((_editor.CurrentAnim.WadAnimation.FrameRate * end) - (ac.Parameter1 - (_editor.CurrentAnim.WadAnimation.FrameRate * start)));

        _timeline.Highlight(start, end);
        SelectFrame();
        UpdateTransformUI();
    }

    [RelayCommand]
    private void Mirror()
    {
        if (_timeline is null || _panel is null || !_editor.ValidAnimationAndFrames) return;

        int start = 0, end = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;
        if (!_editor.SelectionIsEmpty) { start = _editor.Selection.X; end = _editor.Selection.Y; }

        float accumulatedRotation = 0;
        foreach (var kf in _editor.CurrentAnim.DirectXAnimation.KeyFrames)
            accumulatedRotation += Math.Abs((Math.Round(TombLib.MathC.RadToDeg(kf.Rotations[0]).Y / 90.0f) * 90.0f) % 180.0f) != 0 ? 1 : 0;
        accumulatedRotation /= _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
        bool flipZ = accumulatedRotation > 0.5f;

        var bonePairs = _panel.Model.GetBonePairs(flipZ);
        if (bonePairs is null || bonePairs.Count == 0) { ShowPopup("No valid bones were found for mirroring.", PopupType.Error); return; }
        if (flipZ) ShowPopup("Broken identity rotation detected. Impossible to mirror animation correctly.", PopupType.Warning);

        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        for (int i = start; i <= end; i++)
        {
            var frame = _editor.CurrentAnim.DirectXAnimation.KeyFrames[i];
            frame.Translations[0] = new System.Numerics.Vector3(-frame.Translations[0].X, frame.Translations[0].Y, frame.Translations[0].Z);
            var newMin = new System.Numerics.Vector3(-frame.BoundingBox.Minimum.X, frame.BoundingBox.Minimum.Y, frame.BoundingBox.Minimum.Z);
            var newMax = new System.Numerics.Vector3(-frame.BoundingBox.Maximum.X, frame.BoundingBox.Maximum.Y, frame.BoundingBox.Maximum.Z);
            frame.BoundingBox = new TombLib.BoundingBox(newMin, newMax);

            foreach (var pair in bonePairs)
            {
                if (pair[1] != -1)
                {
                    var r1 = TombLib.MathC.RadToDeg(frame.Rotations[pair[0]]);
                    var r2 = TombLib.MathC.RadToDeg(frame.Rotations[pair[1]]);
                    var nr1 = TombLib.MathC.DegToRad(new System.Numerics.Vector3(r2.X, (360.0f - r2.Y) % 360.0f, (360.0f - r2.Z) % 360.0f));
                    var nr2 = TombLib.MathC.DegToRad(new System.Numerics.Vector3(r1.X, (360.0f - r1.Y) % 360.0f, (360.0f - r1.Z) % 360.0f));
                    frame.Rotations[pair[0]] = nr1;
                    frame.Rotations[pair[1]] = nr2;
                    frame.Quaternions[pair[0]] = System.Numerics.Quaternion.CreateFromYawPitchRoll(nr1.Y, nr1.X, nr1.Z);
                    frame.Quaternions[pair[1]] = System.Numerics.Quaternion.CreateFromYawPitchRoll(nr2.Y, nr2.X, nr2.Z);
                }
                else
                {
                    var r1 = TombLib.MathC.RadToDeg(frame.Rotations[pair[0]]);
                    r1 = TombLib.MathC.DegToRad(new System.Numerics.Vector3(r1.X, (360.0f - r1.Y) % 360.0f, (360.0f - r1.Z) % 360.0f));
                    frame.Rotations[pair[0]] = r1;
                    frame.Quaternions[pair[0]] = System.Numerics.Quaternion.CreateFromYawPitchRoll(r1.Y, r1.X, r1.Z);
                }
            }
        }

        // HACK (legacy): bounce undo so bounding box and translation update correctly.
        _editor.Tool.UndoManager.Undo();
        _editor.Tool.UndoManager.Redo();

        if (!flipZ && _editor.Moveable.Id.TypeId != 0 && _editor.Moveable.Meshes.Count > 3)
            ShowPopup("Animation mirroring may fail on certain non-Lara models.\nPlease check animation integrity.", PopupType.Info);

        _timeline.Highlight(start, end);
        SelectFrame();
        UpdateTransformUI();
    }

    // ---- Import / export (slice 3e) ----

    [RelayCommand]
    private void ImportAnimation()
    {
        if (_panel is null || _editor.CurrentAnim is null) return;
        string path = LevelFileDialog.BrowseFile(Owner, "Select a file with animations", BaseGeometryImporter.AnimationFileExtensions, false);
        if (path is null) return;

        WadAnimation animation;
        bool containsMetadata = false;
        try
        {
            if (Path.GetExtension(path) == ".anim") { containsMetadata = true; animation = WadActions.ImportAnimationFromXml(_editor.Tool, path); }
            else if (Path.GetExtension(path) == ".trw") { containsMetadata = true; animation = WadActions.ImportAnimationFromTrw(path, _editor.CurrentAnim.Index); }
            else animation = WadActions.ImportAnimationFromModel(_editor.Tool, Owner, _editor.Moveable.Bones.Count, path);
        }
        catch (Exception ex) { ShowPopup("Error while loading animation.\nException: " + ex.Message, PopupType.Error); return; }

        if (animation is null) return;
        if (animation.Name == new WadAnimation().Name) animation.Name = _editor.CurrentAnim.WadAnimation.Name;
        if (animation.KeyFrames[0].Angles.Count != _editor.Moveable.Bones.Count)
        { ShowPopup("You can only import an animation with the same number of bones as current moveable.", PopupType.Error); return; }

        _editor.CheckAnimationIntegrity(animation);
        _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);

        if (containsMetadata)
            _editor.CurrentAnim.WadAnimation = animation;
        else
        {
            _editor.CurrentAnim.WadAnimation.KeyFrames.Clear();
            _editor.CurrentAnim.WadAnimation.KeyFrames.AddRange(animation.KeyFrames);
            _editor.CurrentAnim.WadAnimation.EndFrame = animation.EndFrame;
        }

        _editor.CurrentAnim.DirectXAnimation = Animation.FromWad2(_editor.Moveable.Bones, animation);
        if (SelectedAnim is not null) SelectedAnim.Label = "(" + _editor.CurrentAnim.Index + ") " + _editor.CurrentAnim.WadAnimation.Name;
        _timeline.Value = 0;
        SelectAnimation(_editor.CurrentAnim);
        _panel.Invalidate();
    }

    [RelayCommand]
    private void ExportAnimation()
    {
        if (_editor.CurrentAnim is null) return;
        string path = LevelFileDialog.BrowseFile(Owner, "Specify file to save animation",
            new System.Collections.Generic.List<FileFormat> { new FileFormat("TombEditor XML", "anim") }, true);
        if (path is null) return;

        var animationToSave = _editor.GetSavedAnimation(_editor.CurrentAnim);
        if (!WadActions.ExportAnimationToXml(_editor.Moveable, animationToSave, path))
            ShowPopup("Can't export current animation to XML file", PopupType.Error);
    }

    [RelayCommand]
    private void Ok() => DialogResult = true;
    [RelayCommand]
    private void Cancel() => DialogResult = false;

    /// <summary>
    /// Asks what to do with unsaved changes (like the previous form) and saves on OK. Returns false to keep the window open.
    /// </summary>
    public bool HandleClosing()
    {
        if (!_isSaved && DialogResult != true)
        {
            var result = UserDialogs.Show(Owner, "You have unsaved changes. Do you want to save changes to animations?",
                "Confirm", DialogButtons.YesNoCancel, DialogIcon.Question);

            switch (result)
            {
                case DialogChoice.Yes:
                    DialogResult = true;
                    break;

                case DialogChoice.No:
                    DialogResult = false;
                    break;

                default:
                    return false;
            }
        }

        if (_playTimer is not null)
            _playTimer.Stop();

        if (DialogResult == true)
            _editor.SaveChanges();

        return true;
    }
}
