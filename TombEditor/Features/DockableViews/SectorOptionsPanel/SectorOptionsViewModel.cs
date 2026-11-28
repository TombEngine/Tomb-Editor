#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Numerics;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Icons;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.SectorOptionsPanel;

public partial class SectorOptionsViewModel : ObservableObject
{
    private const float IconSwitchBrightnessThreshold = 0.8f;

    private static readonly ImageSource FloorIcon = IconSources.Load("Sectortype/Floor_1");
    private static readonly ImageSource FloorIconNegative = IconSources.Load("Sectortype/Floor_neg");
    private static readonly ImageSource CeilingIcon = IconSources.Load("Sectortype/Roof");
    private static readonly ImageSource CeilingIconNegative = IconSources.Load("Sectortype/Roof_neg");
    private static readonly ImageSource BoxIcon = IconSources.Load("Sectortype/Box");
    private static readonly ImageSource BoxIconNegative = IconSources.Load("Sectortype/Box_neg");
    private static readonly ImageSource NotWalkableIcon = IconSources.Load("Sectortype/NotWalkable");
    private static readonly ImageSource NotWalkableIconNegative = IconSources.Load("Sectortype/NotWalkable_neg");
    private static readonly ImageSource MonkeyIcon = IconSources.Load("Sectortype/Monkey");
    private static readonly ImageSource MonkeyIconNegative = IconSources.Load("Sectortype/Monkey_neg");
    private static readonly ImageSource DeathIcon = IconSources.Load("Sectortype/Death");
    private static readonly ImageSource DeathIconNegative = IconSources.Load("Sectortype/Death_neg");
    private static readonly ImageSource PortalIcon = IconSources.Load("Sectortype/Portal");
    private static readonly ImageSource PortalIconNegative = IconSources.Load("Sectortype/Portal_neg");
    private static readonly ImageSource WallIcon = IconSources.Load("Sectortype/Wall_1");
    private static readonly ImageSource WallIconNegative = IconSources.Load("Sectortype/Wall_neg");

    private static readonly ImageSource TriggerTriggererIconTR4 = IconSources.Load("Sectortype/TriggerTriggerer");
    private static readonly ImageSource TriggerTriggererIconTR3 = IconSources.Load("Sectortype/MinecartLeft");
    private static readonly ImageSource BeetleIconTR4 = IconSources.Load("Sectortype/Beetle");
    private static readonly ImageSource BeetleIconTR3 = IconSources.Load("Sectortype/MinecartRight");

    [ObservableProperty] private Vector4 floorColor;
    [ObservableProperty] private Vector4 boxColor;
    [ObservableProperty] private Vector4 notWalkableColor;
    [ObservableProperty] private Vector4 monkeyswingColor;
    [ObservableProperty] private Vector4 deathColor;
    [ObservableProperty] private Vector4 portalColor;
    [ObservableProperty] private Vector4 wallColor;

    [ObservableProperty] private ImageSource floorIconSource = FloorIcon;
    [ObservableProperty] private ImageSource ceilingIconSource = CeilingIcon;
    [ObservableProperty] private ImageSource boxIconSource = BoxIcon;
    [ObservableProperty] private ImageSource notWalkableIconSource = NotWalkableIcon;
    [ObservableProperty] private ImageSource monkeyIconSource = MonkeyIcon;
    [ObservableProperty] private ImageSource deathIconSource = DeathIcon;
    [ObservableProperty] private ImageSource portalIconSource = PortalIcon;
    [ObservableProperty] private ImageSource wallIconSource = WallIcon;

    [ObservableProperty] private bool supportsClimbing;
    [ObservableProperty] private bool supportsMonkeySwing;
    [ObservableProperty] private bool supportsBeetleCheckpoint;
    [ObservableProperty] private bool supportsTriggerTriggerer;

    [ObservableProperty] private ImageSource triggerTriggererIcon = TriggerTriggererIconTR4;
    [ObservableProperty] private ImageSource beetleIcon = BeetleIconTR4;

    public ICommand SetFloorCommand { get; }
    public ICommand SetCeilingCommand { get; }
    public ICommand SetBoxCommand { get; }
    public ICommand SetNotWalkableCommand { get; }
    public ICommand SetMonkeyswingCommand { get; }
    public ICommand SetDeathCommand { get; }
    public ICommand AddPortalCommand { get; }
    public ICommand SetWallCommand { get; }
    public ICommand SetTriggerTriggererCommand { get; }
    public ICommand SetBeetleCheckpointCommand { get; }
    public ICommand SetClimbPositiveZCommand { get; }
    public ICommand SetClimbPositiveXCommand { get; }
    public ICommand SetClimbNegativeZCommand { get; }
    public ICommand SetClimbNegativeXCommand { get; }
    public ICommand AddGhostBlocksToSelectionCommand { get; }
    public ICommand ToggleForceFloorSolidCommand { get; }
    public ICommand FloorStepCommand { get; }
    public ICommand CeilingStepCommand { get; }
    public ICommand DiagonalWallCommand { get; }

    private readonly Editor _editor;

    public SectorOptionsViewModel(Editor editor)
    {
        _editor = editor;
        _editor.EditorEventRaised += EditorEventRaised;

        var args = new CommandArgs(WPFUtils.GetWindowOwner(), _editor);

        SetFloorCommand = CommandHandler.GetCommand("SetFloor", args);
        SetCeilingCommand = CommandHandler.GetCommand("SetCeiling", args);
        SetBoxCommand = CommandHandler.GetCommand("SetBox", args);
        SetNotWalkableCommand = CommandHandler.GetCommand("SetNotWalkable", args);
        SetMonkeyswingCommand = CommandHandler.GetCommand("SetMonkeyswing", args);
        SetDeathCommand = CommandHandler.GetCommand("SetDeath", args);
        AddPortalCommand = CommandHandler.GetCommand("AddPortal", args);
        SetWallCommand = CommandHandler.GetCommand("SetWall", args);
        SetTriggerTriggererCommand = CommandHandler.GetCommand("SetTriggerTriggerer", args);
        SetBeetleCheckpointCommand = CommandHandler.GetCommand("SetBeetleCheckpoint", args);
        SetClimbPositiveZCommand = CommandHandler.GetCommand("SetClimbPositiveZ", args);
        SetClimbPositiveXCommand = CommandHandler.GetCommand("SetClimbPositiveX", args);
        SetClimbNegativeZCommand = CommandHandler.GetCommand("SetClimbNegativeZ", args);
        SetClimbNegativeXCommand = CommandHandler.GetCommand("SetClimbNegativeX", args);
        AddGhostBlocksToSelectionCommand = CommandHandler.GetCommand("AddGhostBlocksToSelection", args);
        ToggleForceFloorSolidCommand = CommandHandler.GetCommand("ToggleForceFloorSolid", args);
        FloorStepCommand = CommandHandler.GetCommand("SetDiagonalFloorStep", args);
        CeilingStepCommand = CommandHandler.GetCommand("SetDiagonalCeilingStep", args);
        DiagonalWallCommand = CommandHandler.GetCommand("SetDiagonalWall", args);

        SetButtonColors();
        UpdateVersionSpecificControls();
    }

    public void Cleanup()
        => _editor.EditorEventRaised -= EditorEventRaised;

    private void EditorEventRaised(IEditorEvent obj)
    {
        if (obj is Editor.ConfigurationChangedEvent or Editor.InitEvent)
            SetButtonColors();

        if (obj is Editor.InitEvent or Editor.GameVersionChangedEvent or Editor.LevelChangedEvent)
            UpdateVersionSpecificControls();
    }

    private void SetButtonColors()
    {
        FloorColor = _editor.Configuration.UI_ColorScheme.ColorFloor;
        BoxColor = _editor.Configuration.UI_ColorScheme.ColorBox;
        NotWalkableColor = _editor.Configuration.UI_ColorScheme.ColorNotWalkable;
        MonkeyswingColor = _editor.Configuration.UI_ColorScheme.ColorMonkey;
        DeathColor = _editor.Configuration.UI_ColorScheme.ColorDeath;
        PortalColor = _editor.Configuration.UI_ColorScheme.ColorPortal;
        WallColor = _editor.Configuration.UI_ColorScheme.ColorWall;

        FloorIconSource = GetContrastAwareIcon(FloorColor, FloorIcon, FloorIconNegative);
        CeilingIconSource = GetContrastAwareIcon(FloorColor, CeilingIcon, CeilingIconNegative);
        BoxIconSource = GetContrastAwareIcon(BoxColor, BoxIcon, BoxIconNegative);
        NotWalkableIconSource = GetContrastAwareIcon(NotWalkableColor, NotWalkableIcon, NotWalkableIconNegative);
        MonkeyIconSource = GetContrastAwareIcon(MonkeyswingColor, MonkeyIcon, MonkeyIconNegative);
        DeathIconSource = GetContrastAwareIcon(DeathColor, DeathIcon, DeathIconNegative);
        PortalIconSource = GetContrastAwareIcon(PortalColor, PortalIcon, PortalIconNegative);
        WallIconSource = GetContrastAwareIcon(WallColor, WallIcon, WallIconNegative);
    }

    private static ImageSource GetContrastAwareIcon(Vector4 color, ImageSource defaultIcon, ImageSource negativeIcon)
        => color.ToWPFBrush().GetBrightness() > IconSwitchBrightnessThreshold ? negativeIcon : defaultIcon;

    private void UpdateVersionSpecificControls()
    {
        var version = _editor.Level.Settings.GameVersion;
        bool beetleTrigMineSupported = version >= TRVersion.Game.TR3;

        SupportsClimbing = version.SupportsClimbing();
        SupportsMonkeySwing = version.SupportsMonkeySwing();
        SupportsBeetleCheckpoint = beetleTrigMineSupported;
        SupportsTriggerTriggerer = beetleTrigMineSupported;

        if (version.Native() >= TRVersion.Game.TR4)
        {
            TriggerTriggererIcon = TriggerTriggererIconTR4;
            BeetleIcon = BeetleIconTR4;
        }
        else
        {
            TriggerTriggererIcon = TriggerTriggererIconTR3;
            BeetleIcon = BeetleIconTR3;
        }
    }

    [RelayCommand]
    private void SetSectorColoringInfoPriority(SectorColoringType type)
    {
        if (!_editor.Configuration.UI_AutoSwitchSectorColoringInfo)
            return;

        _editor.SectorColoringManager.SetPriority(type);
    }
}
