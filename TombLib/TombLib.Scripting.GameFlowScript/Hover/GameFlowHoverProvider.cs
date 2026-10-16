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
			return CreateHoverInfo($"GameFlow section \"{hoveredWord}\".", hoveredWord, ObjectType.Section);

		if (Contains(GameFlowDefinitionCatalog.SpecialProperties, hoveredWord))
			return CreateHoverInfo($"GameFlow special property \"{hoveredWord}\".", hoveredWord, ObjectType.SpecialProperty);

		if (Contains(GameFlowDefinitionCatalog.Properties, hoveredWord))
			return CreateHoverInfo($"GameFlow property \"{hoveredWord}\".", hoveredWord, ObjectType.Property);

		if (Contains(GameFlowDefinitionCatalog.Constants, hoveredWord))
			return CreateHoverInfo($"GameFlow constant \"{hoveredWord}\".", hoveredWord, ObjectType.Constant);

		return null;
	}

	private static TextHoverInfo CreateHoverInfo(string content, string symbolName, ObjectType objectType)
		=> new(content)
		{
			SymbolName = symbolName,
			DefinitionDiscriminator = new GameFlowObjectDiscriminator(objectType)
		};

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
		TextRange? range = IdentifierOperations.FindTokenSpan(snapshot, offset, IdentifierCharacterPolicy.Default);

		if (range is null)
			return null;

			return range.Value.GetTextFrom(snapshot).Trim();
	}
}
