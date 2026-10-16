using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.ClassicScript.Commands;

namespace TombLib.Scripting.ClassicScript.Syntaxes;

/// <summary>
/// Loads command syntax definitions from Commands.json (Resources/ClassicScript/Commands.json)
/// once and serves them as read-only lookups.
/// </summary>
public sealed class ClassicScriptSyntaxCatalogService
{
	private readonly ClassicScriptCommandsLoader _loader;
	private readonly Lazy<ClassicScriptSyntaxCatalogSnapshot> _snapshot;

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptSyntaxCatalogService"/> class.
	/// </summary>
	public ClassicScriptSyntaxCatalogService()
		: this(new ClassicScriptCommandsLoader())
	{ }

	internal ClassicScriptSyntaxCatalogService(ClassicScriptCommandsLoader loader)
	{
		_loader = loader;
		_snapshot = new Lazy<ClassicScriptSyntaxCatalogSnapshot>(LoadSnapshot);
	}

	/// <summary>
	/// Gets all command syntax definitions, old commands first, then new commands.
	/// </summary>
	public IReadOnlyList<ClassicScriptSyntaxDefinition> GetCommandSyntaxDefinitions() => _snapshot.Value.CommandDefinitions;

	/// <summary>
	/// Gets the syntax definition for the given command key, or null when the key is unknown.
	/// </summary>
	public ClassicScriptSyntaxDefinition? GetCommandDefinition(string key)
		=> GetDefinition(_snapshot.Value.CommandDefinitionsByKey, key);

	/// <summary>
	/// Gets the syntax text for the given command key, or null when the key is unknown.
	/// </summary>
	public string? GetCommandSyntax(string key)
		=> GetCommandDefinition(key)?.SyntaxText;

	/// <summary>
	/// Gets the syntax text for the given Customize command key, or null when the key is unknown.
	/// </summary>
	public string? GetCustomizeSyntax(string key)
		=> GetDefinition(_snapshot.Value.CustomizeSyntaxesByKey, key)?.SyntaxText;

	/// <summary>
	/// Gets the syntax text for the given Parameters command key, or null when the key is unknown.
	/// </summary>
	public string? GetParameterSyntax(string key)
		=> GetDefinition(_snapshot.Value.ParameterSyntaxesByKey, key)?.SyntaxText;

	private static ClassicScriptSyntaxDefinition? GetDefinition(
		IReadOnlyDictionary<string, ClassicScriptSyntaxDefinition> definitions,
		string key)
	{
		if (string.IsNullOrWhiteSpace(key))
			return null;

		return definitions.TryGetValue(key, out ClassicScriptSyntaxDefinition? definition) ? definition : null;
	}

	private ClassicScriptSyntaxCatalogSnapshot LoadSnapshot()
	{
		ClassicScriptCommandsCatalog catalog = _loader.Load();

		// Keep classic command syntaxes before new command syntaxes; the catalog and completion ordering depend on it.
		IReadOnlyList<ClassicScriptSyntaxDefinition> oldCommandDefinitions = CreateDefinitions(catalog, ClassicScriptCommandKind.Old);
		IReadOnlyList<ClassicScriptSyntaxDefinition> newCommandDefinitions = CreateDefinitions(catalog, ClassicScriptCommandKind.New);

		var commandDefinitions = new List<ClassicScriptSyntaxDefinition>(oldCommandDefinitions.Count + newCommandDefinitions.Count);
		commandDefinitions.AddRange(oldCommandDefinitions);
		commandDefinitions.AddRange(newCommandDefinitions);

		return new ClassicScriptSyntaxCatalogSnapshot(
			commandDefinitions,
			ToLookup(commandDefinitions),
			ToLookup(CreateDefinitions(catalog, ClassicScriptCommandKind.Customize)),
			ToLookup(CreateDefinitions(catalog, ClassicScriptCommandKind.Parameter)));
	}

	private static IReadOnlyList<ClassicScriptSyntaxDefinition> CreateDefinitions(
		ClassicScriptCommandsCatalog catalog,
		ClassicScriptCommandKind kind)
	{
		var definitions = new List<ClassicScriptSyntaxDefinition>();

		foreach (ClassicScriptCommandEntry entry in catalog.Commands)
		{
			if (entry.Kind != kind)
				continue;

			foreach (ClassicScriptSyntaxEntry syntax in entry.Syntaxes)
				definitions.Add(CreateDefinition(syntax.Key, syntax.Text));
		}

		return definitions;
	}

	private static ClassicScriptSyntaxDefinition CreateDefinition(string key, string syntaxText)
	{
		return new(
			key,
			syntaxText,
			GetApplicableSection(syntaxText),
			GetArgumentCount(syntaxText),
			ContainsArrayArgument(syntaxText));
	}

	private static bool ContainsArrayArgument(string syntaxText)
		=> syntaxText.Contains("ARRAY", StringComparison.OrdinalIgnoreCase);

	private static string GetApplicableSection(string syntaxText)
	{
		int sectionStart = syntaxText.IndexOf('[');
		int sectionEnd = syntaxText.IndexOf(']');

		if (sectionStart < 0 || sectionEnd <= sectionStart)
			return string.Empty;

		return syntaxText.Substring(sectionStart + 1, sectionEnd - sectionStart - 1).Trim();
	}

	private static int GetArgumentCount(string syntaxText)
	{
		int sectionEnd = syntaxText.IndexOf(']');
		string argumentsText = sectionEnd >= 0 ? syntaxText[(sectionEnd + 1)..].Trim() : syntaxText.Trim();

		if (string.IsNullOrWhiteSpace(argumentsText))
			return 0;

		return argumentsText.Split(',').Length;
	}

	private static IReadOnlyDictionary<string, ClassicScriptSyntaxDefinition> ToLookup(IReadOnlyList<ClassicScriptSyntaxDefinition> definitions)
		=> definitions.ToDictionary(definition => definition.Key, StringComparer.OrdinalIgnoreCase);

	private sealed record class ClassicScriptSyntaxCatalogSnapshot(
		IReadOnlyList<ClassicScriptSyntaxDefinition> CommandDefinitions,
		IReadOnlyDictionary<string, ClassicScriptSyntaxDefinition> CommandDefinitionsByKey,
		IReadOnlyDictionary<string, ClassicScriptSyntaxDefinition> CustomizeSyntaxesByKey,
		IReadOnlyDictionary<string, ClassicScriptSyntaxDefinition> ParameterSyntaxesByKey);
}
