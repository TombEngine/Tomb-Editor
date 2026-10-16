#nullable enable

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Narrow host-action interface for the specific outbound operations
/// Scripting Studio needs from the outer TombIDE host.
/// </summary>
public interface IScriptingHostOperations
{
	/// <summary>
	/// Notifies the host that an external change was made to a script file,
	/// so the host can update its UI state (e.g. save button, dirty indicator).
	/// </summary>
	void IndicateExternalChange();
}
