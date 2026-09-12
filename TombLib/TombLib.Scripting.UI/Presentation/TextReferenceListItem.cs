using Nickelony.LanguageServer.Abstractions;

namespace TombLib.Scripting.UI.Presentation;

/// <summary>
/// Describes a single reference to a symbol within a file.
/// </summary>
/// <param name="FilePath">The full path of the file that contains the reference.</param>
/// <param name="Range">The range of the reference in the document.</param>
/// <param name="LineNumber">The one-based line number of the reference.</param>
/// <param name="ColumnNumber">The one-based column number of the reference.</param>
/// <param name="PreviewText">The preview text around the reference.</param>
public sealed record class TextReferenceListItem(
	string FilePath,
	TextDocumentRange Range,
	int LineNumber,
	int ColumnNumber,
	string PreviewText)
{
	/// <summary>
	/// Gets the display text that combines the location with the preview text.
	/// </summary>
	public string DisplayText => $"{LineNumber}:{ColumnNumber}  {PreviewText}";
}
