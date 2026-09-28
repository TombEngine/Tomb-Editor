using System.IO;
using System.Xml.Serialization;
using TombLib.Scripting.ClassicScript.Highlighting;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Resources;
using TombLib.Utils;

namespace TombLib.Scripting.ClassicScript;

/// <summary>
/// Configuration for the ClassicScript editor, including its color scheme.
/// </summary>
public sealed class ClassicScriptEditorConfiguration : ColorSchemeConfigBase<ColorScheme>
{
	/// <inheritdoc/>
	public override string DefaultPath { get; }

	// Properties

	/// <summary>
	/// Gets or sets whether section separator lines are rendered.
	/// </summary>
	public bool ShowSectionSeparators { get; set; } = ConfigurationDefaults.ShowSectionSeparators;

	/// <summary>
	/// Gets or sets whether a space is inserted before the equals sign when tidying.
	/// </summary>
	[XmlElement("Tidy_PreEqualSpace")]
	public bool SpaceBeforeEquals { get; set; } = ConfigurationDefaults.SpaceBeforeEquals;

	/// <summary>
	/// Gets or sets whether a space is inserted after the equals sign when tidying.
	/// </summary>
	[XmlElement("Tidy_PostEqualSpace")]
	public bool SpaceAfterEquals { get; set; } = ConfigurationDefaults.SpaceAfterEquals;

	/// <summary>
	/// Gets or sets whether a space is inserted before the comma when tidying.
	/// </summary>
	[XmlElement("Tidy_PreCommaSpace")]
	public bool SpaceBeforeComma { get; set; } = ConfigurationDefaults.SpaceBeforeComma;

	/// <summary>
	/// Gets or sets whether a space is inserted after the comma when tidying.
	/// </summary>
	[XmlElement("Tidy_PostCommaSpace")]
	public bool SpaceAfterComma { get; set; } = ConfigurationDefaults.SpaceAfterComma;

	/// <summary>
	/// Gets or sets whether multiple spaces are collapsed when tidying.
	/// </summary>
	[XmlElement("Tidy_ReduceSpaces")]
	public bool CollapseMultipleSpaces { get; set; } = ConfigurationDefaults.CollapseMultipleSpaces;

	// Color scheme

	/// <inheritdoc/>
	protected override string GetColorSchemeFilePath(string colorSchemeName)
		=> Path.Combine(ScriptingPaths.Default.ClassicScriptColorConfigsDirectory, colorSchemeName + ScriptingDefaults.ColorSchemeFileExtension);

	/// <inheritdoc/>
	protected override ColorScheme ReadColorSchemeFile(string colorSchemeFilePath)
		=> JsonUtils.ReadJsonFile<ColorScheme>(colorSchemeFilePath);

	// Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassicScriptEditorConfiguration"/> class.
	/// </summary>
	public ClassicScriptEditorConfiguration()
	{
		DefaultPath = Path.Combine(ScriptingPaths.Default.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);

		// These type of brackets aren't being used while writing in Classic Script, therefore auto closing should be disabled for them
		AutoCloseParentheses = false;
		AutoCloseBraces = false;

		SelectedColorSchemeName = ScriptingDefaults.SelectedColorSchemeName;
	}
}
