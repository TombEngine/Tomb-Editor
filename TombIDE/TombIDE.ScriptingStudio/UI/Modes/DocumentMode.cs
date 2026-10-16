namespace TombIDE.ScriptingStudio.UI;

/// <summary>
/// Identifies the stable document kind used by scripting registrations, persisted document
/// state, and host presentation context.
/// </summary>
public enum DocumentMode
{
	None,
	PlainText,
	ClassicScript,
	Lua,
	GameFlowScript,
	TRX,
	Strings
}
