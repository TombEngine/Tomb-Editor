#nullable enable

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TombIDE.Shared.Messaging.Scripting;

/// <summary>
/// Requests a synchronous can-close decision from scripting participants.
/// </summary>
public sealed class ScriptingCanCloseRequestMessage : RequestMessage<bool>
{
}

/// <summary>
/// Requests whether a level script is already defined.
/// </summary>
public sealed class ScriptingIsScriptDefinedRequestMessage : RequestMessage<bool>
{
	/// <summary>
	/// Initializes a new request.
	/// </summary>
	/// <param name="levelName">The level name to check.</param>
	public ScriptingIsScriptDefinedRequestMessage(string levelName)
	{
		LevelName = levelName ?? string.Empty;
	}

	/// <summary>
	/// Gets the level name to check.
	/// </summary>
	public string LevelName { get; }
}

/// <summary>
/// Requests whether a language string is already defined.
/// </summary>
public sealed class ScriptingIsStringDefinedRequestMessage : RequestMessage<bool>
{
	/// <summary>
	/// Initializes a new request.
	/// </summary>
	/// <param name="value">The string value to check.</param>
	public ScriptingIsStringDefinedRequestMessage(string value)
	{
		Value = value ?? string.Empty;
	}

	/// <summary>
	/// Gets the string value to check.
	/// </summary>
	public string Value { get; }
}
