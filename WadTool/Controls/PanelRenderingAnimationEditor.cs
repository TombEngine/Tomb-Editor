using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    public class PanelRenderingAnimationEditor : RenderingPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Configuration { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimatedModel Model { get { return _model; } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimatedModel Skin { get { return _skinModel; } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Level Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Room Room { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector3 RoomPosition { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Vector3 GridPosition
        {
            get { return _gridPosition; }
            set
            {
                if (value == _gridPosition) return;
                _gridPosition = new Vector3(value.X % 4096.0f, value.Y % 4096.0f, value.Z % 4096.0f);
            }
        }
        private Vector3 _gridPosition;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<WadMeshBoneNode> Skeleton { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObjectMesh SelectedMesh
        {
            get { return _selectedMesh; }
            set { _selectedMesh = value; Invalidate(); }
        }
        private ObjectMesh _selectedMesh;

        // General state
        private AnimationEditor _editor;

        // Interaction state
        private float _lastX;
        private float _lastY;

        // Rendering state
        private RenderingTextureAllocator _fontTexture;
        private RenderingFont _fontDefault;

        // Legacy rendering state
        private GraphicsDevice _device;
        private DeviceManager _deviceManager;
        private WadRenderer _wadRenderer;
        private GizmoAnimationEditor _gizmo;
        private GeometricPrimitive _plane;
        private AnimatedModel _model;
        private AnimatedModel _skinModel;
        private RasterizerState _rasterizerWireframe;
        private Buffer<SolidVertex> _vertexBufferVisibility;

        public void InitializeRendering(AnimationEditor editor, DeviceManager deviceManager, WadMoveable skin)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            base.InitializeRendering(deviceManager.Device, Configuration.RenderingItem_Antialias);
            ResetCamera();

            _editor = editor;
            _wadRenderer = new WadRenderer(deviceManager.___LegacyDevice, false, true, 4096, 2048, true);
            _model = _wadRenderer.GetMoveable(editor.Moveable);

            Configuration = _editor.Tool.Configuration;

            if (skin != null)
                _skinModel = _wadRenderer.GetMoveable(editor.Moveable.ReplaceDummyMeshes(skin));

            // Actual "InitializeRendering"
            _fontTexture = deviceManager.Device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(512, 512, 2) });
            _fontDefault = deviceManager.Device.CreateFont(new RenderingFont.Description
            {
                FontName = _editor.Tool.Configuration.Rendering3D_FontName,
                FontSize = _editor.Tool.Configuration.Rendering3D_FontSize,
                FontIsBold = _editor.Tool.Configuration.Rendering3D_FontIsBold,
                TextureAllocator = _fontTexture
            });

            // Legacy rendering
            {
                _device = deviceManager.___LegacyDevice;
                _deviceManager = deviceManager;
                new BasicEffect(_device); // This effect is used for editor special meshes like sinks, cameras, light meshes, etc
                SharpDX.Direct3D11.RasterizerStateDescription renderStateDesc =
                    new SharpDX.Direct3D11.RasterizerStateDescription
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
                    };

                _rasterizerWireframe = RasterizerState.New(deviceManager.___LegacyDevice, renderStateDesc);

                _gizmo = new GizmoAnimationEditor(editor, _device, _deviceManager.___LegacyEffects["Solid"], this);
                _plane = GeometricPrimitive.GridPlane.New(_device, 8, 4);
            }
        }

        public void ResetCamera()
        {
            Camera = new ArcBallCamera(new Vector3(0.0f, 256.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 2048.0f, 100, 1000000, (float)Math.PI / 4.0f);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fontTexture?.Dispose();
                _fontDefault?.Dispose();
                _gizmo?.Dispose();
                _plane?.Dispose();
                _model?.Dispose();
                _skinModel?.Dispose();
                _wadRenderer?.Dispose();
                _vertexBufferVisibility?.Dispose();
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

            if (_model != null)
            {
                var skin = (_skinModel != null ? _skinModel : _model);
                var effect = _deviceManager.___LegacyEffects["Model"];

                effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                effect.Parameters["TextureSampler"].SetResource(_device.SamplerStates.Default);
                effect.Parameters["StaticLighting"].SetValue(false);
                effect.Parameters["ColoredVertices"].SetValue(false);

                // Build animation transforms
                var matrices = new List<Matrix4x4>();
                if (_editor.ValidAnimationAndFrames)
                {
                    for (var b = 0; b < _model.Meshes.Count; b++)
                        matrices.Add(_model.AnimationTransforms[b]);
                }
                else
                {
                    foreach (var bone in _model.Bones)
                        matrices.Add(bone.GlobalTransform);
                }

                bool showSkin = _editor.Wad.GameVersion == TRVersion.Game.TombEngine && Configuration.AnimationEditor_ShowSkin && skin.Skin != null;

                for (int i = 0; i < skin.Meshes.Count; i++)
                {
                    var mesh = skin.Meshes[i];
                    if (mesh.Vertices.Count == 0)
                        continue;

                    if (showSkin && skin.Meshes[i].Hidden)
                        continue;

                    mesh.UpdateBuffers(Camera.GetPosition());

                    _device.SetVertexBuffer(0, mesh.VertexBuffer);
                    _device.SetIndexBuffer(mesh.IndexBuffer, true);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, mesh.VertexBuffer));

                    if (SelectedMesh == _model.Meshes[i] && _editor.ValidAnimationAndFrames)
                        effect.Parameters["Color"].SetValue(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                    else
                        effect.Parameters["Color"].SetValue(Vector4.One);

                    effect.Parameters["ModelViewProjection"].SetValue((matrices[i] * viewProjection).ToSharpDX());

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

                if (showSkin)
                {
                    effect.Parameters["AlphaTest"].SetValue(true);
                    effect.Parameters["Color"].SetValue(Vector4.One);

                    Skin.Skin.UpdateBuffers(Camera.GetPosition());
                    Skin.RenderSkin(_device, effect, viewProjection.ToSharpDX(), _model);
                }

                _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                _device.SetBlendState(_device.BlendStates.Opaque);

                if (_editor.ValidAnimationAndFrames)
                {
                    _device.SetRasterizerState(_rasterizerWireframe);

                    // Draw selection box
                    if (SelectedMesh != null)
                    {
                        if (_vertexBufferVisibility != null)
                            _vertexBufferVisibility.Dispose();
                        int meshIndex = _model.Meshes.IndexOf(SelectedMesh);
                        _vertexBufferVisibility = Skin.Meshes[meshIndex].BoundingBox.GetVertexBuffer(_device);

                        _device.SetVertexBuffer(_vertexBufferVisibility);
                        _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferVisibility));
                        _device.SetIndexBuffer(null, false);

                        solidEffect.Parameters["ModelViewProjection"].SetValue((_model.AnimationTransforms[meshIndex] * viewProjection).ToSharpDX());
                        solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                        solidEffect.CurrentTechnique.Passes[0].Apply();

                        _device.Draw(PrimitiveType.LineList, _vertexBufferVisibility.ElementCount);
                    }

                    // Draw collision box
                    if (Configuration.AnimationEditor_ShowCollisionBox)
                    {
                        if (_vertexBufferVisibility != null)
                            _vertexBufferVisibility.Dispose();
                        _vertexBufferVisibility = _editor.CurrentKeyFrame.BoundingBox.GetVertexBuffer(_device);

                        _device.SetVertexBuffer(_vertexBufferVisibility);
                        _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _vertexBufferVisibility));
                        _device.SetIndexBuffer(null, false);

                        solidEffect.Parameters["ModelViewProjection"].SetValue((viewProjection).ToSharpDX());
                        solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                        solidEffect.CurrentTechnique.Passes[0].Apply();

                        _device.Draw(PrimitiveType.LineList, _vertexBufferVisibility.ElementCount);
                    }
                }
            }

            if (Configuration.AnimationEditor_ShowGrid)
            {
                // Draw the grid
                _device.SetVertexBuffer(0, _plane.VertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _plane.VertexBuffer));
                _device.SetIndexBuffer(_plane.IndexBuffer, true);
                _device.SetRasterizerState(_rasterizerWireframe);

                var shift = Matrix4x4.CreateTranslation(new Vector3(-GridPosition.X, GridPosition.Y, -GridPosition.Z));
                solidEffect.Parameters["ModelViewProjection"].SetValue((shift * viewProjection).ToSharpDX());
                solidEffect.Parameters["Color"].SetValue(Vector4.One);
                solidEffect.Techniques[0].Passes[0].Apply();

                _device.Draw(PrimitiveType.LineList, _plane.VertexBuffer.ElementCount);
            }

            if (Configuration.AnimationEditor_ShowGizmo && 
                SelectedMesh != null && _editor.ValidAnimationAndFrames)
            {
                // Draw the gizmo
                SwapChain.ClearDepth();
                _gizmo.Draw(viewProjection);
            }

            if (_editor.CurrentAnim != null && 
                Configuration.RenderingItem_ShowDebugInfo)
            {
                ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState(); // To make sure SharpDx.Toolkit didn't change settings.
                string debugMessage = "Frame: " + (_editor.CurrentFrameIndex + 1) + "/" + _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count;
                if (SelectedMesh != null)
                {
                    debugMessage += "\nMesh: " + SelectedMesh.Name;
                    debugMessage += "\nBone: " + _model.Bones[_model.Meshes.IndexOf(SelectedMesh)].Name;
                    debugMessage += "\nRotation: " + _editor.CurrentKeyFrame.Rotations[Model.Meshes.IndexOf(SelectedMesh)];
                }
                SwapChain.RenderText(new Text
                {
                    Font = _fontDefault,
                    Overlay = true,
                    PixelPos = new Vector2(10, -10),
                    Alignment = new Vector2(0, 0),
                    String = debugMessage
                });
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            // Make this control able to receive scroll and key board events...
            base.OnMouseEnter(e);

            if (Form.ActiveForm == FindForm())
                Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Camera.Zoom(-e.Delta * Configuration.RenderingItem_NavigationSpeedMouseWheelZoom);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

                if (_editor.ValidAnimationAndFrames)
                {
                    // Try to do gizmo picking
                    if (Configuration.AnimationEditor_ShowGizmo)
                    {
                        var result = _gizmo.DoPicking(ray);
                        if (result != null)
                        {
                            _gizmo.ActivateGizmo(result);
                            _editor.Tool.AnimationEditorGizmoPicked();
                            Invalidate();
                            return;
                        }
                    }

                    // Try to do node picking
                    ObjectMesh foundMesh = null;
                    for (int i = 0; i < _model.Meshes.Count; i++)
                    {
                        float distance = 0;
                        float minDistance = float.PositiveInfinity;
                        if (DoMeshPicking(ray, i, out distance))
                        {
                            if (distance < minDistance)
                            {
                                distance = minDistance;
                                foundMesh = _model.Meshes[i];
                            }
                        }
                    }

                    if (SelectedMesh != foundMesh)
                    {
                        SelectedMesh = foundMesh;
                        _editor.Tool.AnimationEditorMeshSelected(Model, SelectedMesh);
                    }
                }
                else
                {
                    SelectedMesh = null;
                    _editor.Tool.AnimationEditorMeshSelected(Model, SelectedMesh);
                }
            }

            Invalidate();

            _lastX = e.X;
            _lastY = e.Y;
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
                        Camera.Zoom(-deltaY * Configuration.RenderingItem_NavigationSpeedMouseZoom);
                    else if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                        Camera.Rotate(deltaX * Configuration.RenderingItem_NavigationSpeedMouseRotate,
                                     -deltaY * Configuration.RenderingItem_NavigationSpeedMouseRotate);
                }
                if ((e.Button == MouseButtons.Right && (ModifierKeys & Keys.Shift) == Keys.Shift) ||
                     e.Button == MouseButtons.Middle)
                    Camera.MoveCameraPlane(new Vector3(deltaX, deltaY, 0) * Configuration.RenderingItem_NavigationSpeedMouseTranslate);

                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_gizmo.MouseUp())
                Invalidate();
        }

        private bool DoMeshPicking(Ray ray, int meshIndex, out float meshDistance)
        {
            meshDistance = 0;

            // Transform view ray to object space space
            Matrix4x4 inverseObjectMatrix;
            if (!Matrix4x4.Invert((_editor.CurrentAnim != null ? _model.AnimationTransforms[meshIndex] : _model.BindPoseTransforms[meshIndex]), out inverseObjectMatrix))
                return false;
            Vector3 transformedRayPos = MathC.HomogenousTransform(ray.Position, inverseObjectMatrix);
            Vector3 transformedRayDestination = MathC.HomogenousTransform(ray.Position + ray.Direction, inverseObjectMatrix);
            Ray transformedRay = new Ray(transformedRayPos, transformedRayDestination - transformedRayPos);
            transformedRay.Direction = Vector3.Normalize(transformedRay.Direction);

            // Now do a ray - triangle intersection test
            bool hit = false;
            float minDistance = float.PositiveInfinity;
            var mesh = _skinModel.Meshes[meshIndex];
            foreach (var submesh in mesh.Submeshes)
                for (int k = 0; k < submesh.Value.Indices.Count; k += 3)
                {
                    Vector3 p1 = mesh.Vertices[submesh.Value.Indices[k]].Position;
                    Vector3 p2 = mesh.Vertices[submesh.Value.Indices[k + 1]].Position;
                    Vector3 p3 = mesh.Vertices[submesh.Value.Indices[k + 2]].Position;

                    float distance;
                    if (Collision.RayIntersectsTriangle(transformedRay, p1, p2, p3, true, out distance) && distance < minDistance)
                    {
                        minDistance = distance;
                        hit = true;
                    }
                }

            if (hit)
            {
                meshDistance = minDistance;
                return true;
            }
            else
                return false;
        }
    }
}
