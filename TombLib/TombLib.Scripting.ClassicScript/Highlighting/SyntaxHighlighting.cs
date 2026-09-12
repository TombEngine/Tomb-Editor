using Nickelony.IDEKit.AvalonEdit.Highlighting;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Commands;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Scripting.ClassicScript.Highlighting;

/// <summary>
/// Provides the highlighting definition for the ClassicScript editor.
/// </summary>
public sealed class SyntaxHighlighting : RegexHighlightingDefinition
{
	private readonly ColorScheme _scheme;
	private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService = new();
	private readonly ClassicScriptCommandCatalogService _commandCatalogService = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="SyntaxHighlighting"/> class.
	/// </summary>
	/// <param name="scheme">The color scheme used for the highlighting rules.</param>
	public SyntaxHighlighting(ColorScheme scheme)
		: base("ClassicScript Rules", cacheVersion: () => ClassicScriptMnemonicCatalogService.CurrentSnapshotVersion)
	{
		_scheme = scheme;
	}

	/// <inheritdoc/>
	protected override IEnumerable<RegexHighlightingRule> BuildRules()
	{
		var rules = new List<RegexHighlightingRule>();

		/* Comments */
		rules.Add(new(new Regex(";.*$"), Style(_scheme.Comments)));

		/* Sections */
		if (_commandCatalogService.Sections.Count > 0)
			rules.Add(new(BuildWordBoundaryAlternation(@"\[\b({0})\b\]", _commandCatalogService.Sections, RegexOptions.IgnoreCase | RegexOptions.Compiled), Style(_scheme.Sections)));

		/* Standard commands */
		if (_commandCatalogService.OldCommands.Count > 0)
			rules.Add(new(BuildWordBoundaryAlternation(@"\b({0})\b\s*=", _commandCatalogService.OldCommands, RegexOptions.IgnoreCase | RegexOptions.Compiled), Style(_scheme.StandardCommands)));

		/* New commands */
		string[] newCommands = _commandCatalogService.NewCommands.Where(name => !name.StartsWith('#')).ToArray();

		if (newCommands.Length > 0)
			rules.Add(new(BuildWordBoundaryAlternation(@"\b({0})\b\s*=", newCommands, RegexOptions.IgnoreCase | RegexOptions.Compiled), Style(_scheme.NewCommands)));

		/* Next line keys */
		rules.Add(new(new Regex(">"), new RegexHighlightingStyle(_scheme.NewCommands.HtmlColor, IsBold: true)));

		/* Mnemonics */
		if (_mnemonicCatalogService.GetAllFlags().Count > 0)
			rules.Add(new(_mnemonicCatalogService.GetMnemonicRegex(), Style(_scheme.References)));

		/* Hex values */
		rules.Add(new(new Regex(@"\$[a-f0-9]*", RegexOptions.IgnoreCase), Style(_scheme.References)));

		/* Directives (#...) */
		rules.Add(new(new Regex(@"#(define|first_id|include)\s", RegexOptions.IgnoreCase), Style(_scheme.References)));

		/* Values */
		rules.Add(new(new Regex("\\d|\\w|\"|'|\\.|\\\\"), Style(_scheme.Values)));

		return rules;
	}

	private static RegexHighlightingStyle Style(HighlightingObject scheme)
		=> new(scheme.HtmlColor, scheme.IsBold, scheme.IsItalic);

	/// <summary>
	/// Builds a compiled regex from a word-boundary template with every alternative regex-escaped.
	/// </summary>
	private static Regex BuildWordBoundaryAlternation(string template, IEnumerable<string> names, RegexOptions options)
		=> new(string.Format(template, string.Join("|", names.Select(Regex.Escape))), options);
}
