using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Completion;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using Nickelony.IDEKit.IntelliSense.Hover;
using Nickelony.IDEKit.IntelliSense.Navigation;
using Nickelony.IDEKit.IntelliSense.Signatures;
using System.Reflection;
using System.Windows;
using static TombLib.Tests.WPFTestHelper;

namespace TombLib.Tests;

[TestClass]
public class LuaEditorIntelliSenseStateTests
{
	[TestMethod]
	public void ShouldRefreshSignatureHelpAfterTextInput_ReturnsTrueWhenSignatureHelpPresentationIsVisibleOrRequestPending()
	{
		bool shouldRefresh = InvokePrivateStaticBooleanMethod(
			"ShouldRefreshSignatureHelpAfterTextInput",
			[typeof(string), typeof(bool)],
			"a",
			true);

		Assert.IsTrue(shouldRefresh);
	}

	[TestMethod]
	public void ShouldRefreshSignatureHelpAfterTextInput_ReturnsFalseWhenSignatureHelpIsInactive()
	{
		bool shouldRefresh = InvokePrivateStaticBooleanMethod(
			"ShouldRefreshSignatureHelpAfterTextInput",
			[typeof(string), typeof(bool)],
			"a",
			false);

		Assert.IsFalse(shouldRefresh);
	}

	[TestMethod]
	public void ShouldDismissSignatureHelpOnAutoClosingSkip_ReturnsTrueOnlyForMatchingParenthesis()
	{
		bool shouldDismissMatchingParenthesis = InvokePrivateStaticBooleanMethod(
			"ShouldDismissSignatureHelpOnAutoClosingSkip",
			[typeof(string), typeof(string)],
			")",
			")");

		bool shouldDismissOtherElement = InvokePrivateStaticBooleanMethod(
			"ShouldDismissSignatureHelpOnAutoClosingSkip",
			[typeof(string), typeof(string)],
			"]",
			")");

		Assert.IsTrue(shouldDismissMatchingParenthesis);
		Assert.IsFalse(shouldDismissOtherElement);
	}

	[TestMethod]
	public void TryGetCompletionTrigger_ReturnsExplicitAndImplicitTriggers()
	{
		Assert.IsTrue(InvokeTryGetCompletionTrigger(".", out char? dotTrigger));
		Assert.AreEqual('.', dotTrigger);

		Assert.IsTrue(InvokeTryGetCompletionTrigger(":", out char? colonTrigger));
		Assert.AreEqual(':', colonTrigger);

		Assert.IsTrue(InvokeTryGetCompletionTrigger("a", out char? identifierTrigger));
		Assert.IsNull(identifierTrigger);
	}

	[TestMethod]
	public void TryGetCompletionTrigger_RejectsEmptyMultiCharacterAndNonIdentifierInput()
	{
		Assert.IsFalse(InvokeTryGetCompletionTrigger(null, out _));
		Assert.IsFalse(InvokeTryGetCompletionTrigger(string.Empty, out _));
		Assert.IsFalse(InvokeTryGetCompletionTrigger("ab", out _));
		Assert.IsFalse(InvokeTryGetCompletionTrigger(" ", out _));
	}

	[TestMethod]
	public void ShouldKeepCompletionWindowOpen_ReturnsTrueOnlyForIdentifierCharacters()
	{
		Assert.IsTrue(InvokePrivateStaticBooleanMethod(
			"ShouldKeepCompletionWindowOpen",
			[typeof(string)],
			"a"));

		Assert.IsTrue(InvokePrivateStaticBooleanMethod(
			"ShouldKeepCompletionWindowOpen",
			[typeof(string)],
			"_"));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"ShouldKeepCompletionWindowOpen",
			[typeof(string)],
			[null]));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"ShouldKeepCompletionWindowOpen",
			[typeof(string)],
			"."));
	}

	[TestMethod]
	public void IsAsyncEditorResultCurrent_ReturnsTrueForCurrentLoadedAvailableRequest()
	{
		bool isCurrent = InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			false,
			3,
			3,
			8,
			8,
			2,
			2,
			true,
			true);

		Assert.IsTrue(isCurrent);
	}

	[TestMethod]
	public void IsAsyncEditorResultCurrent_RejectsCanceledOrStaleResults()
	{
		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			true,
			3,
			3,
			8,
			8,
			2,
			2,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			false,
			3,
			4,
			8,
			8,
			2,
			2,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			false,
			3,
			3,
			8,
			9,
			2,
			2,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			false,
			3,
			3,
			8,
			8,
			2,
			3,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			false,
			3,
			3,
			8,
			8,
			2,
			2,
			false,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsAsyncEditorResultCurrent",
			[typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool)],
			false,
			3,
			3,
			8,
			8,
			2,
			2,
			true,
			false));
	}

	[TestMethod]
	public void IsCompletionItemCurrent_ReturnsTrueForMatchingMetadata()
	{
		bool isCurrent = InvokePrivateStaticBooleanMethod(
			"IsCompletionItemCurrent",
			[typeof(int?), typeof(int), typeof(int?), typeof(int), typeof(bool), typeof(bool)],
			8,
			8,
			2,
			2,
			true,
			true);

		Assert.IsTrue(isCurrent);
	}

	[TestMethod]
	public void IsCompletionItemCurrent_AllowsUnstampedItemsButRejectsStaleOrIncompleteMetadata()
	{
		Assert.IsTrue(InvokePrivateStaticBooleanMethod(
			"IsCompletionItemCurrent",
			[typeof(int?), typeof(int), typeof(int?), typeof(int), typeof(bool), typeof(bool)],
			null,
			8,
			null,
			2,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsCompletionItemCurrent",
			[typeof(int?), typeof(int), typeof(int?), typeof(int), typeof(bool), typeof(bool)],
			8,
			9,
			2,
			2,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsCompletionItemCurrent",
			[typeof(int?), typeof(int), typeof(int?), typeof(int), typeof(bool), typeof(bool)],
			8,
			8,
			null,
			2,
			true,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsCompletionItemCurrent",
			[typeof(int?), typeof(int), typeof(int?), typeof(int), typeof(bool), typeof(bool)],
			8,
			8,
			2,
			2,
			false,
			true));

		Assert.IsFalse(InvokePrivateStaticBooleanMethod(
			"IsCompletionItemCurrent",
			[typeof(int?), typeof(int), typeof(int?), typeof(int), typeof(bool), typeof(bool)],
			8,
			8,
			2,
			2,
			true,
			false));
	}

	[TestMethod]
	public void NavigateToDefinitionAtCaretAsync_RaisesDefinitionNavigationRequestedForResolvedLocation()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaIntellisenseProvider
			{
				DefinitionResponse = new TextDefinitionLocation(
					new TextPositionRange(new TextPosition(4, 2), new TextPosition(4, 2)),
					@"C:\Workspace\Definitions\spawn.lua")
			};

			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua",
				Text = "spawn()",
				IntelliSenseProvider = provider,
				CaretOffset = 2
			};

			TextDefinitionLocation? navigatedLocation = null;

			editor.DefinitionNavigationRequested += location => navigatedLocation = location;

			Window window = ShowInHostWindow(editor);

			try
			{
				editor.NavigateToDefinitionAtCaretAsync().GetAwaiter().GetResult();
			}
			finally
			{
				window.Close();
			}

			Assert.IsNotNull(navigatedLocation);
			Assert.AreEqual(provider.DefinitionResponse!.DocumentId, navigatedLocation.DocumentId);
			Assert.AreEqual(provider.DefinitionResponse.TargetRange, navigatedLocation.TargetRange);
			Assert.AreEqual(provider.DefinitionResponse.SelectionRange, navigatedLocation.SelectionRange);
			Assert.AreEqual(1, provider.DefinitionRequests.Count);
			Assert.AreEqual(0, provider.DefinitionRequests[0].Position.Line);
			Assert.AreEqual(0, provider.DefinitionRequests[0].Position.Character);
		});
	}

	[TestMethod]
	public void NavigateToDefinitionAtCaretAsync_DoesNotRaiseEventWhenProviderReturnsNoDefinition()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaIntellisenseProvider();

			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua",
				Text = "spawn()",
				IntelliSenseProvider = provider,
				CaretOffset = 2
			};

			int navigationRequestCount = 0;

			editor.DefinitionNavigationRequested += _ => navigationRequestCount++;

			Window window = ShowInHostWindow(editor);

			try
			{
				editor.NavigateToDefinitionAtCaretAsync().GetAwaiter().GetResult();
			}
			finally
			{
				window.Close();
			}

			Assert.AreEqual(0, navigationRequestCount);
			Assert.AreEqual(1, provider.DefinitionRequests.Count);
		});
	}

	[TestMethod]
	public void RequestSignatureHelpAsync_RoutesRequestToProvider()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaIntellisenseProvider();

			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua",
				Text = "spawn(",
				IntelliSenseProvider = provider
			};

			Window window = ShowInHostWindow(editor);

			try
			{
				Task requestTask = (Task)(InvokeInstanceMethod(editor, "RequestSignatureHelpAsync", [typeof(int)], 6)
					?? throw new InvalidOperationException("Private instance method 'RequestSignatureHelpAsync' returned null."));

				requestTask.GetAwaiter().GetResult();

				// The editor routes the caret position to the signature-help provider. Popup
				// visibility is owned by the shared TextSignatureHelpController (see its tests).
				Assert.AreEqual(1, provider.SignatureRequests.Count);
				Assert.AreEqual(0, provider.SignatureRequests[0].Position.Line);
				Assert.AreEqual(6, provider.SignatureRequests[0].Position.Character);
			}
			finally
			{
				window.Close();
			}
		});
	}

	[TestMethod]
	public void RequestSignatureHelpAsync_ResolvedSignature_IsRequestedFromProvider()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaIntellisenseProvider
			{
				SignatureResponse = new TextSignatureHelp(
					[new TextSignatureInformation(
						"spawn(room)",
						"Spawns an object.",
						activeParameter: TextSignatureActiveParameter.At(0),
						parameters: [new TextSignatureParameterInfo("room", "Room id.")])])
			};

			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Workspace\Scripts\test.lua",
				Text = "spawn(",
				IntelliSenseProvider = provider
			};

			Window window = ShowInHostWindow(editor);

			try
			{
				Task requestTask = (Task)(InvokeInstanceMethod(editor, "RequestSignatureHelpAsync", [typeof(int)], 6)
					?? throw new InvalidOperationException("Private instance method 'RequestSignatureHelpAsync' returned null."));

				requestTask.GetAwaiter().GetResult();

				// The editor routes the caret position to the signature-help provider. Popup
				// visibility is owned by the shared TextSignatureHelpController (see its tests).
				Assert.AreEqual(1, provider.SignatureRequests.Count);
				Assert.AreEqual(0, provider.SignatureRequests[0].Position.Line);
				Assert.AreEqual(6, provider.SignatureRequests[0].Position.Character);
			}
			finally
			{
				window.Close();
			}
		});
	}

	private static bool InvokePrivateStaticBooleanMethod(string methodName, Type[] parameterTypes, params object?[] arguments)
	{
		return (bool)(InvokeStaticMethod(typeof(LuaEditor), methodName, parameterTypes, arguments)
			?? throw new InvalidOperationException($"Private static method '{methodName}' returned null."));
	}

	private static bool InvokeTryGetCompletionTrigger(string? inputText, out char? triggerCharacter)
	{
		MethodInfo method = typeof(LuaEditor).GetMethod(
			"TryGetCompletionTrigger",
			BindingFlags.Static | BindingFlags.NonPublic,
			binder: null,
			[typeof(string), typeof(char?).MakeByRefType()],
			modifiers: null)
			?? throw new InvalidOperationException("Private static method 'TryGetCompletionTrigger' was not found.");

		object?[] arguments = [inputText, null];
		bool result = (bool)(method.Invoke(null, arguments)
			?? throw new InvalidOperationException("Private static method 'TryGetCompletionTrigger' returned null."));

		triggerCharacter = arguments[1] as char?;
		return result;
	}

	private readonly record struct ProviderRequest(string FilePath, string Content, TextPosition Position);

	private sealed class FakeLuaIntellisenseProvider : ILuaLanguageServerIntelliSenseProvider
	{
		public bool IsAvailable { get; set; } = true;

		public LanguageServerProviderState State => LanguageServerProviderState.Ready;

		public bool SupportsReferences => false;
		public bool SupportsRename => false;
		public bool SupportsFormatting => false;
		public bool SupportsDocumentSymbols => false;
		public bool SupportsCodeActions => false;

		public TextHoverInfo? HoverResponse { get; set; }

		public TextDefinitionLocation? DefinitionResponse { get; set; }

		public TextSignatureHelp? SignatureResponse { get; set; }

		public Func<ProviderRequest, CancellationToken, Task<TextSignatureHelp?>>? SignatureHelpHandler { get; set; }

		public IReadOnlyList<TextCompletionItem> CompletionItems { get; set; } = [];

		public List<ProviderRequest> DefinitionRequests { get; } = [];

		public List<ProviderRequest> SignatureRequests { get; } = [];

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

		public event EventHandler<SemanticTokensUpdatedEventArgs>? SemanticTokensUpdated
		{
			add { }
			remove { }
		}

		public IReadOnlyList<TextDiagnostic> GetDiagnostics(string filePath) => [];

		public IReadOnlyList<SemanticToken> GetSemanticTokens(string filePath) => [];

		public void OpenDocument(string filePath, string content)
		{ }

		public void UpdateDocument(string filePath, string content)
		{ }

		public void CloseDocument(string filePath)
		{ }

		public void MoveDocument(string oldFilePath, string newFilePath, string content)
		{ }

		public Task<IReadOnlyList<TextCodeAction>> GetCodeActionsAsync(LanguageServerCodeActionRequest request,
			CancellationToken cancellationToken = default)
			=> Task.FromResult<IReadOnlyList<TextCodeAction>>([]);

		public Task<IReadOnlyList<TextCompletionItem>> GetCompletionItemsAsync(LanguageServerCompletionRequest request,
			CancellationToken cancellationToken = default)
			=> Task.FromResult(CompletionItems);

		public Task<TextHoverInfo?> GetHoverAsync(string filePath, string content,
			TextPosition position, CancellationToken cancellationToken = default)
			=> Task.FromResult(HoverResponse);

		public Task<TextDefinitionLocation?> GetDefinitionAsync(string filePath, string content,
			TextPosition position, CancellationToken cancellationToken = default)
		{
			DefinitionRequests.Add(new ProviderRequest(filePath, content, position));
			return Task.FromResult(DefinitionResponse);
		}

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
			=> Task.FromResult<IReadOnlyList<TextDocumentSymbol>>([]);

		public Task<TextSignatureHelp?> GetSignatureHelpAsync(LanguageServerSignatureHelpRequest request,
			CancellationToken cancellationToken = default)
		{
			var providerRequest = new ProviderRequest(request.FilePath, request.DocumentText, request.Position);
			SignatureRequests.Add(providerRequest);

			if (SignatureHelpHandler is not null)
				return SignatureHelpHandler(providerRequest, cancellationToken);

			return Task.FromResult(SignatureResponse);
		}

		public void Dispose()
		{ }

		public ValueTask DisposeAsync() => ValueTask.CompletedTask;
	}
}
