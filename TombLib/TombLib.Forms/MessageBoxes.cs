using DarkUI.Forms;
using System.Windows.Forms;

namespace TombLib.Forms;

public static class MessageBoxes
{
	public static DialogResult NonANSIFilePathError(IWin32Window owner)
		=> DarkMessageBox.Show(owner,
			"Filename or path is invalid. Please use standard characters.",
			"Wrong filename", MessageBoxIcon.Error);

	public static DialogResult LuaNameAlreadyTakenError(IWin32Window owner)
		=> DarkMessageBox.Show(owner,
			"The value of Lua Name is already taken by another object",
			"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
