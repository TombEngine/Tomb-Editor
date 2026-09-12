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
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Compilers;

namespace TombIDE.ScriptingStudio.GameFlowScript;

internal sealed record GameFlowWorkspaceAutomationCallbacks(
	Action<string> AppendScript,
	Func<string, bool> IsLevelScriptDefined,
	Action<string, string> RenameRequestedLevelScript,
	Action SaveAll,
	Action ShowCompilerLogsPane,
	Action<string> UpdateCompilerLogs);

internal sealed class GameFlowWorkspaceAutomationProvider : IStudioWorkspaceAutomationProvider
{
	private readonly GameFlowWorkspaceAutomationCallbacks _callbacks;
	private readonly string _engineExecutableFilePath;
	private readonly string _engineDirectoryPath;
	private readonly IWin32Window _promptOwner;
	private readonly string _scriptRootDirectoryPath;
	private readonly Func<bool> _showCompilerLogsAfterBuildProvider;
	private readonly StudioSilentActionService _silentActionService;
	private readonly ScriptingWorkspaceProfile _workspaceProfile;

	public GameFlowWorkspaceAutomationProvider(
		IWin32Window promptOwner,
		ScriptingWorkspaceProfile workspaceProfile,
		StudioSilentActionService silentActionService,
		string scriptRootDirectoryPath,
		string engineDirectoryPath,
		string engineExecutableFilePath,
		GameFlowWorkspaceAutomationCallbacks callbacks,
		Func<bool> showCompilerLogsAfterBuildProvider)
	{
		_promptOwner = promptOwner ?? throw new ArgumentNullException(nameof(promptOwner));
		_workspaceProfile = workspaceProfile ?? throw new ArgumentNullException(nameof(workspaceProfile));
		_silentActionService = silentActionService ?? throw new ArgumentNullException(nameof(silentActionService));
		_scriptRootDirectoryPath = scriptRootDirectoryPath ?? string.Empty;
		_engineDirectoryPath = engineDirectoryPath ?? string.Empty;
		_engineExecutableFilePath = engineExecutableFilePath ?? string.Empty;
		_callbacks = callbacks ?? throw new ArgumentNullException(nameof(callbacks));
		_showCompilerLogsAfterBuildProvider = showCompilerLogsAfterBuildProvider ?? throw new ArgumentNullException(nameof(showCompilerLogsAfterBuildProvider));
	}

	public void HandleIDEEvent(IIDEEvent ideEvent)
	{
		if (ideEvent is null || !IsSilentAction(ideEvent))
			return;

		switch (ideEvent)
		{
			case IDE.ScriptEditor_AppendScriptEvent appendEvent:
				AppendScript(appendEvent.Result);
				break;

			case IDE.ScriptEditor_ScriptPresenceCheckEvent scriptPresenceEvent:
				IDE.Instance.ScriptDefined = IsScriptDefined(scriptPresenceEvent.LevelName);
				break;

			case IDE.ScriptEditor_RenameLevelEvent renameLevelEvent:
				RenameLevel(renameLevelEvent.OldName, renameLevelEvent.NewName);
				break;
		}
	}

	public void AppendScript(ScriptGenerationResult result)
	{
		if (!result.HasContent)
			return;

		string scriptFilePath = PathHelper.GetScriptFilePath(_scriptRootDirectoryPath, TRVersion.Game.TR2);
		SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);

		_callbacks.AppendScript(result.GameFlowScript);
		_silentActionService.Complete(true, _silentActionService.CreateCompletion(scriptFileState));
	}

	public void Build()
	{
		_callbacks.SaveAll();

		try
		{
			FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(_engineExecutableFilePath);
			var productVersion = new Version(fileVersionInfo.ProductVersion ?? "0.0");
			bool success;

			if (_workspaceProfile.GameVersion == TRVersion.Game.TR3
				&& productVersion >= new Version(2, 0, 0, 0))
			{
				success = ScriptCompiler.CompileTR3Version2Plus(
					_scriptRootDirectoryPath,
					Path.Combine(_engineDirectoryPath, "data"),
					_showCompilerLogsAfterBuildProvider());
			}
			else
			{
				success = ScriptCompiler.ClassicCompile(
					_scriptRootDirectoryPath,
					Path.Combine(_engineDirectoryPath, "data"),
					_workspaceProfile.GameVersion == TRVersion.Game.TR3,
					_showCompilerLogsAfterBuildProvider());
			}

			_callbacks.UpdateCompilerLogs(success ? "Script compiled successfully!" : "ERROR: Couldn't compile script.");

			if (_showCompilerLogsAfterBuildProvider() || !success)
				_callbacks.ShowCompilerLogsPane();
		}
		catch (Exception exception)
		{
			DarkMessageBox.Show(_promptOwner, exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	public void ShowDocumentation()
		=> OpenPathIfExists(GameFlowDocumentationPaths.MainManualPath);

	public bool IsScriptDefined(string levelName)
		=> _callbacks.IsLevelScriptDefined(levelName);

	public void RenameLevel(string oldName, string newName)
	{
		string scriptFilePath = PathHelper.GetScriptFilePath(_scriptRootDirectoryPath, TRVersion.Game.TR2);
		SilentActionFileState scriptFileState = _silentActionService.CaptureFileState(scriptFilePath);

		_callbacks.RenameRequestedLevelScript(oldName, newName);
		_silentActionService.Complete(true, _silentActionService.CreateCompletion(scriptFileState));
	}

	private static bool IsSilentAction(IIDEEvent ideEvent)
	{
		return ideEvent is IDE.ScriptEditor_AppendScriptEvent
			or IDE.ScriptEditor_ScriptPresenceCheckEvent
			or IDE.ScriptEditor_RenameLevelEvent;
	}

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
