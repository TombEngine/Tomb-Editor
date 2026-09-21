using Nickelony.IDEKit.AvalonEdit.Highlighting;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Scripting.TRX.Highlighting;

/// <summary>
/// Provides the TRX highlighting definition built from the active color scheme and GameFlow schema.
/// </summary>
public sealed class SyntaxHighlighting : RegexHighlightingDefinition
{
	private readonly ColorScheme _scheme;
	private readonly ITRXGameFlowSchemaService _schemaService;

	/// <summary>
	/// Initializes a new instance of the <see cref="SyntaxHighlighting"/> class.
	/// </summary>
	/// <param name="scheme">The color scheme used for the highlighting rules.</param>
	/// <param name="schemaService">The schema service used to source the highlighting keywords.</param>
	public SyntaxHighlighting(ColorScheme scheme, ITRXGameFlowSchemaService schemaService)
		: base("TRX Rules")
	{
		_scheme = scheme;
		_schemaService = schemaService;
	}

	/// <inheritdoc/>
	protected override IEnumerable<RegexHighlightingRule> BuildRules()
	{
		var patterns = new Patterns(_schemaService);
		var rules = new List<RegexHighlightingRule>();

		(string regex, HighlightingObject scheme, RegexOptions options)[] ruleDescriptors =
		[
			(patterns.Comments, _scheme.Comments, RegexOptions.None),
			(patterns.Collections, _scheme.Collections, RegexOptions.IgnoreCase),
			(patterns.Properties, _scheme.Properties, RegexOptions.IgnoreCase),
			(patterns.Constants, _scheme.Constants, RegexOptions.IgnoreCase),
			(patterns.Values, _scheme.Values, RegexOptions.IgnoreCase),
			(patterns.Strings, _scheme.Strings, RegexOptions.None)
		];

		foreach ((string regex, HighlightingObject scheme, RegexOptions options) in ruleDescriptors)
		{
			// Skip empty patterns: an empty regex would match at every position and override
			// the baseline colors (for example when the schema produced no keywords).
			if (string.IsNullOrEmpty(regex))
				continue;

			rules.Add(new(new Regex(regex, options), Style(scheme)));
		}

		return rules;
	}

	private static RegexHighlightingStyle Style(HighlightingObject scheme)
		=> new(scheme.HtmlColor, scheme.IsBold, scheme.IsItalic);
}
