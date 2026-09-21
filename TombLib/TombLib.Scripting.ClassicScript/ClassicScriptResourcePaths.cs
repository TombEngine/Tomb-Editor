using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.ClassicScript;

/// <summary>
/// Provides the paths to the ClassicScript resource files.
/// </summary>
public static class ClassicScriptResourcePaths
{
	/// <summary>
	/// Gets the resource path for the given relative segments.
	/// </summary>
	/// <param name="relativeSegments">The relative path segments.</param>
	/// <returns>The full resource path.</returns>
	public static string GetResourcePath(params string[] relativeSegments)
		=> ScriptingResourcePaths.GetResourcePath("ClassicScript", relativeSegments);

	/// <summary>
	/// Gets the path of the command catalog resource.
	/// </summary>
	/// <returns>The path of the commands resource.</returns>
	public static string GetCommandsPath()
		=> GetResourcePath("Commands.json");

	/// <summary>
	/// Gets the path of the mnemonic constants resource.
	/// </summary>
	/// <returns>The path of the mnemonic constants resource.</returns>
	public static string GetMnemonicConstantsPath()
		=> GetResourcePath("MnemonicConstants.json");
}
