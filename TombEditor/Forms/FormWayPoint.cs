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
            numSequence.Value = _wayPoint.Sequence;
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
            WayPointType newType = (WayPointType)cmbType.SelectedIndex;
            WayPointType oldType = _wayPoint.Type;
            
            // If type changed to singular, reset number to 0
            bool newIsSingular = newType == WayPointType.Point ||
                                newType == WayPointType.Circle ||
                                newType == WayPointType.Ellipse ||
                                newType == WayPointType.Square ||
                                newType == WayPointType.Rectangle;
            
            bool oldIsSingular = oldType == WayPointType.Point ||
                                oldType == WayPointType.Circle ||
                                oldType == WayPointType.Ellipse ||
                                oldType == WayPointType.Square ||
                                oldType == WayPointType.Rectangle;
            
            // Reset number to 0 when changing to singular type from multi-point type
            if (newIsSingular && !oldIsSingular)
            {
                numNumber.Value = 0;
            }
            
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
                DarkMessageBox.Show(this, "Waypoint name cannot be empty.", "Validation Error",
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

            // Check for changes
            bool nameChanged = oldBaseName != newName;
            ushort currentNumber = (ushort)numNumber.Value;
            bool numberChanged = _wayPoint.Number != currentNumber;
            ushort currentSequence = (ushort)numSequence.Value;
            bool sequenceChanged = _wayPoint.Sequence != currentSequence;

            // Validation rules:
            // 1. Same name + same sequence can only exist if numbers are different
            // 2. Same name + different sequence is NOT allowed
            // 3. Different name + same sequence is NOT allowed
            // 4. Same sequence + same name + same number is NOT allowed
            // Always validate if any of these changed (removed the condition check)
            if (_editor?.Level != null)
            {
                foreach (var room in _editor.Level.ExistingRooms)
                {
                    foreach (var obj in room.Objects.OfType<WayPointInstance>())
                    {
                        if (obj == _wayPoint)
                            continue;

                        // Rule 1 & 4: Check for same name + same sequence
                        if (obj.BaseName == newName && obj.Sequence == currentSequence)
                        {
                            // Only allowed if numbers are different
                            if (obj.Number == currentNumber)
                            {
                                DarkMessageBox.Show(this,
                                    $"A waypoint with name '{newName}', sequence {currentSequence}, and number {currentNumber} already exists.",
                                    "Validation Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            // Different numbers - this is allowed
                        }
                        // Rule 2: Check for same name + different sequence
                        else if (obj.BaseName == newName && obj.Sequence != currentSequence)
                        {
                            DarkMessageBox.Show(this,
                                $"A waypoint with name '{newName}' already exists with a different sequence ({obj.Sequence}). The same name cannot be used with different sequence numbers.",
                                "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        // Rule 3: Check for different name + same sequence
                        else if (obj.BaseName != newName && obj.Sequence == currentSequence)
                        {
                            DarkMessageBox.Show(this,
                                $"A waypoint with sequence {currentSequence} already exists with a different name ('{obj.BaseName}'). The same sequence number cannot be used with different names.",
                                "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }

            // Batch type update: if type changed, update all waypoints with the same name/sequence pair
            if (oldType != newType && _editor?.Level != null)
            {
                var oldSequence = _wayPoint.Sequence;
                foreach (var room in _editor.Level.ExistingRooms)
                {
                    foreach (var obj in room.Objects.OfType<WayPointInstance>())
                    {
                        // Update all waypoints that share the same base name (which implies same sequence)
                        if (obj != _wayPoint && obj.BaseName == oldBaseName)
                        {
                            obj.Type = newType;
                        }
                    }
                }
            }

            // Generate the new full name that will be used
            string fullNewName = newName;
            ushort newNumber = (ushort)numNumber.Value;
            
            bool newIsSingular = newType == WayPointType.Point ||
                                newType == WayPointType.Circle ||
                                newType == WayPointType.Ellipse ||
                                newType == WayPointType.Square ||
                                newType == WayPointType.Rectangle;
            
            if (!newIsSingular)
            {
                fullNewName = newName + "_" + newNumber;
            }

            // Update waypoint properties
            _wayPoint.Name = newName;
            _wayPoint.Sequence = (ushort)numSequence.Value;
            _wayPoint.Number = newNumber;
            _wayPoint.Type = newType;
            _wayPoint.Radius1 = (float)numDimension1.Value;
            _wayPoint.Radius2 = (float)numDimension2.Value;
            _wayPoint.RotationX = (float)numRotationX.Value;
            _wayPoint.RotationY = (float)numRotationY.Value;
            _wayPoint.Roll = (float)numRoll.Value;

            // Batch type update: if type changed, update all waypoints with either:
            // 1. Same original base name OR
            // 2. Same sequence number
            if (oldType != newType && _editor?.Level != null)
            {
                var oldSequence = _wayPoint.Sequence;
                foreach (var room in _editor.Level.ExistingRooms)
                {
                    foreach (var obj in room.Objects.OfType<WayPointInstance>())
                    {
                        if (obj != _wayPoint && (obj.BaseName == oldBaseName || obj.Sequence == oldSequence))
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
