#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Threading;
using TombEditor.Controls.FlybyTimeline.Preview;
using TombEditor.Controls.FlybyTimeline.Sequence;
using TombLib.LevelData;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Controls.FlybyTimeline.ViewModel;

/// <summary>
/// Main view model for the Flyby Sequence Manager window.
/// Delegates data operations to FlybySequenceHelper and preview to FlybyPreviewController.
/// </summary>
public partial class FlybyTimelineViewModel : ObservableObject
{
    [Flags]
    private enum SelectionUpdateBehavior
    {
        None = 0,
        SyncEditorSelection = 1 << 0,
        RestoreSelectedCameraState = 1 << 1,
        RefreshTimeline = 1 << 2,
        All = SyncEditorSelection | RestoreSelectedCameraState | RefreshTimeline
    }

    private readonly Editor _editor;
    private readonly FlybyPreviewController _preview;
    private readonly Dispatcher _dispatcher;
    private readonly IWin32Window? _dialogOwner;
    private readonly IMessageService _messageService;
    private readonly ILocalizationService _localizationService;

    private bool _isUpdating;
    private bool _isApplyingProperty;
    private bool _isSyncingSelection;
    private bool _suppressNextAddedCameraZoomToFit;
    private bool _isDisposed;
    private bool _isTimelineRefreshQueued;
    private bool _queuedTimelineRefreshCameraList;
    private bool _queuedTimelineRefreshTimeline;
    private bool _queuedTimelineRefreshPreview;
    private int _activeDraggedCameraIndex = -1;
    private FlybyCameraInstance[]? _cachedVisibleCameras;
    private FlybySequenceTiming? _cachedSequenceTiming;
    private FlybyCameraInstance[]? _cachedSequenceTimingCameras;
    private DispatcherOperation? _queuedTimelineRefreshOperation;

    /// <summary>
    /// Gets the available flyby sequence ids shown in the UI.
    /// </summary>
    public ObservableCollection<ushort> AvailableSequences { get; } = [];

    /// <summary>
    /// Gets the camera items for the currently selected sequence.
    /// </summary>
    public ObservableCollection<FlybyCameraItemViewModel> CameraList { get; } = [];

    /// <summary>
    /// Stores the currently selected flyby sequence id.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSequenceSelected))]
    private ushort? _selectedSequence;

    /// <summary>
    /// Stores the currently selected camera item.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditProperties))]
    private FlybyCameraItemViewModel? _selectedCamera;

    /// <summary>
    /// Stores whether sequence playback is currently active.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PlayStopIcon))]
    [NotifyPropertyChangedFor(nameof(PlayStopTooltip))]
    private bool _isPlaying;

    /// <summary>
    /// Gets whether the editor is currently showing a flyby preview.
    /// </summary>
    public bool IsPreviewActive => _editor.CameraPreviewMode != CameraPreviewType.None;

    /// <summary>
    /// Gets whether TombEngine smooth pause behavior should be used.
    /// </summary>
    private bool UseSmoothPause => FlybyConstants.UseSmoothPause(_editor.Level?.Settings.GameVersion);

    // Camera properties for the selected camera.
    /// <summary>
    /// Stores the editable speed value of the selected camera.
    /// </summary>
    [ObservableProperty]
    private float _cameraSpeed;

    /// <summary>
    /// Stores the editable field-of-view value of the selected camera.
    /// </summary>
    [ObservableProperty]
    private float _cameraFov;

    /// <summary>
    /// Stores the editable roll value of the selected camera.
    /// </summary>
    [ObservableProperty]
    private float _cameraRoll;

    /// <summary>
    /// Stores the editable X rotation value of the selected camera.
    /// </summary>
    [ObservableProperty]
    private float _cameraRotationX;

    /// <summary>
    /// Stores the editable Y rotation value of the selected camera.
    /// </summary>
    [ObservableProperty]
    private float _cameraRotationY;

    /// <summary>
    /// Stores the editable timer value of the selected camera.
    /// </summary>
    [ObservableProperty]
    private short _cameraTimer;

    /// <summary>
    /// Stores the editable flags value of the selected camera.
    /// </summary>
    [ObservableProperty]
    private ushort _cameraFlags;

    /// <summary>
    /// Stores the formatted real playback time shown at the playhead in the UI.
    /// </summary>
    [ObservableProperty]
    private string _playheadTimecode = "00:00.00";

    /// <summary>
    /// Stores the current playhead position in timeline seconds. Negative values indicate the playhead is hidden.
    /// </summary>
    [ObservableProperty]
    private float _playheadSeconds = -1.0f;

    /// <summary>
    /// Gets the icon resource used for the play or stop button.
    /// </summary>
    public string PlayStopIcon => IsPlaying
        ? "pack://application:,,,/TombEditor;component/Resources/icons_transport/transport-stop-24.png"
        : "pack://application:,,,/TombEditor;component/Resources/icons_transport/transport-play-24.png";

    /// <summary>
    /// Gets the tooltip text used for the play or stop button.
    /// </summary>
    public string PlayStopTooltip => IsPlaying
        ? _localizationService["StopSequenceTooltip"]
        : _localizationService["PlaySequenceTooltip"];

    /// <summary>
    /// Gets whether a flyby sequence is currently selected.
    /// </summary>
    public bool HasSequenceSelected => SelectedSequence.HasValue;

    /// <summary>
    /// Gets whether the selected camera properties can currently be edited.
    /// </summary>
    public bool CanEditProperties => SelectedCamera is not null && !IsPlaying;

    // Selected cameras are tracked by instance so refreshes do not invalidate selection state.
    private readonly HashSet<FlybyCameraInstance> _selectedCameras = [];

    /// <summary>
    /// Gets the currently selected flyby cameras.
    /// </summary>
    public IReadOnlyCollection<FlybyCameraInstance> SelectedCameras => _selectedCameras;

    // Temporary sequences added by user (persist until window closes).
    private readonly HashSet<ushort> _userAddedSequences = [];

    /// <summary>
    /// Fired when the timeline needs a visual refresh.
    /// </summary>
    public event Action? TimelineRefreshRequested;

    /// <summary>
    /// Fired when the timeline should zoom to fit the current sequence.
    /// </summary>
    public event Action? ZoomToFitRequested;

    /// <summary>
    /// Creates the main view model for the flyby timeline UI.
    /// </summary>
    /// <param name="editor">Editor instance providing level, selection, and undo services.</param>
    /// <param name="dispatcher">UI dispatcher used to marshal editor events onto the UI thread.</param>
    /// <param name="dialogOwner">Optional WinForms owner used for modal dialogs.</param>
    /// <param name="messageService">Optional message service used for confirmations.</param>
    /// <param name="localizationService">Optional localization service used for UI strings.</param>
    public FlybyTimelineViewModel(
        Editor editor,
        Dispatcher dispatcher,
        IWin32Window? dialogOwner = null,
        IMessageService? messageService = null,
        ILocalizationService? localizationService = null)
    {
        _editor = editor;
        _dispatcher = dispatcher;
        _dialogOwner = dialogOwner;
        _messageService = ServiceLocator.ResolveService(messageService);
        _localizationService = ServiceLocator.ResolveService(localizationService)
            .WithKeysFor(this);

        _preview = new FlybyPreviewController(editor);
        _preview.StateChanged += OnPreviewStateChanged;
        _preview.PlayheadChanged += OnPreviewPlayheadChanged;

        _editor.EditorEventRaised += OnEditorEventRaised;

        RefreshSequenceList();
    }

    /// <summary>
    /// Unhooks preview and editor event subscriptions.
    /// </summary>
    public void Cleanup()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        AbortQueuedTimelineRefresh();

        _preview.StateChanged -= OnPreviewStateChanged;
        _preview.PlayheadChanged -= OnPreviewPlayheadChanged;
        _preview.Dispose();

        _editor.EditorEventRaised -= OnEditorEventRaised;
    }

    /// <summary>
    /// Gets the best available owner window for flyby modal dialogs.
    /// </summary>
    public IWin32Window? DialogOwner => Form.ActiveForm ?? _dialogOwner;
}
