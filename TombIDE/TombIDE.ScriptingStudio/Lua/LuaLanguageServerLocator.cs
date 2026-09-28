#nullable enable

using System.IO;

namespace TombIDE.ScriptingStudio.Lua;

internal static class LuaLanguageServerLocator
{
	private const string ExecutableFileName = "lua-language-server.exe";

	/// <summary>
	/// Resolves the bundled Lua language server executable when it is installed with TombIDE.
	/// </summary>
	/// <returns>The bundled executable path, or <see langword="null"/> when it is unavailable.</returns>
	public static string? ResolveExecutablePath()
	{
		string bundledExecutablePath = Path.Combine(DefaultPaths.TIDEDirectory, "LuaLS", "bin", ExecutableFileName);
		return File.Exists(bundledExecutablePath) ? bundledExecutablePath : null;
	}
}
