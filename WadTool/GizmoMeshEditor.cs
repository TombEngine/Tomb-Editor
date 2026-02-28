using TombLib.Graphics.Dx11Toolkit;
using System;
using System.Numerics;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using WadTool.Controls;

namespace WadTool
{
    public class GizmoMeshEditor : BaseGizmo
    {
        private readonly Configuration _configuration;
        private readonly PanelRenderingMesh _control;

        public GizmoMeshEditor(Configuration configuration, GraphicsDevice device, Effect effect, PanelRenderingMesh control)
            : base(device, effect)
        {
            _configuration = configuration;
            _control = control;
        }

        protected override void GizmoMove(Vector3 newPos)
        {
            _control.Mesh.BoundingSphere = new BoundingSphere(newPos, _control.Mesh.BoundingSphere.Radius);
            _control.CurrentElement = -1; // It's needed to externally update sphere values in form UI
        }

        protected override void GizmoScaleX(float newScale)
        {
            // Set some limits to scale
            // TODO: object risks to be too small and to be not pickable. We should add some size check
            if (newScale < 0.1f)
                newScale = 0.1f;

            _control.Mesh.BoundingSphere = new BoundingSphere(_control.Mesh.BoundingSphere.Center, newScale);
            _control.CurrentElement = -1; // It's needed to externally update sphere values in form UI
        }
        protected override void GizmoScaleY(float newScale) => GizmoScaleX(newScale);
        protected override void GizmoScaleZ(float newScale) => GizmoScaleX(newScale);

        protected override void GizmoMoveDelta(Vector3 delta) { }
        protected override void GizmoRotateX(float newAngle) { }
        protected override void GizmoRotateY(float newAngle) { }
        protected override void GizmoRotateZ(float newAngle) { }

        protected override Vector3 Position => _control != null && _control.Mesh != null ? _control.Mesh.BoundingSphere.Center : Vector3.Zero;
        protected override Vector3 Scale => _control != null && _control.Mesh != null ? new Vector3(_control.Mesh.BoundingSphere.Radius) : Vector3.One;

        protected override float RotationY => 0.0f;
        protected override float RotationX => 0.0f;
        protected override float RotationZ => 0.0f;

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
        protected override bool SupportRotationY => false;
        protected override bool SupportRotationX => false;
        protected override bool SupportRotationZ => false;

        private float _UIscale => Math.Max(_control.Mesh.CalculateBoundingSphere().Radius / (Level.SectorSizeUnit * 2), 0.1f);
    }
}
