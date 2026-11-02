# WPF vs WinForms behaviour test plan and tracker

Goal: every window and every main-window feature of Tomb Editor, WadTool and SoundTool must LOOK and BEHAVE like the WinForms original (develop worktree `D:\Repositories\Tomb-Editor-develop`). Each row below is opened in BOTH builds, exercised with the same steps, and the result is recorded here. Nothing is "done" until both columns have a result.

## Method (same for every row)

1. Same input file in both builds (copies in the scratchpad: `parity-te.prj2`, `parity-level.prj2`, `parity-a1.wad2`, `parity-te.wad2`, `parity-sounds.xml`, `import1.prj`). Never edit the originals.
2. Real mouse and keyboard through the guarded harness (`wt.ps1`): foreground window must be the test app, only the target window is captured, cursor restored. If the guard aborts, stop GUI tests (the user is active).
3. For every row run the checks D1-D8 that apply, record PASS, FIXED (difference found and fixed, commit id), DIFF (difference found, open), N/A (control does not exist in WinForms) or NOT RUN.
4. A difference found is fixed in the WPF code (never by changing WinForms), logged in `WPF-PARITY-FIXES.md` and committed with the fix; then the row is re-run in the WPF build.
5. Data checks (D8): do the same edit in both builds, save, and compare the saved files (prj2 / wad2 / xml) with a diff or hash of the relevant section. Same edit must give the same data.
6. Evidence: screenshot names `<row>-wf.png` / `<row>-wpf.png` in the scratchpad (not committed).

## Check dimensions

- D1 Look: layout, labels, texts, sizes, enabled/disabled state, tab order, icons, default and minimum size.
- D2 Initial state: values, selection, focus and default button when the window opens (also with empty data and with big data).
- D3 Mouse: click, double click, right click and context menu, drag, drag and drop, wheel, hover tooltips, click on empty area.
- D4 Keyboard: Tab order, Enter, Esc, Delete, arrows, Ctrl+A/C/V/Z/Y, F-keys, hotkeys from the keyboard layout, mnemonics.
- D5 Input rules: validation, limits, rounding, invalid text, paste, clamping, error messages and their wording.
- D6 Dialog flow: OK, Cancel, X, Esc, Apply, dirty prompts ("save changes?"), undo/redo, what happens to data on each exit.
- D7 Persistence: window size/position, last used values, settings saved to configuration, state after reopen.
- D8 Data result: the level/wad/catalog data produced is identical (see method 5).
- D9 DPI: sharp text and icons at 100%, 125%, 150%, 200% (needs permission to change the Windows scale; not run so far).

Legend in the tables: `-` not run, `P` pass, `F` fixed, `D` difference open, `n/a`. A cell holding `V` means "look already compared side by side in an earlier session, behaviour not yet" and counts as NOT RUN for D3-D8.

## Wave 1 - Tomb Editor main window (highest use)

| ID | Feature | Steps to run in both builds | D1 | D2 | D3 | D4 | D5 | D6 | D7 | D8 |
|----|---------|-----------------------------|----|----|----|----|----|----|----|----|
| TE-M1 | Menus (File, Edit, View, Rooms, Items, Textures, Transform, Tools, Window, Help) | open each, compare items, order, enabled state with and without selection, shortcuts text | V | - | - | - | n/a | n/a | n/a | n/a |
| TE-M2 | Toolbar | every button action, toggle state, tooltips, Customize | V | - | - | - | n/a | n/a | - | - |
| TE-M3 | 3D view navigation | orbit, pan, zoom, fly mode, wheel, keys, camera reset, relocate camera | V | P | P(orbit, wheel, pan) | - | n/a | n/a | n/a | n/a |
| TE-M4 | 3D view selection and editing | select sectors by click and drag, shift/ctrl combos, paint texture, select/move objects, copy/paste/stamp, delete | - | - | F(right click menu), P(click select) | - | n/a | - | n/a | - |
| TE-M5 | 2D map | selection, drag rooms, zoom, pan, room context menu | - | - | P(empty area menu) | - | n/a | - | n/a | - |
| TE-M6 | Sector options panel | every flag/toggle/button applied to a selected area, result in the level | V | - | - | - | n/a | n/a | n/a | - |
| TE-M7 | Room options panel | room type, flipmap, reverb, flags, ambient colour, effect, add/duplicate/delete room, tags | V | - | - | - | - | - | n/a | - |
| TE-M8 | Item browser / Imported geometry browser / Content browser | search, categories, drag to 3D view, double click, filter | V | - | - | - | - | n/a | - | - |
| TE-M9 | Lists: Objects in room, Rooms, Trigger list, Classic triggers | selection sync, double click, Delete key, multi select, sort | V | - | - | - | n/a | - | n/a | - |
| TE-M10 | Lighting panel | all fields per light type, undo | V | - | - | - | - | - | n/a | - |
| TE-M11 | Palette and Texture panel | pick colour, texture selection, tile size, bump/material buttons, sounds button | V | - | - | - | - | n/a | - | - |
| TE-M12 | Statistics, status bar | counts after add/delete, selected room line, coordinates | V | - | n/a | n/a | n/a | n/a | n/a | n/a |
| TE-M13 | Flyby timeline | add, move, delete flyby cameras, sequence combo, play (WPF keeps it dockable on purpose) | V | - | - | - | - | - | n/a | - |
| TE-M14 | Docking and layouts | float, hide, pin, save/restore layout, Window menu entries | V | - | - | n/a | n/a | n/a | - | n/a |
| TE-M15 | Undo/redo | every editing action undone and redone, object, room, texture, geometry | n/a | n/a | n/a | - | n/a | P(geometry: raise twice, undo once, lower = identical file; Edit > Undo after geometry hotkeys) | n/a | P(see data checks) |
| TE-M16 | File operations | New, Open, Save, Save as, recent files, dirty prompt on New/Open/Quit, file drop onto window, window title with * | - | - | - | - | n/a | - | - | - |
| TE-M17 | Build level / Build and play | log window, progress, errors, output file | - | - | - | - | n/a | - | n/a | - |

## Wave 2 - Tomb Editor dialogs

| ID | Window | Steps | D1 | D2 | D3 | D4 | D5 | D6 | D7 | D8 |
|----|--------|-------|----|----|----|----|----|----|----|----|
| TE-D1 | Search | type keyword, scope combo, list sort, double click, Enter, Delete, selection of result | P | - | - | - | - | - | - | n/a |
| TE-D2 | Search and replace objects | each object type and parameter, replace result | P | - | - | - | - | - | n/a | - |
| TE-D3 | Select rooms by tags | tag typing, case, result selection | P | - | - | - | - | - | n/a | n/a |
| TE-D4 | Keyboard layout | search, record a new hotkey, conflict, reset, save to configuration | P | - | - | - | - | - | - | - |
| TE-D5 | Editor options (9 pages) | every control of every page, apply, cancel, reset page, saved configuration file compared | P | - | - | - | - | - | - | - |
| TE-D6 | Level settings (10 tabs) | every tab: add/remove/reorder rows, browse, validation colours, OK vs Cancel vs Apply, saved prj2 compared | P | - | - | - | - | - | - | - |
| TE-D7 | Moveable / Static / Light / Camera / Flyby / Sink / Sound source / Sprite / Memo object dialogs | every field, ranges, OCB rules, colour pick, flags, result written to the object | P/V | - | - | - | - | - | - | - |
| TE-D8 | Trigger editor | every trigger type and target type, parameter lists, Locate, copy for script, raw mode, bits, undo | P | - | - | - | - | - | n/a | - |
| TE-D9 | Event set editor (partly run, see progress) | sets, events, node editor (rubber band, drag, groups), level script panel, filters, backup restore on cancel | P | - | - | - | - | - | - | - |
| TE-D10 | Animated textures | sets, frames, area edit, procedural animator, preview, rename/clone/delete | P | - | - | - | - | - | n/a | - |
| TE-D11 | Texture remap | pick source/destination, apply, undo | P | - | - | - | - | - | n/a | - |
| TE-D12 | Texture sounds, Bumpmaps, Material editor | per texture assignments and previews | P | - | - | - | - | - | n/a | - |
| TE-D13 | Search textures, Apply room properties, Crop room | run each and compare the room/level result | P | - | - | - | - | - | n/a | - |
| TE-D14 | Portal, Transform, Memo, Choose room, Quick item group, Select color | edit and apply, compare object data | P/- | - | - | - | - | - | n/a | - |
| TE-D15 | Import PRJ + Operation, Export/Import rooms, Geometry IO settings | full import of test.PRJ, imported room geometry, presets, saved result compared | P | - | - | - | - | - | - | - |
| TE-D16 | Texture preview, Wad preview, Toolbar layout, Imported geometry editor | open, interact, close behaviour | P/- | - | - | - | n/a | - | - | n/a |

## Wave 3 - WadTool

| ID | Window | Steps | D1 | D2 | D3 | D4 | D5 | D6 | D7 | D8 |
|----|--------|-------|----|----|----|----|----|----|----|----|
| WT-1 | Main window | open/save/new wad, source/destination trees, add object, change slot, delete, drag and drop, search, reference project, dirty prompt | V | - | - | - | n/a | - | - | - |
| WT-2 | Animation editor (partly run, see progress) | list ops (add, delete, cut/copy/paste, multi select, import/export), frames ops, timeline drag, play, keyframes, bounding box, root motion, bones list, undo/redo, menu bar items, hotkeys, rendering options | V | P(part) | - | - | - | P(part) | - | - |
| WT-3 | State changes, Anim commands, Find/replace anim commands | edit every column, dispatches, blend curve, play from here, apply vs OK | V | P | - | - | - | - | n/a | - |
| WT-4 | Mesh editor | tree and vertex modes, tools, attach, merge, normals, UV, textures, import/export | V | - | - | - | - | - | - | - |
| WT-5 | Static editor | lights, shatter, visibility box, collision box, colour | V | - | - | - | - | - | - | - |
| WT-6 | Skeleton editor | bone tree, move, add, remove, rotation order | V | - | - | - | - | - | n/a | - |
| WT-7 | Sprite sequence editor | add/remove sprites, order, preview | - | - | - | - | - | - | n/a | - |
| WT-8 | Animation fixer, New wad, Options, Item properties, Select slot, Lua properties, About | each field and button | P | - | - | - | - | - | - | - |

## Wave 4 - SoundTool

| ID | Window | Steps | D1 | D2 | D3 | D4 | D5 | D6 | D7 | D8 |
|----|--------|-------|----|----|----|----|----|----|----|----|
| ST-1 | Main window | New/Open/Save/Save as/Close with the WinForms save prompt differences (kept on purpose: abort on failed save), sound list, filter, add/delete, copy/paste, details pane, samples list, preview, Stop | V | - | - | - | - | - | - | - |
| ST-2 | Sound details | every field, chance/volume/pitch/range limits, flags, random pitch/volume, loop, panning, XML saved compared | V | - | - | - | - | - | n/a | - |
| ST-3 | Compile, unpack, index/unindex, reference catalog | each tool menu item and its output | - | - | - | - | n/a | - | n/a | - |
| ST-4 | Options | all fields | V | - | - | - | - | - | - | - |

## Order of work

1. Wave 1 first (TE-M3 to TE-M5, TE-M15, TE-M16 are the riskiest: real input handling), then Wave 2 in the order TE-D6, TE-D5, TE-D7, TE-D8, TE-D9, then Wave 3 WT-2 and WT-1, then Wave 4.
2. Each session: pick rows, run both builds, update the cells, commit this file together with any fix.
3. Rows that cannot be reproduced in the harness (for example needing a real game) are marked `D` with the reason, never silently skipped.

## Progress

Counted in cells (a row has up to 8 cells): see the tables. Summary line to update after each session:

- Data checks done with identical saved files from both builds (same edits, Ctrl+S, same SHA-256): untouched save; raise/lower hotkeys Q W R Shift+E; Q Q Ctrl+Z A; Transform menu Flatten floor + Smooth ceiling on a dragged area; Shift+Q Shift+W E then Edit > Undo; Rooms > New room + Duplicate rooms. Texture paint (F3, click face, click a tile of the texture panel): the same texture shows in both builds but the saved files differ, because a click at the same window fraction hits a different texture area in the two layouts; not comparable by hash (needs a keyboard or fixed-coordinate way to pick the tile). Edit > Select all + Delete and Copy/Paste on an area: WPF changes nothing, WinForms Delete leaves an extra window open and the file was not saved: OPEN, to be investigated (screenshot of the extra window pending, the foreground guard aborted again).
- WT-2 Animation editor run on screen: menu ops New animation, Insert frame, Reverse, Mirror, Calculate bounding box, field edits (Framerate, End frame, Next anim) give the same wad2 (1-2 bytes of date differ); timeline click and drag scrub, Left/Right/Down/I/O/Ctrl+C/Ctrl+V behave the same (no data change in either); State field with suggestions works. Not yet run: Split, Resample, Import/Export, anim list multi-select, cut/paste of animations, frame cut/paste with a real selection, interpolate, undo/redo stack depth, rendering toggles, bones list, transform fields while playing.
- TE-D9 Event set editor run on screen: new set, Action/Condition/Action, Ctrl+A, click on empty area, rubber band, Delete behave the same; saved level identical except node positions. Not yet run: drag of nodes and grips, Link button, copy/paste of nodes, colour, lock, export, level script mode, set tree drag and drop, backup restore on Cancel.
- TE-D6 Level settings Misc tab (Compress textures, Remove unused objects, tile padding 12, OK, save): identical prj2. TE-D5 Editor options General page (autosave off and 300 s, multiple instances, undo depth 30, OK): same four configuration keys changed in both builds. One text fixed (missing space after a comma). Other Level settings tabs and Options pages not yet run.
- TE-D7 Camera dialog: Search > Edit object properties opens the same dialog in both; glide timer set to 5 writes value 5 in both saved levels (the two builds edited different cameras because the harness double click hit different rows, so a byte-exact file match was not possible). Moveable dialog: opens in WPF (Object: bits, Invisible, No reflection, OCB, Tint); in the WinForms run the Search double click did not select Lara, so not compared yet.
- D1 look compared side by side now for all 9 Editor options pages and for the Level settings tabs Game, Sky & Font, Sound Infos, Sound Sample Paths, Path Placeholders, Misc, Imported Geometry, Static Meshes: same (one fix: red path boxes in Sky & Font). Input/data checks for the remaining Options pages and Level settings tabs (adding rows, browse, reload, Apply) not run.
- TE-D7 Object (moveable) dialog: opened for a FISH_EMITTER in both builds, same layout (Bit 1-5, Invisible, No reflection, OCB, Tint with reset). Setting Bit 2, Invisible and OCB 77 then OK and Save level: both saved levels grow by the same one byte and differ from each other only in which of the several FISH_EMITTER objects was edited (the harness picked different search rows) - semantics equal. Search results: WinForms also lists fuzzy matches dimmed below the exact ones; check the WPF Search for the same.
- TE-M16 File operations on screen: dirty marker "*" in the title after an edit; File > New level with unsaved changes shows the same prompt (title "New level", text, Yes/No/Cancel, No default) and, with No, the same empty Untitled level (one 18x18 room, status counts); File > Save level as writes an identical file in both and renames the window; File > Quit editor with unsaved changes shows the same prompt (title "Exit"). Not run: Open existing level, Open recent, drop of files onto the window, Build level.
- Rows fully run (all applicable cells P or F): 0 / 45
- Rows with the look already compared (V or P in D1): all rows except TE-M3, TE-M4, TE-M5, TE-M16, TE-M17, TE-M15, WT-7, ST-3
- Differences found by this plan and fixed: 5 (Items panel placeholder text; 3D view context menu never appeared; Sky & Font path boxes not red; Search weaker matches not dimmed; one text typo)
- Test setup note: both builds must use the same TombEditorConfiguration.xml (the WPF one differed: show all rooms, portals, FOV), baseline saved in the scratchpad as wf-config-baseline.xml; eset-config.ps1 applies it before each run; the user's WPF config is saved as wpf-config-backup.xml and must be put back at the end
