using System;
using System.Collections.Generic;

namespace TombLib.Scripting.TRX.Models;

/// <summary>
/// Immutable keyword categories derived from the GameFlow schema. Caller-owned collections are
/// copied into read-only owned storage by the constructor.
/// </summary>
public sealed class TRXSchemaKeywords
{
	/// <summary>
	/// Gets an empty keyword set, used when no schema is available.
	/// </summary>
	public static TRXSchemaKeywords Empty { get; } = new([], [], []);

	/// <summary>
	/// Gets the collection keywords (properties declared as arrays).
	/// </summary>
	public IReadOnlyList<string> Collections { get; }

	/// <summary>
	/// Gets the property keywords (properties not declared as arrays).
	/// </summary>
	public IReadOnlyList<string> Properties { get; }

	/// <summary>
	/// Gets the constant keywords (string enum and const values).
	/// </summary>
	public IReadOnlyList<string> Constants { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXSchemaKeywords"/> class.
	/// </summary>
	/// <param name="collections">The collection keywords.</param>
	/// <param name="properties">The property keywords.</param>
	/// <param name="constants">The constant keywords.</param>
	public TRXSchemaKeywords(IReadOnlyList<string> collections, IReadOnlyList<string> properties, IReadOnlyList<string> constants)
	{
		ArgumentNullException.ThrowIfNull(collections);
		ArgumentNullException.ThrowIfNull(properties);
		ArgumentNullException.ThrowIfNull(constants);

		// Copy into read-only owned storage so later caller mutations cannot leak into the keyword sets.
		Collections = Array.AsReadOnly([.. collections]);
		Properties = Array.AsReadOnly([.. properties]);
		Constants = Array.AsReadOnly([.. constants]);
	}
}
