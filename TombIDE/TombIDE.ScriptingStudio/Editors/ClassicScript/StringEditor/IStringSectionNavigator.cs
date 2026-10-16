#nullable enable

namespace TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;

/// <summary>
/// Provides navigation between string sections in a Classic Script string editor.
/// </summary>
public interface IStringSectionNavigator
{
	/// <summary>
	/// Navigates to the previous string section.
	/// </summary>
	void GoToPreviousSection();

	/// <summary>
	/// Navigates to the next string section.
	/// </summary>
	void GoToNextSection();

	/// <summary>
	/// Gets the name of the current section, or <see langword="null"/> if no section is active.
	/// </summary>
	string? CurrentSectionName { get; }

	/// <summary>
	/// Clears the currently selected string.
	/// </summary>
	void ClearSelectedString();

	/// <summary>
	/// Removes the most recently added string from the selection.
	/// </summary>
	void RemoveLastString();
}
