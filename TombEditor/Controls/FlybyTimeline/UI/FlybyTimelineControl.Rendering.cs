#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using TombEditor.Controls.FlybyTimeline.Sequence;

namespace TombEditor.Controls.FlybyTimeline.UI;

// WPF OnRender drawing for ruler, track, markers, and playhead.
public partial class FlybyTimelineControl
{
    private const int RulerTextCacheCapacity = 128;

    /// <summary>
    /// Renders the ruler, track, markers, selection, and playhead.
    /// </summary>
    protected override void OnRender(DrawingContext context)
    {
        base.OnRender(context);

        float w = (float)ActualWidth;
        float h = (float)ActualHeight;

        if (w <= 0 || h <= 0)
            return;

        context.DrawRectangle(BackgroundBrush, null, new Rect(0, 0, w, h));
        context.DrawRectangle(RulerBrush, null, new Rect(0, 0, w, FlybyConstants.TimelineRulerHeight));

        DrawTimeRuler(context, w);

        const float trackY = FlybyConstants.TimelineRulerHeight;
        float trackHeight = Math.Max(1.0f, h - FlybyConstants.TimelineRulerHeight);
        context.DrawRectangle(TrackBrush, null, new Rect(0, trackY, w, trackHeight));

        DrawSegmentRegions(context, w, trackY, trackHeight);
        DrawSpeedCurve(context, w, trackY, trackHeight);
        DrawMarkers(context, w, trackY, trackHeight);

        if (_interactionMode == InteractionMode.RangeSelecting && HasExceededSelectionThreshold(_rangeEndPoint))
        {
            GetRangeSelectionBounds(out float selLeft, out float selRight);
            context.DrawRectangle(SelectionBrush, null, new Rect(selLeft, trackY, selRight - selLeft, trackHeight));
        }

        if (_isMouseOver && _mouseX >= 0 && _mouseX <= w)
            context.DrawLine(CursorLinePen, new Point(_mouseX, 0), new Point(_mouseX, h));

        if (_playheadSeconds >= 0)
        {
            float phX = TimeToPixel(_playheadSeconds, w);

            if (phX >= 0 && phX <= w)
                context.DrawLine(PlayheadPen, new Point(phX, 0), new Point(phX, h));
        }
    }

    /// <summary>
    /// Draws ruler ticks and labels in displayed playback time for the current viewport.
    /// </summary>
    private void DrawTimeRuler(DrawingContext context, float width)
    {
        double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        if (!TryGetVisibleRulerRange(out float visibleRulerStartSeconds, out float visibleRulerEndSeconds))
        {
            if (!IsVisibleViewportInsideCutRegion())
                DrawCollapsedTimeRulerLabel(context, pixelsPerDip, TimelineToRulerTime(_visibleStartSeconds));

            return;
        }

        float visibleTimelineDuration = _visibleEndSeconds - _visibleStartSeconds;

        if (!float.IsFinite(visibleTimelineDuration) || visibleTimelineDuration <= 0.0f)
        {
            DrawCollapsedTimeRulerLabel(context, pixelsPerDip, visibleRulerStartSeconds);
            return;
        }

        float timelinePixelsPerSecond = width / visibleTimelineDuration;
        float tickInterval = CalculateTickInterval(timelinePixelsPerSecond);
        float startTick = MathF.Floor(visibleRulerStartSeconds / tickInterval) * tickInterval;
        float lastTickX = float.NegativeInfinity;

        for (float rulerTime = startTick; rulerTime <= visibleRulerEndSeconds; rulerTime += tickInterval)
        {
            float x = TimeToPixel(RulerTimeToTimelineTime(rulerTime), width);

            if (x < 0 || x > width)
                continue;

            if (float.IsFinite(lastTickX) && x <= lastTickX + 1.0f)
                continue;

            lastTickX = x;

            context.DrawLine(GridLinePen, new Point(x, 0), new Point(x, ActualHeight));

            string label = FlybySequenceHelper.FormatRulerLabel(rulerTime);
            var formattedText = GetRulerLabelText(label, pixelsPerDip);

            context.DrawText(formattedText, new Point(x + 2, 2));
        }
    }

    /// <summary>
    /// Draws a single ruler label when the visible viewport collapses to one playback instant.
    /// </summary>
    private void DrawCollapsedTimeRulerLabel(DrawingContext context, double pixelsPerDip, float rulerTime)
    {
        if (!float.IsFinite(rulerTime))
            return;

        string label = FlybySequenceHelper.FormatRulerLabel(rulerTime);
        var formattedText = GetRulerLabelText(label, pixelsPerDip);

        context.DrawText(formattedText, new Point(2, 2));
    }

    /// <summary>
    /// Returns cached formatted ruler text for the provided label.
    /// </summary>
    private FormattedText GetRulerLabelText(string label, double pixelsPerDip)
    {
        if (Math.Abs(_rulerTextPixelsPerDip - pixelsPerDip) > double.Epsilon)
        {
            _rulerTextCache.Clear();
            _rulerTextPixelsPerDip = pixelsPerDip;
        }

        if (_rulerTextCache.TryGetValue(label, out var formattedText))
            return formattedText;

        if (_rulerTextCache.Count >= RulerTextCacheCapacity)
            _rulerTextCache.Clear();

        formattedText = new FormattedText(
            label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            DefaultTypeface, 9, RulerTextBrush, pixelsPerDip);

        _rulerTextCache[label] = formattedText;
        return formattedText;
    }

    /// <summary>
    /// Draws freeze and cut-region overlays for visible track segments.
    /// Cut regions are collected once and drawn to both the ruler and track bands.
    /// </summary>
    /// <param name="context">Drawing context receiving the overlays.</param>
    /// <param name="width">Current control width in pixels.</param>
    /// <param name="trackY">Top y-coordinate of the track area.</param>
    /// <param name="trackHeight">Height of the track area in pixels.</param>
    private void DrawSegmentRegions(DrawingContext context, float width, float trackY, float trackHeight)
    {
        PopulateVisibleCutSpans(width);

        foreach (var (cutLeft, cutRight) in _visibleCutSpans)
        {
            float cutWidth = cutRight - cutLeft;
            context.DrawRectangle(CameraCutRegionRulerBrush, null, new Rect(cutLeft, 0, cutWidth, FlybyConstants.TimelineRulerHeight));
            DrawDiagonalHatch(context, cutLeft, trackY, cutWidth, trackHeight, CameraCutPen);
        }

        for (int i = 0; i < _markers.Count; i++)
        {
            var marker = _markers[i];

            if (!float.IsFinite(marker.TimeSeconds))
                continue;

            float startX = TimeToPixel(marker.TimeSeconds, width);

            if (marker.HasFreeze && float.IsFinite(marker.FreezeDuration) && marker.FreezeDuration > 0.0f)
            {
                float freezeRight = Math.Min(width, TimeToPixel(marker.TimeSeconds + marker.FreezeDuration, width));
                float freezeLeft = Math.Max(0.0f, startX);

                if (freezeRight > freezeLeft)
                    context.DrawRectangle(FreezeRegionBrush, null, new Rect(freezeLeft, trackY, freezeRight - freezeLeft, trackHeight));
            }
        }
    }

    /// <summary>
    /// Collects visible cut-region pixel spans from the current marker data.
    /// </summary>
    /// <param name="width">Current control width in pixels.</param>
    private void PopulateVisibleCutSpans(float width)
    {
        _visibleCutSpans.Clear();

        for (int i = 0; i < _markers.Count - 1; i++)
        {
            var marker = _markers[i];

            if (!float.IsFinite(marker.TimeSeconds) || !marker.HasCameraCut)
                continue;

            float bypassDuration = marker.CutBypassDuration > 0.0f
                ? marker.CutBypassDuration
                : marker.SegmentDuration;

            if (!float.IsFinite(bypassDuration) || bypassDuration <= 0.0f)
                continue;

            float cutLeft = Math.Max(0.0f, TimeToPixel(marker.TimeSeconds, width));
            float cutRight = Math.Min(width, TimeToPixel(marker.TimeSeconds + bypassDuration, width));

            if (cutRight > cutLeft)
                _visibleCutSpans.Add((cutLeft, cutRight));
        }
    }

    /// <summary>
    /// Draws diagonal hatch lines inside the provided rectangle.
    /// </summary>
    /// <param name="context">Drawing context receiving the hatch strokes.</param>
    /// <param name="x">Left edge of the hatch rectangle.</param>
    /// <param name="y">Top edge of the hatch rectangle.</param>
    /// <param name="w">Width of the hatch rectangle.</param>
    /// <param name="h">Height of the hatch rectangle.</param>
    /// <param name="pen">Pen used for the diagonal lines.</param>
    private static void DrawDiagonalHatch(DrawingContext context, float x, float y, float w, float h, Pen pen)
    {
        const float spacing = 6.0f;
        var clip = new RectangleGeometry(new Rect(x, y, w, h));

        context.PushClip(clip);

        for (float offset = -h; offset < w + h; offset += spacing)
        {
            context.DrawLine(pen,
                new Point(x + offset, y + h),
                new Point(x + offset + h, y));
        }

        context.Pop();
    }

    /// <summary>
    /// Returns the normalized speed sample for the given timeline time, or a negative value when no sample is available.
    /// </summary>
    private float GetNormalizedSpeedAtTime(float timeSeconds)
    {
        if (!float.IsFinite(timeSeconds) || _cache?.IsValid != true || _markers.Count < 2)
            return -1.0f;

        float speed = _cache.GetSpeedAtTime(timeSeconds);

        if (!float.IsFinite(speed) || speed < 0.0f)
            return -1.0f;

        return float.IsFinite(_peakSpeed) && _peakSpeed > FlybyConstants.MinSpeed ? speed / _peakSpeed : 0.0f;
    }

    /// <summary>
    /// Draws the mirrored cached speed waveform behind the timeline markers.
    /// </summary>
    /// <param name="context">Drawing context receiving the waveform geometry.</param>
    /// <param name="width">Current control width in pixels.</param>
    /// <param name="trackY">Top y-coordinate of the track area.</param>
    /// <param name="trackHeight">Height of the track area in pixels.</param>
    private void DrawSpeedCurve(DrawingContext context, float width, float trackY, float trackHeight)
    {
        if (_markers.Count < 2)
            return;

        float maxHalfAmplitude = (trackHeight / 2.0f) - 5.0f;
        float centerY = trackY + (trackHeight / 2.0f);
        int sampleCount = Math.Max(2, (int)(width / 2.0f));

        EnsureSpeedCurveSampleCapacity(sampleCount + 1);

        var cachedSpeeds = _speedCurveSamples;
        var spans = _speedCurveSpans;
        spans.Clear();

        int spanStart = -1;

        for (int i = 0; i <= sampleCount; i++)
        {
            float x = width * i / sampleCount;
            float speed = GetNormalizedSpeedAtTime(PixelToTime(x, width));
            cachedSpeeds[i] = speed;

            if (speed >= 0)
            {
                if (spanStart < 0)
                    spanStart = i;
            }
            else if (spanStart >= 0)
            {
                spans.Add((spanStart, i - 1));
                spanStart = -1;
            }
        }

        if (spanStart >= 0)
            spans.Add((spanStart, sampleCount));

        if (spans.Count == 0)
            return;

        var geometry = new StreamGeometry();

        using (var streamContext = geometry.Open())
        {
            foreach (var (start, end) in spans)
                DrawFilledWaveformSpan(streamContext, width, centerY, sampleCount, start, end, maxHalfAmplitude, cachedSpeeds);
        }

        geometry.Freeze();

        context.PushClip(new RectangleGeometry(new Rect(0, trackY, width, trackHeight)));
        context.DrawGeometry(SpeedCurveFillBrush, null, geometry);
        context.Pop();
    }

    /// <summary>
    /// Ensures the reusable speed-curve sample buffer can hold the requested number of entries.
    /// </summary>
    private void EnsureSpeedCurveSampleCapacity(int requiredLength)
    {
        if (_speedCurveSamples.Length < requiredLength)
            _speedCurveSamples = new float[requiredLength];
    }

    /// <summary>
    /// Draws one continuous filled speed span for the waveform.
    /// </summary>
    /// <param name="context">Geometry stream receiving the polygon outline.</param>
    /// <param name="width">Current control width in pixels.</param>
    /// <param name="centerY">Vertical center of the waveform.</param>
    /// <param name="sampleCount">Total number of sampled waveform points.</param>
    /// <param name="start">Inclusive start sample index of the continuous span.</param>
    /// <param name="end">Inclusive end sample index of the continuous span.</param>
    /// <param name="maxHalf">Maximum half-height of the waveform in pixels.</param>
    /// <param name="cachedSpeeds">Cached normalized speed samples for the whole visible range.</param>
    private static void DrawFilledWaveformSpan(StreamGeometryContext context, float width, float centerY,
        int sampleCount, int start, int end, float maxHalf, float[] cachedSpeeds)
    {
        context.BeginFigure(BuildWavePoint(width, centerY, sampleCount, start, maxHalf, cachedSpeeds, upper: true), true, true);

        for (int step = start + 1; step <= end; step++)
            context.LineTo(BuildWavePoint(width, centerY, sampleCount, step, maxHalf, cachedSpeeds, upper: true), true, false);

        for (int step = end; step >= start; step--)
            context.LineTo(BuildWavePoint(width, centerY, sampleCount, step, maxHalf, cachedSpeeds, upper: false), true, false);
    }

    /// <summary>
    /// Builds one point on the mirrored speed waveform.
    /// </summary>
    private static Point BuildWavePoint(float width, float centerY, int sampleCount, int step,
        float maxHalf, float[] cachedSpeeds, bool upper)
    {
        float x = width * step / sampleCount;
        float speed = Math.Max(0.0f, cachedSpeeds[step]);
        float half = Math.Min(maxHalf, Math.Max(1.0f, speed * maxHalf));
        return new Point(x, upper ? centerY - half : centerY + half);
    }

    /// <summary>
    /// Draws all visible timeline markers and reposition ghosts.
    /// </summary>
    /// <param name="context">Drawing context receiving the marker geometry.</param>
    /// <param name="width">Current control width in pixels.</param>
    /// <param name="trackY">Top y-coordinate of the track area.</param>
    /// <param name="trackHeight">Height of the track area in pixels.</param>
    private void DrawMarkers(DrawingContext context, float width, float trackY, float trackHeight)
    {
        float centerY = trackY + (trackHeight / 2.0f);

        for (int i = 0; i < _markers.Count; i++)
        {
            var marker = _markers[i];

            if (!TryGetMarkerPixel(marker, width, out float x))
                continue;

            var fill = GetMarkerFillBrush(marker);
            DrawMarker(context, marker, fill, MarkerOutlinePen, x, centerY);
        }

        if (_interactionMode == InteractionMode.Repositioning)
            DrawGhostMarkers(context, width, centerY);
    }

    /// <summary>
    /// Returns the fill brush for a marker based on its state.
    /// </summary>
    private static Brush GetMarkerFillBrush(FlybyTimelineMarker marker)
    {
        if (marker.IsDuplicate)
            return MarkerErrorBrush;

        if (marker.IsSelected)
            return MarkerSelectedBrush;

        return MarkerBrush;
    }

    /// <summary>
    /// Draws a single marker using the provided outline pen.
    /// </summary>
    /// <param name="context">Drawing context receiving the marker.</param>
    /// <param name="marker">Marker data to render.</param>
    /// <param name="fill">Brush used to fill the marker.</param>
    /// <param name="outlinePen">Pen used to outline the marker.</param>
    /// <param name="x">Horizontal center of the marker.</param>
    /// <param name="centerY">Vertical center of the marker.</param>
    private static void DrawMarker(DrawingContext context, FlybyTimelineMarker marker, Brush fill, Pen outlinePen, float x, float centerY)
    {
        if (marker.HasCameraCut)
        {
            DrawTriangleMarker(context, fill, outlinePen, x, centerY);
            return;
        }

        if (marker.HasFreeze)
        {
            const float half = FlybyConstants.TimelineMarkerRadius;
            context.DrawRectangle(fill, outlinePen, new Rect(x - half, centerY - half, half * 2, half * 2));
            return;
        }

        context.DrawEllipse(fill, outlinePen, new Point(x, centerY), FlybyConstants.TimelineMarkerRadius, FlybyConstants.TimelineMarkerRadius);
    }

    /// <summary>
    /// Creates the triangle geometry used for camera-cut markers.
    /// </summary>
    private static StreamGeometry CreateTriangleMarkerGeometry()
    {
        var geometry = new StreamGeometry();
        const float radius = FlybyConstants.TimelineMarkerRadius;

        using (var streamContext = geometry.Open())
        {
            streamContext.BeginFigure(new Point(radius, 0.0f), true, true);
            streamContext.LineTo(new Point(-radius, -radius), true, false);
            streamContext.LineTo(new Point(-radius, radius), true, false);
        }

        geometry.Freeze();
        return geometry;
    }

    /// <summary>
    /// Draws a triangle marker centered at the provided position.
    /// </summary>
    /// <param name="context">Drawing context receiving the triangle marker.</param>
    /// <param name="fill">Brush used to fill the triangle.</param>
    /// <param name="outlinePen">Pen used to outline the triangle.</param>
    /// <param name="centerX">Horizontal center of the marker.</param>
    /// <param name="centerY">Vertical center of the marker.</param>
    private static void DrawTriangleMarker(DrawingContext context, Brush fill, Pen outlinePen, float centerX, float centerY)
    {
        context.PushTransform(new TranslateTransform(centerX, centerY));
        context.DrawGeometry(fill, outlinePen, CameraCutMarkerGeometry);
        context.Pop();
    }

    /// <summary>
    /// Draws ghost markers used while reordering cameras.
    /// </summary>
    /// <param name="context">Drawing context receiving the ghost markers.</param>
    /// <param name="width">Current control width in pixels.</param>
    /// <param name="centerY">Vertical center used for marker placement.</param>
    private void DrawGhostMarkers(DrawingContext context, float width, float centerY)
    {
        if (_repositionFromIndex < 0 || _repositionFromIndex >= _markers.Count)
            return;

        DrawMarker(context, _markers[_repositionFromIndex], GhostMarkerBrush, GhostMarkerPen, _repositionGhostX, centerY);

        if (_repositionTargetIndex >= 0 && _repositionTargetIndex < _markers.Count &&
            _repositionTargetIndex != _repositionFromIndex)
        {
            if (TryGetMarkerPixel(_markers[_repositionTargetIndex], width, out float targetX) &&
                targetX >= 0.0f && targetX <= width)
            {
                DrawMarker(context, _markers[_repositionTargetIndex], GhostMarkerBrush, GhostMarkerPen, targetX, centerY);
            }
        }
    }

    /// <summary>
    /// Chooses a ruler tick interval based on the current zoom level.
    /// </summary>
    private static float CalculateTickInterval(float pixelsPerSecond)
    {
        foreach (float interval in RulerTickIntervals)
        {
            if (interval * pixelsPerSecond >= FlybyConstants.TimelineMinTickSpacing)
                return interval;
        }

        return RulerTickIntervals[^1];
    }
}
