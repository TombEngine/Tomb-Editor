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

## Open items

- Tomb Editor (from the functional audit, still to fix): hotkey text refresh on keyboard layout change; Alt handling in fly mode; Debug menu; Texturing level-texture sync with the editor and panel activation; Room Options tags commit/casing; item preview animate option not re-applied; Palette layout adaption.
- SoundTool: a failed or cancelled save still aborts New / Open / Close (WinForms continues); left as is on request.
- SoundTool: double-click preview needs a loaded reference project; MAIN.SFX build/unpack not driven live.
- WadTool: tree search is an inline row, not the WinForms pop-up; destination panel header is not highlighted when active (S3); tree rows are about 3 px denser (S3).
- All tools: dialogs (Waves B/C) not started.

## Intentional divergences (kept on purpose)

- Dark custom title bar instead of the system accent color.
- Theme font is 11 px (WinForms 9 pt = 12 px), so menus and labels are slightly smaller everywhere; changing it would touch every control.
- Tomb Editor: dockable flyby timeline, extra "Rooms" panel, 2D/3D tabs, pin/dropdown buttons in dock headers, extra status bar fields.
- SoundTool: details pane always visible but disabled without a selection; extra Stop button; "Select/Deselect all" marks the catalog as modified; build/unpack iterate sounds ordered by ID; Ctrl+C/V bound to the sound list only.
- WadTool: inline tree search row with an extra "Next" button.
