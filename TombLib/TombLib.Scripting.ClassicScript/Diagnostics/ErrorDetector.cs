using Nickelony.IDEKit.Core.Diagnostics;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.ClassicScript.Commands;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Syntaxes;
using Nickelony.IDEKit.Core.Comments;

namespace TombLib.Scripting.ClassicScript.Diagnostics;

/// <summary>
/// Detects errors in ClassicScript document content.
/// </summary>
public sealed class ErrorDetector : ITextDiagnosticsProvider
{
	private readonly IClassicScriptLineService _lineService;
	private readonly IClassicScriptCommandService _commandService;
	private readonly ClassicScriptSyntaxCatalogService _syntaxCatalogService;
	private readonly ClassicScriptCommandCatalogService _commandCatalogService = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="ErrorDetector"/> class.
	/// </summary>
	/// <param name="lineService">The line service used to analyze document lines.</param>
	/// <param name="commandService">The command service used to validate command syntax.</param>
	/// <param name="syntaxCatalogService">The syntax catalog service used to validate arguments.</param>
	public ErrorDetector(
		IClassicScriptLineService lineService,
		IClassicScriptCommandService commandService,
		ClassicScriptSyntaxCatalogService syntaxCatalogService)
	{
		_lineService = lineService;
		_commandService = commandService;
		_syntaxCatalogService = syntaxCatalogService;
	}

	// Public methods

	/// <summary>
	/// Gets the diagnostics for the given request.
	/// </summary>
	/// <param name="request">The diagnostics request.</param>
	/// <returns>The diagnostics describing the detected errors.</returns>
	public IReadOnlyList<TextDiagnostic> GetDiagnostics(TextDiagnosticsRequest request)
		=> DetectErrorLines(new StringTextSnapshot(request.DocumentText));

	// Error line finding

	private List<TextDiagnostic> DetectErrorLines(ITextSnapshot source)
	{
		var errorLines = new List<TextDiagnostic>();

		bool commandSectionCheckRequired = _commandService.DocumentContainsSections(source);

		foreach (ITextLine processedLine in source.Lines)
		{
			string processedLineText = source.GetText(processedLine.Offset, processedLine.Length);

			if (_lineService.IsEmptyOrComments(processedLineText))
				continue;

			TextDiagnostic? error = FindErrorsInLine(source, processedLine, processedLineText, commandSectionCheckRequired);

			if (error is not null)
				errorLines.Add(error);
		}

		return errorLines;
	}

	private TextDiagnostic? FindErrorsInLine(ITextSnapshot source, ITextLine line, string lineText, bool commandSectionCheckRequired)
	{
		if (_lineService.IsSectionHeaderLine(lineText))
		{
			return FindErrorsInSectionHeaderLine(source, line, lineText);
		}
		else
		{
			if (commandSectionCheckRequired && IsLineInStandardStringSection(source, line))
				return null;
			else if (commandSectionCheckRequired && IsLineInExtraNGSection(source, line))
				return FindErrorsInNGStringLine(source, line, lineText);
			else
				return FindErrorsInCommandLine(source, line, lineText, commandSectionCheckRequired);
		}
	}

	private TextDiagnostic? FindErrorsInSectionHeaderLine(ITextSnapshot source, ITextLine line, string lineText)
	{
		if (!IsValidSectionName(lineText))
		{
			return CreateDiagnostic(source, line,
				"Invalid section name. Please check its spelling.", _lineService.RemoveComments(lineText));
		}

		return null;
	}

	private TextDiagnostic? FindErrorsInNGStringLine(ITextSnapshot source, ITextLine line, string lineText)
	{
		if (!IsNGStringLineWellFormatted(lineText))
		{
			return CreateDiagnostic(source, line,
				"NG string must start with an index.\n\nExample:\n0: First String\n1: Second String",
				_lineService.RemoveComments(lineText));
		}

		return null;
	}

	private TextDiagnostic? FindErrorsInCommandLine(ITextSnapshot source, ITextLine line, string lineText, bool commandSectionCheckRequired)
	{
		string? commandKey = _commandService.GetCommandKey(source, line.Offset);

		if (!IsValidCommandKey(commandKey))
		{
			string errorSegmentText = Regex.Match(_lineService.RemoveComments(lineText), "^.*=").Value.TrimEnd();

			if (errorSegmentText.Length == 0 && commandKey is not null)
				return null;

			if (commandKey is null)
				errorSegmentText = lineText.TrimEnd();

			return CreateDiagnostic(source, line,
				"Invalid command. Please check its spelling.", errorSegmentText);
		}

		if (commandSectionCheckRequired && commandKey is not null && !IsCommandLineInCorrectSection(source, line.LineNumber, commandKey))
		{
			return CreateDiagnostic(source, line,
				"Command is placed in the wrong section. Please check the command syntax.",
				_lineService.RemoveComments(lineText));
		}

		if (ContainsBrokenNextLines(source, line.Offset))
		{
			return CreateArgumentDiagnostic(source, line, lineText,
				"Misplaced \">\" symbols were found.\nYou can only use these symbols at the end of the line and there can only be one on each line.");
		}

		if (!IsArgumentCountValid(source, line.Offset))
			return CreateArgumentDiagnostic(source, line, lineText, "Invalid argument count. Please check the command syntax.");

		if (ContainsEmptyArguments(source, line.Offset))
			return CreateArgumentDiagnostic(source, line, lineText, "Empty arguments were found.");

		return null;
	}

	private TextDiagnostic? CreateArgumentDiagnostic(ITextSnapshot source, ITextLine line, string lineText, string message)
	{
		string errorSegmentText = Regex.Match(_lineService.RemoveComments(lineText), @"=\s*(\b.*)").Groups[1].Value;

		if (errorSegmentText.Length == 0)
			errorSegmentText = _lineService.RemoveComments(lineText);

		return CreateDiagnostic(source, line, message, errorSegmentText);
	}

	private static TextDiagnostic CreateDiagnostic(ITextSnapshot source, ITextLine line, string message, string errorSegmentText)
	{
		string lineText = source.GetText(line.Offset, line.Length);
		string segmentText = string.IsNullOrWhiteSpace(errorSegmentText)
			? lineText.Trim()
			: errorSegmentText;

		int startOffset = line.Offset;
		int endOffset = Math.Max(line.Offset + 1, line.EndOffset);

		if (!string.IsNullOrWhiteSpace(segmentText))
		{
			int matchIndex = lineText.IndexOf(segmentText, StringComparison.Ordinal);

			if (matchIndex >= 0)
			{
				startOffset = line.Offset + matchIndex;
				endOffset = startOffset + segmentText.Length;
			}
		}

		return new TextDiagnostic(TextDiagnosticSeverity.Error, message, startOffset, endOffset);
	}

	// Error detection methods

	private bool IsValidSectionName(string sectionHeaderLineText)
	{
		string? section = _lineService.GetSectionHeaderText(sectionHeaderLineText);

		if (string.IsNullOrEmpty(section))
			return false;

		foreach (string entry in _commandCatalogService.Sections)
		{
			if (section.Equals(entry, StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	private static bool IsNGStringLineWellFormatted(string lineText)
		=> Regex.IsMatch(lineText, @"^\d*:.*");

	private bool IsValidCommandKey(string? commandKey)
	{
		return commandKey is not null && _syntaxCatalogService.GetCommandDefinition(commandKey) is not null;
	}

	private bool IsLineInStandardStringSection(ITextSnapshot source, ITextLine line)
	{
		string? sectionName = _commandService.GetCurrentSectionName(source, line.Offset);
		return _lineService.IsStandardStringSectionName(sectionName);
	}

	private bool IsLineInExtraNGSection(ITextSnapshot source, ITextLine line)
	{
		string? sectionName = _commandService.GetCurrentSectionName(source, line.Offset);
		return _lineService.IsExtraNGSectionName(sectionName);
	}

	private bool IsCommandLineInCorrectSection(ITextSnapshot source, int lineNumber, string command)
	{
		ClassicScriptSyntaxDefinition? definition = _syntaxCatalogService.GetCommandDefinition(command);
		string correctSection = definition?.ApplicableSection ?? string.Empty;

		if (string.IsNullOrWhiteSpace(correctSection) || correctSection.Equals("any", StringComparison.OrdinalIgnoreCase))
			return true;

		ITextLine line = source.GetLineByNumber(lineNumber);
		int? sectionStartLineNumber = _commandService.GetStartLineOfCurrentSection(source, line.Offset);

		if (sectionStartLineNumber is null)
			return false;

		ITextLine sectionStartLine = source.GetLineByNumber(sectionStartLineNumber.Value);
		string sectionHeaderText = source.GetText(sectionStartLine.Offset, sectionStartLine.Length);

		if (correctSection.Equals("level", StringComparison.OrdinalIgnoreCase))
			return Regex.IsMatch(sectionHeaderText, @"\[(level|title)\]", RegexOptions.IgnoreCase);

		return Regex.IsMatch(sectionHeaderText, @"\[" + correctSection + @"\]", RegexOptions.IgnoreCase);
	}

	private bool ContainsBrokenNextLines(ITextSnapshot source, int lineOffset)
	{
		int? startingLineNumber = _commandService.GetCommandStartLine(source, lineOffset);

		if (startingLineNumber is null)
			return false;

		string nextLineText;

		int i = startingLineNumber.Value;

		do
		{
			if (i > source.LineCount)
				break;

			ITextLine nextLine = source.GetLineByNumber(i);
			nextLineText = _lineService.EscapeComments(source.GetText(nextLine.Offset, nextLine.Length));

			if ((nextLineText.Contains('>') && !ContinuationOperations.EndsWithContinuationMarker(nextLineText, new CommentSyntax(";", null, StringLiteralStyle.None), '>')) || nextLineText.Count(c => c == '>') > 1)
				return true;

			i++;
		}
		while (ContinuationOperations.EndsWithContinuationMarker(nextLineText, new CommentSyntax(";", null, StringLiteralStyle.None), '>'));

		return false;
	}

	private bool IsArgumentCountValid(ITextSnapshot source, int lineOffset)
	{
		string? lineText = _commandService.GetWholeCommandLineText(source, lineOffset);

		if (lineText is null)
			return false;

		if (lineText.TrimStart().StartsWith('#'))
			return true;

		lineText = _lineService.EscapeComments(lineText);

		if (!lineText.Contains('='))
			return false;

		string? command = _commandService.GetCommandKey(source, lineOffset);

		if (string.IsNullOrEmpty(command))
			return false;

		if (command.Equals("Legend", StringComparison.OrdinalIgnoreCase)) // "Legend=" ignores commas
			return true;

		int argumentCount = _lineService.EscapeComments(lineText).Split('=')[1].Split(',').Length;

		if (argumentCount == 1 && string.IsNullOrWhiteSpace(lineText.Split('=')[1]))
			argumentCount = 0;

		ClassicScriptSyntaxDefinition? definition = _syntaxCatalogService.GetCommandDefinition(command);

		if (definition is null)
			return false;

		// Array-argument commands accept any number of arguments, so the count is always valid.
		if (definition.HasArrayArguments)
			return true;

		return argumentCount == definition.ArgumentCount;
	}

	private bool ContainsEmptyArguments(ITextSnapshot source, int lineOffset)
	{
		string? lineText = _commandService.GetWholeCommandLineText(source, lineOffset);

		if (string.IsNullOrEmpty(lineText))
			return true;

		string[] arguments = _lineService.EscapeComments(lineText).Split(',');

		foreach (string argument in arguments)
		{
			if (string.IsNullOrWhiteSpace(argument.Replace('>', ' ')))
				return true;
		}

		return false;
	}
}
