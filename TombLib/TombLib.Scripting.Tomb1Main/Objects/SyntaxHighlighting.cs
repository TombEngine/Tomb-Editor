using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.Tomb1Main.Resources;
using TombLib.Scripting.Tomb1Main.Services;

namespace TombLib.Scripting.Tomb1Main.Objects
{
	public sealed class SyntaxHighlighting : IHighlightingDefinition
	{
		private readonly ColorScheme _scheme;
		private readonly IGameflowSchemaService _schemaService;

		#region Construction

		public SyntaxHighlighting(ColorScheme scheme, IGameflowSchemaService schemaService)
		{
			_scheme = scheme;
			_schemaService = schemaService;
		}

		#endregion Construction

		#region Rules

		public HighlightingRuleSet MainRuleSet
		{
			get
			{
				var patterns = new Patterns(_schemaService);
				var ruleSet = new HighlightingRuleSet();

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(patterns.Comments),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Comments.HtmlColor)),
						FontWeight = _scheme.Comments.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Comments.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(patterns.Collections, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Collections.HtmlColor)),
						FontWeight = _scheme.Collections.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Collections.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(patterns.Properties, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Properties.HtmlColor)),
						FontWeight = _scheme.Properties.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Properties.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(patterns.Constants, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Constants.HtmlColor)),
						FontWeight = _scheme.Constants.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Constants.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(patterns.Values, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Values.HtmlColor)),
						FontWeight = _scheme.Values.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Values.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(patterns.Strings),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Strings.HtmlColor)),
						FontWeight = _scheme.Strings.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Strings.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Name = "TRX Rules";
				return ruleSet;
			}
		}

		#endregion Rules

		#region Other

		public string Name => "TRX Rules";

		public IEnumerable<HighlightingColor> NamedHighlightingColors => throw new NotImplementedException();
		public IDictionary<string, string> Properties => throw new NotImplementedException();

		public HighlightingColor GetNamedColor(string name)
			=> throw new NotImplementedException();

		public HighlightingRuleSet GetNamedRuleSet(string name)
			=> throw new NotImplementedException();

		#endregion Other
	}
}
