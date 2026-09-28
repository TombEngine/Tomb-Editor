#nullable enable

using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Messaging.Scripting;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.Controls;

/// <summary>
/// Creates <see cref="IEditorDocumentController"/> instances with editor registrations
/// already applied from the workspace profile.
/// </summary>
public interface IEditorDocumentControllerFactory
{
	/// <summary>
	/// Creates a new document controller, registers editors from the profile,
	/// and returns the initialized controller.
	/// </summary>
	IEditorDocumentController Create(
		ScriptingWorkspaceProfile profile,
		IScriptingProjectContext projectContext,
		IMessageService messageService);
}
