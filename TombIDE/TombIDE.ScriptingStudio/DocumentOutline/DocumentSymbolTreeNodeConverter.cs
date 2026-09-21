#nullable enable

using DarkUI.Controls;
using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using System.Collections.Generic;

namespace TombIDE.ScriptingStudio.DocumentOutline;

/// <summary>
/// Converts neutral <see cref="TextDocumentSymbol"/> trees into host <see cref="DarkTreeNode"/> trees.
/// </summary>
/// <remarks>
/// This is the host-side DarkUI conversion for the content-explorer outline: language libraries
/// produce protocol-neutral document symbols and the host projects them onto its DarkUI tree model
/// here, which keeps DarkUI out of the language libraries.
/// </remarks>
internal static class DocumentSymbolTreeNodeConverter
{
	/// <summary>
	/// Converts a document-symbol tree into a list of tree nodes.
	/// </summary>
	/// <param name="symbols">The document symbols to convert.</param>
	/// <returns>The converted tree nodes.</returns>
	public static IReadOnlyList<DarkTreeNode> ToTreeNodes(IReadOnlyList<TextDocumentSymbol> symbols)
	{
		var result = new List<DarkTreeNode>(symbols.Count);

		foreach (TextDocumentSymbol symbol in symbols)
			result.Add(ToTreeNode(symbol));

		return result;
	}

	private static DarkTreeNode ToTreeNode(TextDocumentSymbol symbol)
	{
		var node = new DarkTreeNode(symbol.Name)
		{
			Expanded = symbol.Children.Count > 0,
			Tag = symbol.Data
		};

		foreach (TextDocumentSymbol child in symbol.Children)
			node.Nodes.Add(ToTreeNode(child));

		return node;
	}
}
