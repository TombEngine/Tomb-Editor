#nullable enable

using Moq;
using System;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.TextEditing;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
[TestCategory("TextEditorBaseModernization")]
public sealed class EditorSessionTests
{
	private static readonly TextFileFormat FileFormat = new(
		TextEncodingKind.Utf8,
		false,
		TextNewlineStyle.Lf);

	[TestMethod]
	public void TransientSession_ClosesOnlyAcquiredViewAndRestoresPreviousActiveEditor()
		=> StaTestHelper.RunInSta(() =>
		{
			const string filePath = "C:\\Scripts\\transient.lua";
			var previousEditor = new Mock<IEditorControl>().Object;
			var acquiredEditor = new Mock<IEditorControl>().Object;
			IEditorControl? currentEditor = previousEditor;
			bool viewOpened = false;
			var documentController = new Mock<IEditorDocumentController>();
			documentController.SetupGet(controller => controller.CurrentEditor).Returns(() => currentEditor);
			documentController
				.Setup(controller => controller.FindEditorsOfFile(filePath))
				.Returns(() => viewOpened ? [acquiredEditor] : []);
			documentController
				.Setup(controller => controller.OpenSourceFile(filePath, It.IsAny<DocumentLoadOptions>()))
				.Callback(() =>
				{
					viewOpened = true;
					currentEditor = acquiredEditor;
				});
			documentController.Setup(controller => controller.ContainsEditor(It.IsAny<IEditorControl>())).Returns(true);
			documentController.Setup(controller => controller.TryCloseEditor(acquiredEditor)).Returns(true);
			documentController
				.Setup(controller => controller.ActivateEditor(It.IsAny<IEditorControl>()))
				.Callback((IEditorControl editor) => currentEditor = editor);

			WorkspaceDocumentSnapshot snapshot = CreateSnapshot(filePath, "canonical");
			var host = new DocumentControllerTextEditorHost(documentController.Object);
			EditorSessionOpenResult result = host.Open(
				snapshot,
				new(EditorSessionMode.Transient));

			Assert.AreEqual(EditorSessionOpenStatus.Opened, result.Status);
			Assert.IsNotNull(result.Session);
			IEditorSession session = result.Session ?? throw new AssertFailedException("Expected an acquired session.");
			Assert.AreEqual(snapshot.DocumentKey, session.DocumentKey);
			Assert.AreEqual(snapshot.DocumentId, session.DocumentId);
			Assert.IsTrue(session.IsActive);

			session.Dispose();
			session.Dispose();

			documentController.Verify(controller => controller.TryCloseEditor(acquiredEditor), Times.Once);
			documentController.Verify(controller => controller.ActivateEditor(previousEditor), Times.Once);
			Assert.IsFalse(session.IsActive);
			Assert.AreSame(previousEditor, currentEditor);
		});

	[TestMethod]
	public void ExistingDirtyEditor_TransientSessionDoesNotCloseUnownedView()
		=> StaTestHelper.RunInSta(() =>
		{
			const string filePath = "C:\\Scripts\\dirty.lua";
			var previousEditor = new Mock<IEditorControl>().Object;
			var existingEditor = new Mock<IEditorControl>();
			existingEditor.SetupGet(editor => editor.IsContentChanged).Returns(true);
			IEditorControl? currentEditor = previousEditor;
			var documentController = new Mock<IEditorDocumentController>();
			documentController.SetupGet(controller => controller.CurrentEditor).Returns(() => currentEditor);
			documentController
				.Setup(controller => controller.FindEditorsOfFile(filePath))
				.Returns([existingEditor.Object]);
			documentController.Setup(controller => controller.ContainsEditor(It.IsAny<IEditorControl>())).Returns(true);
			documentController
				.Setup(controller => controller.ActivateEditor(It.IsAny<IEditorControl>()))
				.Callback((IEditorControl editor) => currentEditor = editor);

			var host = new DocumentControllerTextEditorHost(documentController.Object);
			EditorSessionOpenResult result = host.Open(
				CreateSnapshot(filePath, "canonical"),
				new(EditorSessionMode.Transient));

			Assert.AreEqual(EditorSessionOpenStatus.AlreadyOpen, result.Status);
			Assert.IsNotNull(result.Session);
			IEditorSession session = result.Session ?? throw new AssertFailedException("Expected an existing-view session.");
			session.Dispose();

			documentController.Verify(controller => controller.TryCloseEditor(It.IsAny<IEditorControl>()), Times.Never);
			documentController.Verify(controller => controller.ActivateEditor(previousEditor), Times.Once);
			Assert.AreSame(previousEditor, currentEditor);
		});

	private static WorkspaceDocumentSnapshot CreateSnapshot(string filePath, string content)
		=> new(
			new WorkspaceDocumentKey(Guid.NewGuid()),
			filePath,
			filePath,
			0,
			0,
			false,
			new StringTextSnapshot(content, filePath),
			FileFormat,
			FileStamp.Missing);
}