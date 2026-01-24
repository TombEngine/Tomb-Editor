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
            cmbShape.SelectedIndex = (int)_wayPoint.Shape;
            numRadius1.Value = (decimal)_wayPoint.Radius1;
            numRadius2.Value = (decimal)_wayPoint.Radius2;
            numRotationX.Value = (decimal)_wayPoint.RotationX;
            numRotationY.Value = (decimal)_wayPoint.RotationY;
            numRoll.Value = (decimal)_wayPoint.Roll;

            UpdateRadiusVisibility();
        }

        private void cmbShape_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRadiusVisibility();
        }

        private void UpdateRadiusVisibility()
        {
            bool isEllipse = cmbShape.SelectedIndex == (int)WayPointShape.Ellipse;
            lblRadius2.Visible = isEllipse;
            numRadius2.Visible = isEllipse;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            _wayPoint.Name = txtName.Text;
            _wayPoint.Sequence = (ushort)numSequence.Value;
            _wayPoint.Number = (ushort)numNumber.Value;
            _wayPoint.PathType = (PathType)cmbPathType.SelectedIndex;
            _wayPoint.Shape = (WayPointShape)cmbShape.SelectedIndex;
            _wayPoint.Radius1 = (float)numRadius1.Value;
            _wayPoint.Radius2 = (float)numRadius2.Value;
            _wayPoint.RotationX = (float)numRotationX.Value;
            _wayPoint.RotationY = (float)numRotationY.Value;
            _wayPoint.Roll = (float)numRoll.Value;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
