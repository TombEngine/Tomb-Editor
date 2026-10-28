#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TombLib.LevelData;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.ObjectList;

public sealed class ObjectEntry
{
    public ObjectInstance Instance { get; }
    public string Text { get; private set; }

    public ObjectEntry(ObjectInstance instance)
    {
        Instance = instance;
        Text = instance.ToShortString();
    }

    public void RefreshText() => Text = Instance.ToShortString();
}

public partial class ObjectListViewModel : ObservableObject
{
    private readonly Editor _editor;
    private bool _suppressEditorSync;
    private bool _disposed;

    public ObservableCollection<ObjectEntry> Objects { get; } = new();

    private ObjectEntry? _selectedItem;
    public ObjectEntry? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (!SetProperty(ref _selectedItem, value))
                return;

            ((RelayCommand)EditObjectCommand).NotifyCanExecuteChanged();

            if (_suppressEditorSync)
                return;

            // Don't push back to Editor when a group is selected (the previous panel did the
            // same - group selection in the viewport must not collapse to a single instance).
            if (_editor.SelectedObject is ObjectGroup)
                return;

            if (value?.Instance is { } instance)
                _editor.SelectedObject = instance;
        }
    }

    public ICommand EditObjectCommand { get; }
    public IRelayCommand<IList<object>> DeleteObjectsCommand { get; }

    public ObjectListViewModel(Editor editor)
    {
        _editor = editor;
        _editor.EditorEventRaised += OnEditorEventRaised;

        EditObjectCommand = new RelayCommand(EditSelected, () => SelectedItem is not null);
        DeleteObjectsCommand = new RelayCommand<IList<object>>(DeleteSelection, items => items is { Count: > 0 });

        RebuildList();
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
        if (obj is Editor.SelectedRoomChangedEvent || obj is Editor.GameVersionChangedEvent)
        {
            RebuildList();
            SyncSelection();
            return;
        }

        if (obj is Editor.ObjectChangedEvent e)
        {
            if (e.Room != _editor.SelectedRoom)
                return;

            _suppressEditorSync = true;
            try
            {
                switch (e.ChangeType)
                {
                    case ObjectChangeType.Add:
                        Objects.Add(new ObjectEntry(e.Object));
                        break;

                    case ObjectChangeType.Remove:
                        var toRemove = Objects.FirstOrDefault(o => o.Instance == e.Object);
                        if (toRemove is not null)
                            Objects.Remove(toRemove);
                        break;

                    case ObjectChangeType.Change:
                        var toUpdate = Objects.FirstOrDefault(o => o.Instance == e.Object);
                        if (toUpdate is not null)
                        {
                            // ObservableCollection cannot detect in-place mutation; replace the entry
                            // so the bound TextBlock re-evaluates Text.
                            var index = Objects.IndexOf(toUpdate);
                            Objects[index] = new ObjectEntry(e.Object);
                        }
                        else
                        {
                            RebuildList();
                        }
                        break;
                }
            }
            finally
            {
                _suppressEditorSync = false;
            }
            return;
        }

        if (obj is Editor.SelectedObjectChangedEvent)
            SyncSelection();
    }

    private void RebuildList()
    {
        _suppressEditorSync = true;
        try
        {
            Objects.Clear();

            if (_editor.SelectedRoom is null)
                return;

            foreach (var o in _editor.SelectedRoom.Objects)
                Objects.Add(new ObjectEntry(o));

            foreach (var o in _editor.SelectedRoom.GhostBlocks)
                Objects.Add(new ObjectEntry(o));
        }
        finally
        {
            _suppressEditorSync = false;
        }
    }

    private void SyncSelection()
    {
        _suppressEditorSync = true;
        try
        {
            if (_editor.SelectedObject is (PositionBasedObjectInstance or GhostBlockInstance) &&
                _editor.SelectedObject.Room == _editor.SelectedRoom)
                SelectedItem = Objects.FirstOrDefault(o => o.Instance == _editor.SelectedObject);
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
        if (SelectedItem?.Instance is { } instance)
            EditorActions.EditObject(instance, WPFUtils.GetWindowOwner());
    }

    private void DeleteSelection(IList<object>? items)
    {
        if (items is null || items.Count == 0)
            return;

        var instances = items.OfType<ObjectEntry>().Select(o => o.Instance).ToList();
        if (instances.Count == 0)
            return;

        EditorActions.DeleteObjects(instances, WPFUtils.GetWindowOwner());
    }
}
