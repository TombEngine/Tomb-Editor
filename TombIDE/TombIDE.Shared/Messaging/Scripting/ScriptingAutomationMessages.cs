#nullable enable

using TombIDE.Shared.SharedClasses;

namespace TombIDE.Shared.Messaging.Scripting;

/// <summary>
/// Requests that script content be appended to the active scripting workspace.
/// </summary>
/// <param name="Result">The generated script payload.</param>
public sealed record ScriptingAppendScriptRequestedMessage(ScriptGenerationResult Result);

/// <summary>
/// Requests that a new level string be added to the active scripting workspace.
/// </summary>
/// <param name="LevelName">The level name to add.</param>
public sealed record ScriptingAddLevelStringRequestedMessage(string LevelName);

/// <summary>
/// Requests that a new plugin entry be added to the active scripting workspace.
/// </summary>
/// <param name="PluginString">The plugin entry to add.</param>
public sealed record ScriptingAddPluginEntryRequestedMessage(string PluginString);

/// <summary>
/// Requests that a new NG string be added to the active scripting workspace.
/// </summary>
/// <param name="NgString">The NG string to add.</param>
public sealed record ScriptingAddNgStringRequestedMessage(string NgString);

/// <summary>
/// Signals that the active scripting content changed outside the editor workflow.
/// </summary>
public sealed record ScriptingExternalContentChangedMessage();

/// <summary>
/// Requests that the active scripting workspace rename a level-related script surface.
/// </summary>
/// <param name="OldName">The existing level name.</param>
/// <param name="NewName">The replacement level name.</param>
public sealed record ScriptingRenameLevelRequestedMessage(string OldName, string NewName);

/// <summary>
/// Requests a syntax-highlighting reload for the active scripting workspace.
/// </summary>
public sealed record ScriptingReloadSyntaxHighlightingRequestedMessage();

/// <summary>
/// Requests that the current host close the scripting workflow.
/// </summary>
public sealed record ScriptingRequestCloseMessage();

/// <summary>
/// Signals that the project script path changed.
/// </summary>
/// <param name="OldPath">The previous script path.</param>
/// <param name="NewPath">The new script path.</param>
public sealed record ScriptingScriptPathChangedMessage(string OldPath, string NewPath);

/// <summary>
/// Signals that the project levels path changed.
/// </summary>
/// <param name="OldPath">The previous levels path.</param>
/// <param name="NewPath">The new levels path.</param>
public sealed record ScriptingLevelsPathChangedMessage(string OldPath, string NewPath);

/// <summary>
/// Signals that shell-owned scripting settings changed for a workspace.
/// </summary>
/// <param name="WorkspaceKind">The scripting workspace kind that changed.</param>
public sealed record ScriptingSettingsChangedMessage(string WorkspaceKind);
