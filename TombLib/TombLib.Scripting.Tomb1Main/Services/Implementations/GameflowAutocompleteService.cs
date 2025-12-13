#nullable enable

using ICSharpCode.AvalonEdit.CodeCompletion;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using TombLib.Scripting.Tomb1Main.Utils;

namespace TombLib.Scripting.Tomb1Main.Services.Implementations;

public sealed class GameflowAutocompleteService : IGameflowAutocompleteService
{
	private static readonly string[] JsonPrimitives = { "true", "false", "null" };

	private readonly IGameflowSchemaService _schemaService;

	public GameflowAutocompleteService(IGameflowSchemaService schemaService)
		=> _schemaService = schemaService;

	public List<ICompletionData> GetAutocompleteData()
	{
		try
		{
			var completionBuilder = new CompletionDataBuilder();
			var schema = _schemaService.Schema;

			if (schema is null || schema.Properties is null)
				return completionBuilder.Build();

			// Process schema data
			ProcessTopLevelProperties(completionBuilder, schema);
			ProcessDefinitions(completionBuilder, schema);
			AddJsonPrimitives(completionBuilder);

			return completionBuilder.Build();
		}
		catch
		{
			return new List<ICompletionData>();
		}
	}

	private static void ProcessTopLevelProperties(CompletionDataBuilder builder, JSchema schema)
	{
		var schemaProcessor = new SchemaProcessor(builder);
		schemaProcessor.ProcessTopLevelPropertiesOnly(schema);
	}

	private static void ProcessDefinitions(CompletionDataBuilder builder, JSchema schema)
	{
		if (schema.ExtensionData?.ContainsKey("definitions") != true)
			return;

		if (schema.ExtensionData["definitions"] is not JObject definitions)
			return;

		foreach (var definition in definitions)
		{
			var defSchema = definition.Value?.ToObject<JSchema>();

			if (defSchema is null)
				continue;

			var schemaProcessor = new SchemaProcessor(builder);
			schemaProcessor.ProcessSchema(defSchema);
		}
	}

	private static void AddJsonPrimitives(CompletionDataBuilder builder)
	{
		foreach (var primitive in JsonPrimitives)
			builder.TryAdd(primitive);
	}
}
