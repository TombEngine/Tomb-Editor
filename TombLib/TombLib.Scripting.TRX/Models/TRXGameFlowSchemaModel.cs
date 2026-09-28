using System;
using System.Collections.Generic;

namespace TombLib.Scripting.TRX.Models;

/// <summary>
/// Immutable, library-neutral view of the GameFlow JSON schema, containing only the information
/// required by completion, hover and highlighting. Instances are produced once by the schema
/// service and must never be mutated by consumers. Caller-owned collections are copied into
/// read-only owned storage by the constructor.
/// </summary>
public sealed class TRXGameFlowSchemaModel
{
	/// <summary>
	/// Gets all properties declared by the schema, including those reachable through its
	/// definitions. Array and object classification is available on each property.
	/// </summary>
	public IReadOnlyList<TRXGameFlowProperty> Properties { get; }

	/// <summary>
	/// Gets the schema-derived keyword categories used by syntax highlighting.
	/// </summary>
	public TRXSchemaKeywords Keywords { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXGameFlowSchemaModel"/> class.
	/// </summary>
	/// <param name="properties">The schema properties.</param>
	/// <param name="keywords">The schema-derived keyword categories.</param>
	public TRXGameFlowSchemaModel(IReadOnlyList<TRXGameFlowProperty> properties, TRXSchemaKeywords keywords)
	{
		ArgumentNullException.ThrowIfNull(properties);
		ArgumentNullException.ThrowIfNull(keywords);

		// Copy into read-only owned storage so later caller mutations cannot leak into the model.
		Properties = Array.AsReadOnly([.. properties]);
		Keywords = keywords;
	}
}
