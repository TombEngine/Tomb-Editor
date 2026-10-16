#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared.Docking;
using TombLib.LevelData;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.WorkspaceProfile;

public enum ScriptingWorkspaceKind
{
	ClassicScript,
	GameFlowScript,
	TRX,
	Lua
}

public enum ScriptingSettingsPageKind
{
	ClassicScript,
	GameFlowScript,
	TRX,
	Lua
}

public sealed class ScriptingWorkspaceSettingsPage
{
	public ScriptingWorkspaceSettingsPage(ScriptingSettingsPageKind kind, string title)
	{
		Kind = kind;
		Title = title ?? throw new ArgumentNullException(nameof(title));
	}

	public ScriptingSettingsPageKind Kind { get; }

	public string Title { get; }
}

public sealed class ScriptingWorkspaceViewContribution
{
	public ScriptingWorkspaceViewContribution(UICommand command)
	{
		Command = command;
	}

	public UICommand Command { get; }
}

public sealed class ScriptingWorkspaceProfile
{
	private readonly ScriptingWorkspaceLayoutPersistence _layoutPersistence;

	public ScriptingWorkspaceProfile(
		ScriptingWorkspaceKind kind,
		TRVersion.Game gameVersion,
		IReadOnlyList<ScriptingDocumentRegistration> documentRegistrations,
		string initialFilePath,
		IReadOnlyList<ScriptingWorkspaceViewContribution> viewContributions,
		IReadOnlyList<StudioStatusStripSegment> statusStripSegments,
		IReadOnlyList<ScriptingWorkspaceSettingsPage> settingsPages,
		IReadOnlyList<StudioToolStripItem> menuStripContributions,
		IReadOnlyList<StudioToolStripItem> toolStripContributions,
		string fileExplorerFilter,
		string fileExplorerExcludedDirectoryFilter,
		string commentPrefix,
		bool supportsBuild,
		bool supportsDocumentation,
		ScriptingWorkspaceLayoutPersistence layoutPersistence,
		bool supportsLuaActivation = false)
	{
		Kind = kind;
		GameVersion = gameVersion;
		DocumentRegistrations = documentRegistrations?.ToArray() ?? Array.Empty<ScriptingDocumentRegistration>();
		AllowedDocumentModes = DocumentRegistrations
			.SelectMany(registration => registration.SupportedDocumentModes)
			.Distinct()
			.ToArray();
		InitialFilePath = initialFilePath ?? string.Empty;
		ViewContributions = viewContributions ?? Array.Empty<ScriptingWorkspaceViewContribution>();
		StatusStripSegments = statusStripSegments ?? Array.Empty<StudioStatusStripSegment>();
		SettingsPages = settingsPages ?? Array.Empty<ScriptingWorkspaceSettingsPage>();
		MenuStripContributions = menuStripContributions ?? Array.Empty<StudioToolStripItem>();
		ToolStripContributions = toolStripContributions ?? Array.Empty<StudioToolStripItem>();
		FileExplorerFilter = fileExplorerFilter ?? string.Empty;
		FileExplorerExcludedDirectoryFilter = fileExplorerExcludedDirectoryFilter ?? string.Empty;
		CommentPrefix = commentPrefix ?? string.Empty;
		SupportsBuild = supportsBuild;
		SupportsDocumentation = supportsDocumentation;
		SupportsLuaActivation = supportsLuaActivation;
		SupportsLua = AllowedDocumentModes.Contains(DocumentMode.Lua) || kind == ScriptingWorkspaceKind.Lua;
		_layoutPersistence = layoutPersistence;
		DefaultLayout = layoutPersistence.DefaultLayout;
	}

	public ScriptingWorkspaceKind Kind { get; }

	public TRVersion.Game GameVersion { get; }

	public IReadOnlyList<DocumentMode> AllowedDocumentModes { get; }

	public IReadOnlyList<ScriptingDocumentRegistration> DocumentRegistrations { get; }

	public string InitialFilePath { get; }

	public IReadOnlyList<ScriptingWorkspaceSettingsPage> SettingsPages { get; }

	public IReadOnlyList<ScriptingWorkspaceViewContribution> ViewContributions { get; }

	public IReadOnlyList<StudioStatusStripSegment> StatusStripSegments { get; }

	public IReadOnlyList<StudioToolStripItem> MenuStripContributions { get; }

	public IReadOnlyList<StudioToolStripItem> ToolStripContributions { get; }

	public string FileExplorerFilter { get; }

	public string FileExplorerExcludedDirectoryFilter { get; }

	public string CommentPrefix { get; }

	public bool SupportsBuild { get; }

	public bool SupportsDocumentation { get; }

	public bool SupportsLuaActivation { get; }

	public bool SupportsLua { get; }

	public DockPanelState DefaultLayout { get; }

	public void RegisterEditors(IEditorDocumentController documentController)
	{
		ArgumentNullException.ThrowIfNull(documentController);

		foreach (ScriptingDocumentRegistration registration in DocumentRegistrations)
			documentController.RegisterDocument(registration);
	}

	public DockPanelState LoadDockPanelState()
		=> _layoutPersistence.LoadDockPanelState();

	public string LoadAvalonDockLayoutXml()
		=> _layoutPersistence.LoadAvalonDockLayoutXml();

	public void SaveAvalonDockLayoutXml(string layoutXml)
		=> _layoutPersistence.SaveAvalonDockLayoutXml(layoutXml);

	public bool SupportsView(UICommand command)
		=> ViewContributions.Any(contribution => contribution.Command == command);

}
