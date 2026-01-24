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
            // Extract base name from full name (remove _number suffix if present)
            string baseName = _wayPoint.Name;
            if (!_wayPoint.IsSingularType())
            {
                int lastUnderscore = baseName.LastIndexOf('_');
                if (lastUnderscore >= 0)
                {
                    string suffix = baseName.Substring(lastUnderscore + 1);
                    if (ushort.TryParse(suffix, out _))
                    {
                        baseName = baseName.Substring(0, lastUnderscore);
                    }
                }
            }
            
            txtName.Text = baseName;
            numNumber.Value = _wayPoint.Number;
            cmbType.SelectedIndex = (int)_wayPoint.Type;
            numDimension1.Value = (decimal)_wayPoint.Radius1;
            numDimension2.Value = (decimal)_wayPoint.Radius2;
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

            // Number field only for multi-point types - disable instead of hide
            numNumber.Enabled = !isSingular;

            // Dimension fields only for shape types - disable instead of hide
            bool requiresDimension = type == WayPointType.Circle ||
                                 type == WayPointType.Ellipse ||
                                 type == WayPointType.Square ||
                                 type == WayPointType.Rectangle;

            numDimension1.Enabled = requiresDimension;

            // Dimension2 only for ellipse and rectangle - disable instead of hide
            bool requiresTwoDimensions = type == WayPointType.Ellipse ||
                                    type == WayPointType.Rectangle;

            numDimension2.Enabled = requiresTwoDimensions;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            // Validate name is not empty
            string newName = txtName.Text.Trim();
            if (string.IsNullOrEmpty(newName))
            {
                DarkMessageBox.Show(this, "WayPoint name cannot be empty.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var oldType = _wayPoint.Type;
            var newType = (WayPointType)cmbType.SelectedIndex;
            
            // Extract old base name for comparison
            string oldBaseName = _wayPoint.Name;
            if (!_wayPoint.IsSingularType())
            {
                int lastUnderscore = oldBaseName.LastIndexOf('_');
                if (lastUnderscore >= 0)
                {
                    string suffix = oldBaseName.Substring(lastUnderscore + 1);
                    if (ushort.TryParse(suffix, out _))
                    {
                        oldBaseName = oldBaseName.Substring(0, lastUnderscore);
                    }
                }
            }

            // Check for duplicate names only if name changed
            bool nameChanged = oldBaseName != newName;
            if (nameChanged && _editor?.Level != null)
            {
                foreach (var room in _editor.Level.ExistingRooms)
                {
                    foreach (var obj in room.Objects.OfType<WayPointInstance>())
                    {
                        if (obj != _wayPoint)
                        {
                            // Extract base name from existing waypoint
                            string existingBaseName = obj.Name;
                            if (!obj.IsSingularType())
                            {
                                int lastUnderscore = existingBaseName.LastIndexOf('_');
                                if (lastUnderscore >= 0)
                                {
                                    string suffix = existingBaseName.Substring(lastUnderscore + 1);
                                    if (ushort.TryParse(suffix, out _))
                                    {
                                        existingBaseName = existingBaseName.Substring(0, lastUnderscore);
                                    }
                                }
                            }
                            
                            if (existingBaseName == newName)
                            {
                                DarkMessageBox.Show(this, $"A WayPoint with the name '{newName}' already exists.", "Duplicate Name",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }
                }
            }

            // Update waypoint properties
            _wayPoint.Name = newName;
            _wayPoint.Number = (ushort)numNumber.Value;
            _wayPoint.Type = newType;
            _wayPoint.Radius1 = (float)numDimension1.Value;
            _wayPoint.Radius2 = (float)numDimension2.Value;
            _wayPoint.RotationX = (float)numRotationX.Value;
            _wayPoint.RotationY = (float)numRotationY.Value;
            _wayPoint.Roll = (float)numRoll.Value;

            // Batch type update: if type changed, update all waypoints with the same name
            if (oldType != newType && _editor?.Level != null)
            {
                foreach (var room in _editor.Level.ExistingRooms)
                {
                    foreach (var obj in room.Objects.OfType<WayPointInstance>())
                    {
                        if (obj != _wayPoint)
                        {
                            // Extract base name
                            string objBaseName = obj.Name;
                            if (!obj.IsSingularType())
                            {
                                int lastUnderscore = objBaseName.LastIndexOf('_');
                                if (lastUnderscore >= 0)
                                {
                                    string suffix = objBaseName.Substring(lastUnderscore + 1);
                                    if (ushort.TryParse(suffix, out _))
                                    {
                                        objBaseName = objBaseName.Substring(0, lastUnderscore);
                                    }
                                }
                            }
                            
                            if (objBaseName == newName)
                            {
                                obj.Type = newType;
                            }
                        }
                    }
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
