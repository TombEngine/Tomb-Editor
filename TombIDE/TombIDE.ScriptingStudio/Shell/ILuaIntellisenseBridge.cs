#nullable enable

using System;

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Owns the provider-to-UI dispatch boundary for Lua IntelliSense events.
/// Must not own layout or pane ownership.
/// </summary>
public interface ILuaIntellisenseBridge : IDisposable
{
	/// <summary>
	/// Attaches the bridge to IntelliSense provider events.
	/// </summary>
	void Attach();
}
