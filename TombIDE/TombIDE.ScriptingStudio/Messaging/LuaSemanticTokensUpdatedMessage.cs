#nullable enable

using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Collections.Generic;

namespace TombIDE.ScriptingStudio.Messaging;

/// <summary>
/// Carries Lua semantic tokens for a specific file path.
/// Consumed by <see cref="Lua.LuaTrackedDocumentStateService"/>.
/// </summary>
public sealed class LuaSemanticTokensUpdatedMessage(LuaSemanticTokensPayload payload)
	: ValueChangedMessage<LuaSemanticTokensPayload>(payload);

public readonly record struct LuaSemanticTokensPayload(
	string FilePath,
	IReadOnlyList<SemanticToken> SemanticTokens
);
