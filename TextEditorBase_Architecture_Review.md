# TextEditorBase Architecture Review

**Rating: 6/10**

`TextEditorBase` is a solid transitional architecture. The partial-class split improves navigability, language-specific behavior is pushed into subclasses, and most reusable behavior now lives in focused coordinators. However, the class is still a large UI-owned facade with inconsistent lifecycle and configuration contracts. The extraction reduced file size more than it reduced coupling.

## Findings

### High: Silent loads are not silent from the editor event surface

`TextEditorBase.Persistence.cs` calls `base.Load(filePath)` before setting `IsSilentSession`. During the load, `TextChanged` can process persistence state and invoke language-specific change handling before the silent flag is active.

Relevant locations:

- [TextEditorBase.Persistence.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Persistence.cs#L22-L32)
- [TextEditorBase.Events.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Events.cs#L81-L86)
- [ContentPersistenceCoordinator.cs](TombLib/TombLib.Scripting.UI/Documents/ContentPersistenceCoordinator.cs#L185-L194)
- [LuaDocumentLifecycleCoordinator.cs](TombIDE/TombIDE.ScriptingStudio/Lua/LuaDocumentLifecycleCoordinator.cs#L96-L100)

The diagnostics worker eventually notices the silent state, but delayed persistence events can still be raised, and Lua consumes those events to update the language server. Therefore, `Load(..., silentSession: true)` can still cause delayed document updates and language-service traffic.

Silent mode should be established before replacing the document, with the coordinator exposing an explicit load/reset operation that suppresses pending notifications.

### High: Disposal semantics are inconsistent and permit post-disposal work

The class documents disposal as terminal, but `EnsureNotDisposed()` is used only by a subset of operations, mainly persistence and settings.

Several public operations remain usable after disposal:

- `SetDiagnostics` still mutates state and invalidates the view at [TextEditorBase.Diagnostics.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Diagnostics.cs#L25-L30).
- `ShowToolTip` can reopen a disposed popup through [TextEditorBase.Editing.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Editing.cs#L264-L265).
- `ShowCompletionWindow` can operate on a disposed completion coordinator at [TextEditorBase.Completion.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Completion.cs#L21-L22).
- `ToggleBookmark` can invoke the bookmark callback and save after disposal at [TextEditorBase.Editing.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Editing.cs#L71-L72).
- `FilePath` can still change the AvalonEdit document filename at [TextEditorBase.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.cs#L50-L57).

The existing disposal tests cover persistence and diagnostics workers, but not these public entry points. The class needs one explicit policy: either all public operations throw after disposal, or all are safely no-op. The derived cleanup path means this probably requires internal `*Core` methods rather than simply adding guards everywhere.

### Medium: `IntelliSenseEnabled` is a passive flag, not an actual runtime control

`TextEditorBase` exposes `IntelliSenseEnabled` as a mutable property, described as controlling IntelliSense. It is only used while applying configuration to derive other flags.

Relevant locations:

- [TextEditorBase.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.cs#L263-L264)
- [TextEditorBase.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.cs#L435-L438)

Setting `editor.IntelliSenseEnabled = false` directly does not disable completion, hover, diagnostics, or signature help if their individual flags remain enabled.

This is a configuration model problem. Either make the property private-set and require configuration application, or make the subordinate feature properties derive from a single effective IntelliSense state.

### Medium: Zoom invariants are distributed and incomplete

`MinZoom` and `MaxZoom` validate only that their individual values are positive. They do not enforce `MinZoom <= MaxZoom`. The public `Zoom` setter accepts arbitrary values without clamping or validation and immediately converts them into `FontSize`.

Relevant locations:

- [TextEditorBase.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.cs#L107-L141)
- [TextEditorBase.Editing.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Editing.cs#L121-L128)
- [TextEditorStatusCoordinator.cs](TombLib/TombLib.Scripting.UI/Editors/TextEditorStatusCoordinator.cs#L42-L72)

A caller can configure an unusable zoom range or apply an invalid zoom value. Existing validation tests do not cover these relationships or direct `Zoom` assignment.

### Medium: Initialization APIs have unclear ownership and repeatability semantics

The protected initialization methods replace existing controllers without disposing them:

- [TextEditorBase.Navigation.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Navigation.cs#L30-L31)
- [TextEditorBase.Navigation.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Navigation.cs#L39-L47)
- [TextEditorBase.Navigation.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Navigation.cs#L70-L71)

`InitializeHover` replaces an existing hover controller without disposing it. `InitializeDiagnostics` replaces an existing diagnostics coordinator without disposing its worker. A previous diagnostics coordinator can continue running and retain the editor through its callback.

These methods are currently called once by subclasses, but their protected visibility makes repeated calls possible. They should either be explicitly one-shot and reject repeated initialization, or dispose the previous controller before replacement.

## Architectural Assessment

### Strengths

- The partial split improves local discoverability by responsibility.
- Language-specific behavior is expressed through protected hooks and subclass-owned services rather than hard-coded language checks.
- Persistence, diagnostics, completion, hover, rendering, bookmarks, and view operations have recognizable ownership boundaries.
- Async UI events have one controlled `async void` boundary in [TextEditorBase.Events.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Events.cs#L105-L116).
- Disposal is idempotent and existing tests cover worker shutdown, collectibility, and several post-disposal persistence operations.
- `TextEditorServiceComposition` is intentionally narrow rather than becoming a general-purpose service locator.

### Weaknesses

The main weakness is that `TextEditorServiceComposition.Create(this)` still constructs all concrete services directly inside the control, and those services capture the concrete editor, document, dispatcher, and popup host.

Relevant locations:

- [TextEditorBase.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.cs#L342-L365)
- [TextEditorServiceComposition.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorServiceComposition.cs#L73-L116)

That approach is workable for a WPF control, but it makes lifecycle behavior difficult to reason about and makes isolated testing dependent on STA/WPF infrastructure. The base remains a broad façade over AvalonEdit and owns policy for persistence, editing, configuration, language hooks, rendering, and popup lifecycle.

The partial-class split improves organization, but it does not by itself reduce the number of responsibilities or the coupling between the editor and its services.

## Recommended Direction

1. Introduce explicit editor lifecycle operations for load, silent mode, and disposal.
2. Normalize all post-disposal behavior across the public API.
3. Replace the collection of independent mutable feature flags with an effective settings object or controlled settings application.
4. Make language-service initialization one-shot or safely replaceable.
5. Keep the partial-class split, but gradually move public editor-control contracts into smaller interfaces or facade services.

## Test Gaps

Add focused tests for:

- Silent loads not raising delayed persistence or language-service updates.
- Every public operation's behavior after disposal.
- Repeated hover and diagnostics initialization, including disposal of replaced workers.
- `MinZoom` and `MaxZoom` relationship validation.
- Direct `Zoom` assignment outside the configured range.
- Runtime behavior when `IntelliSenseEnabled` is changed independently of the subordinate feature flags.

## Suggested Priority

Address silent-load ordering and disposal semantics first. They affect lifecycle correctness and can create stale background work or retained editor instances. The configuration and initialization issues are important next because they make the public API easier to misuse and complicate future extraction of editor services.
