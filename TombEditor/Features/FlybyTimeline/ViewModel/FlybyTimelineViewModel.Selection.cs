#nullable enable

using System.Collections.Generic;
using System.Linq;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Features.FlybyTimeline.ViewModel;

// Editor event handling and camera selection synchronization.
public partial class FlybyTimelineViewModel
{
    #region Editor event handling

    /// <summary>
    /// Handles editor events that affect flyby data, preview, or selection.
    /// </summary>
    private void OnEditorEventRaised(IEditorEvent @event)
    {
        if (_isDisposed || TryQueueEditorEventOnDispatcher(@event))
            return;

        switch (@event)
        {
            case Editor.LevelChangedEvent or Editor.GameVersionChangedEvent:
                HandleLevelOrGameVersionChangedEvent(@event);
                break;

            case Editor.RoomPositionChangedEvent roomPositionChangedEvent:
                HandleRoomPositionChangedEvent(roomPositionChangedEvent);
                break;

            case Editor.ObjectChangedEvent changeEvent when changeEvent.Object is FlybyCameraInstance flybyCamera:
                HandleFlybyObjectChangedEvent(changeEvent, flybyCamera);
                break;

            case Editor.ToggleCameraPreviewEvent previewEvent:
                HandlePreviewToggleEvent(previewEvent);
                break;

            case Editor.SelectedObjectChangedEvent selectedObjectChangedEvent when !_isSyncingSelection:
                HandleSelectedObjectChangedEvent(selectedObjectChangedEvent);
                break;
        }
    }

    /// <summary>
    /// Queues editor event processing back onto the UI dispatcher when required.
    /// </summary>
    private bool TryQueueEditorEventOnDispatcher(IEditorEvent @event)
    {
        if (_dispatcher.CheckAccess())
            return false;

        if (_isDisposed)
            return true;

        _dispatcher.BeginInvoke(() => OnEditorEventRaised(@event));
        return true;
    }

    /// <summary>
    /// Rebuilds the visible sequence state after level-wide flyby changes.
    /// </summary>
    private void HandleLevelOrGameVersionChangedEvent(IEditorEvent @event)
    {
        // Level-wide changes can keep the same SelectedSequence value, so force a full list rebuild here.
        _preview.StopPlayback();

        if (IsPreviewActive)
            _preview.ExitPreview();

        _preview.InvalidateCache();
        InvalidateVisibleCameraState();
        ResetPlayhead();

        if (@event is Editor.LevelChangedEvent)
            _userAddedSequences.Clear();

        RefreshSequenceList();
        RefreshTimelineState(true, false);
    }

    /// <summary>
    /// Refreshes the visible timeline state after a room move affects flyby cameras.
    /// </summary>
    private void HandleRoomPositionChangedEvent(Editor.RoomPositionChangedEvent roomPositionChangedEvent)
    {
        bool affectsVisibleSequence = CameraList.Any(item => item.Camera.Room == roomPositionChangedEvent.Room);

        if (!affectsVisibleSequence)
            return;

        _preview.InvalidateCache();
        InvalidateVisibleCameraState();
        RefreshAfterDataChange();
    }

    /// <summary>
    /// Updates sequence and preview state after a flyby camera changes in the editor.
    /// </summary>
    private void HandleFlybyObjectChangedEvent(Editor.ObjectChangedEvent changeEvent, FlybyCameraInstance flybyCamera)
    {
        bool affectsVisibleSequence = SelectedSequence.HasValue &&
            (flybyCamera.Sequence == SelectedSequence.Value || CameraList.Any(item => item.Camera == flybyCamera));

        if (!_isApplyingProperty)
            RefreshSequenceListIfNeeded();

        if (!affectsVisibleSequence)
            return;

        _preview.InvalidateCache();

        if (_isApplyingProperty && changeEvent.ChangeType == ObjectChangeType.Change)
            InvalidateSequenceTiming();
        else
            InvalidateVisibleCameraState();

        if (!_isApplyingProperty)
        {
            RefreshAfterDataChange();

            if (changeEvent.ChangeType == ObjectChangeType.Add && !_suppressNextAddedCameraZoomToFit)
                RequestZoomToFit();
        }
    }

    /// <summary>
    /// Handles external camera-preview requests raised through the editor command layer.
    /// Flyby sequences are redirected into the timeline playback path so playhead updates stay in sync.
    /// </summary>
    private void HandlePreviewToggleEvent(Editor.ToggleCameraPreviewEvent previewEvent)
    {
        if (!previewEvent.PreviewState)
        {
            _preview.OnExternalPreviewExit();
            return;
        }

        if (previewEvent.Object is FlybyCameraInstance flyby)
            StartSequencePreviewFromBeginning(flyby);
    }

    /// <summary>
    /// Mirrors editor selection changes into the timeline selection state.
    /// </summary>
    private void HandleSelectedObjectChangedEvent(Editor.SelectedObjectChangedEvent selectedObjectChangedEvent)
        => SyncSelectionFromEditor(selectedObjectChangedEvent.Current);

    #endregion Editor event handling

    #region Selection synchronization

    /// <summary>
    /// Replaces the selected flyby cameras and optionally syncs editor selection.
    /// </summary>
    private void SetSelectedCameras(IEnumerable<FlybyCameraInstance> cameras,
        SelectionUpdateBehavior behavior = SelectionUpdateBehavior.All)
    {
        // The timeline only shows one sequence at a time, so discard any flyby cameras from other sequences up front.
        var normalizedSelection = SelectedSequence.HasValue
            ? cameras.Where(camera => camera.Sequence == SelectedSequence.Value).ToHashSet()
            : [];

        _selectedCameras.Clear();

        foreach (var camera in normalizedSelection)
            _selectedCameras.Add(camera);

        if ((behavior & SelectionUpdateBehavior.RestoreSelectedCameraState) != 0)
            RestoreSelectedCameraState();

        if ((behavior & SelectionUpdateBehavior.SyncEditorSelection) != 0)
            SyncEditorSelection();

        if ((behavior & SelectionUpdateBehavior.RefreshTimeline) != 0)
            TimelineRefreshRequested?.Invoke();
    }

    /// <summary>
    /// Pushes current timeline selection into <see cref="Editor.SelectedObject"/>.
    /// </summary>
    private void SyncEditorSelection()
    {
        if (_isSyncingSelection)
            return;

        _isSyncingSelection = true;

        try
        {
            SetEditorSelection([.. _selectedCameras]);
        }
        finally
        {
            _isSyncingSelection = false;
        }
    }

    /// <summary>
    /// Restores selected camera state after the visible camera list changes.
    /// </summary>
    private void RestoreSelectedCameraState()
    {
        if (!SelectedSequence.HasValue)
        {
            _selectedCameras.Clear();
            SelectedCamera = null;
            return;
        }

        var visibleCameras = CameraList.Select(item => item.Camera).ToHashSet();
        _selectedCameras.RemoveWhere(camera => camera.Sequence != SelectedSequence.Value || !visibleCameras.Contains(camera));

        if (_selectedCameras.Count == 1)
            SelectedCamera = CameraList.FirstOrDefault(item => _selectedCameras.Contains(item.Camera));
        else
            SelectedCamera = null;
    }

    /// <summary>
    /// Mirrors the editor selection into the flyby timeline selection.
    /// </summary>
    private void SyncSelectionFromEditor(ObjectInstance? currentSelection)
    {
        var selectedCameras = GetSelectedFlybyCameras(currentSelection);

        _isSyncingSelection = true;

        try
        {
            AlignSequenceToSelection(selectedCameras);
            SetSelectedCameras(selectedCameras, SelectionUpdateBehavior.RestoreSelectedCameraState | SelectionUpdateBehavior.RefreshTimeline);
            MovePlayheadToSingleSelectedCamera(selectedCameras);
        }
        finally
        {
            _isSyncingSelection = false;
        }
    }

    /// <summary>
    /// Restores timeline selection from the editor's current flyby selection when it belongs to the active sequence.
    /// </summary>
    /// <returns><see langword="true"/> when at least one selected editor flyby camera was restored into the current timeline selection; otherwise <see langword="false"/>.</returns>
    private bool TryRestoreSelectionFromEditor()
    {
        var selectedCameras = GetSelectedFlybyCameras(_editor.SelectedObject);

        if (selectedCameras.Count == 0)
            return false;

        SetSelectedCameras(selectedCameras, SelectionUpdateBehavior.RestoreSelectedCameraState | SelectionUpdateBehavior.RefreshTimeline);
        MovePlayheadToSingleSelectedCamera(selectedCameras);
        return _selectedCameras.Count > 0;
    }

    /// <summary>
    /// Moves the playhead when exactly one externally selected flyby camera is active in the timeline.
    /// </summary>
    private void MovePlayheadToSingleSelectedCamera(IReadOnlyList<FlybyCameraInstance> selectedCameras)
    {
        if (selectedCameras.Count != 1 || _selectedCameras.Count != 1)
            return;

        MovePlayheadToCamera(selectedCameras[0]);
    }

    /// <summary>
    /// Switches the active sequence to match the current camera selection.
    /// </summary>
    private void AlignSequenceToSelection(IReadOnlyList<FlybyCameraInstance> selectedCameras)
    {
        if (selectedCameras.Count == 0)
            return;

        if (SelectedSequence.HasValue && selectedCameras.Any(camera => camera.Sequence == SelectedSequence.Value))
            return;

        ushort? sequence = selectedCameras
            .Select(camera => camera.Sequence)
            .Where(AvailableSequences.Contains)
            .Select(sequenceValue => (ushort?)sequenceValue)
            .OrderBy(sequenceValue => sequenceValue)
            .FirstOrDefault();

        if (sequence.HasValue)
            SelectedSequence = sequence.Value;
    }

    /// <summary>
    /// Extracts flyby cameras from the current editor selection object.
    /// </summary>
    private static IReadOnlyList<FlybyCameraInstance> GetSelectedFlybyCameras(ObjectInstance? currentSelection)
    {
        if (currentSelection is FlybyCameraInstance flybyCamera)
            return [flybyCamera];

        if (currentSelection is ObjectGroup group)
            return [.. group.OfType<FlybyCameraInstance>()];

        return [];
    }

    /// <summary>
    /// Applies a new selection back into the editor.
    /// </summary>
    private void SetEditorSelection(IReadOnlyList<PositionBasedObjectInstance> selectedObjects)
    {
        IReadOnlyList<PositionBasedObjectInstance> currentSelection = _editor.SelectedObject switch
        {
            ObjectGroup group => [.. group.Cast<PositionBasedObjectInstance>()],
            PositionBasedObjectInstance positionBased => [positionBased],
            _ => []
        };

        if (currentSelection.Count == selectedObjects.Count && currentSelection.All(selectedObjects.Contains))
            return;

        _editor.SelectedObject = BuildSelectionObject(selectedObjects);
    }

    /// <summary>
    /// Builds the appropriate editor selection object for the given items.
    /// </summary>
    private static ObjectInstance? BuildSelectionObject(IReadOnlyList<PositionBasedObjectInstance> selectedObjects)
    {
        if (selectedObjects.Count == 0)
            return null;

        if (selectedObjects.Count == 1)
            return selectedObjects[0];

        return new ObjectGroup([.. selectedObjects]);
    }

    #endregion Selection synchronization

    #region Undo helpers

    /// <summary>
    /// Creates undo instances for property changes on the given cameras.
    /// </summary>
    private List<UndoRedoInstance> CreateFlybyCameraPropertyUndo(IEnumerable<FlybyCameraInstance> cameras)
    {
        return [.. cameras
            .Where(camera => camera.Room is not null)
            .Distinct()
            .Select(camera => new ChangeObjectPropertyUndoInstance(_editor.UndoManager, camera))];
    }

    /// <summary>
    /// Creates undo instances for deleting the given cameras.
    /// </summary>
    private List<UndoRedoInstance> CreateFlybyCameraDeletionUndo(IEnumerable<FlybyCameraInstance> cameras)
    {
        return [.. cameras
            .Where(camera => camera.Room is not null)
            .Distinct()
            .Select(camera => new AddRemoveObjectUndoInstance(_editor.UndoManager, camera, false))];
    }

    /// <summary>
    /// Pushes undo instances only when there is captured undo state.
    /// </summary>
    private void PushUndoIfAny(List<UndoRedoInstance> undoInstances)
    {
        if (undoInstances.Count > 0)
            _editor.UndoManager.Push(undoInstances);
    }

    /// <summary>
    /// Creates an undo snapshot when a timeline drag starts affecting a segment.
    /// </summary>
    private void EnsureTimelineDragUndoSnapshot(int cameraIndex)
    {
        int speedCameraIndex = cameraIndex - 1;

        if (_activeDraggedCameraIndex == speedCameraIndex || speedCameraIndex < 0 || speedCameraIndex >= CameraList.Count)
            return;

        PushUndoIfAny(CreateFlybyCameraPropertyUndo([CameraList[speedCameraIndex].Camera]));
        _activeDraggedCameraIndex = speedCameraIndex;
    }

    #endregion Undo helpers
}
