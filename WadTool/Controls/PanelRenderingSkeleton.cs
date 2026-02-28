using TombLib.Graphics.Dx11Toolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.Graphics.Primitives;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool.Controls
{
    public class PanelRenderingSkeleton : RenderingPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; } = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 100, 1000000, (float)Math.PI / 4.0f);
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Configuration { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGrid { get; set; } = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGizmo { get; set; } = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector3 StaticPosition { get; set; } = Vector3.Zero;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector3 StaticRotation { get; set; } = Vector3.Zero;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float StaticScale { get; set; } = 1.0f;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<WadMeshBoneNode> Skeleton { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadMeshBoneNode SelectedNode { get; set; }

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
        private GizmoSkeletonEditor _gizmo;
        private GeometricPrimitive _plane;
        private WadRenderer _wadRenderer; // TODO Remove internal hack that destroys rendering encapsulation
        private WadStatic _dummyStatic = new WadStatic(new WadStaticId(0));
        private Buffer<SolidVertex> _vertexBufferVisibility;

        public void InitializeRendering(WadToolClass tool, DeviceManager deviceManager)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            base.InitializeRendering(deviceManager.Device, tool.Configuration.RenderingItem_Antialias);
            _tool = tool;
            _device = deviceManager.___LegacyDevice;
            _deviceManager = deviceManager;

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
                _wadRenderer = new WadRenderer(_device, false, true, 4096, 2048, false);
                new BasicEffect(_device); // This effect is used for editor special meshes like sinks, cameras, light meshes, etc
                _rasterizerWireframe = RasterizerState.New(_device, new RasterizerStateDescription
                {
                    CullMode = CullMode.None,
                    DepthBias = 0,
                    DepthBiasClamp = 0,
                    FillMode = FillMode.Wireframe,
                    IsAntialiasedLineEnabled = true,
                    IsDepthClipEnabled = true,
                    IsFrontCounterClockwise = false,
                    IsMultisampleEnabled = true,
                    IsScissorEnabled = false,
                    SlopeScaledDepthBias = 0
                });
                _gizmo = new GizmoSkeletonEditor(_tool, _tool.Configuration, _device, _deviceManager.___LegacyEffects["Solid"], this);
                _plane = GeometricPrimitive.GridPlane.New(_device, 8, 4);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fontTexture?.Dispose();
                _fontDefault?.Dispose();
                _rasterizerWireframe?.Dispose();
                _vertexBufferVisibility?.Dispose();
                _gizmo?.Dispose();
                _plane?.Dispose();
                _wadRenderer?.Dispose();
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

            Matrix4x4 viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            Effect solidEffect = _deviceManager.___LegacyEffects["Solid"];
            
            if (DrawGrid)
            {
                _device.SetRasterizerState(_rasterizerWireframe);

                // Draw the grid
                _device.SetVertexBuffer(0, _plane.VertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _plane.VertexBuffer));
                _device.SetIndexBuffer(_plane.IndexBuffer, true);

                solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection);
                solidEffect.Parameters["Color"].SetValue(Vector4.One);
                solidEffect.Techniques[0].Passes[0].Apply();

                _device.Draw(PrimitiveType.LineList, _plane.VertexBuffer.ElementCount);
            }

            if (Skeleton != null)
            {
                var effect = _deviceManager.___LegacyEffects["Model"];

                effect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);

                foreach (var node in Skeleton)
                {
                    // TODO Keep data on GPU, optimize data upload
                    // Use new renderer

                    _dummyStatic.Mesh = node.Mesh;
                    _dummyStatic.Version = DataVersion.GetNext();
                    var mesh = _wadRenderer.GetStatic(_dummyStatic);
                    mesh.UpdateBuffers(Camera.GetPosition());

                    effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                    effect.Parameters["ModelViewProjection"].SetValue((node.GlobalTransform * viewProjection));
                    effect.Parameters["Color"].SetValue(Vector4.One);
                    effect.Parameters["StaticLighting"].SetValue(node.Mesh.LightingType != WadMeshLightingType.Normals);
                    effect.Parameters["ColoredVertices"].SetValue(_tool.DestinationWad.GameVersion == TombLib.LevelData.TRVersion.Game.TombEngine);

                    foreach (var mesh_ in mesh.Meshes)
                    {
                        _device.SetVertexBuffer(0, mesh_.VertexBuffer);
                        _device.SetIndexBuffer(mesh_.IndexBuffer, true);
                        _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, mesh_.VertexBuffer));
                        effect.Techniques[0].Passes[0].Apply();

                        foreach (var submesh in mesh_.Submeshes)
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
                }

                _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                _device.SetBlendState(_device.BlendStates.Opaque);

                // Draw box
                if (SelectedNode != null)
                {
                    _vertexBufferVisibility?.Dispose();
                    _vertexBufferVisibility = SelectedNode.Mesh.BoundingBox.GetVertexBuffer(_device);

                    _device.SetVertexBuffer(_vertexBufferVisibility);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferVisibility));
                    _device.SetIndexBuffer(null, false);

                    solidEffect.Parameters["ModelViewProjection"].SetValue((SelectedNode.GlobalTransform * viewProjection));
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.Draw(PrimitiveType.LineList, _vertexBufferVisibility.ElementCount);
                }
            }

            if (DrawGizmo && SelectedNode != null)
            {
                // Draw the gizmo
                SwapChain.ClearDepth();
                _gizmo.Draw(viewProjection);
            }

            // Draw debug strings
            if (SelectedNode != null)
            {
                ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState(); // To make sure legacy toolkit didn't change settings.
                Matrix4x4 worldViewProjection = SelectedNode.GlobalTransform * viewProjection;
                SwapChain.RenderText(new Text
                {
                    Font = _fontDefault,
                    Overlay = true,
                    Pos = worldViewProjection.TransformPerspectively(SelectedNode.Center - Vector3.UnitY * 128.0f).To2(),
                    TextAlignment = new Vector2(0, 0),
                    ScreenAlignment = new Vector2(0.5f, 0.5f),
                    String =
                        "Name: " + SelectedNode.Bone.Name +
                        "\nLocal offset: " + SelectedNode.Bone.Translation
                });
            }
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

                // Try to do gizmo picking
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

                // Try to do node picking
                WadMeshBoneNode foundNode = null;
                foreach (var node in Skeleton)
                {
                    float distance = 0;
                    float minDistance = float.PositiveInfinity;
                    if (DoNodePicking(ray, node, out distance))
                    {
                        if (distance < minDistance)
                        {
                            distance = minDistance;
                            foundNode = node;
                        }
                    }
                }
                SelectedNode = foundNode;
                _tool.BonePicked();
            }

            Invalidate();

            _lastX = e.X;
            _lastY = e.Y;

            base.OnMouseDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

            if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(ray)))
                Invalidate();
            if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ray))
                Invalidate();

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
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_gizmo.MouseUp())
                Invalidate();
        }

        private bool DoNodePicking(Ray ray, WadMeshBoneNode node, out float nodeDistance)
        {
            nodeDistance = 0;

            // Transform view ray to object space space
            Matrix4x4 inverseObjectMatrix;
            if (!Matrix4x4.Invert(node.GlobalTransform, out inverseObjectMatrix))
                return false;
            Vector3 transformedRayPos = MathC.HomogenousTransform(ray.Position, inverseObjectMatrix);
            Vector3 transformedRayDestination = MathC.HomogenousTransform(ray.Position + ray.Direction, inverseObjectMatrix);
            Ray transformedRay = new Ray(transformedRayPos, transformedRayDestination - transformedRayPos);
            transformedRay.Direction = Vector3.Normalize(transformedRay.Direction);

            // Now do a ray - triangle intersection test
            bool hit = false;
            float minDistance = float.PositiveInfinity;
            var mesh = node.Mesh;
            foreach (var poly in mesh.Polys)
            {
                if (poly.Shape == WadPolygonShape.Quad)
                {
                    Vector3 p1 = mesh.VertexPositions[poly.Index0];
                    Vector3 p2 = mesh.VertexPositions[poly.Index1];
                    Vector3 p3 = mesh.VertexPositions[poly.Index2];
                    Vector3 p4 = mesh.VertexPositions[poly.Index3];

                    float distance;
                    if (Collision.RayIntersectsTriangle(transformedRay, p1, p2, p3, true, out distance) && distance < minDistance)
                    {
                        minDistance = distance;
                        hit = true;
                    }

                    if (Collision.RayIntersectsTriangle(transformedRay, p1, p3, p4, true, out distance) && distance < minDistance)
                    {
                        minDistance = distance;
                        hit = true;
                    }
                }
                else
                {
                    Vector3 p1 = mesh.VertexPositions[poly.Index0];
                    Vector3 p2 = mesh.VertexPositions[poly.Index1];
                    Vector3 p3 = mesh.VertexPositions[poly.Index2];

                    float distance;
                    if (Collision.RayIntersectsTriangle(transformedRay, p1, p2, p3, true, out distance) && distance < minDistance)
                    {
                        minDistance = distance;
                        hit = true;
                    }
                }
            }

            if (hit)
            {
                nodeDistance = minDistance;
                return true;
            }
            else
                return false;
        }
        public void UpdateModel() => _wadRenderer.Dispose();
    }
}
