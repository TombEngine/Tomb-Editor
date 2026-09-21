#nullable enable

using CommunityToolkit.Mvvm.Messaging;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TombIDE.ScriptingStudio.ClassicScript;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.Editors.ClassicScript.StringEditor;
using TombIDE.ScriptingStudio.FindAndReplace;
using TombIDE.ScriptingStudio.Lua;
using TombIDE.ScriptingStudio.Messaging;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared;
using TombIDE.Shared.Messaging.Scripting;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Workbench;

internal sealed class WorkbenchService : IWorkbenchService
{
	private readonly WorkbenchComposition _composition;
	private readonly WorkbenchComponents _components;
	private bool _disposed;

	public WorkbenchService(WorkbenchComposition composition)
	{
		ArgumentNullException.ThrowIfNull(composition);

		_composition = composition;
		WorkbenchComponents components = new(composition, UpdateUi);
		_components = components;

		try
		{
			composition.Messenger.Register<FindAllPerformedMessage>(this, (_, m) => HandleFindAllPerformed(m.Value));

			_components.LayoutCoordinator.RestoreInitialLayout(UpdateUi);

			composition.Messenger.Register<ShellUiRefreshMessage>(this, (_, _) => UpdateUi());
			composition.Messenger.Register<CommandStateRefreshMessage>(this, (_, _) => UpdateCommandStates());

			composition.DocumentController.CurrentEditorChanged += DocumentController_CurrentEditorChanged;
			composition.DocumentController.EditorClosed += DocumentController_EditorClosed;
			composition.DocumentController.EditorTitleChanged += DocumentController_EditorTitleChanged;
			if (composition.WorkspaceProfile.SupportsLua)
			{
				composition.LuaHostServices?.IntellisenseBridge.Attach();
				composition.LuaHostServices?.EditorLifecycleService.Attach();
			}
			_components.EditorLifecycleService.Attach();

			composition.DocumentController.CheckPreviousSession();

			if (!composition.DocumentController.GetOpenEditors().Any() && File.Exists(composition.WorkspaceProfile.InitialFilePath))
				composition.DocumentController.OpenFile(composition.WorkspaceProfile.InitialFilePath);

			ApplyEditorSettings();

			UpdateUi();
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	public FrameworkElement WorkbenchView => _composition.DockHost.View;

	public string CaptureLayout() => _components.LayoutCoordinator.CaptureLayout();

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		_composition.Messenger.UnregisterAll(this);
		_composition.DocumentController.CurrentEditorChanged -= DocumentController_CurrentEditorChanged;
		_composition.DocumentController.EditorClosed -= DocumentController_EditorClosed;
		_composition.DocumentController.EditorTitleChanged -= DocumentController_EditorTitleChanged;
		_components.Dispose();
	}

	public void EnsureTabFileSynchronization()
		=> _components.LayoutCoordinator.EnsureTabFileSynchronization();

	public void ApplyEditorSettings()
	{
		_components.ScriptingMessageService.ApplyEditorSettings();

		if (_composition.LuaHostServices is not null)
		{
			foreach (LuaEditor editor in _composition.DocumentController.GetOpenEditors().OfType<LuaEditor>())
				_composition.LuaHostServices.TrackedDocumentStateService.ApplyTrackedState(editor);
		}

		UpdateUi();
	}

	public void NotifyMainWindowFocusChanged(bool isFocused)
		=> _components.LayoutCoordinator.NotifyMainWindowFocusChanged(isFocused);

	public void RestoreDefaultLayout()
		=> _components.LayoutCoordinator.RestoreDefaultLayout(UpdateUi);

	public bool TryExecuteCommand(UICommand command)
		=> _components.CommandRouter.TryExecuteCommand(command);

	private void DocumentController_CurrentEditorChanged(object? sender, ScriptingDocumentContextChangedEventArgs e)
	{
		UpdateUi(e.Context);
	}

	private void DocumentController_EditorClosed(object? sender, EditorControlEventArgs e)
	{
		_components.TextEditorDiagnosticsCoordinator.EditorClosed(e.Editor);

		UpdateUi();
	}

	private void DocumentController_EditorTitleChanged(object? sender, EditorControlEventArgs e)
		=> _components.CommandRouter.UpdateUndoRedoCommandPresentation();

	private void UpdateCommandStates()
		=> _components.CommandRouter.UpdateCommandStates();

	private void UpdateUi()
		=> UpdateUi(_composition.DocumentController.CurrentDocumentContext ?? ScriptingDocumentContext.Empty);

	private void UpdateUi(ScriptingDocumentContext documentContext)
	{
		IEditorControl? currentEditor = documentContext.Editor;
		_components.CommandRouter.UpdateDocumentCommandSurface(currentEditor, documentContext);
		_components.TextEditorDiagnosticsCoordinator.Update(documentContext);
		_components.PaneCoordinator.UpdateActiveDocument(documentContext);
		_components.PaneCoordinator.UpdatePaneVisibilityChecks();
		UpdateCommandStates();
	}

	private Task FindLuaReferencesAsync(LuaEditor editor)
		=> _components.CodeNavigationCoordinator.FindLuaReferencesAsync(editor);

	private void HandleFindAllPerformed(IReadOnlyList<FindReplaceSource> sources)
		=> _components.PaneCoordinator.ShowFindAllResults(sources);

}
