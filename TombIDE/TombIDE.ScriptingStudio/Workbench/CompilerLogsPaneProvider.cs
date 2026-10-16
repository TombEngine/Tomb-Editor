#nullable enable

using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.Build;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class CompilerLogsPaneProvider : IStudioPaneContributionProvider
{
	private readonly ScriptingWorkspaceProfile _profile;

	public CompilerLogsPaneProvider(ScriptingWorkspaceProfile profile)
	{
		ArgumentNullException.ThrowIfNull(profile);
		_profile = profile;
	}

	public IReadOnlyList<StudioPaneContribution> GetPaneContributions()
	{
		if (!_profile.SupportsView(UICommand.CompilerLogs))
			return [];

		var pane = new CompilerLogsToolWindow();
		return [new StudioPaneContribution(UICommand.CompilerLogs, pane.SerializationKey, () => pane)];
	}
}
