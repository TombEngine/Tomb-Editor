# WPF Visual and Functional Parity Plan (Tomb Editor, WadTool, SoundTool)

Goal: every WPF window looks "close enough" to its WinForms counterpart (develop branch) that a user would not notice a major difference, and does the same things. Not pixel-identical. Colors, density, grouping, control order and behavior must feel the same.

## 1. Ground rules

- Reference = WinForms build of `develop` (separate worktree: `git worktree add ../Tomb-Editor-develop develop`, built to its own `Build (x64)` folder).
- Fix order of preference (cleanest first):
  1. Fix the window's own layout (Grid/StackPanel structure, margins via shared styles/resources).
  2. Fix a DarkUI.WPF theme resource (brush, spacing constant, AvalonDock style, implicit style for an unstyled control).
  3. Fix a DarkUI.WPF primitive (Button, TextBox, ...). Only if the same gap shows up in 3+ windows. Needs a ripple check: re-screenshot the 5 most complex already-approved windows.
  4. Never: inline pixel sizes, inline colors, hardcoded Width/Height on a control just to match WinForms.
- Do NOT bring back worse WinForms design. Explicit exceptions:
  - Flyby timeline stays a dockable panel (WinForms had it as a fixed, non-dockable control).
  - `ToolPaletteFloating` is not reproduced; the docked toolbox is the design.
  - Any case where docking, resizing, DPI scaling or localization behave better in WPF stays WPF.
  - Report each such case in the window's notes as "intentional divergence".
- A layout change must never silently drop a feature. If a WinForms control has no WPF counterpart, either port it or list it as a deliberate removal in the window's notes.
- Files: Windows line endings, ASCII only, trailing newline (AGENTS.md).

## 2. Method (per window)

### 2.1 Visual parity
1. Capture WinForms screenshot (default size, then resized larger, then minimum size if resizable).
2. Capture WPF screenshot, same state, same data (same test level / wad2 / sound XML).
3. Sonnet 5.5 agent compares the pair against the checklist below and returns a ranked diff list.
4. Triage: S1 = noticeable at first glance (wrong colors, missing/misplaced controls, clipped text, window far too large/small). S2 = visible on comparison (spacing, alignment, label wrap, group box styling). S3 = nitpicks (1-2 px). Fix S1 and S2 only; log S3.
5. Fix using the preference order in section 1; rebuild; re-capture; confirm.

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

### 2.2 Functional parity (mandatory, runs after every visual change to a window)
Visual work often rewires bindings and moves controls, so behavior must be re-verified, not assumed.
1. Inventory: read the WinForms form code (`Form*.cs` and its event handlers) and list every user-visible action and rule: buttons, menu items, shortcuts, context menus, double-click and drag/drop behavior, validation and value limits, enable/disable conditions, default and cancel buttons, tab order, state persisted in the configuration, messages and confirmation prompts.
2. Map: for each item find the WPF equivalent (XAML element plus ViewModel command/property). Mark each as same / different / missing.
3. Verify by driving the real app, not just by reading code: open the window with real data, perform each action in both builds (mouse, keyboard or UI Automation scripts via the screenshot helper), and compare the results (state change, saved file content, dialog shown, enabled state). Capture before/after screenshots for non-obvious cases.
4. Resolve every "different" or "missing" item: fix it, or record it as an intentional divergence with a reason (and get user approval for removals).
5. Record the result in the tracking table (section 6) and add a regression test when the logic lives in a ViewModel that can be tested without UI.

Typical things to check: Apply vs OK vs Cancel semantics, values committed on lost focus vs on every keystroke, min/max clamps, commands that were disabled in WinForms when there is no selection, keyboard shortcuts, context menu entries, hints or status texts that change with state, multi-selection behavior, and anything the old form saved to the configuration file.

## 3. Screenshot and automation tooling (Phase 0, once)

- Small PowerShell helper that launches an exe, waits for its main window, optionally sends keystrokes (menu accelerators, Enter, etc.) to reach a dialog, captures the foreground window (PrintWindow or screen copy, DPI aware) to `artifacts/parity/<tool>/<window>/{winforms,wpf}-<state>.png`, then kills the process.
- Per-window launch recipe (menu path, command line arguments, or key sequence) recorded in the tracking table, so the walk is repeatable. Test data is passed on the command line where the tool supports it.
- Launch once, screenshot, kill (the user's own windows must not be disturbed).
- `artifacts/` is not committed.

## 4. Execution waves (shell first, because theme fixes ripple everywhere)

- Wave A: main windows and docking theme (AvalonDock styles, menus, toolbars, status bars). One agent per tool, results reviewed by hand, because they set colors for everything else.
- Wave B: simple dialogs (few controls) per tool, up to 4 Sonnet agents in parallel, each owning disjoint files.
- Wave C: complex dialogs and editors (one agent per window, serial where they touch the same DarkUI file).
- Wave D: ripple regression pass: re-capture all Wave A windows and a sample of B/C after any theme or primitive change; final side-by-side contact sheet per tool.

Agents never edit the same DarkUI.WPF file concurrently; primitive changes are proposed first with the list of affected windows.

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
Wave C (complex): FormOptions, FormKeyboardLayout, FormRoomProperties, FormPortal, FormMoveable, FormStatic, FormSoundSource, FormFlybyCamera, FormTrigger, FormEventSetEditor (+ NodeEditorView), FormFindTextures, FormTextureRemap, FormImportedGeometry, level settings form (LevelSettingsWindow).
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

| Window | WF shot | WPF shot | S1 | S2 | Fix level (layout/theme/primitive) | Intentional divergence | Functional parity (inventory / verified) | Done |
|--------|---------|----------|----|----|-----------------------------------|------------------------|------------------------------------------|------|

## 7. Recommended start

1. Phase 0: worktree + screenshot/automation helper + launch recipes (SoundTool first as a pilot because it has only 2 windows).
2. SoundTool full pass, visual and functional (validates the method and the budget).
3. WadTool Wave A, then Tomb Editor Wave A (theme-level decisions).
4. Remaining waves per tool.

## 8. Open questions

- Test data to use for screenshots (a standard level `.prj2`, a `.wad2` with animations/sprites, a sound catalog XML)? Repo samples are used unless other data is provided. So far: `Catalogs/Sounds.tr2.xml` for SoundTool.
- Is it acceptable to tune AvalonDock styles (tab height, header colors) globally in DarkUI.WPF in Wave A?
- DPI targets besides 100% and 150%?

## 9. Progress log

### SoundTool - visual pass done, functional pass mostly done (2026-10-30)
- Main window: layout of the details pane rebuilt to follow WinForms (ID/Global/Include row, name plus icon buttons, single parameters group with units, sample buttons above the sample list, no scrolling). Menu structure aligned (Options under Tools, Build/Select labels).
- Options window: unit "%" and colon label added.
- Theme fixes (ripple to all tools): DataGrid visual states no longer animate shared brushes (this caused the #525252 window background); implicit GridSplitter style; window title trims with ellipsis and no longer runs under the caption buttons.
- Functional inventory compared WinForms `FormMain` and `SoundInfoEditor` against `MainViewModel` / `SoundInfoViewModel`.
  - Same (read and covered by unit tests): new/open/save flow and unsaved-changes prompts, add sound (default name NEW_SOUND, duplicate ID rejected), delete with confirmation, search with wrap, reset (keeps name and samples), paste (keeps ID), index/unindex all, value limits (volume/chance 0-100, pitch -100..100, range 0-255), sample add/browse/delete, MAIN.SFX build/unpack, recent files, reference project load/unload.
  - Fixed: playback mode showed raw enum names and no hint; now shows "None / One shot wait / One shot rewound / Loop" with the legacy hint text. Ctrl+C / Ctrl+V on the sound list now copy/paste the sound info (verified live: pasted sound keeps its ID and takes name, parameters and samples).
  - Verified live: details pane population on selection, hint under Mode, copy/paste through the list.
- Intentional divergences (to be confirmed): dark custom title bar instead of the system accent color; details pane always visible but disabled when nothing is selected; Stop preview button added; Index/Unindex all marks the catalog as changed (WinForms did not); compile/unpack iterate sounds ordered by ID (WinForms used grid order); Ctrl+C/V are bound to the sound list only so text boxes keep normal clipboard behavior.
- Behavior aligned afterwards: Options now has the page list, "Set page to default", Apply/OK/Cancel; File menu labels, "Open recent..." submenu (with "Clear recent file list" inside it) and Save enablement (only for a new or modified catalog) match WinForms; the preview/stop buttons are hidden without a reference project.
- Still open: double-click preview needs a loaded reference project; MAIN.SFX build/unpack were not driven live; SoundTool still aborts New/Open/Close when a requested save fails or is cancelled (WinForms continues anyway, see WadTool below) - decision needed.

### WadTool main window - visual pass (2026-10-30), functional pass PENDING
- Compared with `TombEngine.wad2` loaded in both builds. Layout, toolbar, panel headers, bottom buttons, status bar and 3D viewport color already match closely.
- Fixed: destination and source wad trees used `Brush_Background_Low` (#313335) while WinForms uses the panel color (#3C3F41); now `Brush_Background`.
- Left as S3: WPF tree rows are about 3 px denser than WinForms; WPF toolbar buttons have visible borders; the WinForms destination panel header is highlighted when active; WPF shows the saved reference project name in the toolbar (data, not a defect).
- Functional audit of `FormMain` (read-only agent, then spot-checked in code): menus, shortcuts, toolbar, context menus, tree interactions, reference project, recent files, unsaved-changes prompts, window title, status bar, command line and saved window placement all match. Not driven live yet.
  - Fixed: the Delete shortcut was subscribed on the whole tree control and could reach `DeleteObjects` from the notes, search and game-version inputs; it is now ignored for text boxes and combo boxes.
  - Behavior aligned to WinForms afterwards: Add object / Add to different slot / Change slot / Delete / Edit item are always enabled and report problems themselves; Save As becomes available with the first change (or for new/imported wads) and stays available; tree search follows the legacy pop-up rules (exact substring first, otherwise edit distance below 2, wraps, index resets when the text changes, red flash when nothing matches, closes on Escape or focus loss) but stays an inline row with an extra "Next" button; after answering "Yes" to the unsaved-changes prompt the action continues even if the save failed or was cancelled.
  - Not covered: dialog contents (Wave B/C), live driving of tree actions.

### Tomb Editor main window - first pass (2026-10-30), functional pass PENDING
- Compared the empty default level in both builds (floating tool palette is a separate window: capture with the screen-copy mode, not PrintWindow).
- Fixed: window title shows the product version like WinForms ("2.0.1", not "2.0.1.0"); the "Search for menu entry" box and button at the right of the menu bar were missing (command `SearchMenus` had no target) - now ported with the same rules (Enter or button, first enabled leaf containing the text, opens the parent menus) and focus on the `SearchMenus` shortcut; dock panel titles use the WinForms casing and names ("Sector Options", "Room Options", "Imported Geometry", "Objects in Room", "Item Properties", "Content Browser", "Texturing").
- Open S2: the Items and Imported Geometry panels show an empty box instead of the "Click here to load new WAD file." message (the view model message is set; the button content is not visible, cause not found); menu font is about one point smaller than WinForms.
- Intentional divergences kept: dockable flyby timeline panel, extra "Rooms" panel and 2D/3D tabs, pin/dropdown buttons in dock headers, extra status bar fields.
- Not done: functional inventory of FormMain, panels and toolbar.