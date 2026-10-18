using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorGeometry;
using TombLib.LevelData.SectorStructs;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public class RoomGeometry
    {
        // EditorUV Map:
        //                      | +Y
        //   ################## | ##################
        //   ###   Triangle   # | #                #
        //   #  ###    Split  # | #                #
        //   #     ###        # | #      Quad      #
        //   #        ###     # | #      Face      #
        //   #           ###  # | #                #
        //   #              ### | #                #
        //   ################## | ##################    +x
        // ---------------------0---------------------------
        //   ################## | ##################
        //   ###   Triangle   # | ###   Triangle   #
        //   #  ###    Split  # | #  ###    Split  #
        //   #     ###        # | #     ###        #
        //   #        ###     # | #        ###     #
        //   #           ###  # | #           ###  #
        //   #              ### | #              ###
        //   ################## | ##################
        //                      |

        // Quads are reformed by 2 triangles.
        public int DoubleSidedTriangleCount { get; private set; }

        public List<Vector3> VertexPositions { get; } = new List<Vector3>(); // one for each vertex
        public List<Vector2> VertexEditorUVs { get; } = new List<Vector2>(); // one for each vertex (ushort to save (GPU) memory and bandwidth)
        public List<Vector3> VertexColors { get; } = new List<Vector3>(); // one for each vertex

        public List<TextureArea> TriangleTextureAreas { get; } = new List<TextureArea>(); // one for each triangle
        public List<SectorFaceIdentity> TriangleSectorInfo { get; } = new List<SectorFaceIdentity>(); // one for each triangle

        public Dictionary<Vector3, List<int>> SharedVertices { get; } = new Dictionary<Vector3, List<int>>();
        public SortedList<SectorFaceIdentity, VertexRange> VertexRangeLookup { get; } = new SortedList<SectorFaceIdentity, VertexRange>();

        // useLegacyCode is used for converting legacy .PRJ files to .PRJ2 files
        public void Build(Room room, bool useLegacyCode = false)
        {
            ClearGeometryData();

            int xMax = room.NumXSectors - 1;
            int zMax = room.NumZSectors - 1;
            Sector[,] sectors = room.Sectors;

            // Build face polygons
            for (int x = 0; x <= xMax; x++) // Outer X loop order matches VertexRangeKey sorting.
            {
                for (int z = 0; z <= zMax; z++)
                {
                    if (IsCornerSector(x, z, xMax, zMax))
                        continue;

                    Sector sector = sectors[x, z];

                    // Inner vertical faces for each cardinal direction
                    TryAddInnerVerticalFace(room, sectors, x, z, xMax, zMax, FaceDirection.PositiveZ, useLegacyCode);
                    TryAddInnerVerticalFace(room, sectors, x, z, xMax, zMax, FaceDirection.NegativeZ, useLegacyCode);
                    TryAddInnerVerticalFace(room, sectors, x, z, xMax, zMax, FaceDirection.PositiveX, useLegacyCode);
                    TryAddInnerVerticalFace(room, sectors, x, z, xMax, zMax, FaceDirection.NegativeX, useLegacyCode);

                    // Diagonal floor/ceiling faces
                    TryAddDiagonalFaces(room, sector, x, z, useLegacyCode);

                    // Border wall faces for each cardinal direction
                    TryAddBorderWallFace(room, sectors, x, z, xMax, zMax, FaceDirection.PositiveZ, useLegacyCode);
                    TryAddBorderWallFace(room, sectors, x, z, xMax, zMax, FaceDirection.NegativeZ, useLegacyCode);
                    TryAddBorderWallFace(room, sectors, x, z, xMax, zMax, FaceDirection.PositiveX, useLegacyCode);
                    TryAddBorderWallFace(room, sectors, x, z, xMax, zMax, FaceDirection.NegativeX, useLegacyCode);

                    // Floor and ceiling polygons
                    AddFloorAndCeilingFaces(room, sector, x, z);
                }
            }

            GroupSharedVertices();

            // Build color array
            VertexColors.Resize(VertexPositions.Count, room.Properties.AmbientLight);
        }

        #region Build Helpers

        private void ClearGeometryData()
        {
            VertexPositions.Clear();
            VertexEditorUVs.Clear();
            VertexColors.Clear();
            TriangleTextureAreas.Clear();
            TriangleSectorInfo.Clear();
            SharedVertices.Clear();
            VertexRangeLookup.Clear();
            DoubleSidedTriangleCount = 0;
        }

        private static bool IsCornerSector(int x, int z, int xMax, int zMax)
            => (x == 0 || x == xMax) && (z == 0 || z == zMax);

        /// <summary>
        /// Tries to add vertical faces at an inner (non-border) sector for the given cardinal direction.
        /// Skips if the sector is out of bounds or if the neighbor completely blocks the face.
        /// </summary>
        private void TryAddInnerVerticalFace(Room room, Sector[,] sectors, int x, int z,
            int xMax, int zMax, FaceDirection direction, bool useLegacyCode)
        {
            if (!IsInBoundsForInnerFace(x, z, xMax, zMax, direction))
                return;

            var (nx, nz) = GetNeighborPosition(x, z, direction);

            if (IsNeighborBlockingFace(sectors[nx, nz], direction))
                return;

            bool hasMiddle = ShouldCurrentSectorHaveMiddle(sectors[x, z], direction);

            AddVerticalFaces(room, x, z, direction,
                hasFloorPart: true, hasCeilingPart: true, hasMiddlePart: hasMiddle, useLegacyCode);
        }

        /// <summary>
        /// Adds diagonal floor/ceiling vertical faces if the sector has a diagonal split.
        /// </summary>
        private void TryAddDiagonalFaces(Room room, Sector sector, int x, int z, bool useLegacyCode)
        {
            if (sector.Floor.DiagonalSplit != DiagonalSplit.None)
            {
                bool isWall = sector.Type == SectorType.Wall;

                AddVerticalFaces(room, x, z, FaceDirection.DiagonalFloor,
                    hasFloorPart: true, hasCeilingPart: isWall, hasMiddlePart: isWall, useLegacyCode);
            }

            if (sector.Ceiling.DiagonalSplit != DiagonalSplit.None && sector.Type != SectorType.Wall)
            {
                AddVerticalFaces(room, x, z, FaceDirection.DiagonalCeiling,
                    hasFloorPart: false, hasCeilingPart: true, hasMiddlePart: false, useLegacyCode);
            }
        }

        /// <summary>
        /// Tries to add vertical faces at a border sector for the given cardinal direction.
        /// Border walls may require portal lookup to determine if the middle face is needed.
        /// </summary>
        private void TryAddBorderWallFace(Room room, Sector[,] sectors, int x, int z,
            int xMax, int zMax, FaceDirection direction, bool useLegacyCode)
        {
            if (!IsOnBorderForDirection(x, z, xMax, zMax, direction))
                return;

            var (innerX, innerZ) = GetInnerNeighborForBorder(x, z, xMax, zMax, direction);

            if (IsNeighborBlockingFace(sectors[innerX, innerZ], direction))
                return;

            bool hasMiddle = ShouldBorderWallHaveMiddle(room, sectors[x, z], x, z, direction);

            AddVerticalFaces(room, x, z, direction,
                hasFloorPart: true, hasCeilingPart: true, hasMiddlePart: hasMiddle, useLegacyCode);
        }

        /// <summary>
        /// Builds floor and ceiling face polygons for the given sector, including ceiling winding reversal.
        /// </summary>
        private void AddFloorAndCeilingFaces(Room room, Sector sector, int x, int z)
        {
            // Floor polygons
            Room.RoomConnectionInfo floorPortalInfo = room.GetFloorRoomConnectionInfo(new VectorInt2(x, z));

            BuildFloorOrCeilingFace(room, x, z,
                sector.Floor.XnZp, sector.Floor.XpZp, sector.Floor.XpZn, sector.Floor.XnZn,
                sector.Floor.DiagonalSplit, sector.Floor.SplitDirectionIsXEqualsZ,
                SectorFace.Floor, SectorFace.Floor_Triangle2, floorPortalInfo.VisualType);

            // Ceiling polygons (vertices need reversed winding)
            int ceilingStartVertex = VertexPositions.Count;

            Room.RoomConnectionInfo ceilingPortalInfo = room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z));

            BuildFloorOrCeilingFace(room, x, z,
                sector.Ceiling.XnZp, sector.Ceiling.XpZp, sector.Ceiling.XpZn, sector.Ceiling.XnZn,
                sector.Ceiling.DiagonalSplit, sector.Ceiling.SplitDirectionIsXEqualsZ,
                SectorFace.Ceiling, SectorFace.Ceiling_Triangle2, ceilingPortalInfo.VisualType);

            ReverseCeilingWinding(ceilingStartVertex);
        }

        /// <summary>
        /// Reverses the winding order of ceiling triangles so they face downward.
        /// Swaps the first and third vertices of each triangle.
        /// </summary>
        private void ReverseCeilingWinding(int startVertex)
        {
            for (int i = startVertex; i < VertexPositions.Count; i += 3)
            {
                (VertexPositions[i], VertexPositions[i + 2]) = (VertexPositions[i + 2], VertexPositions[i]);
                (VertexEditorUVs[i], VertexEditorUVs[i + 2]) = (VertexEditorUVs[i + 2], VertexEditorUVs[i]);

                TextureArea textureArea = TriangleTextureAreas[i / 3];
                Swap.Do(ref textureArea.TexCoord0, ref textureArea.TexCoord2);
                TriangleTextureAreas[i / 3] = textureArea;
            }
        }

        private void GroupSharedVertices()
        {
            for (int i = 0; i < VertexPositions.Count; ++i)
            {
                Vector3 position = VertexPositions[i];

                if (!SharedVertices.TryGetValue(position, out List<int> list))
                    SharedVertices.Add(position, list = new List<int>());

                list.Add(i);
            }
        }

        #endregion Build Helpers

        #region Direction & Geometry Helpers

        /// <summary>
        /// Returns the pair of diagonal splits that face toward the given direction.
        /// A wall sector with one of these splits (or <see cref="DiagonalSplit.None"/>) blocks faces from this direction.
        /// </summary>
        private static (DiagonalSplit Split1, DiagonalSplit Split2) GetBlockingDiagonalSplits(FaceDirection direction)
        {
            return direction switch
            {
                FaceDirection.PositiveZ => (DiagonalSplit.XpZn, DiagonalSplit.XnZn),
                FaceDirection.NegativeZ => (DiagonalSplit.XpZp, DiagonalSplit.XnZp),
                FaceDirection.PositiveX => (DiagonalSplit.XnZn, DiagonalSplit.XnZp),
                FaceDirection.NegativeX => (DiagonalSplit.XpZn, DiagonalSplit.XpZp),
                _ => throw new ArgumentException("Invalid cardinal direction.", nameof(direction))
            };
        }

        /// <summary>
        /// Checks whether a neighboring sector is a wall that completely blocks the face from the given direction.
        /// A wall blocks when it has no diagonal split, or its split faces into the given direction.
        /// </summary>
        private static bool IsNeighborBlockingFace(Sector neighbor, FaceDirection direction)
        {
            if (neighbor.Type != SectorType.Wall)
                return false;

            DiagonalSplit ds = neighbor.Floor.DiagonalSplit;

            if (ds == DiagonalSplit.None)
                return true;

            var (split1, split2) = GetBlockingDiagonalSplits(direction);

            return ds == split1 || ds == split2;
        }

        /// <summary>
        /// Determines if the current sector should have a middle wall part for the given direction.
        /// Middle parts appear on wall sectors and portals with textured faces, unless the sector's
        /// diagonal split faces the same direction (making a middle face unnecessary).
        /// </summary>
        private static bool ShouldCurrentSectorHaveMiddle(Sector sector, FaceDirection direction)
        {
            bool isWallOrTexturedPortal = sector.Type == SectorType.Wall
                || (sector.WallPortal?.HasTexturedFaces ?? false);

            if (!isWallOrTexturedPortal)
                return false;

            var (split1, split2) = GetBlockingDiagonalSplits(direction);
            DiagonalSplit ds = sector.Floor.DiagonalSplit;

            return ds != split1 && ds != split2;
        }

        /// <summary>
        /// Checks whether the sector at (x, z) is within valid bounds for an inner (non-border) vertical face
        /// in the given direction. The perpendicular axis must be in the inner range, and the along-axis
        /// must have room for a valid inner neighbor.
        /// </summary>
        private static bool IsInBoundsForInnerFace(int x, int z, int xMax, int zMax, FaceDirection direction)
        {
            return direction switch
            {
                FaceDirection.PositiveZ => x > 0 && x < xMax && z > 0 && z < zMax - 1,
                FaceDirection.NegativeZ => x > 0 && x < xMax && z > 1 && z < zMax,
                FaceDirection.PositiveX => z > 0 && z < zMax && x > 0 && x < xMax - 1,
                FaceDirection.NegativeX => z > 0 && z < zMax && x > 1 && x < xMax,
                _ => false
            };
        }

        private static (int X, int Z) GetNeighborPosition(int x, int z, FaceDirection direction)
        {
            return direction switch
            {
                FaceDirection.PositiveZ => (x, z + 1),
                FaceDirection.NegativeZ => (x, z - 1),
                FaceDirection.PositiveX => (x + 1, z),
                FaceDirection.NegativeX => (x - 1, z),
                _ => throw new ArgumentException("Invalid cardinal direction.", nameof(direction))
            };
        }

        private static FaceDirection GetOppositeDirection(FaceDirection direction)
        {
            return direction switch
            {
                FaceDirection.PositiveZ => FaceDirection.NegativeZ,
                FaceDirection.NegativeZ => FaceDirection.PositiveZ,
                FaceDirection.PositiveX => FaceDirection.NegativeX,
                FaceDirection.NegativeX => FaceDirection.PositiveX,
                _ => throw new ArgumentException("Invalid cardinal direction.", nameof(direction))
            };
        }

        /// <summary>
        /// Checks whether (x, z) is on the room border that corresponds to the given face direction.
        /// Border at z=0 faces +Z (inward), border at x=0 faces +X (inward), etc.
        /// </summary>
        private static bool IsOnBorderForDirection(int x, int z, int xMax, int zMax, FaceDirection direction)
        {
            return direction switch
            {
                FaceDirection.PositiveZ => z == 0 && x > 0 && x < xMax,
                FaceDirection.NegativeZ => z == zMax && x > 0 && x < xMax,
                FaceDirection.PositiveX => x == 0 && z > 0 && z < zMax,
                FaceDirection.NegativeX => x == xMax && z > 0 && z < zMax,
                _ => false
            };
        }

        /// <summary>
        /// Gets the position of the first inner (non-border) sector adjacent to the border
        /// in the given direction. For +Z border (z=0), this is (x, 1), etc.
        /// </summary>
        private static (int X, int Z) GetInnerNeighborForBorder(int x, int z, int xMax, int zMax, FaceDirection direction)
        {
            return direction switch
            {
                FaceDirection.PositiveZ => (x, 1),
                FaceDirection.NegativeZ => (x, zMax - 1),
                FaceDirection.PositiveX => (1, z),
                FaceDirection.NegativeX => (xMax - 1, z),
                _ => throw new ArgumentException("Invalid cardinal direction.", nameof(direction))
            };
        }

        /// <summary>
        /// Determines if a border wall should have a middle face, considering:
        /// 1. Border walls without portals always have middle faces.
        /// 2. Portals with textured faces show the middle.
        /// 3. Portals where the adjoining room's facing sector is a blocking wall.
        /// </summary>
        private static bool ShouldBorderWallHaveMiddle(Room room, Sector sector, int x, int z, FaceDirection direction)
        {
            if (sector.Type == SectorType.BorderWall && sector.WallPortal is null)
                return true;

            if (sector.WallPortal?.HasTexturedFaces ?? false)
                return true;

            if (sector.WallPortal is not null)
            {
                Room adjoiningRoom = ResolveAdjoiningRoom(room, sector.WallPortal);
                Sector facingSector = GetAdjoiningFacingSector(room, adjoiningRoom, x, z, direction);
                return IsNeighborBlockingFace(facingSector, GetOppositeDirection(direction));
            }

            return false;
        }

        /// <summary>
        /// Resolves the actual adjoining room, accounting for alternated (flipped) room pairs.
        /// </summary>
        private static Room ResolveAdjoiningRoom(Room room, PortalInstance portal)
        {
            Room adjoiningRoom = portal.AdjoiningRoom;

            if (room.Alternated && room.AlternateBaseRoom is not null &&
                adjoiningRoom.Alternated && adjoiningRoom.AlternateRoom is not null)
            {
                adjoiningRoom = adjoiningRoom.AlternateRoom;
            }

            return adjoiningRoom;
        }

        /// <summary>
        /// Gets the sector in the adjoining room that faces back toward our room's border sector.
        /// The facing sector is at the opposite border edge of the adjoining room.
        /// </summary>
        private static Sector GetAdjoiningFacingSector(Room room, Room adjoiningRoom, int x, int z, FaceDirection direction)
        {
            int facingX = x + (room.Position.X - adjoiningRoom.Position.X);
            int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);

            return direction switch
            {
                FaceDirection.PositiveZ => adjoiningRoom.GetSectorTry(facingX, adjoiningRoom.NumZSectors - 2) ?? Sector.Empty,
                FaceDirection.NegativeZ => adjoiningRoom.GetSectorTry(facingX, 1) ?? Sector.Empty,
                FaceDirection.PositiveX => adjoiningRoom.GetSectorTry(adjoiningRoom.NumXSectors - 2, facingZ) ?? Sector.Empty,
                FaceDirection.NegativeX => adjoiningRoom.GetSectorTry(1, facingZ) ?? Sector.Empty,
                _ => throw new ArgumentException("Invalid cardinal direction.", nameof(direction))
            };
        }

        #endregion Direction & Geometry Helpers

        public void UpdateFaceTexture(int x, int z, SectorFace face, TextureArea texture, bool wasDoubleSided)
        {
            VertexRange range = VertexRangeLookup.GetValueOrDefault(new SectorFaceIdentity(x, z, face));

            if (range.Count == 3) // Triangle
            {
                if (wasDoubleSided)
                    DoubleSidedTriangleCount--;

                if (texture.DoubleSided)
                    DoubleSidedTriangleCount++;

                if (face is SectorFace.Ceiling or SectorFace.Ceiling_Triangle2)
                    Swap.Do(ref texture.TexCoord0, ref texture.TexCoord2);

                TriangleTextureAreas[range.Start / 3] = texture;
            }
            else if (range.Count == 6) // Quad
            {
                if (wasDoubleSided)
                    DoubleSidedTriangleCount -= 2;

                if (texture.DoubleSided)
                    DoubleSidedTriangleCount += 2;

                TextureArea texture0 = texture;
                texture0.TexCoord0 = texture.TexCoord2;
                texture0.TexCoord1 = texture.TexCoord3;
                texture0.TexCoord2 = texture.TexCoord1;

                if (face is SectorFace.Ceiling or SectorFace.Ceiling_Triangle2)
                    Swap.Do(ref texture0.TexCoord0, ref texture0.TexCoord2);

                TriangleTextureAreas[range.Start / 3] = texture0;

                TextureArea texture1 = texture;
                texture1.TexCoord0 = texture.TexCoord0;
                texture1.TexCoord1 = texture.TexCoord1;
                texture1.TexCoord2 = texture.TexCoord3;

                if (face is SectorFace.Ceiling or SectorFace.Ceiling_Triangle2)
                    Swap.Do(ref texture1.TexCoord0, ref texture1.TexCoord2);

                TriangleTextureAreas[(range.Start + 3) / 3] = texture1;
            }
        }

        private enum FaceDirection
        {
            PositiveZ, NegativeZ, PositiveX, NegativeX, DiagonalFloor, DiagonalCeiling, DiagonalWall
        }

        private void BuildFloorOrCeilingFace(Room room, int x, int z, int h0, int h1, int h2, int h3,
            DiagonalSplit splitType, bool diagonalSplitXEqualsY,
            SectorFace face1, SectorFace face2, Room.RoomConnectionType portalMode)
        {
            Sector sector = room.Sectors[x, z];

            if (!ShouldBuildHorizontalFace(sector.Type, splitType, portalMode))
                return;

            // Adjust split direction based on triangular portal orientation
            diagonalSplitXEqualsY = AdjustSplitDirectionForPortal(h0, h1, h2, h3, diagonalSplitXEqualsY, portalMode);

            // Apply default textures if needed
            ApplyDefaultTextureIfNone(sector, face1, room.Level.Settings.DefaultTexture);
            ApplyDefaultTextureIfNone(sector, face2, room.Level.Settings.DefaultTexture);

            TextureArea face1Texture = sector.GetFaceTexture(face1);
            TextureArea face2Texture = sector.GetFaceTexture(face2);

            // Precompute corner world-space coordinates
            //
            // XnZp(h0) --- XpZp(h1)
            //    |     \ /     |
            // XnZn(h3) --- XpZn(h2)
            //
            float xBase = x * Level.SectorSizeUnit;
            float xNext = (x + 1) * Level.SectorSizeUnit;
            float zBase = z * Level.SectorSizeUnit;
            float zNext = (z + 1) * Level.SectorSizeUnit;

            if (splitType != DiagonalSplit.None)
            {
                BuildDiagonalSplitFace(x, z, sector.Type, splitType, portalMode,
                    face1, face2, face1Texture, face2Texture,
                    xBase, xNext, zBase, zNext, h0, h1, h2, h3);
            }
            else if (SectorSurface.IsQuad2(h0, h1, h2, h3) && portalMode == Room.RoomConnectionType.NoPortal)
            {
                // Flat quad - no triangle split needed
                AddQuad(x, z, face1,
                    new Vector3(xBase, h0, zNext), new Vector3(xNext, h1, zNext),
                    new Vector3(xNext, h2, zBase), new Vector3(xBase, h3, zBase),
                    face1Texture,
                    new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            }
            else
            {
                // Non-flat quad: split into 2 triangles
                BuildSplitQuadFace(x, z, portalMode, diagonalSplitXEqualsY,
                    face1, face2, face1Texture, face2Texture,
                    xBase, xNext, zBase, zNext, h0, h1, h2, h3);
            }
        }

        private static bool ShouldBuildHorizontalFace(SectorType sectorType, DiagonalSplit splitType, Room.RoomConnectionType portalMode)
        {
            if (portalMode == Room.RoomConnectionType.FullPortal)
                return false;

            if (sectorType == SectorType.BorderWall)
                return false;

            if (sectorType == SectorType.Wall && splitType == DiagonalSplit.None)
                return false;

            return true;
        }

        private static bool AdjustSplitDirectionForPortal(int h0, int h1, int h2, int h3,
            bool splitXEqualsY, Room.RoomConnectionType portalMode)
        {
            if (!SectorSurface.IsQuad2(h0, h1, h2, h3))
                return splitXEqualsY;

            return portalMode switch
            {
                Room.RoomConnectionType.TriangularPortalXnZp or
                Room.RoomConnectionType.TriangularPortalXpZn => true,

                Room.RoomConnectionType.TriangularPortalXpZp or
                Room.RoomConnectionType.TriangularPortalXnZn => false,

                _ => splitXEqualsY
            };
        }

        private static void ApplyDefaultTextureIfNone(Sector sector, SectorFace face, TextureArea defaultTexture)
        {
            if (defaultTexture != TextureArea.None && sector.GetFaceTexture(face) == TextureArea.None)
                sector.SetFaceTexture(face, defaultTexture);
        }

        /// <summary>
        /// Builds triangles for a diagonally split floor/ceiling sector.
        /// One triangle is "flat" (all vertices at the same height from the opposite corner)
        /// and the other is "sloped" (vertices at their individual heights).
        /// </summary>
        private void BuildDiagonalSplitFace(int x, int z, SectorType sectorType, DiagonalSplit splitType,
            Room.RoomConnectionType portalMode, SectorFace face1, SectorFace face2,
            TextureArea face1Texture, TextureArea face2Texture,
            float xBase, float xNext, float zBase, float zNext,
            int h0, int h1, int h2, int h3)
        {
            bool isWall = sectorType == SectorType.Wall;

            switch (splitType)
            {
                case DiagonalSplit.XpZn:
                    // Flat triangle (face1): all vertices at h0 (opposite corner XnZp)
                    if (portalMode != Room.RoomConnectionType.TriangularPortalXnZp)
                    {
                        AddTriangle(x, z, face1,
                            new Vector3(xBase, h0, zBase), new Vector3(xBase, h0, zNext), new Vector3(xNext, h0, zNext),
                            face1Texture, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), isXEqualYDiagonal: true);
                    }

                    // Sloped triangle (face2): individual corner heights
                    if (portalMode != Room.RoomConnectionType.TriangularPortalXpZn && !isWall)
                    {
                        AddTriangle(x, z, face2,
                            new Vector3(xNext, h1, zNext), new Vector3(xNext, h2, zBase), new Vector3(xBase, h3, zBase),
                            face2Texture, new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), isXEqualYDiagonal: true);
                    }

                    break;

                case DiagonalSplit.XnZn:
                    // Flat triangle (face1): all vertices at h1 (opposite corner XpZp)
                    if (portalMode != Room.RoomConnectionType.TriangularPortalXpZp)
                    {
                        AddTriangle(x, z, face1,
                            new Vector3(xBase, h1, zNext), new Vector3(xNext, h1, zNext), new Vector3(xNext, h1, zBase),
                            face1Texture, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), isXEqualYDiagonal: false);
                    }

                    // Sloped triangle (face2)
                    if (portalMode != Room.RoomConnectionType.TriangularPortalXnZn && !isWall)
                    {
                        AddTriangle(x, z, face2,
                            new Vector3(xNext, h2, zBase), new Vector3(xBase, h3, zBase), new Vector3(xBase, h0, zNext),
                            face2Texture, new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), isXEqualYDiagonal: false);
                    }

                    break;

                case DiagonalSplit.XnZp:
                    // Flat triangle (face2): all vertices at h2 (opposite corner XpZn)
                    if (portalMode != Room.RoomConnectionType.TriangularPortalXpZn)
                    {
                        AddTriangle(x, z, face2,
                            new Vector3(xNext, h2, zNext), new Vector3(xNext, h2, zBase), new Vector3(xBase, h2, zBase),
                            face2Texture, new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), isXEqualYDiagonal: true);
                    }

                    // Sloped triangle (face1)
                    if (portalMode != Room.RoomConnectionType.TriangularPortalXnZp && !isWall)
                    {
                        AddTriangle(x, z, face1,
                            new Vector3(xBase, h3, zBase), new Vector3(xBase, h0, zNext), new Vector3(xNext, h1, zNext),
                            face1Texture, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0), isXEqualYDiagonal: true);
                    }

                    break;

                case DiagonalSplit.XpZp:
                    // Flat triangle (face2): all vertices at h3 (opposite corner XnZn)
                    if (portalMode != Room.RoomConnectionType.TriangularPortalXnZn)
                    {
                        AddTriangle(x, z, face2,
                            new Vector3(xNext, h3, zBase), new Vector3(xBase, h3, zBase), new Vector3(xBase, h3, zNext),
                            face2Texture, new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0), isXEqualYDiagonal: false);
                    }

                    // Sloped triangle (face1)
                    if (portalMode != Room.RoomConnectionType.TriangularPortalXpZp && !isWall)
                    {
                        AddTriangle(x, z, face1,
                            new Vector3(xBase, h0, zNext), new Vector3(xNext, h1, zNext), new Vector3(xNext, h2, zBase),
                            face1Texture, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), isXEqualYDiagonal: false);
                    }

                    break;

                default:
                    throw new NotSupportedException("Unknown DiagonalSplit type.");
            }
        }

        /// <summary>
        /// Builds two triangles for a non-flat quad face (no diagonal wall split).
        /// The split direction determines if the diagonal goes along X=Z or X=-Z.
        /// </summary>
        private void BuildSplitQuadFace(int x, int z, Room.RoomConnectionType portalMode, bool splitXEqualsY,
            SectorFace face1, SectorFace face2, TextureArea face1Texture, TextureArea face2Texture,
            float xBase, float xNext, float zBase, float zNext,
            int h0, int h1, int h2, int h3)
        {
            Vector3 pXnZp = new(xBase, h0, zNext);
            Vector3 pXpZp = new(xNext, h1, zNext);
            Vector3 pXpZn = new(xNext, h2, zBase);
            Vector3 pXnZn = new(xBase, h3, zBase);

            if (splitXEqualsY
                || portalMode == Room.RoomConnectionType.TriangularPortalXnZp
                || portalMode == Room.RoomConnectionType.TriangularPortalXpZn)
            {
                // Split along XnZn-XpZp diagonal (X = Z direction)
                if (portalMode != Room.RoomConnectionType.TriangularPortalXnZp)
                {
                    AddTriangle(x, z, face2, pXnZn, pXnZp, pXpZp,
                        face2Texture, new Vector2(0, 1), new Vector2(0, 0), new Vector2(1, 0),
                        isXEqualYDiagonal: true);
                }

                if (portalMode != Room.RoomConnectionType.TriangularPortalXpZn)
                {
                    AddTriangle(x, z, face1, pXpZp, pXpZn, pXnZn,
                        face1Texture, new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                        isXEqualYDiagonal: true);
                }
            }
            else
            {
                // Split along XnZp-XpZn diagonal (X = -Z direction)
                if (portalMode != Room.RoomConnectionType.TriangularPortalXpZp)
                {
                    AddTriangle(x, z, face1, pXnZp, pXpZp, pXpZn,
                        face1Texture, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1),
                        isXEqualYDiagonal: false);
                }

                if (portalMode != Room.RoomConnectionType.TriangularPortalXnZn)
                {
                    AddTriangle(x, z, face2, pXpZn, pXnZn, pXnZp,
                        face2Texture, new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, 0),
                        isXEqualYDiagonal: false);
                }
            }
        }

        /// <summary>
        /// Builds and adds vertical wall face polygons (floor part, ceiling part, and/or middle part)
        /// for a single sector edge in the given <paramref name="faceDirection"/>.
        /// <para>
        /// The wall data is retrieved from the room based on the direction, then each requested
        /// wall section (floor, ceiling, middle) is decomposed into triangles/quads and appended
        /// to the geometry buffers via <see cref="AddFace"/>.
        /// </para>
        /// </summary>
        /// <param name="room">The room containing the sector.</param>
        /// <param name="x">Sector X coordinate.</param>
        /// <param name="z">Sector Z coordinate.</param>
        /// <param name="faceDirection">The wall direction (cardinal or diagonal).</param>
        /// <param name="hasFloorPart">Whether to generate the floor portion of the wall (QA + extra floor splits).</param>
        /// <param name="hasCeilingPart">Whether to generate the ceiling portion of the wall (WS + extra ceiling splits).</param>
        /// <param name="hasMiddlePart">Whether to generate the middle wall face between floor and ceiling portions.</param>
        /// <param name="useLegacyCode">When <see langword="true"/>, uses legacy PRJ wall geometry logic
        /// and skips wall data normalization.</param>
        private void AddVerticalFaces(Room room, int x, int z, FaceDirection faceDirection,
            bool hasFloorPart, bool hasCeilingPart, bool hasMiddlePart, bool useLegacyCode = false)
        {
            bool normalize = !useLegacyCode;

            SectorWallData wallData = faceDirection switch
            {
                FaceDirection.PositiveZ => room.GetPositiveZWallData(x, z, normalize),
                FaceDirection.NegativeZ => room.GetNegativeZWallData(x, z, normalize),
                FaceDirection.PositiveX => room.GetPositiveXWallData(x, z, normalize),
                FaceDirection.NegativeX => room.GetNegativeXWallData(x, z, normalize),
                FaceDirection.DiagonalFloor => room.GetDiagonalWallData(x, z, isDiagonalCeiling: false, normalize),
                FaceDirection.DiagonalCeiling => room.GetDiagonalWallData(x, z, isDiagonalCeiling: true, normalize),
                _ => throw new NotSupportedException($"Face direction '{faceDirection}' is not supported for vertical faces.")
            };

            Sector sector = room.Sectors[x, z];

            if (hasFloorPart)
            {
                IReadOnlyList<SectorFaceData> floorFaces = useLegacyCode
                    ? LegacyWallGeometry.GetVerticalFloorPartFaces(wallData, sector.IsAnyWall)
                    : wallData.GetVerticalFloorPartFaces(sector.Floor.DiagonalSplit);

                for (int i = 0; i < floorFaces.Count; i++)
                    AddFace(room, x, z, floorFaces[i]);
            }

            if (hasCeilingPart)
            {
                IReadOnlyList<SectorFaceData> ceilingFaces = useLegacyCode
                    ? LegacyWallGeometry.GetVerticalCeilingPartFaces(wallData, sector.IsAnyWall)
                    : wallData.GetVerticalCeilingPartFaces(sector.Ceiling.DiagonalSplit);

                for (int i = 0; i < ceilingFaces.Count; i++)
                    AddFace(room, x, z, ceilingFaces[i]);
            }

            if (hasMiddlePart)
            {
                SectorFaceData? middleFace = useLegacyCode
                    ? LegacyWallGeometry.GetVerticalMiddlePartFace(wallData)
                    : wallData.GetVerticalMiddleFace();

                if (middleFace.HasValue)
                    AddFace(room, x, z, middleFace.Value);
            }
        }

        /// <summary>
        /// Adds a single vertical wall face (triangle or quad) to the geometry buffers.
        /// Applies the default texture if the face has no texture assigned.
        /// </summary>
        private void AddFace(Room room, int x, int z, SectorFaceData face)
        {
            Sector sector = room.Sectors[x, z];

            bool shouldApplyDefaultTexture = sector.GetFaceTexture(face.Face) == TextureArea.None
                && room.Level.Settings.DefaultTexture != TextureArea.None;

            if (shouldApplyDefaultTexture)
                sector.SetFaceTexture(face.Face, room.Level.Settings.DefaultTexture);

            TextureArea texture = sector.GetFaceTexture(face.Face);

            if (face.IsQuad)
                AddQuad(x, z, face.Face, face.P0, face.P1, face.P2, face.P3.Value, texture, face.UV0, face.UV1, face.UV2, face.UV3.Value);
            else
                AddTriangle(x, z, face.Face, face.P0, face.P1, face.P2, texture, face.UV0, face.UV1, face.UV2, face.IsXEqualYDiagonal.Value);
        }

        private void AddQuad(int x, int z, SectorFace face, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
                             TextureArea texture, Vector2 editorUV0, Vector2 editorUV1, Vector2 editorUV2, Vector2 editorUV3)
        {
            if (texture.DoubleSided)
                DoubleSidedTriangleCount += 2;

            VertexRangeLookup.Add(new SectorFaceIdentity(x, z, face), new VertexRange(VertexPositions.Count, 6));

            VertexPositions.Add(p1);
            VertexPositions.Add(p2);
            VertexPositions.Add(p0);
            VertexPositions.Add(p3);
            VertexPositions.Add(p0);
            VertexPositions.Add(p2);

            VertexEditorUVs.Add(editorUV1);
            VertexEditorUVs.Add(editorUV2);
            VertexEditorUVs.Add(editorUV0);
            VertexEditorUVs.Add(editorUV3);
            VertexEditorUVs.Add(editorUV0);
            VertexEditorUVs.Add(editorUV2);

            TextureArea texture0 = texture;
            texture0.TexCoord0 = texture.TexCoord2;
            texture0.TexCoord1 = texture.TexCoord3;
            texture0.TexCoord2 = texture.TexCoord1;
            TriangleTextureAreas.Add(texture0);
            TriangleSectorInfo.Add(new SectorFaceIdentity(x, z, face));

            TextureArea texture1 = texture;
            texture1.TexCoord0 = texture.TexCoord0;
            texture1.TexCoord1 = texture.TexCoord1;
            texture1.TexCoord2 = texture.TexCoord3;
            TriangleTextureAreas.Add(texture1);
            TriangleSectorInfo.Add(new SectorFaceIdentity(x, z, face));
        }

        private void AddTriangle(int x, int z, SectorFace face, Vector3 p0, Vector3 p1, Vector3 p2, TextureArea texture, Vector2 editorUV0, Vector2 editorUV1, Vector2 editorUV2, bool isXEqualYDiagonal)
        {
            if (texture.DoubleSided)
                DoubleSidedTriangleCount++;

            var editorUvFactor = new Vector2(isXEqualYDiagonal ? -1.0f : 1.0f, -1.0f);
            VertexRangeLookup.Add(new SectorFaceIdentity(x, z, face), new VertexRange(VertexPositions.Count, 3));

            VertexPositions.Add(p0);
            VertexPositions.Add(p1);
            VertexPositions.Add(p2);

            VertexEditorUVs.Add(editorUV0 * editorUvFactor);
            VertexEditorUVs.Add(editorUV1 * editorUvFactor);
            VertexEditorUVs.Add(editorUV2 * editorUvFactor);

            TriangleTextureAreas.Add(texture);
            TriangleSectorInfo.Add(new SectorFaceIdentity(x, z, face));
        }

        private static bool RayTraceCheckFloorCeiling(Room room, int x, int y, int z, int xLight, int zLight)
        {
            int currentX = (x / (int)Level.SectorSizeUnit) - (x > xLight ? 1 : 0);
            int currentZ = (z / (int)Level.SectorSizeUnit) - (z > zLight ? 1 : 0);

            if (currentX < 0 || currentX >= room.NumXSectors ||
                currentZ < 0 || currentZ >= room.NumZSectors)
            {
                return false;
            }

            Sector sector = room.Sectors[currentX, currentZ];
            int floorMin = Clicks.FromWorld(sector.Floor.Min);
            int ceilingMax = Clicks.FromWorld(sector.Ceiling.Max);
            int yClicks = Clicks.FromWorld(y);

            return floorMin <= yClicks && ceilingMax >= yClicks;
        }

        private static bool RayTraceX(Room room, int x, int y, int z, int xLight, int yLight, int zLight)
        {
            int deltaX;
            int deltaY;
            int deltaZ;

            int minX;
            int maxX;

            yLight = -yLight;
            y = -y;

            int yPoint = y;
            int zPoint = z;

            if (x <= xLight)
            {
                deltaX = xLight - x;
                deltaY = yLight - y;
                deltaZ = zLight - z;

                minX = x;
                maxX = xLight;
            }
            else
            {
                deltaX = x - xLight;
                deltaY = y - yLight;
                deltaZ = z - zLight;

                minX = xLight;
                maxX = x;

                yPoint = yLight;
                zPoint = zLight;
            }

            if (deltaX == 0)
                return true;

            int fracX = (((minX >> 10) + 1) << 10) - minX;
            int currentX = ((minX >> 10) + 1) << 10;
            int currentZ = (deltaZ * fracX / (deltaX + 1)) + zPoint;
            int currentY = (deltaY * fracX / (deltaX + 1)) + yPoint;

            if (currentX > maxX)
                return true;

            do
            {
                int currentXsector = currentX / (int)Level.SectorSizeUnit;
                int currentZsector = currentZ / (int)Level.SectorSizeUnit;

                if (currentZsector < 0 || currentXsector >= room.NumXSectors || currentZsector >= room.NumZSectors)
                {
                    if (currentX == maxX)
                        return true;
                }
                else
                {
                    int currentYclick = -Clicks.FromWorld(currentY, RoundingMethod.Integer);

                    if (currentXsector > 0)
                    {
                        Sector currentSector = room.Sectors[currentXsector - 1, currentZsector];

                        if ((Clicks.FromWorld(currentSector.Floor.XnZp, RoundingMethod.Integer) + Clicks.FromWorld(currentSector.Floor.XnZn, RoundingMethod.Integer)) / 2 > currentYclick ||
                            (Clicks.FromWorld(currentSector.Ceiling.XnZp, RoundingMethod.Integer) + Clicks.FromWorld(currentSector.Ceiling.XnZn, RoundingMethod.Integer)) / 2 < currentYclick ||
                            currentSector.Type == SectorType.Wall)
                        {
                            return false;
                        }
                    }

                    if (currentX == maxX)
                    {
                        return true;
                    }

                    if (currentXsector > 0)
                    {
                        var currentSector = room.Sectors[currentXsector - 1, currentZsector];
                        var nextSector = room.Sectors[currentXsector, currentZsector];

                        if ((Clicks.FromWorld(currentSector.Floor.XpZn, RoundingMethod.Integer) + Clicks.FromWorld(currentSector.Floor.XpZp, RoundingMethod.Integer)) / 2 > currentYclick ||
                            (Clicks.FromWorld(currentSector.Ceiling.XpZn, RoundingMethod.Integer) + Clicks.FromWorld(currentSector.Ceiling.XpZp, RoundingMethod.Integer)) / 2 < currentYclick ||
                            currentSector.Type == SectorType.Wall ||
                            (Clicks.FromWorld(nextSector.Floor.XnZp, RoundingMethod.Integer) + Clicks.FromWorld(nextSector.Floor.XnZn, RoundingMethod.Integer)) / 2 > currentYclick ||
                            (Clicks.FromWorld(nextSector.Ceiling.XnZp, RoundingMethod.Integer) + Clicks.FromWorld(nextSector.Ceiling.XnZn, RoundingMethod.Integer)) / 2 < currentYclick ||
                            nextSector.Type == SectorType.Wall)
                        {
                            return false;
                        }
                    }
                }

                currentX += (int)Level.SectorSizeUnit;
                currentZ += (deltaZ << 10) / (deltaX + 1);
                currentY += (deltaY << 10) / (deltaX + 1);
            }
            while (currentX <= maxX);

            return true;
        }

        private static bool RayTraceZ(Room room, int x, int y, int z, int xLight, int yLight, int zLight)
        {
            int deltaX;
            int deltaY;
            int deltaZ;

            int minZ;
            int maxZ;

            yLight = -yLight;
            y = -y;

            int yPoint = y;
            int xPoint = x;

            if (z <= zLight)
            {
                deltaX = xLight - x;
                deltaY = yLight - y;
                deltaZ = zLight - z;

                minZ = z;
                maxZ = zLight;
            }
            else
            {
                deltaX = x - xLight;
                deltaY = y - yLight;
                deltaZ = z - zLight;

                minZ = zLight;
                maxZ = z;

                xPoint = xLight;
                yPoint = yLight;
            }

            if (deltaZ == 0)
                return true;

            int fracZ = (((minZ >> 10) + 1) << 10) - minZ;
            int currentZ = ((minZ >> 10) + 1) << 10;
            int currentX = (deltaX * fracZ / (deltaZ + 1)) + xPoint;
            int currentY = (deltaY * fracZ / (deltaZ + 1)) + yPoint;

            if (currentZ > maxZ)
                return true;

            do
            {
                int currentXsector = currentX / (int)Level.SectorSizeUnit;
                int currentZsector = currentZ / (int)Level.SectorSizeUnit;

                if (currentXsector < 0 || currentZsector >= room.NumZSectors || currentXsector >= room.NumXSectors)
                {
                    if (currentZ == maxZ)
                        return true;
                }
                else
                {
                    int currentYclick = -Clicks.FromWorld(currentY, RoundingMethod.Integer);

                    if (currentZsector > 0)
                    {
                        var currentSector = room.Sectors[currentXsector, currentZsector - 1];

                        if ((Clicks.FromWorld(currentSector.Floor.XpZn, RoundingMethod.Integer) + Clicks.FromWorld(currentSector.Floor.XnZn, RoundingMethod.Integer)) / 2 > currentYclick ||
                            (Clicks.FromWorld(currentSector.Ceiling.XpZn, RoundingMethod.Integer) + Clicks.FromWorld(currentSector.Ceiling.XnZn, RoundingMethod.Integer)) / 2 < currentYclick ||
                            currentSector.Type == SectorType.Wall)
                        {
                            return false;
                        }
                    }

                    if (currentZ == maxZ)
                    {
                        return true;
                    }

                    if (currentZsector > 0)
                    {
                        var currentSector = room.Sectors[currentXsector, currentZsector - 1];
                        var nextSector = room.Sectors[currentXsector, currentZsector];

                        if ((Clicks.FromWorld(currentSector.Floor.XnZp, RoundingMethod.Integer) + Clicks.FromWorld(currentSector.Floor.XpZp, RoundingMethod.Integer)) / 2 > currentYclick ||
                            (Clicks.FromWorld(currentSector.Ceiling.XnZp, RoundingMethod.Integer) + Clicks.FromWorld(currentSector.Ceiling.XpZp, RoundingMethod.Integer)) / 2 < currentYclick ||
                            currentSector.Type == SectorType.Wall ||
                            (Clicks.FromWorld(nextSector.Floor.XpZn, RoundingMethod.Integer) + Clicks.FromWorld(nextSector.Floor.XnZn, RoundingMethod.Integer)) / 2 > currentYclick ||
                            (Clicks.FromWorld(nextSector.Ceiling.XpZn, RoundingMethod.Integer) + Clicks.FromWorld(nextSector.Ceiling.XnZn, RoundingMethod.Integer)) / 2 < currentYclick ||
                            nextSector.Type == SectorType.Wall)
                        {
                            return false;
                        }
                    }
                }

                currentZ += (int)Level.SectorSizeUnit;
                currentX += (deltaX << 10) / (deltaZ + 1);
                currentY += (deltaY << 10) / (deltaZ + 1);
            }
            while (currentZ <= maxZ);

            return true;
        }

        private static bool LightRayTrace(Room room, Vector3 position, Vector3 lightPosition)
        {
            return !(
            RayTraceCheckFloorCeiling(room, (int)position.X, (int)position.Y, (int)position.Z, (int)lightPosition.X, (int)lightPosition.Z) &&
            RayTraceX(room, (int)position.X, (int)position.Y, (int)position.Z, (int)lightPosition.X, (int)lightPosition.Y, (int)lightPosition.Z) &&
            RayTraceZ(room, (int)position.X, (int)position.Y, (int)position.Z, (int)lightPosition.X, (int)lightPosition.Y, (int)lightPosition.Z));
        }

        private static int GetLightSampleCount(LightInstance light, LightQuality defaultQuality = LightQuality.Low)
        {
            LightQuality quality = light.Quality == LightQuality.Default ? defaultQuality : light.Quality;

            return quality switch
            {
                LightQuality.Medium => 3,
                LightQuality.High => 5,
                _ => 1
            };
        }

        private static float GetSampleSumFromLightTracing(int numSamples, Room room, Vector3 position, LightInstance light)
        {
            // Fast path for single-sample case
            if (numSamples == 1)
            {
                if (!light.IsObstructedByRoomGeometry)
                    return 1.0f;

                return LightRayTrace(room, position, light.Position) ? 0.0f : 1.0f;
            }

            // Multi-sample: grid of XZ offsets around the light position (Y fixed at 0)
            int halfSamples = numSamples / 2;
            float sampleSum = 0.0f;

            for (int sx = -halfSamples; sx <= halfSamples; sx++)
            {
                for (int sz = -halfSamples; sz <= halfSamples; sz++)
                {
                    if (!light.IsObstructedByRoomGeometry)
                    {
                        sampleSum++;
                        continue;
                    }

                    var sampleOffset = new Vector3(sx * 256, 0, sz * 256);

                    if (!LightRayTrace(room, position, light.Position + sampleOffset))
                        sampleSum++;
                }
            }

            return sampleSum / (numSamples * numSamples);
        }

        public static float GetRaytraceResult(Room room, LightInstance light, Vector3 position, bool highQuality)
        {
            if (light.Type is LightType.Effect or LightType.FogBulb)
                return 1.0f;

            int numSamples = highQuality
                ? GetLightSampleCount(light, room.Level.Settings.DefaultLightQuality)
                : 1;

            float result = GetSampleSumFromLightTracing(numSamples, room, position, light);
            return result < 0.000001f ? 0.0f : result;
        }

        public static Vector3 CalculateLightForVertex(Room room, LightInstance light, Vector3 position,
            Vector3 normal, bool legacyPointLightModel, bool highQuality)
        {
            if (!light.Enabled)
                return Vector3.Zero;

            return light.Type switch
            {
                LightType.Point or LightType.Shadow => CalculatePointLight(room, light, position, normal, legacyPointLightModel, highQuality),
                LightType.Effect => CalculateEffectLight(light, position),
                LightType.Sun => CalculateSunLight(room, light, position, normal, highQuality),
                LightType.Spot => CalculateSpotLight(room, light, position, normal, highQuality),
                _ => Vector3.Zero
            };
        }

        private const float IntensityScale = 8192.0f;
        private const float ColorNormalization = 1.0f / 64.0f;
        private const float DistanceMargin = 64.0f;

        private static Vector3 CalculatePointLight(Room room, LightInstance light, Vector3 position,
            Vector3 normal, bool legacyPointLightModel, bool highQuality)
        {
            Vector3 lightVector = position - light.Position;
            float distance = lightVector.Length();
            float outerRadius = light.OuterRange * Level.SectorSizeUnit;

            if (distance + DistanceMargin > outerRadius)
                return Vector3.Zero;

            lightVector = Vector3.Normalize(lightVector);

            // Distance-based attenuation: 1.0 at inner range, fading to 0.0 at outer range
            float innerRadius = light.InnerRange * Level.SectorSizeUnit;
            float rangeDelta = outerRadius - innerRadius;
            float attenuation = rangeDelta > 0 ? Math.Clamp((outerRadius - distance) / rangeDelta, 0.0f, 1.0f) : 1.0f;

            if (attenuation <= 0.0f)
                return Vector3.Zero;

            // Diffuse factor: dot product of surface normal with light direction
            Vector3 effectiveDirection = legacyPointLightModel ? normal : -lightVector;
            float dotN = Vector3.Dot(effectiveDirection, normal);

            if (dotN <= 0)
                return Vector3.Zero;

            float raytraceResult = GetRaytraceResult(room, light, position, highQuality);
            int diffuse = (int)(light.Intensity * IntensityScale);
            float finalIntensity = Math.Max(0, dotN * attenuation * raytraceResult) * diffuse;

            return finalIntensity * light.Color * ColorNormalization;
        }

        private static Vector3 CalculateEffectLight(LightInstance light, Vector3 position)
        {
            float distance = Math.Abs(Vector3.Distance(position, light.Position));

            if (distance + DistanceMargin > light.OuterRange * Level.SectorSizeUnit)
                return Vector3.Zero;

            // Effect lights only apply to vertices at the sector corners surrounding the light
            // TODO: winroomedit was supporting effect lights placed on vertical faces
            int x1 = (int)(Math.Floor(light.Position.X / Level.SectorSizeUnit) * Level.SectorSizeUnit);
            int z1 = (int)(Math.Floor(light.Position.Z / Level.SectorSizeUnit) * Level.SectorSizeUnit);
            int x2 = (int)(Math.Ceiling(light.Position.X / Level.SectorSizeUnit) * Level.SectorSizeUnit);
            int z2 = (int)(Math.Ceiling(light.Position.Z / Level.SectorSizeUnit) * Level.SectorSizeUnit);

            bool isAtCorner = (position.X == x1 || position.X == x2)
                           && (position.Z == z1 || position.Z == z2);

            if (!isAtCorner || position.Y > light.Position.Y)
                return Vector3.Zero;

            float finalIntensity = light.Intensity * IntensityScale * 0.25f;
            return finalIntensity * light.Color * ColorNormalization;
        }

        private static Vector3 CalculateSunLight(Room room, LightInstance light, Vector3 position,
            Vector3 normal, bool highQuality)
        {
            Vector3 lightDirection = light.GetDirection();
            float diffuse = Math.Clamp(-Vector3.Dot(lightDirection, normal), 0.0f, 1.0f);

            if (diffuse <= 0)
                return Vector3.Zero;

            float raytraceResult = GetRaytraceResult(room, light, position, highQuality);
            float finalIntensity = diffuse * light.Intensity * IntensityScale * raytraceResult;

            if (finalIntensity < 0)
                return Vector3.Zero;

            return finalIntensity * light.Color * ColorNormalization;
        }

        private static Vector3 CalculateSpotLight(Room room, LightInstance light, Vector3 position,
            Vector3 normal, bool highQuality)
        {
            float outerRadius = light.OuterRange * Level.SectorSizeUnit;
            float distance = (position - light.Position).Length();

            if (distance + DistanceMargin > outerRadius)
                return Vector3.Zero;

            Vector3 lightVector = Vector3.Normalize(position - light.Position);
            Vector3 lightDirection = light.GetDirection();

            // Cone angle check
            double cosAngle = Vector3.Dot(lightVector, lightDirection);
            double cosInner = Math.Cos(light.InnerAngle * (Math.PI / 180));
            double cosOuter = Math.Cos(light.OuterAngle * (Math.PI / 180));

            if (cosAngle < cosOuter)
                return Vector3.Zero;

            // Cone angular attenuation: 1.0 inside inner cone, fading to 0.0 at outer cone
            double coneDelta = cosOuter - cosInner;
            float coneFactor = coneDelta != 0 ? Math.Clamp((float)(1.0 - ((cosAngle - cosInner) / coneDelta)), 0.0f, 1.0f) : 1.0f;

            if (coneFactor <= 0.0f)
                return Vector3.Zero;

            // Distance attenuation
            float innerRadius = light.InnerRange * Level.SectorSizeUnit;
            float rangeDelta = outerRadius - innerRadius;
            float distAttenuation = 1.0f;

            if (distance >= innerRadius && rangeDelta > 0)
                distAttenuation = Math.Clamp(1.0f - ((distance - innerRadius) / rangeDelta), 0.0f, 1.0f);

            if (distAttenuation <= 0.0f)
                return Vector3.Zero;

            // Surface facing factor
            float surfaceDot = Math.Clamp(-Vector3.Dot(lightDirection, normal), 0.0f, 1.0f);

            if (surfaceDot <= 0.0f)
                return Vector3.Zero;

            float raytraceResult = GetRaytraceResult(room, light, position, highQuality);
            float finalIntensity = distAttenuation * surfaceDot * coneFactor * light.Intensity * IntensityScale * raytraceResult;

            return finalIntensity * light.Color * ColorNormalization;
        }

        public void Relight(Room room, bool highQuality = false)
        {
            // Pre-filter to only statically used lights (avoids per-vertex checks)
            var lights = new List<LightInstance>();

            foreach (var instance in room.Objects)
            {
                if (instance is LightInstance light && light.IsStaticallyUsed)
                    lights.Add(light);
            }

            // Calculate per-vertex lighting
            for (int i = 0; i < VertexPositions.Count; i += 3)
            {
                Vector3 normal = Vector3.Normalize(Vector3.Cross(
                    VertexPositions[i + 1] - VertexPositions[i],
                    VertexPositions[i + 2] - VertexPositions[i]));

                for (int j = 0; j < 3; ++j)
                {
                    Vector3 position = VertexPositions[i + j];
                    Vector3 color = room.Properties.AmbientLight * 128;

                    foreach (var light in lights)
                        color += CalculateLightForVertex(room, light, position, normal, true, highQuality);

                    VertexColors[i + j] = Vector3.Max(color, Vector3.Zero) * (1.0f / 128.0f);
                }
            }

            // Average colors across shared vertices for smooth lighting
            foreach (var pair in SharedVertices)
            {
                Vector3 averageColor = Vector3.Zero;

                foreach (int vertexIndex in pair.Value)
                    averageColor += VertexColors[vertexIndex];

                averageColor /= pair.Value.Count;

                foreach (int vertexIndex in pair.Value)
                    VertexColors[vertexIndex] = averageColor;
            }
        }

        public struct IntersectionInfo
        {
            public VectorInt2 Pos;
            public SectorFace Face;
            public float Distance;
            public float VerticalCoord;
        }

        public IntersectionInfo? RayIntersectsGeometry(Ray ray)
        {
            var result = new IntersectionInfo { Distance = float.NaN };

            foreach (var entry in VertexRangeLookup)
            {
                for (int i = 0; i < entry.Value.Count; i += 3)
                {
                    Vector3 p0 = VertexPositions[entry.Value.Start + i];
                    Vector3 p1 = VertexPositions[entry.Value.Start + i + 1];
                    Vector3 p2 = VertexPositions[entry.Value.Start + i + 2];

                    if (!Collision.RayIntersectsTriangle(ray, p0, p1, p2, true, out Vector3 hitPosition))
                        continue;

                    float distance = (hitPosition - ray.Position).Length();
                    Vector3 normal = Vector3.Cross(p1 - p0, p2 - p0);

                    // Only consider front-facing triangles that are closer than current best
                    if (Vector3.Dot(ray.Direction, normal) <= 0 && !(distance > result.Distance))
                    {
                        result = new IntersectionInfo
                        {
                            Distance = distance,
                            Face = entry.Key.Face,
                            Pos = entry.Key.Position,
                            VerticalCoord = hitPosition.Y
                        };
                    }
                }
            }

            return float.IsNaN(result.Distance) ? null : result;
        }
    }

    public struct VertexRange
    {
        public int Start;
        public int Count;

        public VertexRange(int start, int count)
        {
            Start = start;
            Count = count;
        }
    }
}
