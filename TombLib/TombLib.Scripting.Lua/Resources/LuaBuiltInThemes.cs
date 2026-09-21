using TombLib.Scripting.Lua.Themes;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Scripting.Lua.Resources;

/// <summary>
/// Defines the built-in Lua theme defaults used when no external theme data is available.
/// </summary>
internal static class LuaBuiltInThemes
{
	// Theme identity
	public const string DefaultThemeName = "SharpLua Classic";
	public const string DefaultThemeAlias = "SharpLua";

	// Base colors
	public const string DefaultBackground = LuaBuiltInTextMateThemeDefaults.DefaultBackground;
	public const string DefaultForeground = LuaBuiltInTextMateThemeDefaults.DefaultForeground;

	// Semantic colors
	public const string DefaultMutedText = LuaBuiltInTextMateThemeDefaults.DefaultMutedText;
	public const string DefaultMisc = LuaBuiltInTextMateThemeDefaults.DefaultMisc;
	public const string DefaultMethod = LuaBuiltInTextMateThemeDefaults.DefaultMethod;
	public const string DefaultVariable = LuaBuiltInTextMateThemeDefaults.DefaultVariable;
	public const string DefaultProperty = LuaBuiltInTextMateThemeDefaults.DefaultProperty;
	public const string DefaultType = LuaBuiltInTextMateThemeDefaults.DefaultType;
	public const string DefaultKeyword = LuaBuiltInTextMateThemeDefaults.DefaultKeyword;
	public const string DefaultLanguageConstant = LuaBuiltInTextMateThemeDefaults.DefaultLanguageConstant;
	public const string DefaultConstant = LuaBuiltInTextMateThemeDefaults.DefaultConstant;
	public const string DefaultFile = LuaBuiltInTextMateThemeDefaults.DefaultFile;
	public const string DefaultSignatureParameterDocumentation = LuaBuiltInTextMateThemeDefaults.DefaultSignatureParameterDocumentation;
	public const string DefaultSignatureActiveParameter = LuaBuiltInTextMateThemeDefaults.DefaultSignatureActiveParameter;
	public const string DefaultSignatureText = LuaBuiltInTextMateThemeDefaults.DefaultSignatureText;

	/// <summary>
	/// Creates the built-in fallback Lua theme.
	/// </summary>
	/// <returns>A fully populated default Lua theme instance.</returns>
	public static LuaTheme CreateDefaultTheme() => new LuaTheme
	{
		Name = DefaultThemeName,
		Aliases = [DefaultThemeAlias],
		Background = DefaultBackground,
		Foreground = DefaultForeground,

		SemanticColors = new()
		{
			MutedText = DefaultMutedText,
			Misc = DefaultMisc,
			Method = DefaultMethod,
			Variable = DefaultVariable,
			Property = DefaultProperty,
			Type = DefaultType,
			Keyword = DefaultKeyword,
			LanguageConstant = DefaultLanguageConstant,
			Constant = DefaultConstant,
			File = DefaultFile,
			SignatureParameterDocumentation = DefaultSignatureParameterDocumentation,
			SignatureActiveParameter = DefaultSignatureActiveParameter,
			SignatureText = DefaultSignatureText
		},

		TextMateTheme = LuaBuiltInTextMateThemeDefaults.CreateDefaultTextMateTheme()
	};
}
