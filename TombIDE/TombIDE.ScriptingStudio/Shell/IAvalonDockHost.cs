#nullable enable

using System;
using System.Windows;
using System.Windows.Threading;
using TombIDE.Shared.Docking;

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Adapter around the AvalonDock host view, exposing only layout,
/// pane visibility, and document synchronization operations.
/// </summary>
public interface IAvalonDockHost
{
	/// <summary>
	/// Gets the WPF view for the dock host.
	/// </summary>
	FrameworkElement View { get; }

	/// <summary>
	/// Gets the dispatcher associated with the dock host view.
	/// </summary>
	Dispatcher Dispatcher { get; }

	/// <summary>
	/// Saves the current AvalonDock layout as XML.
	/// </summary>
	string SaveLayout();

	/// <summary>
	/// Restores the AvalonDock layout from a serialized XML string,
	/// falling back to the default layout on failure.
	/// </summary>
	void RestoreLayout(string? layoutXml, DockPanelState legacyLayout, Action? onLayoutRestored = null);

	/// <summary>
	/// Restores the default dock layout.
	/// </summary>
	void RestoreDefaultLayout(DockPanelState legacyLayout, Action? onLayoutRestored = null);

	/// <summary>
	/// Detaches the document controller from dock events.
	/// </summary>
	void DetachDocumentController();

	/// <summary>
	/// Returns true if the given pane is currently visible.
	/// </summary>
	bool IsPaneVisible(StudioDockPane? pane);

	/// <summary>
	/// Shows the given pane and activates it.
	/// Returns true if the pane is now visible.
	/// </summary>
	bool ShowPane(StudioDockPane? pane);

	/// <summary>
	/// Toggles the visibility of the given pane.
	/// Returns true if the pane is visible after the operation.
	/// </summary>
	bool TogglePane(StudioDockPane? pane);

	/// <summary>
	/// Ensures the pane is registered in the dock layout.
	/// Returns true if registration succeeded or the pane was already registered.
	/// </summary>
	bool EnsurePane(StudioDockPane? pane);
}
