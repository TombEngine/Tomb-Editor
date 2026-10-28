#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.DockableViews.RoomOptionsPanel;

public partial class RoomOptionsViewModel : ObservableObject
{
    private readonly record struct UnsupportedRoomTypeWarningKey(Room Room, RoomType Type, TRVersion.Game Version);

    private static readonly IReadOnlyList<string> BaseRoomTypeKeys =
    [
        "RoomTypeNormal",
        "RoomTypeWater"
    ];

    private static readonly IReadOnlyList<string> ExtendedRoomTypeKeys =
    [
        .. BaseRoomTypeKeys,
        "RoomTypeQuicksand"
    ];

    private static readonly IReadOnlyList<string> PortalShadeKeys =
    [
        "PortalShadeDefault",
        "PortalShadeSmooth",
        "PortalShadeSharp"
    ];

    private static readonly IReadOnlyList<string> DefaultEffectKeys =
    [
        "EffectNone",
        "EffectDefault",
        "EffectReflection",
        "EffectGlow",
        "EffectMove",
        "EffectGlowAndMove",
        "EffectMist"
    ];

    private static readonly IReadOnlyList<string> TR2EffectKeys =
    [
        "EffectNone",
        "EffectDefault",
        "EffectReflection",
        "EffectGlow",
        "EffectFlicker",
        "EffectSunset",
        "EffectMist"
    ];

    private static readonly IReadOnlyList<string> ReverbTypeKeys =
    [
        "ReverbNone",
        "ReverbSmall",
        "ReverbMedium",
        "ReverbLarge",
        "ReverbPipe"
    ];

    private static readonly IReadOnlyList<string> ExtraReverbTypeKeys =
    [
        "ReverbNone",
        "ReverbDefault",
        "ReverbGeneric",
        "ReverbPaddedCell",
        "ReverbRoom",
        "ReverbBathroom",
        "ReverbLivingRoom",
        "ReverbStoneRoom",
        "ReverbAuditorium",
        "ReverbConcertHall",
        "ReverbCave",
        "ReverbArena",
        "ReverbHangar",
        "ReverbCarpetedHallway",
        "ReverbHallway",
        "ReverbStoneCorridor",
        "ReverbAlley",
        "ReverbForest",
        "ReverbCity",
        "ReverbMountains",
        "ReverbQuarry",
        "ReverbPlain",
        "ReverbParkingLot",
        "ReverbSewerPipe",
        "ReverbUnderwater",
        "ReverbSmallRoom",
        "ReverbMediumRoom",
        "ReverbLargeRoom",
        "ReverbMediumHall",
        "ReverbLargeHall",
        "ReverbPlate"
    ];

    private readonly Editor _editor;
    private readonly ILocalizationService _localizationService;

    private readonly CommandArgs _commandArgs;
    private UnsupportedRoomTypeWarningKey? _lastUnsupportedRoomTypeWarning;

    private IReadOnlyList<string> _rooms = [];
    public IReadOnlyList<string> Rooms => _rooms;

    private IReadOnlyList<string> _roomTypes = [];
    public IReadOnlyList<string> RoomTypes => _roomTypes;

    private IReadOnlyList<string> _reverbValues = [];
    public IReadOnlyList<string> ReverbValues => _reverbValues;

    private IReadOnlyList<string> _effects = [];
    public IReadOnlyList<string> Effects => _effects;

    private IReadOnlyList<string> _portalShades = [];
    public IReadOnlyList<string> PortalShades => _portalShades;

    private IReadOnlyList<string> _tagsAutoCompleteData = [];
    public IReadOnlyList<string> TagsAutoCompleteData => _tagsAutoCompleteData;

    public int SelectedRoom
    {
        get => Array.IndexOf(_editor.Level.Rooms, _editor.SelectedRoom);
        set
        {
            if (value < 0 || value >= _editor.Level.Rooms.Length)
                return;

            var selectedRoom = _editor.Level.Rooms[value];

            if (selectedRoom is null)
            {
                EditorActions.MakeNewRoom(value);
                return;
            }

            if (selectedRoom == _editor.SelectedRoom)
                return;

            _editor.SelectRoom(selectedRoom);
        }
    }

    public string Tags
    {
        get => string.Join(' ', _editor.SelectedRoom.Properties.Tags);
        set => UpdateTags(value);
    }

    public bool Skybox
    {
        get => _editor.SelectedRoom.Properties.FlagHorizon;
        set => UpdateSelectedRoomProperty(
            static properties => properties.FlagHorizon,
            static (properties, newValue) => properties.FlagHorizon = newValue,
            value);
    }

    public bool Wind
    {
        get => _editor.SelectedRoom.Properties.FlagOutside;
        set => UpdateSelectedRoomProperty(
            static properties => properties.FlagOutside,
            static (properties, newValue) => properties.FlagOutside = newValue,
            value);
    }

    public bool Damage
    {
        get => _editor.SelectedRoom.Properties.FlagDamage;
        set => UpdateSelectedRoomProperty(
            static properties => properties.FlagDamage,
            static (properties, newValue) => properties.FlagDamage = newValue,
            value);
    }

    public bool Cold
    {
        get => _editor.SelectedRoom.Properties.FlagCold;
        set => UpdateSelectedRoomProperty(
            static properties => properties.FlagCold,
            static (properties, newValue) => properties.FlagCold = newValue,
            value);
    }

    public bool NoPathfinding
    {
        get => _editor.SelectedRoom.Properties.FlagExcludeFromPathFinding;
        set => UpdateSelectedRoomProperty(
            static properties => properties.FlagExcludeFromPathFinding,
            static (properties, newValue) => properties.FlagExcludeFromPathFinding = newValue,
            value);
    }

    public bool NoLensflare
    {
        get => _editor.SelectedRoom.Properties.FlagNoLensflare;
        set => UpdateSelectedRoomProperty(
            static properties => properties.FlagNoLensflare,
            static (properties, newValue) => properties.FlagNoLensflare = newValue,
            value);
    }

    public bool NoCaustics
    {
        get => _editor.SelectedRoom.Properties.FlagNoCaustics;
        set => UpdateSelectedRoomProperty(
            static properties => properties.FlagNoCaustics,
            static (properties, newValue) => properties.FlagNoCaustics = newValue,
            value);
    }

    public int SelectedRoomType
    {
        get
        {
            int roomType = GetSelectedRoomTypeIndex();

            if (roomType == -1)
                WarnUnsupportedSelectedRoomType();
            else
                _lastUnsupportedRoomTypeWarning = null;

            return roomType;
        }
        set
        {
            if (value < 0 || value >= _roomTypes.Count)
                return;

            var (newType, newStrength) = value switch
            {
                0 => (RoomType.Normal, (byte)0),
                1 => (RoomType.Water, (byte)0),
                2 => (RoomType.Quicksand, (byte)0),
                <= 6 => (RoomType.Rain, (byte)(value - 3)),
                _ => (RoomType.Snow, (byte)(value - 7))
            };

            UpdateSelectedRoomType(newType, newStrength);
        }
    }

    public int SelectedFlipMap
    {
        get
        {
            var room = _editor.SelectedRoom;

            if (!room.Alternated)
                return 0;

            int index = room.AlternateGroup + 1;
            return index >= 0 && index < FlipMaps.Count ? index : -1;
        }
        set
        {
            var room = _editor.SelectedRoom;

            if (value < 0 || value >= FlipMaps.Count)
            {
                // Restore the current selection when the combo box temporarily reports no valid item.
                EditorEventRaised(new Editor.RoomPropertiesChangedEvent { Room = room });
                return;
            }

            short alternateGroupIndex = (short)(value - 1);

            if (room.Alternated)
            {
                if (alternateGroupIndex == -1)
                {
                    // Delete flipped room.
                    EditorActions.AlternateRoomDisableWithWarning(room, _commandArgs.Window);
                }
                else if (room.AlternateOpposite is { } alternateOpposite &&
                    room.AlternateGroup != alternateGroupIndex &&
                    alternateOpposite.AlternateGroup != alternateGroupIndex)
                {
                    // Change flipped map number.
                    room.AlternateGroup = alternateGroupIndex;
                    alternateOpposite.AlternateGroup = alternateGroupIndex;

                    _editor.RoomPropertiesChange(room);
                    _editor.RoomPropertiesChange(alternateOpposite);
                }
            }
            else if (alternateGroupIndex != -1)
            {
                // Create a new flipped room.
                EditorActions.AlternateRoomEnable(room, alternateGroupIndex);
            }

            // Update combo box even if nothing changed internally to correct invalid user input.
            EditorEventRaised(new Editor.RoomPropertiesChangedEvent { Room = room });
        }
    }

    public int SelectedReverb
    {
        get => _editor.SelectedRoom.Properties.Reverberation < ReverbValues.Count
            ? _editor.SelectedRoom.Properties.Reverberation
            : -1;
        set
        {
            if (value < 0 || value >= ReverbValues.Count)
                return;

            UpdateSelectedRoomProperty(
                static properties => properties.Reverberation,
                static (properties, newValue) => properties.Reverberation = newValue,
                (byte)value);
        }
    }

    public string? ReverbTooltip => _editor.Level.Settings.GameEnableExtraReverbPresets &&
        SelectedReverb > 0 &&
        SelectedReverb < ReverbValues.Count
            ? ReverbValues[SelectedReverb]
            : null;

    public int SelectedPortalShade
    {
        get => (int)_editor.SelectedRoom.Properties.LightInterpolationMode;
        set
        {
            if (!Enum.IsDefined(typeof(RoomLightInterpolationMode), value))
                return;

            UpdateSelectedRoomProperty(
                static properties => properties.LightInterpolationMode,
                static (properties, newValue) => properties.LightInterpolationMode = newValue,
                (RoomLightInterpolationMode)value);
        }
    }

    public int SelectedEffect
    {
        get => (int)_editor.SelectedRoom.Properties.LightEffect;
        set
        {
            if (!Enum.IsDefined(typeof(RoomLightEffect), value))
                return;

            UpdateSelectedRoomProperty(
                static properties => properties.LightEffect,
                static (properties, newValue) => properties.LightEffect = newValue,
                (RoomLightEffect)value);
        }
    }

    public byte EffectStrength
    {
        get => _editor.SelectedRoom.Properties.LightEffectStrength;
        set => UpdateSelectedRoomProperty(
            static properties => properties.LightEffectStrength,
            static (properties, newValue) => properties.LightEffectStrength = newValue,
            value);
    }

    public bool Hidden => _editor.SelectedRoom.Properties.Hidden;

    public Color AmbientLightColor => (_editor.SelectedRoom.Properties.AmbientLight * 0.5f).ToWPFColor();

    [ObservableProperty] private bool supportsHorizon;
    [ObservableProperty] private bool supportsFlagOutside;
    [ObservableProperty] private bool supportsFlagCold;
    [ObservableProperty] private bool supportsFlagDamage;
    [ObservableProperty] private bool supportsNoLensflare;
    [ObservableProperty] private bool supportsNoCaustics;
    [ObservableProperty] private bool supportsReverb;
    [ObservableProperty] private bool supportsLightEffect;
    [ObservableProperty] private bool supportsLightEffectStrength;
    [ObservableProperty] private bool canLockRoom = true;

    [ObservableProperty] private bool locked;

    public ICommand EditRoomNameCommand { get; }
    public ICommand AddNewRoomCommand { get; }
    public ICommand DuplicateRoomCommand { get; }
    public ICommand DeleteRoomsCommand { get; }
    public ICommand CropRoomCommand { get; }
    public ICommand MoveRoomUpCommand { get; }
    public ICommand SplitRoomCommand { get; }
    public ICommand MoveRoomDownCommand { get; }
    public ICommand SelectPreviousRoomCommand { get; }
    public ICommand LockRoomCommand { get; }
    public ICommand HideRoomCommand { get; }
    public ICommand EditAmbientLightCommand { get; }

    private IReadOnlyList<string> _flipMaps = [];
    public IReadOnlyList<string> FlipMaps => _flipMaps;

    public RoomOptionsViewModel(Editor editor, ILocalizationService? localizationService = null)
    {
        _editor = editor;
        _editor.EditorEventRaised += EditorEventRaised;

        _localizationService = ServiceLocator.ResolveService(localizationService)
            .WithKeysFor(this);

        RefreshVersionSpecificState();
        RefreshRooms();
        RefreshTagsAutoCompleteData();
        RefreshSelectedRoomLockState();
        RepopulateFlipMaps();

        _commandArgs = new CommandArgs(WPFUtils.GetWindowOwner(), _editor);

        EditRoomNameCommand = CommandHandler.GetCommand("EditRoomName", _commandArgs);
        AddNewRoomCommand = CommandHandler.GetCommand("AddNewRoom", _commandArgs);
        DuplicateRoomCommand = CommandHandler.GetCommand("DuplicateRoom", _commandArgs);
        DeleteRoomsCommand = CommandHandler.GetCommand("DeleteRooms", _commandArgs);
        CropRoomCommand = CommandHandler.GetCommand("CropRoom", _commandArgs);
        MoveRoomUpCommand = CommandHandler.GetCommand("MoveRoomUp", _commandArgs);
        SplitRoomCommand = CommandHandler.GetCommand("SplitRoom", _commandArgs);
        MoveRoomDownCommand = CommandHandler.GetCommand("MoveRoomDown", _commandArgs);
        SelectPreviousRoomCommand = CommandHandler.GetCommand("SelectPreviousRoom", _commandArgs);
        LockRoomCommand = CommandHandler.GetCommand("LockRoom", _commandArgs);
        HideRoomCommand = CommandHandler.GetCommand("HideRoom", _commandArgs);
        EditAmbientLightCommand = CommandHandler.GetCommand("EditAmbientLight", _commandArgs);
    }

    public void Cleanup()
        => _editor.EditorEventRaised -= EditorEventRaised;

    private IReadOnlyList<string> BuildRoomTypeOptions(TRVersion.Game version)
    {
        if (_editor.Level.IsNG)
        {
            return
            [
                .. LocalizeKeys(ExtendedRoomTypeKeys),
                .. BuildIndexedLabels("RoomTypeRain", 4),
                .. BuildIndexedLabels("RoomTypeSnow", 4)
            ];
        }

        if (version is TRVersion.Game.TR3 || _editor.Level.IsTRX || _editor.Level.IsTombEngine)
            return LocalizeKeys(ExtendedRoomTypeKeys);

        return LocalizeKeys(BaseRoomTypeKeys);
    }

    private IReadOnlyList<string> BuildIndexedLabels(string key, int count)
        => [.. Enumerable.Range(1, count).Select(index => _localizationService.Format(key, index))];

    private int GetSelectedRoomTypeIndex()
    {
        var level = _editor.Level;
        var room = _editor.SelectedRoom;
        var version = level.Settings.GameVersion;

        // Map room types to UI indices while hiding engine-specific unsupported values.
        // TombEngine expects rain and snow to be configured through triggers or script.

        if (room.Properties.Type is RoomType.Quicksand &&
            version is not (TRVersion.Game.TR3 or TRVersion.Game.TRNG or TRVersion.Game.TombEngine) &&
            !level.IsTRX)
        {
            return -1;
        }

        if (room.Properties.Type is RoomType.Rain or RoomType.Snow && version is not TRVersion.Game.TRNG)
            return -1;

        return room.Properties.Type switch
        {
            RoomType.Normal => 0,
            RoomType.Water => 1,
            RoomType.Quicksand => 2,
            RoomType.Rain => 3 + room.Properties.TypeStrength,
            RoomType.Snow => 7 + room.Properties.TypeStrength,
            _ => -1
        };
    }

    private void NotifySelectedRoomPropertiesChanged()
    {
        OnPropertyChanged(nameof(Tags));
        OnPropertyChanged(nameof(Skybox));
        OnPropertyChanged(nameof(Wind));
        OnPropertyChanged(nameof(Damage));
        OnPropertyChanged(nameof(Cold));
        OnPropertyChanged(nameof(NoPathfinding));
        OnPropertyChanged(nameof(NoLensflare));
        OnPropertyChanged(nameof(NoCaustics));
        OnPropertyChanged(nameof(SelectedRoomType));
        OnPropertyChanged(nameof(SelectedFlipMap));
        OnPropertyChanged(nameof(SelectedReverb));
        OnPropertyChanged(nameof(ReverbTooltip));
        OnPropertyChanged(nameof(SelectedPortalShade));
        OnPropertyChanged(nameof(SelectedEffect));
        OnPropertyChanged(nameof(EffectStrength));
        OnPropertyChanged(nameof(Hidden));
        OnPropertyChanged(nameof(AmbientLightColor));
    }

    private void RefreshRooms()
        => _rooms = [.. _editor.Level.Rooms.Select((room, index) => $"{index}: {room?.Name ?? _localizationService["EmptyRoom"]}")];

    private void RefreshSelectedRoomLockState()
    {
        var room = _editor.SelectedRoom;

        if (room.AlternateBaseRoom is { } alternateBaseRoom)
        {
            CanLockRoom = false;
            Locked = alternateBaseRoom.Properties.Locked;
            return;
        }

        CanLockRoom = true;
        Locked = room.Properties.Locked;
    }

    private void RefreshTagsAutoCompleteData()
    {
        _tagsAutoCompleteData = [.. _editor.Level.Rooms
            .Where(room => room?.ExistsInLevel == true)
            .SelectMany(room => room.Properties.Tags)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(tag => tag, StringComparer.Ordinal)];
    }

    private void RefreshVersionSpecificState()
    {
        var level = _editor.Level;
        var version = level.Settings.GameVersion;
        bool isTR1 = version.Native() == TRVersion.Game.TR1;
        bool isTEN = version is TRVersion.Game.TombEngine;
        bool isNgOrTEN = version is TRVersion.Game.TRNG or TRVersion.Game.TombEngine;

        SupportsHorizon = !isTR1 || level.IsTRX;
        SupportsFlagOutside = !isTR1 || level.IsTRX;
        SupportsFlagCold = isNgOrTEN || level.IsTRX;
        SupportsFlagDamage = isNgOrTEN || level.IsTRX;
        SupportsNoLensflare = version.SupportsLensflare();
        SupportsNoCaustics = isTEN;
        SupportsReverb = version.SupportsReverberation();
        SupportsLightEffect = !isTR1;
        SupportsLightEffectStrength = !isTR1;

        _roomTypes = BuildRoomTypeOptions(version);
        _reverbValues = BuildReverbValues(version, level.Settings.GameEnableExtraReverbPresets);
        _effects = LocalizeKeys(version is TRVersion.Game.TR2 ? TR2EffectKeys : DefaultEffectKeys);
        _portalShades = LocalizeKeys(PortalShadeKeys);
    }

    private IReadOnlyList<string> BuildReverbValues(TRVersion.Game version, bool enableExtraPresets)
    {
        if (version.Native() is not TRVersion.Game.TR4 || !enableExtraPresets)
            return LocalizeKeys(ReverbTypeKeys);

        IReadOnlyList<string> localizedExtraReverbs = LocalizeKeys(ExtraReverbTypeKeys);
        int customPresetCount = StringEnums.ExtraReverberationTypes.Count - ExtraReverbTypeKeys.Count;

        if (customPresetCount <= 0)
            return localizedExtraReverbs;

        return
        [
            .. localizedExtraReverbs,
            .. Enumerable.Range(1, customPresetCount)
                .Select(index => _localizationService.Format("ReverbCustom", index))
        ];
    }

    private IReadOnlyList<string> LocalizeKeys(IReadOnlyList<string> keys)
        => [.. keys.Select(key => _localizationService[key])];

    private void RepopulateFlipMaps()
    {
        int flipmapCount = _editor.Level.Settings.GameVersion is TRVersion.Game.TombEngine ? byte.MaxValue : 15;

        _flipMaps =
        [
            _localizationService["FlipmapNone"],
            .. Enumerable.Range(0, flipmapCount).Select(static i => i.ToString())
        ];

        OnPropertyChanged(nameof(FlipMaps));
        OnPropertyChanged(nameof(SelectedFlipMap));
    }

    private void UpdateSelectedRoomType(RoomType newType, byte newStrength)
    {
        var room = _editor.SelectedRoom;

        if (room.Properties.Type == newType && room.Properties.TypeStrength == newStrength)
            return;

        room.Properties.Type = newType;
        room.Properties.TypeStrength = newStrength;

        _editor.RoomPropertiesChange(room);
    }

    private void UpdateTags(string? value)
    {
        var room = _editor.SelectedRoom;
        string input = value ?? string.Empty;

        List<string> newTags = [.. input
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries) // `null` = split on any whitespace
            .Select(tag => tag.ToLowerInvariant())];

        string normalizedTags = string.Join(' ', newTags);

        if (room.Properties.Tags.SequenceEqual(newTags))
        {
            if (!string.Equals(input, normalizedTags, StringComparison.Ordinal))
                OnPropertyChanged(nameof(Tags));

            return;
        }

        room.Properties.Tags = newTags;
        _editor.RoomPropertiesChange(room);

        RefreshTagsAutoCompleteData();

        if (!string.Equals(input, normalizedTags, StringComparison.Ordinal))
            OnPropertyChanged(nameof(Tags));

        OnPropertyChanged(nameof(TagsAutoCompleteData));
    }

    private void UpdateSelectedRoomProperty<T>(Func<RoomProperties, T> getter, Action<RoomProperties, T> setter, T value)
    {
        var room = _editor.SelectedRoom;

        if (EqualityComparer<T>.Default.Equals(getter(room.Properties), value))
            return;

        setter(room.Properties, value);
        _editor.RoomPropertiesChange(room);
    }

    private void WarnUnsupportedSelectedRoomType()
    {
        var room = _editor.SelectedRoom;
        var warningKey = new UnsupportedRoomTypeWarningKey(room, room.Properties.Type, _editor.Level.Settings.GameVersion);

        if (_lastUnsupportedRoomTypeWarning == warningKey)
            return;

        _lastUnsupportedRoomTypeWarning = warningKey;
        _editor.SendMessage(_localizationService["UnsupportedRoomTypeMessage"], PopupType.Warning);
    }

    private void EditorEventRaised(IEditorEvent obj)
    {
        bool isSelectedRoomPropertyChanged = _editor.IsSelectedRoomEvent(obj as Editor.RoomPropertiesChangedEvent);

        // Disable version-specific controls.
        if (obj is Editor.InitEvent or Editor.GameVersionChangedEvent or Editor.LevelChangedEvent)
        {
            RefreshVersionSpecificState();
            OnPropertyChanged(nameof(RoomTypes));
            OnPropertyChanged(nameof(ReverbValues));
            OnPropertyChanged(nameof(ReverbTooltip));
            OnPropertyChanged(nameof(Effects));
            OnPropertyChanged(nameof(PortalShades));
            RepopulateFlipMaps();
        }

        // Update the room list.
        if (obj is Editor.InitEvent or Editor.LevelChangedEvent or Editor.RoomListChangedEvent)
        {
            int cachedRoomIndex = SelectedRoom;

            RefreshRooms();
            OnPropertyChanged(nameof(Rooms));

            SelectedRoom = cachedRoomIndex;
            OnPropertyChanged(nameof(SelectedRoom));
        }

        // Update tag list.
        if (obj is Editor.InitEvent or Editor.LevelChangedEvent or Editor.RoomListChangedEvent or Editor.SelectedRoomChangedEvent ||
            isSelectedRoomPropertyChanged)
        {
            RefreshTagsAutoCompleteData();
            OnPropertyChanged(nameof(TagsAutoCompleteData));
        }

        // Update all room property controls.
        if (obj is Editor.InitEvent or Editor.SelectedRoomChangedEvent or Editor.LevelChangedEvent or Editor.GameVersionChangedEvent ||
            isSelectedRoomPropertyChanged)
        {
            if (obj is Editor.SelectedRoomChangedEvent)
                OnPropertyChanged(nameof(SelectedRoom));

            NotifySelectedRoomPropertiesChanged();
            RefreshSelectedRoomLockState();
        }
    }
}
