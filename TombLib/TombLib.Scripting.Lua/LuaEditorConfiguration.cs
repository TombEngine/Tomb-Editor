using System.IO;
using System.Xml.Serialization;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.Lua.Themes;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.Lua;

/// <summary>
/// Stores user-configurable settings for the Lua editor.
/// </summary>
public sealed class LuaEditorConfiguration : TextEditorConfigBase
{
	/// <summary>
	/// Gets the default file path used to persist this configuration.
	/// </summary>
	public override string DefaultPath { get; }

	/// <summary>
	/// Gets or sets the selected Lua theme name. The name is stored as given and resolved to a theme
	/// (including legacy aliases) through <see cref="Theme"/>.
	/// </summary>
	public string SelectedThemeName { get; set; } = ConfigurationDefaults.SelectedThemeName;

	/// <summary>
	/// Gets the resolved theme object for the current selection.
	/// </summary>
	[XmlIgnore]
	public LuaTheme Theme => LuaThemeRepository.GetTheme(SelectedThemeName);

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaEditorConfiguration"/> class.
	/// </summary>
	public LuaEditorConfiguration()
	{
		DefaultPath = Path.Combine(ScriptingPaths.Default.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);
	}
}
