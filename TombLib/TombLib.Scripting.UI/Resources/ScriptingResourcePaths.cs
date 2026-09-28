using System;
using System.IO;

namespace TombLib.Scripting.UI.Resources;

/// <summary>
/// Resolves paths to the per-language resource directories shipped next to the application.
/// </summary>
public static class ScriptingResourcePaths
{
	/// <summary>
	/// Gets the full path to a file inside a language resource subdirectory.
	/// </summary>
	/// <param name="subdirectory">The language resource subdirectory (for example "ClassicScript").</param>
	/// <param name="relativeSegments">The path segments relative to that subdirectory.</param>
	public static string GetResourcePath(string subdirectory, params string[] relativeSegments)
	{
		string[] pathSegments = new string[relativeSegments.Length + 3];
		pathSegments[0] = AppContext.BaseDirectory;
		pathSegments[1] = "Resources";
		pathSegments[2] = subdirectory;

		Array.Copy(relativeSegments, 0, pathSegments, 3, relativeSegments.Length);

		return Path.Combine(pathSegments);
	}
}
