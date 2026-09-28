using System;
using System.IO;

namespace TombLib.Scripting.UI.Resources;

/// <summary>
/// Resolves the installation-relative paths shared by all scripting editors: the text-editor
/// configuration and color-scheme directory tree. This type performs pure path composition and
/// carries no UI side effects or platform-specific behavior.
/// </summary>
public sealed class ScriptingPaths
{
	/// <summary>
	/// Gets the default path layout rooted at the application base directory.
	/// </summary>
	public static ScriptingPaths Default { get; } = new(AppContext.BaseDirectory);

	private readonly string _programDirectory;

	/// <summary>
	/// Initializes a new instance rooted at the supplied program directory.
	/// </summary>
	/// <param name="programDirectory">The application root directory.</param>
	public ScriptingPaths(string programDirectory) => _programDirectory = programDirectory;

	/// <summary>
	/// Gets the application root directory.
	/// </summary>
	public string ProgramDirectory => _programDirectory;

	/// <summary>
	/// Gets the top-level configuration directory.
	/// </summary>
	public string ConfigsDirectory => Path.Combine(_programDirectory, "Configs");

	/// <summary>
	/// Gets the directory that stores text editor configurations.
	/// </summary>
	public string TextEditorConfigsDirectory => Path.Combine(ConfigsDirectory, "TextEditors");

	/// <summary>
	/// Gets the directory that stores text editor themes.
	/// </summary>
	public string TextEditorThemesDirectory => Path.Combine(TextEditorConfigsDirectory, "Themes");

	/// <summary>
	/// Gets the directory that stores text editor color schemes.
	/// </summary>
	public string ColorSchemesDirectory => Path.Combine(TextEditorConfigsDirectory, "ColorSchemes");

	/// <summary>
	/// Gets the directory that stores ClassicScript color schemes.
	/// </summary>
	public string ClassicScriptColorConfigsDirectory => Path.Combine(ColorSchemesDirectory, "ClassicScript");

	/// <summary>
	/// Gets the directory that stores GameFlow color schemes.
	/// </summary>
	public string GameFlowColorConfigsDirectory => Path.Combine(ColorSchemesDirectory, "GameFlowScript");

	/// <summary>
	/// Gets the directory that stores TRX color schemes.
	/// </summary>
	public string TRXColorConfigsDirectory => Path.Combine(ColorSchemesDirectory, "TRX");

	/// <summary>
	/// Gets the directory that stores Lua themes.
	/// </summary>
	public string LuaThemeConfigsDirectory => Path.Combine(TextEditorThemesDirectory, "Lua");
}
