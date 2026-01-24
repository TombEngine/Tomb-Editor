using System;
using System.Linq;
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
            txtName.Text = _wayPoint.BaseName;
            numSequence.Value = _wayPoint.Sequence;
            numNumber.Value = _wayPoint.Number;
            cmbType.SelectedIndex = (int)_wayPoint.Type;
            numRadius1.Value = (decimal)_wayPoint.Radius1;
            numRadius2.Value = (decimal)_wayPoint.Radius2;
            numRotationX.Value = (decimal)_wayPoint.RotationX;
            numRotationY.Value = (decimal)_wayPoint.RotationY;
            numRoll.Value = (decimal)_wayPoint.Roll;

            UpdateFieldVisibility();
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFieldVisibility();
        }

        private void UpdateFieldVisibility()
        {
            WayPointType type = (WayPointType)cmbType.SelectedIndex;
            
            // Check if this is a singular type
            bool isSingular = type == WayPointType.Point ||
                             type == WayPointType.Circle ||
                             type == WayPointType.Ellipse ||
                             type == WayPointType.Square ||
                             type == WayPointType.Rectangle;

            // Number field only for multi-point types
            lblNumber.Visible = !isSingular;
            numNumber.Visible = !isSingular;

            // Radius fields only for shape types
            bool requiresRadius = type == WayPointType.Circle ||
                                 type == WayPointType.Ellipse ||
                                 type == WayPointType.Square ||
                                 type == WayPointType.Rectangle;

            lblRadius1.Visible = requiresRadius;
            numRadius1.Visible = requiresRadius;

            // Radius2 only for ellipse and rectangle
            bool requiresTwoRadii = type == WayPointType.Ellipse ||
                                    type == WayPointType.Rectangle;

            lblRadius2.Visible = requiresTwoRadii;
            numRadius2.Visible = requiresTwoRadii;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            var oldType = _wayPoint.Type;
            var newType = (WayPointType)cmbType.SelectedIndex;
            var sequence = (ushort)numSequence.Value;

            _wayPoint.BaseName = txtName.Text;
            _wayPoint.Sequence = sequence;
            _wayPoint.Number = (ushort)numNumber.Value;
            _wayPoint.Type = newType;
            _wayPoint.Radius1 = (float)numRadius1.Value;
            _wayPoint.Radius2 = (float)numRadius2.Value;
            _wayPoint.RotationX = (float)numRotationX.Value;
            _wayPoint.RotationY = (float)numRotationY.Value;
            _wayPoint.Roll = (float)numRoll.Value;

            // Batch type update: if type changed, update all waypoints in the same sequence
            if (oldType != newType && _editor?.Level != null)
            {
                foreach (var room in _editor.Level.ExistingRooms)
                {
                    foreach (var obj in room.Objects.OfType<WayPointInstance>())
                    {
                        if (obj.Sequence == sequence && obj != _wayPoint)
                        {
                            obj.Type = newType;
                        }
                    }
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
