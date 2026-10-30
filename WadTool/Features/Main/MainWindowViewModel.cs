#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Wad;

namespace WadTool.Features.Main;

/// <summary>Exposes shell commands, their availability and status independently of WPF controls.</summary>
public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly WadToolClass _tool;
    private bool _hasUnsavedChanges;
    private bool _saveAsLatched;

    [ObservableProperty] private string _title = "WadTool";
    [ObservableProperty] private string _statistics = string.Empty;
    [ObservableProperty] private string _sourceHeader = "Source";
    [ObservableProperty] private bool _hasDestinationSelection;
    [ObservableProperty] private bool _hasSourceSelection;

    /// <summary>Gets shell actions shared by menus, toolbar buttons and shortcuts.</summary>
    public IReadOnlyDictionary<string, IRelayCommand> Commands { get; }

    /// <summary>Gets whether the destination has changes that can be saved.</summary>
    public bool CanSave => _tool.DestinationWad is { } wad && (_hasUnsavedChanges || string.IsNullOrEmpty(wad.FileName));

    /// <summary>
    /// Gets whether a destination can be saved under another filename. Like the original form, it becomes
    /// available with the first change (or for new and imported wads) and stays available while a destination exists.
    /// </summary>
    public bool CanSaveAs => _tool.DestinationWad is not null && _saveAsLatched;

    private bool HasDestination => _tool.DestinationWad is not null;

    /// <summary>Creates shell bindings from application actions and observes editor state.</summary>
    public MainWindowViewModel(WadToolClass tool, IEnumerable<KeyValuePair<string, Action>> actions)
    {
        _tool = tool;
        Commands = actions.ToDictionary(pair => pair.Key, pair => (IRelayCommand)new RelayCommand(pair.Value, () => CanExecute(pair.Key)));
        _tool.EditorEventRaised += OnEditorEvent;
        UpdateSaveAsLatch();
    }

    /// <summary>Executes an available shell command.</summary>
    public bool TryExecute(string name)
    {
        if (!Commands.TryGetValue(name, out var command) || !command.CanExecute(null))
            return false;

        command.Execute(null);
        return true;
    }

    private bool CanExecute(string name) => name switch
    {
        "SaveMenu_Click" => CanSave,
        "SaveAsMenu_Click" => CanSaveAs,
        "CloseReferenceLevelMenu_Click" => _tool.ReferenceLevel is not null,
        "ConsolidateTexturesMenu_Click" or "ConvertToDynamicLightingMenu_Click" or "ConvertToStaticLightingMenu_Click" or
            "ConvertToTiledMenu_Click" or "ConvertToUVMappedMenu_Click" => HasDestination && HasDestinationSelection,
        "EditSkeletonButton_Click" => _tool.MainSelection?.Id is WadMoveableId,
        "AnimatedTexturesMenu_Click" or "ConvertWadToTombEngineMenu_Click" or "DebugMeshEditorMenu_Click" or
            "ItemPropertiesMenu_Click" or "MeshEditorMenu_Click" or "NewMoveableMenu_Click" or
            "NewSpriteSequenceMenu_Click" or "NewStaticMenu_Click" => HasDestination,
        _ => true
    };

    private void OnEditorEvent(IEditorEvent editorEvent)
    {
        if (editorEvent is not (IWadChangedEvent or WadToolClass.ReferenceLevelChangedEvent or WadToolClass.UnsavedChangesEvent))
            return;

        if (editorEvent is WadToolClass.UnsavedChangesEvent changes)
            _hasUnsavedChanges = changes.UnsavedChanges;

        UpdateSaveAsLatch();
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(CanSaveAs));
        RefreshCommands();
    }

    private void UpdateSaveAsLatch()
    {
        if (_tool.DestinationWad is not { } wad)
            _saveAsLatched = false;
        else if (string.IsNullOrEmpty(wad.FileName) || _hasUnsavedChanges)
            _saveAsLatched = true;
    }

    partial void OnHasDestinationSelectionChanged(bool value) => RefreshCommands();
    partial void OnHasSourceSelectionChanged(bool value) => RefreshCommands();

    private void RefreshCommands()
    {
        foreach (var command in Commands.Values)
            command.NotifyCanExecuteChanged();
    }

    /// <summary>Stops observing editor events when the shell closes.</summary>
    public void Dispose() => _tool.EditorEventRaised -= OnEditorEvent;
}
