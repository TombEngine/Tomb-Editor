namespace TombIDE.ScriptingStudio.Shell;

internal sealed class StudioWindowFocusService
{
	public bool IsMainWindowFocused { get; private set; }

	public void Update(bool isMainWindowFocused)
		=> IsMainWindowFocused = isMainWindowFocused;
}
