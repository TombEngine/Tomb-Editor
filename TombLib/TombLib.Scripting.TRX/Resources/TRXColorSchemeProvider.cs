using TombLib.Scripting.UI.Providers;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.TRX.Resources;

/// <summary>
/// Provides the TRX color schemes through the shared scripting color provider contract.
/// </summary>
public sealed class TRXColorSchemeProvider : FileSystemColorSchemeProvider<TRXEditorConfiguration>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TRXColorSchemeProvider"/> class.
	/// </summary>
	public TRXColorSchemeProvider()
		: base(ScriptingPaths.Default.TRXColorConfigsDirectory)
	{ }
}
