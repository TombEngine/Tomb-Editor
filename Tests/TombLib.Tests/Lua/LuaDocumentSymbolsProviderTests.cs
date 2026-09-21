using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Completion;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using Nickelony.IDEKit.IntelliSense.Hover;
using Nickelony.IDEKit.IntelliSense.Navigation;
using Nickelony.IDEKit.IntelliSense.Signatures;
using TombLib.Scripting.Lua.ContentNodes;

namespace TombLib.Tests;

[TestClass]
public sealed class LuaDocumentSymbolsProviderTests
{
	private const string FilePath = @"C:\Scripts\test.lua";

	[TestMethod]
	public void GetSymbols_RequestsSymbolsForTheBoundDocument()
	{
		using var intelliSenseProvider = new FakeIntelliSenseProvider
		{
			SymbolResponse =
			[
				new TextDocumentSymbol("value", TextDocumentSymbolKind.Variable)
			]
		};
		var provider = new LuaDocumentSymbolsProvider(intelliSenseProvider, FilePath);

		IReadOnlyList<TextDocumentSymbol> symbols = provider.GetSymbols(
			new TextDocumentSymbolRequest("local value = 1", string.Empty));

		Assert.AreEqual(1, intelliSenseProvider.SymbolRequests.Count);
		Assert.AreEqual(FilePath, intelliSenseProvider.SymbolRequests[0].FilePath);
		Assert.AreEqual("local value = 1", intelliSenseProvider.SymbolRequests[0].Content);

		Assert.AreEqual(1, symbols.Count);
		Assert.AreEqual("value", symbols[0].Name);
		Assert.AreEqual(TextDocumentSymbolKind.Variable, symbols[0].Kind);
	}

	[TestMethod]
	public void GetSymbols_EmptyFilter_ReturnsTheWholeTree()
	{
		using var intelliSenseProvider = new FakeIntelliSenseProvider
		{
			SymbolResponse = CreateTree()
		};
		var provider = new LuaDocumentSymbolsProvider(intelliSenseProvider, FilePath);

		IReadOnlyList<TextDocumentSymbol> symbols = provider.GetSymbols(
			new TextDocumentSymbolRequest("content", string.Empty));

		Assert.AreEqual(2, symbols.Count);
		Assert.AreEqual("engine", symbols[0].Name);
		Assert.AreEqual(2, symbols[0].Children.Count);
		Assert.AreEqual("value", symbols[1].Name);
	}

	[TestMethod]
	public void GetSymbols_Filter_KeepsMatchesAndTheirAncestors()
	{
		using var intelliSenseProvider = new FakeIntelliSenseProvider
		{
			SymbolResponse = CreateTree()
		};
		var provider = new LuaDocumentSymbolsProvider(intelliSenseProvider, FilePath);

		IReadOnlyList<TextDocumentSymbol> symbols = provider.GetSymbols(
			new TextDocumentSymbolRequest("content", "SPA"));

		// The matching child keeps its ancestor chain visible; non-matching siblings and unrooted
		// symbols are omitted.
		Assert.AreEqual(1, symbols.Count);
		Assert.AreEqual("engine", symbols[0].Name);
		Assert.AreEqual(1, symbols[0].Children.Count);
		Assert.AreEqual("spawn", symbols[0].Children[0].Name);
	}

	[TestMethod]
	public void GetSymbols_FilterMatchingAncestor_OmitsNonMatchingChildren()
	{
		using var intelliSenseProvider = new FakeIntelliSenseProvider
		{
			SymbolResponse = CreateTree()
		};
		var provider = new LuaDocumentSymbolsProvider(intelliSenseProvider, FilePath);

		IReadOnlyList<TextDocumentSymbol> symbols = provider.GetSymbols(
			new TextDocumentSymbolRequest("content", "engine"));

		Assert.AreEqual(1, symbols.Count);
		Assert.AreEqual("engine", symbols[0].Name);
		Assert.AreEqual(0, symbols[0].Children.Count);
	}

	[TestMethod]
	public void GetSymbols_FailedRequest_ReturnsTheLastSuccessfulSnapshot()
	{
		using var intelliSenseProvider = new FakeIntelliSenseProvider
		{
			SymbolResponse = CreateTree()
		};
		var provider = new LuaDocumentSymbolsProvider(intelliSenseProvider, FilePath);

		IReadOnlyList<TextDocumentSymbol> firstSymbols = provider.GetSymbols(
			new TextDocumentSymbolRequest("content", string.Empty));

		intelliSenseProvider.ThrowOnSymbolRequest = true;

		IReadOnlyList<TextDocumentSymbol> fallbackSymbols = provider.GetSymbols(
			new TextDocumentSymbolRequest("content", string.Empty));

		// A failed request keeps showing the last successful snapshot instead of blanking the outline.
		Assert.AreSame(firstSymbols, fallbackSymbols);
	}

	[TestMethod]
	public void GetSymbols_SuccessfulEmptyResponse_ReplacesTheSnapshotWithAnEmptyOutline()
	{
		using var intelliSenseProvider = new FakeIntelliSenseProvider
		{
			SymbolResponse = CreateTree()
		};
		var provider = new LuaDocumentSymbolsProvider(intelliSenseProvider, FilePath);

		provider.GetSymbols(new TextDocumentSymbolRequest("content", string.Empty));

		intelliSenseProvider.SymbolResponse = [];
		provider.GetSymbols(new TextDocumentSymbolRequest("content", string.Empty));

		intelliSenseProvider.ThrowOnSymbolRequest = true;

		// A successful empty response is authoritative: the server reports no symbols for the document.
		Assert.AreEqual(0, provider.GetSymbols(new TextDocumentSymbolRequest("content", string.Empty)).Count);
	}

	[TestMethod]
	public void GetSymbols_NullRequest_Throws()
	{
		using var intelliSenseProvider = new FakeIntelliSenseProvider();
		var provider = new LuaDocumentSymbolsProvider(intelliSenseProvider, FilePath);

		Assert.ThrowsException<ArgumentNullException>(() => provider.GetSymbols(null!));
	}

	[TestMethod]
	public void Constructor_InvalidArguments_Throw()
	{
		using var intelliSenseProvider = new FakeIntelliSenseProvider();

		Assert.ThrowsException<ArgumentNullException>(() => new LuaDocumentSymbolsProvider(null!, FilePath));
		Assert.ThrowsException<ArgumentException>(() => new LuaDocumentSymbolsProvider(intelliSenseProvider, "   "));
	}

	private static List<TextDocumentSymbol> CreateTree() =>
	[
		new TextDocumentSymbol("engine", TextDocumentSymbolKind.Module)
		{
			Children =
			[
				new TextDocumentSymbol("spawn", TextDocumentSymbolKind.Function),
				new TextDocumentSymbol("move", TextDocumentSymbolKind.Function)
			]
		},
		new TextDocumentSymbol("value", TextDocumentSymbolKind.Variable)
	];

	private sealed class FakeIntelliSenseProvider : ILanguageServerIntelliSenseProvider
	{
		public IReadOnlyList<TextDocumentSymbol> SymbolResponse { get; set; } = [];

		public bool ThrowOnSymbolRequest { get; set; }

		public List<(string FilePath, string Content)> SymbolRequests { get; } = [];

		public bool IsAvailable => true;

		public LanguageServerProviderState State => LanguageServerProviderState.Ready;

		public bool SupportsReferences => false;

		public bool SupportsRename => false;

		public bool SupportsFormatting => false;

		public bool SupportsDocumentSymbols => false;

		public bool SupportsCodeActions => false;

		public event EventHandler<DiagnosticsUpdatedEventArgs>? DiagnosticsUpdated
		{
			add { }
			remove { }
		}

		public event EventHandler? CapabilitiesChanged
		{
			add { }
			remove { }
		}

		public event EventHandler<StartupFailedEventArgs>? StartupFailed
		{
			add { }
			remove { }
		}

		public event EventHandler<WorkspaceWatcherFailedEventArgs>? WorkspaceWatcherFailed
		{
			add { }
			remove { }
		}

		public IReadOnlyList<TextDiagnostic> GetDiagnostics(string filePath) => [];

		public void OpenDocument(string filePath, string content)
		{ }

		public void UpdateDocument(string filePath, string content)
		{ }

		public void CloseDocument(string filePath)
		{ }

		public void MoveDocument(string oldFilePath, string newFilePath, string content)
		{ }

		public void Dispose()
		{ }

		public ValueTask DisposeAsync() => ValueTask.CompletedTask;

		public Task<IReadOnlyList<TextCodeAction>> GetCodeActionsAsync(LanguageServerCodeActionRequest request,
			CancellationToken cancellationToken = default)
			=> Task.FromResult<IReadOnlyList<TextCodeAction>>([]);

		public Task<IReadOnlyList<TextCompletionItem>> GetCompletionItemsAsync(LanguageServerCompletionRequest request,
			CancellationToken cancellationToken = default)
			=> Task.FromResult<IReadOnlyList<TextCompletionItem>>([]);

		public Task<TextHoverInfo?> GetHoverAsync(string filePath, string content,
			TextPosition position, CancellationToken cancellationToken = default)
			=> Task.FromResult<TextHoverInfo?>(null);

		public Task<TextDefinitionLocation?> GetDefinitionAsync(string filePath, string content,
			TextPosition position, CancellationToken cancellationToken = default)
			=> Task.FromResult<TextDefinitionLocation?>(null);

		public Task<TextSignatureHelp?> GetSignatureHelpAsync(LanguageServerSignatureHelpRequest request,
			CancellationToken cancellationToken = default)
			=> Task.FromResult<TextSignatureHelp?>(null);

		public Task<IReadOnlyList<TextReferenceLocation>> GetReferencesAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
			=> Task.FromResult<IReadOnlyList<TextReferenceLocation>>([]);

		public Task<IReadOnlyList<TextReferenceLocation>> GetReferencesAsync(TextReferenceRequest request, CancellationToken cancellationToken = default)
			=> Task.FromResult<IReadOnlyList<TextReferenceLocation>>([]);

		public Task<TextWorkspaceEdit?> RenameSymbolAsync(TextRenameRequest request, CancellationToken cancellationToken = default)
			=> Task.FromResult<TextWorkspaceEdit?>(null);

		public Task<TextWorkspaceEdit?> FormatDocumentAsync(TextFormatRequest request, CancellationToken cancellationToken = default)
			=> Task.FromResult<TextWorkspaceEdit?>(null);

		public Task<IReadOnlyList<TextDocumentSymbol>> GetDocumentSymbolsAsync(string filePath, string content,
			CancellationToken cancellationToken = default)
		{
			SymbolRequests.Add((filePath, content));

			if (ThrowOnSymbolRequest)
				throw new InvalidOperationException("Simulated document-symbol request failure.");

			return Task.FromResult(SymbolResponse);
		}
	}
}
