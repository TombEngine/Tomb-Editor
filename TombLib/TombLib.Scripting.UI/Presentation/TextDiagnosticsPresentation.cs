namespace TombLib.Scripting.UI.Presentation;

/// <summary>
/// Carries the localized text used by the diagnostics presentation.
/// </summary>
/// <param name="NoActiveDocumentText">The text shown when no document is active.</param>
/// <param name="PendingText">The text shown while diagnostics are pending.</param>
/// <param name="EmptyText">The text shown when the document has no diagnostics.</param>
/// <param name="ErrorsLabel">The label for errors.</param>
/// <param name="WarningsLabel">The label for warnings.</param>
/// <param name="MessagesLabel">The label for messages.</param>
/// <param name="SeverityHeader">The severity column header.</param>
/// <param name="LineHeader">The line column header.</param>
/// <param name="ColumnHeader">The column column header.</param>
/// <param name="SourceHeader">The source column header.</param>
/// <param name="MessageHeader">The message column header.</param>
public sealed record class TextDiagnosticsPresentation(
	string NoActiveDocumentText,
	string PendingText,
	string EmptyText,
	string ErrorsLabel,
	string WarningsLabel,
	string MessagesLabel,
	string SeverityHeader,
	string LineHeader,
	string ColumnHeader,
	string SourceHeader,
	string MessageHeader);
