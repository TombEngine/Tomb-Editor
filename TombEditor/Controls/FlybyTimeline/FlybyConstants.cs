using TombLib;
using TombLib.LevelData;

namespace TombEditor.Controls.FlybyTimeline;

public static class FlybyConstants
{
    // The game logic runs at 30 ticks per second.
    public const float TickRate = 30.0f;
    public const float TimeStep = 1.0f / TickRate;

    /// <summary>
    /// Converts native flyby speed units into sectors-per-second style values used by the editor math.
    /// </summary>
    public const float SpeedScale = TickRate / 100.0f;

    // Native camera flags.

    public const int FlagCameraCut = 1 << 7;
    public const int FlagFreezeCamera = 1 << 8;

    // Spline constants.

    /// <summary>
    /// Distance, in normalized segment time, over which freeze cameras ease in and out instead of snapping.
    /// </summary>
    public const float FreezeEaseDistance = 0.15f;

    /// <summary>
    /// Smallest meaningful flyby speed. Also acts as a zero guard when normalizing speed-derived UI values.
    /// </summary>
    public const float MinSpeed = 0.001f;

    public const float MaxSpeed = FlybyCameraInstance.MaxFlybySpeed;

    /// <summary>
    /// Time tolerance, in seconds, used by the speed solver to stop once the requested camera time is matched closely enough.
    /// </summary>
    public const float SpeedSolveTargetTimeTolerance = 0.0001f;

    /// <summary>
    /// Squared-length threshold for treating a computed look vector as degenerate.
    /// This is in squared world-space units because it is compared against LengthSquared().
    /// </summary>
    public const float LookDirectionLengthSquaredEpsilon = 0.001f;

    /// <summary>
    /// Minimum horizontal or vertical separation required before solving a stable yaw/pitch pair.
    /// </summary>
    public const float RotationSolveDistanceEpsilon = 0.001f;

    // Preview constants.

    public const int PreviewTimerInterval = 16;

    /// <summary>
    /// End-of-sequence tolerance, in seconds, used to decide whether pressing play should restart from 0.
    /// </summary>
    public const float PreviewReplayEndTolerance = 0.001f;

    /// <summary>
    /// Lower bound for preview FOV values to avoid invalid or near-zero projection setup.
    /// </summary>
    public const float PreviewMinFieldOfViewRadians = 0.01f;

    /// <summary>
    /// Upper bound for flyby camera field-of-view values, expressed in degrees.
    /// </summary>
    public const float MaxFlybyFieldOfViewDegrees = 179.99f;

    /// <summary>
    /// Default preview field of view, expressed in degrees, used as a fallback when flyby data is invalid or non-finite.
    /// </summary>
    public const float DefaultPreviewFieldOfViewDegrees = 80.0f;

    /// <summary>
    /// Upper bound for preview projection field-of-view values, expressed in radians.
    /// </summary>
    public static readonly float MaxPreviewFieldOfViewRadians = MathC.DegToRad(MaxFlybyFieldOfViewDegrees);

    // Timeline constants.

    public const float TimelineZoomOutScale = 1.05f;
    public const float TimelinePanStepFraction = 0.05f;
    public const float TimelineDragSpeedStep = 0.01f;
    public const bool TimelineSmoothPanEnabled = true;
    public const bool TimelineSmoothZoomEnabled = true;
    public const int TimelineSmoothViewportTimerInterval = 8;

    /// <summary>
    /// Interpolation factor applied on each smooth-pan or smooth-zoom timer tick.
    /// Higher values settle faster but feel less smooth.
    /// </summary>
    public const float TimelineSmoothViewportLerpFactor = 0.3f;

    /// <summary>
    /// Viewport delta threshold, in seconds, below which smooth viewport animation snaps to the target.
    /// </summary>
    public const float TimelineSmoothViewportEpsilon = 0.001f;

    public const float TimelineMarkerRadius = 6.375f;
    public const float TimelineMinTickSpacing = 40.0f;
    public const float TimelineRulerHeight = 20.0f;

    /// <summary>
    /// Smallest cursor time, in seconds, accepted when inserting a camera from the timeline.
    /// Prevents creating a new first key too close to absolute sequence start.
    /// </summary>
    public const float TimelineAddCameraMinCursorTime = 0.01f;

    /// <summary>
    /// Time tolerance, in seconds, for treating markers as visible when viewport comparisons are affected by float drift.
    /// </summary>
    public const float TimelineViewportTimeTolerance = 0.0001f;

    /// <summary>
    /// Time tolerance, in seconds, for treating the playhead as being at the final camera marker.
    /// </summary>
    public const float TimelineSequenceEndTolerance = TimelineViewportTimeTolerance;

    /// <summary>
    /// Time tolerance, in seconds, for aligning cut-region boundaries in ruler and viewport calculations.
    /// </summary>
    public const float CutBoundaryTolerance = TimelineViewportTimeTolerance;

    /// <summary>
    /// Smallest visible timeline span, in seconds, allowed after a zoom operation.
    /// </summary>
    public const float TimelineMinViewportRange = 0.1f;

    /// <summary>
    /// Distance from camera to target point used when converting flyby rotations into a synthetic look target.
    /// This matches the level compiler convention so editor preview and exported data stay aligned.
    /// </summary>
    public const float TargetDistance = Level.SectorSizeUnit;

    /// <summary>
    /// Returns whether the given game version uses the TombEngine smooth-pause timing model.
    /// </summary>
    public static bool UseSmoothPause(TRVersion.Game? gameVersion) => gameVersion == TRVersion.Game.TombEngine;
}
