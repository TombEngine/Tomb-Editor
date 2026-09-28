using System;
using System.Text.RegularExpressions;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.GameFlowScript.Types;

namespace TombLib.Scripting.GameFlowScript.Services;

/// <summary>
/// Default implementation of <see cref="IGameFlowScriptDocumentService"/>.
/// Provides document-level operations using <see cref="ITextSnapshot"/> and
/// the line service.
/// </summary>
public sealed class GameFlowScriptDocumentService : IGameFlowScriptDocumentService
{
	private readonly IGameFlowScriptLineService _lineService;
	private readonly Regex _levelPropertyRegex = new(Patterns.LevelProperty, RegexOptions.IgnoreCase);

	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowScriptDocumentService"/> class.
	/// </summary>
	public GameFlowScriptDocumentService(IGameFlowScriptLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_lineService = lineService;
	}

	/// <inheritdoc/>
	public bool IsLevelScriptDefined(ITextSnapshot source, string levelName)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(levelName);

		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);

			if (_levelPropertyRegex.IsMatch(lineText))
			{
					string scriptLevelName = _levelPropertyRegex.Replace(
						_lineService.RemoveComments(lineText), string.Empty).Trim();

				if (scriptLevelName == levelName)
					return true;
			}
		}

		return false;
	}

	/// <inheritdoc/>
	public int? FindDocumentLineOfObject(ITextSnapshot source, string objectName, ObjectType type)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(objectName);

		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);

			switch (type)
			{
				case ObjectType.Section:
					if (lineText.StartsWith(objectName, StringComparison.Ordinal))
						return line.LineNumber;

					break;

				case ObjectType.Level:
					string levelText = _levelPropertyRegex.Replace(lineText, string.Empty);

					if (levelText.StartsWith(objectName, StringComparison.Ordinal))
						return line.LineNumber;

					break;
			}
		}

		return null;
	}
}
