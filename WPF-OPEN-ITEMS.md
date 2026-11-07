# WPF docking: open items

Known gaps and cleanups left after the AvalonDock theme and popout work on Nickelony/NEW-WPF.

## Bugs

- **Dock minimum sizes are not applied to panes shown later.** `ScheduleDockMinimums()` runs only after first render and after `LoadDockState`. `ToggleAnchorable`, `ActivatePanel`, `ActivateSearch` and `ApplyFlybyTimelineVisibility` in `TombEditor/MainWindow.xaml.cs` show or re-dock panes without re-applying minimums.
  - Repro: start the editor, then open the Flyby timeline from the Window menu. Its pane keeps `DockMinHeight` 0, so a splitter can collapse it below its content.
  - Fix: call `ScheduleDockMinimums()` after each show or dock. Do not subscribe to `LayoutUpdated`, which would flood the event handlers.
- **`ApplyDockMinimums` keeps stale values.** It skips panes whose views report a zero `DesiredSize`, so an old minimum stays in place.

## Cleanups

- `ApplyDockMinimums` has a 19-line loop inside the 1000-plus-line `MainWindow` class. AGENTS.md asks for large blocks to move into helpers. Move it into a static helper that takes the `DockingManager`.
- `TombEditor/Command.cs`: the pop-out helper silently does nothing if `Application.Current.MainWindow` is not a `MainWindow`. AGENTS.md asks for user-action warnings to be logged through NLog.
- `TombLib/TombLib.WPF/WPFInitializer.cs`: `ApplyIconInversion` is a single-line body and can be an expression-bodied member.
- `TombLib/TombLib.Icons/IconSources.cs` around line 83: the XML doc on a private method still mentions the removed pixel shader. Drop the history.
- `TombLib/TombLib.Icons/IconSources.cs` around line 97: explicit array construction where AGENTS.md prefers collection expressions. Check the result before changing it.
- `TombEditor/MainWindow.xaml.cs` around line 66: `System.Windows.Threading.DispatcherTimer` is fully qualified even though the namespace is imported. Use `DispatcherTimer`.

## Pre-existing style issues (not from this change)

- UTF-8 BOMs and tab indentation remain in `DarkUI/DarkUI.WPF/ClassicRoomEdit/Button.xaml`, `ToggleButton.xaml`, `DarkUI/DarkUI.WPF/Styles/Icon.xaml`, `DarkUI/DarkUI.WPF/Defaults.cs` and `DarkUI/DarkUI.WPF.Demo/App.xaml.cs`. They sit on untouched lines, so reformatting them would bury the real changes. Clean them up in a separate commit.

## Design notes

- The caption height is the AvalonDock default, about 20 px. The WinForms reference measured 25 px, so headers are shorter than the original.
- Icons that are already on screen keep their old inversion if the theme changes at runtime. The editor only applies the theme at startup, so nothing visible changes today.

## Not verified yet

- Launching with a saved layout can show a 160x28 window. The default layout was the only layout tested.
- Dock minimums while dragging a splitter.
- Visuals of popped-out floating windows. The capture tool only captures the main window.
- Switching modes while a document is floating, and re-docking a floating view by dragging its tab.

## Repo hygiene

- `.codebase-memory/` is an untracked tool folder. Do not commit it. Consider adding it to `.gitignore`.
