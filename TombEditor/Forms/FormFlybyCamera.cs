using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormFlybyCamera : DarkForm
    {
        public bool IsNew { get; set; }

        private readonly FlybyCameraInstance _flyByCamera;
        private readonly Editor _editor;

        public FormFlybyCamera(FlybyCameraInstance flyByCamera)
        {
            _flyByCamera = flyByCamera;
            _editor = Editor.Instance;

            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormFlybyCamera_Load(object sender, EventArgs e)
        {
            cbBit0.Checked = (_flyByCamera.Flags & (1 << 0)) != 0;
            cbBit1.Checked = (_flyByCamera.Flags & (1 << 1)) != 0;
            cbBit2.Checked = (_flyByCamera.Flags & (1 << 2)) != 0;
            cbBit3.Checked = (_flyByCamera.Flags & (1 << 3)) != 0;
            cbBit4.Checked = (_flyByCamera.Flags & (1 << 4)) != 0;
            cbBit5.Checked = (_flyByCamera.Flags & (1 << 5)) != 0;
            cbBit6.Checked = (_flyByCamera.Flags & (1 << 6)) != 0;
            cbBit7.Checked = (_flyByCamera.Flags & (1 << 7)) != 0;
            cbBit8.Checked = (_flyByCamera.Flags & (1 << 8)) != 0;
            cbBit9.Checked = (_flyByCamera.Flags & (1 << 9)) != 0;
            cbBit10.Checked = (_flyByCamera.Flags & (1 << 10)) != 0;
            cbBit11.Checked = (_flyByCamera.Flags & (1 << 11)) != 0;
            cbBit12.Checked = (_flyByCamera.Flags & (1 << 12)) != 0;
            cbBit13.Checked = (_flyByCamera.Flags & (1 << 13)) != 0;
            cbBit14.Checked = (_flyByCamera.Flags & (1 << 14)) != 0;
            cbBit15.Checked = (_flyByCamera.Flags & (1 << 15)) != 0;

            numSequence.Value = _flyByCamera.Sequence;
            numNumber.Value = _flyByCamera.Number;
            numTimer.Value = _flyByCamera.Timer;
            numSpeed.Value = (decimal)_flyByCamera.Speed;
            numFOV.Value = (decimal)_flyByCamera.Fov;
            numRoll.Value = (decimal)_flyByCamera.Roll;
            numRotationX.Value = (decimal)_flyByCamera.RotationX;
            numRotationY.Value = (decimal)_flyByCamera.RotationY;

            if (_editor.Level.Settings.GameVersion is TRVersion.Game.TR5 or TRVersion.Game.TombEngine)
            {
                cbBit1.Text = "Vignette";
                cbBit4.Text = "Hide Lara";
                cbBit12.Text = "Make fade-in";
                cbBit13.Text = "Make fade-out";
            }
            else if (_editor.Level.Settings.GameVersion.Native() <= TRVersion.Game.TR3)
            {
                Size = new System.Drawing.Size(205, 319);
            }
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            ushort flags = 0;
            flags |= (ushort)(cbBit0.Checked ? 1 << 0 : 0);
            flags |= (ushort)(cbBit1.Checked ? 1 << 1 : 0);
            flags |= (ushort)(cbBit2.Checked ? 1 << 2 : 0);
            flags |= (ushort)(cbBit3.Checked ? 1 << 3 : 0);
            flags |= (ushort)(cbBit4.Checked ? 1 << 4 : 0);
            flags |= (ushort)(cbBit5.Checked ? 1 << 5 : 0);
            flags |= (ushort)(cbBit6.Checked ? 1 << 6 : 0);
            flags |= (ushort)(cbBit7.Checked ? 1 << 7 : 0);
            flags |= (ushort)(cbBit8.Checked ? 1 << 8 : 0);
            flags |= (ushort)(cbBit9.Checked ? 1 << 9 : 0);
            flags |= (ushort)(cbBit10.Checked ? 1 << 10 : 0);
            flags |= (ushort)(cbBit11.Checked ? 1 << 11 : 0);
            flags |= (ushort)(cbBit12.Checked ? 1 << 12 : 0);
            flags |= (ushort)(cbBit13.Checked ? 1 << 13 : 0);
            flags |= (ushort)(cbBit14.Checked ? 1 << 14 : 0);
            flags |= (ushort)(cbBit15.Checked ? 1 << 15 : 0);
            _flyByCamera.Flags = flags;

            _flyByCamera.Sequence = (ushort)numSequence.Value;
            _flyByCamera.Number = (ushort)numNumber.Value;
            _flyByCamera.Timer = (short)numTimer.Value;
            _flyByCamera.Speed = (float)numSpeed.Value;
            _flyByCamera.Fov = (float)numFOV.Value;
            _flyByCamera.Roll = (float)numRoll.Value;
            _flyByCamera.RotationX = (float)numRotationX.Value;
            _flyByCamera.RotationY = (float)numRotationY.Value;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
