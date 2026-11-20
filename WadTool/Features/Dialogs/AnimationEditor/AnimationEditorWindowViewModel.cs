#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Windows.Threading;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Services;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using TombLib.WPF.CustomControls;
using TombLib.WPF.Dialogs;
using WadTool.Controls;

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
    [ObservableProperty] private string _stateIdToolTip = string.Empty;
    [ObservableProperty] private string? _selectedStateName;

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
    [ObservableProperty] private string _soundConditionToolTip = string.Empty;

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

    public RangeObservableCollection<AnimListItem> Animations { get; } = new();
    public ObservableCollection<MeshBoneItem> Bones { get; } = new();
    public ObservableCollection<object> Rooms { get; } = new();

    /// <summary>Gets the known state names of the edited moveable, used for the State field suggestions and drop-down list.</summary>
    public List<string> StateNames { get; } = new();

    // A separate list for the State drop-down: the suggestion box filters the shared default view of StateNames.
    public List<string> StateNameChoices { get; } = new();

    /// <summary>Raised when the animation name field should receive keyboard focus, e.g. after adding a new animation.</summary>
    public event EventHandler? NameFocusRequested;

    /// <summary>Gets the application configuration (used by the window to persist its placement).</summary>
    public WadTool.Configuration Configuration => _editor.Tool.Configuration;

    public AnimationEditorWindowViewModel(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadMoveableId id)
    {
        _deviceManager = deviceManager;
        _moveableId = id;
        _editor = new AnimationEditor(tool, wad, id);
        _title = "Animation editor - " + _editor.Moveable.Id.ToString(_editor.Wad.GameVersion);

        // Max state value in TR5 was 137 but just in case.
        for (uint i = 0; i < MaxStateId; i++)
        {
            var name = TrCatalog.GetStateName(_editor.Wad.GameVersion, _editor.Moveable.Id.TypeId, i);

            if (!name.Contains("Unknown"))
            {
                StateNames.Add(name);
                StateNameChoices.Add(name);
            }
        }
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

        if (!IsTombEngine && _editor.Tool.Configuration.AnimationEditor_SoundPreviewType > SoundPreviewType.Water)
            _editor.Tool.Configuration.AnimationEditor_SoundPreviewType = SoundPreviewType.Land;

        _allowUpdate = false;
        ChainPlayback = _editor.Tool.Configuration.AnimationEditor_ChainPlayback;
        SoundPreview = _editor.Tool.Configuration.AnimationEditor_SoundPreview && _editor.Tool.ReferenceLevel is not null;
        UpdateSoundCondition();
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

        // The WinForms grid selects its first row on load, which selects the root mesh and draws its gizmo.
        if (Bones.Count > 0)
            SelectedBoneItem = Bones[0];

        // Nothing edited yet, whatever the initial selection raised.
        SetSaved(true);
    }

    public void Detach()
    {
        if (_playTimer is not null) _playTimer.Stop();
        _editor.Tool.EditorEventRaised -= OnEditorEventRaised;
        _editor.Tool.UndoManager.ClearAll();
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
            if (!ReferenceEquals(item, SelectedBoneItem))
            {
                bool prev = _allowUpdate;
                _allowUpdate = false; // avoid re-driving panel selection
                SelectedBoneItem = item;
                _allowUpdate = prev;
            }
        }

        if (obj is WadToolClass.AnimationEditorAnimationChangedEvent animationChanged)
            ReplaceAnimationItem(animationChanged.Animation, animationChanged.Focus);

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

        if (_panel is not null)
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

        var previousAnim = _editor.CurrentAnim;
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
            StateIdText = GetStateDisplayText(node.WadAnimation.StateId);
            StateIdToolTip = "State ID = " + node.WadAnimation.StateId;
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

        OnKeyframesListChanged();

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

        // Continue scrolling the grid if we're in chain playback mode.
        if (!(_editor.Tool.Configuration.AnimationEditor_ChainPlayback && IsPlaying))
            _panel.GridPosition = Vector3.Zero;

        _panel.Invalidate();

        UpdateStatusLabel();

        if (_editor.CurrentAnim is not null)
            _editor.Tool.AnimationEditorCurrentAnimationChanged(previousAnim, _editor.CurrentAnim);
    }

    /// <summary>Returns the state name shown in the State field, or the raw number if the state has no known name.</summary>
    private string GetStateDisplayText(int stateId)
    {
        var name = TrCatalog.GetStateName(_editor.Wad.GameVersion, _editor.Moveable.Id.TypeId, (uint)stateId);
        return StateNames.Contains(name) ? name : stateId.ToString();
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
            OnKeyframesListChanged();
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
        // WinForms marks the animation dirty on every parameter edit, also when the undo entry is coalesced.
        SetSaved(false);

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

    [RelayCommand]
    private void Ok()
    {
        DialogResult = null;
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = null;
        DialogResult = false;
    }

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
                    // Reset, otherwise the next Cancel sets the same value again and the window never closes.
                    DialogResult = null;
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
