namespace TombLib.Scripting.Lua.Editing;

/// <summary>
/// Describes a completion insertion after multiline indentation normalization.
/// </summary>
/// <param name="Text">The normalized insertion text.</param>
/// <param name="CaretOffset">The normalized zero-based caret offset within the inserted text, if known.</param>
public readonly record struct CompletionInsertionResult(string Text, int? CaretOffset);
