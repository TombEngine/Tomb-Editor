using System;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.CommandSurface;

public class UIElementArgs
{
	public UICommand Command { get; }
	public Type UIModeEnumType { get; }

	public UIElementArgs(Type uiModeEnumType, UICommand command = UICommand.None)
	{
		Command = command;
		UIModeEnumType = uiModeEnumType;
	}
}
