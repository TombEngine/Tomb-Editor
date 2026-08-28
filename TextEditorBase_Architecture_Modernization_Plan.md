# TextEditorBase Architecture Modernization Plan

This document tracks the phased modernization of `TextEditorBase`. It is
intended to be updated as work is completed so the architectural decisions and
remaining work survive context compaction.

Revision: 2026-08-25 (handover contract revision 9; this revision supersedes
the earlier draft contracts with event-free store mutation results,
single-flight path identity, captured-snapshot persistence, bridge-ordered
projection dispatch, complete workspace-file inventory, async-scope teardown,
smaller implementation slices, stricter abstraction entry gates, an exact
phase-ID directory with reference rules, and unambiguous `1A-Bridge` naming).

Revision 9 (final handover repair): store operations return the accepted or
current immutable snapshot atomically instead of raising a store change event;
the bridge serializes projection acknowledgement and fan-out through the
existing TombIDE UI dispatcher; concurrent opens and identity changes reserve
normalized paths; commit persists one captured snapshot while later edits
remain dirty; disk-operation admission is fail-fast; destructive delete and
rename/reload recheck source stamps and block concurrent edits; external
conflicts have an explicit `UseDisk`/`UseLogical` resolution operation; file
create/delete/rename and backup-restore paths join the authority inventory;
Gate A checks document authority only; contract-only optional phases add no
production types before their first consumer.

Related review: [TextEditorBase Architecture Review](TextEditorBase_Architecture_Review.md)

## Handover Verdict

Revision 9 is ready for slice-by-slice handover. It defines one WPF-free
logical document authority, a TombIDE bridge for unloaded projections, a pure
edit kernel, a ClassicScript-specific codec, and explicit controller ownership
for prompts and views. It does not assume that AvalonEdit and string-table
views share one document implementation.

The handover order is binding: characterize current behavior, repair the
verified load-order defect, record the contract matrix, implement and validate
the in-memory store, add filesystem persistence, compose the fake-projection
bridge, then cut over text and domain views in separate slices. Headless
callers do not use the store until both projection kinds are safe. Gate A runs
only after Phases 1C-1H2 remove the ad-hoc headless document, timestamp content
authority, direct controller/view persistence, and duplicate dirty baseline.

Each alphanumeric phase is one prompt and one reviewable change. No agent may
execute the entire map as one unattended change. The recommended outcome after
Gate A is to keep the smallest useful document-first architecture and defer
later settings, ports, capabilities, and sessions unless each next slice names
a concrete consumer and a dependency or defect it will remove.

## Progress Log

Update one row after every completed slice or gate record. Status values:
`Not started`, `In progress`, `Complete`, or `Deferred`.

| Slice | Status | Date | Handover record / notes |
| --- | --- | --- | --- |
| 0A inventory and seam map | Not started | | |
| 0C lifecycle and baseline test contract | Not started | | |
| 2A load and reset lifecycle repair | Not started | | |
| 0B authority, projection, and persistence contract | Not started | | |
| 1A-Store WPF-free document state machine | Not started | | |
| 1A-Filesystem document persistence | Not started | | |
| 1A-Bridge TombIDE bridge and workspace composition | Not started | | |
| Architecture Checkpoint A0 foundation review | Not started | | human sign-off required |
| 1B WPF-free edit kernel | Not started | | |
| 1B2a text projection and live edit target | Not started | | |
| 1B2b multi-file edit coordination | Not started | | requires 1B2a |
| 1C alternate-view projections and codec | Not started | | |
| 1D workspace-edit caller migration | Not started | | requires 1B2b and 1C |
| 1E transient view leases | Not started | | |
| 1F ClassicScript and GameFlowScript lease callers | Not started | | |
| 1G Lua and TRX lease callers | Not started | | |
| 1H1 persistence, close, and reload authority | Not started | | |
| 1H2 path identity and destructive lifecycle | Not started | | requires 1H1 |
| Architecture Gate A record | Not started | | human sign-off required |
| 2B processing-state rename | Not started | | blocked without Gate A `Continue` |
| 2C generation and cancellation boundaries | Not started | | blocked without Gate A `Continue` |
| 2D deterministic disposal and initialization | Not started | | blocked without Gate A `Continue` |
| 3A settings model and first consumer | Not started | | optional after Gate A |
| 3B settings runtime path | Not started | | optional after Gate A |
| 3C caller migration and compatibility removal | Not started | | optional after Gate A |
| 4A text and line transformations | Not started | | optional after Gate A |
| 4B language-independent editing rules | Not started | | optional after Gate A |
| 4C view/edit boundary migration | Not started | | optional after Gate A |
| 5A ports from concrete consumers | Not started | | optional; Gate B applies |
| 5B persistence and diagnostics ports | Not started | | optional; Gate B applies |
| 5C completion and presentation ports | Not started | | optional; Gate B applies |
| 5D composition and lifetime | Not started | | optional; Gate B applies |
| Architecture Gate B record | Not started | | human sign-off required |
| 6A capability contracts through ClassicScript | Not started | | optional; Gate B pass required |
| 6C GameFlowScript capabilities | Not started | | optional; Gate B pass required |
| 6D TRX capabilities | Not started | | optional; Gate B pass required |
| 6E Lua capabilities | Not started | | optional; Gate B pass required |
| 6F protected-initialization removal | Not started | | optional; Gate B pass required |
| 7A session core | Not started | | optional; Gate B pass required |
| 7B persistence and diagnostics async boundaries | Not started | | optional; Gate B pass required |
| 7C language-service async boundaries | Not started | | optional; Gate B pass required |
| 8A slim control adapter | Not started | | only after approved slices |
| 8B documentation, audit, full verification | Not started | | only after approved slices |

An executing agent updates exactly its own row and no other slice's
checkboxes, exit criteria, or handover records.

## Phase ID Directory and Reference Rules

The IDs below are the only valid references to this plan's slices. Reference
slices by their exact ID and section heading; never invent, renumber, merge, or
split an ID. Layer prefixes carry meaning: 0 foundation and characterization,
1 document authority, 2 lifecycle, 3 settings, 4 view/edit boundary, 5 ports,
6 capabilities, 7 sessions, and 8 cleanup and verification. Numbering within a
layer is a grouping hint, not a binding order; the Recommended Execution Order
section is the only binding execution order.

| ID | Scope |
| --- | --- |
| 0A | inventory and seam map |
| 0C | lifecycle and baseline test contract |
| 2A | load and reset lifecycle repair |
| 0B | authority, projection, and persistence contract |
| 1A-Store | WPF-free document state machine |
| 1A-Filesystem | document persistence |
| 1A-Bridge | TombIDE bridge and workspace composition |
| 1B | WPF-free edit kernel |
| 1B2a | text projection and live edit target |
| 1B2b | multi-file edit coordination |
| 1C | alternate-view projections and codec |
| 1D | workspace-edit caller migration |
| 1E | transient view leases |
| 1F | ClassicScript and GameFlowScript lease callers |
| 1G | Lua and TRX lease callers |
| 1H1 | persistence, close, and reload authority |
| 1H2 | path identity and destructive lifecycle |
| 2B | processing-state rename |
| 2C | generation and cancellation boundaries |
| 2D | deterministic disposal and initialization |
| 3A | settings model through the first runtime consumer |
| 3B | settings through one runtime path |
| 3C | caller migration and compatibility removal |
| 4A | text and line transformations |
| 4B | language-independent editing rules |
| 4C | view/edit boundary migration |
| 5A | ports from concrete consumers |
| 5B | persistence and diagnostics ports |
| 5C | completion and presentation ports |
| 5D | composition and lifetime |
| 6A | capability contracts through ClassicScript |
| 6C | GameFlowScript capabilities |
| 6D | TRX capabilities |
| 6E | Lua capabilities |
| 6F | protected-initialization removal |
| 7A | session core |
| 7B | persistence and diagnostics async boundaries |
| 7C | language-service async boundaries |
| 8A | slim control adapter and API removal |
| 8B | documentation, audit, and full verification |

Naming conventions are part of the ID: `-Store`/`-Filesystem`/`-Bridge`
suffixes, lowercase split halves (`1B2a`, `1B2b`), and numeric split halves
(`1H1`, `1H2`) are complete IDs, not sub-headings. Retired IDs are never
reused and must not be recreated: `6B` is intentionally retired, so the
capability sequence is `6A` then `6C`. If a future revision renumbers slices,
it must update every cross-reference, the Progress Log, the focused-validation
tables, the Recommended Execution Order, and this directory in the same
change.

## Review Findings Applied

- **Canonical document ambiguity:** one file can have a `TextEditorBase` view
  and the active WPF `StringEditorView` projection. The canonical object must
  be a logical serialized workspace document; AvalonEdit and the WPF grid are
  projections, not interchangeable document implementations. The old
  WinForms `StringEditor` remains in the project as unregistered legacy source
  and is not an active alternate view. See
  [IEditorDocumentController.cs](TombIDE/TombIDE.ScriptingStudio/Controls/IEditorDocumentController.cs),
  [StringEditorView.xaml.cs](TombIDE/TombIDE.ScriptingStudio/Editors/ClassicScript/StringEditor/StringEditorView.xaml.cs),
  and [ScriptingWorkspaceProfileSelector.cs](TombIDE/TombIDE.ScriptingStudio/WorkspaceProfile/ScriptingWorkspaceProfileSelector.cs).
- **Document lifetime was unspecified:** a transient view must not dispose or
  discard a dirty logical document when it closes. The initial store retains
  all opened documents for the workspace lifetime; pins and eviction are
  deferred until measured memory pressure justifies them.
- **Transaction terminology was too strong:**
  `TextWorkspaceEditTransaction` currently stores before/after snapshots; it
  is not an atomic multi-file database transaction. Validation failures,
  completed application, and partial runtime application are separate
  outcomes. Rollback is added only with a production target that can prove
  complete restoration and a caller that needs the distinction.
- **Authority migration is specific:** `LastModified` currently selects the
  most recently modified projection for multi-view synchronization and related
  save ordering in
  [EditorDocumentControllerCore.cs](TombIDE/TombIDE.ScriptingStudio/Controls/EditorDocumentControllerCore.cs)
  and [EditorDocumentController.cs](TombIDE/TombIDE.ScriptingStudio/Controls/EditorDocumentController.cs).
  Close decisions already use dirty state and reload compares content with
  disk. Replacing the timestamp authority is still a behavior migration, but
  it must not be described as a blanket rewrite of close or reload behavior.
- **The processing rename is cross-project:** `IsSilentSession` and
  `silentSession` also exist on `IEditorControl`, the active WPF string-table
  editor, `IEditorDocumentController`, and the controller implementation. The
  rename cannot be scoped to `TextEditorBase`; the unregistered WinForms
  implementation should be removed rather than migrated again.
- **The existing headless path must be absorbed:**
  `DocumentControllerTextEditorHost.TryGetTextDocument` creates a new
  `TextDocument` for every unopened lookup. Leaving it beside a new store
  would create two authorities. Phase 1A-Bridge leaves this legacy path unchanged
  while it tests composition only; Phase 1D migrates the concrete read-only
  caller to snapshots and deletes the mutable fallback and its test doubles.
- **The current edit test exposes a policy gap:** the invalid second file in
  `TextWorkspaceEditApplierTests` is rejected after the first file has already
  changed. The test name suggests atomic behavior, but the assertion documents
  partial application. The modernization must choose and test one policy.
- **Async modernization is misnamed:** the current content and diagnostics
  workers use tasks, dispatcher timers, and dispatcher-marshaled completion;
  there is no blanket `BackgroundWorker` replacement to perform. Modernize
  cancellation and ownership deliberately rather than rewriting working code
  by name.
- **File persistence details are missing:** normalized path identity, encoding,
  BOM, newline style, missing-file behavior, rename, and external reload must
  be part of the document contract before headless writes are introduced.
- **String-table load lifecycle is also affected:** the active WPF
  [StringEditorView.xaml.cs](TombIDE/TombIDE.ScriptingStudio/Editors/ClassicScript/StringEditor/StringEditorView.xaml.cs)
  replaces grid content before applying `IsSilentSession`. Phase 2A covers
  this control and the shared `IEditorControl.Load` contract. The unregistered
  WinForms `StringEditor` is a cleanup/removal concern, not an active lifecycle
  implementation to preserve.
- **The store dependency boundary is explicit:** the WPF-free logical store
  owns logical documents and disk metadata; a TombIDE adapter attaches
  unloaded controls to canonical snapshots. The core store must not reference
  `EditorDocumentController` or AvalonEdit to find a projection.
- **Headless snapshots stay WPF-free:** the store uses
  `TombLib.Scripting.Text.ITextSnapshot` and `StringTextSnapshot`.
  `TextDocumentSnapshot` is permitted only in the AvalonEdit projection
  adapter.
- **The string-table representation is intentionally lossy:** source text is
  the logical serialized content until an accepted grid edit publishes a
  serialized replacement. Parsing a grid is read-only; publishing a grid edit
  is an explicit normalization that may remove comments and blank lines.
- **The edit policy is now explicit:** the shared kernel rejects overlapping
  edits after a provider audit, validates every file before mutation, and
  reports validation, complete, and partial runtime outcomes separately. The
  previous descending-offset overlap test is not the new general contract.
- **All headless callers are in scope:** the migration inventory includes
  `ScriptingMessageService`, `TombEngineLevelScriptService`,
  `LuaTrackedDocumentStateService`, `LuaReferenceSearchService`, host
  registrations, and their test doubles so `TryGetTextDocument` cannot remain
  an independent headless authority.
- **Abstraction growth has a gate:** the document, store, session, projection,
  view-lease, controller, and provider owners are defined below. A later port
  or session extraction may proceed only when it removes a demonstrated
  duplicate dependency, has a focused consumer and test seam, and passes the
  continuation decision at Architecture Gate A or B as applicable.
- **Repository test topology is explicit:** the WPF-free core tests belong in
  the new `Tests/TombLib.Scripting.Tests` project. Existing
  `Tests/TombLib.Tests` and `Tests/TombEditor.Tests` remain mixed Windows test
  projects; the latter hosts the TombIDE/ScriptingStudio tests. No phase may
  treat `TombLib.Test` as a test project: the root-level folder of that name
  contains only stale `obj/` build output, and no standalone TombIDE test
  project exists.
- **Versioned snapshots are explicit:** every snapshot carries the logical
  document identity, document version, content, and file-format metadata from
  one capture; a separately read content/version pair is not a valid
  stale-write check.
- **The first migration order is explicit:** the string-table projection
  registration and source/grid conflict contract are established before any
  store-based workspace-edit caller migration; an unregistered dirty domain
  projection must never be silently replaced with disk content.
- **Edit extraction is split:** the WPF-free edit kernel, target adapters, and
  behavior-changing multi-file policy are separate slices; target application
  and multi-file policy must not be described as a pure helper extraction.
- **`LastModified` is fully audited:** close-time propagation, save-all,
  synchronization, and rename/reload behavior are covered together. It is not
  removed from one call site while another call site still selects content
  authority.
- **The handover contracts are executable:** store operations return their
  accepted/current snapshot atomically; the bridge returns explicit open and
  projection-conflict outcomes; commits use an expected on-disk stamp; edit
  results distinguish validation, complete, and partial outcomes; and each
  implementation phase has a focused validation command.
- **The projection protocol is complete:** the TombIDE bridge owns projection
  unloaded creation, attach/detach, canonical refresh, pending-edit publish,
  version checks, ordered acknowledgement/fan-out, and conflict resolution.
  The WPF-free store never raises UI-facing change events, calls a projection,
  or owns a projection callback.
- **Loaded-view import was removed:** each production view joins the canonical
  path only when its controller branch can create it unloaded and attach it
  through the bridge. Phase 1B2a migrates text-only profiles; Phase 1C migrates
  domain-capable profiles. A legacy-loaded and bridge-managed projection never
  coexist for one identity.
- **Ordinary text opening and mutation have one cutover:** the bridge exposes
  an `OpenOrAttachAsync` path for a user-facing projection. Phase 1B2a routes
  `EditorDocumentControllerCore.OpenFile` through that path only after the
  text projection's pending-edit event is wired to version-checked publish.
  AvalonEdit typing, undo/redo, completion insertion, and programmatic text
  mutations all enter that same event path; load, refresh, and acknowledgement
  use a suppression scope and never publish feedback changes.
- **Commit and logical-document lifetime have one owner:** the store exposes
  `CommitAsync` and owns the operation gate, persistence baseline, and disposal
  of store-owned document state. Callers receive immutable snapshots and send
  version-checked requests; projections and leases release registrations
  instead of disposing a logical document directly.
- **Document instances are unambiguous:** a stable `WorkspaceDocumentKey`
  survives rename. Snapshots, commits, projections, edits, and asynchronous
  results carry that key in addition to normalized identity and document
  version. A later eviction feature must issue a new key on reopen.
- **Dirty state is baseline-derived:** `Version` is monotonic and
  `PersistedVersion` records baseline provenance, while `IsDirty` compares
  current content/format with persisted content/format. Undo-to-baseline is
  clean without decrementing the version.
- **Encoding is profile-aware:** BOMs are authoritative; BOM-less
  ClassicScript uses Windows-1252 and BOM-less Lua/UTF-8 profiles use UTF-8.
  Ambiguous ASCII is never resolved by a store-global guessing heuristic.
- **Mutation and synchronization are single-owner:** snapshots are read-only,
  only the store replaces logical content, a synchronous state lock protects
  in-memory state, and an asynchronous per-document operation gate serializes disk
  workflows. No lock is held across an await or UI callback.
- **The result surface is demand-driven:** a result type or status is added
  only with a production branch and focused test. Calls after disposal throw
  `ObjectDisposedException` instead of adding `Disposed` to every enum.
- **The persisted baseline is singular:** after cutover,
  `ContentPersistenceCoordinator` schedules backups/events from immutable
  store snapshots and no projection or worker independently stores or compares
  persisted content.
- **File concurrency is explicit:** disk operations use asynchronous I/O and
  an injectable filesystem boundary. The default is optimistic stamp checking
  followed by an atomic same-directory replacement; the plan makes clear that
  this is not an inter-process compare-and-swap and tests the observed-conflict
  path without claiming a guarantee the OS cannot provide.
- **Store lifetime is explicit:** one store is registered per ScriptingStudio
  workspace composition scope, only the bridge receives that raw instance,
  controllers use the bridge boundary, and the async scope owns disposal after
  the bridge stop barrier has detached projections.
- **Serializer ownership is explicit:** `TombLib.Scripting` is already the
  WPF-free home for the reusable text and snapshot contracts. The pure
  ClassicScript string-table model, reader, writer, diagnostics, and golden
  corpus belong there; the WPF string editor remains an adapter. No additional
  `ClassicScript.Core` assembly is required.
- **Core type namespace is fixed:** the store, snapshot, format, projection,
  result, and filesystem contract types added by this plan live in a
  dedicated `TombLib.Scripting.Workspace` namespace. They do not join the
  existing `TombLib.Scripting.Text` snapshot namespace or the project root,
  so the established snapshot vocabulary stays stable and the new boundary
  stays easy to locate and audit.
- **Future format reuse stays deliberately small:** this slice keeps the
  ClassicScript reader/writer format-specific and does not invent a Lua model,
  format registry, or generic parser hierarchy. A later language can add a
  pure codec behind the same projection boundary when it has a concrete
  consumer.
- **Mutation ownership is audited:** visible editor mutations, direct editor
  file I/O, generated-file I/O, and language-server edit producers are
  classified separately, with an owner and focused test for each remaining
  path.
- **Test topology is executable:** core lifecycle and document tests run in
  `TombLib.Scripting.Tests`, while WPF lifecycle and active string-table tests
  run in `TombEditor.Tests`; no WPF test is assigned to the WPF-free project.
- **The test topology is bootstrapped before it is consumed:** Phase 0C creates
  and registers `Tests/TombLib.Scripting.Tests` and characterizes only current
  production subjects. Phase 0B records the test matrix without a fake store;
  production store and bridge tests arrive with Phases 1A and 1A-Bridge.

## Execution Rules

- Do not combine distinct ownership changes merely because they appear
  related.
- Before editing, read this plan, the named primary references, current tests,
  and the repository instructions that apply to the touched files.
- Keep the first edit inside the phase scope. After it, run the phase's
  narrowest executable validation before reading broadly or opening a second
  edit slice.
- A phase is complete only when its exit criteria and focused tests pass. A
  solution build alone is not an exit criterion for a lifecycle or ownership
  change.
- Do not use a generic `IEditorService`, service locator, or dependency
  container as a substitute for a missing ownership decision.
- Do not remove user changes in unrelated files. Keep unrelated worktree
  changes out of the phase diff.
- When an API is removed, update all callers and its focused tests in the same
  phase, or leave the API in place and document why removal is deferred.
- Respect the existing build contract of `TombLib.Scripting`: it generates
  XML documentation and treats `CS1591` as an error. Every new public type and
  member in that project must therefore have XML documentation, or remain
  internal when it is not part of a public boundary. Run the focused project
  build early after adding a public contract.
- Phase the visibility of the new contract types to control the public-API
  tax. Keep the store, snapshot, format, projection, and result types
  `internal` to `TombLib.Scripting` until the first cross-project consumer
  requires them public, normally Phase 1A-Bridge or Phase 1D for the
  caller migration. Give test projects access through `InternalsVisibleTo`
  rather than promoting a type to public for test access alone. A type
  promoted to public must receive its XML documentation in the same change.
- Prefer a phase with one architectural decision, one production ownership
  boundary, and one test cluster. A phase that touches more than one of those
  dimensions should be split before execution.

### Handover Readiness Checklist

Each agent receives exactly one phase or gate prompt. Before the first edit it
must:

1. Read this plan, the applicable repository instructions, the phase's named
  primary references, and its focused tests.
2. Confirm the phase entry dependencies and current worktree state. Existing
  user changes are preserved and are not folded into the phase.
3. State the local ownership hypothesis, the cheapest falsifying check, and the
  allowed production/test files before editing.
4. Make the smallest phase-scoped edit, then run the phase's focused
  executable validation before broadening the investigation or starting a
  second edit slice.
5. Update only the phase checklist, its handover record, and its Progress Log
  row after validation. The record lists changed files, behavior changes,
  tests/commands and results, pre-existing failures, deferred work, and any
  measured dependency reduction.
6. For a gate phase, prepare the measurement record but do not record a
  continuation decision. `Continue`, `Repair`, `Stop and defer`, and Gate B
  pass/fail require human sign-off before the next slice may start.

An agent must stop and add a repair slice when a required contract is missing,
the named project topology cannot be satisfied, an unloaded projection cannot
attach without bypassing the bridge, a filesystem result is ambiguous, or a
new abstraction has no concrete consumer and removed dependency edge. It must
not resolve these conditions by inventing a new policy inside an implementation phase. Gate A
blocks the Phase 2B processing-state cutover and later authority follow-up
until the document boundary and net-benefit decision are proved; the
explicitly earlier Phase 2A lifecycle repair is an approved prerequisite and
is not blocked by Gate A.
Gate B blocks optional ports, capabilities, and session extraction until the
measured net-benefit record is complete.

### Standard Handover Prompt Shape

Every phase prompt should state: `Read this plan first`; the exact phase ID;
the allowed production projects and tests; the explicit non-goals; the focused
validation command or test files; and the required plan update. The agent must
report changed files, behavior changes, tests run, and any deferred decision.
Every prompt for a slice after Phase 2A must also state that `Stop and defer`
at Architecture Gate A is a supported end state and that the slice starts only
because its named concrete consumer and removed dependency edge were recorded
in the previous handover. No slice is started merely to complete the phase
map.

For the first document/edit slices, the following test files and filters are
fixed so the handoff does not depend on a future agent guessing the topology:

| Slice | Test file(s) | Focused command |
| --- | --- | --- |
| 0C current core characterization | `Tests/TombLib.Scripting.Tests/StringTextSnapshotCharacterizationTests.cs` | `dotnet test Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` |
| 0C WPF lifecycle characterization | `Tests/TombEditor.Tests/ScriptingStudio/ScriptingPhase0LifecycleTests.cs` | `dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` |
| 0B decision/test matrix | No new test file; update the Binding Decision Record matrix in this plan | Re-run the Phase 0C and 2A category-filtered characterization tests |
| 1A-Store state machine | `Tests/TombLib.Scripting.Tests/WorkspaceDocumentStoreTests.cs` | `dotnet test Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` |
| 1A-Filesystem persistence | `Tests/TombLib.Scripting.Tests/WorkspaceDocumentFileSystemTests.cs` | `dotnet test Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` |
| 1A-Bridge composition | `Tests/TombEditor.Tests/ScriptingStudio/WorkspaceDocumentBridgeTests.cs` | `dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` |
| Checkpoint A0 foundation review | No new test file; review the 1A/1A-Bridge handover records and surface measurements | Re-run the 1A and 1A-Bridge category-filtered commands; record the human decision |
| 1B kernel | `Tests/TombLib.Scripting.Tests/TextEditKernelTests.cs` | `dotnet test Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` |
| 1B2a text projection/live target | `Tests/TombEditor.Tests/ScriptingStudio/WorkspaceTextProjectionTests.cs` | `dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"`; run the recorded release benchmark |
| 1B2b multi-file coordinator | `Tests/TombEditor.Tests/ScriptingStudio/TextWorkspaceEditApplierTests.cs` | `dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` |
| 1C projections | `Tests/TombLib.Scripting.Tests/StringTableSerializerTests.cs`, `Tests/TombEditor.Tests/ScriptingStudio/StringTableProjectionTests.cs` | `dotnet test Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"`; then `dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` |
| 1D callers | `Tests/TombEditor.Tests/ScriptingStudio/LuaReferenceAndRenameTests.cs`, `Tests/TombEditor.Tests/ScriptingStudio/ScriptingMessageServiceTests.cs` | `dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` |

Every new or intentionally migrated test for this plan uses the
`TextEditorBaseModernization` test category. Focused commands use that category
filter consistently; a phase may add a second class-name filter for a local
run, but the category remains mandatory. Core tests use the
  `TombLib.Scripting.Tests` namespace and bridge, projection, and host tests stay
  in `TombEditor.Tests` because they require TombIDE/WPF composition. Existing
  non-TombIDE editor tests remain in `TombLib.Tests`; active ScriptingStudio
  lifecycle, projection, bridge, and host tests belong in `TombEditor.Tests`.
  The core test project references `TombLib.Scripting` from Phase 0C onward;
  Phase 1C adds the codec and model tests there without introducing another
  production project reference. A phase may add narrower tests, but it must
  not move WPF tests into the WPF-free project or refer to a nonexistent
  project.

## Prompt-Size Assessment

The revised map contains 40 alphanumeric slices. The slices are intentionally
smaller than the original eight phases:

- **Small:** inventory, model, focused test, and single-language slices such
  as 0A, 0B, 0C, 3A, 4A, 6C, and 6D.
- **Medium:** a single ownership migration plus its callers, such as
  1A-Store, 1A-Filesystem, 1A-Bridge,
  1B2a, 1B2b, 1C, 1D, 1E, 2A, 2B, 3B, 4B, 5B, 5C, and 7B. Phase 1A-Bridge owns the TombIDE
  bridge and workspace composition handover. Phase 1B is medium because it
  changes a behavior contract and its provider audit; Phase 1D is medium
  because the grid projection is intentionally lossy.
- **Large but bounded:** Lua-only slices 6E and 7C, final adapter cleanup 8A,
  and full verification 8B. They remain isolated from unrelated ownership
  changes; split one by an existing ownership boundary if its named references
  do not fit the available context.

The former Phase 1A split is now normative. `1A-Store` owns only the in-memory
state machine and filesystem seam; `1A-Filesystem` owns real loading, encoding,
stamps, commit, and replacement recovery. Do not recombine them and do not
implement a fake store in Phase 0B.

No slice is allowed to combine a new cross-project contract, a second
independent ownership migration, and a full solution-wide test rewrite. In
particular, the WPF-free store and the TombIDE bridge/composition must remain
separate slices even though they share a behavioral contract. If a slice
discovers that its named references exceed the available context, split the
slice by the same ownership boundary before editing and record the new ID here.

## Direction

Treat this as a full overhaul rather than a compatibility migration. Keep a
small `TextEditorBase` control because AvalonEdit/WPF still needs a control
adapter, but do not keep the current public/protected surface merely to avoid
updating callers. Move policy and feature ownership into explicit, narrow
capabilities and remove obsolete APIs as each boundary is established.

The target architecture is document-first:

```text
Logical workspace document
  |
  +-- Text projection for open AvalonEdit views
  |
  +-- Domain projection for string-table or other specialized views
  |
  +-- Headless operation target for unopened-file work
  |
  +-- Optional editor view lease for UI-specific work
```

`TextEditorBase` should own WPF/AvalonEdit adaptation and view state. It should
not be the universal document engine for both visible and headless operations.
There should be one canonical logical document per normalized file identity,
whether or not a view is currently attached. That logical document is not
required to be an AvalonEdit `TextDocument`. An AvalonEdit document is a text
projection, and a string-table control is a domain projection with explicit
parse, serialize, and conflict rules.

## Scope Decision

The processing-state rename and document-first text-edit architecture are part
of the main overhaul. They are not follow-up work. The rename establishes the
correct lifecycle vocabulary, and the document workspace establishes the
correct ownership boundary for every later extraction. Backward compatibility
is not a constraint: obsolete APIs, boolean parameters, and broad editor-host
contracts may be removed once their callers move to the new owners.

### Responsibility rules

- Use extension methods for stateless queries and transformations that do not
  touch WPF, editor state, timers, events, or disposal.
- Use a provider for language data or domain information, such as diagnostics,
  completion items, hover information, or definition locations.
- Use a coordinator or controller for scheduling, cancellation, stale-result
  handling, and feature state.
- Use a presenter or adapter for AvalonEdit and WPF operations such as
  selection, scrolling, popups, renderers, and dispatcher interaction.
- Use the document and store for canonical document identity, content, dirty
  state, loading, persistence metadata, retention, and disposal of logical
  documents.
- Use a session or lifecycle owner for operation/view lifetime, processing
  mode, generation, cancellation ownership, and provider coordination. A
  session references a logical document; it does not become a second owner of
  document content or dirty state.
- Do not create a generic `IEditorService`, `IEditorProvider`, or large utility
  class that merely moves the existing coupling under a new name.

### Ownership matrix

The following ownership matrix is a design constraint, not a requirement to
create one interface for every row. Add an interface only when a concrete
consumer needs it and a focused test can exercise the boundary.

| Owner | Owns | Must not own |
| --- | --- | --- |
| Logical workspace document | normalized identity, serialized content, document version, persisted baseline, dirty state, and file-format metadata | WPF controls, tabs, popups, dispatcher timers, or provider scheduling |
| Workspace document store | logical-document lookup/load, retention, disk commit, rename, and external-file state | projection registration, editor selection, activation, prompts, or language-provider policy |
| TombIDE workspace profile selector | file/profile classification and `WorkspaceDocumentOpenOptions` selection | decoding bytes, canonical content, projection state, or persistence |
| TombIDE document bridge | unloaded-projection attach/registration, detach, notification dispatch, publication acknowledgement, conflict coordination, and view-to-document mapping | canonical content/version, disk I/O, document retention, or UI policy outside the registered projection |
| Shared edit kernel | value-based range conversion, preflight, overlap policy, edit ordering, offset mapping, and target-independent results | AvalonEdit mutation, undo UI, persistence scheduling, or selection restoration |
| Editor session/lifecycle owner | processing mode, operation scope, cancellation, generation checks, and provider coordination | canonical content, persisted baseline, or store retention decisions |
| Text/domain projection adapter | parse, serialize, attach/detach, projection version, and stale/conflict handling | a second canonical content or dirty-state authority |
| Editor view lease | exact view attachment, transient/persistent disposition, active-view restoration, and lease disposal | logical-document disposal or unrelated tabs |
| ScriptingStudio shell composition | shutdown ordering: stop work, detach views/projections, release leases/sessions, and finally dispose scoped stores | canonical document content or DI registration policy |
| `TextEditorBase` adapter | AvalonEdit/WPF document binding, selection, caret, scrolling, renderers, popups, and view events | headless loading, cross-view authority, or language data |
| `IEditorDocumentController` | user-facing tabs, activation, save/close prompts, and workflow events | choosing a canonical document by timestamp or creating ad-hoc headless documents |
| Language/domain provider | diagnostics, completion, hover, definitions, parsing, and domain rules | WPF controls, view leases, persistence, or stale-result policy |

This matrix is the architecture gate: a later extraction is justified only if
it moves behavior to the owner shown here, removes a demonstrated duplicate
dependency, and leaves the existing behavior covered by a focused test.

### Abstraction proof obligation

The architecture is intentionally allowed to stop. Before adding a port,
capability, coordinator, or session type in Phases 5-7, or a new settings
model or transformation service in Phases 3A-4C, the phase prompt must
include all of the following:

- the concrete consumer that will depend on the new type;
- the direct dependency or member-use edge removed from that consumer;
- the production implementation and its disposal owner; and
- a focused test that exercises behavior through the new boundary.

The phase update must report the before/after dependency reduction. A fake,
interface, or adapter added without a removed dependency is rejected as
speculative abstraction and the phase is not complete. `TextEditorServiceComposition`
may remain a concrete composition object; it must not be replaced by a
general-purpose registry or container merely to satisfy this plan.

Phases 3A-4C are optional after Architecture Gate A. The settings model and
transformation extractions may proceed only when they have a concrete
consumer and a focused test that justify the new boundary; otherwise they are
marked deferred with the same `Stop and defer` procedure as Phases 6A-7C.

`SelectLine()` is a view operation, not a stateless utility. It should remain
behind a view port or view service. An extension method could make the call
shorter, but it would still mutate AvalonEdit selection state and would not
improve ownership.

## Processing and Open-State Vocabulary

The current `IsSilentSession` boolean combines concepts that must be kept
separate. Since this is a full overhaul, replace it rather than retaining it as
a compatibility property.

### What the current state actually does

`IsSilentSession` currently affects:

- content-change persistence work;
- backup-file creation;
- diagnostics scheduling;
- diagnostics result publication; and
- the behavior of an editor opened by a host operation.

The first four are editor processing policy. The last is document-controller
ownership and open/close policy. The editor should not decide whether a
document tab is kept open.

### Recommendation

Use two explicit concepts:

1. `EditorProcessingMode`, with values such as `Normal` and `Suppressed`,
  controls persistence scheduling, backup creation, diagnostics, and other
  background editor work.
2. `EditorOpenDisposition`, with values such as `Persistent` and `Transient`,
  controls whether a view opened for an operation remains a user-facing tab.

The API keeps document acquisition, processing policy, and view attachment
separate. The following names and meanings are fixed for the handover; an
implementer may choose an equivalent naming style only when the public API
would otherwise conflict with an existing type:

```csharp
public enum EditorProcessingMode
{
    Normal,
    Suppressed
}

public readonly record struct DocumentLoadOptions(
  EditorProcessingMode ProcessingMode = EditorProcessingMode.Normal);

public enum EditorOpenDisposition
{
    Persistent,
    Transient
}

public readonly record struct EditorViewLeaseOptions(
  EditorOpenDisposition Disposition = EditorOpenDisposition.Persistent);
```

`EditorProcessingMode` is owned by an operation/session scope. A control or
projection receives the scope and exposes only its current read-only mode; it
does not become the long-lived authority for processing state. The scope is
nest-safe, restores the previous mode in `Dispose`, and invalidates pending
work when it ends. `DocumentLoadOptions` carries the requested initial mode
into an editor/view load. It belongs to that UI lifecycle boundary and is not
passed to the WPF-free document store or bridge. `EditorOpenDisposition` is
owned only by the view host and lease; it is never passed to a headless
document operation.

Do not put `EditorOpenDisposition` on a headless document operation. A query or
edit that does not need a view has no tab lifetime to manage.

`Suppressed` is more accurate than `Silent` for editor processing. `Transient`
or `OperationScoped` is more accurate for a view that should close after the
operation. Neither should be represented by `IsSilentSession`.

Processing suppression must be scoped and nest-safe. A load or operation enters
the requested mode before content replacement, nested scopes restore the
previous mode in a `finally` path, and disposal cancels any active scope. Do
not implement suppression as an unbalanced mutable flag on a view control.

## Binding Decision Record

This section is normative. Phase 0B records tests for these decisions; it does
not choose different policies. A later agent may refine an implementation
detail only if the behavior remains identical and the change is recorded in
the phase update.

### Identity and snapshots

- A path is rejected when it is null, empty, or whitespace. Otherwise its
  identity is `Path.GetFullPath(path)` with the platform directory separator
  normalized. On Windows identity comparison is
  `StringComparer.OrdinalIgnoreCase`; the first user-supplied path remains the
  display path until an explicit rename.
- `DocumentId` is the normalized identity string. The store contains at most
  one logical document for a `DocumentId`.
- Concurrent first opens use one internal single-flight reservation per
  normalized `DocumentId`. The first caller performs the load. Followers wait
  for that reservation, then receive the resulting `AlreadyOpen` snapshot. A
  follower may cancel only its own wait. If the loading caller fails or
  cancels, the reservation is removed without creating a document and exactly
  one remaining caller may become the next loader. The store never performs
  two simultaneous disk loads that can install competing documents for one
  identity.
- Open and identity-change operations share the same internal normalized-path
  reservation table. An open that encounters a rename/Save As destination
  reservation waits cancellably: success returns the newly re-keyed document
  as `AlreadyOpen`, while failure releases the path and lets one waiter become
  loader. A rename/Save As that encounters an open-load or other identity
  reservation returns `DestinationBusy` before filesystem mutation. The store
  never resolves the race by creating a second logical document.
- `WorkspaceDocumentKey` is an immutable document-instance key, normally a
  generated `Guid`. It remains stable across rename and ordinary reload, but a
  document that is evicted and later reopened receives a new key. A path is
  therefore an address, not an incarnation identity.
- A document version starts at `0` after load and increments exactly once for
  each accepted logical content replacement, including a normalized
  string-table publish. A no-op replacement does not increment it. A
  successful disk commit increments no document version.
- `PersistedVersion` records the document version at which the current
  persisted baseline was installed or realigned. Loading disk content sets
  `Version` and `PersistedVersion` to `0`; a successful commit sets
  `PersistedVersion` to the committed `Version` without incrementing
  `Version`.
- The persisted baseline is the last accepted persisted content and
  `TextFileFormat`. `IsDirty` is derived by ordinal content comparison and
  value equality of the current format against that baseline; it is not
  derived from version inequality. Editing and then undoing exactly to the
  persisted content and format makes the document clean while `Version`
  remains monotonic.
- Discarding dirty changes restores both persisted content and format
  atomically, increments `Version` once when either current value changes,
  and sets `PersistedVersion` to the resulting `Version`. Discarding a clean
  document is a no-op.
- `CaptureSnapshot()` returns one immutable `WorkspaceDocumentSnapshot` that
  contains `DocumentId`, `Version`, `Content`, `FileFormat`, and the on-disk
  state captured under the state lock. Consumers must use this snapshot's
  version for stale-write checks; reading `Version` and content separately is
  not sufficient.
- A replacement is a store operation and requires `expectedDocumentKey` and
  `expectedVersion`. A version mismatch returns `StaleDocument`; a key
  mismatch returns `StaleDocumentInstance`. Both outcomes leave content,
  format, version, and dirty state unchanged. Captured snapshots are read-only;
  only the store can accept a replacement. Every mutation result contains the
  accepted snapshot or, for a stale/no-change result, the current snapshot
  captured in the same state-lock transition. Callers never perform a second
  snapshot lookup to interpret a mutation result.

### File format and persistence

- `TextFileFormat` is an immutable value containing `TextEncodingKind
  Encoding`, `bool HasBom`, and `TextNewlineStyle NewlineStyle`. Supported
  encodings are UTF-8, UTF-16 little-endian, UTF-16 big-endian, and
  Windows-1252. A BOM selects UTF-8, UTF-16 LE, or UTF-16 BE. A file without
  a BOM is decoded strictly using the `NoBomEncoding` supplied in
  `WorkspaceDocumentOpenOptions`; content sniffing must not guess between an
  ASCII-compatible UTF-8 file and an ASCII-compatible Windows-1252 file.
  Invalid sequences in the selected encoding fail the load and are never
  silently replaced.
- Recorded product decision (2026-08-24): ClassicScript only works with
  Windows-1252, so Windows-1252 is a first-class supported
  `TextEncodingKind`, not a deferred fallback. The existing TombIDE workspace
  profile selector supplies Windows-1252/no-BOM for ClassicScript and
  UTF-8/no-BOM for Lua and other UTF-8 profiles when it calls the bridge. A BOM
  still takes precedence. The five undefined Windows-1252 bytes (0x81, 0x8D, 0x8F,
  0x90, 0x9D)
  return `LoadFailed` with a typed encoding error surfaced by the controller
  to the user and are never silently replaced. New documents default to
  the `NewFileFormat` supplied in the same options; there is no store-global
  default that can silently override a language profile. This retires the
  current behavior, in which
  default-encoding reads silently replace invalid sequences with U+FFFD and
  save the corrupted text back. Phase 0B records golden-corpus tests for
  this rule, including an ASCII ClassicScript file followed by a non-ASCII
  edit; no phase may invent another code-page fallback implicitly.
- `TextNewlineStyle` is `CrLf`, `Lf`, `Cr`, `Mixed`, or `None`. The exact
  newline sequence used for a generated replacement is an explicit serializer
  option; it is never read from `Environment.NewLine`.
- `FileStamp` is an immutable value containing `Exists`, `Length`,
  `LastWriteTimeUtc`, and a SHA-256 content hash. A missing file is represented
  by the non-null `FileStamp.Missing` value with `Exists = false` and no
  length, timestamp, or hash. Snapshots and persistence requests never use a
  nullable stamp; the stamp captured in a snapshot is the only expected
  on-disk state a commit may use.
- Ordinary text edits preserve existing newline characters; the store does not
  normalize source text on load or ordinary replacement. Domain serializers
  choose an explicit output newline style and return it with the replacement.
- A missing file opens as an empty, non-dirty document with `ExistsOnDisk =
  false`, `FileStamp.Missing`, the options' `NewFileFormat`, and
  `PersistedVersion = 0`. Editing its content or format makes it dirty; an
  explicit commit may create the file.
- Commit takes the expected logical document key/version and `FileStamp` from
  one `WorkspaceDocumentSnapshot` and copies that snapshot's content and
  format when the operation is admitted. It persists exactly those captured
  bytes even if later logical replacements advance the live document during
  I/O. It rechecks the disk stamp immediately before replacement and returns
  `ExternalFileConflict` with the actual stamp when the file changed. It never
  intentionally overwrites a changed stamp observed before replacement. A
  clean document whose unchanged stamp says the file already exists returns
  `Committed` as a no-op without rewriting the file; a clean missing document
  still performs replacement so explicit commit can create an empty file. A
  clean document whose on-disk stamp changed returns `ExternalFileConflict`
  and never silently overwrites the newer disk content.
- A successful commit writes through a temporary file in the same directory,
  flushes the temporary file, and uses the filesystem replacement primitive
  only after the temporary write succeeds. The persisted baseline and on-disk
  stamp advance only after replacement succeeds. The baseline becomes the
  captured content/format and `PersistedVersion` records the captured version.
  If later edits advanced the live version or content while I/O was in
  progress, the returned current snapshot remains dirty against that newly
  installed baseline; the store never marks those later edits persisted and
  the controller does not perform a hidden retry. On success the filesystem
  boundary returns the post-replacement stamp in its replacement result, and
  the store advances its persisted baseline from that single observation
  rather than capturing the stamp a second time. The default implementation
  derives the stamp length and content hash from the bytes it wrote and
  observes the destination's last-write time after the replacement succeeds,
  avoiding a second full read of the file. A failed commit leaves the
  document dirty and returns `WriteFailed` with the original exception. If the
  operating system reports an ambiguous replacement outcome, the store
  returns `ReplacementStateUnknown`, does not advance its persisted baseline,
  and includes a newly observed stamp when it can obtain one. The caller must
  explicitly capture a new snapshot and resolve the outcome before retrying.
- A commit marks the document's disk operation as active under the state lock
  before starting I/O. Logical replacements remain allowed and become later
  dirty edits. Synchronous discard returns `OperationInProgress` while commit,
  reload/conflict resolution, rename, Save As, or delete owns the document
  operation gate; it never races a persisted-baseline transition. Close and
  destructive controller actions wait for or cancel the admitted operation at
  their async boundary.
- Disk-operation admission is fail-fast rather than an internal FIFO. Commit,
  reload/conflict resolution, rename, Save As, or delete finds the per-document
  operation gate already owned, returns `OperationInProgress` with the atomic
  current snapshot, and performs no I/O. A controller that needs a follow-up
  operation explicitly awaits or cancels the tracked admitted operation,
  captures a new request, and submits it again; the store never queues a
  request whose key/version/stamp can become stale while waiting.
- The stamp check is optimistic concurrency, not an inter-process
  compare-and-swap. An unrelated process can change a file after the final
  observed stamp and before replacement unless it participates in the same
  locking protocol. This plan does not claim to eliminate that unavoidable
  race; it requires tests and UI outcomes for observed conflicts and unknown
  replacement state. A future strict writer-lock protocol may be added only
  as a separately justified capability, not silently assumed by the store.
- The store records an on-disk stamp consisting of existence, length, last
  write time, and a content hash captured at load or commit. External change
  on a clean document may be reloaded only through an explicit reload
  operation. External change on a dirty document returns a conflict and never
  overwrites the logical document automatically.
- Rename is an explicit store operation. Before I/O, the store reserves the
  normalized destination under the same state lock used by open. It rejects a
  destination that is already a retained document or is reserved by another
  open/identity operation. It releases the state lock while the filesystem
  move awaits, then reacquires it to install `DocumentId`, display path, and
  file metadata atomically while the reservation still belongs to that
  operation. A failed move releases the reservation and leaves the old
  identity intact. The core store result reports filesystem and
  logical-document state; the bridge then
  uses the projection identity-acknowledgement operation added in Phase 1H2 for
  every attached projection.
  If the filesystem move succeeds but a projection notification fails, the
  logical document keeps the new identity, the bridge result reports the
  failed projection IDs, and the caller must refresh or detach them; it must
  not silently restore the old path or report complete synchronization.
- Save As is an explicit store operation with its own result; it is not a
  commit that accepts a different path. It reserves the normalized destination
  before I/O and rejects an existing or reserved retained-document identity.
  It releases the state lock while writing the new destination from one
  captured snapshot, then reacquires it to change the logical identity only
  after the destination succeeds and while the reservation remains owned.
  Later edits remain dirty under the new identity. A failed Save As releases
  the reservation and leaves the original
  identity, source path, dirty state, and projections unchanged. The bridge
  performs the projection acknowledgement step after the store operation; a
  projection-update failure after the destination succeeds is reported as
  partial synchronization with the new identity, not as a successful old-path
  save.
- Rename and delete requests carry the source key, version, and on-disk stamp
  from one snapshot. The store compares every retained source stamp immediately
  before filesystem mutation. A changed source returns
  `ExternalFileConflict`; it is never moved or deleted merely because an
  earlier prompt observed the old stamp. Directory operations preflight every
  retained descendant with the same rule.
- Delete is the one disk workflow that rejects logical replacement while it is
  active. After save/discard/cancel decisions, the bridge puts every affected
  projection behind a delete-specific read-only barrier on the UI dispatcher.
  The store then validates every affected key/version/stamp and atomically
  marks all affected document gates as delete-active before filesystem I/O. If
  any projection cannot enter the barrier or any document cannot be admitted,
  no delete begins and all applied barriers are released. `TryReplace` against
  a delete-active document returns `OperationInProgress`; a headless caller
  cannot bypass the barrier. Filesystem failure/cancellation retains every
  document and releases the gates/barriers. Success atomically retires every
  affected document before projections detach, so no edit can be accepted
  between deletion and key invalidation.
- `Reload` succeeds only for a clean document whose on-disk stamp changed. A
  dirty document returns `ExternalFileConflict`; it is never overwritten by a
  reload request. Reload results distinguish `Reloaded`, `Unchanged`,
  `StaleDocument`, `StaleDocumentInstance`, `DocumentNotFound`,
  `OperationInProgress`, `ExternalFileConflict`, `ReadFailed`, and `Cancelled`.
  Reload captures one expected key/version before reading. If a logical edit
  wins while the read is in flight, final admission returns `StaleDocument`
  with the atomic current snapshot and never overwrites that edit. A reload
  that changes logical content increments `Version` once and sets
  `PersistedVersion` to the new version.
- `ResolveExternalConflictAsync` is added with the Phase 1H1 controller prompt;
  ordinary commit/reload never gains a hidden force flag. Its request carries
  one current key/version and the observed external stamp returned by the
  conflict. `UseDisk` reads using the document's retained open/encoding options,
  rechecks key/version/stamp before one atomic content/format/baseline install,
  and returns `StaleDocument` if a logical edit wins during the read.
  `UseLogical` captures one current canonical snapshot and replaces the
  destination only when it still matches the observed stamp; later logical
  edits remain dirty exactly as for commit. A second external change returns
  `ExternalFileConflict`. Either successful choice returns one atomic current
  snapshot and clears the observed conflict; cancellation changes nothing.
  The same operation recovers `ReplacementStateUnknown` after the controller
  displays the newly observed destination state. Its statuses are
  `ResolvedWithDisk`, `ResolvedWithLogical`,
  `ResolvedWithUnsynchronizedProjection`, `StaleDocument`,
  `StaleDocumentInstance`, `DocumentNotFound`, `OperationInProgress`,
  `ProjectionNotSynchronized`, `ExternalFileConflict`, `ReadFailed`,
  `WriteFailed`, `ReplacementStateUnknown`, and `Cancelled`. If a domain
  projection becomes pending after preflight while either choice performs I/O,
  the bridge maps an otherwise successful result to
  `ResolvedWithUnsynchronizedProjection`, carries the resolution choice and
  blocking IDs, preserves that local projection as conflicted, and blocks
  close. It never reports that the disk and every projection agree.

### File I/O and store composition

- Disk-bound operations are asynchronous. Initial `OpenOrLoadAsync` and
  `CommitAsync`, plus `ReloadAsync`, `ResolveExternalConflictAsync`,
  `RenameAsync`, and `SaveAsAsync` when Phases 1H1/1H2 add them with their first
  callers, accept a `CancellationToken` and return `Task` result values. Use
  `Task` consistently in these contracts;
  do not introduce another return type without a measured allocation problem
  and a separate contract decision. Logical replacements, snapshot capture,
  and result publication remain synchronous where they do not perform disk I/O.
- The store depends on an injectable `IWorkspaceFileSystem` boundary for
  asynchronous read, stamp capture, temporary-file creation, flush, atomic
  same-directory replacement, and temporary cleanup. Rename and destructive
  path methods are added only with their first Phase 1H2 callers. The default
  implementation
  uses `FileStream` with asynchronous options and computes the content hash
  from the bytes read. Controllers, projections, and language providers do
  not call `File.ReadAllText`, `File.WriteAllText`, or equivalent workspace
  file APIs directly.

  The minimum seam is:

  ```csharp
  public interface IWorkspaceFileSystem
  {
    Task<WorkspaceFileReadResult> ReadAsync(
      string path,
      CancellationToken cancellationToken);

    Task<FileStamp> CaptureStampAsync(
      string path,
      CancellationToken cancellationToken);

    Task<WorkspaceTemporaryFile> WriteTemporaryAsync(
      string directory,
      ReadOnlyMemory<byte> content,
      CancellationToken cancellationToken);

    Task<WorkspaceFileReplacementResult> ReplaceAsync(
      WorkspaceTemporaryFile temporaryFile,
      string destinationPath,
      FileStamp expectedStamp,
      CancellationToken cancellationToken);

    Task DeleteTemporaryAsync(WorkspaceTemporaryFile temporaryFile);
  }
  ```

  `WriteTemporaryAsync` completes and flushes the temporary file before it is
  eligible for replacement. The test fake can pause between stamp capture and
  replacement, report an observed conflict, or report an ambiguous replacement
  outcome without using wall-clock timing. The interface does not promise an
  inter-process compare-and-swap; `ReplaceAsync` documents the platform
  primitive and returns the actual replacement state.
- For an existing destination, the default Windows implementation uses the
  platform replacement primitive that preserves the destination's required
  metadata contract. For a missing destination, it uses a same-directory
  atomic rename of the flushed temporary file. It never deletes the
  destination first as a replacement strategy. If the platform cannot perform
  the requested operation with a determinate result, the filesystem boundary
  returns `ReplacementStateUnknown` and leaves baseline advancement to the
  store's recovery path.
- Store dictionary access, path reservations, and short in-memory state
  transitions use one
  synchronous state lock. `TryGetSnapshot` uses only that lock and never waits
  for disk I/O. Each logical document separately owns a store-managed
  asynchronous operation gate that serializes disk workflows and their
  associated state transitions. No synchronous lock is held across `await`,
  and neither store lock nor gate is held while calling a projection,
  dispatcher, prompt, or language provider. The store captures the resulting
  immutable snapshot under the state lock and returns it in the operation
  result. It raises no UI-facing document-change event.
- Cancellation before the filesystem replacement begins returns `Cancelled`
  without changing logical or persisted state. Once replacement begins, the
  filesystem operation runs to a determinate result; cancellation is not
  converted into a false `Cancelled` result. An ambiguous OS result is always
  reported as `ReplacementStateUnknown`.
- The ScriptingStudio workspace composition registers exactly one
  `IWorkspaceDocumentStore` instance per workspace DI scope. The
  `AsyncServiceScope` that creates `IEditorDocumentController` and the bridge
  owns the store; only the bridge resolves that raw instance.
  `ScriptingStudioShell` is the explicit shutdown coordinator: its asynchronous
  stop path prevents new work and awaits bridge detachment and current bridge
  operations, then asynchronously disposes the workspace service scope. The
  async scope is the sole DI disposal owner and disposes the bridge before the
  store according to their explicit dependency/lifetime registration. The
  shell does not manually dispose the store and then synchronously dispose the
  same scope.
  Later view/lease slices register through the bridge and therefore join its
  existing stop barrier; they do not become store disposal owners. DI
  registration order is not a teardown contract: the bridge-to-store
  dependency and observable scope-disposal test prove the required order.
  Neither a view nor an individual document disposes the shared store.
- `ScriptingStudioServiceCollectionExtensions` must register the store at the
  same scoped composition boundary as `IEditorDocumentController`. A bridge
  composition test proves repeated bridge resolutions receive the same store,
  one-store-per-workspace behavior, and scope disposal after projection
  detachment. The test uses observable bridge/store disposal sentinels and
  calls the shell shutdown path twice to prove idempotence. A second
  independently constructed store in the same workspace is a composition
  failure.

### Retention and disposal

- The initial implementation retains every opened logical document for the
  lifetime of its ScriptingStudio workspace scope. Closing a view or a
  transient lease never evicts or disposes the logical document. This simple
  policy prevents orphaned projections and removes the need for pins,
  operation leases, and eviction bookkeeping in the first migration. The only
  earlier retirement is an explicit, user-confirmed Phase 1H2 destructive
  delete after its save/discard/cancel preflight; that is lifecycle behavior,
  not cache eviction.
- Eviction may be added only after Gate A when measured workspace memory
  pressure justifies it. That later slice must define projection, operation,
  and dirty-document eligibility together and prove that an old
  `WorkspaceDocumentKey` cannot affect a reopened document. No phase adds
  eviction merely to complete an anticipated cache abstraction.
- Store disposal is idempotent and is the only bulk operation that disposes
  retained logical document state. Phase 1H2 deletion may retire only its
  explicitly affected documents through the same store owner. After store
  disposal, mutating or commit operations throw
  `ObjectDisposedException`; read-only inspection of an already captured
  snapshot remains valid. UI adapters use the same throw policy for public
  operations rather than mixing throws and silent no-ops. An asynchronous
  operation admitted before disposal is cancelled and returns its existing
  operation-specific `Cancelled` outcome when that outcome is part of the
  contract; a new operation called after disposal throws immediately. Do not
  add a generic `Disposed` status to every result type.
- The async composition scope and store are the only owners allowed to dispose
  or retire a logical document. Views, projections, leases, and callers release
  their registrations instead. Workspace shutdown disposes documents only
  after views and projections have detached and in-flight operations have
  ended.

### Projection and conflict policy

- Source text in the logical document is the canonical serialized content.
  Parsing a text or string-table projection is read-only and records the
  source `Version` used for parsing. A parse failure leaves the prior
  projection intact and reports the failure.
- A grid edit publishes only when its recorded source version equals the
  document version. The publish is one full replacement produced by a pure
  serializer. A mismatch returns the store's `StaleDocument` or
  `StaleDocumentInstance` result without mutation, and the bridge marks the
  projection conflicted. A successful publish increments the document version
  once and marks the document dirty unless its serialized content is identical.
- A clean grid parse followed by no user edit never serializes back to source.
  This preserves comments, blank lines, generated headers, escapes, and
  original newline spelling even though the grid representation cannot retain
  all of them.
- `StringEditorView.Content` must not serialize `_viewModel.Sections` on every
  read and treat that lossy result as canonical source. The projection stores
  the canonical source snapshot it parsed and tracks whether a user edit has
  made the grid pending. Only an explicit publish operation may serialize the
  pending model; a clean projection returns the canonical source unchanged.
- When canonical text changes, a text projection refreshes immediately. A
  clean grid reparses; a grid with pending edits becomes stale and requires an
  explicit refresh or conflict resolution. No projection is silently copied
  over another projection's pending edits.
- String-table serialization uses a WPF-free serializer with explicit header,
  escape, embedded-newline, and newline options. Its minimum value contracts
  are:

  ```csharp
  public sealed record StringTableParseOptions(
    string EmbeddedNewlineToken = "\\n",
    string ParsedNewline = "\n");

  public sealed record StringTableSerializeOptions(
    string HeaderText,
    string Newline,
    string EmbeddedNewlineToken = "\\n",
    bool EscapeSemicolons = true);

  public sealed record StringTableParseResult(
    StringTableParseStatus Status,
    StringTableModel? Model,
    long SourceVersion,
    TextFileFormat SourceFormat,
    IReadOnlyList<StringTableParseDiagnostic> Diagnostics);

  public sealed record StringTableSerializationResult(
    StringTableSerializationStatus Status,
    string? Content,
    TextFileFormat FileFormat,
    IReadOnlyList<StringTableParseDiagnostic> Diagnostics);

  public enum StringTableParseStatus
  {
    Parsed,
    Invalid
  }

  public enum StringTableSerializationStatus
  {
    Serialized,
    Invalid
  }

  public sealed record StringTableParseDiagnostic(
    string Code,
    int Line,
    int Column,
    string Message);
  ```

  `StringTableModel` is a WPF-free value/domain model. `Parse` receives the
  source text and explicit parse options; `Serialize` receives the model and
  explicit header/newline/escape options. The header text, including any
  product or tool version, is supplied by the caller. The parser and
  serializer must not read `Application.ProductVersion`, `Environment.NewLine`,
  `DataGridView`, or other UI-global state. A parse result records the source
  document version and format, while serialization reports the format of its
  generated replacement. Invalid structural input returns `Invalid` with one
  or more typed diagnostics and a non-publishable result; it is never silently
  normalized into a different model. An invalid parse returns `Model = null`
  and an invalid serialization returns `Content = null`; callers must inspect
  the status before using either value. The parser accepts CRLF, LF, and CR
  input independently of the host operating system. The serializer emits only
  the explicitly requested `Newline` sequence and never calls
  `Environment.NewLine`.

  This is intentionally a ClassicScript codec, not a universal format API.
  The reusable extension point is the format-neutral workspace document and
  projection contract: a future language can provide its own WPF-free model,
  reader, writer, options, and diagnostics, then use the same snapshot,
  version, refresh, and publish flow. The store must not learn the grammar of
  any language, and this phase must not add a reader/writer registry or force
  unrelated formats into `StringTableModel`.

### Edit result policy

- Range positions use one-based line and column numbers and UTF-16 code-unit
  offsets, matching the language-server contract. Columns may point at the
  end of a line but not beyond it.
- Edits within one document are preflighted in ascending order. Any two
  non-empty ranges that overlap are rejected. Same-offset insertions and an
  insertion at a replaced range are also rejected unless the provider declares
  an explicit ordered-edit contract; descending-offset application is not a
  general policy.
- Every document and every edit is validated before any target mutates. A
  validation failure returns `ValidationFailed` with zero mutations.
- The initial coordinator returns `Completed` only when every target applies
  successfully and `PartiallyApplied` when a runtime target failure occurs
  after preflight. The result identifies changed targets and the failure; a
  before/after record is never called atomic.
- `RolledBack` is not part of the initial contract. Add it only with a concrete
  production target that can prove restoration of canonical content, version,
  dirty state, undo history, and projection acknowledgement, and only when a
  caller makes a distinct decision for that outcome.
- Undo history, dirty state, notifications, selection mapping, and optional
  disk commit are target policies applied after the shared preflight result.
  Headless and live targets must produce equal content for equal valid edits.

### Test topology and commands

- Phase 0C adds `Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj`.
  It explicitly targets `net8.0`, enables nullable, does not enable WPF or
  Windows Forms, and references only `TombLib/TombLib.Scripting.csproj`.
- Core store, snapshot, serializer, and pure edit-kernel tests run in the
  WPF-free `TombLib.Scripting.Tests` project created by Phase 0C. WPF lifecycle,
  projection, bridge, and host tests remain in
  `Tests/TombLib.Tests/TombLib.Tests.csproj` and
  `Tests/TombEditor.Tests/TombEditor.Tests.csproj` respectively. The bridge
  tests are in the `TombEditor.Tests.ScriptingStudio` namespace because that
  is the repository's existing TombIDE test host.
- Focused commands are:

```powershell
dotnet test Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"
dotnet test Tests/TombLib.Tests/TombLib.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"
dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"
dotnet build TombLib/TombLib.Scripting/TombLib.Scripting.csproj
dotnet build TombLib/TombLib.Scripting.UI/TombLib.Scripting.UI.csproj
dotnet build TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj
```

The first command is available only after Phase 0C creates the project. A
phase must name a narrower filter when one exists; a full solution build is a
final verification step, not a lifecycle exit criterion.

### Asynchronous operation contract

The following rules apply to persistence, diagnostics, completion, hover,
definition, signature-help, semantic-token, and language-server operations.
They are a behavior contract, not a requirement to replace an existing worker
with a new implementation:

- Every operation captures the immutable `WorkspaceDocumentKey`, logical
  `DocumentId`, document version, projection version when applicable, session
  generation, and cancellation token before starting work. A result carries
  the same identity tuple. A matching path alone is never sufficient; this
  also protects a future eviction/reopen implementation.
- A normal result publishes only when the document, projection, session, and
  owner are still current. A stale result is reported as `Stale` and is
  discarded without mutating content, dirty state, UI state, or persisted
  metadata.
- Cancellation is cooperative. A cancelled operation reports `Cancelled`,
  publishes nothing, and does not count as provider failure. Lower-level
  `OperationCanceledException` is normalized at the coordinator boundary.
- When a newer request supersedes a coalesced request, the older request
  reports `Superseded`; it must not publish an error or completion event after
  the newer request owns the slot. Only one mutation/persistence operation may
  be active for a logical document at one time.
- Provider or I/O failures report `Failed` with the original exception and
  useful document/request identity. They are handled at a boundary that can
  notify the host or retry; no worker catches and silently discards them.
- Disposal first prevents new requests, cancels pending work, and waits for
  in-flight dispatcher-free work to finish. An async owner exposes
  `DisposeAsync` when awaiting is required; a synchronous `Dispose` may remain
  only when it provides the same completion barrier without blocking the UI
  dispatcher. After disposal returns, no callback may publish or mutate a
  document or view.
- Dispatcher publication is an explicit final adapter step. Core work never
  captures a WPF control or creates a dispatcher. The UI adapter captures the
  intended dispatcher at composition time, checks generation/disposal again on
  that dispatcher, and publishes only a current result.
- Existing `ContentChangedWorker` and `ErrorDetectionWorker` task,
  cancellation, coalescing, and dispatcher behavior is preserved when it
  satisfies these rules. Modernize it only for a measured ownership,
  cancellation, error-reporting, or disposal defect, and add a focused test for
  that defect.
- Completion events such as `ContentChangedWorkerRunCompleted` must carry, or
  provide access to, the normalized operation outcome and request identity.
  A legacy `EventArgs` notification may remain only as a UI compatibility
  signal while all subscribers migrate; it must not be the sole way to
  distinguish success, cancellation, supersession, staleness, and failure.

## Document and Editor Ownership

The same ownership rules apply to all text-editing features:

- Normalize file identity before store lookup. On Windows, use a full path and
  case-insensitive comparison, while preserving the display path separately.
- An open file with unsaved changes must be edited through its canonical
  logical workspace document. Never reload disk content over that state.
- An unopened file should be loaded into a headless workspace document. A
  background operation must not create or activate a full editor merely to
  inspect or edit text.
- A caller that needs selection, scrolling, popups, or visible undo behavior
  must explicitly acquire an editor view lease for the canonical document.
- A view lease may be persistent or transient. A transient lease detaches and
  closes only the view it acquired; it never decides whether the logical
  document is discarded.
- The store retains a dirty document after its last view closes until the user
  saves, discards, explicitly closes the document, or the workspace is closed.
  The initial implementation retains clean documents for the same workspace
  lifetime; eviction is not part of the initial contract.
- Text views and domain views must observe one logical document. A domain view
  may maintain a parsed projection, but it must publish edits through the
  document boundary and must define reparse, serialization, and conflict
  behavior. `LastModified` is not an authority rule.
- File encoding, BOM, newline style, external reload, rename, and missing-file
  behavior belong to the document/store contract. A headless write must not
  silently change these properties.

The first document-oriented contracts are narrow and explicit. Production
callers exchange immutable snapshots and requests; the mutable logical
document remains an internal store implementation detail. The names may be
adjusted to match local conventions, but the ownership and result semantics
are fixed:

```csharp
public readonly record struct WorkspaceDocumentOpenOptions(
  TextEncodingKind NoBomEncoding,
  TextFileFormat NewFileFormat);

public sealed record WorkspaceOperationFailure(
  string Code,
  string Message,
  Exception? Exception = null);

public sealed record WorkspaceDocumentOpenResult(
  WorkspaceDocumentOpenStatus Status,
  WorkspaceDocumentSnapshot? Snapshot,
  WorkspaceOperationFailure? Failure = null);

public enum WorkspaceDocumentOpenStatus
{
  Opened,
  AlreadyOpen,
  InvalidPath,
  LoadFailed,
  Cancelled
}

public interface IWorkspaceDocumentStore : IAsyncDisposable
{
  Task<WorkspaceDocumentOpenResult> OpenOrLoadAsync(
    string filePath,
    WorkspaceDocumentOpenOptions options,
    CancellationToken cancellationToken = default);

  bool TryGetSnapshot(string filePath, out WorkspaceDocumentSnapshot snapshot);

  WorkspaceDocumentMutationResult TryReplace(
    WorkspaceDocumentReplaceRequest request);

  Task<WorkspaceDocumentCommitResult> CommitAsync(
    WorkspaceDocumentCommitRequest request,
    CancellationToken cancellationToken = default);

  WorkspaceDocumentMutationResult Discard(
    WorkspaceDocumentDiscardRequest request);
}

public interface IWorkspaceDocumentBridge : IAsyncDisposable
{
  Task<WorkspaceDocumentOpenResult> OpenOrLoadAsync(
    string filePath,
    WorkspaceDocumentOpenOptions options,
    CancellationToken cancellationToken = default);

  Task<WorkspaceDocumentBridgeOpenResult> OpenOrAttachAsync(
    string filePath,
    WorkspaceDocumentOpenOptions options,
    IWorkspaceDocumentProjection projection,
    CancellationToken cancellationToken = default);

  WorkspaceDocumentMutationResult Replace(
    WorkspaceDocumentReplaceRequest request);

  Task<WorkspaceDocumentCommitResult> CommitAsync(
    WorkspaceDocumentCommitRequest request,
    CancellationToken cancellationToken = default);

  void UnregisterOpenProjection(IWorkspaceDocumentProjection projection);
}

public interface IWorkspaceDocumentProjection
{
  event EventHandler<WorkspaceProjectionPublishRequestedEventArgs> PublishRequested;

  string ProjectionId { get; }
  string DocumentId { get; }

  WorkspaceDocumentKey? DocumentKey { get; }

  bool HasPendingEdits { get; }
  bool HasConflict { get; }

  WorkspaceProjectionAttachResult Attach(WorkspaceDocumentSnapshot snapshot);
  WorkspaceProjectionRefreshResult Refresh(WorkspaceDocumentSnapshot snapshot);

  WorkspaceProjectionRefreshResult AcknowledgePublish(
    WorkspaceDocumentMutationResult result);

  void Detach();
}

public sealed record WorkspaceDocumentSnapshot(
  WorkspaceDocumentKey DocumentKey,
  string DocumentId,
  string DisplayPath,
  long Version,
  long PersistedVersion,
  bool IsDirty,
  ITextSnapshot Text,
  TextFileFormat FileFormat,
  FileStamp OnDiskStamp)
{
  public bool ExistsOnDisk => OnDiskStamp.Exists;
  public string Content => Text.GetText(0, Text.TextLength);
}

public sealed record WorkspaceDocumentReplaceRequest(
  WorkspaceDocumentKey ExpectedDocumentKey,
  string DocumentId,
  long ExpectedVersion,
  string Content,
  TextFileFormat FileFormat);

public sealed record WorkspaceDocumentCommitRequest(
  WorkspaceDocumentKey ExpectedDocumentKey,
  string DocumentId,
  long ExpectedVersion,
  FileStamp ExpectedOnDiskStamp);

public sealed record WorkspaceDocumentDiscardRequest(
  WorkspaceDocumentKey ExpectedDocumentKey,
  string DocumentId,
  long ExpectedVersion);

public sealed record WorkspaceDocumentMutationResult(
  WorkspaceDocumentMutationStatus Status,
  WorkspaceDocumentKey RequestedDocumentKey,
  string RequestedDocumentId,
  long RequestedVersion,
  WorkspaceDocumentSnapshot? Snapshot);

public enum WorkspaceDocumentMutationStatus
{
  Replaced,
  NoChange,
  StaleDocument,
  StaleDocumentInstance,
  DocumentNotFound,
  OperationInProgress
}

public sealed record WorkspaceDocumentCommitResult(
  WorkspaceDocumentCommitStatus Status,
  WorkspaceDocumentKey RequestedDocumentKey,
  string RequestedDocumentId,
  long RequestedVersion,
  WorkspaceDocumentSnapshot? Snapshot,
  FileStamp? ObservedOnDiskStamp = null,
  IReadOnlyList<string>? BlockingProjectionIds = null,
  WorkspaceOperationFailure? Failure = null);

public enum WorkspaceDocumentCommitStatus
{
  Committed,
  CommittedWithUnsynchronizedProjection,
  StaleDocument,
  StaleDocumentInstance,
  DocumentNotFound,
  OperationInProgress,
  ProjectionNotSynchronized,
  ExternalFileConflict,
  WriteFailed,
  ReplacementStateUnknown,
  Cancelled
}

public sealed record WorkspaceDocumentBridgeOpenResult(
  WorkspaceDocumentBridgeOpenStatus Status,
  WorkspaceDocumentSnapshot? Snapshot,
  WorkspaceOperationFailure? Failure = null);

public enum WorkspaceDocumentBridgeOpenStatus
{
  Attached,
  AlreadyAttached,
  ProjectionInConflictState,
  InvalidPath,
  LoadFailed,
  AttachFailed,
  Cancelled
}

public sealed record WorkspaceDocumentKey(Guid Value);

public sealed record WorkspaceProjectionPublishRequestedEventArgs(
  WorkspaceDocumentReplaceRequest Request) : EventArgs;

public sealed record WorkspaceProjectionAttachResult(
  WorkspaceProjectionAttachStatus Status,
  WorkspaceOperationFailure? Failure = null);

public enum WorkspaceProjectionAttachStatus
{
  Attached,
  AlreadyAttached,
  Unavailable
}

public sealed record WorkspaceProjectionRefreshResult(
  WorkspaceProjectionRefreshStatus Status,
  WorkspaceOperationFailure? Failure = null);

public enum WorkspaceProjectionRefreshStatus
{
  Refreshed,
  MarkedStale,
  UpdateFailed
}

public interface IEditorViewLease : IDisposable
{
  WorkspaceDocumentKey DocumentKey { get; }
  string DocumentId { get; }
  EditorOpenDisposition Disposition { get; }
  bool IsActive { get; }
}

public sealed record EditorViewAttachResult(
  EditorViewAttachStatus Status,
  IEditorViewLease? Lease);

public enum EditorViewAttachStatus
{
  Attached,
  AlreadyAttached,
  Unavailable
}

public interface IEditorViewHost
{
  EditorViewAttachResult Attach(
    WorkspaceDocumentSnapshot snapshot,
    EditorViewLeaseOptions options);
}
```

The mandatory result surface is deliberately small. Add a distinct result
type only when a production caller must branch differently and the same phase
adds that branch and its focused test. The initial minimum statuses are:

- store open: `Opened`, `AlreadyOpen`, `InvalidPath`, `LoadFailed`, and
  `Cancelled`;
- bridge open/attach: `Attached`, `AlreadyAttached`,
  `ProjectionInConflictState`, `InvalidPath`, `LoadFailed`, `AttachFailed`,
  and `Cancelled`;
- document mutation: `Replaced`, `NoChange`, `StaleDocument`, and
  `StaleDocumentInstance`; an explicitly retired key returns
  `DocumentNotFound`; discard and a replacement blocked by active destructive
  deletion may additionally return `OperationInProgress`;
- commit: `Committed`, `CommittedWithUnsynchronizedProjection`,
  `StaleDocument`, `StaleDocumentInstance`, `DocumentNotFound`,
  `OperationInProgress`, `ProjectionNotSynchronized`,
  `ExternalFileConflict`, `WriteFailed`, `ReplacementStateUnknown`, and
  `Cancelled`;
- projection attach: `Attached`, `AlreadyAttached`, and `Unavailable`;
- projection refresh: `Refreshed`, `MarkedStale`, and `UpdateFailed`; and
- view attachment: `Attached`, `AlreadyAttached`, and `Unavailable`.

Calls made after disposal throw `ObjectDisposedException`; `Disposed` is not
repeated across every result enum. Serialization failure belongs to the
domain adapter that performs serialization and is surfaced before it raises
`PublishRequested`. Reload, external-conflict resolution, rename, Save As,
delete, and projection-identity result types are added in Phases 1H1/1H2 with
their first production callers, not in the initial store contract. Open/attach success statuses always carry a
non-null snapshot. Failure statuses carry a compact
`WorkspaceOperationFailure` when an exception or typed decoding/parse failure
exists; cancellation and ordinary stale/conflict statuses do not manufacture
exceptions. `ExternalFileConflict` and `ReplacementStateUnknown` carry an
observed stamp when one is available. Every mutation/commit result carries the
current immutable snapshot captured as part of that operation's final state
transition when the requested document still exists; only `DocumentNotFound`
has a null snapshot. Callers do not follow a result with a racy snapshot
lookup. A stale instance after the path has reopened carries the new current
snapshot; a retired key with no replacement returns `DocumentNotFound`.
`ProjectionNotSynchronized` is a bridge-only preflight outcome and carries the
IDs of every attached projection that is pending, conflicted, or has a
bridge-tracked callback failure not cleared by a later successful update. The
bridge does not call the store until that list is empty. If a projection
becomes unsynchronized after preflight while an admitted commit performs I/O,
the captured canonical snapshot may still be committed. Bridge postflight
maps that successful store result to
`CommittedWithUnsynchronizedProjection` and carries the blocking IDs; callers
report that the captured source was saved but must not close or claim all
projection edits are saved.

`WorkspaceDocumentSnapshot` may wrap the existing
`ITextSnapshot` implementation, but it must not be replaced by a separately
read content/version pair. Its `DocumentKey` must match the live document
instance as well as its `DocumentId` and `Version`. The store's `TryReplace`
rejects stale expectations without mutation and is the only logical content
writer. Its result contains the accepted/current snapshot from that same
transition. `CommitAsync`, `Discard`, the operation gate,
persisted-baseline advancement, and document disposal also remain store-owned.
The bridge is the
only application-facing acquisition and commit path, so a controller or
headless caller cannot bypass a known pending projection by resolving the raw
store directly.

The initial migration does not import arbitrary already-loaded controls. Every
production editor joins the canonical path in its own cutover slice by being
created without loading content and then passed to `OpenOrAttachAsync`.
Legacy controls remain entirely on the legacy path until that cutover; no
store-based headless caller is enabled for a file type whose active projection
has not migrated. This removes the need to reconstruct the current private
`ContentChangedWorker` persisted baseline or maintain a second loaded-view
import state machine. Tests may use an unloaded fake projection, but no
test-only store or import implementation defines production behavior.

Phase 1H1 introduces reload results; Phase 1H2 introduces rename, Save As,
delete, and batch-directory results with their first production callers. Every
result includes the originating `WorkspaceDocumentKey`, normalized document
ID, requested/current version, atomic current snapshot or affected snapshot
list, and observed stamp/failure/projection IDs where applicable. The exact
minimum statuses are binding in those two phase sections; an implementation
phase does not replace them with an untyped `Failed`. Store and bridge share
one result shape rather than creating parallel hierarchies.

`ReplacementStateUnknown` is a recovery state, not a successful commit. The
controller must inspect a fresh stamp, tell the user whether the destination
exists and whether the logical content is still dirty, and require an explicit
decision before retrying or discarding. No result may silently map this state
to `WriteFailed` or `Committed`.

The `WorkspaceDocumentCommitRequest` must be created from one captured
`WorkspaceDocumentSnapshot`. The implementation checks
`ExpectedDocumentKey`, `ExpectedVersion`, and `ExpectedOnDiskStamp` under the
document/store synchronization boundary and returns the typed conflict result
without advancing either baseline when any expectation is stale. All async
results carry the originating `DocumentKey`, `DocumentId`, and expected/current
version values needed to reject an old instance. If eviction is added after
Gate A, the same key check also rejects an operation against a reopened path.

`IWorkspaceDocumentProjection` is implemented by an open projection and is
the only bridge input needed to attach an unloaded view to canonical content.
Its implementation may depend on AvalonEdit, WinForms, WPF, or a domain grid,
but the interface and the store must not. Before `Attach`, `DocumentKey` is
null and the control has not loaded or serialized workspace content. `Attach`
initializes the projection from one canonical snapshot under publication
suppression; the bridge records the mapping only after that succeeds. Opening
a second projection for the same identity attaches it from the same store
snapshot rather than asking either view to supply authority.

`IWorkspaceDocumentBridge.DisposeAsync` is idempotent and detaches every
registered projection before completing. The workspace async scope owns final
service disposal; bridge disposal completes before store disposal, and a
projection never remains registered against a disposed store.

The projection protocol has one owner and one direction of authority:

1. The controller creates an unloaded projection and passes it, the requested
  path, and profile-specific `WorkspaceDocumentOpenOptions` to the bridge.
  The bridge rejects a projection that is already loaded, pending, conflicted,
  or registered; there is no loaded-view import fallback.
2. The bridge obtains the canonical snapshot from the store, calls `Attach`,
  and records the projection-to-document mapping only after `Attach` succeeds.
  Attach failure detaches the projection and leaves the controller's open-view
  collection unchanged.
3. The bridge is the sole production mutation facade. It runs each complete
  synchronous mutation workflow through the existing TombIDE
  `IUiDispatcherService.Invoke`: call store `TryReplace`, acknowledge the
  source projection, and then fan the result snapshot out to every other
  projection before the dispatcher action returns. Dispatcher serialization,
  rather than a second event stream or dispatcher abstraction, fixes the
  observable order. A headless mutation has no source projection and refreshes
  all attached projections. Store state locks and disk-operation gates are not
  held while the bridge invokes projections.
4. A domain projection serializes pending edits before raising
  `PublishRequested`; serialization failure remains local and never reaches
  the store. A text projection raises the same event after every user or
  programmatic content mutation, including typing, undo/redo, completion
  insertion, and edit-target application. The event carries one immutable
  `WorkspaceDocumentReplaceRequest`; there is no duplicate projection request
  shape. The event sender identifies the source projection. The bridge submits
  that exact request, calls the source projection's `AcknowledgePublish` with
  the atomic mutation result, excludes the source from its own fan-out, and
  refreshes other projections from the result snapshot. Attach, refresh, and
  acknowledgement mutations
  run inside the adapter's counted, re-entrant publication-suppression scope.
  A view-document mutation outside that scope is a local mutation and
  publishes exactly once.
5. `Detach` is idempotent and releases only the projection registration. It
  never evicts or disposes a dirty logical document. Bridge callbacks are
  never invoked while the store holds its state lock or operation gate, and all UI
  projection calls occur through the injected existing UI dispatcher. Detach
  unsubscribes first; a queued publish from an unregistered projection is
  rejected before store mutation.

Successful `Replaced` fan-out refreshes only projections whose acknowledged
version is older than the result snapshot. `NoChange` acknowledges the source
but does not refresh peers. Stale results acknowledge only the source with the
current snapshot and enter its conflict path; they do not pretend that the
rejected content changed peers. Commit performs its bridge preflight on the UI
dispatcher and returns `ProjectionNotSynchronized` with all pending,
conflicted, or uncleared bridge callback-failure IDs before it calls the store.
Commit completion is dispatched with the store's atomic current snapshot,
which may include later edits and remain dirty. It then rechecks attached
projection state: IDs that became unsynchronized during I/O produce
`CommittedWithUnsynchronizedProjection` without rewriting or rolling back the
committed captured source. These rules are covered by deterministic
interleaving tests; no test uses dispatcher timing or sleeps as an ordering
assertion.

`AcknowledgePublish` is result-bearing rather than a void callback. A source
returns `Refreshed` after recording an accepted version, `MarkedStale` after
retaining a rejected local edit, or `UpdateFailed` with a structured failure
code such as parse or view-update failure.
The bridge records/logs a failed acknowledgement, keeps that projection
unsynchronized for later commit preflight, and continues peer fan-out from the
canonical result snapshot. The same tracking applies to a failed peer refresh.
A later successful refresh/acknowledgement clears the tracked failure; detach
removes it. An exception is translated to the same structured projection
failure at the bridge boundary; it never escapes the UI dispatcher or causes
the accepted store replacement to be reported as rolled back.

A stale text publish is a retained conflict, not a silent overwrite. The
AvalonEdit document already contains the local edit when the store returns
`StaleDocument` or `StaleDocumentInstance`, so `AcknowledgePublish` preserves
that local content, records the competing canonical snapshot, sets
`HasConflict`, and suppresses subsequent canonical refreshes. Commit and
close-with-save from that projection are blocked until the conflict is
resolved. The controller presents exactly three choices:

- **Use local:** capture the projection's current serialized text and submit a
  new replacement against the recorded canonical key and version. Another
  concurrent change may return to the same conflict state.
- **Use canonical:** ask the bridge for the latest current snapshot at
  resolution time, refresh from that snapshot under suppression, and clear
  local pending/conflict state. The recorded competing snapshot is diagnostic
  provenance only and is never used after a newer canonical version exists.
- **Cancel:** leave local content and conflict state untouched; do not commit,
  refresh, close, or claim that the canonical document contains those edits.

A pending domain projection follows the same no-overwrite rule, but keeps its
parsed model rather than AvalonEdit text. Successful publication clears
pending/conflict state only after the projection acknowledges the resulting
canonical snapshot. Closing a conflicted projection requires the same explicit
resolution or a separately confirmed discard of its local pending state.

The final contracts may differ in naming, but content operations must depend on
store snapshots/requests and UI operations must depend on a view host. The
current `ITextEditorHost.OpenTextEditor()` boundary is too broad because it
forces every content operation through a visible editor. The WPF-free store
must not reference `EditorDocumentController`, `TextEditorBase`, or AvalonEdit.
The TombIDE bridge owns the mapping between a logical document and an attached
view; the core store never queries the controller and never invokes a UI
projection. The bridge and view host expose enough identity/version
information to reject stale projection writes.

The bridge registration order is mandatory:

1. Normalize the requested path and select its profile-specific open options.
2. Create the projection without loading or serializing workspace content.
3. Ask the store for the existing canonical snapshot or load/create it once.
4. Attach the projection from that snapshot under publication suppression.
5. Record the bridge mapping and add the view to the controller only after
  successful attachment. Failure detaches the partial projection and leaves no
  open-view registration.

The ordinary text-view opening sequence is also mandatory once Phase 1B2a
starts its authority cutover:

1. `EditorDocumentControllerCore` creates the view and its text projection
  without loading file content.
2. The controller calls `OpenOrAttachAsync`; the bridge performs the sequence
  above and reports attach failure before the controller adds the view to
  `_openEditors`.
3. The bridge subscribes to `PublishRequested` only after successful attach
  and owns that subscription until `UnregisterOpenProjection`.
4. Only a successful attach allows the controller to complete the open. The
  controller must not call `IEditorControl.Load` or read the workspace file
  directly on this path.

Before Phase 1B2a, Phase 1A-Bridge tests the bridge only with an unloaded fake
projection and scoped composition; it does not change the controller's legacy
open path. Phase 1B2a routes only editor profiles that are guaranteed to use a
text projection. A profile that can expose both text and a legacy domain view
waits for Phase 1C. A bridge-managed projection and a legacy-loaded projection
must never coexist for the same document identity.

This rule allows the two Phase 1A store slices to remain WPF-free. Phase 1A-Bridge
defines the TombIDE open-text bridge and registers projections.
Phase 1C defines the string-table adapter before Phase 1D routes callers
through the store. No store-based headless caller is enabled for an unmigrated
profile merely because its file is not currently visible.

Use the existing `TombLib.Scripting.Text.ITextSnapshot` and
`StringTextSnapshot` abstractions for the core store. `TextDocumentSnapshot` is
allowed only in the AvalonEdit projection adapter because it depends on the
UI project. Do not introduce a second snapshot model merely to give the new
store a different name. A workspace document may expose a store-specific
identity and version around an existing snapshot.

### Canonical mutation and file-I/O inventory

Every content mutation or workspace file write must be classified before the
authority cutover. The following inventory is normative for the migration:

| Current path | Classification | Final owner | Required migration/test |
| --- | --- | --- | --- |
| `TextEditorBase.SetContent`, `SelectedText`, `AppendText`, `ReplaceLine` | visible text mutation | live edit target through the logical document | equal content test when a concrete headless target exists, plus undo/selection UI test; otherwise test canonical live content |
| `FindAndReplaceViewModel.ReplaceWithDirection` and all-tabs traversal | visible selection mutation/read | canonical live edit target plus view-only selection/caret adapter | current-tab and all-tabs tests against text/grid, stale, dirty, and unsupported projections; no direct `SelectedText` write after cutover |
| `StringEditorView.UpdateContent` and `ContentBuilder.BuildContent` | domain projection refresh/publish | string-table projection and WPF-free serializer | clean parse preservation, explicit publish, stale publish, and serializer corpus tests |
| `ScriptingMessageService` visible append/insert/rename operations | workflow mutation requiring a view | store edit target plus an explicit view lease when caret/selection is required | provider tests for pre-open, open-clean, and open-dirty documents |
| `DocumentControllerTextEditorHost.TryGetTextDocument` | mutable headless compatibility path | snapshot/store query | remove in Phase 1D; no new callers or mutable store return type |
| `FileReloadCoordinator` direct reads and per-view replacement | external reload authority | store `ReloadAsync` and controller prompt | observed conflict, clean reload, dirty reload, read failure, and no swallowed exception tests |
| `FileExplorerViewModel.CreateNewFile` and `FileCreationViewModel.Accept` (`File.WriteAllText`/`File.Create`) | workspace document creation | bridge open-missing plus store commit using profile-specific format; Save As mode uses store `SaveAsAsync` | existing destination/reservation, initial content/format, cancellation, failed write, and no empty-file side effect before acceptance |
| `FileExplorerViewModel.RenameSelectedItem` file rename | workspace identity mutation | Phase 1H2 bridge/store rename workflow | retained/open/dirty projection, case-only rename, destination on disk, retained destination, concurrent open/reservation, move failure, and acknowledgement-failure tests |
| `FileExplorerViewModel.RenameSelectedItem` directory rename | batch workspace identity mutation | Phase 1H2 directory coordinator using store path reservations and one filesystem move | preflight every retained descendant, reserve every rebased destination, atomically re-key after move, and report all projection acknowledgement failures; zero state change on collision/move failure |
| `FileExplorerViewModel.DeleteSelectedItem` file/directory recycle operation | destructive workspace identity mutation | Phase 1H2 bridge preflight plus the existing recycle-bin filesystem executor | save/discard/cancel dirty or pending descendants, failed/cancelled delete, key invalidation after success, no orphan projection, and directory descendant coverage |
| `FileExplorerViewModel` watcher created/deleted/renamed events and `EditorDocumentController.CloseInvalidEditors` | external identity observation | bridge reconciliation/reload queue and controller prompt | watcher events never re-key, close, or overwrite a retained document independently; test external delete/rename for clean, dirty, pending, and conflicted projections |
| `EditorDocumentController.RestoreSession` backup `File.ReadAllText` plus direct `Content` assignment | auxiliary backup read followed by workspace mutation | backup reader remains auxiliary; restored text enters through version-checked store replacement | missing/corrupt backup, open dirty target, stale replacement, successful restore marked dirty, and backup cleanup tests |
| `ScriptingMessageService.CreateGeneratedFiles` | generated artifact output, not a workspace document | the ScriptingMessageService artifact-output boundary, optionally extracted to a concrete writer if that removes coupling | path traversal, directory creation, encoding, cancellation, and write-failure tests; it must not enter the document store |
| ClassicScript/GameFlow/TRX compiler and log I/O | compiler/artifact boundary | existing compiler or artifact service | retain outside document authority; audit only for accidental open-document overwrites |
| `ContentChangedWorker` backup writes (`File.WriteAllTextAsync`) | editor-auxiliary state, not a workspace document | existing persistence coordinator under the Phase 2A suppression scope | backup suppression and delayed-notification tests; it must not enter the document store |
| `BookmarkCoordinator` bookmark files (`File.ReadAllLines`) | editor-auxiliary state, not a workspace document | existing bookmark coordinator | retain outside document authority; no store migration and no new headless caller |
| language-server formatting/rename/reference edit producers | edit input | shared value-based edit kernel | producer payload audit, overlap policy fixtures, and zero-mutation validation tests |
| `FileExplorerViewModel.CreateNewFolder` and empty directory lifecycle | directory-only workspace structure | existing explorer filesystem workflow until a retained document is affected | path validation and filesystem failure tests; no document-store entry is created for an empty directory |

The inventory distinguishes workspace document persistence from generated
artifacts and compiler outputs. Moving every `File.*` call into the document
store would overreach the architecture; leaving workspace document I/O in
views or controllers would preserve the duplicate authority. Each remaining
direct write must have an explicit owner and a focused test before Gate A.

Directory operations are not inferred from watcher timing. Phase 1H2 treats a
directory rename as one preflighted identity batch: normalize every retained
descendant's rebased destination, reserve all destinations, perform one move,
then atomically re-key the retained set and return its snapshots. Any collision
or move failure releases every reservation and changes no logical identity.
Deletion first resolves every dirty, pending, or conflicted descendant through
the controller's save/discard/cancel policy. A successful delete detaches its
projections and removes the affected retained entries so their keys become
invalid; reopening a path creates a new document instance. This explicit
destructive removal is not cache eviction and does not introduce general
retention or pin policy. The existing recycle-bin executor remains a UI
filesystem concern behind a controllable Phase 1H2 operation boundary.

## Shared Edit Kernel and Side-Effect Policies

The live-document and headless-document paths must not become two independent
implementations of insertion and replacement. They are two targets for one
shared edit pipeline:

```text
Language/domain edit planner
    -> TextEditOperation[]
    -> shared edit applier
    -> TextEditTransaction
         |
         +-- live document target
         |     + undo history
         |     + dirty state
         |     + document notifications
         |
         +-- headless document target
               + no WPF events
               + optional disk commit

Optional view adapter
    -> caret, selection, scrolling, popups, and visual effects
```

- [ ] Keep range calculation, line matching, replacement preparation, edit
  ordering, overlap policy, offset mapping, and preflight validation in one
  shared document-oriented edit kernel.
- [ ] Represent edits as value-based operations rather than methods that
  require `TextEditorBase`.
- [ ] Use one low-level document applier for live and, only when a concrete
  caller justifies it, headless targets.
- [ ] Put undo grouping, dirty-state transitions, persistence scheduling,
  diagnostics scheduling, and notification behavior in target/commit policy,
  not in duplicate edit algorithms.
- [ ] Make `EditorProcessingMode` a side-effect policy applied around the same
  edit pipeline. It must not select a second implementation of insertion or
  replacement.
- [ ] Keep caret movement, selection restoration, scrolling, renderer
  invalidation, and popup behavior in an optional editor/view adapter.
- [ ] Require document-only services to accept a store snapshot/request or edit
  target instead of `TextEditorBase`.
- [ ] Audit every edit producer and decide the overlap policy. The default
  recommendation is to reject overlapping edits during preflight. If a
  provider depends on the current descending-offset behavior, normalize that
  provider or introduce an explicit ordered operation instead of preserving an
  implicit rule.
- [ ] Preflight every file and every edit before mutating any target. A
  validation failure must produce zero mutation and must not activate a view.
  Runtime failures report the targets that remain changed or whose final state
  is unknown; a snapshot record must not be called atomic merely because it
  has before/after content.
- [ ] Give each edit target the smallest capability contract needed to
  identify the logical document, expose current version/content for preflight,
  and apply a prepared change. Do not require rollback capability merely to
  make target implementations symmetrical.
- [ ] Make `TextEditApplicationResult` carry the status, prepared operation
  count, mutated target IDs, and failure information.
  `ValidationFailed` carries zero mutated targets; `Completed` means all
  targets applied; `PartiallyApplied` means at least one target remains changed
  or its final state is unknown.
- [ ] Do not expose `TextWorkspaceEditTransaction` as an atomic transaction
  based on its before/after snapshots. Rename it to a result/change record or
  document its non-atomic semantics before changing callers. Introduce a
  rollback status only under the proof rule in the Binding Decision Record.
- [ ] Preserve the distinction between an undo record, a dirty baseline, and a
  disk commit. Applying a workspace edit to an open document marks it dirty;
  committing a headless result to disk updates the persisted baseline only
  after the write succeeds.
- [ ] Keep text-document mutation independent from selection-based helpers such
  as `TextEditorBase.SetContent`, which are view operations.

The target-specific difference is policy, not algorithm: a live target updates
the canonical open document and its undo/dirty state, while a headless target
updates an in-memory document and may commit the result to disk. Both must
produce the same content for the same valid edit operations.

## Phase 0A - Inventory and Seam Map

Primary references:
[TextEditorBase.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.cs),
[IEditorControl.cs](TombLib/TombLib.Scripting.UI/Editors/IEditorControl.cs),
[ITextEditorHost.cs](TombLib/TombLib.Scripting.UI/Editors/ITextEditorHost.cs),
and [IEditorDocumentController.cs](TombIDE/TombIDE.ScriptingStudio/Controls/IEditorDocumentController.cs).

Scope: documentation and inventory only. Do not add production abstractions or
rename APIs in this phase.

- [ ] Run the inventory against the current working tree, not HEAD: an
  unrelated in-flight refactor may be the effective code baseline. Record the
  uncommitted worktree changes that touch the inventoried files so later
  slices can keep their own diffs separate.
- [ ] Inventory every public, protected, internal, and event member of
  `TextEditorBase` and `IEditorControl`.
- [ ] Classify each member as logical-document state, projection state, pure
  transformation, view operation, workflow coordination, or language behavior.
- [ ] Trace every `ITextEditorHost`, `OpenTextEditor`, `TryGetTextDocument`,
  `IEditorControl.Load`, `IsSilentSession`, `silentSession`, `LastModified`,
  `TombEngineLevelScriptService`, and
  `ContentChangedWorkerRunCompleted` caller.
- [ ] Trace `FileExplorerViewModel` and `FileCreationViewModel` create, rename,
  directory rename, recycle/delete, and watcher paths;
  `EditorDocumentController.RestoreSession` and `CloseInvalidEditors`; and
  `FindAndReplaceViewModel` reads/mutations. Classify each as document content,
  document identity, destructive lifecycle, auxiliary artifact, directory-only
  structure, or view-only behavior.
- [ ] Include `ScriptingMessageService`, `LuaTrackedDocumentStateService`,
  `LuaReferenceSearchService`, host registrations, all `ITextEditorHost`
  implementations, and their test doubles in the dependency map.
- [ ] Record which callers require a visible view and which can be headless.
- [ ] Record every alternate representation of a file, especially the
  string-table controls and source views.

### Phase 0A exit criteria

- [ ] The inventory names the replacement owner and migration target for every
  planned API removal.
- [ ] The dependency map identifies all projects and focused test files for
  each later phase.
- [ ] No production code was changed.

## Phase 0B - Authority, Projection, and Persistence Contract

Execution order note: Phase 0B appears before Phase 0C in this document but
executes after Phase 0C and the independent Phase 2A lifecycle repair (see
Recommended Execution Order).

Dependency: Phase 0A, Phase 0C, and Phase 2A.

Scope: turn the Binding Decision Record into a handover decision record and an
executable test matrix. Do not add a test-only document store, projection
bridge, or persistence state machine. The first executable implementation of
those rules is the production store in Phase 1A-Store; duplicating it in a fake
would make the tests prove the fake rather than the architecture. Existing
characterization tests from Phase 0C and 2A remain executable during this
phase.

- [ ] Copy every binding decision into a matrix row with the owning production
  slice, exact observable outcome, planned test class, and focused command.
- [ ] Assign normalized identity, display paths, document keys, monotonic
  versions, baseline-derived dirty state, immutable snapshots, stale
  replacement, concurrent first-open single-flight, retention, discard, and
  disposal cases to Phase 1A-Store.
- [ ] Assign profile-aware no-BOM encoding, BOM/newline preservation,
  missing-file stamps, asynchronous I/O, cancellation, external conflicts,
  temporary replacement, post-replacement stamps, and
  `ReplacementStateUnknown` cases to Phase 1A-Filesystem. Assign captured
  commit with a later edit and discard-during-commit to the same slice.
- [ ] Assign unloaded attach, no-op refresh, stale text conflict, explicit
  local/canonical/cancel resolution, simultaneous publish, attach failure,
  and detach cases to Phases 1A-Bridge and 1B2a. Assign ClassicScript parse,
  serialization, and domain-conflict cases to Phase 1C.
- [ ] Assign UTF-16 ranges, zero-length edits, overlap rejection, and pure
  preflight to Phase 1B; assign multi-file zero-mutation validation and
  runtime partial application to Phases 1B2a/1B2b.
- [ ] Assign file/directory creation, rename/Save As destination reservations,
  case-only rename, external delete/rename, session-backup restoration,
  directory descendant rebasing, recycle/delete, and destructive key
  invalidation to Phases 1H1/1H2. Every row names the controller prompt owner,
  operation result payload, and a deterministic failure/collision test.
- [ ] Record that initial retention is workspace-lifetime retention. Do not
  specify eviction, pins, or operation leases in this matrix; those require
  measured memory pressure and a later decision after Gate A.
- [ ] Define the composition-test contract for one store per workspace scope,
  shared reference identity, projection detachment before store disposal, and
  rejection of a second store instance. Implement that integration test in
  Phase 1A-Bridge, where the TombIDE bridge and shell shutdown path exist.
- [ ] Add Unicode tests for astral characters, combining marks, and mixed
  BMP/non-BMP text. Positions are UTF-16 code-unit positions, and a valid edit
  must not split a surrogate pair unless the language-server range explicitly
  requests that code-unit boundary. Snapshot length and offset mapping must be
  tested separately from grapheme-cluster expectations.
- [ ] Mark every new test with the `TextEditorBaseModernization` category and
  keep core tests in `Tests/TombLib.Scripting.Tests`. This requirement applies
  when the assigned implementation phase creates each test; Phase 0B itself
  adds no placeholder or ignored tests.

### Phase 0B exit criteria

- [ ] The contract has one answer for every authority and lifetime case.
- [ ] The contract explicitly covers a workspace edit against an open
  string-table representation.
- [ ] The contract explicitly distinguishes source text from normalized
  string-table serialization and includes a no-op grid parse case.
- [ ] Every matrix row names either a WPF-free production-store test in
  `Tests/TombLib.Scripting.Tests` or a projection/host test in
  `Tests/TombEditor.Tests` and identifies the phase that makes it pass.
- [ ] The planned store and projection APIs can reject stale writes.
- [ ] No test-only store or bridge duplicates a production state machine.
- [ ] Existing Phase 0C and 2A characterization tests still pass. Future store,
  filesystem, and projection tests are assigned to their implementation
  slices and use no timing-dependent sleeps.

## Phase 0C - Lifecycle and Baseline Test Contract

Scope: create the WPF-free test topology, characterize behavior that exists
today, and record lifecycle rules before ownership moves. Keep production APIs
on current names and do not require future logical-document behavior from a
store that has not been implemented.

- [ ] Define document and view states: construction, loading, active,
  processing-suppressed, closing, disposed, and retained-without-view.
- [ ] Characterize the current post-disposal behavior of `TextEditorBase`, the
  active WPF `StringEditorView`, and their workers. Record inconsistencies as
  inputs to later slices; do not make a future store policy pass against a
  test-only implementation.
- [ ] Document UI-thread requirements and the allowed asynchronous event
  boundaries. Distinguish dispatcher-free core work from WPF publication.
- [ ] Create `Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj`
  as the explicit `net8.0` non-WPF test project described in the Binding
  Decision Record, add it to `Tomb Editor.sln`, and verify that the solution
  configuration includes it for the supported test/build platforms. Do not
  move existing UI tests into it.
- [ ] Keep the new core test project limited to dispatcher-free document,
  snapshot, format, and lifecycle contracts. It must explicitly set
  `TargetFramework=net8.0`, `UseWPF=false`, `UseWindowsForms=false`, and
  reference only `TombLib/TombLib.Scripting/TombLib.Scripting.csproj`; Phase
  1C adds the pure ClassicScript codec and model to that same production
  project and does not add another project reference.
- [ ] Add only current WPF-free characterization that has a production subject,
  such as `StringTextSnapshot` text/length/offset behavior. Reserve logical
  document construction, dirty baselines, file stamps, retention, and store
  disposal for Phase 1A-Store and 1A-Filesystem tests.
- [ ] Extend the existing WPF characterization suite in
  `Tests/TombEditor.Tests/ScriptingStudio/ScriptingPhase0LifecycleTests.cs`.
  It covers load ordering, edits, open/close, multiple representations,
  external reload, background work, settings, dispatcher publication, and
  disposal for `TextEditorBase` and the active WPF `StringEditorView` without
  changing production behavior. Record the unregistered WinForms
  implementation in the cleanup inventory without treating it as a live
  editor.
- [ ] Characterize one current edit operation reaching the current live editor
  with its existing undo, dirty-state, and view side effects. Add live/headless
  parity only in Phase 1D when a concrete headless edit caller exists.
- [ ] Mark every new baseline test with the
  `TextEditorBaseModernization` category and record the exact filtered command
  in the phase update.

### Phase 0C exit criteria

- [ ] Baseline tests pass or their pre-existing failures are recorded.
- [ ] The characterization record distinguishes current content ownership,
  processing suppression, and host-level transient view behavior. Future
  document authority and composition-order tests are reserved for the store
  and TombIDE bridge slices.
- [ ] The new core test project and the WPF characterization test project build
  and their filtered commands are recorded in the phase update. Existing
  tests remain runnable through the commands in the Binding Decision Record.

## Phase 1A-Store - Implement the WPF-Free Document State Machine

Primary references:
[StringTextSnapshot.cs](TombLib/TombLib.Scripting/Text/StringTextSnapshot.cs),
the Phase 0B contract tests, and the existing `TombLib.Scripting` project
settings. UI bridge references are intentionally deferred to Phase 1A-Bridge.

Entry dependency: Phase 0B.

Allowed production scope: the narrow document/store contracts, the in-memory
state machine, the `IWorkspaceFileSystem` seam, and focused tests using a
controllable filesystem fake. Do not implement the real filesystem adapter,
add TombIDE references, register projections, migrate callers, add view
leases, implement string-table behavior, or change `LastModified`.

The core store must not reference `EditorDocumentController`,
`DocumentControllerTextEditorHost`, `TextEditorBase`, AvalonEdit, WPF, or
TombIDE. No Phase 1A-Store code inspects open editor controls.

- [ ] Implement normalized file identity and one store entry per logical file.
- [ ] Implement one internal single-flight reservation per normalized path.
  Concurrent first opens share one load; a waiting caller can cancel only its
  wait, and failed/cancelled loaders leave no document or orphan reservation.
- [ ] Implement immutable snapshots, an immutable `WorkspaceDocumentKey`,
  monotonic document versions, persisted content/format baselines, and
  baseline-derived dirty state. Undo-to-baseline becomes clean without
  decreasing the version.
- [ ] Implement store-owned version-checked replacement and discard. No public
  document handle exposes mutation or disposal. Every result returns the
  accepted/current snapshot captured in the same locked transition; the store
  exposes no document-change event.
- [ ] Implement result-bearing `OpenOrLoadAsync` outcomes for invalid paths,
  fake load failures, and cancellation; do not encode them as `null` or disk
  fallback and do not add projection-specific statuses to the store.
- [ ] Use a short synchronous state lock for dictionary and in-memory state.
  Never hold it across `await`, filesystem operations, events, or callbacks.
- [ ] Retain every opened logical document until workspace-store disposal.
  Implement idempotent asynchronous store disposal and the fixed post-disposal
  throw policy.

### Phase 1A-Store exit criteria

- [ ] Repeated and concurrent first lookup returns the same logical document
  for one normalized path and performs one filesystem read.
- [ ] Open-dirty content wins over disk content.
- [ ] Unopened lookup is headless and is covered by a non-UI test.
- [ ] Path normalization, version/baseline, undo-to-clean, stale key/version
  rejection, failed/cancelled single-flight retry, workspace-lifetime
  retention, atomic result snapshots, and disposal behavior are tested.
- [ ] The core-store project has no reference to AvalonEdit, WPF, or TombIDE.

### Phase 1A-Store focused validation

- [ ] Run `dotnet test Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` and inspect the store tests.
- [ ] Build `TombLib/TombLib.Scripting/TombLib.Scripting.csproj`.

## Phase 1A-Filesystem - Implement Document Persistence

Dependency: Phase 1A-Store.

Allowed production scope: the WPF-free real `IWorkspaceFileSystem` adapter,
profile-aware load/format detection, store commit, and focused filesystem
tests. Do not add UI composition or migrate callers.

- [ ] Implement asynchronous headless load and commit without constructing or
  activating a visible editor.
- [ ] Honor `WorkspaceDocumentOpenOptions`: BOMs override profile defaults;
  BOM-less ClassicScript files use Windows-1252 and BOM-less Lua/UTF-8 profiles
  use UTF-8. Invalid input returns `LoadFailed`; do not content-sniff ambiguous
  ASCII into a store-global fallback.
- [ ] Preserve newline and BOM metadata, represent missing files with
  `FileStamp.Missing`, and use the requested new-file format. Test that an
  explicit commit of a clean empty missing document performs one replacement
  and creates the file rather than taking the existing-file clean no-op path.
- [ ] Implement one per-document asynchronous operation gate, expected
  key/version/stamp checks, same-directory temporary write and flush, atomic
  replacement where supported, post-replacement stamp capture, and explicit
  observed-conflict/`ReplacementStateUnknown` results. Hold no state lock
  across `await`.
- [ ] Persist one captured snapshot per admitted commit. Permit later logical
  replacements while I/O runs, install the captured content/format as the
  persisted baseline on success, and return a current snapshot that remains
  dirty when later edits exist. Reject discard with `OperationInProgress`
  while a disk operation owns the gate; never interleave discard with baseline
  advancement.
- [ ] Use the controllable filesystem fake to test cancellation before
  replacement, observed external conflict, temporary cleanup, write/flush/
  replacement failures, post-replacement stamps, ambiguous replacement,
  edit-during-commit, discard-during-commit, and a second disk operation during
  commit. Assert `OperationInProgress` performs no second I/O. Use barriers,
  not sleeps or an OS timestamp-resolution assumption.
  external process.

### Phase 1A-Filesystem exit criteria

- [ ] Missing-file, profile encoding, BOM/newline, file-format metadata,
  commit-baseline timing, external conflict, cancellation, and replacement
  recovery tests pass.
- [ ] A successful commit advances both persisted content and format from the
  committed snapshot; a failed or stale commit advances neither.
- [ ] The production filesystem adapter remains WPF-free.

### Phase 1A-Filesystem focused validation

- [ ] Run `dotnet test Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` and inspect the filesystem tests.
- [ ] Build `TombLib/TombLib.Scripting/TombLib.Scripting.csproj`.

## Phase 1A-Bridge - Add the TombIDE Bridge and Workspace Composition

Primary references:
[ITextEditorHost.cs](TombLib/TombLib.Scripting.UI/Editors/ITextEditorHost.cs),
[DocumentControllerTextEditorHost.cs](TombIDE/TombIDE.ScriptingStudio/TextEditing/DocumentControllerTextEditorHost.cs),
[ScriptingStudioServiceCollectionExtensions.cs](TombIDE/TombIDE.ScriptingStudio/Composition/ScriptingStudioServiceCollectionExtensions.cs),
[ScriptingStudioShell.cs](TombIDE/TombIDE.ScriptingStudio/Host/ScriptingStudioShell.cs),
[IUiDispatcherService.cs](TombIDE/TombIDE.Shared/Messaging/IUiDispatcherService.cs),
and [WorkbenchComponents.cs](TombIDE/TombIDE.ScriptingStudio/Workbench/WorkbenchComponents.cs).

Dependency: Phase 1A-Filesystem. This is the TombIDE integration slice for the already
validated WPF-free store. Do not migrate workspace-edit callers, view leases,
string-table publication, processing vocabulary, or `LastModified` here.

Allowed production scope: the TombIDE bridge, scoped composition registration,
and the existing shell/workbench shutdown entry points needed to make the
store lifetime observable. Add focused tests in
`Tests/TombEditor.Tests/ScriptingStudio`.

- [ ] Add the TombIDE projection bridge without changing a production
  controller open path. The bridge receives only an unloaded projection; it
  never asks a loaded view to supply source or persisted-baseline authority.
- [ ] Implement bridge `OpenOrAttachAsync` for a projection created without
  loading file content. It must load or resolve the canonical snapshot, attach
  under publication suppression, register only after success, and report an
  explicit failure. Test this with an unloaded fake projection only; the
  existing controller open path remains deferred to Phase 1B2a.
- [ ] Route attach, acknowledgement, and refresh through the existing
  synchronous `IUiDispatcherService.Invoke` boundary. For each replace, call
  the store, acknowledge the source, and fan out the atomic result snapshot to
  peers in one dispatcher action. Add no second dispatcher abstraction and
  subscribe to no store change event.
- [ ] Leave the legacy `TryGetTextDocument` implementation and callers
  unchanged and add no new caller. It remains outside the canonical path until
  Phase 1D removes it; no production service may start using the store before
  its profile's projection cutover.
- [ ] Register exactly one store per ScriptingStudio workspace composition
  scope and prove that every bridge resolution receives that instance. Do not
  inject it into the unchanged controller, legacy host, shell, or workbench
  merely to satisfy a composition test.
- [ ] Inject the raw `IWorkspaceDocumentStore` only into the bridge.
  Controllers, hosts, headless services, and the shutdown coordinator receive
  the bridge or narrower boundaries. A production constructor that resolves
  the raw store elsewhere is a composition-test failure.
- [ ] Make `ScriptingStudioShell` the explicit shutdown coordinator and store
  an `AsyncServiceScope` created by the shell factory. Its awaited stop barrier
  rejects new bridge work, waits admitted bridge operations, and detaches
  projections; `AsyncServiceScope.DisposeAsync` then remains the sole DI
  disposal owner and disposes the bridge before its store dependency. Do not
  manually dispose the store, invoke synchronous scope disposal, invent future
  lease/session owners, or rely on fire-and-forget/UI-thread blocking cleanup.
- [ ] Locate the awaitable host teardown entry point before wiring the
  ordered stop path. `ScriptingStudioShell.Dispose` is currently synchronous
  and host-called; the phase must identify or narrowly add the host
  close/workbench shutdown await point so the bridge stop barrier and async
  scope disposal complete before host teardown. A synchronous-only host path is a blocking
  finding: stop and add a repair slice instead of substituting a
  fire-and-forget or UI-thread-blocking workaround.
- [ ] Add a composition test with observable disposal sentinels that proves
  projection detachment precedes bridge/store scope disposal, repeated bridge
  resolutions receive the same store, async scope disposal occurs exactly
  once, and repeated shell shutdown is idempotent.
- [ ] Keep the WPF-free store free of AvalonEdit, WPF, `TextEditorBase`,
  `EditorDocumentController`, `DocumentControllerTextEditorHost`, and
  TombIDE references.

### Phase 1A-Bridge exit criteria

- [ ] `OpenOrAttachAsync` proves load/resolve, attach, registration ordering,
  acknowledgement-before-peer-fan-out, and explicit attach failure with an
  unloaded fake projection.
- [ ] Failed and throwing acknowledgement/refresh fakes are recorded as
  unsynchronized without escaping the dispatcher or stopping peer fan-out; a
  later successful update clears the tracked failure.
- [ ] A loaded, pending, conflicted, or already registered projection is
  rejected without mutation; there is no loaded-view import fallback.
- [ ] Bridge and workspace-scope service resolutions share one scoped store
  instance.
- [ ] The shell/workbench teardown test proves current bridge registrations
  detach before async scope disposal and no callback publishes after the
  awaited shutdown barrier. The shell never resolves or manually disposes the
  raw store.
- [ ] `TryGetTextDocument` has no new callers and remains on the unchanged
  legacy path until Phase 1D removes it.

### Phase 1A-Bridge focused validation

- [ ] Run `dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` and inspect the bridge and composition tests.
- [ ] Build `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`.

## Architecture Checkpoint A0 - Validate the Foundation Before Cutover

This human review runs after Phase 1A-Bridge and before Phase 1B. No production
controller, projection, workspace-edit caller, or headless query uses the new
foundation yet, so stopping here remains cheap and leaves no dual authority in
production. An executing agent prepares the record but cannot self-certify the
decision.

- [ ] Confirm concurrent first-open, failed/cancelled retry, stale key/version,
  captured commit with later edit, fail-fast operation-gate contention,
  discard-during-commit, and disposal tests are deterministic and use barriers
  rather than sleeps.
- [ ] Confirm the store is WPF-free, has no UI-facing event, and returns atomic
  snapshots in operation results. Confirm the bridge alone receives the raw
  store and uses the existing `IUiDispatcherService` for source
  acknowledgement before peer fan-out.
- [ ] Confirm the async scope is the sole DI disposal owner, the awaited bridge
  stop barrier detaches projections first, and repeated shell shutdown is
  idempotent without blocking the UI thread.
- [ ] Record the exact new production surface: public/internal types, result
  statuses, adapters, registrations, synchronization owners, and non-test
  lines. For each item, name the Phase 1B2a/1C/1D/1H1/1H2 consumer and the duplicate
  authority or dependency it is expected to remove.
- [ ] Review the fake-projection tests for implementation leakage. They must
  prove observable attach/order/conflict/disposal behavior, not mirror private
  dictionaries or locks.
- [ ] Record one of: `Continue` (the foundation is bounded and production
  cutover may begin), `Repair` (one named foundation defect gets a focused
  repair slice), or `Stop and remove foundation` (remove unconsumed store/
  bridge production code in a cleanup slice, retain characterization and Phase
  2A fixes, and defer Phases 1B-8B).

### Architecture Checkpoint A0 exit criteria

- [ ] A human-signed decision and measurement record exists.
- [ ] `Continue` is required before Phase 1B starts; it authorizes only Phase
  1B, not the remaining map.
- [ ] No unresolved duplicate identity, unordered projection publication,
  synchronous-scope teardown, or ambiguous operation result remains.

## Phase 1B - Extract the WPF-Free Edit Kernel

Primary references:
[TextWorkspaceEditApplier.cs](TombLib/TombLib.Scripting.UI/Editing/TextWorkspaceEditApplier.cs),
[TextWorkspaceEditTransaction.cs](TombLib/TombLib.Scripting.UI/Editing/TextWorkspaceEditTransaction.cs),
[TextEditorEditHelper.cs](TombLib/TombLib.Scripting.UI/Editing/TextEditorEditHelper.cs),
and [TextWorkspaceEditApplierTests.cs](Tests/TombEditor.Tests/ScriptingStudio/TextWorkspaceEditApplierTests.cs).
The producer audit must also cover
[TextWorkspaceCommandService.cs](TombIDE/TombIDE.ScriptingStudio/TextEditing/TextWorkspaceCommandService.cs),
[TextDocumentFormatterProvider.cs](TombLib/TombLib.Scripting.UI/Editing/TextDocumentFormatterProvider.cs),
the language-server edit providers, and their focused tests.

Dependency: Architecture Checkpoint A0 with a human-recorded `Continue`.

Allowed production scope: value-based edit preparation, validation, mapping,
and result types in `TombLib.Scripting`, plus pure kernel tests in
`Tests/TombLib.Scripting.Tests`. Do not change the current UI target,
multi-file application behavior, tab synchronization, or projection
synchronization in this phase.

- [ ] Represent edits as value-based operations independent of
  `TextEditorBase`.
- [ ] Extract UTF-16 range conversion, ordering, overlap validation, and offset
  mapping from the host-bound applier. Selection mapping remains a separate UI
  concern and is not part of the kernel.
- [ ] Inventory every edit producer before changing behavior, including
  formatting, rename, reference edits, completion insertion, and language
  server responses. The inventory must name
  `TextDocumentFormatterProvider`, `TextWorkspaceCommandService`,
  `ScriptingMessageService` visible mutations, Lua reference/rename callers,
  completion insertion, and every language-server workspace-edit response,
  with its focused test or an explicit no-caller result.
- [ ] Add representative provider payload fixtures before enforcing overlap
  rejection: same-offset insertions, adjacent insertions, insertion inside a
  replacement, overlapping replacements, descending non-overlapping edits,
  and edits targeting an open string-table projection. Each producer must be
  assigned either the default rejection policy or a named ordered-edit
  contract with an owner and test.
- [ ] Implement the fixed overlap policy: reject overlapping ranges, same-
  offset insertions, and insertion/replacement intersections unless an edit
  producer declares an explicit ordered-edit contract.
- [ ] Return a pure preflight result that contains prepared operations and
  deterministic diagnostics; it must not mutate a document or require an STA.
- [ ] Keep the kernel limited to one logical document. Multi-file preflight and
  target application are Phase 1B2b responsibilities.

### Phase 1B exit criteria

- [ ] Pure edit preparation runs without WPF or an STA.
- [ ] Invalid ranges and overlap behavior are deterministic.
- [ ] No document is mutated by a failed single-document preflight.
- [ ] UTF-16 line/column and zero-length edit behavior is covered.
- [ ] The kernel result type does not claim atomicity or own undo, persistence,
  selection, or WPF behavior.

### Phase 1B focused validation

- [ ] Run `dotnet test Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` and inspect `TextEditKernelTests`.
- [ ] Build `TombLib/TombLib.Scripting/TombLib.Scripting.csproj`.

## Phase 1B2a - Cut Over the Text Projection and Live Edit Target

Primary references:
[TextEditorBase.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.cs),
[IEditorControl.cs](TombLib/TombLib.Scripting.UI/Editors/IEditorControl.cs),
[EditorDocumentControllerCore.cs](TombIDE/TombIDE.ScriptingStudio/Controls/EditorDocumentControllerCore.cs),
[FindAndReplaceViewModel.cs](TombIDE/TombIDE.ScriptingStudio/FindAndReplace/FindAndReplaceViewModel.cs),
and the Phase 1A-Bridge composition tests.

Dependency: Phases 1A-Bridge and 1B. This phase is the deliberate behavior migration
from direct AvalonEdit mutation to one bridge-managed text projection and live
target. It must not be described as a pure extraction. Multi-file coordination
is Phase 1B2b. Do not add a generic headless target: Phase 1D may add a narrow
headless adapter only when its concrete store-based caller is ready to use it.

Allowed production scope: the live UI target adapter, text projection,
text-only controller-open branch, bridge wiring, persistence-coordinator
demotion for that branch, and focused tests in `Tests/TombEditor.Tests`. Do not
migrate multi-file callers, leases, string-table projections, processing
vocabulary, or `LastModified` here.

- [ ] Implement one target-independent prepared-edit application contract and
  a live AvalonEdit target adapter with explicit side-effect policy.
- [ ] Implement the text projection's `ContentChanged` publication path. It
  must capture the post-mutation content, document key, expected version, and
  file format in one immutable request for typing, undo/redo, completion
  insertion, and programmatic edits. The bridge publishes that request through
  the store and acknowledges only the resulting snapshot; refresh/load edits
  are suppressed and never republished.
- [ ] Make the projection adapter the single writer to its AvalonEdit
  document. Every programmatic mutation (attach, refresh, acknowledgement,
  and edit-target application) enters the counted suppression scope
  synchronously before mutation and clears it afterward. Undo/redo,
  completion insertion, and programmatic text changes count as user
  mutations only when they occur outside the scope, so no publish is lost
  and none is duplicated.
- [ ] Record the full-content publication cost as an accepted tradeoff: one
  publish request and one snapshot capture are O(n) in document length per
  text mutation. Before the controller cutover, run a release-build benchmark
  over 100 warmed single-character insert/undo cycles in a 1 MiB document and
  record hardware, runtime, mutation-to-ack median/p95, and managed allocation
  per cycle. The handover threshold is p95 at most 50 ms and at most 8 MiB of
  transient managed allocation per cycle. If either threshold fails, stop and
  add a measured incremental-text repair slice; do not cut over `OpenFile` or
  add a persistent text structure speculatively.
- [ ] Route ordinary `EditorDocumentControllerCore.OpenFile` through
  `OpenOrAttachAsync` only for profiles guaranteed to create a text editor:
  create the editor/projection without loading content, load or resolve through
  the bridge, attach successfully, then add the view to the controller. An
  attach failure leaves no open editor and does not fall back to direct
  `IEditorControl.Load`. Profiles that can create a string-table/domain view
  remain wholly legacy until Phase 1C; bridge-managed and legacy-loaded views
  must not coexist for one identity.
- [ ] On the bridge-managed text path, demote
  `ContentPersistenceCoordinator` to backup/event scheduling from immutable
  store snapshots. It must not retain a second persisted-content baseline or
  calculate canonical dirty state. A compatibility `IsContentChanged` value,
  while still required by the UI, is derived from the store snapshot.
- [ ] Route `FindAndReplaceViewModel.ReplaceWithDirection` through the
  canonical live target instead of assigning `SelectedText` directly. Preserve
  its selection/caret behavior in the view adapter; all-tabs reads use bridge
  snapshots and never select an arbitrary stale view as content authority.
- [ ] Add category-marked tests for typing, undo/redo, completion or
  programmatic insertion, suppressed refresh, stale publish conflict, all
  three local/canonical/cancel resolutions, failed/successful text-only open
  ordering, and current/all-tabs find/replace authority. Prove each successful
  local mutation changes canonical content exactly once.

### Phase 1B2a exit criteria

- [ ] The live target produces the expected canonical content for equal valid
  edits, with undo and selection behavior covered separately.
- [ ] Ordinary text opens no longer call `IEditorControl.Load` before bridge
  attach, and every direct text mutation enters the version-checked publication
  path without feedback loops. Domain-capable profiles remain wholly legacy
  until Phase 1C.
- [ ] A stale text edit remains visible in an explicit conflict state until
  local, canonical, or cancel resolution; it is never silently overwritten.
- [ ] The 1 MiB publication benchmark meets both recorded thresholds, or a
  repair slice blocks the controller cutover.

### Phase 1B2a focused validation

- [ ] Run `dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` and inspect text projection, open-order, suppression, conflict, and live-target tests.
- [ ] Build `TombLib/TombLib.Scripting.UI/TombLib.Scripting.UI.csproj` and
  `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`.

## Phase 1B2b - Add Multi-File Edit Coordination

Primary references:
[TextWorkspaceEditApplier.cs](TombLib/TombLib.Scripting.UI/Editing/TextWorkspaceEditApplier.cs),
[TextWorkspaceEditTransaction.cs](TombLib/TombLib.Scripting.UI/Editing/TextWorkspaceEditTransaction.cs),
[TextWorkspaceCommandService.cs](TombIDE/TombIDE.ScriptingStudio/TextEditing/TextWorkspaceCommandService.cs),
and [TextWorkspaceEditApplierTests.cs](Tests/TombEditor.Tests/ScriptingStudio/TextWorkspaceEditApplierTests.cs).

Dependency: Phase 1B2a. This is the behavior-changing multi-file policy slice,
not a pure helper extraction. Allowed production scope is the smallest
multi-file coordinator required by existing callers and its focused tests. Do
not migrate string-table profiles, headless callers, leases, processing
vocabulary, or `LastModified` here.

- [ ] Resolve every target, capture every expected document version, and
  preflight every file and every edit before mutating any target or activating
  a transient view. A missing target, stale version, invalid range, overlap,
  or unsupported target capability is a validation failure with zero mutation.
- [ ] Return `ValidationFailed`, `Completed`, or `PartiallyApplied`. A runtime
  failure after complete preflight reports the changed/unknown target IDs and
  original failure. Do not add rollback infrastructure without a concrete
  production target and caller that satisfy the Binding Decision Record.
- [ ] Define the application order and target IDs in the result so a caller
  can explain which files changed. Do not report `Completed` until every
  target has acknowledged its final content and document version.
- [ ] Preserve one undo group per live document and keep selection restoration
  in the optional view adapter.
- [ ] Update the invalid-range test to assert zero mutation and replace the
  descending-offset overlap test with rejection coverage or an explicitly
  named ordered-edit producer contract.
- [ ] Rename `TextWorkspaceEditTransaction` to a non-atomic change-set or
  application-result name, or document that its current before/after record is
  non-atomic. It must not expose an `IsAtomic`-style claim.
- [ ] Add category-marked tests for all-target preflight, zero-mutation
  validation failure, complete application, runtime partial application,
  deterministic application order, and unknown target state. Record the
  concrete Phase 1D caller that will justify any later headless target instead
  of adding one for test symmetry.

### Phase 1B2b exit criteria

- [ ] Pure preflight remains covered by `Tests/TombLib.Scripting.Tests`.
- [ ] Multi-file validation failure produces zero mutation.
- [ ] Runtime failure tests distinguish complete and partial outcomes and
  identify every changed or unknown target.
- [ ] Existing snapshot replay behavior remains covered without implying
  database-style atomicity. Headless content parity is a Phase 1D exit
  criterion only after a concrete headless consumer is identified.

### Phase 1B2b focused validation

- [ ] Run `dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` and inspect `TextWorkspaceEditApplierTests`.
- [ ] Build `TombLib/TombLib.Scripting.UI/TombLib.Scripting.UI.csproj` and
  `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`.

## Phase 1C - Define and Implement Alternate-View Projections

Primary references:
[StringEditorView.xaml.cs](TombIDE/TombIDE.ScriptingStudio/Editors/ClassicScript/StringEditor/StringEditorView.xaml.cs),
[ContentReader.cs](TombIDE/TombIDE.ScriptingStudio/Editors/ClassicScript/StringEditor/ContentReader.cs),
[ContentBuilder.cs](TombIDE/TombIDE.ScriptingStudio/Editors/ClassicScript/Strings/ContentBuilder.cs),
[EditorDocumentController.cs](TombIDE/TombIDE.ScriptingStudio/Controls/EditorDocumentController.cs),
and [EditorDocumentControllerCore.cs](TombIDE/TombIDE.ScriptingStudio/Controls/EditorDocumentControllerCore.cs).
The legacy WinForms `Strings/ContentReader.cs` belongs to the unregistered
`StringEditor` and is a Phase 3C cleanup subject, not an active codec
reference.

Dependency: Phases 1A-Filesystem, 1A-Bridge, 1B2a, 1B2b, and 2A, plus Phase 0B. This phase
must complete before any store-based workspace-edit caller migration. Allowed
production scope:
representation bridging for string-table/source views, the WPF-free
string-table parser/serializer boundary in the existing `TombLib.Scripting`
project, and tests. Do not migrate
workspace-edit callers, transient leases, or remove `LastModified` authority
here.

- [ ] Define the string-table view as a parsed projection of the logical source
  text, not as a second authority. Parsing alone must not rewrite the logical
  document.
- [ ] Add a versioned refresh/publish path from logical document to grid view
  and from accepted grid edits back to logical content. Grid publication must
  serialize a full normalized replacement using the existing generated format.
- [ ] Define behavior for a stale grid projection, parse failure, serialization
  failure, and simultaneous edits. A stale pending grid remains conflicted and
  preserves its model until the user chooses local, canonical, or cancel.
- [ ] Define and implement the projection protocol from the Binding Decision
  Record: unloaded creation, canonical load/resolve, attach, registration,
  canonical refresh, pending-edit publish, acknowledgement, and idempotent
  detach. Bridge operation results carry one immutable snapshot and never
  invoke WPF while holding the store gate.
- [ ] Define no-op behavior: opening and parsing a grid without a user edit
  must preserve the original logical source text, even though the generated
  grid serialization is lossy.
- [ ] Define how generated headers, comments, blank lines, escaped semicolons,
  embedded newlines, and newline style are represented at the source and
  normalized-serialization boundaries.
- [ ] Ensure a workspace edit against a file open in a grid and text view
  updates or invalidates both representations deterministically.
- [ ] Replace the projection's independent persisted-content/dirty authority
  with store snapshots. Compatibility members such as `ApplyPersistedContent`
  may schedule or refresh UI state during migration, but they do not retain a
  second baseline or decide canonical dirty state.
- [ ] Preserve the Phase 2A load/reset suppression-scope semantics on the new
  attach/refresh path: attach, refresh, and acknowledgement mutations run
  inside the suppression scope, never raise a publish event, never create a
  backup, and never start the content worker. The grid load path keeps the
  suppressed mode entered before content replacement exactly as Phase 2A
  established it.
- [ ] Add a category-marked test proving that opening a clean grid, parsing,
  and closing without a user edit produces no backup write, no content-change
  event, and no content-worker run: a clean parse is a no-op projection attach,
  not an edit.
- [ ] Register the active `StringEditorView` as the domain projection through
  the TombIDE bridge. Change its controller branch to create the view unloaded
  and complete only through `OpenOrAttachAsync`; only then may the generic
  `OpenFile` path be considered store-backed for domain-capable profiles. A
  pending grid is discoverable through its bridge registration before any
  store caller is enabled. Confirm that the legacy WinForms `StringEditor` has
  no registration or production caller.
- [ ] Move string-table parse/serialize logic used by the projection boundary
  into the existing WPF-free `TombLib.Scripting` project under a
  ClassicScript-specific namespace. Keep `StringEditorView`,
  `StringTableSection`, and grid rows out of the library. The model, reader,
  writer, and diagnostics must not expose `DataGridView`, WPF collections,
  `Application.ProductVersion`, or `Environment.NewLine`. Keep this codec
  specific to the current ClassicScript INI-like format; do not add a Lua
  object model, format registry, or generic reader/writer hierarchy before a
  second concrete consumer requires one.
- [ ] Keep canonical source text, source version/format, and pending grid
  serialization as separate values in `StringEditorView`. `Content` and
  `ApplyPersistedContent` must follow the store's source/publish contract;
  direct grid serialization is not a clean-load side effect.
- [ ] Add the pure ClassicScript model, reader, writer, options, and typed
  diagnostics as ordinary source files in
  `TombLib/TombLib.Scripting/TombLib.Scripting.csproj`. Reuse the existing
  WPF-free project and its current test-project reference; do not create a new
  project or solution entry. Verify that the codec has no reverse dependency
  on `TombLib.Scripting.UI`, `TombIDE`, WPF, or Windows Forms.
- [ ] Define the executable grammar: section headers are non-empty bracketed
  lines matching the existing `[Name]` form; leading/trailing whitespace is
  accepted around a header; semicolon comments and blank lines are ignored
  inside sections; normal-section rows are the complete remaining line; an
  `ExtraNG` row is `integer: value`; `\x3B` decodes to `;` and `\n` decodes to
  the explicit parsed-newline option. Unknown escapes remain literal. A
  malformed header, malformed `ExtraNG` row, invalid integer, or duplicate
  section name returns a typed diagnostic with source line and column and does
  not produce a publishable model. Comment/blank preamble lines before the
  first section are accepted but are not part of the model; the projection's
  canonical source snapshot preserves them and normalized serialization never
  emits them. Other non-comment text before the first section returns a typed
  `UnexpectedPreambleText` diagnostic.
- [ ] Add the golden corpus under the WPF-free test project with exact
  success/failure, diagnostic, model, and normalized-output expectations for
  CRLF/LF/CR input, all requested output newline styles, comments, blank lines,
  generated headers, escaped semicolons, embedded newlines, normal sections,
  `ExtraNG` rows, malformed lines, unexpected preamble text, duplicate
  sections, and trailing text.

### Phase 1C exit criteria

- [ ] Grid/source synchronization is version-based rather than timestamp-based
  within the new projection path.
- [ ] Workspace edits cannot leave a known open projection silently stale.
- [ ] Parse, serialize, no-op parse, conflict, lossy-normalization, and
  failed-projection tests pass.
- [ ] Opening and closing a clean grid without edits produces no backup, no
  content-change event, and no content-worker traffic; the Phase 2A
  suppression scope remains in force on the attach/refresh path.
- [ ] WPF-free serializer tests cover explicit header/version input,
  `CrLf`/`Lf`/`Cr` output, escaped semicolons, embedded newline tokens,
  comments, blank lines, and the absence of `Environment.NewLine` or
  `Application.ProductVersion` dependencies.
- [ ] The serializer contract has a golden corpus containing normal sections,
  `ExtraNG` id/value rows, comments, blank lines, all supported input newline
  forms, escaped semicolons, embedded newlines, generated headers, malformed
  section headers, invalid `ExtraNG` IDs, duplicate section names, and trailing
  text. Each case records whether parsing succeeds, the diagnostic code and
  source line when it fails, the normalized model, and the exact output for
  each requested newline style. Unknown escape sequences are preserved as
  literal text; malformed structural lines are rejected with a typed parse
  diagnostic rather than silently dropped.
- [ ] The active WPF string-table implementation passes the
  source-preservation and stale-publish contract tests. The unregistered
  WinForms implementation has a recorded no-caller result and a cleanup
  decision; it is not treated as a second live projection.
- [ ] Domain-capable `OpenFile` paths create unloaded projections, attach before
  controller registration, and have no legacy-loaded/bridge-managed overlap.
- [ ] Text and grid views derive dirty state from one store baseline; no
  projection or persistence coordinator retains a competing baseline.

### Phase 1C focused validation

- [ ] Run `dotnet test Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` and inspect `StringTableSerializerTests`.
- [ ] Run `dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` and inspect `StringTableProjectionTests`.
- [ ] Build `TombLib/TombLib.Scripting/TombLib.Scripting.csproj`,
  `TombLib/TombLib.Scripting.UI/TombLib.Scripting.UI.csproj`, and
  `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`.

## Phase 1D - Migrate Text Workspace Edits to the Store

Primary references:
[DocumentControllerTextEditorHost.cs](TombIDE/TombIDE.ScriptingStudio/TextEditing/DocumentControllerTextEditorHost.cs),
[TextWorkspaceEditApplier.cs](TombLib/TombLib.Scripting.UI/Editing/TextWorkspaceEditApplier.cs),
[TextWorkspaceCommandService.cs](TombIDE/TombIDE.ScriptingStudio/TextEditing/TextWorkspaceCommandService.cs),
[ScriptingMessageService.cs](TombIDE/TombIDE.ScriptingStudio/Workbench/ScriptingMessageService.cs),
[TombEngineLevelScriptService.cs](TombLib/TombLib.Scripting.Lua/Documents/TombEngineLevelScriptService.cs),
and [LuaReferenceAndRenameTests.cs](Tests/TombEditor.Tests/ScriptingStudio/LuaReferenceAndRenameTests.cs).
The service migration also covers
[TombEngineLevelScriptServiceTests.cs](Tests/TombLib.Tests/Lua/TombEngineLevelScriptServiceTests.cs).
Also include [LuaTrackedDocumentStateService.cs](TombIDE/TombIDE.ScriptingStudio/Lua/LuaTrackedDocumentStateService.cs),
[LuaReferenceSearchService.cs](TombIDE/TombIDE.ScriptingStudio/Lua/LuaReferenceSearchService.cs),
the `ITextEditorHost` registration, and all host test doubles.

Dependency: Phases 1B2b and 1C. The projection registration and source/grid
conflict contract must be complete before this phase begins.

Allowed production scope: the text workspace-edit adapter, the document/store
bridge, and every direct headless/query caller that currently creates an
ad-hoc `TextDocument`. A document-only query service may be adapted to
`ITextSnapshot` in this phase. The `ScriptingMessageService` change is limited
to its headless level-definition query callback; its visible automation
callbacks remain for Phases 1F and 1G. Keep transient tab cleanup deferred to
Phase 1E.

- [ ] Keep the existing three-member `ITextEditorHost` as the visible-view
  boundary; it is already narrower than the earlier draft assumed. Do not
  split or rename it merely to satisfy this plan.
- [ ] Route workspace edits and headless queries through the canonical store.
- [ ] Replace the remaining read-only `TryGetTextDocument` caller with the
  snapshot-oriented service contract, then delete the concrete-only
  `DocumentControllerTextEditorHost.TryGetTextDocument` helper in this phase.
  It is not an `ITextEditorHost` member; update only the real caller and any
  test double for that caller's new snapshot boundary. No store or bridge API
  may return a mutable AvalonEdit document.
- [ ] Migrate `ScriptingMessageService` level-definition queries and adapt
  `TombEngineLevelScriptService` to the existing WPF-free snapshot contract
  rather than constructing an AvalonEdit `TextDocument` for unopened files.
- [ ] Migrate `LuaTrackedDocumentStateService`, `LuaReferenceSearchService`,
  host registrations, and all test doubles that depend on the broad host
  contract. Open-editor previews must use the canonical snapshot before
  falling back to disk.
- [ ] Classify every `ScriptingMessageService` callback as document-only,
  visible-view mutation, or generated-artifact output. The level-definition
  callback moves to the store in this phase; visible append/insert/rename
  callbacks remain in their provider phases but must be recorded as explicit
  store edit-target operations with a view lease where selection/caret state is
  required. `CreateGeneratedFiles` remains outside the document store behind a
  dedicated artifact writer with traversal and write-failure tests.
- [ ] Migrate `TombEngineLevelScriptService` and its tests from AvalonEdit
  `TextDocument` inputs to the shared `ITextSnapshot` contract. Do not add a
  second adapter that constructs a `TextDocument` from file contents.
- [ ] Keep open text and domain editors attached to the canonical logical
  document rather than choosing an arbitrary `TextEditorBase`.
- [ ] Apply the shared edit kernel through the Phase 1B2a live target. Add a
  narrow store-backed headless target here only if a concrete migrated caller
  requires target-side behavior beyond the store's value replacement; do not
  introduce one for parity tests alone.
- [ ] Mark open documents dirty without writing to disk unexpectedly.
- [ ] Commit unopened headless edits only through an explicit write operation
  that calls the store-owned `CommitAsync` and updates the persisted baseline
  only after success. No caller, projection, controller, or host may call the
  filesystem or implement a second commit path.
- [ ] Preserve selection mapping only for callers that explicitly supplied a
  view; headless callers must not activate a tab.

### Phase 1D exit criteria

- [ ] An unopened workspace edit completes without constructing a visible
  `TextEditorBase`.
- [ ] An open dirty text or string-table document is edited from its canonical
  state or returns the explicit projection conflict.
- [ ] Queries do not create tabs or change the active editor.
- [ ] All direct headless callers use the store/snapshot path; no independent
  `new TextDocument(File.ReadAllText(...))` authority remains.
- [ ] `TombEngineLevelScriptService` and reference-preview paths accept the
  canonical snapshot contract rather than creating ad-hoc AvalonEdit documents
  or reading disk over dirty content.
- [ ] Live/headless content parity and dirty/save semantics are tested when a
  concrete headless edit caller exists; otherwise record the deferred target
  and test the store's value replacement directly.
- [ ] The Phase 0A mutation inventory has no unclassified direct editor
  mutation or workspace file write. Every deferred visible mutation names its
  owning phase and focused test.

### Phase 1D focused validation

- [ ] Run `dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` and inspect the workspace-edit, bridge, and level-definition tests.
- [ ] Run `dotnet test Tests/TombLib.Tests/TombLib.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` and inspect the `TombEngineLevelScriptService` tests.
- [ ] Build `TombLib/TombLib.Scripting/TombLib.Scripting.csproj`,
  `TombLib/TombLib.Scripting.UI/TombLib.Scripting.UI.csproj`, and
  `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`.

## Phase 1E - Implement Explicit Transient View Leases

Primary references:
[StudioSilentActionService.cs](TombIDE/TombIDE.ScriptingStudio/TextEditing/StudioSilentActionService.cs),
[IEditorDocumentController.cs](TombIDE/TombIDE.ScriptingStudio/Controls/IEditorDocumentController.cs),
and [DocumentControllerTextEditorHost.cs](TombIDE/TombIDE.ScriptingStudio/TextEditing/DocumentControllerTextEditorHost.cs).

Dependency: Phase 1D. Document-only operations must already use the store
before transient view ownership is introduced.

Allowed production scope: lease contract/implementation and the host adapter.
Do not migrate automation providers or rename `IsSilentSession` here.

- [ ] Implement the fixed `IEditorViewLease` and `IEditorViewHost.Attach`
  contract from the Binding Decision Record: a lease records the exact
  acquired view, the previous active view, and its persistent/transient
  disposition. The store's initial workspace-lifetime retention means the
  lease needs no document pin.
- [ ] Make lease disposal restore the previous active view and close only a
  view acquired by that lease.
- [ ] Replace `StudioSilentActionService` save/close inference with explicit
  lease ownership while preserving existing save and dirty-file behavior.
- [ ] Keep document-only queries and edits on the store path; only operations
  that need caret, scrolling, popup, or visible undo behavior may acquire a
  lease.

### Phase 1E exit criteria

- [ ] A lease test proves exact-view ownership, active-view restoration, and
  idempotent disposal.
- [ ] A pre-existing dirty tab is never saved or closed by lease cleanup.
- [ ] No automation provider caller was migrated in this phase.

## Phase 1F - Migrate ClassicScript and GameFlowScript Lease Callers

Primary reference:
[ScriptingMessageService.cs](TombIDE/TombIDE.ScriptingStudio/Workbench/ScriptingMessageService.cs)
and the ClassicScript/GameFlowScript workspace automation providers.

Dependency: Phase 1E. Migrate only ClassicScript and GameFlowScript callers in
this prompt; leave Lua and TRX untouched.

- [ ] Replace their cleanup inference with explicit leases.
- [ ] Keep document-only queries headless and acquire a lease only for visible
  editing, caret, scrolling, or selection behavior.
- [ ] Route visible append, insert, line-replacement, and rename operations
  through the canonical document edit target before applying view-only caret or
  selection effects. Add tests proving that an already-open dirty text or
  string-table projection is not bypassed by a direct control mutation.
- [ ] Preserve save, dirty-file, and active-view behavior for each provider.
- [ ] Add focused provider tests for pre-open, already-open-clean, and
  already-open-dirty scenarios.

### Phase 1F exit criteria

- [ ] ClassicScript and GameFlowScript automation have no implicit tab cleanup.
- [ ] Their focused provider tests pass without changing Lua or TRX callers.

## Phase 1G - Migrate Lua and TRX Lease Callers

Primary reference:
[ScriptingMessageService.cs](TombIDE/TombIDE.ScriptingStudio/Workbench/ScriptingMessageService.cs)
and the Lua/TRX workspace automation providers.

Dependency: Phase 1F. Migrate only Lua and TRX callers in this prompt.

- [ ] Replace their cleanup inference with explicit leases.
- [ ] Preserve Lua language-server document tracking and TRX provider behavior
  while adding visible-view ownership only where required.
- [ ] Route every visible Lua/TRX mutation through the canonical document edit
  target and record any provider-specific ordered-edit contract in the Phase
  1B producer inventory. View leases may add selection or popup effects only
  after the document mutation succeeds.
- [ ] Add focused provider tests for pre-open, already-open-clean, and
  already-open-dirty scenarios.

### Phase 1G exit criteria

- [ ] Lua and TRX automation have no implicit tab cleanup.
- [ ] All four providers now use the same lease contract with isolated tests.

## Phase 1H1 - Cut Over Persistence, Close, and Reload Authority

Primary references:
[EditorDocumentControllerCore.cs](TombIDE/TombIDE.ScriptingStudio/Controls/EditorDocumentControllerCore.cs),
[EditorDocumentController.cs](TombIDE/TombIDE.ScriptingStudio/Controls/EditorDocumentController.cs),
and [FileReloadCoordinator.cs](TombIDE/TombIDE.ScriptingStudio/Editors/FileReloadCoordinator.cs).

Dependency: Phases 1C-1G and Phase 0B. This phase cuts over content persistence
and close/reload authority. Path identity and destructive lifecycle follow in
Phase 1H2.

Allowed production scope: save, save-all, close, reload, backup restoration,
and content synchronization authority. Do not change file/directory identity
operations or start settings, capability, or async extraction here.

- [ ] Replace every content-authority use of `LastModified`, including
  save-all, synchronization, close-time propagation, and any related save
  ordering, with the canonical logical-document pointer and document version.
- [ ] Define and implement the resulting close policy: closing one of several
  views never copies an arbitrary view into another; closing the final view
  prompts against the logical document's dirty state. The prompt outcome maps
  to store operations: Save runs `CommitAsync` from a freshly captured
  snapshot; Discard runs store `Discard` and then closes the requested views;
  Cancel retains the document and its views. Save and Discard update canonical
  state, but no close path evicts a logical document during the initial
  workspace-lifetime retention model.
- [ ] Define and implement save-all once per logical document, reload for clean
  documents, and explicit conflict handling for dirty documents. Before any
  commit, the bridge returns `ProjectionNotSynchronized` with every pending or
  conflicted projection ID and every uncleared bridge callback-failure ID
  instead of persisting a source snapshot those projections have not accepted.
  If one becomes unsynchronized during I/O, bridge postflight returns
  `CommittedWithUnsynchronizedProjection`; close remains blocked until that
  projection publishes, refreshes successfully, or is explicitly discarded.
- [ ] Add reload request/result types with the first production caller. Reuse
  the common key, identity, version, stamp, snapshot, and failure payload. Use
  the complete reload status set from the Binding Decision Record; do not map
  stale/not-found/in-progress outcomes to `ReadFailed`.
- [ ] Add `ResolveExternalConflictAsync` request/result types only with the
  controller prompt that consumes them. Implement the exact Binding Decision
  Record statuses and require the user to choose `UseDisk` or `UseLogical`;
  ordinary save/reload exposes no force flag. Before `UseLogical`, bridge
  commit preflight requires all projections synchronized. Before `UseDisk`,
  the controller explicitly resolves or confirms discard of each pending or
  conflicted projection, then the successful atomic disk install refreshes
  attached projections on the dispatcher. A callback failure remains tracked
  and blocks close.
- [ ] Specify and implement ordinary-save staleness handling: Save captures
  the current snapshot immediately before `CommitAsync`. Admission rejects an
  already stale key/version/stamp. Once admitted, the operation persists that
  captured content/format exactly once; an edit during I/O remains dirty in
  the atomic result snapshot and is never included by a hidden retry. The next
  explicit user Save may capture that newer state. `ExternalFileConflict` and
  `ReplacementStateUnknown` are always surfaced for explicit resolution.
- [ ] Preserve user choice when a dirty logical document conflicts with disk.
- [ ] Route `EditorDocumentController.RestoreSession` through an auxiliary
  backup reader followed by one version-checked store replacement. Restored
  text is dirty canonical content; direct `_currentEditor.Content` assignment
  is removed, and a stale/open-dirty target is never silently overwritten.
- [ ] Route `FileReloadCoordinator` through the store's stamped `Reload`
  result. The coordinator may own the prompt, but it must not read disk and
  apply content independently to each view. A clean reload updates the
  logical document once and projections consume that update; a dirty reload
  surfaces `ExternalFileConflict` for the prompt decision. Reload and I/O
  failures must be reported to the controller or logger, not swallowed by an
  empty catch block. A barrier-based read-in-flight test proves a winning
  logical edit returns `StaleDocument` and is not overwritten.
- [ ] Remove synchronization code that copies one arbitrary view into another
  as an authority mechanism; projections consume canonical-document updates
  and publish only through version-checked operations.
- [ ] Update titles, dirty notifications, and file-reload queue integration to
  consume logical-document state.
- [ ] Remove persisted-baseline ownership from
  `ContentPersistenceCoordinator` and any equivalent view worker. They may
  schedule backups and compatibility events from immutable store snapshots,
  but may not store persisted content, compare canonical dirty state, or
  advance a baseline. `IsContentChanged` is derived from the current store
  snapshot until the compatibility property is removed.
- [ ] Audit `GetMostRecentlyModifiedEditorOfFile`,
  `IsMostRecentlyModifiedEditorOfFile`, every `LastModified` read/write, and
  `StudioSilentActionService`. A temporary `LastModified` property may remain
  only as presentation metadata; it must not select content, save order,
  reload behavior, or close propagation.

### Phase 1H1 exit criteria

- [ ] Multi-view synchronization, save-all, close, backup restore, reload, and
  external-conflict tests no longer depend on `LastModified` ordering.
- [ ] Barrier-based tests cover both external-conflict choices, a second disk
  change, a logical edit during `UseDisk`, a later edit during `UseLogical`,
  a domain projection becoming pending during either operation, cancellation,
  and `ReplacementStateUnknown` recovery without a hidden force write or racy
  follow-up snapshot lookup.
- [ ] The authority matrix from Phase 0B is implemented for all current view
  types.
- [ ] The old timestamp authority is removed from synchronization, or any
  remaining use is documented as presentation-only and cannot select a
  content authority.
- [ ] No projection, controller, `ContentPersistenceCoordinator`, or worker
  retains a persisted-content/format baseline independent of the store.

### Phase 1H1 focused validation

- [ ] Run `dotnet test Tests/TombLib.Tests/TombLib.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` and `dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"`; inspect save/edit interleaving, pending-projection preflight, close, reload, restore, and no-`LastModified` authority tests.
- [ ] Build `TombLib/TombLib.Scripting.UI/TombLib.Scripting.UI.csproj` and
  `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`.

## Phase 1H2 - Cut Over Path Identity and Destructive Lifecycle

Primary references:
[EditorDocumentController.cs](TombIDE/TombIDE.ScriptingStudio/Controls/EditorDocumentController.cs),
[FileExplorerViewModel.cs](TombIDE/TombIDE.ScriptingStudio/FileExplorer/FileExplorerViewModel.cs),
and [FileCreationViewModel.cs](TombIDE/TombIDE.ScriptingStudio/FileExplorer/FileCreationViewModel.cs).

Dependency: Phase 1H1. This is the final mandatory document-authority slice.
Allowed production scope is new-file creation, Save As, file/directory rename,
file/directory recycle/delete, external identity reconciliation, and projection
identity acknowledgement. Do not add general eviction, pins, settings,
capabilities, or sessions.

- [ ] Add rename, Save As, delete, and batch-directory operation request/result
  types with these first callers. Every result carries status, requested key/
  normalized identity/version, the atomic current snapshot or affected
  snapshot list when one exists, optional observed stamp, optional failed
  projection IDs, and optional `WorkspaceOperationFailure`. Do not create
  parallel store/bridge result hierarchies.
- [ ] Use these minimum statuses: rename has `Renamed`, `NoChange`,
  `InvalidPath`, `StaleDocument`, `StaleDocumentInstance`,
  `DocumentNotFound`, `OperationInProgress`, `ExternalFileConflict`,
  `DestinationExists`, `DestinationInUse`, `DestinationBusy`, `MoveFailed`,
  `ProjectionUpdateFailed`, and `Cancelled`; Save As has `SavedAs`,
  `InvalidPath`, `StaleDocument`, `StaleDocumentInstance`, `DocumentNotFound`,
  `OperationInProgress`,
  `ProjectionNotSynchronized`, `DestinationExists`, `DestinationInUse`,
  `DestinationBusy`, `WriteFailed`, `ReplacementStateUnknown`,
  `ProjectionUpdateFailed`, and `Cancelled`; delete has `Deleted`,
  `InvalidPath`, `StaleDocument`, `StaleDocumentInstance`, `DocumentNotFound`,
  `OperationInProgress`, `ProjectionNotSynchronized`, `ExternalFileConflict`,
  `DeleteFailed`, `ProjectionUpdateFailed`, and `Cancelled`. Batch directory
  results use the same destination/failure vocabulary plus all affected
  snapshots; no generic `Failed` status discards the cause.
- [ ] Reserve every normalized destination under the store lock before file
  I/O. A retained destination returns `DestinationInUse`; an in-flight
  reservation returns `DestinationBusy`; an unrelated on-disk destination
  returns `DestinationExists`. Release reservations on every failure and
  cancellation path. Add the filesystem move/delete/recycle seams only now,
  with these production callers.
- [ ] Implement case-only Windows rename as one document identity: preserve
  `WorkspaceDocumentKey` and normalized `DocumentId`, update `DisplayPath` and
  disk casing through the filesystem seam, and test the platform's required
  intermediate move without exposing an intermediate logical identity.
- [ ] Route `FileExplorerViewModel.CreateNewFile` through bridge open-missing
  plus commit with profile-specific initial content/format, and route
  `FileCreationViewModel` Save As mode through `SaveAsAsync`. Remove direct
  `File.Create`/`File.WriteAllText` workspace-document creation. Empty-folder
  creation remains an explorer filesystem operation and creates no document.
- [ ] Route explorer file rename through the store/bridge operation. For a
  directory rename, preflight every retained descendant, reserve every rebased
  destination, perform one filesystem move, then atomically re-key all
  descendants and acknowledge every attached projection. A collision or move
  failure changes no logical identity.
- [ ] Before file/directory recycle/delete, resolve every dirty, pending, or
  conflicted affected document through save/discard/cancel. On success detach
  projections and remove affected retained entries so their keys become stale;
  on failure/cancel keep all identities and registrations. This explicit user
  destruction is not cache eviction.
- [ ] Add only the delete-specific projection barrier needed by this caller:
  `SetDeleteBarrier(bool active)` returns
  `WorkspaceProjectionDeleteBarrierResult` with `Applied` or `Failed` and an
  optional `WorkspaceOperationFailure`. Enter barriers for all affected
  projections in one dispatcher action before store admission. The adapter
  makes its view read-only while active. A partial enter rolls back applied
  barriers and performs no I/O; a failed release or post-delete detach returns
  `ProjectionUpdateFailed` with every failed projection ID.
- [ ] Admit a file/directory delete by validating every expected key, version,
  and source stamp and marking every affected document delete-active in one
  state-lock transition. Use deterministic key ordering for any per-document
  resources. `TryReplace` returns `OperationInProgress` while active. Failure
  clears every gate and changes no identity; success retires all affected
  documents atomically before the bridge detaches projections.
- [ ] Treat file-watcher create/delete/rename notifications and
  `CloseInvalidEditors` as external observations. They enter bridge
  reconciliation/prompt flow and never independently re-key, clear, close, or
  overwrite a retained logical document.
- [ ] After a store identity operation succeeds, acknowledge projections on
  the UI dispatcher. If one fails, retain the new canonical identity and return
  `ProjectionUpdateFailed` with all failed IDs; never roll the filesystem back
  or report the old identity as current.
- [ ] Add the projection identity operation only with this first consumer:
  `AcknowledgeIdentity(WorkspaceDocumentIdentityChange change)` receives the
  old key and the atomic current snapshot, including its new key/display path,
  and returns `WorkspaceProjectionIdentityResult`. Its statuses are `Updated`
  and `Failed`; failure carries `WorkspaceOperationFailure`. A case-only rename
  may keep the normalized key while changing the display path. A general
  rename/Save As keeps `WorkspaceDocumentKey` but changes `DocumentId`, so a
  request carrying the old normalized identity can no longer address the
  document. The bridge updates its registration only after `Updated`;
  otherwise it records the failed projection ID for explicit refresh or
  detach.

### Phase 1H2 exit criteria

- [ ] New file, Save As, case-only/file/directory rename, recycle/delete, and
  external watcher tests cover clean, dirty, pending, conflicted, collision,
  cancellation, external source-stamp change, filesystem failure,
  projection-barrier failure, edit-during-delete rejection, and
  projection-acknowledgement outcomes.
- [ ] Concurrent open/rename/Save As tests prove one reservation owner and no
  duplicate retained `DocumentId`; directory failures leave every descendant
  on its original identity.
- [ ] No controller, view, `FileExplorerViewModel`, or `FileCreationViewModel`
  directly creates, moves, deletes, or rewrites a retained workspace document.
- [ ] Successful destructive deletion invalidates prior keys and reopening the
  same path creates a new document instance.

### Phase 1H2 focused validation

- [ ] Run `dotnet test Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"` and `dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization"`; inspect path-reservation, identity-batch, explorer, and destructive-lifecycle tests.
- [ ] Build `TombLib/TombLib.Scripting/TombLib.Scripting.csproj` and
  `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`.

## Architecture Gate A - Verify Document Authority

This is a required review checkpoint after Phase 1H2 and before Phase 2B. It is
not a production implementation phase and must not be combined with the next
alphanumeric phase. If the gate fails, add a small repair slice with its own test
before continuing.

- [ ] Confirm the ownership matrix is reflected in the code: the logical
  document/store owns content, version, dirty baseline, file metadata, and
  retention; projections own parse/serialize state; views own WPF state; the
  controller owns tabs and prompts. Processing/session ownership is not a Gate
  A prerequisite and remains deferred unless a later approved slice creates it.
- [ ] Confirm the WPF-free store has no dependency on AvalonEdit, WPF,
  `TextEditorBase`, `EditorDocumentController`, or TombIDE.
- [ ] Confirm the store has one composition owner and one instance per
  ScriptingStudio workspace async scope, with the bridge stop barrier detaching
  projection adapters before scope disposal. Gate A does not require a future
  session owner.
- [ ] Confirm the TombIDE bridge is the only open-projection lookup path and
  `TryGetTextDocument`, its implementation, and its mocks are removed. No
  bridge or store API returns a mutable AvalonEdit document.
- [ ] Confirm only the bridge receives `IWorkspaceDocumentStore` in production
  composition. The shutdown owner disposes the async scope rather than
  resolving the store; controllers, hosts, and headless services cannot bypass
  pending/conflicted projections through the raw store.
- [ ] Confirm ordinary `EditorDocumentControllerCore.OpenFile` creates a
  projection before content loading and completes only through
  `OpenOrAttachAsync`; no direct `IEditorControl.Load` path remains for a
  store-backed text or domain open.
- [ ] Confirm every text projection mutation, including typing, undo/redo,
  completion insertion, and programmatic edits, produces one immutable,
  version-checked publish request, while load/refresh/acknowledgement changes
  are suppressed.
- [ ] Confirm `CommitAsync`, persisted-baseline advancement, operation gates,
  and logical-document disposal have the store as their only owner. Document
  snapshots, projections, controllers, and hosts release registrations but
  never dispose documents or write workspace files directly.
- [ ] Confirm the bridge implements the complete projection protocol:
  unloaded creation, canonical load/resolve, attach, registration, canonical
  refresh, pending-edit staleness, version-checked publish, acknowledgement,
  conflict resolution, and idempotent detach. Store operations return atomic
  snapshots; there is no store callback/event path. The bridge acknowledges
  the source before peer fan-out and never calls UI code under a store gate.
- [ ] Confirm every snapshot, commit request, replace request, and async
  result carries `WorkspaceDocumentKey` as well as path/document version, and
  stale keys cannot affect another document instance.
- [ ] Confirm no independent `new TextDocument(File.ReadAllText(...))` path
  remains for headless queries.
- [ ] Confirm all workspace document reads/writes use the asynchronous
  filesystem boundary and that generated artifacts/compiler outputs are either
  explicitly owned outside the store or have been migrated with a named owner.
  No direct view/controller workspace I/O remains.
- [ ] Confirm the mutation inventory has a production owner and focused test
  for every visible editor mutation, direct workspace write, path-identity or
  destructive operation, watcher reconciliation, backup restore, and edit
  producer. Explicitly include `FindAndReplaceViewModel`,
  `FileExplorerViewModel`, `FileCreationViewModel`, `RestoreSession`, and
  `CloseInvalidEditors`.
- [ ] Confirm concurrent open/rename/Save As and directory-rebase tests prove
  one normalized destination reservation owner, and successful destructive
  deletion invalidates the old key without introducing general eviction.
- [ ] Confirm a clean grid parse does not rewrite source text and an accepted
  grid edit publishes through a version-checked normalized serialization.
- [ ] Confirm workspace edits against text and string-table views have one
  canonical content/version authority and deterministic stale/conflict tests.
- [ ] Confirm the old `LastModified` value cannot select a content authority.
- [ ] Confirm `ContentPersistenceCoordinator`, projections, and view workers
  do not retain or compare an independent persisted baseline. Store snapshots
  are the only source of canonical dirty state.
- [ ] Confirm initial retention is one documented workspace-lifetime policy;
  no unused pin, operation-lease, or eviction abstraction was added.

### Architecture Gate A continuation decision

The gate record must end with exactly one of these decisions:

- `Continue`: the document-authority migration produced a measurable net
  benefit, and the next planned slice may start.
- `Repair`: one narrowly identified authority, lifetime, or ownership defect
  remains; add a repair slice with its own focused test before continuing.
- `Stop and defer`: the completed migration is correct but did not reduce
  duplication or coupling enough to justify more layers. Keep the smallest
  useful extraction, mark Phases 2B-8A deferred, and do not add ports or
  sessions to justify the plan.

The gate record requires human sign-off. An executing agent may prepare the
record, the before/after measurements, and a recommendation, but it must not
self-certify `Continue` or Gate B's pass. The human operator records the
decision in this plan before the next slice starts; an uncertified gate
blocks the next slice.

`Continue` requires before/after measurements for duplicate content/version/
baseline authorities, direct `TextEditorBase`/AvalonEdit member-use edges in
migrated consumers, independent workspace I/O paths, and disposal owners. The
gate also counts every new interface, public result type, result status,
adapter, and composition registration introduced by Phases 1A-Store through
1H2, and names the production consumer and removed dependency or authority
that justifies each one.

The record includes concrete counts captured with the same search commands
before migration and after Phase 1H2: `new TextDocument(` constructions in
`TombIDE`; `File.ReadAllText`, `File.WriteAllText`, and `File.ReadAllLines`
call sites in `TombIDE.ScriptingStudio`; `LastModified` reads/writes on editor
types; independent persisted-baseline fields; and members that can dispose the
same logical document. A single lower count is not enough by itself. Continue
only when duplicate authorities are zero, the measured coupling/I/O surface
meaningfully decreases, every retained new abstraction has a concrete
consumer, and the reviewer judges the maintenance cost lower overall. The
default recommendation is `Stop and defer` unless a later slice has a named
defect or dependency it will remove. This is the first net-benefit gate;
Architecture Gate B remains the second gate for any later optional capability
and session layers.

### Architecture Gate A exit criteria

- [ ] A short gate record identifies the production owner and focused test for
  each row of the ownership matrix, and records `Continue`, `Repair`, or `Stop
  and defer`.
- [ ] No later slice is approved while two code paths can independently load,
  mutate, or persist the same logical file.
- [ ] The gate record includes the store composition scope, document-key
  incarnation tests, projection protocol tests, observed-conflict/
  `ReplacementStateUnknown` tests, and the direct-mutation inventory result.
- [ ] The gate record includes new-surface counts and a before/after authority,
  coupling, I/O, and maintenance-cost comparison; one improved grep count is
  not accepted as proof of net benefit.
- [ ] A human reviewer records `Continue`, or a repair/deferral decision is
  added to this plan before Phase 2B starts. Phase 2B and every later slice
  require the recorded `Continue` decision unless the plan is explicitly
  resumed after repair.

## Phase 2A - Make Load and Reset Lifecycle-Safe

Primary references:
[TextEditorBase.Persistence.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Persistence.cs),
[TextEditorBase.Events.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Events.cs),
[ContentPersistenceCoordinator.cs](TombLib/TombLib.Scripting.UI/Documents/ContentPersistenceCoordinator.cs),
[TextDiagnosticsCoordinator.cs](TombLib/TombLib.Scripting.UI/Diagnostics/TextDiagnosticsCoordinator.cs),
[IEditorControl.cs](TombLib/TombLib.Scripting.UI/Editors/IEditorControl.cs),
and [StringEditorView.xaml.cs](TombIDE/TombIDE.ScriptingStudio/Editors/ClassicScript/StringEditor/StringEditorView.xaml.cs).

Dependency: Phase 0C. This independent repair executes before Phase 0B and the
document-store migration. It fixes the verified load-ordering bug in every
current `IEditorControl` implementation; it does not introduce future store
types, rename the public vocabulary, or migrate callers.

- [ ] Add an explicit load/reset scope that establishes suppressed processing
  before replacing document content.
- [ ] Apply the scope to `TextEditorBase` and the active `StringEditorView`;
  `IEditorControl.Load` must enter the requested mode before reading or
  replacing content, setting the persisted baseline, or invoking a worker.
  Assigning the mode after `UpdateContent` is explicitly forbidden. Record the
  legacy WinForms
  `StringEditor` as unregistered and out of the active lifecycle path.
- [ ] Stop delayed persistence scheduling and reset the persisted baseline in
  one documented operation.
- [ ] Prevent suppressed loads from publishing delayed persistence events,
  backup writes, diagnostics, or language-service traffic, including work
  queued before the replacement.
- [ ] Ensure the reset operation clears or invalidates pending delayed work and
  restores the prior processing mode only after the baseline and projection
  state are consistent.
- [ ] Preserve the current normal-load behavior outside the scope.

### Phase 2A exit criteria

- [ ] A suppressed load cannot raise a forbidden delayed notification during
  replacement or from work queued before replacement.
- [ ] A suppressed load in the active WPF string-table editor cannot create a
  backup or publish a content-change event while its grid is being rebuilt.
- [ ] Lua and other language-service consumers receive no suppressed-load
  update.
- [ ] Focused load-order, backup-suppression, and delayed-notification tests
  cover AvalonEdit and the active WPF string-table implementation.

## Phase 2B - Rename Processing State Across All Control Types

Primary references:
[IEditorControl.cs](TombLib/TombLib.Scripting.UI/Editors/IEditorControl.cs),
[TextEditorBase.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.cs),
[StringEditorView.xaml.cs](TombIDE/TombIDE.ScriptingStudio/Editors/ClassicScript/StringEditor/StringEditorView.xaml.cs),
[IEditorDocumentController.cs](TombIDE/TombIDE.ScriptingStudio/Controls/IEditorDocumentController.cs),
[EditorDocumentController.cs](TombIDE/TombIDE.ScriptingStudio/Controls/EditorDocumentController.cs),
and [EditorDocumentControllerCore.cs](TombIDE/TombIDE.ScriptingStudio/Controls/EditorDocumentControllerCore.cs).

Dependency: Phase 2A and Architecture Gate A with a recorded `Continue`
decision. This is a cross-project
processing-scope API cutover; do not add request-generation tokens or
speculative feature ports in the same prompt.

- [ ] Introduce one processing scope owner for `EditorProcessingMode` per
  editor lifecycle; controls expose the current mode but do not own an
  unbalanced public flag. The scope is the migration seam that Phase 7A may
  move into the session core without changing event semantics.
- [ ] Replace `IsSilentSession` with the finalized processing-mode contract on
  `TextEditorBase`, `IEditorControl`, and the active WPF string-table editor.
  Do not migrate the unregistered WinForms implementation; Phase 3C is the
  sole owner of its no-caller audit and removal.
- [ ] Replace `silentSession` parameters on `IEditorDocumentController`, the
  controller implementation, `EditorDocumentControllerCore`,
  `IEditorControl.Load`, and every editor load path with
  `DocumentLoadOptions` or an equivalent explicit scope.
- [ ] Ensure backup creation, persistence scheduling, diagnostics scheduling,
  delayed notifications, and result publication all consult the same
  processing-state owner.
- [ ] Keep `EditorOpenDisposition` out of document processing APIs; view
  disposition remains a host/lease concern.
- [ ] Migrate existing silent-session tests rather than duplicating them.

### Phase 2B exit criteria

- [ ] No active production editor, controller, or workspace path uses
  `IsSilentSession` or `silentSession`; the explicitly unregistered WinForms
  implementation remains only until the Phase 3C no-caller audit and removal.
- [ ] Normal and suppressed processing behavior is unchanged except for the
  intentional vocabulary and ownership correction.
- [ ] TombLib and TombIDE focused builds/tests pass.

## Phase 2C - Add Generation and Cancellation Boundaries

Primary references:
[ErrorDetectionWorker.cs](TombLib/TombLib.Scripting.UI/Diagnostics/ErrorDetectionWorker.cs),
[TextDiagnosticsCoordinator.cs](TombLib/TombLib.Scripting.UI/Diagnostics/TextDiagnosticsCoordinator.cs),
[TextCompletionController.cs](TombLib/TombLib.Scripting.UI/Completion/TextCompletionController.cs),
and the Lua request-generation implementation in
[LuaEditor.cs](TombLib/TombLib.Scripting.Lua/LuaEditor.cs).

Dependency: Phase 2B. Keep the existing task/timer implementation unless a
specific cancellation or ownership defect requires change; do not perform a
blanket `BackgroundWorker` rewrite.

- [ ] Make the ownership relationship explicit: document version identifies a
  logical content snapshot; projection version identifies the source version
  used to parse a view; session generation identifies operation ownership and
  invalidates asynchronous work. These counters must not be interchangeable.
- [ ] Add one session-generation source for load, replace, rename, and
  disposal boundaries. The document store remains the only document-version
  source.
- [ ] Capture generation and logical-document identity in persistence,
  diagnostics, hover, completion, definition, and signature-help requests.
- [ ] Cancel or invalidate pending work when content is replaced, the document
  changes, or the owner is disposed.
- [ ] Map Lua's existing `DocumentVersion` to the logical document version and
  its request-generation field to the session generation; remove or wrap
  duplicate counters rather than adding a third authority.
- [ ] Keep dispatcher publication at the UI boundary and reject results for a
  newer generation or different logical document.
- [ ] Use explicit completion outcomes at the coordinator boundary:
  `Completed`, `Cancelled`, `Superseded`, `Stale`, and `Failed`. Calls made
  after disposal throw `ObjectDisposedException`; already-admitted work uses
  its normal cancellation outcome. Do not publish cancellation or staleness as
  a diagnostic/provider error.
- [ ] Define the request identity tuple and outcome vocabulary here, but do
  not change worker event argument types or migrate completion-event
  subscribers. Phase 7B owns that event-shape migration.

### Phase 2C exit criteria

- [ ] Deterministic tests prove that stale results cannot mutate a newer
  document or a disposed view.
- [ ] Cancellation, provider failure, and normal completion have documented
  outcomes.
- [ ] Coalescing tests prove superseded requests complete without publication,
  and disposal tests prove that pending and in-flight work cannot publish
  after the owner has finished disposal.
- [ ] No dispatcher-free core object captures a WPF control.

## Phase 2D - Make Disposal and Initialization Deterministic

Primary references:
[TextEditorBase.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.cs),
[TextEditorBase.Navigation.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Navigation.cs),
[TextEditorBase.Diagnostics.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Diagnostics.cs),
and [TextEditorBaseDisposalTests.cs](Tests/TombLib.Tests/Editors/TextEditorBaseDisposalTests.cs).

Dependency: Phase 2C. Keep settings and capability extraction deferred.

- [ ] Apply the chosen post-disposal policy to diagnostics, tooltips,
  completion, bookmarks, `FilePath`, content, settings, and navigation entry
  points.
- [ ] Ensure disposal unsubscribes callbacks and stops all timers and workers.
- [ ] Await or otherwise complete cancellation of owned async work before
  disposal returns. Use `DisposeAsync` when a completion barrier cannot be
  provided synchronously without blocking the UI dispatcher.
- [ ] Make repeated hover and diagnostics initialization either reject the
  second initialization or dispose the previous controller before replacement.
- [ ] Add tests for every public/protected entry point whose behavior is part
  of the chosen policy, including replaced-worker collectibility.

### Phase 2D exit criteria

- [ ] No asynchronous callback can mutate a disposed editor.
- [ ] Repeated initialization has deterministic ownership and disposal.
- [ ] The focused editor, diagnostics, and disposal tests pass.

## Phase 3A - Define Settings Through the First Runtime Consumer

Primary reference:
[TextEditorBase.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.cs),
[TextEditorConfigBase.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorConfigBase.cs),
the active WPF string-table control
([StringEditorView.xaml.cs](TombIDE/TombIDE.ScriptingStudio/Editors/ClassicScript/StringEditor/StringEditorView.xaml.cs)),
and the existing editor configuration tests.

Scope: define value types, defaults, validation rules, and mapping only as
needed to migrate one named existing runtime consumer in the same change. Do
not remove public properties or migrate every caller. This slice is optional
after Architecture Gate A; if the handover cannot name the consumer, the
duplicated validation/default path it removes, and its focused test, defer the
slice without adding a settings type.

- [ ] Introduce an immutable settings model for editor presentation,
  IntelliSense, auto-closing, zoom, and editor behavior.
- [ ] Define a shared presentation/zoom/undo model that the active WPF
  string-table control can consume, plus capability-specific language settings
  for `TextEditorBase`. Do not force string-table controls to implement fake
  IntelliSense settings.
- [ ] Define defaults and null/range/cross-property validation at the model
  boundary.
- [ ] Define whether invalid settings are rejected as a whole or normalized,
  and document the choice.
- [ ] Define effective IntelliSense feature state separately from configured
  feature preferences.
- [ ] Define zoom range and direct-assignment behavior, including
  `MinZoom <= MaxZoom` and out-of-range `Zoom` values.
- [ ] Add model-level tests that do not require constructing a WPF editor.
- [ ] Migrate the selected production settings-application caller to the model
  in this slice and delete its superseded default/validation logic. Keep a
  narrow compatibility mapping only when another current caller still needs
  it, and name that caller for Phase 3B.

### Phase 3A exit criteria

- [ ] The settings model has one documented source of defaults and invariants.
- [ ] Effective IntelliSense and zoom behavior are testable independently of
  the control.
- [ ] One production consumer uses the model, one duplicate settings path is
  removed, and the before/after dependency or validation-owner count is
  recorded. A model plus tests alone does not complete this slice.

## Phase 3B - Apply Settings Through One Runtime Path

Primary references:
[TextEditorBase.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.cs),
[TextEditorConfigBase.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorConfigBase.cs),
the active WPF string-table control, and
[TextEditorSettingsValidationTests.cs](Tests/TombLib.Tests/Editors/TextEditorSettingsValidationTests.cs).

Dependency: Phase 3A. Keep the existing public compatibility surface until
all callers are migrated; do not combine this with capability extraction.

- [ ] Apply settings through one atomic runtime path.
- [ ] Make direct changes and configuration application produce the same
  effective runtime state, or explicitly remove direct mutation.
- [ ] Make the IntelliSense master setting gate completion, hover,
  diagnostics, and signature help at the runtime boundary.
- [ ] Centralize zoom calculation and notification behavior.
- [ ] Test configuration application, direct changes, effective feature gates,
  notifications, and the active WPF string-table settings adapter.

### Phase 3B exit criteria

- [ ] Invalid settings cannot leave the editor in an unusable intermediate
  state.
- [ ] Direct and configured settings have the same runtime meaning.
- [ ] Focused settings tests pass.

## Phase 3C - Migrate Callers and Remove Configuration Compatibility

Primary references: all derived editor configuration types, the active WPF
string-table control, and their focused tests under `TombLib/TombLib.Scripting.*`,
`TombIDE`, `Tests/TombLib.Tests`, and `Tests/TombEditor.Tests`.

Dependency: Phase 3B. Do not start unrelated control slimming.

- [ ] Migrate derived editors and configuration callers to the settings
  contract.
- [ ] Migrate `StringEditorView` to the shared presentation/zoom/undo settings
  subset without adding language capability dependencies it does not support.
- [ ] Own the no-caller audit and remove the unregistered WinForms
  [StringEditor.cs](TombIDE/TombIDE.ScriptingStudio/Editors/ClassicScript/Strings/StringEditor.cs)
  and its control-only helpers when no shared parser/serializer dependency
  remains. This is the only phase that removes that legacy implementation.
- [ ] Remove redundant mutable configuration properties only after every
  caller has a replacement.
- [ ] Update serialization or persisted configuration mapping where required.
- [ ] Remove compatibility shims that are no longer used and add API-level
  tests for the intended public surface.

### Phase 3C exit criteria

- [ ] No production caller depends on the removed compatibility properties.
- [ ] Configuration persistence and all focused editor tests pass.

## Phase 4A - Extract Text and Line Transformations

Primary references:
[TextEditorBase.Editing.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Editing.cs),
[TextEditorEditHelper.cs](TombLib/TombLib.Scripting.UI/Editing/TextEditorEditHelper.cs),
and the existing line/edit services under
`TombLib/TombLib.Scripting.UI/Editing`.

Dependency: Phase 1B2b. Do not move WPF selection, scrolling, or popup behavior.

- [ ] Inventory line matching, section lookup, replacement calculation, word
  and offset helpers, and line operations.
- [ ] Move text-only algorithms to focused services or value-based operations
  that accept text, snapshots, ranges, or WPF-free line metadata. Do not
  expose AvalonEdit `DocumentLine` from extracted transformations.
- [ ] Return edit descriptions containing replacement text and any required
  caret/selection mapping rather than mutating the editor from the algorithm.
- [ ] Keep low-level application on the shared edit applier from Phase 1B2b.
- [ ] Add non-WPF tests for normal, empty, boundary, and invalid input cases.

### Phase 4A exit criteria

- [ ] Text and line transformations run without an STA or WPF host.
- [ ] No extracted helper owns editor state, events, timers, popups, or disposal.

## Phase 4B - Extract Language-Independent Editing Rules

Primary references:
[TextLineCommentService.cs](TombLib/TombLib.Scripting.UI/Editing/TextLineCommentService.cs),
[TextAutoClosingService.cs](TombLib/TombLib.Scripting.UI/Editing/TextAutoClosingService.cs),
[TextEditorFormattingService.cs](TombLib/TombLib.Scripting.UI/Cleaning/TextEditorFormattingService.cs),
and the completion insertion/auto-indentation callers.

Dependency: Phase 4A. Keep language providers and view application separate.

- [ ] Extract formatting, commenting, auto-closing, completion insertion, and
  auto-indentation calculations that do not require WPF.
- [ ] Keep language-specific skip rules as explicit provider/capability inputs,
  not hidden references to `TextEditorBase`.
- [ ] Keep caret movement, selection restoration, undo grouping, and renderer
  invalidation outside the transformation services.
- [ ] Add parity tests proving equal text results for live and headless targets
  with intentionally different view side effects only when a concrete
  headless target exists; otherwise keep the rule as a deferred Phase 1D test.

### Phase 4B exit criteria

- [ ] The extracted rules have deterministic non-UI tests.
- [ ] The shared edit applier remains the only low-level mutation path.
- [ ] Language-specific behavior is represented by explicit inputs or
  capabilities.

## Phase 4C - Complete the View/Edit Boundary Migration

Primary references:
[TextEditorViewService.cs](TombLib/TombLib.Scripting.UI/Editors/TextEditorViewService.cs),
[TextEditorBase.Editing.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Editing.cs),
and the current editor command callers.

Dependency: Phase 4B. Do not begin broad port extraction; this phase only
removes document responsibilities that already have a tested replacement.

- [ ] Keep `SelectLine()` and all selection, scrolling, caret, popup, renderer,
  and dispatcher work behind one explicit view/editor port.
- [ ] Rename or split `TextEditorViewService` if it has accumulated unrelated
  document logic.
- [ ] Remove `TextEditorBase` document-operation methods only after all callers
  use the tested replacement.
- [ ] Update command, language-writer, and formatting callers in focused
  batches rather than changing every feature at once.

### Phase 4C exit criteria

- [ ] View mutation remains in one explicit UI-facing boundary.
- [ ] Pure transformations remain independently testable.
- [ ] Removed forwarding members have no remaining production callers.

## Phase 5A - Define Ports From Concrete Consumers

Primary references:
[TextEditorServiceComposition.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorServiceComposition.cs),
[IEditorControl.cs](TombLib/TombLib.Scripting.UI/Editors/IEditorControl.cs),
and the coordinators under `TombLib/TombLib.Scripting.UI`.

Scope: identify and define only ports required by one immediately selected
consumer. Do not create a complete port taxonomy in this phase.

- [ ] For each selected coordinator, list the exact editor members it uses.
- [ ] Replace only those member dependencies with narrow ports such as
  `IEditorDocumentPort`, `ITextEditTarget`, `IEditorViewPort`,
  `IEditorSessionState`, or `ICompletionWindowHost` when the usage proves the
  boundary is real.
- [ ] Keep AvalonEdit-specific implementations in the UI project.
- [ ] Keep language providers independent of WPF controls and popups.
- [ ] Reject ports that only rename `TextEditorBase` or aggregate unrelated
  capabilities.

### Phase 5A exit criteria

- [ ] Every new port has one production implementation, one focused consumer,
  and one test seam.
- [ ] No speculative or general-purpose editor service was added.

## Phase 5B - Port Persistence and Diagnostics Coordinators

Primary references:
[ContentPersistenceCoordinator.cs](TombLib/TombLib.Scripting.UI/Documents/ContentPersistenceCoordinator.cs),
[TextDiagnosticsCoordinator.cs](TombLib/TombLib.Scripting.UI/Diagnostics/TextDiagnosticsCoordinator.cs),
[ErrorDetectionWorker.cs](TombLib/TombLib.Scripting.UI/Diagnostics/ErrorDetectionWorker.cs),
and their current tests.

Dependency: Phase 5A and Phase 2C. Keep completion and presentation ports
deferred.

- [ ] Stop passing the concrete editor to persistence and diagnostics code when
  a document/session or result-publication port is sufficient.
- [ ] Keep scheduling, cancellation, generation, and stale-result decisions in
  coordinators rather than providers.
- [ ] Add fakes for document content, processing mode, generation, and
  diagnostic publication.
- [ ] Preserve the event contract consumed by host services, or migrate every
  subscriber in the same focused slice.

### Phase 5B exit criteria

- [ ] Persistence and diagnostics core tests run without constructing the full
  WPF editor wherever the port permits.
- [ ] Existing UI tests still cover dispatcher publication and render invalidation.

## Phase 5C - Port Completion and Presentation Boundaries

Primary references:
[TextCompletionController.cs](TombLib/TombLib.Scripting.UI/Completion/TextCompletionController.cs),
[CompletionWindowCoordinator.cs](TombLib/TombLib.Scripting.UI/Completion/CompletionWindowCoordinator.cs),
[EditorToolTipPresenter.cs](TombLib/TombLib.Scripting.UI/Presentation/EditorToolTipPresenter.cs),
and [TextEditorViewService.cs](TombLib/TombLib.Scripting.UI/Editors/TextEditorViewService.cs).

Dependency: Phase 5B. Do not extract language capabilities yet.

- [ ] Separate completion/hover data and scheduling from popup presentation.
- [ ] Introduce `ICompletionWindowHost` or an equivalent narrow adapter only
  for the actual popup operations it owns.
- [ ] Keep selection, scrolling, renderer, popup, and dispatcher behavior in
  view-facing ports or presenters.
- [ ] Add fakes for completion scheduling and presentation and retain WPF tests
  for popup positioning and lifetime.

### Phase 5C exit criteria

- [ ] Completion and presentation coordinators no longer require unrelated
  editor capabilities.
- [ ] Provider tests run without AvalonEdit/WPF where their contracts allow it.
- [ ] Popup and selection behavior remains covered by WPF tests.

## Phase 5D - Make Composition and Lifetime Explicit

Primary reference:
[TextEditorServiceComposition.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorServiceComposition.cs).

Dependency: complete the port migrations that justify the composition changes.
Do not introduce a general-purpose service locator or container for this
control.

- [ ] Make the composition object a persistent owner of the services it creates
  with one explicit disposal path.
- [ ] Ensure services are disposed in an order that prevents callbacks into a
  disposed document or view.
- [ ] Stop passing the concrete editor to any remaining service that only needs
  a document, callback, or narrow UI capability.
- [ ] Ensure any live and headless edit targets still use the shared edit
  applier and differ only in explicit side-effect policy. If no concrete
  headless consumer justified a target, record that deferred decision instead
  of adding a symmetry-only adapter.
- [ ] Add ownership/collectibility tests for the composition and replaced
  coordinators.

### Phase 5D exit criteria

- [ ] Service lifetime and disposal ownership are explicit and tested.
- [ ] Stateful services require `TextEditorBase` only for a demonstrated
  control-specific capability.
- [ ] The composition is narrow and not a hidden service registry.

## Architecture Gate B - Verify Net Benefit Before Further Extraction

This is a required review checkpoint after Phase 5D and before Phase 6A. It is
not a production implementation phase. It may run only after Architecture
Gate A recorded `Continue`. The later capability and session phases are
optional until this gate passes.

- [ ] For every port and coordinator added in Phases 5A-5D, record the
  concrete consumer, the removed direct dependency/member-use edge, the
  implementation owner, and the focused test.
- [ ] Confirm that no new type owns a duplicate content, document version,
  persisted baseline, processing scope, session generation, projection state,
  or disposal path.
- [ ] Confirm that at least one meaningful consumer can now be tested without
  the WPF control or that a concrete lifetime/ownership defect is fixed. A
  test fake alone is not evidence of benefit.
- [ ] Compare the before/after dependency map from Phase 0A. The number of
  layers may increase only when a direct UI dependency, duplicate authority,
  or untestable lifecycle path is removed.
- [ ] Record measurable before/after values for each new boundary: direct
  `TextEditorBase`/AvalonEdit member-use edges from the consumer, project
  references to WPF-bound assemblies, dispatcher/STA-required tests, duplicate
  disposal owners, and independently mutable content/version state. A port
  receives credit only when at least one of these values decreases or a named
  lifetime defect is demonstrably prevented.
- [ ] Record the maintenance cost introduced by each boundary: new production
  types, adapters, result statuses, and composition registrations. The gate
  fails when a boundary adds only an interface/fake or increases state owners
  without a corresponding dependency reduction or behavior fix.
- [ ] Run the focused Phase 5 tests and the existing editor lifecycle suite.

### Architecture Gate B exit criteria

- [ ] A human reviewer records a pass with the dependency reductions and
  residual complexity; an executing agent may not self-certify the gate.
- [ ] The gate record includes the concrete consumer, removed dependency edge,
  implementation/disposal owner, focused test, before/after measurements, and
  residual complexity for every Phase 5 port or coordinator.
- [ ] If the gate does not pass, stop the modernization at the completed
  slice, leave the smallest useful extraction in place, and mark Phases 6-7
  deferred. Do not add capability or session layers to justify the earlier
  abstractions.
- [ ] Phase 6A is not started until the gate record is committed to this plan.

## Phase 6A - Define Capability Contracts Through ClassicScript

Primary references:
[TextEditorBase.Navigation.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Navigation.cs),
[TextEditorBase.Completion.cs](TombLib/TombLib.Scripting.UI/Bases/TextEditorBase.Completion.cs),
and the ClassicScript editor initialization sites and focused tests.

Dependency: Architecture Gate B and the relevant Phase 5 ports. Scope: define
only the capabilities required by ClassicScript and migrate that first
production consumer in the same slice. If no protected-initialization or
concrete-editor dependency is removed, defer this slice without adding a
capability contract.

- [ ] Define independent optional capabilities for completion, hover,
  definition navigation, diagnostics, signature help, and language editing
  rules.
- [ ] Keep providers focused on supplying language/domain data.
- [ ] Keep coordinators focused on scheduling, cancellation, presentation
  decisions, and stale-result handling.
- [ ] Keep WPF presentation in presenters or ports rather than providers.
- [ ] Define whether capability composition is immutable after construction or
  has a safe replacement/disposal protocol.
- [ ] Add capability-parity tests so unsupported features remain valid absent
  capabilities rather than requiring universal no-op services.
- [ ] Compose only the ClassicScript capabilities it actually supports, move
  provider data access out of the editor control where the new contract
  permits, and keep presentation in existing UI ports.
- [ ] Preserve ClassicScript-specific edit rules as explicit capability input
  and add lifecycle/parity tests for supported and unsupported features.

### Phase 6A exit criteria

- [ ] Each capability has one owner, one composition path, and one test seam.
- [ ] The contracts do not expose `TextEditorBase` unless the capability is
  intrinsically view-specific.
- [ ] ClassicScript no longer depends on protected initialization for migrated
  capabilities.
- [ ] ClassicScript providers are testable without WPF where applicable.
- [ ] Every new capability contract has the migrated ClassicScript consumer in
  this diff and records the concrete dependency edge it removed; contracts
  used only by tests are deleted or the slice is not complete.

## Phase 6C - Migrate GameFlowScript Capabilities

Primary references: GameFlowScript editor initialization, language service
composition, and focused GameFlowScript tests.

Dependency: Phase 6A. Migrate only GameFlowScript in this prompt.

- [ ] Compose the GameFlowScript capabilities it supports.
- [ ] Preserve its document and formatting behavior through explicit provider
  and coordinator contracts.
- [ ] Add parity, unsupported-feature, and disposal tests.

### Phase 6C exit criteria

- [ ] GameFlowScript uses the capability composition path for migrated features.
- [ ] No unrelated language editor was changed.

## Phase 6D - Migrate TRX Capabilities

Primary references: TRX editor initialization, language service composition,
and focused TRX tests.

Dependency: Phase 6C. Migrate only TRX in this prompt.

- [ ] Compose the TRX capabilities it supports.
- [ ] Keep schema/domain data in providers and WPF behavior in UI ports.
- [ ] Add parity, unsupported-feature, and disposal tests.

### Phase 6D exit criteria

- [ ] TRX uses the capability composition path for migrated features.
- [ ] Provider tests do not require AvalonEdit where the contract is document-only.

## Phase 6E - Migrate Lua Capabilities

Primary references:
[LuaEditor.cs](TombLib/TombLib.Scripting.Lua/LuaEditor.cs),
Lua lifecycle/intellisense coordinators, and focused Lua tests.

Dependency: complete Phases 6A, 6C, and 6D and Phase 2C. Lua is intentionally isolated
because it has language-server, semantic-token, document-version, and request
generation behavior beyond the other editors.

- [ ] Reconcile Lua language-server document state with the logical-document
  generation contract before moving ownership.
- [ ] Compose Lua completion, hover, diagnostics, navigation, signature-help,
  semantic-token, and editing capabilities one at a time.
- [ ] Keep provider lifetime owned by the existing DI scope and editor/session
  lifecycle; do not duplicate provider disposal.
- [ ] Add stale-result, restart, document-replay, and disposal tests for every
  migrated capability.

### Phase 6E exit criteria

- [ ] Lua uses explicit capability composition without losing its language
  server lifecycle guarantees.
- [ ] Lua generation/version tests remain deterministic.

## Phase 6F - Remove Protected Initialization Seams

Primary references: all derived editor initialization sites and the migrated
capability composition roots.

Dependency: Phases 6A and 6C-6E. Do not remove a seam that still has a caller.

- [ ] Remove protected initialization methods only after all derived editors
  use the capability composition path.
- [ ] Remove obsolete no-op service construction and compatibility forwarding.
- [ ] Make initialization ownership explicit and immutable, or enforce the
  tested replacement/disposal protocol.
- [ ] Run capability-parity and full derived-editor tests.

### Phase 6F exit criteria

- [ ] Derived editors compose only the capabilities they support.
- [ ] `TextEditorBase` contains no language-specific policy or initialization
  ownership.
- [ ] All removed seams have migrated callers and focused tests.

## Phase 7A - Consolidate the Session Core

Primary references: the Phase 1 logical document store, Phase 2 generation
contract, and Phase 5 coordinator ports.

Dependency: complete the document, lifecycle, port, and capability migrations
that establish the inputs. Do not modernize worker implementations in this
phase.

- [ ] Extract a session/orchestration layer for processing mode, generation,
  cancellation ownership, and provider coordination. The session may
  reference document identity and dirty state, but the logical document/store
  remains their only owner.
- [ ] Keep the session core independent of WPF and dispatcher types.
- [ ] Ensure one document operation cannot publish into another logical
  document or view instance.
- [ ] Make session disposal and view detachment align with the store's
  dirty-document retention policy without disposing a retained logical
  document merely because its last view closes.
- [ ] Add focused session tests for load, replace, rename, close, dispose, and
  stale publication.

### Phase 7A exit criteria

- [ ] Session identity and generation have one owner rather than competing
  editor/Lua/controller counters, while document content/version remains owned
  by the workspace document and retention remains owned by the store.
- [ ] Core session tests run without a WPF control.

## Phase 7B - Modernize Persistence and Diagnostics Async Boundaries

Primary references:
[ContentChangedWorker.cs](TombLib/TombLib.Scripting.UI/Documents/ContentChangedWorker.cs),
[ErrorDetectionWorker.cs](TombLib/TombLib.Scripting.UI/Diagnostics/ErrorDetectionWorker.cs),
[ContentPersistenceCoordinator.cs](TombLib/TombLib.Scripting.UI/Documents/ContentPersistenceCoordinator.cs),
and [TextDiagnosticsCoordinator.cs](TombLib/TombLib.Scripting.UI/Diagnostics/TextDiagnosticsCoordinator.cs).

Dependency: Phase 7A. Change only the selected persistence and diagnostics
workers in this prompt.

- [ ] Use `Task`, `CancellationToken`, and explicit dispatcher publication
  where they improve cancellation or ownership; do not rewrite code solely for
  terminology.
- [ ] Define exception, cancellation, coalescing, and stale-result behavior for
  each worker.
- [ ] Migrate `ContentChangedWorkerRunCompleted` and diagnostics completion
  event arguments and all subscribers to the Phase 2C outcome contract. Do not
  silently map cancellation or stale completion to a successful `EventArgs`
  notification. Phase 2C defines the vocabulary; this phase owns the event
  shape and subscriber cutover.
- [ ] Keep dispatcher-free work independent of WPF and marshal only the result
  publication that is UI-bound.
- [ ] Preserve host event contracts or migrate all subscribers in this slice.
- [ ] Move core worker tests out of STA where the new ports permit it, while
  retaining WPF tests for dispatcher and event wiring.

### Phase 7B exit criteria

- [ ] Persistence and diagnostics cancellation/staleness tests are deterministic.
- [ ] Dispatcher use is explicit and localized.
- [ ] No worker callback can publish after its session or document is disposed.
- [ ] Focused tests cover `Completed`, `Cancelled`, `Superseded`, `Stale`, and
  `Failed` outcomes, including a provider exception and a coalesced request.

## Phase 7C - Modernize Language-Service Async Boundaries

Primary references: Lua language-server lifecycle, completion/hover/signature
controllers, language diagnostics, and their focused tests.

Dependency: Phase 7B and Phase 6E. Keep UI event handlers as the only
unavoidable `async void` boundaries.

- [ ] Apply the session generation and cancellation policy to language-service
  requests and provider callbacks.
- [ ] Reconcile provider restart, document replay, cancellation, and disposal
  behavior with the session owner.
- [ ] Keep `ConfigureAwait(false)` in headless/core paths and explicit UI
  dispatcher application at the presentation boundary.
- [ ] Retain WPF tests for selection, popup, renderer, dispatcher, and event
  wiring behavior.
- [ ] Add deterministic tests for provider failure, cancellation, restart,
  stale responses, and cross-document publication.
- [ ] Verify that provider restart and owner disposal await or observe the
  completion of cancellation before releasing the provider/session, and that
  no `async void` boundary is used below the UI event layer.

### Phase 7C exit criteria

- [ ] Most language-service coordination is testable without constructing the
  complete WPF control.
- [ ] Dispatcher use and `async void` boundaries are documented.
- [ ] Cancellation and stale-result behavior is covered by deterministic tests.

## Phase 8A - Slim the Control Adapter and Remove Obsolete APIs

Primary references: all `TextEditorBase` partials, `IEditorControl`, derived
editor composition roots, and the completed Phase 1-7 migration tests.

Dependency: all earlier phase exit criteria. This phase is the final behavior
and API removal slice; do not perform documentation-only cleanup here.

- [ ] Reduce `TextEditorBase` to a WPF/AvalonEdit control adapter and
  view-facing API.
- [ ] Remove obsolete protected initialization methods after all derived
  editors use capability composition.
- [ ] Remove `IsSilentSession` and all boolean `silentSession` parameters;
  Phase 2B completed that migration before this slice.
- [ ] Remove redundant mutable configuration properties and broad editor
  contracts if Phase 3C completed the migration.
- [ ] Remove forwarding methods that no longer have a view-specific
  responsibility.
- [ ] Review partial classes and organize them around actual ownership rather
  than file length.

### Phase 8A exit criteria

- [ ] The base control owns WPF adaptation and view state, not the majority of
  document or feature policy.
- [ ] The final public vocabulary distinguishes processing mode from document
  lifetime.
- [ ] Every removed API has migrated callers, a tested replacement, and no
  compatibility-only residue.

## Phase 8B - Documentation, Audit, and Full Verification

Primary references: this plan, [TextEditorBase Architecture Review](TextEditorBase_Architecture_Review.md),
the repository contributor guidance, and all focused tests from completed
phases.

Dependency: Phase 8A. Do not introduce new architecture in this phase.

- [ ] Update architecture documentation and contributor guidance with the
  final ownership, projection, lifetime, and async rules.
- [ ] Audit all public/protected APIs, obsolete identifiers, event subscribers,
  and project references for removed contracts.
- [ ] Verify that no hidden `LastModified` authority, ad-hoc headless load, or
  transient-view cleanup inference remains.
- [ ] Re-run Architecture Gate A and verify that no document, session,
  projection, controller, or provider owns a duplicate content/version/dirty
  authority.
- [ ] Re-run Architecture Gate B and verify that every later port, capability,
  and session extraction has a recorded consumer and measurable dependency
  reduction.
- [ ] Verify that `ScriptingMessageService`,
  `TombEngineLevelScriptService`, `LuaTrackedDocumentStateService`,
  `LuaReferenceSearchService`, host registrations, and test doubles all use
  the intended store/view boundary.
- [ ] Run the full relevant solution builds and test suites.
- [ ] Record pre-existing warnings/failures separately from regressions.
- [ ] Update this plan with completed checkboxes, behavior changes, deliberate
  removals, and any residual risks.

### Phase 8B verification commands

Run the following from the repository root, using the repository's default
x64 platform unless a phase records a required platform override:

```powershell
dotnet build "Tomb Editor.sln" -p:Platform=x64
dotnet test Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj
dotnet test Tests/TombLib.Tests/TombLib.Tests.csproj
dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj
```

The final report must list each command separately, classify failures as
pre-existing or introduced, and include the Architecture Gate A and Gate B
records. A solution build alone does not satisfy the focused lifecycle,
projection, authority, or net-benefit exit criteria.

### Phase 8B exit criteria

- [ ] Full relevant builds pass, or every failure is classified and documented.
- [ ] Focused lifecycle, document, projection, capability, and UI tests pass.
- [ ] Architecture Gate A has a recorded `Continue` decision and no unresolved
  duplicate authority remains, or the remaining phases are explicitly
  documented as deferred after a `Stop and defer` decision.
- [ ] Architecture Gate B has a recorded pass, or Phases 6-7 are explicitly
  documented as deferred because the later abstractions were not a net
  positive.
- [ ] The architecture review and this plan agree on the final ownership
  boundaries.

## Recommended Execution Order

The headings are grouped by concern, but the safest dependency order is:

```text
0A -> 0C -> 2A -> 0B
  -> 1A-Store -> 1A-Filesystem -> 1A-Bridge
  -> Architecture Checkpoint A0 [Continue / Repair / Stop and remove foundation]
  -> 1B -> 1B2a -> 1B2b
  -> 1C -> 1D -> 1E -> 1F -> 1G -> 1H1 -> 1H2
  -> Architecture Gate A [Continue / Repair / Stop and defer]
  -> 2B -> 2C -> 2D
  -> 3A -> 3B -> 3C
  -> 4A -> 4B -> 4C
  -> 5A -> 5B -> 5C -> 5D
  -> Architecture Gate B
  -> 6A -> 6C -> 6D -> 6E -> 6F
  -> 7A -> 7B -> 7C
  -> 8A -> 8B
```

Phase 2A is intentionally pulled forward immediately after characterization.
It fixes the verified suppressed-load ordering defect in AvalonEdit and the
active WPF string-table control before a future architecture can depend on
lifecycle behavior. Phase 1A-Store then implements the in-memory authority;
Phase 1A-Filesystem adds real persistence; Phase 1A-Bridge adds the TombIDE bridge
and explicit shell teardown ordering. Architecture Checkpoint A0 then decides
whether that still-unconsumed foundation is bounded enough to justify the
first production cutover; stopping there removes it before dual authority can
enter production.

The unregistered WinForms string editor is handled by the later cleanup audit.
Phase 0C creates the non-WPF test project and baseline before Phase 0B records
the authority contract; no contract test is assigned to a project that does
not yet exist. Architecture Gate A then verifies both document authority and
measured net benefit before processing-state and feature extraction continue.
Phases 3A-4C are optional after Gate A and are bound by the same proof
obligation as Phases 5-7 (see Abstraction proof obligation); deferring them at
Gate A is not a failure of the migration.

### Minimum Viable Outcome

The minimum viable outcome is the complete document-authority migration, not a
partially connected store. It includes 0A, 0C, 2A, 0B, both 1A slices, and
Phases 1A-Bridge through 1H2 before Architecture Gate A. At that point the store is
the only content/version/baseline/persistence authority, text and string-table
views are attached projections, headless callers and view leases use the new
boundary, and `LastModified` no longer selects content.

`Stop and defer` at Gate A is the recommended successful end state unless a
later slice names a concrete defect or dependency it will remove. Phases
2B-8B are then marked deferred. `Continue` authorizes only the next justified
slice; it is not approval to execute the remaining map. A reviewer must not
approve additional slices solely to complete the document; every later slice
requires its own production consumer, removed dependency/authority, and
focused test.

### Phase Validation Matrix

Every phase must run the command(s) in its row before its exit criteria are
marked complete. The phase sections name the narrower test classes to inspect;
the category filter below is the required stable filter for all tests added or
intentionally migrated by this plan.

```powershell
$coreTest = { dotnet test Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj --filter "TestCategory=TextEditorBaseModernization" }
$libTest = { dotnet test Tests/TombLib.Tests/TombLib.Tests.csproj --filter "TestCategory=TextEditorBaseModernization" }
$editorTest = { dotnet test Tests/TombEditor.Tests/TombEditor.Tests.csproj --filter "TestCategory=TextEditorBaseModernization" }
$scriptBuild = { dotnet build TombLib/TombLib.Scripting/TombLib.Scripting.csproj }
$scriptUiBuild = { dotnet build TombLib/TombLib.Scripting.UI/TombLib.Scripting.UI.csproj }
$classicBuild = { dotnet build TombLib/TombLib.Scripting.ClassicScript/TombLib.Scripting.ClassicScript.csproj }
$gameFlowBuild = { dotnet build TombLib/TombLib.Scripting.GameFlowScript/TombLib.Scripting.GameFlowScript.csproj }
$luaBuild = { dotnet build TombLib/TombLib.Scripting.Lua/TombLib.Scripting.Lua.csproj }
$trxBuild = { dotnet build TombLib/TombLib.Scripting.TRX/TombLib.Scripting.TRX.csproj }
$studioBuild = { dotnet build TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj }
```

| Phase or gate | Required validation |
| --- | --- |
| 0A | `git diff --check -- TextEditorBase_Architecture_Modernization_Plan.md`; review the inventory and dependency map against the named references. |
| 0C | `dotnet build Tests/TombLib.Scripting.Tests/TombLib.Scripting.Tests.csproj`; `& $coreTest`; `& $editorTest` for `ScriptingPhase0LifecycleTests`. |
| 2A | `& $editorTest`; inspect load-order, backup-suppression, and delayed-notification tests; build `TombLib/TombLib.Scripting.UI/TombLib.Scripting.UI.csproj` and `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`. |
| 0B | Re-run `& $coreTest` and `& $editorTest`; review every Binding Decision Record matrix row for an owning implementation phase and focused test. |
| 1A-Store | `& $coreTest`; build `TombLib/TombLib.Scripting/TombLib.Scripting.csproj`; inspect in-memory state-machine tests. |
| 1A-Filesystem | `& $coreTest`; build `TombLib/TombLib.Scripting/TombLib.Scripting.csproj`; inspect encoding, stamp, conflict, cancellation, and replacement tests. |
| 1A-Bridge | `& $editorTest`; build `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`. |
| Checkpoint A0 | `& $coreTest`; `& $editorTest`; record foundation surface counts, consumer/removal mapping, async-scope ownership, and the human decision. |
| 1B | `& $coreTest`; build `TombLib/TombLib.Scripting/TombLib.Scripting.csproj`. |
| 1B2a | `& $editorTest`; run and record the 1 MiB release-build publication benchmark; build `TombLib/TombLib.Scripting.UI/TombLib.Scripting.UI.csproj` and `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`. |
| 1B2b | `& $coreTest`; `& $editorTest`; inspect all-target preflight and partial-runtime-application tests; build `TombLib/TombLib.Scripting.UI/TombLib.Scripting.UI.csproj` and `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`. |
| 1C | `& $coreTest`; `& $editorTest`; `& $scriptBuild`; `& $classicBuild`; `& $scriptUiBuild`; `& $studioBuild`. |
| 1D | `& $libTest`; `& $editorTest`; `& $scriptBuild`; `& $scriptUiBuild`; `& $studioBuild`. |
| 1E | `& $editorTest`; inspect the exact-view lease tests and build `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`. |
| 1F | `& $editorTest`; inspect the ClassicScript/GameFlowScript provider tests and build `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`. |
| 1G | `& $editorTest`; inspect the Lua/TRX provider tests and build `TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj`. |
| 1H1 | `& $libTest`; `& $editorTest`; inspect save/edit interleaving, projection preflight, close, reload, and restore tests; review a no-`LastModified` authority trace; `& $scriptUiBuild`; `& $studioBuild`. |
| 1H2 | `& $coreTest`; `& $editorTest`; inspect path-reservation, identity-batch, explorer, and destructive-lifecycle tests; `& $scriptBuild`; `& $studioBuild`. |
| Gate A | `& $coreTest`; `& $libTest`; `& $editorTest`; record the ownership and duplicate-authority review. |
| 2B | `& $libTest`; `& $editorTest`; inspect processing-mode migration tests; `& $scriptUiBuild`; `& $studioBuild`. |
| 2C | `& $coreTest`; `& $libTest`; `& $editorTest`; inspect generation, cancellation, and stale-publication tests; `& $scriptUiBuild`; `& $luaBuild`; `& $studioBuild`. |
| 2D | `& $libTest`; `& $editorTest`; inspect disposal and repeated-initialization tests; `& $scriptUiBuild`; `& $studioBuild`. |
| 3A | `& $libTest`; run the selected consumer's focused test and build its production project; record the duplicate settings path removed. |
| 3B | `& $libTest`; `& $editorTest`; inspect settings-application and runtime-adapter tests; `& $scriptUiBuild`; `& $studioBuild`. |
| 3C | `& $libTest`; `& $editorTest`; inspect compatibility-removal and persistence tests; `& $classicBuild`; `& $gameFlowBuild`; `& $luaBuild`; `& $trxBuild`; `& $scriptUiBuild`; `& $studioBuild`. |
| 4A | `& $coreTest`; `& $editorTest`; inspect non-WPF transformation and view-boundary tests. |
| 4B | `& $coreTest`; `& $libTest`; `& $editorTest`; inspect live/headless parity tests when a headless target exists; `& $scriptBuild`; `& $scriptUiBuild`; `& $studioBuild`. |
| 4C | `& $editorTest`; inspect selection/view-port tests and build `TombLib/TombLib.Scripting.UI/TombLib.Scripting.UI.csproj`. |
| 5A | `& $libTest`; `& $editorTest`; inspect each new port's consumer test; `& $scriptUiBuild`; `& $studioBuild`. |
| 5B | `& $libTest`; `& $editorTest`; inspect persistence/diagnostics fakes and dispatcher tests; `& $scriptUiBuild`; `& $studioBuild`. |
| 5C | `& $editorTest`; inspect popup/selection WPF tests; `& $scriptUiBuild`; `& $studioBuild`. |
| 5D | `& $libTest`; `& $editorTest`; inspect composition disposal/collectibility tests; `& $scriptUiBuild`; `& $studioBuild`. |
| Gate B | `& $coreTest`; `& $libTest`; `& $editorTest`; record each new port's consumer, removed dependency, owner, focused test, before/after measurements, maintenance cost, and residual complexity. |
| 6A | `& $libTest`; `& $editorTest`; inspect capability-contract, ClassicScript parity/lifecycle, and unsupported-feature tests; `& $classicBuild`; `& $studioBuild`. |
| 6C | `& $editorTest`; inspect GameFlowScript capability-parity and disposal tests; `& $gameFlowBuild`; `& $studioBuild`. |
| 6D | `& $editorTest`; inspect TRX capability-parity tests; `& $trxBuild`; `& $studioBuild`. |
| 6E | `& $libTest`; `& $editorTest`; inspect Lua generation/restart/replay/disposal tests; `& $luaBuild`; `& $studioBuild`. |
| 6F | `& $libTest`; `& $editorTest`; inspect protected-initialization removal tests; `& $classicBuild`; `& $gameFlowBuild`; `& $luaBuild`; `& $trxBuild`; `& $scriptUiBuild`; `& $studioBuild`. |
| 7A | `& $coreTest`; `& $libTest`; `& $editorTest`; inspect session identity/generation/disposal tests; `& $scriptBuild`; `& $scriptUiBuild`; `& $studioBuild`. |
| 7B | `& $coreTest`; `& $libTest`; `& $editorTest`; inspect deterministic persistence/diagnostics outcome tests; `& $scriptUiBuild`; `& $studioBuild`. |
| 7C | `& $coreTest`; `& $libTest`; `& $editorTest`; inspect language-service cancellation/restart/stale-publication tests; `& $luaBuild`; `& $studioBuild`. |
| 8A | `& $coreTest`; `& $libTest`; `& $editorTest`; `dotnet build "Tomb Editor.sln" -p:Platform=x64`. |
| 8B | Run every command in the Phase 8B verification block, then record pre-existing failures separately from regressions. |

The PowerShell assignments above are command aliases for the handover prompt;
an agent may paste the expanded command when the terminal does not preserve
variables. A phase may add a narrower class-name filter for diagnosis, but it
must still run the category-filtered command before completion. If a named test
does not yet carry the category, adding that category is part of the phase,
not a reason to change the command.

## First Implementation Slices After Preflight

After Phases 0A, 0C, 2A, and 0B complete in the recommended order, use three
sequential prompts. The first implements only in-memory document authority,
the second adds real persistence, and the third composes the bridge with an
unloaded fake projection. None changes a production controller open path,
workspace-edit caller, processing vocabulary, or tab/lease behavior.

### Phase 1A-Store handover prompt

- [ ] Read the Phase 0B matrix and Phase 1A-Store primary references.
- [ ] Add the narrow workspace types and `IWorkspaceFileSystem` seam in
  `TombLib.Scripting.Workspace`; keep them internal until a cross-project
  consumer requires public visibility.
- [ ] Implement normalized identity, immutable snapshots and document keys,
  monotonic versions, baseline-derived dirty state, store-only replacement/
  discard, workspace-lifetime retention, state locking, and disposal.
- [ ] Use a controllable filesystem fake only as an I/O boundary. Do not add a
  fake store, real filesystem adapter, TombIDE reference, projection, view
  lease, or caller migration.
- [ ] Add `WorkspaceDocumentStoreTests` for repeated identity, dirty authority,
  undo-to-clean, stale key/version rejection, retention, and disposal.
- [ ] Run the Phase 1A-Store focused validation and update only its Progress
  Log row and handover record.

### Phase 1A-Filesystem handover prompt

Begin only after Phase 1A-Store tests and build pass.

- [ ] Read the Phase 1A-Store handover record and Phase 1A-Filesystem contract.
- [ ] Implement the real asynchronous filesystem adapter, profile-aware
  encoding, BOM/newline metadata, `FileStamp.Missing`, expected stamp checks,
  per-document operation gate, temporary write/flush/replacement, and
  post-replacement stamp capture.
- [ ] Preserve both content and format baselines only after successful commit;
  return observed conflict or `ReplacementStateUnknown` without guessing.
- [ ] Add deterministic `WorkspaceDocumentFileSystemTests` with the controllable
  filesystem fake. Do not use sleeps, external processes, WPF, or TombIDE.
- [ ] Run the Phase 1A-Filesystem focused validation and update only its
  Progress Log row and handover record.

### Phase 1A-Bridge handover prompt

The Phase 1A-Bridge prompt begins only after both Phase 1A slices pass.

- [ ] Read both Phase 1A handover records and the Phase 1A-Bridge primary references.
- [ ] Add the TombIDE bridge and implement `OpenOrAttachAsync` with an unloaded
  fake projection. It loads or resolves the canonical snapshot, attaches under
  suppression, registers only after success, and reports attach failure
  without adding a view.
- [ ] Reject a loaded, pending, conflicted, or already registered projection;
  do not add a loaded-view import path.
- [ ] Leave `TryGetTextDocument`, production controller opens, and legacy
  domain views unchanged, with no new caller. Phase 1B2a cuts over text-only
  opens and Phase 1C cuts over domain-capable opens.
- [ ] Register one store per ScriptingStudio workspace scope and prove repeated
  bridge resolutions receive the same scoped instance.
- [ ] Make the shell/workbench shutdown path stop new bridge work, detach its
  current fake registrations, await the bridge stop barrier, and then
  asynchronously dispose the scope as the sole DI disposal owner. Prove the
  order, absence of post-shutdown publication, and repeated shutdown
  idempotence with observable test doubles. Later migration slices add their
  concrete view and lease owners when needed.
- [ ] Do not migrate content callers, add a generic headless edit target, add
  string-table publication, rename processing state, or change `LastModified`.
- [ ] Run the Phase 1A-Bridge composition test and build. Record changed files,
  disposal ownership, tests, and deferred caller migration before handing off
  to Architecture Checkpoint A0. Phase 1B is blocked until its human
  `Continue` decision.

Do not begin by moving `SelectLine()` into an extension class. That is a useful
candidate for a later API cleanup only if the resulting operation is a pure
document query. Keep selection, scrolling, caret movement, and popup behavior
on the editor/view side. The first modernization value comes from making
document authority, lifecycle, and ownership correct.

## Tracking Rules

- [ ] Update the phase checklist and the Progress Log table immediately after
  each completed slice.
- [ ] Record behavior changes and deliberate API removals near the phase that
  introduced them.
- [ ] Do not mark a phase complete until its exit criteria and focused tests
  pass.
- [ ] Keep unrelated refactors out of the active phase.
- [ ] Re-read this document before starting a new extraction after context
  compaction.
