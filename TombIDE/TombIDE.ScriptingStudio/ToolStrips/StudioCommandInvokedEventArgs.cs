#nullable enable

using System;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.ToolStrips;

public sealed class StudioCommandInvokedEventArgs : EventArgs
{
	public StudioCommandInvokedEventArgs(UICommand command)
	{
		Command = command;
	}

	public UICommand Command { get; }
}
