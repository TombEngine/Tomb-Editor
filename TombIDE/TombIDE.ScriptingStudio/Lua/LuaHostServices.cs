#nullable enable

using System;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.ScriptingStudio.TextEditing;

namespace TombIDE.ScriptingStudio.Lua;

internal sealed class LuaHostServices
{
	public LuaHostServices(
		ILuaEditorLifecycleService editorLifecycleService,
		ILuaIntellisenseBridge intellisenseBridge,
		LuaTrackedDocumentStateService trackedDocumentStateService,
		LuaReferenceSearchService referenceSearchService,
		TextWorkspaceCommandService workspaceCommandService)
	{
		ArgumentNullException.ThrowIfNull(editorLifecycleService);
		ArgumentNullException.ThrowIfNull(intellisenseBridge);
		ArgumentNullException.ThrowIfNull(trackedDocumentStateService);
		ArgumentNullException.ThrowIfNull(referenceSearchService);
		ArgumentNullException.ThrowIfNull(workspaceCommandService);

		EditorLifecycleService = editorLifecycleService;
		IntellisenseBridge = intellisenseBridge;
		TrackedDocumentStateService = trackedDocumentStateService;
		ReferenceSearchService = referenceSearchService;
		WorkspaceCommandService = workspaceCommandService;
	}

	public ILuaEditorLifecycleService EditorLifecycleService { get; }
	public ILuaIntellisenseBridge IntellisenseBridge { get; }
	public LuaTrackedDocumentStateService TrackedDocumentStateService { get; }
	public LuaReferenceSearchService ReferenceSearchService { get; }
	public TextWorkspaceCommandService WorkspaceCommandService { get; }
}
