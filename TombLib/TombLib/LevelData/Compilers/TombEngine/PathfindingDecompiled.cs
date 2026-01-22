using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.Compilers.TombEngine
{
    /*
     * =============================================================================================
     * TOMBENGINE PATHFINDING SYSTEM - BOX AND OVERLAP GENERATION
     * =============================================================================================
     *
     * OVERVIEW:
     * ---------
     * The pathfinding system divides walkable floor areas into rectangular "boxes".
     * Boxes are connected via "overlaps" which define how enemies can move between them.
     * The game engine uses this pre-computed data for efficient AI navigation.
     *
     * DATA STRUCTURES:
     * ----------------
     * - Box: A rectangular walkable floor region with uniform height
     * - Overlap: A connection between two boxes with movement capability flags
     * - Zone: Groups of boxes reachable by a specific enemy type (computed in Pathfinding.cs)
     *
     * COORDINATE SYSTEM:
     * ------------------
     * TombEditor uses X,Z horizontal coordinates with Z=0 at bottom (increasing upward).
     * Vertical (height) uses Y axis with negative values being higher (TR convention).
     *
     * ALGORITHM OVERVIEW:
     * -------------------
     * 1. For each sector in each room, try to create/expand a box (spiral expansion)
     * 2. Deduplicate boxes (same bounds + height + water = same box)
     * 3. For each pair of boxes, check if they overlap/connect
     * 4. Store overlap connections with capability flags (jump, monkey swing)
     * =============================================================================================
     */

    /// <summary>
    /// Auxiliary box structure used during pathfinding generation.
    /// This is the internal working format before conversion to TombEngineBox.
    /// </summary>
    public class dec_TombEngine_box_aux
    {
        // =========================================================================================
        // BOUNDS (in world sector coordinates)
        // =========================================================================================
        public int Zmin;           // Bottom edge (minimum Z, corresponds to "top" in original)
        public int Zmax;           // Top edge (maximum Z, corresponds to "bottom" in original)
        public int Xmin;           // Left edge (minimum X)
        public int Xmax;           // Right edge (maximum X)

        // =========================================================================================
        // FLOOR PROPERTIES
        // =========================================================================================
        public int Height;         // Average floor height (room.Position.Y + average of 4 corners)
        public int OverlapIndex;   // Index into overlap array (-1 if no overlaps)
        public bool Slope;         // True if floor is too steep for most enemies (gradient >= 3 clicks)

        // =========================================================================================
        // BOX FLAGS
        // =========================================================================================
        public bool Splitter;      // Box is a "splitter" (1x1 box at SPLITTER)
        public bool NotWalkableBox;// Box is marked as not walkable
        public bool Monkey;        // Box has monkey swing ceiling (MONKEY)
        public bool Jump;          // Box requires jumping to reach (set during overlap check)
        public bool Water;         // Box is in a water room
        public bool Shallow;       // Box is in shallow water (water depth <= 1 click)

        // =========================================================================================
        // FLIP STATE FLAGS
        // =========================================================================================
        // These flags track which room states the box exists in.
        // A box in a non-flipped room has both flags set (exists in both states).
        // A box in a flip pair has only one flag set.
        public bool Unflipped;     // Box exists in normal (unflipped) room state
        public bool Flipped;       // Box exists in alternate (flipped) room state

        // =========================================================================================
        // ROOM REFERENCE
        // =========================================================================================
        public Room Room;          // The room this box belongs to (after floor portal traversal)
    }

    /// <summary>
    /// Partial class containing box and overlap generation code.
    /// </summary>
    public sealed partial class LevelCompilerTombEngine
    {
        public class BoxFlags
        {
            public const int Water		= 0x0200;
            public const int Shallow	= 0x0400;
            public const int Blocked	= 0x4000;
            public const int Splitter	= 0x8000;
        }

        public class OverlapFlags
		{
			public const int Jump = 0x0800;
			public const int Monkey = 0x2000;
            public const int AmphibiousTraversable = 0x4000;
			public const int End = 0x8000;
		}

        // =========================================================================================
        // GLOBAL STATE VARIABLES
        // =========================================================================================
        // These mirror global variables from the original C code.
        // They are used to pass state between functions without explicit parameters.

        /// <summary>
        /// Flag set by Dec_GetHeight when a SPLITTER is encountered.
        /// When true, the current sector must become a 1x1 "splitter" box.
        /// Splitter boxes break up larger boxes to allow fine-grained AI control.
        /// </summary>
        private bool dec_splitter;

        /// <summary>
        /// Flag for shallow water detection.
        /// Set to false when shallow water is detected (water depth &lt;= 1 click).
        /// Shallow water is treated as dry land for pathfinding purposes.
        /// </summary>
        private bool dec_checkUnderwater = true;

        /// <summary>
        /// Flag set by Dec_GetHeight when shallow water is detected.
        /// True when water depth &lt;= 1 click and there's air above.
        /// Used to set the SHALLOW flag (0x0400) on the box.
        /// </summary>
        private bool dec_shallowWater;

        /// <summary>
        /// Flag set by Dec_GetHeight when a MONKEY is encountered.
        /// Indicates the sector has a monkey swing ceiling.
        /// Boxes with monkey swing get the MONKEY_BOX_FLAG.
        /// </summary>
        private bool dec_monkey;

        /// <summary>
        /// Current flip state being processed.
        /// False = processing normal rooms, True = processing flipped rooms.
        /// </summary>
        private bool dec_flipped;

        /// <summary>
        /// Flag set by Dec_CheckOverlap when boxes are connected via a jump.
        /// Used to set the JUMP_BIT (0x0800) flag on the overlap.
        /// </summary>
        private bool dec_jump;

        /// <summary>
        /// Current room being processed.
        /// Updated by Dec_ClampRoom and Dec_GetHeight as they traverse portals.
        /// </summary>
        private Room dec_room;

        /// <summary>
        /// Floor corner heights for the current sector.
        /// Used to detect slopes and calculate average floor height.
        /// Naming: dec_cornerHeight[1-4] corresponds to XnZp, XpZp, XpZn, XnZn corners.
        /// </summary>
        private int dec_cornerHeight1 = Clicks.ToWorld(-1);  // Floor.XnZp (X-, Z+)
        private int dec_cornerHeight2 = Clicks.ToWorld(-1);  // Floor.XpZp (X+, Z+)
        private int dec_cornerHeight3 = Clicks.ToWorld(-1);  // Floor.XpZn (X+, Z-)
        private int dec_cornerHeight4 = Clicks.ToWorld(-1);  // Floor.XnZn (X-, Z-)

        /// <summary>
        /// List of generated boxes (internal format).
        /// Converted to _boxes (TombEngineBox format) after generation.
        /// </summary>
        private List<dec_TombEngine_box_aux> dec_boxes;

        /// <summary>
        /// List of generated overlaps.
        /// Copied directly to _overlaps after generation.
        /// </summary>
        private List<TombEngineOverlap> dec_overlaps;

        /// <summary>
        /// Flag set when traversing through a door/portal.
        /// Used to handle boxes that span multiple rooms.
        /// </summary>
        private bool dec_doorCheck;

        /// <summary>
        /// Sentinel value indicating an invalid/unwalkable height.
        /// Returns from Dec_GetHeight when a sector cannot be walked on.
        /// </summary>
        private const int _noHeight = int.MinValue + byte.MaxValue;


        /// <summary>
        /// Main entry point for box and overlap generation.
        ///
        /// ALGORITHM:
        /// ==========
        /// Pass 1 (flipped=0): Process all base/normal rooms
        /// Pass 2 (flipped=1): Process all alternate/flipped rooms
        ///
        /// For each room, iterate through all non-border sectors and:
        /// 1. Try to create a box starting from that sector (Dec_GetBox)
        /// 2. Add the box to the list if valid, deduplicating (Dec_AddBox)
        /// 3. Store the box index in the sector for runtime lookup
        ///
        /// After all boxes are created, build the overlap connections (Dec_BuildOverlaps).
        ///
        /// NOTE: The original code called FlipAllRooms() to swap room pointers.
        /// Here we use the dec_flipped flag to track state instead.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void Dec_BuildBoxesAndOverlaps()
        {
            dec_room = _level.Rooms[0];
            dec_boxes = new List<dec_TombEngine_box_aux>();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // ===================================================================================
            // BOX GENERATION - Two passes for flip states
            // ===================================================================================
            for (int flipped = 0; flipped < 2; flipped++)
            {
                for (int i = 0; i < _level.Rooms.Length; i++)
                {
                    Room room = _level.Rooms[i];

                    // Filter rooms based on flip state:
                    // - Pass 0 (flipped=0): Process rooms WITHOUT an alternate base (base rooms + non-flip rooms)
                    // - Pass 1 (flipped=1): Process rooms WITH an alternate base (flipped versions)
                    if (room != null && (flipped == 0 && room.AlternateBaseRoom == null || flipped == 1 && room.AlternateBaseRoom != null))
                    {
                        TombEngineRoom tempRoom = _tempRooms[room];

                        // Iterate through all sectors in the room
                        for (int z = 0; z < room.NumZSectors; z++)
                        {
                            for (int x = 0; x < room.NumXSectors; x++)
                            {
                                int boxIndex = -1;  // -1 = no box for this sector

                                // Skip rooms excluded from pathfinding
                                if (!room.Properties.FlagExcludeFromPathFinding)
                                {
                                    dec_TombEngine_box_aux box = new dec_TombEngine_box_aux();

                                    // Skip border sectors (x=0, z=0, x=max, z=max)
                                    // Border sectors are always walls in TR format
                                    if (x != 0 &&
                                        z != 0 &&
                                        x != room.NumXSectors - 1 &&
                                        z != room.NumZSectors - 1 &&
                                        Dec_GetBox(box, x, z, room))  // Try to create/expand a box
                                    {
                                        // Add box to list (may return existing index if duplicate)
                                        boxIndex = Dec_AddBox(box);
                                        if (boxIndex < 0) return;  // Error condition
                                    }
                                }

                                // Store box index in sector for runtime lookup
                                // Sector array is stored as [x * NumZSectors + z]
                                tempRoom.Sectors[tempRoom.NumZSectors * x + z].BoxIndex = boxIndex;
                            }
                        }

                        _tempRooms[room] = tempRoom;
                    }
                }

                // Switch to flipped room processing
                // Original code called FlipAllRooms() which swapped room pointers.
                // We use a flag instead since TombEditor's data model doesn't support room swapping.
                dec_flipped = true;
            }

            dec_flipped = false;

            watch.Stop();
            Console.WriteLine("Dec_BuildBoxesAndOverlaps() -> Build boxes: " + watch.ElapsedMilliseconds + " ms, Count = " + dec_boxes.Count);

            watch.Restart();

            // ===================================================================================
            // OVERLAP GENERATION
            // ===================================================================================
            Dec_BuildOverlaps();

            watch.Stop();
            Console.WriteLine("Dec_BuildBoxesAndOverlaps() -> Build overlaps: " + watch.ElapsedMilliseconds + " ms, Count = " + dec_overlaps.Count);
        }

        /// <summary>
        /// Builds overlap connections between all box pairs.
        ///
        /// OVERLAP DATA FORMAT:
        /// ====================
        /// Overlaps are stored as a flat array. Each box has an OverlapIndex pointing
        /// to the start of its overlap list. The list ends when END_BIT (0x8000) is set.
        ///
        /// OVERLAP FLAGS:
        /// - 0x0800 (JUMP_BIT): Connection requires jumping across a gap
        /// - 0x2000 (MONKEY_BIT): Connection uses monkey swing ceiling
        /// - 0x8000 (END_BIT): Last overlap in this box's list
        ///
        /// FLIP STATE HANDLING:
        /// ====================
        /// Boxes in non-flipped rooms get both Unflipped and Flipped flags set,
        /// allowing them to connect to boxes in either state.
        ///
        /// Boxes in flip pairs only have one flag set, ensuring they only connect
        /// to boxes in the same flip state.
        ///
        /// The algorithm processes overlaps in two passes:
        /// 1. Unflipped pass: Check box pairs where both have Unflipped flag
        /// 2. Flipped pass: Check box pairs where both have Flipped flag (skip if already done)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool Dec_BuildOverlaps()
        {
            dec_overlaps = new List<TombEngineOverlap>();

            int i = 0;
            int j = 0;

            // ===================================================================================
            // PRE-PROCESS: Set flip flags for boxes in non-flipped rooms
            // ===================================================================================
            // Boxes in rooms without flip pairs should be accessible from both flip states.
            // This is a workaround because TombEditor doesn't support FlipAllRooms().
            // In original code, FlipAllRooms() would cause these boxes to be processed twice.
            for (int k = 0; k < dec_boxes.Count; k++)
            {
                if (!_tempRooms[dec_boxes[k].Room].Flipped)
                {
                    dec_boxes[k].Unflipped = true;
                    dec_boxes[k].Flipped = true;
                }
            }

            // ===================================================================================
            // MAIN LOOP: Check all box pairs for overlaps
            // ===================================================================================
            do
            {
                var box1 = dec_boxes[i];
                dec_boxes[i].OverlapIndex = -1;  // Reset overlap index

                int numOverlapsAdded = 0;

                // ===============================================================================
                // PASS 1: Check overlaps in UNFLIPPED state
                // ===============================================================================
                if (box1.Unflipped)
                {
                    if (dec_flipped)
                    {
                        dec_flipped = false;
                    }

                    j = 0;
                    do
                    {
                        if (i != j)  // Don't check box against itself
                        {
                            var box2 = dec_boxes[j];

                            // Only check if box2 also exists in unflipped state
                            if (box2.Unflipped)
                            {
                                if (Dec_CheckOverlap(box1, box2))
                                {
                                    // First overlap for this box - record starting index
                                    if (dec_boxes[i].OverlapIndex == -1)
                                        dec_boxes[i].OverlapIndex = dec_overlaps.Count;

                                    // Create overlap entry
                                    var overlap = new TombEngineOverlap
                                    {
                                        Box = j
                                    };

                                    // Set capability flags based on Dec_CheckOverlap results
                                    if (dec_jump)
                                        overlap.Flags |= OverlapFlags.Jump;   // JUMP_BIT
                                    if (dec_monkey)
                                        overlap.Flags |= OverlapFlags.Monkey;  // MONKEY_BIT

                                    // Set AmphibiousTraversable flag
                                    // Water-Water: always traversable
                                    // Land-Land or Water-Land: traversable if height diff <= 1 click
                                    bool bothWater = (box1.Water || box1.Shallow) && (box2.Water || box2.Shallow);
                                    int heightDiff = Math.Abs(box1.Height - box2.Height);
                                    if (bothWater || heightDiff <= Clicks.ToWorld(1))
                                        overlap.Flags |= OverlapFlags.AmphibiousTraversable;

                                    dec_overlaps.Add(overlap);
                                    numOverlapsAdded++;
                                }
                            }
                        }

                        j++;
                    }
                    while (j < dec_boxes.Count);
                }

                // ===============================================================================
                // PASS 2: Check overlaps in FLIPPED state
                // ===============================================================================
                if (box1.Flipped)
                {
                    if (!dec_flipped)
                    {
                        dec_flipped = true;
                    }

                    j = 0;
                    do
                    {
                        if (i != j)
                        {
                            var box2 = dec_boxes[j];

                            // Only check if box2 also exists in flipped state
                            if (box2.Flipped)
                            {
                                // Skip if already checked in unflipped pass
                                // (both boxes have Unflipped flag = already processed)
                                if (!(box1.Unflipped && box2.Unflipped))
                                {
                                    if (Dec_CheckOverlap(box1, box2))
                                    {
                                        if (dec_boxes[i].OverlapIndex == -1)
                                            dec_boxes[i].OverlapIndex = dec_overlaps.Count;

                                        var overlap = new TombEngineOverlap
                                        {
                                            Box = j
                                        };

                                        if (dec_jump)
                                            overlap.Flags |= OverlapFlags.Jump;
                                        if (dec_monkey)
                                            overlap.Flags |= OverlapFlags.Monkey;

                                        // Set AmphibiousTraversable flag
                                        // Water-Water: always traversable
                                        // Land-Land or Water-Land: traversable if height diff <= 1 click
                                        bool bothWater = box1.Water && box2.Water;
                                        int heightDiff = Math.Abs(box1.Height - box2.Height);
                                        if (bothWater || heightDiff <= Clicks.ToWorld(1))
                                            overlap.Flags |= OverlapFlags.AmphibiousTraversable;

                                        dec_overlaps.Add(overlap);
                                        numOverlapsAdded++;
                                    }
                                }
                            }
                        }

                        j++;
                    }
                    while (j < dec_boxes.Count);
                }

                i++;

                // Mark end of this box's overlap list with END_BIT
                if (numOverlapsAdded != 0)
                    dec_overlaps[dec_overlaps.Count - 1].Flags |= OverlapFlags.End;  // END_BIT
            }
            while (i < dec_boxes.Count);

            dec_flipped = false;

            return true;
        }

        /// <summary>
        /// Adds a box to the box list, with deduplication.
        ///
        /// DEDUPLICATION:
        /// ==============
        /// Boxes are considered duplicates if they have identical:
        /// - Bounds (Xmin, Xmax, Zmin, Zmax)
        /// - Height
        /// - Water flag
        ///
        /// This is critical because the same floor area may be processed multiple times
        /// (from different starting sectors, or during flip state passes).
        ///
        /// SIMD OPTIMIZATION:
        /// ==================
        /// Uses SSE2 SIMD instructions when available to compare multiple values
        /// simultaneously, significantly speeding up the search for duplicates.
        ///
        /// FLIP STATE HANDLING:
        /// ====================
        /// When a duplicate is found during the flipped pass (dec_flipped=true),
        /// the existing box is marked as Flipped, indicating it exists in both states.
        /// </summary>
        /// <param name="box">Box to add</param>
        /// <returns>Index of the box (new or existing duplicate)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private int Dec_AddBox(dec_TombEngine_box_aux box)
        {
            int boxIndex = -1;

            // ===================================================================================
            // SIMD-ACCELERATED DUPLICATE SEARCH
            // ===================================================================================
            if (Sse2.IsSupported)
            {
                // Convert bool to int for SIMD comparison
                int boxWaterInt = box.Water ? 1 : 0;
                int boxShallowInt = box.Shallow ? 1 : 0;

                // Pack search criteria into 128-bit vectors (4 x 32-bit integers)
                var searchBounds = Vector128.Create(box.Xmin, box.Xmax, box.Zmin, box.Zmax);
                var searchExtra = Vector128.Create(box.Height, boxWaterInt, boxShallowInt, 0);

                for (int i = 0; i < dec_boxes.Count; i++)
                {
                    var candidate = dec_boxes[i];

                    // Pack candidate values
                    var candBounds = Vector128.Create(candidate.Xmin, candidate.Xmax, candidate.Zmin, candidate.Zmax);
                    var candExtra = Vector128.Create(candidate.Height, candidate.Water ? 1 : 0, candidate.Shallow ? 1 : 0, 0);

                    // Compare all 4 bounds simultaneously
                    var cmpBounds = Sse2.CompareEqual(searchBounds, candBounds);
                    var cmpExtra = Sse2.CompareEqual(searchExtra, candExtra);

                    // Extract comparison results as bitmasks
                    int maskBounds = Sse2.MoveMask(cmpBounds.AsByte());
                    int maskExtra = Sse2.MoveMask(cmpExtra.AsByte());

                    // All 4 bounds must match (0xFFFF = all 16 bytes equal)
                    // First 3 extra values must match (0x0FFF = first 12 bytes equal)
                    if (maskBounds == 0xFFFF && (maskExtra & 0x0FFF) == 0x0FFF)
                    {
                        boxIndex = i;
                        break;
                    }
                }
            }
            else
            {
                // ===================================================================================
                // SCALAR FALLBACK (non-SSE2 systems)
                // ===================================================================================
                for (int i = 0; i < dec_boxes.Count; i++)
                {
                    if (dec_boxes[i].Xmin == box.Xmin &&
                        dec_boxes[i].Xmax == box.Xmax &&
                        dec_boxes[i].Zmin == box.Zmin &&
                        dec_boxes[i].Zmax == box.Zmax &&
                        dec_boxes[i].Height == box.Height &&
                        dec_boxes[i].Water == box.Water &&
                        dec_boxes[i].Shallow == box.Shallow)
                    {
                        boxIndex = i;
                        break;
                    }
                }
            }

            // ===================================================================================
            // ADD OR UPDATE BOX
            // ===================================================================================
            if (boxIndex == -1)
            {
                // No duplicate found - add as new box
                boxIndex = dec_boxes.Count;
                box.OverlapIndex = -1;
                dec_boxes.Add(box);
            }
            else
            {
                // Duplicate found - update flip state if needed
                if (dec_flipped)
                    dec_boxes[boxIndex].Flipped = true;
            }

            return boxIndex;
        }

        /// <summary>
        /// Creates a box starting from a specific sector, expanding in all 4 directions.
        ///
        /// BOX EXPANSION ALGORITHM (Spiral Expansion):
        /// ============================================
        /// Starting from the initial sector, the algorithm expands outward in all 4 directions
        /// simultaneously until it can no longer expand in any direction.
        ///
        /// Directions are encoded as bits in a direction mask:
        /// - 0x01: Expand Xmin (left, -X direction)
        /// - 0x02: Expand Xmax (right, +X direction)
        /// - 0x04: Expand Zmin (down, -Z direction, "top" in original)
        /// - 0x08: Expand Zmax (up, +Z direction, "bottom" in original)
        ///
        /// Expansion in a direction stops when:
        /// - Floor height changes
        /// - Monkey swing state changes
        /// - Encounter a wall or unwalkable sector
        /// - Cross into a room with different flip state
        ///
        /// SPLITTER BOXES:
        /// ===============
        /// If a SPLITTER is encountered, the box becomes a 1x1 splitter box.
        /// Splitter boxes are used to create fine-grained AI control points.
        ///
        /// GHOST BLOCKS:
        /// =============
        /// Ghost blocks override actual floor geometry for pathfinding purposes.
        /// Used to create invisible walkways or barriers for AI.
        /// </summary>
        /// <param name="box">Box structure to fill</param>
        /// <param name="x">Starting sector X (local to room)</param>
        /// <param name="z">Starting sector Z (local to room)</param>
        /// <param name="theRoom">Room containing the starting sector</param>
        /// <returns>True if a valid box was created, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool Dec_GetBox(dec_TombEngine_box_aux box, int x, int z, Room theRoom)
        {
            bool monkeyInit = false;

            Room room = theRoom;
            Sector sector = room.Sectors[x, z];

            // ===================================================================================
            // INITIAL SECTOR VALIDATION
            // ===================================================================================
            // Check if current sector is not walkable (sector flag)
            if ((sector.Flags & SectorFlags.NotWalkableFloor) != 0) return false;

            // Check for walls and solid portals
            if (sector.Type == SectorType.Wall ||
                sector.Type == SectorType.BorderWall ||
                sector.WallPortal != null && sector.WallPortal.Opacity == PortalOpacity.SolidFaces)
            {
                return false;
            }

            // ===================================================================================
            // GET INITIAL FLOOR HEIGHTS
            // ===================================================================================
            // Use ghost block if present, otherwise use actual floor
            // Ghost blocks override floor geometry for AI purposes
            dec_cornerHeight1 = sector.HasGhostBlock ? sector.GhostBlock.Floor.XnZp : sector.Floor.XnZp;
            dec_cornerHeight2 = sector.HasGhostBlock ? sector.GhostBlock.Floor.XpZp : sector.Floor.XpZp;
            dec_cornerHeight3 = sector.HasGhostBlock ? sector.GhostBlock.Floor.XpZn : sector.Floor.XpZn;
            dec_cornerHeight4 = sector.HasGhostBlock ? sector.GhostBlock.Floor.XnZn : sector.Floor.XnZn;

            // Convert local coordinates to world sector coordinates
            int currentX = room.Position.X + x;
            int currentZ = room.Position.Z + z;

            dec_room = theRoom;

            // Traverse through portals to find the actual room at this position
            Dec_ClampRoom(currentX, currentZ);

            // Reset state flags for Dec_GetHeight
            dec_splitter = false;
            dec_checkUnderwater = true;
            dec_shallowWater = false;
            dec_monkey = false;
            
            // ===================================================================================
            // GET INITIAL FLOOR HEIGHT AND PROPERTIES
            // ===================================================================================
            bool slope;
            int floor = Dec_GetHeight(currentX, currentZ, out slope);
            box.Height = floor;
            box.Slope = slope;

            // Sector is not walkable
            if (floor == _noHeight) return false;

            // Set box room and water state
            box.Room = dec_room;
            box.Water = dec_room.Properties.Type == RoomType.Water;

            // Set flip state flags
            if (dec_flipped)
            {
                box.Flipped = true;
            }
            else
            {
                box.Unflipped = true;
            }

            // Record initial monkey swing state
            if (dec_monkey)
            {
                box.Monkey = true;
                monkeyInit = true;
            }

            // Handle shallow water (water depth <= 1 click)
            // Shallow water is treated as dry land for pathfinding
            if (!dec_checkUnderwater)
            {
                box.Water = false;
            }
            box.Shallow = dec_shallowWater;

            // ===================================================================================
            // SPLITTER BOX - Single sector box
            // ===================================================================================
            if (dec_splitter)
            {
                // SPLITTER encountered - create 1x1 box
                box.Xmin = currentX;
                box.Zmin = currentZ;
                box.Xmax = currentX + 1;
                box.Zmax = currentZ + 1;
                box.Splitter = true;

                return true;
            }
            else
            {
                // ===================================================================================
                // SPIRAL EXPANSION - Expand box in all 4 directions
                // ===================================================================================
                // Set splitter flag to prevent infinite recursion
                // (Dec_GetHeight will now return _noHeight if it hits another TT_SPLITTER)
                dec_splitter = true;

                // Direction bitmask: 0x01=left, 0x02=right, 0x04=down(-Z), 0x08=up(+Z)
                int direction = 0x0f;      // All directions active
                int directionBase = 0x0f;

                // Current box bounds (will be expanded)
                int xMin = currentX;
                int xMax = currentX;
                int zMin = currentZ;
                int zMax = currentZ;

                // Track current room for each expansion direction
                // This allows the box to span multiple connected rooms
                Room currentRoom = theRoom;
                Room currentRoom1 = theRoom;  // Direction 0x04 (Zmin, -Z)
                Room currentRoom2 = theRoom;  // Direction 0x02 (Xmax, +X)
                Room currentRoom3 = theRoom;  // Direction 0x08 (Zmax, +Z)
                Room currentRoom4 = theRoom;  // Direction 0x01 (Xmin, -X)

                int searchX = xMin;
                int searchZ = zMin;

                // Main expansion loop - continues until no direction can expand
                while (true)
                {
                    // ===============================================================================
                    // DIRECTION 0x04: Expand Zmin (-Z direction, "top" in original)
                    // ===============================================================================
                    if ((directionBase & 0x04) == 0x04)
                    {
                        dec_doorCheck = false;
                        dec_room = currentRoom1;
                        currentRoom = currentRoom1;

                        searchX = xMin;

                        if (xMin <= xMax)
                        {
                            bool finishedDirection = true;

                            // Check all sectors along the current edge
                            while (floor == Dec_GetHeight(searchX, zMin) &&
                                   floor == Dec_GetHeight(searchX, zMin - 1) &&  // Check the row we want to expand into
                                   dec_monkey == monkeyInit)
                            {
                                // Update room reference at start of edge
                                if (searchX == xMin) currentRoom1 = dec_room;

                                // Handle room transitions
                                if (dec_doorCheck)
                                {
                                    // Stop if crossing into room with different flip state
                                    if (dec_room != currentRoom &&
                                        (dec_room.Alternated ||
                                         currentRoom.Alternated))
                                    {
                                        break;
                                    }

                                    // Reset to original room and verify floor height
                                    dec_room = currentRoom;

                                    if (floor != Dec_GetHeight(searchX, zMin - 1)) break;

                                    dec_doorCheck = false;
                                }

                                searchX++;

                                // Successfully checked entire edge
                                if (searchX > xMax)
                                {
                                    finishedDirection = false;
                                    break;
                                }
                            }

                            // Direction finished if we couldn't check entire edge
                            if (finishedDirection) direction -= 0x04;
                        }

                        directionBase = direction;
                        // Expand bounds if direction still active
                        if ((directionBase & 0x04) == 0x04) zMin--;
                    }

                    // ===============================================================================
                    // DIRECTION 0x02: Expand Xmax (+X direction, "right" in original)
                    // ===============================================================================
                    if ((directionBase & 0x02) == 0x02)
                    {
                        dec_doorCheck = false;
                        dec_room = currentRoom2;
                        currentRoom = currentRoom2;

                        searchZ = zMin;

                        if (zMin <= zMax)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetHeight(xMax, searchZ) &&
                                   floor == Dec_GetHeight(xMax + 1, searchZ) &&
                                   dec_monkey == monkeyInit)
                            {
                                if (searchZ == zMin) currentRoom2 = dec_room;

                                if (dec_doorCheck)
                                {
                                    if (dec_room != currentRoom &&
                                        (dec_room.Alternated ||
                                         currentRoom.Alternated))
                                    {
                                        break;
                                    }

                                    dec_room = currentRoom;

                                    if (floor != Dec_GetHeight(xMax + 1, searchZ)) break;

                                    dec_doorCheck = false;
                                }

                                searchZ++;

                                if (searchZ > zMax)
                                {
                                    finishedDirection = false;
                                    break;
                                }
                            }

                            if (finishedDirection) direction -= 0x02;
                        }

                        directionBase = direction;
                        if ((directionBase & 0x02) == 0x02) xMax++;
                    }

                    // ===============================================================================
                    // DIRECTION 0x08: Expand Zmax (+Z direction, "bottom" in original)
                    // ===============================================================================
                    if ((directionBase & 0x08) == 0x08)
                    {
                        dec_doorCheck = false;
                        dec_room = currentRoom3;
                        currentRoom = currentRoom3;

                        searchX = xMax;

                        if (xMax >= xMin)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetHeight(searchX, zMax) &&
                                   floor == Dec_GetHeight(searchX, zMax + 1) &&
                                   dec_monkey == monkeyInit)
                            {
                                if (searchX == xMax) currentRoom3 = dec_room;

                                if (dec_doorCheck)
                                {
                                    if (dec_room != currentRoom &&
                                        (dec_room.Alternated ||
                                         currentRoom.Alternated))
                                    {
                                        break;
                                    }

                                    dec_room = currentRoom;

                                    if (floor != Dec_GetHeight(searchX, zMax + 1)) break;

                                    dec_doorCheck = false;
                                }

                                searchX--;

                                if (searchX < xMin)
                                {
                                    finishedDirection = false;
                                    break;
                                }
                            }

                            if (finishedDirection) direction -= 0x08;
                        }

                        directionBase = direction;
                        if ((directionBase & 0x08) == 0x08) zMax++;
                    }

                    // ===============================================================================
                    // DIRECTION 0x01: Expand Xmin (-X direction, "left" in original)
                    // ===============================================================================
                    if ((directionBase & 0x01) == 0x01)
                    {
                        dec_doorCheck = false;
                        dec_room = currentRoom4;
                        currentRoom = currentRoom4;

                        searchZ = zMax;

                        if (zMax >= zMin)
                        {
                            bool finishedDirection = true;

                            while (floor == Dec_GetHeight(xMin, searchZ) &&
                                   floor == Dec_GetHeight(xMin - 1, searchZ) &&
                                   dec_monkey == monkeyInit)
                            {
                                if (searchZ == zMax) currentRoom4 = dec_room;

                                if (dec_doorCheck)
                                {
                                    if (dec_room != currentRoom &&
                                        (dec_room.Alternated ||
                                         currentRoom.Alternated))
                                    {
                                        break;
                                    }

                                    dec_room = currentRoom;

                                    if (floor != Dec_GetHeight(xMin - 1, searchZ)) break;

                                    dec_doorCheck = false;
                                }

                                searchZ--;

                                if (searchZ < zMin)
                                {
                                    finishedDirection = false;
                                    break;
                                }
                            }

                            if (finishedDirection) direction -= 0x01;
                        }

                        directionBase = direction;
                        if ((directionBase & 0x01) == 0x01) xMin--;
                    }

                    // All directions exhausted - expansion complete
                    if (directionBase == 0x00) break;

                    currentX = xMin;
                }

                // ===================================================================================
                // SET FINAL BOX BOUNDS
                // ===================================================================================
                // Note: Xmax and Zmax are exclusive (one past the last included sector)
                box.Xmin = xMin;
                box.Zmin = zMin;
                box.Xmax = xMax + 1;
                box.Zmax = zMax + 1;

                return true;
            }
        }

        /// <summary>
        /// Traverses through portals to find the room containing a world position.
        ///
        /// This function handles two types of portal traversal:
        /// 1. WALL PORTALS: Horizontal connections between rooms
        /// 2. FLOOR PORTALS: Vertical connections (stacked rooms)
        ///
        /// WALL PORTAL TRAVERSAL:
        /// =====================
        /// If the target position is outside the current room bounds, the function
        /// clamps to the nearest border sector and follows any wall portal there.
        /// This continues until the position is within a room's bounds.
        ///
        /// FLOOR PORTAL TRAVERSAL:
        /// =======================
        /// After horizontal positioning, the function descends through floor portals
        /// until it reaches the actual floor (stops at water boundaries).
        ///
        /// CORNER HANDLING:
        /// ================
        /// Corner sectors (0,0), (0,max), (max,0), (max,max) are problematic because
        /// they have no portal direction. The function returns false if stuck in a corner.
        /// </summary>
        /// <param name="x">World X coordinate (in sectors)</param>
        /// <param name="z">World Z coordinate (in sectors)</param>
        /// <returns>True if position is reachable, false if blocked</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool Dec_ClampRoom(int x, int z)
        {
            Room room = dec_room;

            int localX = 0;
            int localZ = 0;

            Sector sector;

            // ===================================================================================
            // PHASE 1: WALL PORTAL TRAVERSAL
            // ===================================================================================
            // Loop while position is outside current room bounds
            while (Dec_NeedsClamping(x, z))
            {
                // Convert world coords to local room coords
                localX = x - room.Position.X;
                localZ = z - room.Position.Z;

                // Clamp to room bounds (find border sector to check for portal)
                if (localX >= 0)
                {
                    if (localX >= room.NumXSectors)
                        localX = room.NumXSectors - 1;
                }
                else
                {
                    localX = 0;
                }

                if (localZ >= 0)
                {
                    if (localZ >= room.NumZSectors)
                        localZ = room.NumZSectors - 1;
                }
                else
                {
                    localZ = 0;
                }

                sector = room.Sectors[localX, localZ];

                // CORNER DETECTION HACK:
                // Original code didn't handle corners. If we land in a corner sector,
                // there's no portal to follow (portals are on edges, not corners).
                // This can happen with 3+ rooms connected at a point.
                if (localX == 0 && localZ == 0 ||
                    localX == 0 && localZ == room.NumZSectors - 1 ||
                    localX == room.NumXSectors - 1 && localZ == 0 ||
                    localX == room.NumXSectors - 1 && localZ == room.NumZSectors - 1)
                    return false;

                // No wall portal - can't reach the position
                if (sector.WallPortal == null)
                    break;

                // Follow wall portal to adjoining room
                Room adjoiningRoom = sector.WallPortal.AdjoiningRoom;

                dec_room = adjoiningRoom;
                room = adjoiningRoom;

                // Stop if portal is solid (toggle opacity 1)
                if (sector.WallPortal.Opacity == PortalOpacity.SolidFaces)
                    return false;
            }

            room = dec_room;

            localX = x - room.Position.X;
            localZ = z - room.Position.Z;

            // Safety check for ultra-rare edge cases
            if (localX < 0 || localZ < 0 || localX >= room.NumXSectors || localZ >= room.NumZSectors)
                return false;

            sector = room.Sectors[localX, localZ];

            // ===================================================================================
            // PHASE 2: FLOOR PORTAL TRAVERSAL
            // ===================================================================================
            // Descend through floor portals to find the actual floor room.
            // Stops at water boundaries (water/land transition).
            while (room.GetFloorRoomConnectionInfo(new VectorInt2(localX, localZ), true).TraversableType == Room.RoomConnectionType.FullPortal)
            {
                Room adjoiningRoom = sector.FloorPortal.AdjoiningRoom;

                // Stop at water boundary (don't cross water/land transition via floor portals)
                if (room.Properties.Type == RoomType.Water != (adjoiningRoom.Properties.Type == RoomType.Water))
                    break;

                dec_room = adjoiningRoom;

                room = dec_room;

                localX = x - room.Position.X;
                localZ = z - room.Position.Z;

                sector = room.Sectors[localX, localZ];
            }

            return true;
        }

        /// <summary>
        /// Checks if a world position is outside the current room bounds.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private bool Dec_NeedsClamping(int x, int z)
        {
            Room room = dec_room;
            return x < 0 || z < 0 || x > room.NumXSectors - 1 || z > room.NumZSectors - 1;
        }

        /// <summary>
        /// Gets floor height at a world position (simplified overload).
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.AggressiveOptimization)]
        private int Dec_GetHeight(int x, int z)
        {
            bool slope;
            return Dec_GetHeight(x, z, out slope);
        }

        /// <summary>
        /// Gets floor height and slope status at a world position.
        ///
        /// This function calculates the average floor height at a sector and determines
        /// if the sector is a slope (too steep for most enemies to walk on).
        ///
        /// FLOOR HEIGHT CALCULATION:
        /// =========================
        /// The floor height is the average of the 4 corner heights plus the room's Y position.
        /// Ghost blocks override actual geometry if present.
        ///
        /// SLOPE DETECTION:
        /// ================
        /// A sector is considered a slope if:
        /// - 3 or more edges have gradient >= 3 clicks (768 world units)
        /// - Opposite edges both have steep gradients
        /// - Adjacent edges have steep gradients (based on split type)
        ///
        /// SIDE EFFECTS:
        /// =============
        /// This function sets several global state variables:
        /// - dec_cornerHeight1-4: Corner heights for the sector
        /// - dec_monkey: True if sector has monkey swing flag
        /// - dec_splitter: Set to true if SPLITTER encountered
        /// - dec_checkUnderwater: Set to false if shallow water detected
        /// - dec_room: Updated when traversing through portals
        /// - dec_doorCheck: Set when crossing room boundaries
        /// </summary>
        /// <param name="x">World X coordinate (in sectors)</param>
        /// <param name="z">World Z coordinate (in sectors)</param>
        /// <param name="slope">Output: true if sector is too steep for walking</param>
        /// <returns>Floor height, or _noHeight if unwalkable</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveOptimization)]
        private int Dec_GetHeight(int x, int z, out bool slope)
        {
            slope = false;

            Room adjoiningRoom = dec_room;
            Room room = dec_room;

            // Ignore pathfinding for current room?
            if (dec_room.Properties.FlagExcludeFromPathFinding)
                return _noHeight;

            int localX = x - room.Position.X;
            int localZ = z - room.Position.Z;

            // If out of bounds, return no height
            if (localX < 0 ||
                localX > room.NumXSectors - 1 ||
                localZ < 0 ||
                localZ > room.NumZSectors - 1)
            {
                return _noHeight;
            }

            Sector sector = room.Sectors[localX, localZ];

            // If sector is a wall or is a vertical toggle opacity 1
            // Note that is & 8 because wall and border wall are the only sectors with bit 4 (0x08) set
            if ((sector.Type == SectorType.Wall ||
                 sector.Type == SectorType.BorderWall) && (sector.WallPortal == null ||
                sector.WallPortal != null && sector.WallPortal.Opacity == PortalOpacity.SolidFaces) ||
                (sector.Flags & SectorFlags.NotWalkableFloor) != 0)
            {
                dec_cornerHeight1 = Clicks.ToWorld(-1);
                dec_cornerHeight2 = Clicks.ToWorld(-1);
                dec_cornerHeight3 = Clicks.ToWorld(-1);
                dec_cornerHeight4 = Clicks.ToWorld(-1);

                return _noHeight;
            }

            // If it's not a wall portal or is vertical toggle opacity 1
            if (sector.WallPortal is not null && sector.WallPortal.Opacity != PortalOpacity.SolidFaces)
            {
                adjoiningRoom = sector.WallPortal.AdjoiningRoom;
                dec_room = adjoiningRoom;
                dec_doorCheck = true;

                room = dec_room;

                localX = x - room.Position.X;
                localZ = z - room.Position.Z;

                sector = room.Sectors[localX, localZ];
            }

            Room oldRoom = dec_room; 

            var connInfo = room.GetFloorRoomConnectionInfo(new VectorInt2(localX, localZ));
            while (sector.FloorPortal != null && connInfo.TraversableType != Room.RoomConnectionType.NoPortal)
            {
                adjoiningRoom = sector.FloorPortal.AdjoiningRoom;

                if (sector.FloorPortal.Opacity == PortalOpacity.SolidFaces)
                {
                    if (!((room.Properties.Type == RoomType.Water) ^ (adjoiningRoom.Properties.Type == RoomType.Water)))
                        break;
                }

                dec_room = adjoiningRoom;
                room = dec_room;

                localX = x - room.Position.X;
                localZ = z - room.Position.Z;

                sector = room.Sectors[localX, localZ];
                connInfo = room.GetFloorRoomConnectionInfo(new VectorInt2(localX, localZ));
            }

            if ((sector.Flags & SectorFlags.NotWalkableFloor) != 0) 
                return _noHeight;

            int floorXnZp = sector.HasGhostBlock ? sector.GhostBlock.Floor.XnZp : sector.Floor.XnZp,
                floorXpZp = sector.HasGhostBlock ? sector.GhostBlock.Floor.XpZp : sector.Floor.XpZp,
                floorXpZn = sector.HasGhostBlock ? sector.GhostBlock.Floor.XpZn : sector.Floor.XpZn,
                floorXnZn = sector.HasGhostBlock ? sector.GhostBlock.Floor.XnZn : sector.Floor.XnZn;

            int sumHeights = floorXnZp + floorXpZp + floorXpZn + floorXnZn;
            int tilt = sumHeights / 4;

            dec_cornerHeight1 = floorXnZp;
            dec_cornerHeight2 = floorXpZp;
            dec_cornerHeight3 = floorXpZn;
            dec_cornerHeight4 = floorXnZn;

            int grad1 = Math.Abs(dec_cornerHeight1 - dec_cornerHeight2) >= Clicks.ToWorld(3) ? 1 : 0;
            int grad2 = Math.Abs(dec_cornerHeight2 - dec_cornerHeight3) >= Clicks.ToWorld(3) ? 1 : 0;
            int grad3 = Math.Abs(dec_cornerHeight3 - dec_cornerHeight4) >= Clicks.ToWorld(3) ? 1 : 0;
            int grad4 = Math.Abs(dec_cornerHeight4 - dec_cornerHeight1) >= Clicks.ToWorld(3) ? 1 : 0;

            int type;

            if (floorXnZp == floorXpZn)
                type = 0;
            else if (floorXpZp == floorXnZn)
                type = 1;
            else if (floorXnZp < floorXpZp && floorXnZp < floorXnZn ||
                        floorXpZn < floorXpZp && floorXpZn < floorXnZn ||
                        floorXnZp > floorXpZp && floorXnZp > floorXnZn ||
                        floorXpZn > floorXpZp && floorXpZn > floorXnZn)
                type = 1;
            else
                type = 0;

            int height = tilt + room.Position.Y;

            int ceiling = sector.HasGhostBlock
                ? sector.GhostBlock.Ceiling.Max + room.Position.Y
                : sector.Ceiling.Max + room.Position.Y;

            // Check for shallow water
            if (dec_checkUnderwater && room.Properties.Type == RoomType.Water && (ceiling - height) <= Clicks.ToWorld(1) && sector.CeilingPortal != null)
            {
                adjoiningRoom = sector.CeilingPortal.AdjoiningRoom;
                if (adjoiningRoom.AlternateRoom != null && dec_flipped) 
                    adjoiningRoom = adjoiningRoom.AlternateRoom;

                if (adjoiningRoom.Properties.Type != RoomType.Water)
                {
                    dec_checkUnderwater = false;
                    dec_shallowWater = true;
                }
            }

            dec_room = oldRoom;

            if ((grad1 + grad2 + grad3 + grad4 >= 3 || 
                (grad1 + grad3 == 2) || 
                (grad2 + grad4 == 2) || 
                (type == 0 && ((grad1 + grad4 == 2) || (grad2 + grad3 == 2))) || 
                (type == 1 && ((grad1 + grad2 == 2) || (grad3 + grad4 == 2)))) && 
                dec_checkUnderwater && room.Properties.Type != RoomType.Water)
            {
                slope = true;
                // return height
            }

            if ((sector.Flags & SectorFlags.Box) != 0)
            {
                if (dec_splitter)
                    return _noHeight;
                else
                    dec_splitter = true;
            }

            if ((sector.Flags & SectorFlags.Monkey) != 0)
                dec_monkey = true;
            else
                dec_monkey = false;

            return height;
        }

        /// <summary>
        /// Tests if two boxes can be connected via a jump in the Z direction.
        ///
        /// JUMP DETECTION:
        /// ===============
        /// Enemies like skeletons and humans can jump across 1-2 sector gaps.
        /// This function checks if the gap between boxes is jumpable.
        ///
        /// Requirements for a valid jump:
        /// 1. Boxes must have the same floor height
        /// 2. Gap must be exactly 1 or 2 sectors wide
        /// 3. Gap floor must be at least 2 clicks below the box floor (room to jump)
        /// 4. Gap must be walkable (not a wall or solid portal)
        ///
        /// COORDINATE MAPPING:
        /// ==================
        /// Original: TestTBJumpOverlap (Top-Bottom = Y direction)
        /// TombEditor: Dec_TestJumpOverlapX (tests Z direction due to axis swap)
        /// </summary>
        /// <param name="test">First box to test</param>
        /// <param name="box">Second box to test</param>
        /// <returns>True if boxes can be connected via jump in Z direction</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool Dec_TestJumpOverlapX(dec_TombEngine_box_aux test, dec_TombEngine_box_aux box)
        {
            // Boxes must have the same height for jump
            if (test.Height != box.Height)
                return false;

            int xMin = test.Xmin > box.Xmin ? test.Xmin : box.Xmin;
            int xMax = test.Xmax < box.Xmax ? test.Xmax : box.Xmax;

            int zMin = test.Zmin;
            int zMax = box.Zmax;

            int currentX = (xMin + xMax) >> 1;

            int floor = 0;

            // 1 sector jump
            if (zMax == zMin - 1)
            {
                dec_room = box.Room;

                if (!Dec_ClampRoom(currentX, zMax - 1))
                    return false;

                if (!Dec_ClampRoom(currentX, zMax))
                    return false;

                floor = Dec_GetHeight(currentX, zMax);
                if (floor > box.Height - Clicks.ToWorld(2) || floor == _noHeight)
                    return false;

                return true;
            }

            // 2 sectors jump
            if (zMax == zMin - 2)
            {
                dec_room = box.Room;

                if (!Dec_ClampRoom(currentX, zMax - 1))
                    return false;

                if (!Dec_ClampRoom(currentX, zMax))
                    return false;

                floor = Dec_GetHeight(currentX, zMax);
                if (floor > box.Height - Clicks.ToWorld(2) || floor == _noHeight)
                    return false;

                if (!Dec_ClampRoom(currentX, zMax + 1))
                    return false;

                floor = Dec_GetHeight(currentX, zMax + 1);
                if (floor > box.Height - Clicks.ToWorld(2) || floor == _noHeight)
                    return false;

                return true;
            }

            // Swap cases

            zMin = box.Zmin;
            zMax = test.Zmax;

            // 1 sector jump
            if (zMax == zMin - 1)
            {
                dec_room = box.Room;

                if (!Dec_ClampRoom(currentX, zMax - 1))
                    return false;

                if (!Dec_ClampRoom(currentX, zMax))
                    return false;

                floor = Dec_GetHeight(currentX, zMax);
                if (floor > box.Height - Clicks.ToWorld(2) || floor == _noHeight)
                    return false;

                return true;
            }

            // 2 sectors jump
            if (zMax == zMin - 2)
            {
                dec_room = box.Room;

                if (!Dec_ClampRoom(currentX, zMax - 1))
                    return false;

                if (!Dec_ClampRoom(currentX, zMax))
                    return false;

                floor = Dec_GetHeight(currentX, zMax);
                if (floor > box.Height - Clicks.ToWorld(2) || floor == _noHeight)
                    return false;

                if (!Dec_ClampRoom(currentX, zMax + 1))
                    return false;

                floor = Dec_GetHeight(currentX, zMax + 1);
                if (floor > box.Height - Clicks.ToWorld(2) || floor == _noHeight)
                    return false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Tests if two boxes can be connected via a jump in the X direction.
        ///
        /// See Dec_TestJumpOverlapX for detailed documentation.
        /// This function tests the X direction (left-right) while that one tests Z.
        ///
        /// COORDINATE MAPPING:
        /// ==================
        /// Original: TestLRJumpOverlap (Left-Right = X direction)
        /// TombEditor: Dec_TestJumpOverlapZ (tests X direction, name is historical)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool Dec_TestJumpOverlapZ(dec_TombEngine_box_aux a, dec_TombEngine_box_aux b)
        {
            // Boxes must have the same height for jump
            if (a.Height != b.Height)
                return false;

            int zMin = a.Zmin > b.Zmin ? a.Zmin : b.Zmin;
            int zMax = a.Zmax < b.Zmax ? a.Zmax : b.Zmax;

            int xMin = a.Xmin;
            int xMax = b.Xmax;

            int currentZ = (zMin + zMax) >> 1;

            int floor = 0;

            // 1 sector jump
            if (xMax == xMin - 1)
            {
                dec_room = b.Room;

                if (!Dec_ClampRoom(xMax - 1, currentZ))
                    return false;

                if (!Dec_ClampRoom(xMax, currentZ))
                    return false;

                floor = Dec_GetHeight(xMax, currentZ);
                if (floor > b.Height - Clicks.ToWorld(2) || floor == _noHeight)
                    return false;

                return true;
            }

            // 2 sectors jump
            if (xMax == xMin - 2)
            {
                dec_room = b.Room;

                if (!Dec_ClampRoom(xMax - 1, currentZ))
                    return false;

                if (!Dec_ClampRoom(xMax, currentZ))
                    return false;

                floor = Dec_GetHeight(xMax, currentZ);
                if (floor > b.Height - Clicks.ToWorld(2) || floor == _noHeight)
                    return false;

                if (!Dec_ClampRoom(xMax + 1, currentZ))
                    return false;

                floor = Dec_GetHeight(xMax + 1, currentZ);
                if (floor > b.Height - Clicks.ToWorld(2) || floor == _noHeight)
                    return false;

                return true;
            }

            // Swap cases
            xMin = b.Xmin;
            xMax = a.Xmax;

            // 1 sector jump
            if (xMax == xMin - 1)
            {
                dec_room = b.Room;

                if (!Dec_ClampRoom(xMax - 1, currentZ))
                    return false;

                if (!Dec_ClampRoom(xMax, currentZ))
                    return false;

                floor = Dec_GetHeight(xMax, currentZ);
                if (floor > b.Height - Clicks.ToWorld(2) || floor == _noHeight)
                    return false;

                return true;
            }

            // 2 sectors jump
            if (xMax == xMin - 2)
            {
                dec_room = b.Room;

                if (!Dec_ClampRoom(xMax - 1, currentZ))
                    return false;

                if (!Dec_ClampRoom(xMax, currentZ))
                    return false;

                floor = Dec_GetHeight(xMax, currentZ);
                if (floor > b.Height - Clicks.ToWorld(2) || floor == _noHeight)
                    return false;

                if (!Dec_ClampRoom(xMax + 1, currentZ))
                    return false;

                floor = Dec_GetHeight(xMax + 1, currentZ);
                if (floor > b.Height - Clicks.ToWorld(2) || floor == _noHeight)
                    return false;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Tests edge adjacency when test.Xmax touches box (right edge overlap).
        ///
        /// EDGE ADJACENCY:
        /// ===============
        /// Two boxes are adjacent if they share an edge (not just a corner).
        /// This function verifies that the shared edge has matching floor heights.
        ///
        /// Tests all sectors along the shared edge to ensure they connect properly.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool Dec_TestOverlapXmax(dec_TombEngine_box_aux test, dec_TombEngine_box_aux box)
        {
            int startZ = test.Zmin > box.Zmin ? test.Zmin : box.Zmin;
            int endZ = test.Zmax < box.Zmax ? test.Zmax : box.Zmax;

            for (int z = startZ; z < endZ; z++)
            {
                dec_room = test.Room;

                if (!Dec_ClampRoom(test.Xmax - 1, z))
                    return false;

                dec_splitter = false;

                if (box.Height != Dec_GetHeight(test.Xmax, z))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Tests edge adjacency when test.Xmin touches box (left edge overlap).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool Dec_TestOverlapXmin(dec_TombEngine_box_aux test, dec_TombEngine_box_aux box)
        {
            int startZ = test.Zmin > box.Zmin ? test.Zmin : box.Zmin;
            int endZ = test.Zmax < box.Zmax ? test.Zmax : box.Zmax;

            for (int z = startZ; z < endZ; z++)
            {
                dec_room = test.Room;

                if (!Dec_ClampRoom(test.Xmin, z)) 
                    return false;

                dec_splitter = false;

                if (box.Height != Dec_GetHeight(test.Xmin - 1, z))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Tests edge adjacency when test.Zmax touches box (top edge overlap).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool Dec_TestOverlapZmax(dec_TombEngine_box_aux test, dec_TombEngine_box_aux box)
        {
            int startX = test.Xmin > box.Xmin ? test.Xmin : box.Xmin;
            int endX = test.Xmax < box.Xmax ? test.Xmax : box.Xmax;

            for (int x = startX; x < endX; x++)
            {
                dec_room = test.Room;

                if (!Dec_ClampRoom(x, test.Zmax - 1))
                    return false;

                dec_splitter = false;

                if (box.Height != Dec_GetHeight(x, test.Zmax))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Tests edge adjacency when test.Zmin touches box (bottom edge overlap).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public bool Dec_TestOverlapZmin(dec_TombEngine_box_aux test, dec_TombEngine_box_aux box)
        {
            int startX = test.Xmin > box.Xmin ? test.Xmin : box.Xmin;
            int endX = test.Xmax < box.Xmax ? test.Xmax : box.Xmax;

            for (int x = startX; x < endX; x++)
            {
                dec_room = test.Room;

                if (!Dec_ClampRoom(x, test.Zmin))
                    return false;

                dec_splitter = false;

                if (box.Height != Dec_GetHeight(x, test.Zmin - 1))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Main overlap check between two boxes.
        ///
        /// OVERLAP TYPES:
        /// ==============
        /// 1. STRAIGHT OVERLAP: Boxes overlap in both X and Z (must have same height)
        /// 2. EDGE ADJACENCY: Boxes share an edge (adjacent but not overlapping)
        /// 3. JUMP OVERLAP: Boxes separated by 1-2 sector gap (jumpable)
        ///
        /// OUTPUT FLAGS (set as side effects):
        /// - dec_jump: True if connection requires jumping
        /// - dec_monkey: True if both boxes have monkey swing ceilings
        ///
        /// ALGORITHM:
        /// ==========
        /// 1. Always process from higher to lower box (swap if needed)
        /// 2. Check X overlap first:
        ///    - If no X overlap: try X-direction jump or edge adjacency
        ///    - If X overlap: check Z overlap (straight) or try Z-direction jump or edge adjacency
        /// 3. Set monkey flag if both boxes have monkey swing
        ///
        /// </summary>
        /// <param name="a">First box to test</param>
        /// <param name="b">Second box to test</param>
        /// <returns>True if boxes overlap/connect, false otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool Dec_CheckOverlap(dec_TombEngine_box_aux a, dec_TombEngine_box_aux b)
        {
            dec_jump = false;
            dec_monkey = false;

            // Always process from higher to lower box (numerically higher Height value)
            dec_TombEngine_box_aux box1 = a;
            dec_TombEngine_box_aux box2 = b;

            if (b.Height > a.Height)
            {
                box1 = b;
                box2 = a;
            }

            bool hasXOverlap = box1.Xmax > box2.Xmin && box1.Xmin < box2.Xmax;
            bool hasZOverlap = box1.Zmax > box2.Zmin && box1.Zmin < box2.Zmax;

            // ===================================================================================
            // CASE 1: No X overlap - check for X-direction connections
            // ===================================================================================
            if (!hasXOverlap)
            {
                // Try X-direction jump if Z ranges overlap
                if (hasZOverlap && Dec_TestJumpOverlapZ(box1, box2))
                {
                    dec_jump = true;
                    return true;
                }

                // Edge adjacency requires Z overlap
                if (!hasZOverlap)
                    return false;

                // Check X edge adjacency
                if (box1.Xmax == box2.Xmin)
                {
                    if (!Dec_TestOverlapXmax(box1, box2))
                        return false;
                }
                else if (box1.Xmin == box2.Xmax)
                {
                    if (!Dec_TestOverlapXmin(box1, box2))
                        return false;
                }
                else
                {
                    return false;
                }

                if (box1.Monkey && box2.Monkey) dec_monkey = true;
                return true;
            }

            // ===================================================================================
            // CASE 2: X overlap and Z overlap - straight overlap
            // ===================================================================================
            if (hasZOverlap)
            {
                // Straight overlap - must have same height
                if (box1.Height != box2.Height)
                    return false;

                if (box1.Monkey && box2.Monkey) dec_monkey = true;
                return true;
            }

            // ===================================================================================
            // CASE 3: X overlap but no Z overlap - check for Z-direction connections
            // ===================================================================================
            // Try Z-direction jump
            if (Dec_TestJumpOverlapX(box2, box1))
            {
                dec_jump = true;
                return true;
            }

            // Check Z edge adjacency
            if (box1.Zmax == box2.Zmin)
            {
                if (!Dec_TestOverlapZmax(box1, box2))
                    return false;
            }
            else if (box1.Zmin == box2.Zmax)
            {
                if (!Dec_TestOverlapZmin(box1, box2))
                    return false;
            }
            else
            {
                return false;
            }

            if (box1.Monkey && box2.Monkey) dec_monkey = true;
            return true;
        }
    }
}
