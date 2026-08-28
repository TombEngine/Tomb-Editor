using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
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
			string schemaContent = File.ReadAllText(schemaFilePath);
			Model = BuildModel(JSchema.Parse(schemaContent));
			LoadState = TRXSchemaLoadState.Loaded;
		}
		catch (IOException exception)
		{
			LoadState = TRXSchemaLoadState.MissingResource;
			s_log.Warn(exception, "Failed to read the GameFlow schema at '{Path}'; schema-aware features are disabled.", schemaFilePath);
		}
		catch (Exception exception)
		{
			LoadState = TRXSchemaLoadState.InvalidSchema;
			s_log.Warn(exception, "Failed to parse the GameFlow schema at '{Path}'; schema-aware features are disabled.", schemaFilePath);
		}
	}

	private static TRXGameFlowSchemaModel BuildModel(JSchema schema)
	{
		var properties = new List<TRXGameFlowProperty>();
		var collections = new HashSet<string>();
		var propertyNames = new HashSet<string>();
		var constants = new HashSet<string>();

		// Highlighting keywords cover every schema reachable from the root.
		ExtractKeywords(schema, collections, propertyNames, constants);

		// Completion properties cover the root's direct properties.
		ExtractProperties(schema, properties);

		// Definitions and $defs contribute both keywords and completion properties. In-file $ref
		// targets are resolved by the schema reader, so referenced definitions are already
		// reachable; both sections are walked as-is to also surface definition-only content.
		ExtractDefinitions(schema.ExtensionData, collections, propertyNames, constants, properties);

		return new TRXGameFlowSchemaModel(
			properties,
			new TRXSchemaKeywords(collections.ToArray(), propertyNames.ToArray(), constants.ToArray()));
	}

	private static void ExtractDefinitions(
		IDictionary<string, JToken>? extensionData,
		HashSet<string> collections,
		HashSet<string> propertyNames,
		HashSet<string> constants,
		List<TRXGameFlowProperty> properties)
	{
		if (extensionData is null)
			return;

		string[] sectionNames = ["definitions", "$defs"];

		foreach (string sectionName in sectionNames)
		{
			if (!extensionData.TryGetValue(sectionName, out JToken? section) || section is not JObject definitions)
				continue;

			foreach (var definition in definitions)
			{
				var definitionSchema = definition.Value?.ToObject<JSchema>();

				if (definitionSchema is null)
					continue;

				ExtractKeywords(definitionSchema, collections, propertyNames, constants);

				foreach (JSchema nested in SchemaTraversal.FlattenSchemas(definitionSchema))
					ExtractProperties(nested, properties);
			}
		}
	}

	private static void ExtractKeywords(JSchema schema, HashSet<string> collections, HashSet<string> properties, HashSet<string> constants)
	{
		foreach (JSchema currentSchema in SchemaTraversal.FlattenSchemas(schema))
		{
			// Extract string const values at any level.
			if (currentSchema.Const is not null && currentSchema.Const.Type == JTokenType.String)
				constants.Add(currentSchema.Const.ToString());

			// Extract string enum values at any level.
			if (currentSchema.Enum is not null)
			{
				foreach (var enumValue in currentSchema.Enum)
				{
					if (enumValue.Type == JTokenType.String)
						constants.Add(enumValue.ToString());
				}
			}

			// Classify the schema's own properties.
			if (currentSchema.Properties is not null)
			{
				foreach (var property in currentSchema.Properties)
				{
					if (property.Value.Type == JSchemaType.Array)
						collections.Add(property.Key);
					else
						properties.Add(property.Key);
				}
			}
		}
	}

	private static void ExtractProperties(JSchema schema, List<TRXGameFlowProperty> result)
	{
		if (schema.Properties is null)
			return;

		foreach (var property in schema.Properties)
		{
			// The same name can be reachable through the root and through a referenced
			// definition; the first occurrence (the root context) wins.
			if (result.Any(candidate => candidate.Name == property.Key))
				continue;

			result.Add(new TRXGameFlowProperty(
				property.Key,
				ToPropertyTypes(property.Value.Type),
				property.Value.Description));
		}
	}

	private static IReadOnlyList<TRXGameFlowPropertyType> ToPropertyTypes(JSchemaType? type)
	{
		if (type is null)
			return [];

		var result = new List<TRXGameFlowPropertyType>();

		if (type.Value.HasFlag(JSchemaType.String))
			result.Add(TRXGameFlowPropertyType.String);

		if (type.Value.HasFlag(JSchemaType.Number))
			result.Add(TRXGameFlowPropertyType.Number);

		if (type.Value.HasFlag(JSchemaType.Integer))
			result.Add(TRXGameFlowPropertyType.Integer);

		if (type.Value.HasFlag(JSchemaType.Boolean))
			result.Add(TRXGameFlowPropertyType.Boolean);

		if (type.Value.HasFlag(JSchemaType.Object))
			result.Add(TRXGameFlowPropertyType.Object);

		if (type.Value.HasFlag(JSchemaType.Array))
			result.Add(TRXGameFlowPropertyType.Array);

		if (type.Value.HasFlag(JSchemaType.Null))
			result.Add(TRXGameFlowPropertyType.Null);

		return result;
	}
}
