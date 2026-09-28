using System.Collections.Generic;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.ClassicScript.Types;

namespace TombLib.Scripting.ClassicScript.Services;

/// <summary>
/// Provides command-level and document-level operations for ClassicScript source text.
/// Methods that locate document lines return <c>int?</c> one-based line numbers.
/// No method exposes AvalonEdit types.
/// </summary>
public interface IClassicScriptCommandService
{
	// ------------------------------------------------------------------
	// Command location and text
	// ------------------------------------------------------------------

	/// <summary>
	/// Gets the one-based line number where the command that contains the specified offset begins.
	/// Returns <see langword="null"/> if the offset is on a section header or cannot be resolved.
	/// </summary>
	int? GetCommandStartLine(ITextSnapshot source, int offset);

	/// <summary>
	/// Gets the full logical text of the command that contains the specified offset,
	/// including merged continuation lines separated by newlines.
	/// Returns <see langword="null"/> if the command cannot be resolved.
	/// </summary>
	string? GetWholeCommandLineText(ITextSnapshot source, int offset);

	/// <summary>
	/// Gets the command key for the command that contains the specified offset.
	/// Section-variant commands (Level, Cut, FMV) are resolved to their section-appropriate keys
	/// (e.g. LevelLevel, LevelPC, CutPC, FMVLevel).
	/// Returns <see langword="null"/> for section headers, comment lines, or unrecognized text.
	/// </summary>
	string? GetCommandKey(ITextSnapshot source, int offset);

	/// <summary>
	/// Gets the command syntax string for the command that contains the specified offset.
	/// Returns <see langword="null"/> if no syntax is available.
	/// </summary>
	string? GetCommandSyntax(ITextSnapshot source, int offset);

	/// <summary>
	/// Gets all registered command syntax definitions from the syntax catalog.
	/// Retained while tests and public interfaces exercise it; replace only with an
	/// intentional contract change.
	/// </summary>
	IReadOnlyList<ClassicScriptSyntaxDefinition> GetCommandSyntaxDefinitions();

	// ------------------------------------------------------------------
	// Argument navigation
	// ------------------------------------------------------------------

	/// <summary>
	/// Gets the zero-based argument index at the specified offset within a command.
	/// Returns -1 if the offset is not within a command with arguments.
	/// </summary>
	int GetArgumentIndexAtOffset(ITextSnapshot source, int offset);

	/// <summary>
	/// Gets the argument text at the specified zero-based index within the command
	/// that contains the given offset. The argument text is extracted from the
	/// merged whole-command text, split by commas.
	/// Returns <see langword="null"/> if the command text is unavailable.
	/// </summary>
	string? GetArgumentFromIndex(ITextSnapshot source, int offset, int index);

	/// <summary>
	/// Gets the flag prefix (e.g. "CUST_") for the argument at the specified offset,
	/// derived from the command syntax definition.
	/// Returns <see langword="null"/> if no flag prefix applies.
	/// </summary>
	string? GetFlagPrefixOfCurrentArgument(ITextSnapshot source, int offset);

	// ------------------------------------------------------------------
	// Include path resolution
	// ------------------------------------------------------------------

	/// <summary>
	/// Resolves the full file system path of an include directive at the specified offset.
	/// Returns <see langword="null"/> if the line is not a valid include directive
	/// or the source has no file name.
	/// </summary>
	string? GetFullIncludePath(ITextSnapshot source, int offset);

	// ------------------------------------------------------------------
	// Section queries
	// ------------------------------------------------------------------

	/// <summary>
	/// Determines whether the source text contains any section headers.
	/// </summary>
	bool DocumentContainsSections(ITextSnapshot source);

	/// <summary>
	/// Gets the number of section headers in the source text.
	/// </summary>
	int GetSectionsCount(ITextSnapshot source);

	/// <summary>
	/// Gets the name of the section that contains the specified offset.
	/// Returns <see langword="null"/> if the offset is before the first section header.
	/// </summary>
	string? GetCurrentSectionName(ITextSnapshot source, int offset);

	/// <summary>
	/// Gets the one-based line number of the section header that contains the specified offset.
	/// Returns <see langword="null"/> if the offset is before the first section header.
	/// </summary>
	int? GetStartLineOfCurrentSection(ITextSnapshot source, int offset);

	/// <summary>
	/// Gets the one-based line number of the last non-empty line in the section
	/// that contains the specified offset.
	/// Returns <see langword="null"/> if the section cannot be determined.
	/// </summary>
	int? GetLastLineOfCurrentSection(ITextSnapshot source, int offset);

	/// <summary>
	/// Finds the one-based line number of the section header with the specified name.
	/// The comparison is case-insensitive. Brackets in the name are stripped.
	/// Returns <see langword="null"/> if no matching section is found.
	/// </summary>
	int? FindDocumentLineOfSection(ITextSnapshot source, string sectionName);

	// ------------------------------------------------------------------
	// Object lookup
	// ------------------------------------------------------------------

	/// <summary>
	/// Finds the one-based line number of a named object of the specified type.
	/// Returns <see langword="null"/> if no matching object is found.
	/// </summary>
	int? FindDocumentLineOfObject(ITextSnapshot source, string objectName, ObjectType type);

	/// <summary>
	/// Determines whether a level script with the specified name is defined
	/// in the source text.
	/// </summary>
	bool IsLevelScriptDefined(ITextSnapshot source, string levelName);

	/// <summary>
	/// Determines whether a level language string with the specified name is defined
	/// in the source text.
	/// </summary>
	bool IsLevelLanguageStringDefined(ITextSnapshot source, string levelName);

	/// <summary>
	/// Determines whether a plugin with the specified name is defined
	/// in the Options section of the source text.
	/// </summary>
	bool IsPluginDefined(ITextSnapshot source, string pluginName);
}
