#nullable enable

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.Tomb1Main.Enums;

namespace TombLib.Scripting.Tomb1Main.Utils;

/// <summary>
/// Handles recursive schema processing and data extraction.
/// </summary>
public sealed class SchemaProcessor
{
	private readonly CompletionDataBuilder _builder;
	private readonly HashSet<JSchema> _processedSchemas = new();

	public SchemaProcessor(CompletionDataBuilder builder)
		=> _builder = builder;

	public void ProcessSchema(JSchema schema)
	{
		// Prevent infinite recursion
		if (!_processedSchemas.Add(schema))
			return;

		ExtractProperties(schema);
		ExtractEnumValues(schema);
		ExtractConstValue(schema);

		ProcessNestedSchemas(schema);
	}

	public void ProcessTopLevelPropertiesOnly(JSchema schema)
		=> ExtractProperties(schema);

	private void ExtractProperties(JSchema schema)
	{
		if (schema.Properties is null)
			return;

		foreach (var property in schema.Properties)
		{
			var text = $"\"{property.Key}\": ";
			var completionType = GetCompletionType(property.Value);
			var description = property.Value.Description ?? $"Property: {property.Key}";

			_builder.TryAdd(text, completionType, description);
		}
	}

	private void ExtractEnumValues(JSchema schema)
	{
		if (schema.Enum?.Count > 0)
		{
			var stringEnums = schema.Enum
				.Where(e => e.Type == JTokenType.String)
				.Select(e => $"\"{e}\"");

			foreach (var enumValue in stringEnums)
				_builder.TryAdd(enumValue, CompletionType.Constant);
		}
	}

	private void ExtractConstValue(JSchema schema)
	{
		if (schema.Const?.Type == JTokenType.String)
		{
			var constValue = $"\"{schema.Const}\"";
			_builder.TryAdd(constValue, CompletionType.Constant);
		}
	}

	private void ProcessNestedSchemas(JSchema schema)
	{
		ProcessSchemaProperties(schema);
		ProcessArrayItems(schema);

		ProcessSchemaCollection(schema.OneOf);
		ProcessSchemaCollection(schema.AnyOf);
		ProcessSchemaCollection(schema.AllOf);
	}

	private void ProcessSchemaProperties(JSchema schema)
	{
		if (schema.Properties is null)
			return;

		foreach (var property in schema.Properties.Values)
			ProcessSchema(property);
	}

	private void ProcessArrayItems(JSchema schema)
	{
		if (schema.Items?.Count > 0)
		{
			foreach (var item in schema.Items)
				ProcessSchema(item);
		}
	}

	private void ProcessSchemaCollection(IList<JSchema>? schemas)
	{
		if (schemas is null)
			return;

		foreach (var nestedSchema in schemas)
			ProcessSchema(nestedSchema);
	}

	private static CompletionType GetCompletionType(JSchema schema) => schema.Type switch
	{
		JSchemaType.Array => CompletionType.Array,
		_ => CompletionType.Property
	};
}
