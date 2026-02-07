using System;
using System.IO;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.Level.Rename;

public sealed class LevelRenameService : ILevelRenameService
{
	public LevelRenameResult RenameLevel(ILevelProject level, IGameProject project, LevelRenameOptions options)
	{
		string? newName = ValidateName(options.NewName);
		if (newName is null)
			throw new ArgumentException("You must enter a valid name for the level.");

		bool renameDirectory = options.RenameDirectory;
		bool renameScriptEntry = options.RenameScriptEntry;
		string oldName = level.Name;

		if (newName == level.Name)
		{
			// If the name hasn't changed, but the directory name is different and the user wants to rename it
			if (Path.GetFileName(level.DirectoryPath) != newName && renameDirectory)
			{
				string newDirectory = Path.Combine(Path.GetDirectoryName(level.DirectoryPath) ?? string.Empty, newName);

				if (Directory.Exists(newDirectory))
					throw new ArgumentException("A directory with the same name already exists in the parent directory.");

				level.Rename(newName, true);

				return new LevelRenameResult
				{
					Success = true,
					ChangesMade = true,
					ScriptRenameNeeded = false
				};
			}

			// No changes needed
			return new LevelRenameResult
			{
				Success = true,
				ChangesMade = false,
				ScriptRenameNeeded = false
			};
		}

		// Name changed
		string targetDirectory = Path.Combine(Path.GetDirectoryName(level.DirectoryPath) ?? string.Empty, newName);

		if (renameDirectory && Directory.Exists(targetDirectory) && !targetDirectory.Equals(level.DirectoryPath, StringComparison.OrdinalIgnoreCase))
			throw new ArgumentException("A directory with the same name already exists in the parent directory.");

		level.Rename(newName, renameDirectory);

		return new LevelRenameResult
		{
			Success = true,
			ChangesMade = true,
			ScriptRenameNeeded = renameScriptEntry,
			OldName = oldName,
			NewName = newName
		};
	}

	public string? ValidateName(string newName)
	{
		string sanitized = PathHelper.RemoveIllegalPathSymbols(newName.Trim());
		sanitized = LevelNameHelper.RemoveIllegalNameSymbols(sanitized);

		return string.IsNullOrWhiteSpace(sanitized) ? null : sanitized;
	}

	public LevelRenameState GetInitialRenameState(
		ILevelProject level,
		IGameProject project,
		Func<string, bool> isScriptDefined,
		Func<string, bool> isStringDefined)
	{
		bool isExternal = level.IsExternal(project.LevelsDirectoryPath);
		string directoryRenameText = isExternal
			? "Can't rename external level folders"
			: "Rename level folder as well (Recommended)";

		var state = new LevelRenameState
		{
			CanRenameDirectory = !isExternal,
			ShouldRenameDirectory = true,
			DirectoryRenameText = directoryRenameText,
			CanRenameScript = true,
			ShouldRenameScript = true,
			ScriptRenameText = "Rename script entry as well (Recommended)",
			ShowScriptError = false,
			ShowLanguageError = false
		};

		// Handle TombEngine - only language entries
		if (project.GameVersion == TRVersion.Game.TombEngine)
		{
			state.ScriptRenameText = "Rename language entry as well (Recommended)";

			if (!isStringDefined(level.Name))
			{
				state.CanRenameScript = false;
				state.ShouldRenameScript = false;
				state.ShowLanguageError = true;
			}

			return state;
		}

		// Handle TR1, TR2X, TR2, TR3 - only script entries
		if (project.GameVersion is TRVersion.Game.TR1 or TRVersion.Game.TR2X or TRVersion.Game.TR2 or TRVersion.Game.TR3)
		{
			if (!isScriptDefined(level.Name))
			{
				state.CanRenameScript = false;
				state.ShouldRenameScript = false;
				state.ShowScriptError = true;
			}

			return state;
		}

		// Handle TR4/TRNG - both script and language entries
		bool scriptDefined = isScriptDefined(level.Name);
		bool stringDefined = isStringDefined(level.Name);

		if (!scriptDefined || !stringDefined)
		{
			state.CanRenameScript = false;
			state.ShouldRenameScript = false;
			state.ShowScriptError = !scriptDefined;
			state.ShowLanguageError = !stringDefined;
		}

		return state;
	}

	public LevelRenameState GetRenameStateForText(
		ILevelProject level,
		IGameProject project,
		string currentText,
		bool hasScriptErrors,
		bool hasLanguageErrors)
	{
		string textBoxContent = PathHelper.RemoveIllegalPathSymbols(currentText.Trim());
		textBoxContent = LevelNameHelper.RemoveIllegalNameSymbols(textBoxContent);

		bool isExternal = level.IsExternal(project.LevelsDirectoryPath);
		string directoryName = Path.GetFileName(level.DirectoryPath);
		string directoryRenameText = isExternal
			? "Can't rename external level folders"
			: "Rename level folder as well (Recommended)";
		string scriptRenameText = GetScriptRenameText(project);

		// If the name hasn't changed, but the level folder name is different
		if (textBoxContent == level.Name && directoryName != textBoxContent)
		{
			return new LevelRenameState
			{
				CanRenameDirectory = !isExternal,
				ShouldRenameDirectory = !isExternal,
				DirectoryRenameText = directoryRenameText,
				CanRenameScript = false,
				ShouldRenameScript = false,
				ScriptRenameText = scriptRenameText,
				ShowScriptError = hasScriptErrors,
				ShowLanguageError = hasLanguageErrors
			};
		}

		// If the name changed, but the level folder name is the same
		if (textBoxContent != level.Name && directoryName == textBoxContent)
		{
			bool canRenameScript = !hasScriptErrors && !hasLanguageErrors;
			return new LevelRenameState
			{
				CanRenameDirectory = false,
				ShouldRenameDirectory = false,
				DirectoryRenameText = directoryRenameText,
				CanRenameScript = canRenameScript,
				ShouldRenameScript = canRenameScript,
				ScriptRenameText = scriptRenameText,
				ShowScriptError = hasScriptErrors,
				ShowLanguageError = hasLanguageErrors
			};
		}

		// If the name hasn't changed and the level folder name is the same
		if (textBoxContent == level.Name)
		{
			return new LevelRenameState
			{
				CanRenameDirectory = false,
				ShouldRenameDirectory = false,
				DirectoryRenameText = directoryRenameText,
				CanRenameScript = false,
				ShouldRenameScript = false,
				ScriptRenameText = scriptRenameText,
				ShowScriptError = hasScriptErrors,
				ShowLanguageError = hasLanguageErrors
			};
		}

		// Basically every other scenario
		bool canRename = !hasScriptErrors && !hasLanguageErrors;
		return new LevelRenameState
		{
			CanRenameDirectory = !isExternal,
			ShouldRenameDirectory = !isExternal,
			DirectoryRenameText = directoryRenameText,
			CanRenameScript = canRename,
			ShouldRenameScript = canRename,
			ScriptRenameText = scriptRenameText,
			ShowScriptError = hasScriptErrors,
			ShowLanguageError = hasLanguageErrors
		};
	}

	public bool CanApply(string currentText)
	{
		string textBoxContent = PathHelper.RemoveIllegalPathSymbols(currentText.Trim());
		textBoxContent = LevelNameHelper.RemoveIllegalNameSymbols(textBoxContent);
		return !string.IsNullOrWhiteSpace(textBoxContent);
	}

	private static string GetScriptRenameText(IGameProject project)
	{
		return project.GameVersion == TRVersion.Game.TombEngine
			? "Rename language entry as well (Recommended)"
			: "Rename script entry as well (Recommended)";
	}
}
