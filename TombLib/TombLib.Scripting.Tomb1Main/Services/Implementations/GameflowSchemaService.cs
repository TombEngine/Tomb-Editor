#nullable enable

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.Scripting.Tomb1Main.Objects;

namespace TombLib.Scripting.Tomb1Main.Services.Implementations;

public sealed class GameflowSchemaService : IGameflowSchemaService
{
	public static IGameflowSchemaService? Instance { get; set; } // Dirty singleton for easy access in settings - TODO: Refactor later

	public JSchema? Schema { get; }

	public GameflowSchemaService(string schemaFilePath)
	{
		try
		{
			string schemaContent = File.ReadAllText(schemaFilePath);
			Schema = JSchema.Parse(schemaContent);
		}
		catch
		{
			Schema = null;
		}
	}

	public SchemaKeywords? GetSchemaKeywords()
	{
		var schema = Schema;

		if (schema is null)
			return null;

		var collections = new HashSet<string>();
		var properties = new HashSet<string>();
		var constants = new HashSet<string>();

		// Extract from the main schema
		ExtractKeywordsRecursively(schema, collections, properties, constants);

		// Also extract from definitions section
		if (schema.ExtensionData?.ContainsKey("definitions") == true)
		{
			var definitions = schema.ExtensionData["definitions"] as JObject;

			if (definitions is not null)
			{
				foreach (var definition in definitions)
				{
					var defSchema = definition.Value?.ToObject<JSchema>();

					if (defSchema is not null)
						ExtractKeywordsRecursively(defSchema, collections, properties, constants);
				}
			}
		}

		return new SchemaKeywords
		{
			Collections = collections.ToArray(),
			Properties = properties.ToArray(),
			Constants = constants.ToArray()
		};
	}

	private static void ExtractKeywordsRecursively(JSchema schema, HashSet<string> collections, HashSet<string> properties, HashSet<string> constants)
	{
		// Extract top-level properties
		if (schema.Properties is not null)
		{
			foreach (var property in schema.Properties)
			{
				if (property.Value.Type == JSchemaType.Array)
					collections.Add(property.Key);
				else
					properties.Add(property.Key);

				// Extract enum values as constants
				if (property.Value.Enum is not null)
				{
					foreach (var enumValue in property.Value.Enum)
					{
						if (enumValue.Type == JTokenType.String)
							constants.Add(enumValue.ToString());
					}
				}

				// Extract const values as constants
				if (property.Value.Const is not null && property.Value.Const.Type == JTokenType.String)
					constants.Add(property.Value.Const.ToString());

				// Recursively process nested schemas
				ExtractKeywordsRecursively(property.Value, collections, properties, constants);
			}
		}

		// Extract from array items
		if (schema.Items?.Count > 0)
		{
			foreach (var item in schema.Items)
				ExtractKeywordsRecursively(item, collections, properties, constants);
		}

		// Extract from oneOf/anyOf schemas
		if (schema.OneOf is not null)
		{
			foreach (var oneOfSchema in schema.OneOf)
				ExtractKeywordsRecursively(oneOfSchema, collections, properties, constants);
		}

		if (schema.AnyOf is not null)
		{
			foreach (var anyOfSchema in schema.AnyOf)
				ExtractKeywordsRecursively(anyOfSchema, collections, properties, constants);
		}

		// Extract from allOf schemas
		if (schema.AllOf is not null)
		{
			foreach (var allOfSchema in schema.AllOf)
				ExtractKeywordsRecursively(allOfSchema, collections, properties, constants);
		}

		// Extract const values at any level
		if (schema.Const is not null && schema.Const.Type == JTokenType.String)
			constants.Add(schema.Const.ToString());

		// Extract enum values at any level
		if (schema.Enum is not null)
		{
			foreach (var enumValue in schema.Enum)
			{
				if (enumValue.Type == JTokenType.String)
					constants.Add(enumValue.ToString());
			}
		}
	}
}
