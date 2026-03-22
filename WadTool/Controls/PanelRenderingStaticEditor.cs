using DarkUI.Forms;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.Graphics.Primitives;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool.Controls
{
    public class PanelRenderingStaticEditor : RenderingPanel
    {
        public enum StaticEditorAction
        {
            Normal,
            PlaceLight
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Configuration { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadStatic Static { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; } = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 100, 1000000, (float)Math.PI / 4.0f);
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadLight SelectedLight { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawVisibilityBox { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawCollisionBox { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGrid { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGizmo { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawLights { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawNormals { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector3 StaticPosition { get; set; } = Vector3.Zero;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector3 StaticRotation { get; set; } = Vector3.Zero;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float StaticScale { get; set; } = 1.0f;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StaticEditorAction Action { get; set; }

        public Matrix4x4 GizmoTransform
        {
            get
            {
                return Matrix4x4.CreateScale(StaticScale) *
                       Matrix4x4.CreateFromYawPitchRoll(StaticRotation.Y, StaticRotation.X, StaticRotation.Z) *
                       Matrix4x4.CreateTranslation(StaticPosition);
            }
        }

        // General state
        private WadToolClass _tool;

        // Interaction state
        private float _lastX;
        private float _lastY;

        // Rendering state
        private RenderingTextureAllocator _fontTexture;
        private RenderingFont _fontDefault;

        // Legacy rendering state
        private GraphicsDevice _device;
        private DeviceManager _deviceManager;
        private RasterizerState _rasterizerWireframe;
        private GizmoStaticEditor _gizmo;
        private GizmoStaticEditorLight _gizmoLight;
        private GeometricPrimitive _plane;
        private GeometricPrimitive _sphere;
        private GeometricPrimitive _littleSphere;
        private WadRenderer _wadRenderer;
        private Buffer<SolidVertex> _vertexBufferVisibility;
        private Buffer<SolidVertex> _vertexBufferCollision;

        public void InitializeRendering(WadToolClass tool, DeviceManager deviceManager)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            base.InitializeRendering(deviceManager.Device, tool.Configuration.RenderingItem_Antialias);
            _tool = tool;

            // Actual "InitializeRendering"
            _fontTexture = deviceManager.Device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(512, 512, 2) });
            _fontDefault = deviceManager.Device.CreateFont(new RenderingFont.Description
            {
                FontName = _tool.Configuration.Rendering3D_FontName,
                FontSize = _tool.Configuration.Rendering3D_FontSize,
                FontIsBold = _tool.Configuration.Rendering3D_FontIsBold,
                TextureAllocator = _fontTexture
            });

            // Legacy rendering
            {
                _device = deviceManager.___LegacyDevice;
                _deviceManager = deviceManager;
                _wadRenderer = new WadRenderer(_device, false, true, 4096, 2048, false);
                new BasicEffect(_device); // This effect is used for editor special meshes like sinks, cameras, light meshes, etc
                _rasterizerWireframe = RasterizerState.New(_device, new SharpDX.Direct3D11.RasterizerStateDescription
                {
                    CullMode = SharpDX.Direct3D11.CullMode.None,
                    DepthBias = 0,
                    DepthBiasClamp = 0,
                    FillMode = SharpDX.Direct3D11.FillMode.Wireframe,
                    IsAntialiasedLineEnabled = true,
                    IsDepthClipEnabled = true,
                    IsFrontCounterClockwise = false,
                    IsMultisampleEnabled = true,
                    IsScissorEnabled = false,
                    SlopeScaledDepthBias = 0
                });
                _gizmo = new GizmoStaticEditor(_tool.Configuration, _device, _deviceManager.___LegacyEffects["Solid"], this);
                _gizmoLight = new GizmoStaticEditorLight(_tool.Configuration, _device, _deviceManager.___LegacyEffects["Solid"], this);
                _plane = GeometricPrimitive.GridPlane.New(_device, 8, 4);
                _littleSphere = GeometricPrimitive.Sphere.New(_device, 2 * 128.0f, 8);
                _sphere = GeometricPrimitive.Sphere.New(_device, 1024.0f, 6);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fontDefault?.Dispose();
                _gizmo?.Dispose();
                _gizmoLight?.Dispose();
                _plane?.Dispose();
                _sphere?.Dispose();
                _littleSphere?.Dispose();
                _rasterizerWireframe?.Dispose();
                _wadRenderer?.Dispose();
                _vertexBufferVisibility?.Dispose();
                _vertexBufferCollision?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override Vector4 ClearColor => Configuration.RenderingItem_BackgroundColor;

        protected override void OnDraw()
        {
            // To make sure things are in a defined state for legacy rendering...
            ((TombLib.Rendering.DirectX11.Dx11RenderingSwapChain)SwapChain).BindForce();
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            _device.SetDepthStencilState(_device.DepthStencilStates.Default);
            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
            _device.SetBlendState(_device.BlendStates.Opaque);

            var viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            var solidEffect = _deviceManager.___LegacyEffects["Solid"];

            if (DrawGrid)
            {
                _device.SetRasterizerState(_rasterizerWireframe);

                // Draw the grid
                _device.SetVertexBuffer(0, _plane.VertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _plane.VertexBuffer));
                _device.SetIndexBuffer(_plane.IndexBuffer, true);

                solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                solidEffect.Parameters["Color"].SetValue(Vector4.One);
                solidEffect.Techniques[0].Passes[0].Apply();

                _device.Draw(PrimitiveType.LineList, _plane.VertexBuffer.ElementCount);
            }

            if (DrawLights)
            {
                _device.SetRasterizerState(_rasterizerWireframe);

                foreach (var light in Static.Lights)
                {
                    // Draw the little sphere
                    _device.SetVertexBuffer(0, _littleSphere.VertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _littleSphere.VertexBuffer));
                    _device.SetIndexBuffer(_littleSphere.IndexBuffer, false);

                    var world = Matrix4x4.CreateTranslation(light.Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 0.0f, 1.0f));
                    solidEffect.Techniques[0].Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);

                    if (SelectedLight == light)
                    {
                        _device.SetVertexBuffer(0, _sphere.VertexBuffer);
                        _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _sphere.VertexBuffer));
                        _device.SetIndexBuffer(_sphere.IndexBuffer, false);

                        world = Matrix4x4.CreateScale(light.Radius * 2.0f) * Matrix4x4.CreateTranslation(light.Position);
                        solidEffect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                        solidEffect.Techniques[0].Passes[0].Apply();

                        _device.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);
                    }
                }

                _device.SetRasterizerState(_device.RasterizerStates.CullBack);
            }

            if (Static != null)
            {
                var model = _wadRenderer.GetStatic(Static);
                var effect = _deviceManager.___LegacyEffects["Model"];
                var world = GizmoTransform;

                effect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                effect.Parameters["Color"].SetValue(Vector4.One);
                effect.Parameters["StaticLighting"].SetValue(Static.Mesh.LightingType != WadMeshLightingType.Normals);
                effect.Parameters["ColoredVertices"].SetValue(_tool.DestinationWad.GameVersion == TombLib.LevelData.TRVersion.Game.TombEngine);
                effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                effect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    var mesh = model.Meshes[i];
                    mesh.UpdateBuffers(Camera.GetPosition());

                    _device.SetVertexBuffer(0, mesh.VertexBuffer);
                    _device.SetIndexBuffer(mesh.IndexBuffer, true);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, mesh.VertexBuffer));

                    effect.Parameters["ModelViewProjection"].SetValue((world * viewProjection).ToSharpDX());
                    effect.Techniques[0].Passes[0].Apply();

                    foreach (var submesh in mesh.Submeshes)
                    {
                        if (submesh.Value.Material.AdditiveBlending)
                            _device.SetBlendState(_device.BlendStates.Additive);
                        else
                            _device.SetBlendState(_device.BlendStates.Opaque);

                        if (submesh.Value.Material.DoubleSided)
                            _device.SetRasterizerState(_device.RasterizerStates.CullNone);
                        else
                            _device.SetRasterizerState(_device.RasterizerStates.CullBack);

                        _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                    }
                }

                _device.SetBlendState(_device.BlendStates.Opaque);
                _device.SetRasterizerState(_rasterizerWireframe);

                // Draw boxes
                if (DrawVisibilityBox || DrawCollisionBox)
                {

                    if (DrawVisibilityBox)
                    {
                        _vertexBufferVisibility?.Dispose();
                        _vertexBufferVisibility = Static.VisibilityBox.GetVertexBuffer(_device);

                        _device.SetVertexBuffer(_vertexBufferVisibility);
                        _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferVisibility));
                        _device.SetIndexBuffer(null, false);

                        solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
                        solidEffect.CurrentTechnique.Passes[0].Apply();

                        _device.Draw(PrimitiveType.LineList, _vertexBufferVisibility.ElementCount);
                    }

                    if (DrawCollisionBox)
                    {
                        _vertexBufferCollision?.Dispose();
                        _vertexBufferCollision = Static.CollisionBox.GetVertexBuffer(_device);

                        _device.SetVertexBuffer(_vertexBufferCollision);
                        _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferCollision));
                        _device.SetIndexBuffer(null, false);

                        solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                        solidEffect.CurrentTechnique.Passes[0].Apply();

                        _device.Draw(PrimitiveType.LineList, _vertexBufferCollision.ElementCount);
                    }
                }

                // Draw normals
                if (DrawNormals)
                {
                    var lines = new List<SolidVertex>();
                    for (int i = 0; i < Static.Mesh.VertexNormals.Count; i++)
                    {
                        var p = Vector3.Transform(Static.Mesh.VertexPositions[i], world);
                        var n = Vector3.TransformNormal(Static.Mesh.VertexNormals[i] /
                            Static.Mesh.VertexNormals[i].Length(), world);

                        var v = new SolidVertex();
                        v.Position = p;
                        v.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                        lines.Add(v);

                        v = new SolidVertex();
                        v.Position = p + n * 32.0f;
                        v.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                        lines.Add(v);
                    }

                    var bufferLines = SharpDX.Toolkit.Graphics.Buffer.New(_device, lines.ToArray(), BufferFlags.VertexBuffer, SharpDX.Direct3D11.ResourceUsage.Default);

                    _device.SetVertexBuffer(bufferLines);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, bufferLines));
                    _device.SetIndexBuffer(null, false);

                    solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.Draw(PrimitiveType.LineList, bufferLines.ElementCount);
                }
            }

            if (DrawGizmo)
            {
                // Draw the gizmo
                SwapChain.ClearDepth();
                _gizmo.Draw(viewProjection);
            }

            if (SelectedLight != null)
            {
                // Draw the gizmo of selected light
                SwapChain.ClearDepth();
                _gizmoLight.Draw(viewProjection);
            }

            // Draw debug strings
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState(); // To make sure SharpDx.Toolkit didn't change settings.
            SwapChain.RenderText(new Text
            {
                Font = _fontDefault,
                PixelPos = new Vector2(10, -10),
                Alignment = new Vector2(0, 0),
                Overlay = true,
                String =
                    "Position: " + StaticPosition +
                    "\nRotation: " + StaticRotation.X * (180 / Math.PI) +
                    "\nScale: " + StaticScale
            });
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            // Make this control able to receive scroll and key board events...
            base.OnMouseEnter(e);
            Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Camera.Zoom(-e.Delta * _tool.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom);
            Invalidate();
        }

        private void PlaceLight(int x, int y)
        {
            // Get the intersection point between ray and the horizontal plane
            var ray = Ray.GetPickRay(Camera, ClientSize, x, y);
            var plane = new Plane(Vector3.UnitY, 0.0f);
            var point = Vector3.Zero;
            Collision.RayIntersectsPlane(ray, plane, out point);

            // Add the light at the intersection point
            var light = new WadLight(point, 1.0f, 0.5f);
            Static.Lights.Add(light);

            _tool.StaticLightsChanged();

            Action = StaticEditorAction.Normal;
            Invalidate();
        }

        public void DeleteLight(WadLight light)
        {
            if (DarkMessageBox.Show(Parent, "Do you really want to delete this light?", "Confirm delete",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Static.Lights.Remove(light);
                SelectedLight = null;
                _tool.StaticLightsChanged();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

            if (e.Button == MouseButtons.Left)
            {
                if (Action != StaticEditorAction.PlaceLight)
                {
                    if (DrawGizmo)
                    {
                        var result = _gizmo.DoPicking(ray);
                        if (result != null)
                        {
                            _gizmo.ActivateGizmo(result);
                            Invalidate();
                            return;
                        }
                    }

                    if (SelectedLight != null)
                    {
                        var result = _gizmoLight.DoPicking(ray);
                        if (result != null)
                        {
                            _gizmoLight.ActivateGizmo(result);
                            Invalidate();
                            return;
                        }
                    }

                    // Try to pick lights
                    float minDistance = float.MaxValue;
                    SelectedLight = null;
                    foreach (var light in Static.Lights)
                    {
                        float distance = 0;
                        if (Collision.RayIntersectsSphere(ray, new BoundingSphere(light.Position, 128.0f),
                                                          out distance))
                        {
                            if (distance <= minDistance)
                                minDistance = distance;
                            SelectedLight = light;
                        }
                    }

                    _tool.StaticSelectedLightChanged();

                    if (SelectedLight != null)
                        Invalidate();
                }
                else
                {
                    PlaceLight(e.X, e.Y);
                }
            }

            Invalidate();

            _lastX = e.X;
            _lastY = e.Y;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
                Action = StaticEditorAction.Normal;
            else if (e.KeyCode == Keys.Delete && SelectedLight != null)
                DeleteLight(SelectedLight);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

            if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(ray)))
                Invalidate();
            if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ray))
                Invalidate();

            if (_gizmoLight.GizmoUpdateHoverEffect(_gizmoLight.DoPicking(ray)))
                Invalidate();
            if (_gizmoLight.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ray))
                Invalidate();

            if (Action == StaticEditorAction.Normal)
                Cursor = Cursors.Default;
            else
                Cursor = Cursors.Cross;

            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                // Use height for X coordinate because the camera FOV per pixel is defined by the height.
                float deltaX = (e.X - _lastX) / Height;
                float deltaY = (e.Y - _lastY) / Height;

                _lastX = e.X;
                _lastY = e.Y;

                if (e.Button == MouseButtons.Right)
                {
                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                        Camera.Zoom(-deltaY * _tool.Configuration.RenderingItem_NavigationSpeedMouseZoom);
                    else if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                        Camera.Rotate(deltaX * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate,
                                     -deltaY * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate);
                }
                if ((e.Button == MouseButtons.Right && (ModifierKeys & Keys.Shift) == Keys.Shift) ||
                     e.Button == MouseButtons.Middle)
                    Camera.MoveCameraPlane(new Vector3(deltaX, deltaY, 0) * _tool.Configuration.RenderingItem_NavigationSpeedMouseTranslate);

                Invalidate();
            }

            ((FormStaticEditor)FindForm()).UpdatePositionUI();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_gizmo.MouseUp())
                Invalidate();

            if (_gizmoLight.MouseUp())
                Invalidate();

            ((FormStaticEditor)FindForm()).UpdatePositionUI();
        }

        public void UpdateLights()
        {
            Static.Mesh.VertexColors.Clear();

            Matrix4x4 world = Matrix4x4.CreateFromYawPitchRoll(StaticRotation.Y, StaticRotation.X, StaticRotation.Z) *  
                              Matrix4x4.CreateTranslation(StaticPosition);

            for (int i = 0; i < Static.Mesh.VertexPositions.Count; i++)
            {
                float newShade = Static.AmbientLight / 255.0f;
                foreach (var light in Static.Lights)
                {
                    // Transform current vertex and normal
                    var p = Vector3.Transform(Static.Mesh.VertexPositions[i], world);
                    var n = Vector3.TransformNormal(Static.Mesh.VertexNormals[i] / Static.Mesh.VertexNormals[i].Length(), world);

                    // Get the light direction vector
                    var lightDirection = light.Position - p;
                    if (lightDirection.Length() > light.Radius * 1024)
                        continue;

                    var l = lightDirection / lightDirection.Length();

                    // Calculate cosine
                    float dot = Vector3.Dot(n, l);
                    if (dot <= 0)
                        continue;

                    newShade += dot * light.Intensity * (1.0f - (light.Position - p).Length() /
                                (light.Radius * Level.SectorSizeUnit));
                }

                Static.Mesh.VertexColors.Add(new Vector3(Math.Min(newShade, 1.0f)));
            }

            _tool.StaticLightsChanged();
        }

        public void UpdateMesh() => _wadRenderer.Dispose();
    }
}
