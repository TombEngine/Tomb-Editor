#nullable enable

using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.Shell;

/// <summary>
/// Default implementation of <see cref="IScriptingHostOperations"/> that delegates
/// to the legacy <see cref="IDE.Instance"/> singleton.
/// </summary>
internal sealed class DefaultScriptingHostOperations : IScriptingHostOperations
{
	public static readonly DefaultScriptingHostOperations Instance = new();

	private DefaultScriptingHostOperations()
	{ }

	public void IndicateExternalChange()
		=> IDE.Instance.ScriptEditor_IndicateExternalChange();
}
