#nullable enable

using System;
using System.Windows;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Workbench;

/// <summary>
/// Owns workbench commands, editor settings, layout capture/restore, and host-focus forwarding.
/// Does not own shell settings persistence or the outer ElementHost boundary.
/// </summary>
public interface IWorkbenchService : IDisposable
{
	/// <summary>
	/// Gets the workbench view as a WPF <see cref="FrameworkElement"/>.
	/// </summary>
	FrameworkElement WorkbenchView { get; }

	/// <summary>
	/// Captures the current AvalonDock layout XML for persistence.
	/// </summary>
	string CaptureLayout();

	/// <summary>
	/// Ensures open document tabs are synchronized with the file system.
	/// </summary>
	void EnsureTabFileSynchronization();

	/// <summary>
	/// Notifies the workbench that the host application's main window focus changed.
	/// </summary>
	void NotifyMainWindowFocusChanged(bool isFocused);

	/// <summary>
	/// Applies stored editor settings (themes, fonts, etc.) to all open editors.
	/// </summary>
	void ApplyEditorSettings();

	/// <summary>
	/// Restores the default dock layout and pane visibility.
	/// </summary>
	void RestoreDefaultLayout();

	/// <summary>
	/// Attempts to execute a workbench-level command.
	/// Returns true if the command was handled.
	/// </summary>
	bool TryExecuteCommand(UICommand command);
}
