using Nickelony.IDEKit.AvalonEdit.Highlighting;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TombLib.Scripting.GameFlowScript.Resources;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Scripting.GameFlowScript.Highlighting;

/// <summary>
/// Provides the highlighting definition for the GameFlow editor.
/// </summary>
public sealed class SyntaxHighlighting : RegexHighlightingDefinition
{
	private readonly ColorScheme _scheme;

	/// <summary>
	/// Initializes a new instance of the <see cref="SyntaxHighlighting"/> class.
	/// </summary>
	/// <param name="scheme">The color scheme used for the highlighting rules.</param>
	public SyntaxHighlighting(ColorScheme scheme)
		: base("GameFlowScript Rules")
	{
		_scheme = scheme;
	}

	/// <inheritdoc/>
	protected override IEnumerable<RegexHighlightingRule> BuildRules()
	{
		var rules = new List<RegexHighlightingRule>();

		rules.Add(new(new Regex(Patterns.Comments), Style(_scheme.Comments)));
		rules.Add(new(new Regex(Patterns.BlockComments), Style(_scheme.Comments)));

		if (GameFlowDefinitionCatalog.Sections.Count > 0)
			rules.Add(new(new Regex(Patterns.Sections, RegexOptions.IgnoreCase), Style(_scheme.Sections)));

		if (GameFlowDefinitionCatalog.SpecialProperties.Count > 0)
			rules.Add(new(new Regex(Patterns.SpecialProperties, RegexOptions.IgnoreCase), Style(_scheme.SpecialProperties)));

		if (GameFlowDefinitionCatalog.Properties.Count > 0)
			rules.Add(new(new Regex(Patterns.Properties, RegexOptions.IgnoreCase), Style(_scheme.Properties)));

		if (GameFlowDefinitionCatalog.Constants.Count > 0)
			rules.Add(new(new Regex(Patterns.Constants, RegexOptions.IgnoreCase), Style(_scheme.Constants)));

		rules.Add(new(new Regex(Patterns.Values), Style(_scheme.Values)));

		return rules;
	}

	private static RegexHighlightingStyle Style(HighlightingObject scheme)
		=> new(scheme.HtmlColor, scheme.IsBold, scheme.IsItalic);
}
