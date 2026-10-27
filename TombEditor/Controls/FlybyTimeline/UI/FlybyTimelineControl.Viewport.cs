#nullable enable

using System;
using System.Windows;

namespace TombEditor.Controls.FlybyTimeline.UI;

// Coordinate conversion, scrolling, zooming, and smooth viewport animation.
public partial class FlybyTimelineControl
{
    /// <summary>
    /// Stops viewport animation when the control is unloaded.
    /// </summary>
    private void OnUnloaded(object? sender, RoutedEventArgs e)
        => StopSmoothViewport(false);

    /// <summary>
    /// Advances the smooth viewport animation toward its target range.
    /// </summary>
    private void OnSmoothViewportTick(object? sender, EventArgs e)
    {
        float startDelta = _smoothViewportTargetStartSeconds - _visibleStartSeconds;
        float endDelta = _smoothViewportTargetEndSeconds - _visibleEndSeconds;

        if (MathF.Abs(startDelta) <= FlybyConstants.TimelineSmoothViewportEpsilon &&
            MathF.Abs(endDelta) <= FlybyConstants.TimelineSmoothViewportEpsilon)
        {
            SetViewport(_smoothViewportTargetStartSeconds, _smoothViewportTargetEndSeconds, false);
            StopSmoothViewport(false);
            InvalidateVisual();
            return;
        }

        SetViewport(
            _visibleStartSeconds + (startDelta * FlybyConstants.TimelineSmoothViewportLerpFactor),
            _visibleEndSeconds + (endDelta * FlybyConstants.TimelineSmoothViewportLerpFactor),
            false);

        InvalidateVisual();
    }

    /// <summary>
    /// Pans the viewport by a time delta.
    /// </summary>
    private void PanBy(float deltaSeconds, bool smooth)
    {
        GetInteractiveViewport(out float startSeconds, out float endSeconds);
        float visibleRange = endSeconds - startSeconds;

        if (visibleRange <= 0)
            return;

        float targetStart = ClampVisibleStart(startSeconds + deltaSeconds, visibleRange);
        ApplyViewport(targetStart, targetStart + visibleRange, smooth);
    }

    /// <summary>
    /// Clamps the viewport start so the visible range stays within bounds.
    /// </summary>
    private float ClampVisibleStart(float newStart, float visibleRange)
    {
        if (!float.IsFinite(newStart) || !float.IsFinite(visibleRange) || visibleRange <= 0.0f)
            return 0.0f;

        return Math.Clamp(newStart, 0.0f, GetMaxViewportStart(visibleRange));
    }

    /// <summary>
    /// Returns the maximum allowed viewport start for a given visible range.
    /// </summary>
    private float GetMaxViewportStart(float visibleRange)
    {
        if (!float.IsFinite(visibleRange) || visibleRange <= 0.0f)
            return 0.0f;

        float maxEnd = Math.Max(_totalDurationSeconds * 1.5f, visibleRange);
        return Math.Max(0.0f, maxEnd - visibleRange);
    }

    /// <summary>
    /// Normalizes the stored viewport so it remains valid.
    /// </summary>
    private void NormalizeVisibleViewport()
    {
        if (!float.IsFinite(_visibleStartSeconds) || !float.IsFinite(_visibleEndSeconds))
        {
            _visibleStartSeconds = 0.0f;
            _visibleEndSeconds = Math.Max(1.0f, _totalDurationSeconds);
            return;
        }

        if (_visibleStartSeconds >= _visibleEndSeconds)
        {
            _visibleStartSeconds = 0.0f;
            _visibleEndSeconds = Math.Max(1.0f, _totalDurationSeconds);
            return;
        }

        float visibleRange = _visibleEndSeconds - _visibleStartSeconds;

        if (visibleRange <= 0)
        {
            _visibleStartSeconds = 0.0f;
            _visibleEndSeconds = Math.Max(1.0f, _totalDurationSeconds);
            return;
        }

        _visibleStartSeconds = ClampVisibleStart(_visibleStartSeconds, visibleRange);
        _visibleEndSeconds = _visibleStartSeconds + visibleRange;
    }

    /// <summary>
    /// Returns the viewport that should be used for active interactions.
    /// </summary>
    private void GetInteractiveViewport(out float startSeconds, out float endSeconds)
    {
        if (_smoothViewportTimer.IsEnabled)
        {
            startSeconds = _smoothViewportTargetStartSeconds;
            endSeconds = _smoothViewportTargetEndSeconds;
            return;
        }

        startSeconds = _visibleStartSeconds;
        endSeconds = _visibleEndSeconds;
    }

    /// <summary>
    /// Clamps a viewport range to the allowed timeline bounds.
    /// </summary>
    private void ClampViewportToBounds(ref float startSeconds, ref float endSeconds)
    {
        if (!float.IsFinite(startSeconds) || !float.IsFinite(endSeconds))
        {
            startSeconds = 0.0f;
            endSeconds = Math.Max(1.0f, _totalDurationSeconds);
            return;
        }

        float maxEnd = _totalDurationSeconds * 1.5f;

        if (startSeconds < 0.0f)
            startSeconds = 0.0f;

        if (endSeconds > maxEnd)
            endSeconds = maxEnd;
    }

    /// <summary>
    /// Validates and clamps a viewport before it is stored or animated.
    /// </summary>
    /// <param name="startSeconds">Requested viewport start time, updated in place to the clamped value.</param>
    /// <param name="endSeconds">Requested viewport end time, updated in place to the clamped value.</param>
    /// <returns><see langword="true"/> when the clamped viewport still has a positive range; <see langword="false"/> when the requested viewport collapses to an invalid or empty span.</returns>
    private bool TryNormalizeViewport(ref float startSeconds, ref float endSeconds)
    {
        ClampViewportToBounds(ref startSeconds, ref endSeconds);
        return endSeconds > startSeconds;
    }

    /// <summary>
    /// Applies a viewport immediately or through smooth animation.
    /// </summary>
    /// <param name="startSeconds">Requested viewport start time in seconds.</param>
    /// <param name="endSeconds">Requested viewport end time in seconds.</param>
    /// <param name="smooth"><see langword="true"/> to animate toward the target viewport; otherwise apply it immediately.</param>
    private void ApplyViewport(float startSeconds, float endSeconds, bool smooth)
    {
        if (!TryNormalizeViewport(ref startSeconds, ref endSeconds))
            return;

        if (smooth)
        {
            StartSmoothViewport(startSeconds, endSeconds);
            return;
        }

        StopSmoothViewport(false);
        SetViewport(startSeconds, endSeconds, true);
    }

    /// <summary>
    /// Starts smooth animation toward a target viewport.
    /// </summary>
    /// <param name="targetStart">Target viewport start time in seconds.</param>
    /// <param name="targetEnd">Target viewport end time in seconds.</param>
    private void StartSmoothViewport(float targetStart, float targetEnd)
    {
        if (!TryNormalizeViewport(ref targetStart, ref targetEnd))
            return;

        _smoothViewportTargetStartSeconds = targetStart;
        _smoothViewportTargetEndSeconds = targetEnd;

        if (!_smoothViewportTimer.IsEnabled)
            _smoothViewportTimer.Start();
    }

    /// <summary>
    /// Stops smooth viewport animation and optionally snaps to its target.
    /// </summary>
    /// <param name="snapToTarget"><see langword="true"/> to snap the viewport to the last requested smooth target after stopping the animation.</param>
    private void StopSmoothViewport(bool snapToTarget)
    {
        if (!_smoothViewportTimer.IsEnabled)
            return;

        _smoothViewportTimer.Stop();

        if (snapToTarget)
            SetViewport(_smoothViewportTargetStartSeconds, _smoothViewportTargetEndSeconds, false);
    }

    /// <summary>
    /// Stores the current viewport and optionally redraws the control.
    /// </summary>
    /// <param name="startSeconds">Viewport start time in seconds.</param>
    /// <param name="endSeconds">Viewport end time in seconds.</param>
    /// <param name="invalidateVisual"><see langword="true"/> to redraw the control immediately after storing the viewport.</param>
    private void SetViewport(float startSeconds, float endSeconds, bool invalidateVisual)
    {
        if (!TryNormalizeViewport(ref startSeconds, ref endSeconds))
            return;

        _visibleStartSeconds = startSeconds;
        _visibleEndSeconds = endSeconds;

        if (invalidateVisual)
            InvalidateVisual();
    }

    /// <summary>
    /// Snaps the stored viewport to the currently interactive viewport state.
    /// </summary>
    private void SnapViewportToInteractiveState()
    {
        GetInteractiveViewport(out float startSeconds, out float endSeconds);
        StopSmoothViewport(false);
        SetViewport(startSeconds, endSeconds, false);
    }

    /// <summary>
    /// Converts a timeline time to an x-coordinate in the current viewport.
    /// </summary>
    /// <param name="timeSeconds">Timeline time to project.</param>
    /// <param name="width">Current control width in pixels.</param>
    /// <returns>The projected x-coordinate in pixels.</returns>
    private float TimeToPixel(float timeSeconds, float width)
    {
        float range = _visibleEndSeconds - _visibleStartSeconds;

        if (width <= 0.0f || !float.IsFinite(timeSeconds) || !float.IsFinite(range) || range <= 0.0f)
            return 0.0f;

        return (timeSeconds - _visibleStartSeconds) / range * width;
    }

    /// <summary>
    /// Converts a timeline time to the ruler's displayed time.
    /// </summary>
    /// <param name="timeSeconds">Timeline time to convert.</param>
    /// <returns>The displayed ruler time in seconds.</returns>
    private float TimelineToRulerTime(float timeSeconds)
    {
        if (!float.IsFinite(timeSeconds))
            return timeSeconds;

        float rulerTime = _cache?.Timing.TimelineToPlaybackTime(timeSeconds) ?? timeSeconds;
        return float.IsFinite(rulerTime) ? rulerTime : timeSeconds;
    }

    /// <summary>
    /// Converts a displayed ruler time back into timeline time for viewport projection.
    /// </summary>
    /// <param name="rulerTime">Displayed ruler time in seconds.</param>
    /// <returns>The corresponding timeline time in seconds.</returns>
    private float RulerTimeToTimelineTime(float rulerTime)
    {
        if (!float.IsFinite(rulerTime) || _cache?.Timing is null)
            return rulerTime;

        float accumulatedCutTime = 0.0f;

        foreach (var cut in _cache.Timing.CutRegions)
        {
            float cutPlaybackStart = cut.StartTime - accumulatedCutTime;
            float playbackDelta = rulerTime - cutPlaybackStart;

            if (playbackDelta < -FlybyConstants.CutBoundaryTolerance)
                break;

            if (MathF.Abs(playbackDelta) <= FlybyConstants.CutBoundaryTolerance)
                return cut.StartTime;

            accumulatedCutTime += cut.Duration;
        }

        return rulerTime + accumulatedCutTime;
    }

    /// <summary>
    /// Converts the visible viewport into the ruler's displayed time range.
    /// </summary>
    /// <param name="rulerStartSeconds">Receives the visible ruler start time.</param>
    /// <param name="rulerEndSeconds">Receives the visible ruler end time.</param>
    /// <returns><see langword="true"/> when the visible viewport spans a positive playback range; otherwise <see langword="false"/>.</returns>
    private bool TryGetVisibleRulerRange(out float rulerStartSeconds, out float rulerEndSeconds)
    {
        rulerStartSeconds = TimelineToRulerTime(_visibleStartSeconds);
        rulerEndSeconds = TimelineToRulerTime(_visibleEndSeconds);

        return float.IsFinite(rulerStartSeconds) &&
            float.IsFinite(rulerEndSeconds) &&
            rulerEndSeconds > rulerStartSeconds;
    }

    /// <summary>
    /// Returns whether the entire visible viewport lies inside one cut-bypassed region.
    /// </summary>
    private bool IsVisibleViewportInsideCutRegion()
    {
        if (_cache?.Timing is null)
            return false;

        foreach (var cut in _cache.Timing.CutRegions)
        {
            if (_visibleStartSeconds >= cut.StartTime - FlybyConstants.CutBoundaryTolerance &&
                _visibleEndSeconds <= cut.EndTime + FlybyConstants.CutBoundaryTolerance)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Converts a marker time to a pixel position when the marker has a valid timeline time.
    /// </summary>
    /// <param name="marker">Marker whose x-position should be resolved.</param>
    /// <param name="width">Current control width in pixels.</param>
    /// <param name="x">Receives the resolved x-coordinate when the marker can be projected into the current viewport.</param>
    /// <returns><see langword="true"/> when the marker time and viewport state are valid and an x-coordinate was produced; <see langword="false"/> when the marker cannot be projected.</returns>
    private bool TryGetMarkerPixel(FlybyTimelineMarker marker, float width, out float x)
    {
        x = 0.0f;

        float range = _visibleEndSeconds - _visibleStartSeconds;

        if (width <= 0.0f || !float.IsFinite(marker.TimeSeconds) || !float.IsFinite(range) || range <= 0.0f)
            return false;

        if (marker.TimeSeconds < _visibleStartSeconds - FlybyConstants.TimelineViewportTimeTolerance ||
            marker.TimeSeconds > _visibleEndSeconds + FlybyConstants.TimelineViewportTimeTolerance)
        {
            return false;
        }

        x = TimeToPixel(marker.TimeSeconds, width);
        return float.IsFinite(x);
    }

    /// <summary>
    /// Converts an x-coordinate to a timeline time using the current viewport.
    /// </summary>
    /// <param name="pixel">X-coordinate in pixels.</param>
    /// <param name="width">Current control width in pixels.</param>
    /// <returns>The corresponding timeline time in seconds.</returns>
    private float PixelToTime(float pixel, float width)
        => PixelToTime(pixel, width, _visibleStartSeconds, _visibleEndSeconds);

    /// <summary>
    /// Converts an x-coordinate to a timeline time for the provided viewport.
    /// </summary>
    /// <param name="pixel">X-coordinate in pixels.</param>
    /// <param name="width">Control width in pixels.</param>
    /// <param name="visibleStartSeconds">Viewport start time in seconds.</param>
    /// <param name="visibleEndSeconds">Viewport end time in seconds.</param>
    /// <returns>The corresponding timeline time in seconds for the provided viewport.</returns>
    private static float PixelToTime(float pixel, float width, float visibleStartSeconds, float visibleEndSeconds)
    {
        if (width <= 0.0f || !float.IsFinite(pixel) || !float.IsFinite(visibleStartSeconds) || !float.IsFinite(visibleEndSeconds))
            return float.IsFinite(visibleStartSeconds) ? visibleStartSeconds : 0.0f;

        float range = visibleEndSeconds - visibleStartSeconds;

        if (!float.IsFinite(range) || range <= 0.0f)
            return visibleStartSeconds;

        return visibleStartSeconds + (pixel / width * range);
    }
}
