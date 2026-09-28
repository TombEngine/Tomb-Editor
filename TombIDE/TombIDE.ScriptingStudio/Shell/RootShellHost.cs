#nullable enable

using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombIDE.ScriptingStudio.Shell;

internal sealed class RootShellHost : ElementHost
{
	public RootShellHost(RootShellView view)
	{
		ArgumentNullException.ThrowIfNull(view);

		Dock = DockStyle.Fill;
		Child = view;
	}
}
