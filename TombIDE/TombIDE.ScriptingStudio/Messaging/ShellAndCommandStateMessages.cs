#nullable enable

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TombIDE.ScriptingStudio.Messaging;

/// <summary>
/// Signals that the shell UI should be refreshed (command surfaces, pane visibility, etc.).
/// </summary>
public sealed class ShellUiRefreshMessage() : ValueChangedMessage<bool>(true);

/// <summary>
/// Signals that editor command states (undo/redo/save enabled) should be refreshed.
/// </summary>
public sealed class CommandStateRefreshMessage() : ValueChangedMessage<bool>(true);
