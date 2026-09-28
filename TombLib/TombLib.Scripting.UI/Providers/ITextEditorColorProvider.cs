using System.Collections.Generic;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.UI.Providers;

/// <summary>
/// Provides the available color schemes or themes for a scripting editor, and reads or writes
/// the user's current selection through the language-specific editor configuration.
/// </summary>
public interface ITextEditorColorProvider
{
	/// <summary>
	/// Gets the names of all available color schemes or themes.
	/// </summary>
	/// <returns>The ordered list of available names.</returns>
	IReadOnlyList<string> GetAvailableNames();

	/// <summary>
	/// Gets the currently selected scheme or theme name from the supplied editor configuration.
	/// </summary>
	/// <param name="config">The editor configuration to read from.</param>
	/// <returns>The selected scheme or theme name.</returns>
	string GetSelectedName(TextEditorConfigBase config);

	/// <summary>
	/// Sets the selected scheme or theme name on the supplied editor configuration.
	/// </summary>
	/// <param name="config">The editor configuration to write to.</param>
	/// <param name="name">The scheme or theme name to select.</param>
	void SetSelectedName(TextEditorConfigBase config, string name);
}
