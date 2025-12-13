using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorStructs;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.Compilers.TombEngine
{
    public sealed partial class LevelCompilerTombEngine
    {
        private readonly Dictionary<Room, int> _roomRemapping = new Dictionary<Room, int>(new ReferenceEqualityComparer<Room>());
        private readonly Dictionary<TombEnginePortal, PortalInstance> _portalRemapping = new Dictionary<TombEnginePortal, PortalInstance>();
        private readonly List<Room> _roomUnmapping = new List<Room>();
        private Dictionary<WadPolygon, TombEngineTexInfoManager.Result> _mergedStaticMeshTextureInfos = new Dictionary<WadPolygon, TombEngineTexInfoManager.Result>();
        private Dictionary<ShadeMatchSignature, Vector3> _vertexColors;
		private Dictionary<Vector3, List<(TombEngineRoom room, int vertexIndex, NormalHelper poly)>> _normalGroups;
        private Dictionary<Room, VectorInt2> _roomsMinFloorMaxCeilingCache = new Dictionary<Room, VectorInt2>();
        
		private void BuildRooms(CancellationToken cancelToken)
        {
            ReportProgress(5, "Lighting Rooms");

            ParallelOptions parallelOptions = new ParallelOptions()
            {
                CancellationToken = cancelToken
            };

            Parallel.ForEach(_level.ExistingRooms, parallelOptions, (room) =>
            {
                room.RebuildLighting(!_level.Settings.FastMode);
            });

			ReportProgress(15, "Building rooms");

            foreach (var room in _level.ExistingRooms)
            {
                _roomRemapping.Add(room, _roomUnmapping.Count);
                _roomUnmapping.Add(room);
                room.RebuildPortalsCache();
                _roomsMinFloorMaxCeilingCache.Add(
                    room,
                    new VectorInt2(room.GetLowestCorner(), room.GetHighestCorner()));
            }

            foreach (var room in _roomRemapping.Keys)
            {
                cancelToken.ThrowIfCancellationRequested();
                _tempRooms.Add(room, BuildRoom(room));
            }

            // Remove WaterScheme values for water rooms
            Parallel.ForEach(_tempRooms.Values, parallelOptions, (TombEngineRoom trRoom) => { if ((trRoom.Flags & 0x0001) != 0) trRoom.WaterScheme = 0; });

            ReportProgress(20, "    Number of rooms: " + _roomUnmapping.Count);

            if (!_level.Settings.FastMode)
            {
                ReportProgress(23, "    Matching vertex colors on portals...");

                _vertexColors = new Dictionary<ShadeMatchSignature, Vector3>();
                var rooms = _tempRooms.Values.ToList();
                for (int flipped = 0; flipped <= 1; flipped++)
                    foreach (var room in rooms)
                    {
                        cancelToken.ThrowIfCancellationRequested();
                        MatchDoorShades(rooms, room, false, flipped == 1);
                    }
            }

            Parallel.ForEach(_tempRooms.Values, parallelOptions, (TombEngineRoom trRoom) =>
            {
                for (int i = 0; i < trRoom.Vertices.Count; i++)
                {
                    var v = trRoom.Vertices[i];
                    if (!v.IsOnPortal)
                        continue;

                    var sig = new ShadeMatchSignature()
                    {
                        IsWater = ((trRoom.Flags & 1) == 1),
                        AlternateGroup = trRoom.AlternateKind == AlternateKind.AlternateRoom ? trRoom.AlternateGroup : -1,
                        Position = new VectorInt3(trRoom.Info.X + (int)v.Position.X, (int)v.Position.Y, trRoom.Info.Z + (int)v.Position.Z)
                    };

                    if (_vertexColors.ContainsKey(sig))
                    {
                        v.Color = _vertexColors[sig];
                        v.Color = _vertexColors[sig];
                        trRoom.Vertices[i] = v;
                    }
                }
            });

            ReportProgress(25, "    Vertex colors on portals matched.");
        }

        private Vector3 CalculateLightForCustomVertex(Room room, Vector3 position, Vector3 normal, bool forImportedGeometry, Vector3 ambientColor)
        {
            Vector3 output = ambientColor;

            if (position.X >= 0 && position.Z >= 0 &&
                position.X < room.NumXSectors * Level.SectorSizeUnit && position.Z < room.NumZSectors * Level.SectorSizeUnit)
                foreach (var obj in room.Objects)
                    if (obj is LightInstance)
                    {
                        var light = obj as LightInstance;

                        // Disable this light for imported geometry, if IsUsedForImportedGeometry flag is not set,
                        // or for static meshes, if IsStaticallyUsed is not set

                        if ((!light.IsUsedForImportedGeometry && forImportedGeometry) ||
                            (!light.IsStaticallyUsed && !forImportedGeometry))
                            continue;

                        output += RoomGeometry.CalculateLightForVertex(room, light, position, normal, false, false);
                    }

            return Vector3.Max(output, new Vector3()) * (1.0f / 128.0f);
        }

        private TombEngineRoom BuildRoom(Room room)
        {
            int maxDimensions = _limits[Limit.RoomDimensions];
            if (room.NumXSectors >= maxDimensions || room.NumZSectors >= maxDimensions)
                _progressReporter.ReportWarn("Room '" + room + "' is very big! Rooms bigger than " + maxDimensions + " sectors per side may cause trouble with rendering.");

            if (room.Position.X <= -1 || room.Position.Z <= -1)
                _progressReporter.ReportWarn("Room '" + room + "' is out of map bounds. Collision and AI errors may occur. Move room back into the map if it's reachable.");

            var newRoom = new TombEngineRoom
            {
                OriginalRoom = room,
                Lights = new List<TombEngineRoomLight>(),
                StaticMeshes = new List<TombEngineRoomStaticMesh>(),
                Portals = new List<TombEnginePortal>(),
                Info = new tr_room_info
                {
                    X = room.WorldPos.X,
                    Z = room.WorldPos.Z,
                    YTop = -(room.WorldPos.Y + room.GetHighestCorner()),
                    YBottom = -(room.WorldPos.Y + room.GetLowestCorner())
                },
                NumXSectors = checked((ushort)room.NumXSectors),
                NumZSectors = checked((ushort)room.NumZSectors),
                AlternateRoom = room.Alternated && room.AlternateRoom != null ? _roomRemapping[room.AlternateRoom] : -1,
                AlternateGroup = room.Alternated ? room.AlternateGroup : -1,
                Flipped = room.Alternated,
                FlippedRoom = room.AlternateRoom,
                BaseRoom = room.AlternateBaseRoom,
                ReverbInfo = room.Properties.Reverberation,
                Flags = 0
            };

            if (!room.Alternated)
                newRoom.AlternateKind = AlternateKind.NotAlternated;
            else if (room.AlternateBaseRoom != null)
                newRoom.AlternateKind = AlternateKind.AlternateRoom;
            else if (room.AlternateRoom != null)
                newRoom.AlternateKind = AlternateKind.BaseRoom;

            // Store ambient intensity
            newRoom.AmbientLight = room.Properties.AmbientLight;

            // Room flags
            if (room.Properties.FlagHorizon)
                newRoom.Flags |= 0x0008;
            if (room.Properties.FlagOutside)
                newRoom.Flags |= 0x0020;

            // Not-near-horizon flag (set automatically)
            if (!room.Properties.FlagHorizon && !room.PortalsCache.Any(p => p.Room.Properties.FlagHorizon))
                newRoom.Flags |= 0x0040;

            // TRNG-specific flags
            if (room.Properties.FlagDamage)
                newRoom.Flags |= 0x0800;
            if (room.Properties.FlagCold)
                newRoom.Flags |= 0x1000;
            if (room.Properties.FlagNoLensflare)
                newRoom.Flags |= 0x0080;

            // Room type
            switch (room.Properties.Type)
            {
                case RoomType.Water:
                    newRoom.Flags |= 0x0001;
                    break;
                case RoomType.Quicksand:
                    newRoom.Flags |= 0x0004;
                    break;
            }

            var lightEffect = room.Properties.LightEffect;
            var waterPortals = room.PortalsCache.Where(p => (room.Properties.Type == RoomType.Normal && p.Direction == PortalDirection.Floor   && p.AdjoiningRoom.Properties.Type >= RoomType.Water) ||
                                                            (room.Properties.Type >= RoomType.Water  && p.Direction == PortalDirection.Ceiling && p.AdjoiningRoom.Properties.Type == RoomType.Normal)).ToList();

            bool waterSchemeSet = false;

            // Calculate bottom room-based water scheme in advance, if mode is default, mist or reflection
            if (waterPortals.Count > 0 && room.Properties.Type < RoomType.Water &&
                (lightEffect == RoomLightEffect.Default || lightEffect == RoomLightEffect.Reflection || lightEffect == RoomLightEffect.Mist))
            {
                var waterRoom = waterPortals.First().AdjoiningRoom;
                newRoom.WaterScheme = (byte)((waterRoom.Properties.LightEffectStrength * 4) + room.Properties.LightEffectStrength);
                waterSchemeSet = true;
            }

            // Force different effect type
            if (lightEffect == RoomLightEffect.Default)
            {
                switch (room.Properties.Type)
                {
                    case RoomType.Water:
                        lightEffect = RoomLightEffect.Glow;
                        break;
                    case RoomType.Quicksand:
                        lightEffect = RoomLightEffect.Movement;
                        break;
                    default:
                        lightEffect = RoomLightEffect.None;
                        break;
                }
            }

            // Light effect
            // WARNING: DO NOT use raw value of 1 in WaterScheme ever with classic tomb engines,
            // it will result in broken effect.

            switch (lightEffect)
            {
                case RoomLightEffect.GlowAndMovement:
                    if (!waterSchemeSet) newRoom.WaterScheme = (byte)(room.Properties.LightEffectStrength * 5.0f);
                    newRoom.Flags |= 0x0100;
                    break;

                case RoomLightEffect.Movement:
                    if (!waterSchemeSet) newRoom.WaterScheme = (byte)(room.Properties.LightEffectStrength * 5.0f);
                    break;

                case RoomLightEffect.Glow:
                case RoomLightEffect.Mist:
                    if (!waterSchemeSet) newRoom.WaterScheme = (byte)(room.Properties.LightEffectStrength == 0 ? 0 : room.Properties.LightEffectStrength + 1);
                    newRoom.Flags |= 0x0100;
                    break;

                case RoomLightEffect.Reflection:
                    newRoom.Flags |= 0x0200;
                    break;

                case RoomLightEffect.None:
                    if (!waterSchemeSet)
                        newRoom.WaterScheme = (byte)(room.Properties.LightEffectStrength * 5.0f);
                    break;
            }

            // Light interpolation mode
            var interpMode = room.Properties.LightInterpolationMode;
            if (interpMode == RoomLightInterpolationMode.Default)
            {
                switch (room.Properties.Type)
                {
                    case RoomType.Water:
                        interpMode = RoomLightInterpolationMode.NoInterpolate;
                        break;

                    default:
                        interpMode = RoomLightInterpolationMode.Interpolate;
                        break;
                }
            }

            // Generate geometry
            {
                // Add room geometry

                var vertexPositions = room.RoomGeometry.VertexPositions;
                var vertexColors = room.RoomGeometry.VertexColors;

                var roomVerticesDictionary = new Dictionary<int, int>();
                var roomVertices = new List<TombEngineVertex>();
                var roomPolygons = new List<TombEnginePolygon>();

                // Add room's own geometry

                if (!room.Properties.Hidden)
                    for (int z = 0; z < room.NumZSectors; ++z)
                        for (int x = 0; x < room.NumXSectors; ++x)
                            foreach (SectorFace face in room.Sectors[x, z].GetFaceTextures().Keys)
                            {
                                var range = room.RoomGeometry.VertexRangeLookup.TryGetOrDefault(new SectorFaceIdentity(x, z, face));
                                var shape = room.GetFaceShape(x, z, face);
								
                                if (range.Count == 0)
                                    continue;

                                TextureArea texture = room.Sectors[x, z].GetFaceTexture(face);
                                if (texture.TextureIsInvisible)
                                    continue;

                                if (texture.TextureIsUnavailable)
                                {
                                    _progressReporter.ReportWarn("Missing texture at sector (" + x + "," + z + ") in room " + room.Name + ". Check texture file location.");
                                    continue;
                                }

                                if ((shape == FaceShape.Triangle && texture.TriangleCoordsOutOfBounds) || (shape == FaceShape.Quad && texture.QuadCoordsOutOfBounds))
                                {
                                    _progressReporter.ReportWarn("Texture is out of bounds at sector (" + x + "," + z + ") in room " + room.Name + ". Wrong or resized texture file?");
                                    continue;
                                }

								var realBlendMode = texture.BlendMode;
								if (texture.BlendMode == BlendMode.Normal)
									realBlendMode = texture.Texture.Image.HasAlpha(TRVersion.Game.TombEngine, texture.GetRect());
                           
                                var materialIndex = _materialNames.IndexOf(texture.Texture.Image.FileName);
                                if (materialIndex == -1)
                                    materialIndex = 0;

                                int rangeEnd = range.Start + range.Count;
                                for (int i = range.Start; i < rangeEnd; i += 3)
                                {
                                    int vertex0Index, vertex1Index, vertex2Index;

                                    if (shape == FaceShape.Quad)
                                    {
                                        int vertex3Index;

                                        if (face == SectorFace.Ceiling)
                                        {
                                            texture.Mirror();
                                            vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 1], vertexColors[i + 1], 0);
                                            vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 2], vertexColors[i + 2], 1);
                                            vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 0], vertexColors[i + 0], 2);
                                            vertex3Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 5], vertexColors[i + 5], 3);
                                        }
                                        else
                                        {
                                            vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 3], vertexColors[i + 3], 0);
                                            vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 2], vertexColors[i + 2], 1);
                                            vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 0], vertexColors[i + 0], 2);
                                            vertex3Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 1], vertexColors[i + 1], 3);
                                        }

                                        var result = _textureInfoManager.AddTexture(texture, TextureDestination.RoomOrAggressive, false, realBlendMode);
                                        var poly = result.CreateTombEnginePolygon4(new int[] { vertex0Index, vertex1Index, vertex2Index, vertex3Index },
                                                         realBlendMode, materialIndex, roomVertices);
                                        roomPolygons.Add(poly);
                                        roomVertices[vertex0Index].NormalHelpers.Add(new NormalHelper(poly));
                                        roomVertices[vertex1Index].NormalHelpers.Add(new NormalHelper(poly));
                                        roomVertices[vertex2Index].NormalHelpers.Add(new NormalHelper(poly));
                                        roomVertices[vertex3Index].NormalHelpers.Add(new NormalHelper(poly));
                                        if (texture.DoubleSided)
                                        {
                                            texture.Mirror();
                                            result = _textureInfoManager.AddTexture(texture, TextureDestination.RoomOrAggressive, false, realBlendMode);
                                            poly = result.CreateTombEnginePolygon4(new int[] { vertex3Index, vertex2Index, vertex1Index, vertex0Index },
                                                            realBlendMode, materialIndex, roomVertices);
                                            roomPolygons.Add(poly);

                                            // TODO: Solve problems with averaging normals on double-sided triangles
                                            // roomVertices[vertex0Index].NormalHelpers.Add(new NormalHelper(poly));
                                            // roomVertices[vertex1Index].NormalHelpers.Add(new NormalHelper(poly));
                                            // roomVertices[vertex2Index].NormalHelpers.Add(new NormalHelper(poly));
                                            // roomVertices[vertex3Index].NormalHelpers.Add(new NormalHelper(poly));
                                        }
                                        i += 3;
                                    }
                                    else
                                    {
                                        if (face == SectorFace.Ceiling || face == SectorFace.Ceiling_Triangle2)
                                            texture.Mirror(true);

                                        vertex0Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 0], vertexColors[i + 0], 0);
                                        vertex1Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 1], vertexColors[i + 1], 1);
                                        vertex2Index = GetOrAddVertex(room, roomVerticesDictionary, roomVertices, vertexPositions[i + 2], vertexColors[i + 2], 2);

                                        var result = _textureInfoManager.AddTexture(texture, TextureDestination.RoomOrAggressive, true, realBlendMode);
                                        var poly = result.CreateTombEnginePolygon3(new int[] { vertex0Index, vertex1Index, vertex2Index },
                                                        realBlendMode, materialIndex, roomVertices);
                                        roomPolygons.Add(poly);
                                        roomVertices[vertex0Index].NormalHelpers.Add(new NormalHelper(poly));
                                        roomVertices[vertex1Index].NormalHelpers.Add(new NormalHelper(poly));
                                        roomVertices[vertex2Index].NormalHelpers.Add(new NormalHelper(poly));
                                        if (texture.DoubleSided)
                                        {
                                            texture.Mirror(true);
                                            result = _textureInfoManager.AddTexture(texture, TextureDestination.RoomOrAggressive, true, realBlendMode);
                                            poly = result.CreateTombEnginePolygon3(new int[] { vertex2Index, vertex1Index, vertex0Index },
                                                            realBlendMode, materialIndex, roomVertices);
                                            roomPolygons.Add(poly);

                                            // TODO: Solve problems with averaging normals on double-sided triangles
                                            // roomVertices[vertex0Index].NormalHelpers.Add(new NormalHelper(poly));
                                            // roomVertices[vertex1Index].NormalHelpers.Add(new NormalHelper(poly));
                                            // roomVertices[vertex2Index].NormalHelpers.Add(new NormalHelper(poly));
                                        }
                                    }
                                }
                            }

                // Merge static meshes

                if (!_level.Settings.FastMode)
                    foreach (var staticMesh in room.Objects.OfType<StaticInstance>())
                    {
                        // Сheck if static Mesh is in the Auto Merge list
                        var entry = _level.Settings.AutoStaticMeshMerges.Find((mergeEntry) =>
                            mergeEntry.meshId == staticMesh.WadObjectId.TypeId);

                        if (entry == null)
                            continue;

                        bool interpretShadesAsEffect = entry.InterpretShadesAsEffect;
                        bool clearShades = entry.ClearShades;
                        int meshVertexBase = roomVertices.Count;
                        var worldTransform = staticMesh.ScaleMatrix *
                                             staticMesh.RotationMatrix *
                                             Matrix4x4.CreateTranslation(staticMesh.Position);
                        var normalTransform = staticMesh.RotationMatrix;
                        WadStatic wadStatic = _level.Settings.WadTryGetStatic(staticMesh.WadObjectId);

                        if (wadStatic == null || wadStatic.Mesh == null)
                            continue;

                        for (int j = 0; j < wadStatic.Mesh.VertexPositions.Count; j++)
                        {
                            // Apply the transform to the vertex
                            Vector3 position = MathC.HomogenousTransform(wadStatic.Mesh.VertexPositions[j], worldTransform);
                            Vector3 normal = MathC.HomogenousTransform(wadStatic.Mesh.VertexNormals[j], normalTransform);
                            Vector3 shade = Vector3.One;

                            var glow = 0f;
                            var move = 0f;

                            if (interpretShadesAsEffect)
                            {
                                if (j < wadStatic.Mesh.VertexColors.Count)
                                {
                                    var luma = wadStatic.Mesh.VertexColors[j].GetLuma();
                                    if (luma < 0.5f) move = luma * 2.0f;   // Movement
                                    else if (luma < 1.0f) glow = (luma - 0.5f) * 2.0f; // Glow
                                }
                            }
                            else
                            {
                                // If we have vertex colors, use them as a luma factor for the resulting vertex color
                                if (!clearShades && wadStatic.Mesh.HasColors)
                                    shade = wadStatic.Mesh.VertexColors[j];

                                if (wadStatic.Mesh.HasAttributes)
                                {
                                    if (wadStatic.Mesh.VertexAttributes[j].Move > 0)
                                        move = (float)wadStatic.Mesh.VertexAttributes[j].Move / 64.0f; // Movement

                                    if (wadStatic.Mesh.VertexAttributes[j].Glow > 0)
                                        glow = (float)wadStatic.Mesh.VertexAttributes[j].Glow / 64.0f; // Glow
                                }
                            }

                            Vector3 color;
                            if (!entry.TintAsAmbient)
                            {
                                color = CalculateLightForCustomVertex(room, position, normal, false, room.Properties.AmbientLight * 128);
                                // Apply Shade factor
                                color *= shade;
                                // Apply Instance Color
                                color *= staticMesh.Color;
                            }
                            else
                            {
                                color = CalculateLightForCustomVertex(room, position, normal, false, staticMesh.Color * 128);
                                //Apply Shade factor
                                color *= shade;
                            }

                            var trVertex = new TombEngineVertex
                            {
                                Position = new Vector3(position.X, -(position.Y + room.WorldPos.Y), position.Z),
                                Color = color,
                                Normal = normal,
                                Glow = glow,
                                Move = move
                            };

                            roomVertices.Add(trVertex);
                        }

                        foreach (bool doubleSided in new[] { false, true })
                        {
                            for (int i = 0; i < wadStatic.Mesh.Polys.Count; i++)
                            {
                                WadPolygon poly = wadStatic.Mesh.Polys[i];

                                if (!poly.Texture.DoubleSided && doubleSided)
                                    continue;

								int index0 = poly.Index0 + meshVertexBase;
                                int index1 = poly.Index1 + meshVertexBase;
                                int index2 = poly.Index2 + meshVertexBase;
                                int index3 = poly.Index3 + meshVertexBase;

                                var texture = poly.Texture;
                                texture.ClampToBounds();

                                int[] indices = poly.IsTriangle ? new int[] { index0, index1, index2 } :
                                                                  new int[] { index0, index1, index2, index3 };
                                var key = poly;

                                if (doubleSided)
                                {
                                    Array.Reverse(indices);
                                    texture.Mirror(poly.IsTriangle);
                                    key.Index0 = indices[0]; key.Index1 = indices[1]; key.Index2 = indices[2];
                                    if (!poly.IsTriangle) key.Index3 = indices[3];
                                }

                                var realBlendMode = texture.BlendMode;
                                if (texture.BlendMode == BlendMode.Normal)
                                    realBlendMode = texture.Texture.Image.HasAlpha(TRVersion.Game.TombEngine, texture.GetRect());

								var materialIndex = _materialNames.IndexOf(texture.Texture.Image.FileName);
                                if (materialIndex == -1)
                                    materialIndex = 0;

                                bool texInfoExists = _mergedStaticMeshTextureInfos.ContainsKey(key);
                                var result = texInfoExists ? _mergedStaticMeshTextureInfos[key] :
                                            _textureInfoManager.AddTexture(texture, TextureDestination.RoomOrAggressive, poly.IsTriangle, realBlendMode);

                                var face = poly.IsTriangle ?
                                    result.CreateTombEnginePolygon3(indices, realBlendMode, materialIndex, roomVertices) :
                                    result.CreateTombEnginePolygon4(indices, realBlendMode, materialIndex, roomVertices);

                                if (!texInfoExists)
                                    _mergedStaticMeshTextureInfos.Add(key, result);

                                roomPolygons.Add(face);

                                if (!doubleSided)
                                {
                                    roomVertices[index0].NormalHelpers.Add(new NormalHelper(face));
                                    roomVertices[index1].NormalHelpers.Add(new NormalHelper(face));
                                    roomVertices[index2].NormalHelpers.Add(new NormalHelper(face));
                                    if (!poly.IsTriangle)
                                        roomVertices[index3].NormalHelpers.Add(new NormalHelper(face));
                                }
                            }
                        }
                    }

                // Add geometry imported objects

                foreach (var geometry in room.Objects.OfType<ImportedGeometryInstance>())
                {
                    if (geometry.Model?.DirectXModel == null)
                        continue;

                    var meshes = geometry.Model.DirectXModel.Meshes;
                    var worldTransform = geometry.RotationMatrix *
                                         Matrix4x4.CreateScale(geometry.Scale) *
                                         Matrix4x4.CreateTranslation(geometry.Position);
                    var normalTransform = geometry.RotationMatrix;
                    var indexList = new List<int>();
                    int baseIndex = 0;

                    foreach (var mesh in meshes)
                    {
                        int currentMeshIndexCount = 0;

                        foreach (var submesh in mesh.Submeshes)
                        {
                            foreach (bool doubleSided in (submesh.Key.DoubleSided ? new[] { false, true } : new[] { false }))
                            {
                                for (int j = 0; j < submesh.Value.Indices.Count; j += 3)
                                {
                                    int[] tempIndices = new int[3];

                                    // We need to add vertices here
                                    for (int k = 0; k < 3; k++)
                                    {
                                        var vertex = mesh.Vertices[submesh.Value.Indices[j + k]];

                                        // Since imported geometry can be used as reimported room mesh, we need to make sure
                                        // coordinates are integer to comply with portal positions, so MatchDoorShades function
                                        // will later work correctly. While technically rounding vertex positions is incorrect,
                                        // thankfully TR engines use large-scale coordinate system, so we can ignore such precision loss.

                                        vertex.Position = MathC.Round(vertex.Position);

                                        // Apply the transform to the vertex
                                        Vector3 position = MathC.HomogenousTransform(vertex.Position, worldTransform);
                                        Vector3 normal = MathC.HomogenousTransform(vertex.Normal, normalTransform);
                                        normal = Vector3.Normalize(normal);

                                        if (doubleSided)
                                            normal = -normal;

                                        var trVertex = new TombEngineVertex
                                        {
                                            Position = new Vector3(position.X, -(position.Y + room.WorldPos.Y), position.Z),
                                            Normal = normal,
                                            DoubleSided = doubleSided
                                        };

                                        // Pack the light according to chosen lighting model
                                        if (geometry.LightingModel == ImportedGeometryLightingModel.VertexColors)
                                        {
                                            trVertex.Color = vertex.Color;
                                        }
                                        else if (geometry.LightingModel == ImportedGeometryLightingModel.CalculateFromLightsInRoom)
                                        {
                                            var color = CalculateLightForCustomVertex(room, position, normal, true, room.Properties.AmbientLight * 128);
                                            trVertex.Color = color;
                                        }
                                        else
                                        {
                                            var color = room.Properties.AmbientLight;
                                            trVertex.Color = color;
                                        }

                                        // HACK: Find a vertex with same coordinates and merge with it.
                                        // This is needed to overcome disjointed vertices bug buried deep in geometry importer AND assimp's own
                                        // strange behaviour which splits imported geometry based on materials.
                                        // We still preserve sharp edges if explicitly specified by flag though.

                                        int existingIndex;
                                        if (geometry.SharpEdges)
                                        {
                                            existingIndex = roomVertices.Count;
                                            roomVertices.Add(trVertex);
                                        }
                                        else
                                        {
                                            existingIndex = -1;
											var span = CollectionsMarshal.AsSpan(roomVertices);
                                            for (int i = 0; (uint)i < (uint)span.Length; i++)
                                                if (span[i].Position == trVertex.Position &&
                                                    span[i].Color == trVertex.Color &&
                                                    span[i].DoubleSided == trVertex.DoubleSided)
                                                {
                                                    existingIndex = i;
                                                    break;
                                                }
                                            if (existingIndex == -1)
                                            {
                                                existingIndex = roomVertices.Count;
                                                roomVertices.Add(trVertex);
                                            }
                                        }

                                        indexList.Add(existingIndex);
                                        tempIndices[k] = existingIndex;
                                        currentMeshIndexCount++;
                                    }

                                    // Avoid degenerate triangles
                                    if (tempIndices.Distinct().Count() < 3)
                                    {
                                        continue;
                                    }

									int index0 = tempIndices[0];
                                    int index1 = tempIndices[1];
                                    int index2 = tempIndices[2];

                                    int[] indices = new int[] { index0, index1, index2 };
                                    if (doubleSided)
                                    {
                                        Array.Reverse(indices);
                                    }

                                    // TODO Move texture area into the mesh
                                    TextureArea texture = new TextureArea();
                                    texture.DoubleSided = submesh.Key.DoubleSided;
                                    texture.BlendMode = submesh.Key.AdditiveBlending ? BlendMode.Additive : BlendMode.Normal;
                                    texture.Texture = submesh.Value.Material.Texture;
                                    texture.TexCoord0 = mesh.Vertices[submesh.Value.Indices[j + 0]].UV;
                                    texture.TexCoord1 = mesh.Vertices[submesh.Value.Indices[j + 1]].UV;
                                    texture.TexCoord2 = mesh.Vertices[submesh.Value.Indices[j + 2]].UV;
                                    texture.TexCoord3 = texture.TexCoord2;

                                    if (geometry.Model.Info.MappedUV)
                                        texture.SetParentArea(_limits[Limit.TexPageSize]);

                                    texture.ClampToBounds();

                                    var realBlendMode = texture.BlendMode;
                                    if (texture.BlendMode == BlendMode.Normal)
                                        realBlendMode = texture.Texture.Image.HasAlpha(TRVersion.Game.TombEngine, texture.GetRect());

                                    if (realBlendMode == BlendMode.AlphaBlend && geometry.UseAlphaTestInsteadOfAlphaBlend)
                                        realBlendMode = BlendMode.AlphaTest;

									var materialIndex = _materialNames.IndexOf(texture.Texture.Image.FileName);
                                    if (materialIndex == -1)
                                        materialIndex = 0;

                                    if (doubleSided)
                                    {
                                        texture.Mirror(true);
                                    }

                                    var result = _textureInfoManager.AddTexture(texture, TextureDestination.RoomOrAggressive, true, realBlendMode);
                                    var tri = result.CreateTombEnginePolygon3(indices, realBlendMode, materialIndex, roomVertices);

                                    roomPolygons.Add(tri);
                                    roomVertices[index0].NormalHelpers.Add(new NormalHelper(tri));
                                    roomVertices[index1].NormalHelpers.Add(new NormalHelper(tri));
                                    roomVertices[index2].NormalHelpers.Add(new NormalHelper(tri));
                                }
                            }
                        }

                        baseIndex += currentMeshIndexCount;
                    }
                }

                newRoom.Vertices = roomVertices;
                newRoom.Polygons = roomPolygons;

                // Now we need to build final normals, tangents, bitangents
            }

            // Assign vertex effects

            for (int i = 0; i < newRoom.Vertices.Count; ++i)
            {
                var trVertex = newRoom.Vertices[i];
                bool allowGlow = true;

                var xv = (int)(trVertex.Position.X / Level.SectorSizeUnit);
                var zv = (int)(trVertex.Position.Z / Level.SectorSizeUnit);

                // Check for vertex out of room bounds
                if (xv <= 0 || zv <= 0 || xv >= room.NumXSectors || zv >= room.NumZSectors)
                    continue;

                foreach (var portal in room.PortalsCache)
                {
                    var otherRoomLightEffect = portal.AdjoiningRoom.Properties.LightEffect;
                    if (otherRoomLightEffect == RoomLightEffect.Default)
                    {
                        switch (portal.AdjoiningRoom.Properties.Type)
                        {
                            case RoomType.Water:
                                otherRoomLightEffect = RoomLightEffect.Glow;
                                break;
                            case RoomType.Quicksand:
                                otherRoomLightEffect = RoomLightEffect.Movement;
                                break;
                            default:
                                otherRoomLightEffect = RoomLightEffect.None;
                                break;
                        }
                    }

                    var connectionInfo1 = portal.Direction == PortalDirection.Floor ? room.GetFloorRoomConnectionInfo(new VectorInt2(xv, zv)) : room.GetCeilingRoomConnectionInfo(new VectorInt2(xv, zv));
                    var connectionInfo2 = portal.Direction == PortalDirection.Floor ? room.GetFloorRoomConnectionInfo(new VectorInt2(xv - 1, zv)) : room.GetCeilingRoomConnectionInfo(new VectorInt2(xv - 1, zv));
                    var connectionInfo3 = portal.Direction == PortalDirection.Floor ? room.GetFloorRoomConnectionInfo(new VectorInt2(xv, zv - 1)) : room.GetCeilingRoomConnectionInfo(new VectorInt2(xv, zv - 1));
                    var connectionInfo4 = portal.Direction == PortalDirection.Floor ? room.GetFloorRoomConnectionInfo(new VectorInt2(xv - 1, zv - 1)) : room.GetCeilingRoomConnectionInfo(new VectorInt2(xv - 1, zv - 1));

                    bool isTraversablePortal = connectionInfo1.TraversableType == Room.RoomConnectionType.FullPortal &&
                                               connectionInfo2.TraversableType == Room.RoomConnectionType.FullPortal &&
                                               connectionInfo3.TraversableType == Room.RoomConnectionType.FullPortal &&
                                               connectionInfo4.TraversableType == Room.RoomConnectionType.FullPortal;

                    bool isOppositeCorner = connectionInfo1.TraversableType == Room.RoomConnectionType.TriangularPortalXnZn &&
                                            !connectionInfo2.IsTriangularPortal &&
                                            !connectionInfo3.IsTriangularPortal &&
                                            !connectionInfo4.IsTriangularPortal
                                            ||
                                            !connectionInfo1.IsTriangularPortal &&
                                            connectionInfo2.TraversableType == Room.RoomConnectionType.TriangularPortalXpZn &&
                                            !connectionInfo3.IsTriangularPortal &&
                                            !connectionInfo4.IsTriangularPortal
                                            ||
                                            !connectionInfo1.IsTriangularPortal &&
                                            !connectionInfo2.IsTriangularPortal &&
                                            connectionInfo3.TraversableType == Room.RoomConnectionType.TriangularPortalXnZp &&
                                            !connectionInfo4.IsTriangularPortal
                                            ||
                                            !connectionInfo1.IsTriangularPortal &&
                                            !connectionInfo2.IsTriangularPortal &&
                                            !connectionInfo3.IsTriangularPortal &&
                                            connectionInfo4.TraversableType == Room.RoomConnectionType.TriangularPortalXpZp;

                    var pos = new VectorInt3((int)trVertex.Position.X, (int)trVertex.Position.Y, (int)trVertex.Position.Z);

                    // Preemptively disable movement for all portal faces
                    if (portal.PositionOnPortalFast(pos, false, false, _roomsMinFloorMaxCeilingCache[portal.Room].X, _roomsMinFloorMaxCeilingCache[portal.Room].Y) || 
                        portal.PositionOnPortalFast(pos, true, false, _roomsMinFloorMaxCeilingCache[portal.Room].X, _roomsMinFloorMaxCeilingCache[portal.Room].Y))
                        trVertex.Locked = true;

                    // A bit complex but working code for water surface movement.
                    // Works better than winroomedit as it takes adjacent portals into account.
                    if ((waterPortals.Contains(portal) && !portal.PositionOnPortalFast(pos, false, true, _roomsMinFloorMaxCeilingCache[portal.Room].X, _roomsMinFloorMaxCeilingCache[portal.Room].Y)))
                    {
                        // A candidate vertex must belong to portal sectors, non triangular, not wall, not solid floor
                        if ((isTraversablePortal || isOppositeCorner) &&
                            connectionInfo1.AnyType != Room.RoomConnectionType.NoPortal &&
                            !room.Sectors[xv, zv].IsAnyWall &&
                            connectionInfo2.AnyType != Room.RoomConnectionType.NoPortal &&
                            !room.Sectors[xv - 1, zv].IsAnyWall &&
                            connectionInfo3.AnyType != Room.RoomConnectionType.NoPortal &&
                            !room.Sectors[xv, zv - 1].IsAnyWall &&
                            connectionInfo4.AnyType != Room.RoomConnectionType.NoPortal &&
                            !room.Sectors[xv - 1, zv - 1].IsAnyWall)
                        {
                            trVertex.Locked = false;
                            trVertex = trVertex.SetEffects(room, RoomLightEffect.Movement);
                        }
                    }

                    if (lightEffect == RoomLightEffect.Mist && portal.Direction == PortalDirection.Floor && isTraversablePortal)
                    {
                        // Assign mist, if set, for vertices inside portal
                        if (portal.PositionOnPortalFast(pos, true, false, _roomsMinFloorMaxCeilingCache[portal.Room].X, _roomsMinFloorMaxCeilingCache[portal.Room].Y))
                        {
                            trVertex = trVertex.SetEffects(room, RoomLightEffect.Glow);
                            break;
                        }
                    }
                    else if (lightEffect == RoomLightEffect.Reflection && portal.Direction == PortalDirection.Floor &&
                        ((room.Properties.Type == RoomType.Water || room.Properties.Type == RoomType.Quicksand) != (portal.AdjoiningRoom.Properties.Type == RoomType.Water || portal.AdjoiningRoom.Properties.Type == RoomType.Quicksand)))
                    {
                        // Assign reflection, if set, for all enclosed portal faces
                        if (portal.PositionOnPortalFast(pos, false, false, _roomsMinFloorMaxCeilingCache[portal.Room].X, _roomsMinFloorMaxCeilingCache[portal.Room].Y) ||
                            portal.PositionOnPortalFast(pos, true, false, _roomsMinFloorMaxCeilingCache[portal.Room].X, _roomsMinFloorMaxCeilingCache[portal.Room].Y))
                        {
                            trVertex = trVertex.SetEffects(room, RoomLightEffect.Glow);
                            break;
                        }
                    }

                    if (lightEffect == RoomLightEffect.Glow || lightEffect == RoomLightEffect.GlowAndMovement)
                    {
                        if (portal.PositionOnPortalFast(pos, false, false, _roomsMinFloorMaxCeilingCache[portal.Room].X, _roomsMinFloorMaxCeilingCache[portal.Room].Y) || 
                            portal.PositionOnPortalFast(pos, true, false, _roomsMinFloorMaxCeilingCache[portal.Room].X, _roomsMinFloorMaxCeilingCache[portal.Room].Y))
                        {
                            // Disable glow for portal faces, if room light interp mode is not sharp-cut
                            if (interpMode != RoomLightInterpolationMode.NoInterpolate)
                                allowGlow = false;

                            // Disable effect if adjoining room effect strength is different
                            if ((otherRoomLightEffect == RoomLightEffect.Glow || otherRoomLightEffect == RoomLightEffect.GlowAndMovement) &&
                                portal.AdjoiningRoom.Properties.LightEffectStrength != room.Properties.LightEffectStrength)
                                allowGlow = false;
                        }
                    }
                }

                if (lightEffect == RoomLightEffect.Movement || lightEffect == RoomLightEffect.GlowAndMovement)
                    trVertex = trVertex.SetEffects(room, RoomLightEffect.Movement);
                if (allowGlow && (lightEffect == RoomLightEffect.Glow || lightEffect == RoomLightEffect.GlowAndMovement))
                    trVertex = trVertex.SetEffects(room, RoomLightEffect.Glow);

                newRoom.Vertices[i] = trVertex;
            }

            // Build portals

            var tempIdPortals = new List<PortalInstance>();

            for (var z = 0; z < room.NumZSectors; z++)
            {
                for (var x = 0; x < room.NumXSectors; x++)
                {
                    if (room.Sectors[x, z].WallPortal != null && !tempIdPortals.Contains(room.Sectors[x, z].WallPortal))
                        tempIdPortals.Add(room.Sectors[x, z].WallPortal);

                    if (room.Sectors[x, z].FloorPortal != null &&
                        !tempIdPortals.Contains(room.Sectors[x, z].FloorPortal))
                        tempIdPortals.Add(room.Sectors[x, z].FloorPortal);

                    if (room.Sectors[x, z].CeilingPortal != null &&
                        !tempIdPortals.Contains(room.Sectors[x, z].CeilingPortal))
                        tempIdPortals.Add(room.Sectors[x, z].CeilingPortal);
                }
            }

            ConvertPortals(room, tempIdPortals, newRoom);
            ConvertSectors(room, newRoom);

            foreach (var instance in room.Objects.OfType<StaticInstance>())
            {
                var sm = _level.Settings?.WadTryGetStatic(instance.WadObjectId);
                newRoom.StaticMeshes.Add(new TombEngineRoomStaticMesh
                {
                    X = (int)Math.Round(newRoom.Info.X + instance.Position.X),
                    Y = (int)-Math.Round(room.WorldPos.Y + instance.Position.Y),
                    Z = (int)Math.Round(newRoom.Info.Z + instance.Position.Z),
                    Yaw = ToTrAngle(instance.RotationY),
                    Scale = instance.Scale,
                    ObjectID = checked((ushort)instance.WadObjectId.TypeId),
                    Flags = (ushort)(0x0007), // FIXME: later let user choose if solid (0x0007) or soft (0x0005)!
                    Color = new Vector4(instance.Color.X, instance.Color.Y, instance.Color.Z, 1.0f),
                    HitPoints = 0,
                    LuaName = instance.LuaName ?? string.Empty
                }) ;
            }

            ConvertLights(room, newRoom);

            return newRoom;
        }

        private static int GetOrAddVertex(Room room, Dictionary<int, int> roomVerticesDictionary, List<TombEngineVertex> roomVertices,
            Vector3 Position, Vector3 color, int index)
        {
            var trVertex = new TombEngineVertex();

            trVertex.Position = new Vector3(Position.X, -(Position.Y + room.WorldPos.Y), Position.Z);
            trVertex.Color = color;
            trVertex.IsOnPortal = false;
            trVertex.IndexInPoly = index;

            int vertexIndex;
            if (roomVerticesDictionary.TryGetValue(trVertex.GetHashCode(), out vertexIndex))
                return vertexIndex;

            vertexIndex = roomVertices.Count;
            roomVertices.Add(trVertex);
            roomVerticesDictionary.Add(trVertex.GetHashCode(), vertexIndex);
            return vertexIndex;
        }

        private void ConvertLights(Room room, TombEngineRoom newRoom)
        {
            int lightCount = 0;

            foreach (var light in room.Objects.OfType<LightInstance>())
            {
                if (!light.Enabled || !light.IsDynamicallyUsed)
                    continue;

                if (light.Intensity == 0 || light.Color.X == 0 && light.Color.Y == 0 && light.Color.Z == 0)
                    continue;

                lightCount += 1;

                var newLight = new TombEngineRoomLight
                {
                    Position = new VectorInt3(
                        (int)Math.Round(newRoom.Info.X + light.Position.X),
                        (int)-Math.Round(light.Position.Y + room.WorldPos.Y),
                        (int)Math.Round(newRoom.Info.Z + light.Position.Z)),
                    Color = light.Color,
                    Intensity = light.Intensity
                };

                switch (light.Type)
                {
                    case LightType.Point:
                        newLight.LightType = 1;
                        newLight.In  = light.InnerRange * Level.SectorSizeUnit;
                        newLight.Out = light.OuterRange * Level.SectorSizeUnit;
                        newLight.CastDynamicShadows = light.CastDynamicShadows;
                        break;
                    case LightType.Shadow:
                        newLight.LightType = 3;
                        newLight.In  = light.InnerRange * Level.SectorSizeUnit;
                        newLight.Out = light.OuterRange * Level.SectorSizeUnit;
                        break;
                    case LightType.Spot:
                        newLight.LightType = 2;
                        newLight.In = light.InnerAngle;
                        newLight.Out = light.OuterAngle;
                        newLight.Length = light.InnerRange * Level.SectorSizeUnit;
                        newLight.CutOff = light.OuterRange * Level.SectorSizeUnit;
                        Vector3 spotDirection = light.GetDirection();
                        newLight.Direction.X = spotDirection.X;
                        newLight.Direction.Y = -spotDirection.Y;
                        newLight.Direction.Z = spotDirection.Z;
                        newLight.CastDynamicShadows = light.CastDynamicShadows;
                        break;
                    case LightType.Sun:
                        newLight.LightType = 0;
                        newLight.In = 0;
                        newLight.Out = 0;
                        newLight.Length = 0;
                        newLight.CutOff = 0;
                        Vector3 sunDirection = light.GetDirection();
                        newLight.Direction.X = sunDirection.X;
                        newLight.Direction.Y = -sunDirection.Y;
                        newLight.Direction.Z = sunDirection.Z;
                        newLight.CastDynamicShadows = light.CastDynamicShadows;
                        break;
                    case LightType.FogBulb:
                        newLight.LightType = 4;
                        newLight.In  = light.InnerRange * Level.SectorSizeUnit;
                        newLight.Out = light.OuterRange * Level.SectorSizeUnit;
                        break;
                    case LightType.Effect:
                        continue;
                    default:
                        throw new Exception("Unknown light type '" + light.Type + "' encountered.");
                }

                newRoom.Lights.Add(newLight);
            }
        }

        private void ConvertSectors(Room room, TombEngineRoom newRoom)
        {
            newRoom.Sectors = new TombEngineRoomSector[room.NumXSectors * room.NumZSectors];
            newRoom.AuxSectors = new TrSectorAux[room.NumXSectors, room.NumZSectors];

            for (var z = 0; z < room.NumZSectors; z++)
            {
                for (var x = 0; x < room.NumXSectors; x++)
                {
                    var sector = room.Sectors[x, z];
                    var compiledSector = new TombEngineRoomSector();
                    var aux = new TrSectorAux();

                    compiledSector.FloorCollision = new TombEngineCollisionInfo();
                    compiledSector.FloorCollision.Portals = new int[2] {-1, -1};
                    compiledSector.FloorCollision.Planes = new Vector3[2];
                    compiledSector.CeilingCollision = new TombEngineCollisionInfo();
                    compiledSector.CeilingCollision.Portals = new int[2] {-1, -1};
                    compiledSector.CeilingCollision.Planes = new Vector3[2];
                    compiledSector.WallPortal = -1;
                    compiledSector.StepSound = (int)GetTextureSound(room, x, z);
                    compiledSector.BoxIndex = -1;
                    compiledSector.TriggerIndex = -1;

                    compiledSector.Flags = new TombEngineSectorFlags()
                    {
                        ClimbEast     = (sector.Flags & SectorFlags.ClimbPositiveX) != 0,
                        ClimbNorth    = (sector.Flags & SectorFlags.ClimbPositiveZ) != 0,
                        ClimbSouth    = (sector.Flags & SectorFlags.ClimbNegativeZ) != 0,
                        ClimbWest     = (sector.Flags & SectorFlags.ClimbNegativeX) != 0,
                        Death         = (sector.Flags & SectorFlags.DeathFire) != 0,
                        MarkBeetle    = (sector.Flags & SectorFlags.Beetle) != 0,
                        Monkeyswing   = (sector.Flags & SectorFlags.Monkey) != 0,
                        MarkTriggerer = (sector.Flags & SectorFlags.TriggerTriggerer) != 0
                    };

                    // Setup portals
                    if (room.GetFloorRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType != Room.RoomConnectionType.NoPortal)
                    {
                        aux.Portal = true;
                        aux.FloorPortal = sector.FloorPortal;
                    }
                    else
                    {
                        aux.FloorPortal = null;
                    }

                    if (sector.WallPortal != null && sector.WallPortal.Opacity != PortalOpacity.SolidFaces)
                        aux.WallPortal = sector.WallPortal.AdjoiningRoom;
                    else
                        aux.WallPortal = null;

                    // TODO: We are using clicks here, but should we really? Please review.
                    SectorSurface floor = sector.Floor.WorldToClicks();

                    // Setup some flags for box generation
                    if (sector.Type == SectorType.BorderWall)
                        aux.BorderWall = true;
                    if ((sector.Flags & SectorFlags.Monkey) != 0)
                        aux.Monkey = true;
                    if ((sector.Flags & SectorFlags.Box) != 0)
                        aux.Box = true;
                    if ((sector.Flags & SectorFlags.NotWalkableFloor) != 0)
                        aux.NotWalkableFloor = true;
                    if (room.Properties.Type != RoomType.Water && (Math.Abs(floor.IfQuadSlopeX) == 1 ||
                                                        Math.Abs(floor.IfQuadSlopeX) == 2 ||
                                                        Math.Abs(floor.IfQuadSlopeZ) == 1 ||
                                                        Math.Abs(floor.IfQuadSlopeZ) == 2))
                        aux.SoftSlope = true;
                    if (room.Properties.Type != RoomType.Water && (Math.Abs(floor.IfQuadSlopeX) > 2 || Math.Abs(floor.IfQuadSlopeZ) > 2))
                        aux.HardSlope = true;
                    if (sector.Type == SectorType.Wall)
                        aux.Wall = true;

                    // TODO: Is this LowestFloor field ever even used in TEN? Consider removing.
                    aux.LowestFloor = (sbyte)(-Clicks.FromWorld(room.Position.Y) - floor.Min);
                    var q0 = floor.XnZp;
                    var q1 = floor.XpZp;
                    var q2 = floor.XpZn;
                    var q3 = floor.XnZn;

                    if (!SectorSurface.IsQuad2(q0, q1, q2, q3) && floor.IfQuadSlopeX == 0 &&
                        floor.IfQuadSlopeZ == 0)
                    {
                        if (!floor.SplitDirectionIsXEqualsZ)
                            aux.LowestFloor = (sbyte)(-Clicks.FromWorld(room.Position.Y) - Math.Min(floor.XnZp, floor.XpZn));
                        else
                            aux.LowestFloor = (sbyte)(-Clicks.FromWorld(room.Position.Y) - Math.Min(floor.XpZp, floor.XnZn));
                    }

                    newRoom.AuxSectors[x, z] = aux;
                    newRoom.Sectors[room.NumZSectors * x + z] = compiledSector;
                }
            }
        }

        private void ConvertPortals(Room room, IEnumerable<PortalInstance> portals, TombEngineRoom newRoom)
        {
            foreach (var portal in portals)
            {
                switch (portal.Direction)
                {
                    case PortalDirection.WallNegativeZ:
                        ConvertWallPortal(room, portal, newRoom.Portals, new SectorEdge[] { SectorEdge.XnZn, SectorEdge.XpZn },
                            new SectorEdge[] { SectorEdge.XnZp, SectorEdge.XpZp });
                        break;
                    case PortalDirection.WallNegativeX:
                        ConvertWallPortal(room, portal, newRoom.Portals, new SectorEdge[] { SectorEdge.XnZn, SectorEdge.XnZp },
                             new SectorEdge[] { SectorEdge.XpZn, SectorEdge.XpZp });
                        break;
                    case PortalDirection.WallPositiveZ:
                        ConvertWallPortal(room, portal, newRoom.Portals, new SectorEdge[] { SectorEdge.XpZp, SectorEdge.XnZp },
                            new SectorEdge[] { SectorEdge.XpZn, SectorEdge.XnZn });
                        break;
                    case PortalDirection.WallPositiveX:
                        ConvertWallPortal(room, portal, newRoom.Portals, new SectorEdge[] { SectorEdge.XpZp, SectorEdge.XpZn },
                            new SectorEdge[] { SectorEdge.XnZp, SectorEdge.XnZn });
                        break;
                    case PortalDirection.Floor:
                        ConvertFloorCeilingPortal(room, portal, newRoom.Portals, false);
                        break;
                    case PortalDirection.Ceiling:
                        ConvertFloorCeilingPortal(room, portal, newRoom.Portals, true);
                        break;
                    default:
                        throw new ApplicationException("Unknown PortalDirection");
                }
            }

            MergePortals(newRoom);
        }

        private void ConvertWallPortal(Room room, PortalInstance portal, List<TombEnginePortal> outPortals, SectorEdge[] relevantEdges, SectorEdge[] oppositeRelevantEdges)
        {
            // Calculate dimensions of portal
            var yMin = float.MaxValue;
            var yMax = float.MinValue;

            var startX = 0;
            var endX = 0;
            var startZ = 0;
            var endZ = 0;

            var oppositeStartX = 0;
            var oppositeEndX = 0;
            var oppositeStartZ = 0;
            var oppositeEndZ = 0;

            switch (portal.Direction)
            {
                case PortalDirection.WallNegativeX:
                    startX = 1;
                    endX = 1;
                    startZ = Math.Min(portal.Area.Y0, portal.Area.Y1);
                    endZ = Math.Max(portal.Area.Y0, portal.Area.Y1);

                    oppositeStartX = portal.AdjoiningRoom.NumXSectors - 2;
                    oppositeEndX = portal.AdjoiningRoom.NumXSectors - 2;
                    oppositeStartZ = Math.Min(portal.Area.Y0, portal.Area.Y1) +
                        portal.Room.Position.Z - portal.AdjoiningRoom.Position.Z;
                    oppositeEndZ = Math.Max(portal.Area.Y0, portal.Area.Y1) +
                        portal.Room.Position.Z - portal.AdjoiningRoom.Position.Z;

                    break;

                case PortalDirection.WallPositiveX:
                    startX = room.NumXSectors - 2;
                    endX = room.NumXSectors - 2;
                    startZ = Math.Min(portal.Area.Y0, portal.Area.Y1);
                    endZ = Math.Max(portal.Area.Y0, portal.Area.Y1);

                    oppositeStartX = 1;
                    oppositeEndX = 1;
                    oppositeStartZ = Math.Min(portal.Area.Y0, portal.Area.Y1) +
                        portal.Room.Position.Z - portal.AdjoiningRoom.Position.Z;
                    oppositeEndZ = Math.Max(portal.Area.Y0, portal.Area.Y1) +
                        portal.Room.Position.Z - portal.AdjoiningRoom.Position.Z;

                    break;

                case PortalDirection.WallNegativeZ:
                    startX = Math.Min(portal.Area.X0, portal.Area.X1);
                    endX = Math.Max(portal.Area.X0, portal.Area.X1);
                    startZ = 1;
                    endZ = 1;

                    oppositeStartX = Math.Min(portal.Area.X0, portal.Area.X1) +
                        portal.Room.Position.X - portal.AdjoiningRoom.Position.X;
                    oppositeEndX = Math.Max(portal.Area.X0, portal.Area.X1) +
                        portal.Room.Position.X - portal.AdjoiningRoom.Position.X;
                    oppositeStartZ = portal.AdjoiningRoom.NumZSectors - 2;
                    oppositeEndZ = portal.AdjoiningRoom.NumZSectors - 2;

                    break;

                case PortalDirection.WallPositiveZ:
                    startX = Math.Min(portal.Area.X0, portal.Area.X1);
                    endX = Math.Max(portal.Area.X0, portal.Area.X1);
                    startZ = room.NumZSectors - 2;
                    endZ = room.NumZSectors - 2;

                    oppositeStartX = Math.Min(portal.Area.X0, portal.Area.X1) +
                       portal.Room.Position.X - portal.AdjoiningRoom.Position.X;
                    oppositeEndX = Math.Max(portal.Area.X0, portal.Area.X1) +
                        portal.Room.Position.X - portal.AdjoiningRoom.Position.X;
                    oppositeStartZ = 1;
                    oppositeEndZ = 1;

                    break;
            }

            for (var z = 0; z <= endZ - startZ; ++z)
                for (var x = 0; x <= endX - startX; ++x)
                {
                    Sector sector = room.Sectors[x + startX, z + startZ];
                    Sector oppositeSector = portal.AdjoiningRoom.Sectors[x + oppositeStartX, z + oppositeStartZ];

                    for (int i = 0; i < relevantEdges.Length; i++)
                    {
                        // We need to check both the current sector and the opposite sector and choose
                        // the largest area possible for avoiding strange rendering bugs
                        var relevantDirection = relevantEdges[i];
                        var oppositeRelevantDirection = oppositeRelevantEdges[i];

                        var floor   = sector.Floor.GetHeight(relevantDirection) + room.WorldPos.Y;
                        var ceiling = sector.Ceiling.GetHeight(relevantDirection) + room.WorldPos.Y;

                        var floorOpposite   = oppositeSector.Floor.GetHeight(oppositeRelevantDirection) + portal.AdjoiningRoom.WorldPos.Y;
                        var ceilingOpposite = oppositeSector.Ceiling.GetHeight(oppositeRelevantDirection) + portal.AdjoiningRoom.WorldPos.Y;

                        floor = Math.Min(floor, floorOpposite);
                        ceiling = Math.Max(ceiling, ceilingOpposite);

                        yMin = Math.Min(yMin, Math.Min(floor, ceiling));
                        yMax = Math.Max(yMax, Math.Max(floor, ceiling));
                    }
                }
            yMin = (float)Math.Floor(yMin);
            yMax = (float)Math.Ceiling(yMax);

            var xMin = portal.Area.X0 * Level.SectorSizeUnit;
            var xMax = (portal.Area.X1 + 1) * Level.SectorSizeUnit;
            var zMin = portal.Area.Y0 * Level.SectorSizeUnit;
            var zMax = (portal.Area.Y1 + 1) * Level.SectorSizeUnit;

            // Determine normal and portal vertices
            VectorInt3[] portalVertices = new VectorInt3[4];
            VectorInt3 normal;
            switch (portal.Direction)
            {
                case PortalDirection.WallPositiveZ:
                    normal = new VectorInt3(0, 0, -1);
                    portalVertices[0] = new VectorInt3((int)xMin, (int)-yMax, (int)(zMax - Level.SectorSizeUnit));
                    portalVertices[1] = new VectorInt3((int)xMax, (int)-yMax, (int)(zMax - Level.SectorSizeUnit));
                    portalVertices[2] = new VectorInt3((int)xMax, (int)-yMin, (int)(zMax - Level.SectorSizeUnit));
                    portalVertices[3] = new VectorInt3((int)xMin, (int)-yMin, (int)(zMax - Level.SectorSizeUnit));
                    break;
                case PortalDirection.WallPositiveX:
                    normal = new VectorInt3(-1, 0, 0);
                    portalVertices[0] = new VectorInt3((int)(xMax - Level.SectorSizeUnit), (int)-yMin, (int)zMax);
                    portalVertices[1] = new VectorInt3((int)(xMax - Level.SectorSizeUnit), (int)-yMax, (int)zMax);
                    portalVertices[2] = new VectorInt3((int)(xMax - Level.SectorSizeUnit), (int)-yMax, (int)zMin);
                    portalVertices[3] = new VectorInt3((int)(xMax - Level.SectorSizeUnit), (int)-yMin, (int)zMin);
                    break;
                case PortalDirection.WallNegativeZ:
                    normal = new VectorInt3(0, 0, 1);
                    portalVertices[0] = new VectorInt3((int)xMax, (int)-yMax, (int)(zMin + Level.SectorSizeUnit - 1));
                    portalVertices[1] = new VectorInt3((int)xMin, (int)-yMax, (int)(zMin + Level.SectorSizeUnit - 1));
                    portalVertices[2] = new VectorInt3((int)xMin, (int)-yMin, (int)(zMin + Level.SectorSizeUnit - 1));
                    portalVertices[3] = new VectorInt3((int)xMax, (int)-yMin, (int)(zMin + Level.SectorSizeUnit - 1));
                    break;
                case PortalDirection.WallNegativeX:
                    normal = new VectorInt3(1, 0, 0);
                    portalVertices[0] = new VectorInt3((int)(xMin + Level.SectorSizeUnit - 1), (int)-yMin, (int)zMin);
                    portalVertices[1] = new VectorInt3((int)(xMin + Level.SectorSizeUnit - 1), (int)-yMax, (int)zMin);
                    portalVertices[2] = new VectorInt3((int)(xMin + Level.SectorSizeUnit - 1), (int)-yMax, (int)zMax);
                    portalVertices[3] = new VectorInt3((int)(xMin + Level.SectorSizeUnit - 1), (int)-yMin, (int)zMax);
                    break;
                default:
                    throw new ApplicationException("Unknown PortalDirection");
            }

            // Create portal
            var portalToAdd = new TombEnginePortal
            {
                AdjoiningRoom = (ushort)_roomRemapping[portal.AdjoiningRoom],
                Vertices = portalVertices,
                Normal = normal,
                Direction = portal.Direction
            };

            _portalRemapping.TryAdd(portalToAdd, portal);
            outPortals.Add(portalToAdd);

            if (portal.Effect == PortalEffectType.ClassicMirror)
            {
                var room2DPosition = new Vector3(
                    room.Position.X * Level.SectorSizeUnit, 0, room.Position.Z * Level.SectorSizeUnit);
                
                var mirror = new TombEngineMirror();
                mirror.Room = (short)_roomRemapping[room];

				mirror.Plane.X = normal.X;
                mirror.Plane.Y = normal.Y;
                mirror.Plane.Z = normal.Z;
                mirror.Plane.W = -(
                    normal.X * (portalVertices[0].X + room2DPosition.X) +
                    normal.Y * (portalVertices[0].Y) +
                    normal.Z * (portalVertices[0].Z + room2DPosition.Z));

                mirror.ReflectLara = portal.Properties.ReflectLara;
                mirror.ReflectMoveables = portal.Properties.ReflectMoveables;
                mirror.ReflectStatics = portal.Properties.ReflectStatics;
                mirror.ReflectSprites = portal.Properties.ReflectSprites;
                mirror.ReflectLights = portal.Properties.ReflectLights;

				if (!_mirrors.Any(m => m.Room == mirror.Room && m.Plane == mirror.Plane))
                {
                    _mirrors.Add(mirror);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct PortalPlane
        {
            public readonly int SlopeX;
            public readonly int SlopeZ;
            public readonly int Height;

            public int EvaluateHeight(int x, int z)
            {
                return Height + x * SlopeX + z * SlopeZ;
            }

            public PortalPlane(int ankerX, int ankerY, int ankerZ, int slopeX, int slopeZ)
            {
                SlopeX = slopeX;
                SlopeZ = slopeZ;
                Height = ankerY - ankerX * slopeX - ankerZ * slopeZ;
            }

            public static unsafe bool FastEquals(PortalPlane first, PortalPlane second)
            {
                return *(ulong*)&first == *(ulong*)&second;
            }
        }

        private void AddPortalPlane(List<PortalPlane> portalPlanes, List<RectangleInt2> portalAreas, int x, int z, PortalPlane portalPlane)
        {
            // Try to extend an existing portal plane
            for (int i = 0; i < portalPlanes.Count; ++i)
                if (PortalPlane.FastEquals(portalPlanes[i], portalPlane))
                {
                    var area = portalAreas[i];
                    area.X0 = Math.Min(area.X0, x);
                    area.X1 = Math.Max(area.X1, x);
                    area.Y0 = Math.Min(area.Y0, z);
                    area.Y1 = Math.Max(area.Y1, z);
                    portalAreas[i] = area;
                    return;
                }

            // Add new portal plane
            portalPlanes.Add(portalPlane);
            portalAreas.Add(new RectangleInt2(x, z, x, z));
        }

        private void MergePortals(TombEngineRoom room)
        {
            List<TombEnginePortal> mergedPortals = new List<TombEnginePortal>();

            foreach (TombEnginePortal portalToMerge in room.Portals)
            {
                bool found = false;

                foreach (TombEnginePortal mergedPortal in mergedPortals)
                {
                    if (portalToMerge.AdjoiningRoom == mergedPortal.AdjoiningRoom &&
                        portalToMerge.Direction == mergedPortal.Direction)
                    {
                        int xMin = Math.Min(portalToMerge.Vertices.Min(v => v.X), mergedPortal.Vertices.Min(v => v.X));
                        int yTop = Math.Min(portalToMerge.Vertices.Min(v => v.Y), mergedPortal.Vertices.Min(v => v.Y));
                        int zMin = Math.Min(portalToMerge.Vertices.Min(v => v.Z), mergedPortal.Vertices.Min(v => v.Z));
                        int xMax = Math.Max(portalToMerge.Vertices.Max(v => v.X), mergedPortal.Vertices.Max(v => v.X));
                        int yBottom = Math.Max(portalToMerge.Vertices.Max(v => v.Y), mergedPortal.Vertices.Max(v => v.Y));
                        int zMax = Math.Max(portalToMerge.Vertices.Max(v => v.Z), mergedPortal.Vertices.Max(v => v.Z));

                        found = true;

                        switch (portalToMerge.Direction)
                        {
                            case PortalDirection.WallPositiveX:
                                mergedPortal.Vertices[0] = new VectorInt3(xMax, yTop, zMax);
                                mergedPortal.Vertices[1] = new VectorInt3(xMax, yTop, zMin);
                                mergedPortal.Vertices[2] = new VectorInt3(xMax, yBottom, zMin);
                                mergedPortal.Vertices[3] = new VectorInt3(xMax, yBottom, zMax);
                                break;

                            case PortalDirection.WallNegativeX:
                                mergedPortal.Vertices[0] = new VectorInt3(xMin, yTop, zMin);
                                mergedPortal.Vertices[1] = new VectorInt3(xMin, yTop, zMax);
                                mergedPortal.Vertices[2] = new VectorInt3(xMin, yBottom, zMax);
                                mergedPortal.Vertices[3] = new VectorInt3(xMin, yBottom, zMin);
                                break;

                            case PortalDirection.WallPositiveZ:
                                mergedPortal.Vertices[0] = new VectorInt3(xMin, yTop, zMax);
                                mergedPortal.Vertices[1] = new VectorInt3(xMax, yTop, zMax);
                                mergedPortal.Vertices[2] = new VectorInt3(xMax, yBottom, zMax);
                                mergedPortal.Vertices[3] = new VectorInt3(xMin, yBottom, zMax);
                                break;

                            case PortalDirection.WallNegativeZ:
                                mergedPortal.Vertices[0] = new VectorInt3(xMax, yTop, zMin);
                                mergedPortal.Vertices[1] = new VectorInt3(xMin, yTop, zMin);
                                mergedPortal.Vertices[2] = new VectorInt3(xMin, yBottom, zMin);
                                mergedPortal.Vertices[3] = new VectorInt3(xMax, yBottom, zMin);
                                break;

                            case PortalDirection.Floor:
                                mergedPortal.Vertices[0] = new VectorInt3(xMax, yBottom, zMax);
                                mergedPortal.Vertices[1] = new VectorInt3(xMin, yBottom, zMax);
                                mergedPortal.Vertices[2] = new VectorInt3(xMin, yBottom, zMin);
                                mergedPortal.Vertices[3] = new VectorInt3(xMax, yBottom, zMin);
                                break;

                            case PortalDirection.Ceiling:
                                mergedPortal.Vertices[0] = new VectorInt3(xMax, yTop, zMin);
                                mergedPortal.Vertices[1] = new VectorInt3(xMin, yTop, zMin);
                                mergedPortal.Vertices[2] = new VectorInt3(xMin, yTop, zMax);
                                mergedPortal.Vertices[3] = new VectorInt3(xMax, yTop, zMax);
                                break;
                        }
                    }
                }

                if (!found)
                {
                    mergedPortals.Add(portalToMerge);
                }
            }

            room.Portals = mergedPortals;
        }

        private void ConvertFloorCeilingPortal(Room room, PortalInstance portal, List<TombEnginePortal> outPortals, bool isCeiling)
        {
            // Construct planes that contain all portal sectors
            List<PortalPlane> portalPlanes = new List<PortalPlane>();
            List<RectangleInt2> portalAreas = new List<RectangleInt2>();
            for (int z = portal.Area.Y0; z <= portal.Area.Y1; ++z)
                for (int x = portal.Area.X0; x <= portal.Area.X1; ++x)
                {
                    Sector sector = room.Sectors[x, z];
                    Room.RoomConnectionInfo roomConnectionInfo = isCeiling ?
                        room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z), false) :
                        room.GetFloorRoomConnectionInfo(new VectorInt2(x, z), false);

                    if (roomConnectionInfo.AnyType != Room.RoomConnectionType.NoPortal)
                    {
                        SectorSurface s = isCeiling ? sector.Ceiling : sector.Floor;

                        if (s.DiagonalSplit != DiagonalSplit.None || SectorSurface.IsQuad2(s.XnZn, s.XpZn, s.XnZp, s.XpZp))
                        {
                            switch (s.DiagonalSplit)
                            {
                                // Ordinary quad
                                case DiagonalSplit.None:
                                    AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XnZn, z, s.XpZn - s.XnZn, s.XnZp - s.XnZn));
                                    break;

                                case DiagonalSplit.XnZn:
                                    AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XpZp, z, 0, 0));
                                    break;

                                case DiagonalSplit.XnZp:
                                    AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XpZn, z, 0, 0));
                                    break;

                                case DiagonalSplit.XpZn:
                                    AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XnZp, z, 0, 0));
                                    break;

                                case DiagonalSplit.XpZp:
                                    AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XnZn, z, 0, 0));
                                    break;
                            }
                        }
                        else if (s.SplitDirectionIsXEqualsZ)
                        { // Diagonal is split X = Y
                            if (roomConnectionInfo.AnyType == Room.RoomConnectionType.FullPortal || roomConnectionInfo.AnyType == Room.RoomConnectionType.TriangularPortalXnZp)
                                AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XnZn, z, s.XpZp - s.XnZp, s.XnZp - s.XnZn));
                            if (roomConnectionInfo.AnyType == Room.RoomConnectionType.FullPortal || roomConnectionInfo.AnyType == Room.RoomConnectionType.TriangularPortalXpZn)
                                AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XnZn, z, s.XpZn - s.XnZn, s.XpZp - s.XpZn));
                        }
                        else
                        { // Diagonal is split X = -Y
                            if (roomConnectionInfo.AnyType == Room.RoomConnectionType.FullPortal || roomConnectionInfo.AnyType == Room.RoomConnectionType.TriangularPortalXnZn)
                                AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x, s.XnZn, z, s.XpZn - s.XnZn, s.XnZp - s.XnZn));
                            if (roomConnectionInfo.AnyType == Room.RoomConnectionType.FullPortal || roomConnectionInfo.AnyType == Room.RoomConnectionType.TriangularPortalXpZp)
                                AddPortalPlane(portalPlanes, portalAreas, x, z, new PortalPlane(x + 1, s.XpZp, z + 1, s.XpZp - s.XnZp, s.XpZp - s.XpZn));
                        }
                    }
                }

            // Add portals for all planes in the portal
            for (int i = 0; i < portalPlanes.Count; ++i)
            {
                PortalPlane portalPlane = portalPlanes[i];
                RectangleInt2 portalArea = portalAreas[i];

                float xMin = portalArea.X0 * Level.SectorSizeUnit;
                float xMax = (portalArea.X1 + 1) * Level.SectorSizeUnit;
                float zMin = portalArea.Y0 * Level.SectorSizeUnit;
                float zMax = (portalArea.Y1 + 1) * Level.SectorSizeUnit;

                float yAtXMinZMin = room.Position.Y + portalPlane.EvaluateHeight(portalArea.X0, portalArea.Y0);
                float yAtXMaxZMin = room.Position.Y + portalPlane.EvaluateHeight(portalArea.X1 + 1, portalArea.Y0);
                float yAtXMinZMax = room.Position.Y + portalPlane.EvaluateHeight(portalArea.X0, portalArea.Y1 + 1);
                float yAtXMaxZMax = room.Position.Y + portalPlane.EvaluateHeight(portalArea.X1 + 1, portalArea.Y1 + 1);

                // Choose portal coordinates
                VectorInt3[] portalVertices = new VectorInt3[4];
                VectorInt3 normal = new VectorInt3((int)-portalPlane.SlopeX, 4, (int)-portalPlane.SlopeZ);
                if (isCeiling)
                {
                    // TEST: this should solve flickering rooms when camera is on portal
                    Vector3 n = Vector3.UnitY;

                    portalVertices[0] = new VectorInt3((int)xMax, (int)(-yAtXMaxZMin - 1), (int)zMin);
                    portalVertices[1] = new VectorInt3((int)xMin, (int)(-yAtXMinZMin - 1), (int)zMin);
                    portalVertices[2] = new VectorInt3((int)xMin, (int)(-yAtXMinZMax - 1), (int)zMax);
                    portalVertices[3] = new VectorInt3((int)xMax, (int)(-yAtXMaxZMax - 1), (int)zMax);
                }
                else
                {
                    normal = new VectorInt3((int)-portalPlane.SlopeX, -4, (int)-portalPlane.SlopeZ);

                    portalVertices[0] = new VectorInt3((int)xMax, (int)(-yAtXMaxZMax), (int)zMax);
                    portalVertices[1] = new VectorInt3((int)xMin, (int)(-yAtXMinZMax), (int)zMax);
                    portalVertices[2] = new VectorInt3((int)xMin, (int)(-yAtXMinZMin), (int)zMin);
                    portalVertices[3] = new VectorInt3((int)xMax, (int)(-yAtXMaxZMin), (int)zMin);
                }

                // Make the normal vector as short as possible
                while (normal.X % 2 == 0 && normal.Y % 2 == 0 && normal.Z % 2 == 0)
                    normal = new VectorInt3((int)(normal.X / 2), (int)(normal.Y / 2), (int)(normal.Z / 2));

                // Add portal
                var portalToAdd = new TombEnginePortal
                {
                    AdjoiningRoom = (ushort)_roomRemapping[portal.AdjoiningRoom],
                    Vertices = portalVertices,
                    Normal = normal,
                    Direction = isCeiling ? PortalDirection.Ceiling : PortalDirection.Floor
                };

                _portalRemapping.TryAdd(portalToAdd, portal);
                outPortals.Add(portalToAdd);

				if (portal.Effect == PortalEffectType.ClassicMirror)
				{
					var mirror = new TombEngineMirror();
					mirror.Room = (short)_roomRemapping[room];

                    mirror.Plane.X = normal.X;
					mirror.Plane.Y = normal.Y;
					mirror.Plane.Z = normal.Z;
                    mirror.Plane.W = -normal.Y * portalVertices[0].Y;

                    mirror.ReflectLara = portal.Properties.ReflectLara;
                    mirror.ReflectMoveables = portal.Properties.ReflectMoveables;
                    mirror.ReflectStatics = portal.Properties.ReflectStatics;
                    mirror.ReflectSprites = portal.Properties.ReflectSprites;
                    mirror.ReflectLights = portal.Properties.ReflectLights;
                    
                    if (!_mirrors.Any(m => m.Room == mirror.Room && m.Plane == mirror.Plane))
					{
						_mirrors.Add(mirror);
					}
				}
			}
        }

        private void MatchDoorShades(List<TombEngineRoom> roomList, TombEngineRoom room, bool grayscale, bool flipped)
        {
            // Do we want to interpolate?
            if (room.OriginalRoom.Properties.LightInterpolationMode == RoomLightInterpolationMode.NoInterpolate)
                return;

            foreach (var p in room.Portals)
            {
                if (_portalRemapping.ContainsKey(p))
                {
                    if (_portalRemapping[p].Opacity == PortalOpacity.SolidFaces &&
                        room.OriginalRoom.Properties.LightInterpolationMode != RoomLightInterpolationMode.Interpolate)
                    {
                        continue;
                    }
                }

                var otherRoom = roomList[p.AdjoiningRoom];

                // Here we must decide if match or not, basing on flipped flag.
                // In winroomedit.exe, all flipped rooms were swapped with their counterparts,
                // here instead we'll decide per portal
                if (!flipped)
                {
                    // We allow here normal vs normal or base room vs base room
                    if (room.AlternateKind == AlternateKind.AlternateRoom || otherRoom.AlternateKind == AlternateKind.AlternateRoom)
                        continue;
                }
                else
                {
                    // We allow here flipped vs flipped, flipped vs normal and flipped vs base room.
                    // NOTICE: We only take other flipped room into account if it belongs to SAME ALTERNATE GROUP.
                    if (otherRoom.AlternateKind == AlternateKind.BaseRoom && roomList[otherRoom.AlternateRoom].AlternateGroup == room.AlternateGroup)
                        otherRoom = roomList[otherRoom.AlternateRoom];

                    // Don't proceed if current room isn't alternate or if alternate group don't match (paranoidal foolproof in case previous check failed).
                    if (room.AlternateKind != AlternateKind.AlternateRoom || (otherRoom.AlternateKind == AlternateKind.AlternateRoom && room.AlternateGroup != otherRoom.AlternateGroup))
                        continue;
                }

                // Check if the other room must be interpolated
                if (otherRoom.OriginalRoom.Properties.LightInterpolationMode == RoomLightInterpolationMode.NoInterpolate)
                    continue;

                // If we have a pair of water room and dry room, orginal behaviour of TRLE was to not interpolate,
                // but now we have flags
                bool isWaterAndDryPair = ((room.Flags & 1) == 1 ^ (otherRoom.Flags & 1) == 1);

                if (!isWaterAndDryPair || (isWaterAndDryPair &&
                    (room.OriginalRoom.Properties.LightInterpolationMode == RoomLightInterpolationMode.Interpolate ||
                     otherRoom.OriginalRoom.Properties.LightInterpolationMode == RoomLightInterpolationMode.Interpolate)))
                {
                    int x1 = p.Vertices[0].X;
                    int y1 = p.Vertices[0].Y;
                    int z1 = p.Vertices[0].Z;

                    int x2 = x1 + 1;
                    int y2 = y1 + 1;
                    int z2 = z1 + 1;

                    for (int i = 1; i < 4; i++)
                    {
                        if (p.Vertices[i].X < x1)
                            x1 = p.Vertices[i].X;
                        else if (p.Vertices[i].X > x2)
                            x2 = p.Vertices[i].X + 1;

                        if (p.Vertices[i].Y < y1)
                            y1 = p.Vertices[i].Y;
                        else if (p.Vertices[i].Y > y2)
                            y2 = p.Vertices[i].Y + 1;

                        if (p.Vertices[i].Z < z1)
                            z1 = p.Vertices[i].Z;
                        else if (p.Vertices[i].Z > z2)
                            z2 = p.Vertices[i].Z + 1;
                    }

                    for (int i = 0; i < room.Vertices.Count; i++)
                    {
                        var v1 = room.Vertices[i];
                        var sig = new ShadeMatchSignature()
                        {
                            // NOTE: We keep alternate group and water flag in dictionary as well, this way we only apply vertex colour to
                            // appropriate rooms, and not all rooms at once.
                            IsWater = (room.Flags & 1) == 1,
                            AlternateGroup = room.AlternateKind == AlternateKind.AlternateRoom ? room.AlternateGroup : -1,
                            Position = new VectorInt3((int)v1.Position.X + room.Info.X, (int)v1.Position.Y, (int)v1.Position.Z + room.Info.Z)
                        };

                        if (v1.Position.X >= x1 && v1.Position.X <= x2)
                            if (v1.Position.Y >= y1 && v1.Position.Y <= y2)
                                if (v1.Position.Z >= z1 && v1.Position.Z <= z2)
                                {
                                    v1.IsOnPortal = true;
                                    room.Vertices[i] = v1;

                                    int otherX = (int)v1.Position.X + room.Info.X - otherRoom.Info.X;
                                    int otherY = (int)v1.Position.Y;
                                    int otherZ = (int)v1.Position.Z + room.Info.Z - otherRoom.Info.Z;

                                    for (int j = 0; j < otherRoom.Vertices.Count; j++)
                                    {
                                        var v2 = otherRoom.Vertices[j];
                                        Vector3 refColor;
                                        var isPresentInLookup = _vertexColors.TryGetValue(sig, out refColor);
                                        if (!isPresentInLookup) refColor = v1.Color;

                                        if (room.Info.X + v1.Position.X == otherRoom.Info.X + v2.Position.X &&
                                            v1.Position.Y == v2.Position.Y &&
                                            room.Info.Z + v1.Position.Z == otherRoom.Info.Z + v2.Position.Z)
                                        {
                                            Vector3 newColor;

                                            // NOTE: We DON'T INTERPOLATE colours of both rooms in case we're dealing with alternate room and matched room
                                            // isn't alternate room itself. Instead, we simply copy vertex colour from matched base room.
                                            // This way we don't get sharp-cut half-transitioned vertex colour.

                                            if (flipped && otherRoom.AlternateKind != AlternateKind.AlternateRoom)
                                            {
                                                var baseSig = new ShadeMatchSignature() { IsWater = sig.IsWater, AlternateGroup = -1, Position = sig.Position };
                                                if (!_vertexColors.TryGetValue(baseSig, out newColor)) newColor = v2.Color;
                                            }
                                            else
                                            {
                                                newColor = (v2.Color + refColor) / 2.0f;
                                            }

                                            if (!isPresentInLookup)
                                                _vertexColors.TryAdd(sig, newColor);
                                            else
                                                _vertexColors[sig] = newColor;
                                        }
                                    }
                                }
                    }
                }
            }
        }

        private static void FindConnectedRooms(List<Room> outSharedRooms, Room currentRoom, Vector3 worldPos, bool checkFloorCeiling)
        {
            if (outSharedRooms.Contains(currentRoom))
                return;
            outSharedRooms.Add(currentRoom);

            Vector3 localPos = worldPos - currentRoom.WorldPos;
            int sectorPosX = (int)(localPos.X * (1.0f / Level.SectorSizeUnit) + 0.5f);
            int sectorPosZ = (int)(localPos.Z * (1.0f / Level.SectorSizeUnit) + 0.5f);
            int sectorPosX2 = sectorPosX - 1;
            int sectorPosZ2 = sectorPosZ - 1;

            // Check up to 4 sectors around the destination
            if (sectorPosX >= 0 && sectorPosX < currentRoom.NumXSectors)
            {
                if (sectorPosZ >= 0 && sectorPosZ < currentRoom.NumZSectors)
                    FindConnectedRoomsCheckSector(outSharedRooms, currentRoom, worldPos, checkFloorCeiling, sectorPosX, sectorPosZ);
                if (sectorPosZ2 >= 0 && sectorPosZ2 < currentRoom.NumZSectors)
                    FindConnectedRoomsCheckSector(outSharedRooms, currentRoom, worldPos, checkFloorCeiling, sectorPosX, sectorPosZ2);
            }
            if (sectorPosX2 >= 0 && sectorPosX2 < currentRoom.NumXSectors)
            {
                if (sectorPosZ >= 0 && sectorPosZ < currentRoom.NumZSectors)
                    FindConnectedRoomsCheckSector(outSharedRooms, currentRoom, worldPos, checkFloorCeiling, sectorPosX2, sectorPosZ);
                if (sectorPosZ2 >= 0 && sectorPosZ2 < currentRoom.NumZSectors)
                    FindConnectedRoomsCheckSector(outSharedRooms, currentRoom, worldPos, checkFloorCeiling, sectorPosX2, sectorPosZ2);
            }
        }

        private static void FindConnectedRoomsCheckSector(List<Room> outSharedRooms, Room currentRoom, Vector3 worldPos, bool checkFloorCeiling, int x, int z)
        {
            Sector sector = currentRoom.Sectors[x, z];

            if (checkFloorCeiling)
            {
                if (sector.FloorPortal != null)
                    foreach (var roomPair in Room.GetPossibleAlternateRoomPairs(currentRoom, sector.FloorPortal.AdjoiningRoom))
                    {
                        if (roomPair.Key != currentRoom)
                            FindConnectedRooms(outSharedRooms, roomPair.Key, worldPos, checkFloorCeiling);
                        FindConnectedRooms(outSharedRooms, roomPair.Value, worldPos, false);
                    }
                if (sector.CeilingPortal != null)
                    foreach (var roomPair in Room.GetPossibleAlternateRoomPairs(currentRoom, sector.CeilingPortal.AdjoiningRoom))
                    {
                        if (roomPair.Key != currentRoom)
                            FindConnectedRooms(outSharedRooms, roomPair.Key, worldPos, checkFloorCeiling);
                        FindConnectedRooms(outSharedRooms, roomPair.Value, worldPos, false);
                    }
            }

            if (sector.WallPortal != null)
                foreach (var roomPair in Room.GetPossibleAlternateRoomPairs(currentRoom, sector.WallPortal.AdjoiningRoom))
                {
                    if (roomPair.Key != currentRoom)
                        FindConnectedRooms(outSharedRooms, roomPair.Key, worldPos, checkFloorCeiling);
                    FindConnectedRooms(outSharedRooms, roomPair.Value, worldPos, checkFloorCeiling);
                }
        }

		private TombEngineBucket GetOrAddBucket(int textureId, byte blendMode, int materialIndex, int animatedSequence, Dictionary<TombEngineMaterial, TombEngineBucket> buckets)
		{
			var material = new TombEngineMaterial()
			{
				Texture = textureId,
				BlendMode = blendMode,
				Animated = animatedSequence >= 0,
				AnimatedSequence = animatedSequence,
				MaterialIndex = materialIndex
			};

			if (!buckets.ContainsKey(material))
                buckets.Add(material, new TombEngineBucket { Material = material });

            return buckets[material];
        }

		private void PrepareRoomsBuckets()
        {
            CollectNormalGroups();   

			for (int i = 0; i < _tempRooms.Count; i++)
                PrepareRoomBuckets(_tempRooms.ElementAt(i).Value);

            AverageAndApplyNormals();
		}

		private void PrepareRoomBuckets(TombEngineRoom room)
        {
            // Build buckets and assign texture coordinates
            var textures = _textureInfoManager.GetObjectTextures();
            var buckets = new Dictionary<TombEngineMaterial, TombEngineBucket>(new TombEngineMaterial.TombEngineMaterialComparer());
           
            foreach (var poly in room.Polygons)
            {
                poly.AnimatedSequence = -1;
                poly.AnimatedFrame = -1;

                if (poly.Animated)
                {
                    var animInfo = _textureInfoManager.GetAnimatedTexture(poly.TextureId);
                    if (animInfo != null)
                    {
                        poly.AnimatedSequence = animInfo.Item1;
                        poly.AnimatedFrame = animInfo.Item2;
                        Console.WriteLine(animInfo.Item1);
                    }
                }

                var bucket = GetOrAddBucket(textures[poly.TextureId].AtlasIndex, poly.BlendMode, poly.MaterialIndex, poly.AnimatedSequence, buckets);

                var texture = textures[poly.TextureId];

                if (poly.Shape == TombEnginePolygonShape.Quad)
                {
                    for (int n = 0; n < 4; n++)
                    {
                        poly.TextureCoordinates.Add(texture.TexCoordFloat[n]);
                        poly.Normals.Add(Vector3.Zero);
                        poly.Tangents.Add(Vector3.Zero);
                        poly.Binormals.Add(Vector3.Zero);
                    }

                    bucket.Polygons.Add(poly);

                }
                else
                {
                    for (int n = 0; n < 3; n++)
                    {
                        poly.TextureCoordinates.Add(texture.TexCoordFloat[n]);
                        poly.Normals.Add(Vector3.Zero);
                        poly.Tangents.Add(Vector3.Zero);
                        poly.Binormals.Add(Vector3.Zero);
                    }

                    bucket.Polygons.Add(poly);
                }
            }

            room.Buckets = buckets.Values.ToList();
            room.Buckets.Sort(TombEngineBucketComparer.Instance);

            // Calculate tangents and binormals
            for (int i = 0; i < room.Vertices.Count; i++)
            {
                var vertex = room.Vertices[i];
                var normalHelpers = vertex.NormalHelpers;

                for (int j = 0; j < normalHelpers.Count; j++)
                {
                    var normalHelper = normalHelpers[j];

                    var e1 = room.Vertices[normalHelper.Polygon.Indices[1]].Position - room.Vertices[normalHelper.Polygon.Indices[0]].Position;
                    var e2 = room.Vertices[normalHelper.Polygon.Indices[2]].Position - room.Vertices[normalHelper.Polygon.Indices[0]].Position;

                    var uv1 = normalHelper.Polygon.TextureCoordinates[1] - normalHelper.Polygon.TextureCoordinates[0];
                    var uv2 = normalHelper.Polygon.TextureCoordinates[2] - normalHelper.Polygon.TextureCoordinates[0];

                    float r = 1.0f / (uv1.X * uv2.Y - uv1.Y * uv2.X);
                    normalHelper.Polygon.Tangent = Vector3.Normalize((e1 * uv2.Y - e2 * uv1.Y) * r);
                    normalHelper.Polygon.Binormal = Vector3.Cross(normalHelper.Polygon.Normal, normalHelper.Polygon.Tangent);
                }
            }
        }

		private void CollectNormalGroups()
		{
			_normalGroups = new Dictionary<Vector3, List<(TombEngineRoom room, int vertexIndex, NormalHelper poly)>>();

			foreach (var room in _tempRooms.Values)
			{
				var roomPosition = new Vector3(room.Info.X, 0, room.Info.Z);

				for (int i = 0; i < room.Vertices.Count; i++)
				{
					var vertex = room.Vertices[i];
					if (!vertex.IsOnPortal)
						continue;

					var pos = roomPosition + vertex.Position;

					if (!_normalGroups.TryGetValue(pos, out var list))
					{
						list = new List<(TombEngineRoom, int, NormalHelper)>();
						_normalGroups[pos] = list;
					}

					foreach (var helper in vertex.NormalHelpers)
						list.Add((room, i, helper));
				}
			}
		}

		private void NormalizeLocalVertexNormals()
		{
			foreach (var room in _tempRooms.Values)
			{
				for (int i = 0; i < room.Vertices.Count; i++)
				{
					var vertex = room.Vertices[i];
					if (vertex.IsOnPortal)
						continue;

					var normalSum = Vector3.Zero;
					var helpers = vertex.NormalHelpers;

					if (helpers.Count == 0)
						continue;

					foreach (var helper in helpers)
						normalSum += helper.Polygon.Normal;

					var finalNormal = -Vector3.Normalize(normalSum);

					if (float.IsNaN(finalNormal.X) || float.IsNaN(finalNormal.Y) || float.IsNaN(finalNormal.Z))
					{
						finalNormal = Vector3.Zero;
						var reference = helpers[0].Polygon.Normal;
						foreach (var helper in helpers)
						{
							var n = helper.Polygon.Normal;
							if (Vector3.Dot(reference, n) < 0)
								n = -n;
							finalNormal += n;
						}
						finalNormal = -Vector3.Normalize(finalNormal);
					}

					foreach (var helper in helpers)
					{
						for (int k = 0; k < helper.Polygon.Indices.Count; k++)
						{
							if (helper.Polygon.Indices[k] == i)
							{
								helper.Polygon.Normals[k] = finalNormal;
								helper.Polygon.Tangents[k] = helper.Polygon.Tangent;
								helper.Polygon.Binormals[k] = helper.Polygon.Binormal;
								break;
							}
						}
					}
				}
			}
		}

		private void AverageAndApplyNormals()
        {
            _progressReporter.ReportInfo("Averaging room normals");

            NormalizeLocalVertexNormals();

			foreach (var kvp in _normalGroups)
			{
				var helpers = kvp.Value;

				// Split normals in clusters by angle threshold
				var clusters = ClusterNormals(helpers, angleThreshold: -0.1f); // ≈ 95°

				foreach (var cluster in clusters)
				{
					var normalSum = Vector3.Zero;
					foreach (var (_, _, helper) in cluster)
					{
						normalSum += Vector3.Normalize(helper.Polygon.Normal);
					}

					var sharedNormal = -Vector3.Normalize(normalSum / cluster.Count);

					// Fallback in the case of NaN
					if (float.IsNaN(sharedNormal.X) || float.IsNaN(sharedNormal.Y) || float.IsNaN(sharedNormal.Z))
					{
						sharedNormal = -Vector3.Normalize(cluster[0].poly.Polygon.Normal);
					}

					// Apply the normal to the vertices of that cluster
					foreach (var (_, vertexIndex, helper) in cluster)
					{
						for (int k = 0; k < helper.Polygon.Indices.Count; k++)
						{
							if (helper.Polygon.Indices[k] == vertexIndex)
							{
								helper.Polygon.Normals[k] = sharedNormal;
								helper.Polygon.Tangents[k] = helper.Polygon.Tangent;
								helper.Polygon.Binormals[k] = helper.Polygon.Binormal;
								break;
							}
						}
					}
				}
			}
		}

		private List<List<(TombEngineRoom room, int index, NormalHelper poly)>> ClusterNormals(
			List<(TombEngineRoom room, int index, NormalHelper poly)> helpers, float angleThreshold)
		{
			var clusters = new List<List<(TombEngineRoom room, int index, NormalHelper poly)>>();

			foreach (var helper in helpers)
			{
				var normal = Vector3.Normalize(helper.poly.Polygon.Normal);
				bool added = false;

				foreach (var cluster in clusters)
				{
					var referenceNormal = Vector3.Normalize(cluster[0].poly.Polygon.Normal);
					if (Vector3.Dot(normal, referenceNormal) >= angleThreshold)
					{
						cluster.Add(helper);
						added = true;
						break;
					}
				}

				if (!added)
				{
					clusters.Add(new List<(TombEngineRoom room, int index, NormalHelper poly)> { helper });
				}
			}

			return clusters;
		}
	}
}
