using System;
using System.IO;
using System.Linq;
using Moq;
using TombIDE.ScriptingStudio;
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
public sealed class ScriptingPhase0ProfileMatrixTests
{
	[DataTestMethod]
	[DataRow(TRVersion.Game.TR1, ScriptingWorkspaceKind.TRX, DocumentMode.TRX)]
	[DataRow(TRVersion.Game.TR2, ScriptingWorkspaceKind.GameFlowScript, DocumentMode.GameFlowScript)]
	[DataRow(TRVersion.Game.TR3, ScriptingWorkspaceKind.GameFlowScript, DocumentMode.GameFlowScript)]
	[DataRow(TRVersion.Game.TR4, ScriptingWorkspaceKind.ClassicScript, DocumentMode.ClassicScript)]
	[DataRow(TRVersion.Game.TRNG, ScriptingWorkspaceKind.ClassicScript, DocumentMode.ClassicScript)]
	[DataRow(TRVersion.Game.TR2X, ScriptingWorkspaceKind.TRX, DocumentMode.TRX)]
	[DataRow(TRVersion.Game.TR3X, ScriptingWorkspaceKind.TRX, DocumentMode.TRX)]
	[DataRow(TRVersion.Game.TombEngine, ScriptingWorkspaceKind.Lua, DocumentMode.Lua)]
	[DataRow(TRVersion.Game.TR1, ScriptingWorkspaceKind.TRX, DocumentMode.TRX, false)]
	[DataRow(TRVersion.Game.TR2, ScriptingWorkspaceKind.GameFlowScript, DocumentMode.GameFlowScript, false)]
	[DataRow(TRVersion.Game.TR3, ScriptingWorkspaceKind.GameFlowScript, DocumentMode.GameFlowScript, false)]
	[DataRow(TRVersion.Game.TR4, ScriptingWorkspaceKind.ClassicScript, DocumentMode.ClassicScript, false)]
	[DataRow(TRVersion.Game.TRNG, ScriptingWorkspaceKind.ClassicScript, DocumentMode.ClassicScript, false)]
	[DataRow(TRVersion.Game.TR2X, ScriptingWorkspaceKind.TRX, DocumentMode.TRX, false)]
	[DataRow(TRVersion.Game.TR3X, ScriptingWorkspaceKind.TRX, DocumentMode.TRX, false)]
	[DataRow(TRVersion.Game.TombEngine, ScriptingWorkspaceKind.Lua, DocumentMode.Lua, false)]
	public void ProfileSelector_DerivesModesAcrossAllBranches(
		TRVersion.Game gameVersion,
		ScriptingWorkspaceKind expectedWorkspaceKind,
		DocumentMode expectedPrimaryMode,
		bool supportsLua = true)
	{
		using var scriptDirectory = new TemporaryScriptDirectory(gameVersion);
		ScriptingWorkspaceProfile profile = CreateProfile(gameVersion, supportsLua, scriptDirectory.Path);

		Assert.AreEqual(expectedWorkspaceKind, profile.Kind);
		Assert.IsTrue(profile.DocumentRegistrations.Any(registration => registration.DocumentMode == expectedPrimaryMode));

		DocumentMode[] expectedModes = profile.DocumentRegistrations
			.SelectMany(registration => registration.SupportedDocumentModes)
			.Distinct()
			.ToArray();
		CollectionAssert.AreEquivalent(expectedModes, profile.AllowedDocumentModes.ToArray());

		bool luaExpected = gameVersion is TRVersion.Game.TR4 or TRVersion.Game.TRNG or TRVersion.Game.TR1 or TRVersion.Game.TR2X or TRVersion.Game.TR3X
			? supportsLua
			: gameVersion == TRVersion.Game.TombEngine;
		Assert.AreEqual(luaExpected, profile.DocumentRegistrations.Any(registration => registration.DocumentMode == DocumentMode.Lua));
	}

	[TestMethod]
	public void ProfileSelector_PreservesRegistrationPriorityAndFallbackSemantics()
	{
		using var scriptDirectory = new TemporaryScriptDirectory(TRVersion.Game.TR4);
		ScriptingWorkspaceProfile firstProfile = CreateProfile(TRVersion.Game.TR4, supportsLua: true, scriptDirectory.Path);
		ScriptingWorkspaceProfile secondProfile = CreateProfile(TRVersion.Game.TR4, supportsLua: true, scriptDirectory.Path);

		string[] firstSignature = firstProfile.DocumentRegistrations.Select(CreateRegistrationSignature).ToArray();
		string[] secondSignature = secondProfile.DocumentRegistrations.Select(CreateRegistrationSignature).ToArray();
		CollectionAssert.AreEqual(firstSignature, secondSignature);

		int firstFallbackIndex = Array.FindIndex(firstProfile.DocumentRegistrations.ToArray(), registration => registration.IsFallback);
		if (firstFallbackIndex >= 0)
			Assert.IsTrue(firstProfile.DocumentRegistrations.Take(firstFallbackIndex).All(registration => !registration.IsFallback));

		foreach (ScriptingDocumentRegistration registration in firstProfile.DocumentRegistrations.Where(registration => registration.IsFallback))
		{
			Assert.IsTrue(
				registration.SupportedDocumentModes.Contains(DocumentMode.PlainText),
				"Fallback registrations intentionally advertise PlainText compatibility while retaining their editor identity.");
			Assert.IsFalse(registration.SupportsFile("Script.txt"));
			Assert.IsFalse(registration.IsDefaultForFile("Script.txt"));
		}
	}

	private static ScriptingWorkspaceProfile CreateProfile(
		TRVersion.Game gameVersion,
		bool supportsLua,
		string scriptDirectoryPath)
		=> ScriptingWorkspaceProfileTestFactory.CreateSelectorProfile(gameVersion, supportsLua, scriptDirectoryPath);

	private static string CreateRegistrationSignature(ScriptingDocumentRegistration registration)
		=> string.Join(
			"|",
			registration.EditorType,
			registration.DocumentMode,
			registration.IsFallback,
			string.Join(",", registration.SupportedDocumentModes));

	private sealed class TemporaryScriptDirectory : IDisposable
	{
		public TemporaryScriptDirectory(TRVersion.Game gameVersion)
		{
			Path = Directory.CreateTempSubdirectory("TombEditor-Phase0-Profile-").FullName;
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
}
