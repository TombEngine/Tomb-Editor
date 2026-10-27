using DarkUI.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using TombEditor.Controls.FlybyTimeline;
using TombEditor.Controls.FlybyTimeline.Sequence;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormFlybyCamera : DarkForm
    {
        private const float ChangeComparisonEpsilon = 0.0001f;
        private static readonly Size DefaultWindowSize = new Size(561, 461);
        private static readonly Size CompactWindowSize = new Size(205, 319);

        public bool IsNew { get; set; }
        public bool HasChanges { get; private set; }

        private readonly FlybyCameraInstance _flyByCamera;
        private readonly Editor _editor;

        // Original values for restoring on Cancel.
        private ushort _originalFlags;
        private ushort _originalSequence;
        private ushort _originalNumber;
        private short _originalTimer;
        private float _originalSpeed;
        private float _originalFov;
        private float _originalRoll;
        private float _originalRotationX;
        private float _originalRotationY;
        private float _originalDofDistance;
        private float _originalDofRange;
        private float _originalDofStrength;
        private DofMode _originalDofMode;

        private bool _isLoading;
        private bool _ownedPreview;
        private bool _restoreOriginalValuesOnClose;

        public FormFlybyCamera(FlybyCameraInstance flyByCamera)
        {
            _flyByCamera = flyByCamera;
            _editor = Editor.Instance;

            InitializeComponent();
            numFOV.Maximum = (decimal)FlybyConstants.MaxFlybyFieldOfViewDegrees;

            LoadWindowState();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            _restoreOriginalValuesOnClose = true;

            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormFlybyCamera_Load(object sender, EventArgs e)
        {
            SaveOriginalValues();

            _isLoading = true;

            var flagCheckBoxes = GetFlagCheckBoxes();

            for (int i = 0; i < flagCheckBoxes.Length; i++)
                flagCheckBoxes[i].Checked = FlybySequenceHelper.GetFlagBit(_flyByCamera.Flags, i);

            numSequence.Value = _flyByCamera.Sequence;
            numNumber.Value = _flyByCamera.Number;
            numTimer.Value = _flyByCamera.Timer;
            numSpeed.Value = ClampNumericValue(_flyByCamera.Speed, numSpeed);
            numFOV.Value = ClampNumericValue(_flyByCamera.Fov, numFOV);
            numRoll.Value = ClampNumericValue(_flyByCamera.Roll, numRoll);
            numRotationX.Value = ClampNumericValue(_flyByCamera.RotationX, numRotationX);
            numRotationY.Value = ClampNumericValue(_flyByCamera.RotationY, numRotationY);
            numDofDistance.Value = ClampNumericValue(_flyByCamera.DofDistance, numDofDistance);
            numDofRange.Value = ClampNumericValue(_flyByCamera.DofRange, numDofRange);
            numDofStrength.Value = ClampNumericValue(_flyByCamera.DofStrength, numDofStrength);
            comboDofMode.SelectedIndex = (int)_flyByCamera.DofMode;

            SetDofControlsVisible();

            if (_editor.Level.Settings.GameVersion is TRVersion.Game.TR5 or TRVersion.Game.TombEngine)
            {
                cbBit1.Text = "Vignette";
                cbBit4.Text = "Hide Lara";
                cbBit12.Text = "Make fade-in";
                cbBit13.Text = "Make fade-out";
            }

            _isLoading = false;

            // Subscribe to value changes for live preview.
            numFOV.ValueChanged += PreviewParameter_Changed;
            numRoll.ValueChanged += PreviewParameter_Changed;
            numRotationX.ValueChanged += PreviewParameter_Changed;
            numRotationY.ValueChanged += PreviewParameter_Changed;
            numDofDistance.ValueChanged += PreviewParameter_Changed;
            numDofRange.ValueChanged += PreviewParameter_Changed;
            numDofStrength.ValueChanged += PreviewParameter_Changed;
            comboDofMode.SelectedIndexChanged += PreviewParameter_Changed;

            // Start live camera preview. Only toggle if preview is not already active
            // (e.g. flyby timeline may have entered preview before this form was opened).
            if (!_editor.FlyMode)
            {
                if (_editor.CameraPreviewMode == CameraPreviewType.None)
                {
                    _editor.ToggleCameraPreview(true);
                    _ownedPreview = true;
                }

                _editor.CameraPreviewUpdated(_flyByCamera);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing &&
                !_restoreOriginalValuesOnClose &&
                DialogResult != DialogResult.OK)
            {
                AcceptPendingChanges();
                DialogResult = DialogResult.OK;
            }

            base.OnFormClosing(e);

            if (!e.Cancel)
                ConfigurationBase.SaveWindowProperties(this, _editor.Configuration);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_restoreOriginalValuesOnClose)
            {
                RestoreOriginalValues();

                if (!_ownedPreview && _editor.CameraPreviewMode != CameraPreviewType.None)
                    _editor.CameraPreviewUpdated(_flyByCamera);
            }

            // Only exit the preview mode if we were the one who entered it.
            if (_ownedPreview && _editor.CameraPreviewMode != CameraPreviewType.None)
                _editor.ToggleCameraPreview(false);

            base.OnFormClosed(e);
        }

        private void PreviewParameter_Changed(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            _flyByCamera.Fov = (float)numFOV.Value;
            _flyByCamera.Roll = (float)numRoll.Value;
            _flyByCamera.RotationX = (float)numRotationX.Value;
            _flyByCamera.RotationY = (float)numRotationY.Value;
            _flyByCamera.DofDistance = (float)numDofDistance.Value;
            _flyByCamera.DofRange = (float)numDofRange.Value;
            _flyByCamera.DofStrength = (float)numDofStrength.Value;
            _flyByCamera.DofMode = (DofMode)comboDofMode.SelectedIndex;

            _editor.CameraPreviewUpdated(_flyByCamera);
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            AcceptPendingChanges();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void AcceptPendingChanges()
        {
            HasChanges = HasPendingChanges();
            ApplyPendingValues(_flyByCamera);
        }

        private void LoadWindowState()
        {
            ConfigurationBase.LoadWindowProperties(this, _editor.Configuration);
            WindowState = FormWindowState.Normal;
            ApplyWindowSizeForGameVersion();
            ConfigurationBase.ClampWindowLocation(this);
        }

        private void ApplyWindowSizeForGameVersion()
        {
            Size = UsesCompactWindowLayout() ? CompactWindowSize : DefaultWindowSize;
        }

        private bool UsesCompactWindowLayout()
        {
            if (_editor.Level is null)
                return false;

            if (_editor.Level.IsTRX)
                return _editor.Level.Settings.TrxConvertFlybysToCinematicFrames;

            return _editor.Level.Settings.GameVersion.Native() <= TRVersion.Game.TR3;
        }

        private void SaveOriginalValues()
        {
            _originalFlags = _flyByCamera.Flags;
            _originalSequence = _flyByCamera.Sequence;
            _originalNumber = _flyByCamera.Number;
            _originalTimer = _flyByCamera.Timer;
            _originalSpeed = _flyByCamera.Speed;
            _originalFov = _flyByCamera.Fov;
            _originalRoll = _flyByCamera.Roll;
            _originalRotationX = _flyByCamera.RotationX;
            _originalRotationY = _flyByCamera.RotationY;
            _originalDofDistance = _flyByCamera.DofDistance;
            _originalDofRange = _flyByCamera.DofRange;
            _originalDofStrength = _flyByCamera.DofStrength;
            _originalDofMode = _flyByCamera.DofMode;
        }

        private void RestoreOriginalValues()
        {
            _flyByCamera.Flags = _originalFlags;
            _flyByCamera.Sequence = _originalSequence;
            _flyByCamera.Number = _originalNumber;
            _flyByCamera.Timer = _originalTimer;
            _flyByCamera.Speed = _originalSpeed;
            _flyByCamera.Fov = _originalFov;
            _flyByCamera.Roll = _originalRoll;
            _flyByCamera.RotationX = _originalRotationX;
            _flyByCamera.RotationY = _originalRotationY;
            _flyByCamera.DofDistance = _originalDofDistance;
            _flyByCamera.DofRange = _originalDofRange;
            _flyByCamera.DofStrength = _originalDofStrength;
            _flyByCamera.DofMode = _originalDofMode;
        }

        private bool HasPendingChanges()
        {
            var pendingCamera = new FlybyCameraInstance();
            ApplyPendingValues(pendingCamera);

            return pendingCamera.Flags != _originalFlags ||
                pendingCamera.Sequence != _originalSequence ||
                pendingCamera.Number != _originalNumber ||
                pendingCamera.Timer != _originalTimer ||
                !MathC.WithinEpsilon(pendingCamera.Speed, _originalSpeed, ChangeComparisonEpsilon) ||
                !MathC.WithinEpsilon(pendingCamera.Fov, _originalFov, ChangeComparisonEpsilon) ||
                !MathC.WithinEpsilon(pendingCamera.Roll, _originalRoll, ChangeComparisonEpsilon) ||
                !MathC.WithinEpsilon(pendingCamera.RotationX, _originalRotationX, ChangeComparisonEpsilon) ||
                !MathC.WithinEpsilon(pendingCamera.RotationY, _originalRotationY, ChangeComparisonEpsilon) ||
                !MathC.WithinEpsilon(pendingCamera.DofDistance, _originalDofDistance, ChangeComparisonEpsilon) ||
                !MathC.WithinEpsilon(pendingCamera.DofRange, _originalDofRange, ChangeComparisonEpsilon) ||
                !MathC.WithinEpsilon(pendingCamera.DofStrength, _originalDofStrength, ChangeComparisonEpsilon) ||
                pendingCamera.DofMode != _originalDofMode;
        }

        private void ApplyPendingValues(FlybyCameraInstance camera)
        {
            camera.Flags = CollectFlags();
            camera.Sequence = (ushort)numSequence.Value;
            camera.Number = (ushort)numNumber.Value;
            camera.Timer = (short)numTimer.Value;
            camera.Speed = (float)numSpeed.Value;
            camera.Fov = (float)numFOV.Value;
            camera.Roll = (float)numRoll.Value;
            camera.RotationX = (float)numRotationX.Value;
            camera.RotationY = (float)numRotationY.Value;
            camera.DofDistance = (float)numDofDistance.Value;
            camera.DofRange = (float)numDofRange.Value;
            camera.DofStrength = (float)numDofStrength.Value;
            camera.DofMode = (DofMode)comboDofMode.SelectedIndex;
        }

        private void SetDofControlsVisible()
        {
            Control[] dofControls =
            [
                labelDofMode,
                labelDofDistance,
                labelDofRange,
                labelDofStrength,
                comboDofMode,
                numDofDistance,
                numDofRange,
                numDofStrength
            ] ;

            foreach (var control in dofControls)
                control.Visible = _editor.Level.IsTombEngine;
        }

        private static decimal ClampNumericValue(float value, NumericUpDown numeric)
        {
            decimal fallbackValue = Math.Min(numeric.Maximum, Math.Max(numeric.Minimum, numeric.Value));

            if (!float.IsFinite(value))
                return fallbackValue;

            double min = (double)numeric.Minimum;
            double max = (double)numeric.Maximum;
            double clampedValue = Math.Min(max, Math.Max(min, value));

            return (decimal)clampedValue;
        }

        private ushort CollectFlags()
        {
            ushort flags = 0;

            var flagCheckBoxes = GetFlagCheckBoxes();

            for (int i = 0; i < flagCheckBoxes.Length; i++)
                flags = FlybySequenceHelper.SetFlagBit(flags, i, flagCheckBoxes[i].Checked);

            return flags;
        }

        private CheckBox[] GetFlagCheckBoxes() =>
        [
            cbBit0,
            cbBit1,
            cbBit2,
            cbBit3,
            cbBit4,
            cbBit5,
            cbBit6,
            cbBit7,
            cbBit8,
            cbBit9,
            cbBit10,
            cbBit11,
            cbBit12,
            cbBit13,
            cbBit14,
            cbBit15
        ];
    }
}
