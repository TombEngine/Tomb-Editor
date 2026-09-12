using Nickelony.LanguageServer.Abstractions;
using Nickelony.IDEKit.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using Nickelony.IDEKit.Core.Editing;
using Nickelony.IDEKit.Core.Text;
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
	public TextWorkspaceEditApplicationResult Apply(TextWorkspaceEdit workspaceEdit, TextWorkspaceEditSelectionState? selectionState = null)
	{
		ArgumentNullException.ThrowIfNull(workspaceEdit);

		if (!workspaceEdit.HasEdits)
			return CreateCompletedResult([], 0, []);

		PreparedWorkspaceEdit preparedEdit = Preflight(workspaceEdit, selectionState);
		if (!preparedEdit.IsValid)
			return new TextWorkspaceEditApplicationResult(
				TextWorkspaceEditApplicationStatus.ValidationFailed,
				preparedEdit.PreparedOperationCount,
				preparedEdit.TargetResults,
				[],
				[],
				preparedEdit.Diagnostics,
				preparedEdit.Failure,
				new TextWorkspaceEditTransaction([]));

		return _textEditorHost.ExecutePreservingSelection(() =>
		{
			var documentChanges = new List<TextWorkspaceDocumentChange>();
			var targetResults = new List<TextWorkspaceEditTargetResult>(preparedEdit.Targets.Count);
			var changedTargetIds = new List<string>();
			var unknownTargetIds = new List<string>();

			for (int targetIndex = 0; targetIndex < preparedEdit.Targets.Count; targetIndex++)
			{
				PreparedTarget target = preparedEdit.Targets[targetIndex];
				if (!TryConfirmPreflightState(target, out TextWorkspaceEditFailure? stateFailure))
				{
					TextWorkspaceEditFailure failure = stateFailure ?? new TextWorkspaceEditFailure(
						"TargetUnavailable",
						$"The target '{target.TargetId}' could not be confirmed.");
					targetResults.Add(target.CreateResult(
						TextWorkspaceEditTargetStatus.Unknown,
						TryGetVersion(target.EditTarget),
						failure));
					unknownTargetIds.Add(target.TargetId);
					AddNotAppliedTargetResults(preparedEdit.Targets, targetResults, targetIndex + 1);
					return CreatePartialResult(
						preparedEdit,
						documentChanges,
						targetResults,
						changedTargetIds,
						unknownTargetIds,
						failure);
				}

				if (string.Equals(target.BeforeContent, target.ExpectedAfterContent, StringComparison.Ordinal))
				{
					targetResults.Add(target.CreateResult(
						TextWorkspaceEditTargetStatus.Applied,
						TryGetVersion(target.EditTarget)));
					continue;
				}

				try
				{
					target.EditTarget.Apply(target.Operations);
					string actualContent = target.EditTarget.Text;
					long? actualVersion = TryGetVersion(target.EditTarget);

					if (!string.Equals(actualContent, target.ExpectedAfterContent, StringComparison.Ordinal))
					{
						if (!string.Equals(actualContent, target.BeforeContent, StringComparison.Ordinal))
						{
							documentChanges.Add(new TextWorkspaceDocumentChange(
								target.TargetId,
								target.BeforeContent,
								actualContent ?? string.Empty));
							changedTargetIds.Add(target.TargetId);
							targetResults.Add(target.CreateResult(
								TextWorkspaceEditTargetStatus.Changed,
								actualVersion,
								new TextWorkspaceEditFailure(
									"UnexpectedTargetContent",
									"The target did not reach the prepared final content.")));
						}
						else
						{
							unknownTargetIds.Add(target.TargetId);
							targetResults.Add(target.CreateResult(
								TextWorkspaceEditTargetStatus.Unknown,
								actualVersion,
								new TextWorkspaceEditFailure(
									"TargetNotChanged",
									"The target did not apply the prepared operations.")));
						}

						AddNotAppliedTargetResults(preparedEdit.Targets, targetResults, targetIndex + 1);
						return CreatePartialResult(
							preparedEdit,
							documentChanges,
							targetResults,
							changedTargetIds,
							unknownTargetIds,
							new TextWorkspaceEditFailure(
								"UnexpectedTargetContent",
								"The target did not reach the prepared final content."));
					}

					documentChanges.Add(new TextWorkspaceDocumentChange(
						target.TargetId,
						target.BeforeContent,
						actualContent));
					changedTargetIds.Add(target.TargetId);
					targetResults.Add(target.CreateResult(
						TextWorkspaceEditTargetStatus.Applied,
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
							documentChanges.Add(new TextWorkspaceDocumentChange(
								target.TargetId,
								target.BeforeContent,
								actualContent ?? string.Empty));
							changedTargetIds.Add(target.TargetId);
						}
						else
							unknownTargetIds.Add(target.TargetId);
					}
					else
					{
						unknownTargetIds.Add(target.TargetId);
					}

					TextWorkspaceEditFailure failure = new("TargetApplicationFailed", exception.Message, exception);
					targetResults.Add(target.CreateResult(
						unknownTargetIds.Contains(target.TargetId, StringComparer.OrdinalIgnoreCase)
							? TextWorkspaceEditTargetStatus.Unknown
							: TextWorkspaceEditTargetStatus.Changed,
						TryGetVersion(target.EditTarget),
						failure));
					AddNotAppliedTargetResults(preparedEdit.Targets, targetResults, targetIndex + 1);
					return CreatePartialResult(
						preparedEdit,
						documentChanges,
						targetResults,
						changedTargetIds,
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
	public IReadOnlyList<string> ApplyBeforeSnapshot(TextWorkspaceEditTransaction transaction)
		=> ApplyContentSnapshots(transaction, static documentChange => documentChange.BeforeContent);

	/// <summary>
	/// Applies the after snapshots from a transaction.
	/// </summary>
	/// <param name="transaction">The transaction whose after snapshots should be restored.</param>
	/// <returns>The file paths whose contents changed.</returns>
	public IReadOnlyList<string> ApplyAfterSnapshot(TextWorkspaceEditTransaction transaction)
		=> ApplyContentSnapshots(transaction, static documentChange => documentChange.AfterContent);

	private IReadOnlyList<string> ApplyContentSnapshots(TextWorkspaceEditTransaction transaction, Func<TextWorkspaceDocumentChange, string> selectContent)
	{
		ArgumentNullException.ThrowIfNull(transaction);
		ArgumentNullException.ThrowIfNull(selectContent);

		if (!transaction.HasChanges)
			return [];

		return _textEditorHost.ExecutePreservingSelection(() =>
		{
			var updatedFiles = new List<string>(transaction.DocumentChanges.Count);

			foreach (TextWorkspaceDocumentChange documentChange in transaction.DocumentChanges)
			{
				TextEditorBase textEditor = _textEditorHost.OpenTextEditor(documentChange.FilePath);
				ApplyDocumentContent(textEditor, selectContent(documentChange));
				if (textEditor.WorkspaceEditTarget is null)
					SynchronizeOpenEditors(documentChange.FilePath, textEditor);
				updatedFiles.Add(documentChange.FilePath);
			}

			return (IReadOnlyList<string>)updatedFiles;
		});
	}

	private static RestoredSelectionState MapSelectionState(TextWorkspaceEditSelectionState selectionState, IReadOnlyList<TextEditOperation> preparedTextEdits)
	{
		int selectionStart = TextEditKernel.MapOffset(selectionState.SelectionStart, preparedTextEdits);
		int selectionEnd = TextEditKernel.MapOffset(selectionState.SelectionEnd, preparedTextEdits);
		int caretOffset = TextEditKernel.MapOffset(selectionState.CaretOffset, preparedTextEdits);

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

			editTarget.Apply([new TextEditOperation(0, editTarget.Text.Length, content, 0)]);
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
		var targetResults = new List<TextWorkspaceEditTargetResult>();
		var diagnostics = new List<TextEditPreparationDiagnostic>();
		TextWorkspaceEditFailure? failure = null;

		IEnumerable<IGrouping<string, TextDocumentEdit>> fileGroups = workspaceEdit.DocumentEdits
			.GroupBy(documentEdit => documentEdit.FilePath, StringComparer.OrdinalIgnoreCase)
			.OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

		foreach (IGrouping<string, TextDocumentEdit> fileGroup in fileGroups)
		{
			string targetId = fileGroup.Key;
			if (string.IsNullOrWhiteSpace(targetId))
			{
				TextWorkspaceEditFailure targetFailure = new(
					"InvalidTarget",
					"A workspace edit target must have a file path.");
				failure ??= targetFailure;
				targetResults.Add(new TextWorkspaceEditTargetResult(
					targetId,
					0,
					null,
					0,
					TextWorkspaceEditTargetStatus.NotApplied,
					targetFailure));
				continue;
			}

			try
			{
				TextEditorBase editor = _textEditorHost.OpenTextEditor(targetId);
				ITextEditTarget editTarget = _textEditorHost.GetTextEditTarget(editor);
				if (editTarget is not ITextEditTargetVersion)
				{
					TextWorkspaceEditFailure targetFailure = new(
						"UnsupportedTargetCapability",
						$"The target '{targetId}' does not expose a document version.");
					failure ??= targetFailure;
					targetResults.Add(new TextWorkspaceEditTargetResult(
						targetId,
						0,
						null,
						0,
						TextWorkspaceEditTargetStatus.NotApplied,
						targetFailure));
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
					targetResults.Add(new TextWorkspaceEditTargetResult(
						targetId,
						expectedVersion,
						expectedVersion,
						preparation.Operations.Count,
						TextWorkspaceEditTargetStatus.NotApplied));
					continue;
				}

				TextWorkspaceEditSelectionState? selectionStateForTarget = selectionState is not null
					&& string.Equals(selectionState.FilePath, targetId, StringComparison.OrdinalIgnoreCase)
						? selectionState
						: null;
				RestoredSelectionState? restoredSelectionState = selectionStateForTarget is null
					? null
					: MapSelectionState(selectionStateForTarget, preparation.Operations);

				var target = new PreparedTarget(
					targetId,
					editor,
					editTarget,
					beforeContent,
					ApplyOperations(beforeContent, preparation.Operations),
					preparation.Operations,
					expectedVersion,
					selectionStateForTarget,
					restoredSelectionState);
				targets.Add(target);
				targetResults.Add(target.CreateResult(TextWorkspaceEditTargetStatus.NotApplied, expectedVersion));
			}
			catch (Exception exception)
			{
				TextWorkspaceEditFailure targetFailure = new("TargetResolutionFailed", exception.Message, exception);
				failure ??= targetFailure;
				targetResults.Add(new TextWorkspaceEditTargetResult(
					targetId,
					0,
					null,
					0,
					TextWorkspaceEditTargetStatus.NotApplied,
					targetFailure));
			}
		}

		int preparedOperationCount = targets.Sum(target => target.Operations.Count);
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

	private static bool TryConfirmPreflightState(PreparedTarget target, out TextWorkspaceEditFailure? failure)
	{
		failure = null;
		try
		{
			if (!string.Equals(target.EditTarget.Text, target.BeforeContent, StringComparison.Ordinal)
				|| TryGetVersion(target.EditTarget) != target.ExpectedVersion)
			{
				failure = new TextWorkspaceEditFailure(
					"TargetChangedDuringPreflight",
					$"The target '{target.TargetId}' changed after preflight.");
				return false;
			}

			return true;
		}
		catch (Exception exception)
		{
			failure = new TextWorkspaceEditFailure("TargetUnavailable", exception.Message, exception);
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
		ICollection<TextWorkspaceEditTargetResult> targetResults,
		int startIndex)
	{
		for (int index = startIndex; index < targets.Count; index++)
			targetResults.Add(targets[index].CreateResult(TextWorkspaceEditTargetStatus.NotApplied, null));
	}

	private static TextWorkspaceEditApplicationResult CreateCompletedResult(
		IReadOnlyList<TextWorkspaceDocumentChange> documentChanges,
		int preparedOperationCount,
		IReadOnlyList<TextWorkspaceEditTargetResult> targetResults)
		=> new(
			TextWorkspaceEditApplicationStatus.Completed,
			preparedOperationCount,
			targetResults,
			documentChanges.Select(change => change.FilePath).ToArray(),
			[],
			[],
			null,
			new TextWorkspaceEditTransaction(documentChanges));

	private static TextWorkspaceEditApplicationResult CreatePartialResult(
		PreparedWorkspaceEdit preparedEdit,
		IReadOnlyList<TextWorkspaceDocumentChange> documentChanges,
		IReadOnlyList<TextWorkspaceEditTargetResult> targetResults,
		IReadOnlyList<string> changedTargetIds,
		IReadOnlyList<string> unknownTargetIds,
		TextWorkspaceEditFailure failure)
		=> new(
			TextWorkspaceEditApplicationStatus.PartiallyApplied,
			preparedEdit.PreparedOperationCount,
			targetResults,
			changedTargetIds,
			unknownTargetIds,
			preparedEdit.Diagnostics,
			failure,
			new TextWorkspaceEditTransaction(documentChanges));

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
		IReadOnlyList<TextWorkspaceEditTargetResult> targetResults,
		int preparedOperationCount,
		IReadOnlyList<TextEditPreparationDiagnostic> diagnostics,
		TextWorkspaceEditFailure? failure)
	{
		public IReadOnlyList<PreparedTarget> Targets { get; } = targets;
		public IReadOnlyList<TextWorkspaceEditTargetResult> TargetResults { get; } = targetResults;
		public int PreparedOperationCount { get; } = preparedOperationCount;
		public IReadOnlyList<TextEditPreparationDiagnostic> Diagnostics { get; } = diagnostics;
		public TextWorkspaceEditFailure? Failure { get; } = failure;
		public bool IsValid => Failure is null && Diagnostics.Count == 0;
	}

	private sealed class PreparedTarget(
		string targetId,
		TextEditorBase editor,
		ITextEditTarget editTarget,
		string beforeContent,
		string expectedAfterContent,
		IReadOnlyList<TextEditOperation> operations,
		long expectedVersion,
		TextWorkspaceEditSelectionState? selectionState,
		RestoredSelectionState? restoredSelectionState)
	{
		public string TargetId { get; } = targetId;
		public TextEditorBase Editor { get; } = editor;
		public ITextEditTarget EditTarget { get; } = editTarget;
		public string BeforeContent { get; } = beforeContent;
		public string ExpectedAfterContent { get; } = expectedAfterContent;
		public IReadOnlyList<TextEditOperation> Operations { get; } = operations;
		public long ExpectedVersion { get; } = expectedVersion;
		public TextWorkspaceEditSelectionState? SelectionState { get; } = selectionState;
		public RestoredSelectionState? RestoredSelectionState { get; } = restoredSelectionState;

		public TextWorkspaceEditTargetResult CreateResult(
			TextWorkspaceEditTargetStatus status,
			long? actualVersion,
			TextWorkspaceEditFailure? failure = null)
			=> new(TargetId, ExpectedVersion, actualVersion, Operations.Count, status, failure);
	}
}
