using Nickelony.IDEKit.IntelliSense.Completion;
using System.Collections.Generic;

namespace TombLib.Scripting.TRX.Completion;

/// <summary>
/// Helper class to build completion data while avoiding duplicates.
/// </summary>
public sealed class CompletionDataBuilder
{
	private readonly List<TextCompletionItem> _data = new();
	private readonly HashSet<string> _addedTexts = new();

	/// <summary>
	/// Adds a completion item with the given text, kind and description when it is not already present.
	/// </summary>
	/// <param name="text">The text of the completion item.</param>
	/// <param name="kind">The completion item kind.</param>
	/// <param name="description">The optional completion item description.</param>
	/// <returns>True if the item was added; otherwise false if a duplicate was rejected.</returns>
	public bool TryAdd(string text, TextCompletionItemKind? kind = null, string? description = null)
	{
		if (!_addedTexts.Add(text))
			return false;

		_data.Add(new TextCompletionItem(text, text, description, kind: kind ?? TextCompletionItemKind.Generic));
		return true;
	}

	/// <summary>
	/// Builds the collected completion items.
	/// </summary>
	/// <returns>The collected completion items.</returns>
	public IReadOnlyList<TextCompletionItem> Build() => _data;
}
