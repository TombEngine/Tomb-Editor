using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;

namespace TombIDE.ScriptingStudio.Shell;

internal sealed class StudioDockLayoutPersistenceService
{
	public string LoadLayoutXml(ScriptingWorkspaceProfile workspaceProfile)
		=> workspaceProfile?.LoadAvalonDockLayoutXml() ?? string.Empty;

	public DockPanelState LoadLegacyState(ScriptingWorkspaceProfile workspaceProfile)
		=> workspaceProfile?.LoadDockPanelState();

	public void SaveState(ScriptingWorkspaceProfile workspaceProfile, string layoutXml)
	{
		if (workspaceProfile is null)
			return;

		workspaceProfile.SaveAvalonDockLayoutXml(layoutXml ?? string.Empty);
	}
}
