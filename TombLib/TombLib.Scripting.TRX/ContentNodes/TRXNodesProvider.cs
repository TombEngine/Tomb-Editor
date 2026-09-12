using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.ContentNodes;

/// <summary>
/// Provides the document symbols (outline entries) for level names found in TRX documents.
/// </summary>
public sealed class TRXNodesProvider : ITextDocumentSymbolProvider
{
	private static readonly Regex s_levelCommentRegex = new(Patterns.LevelCommentName, RegexOptions.IgnoreCase);

	private readonly ITRXLineService _lineService;

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXNodesProvider"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to strip comments from lines.</param>
	public TRXNodesProvider(ITRXLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_lineService = lineService;
	}

	/// <summary>
	/// Gets the level-name symbols for the supplied request.
	/// </summary>
	/// <param name="request">The document-symbol request.</param>
	/// <returns>The level-name symbols that match the filter.</returns>
	public IReadOnlyList<TextDocumentSymbol> GetSymbols(TextDocumentSymbolRequest request)
	{
		var nodes = new List<string>();
		var source = new StringTextSnapshot(request.DocumentText);

		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);
			string? levelNode = GetLevelNode(lineText, request.Filter);

			if (levelNode is not null)
				nodes.Add(levelNode);
		}

		return DocumentSymbolTreeBuilder.BuildFlatNodes(nodes, node => node);
	}

	private string? GetLevelNode(string lineText, string filter)
	{
		if (TRXLevelNameParser.LevelPropertyRegex.IsMatch(lineText))
		{
			string strippedLineText = _lineService.RemoveComments(lineText);
			string levelName = TRXLevelNameParser.ExtractTitleName(strippedLineText);

			if (!string.IsNullOrWhiteSpace(levelName) && levelName.Contains(filter, StringComparison.OrdinalIgnoreCase))
				return levelName;
		}

		// The fallback runs against the raw line text so that a level-name comment is still
		// discoverable when the line also matches the title property (malformed mixed input).
		Match regexMatch = s_levelCommentRegex.Match(lineText);

		if (regexMatch.Success)
		{
			string levelName = regexMatch.Groups[3].Value.Trim();

			if (!string.IsNullOrWhiteSpace(levelName) && levelName.Contains(filter, StringComparison.OrdinalIgnoreCase))
				return levelName;
		}

		return null;
	}
}
