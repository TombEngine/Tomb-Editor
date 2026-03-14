using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Numerics;
using System.Windows.Input;
using TombLib.LevelData;
using TombLib.Rendering;

namespace TombEditor.WPF.ViewModels;

public partial class SectorOptionsViewModel : ObservableObject
{
	[ObservableProperty] private Vector4 floorColor;
	[ObservableProperty] private Vector4 boxColor;
	[ObservableProperty] private Vector4 notWalkableColor;
	[ObservableProperty] private Vector4 monkeyswingColor;
	[ObservableProperty] private Vector4 deathColor;
	[ObservableProperty] private Vector4 portalColor;
	[ObservableProperty] private Vector4 wallColor;

	[ObservableProperty] private bool supportsClimbing;
	[ObservableProperty] private bool supportsMonkeySwing;
	[ObservableProperty] private bool supportsBeetleCheckpoint;
	[ObservableProperty] private bool supportsTriggerTriggerer;

	[ObservableProperty] private string triggerTriggererIcon = TriggerTriggererIconTR4;
	[ObservableProperty] private string beetleIcon = BeetleIconTR4;

	private const string TriggerTriggererIconTR4 = "/TombEditor;component/Resources/icons_sectortype/sectortype_TriggerTriggerer-16.png";
	private const string TriggerTriggererIconTR3 = "/TombEditor;component/Resources/icons_sectortype/sectortype_MinecartLeft-16.png";
	private const string BeetleIconTR4 = "/TombEditor;component/Resources/icons_sectortype/sectortype_Beetle-16.png";
	private const string BeetleIconTR3 = "/TombEditor;component/Resources/icons_sectortype/sectortype_MinecartRight-16.png";

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

		SetFloorCommand = CommandHandler.GetCommand("SetFloor", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetCeilingCommand = CommandHandler.GetCommand("SetCeiling", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetBoxCommand = CommandHandler.GetCommand("SetBox", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetNotWalkableCommand = CommandHandler.GetCommand("SetNotWalkable", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetMonkeyswingCommand = CommandHandler.GetCommand("SetMonkeyswing", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetDeathCommand = CommandHandler.GetCommand("SetDeath", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddPortalCommand = CommandHandler.GetCommand("AddPortal", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetWallCommand = CommandHandler.GetCommand("SetWall", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetTriggerTriggererCommand = CommandHandler.GetCommand("SetTriggerTriggerer", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetBeetleCheckpointCommand = CommandHandler.GetCommand("SetBeetleCheckpoint", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetClimbPositiveZCommand = CommandHandler.GetCommand("SetClimbPositiveZ", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetClimbPositiveXCommand = CommandHandler.GetCommand("SetClimbPositiveX", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetClimbNegativeZCommand = CommandHandler.GetCommand("SetClimbNegativeZ", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SetClimbNegativeXCommand = CommandHandler.GetCommand("SetClimbNegativeX", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddGhostBlocksToSelectionCommand = CommandHandler.GetCommand("AddGhostBlocksToSelection", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		ToggleForceFloorSolidCommand = CommandHandler.GetCommand("ToggleForceFloorSolid", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		FloorStepCommand = CommandHandler.GetCommand("SetDiagonalFloorStep", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		CeilingStepCommand = CommandHandler.GetCommand("SetDiagonalCeilingStep", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DiagonalWallCommand = CommandHandler.GetCommand("SetDiagonalWall", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));

		SetButtonColors();
		UpdateVersionSpecificControls();
	}

	public void Cleanup()
	{
		_editor.EditorEventRaised -= EditorEventRaised;
	}

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
	}

	private void UpdateVersionSpecificControls()
	{
		var version = _editor.Level.Settings.GameVersion;
		bool isTR345 = version.Native() >= TRVersion.Game.TR3;

		SupportsClimbing = version.SupportsClimbing();
		SupportsMonkeySwing = version.SupportsMonkeySwing();
		SupportsBeetleCheckpoint = isTR345;
		SupportsTriggerTriggerer = isTR345;

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
