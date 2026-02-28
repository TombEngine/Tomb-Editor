using TombLib.Graphics.Dx11Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib.Graphics;
using TombLib.LevelData;
using WadTool.Controls;

namespace WadTool
{
    public class GizmoStaticEditorLight : BaseGizmo
    {
        private readonly Configuration _configuration;
        private readonly PanelRenderingStaticEditor _control;

        public GizmoStaticEditorLight(Configuration configuration, GraphicsDevice device, Effect effect, PanelRenderingStaticEditor control)
            : base(device, effect)
        {
            _configuration = configuration;
            _control = control;
        }

        protected override void GizmoMove(Vector3 newPos)
        {
            _control.SelectedLight.Position = newPos;
            _control.UpdateLights();
        }
        protected override void GizmoMoveDelta(Vector3 delta) { }
        protected override void GizmoRotateX(float newAngle) { }
        protected override void GizmoRotateY(float newAngle) { }
        protected override void GizmoRotateZ(float newAngle) { }
        protected override void GizmoScaleX(float newScale)
        {
            // Set some limits to scale
            // TODO: object risks to be too small and to be not pickable. We should add some size check
            if (newScale < 1.0f)
                newScale = 1.0f;
            if (newScale > 128.0f)
                newScale = 128.0f;

            _control.SelectedLight.Radius = newScale;
            _control.UpdateLights();
        }
        protected override void GizmoScaleY(float newScale) => GizmoScaleX(newScale);
        protected override void GizmoScaleZ(float newScale) => GizmoScaleX(newScale);

        protected override Vector3 Position => _control != null && _control.SelectedLight != null ? _control.SelectedLight.Position : Vector3.Zero;
        protected override float RotationY => 0;
        protected override float RotationX => 0;
        protected override float RotationZ => 0;
        protected override Vector3 Scale => _control != null && _control.SelectedLight != null ? new Vector3(_control.SelectedLight.Radius) : Vector3.Zero;

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

        private float _UIscale => Math.Max(_control.Static.Mesh.CalculateBoundingSphere().Radius / (Level.SectorSizeUnit * 2), 0.1f);
    }
}
