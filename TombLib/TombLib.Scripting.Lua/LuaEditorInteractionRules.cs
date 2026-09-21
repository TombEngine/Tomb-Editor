using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.AvalonEdit.Documents;
using Nickelony.IDEKit.Core.Identifiers;
using Nickelony.IDEKit.Core.Text;
using System;
using System.Runtime.CompilerServices;
using TombLib.Scripting.Lua.Parsing;

namespace TombLib.Scripting.Lua;

/// <summary>
/// Encapsulates Lua-editor interaction rules for hover, completion, and definition navigation.
/// </summary>
internal static class LuaEditorInteractionRules
{
	private static readonly ConditionalWeakTable<TextDocument, DocumentLineStateCache<LuaLineParserState>> LineStartStateCaches = [];

	/// <summary>
	/// Determines whether hover content may be shown while other transient popups are active.
	/// </summary>
	/// <param name="isCompletionWindowOpen">Whether a completion window is currently open.</param>
	/// <param name="isSignatureHelpOpen">Whether signature help is currently visible.</param>
	/// <returns><see langword="true"/> if hover may be shown; otherwise, <see langword="false"/>.</returns>
	public static bool CanShowHover(bool isCompletionWindowOpen, bool isSignatureHelpOpen)
		=> !isCompletionWindowOpen && !isSignatureHelpOpen;

	/// <summary>
	/// Attempts to resolve the exact offset that should be used for a hover request.
	/// </summary>
	/// <param name="document">The document being inspected.</param>
	/// <param name="offset">The zero-based character offset under the mouse.</param>
	/// <param name="hoverOffset">When this method returns, contains the resolved hover offset.</param>
	/// <returns><see langword="true"/> if a hoverable identifier exists at the requested offset; otherwise, <see langword="false"/>.</returns>
	public static bool TryGetHoverOffset(TextDocument? document, int offset, out int hoverOffset)
	{
		hoverOffset = 0;

		if (document is null || document.TextLength == 0)
			return false;

		int safeOffset = ClampOffset(document, offset);

		if (safeOffset >= document.TextLength)
			return false;

		if (IsInsideCommentOrString(document, safeOffset))
			return false;

		if (!LuaLineParser.IsIdentifierCharacter(document.GetCharAt(safeOffset)))
			return false;

		hoverOffset = safeOffset;
		return true;
	}

	/// <summary>
	/// Determines whether the current caret context allows an automatic completion request.
	/// </summary>
	/// <param name="document">The document being inspected.</param>
	/// <param name="offset">The zero-based caret offset after text entry.</param>
	/// <param name="triggerCharacter">The character that triggered completion, if any.</param>
	/// <returns><see langword="true"/> if completion should be requested; otherwise, <see langword="false"/>.</returns>
	public static bool IsValidCompletionContext(TextDocument? document, int offset, char? triggerCharacter)
	{
		if (offset <= 0 || document is null || document.TextLength == 0)
			return false;

		if (IsInsideCommentOrString(document, offset))
			return false;

		if (triggerCharacter is '.' || triggerCharacter is ':')
			return true;

		char typedCharacter = document.GetCharAt(offset - 1);

		if (!LuaLineParser.IsIdentifierCharacter(typedCharacter))
			return false;

		if (offset >= 2 && document.GetCharAt(offset - 2) == '.')
			return false;

		return true;
	}

	/// <summary>
	/// Determines whether the current caret context allows a manual completion request.
	/// </summary>
	/// <param name="document">The document being inspected.</param>
	/// <param name="offset">The zero-based caret offset.</param>
	/// <returns><see langword="true"/> if manual completion may be requested; otherwise, <see langword="false"/>.</returns>
	public static bool IsValidManualCompletionContext(TextDocument? document, int offset)
	{
		if (document is null)
			return false;

		if (document.TextLength == 0)
			return true;

		return !IsInsideCommentOrString(document, offset);
	}

	/// <summary>
	/// Attempts to resolve the identifier start offset that should be used for a go-to-definition request.
	/// </summary>
	/// <param name="document">The document being inspected.</param>
	/// <param name="offset">The zero-based offset near the identifier.</param>
	/// <param name="definitionOffset">When this method returns, contains the identifier start offset.</param>
	/// <returns><see langword="true"/> if a definition target offset was found; otherwise, <see langword="false"/>.</returns>
	public static bool TryGetDefinitionStartOffset(TextDocument? document, int offset, out int definitionOffset)
	{
		definitionOffset = 0;

		if (document is null || document.TextLength == 0)
			return false;

		int safeOffset = ClampOffset(document, offset);

		if (IsInsideCommentOrString(document, safeOffset))
			return false;

		if (!TryGetDefinitionWordBounds(document, safeOffset, out definitionOffset, out _))
			return false;

		return true;
	}

	/// <summary>
	/// Determines whether the specified offset is inside a comment or string using document-aware long-block state.
	/// </summary>
	/// <param name="document">The document being inspected.</param>
	/// <param name="offset">The zero-based character offset.</param>
	/// <returns><see langword="true"/> if the offset is inside a comment or string on the current line; otherwise, <see langword="false"/>.</returns>
	public static bool IsInsideCommentOrString(TextDocument? document, int offset)
	{
		if (document is null || document.TextLength == 0)
			return false;

		int safeOffset = ClampOffset(document, offset);
		DocumentLine currentLine = document.GetLineByOffset(safeOffset);
		LuaLineParserState lineStartState = GetLineStartParserState(document, currentLine);
		int lineStart = currentLine.Offset;
		int inspectedLength = Math.Max(0, Math.Min(safeOffset, currentLine.EndOffset) - lineStart);
		string lineText = document.GetText(lineStart, inspectedLength);

		return LuaLineParser.IsInsideCommentOrString(lineText, lineStartState, out _);
	}

	private static LuaLineParserState GetLineStartParserState(TextDocument document, DocumentLine currentLine)
		=> LineStartStateCaches.GetValue(document, static doc => new DocumentLineStateCache<LuaLineParserState>(doc, TransitionLineState)).GetLineStartState(currentLine.LineNumber);

	private static LuaLineParserState TransitionLineState(string lineText, LuaLineParserState state)
	{
		LuaLineParser.IsInsideCommentOrString(lineText, state, out LuaLineParserState nextState);
		return nextState;
	}

	private static int ClampOffset(TextDocument document, int offset)
		=> Math.Clamp(offset, 0, document.TextLength);

	private static bool TryGetDefinitionWordBounds(TextDocument document, int offset, out int wordStart, out int wordEnd)
	{
		wordStart = 0;
		wordEnd = 0;

		if (document.TextLength == 0)
			return false;

		var snapshot = new TextDocumentSnapshot(document);
		TextRange? range = IdentifierOperations.FindTokenSpan(snapshot, offset, IdentifierCharacterPolicy.Default);

		if (range is null)
			return false;

		wordStart = range.Value.Offset;
		wordEnd = range.Value.EndOffset;
		return true;
	}
}
