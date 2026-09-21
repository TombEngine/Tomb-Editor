using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.IntelliSense.Completion;

namespace TombLib.Scripting.TRX.Completion;

/// <summary>
/// Coordinates completion session decisions for the TRX editor.
/// </summary>
public sealed class TRXCompletionSessionCoordinator
{
	private readonly ITextCompletionProvider _completionProvider;
	private readonly TextAnalysisService _textAnalysisService;
	private readonly CompletionManager _completionManager;
	private readonly TextCompletionSessionKernel _kernel;

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXCompletionSessionCoordinator"/> class.
	/// </summary>
	/// <param name="completionProvider">The completion provider used to source completion items.</param>
	/// <param name="textAnalysisService">The text analysis service used to validate completion contexts.</param>
	/// <param name="completionManager">The completion manager used to filter completion items.</param>
	public TRXCompletionSessionCoordinator(
		ITextCompletionProvider completionProvider,
		TextAnalysisService textAnalysisService,
		CompletionManager completionManager)
	{
		_completionProvider = completionProvider;
		_textAnalysisService = textAnalysisService;
		_completionManager = completionManager;
		_kernel = new TextCompletionSessionKernel(filter: completionManager.FilterCompletions);
	}

	/// <summary>
	/// Computes the completion session decision for a Ctrl+Space invocation.
	/// </summary>
	/// <param name="document">The current document.</param>
	/// <param name="caretOffset">The current caret offset.</param>
	/// <param name="completionWindowIsOpen">Whether a completion window is already open.</param>
	/// <returns>The completion session decision to apply.</returns>
	public TextCompletionSessionDecision GetCtrlSpaceDecision(TextDocument document, int caretOffset, bool completionWindowIsOpen)
	{
		if (completionWindowIsOpen || !_textAnalysisService.IsValidPositionForCtrlSpaceCompletion(document, caretOffset))
			return TextCompletionSessionDecision.None;

		return CreateOpenDecision(document, caretOffset);
	}

	/// <summary>
	/// Computes the completion session decision for text entered into the editor.
	/// </summary>
	/// <param name="document">The current document.</param>
	/// <param name="caretOffset">The current caret offset.</param>
	/// <param name="inputText">The text that was entered.</param>
	/// <param name="completionWindowIsOpen">Whether a completion window is already open.</param>
	/// <returns>The completion session decision to apply.</returns>
	public TextCompletionSessionDecision GetTextEnteredDecision(TextDocument document, int caretOffset, string inputText, bool completionWindowIsOpen)
	{
		if (completionWindowIsOpen)
			return HasMatchingCompletions(document, caretOffset)
				? TextCompletionSessionDecision.None
				: TextCompletionSessionDecision.Close;

		if (ShouldTriggerCompletion(inputText))
		{
			int contextOffset = caretOffset > 0 ? caretOffset - 1 : 0;

			if (!_textAnalysisService.IsValidContextForCompletion(document, contextOffset))
				return TextCompletionSessionDecision.None;

			return CreateOpenDecision(document, caretOffset);
		}

		var source = new StringTextSnapshot(document.Text, document.FileName);

		if (_completionManager.ShouldTriggerCompletionOnEmptyLine(source, caretOffset))
			return CreateOpenDecision(document, caretOffset);

		return TextCompletionSessionDecision.None;
	}

	private static bool ShouldTriggerCompletion(string inputText)
		=> inputText == "\"";

	private bool HasMatchingCompletions(TextDocument document, int caretOffset)
		=> CreateOpenDecision(document, caretOffset) != TextCompletionSessionDecision.None;

	private TextCompletionSessionDecision CreateOpenDecision(TextDocument document, int caretOffset)
	{
		string currentWord = _textAnalysisService.GetCurrentWordBeingTyped(document, caretOffset);
		var source = new StringTextSnapshot(document.Text, document.FileName);
		(int startOffset, int endOffset) = _completionManager.GetCompletionWindowOffsets(source, caretOffset, currentWord);

		var wordInfo = new TextCompletionWordSpan(currentWord, new TextRange(startOffset, endOffset - startOffset));
		return _kernel.GetDecision(source, caretOffset, _completionProvider, wordSpan: wordInfo);
	}
}
