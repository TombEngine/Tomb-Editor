using Nickelony.IDEKit.IntelliSense.Completion;
using System;
using System.Collections.Generic;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Scripting.GameFlowScript.Completion;

/// <summary>
/// Builds completion items from the GameFlow definition catalog.
/// </summary>
public sealed class GameFlowCompletionProvider : ITextCompletionProvider
{
	/// <summary>
	/// Gets the completion items for the given context.
	/// </summary>
	/// <param name="context">The completion context.</param>
	/// <returns>The full set of contextually valid completion items.</returns>
	public IReadOnlyList<TextCompletionItem> GetCompletionItems(TextCompletionContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		var items = new List<TextCompletionItem>();
		var seenInsertionTexts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		AddItems(items, seenInsertionTexts, GameFlowDefinitionCatalog.Sections, ": ");
		AddItems(items, seenInsertionTexts, GameFlowDefinitionCatalog.SpecialProperties, ": ");
		AddItems(items, seenInsertionTexts, GameFlowDefinitionCatalog.Properties, ": ");
		AddItems(items, seenInsertionTexts, GameFlowDefinitionCatalog.Constants, string.Empty);

		// The provider returns the full contextually-valid candidate set; the session
		// coordinator owns word filtering so candidates are not filtered twice.
		return items;
	}

	private static void AddItems(List<TextCompletionItem> items, HashSet<string> seenInsertionTexts, IReadOnlyList<string> values, string suffix)
	{
		foreach (string value in values)
		{
			string insertionText = value + suffix;

			// A name that belongs to more than one category keeps its first insertion text, so the
			// intended category (sections, special properties, properties, then constants) wins.
			if (seenInsertionTexts.Add(insertionText))
				items.Add(new TextCompletionItem(value, insertionText));
		}
	}
}
