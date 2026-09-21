namespace TombLib.Scripting.ClassicScript.Resources;

/// <summary>
/// Defines default values used by Classic Script editor configuration objects.
/// </summary>
public static class ConfigurationDefaults
{
	/// <summary>
	/// Gets the default file name used to persist Classic Script editor configuration.
	/// </summary>
	public const string ConfigurationFileName = "ClassicScriptConfiguration.xml";

	/// <summary>
	/// Gets the default state of the section separator display.
	/// </summary>
	public const bool ShowSectionSeparators = true;

	/// <summary>
	/// Gets the default state of the space before the equals sign when formatting commands.
	/// </summary>
	public const bool SpaceBeforeEquals = false;

	/// <summary>
	/// Gets the default state of the space after the equals sign when formatting commands.
	/// </summary>
	public const bool SpaceAfterEquals = true;

	/// <summary>
	/// Gets the default state of the space before a comma when formatting command arguments.
	/// </summary>
	public const bool SpaceBeforeComma = false;

	/// <summary>
	/// Gets the default state of the space after a comma when formatting command arguments.
	/// </summary>
	public const bool SpaceAfterComma = true;

	/// <summary>
	/// Gets the default state of collapsing multiple spaces into one when formatting commands.
	/// </summary>
	public const bool CollapseMultipleSpaces = true;
}
