#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.Search;

public partial class SearchWindowViewModel : ObservableObject, IModalDialogViewModel, IDisposable
{
    public enum ScopeMode
    {
        Everything,
        Rooms,
        AllObjects,
        ObjectsInSelectedRooms,
        ItemTypes,
        Triggers
    }

    public sealed record ScopeItem(ScopeMode Mode, string DisplayName);

    public sealed class SearchResultRow
    {
        public object Target { get; init; } = null!;
        public string Name { get; init; } = string.Empty;
        public string Room { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public int Rate { get; init; }
    }

    private readonly Editor _editor;
    private bool _suppressNavigation;
    private readonly IMessageService _messageService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private ScopeMode _scope = ScopeMode.Everything;
    [ObservableProperty] private string _keyword = string.Empty;
    [ObservableProperty]
    private SearchResultRow? _selectedRow;

    public ObservableCollection<SearchResultRow> Rows { get; } = new();

    public IReadOnlyList<ScopeItem> Scopes { get; }

    public SearchWindowViewModel(
        Editor editor,
        IMessageService? messageService = null,
        ILocalizationService? localizationService = null)
    {
        _editor = editor;
        _messageService = ServiceLocator.ResolveService(messageService);
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        Scopes = new List<ScopeItem>
        {
            new(ScopeMode.Everything, _localizationService["ScopeEverything"]),
            new(ScopeMode.Rooms, _localizationService["ScopeRooms"]),
            new(ScopeMode.AllObjects, _localizationService["ScopeAllObjects"]),
            new(ScopeMode.ObjectsInSelectedRooms, _localizationService["ScopeObjectsInSelectedRooms"]),
            new(ScopeMode.ItemTypes, _localizationService["ScopeItemTypes"]),
            new(ScopeMode.Triggers, _localizationService["ScopeTriggers"])
        };

        _editor.EditorEventRaised += OnEditorEvent;
        Rebuild();
    }

    partial void OnScopeChanged(ScopeMode value) => Rebuild();
    partial void OnKeywordChanged(string value) => Rebuild();

    partial void OnSelectedRowChanged(SearchResultRow? value)
    {
        if (value is null || _suppressNavigation)
            return;

        switch (value.Target)
        {
            case Room r:
                _editor.SelectedRoom = r;
                break;
            case ObjectInstance oi:
                _editor.ShowObject(oi);
                break;
            case ItemType itemType:
                var wadObj = itemType.ToIWadObject(_editor.Level.Settings);
                if (wadObj is not null)
                    _editor.ChosenItems = new[] { wadObj };
                break;
        }
    }

    private void OnEditorEvent(IEditorEvent obj)
    {
        switch (obj)
        {
            case Editor.LoadedWadsChangedEvent when Scope is ScopeMode.ItemTypes or ScopeMode.Everything:
            case Editor.RoomListChangedEvent:
            case Editor.SelectedRoomChangedEvent when Scope == ScopeMode.ObjectsInSelectedRooms:
                Rebuild();
                break;
            case IEditorObjectChangedEvent objectEvent when IsObjectEventRelevant(objectEvent):
                ApplyObjectEvent(objectEvent);
                break;
        }
    }

    private bool IsObjectEventRelevant(IEditorObjectChangedEvent objectEvent) => Scope switch
    {
        ScopeMode.AllObjects or ScopeMode.Everything => true,
        ScopeMode.ObjectsInSelectedRooms => _editor.SelectedRooms.Contains(objectEvent.Room),
        ScopeMode.Triggers => objectEvent.Object is TriggerInstance,
        _ => false
    };

    private void ApplyObjectEvent(IEditorObjectChangedEvent objectEvent)
    {
        switch (objectEvent.ChangeType)
        {
            case ObjectChangeType.Add:
                AddObject(objectEvent.Object);
                break;
            case ObjectChangeType.Change:
                RemoveObject(objectEvent.Object);
                AddObject(objectEvent.Object);
                break;
            case ObjectChangeType.Remove:
                RemoveObject(objectEvent.Object);
                break;
        }
    }

    private void AddObject(object target)
    {
        var row = CreateRow(target, Keyword.ToLower());
        int index = 0;

        while (index < Rows.Count && Rows[index].Rate <= row.Rate)
            index++;

        RunWithoutNavigation(() =>
        {
            var selected = SelectedRow;
            Rows.Insert(index, row);
            SelectedRow = selected;
        });
    }

    private void RemoveObject(object target)
    {
        RunWithoutNavigation(() =>
        {
            var selected = SelectedRow;

            // Rows of objects that were removed together with their room are dropped as well.
            foreach (var row in Rows.Where(r => ReferenceEquals(r.Target, target) || r.Target is ObjectInstance { Room: null }).ToList())
            {
                Rows.Remove(row);

                if (ReferenceEquals(row, selected))
                    selected = null;
            }

            SelectedRow = selected;
        });
    }

    private void RunWithoutNavigation(Action action)
    {
        bool previous = _suppressNavigation;
        _suppressNavigation = true;

        try
        {
            action();
        }
        finally
        {
            _suppressNavigation = previous;
        }
    }

    private void Rebuild() => RunWithoutNavigation(() =>
    {
        var previousTarget = SelectedRow?.Target;
        string lowerKeyword = Keyword.ToLower();

        Rows.Clear();

        foreach (var row in GetRelevant(Scope).Select(target => CreateRow(target, lowerKeyword)).OrderBy(r => r.Rate))
            Rows.Add(row);

        SelectedRow = previousTarget is null ? null : Rows.FirstOrDefault(r => ReferenceEquals(r.Target, previousTarget));
    });

    private SearchResultRow CreateRow(object target, string lowerKeyword)
    {
        int levenshtein = Levenshtein.DistanceSubstring(target.ToString()!.ToLower(), lowerKeyword, out int startIndex);
        int rate = Math.Min(levenshtein * 256 + Math.Min(startIndex, 255), 1 << 16);

        var room = GetRoom(target);
        string roomLabel = room is null
            ? _localizationService["UnknownRoom"]
            : _editor.Level.Rooms.ReferenceIndexOf(room) + ":   " + room.Name;

        string name = target switch
        {
            Room r when r.Properties.Tags.Count > 0 => r.Name + " / " + string.Join(" ", r.Properties.Tags) + " /",
            Room r => r.Name,
            _ => target.ToString()!
        };

        return new SearchResultRow
        {
            Target = target,
            Name = name,
            Room = roomLabel,
            Type = FormatType(target),
            Rate = rate
        };
    }

    private IEnumerable<object> GetRelevant(ScopeMode scope)
    {
        if (scope is ScopeMode.Everything or ScopeMode.Rooms)
            foreach (var room in _editor.Level.ExistingRooms)
                yield return room;

        if (scope is ScopeMode.Everything or ScopeMode.AllObjects)
        {
            foreach (var room in _editor.Level.ExistingRooms)
                foreach (var instance in room.AnyObjects)
                    yield return instance;
        }
        else if (scope == ScopeMode.ObjectsInSelectedRooms)
        {
            foreach (var instance in _editor.SelectedRooms.SelectMany(r => r.AnyObjects))
                yield return instance;
        }

        if (scope is ScopeMode.Everything or ScopeMode.ItemTypes)
        {
            foreach (var obj in _editor.Level.Settings.WadGetAllMoveables().Values)
                yield return new ItemType(obj.Id, _editor.Level.Settings);
            foreach (var obj in _editor.Level.Settings.WadGetAllStatics().Values)
                yield return new ItemType(obj.Id, _editor.Level.Settings);
        }

        if (scope == ScopeMode.Triggers)
        {
            foreach (var room in _editor.Level.ExistingRooms)
                foreach (var trigger in room.AnyObjects.OfType<TriggerInstance>())
                    yield return trigger;
        }
    }

    private static Room? GetRoom(object obj) => obj switch
    {
        Room r => r,
        ObjectInstance oi => oi.Room,
        _ => null
    };

    private static string FormatType(object obj)
    {
        string name = obj.GetType().Name;
        if (name.EndsWith("Instance"))
            name = name[..^"Instance".Length];

        // Insert a space before each Capital that follows a lowercase letter.
        bool lastWasUpper = true;
        for (int i = 0; i < name.Length; i++)
        {
            bool isUpper = char.IsUpper(name[i]);
            if (isUpper && !lastWasUpper)
                name = name.Insert(i++, " ");
            lastWasUpper = isUpper;
        }

        return name;
    }

    /// <summary>Gets or sets every row highlighted in the results (the list allows multi-selection).</summary>
    public IReadOnlyList<SearchResultRow> SelectedRows { get; set; } = [];

    [RelayCommand]
    private void DeleteSelected()
    {
        var rows = SelectedRows.Count > 0 ? SelectedRows : SelectedRow is null ? [] : [SelectedRow];

        var roomsToDelete = new List<Room>();
        var objectsToDelete = new List<ObjectInstance>();
        bool illegalOperation = false;

        foreach (var row in rows)
        {
            switch (row.Target)
            {
                case Room room:
                    roomsToDelete.Add(room);
                    break;
                case TriggerInstance or ISpatial:
                    objectsToDelete.Add((ObjectInstance)row.Target);
                    break;
                default:
                    illegalOperation = true;
                    break;
            }
        }

        if ((objectsToDelete.Count > 0 || roomsToDelete.Count > 0) &&
            _messageService.ShowConfirmation(_localizationService["DeleteConfirmationMessage"], _localizationService["DeleteConfirmationTitle"]))
        {
            // Triggers go first to avoid clashes with the objects they reference.
            EditorActions.DeleteObjects(objectsToDelete.OrderBy(o => o is not TriggerInstance).ToList(), null, false);
            EditorActions.DeleteRooms(roomsToDelete, null);
        }

        if (illegalOperation)
            _messageService.ShowInformation(_localizationService["MapOnlyDeleteMessage"]);
    }
    [RelayCommand]
    private void Close() => DialogResult = false;

    public void Dispose() => _editor.EditorEventRaised -= OnEditorEvent;
}
