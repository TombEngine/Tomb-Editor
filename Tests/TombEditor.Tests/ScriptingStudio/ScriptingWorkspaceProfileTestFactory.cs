using System.Collections.Generic;
using System.Linq;
using Moq;
using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;
using TombLib.Scripting.UI.Editors;

namespace TombEditor.Tests.ScriptingStudio;

internal static class ScriptingWorkspaceProfileTestFactory
{
	public static ScriptingWorkspaceProfile CreateSelectorProfile(
		TRVersion.Game gameVersion,
		bool supportsLua,
		string scriptDirectoryPath,
		IScriptingStudioShellSettingsStore? settingsStore = null,
		Func<ScriptingDocumentContext, ITextDocumentSymbolProvider?>? luaOutlineProviderFactory = null)
	{
		ArgumentNullException.ThrowIfNull(scriptDirectoryPath);

		var project = new Mock<IGameProject>();
		project.SetupGet(value => value.GameVersion).Returns(gameVersion);
		project.Setup(value => value.GetScriptRootDirectory()).Returns(scriptDirectoryPath);

		var projectContext = new Mock<IScriptingProjectContext>();
		projectContext.SetupGet(value => value.Project).Returns(project.Object);
		projectContext.SetupGet(value => value.ScriptRootDirectoryPath).Returns(scriptDirectoryPath);

		settingsStore ??= CreateSettingsStore(supportsLua);

		return ScriptingWorkspaceProfileSelector.Create(
			projectContext.Object,
			settingsStore,
			ScriptingLanguageServicesTestFactory.CreateClassicScript(),
			ScriptingLanguageServicesTestFactory.CreateGameFlowScript(),
			ScriptingLanguageServicesTestFactory.CreateTRX(),
			luaOutlineProviderFactory);
	}

	public static ScriptingWorkspaceProfile CreateLuaProfile(
		IReadOnlyList<UICommand>? viewCommands = null,
		bool supportsLuaActivation = false,
		string fileExplorerFilter = "*.lua",
		string? initialFilePath = null)
	{
		ScriptingWorkspaceViewContribution[] viewContributions = viewCommands is null
			? []
			: [.. viewCommands.Select(static command => new ScriptingWorkspaceViewContribution(command))];

		return new ScriptingWorkspaceProfile(
			ScriptingWorkspaceKind.Lua,
			TRVersion.Game.TombEngine,
			[],
			initialFilePath ?? string.Empty,
			viewContributions,
			[],
			[],
			[],
			[],
			fileExplorerFilter,
			string.Empty,
			"--",
			supportsBuild: false,
			supportsDocumentation: false,
			new ScriptingWorkspaceLayoutPersistence(
				new DockPanelState(),
				() => new DockPanelState(),
				() => string.Empty,
				_ => { }),
			supportsLuaActivation);
	}

	private static IScriptingStudioShellSettingsStore CreateSettingsStore(bool supportsLua)
	{
		var settingsStore = new Mock<IScriptingStudioShellSettingsStore>();
		settingsStore
			.Setup(value => value.IsLuaEnabled(It.IsAny<ScriptingWorkspaceKind>()))
			.Returns(supportsLua);
		settingsStore
			.Setup(value => value.Load(It.IsAny<ScriptingWorkspaceProfile>()))
			.Returns(new ScriptingStudioShellWorkspaceSettings());
		return settingsStore.Object;
	}
}
