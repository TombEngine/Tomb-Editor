#nullable enable

using ICSharpCode.AvalonEdit.Document;

namespace TombLib.Scripting.Tomb1Main.Services;

/// <summary>
/// Provides text analysis capabilities for editor functionality.
/// </summary>
public interface ITextAnalysisService
{
	/// <summary>
	/// Determines if the caret position is valid for Ctrl+Space autocomplete.
	/// </summary>
	bool IsValidPositionForCtrlSpaceAutocomplete(TextDocument document, int caretOffset);

	/// <summary>
	/// Determines if the context is valid for autocomplete.
	/// </summary>
	bool IsValidContextForAutocomplete(TextDocument document, int caretOffset);

	/// <summary>
	/// Gets the current word being typed at the specified position.
	/// </summary>
	string GetCurrentWordBeingTyped(TextDocument document, int caretOffset);
}
