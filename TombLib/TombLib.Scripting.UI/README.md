# TombLib.Scripting.UI

Purpose: shared editor UI, AvalonEdit integration, and host-facing scripting behavior.

Use this project for:
- the thin shared editor host and shared presentation infrastructure
- completion, tooltip, navigation, diagnostics, bookmark, and settings UI behavior
- extracted editor services/coordinators that still require UI types

Do not place here:
- source-of-truth language definitions or parser-owned specification data
- host-specific docking, menu, or tool-window orchestration that belongs in TombIDE
- TombIDE-only WinForms editors or helpers such as the classic-script strings editor island

Folder direction:
- prefer feature-based slices such as Completion, Highlighting, Cleaning, Presentation, and Services
- keep language-specific presentation here only when the behavior is truly shared with the editor shell