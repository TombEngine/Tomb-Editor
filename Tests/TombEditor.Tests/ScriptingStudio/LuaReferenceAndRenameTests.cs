using ICSharpCode.AvalonEdit.Document;
using Moq;
using Nickelony.LanguageServer.Abstractions;
using System.IO;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.Workspace;
using Nickelony.IDEKit.Workspace.Views;
using TombLib.Scripting.Lua;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Editing;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public sealed class LuaReferenceAndRenameTests
{
	[TestMethod]
	public void ReferenceSearch_GroupsLocationsAndUsesOpenEditorPreview()
	{
		StaTestHelper.RunInSta(() =>
		{
			string rootPath = Path.Combine(Path.GetTempPath(), "TombEditor-LuaReferences-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(rootPath);
			string firstFilePath = Path.Combine(rootPath, "first.lua");
			string secondFilePath = Path.Combine(rootPath, "nested", "second.lua");
			Directory.CreateDirectory(Path.GetDirectoryName(secondFilePath)!);
			File.WriteAllText(firstFilePath, "local first = 1\n");
			File.WriteAllText(secondFilePath, "local second = 2\n");

			try
			{
				var editor = new LuaEditor(new Version(1, 0))
				{
					FilePath = firstFilePath,
					Text = "local first = 1\nreturn first"
				};
				var textEditorHost = new Mock<ITextEditorHost>();
				textEditorHost
					.Setup(host => host.GetOpenEditors(It.IsAny<string>()))
					.Returns((string filePath) => filePath.Equals(firstFilePath, StringComparison.OrdinalIgnoreCase)
						? [editor]
						: []);
				var referencesProvider = new Mock<ILanguageServerReferencesProvider>();
				referencesProvider.SetupGet(provider => provider.SupportsReferences).Returns(true);
				referencesProvider
					.Setup(provider => provider.GetReferencesAsync(It.IsAny<TextReferenceRequest>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync([
						new TextReferenceLocation(secondFilePath, new TextPositionRange(new TextPosition(0, 6), new TextPosition(0, 12))),
						new TextReferenceLocation(firstFilePath, new TextPositionRange(new TextPosition(1, 7), new TextPosition(1, 12))),
						new TextReferenceLocation(firstFilePath, new TextPositionRange(new TextPosition(0, 6), new TextPosition(0, 11)))
					]);

				var service = new LuaReferenceSearchService(textEditorHost.Object, referencesProvider.Object, rootPath);
				IReadOnlyList<TombLib.Scripting.UI.Presentation.TextReferenceGroup> groups =
					service.FindReferencesAsync(editor, CancellationToken.None).GetAwaiter().GetResult();

				Assert.AreEqual(2, groups.Count);
				Assert.AreEqual("first.lua", groups[0].DisplayPath);
				Assert.AreEqual(2, groups[0].Count);
				Assert.AreEqual("local first = 1", groups[0].Items[0].PreviewText);
				Assert.AreEqual("nested" + Path.DirectorySeparatorChar + "second.lua", groups[1].DisplayPath);
				editor.Dispose();
			}
			finally
			{
				Directory.Delete(rootPath, recursive: true);
			}
		});
	}

	[TestMethod]
	public void ReferenceSearch_UsesCanonicalSnapshotForClosedFilePreview()
		=> StaTestHelper.RunInSta(() =>
		{
			string rootPath = Path.Combine(Path.GetTempPath(), "TombEditor-LuaReferences-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(rootPath);
			string filePath = Path.Combine(rootPath, "closed.lua");
			File.WriteAllText(filePath, "stale disk text\n");

			try
			{
				using var editor = new LuaEditor(new Version(1, 0))
				{
					FilePath = Path.Combine(rootPath, "request.lua"),
					Text = "return true"
				};
				var textEditorHost = new Mock<ITextEditorHost>();
				textEditorHost
					.Setup(host => host.GetOpenEditors(filePath))
					.Returns([]);
				var referencesProvider = new Mock<ILanguageServerReferencesProvider>();
				referencesProvider.SetupGet(provider => provider.SupportsReferences).Returns(true);
				referencesProvider
					.Setup(provider => provider.GetReferencesAsync(It.IsAny<TextReferenceRequest>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync([new TextReferenceLocation(filePath, new TextPositionRange(new TextPosition(0, 0), new TextPosition(0, 6)))]);

				WorkspaceDocumentSnapshot snapshot = new(
					new WorkspaceDocumentKey(Guid.NewGuid()),
					filePath,
					filePath,
					1,
					1,
					false,
					new StringTextSnapshot("canonical workspace text\n", filePath),
					new TextFileFormat(TextEncodingKind.Utf8, false, TextNewlineStyle.Lf),
					FileStamp.Missing);
				var documentManager = new TestDocumentBridge(snapshot);

				var service = new LuaReferenceSearchService(
					textEditorHost.Object,
					referencesProvider.Object,
					rootPath,
					documentManager);
				IReadOnlyList<TombLib.Scripting.UI.Presentation.TextReferenceGroup> groups =
					service.FindReferencesAsync(editor, CancellationToken.None).GetAwaiter().GetResult();

				Assert.AreEqual("canonical workspace text", groups[0].Items[0].PreviewText);
				Assert.AreEqual(1, documentManager.OpenCount);
			}
			finally
			{
				Directory.Delete(rootPath, recursive: true);
			}
		});

	[TestMethod]
	public void WorkspaceCommandService_RenameForwardsRequestToLanguageServerProvider()
	{
		StaTestHelper.RunInSta(() =>
		{
			var editor = new LuaEditor(new Version(1, 0))
			{
				FilePath = @"C:\Scripts\test.lua",
				Text = "local value = 1"
			};
			var textEditorHost = new Mock<ITextEditorHost>();
			var editProvider = new Mock<ILanguageServerRenameProvider>();
			editProvider.SetupGet(provider => provider.SupportsRename).Returns(true);
			editProvider
				.Setup(provider => provider.RenameSymbolAsync(It.IsAny<TextRenameRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((TextWorkspaceEdit?)null);

			var service = new TextWorkspaceCommandService(new TextWorkspaceEditApplier(textEditorHost.Object), editProvider.Object);
			TextWorkspaceCommandResult result = service.RenameSymbolAsync(editor, 3, 4, "renamed").GetAwaiter().GetResult();

			Assert.AreEqual(TextWorkspaceCommandStatus.NoChanges, result.Status);
			editProvider.Verify(provider => provider.RenameSymbolAsync(
				It.Is<TextRenameRequest>(request =>
					request.FilePath == editor.FilePath
					&& request.DocumentText == editor.Text
					&& request.Position.Line == 3
					&& request.Position.Character == 4
					&& request.NewName == "renamed"),
				It.IsAny<CancellationToken>()), Times.Once);
			editor.Dispose();
		});
	}

	private sealed class TestDocumentBridge(WorkspaceDocumentSnapshot snapshot) : IWorkspaceDocumentManager
	{
		private readonly WorkspaceDocumentSnapshot _snapshot = snapshot;

		public int OpenCount { get; private set; }

		public Task<WorkspaceDocumentManagerOpenResult> OpenAsync(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			CancellationToken cancellationToken = default)
		{
			OpenCount++;
			return Task.FromResult(new WorkspaceDocumentManagerOpenResult(
				WorkspaceDocumentManagerOpenOutcome.AlreadyOpen,
				_snapshot));
		}

		public IWorkspaceDocumentReader Documents => throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerOpenResult> OpenWithViewAsync(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			IWorkspaceDocumentView view,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerMutationResult> ReplaceAsync(WorkspaceDocumentReplaceRequest request)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerMutationResult> DiscardAsync(WorkspaceDocumentDiscardRequest request)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerRenameResult> RenameAsync(
			WorkspaceDocumentRenameRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerSaveAsResult> SaveAsAsync(
			WorkspaceDocumentSaveAsRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerDeleteResult> DeleteAsync(
			WorkspaceDocumentDeleteRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerDirectoryRenameResult> RenameDirectoryAsync(
			WorkspaceDocumentDirectoryRenameRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerDirectoryDeleteResult> DeleteDirectoryAsync(
			WorkspaceDocumentDirectoryDeleteRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerCommitResult> CommitAsync(
			WorkspaceDocumentCommitRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerReloadResult> ReloadAsync(
			WorkspaceDocumentReloadRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentManagerConflictResolutionResult> ResolveExternalConflictAsync(
			WorkspaceDocumentConflictResolutionRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public void UnregisterOpenView(IWorkspaceDocumentView view) { }

		public Task StopAsync() => Task.CompletedTask;

		public ValueTask DisposeAsync() => ValueTask.CompletedTask;
	}
}