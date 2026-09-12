using Nickelony.IDEKit.IntelliSense.Diagnostics;

namespace TombLib.Scripting.UI.Presentation;

/// <summary>
/// Describes a single diagnostic for presentation in a diagnostics list.
/// </summary>
/// <param name="FilePath">The path of the file that contains the diagnostic.</param>
/// <param name="Severity">The severity of the diagnostic.</param>
/// <param name="SeverityLabel">The localized label of the diagnostic severity.</param>
/// <param name="LineNumber">The one-based line number of the diagnostic.</param>
/// <param name="ColumnNumber">The one-based column number of the diagnostic.</param>
/// <param name="Message">The diagnostic message.</param>
/// <param name="StartOffset">The start offset of the diagnostic in the document.</param>
/// <param name="EndOffset">The end offset of the diagnostic in the document.</param>
public sealed record class TextDiagnosticListItem(
	string FilePath,
	TextEditorDiagnosticSeverity Severity,
	string SeverityLabel,
	int LineNumber,
	int ColumnNumber,
	string Message,
	int StartOffset,
	int EndOffset);
