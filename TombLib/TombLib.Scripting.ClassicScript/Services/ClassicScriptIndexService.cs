using NCalc;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.Extensions;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.ClassicScript.Services;

/// <summary>
/// Default implementation of <see cref="IClassicScriptIndexService"/>.
/// Uses a per-call <see cref="EvaluationContext"/> and CoreCLR-NCalc for expression evaluation.
/// </summary>
public sealed class ClassicScriptIndexService : IClassicScriptIndexService
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private readonly IClassicScriptCommandService _commandService;
	private readonly IClassicScriptLineService _lineService;
	private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService;

	// Commands that support sequential free-index discovery.
	private static readonly string[] IndexedCommands =
	[
		"AddEffect", "ColorRGB", "GlobalTrigger", "Image", "ItemGroup",
		"MultiEnvCondition", "Organizer", "Parameters", "TestPosition",
		"TriggerGroup", "Plugin"
	];

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptIndexService"/> class.
	/// </summary>
	public ClassicScriptIndexService(
		IClassicScriptCommandService commandService,
		IClassicScriptLineService lineService,
		ClassicScriptMnemonicCatalogService mnemonicCatalogService)
	{
		_commandService = commandService;
		_lineService = lineService;
		_mnemonicCatalogService = mnemonicCatalogService;
	}

	/// <inheritdoc/>
	public int GetNextFreeIndex(ITextSnapshot source, int offset)
	{
		string? commandKey = _commandService.GetCommandKey(source, offset);
		return GetNextFreeIndex(source, offset, commandKey);
	}

	/// <inheritdoc/>
	public int GetNextFreeIndex(ITextSnapshot source, int offset, string? commandKey)
	{
		if (string.IsNullOrEmpty(commandKey))
			return -1;

		if (!commandKey.IgnoreCaseEqualsAny(IndexedCommands))
			return -1;

		int loopStartLine = 1;
		int loopEndLine = source.LineCount;

		if (_commandService.DocumentContainsSections(source))
		{
			int? sectionStartLineNumber = _commandService.GetStartLineOfCurrentSection(source, offset);

			if (sectionStartLineNumber is null)
				return -1;

			loopStartLine = sectionStartLineNumber.Value + 1;
			loopEndLine = GetSectionEndLine(source, sectionStartLineNumber.Value);
		}

		var context = new EvaluationContext();
		int firstId = GetFirstId(source, commandKey, loopStartLine, loopEndLine, context);
		IEnumerable<int> takenIndicesList = GetTakenIndicesList(source, commandKey, loopStartLine, loopEndLine);

		int nextFreeIndex = firstId;

		while (takenIndicesList.Contains(nextFreeIndex))
			nextFreeIndex++;

		return nextFreeIndex;
	}

	// ------------------------------------------------------------------
	// #FIRST_ID evaluation
	// ------------------------------------------------------------------

	private int GetFirstId(ITextSnapshot source, string commandKey, int loopStartLine, int loopEndLine, EvaluationContext context)
	{
		var firstIdRegex = new Regex(
			$@"^\s*#FIRST_ID\s+{Regex.Escape(commandKey)}\s*=\s*(.*)\s*(;.*)?$",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		return EvaluateDefinePattern(source, firstIdRegex, loopStartLine, loopEndLine, context, clampToMinOne: true);
	}

	// ------------------------------------------------------------------
	// Variable resolution
	// ------------------------------------------------------------------

	private string ResolveExpressionVariables(ITextSnapshot source, string expressionString, EvaluationContext context)
	{
		IEnumerable<string> variables = expressionString
			.Split('+', '-', '*', '/')
			.Select(x => x.Replace('(', ' ').Replace(')', ' ').Trim())
			.Where(x => !string.IsNullOrWhiteSpace(x));

		foreach (string variable in variables)
		{
			if (int.TryParse(variable, out int _))
				continue;

			if (_mnemonicCatalogService.TryGetDecimalValue(variable, out int rowValue))
			{
				expressionString = expressionString.Replace(variable, rowValue.ToString());
			}
			else
			{
				expressionString = expressionString.Replace(
					variable,
					GetVariableValue(source, variable, context).ToString());
			}
		}

		return expressionString;
	}

	private int GetVariableValue(ITextSnapshot source, string variable, EvaluationContext context)
	{
		if (!context.VisitedVariables.Add(variable))
			return 0;

		var defineRegex = new Regex(
			$@"^\s*#DEFINE\s+{Regex.Escape(variable)}\s+(.*)\s*(;.*)?$",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		return EvaluateDefinePattern(source, defineRegex, loopStartLine: 1, loopEndLine: source.LineCount, context, clampToMinOne: false);
	}

	private int EvaluateDefinePattern(ITextSnapshot source, Regex defineRegex, int loopStartLine, int loopEndLine, EvaluationContext context, bool clampToMinOne)
	{
		int result = 0;

		try
		{
			for (int i = loopStartLine; i <= loopEndLine; i++)
			{
				ITextLine line = source.GetLineByNumber(i);
				string? lineText = _commandService.GetWholeCommandLineText(source, line.Offset);

				if (lineText is null)
					continue;

				lineText = _lineService.EscapeCommentsAndNewLines(lineText);
				Match match = defineRegex.Match(lineText);

				if (match.Success)
				{
					string expressionString = match.Groups[1].Value;
					expressionString = ResolveExpressionVariables(source, expressionString, context);
					result = EvaluateExpression(expressionString);
				}
			}
		}
		catch (Exception exception)
		{
			// A malformed define expression yields the default result instead of failing the scan.
			Log.Warn(exception, "Failed to evaluate a define expression; using the default result.");
		}

		return clampToMinOne && result < 1 ? 1 : result;
	}

	// ------------------------------------------------------------------
	// NCalc expression evaluation
	// ------------------------------------------------------------------

	private static int EvaluateExpression(string expressionString)
	{
		var expression = new Expression(expressionString);

		if (expression.HasErrors())
			return 0;

		if (expressionString.Contains('/'))
			return expression.HasErrors() ? 0 : (int)Math.Floor((double)expression.Evaluate());
		else
			return expression.HasErrors() ? 0 : (int)expression.Evaluate();
	}

	// ------------------------------------------------------------------
	// Taken indices
	// ------------------------------------------------------------------

	private IEnumerable<int> GetTakenIndicesList(ITextSnapshot source, string commandKey, int loopStartLine, int loopEndLine)
	{
		for (int i = loopStartLine; i <= loopEndLine; i++)
		{
			ITextLine processedLine = source.GetLineByNumber(i);
			string processedLineText = source.GetText(processedLine.Offset, processedLine.Length);
			processedLineText = _lineService.EscapeComments(processedLineText);

			string? command = _commandService.GetCommandKey(source, processedLine.Offset);

			if (string.IsNullOrEmpty(command) || !processedLineText.Contains('='))
				continue;

			if (command.Equals(commandKey, StringComparison.OrdinalIgnoreCase))
			{
				if (int.TryParse(processedLineText.Split('=')[1].Split(',')[0].Trim(), out int takenIndex))
					yield return takenIndex;
			}
		}
	}

	private static int GetSectionEndLine(ITextSnapshot source, int sectionStartLineNumber)
	{
		for (int i = sectionStartLineNumber + 1; i <= source.LineCount; i++)
		{
			ITextLine line = source.GetLineByNumber(i);
			string lineText = source.GetText(line.Offset, line.Length);

			if (lineText.StartsWith('['))
				return i - 1;
		}

		return source.LineCount;
	}

	/// <summary>
	/// Holds per-call state for #DEFINE variable resolution to detect cycles.
	/// Each call to <see cref="GetNextFreeIndex(ITextSnapshot, int, string?)"/>
	/// creates a fresh instance, eliminating mutable singleton state.
	/// </summary>
	private sealed class EvaluationContext
	{
		/// <summary>
		/// Gets the set of variable names already visited in the current resolution chain.
		/// Case-insensitive comparison for #DEFINE variable names.
		/// </summary>
		public HashSet<string> VisitedVariables { get; } = new(StringComparer.OrdinalIgnoreCase);
	}
}
