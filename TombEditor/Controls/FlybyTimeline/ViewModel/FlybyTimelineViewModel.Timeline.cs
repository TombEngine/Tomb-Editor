#nullable enable

using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Numerics;
using TombEditor.Controls.FlybyTimeline.Sequence;
using TombEditor.Controls.FlybyTimeline.UI;
using TombLib;
using TombLib.Forms;
using TombLib.LevelData;

namespace TombEditor.Controls.FlybyTimeline.ViewModel;

// Preview playback, timeline scrubbing, and timecode calculation.
public partial class FlybyTimelineViewModel
{
    #region Preview and playback

    /// <summary>
    /// Starts or stops sequence playback.
    /// </summary>
    [RelayCommand]
    private void TogglePlayStop()
    {
        if (IsPlaying)
            _preview.StopPlayback();
        else if (SelectedSequence.HasValue)
            _preview.StartPlayback(GetCamerasAsList(), SelectedSequence.Value, PlayheadSeconds);
    }

    /// <summary>
    /// Starts preview playback for the flyby's sequence from the beginning using the timeline controller.
    /// </summary>
    private void StartSequencePreviewFromBeginning(FlybyCameraInstance flyby)
    {
        if (_editor.Level is null)
            return;

        if (SelectedSequence != flyby.Sequence)
            SelectedSequence = flyby.Sequence;

        var cameras = GetCamerasAsList();

        if (cameras.Count < 2)
        {
            _editor.SendMessage("Flyby sequence needs at least 2 cameras to play.", PopupType.Info);
            return;
        }

        SetSelectedCameras([flyby], SelectionUpdateBehavior.RestoreSelectedCameraState | SelectionUpdateBehavior.RefreshTimeline);
        ScrubToTime(0.0f);

        _preview.StartPlayback(cameras, flyby.Sequence, 0.0f);
    }

    /// <summary>
    /// Scrubs the timeline to a specific time in seconds.
    /// </summary>
    public void ScrubToTime(float timeSeconds)
    {
        if (!SelectedSequence.HasValue || CameraList.Count == 0 || !float.IsFinite(timeSeconds))
            return;

        var cameras = GetCamerasAsList();
        var frame = _preview.ScrubToTime(cameras, SelectedSequence.Value, timeSeconds);

        if (frame.HasValue)
            UpdateSelectedRoomByPosition(frame.Value.Position);
    }

    /// <summary>
    /// Updates the selected room when the preview position crosses into a different room.
    /// </summary>
    public void UpdateSelectedRoomByPosition(Vector3 worldPosition)
    {
        if (_editor.Level is null || !worldPosition.IsFinite())
            return;

        var room = _editor.GetRoomAtPosition(worldPosition);

        if (room is null || room == _editor.SelectedRoom)
            return;

        _editor.SelectedRoom = room;
        _editor.ResetCamera(false, room);
    }

    /// <summary>
    /// Called when a camera is dragged on the timeline to a new timecode position.
    /// </summary>
    public void OnTimelineCameraDragged(int cameraIndex, float newTimeSeconds)
    {
        if (cameraIndex <= 0 || cameraIndex >= CameraList.Count || !float.IsFinite(newTimeSeconds))
            return;

        _preview.StopPlayback();

        EnsureTimelineDragUndoSnapshot(cameraIndex);

        var cameras = GetCamerasAsList();
        var timing = GetSequenceTiming(cameras);
        var previousCamera = CameraList[cameraIndex - 1].Camera;
        float prevTime = timing.GetCameraTime(cameraIndex - 1);
        float freezeAtPrev = timing.GetFreezeDuration(cameraIndex - 1);
        float minTargetTime = prevTime + freezeAtPrev + FlybyConstants.TimeStep;
        float targetTime = Math.Max(newTimeSeconds, minTargetTime);

        float newSpeed = FlybySequenceHelper.SnapSpeedToStep(
            FlybySequenceHelper.SolveSegmentSpeedForTargetTime(
                cameras,
                cameraIndex - 1,
                cameraIndex,
                targetTime,
                UseSmoothPause),
            FlybyConstants.TimelineDragSpeedStep);

        _isApplyingProperty = true;

        try
        {
            previousCamera.Speed = newSpeed;
            _editor.ObjectChange(previousCamera, ObjectChangeType.Change);
        }
        finally
        {
            _isApplyingProperty = false;
        }

        _preview.InvalidateCache();
        InvalidateSequenceTiming();
        RefreshTimelineState(false);
    }

    /// <summary>
    /// Clears temporary drag state after a timeline drag finishes.
    /// </summary>
    public void OnTimelineCameraDragCompleted() => _activeDraggedCameraIndex = -1;

    #endregion Preview and playback

    #region Timecode helpers

    /// <summary>
    /// Returns the timeline time for a camera index in the current sequence.
    /// </summary>
    private float GetTimecodeForCamera(int index) => GetSequenceTiming().GetCameraTime(index);

    /// <summary>
    /// Builds the marker and cache state needed by the timeline control.
    /// </summary>
    public FlybyTimelineRenderState BuildTimelineRenderState()
    {
        var cameras = GetCamerasAsList();
        var cache = GetSequenceCache(cameras);
        FlybySequenceTiming timing;

        if (cache is not null && cache.Timing.CameraCount == cameras.Count)
        {
            timing = cache.Timing;
            CacheSequenceTiming(cameras, timing);
        }
        else
        {
            timing = GetSequenceTiming(cameras);
        }

        var selectedIndices = GetSelectedIndices();
        var cutBypassedSegments = GetCutBypassedSegments(cameras);
        var markers = new List<FlybyTimelineMarker>(CameraList.Count);

        for (int i = 0; i < CameraList.Count; i++)
            markers.Add(BuildTimelineMarker(CameraList[i], i, selectedIndices, cutBypassedSegments, timing));

        return new FlybyTimelineRenderState(markers, cache, GetNormalizedTimelineDuration(timing));
    }

    /// <summary>
    /// Builds the rendered timeline marker data for a single camera.
    /// </summary>
    private static FlybyTimelineMarker BuildTimelineMarker(FlybyCameraItemViewModel item, int index,
        IReadOnlySet<int> selectedIndices, IReadOnlySet<int> cutBypassedSegments, FlybySequenceTiming timing)
    {
        var camera = item.Camera;

        return new FlybyTimelineMarker
        {
            TimeSeconds = timing.GetCameraTime(index),
            IsDuplicate = item.IsDuplicateIndex,
            IsSelected = selectedIndices.Contains(index),
            HasCameraCut = (camera.Flags & FlybyConstants.FlagCameraCut) != 0,
            IsInCutBypass = cutBypassedSegments.Contains(index),
            CutBypassDuration = timing.GetCutBypassDuration(index),
            SegmentDuration = index < timing.CameraCount - 1 ? timing.GetSegmentDuration(index) : 0.0f,
            HasFreeze = (camera.Flags & FlybyConstants.FlagFreezeCamera) != 0,
            FreezeDuration = timing.GetFreezeDuration(index)
        };
    }

    /// <summary>
    /// Returns the duration the timeline should expose to the control.
    /// </summary>
    private static float GetNormalizedTimelineDuration(FlybySequenceTiming timing)
        => float.IsFinite(timing.TotalDuration) ? Math.Max(timing.TotalDuration, 1.0f) : 1.0f;

    /// <summary>
    /// Returns segment indices whose outgoing spans are bypassed by camera cuts.
    /// </summary>
    private static IReadOnlySet<int> GetCutBypassedSegments(IReadOnlyList<FlybyCameraInstance> cameras)
    {
        var cutBypassedSegments = new HashSet<int>();

        for (int i = 0; i < cameras.Count; i++)
        {
            if (!FlybySequenceHelper.TryResolveCutTargetIndex(cameras, i, out int targetIndex))
                continue;

            for (int j = i; j < targetIndex && j < cameras.Count - 1; j++)
                cutBypassedSegments.Add(j);
        }

        return cutBypassedSegments;
    }

    /// <summary>
    /// Returns the sequence cache for the current selection when available.
    /// </summary>
    private FlybySequenceCache? GetSequenceCache(IReadOnlyList<FlybyCameraInstance> cameras)
    {
        if (!SelectedSequence.HasValue)
            return null;

        var cache = _preview.GetOrBuildCache(cameras, SelectedSequence.Value);

        if (cache is not null && cache.Timing.CameraCount == cameras.Count)
            CacheSequenceTiming(cameras, cache.Timing);

        return cache;
    }

    /// <summary>
    /// Recomputes formatted timecodes for every visible camera item.
    /// </summary>
    private void RecalculateTimecodes()
    {
        var timing = GetSequenceTiming();

        for (int i = 0; i < CameraList.Count; i++)
            CameraList[i].Timecode = FlybySequenceHelper.FormatTimecode(timing.GetCameraTime(i));

        RefreshPlayheadTimecode();
    }

    /// <summary>
    /// Updates the formatted playhead timecode whenever the playhead position changes.
    /// </summary>
    partial void OnPlayheadSecondsChanged(float value)
        => RefreshPlayheadTimecode(value);

    /// <summary>
    /// Recomputes the displayed real-time playhead label.
    /// </summary>
    private void RefreshPlayheadTimecode()
        => RefreshPlayheadTimecode(PlayheadSeconds);

    /// <summary>
    /// Recomputes the displayed real-time playhead label for the provided timeline position.
    /// </summary>
    private void RefreshPlayheadTimecode(float timelineSeconds)
    {
        float displayTimelineSeconds = float.IsFinite(timelineSeconds) && timelineSeconds >= 0.0f ? timelineSeconds : 0.0f;
        float realPlaybackSeconds = GetRealPlaybackSeconds(displayTimelineSeconds);

        PlayheadTimecode = FlybySequenceHelper.FormatTimecode(realPlaybackSeconds);
    }

    /// <summary>
    /// Returns the wall-clock playback time represented by the current timeline position.
    /// </summary>
    private float GetRealPlaybackSeconds(float timelineSeconds)
    {
        if (!SelectedSequence.HasValue || CameraList.Count == 0)
            return 0.0f;

        return GetSequenceTiming().TimelineToPlaybackTime(timelineSeconds);
    }

    /// <summary>
    /// Clears the visible playhead when sequence context changes.
    /// </summary>
    private void ResetPlayhead() => PlayheadSeconds = -1.0f;

    #endregion Timecode helpers

    #region Preview state sync

    /// <summary>
    /// Synchronizes bindable preview flags from the preview controller.
    /// </summary>
    private void OnPreviewStateChanged()
    {
        if (_isDisposed)
            return;

        IsPlaying = _preview.IsPlaying;
        OnPropertyChanged(nameof(IsPreviewActive));
        OnPropertyChanged(nameof(CanEditProperties));
    }

    /// <summary>
    /// Synchronizes the playhead position from preview playback.
    /// </summary>
    private void OnPreviewPlayheadChanged()
    {
        if (_isDisposed)
            return;

        PlayheadSeconds = _preview.PlayheadSeconds;
    }

    #endregion Preview state sync
}
