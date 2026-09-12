#nullable enable

using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Services;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript.Compilers;

namespace TombIDE.ScriptingStudio.ClassicScript;

internal sealed record ClassicScriptWorkspaceAutomationCallbacks(
	Action<string> AppendScript,
	Action<string> AddNewLevelNameString,
	Func<string, bool> AddNewPluginEntry,
	Func<string, bool> AddNewNGString,
	Func<string, bool> IsLevelScriptDefined,
	Func<string, bool> IsLevelLanguageStringDefined,
	Action<string, string> RenameRequestedLevelScript,
	Action<string, string> RenameRequestedLanguageString,
	Action ApplyUserSettings,
	Action SaveAll,
	Action ShowCompilerLogsPane,
	Action<string> UpdateCompilerLogs);

internal sealed class ClassicScriptWorkspaceAutomationProvider : IStudioWorkspaceAutomationProvider
{
	private readonly ClassicScriptWorkspaceAutomationCallbacks _callbacks;
	private readonly string _engineDirectoryPath;
	private readonly IWin32Window _promptOwner;
	private readonly string _scriptRootDirectoryPath;
	private readonly Func<bool> _showCompilerLogsAfterBuildProvider;
	private readonly StudioSilentActionService _silentActionService;
	private readonly Func<bool> _useNewIncludeMethodProvider;
	private readonly ScriptingWorkspaceProfile _workspaceProfile;

	public ClassicScriptWorkspaceAutomationProvider(
		IWin32Window promptOwner,
		ScriptingWorkspaceProfile workspaceProfile,
		StudioSilentActionService silentActionService,
		string scriptRootDirectoryPath,
		string engineDirectoryPath,
		ClassicScriptWorkspaceAutomationCallbacks callbacks,
		Func<bool> showCompilerLogsAfterBuildProvider,
		Func<bool> useNewIncludeMethodProvider)
	{
		_promptOwner = promptOwner ?? throw new ArgumentNullException(nameof(promptOwner));
		_workspaceProfile = workspaceProfile ?? throw new ArgumentNullException(nameof(workspaceProfile));
		_silentActionService = silentActionService ?? throw new ArgumentNullException(nameof(silentActionService));
		_scriptRootDirectoryPath = scriptRootDirectoryPath ?? string.Empty;
		_engineDirectoryPath = engineDirectoryPath ?? string.Empty;
		_callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
		_showCompilerLogsAfterBuildProvider = showCompilerLogsAfterBuildProvider ?? throw new ArgumentNullException(nameof(showCompilerLogsAfterBuildProvider));
		_useNewIncludeMethodProvider = useNewIncludeMethodProvider ?? throw new ArgumentNullException(nameof(useNewIncludeMethodProvider));
	}

	public void HandleIDEEvent(IIDEEvent ideEvent)
	{
		if (ideEvent is null)
			return;

		if (ideEvent is IDE.ScriptEditor_ReloadSyntaxHighlightingEvent)
		{
			ReloadSyntaxHighlighting();
			return;
		}

		if (!IsSilentAction(ideEvent))
			return;

		switch (ideEvent)
		{
			case IDE.ScriptEditor_AppendScriptEvent appendEvent:
				AppendScript(appendEvent.Result);
				break;

			case IDE.ScriptEditor_AddNewLevelStringEvent addLevelStringEvent:
				AddLevelString(addLevelStringEvent.LevelName);
				break;

			case IDE.ScriptEditor_AddNewPluginEntryEvent addPluginEntryEvent:
				AddPluginEntry(addPluginEntryEvent.PluginString);
				break;

			case IDE.ScriptEditor_AddNewNGStringEvent addNgStringEvent:
				AddNgString(addNgStringEvent.NGString);
				break;

			case IDE.ScriptEditor_ScriptPresenceCheckEvent scriptPresenceEvent:
				IDE.Instance.ScriptDefined = IsScriptDefined(scriptPresenceEvent.LevelName);
				break;

			case IDE.ScriptEditor_StringPresenceCheckEvent stringPresenceEvent:
				IDE.Instance.StringDefined = IsStringDefined(stringPresenceEvent.String);
				break;

			case IDE.ScriptEditor_RenameLevelEvent renameLevelEvent:
				RenameLevel(renameLevelEvent.OldName, renameLevelEvent.NewName);
				break;
		}
	}

	public void AddLevelString(string levelName)
	{
		string languageFilePath = PathHelper.GetLanguageFilePath(_scriptRootDirectoryPath, TRVersion.Game.TR4);
		SilentActionFileState languageFileState = _silentActionService.CaptureSourceFileState(languageFilePath);
		_callbacks.AddNewLevelNameString(levelName);
		_silentActionService.Complete(true, _silentActionService.CreateCompletion(languageFileState));
	}

	public void AddNgString(string ngString)
	{
		string ngLanguageFilePath = PathHelper.GetLanguageFilePath(_scriptRootDirectoryPath, TRVersion.Game.TRNG);
		SilentActionFileState ngLanguageFileState = _silentActionService.CaptureSourceFileState(ngLanguageFilePath);
		bool isChanged = _callbacks.AddNewNGString(ngString);
		_silentActionService.Complete(isChanged, _silentActionService.CreateCompletion(ngLanguageFileState));
	}

	public void AddPluginEntry(string pluginString)
	{
		string scriptFilePath = PathHelper.GetScriptFilePath(_scriptRootDirectoryPath, TRVersion.Game.TR4);
		SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
		bool isChanged = _callbacks.AddNewPluginEntry(pluginString);
		_silentActionService.Complete(isChanged, _silentActionService.CreateCompletion(scriptFileState));
	}

	public void AppendScript(ScriptGenerationResult result)
	{
		if (!result.HasContent)
			return;

		string scriptFilePath = PathHelper.GetScriptFilePath(_scriptRootDirectoryPath, TRVersion.Game.TR4);
		SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
		_callbacks.AppendScript(result.GameFlowScript);
		_silentActionService.Complete(true, _silentActionService.CreateCompletion(scriptFileState));
	}

	public void Build()
	{
		_callbacks.SaveAll();

		if (_workspaceProfile.GameVersion == TRVersion.Game.TR4)
			CompileTR4Script();
		else if (_workspaceProfile.GameVersion == TRVersion.Game.TRNG)
			CompileTRNGScript();
	}

	public void ShowDocumentation()
	{
		string pdfPath = Path.Combine(DefaultPaths.ResourcesDirectory, "ClassicScript", "TRNG Script Reference Manual.pdf");
		OpenPathIfExists(pdfPath);
	}

	public bool IsScriptDefined(string levelName)
		=> _callbacks.IsLevelScriptDefined(levelName);

	public bool IsStringDefined(string value)
		=> _callbacks.IsLevelLanguageStringDefined(value);

	public void ReloadSyntaxHighlighting()
		=> _callbacks.ApplyUserSettings();

	public void RenameLevel(string oldName, string newName)
	{
		string scriptFilePath = PathHelper.GetScriptFilePath(_scriptRootDirectoryPath, TRVersion.Game.TR4);
		string languageFilePath = PathHelper.GetLanguageFilePath(_scriptRootDirectoryPath, TRVersion.Game.TR4);
		SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);
		SilentActionFileState languageFileState = _silentActionService.CaptureSourceFileState(languageFilePath);
		_callbacks.RenameRequestedLevelScript(oldName, newName);
		_callbacks.RenameRequestedLanguageString(oldName, newName);
		_silentActionService.Complete(
			true,
			_silentActionService.CreateCompletion(scriptFileState),
			_silentActionService.CreateCompletion(languageFileState));
	}

	private static bool IsSilentAction(IIDEEvent ideEvent)
	{
		return ideEvent is IDE.ScriptEditor_AppendScriptEvent
			or IDE.ScriptEditor_AddNewLevelStringEvent
			or IDE.ScriptEditor_AddNewPluginEntryEvent
			or IDE.ScriptEditor_AddNewNGStringEvent
			or IDE.ScriptEditor_ScriptPresenceCheckEvent
			or IDE.ScriptEditor_StringPresenceCheckEvent
			or IDE.ScriptEditor_RenameLevelEvent;
	}

	private void CompileTR4Script()
	{
		try
		{
			string logs = TR4Compiler.Compile(_scriptRootDirectoryPath, _engineDirectoryPath);

			if (_showCompilerLogsAfterBuildProvider())
				_callbacks.ShowCompilerLogsPane();

			_callbacks.UpdateCompilerLogs(logs);
		}
		catch (Exception exception)
		{
			ShowError(exception.Message);
		}
	}

	private void CompileTRNGScript()
	{
		try
		{
			bool success = NGCompiler.Compile(
				_scriptRootDirectoryPath,
				_engineDirectoryPath,
				_useNewIncludeMethodProvider());

			string logFilePath = Path.Combine(DefaultPaths.VGEDirectory, "LastCompilerLog.txt");
			_callbacks.UpdateCompilerLogs(File.ReadAllText(logFilePath));

			if (!success)
				ShowError("Script compilation yielded an error. Please check the logs.");

			if (_showCompilerLogsAfterBuildProvider() || !success)
				_callbacks.ShowCompilerLogsPane();
		}
		catch (Exception exception)
		{
			ShowError(exception.Message);
		}
	}

	private void ShowError(string message)
		=> DarkMessageBox.Show(_promptOwner, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

	private static void OpenPathIfExists(string filePath)
	{
		if (!File.Exists(filePath))
			return;

		Process.Start(new ProcessStartInfo
		{
			FileName = filePath,
			UseShellExecute = true
		});
	}
}
