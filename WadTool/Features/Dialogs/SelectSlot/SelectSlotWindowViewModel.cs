#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLib.LevelData;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace WadTool.Features.Dialogs.SelectSlot;

public sealed record SlotEntry(uint Id, string DisplayName);

public partial class SelectSlotWindowViewModel : ObservableObject, IModalDialogViewModel
{
    /// <summary>All catalog slots eligible for this wad (free, allowed and not hidden), before search filtering.</summary>
    private readonly List<KeyValuePair<uint, string>> _slotSuggestions;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private uint _chosenId;
    [ObservableProperty] private SlotEntry? _selectedSlot;

    public Type TypeClass { get; }
    public TRVersion.Game GameVersion { get; }
    public IWadObjectId NewId { get; private set; }

    public ObservableCollection<SlotEntry> Slots { get; } = new();

    public SelectSlotWindowViewModel(
        Wad2 wad,
        IWadObjectId currentId,
        List<uint>? additionalObjectsToHide = null,
        IEnumerable<uint>? allowedObjectIds = null,
        ILocalizationService? localizationService = null)
    {
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        NewId = currentId;
        TypeClass = currentId.GetType();
        GameVersion = wad.GameVersion.Native();

        HashSet<uint>? allowed = allowedObjectIds is null ? null : new HashSet<uint>(allowedObjectIds);

        bool isVisible(uint id)
            => (allowed is null || allowed.Contains(id))
            && !(additionalObjectsToHide?.Any(add => add == id) ?? false);

        if (TypeClass == typeof(WadMoveableId))
        {
            _chosenId = ((WadMoveableId)currentId).TypeId;
            _slotSuggestions = TrCatalog.GetAllMoveables(GameVersion)
                .Where(item => isVisible(item.Key) && !wad.Moveables.Any(moveable => moveable.Key.TypeId == item.Key))
                .ToList();
        }
        else if (TypeClass == typeof(WadStaticId))
        {
            _chosenId = ((WadStaticId)currentId).TypeId;
            _slotSuggestions = TrCatalog.GetAllStatics(GameVersion)
                .Where(item => isVisible(item.Key) && !wad.Statics.Any(stat => stat.Key.TypeId == item.Key))
                .ToList();
        }
        else if (TypeClass == typeof(WadSpriteSequenceId))
        {
            _chosenId = ((WadSpriteSequenceId)currentId).TypeId;
            _slotSuggestions = TrCatalog.GetAllSpriteSequences(GameVersion)
                .Where(item => isVisible(item.Key) && !wad.SpriteSequences.Any(sprite => sprite.Key.TypeId == item.Key))
                .ToList();
        }
        else
        {
            throw new NotImplementedException("The " + TypeClass + " is not implemented yet.");
        }

        ReloadSlots();

        // Like the legacy form: preselect the first free slot, if any.
        if (Slots.Count > 0)
            ChosenId = Slots[0].Id;

        SyncSelectionToChosenId();
    }

    /// <summary>Rebuilds the visible slot list, applying the current search keyword.</summary>
    private void ReloadSlots()
    {
        Slots.Clear();

        foreach (KeyValuePair<uint, string> suggestion in _slotSuggestions)
        {
            if (!string.IsNullOrEmpty(SearchText)
                && suggestion.Value.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) == -1)
                continue;

            Slots.Add(new SlotEntry(suggestion.Key, "(" + suggestion.Key + ") " + suggestion.Value));
        }
    }

    private void SyncSelectionToChosenId()
        => SelectedSlot = Slots.FirstOrDefault(slot => slot.Id == ChosenId);

    partial void OnSearchTextChanged(string value) => ReloadSlots();

    partial void OnChosenIdChanged(uint value) => SyncSelectionToChosenId();

    partial void OnSelectedSlotChanged(SlotEntry? value)
    {
        if (value is not null)
            ChosenId = value.Id;
    }

    [RelayCommand]
    private void Confirm()
    {
        uint newId = Slots.Count == 0 || SelectedSlot is null ? ChosenId : SelectedSlot.Id;

        if (TypeClass == typeof(WadMoveableId))
            NewId = new WadMoveableId(newId);
        else if (TypeClass == typeof(WadStaticId))
            NewId = new WadStaticId(newId);
        else if (TypeClass == typeof(WadSpriteSequenceId))
            NewId = new WadSpriteSequenceId(newId);
        else
            throw new NotImplementedException("The " + TypeClass + " is not implemented yet.");

        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel() => DialogResult = false;
}
