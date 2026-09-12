#nullable enable

using Nickelony.LanguageServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Forms.ViewModels;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.UI.Presentation;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using Nickelony.IDEKit.AvalonEdit.Documents;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class WorkbenchCodeNavigationCoordinator : IDisposable
{
	private readonly ScriptingWorkspaceProfile _workspaceProfile;
	private readonly IEditorDocumentController _documentController;
	private readonly WorkbenchPaneCoordinator _paneCoordinator;
	private readonly LuaHostServices? _luaHostServices;
	private readonly WorkbenchDialogCoordinator _dialogCoordinator;
	private readonly IMessageService _messageService;
	private readonly ClassicScriptLanguageServices _languageServices;
	private CancellationTokenSource? _referenceSearchCancellation;
	private bool _disposed;

	public WorkbenchCodeNavigationCoordinator(
		ScriptingWorkspaceProfile workspaceProfile,
		IEditorDocumentController documentController,
		WorkbenchPaneCoordinator paneCoordinator,
		LuaHostServices? luaHostServices,
		WorkbenchDialogCoordinator dialogCoordinator,
		IMessageService messageService,
		ClassicScriptLanguageServices languageServices)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);
		ArgumentNullException.ThrowIfNull(documentController);
		ArgumentNullException.ThrowIfNull(paneCoordinator);
		ArgumentNullException.ThrowIfNull(dialogCoordinator);
		ArgumentNullException.ThrowIfNull(messageService);
		ArgumentNullException.ThrowIfNull(languageServices);

		_workspaceProfile = workspaceProfile;
		_documentController = documentController;
		_paneCoordinator = paneCoordinator;
		_luaHostServices = luaHostServices;
		_dialogCoordinator = dialogCoordinator;
		_messageService = messageService;
		_languageServices = languageServices;

		_documentController.CurrentEditorChanged += DocumentController_CurrentEditorChanged;
	}

	public bool SupportsReferences
		=> _luaHostServices?.ReferenceSearchService.SupportsReferences == true;

	public bool SupportsRename
		=> _luaHostServices?.WorkspaceCommandService.SupportsRename == true;

	public void UpdateActiveDocument(ScriptingDocumentContext documentContext)
	{
		ArgumentNullException.ThrowIfNull(documentContext);

		CancelReferenceSearch();

		if (documentContext.Editor is LuaEditor or null)
			_paneCoordinator.ShowReferenceSearchNoActiveDocument();
		else
			_paneCoordinator.ShowReferenceSearchUnsupported();
	}

	public void ExecuteGoToDefinition()
	{
		if (_documentController.CurrentEditor is LuaEditor luaEditor)
		{
			_ = luaEditor.NavigateToDefinitionAtCaretAsync();
			return;
		}

		if (_workspaceProfile.Kind == ScriptingWorkspaceKind.ClassicScript)
			_paneCoordinator.ShowPane(UICommand.ReferenceBrowser);

		if (_documentController.CurrentEditor is ClassicScriptEditor classicEditor)
		{
			var source = new TextDocumentSnapshot(classicEditor.Document);
			string? word = _languageServices.LineService.GetWordAtOffset(source, classicEditor.CaretOffset);

			if (word is not null)
				classicEditor.GoToObject(word);
		}
		else if (_documentController.CurrentEditor is TextEditorBase textEditor and INameBasedObjectNavigator navigator)
		{
			string? word = textEditor.GetWordFromOffset(textEditor.CaretOffset);

			if (word is not null)
				navigator.GoToObject(word);
		}
	}

	public void ExecuteFindReferences()
	{
		if (_documentController.CurrentEditor is LuaEditor luaEditor)
		{
			_ = FindLuaReferencesAsync(luaEditor);
			return;
		}

		if (_workspaceProfile.Kind == ScriptingWorkspaceKind.ClassicScript)
			_paneCoordinator.ShowPane(UICommand.ReferenceBrowser);
	}

	public void ExecuteRenameSymbol()
	{
		if (_documentController.CurrentEditor is LuaEditor luaEditor)
			_ = RenameLuaSymbolAsync(luaEditor);
	}

	public Task FindLuaReferencesAsync(LuaEditor editor)
	{
		ArgumentNullException.ThrowIfNull(editor);
		return FindLuaReferencesCoreAsync(editor);
	}

	private async Task FindLuaReferencesCoreAsync(LuaEditor editor)
	{
		CancellationToken cancellationToken = default;
		long requestGeneration = 0;
		bool hasRequestGeneration = false;
		int requestDocumentVersion = 0;

		try
		{
			LuaReferenceSearchService? referenceSearchService = _luaHostServices?.ReferenceSearchService;
			if (referenceSearchService is null || !referenceSearchService.SupportsReferences)
			{
				_paneCoordinator.ShowReferenceSearchUnsupported();
				return;
			}

			CancelReferenceSearch();
			_referenceSearchCancellation = new CancellationTokenSource();
			cancellationToken = _referenceSearchCancellation.Token;
			requestDocumentVersion = editor.DocumentVersion;
			requestGeneration = _documentController.CurrentDocumentContext.Generation;
			hasRequestGeneration = true;
			_paneCoordinator.ShowReferenceSearchLoading();

			IReadOnlyList<TextReferenceGroup> groups =
				await referenceSearchService.FindReferencesAsync(editor, cancellationToken);
			if (!cancellationToken.IsCancellationRequested
				&& IsCurrentReferenceRequest(editor, requestGeneration, requestDocumentVersion))
			{
				_paneCoordinator.ShowReferenceSearchResults(groups);
			}
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{ }
		catch (Exception ex)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			if (hasRequestGeneration
				&& !IsCurrentReferenceRequest(editor, requestGeneration, requestDocumentVersion))
				return;

			_paneCoordinator.ShowReferenceSearchResults([]);
			_messageService.ShowError(ex.Message, "Lua References");
		}
	}

	private bool IsCurrentReferenceRequest(
		LuaEditor editor,
		long requestGeneration,
		int requestDocumentVersion)
		=> _documentController.CurrentDocumentContext.Generation == requestGeneration
			&& ReferenceEquals(_documentController.CurrentEditor, editor)
			&& editor.DocumentVersion == requestDocumentVersion;

	private async Task RenameLuaSymbolAsync(LuaEditor editor)
	{
		try
		{
			TextWorkspaceCommandService? workspaceCommandService = _luaHostServices?.WorkspaceCommandService;
			if (workspaceCommandService is null || !workspaceCommandService.SupportsRename)
				return;

			string currentName = editor.GetWordFromOffset(editor.CaretOffset) ?? string.Empty;
			var inputBox = new InputBoxWindowViewModel(
				title: "Rename Symbol",
				label: "New name:",
				placeholder: currentName);

			bool? dialogResult = _dialogCoordinator.ShowInputBox(inputBox);
			if (dialogResult is not true || string.IsNullOrWhiteSpace(inputBox.Value))
				return;

			int line = Math.Max(0, editor.CurrentRow - 1);
			int column = Math.Max(0, editor.CurrentColumn - 1);

			await workspaceCommandService.RenameSymbolAsync(editor, line, column, inputBox.Value.Trim());
		}
		catch (OperationCanceledException)
		{ }
		catch (Exception ex)
		{
			_messageService.ShowError(ex.Message, "Rename Symbol");
		}
	}

	private void DocumentController_CurrentEditorChanged(object? sender, ScriptingDocumentContextChangedEventArgs e)
		=> UpdateActiveDocument(e.Context);

	private void CancelReferenceSearch()
	{
		_referenceSearchCancellation?.Cancel();
		_referenceSearchCancellation?.Dispose();
		_referenceSearchCancellation = null;
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		CancelReferenceSearch();
		_documentController.CurrentEditorChanged -= DocumentController_CurrentEditorChanged;
	}
}
