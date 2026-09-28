using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.TRX;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class ScriptingSettingsWindowViewModelTests
{
	private static readonly ClassicScriptLanguageServices ClassicLanguageServices = ScriptingLanguageServicesTestFactory.CreateClassicScript();
	private static readonly GameFlowLanguageServices GameFlowLanguageServices = ScriptingLanguageServicesTestFactory.CreateGameFlowScript();
	private static readonly TRXLanguageServices TrxLanguageServices = ScriptingLanguageServicesTestFactory.CreateTRX();

	[TestMethod]
	[DataRow(ScriptingWorkspaceKind.ClassicScript, true)]
	[DataRow(ScriptingWorkspaceKind.TRX, true)]
	[DataRow(ScriptingWorkspaceKind.GameFlowScript, false)]
	[DataRow(ScriptingWorkspaceKind.Lua, false)]
	public void LuaActivationVisibility_IsLimitedToSupportedPrimaryWorkspaces(ScriptingWorkspaceKind workspaceKind, bool expected)
	{
		StaTestHelper.RunInSta(() =>
		{
			var viewModel = CreateViewModel(workspaceKind, luaEnabled: false, supportsLuaActivation: expected);

			Assert.AreEqual(expected, viewModel.SupportsLuaActivation);
		});
	}

	[TestMethod]
	public void LuaActivation_PreservesPersistedValueAndReopenDescription()
	{
		StaTestHelper.RunInSta(() =>
		{
			var viewModel = CreateViewModel(ScriptingWorkspaceKind.ClassicScript, luaEnabled: true, supportsLuaActivation: true);

			Assert.IsTrue(viewModel.LuaEnabled);
			StringAssert.Contains(viewModel.LuaActivationDescription, "reopened");
		});
	}

	private static ScriptingSettingsWindowViewModel CreateViewModel(
		ScriptingWorkspaceKind workspaceKind,
		bool luaEnabled,
		bool supportsLuaActivation)
	{
		var profile = new ScriptingWorkspaceProfile(
			workspaceKind,
			workspaceKind == ScriptingWorkspaceKind.Lua ? TRVersion.Game.TombEngine : TRVersion.Game.TR4,
			[],
			string.Empty,
			[],
			[],
			[],
			[],
			[],
			"*.txt",
			string.Empty,
			";",
			false,
			false,
			new ScriptingWorkspaceLayoutPersistence(
				new DockPanelState(),
				() => new DockPanelState(),
				() => string.Empty,
				_ => { }),
			supportsLuaActivation);

		return new ScriptingSettingsWindowViewModel(
			profile,
			null,
			luaEnabled,
			ClassicLanguageServices,
			GameFlowLanguageServices,
			TrxLanguageServices);
	}
}