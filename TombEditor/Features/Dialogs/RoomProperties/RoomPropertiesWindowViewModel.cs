#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.RoomProperties;

public partial class RoomPropertiesWindowViewModel : ObservableObject, IModalDialogViewModel, IDisposable
{
    public partial class PropertyRow : ObservableObject
    {
        [ObservableProperty] private bool _replace;
        public string Name { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
    }

    private readonly Editor _editor;
    private readonly ILocalizationService _localizationService;

    public ObservableCollection<PropertyRow> Rows { get; } = new();

    [ObservableProperty] private bool? _dialogResult;

    public RoomPropertiesWindowViewModel(
        Editor? editor = null,
        ILocalizationService? localizationService = null)
    {
        _editor = editor ?? Editor.Instance;
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        // Collect every property of RoomProperties exactly once (by DisplayName).
        var seen = new HashSet<string>();
        foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(typeof(TombLib.LevelData.RoomProperties)))
        {
            if (!seen.Add(p.DisplayName))
                continue;

            var row = new PropertyRow { Name = p.Name, DisplayName = p.DisplayName };
            row.PropertyChanged += OnRowChanged;
            Rows.Add(row);
        }

        _editor.EditorEventRaised += OnEditorEvent;
        Refresh();
    }

    private void OnRowChanged(object? sender, PropertyChangedEventArgs e) => Refresh();

    private void OnEditorEvent(IEditorEvent obj)
    {
        if (obj is Editor.SelectedRoomChangedEvent
            || obj is Editor.SelectedRoomsChangedEvent
            || obj is Editor.RoomListChangedEvent
            || obj is Editor.LevelChangedEvent)
        {
            Refresh();
        }
    }

    private void Refresh()
        => ApplyCommand.NotifyCanExecuteChanged();

    private bool CanApply
        => Rows.Any(r => r.Replace) && _editor.SelectedRooms.Count > 1;

    [RelayCommand(CanExecute = nameof(CanApply))]
    private void Apply()
    {
        var undoList = new List<UndoRedoInstance>();
        var propInfo = typeof(TombLib.LevelData.RoomProperties).GetProperties();
        var curr = _editor.SelectedRoom;

        foreach (var r in _editor.SelectedRooms.Skip(1))
        {
            undoList.Add(new RoomPropertyUndoInstance(_editor.UndoManager, r));

            // Clone so reference-type properties (e.g. room tags) don't get aliased across rooms.
            var newProp = curr.Properties.Clone();

            foreach (var row in Rows)
            {
                if (!row.Replace)
                    continue;

                foreach (var prop in propInfo)
                {
                    var attrib = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault();
                    if (attrib is DisplayNameAttribute dna && dna.DisplayName == row.DisplayName)
                    {
                        prop.SetValue(r.Properties, prop.GetValue(newProp));

                        // HACK: rebuild lighting when AmbientLight changes.
                        if (prop.Name == nameof(r.Properties.AmbientLight))
                            _editor.UpdateRoomLighting(r);
                    }
                }
            }

            _editor.RoomPropertiesChange(r);
        }

        _editor.UndoManager.Push(undoList);
        _editor.SendMessage(_localizationService["PropertiesAppliedMessage"], PopupType.Info, true);
    }

    [RelayCommand]
    private void Close() => DialogResult = false;

    public void Dispose()
    {
        foreach (var row in Rows)
            row.PropertyChanged -= OnRowChanged;
        _editor.EditorEventRaised -= OnEditorEvent;
    }
}
