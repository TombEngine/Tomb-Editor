# WPF integration work plan

## Merge develop

- [x] Inspect clean Nickelony/NEW-WPF worktree.
- [x] Fetch origin and create backup/new-wpf-before-develop-20261028.
- [x] Merge origin/develop and resolve deleted WinForms conflicts in favor of WPF replacements.
- [x] Combine WPF light undo with develop lighting invalidation gateway.
- [x] Preserve PerMonitorV2 DPI initialization.
- [x] Port all incoming behavior to WPF equivalents.
- [x] Build Debug/Release x64 and run relevant regression tests.
- [x] Commit verified merge (322a6fb0e).

## Incoming WPF functionality

- [x] Level settings: alternate TRX groups option and localization.
- [x] Camera: correct TR3X move timer availability.
- [x] Object properties: engine-aware catalog and substitute IDs for TRX.
- [x] Transform: live/deferred lighting, no-op handling and close/cancel restoration.
- [x] Palette, replace object and room properties: lighting gateway.
- [x] Lighting panel: avoid irrelevant relights for dynamic flags.
- [x] Content/item browser: avoid asset rebuild on unrelated configuration changes.
- [x] WadTool: animation root-motion corrections and real range divisor.
- [x] WadTool: engine-aware Lua property catalog.
- [ ] Verify incoming TombIDE TR3X setup/import/scripts behavior.

## Icons

- [x] Inventory current 235 SVGs and 940 PNG derivatives.
- [x] Map legacy develop PNGs to SVGs and document unmatched resources.
- [x] Produce contact sheets showing original PNG and current SVG side by side.
- [x] Define 16x16 viewport, 1px primary strokes and consistent padding.
- [x] Preserve original subjects, silhouettes, orientation and semantic colors.
- [x] Keep TombLib.Icons categories; add Actions/Transport, Actions/Transform and nested resource support.
- [x] Review the full catalog and redraw mismatches after inspecting legacy references.
- [x] Cover transport, transform, texture mirror/rotate and invisible-tool gaps.
- [x] Generate 1044 PNG bucket files reproducibly from 261 SVG sources.
- [x] Validate static resource references; retain compatibility assets used by residual WinForms components.
- [ ] Check icons in actual UI at 100, 125, 150 and 200 percent DPI.

## Final validation

- [x] Debug and Release builds on x64 and x86 (zero errors).
- [x] TombEditor regression suite: 65 passed; TombLib regression suite: 131 passed.
- [ ] Manual level open/save/reopen, editing, undo/redo and compilation smoke checks.
- [ ] WPF keyboard, ownership, modal behavior, localization and resizing checks.
- [ ] Check logs, binding errors and performance of bulk updates.
- [x] Review ASCII, CRLF and trailing newlines for authored text files.
- [x] Record remaining limitations and unperformed manual checks explicitly.

## Verification notes

- Merge includes all commits from origin/develop 8c32757740d1477fcaaa85934e327dee35dd33a2.
- Existing target frameworks remain net6.0/net6.0-windows; framework migration is outside this integration.
- Four Transform cancellation/mode-switch regressions found and covered by tests.
- Added icon ID, stream-lifetime, viewport and complete embedded-resource coverage tests.
- Real application interaction and multi-monitor DPI checks remain manual verification tasks.
- Legacy map covers all 363 reviewed original PNG paths; 359 UI assets regenerated at original sizes.
- Four larger renderer/shadow textures were identified as non-UI assets and retained.
- Canonical PNG coverage and raw SVG viewport audits both report zero errors.
- Existing build warnings include net6.0 support and dependency compatibility warnings.

## Native WPF completion for TombEditor, WadTool and SoundTool

- [x] Replace SoundTool main window, catalog/sample editor, options and startup with WPF views and view models.
- [x] Preserve catalog XML/TXT, SFX archive workflows, reference paths, recent files and sound preview APIs.
- [x] Replace shared material editor with WPF, typed property editing, map previews and atomic sidecar persistence.
- [x] Preserve missing material map references, all map types, cancellation and failed-load protection.
- [x] Replace the shared color picker with WPF HSV/RGB/hex controls, live preview and custom color slots.
- [x] Replace TombEditor floating palettes, sprite preview and missing-resource recovery with native WPF components.
- [x] Replace every WindowsFormsHost in all three applications.
- [x] Host Direct3D swap chains in native WPF HwndHost surfaces with input, DPI, OLE drag/drop and deterministic cleanup.
- [x] Preserve managed editor object identity through OLE drag/drop.
- [x] Add native rendering failure recovery guidance and logging.
- [x] Move startup, exception dialogs, shortcuts, camera timers and autosave to WPF services/dispatchers.
- [x] Add WadTool shell command view model and command availability policies.
- [x] Prevent mesh shortcuts from stealing text/numeric input and consume handled Escape/Delete actions.
- [x] Replace remaining UI icon consumers with the shared SVG catalog and remove obsolete bitmap resource catalogs.
- [x] Remove obsolete SoundTool forms, material WinForms form and unused palette/context-menu wrappers.
- [x] Add persistent SoundTool.Tests and WadTool.Tests projects to the solution.
- [x] Build the complete solution in Debug and Release on x64 and x86 (zero errors).
- [x] Run regression suites: TombLib 159, TombEditor 65, SoundTool 6, WadTool 5; all 235 passed.
- [x] Run offscreen STA layout checks for SoundTool, options, color picker and material editor.
- [x] Run offscreen TombEditor/WadTool GPU startup, Clear/Draw/Present and disposal checks without modifying saved configurations.
- [ ] Complete interactive audio/SFX, real mouse drag/drop and multi-monitor DPI acceptance checks.

The Direct3D backend remains in use through native WPF HWND surfaces. The dependency-removal phase below replaces the remaining legacy input and ownership contracts. Shared legacy controls retained for other suite consumers are outside the three editors' runtime graph.

Existing .NET 6 target frameworks and their compatibility warnings remain unchanged.

## Remove Windows Forms dependencies from the three editors

- [x] Replace core key, pointer, drag/drop, dialog and window-owner contracts with presentation-neutral types.
- [x] Preserve serialized shortcut numeric values, aliases and modifier flags.
- [x] Extract WPF views, view models, services and native rendering hosts from TombLib.Forms into TombLib.WPF.
- [x] Separate the SVG icon catalog and WPF theme palette from legacy bitmap and DarkUI dependencies.
- [x] Replace file/folder dialogs, clipboard access, cursor state and dispatcher pumping with native WPF services.
- [x] Remove UseWindowsForms and legacy project references from TombEditor, WadTool and SoundTool.
- [x] Preserve old layout XML without loading legacy docking types.
- [x] Remove the NAudio.WinForms dependency while preserving MP3 and FLAC decoding.
- [x] Replace the MvvmDialogs package whose compiled assembly references System.Windows.Forms.
- [x] Select only the SharpVectors WPF assemblies, excluding its unused Windows Forms GDI renderer.
- [x] Add a dependency audit that checks runtime manifests and reachable managed assembly references.
- [x] Rebuild all three executable manifests and pass the dependency audit.
- [x] Run editor, shared-library, input/resource and configuration compatibility tests.
- [x] Run offscreen TombEditor/WadTool GPU startup and drawing without loading Windows Forms assemblies.
- [x] Verify the complete solution still builds for legacy suite consumers in Debug/Release on x64/x86.
- [x] Review formatting and record remaining interactive checks.

SharpDX.dll and SharpDX.Toolkit.Graphics.dll remain unchanged by explicit user instruction. Their internal System.Windows.Forms references are the only permitted vendor exceptions; the renderer will be migrated separately. Legacy Windows Forms projects required by other suite applications remain outside the three editors' dependency graph.

The final audit passed for all twelve executable outputs across Debug/Release and x64/x86. Each configuration inspected 49 local runtime assemblies for TombEditor and 47 each for WadTool and SoundTool, with zero forbidden dependencies outside those two vendor exceptions. All 263 regression tests passed: TombLib 166, TombEditor 86, WadTool 5, SoundTool 6. TombEditor includes 19 input/resource compatibility cases and two legacy layout XML cases; TombLib includes seven WPF dialog lifecycle and ownership cases.

The final offscreen GPU checks created the native viewport HWNDs and completed drawing/presentation for TombEditor and WadTool with the original SharpDX binaries. Neither process loaded a System.Windows.Forms assembly, and configuration file hashes remained unchanged. Interactive audio/SFX, mouse drag/drop and multi-monitor DPI acceptance checks remain manual tasks.
