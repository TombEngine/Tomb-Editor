namespace TombLib.Scripting.GameFlowScript;

/// <summary>
/// Provides the paths to the GameFlow documentation resources.
/// </summary>
public static class GameFlowDocumentationPaths
{
	/// <summary>
	/// Gets the path to the main GameFlow manual.
	/// </summary>
	public static string MainManualPath => GameFlowResourcePaths.GetResourcePath("TRGameflow.pdf");

	/// <summary>
	/// Gets the path to the extra commands manual.
	/// </summary>
	public static string ExtraCommandsManualPath => GameFlowResourcePaths.GetResourcePath("TRGameflow extra commands.pdf");
}
