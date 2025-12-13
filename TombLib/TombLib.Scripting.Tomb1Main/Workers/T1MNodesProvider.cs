using DarkUI.Controls;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Tomb1Main.Parsers;
using TombLib.Scripting.Tomb1Main.Resources;

namespace TombLib.Scripting.Tomb1Main.Workers
{
	public class T1MNodesProvider : ContentNodesProviderBase
	{
		protected override IEnumerable<DarkTreeNode> GetNodes(string content)
		{
			var nodes = new List<DarkTreeNode>();
			var document = new TextDocument(content);

			foreach (DocumentLine line in document.Lines)
			{
				string lineText = document.GetText(line.Offset, line.Length);
				DarkTreeNode levelNode = GetLevelNode(lineText);

				if (levelNode != null)
					nodes.Add(levelNode);
			}

			return nodes;
		}

		private DarkTreeNode GetLevelNode(string lineText)
		{
			var regex = new Regex(Patterns.LevelProperty, RegexOptions.IgnoreCase);

			if (regex.IsMatch(lineText))
			{
				lineText = LineParser.RemoveComments(lineText);
				string levelName = regex.Replace(lineText, string.Empty).Trim().TrimEnd(',').Trim('"'); // Removes "title":

				if (!string.IsNullOrWhiteSpace(levelName) && levelName.Contains(Filter, StringComparison.OrdinalIgnoreCase))
					return new DarkTreeNode(levelName);
			}

			regex = new Regex(Patterns.LevelCommentName, RegexOptions.IgnoreCase);
			Match regexMatch = regex.Match(lineText);

			if (regexMatch.Success)
			{
				string levelName = regexMatch.Groups[3].Value.Trim(); // Extracts the level name from the comment

				if (!string.IsNullOrWhiteSpace(levelName) && levelName.Contains(Filter, StringComparison.OrdinalIgnoreCase))
					return new DarkTreeNode(levelName);
			}

			return null;
		}
	}
}
