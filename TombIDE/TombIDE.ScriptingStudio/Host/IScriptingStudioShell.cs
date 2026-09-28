#nullable enable

using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TombIDE.ScriptingStudio.Host;

/// <summary>
/// Defines the narrow host boundary for mounting the ScriptingStudio shell into TombIDE.
/// </summary>
public interface IScriptingStudioShell : IAsyncDisposable
{
	/// <summary>
	/// Stops shell work, detaches workspace projections, and disposes the shell scope.
	/// </summary>
	Task StopAsync();

	/// <summary>
	/// Mounts the shell into the supplied WinForms host container.
	/// </summary>
	/// <param name="hostContainer">The host container that should display the shell.</param>
	/// <param name="ownerForm">The owning TombIDE form.</param>
	void Mount(Control hostContainer, Form ownerForm);

	/// <summary>
	/// Forwards a main-window focus change to the shell.
	/// </summary>
	/// <param name="isFocused">Whether the main window is focused.</param>
	void NotifyMainWindowFocusChanged(bool isFocused);

	/// <summary>
	/// Notifies the shell that its host tab became active.
	/// </summary>
	void NotifyHostTabActivated();
}
