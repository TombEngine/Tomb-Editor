using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Nickelony.IDEKit.Core.Comments;
using Nickelony.IDEKit.Core.Text;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.GameFlowScript.Types;

namespace TombLib.Scripting.GameFlowScript.ContentNodes;

internal sealed class GameFlowContentNodeService
{
	private static readonly Regex LevelPropertyRegex = new(Patterns.LevelProperty, RegexOptions.IgnoreCase);
	private readonly IGameFlowScriptLineService _lineService;

	public GameFlowContentNodeService(IGameFlowScriptLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_lineService = lineService;
	}

	public IReadOnlyList<GameFlowContentNodeGroup> GetNodeGroups(string content, string filter)
	{
		var source = new StringTextSnapshot(content ?? string.Empty);
		var sectionNodes = new List<GameFlowContentNode>();
		var levelNodes = new List<GameFlowContentNode>();

		foreach (ITextLine line in source.Lines)
		{
			string lineText = source.GetText(line.Offset, line.Length);

			GameFlowContentNode? sectionNode = GetSectionNode(lineText, filter);

			if (sectionNode is GameFlowContentNode matchedSectionNode)
			{
				sectionNodes.Add(matchedSectionNode);
				continue;
			}

			GameFlowContentNode? levelNode = GetLevelNode(lineText, filter);

			if (levelNode is GameFlowContentNode matchedLevelNode)
				levelNodes.Add(matchedLevelNode);
		}

		var groups = new List<GameFlowContentNodeGroup>(2);

		if (sectionNodes.Count > 0)
			groups.Add(new GameFlowContentNodeGroup("Sections", sectionNodes));

		if (levelNodes.Count > 0)
			groups.Add(new GameFlowContentNodeGroup("Levels", levelNodes));

		return groups;
	}

	private GameFlowContentNode? GetSectionNode(string lineText, string filter)
	{
		if (!_lineService.IsSectionHeaderLine(lineText))
			return null;

		string? headerText = _lineService.GetSectionHeaderText(lineText);

		if (string.IsNullOrWhiteSpace(headerText))
			return null;

		bool isLevelHeader = headerText.Equals("Level", StringComparison.OrdinalIgnoreCase);
		bool isEndHeader = headerText.Equals("END", StringComparison.OrdinalIgnoreCase);

		if (isLevelHeader || isEndHeader
			|| !GameFlowDefinitionCatalog.Sections.Any(x => x.Equals(headerText, StringComparison.OrdinalIgnoreCase))
			|| !headerText.Contains(filter, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		return new GameFlowContentNode(headerText, ObjectType.Section);
	}

	private static GameFlowContentNode? GetLevelNode(string lineText, string filter)
	{
		if (!LevelPropertyRegex.IsMatch(lineText))
			return null;

		string sanitizedLineText = CommentOperations.RemoveComments(lineText, new CommentSyntax("//", null, StringLiteralStyle.DoubleQuoted | StringLiteralStyle.TripleDoubleQuoted));
		string levelName = LevelPropertyRegex.Replace(sanitizedLineText, string.Empty);

		if (string.IsNullOrWhiteSpace(levelName) || !levelName.Contains(filter, StringComparison.OrdinalIgnoreCase))
			return null;

		return new GameFlowContentNode(levelName, ObjectType.Level);
	}
}

internal sealed class GameFlowContentNodeGroup(string header, IReadOnlyList<GameFlowContentNode> nodes)
{
	public string Header { get; } = header ?? string.Empty;
	public IReadOnlyList<GameFlowContentNode> Nodes { get; } = nodes ?? [];
}

internal readonly record struct GameFlowContentNode(string Text, ObjectType ObjectType);
