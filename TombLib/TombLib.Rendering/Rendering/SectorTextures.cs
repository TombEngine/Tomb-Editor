using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;
using TombLib.LevelData.SectorStructs;

namespace TombLib.Rendering
{
    public enum SectorTexture
    {
        None,
        arrow_up,
        arrow_down,
        arrow_up_down,
        arrow_left,
        arrow_right,
        arrow_ne,
        arrow_nw,
        arrow_se,
        arrow_sw,
        arrow_ne_se,
        arrow_nw_se,
        arrow_nw_sw,
        arrow_sw_ne,
        cross,
        illegal_slope,
        slide_east,
        slide_east_flip,
        slide_north,
        slide_north_flip,
        slide_south,
        slide_south_flip,
        slide_west,
        slide_west_flip,
        texture_coord_out_of_bounds,
        texture_unavailable
    }

    public enum SelectionType
    {
        None,
        Selected,
        Highlight
    }

    public delegate SectorTextureResult SectorTextureGetDelegate(Room room, int x, int z, SectorFace face);

    public struct SectorTextureResult
    {
        public Vector4 Color;
        public Vector4 Overlay;
        public SectorTexture SectorTexture;
        public bool Selected;
        public bool Highlighted;
        public bool Dimmed;
        public bool Hidden;
    }

    public class SectorTextureDefault
    {
        public static SectorTextureDefault Default { get; } = new SectorTextureDefault();
        public SectorColoringInfo ColoringInfo = SectorColoringInfo.Default;
        public RectangleInt2 SelectionArea = new RectangleInt2(-1, -1, -1, -1);
        public RectangleInt2 HighlightArea = new RectangleInt2(-1, -1, -1, -1);
        public ArrowType SelectionArrow = ArrowType.EntireFace;
        public bool ProbeAttributesThroughPortals = true;
        public bool DrawIllegalSlopes = true;
        public bool DrawSlideDirections = true;
        public bool HideHiddenRooms = true;

        public static readonly HashSet<SectorColoringType> IgnoredHighlights = new HashSet<SectorColoringType>
        {
            SectorColoringType.Portal,
            SectorColoringType.BorderWall,
            SectorColoringType.Wall,
            SectorColoringType.Beetle,
            SectorColoringType.TriggerTriggerer
        };

        public static readonly HashSet<SectorColoringType> IgnoredHighlightsForFloor = new HashSet<SectorColoringType>
        {
            SectorColoringType.Portal,
            SectorColoringType.CeilingPortal,
            SectorColoringType.BorderWall,
            SectorColoringType.Wall,
            SectorColoringType.Beetle,
            SectorColoringType.TriggerTriggerer
        };

        public static readonly HashSet<SectorColoringType> IgnoredHighlightsForCeiling = new HashSet<SectorColoringType>
        {
            SectorColoringType.Portal,
            SectorColoringType.FloorPortal,
            SectorColoringType.BorderWall,
            SectorColoringType.Wall,
            SectorColoringType.Beetle,
            SectorColoringType.TriggerTriggerer
        };

        public static readonly List<SectorColoringShape> UsedShapes = new List<SectorColoringShape>
        {
            SectorColoringShape.Rectangle
        };

        public SectorTextureResult Get(Room room, int x, int z, SectorFace face)
        {
            SectorTexture SectorTexture = SectorTexture.None;
            Vector4 Color;
            Vector4 Overlay = new Vector4();
            bool Dimmed = false;

            // Choose base color
            if (face.IsFloorWall())
            {
                Color = ColoringInfo.SectorColorScheme.ColorWallLower;

                if (room.Sectors[x, z].WallPortal != null)
                    Color = ColoringInfo.SectorColorScheme.ColorPortalFace;
            }
            else if (face.IsMiddleWall())
            {
                Color = ColoringInfo.SectorColorScheme.ColorWall;

                if (room.Sectors[x, z].WallPortal != null)
                    Color = ColoringInfo.SectorColorScheme.ColorPortalFace;
            }
            else if (face.IsCeilingWall())
            {
                Color = ColoringInfo.SectorColorScheme.ColorWallUpper;

                if (room.Sectors[x, z].WallPortal != null)
                    Color = ColoringInfo.SectorColorScheme.ColorPortalFace;
            }
            else if (face.IsFloor())
            {
                // For now, we only render rectangular solid highlights, so use single rectangle solid shape in UsedShapes list, and use first and only entry in returned highlight list.
                var currentHighlights = ColoringInfo.GetColors(ColoringInfo.SectorColorScheme, room, x, z, ProbeAttributesThroughPortals, IgnoredHighlightsForFloor, UsedShapes);

                if (currentHighlights != null)
                    Color = currentHighlights[0].Color;
                else
                    Color = ColoringInfo.SectorColorScheme.ColorFloor;

                if (room.Sectors[x, z].Floor.DiagonalSplit != DiagonalSplit.None)
                {
                    if ((room.Sectors[x, z].Floor.DiagonalSplit > DiagonalSplit.XpZp && face == SectorFace.Floor) ||
                        (room.Sectors[x, z].Floor.DiagonalSplit <= DiagonalSplit.XpZp && face == SectorFace.Floor_Triangle2))
                        Dimmed = true;
                }
            }
            else if (face.IsCeiling())
            {
                // For now, we only render rectangular solid highlights, so use single rectangle solid shape in UsedShapes list, and use first and only entry in returned highlight list.
                var currentHighlights2 = ColoringInfo.GetColors(ColoringInfo.SectorColorScheme, room, x, z, ProbeAttributesThroughPortals, IgnoredHighlightsForCeiling, UsedShapes);

                if (currentHighlights2 != null)
                    Color = currentHighlights2[0].Color;
                else
                    Color = ColoringInfo.SectorColorScheme.ColorFloor;

                if (room.Sectors[x, z].Ceiling.DiagonalSplit != DiagonalSplit.None)
                {
                    if ((room.Sectors[x, z].Ceiling.DiagonalSplit > DiagonalSplit.XpZp && face == SectorFace.Ceiling) ||
                        (room.Sectors[x, z].Ceiling.DiagonalSplit <= DiagonalSplit.XpZp && face == SectorFace.Ceiling_Triangle2))
                        Dimmed = true;
                }
            }
            else
                throw new ArgumentOutOfRangeException($"Unknown {nameof(SectorFace)} encountered.");

            // Draw climbable walls
            Direction direction = face.GetDirection();
            RoomSectorPair lookupSector;

            switch (direction)
            {
                case Direction.PositiveZ:
                    lookupSector = room.ProbeLowestSector(x, z + 1, ProbeAttributesThroughPortals);
                    if (lookupSector.Sector != null && lookupSector.Sector.HasFlag(SectorFlags.ClimbNegativeZ))
                        Color = ColoringInfo.SectorColorScheme.ColorClimb;
                    break;
                case Direction.PositiveX:
                    lookupSector = room.ProbeLowestSector(x + 1, z, ProbeAttributesThroughPortals);
                    if (lookupSector.Sector != null && lookupSector.Sector.HasFlag(SectorFlags.ClimbNegativeX))
                        Color = ColoringInfo.SectorColorScheme.ColorClimb;
                    break;
                case Direction.NegativeZ:
                    lookupSector = room.ProbeLowestSector(x, z - 1, ProbeAttributesThroughPortals);
                    if (lookupSector.Sector != null && lookupSector.Sector.HasFlag(SectorFlags.ClimbPositiveZ))
                        Color = ColoringInfo.SectorColorScheme.ColorClimb;
                    break;
                case Direction.NegativeX:
                    lookupSector = room.ProbeLowestSector(x - 1, z, ProbeAttributesThroughPortals);
                    if (lookupSector.Sector != null && lookupSector.Sector.HasFlag(SectorFlags.ClimbPositiveX))
                        Color = ColoringInfo.SectorColorScheme.ColorClimb;
                    break;
            }

            // Draw slopes
            if (DrawSlideDirections)
                if (face == SectorFace.Floor || face == SectorFace.Floor_Triangle2)
                {
                    var method = room.Level.IsTombEngine ? Sector.SlopeCalculationMode.TombEngine : Sector.SlopeCalculationMode.Legacy;
                    var slopeDirection = room.Sectors[x, z].GetFloorTriangleSlopeDirections(method)[face == SectorFace.Floor ? 0 : 1];
                    bool flipped = room.Sectors[x, z].Floor.SplitDirectionIsXEqualsZ;
                    switch (slopeDirection)
                    {
                        case Direction.PositiveX:
                            SectorTexture = flipped ? SectorTexture.slide_east_flip : SectorTexture.slide_east;
                            break;
                        case Direction.NegativeX:
                            SectorTexture = flipped ? SectorTexture.slide_west_flip : SectorTexture.slide_west;
                            break;
                        case Direction.PositiveZ:
                            SectorTexture = flipped ? SectorTexture.slide_north_flip : SectorTexture.slide_north;
                            break;
                        case Direction.NegativeZ:
                            SectorTexture = flipped ? SectorTexture.slide_south_flip : SectorTexture.slide_south;
                            break;
                    }

                    if(slopeDirection != Direction.None)
                        Overlay = ColoringInfo.SectorColorScheme.ColorSlideDirection;
                }

            // Draw illegal slopes
            if (DrawIllegalSlopes)
                if (face == SectorFace.Floor || face == SectorFace.Floor_Triangle2)
                    if (room.IsIllegalSlope(x, z))
                    {
                        SectorTexture = SectorTexture.illegal_slope;
                        Overlay = ColoringInfo.SectorColorScheme.ColorIllegalSlope;
                    }

            // Draw selection
            if (SelectionArea.Contains(new VectorInt2(x, z)))
            {
                SectorTexture = SectorTexture.None;
                Color = ColoringInfo.SectorColorScheme.ColorSelection; // Selection color
                Overlay = Color; // Overlay is the same as color if sector is selected

                if (face.IsFloor())
                {
                    switch (SelectionArrow)
                    {
                        case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_up; break;
                        case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_right; break;
                        case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_down; break;
                        case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_left; break;
                        case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_nw; break;
                        case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_ne; break;
                        case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_se; break;
                        case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_sw; break;
                    }
                }
                else if (face.IsCeiling())
                {
                    switch (SelectionArrow)
                    {
                        case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_up; break;
                        case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_right; break;
                        case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_down; break;
                        case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_left; break;
                        case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_nw; break;
                        case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_ne; break;
                        case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_se; break;
                        case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_sw; break;
                    }
                }
                else
                {
                    SectorFaceType faceType = face.GetFaceType();

                    switch (direction)
                    {
                        case Direction.PositiveZ:
                            switch (faceType)
                            {
                                case SectorFaceType.Floor:
                                    switch (SelectionArrow)
                                    {
                                        case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_up; break;
                                        case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_nw; break;
                                        case ArrowType.EdgeS: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_ne; break;
                                        case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_ne; break;
                                        case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_nw; break;
                                        case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                                    }
                                    break;

                                case SectorFaceType.Ceiling:
                                    switch (SelectionArrow)
                                    {
                                        case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_down; break;
                                        case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_sw; break;
                                        case ArrowType.EdgeS: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_se; break;
                                        case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_se; break;
                                        case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_sw; break;
                                        case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                                    }
                                    break;

                                case SectorFaceType.Wall:
                                    switch (SelectionArrow)
                                    {
                                        case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_up_down; break;
                                        case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                        case ArrowType.EdgeS: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_ne_se; break;
                                        case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_ne_se; break;
                                        case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                        case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                                    }
                                    break;
                            }
                            break;

                        case Direction.PositiveX:
                            switch (faceType)
                            {
                                case SectorFaceType.Floor:
                                    switch (SelectionArrow)
                                    {
                                        case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_ne; break;
                                        case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_up; break;
                                        case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_nw; break;
                                        case ArrowType.EdgeW: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_ne; break;
                                        case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_nw; break;
                                        case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                                    }
                                    break;

                                case SectorFaceType.Ceiling:
                                    switch (SelectionArrow)
                                    {
                                        case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_se; break;
                                        case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_down; break;
                                        case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_sw; break;
                                        case ArrowType.EdgeW: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_se; break;
                                        case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_sw; break;
                                        case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                                    }
                                    break;

                                case SectorFaceType.Wall:
                                    switch (SelectionArrow)
                                    {
                                        case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_ne_se; break;
                                        case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_up_down; break;
                                        case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                        case ArrowType.EdgeW: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_ne_se; break;
                                        case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                        case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                                    }
                                    break;
                            }
                            break;

                        case Direction.NegativeZ:
                            switch (faceType)
                            {
                                case SectorFaceType.Floor:
                                    switch (SelectionArrow)
                                    {
                                        case ArrowType.EdgeN: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_ne; break;
                                        case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_up; break;
                                        case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_nw; break;
                                        case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_ne; break;
                                        case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_nw; break;
                                    }
                                    break;

                                case SectorFaceType.Ceiling:
                                    switch (SelectionArrow)
                                    {
                                        case ArrowType.EdgeN: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_se; break;
                                        case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_down; break;
                                        case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_sw; break;
                                        case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_se; break;
                                        case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_sw; break;
                                    }
                                    break;

                                case SectorFaceType.Wall:
                                    switch (SelectionArrow)
                                    {
                                        case ArrowType.EdgeN: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_ne_se; break;
                                        case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_up_down; break;
                                        case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                        case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_ne_se; break;
                                        case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                    }
                                    break;
                            }
                            break;

                        case Direction.NegativeX:
                            switch (faceType)
                            {
                                case SectorFaceType.Floor:
                                    switch (SelectionArrow)
                                    {
                                        case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_nw; break;
                                        case ArrowType.EdgeE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_ne; break;
                                        case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_up; break;
                                        case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_nw; break;
                                        case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_ne; break;
                                    }
                                    break;

                                case SectorFaceType.Ceiling:
                                    switch (SelectionArrow)
                                    {
                                        case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_sw; break;
                                        case ArrowType.EdgeE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_se; break;
                                        case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_down; break;
                                        case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_sw; break;
                                        case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_se; break;
                                    }
                                    break;

                                case SectorFaceType.Wall:
                                    switch (SelectionArrow)
                                    {
                                        case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                        case ArrowType.EdgeE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_ne_se; break;
                                        case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_up_down; break;
                                        case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                        case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                                        case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_ne_se; break;
                                    }
                                    break;
                            }
                            break;

                        case Direction.Diagonal:
                            switch (faceType)
                            {
                                case SectorFaceType.Floor:
                                    switch (room.Sectors[x, z].Floor.DiagonalSplit)
                                    {
                                        case DiagonalSplit.XnZp: // OK
                                            switch (SelectionArrow)
                                            {
                                                case ArrowType.EdgeN:
                                                case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_ne; break;
                                                case ArrowType.EdgeS:
                                                case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_nw; break;
                                                case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_ne; break;
                                                case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_nw; break;
                                            }
                                            break;

                                        case DiagonalSplit.XnZn: // OK
                                            switch (SelectionArrow)
                                            {
                                                case ArrowType.EdgeS:
                                                case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_nw; break;
                                                case ArrowType.EdgeN:
                                                case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_ne; break;
                                                case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_ne; break;
                                                case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_nw; break;
                                                case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                                            }
                                            break;

                                        case DiagonalSplit.XpZn: // OK
                                            switch (SelectionArrow)
                                            {
                                                case ArrowType.EdgeN:
                                                case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_nw; break;
                                                case ArrowType.EdgeS:
                                                case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_ne; break;
                                                case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_nw; break;
                                                case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_ne; break;
                                            }
                                            break;

                                        case DiagonalSplit.XpZp: // OK
                                            switch (SelectionArrow)
                                            {
                                                case ArrowType.EdgeN:
                                                case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_nw; break;
                                                case ArrowType.EdgeS:
                                                case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_ne; break;
                                                case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_nw; break;
                                                case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_ne; break;
                                                case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                                            }
                                            break;
                                    }
                                    break;

                                case SectorFaceType.Ceiling:
                                    switch (room.Sectors[x, z].Floor.DiagonalSplit)
                                    {
                                        case DiagonalSplit.XnZp: // OK
                                            switch (SelectionArrow)
                                            {
                                                case ArrowType.EdgeN:
                                                case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_se; break;
                                                case ArrowType.EdgeS:
                                                case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_sw; break;
                                                case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_se; break;
                                                case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_sw; break;
                                            }
                                            break;

                                        case DiagonalSplit.XnZn: // OK
                                            switch (SelectionArrow)
                                            {
                                                case ArrowType.EdgeS:
                                                case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_sw; break;
                                                case ArrowType.EdgeN:
                                                case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_se; break;
                                                case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_se; break;
                                                case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_sw; break;
                                                case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                                            }
                                            break;

                                        case DiagonalSplit.XpZn: // OK
                                            switch (SelectionArrow)
                                            {
                                                case ArrowType.EdgeN:
                                                case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_sw; break;
                                                case ArrowType.EdgeS:
                                                case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_se; break;
                                                case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_sw; break;
                                                case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_se; break;
                                            }
                                            break;

                                        case DiagonalSplit.XpZp: // OK
                                            switch (SelectionArrow)
                                            {
                                                case ArrowType.EdgeN:
                                                case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_sw; break;
                                                case ArrowType.EdgeS:
                                                case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_se; break;
                                                case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_sw; break;
                                                case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_se; break;
                                                case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                                            }
                                            break;
                                    }
                                    break;

                                case SectorFaceType.Wall:
                                    switch (room.Sectors[x, z].Floor.DiagonalSplit)
                                    {
                                        case DiagonalSplit.XnZp: // OK
                                            switch (SelectionArrow)
                                            {
                                                case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_ne_se; break;
                                                case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_ne_se; break;
                                                case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                                case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                                case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_ne_se; break;
                                                case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                            }
                                            break;

                                        case DiagonalSplit.XnZn: //OK
                                            switch (SelectionArrow)
                                            {
                                                case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_ne_se; break;
                                                case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                                case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                                case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_ne_se; break;
                                                case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_ne_se; break;
                                                case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                                case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                                            }
                                            break;

                                        case DiagonalSplit.XpZn: // OK
                                            switch (SelectionArrow)
                                            {
                                                case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                                case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                                case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_ne_se; break;
                                                case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_ne_se; break;
                                                case ArrowType.CornerNW: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerNE: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                                case ArrowType.CornerSE: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerSW: SectorTexture = SectorTexture.arrow_ne_se; break;
                                            }
                                            break;

                                        case DiagonalSplit.XpZp: // OK
                                            switch (SelectionArrow)
                                            {
                                                case ArrowType.EdgeN: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                                case ArrowType.EdgeE: SectorTexture = SectorTexture.arrow_ne_se; break;
                                                case ArrowType.EdgeS: SectorTexture = SectorTexture.arrow_ne_se; break;
                                                case ArrowType.EdgeW: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                                case ArrowType.CornerNW: SectorTexture = SectorTexture.arrow_nw_sw; break;
                                                case ArrowType.CornerNE: SectorTexture = SectorTexture.cross; break;
                                                case ArrowType.CornerSE: SectorTexture = SectorTexture.arrow_ne_se; break;
                                                case ArrowType.CornerSW: SectorTexture = SectorTexture.cross; break;
                                            }
                                            break;
                                    }
                                    break;
                            }
                            break;
                    }
                }
            }

            return new SectorTextureResult
            {
                Color = Color,
                Overlay = Overlay,
                SectorTexture = SectorTexture,
                Dimmed = Dimmed,
                Hidden = room.Properties.Hidden && HideHiddenRooms,
                Selected = (SelectionArea.Contains(new VectorInt2(x, z))),
                Highlighted = (HighlightArea.Contains(new VectorInt2(x, z)))
            };
        }
    }
}
