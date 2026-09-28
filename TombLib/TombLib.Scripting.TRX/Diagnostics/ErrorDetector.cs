using Nickelony.IDEKit.Core.Text;
using Nickelony.IDEKit.Core.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;
using System.Collections.Generic;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Diagnostics;

/// <summary>
/// Detects errors in TRX documents, such as removed keywords for the target engine version.
/// </summary>
public sealed class ErrorDetector : ITextDiagnosticsProvider
{
	private readonly ITRXLineService _lineService;
	private readonly Version _engineVersion;

	/// <summary>
	/// Initializes a new instance of the <see cref="ErrorDetector"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to analyze document lines.</param>
	/// <param name="engineVersion">The engine version whose removed-keyword rules apply.</param>
	public ErrorDetector(ITRXLineService lineService, Version engineVersion)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		ArgumentNullException.ThrowIfNull(engineVersion);

		_lineService = lineService;
		_engineVersion = engineVersion;
	}

	/// <inheritdoc/>
	public IReadOnlyList<TextDiagnostic> GetDiagnostics(TextDiagnosticsRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		return FindErrors(request.DocumentText);
	}

	private IReadOnlyList<TextDiagnostic> FindErrors(string editorContent)
	{
		// Before 4.8 no removed-keyword rules apply.
		if (_engineVersion < new Version(4, 8))
			return [];

		return DetectErrorLines(new StringTextSnapshot(editorContent));
	}

	private List<TextDiagnostic> DetectErrorLines(ITextSnapshot source)
	{
		var errorLines = new List<TextDiagnostic>();

		foreach (ITextLine processedLine in source.Lines)
		{
			string processedLineText = source.GetText(processedLine.Offset, processedLine.Length);

			if (_lineService.IsEmptyOrComments(processedLineText))
				continue;

			processedLineText = _lineService.EscapeComments(processedLineText);
			TextDiagnostic? error = FindErrorsInLine(processedLine, processedLineText);

			if (error is not null)
				errorLines.Add(error);
		}

		return errorLines;
	}

	private TextDiagnostic? FindErrorsInLine(ITextLine line, string lineText)
	{
		// Check whether there are JSON keys which are marked as "Removed"
		TextDiagnostic? removedProperty = FindRemovedKeyword(line, lineText, Keywords.RemovedProperties, "property");

		if (removedProperty is not null)
			return removedProperty;

		return FindRemovedKeyword(line, lineText, Keywords.RemovedConstants, "constant");
	}

	private TextDiagnostic? FindRemovedKeyword(ITextLine line, string lineText, IReadOnlyList<RemovedKeyword> keywords, string kindLabel)
	{
		foreach (RemovedKeyword keyword in keywords)
		{
			if (_engineVersion < keyword.RemovedVersion)
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

	private static TextDiagnostic CreateDiagnostic(ITextLine line, string lineText, string message, string keyPattern)
	{
		int matchIndex = string.IsNullOrWhiteSpace(keyPattern)
			? -1
			: lineText.IndexOf(keyPattern, StringComparison.Ordinal);

		int startOffset = matchIndex >= 0 ? line.Offset + matchIndex : line.Offset;
		int endOffset = matchIndex >= 0
			? startOffset + keyPattern.Length
			: Math.Max(line.Offset + 1, line.EndOffset);

		return new TextDiagnostic(TextDiagnosticSeverity.Error, message, startOffset, endOffset);
	}
}
