using Nickelony.IDEKit.JsonSchema;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.Scripting.TRX.Models;

namespace TombLib.Scripting.TRX.Services;

/// <summary>
/// Loads the GameFlow level schema once and exposes the derived immutable schema model and
/// keyword categories. Schema loading failures are recoverable editor configuration errors:
/// the service records the load state and schema-aware features degrade to their non-schema
/// behavior when the schema is unavailable.
/// </summary>
public sealed class TRXGameFlowSchemaService : ITRXGameFlowSchemaService
{
	private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

	/// <inheritdoc/>
	public TRXSchemaLoadState LoadState { get; }

	/// <inheritdoc/>
	public TRXGameFlowSchemaModel? Model { get; }

	/// <inheritdoc/>
	public TRXSchemaKeywords Keywords => Model?.Keywords ?? TRXSchemaKeywords.Empty;

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXGameFlowSchemaService"/> class.
	/// </summary>
	/// <param name="schemaFilePath">The path of the schema file to load.</param>
	public TRXGameFlowSchemaService(string schemaFilePath)
	{
		try
		{
			using var reader = new StreamReader(schemaFilePath);
			JsonSchemaVocabularyIndexResult result = new JsonSchemaVocabularyIndexBuilder().Build(reader);

			if (result.Succeeded)
			{
				Model = BuildModel(result.Index!);
				LoadState = TRXSchemaLoadState.Loaded;
			}
			else
			{
				LoadState = TRXSchemaLoadState.InvalidSchema;
				s_log.Warn("Failed to parse the GameFlow schema at '{Path}'; schema-aware features are disabled. {Diagnostics}",
					schemaFilePath, string.Join(" ", result.Diagnostics));
			}
		}
		catch (IOException exception)
		{
			LoadState = TRXSchemaLoadState.MissingResource;
			s_log.Warn(exception, "Failed to read the GameFlow schema at '{Path}'; schema-aware features are disabled.", schemaFilePath);
		}
		catch (Exception exception)
		{
			LoadState = TRXSchemaLoadState.InvalidSchema;
			s_log.Warn(exception, "Failed to load the GameFlow schema at '{Path}'; schema-aware features are disabled.", schemaFilePath);
		}
	}

	private static TRXGameFlowSchemaModel BuildModel(JsonSchemaVocabularyIndex index)
	{
		var properties = new List<TRXGameFlowProperty>();

		foreach (JsonSchemaVocabularyPropertyDescriptor descriptor in index.Properties)
		{
			properties.Add(new TRXGameFlowProperty(
				descriptor.Name,
				ToPropertyTypes(descriptor.Types),
				descriptor.Description));
		}

		// The schema index classifies only the array shape; the TRX keyword categories
		// (collections vs properties) derive from that classification here.
		var keywords = new TRXSchemaKeywords(
			index.Properties.Where(property => property.IsArray).Select(property => property.Name).ToArray(),
			index.Properties.Where(property => !property.IsArray).Select(property => property.Name).ToArray(),
			index.Constants.ToArray());

		return new TRXGameFlowSchemaModel(properties, keywords);
	}

	private static IReadOnlyList<TRXGameFlowPropertyType> ToPropertyTypes(IReadOnlyList<JsonSchemaPropertyType> types)
	{
		var result = new List<TRXGameFlowPropertyType>(types.Count);

		foreach (JsonSchemaPropertyType type in types)
			result.Add(MapType(type));

		return result;

		static TRXGameFlowPropertyType MapType(JsonSchemaPropertyType type)
			=> type switch
			{
				JsonSchemaPropertyType.Object => TRXGameFlowPropertyType.Object,
				JsonSchemaPropertyType.Array => TRXGameFlowPropertyType.Array,
				JsonSchemaPropertyType.String => TRXGameFlowPropertyType.String,
				JsonSchemaPropertyType.Integer => TRXGameFlowPropertyType.Integer,
				JsonSchemaPropertyType.Number => TRXGameFlowPropertyType.Number,
				JsonSchemaPropertyType.Boolean => TRXGameFlowPropertyType.Boolean,
				JsonSchemaPropertyType.Null => TRXGameFlowPropertyType.Null,
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};
	}
}
