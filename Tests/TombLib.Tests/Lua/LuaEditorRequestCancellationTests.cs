using Nickelony.IDEKit.IntelliSense.Completion;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Hover;
using Nickelony.IDEKit.IntelliSense.Navigation;
using Nickelony.IDEKit.IntelliSense.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static TombLib.Tests.WPFTestHelper;

namespace TombLib.Tests;

[TestClass]
public class LuaEditorRequestCancellationTests
{
	[TestMethod]
	public void CompletionRequest_ObservesCancellation_WhenRequestsAreInvalidated()
	{
		RunInSta(() =>
		{
			var provider = new TrackingIntelliSenseProvider();
			var editor = CreateEditor(provider, "spa");
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				_ = InvokeCompletionRequest(editor);

				Assert.IsFalse(provider.LastCompletionToken.IsCancellationRequested);

				InvokeInstanceMethod(editor, "InvalidateAsyncEditorRequests", Type.EmptyTypes);

				Assert.IsTrue(provider.LastCompletionToken.IsCancellationRequested);

				provider.CompleteCompletionRequest();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void SignatureHelpRequest_ObservesCancellation_WhenRequestsAreInvalidated()
	{
		RunInSta(() =>
		{
			var provider = new TrackingIntelliSenseProvider();
			var editor = CreateEditor(provider, "spawn(");
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				_ = InvokeSignatureHelpRequest(editor);

				Assert.IsFalse(provider.LastSignatureHelpToken.IsCancellationRequested);

				InvokeInstanceMethod(editor, "InvalidateAsyncEditorRequests", Type.EmptyTypes);

				Assert.IsTrue(provider.LastSignatureHelpToken.IsCancellationRequested);

				provider.CompleteSignatureHelpRequest();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void Load_ObservesCancellation_WhenEditorRequestsArePending()
	{
		RunInSta(() =>
		{
			string filePath = Path.Combine(Path.GetTempPath(), $"tomb-editor-{Guid.NewGuid():N}.lua");
			File.WriteAllText(filePath, "local loaded = true");

			var provider = new TrackingIntelliSenseProvider();
			var editor = CreateEditor(provider, "spa");
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				_ = InvokeCompletionRequest(editor);
				Assert.IsFalse(provider.LastCompletionToken.IsCancellationRequested);

				editor.Load(filePath, default);

				Assert.IsTrue(provider.LastCompletionToken.IsCancellationRequested);
			}
			finally
			{
				hostWindow.Close();
				File.Delete(filePath);
			}
		});
	}

	[TestMethod]
	public void CompletionRequest_ObservesCancellation_WhenEditorIsDisposed()
	{
		RunInSta(() =>
		{
			var provider = new TrackingIntelliSenseProvider();
			var editor = CreateEditor(provider, "spa");
			Window hostWindow = ShowInHostWindow(editor);

			try
			{
				_ = InvokeCompletionRequest(editor);

				Assert.IsFalse(provider.LastCompletionToken.IsCancellationRequested);

				editor.Dispose();

				Assert.IsTrue(provider.LastCompletionToken.IsCancellationRequested);

				provider.CompleteCompletionRequest();
				PumpDispatcher(editor.Dispatcher, DispatcherPriority.ContextIdle);
			}
			finally
			{
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

	private static Task InvokeCompletionRequest(LuaEditor editor)
	{
		return (Task)(InvokeInstanceMethod(editor, "RequestCompletionAsync", [typeof(int), typeof(char?)], 3, null)
			?? throw new InvalidOperationException("RequestCompletionAsync returned null."));
	}

	private static Task InvokeSignatureHelpRequest(LuaEditor editor)
	{
		return (Task)(InvokeInstanceMethod(editor, "RequestSignatureHelpAsync", [typeof(int)], 6)
			?? throw new InvalidOperationException("RequestSignatureHelpAsync returned null."));
	}

	private sealed class TrackingIntelliSenseProvider : ILuaIntelliSenseProvider
	{
		private readonly TaskCompletionSource<IReadOnlyList<TextCompletionItem>> _completionResponse = new();
		private readonly TaskCompletionSource<TextSignatureHelpInfo?> _signatureHelpResponse = new();

		public bool IsAvailable { get; set; } = true;

		public LanguageServerProviderState State => LanguageServerProviderState.Ready;

		public bool SupportsReferences => false;
		public bool SupportsRename => false;
		public bool SupportsFormatting => false;

		public CancellationToken LastCompletionToken { get; private set; }

		public CancellationToken LastSignatureHelpToken { get; private set; }

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
		{
			LastCompletionToken = cancellationToken;
			return _completionResponse.Task;
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
		{
			LastSignatureHelpToken = cancellationToken;
			return _signatureHelpResponse.Task;
		}

		public void CompleteCompletionRequest()
			=> _completionResponse.TrySetResult([]);

		public void CompleteSignatureHelpRequest()
			=> _signatureHelpResponse.TrySetResult(null);

		public void Dispose()
		{ }
	}
}
