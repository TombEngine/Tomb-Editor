#nullable enable

using System;
using System.Windows.Forms;

namespace TombIDE.ScriptingStudio.Shell;

internal sealed class Win32DialogOwnerProvider : IWin32DialogOwnerProvider
{
	private IWin32Window? _owner;

	public void SetOwner(IWin32Window owner)
	{
		ArgumentNullException.ThrowIfNull(owner);

		if (_owner is not null)
			throw new InvalidOperationException("The dialog owner has already been set.");

		_owner = owner;
	}

	public IWin32Window? GetOwner() => _owner;
}
