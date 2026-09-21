using Nickelony.IDEKit.KeyBindings;

namespace TombEditor.Tests.ScriptingStudio;

/// <summary>
/// In-memory overrides store for the scripting-studio test services.
/// </summary>
internal sealed class KeyBindingTestStore : IKeyBindingOverridesStore
{
	public KeyBindingOverrides Load() => new();

	public bool Save(KeyBindingOverrides snapshot) => true;
}