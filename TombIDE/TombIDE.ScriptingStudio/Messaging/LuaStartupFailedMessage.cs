#nullable enable

using CommunityToolkit.Mvvm.Messaging.Messages;
using Nickelony.LanguageServer.Abstractions;

namespace TombIDE.ScriptingStudio.Messaging;

/// <summary>
/// Signals that the Lua language server failed to start.
/// </summary>
public sealed class LuaStartupFailedMessage(LanguageServerStartupFailure failure)
	: ValueChangedMessage<LanguageServerStartupFailure>(failure);
