#nullable enable

using System.Linq;

namespace TombIDE.Shared.SharedClasses;

/// <summary>
/// Provides helper methods for validating and sanitizing level names and data file names.
/// </summary>
public static class LevelNameHelper
{
	/// <summary>
	/// Characters that are not allowed in level names.
	/// </summary>
	private static readonly char[] IllegalNameChars = { ';', '[', ']', '=', ',', '.', '!' };

	/// <summary>
	/// Characters that are not allowed in variable-style names (used for data file names).
	/// </summary>
	private static readonly char[] IllegalVariableChars =
	{
		' ', ';', ':', '(', ')', '[', ']', '{', '}', '<', '>', '=', ',', '.', '!',
		'-', '+', '*', '?', '/', '\\', '"', '\'', '&', '%', '#', '@', '|', '^', '`', '~', '$'
	};

	/// <summary>
	/// Validates and sanitizes a level name by removing invalid path and name characters.
	/// </summary>
	/// <param name="levelName">The raw level name to validate.</param>
	/// <returns>The sanitized level name, or <see langword="null"/> if the result is empty or whitespace.</returns>
	public static string? ValidateLevelName(string levelName)
	{
		string sanitized = PathHelper.RemoveIllegalPathSymbols(levelName.Trim());
		sanitized = RemoveIllegalNameSymbols(sanitized);

		return string.IsNullOrWhiteSpace(sanitized) ? null : sanitized;
	}

	/// <summary>
	/// Validates and sanitizes a data file name by converting it to a valid variable-style name.
	/// </summary>
	/// <param name="dataFileName">The raw data file name to validate.</param>
	/// <returns>The sanitized data file name, or <see langword="null"/> if the result is empty or whitespace.</returns>
	public static string? ValidateDataFileName(string dataFileName)
	{
		string sanitized = MakeValidVariableName(dataFileName.Trim());
		return string.IsNullOrWhiteSpace(sanitized) ? null : sanitized;
	}

	/// <summary>
	/// Converts a level name to a suggested data file name by replacing spaces with underscores.
	/// </summary>
	/// <param name="levelName">The level name to convert.</param>
	/// <returns>A suggested data file name (e.g. <c>"My Level"</c> becomes <c>"My_Level"</c>).</returns>
	public static string SuggestDataFileName(string levelName)
		=> levelName.Trim().Replace(' ', '_');

	/// <summary>
	/// Removes characters that are illegal in level names.
	/// <para>
	/// These include semicolons, brackets, equals, commas, dots, and exclamation marks,
	/// which would conflict with classic Tomb Raider script file syntax.
	/// </para>
	/// </summary>
	/// <param name="levelName">The level name to sanitize.</param>
	/// <returns>The level name with illegal characters removed.</returns>
	public static string RemoveIllegalNameSymbols(string levelName)
		=> IllegalNameChars.Aggregate(levelName, (current, c) => current.Replace(c.ToString(), string.Empty));

	/// <summary>
	/// Converts a string into a valid variable-style name suitable for use as a data file name.
	/// <para>
	/// Replaces special characters with underscores, prefixes with an underscore if the name
	/// starts with a digit, and collapses consecutive underscores into a single one.
	/// </para>
	/// </summary>
	/// <param name="name">The name to convert.</param>
	/// <returns>A valid variable-style name.</returns>
	public static string MakeValidVariableName(string name)
	{
		string result = IllegalVariableChars.Aggregate(name.Trim(), (current, c) => current.Replace(c.ToString(), "_"));

		if (char.IsDigit(result.FirstOrDefault()))
			result = "_" + result;

		// Collapse consecutive underscores
		while (result.Contains("__"))
			result = result.Replace("__", "_");

		return result;
	}
}
