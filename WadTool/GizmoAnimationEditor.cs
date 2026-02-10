using System.Numerics;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Graphics;
using WadTool.Controls;
using TombLib;

namespace WadTool
{
    public class GizmoAnimationEditor : BaseGizmo
    {
        private readonly Configuration _configuration;
        private readonly PanelRenderingAnimationEditor _control;
        private readonly AnimationEditor _editor;

        // Pick-time state captured once per gizmo drag to avoid gimbal lock.
        // Using Euler values that were read back after a QuaternionToEuler round-trip
        // would cause flickering near ±90° pitch, because the decomposition is
        // numerically unstable there. By freezing the reference values at pick time
        // and computing a cumulative delta from them, the quaternion target stays
        // smooth for the entire drag.
        private Quaternion _pickQuaternion;
        private Vector3 _pickEuler;

        public GizmoAnimationEditor(AnimationEditor editor, GraphicsDevice device,
                                    Effect effect, PanelRenderingAnimationEditor control)
            : base(device, effect)
        {
            _editor = editor;
            _configuration = editor.Tool.Configuration;
            _control = control;
        }

        protected override void GizmoMove(Vector3 newPos) { } // Absorb event
        protected override void GizmoRotateX(float newAngle) => GizmoRotate(GizmoMode.RotateX, newAngle);
        protected override void GizmoRotateY(float newAngle) => GizmoRotate(GizmoMode.RotateY, newAngle);
        protected override void GizmoRotateZ(float newAngle) => GizmoRotate(GizmoMode.RotateZ, newAngle);

        private void GizmoRotate(GizmoMode mode, float newAngle)
        {
            if (_control != null)
            {
                var model = _control.Model;
                var animation = _editor.CurrentAnim;
                if (animation == null || _control.SelectedMesh == null)
                    return;

                var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);

                // Capture pick-time state on the first call after each new gizmo pick.
                // MadeChanges is reset to false by the AnimationEditorGizmoPickedEvent,
                // so it is false exactly on the first rotation call of a new drag.
                if (!_editor.MadeChanges)
                {
                    _pickQuaternion = _editor.CurrentKeyFrame.Quaternions[meshIndex];
                    _pickEuler = _editor.CurrentKeyFrame.Rotations[meshIndex];
                }

                var axis = Vector3.Zero;
                float totalDelta = 0f;

                // Compute the cumulative angle change since pick time.
                // _pickEuler is a stable, frozen snapshot — it never suffers from
                // the QuaternionToEuler instability that the live Rotations[] values do.
                switch (mode)
                {
                    case GizmoMode.RotateX: totalDelta = newAngle - _pickEuler.X; axis = Vector3.UnitX; break;
                    case GizmoMode.RotateY: totalDelta = newAngle - _pickEuler.Y; axis = Vector3.UnitY; break;
                    case GizmoMode.RotateZ: totalDelta = newAngle - _pickEuler.Z; axis = Vector3.UnitZ; break;
                }

                // Apply the full cumulative delta to the stable pick-time quaternion.
                // This avoids the old approach of computing an incremental delta from
                // the current (potentially jumped) Euler values every frame.
                var targetQuat = _pickQuaternion * Quaternion.CreateFromAxisAngle(axis, totalDelta);
                _editor.UpdateTransformQuaternion(meshIndex, targetQuat, _editor.CurrentKeyFrame.Translations[0]);

                _control.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
                _control.Invalidate();
            }
        }

        protected override void GizmoScaleX(float newScale) { }
        protected override void GizmoScaleY(float newScale) { }
        protected override void GizmoScaleZ(float newScale) { }

        protected override void GizmoMoveDelta(Vector3 delta)
        {
            if (_control != null)
            {
                var model = _control.Model;
                var animation = _editor.CurrentAnim;
                if (animation == null || _control.SelectedMesh == null)
                    return;

                var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);
                var translationVector = _editor.CurrentKeyFrame.Translations[meshIndex];
                translationVector += delta;
                _editor.UpdateTransform(meshIndex, _editor.CurrentKeyFrame.Rotations[meshIndex], translationVector);

                _control.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
                _control.Invalidate();
            }
        }

        protected override Vector3 Position
        {
            get
            {
                if (_control != null)
                {
                    var model = _control.Model;
                    var animation = _editor.CurrentAnim;
                    if (animation == null || _control.SelectedMesh == null)
                        return Vector3.Zero;
                    var meshIndex = model.Meshes.IndexOf(_control.SelectedMesh);
                    var centre = new Vector3(0, 0, 0);
                    return MathC.HomogenousTransform(centre, model.AnimationTransforms[meshIndex]);
                }
                else
                    return Vector3.Zero;
            }
        }

        protected override float RotationY
        {
            get
            {
                if (_control == null || _editor.CurrentAnim == null || _control.SelectedMesh == null ||
                    _editor.CurrentFrameIndex >= _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count)
                    return 0;
                var meshIndex = _control.Model.Meshes.IndexOf(_control.SelectedMesh);
                return _editor.CurrentKeyFrame.Rotations[meshIndex].Y;
            }
        }

        protected override float RotationX
        {
            get
            {
                if (_control == null || _editor.CurrentAnim == null || _control.SelectedMesh == null ||
                    _editor.CurrentFrameIndex >= _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count)
                    return 0;
                var meshIndex = _control.Model.Meshes.IndexOf(_control.SelectedMesh);
                return _editor.CurrentKeyFrame.Rotations[meshIndex].X;
            }
        }

        protected override float RotationZ
        {
            get
            {
                if (_control == null || _editor.CurrentAnim == null || _control.SelectedMesh == null ||
                    _editor.CurrentFrameIndex >= _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count)
                    return 0;
                var meshIndex = _control.Model.Meshes.IndexOf(_control.SelectedMesh);
                return _editor.CurrentKeyFrame.Rotations[meshIndex].Z;
            }
        }

        protected override Vector3 Scale => Vector3.One;
        protected override GizmoOrientation Orientation => GizmoOrientation.Normal;

        protected override float CentreCubeSize => _configuration.GizmoAnimationEditor_CenterCubeSize;
        protected override float TranslationConeSize => _configuration.GizmoAnimationEditor_TranslationConeSize;
        protected override float Size => _configuration.GizmoAnimationEditor_Size;
        protected override float ScaleCubeSize => _configuration.GizmoAnimationEditor_ScaleCubeSize;
        protected override float LineThickness => _configuration.GizmoAnimationEditor_LineThickness;

        protected override bool SupportScale => false;
        protected override bool SupportTranslateX
        {
            get
            {
                return (_control != null &&
                        _control.SelectedMesh != null &&
                        _control.Model.Meshes.IndexOf(_control.SelectedMesh) == 0);
            }
        }
        protected override bool SupportTranslateY => SupportTranslateX;
        protected override bool SupportTranslateZ => SupportTranslateX;
        protected override bool SupportRotationY => true;
        protected override bool SupportRotationX => true;
        protected override bool SupportRotationZ => true;
    }
}
