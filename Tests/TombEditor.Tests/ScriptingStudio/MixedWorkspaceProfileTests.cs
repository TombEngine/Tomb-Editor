using System;
using System.IO;
using Moq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;
using TombLib.Scripting.UI.Editors;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public class MixedWorkspaceProfileTests
{
	[TestMethod]
	[DataRow(TRVersion.Game.TR4, ScriptingWorkspaceKind.ClassicScript, DocumentMode.ClassicScript)]
	[DataRow(TRVersion.Game.TR2, ScriptingWorkspaceKind.GameFlowScript, DocumentMode.GameFlowScript)]
	[DataRow(TRVersion.Game.TR1, ScriptingWorkspaceKind.TRX, DocumentMode.TRX)]
	[DataRow(TRVersion.Game.TombEngine, ScriptingWorkspaceKind.Lua, DocumentMode.Lua)]
	public void ProfileSelector_UsesPrimaryProviderForSupportedGameVersion(
		TRVersion.Game gameVersion,
		ScriptingWorkspaceKind workspaceKind,
		DocumentMode documentMode)
	{
		using var scriptDirectory = new TemporaryScriptDirectory(gameVersion);
		ScriptingWorkspaceProfile profile = CreateProfile(gameVersion, supportsLua: false, scriptDirectory.Path);

		Assert.AreEqual(workspaceKind, profile.Kind);
		CollectionAssert.Contains(profile.AllowedDocumentModes.ToArray(), documentMode);
	}

	[TestMethod]
	public void TrxProfile_WithoutLua_KeepsPrimaryOnlyCapabilityMatrix()
	{
		using var scriptDirectory = new TemporaryScriptDirectory(TRVersion.Game.TR1);
		ScriptingWorkspaceProfile profile = CreateProfile(TRVersion.Game.TR1, supportsLua: false, scriptDirectory.Path);

		Assert.IsFalse(profile.SupportsLua);
		CollectionAssert.DoesNotContain(profile.AllowedDocumentModes.ToArray(), DocumentMode.Lua);
		CollectionAssert.DoesNotContain(profile.SettingsPages.Select(page => page.Kind).ToArray(), ScriptingSettingsPageKind.Lua);
		Assert.IsTrue(profile.SupportsView(UICommand.LuaDiagnostics));
		Assert.IsFalse(profile.SupportsView(UICommand.LuaReferencesResults));
		Assert.AreEqual("*.json5", profile.FileExplorerFilter);

		var controller = new Mock<IEditorDocumentController>();
		profile.RegisterEditors(controller.Object);

		controller.Verify(mock => mock.RegisterDocument(It.Is<ScriptingDocumentRegistration>(registration => registration.DocumentMode == DocumentMode.Lua)), Times.Never);
	}

	[TestMethod]
	public void TrxProfile_WithLua_AddsLuaAfterPrimaryCapability()
	{
		using var scriptDirectory = new TemporaryScriptDirectory(TRVersion.Game.TR1);
		ScriptingWorkspaceProfile profile = CreateProfile(TRVersion.Game.TR1, supportsLua: true, scriptDirectory.Path);

		Assert.IsTrue(profile.SupportsLua);
		CollectionAssert.AreEqual(
			new[] { DocumentMode.TRX, DocumentMode.Lua },
			profile.AllowedDocumentModes.ToArray());
		CollectionAssert.AreEqual(
			new[] { ScriptingSettingsPageKind.TRX, ScriptingSettingsPageKind.Lua },
			profile.SettingsPages.Select(page => page.Kind).ToArray());
		Assert.IsTrue(profile.SupportsView(UICommand.LuaDiagnostics));
		Assert.IsTrue(profile.SupportsView(UICommand.LuaReferencesResults));
		Assert.AreEqual("*.json5|*.lua", profile.FileExplorerFilter);

		var controller = new Mock<IEditorDocumentController>();
		profile.RegisterEditors(controller.Object);

		controller.Verify(mock => mock.RegisterDocument(It.Is<ScriptingDocumentRegistration>(registration => registration.DocumentMode == DocumentMode.TRX)), Times.Once);
		controller.Verify(mock => mock.RegisterDocument(It.Is<ScriptingDocumentRegistration>(registration => registration.DocumentMode == DocumentMode.Lua)), Times.Once);
	}

	[TestMethod]
	public void ProfileCatalog_DerivesSupportedModesAndPreservesRegistrationOrder()
	{
		using var scriptDirectory = new TemporaryScriptDirectory(TRVersion.Game.TR1);
		ScriptingWorkspaceProfile profile = CreateProfile(TRVersion.Game.TR1, supportsLua: true, scriptDirectory.Path);

		CollectionAssert.AreEqual(new[] { DocumentMode.TRX, DocumentMode.Lua }, profile.DocumentRegistrations.Select(registration => registration.DocumentMode).ToArray());
		CollectionAssert.AreEqual(new[] { DocumentMode.TRX, DocumentMode.Lua }, profile.AllowedDocumentModes.ToArray());
		Assert.IsTrue(profile.DocumentRegistrations.All(registration => registration.SupportedDocumentModes.Count == 1));
	}

	[TestMethod]
	public void ClassicScriptProfile_WithLua_AddsLuaContributionsWithoutReplacingPrimaryLanguage()
	{
		using var scriptDirectory = new TemporaryScriptDirectory(TRVersion.Game.TR4);
		ScriptingWorkspaceProfile profile = CreateProfile(TRVersion.Game.TR4, supportsLua: true, scriptDirectory.Path);

		Assert.IsTrue(profile.SupportsLua);
		CollectionAssert.Contains(profile.AllowedDocumentModes.ToArray(), DocumentMode.ClassicScript);
		CollectionAssert.Contains(profile.AllowedDocumentModes.ToArray(), DocumentMode.Strings);
		CollectionAssert.Contains(profile.AllowedDocumentModes.ToArray(), DocumentMode.Lua);
		CollectionAssert.Contains(profile.SettingsPages.Select(page => page.Kind).ToArray(), ScriptingSettingsPageKind.ClassicScript);
		CollectionAssert.Contains(profile.SettingsPages.Select(page => page.Kind).ToArray(), ScriptingSettingsPageKind.Lua);
		Assert.IsTrue(profile.SupportsView(UICommand.ReferenceBrowser));
		Assert.IsTrue(profile.SupportsView(UICommand.LuaDiagnostics));
		Assert.AreEqual("*.txt|*.lua", profile.FileExplorerFilter);

		var controller = new Mock<IEditorDocumentController>();
		profile.RegisterEditors(controller.Object);

		controller.Verify(mock => mock.RegisterDocument(It.Is<ScriptingDocumentRegistration>(registration => registration.DocumentMode == DocumentMode.ClassicScript)), Times.Exactly(2));
		controller.Verify(mock => mock.RegisterDocument(It.Is<ScriptingDocumentRegistration>(registration => registration.DocumentMode == DocumentMode.Lua)), Times.Once);
	}

	[TestMethod]
	public void ProfileSelector_UsesPersistedLuaCapabilityOnWorkspaceReopen()
	{
		using var scriptDirectory = new TemporaryScriptDirectory(TRVersion.Game.TR4);
		var settingsStore = new InMemorySettingsStore();

		settingsStore.SetLuaEnabled(ScriptingWorkspaceKind.ClassicScript, true);
		ScriptingWorkspaceProfile enabledProfile = CreateProfile(TRVersion.Game.TR4, settingsStore, scriptDirectory.Path);

		Assert.IsTrue(enabledProfile.SupportsLua);
		Assert.IsTrue(enabledProfile.SupportsView(UICommand.LuaDiagnostics));

		settingsStore.SetLuaEnabled(ScriptingWorkspaceKind.ClassicScript, false);
		ScriptingWorkspaceProfile disabledProfile = CreateProfile(TRVersion.Game.TR4, settingsStore, scriptDirectory.Path);

		Assert.IsFalse(disabledProfile.SupportsLua);
		Assert.IsTrue(disabledProfile.SupportsView(UICommand.LuaDiagnostics));
	}

	private static ScriptingWorkspaceProfile CreateProfile(
		TRVersion.Game gameVersion,
		IScriptingStudioShellSettingsStore settingsStore,
		string scriptDirectoryPath)
		=> ScriptingWorkspaceProfileTestFactory.CreateSelectorProfile(gameVersion, false, scriptDirectoryPath, settingsStore);

	private static ScriptingWorkspaceProfile CreateProfile(TRVersion.Game gameVersion, bool supportsLua, string scriptDirectoryPath)
		=> ScriptingWorkspaceProfileTestFactory.CreateSelectorProfile(gameVersion, supportsLua, scriptDirectoryPath);

	private sealed class TemporaryScriptDirectory : IDisposable
	{
		public TemporaryScriptDirectory(TRVersion.Game gameVersion)
		{
			Path = Directory.CreateTempSubdirectory("TombEditor-MixedWorkspace-").FullName;
			string fileName = gameVersion == TRVersion.Game.TombEngine
				? "Gameflow.lua"
				: gameVersion is TRVersion.Game.TR1 or TRVersion.Game.TR2X or TRVersion.Game.TR3X
				? "gameflow.json5"
				: "Script.txt";
			File.WriteAllText(System.IO.Path.Combine(Path, fileName), string.Empty);
		}

		public string Path { get; }

		public void Dispose()
		{
			if (Directory.Exists(Path))
				Directory.Delete(Path, recursive: true);
		}
	}

	private sealed class InMemorySettingsStore : IScriptingStudioShellSettingsStore
	{
		private readonly ScriptingStudioShellSettingsDocument _document = new();

		public ScriptingStudioShellWorkspaceSettings CreateDefault(ScriptingWorkspaceProfile workspaceProfile)
			=> new();

		public ScriptingStudioShellWorkspaceSettings Load(ScriptingWorkspaceProfile workspaceProfile)
			=> _document.GetWorkspace(workspaceProfile.Kind).Clone();

		public ScriptingStudioShellWorkspaceSettings Load(ScriptingWorkspaceKind workspaceKind, DockPanelState defaultLayout)
			=> _document.GetWorkspace(workspaceKind).Clone();

		public bool IsLuaEnabled(ScriptingWorkspaceKind workspaceKind)
			=> _document.GetWorkspace(workspaceKind).LuaEnabled;

		public void Save(ScriptingWorkspaceKind workspaceKind, ScriptingStudioShellWorkspaceSettings settings)
			=> _document.SetWorkspace(workspaceKind, settings.Clone());

		public bool SaveShortcutOverrides(ScriptingWorkspaceKind workspaceKind, Nickelony.IDEKit.KeyBindings.KeyBindingOverrideCollection overrides)
			=> true;

		public void SetLuaEnabled(ScriptingWorkspaceKind workspaceKind, bool enabled)
			=> _document.GetWorkspace(workspaceKind).LuaEnabled = enabled;
	}
}