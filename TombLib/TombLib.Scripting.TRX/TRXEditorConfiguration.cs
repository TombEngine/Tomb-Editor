using System.IO;
using TombLib.Scripting.TRX.Highlighting;
using TombLib.Scripting.TRX.Resources;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Resources;
using TombLib.Utils;

namespace TombLib.Scripting.TRX;

/// <summary>
/// Configuration for the TRX editor, including its color scheme.
/// </summary>
public sealed class TRXEditorConfiguration : ColorSchemeConfigBase<ColorScheme>
{
	/// <summary>
	/// Gets the default path used to persist this configuration.
	/// </summary>
	public override string DefaultPath { get; }

	// Properties

	/// <summary>
	/// Gets or sets whether commas are added automatically after closing braces and brackets.
	/// </summary>
	public bool AutoAddCommas { get; set; } = ConfigurationDefaults.AutoAddCommas;

	// Color scheme

	/// <inheritdoc/>
	protected override string GetColorSchemeFilePath(string colorSchemeName)
		=> Path.Combine(ScriptingPaths.Default.TRXColorConfigsDirectory, colorSchemeName + ScriptingDefaults.ColorSchemeFileExtension);

	/// <inheritdoc/>
	protected override ColorScheme ReadColorSchemeFile(string colorSchemeFilePath)
		=> JsonUtils.ReadJsonFile<ColorScheme>(colorSchemeFilePath);

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXEditorConfiguration"/> class.
	/// </summary>
	public TRXEditorConfiguration()
	{
		DefaultPath = Path.Combine(ScriptingPaths.Default.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);

		AutoCloseParentheses = false;

		SelectedColorSchemeName = ScriptingDefaults.SelectedColorSchemeName;
	}
}
