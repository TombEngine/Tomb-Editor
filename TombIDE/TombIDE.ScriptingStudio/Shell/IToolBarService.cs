#nullable enable

using System;
using System.Collections.Generic;
using System.Windows;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.ToolStrips;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Owns the WPF toolbar view and toolbar command-state mutation.
/// Must not own layout persistence or dialogs.
/// </summary>
public interface IToolBarService : IDisposable
{
	/// <summary>
	/// Gets the toolbar view as a WPF <see cref="FrameworkElement"/>.
	/// </summary>
	FrameworkElement ToolBarView { get; }

	/// <summary>
	/// Raised when a command is invoked through the toolbar.
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
	/// Sets the tooltip text for the given command.
	/// </summary>
	void SetCommandToolTip(UICommand command, string toolTipText);

	/// <summary>
	/// Sets the document-mode command surface items for the toolbar.
	/// </summary>
	void SetDocumentCommandSurface(
		DocumentMode documentMode,
		IReadOnlyList<StudioToolStripItem> toolStripItems);

	/// <summary>
	/// Updates the enabled state for every command. Shell-owned commands
	/// are resolved internally; all other commands delegate to
	/// <paramref name="canExecuteCommand"/>.
	/// </summary>
	void UpdateCommandEnabledStates(Func<UICommand, bool> canExecuteCommand);
}
