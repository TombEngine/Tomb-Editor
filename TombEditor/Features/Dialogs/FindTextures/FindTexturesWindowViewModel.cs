#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.FindTextures;

public sealed record TextureSearchTypeItem(TextureSearchType Type, string DisplayName);

public sealed record FindTextureResult(Room Room, VectorInt2 Position)
{
    public string RoomName => Room.Name;
    public string Coordinates => $"{Position.X}, {Position.Y}";
}

public partial class FindTexturesWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private const uint MaxEntries = 1000;

    private readonly Editor _editor;
    private readonly ILocalizationService _localizationService;
    private bool _suppressNavigation = true;
    private bool _disposed;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private TextureSearchTypeItem _selectedSearchType;
    [ObservableProperty] private bool _onlySelectedRooms = true;
    [ObservableProperty] private string _statusText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NavigateToCommand))]
    private FindTextureResult? _selectedResult;

    public IReadOnlyList<TextureSearchTypeItem> SearchTypes { get; }
    public ObservableCollection<FindTextureResult> Results { get; } = new();

    public FindTexturesWindowViewModel(
        Editor? editor = null,
        ILocalizationService? localizationService = null)
    {
        _editor = editor ?? Editor.Instance;
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        SearchTypes = new List<TextureSearchTypeItem>
        {
            new(TextureSearchType.Empty, _localizationService["TypeEmpty"]),
            new(TextureSearchType.Invisible, _localizationService["TypeInvisible"]),
            new(TextureSearchType.Broken, _localizationService["TypeBroken"]),
            new(TextureSearchType.ExactMatch, _localizationService["TypeExactMatch"]),
            new(TextureSearchType.PartialMatch, _localizationService["TypePartialMatch"]),
            new(TextureSearchType.TextureSet, _localizationService["TypeTextureSet"])
        };

        _selectedSearchType = SearchTypes[0];

        _editor.EditorEventRaised += OnEditorEventRaised;
    }

    [RelayCommand]
    private void Close() => DialogResult = false;

    public void Cleanup()
    {
        if (_disposed)
            return;

        _disposed = true;
        _editor.EditorEventRaised -= OnEditorEventRaised;
    }

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        if (obj is Editor.LevelChangedEvent)
        {
            RunSearch();
            return;
        }

        if (obj is Editor.RoomListChangedEvent)
        {
            // Prune entries pointing at rooms that no longer exist.
            var alive = Results.Where(r => _editor.Level.Rooms.Contains(r.Room)).ToList();
            Results.Clear();
            foreach (FindTextureResult result in alive)
                Results.Add(result);
        }
    }

    [RelayCommand]
    private void RunSearch()
    {
        TextureArea canonical = _editor.SelectedTexture.GetCanonicalTexture(_editor.SelectedTexture.TextureIsTriangle);

        List<KeyValuePair<Room, VectorInt2>> hits = EditorActions.FindTextures(
            SelectedSearchType.Type,
            canonical,
            OnlySelectedRooms,
            MaxEntries,
            out bool tooMany);

        _suppressNavigation = true;
        try
        {
            Results.Clear();
            foreach (var entry in hits)
                Results.Add(new FindTextureResult(entry.Key, entry.Value));
            SelectedResult = null;
        }
        finally
        {
            _suppressNavigation = false;
        }

        if (tooMany)
            StatusText = _localizationService.Format("StatusTooMany", hits.Count);
        else if (hits.Count == 0)
            StatusText = _localizationService["StatusNoEntries"];
        else
            StatusText = _localizationService.Format("StatusFound", hits.Count);
    }

    private bool CanNavigateTo() => SelectedResult is not null;

    [RelayCommand(CanExecute = nameof(CanNavigateTo))]
    private void NavigateTo()
    {
        if (_suppressNavigation || SelectedResult is null)
            return;

        var entry = SelectedResult;

        if (_editor.Level.Rooms.Contains(entry.Room))
            _editor.SelectRoom(entry.Room);

        if (_editor.SelectedRoom is not null && !_editor.SelectedRoom.CoordinateInvalid(entry.Position))
            _editor.SelectedSectors = new SectorSelection { Start = entry.Position, End = entry.Position };

        _editor.MoveCameraToSector(entry.Position);
    }

    partial void OnSelectedResultChanged(FindTextureResult? value) => NavigateTo();
}
