#nullable enable

using System;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Messaging.Scripting;
using Nickelony.IDEKit.Workspace.Documents;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.Controls;

/// <summary>
/// Default implementation of <see cref="IEditorDocumentControllerFactory"/>.
/// Creates an <see cref="EditorDocumentController"/> and registers editors
/// from the workspace profile.
/// </summary>
internal sealed class EditorDocumentControllerFactory : IEditorDocumentControllerFactory
{
	private readonly IWorkspaceDocumentManager _documentManager;
	private readonly IWorkspaceFileSystem _workspaceFileSystem;

	public EditorDocumentControllerFactory(
		IWorkspaceDocumentManager documentManager,
		IWorkspaceFileSystem workspaceFileSystem)
	{
		_documentManager = documentManager ?? throw new ArgumentNullException(nameof(documentManager));
		_workspaceFileSystem = workspaceFileSystem ?? throw new ArgumentNullException(nameof(workspaceFileSystem));
	}

	public IEditorDocumentController Create(
		ScriptingWorkspaceProfile profile,
		IScriptingProjectContext projectContext,
		IMessageService messageService)
	{
		ArgumentNullException.ThrowIfNull(profile);
		ArgumentNullException.ThrowIfNull(projectContext);
		ArgumentNullException.ThrowIfNull(messageService);

		var controller = new EditorDocumentController(
			projectContext.Project.GetCurrentEngineVersion(),
			projectContext.ScriptRootDirectoryPath,
			messageService,
			_documentManager,
			_workspaceFileSystem);

		profile.RegisterEditors(controller);

		return controller;
	}
}
