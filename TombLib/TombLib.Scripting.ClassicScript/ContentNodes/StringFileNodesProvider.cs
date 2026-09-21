using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using System;
using System.Collections.Generic;
using TombLib.Scripting.ClassicScript.Services;

namespace TombLib.Scripting.ClassicScript.ContentNodes;

/// <summary>
/// Provides the document symbols (outline entries) for a ClassicScript strings file.
/// </summary>
public sealed class StringFileNodesProvider : ITextDocumentSymbolProvider
{
	private readonly IClassicScriptLineService _lineService;

	/// <summary>
	/// Initializes a new instance of the <see cref="StringFileNodesProvider"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to identify section headers.</param>
	public StringFileNodesProvider(IClassicScriptLineService lineService)
		=> _lineService = lineService;

	/// <summary>
	/// Gets the section-header symbols for the supplied request.
	/// </summary>
	/// <param name="request">The document-symbol request.</param>
	/// <returns>The section-header symbols that match the filter.</returns>
	public IReadOnlyList<TextDocumentSymbol> GetSymbols(TextDocumentSymbolRequest request)
	{
		var nodes = new List<string>();
		var source = new StringTextSnapshot(request.DocumentText);

		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);

			if (_lineService.IsSectionHeaderLine(lineText))
			{
				string? headerText = _lineService.GetSectionHeaderText(lineText);

				if (headerText is not null && headerText.Contains(request.FilterText, StringComparison.OrdinalIgnoreCase))
					nodes.Add(headerText);
			}
		}

		return DocumentSymbolOutlineBuilder.BuildFlatOutline(
			nodes,
			new DocumentSymbolProjection<string>(static node => node, static _ => TextDocumentSymbolKind.Variable));
	}
}
