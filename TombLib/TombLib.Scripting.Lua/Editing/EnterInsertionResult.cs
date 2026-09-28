namespace TombLib.Scripting.Lua.Editing;

/// <summary>
/// Describes the text inserted when Enter is pressed, and the resulting caret position.
/// </summary>
/// <param name="Text">The complete text to insert.</param>
/// <param name="CaretOffset">The zero-based caret offset within the inserted text.</param>
/// <param name="RemoveFollowingWhitespaceLength">
/// The number of whitespace characters after the caret that the editor should remove.
/// </param>
public readonly record struct EnterInsertionResult(string Text, int CaretOffset, int RemoveFollowingWhitespaceLength);
