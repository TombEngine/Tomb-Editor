#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.RoomOptionsPanel;

public partial class RoomOptionsViewModel : ObservableObject
{
	public IEnumerable<string> Rooms => _editor.Level.Rooms
		.Select((room, index) => $"{index}: {room?.Name ?? Localizer.Instance["EmptyRoom"]}");

	public IEnumerable<string> RoomTypes
	{
		get
		{
			var result = new List<string>
			{
				"Normal",
				"Water"
			};

			if (_editor.Level.Settings.GameVersion == TRVersion.Game.TR3 ||
				_editor.Level.IsTRX ||
				_editor.Level.IsNG ||
				_editor.Level.IsTombEngine)
				result.Add("Quicksand");

			if (_editor.Level.IsNG)
				StringEnums.NGRoomTypes.ForEach(result.Add);

			return result;
		}
	}

	public IEnumerable<string> ReverbValues
		=> _editor.Level.Settings.GameVersion.Native() == TRVersion.Game.TR4 && _editor.Level.Settings.GameEnableExtraReverbPresets
			? StringEnums.ExtraReverberationTypes
			: StringEnums.ReverberationTypes;

	public IEnumerable<string> PortalShades => new List<string>
	{
		"Default",
		"Smooth",
		"Sharp"
	};

	public IEnumerable<string> Effects
	{
		get
		{
			bool isTR2 = _editor.Level.Settings.GameVersion is TRVersion.Game.TR2;

			return new List<string>
			{
				"None",
				"Default",
				"Reflection",
				"Glow",
				isTR2 ? "Flicker" : "Move",
				isTR2 ? "Sunset" : "Glow & Move",
				"Mist"
			};
		}
	}

	public int SelectedRoom
	{
		get => Array.IndexOf(_editor.Level.Rooms, _editor.SelectedRoom);
		set
		{
			if (value == -1)
				return;

			Room selectedRoom = _editor.Level.Rooms[value];

			if (selectedRoom is null)
				EditorActions.MakeNewRoom(value);
			else
				_editor.SelectRoom(selectedRoom);
		}
	}

	public string Tags
	{
		get => string.Join(' ', _editor.SelectedRoom.Properties.Tags);
		set
		{
			if (_editor.SelectedRoom is null)
				return;

			_editor.SelectedRoom.Properties.Tags = value.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
			_editor.RoomPropertiesChange(_editor.SelectedRoom);

			OnPropertyChanged(nameof(TagsAutoCompleteData));
		}
	}

	public bool Skybox
	{
		get => _editor.SelectedRoom.Properties.FlagHorizon;
		set
		{
			_editor.SelectedRoom.Properties.FlagHorizon = value;
			_editor.RoomPropertiesChange(_editor.SelectedRoom);
		}
	}

	public bool Wind
	{
		get => _editor.SelectedRoom.Properties.FlagOutside;
		set
		{
			_editor.SelectedRoom.Properties.FlagOutside = value;
			_editor.RoomPropertiesChange(_editor.SelectedRoom);
		}
	}

	public bool Damage
	{
		get => _editor.SelectedRoom.Properties.FlagDamage;
		set
		{
			_editor.SelectedRoom.Properties.FlagDamage = value;
			_editor.RoomPropertiesChange(_editor.SelectedRoom);
		}
	}

	public bool Cold
	{
		get => _editor.SelectedRoom.Properties.FlagCold;
		set
		{
			_editor.SelectedRoom.Properties.FlagCold = value;
			_editor.RoomPropertiesChange(_editor.SelectedRoom);
		}
	}

	public bool NoPathfinding
	{
		get => _editor.SelectedRoom.Properties.FlagExcludeFromPathFinding;
		set
		{
			_editor.SelectedRoom.Properties.FlagExcludeFromPathFinding = value;
			_editor.RoomPropertiesChange(_editor.SelectedRoom);
		}
	}

	public bool NoLensflare
	{
		get => _editor.SelectedRoom.Properties.FlagNoLensflare;
		set
		{
			_editor.SelectedRoom.Properties.FlagNoLensflare = value;
			_editor.RoomPropertiesChange(_editor.SelectedRoom);
		}
	}

	public bool NoCaustics
	{
		get => _editor.SelectedRoom.Properties.FlagNoCaustics;
		set
		{
			_editor.SelectedRoom.Properties.FlagNoCaustics = value;
			_editor.RoomPropertiesChange(_editor.SelectedRoom);
		}
	}

	public int SelectedRoomType
	{
		get
		{
			Room room = _editor.SelectedRoom;

			// Cleverly switch room types dependent on game version.
			// We disable rain/snow types for TombEngine because it is expected to set these options with triggers and/or script.

			int roomType;

			if (room.Properties.Type is RoomType.Quicksand &&
				_editor.Level.Settings.GameVersion is not TRVersion.Game.TR3 and not TRVersion.Game.TRNG and not TRVersion.Game.TombEngine &&
				!_editor.Level.IsTRX)
				roomType = -1;
			else if (room.Properties.Type is RoomType.Rain or RoomType.Snow && _editor.Level.Settings.GameVersion is not TRVersion.Game.TRNG)
				roomType = -1;
			else
			{
				roomType = room.Properties.Type switch
				{
					RoomType.Normal => 0,
					RoomType.Water => 1,
					RoomType.Quicksand => 2,
					RoomType.Rain => 3 + room.Properties.TypeStrength,
					RoomType.Snow => 7 + room.Properties.TypeStrength,
					_ => -1
				};
			}

			// If selected type is -1 it means this room type is unsupported in current version. Throw a message about it.
			if (roomType == -1)
				_editor.SendMessage("Current room type is not supported in this engine version.\nChange it manually or switch engine version.", PopupType.Warning);

			return roomType;
		}
		set
		{
			RoomType newType;
			byte newStrength = 0;

			switch (value)
			{
				case -1:
					// Wrong type, do nothing
					return;

				case 0:
					newType = RoomType.Normal;
					break;

				case 1:
					newType = RoomType.Water;
					break;

				case 2:
					newType = RoomType.Quicksand;
					break;

				default:
					if (value <= 6)
					{
						newType = RoomType.Rain;
						newStrength = (byte)(value - 3);
					}
					else
					{
						newType = RoomType.Snow;
						newStrength = (byte)(value - 7);
					}

					break;
			}

			if (_editor.SelectedRoom.Properties.Type != newType || _editor.SelectedRoom.Properties.TypeStrength != newStrength)
			{
				_editor.SelectedRoom.Properties.Type = newType;
				_editor.SelectedRoom.Properties.TypeStrength = newStrength;
				_editor.RoomPropertiesChange(_editor.SelectedRoom);
			}
		}
	}

	public int SelectedFlipMap
	{
		get
		{
			Room room = _editor.SelectedRoom;

			if (!room.Alternated)
				return 0;

			int index = room.AlternateGroup + 1;
			return index < FlipMaps.Count ? index : -1;
		}
		set
		{
			if (_editor.SelectedRoom is null)
				return;

			Room room = _editor.SelectedRoom;
			short alternateGroupIndex = (short)(value - 1);

			if (room.Alternated)
			{
				if (alternateGroupIndex == -1)
				{
					// Delete flipped room
					EditorActions.AlternateRoomDisableWithWarning(room, WPFUtils.GetWin32WindowFromCaller(this));
				}
				else
				{
					// Change flipped map number, not much to do here
					if (room.AlternateGroup != alternateGroupIndex &&
						room.AlternateOpposite.AlternateGroup != alternateGroupIndex)
					{
						room.AlternateGroup = alternateGroupIndex;
						room.AlternateOpposite.AlternateGroup = alternateGroupIndex;
						_editor.RoomPropertiesChange(room);
						_editor.RoomPropertiesChange(room.AlternateOpposite);
					}
				}
			}
			else if (alternateGroupIndex != -1)
			{
				// Create a new flipped room
				EditorActions.AlternateRoomEnable(room, alternateGroupIndex);
			}

			// Update combo box even if nothing changed internally
			// to correct invalid user input
			EditorEventRaised(new Editor.RoomPropertiesChangedEvent { Room = room });
		}
	}

	public int SelectedReverb
	{
		get => _editor.SelectedRoom.Properties.Reverberation;
		set
		{
			if (_editor.SelectedRoom.Properties.Reverberation == value)
				return;

			_editor.SelectedRoom.Properties.Reverberation = (byte)value;
			_editor.RoomPropertiesChange(_editor.SelectedRoom);
		}
	}

	public int SelectedPortalShade
	{
		get => (int)_editor.SelectedRoom.Properties.LightInterpolationMode;
		set
		{
			if (_editor.SelectedRoom.Properties.LightInterpolationMode == (RoomLightInterpolationMode)value)
				return;

			_editor.SelectedRoom.Properties.LightInterpolationMode = (RoomLightInterpolationMode)value;
			_editor.RoomPropertiesChange(_editor.SelectedRoom);
		}
	}

	public int SelectedEffect
	{
		get => (int)_editor.SelectedRoom.Properties.LightEffect;
		set
		{
			if (_editor.SelectedRoom.Properties.LightEffect == (RoomLightEffect)value)
				return;

			_editor.SelectedRoom.Properties.LightEffect = (RoomLightEffect)value;
			_editor.RoomPropertiesChange(_editor.SelectedRoom);
		}
	}

	public byte EffectStrength
	{
		get => _editor.SelectedRoom.Properties.LightEffectStrength;
		set
		{
			_editor.SelectedRoom.Properties.LightEffectStrength = value;
			_editor.RoomPropertiesChange(_editor.SelectedRoom);
		}
	}

	public IReadOnlyList<string> TagsAutoCompleteData => _editor.Level.Rooms
		.Where(room => room?.ExistsInLevel == true)
		.SelectMany(room => room.Properties.Tags)
		.Distinct()
		.ToList();

	public bool Hidden => _editor.SelectedRoom.Properties.Hidden;

	public Color AmbientLightColor => _editor.SelectedRoom.Properties.AmbientLight.ToWPFColor();

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

	private readonly Editor _editor;

	public ObservableCollection<string> FlipMaps { get; } = new();

	public RoomOptionsViewModel(Editor editor)
	{
		_editor = editor;
		_editor.EditorEventRaised += EditorEventRaised;
		RepopulateFlipMaps();

		EditRoomNameCommand = CommandHandler.GetCommand("EditRoomName", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		AddNewRoomCommand = CommandHandler.GetCommand("AddNewRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DuplicateRoomCommand = CommandHandler.GetCommand("DuplicateRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		DeleteRoomsCommand = CommandHandler.GetCommand("DeleteRooms", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		CropRoomCommand = CommandHandler.GetCommand("CropRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MoveRoomUpCommand = CommandHandler.GetCommand("MoveRoomUp", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SplitRoomCommand = CommandHandler.GetCommand("SplitRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		MoveRoomDownCommand = CommandHandler.GetCommand("MoveRoomDown", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		SelectPreviousRoomCommand = CommandHandler.GetCommand("SelectPreviousRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		LockRoomCommand = CommandHandler.GetCommand("LockRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		HideRoomCommand = CommandHandler.GetCommand("HideRoom", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
		EditAmbientLightCommand = CommandHandler.GetCommand("EditAmbientLight", new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor));
	}

	public void Cleanup()
	{
		_editor.EditorEventRaised -= EditorEventRaised;
	}

	private void RepopulateFlipMaps()
	{
		int index = FlipMaps.Count > 0 ? FlipMaps.IndexOf(FlipMaps.FirstOrDefault()) : -1;

		FlipMaps.Clear();
		FlipMaps.Add("None");

		int flipmapCount = _editor.Level.Settings.GameVersion == TRVersion.Game.TombEngine ? byte.MaxValue : 15;

		for (int i = 0; i < flipmapCount; i++)
			FlipMaps.Add(i.ToString());
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		// Disable version-specific controls.
		if (obj is Editor.InitEvent or
			Editor.GameVersionChangedEvent or
			Editor.LevelChangedEvent)
		{
			var version = _editor.Level.Settings.GameVersion;
			bool isNGorTEN = version is TRVersion.Game.TRNG or TRVersion.Game.TombEngine;
			bool isTR1 = version.Native() == TRVersion.Game.TR1;
			bool isTEN = version is TRVersion.Game.TombEngine;

			SupportsHorizon = !isTR1 || _editor.Level.IsTRX;
			SupportsFlagOutside = !isTR1 || _editor.Level.IsTRX;
			SupportsFlagCold = isNGorTEN;
			SupportsFlagDamage = isNGorTEN;
			SupportsNoLensflare = version.SupportsLensflare();
			SupportsNoCaustics = isTEN;
			SupportsReverb = version.SupportsReverberation();

			OnPropertyChanged(nameof(RoomTypes));
			OnPropertyChanged(nameof(ReverbValues));
			OnPropertyChanged(nameof(Effects));
			RepopulateFlipMaps();

			SupportsLightEffect = !isTR1;
			SupportsLightEffectStrength = !isTR1;
		}

		// Update the room list.
		if (obj is Editor.InitEvent or Editor.RoomListChangedEvent)
		{
			int cachedRoomIndex = SelectedRoom;

			OnPropertyChanged(nameof(Rooms));

			SelectedRoom = cachedRoomIndex;
			OnPropertyChanged(nameof(SelectedRoom));
		}

		// Update tag list.
		if (obj is Editor.LevelChangedEvent or Editor.SelectedRoomChangedEvent)
			OnPropertyChanged(nameof(TagsAutoCompleteData));

		// Update all room property controls.
		if (obj is Editor.InitEvent || obj is Editor.SelectedRoomChangedEvent || obj is Editor.LevelChangedEvent ||
			_editor.IsSelectedRoomEvent(obj as Editor.RoomPropertiesChangedEvent))
		{
			if (obj is Editor.SelectedRoomChangedEvent)
				OnPropertyChanged(nameof(SelectedRoom));

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
			OnPropertyChanged(nameof(SelectedPortalShade));
			OnPropertyChanged(nameof(SelectedEffect));
			OnPropertyChanged(nameof(EffectStrength));
			OnPropertyChanged(nameof(Hidden));
			OnPropertyChanged(nameof(AmbientLightColor));

			Room room = _editor.SelectedRoom;

			if (room.AlternateBaseRoom != null)
			{
				CanLockRoom = false;
				Locked = room.AlternateBaseRoom.Properties.Locked;
			}
			else
			{
				CanLockRoom = true;
				Locked = room.Properties.Locked;
			}
		}
	}
}
