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
	{
		IReadOnlyList<ClassicScriptContentNodeGroup> groups = _nodeService.GetNodeGroups(request.DocumentText, request.FilterText);

		// Group roots are modules and entries are variables, matching the outline vocabulary the editors display.
		var groupProjection = new DocumentSymbolProjection<ClassicScriptContentNodeGroup>(
			static group => group.Header,
			static _ => TextDocumentSymbolKind.Module);
		var itemProjection = new DocumentSymbolProjection<ClassicScriptContentNode>(
			static node => node.Text,
			static _ => TextDocumentSymbolKind.Variable,
			static node => new ClassicScriptObjectDiscriminator(node.ObjectType));

		return DocumentSymbolOutlineBuilder.BuildGroupedOutline(groups, groupProjection, static group => group.Nodes, itemProjection);
	}
}
