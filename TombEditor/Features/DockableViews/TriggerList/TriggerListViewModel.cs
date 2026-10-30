#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TombLib;
using TombLib.LevelData;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.TriggerList;

public sealed record TriggerEntry(TriggerInstance Trigger, string Text, bool IsSetupTrigger);

public partial class TriggerListViewModel : ObservableObject
{
    private readonly Editor _editor;
    private bool _suppressEditorSync;
    private bool _disposed;

    public ObservableCollection<TriggerEntry> Triggers { get; } = new();

    /// <summary>Gets or sets all triggers highlighted in the list (the list allows multi-selection).</summary>
    public IReadOnlyList<TriggerEntry> SelectedItems { get; set; } = [];

    private TriggerEntry? _selectedItem;
    public TriggerEntry? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (!SetProperty(ref _selectedItem, value))
                return;

            ((RelayCommand)EditTriggerCommand).NotifyCanExecuteChanged();
            ((RelayCommand)DeleteTriggersCommand).NotifyCanExecuteChanged();

            if (_suppressEditorSync)
                return;

            if (value?.Trigger is { } trigger)
                _editor.SelectedObject = trigger;
        }
    }

    private string _title = "Triggers";
    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public ICommand AddTriggerCommand { get; }
    public ICommand EditTriggerCommand { get; }
    public ICommand DeleteTriggersCommand { get; }

    public TriggerListViewModel(Editor editor)
    {
        _editor = editor;
        _editor.EditorEventRaised += OnEditorEventRaised;

        AddTriggerCommand = CommandHandler.GetCommand(
            "AddTrigger",
            () => new CommandArgs { Editor = _editor, Window = WPFUtils.GetWindowOwner() });

        EditTriggerCommand = new RelayCommand(EditSelected, () => SelectedItem is not null);
        DeleteTriggersCommand = new RelayCommand(DeleteSelected, () => SelectedItem is not null || SelectedItems.Count > 0);
    }

    public void Cleanup()
    {
        if (_disposed)
            return;
        _disposed = true;
        _editor.EditorEventRaised -= OnEditorEventRaised;
    }

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        if (obj is Editor.InitEvent || obj is Editor.LevelChangedEvent || obj is Editor.GameVersionChangedEvent)
            Title = _editor.Level?.IsTombEngine == true ? "Classic Triggers" : "Triggers";

        if (obj is Editor.SelectedSectorsChangedEvent
            || obj is Editor.SelectedRoomChangedEvent
            || obj is Editor.RoomSectorPropertiesChangedEvent)
        {
            RefreshList();
            SyncSelection();
        }
        else if (obj is Editor.SelectedObjectChangedEvent)
        {
            SyncSelection();
        }
        else if (obj is Editor.ObjectChangedEvent objectChanged)
        {
            if (objectChanged.Object is TriggerInstance && objectChanged.Object.Room == _editor.SelectedRoom)
            {
                RefreshList();
                SyncSelection();
            }
        }
    }

    private void RefreshList()
    {
        Triggers.Clear();

        if (_editor.Level is null || !_editor.SelectedSectors.Valid || _editor.SelectedRoom is null)
            return;

        var area = _editor.SelectedSectors.Area;
        var origin = new VectorInt2(-1);
        var collected = new List<TriggerInstance>();
        bool noSort = true;

        for (int x = area.X0; x <= area.X1; x++)
            for (int z = area.Y0; z <= area.Y1; z++)
                foreach (var trigger in _editor.SelectedRoom.GetSectorTry(x, z)?.Triggers ?? new List<TriggerInstance>())
                {
                    if (collected.Contains(trigger))
                        continue;

                    // First sector contributing triggers becomes the sort origin; anything from
                    // a different "first sector" means overlapping triggers - drop sorting then.
                    if (origin.X < 0) origin = new VectorInt2(x, origin.Y);
                    if (origin.Y < 0) origin = new VectorInt2(origin.X, z);
                    noSort = origin.X != x || origin.Y != z;
                    collected.Add(trigger);
                }

        if (collected.Count == 1)
            noSort = true;

        if (collected.Count == 0)
            return;

        if (!noSort)
            TriggerInstance.SortTriggerList(ref collected);

        for (int i = 0; i < collected.Count; i++)
            Triggers.Add(new TriggerEntry(collected[i], collected[i].ToShortString(), !noSort && i == 0));
    }

    private void SyncSelection()
    {
        _suppressEditorSync = true;
        try
        {
            if (_editor.SelectedObject is TriggerInstance trigger)
                SelectedItem = Triggers.FirstOrDefault(t => t.Trigger == trigger);
            else
                SelectedItem = null;
        }
        finally
        {
            _suppressEditorSync = false;
        }
    }

    private void EditSelected()
    {
        if (SelectedItem?.Trigger is { } trigger)
            EditorActions.EditObject(trigger, WPFUtils.GetWindowOwner());
    }

    private void DeleteSelected()
    {
        if (_editor.SelectedRoom is null)
            return;

        var triggers = SelectedItems.Count > 0 ? SelectedItems.Select(item => item.Trigger) : [SelectedItem?.Trigger];
        var toRemove = triggers.OfType<ObjectInstance>().ToList();
        if (toRemove.Count > 0)
            EditorActions.DeleteObjects(toRemove, WPFUtils.GetWindowOwner());
    }

    /// <summary>Re-evaluates the delete command after the list selection changed.</summary>
    public void NotifySelectionChanged() => ((RelayCommand)DeleteTriggersCommand).NotifyCanExecuteChanged();
}
