using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        private FlybyCameraInstance _flyByCamera;
        private readonly Editor _editor;
        private List<FlybyCameraInstance> _sequenceCameras;
        private readonly Dictionary<FlybyCameraInstance, FlybyCameraInstance> _originalValues = new();

        public IReadOnlyList<FlybyCameraInstance> ChangedCameras => _originalValues
            .Where(pair => !CameraValuesMatch(pair.Key, pair.Value))
            .Select(pair => pair.Key)
            .ToList();

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
            _sequenceCameras = FlybySequenceHelper.GetCameras(_editor.Level, _flyByCamera.Sequence).ToList();
            SaveOriginalValues();

            LoadCameraValues();

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

            UpdateNavigationButtons();

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
            ApplyPendingValues(_flyByCamera);
            HasChanges = ChangedCameras.Count > 0;
        }

        private void butPrevious_Click(object sender, EventArgs e) => NavigateCamera(-1);

        private void butNext_Click(object sender, EventArgs e) => NavigateCamera(1);

        private void NavigateCamera(int direction)
        {
            int currentIndex = _sequenceCameras.IndexOf(_flyByCamera);
            int nextIndex = currentIndex + direction;

            if (nextIndex < 0 || nextIndex >= _sequenceCameras.Count)
                return;

            ApplyPendingValues(_flyByCamera);
            _flyByCamera = _sequenceCameras[nextIndex];
            LoadCameraValues();
            UpdateNavigationButtons();

            // The timeline mirrors editor flyby selection to both its camera selection and playhead.
            _editor.SelectedObject = _flyByCamera;

            if (!_editor.FlyMode && _editor.CameraPreviewMode != CameraPreviewType.None)
                _editor.CameraPreviewUpdated(_flyByCamera);
        }

        private void UpdateNavigationButtons()
        {
            int currentIndex = _sequenceCameras.IndexOf(_flyByCamera);
            butPrevious.Enabled = currentIndex > 0;
            butNext.Enabled = currentIndex >= 0 && currentIndex < _sequenceCameras.Count - 1;
        }

        private void LoadCameraValues()
        {
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
            _isLoading = false;
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
            foreach (var camera in _sequenceCameras)
                _originalValues[camera] = CloneCameraValues(camera);
        }

        private void RestoreOriginalValues()
        {
            foreach (var pair in _originalValues)
                CopyCameraValues(pair.Value, pair.Key);
        }

        private static FlybyCameraInstance CloneCameraValues(FlybyCameraInstance camera) => new()
        {
            Flags = camera.Flags,
            Sequence = camera.Sequence,
            Number = camera.Number,
            Timer = camera.Timer,
            Speed = camera.Speed,
            Fov = camera.Fov,
            Roll = camera.Roll,
            RotationX = camera.RotationX,
            RotationY = camera.RotationY,
            DofDistance = camera.DofDistance,
            DofRange = camera.DofRange,
            DofStrength = camera.DofStrength,
            DofMode = camera.DofMode
        };

        private static void CopyCameraValues(FlybyCameraInstance source, FlybyCameraInstance target)
        {
            target.Flags = source.Flags;
            target.Sequence = source.Sequence;
            target.Number = source.Number;
            target.Timer = source.Timer;
            target.Speed = source.Speed;
            target.Fov = source.Fov;
            target.Roll = source.Roll;
            target.RotationX = source.RotationX;
            target.RotationY = source.RotationY;
            target.DofDistance = source.DofDistance;
            target.DofRange = source.DofRange;
            target.DofStrength = source.DofStrength;
            target.DofMode = source.DofMode;
        }

        private static bool CameraValuesMatch(FlybyCameraInstance camera, FlybyCameraInstance original) =>
            camera.Flags == original.Flags &&
            camera.Sequence == original.Sequence &&
            camera.Number == original.Number &&
            camera.Timer == original.Timer &&
            MathC.WithinEpsilon(camera.Speed, original.Speed, ChangeComparisonEpsilon) &&
            MathC.WithinEpsilon(camera.Fov, original.Fov, ChangeComparisonEpsilon) &&
            MathC.WithinEpsilon(camera.Roll, original.Roll, ChangeComparisonEpsilon) &&
            MathC.WithinEpsilon(camera.RotationX, original.RotationX, ChangeComparisonEpsilon) &&
            MathC.WithinEpsilon(camera.RotationY, original.RotationY, ChangeComparisonEpsilon) &&
            MathC.WithinEpsilon(camera.DofDistance, original.DofDistance, ChangeComparisonEpsilon) &&
            MathC.WithinEpsilon(camera.DofRange, original.DofRange, ChangeComparisonEpsilon) &&
            MathC.WithinEpsilon(camera.DofStrength, original.DofStrength, ChangeComparisonEpsilon) &&
            camera.DofMode == original.DofMode;

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
