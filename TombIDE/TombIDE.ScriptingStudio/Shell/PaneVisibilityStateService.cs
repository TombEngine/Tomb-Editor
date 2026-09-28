#nullable enable

using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Owns pane visibility checked-state for menu and toolbar presentation.
/// Must not construct panes or access AvalonDock.
/// The operational show/hide is performed by the workbench; this service
/// tracks the resulting checked state.
/// </summary>
internal sealed class PaneVisibilityStateService : IPaneHostService
{
	private static readonly UICommand[] PaneVisibilityCommands =
	[
		UICommand.ContentExplorer,
		UICommand.FileExplorer,
		UICommand.ReferenceBrowser,
		UICommand.CompilerLogs,
		UICommand.SearchResults,
		UICommand.LuaDiagnostics,
		UICommand.LuaReferencesResults
	];

	private readonly Dictionary<UICommand, bool> _paneVisibilityStates = [];
	private readonly IMenuService _menuService;
	private readonly IToolBarService _toolBarService;
	private readonly ScriptingWorkspaceProfile _workspaceProfile;

	public PaneVisibilityStateService(
		ScriptingWorkspaceProfile workspaceProfile,
		IMenuService menuService,
		IToolBarService toolBarService)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);
		ArgumentNullException.ThrowIfNull(menuService);
		ArgumentNullException.ThrowIfNull(toolBarService);

		_workspaceProfile = workspaceProfile;
		_menuService = menuService;
		_toolBarService = toolBarService;

		InitializePaneVisibilityStates();
		ApplyWorkspaceVisibility();
		RefreshPaneVisibilityChecks();
	}

	public bool IsPaneVisibilityCommand(UICommand command)
		=> _paneVisibilityStates.ContainsKey(command);

	public void SetPaneVisibility(UICommand command, bool isVisible)
	{
		if (!_paneVisibilityStates.ContainsKey(command))
			return;

		_paneVisibilityStates[command] = isVisible;
		SetCommandChecked(command, isVisible);
	}

	public bool TogglePaneVisibility(UICommand command)
	{
		if (!_paneVisibilityStates.TryGetValue(command, out bool isVisible))
			return false;

		bool newValue = !isVisible;
		SetPaneVisibility(command, newValue);
		return newValue;
	}

	public void ResetPaneVisibilityStates()
	{
		foreach (UICommand command in _paneVisibilityStates.Keys)
			_paneVisibilityStates[command] = false;

		RefreshPaneVisibilityChecks();
	}

	/// <summary>
	/// Refreshes checked state for all pane visibility commands.
	/// Called externally when dock layout changes so menu/toolbar
	/// checked state stays in sync with actual pane visibility.
	/// </summary>
	public void RefreshPaneVisibilityChecks()
	{
		foreach ((UICommand command, bool isVisible) in _paneVisibilityStates)
			SetCommandChecked(command, isVisible);
	}

	public void Dispose()
	{
		// No event subscriptions to clean up.
	}

	private void ApplyWorkspaceVisibility()
	{
		foreach (UICommand command in PaneVisibilityCommands)
		{
			bool isVisible = _workspaceProfile.SupportsView(command);
			_menuService.SetCommandVisible(command, isVisible);
			_toolBarService.SetCommandVisible(command, isVisible);
		}
	}

	private void InitializePaneVisibilityStates()
	{
		foreach (UICommand command in PaneVisibilityCommands)
		{
			if (_workspaceProfile.SupportsView(command))
				_paneVisibilityStates[command] = false;
		}
	}

	private void SetCommandChecked(UICommand command, bool isChecked)
	{
		_menuService.SetCommandChecked(command, isChecked);
		_toolBarService.SetCommandChecked(command, isChecked);
	}
}
