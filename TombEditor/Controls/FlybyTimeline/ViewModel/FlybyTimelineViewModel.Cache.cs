#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Threading;
using TombEditor.Controls.FlybyTimeline.Sequence;
using TombLib.LevelData;

namespace TombEditor.Controls.FlybyTimeline.ViewModel;

// Timing cache, sequence cache, and camera snapshot helpers.
public partial class FlybyTimelineViewModel
{
    #region Timing cache

    /// <summary>
    /// Returns the current or freshly built timing data for the visible sequence cameras.
    /// </summary>
    private FlybySequenceTiming GetSequenceTiming(IReadOnlyList<FlybyCameraInstance>? cameras = null)
    {
        var sequenceCameras = cameras ?? GetCamerasAsList();

        if (TryGetCachedSequenceTiming(sequenceCameras, out var cachedSequenceTiming))
            return cachedSequenceTiming;

        var timing = FlybySequenceTiming.Build(sequenceCameras, UseSmoothPause);
        CacheSequenceTiming(sequenceCameras, timing);
        return timing;
    }

    /// <summary>
    /// Returns cached sequence timing when it still matches the provided camera list by reference.
    /// </summary>
    /// <param name="cameras">Visible sequence cameras that the cached timing must correspond to.</param>
    /// <param name="cachedSequenceTiming">Receives the cached timing when it still matches the provided camera list.</param>
    /// <returns><see langword="true"/> when cached timing exists and was built from the same camera instances in the same order; <see langword="false"/> when the timing must be rebuilt.</returns>
    private bool TryGetCachedSequenceTiming(IReadOnlyList<FlybyCameraInstance> cameras,
        [NotNullWhen(true)] out FlybySequenceTiming? cachedSequenceTiming)
    {
        cachedSequenceTiming = _cachedSequenceTiming;
        var cachedTimingCameras = _cachedSequenceTimingCameras;

        if (cachedSequenceTiming is null || cachedTimingCameras is null)
            return false;

        if (cachedSequenceTiming.CameraCount != cameras.Count)
            return false;

        return FlybySequenceHelper.CameraListsMatchByReference(cachedTimingCameras, cameras);
    }

    /// <summary>
    /// Stores sequence timing together with the camera list it was derived from.
    /// </summary>
    private void CacheSequenceTiming(IReadOnlyList<FlybyCameraInstance> cameras, FlybySequenceTiming timing)
    {
        _cachedSequenceTiming = timing;
        _cachedSequenceTimingCameras = [.. cameras];
    }

    /// <summary>
    /// Clears cached sequence timing while keeping the visible camera list.
    /// </summary>
    private void InvalidateSequenceTiming()
    {
        _cachedSequenceTiming = null;
        _cachedSequenceTimingCameras = null;
    }

    /// <summary>
    /// Clears the cached visible camera list and any timing derived from it.
    /// </summary>
    private void InvalidateVisibleCameraState()
    {
        _cachedVisibleCameras = null;
        _cachedSequenceTiming = null;
        _cachedSequenceTimingCameras = null;
    }

    #endregion Timing cache

    #region Camera list helpers

    /// <summary>
    /// Returns the current sequence cameras, falling back to the editor state when the camera list is empty.
    /// </summary>
    private IReadOnlyList<FlybyCameraInstance> GetCamerasWithFallback()
    {
        if (!SelectedSequence.HasValue || _editor.Level is null)
            return [];

        if (CameraList.Count > 0)
            return GetCamerasAsList();

        return FlybySequenceHelper.GetCameras(_editor.Level, SelectedSequence.Value);
    }

    /// <summary>
    /// Returns the visible camera items as a materialized camera list.
    /// </summary>
    private IReadOnlyList<FlybyCameraInstance> GetCamerasAsList()
    {
        if (TryGetCachedVisibleCameras(out var cachedVisibleCameras))
            return cachedVisibleCameras;

        _cachedVisibleCameras = [.. CameraList.Select(vm => vm.Camera)];
        return _cachedVisibleCameras;
    }

    /// <summary>
    /// Returns the cached visible camera list when it still matches the current camera items by reference.
    /// </summary>
    /// <param name="cachedVisibleCameras">Receives the cached visible camera list when it still matches the current camera items.</param>
    /// <returns><see langword="true"/> when the cached list is still valid for the current <see cref="CameraList"/> contents; <see langword="false"/> when the list must be rebuilt.</returns>
    private bool TryGetCachedVisibleCameras([NotNullWhen(true)] out FlybyCameraInstance[]? cachedVisibleCameras)
    {
        cachedVisibleCameras = _cachedVisibleCameras;

        if (cachedVisibleCameras is null || cachedVisibleCameras.Length != CameraList.Count)
            return false;

        for (int i = 0; i < CameraList.Count; i++)
        {
            if (!ReferenceEquals(cachedVisibleCameras[i], CameraList[i].Camera))
                return false;
        }

        return true;
    }

    #endregion Camera list helpers

    #region Queued refresh

    /// <summary>
    /// Queues a batched timeline and preview refresh on the dispatcher so rapid property changes collapse into one update.
    /// </summary>
    /// <param name="refreshCameraList"><see langword="true"/> to rebuild the visible camera list before refreshing timeline state.</param>
    /// <param name="syncPreview"><see langword="true"/> to synchronize the preview after queued updates complete.</param>
    /// <param name="refreshTimeline"><see langword="true"/> to rebuild timeline timecodes and notify the view.</param>
    private void QueueTimelineRefresh(bool refreshCameraList, bool syncPreview = true, bool refreshTimeline = true)
    {
        if (_isDisposed)
            return;

        _queuedTimelineRefreshCameraList |= refreshCameraList;
        _queuedTimelineRefreshTimeline |= refreshTimeline;
        _queuedTimelineRefreshPreview |= syncPreview;

        if (_isTimelineRefreshQueued)
            return;

        _isTimelineRefreshQueued = true;

        _queuedTimelineRefreshOperation = _dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(ProcessQueuedTimelineRefresh));
    }

    /// <summary>
    /// Cancels any queued dispatcher refresh and clears the accumulated refresh state.
    /// </summary>
    private void AbortQueuedTimelineRefresh()
    {
        if (_queuedTimelineRefreshOperation?.Status is DispatcherOperationStatus.Pending or DispatcherOperationStatus.Executing)
        {
            _queuedTimelineRefreshOperation.Abort();
        }

        _queuedTimelineRefreshOperation = null;
        ClearQueuedTimelineRefreshState();
    }

    /// <summary>
    /// Runs the accumulated queued refresh work unless cleanup already disposed the view model.
    /// </summary>
    private void ProcessQueuedTimelineRefresh()
    {
        _queuedTimelineRefreshOperation = null;

        if (_isDisposed)
        {
            ClearQueuedTimelineRefreshState();
            return;
        }

        bool queuedRefreshCameraList = _queuedTimelineRefreshCameraList;
        bool queuedRefreshTimeline = _queuedTimelineRefreshTimeline;
        bool queuedRefreshPreview = _queuedTimelineRefreshPreview;

        ClearQueuedTimelineRefreshState();
        RefreshTimelineState(queuedRefreshCameraList, queuedRefreshPreview, queuedRefreshTimeline);
    }

    /// <summary>
    /// Clears the batched refresh flags after queued work is consumed or cancelled.
    /// </summary>
    private void ClearQueuedTimelineRefreshState()
    {
        _isTimelineRefreshQueued = false;
        _queuedTimelineRefreshCameraList = false;
        _queuedTimelineRefreshTimeline = false;
        _queuedTimelineRefreshPreview = false;
    }

    #endregion Queued refresh
}
