using ICSharpCode.AvalonEdit.CodeCompletion;
using Nickelony.IDEKit.IntelliSense.Completion;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Hover;
using Nickelony.IDEKit.IntelliSense.Navigation;
using Nickelony.IDEKit.IntelliSense.Signatures;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TombLib.Scripting.UI.Completion;
using static TombLib.Tests.WPFTestHelper;

namespace TombLib.Tests;

[TestClass]
public class LuaEditorCompletionWindowTests
{
	[TestMethod]
	public void RequestCompletionAsync_OpensCompletionWindowWithCurrentItemsAndOffsets()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaCompletionProvider();

			provider.EnqueueCompletionResponse(
			[
				new TextCompletionItem("spawn_room", detail: "local variable")
			]);

			var editor = CreateEditor(provider, "spa");
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				CompletionWindow? completionWindow = editor.ActiveCompletionWindow;

				Assert.IsNotNull(completionWindow);
				Assert.AreEqual(1, completionWindow.CompletionList.CompletionData.Count);
				Assert.AreEqual(0, completionWindow.StartOffset);
				Assert.AreEqual(3, completionWindow.EndOffset);
				Assert.IsNotNull(completionWindow.CompletionList.ListBox.SelectedItem);
				Assert.AreEqual(1, provider.CompletionRequests.Count);
				Assert.AreEqual(0, provider.CompletionRequests[0].Line);
				Assert.AreEqual(3, provider.CompletionRequests[0].Column);
			}
			finally
			{
				CloseCompletionWindow(editor);
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RequestCompletionAsync_WhenIntelliSenseIsDisabled_DoesNotQueryProvider()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaCompletionProvider();
			var editor = CreateEditor(provider, "spa");
			editor.IntelliSenseEnabled = false;
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				Assert.AreEqual(0, provider.CompletionRequests.Count);
				Assert.IsNull(editor.ActiveCompletionWindow);
			}
			finally
			{
				CloseCompletionWindow(editor);
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RequestCompletionAsync_RefreshClosesPreviousTooltipAndRecreatesWindow()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaCompletionProvider();

			provider.EnqueueCompletionResponse(
			[
				new TextCompletionItem("spawn_room", detail: "local variable")
			]);

			provider.EnqueueCompletionResponse(
			[
				new TextCompletionItem("spell_room", detail: "global variable")
			]);

			var editor = CreateEditor(provider, "spa");
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				CompletionWindow? firstWindow = editor.ActiveCompletionWindow;
				Assert.IsNotNull(firstWindow);
				ToolTip firstToolTip = GetCompletionToolTip(firstWindow);
				firstToolTip.Content = new TextBlock { Text = "old tooltip" };
				firstToolTip.IsOpen = true;

				editor.Text = "spe";
				editor.CaretOffset = 3;

				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				CompletionWindow? refreshedWindow = editor.ActiveCompletionWindow;
				Assert.IsNotNull(refreshedWindow);
				var refreshedItem = (CompletionData)refreshedWindow.CompletionList.CompletionData[0];

				Assert.AreNotSame(firstWindow, refreshedWindow);
				Assert.IsFalse(firstToolTip.IsOpen);
				Assert.AreEqual("spell_room", refreshedItem.DisplayText);
			}
			finally
			{
				CloseCompletionWindow(editor);
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void RequestCompletionAsync_EmptyResults_CloseExistingWindow()
	{
		RunInSta(() =>
		{
			var provider = new FakeLuaCompletionProvider();

			provider.EnqueueCompletionResponse(
			[
				new TextCompletionItem("spawn_room", detail: "local variable")
			]);

			provider.EnqueueCompletionResponse([]);

			var editor = CreateEditor(provider, "spa");
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				InvokePrivateTask(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null).GetAwaiter().GetResult();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);

				Assert.IsNull(editor.ActiveCompletionWindow);
			}
			finally
			{
				CloseCompletionWindow(editor);
				hostWindow.Close();
			}
		});
	}

	private static LuaEditor CreateEditor(ILuaIntelliSenseProvider provider, string text) => new(new Version(1, 0))
	{
		FilePath = @"C:\Workspace\Scripts\test.lua",
		Text = text,
		IntelliSenseProvider = provider
	};

	private static Task InvokePrivateTask(object instance, string methodName, Type[] parameterTypes, params object?[] arguments)
	{
		return (Task)(InvokeInstanceMethod(instance, methodName, parameterTypes, arguments)
			?? throw new InvalidOperationException($"Private instance method '{methodName}' returned null."));
	}

	private static void CloseCompletionWindow(LuaEditor editor)
		=> InvokeInstanceMethod(editor, "CloseCompletionWindow", Type.EmptyTypes);

	private static ToolTip GetCompletionToolTip(CompletionWindow completionWindow)
	{
		FieldInfo field = typeof(CompletionWindow).GetField("toolTip", BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("CompletionWindow private field 'toolTip' was not found.");

		return (ToolTip)(field.GetValue(completionWindow)
			?? throw new InvalidOperationException("CompletionWindow private field 'toolTip' returned null."));
	}

	private readonly record struct CompletionRequest(string FilePath, string Content, int Line, int Column, char? TriggerCharacter);

	private sealed class FakeLuaCompletionProvider : ILuaIntelliSenseProvider
	{
		private readonly Queue<IReadOnlyList<TextCompletionItem>> _completionResponses = [];

		public bool IsAvailable { get; set; } = true;

		public LanguageServerProviderState State => LanguageServerProviderState.Ready;

		public bool SupportsReferences => false;
		public bool SupportsRename => false;
		public bool SupportsFormatting => false;

		public List<CompletionRequest> CompletionRequests { get; } = [];

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

		public void EnqueueCompletionResponse(IReadOnlyList<TextCompletionItem> items)
			=> _completionResponses.Enqueue(items);

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
		{
			CompletionRequests.Add(new CompletionRequest(filePath, content, line, column, triggerCharacter));
			IReadOnlyList<TextCompletionItem> response = _completionResponses.Count > 0 ? _completionResponses.Dequeue() : [];
			return Task.FromResult(response);
		}

		public Task<TextHoverInfo?> GetHoverAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
			=> Task.FromResult<TextHoverInfo?>(null);

		public Task<TextDefinitionLocation?> GetDefinitionAsync(string filePath, string content,
			int line, int column, CancellationToken cancellationToken = default)
			=> Task.FromResult<TextDefinitionLocation?>(null);

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
			=> Task.FromResult<TextSignatureHelpInfo?>(null);

		public void Dispose()
		{ }
	}
}
