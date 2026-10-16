using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TombLib.Scripting.ClassicScript.Commands;

/// <summary>
/// Loads the unified command catalog into a read-only model of sections and command entries.
/// </summary>
public sealed class ClassicScriptCommandsLoader
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	/// <summary>
	/// Loads the command catalog from the bundled commands resource.
	/// </summary>
	/// <returns>The loaded catalog.</returns>
	public ClassicScriptCommandsCatalog Load()
		=> Load(ClassicScriptResourcePaths.GetCommandsPath());

	/// <summary>
	/// Loads the command catalog from the given file.
	/// </summary>
	/// <param name="filePath">The path of the command catalog file.</param>
	/// <returns>The loaded catalog, or an empty catalog when the file is missing or malformed.</returns>
	public ClassicScriptCommandsCatalog Load(string filePath)
	{
		if (!File.Exists(filePath))
			return new ClassicScriptCommandsCatalog([], []);

		try
		{
			using JsonDocument document = JsonDocument.Parse(File.ReadAllText(filePath));

			if (document.RootElement.ValueKind != JsonValueKind.Object)
				return new ClassicScriptCommandsCatalog([], []);

			return new ClassicScriptCommandsCatalog(
				ReadSections(document.RootElement),
				ReadCommands(document.RootElement));
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to load the command catalog from '{Path}'; using an empty catalog.", filePath);
			return new ClassicScriptCommandsCatalog([], []);
		}
	}

	private static IReadOnlyList<string> ReadSections(JsonElement root)
	{
		if (!root.TryGetProperty("Sections", out JsonElement sections) || sections.ValueKind != JsonValueKind.Array)
			return [];

		var result = new List<string>();

		foreach (JsonElement section in sections.EnumerateArray())
		{
			if (section.ValueKind == JsonValueKind.String)
				result.Add(section.GetString() ?? string.Empty);
		}

		return result;
	}

	private static IReadOnlyList<ClassicScriptCommandEntry> ReadCommands(JsonElement root)
	{
		if (!root.TryGetProperty("Commands", out JsonElement commands) || commands.ValueKind != JsonValueKind.Array)
			return [];

		var result = new List<ClassicScriptCommandEntry>();

		foreach (JsonElement command in commands.EnumerateArray())
		{
			ClassicScriptCommandEntry? entry = ReadCommand(command);

			if (entry is not null)
				result.Add(entry);
		}

		return result;
	}

	private static ClassicScriptCommandEntry? ReadCommand(JsonElement element)
	{
		if (element.ValueKind != JsonValueKind.Object)
			return null;

		string? name = GetStringProperty(element, "Name");

		if (string.IsNullOrWhiteSpace(name))
			return null;

		if (!TryGetKind(GetStringProperty(element, "Kind"), out ClassicScriptCommandKind kind))
			return null;

		return new ClassicScriptCommandEntry(
			name,
			kind,
			ReadSyntaxes(element),
			GetStringProperty(element, "DescriptionKey"));
	}

	private static IReadOnlyList<ClassicScriptSyntaxEntry> ReadSyntaxes(JsonElement element)
	{
		if (!element.TryGetProperty("Syntaxes", out JsonElement syntaxes) || syntaxes.ValueKind != JsonValueKind.Array)
			return [];

		var result = new List<ClassicScriptSyntaxEntry>();

		foreach (JsonElement syntax in syntaxes.EnumerateArray())
		{
			if (syntax.ValueKind != JsonValueKind.Object)
				continue;

			string? key = GetStringProperty(syntax, "Key");
			string? text = GetStringProperty(syntax, "Text");

			if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(text))
				continue;

			result.Add(new ClassicScriptSyntaxEntry(key, text));
		}

		return result;
	}

	private static string? GetStringProperty(JsonElement element, string propertyName)
	{
		if (!element.TryGetProperty(propertyName, out JsonElement property) || property.ValueKind != JsonValueKind.String)
			return null;

		return property.GetString();
	}

	private static bool TryGetKind(string? kind, out ClassicScriptCommandKind result)
	{
		if (string.Equals(kind, "new", StringComparison.OrdinalIgnoreCase))
		{
			result = ClassicScriptCommandKind.New;
			return true;
		}

		if (string.Equals(kind, "old", StringComparison.OrdinalIgnoreCase))
		{
			result = ClassicScriptCommandKind.Old;
			return true;
		}

		if (string.Equals(kind, "customize", StringComparison.OrdinalIgnoreCase))
		{
			result = ClassicScriptCommandKind.Customize;
			return true;
		}

		if (string.Equals(kind, "parameter", StringComparison.OrdinalIgnoreCase))
		{
			result = ClassicScriptCommandKind.Parameter;
			return true;
		}

		result = default;
		return false;
	}
}

/// <summary>
/// Identifies the kind of a ClassicScript command.
/// </summary>
public enum ClassicScriptCommandKind
{
	/// <summary>
	/// A new-style command.
	/// </summary>
	New,

	/// <summary>
	/// An old-style command.
	/// </summary>
	Old,

	/// <summary>
	/// A customize command.
	/// </summary>
	Customize,

	/// <summary>
	/// A command parameter.
	/// </summary>
	Parameter
}

/// <summary>
/// Describes a single syntax variant of a command.
/// </summary>
/// <param name="Key">The lookup key of the syntax variant.</param>
/// <param name="Text">The display text of the syntax variant.</param>
public sealed record class ClassicScriptSyntaxEntry(string Key, string Text);

/// <summary>
/// Describes a single command entry in the catalog.
/// </summary>
/// <param name="Name">The command name.</param>
/// <param name="Kind">The command kind.</param>
/// <param name="Syntaxes">The syntax variants of the command.</param>
/// <param name="DescriptionKey">The optional description key when it differs from the name.</param>
public sealed record class ClassicScriptCommandEntry(
	string Name,
	ClassicScriptCommandKind Kind,
	IReadOnlyList<ClassicScriptSyntaxEntry> Syntaxes,
	string? DescriptionKey);

/// <summary>
/// Describes the loaded command catalog.
/// </summary>
/// <param name="Sections">The section names of the catalog.</param>
/// <param name="Commands">The command entries of the catalog.</param>
public sealed record class ClassicScriptCommandsCatalog(
	IReadOnlyList<string> Sections,
	IReadOnlyList<ClassicScriptCommandEntry> Commands);
