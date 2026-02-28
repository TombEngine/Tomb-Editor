using TombLib.Graphics.Dx11Toolkit;
using System;
using System.Numerics;
using TombLib.Graphics;
using TombLib.LevelData;
using WadTool.Controls;

namespace WadTool
{
    public class GizmoStaticEditor : BaseGizmo
    {
        private readonly Configuration _configuration;
        private readonly PanelRenderingStaticEditor _control;

        public GizmoStaticEditor(Configuration configuration, GraphicsDevice device, Effect effect, PanelRenderingStaticEditor control)
            : base(device, effect)
        {
            _configuration = configuration;
            _control = control;
        }

        protected override void GizmoMove(Vector3 newPos) => _control.StaticPosition = newPos;
        protected override void GizmoMoveDelta(Vector3 delta) { }
        protected override void GizmoRotateX(float newAngle) => _control.StaticRotation = new Vector3(newAngle, _control.StaticRotation.Y, _control.StaticRotation.Z);
        protected override void GizmoRotateY(float newAngle) => _control.StaticRotation = new Vector3(_control.StaticRotation.X, newAngle, _control.StaticRotation.Z);
        protected override void GizmoRotateZ(float newAngle) => _control.StaticRotation = new Vector3(_control.StaticRotation.X, _control.StaticRotation.Y, newAngle);
        protected override void GizmoScaleX(float newScale)
        {
            // Set some limits to scale
            // TODO: object risks to be too small and to be not pickable. We should add some size check
            if (newScale < 1.0f)
                newScale = 1.0f;
            if (newScale > 128.0f)
                newScale = 128.0f;

            _control.StaticScale = newScale;
        }
        protected override void GizmoScaleY(float newScale) => GizmoScaleX(newScale);
        protected override void GizmoScaleZ(float newScale) => GizmoScaleX(newScale);

        protected override Vector3 Position => _control != null ? _control.StaticPosition : Vector3.Zero;
        protected override float RotationY => _control.StaticRotation.Y;
        protected override float RotationX => _control.StaticRotation.X;
        protected override float RotationZ => _control.StaticRotation.Z;
        protected override Vector3 Scale => new Vector3(_control.StaticScale);

        protected override float CentreCubeSize => _UIscale * _configuration.GizmoStatic_CenterCubeSize;
        protected override float TranslationConeSize => _UIscale * _configuration.GizmoStatic_TranslationConeSize;
        protected override float Size => _UIscale * _configuration.GizmoStatic_Size;
        protected override float ScaleCubeSize => _UIscale * _configuration.GizmoStatic_ScaleCubeSize;
        protected override float LineThickness => _UIscale * _configuration.GizmoStatic_LineThickness;

        protected override GizmoOrientation Orientation => GizmoOrientation.Normal;

        protected override bool SupportScale => true;
        protected override bool SupportTranslateX => true;
        protected override bool SupportTranslateY => true;
        protected override bool SupportTranslateZ => true;
        protected override bool SupportRotationY => true;
        protected override bool SupportRotationX => true;
        protected override bool SupportRotationZ => true;

        private float _UIscale => Math.Max(_control.Static.Mesh.CalculateBoundingSphere().Radius / (Level.SectorSizeUnit * 2), 0.1f);
    }
}
