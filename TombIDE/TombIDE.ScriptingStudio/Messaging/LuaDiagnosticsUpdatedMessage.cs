#nullable enable

using CommunityToolkit.Mvvm.Messaging.Messages;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System.Collections.Generic;

namespace TombIDE.ScriptingStudio.Messaging;

/// <summary>
/// Carries Lua diagnostics for a specific file path.
/// Consumed by <see cref="Lua.LuaTrackedDocumentStateService"/>.
/// </summary>
public sealed class LuaDiagnosticsUpdatedMessage(LuaDiagnosticsPayload payload)
	: ValueChangedMessage<LuaDiagnosticsPayload>(payload);

public readonly record struct LuaDiagnosticsPayload(
	string FilePath,
	IReadOnlyList<TextEditorDiagnostic> Diagnostics
);
