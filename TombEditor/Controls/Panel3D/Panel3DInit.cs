using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.Graphics.Primitives;
using TombLib.LevelData;
using TombLib.Rendering;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private Buffer<SolidVertex> _flybyPyramidSolidVertexBuffer;
        private Buffer<SolidVertex> _flybyPyramidAccentVertexBuffer;
        private Buffer<SolidVertex> _flybyPyramidWireVertexBuffer;

        private const float _flybyPyramidReferenceFov = 80.0f;
        private const float _flybyPyramidSelectedLengthScale = 1.5f;
        private const float _flybyPyramidSelectedBaseScale = 0.5f;
        private const float _flybyPyramidInactiveLength = 300.0f;
        private const float _flybyPyramidInactiveBaseHeight = 200.0f;
        private const float _flybyPyramidNormalizedHalfWidth = 1.0f;
        private const float _flybyPyramidNormalizedHalfHeight = 1.0f;
        private const float _flybyPyramidNormalizedLength = 1.0f;
        private const int _flybyPyramidPerimeterSegments = 5;
        private const int _flybyPyramidStripeLineCount = 8;
        private const float _flybyPyramidStripeStart = 0.95f;
        private const float _flybyPyramidStripeEnd = 1.0f;
        private static readonly SolidVertex[] _flybyPyramidSolidVertices = BuildFlybyPyramidSolidVertices();
        private static readonly SolidVertex[] _flybyPyramidAccentVertices = BuildFlybyPyramidLineVertices(false);
        private static readonly SolidVertex[] _flybyPyramidWireVertices = BuildFlybyPyramidLineVertices(true);

        public override void InitializeRendering(RenderingDevice device, bool antialias, ObjectRenderingQuality objectQuality)
        {
            base.InitializeRendering(device, antialias, objectQuality);

            // Fall back to half of the max. page count for texture allocation, if editor is in safe mode.
            // This workaround is needed for very old PCs which have troubles with providing D3D device caps.

            var texDescription = new RenderingTextureAllocator.Description();
            if (_editor.Configuration.Rendering3D_SafeMode)
                texDescription = new RenderingTextureAllocator.Description { Size = new VectorInt3(texDescription.Size.X, texDescription.Size.X, RenderingTextureAllocator.SafePageCount) };

            _renderingTextures = device.CreateTextureAllocator(texDescription);
            _renderingStateBuffer = device.CreateStateBuffer();
            _fontTexture = device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(512, 512, 2) });

            _fontDefault = device.CreateFont(new RenderingFont.Description
            {
                FontName = _editor.Configuration.Rendering3D_FontName,
                FontSize = _editor.Configuration.Rendering3D_FontSize,
                FontIsBold = _editor.Configuration.Rendering3D_FontIsBold,
                TextureAllocator = _fontTexture
            });

            // Legacy
            {
                _legacyDevice = DeviceManager.DefaultDeviceManager.___LegacyDevice;

                int atlasSize = objectQuality switch
                {
                    ObjectRenderingQuality.High => 4096,
                    ObjectRenderingQuality.Medium => 1024,
                    _ => 512
                };

                int maxAllocationSize = objectQuality switch
                {
                    ObjectRenderingQuality.High => 2048,
                    ObjectRenderingQuality.Medium => 256,
                    _ => 128
                };

                _wadRenderer = new WadRenderer(_legacyDevice, true, true, atlasSize, maxAllocationSize, false);
                // Initialize vertex buffers
                _ghostBlockVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New<SolidVertex>(_legacyDevice, 84);
                _boxVertexBuffer = new BoundingBox(new Vector3(-_littleCubeRadius), new Vector3(_littleCubeRadius)).GetVertexBuffer(_legacyDevice);
                _flybyPyramidSolidVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New(_legacyDevice, _flybyPyramidSolidVertices, SharpDX.Direct3D11.ResourceUsage.Dynamic);
                _flybyPyramidAccentVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New(_legacyDevice, _flybyPyramidAccentVertices, SharpDX.Direct3D11.ResourceUsage.Dynamic);
                _flybyPyramidWireVertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New(_legacyDevice, _flybyPyramidWireVertices, SharpDX.Direct3D11.ResourceUsage.Dynamic);

                // Maybe I could use this as bounding box, scaling it properly before drawing
                _linesCube = GeometricPrimitive.LinesCube.New(_legacyDevice, 128, 128, 128);

                // This sphere will be scaled up and down multiple times for using as In & Out of lights
                _sphere = GeometricPrimitive.Sphere.New(_legacyDevice, 1024, 6);

                //Little cubes and little spheres are used as mesh for lights, cameras, sinks, etc
                _littleCube = GeometricPrimitive.Cube.New(_legacyDevice, 2 * _littleCubeRadius);
                _littleSphere = GeometricPrimitive.Sphere.New(_legacyDevice, 2 * _littleSphereRadius, 8);

                _cone = GeometricPrimitive.Cone.New(_legacyDevice, _coneRadius, _coneRadius);

                // This effect is used for editor special meshes like sinks, cameras, light meshes, etc
                new BasicEffect(_legacyDevice);

                // Initialize the rasterizer state for wireframe drawing
                var renderStateDesc =
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
                _rasterizerWireframe = RasterizerState.New(_legacyDevice, renderStateDesc);

                _rasterizerStateDepthBias = RasterizerState.New(_legacyDevice, new SharpDX.Direct3D11.RasterizerStateDescription
                {
                    CullMode = SharpDX.Direct3D11.CullMode.Back,
                    FillMode = SharpDX.Direct3D11.FillMode.Solid,
                    DepthBias = -2,
                    SlopeScaledDepthBias = -2
                });

                _gizmo = new Gizmo(DeviceManager.DefaultDeviceManager.___LegacyEffects["Solid"]);

                ResetCamera(true);
            }
        }

        RenderingDrawingRoom CacheRoom(Room room)
        {
            var sectorTextures = new SectorTextureDefault
            {
                ColoringInfo = _editor.SectorColoringManager.ColoringInfo,
                DrawIllegalSlopes = ShowIllegalSlopes,
                DrawSlideDirections = ShowSlideDirections,
                ProbeAttributesThroughPortals = _editor.Configuration.UI_ProbeAttributesThroughPortals,
                HideHiddenRooms = DisablePickingForHiddenRooms
            };

            if (_editor.SelectedRoom == room)
            {
                sectorTextures.HighlightArea = _editor.HighlightedSectors.Area;
                sectorTextures.SelectionArea = _editor.SelectedSectors.Area;
                sectorTextures.SelectionArrow = _editor.SelectedSectors.Arrow;
            }

            return Device.CreateDrawingRoom(
                    new RenderingDrawingRoom.Description
                    {
                        Room = room,
                        TextureAllocator = _renderingTextures,
                        SectorTextureGet = sectorTextures.Get
                    });
        }

        private static SolidVertex[] BuildFlybyPyramidSolidVertices()
        {
            var vertices = new List<SolidVertex>();
            var apex = Vector3.Zero;
            var baseCorners = GetFlybyPyramidBaseCorners();

            for (int i = 0; i < baseCorners.Length; i++)
                AddFlybyPyramidTriangle(vertices, apex, baseCorners[i], baseCorners[(i + 1) % baseCorners.Length]);

            AddFlybyPyramidTriangle(vertices, baseCorners[0], baseCorners[1], baseCorners[2]);
            AddFlybyPyramidTriangle(vertices, baseCorners[0], baseCorners[2], baseCorners[3]);
            return vertices.ToArray();
        }

        private static SolidVertex[] BuildFlybyPyramidLineVertices(bool includeOutline)
        {
            var vertices = new List<SolidVertex>();
            var perimeterPoints = BuildFlybyPyramidPerimeterPoints();

            foreach (var point in perimeterPoints)
                AddFlybyPyramidLine(vertices, Vector3.Zero, point);

            if (includeOutline)
                for (int i = 0; i < perimeterPoints.Count; i++)
                    AddFlybyPyramidLine(vertices, perimeterPoints[i], perimeterPoints[(i + 1) % perimeterPoints.Count]);

            AddFlybyPyramidTopStripe(vertices);
            return vertices.ToArray();
        }

        private static Vector3[] GetFlybyPyramidBaseCorners()
        {
            return new[]
            {
                CreateFlybyPyramidBasePoint(-1.0f, 1.0f),
                CreateFlybyPyramidBasePoint(1.0f, 1.0f),
                CreateFlybyPyramidBasePoint(1.0f, -1.0f),
                CreateFlybyPyramidBasePoint(-1.0f, -1.0f)
            };
        }

        private static List<Vector3> BuildFlybyPyramidPerimeterPoints()
        {
            var points = new List<Vector3>(_flybyPyramidPerimeterSegments * 4);

            AddFlybyPyramidEdgePoints(points, new Vector2(-1.0f, 1.0f), new Vector2(1.0f, 1.0f));
            AddFlybyPyramidEdgePoints(points, new Vector2(1.0f, 1.0f), new Vector2(1.0f, -1.0f));
            AddFlybyPyramidEdgePoints(points, new Vector2(1.0f, -1.0f), new Vector2(-1.0f, -1.0f));
            AddFlybyPyramidEdgePoints(points, new Vector2(-1.0f, -1.0f), new Vector2(-1.0f, 1.0f));

            return points;
        }

        private static void AddFlybyPyramidEdgePoints(List<Vector3> points, Vector2 start, Vector2 end)
        {
            for (int i = 0; i < _flybyPyramidPerimeterSegments; i++)
            {
                float interpolation = i / (float)_flybyPyramidPerimeterSegments;
                var point = Vector2.Lerp(start, end, interpolation);
                points.Add(CreateFlybyPyramidBasePoint(point.X, point.Y));
            }
        }

        private static void AddFlybyPyramidTopStripe(List<SolidVertex> vertices)
        {
            int segmentCount = Math.Max(_flybyPyramidStripeLineCount - 1, 1);

            for (int i = 0; i < _flybyPyramidStripeLineCount; i++)
            {
                float interpolation = i / (float)segmentCount;
                float depth = _flybyPyramidStripeStart + ((_flybyPyramidStripeEnd - _flybyPyramidStripeStart) * interpolation);
                AddFlybyPyramidLine(vertices,
                    new Vector3(-_flybyPyramidNormalizedHalfWidth * depth, _flybyPyramidNormalizedHalfHeight * depth, _flybyPyramidNormalizedLength * depth),
                    new Vector3(_flybyPyramidNormalizedHalfWidth * depth, _flybyPyramidNormalizedHalfHeight * depth, _flybyPyramidNormalizedLength * depth));
            }
        }

        private static Vector3 CreateFlybyPyramidBasePoint(float x, float y)
        {
            return new Vector3(x * _flybyPyramidNormalizedHalfWidth, y * _flybyPyramidNormalizedHalfHeight, _flybyPyramidNormalizedLength);
        }

        private static void AddFlybyPyramidTriangle(List<SolidVertex> vertices, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            vertices.Add(new SolidVertex(p0));
            vertices.Add(new SolidVertex(p1));
            vertices.Add(new SolidVertex(p2));
        }

        private static void AddFlybyPyramidLine(List<SolidVertex> vertices, Vector3 start, Vector3 end)
        {
            vertices.Add(new SolidVertex(start));
            vertices.Add(new SolidVertex(end));
        }
    }
}
