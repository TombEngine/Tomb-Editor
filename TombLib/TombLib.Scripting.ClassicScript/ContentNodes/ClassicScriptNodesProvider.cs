using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using System.Collections.Generic;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Types;

namespace TombLib.Scripting.ClassicScript.ContentNodes;

/// <summary>
/// Provides the document symbols (outline entries) for a ClassicScript document.
/// </summary>
public sealed class ClassicScriptNodesProvider : ITextDocumentSymbolProvider
{
	private readonly ClassicScriptContentNodeService _nodeService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptNodesProvider"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to analyze document lines.</param>
	public ClassicScriptNodesProvider(IClassicScriptLineService lineService)
	{
		_nodeService = new ClassicScriptContentNodeService(lineService);
	}

	/// <inheritdoc/>
	public IReadOnlyList<TextDocumentSymbol> GetSymbols(TextDocumentSymbolRequest request)
		=> DocumentSymbolTreeBuilder.BuildGroupedNodes(
			_nodeService.GetNodeGroups(request.DocumentText, request.Filter),
			group => group.Header,
			group => group.Nodes,
			node => node.Text,
			node => new ClassicScriptObjectDiscriminator(node.ObjectType));
}
