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
				var referencesProvider = new Mock<ITextReferencesProvider>();
				referencesProvider.SetupGet(provider => provider.SupportsReferences).Returns(true);
				referencesProvider
					.Setup(provider => provider.GetReferencesAsync(It.IsAny<TextReferenceRequest>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync([
						new TextReferenceLocation(secondFilePath, 2, 1, 2, 6),
						new TextReferenceLocation(firstFilePath, 2, 8, 2, 13),
						new TextReferenceLocation(firstFilePath, 1, 7, 1, 11)
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
				var referencesProvider = new Mock<ITextReferencesProvider>();
				referencesProvider.SetupGet(provider => provider.SupportsReferences).Returns(true);
				referencesProvider
					.Setup(provider => provider.GetReferencesAsync(It.IsAny<TextReferenceRequest>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync([new TextReferenceLocation(filePath, 1, 1, 1, 7)]);

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
			var editProvider = new Mock<ITextEditProvider>();
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
					&& request.Line == 3
					&& request.Column == 4
					&& request.NewName == "renamed"),
				It.IsAny<CancellationToken>()), Times.Once);
			editor.Dispose();
		});
	}

	private sealed class TestDocumentBridge(WorkspaceDocumentSnapshot snapshot) : IWorkspaceDocumentManager
	{
		private readonly WorkspaceDocumentSnapshot _snapshot = snapshot;

		public int OpenCount { get; private set; }

		public Task<WorkspaceDocumentOpenResult> OpenAsync(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			CancellationToken cancellationToken = default)
		{
			OpenCount++;
			return Task.FromResult(new WorkspaceDocumentOpenResult(
				WorkspaceDocumentOpenStatus.AlreadyOpen,
				_snapshot));
		}

		public IReadOnlyList<WorkspaceDocumentSnapshot> GetSnapshotsUnderDirectory(string directoryPath)
			=> new[] { _snapshot };

		public Task<WorkspaceDocumentManagerOpenResult> OpenWithViewAsync(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			IWorkspaceDocumentView view,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public WorkspaceDocumentManagerOpenResult OpenWithView(
			string? filePath,
			WorkspaceDocumentOpenOptions options,
			IWorkspaceDocumentView view,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public WorkspaceDocumentMutationResult Replace(WorkspaceDocumentReplaceRequest request)
			=> throw new NotSupportedException();

		public WorkspaceDocumentMutationResult Discard(WorkspaceDocumentDiscardRequest request)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentRenameResult> RenameAsync(
			WorkspaceDocumentRenameRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentSaveAsResult> SaveAsAsync(
			WorkspaceDocumentSaveAsRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentDeleteResult> DeleteAsync(
			WorkspaceDocumentDeleteRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentDirectoryRenameResult> RenameDirectoryAsync(
			WorkspaceDocumentDirectoryRenameRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentDirectoryDeleteResult> DeleteDirectoryAsync(
			WorkspaceDocumentDirectoryDeleteRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentCommitResult> CommitAsync(
			WorkspaceDocumentCommitRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentReloadResult> ReloadAsync(
			WorkspaceDocumentReloadRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public Task<WorkspaceDocumentConflictResolutionResult> ResolveExternalConflictAsync(
			WorkspaceDocumentConflictResolutionRequest request,
			CancellationToken cancellationToken = default)
			=> throw new NotSupportedException();

		public void UnregisterOpenView(IWorkspaceDocumentView view) { }

		public Task StopAsync() => Task.CompletedTask;

		public ValueTask DisposeAsync() => ValueTask.CompletedTask;
	}
}