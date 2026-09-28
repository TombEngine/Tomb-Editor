#nullable enable

using Nickelony.LanguageServer.Abstractions;
using Nickelony.IDEKit.AvalonEdit.Editing;
using Nickelony.IDEKit.Core.Editing;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.Workspace.Editing;
using System;
using System.Threading;
using System.Threading.Tasks;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombIDE.ScriptingStudio.TextEditing;

internal sealed class TextWorkspaceCommandService(TextWorkspaceEditApplier workspaceEditApplier, ILanguageServerRenameProvider? editProvider = null)
{
	private readonly ILanguageServerRenameProvider? _editProvider = editProvider;
	private readonly TextWorkspaceEditApplier _workspaceEditApplier = workspaceEditApplier ?? throw new ArgumentNullException(nameof(workspaceEditApplier));

	public bool SupportsRename => _editProvider?.SupportsRename == true;

	public async Task<TextWorkspaceCommandResult> FormatDocumentAsync(
		TextEditorBase editor,
		ILanguageServerFormattingProvider formattingProvider,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(formattingProvider);

		try
		{
			TextWorkspaceEdit? workspaceEdit = await formattingProvider
				.FormatDocumentAsync(CreateFormatRequest(editor), cancellationToken)
				.ConfigureAwait(true);

			if (workspaceEdit is null || !workspaceEdit.HasChanges)
				return TextWorkspaceCommandResult.NoChanges;

			TextWorkspaceEditSelectionState selectionState = TextWorkspaceEditSelectionState.Capture(editor, editor.FilePath);
			WorkspaceEditApplicationResult result = _workspaceEditApplier.Apply(workspaceEdit, selectionState);

			return result.Outcome switch
			{
				WorkspaceEditApplicationOutcome.Completed when result.ChangeSet.HasChanges => TextWorkspaceCommandResult.Applied(result),
				WorkspaceEditApplicationOutcome.Completed => TextWorkspaceCommandResult.NoChanges,
				WorkspaceEditApplicationOutcome.ValidationFailed => TextWorkspaceCommandResult.ValidationFailed(result),
				WorkspaceEditApplicationOutcome.PartiallyApplied => TextWorkspaceCommandResult.PartiallyApplied(result),
				_ => throw new InvalidOperationException($"Unknown workspace edit status: {result.Outcome}.")
			};
		}
		catch (OperationCanceledException)
		{
			return TextWorkspaceCommandResult.Cancelled;
		}
	}

	public async Task<TextWorkspaceCommandResult> RenameSymbolAsync(
		TextEditorBase editor,
		int line,
		int column,
		string newName,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(editor);

		if (_editProvider is null)
			return TextWorkspaceCommandResult.NoChanges;

		try
		{
			TextWorkspaceEdit? workspaceEdit = await _editProvider
				.RenameSymbolAsync(new TextRenameRequest(editor.FilePath, editor.Text, new TextPosition(line, column), newName), cancellationToken)
				.ConfigureAwait(true);

			if (workspaceEdit is null || !workspaceEdit.HasChanges)
				return TextWorkspaceCommandResult.NoChanges;

			WorkspaceEditApplicationResult result = _workspaceEditApplier.Apply(workspaceEdit);

			return result.Outcome switch
			{
				WorkspaceEditApplicationOutcome.Completed when result.ChangeSet.HasChanges => TextWorkspaceCommandResult.Applied(result),
				WorkspaceEditApplicationOutcome.Completed => TextWorkspaceCommandResult.NoChanges,
				WorkspaceEditApplicationOutcome.ValidationFailed => TextWorkspaceCommandResult.ValidationFailed(result),
				WorkspaceEditApplicationOutcome.PartiallyApplied => TextWorkspaceCommandResult.PartiallyApplied(result),
				_ => throw new InvalidOperationException($"Unknown workspace edit status: {result.Outcome}.")
			};
		}
		catch (OperationCanceledException)
		{
			return TextWorkspaceCommandResult.Cancelled;
		}
	}

	private static TextFormatRequest CreateFormatRequest(TextEditorBase editor)
	{
		int tabSize = editor.Options.IndentationSize > 0
			? editor.Options.IndentationSize
			: 4;

		return new TextFormatRequest(
			editor.FilePath,
			editor.Text,
			new TextFormattingOptions(tabSize, editor.Options.ConvertTabsToSpaces));
	}
}
