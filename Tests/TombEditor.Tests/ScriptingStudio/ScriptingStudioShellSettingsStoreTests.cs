using System.IO;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.WorkspaceProfile;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class ScriptingStudioShellSettingsStoreTests
{
	[TestMethod]
	public void MissingSettingsFile_UsesLuaDisabledDefaults()
	{
		using var temporaryDirectory = TemporaryDirectory.Create();
		string settingsPath = Path.Combine(temporaryDirectory.Path, "settings.xml");
		var store = new XmlScriptingStudioShellSettingsStore(settingsPath);

		Assert.IsFalse(store.IsLuaEnabled(ScriptingWorkspaceKind.ClassicScript));
		Assert.IsFalse(store.IsLuaEnabled(ScriptingWorkspaceKind.TRX));
	}

	[TestMethod]
	public void CorruptSettingsFile_UsesLuaDisabledDefaults()
	{
		using var temporaryDirectory = TemporaryDirectory.Create();
		string settingsPath = Path.Combine(temporaryDirectory.Path, "settings.xml");
		File.WriteAllText(settingsPath, "<invalid");
		var store = new XmlScriptingStudioShellSettingsStore(settingsPath);

		Assert.IsFalse(store.IsLuaEnabled(ScriptingWorkspaceKind.ClassicScript));
		Assert.IsFalse(store.IsLuaEnabled(ScriptingWorkspaceKind.TRX));
	}

	[TestMethod]
	public void SaveAndReload_PreservesWorkspaceIsolationAndLuaDefault()
	{
		using var temporaryDirectory = TemporaryDirectory.Create();
		string settingsPath = Path.Combine(temporaryDirectory.Path, "settings.xml");
		var store = new XmlScriptingStudioShellSettingsStore(settingsPath);
		store.Save(ScriptingWorkspaceKind.ClassicScript, new ScriptingStudioShellWorkspaceSettings { LuaEnabled = true });

		var reloadedStore = new XmlScriptingStudioShellSettingsStore(settingsPath);

		Assert.IsTrue(reloadedStore.IsLuaEnabled(ScriptingWorkspaceKind.ClassicScript));
		Assert.IsFalse(reloadedStore.IsLuaEnabled(ScriptingWorkspaceKind.TRX));
		Assert.IsTrue(reloadedStore.IsLuaEnabled(ScriptingWorkspaceKind.Lua));
	}

	private sealed class TemporaryDirectory : IDisposable
	{
		private TemporaryDirectory(string path)
		{
			Path = path;
		}

		public string Path { get; }

		public static TemporaryDirectory Create()
			=> new(Directory.CreateTempSubdirectory("TombEditor-Settings-").FullName);

		public void Dispose()
		{
			if (Directory.Exists(Path))
				Directory.Delete(Path, recursive: true);
		}
	}
}