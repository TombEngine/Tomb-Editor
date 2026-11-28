#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using TombEditor.Features.FlybyTimeline.Preview;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Features.FlybyTimeline.Sequence;

/// <summary>
/// <para>
/// Pre-calculates a flyby sequence into a frame array at fixed time resolution.
/// Both timeline scrubbing and playback sample from this cache via linear interpolation,
/// eliminating real-time state tracking for freeze, cut, and smooth pause flags.
/// </para>
/// <para>
/// Sequence timing analysis, spline playback timeline, and cut regions come from
/// <see cref="FlybySequenceTiming"/>. The cache only evaluates spline channels in
/// parallel and post-processes the resulting frames for interpolation.
/// </para>
/// </summary>
public sealed class FlybySequenceCache
{
    private const float InvalidSpeed = -1.0f;

    /// <summary>
    /// Stores the non-null camera data needed to build spline knots.
    /// </summary>
    private readonly struct CameraSplineData
    {
        public Vector3 WorldPosition { get; init; }
        public float RotationY { get; init; }
        public float RotationX { get; init; }
        public float Roll { get; init; }
        public float Fov { get; init; }
        public float DofDistance { get; init; }
        public float DofRange { get; init; }
        public float DofStrength { get; init; }
        public DofMode DofMode { get; init; }
    }

    /// <summary>
    /// Stores one contiguous playback spline segment. Segments are split at camera cuts
    /// so post-cut interpolation starts from a fresh local knot set.
    /// </summary>
    private readonly struct SplineSegment
    {
        public int StartCameraIndex { get; init; }
        public int EndCameraIndex { get; init; }
        public int NumSegments { get; init; }
        public CameraSplineData SingleCamera { get; init; }
        public float[] PosX { get; init; }
        public float[] PosY { get; init; }
        public float[] PosZ { get; init; }
        public float[] TgtX { get; init; }
        public float[] TgtY { get; init; }
        public float[] TgtZ { get; init; }
        public float[] RollKnots { get; init; }
        public float[] FovKnots { get; init; }
        public float[] DofDistanceKnots { get; init; }
        public float[] DofRangeKnots { get; init; }
        public float[] DofStrengthKnots { get; init; }
        public DofMode[] DofModes { get; init; }

        public bool IsSingleCamera => NumSegments == 0;
    }

    private readonly FlybyFrameState[] _frames;
    private readonly FlybyCutRegion[] _cutRegions;
    private readonly bool[] _framesInsideCutRegion;
    private readonly bool[] _cutBoundaryFrames;
    private readonly float[] _smoothedSpeeds;

    /// <summary>
    /// Gets the analyzed timing data used to build this cache.
    /// </summary>
    public FlybySequenceTiming Timing { get; }

    /// <summary>
    /// Gets the total duration of the cached sequence in seconds.
    /// </summary>
    public float TotalDuration => Timing.TotalDuration;

    /// <summary>
    /// Gets whether the cache contains enough data for interpolation.
    /// </summary>
    public bool IsValid => _frames.Length > 0;

    /// <summary>
    /// Gets the peak precomputed speed used to normalize the timeline speed curve.
    /// </summary>
    public float PeakSpeed { get; }

    /// <summary>
    /// Builds a new sequence cache for the provided cameras.
    /// </summary>
    /// <param name="cameras">The flyby cameras to cache.</param>
    /// <param name="useSmoothPause">Whether TombEngine smooth-pause behavior should be applied.</param>
    /// <returns>The pre-calculated flyby sequence cache.</returns>
    public static FlybySequenceCache Build(IReadOnlyList<FlybyCameraInstance> cameras, bool useSmoothPause)
        => new(cameras, useSmoothPause);

    /// <summary>
    /// Returns an empty cache with no frame data.
    /// </summary>
    public static FlybySequenceCache Empty { get; } = new([], false);

    private FlybySequenceCache(IReadOnlyList<FlybyCameraInstance> cameras, bool useSmoothPause)
    {
        var validCameras = new List<FlybyCameraInstance>(cameras.Count);
        var splineData = new List<CameraSplineData>(cameras.Count);

        foreach (var camera in cameras)
        {
            var room = camera.Room;

            if (room is null)
                continue;

            validCameras.Add(camera);
            splineData.Add(new CameraSplineData
            {
                WorldPosition = camera.Position + room.WorldPos,
                RotationY = camera.RotationY,
                RotationX = camera.RotationX,
                Roll = camera.Roll,
                Fov = camera.Fov,
                DofDistance = camera.DofDistance,
                DofRange = camera.DofRange,
                DofStrength = camera.DofStrength,
                DofMode = camera.DofMode
            });
        }

        Timing = FlybySequenceTiming.Build(validCameras, useSmoothPause);

        if (validCameras.Count < 2)
        {
            _frames = [];
            _cutRegions = [];
            _framesInsideCutRegion = [];
            _cutBoundaryFrames = [];
            _smoothedSpeeds = [];
            PeakSpeed = 0.0f;
            return;
        }

        // Build playback spline sections with cut-aware tangent resets.
        SplineSegment[] splineSegments = BuildSplineSegments(validCameras, splineData);

        // Precomputed spline timeline and cut regions come from FlybySequenceTiming.
        float[] splineParams = [.. Timing.SplineTimeline];

        _cutRegions = [.. Timing.CutRegions];

        // Evaluate all spline channels in parallel.
        _frames = EvaluateFramesParallel(splineParams, splineSegments);

        // Unwrap yaw/pitch/roll to prevent discontinuities from atan2 wrapping.
        UnwrapFrameAngles(_frames);

        BuildCutLookupTables(_cutRegions, _frames.Length, out _framesInsideCutRegion, out _cutBoundaryFrames);

        // Pre-compute smoothed speed curve.
        _smoothedSpeeds = ComputeSmoothedSpeeds();
        PeakSpeed = ComputePeakSpeed(_smoothedSpeeds);
    }

    /// <summary>
    /// Samples the cache at a given time, linearly interpolating between adjacent frames.
    /// Interpolation is suppressed at cut region boundaries to prevent blending between
    /// valid sequence frames and dummy still frames filling the cut gap.
    /// </summary>
    /// <param name="timeSeconds">The timeline time, in seconds, to sample.</param>
    /// <returns>The sampled preview frame, or <see langword="default"/> when the cache is invalid.</returns>
    public FlybyFrameState SampleAtTime(float timeSeconds)
    {
        if (!IsValid)
            return default;

        if (float.IsNaN(timeSeconds) || float.IsNegativeInfinity(timeSeconds))
            return _frames[0];

        if (float.IsPositiveInfinity(timeSeconds))
            return _frames[^1];

        if (timeSeconds <= 0.0f)
            return _frames[0];

        float index = timeSeconds / FlybyConstants.TimeStep;
        int frameIndex = (int)MathF.Floor(index);
        float interpolationFactor = index - frameIndex;

        if (frameIndex >= _frames.Length - 1)
            return _frames[^1];

        // Suppress interpolation across cut region boundaries.
        if (interpolationFactor > 0.0f && IsAtCutBoundary(frameIndex))
            return _frames[IsInsideCutRegion(frameIndex) ? frameIndex + 1 : frameIndex];

        return LerpFrames(_frames[frameIndex], _frames[frameIndex + 1], interpolationFactor);
    }

    /// <summary>
    /// Returns the pre-computed smoothed speed (world units per second) at the given timeline time.
    /// Returns <c>-1.0f</c> if the time is outside the sequence, within a cut region,
    /// or the cache is not valid.
    /// </summary>
    /// <param name="timeSeconds">The timeline time, in seconds, to evaluate.</param>
    /// <returns>The smoothed speed in world units per second, or <c>-1.0f</c> when no speed should be shown.</returns>
    public float GetSpeedAtTime(float timeSeconds)
    {
        if (_smoothedSpeeds.Length == 0)
            return InvalidSpeed;

        if (!float.IsFinite(timeSeconds))
            return InvalidSpeed;

        if (timeSeconds < 0.0f || timeSeconds > TotalDuration)
            return InvalidSpeed;

        float index = timeSeconds / FlybyConstants.TimeStep;
        int i0 = Math.Clamp((int)index, 0, _smoothedSpeeds.Length - 1);

        if (IsInsideCutRegion(i0))
            return InvalidSpeed;

        int i1 = Math.Min(i0 + 1, _smoothedSpeeds.Length - 1);
        float frac = index - (int)index;

        return _smoothedSpeeds[i0] + ((_smoothedSpeeds[i1] - _smoothedSpeeds[i0]) * frac);
    }

    /// <summary>
    /// Interpolates two frames into a blended frame.
    /// </summary>
    /// <param name="a">Frame sampled at the lower timeline slot.</param>
    /// <param name="b">Frame sampled at the upper timeline slot.</param>
    /// <param name="t">Interpolation factor between <paramref name="a"/> and <paramref name="b"/>.</param>
    /// <returns>The interpolated preview frame.</returns>
    private static FlybyFrameState LerpFrames(FlybyFrameState a, FlybyFrameState b, float t) => new()
    {
        Position = Vector3.Lerp(a.Position, b.Position, t),
        RotationY = a.RotationY + ((b.RotationY - a.RotationY) * t),
        RotationX = a.RotationX + ((b.RotationX - a.RotationX) * t),
        Roll = a.Roll + ((b.Roll - a.Roll) * t),
        Fov = a.Fov + ((b.Fov - a.Fov) * t),
        DofDistance = a.DofDistance + ((b.DofDistance - a.DofDistance) * t),
        DofRange = a.DofRange + ((b.DofRange - a.DofRange) * t),
        DofStrength = a.DofStrength + ((b.DofStrength - a.DofStrength) * t),
        DofMode = t < 0.5f ? a.DofMode : b.DofMode
	};

    /// <summary>
    /// Returns whether the given frame index lies inside a cut region.
    /// </summary>
    private bool IsInsideCutRegion(int frameIndex)
        => frameIndex >= 0 && frameIndex < _framesInsideCutRegion.Length && _framesInsideCutRegion[frameIndex];

    /// <summary>
    /// Returns whether a frame pair crosses a cut boundary.
    /// </summary>
    private bool IsAtCutBoundary(int frameIndex)
        => frameIndex >= 0 && frameIndex < _cutBoundaryFrames.Length && _cutBoundaryFrames[frameIndex];

    /// <summary>
    /// Builds per-frame cut membership and boundary lookup tables for fast sampling.
    /// </summary>
    /// <param name="cutRegions">Cut regions present in the analyzed sequence timing.</param>
    /// <param name="frameCount">Number of cached timeline frames.</param>
    /// <param name="framesInsideCutRegion">Receives a lookup indicating whether each frame lies inside a cut hold region.</param>
    /// <param name="cutBoundaryFrames">Receives a lookup indicating whether a frame pair crosses a cut boundary.</param>
    private static void BuildCutLookupTables(IReadOnlyList<FlybyCutRegion> cutRegions, int frameCount,
        out bool[] framesInsideCutRegion, out bool[] cutBoundaryFrames)
    {
        framesInsideCutRegion = new bool[frameCount];
        cutBoundaryFrames = new bool[Math.Max(0, frameCount - 1)];

        foreach (var cut in cutRegions)
        {
            int startFrame = TimeToFrameIndex(cut.StartTime, frameCount);
            int endFrame = TimeToFrameIndex(cut.EndTime, frameCount);

            for (int frameIndex = startFrame; frameIndex < endFrame && frameIndex < frameCount; frameIndex++)
                framesInsideCutRegion[frameIndex] = true;

            if (startFrame > 0 && startFrame - 1 < cutBoundaryFrames.Length)
                cutBoundaryFrames[startFrame - 1] = true;

            if (endFrame > 0 && endFrame - 1 < cutBoundaryFrames.Length)
                cutBoundaryFrames[endFrame - 1] = true;
        }
    }

    /// <summary>
    /// Converts a cut-boundary time aligned to the cache step into a clamped frame index.
    /// </summary>
    /// <param name="timeSeconds">Cut-boundary time expressed in seconds.</param>
    /// <param name="frameCount">Number of cached frames available for sampling.</param>
    /// <returns>The nearest valid cached frame index for the provided time.</returns>
    private static int TimeToFrameIndex(float timeSeconds, int frameCount)
    {
        if (frameCount <= 0 || !float.IsFinite(timeSeconds))
            return 0;

        int frameIndex = (int)MathF.Round(timeSeconds / FlybyConstants.TimeStep);
        return Math.Clamp(frameIndex, 0, frameCount - 1);
    }

    #region Parallel spline evaluation

    /// <summary>
    /// Builds all cached frames by evaluating spline parameters in parallel.
    /// </summary>
    /// <param name="splineParams">Playback spline parameters sampled at fixed timeline steps.</param>
    /// <param name="splineSegments">Cut-aware spline segments used for frame evaluation.</param>
    /// <returns>The fully evaluated cached frame array.</returns>
    private static FlybyFrameState[] EvaluateFramesParallel(float[] splineParams, SplineSegment[] splineSegments)
    {
        int count = splineParams.Length;
        var result = new FlybyFrameState[count];

        Parallel.For(0, count, i => result[i] = EvaluateSplineFrame(splineParams[i], splineSegments));
        return result;
    }

    /// <summary>
    /// Evaluates all spline channels at parameter t and returns a frame.
    /// Pure function of the knot arrays, safe for parallel invocation.
    /// </summary>
    /// <param name="t">Spline parameter to evaluate.</param>
    /// <param name="splineSegments">Cut-aware spline segments used for frame evaluation.</param>
    /// <returns>The evaluated frame for the requested spline position.</returns>
    private static FlybyFrameState EvaluateSplineFrame(float t, SplineSegment[] splineSegments)
    {
        var segment = ResolveSplineSegment(t, splineSegments);

        if (segment.IsSingleCamera)
            return BuildCameraFrame(segment.SingleCamera);

        float localT = Math.Clamp(t - segment.StartCameraIndex, 0.0f, segment.NumSegments);

        float px = CatmullRomSpline.Evaluate(localT, segment.PosX);
        float py = CatmullRomSpline.Evaluate(localT, segment.PosY);
        float pz = CatmullRomSpline.Evaluate(localT, segment.PosZ);
        float tx = CatmullRomSpline.Evaluate(localT, segment.TgtX);
        float ty = CatmullRomSpline.Evaluate(localT, segment.TgtY);
        float tz = CatmullRomSpline.Evaluate(localT, segment.TgtZ);
        float roll = CatmullRomSpline.Evaluate(localT, segment.RollKnots);
        float fov = CatmullRomSpline.Evaluate(localT, segment.FovKnots);
        float dofDistance = CatmullRomSpline.Evaluate(localT, segment.DofDistanceKnots);
        float dofRange = CatmullRomSpline.Evaluate(localT, segment.DofRangeKnots);
        float dofStrength = CatmullRomSpline.Evaluate(localT, segment.DofStrengthKnots);
        int dofModeIndex = Math.Clamp((int)MathF.Round(localT), 0, segment.DofModes.Length - 1);
        var dofMode = segment.DofModes[dofModeIndex];

        float dx = tx - px;
        float dy = ty - py;
        float dz = tz - pz;
        float horizontalDist = MathF.Sqrt((dx * dx) + (dz * dz));

        float yaw = 0.0f;
        float pitch = 0.0f;

        if (horizontalDist > FlybyConstants.RotationSolveDistanceEpsilon || MathF.Abs(dy) > FlybyConstants.RotationSolveDistanceEpsilon)
        {
            yaw = MathF.Atan2(dx, dz);
            pitch = MathF.Atan2(-dy, horizontalDist);
        }

        return new FlybyFrameState
        {
            Position = new Vector3(px, py, pz),
            RotationY = yaw,
            RotationX = pitch,
            Roll = MathC.DegToRad(roll),
            Fov = MathC.DegToRad(fov),
            DofDistance = dofDistance,
            DofRange = dofRange,
            DofStrength = dofStrength,
			DofMode = dofMode
        };
    }

    /// <summary>
    /// Resolves the active spline segment for a global playback parameter.
    /// </summary>
    private static SplineSegment ResolveSplineSegment(float t, IReadOnlyList<SplineSegment> splineSegments)
    {
        if (splineSegments.Count == 0)
            throw new InvalidOperationException("At least one spline segment is required.");

        int low = 0;
        int high = splineSegments.Count - 1;

        while (low < high)
        {
            int mid = (low + high + 1) >> 1;

            if (t >= splineSegments[mid].StartCameraIndex)
                low = mid;
            else
                high = mid - 1;
        }

        return splineSegments[low];
    }

    /// <summary>
    /// Converts one camera sample directly into a frame.
    /// </summary>
    private static FlybyFrameState BuildCameraFrame(CameraSplineData camera)
    {
        return FlybyFrameState.FromDegrees(
            camera.WorldPosition, camera.RotationY, camera.RotationX, camera.Roll, camera.Fov,
            camera.DofDistance, camera.DofRange, camera.DofStrength, camera.DofMode);
    }

    #endregion Parallel spline evaluation

    #region Knot building

    /// <summary>
    /// Splits playback into cut-aware spline segments.
    /// </summary>
    private static SplineSegment[] BuildSplineSegments(IReadOnlyList<FlybyCameraInstance> cameras, IReadOnlyList<CameraSplineData> splineData)
    {
        var segments = new List<SplineSegment>();
        int segmentStart = 0;
        int processedBoundary = 0;
        int lastCameraIndex = cameras.Count - 1;

        while (processedBoundary < lastCameraIndex)
        {
            int nextCameraIndex = processedBoundary + 1;
            processedBoundary = nextCameraIndex;

            if (!FlybySequenceHelper.TryResolveCutTargetIndex(cameras, nextCameraIndex, out int targetIndex) || targetIndex <= nextCameraIndex)
                continue;

            segments.Add(CreateSplineSegment(segmentStart, nextCameraIndex, splineData));
            segmentStart = targetIndex;
            processedBoundary = targetIndex;
        }

        segments.Add(CreateSplineSegment(segmentStart, lastCameraIndex, splineData));
        return [.. segments];
    }

    /// <summary>
    /// Builds one playback spline segment from the requested camera range.
    /// </summary>
    private static SplineSegment CreateSplineSegment(int startIndex, int endIndex, IReadOnlyList<CameraSplineData> splineData)
    {
        int cameraCount = (endIndex - startIndex) + 1;

        if (cameraCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(startIndex), "Spline segment must contain at least one camera.");

        if (cameraCount == 1)
        {
            return new SplineSegment
            {
                StartCameraIndex = startIndex,
                EndCameraIndex = endIndex,
                NumSegments = 0,
                SingleCamera = splineData[startIndex],
                PosX = [],
                PosY = [],
                PosZ = [],
                TgtX = [],
                TgtY = [],
                TgtZ = [],
                RollKnots = [],
                FovKnots = [],
                DofDistanceKnots = [],
                DofRangeKnots = [],
                DofStrengthKnots = [],
                DofModes = []
            };
        }

        BuildKnotArrays(splineData, startIndex, cameraCount,
            out float[] posX, out float[] posY, out float[] posZ,
            out float[] tgtX, out float[] tgtY, out float[] tgtZ,
            out float[] rollKnots, out float[] fovKnots,
            out float[] dofDistanceKnots, out float[] dofRangeKnots, out float[] dofStrengthKnots,
            out DofMode[] dofModes);

        return new SplineSegment
        {
            StartCameraIndex = startIndex,
            EndCameraIndex = endIndex,
            NumSegments = cameraCount - 1,
            SingleCamera = splineData[startIndex],
            PosX = posX,
            PosY = posY,
            PosZ = posZ,
            TgtX = tgtX,
            TgtY = tgtY,
            TgtZ = tgtZ,
            RollKnots = rollKnots,
            FovKnots = fovKnots,
            DofDistanceKnots = dofDistanceKnots,
            DofRangeKnots = dofRangeKnots,
            DofStrengthKnots = dofStrengthKnots,
            DofModes = dofModes
        };
    }

    /// <summary>
    /// Builds all spline knot arrays needed for cache frame evaluation.
    /// </summary>
    /// <param name="splineData">Ordered camera samples that define the sequence.</param>
    /// <param name="startIndex">Start camera index of the segment to convert.</param>
    /// <param name="cameraCount">Number of cameras to include in the segment.</param>
    /// <param name="posX">Receives padded X-position spline knots.</param>
    /// <param name="posY">Receives padded Y-position spline knots.</param>
    /// <param name="posZ">Receives padded Z-position spline knots.</param>
    /// <param name="tgtX">Receives padded X target-position spline knots.</param>
    /// <param name="tgtY">Receives padded Y target-position spline knots.</param>
    /// <param name="tgtZ">Receives padded Z target-position spline knots.</param>
    /// <param name="rollKnots">Receives padded roll spline knots in degrees.</param>
    /// <param name="fovKnots">Receives padded field-of-view spline knots in degrees.</param>
    /// <param name="dofDistanceKnots">Receives padded focus-distance spline knots in world units.</param>
    /// <param name="dofRangeKnots">Receives padded focus-range spline knots in world units.</param>
    /// <param name="dofStrengthKnots">Receives padded focus-strength spline knots.</param>
    /// <param name="dofModes">Receives the discrete DOF mode at each camera knot.</param>
    private static void BuildKnotArrays(
        IReadOnlyList<CameraSplineData> splineData,
        int startIndex,
        int cameraCount,
        out float[] posX, out float[] posY, out float[] posZ,
        out float[] tgtX, out float[] tgtY, out float[] tgtZ,
        out float[] rollKnots, out float[] fovKnots,
        out float[] dofDistanceKnots, out float[] dofRangeKnots, out float[] dofStrengthKnots,
        out DofMode[] dofModes)
    {
        var rawPosX = new float[cameraCount];
        var rawPosY = new float[cameraCount];
        var rawPosZ = new float[cameraCount];
        var rawTgtX = new float[cameraCount];
        var rawTgtY = new float[cameraCount];
        var rawTgtZ = new float[cameraCount];
        var rawRoll = new float[cameraCount];
        var rawFov = new float[cameraCount];
        var rawDofDistance = new float[cameraCount];
        var rawDofRange = new float[cameraCount];
        var rawDofStrength = new float[cameraCount];
        dofModes = new DofMode[cameraCount];

        for (int i = 0; i < cameraCount; i++)
        {
            var camera = splineData[startIndex + i];
            var target = BuildTargetPoint(camera);

            rawPosX[i] = camera.WorldPosition.X;
            rawPosY[i] = camera.WorldPosition.Y;
            rawPosZ[i] = camera.WorldPosition.Z;
            rawTgtX[i] = target.X;
            rawTgtY[i] = target.Y;
            rawTgtZ[i] = target.Z;
            rawRoll[i] = camera.Roll;
            rawFov[i] = camera.Fov;
            rawDofDistance[i] = camera.DofDistance;
            rawDofRange[i] = camera.DofRange;
            rawDofStrength[i] = camera.DofStrength;
            dofModes[i] = camera.DofMode;
        }

        UnwrapAngles(rawRoll);

        posX = CatmullRomSpline.PadKnots(rawPosX);
        posY = CatmullRomSpline.PadKnots(rawPosY);
        posZ = CatmullRomSpline.PadKnots(rawPosZ);
        tgtX = CatmullRomSpline.PadKnots(rawTgtX);
        tgtY = CatmullRomSpline.PadKnots(rawTgtY);
        tgtZ = CatmullRomSpline.PadKnots(rawTgtZ);
        rollKnots = CatmullRomSpline.PadKnots(rawRoll);
        fovKnots = CatmullRomSpline.PadKnots(rawFov);
        dofDistanceKnots = CatmullRomSpline.PadKnots(rawDofDistance);
        dofRangeKnots = CatmullRomSpline.PadKnots(rawDofRange);
        dofStrengthKnots = CatmullRomSpline.PadKnots(rawDofStrength);
    }

    /// <summary>
    /// Converts flyby rotation data into a synthetic target point used for spline interpolation.
    /// </summary>
    private static Vector3 BuildTargetPoint(CameraSplineData camera)
    {
        float yawRad = MathC.DegToRad(camera.RotationY);
        float pitchRad = MathC.DegToRad(camera.RotationX);
        float cosPitch = MathF.Cos(pitchRad);

        return new Vector3(
            camera.WorldPosition.X + (FlybyConstants.TargetDistance * cosPitch * MathF.Sin(yawRad)),
            camera.WorldPosition.Y + (FlybyConstants.TargetDistance * MathF.Sin(pitchRad)),
            camera.WorldPosition.Z + (FlybyConstants.TargetDistance * cosPitch * MathF.Cos(yawRad)));
    }

    /// <summary>
    /// Unwraps degree-based angle knots so spline interpolation stays continuous.
    /// </summary>
    private static void UnwrapAngles(float[] angles)
    {
        for (int i = 1; i < angles.Length; i++)
        {
            float delta = angles[i] - angles[i - 1];
            angles[i] -= MathF.Round(delta / 360.0f) * 360.0f;
        }
    }

    /// <summary>
    /// Unwraps RotationY, RotationX and Roll across consecutive cached frames to prevent
    /// discontinuities from atan2 wrapping at the +/-pi boundary.
    /// </summary>
    private static void UnwrapFrameAngles(FlybyFrameState[] frames)
    {
        for (int i = 1; i < frames.Length; i++)
        {
            float dyaw = frames[i].RotationY - frames[i - 1].RotationY;
            frames[i].RotationY -= MathF.Round(dyaw / MathC.TwoPi) * MathC.TwoPi;

            float dpitch = frames[i].RotationX - frames[i - 1].RotationX;
            frames[i].RotationX -= MathF.Round(dpitch / MathC.TwoPi) * MathC.TwoPi;

            float droll = frames[i].Roll - frames[i - 1].Roll;
            frames[i].Roll -= MathF.Round(droll / MathC.TwoPi) * MathC.TwoPi;
        }
    }

    #endregion Knot building

    #region Speed smoothing

    /// <summary>
    /// Pre-computes a smoothed speed curve from position deltas between consecutive frames.
    /// Uses three passes of box averaging to produce a visually smooth graph.
    /// </summary>
    private float[] ComputeSmoothedSpeeds()
    {
        if (_frames.Length < 2)
            return [];

        int count = _frames.Length - 1;
        var speeds = new float[count];
        var buffer = new float[count];

        for (int i = 0; i < count; i++)
        {
            if (IsInsideCutRegion(i) || IsInsideCutRegion(i + 1))
            {
                speeds[i] = 0.0f;
            }
            else
            {
                var delta = _frames[i + 1].Position - _frames[i].Position;
                speeds[i] = delta.Length() / FlybyConstants.TimeStep;
            }
        }

        // Three passes of box smoothing approximate a Gaussian filter.
        for (int pass = 0; pass < 3; pass++)
        {
            BoxSmooth(speeds, buffer, 5);
            (speeds, buffer) = (buffer, speeds);
        }

        return speeds;
    }

    /// <summary>
    /// Applies one pass of box smoothing to the provided data.
    /// </summary>
    /// <param name="source">Input samples to smooth.</param>
    /// <param name="destination">Destination buffer receiving the smoothed result.</param>
    /// <param name="radius">Radius of the averaging window in samples.</param>
    private static void BoxSmooth(float[] source, float[] destination, int radius)
    {
        int len = source.Length;

        for (int i = 0; i < len; i++)
        {
            float sum = 0;
            int low = Math.Max(0, i - radius);
            int high = Math.Min(len - 1, i + radius);

            for (int j = low; j <= high; j++)
                sum += source[j];

            destination[i] = sum / (high - low + 1);
        }
    }

    /// <summary>
    /// Returns the maximum valid speed value contained in the provided array.
    /// </summary>
    private static float ComputePeakSpeed(float[] speeds)
    {
        float peak = 0.0f;

        for (int i = 0; i < speeds.Length; i++)
        {
            float speed = speeds[i];

            if (float.IsFinite(speed) && speed > peak)
                peak = speed;
        }

        return peak;
    }

    #endregion Speed smoothing
}
