using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TombLib.Scripting.GameFlowScript;

/// <summary>
/// Describes the set of GameFlow definitions. Instances are immutable; caller-owned collections are
/// copied into owned read-only storage at construction and can never be mutated by consumers.
/// </summary>
public sealed class GameFlowDefinitionSet
{
	/// <summary>
	/// Gets an empty definition set used when no definitions are available.
	/// </summary>
	public static GameFlowDefinitionSet Empty { get; } = new([], [], [], []);

	/// <summary>
	/// Gets the special property names.
	/// </summary>
	public IReadOnlyList<string> SpecialProperties { get; }

	/// <summary>
	/// Gets the section names.
	/// </summary>
	public IReadOnlyList<string> Sections { get; }

	/// <summary>
	/// Gets the constant names.
	/// </summary>
	public IReadOnlyList<string> Constants { get; }

	/// <summary>
	/// Gets the property names.
	/// </summary>
	public IReadOnlyList<string> Properties { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowDefinitionSet"/> class.
	/// </summary>
	/// <param name="specialProperties">The special property names.</param>
	/// <param name="sections">The section names.</param>
	/// <param name="constants">The constant names.</param>
	/// <param name="properties">The property names.</param>
	[JsonConstructor]
	public GameFlowDefinitionSet(
		IReadOnlyList<string> specialProperties,
		IReadOnlyList<string> sections,
		IReadOnlyList<string> constants,
		IReadOnlyList<string> properties)
	{
		SpecialProperties = CopyOwned(specialProperties);
		Sections = CopyOwned(sections);
		Constants = CopyOwned(constants);
		Properties = CopyOwned(properties);
	}

	private static IReadOnlyList<string> CopyOwned(IReadOnlyList<string> source)
		=> Array.AsReadOnly([.. (source ?? [])]);
}
