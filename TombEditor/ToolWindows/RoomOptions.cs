using DarkUI.Docking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.ToolWindows
{
    public partial class RoomOptions : DarkToolWindow
    {
        private readonly Editor _editor;

        public RoomOptions()
        {
            InitializeComponent();
            CommandHandler.AssignCommandsToControls(Editor.Instance, this, toolTip);

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            // A hack to edit textbox size
            tbRoomTags.AutoSize = false;
            tbRoomTags.Height = 23;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private int GetFlipMapSelectedIndex(Room room)
        {
            return room.Alternated ? ((comboFlipMap.Items.Count - 1 <= room.AlternateGroup) ? -1 : room.AlternateGroup + 1) : 0;
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Disable version-specific controls
            if (obj is Editor.InitEvent ||
                obj is Editor.GameVersionChangedEvent ||
                obj is Editor.LevelChangedEvent)
            {
                bool isTR4orNG = _editor.Level.Settings.GameVersion.Native() == TRVersion.Game.TR4;
                bool isNGorTEN = _editor.Level.Settings.GameVersion is TRVersion.Game.TRNG or TRVersion.Game.TombEngine;
                bool supportsLensflare = _editor.Level.Settings.GameVersion.SupportsLensflare();
                bool supportsReverb = _editor.Level.Settings.GameVersion.SupportsReverberation();
                bool isTR1 = _editor.Level.Settings.GameVersion.Native() == TRVersion.Game.TR1;

                cbHorizon.Enabled = !isTR1 || _editor.Level.IsTRX;
                cbFlagOutside.Enabled = !isTR1 || _editor.Level.IsTRX;
                cbFlagCold.Enabled = isNGorTEN;
                cbFlagDamage.Enabled = isNGorTEN;
                cbNoLensflare.Enabled = supportsLensflare;
                comboReverberation.Enabled = supportsReverb;
                comboReverberation.SelectedIndexChanged -= comboReverberation_SelectedIndexChanged; // Prevent SelectedIndexChanged event from DataSource assignment in next line
                comboReverberation.DataSource = isTR4orNG && _editor.Level.Settings.GameEnableExtraReverbPresets ? StringEnums.ExtraReverberationTypes : StringEnums.ReverberationTypes;
                comboReverberation.SelectedIndexChanged += comboReverberation_SelectedIndexChanged;
                comboReverberation.SelectedIndex = _editor.SelectedRoom.Properties.Reverberation < comboReverberation.Items.Count ? _editor.SelectedRoom.Properties.Reverberation : -1;
                comboReverberation.DropDownWidth = isTR4orNG && _editor.Level.Settings.GameEnableExtraReverbPresets ? 121 : 71;
                comboLightEffect.Enabled = !isTR1;
                numLightEffectStrength.Enabled = !isTR1;

                RepopulateRoomTypes();
                RepopulateFlipmapList();

                comboFlipMap.SelectedIndex = GetFlipMapSelectedIndex(_editor.SelectedRoom);
            }

            // Update the room list
            if (obj is Editor.InitEvent || obj is Editor.RoomListChangedEvent)
            {
                // Adjust the amount of entries in the combo list
                while (comboRoom.Items.Count > _editor.Level.Rooms.GetLength(0))
                    comboRoom.Items.RemoveAt(comboRoom.Items.Count - 1);
                while (comboRoom.Items.Count < _editor.Level.Rooms.GetLength(0))
                    comboRoom.Items.Add("");

                // Update the room list
                for (int i = 0; i < _editor.Level.Rooms.GetLength(0); i++)
                    if (_editor.Level.Rooms[i] != null)
                        comboRoom.Items[i] = i + ": " + _editor.Level.Rooms[i].Name;
                    else
                        comboRoom.Items[i] = i + ": --- Empty room ---";
            }

            // Update taglist
            if (obj is Editor.LevelChangedEvent || obj is Editor.SelectedRoomChangedEvent)
            {
                tbRoomTags.AutocompleteWords.Clear();
                foreach (var room in (_editor.Level.Rooms))
                    if (room != null && room.ExistsInLevel)
                        tbRoomTags.AutocompleteWords.AddRange(room.Properties.Tags.Except(tbRoomTags.AutocompleteWords));
            }

            // Update the room property controls
            if (obj is Editor.InitEvent || obj is Editor.SelectedRoomChangedEvent || obj is Editor.LevelChangedEvent ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomPropertiesChangedEvent))
            {
                Room room = _editor.SelectedRoom;
                if (obj is Editor.InitEvent || obj is Editor.SelectedRoomChangedEvent)
                    comboRoom.SelectedIndex = _editor.Level.Rooms.ReferenceIndexOf(room);

                // Update the state of other controls
                ReadRoomType();
                panelRoomAmbientLight.BackColor = (room.Properties.AmbientLight * new Vector3(0.5f, 0.5f, 0.5f)).ToWinFormsColor();
                comboReverberation.SelectedIndex = room.Properties.Reverberation < comboReverberation.Items.Count ? room.Properties.Reverberation : -1;
                comboPortalShade.SelectedIndex = (int)room.Properties.LightInterpolationMode;
                comboLightEffect.SelectedIndex = (int)room.Properties.LightEffect;
                numLightEffectStrength.Value = room.Properties.LightEffectStrength;
                cbFlagCold.Checked = room.Properties.FlagCold;
                cbFlagDamage.Checked = room.Properties.FlagDamage;
                cbFlagOutside.Checked = room.Properties.FlagOutside;
                cbHorizon.Checked = room.Properties.FlagHorizon;
                cbNoLensflare.Checked = room.Properties.FlagNoLensflare;
                cbNoPathfinding.Checked = room.Properties.FlagExcludeFromPathFinding;
                butHidden.Checked = room.Properties.Hidden;

                if (!tbRoomTags.ReadOnly) // Only update tags field if we're not in the process of editing
                {
                    if (room.Properties.Tags.Count > 0)
                        tbRoomTags.Text = string.Join(" ", room.Properties.Tags);
                    else
                        tbRoomTags.Text = "";
                }

                if (room.AlternateBaseRoom != null)
                {
                    butLocked.Enabled = false;
                    butLocked.Checked = room.AlternateBaseRoom.Properties.Locked;
                }
                else
                {
                    butLocked.Enabled = true;
                    butLocked.Checked = room.Properties.Locked;
                }

                comboFlipMap.SelectedIndex = GetFlipMapSelectedIndex(room);
            }

            // Activate default control
            if (obj is Editor.DefaultControlActivationEvent)
            {
                if (DockPanel != null && ((Editor.DefaultControlActivationEvent)obj).ContainerName == GetType().Name)
                {
                    MakeActive();
                    comboRoom.Search();
                }
            }

            // Update tooltip texts
            if (obj is Editor.ConfigurationChangedEvent)
            {
                if (((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
                    CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);
            }
        }

        private void RepopulateFlipmapList()
        {
            var index = comboFlipMap.SelectedIndex;

            comboFlipMap.Items.Clear();
            comboFlipMap.Items.Add("None");

            int flipmapCount = (_editor.Level.Settings.GameVersion == TRVersion.Game.TombEngine) ? byte.MaxValue : 15;

            for (int i = 0; i < flipmapCount; i++)
                comboFlipMap.Items.Add(i.ToString());

            comboFlipMap.SelectedIndex = (comboFlipMap.Items.Count - 1 <= index) ? -1 : index;
        }

        private void RepopulateRoomTypes()
        {
            // Repopulate room type
            comboRoomType.Items.Clear();
            comboRoomType.Items.Add("Normal");
            comboRoomType.Items.Add("Water");

            if (_editor.Level.Settings.GameVersion == TRVersion.Game.TR3 ||
                _editor.Level.IsTRX ||
                _editor.Level.IsNG ||
                _editor.Level.IsTombEngine)
                comboRoomType.Items.Add("Quicksand");

            if (_editor.Level.IsNG)
                StringEnums.NGRoomTypes.ForEach(i => comboRoomType.Items.Add(i));

            ReadRoomType();

            // Repopulate room effect type
            bool isTR2 = _editor.Level.Settings.GameVersion.Native() == TRVersion.Game.TR2;
            var list = new List<string>()
            {
                "None",
                "Default",
                "Reflection",
                "Glow",
                isTR2 ? "Flicker" : "Move",       // Show as flicker for TR2
                isTR2 ? "Sunset" : "Glow & Move", // Show as sunset for TR2
                "Mist"
            };

            var backupIndex = comboLightEffect.SelectedIndex;
            comboLightEffect.Items.Clear();
            list.ForEach(i => comboLightEffect.Items.Add(i));
            comboLightEffect.SelectedIndex = backupIndex;
        }

        private void WriteRoomType()
        {
            RoomType newType;
            byte newStrength = 0;

            switch (comboRoomType.SelectedIndex)
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
                    if (comboRoomType.SelectedIndex <= 6)
                    {
                        newType = RoomType.Rain;
                        newStrength = (byte)(comboRoomType.SelectedIndex - 3);
                    }
                    else
                    {
                        newType = RoomType.Snow;
                        newStrength = (byte)(comboRoomType.SelectedIndex - 7);
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

        private void ReadRoomType()
        {
            var room = _editor.SelectedRoom;

            // Cleverly switch room types dependent on game version.
            // We disable rain/snow types for TombEngine because it is expected to set these options with triggers and/or script.

            int roomType = -1;
            if (room.Properties.Type == RoomType.Quicksand &&
                (_editor.Level.Settings.GameVersion != TRVersion.Game.TR3 &&
                 _editor.Level.Settings.GameVersion != TRVersion.Game.TRNG &&
                 _editor.Level.Settings.GameVersion != TRVersion.Game.TombEngine &&
                 !_editor.Level.IsTRX))
                roomType = -1;
            else if ((room.Properties.Type == RoomType.Rain || room.Properties.Type == RoomType.Snow) &&
                     _editor.Level.Settings.GameVersion != TRVersion.Game.TRNG)
                roomType = -1;
            else
            {
                switch (room.Properties.Type)
                {
                    case RoomType.Normal:
                        roomType = 0;
                        break;
                    case RoomType.Water:
                        roomType = 1;
                        break;
                    case RoomType.Quicksand:
                        roomType = 2;
                        break;
                    case RoomType.Rain:
                        roomType = 3 + room.Properties.TypeStrength;
                        break;
                    case RoomType.Snow:
                        roomType = 7 + room.Properties.TypeStrength;
                        break;
                }
            }
            
            comboRoomType.SelectedIndex = roomType;

            // If selected type is -1 it means this room type is unsupported in current version. Throw a message about it.
            if (roomType == -1)
                _editor.SendMessage("Current room type is not supported in this engine version.\nChange it manually or switch engine version.", PopupType.Warning);

        }

        private void comboRoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            Room selectedRoom = _editor.Level.Rooms[comboRoom.SelectedIndex];
            if (selectedRoom == null)
                EditorActions.MakeNewRoom(comboRoom.SelectedIndex);
            else
                _editor.SelectRoom(selectedRoom);
        }

        private void comboFlipMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null || comboFlipMap.SelectedIndex == -1)
                return;

            var room = _editor.SelectedRoom;
            short alternateGroupIndex = (short)(comboFlipMap.SelectedIndex - 1);

            if (room.Alternated)
            {
                if (alternateGroupIndex == -1)
                { // Delete flipped room
                    EditorActions.AlternateRoomDisableWithWarning(room, this);
                }
                else
                { // Change flipped map number, not much to do here
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
            else
            {
                if (alternateGroupIndex != -1)
                {// Create a new flipped room
                    EditorActions.AlternateRoomEnable(room, alternateGroupIndex);
                }
            }

            // Update combo box even if nothing changed internally
            // to correct invalid user input
            EditorEventRaised(new Editor.RoomPropertiesChangedEvent { Room = room });
        }

        private void comboReverberation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom.Properties.Reverberation == comboReverberation.SelectedIndex)
                return;

            // Show extra tooltips in case reverb modes are extended
            if (_editor.Level.Settings.GameEnableExtraReverbPresets && comboReverberation.SelectedIndex > 0)
                toolTip.SetToolTip(comboReverberation, comboReverberation.Text);
            else
                toolTip.SetToolTip(comboReverberation, string.Empty);

            _editor.SelectedRoom.Properties.Reverberation = (byte)comboReverberation.SelectedIndex;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void numLightEffectStrength_ValueChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.Properties.LightEffectStrength = (byte)numLightEffectStrength.Value;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void comboRoomType_SelectedIndexChanged(object sender, EventArgs e) => WriteRoomType();

        private void comboLightEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom.Properties.LightEffect == (RoomLightEffect)comboLightEffect.SelectedIndex)
                return;

            _editor.SelectedRoom.Properties.LightEffect = (RoomLightEffect)comboLightEffect.SelectedIndex;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void comboPortalShade_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (_editor.SelectedRoom.Properties.LightInterpolationMode == (RoomLightInterpolationMode)comboPortalShade.SelectedIndex)
                return;

            _editor.SelectedRoom.Properties.LightInterpolationMode = (RoomLightInterpolationMode)comboPortalShade.SelectedIndex;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void TbTags_TextChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null)
                return;

            tbRoomTags.ReadOnly = true; // Prevent textbox from internally recalling this event

            _editor.SelectedRoom.Properties.Tags = tbRoomTags.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            _editor.RoomPropertiesChange(_editor.SelectedRoom);

            tbRoomTags.ReadOnly = false; // Re-enable editing
        }
    }
}
