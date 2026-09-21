using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ScriptingStudio.Services;

/// <summary>
/// Provides automation operations for the scripting workspace, including script generation,
/// level and string management, and build integration.
/// </summary>
public interface IStudioWorkspaceAutomationProvider
{
	/// <summary>
	/// Handles an IDE event that affects the workspace.
	/// </summary>
	void HandleIDEEvent(IIDEEvent ideEvent);

	/// <summary>
	/// Appends a generated script result to the workspace.
	/// </summary>
	void AppendScript(ScriptGenerationResult result)
	{ }

	/// <summary>
	/// Adds a new level string entry with the specified name.
	/// </summary>
	void AddLevelString(string levelName)
	{ }

	/// <summary>
	/// Adds a plugin entry string to the workspace.
	/// </summary>
	void AddPluginEntry(string pluginString)
	{ }

	/// <summary>
	/// Adds an NG (next generation) string entry to the workspace.
	/// </summary>
	void AddNgString(string ngString)
	{ }

	/// <summary>
	/// Determines whether a script is already defined for the specified level name.
	/// </summary>
	bool IsScriptDefined(string levelName) => false;

	/// <summary>
	/// Determines whether a string value is already defined.
	/// </summary>
	bool IsStringDefined(string value) => false;

	/// <summary>
	/// Renames a level from the old name to the new name across the workspace.
	/// </summary>
	void RenameLevel(string oldName, string newName)
	{ }

	/// <summary>
	/// Reloads syntax highlighting settings for all open editors.
	/// </summary>
	void ReloadSyntaxHighlighting()
	{ }

	/// <summary>
	/// Handles cleanup and state saving when the program is closing.
	/// </summary>
	void HandleProgramClosing()
	{ }

	/// <summary>
	/// Builds the current project. Not all scripting languages require a build step;
	/// the default implementation is a no-op.
	/// </summary>
	void Build()
	{ }

	/// <summary>
	/// Opens the documentation for the current scripting language.
	/// </summary>
	void ShowDocumentation()
	{ }
}
