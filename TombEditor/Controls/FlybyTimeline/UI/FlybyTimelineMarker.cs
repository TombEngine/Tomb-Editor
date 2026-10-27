namespace TombEditor.Controls.FlybyTimeline.UI;

/// <summary>
/// Represents one rendered marker on the timeline.
/// </summary>
public readonly struct FlybyTimelineMarker
{
    /// <summary>
    /// Gets the marker time on the timeline.
    /// </summary>
    public float TimeSeconds { get; init; }

    /// <summary>
    /// Gets whether this marker has a duplicate camera index.
    /// </summary>
    public bool IsDuplicate { get; init; }

    /// <summary>
    /// Gets whether this marker is currently selected.
    /// </summary>
    public bool IsSelected { get; init; }

    /// <summary>
    /// Gets whether this marker starts a camera cut.
    /// </summary>
    public bool HasCameraCut { get; init; }

    /// <summary>
    /// Gets whether this marker lies inside a cut-bypassed region.
    /// </summary>
    public bool IsInCutBypass { get; init; }

    /// <summary>
    /// Gets the duration bypassed by a cut starting at this marker.
    /// </summary>
    public float CutBypassDuration { get; init; }

    /// <summary>
    /// Gets the duration of the outgoing segment starting at this marker.
    /// </summary>
    public float SegmentDuration { get; init; }

    /// <summary>
    /// Gets whether this marker starts a freeze region.
    /// </summary>
    public bool HasFreeze { get; init; }

    /// <summary>
    /// Gets the duration of the freeze starting at this marker.
    /// </summary>
    public float FreezeDuration { get; init; }
}
