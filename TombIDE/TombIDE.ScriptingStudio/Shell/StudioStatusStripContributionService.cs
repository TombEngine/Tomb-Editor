#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace TombIDE.ScriptingStudio.Shell;

internal sealed class StudioStatusStripContributionService
{
	public StudioStatusStripSegment[] CreateSegments(
		IReadOnlyList<StudioStatusStripSegment>? workspaceSegments,
		IReadOnlyList<StudioStatusStripSegment> documentSegments)
	{
		var segments = new HashSet<StudioStatusStripSegment>();

		if (workspaceSegments is not null)
		{
			foreach (StudioStatusStripSegment segment in workspaceSegments)
				segments.Add(segment);
		}

		foreach (StudioStatusStripSegment segment in documentSegments)
			segments.Add(segment);

		return segments.ToArray();
	}
}
