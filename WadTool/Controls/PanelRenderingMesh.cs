using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
    public class PanelRenderingMesh : RenderingPanel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArcBallCamera Camera { get; set; } = new ArcBallCamera(new Vector3(0.0f, 0.0f, 0.0f), 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, 512.0f, 100, 1000000, (float)Math.PI / 4.0f);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WadMesh Mesh
        {
            get { return _mesh; }
            set
            {
                if (_mesh == value)
                    return;

                _mesh = value;
                _previewMesh = _mesh == null ? null : _mesh.Clone();
                InitializeVertexBuffer();

                if (ResetCameraOnMeshChange)
                    ResetCamera();

                if (EditingMode == MeshEditingMode.VertexWeights)
                    ColorizeVertexWeights();

                CurrentElement = -1;
            }
        }
        private WadMesh _mesh;

        public WadMesh VisibleMesh => ((_editingMode == MeshEditingMode.VertexWeights || _previewTimer.Enabled) && _previewMesh != null) ? _previewMesh : _mesh;
        private WadMesh _previewMesh;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MeshEditingMode EditingMode
        {
            get {  return _editingMode; }
            set
            {
                if (_editingMode == value) 
                    return;

                if (value != MeshEditingMode.VertexEffects)
                    StopPreview();

                if (value == MeshEditingMode.VertexWeights)
                    ColorizeVertexWeights();

                CurrentElement = -1;
                _editingMode = value;
                Invalidate();
            }
        }
        private MeshEditingMode _editingMode = MeshEditingMode.None;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentElement
        {
            get { return _currentElement; }
            set
            {
                if (Mesh == null)
                    return;

                bool engageUndo = false;

                switch (EditingMode)
                {
                    case MeshEditingMode.FaceAttributes:
                        if (_currentElement == value) return;  // Face mode does not need continuous editing of same element
                        SelectElement(value);
                        engageUndo = !Control.ModifierKeys.HasFlag(Keys.Alt);
                        break;

                    case MeshEditingMode.VertexEffects:
                    case MeshEditingMode.VertexColorsAndNormals:
                    case MeshEditingMode.VertexWeights:
                    case MeshEditingMode.VertexRemap:
                        SelectElement(value);
                        engageUndo = !Control.ModifierKeys.HasFlag(Keys.Alt) && EditingMode != MeshEditingMode.VertexRemap;
                        break;

                    default:
                        _currentElement = -1;
                        break;
                }

                if (engageUndo && !_actionStarted && value != -1)
                {
                    _tool.UndoManager.PushMeshChanged(this);
                    _actionStarted = true;
                }

                _tool.MeshEditorElementChanged(_currentElement);

                if (EditingMode == MeshEditingMode.VertexWeights && value != -1)
                    ColorizeVertexWeights();
            }
        }
        private int _currentElement = -1;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool WireframeMode
        {
            get { return _wireframeMode; }
            set
            {
                if (_wireframeMode == value)
                    return;

                _wireframeMode = value;
                Invalidate();
            }
        }
        private bool _wireframeMode = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AlphaTest
        {
            get { return _alphaTest; }
            set
            {
                if (_alphaTest == value)
                    return;

                _alphaTest = value;
                Invalidate();
            }
        }
        private bool _alphaTest = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Bilinear
        {
            get { return _bilinear; }
            set
            {
                if (_bilinear == value)
                    return;

                _wadRenderer.Dispose();
                _wadRenderer = new WadRenderer(_device, false, value, 4096, 2048, false);
                _bilinear = value;
                Invalidate();
            }
        }
        private bool _bilinear = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawGrid
        {
            get { return _drawGrid; }
            set
            {
                if (_drawGrid == value)
                    return;

                _drawGrid = value;
                Invalidate();
            }
        }
        private bool _drawGrid = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DrawExtraInfo
        {
            get { return _drawInformationForAllElements; }
            set
            {
                if (_drawInformationForAllElements == value)
                    return;

                _drawInformationForAllElements = value;
                Invalidate();
            }
        }
        private bool _drawInformationForAllElements = false;

        public bool ResetCameraOnMeshChange = true;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SafeVertexRemapLimit
        {
            // HACK: Determine remappable vertices (only for legacy engines).
            // For more info: https://www.tombraiderforums.com/showthread.php?t=132749

            get
            {
                int safeIndex = int.MaxValue;
                if (_tool.DestinationWad.GameVersion != TRVersion.Game.TombEngine)
                {
                    if (_mesh.VertexPositions.Count <= 255)
                        safeIndex = 127;
                    else
                    {
                        var step = (Math.Truncate((float)_mesh.VertexPositions.Count / 256.0f) - 1) * 256.0f;
                        safeIndex = _mesh.VertexPositions.Count - (int)step - 1;
                        if (safeIndex > 127) safeIndex = 127;
                    }
                }
                return safeIndex;
            }
        }

        private float VertexSphereRadius
        {
            // A helper function to define vertex pick sphere radius
            get
            {
                var distances = new List<float>();

                if (Mesh == null || Mesh.VertexPositions.Count == 0)
                    return 0;

                foreach (var p in Mesh.Polys)
                {
                    distances.Add(Vector3.Distance(Mesh.VertexPositions[p.Index0], Mesh.VertexPositions[p.Index1]));
                    distances.Add(Vector3.Distance(Mesh.VertexPositions[p.Index1], Mesh.VertexPositions[p.Index2]));
                    
                    if (p.IsTriangle)
                        distances.Add(Vector3.Distance(Mesh.VertexPositions[p.Index2], Mesh.VertexPositions[p.Index0]));
                    else
                    {
                        distances.Add(Vector3.Distance(Mesh.VertexPositions[p.Index2], Mesh.VertexPositions[p.Index3]));
                        distances.Add(Vector3.Distance(Mesh.VertexPositions[p.Index3], Mesh.VertexPositions[p.Index0]));
                    }
                }

                return distances.Sum() / distances.Count / 12.0f;
            }
        }

        // General state
        private WadToolClass _tool;

        // Interaction state
        private bool _actionStarted;
        private bool _highlightFace;
        private Point _lastMousePosition;
        private List<Vector3> _lastElementPos = new List<Vector3>();
        private List<int> _clickchain = new List<int>();

        // Legacy rendering state
        private GraphicsDevice _device;
        private RasterizerState _rasterizerWireframe;
        private VertexInputLayout _layout;
        private GeometricPrimitive _littleSphere;
        private GeometricPrimitive _bigSphere;
        private GeometricPrimitive _plane;
        private float _normalLength = 1.0f;
        private Buffer<SolidVertex> _faceVertexBuffer;
        private WadRenderer _wadRenderer;
        private WadStatic _dummyStatic = new WadStatic(new WadStaticId(0));

        // Rendering state
        private RenderingTextureAllocator _fontTexture;
        private RenderingFont _fontDefault;

        // Vertex effect preview
        private readonly Timer _previewTimer = new Timer { Interval = 33 };
        private int _frameCount;

        // Vertex weight preview
        private const int _maxBones = 32;
        private Vector4[] _boneColors = new Vector4[_maxBones];

        // Gizmo
        private GizmoMeshEditor _gizmo;

        // Constants
        private readonly List<int> _oldLaraHairIndices = new List<int>() { 37, 38, 39, 40 };
        private readonly List<int> _youngLaraHairIndices = new List<int>() { 68, 69, 70, 71, 76, 77, 78, 79 };

        protected override Vector4 ClearColor => _tool.Configuration.RenderingItem_BackgroundColor;

        public PanelRenderingMesh()
        {
            _previewTimer.Tick += new EventHandler(PreviewTimer_Tick);
            GenerateBoneColors();
        }

        public void InitializeRendering(WadToolClass tool, DeviceManager deviceManager)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;

            base.InitializeRendering(deviceManager.Device, tool.Configuration.RenderingItem_Antialias);
            _tool = tool;

            // Legacy rendering
            {
                _device = deviceManager.___LegacyDevice;
                _wadRenderer = new WadRenderer(deviceManager.___LegacyDevice, false, false, 4096, 2048, false);

                _fontTexture = deviceManager.Device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(512, 512, 2) });
                _fontDefault = deviceManager.Device.CreateFont(new RenderingFont.Description
                {
                    FontName = _tool.Configuration.Rendering3D_FontName,
                    FontSize = _tool.Configuration.Rendering3D_FontSize,
                    FontIsBold = _tool.Configuration.Rendering3D_FontIsBold,
                    TextureAllocator = _fontTexture
                });

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

                _littleSphere = GeometricPrimitive.Sphere.New(_device, 2, 4);
                _bigSphere = GeometricPrimitive.Sphere.New(_device, 1, 10);
                _plane = GeometricPrimitive.GridPlane.New(_device, 8, 4);
                _gizmo = new GizmoMeshEditor(_tool.Configuration, _device, DeviceManager.DefaultDeviceManager.___LegacyEffects["Solid"], this);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _previewTimer.Stop();
                _previewTimer.Tick -= new EventHandler(PreviewTimer_Tick);

                _gizmo?.Dispose();
                _rasterizerWireframe?.Dispose();
                _littleSphere?.Dispose();
                _bigSphere?.Dispose();
                _plane?.Dispose();
                _wadRenderer?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnDraw()
        {
            if (VisibleMesh == null)
                return;

            // To make sure things are in a defined state for legacy rendering...
            ((TombLib.Rendering.DirectX11.Dx11RenderingSwapChain)SwapChain).BindForce();
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            var viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            var solidEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Solid"];

            _device.SetDepthStencilState(_device.DepthStencilStates.Default);

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

            _dummyStatic.Mesh = VisibleMesh;
            _dummyStatic.Version = DataVersion.GetNext();
            var mesh = _wadRenderer.GetStatic(_dummyStatic);
            mesh.UpdateBuffers(Camera.GetPosition());

            var world  = Matrix4x4.Identity;

            var textToDraw  = new List<Text>();
            var linesToDraw = new List<SolidVertex>();

            // At first, draw either vertex spheres (if mode is set to vertex remap)
            // or individual colored shininess faces (if mode is set to shininess editing).

            if (EditingMode == MeshEditingMode.VertexRemap ||
                EditingMode == MeshEditingMode.VertexEffects ||
                EditingMode == MeshEditingMode.VertexWeights ||
                EditingMode == MeshEditingMode.VertexColorsAndNormals)
            {
                // Draw model first in vertex or sphere modes
                DrawModel(mesh, world * viewProjection);

                _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                _device.SetBlendState(_device.BlendStates.AlphaBlend);
                _device.SetDepthStencilState(_device.DepthStencilStates.Default);

                _device.SetVertexBuffer(_littleSphere.VertexBuffer);
                _device.SetVertexInputLayout(_littleSphere.InputLayout);
                _device.SetIndexBuffer(_littleSphere.IndexBuffer, _littleSphere.IsIndex32Bits);

                var safeIndex    = SafeVertexRemapLimit;

                for (int i = 0; i < _mesh.VertexPositions.Count; i++)
                {
                    var selected = (i == _currentElement);

                    // Don't draw vertices from clickchain
                    if (!selected && _currentElement != -1 && _mesh.VertexPositions[i] == _mesh.VertexPositions[_currentElement])
                        continue;

                    var posMatrix = Matrix4x4.Identity * Matrix4x4.CreateTranslation(VisibleMesh.VertexPositions[i]) * viewProjection;
                    solidEffect.Parameters["ModelViewProjection"].SetValue(posMatrix.ToSharpDX());

                    if (selected)
                    {
                        // Highlight selection
                        solidEffect.Parameters["Color"].SetValue(new Vector4(1, 0, 0, 0.5f));
                    }
                    else
                    {
                        switch (EditingMode)
                        {
                            case MeshEditingMode.VertexRemap:

                                // Highlight safe remap indices
                                if (i <= safeIndex)  
                                    solidEffect.Parameters["Color"].SetValue(new Vector4(0, 0.3f, 1, 0.8f));
                                else
                                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.8f, 0.8f, 0, 0.8f));
                                break;

                            case MeshEditingMode.VertexEffects:

                                // Mix glow and move attributes for now as green and blue color components for vertex spheres.
                                // TODO: If in future there will be more vertex attributes, another way of indication must be invented.
                                if (_mesh.HasAttributes)
                                {
                                    var glowPower = _mesh.VertexAttributes[i].Glow == 0 ? 0 : (_mesh.VertexAttributes[i].Glow + 64.0f) / 128.0f;
                                    var movePower = _mesh.VertexAttributes[i].Move == 0 ? 0 : (_mesh.VertexAttributes[i].Move + 64.0f) / 128.0f;
                                    solidEffect.Parameters["Color"].SetValue(new Vector4(0, glowPower, movePower, 0.7f));
                                }
                                else
                                    solidEffect.Parameters["Color"].SetValue(new Vector4(0, 0, 0, 0.8f));
                                break;

                            case MeshEditingMode.VertexWeights:
                                if (_mesh.HasWeights)
                                    solidEffect.Parameters["Color"].SetValue(new Vector4(1, 1, 1, 1));
                                else
                                    solidEffect.Parameters["Color"].SetValue(new Vector4(0, 0, 0, 1));

                                break;

                            case MeshEditingMode.VertexColorsAndNormals:

                                // Simply draw normal color, since we don't need any extra indication for this mode
                                solidEffect.Parameters["Color"].SetValue(new Vector4(1, 1, 1, 0.6f));
                                break;
                        }
                    }

                    solidEffect.Techniques[0].Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _littleSphere.IndexBuffer.ElementCount);

                    if (DrawExtraInfo || selected)
                    {
                        // Only draw texts which are actually visible
                        if (posMatrix.TransformPerspectively(new Vector3()).Z <= 1.0f)
                        {
                            var pos = posMatrix.TransformPerspectively(new Vector3()).To2();
                            var message = string.Empty;

                            switch (EditingMode)
                            {
                                case MeshEditingMode.VertexRemap:
                                    {
                                        // Filter out labels which sit on the same coordinate and show ellipsis instead
                                        var existingText = textToDraw.Where(t => t.Pos == pos).ToList();
                                        if (existingText.Count > 0)
                                        {
                                            if (existingText[0].String != _currentElement.ToString())
                                                existingText[0].String = "...";
                                            continue;
                                        }

                                        message = i.ToString();
                                    }
                                    break;

                                case MeshEditingMode.VertexWeights:
                                    {
                                        if (_mesh.HasWeights)
                                            message = _mesh.VertexWeights[i].ToString();
                                    }
                                    break;

                                case MeshEditingMode.VertexEffects:
                                    {
                                        if (_mesh.HasAttributes)
                                            message = _mesh.VertexAttributes[i].Glow + ", " + _mesh.VertexAttributes[i].Move;
                                    }
                                    break;

                                case MeshEditingMode.VertexColorsAndNormals:
                                    {
                                        if (_mesh.HasNormals)
                                        {
                                            var color = selected ? new Vector4(1, 0, 0, 1) : Vector4.One;

                                            var p = Vector3.Transform(_mesh.VertexPositions[i], world);
                                            var n = Vector3.TransformNormal(_mesh.VertexNormals[i] /
                                                _mesh.VertexNormals[i].Length(), world);

                                            var v = new SolidVertex();
                                            v.Position = p;
                                            v.Color = color;
                                            linesToDraw.Add(v);

                                            v = new SolidVertex();
                                            v.Position = p + n * _normalLength;
                                            v.Color = color;
                                            linesToDraw.Add(v);
                                        }
                                    }
                                    break;
                            }


                            if (!string.IsNullOrEmpty(message))
                                textToDraw.Add(new Text
                                {
                                    Font = _fontDefault,
                                    TextAlignment = new Vector2(0.0f, 0.0f),
                                    PixelPos = new VectorInt2(2, -2),
                                    Pos = pos,
                                    Overlay = _tool.Configuration.Rendering3D_DrawFontOverlays,
                                    String = message
                                });
                        }
                    }
                }
            }
            
            if (EditingMode == MeshEditingMode.FaceAttributes ||
                EditingMode == MeshEditingMode.VertexWeights)
            {
                // Accumulate and draw extra face info (for now, only shininess values)

                if ((DrawExtraInfo || _highlightFace || EditingMode == MeshEditingMode.VertexWeights) && _mesh.Polys.Count > 0)
                {
                    _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                    _device.SetBlendState(_device.BlendStates.Opaque);
                    _device.SetDepthStencilState(_device.DepthStencilStates.Default);

                    _device.SetVertexBuffer(_faceVertexBuffer);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _faceVertexBuffer));

                    // Create a vertex array
                    var vtxs = new SolidVertex[_faceVertexBuffer.ElementCount];
                    int vertexCount = 0;

                    for (int i = 0; i < _mesh.Polys.Count; i++)
                    {
                        var poly = _mesh.Polys[i];
                        var strength = _mesh.Polys[i].ShineStrength == 0 ? 0 : (_mesh.Polys[i].ShineStrength + 32.0f) / 95.0f;
                        int vn = 0;

                        // Draw one triangle for triangular face or 2 triangles for quad face

                        for (int j = 0; j < (poly.Shape == WadPolygonShape.Quad ? 2 : 1); j++)
                            for (int v = 0; v < 3; v++)
                            {
                                int index = 0;
                                Vector3 pos = Vector3.Zero;
                                Vector4 color = Vector4.Zero;

                                if (DrawExtraInfo || i == _currentElement || EditingMode == MeshEditingMode.VertexWeights)
                                {
                                    switch (vn)
                                    {
                                        case 0: index = _mesh.Polys[i].Index0; break;
                                        case 1: index = _mesh.Polys[i].Index1; break;
                                        case 2: index = _mesh.Polys[i].Index2; break;

                                        case 3: index = _mesh.Polys[i].Index2; break;
                                        case 4: index = _mesh.Polys[i].Index3; break;
                                        case 5: index = _mesh.Polys[i].Index0; break;
                                    }

                                    pos = _mesh.VertexPositions[index];

                                    if (EditingMode == MeshEditingMode.FaceAttributes)
                                        color = DrawExtraInfo ? new Vector4(1, 1 - strength, 1 - strength, 1) : new Vector4(1, 0, 0, 1);
                                    else if (_previewMesh != null && _mesh.HasWeights)
                                        color = new Vector4(_previewMesh.VertexColors[index], 1.0f);
                                }

                                vtxs[vertexCount] = new SolidVertex(pos) { Color = color };
                                vn++;
                                vertexCount++;
                            }
                    }

                    _faceVertexBuffer.SetData(vtxs);

                    solidEffect.Parameters["Color"].SetValue(Vector4.One);
                    solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                    solidEffect.Techniques[0].Passes[0].Apply();

                    if (!DrawExtraInfo && EditingMode != MeshEditingMode.VertexWeights)
                    {
                        _device.SetRasterizerState(_rasterizerWireframe);
                        _device.SetBlendState(_device.BlendStates.Opaque);
                    }

                    _device.Draw(PrimitiveType.TriangleList, _faceVertexBuffer.ElementCount);
                }

                // Draw model last in face editing only if wireframe mode is set or extra mode is unset
                if (WireframeMode || !DrawExtraInfo)
                    DrawModel(mesh, world * viewProjection);
            }
            else if (EditingMode == MeshEditingMode.Sphere)
            {
                // Draw model first
                DrawModel(mesh, world * viewProjection);

                // Now prepare and draw wireframe sphere
                    
                _device.SetRasterizerState(_rasterizerWireframe);
                _device.SetBlendState(_device.BlendStates.AlphaBlend);
                _device.SetDepthStencilState(_device.DepthStencilStates.DepthRead);

                _device.SetVertexBuffer(_bigSphere.VertexBuffer);
                _device.SetVertexInputLayout(_bigSphere.InputLayout);
                _device.SetIndexBuffer(_bigSphere.IndexBuffer, _bigSphere.IsIndex32Bits);

                var posMatrix = Matrix4x4.Identity * Matrix4x4.CreateTranslation(_mesh.BoundingSphere.Center);
                var finalMatrix = Matrix4x4.CreateScale(_mesh.BoundingSphere.Radius * 2) * posMatrix * viewProjection;

                solidEffect.Parameters["ModelViewProjection"].SetValue(finalMatrix.ToSharpDX());
                solidEffect.Parameters["Color"].SetValue(new Vector4(Vector3.One, 0.5f));
                solidEffect.Techniques[0].Passes[0].Apply();

                _device.DrawIndexed(PrimitiveType.TriangleList, _bigSphere.IndexBuffer.ElementCount);

                // Draw gizmo if needed

                if (DrawExtraInfo)
                {
                    SwapChain.ClearDepth();
                    _gizmo.Draw(viewProjection);
                }
            }
            else if (EditingMode == MeshEditingMode.None)
            {
                // Simply draw model without any indications
                DrawModel(mesh, world * viewProjection);
            }

            if (textToDraw.Count > 0)
            {
                _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                _device.SetBlendState(_device.BlendStates.AlphaBlend);
                SwapChain.RenderText(textToDraw);
            }

            if (linesToDraw.Count > 0)
            {
                var bufferLines = SharpDX.Toolkit.Graphics.Buffer.New(_device, linesToDraw.ToArray(), BufferFlags.VertexBuffer, SharpDX.Direct3D11.ResourceUsage.Default);

                _device.SetVertexBuffer(bufferLines);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, bufferLines));
                _device.SetIndexBuffer(null, false);

                solidEffect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                solidEffect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                solidEffect.CurrentTechnique.Passes[0].Apply();

                _device.Draw(PrimitiveType.LineList, bufferLines.ElementCount);
            }
        }

        private void GenerateBoneColors()
        {
            for (int i = 0; i < _maxBones; i++)
                _boneColors[i] = MathC.GetRandomColorByIndex(i, _maxBones);
        }

        public void SelectElement(int element, bool highlight = false)
        {
            if (EditingMode == MeshEditingMode.FaceAttributes)
                _currentElement = (Mesh.Polys.Count > element) ? element : -1;
            else if (EditingMode != MeshEditingMode.Sphere)
                _currentElement = (Mesh.VertexPositions.Count > element) ? element : -1;

            _highlightFace = highlight;

            if (EditingMode != MeshEditingMode.None)
                Invalidate();
        }

        private void DrawModel(StaticModel mesh, Matrix4x4 world)
        {
            if (mesh.Meshes.Count == 0)
                return;

            if (!WireframeMode && EditingMode == MeshEditingMode.VertexWeights)
                return;

            // Next, draw whole textured mesh.
            // In case mode is set to shininess editing, only draw in wireframe mode to avoid Z-fighting.

            if (WireframeMode)
            {
                _device.SetRasterizerState(_rasterizerWireframe);
                _device.SetBlendState(_device.BlendStates.Opaque);
            }

            var showColors = EditingMode == MeshEditingMode.VertexColorsAndNormals || (EditingMode == MeshEditingMode.VertexEffects && _previewTimer.Enabled);

            var effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];
            effect.Parameters["ModelViewProjection"].SetValue(world.ToSharpDX());
            effect.Parameters["Color"].SetValue(WireframeMode ? new Vector4(1.0f - ClearColor.To3().GetLuma()) : Vector4.One);
            effect.Parameters["StaticLighting"].SetValue(showColors);
            effect.Parameters["ColoredVertices"].SetValue(_tool.DestinationWad.GameVersion == TRVersion.Game.TombEngine);
            effect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
            effect.Parameters["TextureSampler"].SetResource(_bilinear ? _device.SamplerStates.AnisotropicWrap : _device.SamplerStates.PointClamp);
            effect.Parameters["AlphaTest"].SetValue(!WireframeMode && AlphaTest);
            effect.Techniques[0].Passes[0].Apply();

            foreach (var mesh_ in mesh.Meshes)
            {
                if (mesh_.Vertices.Count == 0)
                    continue;

                _device.SetVertexBuffer(0, mesh_.VertexBuffer);
                _device.SetIndexBuffer(mesh_.IndexBuffer, true);
                _layout = VertexInputLayout.FromBuffer(0, mesh_.VertexBuffer);
                _device.SetVertexInputLayout(_layout);

                foreach (var submesh in mesh_.Submeshes)
                {
                    if (!WireframeMode)
                    {
                        if (AlphaTest && submesh.Value.Material.AdditiveBlending)
                            _device.SetBlendState(_device.BlendStates.Additive);
                        else if (AlphaTest)
                            _device.SetBlendState(_device.BlendStates.NonPremultiplied);
                        else
                            _device.SetBlendState(_device.BlendStates.Opaque);

                        if (submesh.Value.Material.DoubleSided)
                            _device.SetRasterizerState(_device.RasterizerStates.CullNone);
                        else
                            _device.SetRasterizerState(_device.RasterizerStates.CullBack);
                    }

                    _device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                }
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

            Camera.Zoom(-e.Delta * _tool.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom / 4);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _lastMousePosition = e.Location;

            if (DrawExtraInfo)
            {
                var result = _gizmo.DoPicking(Ray.GetPickRay(Camera, ClientSize, e.X, e.Y));
                if (result != null)
                {
                    _gizmo.ActivateGizmo(result);
                    Invalidate();
                    return;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                // Warp cursor
                var delta = WarpMouseCursor(e.Location, _lastMousePosition);

                if (e.Button == MouseButtons.Right)
                {
                    if ((ModifierKeys & Keys.Control) == Keys.Control)
                        Camera.Zoom(-delta.Y * _tool.Configuration.RenderingItem_NavigationSpeedMouseZoom);
                    else if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                        Camera.Rotate(delta.X * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate,
                                     -delta.Y * _tool.Configuration.RenderingItem_NavigationSpeedMouseRotate);
                }
                if ((e.Button == MouseButtons.Right && (ModifierKeys & Keys.Shift) == Keys.Shift) ||
                     e.Button == MouseButtons.Middle)
                    Camera.MoveCameraPlane(new Vector3(delta.X, delta.Y, 0) * _tool.Configuration.RenderingItem_NavigationSpeedMouseTranslate);

                Invalidate();
            }
            else if (EditingMode != MeshEditingMode.None &&
                     EditingMode != MeshEditingMode.VertexRemap && 
                     EditingMode != MeshEditingMode.Sphere &&
                     e.Button == MouseButtons.Left)
            {
                TryPickElement(e.X, e.Y, true);
            }
            else if (EditingMode == MeshEditingMode.Sphere)
            {
                var ray = Ray.GetPickRay(Camera, ClientSize, e.X, e.Y);

                if (_gizmo.GizmoUpdateHoverEffect(_gizmo.DoPicking(ray)))
                    Invalidate();
                if (_gizmo.MouseMoved(Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height), ray))
                {
                    Invalidate();
                    if (!_actionStarted)
                    {
                        _tool.UndoManager.PushMeshChanged(this);
                        _actionStarted = true;
                    }
                }
            }

            _lastMousePosition = e.Location;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != MouseButtons.Left)
                return;

            CurrentElement = -1;

            if (EditingMode == MeshEditingMode.Sphere && _gizmo.MouseUp())
                Invalidate();

            if (EditingMode != MeshEditingMode.None)
                TryPickElement(e.X, e.Y);

            _actionStarted = false;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button == MouseButtons.Right)
                ResetCamera();
        }

        private void TryPickElement(float x, float y, bool continuous = false)
        {
            if (_mesh == null)
                return;

            var ray = Ray.GetPickRay(Camera, ClientSize, x, y);

            float distance = float.MaxValue;
            int candidate = -1;

            if (EditingMode == MeshEditingMode.VertexRemap ||
                EditingMode == MeshEditingMode.VertexEffects ||
                EditingMode == MeshEditingMode.VertexWeights ||
                EditingMode == MeshEditingMode.VertexColorsAndNormals)
            {
                // Try to pick a vertex sphere

                var radius = VertexSphereRadius / 2.0f;
                for (int i = 0; i < _mesh.VertexPositions.Count; i++)
                {
                    var vertex = VisibleMesh.VertexPositions[i];
                    var sphere = new BoundingSphere(vertex, radius);
                    float newDistance;

                    if (Collision.RayIntersectsSphere(ray, sphere, out newDistance))
                    {
                        if (newDistance <= distance || candidate == -1)
                        {
                            distance = newDistance;
                            candidate = i;
                        }
                    }

                    // Clickchain is only used for vertex picking since model may have 2 vertices at same coordinate
                    // but not 2 faces with same coordinates (it means Z-fighting will appear and it is a model issue).

                    if (candidate != -1)
                    {
                        if (continuous && candidate != CurrentElement)
                        {
                            CurrentElement = candidate;
                            return;
                        }
                        else if (!continuous && !_clickchain.Contains(candidate))
                        {
                            CurrentElement = candidate;

                            // Reset clickchain in case other coordinate is picked
                            if (_lastElementPos.Count != 1 || _lastElementPos[0] != _mesh.VertexPositions[candidate])
                            {
                                _lastElementPos = new List<Vector3>() { _mesh.VertexPositions[candidate] };
                                _clickchain.Clear();
                            }
                            _clickchain.Add(candidate);
                            return;
                        }
                    }
                }

                if (!continuous)
                {
                    if (_clickchain.Count > 0)
                    {
                        _clickchain.Clear();
                        TryPickElement(x, y); // All similar vertices clicked, restart picking
                    }
                    else
                        CurrentElement = -1;
                }
            }
            else
            {
                // Try to pick a face

                for (int i = 0; i < _mesh.Polys.Count; i++)
                {
                    var poly = _mesh.Polys[i];
                    
                    for (int j = 0; j < (poly.Shape == WadPolygonShape.Quad ? 2 : 1); j++)
                    {
                        var v = new Vector3[3]
                        {
                            _mesh.VertexPositions[(j == 0 ? poly.Index0 : poly.Index2)],
                            _mesh.VertexPositions[(j == 0 ? poly.Index1 : poly.Index3)],
                            _mesh.VertexPositions[(j == 0 ? poly.Index2 : poly.Index0)]
                        };

                        float newDistance;
                        if (Collision.RayIntersectsTriangle(ray, v[0], v[1], v[2], poly.Texture.DoubleSided, out newDistance))
                        {
                            if (newDistance <= distance || candidate == -1)
                            {
                                distance = newDistance;
                                candidate = i;
                            }
                        }
                    }
                }

                if (candidate != -1)
                {
                    if (continuous && candidate != CurrentElement)
                    {
                        CurrentElement = candidate;
                    }
                    else
                    {
                        CurrentElement = candidate;
                        _clickchain.Clear(); // Clickchain is not used for face picking, so clear it just in case.
                    }
                }
            }
        }

        public void InitializeVertexBuffer()
        {
            if (_mesh?.Polys.Count > 0)
            {
                var vertexCount = 0;
                foreach (var poly in _mesh.Polys)
                    if (poly.IsTriangle) vertexCount += 3; else vertexCount += 6;

                _faceVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New<SolidVertex>(_device, vertexCount);
            }

            _littleSphere = GeometricPrimitive.Sphere.New(_device, VertexSphereRadius, 4);
            _normalLength = VertexSphereRadius * 3.0f;
        }

        public void ResetCamera()
        {
            // Smart reset camera which fits an object into window. Later reuse for TE item preview!

            var center = Vector3.Zero;
            var radius = 256.0f;

            if (Mesh != null)
            {
                var bs = Mesh.CalculateBoundingSphere();
                center = bs.Center;
                radius = bs.Radius * 1.15f; // Zoom out a bit
            }

            Camera = new ArcBallCamera(center, 0, 0, -(float)Math.PI / 2, (float)Math.PI / 2, radius * 3, 50, 1000000, (float)Math.PI / 4.0f);
            Invalidate();
        }

        private void TransformVertices()
        {
            if (_mesh == null || _previewMesh == null || _mesh.VertexPositions.Count == 0 || !_mesh.HasAttributes)
                return;

            for (int i = 0; i < _mesh.VertexPositions.Count; i++)
            {
                var v = _mesh.VertexPositions[i];
                var a = _mesh.VertexAttributes[i];

                var hash = MathC.GetVector3Hash(v).GetHashCode();

                var wibble = (float)Math.Sin((((_frameCount + hash) % 64) / 64.0f) * (Math.PI * 2));

                var newPos = v;
                var newCol = _mesh.HasColors ? _mesh.VertexColors[i] : Vector3.One;

                if (a.Glow > 0.0f)
                {
                    float intensity = a.Glow / 63.0f * (float)MathC.Lerp(-0.5f, 1.0f, wibble * 0.5f + 0.5f);
                    newCol = Vector3.Min(newCol + new Vector3(intensity, intensity, intensity), new Vector3(2));
                }

                if (a.Move > 0.0f)
                    newPos.Y += wibble * a.Move / 63.0f * 128.0f; // 128 units offset to top and bottom (256 total)

                _previewMesh.VertexPositions[i] = newPos;

                if (!_previewMesh.HasColors)
                    _previewMesh.VertexColors.Add(newCol);
                else
                    _previewMesh.VertexColors[i] = newCol;
            }
        }

        public void ColorizeVertexWeights()
        {
            if (_mesh == null || _mesh.VertexPositions.Count == 0 || !_mesh.HasWeights)
                return;

            if (_previewMesh == null)
                _previewMesh = _mesh.Clone();

            for (int i = 0; i < _mesh.VertexPositions.Count; i++)
            {
                var color = Vector4.UnitW;

                for (int w = 0; w < 4; w++)
                {
                    int boneIndex = _mesh.VertexWeights[i].Index[w];
                    float weight = _mesh.VertexWeights[i].Weight[w];

                    if (boneIndex >= 0 && boneIndex < _maxBones && weight > 0)
                    {
                        var boneColor = _boneColors[boneIndex];
                        color += boneColor * weight;
                    }
                }

                if (!_previewMesh.HasColors)
                    _previewMesh.VertexColors.Add(color.To3());
                else
                    _previewMesh.VertexColors[i] = color.To3();
            }
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            TransformVertices();
            Invalidate();
            _frameCount++;
        }

        public void StartPreview()
        {
            _frameCount = 0;
            _previewMesh = _mesh.Clone();
            _previewTimer.Start();
        }

        public void StopPreview()
        {
            _previewTimer.Stop();
            _previewMesh = null;
            Invalidate();
        }
    }
}
