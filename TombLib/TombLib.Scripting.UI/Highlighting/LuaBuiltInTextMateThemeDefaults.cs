using Nickelony.IDEKit.AvalonEdit.TextMate.Highlighting;
using System.Collections.Generic;

namespace TombLib.Scripting.UI.Highlighting;

/// <summary>
/// Provides the default colors used by the built-in Lua TextMate theme.
/// </summary>
public static class LuaBuiltInTextMateThemeDefaults
{
	/// <summary>
	/// The default background color.
	/// </summary>
	public const string DefaultBackground = "#2D2D2D";

	/// <summary>
	/// The default foreground color.
	/// </summary>
	public const string DefaultForeground = "#CCCCCC";

	/// <summary>
	/// The default color for muted text, such as comments and documentation.
	/// </summary>
	public const string DefaultMutedText = "#999999";

	/// <summary>
	/// The default color for miscellaneous tokens.
	/// </summary>
	public const string DefaultMisc = "#CCCCCC";

	/// <summary>
	/// The default color for method names.
	/// </summary>
	public const string DefaultMethod = "#FFCC66";

	/// <summary>
	/// The default color for variables.
	/// </summary>
	public const string DefaultVariable = "#E6C8FF";

	/// <summary>
	/// The default color for properties.
	/// </summary>
	public const string DefaultProperty = "#D7B8FF";

	/// <summary>
	/// The default color for types.
	/// </summary>
	public const string DefaultType = "#66CCCC";

	/// <summary>
	/// The default color for keywords.
	/// </summary>
	public const string DefaultKeyword = "#6699CC";

	/// <summary>
	/// The default color for language constants.
	/// </summary>
	public const string DefaultLanguageConstant = "#66CCCC";

	/// <summary>
	/// The default color for constants.
	/// </summary>
	public const string DefaultConstant = "#99CC99";

	/// <summary>
	/// The default color for file and string tokens.
	/// </summary>
	public const string DefaultFile = "#F99157";

	/// <summary>
	/// The default color for signature parameter documentation.
	/// </summary>
	public const string DefaultSignatureParameterDocumentation = "#999999";

	/// <summary>
	/// The default color for the active signature parameter.
	/// </summary>
	public const string DefaultSignatureActiveParameter = "#FFCC66";

	/// <summary>
	/// The default color for signature text.
	/// </summary>
	public const string DefaultSignatureText = "#CCCCCC";

	/// <summary>
	/// Creates the default TextMate token theme.
	/// </summary>
	/// <returns>The default token theme.</returns>
	public static TextMateTokenTheme CreateDefaultTextMateTheme()
	{
		return new TextMateTokenTheme
		{
			Rules = new List<TextMateTokenThemeRule>
			{
				new TextMateTokenThemeRule { Scope = "comment", Foreground = DefaultMutedText },
				new TextMateTokenThemeRule { Scope = "string", Foreground = DefaultFile },
				new TextMateTokenThemeRule { Scope = "constant.numeric", Foreground = DefaultConstant },
				new TextMateTokenThemeRule { Scope = "constant.character.escape", Foreground = DefaultFile },
				new TextMateTokenThemeRule { Scope = "constant.language", Foreground = DefaultLanguageConstant, FontStyle = "bold" },
				new TextMateTokenThemeRule { Scope = "keyword, storage", Foreground = DefaultKeyword, FontStyle = "bold" },
				new TextMateTokenThemeRule { Scope = "keyword.control.goto, keyword.control.return, keyword.control.break", Foreground = "#CC99CC" },
				new TextMateTokenThemeRule { Scope = "keyword.control.local", Foreground = DefaultFile, FontStyle = "bold" },
				new TextMateTokenThemeRule { Scope = "entity.name.class, support.class, support.type, support.variable, variable.language.self", Foreground = DefaultType, FontStyle = "bold" },
				new TextMateTokenThemeRule { Scope = "entity.other.attribute, support.type.property-name", Foreground = DefaultProperty },
				new TextMateTokenThemeRule { Scope = "variable.parameter, variable.other.object", Foreground = DefaultVariable },
				new TextMateTokenThemeRule { Scope = "entity.name.function, support.function, support.function.library, support.function.any-method", Foreground = DefaultMethod, FontStyle = "bold" },
				new TextMateTokenThemeRule { Scope = "keyword.operator, punctuation", Foreground = DefaultMisc }
			}
		};
	}
}
