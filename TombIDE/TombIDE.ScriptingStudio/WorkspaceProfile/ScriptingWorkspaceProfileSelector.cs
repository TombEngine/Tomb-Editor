#nullable enable

using Nickelony.IDEKit.IntelliSense.DocumentSymbols;
using System;
using System.Collections.Generic;
using System.Linq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.DocumentOutline;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;
using TombIDE.ScriptingStudio.Helpers;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.Docking;
using TombIDE.Shared.Messaging.Scripting;
using TombIDE.Shared.SharedClasses;
using TombLib.LevelData;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Documents;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.WorkspaceProfile;

public static class ScriptingWorkspaceProfileSelector
{
	public static ScriptingWorkspaceProfile Create(IScriptingProjectContext projectContext, IScriptingStudioShellSettingsStore settingsStore, ClassicScriptLanguageServices classicScriptServices, GameFlowLanguageServices gameFlowServices, TRXLanguageServices trxServices)
	{
		ArgumentNullException.ThrowIfNull(projectContext);
		ArgumentNullException.ThrowIfNull(settingsStore);
		ArgumentNullException.ThrowIfNull(classicScriptServices);
		ArgumentNullException.ThrowIfNull(gameFlowServices);
		ArgumentNullException.ThrowIfNull(trxServices);

		return projectContext.Project.GameVersion switch
		{
			TRVersion.Game.TR4 or TRVersion.Game.TRNG => CreateClassicScriptProfile(projectContext, settingsStore, classicScriptServices, settingsStore.IsLuaEnabled(ScriptingWorkspaceKind.ClassicScript)),
			TRVersion.Game.TR2 or TRVersion.Game.TR3 => CreateGameFlowProfile(projectContext, settingsStore, gameFlowServices),
			TRVersion.Game.TR1 or TRVersion.Game.TR2X or TRVersion.Game.TR3X => CreateTrxProfile(projectContext, settingsStore, trxServices, settingsStore.IsLuaEnabled(ScriptingWorkspaceKind.TRX)),
			TRVersion.Game.TombEngine => CreateLuaProfile(projectContext, settingsStore),
			_ => throw new NotSupportedException($"Unsupported scripting workspace game version: {projectContext.Project.GameVersion}.")
		};
	}

	private static ScriptingWorkspaceProfile CreateClassicScriptProfile(IScriptingProjectContext projectContext, IScriptingStudioShellSettingsStore settingsStore, ClassicScriptLanguageServices languageServices, bool supportsLua)
	{
		return new ScriptingWorkspaceProfile(
			ScriptingWorkspaceKind.ClassicScript,
			projectContext.Project.GameVersion,
			CreateClassicScriptRegistrations(languageServices, supportsLua),
			PathHelper.GetScriptFilePath(projectContext.ScriptRootDirectoryPath, TRVersion.Game.TR4),
			CreateClassicScriptViewContributions(supportsLua),
			CreateSharedStatusStripSegments(),
			CreateSettingsPages(
				new(ScriptingSettingsPageKind.ClassicScript, "TR4 / TRNG Script"),
				supportsLua),
			ScriptingWorkspaceCommandSurfaceFactory.CreateMenuStripContributions(ScriptingWorkspaceKind.ClassicScript),
			ScriptingWorkspaceCommandSurfaceFactory.CreateToolStripContributions(ScriptingWorkspaceKind.ClassicScript),
			supportsLua ? "*.txt|*.lua" : "*.txt",
			string.Empty,
			";",
			true,
			true,
			CreateLayoutPersistence(settingsStore, ScriptingWorkspaceKind.ClassicScript, DefaultLayouts.ClassicScriptLayout),
			supportsLuaActivation: true);
	}

	private static ScriptingWorkspaceProfile CreateGameFlowProfile(IScriptingProjectContext projectContext, IScriptingStudioShellSettingsStore settingsStore, GameFlowLanguageServices gameFlowServices)
	{
		return new ScriptingWorkspaceProfile(
			ScriptingWorkspaceKind.GameFlowScript,
			projectContext.Project.GameVersion,
			CreateGameFlowRegistrations(gameFlowServices),
			PathHelper.GetScriptFilePath(projectContext.ScriptRootDirectoryPath, TRVersion.Game.TR2),
			CreateViewContributions(
				UICommand.ContentExplorer,
				UICommand.FileExplorer,
				UICommand.CompilerLogs,
				UICommand.SearchResults,
				UICommand.ToolStrip,
				UICommand.StatusStrip),
			CreateSharedStatusStripSegments(),
			[new(ScriptingSettingsPageKind.GameFlowScript, "TR2 / TR3 Script")],
			ScriptingWorkspaceCommandSurfaceFactory.CreateMenuStripContributions(ScriptingWorkspaceKind.GameFlowScript),
			ScriptingWorkspaceCommandSurfaceFactory.CreateToolStripContributions(ScriptingWorkspaceKind.GameFlowScript),
			"*.txt",
			string.Empty,
			"//",
			true,
			true,
			CreateLayoutPersistence(settingsStore, ScriptingWorkspaceKind.GameFlowScript, DefaultLayouts.GameFlowScriptLayout),
			supportsLuaActivation: false);
	}

	private static ScriptingWorkspaceProfile CreateTrxProfile(IScriptingProjectContext projectContext, IScriptingStudioShellSettingsStore settingsStore, TRXLanguageServices trxServices, bool supportsLua)
	{
		return new ScriptingWorkspaceProfile(
			ScriptingWorkspaceKind.TRX,
			projectContext.Project.GameVersion,
			CreateTrxRegistrations(trxServices, supportsLua),
			PathHelper.GetScriptFilePath(projectContext.ScriptRootDirectoryPath, projectContext.Project.GameVersion),
			CreateTrxViewContributions(supportsLua),
			CreateSharedStatusStripSegments(),
			CreateSettingsPages(new(ScriptingSettingsPageKind.TRX, "TRX Script"), supportsLua),
			ScriptingWorkspaceCommandSurfaceFactory.CreateMenuStripContributions(ScriptingWorkspaceKind.TRX),
			ScriptingWorkspaceCommandSurfaceFactory.CreateToolStripContributions(ScriptingWorkspaceKind.TRX),
			supportsLua ? "*.json5|*.lua" : "*.json5",
			string.Empty,
			"//",
			false,
			false,
			CreateLayoutPersistence(settingsStore, ScriptingWorkspaceKind.TRX, DefaultLayouts.TRXLayout),
			supportsLuaActivation: true);
	}

	private static ScriptingWorkspaceProfile CreateLuaProfile(IScriptingProjectContext projectContext, IScriptingStudioShellSettingsStore settingsStore)
	{
		return new ScriptingWorkspaceProfile(
			ScriptingWorkspaceKind.Lua,
			projectContext.Project.GameVersion,
			CreateLuaRegistrations(),
			PathHelper.GetScriptFilePath(projectContext.ScriptRootDirectoryPath, TRVersion.Game.TombEngine),
			CreateViewContributions(
				UICommand.ContentExplorer,
				UICommand.FileExplorer,
				UICommand.SearchResults,
				UICommand.LuaDiagnostics,
				UICommand.LuaReferencesResults,
				UICommand.ToolStrip,
				UICommand.StatusStrip),
			CreateSharedStatusStripSegments(),
			[new(ScriptingSettingsPageKind.Lua, "Lua")],
			ScriptingWorkspaceCommandSurfaceFactory.CreateMenuStripContributions(ScriptingWorkspaceKind.Lua),
			ScriptingWorkspaceCommandSurfaceFactory.CreateToolStripContributions(ScriptingWorkspaceKind.Lua),
			"*.lua",
			"Scripts\\Engine",
			"--",
			false,
			false,
			CreateLayoutPersistence(settingsStore, ScriptingWorkspaceKind.Lua, DefaultLayouts.LuaLayout),
			supportsLuaActivation: false);
	}

	private static ScriptingWorkspaceLayoutPersistence CreateLayoutPersistence(
		IScriptingStudioShellSettingsStore settingsStore,
		ScriptingWorkspaceKind workspaceKind,
		DockPanelState defaultLayout)
		=> new(
			defaultLayout,
			() => LoadLayoutSettings(settingsStore, workspaceKind, defaultLayout).DockPanelState,
			() => LoadLayoutSettings(settingsStore, workspaceKind, defaultLayout).AvalonDockLayoutXml,
			xml => SaveLayoutXml(settingsStore, workspaceKind, defaultLayout, xml));

	private static ScriptingStudioShellWorkspaceSettings LoadLayoutSettings(
		IScriptingStudioShellSettingsStore settingsStore,
		ScriptingWorkspaceKind workspaceKind,
		DockPanelState defaultLayout)
		=> settingsStore.Load(workspaceKind, defaultLayout);

	private static void SaveLayoutXml(
		IScriptingStudioShellSettingsStore settingsStore,
		ScriptingWorkspaceKind workspaceKind,
		DockPanelState defaultLayout,
		string xml)
	{
		ScriptingStudioShellWorkspaceSettings settings = settingsStore.Load(workspaceKind, defaultLayout);
		settings.AvalonDockLayoutXml = xml ?? string.Empty;
		settingsStore.Save(workspaceKind, settings);
	}

	private static ScriptingWorkspaceViewContribution[] CreateViewContributions(params UICommand[] commands)
		=> [.. commands.Select(static command => new ScriptingWorkspaceViewContribution(command))];

	private static StudioStatusStripSegment[] CreateSharedStatusStripSegments() =>
	[
		StudioStatusStripSegment.CaretPosition,
		StudioStatusStripSegment.SelectionLength,
		StudioStatusStripSegment.Zoom
	];

	private static IReadOnlyList<ScriptingDocumentRegistration> CreateClassicScriptRegistrations(
		ClassicScriptLanguageServices languageServices,
		bool supportsLua)
	{
		DocumentContributionSet contributions = CreateDocumentContributions(languageServices, null, null);
		var registrations = new List<ScriptingDocumentRegistration>
		{
			new(
				EditorType.Text,
				DocumentMode.ClassicScript,
				FileHelper.IsTextFile,
				filePath => !FileHelper.IsStringFile(filePath, languageServices.LineService),
				engineVersion => new ClassicScriptEditor(engineVersion, languageServices),
				contributions.ClassicScript),
			new(
				EditorType.Strings,
				DocumentMode.Strings,
				FileHelper.IsTextFile,
				filePath => FileHelper.GetClassicScriptFileKind(filePath, languageServices.LineService) == ClassicScriptFileKind.Strings,
				static engineVersion => new StringEditorView(engineVersion),
				contributions.Strings),
			new(
				EditorType.Text,
				DocumentMode.ClassicScript,
				static _ => false,
				static _ => false,
				engineVersion => new ClassicScriptEditor(engineVersion, languageServices),
				contributions.ClassicScript,
				isFallback: true,
				supportedDocumentModes: [DocumentMode.PlainText])
		};

		if (supportsLua)
			registrations.Add(CreateLuaRegistration(contributions.Lua));

		return registrations;
	}

	private static IReadOnlyList<ScriptingDocumentRegistration> CreateGameFlowRegistrations(GameFlowLanguageServices languageServices)
	{
		DocumentContributionSet contributions = CreateDocumentContributions(null, languageServices, null);
		return
		[
			new(
				EditorType.Text,
				DocumentMode.GameFlowScript,
				FileHelper.IsTextFile,
				static _ => true,
				engineVersion => new GameFlowEditor(engineVersion, languageServices),
				contributions.GameFlowScript),
			new(
				EditorType.Text,
				DocumentMode.GameFlowScript,
				static _ => false,
				static _ => false,
				engineVersion => new GameFlowEditor(engineVersion, languageServices),
				contributions.GameFlowScript,
				isFallback: true,
				supportedDocumentModes: [DocumentMode.PlainText])
		];
	}

	private static IReadOnlyList<ScriptingDocumentRegistration> CreateTrxRegistrations(TRXLanguageServices languageServices, bool supportsLua)
	{
		DocumentContributionSet contributions = CreateDocumentContributions(null, null, languageServices);
		var registrations = new List<ScriptingDocumentRegistration>
		{
			new(
				EditorType.Text,
				DocumentMode.TRX,
				FileHelper.IsJson5File,
				static _ => true,
				engineVersion => new TRXEditor(engineVersion, languageServices),
				contributions.TRX)
		};

		if (supportsLua)
			registrations.Add(CreateLuaRegistration(contributions.Lua));

		return registrations;
	}

	private static IReadOnlyList<ScriptingDocumentRegistration> CreateLuaRegistrations()
		=> [CreateLuaRegistration(CreateDocumentContributions(null, null, null).Lua)];

	private static ScriptingDocumentRegistration CreateLuaRegistration(ScriptingDocumentContributions contributions)
		=> new(
			EditorType.Text,
			DocumentMode.Lua,
			FileHelper.IsLuaFile,
			static _ => true,
			static engineVersion => new LuaEditor(engineVersion),
			contributions);

	private static DocumentContributionSet CreateDocumentContributions(
		ClassicScriptLanguageServices? classicScriptServices,
		GameFlowLanguageServices? gameFlowServices,
		TRXLanguageServices? trxServices)
	{
		var outlineFactory = classicScriptServices is not null || gameFlowServices is not null || trxServices is not null
			? new DocumentOutlineNodesProviderFactory(
				classicScriptServices,
				gameFlowServices,
				trxServices)
			: null;

		return new DocumentContributionSet(
			CreateContribution(
				ScriptingSettingsPageKind.ClassicScript,
				ScriptingDocumentConfigurationKind.ClassicScript,
				TypedDocumentCommandSurfaceProvider.CreateClassicScript(),
				outlineFactory is null ? null : () => outlineFactory.CreateClassicScript(),
				new ClassicScriptDocumentStatusStripProvider()),
			CreateContribution(
				ScriptingSettingsPageKind.ClassicScript,
				ScriptingDocumentConfigurationKind.ClassicScript,
				TypedDocumentCommandSurfaceProvider.CreateStrings(),
				outlineFactory is null ? null : () => outlineFactory.CreateStrings()),
			CreateContribution(
				ScriptingSettingsPageKind.GameFlowScript,
				ScriptingDocumentConfigurationKind.GameFlowScript,
				TypedDocumentCommandSurfaceProvider.CreateGameFlowScript(),
				outlineFactory is null ? null : () => outlineFactory.CreateGameFlowScript()),
			CreateContribution(
				ScriptingSettingsPageKind.TRX,
				ScriptingDocumentConfigurationKind.TRX,
				TypedDocumentCommandSurfaceProvider.CreateTrx(),
				outlineFactory is null ? null : () => outlineFactory.CreateTrx()),
			CreateContribution(
				ScriptingSettingsPageKind.Lua,
				ScriptingDocumentConfigurationKind.Lua,
				TypedDocumentCommandSurfaceProvider.CreateLua(),
				null),
			new(
				null,
				ScriptingDocumentConfigurationKind.None,
				TypedDocumentCommandSurfaceProvider.CreatePlainText()));
	}

	private static ScriptingDocumentContributions CreateContribution(
		ScriptingSettingsPageKind settingsPageKind,
		ScriptingDocumentConfigurationKind configurationKind,
		IStudioDocumentCommandSurfaceProvider commandSurfaceProvider,
		Func<ITextDocumentSymbolProvider?>? outlineProviderFactory,
		IStudioDocumentStatusStripProvider? statusStripProvider = null)
	{
		return new ScriptingDocumentContributions(
			settingsPageKind,
			configurationKind,
			commandSurfaceProvider,
			outlineProviderFactory,
			statusStripProvider);
	}

	private sealed record DocumentContributionSet(
		ScriptingDocumentContributions ClassicScript,
		ScriptingDocumentContributions Strings,
		ScriptingDocumentContributions GameFlowScript,
		ScriptingDocumentContributions TRX,
		ScriptingDocumentContributions Lua,
		ScriptingDocumentContributions PlainText);

	private static IReadOnlyList<ScriptingWorkspaceSettingsPage> CreateSettingsPages(ScriptingWorkspaceSettingsPage primaryPage, bool supportsLua)
		=> supportsLua
			? [primaryPage, new(ScriptingSettingsPageKind.Lua, "Lua")]
			: [primaryPage];

	private static UICommand[] CreateLuaViewCommands()
		=> [UICommand.LuaReferencesResults];

	private static IReadOnlyList<ScriptingWorkspaceViewContribution> CreateTrxViewContributions(bool supportsLua)
	{
		var commands = new[]
		{
			UICommand.ContentExplorer,
			UICommand.FileExplorer,
			UICommand.SearchResults,
			UICommand.LuaDiagnostics,
			UICommand.ToolStrip,
			UICommand.StatusStrip
		};

		return CreateViewContributions([.. commands, .. (supportsLua ? CreateLuaViewCommands() : [])]);
	}

	private static IReadOnlyList<ScriptingWorkspaceViewContribution> CreateClassicScriptViewContributions(bool supportsLua)
	{
		var commands = new[]
		{
			UICommand.ContentExplorer,
			UICommand.FileExplorer,
			UICommand.ReferenceBrowser,
			UICommand.CompilerLogs,
			UICommand.SearchResults,
			UICommand.LuaDiagnostics,
			UICommand.ToolStrip,
			UICommand.StatusStrip
		};

		var viewCommands = new List<UICommand>(commands);
		if (supportsLua)
		{
			viewCommands.AddRange(CreateLuaViewCommands());
		}

		return CreateViewContributions(viewCommands.ToArray());
	}
}
