# Workbench

Phase 6 establishes the first real document/workbench slice.

The active shell now reuses the existing editor layer through `DocumentWorkbenchService`, which composes:

- `EditorDocumentController` for document ownership, save flows, and file synchronization.
- `StudioAvalonDockHostView` for WPF document hosting and layout restore/save.
- `StudioEditorLifecycleCoordinator` for local shortcut handling and high-frequency editor event wiring.

This slice keeps editor events local to the workbench, drives document command surfaces and command enablement from the active editor, and avoids reviving `StudioBase` or per-language shell branching in the active path.

Phase 7 extends that workbench ownership to cross-module scripting automation. `ScriptingMessageService` now subscribes the active workbench to the shared typed scripting messages used by ProjectMaster and PluginManager, and routes them through the existing per-language automation providers instead of through `IDE.IDEEventRaised`.

Phase 4 moves pane content ownership out of the old horizontal tool-window bucket. The workbench still hosts pane registration, visibility, and workflow orchestration, but the active pane wrappers now live in `Build/`, `Diagnostics/`, `DocumentOutline/`, `FileExplorer/`, `FindAndReplace/`, and `Navigation/` instead of a generic `ToolWindows/` bucket.