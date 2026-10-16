using System;
using System.Collections.Generic;

namespace TombLib.Scripting.TRX.Models;

/// <summary>
/// Immutable, library-neutral description of a single GameFlow schema property. Caller-owned
/// collections are copied into read-only owned storage by the constructor.
/// </summary>
public sealed class TRXGameFlowProperty
{
	/// <summary>
	/// Gets the property name.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the JSON types the property declares, or empty when the schema declares none.
	/// </summary>
	public IReadOnlyList<TRXGameFlowPropertyType> Types { get; }

	/// <summary>
	/// Gets the property description, or null when the schema declares none.
	/// </summary>
	public string? Description { get; }

	/// <summary>
	/// Gets whether the property is declared exactly as an array.
	/// </summary>
	public bool IsArray => Types.Count == 1 && Types[0] == TRXGameFlowPropertyType.Array;

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXGameFlowProperty"/> class.
	/// </summary>
	/// <param name="name">The property name.</param>
	/// <param name="types">The declared JSON types.</param>
	/// <param name="description">The property description.</param>
	public TRXGameFlowProperty(string name, IReadOnlyList<TRXGameFlowPropertyType> types, string? description)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(types);

		Name = name;

		// Copy into read-only owned storage so later caller mutations cannot leak into the property.
		Types = Array.AsReadOnly([.. types]);
		Description = description;
	}
}
