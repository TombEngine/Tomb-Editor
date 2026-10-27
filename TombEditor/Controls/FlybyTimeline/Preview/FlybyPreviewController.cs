#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;
using TombEditor.Controls.FlybyTimeline.Sequence;
using TombLib.Forms;
using TombLib.LevelData;

namespace TombEditor.Controls.FlybyTimeline.Preview;

/// <summary>
/// Manages flyby camera preview and playback lifecycle.
/// Scrubbing and playback are backed by a pre-calculated <see cref="FlybySequenceCache"/>;
/// all time-to-frame mapping is a simple array lookup with linear interpolation.
/// </summary>
public sealed class FlybyPreviewController(Editor editor) : IDisposable
{
    private readonly Editor _editor = editor;

    private FlybySequenceCache? _cache;
    private ushort? _cacheSequence;
    private FlybyCameraInstance[]? _cacheCameras;
    private FlybyPreview? _playbackPreview;
    private DispatcherTimer? _playbackTimer;
    private bool _isChangingPreview;

    private bool UseSmoothPause => FlybyConstants.UseSmoothPause(_editor.Level?.Settings.GameVersion);

    /// <summary>
    /// Gets whether the editor is currently in any camera preview mode.
    /// </summary>
    public bool IsPreviewActive => _editor.CameraPreviewMode != CameraPreviewType.None;

    /// <summary>
    /// Gets whether timed sequence playback is currently running.
    /// </summary>
    public bool IsPlaying { get; private set; }

    /// <summary>
    /// Gets the current timeline playhead position in seconds.
    /// </summary>
    public float PlayheadSeconds { get; private set; } = -1.0f;

    /// <summary>
    /// Raised when preview or playback state changes.
    /// </summary>
    public event Action? StateChanged;

    /// <summary>
    /// Raised when the playhead position changes.
    /// </summary>
    public event Action? PlayheadChanged;

    /// <summary>
    /// Exits preview mode and stops any active playback.
    /// </summary>
    public void ExitPreview()
    {
        if (!IsPreviewActive)
            return;

        StopPlaybackCore(false);
        SetPreviewActive(false);

        StateChanged?.Invoke();
    }

    /// <summary>
    /// Updates the shown camera while static preview is active.
    /// </summary>
    /// <param name="camera">The flyby camera to display.</param>
    public void ShowCamera(FlybyCameraInstance camera)
    {
        if (!IsPreviewActive || IsPlaying)
            return;

        if (!EnsureStaticPreviewActive())
            return;

        _editor.CameraPreviewUpdated(camera);
    }

    /// <summary>
    /// Starts playback for the provided sequence.
    /// </summary>
    /// <param name="cameras">The flyby cameras belonging to the sequence.</param>
    /// <param name="sequence">The sequence number to preview.</param>
    /// <param name="startTimelineSeconds">The timeline time, in seconds, from which playback should begin.</param>
    public void StartPlayback(IReadOnlyList<FlybyCameraInstance> cameras, ushort sequence, float startTimelineSeconds)
    {
        if (cameras.Count < 2)
        {
            _editor.SendMessage("Flyby sequence needs at least 2 cameras to play.", PopupType.Info);
            return;
        }

        if (!EnsureStaticPreviewActive())
            return;

        var camera = _editor.GetViewportCamera?.Invoke();

        if (camera is null)
            return;

        if (!TryEnsureValidCache(cameras, sequence, out var cache))
            return;

        if (IsPlaying)
            StopPlaybackCore(false);

        _playbackPreview?.Dispose();
        _playbackPreview = new FlybyPreview(cache, camera);

        float normalizedStartTime = NormalizePlaybackStartTime(startTimelineSeconds, cache.TotalDuration);
        float startOffset = normalizedStartTime > 0.0f
            ? cache.Timing.TimelineToPlaybackTime(normalizedStartTime)
            : 0.0f;

        _playbackPreview.BeginExternalUpdate(startOffset);
        SetPlayheadSeconds(normalizedStartTime);

        IsPlaying = true;

        _playbackTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(FlybyConstants.PreviewTimerInterval)
        };

        _playbackTimer.Tick += OnPlaybackTimerTick;
        _playbackTimer.Start();

        StateChanged?.Invoke();
    }

    /// <summary>
    /// Stops active playback if it is running.
    /// </summary>
    public void StopPlayback()
    {
        if (!IsPlaying)
            return;

        StopPlaybackCore(true);
    }

    /// <summary>
    /// Scrubs preview playback to a specific time in the sequence.
    /// </summary>
    /// <param name="cameras">The flyby cameras belonging to the sequence.</param>
    /// <param name="sequence">The sequence number to sample.</param>
    /// <param name="timeSeconds">The timeline time, in seconds, to preview.</param>
    /// <returns>The sampled frame used for preview, or <see langword="null"/> when no frame could be resolved.</returns>
    public FlybyFrameState? ScrubToTime(IReadOnlyList<FlybyCameraInstance> cameras, ushort sequence, float timeSeconds)
    {
        if (cameras.Count == 0)
            return null;

        if (!float.IsFinite(timeSeconds))
            return null;

        if (IsPlaying)
            StopPlayback();

        if (!EnsureStaticPreviewActive())
            return null;

        FlybyFrameState? frame = null;

        if (TryEnsureValidCache(cameras, sequence, out var cache))
        {
            var sampledFrame = cache.SampleAtTime(timeSeconds);

            _editor.CameraPreviewScrub(sampledFrame);
            frame = sampledFrame;
        }
        else
        {
            var timing = FlybySequenceTiming.Build(cameras, UseSmoothPause);
            int nearestIndex = FlybySequenceHelper.FindCameraIndexAtTime(cameras, timeSeconds, timing);
            var camera = cameras[nearestIndex];

            _editor.CameraPreviewUpdated(camera);

            if (camera.Room is not null)
                frame = FlybyPreview.GetFrameForCamera(camera);
        }

        SetPlayheadSeconds(timeSeconds);
        return frame;
    }

    /// <summary>
    /// Called when an external preview exit is detected via editor event (e.g. ESC key).
    /// </summary>
    public void OnExternalPreviewExit()
    {
        if (_isChangingPreview)
            return;

        bool wasPreviewActive = IsPreviewActive;
        bool wasPlaying = IsPlaying;

        if (!wasPreviewActive && !wasPlaying && PlayheadSeconds < 0.0f)
            return;

        StopPlaybackCore(false);
        SetPlayheadSeconds(-1.0f);

        if (wasPreviewActive || wasPlaying)
            StateChanged?.Invoke();
    }

    /// <summary>
    /// Returns the current sequence cache, building it if necessary.
    /// </summary>
    /// <param name="cameras">The flyby cameras belonging to the sequence.</param>
    /// <param name="sequence">The sequence number to resolve.</param>
    /// <returns>The matching sequence cache, or <see langword="null"/> when no valid cache can be produced.</returns>
    public FlybySequenceCache? GetOrBuildCache(IReadOnlyList<FlybyCameraInstance> cameras, ushort sequence)
        => TryEnsureValidCache(cameras, sequence, out var cache) ? cache : null;

    /// <summary>
    /// Invalidates the current cache (e.g. after camera edits) and forces a rebuild on next access.
    /// Does not raise any events or change preview state by itself.
    /// </summary>
    public void InvalidateCache()
    {
        _cache = null;
        _cacheSequence = null;
        _cacheCameras = null;
    }

    /// <summary>
    /// Releases preview state and playback resources.
    /// </summary>
    public void Dispose()
    {
        StopPlaybackCore(false);
        InvalidateCache();

        if (IsPreviewActive)
            SetPreviewActive(false);
    }

    /// <summary>
    /// Ensures the editor is in static preview mode before preview updates are sent.
    /// </summary>
    private bool EnsureStaticPreviewActive()
    {
        if (_editor.FlyMode)
            return false;

        if (_editor.CameraPreviewMode == CameraPreviewType.Static)
            return true;

        if (IsPreviewActive)
            SetPreviewActive(false);

        SetPreviewActive(true);
        return _editor.CameraPreviewMode == CameraPreviewType.Static;
    }

    /// <summary>
    /// Toggles the editor camera preview state while suppressing feedback loops.
    /// </summary>
    private void SetPreviewActive(bool enabled)
    {
        _isChangingPreview = true;

        try
        {
            _editor.ToggleCameraPreview(enabled);
        }
        finally
        {
            _isChangingPreview = false;
        }
    }

    /// <summary>
    /// Advances playback and updates the editor preview frame.
    /// </summary>
    private void OnPlaybackTick()
    {
        if (_playbackPreview is null)
        {
            StopPlaybackCore(true);
            return;
        }

        if (_editor.CameraPreviewMode != CameraPreviewType.Static)
        {
            OnExternalPreviewExit();
            return;
        }

        var frame = _playbackPreview.Update();

        if (_playbackPreview.IsFinished)
        {
            float totalDuration = _playbackPreview.Cache.TotalDuration; // Cache before stopping playback, as preview dispose will invalidate the cache reference.
            StopPlayback();

            SetPlayheadSeconds(totalDuration);
            return;
        }

        _editor.CameraPreviewScrub(frame);

        SetPlayheadSeconds(_playbackPreview.GetCurrentTimeSeconds());
    }

    /// <summary>
    /// Forwards timer ticks to the playback update routine.
    /// </summary>
    private void OnPlaybackTimerTick(object? sender, EventArgs e)
        => OnPlaybackTick();

    /// <summary>
    /// Normalizes a requested timeline start time for playback.
    /// </summary>
    private static float NormalizePlaybackStartTime(float timeSeconds, float totalDuration)
    {
        if (!float.IsFinite(timeSeconds) || timeSeconds <= 0.0f)
            return 0.0f;

        if (timeSeconds >= totalDuration - FlybyConstants.PreviewReplayEndTolerance)
            return 0.0f;

        return Math.Min(timeSeconds, totalDuration);
    }

    /// <summary>
    /// Updates the cached playhead position and notifies listeners.
    /// </summary>
    private void SetPlayheadSeconds(float timeSeconds)
    {
        if (!float.IsFinite(timeSeconds))
            timeSeconds = -1.0f;

        if (PlayheadSeconds == timeSeconds)
            return;

        PlayheadSeconds = timeSeconds;
        PlayheadChanged?.Invoke();
    }

    /// <summary>
    /// Stops timer-driven playback and optionally raises a state change event.
    /// </summary>
    private void StopPlaybackCore(bool raiseStateChanged)
    {
        if (_playbackTimer is not null)
        {
            _playbackTimer.Stop();
            _playbackTimer.Tick -= OnPlaybackTimerTick;
            _playbackTimer = null;
        }

        _playbackPreview?.Dispose();
        _playbackPreview = null;

        bool wasPlaying = IsPlaying;
        IsPlaying = false;

        if (raiseStateChanged && wasPlaying)
            StateChanged?.Invoke();
    }

    /// <summary>
    /// Ensures the cache matches the current sequence and camera list.
    /// </summary>
    /// <param name="cameras">The flyby cameras that should back the cache.</param>
    /// <param name="sequence">The sequence id the cache should represent.</param>
    /// <param name="cache">Receives the matching cache instance when one is available.</param>
    /// <returns><see langword="true"/> when a valid cache exists or is built successfully; <see langword="false"/> when no valid cache can be produced.</returns>
    private bool TryEnsureValidCache(IReadOnlyList<FlybyCameraInstance> cameras, ushort sequence, [NotNullWhen(true)] out FlybySequenceCache? cache)
    {
        bool cacheMatches = _cache is not null && _cacheSequence == sequence &&
            FlybySequenceHelper.CameraListsMatchByReference(_cacheCameras, cameras);

        if (!cacheMatches)
        {
            if (_editor.Level is null)
            {
                InvalidateCache();
            }
            else
            {
                _cache = FlybySequenceCache.Build(cameras, UseSmoothPause);
                _cacheSequence = sequence;
                _cacheCameras = [.. cameras];
            }
        }

        cache = _cache;
        return cache?.IsValid == true;
    }
}
