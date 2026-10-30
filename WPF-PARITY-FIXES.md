# WPF Parity Fixes Log

Every fix made while aligning the WPF editors with their WinForms counterparts (`develop` branch). The method and the per-window status are in `WPF-VISUAL-PARITY-PLAN.md`; this file is the running list of what was actually changed.

Rule for this file: every fix is added here in the same commit that makes it (hash filled in after the commit if needed). Items not yet fixed are kept in "Open items" until resolved.

## Shared theme (DarkUI.WPF) - affects every tool

| Fix | Why | Commit |
|-----|-----|--------|
| DataGrid visual states (Disabled, Current, alternating rows, selection) now swap whole brushes with `ObjectAnimationUsingKeyFrames` instead of animating `SolidColorBrush.Color` (`Styles/DataGrid.xaml`, 8 sites) | The animation mutated the shared `Brush_Background` / `Brush_Border` resources, turning a whole window `#525252` instead of `#3C3F41` once a disabled DataGrid was shown | b4e4cf0ac |
| New implicit `GridSplitter` style: transparent, `Brush_Border` on hover/drag (`Styles/GridSplitter.xaml`, merged in `Generic.xaml`) | Splitters without an explicit background were drawn white | b4e4cf0ac |
| Window title trims with an ellipsis and keeps clear of the caption buttons (`Styles/Window.xaml`, `Defaults.TitleBar_ButtonsWidth`) | Long titles (file paths) ran under the minimize/maximize/close buttons | b4e4cf0ac |

## SoundTool

| Fix | Why | Commit |
|-----|-----|--------|
| Details pane rebuilt to follow WinForms: ID / Global / Include for MAIN.SFX row, name with icon buttons, single parameters group with units, sample buttons above the list, no scrolling | Layout differed and buttons were hidden below the fold | 3afcb8de3 |
| Tools menu holds Options, Build TR2/TR3, Unpack, Select/Deselect all; Edit holds only Copy/Paste; labels match WinForms | Menu structure and wording differed | 3afcb8de3 |
| Options window: "%" unit and colon label | Cosmetic parity | 3afcb8de3 |
| Playback mode shows "None / One shot wait / One shot rewound / Loop" with the hint text under Mode (`LoopBehaviourOption`) | WPF showed raw enum names (`Looped`) and no hint | 065890b56 |
| Ctrl+C / Ctrl+V on the sound list copy and paste the sound info; menu shows the gestures | Shortcuts did nothing | 065890b56 |
| Unit tests: default new-sound values, select/deselect all, delete, playback mode names and hint updates | Protect the restored behavior | 065890b56 |
| Options: page list ("General"), "Set page to default", Apply / OK / Cancel (Apply applies without closing) | Dialog was a simplified single page with OK/Cancel only | f9e6b0ec7 |
| Unit tests for MAIN.SFX build and unpack (round trip through real files, and rejection of sample names that escape the destination folder) | Build/unpack had only been checked by reading the code | (this commit) |
| File menu: WinForms labels ("New XML", "Open XML, TXT or SFX...", "Save XML", "Save XML as..."), "Open recent..." submenu containing "Clear recent file list", disabled when empty | Menu wording and structure differed | f9e6b0ec7 |
| "Save XML" enabled only for a new or modified catalog (command `CanExecute`, so Ctrl+S follows it) | Always enabled | f9e6b0ec7 |
| Preview and stop buttons hidden when no reference project is loaded | They were only disabled | f9e6b0ec7 |
| "About Sound Tool" label and "Sound Infos: N" status text | Wording | f9e6b0ec7 |

## WadTool

| Fix | Why | Commit |
|-----|-----|--------|
| Destination and source wad trees use the panel background (`Brush_Background`) | They used `Brush_Background_Low` (`#313335`), darker than WinForms (`#3C3F41`) | 6e0d8aafa |
| Delete key handler ignores key events coming from text boxes and combo boxes inside the tree control | The handler was attached to the whole control (notes, search, game version included) and could delete the selected wad objects | 0e8374e68 |
| Add object / Add to different slot / Change slot / Delete / Edit item always enabled; the actions report problems themselves | WPF disabled them, hiding the WinForms error messages | 514f6a0c1 |
| Save As becomes available with the first change (or for new/imported wads) and stays available while a destination exists | WPF enabled it whenever a wad existed | 514f6a0c1 |
| Tree search follows the legacy pop-up rules: exact substring first, otherwise edit distance below 2, wraps around, index resets when the text changes, red flash when nothing matches, closes on Escape or focus loss (still an inline row with an extra "Next" button) | Search only did a plain next-match and never failed visibly | 514f6a0c1 |
| After answering "Yes" to the unsaved-changes prompt, New / Open / Close continue even if the save failed or was cancelled | Matches WinForms (WPF used to abort); less safe, chosen on purpose | 514f6a0c1 |
| Shell tests updated to the WinForms rules | Old tests encoded the previous WPF behavior | 514f6a0c1 |

## Tomb Editor

| Fix | Why | Commit |
|-----|-----|--------|
| Window title uses the product version ("Tomb Editor 2.0.1 - ...") | It showed "2.0.1.0" | b2965c938 |
| "Search for menu entry" box and button restored at the right of the menu bar (`MenuSearchHelper`); focus on the `SearchMenus` shortcut | The command existed but nothing received it; feature was missing | b2965c938 |
| Dock panel titles use the WinForms names: Sector Options, Room Options, Imported Geometry, Objects in Room, Item Properties, Content Browser, Texturing | Casing and the "Textures" name differed | b2965c938 |
| Items and Imported Geometry panels show "Click here to load a new WAD file." / geometry message when nothing is loaded (message button no longer has a fixed 24 px height) | The text was clipped away: the button style forces a height of 24 px, the message needs about 34 px | (this commit) |
| Main menu enabled/visible rules ported (`MainMenuStateUpdater`): Cut/Copy/Delete/Stamp/Bookmark/Edit/Split follow selection and mode, Undo/Redo follow the undo stack, Save follows unsaved changes, sector transforms follow a valid sector selection, texture/wad/sound entries follow loaded counts, "select bookmarked object" follows the bookmark, Paste follows the clipboard when Edit opens; version-specific entries (sprite, NG group, TEN volumes/event sets, object names, step height + separator) are shown only for the right game versions | Every menu item was always enabled and visible; audit found no `IsEnabled`/`Visibility` rules at all | (this commit) |
| Main window shell: file drag-and-drop onto the window (prj, prj2, wad, images), second launch with a .prj2 opens it in the running instance (single-instance receive), window size/position/maximized state saved and restored (`Window_FormMain_*`), editor is told when the window is activated (`MainWindow.Shell.cs`) | Dropping files, opening a level from Explorer into a running editor, remembering the window and activation focus were missing | (this commit) |
| In-place search shortcuts (Shift+F1..F4: rooms, items, imported geometry, textures) activate the panel and open its search box (`SearchableComboBox.OpenAndFocusSearch`, `MainWindow.ActivateSearch`) | The commands raised an activation event nobody handled | (this commit) |
| Triggers list: multi-selection, Delete/Backspace delete the selected triggers, delete button removes all selected, panel title becomes "Classic Triggers" for TombEngine levels | Single selection only, no keyboard delete, title was fixed | (this commit) |
| Objects in Room list keeps its selection when an object changes (entry text updated in place) and scrolls the selected object into view; Rooms list keeps its selection when a room property changes and scrolls into view | Moving or editing the selected object un-highlighted it; selection was never scrolled to | (this commit) |
| Texturing double-sided button is a toggle that shows the selected texture state | It was a plain button with no checked state | (this commit) |
| Toolbar enabled states (`ToolbarEnableRules`): Undo/Redo follow the undo stack, Copy/Stamp need a spatial selection, Paste follows the clipboard (listener on the window) and mode, Center camera is off in 2D, Flip map needs an alternated room, volumes and sprite buttons follow the game version | Those buttons were always enabled | (this commit) |
| Window > Tool Palette shows/hides the floating tool palette and the state is saved per layout (`ShowToolbox`), with its check mark; View > "Draw untextured in Lighting Mode" and "Show real tint for objects" show their check marks | The command had no target (palette always shown) and the toggles had no state | (this commit) |
| Toolbar Undo shows the "irreversible undo" icon when the next undo cannot be reverted; Flip map is a toggle showing whether the selected room is the flipped one | Icon never changed and Flip map had no checked state | (this commit) |
| The dock layout in use is saved on exit and restored at the next start, also without a custom layout (`Window_Layout.AvalonDockState`) | Only named layouts were persisted; the default always came back as the XAML baseline | (this commit) |
| Shortcut text in menus and toolbar tooltips refreshes when the keyboard configuration changes | Computed once at load, stale after changing hotkeys | (this commit) |
| In fly mode the Alt key is swallowed so the menu bar does not activate | Alt opened the menus while flying | (this commit) |
| Texturing: every change of the visible level texture is announced to the editor (Palette sampling follows it), and the panel comes to the front when a texture is centered from elsewhere | Editor was not told after rebuilds or selection events; panel stayed in the background | (this commit) |
| Items and Imported Geometry previews re-apply the "animate preview" option when the configuration changes and their panels come to the front when the chosen item changes elsewhere | Option applied only once at startup; no activation | (this commit) |
| Room tags (and any AutoCompleteBox): Enter commits the text | Only losing focus committed it | (this commit) |
| Palette toolbar sits on the left when the panel is wide and on top otherwise (not verified on screen) | Fixed left column | (this commit) |

## Tomb Editor dialogs (Wave B)

| Fix | Why | Commit |
|-----|-----|--------|
| FootStepSounds and BumpMaps remember size, position and maximized state (Window_FormFootStepSounds_*, Window_FormBumpMaps_*) | The windows always opened at the default size | (this commit) |
| BumpMaps: the custom-file checkbox is disabled without a selected texture; clicking the path label toggles the custom file | WinForms behavior | (this commit) |
| Crop Room (ResizeRoom): title "Crop Room", Preview and New dimensions groups, the four spinners placed around a north indicator (Z+ top, X- left, X+ right, Z- bottom, labels as tooltips), "Allow rooms bigger than 30x30" text from the game limit, walls option above floor, size 600x400 with minimum 540x355 | Layout was four labeled rows and a generic title (verified on screen against WinForms) | (this commit) |
| Titles and labels aligned with WinForms: Select Rooms by Tags, Search & replace objects, Transform Edit with "Position:", Sink with "Current strength:", Search "Keywords:" and plural delete confirmations | Wording differed | (this commit) |
| Transform Edit: axis letters (X/Y/Z, P/Y/R), rotation with 2 decimals and wrap-around, scale with 2 decimals, Alt increments (16 / 5 / 1); OK clears the pending relight when nothing changed, Cancel relights only when something changed and relighting is enabled (not verified on screen: needs a selected object) | Rotation was shown rounded, no axis labels, relight rules differed | (this commit) |
| Search & replace objects: Enter runs "New search" (not Replace); picking up the object selected in the level when the selection type changes; the Select buttons start with the right enabled state | Enter replaced objects by mistake; selected object was ignored | (this commit) |
| Memo: Tab moves focus (no tab character) and Ctrl+Enter saves even while typing | AcceptsTab inserted tabs; Enter handling could swallow Ctrl+Enter | (this commit) |
| Operation dialog: Cancel closes the dialog after a cancelled run (it could stay stuck); the log supports selection and Ctrl+C | Cancel did nothing the second time; log was not copyable | (this commit) |
| Search dialog: multi-selection with delete of rooms and objects together (triggers first, partial-delete notice), keywords field first and focused, columns Type / Room / Name, trash icon button on the left and OK on the right (Enter closes), default size 650x350, delete never disabled (verified on screen) | Only one entry could be selected and deleted; layout and Enter behavior differed | (this commit) |
| Toolbar layout dialog: every button shows its toolbar icon, double-click moves a button between the lists, window cannot grow horizontally (not verified on screen: opens from the toolbar context menu) | Lists were plain text and double-click did nothing | (this commit) |
| Tag autocomplete (Select Rooms by Tags and Room Options): suggestions are words that start with what was typed, never a tag already entered, the first match is highlighted and Enter inserts it (not verified on screen) | It matched any substring, repeated tags and needed the mouse or arrow keys | (this commit) |
| Import PRJ: title shows the PRJ file name, groups "Base sound catalog (optional)" and "Import settings" with the always-visible description texts, no extra PRJ path row (not verified on screen) | Descriptions were hidden in tooltips | (this commit) |

## Tomb Editor dialogs (Wave C)

| Fix | Why | Commit |
|-----|-----|--------|
| Texture remapping assistant: restrict-to-selected-rooms and remap-animated-textures start checked, scale factor 0.25..256 with 2 decimals, Enter runs Remap, WinForms captions, size and position remembered | The defaults made Apply touch the whole level; fractional scaling was impossible | (this commit) |
| Search textures: size and position remembered, minimum size 330x400 | Window did not remember its place | (this commit) |
| Moveable and Static: the tint is written on OK only when the user changed it (the 8-bit swatch round trip silently altered the stored value) and only for colorable objects; WinForms captions ("Object", "Tint:", "Reset tint to default", "Activate heavy trigger on collision" and the other static flags) | Opening and OK-ing a dialog changed the tint; wording differed | (this commit) |
| Flyby camera: value ranges and steps as in WinForms (sequence and number 0..255, roll 0..360 and rotations with wrap-around, DOF distance/range up to 1,000,000 step 128, DOF strength 0..1), unused flag bits labeled "Unused" outside TR5/TombEngine, shorter DOF labels | Ranges were wider and flag captions wrong for other games | (this commit) |
| Options: numeric fields without an explicit limit are capped at 100 like in WinForms, zoom steps and minimums as in WinForms, title "Options" and "Set page to default" | The WPF defaults allowed up to 1,000,000 | (this commit) |
| Keyboard layout: hotkeys with Alt can be captured (WPF reports Key.System while Alt is held) | Alt combinations could not be recorded | (this commit) |
| Portal, Imported geometry: WinForms captions ("Edit portal", "Portal effect:", "Mirror options", "Chosen entry:", "Force sharp edges", "Hide in editor", "Faster alpha testing") | Wording differed | (this commit) |## WadTool dialogs

| Fix | Why | Commit |
|-----|-----|--------|
| English strings added for Select slot, Item Properties (Lua), Find & replace anim commands and Animation fixer (titles, labels, tooltips, status and warning messages, with the WinForms wording) | These windows had no strings and would have shown raw keys such as "ChosenId" or "EndFrameTooltip" | (this commit) |
| WadTool options: missing "Auto-consolidate textures when copying objects" option added, captions and the "Options" / "Set page to default" texts restored | The option could not be toggled; wording differed | (this commit) |
| Mesh editor (subagent work, checked on screen): icon toolbar in the WinForms order with WinForms tooltips, Mode: label and WinForms mode names, per-tab captions and tooltips, Preview and Convert from shades in Vertex effects, texture panel as icon buttons with full captions as tooltips, Mesh list heading and search icon, window 1017x646 min 1000x640. Open: "Animated textures" button not visible next to Materials in the test mesh | Toolbar and captions differed | 281e24a9b |
| Mesh editor: Cancel hidden in realtime tree mode, Enter runs Remap on the vertex number and Search on the search box, WinForms tooltips on the toolbar. Opened on screen (single-mesh mode); the keys and tree mode are only built, not tried. The empty 3D view is the same in WinForms (the test mesh is tiny), so it is not a WPF defect. The Texture group now comes first with a taller map like the WinForms Texturing section; the texture preview zoom still differs (WinForms shows it larger) | Parity with the previous form | (this commit) |
| State changes: double click plays only on a cell (not on header or blend curve button), side buttons use icons; Blend curve: double click on a handle also resets to Linear; Anim commands: side buttons in the WinForms order with icons, a neighbour row is selected after Delete; new shared FindAncestor helper in TombLib.WPF. Window opening and double click tried with the real mouse | Wider double click scope, glyph buttons | (this commit) |
| Animation editor keys compared side by side with the WinForms editor using the real keyboard and mouse: Left wraps to the last frame, I / O mark the selection, Ctrl+A selects all, Esc clears selections (all match now); status bar shows "Selected keyframes: a to b"; captions (Name:, State:, Framerate, Start/End X/Z vel), transform modes (None ... Symmetric smooth) and blend presets (Ease In, Ease Out, Ease In and Out) restored | Keys and captions differed | (this commit) |
| Animation editor: unsaved-changes prompt on close (Yes saves, No discards, Cancel stays), title shows * while dirty. Tested with the real mouse (edit frame rate, close with X: prompt appears; clean close: no prompt) | Closing with X silently discarded the work | (this commit) |
| Static editor right column rebuilt on the WinForms layout: X/Y/Z in three columns with the label above, Clear left of Calculate, Reset to default beside Shatter, sound combo with play icon, lighting combo with add/delete icons, light list beside Intensity/Radius/Ambient, Draw order Lights/Gizmo/Grid/Normals, no scrolling, min size 900x765. Compared on screen with the WinForms editor | Layout differed and needed a scroll bar | (this commit) |
| Static, Mesh, Animation and Anim command editors: local NumericUpDown styles now use BasedOn the theme style (the numeric fields were drawn without template, i.e. invisible in the Static editor); Static editor: Intensity and Radius keep 2 decimals (step 0.03), position/box range +-65535, searchable shatter sound combo, Normals lighting disables the light controls on open, Pos Z no longer clipped. Verified on screen (WPF Static and Animation editors) | Numeric fields were not visible; decimals were lost | (this commit) |
| English strings added for Blend curve, Sprite sequence and State changes editors (titles, columns, tooltips, file dialog titles, error and status messages) | These windows would have shown raw keys | (this commit) |
| Mesh editor and Animation editor remember size, position and maximized state; Skeleton editor context menu shows the Ctrl+O / Ctrl+P / Ctrl+Up / Ctrl+Down shortcuts and has the WinForms minimum size (800x500) | The two editors always opened at their default size | (this commit) |
| Animation editor keyboard shortcuts of the previous form: Ctrl+Z / Ctrl+Y / Ctrl+S, Ctrl+X / C / V on frames, Ctrl+I interpolate, Space play, Left / Right / Up / Down frame navigation, Delete frames (ignored while typing; not verified on screen) | The window had no keyboard shortcuts at all | (this commit) |
## DPI and side by side checks

| Fix | Why | Commit |
|-----|-----|--------|
| DPI: audited all three manifests (PerMonitorV2 with fallback; SoundTool now uses the same value as the others), icons are SVG vectors (crisp at any scale), window .ico files carry 16-256 px sizes; the only raster UI icon (search-16.png in SearchableComboBox) replaced by a vector magnifier. Windows scale cannot be changed here without altering the user display, so only the current scale was seen on screen | Raster icon blurred above 100 percent | (this commit) |
| Animation editor Animations menu: removed the extra Replace item (WinForms has Replace only on the toolbar); menu names of Edit, Animations, Frames and Rendering otherwise identical to WinForms (compared by UI Automation) | Same menus | (this commit) |
| Animation editor timeline context menu: same six items in WPF and WinForms (right click with the real mouse, read via UI Automation); animation list filter: "walk" and "15" give the same lists | Verified side by side | n/a |
| Animation editor right double click on the 3D view: the panel already delivers the double click message, so the manual timing detection was replaced by OnMouseDoubleClick. Compared with the WinForms editor using the real mouse (orbit, then right double click): the image after the reset is identical to the start in both | The reset did not work in WPF | (this commit) |

## Side by side checks, Tomb Editor

| Fix | Why | Commit |
|-----|-----|--------|
| Tomb Editor menus compared with the WinForms editor (File, Edit, View, Rooms, Items, Textures, Transform, Tools, Window, Help, read via UI Automation): identical except items that depend on the game version (the WinForms editor shows the TombEngine-only volume items on a TR4 level at startup, the WPF editor hides them as the code rule says) | Verified side by side | n/a |
| Level settings, all ten tabs compared side by side on screen with the WinForms dialog (Game tab: game directory field and WinForms captions; the other nine tabs reworked by a subagent from the screenshot differences: columns, captions, button strips, Sky and Font, Sound infos, Misc, Path placeholders). Needs a second side by side pass after the merge | Layout and captions differed | e6561b060, 3be2ded1e |
| Keyboard layout: compact rows (24 px), icon add and delete buttons, magnifier search button, hotkeys column fills the width. Compared on screen with WinForms | Rows were twice as tall, text buttons | (this commit) |
| Crop room: vector compass (ring, ticks, north arrow) instead of the "N" box. Compared on screen with WinForms | Different picture | (this commit) |
| Search dialog compared on screen: same layout. Differences left: WinForms right-aligns the Keywords and Scope labels and preselects the first result row | Cosmetic | n/a |

| Editor options: every tab compared side by side with the WinForms dialog on screen and by reading control labels with UI Automation; groups, items and captions now in the WinForms order (composite rows for "Animate camera on:" with three checkboxes, font plus Bold, checkbox with its number). Seven of nine pages identical by the automatic check, the other two differ only by how UI Automation sorts a label against its checkboxes | Order was reversed or different | 4aef75004 |
| Level settings after the merge, compared on screen: Static Meshes and Sound Infos now match the WinForms tabs; Sky and Font path boxes fill the tab width (the tab item aligned the content to the left); Static Meshes column headers widened. Other tabs not yet rechecked after the merge | Layout | (this commit) |

| Level settings: every tab item stretches its content, so the Game tab path boxes and the Misc level path fill the width like in WinForms. Game, Texture Files, Static Meshes, Sky and Font, Sound Infos and Misc compared on screen and matching; Object Files, Imported Geometry, Sound Sample Paths and Path Placeholders still to recheck | Layout | (this commit) |

| WadTool round 3 (subagent, built; items marked on screen were tried with the real mouse and keyboard): State changes numeric cells take only valid Int16 text (letters ignored, tried on screen) and an out of range value keeps the previous one; Delete button no longer saves per row; Sprite sequence and Static editor Enter no longer closes the dialog; Sprite sequence strip order and 50 px rows; Animation editor list click resets the chain playback start and grid position; Anim commands keep the selection on animation change | Behaviour parity | d06c5295c, ed270a08d |
| Tomb Editor dialogs round 3 (subagent, built from the WinForms designers): Search labels right aligned and first row preselected (compared on screen, identical); Moveable OCB focus and tint reset icon; Trigger without default button and WinForms tab order; Import PRJ group order and hint texts; Operation, Memo, Replace object, Texture remap, Footstep and bump map tab order, Toolbar layout icons, Imported geometry title and tooltips, Sound source and Flyby camera layout, Event set editor tooltips. Only Search was compared on screen so far | Layout, captions, tab order | 249ba15eb |

| Palette panel: opening a real TombEngine level crashed with an unhandled NullReferenceException (the texture palette was read from a missing texture, in both the view and the view model); also removed a stray SizeChanged subscription that was added on every palette reset. Found by opening a real level in the WPF editor | Crash on opening levels | (this commit) |

| Statistics bar: when a level was opened the room counters could stay at 0 because a statistics pass already running discarded the newer request (selected room not yet set). The request is now remembered and the pass repeated. Opened a real TombEngine level on screen: Objects now 1 / 167 as in WinForms (before 0 / 167). The same race exists in the WinForms editor but its timing hides it | Wrong room statistics | (this commit) |

| Event set editor compared on screen with the WinForms dialog on a real TombEngine level: list buttons in the WinForms order (new, folder, copy, delete, search), no "Event sets:" label, default size 930x500, node editor area shown (disabled) before a set is selected. The node toolbar now sits at the bottom of the node area with icon buttons (rename, colour, lock, link, delete, export, clear, find) like WinForms; still different: WinForms adds Action and Condition buttons (function chosen per node) while WPF keeps the function box plus Add | Layout | (this commit) |

| Moveable ("Object") dialog compared on screen with the WinForms editor on a real TombEngine level (PUZZLE_HOLE3): same layout (bits, invisible, no reflection, OCB, tint with reset icon, OK and Cancel). The WinForms OCB text is selected on open; the WPF box now selects it too (checked on screen). Also confirmed on this level: statistics bar, status line, texture and wad loading, Edit menu | Verified side by side | n/a |

| Level settings compared on screen with real data (TombEngine level): Texture and Object file rows use a text Browse button and a small icon Show button like WinForms; Sound sample paths are red only when the parsed path is rooted and the folder is missing (the rule of the WinForms grid) and use a compact row; Imported geometry fixed columns narrowed so the Path column is visible. Texture, object and sample tabs now match; Static meshes and Sound infos matched earlier | Layout and validity rule | (this commit) |

| Dialogs compared on screen with the WinForms editor: Search textures (same), Apply room properties (same list; WPF title was truncated by the minimize and maximize buttons, window widened to 320), Animated textures (procedural animation checkboxes Symmetric and Smooth now start unchecked like WinForms). Still different in Animated textures: extra copy button in the set row, no search icon next to the texture combo, extra Text column in the frame list, Type and FPS settings were enabled without a selected set (now disabled, checked on screen) | Layout and defaults | (this commit) |

| Trigger editor compared on screen with the WinForms editor (sector selected with the real mouse, Items > Add trigger): same fields, bit and one shot checkboxes, raw mode, OK and Cancel; fixed the "(&&) Timer" label that showed both ampersands, and added the permanent search icon button next to the type, what and param combos (it opens the combo search like the WinForms search button) | Label and search button | (this commit) |

| Texture remapping assistant compared on screen with the WinForms editor on the real level (texture area selected with the real mouse): same title, texts, options, scale factor and buttons. Fixed: the texture combos show "Maya09b (at $(LevelDirectory)\Maya09b.TGA)" like WinForms (they showed only the path) and the maps start with the top of the texture at the top of the view (they were created before the final size was known and started in the middle). Still different: WinForms maps have scroll bars and section-style headers; the same start-position issue may exist in the Footstep sounds and Bump maps windows | Display name and start position | (this commit) |

| Texture sounds and Bumpmaps windows compared on screen with the WinForms editor on the real level: same combos, sound/bump labels on the texture tiles and buttons. Fixed the combo names (now "Maya09b (at ...)") and the initial map view (top of the texture at the top, as in Texture remap). Still different: WinForms has a search icon next to the texture combo and scroll bars in the map | Display name and start position | (this commit) |

| Search icon button next to the texture combos of Texture remap (source and destination), Texture sounds and Bumpmaps (shared ComboSearch.OpenCommand opens the combo drop-down with the search box focused); the Remap combos are now searchable combos. Checked on screen in Remap and Texture sounds; Bumpmaps built with the same markup. The same search icon was added next to the texture combo of Animated textures and the Bumpmaps one was rechecked (both seen on screen); the Animated textures map now starts at the top of the texture like the other map windows (seen on screen). The black frame thumbnails of the test set come from textures missing in that level, not compared with WinForms | Search button like WinForms | (this commit) |

| Event set editor node editor driven with the real mouse on the real level (global event sets): new set, function chosen, Add pressed three times (nodes chained under the selected node as WinForms does), Ctrl+A selects all three nodes (blue borders), toolbar at the bottom with icons. Rubber band drag and the WinForms comparison of these gestures were not conclusive on the captures | Verified in WPF only | n/a |

## Subagent batch (built only, NOT yet verified on screen)

Implemented by subagents in separate worktrees and merged; each item compiles but still needs a real mouse and keyboard check against the WinForms tool.

| Area | Fix | Commit |
|------|-----|--------|
| Animation editor | Top menu bar (Edit, Animations, Frames, Rendering) with enable rules; insert n frames, delete every nth frame, resample, resample to framerate, batch import/export, fix current/selected/all, find and replace animcommands, per-frame bounding box | 0bd3faf05 |
| Animation editor | Animation list filter (s:/a:/name: tokens), multi-select, add and delete selected buttons, selection no longer cleared when empty | 0bd3faf05 |
| Animation editor | Timeline context menu, double click on an anim command opens its editor, right double click on the 3D view resets the camera | 0bd3faf05 |
| Event set editor | Level-script panel (function list, filter, unassign, browse notice, argument, call count) | d9466f22b |
| Event set editor | Node editor multi-selection (rubber band, Ctrl/Shift, group drag), key map N/A/C/L, Ctrl+A, Esc, Delete/Backspace, Shift+Delete, grid size/step/width/ropes from NodeEditor_* options, Link button | d9466f22b |
| Event set editor | Splitter persistence, set search popup, name warnings as toasts, Delete/Backspace on the tree | d9466f22b |
| Moveable/Static | Tint picker calls EditorActions.EditColor (live preview, rollback on cancel); tint no longer rewritten on OK; Static checkbox order and scalable field visibility | 4570e9e51 |
| Level settings | Browse uses shared reload logic; catalog Reload / Edit in SoundTool / Include all; adding a wad adds its catalog and switches game version; ConvertLevelExtension on version change; lists sync on change | 0ef87333c |
| Level settings | Sky and Font radios with previews, path variables live, prompts above lists, samples tab hidden for MAIN.SFX, TRNG Area / Original ID columns, TR5 weather and Lara type on Misc | 0ef87333c |
| Texture remap / Options | Instruction label and destination clamp; colour preset resets on manual colour edit, refresh on configuration change | 99712b794 |

Known gaps reported by the agents: node grid overlay stays at a fixed 32 px tile, NodeEditor_ShowGrips unused, Export in the node editor still copies one node, blank nodes cannot be created, chain-playback reset on list click not ported, Imported geometry Browse does not use the reload logic, From catalog button handles a single row.
## Open items
| Event set editor: closing with the window X or Escape restores the backup (only OK keeps changes), Unassign clears the volume's event set, generic mode starts with no set selected and hides the volume options | The X button committed; Unassign did nothing; generic mode preselected a set | (this commit) |
| Level settings: size and position remembered; "Dither 16-bit textures" also enabled for TRX games; "Map animated textures to imported geometry" disabled for TombEngine | Window did not remember its place; enable rules differed | (this commit) |
| Level settings: "Show" button on texture and wad rows opens the texture preview and the wad 3D preview next to the row (they close on focus loss), enabled only for loaded files; wad rows with a version mismatch or unknown data are flagged | The preview windows existed but nothing opened them | (this commit) |
| Paste availability (menu and toolbar) tolerates a locked clipboard instead of throwing (`ClipboardHelper`); the WinForms editor crashes at startup in that situation | Reading the clipboard while another program holds it raised an exception | (this commit) |- Tomb Editor: not verified on screen (verified by build and code only): file drops onto the window, saved window placement, selection behavior of the Objects in Room / Rooms lists, trigger deletion with Delete/Backspace, Palette layout when the panel is narrow (wide layout verified).
| Imported geometry dialog: swap/flip/UV/invert-faces columns, delete asks for confirmation, non-ANSI paths are rejected, lighting model shown with the WinForms captions, size and position remembered (not verified on screen: opens from an imported geometry object) | Columns and checks were missing | (this commit) |
| Event set editor: WinForms captions ("Event set name:", "NPC", "Other objects", "Flyby cameras"), tooltips on activators, volume options, Unassign and the per-event "Enable" checkbox (writes the event's enabled flag), Unassign enabled only with a selected set (not verified on screen: the menu entry is TombEngine-only) | Per-event Enable was missing; no tooltips | (this commit) |
| Moveable OCB field accepts digits and a leading minus, Static OCB digits only; Keyboard layout scrolls the found row into view; Sound source minimum size 500x520 | Fields accepted any text; search did not scroll | (this commit) |
- WadTool Animation editor still lacks (from the audit): the menu bar (batch import/export, resample, fix selected/all, find & replace anim commands, insert/delete every nth frame, bounding box per frame), animation list filter with multi-select and add/delete buttons, timeline context menu and anim command double-click, Esc/I/O/Ctrl+A keys, WinForms captions and group headers, right double-click camera reset.
- WadTool Mesh editor still lacks: Cancel hidden in tree mode, Enter on vertex number and search boxes, icon toolbar with the WinForms tooltips, tab captions and tooltips, Preview button placement.
- Wave B (simple Tomb Editor dialogs) done: the 18 dialogs were audited and the differences fixed, except the ones listed in the divergences.
- Wave C (complex Tomb Editor dialogs) is partly done. Still to do from the audits:
  - Event set editor: search button, per-event "Enable" checkbox, level-script mode (function list, browse link, call count), node editor multi-selection and its toolbar and key map (N/A/C/L/Ctrl+A/Esc/Backspace), node editor options from the configuration, tooltips and captions, splitter position, name validation popups.
  - Level settings: layout of the Game/Misc tabs, prompt texts above lists, catalogs grid (reload, edit in SoundTool, include all), sound grid columns and refresh, wad add that also adds its sound catalog and switches game version, level extension on game version change, font/sky/TR5 sprite previews and radios, path variables refresh, Browse via the shared reload logic.
  - Moveable and Static: live tint preview while picking, OCB input filter, WinForms layout and checkbox order (static), compact flyby layout for TR1-TR3, SoundSource search icon and minimum size, Trigger layout details.
  - Keyboard layout: scroll to the found row, icon buttons; Options: preset reset on manual color change, refresh on configuration change; Texture remap: instruction label and source/destination clamp.
## Intentional divergences (kept on purpose)

- Dark custom title bar instead of the system accent color.
- Tomb Editor dialogs: Select Rooms by Tags keeps lowercase tags (consistent with Room Options); Transform scale is limited to 0.01..1024 (WinForms accepted 0 and any value) and positions to +-1048576; Operation dialog colors are darker row bars; ChooseRoom, QuickItemGroup, Camera, Sink and Sprite differ from WinForms only in window size and auto-sizing; Search does not shade rows by match quality; Sprite preview is a flat image, not a 3D render; Memo and FootStepSounds default sizes follow the saved configuration.
- Tomb Editor: the Debug menu (developer scratch actions, visible only with a debugger attached) is not ported; room tags are normalized to lowercase.
- Theme font is 11 px (WinForms 9 pt = 12 px), so menus and labels are slightly smaller everywhere; changing it would touch every control.
- Tomb Editor: dockable flyby timeline, extra "Rooms" panel, 2D/3D tabs, pin/dropdown buttons in dock headers, extra status bar fields.
- SoundTool: a failed or cancelled save still aborts New / Open / Close (WinForms continues anyway); kept on request. Double-click preview needs a loaded reference project (WinForms without one fails silently, so the outcome is the same).
- WadTool: tree search is an inline row, not the WinForms pop-up; the destination panel header is not highlighted when active and tree rows are about 3 px denser (S3).
- SoundTool: details pane always visible but disabled without a selection; extra Stop button; "Select/Deselect all" marks the catalog as modified; build/unpack iterate sounds ordered by ID; Ctrl+C/V bound to the sound list only.
- WadTool: inline tree search row with an extra "Next" button.
