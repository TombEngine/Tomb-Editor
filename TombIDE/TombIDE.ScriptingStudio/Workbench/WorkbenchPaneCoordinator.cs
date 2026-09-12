#nullable enable

using ICSharpCode.AvalonEdit.Document;
using Nickelony.IDEKit.Core.FindReplace;
using Nickelony.IDEKit.IntelliSense.Diagnostics;
using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.Build;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Diagnostics;
using TombIDE.ScriptingStudio.DocumentOutline;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class WorkbenchPaneCoordinator
{
	private readonly PaneCatalog _paneCatalog;
	private readonly IAvalonDockHost _dockHost;
	private readonly IPaneHostService _paneHostService;
	private readonly LuaTrackedDocumentStateService? _luaTrackedDocumentStateService;

	public WorkbenchPaneCoordinator(
		PaneCatalog paneCatalog,
		IAvalonDockHost dockHost,
		IPaneHostService paneHostService,
		LuaTrackedDocumentStateService? luaTrackedDocumentStateService)
	{
		ArgumentNullException.ThrowIfNull(paneCatalog);
		ArgumentNullException.ThrowIfNull(dockHost);
		ArgumentNullException.ThrowIfNull(paneHostService);

		_paneCatalog = paneCatalog;
		_dockHost = dockHost;
		_paneHostService = paneHostService;
		_luaTrackedDocumentStateService = luaTrackedDocumentStateService;
	}

	public void UpdateActiveDocument(ScriptingDocumentContext documentContext)
	{
		ArgumentNullException.ThrowIfNull(documentContext);

		IEditorControl? currentEditor = documentContext.Editor;
		DocumentMode documentMode = documentContext.Registration?.DocumentMode ?? DocumentMode.None;

		if (_paneCatalog.GetPane<DocumentOutlineToolWindow>(UICommand.ContentExplorer) is DocumentOutlineToolWindow documentOutline)
		{
			documentOutline.OutlineProviderFactory = documentContext.Registration?.Contributions.OutlineProviderFactory;
			documentOutline.EditorControl = currentEditor;
		}

		if (currentEditor is LuaEditor luaEditor)
		{
			ShowLuaDiagnostics(
				luaEditor.FilePath,
				luaEditor.Document,
				_luaTrackedDocumentStateService?.GetDiagnostics(luaEditor.FilePath) ?? []);
		}
		else if (documentMode is not (DocumentMode.ClassicScript or DocumentMode.TRX))
			ShowNoActiveDocument();
	}

	public void ShowTextEditorDiagnostics(TextEditorBase textEditor)
	{
		ArgumentNullException.ThrowIfNull(textEditor);

		if (_paneCatalog.GetPane<TextDiagnosticsToolWindow>(UICommand.LuaDiagnostics) is TextDiagnosticsToolWindow diagnostics)
			diagnostics.ShowDiagnostics(textEditor.FilePath, textEditor.Document, textEditor.Diagnostics);
	}

	public void ShowLuaDiagnostics(
		string filePath,
		TextDocument document,
		IReadOnlyList<TextEditorDiagnostic> diagnostics)
	{
		ArgumentNullException.ThrowIfNull(filePath);
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(diagnostics);

		if (_paneCatalog.GetPane<TextDiagnosticsToolWindow>(UICommand.LuaDiagnostics) is TextDiagnosticsToolWindow luaDiagnostics)
			luaDiagnostics.ShowDiagnostics(filePath, document, diagnostics);
	}

	public void ShowNoActiveDocument()
		=> _paneCatalog.GetPane<TextDiagnosticsToolWindow>(UICommand.LuaDiagnostics)?.ShowNoActiveDocument();

	public void UpdatePaneVisibilityChecks()
		=> _paneCatalog.UpdatePaneVisibilityChecks(_dockHost, _paneHostService);

	public void ShowPane(UICommand command)
		=> _paneCatalog.ShowPane(command, _dockHost);

	public void CreateNewFile()
		=> _paneCatalog.GetPane<FileExplorerToolWindow>(UICommand.FileExplorer)?.CreateNewFile();

	public void SelectOutlineNode(string nodeName)
		=> _paneCatalog.GetPane<DocumentOutlineToolWindow>(UICommand.ContentExplorer)?.SelectNode(nodeName);

	public bool TryTogglePane(UICommand command)
		=> _paneCatalog.TryTogglePane(command, _dockHost, _paneHostService);

	public void ShowCompilerLogsPane()
		=> ShowPane(UICommand.CompilerLogs);

	public void ShowReferenceSearchUnsupported()
		=> _paneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults)?.ShowUnsupported();

	public void ShowReferenceSearchNoActiveDocument()
		=> _paneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults)?.ShowNoActiveDocument();

	public void ShowReferenceSearchLoading()
	{
		ShowPane(UICommand.LuaReferencesResults);
		_paneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults)?.ShowLoading();
	}

	public void ShowReferenceSearchResults(IReadOnlyList<TextReferenceGroup> groups)
	{
		ArgumentNullException.ThrowIfNull(groups);
		_paneCatalog.GetPane<TextReferencesResultsToolWindow>(UICommand.LuaReferencesResults)?.ShowReferences(groups);
	}

	public void UpdateCompilerLogs(string text)
		=> _paneCatalog.GetPane<CompilerLogsToolWindow>(UICommand.CompilerLogs)?.UpdateLogs(text);

	public void ShowFindAllResults(IReadOnlyList<FindReplaceSource> sources)
	{
		ArgumentNullException.ThrowIfNull(sources);

		ShowPane(UICommand.SearchResults);
		_paneCatalog.GetPane<SearchResultsToolWindow>(UICommand.SearchResults)?.UpdateResults(
			new FindReplaceEventArgs(sources));
		UpdatePaneVisibilityChecks();
	}
}
