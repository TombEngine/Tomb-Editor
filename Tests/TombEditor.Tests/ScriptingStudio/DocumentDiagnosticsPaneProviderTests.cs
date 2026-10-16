using Moq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Workbench;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Docking;
using TombLib.LevelData;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class DocumentDiagnosticsPaneProviderTests
{
	[TestMethod]
	public void DiagnosticsContribution_PreservesStablePaneAndLegacySerializationKey()
	{
		StaTestHelper.RunInSta(() =>
		{
			var profile = new ScriptingWorkspaceProfile(
				ScriptingWorkspaceKind.TRX,
				TRVersion.Game.TR1,
				[],
				string.Empty,
				[new ScriptingWorkspaceViewContribution(UICommand.LuaDiagnostics)],
				[],
				[],
				[],
				[],
				"*.json5",
				string.Empty,
				"//",
				false,
				false,
				new ScriptingWorkspaceLayoutPersistence(
					new DockPanelState(),
					() => new DockPanelState(),
					() => string.Empty,
					_ => { }));
			var provider = new DocumentDiagnosticsPaneProvider(profile, new Mock<IEditorDocumentController>().Object);

			IReadOnlyList<StudioPaneContribution> contributions = provider.GetPaneContributions();

			Assert.AreEqual(1, contributions.Count);
			Assert.AreEqual("LuaDiagnostics", contributions[0].SerializationKey);
			StudioDockPane firstPane = contributions[0].CreateContent();
			StudioDockPane secondPane = contributions[0].CreateContent();

			try
			{
				Assert.AreSame(firstPane, secondPane);
			}
			finally
			{
				firstPane.Dispose();
			}
		});
	}
}