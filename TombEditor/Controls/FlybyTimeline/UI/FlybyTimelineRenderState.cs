#nullable enable

using System.Collections.Generic;
using TombEditor.Controls.FlybyTimeline.Sequence;

namespace TombEditor.Controls.FlybyTimeline.UI;

/// <summary>
/// Represents the data required to render the current timeline state.
/// </summary>
public readonly struct FlybyTimelineRenderState(IReadOnlyList<FlybyTimelineMarker> markers, FlybySequenceCache? cache, float totalDuration)
{
    /// <summary>
    /// Gets the markers that should be rendered by the timeline control.
    /// </summary>
    public IReadOnlyList<FlybyTimelineMarker> Markers { get; } = markers;

    /// <summary>
    /// Gets the sequence cache associated with the rendered timeline.
    /// </summary>
    public FlybySequenceCache? Cache { get; } = cache;

    /// <summary>
    /// Gets the total duration to use for the visible timeline range.
    /// </summary>
    public float TotalDuration { get; } = totalDuration;
}
