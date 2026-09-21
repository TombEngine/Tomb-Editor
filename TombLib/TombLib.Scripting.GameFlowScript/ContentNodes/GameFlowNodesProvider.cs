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
	{
		IReadOnlyList<GameFlowContentNodeGroup> groups = _nodeService.GetNodeGroups(request.DocumentText, request.FilterText);

		// Group roots are modules and entries are variables, matching the outline vocabulary the editors display.
		var groupProjection = new DocumentSymbolProjection<GameFlowContentNodeGroup>(
			static group => group.Header,
			static _ => TextDocumentSymbolKind.Module);
		var itemProjection = new DocumentSymbolProjection<GameFlowContentNode>(
			static node => node.Text,
			static _ => TextDocumentSymbolKind.Variable,
			static node => new GameFlowObjectDiscriminator(node.ObjectType));

		return DocumentSymbolOutlineBuilder.BuildGroupedOutline(groups, groupProjection, static group => group.Nodes, itemProjection);
	}
}
