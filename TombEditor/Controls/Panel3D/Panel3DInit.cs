using TombLib.Graphics.Dx11Toolkit;
using System.Numerics;
using TombLib.Graphics.Primitives;
using TombLib.Graphics;
using TombLib;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Controls;
using System;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
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
                _ghostBlockVertexBuffer = TombLib.Graphics.Dx11Toolkit.Buffer.Vertex.New<SolidVertex>(_legacyDevice, 84);
                _boxVertexBuffer = new BoundingBox(new Vector3(-_littleCubeRadius), new Vector3(_littleCubeRadius)).GetVertexBuffer(_legacyDevice);

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
                    new RasterizerStateDescription
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
                    };
                _rasterizerWireframe = RasterizerState.New(_legacyDevice, renderStateDesc);

                _rasterizerStateDepthBias = RasterizerState.New(_legacyDevice, new RasterizerStateDescription
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
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
    }
}
