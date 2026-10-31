#nullable enable

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TombLib.Icons;
using TombLib.LevelData;
using static TombLib.LevelData.TRVersion;

namespace TombEditor.Features.Toolbar;

/// <summary>
/// Enabled-state rules of toolbar buttons that depend on the editor state, mirroring the rules the
/// previous toolbar applied from its editor event handler.
/// </summary>
internal static class ToolbarEnableRules
{
    /// <summary>Raised when the clipboard content changed so paste buttons can re-evaluate.</summary>
    public static event Action? ClipboardChanged;

    public static void NotifyClipboardChanged() => ClipboardChanged?.Invoke();

    /// <summary>Applies the rule of <paramref name="command"/> to the button now and whenever the editor state changes.</summary>
    public static void Attach(ButtonBase button, string command)
    {
        if (!HasRule(command))
            return;

        Apply(button, command, null);
        EditorToolButton.SubscribeWeak(button, editorEvent => Apply(button, command, editorEvent));

        if (command == "Paste")
        {
            void OnClipboardChanged() => Apply(button, command, null);
            ClipboardChanged += OnClipboardChanged;
            button.Unloaded += (_, _) => ClipboardChanged -= OnClipboardChanged;
        }
    }

    private static bool HasRule(string command) => command is "Undo" or "Redo" or "Copy" or "StampObject" or "Paste"
        or "ResetCamera" or "ToggleFlipMap" or "AddBoxVolume" or "AddSphereVolume" or "AddSprite";

    private static void Apply(ButtonBase button, string command, IEditorEvent? editorEvent)
    {
        var editor = Editor.Instance;

        switch (command)
        {
            case "Undo" when editorEvent is Editor.UndoStackChangedEvent undo:
                button.IsEnabled = undo.UndoPossible;
                if (button.Content is Image image)
                    image.Source = IconSources.Load(undo.UndoPossible && !undo.UndoReversible ? "General/undo_irreversible" : "General/undo");
                break;

            case "Redo" when editorEvent is Editor.UndoStackChangedEvent redo:
                button.IsEnabled = redo.RedoPossible;
                break;

            case "Undo" or "Redo":
                // The undo stack is empty until the first change.
                if (editorEvent is null)
                    button.IsEnabled = false;
                break;

            case "Copy" or "StampObject":
                button.IsEnabled = editor.SelectedObject is ISpatial;
                break;

            case "Paste":
                button.IsEnabled = editor.Mode != EditorMode.Map2D && ClipboardHelper.CanPaste(roomMode: false, includeSectors: false);
                break;

            case "ResetCamera":
                button.IsEnabled = editor.Mode != EditorMode.Map2D;
                break;

            case "ToggleFlipMap":
                button.IsEnabled = editor.SelectedRoom?.Alternated ?? false;
                if (button is ToggleButton flipMap)
                    flipMap.IsChecked = editor.SelectedRoom?.AlternateBaseRoom is not null;
                break;

            case "AddBoxVolume" or "AddSphereVolume":
                button.IsEnabled = editor.Level?.IsTombEngine ?? false;
                break;

            case "AddSprite":
                button.IsEnabled = editor.Level is { } level && level.Settings.GameVersion.Native() <= Game.TR2;
                break;
        }
    }
}