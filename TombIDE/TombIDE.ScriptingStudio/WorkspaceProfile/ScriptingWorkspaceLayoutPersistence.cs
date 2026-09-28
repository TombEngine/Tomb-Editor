#nullable enable

using System;
using TombIDE.Shared.Docking;

namespace TombIDE.ScriptingStudio.WorkspaceProfile;

/// <summary>
/// Owns the default layout and persistence operations for one scripting workspace.
/// </summary>
public sealed class ScriptingWorkspaceLayoutPersistence
{
	private readonly Func<DockPanelState> _loadDockPanelState;
	private readonly Func<string> _loadAvalonDockLayoutXml;
	private readonly Action<string> _saveAvalonDockLayoutXml;

	/// <summary>
	/// Initializes a layout persistence policy.
	/// </summary>
	/// <param name="defaultLayout">The layout to use when no persisted layout exists.</param>
	/// <param name="loadDockPanelState">Loads the persisted dock-panel state.</param>
	/// <param name="loadAvalonDockLayoutXml">Loads the persisted AvalonDock layout.</param>
	/// <param name="saveAvalonDockLayoutXml">Saves the AvalonDock layout.</param>
	public ScriptingWorkspaceLayoutPersistence(
		DockPanelState defaultLayout,
		Func<DockPanelState>? loadDockPanelState,
		Func<string>? loadAvalonDockLayoutXml,
		Action<string>? saveAvalonDockLayoutXml)
	{
		ArgumentNullException.ThrowIfNull(loadDockPanelState);
		ArgumentNullException.ThrowIfNull(loadAvalonDockLayoutXml);
		ArgumentNullException.ThrowIfNull(saveAvalonDockLayoutXml);

		DefaultLayout = defaultLayout;
		_loadDockPanelState = loadDockPanelState;
		_loadAvalonDockLayoutXml = loadAvalonDockLayoutXml;
		_saveAvalonDockLayoutXml = saveAvalonDockLayoutXml;
	}

	/// <summary>
	/// Gets the default layout for the workspace.
	/// </summary>
	public DockPanelState DefaultLayout { get; }

	internal DockPanelState LoadDockPanelState()
		=> _loadDockPanelState();

	internal string LoadAvalonDockLayoutXml()
		=> _loadAvalonDockLayoutXml();

	internal void SaveAvalonDockLayoutXml(string layoutXml)
		=> _saveAvalonDockLayoutXml(layoutXml);
}
