using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.AvalonEdit.IntelliSense.Completion;
using Nickelony.IDEKit.Core.Identifiers;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Completion;
using System.Windows.Documents;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Scripting.GameFlowScript.Completion;

/// <summary>
/// Coordinates completion session decisions for the GameFlow editor.
/// </summary>
/// <param name="completionProvider">The completion provider used to source completion items.</param>
/// <param name="lineService">The line service used to analyze document lines.</param>
public sealed class GameFlowCompletionSessionCoordinator(GameFlowCompletionProvider completionProvider, IGameFlowScriptLineService lineService)
{
	private readonly GameFlowCompletionProvider _completionProvider = completionProvider;
	private readonly IGameFlowScriptLineService _lineService = lineService;
	private readonly CompletionSessionKernel _kernel = new();

	/// <summary>
	/// Gets the decision for whether a completion session should open at the caret.
	/// </summary>
	/// <param name="document">The current document.</param>
	/// <param name="caretOffset">The caret offset.</param>
	/// <param name="completionWindowIsOpen">Whether a completion window is already open.</param>
	/// <returns>The completion session decision.</returns>
	public TextCompletionSessionDecision GetOpenDecision(TextDocument document, int caretOffset, bool completionWindowIsOpen)
	{
		var source = new StringTextSnapshot(document.Text, document.FileName);

		if (completionWindowIsOpen || !ShouldShowCompletion(source, caretOffset))
			return TextCompletionSessionDecision.None;

		CompletionWordInfo? wordInfo = LocateWord(document, caretOffset);

		return wordInfo is null
			? TextCompletionSessionDecision.None
			: _kernel.GetDecision(source, caretOffset, _completionProvider, wordInfo: wordInfo.Value);
	}

	private static CompletionWordInfo? LocateWord(TextDocument document, int caretOffset)
	{
		// TextUtilities.GetNextCaretPosition is AvalonEdit-specific and must use TextDocument.
		int wordStartOffset = TextUtilities.GetNextCaretPosition(document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol);

		if (wordStartOffset < 0)
			return null;

		string word = document.GetText(wordStartOffset, caretOffset - wordStartOffset);
		int startOffset = word.StartsWith(':') ? caretOffset : wordStartOffset;

		// The kernel filters by the identifier prefix before the caret, matching the legacy
		// FilterByCurrentWord behavior, while the replacement range keeps WordStartOrSymbol
		// semantics plus the ':' prefix quirk.
		string filterWord = IdentifierHelper.GetPrefix(document.Text, caretOffset);

		return new CompletionWordInfo(filterWord, new Nickelony.IDEKit.Core.Text.TextRange(startOffset, caretOffset - startOffset));
	}

	private bool ShouldShowCompletion(ITextSnapshot source, int caretOffset)
	{
		if (source.TextLength == 0 || caretOffset < 0 || caretOffset > source.TextLength)
			return false;

		ITextLine line = source.GetLineByOffset(caretOffset);
		string currentLineText = _lineService.EscapeComments(source.GetText(line.Offset, line.Length)).Trim();
		return EditorCompletionTriggerHelper.IsSingleCharacterLine(currentLineText);
	}
}
