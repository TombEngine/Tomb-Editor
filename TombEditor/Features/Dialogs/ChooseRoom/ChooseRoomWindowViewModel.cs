#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TombLib.LevelData;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.ChooseRoom;

public partial class ChooseRoomWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly Action<Room>? _roomSelectionChanged;

    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private Room? _selectedRoom;

    public string Prompt { get; }

    public ObservableCollection<Room> Rooms { get; }

    public ChooseRoomWindowViewModel(
        string prompt,
        IEnumerable<Room> rooms,
        Action<Room>? roomSelectionChanged = null,
        ILocalizationService? localizationService = null)
    {
        Prompt = prompt;
        Rooms = new ObservableCollection<Room>(rooms);
        _roomSelectionChanged = roomSelectionChanged;
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);
    }

    partial void OnSelectedRoomChanged(Room? value)
    {
        if (value is not null)
            _roomSelectionChanged?.Invoke(value);
    }

    private bool CanConfirm() => SelectedRoom is not null;

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm() => DialogResult = true;

    [RelayCommand]
    private void Cancel() => DialogResult = false;
}
