#nullable enable

using System;
using TombIDE.Shared;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;
using TombIDE.ScriptingStudio.FileExplorer;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class WorkbenchCommandRouter
{
	private readonly ScriptingWorkspaceProfile _workspaceProfile;
	private readonly IEditorDocumentController _documentController;
	private readonly WorkbenchPaneCoordinator _paneCoordinator;
	private readonly WorkbenchDialogCoordinator _dialogCoordinator;
	private readonly WorkbenchCodeNavigationCoordinator _codeNavigationCoordinator;
	private readonly IMenuService _menuService;
	private readonly IToolBarService _toolBarService;
	private readonly IStatusBarService _statusBarService;
	private readonly IMessageService _messageService;
	private readonly Action _updateUi;
	private readonly Action? _buildAction;
	private readonly Action? _showDocumentationAction;

	public WorkbenchCommandRouter(
		ScriptingWorkspaceProfile workspaceProfile,
		IEditorDocumentController documentController,
		WorkbenchPaneCoordinator paneCoordinator,
		WorkbenchDialogCoordinator dialogCoordinator,
		WorkbenchCodeNavigationCoordinator codeNavigationCoordinator,
		IMenuService menuService,
		IToolBarService toolBarService,
		IStatusBarService statusBarService,
		IMessageService messageService,
		Action updateUi,
		Action? buildAction,
		Action? showDocumentationAction)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(paneCoordinator);
		ArgumentNullException.ThrowIfNull(dialogCoordinator);
		ArgumentNullException.ThrowIfNull(codeNavigationCoordinator);
		ArgumentNullException.ThrowIfNull(menuService);
		ArgumentNullException.ThrowIfNull(toolBarService);
		ArgumentNullException.ThrowIfNull(statusBarService);
		ArgumentNullException.ThrowIfNull(messageService);
		ArgumentNullException.ThrowIfNull(updateUi);

		_workspaceProfile = workspaceProfile;
		_documentController = documentController;
		_paneCoordinator = paneCoordinator;
		_dialogCoordinator = dialogCoordinator;
		_codeNavigationCoordinator = codeNavigationCoordinator;
		_menuService = menuService;
		_toolBarService = toolBarService;
		_statusBarService = statusBarService;
		_messageService = messageService;
		_updateUi = updateUi;
		_buildAction = buildAction;
		_showDocumentationAction = showDocumentationAction;
	}

	public bool TryExecuteCommand(UICommand command)
	{
		switch (command)
		{
			case UICommand.NewFile:
				_paneCoordinator.CreateNewFile();
				_updateUi();
				return true;

			case UICommand.Find:
				_dialogCoordinator.ShowFindReplace(_documentController.CurrentEditor);
				_updateUi();
				return true;

			case UICommand.Build when _workspaceProfile.SupportsBuild && _buildAction is not null:
				_buildAction();
				_updateUi();
				return true;

			case UICommand.ScriptingDocumentation when _workspaceProfile.SupportsDocumentation && _showDocumentationAction is not null:
				_showDocumentationAction();
				_updateUi();
				return true;

			case UICommand.About:
				_dialogCoordinator.ShowAbout();
				_updateUi();
				return true;

			case UICommand.GoToDefinition:
				_codeNavigationCoordinator.ExecuteGoToDefinition();
				_updateUi();
				return true;

			case UICommand.FindReferences:
				_codeNavigationCoordinator.ExecuteFindReferences();
				_updateUi();
				return true;

			case UICommand.RenameSymbol:
				_codeNavigationCoordinator.ExecuteRenameSymbol();
				_updateUi();
				return true;

			default:
				if (_paneCoordinator.TryTogglePane(command))
				{
					_updateUi();
					return true;
				}
				break;
		}

		if (!CanExecuteCommand(command))
			return false;

		switch (command)
		{
			case UICommand.Save:
				_documentController.SaveFile();
				break;

			case UICommand.SaveAs:
				_documentController.SaveFileAs();
				break;

			case UICommand.SaveAll:
				_documentController.SaveAll();
				break;

			case UICommand.Undo:
				_documentController.CurrentEditor?.Undo();
				break;

			case UICommand.Redo:
				_documentController.CurrentEditor?.Redo();
				break;

			case UICommand.Cut:
				_documentController.CurrentEditor?.Cut();
				break;

			case UICommand.Copy:
				_documentController.CurrentEditor?.Copy();
				break;

			case UICommand.Paste:
				_documentController.CurrentEditor?.Paste();
				break;

			case UICommand.SelectAll:
				_documentController.CurrentEditor?.SelectAll();
				break;

			default:
				if (!TryExecuteDocumentCommand(command))
					return false;
				break;
		}

		_updateUi();
		return true;
	}

	public void ExecuteCommand(UICommand command)
		=> _ = TryExecuteCommand(command);

	public bool CanExecuteCommand(UICommand command) => command switch
	{
		UICommand.NewFile => true,
		UICommand.Find => true,
		UICommand.About => true,
		UICommand.Settings => true,
		UICommand.RestoreDefaultLayout => true,
		UICommand.Build => _workspaceProfile.SupportsBuild,
		UICommand.ScriptingDocumentation => _workspaceProfile.SupportsDocumentation,
		UICommand.ContentExplorer or UICommand.FileExplorer or UICommand.ReferenceBrowser
			or UICommand.CompilerLogs or UICommand.SearchResults => true,
		UICommand.LuaDiagnostics
			=> _workspaceProfile.SupportsView(command),
		UICommand.LuaReferencesResults
			=> _workspaceProfile.SupportsLua && _workspaceProfile.SupportsView(command),
		UICommand.GoToDefinition => _documentController.CurrentEditor is not null,
		UICommand.FindReferences => _documentController.CurrentEditor is ClassicScriptEditor
			|| _documentController.CurrentEditor is LuaEditor && _codeNavigationCoordinator.SupportsReferences,
		UICommand.RenameSymbol => _documentController.CurrentEditor is LuaEditor
			&& _codeNavigationCoordinator.SupportsRename,
		UICommand.Save => _documentController.CurrentEditor?.IsContentChanged == true,
		UICommand.SaveAs => _documentController.CurrentEditor is not null,
		UICommand.SaveAll => !_documentController.IsEveryDocumentSaved(),
		UICommand.Undo => _documentController.CurrentEditor?.CanUndo == true,
		UICommand.Redo => _documentController.CurrentEditor?.CanRedo == true,
		UICommand.Cut or UICommand.Copy or UICommand.Paste or UICommand.SelectAll => _documentController.CurrentEditor is not null,
		_ => CanExecuteBuiltInDocumentCommand(command)
	};

	public void UpdateCommandStates()
	{
		_menuService.UpdateCommandEnabledStates(CanExecuteCommand);
		_toolBarService.UpdateCommandEnabledStates(CanExecuteCommand);
		UpdateUndoRedoCommandPresentation();
	}

	public void UpdateDocumentCommandSurface(IEditorControl? editor, ScriptingDocumentContext documentContext)
	{
		DocumentMode documentMode = documentContext.Registration?.DocumentMode ?? DocumentMode.None;
		IStudioDocumentCommandSurfaceProvider? commandSurfaceProvider = documentContext.Registration?.Contributions.CommandSurfaceProvider;

		if (editor is null)
		{
			_menuService.SetDocumentCommandSurface(documentMode, []);
			_toolBarService.SetDocumentCommandSurface(documentMode, []);
			_statusBarService.SetStatusStripContext(null, documentMode, []);
			return;
		}

		_menuService.SetDocumentCommandSurface(
			documentMode,
			commandSurfaceProvider?.GetMenuStripItems(editor) ?? []);
		_toolBarService.SetDocumentCommandSurface(
			documentMode,
			commandSurfaceProvider?.GetToolStripItems(editor) ?? []);
		_statusBarService.SetStatusStripContext(
			editor,
			documentMode,
			documentContext.Registration?.Contributions.StatusStripProvider?.GetSegments(editor) ?? []);
	}

	public void UpdateUndoRedoCommandPresentation()
	{
		bool canUndo = _documentController.CurrentEditor?.CanUndo == true;
		_menuService.SetCommandText(UICommand.Undo, canUndo ? Strings.Default.Undo : Strings.Default.CantUndo);
		_toolBarService.SetCommandText(UICommand.Undo, canUndo ? Strings.Default.Undo : Strings.Default.CantUndo);
		_toolBarService.SetCommandToolTip(UICommand.Undo, canUndo ? Strings.Default.Undo : Strings.Default.CantUndo);

		bool canRedo = _documentController.CurrentEditor?.CanRedo == true;
		_menuService.SetCommandText(UICommand.Redo, canRedo ? Strings.Default.Redo : Strings.Default.CantRedo);
		_toolBarService.SetCommandText(UICommand.Redo, canRedo ? Strings.Default.Redo : Strings.Default.CantRedo);
		_toolBarService.SetCommandToolTip(UICommand.Redo, canRedo ? Strings.Default.Redo : Strings.Default.CantRedo);
	}

	private bool CanExecuteBuiltInDocumentCommand(UICommand command)
	{
		if (_documentController.CurrentEditor is TextEditorBase)
		{
			switch (command)
			{
				case UICommand.TabsToSpaces:
				case UICommand.SpacesToTabs:
				case UICommand.Reindent:
				case UICommand.TrimWhiteSpace:
				case UICommand.ToggleComment:
				case UICommand.CommentOut:
				case UICommand.Uncomment:
				case UICommand.ToggleBookmark:
				case UICommand.PrevBookmark:
				case UICommand.NextBookmark:
				case UICommand.ClearBookmarks:
					return true;
			}
		}

		if (_documentController.CurrentEditor is IStringSectionNavigator)
		{
			switch (command)
			{
				case UICommand.PrevSection:
				case UICommand.NextSection:
				case UICommand.ClearString:
				case UICommand.RemoveLastString:
					return true;
			}
		}

		return false;
	}

	private bool TryExecuteDocumentCommand(UICommand command)
	{
		if (_documentController.CurrentEditor is TextEditorBase textEditor)
		{
			switch (command)
			{
				case UICommand.TabsToSpaces:
					textEditor.ConvertTabsToSpaces();
					return true;

				case UICommand.SpacesToTabs:
					textEditor.ConvertSpacesToTabs();
					return true;

				case UICommand.Reindent:
					textEditor.TidyCode();
					return true;

				case UICommand.TrimWhiteSpace:
					textEditor.TidyCode(true);
					return true;

				case UICommand.ToggleComment:
					textEditor.ToggleCommentLines();
					return true;

				case UICommand.CommentOut:
					textEditor.CommentOutLines();
					return true;

				case UICommand.Uncomment:
					textEditor.UncommentLines();
					return true;

				case UICommand.ToggleBookmark:
					textEditor.ToggleBookmark();
					return true;

				case UICommand.PrevBookmark:
					textEditor.GoToPrevBookmark();
					return true;

				case UICommand.NextBookmark:
					textEditor.GoToNextBookmark();
					return true;

				case UICommand.ClearBookmarks:
					textEditor.ClearAllBookmarks(() => _messageService.ShowConfirmation("Are you sure you want to clear all bookmarks from the current document?"));
					return true;
			}
		}

		if (_documentController.CurrentEditor is IStringSectionNavigator navigator)
		{
			switch (command)
			{
				case UICommand.PrevSection:
					navigator.GoToPreviousSection();
					if (navigator.CurrentSectionName is string previousSectionName)
						_paneCoordinator.SelectOutlineNode(previousSectionName);
					return true;

				case UICommand.NextSection:
					navigator.GoToNextSection();
					if (navigator.CurrentSectionName is string nextSectionName)
						_paneCoordinator.SelectOutlineNode(nextSectionName);
					return true;

				case UICommand.ClearString:
					navigator.ClearSelectedString();
					return true;

				case UICommand.RemoveLastString:
					navigator.RemoveLastString();
					return true;
			}
		}

		return false;
	}
}
