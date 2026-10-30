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
| Portal, Imported geometry: WinForms captions ("Edit portal", "Portal effect:", "Mirror options", "Chosen entry:", "Force sharp edges", "Hide in editor", "Faster alpha testing") | Wording differed | (this commit) |## Open items
| Event set editor: closing with the window X or Escape restores the backup (only OK keeps changes), Unassign clears the volume's event set, generic mode starts with no set selected and hides the volume options | The X button committed; Unassign did nothing; generic mode preselected a set | (this commit) |
| Level settings: size and position remembered; "Dither 16-bit textures" also enabled for TRX games; "Map animated textures to imported geometry" disabled for TombEngine | Window did not remember its place; enable rules differed | (this commit) |
| Level settings: "Show" button on texture and wad rows opens the texture preview and the wad 3D preview next to the row (they close on focus loss), enabled only for loaded files; wad rows with a version mismatch or unknown data are flagged | The preview windows existed but nothing opened them | (this commit) |
| Paste availability (menu and toolbar) tolerates a locked clipboard instead of throwing (`ClipboardHelper`); the WinForms editor crashes at startup in that situation | Reading the clipboard while another program holds it raised an exception | (this commit) |- Tomb Editor: not verified on screen (verified by build and code only): file drops onto the window, saved window placement, selection behavior of the Objects in Room / Rooms lists, trigger deletion with Delete/Backspace, Palette layout when the panel is narrow (wide layout verified).
- Wave B (simple Tomb Editor dialogs) done: the 18 dialogs were audited and the differences fixed, except the ones listed in the divergences. Wave C (complex dialogs) not started.
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
