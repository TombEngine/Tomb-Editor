namespace TombLib.Scripting.Lua.Resources;

/// <summary>
/// Defines default values used by Lua editor configuration objects.
/// </summary>
public static class ConfigurationDefaults
{
	/// <summary>
	/// Gets the default file name used to persist Lua editor configuration.
	/// </summary>
	public const string ConfigurationFileName = "LuaConfiguration.xml";

	/// <summary>
	/// Gets the default selected Lua theme name.
	/// </summary>
	public const string SelectedThemeName = LuaBuiltInThemes.DefaultThemeName;
}
