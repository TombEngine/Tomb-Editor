using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using System;
using System.Collections.Generic;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.GameFlowScript.Types;

namespace TombLib.Scripting.GameFlowScript.ContentNodes;

/// <summary>
/// Provides the document symbols (outline entries) for a GameFlow script document.
/// </summary>
public sealed class GameFlowNodesProvider : ITextDocumentSymbolProvider
{
	private readonly GameFlowContentNodeService _nodeService;

	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowNodesProvider"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to analyze document lines.</param>
	public GameFlowNodesProvider(IGameFlowScriptLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_nodeService = new(lineService);
	}

	/// <inheritdoc/>
	public IReadOnlyList<TextDocumentSymbol> GetSymbols(TextDocumentSymbolRequest request)
		=> DocumentSymbolTreeBuilder.BuildGroupedNodes(
			_nodeService.GetNodeGroups(request.DocumentText, request.Filter),
			group => group.Header,
			group => group.Nodes,
			node => node.Text,
			node => new GameFlowObjectDiscriminator(node.ObjectType));
}
