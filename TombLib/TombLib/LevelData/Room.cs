using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TombLib.LevelData.Compilers;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;
using TombLib.LevelData.SectorStructs;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public enum RoomType : byte
    {
        Normal, Rain, Snow, Water, Quicksand
    }

    public enum RoomLightEffect : byte
    {
        // Movement and GlowAndMovement are respectively remapped to Flicker and Sunset exclusively for TR2.
        None, Default, Reflection, Glow, Movement, GlowAndMovement, Mist, Flicker = Movement, Sunset = GlowAndMovement
    }

    public enum RoomLightInterpolationMode : byte
    {
        Default, Interpolate, NoInterpolate
    }

    public class RoomProperties : ICloneable
    {
        [DisplayName("Ambient light")]
        public Vector3 AmbientLight { get; set; } = new Vector3(0.25f, 0.25f, 0.25f); // Normalized float. (1.0 meaning normal brightness, 2.0 is the maximal brightness supported by tomb4.exe)
        [DisplayName("Room type")]
        public RoomType Type { get; set; } = RoomType.Normal;
        [DisplayName("Room type")]
        public byte TypeStrength { get; set; } = 0; // Internal legacy value for TRNG rain/snow strengths
        [DisplayName("Effect")]
        public RoomLightEffect LightEffect { get; set; } = RoomLightEffect.Default;
        [DisplayName("Effect strength")]
        public byte LightEffectStrength { get; set; } = 1;
        [DisplayName("Portal shade")]
        public RoomLightInterpolationMode LightInterpolationMode { get; set; } = RoomLightInterpolationMode.Default;

        // Flags
        [DisplayName("Damage")]
        public bool FlagDamage { get; set; }
        [DisplayName("Cold")]
        public bool FlagCold { get; set; }
        [DisplayName("Skybox")]
        public bool FlagHorizon { get; set; }
        [DisplayName("Wind")]
        public bool FlagOutside { get; set; }
        [DisplayName("No pathfinding")]
        public bool FlagExcludeFromPathFinding { get; set; }
        [DisplayName("No lensflare")]
        public bool FlagNoLensflare { get; set; }
        [DisplayName("Reverb type")]
        public byte Reverberation { get; set; }
        [DisplayName("Locked")]
        public bool Locked { get; set; }
        [DisplayName("Hidden")]
        public bool Hidden { get; set; }

        // Tags
        [DisplayName("Tags")]
        public List<string> Tags { get; set; } = new List<string>();

        object ICloneable.Clone() => Clone();

        public RoomProperties Clone()
        {
            var result = (RoomProperties)MemberwiseClone();
            result.Tags = new List<string>(Tags);
            return result;
        }
    }

    public enum RoundingMethod
    {
        Default,
        ToFloor,
        ToCeiling,
        Integer
    }

    public static class Clicks
    {
        /// <summary>
        /// Converts world units (256) to clicks (1).
        /// </summary>
        public static int FromWorld(int worldUnits)
            => (int)Math.Round(worldUnits / (float)Level.FullClickHeight);

        /// <inheritdoc />
        public static int FromWorld(int worldUnits, RoundingMethod roundingMethod) => roundingMethod switch
        {
            RoundingMethod.ToFloor => (int)Math.Floor(worldUnits / (float)Level.FullClickHeight),
            RoundingMethod.ToCeiling => (int)Math.Ceiling(worldUnits / (float)Level.FullClickHeight),
            RoundingMethod.Integer => worldUnits / Level.FullClickHeight,
            _ => (int)Math.Round(worldUnits / (float)Level.FullClickHeight)
        };

        /// <summary>
        /// Converts clicks (1) to world units (256).
        /// </summary>
        public static int ToWorld(int clicks)
            => clicks * Level.FullClickHeight;
    }

    public class Room : ITriggerParameter
    {
        public delegate void RemovedFromRoomDelegate(Room instance);
        public event RemovedFromRoomDelegate DeletedEvent;

        public const int DefaultHeight = 12 * Level.FullClickHeight;
        public const short DefaultRoomDimensions = 20;

        public Level Level { get; set; }

        public string Name { get; set; }
        public RoomProperties Properties { get; set; }
        public VectorInt3 Position { get; set; }
        public Sector[,] Sectors { get; private set; }

        private List<PositionBasedObjectInstance> _objects = new List<PositionBasedObjectInstance>();

        public Room AlternateBaseRoom { get; set; }
        public Room AlternateRoom { get; set; }
        public short AlternateGroup { get; set; } = -1;

        // Internal data structures
        public RoomGeometry RoomGeometry { get; } = new RoomGeometry();

        private IEnumerable<PortalInstance> _portalsCache;

		public Room(Level level, int numXSectors, int numZSectors, Vector3 ambientLight, string name = "Unnamed", int ceiling = DefaultHeight)
        {
            Name = name;
            Level = level;
            Properties = new RoomProperties() { AmbientLight = ambientLight };
            Resize(null, new RectangleInt2(0, 0, numXSectors - 1, numZSectors - 1), 0, ceiling, true);
            BuildGeometry();
        }

        public Room(Level level, VectorInt2 sectorSize, Vector3 ambientLight, string name = "Unnamed", int ceiling = DefaultHeight)
            : this(level, sectorSize.X, sectorSize.Y, ambientLight, name, ceiling)
        { }

        // Usually it's highly recommended to call FixupNeighborPortals afterwards, to fix neighboring portals.
        public void Resize(Level level, RectangleInt2 area, int floor = 0, int ceiling = DefaultHeight, bool? useFloor = false)
        {
            int numXSectors = area.Width + 1;
            int numZSectors = area.Height + 1;
            VectorInt2 offset = area.Start;

            if (numXSectors < 3 || numZSectors < 3)
                throw new ArgumentOutOfRangeException(nameof(area), area, "Provided area for resizing the room is too small. The area must span at least 3 sectors in X and Z dimension.");

            // Remove sector based objects if there are any
            var sectorObjects = Sectors != null ? SectorObjects.ToList() : new List<SectorBasedObjectInstance>();
            foreach (var instance in sectorObjects)
                RemoveObjectAndSingularPortalAndKeepAlive(level, instance);

            // Build new sectors
            Sector[,] newSectors = new Sector[numXSectors, numZSectors];
            for (int x = 0; x < numXSectors; x++)
                for (int z = 0; z < numZSectors; z++)
                {
                    VectorInt2 oldSectorVec = new VectorInt2(x, z) + offset;
                    Sector oldSector = GetSectorTry(oldSectorVec);
                    newSectors[x, z] = oldSector ?? new Sector(floor, ceiling);

                    if (oldSector == null || (useFloor.HasValue && newSectors[x, z].Type == SectorType.BorderWall))
                        newSectors[x, z].Type = !useFloor.HasValue || useFloor.Value ? SectorType.Floor : SectorType.Wall;

                    if (!useFloor.HasValue)
                    {
                        RaiseSector(oldSectorVec.X, oldSectorVec.Y, SectorVerticalPart.QA, floor);

                        for (int i = 0; i < oldSector.ExtraFloorSplits.Count; i++)
                            RaiseSector(oldSectorVec.X, oldSectorVec.Y, SectorVerticalPartExtensions.GetExtraFloorSplit(i), floor);

                        RaiseSector(oldSectorVec.X, oldSectorVec.Y, SectorVerticalPart.WS, ceiling);

                        for (int i = 0; i < oldSector.ExtraCeilingSplits.Count; i++)
                            RaiseSector(oldSectorVec.X, oldSectorVec.Y, SectorVerticalPartExtensions.GetExtraCeilingSplit(i), ceiling);
                    }

                    if (x == 0 || z == 0 || x == numXSectors - 1 || z == numZSectors - 1)
                    {
                        newSectors[x, z].Type = SectorType.BorderWall;
                        newSectors[x, z].Floor.DiagonalSplit = DiagonalSplit.None;
                        newSectors[x, z].Ceiling.DiagonalSplit = DiagonalSplit.None;
                    }
                }

            // Update data structures
            Sectors = newSectors;

            // Move objects
            SectorPos = SectorPos + offset;
            foreach (var instance in _objects)
                instance.Position -= new Vector3(offset.X * Level.SectorSizeUnit, 0, offset.Y * Level.SectorSizeUnit);

            // Add sector based objects again
            foreach (var instance in sectorObjects)
                AddObjectAndSingularPortalCutSectors(level, area, instance);
        }

        // Usually it's highly recommended to call FixupNeighborPortals on both rooms afterwards, to fix neighboring portals.
        public Room Split(Level level, RectangleInt2 area, Room alternateOppositeSplitRoom = null)
        {
            var newRoom = Clone(level, instance => !(instance is PositionBasedObjectInstance) && !(instance is PortalInstance));
            newRoom.Name = "Room split from " + Name;
            newRoom.Resize(level, area);

            // Setup alternate rooms
            newRoom.AlternateGroup = AlternateGroup;
            if (alternateOppositeSplitRoom != null)
                if (AlternateRoom != null)
                {
                    alternateOppositeSplitRoom.AlternateBaseRoom = newRoom;
                    newRoom.AlternateRoom = alternateOppositeSplitRoom;
                }
                else if (AlternateBaseRoom != null)
                {
                    alternateOppositeSplitRoom.AlternateRoom = newRoom;
                    newRoom.AlternateBaseRoom = alternateOppositeSplitRoom;
                }

            // Detect if the room was split by a straight line
            // If this is the case, resize the original room
            List<PortalInstance> portals = Portals.ToList();
            if (area.X0 == 0 && area.Y0 == 0 && area.X1 == NumXSectors - 1 && area.Y1 < NumZSectors - 1)
            {
                Resize(level, new RectangleInt2(area.X0, area.Y1 - 1, area.X1, NumZSectors - 1));
                AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(area.X0 + 1, 0, area.X1 - 1, 0), PortalDirection.WallNegativeZ, newRoom));
                newRoom.AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(area.X0 + 1, newRoom.NumZSectors - 1, area.X1 - 1, newRoom.NumZSectors - 1), PortalDirection.WallPositiveZ, this));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectAndSingularPortalCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.Z < Level.SectorSizeUnit)
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else if (area.X0 == 0 && area.Y0 == 0 && area.X1 < NumXSectors - 1 && area.Y1 == NumZSectors - 1)
            {
                Resize(level, new RectangleInt2(area.X1 - 1, area.Y0, NumXSectors - 1, area.Y1));
                AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(0, area.Y0 + 1, 0, area.Y1 - 1), PortalDirection.WallNegativeX, newRoom));
                newRoom.AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(newRoom.NumXSectors - 1, area.Y0 + 1, newRoom.NumXSectors - 1, area.Y1 - 1), PortalDirection.WallPositiveX, this));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectAndSingularPortalCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.X < Level.SectorSizeUnit)
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else if (area.X0 == 0 && area.Y0 > 0 && area.X1 == NumXSectors - 1 && area.Y1 == NumZSectors - 1)
            {
                Resize(level, new RectangleInt2(area.X0, 0, area.X1, area.Y0 + 1));
                AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(area.X0 + 1, NumZSectors - 1, area.X1 - 1, NumZSectors - 1), PortalDirection.WallPositiveZ, newRoom));
                newRoom.AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(area.X0 + 1, 0, area.X1 - 1, 0), PortalDirection.WallNegativeZ, this));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectAndSingularPortalCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.Z > (NumZSectors - 1) * Level.SectorSizeUnit)
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else if (area.X0 > 0 && area.Y0 == 0 && area.X1 == NumXSectors - 1 && area.Y1 == NumZSectors - 1)
            {
                Resize(level, new RectangleInt2(0, area.Y0, area.X0 + 1, area.Y1));
                AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(NumXSectors - 1, area.Y0 + 1, NumXSectors - 1, area.Y1 - 1), PortalDirection.WallPositiveX, newRoom));
                newRoom.AddObjectAndSingularPortal(level, new PortalInstance(new RectangleInt2(0, area.Y0 + 1, 0, area.Y1 - 1), PortalDirection.WallNegativeX, this));

                // Move objects
                foreach (PortalInstance portal in portals)
                    newRoom.AddObjectAndSingularPortalCutSectors(level, area, portal);
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.X > (NumXSectors - 1) * Level.SectorSizeUnit)
                        newRoom.MoveObjectFrom(level, this, instance);
            }
            else
            {
                // Resize area
                for (int z = area.Y0 + 1; z < area.Y1; ++z)
                    for (int x = area.X0 + 1; x < area.X1; ++x)
                        Sectors[x, z].Type = SectorType.Wall;

                // Move objects
                Vector2 start = (area.Start + new Vector2(1, 1)) * Level.SectorSizeUnit;
                Vector2 end = (area.End - new Vector2(0, 0)) * Level.SectorSizeUnit;
                foreach (PortalInstance portal in portals)
                {
                    RemoveObjectAndSingularPortalAndKeepAlive(level, portal);
                    RectangleInt2 validPortalArea = portal.GetValidArea(area);
                    foreach (PortalInstance splitPortal in portal.SplitIntoRectangles(pos => !validPortalArea.Contains(pos), new VectorInt2()))
                        AddObjectAndSingularPortal(level, splitPortal);
                    newRoom.AddObjectAndSingularPortalCutSectors(level, area, portal);
                }
                foreach (PositionBasedObjectInstance instance in Objects.ToList())
                    if (instance.Position.X > start.X && instance.Position.Z > start.Y &&
                        instance.Position.X < end.X && instance.Position.Z < end.Y)
                        newRoom.MoveObjectFrom(level, this, instance);
            }

            newRoom.BuildGeometry();
            BuildGeometry();
            return newRoom;
        }

        public Room Clone(Level level, Predicate<ObjectInstance> decideToCopy, bool fullCopy = false)
        {
            // Copy most variables
            var result = new Room(level, NumXSectors, NumZSectors, Properties.AmbientLight);
            result.Position = Position;
            if (!fullCopy)
            {
                result.AlternateBaseRoom = null;
                result.AlternateRoom = null;
                result.AlternateGroup = -1;
            }

            // Copy properties
            result.Properties = Properties.Clone();

            // Copy sectors
            result.Sectors = new Sector[NumXSectors, NumZSectors];
            for (int z = 0; z < NumZSectors; ++z)
                for (int x = 0; x < NumXSectors; ++x)
                    result.Sectors[x, z] = Sectors[x, z].Clone();

            if (decideToCopy != null)
            {
                // Copy objects
                result._objects = new List<PositionBasedObjectInstance>();
                foreach (var instance in AnyObjects)
                    if (decideToCopy(instance))
                    {
                        var copy = instance.Clone();
                        result.AddObjectAndSingularPortal(level, copy);
                        if (copy is IHasScriptID)
                            (copy as IHasScriptID).AllocateNewScriptId();
                    }
            }

            return result;
        }

        public Room Clone(Level level)
        {
            return Clone(level, instance => !(instance is PortalInstance));
        }

        /// <summary>
        /// This methode should be invoked after the object was added to a room and the object will go out of scope.
        /// </summary>
        public List<Room> Delete(Level level)
        {
            List<Room> result = new List<Room>();

            // Remove all objects in the room
            var objectsToRemove = AnyObjects.ToList();
            foreach (var instance in objectsToRemove)
            {
                if (AlternateBaseRoom != null)
                    RemoveObjectAndSingularPortal(level, instance);
                else
                    RemoveObject(level, instance);

                // Collect affected rooms in case
                result = result.Concat(level.DeleteTriggersForObject(instance)).ToList();
            }

            // Inform of room removal
            DeletedEvent?.Invoke(this);

            return result;
        }

        public List<Room> AndAdjoiningRooms => new List<Room>(Portals.ToList()
                                                                     .GroupBy(item => item.AdjoiningRoom)
                                                                     .Select(group => group.First())
                                                                     .Select(item => item.AdjoiningRoom)
                                                                     .ToList()) { this };

        public bool ExistsInLevel => Level != null && Array.IndexOf(Level.Rooms, this) != -1;
        public bool Alternated => AlternateRoom != null || AlternateBaseRoom != null;
        public bool IsAlternate => AlternateRoom == null && AlternateBaseRoom != null;
        public Room AlternateOpposite => AlternateRoom ?? AlternateBaseRoom;
        public VectorInt2 SectorSize => new VectorInt2(NumXSectors, NumZSectors);
        public RectangleInt2 WorldArea => new RectangleInt2(Position.X, Position.Z, Position.X + NumXSectors - 1, Position.Z + NumZSectors - 1);
        public BoundingBox WorldBoundingBox => new BoundingBox(new Vector3(Position.X, Position.Y + GetLowestCorner(), Position.Z) * new Vector3(Level.SectorSizeUnit, 1, Level.SectorSizeUnit),
                                                               new Vector3(Position.X + NumXSectors - 1, Position.Y + GetHighestCorner(), Position.Z + NumZSectors - 1) * new Vector3(Level.SectorSizeUnit, 1, Level.SectorSizeUnit));
        public RectangleInt2 LocalArea => new RectangleInt2(0, 0, NumXSectors - 1, NumZSectors - 1);

        public bool CoordinateInvalid(int x, int z) => x < 0 || z < 0 || x >= NumXSectors || z >= NumZSectors;
        public bool CoordinateInvalid(VectorInt2 coord) => CoordinateInvalid(coord.X, coord.Y);

        public IEnumerable<Room> Versions
        {
            get
            {
                yield return this;
                Room alternateVersion = AlternateOpposite;
                if (alternateVersion != null)
                    yield return alternateVersion;
            }
        }

        public VectorInt2 SectorPos
        {
            get { return new VectorInt2(Position.X, Position.Z); }
            set { Position = new VectorInt3(value.X, Position.Y, value.Y); }
        }

        public IEnumerable<PortalInstance> Portals
        {
            get
            {   // No LINQ because it is really slow.
                var portals = new HashSet<PortalInstance>();
                foreach (var sector in Sectors)
                    foreach (var portal in sector.Portals)
                        portals.Add(portal);
                return portals;
            }
        }

        public IEnumerable<PortalInstance> RebuildPortalsCache()
        {
            _portalsCache = Portals;
            return _portalsCache;
        }

        public IEnumerable<PortalInstance> PortalsCache
        {
            get => _portalsCache;
        }

        public IEnumerable<TriggerInstance> Triggers
        {
            get
            { // No LINQ because it is really slow.
                var triggers = new HashSet<TriggerInstance>();
                foreach (var sector in Sectors)
                    foreach (var trigger in sector.Triggers)
                        triggers.Add(trigger);
                return triggers;
            }
        }

        public IEnumerable<GhostBlockInstance> GhostBlocks
        {
            get
            { // No LINQ because it is really slow.
                var ghosts = new HashSet<GhostBlockInstance>();
                foreach (var sector in Sectors)
                    if (sector.HasGhostBlock)
                        ghosts.Add(sector.GhostBlock);
                return ghosts;
            }
        }

        public IEnumerable<SectorBasedObjectInstance> SectorObjects
        {
            get
            {
                foreach (var instance in Portals)
                    yield return instance;
                foreach (var instance in Triggers)
                    yield return instance;
                foreach (var instance in GhostBlocks)
                    yield return instance;
            }
        }

        public IReadOnlyList<PositionBasedObjectInstance> Objects => _objects;
        public IEnumerable<VolumeInstance> Volumes => _objects.OfType<VolumeInstance>();

        public IEnumerable<ObjectInstance> AnyObjects
        {
            get
            {
                foreach (var instance in Portals)
                    yield return instance;
                foreach (var instance in Triggers)
                    yield return instance;
                foreach (var instance in GhostBlocks)
                    yield return instance;
                foreach (var instance in _objects)
                    yield return instance;
            }
        }

        public Sector GetSector(VectorInt2 pos)
        {
            return Sectors[pos.X, pos.Y];
        }

        public void SetSector(VectorInt2 pos, Sector sector)
        {
            Sectors[pos.X, pos.Y] = sector;
        }

        public Sector GetSectorTry(int x, int z)
        {
            if (Sectors == null)
                return null;
            if (x >= 0 && z >= 0 && x < NumXSectors && z < NumZSectors)
                return Sectors[x, z];
            return null;
        }

        public bool GetSectorTry(int x, int z, [NotNullWhen(true)] out Sector sector)
        {
            sector = null;

            if (Sectors is null)
                return false;

            if (x >= 0 && z >= 0 && x < NumXSectors && z < NumZSectors)
            {
                sector = Sectors[x, z];
                return true;
            }

            return false;
        }

        public Sector GetSectorTry(VectorInt2 pos)
        {
            return GetSectorTry(pos.X, pos.Y);
        }

        ///<param name="x">The X-coordinate. The point at room.Position is it at (0, 0)</param>
        ///<param name="z">The Z-coordinate. The point at room.Position is it at (0, 0)</param>
        public List<int> GetHeightsAtPoint(int x, int z, SectorVerticalPart vertical)
        {
            List<int> values = new List<int>();
            for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
            {
                Sector sector = GetSectorTry(x - edge.DirectionX(), z - edge.DirectionZ());
                if (sector != null && !sector.IsAnyWall)
                    values.Add(sector.GetHeight(vertical, edge));
            }
            return values;
        }

        public RoomSectorPair ProbeLowestSector(VectorInt2 pos, bool doProbe, out VectorInt2 adjoiningCoordinate)
        {
            adjoiningCoordinate = pos; // Set to source coordinate by default

            Sector sector = GetSectorTry(pos);
            if (sector == null)
                return new RoomSectorPair();
            else if (!doProbe || sector.WallPortal != null)
                return new RoomSectorPair { Room = this, Sector = sector, SectorPosition = pos };

            RoomSectorPair result = GetSectorTryThroughPortal(pos);
            if (result.Sector?.FloorPortal == null)
                return result;
            // We must also check if floor portal is solid or not
            if (!result.Sector.FloorPortal.IsTraversable || GetFloorRoomConnectionInfo(pos, true).TraversableType != RoomConnectionType.FullPortal)
                return result;

            Room adjoiningRoom = result.Sector.FloorPortal.AdjoiningRoom;
            adjoiningCoordinate = pos + (SectorPos - adjoiningRoom.SectorPos);
            return adjoiningRoom.ProbeLowestSector(adjoiningCoordinate, true, out adjoiningCoordinate);
        }
        public RoomSectorPair ProbeLowestSector(int x, int z, bool doProbe = true) { VectorInt2 temp; return ProbeLowestSector(new VectorInt2(x, z), doProbe, out temp); }

        public RoomSectorPair GetSectorTryThroughPortal(VectorInt2 pos)
        {
            Sector sector = GetSectorTry(pos);
            if (sector == null)
                return new RoomSectorPair();

            if (sector.WallPortal == null)
                return new RoomSectorPair { Room = this, Sector = sector, SectorPosition = pos };

            Room adjoiningRoom = sector.WallPortal.AdjoiningRoom;

            if (IsAlternate && adjoiningRoom.AlternateRoom != null)
                adjoiningRoom = adjoiningRoom.AlternateRoom;

            VectorInt2 adjoiningSectorCoordinate = pos + (SectorPos - adjoiningRoom.SectorPos);
            Sector sector2 = adjoiningRoom.GetSectorTry(adjoiningSectorCoordinate);
            return new RoomSectorPair { Room = adjoiningRoom, Sector = sector2, SectorPosition = adjoiningSectorCoordinate };
        }
        public RoomSectorPair GetSectorTryThroughPortal(int x, int z) => GetSectorTryThroughPortal(new VectorInt2(x, z));

        public void ModifyHeightThroughPortal(int x, int z, SectorVerticalPart vertical, int increment, RectangleInt2 area)
        {
            if (x <= 0 || z <= 0 || x >= NumXSectors || z >= NumZSectors)
                return;

            if (area.Contains(new VectorInt2(x, z)))
            {
                if (vertical.IsOnFloor() && Sectors[x, z].Floor.DiagonalSplit == DiagonalSplit.None || vertical.IsOnCeiling() && Sectors[x, z].Ceiling.DiagonalSplit == DiagonalSplit.None)
                {
                    ChangeSectorHeight(x, z, vertical, SectorEdge.XnZn, increment);
                    Sectors[x, z].FixHeights(vertical);
                }
            }
            if (area.Contains(new VectorInt2(x - 1, z)))
            {
                var adjacentLeftSector = GetSectorTry(x - 1, z);
                if (adjacentLeftSector != null && (vertical.IsOnFloor() && adjacentLeftSector.Floor.DiagonalSplit == DiagonalSplit.None || vertical.IsOnCeiling() && adjacentLeftSector.Ceiling.DiagonalSplit == DiagonalSplit.None))
                {
                    ChangeSectorHeight(x - 1, z, vertical, SectorEdge.XpZn, increment);
                    adjacentLeftSector.FixHeights(vertical);
                }
            }
            if (area.Contains(new VectorInt2(x, z - 1)))
            {
                var adjacentBottomSector = GetSectorTry(x, z - 1);
                if (adjacentBottomSector != null && (vertical.IsOnFloor() && adjacentBottomSector.Floor.DiagonalSplit == DiagonalSplit.None || vertical.IsOnCeiling() && adjacentBottomSector.Ceiling.DiagonalSplit == DiagonalSplit.None))
                {
                    ChangeSectorHeight(x, z - 1, vertical, SectorEdge.XnZp, increment);
                    adjacentBottomSector.FixHeights(vertical);
                }
            }
            if (area.Contains(new VectorInt2(x - 1, z - 1)))
            {
                var adjacentBottomLeftSector = GetSectorTry(x - 1, z - 1);
                if (adjacentBottomLeftSector != null && (vertical.IsOnFloor() && adjacentBottomLeftSector.Floor.DiagonalSplit == DiagonalSplit.None || vertical.IsOnCeiling() && adjacentBottomLeftSector.Ceiling.DiagonalSplit == DiagonalSplit.None))
                {
                    ChangeSectorHeight(x - 1, z - 1, vertical, SectorEdge.XpZp, increment);
                    adjacentBottomLeftSector.FixHeights(vertical);
                }
            }
        }

        public bool IsIllegalSlope(int x, int z)
        {
            Sector sector = GetSectorTry(x, z);

            if (Properties.Type == RoomType.Water || sector == null || sector.IsAnyWall || !sector.Floor.HasSlope())
                return false;

            const int lowestPassableHeight = 4 * Level.FullClickHeight;
            const int lowestPassableStep = 2 * Level.FullClickHeight;  // Lara still can bug out of 2-click step heights

            var normals = sector.GetFloorTriangleNormals();
            var slopeDirections = sector.GetFloorTriangleSlopeDirections(Sector.SlopeCalculationMode.Legacy);

            if (slopeDirections[0] != Direction.None && slopeDirections[1] != Direction.None &&
                slopeDirections[0] != slopeDirections[1])
            {
                var diff = normals[0] - normals[1];
                if (Math.Atan2(diff.X, diff.Z) < 0)
                    return true;
            }

            // Main illegal slope calculation takes two adjacent corner heights (heightsToCheck[0-1]) from lookup sector
            // and looks if they are higher than any slope's lower heights (heightsToCompare[0-1]). Also it checks if
            // lookup sector's adjacent ceiling heights (heightsToCheck[2-3]) is lower than minimum passable height.
            // If lookup sector contains floor or ceiling diagonal splits, resolve algorithm is diverted (look further).

            bool slopeIsIllegal = false;

            for (int i = 0; i < 2; i++)
            {
                if (slopeIsIllegal)
                    continue;

                RoomSectorPair lookupSector;
                SectorEdge[] heightsToCompare;
                SectorEdge[] heightsToCheck;

                switch (slopeDirections[i])
                {
                    case Direction.PositiveZ:
                        lookupSector = GetSectorTryThroughPortal(x, z + 1);
                        heightsToCompare = new SectorEdge[2] { SectorEdge.XnZp, SectorEdge.XpZp };
                        heightsToCheck = new SectorEdge[2] { SectorEdge.XnZn, SectorEdge.XpZn };
                        break;

                    case Direction.NegativeZ:
                        lookupSector = GetSectorTryThroughPortal(x, z - 1);
                        heightsToCompare = new SectorEdge[2] { SectorEdge.XpZn, SectorEdge.XnZn };
                        heightsToCheck = new SectorEdge[2] { SectorEdge.XpZp, SectorEdge.XnZp };
                        break;

                    // We only need to override east and west diagonal split HeightsToCompare[1] cases, because
                    // slanted diagonal splits are always 45 degrees, hence these are only two slide directions possible.

                    case Direction.PositiveX:
                        lookupSector = GetSectorTryThroughPortal(x + 1, z);
                        heightsToCompare = new SectorEdge[2] { SectorEdge.XpZp, sector.Floor.DiagonalSplit == DiagonalSplit.XnZp ? SectorEdge.XpZp : SectorEdge.XpZn };
                        heightsToCheck = new SectorEdge[2] { SectorEdge.XnZp, SectorEdge.XnZn };
                        break;

                    case Direction.NegativeX:
                        lookupSector = GetSectorTryThroughPortal(x - 1, z);
                        heightsToCompare = new SectorEdge[2] { SectorEdge.XnZn, sector.Floor.DiagonalSplit == DiagonalSplit.XpZn ? SectorEdge.XnZn : SectorEdge.XnZp };
                        heightsToCheck = new SectorEdge[2] { SectorEdge.XpZn, SectorEdge.XpZp };
                        break;
                    default:
                        continue;
                }

                // Diagonal split resolver

                if (lookupSector.Sector.IsAnyWall && lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.None)
                {
                    // If lookup sector contains non-diagonal wall, we mark slope as illegal in any case.

                    slopeIsIllegal = true;
                    continue;
                }
                else if (lookupSector.Sector.Floor.DiagonalSplit != DiagonalSplit.None)
                {
                    // If lookup sector has diagonal splits...
                    // For floor diagonal steps and diagonal walls, only steps/walls that are facing away
                    // from split are always treated as potentially illegal, because steps facing towards
                    // split are resolved by engine position corrector nicely.

                    // Since in Tomb Editor diagonal steps are made that way so only single corner height
                    // is used for setting step height, we copy one of lookup sector's heightsToCheck to
                    // another.

                    switch (slopeDirections[i])
                    {
                        case Direction.PositiveZ:
                            if (lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XnZn ||
                                lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                            {
                                if (lookupSector.Sector.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                                heightsToCheck[0] = SectorEdge.XpZn;
                            else
                                heightsToCheck[1] = SectorEdge.XnZn;
                            break;
                        case Direction.PositiveX:
                            if (lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XnZn ||
                                lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                            {
                                if (lookupSector.Sector.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XpZp)
                                heightsToCheck[0] = SectorEdge.XnZn;
                            else
                                heightsToCheck[1] = SectorEdge.XnZp;
                            break;
                        case Direction.NegativeZ:
                            if (lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XpZp ||
                                lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                            {
                                if (lookupSector.Sector.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                                heightsToCheck[0] = SectorEdge.XnZp;
                            else
                                heightsToCheck[1] = SectorEdge.XpZp;
                            break;
                        case Direction.NegativeX:
                            if (lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XpZp ||
                                lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XpZn)
                            {
                                if (lookupSector.Sector.IsAnyWall)
                                {
                                    slopeIsIllegal = true;
                                    continue;
                                }
                            }
                            else if (lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XnZn)
                                heightsToCheck[0] = SectorEdge.XpZp;
                            else
                                heightsToCheck[1] = SectorEdge.XpZn;
                            break;
                    }
                }

                int heightAdjust = Position.Y - lookupSector.Room.Position.Y;
                int absoluteLowestPassableHeight = lowestPassableHeight + heightAdjust;
                int absoluteLowestPassableStep = lowestPassableStep + heightAdjust;

                // Main comparer

                if (lookupSector.Sector.FloorPortal == null)
                {
                    if (lookupSector.Sector.Floor.GetHeight(heightsToCheck[0]) - sector.Floor.GetHeight(heightsToCompare[0]) > absoluteLowestPassableStep ||
                        lookupSector.Sector.Floor.GetHeight(heightsToCheck[1]) - sector.Floor.GetHeight(heightsToCompare[1]) > absoluteLowestPassableStep)
                        slopeIsIllegal = true;
                    else
                    {
                        var lookupSectorSlopeDirections = lookupSector.Sector.GetFloorTriangleSlopeDirections(Sector.SlopeCalculationMode.Legacy);

                        for (int j = 0; j < 2; j++)
                        {
                            // If both current and lookup sector triangle split direction is the same, ignore far opposite triangles.
                            // FIXME: works only for normal sectors. Diagonal steps are broken!

                            if (!sector.Floor.IsQuad && !lookupSector.Sector.Floor.IsQuad)
                            {
                                var sectorSplitDirection = sector.Floor.DiagonalSplit == DiagonalSplit.None ? sector.Floor.SplitDirectionIsXEqualsZ : (int)sector.Floor.DiagonalSplit % 2 != 0;
                                var lookupSplitDirection = lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.None ? lookupSector.Sector.Floor.SplitDirectionIsXEqualsZ : (int)lookupSector.Sector.Floor.DiagonalSplit % 2 != 0;

                                if (sectorSplitDirection == lookupSplitDirection)
                                {
                                    if (sector.Floor.DiagonalSplit != DiagonalSplit.None && lookupSector.Sector.Floor.DiagonalSplit != DiagonalSplit.None)
                                        continue;

                                    if (sectorSplitDirection)
                                    {
                                        if ((slopeDirections[i] == Direction.PositiveZ || slopeDirections[i] == Direction.NegativeX) && i == 0 && j == 1 ||
                                            (slopeDirections[i] == Direction.PositiveX || slopeDirections[i] == Direction.NegativeZ) && i == 1 && j == 0)
                                            continue;
                                    }
                                    else
                                    {
                                        if (slopeDirections[i] < Direction.NegativeZ && i == 1 && j == 0 ||
                                            slopeDirections[i] >= Direction.NegativeZ && i == 0 && j == 1)
                                            continue;
                                    }
                                }
                            }

                            // This code is needed to get REAL triangle index for diagonal step cases, because for some reason, triangle indices are inverted in this case.

                            var realTriangleIndex = j;

                            if (lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XpZn || lookupSector.Sector.Floor.DiagonalSplit == DiagonalSplit.XnZp)
                                realTriangleIndex = realTriangleIndex == 0 ? 1 : 0;

                            // Triangle is considered illegal only if its lowest point lies lower than lowest passable step height compared to opposite triangle minimum point.
                            // Triangle is NOT considered illegal, if its slide direction is perpendicular to opposite triangle slide direction.

                            var heightDifference = sector.GetTriangleMinimumFloorPoint(i) - (lookupSector.Sector.GetTriangleMinimumFloorPoint(realTriangleIndex) - heightAdjust);

                            if (heightsToCheck[0] != heightsToCheck[1] && heightDifference <= lowestPassableStep || // Ordinary cases
                                heightsToCheck[0] == heightsToCheck[1] && heightDifference <= 0 && heightDifference > -lowestPassableStep)  // Diagonal step cases
                            {

                                if (lookupSectorSlopeDirections[j] != Direction.None && lookupSectorSlopeDirections[j] != slopeDirections[i])
                                    if ((int)lookupSectorSlopeDirections[j] % 2 == (int)slopeDirections[i] % 2)
                                        slopeIsIllegal = true;
                            }
                        }
                    }
                }

                if (lookupSector.Sector.CeilingPortal == null)
                {
                    if (lookupSector.Sector.Ceiling.GetHeight(heightsToCheck[0]) - sector.Floor.GetHeight(heightsToCompare[0]) < absoluteLowestPassableHeight ||
                        lookupSector.Sector.Ceiling.GetHeight(heightsToCheck[1]) - sector.Floor.GetHeight(heightsToCompare[1]) < absoluteLowestPassableHeight ||
                        lookupSector.Sector.Ceiling.GetHeight(heightsToCheck[0]) - lookupSector.Sector.Floor.GetHeight(heightsToCheck[0]) < lowestPassableHeight ||
                        lookupSector.Sector.Ceiling.GetHeight(heightsToCheck[1]) - lookupSector.Sector.Floor.GetHeight(heightsToCheck[1]) < lowestPassableHeight)
                        slopeIsIllegal = true;
                }
            }

            return slopeIsIllegal;
        }

        public bool IsAnyPortal(PortalDirection direction, RectangleInt2 area)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    switch(direction)
                    {
                        case PortalDirection.Floor:
                            if (Sectors[x, z].FloorPortal != null)
                                return true;
                            continue;
                        case PortalDirection.Ceiling:
                            if (Sectors[x, z].CeilingPortal != null)
                                return true;
                            continue;
                        default:
                            if (Sectors[x, z].WallPortal != null)
                                return true;
                            continue;
                    }
                }
            return false;
        }

        public bool CornerSelected(RectangleInt2 area)
        {
            // Check if one of the four corner is selected
            var cornerSelected = false;
            if (area.X0 == 0 && area.Y0 == 0 || area.X1 == 0 && area.Y1 == 0)
                cornerSelected = true;
            if (area.X0 == 0 && area.Y0 == NumZSectors - 1 || area.X1 == 0 && area.Y1 == NumZSectors - 1)
                cornerSelected = true;
            if (area.X0 == NumXSectors - 1 && area.Y0 == 0 || area.X1 == NumXSectors - 1 && area.Y1 == 0)
                cornerSelected = true;
            if (area.X0 == NumXSectors - 1 && area.Y0 == NumZSectors - 1 || area.X1 == NumXSectors - 1 && area.Y1 == NumZSectors - 1)
                cornerSelected = true;
            return cornerSelected;
        }

        public bool IsFaceDefined(int x, int z, SectorFace face)
        {
            return RoomGeometry.VertexRangeLookup.TryGetOrDefault(new SectorFaceIdentity(x, z, face)).Count != 0;
        }

        public float GetFaceHighestPoint(int x, int z, SectorFace face)
        {
            var range = RoomGeometry.VertexRangeLookup.TryGetOrDefault(new SectorFaceIdentity(x, z, face));
            float max = float.NegativeInfinity;
            for (int i = 0; i < range.Count; ++i)
                max = Math.Max(RoomGeometry.VertexPositions[i + range.Start].Y, max);
            return max;
        }

        public float GetFaceLowestPoint(int x, int z, SectorFace face)
        {
            var range = RoomGeometry.VertexRangeLookup.TryGetOrDefault(new SectorFaceIdentity(x, z, face));
            float max = float.PositiveInfinity;
            for (int i = 0; i < range.Count; ++i)
                max = Math.Min(RoomGeometry.VertexPositions[i + range.Start].Y, max);
            return max;
        }

        public FaceShape GetFaceShape(int x, int z, SectorFace face)
        {
            switch (RoomGeometry.VertexRangeLookup.TryGetOrDefault(new SectorFaceIdentity(x, z, face)).Count)
            {
                case 3:
                    return FaceShape.Triangle;
                case 6:
                    return FaceShape.Quad;
                default:
                    return FaceShape.Unknown;
            }
        }

        public void BuildGeometry(bool highQualityLighting = false, bool useLegacyCode = false)
        {
            RoomGeometry.Build(this, highQualityLighting, useLegacyCode);
        }

        public void RebuildLighting(bool highQualityLighting)
        {
            RoomGeometry.Relight(this, highQualityLighting);
        }

        public Matrix4x4 Transform => Matrix4x4.CreateTranslation(WorldPos);

		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public int GetHighestCorner(RectangleInt2 area)
        {
            area = area.Intersect(LocalArea);

            int max = int.MinValue;
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    if (!Sectors[x, z].IsAnyWall)
                        max = Math.Max(max, Sectors[x, z].Ceiling.Max);

            return max == int.MinValue ? DefaultHeight : max;
        }

		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public int GetHighestNeighborCeiling(int x, int z)
        {
            RoomSectorPair p1 = GetSectorTryThroughPortal(x - 1, z),
                p2 = GetSectorTryThroughPortal(x + 1, z),
                p3 = GetSectorTryThroughPortal(x, z - 1),
                p4 = GetSectorTryThroughPortal(x, z + 1);

            int max = int.MinValue;

            if (p1.Sector is not null && (p1.Sector.Type == SectorType.Floor || (p1.Sector.Type == SectorType.Wall && p1.Sector.Ceiling.DiagonalSplit != DiagonalSplit.None)))
                max = Math.Max(max, p1.Sector.Ceiling.Max + p1.Room.Position.Y);

            if (p2.Sector is not null && (p2.Sector.Type == SectorType.Floor || (p2.Sector.Type == SectorType.Wall && p2.Sector.Ceiling.DiagonalSplit != DiagonalSplit.None)))
                max = Math.Max(max, p2.Sector.Ceiling.Max + p2.Room.Position.Y);

            if (p3.Sector is not null && (p3.Sector.Type == SectorType.Floor || (p3.Sector.Type == SectorType.Wall && p3.Sector.Ceiling.DiagonalSplit != DiagonalSplit.None)))
                max = Math.Max(max, p3.Sector.Ceiling.Max + p3.Room.Position.Y);

            if (p4.Sector is not null && (p4.Sector.Type == SectorType.Floor || (p4.Sector.Type == SectorType.Wall && p4.Sector.Ceiling.DiagonalSplit != DiagonalSplit.None)))
                max = Math.Max(max, p4.Sector.Ceiling.Max + p4.Room.Position.Y);

            return max;
        }

		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public int GetHighestCorner()
        {
            return GetHighestCorner(new RectangleInt2(1, 1, NumXSectors - 2, NumZSectors - 2));
        }

		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public int GetLowestCorner(RectangleInt2 area)
        {
            area = area.Intersect(LocalArea);

            int min = int.MaxValue;
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    if (!Sectors[x, z].IsAnyWall)
                        min = Math.Min(min, Sectors[x, z].Floor.Min);

            return min == int.MaxValue ? 0 : min;
        }

		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public int GetLowestNeighborFloor(int x, int z)
        {
            RoomSectorPair p1 = GetSectorTryThroughPortal(x - 1, z),
                p2 = GetSectorTryThroughPortal(x + 1, z),
                p3 = GetSectorTryThroughPortal(x, z - 1),
                p4 = GetSectorTryThroughPortal(x, z + 1);

            int min = int.MaxValue;

            if (p1.Sector is not null && (p1.Sector.Type == SectorType.Floor || (p1.Sector.Type == SectorType.Wall && p1.Sector.Floor.DiagonalSplit != DiagonalSplit.None)))
                min = Math.Min(min, p1.Sector.Floor.Min + p1.Room.Position.Y);

            if (p2.Sector is not null && (p2.Sector.Type == SectorType.Floor || (p2.Sector.Type == SectorType.Wall && p2.Sector.Floor.DiagonalSplit != DiagonalSplit.None)))
                min = Math.Min(min, p2.Sector.Floor.Min + p2.Room.Position.Y);

            if (p3.Sector is not null && (p3.Sector.Type == SectorType.Floor || (p3.Sector.Type == SectorType.Wall && p3.Sector.Floor.DiagonalSplit != DiagonalSplit.None)))
                min = Math.Min(min, p3.Sector.Floor.Min + p3.Room.Position.Y);

            if (p4.Sector is not null && (p4.Sector.Type == SectorType.Floor || (p4.Sector.Type == SectorType.Wall && p4.Sector.Floor.DiagonalSplit != DiagonalSplit.None)))
                min = Math.Min(min, p4.Sector.Floor.Min + p4.Room.Position.Y);

            return min;
        }

		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public int GetLowestCorner()
        {
            return GetLowestCorner(new RectangleInt2(1, 1, NumXSectors - 2, NumZSectors - 2));
        }

        public VectorInt3 WorldPos
        {
            get { return new VectorInt3(Position.X * (int)Level.SectorSizeUnit, Position.Y, Position.Z * (int)Level.SectorSizeUnit); }
            set { Position = new VectorInt3(value.X / (int)Level.SectorSizeUnit, value.Y, value.Z / (int)Level.SectorSizeUnit); }
        }

        public Vector3 GetLocalCenter()
        {
            float ceilingHeight = GetHighestCorner();
            float floorHeight = GetLowestCorner();
            return new Vector3(
                NumXSectors * (0.5f * Level.SectorSizeUnit),
                (floorHeight + ceilingHeight) * 0.5f,
                NumZSectors * (0.5f * Level.SectorSizeUnit));
        }

        public Vector3 GetFloorMidpointPosition(float sectorX, float sectorZ)
        {
            var sector = GetSectorTry(new VectorInt2((int)sectorX, (int)sectorZ));
            int y = sector == null ? 0 : (sector.Floor.XnZp + sector.Floor.XpZp + sector.Floor.XpZn + sector.Floor.XnZn) / 4;
            return new Vector3(sectorX * Level.SectorSizeUnit + Level.HalfSectorSizeUnit, y, sectorZ * Level.SectorSizeUnit + Level.HalfSectorSizeUnit);
        }

        public int NumXSectors
        {
            get { return Sectors.GetLength(0); }
        }

        public int NumZSectors
        {
            get { return Sectors.GetLength(1); }
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>Transforms the coordinates of QAFaces in such a way that the lowest one falls on Y = 0</summary>
        public void NormalizeRoomY()
        {
            // Exclusive case: all sectors are walls
            bool allSectorsAreWalls = true;
            foreach (var sector in Sectors)
                if (!sector.IsAnyWall)
                {
                    allSectorsAreWalls = false;
                    break;
                }

            // Determine lowest QAFace
            int lowest = int.MaxValue;
            for (int z = 0; z < NumZSectors; z++)
                for (int x = 0; x < NumXSectors; x++)
                {
                    var b = Sectors[x, z];
                    if (allSectorsAreWalls || !b.IsAnyWall)
                        for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                            lowest = Math.Min(lowest, b.Floor.GetHeight(edge));
                }

            // Move room to new position
            Position += new VectorInt3(0, lowest, 0);

            // Transform room content in such a way, their world position is identical to before even though the room position changed
            for (int z = 0; z < NumZSectors; z++)
                for (int x = 0; x < NumXSectors; x++)
                {
                    var b = Sectors[x, z];
                    foreach (SectorVerticalPart vertical in b.GetVerticals())
                        for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                            b.SetHeight(vertical, edge, b.GetHeight(vertical, edge) - lowest);
                }

            foreach (var instance in _objects)
                instance.Position -= new Vector3(0, lowest, 0);
        }

        public static bool RemoveOutsidePortals(Level level, IEnumerable<Room> rooms, Func<IReadOnlyList<PortalInstance>, bool> beforeRemovePortals)
        {
            HashSet<Room> roomLookup = new HashSet<Room>(rooms);

            List<PortalInstance> portalsToRemove = new List<PortalInstance>();
            foreach (Room room in roomLookup)
                foreach (PortalInstance portal in room.Portals)
                    if (!roomLookup.Contains(portal.AdjoiningRoom))
                        portalsToRemove.Add(portal);

            if (portalsToRemove.Count == 0)
                return true;
            if (!beforeRemovePortals(portalsToRemove))
                return false;

            foreach (PortalInstance instance in portalsToRemove)
                if (instance.Room != null)
                    instance.Room.RemoveObject(level, instance);
            return true;
        }

        private bool AddObjectAndSingularPortalCutSectors(Level level, RectangleInt2 newArea, SectorBasedObjectInstance instance)
        {
            // Determine area
            RectangleInt2 instanceNewAreaConstraint = instance.GetValidArea(newArea);
            if (!instance.Area.Intersects(instanceNewAreaConstraint))
                return false;
            RectangleInt2 instanceNewArea = instance.Area.Intersect(instanceNewAreaConstraint) - newArea.Start;

            // Add object
            AddObjectAndSingularPortal(level, instance.Clone(instanceNewArea));
            return true;
        }

        public ObjectInstance AddObjectAndSingularPortal(Level level, ObjectInstance instance)
        {
            if (instance is PositionBasedObjectInstance)
                _objects.Add((PositionBasedObjectInstance)instance);
            else if (instance is GhostBlockInstance)
            {
                var ghost = instance as GhostBlockInstance;
                if (!CoordinateInvalid(ghost.SectorPosition))
                    Sectors[ghost.SectorPosition.X, ghost.SectorPosition.Y].GhostBlock = ghost;
            }

            try
            {
                instance.AddToRoom(level, this);
            }
            catch
            { // If we fail, remove the object from the list
                if (instance is PositionBasedObjectInstance)
                {
                    _objects.Remove((PositionBasedObjectInstance)instance);
                }
                else if (instance is GhostBlockInstance)
                {
                    var ghost = instance as GhostBlockInstance;
                    if (!CoordinateInvalid(ghost.SectorPosition))
                        Sectors[ghost.SectorPosition.X, ghost.SectorPosition.Y].GhostBlock = null;
                }
                throw;
            }

            return instance;
        }

        public ObjectInstance RemoveObjectAndSingularPortalAndKeepAlive(Level level, ObjectInstance instance)
        {
            instance.RemoveFromRoom(level, this);
            if (instance is PositionBasedObjectInstance)
            {
                _objects.Remove((PositionBasedObjectInstance)instance);
            }
            else if (instance is GhostBlockInstance)
            {
                var ghost = instance as GhostBlockInstance;
                if (!CoordinateInvalid(ghost.SectorPosition))
                    Sectors[ghost.SectorPosition.X, ghost.SectorPosition.Y].GhostBlock = null;
            }

            return instance;
        }

        public ObjectInstance RemoveObjectAndSingularPortal(Level level, ObjectInstance instance)
        {
            instance = RemoveObjectAndSingularPortalAndKeepAlive(level, instance);
            instance.Delete();
            return instance;
        }

        public IEnumerable<ObjectInstance> AddObject(Level level, ObjectInstance instance)
        {
            // Add portals and opposite portals
            var portal = instance as PortalInstance;
            if (portal != null)
            {
                RectangleInt2 oppositeArea = PortalInstance.GetOppositePortalArea(portal.Direction, portal.Area) + (SectorPos - portal.AdjoiningRoom.SectorPos);
                var oppositePortal = new PortalInstance(oppositeArea, PortalInstance.GetOppositeDirection(portal.Direction), this);

                // Add portals
                var addedObjects = new List<ObjectInstance>();
                try
                {
                    addedObjects.Add(AddObjectAndSingularPortal(level, portal));
                    if (AlternateOpposite != null)
                        addedObjects.Add(AlternateOpposite.AddObjectAndSingularPortal(level, (PortalInstance)portal.Clone()));

                    addedObjects.Add(portal.AdjoiningRoom.AddObjectAndSingularPortal(level, oppositePortal));
                    if (portal.AdjoiningRoom.AlternateOpposite != null)
                        addedObjects.Add(portal.AdjoiningRoom.AlternateOpposite.AddObjectAndSingularPortal(level, (PortalInstance)oppositePortal.Clone()));
                }
                catch
                {
                    foreach (ObjectInstance instanceToRemove in addedObjects)
                        instanceToRemove.Room.RemoveObjectAndSingularPortal(level, instanceToRemove);
                    throw;
                }
                return addedObjects;
            }

            // Add normal object
            AddObjectAndSingularPortal(level, instance);
            return new[] { instance };
        }

        public IEnumerable<ObjectInstance> RemoveObjectAndKeepAlive(Level level, ObjectInstance instance)
        {
            var result = new List<ObjectInstance>();
            result.Add(RemoveObjectAndSingularPortalAndKeepAlive(level, instance));

            // Delete the corresponding other portals if necessary.
            var portal = instance as PortalInstance;
            if (portal != null)
            {
                var alternatePortal = portal.FindAlternatePortal(this.AlternateOpposite);
                if (alternatePortal != null)
                    result.Add(AlternateOpposite.RemoveObjectAndSingularPortal(level, alternatePortal));

                var oppositePortal = portal.FindOppositePortal(this);
                if (oppositePortal != null)
                    result.Add(portal.AdjoiningRoom.RemoveObjectAndSingularPortal(level, oppositePortal));

                var oppositeAlternatePortal = oppositePortal?.FindAlternatePortal(portal.AdjoiningRoom?.AlternateOpposite);
                if (oppositeAlternatePortal != null)
                    result.Add(oppositeAlternatePortal.Room.RemoveObjectAndSingularPortal(level, oppositeAlternatePortal));
            }

            return result;
        }

        public IEnumerable<ObjectInstance> RemoveObject(Level level, ObjectInstance instance)
        {
            IEnumerable<ObjectInstance> result = RemoveObjectAndKeepAlive(level, instance);
            foreach (ObjectInstance resultInstance in result)
                resultInstance.Delete();
            return result;
        }

        public void MoveObjectFrom(Level level, Room from, PositionBasedObjectInstance instance)
        {
            from.RemoveObject(level, instance);
            instance.Position += from.WorldPos - WorldPos;
            AddObject(level, instance);
        }

        // Fixes portals in neighboring rooms to match those in the current room.
        // This will preserve properties of the portals, i.e. the opacity.
        public static void FixupNeighborPortals(Level level, IEnumerable<Room> newRooms_, IEnumerable<Room> oldRooms_, ref HashSet<Room> relevantRooms)
        {
            // Required base room
            List<Room> newRooms = newRooms_.Select(room => room.AlternateBaseRoom ?? room).ToList();
            List<Room> oldRooms = oldRooms_.Select(room => room.AlternateBaseRoom ?? room).ToList();

            // Add other certainly relevant rooms
            foreach (Room room in newRooms)
                relevantRooms.Add(room);
            foreach (var relevantRoom in relevantRooms.ToArray())
                if (relevantRoom.AlternateOpposite != null)
                    relevantRooms.Add(relevantRoom.AlternateOpposite);

            // Remove old portals from those rooms
            var oldPortals = new List<KeyValuePair<Room, PortalInstance>>();
            foreach (Room relevantRoom in relevantRooms)
                if (!newRooms.Contains(relevantRoom) && !newRooms.Contains(relevantRoom.AlternateOpposite))
                    foreach (PortalInstance portal in relevantRoom.Portals.ToList())
                        if (oldRooms.Contains(portal.AdjoiningRoom))
                        {
                            relevantRoom.RemoveObjectAndSingularPortalAndKeepAlive(level, portal);
                            oldPortals.Add(new KeyValuePair<Room, PortalInstance>(relevantRoom, portal));
                        }

            // A helper function
            Func<Room, Room, PortalInstance, PortalInstance> findBestMatch = (room, roomForWhichToFindMatch, portal) =>
            {
                RectangleInt2 oppositeArea = PortalInstance.GetOppositePortalArea(portal.Direction, portal.Area) + room.SectorPos;
                PortalDirection oppositeDirection = PortalInstance.GetOppositeDirection(portal.Direction);

                // Find portal that is closest to the needed spot
                int bestMatchArea = 0;
                PortalInstance bestMatch = null;
                foreach (var oldPortal in oldPortals)
                    if (oldPortal.Key == roomForWhichToFindMatch &&
                        oldPortal.Value.Direction == oppositeDirection &&
                        oldPortal.Value.Area.Intersects(oppositeArea - oldPortal.Key.SectorPos))
                    {
                        RectangleInt2 area = oldPortal.Value.Area.Intersect(oppositeArea - oldPortal.Key.SectorPos);
                        int newMatchArea = (area.Width + 1) * (area.Height + 1);
                        if (newMatchArea > bestMatchArea)
                        {
                            bestMatchArea = newMatchArea;
                            bestMatch = oldPortal.Value;
                        }
                    }
                if (bestMatch != null)
                {
                    bestMatch.AdjoiningRoom = room;
                    return (PortalInstance)bestMatch.Clone(oppositeArea - portal.AdjoiningRoom.SectorPos);
                }
                return new PortalInstance(oppositeArea - portal.AdjoiningRoom.SectorPos, PortalInstance.GetOppositeDirection(portal.Direction), room);
            };

            // Add new portals where necessary
            foreach (Room room in newRooms)
                foreach (PortalInstance portal in room.Portals)
                    if (!newRooms.Contains(portal.AdjoiningRoom))
                    {
                        var newPortal = findBestMatch(room, portal.AdjoiningRoom, portal);
                        if (newPortal.IsValid(portal.AdjoiningRoom))
                        {
                            portal.AdjoiningRoom.AddObjectAndSingularPortal(level, newPortal);
                            relevantRooms.Add(portal.AdjoiningRoom);
                        }
                        if (portal.AdjoiningRoom.AlternateOpposite != null)
                        {
                            newPortal = findBestMatch(room, portal.AdjoiningRoom.AlternateOpposite, portal);
                            if (newPortal.IsValid(portal.AdjoiningRoom.AlternateOpposite))
                            {
                                portal.AdjoiningRoom.AlternateOpposite.AddObjectAndSingularPortal(level, newPortal);
                                relevantRooms.Add(portal.AdjoiningRoom.AlternateOpposite);
                            }
                        }
                    }

            // Cleanup
            foreach (var oldPortal in oldPortals)
                oldPortal.Value.Delete();
        }

        public enum RoomConnectionType
        {
            NoPortal,
            TriangularPortalXnZn,
            TriangularPortalXpZn,
            TriangularPortalXnZp,
            TriangularPortalXpZp,
            FullPortal
        }

        public static RoomConnectionType Combine(RoomConnectionType first, RoomConnectionType second)
        {
            if (first == second || first == RoomConnectionType.FullPortal)
                return second;
            else
                return RoomConnectionType.NoPortal;
        }

        public struct RoomConnectionInfo
        {
            public PortalInstance Portal { get; }
            public RoomConnectionType AnyType { get; }

            public RoomConnectionInfo(PortalInstance portal, RoomConnectionType anyType)
            {
                Portal = portal;
                AnyType = anyType;
            }

            ///<summary>Gives how the sector visually appears regarding portals</summary>
            public RoomConnectionType VisualType => (Portal?.HasTexturedFaces ?? true) ? RoomConnectionType.NoPortal : AnyType;
            ///<summary>Gives how the sector geometrically behaves regarding portals</summary>
            public RoomConnectionType TraversableType => (Portal?.IsTraversable ?? false) ? AnyType : RoomConnectionType.NoPortal;

            public bool IsTriangularPortal => !(TraversableType == RoomConnectionType.FullPortal || TraversableType == RoomConnectionType.NoPortal);
        }

        public static IEnumerable<KeyValuePair<Room, Room>> GetPossibleAlternateRoomPairs(Room firstRoom, Room secondRoom, bool lookingFromSecond = false)
        {
            if (firstRoom.AlternateOpposite == null)
            {
                if (secondRoom.AlternateOpposite == null)
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom);
                else
                {
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom);
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom.AlternateOpposite);
                }
            }
            else
            {
                if (secondRoom.AlternateOpposite == null)
                {
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom);
                    yield return new KeyValuePair<Room, Room>(firstRoom.AlternateOpposite, secondRoom);
                }
                else if (firstRoom.AlternateGroup == secondRoom.AlternateGroup)
                {
                    bool isAlternateCurrently = lookingFromSecond ? secondRoom.AlternateBaseRoom != null : firstRoom.AlternateBaseRoom != null;
                    if (isAlternateCurrently)
                        yield return new KeyValuePair<Room, Room>(firstRoom.AlternateRoom ?? firstRoom, secondRoom.AlternateRoom ?? secondRoom);
                    else
                        yield return new KeyValuePair<Room, Room>(firstRoom.AlternateBaseRoom ?? firstRoom, secondRoom.AlternateBaseRoom ?? secondRoom);
                }
                else
                {
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom);
                    yield return new KeyValuePair<Room, Room>(firstRoom.AlternateOpposite, secondRoom);
                    yield return new KeyValuePair<Room, Room>(firstRoom, secondRoom.AlternateOpposite);
                    yield return new KeyValuePair<Room, Room>(firstRoom.AlternateOpposite, secondRoom.AlternateOpposite);
                }
            }
        }

        public static RoomConnectionType CalculateRoomConnectionTypeWithoutAlternates(Room roomBelow, Room roomAbove, Sector sectorBelow, Sector sectorAbove, bool includeGhostBlock = false)
        {
            // Evaluate force floor solid
            if (sectorAbove.ForceFloorSolid)
                return RoomConnectionType.NoPortal;

            // Check walls
            DiagonalSplit aboveDiagonalSplit = sectorAbove.Floor.DiagonalSplit;
            DiagonalSplit belowDiagonalSplit = sectorBelow.Ceiling.DiagonalSplit;
            if (sectorAbove.IsAnyWall && aboveDiagonalSplit == DiagonalSplit.None)
                return RoomConnectionType.NoPortal;
            if (sectorBelow.IsAnyWall && belowDiagonalSplit == DiagonalSplit.None)
                return RoomConnectionType.NoPortal;
            if (sectorBelow.IsAnyWall && sectorAbove.IsAnyWall && aboveDiagonalSplit != belowDiagonalSplit)
                return RoomConnectionType.NoPortal;

            // Gather split data
            bool belowSplitXEqualsZ = sectorBelow.Ceiling.SplitDirectionIsXEqualsZWithDiagonalSplit;
            bool aboveSplitXEqualsZ = sectorAbove.Floor.SplitDirectionIsXEqualsZWithDiagonalSplit;

            // Take possible ghost sectors into account
            var realSectorBelowCeiling = (includeGhostBlock && sectorBelow.HasGhostBlock) ? sectorBelow.Ceiling + sectorBelow.GhostBlock.Ceiling : sectorBelow.Ceiling;
            var realSectorAboveFloor   = (includeGhostBlock && sectorAbove.HasGhostBlock) ? sectorAbove.Floor + sectorAbove.GhostBlock.Floor : sectorAbove.Floor;

            bool belowIsQuad = realSectorBelowCeiling.IsQuad;
            bool aboveIsQuad = realSectorAboveFloor.IsQuad;

            // Check where the geometry matches to create a portal
            if (belowIsQuad || aboveIsQuad || belowSplitXEqualsZ == aboveSplitXEqualsZ)
            {
                DiagonalSplit diagonalSplit = aboveDiagonalSplit != DiagonalSplit.None ? aboveDiagonalSplit : belowDiagonalSplit;
                bool splitXEqualsZ = belowIsQuad ? aboveSplitXEqualsZ : belowSplitXEqualsZ;

                bool matchesAtXnYn = roomBelow.Position.Y + realSectorBelowCeiling.GetActualMax(SectorEdge.XnZn) == roomAbove.Position.Y + realSectorAboveFloor.GetActualMin(SectorEdge.XnZn);
                bool matchesAtXpYn = roomBelow.Position.Y + realSectorBelowCeiling.GetActualMax(SectorEdge.XpZn) == roomAbove.Position.Y + realSectorAboveFloor.GetActualMin(SectorEdge.XpZn);
                bool matchesAtXnYp = roomBelow.Position.Y + realSectorBelowCeiling.GetActualMax(SectorEdge.XnZp) == roomAbove.Position.Y + realSectorAboveFloor.GetActualMin(SectorEdge.XnZp);
                bool matchesAtXpYp = roomBelow.Position.Y + realSectorBelowCeiling.GetActualMax(SectorEdge.XpZp) == roomAbove.Position.Y + realSectorAboveFloor.GetActualMin(SectorEdge.XpZp);

                if (matchesAtXnYn && matchesAtXpYn && matchesAtXnYp && matchesAtXpYp && !(sectorAbove.IsAnyWall || sectorBelow.IsAnyWall))
                    return RoomConnectionType.FullPortal;
                if ((belowIsQuad || belowSplitXEqualsZ) && (aboveIsQuad || splitXEqualsZ))
                { // Try to make a triangular portal split which is parallel to X=Z
                    if (matchesAtXnYn && matchesAtXnYp && matchesAtXpYp && (diagonalSplit == DiagonalSplit.XpZn || !(sectorAbove.IsAnyWall || sectorBelow.IsAnyWall)))
                        return RoomConnectionType.TriangularPortalXnZp;
                    if (matchesAtXnYn && matchesAtXpYn && matchesAtXpYp && (diagonalSplit == DiagonalSplit.XnZp || !(sectorAbove.IsAnyWall || sectorBelow.IsAnyWall)))
                        return RoomConnectionType.TriangularPortalXpZn;
                }
                if ((belowIsQuad || !belowSplitXEqualsZ) && (aboveIsQuad || !splitXEqualsZ))
                { // Try to make a triangular portal split which is perpendicular to X=Z
                    if (matchesAtXpYn && matchesAtXnYp && matchesAtXpYp && (diagonalSplit == DiagonalSplit.XnZn || !(sectorAbove.IsAnyWall || sectorBelow.IsAnyWall)))
                        return RoomConnectionType.TriangularPortalXpZp;
                    if (matchesAtXnYn && matchesAtXpYn && matchesAtXnYp && (diagonalSplit == DiagonalSplit.XpZp || !(sectorAbove.IsAnyWall || sectorBelow.IsAnyWall)))
                        return RoomConnectionType.TriangularPortalXnZn;
                }
            }
            return RoomConnectionType.NoPortal;
        }

        public static RoomConnectionType CalculateRoomConnectionType(Room roomBelow, Room roomAbove, VectorInt2 posBelow, VectorInt2 posAbove, bool lookingFromAbove, bool includeGhostBlock = false)
        {
            // Checkout all possible alternate room combinations and combine the results
            RoomConnectionType result = RoomConnectionType.FullPortal;
            foreach (var roomPair in GetPossibleAlternateRoomPairs(roomBelow, roomAbove, lookingFromAbove))
                result = Combine(result,
                    CalculateRoomConnectionTypeWithoutAlternates(roomPair.Key, roomPair.Value,
                        roomPair.Key.GetSector(posBelow), roomPair.Value.GetSector(posAbove), includeGhostBlock));
            return result;
        }

        public RoomConnectionInfo GetFloorRoomConnectionInfo(VectorInt2 pos, bool includeGhostBlock = false)
        {
            Sector sector = GetSector(pos);
            if (sector.FloorPortal != null)
            {
                Room adjoiningRoom = sector.FloorPortal.AdjoiningRoom;
                VectorInt2 adjoiningPos = pos + (SectorPos - adjoiningRoom.SectorPos);
                Sector adjoiningSector = adjoiningRoom.GetSectorTry(adjoiningPos);
                if (adjoiningSector?.CeilingPortal != null)
                    return new RoomConnectionInfo(sector.FloorPortal, CalculateRoomConnectionType(adjoiningRoom, this, adjoiningPos, pos, true, includeGhostBlock));
            }
            return new RoomConnectionInfo();
        }

        public RoomConnectionInfo GetCeilingRoomConnectionInfo(VectorInt2 pos, bool includeGhostBlock = false)
        {
            Sector sector = GetSector(pos);
            if (sector.CeilingPortal != null)
            {
                Room adjoiningRoom = sector.CeilingPortal.AdjoiningRoom;
                VectorInt2 adjoiningPos = pos + (SectorPos - adjoiningRoom.SectorPos);
                Sector adjoiningSector = adjoiningRoom.GetSectorTry(adjoiningPos);
                if (adjoiningSector?.FloorPortal != null)
                    return new RoomConnectionInfo(sector.CeilingPortal, CalculateRoomConnectionType(this, adjoiningRoom, pos, adjoiningPos, false, includeGhostBlock));
            }
            return new RoomConnectionInfo();
        }

        public void SmartBuildGeometry(RectangleInt2 area, bool highQualityLighting = false)
        {
            area = area.Inflate(1); // Add margin

            // Update adjoining rooms
            var roomsToProcess = new List<Room> { this };
            var areaToProcess = new List<RectangleInt2> { area };
            foreach (var portal in Portals)
            {
                if (!portal.Area.Intersects(area))
                    continue; // This portal is irrelavant since no changes happend in its area

                RectangleInt2 portalArea = portal.Area.Intersect(area);
                RectangleInt2 otherRoomPortalArea = PortalInstance.GetOppositePortalArea(portal.Direction, portalArea) + (SectorPos - portal.AdjoiningRoom.SectorPos);

                // Find all related rooms or alternate rooms around the portals
                foreach (var roomPairs in GetPossibleAlternateRoomPairs(this, portal.AdjoiningRoom))
                {
                    // Add itself if necessary
                    int thisRoomIndex = roomsToProcess.IndexOf(roomPairs.Key);
                    if (thisRoomIndex == -1)
                    {
                        roomsToProcess.Add(roomPairs.Key);
                        areaToProcess.Add(area);
                    }
                    else
                        areaToProcess[thisRoomIndex] = areaToProcess[thisRoomIndex].Union(area);

                    // Add other if necessary
                    int adjoiningRoomIndex = roomsToProcess.IndexOf(roomPairs.Value);
                    if (adjoiningRoomIndex == -1)
                    {
                        roomsToProcess.Add(roomPairs.Value);
                        areaToProcess.Add(otherRoomPortalArea);
                    }
                    else
                        areaToProcess[adjoiningRoomIndex] = areaToProcess[adjoiningRoomIndex].Union(otherRoomPortalArea);
                }
            }
            System.Diagnostics.Debug.Assert(roomsToProcess.Count == areaToProcess.Count);

            // Update the collected stuff now
            Parallel.For(0, roomsToProcess.Count, index =>
            {
                roomsToProcess[index].BuildGeometry(highQualityLighting);
            });
        }

        public class CopyDependentLevelSettingsArgs
        {
            public LevelSettings DestinationLevelSettings { get; }
            public LevelSettings SourceLevelSettings { get; }
            public bool UnifyData { get; }
            private Dictionary<ushort, ushort> FlyBySequenceReassociation { get; } = new Dictionary<ushort, ushort>();
            private readonly HashSet<ushort> UsedFlyBySequences = new HashSet<ushort>();
            public CopyDependentLevelSettingsArgs(IEnumerable<ObjectInstance> oldInstances, LevelSettings destinationLevelSettings, LevelSettings sourceLevelSettings, bool unifyData)
            {
                DestinationLevelSettings = destinationLevelSettings;
                SourceLevelSettings = sourceLevelSettings;
                UnifyData = unifyData;

                if (oldInstances != null)
                    foreach (var flyBys in oldInstances.OfType<FlybyCameraInstance>())
                        UsedFlyBySequences.Add(flyBys.Sequence);
            }

            public ushort ReassociateFlyBySequence(ushort sequenceId)
            {
                // Is that sequence already assigned to a new sequence Id?
                ushort result;
                if (FlyBySequenceReassociation.TryGetValue(sequenceId, out result))
                    return result;

                // Find unused sequence ID
                if (!UsedFlyBySequences.Contains(sequenceId))
                    result = sequenceId;
                else
                {
                    result = 0;
                    while (UsedFlyBySequences.Contains(result))
                        result = checked((ushort)(result + 1));
                }

                // Assign and return ID
                UsedFlyBySequences.Add(result);
                FlyBySequenceReassociation.Add(sequenceId, result);
                return result;
            }
        }

        public void CopyDependentLevelSettings(CopyDependentLevelSettingsArgs args)
        {
            foreach (ObjectInstance instance in AnyObjects)
                instance.CopyDependentLevelSettings(args);

            if (args.DestinationLevelSettings.Textures.Count == 0)
            {
                args.DestinationLevelSettings.Textures.AddRange(args.SourceLevelSettings.Textures);
                args.DestinationLevelSettings.AnimatedTextureSets.AddRange(args.SourceLevelSettings.AnimatedTextureSets);
            }
            else if (args.UnifyData)
            {
                bool LookForAnimatedSets = false;
                Cache<LevelTexture, LevelTexture> TextureRemap = null;
                TextureRemap = new Cache<LevelTexture, LevelTexture>(32,
                    sourceTexture =>
                    {
                        foreach (LevelTexture destinationTexture in args.DestinationLevelSettings.Textures)
                            if (destinationTexture == sourceTexture || destinationTexture.Equals(sourceTexture))
                                return destinationTexture;

                        // Copy the texture
                        args.DestinationLevelSettings.Textures.Add(sourceTexture);
                        LookForAnimatedSets = true;
                        return sourceTexture;
                    });

                for (int z = 0; z < NumZSectors; ++z)
                    for (int x = 0; x < NumXSectors; ++x)
                    {
                        Sector sector = Sectors[x, z];
                        foreach (SectorFace face in sector.GetFaceTextures().Keys)
                        {
                            TextureArea textureArea = sector.GetFaceTexture(face);
                            var sourceTexture = textureArea.Texture as LevelTexture;
                            if (sourceTexture != null)
                            {
                                textureArea.Texture = TextureRemap[sourceTexture]; // Remap source texture to destination texture.
                                if (LookForAnimatedSets)
                                {
                                    // Copy animated texture sets
                                    var setsToCopy = args.SourceLevelSettings.AnimatedTextureSets.Where(set => set.Frames.Any(frame => frame.Texture == sourceTexture));
                                    foreach (AnimatedTextureSet setToCopy in setsToCopy)
                                    {
                                        AnimatedTextureSet newSet = setToCopy.Clone();
                                        foreach (AnimatedTextureFrame frame in newSet.Frames)
                                            frame.Texture = TextureRemap[(LevelTexture)frame.Texture]; // Remap source textures to destination textures.
                                        args.DestinationLevelSettings.AnimatedTextureSets.Add(newSet);
                                    }
                                    LookForAnimatedSets = false;
                                }
                                sector.SetFaceTexture(face, textureArea);
                            }
                        }
                    }
            }
        }

        public HashSet<Texture> GetTextures()
        {
            HashSet<Texture> result = new HashSet<Texture>();
            for (int z = 0; z < NumZSectors; ++z)
                for (int x = 0; x < NumXSectors; ++x)
                {
                    Sector sector = Sectors[x, z];
                    foreach (SectorFace face in sector.GetFaceTextures().Keys)
                        result.Add(sector.GetFaceTexture(face).Texture);
                }
            return result;
        }

        // This is simplified version of level compiler's room geometry generator which only roughly counts
        // vertices and faces without generating actual geometry.

        public void GetGeometryStatistics(out int vertices, out int faces)
        {
            // Add room geometry

            var vertexPositions = RoomGeometry.VertexPositions;
            var vertexColors = RoomGeometry.VertexColors;

            var roomVerticesDictionary = new Dictionary<int, ushort>();
            var roomVertices  = new List<tr_room_vertex>();

            faces = 0;
            vertices = 0;

            // Add room's own geometry

            if (!Properties.Hidden)
                for (int z = 0; z < NumZSectors; ++z)
                    for (int x = 0; x < NumXSectors; ++x)
                        foreach (SectorFace face in Sectors[x, z].GetFaceTextures().Keys)
                        {
                            var range = RoomGeometry.VertexRangeLookup.TryGetOrDefault(new SectorFaceIdentity(x, z, face));
                            var shape = GetFaceShape(x, z, face);

                            if (range.Count == 0)
                                continue;

                            var texture = Sectors[x, z].GetFaceTexture(face);
                            if (texture.TextureIsInvisible || texture.TextureIsUnavailable ||
                                (shape == FaceShape.Triangle && texture.TriangleCoordsOutOfBounds) || (shape == FaceShape.Quad && texture.QuadCoordsOutOfBounds))
                                continue;

                            var doubleSided = Level.Settings.GameVersion.Native() > TRVersion.Game.TR2 && texture.DoubleSided;
                            var copyFace = Level.Settings.GameVersion.Native() <= TRVersion.Game.TR2 && texture.DoubleSided;

                            int rangeEnd = range.Start + range.Count;
                            for (int i = range.Start; i < rangeEnd; i += 3)
                            {
                                ushort vertex0Index, vertex1Index, vertex2Index;

                                if (shape == FaceShape.Quad)
                                {
                                    if (face == SectorFace.Ceiling)
                                    {
                                        LevelCompilerClassicTR.GetOrAddVertex(this, roomVerticesDictionary, roomVertices, vertexPositions[i + 1], vertexColors[i + 1]);
                                        LevelCompilerClassicTR.GetOrAddVertex(this, roomVerticesDictionary, roomVertices, vertexPositions[i + 2], vertexColors[i + 2]);
                                        LevelCompilerClassicTR.GetOrAddVertex(this, roomVerticesDictionary, roomVertices, vertexPositions[i + 0], vertexColors[i + 0]);
                                        LevelCompilerClassicTR.GetOrAddVertex(this, roomVerticesDictionary, roomVertices, vertexPositions[i + 5], vertexColors[i + 5]);
                                    }
                                    else
                                    {
                                        LevelCompilerClassicTR.GetOrAddVertex(this, roomVerticesDictionary, roomVertices, vertexPositions[i + 3], vertexColors[i + 3]);
                                        LevelCompilerClassicTR.GetOrAddVertex(this, roomVerticesDictionary, roomVertices, vertexPositions[i + 2], vertexColors[i + 2]);
                                        LevelCompilerClassicTR.GetOrAddVertex(this, roomVerticesDictionary, roomVertices, vertexPositions[i + 0], vertexColors[i + 0]);
                                        LevelCompilerClassicTR.GetOrAddVertex(this, roomVerticesDictionary, roomVertices, vertexPositions[i + 1], vertexColors[i + 1]);
                                    }

                                    faces++;

                                    i += 3;
                                }
                                else
                                {
                                    vertex0Index = LevelCompilerClassicTR.GetOrAddVertex(this, roomVerticesDictionary, roomVertices, vertexPositions[i + 0], vertexColors[i + 0]);
                                    vertex1Index = LevelCompilerClassicTR.GetOrAddVertex(this, roomVerticesDictionary, roomVertices, vertexPositions[i + 1], vertexColors[i + 1]);
                                    vertex2Index = LevelCompilerClassicTR.GetOrAddVertex(this, roomVerticesDictionary, roomVertices, vertexPositions[i + 2], vertexColors[i + 2]);

                                    faces++;
                                }

                                if (copyFace)
                                    faces++;
                            }
                        }

            // Merged static meshes

            foreach (var staticMesh in Objects.OfType<StaticInstance>())
            {
                // Сheck if static Mesh is in the Auto Merge list
                var entry = Level.Settings.AutoStaticMeshMerges.Find((mergeEntry) =>
                    mergeEntry.meshId == staticMesh.WadObjectId.TypeId);

                if (entry == null)
                    continue;

                var wadStatic = Level.Settings.WadTryGetStatic(staticMesh.WadObjectId);

                // This null check prevents TE from crashing in case if user deleted a wad or legacy wad
                // is currently locked and saving by utils such as strpix.

                if (wadStatic == null || wadStatic.Mesh == null)
                    continue;

                for (int j = 0; j < wadStatic.Mesh.VertexPositions.Count; j++)
                {
                    Vector3 position = wadStatic.Mesh.VertexPositions[j];
                    var trVertex = new tr_room_vertex
                    {
                        Position = new tr_vertex
                        {
                            X = (short)position.X,
                            Y = (short)-(position.Y + WorldPos.Y),
                            Z = (short)position.Z
                        }
                    };
                    roomVertices.Add(trVertex);
                }

                faces += wadStatic.Mesh.Polys.Count;
            }

            // Add geometry imported objects

            foreach (var geometry in Objects.OfType<ImportedGeometryInstance>())
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

                    for (int j = 0; j < mesh.Vertices.Count; j++)
                    {
                        var vertex = mesh.Vertices[j];

                        // Apply the transform to the vertex
                        var position = MathC.HomogenousTransform(vertex.Position, worldTransform);
                        var trVertex = new tr_room_vertex
                        {
                            Position = new tr_vertex
                            {
                                X = (short)position.X,
                                Y = (short)-(position.Y + WorldPos.Y),
                                Z = (short)position.Z
                            },
                            Lighting1 = 0,
                            Lighting2 = 0,
                            Attributes = 0
                        };

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
                            existingIndex = roomVertices.IndexOf(v => v.Position == trVertex.Position && v.Color == trVertex.Color);
                            if (existingIndex == -1)
                            {
                                existingIndex = roomVertices.Count;
                                roomVertices.Add(trVertex);
                            }
                        }

                        indexList.Add(existingIndex);
                        currentMeshIndexCount++;
                    }

                    foreach (var submesh in mesh.Submeshes)
                    {
                        for (int j = 0; j < submesh.Value.Indices.Count; j += 3)
                        {
                            ushort index0 = (ushort)(indexList[j + baseIndex + 0]);
                            ushort index1 = (ushort)(indexList[j + baseIndex + 1]);
                            ushort index2 = (ushort)(indexList[j + baseIndex + 2]);

                            var doubleSided = Level.Settings.GameVersion.Native() > TRVersion.Game.TR2 && submesh.Key.DoubleSided;
                            var copyFace = Level.Settings.GameVersion.Native() <= TRVersion.Game.TR2 && submesh.Key.DoubleSided;

                            faces++;

                            if (copyFace)
                                faces++;
                        }
                    }
                    baseIndex += currentMeshIndexCount;
                }
            }

            vertices = roomVertices.Count;
        }

        public IEnumerable<SectorVerticalPart> GetValidFloorSplitsForSector(int x, int z)
        {
            Room targetRoom = this;
            Sector targetSector = Sectors[x, z];

            if (targetSector.WallPortal == null)
            {
                int lowestNeighborFloor = GetLowestNeighborFloor(x, z);

                for (int i = 0; i < targetSector.ExtraFloorSplits.Count; i++)
                {
                    SectorSplit split = targetSector.ExtraFloorSplits[i];
                    bool isInVoid = split.Max + targetRoom.Position.Y <= lowestNeighborFloor;

                    if (!isInVoid)
                        yield return SectorVerticalPartExtensions.GetExtraFloorSplit(i);
                }
            }
        }

        public bool IsFloorSplitInVoid(SectorVerticalPart vertical, int x, int z, out int lowestNeighborFloor)
        {
            Room targetRoom = this;
            Sector targetSector = Sectors[x, z];

            if (targetSector.WallPortal != null)
            {
                RoomSectorPair pair = GetSectorTryThroughPortal(x, z);

                if (pair.Room != this)
                {
                    targetRoom = pair.Room;
                    targetSector = pair.Sector;
                }
            }

            int relativeLowestNeighborFloor = GetLowestNeighborFloor(x, z);
            lowestNeighborFloor = relativeLowestNeighborFloor - targetRoom.Position.Y;

            if (vertical is SectorVerticalPart.QA)
            {
                bool isInVoid = targetSector.Floor.Max + targetRoom.Position.Y <= relativeLowestNeighborFloor;
                return isInVoid;
            }
            else
            {
                SectorSplit split = targetSector.ExtraFloorSplits.ElementAtOrDefault(vertical.GetExtraSplitIndex());

                if (split is null)
                    return true;

                bool isInVoid = split.Max + targetRoom.Position.Y <= relativeLowestNeighborFloor;
                return isInVoid;
            }
        }

        public IEnumerable<SectorVerticalPart> GetValidCeilingSplitsForSector(int x, int z)
        {
            Room targetRoom = this;
            Sector targetSector = Sectors[x, z];

            if (targetSector.WallPortal == null)
            {
                int highestNeighborCeiling = GetHighestNeighborCeiling(x, z);

                for (int i = 0; i < targetSector.ExtraCeilingSplits.Count; i++)
                {
                    SectorSplit split = targetSector.ExtraCeilingSplits[i];
                    bool isInVoid = split.Min + targetRoom.Position.Y >= highestNeighborCeiling;

                    if (!isInVoid)
                        yield return SectorVerticalPartExtensions.GetExtraCeilingSplit(i);
                }
            }
        }

        public bool IsCeilingSplitInVoid(SectorVerticalPart vertical, int x, int z, out int highestNeighborCeiling)
        {
            Room targetRoom = this;
            Sector targetSector = Sectors[x, z];

            if (targetSector.WallPortal != null)
            {
                RoomSectorPair pair = GetSectorTryThroughPortal(x, z);

                if (pair.Room != this)
                {
                    targetRoom = pair.Room;
                    targetSector = pair.Sector;
                }
            }

            int relativeHighestNeighborCeiling = GetHighestNeighborCeiling(x, z);
            highestNeighborCeiling = relativeHighestNeighborCeiling - targetRoom.Position.Y;

            if (vertical is SectorVerticalPart.WS)
            {
                bool isInVoid = targetSector.Ceiling.Min + targetRoom.Position.Y >= relativeHighestNeighborCeiling;
                return isInVoid;
            }
            else
            {
                SectorSplit split = targetSector.ExtraCeilingSplits.ElementAtOrDefault(vertical.GetExtraSplitIndex());

                if (split is null)
                    return true;

                bool isInVoid = split.Min + targetRoom.Position.Y >= relativeHighestNeighborCeiling;
                return isInVoid;
            }
        }

        public void ChangeSectorHeight(int x, int z, SectorVerticalPart vertical, SectorEdge edge, int increment)
        {
            if (increment == 0)
                return;

            Sector sector = Sectors[x, z];

            // Check if split doesn't exist and is a valid next split
            if (vertical.IsExtraSplit() && !sector.SplitExists(vertical) && sector.IsValidNextSplit(vertical))
            {
                // Create the new split if applicable

                if (vertical.IsExtraFloorSplit())
                {
                    if (sector.ExtraFloorSplits.Count == 0)
                    {
                        if (IsFloorSplitInVoid(SectorVerticalPart.QA, x, z, out int lowestNeightborFloor))
                            return;

                        sector.ExtraFloorSplits.Add(new SectorSplit(lowestNeightborFloor));
                    }
                    else
                    {
                        SectorVerticalPart lastVerical = SectorVerticalPartExtensions.GetExtraFloorSplit(sector.ExtraFloorSplits.Count - 1);

                        if (IsFloorSplitInVoid(lastVerical, x, z, out int lowestNeightborFloor))
                            return;

                        sector.ExtraFloorSplits.Add(new SectorSplit(lowestNeightborFloor));
                    }
                }
                else if (vertical.IsExtraCeilingSplit())
                {
                    if (sector.ExtraCeilingSplits.Count == 0)
                    {
                        if (IsCeilingSplitInVoid(SectorVerticalPart.WS, x, z, out int highestNeighborCeiling))
                            return;

                        sector.ExtraCeilingSplits.Add(new SectorSplit(highestNeighborCeiling));
                    }
                    else
                    {
                        SectorVerticalPart lastVerical = SectorVerticalPartExtensions.GetExtraCeilingSplit(sector.ExtraCeilingSplits.Count - 1);

                        if (IsCeilingSplitInVoid(lastVerical, x, z, out int highestNeighborCeiling))
                            return;

                        sector.ExtraCeilingSplits.Add(new SectorSplit(highestNeighborCeiling));
                    }
                }
            }

            sector.SetHeight(vertical, edge, sector.GetHeight(vertical, edge) + increment);
        }

        public void RaiseSector(int x, int z, SectorVerticalPart vertical, int increment, bool diagonalStep = false)
        {
            Sector sector = Sectors[x, z];
            DiagonalSplit split = vertical.IsOnFloor() ? sector.Floor.DiagonalSplit : sector.Ceiling.DiagonalSplit;

            if (diagonalStep)
            {
                switch (split)
                {
                    case DiagonalSplit.XpZn:
                        ChangeSectorHeight(x, z, vertical, SectorEdge.XnZp, increment);
                        break;
                    case DiagonalSplit.XnZn:
                        ChangeSectorHeight(x, z, vertical, SectorEdge.XpZp, increment);
                        break;
                    case DiagonalSplit.XnZp:
                        ChangeSectorHeight(x, z, vertical, SectorEdge.XpZn, increment);
                        break;
                    case DiagonalSplit.XpZp:
                        ChangeSectorHeight(x, z, vertical, SectorEdge.XnZn, increment);
                        break;
                }
            }
            else
            {
                for (SectorEdge edge = 0; edge < SectorEdge.Count; edge++)
                {
                    if ((edge == SectorEdge.XnZp && split == DiagonalSplit.XpZn) ||
                        (edge == SectorEdge.XnZn && split == DiagonalSplit.XpZp) ||
                        (edge == SectorEdge.XpZn && split == DiagonalSplit.XnZp) ||
                        (edge == SectorEdge.XpZp && split == DiagonalSplit.XnZn))
                        continue;

                    ChangeSectorHeight(x, z, vertical, edge, increment);
                }
            }
        }

        public void RaiseSectorStepWise(int x, int z, SectorVerticalPart vertical, bool diagonalStep, int increment, bool autoSwitch = false)
        {
            Sector sector = Sectors[x, z];
            DiagonalSplit split = vertical.IsOnFloor() ? sector.Floor.DiagonalSplit : sector.Ceiling.DiagonalSplit;

            if (split != DiagonalSplit.None)
            {
                bool stepIsLimited = increment != 0 && increment > 0 == (vertical.IsOnCeiling() ^ diagonalStep);

                if ((split == DiagonalSplit.XpZn && sector.GetHeight(vertical, SectorEdge.XnZp) == sector.GetHeight(vertical, SectorEdge.XpZp) && stepIsLimited) ||
                    (split == DiagonalSplit.XnZn && sector.GetHeight(vertical, SectorEdge.XpZp) == sector.GetHeight(vertical, SectorEdge.XpZn) && stepIsLimited) ||
                    (split == DiagonalSplit.XnZp && sector.GetHeight(vertical, SectorEdge.XpZn) == sector.GetHeight(vertical, SectorEdge.XnZn) && stepIsLimited) ||
                    (split == DiagonalSplit.XpZp && sector.GetHeight(vertical, SectorEdge.XnZn) == sector.GetHeight(vertical, SectorEdge.XnZp) && stepIsLimited))
                {
                    if (sector.IsAnyWall && autoSwitch)
                        RaiseSector(x, z, vertical, increment, !diagonalStep);
                    else
                    {
                        if (autoSwitch)
                        {
                            sector.Transform(new RectTransformation { QuadrantRotation = 2 }, vertical.IsOnFloor());
                            RaiseSector(x, z, vertical, increment, !diagonalStep);
                        }

                        return;
                    }
                }
            }

            RaiseSector(x, z, vertical, increment, diagonalStep);
        }

        bool IEquatable<ITriggerParameter>.Equals(ITriggerParameter other) => this == other;

        public HashSet<Room> GetAdjoiningRoomsFromArea(RectangleInt2 area)
        {
            var adjoiningRooms = new HashSet<Room>();

            for (int x = area.X0; x <= area.X1; x++)
            {
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Sector sector = GetSectorTry(x, z);

                    if (sector is null)
                        continue;

                    if (sector.WallPortal is not null)
                        adjoiningRooms.Add(sector.WallPortal.AdjoiningRoom);
                }
            }

            return adjoiningRooms;
        }
    }
}
