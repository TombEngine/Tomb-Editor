using NLog;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using TombLib.Scripting.Lua.Themes;
using TombLib.Scripting.UI.Resources;
using static TombLib.WPF.BrushHelpers;

namespace TombLib.Scripting.Lua.Resources;

/// <summary>
/// Builds the frozen brush palette used by the Lua editor from a resolved theme definition.
/// </summary>
internal static class LuaEditorColorPalette
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	/// <summary>
	/// Creates the editor brush set for the supplied Lua theme.
	/// </summary>
	/// <param name="theme">The theme to materialize into brushes.</param>
	/// <returns>A frozen brush set ready for use by the editor UI.</returns>
	public static LuaThemeBrushSet Create(LuaTheme theme)
	{
		LuaTheme effectiveTheme = theme.Normalize(ConfigurationDefaults.SelectedThemeName);
		LuaThemeSemanticColors semanticColors = effectiveTheme.SemanticColors;

		var brushes = new Dictionary<LuaThemeBrushRole, SolidColorBrush>
		{
			[LuaThemeBrushRole.EditorBackground] = CreateBrush(effectiveTheme.Background, LuaBuiltInThemes.DefaultBackground),
			[LuaThemeBrushRole.EditorForeground] = CreateBrush(effectiveTheme.Foreground, LuaBuiltInThemes.DefaultForeground),
			[LuaThemeBrushRole.MutedText] = CreateBrush(semanticColors.MutedText, LuaBuiltInThemes.DefaultMutedText),
			[LuaThemeBrushRole.Misc] = CreateBrush(semanticColors.Misc, LuaBuiltInThemes.DefaultMisc),
			[LuaThemeBrushRole.Method] = CreateBrush(semanticColors.Method, LuaBuiltInThemes.DefaultMethod),
			[LuaThemeBrushRole.Variable] = CreateBrush(semanticColors.Variable, LuaBuiltInThemes.DefaultVariable),
			[LuaThemeBrushRole.Property] = CreateBrush(semanticColors.Property, LuaBuiltInThemes.DefaultProperty),
			[LuaThemeBrushRole.Type] = CreateBrush(semanticColors.Type, LuaBuiltInThemes.DefaultType),
			[LuaThemeBrushRole.Keyword] = CreateBrush(semanticColors.Keyword, LuaBuiltInThemes.DefaultKeyword),
			[LuaThemeBrushRole.LanguageConstant] = CreateBrush(semanticColors.LanguageConstant, LuaBuiltInThemes.DefaultLanguageConstant),
			[LuaThemeBrushRole.Constant] = CreateBrush(semanticColors.Constant, LuaBuiltInThemes.DefaultConstant),
			[LuaThemeBrushRole.File] = CreateBrush(semanticColors.File, LuaBuiltInThemes.DefaultFile),
			[LuaThemeBrushRole.SignatureParamDoc] = CreateBrush(semanticColors.SignatureParameterDocumentation, LuaBuiltInThemes.DefaultSignatureParameterDocumentation),
			[LuaThemeBrushRole.SignatureActiveParam] = CreateBrush(semanticColors.SignatureActiveParameter, LuaBuiltInThemes.DefaultSignatureActiveParameter),
			[LuaThemeBrushRole.SignatureForeground] = CreateBrush(semanticColors.SignatureText, ColorToString(TextEditorColorPalette.ToolTipForeground.Color))
		};

		return new LuaThemeBrushSet(effectiveTheme.Name, brushes);
	}

	private static SolidColorBrush CreateBrush(string colorValue, string fallbackColorValue)
	{
		try
		{
			string effectiveColorValue = string.IsNullOrWhiteSpace(colorValue)
				? fallbackColorValue
				: colorValue;

			return CreateFrozenBrush(effectiveColorValue);
		}
		catch (Exception exception)
		{
			Log.Warn(exception, "Invalid color value '{ColorValue}' in Lua theme; using the fallback color.", colorValue);
			return CreateFrozenBrush(fallbackColorValue);
		}
	}

	private static string ColorToString(Color color)
		=> $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
