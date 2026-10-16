using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TombLib.Scripting.Lua.ContentNodes;

/// <summary>
/// Provides the document symbols (outline entries) for a Lua document from the live language-server session.
/// </summary>
/// <remarks>
/// <para>
/// The outline surface is synchronous while the language-server request is asynchronous, so
/// <see cref="GetSymbols"/> waits on the request synchronously. The outline refresh runs on a worker
/// thread and the language-server provider bounds the request with its own timeout, so the wait cannot
/// block the UI thread. The adapter instance is bound to one document path and is created per document.
/// </para>
/// <para>
/// The filter text narrows the returned tree by name (case-insensitive containment); a matching
/// descendant keeps its ancestor chain visible, and children that do not match are omitted. When the
/// request cannot produce a result, the last successful snapshot for the document is returned instead
/// so a transient failure does not blank the outline.
/// </para>
/// </remarks>
public sealed class LuaDocumentSymbolsProvider : ITextDocumentSymbolProvider
{
	private readonly ILanguageServerIntelliSenseProvider _intelliSenseProvider;
	private readonly string _filePath;

	private IReadOnlyList<TextDocumentSymbol> _lastSymbols = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaDocumentSymbolsProvider"/> class.
	/// </summary>
	/// <param name="intelliSenseProvider">The language-server provider serving the document.</param>
	/// <param name="filePath">The local file path of the outlined document.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="intelliSenseProvider"/> or <paramref name="filePath"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException"><paramref name="filePath"/> is empty or whitespace-only.</exception>
	public LuaDocumentSymbolsProvider(ILanguageServerIntelliSenseProvider intelliSenseProvider, string filePath)
	{
		_intelliSenseProvider = intelliSenseProvider ?? throw new ArgumentNullException(nameof(intelliSenseProvider));
		ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

		_filePath = filePath;
	}

	/// <inheritdoc/>
	public IReadOnlyList<TextDocumentSymbol> GetSymbols(TextDocumentSymbolRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		IReadOnlyList<TextDocumentSymbol> symbols;

		try
		{
			symbols = _intelliSenseProvider
				.GetDocumentSymbolsAsync(_filePath, request.DocumentText, CancellationToken.None)
				.GetAwaiter()
				.GetResult();
		}
		catch (Exception)
		{
			// Outline generation is best-effort: keep showing the last successful snapshot instead of
			// letting a failed or unavailable request blank the tree.
			return _lastSymbols;
		}

		_lastSymbols = symbols;

		return string.IsNullOrWhiteSpace(request.FilterText)
			? symbols
			: FilterSymbols(symbols, request.FilterText);
	}

	private static IReadOnlyList<TextDocumentSymbol> FilterSymbols(IReadOnlyList<TextDocumentSymbol> symbols, string filterText)
	{
		var result = new List<TextDocumentSymbol>(symbols.Count);

		foreach (TextDocumentSymbol symbol in symbols)
		{
			TextDocumentSymbol? filtered = FilterSymbol(symbol, filterText);

			if (filtered is not null)
				result.Add(filtered);
		}

		return result;
	}

	private static TextDocumentSymbol? FilterSymbol(TextDocumentSymbol symbol, string filterText)
	{
		var matchingChildren = new List<TextDocumentSymbol>(symbol.Children.Count);

		foreach (TextDocumentSymbol child in symbol.Children)
		{
			TextDocumentSymbol? filteredChild = FilterSymbol(child, filterText);

			if (filteredChild is not null)
				matchingChildren.Add(filteredChild);
		}

		// A symbol stays visible when it matches or when a descendant matches; a matching ancestor
		// keeps its matching subtree only.
		if (!symbol.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase) && matchingChildren.Count == 0)
			return null;

		return new TextDocumentSymbol(symbol.Name, symbol.Kind, symbol.Range, symbol.SelectionRange)
		{
			Detail = symbol.Detail,
			Data = symbol.Data,
			Children = matchingChildren
		};
	}
}
