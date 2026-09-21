using TombLib.Scripting.UI.Providers;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.GameFlowScript.Resources;

/// <summary>
/// Provides the GameFlow color schemes through the shared scripting color provider contract.
/// </summary>
public sealed class GameFlowColorSchemeProvider : FileSystemColorSchemeProvider<GameFlowEditorConfiguration>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowColorSchemeProvider"/> class.
	/// </summary>
	public GameFlowColorSchemeProvider()
		: base(ScriptingPaths.Default.GameFlowColorConfigsDirectory)
	{ }
}
