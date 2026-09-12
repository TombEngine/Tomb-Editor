using Nickelony.IDEKit.AvalonEdit.Extras.TextMate.Highlighting;
using System;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Lua.Resources;

namespace TombLib.Tests;

[TestClass]
public class LuaThemeConfigurationTests
{
	[TestMethod]
	public void DefaultConfiguration_UsesBundledSharpLuaClassicTheme()
	{
		var config = new LuaEditorConfiguration();

		Assert.AreEqual("SharpLua Classic", config.SelectedThemeName);
		Assert.AreEqual("SharpLua Classic", config.Theme.Name);
		Assert.AreEqual("#2D2D2D", config.Theme.Background);
		Assert.AreEqual("#CCCCCC", config.Theme.Foreground);
		Assert.IsTrue(config.Theme.TextMateTheme.Rules.Count > 0);
	}

	[TestMethod]
	public void LegacyVs15Alias_MapsToVsCodeDarkTheme()
	{
		var config = new LuaEditorConfiguration
		{
			SelectedThemeName = "VS15"
		};

		Assert.AreEqual("VS15", config.SelectedThemeName);
		Assert.AreEqual("VSCode Dark+", config.Theme.Name);
	}

	[TestMethod]
	public void SwitchingTheme_LoadsBundledPreset()
	{
		var config = new LuaEditorConfiguration
		{
			SelectedThemeName = "Visual Studio 15"
		};

		Assert.AreEqual("Visual Studio 15", config.SelectedThemeName);
		Assert.AreEqual("Visual Studio 2015", config.Theme.Name);
		Assert.AreEqual("#1E1E1E", config.Theme.Background);
		Assert.AreEqual("#DCDCDC", config.Theme.Foreground);
	}

	[TestMethod]
	public void Repository_ExposesBundledThemes()
	{
		string[] themeNames = LuaThemeRepository.GetAvailableThemes().Select(theme => theme.Name).ToArray();

		CollectionAssert.Contains(themeNames, "SharpLua Classic");
		CollectionAssert.Contains(themeNames, "Tomorrow");
		CollectionAssert.Contains(themeNames, "Tomorrow Night");
		CollectionAssert.Contains(themeNames, "VSCode Light+");
		CollectionAssert.Contains(themeNames, "NG_CENTER");
	}

	[TestMethod]
	public void SharpLuaClassicTheme_UsesEnhancedLegacyPalette()
	{
		var config = new LuaEditorConfiguration
		{
			SelectedThemeName = "SharpLua"
		};

		TextMateTokenThemeRule? classRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "entity.name.class, support.class, support.type, support.variable, variable.language.self", StringComparison.Ordinal));
		TextMateTokenThemeRule? attributeRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "entity.other.attribute, support.type.property-name", StringComparison.Ordinal));
		TextMateTokenThemeRule? parameterRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "variable.parameter, variable.other.object", StringComparison.Ordinal));

		Assert.AreEqual("SharpLua", config.SelectedThemeName);
		Assert.AreEqual("#2D2D2D", config.Theme.Background);
		Assert.AreEqual("#CCCCCC", config.Theme.Foreground);
		Assert.AreEqual("#66CCCC", config.Theme.SemanticColors.Type);
		Assert.AreEqual("#E6C8FF", config.Theme.SemanticColors.Variable);
		Assert.AreEqual("#D7B8FF", config.Theme.SemanticColors.Property);
		Assert.AreNotEqual(config.Theme.SemanticColors.Variable, config.Theme.SemanticColors.Property);
		Assert.AreEqual("#66CCCC", classRule?.Foreground);
		Assert.AreEqual("#D7B8FF", attributeRule?.Foreground);
		Assert.AreEqual("#E6C8FF", parameterRule?.Foreground);
	}

	[TestMethod]
	public void VsCodeDarkTheme_SeparatesLanguageConstantsFromNumericConstants()
	{
		var config = new LuaEditorConfiguration
		{
			SelectedThemeName = "VSCode Dark+"
		};

		TextMateTokenThemeRule? numericRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "constant.numeric", StringComparison.Ordinal));
		TextMateTokenThemeRule? languageRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "constant.language", StringComparison.Ordinal));

		Assert.IsNotNull(numericRule);
		Assert.IsNotNull(languageRule);
		Assert.AreNotEqual(numericRule.Foreground, languageRule.Foreground);
	}

	[TestMethod]
	public void VsCodeDarkTheme_UsesFileAccentForEscapeSequences()
	{
		var config = new LuaEditorConfiguration
		{
			SelectedThemeName = "VSCode Dark+"
		};

		TextMateTokenThemeRule? numericRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "constant.numeric", StringComparison.Ordinal));
		TextMateTokenThemeRule? escapeRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "constant.character.escape", System.StringComparison.Ordinal));

		Assert.IsNotNull(numericRule);
		Assert.IsNotNull(escapeRule);
		Assert.AreNotEqual(numericRule.Foreground, escapeRule.Foreground);
		Assert.AreEqual(config.Theme.SemanticColors.File, escapeRule.Foreground);
	}

	[TestMethod]
	public void TomorrowNightTheme_UsesCanonicalPalette()
	{
		var config = new LuaEditorConfiguration
		{
			SelectedThemeName = "TomorrowNight"
		};

		TextMateTokenThemeRule? stringRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "string", StringComparison.Ordinal));
		TextMateTokenThemeRule? numericRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "constant.numeric", StringComparison.Ordinal));
		TextMateTokenThemeRule? languageRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "constant.language", StringComparison.Ordinal));
		TextMateTokenThemeRule? functionRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "entity.name.function, support.function, support.function.library, support.function.any-method", StringComparison.Ordinal));
		TextMateTokenThemeRule? keywordRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "keyword, storage", StringComparison.Ordinal));

		Assert.AreEqual("TomorrowNight", config.SelectedThemeName);
		Assert.AreEqual("#1D1F21", config.Theme.Background);
		Assert.AreEqual("#C5C8C6", config.Theme.Foreground);
		Assert.AreEqual("#81A2BE", config.Theme.SemanticColors.Method);
		Assert.AreEqual("#CC6666", config.Theme.SemanticColors.Variable);
		Assert.AreEqual("#F0C674", config.Theme.SemanticColors.Type);
		Assert.AreEqual("#B5BD68", stringRule?.Foreground);
		Assert.AreEqual("#DE935F", numericRule?.Foreground);
		Assert.AreEqual("#DE935F", languageRule?.Foreground);
		Assert.AreEqual("#81A2BE", functionRule?.Foreground);
		Assert.AreEqual("#B294BB", keywordRule?.Foreground);
	}

	[TestMethod]
	public void TomorrowTheme_UsesCanonicalPalette()
	{
		var config = new LuaEditorConfiguration
		{
			SelectedThemeName = "Tomorrow Light"
		};

		TextMateTokenThemeRule? stringRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "string", System.StringComparison.Ordinal));
		TextMateTokenThemeRule? numericRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "constant.numeric", System.StringComparison.Ordinal));
		TextMateTokenThemeRule? languageRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "constant.language", System.StringComparison.Ordinal));
		TextMateTokenThemeRule? functionRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "entity.name.function, support.function, support.function.library, support.function.any-method", System.StringComparison.Ordinal));
		TextMateTokenThemeRule? keywordRule = config.Theme.TextMateTheme.Rules.FirstOrDefault(rule => string.Equals(rule.Scope, "keyword, storage", System.StringComparison.Ordinal));

		Assert.AreEqual("Tomorrow Light", config.SelectedThemeName);
		Assert.AreEqual("#FFFFFF", config.Theme.Background);
		Assert.AreEqual("#4D4D4C", config.Theme.Foreground);
		Assert.AreEqual("#4271AE", config.Theme.SemanticColors.Method);
		Assert.AreEqual("#C82829", config.Theme.SemanticColors.Variable);
		Assert.AreEqual("#C99E00", config.Theme.SemanticColors.Type);
		Assert.AreEqual("#718C00", stringRule?.Foreground);
		Assert.AreEqual("#F5871F", numericRule?.Foreground);
		Assert.AreEqual("#F5871F", languageRule?.Foreground);
		Assert.AreEqual("#4271AE", functionRule?.Foreground);
		Assert.AreEqual("#8959A8", keywordRule?.Foreground);
	}
}
