# WPF docking: open items

Known gaps and cleanups left after the AvalonDock theme and popout work on Nickelony/NEW-WPF.

## Bugs

None known. Dock minimums are re-applied after every show or dock of a pane (ScheduleDockMinimums() in MainWindow.xaml.cs, logic in Features/DockableViews/DockMinimums.cs), and panes that are not measured yet drop their old minimum.

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
