using System.Windows;
using System.Windows.Interop;
using IWinFormsWindow = System.Windows.Forms.IWin32Window;

namespace TombLib.WPF;

public static class WindowExtensions
{
	/// <summary>
	/// Sets the owner of the specified WPF <see cref="Window" /> to the provided WinForms window.
	/// </summary>
	/// <remarks>
	/// This method establishes ownership between a WPF window and a WinForms window, enabling proper
	/// behaviour for modal dialogs and window activation. Ensure that both the WPF window and the WinForms window are valid
	/// and initialized before calling this method.
	/// </remarks>
	/// <param name="window">The WPF <see cref="Window" /> whose owner is being set. Cannot be <see langword="null" />.</param>
	/// <param name="owner">The WinForms window that will act as the owner. Cannot be <see langword="null" />.</param>
	/// <returns>A <see cref="WindowInteropHelper" /> instance that links the WPF window to the specified owner.</returns>
	public static WindowInteropHelper SetOwner(this Window window, IWinFormsWindow owner)
		=> new(window) { Owner = owner.Handle };
}
