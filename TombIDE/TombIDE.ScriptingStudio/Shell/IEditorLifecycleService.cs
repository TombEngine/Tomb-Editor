#nullable enable

using System;

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Owns general editor attach/detach and keyboard/event lifecycle.
/// Must not own direct shell view mutation.
/// </summary>
public interface IEditorLifecycleService : IDisposable
{
	/// <summary>
	/// Attaches the lifecycle coordinator to editor events.
	/// Idempotent: calling Attach again first detaches.
	/// </summary>
	void Attach();
}
