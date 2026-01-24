using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormWayPoint : DarkForm
    {
        public bool IsNew { get; set; }

        private readonly WayPointInstance _wayPoint;
        private readonly Editor _editor;

        public FormWayPoint(WayPointInstance wayPoint)
        {
            _wayPoint = wayPoint;
            _editor = Editor.Instance;

            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormWayPoint_Load(object sender, EventArgs e)
        {
            txtName.Text = _wayPoint.Name;
            numSequence.Value = _wayPoint.Sequence;
            numNumber.Value = _wayPoint.Number;
            cmbPathType.SelectedIndex = (int)_wayPoint.PathType;
            numRotationX.Value = (decimal)_wayPoint.RotationX;
            numRotationY.Value = (decimal)_wayPoint.RotationY;
            numRoll.Value = (decimal)_wayPoint.Roll;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            _wayPoint.Name = txtName.Text;
            _wayPoint.Sequence = (ushort)numSequence.Value;
            _wayPoint.Number = (ushort)numNumber.Value;
            _wayPoint.PathType = (PathType)cmbPathType.SelectedIndex;
            _wayPoint.RotationX = (float)numRotationX.Value;
            _wayPoint.RotationY = (float)numRotationY.Value;
            _wayPoint.Roll = (float)numRoll.Value;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
