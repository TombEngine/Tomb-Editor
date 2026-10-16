using Nickelony.IDEKit.Core.Themes;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TombLib.Scripting.Lua.Themes;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.Lua.Resources;

/// <summary>
/// Loads and resolves Lua editor themes from disk, falling back to the built-in default theme when needed.
/// </summary>
public static class LuaThemeRepository
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private static readonly Lazy<ThemeCatalog<LuaTheme>> Catalog = new(LoadCatalog);

	/// <summary>
	/// Gets all available Lua themes known to the repository.
	/// </summary>
	/// <returns>The ordered list of available themes.</returns>
	public static IReadOnlyList<LuaTheme> GetAvailableThemes() => Catalog.Value.Themes;

	/// <summary>
	/// Gets the theme matching the supplied name or alias, or the default theme when no match exists.
	/// </summary>
	/// <param name="themeName">The theme name or alias to resolve.</param>
	/// <returns>The resolved theme.</returns>
	public static LuaTheme GetTheme(string themeName)
		=> Catalog.Value.GetTheme(themeName);

	private static ThemeCatalog<LuaTheme> LoadCatalog()
	{
		var themes = new List<LuaTheme>();
		string themesDirectory = ScriptingPaths.Default.LuaThemeConfigsDirectory;

		var serializerOptions = new JsonSerializerOptions
		{
			AllowTrailingCommas = true,
			PropertyNameCaseInsensitive = true,
			ReadCommentHandling = JsonCommentHandling.Skip
		};

		if (Directory.Exists(themesDirectory))
		{
			foreach (string filePath in Directory.GetFiles(themesDirectory, "*.json", SearchOption.TopDirectoryOnly))
			{
				try
				{
					string fileContent = File.ReadAllText(filePath);
					LuaTheme? theme = JsonSerializer.Deserialize<LuaTheme>(fileContent, serializerOptions);

					if (theme is not null)
						themes.Add(theme.Normalize(Path.GetFileNameWithoutExtension(filePath)));
				}
				catch (Exception exception)
				{
					Log.Warn(exception, "Failed to load Lua theme '{FilePath}'.", filePath);
				}
			}
		}

		if (themes.Count == 0)
			themes.Add(LuaBuiltInThemes.CreateDefaultTheme().Normalize(ConfigurationDefaults.SelectedThemeName));

		return new ThemeCatalog<LuaTheme>(
			themes,
			new ThemeCatalogOptions<LuaTheme>(
				GetName: static theme => theme.Name,
				DefaultThemeName: ConfigurationDefaults.SelectedThemeName,
				GetAliases: static theme => theme.Aliases));
	}
}
