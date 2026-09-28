#nullable enable

using System;

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Owns Lua-specific editor event lifecycle.
/// Must not own menu or status bar implementation.
/// </summary>
public interface ILuaEditorLifecycleService : IDisposable
{
	/// <summary>
	/// Attaches the Lua lifecycle coordinator to editor events.
	/// </summary>
	void Attach();
}
