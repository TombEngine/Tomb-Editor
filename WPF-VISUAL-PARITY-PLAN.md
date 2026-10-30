# WPF Visual Parity Plan (Tomb Editor, WadTool, SoundTool)

Goal: every WPF window looks "close enough" to its WinForms counterpart (develop branch) that a user would not notice a major difference. Not pixel-identical. Colors, density, grouping and control order must feel the same.

## 1. Ground rules

- Reference = WinForms build of `develop` (separate worktree: `git worktree add ../Tomb-Editor-develop develop`).
- Fix order of preference (cleanest first):
  1. Fix the window's own layout (Grid/StackPanel structure, margins via shared styles/resources).
  2. Fix a DarkUI.WPF theme resource (brush, spacing constant, AvalonDock style). Safe because it is data, not a primitive.
  3. Fix a DarkUI.WPF primitive (Button, TextBox, ...). Only if the same gap shows up in 3+ windows. Needs a ripple check: re-screenshot the 5 most complex already-approved windows.
  4. Never: inline pixel sizes, inline colors, hardcoded Width/Height on a control just to match WinForms.
- Do NOT bring back worse WinForms design. Explicit exceptions:
  - Flyby timeline stays a dockable panel (WinForms had it as a fixed, non-dockable control).
  - `ToolPaletteFloating` is not reproduced; the docked toolbox is the design.
  - Any case where docking, resizing, DPI scaling or localization behave better in WPF stays WPF.
  - Report each such case in the window's notes as "intentional divergence".
- Files: Windows line endings, ASCII only, trailing newline (AGENTS.md).

## 2. Method (per window)

1. Capture WinForms screenshot (default size, then resized larger, then minimum size if resizable).
2. Capture WPF screenshot, same state, same data (same test level / wad2 / sound XML).
3. Sonnet 5.5 agent compares the pair against the checklist below and returns a ranked diff list.
4. Triage: S1 = noticeable at first glance (wrong colors, missing/misplaced controls, clipped text, window far too large/small). S2 = visible on comparison (spacing, alignment, label wrap, group box styling). S3 = nitpicks (1-2 px). Fix S1 and S2 only; log S3.
5. Fix using the preference order in section 1; rebuild; re-capture; confirm.
6. Tick the window in the tracking table.

Checklist per window:
- Window size, resizability, startup position, min size, title.
- Background/foreground/border colors, group box and separator look, disabled state.
- Control order, grouping, tab order, label alignment, label-to-field spacing.
- Row height and overall density (WPF usually looks looser).
- Font size and weight, text trimming, wrapping.
- Lists/grids/trees: row height, header style, selection color, alternate rows.
- Buttons: size, position (OK/Cancel corner), default/cancel behavior.
- Special content: 3D viewports, timelines, color pickers, curves, texture panels.
- DPI: check at 100% and 150%.

## 3. Screenshot tooling (Phase 0, once)

- Small PowerShell/C# helper that launches an exe, waits for a window by title, captures that window (PrintWindow, DPI aware) to `artifacts/parity/<tool>/<window>/{winforms,wpf}-<state>.png`.
- Per-window launch recipe (menu path, command line flag, or test hook) recorded in the tracking table, so the walk is repeatable.
- Launch once, screenshot, kill (user's windows must not be disturbed; run on a dedicated desktop/virtual display if possible).
- Add `--winforms` fallback usage notes for WadTool (already present).

## 4. Execution waves (shell first, because theme fixes ripple everywhere)

- Wave A: three main windows and docking theme (AvalonDock styles, menus, toolbars, status bars). Done by one agent per tool, sequentially reviewed by me, because they set colors for everything else.
- Wave B: simple dialogs (few controls) per tool, up to 4 Sonnet agents in parallel, each owning disjoint files.
- Wave C: complex dialogs and editors (one agent per window, serial where they touch the same DarkUI file).
- Wave D: ripple regression pass: re-capture all Wave A and a sample of B/C after any primitive change; final side-by-side contact sheet per tool.

Agents never edit the same DarkUI.WPF file concurrently; primitive changes are proposed to me first with the list of affected windows.

## 5. Window inventory and order

### 5.1 Tomb Editor (Wave A)
| # | WinForms | WPF | Notes |
|---|----------|-----|-------|
| 1 | FormMain | MainWindow (+ EditorToolbarView, StatusBarView, StatisticsBarView) | menu, toolbar, status bar, dock theme, tab headers |
| 2 | ToolWindows/MainView | Panel3D (+ ToolboxView, ObjectBrushToolboxView) | viewport overlays, toolbox; floating palette excluded |
| 3 | ToolWindows/RoomsView (room list) | RoomsView | |
| 4 | ToolWindows/SectorOptions | SectorOptionsView | |
| 5 | ToolWindows/RoomOptions | RoomOptionsView | |
| 6 | ToolWindows/ItemBrowser | ItemBrowserView | |
| 7 | ToolWindows/ImportedGeometryBrowser | ImportedGeometryBrowserView | |
| 8 | ToolWindows/ContentBrowser | ContentBrowserView | |
| 9 | ToolWindows/ItemProperties | ItemPropertiesView | property grid look |
| 10 | ToolWindows/Lighting | LightingView | |
| 11 | ToolWindows/ObjectList | ObjectListView | |
| 12 | ToolWindows/Palette | PaletteView | |
| 13 | ToolWindows/TexturePanel | TexturePanelView | |
| 14 | ToolWindows/TriggerList (+ Controls/TriggerManager) | TriggerListView | |
| 15 | (not dockable) timeline | FlybyTimelineView | keep dockable; compare only colors/density |

### 5.2 Tomb Editor dialogs (Waves B/C)
Wave B (simple): FormChooseRoom, FormMemo, FormResizeRoom, FormSelectRoomByTags, FormQuickItemgroup, FormSearch, FormReplaceObject, FormTransform, FormCamera, FormSink, FormSprite, FormOperationDialog, FormFootStepSounds, FormBumpMaps, FormPreviewTexture, FormPreviewWad, FormToolBarLayout, FormImportPrj.
Wave C (complex): FormOptions, FormKeyboardLayout, FormRoomProperties, FormPortal, FormMoveable, FormStatic, FormSoundSource, FormFlybyCamera, FormTrigger, FormEventSetEditor (+ NodeEditorView), FormFindTextures, FormTextureRemap, FormImportedGeometry, LevelSettings (WinForms: level settings form).
Each maps to the `*Window.xaml` of the same name under `TombEditor/Features/Dialogs/`.

### 5.3 WadTool
| Wave | WinForms | WPF |
|------|----------|-----|
| A | FormMain | MainWindow (dock layout, tree, 3D viewport, toolbars, status bar) |
| B | FormSelectSlot, FormNewWad2, FormOptions, FormLuaProperties, FormReplaceAnimCommands, FormAnimationFixer | same names under `WadTool/Features/Dialogs/` |
| C | FormStaticEditor, FormBlendCurveEditor, FormAnimCommandsEditor (+ Controls/AnimCommandEditor), FormSpriteSequenceEditor, FormStateChangesEditor, FormSkeletonEditor, FormMeshEditor, FormAnimationEditor | same names; Animation Editor last (largest, timeline + viewport + panels) |

### 5.4 SoundTool
| Wave | WinForms | WPF |
|------|----------|-----|
| A | FormMain | MainWindow (sound list, properties, toolbar, status bar) |
| B | FormOptions | Views/OptionsWindow |

## 6. Tracking table (copy per tool and fill in while working)

| Window | WF shot | WPF shot | S1 | S2 | Fix level (layout/theme/primitive) | Intentional divergence | Done |
|--------|---------|----------|----|----|-----------------------------------|------------------------|------|

## 7. Recommended start

1. Phase 0: worktree + screenshot helper + launch recipes (one tool at a time, SoundTool first as a pilot because it has only 2 windows).
2. SoundTool full pass (validates the method and the budget).
3. WadTool Wave A, then Tomb Editor Wave A (theme-level decisions).
4. Remaining waves per tool.

## 8. Open questions

- Test data to use for screenshots (a standard level `.prj2`, a `.wad2` with animations/sprites, a sound catalog XML)? I will pick repo samples unless you point me to others.
- Is it acceptable to tune AvalonDock styles (tab height, header colors) globally in DarkUI.WPF in Wave A?
- DPI targets besides 100% and 150%?

## 9. Progress log

### SoundTool - done (2026-10-30)
- Main window: layout of the details pane rebuilt to follow WinForms (ID/Global/Include row, name + icon buttons, single parameters group with units, sample buttons above the sample list, no scrolling). Menu structure aligned (Options under Tools, Build/Select labels).
- Options window: unit "%" and colon label added.
- Theme fixes (ripple to all tools): DataGrid VisualStates no longer animate shared brushes (caused #525252 window background); implicit GridSplitter style; window title trims with ellipsis and no longer runs under caption buttons.
- Intentional divergences: dark custom title bar instead of system accent color; details pane always visible but disabled when nothing is selected; Options has a single page, no "Apply" and no page list.

### WadTool main window - first look
- Background and palette now match. Remaining checks: panel headers, bottom action buttons, status bar, toolbar spacing.