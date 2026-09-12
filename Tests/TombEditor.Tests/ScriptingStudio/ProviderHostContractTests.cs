using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Messaging.Scripting;
using TombLib.Scripting.Lua;
using TombLib.Scripting.TRX;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;
using TombLib.Scripting.UI.Editors;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class ProviderHostContractTests
{
	[TestMethod]
	public void TrxLuaProfile_CreatesPrimaryAndLuaEditorsWithMatchingDocumentModes()
	{
		StaTestHelper.RunInSta(() =>
		{
			string scriptDirectoryPath = Directory.CreateTempSubdirectory("TombEditor-ProviderHost-").FullName;
			try
			{
				File.WriteAllText(Path.Combine(scriptDirectoryPath, "gameflow.json5"), string.Empty);
				var profile = CreateProfile(TRVersion.Game.TR1, supportsLua: true, scriptDirectoryPath);
				var registrations = new RegistrationCapture();

				profile.RegisterEditors(registrations);

				IEditorControl trxEditor = registrations.Create(DocumentMode.TRX);
				IEditorControl luaEditor = registrations.Create(DocumentMode.Lua);

				Assert.IsInstanceOfType(trxEditor, typeof(TRXEditor));
				Assert.IsInstanceOfType(luaEditor, typeof(LuaEditor));
				Assert.AreEqual(DocumentMode.TRX, registrations.GetDocumentRegistration(trxEditor)!.DocumentMode);
				Assert.AreEqual(DocumentMode.Lua, registrations.GetDocumentRegistration(luaEditor)!.DocumentMode);
			}
			finally
			{
				Directory.Delete(scriptDirectoryPath, recursive: true);
			}
		});
	}

	[TestMethod]
	public void TrxLuaProfile_ComposesDistinctOutlineAndConfigurationPolicies()
	{
		StaTestHelper.RunInSta(() =>
		{
			string scriptDirectoryPath = Directory.CreateTempSubdirectory("TombEditor-ProviderHost-").FullName;
			try
			{
				File.WriteAllText(Path.Combine(scriptDirectoryPath, "gameflow.json5"), string.Empty);
				var profile = CreateProfile(TRVersion.Game.TR1, supportsLua: true, scriptDirectoryPath);
				var registrations = new RegistrationCapture();

				profile.RegisterEditors(registrations);

				ScriptingDocumentContributions trx = registrations.GetContributions(DocumentMode.TRX);
				ScriptingDocumentContributions lua = registrations.GetContributions(DocumentMode.Lua);

				Assert.AreEqual(ScriptingDocumentConfigurationKind.TRX, trx.ConfigurationKind);
				Assert.IsNotNull(trx.OutlineProviderFactory);
				Assert.AreEqual(ScriptingDocumentConfigurationKind.Lua, lua.ConfigurationKind);
				Assert.IsNull(lua.OutlineProviderFactory);
			}
			finally
			{
				Directory.Delete(scriptDirectoryPath, recursive: true);
			}
		});
	}

	[TestMethod]
	public void TrxProfileWithoutLua_ComposesPrimaryEditorAndSharedDiagnosticsOnly()
	{
		StaTestHelper.RunInSta(() =>
		{
			string scriptDirectoryPath = Directory.CreateTempSubdirectory("TombEditor-ProviderHost-").FullName;
			try
			{
				File.WriteAllText(Path.Combine(scriptDirectoryPath, "gameflow.json5"), string.Empty);
				var profile = CreateProfile(TRVersion.Game.TR1, supportsLua: false, scriptDirectoryPath);
				var registrations = new RegistrationCapture();

				profile.RegisterEditors(registrations);

				Assert.IsFalse(registrations.Contains(DocumentMode.Lua));
				Assert.IsTrue(profile.SupportsView(UICommand.LuaDiagnostics));
				Assert.IsFalse(profile.SupportsView(UICommand.LuaReferencesResults));
			}
			finally
			{
				Directory.Delete(scriptDirectoryPath, recursive: true);
			}
		});
	}

	private static ScriptingWorkspaceProfile CreateProfile(TRVersion.Game gameVersion, bool supportsLua, string scriptDirectoryPath)
		=> ScriptingWorkspaceProfileTestFactory.CreateSelectorProfile(gameVersion, supportsLua, scriptDirectoryPath);

	private sealed class RegistrationCapture : IEditorDocumentController
	{
		private readonly List<ScriptingDocumentRegistration> _registrations = [];
		private readonly Dictionary<IEditorControl, ScriptingDocumentRegistration> _createdRegistrations = [];

		public string ScriptRootDirectoryPath { get; set; } = string.Empty;

		public IEditorControl? CurrentEditor => null;
		public ScriptingDocumentContext CurrentDocumentContext => ScriptingDocumentContext.Empty;

		public IEditorControl Create(DocumentMode documentMode)
		{
			ScriptingDocumentRegistration registration = _registrations.Single(value => value.DocumentMode == documentMode);
			IEditorControl editor = registration.Factory(new Version(1, 0));
			_createdRegistrations.Add(editor, registration);
			return editor;
		}

		public bool Contains(DocumentMode documentMode)
			=> _registrations.Any(value => value.DocumentMode == documentMode);

		public ScriptingDocumentContributions GetContributions(DocumentMode documentMode)
			=> _registrations.Single(value => value.DocumentMode == documentMode).Contributions;

		public ScriptingDocumentRegistration? GetDocumentRegistration(IEditorControl? editor)
			=> editor is not null && _createdRegistrations.TryGetValue(editor, out ScriptingDocumentRegistration? registration)
				? registration
				: null;

		public void RegisterDocument(ScriptingDocumentRegistration registration)
			=> _registrations.Add(registration);

		public IEnumerable<IEditorControl> GetOpenEditors() => [];
		public IEditorControl? FindEditor(string filePath, EditorType editorType = EditorType.Default) => null;
		public IEditorControl? FindSourceEditor(string filePath) => null;
		public IEnumerable<IEditorControl> FindEditorsOfFile(string filePath) => [];
		public bool ContainsEditor(IEditorControl editor) => false;
		public void CheckPreviousSession() { }
		public void OpenFile(string filePath, EditorType editorType = EditorType.Default, DocumentLoadOptions options = default) { }
		public void OpenSourceFile(string filePath, DocumentLoadOptions options = default) { }
		public void AddFileToReloadQueue(string filePath) { }
		public void TryRunFileReloadQueue() { }
		public bool AskSaveAll() => true;
		public void SaveAll() { }
		public FileSavingResult SaveFile() => FileSavingResult.Success;
		public FileSavingResult SaveFile(IEditorControl editor) => FileSavingResult.Success;
		public FileSavingResult SaveFileAs() => FileSavingResult.Success;
		public FileSavingResult SaveFileAs(IEditorControl editor) => FileSavingResult.Success;
		public void CloseInvalidEditors() { }
		public void ActivateEditor(IEditorControl editor) { }
		public bool TryCloseEditor(IEditorControl editor) => true;
		public bool TryActivatePreviousEditor() => false;
		public bool TryActivateNextEditor() => false;
		public string GetDocumentTitle(IEditorControl editor) => string.Empty;
		public void RenameDocument(string oldFilePath, string newFilePath) { }
		public bool IsEveryDocumentSaved() => true;
		public void EnsureTabFileSynchronization() { }

		public event EventHandler? FileOpened { add { } remove { } }
		public event EventHandler<ScriptingDocumentContextChangedEventArgs>? CurrentEditorChanged { add { } remove { } }
		public event EventHandler<EditorControlEventArgs>? EditorClosed { add { } remove { } }
		public event EventHandler<EditorControlEventArgs>? EditorTitleChanged { add { } remove { } }
		public event EventHandler<DocumentRenamedEventArgs>? DocumentRenamed { add { } remove { } }
	}
}