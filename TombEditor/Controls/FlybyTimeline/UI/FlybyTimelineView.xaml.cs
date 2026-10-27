#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using System.Windows.Threading;
using TombEditor.Controls.FlybyTimeline.ViewModel;

namespace TombEditor.Controls.FlybyTimeline.UI;

/// <summary>
/// WPF UserControl that embeds the flyby timeline and its controls.
/// Hosted inside MainView via ElementHost.
/// </summary>
public partial class FlybyTimelineView : UserControl
{
    private FlybyTimelineViewModel? _viewModel;
    private bool _zoomToFitQueued;
    private bool _initialZoomToFitApplied;

    /// <summary>
    /// Creates the timeline host control.
    /// </summary>
    public FlybyTimelineView()
        => InitializeComponent();

    /// <summary>
    /// Initializes the view model and wires up all event handlers.
    /// Called once when the hosting MainView is ready.
    /// </summary>
    /// <param name="parentForm">Optional WinForms owner used for flyby modal dialogs.</param>
    public void Initialize(System.Windows.Forms.IWin32Window? parentForm = null)
    {
        if (_viewModel is not null)
            return;

        var viewModel = new FlybyTimelineViewModel(Editor.Instance, Dispatcher, parentForm);
        _viewModel = viewModel;
        DataContext = viewModel;

        SubscribeViewModel(viewModel);
        SubscribeTimelineControl();

        RefreshTimeline();
        QueueZoomToFit();
    }

    /// <summary>
    /// Cleans up all event subscriptions.
    /// </summary>
    public void Cleanup()
    {
        if (_viewModel is null)
            return;

        UnsubscribeViewModel(_viewModel);
        UnsubscribeTimelineControl();

        _viewModel.Cleanup();
        DataContext = null;
        _viewModel = null;
        _initialZoomToFitApplied = false;
    }

    /// <summary>
    /// Subscribes to view-model events used by the code-behind.
    /// </summary>
    /// <param name="viewModel">View model instance backing this control.</param>
    private void SubscribeViewModel(FlybyTimelineViewModel viewModel)
    {
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        viewModel.TimelineRefreshRequested += RefreshTimeline;
        viewModel.ZoomToFitRequested += QueueZoomToFit;
    }

    /// <summary>
    /// Unsubscribes from view-model events used by the code-behind.
    /// </summary>
    /// <param name="viewModel">View model instance backing this control.</param>
    private void UnsubscribeViewModel(FlybyTimelineViewModel viewModel)
    {
        viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        viewModel.TimelineRefreshRequested -= RefreshTimeline;
        viewModel.ZoomToFitRequested -= QueueZoomToFit;
    }

    /// <summary>
    /// Subscribes to timeline-control events raised by user interaction.
    /// </summary>
    private void SubscribeTimelineControl()
    {
        timelineControl.MarkerClicked += OnTimelineMarkerClicked;
        timelineControl.MarkerDoubleClicked += OnTimelineMarkerDoubleClicked;
        timelineControl.MarkerDragged += OnTimelineMarkerDragged;
        timelineControl.MarkerDragCompleted += OnTimelineMarkerDragCompleted;
        timelineControl.RangeSelected += OnTimelineRangeSelected;
        timelineControl.PlayheadRequested += OnTimelinePlayheadRequested;
        timelineControl.ScrubRequested += OnTimelineScrubRequested;
        timelineControl.PlayStopRequested += OnTimelinePlayStopRequested;
        timelineControl.DeleteRequested += OnTimelineDeleteRequested;
        timelineControl.SelectAllRequested += OnTimelineSelectAllRequested;
        timelineControl.MarkerReordered += OnTimelineMarkerReordered;
    }

    /// <summary>
    /// Unsubscribes from timeline-control events raised by user interaction.
    /// </summary>
    private void UnsubscribeTimelineControl()
    {
        timelineControl.MarkerClicked -= OnTimelineMarkerClicked;
        timelineControl.MarkerDoubleClicked -= OnTimelineMarkerDoubleClicked;
        timelineControl.MarkerDragged -= OnTimelineMarkerDragged;
        timelineControl.MarkerDragCompleted -= OnTimelineMarkerDragCompleted;
        timelineControl.RangeSelected -= OnTimelineRangeSelected;
        timelineControl.PlayheadRequested -= OnTimelinePlayheadRequested;
        timelineControl.ScrubRequested -= OnTimelineScrubRequested;
        timelineControl.PlayStopRequested -= OnTimelinePlayStopRequested;
        timelineControl.DeleteRequested -= OnTimelineDeleteRequested;
        timelineControl.SelectAllRequested -= OnTimelineSelectAllRequested;
        timelineControl.MarkerReordered -= OnTimelineMarkerReordered;
    }

    /// <summary>
    /// Reacts to view-model property changes that require UI updates.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not FlybyTimelineViewModel viewModel)
            return;

        switch (e.PropertyName)
        {
            case nameof(FlybyTimelineViewModel.SelectedSequence):
                RefreshTimeline();
                QueueZoomToFit();
                break;

            case nameof(FlybyTimelineViewModel.PlayheadSeconds):
                timelineControl.SetPlayheadSeconds(viewModel.PlayheadSeconds);
                break;
        }
    }

    #region Timeline event handlers

    /// <summary>
    /// Rebuilds timeline marker data from the current view-model state.
    /// </summary>
    private void RefreshTimeline()
    {
        if (_viewModel is null)
            return;

        var renderState = _viewModel.BuildTimelineRenderState();
        timelineControl.SetMarkers(renderState.Markers, renderState.TotalDuration, renderState.Cache);
        timelineControl.SetPlayheadSeconds(_viewModel.PlayheadSeconds);
    }

    /// <summary>
    /// Defers zoom-to-fit until after pending timeline refreshes have updated the control state.
    /// </summary>
    private void QueueZoomToFit()
    {
        if (_zoomToFitQueued)
            return;

        _zoomToFitQueued = true;

        Dispatcher.BeginInvoke(DispatcherPriority.Background, new System.Action(() =>
        {
            _zoomToFitQueued = false;
            timelineControl.ZoomToFit(_initialZoomToFitApplied);
            _initialZoomToFitApplied = true;
        }));
    }

    /// <summary>
    /// Selects the clicked marker and syncs the selected room.
    /// </summary>
    private void OnTimelineMarkerClicked(int index)
    {
        if (!TrySelectSingleCamera(index, out var item))
            return;

        _viewModel?.MovePlayheadToCamera(item.Camera);
        _viewModel?.UpdateSelectedRoomByPosition(item.Camera.WorldPosition);
    }

    /// <summary>
    /// Opens the clicked flyby camera for editing.
    /// </summary>
    private void OnTimelineMarkerDoubleClicked(int index)
    {
        if (!TrySelectSingleCamera(index, out var item))
            return;

        EditorActions.EditObject(item.Camera, _viewModel?.DialogOwner);
    }

    /// <summary>
    /// Applies a live drag update to a timeline marker.
    /// </summary>
    private void OnTimelineMarkerDragged(int index, float newTimeSeconds)
        => _viewModel?.OnTimelineCameraDragged(index, newTimeSeconds);

    /// <summary>
    /// Ends the current marker drag operation.
    /// </summary>
    private void OnTimelineMarkerDragCompleted(int _)
        => _viewModel?.OnTimelineCameraDragCompleted();

    /// <summary>
    /// Updates camera selection from a timeline range selection.
    /// </summary>
    /// <param name="selectedIndices">Marker indices reported by the timeline marquee selection.</param>
    private void OnTimelineRangeSelected(IReadOnlyList<int> selectedIndices)
    {
        if (_viewModel is null)
            return;

        // Empty range selection means deselection.
        if (selectedIndices.Count == 0)
        {
            _viewModel.UpdateSelectedCameras([]);
            return;
        }

        var selectedItems = new List<FlybyCameraItemViewModel>(selectedIndices.Count);

        foreach (int index in selectedIndices)
        {
            if (index < 0 || index >= _viewModel.CameraList.Count)
                continue;

            selectedItems.Add(_viewModel.CameraList[index]);
        }

        _viewModel.UpdateSelectedCameras(selectedItems);
    }

    /// <summary>
    /// Moves the timeline playhead without activating preview.
    /// </summary>
    private void OnTimelinePlayheadRequested(float timeSeconds)
        => _viewModel?.MovePlayheadToTime(timeSeconds);

    /// <summary>
    /// Scrubs preview playback to the requested timeline time.
    /// </summary>
    private void OnTimelineScrubRequested(float timeSeconds)
        => _viewModel?.ScrubToTime(timeSeconds);

    /// <summary>
    /// Toggles timeline playback.
    /// </summary>
    private void OnTimelinePlayStopRequested()
        => _viewModel?.TogglePlayStopCommand.Execute(null);

    /// <summary>
    /// Deletes the currently selected timeline cameras.
    /// </summary>
    private void OnTimelineDeleteRequested()
        => _viewModel?.DeleteSelectedCameras();

    /// <summary>
    /// Selects all cameras visible in the current timeline sequence.
    /// </summary>
    private void OnTimelineSelectAllRequested()
        => _viewModel?.SelectAllCameras();

    /// <summary>
    /// Reorders a camera after an Alt-drag reposition operation.
    /// </summary>
    private void OnTimelineMarkerReordered(int fromIndex, int toIndex)
        => _viewModel?.MoveCameraToIndex(fromIndex, toIndex);

    /// <summary>
    /// Tries to resolve a marker index and make it the active single-camera selection.
    /// </summary>
    /// <param name="index">Timeline marker index to resolve.</param>
    /// <param name="item">Receives the selected camera item when the marker index is valid.</param>
    /// <returns><see langword="true"/> when the marker index resolves to a camera item and that item becomes the active selection; <see langword="false"/> when no camera exists at the requested index.</returns>
    private bool TrySelectSingleCamera(int index, [NotNullWhen(true)] out FlybyCameraItemViewModel? item)
    {
        if (!TryGetCameraItem(index, out item))
            return false;

        _viewModel?.UpdateSelectedCameras([item]);
        return true;
    }

    /// <summary>
    /// Returns the camera item for a valid timeline marker index.
    /// </summary>
    /// <param name="index">Timeline marker index to resolve.</param>
    /// <param name="item">Receives the matching camera item when the index is valid.</param>
    /// <returns><see langword="true"/> when the index maps to an existing camera item; <see langword="false"/> when the view model is unavailable or the index is out of range.</returns>
    private bool TryGetCameraItem(int index, [NotNullWhen(true)] out FlybyCameraItemViewModel? item)
    {
        item = null;

        if (_viewModel is null || index < 0 || index >= _viewModel.CameraList.Count)
            return false;

        item = _viewModel.CameraList[index];
        return true;
    }

    #endregion Timeline event handlers
}
