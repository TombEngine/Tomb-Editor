using Nickelony.LanguageServer.Abstractions;
using Nickelony.IDEKit.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using Nickelony.IDEKit.Core.Editing;
using Nickelony.IDEKit.Core.Pathing;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.Workspace.Documents;
using Nickelony.IDEKit.Workspace.Editing;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombLib.Scripting.UI.Editing;

/// <summary>
/// Applies shared workspace edits through a host-provided text-editor seam.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class TextWorkspaceEditApplier
{
	private readonly ITextEditorHost _textEditorHost;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextWorkspaceEditApplier"/> class.
	/// </summary>
	/// <param name="textEditorHost">The host used to open and synchronize text editors.</param>
	public TextWorkspaceEditApplier(ITextEditorHost textEditorHost)
	{
		ArgumentNullException.ThrowIfNull(textEditorHost);
		_textEditorHost = textEditorHost;
	}

	/// <summary>
	/// Applies a workspace edit after resolving and preflighting every target.
	/// </summary>
	/// <param name="workspaceEdit">The workspace edit to apply.</param>
	/// <param name="selectionState">The optional selection state to restore for the initiating editor.</param>
	/// <returns>The explicit application result and its non-atomic change set.</returns>
	public WorkspaceEditApplicationResult Apply(TextWorkspaceEdit workspaceEdit, TextWorkspaceEditSelectionState? selectionState = null)
	{
		ArgumentNullException.ThrowIfNull(workspaceEdit);

		if (!workspaceEdit.HasChanges)
			return CreateCompletedResult([], 0, []);

		PreparedWorkspaceEdit preparedEdit = Preflight(workspaceEdit, selectionState);
		if (!preparedEdit.IsValid)
			return WorkspaceEditApplicationResult.ValidationFailed(
				preparedEdit.Diagnostics,
				preparedEdit.Failure,
				// Host flavor: target ids are Windows file paths, so deduplicate them case-insensitively.
				LocalPathComparisonPolicy.CaseInsensitive);

		return _textEditorHost.ExecutePreservingSelection(() =>
		{
			var documentChanges = new List<WorkspaceDocumentChange>();
			var targetResults = new List<WorkspaceEditTargetResult>(preparedEdit.Targets.Count);
			var unknownTargetIds = new List<string>();

			for (int targetIndex = 0; targetIndex < preparedEdit.Targets.Count; targetIndex++)
			{
				PreparedTarget target = preparedEdit.Targets[targetIndex];
				if (!TryConfirmPreflightState(target, out WorkspaceOperationFailure? stateFailure))
				{
					WorkspaceOperationFailure failure = stateFailure ?? new WorkspaceOperationFailure(
						"TargetUnavailable",
						$"The target '{target.TargetId}' could not be confirmed.");
					targetResults.Add(target.CreateResult(
						WorkspaceEditTargetOutcome.Unknown,
						TryGetVersion(target.EditTarget),
						failure));
					unknownTargetIds.Add(target.TargetId);
					AddNotAppliedTargetResults(preparedEdit.Targets, targetResults, targetIndex + 1);
					return CreatePartialResult(
						preparedEdit,
						documentChanges,
						targetResults,
						unknownTargetIds,
						failure);
				}

				if (string.Equals(target.BeforeContent, target.ExpectedAfterContent, StringComparison.Ordinal))
				{
					targetResults.Add(target.CreateResult(
						WorkspaceEditTargetOutcome.Applied,
						TryGetVersion(target.EditTarget)));
					continue;
				}

				try
				{
					target.EditTarget.Apply(target.Edits);
					string actualContent = target.EditTarget.Text;
					long? actualVersion = TryGetVersion(target.EditTarget);

					if (!string.Equals(actualContent, target.ExpectedAfterContent, StringComparison.Ordinal))
					{
						if (!string.Equals(actualContent, target.BeforeContent, StringComparison.Ordinal))
						{
							documentChanges.Add(new WorkspaceDocumentChange
							{
								TargetId = target.TargetId,
								BeforeContent = target.BeforeContent,
								AfterContent = actualContent ?? string.Empty
							});
							// The content changed, but not to the prepared final content, so the target's final
							// state is unconfirmed.
							targetResults.Add(target.CreateResult(
								WorkspaceEditTargetOutcome.Unknown,
								actualVersion,
								new WorkspaceOperationFailure(
									"UnexpectedTargetContent",
									"The target did not reach the prepared final content.")));
						}
						else
						{
							unknownTargetIds.Add(target.TargetId);
							targetResults.Add(target.CreateResult(
								WorkspaceEditTargetOutcome.Unknown,
								actualVersion,
								new WorkspaceOperationFailure(
									"TargetNotChanged",
									"The target did not apply the prepared operations.")));
						}

						AddNotAppliedTargetResults(preparedEdit.Targets, targetResults, targetIndex + 1);
						return CreatePartialResult(
							preparedEdit,
							documentChanges,
							targetResults,
							unknownTargetIds,
							new WorkspaceOperationFailure(
								"UnexpectedTargetContent",
								"The target did not reach the prepared final content."));
					}

					documentChanges.Add(new WorkspaceDocumentChange
					{
						TargetId = target.TargetId,
						BeforeContent = target.BeforeContent,
						AfterContent = actualContent
					});
					targetResults.Add(target.CreateResult(
						WorkspaceEditTargetOutcome.Applied,
						actualVersion));

					if (target.SelectionState is not null && target.RestoredSelectionState is not null)
						RestoreSelectionState(target.Editor, target.RestoredSelectionState.Value);

					if (target.Editor.WorkspaceEditTarget is null)
						target.Editor.RunContentChangedWorker();
					if (target.Editor.WorkspaceEditTarget is null)
						SynchronizeOpenEditors(target.TargetId, target.Editor);
				}
				catch (Exception exception)
				{
					if (TryGetContent(target.EditTarget, out string? actualContent))
					{
						if (!string.Equals(actualContent, target.BeforeContent, StringComparison.Ordinal))
						{
							documentChanges.Add(new WorkspaceDocumentChange
							{
								TargetId = target.TargetId,
								BeforeContent = target.BeforeContent,
								AfterContent = actualContent ?? string.Empty
							});
						}
						else
							unknownTargetIds.Add(target.TargetId);
					}
					else
					{
						unknownTargetIds.Add(target.TargetId);
					}

					WorkspaceOperationFailure failure = new("TargetApplicationFailed", exception.Message, exception);
					// The target threw after it may have changed the document, so its final state is
					// unconfirmed.
					targetResults.Add(target.CreateResult(
						WorkspaceEditTargetOutcome.Unknown,
						TryGetVersion(target.EditTarget),
						failure));
					AddNotAppliedTargetResults(preparedEdit.Targets, targetResults, targetIndex + 1);
					return CreatePartialResult(
						preparedEdit,
						documentChanges,
						targetResults,
						unknownTargetIds,
						failure);
				}
			}

			return CreateCompletedResult(
				documentChanges,
				preparedEdit.PreparedOperationCount,
				targetResults);
		});
	}

	/// <summary>
	/// Applies the before snapshots from a transaction.
	/// </summary>
	/// <param name="transaction">The transaction whose before snapshots should be restored.</param>
	/// <returns>The file paths whose contents changed.</returns>
	public IReadOnlyList<string> ApplyBeforeSnapshot(WorkspaceEditChangeSet transaction)
		=> ApplyContentSnapshots(transaction, static documentChange => documentChange.BeforeContent);

	/// <summary>
	/// Applies the after snapshots from a transaction.
	/// </summary>
	/// <param name="transaction">The transaction whose after snapshots should be restored.</param>
	/// <returns>The file paths whose contents changed.</returns>
	public IReadOnlyList<string> ApplyAfterSnapshot(WorkspaceEditChangeSet transaction)
		=> ApplyContentSnapshots(transaction, static documentChange => documentChange.AfterContent);

	private IReadOnlyList<string> ApplyContentSnapshots(WorkspaceEditChangeSet transaction, Func<WorkspaceDocumentChange, string> selectContent)
	{
		ArgumentNullException.ThrowIfNull(transaction);
		ArgumentNullException.ThrowIfNull(selectContent);

		if (!transaction.HasChanges)
			return [];

		return _textEditorHost.ExecutePreservingSelection(() =>
		{
			var updatedFiles = new List<string>(transaction.DocumentChanges.Count);

			foreach (WorkspaceDocumentChange documentChange in transaction.DocumentChanges)
			{
				TextEditorBase textEditor = _textEditorHost.OpenTextEditor(documentChange.TargetId);
				ApplyDocumentContent(textEditor, selectContent(documentChange));
				if (textEditor.WorkspaceEditTarget is null)
					SynchronizeOpenEditors(documentChange.TargetId, textEditor);
				updatedFiles.Add(documentChange.TargetId);
			}

			return (IReadOnlyList<string>)updatedFiles;
		});
	}

	private static RestoredSelectionState MapSelectionState(TextWorkspaceEditSelectionState selectionState, PreparedTextEdits preparedTextEdits)
	{
		int selectionStart = preparedTextEdits.MapOffset(selectionState.SelectionStart);
		int selectionEnd = preparedTextEdits.MapOffset(selectionState.SelectionEnd);
		int caretOffset = preparedTextEdits.MapOffset(selectionState.CaretOffset);

		if (selectionEnd < selectionStart)
			(selectionStart, selectionEnd) = (selectionEnd, selectionStart);

		return new RestoredSelectionState(selectionStart, selectionEnd, caretOffset);
	}

	private static void RestoreSelectionState(TextEditorBase textEditor, RestoredSelectionState restoredSelectionState)
	{
		int documentLength = textEditor.Document.TextLength;
		int selectionStart = Math.Clamp(restoredSelectionState.SelectionStart, 0, documentLength);
		int selectionEnd = Math.Clamp(restoredSelectionState.SelectionEnd, 0, documentLength);
		int caretOffset = Math.Clamp(restoredSelectionState.CaretOffset, 0, documentLength);

		if (selectionEnd < selectionStart)
			(selectionStart, selectionEnd) = (selectionEnd, selectionStart);

		textEditor.Select(selectionStart, selectionEnd - selectionStart);
		textEditor.CaretOffset = caretOffset;
	}

	private static void ApplyDocumentContent(TextEditorBase textEditor, string content)
	{
		ITextEditTarget? editTarget = textEditor.WorkspaceEditTarget;
		if (editTarget is not null)
		{
			if (string.Equals(editTarget.Text, content, StringComparison.Ordinal))
				return;

			editTarget.Apply(new PreparedTextEdits([new TextEditOperation(0, editTarget.Text.Length, content, 0)]));
			return;
		}

		if (string.Equals(textEditor.Text, content, StringComparison.Ordinal))
			return;

		textEditor.Content = content;
	}

	private PreparedWorkspaceEdit Preflight(
		TextWorkspaceEdit workspaceEdit,
		TextWorkspaceEditSelectionState? selectionState)
	{
		var targets = new List<PreparedTarget>();
		var targetResults = new List<WorkspaceEditTargetResult>();
		var diagnostics = new List<TextEditPreparationDiagnostic>();
		WorkspaceOperationFailure? failure = null;

		IEnumerable<IGrouping<string, TextDocumentEdit>> fileGroups = workspaceEdit.DocumentEdits
			.GroupBy(documentEdit => documentEdit.FilePath, StringComparer.OrdinalIgnoreCase)
			.OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

		foreach (IGrouping<string, TextDocumentEdit> fileGroup in fileGroups)
		{
			string targetId = fileGroup.Key;
			if (string.IsNullOrWhiteSpace(targetId))
			{
				WorkspaceOperationFailure targetFailure = new(
					"InvalidTarget",
					"A workspace edit target must have a file path.");
				failure ??= targetFailure;
				targetResults.Add(new WorkspaceEditTargetResult
				{
					TargetId = targetId,
					ExpectedVersion = 0,
					PreparedOperationCount = 0,
					Outcome = WorkspaceEditTargetOutcome.NotApplied,
					Failure = targetFailure
				});
				continue;
			}

			try
			{
				TextEditorBase editor = _textEditorHost.OpenTextEditor(targetId);
				ITextEditTarget editTarget = _textEditorHost.GetTextEditTarget(editor);
				if (editTarget is not ITextEditTargetVersion)
				{
					WorkspaceOperationFailure targetFailure = new(
						"UnsupportedTargetCapability",
						$"The target '{targetId}' does not expose a document version.");
					failure ??= targetFailure;
					targetResults.Add(new WorkspaceEditTargetResult
					{
						TargetId = targetId,
						ExpectedVersion = 0,
						PreparedOperationCount = 0,
						Outcome = WorkspaceEditTargetOutcome.NotApplied,
						Failure = targetFailure
					});
					continue;
				}

				string beforeContent = editTarget.Text;
				long expectedVersion = TryGetVersion(editTarget) ?? 0;
				var snapshot = new StringTextSnapshot(beforeContent, targetId);
				TextEditPreparationResult preparation = TextEditKernel.Prepare(
					snapshot,
					TextEditInputAdapter.Convert(
						snapshot,
						fileGroup.SelectMany(documentEdit => documentEdit.TextEdits)));
				if (!preparation.IsValid)
				{
					diagnostics.AddRange(preparation.Diagnostics);
					targetResults.Add(new WorkspaceEditTargetResult
					{
						TargetId = targetId,
						ExpectedVersion = expectedVersion,
						ActualVersion = expectedVersion,
						PreparedOperationCount = preparation.Edits.Operations.Count,
						Outcome = WorkspaceEditTargetOutcome.NotApplied
					});
					continue;
				}

				TextWorkspaceEditSelectionState? selectionStateForTarget = selectionState is not null
					&& string.Equals(selectionState.FilePath, targetId, StringComparison.OrdinalIgnoreCase)
						? selectionState
						: null;
				RestoredSelectionState? restoredSelectionState = selectionStateForTarget is null
					? null
					: MapSelectionState(selectionStateForTarget, preparation.Edits);

				var target = new PreparedTarget(
					targetId,
					editor,
					editTarget,
					beforeContent,
					ApplyOperations(beforeContent, preparation.Edits.Operations),
					preparation.Edits,
					expectedVersion,
					selectionStateForTarget,
					restoredSelectionState);
				targets.Add(target);
				targetResults.Add(target.CreateResult(WorkspaceEditTargetOutcome.NotApplied, expectedVersion));
			}
			catch (Exception exception)
			{
				WorkspaceOperationFailure targetFailure = new("TargetResolutionFailed", exception.Message, exception);
				failure ??= targetFailure;
				targetResults.Add(new WorkspaceEditTargetResult
				{
					TargetId = targetId,
					ExpectedVersion = 0,
					PreparedOperationCount = 0,
					Outcome = WorkspaceEditTargetOutcome.NotApplied,
					Failure = targetFailure
				});
			}
		}

		int preparedOperationCount = targets.Sum(target => target.Edits.Operations.Count);
		return new PreparedWorkspaceEdit(
			targets,
			targetResults,
			preparedOperationCount,
			diagnostics,
			failure);
	}

	private static string ApplyOperations(string content, IReadOnlyList<TextEditOperation> operations)
	{
		var builder = new StringBuilder(content);
		foreach (TextEditOperation operation in operations)
		{
			builder.Remove(operation.StartOffset, operation.Length);
			builder.Insert(operation.StartOffset, operation.NewText);
		}

		return builder.ToString();
	}

	private static bool TryConfirmPreflightState(PreparedTarget target, out WorkspaceOperationFailure? failure)
	{
		failure = null;
		try
		{
			if (!string.Equals(target.EditTarget.Text, target.BeforeContent, StringComparison.Ordinal)
				|| TryGetVersion(target.EditTarget) != target.ExpectedVersion)
			{
				failure = new WorkspaceOperationFailure(
					"TargetChangedDuringPreflight",
					$"The target '{target.TargetId}' changed after preflight.");
				return false;
			}

			return true;
		}
		catch (Exception exception)
		{
			failure = new WorkspaceOperationFailure("TargetUnavailable", exception.Message, exception);
			return false;
		}
	}

	private static long? TryGetVersion(ITextEditTarget editTarget)
		=> editTarget is ITextEditTargetVersion versionedTarget
			? versionedTarget.Version
			: null;

	private static bool TryGetContent(ITextEditTarget editTarget, out string? content)
	{
		try
		{
			content = editTarget.Text;
			return true;
		}
		catch
		{
			content = null;
			return false;
		}
	}

	private static void AddNotAppliedTargetResults(
		IReadOnlyList<PreparedTarget> targets,
		ICollection<WorkspaceEditTargetResult> targetResults,
		int startIndex)
	{
		for (int index = startIndex; index < targets.Count; index++)
			targetResults.Add(targets[index].CreateResult(WorkspaceEditTargetOutcome.NotApplied, null));
	}

	private static WorkspaceEditApplicationResult CreateCompletedResult(
		IReadOnlyList<WorkspaceDocumentChange> documentChanges,
		int preparedOperationCount,
		IReadOnlyList<WorkspaceEditTargetResult> targetResults)
		=> WorkspaceEditApplicationResult.Completed(
			preparedOperationCount,
			targetResults,
			new WorkspaceEditChangeSet(documentChanges),
			// Host flavor: target ids are Windows file paths, so deduplicate them case-insensitively.
			LocalPathComparisonPolicy.CaseInsensitive);

	private static WorkspaceEditApplicationResult CreatePartialResult(
		PreparedWorkspaceEdit preparedEdit,
		IReadOnlyList<WorkspaceDocumentChange> documentChanges,
		IReadOnlyList<WorkspaceEditTargetResult> targetResults,
		IReadOnlyList<string> unknownTargetIds,
		WorkspaceOperationFailure failure)
		=> WorkspaceEditApplicationResult.PartiallyApplied(
			preparedEdit.PreparedOperationCount,
			targetResults,
			unknownTargetIds,
			failure,
			new WorkspaceEditChangeSet(documentChanges),
			// Host flavor: target ids are Windows file paths, so deduplicate them case-insensitively.
			LocalPathComparisonPolicy.CaseInsensitive);

	private void SynchronizeOpenEditors(string filePath, TextEditorBase sourceEditor)
	{
		foreach (IEditorControl editorControl in _textEditorHost.GetOpenEditors(filePath))
		{
			if (editorControl is not TextEditorBase editor)
				continue;

			if (ReferenceEquals(editor, sourceEditor))
				continue;

			ApplyDocumentContent(editor, sourceEditor.Text);
		}
	}

	private readonly record struct RestoredSelectionState(int SelectionStart, int SelectionEnd, int CaretOffset);

	private sealed class PreparedWorkspaceEdit(
		IReadOnlyList<PreparedTarget> targets,
		IReadOnlyList<WorkspaceEditTargetResult> targetResults,
		int preparedOperationCount,
		IReadOnlyList<TextEditPreparationDiagnostic> diagnostics,
		WorkspaceOperationFailure? failure)
	{
		public IReadOnlyList<PreparedTarget> Targets { get; } = targets;
		public IReadOnlyList<WorkspaceEditTargetResult> TargetResults { get; } = targetResults;
		public int PreparedOperationCount { get; } = preparedOperationCount;
		public IReadOnlyList<TextEditPreparationDiagnostic> Diagnostics { get; } = diagnostics;
		public WorkspaceOperationFailure? Failure { get; } = failure;
		public bool IsValid => Failure is null && Diagnostics.Count == 0;
	}

	private sealed class PreparedTarget(
		string targetId,
		TextEditorBase editor,
		ITextEditTarget editTarget,
		string beforeContent,
		string expectedAfterContent,
		PreparedTextEdits edits,
		long expectedVersion,
		TextWorkspaceEditSelectionState? selectionState,
		RestoredSelectionState? restoredSelectionState)
	{
		public string TargetId { get; } = targetId;
		public TextEditorBase Editor { get; } = editor;
		public ITextEditTarget EditTarget { get; } = editTarget;
		public string BeforeContent { get; } = beforeContent;
		public string ExpectedAfterContent { get; } = expectedAfterContent;
		public PreparedTextEdits Edits { get; } = edits;
		public long ExpectedVersion { get; } = expectedVersion;
		public TextWorkspaceEditSelectionState? SelectionState { get; } = selectionState;
		public RestoredSelectionState? RestoredSelectionState { get; } = restoredSelectionState;

		public WorkspaceEditTargetResult CreateResult(
			WorkspaceEditTargetOutcome status,
			long? actualVersion,
			WorkspaceOperationFailure? failure = null)
			=> new()
			{
				TargetId = TargetId,
				ExpectedVersion = ExpectedVersion,
				ActualVersion = actualVersion,
				PreparedOperationCount = Edits.Operations.Count,
				Outcome = status,
				Failure = failure
			};
	}
}
