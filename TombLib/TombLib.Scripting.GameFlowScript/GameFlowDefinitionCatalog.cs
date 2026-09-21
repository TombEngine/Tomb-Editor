using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TombLib.Scripting.GameFlowScript;

/// <summary>
/// Provides the GameFlow definitions loaded from the bundled definitions resource.
/// </summary>
public static class GameFlowDefinitionCatalog
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private static readonly Lazy<GameFlowDefinitionSet> s_definitions = new(LoadDefinitions);

	/// <summary>
	/// Gets the special property names.
	/// </summary>
	public static IReadOnlyList<string> SpecialProperties => s_definitions.Value.SpecialProperties;

	/// <summary>
	/// Gets the section names.
	/// </summary>
	public static IReadOnlyList<string> Sections => s_definitions.Value.Sections;

	/// <summary>
	/// Gets the constant names.
	/// </summary>
	public static IReadOnlyList<string> Constants => s_definitions.Value.Constants;

	/// <summary>
	/// Gets the property names.
	/// </summary>
	public static IReadOnlyList<string> Properties => s_definitions.Value.Properties;

	private static GameFlowDefinitionSet LoadDefinitions()
	{
		string filePath = GameFlowResourcePaths.GetResourcePath("GameFlowDefinitions.json");

		if (!File.Exists(filePath))
		{
			Log.Warn("GameFlow definitions resource '{Path}' was not found; using an empty catalog.", filePath);
			return GameFlowDefinitionSet.Empty;
		}

		try
		{
			string json = File.ReadAllText(filePath);
			return JsonSerializer.Deserialize<GameFlowDefinitionSet>(json) ?? GameFlowDefinitionSet.Empty;
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to load GameFlow definitions from '{Path}'; using an empty catalog.", filePath);
			return GameFlowDefinitionSet.Empty;
		}
	}
}
