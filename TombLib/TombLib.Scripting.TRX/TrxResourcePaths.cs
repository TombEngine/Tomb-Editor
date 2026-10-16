using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.TRX;

/// <summary>
/// Provides resource paths used by the TRX scripting project.
/// </summary>
public static class TRXResourcePaths
{
	/// <summary>
	/// Gets the resource path for the given relative segments under the TRX resource root.
	/// </summary>
	/// <param name="relativeSegments">The relative path segments.</param>
	/// <returns>The combined resource path.</returns>
	public static string GetResourcePath(params string[] relativeSegments)
		=> ScriptingResourcePaths.GetResourcePath("TRX", relativeSegments);

	/// <summary>
	/// Gets the path of the GameFlow schema resource.
	/// </summary>
	/// <returns>The path of the GameFlow schema file.</returns>
	public static string GetGameFlowSchemaPath()
		=> GetResourcePath("gameflow.schema.json");
}
