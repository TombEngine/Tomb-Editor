#nullable enable

using System;
using System.Collections.Generic;
using System.Windows;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.ToolStrips;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Owns the WPF menu view and menu command-state mutation.
/// Must not own pane construction or document control.
/// </summary>
public interface IMenuService : IDisposable
{
	/// <summary>
	/// Gets the menu view as a WPF <see cref="FrameworkElement"/>.
	/// </summary>
	FrameworkElement MenuView { get; }

	/// <summary>
	/// Raised when a command is invoked through the menu.
	/// </summary>
	event EventHandler<StudioCommandInvokedEventArgs>? CommandInvoked;

	/// <summary>
	/// Sets the checked state for the given command.
	/// </summary>
	void SetCommandChecked(UICommand command, bool isChecked);

	/// <summary>
	/// Sets the enabled state for the given command.
	/// </summary>
	void SetCommandEnabled(UICommand command, bool isEnabled);

	/// <summary>
	/// Sets the visibility for the given command.
	/// </summary>
	void SetCommandVisible(UICommand command, bool isVisible);

	/// <summary>
	/// Sets the display text for the given command.
	/// </summary>
	void SetCommandText(UICommand command, string text);

	/// <summary>
	/// Sets the document-mode command surface items for the menu.
	/// </summary>
	void SetDocumentCommandSurface(
		DocumentMode documentMode,
		IReadOnlyList<StudioToolStripItem> menuItems);

	/// <summary>
	/// Updates the enabled state for every command. Shell-owned commands
	/// are resolved internally; all other commands delegate to
	/// <paramref name="canExecuteCommand"/>.
	/// </summary>
	void UpdateCommandEnabledStates(Func<UICommand, bool> canExecuteCommand);
}
