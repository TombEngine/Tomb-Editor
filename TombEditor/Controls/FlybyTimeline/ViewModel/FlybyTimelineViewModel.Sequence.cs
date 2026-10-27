#nullable enable

using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using TombEditor.Controls.FlybyTimeline.Sequence;
using TombLib.Forms;
using TombLib.LevelData;

namespace TombEditor.Controls.FlybyTimeline.ViewModel;

// Sequence and camera CRUD operations (add, delete, duplicate, move).
public partial class FlybyTimelineViewModel
{
    #region Sequence management

    /// <summary>
    /// Adds a new empty sequence and selects it.
    /// </summary>
    [RelayCommand]
    private void AddSequence()
    {
        var availableSequences = new HashSet<ushort>(AvailableSequences);

        for (int newIndex = 0; newIndex <= ushort.MaxValue; newIndex++)
        {
            ushort sequence = (ushort)newIndex;

            if (availableSequences.Contains(sequence))
                continue;

            _userAddedSequences.Add(sequence);
            InsertSequenceSorted(sequence);
            SelectedSequence = sequence;
            return;
        }

        _editor.SendMessage("Maximum amount of flyby sequences reached.", PopupType.Error);
    }

    /// <summary>
    /// Removes the selected sequence and any cameras it contains.
    /// </summary>
    [RelayCommand]
    private void RemoveSequence()
    {
        if (!SelectedSequence.HasValue)
            return;

        ushort seq = SelectedSequence.Value;
        var cameras = GetCamerasWithFallback();

        if (cameras.Count > 0)
        {
            bool confirmed = _messageService.ShowConfirmation(
                _localizationService.Format("RemoveSequenceConfirmationMessage", seq, cameras.Count),
                _localizationService["RemoveSequenceConfirmationTitle"],
                defaultValue: false,
                isRisky: true);

            if (!confirmed)
                return;

            var undoList = CreateFlybyCameraDeletionUndo(cameras);

            if (!TryDeleteCameras(cameras, null))
                return;

            PushUndoIfAny(undoList);
        }

        _userAddedSequences.Remove(seq);
        AvailableSequences.Remove(seq);

        SelectedSequence = AvailableSequences.Count > 0 ? AvailableSequences[0] : null;
    }

    /// <summary>
    /// Refreshes camera and timing state when the selected sequence changes.
    /// </summary>
    partial void OnSelectedSequenceChanged(ushort? value)
    {
        _editor.SelectedFlybySequence = value;

        _preview.StopPlayback();
        _preview.InvalidateCache();
        InvalidateVisibleCameraState();
        ResetPlayhead();

        RefreshCameraList();
        RecalculateTimecodes();

        if (_isSyncingSelection)
            return;

        if (_selectedCameras.Count > 0)
        {
            SyncEditorSelection();
            return;
        }

        if (TryRestoreSelectionFromEditor())
            return;

        if (CameraList.Count > 0)
        {
            SetSelectedCameras([CameraList[0].Camera], SelectionUpdateBehavior.All);
            return;
        }

        SetSelectedCameras([], SelectionUpdateBehavior.All);
    }

    #endregion Sequence management

    #region Camera list management

    /// <summary>
    /// Deletes all currently selected cameras from the level.
    /// </summary>
    public void DeleteSelectedCameras()
    {
        if (_selectedCameras.Count == 0 || !SelectedSequence.HasValue)
            return;

        // Make a copy of the selected cameras before deletion.
        var selectedCameras = _selectedCameras.ToList();

        var remainingCameras = GetCamerasAsList()
            .Where(camera => !selectedCameras.Contains(camera))
            .ToList();

        // Capture the surviving cameras before deletion so renumbering and cut-target fixes undo as one action.
        var undoList = CreateFlybyCameraPropertyUndo(remainingCameras);
        undoList.AddRange(CreateFlybyCameraDeletionUndo(selectedCameras));

        if (!TryDeleteCameras(selectedCameras, DialogOwner))
            return;

        RenumberSequence(SelectedSequence.Value);
        PushUndoIfAny(undoList);

        // Removing the last camera can also remove the active sequence; that selection change already triggers its own refresh.
        ushort? previousSequence = SelectedSequence;

        SetSelectedCameras([], SelectionUpdateBehavior.SyncEditorSelection | SelectionUpdateBehavior.RestoreSelectedCameraState);
        RefreshSequenceList();

        // If the same sequence is still active after refresh, update the timeline to reflect removed cameras and new timing.
        if (SelectedSequence == previousSequence)
            RefreshAfterDataChange();
    }

    /// <summary>
    /// Adds a new camera at the playhead, at the sequence start when the playhead is at 0.0, or at the end of the current sequence.
    /// </summary>
    [RelayCommand]
    private void AddCamera()
    {
        if (!SelectedSequence.HasValue || _editor.Level is null)
            return;

        var room = _editor.SelectedRoom;

        if (room is null)
            return;

        _preview.StopPlayback();

        var cam = new FlybyCameraInstance
        {
            Sequence = SelectedSequence.Value,
        };

        AddCameraUsingPlayhead(cam, room);
    }

    /// <summary>
    /// Places a new camera using the current playhead position, or falls back to the sequence bounds when needed.
    /// </summary>
    /// <param name="cam">Camera instance being inserted.</param>
    /// <param name="room">Room that will own the inserted camera.</param>
    private void AddCameraUsingPlayhead(FlybyCameraInstance cam, Room room)
    {
        if (!float.IsFinite(PlayheadSeconds) || PlayheadSeconds < 0.0f || CameraList.Count < 1)
        {
            AddCameraAtSequenceEnd(cam, room);
            return;
        }

        float cursorTime = PlayheadSeconds;
        var cameras = GetCamerasAsList();

        if (cursorTime == 0.0f)
        {
            if (cameras.Count == 1)
                AddCameraAtSequenceEnd(cam, room);
            else
                AddCameraAtSequenceStart(cam, room, cameras);

            return;
        }

        const float minimumSegmentDuration = FlybyConstants.TimeStep;

        float clampedCursorTime = Math.Max(cursorTime, FlybyConstants.TimelineAddCameraMinCursorTime);
        var timing = GetSequenceTiming(cameras);
        float lastCameraTime = timing.GetCameraTime(cameras.Count - 1);

        if (MathF.Abs(cursorTime - lastCameraTime) <= FlybyConstants.TimelineSequenceEndTolerance)
        {
            AddCameraAtSequenceEnd(cam, room);
            return;
        }

        if (cursorTime > lastCameraTime + FlybyConstants.TimelineSequenceEndTolerance)
        {
            AppendCameraAtPlayhead(cam, room, cameras, clampedCursorTime, lastCameraTime, minimumSegmentDuration);
            return;
        }

        int insertIndex = FlybySequenceHelper.FindInsertionIndex(cameras, cursorTime, timing);

        if (insertIndex <= 0 || insertIndex >= cameras.Count)
        {
            AddCameraAtSequenceEnd(cam, room);
            return;
        }

        InsertCameraAtPlayhead(cam, room, cameras, insertIndex, clampedCursorTime, minimumSegmentDuration);
    }

    /// <summary>
    /// Adds a camera to the start of the sequence using the default speed.
    /// </summary>
    private void AddCameraAtSequenceStart(FlybyCameraInstance cam, Room room, IReadOnlyList<FlybyCameraInstance> cameras)
    {
        var undoList = CreateFlybyCameraPropertyUndo(cameras);

        cam.Number = 0;
        cam.Speed = 1.0f;
        ApplyEditorCameraPosition(cam, room);

        if (cameras.Any(camera => camera.Number == cam.Number))
            PrepareCamerasForInsertion(cameras, cam.Number);

        AddCameraToRoom(room, cam, suppressExternalZoomToFit: true);
        undoList.Add(new AddRemoveObjectUndoInstance(_editor.UndoManager, cam, true));

        PushUndoIfAny(undoList);
        FinalizeAddedCamera(cam, zoomToFit: true);
    }

    /// <summary>
    /// Appends a camera after the last camera and retimes the final segment.
    /// </summary>
    private void AppendCameraAtPlayhead(FlybyCameraInstance cam, Room room,
        IReadOnlyList<FlybyCameraInstance> cameras, float clampedCursorTime, float lastCameraTime,
        float minimumSegmentDuration)
    {
        var undoList = CreateFlybyCameraPropertyUndo(cameras);

        cam.Speed = cameras[^1].Speed;
        cam.Number = GetNextCameraNumber(cameras);
        ApplyEditorCameraPosition(cam, room);

        var tempCameras = cameras.ToList();
        tempCameras.Add(cam);

        float targetTime = Math.Max(clampedCursorTime, lastCameraTime + minimumSegmentDuration);
        float newSpeed = FlybySequenceHelper.SolveSegmentSpeedForTargetTime(
            tempCameras,
            tempCameras.Count - 2,
            tempCameras.Count - 1,
            targetTime,
            UseSmoothPause);

        cameras[^1].Speed = newSpeed;
        _editor.ObjectChange(cameras[^1], ObjectChangeType.Change);

        AddCameraToRoom(room, cam, suppressExternalZoomToFit: true);
        undoList.Add(new AddRemoveObjectUndoInstance(_editor.UndoManager, cam, true));

        PushUndoIfAny(undoList);
        FinalizeAddedCamera(cam, zoomToFit: true);
    }

    /// <summary>
    /// Inserts a camera between two existing cameras and updates both segment speeds.
    /// </summary>
    private void InsertCameraAtPlayhead(FlybyCameraInstance cam, Room room,
        IReadOnlyList<FlybyCameraInstance> cameras, int insertIndex, float clampedCursorTime,
        float minimumSegmentDuration)
    {
        int prevIndex = insertIndex - 1;
        var undoList = CreateFlybyCameraPropertyUndo(cameras);
        var timing = GetSequenceTiming(cameras);

        float segmentStart = timing.GetCameraTime(prevIndex);
        float segmentEnd = timing.GetCameraTime(insertIndex);
        float minimumInsertTime = segmentStart + minimumSegmentDuration;
        float maximumInsertTime = segmentEnd - minimumSegmentDuration;

        float insertTime = maximumInsertTime >= minimumInsertTime
            ? Math.Clamp(clampedCursorTime, minimumInsertTime, maximumInsertTime)
            : minimumInsertTime;

        float nextTargetTime = Math.Max(segmentEnd, insertTime + minimumSegmentDuration);
        ushort insertionNumber = GetInsertionNumber(cameras, prevIndex, insertIndex);
        bool requiresNumberShift = cameras.Any(camera => camera.Number == insertionNumber);

        cam.Number = insertionNumber;
        cam.Speed = cameras[prevIndex].Speed;
        ApplyEditorCameraPosition(cam, room);

        // Solve both adjacent segment speeds against a temporary ordered sequence before the new camera exists in the level.
        var tempCameras = cameras.ToList();
        tempCameras.Insert(insertIndex, cam);

        cameras[prevIndex].Speed = FlybySequenceHelper.SolveSegmentSpeedForTargetTime(
            tempCameras,
            prevIndex,
            insertIndex,
            insertTime,
            UseSmoothPause);

        cam.Speed = FlybySequenceHelper.SolveSegmentSpeedForTargetTime(
            tempCameras,
            insertIndex,
            insertIndex + 1,
            nextTargetTime,
            UseSmoothPause);

        if (requiresNumberShift)
            PrepareCamerasForInsertion(cameras, insertionNumber);

        _editor.ObjectChange(cameras[prevIndex], ObjectChangeType.Change);

        AddCameraToRoom(room, cam, suppressExternalZoomToFit: true);
        undoList.Add(new AddRemoveObjectUndoInstance(_editor.UndoManager, cam, true));

        PushUndoIfAny(undoList);
        FinalizeAddedCamera(cam, zoomToFit: false);
    }

    /// <summary>
    /// Adds a camera to the end of the sequence without changing earlier timing.
    /// </summary>
    private void AddCameraAtSequenceEnd(FlybyCameraInstance cam, Room room)
    {
        cam.Number = GetNextCameraNumber(GetCamerasAsList());

        ApplyEditorCameraPosition(cam, room);

        AddCameraToRoom(room, cam, suppressExternalZoomToFit: true);
        _editor.UndoManager.PushObjectCreated(cam);

        FinalizeAddedCamera(cam, zoomToFit: true);
    }

    /// <summary>
    /// Adds a flyby camera to the level and optionally suppresses the generic external add zoom path.
    /// </summary>
    private void AddCameraToRoom(Room room, FlybyCameraInstance camera, bool suppressExternalZoomToFit)
    {
        room.AddObject(_editor.Level, camera);

        bool previousSuppressZoomToFit = _suppressNextAddedCameraZoomToFit;
        _suppressNextAddedCameraZoomToFit = suppressExternalZoomToFit;

        try
        {
            _editor.ObjectChange(camera, ObjectChangeType.Add);
        }
        finally
        {
            _suppressNextAddedCameraZoomToFit = previousSuppressZoomToFit;
        }
    }

    /// <summary>
    /// Copies the current editor camera position and rotation into a flyby camera.
    /// </summary>
    private void ApplyEditorCameraPosition(FlybyCameraInstance cam, Room room)
    {
        var editorCamera = _editor.GetViewportCamera?.Invoke();

        if (editorCamera is not null)
        {
            cam.Position = editorCamera.GetPosition() - room.WorldPos;
            FlybySequenceHelper.ApplyEditorCameraRotation(editorCamera, cam);
        }
        else
        {
            cam.Position = room.GetLocalCenter();
        }
    }

    /// <summary>
    /// Returns the next available camera number after the current highest numbered camera.
    /// </summary>
    private static ushort GetNextCameraNumber(IReadOnlyList<FlybyCameraInstance> cameras)
        => cameras.Count > 0
            ? (ushort)(cameras.Max(camera => (int)camera.Number) + 1)
            : (ushort)0;

    /// <summary>
    /// Returns the number a new camera should receive when inserted at the given list position.
    /// </summary>
    /// <param name="cameras">Ordered cameras in the active sequence.</param>
    /// <param name="prevIndex">Index of the camera immediately before the insertion point.</param>
    /// <param name="insertIndex">Index of the camera currently occupying the insertion point.</param>
    /// <returns>The camera number that should be assigned to the inserted camera.</returns>
    private static ushort GetInsertionNumber(IReadOnlyList<FlybyCameraInstance> cameras, int prevIndex, int insertIndex)
    {
        int previousNumber = cameras[prevIndex].Number;
        int nextNumber = cameras[insertIndex].Number;

        return nextNumber > previousNumber + 1
            ? (ushort)(previousNumber + 1)
            : (ushort)nextNumber;
    }

    /// <summary>
    /// Shifts existing camera numbers and cut targets to make room for an inserted camera number.
    /// </summary>
    private void PrepareCamerasForInsertion(IReadOnlyList<FlybyCameraInstance> cameras, ushort insertionNumber)
    {
        _isApplyingProperty = true;

        try
        {
            foreach (var camera in cameras)
            {
                bool changed = false;

                if (camera.Number >= insertionNumber)
                {
                    camera.Number++;
                    changed = true;
                }

                if ((camera.Flags & FlybyConstants.FlagCameraCut) != 0 && camera.Timer >= insertionNumber)
                {
                    camera.Timer++;
                    changed = true;
                }

                if (changed)
                    _editor.ObjectChange(camera, ObjectChangeType.Change);
            }
        }
        finally
        {
            _isApplyingProperty = false;
        }
    }

    /// <summary>
    /// Refreshes timeline data after inserting a camera, then focuses it in selection and playhead state.
    /// </summary>
    private void FinalizeAddedCamera(FlybyCameraInstance camera, bool zoomToFit)
    {
        // Stage selection and playhead first so the single rebuild paints the new camera in its final state.
        SetSelectedCameras([camera], SelectionUpdateBehavior.SyncEditorSelection);
        MovePlayheadToCamera(camera);
        RefreshAfterDataChange();

        if (zoomToFit)
            RequestZoomToFit();
    }

    /// <summary>
    /// Moves the playhead to the given camera's current timecode.
    /// Prefers the visible timeline state first, then falls back to editor data when needed.
    /// </summary>
    public void MovePlayheadToCamera(FlybyCameraInstance camera)
    {
        if (TryMovePlayheadToVisibleCamera(camera))
            return;

        TryMovePlayheadToCurrentSequence(camera);
    }

    /// <summary>
    /// Moves the playhead to a timeline time without activating preview.
    /// </summary>
    public void MovePlayheadToTime(float timeSeconds)
    {
        if (!SelectedSequence.HasValue || CameraList.Count == 0 || !float.IsFinite(timeSeconds))
            return;

        PlayheadSeconds = Math.Max(0.0f, timeSeconds);
    }

    /// <summary>
    /// Uses the already-visible timeline state when the camera is present in CameraList.
    /// </summary>
    /// <param name="camera">Camera whose visible timecode should become the playhead position.</param>
    /// <returns><see langword="true"/> when the camera is already visible in <see cref="CameraList"/> and the playhead was updated from that state; <see langword="false"/> when the caller must fall back to fresh editor data.</returns>
    private bool TryMovePlayheadToVisibleCamera(FlybyCameraInstance camera)
    {
        for (int i = 0; i < CameraList.Count; i++)
        {
            if (CameraList[i].Camera != camera)
                continue;

            PlayheadSeconds = GetTimecodeForCamera(i);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Falls back to fresh editor data for cameras that exist in the level but are not yet visible in CameraList.
    /// </summary>
    /// <param name="camera">Camera whose current sequence timecode should become the playhead position.</param>
    /// <returns><see langword="true"/> when the camera exists in the active sequence and the playhead was updated; <see langword="false"/> when the camera could not be found in the active sequence.</returns>
    private bool TryMovePlayheadToCurrentSequence(FlybyCameraInstance camera)
    {
        var cameras = GetCamerasWithFallback();
        var timing = GetSequenceTiming(cameras);

        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i] != camera)
                continue;

            PlayheadSeconds = timing.GetCameraTime(i);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Replaces the current selection with the provided camera items.
    /// </summary>
    public void UpdateSelectedCameras(IEnumerable<FlybyCameraItemViewModel>? items)
    {
        if (items is null)
        {
            SetSelectedCameras([]);
            return;
        }

        SetSelectedCameras(items.Select(item => item.Camera));
    }

    /// <summary>
    /// Selects every camera currently visible in the active sequence.
    /// </summary>
    public void SelectAllCameras()
        => SetSelectedCameras(CameraList.Select(item => item.Camera));

    /// <summary>
    /// Returns indices of all currently selected cameras.
    /// </summary>
    public IReadOnlySet<int> GetSelectedIndices()
    {
        var result = new HashSet<int>();

        for (int i = 0; i < CameraList.Count; i++)
        {
            if (_selectedCameras.Contains(CameraList[i].Camera))
                result.Add(i);
        }

        return result;
    }

    /// <summary>
    /// Moves a camera from one list index to another via drag-reorder.
    /// </summary>
    public void MoveCameraToIndex(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= CameraList.Count ||
            toIndex < 0 || toIndex >= CameraList.Count ||
            fromIndex == toIndex)
        {
            return;
        }

        _preview.StopPlayback();

        var movedCamera = CameraList[fromIndex].Camera;
        var cameras = GetCamerasAsList().ToList();
        var oldTargetByNumber = BuildCameraLookupByNumber(cameras);
        var originalTimerByCamera = cameras.ToDictionary(camera => camera, camera => camera.Timer);
        var undoList = CreateFlybyCameraPropertyUndo(cameras);

        cameras.RemoveAt(fromIndex);
        cameras.Insert(toIndex, movedCamera);

        _isApplyingProperty = true;

        try
        {
            ApplySequentialCameraNumbers(cameras);
            UpdateCameraCutTargets(cameras, oldTargetByNumber, camera => originalTimerByCamera[camera]);
        }
        finally
        {
            _isApplyingProperty = false;
        }

        _preview.InvalidateCache();
        InvalidateVisibleCameraState();
        PushUndoIfAny(undoList);
        SetSelectedCameras([movedCamera], SelectionUpdateBehavior.SyncEditorSelection);
        RefreshAfterDataChange();
    }

    /// <summary>
    /// Updates the editable property fields when the selected camera changes.
    /// </summary>
    partial void OnSelectedCameraChanged(FlybyCameraItemViewModel? value)
    {
        _isUpdating = true;

        try
        {
            if (value is not null)
            {
                CameraSpeed = value.Camera.Speed;
                CameraFov = value.Camera.Fov;
                CameraRoll = value.Camera.Roll;
                CameraRotationX = value.Camera.RotationX;
                CameraRotationY = value.Camera.RotationY;
                CameraTimer = value.Camera.Timer;
                CameraFlags = value.Camera.Flags;
            }
        }
        finally
        {
            _isUpdating = false;
        }

        if (value is not null && IsPreviewActive && !IsPlaying)
            _preview.ShowCamera(value.Camera);
    }

    #endregion Camera list management
}
