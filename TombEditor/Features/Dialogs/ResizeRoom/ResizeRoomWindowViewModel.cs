#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLib;
using TombLib.LevelData;
using TombLib.Wad.Catalog;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.ResizeRoom;

public partial class ResizeRoomWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly ILocalizationService _localizationService;

    private bool _updatingBounds;

    public Room Room { get; }
    public Editor Editor { get; }
    public int RecommendedDimensions { get; }
    public string VisibleDimensions { get; }

    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty] private int _expandXNegative;
    [ObservableProperty] private int _expandXPositive;
    [ObservableProperty] private int _expandZNegative;
    [ObservableProperty] private int _expandZPositive;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOversized))]
    private bool _allowOversizedRooms;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UseWalls))]
    private bool _useFloor = true;

    public bool UseWalls
    {
        get => !UseFloor;
        set => UseFloor = !value;
    }

    [ObservableProperty] private int _minXNegative;
    [ObservableProperty] private int _maxXNegative;
    [ObservableProperty] private int _minXPositive;
    [ObservableProperty] private int _maxXPositive;
    [ObservableProperty] private int _minZNegative;
    [ObservableProperty] private int _maxZNegative;
    [ObservableProperty] private int _minZPositive;
    [ObservableProperty] private int _maxZPositive;

    public RectangleInt2 NewArea => new(
        -ExpandXNegative,
        -ExpandZNegative,
        Room.NumXSectors + ExpandXPositive - 1,
        Room.NumZSectors + ExpandZPositive - 1);

    public bool IsOversized => AllowOversizedRooms
        && (NewArea.Width >= RecommendedDimensions || NewArea.Height >= RecommendedDimensions);

    public string OversizedWarning => _localizationService.Format("OversizedWarning", VisibleDimensions);

    public event System.Action? AreaChanged;

    public ResizeRoomWindowViewModel(
        Editor editor,
        Room roomToResize,
        RectangleInt2 newArea,
        ILocalizationService? localizationService = null)
    {
        Editor = editor;
        Room = roomToResize;
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        RecommendedDimensions = TrCatalog.GetLimit(editor.Level.Settings.GameVersion, Limit.RoomDimensions);
        VisibleDimensions = $"{RecommendedDimensions - 2}x{RecommendedDimensions - 2}";

        _updatingBounds = true;
        _expandXNegative = -newArea.X0;
        _expandZNegative = -newArea.Y0;
        _expandXPositive = newArea.X1 + 1 - roomToResize.NumXSectors;
        _expandZPositive = newArea.Y1 + 1 - roomToResize.NumZSectors;
        _updatingBounds = false;

        UpdateBounds();
    }

    partial void OnExpandXNegativeChanged(int value) => UpdateBounds();
    partial void OnExpandXPositiveChanged(int value) => UpdateBounds();
    partial void OnExpandZNegativeChanged(int value) => UpdateBounds();
    partial void OnExpandZPositiveChanged(int value) => UpdateBounds();
    partial void OnAllowOversizedRoomsChanged(bool value) => UpdateBounds();
    partial void OnUseFloorChanged(bool value) => AreaChanged?.Invoke();

    private void UpdateBounds()
    {
        if (_updatingBounds)
            return;

        _updatingBounds = true;

        int maxDimensions = AllowOversizedRooms ? 255 : RecommendedDimensions;

        MinXNegative = (3 - Room.NumXSectors) - ExpandXPositive;
        MinZNegative = (3 - Room.NumZSectors) - ExpandZPositive;
        MinXPositive = (3 - Room.NumXSectors) - ExpandXNegative;
        MinZPositive = (3 - Room.NumZSectors) - ExpandZNegative;

        MaxXNegative = (maxDimensions - Room.NumXSectors) - ExpandXPositive;
        MaxXPositive = (maxDimensions - Room.NumXSectors) - ExpandXNegative;
        MaxZNegative = (maxDimensions - Room.NumZSectors) - ExpandZPositive;
        MaxZPositive = (maxDimensions - Room.NumZSectors) - ExpandZNegative;

        OnPropertyChanged(nameof(NewArea));
        OnPropertyChanged(nameof(IsOversized));
        OnPropertyChanged(nameof(OversizedWarning));

        _updatingBounds = false;

        AreaChanged?.Invoke();
    }

    [RelayCommand]
    private void Confirm() => DialogResult = true;

    [RelayCommand]
    private void Cancel() => DialogResult = false;
}
