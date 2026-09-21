using Nickelony.IDEKit.IntelliSense.Completion;
using NLog;
using System;
using System.Collections.Generic;
using TombLib.Scripting.UI.Completion;
using TombLib.Scripting.TRX.Models;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Completion;

/// <summary>
/// Builds completion items from the GameFlow JSON schema.
/// </summary>
public sealed class TRXGameFlowCompletionService : ITextCompletionProvider
{
	private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

	private readonly ITRXGameFlowSchemaService _schemaService;

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXGameFlowCompletionService"/> class.
	/// </summary>
	/// <param name="schemaService">The schema service used to source the GameFlow schema.</param>
	public TRXGameFlowCompletionService(ITRXGameFlowSchemaService schemaService) => _schemaService = schemaService;

	/// <inheritdoc/>
	public IReadOnlyList<TextCompletionItem> GetCompletionItems(TextCompletionRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		try
		{
			var completionBuilder = new CompletionDataBuilder();

			// Schema-driven items are only available when the schema model loaded successfully;
			// JSON primitives are always valid completions regardless of schema availability.
			if (_schemaService.Model is { } model)
			{
				AddSchemaProperties(completionBuilder, model);
				AddSchemaConstants(completionBuilder, model);
			}

			AddJsonPrimitives(completionBuilder);

			// The provider returns the full contextually-valid candidate set; the session
			// coordinator owns word filtering so candidates are not filtered twice.
			return completionBuilder.Build();
		}
		catch (Exception exception)
		{
			s_log.Warn(exception, "Failed to build GameFlow completion items; returning an empty list.");
			return [];
		}
	}

	private static void AddSchemaProperties(CompletionDataBuilder builder, TRXGameFlowSchemaModel model)
	{
		foreach (TRXGameFlowProperty property in model.Properties)
		{
			var text = $"\"{property.Name}\": ";
			var completionKind = property.IsArray ? TextCompletionItemKind.Array : TextCompletionItemKind.Property;
			var description = property.Description ?? $"Property: {property.Name}";

			builder.TryAdd(text, completionKind, description);
		}
	}

	private static void AddSchemaConstants(CompletionDataBuilder builder, TRXGameFlowSchemaModel model)
	{
		foreach (string constant in model.Keywords.Constants)
			builder.TryAdd($"\"{constant}\"", TextCompletionItemKind.Constant);
	}

	private static void AddJsonPrimitives(CompletionDataBuilder builder)
	{
		foreach (string primitive in Keywords.Values)
			builder.TryAdd(primitive, TextCompletionItemKind.Constant);
	}
}
