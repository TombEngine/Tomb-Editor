#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombEditor.Controls.FlybyTimeline.Sequence;
using TombLib.LevelData;

namespace TombEditor.Controls.FlybyTimeline.ViewModel;

// Data refresh, camera list rebuilding, and renumbering.
public partial class FlybyTimelineViewModel
{
    /// <summary>
    /// Refreshes the full timeline state after underlying data changes.
    /// </summary>
    private void RefreshAfterDataChange() => RefreshTimelineState(true);

    /// <summary>
    /// Requests the view to zoom the timeline to fit the current sequence.
    /// </summary>
    private void RequestZoomToFit() => ZoomToFitRequested?.Invoke();

    /// <summary>
    /// Deletes the provided cameras and restores the visible timeline state if any camera remains.
    /// </summary>
    /// <param name="cameras">The cameras that should be deleted.</param>
    /// <param name="dialogOwner">Optional owner for confirmation or error dialogs raised by the delete action.</param>
    /// <returns><see langword="true"/> when all requested cameras were deleted successfully; <see langword="false"/> when any requested camera remains in the level after the delete action.</returns>
    private bool TryDeleteCameras(IReadOnlyCollection<FlybyCameraInstance> cameras, IWin32Window? dialogOwner)
    {
        _preview.StopPlayback();

        _isApplyingProperty = true;

        try
        {
            EditorActions.DeleteObjects(cameras.Cast<ObjectInstance>(), dialogOwner, false);
        }
        finally
        {
            _isApplyingProperty = false;
        }

        if (cameras.Any(camera => camera.Room is not null)) // Are there any remaining cameras after deletion?
        {
            RefreshTimelineState(true, false);
            return false;
        }

        _preview.InvalidateCache();
        return true;
    }

    /// <summary>
    /// Rebuilds the available sequence list and preserves selection when possible.
    /// </summary>
    private void RefreshSequenceList()
    {
        var currentSelection = SelectedSequence;
        var sequences = new HashSet<ushort>(_userAddedSequences);

        if (_editor.Level is not null)
        {
            foreach (ushort seq in FlybySequenceHelper.GetAllSequences(_editor.Level))
                sequences.Add(seq);
        }

        AvailableSequences.Clear();

        foreach (ushort seq in sequences.OrderBy(s => s))
            AvailableSequences.Add(seq);

        if (currentSelection.HasValue && AvailableSequences.Contains(currentSelection.Value))
            SelectedSequence = currentSelection.Value;
        else if (AvailableSequences.Count > 0)
            SelectedSequence = AvailableSequences[0];
        else
            SelectedSequence = null;
    }

    /// <summary>
    /// Rebuilds the available sequence list only when level data changed the visible set of sequence ids.
    /// </summary>
    private void RefreshSequenceListIfNeeded()
    {
        var sequences = new HashSet<ushort>(_userAddedSequences);

        if (_editor.Level is not null)
        {
            foreach (ushort seq in FlybySequenceHelper.GetAllSequences(_editor.Level))
                sequences.Add(seq);
        }

        if (AvailableSequences.Count == sequences.Count && AvailableSequences.All(sequences.Contains))
            return;

        RefreshSequenceList();
    }

    /// <summary>
    /// Rebuilds the camera list for the currently selected sequence.
    /// </summary>
    private void RefreshCameraList()
    {
        CameraList.Clear();
        InvalidateVisibleCameraState();

        if (!SelectedSequence.HasValue || _editor.Level is null)
        {
            RestoreSelectedCameraState();
            return;
        }

        var cameras = FlybySequenceHelper.GetCameras(_editor.Level, SelectedSequence.Value);
        _cachedVisibleCameras = [.. cameras];

        var duplicateNumbers = cameras
            .GroupBy(c => c.Number)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

        foreach (var cam in cameras)
        {
            CameraList.Add(new FlybyCameraItemViewModel(cam)
            {
                IsDuplicateIndex = duplicateNumbers.Contains(cam.Number)
            });
        }

        RestoreSelectedCameraState();
    }

    /// <summary>
    /// Inserts a sequence id into the available sequence list in sorted order.
    /// </summary>
    private void InsertSequenceSorted(ushort sequence)
    {
        int insertIndex = 0;

        for (int i = 0; i < AvailableSequences.Count; i++)
        {
            if (AvailableSequences[i] > sequence)
                break;

            insertIndex = i + 1;
        }

        AvailableSequences.Insert(insertIndex, sequence);
    }

    /// <summary>
    /// Renumbers cameras in a sequence and updates cut targets to match.
    /// </summary>
    private void RenumberSequence(ushort sequence, FlybyCameraInstance? excludeFromEvent = null)
    {
        if (_editor.Level is null)
            return;

        var cameras = FlybySequenceHelper.GetCameras(_editor.Level, sequence);
        var oldTargetByNumber = BuildCameraLookupByNumber(cameras);

        _isApplyingProperty = true;

        try
        {
            ApplySequentialCameraNumbers(cameras, excludeFromEvent);
            UpdateCameraCutTargets(cameras, oldTargetByNumber, camera => camera.Timer, excludeFromEvent);
        }
        finally
        {
            _isApplyingProperty = false;
        }
    }

    /// <summary>
    /// Builds a lookup of cameras keyed by their current number.
    /// </summary>
    private static Dictionary<int, FlybyCameraInstance> BuildCameraLookupByNumber(IReadOnlyList<FlybyCameraInstance> cameras)
        => cameras.GroupBy(camera => (int)camera.Number).ToDictionary(group => group.Key, group => group.First());

    /// <summary>
    /// Renumbers cameras to match their current order and raises editor change events for updates.
    /// </summary>
    private void ApplySequentialCameraNumbers(IReadOnlyList<FlybyCameraInstance> cameras, FlybyCameraInstance? excludeFromEvent = null)
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i].Number != (ushort)i)
            {
                cameras[i].Number = (ushort)i;

                if (cameras[i] != excludeFromEvent)
                    _editor.ObjectChange(cameras[i], ObjectChangeType.Change);
            }
        }
    }

    /// <summary>
    /// Updates camera-cut timers after camera numbering changes.
    /// </summary>
    private void UpdateCameraCutTargets(IReadOnlyList<FlybyCameraInstance> cameras,
        IReadOnlyDictionary<int, FlybyCameraInstance> oldTargetByNumber,
        Func<FlybyCameraInstance, short> getOriginalTimer,
        FlybyCameraInstance? excludeFromEvent = null)
    {
        foreach (var camera in cameras)
        {
            if ((camera.Flags & FlybyConstants.FlagCameraCut) == 0)
                continue;

            ushort originalFlags = camera.Flags;
            short originalTimer = getOriginalTimer(camera);

            if (oldTargetByNumber.TryGetValue(originalTimer, out var targetCamera) && cameras.Contains(targetCamera))
            {
                camera.Timer = (short)targetCamera.Number;
            }
            else
            {
                camera.Flags = (ushort)(camera.Flags & ~FlybyConstants.FlagCameraCut);
                camera.Timer = 0;
            }

            if ((camera.Flags != originalFlags || camera.Timer != originalTimer) && camera != excludeFromEvent)
                _editor.ObjectChange(camera, ObjectChangeType.Change);
        }
    }

    /// <summary>
    /// Refreshes camera data and optionally syncs the preview output.
    /// </summary>
    private void RefreshTimelineState(bool refreshCameraList, bool syncPreview = true, bool refreshTimeline = true)
    {
        if (_isDisposed)
            return;

        if (refreshCameraList)
            RefreshCameraList();

        if (refreshTimeline)
        {
            RecalculateTimecodes();
            TimelineRefreshRequested?.Invoke();
        }

        if (syncPreview)
            RefreshPreviewState();
    }

    /// <summary>
    /// Updates the preview camera or scrub state from the current selection.
    /// </summary>
    private void RefreshPreviewState()
    {
        if (_isDisposed)
            return;

        if (!TryGetSequenceContext(out var cameras, out var sequence))
        {
            if (SelectedCamera is not null)
                _preview.ShowCamera(SelectedCamera.Camera);

            return;
        }

        if (IsPreviewActive && PlayheadSeconds >= 0)
            _preview.ScrubToTime(cameras, sequence, PlayheadSeconds);
        else if (SelectedCamera is not null)
            _preview.ShowCamera(SelectedCamera.Camera);
    }

    /// <summary>
    /// Returns the active sequence context needed for preview operations.
    /// </summary>
    /// <param name="cameras">Receives the currently visible flyby cameras when sequence context is available.</param>
    /// <param name="sequence">Receives the currently selected sequence id when sequence context is available.</param>
    /// <returns><see langword="true"/> when a sequence is selected and the current camera list is available for preview operations; <see langword="false"/> when preview operations should be skipped.</returns>
    private bool TryGetSequenceContext(out IReadOnlyList<FlybyCameraInstance> cameras, out ushort sequence)
    {
        if (SelectedSequence.HasValue && CameraList.Count > 0)
        {
            sequence = SelectedSequence.Value;
            cameras = GetCamerasAsList();
            return true;
        }

        cameras = [];
        sequence = 0;
        return false;
    }
}
