using Nickelony.IDEKit.IntelliSense.Completion;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
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
	public void ShouldRefreshSignatureHelpAfterTextInput_ReturnsTrueWhenSignatureHelpIsActiveOrPending()
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
				DefinitionResponse = new TextDefinitionLocation(4, 2, @"C:\Workspace\Definitions\spawn.lua")
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
			Assert.AreEqual(provider.DefinitionResponse!.FilePath, navigatedLocation.FilePath);
			Assert.AreEqual(provider.DefinitionResponse.LineNumber, navigatedLocation.LineNumber);
			Assert.AreEqual(provider.DefinitionResponse.ColumnNumber, navigatedLocation.ColumnNumber);
			Assert.AreEqual(1, provider.DefinitionRequests.Count);
			Assert.AreEqual(0, provider.DefinitionRequests[0].Line);
			Assert.AreEqual(0, provider.DefinitionRequests[0].Column);
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
				Assert.AreEqual(0, provider.SignatureRequests[0].Line);
				Assert.AreEqual(6, provider.SignatureRequests[0].Column);
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
				SignatureResponse = new TextSignatureHelpInfo(
					"spawn(room)",
					0,
					"Spawns an object.",
					[new TextSignatureParameterInfo("room", "Room id.")])
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
				Assert.AreEqual(0, provider.SignatureRequests[0].Line);
				Assert.AreEqual(6, provider.SignatureRequests[0].Column);
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

	private readonly record struct ProviderRequest(string FilePath, string Content, int Line, int Column);

	private sealed class FakeLuaIntellisenseProvider : ILuaIntelliSenseProvider
	{
		public bool IsAvailable { get; set; } = true;

		public LanguageServerProviderState State => LanguageServerProviderState.Ready;

		public bool SupportsReferences => false;
		public bool SupportsRename => false;
		public bool SupportsFormatting => false;

		public TextHoverInfo? HoverResponse { get; set; }

		public TextDefinitionLocation? DefinitionResponse { get; set; }

		public TextSignatureHelpInfo? SignatureResponse { get; set; }

		public Func<ProviderRequest, CancellationToken, Task<TextSignatureHelpInfo?>>? SignatureHelpHandler { get; set; }

		public IReadOnlyList<TextCompletionItem> CompletionItems { get; set; } = [];

		public List<ProviderRequest> DefinitionRequests { get; } = [];

		public List<ProviderRequest> SignatureRequests { get; } = [];

		public event Action<string, IReadOnlyList<TextEditorDiagnostic>>? DiagnosticsUpdated
		{
			add { }
			remove { }
		}

		public event Action? CapabilitiesChanged
		{
			add { }
			remove { }
		}

		public event Action<LanguageServerStartupFailure>? StartupFailed
		{
			add { }
			remove { }
		}

		public event Action<WorkspaceWatcherFailure>? WorkspaceWatcherFailed
		{
			add { }
			remove { }
		}

		public event Action<string, IReadOnlyList<LuaSemanticToken>>? SemanticTokensUpdated
		{
			add { }
			remove { }
		}

		public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath) => [];

		public IReadOnlyList<LuaSemanticToken> GetSemanticTokens(string filePath) => [];

		public void OpenDocument(string filePath, string content)
		{ }

		public void UpdateDocument(string filePath, string content)
		{ }

		public void CloseDocument(string filePath)
		{ }

		public void RenameDocument(string oldFilePath, string newFilePath, string content)
		{ }

		public Task<IReadOnlyList<TextCompletionItem>> GetCompletionItemsAsync(string filePath, string content,
			int line, int column, char? triggerCharacter = null, CancellationToken cancellationToken = default)
			=> Task.FromResult(CompletionItems);

		public Task<TextHoverInfo?> GetHoverAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
			=> Task.FromResult(HoverResponse);

		public Task<TextDefinitionLocation?> GetDefinitionAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
		{
			DefinitionRequests.Add(new ProviderRequest(filePath, content, line, column));
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

		public Task<TextSignatureHelpInfo?> GetSignatureHelpAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
		{
			var request = new ProviderRequest(filePath, content, line, column);
			SignatureRequests.Add(request);

			if (SignatureHelpHandler is not null)
				return SignatureHelpHandler(request, cancellationToken);

			return Task.FromResult(SignatureResponse);
		}

		public void Dispose()
		{ }
	}
}
