using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.GameFlowScript;

internal static class GameFlowResourcePaths
{
	public static string GetResourcePath(params string[] relativeSegments)
		=> ScriptingResourcePaths.GetResourcePath("GameFlow", relativeSegments);
}
