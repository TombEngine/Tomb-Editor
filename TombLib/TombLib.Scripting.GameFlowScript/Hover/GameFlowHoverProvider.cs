using Nickelony.IDEKit.Core.Identifiers;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Hover;
using System;
using System.Collections.Generic;
using TombLib.Scripting.GameFlowScript.Types;

namespace TombLib.Scripting.GameFlowScript.Hover;

/// <summary>
/// Resolves hover information for GameFlow definitions.
/// </summary>
public sealed class GameFlowHoverProvider : ITextHoverProvider
{
	/// <summary>
	/// Gets the hover information for the given request.
	/// </summary>
	/// <param name="request">The hover request.</param>
	/// <returns>The hover information, or <c>null</c> when the hovered word is not a known definition.</returns>
	public TextHoverInfo? GetHoverInfo(TextHoverRequest request)
	{
		string? hoveredWord = GetWordFromOffset(request.DocumentText, request.HoveredOffset);

		if (string.IsNullOrWhiteSpace(hoveredWord))
			return null;

		if (Contains(GameFlowDefinitionCatalog.Sections, hoveredWord))
			return new TextHoverInfo($"GameFlow section \"{hoveredWord}\".", SymbolName: hoveredWord, Identifier: new GameFlowObjectDiscriminator(ObjectType.Section));

		if (Contains(GameFlowDefinitionCatalog.SpecialProperties, hoveredWord))
			return new TextHoverInfo($"GameFlow special property \"{hoveredWord}\".", SymbolName: hoveredWord, Identifier: new GameFlowObjectDiscriminator(ObjectType.SpecialProperty));

		if (Contains(GameFlowDefinitionCatalog.Properties, hoveredWord))
			return new TextHoverInfo($"GameFlow property \"{hoveredWord}\".", SymbolName: hoveredWord, Identifier: new GameFlowObjectDiscriminator(ObjectType.Property));

		if (Contains(GameFlowDefinitionCatalog.Constants, hoveredWord))
			return new TextHoverInfo($"GameFlow constant \"{hoveredWord}\".", SymbolName: hoveredWord, Identifier: new GameFlowObjectDiscriminator(ObjectType.Constant));

		return null;
	}

	private static bool Contains(IReadOnlyList<string> values, string value)
	{
		for (int index = 0; index < values.Count; index++)
		{
			if (string.Equals(values[index], value, StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	// The word is the default identifier token (letters, digits, underscores); the GameFlow
	// definition catalogs only contain identifier-like symbols. Probes on whitespace or
	// punctuation resolve to the adjacent identifier or no word, instead of forming a
	// punctuation-only run.
	private static string? GetWordFromOffset(string documentText, int offset)
	{
		if (offset < 0 || offset > documentText.Length)
			return null;

		var snapshot = new StringTextSnapshot(documentText);
		TextRange? range = IdentifierHelper.TryGetContainingSpan(snapshot, offset, IdentifierCharacterPolicy.Default);

		if (range is null)
			return null;

		return snapshot.GetText(range.Value.Offset, range.Value.Length).Trim();
	}
}
