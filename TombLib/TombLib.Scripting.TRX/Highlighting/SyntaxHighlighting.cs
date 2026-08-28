using ICSharpCode.AvalonEdit.Highlighting;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.TRX.Services;
using TombLib.Scripting.UI.Highlighting;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.TRX.Highlighting;

/// <summary>
/// Provides the TRX highlighting definition built from the active color scheme and GameFlow schema.
/// </summary>
public sealed class SyntaxHighlighting : IHighlightingDefinition
{
	private readonly ColorScheme _scheme;
	private readonly ITRXGameFlowSchemaService _schemaService;

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="SyntaxHighlighting"/> class.
	/// </summary>
	/// <param name="scheme">The color scheme used for the highlighting rules.</param>
	/// <param name="schemaService">The schema service used to source the highlighting keywords.</param>
	public SyntaxHighlighting(ColorScheme scheme, ITRXGameFlowSchemaService schemaService)
	{
		_scheme = scheme;
		_schemaService = schemaService;
	}

	// Rules

	private HighlightingRuleSet? _cachedRuleSet;

	/// <summary>
	/// Gets the main highlighting rule set for the TRX language.
	/// </summary>
	public HighlightingRuleSet MainRuleSet
	{
		get
		{
			_cachedRuleSet ??= BuildRuleSet();
			return _cachedRuleSet;
		}
	}

	private HighlightingRuleSet BuildRuleSet()
	{
		var patterns = new Patterns(_schemaService);
		var ruleSet = new HighlightingRuleSet();

		(string regex, HighlightingObject scheme, RegexOptions options)[] rules =
		[
			(patterns.Comments, _scheme.Comments, RegexOptions.None),
			(patterns.Collections, _scheme.Collections, RegexOptions.IgnoreCase),
			(patterns.Properties, _scheme.Properties, RegexOptions.IgnoreCase),
			(patterns.Constants, _scheme.Constants, RegexOptions.IgnoreCase),
			(patterns.Values, _scheme.Values, RegexOptions.IgnoreCase),
			(patterns.Strings, _scheme.Strings, RegexOptions.None)
		];

		foreach ((string regex, HighlightingObject scheme, RegexOptions options) in rules)
		{
			// Skip empty patterns: an empty regex would match at every position and override
			// the baseline colors (for example when the schema produced no keywords).
			if (string.IsNullOrEmpty(regex))
				continue;

			ruleSet.Rules.Add(new HighlightingRule
			{
				Regex = new Regex(regex, options),
				Color = CreateColor(scheme)
			});
		}

		ruleSet.Name = "TRX Rules";
		return ruleSet;
	}

	private static HighlightingColor CreateColor(HighlightingObject scheme) => new()
	{
		Foreground = new SimpleHighlightingBrush(ScriptingColorParser.ParseColorOrDefault(scheme.HtmlColor, ScriptingColorParser.DefaultHighlightingColor)),
		FontWeight = scheme.IsBold ? FontWeights.Bold : FontWeights.Normal,
		FontStyle = scheme.IsItalic ? FontStyles.Italic : FontStyles.Normal
	};

	// Other

	/// <summary>
	/// Gets the name of the TRX highlighting definition.
	/// </summary>
	public string Name => "TRX Rules";

	/// <inheritdoc/>
	public IEnumerable<HighlightingColor> NamedHighlightingColors => [];

	/// <inheritdoc/>
	public IDictionary<string, string> Properties => new Dictionary<string, string>();

	/// <inheritdoc/>
	public HighlightingColor? GetNamedColor(string name) => null;

	/// <inheritdoc/>
	public HighlightingRuleSet? GetNamedRuleSet(string name)
		=> name == MainRuleSet.Name ? MainRuleSet : null;
}
