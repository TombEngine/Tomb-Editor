namespace TombLib.Scripting.UI.Bases;

/// <summary>
/// Exposes the name of the currently selected color scheme on an editor configuration.
/// </summary>
public interface IColorSchemeConfig
{
	/// <summary>
	/// Gets or sets the name of the currently selected color scheme.
	/// </summary>
	string SelectedColorSchemeName { get; set; }
}
