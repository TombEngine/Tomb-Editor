using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.ClassicScript.Types;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.ClassicScript.Services;

/// <summary>
/// Default implementation of <see cref="IClassicScriptCommandService"/>.
/// Provides command-level text operations for ClassicScript using the <see cref="ITextSnapshot"/> abstraction.
/// </summary>
public sealed class ClassicScriptCommandService : IClassicScriptCommandService
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private readonly IClassicScriptLineService _lineService;
	private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService;
	private readonly ClassicScriptSyntaxCatalogService _syntaxCatalogService;

	// Regex patterns for the ClassicScript continuation, directive, and command syntax.
	private static readonly Regex NextLineKeyRegex = new(@">\s*(;.*)?$", RegexOptions.Compiled);
	private static readonly Regex CustomizeCommandRegex = new(@"^\s*\bCustomize\s*=\s*\b.*\b\s*,", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	private static readonly Regex ParametersCommandRegex = new(@"^\s*\bParameters\s*=\s*\b.*\b\s*,", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	private static readonly Regex NameCommandRegex = new(@"^\s*\bName\s*=\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	private static readonly Regex IncludeCommandRegex = new(@"^\s*#include\s+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	private static readonly Regex DefineCommandRegex = new(@"^\s*#define\s+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptCommandService"/> class.
	/// </summary>
	public ClassicScriptCommandService(
		IClassicScriptLineService lineService,
		ClassicScriptMnemonicCatalogService mnemonicCatalogService,
		ClassicScriptSyntaxCatalogService syntaxCatalogService)
	{
		_lineService = lineService;
		_mnemonicCatalogService = mnemonicCatalogService;
		_syntaxCatalogService = syntaxCatalogService;
	}

	// ------------------------------------------------------------------
	// Command location and text
	// ------------------------------------------------------------------

	/// <inheritdoc/>
	public int? GetCommandStartLine(ITextSnapshot source, int offset)
	{
		ITextLine offsetLine = source.GetLineByOffset(offset);
		string offsetLineText = source.GetText(offsetLine.Offset, offsetLine.Length);
		string escapedText = _lineService.EscapeComments(offsetLineText);

		if (escapedText.Contains('=') || escapedText.TrimStart().StartsWith('#'))
			return offsetLine.LineNumber;

		if (_lineService.IsSectionHeaderLine(offsetLineText))
			return null;

		return FindCommandStartLine(source, offsetLine);
	}

	/// <inheritdoc/>
	public string? GetWholeCommandLineText(ITextSnapshot source, int offset)
	{
		int? commandStartLineNumber = GetCommandStartLine(source, offset);

		if (commandStartLineNumber is null)
			return null;

		ITextLine commandStartLine = source.GetLineByNumber(commandStartLineNumber.Value);
		string commandStartLineText = source.GetText(commandStartLine.Offset, commandStartLine.Length);

		if (!NextLineKeyRegex.IsMatch(commandStartLineText))
			return commandStartLineText;

		List<ITextLine> linesToMerge = GetLinesToMerge(source, commandStartLine);

		if (linesToMerge is null || linesToMerge.Count == 0)
			return null;

		return MergeLines(source, linesToMerge);
	}

	/// <inheritdoc/>
	public string? GetCommandKey(ITextSnapshot source, int offset)
	{
		int? commandStartLineNumber = GetCommandStartLine(source, offset);

		if (commandStartLineNumber is null)
			return null;

		ITextLine commandStartLine = source.GetLineByNumber(commandStartLineNumber.Value);
		string commandStartLineText = source.GetText(commandStartLine.Offset, commandStartLine.Length);
		commandStartLineText = _lineService.EscapeComments(commandStartLineText);

		if (commandStartLineText.TrimStart().StartsWith('#'))
			return commandStartLineText.Split(' ')[0].Trim();

		if (commandStartLineText.Contains('='))
		{
			string rawKey = commandStartLineText.Split('=')[0].Trim();
			return GetCorrectCommandVariation(source, commandStartLine.Offset, rawKey);
		}

		return null;
	}

	/// <inheritdoc/>
	public string? GetCommandSyntax(ITextSnapshot source, int offset)
	{
		string? wholeCommandLineText = GetWholeCommandLineText(source, offset);

		if (string.IsNullOrEmpty(wholeCommandLineText))
			return null;

		if (CustomizeCommandRegex.IsMatch(wholeCommandLineText))
			return GetSubcommandSyntax(wholeCommandLineText, SubcommandType.Cust);

		if (ParametersCommandRegex.IsMatch(wholeCommandLineText))
			return GetSubcommandSyntax(wholeCommandLineText, SubcommandType.Param);

		string? commandKey = GetCommandKey(source, offset);

		if (string.IsNullOrEmpty(commandKey))
			return null;

		return _syntaxCatalogService.GetCommandSyntax(commandKey);
	}

	/// <inheritdoc/>
	public IReadOnlyList<ClassicScriptSyntaxDefinition> GetCommandSyntaxDefinitions()
		=> _syntaxCatalogService.GetCommandSyntaxDefinitions();

	// ------------------------------------------------------------------
	// Argument navigation
	// ------------------------------------------------------------------

	/// <inheritdoc/>
	public int GetArgumentIndexAtOffset(ITextSnapshot source, int offset)
	{
		string? wholeLineText = GetWholeCommandLineText(source, offset);

		if (string.IsNullOrEmpty(wholeLineText))
			return -1;

		wholeLineText = _lineService.EscapeComments(wholeLineText);

		if (string.IsNullOrWhiteSpace(wholeLineText) || !wholeLineText.Contains('='))
			return -1;

		int totalArgumentCount = wholeLineText.Split(',').Length;

		int? commandStartLineNumber = GetCommandStartLine(source, offset);

		if (commandStartLineNumber is null)
			return -1;

		ITextLine commandStartLine = source.GetLineByNumber(commandStartLineNumber.Value);
		int wholeLineSubstringOffset = offset - commandStartLine.Offset;

		if (wholeLineSubstringOffset > wholeLineText.Length)
			return totalArgumentCount - 1;

		string textAfterOffset = wholeLineText.Remove(0, wholeLineSubstringOffset);
		int argumentCountAfterOffset = textAfterOffset.Split(',').Length;

		return totalArgumentCount - argumentCountAfterOffset;
	}

	/// <inheritdoc/>
	public string? GetArgumentFromIndex(ITextSnapshot source, int offset, int index)
	{
		string? wholeLineText = GetWholeCommandLineText(source, offset);

		if (wholeLineText is null)
			return null;

		return wholeLineText.Split(',')[index];
	}

	/// <inheritdoc/>
	public string? GetFlagPrefixOfCurrentArgument(ITextSnapshot source, int offset)
	{
		try
		{
			int currentArgumentIndex = GetArgumentIndexAtOffset(source, offset);

			if (currentArgumentIndex == -1)
				return null;

			string? syntax = GetCommandSyntax(source, offset);

			if (string.IsNullOrEmpty(syntax))
				return null;

			string[] syntaxArguments = syntax.Split(',');

			if (currentArgumentIndex >= syntaxArguments.Length)
				return null;

			string currentSyntaxArgument = syntaxArguments[currentArgumentIndex];

			if ((!currentSyntaxArgument.Contains('(') && !currentSyntaxArgument.Contains('.'))
				|| currentSyntaxArgument.Contains("(*Array*)", StringComparison.OrdinalIgnoreCase))
				return null;

			return currentSyntaxArgument.Split('.')[0].Split('(')[1];
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to determine the flag prefix of the current argument.");
			return null;
		}
	}

	// ------------------------------------------------------------------
	// Include path resolution
	// ------------------------------------------------------------------

	/// <inheritdoc/>
	public string? GetFullIncludePath(ITextSnapshot source, int offset)
	{
		ITextLine caretLine = source.GetLineByOffset(offset);
		string caretLineText = source.GetText(caretLine.Offset, caretLine.Length);

		if (!_lineService.IsValidIncludeLine(caretLineText))
			return null;

		if (source.FileName is null)
			return null;

		string? rootPath = Path.GetDirectoryName(source.FileName);

		if (rootPath is null)
			return null;

		string pathPart = caretLineText.Split('"')[1];

		return Path.Combine(rootPath, pathPart);
	}

	// ------------------------------------------------------------------
	// Section queries
	// ------------------------------------------------------------------

	/// <inheritdoc/>
	public bool DocumentContainsSections(ITextSnapshot source)
	{
		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);

			if (_lineService.IsSectionHeaderLine(lineText))
				return true;
		}

		return false;
	}

	/// <inheritdoc/>
	public int GetSectionsCount(ITextSnapshot source)
	{
		int sectionsCount = 0;

		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);

			if (_lineService.IsSectionHeaderLine(lineText))
				sectionsCount++;
		}

		return sectionsCount;
	}

	/// <inheritdoc/>
	public string? GetCurrentSectionName(ITextSnapshot source, int offset)
	{
		int? sectionStartLineNumber = GetStartLineOfCurrentSection(source, offset);

		if (sectionStartLineNumber is null)
			return null;

		ITextLine sectionStartLine = source.GetLineByNumber(sectionStartLineNumber.Value);
		string lineText = source.GetText(sectionStartLine.Offset, sectionStartLine.Length);

		return _lineService.GetSectionHeaderText(lineText);
	}

	/// <inheritdoc/>
	public int? GetStartLineOfCurrentSection(ITextSnapshot source, int offset)
	{
		ITextLine offsetLine = source.GetLineByOffset(offset);

		for (int i = offsetLine.LineNumber; i >= 1; i--)
		{
			ITextLine currentLine = source.GetLineByNumber(i);
			string currentLineText = source.GetText(currentLine.Offset, currentLine.Length);

			if (currentLineText.StartsWith('['))
				return currentLine.LineNumber;
		}

		return null;
	}

	/// <inheritdoc/>
	public int? GetLastLineOfCurrentSection(ITextSnapshot source, int offset)
	{
		ITextLine offsetLine = source.GetLineByOffset(offset);
		int? sectionStartLineNumber = GetStartLineOfCurrentSection(source, offset);

		if (sectionStartLineNumber is null)
			return null;

		for (int i = offsetLine.LineNumber; i <= source.LineCount; i++)
		{
			ITextLine iLine = source.GetLineByNumber(i);
			string iLineText = source.GetText(iLine.Offset, iLine.Length);

			if (i != sectionStartLineNumber.Value && (iLineText.StartsWith('[') || i == source.LineCount))
			{
				for (int j = i == source.LineCount ? i : i - 1; j >= 1; j--)
				{
					ITextLine jLine = source.GetLineByNumber(j);
					string jLineText = source.GetText(jLine.Offset, jLine.Length);

					if (!string.IsNullOrWhiteSpace(_lineService.RemoveComments(jLineText)))
						return jLine.LineNumber;
				}

				break;
			}
		}

		return null;
	}

	/// <inheritdoc/>
	public int? FindDocumentLineOfSection(ITextSnapshot source, string sectionName)
	{
		sectionName = sectionName.Trim('[').Trim(']').Trim();

		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);

			if (lineText.StartsWith('['))
			{
				string? headerText = _lineService.GetSectionHeaderText(lineText);

				if (headerText is not null && headerText.Equals(sectionName, StringComparison.OrdinalIgnoreCase))
					return line.LineNumber;
			}
		}

		return null;
	}

	// ------------------------------------------------------------------
	// Object lookup
	// ------------------------------------------------------------------

	/// <inheritdoc/>
	public int? FindDocumentLineOfObject(ITextSnapshot source, string objectName, ObjectType type)
	{
		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);

			switch (type)
			{
				case ObjectType.Section:
					if (lineText.TrimStart().StartsWith(objectName, StringComparison.Ordinal))
						return line.LineNumber;
					break;

				case ObjectType.Level:
					if (NameCommandRegex.Replace(lineText, string.Empty).StartsWith(objectName, StringComparison.Ordinal))
						return line.LineNumber;
					break;

				case ObjectType.Include:
					if (IncludeCommandRegex.Replace(lineText, string.Empty).TrimStart('"').StartsWith(objectName, StringComparison.Ordinal))
						return line.LineNumber;
					break;

				case ObjectType.Define:
					if (DefineCommandRegex.Replace(lineText, string.Empty).StartsWith(objectName, StringComparison.Ordinal))
						return line.LineNumber;
					break;
			}
		}

		return null;
	}

	/// <inheritdoc/>
	public bool IsLevelScriptDefined(ITextSnapshot source, string levelName)
	{
		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);

			if (NameCommandRegex.IsMatch(lineText))
			{
				string scriptLevelName = NameCommandRegex.Replace(_lineService.RemoveComments(lineText), string.Empty).Trim();

				if (scriptLevelName == levelName)
					return true;
			}
		}

		return false;
	}

	/// <inheritdoc/>
	public bool IsLevelLanguageStringDefined(ITextSnapshot source, string levelName)
	{
		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);
			string cleanString = _lineService.RemoveComments(_lineService.RemoveNGStringIndex(lineText)).Trim();

			if (cleanString == levelName)
				return true;
		}

		return false;
	}

	/// <inheritdoc/>
	public bool IsPluginDefined(ITextSnapshot source, string pluginName)
	{
		int? optionsSectionLineNumber = FindDocumentLineOfSection(source, "Options");

		if (optionsSectionLineNumber is null)
			return false;

		for (int i = optionsSectionLineNumber.Value; i <= source.LineCount; i++)
		{
			ITextLine line = source.GetLineByNumber(i);
			string? commandKey = GetCommandKey(source, line.Offset);

			if (commandKey is not null && commandKey.Equals("Plugin", StringComparison.OrdinalIgnoreCase))
			{
				string? wholeCommandLineText = GetWholeCommandLineText(source, line.Offset);

				if (wholeCommandLineText is null)
					continue;

				wholeCommandLineText = _lineService.RemoveComments(wholeCommandLineText);

				if (wholeCommandLineText.Contains(','))
				{
					string definedName = wholeCommandLineText.Split(',')[1]
						.Replace("\n", "").Replace("\r", "").Replace(">", "").Trim();

					if (definedName.Equals(pluginName, StringComparison.OrdinalIgnoreCase))
						return true;
				}
			}
		}

		return false;
	}

	// ------------------------------------------------------------------
	// Subcommand resolution
	// ------------------------------------------------------------------

	private string? GetSubcommandSyntax(string wholeCommandLineText, SubcommandType subcommandType)
	{
		string key = wholeCommandLineText.Split('=')[1].Split(',')[0].Trim();
		string? custParamSyntax = FindCustParamSyntaxByKey(key, subcommandType);

		if (!string.IsNullOrEmpty(custParamSyntax))
			return custParamSyntax;

		return subcommandType switch
		{
			SubcommandType.Cust => _syntaxCatalogService.GetCommandSyntax("Customize"),
			SubcommandType.Param => _syntaxCatalogService.GetCommandSyntax("Parameters"),
			_ => null
		};
	}

	private string? FindCustParamSyntaxByKey(string key, SubcommandType subcommandType)
	{
		if (string.IsNullOrWhiteSpace(key))
			return null;

		string? syntax = subcommandType switch
		{
			SubcommandType.Cust => _syntaxCatalogService.GetCustomizeSyntax(key),
			SubcommandType.Param => _syntaxCatalogService.GetParameterSyntax(key),
			_ => null
		};

		if (!string.IsNullOrEmpty(syntax))
			return syntax;

		return _mnemonicCatalogService.TryGetPluginSyntax(key, out string? pluginSyntax)
			? pluginSyntax
			: null;
	}

	// ------------------------------------------------------------------
	// Command variation
	// ------------------------------------------------------------------

	private string? GetCorrectCommandVariation(ITextSnapshot source, int offset, string command)
	{
		string? sectionName = GetCurrentSectionName(source, offset);

		if (command.Equals("level", StringComparison.OrdinalIgnoreCase))
			return GetCorrectLevelCommandForSection(sectionName);
		else if (command.Equals("cut", StringComparison.OrdinalIgnoreCase))
			return GetCorrectCutCommandForSection(sectionName);
		else if (command.Equals("fmv", StringComparison.OrdinalIgnoreCase))
			return GetCorrectFMVCommandForSection(sectionName);
		else
			return command;
	}

	private static string GetCorrectLevelCommandForSection(string? sectionName)
	{
		if (string.IsNullOrEmpty(sectionName))
			return "LevelLevel";

		switch (sectionName.ToUpperInvariant())
		{
			case "PCEXTENSIONS":
				return "LevelPC";

			case "PSXEXTENSIONS":
				return "LevelPSX";

			default:
				return "LevelLevel";
		}
	}

	private static string? GetCorrectCutCommandForSection(string? sectionName)
	{
		if (string.IsNullOrEmpty(sectionName))
			return null;

		switch (sectionName.ToUpperInvariant())
		{
			case "PCEXTENSIONS":
				return "CutPC";

			case "PSXEXTENSIONS":
				return "CutPSX";

			default:
				return null;
		}
	}

	private static string GetCorrectFMVCommandForSection(string? sectionName)
	{
		if (string.IsNullOrEmpty(sectionName))
			return "FMVLevel";

		switch (sectionName.ToUpperInvariant())
		{
			case "PCEXTENSIONS":
				return "FMVPC";

			case "PSXEXTENSIONS":
				return "FMVPSX";

			default:
				return "FMVLevel";
		}
	}

	// ------------------------------------------------------------------
	// Continuation line helpers
	// ------------------------------------------------------------------

	private int? FindCommandStartLine(ITextSnapshot source, ITextLine searchStartingLine)
	{
		ITextLine previousLine;
		string previousLineText;

		int i = searchStartingLine.LineNumber - 1;

		do
		{
			if (i <= 0)
				return null;

			previousLine = source.GetLineByNumber(i);
			previousLineText = _lineService.EscapeComments(source.GetText(previousLine.Offset, previousLine.Length));

			if (NextLineKeyRegex.IsMatch(previousLineText) && previousLineText.Contains('='))
				return previousLine.LineNumber;

			i--;
		}
		while (NextLineKeyRegex.IsMatch(previousLineText));

		return null;
	}

	private static List<ITextLine> GetLinesToMerge(ITextSnapshot source, ITextLine startingLine)
	{
		var lines = new List<ITextLine> { startingLine };

		ITextLine nextLine;
		string nextLineText;

		int i = startingLine.LineNumber + 1;

		do
		{
			if (i > source.LineCount)
				return lines;

			nextLine = source.GetLineByNumber(i);
			nextLineText = source.GetText(nextLine.Offset, nextLine.Length);

			lines.Add(nextLine);

			i++;
		}
		while (NextLineKeyRegex.IsMatch(nextLineText));

		return lines;
	}

	private static string MergeLines(ITextSnapshot source, List<ITextLine> lines)
	{
		var builder = new StringBuilder();

		foreach (ITextLine line in lines)
			builder.Append(source.GetText(line.Offset, line.Length) + Environment.NewLine);

		return builder.ToString();
	}
}
