#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TombIDE.ScriptingStudio.ToolStrips;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Shell;

internal sealed class StatusBarService : IStatusBarService
{
	private readonly StudioStatusStrip _statusStrip;
	private readonly StudioStatusStripContributionService _statusStripContributionService;
	private readonly ScriptingWorkspaceProfile _workspaceProfile;

	public StatusBarService(
		ScriptingWorkspaceProfile workspaceProfile,
		StudioStatusStripContributionService statusStripContributionService)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);
		ArgumentNullException.ThrowIfNull(statusStripContributionService);

		_workspaceProfile = workspaceProfile;
		_statusStripContributionService = statusStripContributionService;
		_statusStrip = new StudioStatusStrip
		{
			DocumentMode = DocumentMode.None,
			SegmentContributions = statusStripContributionService.CreateSegments(
				workspaceProfile.StatusStripSegments,
				[])
		};
	}

	public FrameworkElement StatusBarView => _statusStrip.View;

	public void SetStatusStripContext(
		IEditorControl? editor,
		DocumentMode documentMode,
		IReadOnlyList<StudioStatusStripSegment> documentSegments)
	{
		_statusStrip.DocumentMode = documentMode;
		_statusStrip.EditorControl = editor;
		_statusStrip.SegmentContributions = _statusStripContributionService.CreateSegments(
			_workspaceProfile.StatusStripSegments,
			documentSegments?.ToArray() ?? []);
	}

	public void Dispose()
	{
		// StudioStatusStrip has no event subscriptions to clean up.
	}
}
