#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Controls.FlybyTimeline.Sequence;

/// <summary>
/// Represents one cut interval in the generated playback timeline.
/// </summary>
public readonly struct FlybyCutRegion
{
    /// <summary>
    /// Gets the timeline time where the cut region starts.
    /// </summary>
    public float StartTime { get; init; }

    /// <summary>
    /// Gets the timeline time where the cut region ends.
    /// </summary>
    public float EndTime { get; init; }

    /// <summary>
    /// Gets the duration of the cut region in seconds.
    /// </summary>
    public float Duration => EndTime - StartTime;
}

/// <summary>
/// Stores analyzed flyby timing data used by the timeline, preview cache, and speed solving.
/// </summary>
public sealed class FlybySequenceTiming
{
    private readonly float[] _cameraTimes;
    private readonly float[] _segmentDurations;
    private readonly float[] _freezeDurations;
    private readonly float[] _cutBypassDurations;
    private readonly float[] _splineTimeline;
    private readonly FlybyCutRegion[] _cutRegions;
    private readonly ReadOnlyCollection<float> _splineTimelineView;
    private readonly ReadOnlyCollection<FlybyCutRegion> _cutRegionsView;

    private readonly struct SequenceBoundaryInfo
    {
        public int NextCameraIndex { get; init; }
        public bool HasCut { get; init; }
        public bool HasFreeze { get; init; }
    }

    /// <summary>
    /// Gets an empty timing instance with no cameras or playback data.
    /// </summary>
    public static FlybySequenceTiming Empty { get; } = new([], [], [], [], [], [], 0);

    /// <summary>
    /// Gets the total analyzed duration of the sequence in seconds.
    /// </summary>
    public float TotalDuration { get; }

    /// <summary>
    /// Gets the number of cameras represented by this timing data.
    /// </summary>
    public int CameraCount => _cameraTimes.Length;

    /// <summary>
    /// Gets the sampled playback spline timeline used for frame evaluation.
    /// </summary>
    public IReadOnlyList<float> SplineTimeline => _splineTimelineView;

    /// <summary>
    /// Gets the cut regions inserted into the playback timeline.
    /// </summary>
    public IReadOnlyList<FlybyCutRegion> CutRegions => _cutRegionsView;

    private FlybySequenceTiming(float[] cameraTimes, float[] segmentDurations, float[] freezeDurations, float[] cutBypassDurations,
        float[] splineTimeline, FlybyCutRegion[] cutRegions, float totalDuration)
    {
        _cameraTimes = cameraTimes;
        _segmentDurations = segmentDurations;
        _freezeDurations = freezeDurations;
        _cutBypassDurations = cutBypassDurations;
        _splineTimeline = splineTimeline;
        _cutRegions = cutRegions;
        _splineTimelineView = Array.AsReadOnly(_splineTimeline);
        _cutRegionsView = Array.AsReadOnly(_cutRegions);
        TotalDuration = totalDuration;
    }

    /// <summary>
    /// Returns the timeline time of the requested camera.
    /// </summary>
    public float GetCameraTime(int index)
    {
        if (_cameraTimes.Length == 0)
            return 0.0f;

        return _cameraTimes[Math.Clamp(index, 0, _cameraTimes.Length - 1)];
    }

    /// <summary>
    /// Returns the duration of the segment starting at the given camera.
    /// </summary>
    public float GetSegmentDuration(int index)
    {
        if (index < 0 || index >= _segmentDurations.Length)
            return 0.0f;

        return _segmentDurations[index];
    }

    /// <summary>
    /// Returns the freeze duration assigned to the given camera.
    /// </summary>
    public float GetFreezeDuration(int index)
    {
        if (index < 0 || index >= _freezeDurations.Length)
            return 0.0f;

        return _freezeDurations[index];
    }

    /// <summary>
    /// Returns the time span bypassed by a camera cut at the given index.
    /// </summary>
    public float GetCutBypassDuration(int index)
    {
        if (index < 0 || index >= _cutBypassDurations.Length)
            return 0.0f;

        return _cutBypassDurations[index];
    }

    /// <summary>
    /// Maps wall-clock playback time to timeline time, expanding cut regions.
    /// </summary>
    /// <param name="playbackTime">The wall-clock playback time, in seconds.</param>
    /// <returns>The corresponding timeline time, in seconds.</returns>
    public float PlaybackToTimelineTime(float playbackTime)
    {
        float accumulatedCutTime = 0.0f;

        foreach (var cut in _cutRegions)
        {
            float cutPlaybackStart = cut.StartTime - accumulatedCutTime;

            if (playbackTime < cutPlaybackStart)
                break;

            accumulatedCutTime += cut.Duration;
        }

        return playbackTime + accumulatedCutTime;
    }

    /// <summary>
    /// Maps timeline time to wall-clock playback time, collapsing cut regions.
    /// </summary>
    /// <param name="timelineTime">The timeline time, in seconds.</param>
    /// <returns>The corresponding wall-clock playback time, in seconds.</returns>
    public float TimelineToPlaybackTime(float timelineTime)
    {
        float accumulatedCutTime = 0.0f;

        foreach (var cut in _cutRegions)
        {
            if (timelineTime <= cut.StartTime)
                break;

            if (timelineTime < cut.EndTime)
                return cut.StartTime - accumulatedCutTime;

            accumulatedCutTime += cut.Duration;
        }

        return timelineTime - accumulatedCutTime;
    }

    /// <summary>
    /// Builds full timing data for the provided flyby sequence.
    /// </summary>
    /// <param name="cameras">Ordered flyby cameras that define the sequence.</param>
    /// <param name="useSmoothPause">Whether TombEngine smooth-pause behavior should be applied.</param>
    /// <returns>The analyzed timing data for the sequence.</returns>
    public static FlybySequenceTiming Build(IReadOnlyList<FlybyCameraInstance> cameras, bool useSmoothPause)
    {
        if (cameras.Count == 0)
            return Empty;

        var cameraTimes = new float[cameras.Count];
        var segmentDurations = new float[Math.Max(0, cameras.Count - 1)];
        var freezeDurations = BuildFreezeDurations(cameras);
        var cutBypassDurations = new float[cameras.Count];

        if (cameras.Count < 2)
            return new FlybySequenceTiming(cameraTimes, segmentDurations, freezeDurations, cutBypassDurations, [], [], 0);

        float[] speedKnots = BuildSpeedKnots(cameras);
        int numSegments = cameras.Count - 1;
        PopulateCameraTimes(cameras, speedKnots, useSmoothPause, cameraTimes);

        for (int i = 0; i < segmentDurations.Length; i++)
            segmentDurations[i] = Math.Max(0, cameraTimes[i + 1] - cameraTimes[i]);

        for (int i = 0; i < cameras.Count; i++)
        {
            if (!FlybySequenceHelper.TryResolveCutTargetIndex(cameras, i, out int targetIndex))
                continue;

            cutBypassDurations[i] = Math.Max(0, cameraTimes[targetIndex] - cameraTimes[i]);
        }

        float[] splineTimeline = BuildPlaybackTimeline(cameras, speedKnots, numSegments, useSmoothPause, cameraTimes, freezeDurations, out FlybyCutRegion[] cutRegions);
        float totalDuration = Math.Max(0, splineTimeline.Length - 1) * FlybyConstants.TimeStep;

        return new FlybySequenceTiming(cameraTimes, segmentDurations, freezeDurations, cutBypassDurations,
            splineTimeline, cutRegions, totalDuration);
    }

    /// <summary>
    /// Calculates the resulting camera time when one segment speed is overridden.
    /// </summary>
    /// <param name="cameras">Ordered flyby cameras that define the sequence.</param>
    /// <param name="targetIndex">Camera index whose resulting time should be returned.</param>
    /// <param name="useSmoothPause">Whether TombEngine smooth-pause behavior should be applied.</param>
    /// <param name="speedCameraIndex">Camera index whose outgoing speed should be overridden.</param>
    /// <param name="speed">Temporary speed value to test.</param>
    /// <returns>The resulting camera time after applying the temporary speed override.</returns>
    public static float GetCameraTimeForSpeed(IReadOnlyList<FlybyCameraInstance> cameras,
        int targetIndex, bool useSmoothPause, int speedCameraIndex, float speed)
    {
        if (cameras.Count == 0)
            return 0.0f;

        int clampedTargetIndex = Math.Clamp(targetIndex, 0, cameras.Count - 1);

        if (clampedTargetIndex <= 0 || cameras.Count < 2)
            return 0.0f;

        float[] speedKnots = BuildSpeedKnots(cameras, speedCameraIndex, speed);
        var cameraTimes = new float[clampedTargetIndex + 1];
        PopulateCameraTimes(cameras, speedKnots, useSmoothPause, cameraTimes, clampedTargetIndex);
        return cameraTimes[clampedTargetIndex];
    }

    /// <summary>
    /// Converts camera freeze flags into timeline freeze durations.
    /// </summary>
    private static float[] BuildFreezeDurations(IReadOnlyList<FlybyCameraInstance> cameras)
    {
        var freezeDurations = new float[cameras.Count];

        for (int i = 0; i < cameras.Count; i++)
            freezeDurations[i] = FlybySequenceHelper.GetFreezeDuration(cameras[i]);

        return freezeDurations;
    }

    /// <summary>
    /// Builds the padded spline speed knots for the sequence.
    /// </summary>
    /// <param name="cameras">Ordered flyby cameras that define the sequence.</param>
    /// <param name="overrideCameraIndex">Optional camera index whose speed should be overridden.</param>
    /// <param name="overrideSpeed">Temporary speed value to use for the overridden camera.</param>
    /// <returns>The padded spline knot array for speed evaluation.</returns>
    private static float[] BuildSpeedKnots(IReadOnlyList<FlybyCameraInstance> cameras,
        int overrideCameraIndex = -1, float overrideSpeed = 0)
    {
        var rawSpeed = new float[cameras.Count];

        for (int i = 0; i < cameras.Count; i++)
        {
            float speed = i == overrideCameraIndex ? overrideSpeed : cameras[i].Speed;
            rawSpeed[i] = SanitizeSplineSpeed(speed);
        }

        return CatmullRomSpline.PadKnots(rawSpeed);
    }

    /// <summary>
    /// Populates per-camera timeline times from the spline speed profile.
    /// </summary>
    /// <param name="cameras">Ordered flyby cameras that define the sequence.</param>
    /// <param name="speedKnots">Padded spline knots used for speed evaluation.</param>
    /// <param name="useSmoothPause">Whether TombEngine smooth-pause behavior should be applied.</param>
    /// <param name="cameraTimes">Destination array receiving per-camera timeline times.</param>
    /// <param name="lastCameraIndexToPopulate">Optional upper bound for partial population.</param>
    private static void PopulateCameraTimes(IReadOnlyList<FlybyCameraInstance> cameras,
        float[] speedKnots, bool useSmoothPause, float[] cameraTimes, int lastCameraIndexToPopulate = int.MaxValue)
    {
        int numSegments = cameras.Count - 1;
        int emittedSlots = 0;
        float currentSplineT = 0;
        int processedBoundary = 0;
        int lastCameraIndex = Math.Min(lastCameraIndexToPopulate, cameras.Count - 1);

        while (processedBoundary < numSegments)
        {
            var boundary = BuildBoundaryInfo(cameras, processedBoundary);
            int boundarySlots = TraverseBoundary(cameras, speedKnots, numSegments, useSmoothPause,
                boundary, ref currentSplineT, ref emittedSlots);

            cameraTimes[boundary.NextCameraIndex] = boundarySlots * FlybyConstants.TimeStep;

            if (boundary.NextCameraIndex >= lastCameraIndex)
                return;

            processedBoundary = boundary.NextCameraIndex;
        }
    }

    /// <summary>
    /// Builds the playback spline timeline and cut-region metadata.
    /// </summary>
    /// <param name="cameras">Ordered flyby cameras that define the sequence.</param>
    /// <param name="speedKnots">Padded spline knots used for speed evaluation.</param>
    /// <param name="numSegments">Number of spline segments in the sequence.</param>
    /// <param name="useSmoothPause">Whether TombEngine smooth-pause behavior should be applied.</param>
    /// <param name="cameraTimes">Precomputed per-camera timeline times.</param>
    /// <param name="freezeDurations">Precomputed per-camera freeze durations.</param>
    /// <param name="cutRegions">Receives the cut regions inserted into the playback timeline.</param>
    /// <returns>The sampled playback spline timeline.</returns>
    private static float[] BuildPlaybackTimeline(IReadOnlyList<FlybyCameraInstance> cameras,
        float[] speedKnots, int numSegments, bool useSmoothPause, float[] cameraTimes,
        float[] freezeDurations, out FlybyCutRegion[] cutRegions)
    {
        var regions = new List<FlybyCutRegion>();
        int numCameras = cameras.Count;
        var timeline = new List<float>(numCameras * 200);

        float currentSplineT = 0;
        int emittedSlots = 0;
        int processedBoundary = 0;

        while (processedBoundary < numSegments)
        {
            var boundary = BuildBoundaryInfo(cameras, processedBoundary);
            TraverseBoundary(cameras, speedKnots, numSegments, useSmoothPause,
                boundary, ref currentSplineT, ref emittedSlots, timeline.Add);

            processedBoundary = boundary.NextCameraIndex;

            if (!boundary.HasCut || boundary.NextCameraIndex >= numCameras)
                continue;

            if (!FlybySequenceHelper.TryResolveCutTargetIndex(cameras, boundary.NextCameraIndex, out int targetCameraIndex))
                continue;

            if (targetCameraIndex <= boundary.NextCameraIndex || targetCameraIndex > numSegments)
                continue;

            float bypassedTime = Math.Max(0.0f, cameraTimes[targetCameraIndex] - cameraTimes[boundary.NextCameraIndex]);

            float cutStartTime = timeline.Count * FlybyConstants.TimeStep;
            float targetSplineT = targetCameraIndex;
            int bypassSlots = GetPauseSlotCountFromSeconds(bypassedTime);

            if (bypassSlots <= 0)
                bypassSlots = 1;

            EmitHoldSamples(targetSplineT, bypassSlots, ref emittedSlots, timeline.Add);

            regions.Add(new FlybyCutRegion
            {
                StartTime = cutStartTime,
                EndTime = cutStartTime + (bypassSlots * FlybyConstants.TimeStep)
            });

            processedBoundary = targetCameraIndex;
            currentSplineT = targetCameraIndex;

            if (freezeDurations[targetCameraIndex] > 0)
            {
                int freezeSlots = GetPauseSlotCountFromSeconds(freezeDurations[targetCameraIndex]);
                EmitHoldSamples(targetSplineT, freezeSlots, ref emittedSlots, timeline.Add);

                if (useSmoothPause && targetCameraIndex < numSegments)
                    EmitEaseIn(speedKnots, numSegments, ref currentSplineT, ref emittedSlots, timeline.Add);
            }
        }

        timeline.Add(Math.Min(currentSplineT, numSegments));
        cutRegions = [.. regions];
        return [.. timeline];
    }

    /// <summary>
    /// Builds the boundary metadata for the next camera transition.
    /// </summary>
    private static SequenceBoundaryInfo BuildBoundaryInfo(IReadOnlyList<FlybyCameraInstance> cameras, int processedBoundary)
    {
        int nextCameraIndex = processedBoundary + 1;
        ushort nextFlags = cameras[nextCameraIndex].Flags;
        short nextTimer = cameras[nextCameraIndex].Timer;
        bool hasCut = (nextFlags & FlybyConstants.FlagCameraCut) != 0;

        return new SequenceBoundaryInfo
        {
            NextCameraIndex = nextCameraIndex,
            HasCut = hasCut,
            HasFreeze = !hasCut && (nextFlags & FlybyConstants.FlagFreezeCamera) != 0 && nextTimer > 0,
        };
    }

    /// <summary>
    /// Traverses one sequence boundary, handling the shared freeze and easing behavior.
    /// </summary>
    private static int TraverseBoundary(IReadOnlyList<FlybyCameraInstance> cameras,
        float[] speedKnots, int numSegments, bool useSmoothPause, SequenceBoundaryInfo boundary,
        ref float currentSplineT, ref int emittedSlots, Action<float>? emitSample = null)
    {
        float boundarySplineT = boundary.NextCameraIndex;

        if (useSmoothPause && boundary.HasFreeze)
        {
            float easeOutStartT = boundarySplineT - FlybyConstants.FreezeEaseDistance;
            AdvanceToTarget(speedKnots, numSegments, easeOutStartT, ref currentSplineT, ref emittedSlots, emitSample);
            EmitEaseOut(speedKnots, numSegments, boundarySplineT, ref currentSplineT, ref emittedSlots, emitSample);
            currentSplineT = boundarySplineT;

            int boundarySlots = emittedSlots;
            int holdSlots = GetPauseSlotCountFromFrames(cameras[boundary.NextCameraIndex].TimerToFrames);
            EmitHoldSamples(currentSplineT, holdSlots, ref emittedSlots, emitSample);

            if (boundary.NextCameraIndex < numSegments)
                EmitEaseIn(speedKnots, numSegments, ref currentSplineT, ref emittedSlots, emitSample);

            return boundarySlots;
        }

        AdvanceToTarget(speedKnots, numSegments, boundarySplineT, ref currentSplineT, ref emittedSlots, emitSample);
        currentSplineT = boundarySplineT;

        int boundarySlotCount = emittedSlots;

        if (boundary.HasFreeze)
        {
            int holdSlots = GetPauseSlotCountFromFrames(cameras[boundary.NextCameraIndex].TimerToFrames);
            EmitHoldSamples(currentSplineT, holdSlots, ref emittedSlots, emitSample);
        }

        return boundarySlotCount;
    }

    /// <summary>
    /// Emits repeated hold samples at a fixed spline position.
    /// </summary>
    /// <param name="splineT">Spline position to emit for every hold sample.</param>
    /// <param name="slotCount">Number of timeline slots to emit at the fixed spline position.</param>
    /// <param name="emittedSlots">Total emitted slot count, updated in place.</param>
    /// <param name="emitSample">Optional callback that receives each emitted spline sample.</param>
    private static void EmitHoldSamples(float splineT, int slotCount, ref int emittedSlots, Action<float>? emitSample = null)
    {
        for (int i = 0; i < slotCount; i++)
        {
            emitSample?.Invoke(splineT);
            emittedSlots++;
        }
    }

    /// <summary>
    /// Advances along the spline until the requested boundary is reached.
    /// </summary>
    /// <param name="speedKnots">Padded spline knots used for speed evaluation.</param>
    /// <param name="numSegments">Number of spline segments in the sequence.</param>
    /// <param name="targetT">Spline position to advance toward.</param>
    /// <param name="currentT">Current spline position, updated in place.</param>
    /// <param name="emittedSlots">Generated slot count, updated in place.</param>
    /// <param name="emitSample">Optional callback that receives each emitted spline sample.</param>
    private static void AdvanceToTarget(float[] speedKnots, int numSegments, float targetT,
        ref float currentT, ref int emittedSlots, Action<float>? emitSample = null)
    {
        const float tickFactor = FlybyConstants.SpeedScale * FlybyConstants.TimeStep;

        while (currentT < targetT)
        {
            emitSample?.Invoke(currentT);
            emittedSlots++;
            float speed = GetClampedSplineSpeed(currentT, speedKnots, numSegments);
            float nextT = currentT + (Math.Max(speed, FlybyConstants.MinSpeed) * tickFactor);

            if (!(nextT > currentT))
            {
                currentT = targetT;
                break;
            }

            currentT = nextT;
        }

        currentT = Math.Min(currentT, targetT);
    }

    /// <summary>
    /// Emits a smooth ease-out segment before a freeze pause.
    /// </summary>
    /// <param name="speedKnots">Padded spline knots used for speed evaluation.</param>
    /// <param name="numSegments">Number of spline segments in the sequence.</param>
    /// <param name="boundaryT">Spline boundary where the pause begins.</param>
    /// <param name="currentT">Current spline position, updated in place.</param>
    /// <param name="emittedSlots">Generated slot count, updated in place.</param>
    /// <param name="emitSample">Optional callback that receives each emitted spline sample.</param>
    private static void EmitEaseOut(float[] speedKnots, int numSegments, float boundaryT,
        ref float currentT, ref int emittedSlots, Action<float>? emitSample = null)
    {
        float easeStartT = currentT;
        float remainingDist = Math.Max(boundaryT - easeStartT, FlybyConstants.MinSpeed);
        float speed = GetClampedSplineSpeed(easeStartT, speedKnots, numSegments);
        float speedPerSec = Math.Max(speed, FlybyConstants.MinSpeed) * FlybyConstants.SpeedScale;
        float easeStep = speedPerSec / (2.0f * remainingDist);
        float easeProgress = 0;

        while (easeProgress < 1.0f)
        {
            float nextEaseProgress = Math.Min(easeProgress + (easeStep * FlybyConstants.TimeStep), 1.0f);

            if (!(nextEaseProgress > easeProgress))
            {
                currentT = boundaryT;
                emitSample?.Invoke(boundaryT);
                emittedSlots++;
                break;
            }

            easeProgress = nextEaseProgress;
            currentT = easeStartT + (remainingDist * easeProgress * (2.0f - easeProgress));
            emitSample?.Invoke(Math.Min(currentT, boundaryT));
            emittedSlots++;
        }
    }

    /// <summary>
    /// Emits a smooth ease-in segment after a freeze pause.
    /// </summary>
    /// <param name="speedKnots">Padded spline knots used for speed evaluation.</param>
    /// <param name="numSegments">Number of spline segments in the sequence.</param>
    /// <param name="currentT">Current spline position, updated in place.</param>
    /// <param name="emittedSlots">Generated slot count, updated in place.</param>
    /// <param name="emitSample">Optional callback that receives each emitted spline sample.</param>
    private static void EmitEaseIn(float[] speedKnots, int numSegments,
        ref float currentT, ref int emittedSlots, Action<float>? emitSample = null)
    {
        float speed = GetClampedSplineSpeed(currentT, speedKnots, numSegments);
        float speedPerSec = Math.Max(speed, FlybyConstants.MinSpeed) * FlybyConstants.SpeedScale;
        float easeInStep = speedPerSec / (2.0f * FlybyConstants.FreezeEaseDistance);
        float easeInProgress = 0;

        while (easeInProgress < 1.0f)
        {
            float nextEaseInProgress = Math.Min(easeInProgress + (easeInStep * FlybyConstants.TimeStep), 1.0f);

            if (!(nextEaseInProgress > easeInProgress))
                break;

            easeInProgress = nextEaseInProgress;
            float speedFactor = easeInProgress * easeInProgress;
            float nextT = currentT + (speedPerSec * speedFactor * FlybyConstants.TimeStep);

            if (!(nextT > currentT))
                break;

            currentT = nextT;
            emitSample?.Invoke(currentT);
            emittedSlots++;
        }
    }

    /// <summary>
    /// Evaluates and clamps spline speed to the current segment bounds.
    /// </summary>
    /// <param name="t">Spline position to evaluate.</param>
    /// <param name="speedKnots">Padded spline knots used for speed evaluation.</param>
    /// <param name="numSegments">Number of spline segments in the sequence.</param>
    /// <returns>The clamped spline speed for the requested position.</returns>
    private static float GetClampedSplineSpeed(float t, float[] speedKnots, int numSegments)
    {
        float clampedT = Math.Clamp(t, 0.0f, numSegments);
        float speed = SanitizeSplineSpeed(CatmullRomSpline.Evaluate(clampedT, speedKnots));

        int span = Math.Min((int)clampedT, numSegments - 1);

        if (span < 0)
            return speed;

        float p1 = SanitizeSplineSpeed(speedKnots[span + 1]);
        float p2 = SanitizeSplineSpeed(speedKnots[span + 2]);
        float minSpeed = Math.Min(p1, p2);
        float maxSpeed = Math.Max(p1, p2);

        return Math.Clamp(speed, minSpeed, maxSpeed);
    }

    /// <summary>
    /// Converts a pause duration in animation frames into timeline slots.
    /// </summary>
    /// <param name="frameCount">Pause duration expressed in engine animation frames.</param>
    /// <returns>The corresponding number of fixed timeline slots.</returns>
    private static int GetPauseSlotCountFromFrames(int frameCount)
        => GetPauseSlotCountFromSeconds(Math.Max(0, frameCount) / FlybyConstants.TickRate);

    /// <summary>
    /// Converts a pause duration in seconds into timeline slots.
    /// </summary>
    /// <param name="durationSeconds">Pause duration in seconds.</param>
    /// <returns>The corresponding number of fixed timeline slots.</returns>
    private static int GetPauseSlotCountFromSeconds(float durationSeconds)
    {
        if (!float.IsFinite(durationSeconds))
            return 0;

        float clampedDuration = Math.Max(0.0f, durationSeconds);
        return Math.Max(0, (int)MathF.Round(clampedDuration / FlybyConstants.TimeStep));
    }

    private static float SanitizeSplineSpeed(float speed)
    {
        if (!float.IsFinite(speed))
            return FlybyConstants.MinSpeed;

        return Math.Max(speed, FlybyConstants.MinSpeed);
    }
}
