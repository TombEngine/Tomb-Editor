#nullable enable

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System.Collections.Generic;

namespace TombLib.Scripting.Tomb1Main.Services;

/// <summary>
/// Manages autocomplete functionality for the editor.
/// </summary>
public interface IAutocompleteManager
{
	/// <summary>
	/// Filters completion data based on the current word being typed.
	/// </summary>
	List<ICompletionData> FilterCompletions(List<ICompletionData> autocompleteData, string currentWord);

	/// <summary>
	/// Determines if autocomplete should be triggered for an empty line context.
	/// </summary>
	bool ShouldTriggerAutocompleteOnEmptyLine(TextDocument document, int caretOffset);

	/// <summary>
	/// Calculates the start and end offsets for completion window.
	/// </summary>
	(int startOffset, int endOffset) GetCompletionWindowOffsets(TextDocument document, int caretOffset, string currentWord);
}
