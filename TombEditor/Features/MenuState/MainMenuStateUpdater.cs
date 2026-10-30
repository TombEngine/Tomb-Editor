#nullable enable

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TombLib.LevelData;

namespace TombEditor.Features.MenuState;

/// <summary>
/// Keeps the enabled and visible state of the main menu in sync with the editor, mirroring the rules
/// the previous menu strip applied from its editor event handler.
/// </summary>
public sealed class MainMenuStateUpdater
{
    private static readonly string[] SectorCommands =
    [
        "SmoothRandomCeilingDown", "SmoothRandomCeilingUp", "SmoothRandomFloorDown", "SmoothRandomFloorUp",
        "SmoothFloor", "SmoothCeiling", "SharpRandomCeilingDown", "SharpRandomCeilingUp", "SharpRandomFloorDown",
        "SharpRandomFloorUp", "RealignFloorToStepHeight", "RealignCeilingToStepHeight", "ConvertFloorToQuads",
        "ConvertCeilingToQuads", "AverageCeiling", "AverageFloor", "GridWallsIn3", "GridWallsIn5",
        "GridWallsIn3Squares", "GridWallsIn5Squares"
    ];

    private static readonly string[] TextureCommands = ["RemapTexture", "RemoveTextures", "UnloadTextures", "ReloadTextures", "ConvertTexturesToPNG"];
    private static readonly string[] WadCommands = ["RemoveWads", "ReloadWads"];

    private readonly Editor _editor;
    private readonly Dictionary<string, List<MenuItem>> _items = [];
    private readonly Separator? _stepHeightSeparator;

    public MainMenuStateUpdater(Editor editor, ItemsControl menu, Separator? stepHeightSeparator)
    {
        _editor = editor;
        _stepHeightSeparator = stepHeightSeparator;
        Collect(menu);
        UpdateAll();
    }

    /// <summary>Updates the menu for an editor event.</summary>
    public void Handle(IEditorEvent obj)
    {
        if (obj is Editor.SelectedObjectChangedEvent or Editor.ModeChangedEvent or Editor.SelectedSectorsChangedEvent)
            UpdateSelectionState();

        if (obj is Editor.InitEvent or Editor.GameVersionChangedEvent or Editor.LevelChangedEvent or Editor.ConfigurationChangedEvent)
            UpdateVersionVisibility();

        if (obj is Editor.UndoStackChangedEvent undo)
        {
            SetEnabled(undo.UndoPossible, "Undo");
            SetEnabled(undo.RedoPossible, "Redo");
        }

        if (obj is Editor.SelectedSectorsChangedEvent)
            SetEnabled(_editor.SelectedSectors.Valid, SectorCommands);

        if (obj is Editor.HasUnsavedChangesChangedEvent or Editor.InitEvent)
            SetEnabled(_editor.HasUnsavedChanges, "SaveLevel");

        if (obj is Editor.LoadedTexturesChangedEvent or Editor.LevelChangedEvent)
            SetEnabled(_editor.Level.Settings.Textures.Count > 0, TextureCommands);

        if (obj is Editor.LoadedWadsChangedEvent or Editor.LevelChangedEvent)
            SetEnabled(_editor.Level.Settings.Wads.Count > 0, WadCommands);

        if (obj is Editor.LoadedSoundsCatalogsChangedEvent or Editor.LevelChangedEvent)
            SetEnabled(_editor.Level.Settings.SoundCatalogs.Count > 0, "ReloadSounds");

        if (obj is Editor.BookmarkedObjectChanged bookmark)
            SetEnabled(bookmark.Current is not null, "SelectBookmarkedObject");
    }

    /// <summary>Paste availability depends on the clipboard content, so it is refreshed when the Edit menu opens.</summary>
    public void UpdatePaste()
    {
        SetEnabled(ClipboardHelper.CanPaste(_editor.Mode == EditorMode.Map2D, includeSectors: true), "Paste");
    }

    private void UpdateAll()
    {
        UpdateSelectionState();
        UpdateVersionVisibility();
        UpdatePaste();

        SetEnabled(false, "Undo", "Redo");
        SetEnabled(_editor.SelectedSectors.Valid, SectorCommands);
        SetEnabled(_editor.HasUnsavedChanges, "SaveLevel");
        SetEnabled(_editor.Level.Settings.Textures.Count > 0, TextureCommands);
        SetEnabled(_editor.Level.Settings.Wads.Count > 0, WadCommands);
        SetEnabled(_editor.Level.Settings.SoundCatalogs.Count > 0, "ReloadSounds");
        SetEnabled(_editor.BookmarkedObject is not null, "SelectBookmarkedObject");
    }

    private void UpdateSelectionState()
    {
        var selected = _editor.SelectedObject;
        bool map2D = _editor.Mode == EditorMode.Map2D;
        bool cutCopy = map2D || selected is PositionBasedObjectInstance || _editor.SelectedSectors.Valid;

        SetEnabled(cutCopy, "Copy", "Cut");
        SetEnabled(map2D || selected is not null, "Delete");
        SetEnabled(selected is PositionBasedObjectInstance, "StampObject", "SelectFloorBelowObject");
        SetEnabled(selected is not null, "BookmarkObject", "EditObject");
        SetEnabled(selected is SectorBasedObjectInstance && _editor.SelectedSectors.Valid, "SplitSectorObjectOnSelection");
        UpdatePaste();
    }

    private void UpdateVersionVisibility()
    {
        var level = _editor.Level;
        bool tombEngine = level.IsTombEngine;
        bool stepHeight = tombEngine || _editor.Configuration.Editor_EnableStepHeightControlsForUnsupportedEngines;

        SetVisible(level.Settings.GameVersion.Native() <= TRVersion.Game.TR2, "AddSprite");
        SetVisible(level.IsNG, "MakeQuickItemGroup");
        SetVisible(tombEngine, "AddBoxVolume", "AddSphereVolume", "EditVolumeEventSets", "EditGlobalEventSets");
        SetVisible(tombEngine || level.IsTRX, "GenerateObjectNames");
        SetVisible(stepHeight, "IncreaseStepHeight", "DecreaseStepHeight");

        if (_stepHeightSeparator is not null)
            _stepHeightSeparator.Visibility = stepHeight ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Collect(ItemsControl parent)
    {
        foreach (var item in parent.Items)
        {
            if (item is not MenuItem menuItem)
                continue;

            if (EditorMenu.GetCommand(menuItem) is { Length: > 0 } command)
            {
                if (!_items.TryGetValue(command, out var list))
                    _items[command] = list = [];

                list.Add(menuItem);
            }

            Collect(menuItem);
        }
    }

    /// <summary>Shows the check mark of a toggle entry.</summary>
    public void SetChecked(string command, bool isChecked)
    {
        if (!_items.TryGetValue(command, out var list))
            return;

        foreach (var item in list)
        {
            item.IsCheckable = true;
            item.IsChecked = isChecked;
        }
    }

    private void SetEnabled(bool enabled, params string[] commands)
    {
        foreach (var command in commands)
        {
            if (!_items.TryGetValue(command, out var list))
                continue;

            foreach (var item in list)
                item.IsEnabled = enabled;
        }
    }

    private void SetVisible(bool visible, params string[] commands)
    {
        foreach (var command in commands)
        {
            if (!_items.TryGetValue(command, out var list))
                continue;

            foreach (var item in list)
                item.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}