using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.Graphics.Primitives;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;
using TombLib.LevelData.SectorStructs;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private void DrawDebugLines(Effect effect)
        {
            var drawRoomBounds = _editor.Configuration.Rendering3D_AlwaysShowCurrentRoomBounds;

            if (!_drawHeightLine && !drawRoomBounds)
                return;

            _legacyDevice.SetRasterizerState(_rasterizerWireframe);
            Matrix4x4 model = Matrix4x4.CreateTranslation(_editor.SelectedRoom.WorldPos);
            effect.Parameters["ModelViewProjection"].SetValue((model * _viewProjection).ToSharpDX());
            effect.Parameters["Color"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));

            if (_drawHeightLine)
            {
                _legacyDevice.SetVertexBuffer(_objectHeightLineVertexBuffer);
                _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _objectHeightLineVertexBuffer));
                Matrix4x4 model2 = Matrix4x4.CreateTranslation(_editor.SelectedObject.Room.WorldPos);
                effect.Parameters["ModelViewProjection"].SetValue((model2 * _viewProjection).ToSharpDX());
                effect.CurrentTechnique.Passes[0].Apply();
                _legacyDevice.Draw(PrimitiveType.LineList, 2);
            }

            if (!_flyModeTimer.Enabled && drawRoomBounds)
            {
                if (_editor.SelectedRooms.Count > 0)
                    foreach (Room room in _editor.SelectedRooms)
                        // Draw room bounding box around every selected Room
                        DrawRoomBoundingBox(effect, room);
                else
                    // Draw room bounding box
                    DrawRoomBoundingBox(effect, _editor.SelectedRoom);
            }
        }

        private void DrawText(Room[] roomsToDraw, List<Text> textToDraw)
        {
            // Draw room names
            if (ShowRoomNames)
            {
                Size size = ClientSize;
                for (int i = 0; i < roomsToDraw.Length; i++)
                {
                    var pos = (Matrix4x4.CreateTranslation(roomsToDraw[i].WorldPos) * _viewProjection).TransformPerspectively(roomsToDraw[i].GetLocalCenter());
                    if (pos.Z <= 1.0f)
                        textToDraw.Add(new Text
                        {
                            Font = _fontDefault,
                            Pos = pos.To2(),
                            Overlay = _editor.Configuration.Rendering3D_DrawFontOverlays,
                            String = roomsToDraw[i].Name
                        });
                }
            }

            // Draw North, South, East and West
            if (ShowCardinalDirections)
                DrawCardinalDirections(textToDraw);

            // Construct debug string
            string DebugString = "";
            if (_editor.Configuration.Rendering3D_ShowFPS)
                DebugString += "FPS: " + Math.Round(1.0f / _watch.Elapsed.TotalSeconds, 2) + "\n";

            if (_editor.SelectedObject != null)
                DebugString += "Selected Object: " + _editor.SelectedObject.ToShortString();

            // Draw debug string
            textToDraw.Add(new Text
            {
                Font = _fontDefault,
                PixelPos = new Vector2(10, -10),
                Alignment = new Vector2(0.0f, 0.0f),
                Overlay = _editor.Configuration.Rendering3D_DrawFontOverlays,
                String = DebugString
            });

            // If multiple objects are selected, display multiselection label
            var activeObjectGroup = _editor.SelectedObject as ObjectGroup;
            if (activeObjectGroup != null)
            {
                // Add text message
                textToDraw.Add(CreateTextTagForObject(
                    activeObjectGroup.RotationPositionMatrix * _viewProjection,
                    $"Group of {activeObjectGroup.Count()} objects" +
                    "\n" + GetObjectPositionString(activeObjectGroup.Room, activeObjectGroup)));
            }

            // Finish strings
            SwapChain.RenderText(textToDraw);
        }

        private void DrawRoomBoundingBox(Effect solidEffect, Room room)
        {
            _legacyDevice.SetVertexBuffer(_linesCube.VertexBuffer);
            _legacyDevice.SetVertexInputLayout(_linesCube.InputLayout);
            _legacyDevice.SetIndexBuffer(_linesCube.IndexBuffer, false);

            float height = room.GetHighestCorner() - room.GetLowestCorner();
            Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(room.NumXSectors * 4.0f, height / Level.FullClickHeight, room.NumZSectors * 4.0f);
            float boxX = room.WorldPos.X + room.NumXSectors * Level.SectorSizeUnit / 2.0f;
            float boxY = room.WorldPos.Y + (room.GetHighestCorner() + room.GetLowestCorner()) / 2.0f;
            float boxZ = room.WorldPos.Z + room.NumZSectors * Level.SectorSizeUnit / 2.0f;
            Matrix4x4 translateMatrix = Matrix4x4.CreateTranslation(new Vector3(boxX, boxY, boxZ));
            solidEffect.Parameters["ModelViewProjection"].SetValue((scaleMatrix * translateMatrix * _viewProjection).ToSharpDX());
            solidEffect.CurrentTechnique.Passes[0].Apply();
            _legacyDevice.DrawIndexed(PrimitiveType.LineList, _linesCube.IndexBuffer.ElementCount);
        }

        private void DrawBoundingBoxes(Effect solidEffect, List<ObjectInstance> objectList)
        {
            if (objectList.Count == 0)
                return;

            _legacyDevice.SetRasterizerState(_rasterizerWireframe);
            _legacyDevice.SetBlendState(_legacyDevice.BlendStates.NonPremultiplied);
            _legacyDevice.SetVertexBuffer(_linesCube.VertexBuffer);
            _legacyDevice.SetVertexInputLayout(_linesCube.InputLayout);
            _legacyDevice.SetIndexBuffer(_linesCube.IndexBuffer, false);

            foreach (var obj in objectList)
            {
                if (obj is MoveableInstance)
                {
                    var mov = obj as MoveableInstance;
                    var model = _editor?.Level?.Settings?.WadTryGetMoveable((obj as MoveableInstance).WadObjectId);
                    if (model == null || model.Animations.Count == 0 || model.Animations[0].KeyFrames.Count == 0)
                        continue;

                    var frame = model.Animations[0].KeyFrames[0];

                    var rotPosMatrix = Matrix4x4.CreateScale(frame.BoundingBox.Size / _littleCubeRadius / 2.0f) *
                                       Matrix4x4.CreateTranslation(frame.BoundingBox.Center) *
                                       mov.RotationPositionMatrix;

                    solidEffect.Parameters["ModelViewProjection"].SetValue((rotPosMatrix * _viewProjection).ToSharpDX());
                }

                if (obj is StaticInstance)
                {
                    var stat = obj as StaticInstance;
                    var mesh = _editor?.Level?.Settings?.WadTryGetStatic((obj as StaticInstance).WadObjectId);
                    if (mesh == null || mesh.Mesh == null || mesh.Mesh.BoundingBox.Size.Length() == 0.0f)
                        continue;

                    var rotPosMatrix = Matrix4x4.CreateScale(mesh.CollisionBox.Size * stat.Scale / _littleCubeRadius / 2.0f) *
                                       Matrix4x4.CreateTranslation(mesh.CollisionBox.Center * stat.Scale) *
                                       stat.RotationPositionMatrix;

                    solidEffect.Parameters["ModelViewProjection"].SetValue((rotPosMatrix * _viewProjection).ToSharpDX());
                }

                if (_highlightedObjects.Contains(obj)) // Selection
                    solidEffect.Parameters["Color"].SetValue(_editor.Configuration.UI_ColorScheme.ColorSelection);
                else
                    solidEffect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                solidEffect.CurrentTechnique.Passes[0].Apply();
                _legacyDevice.DrawIndexed(PrimitiveType.LineList, _linesCube.IndexBuffer.ElementCount);
            }
        }

        private void DrawFlybyPath(Effect effect)
        {
            // Add the path of the flyby
            if (_editor.SelectedObject is FlybyCameraInstance &&
                AddFlybyPath(((FlybyCameraInstance)_editor.SelectedObject).Sequence))
            {
                _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullNone);
                _legacyDevice.SetVertexBuffer(_flybyPathVertexBuffer);
                _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _flybyPathVertexBuffer));
                effect.Parameters["ModelViewProjection"].SetValue(_viewProjection.ToSharpDX());
                effect.Parameters["Color"].SetValue(Vector4.One);
                effect.CurrentTechnique.Passes[0].Apply();
                _legacyDevice.Draw(PrimitiveType.TriangleList, _flybyPathVertexBuffer.ElementCount);
            }
        }

        private void DrawSectorSplitHighlights(Effect effect)
        {
            if (_editor.HighlightedSplit == 0 || _editor.SelectedSectors == SectorSelection.None)
                return;

            int splitIndex = _editor.HighlightedSplit - 2;
            Room currentRoom = _editor.SelectedRoom;

            var vertices = new List<SolidVertex>();

            const int
                XZ_OFFSET = 8,
                HEIGHT = 24;

            void DrawRibbon(Vector3 p1, Vector3 p2, int height, int xOffset, int yOffset, int zOffset)
            {
                float halfHeight = height / 2.0f;

                vertices.Add(new SolidVertex(new Vector3((p1.X * Level.SectorSizeUnit) + xOffset, p1.Y + halfHeight + yOffset, (p1.Z * Level.SectorSizeUnit) + zOffset)));
                vertices.Add(new SolidVertex(new Vector3((p2.X * Level.SectorSizeUnit) + xOffset, p2.Y + halfHeight + yOffset, (p2.Z * Level.SectorSizeUnit) + zOffset)));
                vertices.Add(new SolidVertex(new Vector3((p1.X * Level.SectorSizeUnit) + xOffset, p1.Y - halfHeight + yOffset, (p1.Z * Level.SectorSizeUnit) + zOffset)));

                vertices.Add(new SolidVertex(new Vector3((p1.X * Level.SectorSizeUnit) + xOffset, p1.Y - halfHeight + yOffset, (p1.Z * Level.SectorSizeUnit) + zOffset)));
                vertices.Add(new SolidVertex(new Vector3((p2.X * Level.SectorSizeUnit) + xOffset, p2.Y + halfHeight + yOffset, (p2.Z * Level.SectorSizeUnit) + zOffset)));
                vertices.Add(new SolidVertex(new Vector3((p2.X * Level.SectorSizeUnit) + xOffset, p2.Y - halfHeight + yOffset, (p2.Z * Level.SectorSizeUnit) + zOffset)));
            }

            void HandlePositiveZ(int x, int z, SectorSurface surface, int yOffset)
            {
                if (surface.DiagonalSplit is DiagonalSplit.XpZn or DiagonalSplit.XnZn)
                    return;

                Vector3
                    p1 = new Vector3(x + 1, surface.XpZp + yOffset, z + 1) + currentRoom.Position,
                    p2 = new Vector3(x, surface.XnZp + yOffset, z + 1) + currentRoom.Position;

                DrawRibbon(p1, p2, HEIGHT, 0, 0, XZ_OFFSET);
            }

            void HandlePositiveX(int x, int z, SectorSurface surface, int yOffset)
            {
                if (surface.DiagonalSplit is DiagonalSplit.XnZp or DiagonalSplit.XnZn)
                    return;

                Vector3
                    p1 = new Vector3(x + 1, surface.XpZn + yOffset, z) + currentRoom.Position,
                    p2 = new Vector3(x + 1, surface.XpZp + yOffset, z + 1) + currentRoom.Position;

                DrawRibbon(p1, p2, HEIGHT, XZ_OFFSET, 0, 0);
            }

            void HandleNegativeZ(int x, int z, SectorSurface surface, int yOffset)
            {
                if (surface.DiagonalSplit is DiagonalSplit.XpZp or DiagonalSplit.XnZp)
                    return;

                Vector3
                    p1 = new Vector3(x, surface.XnZn + yOffset, z) + currentRoom.Position,
                    p2 = new Vector3(x + 1, surface.XpZn + yOffset, z) + currentRoom.Position;

                DrawRibbon(p1, p2, HEIGHT, 0, 0, -XZ_OFFSET);
            }

            void HandleNegativeX(int x, int z, SectorSurface surface, int yOffset)
            {
                if (surface.DiagonalSplit is DiagonalSplit.XpZn or DiagonalSplit.XpZp)
                    return;

                Vector3
                    p1 = new Vector3(x, surface.XnZp + yOffset, z + 1) + currentRoom.Position,
                    p2 = new Vector3(x, surface.XnZn + yOffset, z) + currentRoom.Position;

                DrawRibbon(p1, p2, HEIGHT, -XZ_OFFSET, 0, 0);
            }

            void HandleDiagonal(int x, int z, SectorSurface surface, int yOffset)
            {
                Vector3 p1, p2;

                switch (surface.DiagonalSplit)
                {
                    case DiagonalSplit.XnZp:
                        p1 = new Vector3(x, surface.XnZn + yOffset, z) + currentRoom.Position;
                        p2 = new Vector3(x + 1, surface.XpZp + yOffset, z + 1) + currentRoom.Position;

                        DrawRibbon(p1, p2, HEIGHT, XZ_OFFSET, 0, -XZ_OFFSET);
                        break;

                    case DiagonalSplit.XpZp:
                        p1 = new Vector3(x, surface.XnZp + yOffset, z + 1) + currentRoom.Position;
                        p2 = new Vector3(x + 1, surface.XpZn + yOffset, z) + currentRoom.Position;

                        DrawRibbon(p1, p2, HEIGHT, -XZ_OFFSET, 0, -XZ_OFFSET);
                        break;

                    case DiagonalSplit.XnZn:
                        p1 = new Vector3(x + 1, surface.XpZn + yOffset, z) + currentRoom.Position;
                        p2 = new Vector3(x, surface.XnZp + yOffset, z + 1) + currentRoom.Position;

                        DrawRibbon(p1, p2, HEIGHT, XZ_OFFSET, 0, XZ_OFFSET);
                        break;

                    case DiagonalSplit.XpZn:
                        p1 = new Vector3(x + 1, surface.XpZp + yOffset, z + 1) + currentRoom.Position;
                        p2 = new Vector3(x, surface.XnZn + yOffset, z) + currentRoom.Position;

                        DrawRibbon(p1, p2, HEIGHT, -XZ_OFFSET, 0, XZ_OFFSET);
                        break;
                }
            }

            for (int x = _editor.SelectedSectors.Area.X0; x <= _editor.SelectedSectors.Area.X1; x++)
                for (int z = _editor.SelectedSectors.Area.Y0; z <= _editor.SelectedSectors.Area.Y1; z++)
                {
                    Sector sector = currentRoom.Sectors[x, z],
                        targetSector = sector;

                    int yOffset = 0;

                    if (sector.WallPortal is not null)
                    {
                        RoomSectorPair pair = currentRoom.GetSectorTryThroughPortal(x, z);

                        if (pair.Room != currentRoom && pair.Sector is not null)
                        {
                            targetSector = pair.Sector;
                            yOffset = pair.Room.Position.Y - currentRoom.Position.Y;
                        }
                    }

                    if (splitIndex is < 0 or > 7) // QA or WS
                    {
                        // PositiveZ Floor
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_PositiveZ_QA)))
                            HandlePositiveZ(x, z, targetSector.Floor, yOffset);
                        // PositiveZ Ceiling
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_PositiveZ_WS)))
                            HandlePositiveZ(x, z, targetSector.Ceiling, yOffset);

                        // PositiveX Floor
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_PositiveX_QA)))
                            HandlePositiveX(x, z, targetSector.Floor, yOffset);
                        // PositiveX Ceiling
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_PositiveX_WS)))
                            HandlePositiveX(x, z, targetSector.Ceiling, yOffset);

                        // NegativeZ Floor
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_NegativeZ_QA)))
                            HandleNegativeZ(x, z, targetSector.Floor, yOffset);
                        // NegativeZ Ceiling
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_NegativeZ_WS)))
                            HandleNegativeZ(x, z, targetSector.Ceiling, yOffset);

                        // NegativeX Floor
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_NegativeX_QA)))
                            HandleNegativeX(x, z, targetSector.Floor, yOffset);
                        // NegativeX Ceiling
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_NegativeX_WS)))
                            HandleNegativeX(x, z, targetSector.Ceiling, yOffset);

                        // Diagonal Floor
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_Diagonal_QA)))
                            HandleDiagonal(x, z, targetSector.Floor, yOffset);
                        // Diagonal Ceiling
                        if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFace.Wall_Diagonal_WS)))
                            HandleDiagonal(x, z, targetSector.Ceiling, yOffset);
                    }
                    else // Actual splits
                    {
                        // Floor split
                        if (splitIndex < targetSector.ExtraFloorSplits.Count)
                        {
                            var floorSurface = new SectorSurface
                            {
                                XnZp = targetSector.ExtraFloorSplits[splitIndex].XnZp,
                                XpZp = targetSector.ExtraFloorSplits[splitIndex].XpZp,
                                XpZn = targetSector.ExtraFloorSplits[splitIndex].XpZn,
                                XnZn = targetSector.ExtraFloorSplits[splitIndex].XnZn,
                                DiagonalSplit = targetSector.Floor.DiagonalSplit
                            };

                            // PositiveZ
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveZ, splitIndex))))
                                HandlePositiveZ(x, z, floorSurface, yOffset);

                            // PositiveX
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.PositiveX, splitIndex))))
                                HandlePositiveX(x, z, floorSurface, yOffset);

                            // NegativeZ
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeZ, splitIndex))))
                                HandleNegativeZ(x, z, floorSurface, yOffset);

                            // NegativeX
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.NegativeX, splitIndex))))
                                HandleNegativeX(x, z, floorSurface, yOffset);

                            // Diagonal
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraFloorSplitFace(Direction.Diagonal, splitIndex))))
                                HandleDiagonal(x, z, floorSurface, yOffset);
                        }

                        // Ceiling split
                        if (splitIndex < targetSector.ExtraCeilingSplits.Count)
                        {
                            var ceilingSurface = new SectorSurface
                            {
                                XnZp = targetSector.ExtraCeilingSplits[splitIndex].XnZp,
                                XpZp = targetSector.ExtraCeilingSplits[splitIndex].XpZp,
                                XpZn = targetSector.ExtraCeilingSplits[splitIndex].XpZn,
                                XnZn = targetSector.ExtraCeilingSplits[splitIndex].XnZn,
                                DiagonalSplit = targetSector.Ceiling.DiagonalSplit
                            };

                            // PositiveZ
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveZ, splitIndex))))
                                HandlePositiveZ(x, z, ceilingSurface, yOffset);

                            // PositiveX
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.PositiveX, splitIndex))))
                                HandlePositiveX(x, z, ceilingSurface, yOffset);

                            // NegativeZ
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeZ, splitIndex))))
                                HandleNegativeZ(x, z, ceilingSurface, yOffset);

                            // NegativeX
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.NegativeX, splitIndex))))
                                HandleNegativeX(x, z, ceilingSurface, yOffset);

                            // Diagonal
                            if (currentRoom.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, SectorFaceExtensions.GetExtraCeilingSplitFace(Direction.Diagonal, splitIndex))))
                                HandleDiagonal(x, z, ceilingSurface, yOffset);
                        }
                    }
                }

            if (vertices.Count == 0)
                return;

            using Buffer<SolidVertex> buffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New(_legacyDevice, vertices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Dynamic);

            _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);
            _legacyDevice.SetVertexBuffer(buffer);
            _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, buffer));
            effect.Parameters["ModelViewProjection"].SetValue(_viewProjection.ToSharpDX());
            effect.Parameters["Color"].SetValue(Vector4.One);
            effect.CurrentTechnique.Passes[0].Apply();
            _legacyDevice.Draw(PrimitiveType.TriangleList, buffer.ElementCount);
        }

        private void DrawLights(Effect effect, Room[] roomsWhoseObjectsToDraw, List<Text> textToDraw, List<Sprite> sprites)
        {
            _legacyDevice.SetRasterizerState(_rasterizerWireframe);
            _legacyDevice.SetVertexBuffer(_littleSphere.VertexBuffer);
            _legacyDevice.SetVertexInputLayout(_littleSphere.InputLayout);
            _legacyDevice.SetIndexBuffer(_littleSphere.IndexBuffer, _littleSphere.IsIndex32Bits);

            var lights = roomsWhoseObjectsToDraw.SelectMany(r => r.Objects).OfType<LightInstance>();

            foreach (var light in lights)
            {
                var color = Vector4.One;

                if (light.Type == LightType.Point)
                    color = new Vector4(1.0f, 1.0f, 0.25f, 1.0f);
                if (light.Type == LightType.Spot)
                    color = new Vector4(1.0f, 1.0f, 0.25f, 1.0f);
                if (light.Type == LightType.FogBulb)
                    color = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
                if (light.Type == LightType.Shadow)
                    color = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
                if (light.Type == LightType.Effect)
                    color = new Vector4(1.0f, 1.0f, 0.25f, 1.0f);
                if (light.Type == LightType.Sun)
                    color = new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
                if (_highlightedObjects.Contains(light))
                    color = _editor.Configuration.UI_ColorScheme.ColorSelection;

                DrawOrQueueServiceObject(light, _littleSphere, color, effect, sprites);
            }

            // Draw cone, light spheres etc.

            if (_editor.SelectedObject is LightInstance && lights.Contains(_editor.SelectedObject))
            {
                var light = (LightInstance)_editor.SelectedObject;
                if (ShowLightMeshes)
                    if (light.Type == LightType.Point || light.Type == LightType.Shadow || light.Type == LightType.FogBulb)
                    {
                        _legacyDevice.SetVertexBuffer(_sphere.VertexBuffer);
                        _legacyDevice.SetVertexInputLayout(_sphere.InputLayout);
                        _legacyDevice.SetIndexBuffer(_sphere.IndexBuffer, _sphere.IsIndex32Bits);

                        Matrix4x4 model;

                        if (light.Type == LightType.Point || light.Type == LightType.Shadow)
                        {
                            model = Matrix4x4.CreateScale(light.InnerRange * 2.0f) * light.ObjectMatrix;
                            effect.Parameters["ModelViewProjection"].SetValue((model * _viewProjection).ToSharpDX());
                            effect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                            effect.CurrentTechnique.Passes[0].Apply();
                            _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);
                        }

                        model = Matrix4x4.CreateScale(light.OuterRange * 2.0f) * light.ObjectMatrix;
                        effect.Parameters["ModelViewProjection"].SetValue((model * _viewProjection).ToSharpDX());
                        effect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                        effect.CurrentTechnique.Passes[0].Apply();
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _sphere.IndexBuffer.ElementCount);
                    }
                    else if (light.Type == LightType.Spot)
                    {
                        _legacyDevice.SetVertexBuffer(_cone.VertexBuffer);
                        _legacyDevice.SetVertexInputLayout(_cone.InputLayout);
                        _legacyDevice.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);

                        // Inner cone
                        float coneAngle = (float)Math.Atan2(512, 1024);
                        float lenScaleH = light.InnerRange;
                        float lenScaleW = light.InnerAngle * (float)(Math.PI / 180) / coneAngle * lenScaleH;

                        Matrix4x4 Model = Matrix4x4.CreateScale(lenScaleW, lenScaleW, lenScaleH) * light.ObjectMatrix;
                        effect.Parameters["ModelViewProjection"].SetValue((Model * _viewProjection).ToSharpDX());
                        effect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                        effect.CurrentTechnique.Passes[0].Apply();

                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);

                        // Outer cone
                        float cutoffScaleH = light.OuterRange;
                        float cutoffScaleW = light.OuterAngle * (float)(Math.PI / 180) / coneAngle * cutoffScaleH;

                        Matrix4x4 model2 = Matrix4x4.CreateScale(cutoffScaleW, cutoffScaleW, cutoffScaleH) * light.ObjectMatrix;
                        effect.Parameters["ModelViewProjection"].SetValue((model2 * _viewProjection).ToSharpDX());
                        effect.Parameters["Color"].SetValue(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

                        effect.CurrentTechnique.Passes[0].Apply();
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                    }
                    else if (light.Type == LightType.Sun)
                    {
                        _legacyDevice.SetVertexBuffer(_cone.VertexBuffer);
                        _legacyDevice.SetVertexInputLayout(_cone.InputLayout);
                        _legacyDevice.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);

                        Matrix4x4 model = Matrix4x4.CreateScale(0.01f, 0.01f, 1.0f) * light.ObjectMatrix;
                        effect.Parameters["ModelViewProjection"].SetValue((model * _viewProjection).ToSharpDX());
                        effect.Parameters["Color"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

                        effect.CurrentTechnique.Passes[0].Apply();
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                    }

                // Add text message
                textToDraw.Add(CreateTextTagForObject(
                    light.ObjectMatrix * _viewProjection,
                    light.Type.ToString().SplitCamelcase() + " Light" + "\n" + GetObjectPositionString(light.Room, light)));

                // Add the line height of the object
                AddObjectHeightLine(light.Room, light.Position);
            }

            _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);
        }

        private void DrawGhostBlocks(Effect effect, List<GhostBlockInstance> ghostBlocksToDraw, List<Text> textToDraw, List<Sprite> sprites)
        {
            if (ghostBlocksToDraw.Count == 0)
                return;

            var baseColor = _editor.Configuration.UI_ColorScheme.ColorFloor;
            var normalColor = new Vector4(baseColor.To3() * 0.4f, 0.9f);
            var selectColor = new Vector4(baseColor.To3() * 0.5f, 1.0f);

            int selectedIndex = -1;
            int lastIndex = -1;
            bool selectedCornerDrawn = false;

            _legacyDevice.SetVertexBuffer(_littleCube.VertexBuffer);
            _legacyDevice.SetVertexInputLayout(_littleCube.InputLayout);
            _legacyDevice.SetIndexBuffer(_littleCube.IndexBuffer, _littleCube.IsIndex32Bits);

            // Draw cubes (prioritize over sector!)
            for (int i = 0; i < ghostBlocksToDraw.Count; i++)
            {
                var instance = ghostBlocksToDraw[i];

                if (_editor.SelectedObject == instance)
                    selectedIndex = i;

                // Switch colours
                if (i == selectedIndex && selectedIndex >= 0)
                {
                    effect.Parameters["Color"].SetValue(selectColor);

                    // Add text message
                    textToDraw.Add(CreateTextTagForObject(
                        instance.CenterMatrix(instance.SelectedFloor) * _viewProjection,
                        instance.InfoMessage()));
                }
                else if (lastIndex == selectedIndex || lastIndex == -1)
                    effect.Parameters["Color"].SetValue(normalColor);
                lastIndex = i;

                if (selectedIndex == i)
                {
                    // Corner cubes
                    for (int f = 0; f < 2; f++)
                    {
                        bool floor = f == 0;
                        for (int j = 0; j < 4; j++)
                        {
                            var lastSelectedCorner = instance.SelectedCorner.HasValue && (int)instance.SelectedCorner.Value == j && instance.SelectedFloor == floor;
                            if (lastSelectedCorner == true || j == 4)
                                _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                            Matrix4x4 currCubeMatrix;
                            if (_movementTimer.Mode == AnimationMode.GhostBlockUnfold && !instance.SelectedCorner.HasValue)
                                currCubeMatrix = Matrix4x4.Lerp(instance.CenterMatrix(true), instance.ControlMatrixes(floor)[j], _movementTimer.MoveMultiplier);
                            else
                                currCubeMatrix = instance.ControlMatrixes(floor)[j];
                            currCubeMatrix *= _viewProjection;

                            effect.Parameters["ModelViewProjection"].SetValue(currCubeMatrix.ToSharpDX());
                            effect.Techniques[0].Passes[0].Apply();
                            _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _littleCube.IndexBuffer.ElementCount);

                            // Bring back solid state and lock it forever
                            if (lastSelectedCorner != selectedCornerDrawn)
                            {
                                _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullNone);
                                selectedCornerDrawn = true;
                            }
                        }
                    }
                }
                else // Default non-selected cube
                    DrawOrQueueServiceObject(instance, _littleCube, normalColor, effect, sprites);
            }
        }

        private void DrawGhostBlockBodies(Effect effect, List<GhostBlockInstance> ghostBlocksToDraw)
        {
            if (ghostBlocksToDraw.Count == 0)
                return;

            var baseColor = _editor.Configuration.UI_ColorScheme.ColorFloor;
            var normalColor = new Vector4(baseColor.To3() * 0.4f, 0.9f);
            var selectColor = new Vector4(baseColor.To3() * 0.5f, 1.0f);

            _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullNone);
            _legacyDevice.SetBlendState(_legacyDevice.BlendStates.NonPremultiplied);
            _legacyDevice.SetDepthStencilState(_legacyDevice.DepthStencilStates.DepthRead);

            _legacyDevice.SetVertexBuffer(_ghostBlockVertexBuffer);
            _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _ghostBlockVertexBuffer));
            effect.Parameters["Color"].SetValue(Vector4.One);

            foreach (var instance in ghostBlocksToDraw)
            {
                var selected = _editor.SelectedObject == instance;

                if (!instance.Valid)
                    continue;

                // Create a vertex array
                SolidVertex[] vtxs = new SolidVertex[84]; // 78 with diagonal steps

                // Derive base sector colours
                var p1c = new Vector4(baseColor.To3() * (selected ? 0.8f : 0.4f), selected ? 0.7f : 0.5f);
                var p2c = new Vector4(baseColor.To3() * (selected ? 0.5f : 0.2f), selected ? 0.7f : 0.5f);

                // Fill it up
                for (int f = 0, c = 0; f < 2; f++)
                {
                    bool floor = f == 0;

                    if (floor && !instance.ValidFloor || !floor && !instance.ValidCeiling)
                        continue;

                    var split = floor ? instance.Sector.Floor.DiagonalSplit : instance.Sector.Ceiling.DiagonalSplit;
                    bool toggled = floor ? instance.FloorSplitToggled : instance.CeilingSplitToggled;
                    var vPos = instance.ControlPositions(floor, false);
                    var vOrg = instance.ControlPositions(floor, true);

                    bool[] shift = new bool[4];
                    shift[0] = split == DiagonalSplit.XpZp || split == DiagonalSplit.XpZn;
                    shift[1] = split == DiagonalSplit.XpZp || split == DiagonalSplit.XnZp;
                    shift[2] = split == DiagonalSplit.XnZn || split == DiagonalSplit.XnZp;
                    shift[3] = split == DiagonalSplit.XnZn || split == DiagonalSplit.XpZn;

                    for (int i = 0; i < 4; i++)
                    {
                        Vector3[] fPos = new Vector3[4];

                        switch (i)
                        {
                            case 0: // Xn
                                fPos[0] = vOrg[0];
                                fPos[1] = vOrg[3];
                                fPos[2] = vPos[3];
                                fPos[3] = vPos[0];
                                if (shift[i])
                                    if (split == DiagonalSplit.XpZp)
                                    {
                                        fPos[0].Y = vOrg[3].Y;
                                        fPos[3].Y = (vOrg[3] + (vPos[0] - vOrg[0])).Y;
                                    }
                                    else
                                    {
                                        fPos[1].Y = vOrg[0].Y;
                                        fPos[2].Y = (vOrg[0] + (vPos[3] - vOrg[3])).Y;
                                    }
                                break;

                            case 1: // Zn
                                fPos[0] = vOrg[3];
                                fPos[1] = vOrg[2];
                                fPos[2] = vPos[2];
                                fPos[3] = vPos[3];
                                if (shift[i])
                                    if (split == DiagonalSplit.XnZp)
                                    {
                                        fPos[0].Y = vOrg[2].Y;
                                        fPos[3].Y = (vOrg[2] + (vPos[3] - vOrg[3])).Y;
                                    }
                                    else
                                    {
                                        fPos[1].Y = vOrg[3].Y;
                                        fPos[2].Y = (vOrg[3] + (vPos[2] - vOrg[2])).Y;
                                    }
                                break;

                            case 2: // Xp
                                fPos[0] = vOrg[2];
                                fPos[1] = vOrg[1];
                                fPos[2] = vPos[1];
                                fPos[3] = vPos[2];
                                if (shift[i])
                                    if (split == DiagonalSplit.XnZn)
                                    {
                                        fPos[0].Y = vOrg[1].Y;
                                        fPos[3].Y = (vOrg[1] + (vPos[2] - vOrg[2])).Y;
                                    }
                                    else
                                    {
                                        fPos[1].Y = vOrg[2].Y;
                                        fPos[2].Y = (vOrg[2] + (vPos[1] - vOrg[1])).Y;
                                    }
                                break;

                            case 3: // Zp
                                fPos[0] = vOrg[1];
                                fPos[1] = vOrg[0];
                                fPos[2] = vPos[0];
                                fPos[3] = vPos[1];
                                if (shift[i])
                                    if (split == DiagonalSplit.XpZn)
                                    {
                                        fPos[0].Y = vOrg[0].Y;
                                        fPos[3].Y = (vOrg[0] + (vPos[1] - vOrg[1])).Y;
                                    }
                                    else
                                    {
                                        fPos[1].Y = vOrg[1].Y;
                                        fPos[2].Y = (vOrg[1] + (vPos[0] - vOrg[0])).Y;
                                    }
                                break;
                        }

                        vtxs[c].Position = fPos[0]; vtxs[c].Color = p1c; c++;
                        vtxs[c].Position = fPos[1]; vtxs[c].Color = p1c; c++;
                        vtxs[c].Position = fPos[3]; vtxs[c].Color = p2c; c++;
                        vtxs[c].Position = fPos[1]; vtxs[c].Color = p1c; c++;
                        vtxs[c].Position = fPos[2]; vtxs[c].Color = p1c; c++;
                        vtxs[c].Position = fPos[3]; vtxs[c].Color = p2c; c++;
                    }

                    // Equality flags to further hide nonexistent triangle
                    bool[] equal = new bool[3];
                    int r = 0;

                    switch (split)
                    {
                        case DiagonalSplit.XpZn: r = 0; break;
                        case DiagonalSplit.XnZn: r = 1; break;
                        case DiagonalSplit.XnZp: r = 2; break;
                        case DiagonalSplit.XpZp: r = 3; break;
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        int ch = 0;
                        bool triShift = i == 0 && (split == DiagonalSplit.XpZn || split == DiagonalSplit.XnZn) ||
                                        i != 0 && (split == DiagonalSplit.XpZp || split == DiagonalSplit.XnZp);

                        ch = i == 0 ? toggled ? 3 : 0 : toggled ? 1 : 2;
                        equal[0] = vPos[ch] == vOrg[ch];
                        vtxs[c].Position = vPos[ch];
                        if (triShift) vtxs[c].Position.Y = vOrg[r].Y + (vPos[ch] - vOrg[ch]).Y;
                        vtxs[c].Color = i == 1 ? p1c : p2c;
                        c++;

                        ch = i == 0 ? toggled ? 0 : 1 : toggled ? 2 : 3;
                        equal[1] = vPos[ch] == vOrg[ch];
                        vtxs[c].Position = vPos[ch];
                        vtxs[c].Color = i == 1 ? p1c : p2c;
                        c++;

                        ch = i == 0 ? toggled ? 1 : 2 : toggled ? 3 : 0;
                        equal[2] = vPos[ch] == vOrg[ch]; vtxs[c].Position = vPos[ch];
                        if (triShift) vtxs[c].Position.Y = vOrg[r].Y + (vPos[ch] - vOrg[ch]).Y;
                        vtxs[c].Color = i == 1 ? p1c : p2c;

                        if (equal[0] && equal[1] && equal[2])
                            vtxs[c].Color = vtxs[c - 1].Color = vtxs[c - 2].Color = Vector4.Zero;
                        c++;
                    }

                    // Draw diagonals
                    bool flip = split == DiagonalSplit.XnZp || split == DiagonalSplit.XpZn;
                    bool draw = split != DiagonalSplit.None && !(floor ? instance.FloorIsQuad : instance.CeilingIsQuad);

                    vtxs[c].Position = flip ? vOrg[1] : vOrg[0]; vtxs[c].Color = draw ? p1c : Vector4.Zero; c++;
                    vtxs[c].Position = flip ? vOrg[3] : vOrg[2]; vtxs[c].Color = draw ? p2c : Vector4.Zero; c++;
                    vtxs[c].Position = flip ? vPos[3] : vPos[2]; vtxs[c].Color = draw ? p1c : Vector4.Zero; c++;
                    vtxs[c].Position = flip ? vPos[3] : vPos[2]; vtxs[c].Color = draw ? p1c : Vector4.Zero; c++;
                    vtxs[c].Position = flip ? vPos[1] : vPos[0]; vtxs[c].Color = draw ? p2c : Vector4.Zero; c++;
                    vtxs[c].Position = flip ? vOrg[1] : vOrg[0]; vtxs[c].Color = draw ? p1c : Vector4.Zero; c++;

                }

                _ghostBlockVertexBuffer.SetData(vtxs);

                effect.Parameters["ModelViewProjection"].SetValue(_viewProjection.ToSharpDX());
                effect.CurrentTechnique.Passes[0].Apply();
                _legacyDevice.Draw(PrimitiveType.TriangleList, 84);
            }
        }

        private void DrawVolumes(Effect effect, List<VolumeInstance> volumesToDraw, List<Text> textToDraw, List<Sprite> sprites)
        {
            if (volumesToDraw.Count == 0)
                return;

            var drawVolume = _editor.Level.IsTombEngine;
            var baseColor = _editor.Configuration.UI_ColorScheme.ColorTrigger;
            var normalColor = new Vector4(baseColor.To3() * 0.6f, 0.55f);
            var selectColor = new Vector4(baseColor.To3(), 0.7f);
            var disabledNormalColor = new Vector4(new Vector3(normalColor.To3().GetLuma()), 0.55f);
            var disabledSelectColor = new Vector4(new Vector3(selectColor.To3().GetLuma()), 0.55f);

            var currentShape = VolumeShape.Box;
            int selectedIndex = -1;
            int lastIndex = -1;
            int elementCount = _littleCube.IndexBuffer.ElementCount;

            _legacyDevice.SetBlendState(_legacyDevice.BlendStates.NonPremultiplied);
            _legacyDevice.SetDepthStencilState(_legacyDevice.DepthStencilStates.DepthRead);
            _legacyDevice.SetVertexBuffer(_littleCube.VertexBuffer);
            _legacyDevice.SetVertexInputLayout(_littleCube.InputLayout);
            _legacyDevice.SetIndexBuffer(_littleCube.IndexBuffer, _littleCube.IsIndex32Bits);

            Vector4 color = normalColor;

            // Draw center cubes
            for (int i = 0; i < volumesToDraw.Count; i++)
            {
                var instance = volumesToDraw[i];
                if (_editor.SelectedObject == instance)
                    selectedIndex = i;

                color = instance.Enabled ? normalColor : disabledNormalColor;

                // Switch colours
                if (i == selectedIndex && selectedIndex >= 0)
                {
                    color = instance.Enabled ? selectColor : disabledSelectColor;
                    _legacyDevice.SetRasterizerState(_rasterizerWireframe); // As wireframe if selected

                    // Add text message
                    textToDraw.Add(CreateTextTagForObject(
                        instance.RotationPositionMatrix * _viewProjection,
                        instance.ToString()));
                }
                else if (lastIndex == selectedIndex || lastIndex == -1)
                {
                    _legacyDevice.SetRasterizerState(_rasterizerStateDepthBias);
                }
                lastIndex = i;

                DrawOrQueueServiceObject(instance, _littleCube, color, effect, sprites);
            }

            // Reset last index back to default
            lastIndex = -1;

            // Draw 3D volumes (only for TombEngine version, otherwise we show only disabled center cube)
            if (drawVolume)
            {
                _legacyDevice.SetRasterizerState(_rasterizerStateDepthBias);

                for (int i = 0; i < volumesToDraw.Count; i++)
                {
                    Matrix4x4 model;
                    var instance = volumesToDraw[i];
                    var shape = instance.Shape();

                    // Switch colours
                    if (_highlightedObjects.Contains(instance))
                        color = instance.Enabled ? selectColor : disabledSelectColor;
                    else
                        color = instance.Enabled ? normalColor : disabledNormalColor;

                    // Switch vertex buffers (only do it if shape is changed)
                    if (shape != currentShape)
                    {
                        elementCount = shape == VolumeShape.Box ? _littleCube.IndexBuffer.ElementCount : _sphere.IndexBuffer.ElementCount;
                        currentShape = shape;

                        switch (currentShape)
                        {
                            default:
                            case VolumeShape.Box:
                                // Do nothing, we're using same cube shape from above
                                break;
                            case VolumeShape.Sphere:
                                _legacyDevice.SetVertexBuffer(_sphere.VertexBuffer);
                                _legacyDevice.SetVertexInputLayout(_sphere.InputLayout);
                                _legacyDevice.SetIndexBuffer(_sphere.IndexBuffer, _sphere.IsIndex32Bits);
                                break;
                        }
                    }

                    switch (shape)
                    {
                        default:
                        case VolumeShape.Box:
                            {
                                var bv = instance as BoxVolumeInstance;
                                model = Matrix4x4.CreateScale(bv.Size / _littleCubeRadius / 2.0f) *
                                        instance.RotationPositionMatrix;
                            }
                            break;
                        case VolumeShape.Sphere:
                            {
                                var sv = instance as SphereVolumeInstance;
                                model = Matrix4x4.CreateScale(sv.Size / (_littleSphereRadius * 8.0f)) *
                                        instance.RotationPositionMatrix;
                            }
                            break;
                    }


                    for (int d = 0; d < 2; d++)
                    {
                        if (d == 1)
                        {
                            if (shape == VolumeShape.Box)
                            {
                                _legacyDevice.SetVertexBuffer(_boxVertexBuffer);
                                _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _boxVertexBuffer));
                            }

                            _legacyDevice.SetRasterizerState(_rasterizerWireframe);
                            effect.Parameters["Color"].SetValue(new Vector4(color.To3() * 0.5f, 0.5f));
                        }
                        else
                        {
                            if (shape == VolumeShape.Box)
                            {
                                _legacyDevice.SetVertexBuffer(_littleCube.VertexBuffer);
                                _legacyDevice.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _littleCube.VertexBuffer));
                            }

                            _legacyDevice.SetRasterizerState(_rasterizerStateDepthBias);
                            effect.Parameters["Color"].SetValue(color);
                        }

                        effect.Parameters["ModelViewProjection"].SetValue((model * _viewProjection).ToSharpDX());
                        effect.CurrentTechnique.Passes[0].Apply();

                        if (shape == VolumeShape.Box && d == 1)
                            _legacyDevice.Draw(PrimitiveType.LineList, _boxVertexBuffer.ElementCount);
                        else
                            _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, elementCount);
                    }
                }
            }
        }

        private void DrawSprites(Room[] roomsWhoseObjectsToDraw, List<Sprite> sprites, bool disableSelection)
        {
            if (_editor.Level.Settings.GameVersion.Native() > TRVersion.Game.TR2)
                return;

            var sequences = _editor.Level.Settings.WadGetAllSpriteSequences();

            foreach (Room room in roomsWhoseObjectsToDraw)
                foreach (var instance in room.Objects.OfType<SpriteInstance>())
                {
                    var sequence = sequences.FirstOrDefault(s => s.Key.TypeId == instance.Sequence).Value;
                    if (sequence != null && sequence.Sprites.Count > instance.Frame)
                    {
                        float depth;
                        var sprite = sequence.Sprites[instance.Frame];
                        var pos = instance.GetViewportRect(sprite.Alignment, Camera.GetPosition(), _viewProjection, ClientSize, out depth);

                        if (depth < 1.0f) // Discard offscreen sprites
                        {
                            var selected = _highlightedObjects.Contains(instance);
                            var newSprite = new Sprite
                            {
                                Texture = sprite.Texture.Image,
                                PosStart = pos.Start,
                                PosEnd = pos.End,
                                Depth = depth
                            };

                            if (!disableSelection && selected)
                                newSprite.Tint = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            else if (_editor.Mode == EditorMode.Lighting)
                                newSprite.Tint = new Vector4(new Vector3(instance.Color.GetLuma()), 1.0f);
                            else
                                newSprite.Tint = Vector4.One;

                            sprites.Add(newSprite);
                        }
                    }
                }
        }

        private void DrawPlaceholders(Effect effect, Room[] roomsWhoseObjectsToDraw, List<Text> textToDraw, List<Sprite> sprites)
        {
            _legacyDevice.SetVertexBuffer(_littleCube.VertexBuffer);
            _legacyDevice.SetVertexInputLayout(_littleCube.InputLayout);
            _legacyDevice.SetIndexBuffer(_littleCube.IndexBuffer, _littleCube.IsIndex32Bits);
            _legacyDevice.SetDepthStencilState(_legacyDevice.DepthStencilStates.Default);
            _legacyDevice.SetBlendState(_legacyDevice.BlendStates.Opaque);

            var groups = roomsWhoseObjectsToDraw.SelectMany(r => r.Objects).GroupBy(o => o.GetType());
            foreach (var group in groups)
            {
                if (group.Key == typeof(SpriteInstance))
                    foreach (SpriteInstance instance in group)
                    {
                        if (_editor.SelectedObject == instance)
                        {
                            // Add text message
                            textToDraw.Add(CreateTextTagForObject(
                                instance.WorldPositionMatrix * _viewProjection,
                                instance.ShortName() +
                                "\n" + GetObjectPositionString(instance.Room, instance)));

                            // Add the line height of the object
                            AddObjectHeightLine(instance.Room, instance.Position);
                        }

                        if (_editor.Level.Settings.GameVersion.Native() > TRVersion.Game.TR2 || !instance.SpriteIsValid)
                        {
                            Vector4 color;
                            if (_editor.SelectedObject == instance)
                            {
                                color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                                _legacyDevice.SetRasterizerState(_rasterizerWireframe);
                            }
                            else
                            {
                                color = new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
                                _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);
                            }

                            DrawOrQueueServiceObject(instance, _littleCube, color, effect, sprites);
                        }
                    }

                if (group.Key == typeof(CameraInstance))
                    foreach (CameraInstance instance in group)
                    {
                        _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

                        var color = new Vector4(0.4f, 0.9f, 0.0f, 1.0f);
                        if (_highlightedObjects.Contains(instance))
                        {
                            color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                            if (_editor.SelectedObject == instance)
                            {
                                // Add text message
                                textToDraw.Add(CreateTextTagForObject(
                                    instance.RotationPositionMatrix * _viewProjection,
                                    "Camera " + (instance.CameraMode == CameraInstanceMode.Locked ? "(Locked)" : instance.CameraMode == CameraInstanceMode.Sniper ? "(Sniper)" : "") +
                                    instance.GetScriptIDOrName() + "\n" +
                                    GetObjectPositionString(instance.Room, instance) + GetObjectTriggerString(instance)));

                                // Add the line height of the object
                                AddObjectHeightLine(instance.Room, instance.Position);
                            }
                        }

                        DrawOrQueueServiceObject(instance, _littleCube, color, effect, sprites);
                    }

                if (group.Key == typeof(FlybyCameraInstance))
                    foreach (FlybyCameraInstance instance in group)
                    {
                        _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

                        var color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

                        if (_editor.SelectedObject is FlybyCameraInstance && (_editor.SelectedObject as FlybyCameraInstance).Sequence == instance.Sequence)
                            color = MathC.GetRandomColorByIndex(instance.Sequence, 32, 0.7f);

                        if (_highlightedObjects.Contains(instance))
                        {
                            color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                            if (_editor.SelectedObject == instance)
                            {
                                // Add text message
                                textToDraw.Add(CreateTextTagForObject(
                                    instance.RotationPositionMatrix * _viewProjection,
                                    "Flyby cam (" + instance.Sequence + ":" + instance.Number + ") " +
                                    instance.GetScriptIDOrName() + "\n" +
                                    GetObjectPositionString(instance.Room, instance) + GetObjectTriggerString(instance)));

                                // Add the line height of the object
                                AddObjectHeightLine(instance.Room, instance.Position);
                            }
                        }

                        DrawOrQueueServiceObject(instance, _littleCube, color, effect, sprites);
                    }

                if (group.Key == typeof(MemoInstance))
                    foreach (MemoInstance instance in group)
                    {
                        _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

                        Vector4 color = Vector4.One;
                        if (_highlightedObjects.Contains(instance))
                        {
                            color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            _legacyDevice.SetRasterizerState(_rasterizerWireframe);
                        }

                        // Add text message
                        if (_editor.SelectedObject == instance || instance.AlwaysDisplay)
                            textToDraw.Add(CreateTextTagForObject(instance.RotationPositionMatrix * _viewProjection, instance.Text));

                        DrawOrQueueServiceObject(instance, _littleCube, color, effect, sprites);
                    }

                if (group.Key == typeof(SinkInstance))
                    foreach (SinkInstance instance in group)
                    {
                        _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

                        Vector4 color = new Vector4(0.0f, 0.6f, 1.0f, 1.0f);
                        if (_highlightedObjects.Contains(instance))
                        {
                            color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                            // Add text message
                            if (_editor.SelectedObject == instance)
                            {
                                textToDraw.Add(CreateTextTagForObject(
                                    instance.RotationPositionMatrix * _viewProjection,
                                    instance.ToShortString() + "\n" +
                                    GetObjectPositionString(instance.Room, instance) + GetObjectTriggerString(instance)));

                                // Add the line height of the object
                                AddObjectHeightLine(instance.Room, instance.Position);
                            }
                        }

                        DrawOrQueueServiceObject(instance, _littleCube, color, effect, sprites);
                    }

                if (group.Key == typeof(SoundSourceInstance))
                    foreach (SoundSourceInstance instance in group)
                    {
                        _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

                        Vector4 color = new Vector4(1.0f, 0.7f, 0.0f, 1.0f);
                        if (_highlightedObjects.Contains(instance))
                        {
                            color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                            if (_editor.SelectedObject == instance)
                            {
                                // Add text message
                                textToDraw.Add(CreateTextTagForObject(
                                    instance.RotationPositionMatrix * _viewProjection,
                                    "Sound source ID " + (instance.SoundId != -1 ? instance.SoundId + ": " + instance.SoundNameToDisplay : "No sound assigned yet") +
                                    instance.GetScriptIDOrName() + "\n" +
                                    GetObjectPositionString(instance.Room, instance)));

                                // Add the line height of the object
                                AddObjectHeightLine(instance.Room, instance.Position);
                            }
                        }

                        DrawOrQueueServiceObject(instance, _littleCube, color, effect, sprites);
                    }

                if (ShowMoveables && group.Key == typeof(MoveableInstance))
                    foreach (MoveableInstance instance in group)
                    {
                        if (_editor?.Level?.Settings?.WadTryGetMoveable(instance.WadObjectId) != null)
                            continue;

                        _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

                        Vector4 color = new Vector4(0.4f, 0.4f, 1.0f, 1.0f);
                        if (_highlightedObjects.Contains(instance))
                        {
                            color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                            if (_editor.SelectedObject == instance)
                            {
                                // Add text message
                                textToDraw.Add(CreateTextTagForObject(
                                    instance.RotationPositionMatrix * _viewProjection,
                                    instance.ShortName() + "\nUnavailable " + instance.ItemType +
                                    instance.GetScriptIDOrName() + "\n" +
                                    GetObjectPositionString(instance.Room, instance) + GetObjectTriggerString(instance)));

                                // Add the line height of the object
                                AddObjectHeightLine(instance.Room, instance.Position);
                            }
                        }

                        DrawOrQueueServiceObject(instance, _littleCube, color, effect, sprites);
                    }

                if (ShowStatics && group.Key == typeof(StaticInstance))
                    foreach (StaticInstance instance in group)
                    {
                        if (_editor?.Level?.Settings?.WadTryGetStatic(instance.WadObjectId) != null)
                            continue;

                        _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

                        Vector4 color = new Vector4(0.4f, 0.4f, 1.0f, 1.0f);
                        if (_highlightedObjects.Contains(instance))
                        {
                            color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                            _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                            if (_editor.SelectedObject == instance)
                            {
                                // Add text message
                                textToDraw.Add(CreateTextTagForObject(
                                    instance.RotationPositionMatrix * _viewProjection,
                                    instance.ShortName() + "\nUnavailable " + instance.ItemType + GetObjectTriggerString(instance)));

                                // Add the line height of the object
                                AddObjectHeightLine(instance.Room, instance.Position);
                            }
                        }

                        DrawOrQueueServiceObject(instance, _littleCube, color, effect, sprites);
                    }

                if (ShowImportedGeometry && group.Key == typeof(ImportedGeometryInstance))
                    foreach (ImportedGeometryInstance instance in group)
                    {
                        if (instance.Model?.DirectXModel == null || instance.Model?.DirectXModel.Meshes.Count == 0 || instance.Hidden)
                        {
                            Vector4 color = new Vector4(0.5f, 0.3f, 1.0f, 1.0f);
                            if (_highlightedObjects.Contains(instance))
                            {
                                color = _editor.Configuration.UI_ColorScheme.ColorSelection;
                                _legacyDevice.SetRasterizerState(_rasterizerWireframe);

                                if (_editor.SelectedObject == instance)
                                {
                                    // Add text message
                                    textToDraw.Add(CreateTextTagForObject(
                                        instance.RotationPositionMatrix * _viewProjection,
                                        instance.ToString()));

                                    // Add the line height of the object
                                    AddObjectHeightLine(instance.Room, instance.Position);
                                }
                            }

                            DrawOrQueueServiceObject(instance, _littleCube, color, effect, sprites);
                        }
                    }
            }

            // Draw extra flyby cones

            _legacyDevice.SetVertexBuffer(_cone.VertexBuffer);
            _legacyDevice.SetVertexInputLayout(_cone.InputLayout);
            _legacyDevice.SetIndexBuffer(_cone.IndexBuffer, _cone.IsIndex32Bits);
            _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullNone);

            bool wireframe = false;
            foreach (Room room in roomsWhoseObjectsToDraw)
                foreach (var instance in room.Objects.OfType<FlybyCameraInstance>())
                {
                    var color = MathC.GetRandomColorByIndex(instance.Sequence, 32, 0.7f);
                    Matrix4x4 model;

                    if (_highlightedObjects.Contains(instance))
                        color = _editor.Configuration.UI_ColorScheme.ColorSelection;

                    for (int pass = 0; pass < 2; pass++)
                    {
                        if (_editor.SelectedObject == instance)
                        {
                            float coneAngle = (float)Math.Atan2(512, 1024);
                            float cutoffScaleH = 1;
                            float cutoffScaleW = instance.Fov * (float)(Math.PI / 360) / coneAngle * cutoffScaleH;

                            if (pass == 0)
                            {
                                // Ordinary cone
                                model = Matrix4x4.CreateScale(cutoffScaleW, cutoffScaleW, cutoffScaleH) * instance.ObjectMatrix;
                            }
                            else
                            {
                                // Roll pointer
                                var step = 1 / _coneRadius;
                                var scale = _littleCubeRadius * 2;
                                var pScale = _littleCubeRadius / 5;
                                var vOffset = -cutoffScaleW / 2 * _coneRadius - scale;
                                var hOffset = cutoffScaleH * _coneRadius;

                                model = Matrix4x4.CreateScale(step * pScale, step * pScale, step * scale) *
                                        Matrix4x4.CreateTranslation(new Vector3(0, hOffset, vOffset)) *
                                        Matrix4x4.CreateRotationX((float)(Math.PI / 2)) *
                                        instance.ObjectMatrix;
                            }

                            if (wireframe == false)
                            {
                                _legacyDevice.SetRasterizerState(_rasterizerWireframe);
                                wireframe = true;
                            }
                        }
                        else
                        {
                            // Don't do second pass for non-selected flybys
                            if (pass == 1)
                                break;

                            // Push unselected cone further away in sprite mode for neatness
                            if (_editor.Configuration.Rendering3D_UseSpritesForServiceObjects)
                                model = Matrix4x4.CreateTranslation(new Vector3(0, 0, -_coneRadius * 0.5f));
                            else
                                model = Matrix4x4.Identity;

                            model *= Matrix4x4.CreateTranslation(new Vector3(0, 0, -_coneRadius * 1.2f)) *
                                     Matrix4x4.CreateRotationY((float)Math.PI) *
                                     Matrix4x4.CreateScale(1 / _coneRadius * _littleCubeRadius * 2.0f) *
                                     instance.ObjectMatrix;

                            if (wireframe == true)
                            {
                                _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullNone);
                                wireframe = false;
                            }
                        }

                        effect.Parameters["ModelViewProjection"].SetValue((model * _viewProjection).ToSharpDX());
                        effect.Parameters["Color"].SetValue(color);
                        effect.CurrentTechnique.Passes[0].Apply();
                        _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                    }
                }
        }

        private void DrawOrQueueServiceObject(ISpatial instance, GeometricPrimitive primitive, Vector4 color, Effect effect, List<Sprite> sprites)
        {
            if (_editor.Configuration.Rendering3D_UseSpritesForServiceObjects)
            {
                foreach (bool shadow in new[] { true, false })
                {
                    if (shadow)
                    {
                        if (_editor.Level.Settings.GameVersion != TRVersion.Game.TombEngine)
                            continue;

                        if (!(instance is LightInstance) || !(instance as LightInstance).CanCastDynamicShadows)
                            continue;
                    }

                    var newSprite = ServiceObjectTextures.GetSprite(instance,
                                                                    Camera.GetPosition(),
                                                                    _viewProjection,
                                                                    ClientSize,
                                                                    shadow ? new Vector4(Vector3.Zero, 1.0f) : color,
                                                                    shadow ? new Vector2(8.0f, -8.0f) : Vector2.Zero,
                                                                    _highlightedObjects.Contains((ObjectInstance)instance));
                    if (newSprite == null)
                        return;

                    sprites.Add(newSprite);
                }

                return;
            }

            if (instance is PositionBasedObjectInstance)
                effect.Parameters["ModelViewProjection"].SetValue(((instance as PositionBasedObjectInstance).RotationPositionMatrix * _viewProjection).ToSharpDX());
            else if (instance is GhostBlockInstance)
                effect.Parameters["ModelViewProjection"].SetValue(((instance as GhostBlockInstance).CenterMatrix(true) * _viewProjection).ToSharpDX());

            effect.Parameters["Color"].SetValue(color);
            effect.Techniques[0].Passes[0].Apply();
            _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, primitive.IndexBuffer.ElementCount);
        }

        private void DrawCardinalDirections(List<Text> textToDraw)
        {
            string[] messages;
            if (_editor.Configuration.Rendering3D_UseRoomEditorDirections)
                messages = new string[] { "+Z (East)", "-Z (West)", "+X (South)", "-X (North)" };
            else
                messages = new string[] { "+Z (North)", "-Z (South)", "+X (East)", "-X (West)" };

            Vector3[] positions = new Vector3[4]
                {
                        new Vector3(0, 0, _editor.SelectedRoom.NumZSectors *  Level.HalfSectorSizeUnit),
                        new Vector3(0, 0, _editor.SelectedRoom.NumZSectors * -Level.HalfSectorSizeUnit),
                        new Vector3(_editor.SelectedRoom.NumXSectors *  Level.HalfSectorSizeUnit, 0, 0),
                        new Vector3(_editor.SelectedRoom.NumXSectors * -Level.HalfSectorSizeUnit, 0, 0)
                 };

            var center = _editor.SelectedRoom.GetLocalCenter();
            var matrix = Matrix4x4.CreateTranslation(_editor.SelectedRoom.WorldPos) * _viewProjection;
            for (int i = 0; i < 4; i++)
            {
                var pos = matrix.TransformPerspectively(center + positions[i]);
                if (pos.Z <= 1.0f)
                    textToDraw.Add(new Text
                    {
                        Font = _fontDefault,
                        Pos = pos.To2(),
                        Overlay = _editor.Configuration.Rendering3D_DrawFontOverlays,
                        String = messages[i]
                    });
            }
        }

        private void DrawSkybox()
        {
            _legacyDevice.SetBlendState(_legacyDevice.BlendStates.Opaque);

            Effect skinnedModelEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];

            skinnedModelEffect.Parameters["TextureSampler"].SetResource(BilinearFilter ? _legacyDevice.SamplerStates.AnisotropicWrap : _legacyDevice.SamplerStates.PointWrap);
            // Get Horizon Id and try to retrieve moveable for skybox rendering
            var version = _editor.Level.Settings.GameVersion;
            WadMoveableId? horizonId = WadMoveableId.GetHorizon(version);
            WadMoveable moveable = null;
            if (horizonId.HasValue)
                moveable = _editor?.Level?.Settings?.WadTryGetMoveable(horizonId.Value);

            if (moveable == null)
                return;

            AnimatedModel model = _wadRenderer.GetMoveable(moveable);

            skinnedModelEffect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
            skinnedModelEffect.Parameters["Color"].SetValue(Vector4.One);
            skinnedModelEffect.Parameters["StaticLighting"].SetValue(false);
            skinnedModelEffect.Parameters["ColoredVertices"].SetValue(false);

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];
                if (mesh.Vertices.Count == 0 || mesh.VertexBuffer == null || mesh.InputLayout == null || mesh.IndexBuffer == null)
                    continue;

                _legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                _legacyDevice.SetVertexInputLayout(mesh.InputLayout);
                _legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);

                Matrix4x4 world = Matrix4x4.CreateScale(128.0f) *
                                  model.AnimationTransforms[i] *
                                  Matrix4x4.CreateTranslation(Camera.GetPosition());

                skinnedModelEffect.Parameters["ModelViewProjection"].SetValue((world * _viewProjection).ToSharpDX());
                skinnedModelEffect.Techniques[0].Passes[0].Apply();

                foreach (var submesh in mesh.Submeshes)
                    _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
            }

            SwapChain.ClearDepth();
        }

        private void DrawMoveables(List<MoveableInstance> moveablesToDraw, List<Text> textToDraw, bool disableSelection = false)
        {
            if (moveablesToDraw.Count == 0)
                return;

            var camPos = Camera.GetPosition();
            var skinnedModelEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];

            skinnedModelEffect.Parameters["AlphaTest"].SetValue(HideTransparentFaces);
            skinnedModelEffect.Parameters["ColoredVertices"].SetValue(_editor.Level.IsTombEngine);
            skinnedModelEffect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
            skinnedModelEffect.Parameters["TextureSampler"].SetResource(BilinearFilter ? _legacyDevice.SamplerStates.AnisotropicWrap : _legacyDevice.SamplerStates.PointWrap);

            var groups = moveablesToDraw.GroupBy(m => m.WadObjectId);
            foreach (var group in groups)
            {
                var movID = _editor?.Level?.Settings?.WadTryGetMoveable(group.Key);
                if (movID == null)
                    continue;

                var model = _wadRenderer.GetMoveable(movID);
                var skin = model;
                var version = _editor.Level.Settings.GameVersion;
                var colored = version.Native() <= TRVersion.Game.TR2 && group.First().CanBeColored();

                if (group.Key == WadMoveableId.Lara) // Show Lara
                {
                    var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(version, group.Key.TypeId));
                    var moveableSkin = _editor.Level.Settings.WadTryGetMoveable(skinId);
                    if (moveableSkin != null && moveableSkin.Meshes.Count == model.Meshes.Count)
                    {
                        movID = moveableSkin;
                        skin = _wadRenderer.GetMoveable(moveableSkin);
                    }
                }

                foreach (var instance in group)
                {
                    // Add text message
                    if (_editor.SelectedObject == instance)
                    {
                        textToDraw.Add(CreateTextTagForObject(
                            instance.RotationPositionMatrix * _viewProjection,
                            instance.ItemType.MoveableId.ShortName(_editor.Level.Settings.GameVersion) +
                            instance.GetScriptIDOrName() + "\n" +
                            GetObjectPositionString(instance.Room, instance) + "\n" +
                            GetObjectRotationString(instance.Room, instance) +
                            (instance.Ocb == 0 ? string.Empty : "\nOCB: " + instance.Ocb) +
                            GetObjectTriggerString(instance)));

                        // Add the line height of the object
                        AddObjectHeightLine(instance.Room, instance.Position);
                    }

                    if (!_editor.Level.IsTombEngine || skin.Skin == null)
                        continue;

                    if (!disableSelection && _highlightedObjects.Contains(instance)) // Selection
                        skinnedModelEffect.Parameters["Color"].SetValue(_editor.Configuration.UI_ColorScheme.ColorSelection);
                    else
                    {
                        if (ShowRealTintForObjects && _editor.Mode == EditorMode.Lighting)
                        {
                            skinnedModelEffect.Parameters["StaticLighting"].SetValue(true);
                            skinnedModelEffect.Parameters["Color"].SetValue(ConvertColor(instance.Room.Properties.AmbientLight * instance.Color));
                        }
                        else
                        {
                            skinnedModelEffect.Parameters["StaticLighting"].SetValue(false);
                            skinnedModelEffect.Parameters["Color"].SetValue(Vector4.One);
                        }
                    }

                    skin.RenderSkin(_legacyDevice, skinnedModelEffect, (instance.ObjectMatrix * _viewProjection).ToSharpDX(), model);
                }

                for (int i = 0; i < skin.Meshes.Count; i++)
                {
                    var mesh = skin.Meshes[i];
                    if (mesh.Vertices.Count == 0 || mesh.VertexBuffer == null || mesh.InputLayout == null || mesh.IndexBuffer == null)
                        continue;

                    if (_editor.Level.IsTombEngine && skin.Skin != null && mesh.Hidden)
                        continue;

                    _legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                    _legacyDevice.SetVertexInputLayout(mesh.InputLayout);
                    _legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);

                    foreach (var instance in group)
                    {
                        if (!disableSelection && _highlightedObjects.Contains(instance)) // Selection
                            skinnedModelEffect.Parameters["Color"].SetValue(_editor.Configuration.UI_ColorScheme.ColorSelection);
                        else
                        {
                            if (ShowRealTintForObjects && _editor.Mode == EditorMode.Lighting)
                            {
                                if (colored || movID.Meshes[i].LightingType != WadMeshLightingType.Normals)
                                {
                                    skinnedModelEffect.Parameters["StaticLighting"].SetValue(true);
                                    skinnedModelEffect.Parameters["Color"].SetValue(ConvertColor(instance.Color));
                                }
                                else
                                {
                                    var color = _editor.Level.IsTombEngine ? instance.Room.Properties.AmbientLight * instance.Color : instance.Room.Properties.AmbientLight;
                                    skinnedModelEffect.Parameters["StaticLighting"].SetValue(_editor.Level.IsTombEngine ? true : false);
                                    skinnedModelEffect.Parameters["Color"].SetValue(ConvertColor(color));
                                }
                            }
                            else
                            {
                                skinnedModelEffect.Parameters["StaticLighting"].SetValue(false);
                                skinnedModelEffect.Parameters["Color"].SetValue(Vector4.One);
                            }
                        }

                        var world = model.AnimationTransforms[i] * instance.ObjectMatrix;
                        skinnedModelEffect.Parameters["ModelViewProjection"].SetValue((world * _viewProjection).ToSharpDX());
                        skinnedModelEffect.Techniques[0].Passes[0].Apply();

                        foreach (var submesh in mesh.Submeshes)
                        {
                            if (submesh.Value.NumIndices == 0)
                                continue;

                            submesh.Key.SetStates(_legacyDevice, _editor.Configuration.Rendering3D_HideTransparentFaces && _editor.SelectedObject != instance);
                            _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                        }
                    }
                }
            }
        }

        private void DrawImportedGeometry(List<ImportedGeometryInstance> importedGeometryToDraw, List<Text> textToDraw, bool disableSelection = false)
        {
            if (importedGeometryToDraw.Count == 0)
                return;

            var geometryEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["RoomGeometry"];
            geometryEffect.Parameters["AlphaTest"].SetValue(HideTransparentFaces);
            geometryEffect.Parameters["TextureSampler"].SetResource(BilinearFilter ? _legacyDevice.SamplerStates.AnisotropicWrap : _legacyDevice.SamplerStates.PointWrap);

            // Before drawing custom geometry, apply a depth bias for reducing Z fighting
            _legacyDevice.SetRasterizerState(_rasterizerStateDepthBias);

            var camPos = Camera.GetPosition();

            var groups = importedGeometryToDraw.GroupBy(g => g.Model.UniqueID);
            foreach (var group in groups)
            {
                var model = group.First().Model.DirectXModel;
                if (model == null || model.Meshes == null || model.Meshes.Count == 0)
                    continue;

                var meshes = model.Meshes;
                for (var i = 0; i < meshes.Count; i++)
                {
                    var mesh = meshes[i];
                    if (mesh.Vertices.Count == 0 || mesh.InputLayout == null || mesh.IndexBuffer == null || mesh.VertexBuffer == null)
                        continue;

                    _legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                    _legacyDevice.SetVertexInputLayout(mesh.InputLayout);
                    _legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);

                    foreach (var instance in group)
                    {
                        if (instance.Hidden)
                            continue;

                        geometryEffect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * _viewProjection).ToSharpDX());

                        // Tint unselected geometry in blue if it's not pickable, otherwise use normal or selection color
                        if (!disableSelection && _highlightedObjects.Contains(instance))
                        {
                            geometryEffect.Parameters["UseVertexColors"].SetValue(false);
                            geometryEffect.Parameters["Color"].SetValue(_editor.Configuration.UI_ColorScheme.ColorSelection);
                        }
                        else if (DisablePickingForImportedGeometry)
                        {
                            geometryEffect.Parameters["UseVertexColors"].SetValue(false);
                            geometryEffect.Parameters["Color"].SetValue(new Vector4(0.4f, 0.4f, 1.0f, 1.0f));
                        }
                        else
                        {
                            var useVertexColors = _editor.Mode == EditorMode.Lighting && ShowRealTintForObjects && instance.LightingModel == ImportedGeometryLightingModel.VertexColors;
                            geometryEffect.Parameters["UseVertexColors"].SetValue(useVertexColors);

                            if (ShowRealTintForObjects && _editor.Mode == EditorMode.Lighting)
                            {
                                switch (instance.LightingModel)
                                {
                                    case ImportedGeometryLightingModel.NoLighting:
                                    case ImportedGeometryLightingModel.CalculateFromLightsInRoom:
                                        geometryEffect.Parameters["Color"].SetValue(ConvertColor(instance.Color * instance.Room.Properties.AmbientLight));
                                        break;

                                    case ImportedGeometryLightingModel.VertexColors:
                                    case ImportedGeometryLightingModel.TintAsAmbient:
                                        geometryEffect.Parameters["Color"].SetValue(ConvertColor(instance.Color));
                                        break;
                                }
                            }
                            else
                                geometryEffect.Parameters["Color"].SetValue(Vector4.One);
                        }

                        foreach (var submesh in mesh.Submeshes)
                        {
                            if (submesh.Value.NumIndices == 0)
                                continue;

                            var texture = submesh.Value.Material.Texture;
                            if (texture != null && texture is ImportedGeometryTexture)
                            {
                                geometryEffect.Parameters["TextureEnabled"].SetValue(true);
                                geometryEffect.Parameters["Texture"].SetResource(((ImportedGeometryTexture)texture).DirectXTexture);
                                geometryEffect.Parameters["ReciprocalTextureSize"].SetValue(new Vector2(1.0f / texture.Image.Width, 1.0f / texture.Image.Height));
                            }
                            else
                                geometryEffect.Parameters["TextureEnabled"].SetValue(false);

                            geometryEffect.Techniques[0].Passes[0].Apply();

                            submesh.Key.SetStates(_legacyDevice, _editor.Configuration.Rendering3D_HideTransparentFaces && _editor.SelectedObject != instance);

                            // If picking for imported geometry is disabled, then draw geometry translucent
                            if (DisablePickingForImportedGeometry)
                                _legacyDevice.SetBlendState(_legacyDevice.BlendStates.Additive);

                            _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                        }

                        // Add text message
                        if (i == 0 && _editor.SelectedObject == instance)
                        {
                            textToDraw.Add(CreateTextTagForObject(
                                instance.RotationPositionMatrix * _viewProjection,
                                instance + "\n" + GetObjectPositionString(_editor.SelectedRoom, instance) + "\n" +
                                GetObjectRotationString(_editor.SelectedRoom, instance) + "\n" +
								"Scale: " + instance.Scale + "\n" +
								"Triangles: " + instance.Model.DirectXModel.TotalTriangles));

                            // Add the line height of the object
                            AddObjectHeightLine(_editor.SelectedRoom, instance.Position);
                        }
                    }
                }
            }

            // Reset GPU states
            _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

            if (DisablePickingForImportedGeometry)
                _legacyDevice.SetBlendState(_legacyDevice.BlendStates.Opaque);
        }

        private void DrawStatics(List<StaticInstance> staticsToDraw, List<Text> textToDraw, bool disableSelection = false)
        {
            if (staticsToDraw.Count == 0)
                return;

            var staticMeshEffect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];

            var camPos = Camera.GetPosition();

            var groups = staticsToDraw.GroupBy(s => s.WadObjectId);
            foreach (var group in groups)
            {
                var statID = _editor?.Level?.Settings?.WadTryGetStatic(group.Key);
                if (statID == null)
                    continue;
                var model = _wadRenderer.GetStatic(statID);

                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    var mesh = model.Meshes[i];
                    if (mesh.Vertices.Count == 0 || mesh.VertexBuffer == null || mesh.IndexBuffer == null || mesh.InputLayout == null)
                        continue;

                    _legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                    _legacyDevice.SetVertexInputLayout(mesh.InputLayout);
                    _legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);

                    foreach (var instance in group)
                    {
                        if (!disableSelection && _highlightedObjects.Contains(instance))
                            staticMeshEffect.Parameters["Color"].SetValue(_editor.Configuration.UI_ColorScheme.ColorSelection);
                        else
                        {
                            if (_editor.Mode == EditorMode.Lighting)
                            {
                                var entry = _editor.Level.Settings.GetStaticMergeEntry(instance.WadObjectId);

                                if (!ShowRealTintForObjects || entry == null && statID.Mesh.LightingType == WadMeshLightingType.VertexColors || entry != null && entry.Merge && entry.TintAsAmbient)
                                    staticMeshEffect.Parameters["Color"].SetValue(ConvertColor(instance.Color));
                                else if (_editor.Level.IsTombEngine)
                                    staticMeshEffect.Parameters["Color"].SetValue(ConvertColor(instance.Room.Properties.AmbientLight * instance.Color));
                                else
                                    staticMeshEffect.Parameters["Color"].SetValue(ConvertColor(instance.Room.Properties.AmbientLight));

                                if (entry != null && entry.Merge)
                                    staticMeshEffect.Parameters["StaticLighting"].SetValue(!entry.ClearShades);
                                else
                                    staticMeshEffect.Parameters["StaticLighting"].SetValue(_editor.Level.IsTombEngine ? true : statID.Mesh.LightingType == WadMeshLightingType.VertexColors);
                            }
                            else
                            {
                                staticMeshEffect.Parameters["Color"].SetValue(Vector4.One);
                                staticMeshEffect.Parameters["StaticLighting"].SetValue(false);
                            }
                        }

                        staticMeshEffect.Parameters["ModelViewProjection"].SetValue((instance.ObjectMatrix * _viewProjection).ToSharpDX());
                        staticMeshEffect.Parameters["AlphaTest"].SetValue(HideTransparentFaces);
                        staticMeshEffect.Parameters["ColoredVertices"].SetValue(_editor.Level.IsTombEngine);
                        staticMeshEffect.Parameters["TextureSampler"].SetResource(BilinearFilter ? _legacyDevice.SamplerStates.AnisotropicWrap : _legacyDevice.SamplerStates.PointWrap);
                        staticMeshEffect.Parameters["Texture"].SetResource(_wadRenderer.Texture);
                        staticMeshEffect.Techniques[0].Passes[0].Apply();

                        foreach (var submesh in mesh.Submeshes)
                        {
                            if (submesh.Value.NumIndices == 0)
                                continue;

                            submesh.Key.SetStates(_legacyDevice, _editor.Configuration.Rendering3D_HideTransparentFaces && _editor.SelectedObject != instance);
                            _legacyDevice.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                        }

                        // Add text message
                        if (i == 0 && _editor.SelectedObject == instance)
                        {
                            textToDraw.Add(CreateTextTagForObject(
                                instance.RotationPositionMatrix * _viewProjection,
                                instance.ItemType.StaticId.ToString(_editor.Level.Settings.GameVersion) +
                            instance.GetScriptIDOrName() + "\n" +
                            GetObjectPositionString(_editor.SelectedRoom, instance) +
                                "\n" + "Rotation Y: " + Math.Round(instance.RotationY, 2) +
                                GetObjectTriggerString(instance)));

                            // Add the line height of the object
                            AddObjectHeightLine(_editor.SelectedRoom, instance.Position);
                        }
                    }
                }
            }
        }

        private void DrawScene()
        {
            // Verify that editor is ready
            if (_editor == null || _editor.Level == null || _editor.SelectedRoom == null || _legacyDevice == null)
                return;

            // If any render exceptions were raised, bring app into safe mode and bypass rendering.
            if (SwapChain.RenderException != null)
            {
                if (!_editor.Configuration.Rendering3D_SafeMode)
                    _editor.Configuration.Rendering3D_SafeMode = true;
                return;
            }

            _watch.Restart();

            // Select light mode (0 = 32-bit, 1 = 16-bit, 2 = monochrome)
            int lightMode = 0;
            switch (_editor.Level.Settings.GameVersion)
            {
                case TRVersion.Game.TR1 or TRVersion.Game.TR1X:
                case TRVersion.Game.TR2 or TRVersion.Game.TR2X:
                    lightMode = 2;
                    break;

                case TRVersion.Game.TR3:
                case TRVersion.Game.TR4:
                    lightMode = 1;
                    break;

                case TRVersion.Game.TR5:
                case TRVersion.Game.TombEngine:
                    lightMode = 0;
                    break;

                case TRVersion.Game.TRNG:
                    lightMode = _editor.Level.Settings.Room32BitLighting ? 0 : 1;
                    break;
            }

            // New rendering setup
            _viewProjection = Camera.GetViewProjectionMatrix(ClientSize.Width, ClientSize.Height);
            _renderingStateBuffer.Set(new RenderingState
            {
                ShowExtraBlendingModes = ShowExtraBlendingModes,
                RoomGridForce = _editor.Mode == EditorMode.Geometry,
                RoomDisableVertexColors = _editor.Mode == EditorMode.FaceEdit,
                RoomGridLineWidth = _editor.Configuration.Rendering3D_LineWidth,
                TransformMatrix = _viewProjection,
                ShowLightingWhiteTextureOnly = ShowLightingWhiteTextureOnly,
                LightMode = lightMode
            });

            var renderArgs = new RenderingDrawingRoom.RenderArgs
            {
                RenderTarget = SwapChain,
                StateBuffer = _renderingStateBuffer,
                BilinearFilter = BilinearFilter
            };

            // Prepare sprite and text lists for collecting
            var spritesToDraw = new List<Sprite>();
            var textToDraw = new List<Text>();

            // Reset
            _drawHeightLine = false;
            ((TombLib.Rendering.DirectX11.Dx11RenderingSwapChain)SwapChain).BindForce();
            _legacyDevice.SetDepthStencilState(_legacyDevice.DepthStencilStates.Default);
            _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);

            // Update frustum
            _frustum.Update(Camera, ClientSize);

            // Collect stuff to draw
            var roomsToDraw = CollectRoomsToDraw().Where(r => _frustum.Contains(r.WorldBoundingBox)).ToArray();
            var moveablesToDraw = CollectMoveablesToDraw(roomsToDraw);
            var staticsToDraw = CollectStaticsToDraw(roomsToDraw);
            var importedGeometryToDraw = CollectImportedGeometryToDraw(roomsToDraw);
            var volumesToDraw = CollectVolumesToDraw(roomsToDraw);
            var ghostBlocksToDraw = CollectGhostBlocksToDraw(roomsToDraw);

            // Draw skybox
            if (ShowHorizon)
                DrawSkybox();

            // Draw enabled rooms
            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();
            foreach (Room room in roomsToDraw.Where(r => !DisablePickingForHiddenRooms || !r.Properties.Hidden))
                _renderingCachedRooms[room].Render(renderArgs);

            // Determine if selection should be visible or not.
            var hiddenSelection = _editor.Mode == EditorMode.Lighting && _editor.HiddenSelection;

            // Draw moveables and static meshes
            {
                _legacyDevice.SetRasterizerState(_rasterizerStateDepthBias);

                if (ShowMoveables)
                    DrawMoveables(moveablesToDraw, textToDraw, hiddenSelection);
                if (ShowStatics)
                    DrawStatics(staticsToDraw, textToDraw, hiddenSelection);

                _legacyDevice.SetRasterizerState(_legacyDevice.RasterizerStates.CullBack);
            }

            // Draw room imported geometry
            if (importedGeometryToDraw.Count != 0 && ShowImportedGeometry)
                DrawImportedGeometry(importedGeometryToDraw, textToDraw, hiddenSelection);

            // Get common effect for service objects
            var effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Solid"];

            // Draw volumes
            if (ShowVolumes)
                DrawVolumes(effect, volumesToDraw, textToDraw, spritesToDraw);

            // Draw moveables and statics bounding boxes
            if (ShowBoundingBoxes)
            {
                var list = moveablesToDraw.Select(m => m as ObjectInstance)
                                          .Concat(staticsToDraw.Select(s => s as ObjectInstance)).ToList();
                DrawBoundingBoxes(effect, list);
            }

            if (ShowOtherObjects)
            {
                // Draw sprites
                DrawSprites(roomsToDraw, spritesToDraw, hiddenSelection);
                // Draw placeholder objects (sinks, cameras, fly-by cameras, sound sources and missing 3D objects)
                DrawPlaceholders(effect, roomsToDraw, textToDraw, spritesToDraw);
                // Draw light objects and bounding volumes
                DrawLights(effect, roomsToDraw, textToDraw, spritesToDraw);
                // Draw flyby path
                DrawFlybyPath(effect);
                // Draw sector split highlights
                DrawSectorSplitHighlights(effect);
            }

            // Draw ghost block cubes
            if (ShowGhostBlocks)
                DrawGhostBlocks(effect, ghostBlocksToDraw, textToDraw, spritesToDraw);

            // Depth-sort sprites
            spritesToDraw = spritesToDraw.OrderByDescending(s => s.Depth).ToList();

            // Draw depth-dependent sprites
            var depthSprites = spritesToDraw.Where(s => s.Depth.HasValue).ToList();
            if (depthSprites.Count > 0)
            {
                _legacyDevice.SetBlendState(_legacyDevice.BlendStates.AlphaBlend);
                SwapChain.RenderSprites(_renderingTextures, BilinearFilter, false, depthSprites);
            }

            // Draw ghost block bodies
            if (ShowGhostBlocks)
                DrawGhostBlockBodies(effect, ghostBlocksToDraw);

            // Draw disabled rooms, so they don't conceal all geometry behind
            var hiddenRooms = roomsToDraw.Where(r => DisablePickingForHiddenRooms && r.Properties.Hidden).ToList();
            if (hiddenRooms.Count > 0)
            {
                _legacyDevice.SetBlendState(_legacyDevice.BlendStates.AlphaBlend);
                _legacyDevice.SetDepthStencilState(_legacyDevice.DepthStencilStates.DepthRead);
                foreach (Room room in hiddenRooms)
                    _renderingCachedRooms[room].Render(renderArgs);
                _legacyDevice.SetBlendState(_legacyDevice.BlendStates.Opaque);
            }

            // Draw the height of the object and room bounding box
            DrawDebugLines(effect);

            ((TombLib.Rendering.DirectX11.Dx11RenderingDevice)Device).ResetState();

            // Draw the gizmo
            SwapChain.ClearDepth();
            _gizmo.Draw(_viewProjection);

            // Draw depth-independent sprites
            var flatSprites = spritesToDraw.Where(s => !s.Depth.HasValue).ToList();
            if (flatSprites.Count > 0)
            {
                _legacyDevice.SetBlendState(_legacyDevice.BlendStates.AlphaBlend);
                SwapChain.RenderSprites(_renderingTextures, BilinearFilter, true, flatSprites);
            }

            _watch.Stop();

            // At last, construct additional labels and draw all in-game text
            DrawText(roomsToDraw, textToDraw);
        }
    }
}
