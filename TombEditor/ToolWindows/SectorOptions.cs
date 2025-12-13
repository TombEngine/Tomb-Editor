using DarkUI.Docking;
using NLog;
using System;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Utils;

namespace TombEditor.ToolWindows
{
    public partial class SectorOptions : DarkToolWindow
    {
        private readonly Editor _editor;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const float iconSwitchBrightnessThreshold = 0.8f;

        public SectorOptions()
        {
            InitializeComponent();
            CommandHandler.AssignCommandsToControls(Editor.Instance, this, toolTip);

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            panel2DGrid.Room = _editor.SelectedRoom;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.SelectedRoomChangedEvent)
                panel2DGrid.Room = ((Editor.SelectedRoomChangedEvent)obj).Current;

            // Update tooltip texts
            if (obj is Editor.ConfigurationChangedEvent)
            {
                if (((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
                    CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);
            }

            // Update color scheme on buttons
            if (obj is Editor.ConfigurationChangedEvent || obj is Editor.InitEvent)
            {
                butFloor.BackColor = _editor.Configuration.UI_ColorScheme.ColorFloor.ToWinFormsColor();
                butFloor.Image = (butFloor.BackColor.GetBrightness() > iconSwitchBrightnessThreshold) ? Properties.Resources.sectortype_Floor_neg_16 : Properties.Resources.sectortype_Floor_1_16;
                butCeiling.BackColor = _editor.Configuration.UI_ColorScheme.ColorFloor.ToWinFormsColor();
                butCeiling.Image = (butCeiling.BackColor.GetBrightness() > iconSwitchBrightnessThreshold) ? Properties.Resources.sectortype_Roof_neg_16 : Properties.Resources.sectortype_Roof_16;
                butBox.BackColor = _editor.Configuration.UI_ColorScheme.ColorBox.ToWinFormsColor();
                butBox.Image = (butBox.BackColor.GetBrightness() > iconSwitchBrightnessThreshold) ? Properties.Resources.sectortype_Box_neg_16 : Properties.Resources.sectortype_Box_16;
                butNotWalkableBox.BackColor = _editor.Configuration.UI_ColorScheme.ColorNotWalkable.ToWinFormsColor();
                butNotWalkableBox.Image = (butNotWalkableBox.BackColor.GetBrightness() > iconSwitchBrightnessThreshold) ? Properties.Resources.sectortype_NotWalkable_neg_16 : Properties.Resources.sectortype_NotWalkable_16;
                butMonkey.BackColor = _editor.Configuration.UI_ColorScheme.ColorMonkey.ToWinFormsColor();
                butMonkey.Image = (butMonkey.BackColor.GetBrightness() > iconSwitchBrightnessThreshold) ? Properties.Resources.sectortype_Monkey_neg_16 : Properties.Resources.sectortype_Monkey_16;
                butDeath.BackColor = _editor.Configuration.UI_ColorScheme.ColorDeath.ToWinFormsColor();
                butDeath.Image = (butDeath.BackColor.GetBrightness() > iconSwitchBrightnessThreshold) ? Properties.Resources.sectortype_Death_neg_16 : Properties.Resources.sectortype_Death_16;
                butPortal.BackColor = _editor.Configuration.UI_ColorScheme.ColorPortal.ToWinFormsColor();
                butPortal.Image = (butPortal.BackColor.GetBrightness() > iconSwitchBrightnessThreshold) ? Properties.Resources.sectortype_Portal_neg__16 : Properties.Resources.sectortype_Portal__16;
                butWall.BackColor = _editor.Configuration.UI_ColorScheme.ColorWall.ToWinFormsColor();
                butWall.Image = (butWall.BackColor.GetBrightness() > iconSwitchBrightnessThreshold) ? Properties.Resources.sectortype_Wall_neg_16 : Properties.Resources.sectortype_Wall_1_16;
            }

            // Disable version-specific controls
            if (obj is Editor.InitEvent ||
                obj is Editor.GameVersionChangedEvent ||
                obj is Editor.LevelChangedEvent)
            {
                bool climbingSupported = _editor.Level.Settings.GameVersion.SupportsClimbing();
                bool isTR345 = _editor.Level.Settings.GameVersion.Native() >= TRVersion.Game.TR3;

                butClimbNegativeX.Enabled = climbingSupported;
                butClimbNegativeZ.Enabled = climbingSupported;
                butClimbPositiveX.Enabled = climbingSupported;
                butClimbPositiveZ.Enabled = climbingSupported;
                butMonkey.Enabled = isTR345;
                butFlagBeetle.Enabled = isTR345;
                butFlagTriggerTriggerer.Enabled = isTR345;

                if (_editor.Level.Settings.GameVersion.Native() >= TRVersion.Game.TR4)
                {
                    butFlagTriggerTriggerer.Image = Properties.Resources.sectortype_TriggerTriggerer_16;
                    butFlagBeetle.Image = Properties.Resources.sectortype_Beetle_16;
                }
                else
                {

                    butFlagTriggerTriggerer.Image = Properties.Resources.sectortype_MinecartLeft_16;
                    butFlagBeetle.Image = Properties.Resources.sectortype_MinecartRight_16;
                }
            }
        }

        private void but_MouseEnter(object sender, EventArgs e)
        {
            SetSectorColoringInfoPriority(sender as Control);
        }

        private void SetSectorColoringInfoPriority(Control button)
        {
            if (!_editor.Configuration.UI_AutoSwitchSectorColoringInfo)
                return;

            if (button == butBox)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Box);
            else if (button == butDeath)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Death);
            else if (button == butMonkey)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Monkey);
            else if (button == butFlagBeetle)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Beetle);
            else if (button == butFlagTriggerTriggerer)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.TriggerTriggerer);
            else if (button == butNotWalkableBox)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.NotWalkableFloor);
            else if (button == butPortal)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Portal);
            else if (button == butForceSolidFloor)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.ForceFloorSolid);
            else if (button == butClimbNegativeX ||
                     button == butClimbNegativeZ ||
                     button == butClimbPositiveX ||
                     button == butClimbPositiveZ)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Climb);
            else if (button == butWall)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Wall);
            else
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Trigger);
        }
    }
}
