using System;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.Level.Rename;

/// <summary>
/// Provides functionality for renaming levels.
/// </summary>
public interface ILevelRenameService
{
	/// <summary>
	/// Renames a level with the specified options.
	/// </summary>
	/// <param name="level">The level to rename.</param>
	/// <param name="project">The project containing the level.</param>
	/// <param name="options">The rename options.</param>
	/// <returns>The result of the rename operation.</returns>
	LevelRenameResult RenameLevel(ILevelProject level, IGameProject project, LevelRenameOptions options);

	/// <summary>
	/// Validates the new level name.
	/// </summary>
	/// <param name="newName">The new name to validate.</param>
	/// <returns>The sanitized name, or null if invalid.</returns>
	string? ValidateName(string newName);

	/// <summary>
	/// Gets the initial rename state for a level based on the project settings.
	/// </summary>
	/// <param name="level">The level to get the state for.</param>
	/// <param name="project">The project containing the level.</param>
	/// <param name="isScriptDefined">Function to check if script is defined for the level name.</param>
	/// <param name="isStringDefined">Function to check if string is defined for the level name.</param>
	/// <returns>The initial rename state.</returns>
	LevelRenameState GetInitialRenameState(
		ILevelProject level,
		IGameProject project,
		Func<string, bool> isScriptDefined,
		Func<string, bool> isStringDefined);

	/// <summary>
	/// Gets the rename state based on the current text input.
	/// </summary>
	/// <param name="level">The level being renamed.</param>
	/// <param name="project">The project containing the level.</param>
	/// <param name="currentText">The current text in the name input.</param>
	/// <param name="hasScriptErrors">Whether there are script errors displayed.</param>
	/// <param name="hasLanguageErrors">Whether there are language errors displayed.</param>
	/// <returns>The updated rename state.</returns>
	LevelRenameState GetRenameStateForText(
		ILevelProject level,
		IGameProject project,
		string currentText,
		bool hasScriptErrors,
		bool hasLanguageErrors);

	/// <summary>
	/// Determines if the apply button should be enabled based on the current text.
	/// </summary>
	/// <param name="currentText">The current text in the name input.</param>
	/// <returns>True if the apply button should be enabled.</returns>
	bool CanApply(string currentText);
}
