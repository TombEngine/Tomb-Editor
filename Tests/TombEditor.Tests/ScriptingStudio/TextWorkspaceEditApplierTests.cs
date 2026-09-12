using ICSharpCode.AvalonEdit.Document;
using Moq;
using Nickelony.LanguageServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.AvalonEdit.Editing;
using Nickelony.IDEKit.Core.Editing;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.Lua;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class TextWorkspaceEditApplierTests
{
	[TestMethod]
	public void Apply_MultipleFilesSynchronizesOpenEditorsAndReplaysSnapshots()
	{
		StaTestHelper.RunInSta(() =>
		{
			var firstEditor = CreateEditor(@"C:\Scripts\first.lua", "first");
			var firstMirror = CreateEditor(@"C:\Scripts\first.lua", "first");
			var secondEditor = CreateEditor(@"C:\Scripts\second.lua", "second");
			var host = new TestEditorHost(firstEditor, firstMirror, secondEditor);
			var applier = new TextWorkspaceEditApplier(host);

			try
			{
				var workspaceEdit = new TextWorkspaceEdit(
					[
						new TextDocumentEdit(@"C:\Scripts\first.lua", [CreateEdit(1, 1, 1, 6, "changed")]),
						new TextDocumentEdit(@"C:\Scripts\second.lua", [CreateEdit(1, 1, 1, 7, "updated")])
					]);

				TextWorkspaceEditApplicationResult result = applier.Apply(workspaceEdit);

				Assert.AreEqual(TextWorkspaceEditApplicationStatus.Completed, result.Status);
				Assert.IsTrue(result.HasChanges);
				Assert.AreEqual(2, result.Targets.Count);
				CollectionAssert.AreEquivalent(
					new[] { @"C:\Scripts\first.lua", @"C:\Scripts\second.lua" },
					result.ChangedTargetIds.ToArray());
				Assert.AreEqual("changed", firstEditor.Text);
				Assert.AreEqual("changed", firstMirror.Text);
				Assert.AreEqual("updated", secondEditor.Text);

				applier.ApplyBeforeSnapshot(result.ChangeSet);
				Assert.AreEqual("first", firstEditor.Text);
				Assert.AreEqual("first", firstMirror.Text);
				Assert.AreEqual("second", secondEditor.Text);

				applier.ApplyAfterSnapshot(result.ChangeSet);
				Assert.AreEqual("changed", firstEditor.Text);
				Assert.AreEqual("changed", firstMirror.Text);
				Assert.AreEqual("updated", secondEditor.Text);
			}
			finally
			{
				firstEditor.Dispose();
				firstMirror.Dispose();
				secondEditor.Dispose();
			}
		});
	}

	[TestMethod]
	public void Apply_UsesDeterministicTargetOrder()
	{
		StaTestHelper.RunInSta(() =>
		{
			var firstEditor = CreateEditor(@"C:\Scripts\first.lua", "first");
			var secondEditor = CreateEditor(@"C:\Scripts\second.lua", "second");
			var applier = new TextWorkspaceEditApplier(new TestEditorHost(firstEditor, secondEditor));

			try
			{
				var workspaceEdit = new TextWorkspaceEdit(
					[
						new TextDocumentEdit(@"C:\Scripts\second.lua", [CreateEdit(1, 1, 1, 7, "updated")]),
						new TextDocumentEdit(@"C:\Scripts\first.lua", [CreateEdit(1, 1, 1, 6, "changed")])
					]);

				TextWorkspaceEditApplicationResult result = applier.Apply(workspaceEdit);

				CollectionAssert.AreEqual(
					new[] { @"C:\Scripts\first.lua", @"C:\Scripts\second.lua" },
					result.Targets.Select(target => target.TargetId).ToArray());
				CollectionAssert.AreEqual(
					new[] { @"C:\Scripts\first.lua", @"C:\Scripts\second.lua" },
					result.ChangeSet.DocumentChanges.Select(change => change.FilePath).ToArray());
			}
			finally
			{
				firstEditor.Dispose();
				secondEditor.Dispose();
			}
		});
	}

	[TestMethod]
	public void Apply_InvalidRange_RejectsBeforeMutatingAnyFile()
	{
		StaTestHelper.RunInSta(() =>
		{
			var firstEditor = CreateEditor(@"C:\Scripts\first.lua", "first");
			var secondEditor = CreateEditor(@"C:\Scripts\second.lua", "second");
			var host = new TestEditorHost(firstEditor, secondEditor);
			var applier = new TextWorkspaceEditApplier(host);

			try
			{
				var workspaceEdit = new TextWorkspaceEdit(
					[
						new TextDocumentEdit(@"C:\Scripts\first.lua", [CreateEdit(1, 1, 1, 6, "changed")]),
						new TextDocumentEdit(@"C:\Scripts\second.lua", [CreateEdit(2, 1, 2, 2, "invalid")])
					]);

				TextWorkspaceEditApplicationResult result = applier.Apply(workspaceEdit);
				Assert.AreEqual(TextWorkspaceEditApplicationStatus.ValidationFailed, result.Status);
				Assert.IsFalse(result.HasChanges);
				Assert.AreEqual(0, result.ChangedTargetIds.Count);
				Assert.AreEqual(0, result.UnknownTargetIds.Count);
				Assert.AreEqual("first", firstEditor.Text);
				Assert.AreEqual("second", secondEditor.Text);
			}
			finally
			{
				firstEditor.Dispose();
				secondEditor.Dispose();
			}
		});
	}

	[TestMethod]
	public void Apply_OverlappingEdits_RejectsBeforeMutatingTheEditor()
	{
		StaTestHelper.RunInSta(() =>
		{
			var editor = CreateEditor(@"C:\Scripts\overlap.lua", "abcdef");
			var host = new TestEditorHost(editor);
			var applier = new TextWorkspaceEditApplier(host);

			try
			{
				var workspaceEdit = new TextWorkspaceEdit(
					[
						new TextDocumentEdit(@"C:\Scripts\overlap.lua", [
							CreateEdit(1, 2, 1, 5, "X"),
							CreateEdit(1, 3, 1, 4, "Y")
						])
					]);

				TextWorkspaceEditApplicationResult result = applier.Apply(workspaceEdit);
				Assert.AreEqual(TextWorkspaceEditApplicationStatus.ValidationFailed, result.Status);
				Assert.IsFalse(result.HasChanges);
				Assert.AreEqual("abcdef", editor.Text);
			}
			finally
			{
				editor.Dispose();
			}
		});
	}

	[TestMethod]
	public void Apply_NoOpEdit_CompletesWithoutAChangeSet()
	{
		StaTestHelper.RunInSta(() =>
		{
			var editor = CreateEditor(@"C:\Scripts\no-op.lua", "same");
			var applier = new TextWorkspaceEditApplier(new TestEditorHost(editor));

			try
			{
				var workspaceEdit = new TextWorkspaceEdit(
					[new TextDocumentEdit(@"C:\Scripts\no-op.lua", [CreateEdit(1, 1, 1, 5, "same")])]);

				TextWorkspaceEditApplicationResult result = applier.Apply(workspaceEdit);

				Assert.AreEqual(TextWorkspaceEditApplicationStatus.Completed, result.Status);
				Assert.IsFalse(result.HasChanges);
				Assert.AreEqual(1, result.PreparedOperationCount);
				Assert.AreEqual(TextWorkspaceEditTargetStatus.Applied, result.Targets[0].Status);
				Assert.AreEqual("same", editor.Text);
			}
			finally
			{
				editor.Dispose();
			}
		});
	}

	[TestMethod]
	public void Apply_RestoresCapturedSelectionAfterAllTargetsApply()
	{
		StaTestHelper.RunInSta(() =>
		{
			var editor = CreateEditor(@"C:\Scripts\selection.lua", "abcdef");
			editor.Select(1, 2);
			editor.CaretOffset = 3;
			var applier = new TextWorkspaceEditApplier(new TestEditorHost(editor));

			try
			{
				var workspaceEdit = new TextWorkspaceEdit(
					[new TextDocumentEdit(@"C:\Scripts\selection.lua", [CreateEdit(1, 1, 1, 1, "X")])]);

				applier.Apply(workspaceEdit, TextWorkspaceEditSelectionState.Capture(editor, editor.FilePath));

				Assert.AreEqual("Xabcdef", editor.Text);
				Assert.AreEqual(2, editor.SelectionStart);
				Assert.AreEqual(2, editor.SelectionLength);
				Assert.AreEqual(4, editor.CaretOffset);
			}
			finally
			{
				editor.Dispose();
			}
		});
	}

	[TestMethod]
	public void Apply_UnsupportedTargetCapability_FailsBeforeMutation()
	{
		StaTestHelper.RunInSta(() =>
		{
			var editor = CreateEditor(@"C:\Scripts\unsupported.lua", "before");
			var host = new TestEditorHost(editor);
			host.SetTarget(editor, new UnversionedTarget("before"));
			var applier = new TextWorkspaceEditApplier(host);

			try
			{
				var workspaceEdit = new TextWorkspaceEdit(
					[new TextDocumentEdit(@"C:\Scripts\unsupported.lua", [CreateEdit(1, 1, 1, 7, "changed")])]);

				TextWorkspaceEditApplicationResult result = applier.Apply(workspaceEdit);

				Assert.AreEqual(TextWorkspaceEditApplicationStatus.ValidationFailed, result.Status);
				Assert.AreEqual("before", editor.Text);
				Assert.AreEqual("UnsupportedTargetCapability", result.Failure?.Code);
				Assert.AreEqual(1, result.Targets.Count);
				Assert.AreEqual(TextWorkspaceEditTargetStatus.NotApplied, result.Targets[0].Status);
			}
			finally
			{
				editor.Dispose();
			}
		});
	}

	[TestMethod]
	public void Apply_RuntimeFailureReportsChangedAndUnknownTargets()
	{
		StaTestHelper.RunInSta(() =>
		{
			var firstEditor = CreateEditor(@"C:\Scripts\first.lua", "first");
			var secondEditor = CreateEditor(@"C:\Scripts\second.lua", "second");
			var host = new TestEditorHost(firstEditor, secondEditor);
			host.SetTarget(secondEditor, new PartiallyMutatingThrowingTarget("second"));
			var applier = new TextWorkspaceEditApplier(host);

			try
			{
				var workspaceEdit = new TextWorkspaceEdit(
					[
						new TextDocumentEdit(@"C:\Scripts\first.lua", [CreateEdit(1, 1, 1, 6, "changed")]),
						new TextDocumentEdit(@"C:\Scripts\second.lua", [CreateEdit(1, 1, 1, 7, "updated")])
					]);

				TextWorkspaceEditApplicationResult result = applier.Apply(workspaceEdit);

				Assert.AreEqual(TextWorkspaceEditApplicationStatus.PartiallyApplied, result.Status);
				CollectionAssert.Contains(result.ChangedTargetIds.ToArray(), @"C:\Scripts\first.lua");
				CollectionAssert.Contains(result.UnknownTargetIds.ToArray(), @"C:\Scripts\second.lua");
				Assert.AreEqual("changed", firstEditor.Text);
				Assert.AreEqual(2, result.Targets.Count);
				Assert.AreEqual(TextWorkspaceEditTargetStatus.Unknown, result.Targets[1].Status);
				Assert.AreEqual(1, result.ChangeSet.DocumentChanges.Count);
			}
			finally
			{
				firstEditor.Dispose();
				secondEditor.Dispose();
			}
		});
	}

	private static LuaEditor CreateEditor(string filePath, string content)
		=> new(new Version(1, 0)) { FilePath = filePath, Content = content };

	private static TextEdit CreateEdit(int startLine, int startColumn, int endLine, int endColumn, string newText)
		=> new(new TextDocumentRange(startLine, startColumn, endLine, endColumn), newText);

	private sealed class TestEditorHost(params LuaEditor[] editors) : ITextEditorHost
	{
		private readonly IReadOnlyList<LuaEditor> _editors = editors;
		private readonly Dictionary<TextEditorBase, ITextEditTarget> _targets = [];

		public TextEditorBase OpenTextEditor(string filePath, EditorType editorType = EditorType.Default, bool openSourceView = false)
			=> _editors.First(editor => string.Equals(editor.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

		public IReadOnlyList<IEditorControl> GetOpenEditors(string filePath)
			=> _editors
				.Where(editor => string.Equals(editor.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
				.Cast<IEditorControl>()
				.ToArray();

		public TResult ExecutePreservingSelection<TResult>(Func<TResult> action)
			=> action();

		public void SetTarget(TextEditorBase editor, ITextEditTarget target)
		{
			_targets[editor] = target;
		}

		public ITextEditTarget GetTextEditTarget(TextEditorBase editor)
			=> _targets.TryGetValue(editor, out ITextEditTarget? target)
				? target
				: new AvalonEditTextEditTarget(editor);
	}

	private sealed class UnversionedTarget(string initialText) : ITextEditTarget
	{
		public string Text { get; private set; } = initialText;

		public void Apply(IReadOnlyList<TextEditOperation> operations)
		{
			Text = "changed";
		}
	}

	private sealed class PartiallyMutatingThrowingTarget(string initialText) : ITextEditTarget, ITextEditTargetVersion
	{
		private string _text = initialText;
		private bool _hasApplied;

		public string Text => _hasApplied
			? throw new InvalidOperationException("The target state is unavailable after the runtime failure.")
			: _text;

		public long Version { get; private set; }

		public void Apply(IReadOnlyList<TextEditOperation> operations)
		{
			_text = "updated";
			Version++;
			_hasApplied = true;
			throw new InvalidOperationException("Synthetic target failure.");
		}
	}
}