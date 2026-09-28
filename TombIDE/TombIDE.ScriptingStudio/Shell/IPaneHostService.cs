#nullable enable

using System;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Owns dock-host pane visibility and registration operations.
/// Exposes no concrete shell view.
/// Must not own menu checked-state policy.
/// </summary>
public interface IPaneHostService : IDisposable
{
	/// <summary>
	/// Returns true if the command is a pane visibility command.
	/// </summary>
	bool IsPaneVisibilityCommand(UICommand command);

	/// <summary>
	/// Sets the visibility state for the given pane command.
	/// </summary>
	void SetPaneVisibility(UICommand command, bool isVisible);

	/// <summary>
	/// Toggles the visibility of the pane associated with the command.
	/// Returns the new visibility state.
	/// </summary>
	bool TogglePaneVisibility(UICommand command);

	/// <summary>
	/// Resets all pane visibility states to their defaults.
	/// </summary>
	void ResetPaneVisibilityStates();
}
