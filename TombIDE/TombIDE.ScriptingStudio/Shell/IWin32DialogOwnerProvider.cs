#nullable enable

using System.Windows.Forms;

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Provides a Win32 owner window for modal dialogs opened from the scripting studio shell.
/// </summary>
public interface IWin32DialogOwnerProvider
{
	/// <summary>
	/// Sets the owner window exactly once. Throws if already set.
	/// </summary>
	void SetOwner(IWin32Window owner);

	/// <summary>
	/// Gets the current owner window, or <see langword="null"/> if not yet set.
	/// </summary>
	IWin32Window? GetOwner();
}
