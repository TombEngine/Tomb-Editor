using Nickelony.LanguageServer.Abstractions.Diagnostics;
using System;
using System.Collections.Generic;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Text;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Diagnostics;

/// <summary>
/// Detects errors in TRX documents, such as removed keywords for the target engine version.
/// </summary>
public sealed class ErrorDetector : ITextDiagnosticsProvider
{
	private readonly ITRXLineService _lineService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ErrorDetector"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to analyze document lines.</param>
	public ErrorDetector(ITRXLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_lineService = lineService;
	}

	/// <inheritdoc/>
	public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(TextDiagnosticsRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		return FindErrors(request.DocumentText, request.EngineVersion);
	}

	private IReadOnlyList<TextEditorDiagnostic> FindErrors(string editorContent, Version engineVersion)
	{
		// Anything before 4.8 should not have errors checked
		if (engineVersion < new Version(4, 8))
			return [];

		return DetectErrorLines(new StringTextSnapshot(editorContent), engineVersion);
	}

	private List<TextEditorDiagnostic> DetectErrorLines(ITextSnapshot source, Version engineVersion)
	{
		var errorLines = new List<TextEditorDiagnostic>();

		foreach (ITextLine processedLine in source.Lines)
		{
			string processedLineText = source.GetText(processedLine.Offset, processedLine.Length);

			if (_lineService.IsEmptyOrComments(processedLineText))
				continue;

			processedLineText = _lineService.EscapeComments(processedLineText);
			TextEditorDiagnostic? error = FindErrorsInLine(processedLine, processedLineText, engineVersion);

			if (error is not null)
				errorLines.Add(error);
		}

		return errorLines;
	}

	private static TextEditorDiagnostic? FindErrorsInLine(ITextLine line, string lineText, Version engineVersion)
	{
		// Check whether there are JSON keys which are marked as "Removed"
		TextEditorDiagnostic? removedProperty = FindRemovedKeyword(line, lineText, Keywords.RemovedProperties, engineVersion, "property");

		if (removedProperty is not null)
			return removedProperty;

		return FindRemovedKeyword(line, lineText, Keywords.RemovedConstants, engineVersion, "constant");
	}

	private static TextEditorDiagnostic? FindRemovedKeyword(ITextLine line, string lineText, IReadOnlyList<RemovedKeyword> keywords, Version engineVersion, string kindLabel)
	{
		foreach (RemovedKeyword keyword in keywords)
		{
			if (engineVersion < keyword.RemovedVersion)
				continue;

			string keyPattern = $"\"{keyword.Keyword}\"";

			if (lineText.Contains(keyPattern))
			{
				return CreateDiagnostic(line, lineText,
					$"This {kindLabel} has been removed from the script syntax and cannot be used in TRX {keyword.RemovedVersion} or newer."
					+ (string.IsNullOrEmpty(keyword.Message) ? "" : "\n" + keyword.Message), keyPattern);
			}
		}

		return null;
	}

	private static TextEditorDiagnostic CreateDiagnostic(ITextLine line, string lineText, string message, string keyPattern)
	{
		int matchIndex = string.IsNullOrWhiteSpace(keyPattern)
			? -1
			: lineText.IndexOf(keyPattern, StringComparison.Ordinal);

		int startOffset = matchIndex >= 0 ? line.Offset + matchIndex : line.Offset;
		int endOffset = matchIndex >= 0
			? startOffset + keyPattern.Length
			: Math.Max(line.Offset + 1, line.EndOffset);

		return new TextEditorDiagnostic(TextEditorDiagnosticSeverity.Error, message, startOffset, endOffset);
	}
}
