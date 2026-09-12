using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.ClassicScript.Types;

namespace TombLib.Scripting.ClassicScript.Services;

/// <summary>
/// Provides line-level text operations for ClassicScript source text.
/// Methods accept either a raw string or an <see cref="ITextSnapshot"/> depending on purpose.
/// No method exposes AvalonEdit types.
/// </summary>
public interface IClassicScriptLineService
{
	/// <summary>
	/// Gets the word at the specified offset within the source text.
	/// Returns <see langword="null"/> if the offset is past the end of the text
	/// or if no word can be extracted.
	/// </summary>
	string? GetWordAtOffset(ITextSnapshot source, int offset);

	/// <summary>
	/// Gets the <see cref="WordType"/> of the word at the specified offset.
	/// Returns <see cref="WordType.Unknown"/> if the offset is past the end of the text.
	/// </summary>
	WordType GetWordTypeAtOffset(ITextSnapshot source, int offset);

	/// <summary>
	/// Determines whether the given line text is a section header (e.g. "[Options]").
	/// </summary>
	bool IsSectionHeaderLine(string lineText);

	/// <summary>
	/// Extracts the section name from a section header line.
	/// For input "[Options] ; comment", returns "Options".
	/// Returns <see langword="null"/> if the line is not a valid section header.
	/// </summary>
	string? GetSectionHeaderText(string sectionHeaderLine);

	/// <summary>
	/// Determines whether the given line text is empty or consists only of a comment.
	/// Handles <see langword="null"/> input, returning <see langword="true"/>.
	/// </summary>
	bool IsEmptyOrComments(string? lineText);

	/// <summary>
	/// Determines whether the given line text is a valid include directive
	/// (e.g. <c>#include "path/to/file.txt"</c>).
	/// </summary>
	bool IsValidIncludeLine(string lineText);

	/// <summary>
	/// Determines whether the supplied section name is a standard string section
	/// (Strings, PCStrings, or PSXStrings). The comparison is case-insensitive.
	/// </summary>
	bool IsStandardStringSectionName(string? sectionName);

	/// <summary>
	/// Determines whether the supplied section name is the ExtraNG section.
	/// The comparison is case-insensitive.
	/// </summary>
	bool IsExtraNGSectionName(string? sectionName);

	/// <summary>
	/// Determines whether the supplied section name is any string section
	/// (Strings, PCStrings, PSXStrings or ExtraNG). The comparison is case-insensitive.
	/// </summary>
	bool IsStringSectionName(string? sectionName);

	/// <summary>
	/// Removes line comments from the text, including preceding whitespace.
	/// </summary>
	string RemoveComments(string lineText);

	/// <summary>
	/// Replaces comments with spaces to preserve the original string length.
	/// Used by algorithms that depend on character positions.
	/// </summary>
	string EscapeComments(string lineText);

	/// <summary>
	/// Escapes comments and replaces continuation markers ('&gt;') and newlines with spaces.
	/// Used by expression evaluation to flatten multi-line text.
	/// </summary>
	string EscapeCommentsAndNewLines(string lineText);

	/// <summary>
	/// Removes the NG string index prefix from a line (e.g. "42: some text" becomes "some text").
	/// </summary>
	string RemoveNGStringIndex(string lineText);
}
