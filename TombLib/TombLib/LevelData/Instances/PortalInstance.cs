using NAudio.Gui;
using System;
using System.Runtime.CompilerServices;
using TombLib.LevelData.SectorEnums;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public enum PortalDirection : byte
    {
        Floor, Ceiling, WallPositiveZ, WallNegativeZ, WallPositiveX, WallNegativeX
    }

    public enum PortalOpacity : byte
    {
        None,
        SolidFaces, // Called 'Opacity 1' in the old editor
        TraversableFaces // Called 'Opacity 2' in the old editor
    }

    public enum PortalEffectType : byte
    {
       None,
       ClassicMirror
    }

    public class PortalProperties
    {
        public bool ReflectLara { get; set; } = true;
        public bool ReflectStatics { get; set; } = true;
        public bool ReflectMoveables { get; set; } = true;
        public bool ReflectSprites { get; set; } = true;
        public bool ReflectLights { get; set; } = true;
    }

    public class PortalInstance : SectorBasedObjectInstance
    {
        private Room _adjoiningRoom;
        public Room AdjoiningRoom
        {
            get { return _adjoiningRoom; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("'AdjoiningRoom' must not be null");
                _adjoiningRoom = value.AlternateBaseRoom ?? value;
            }
        }
        private PortalDirection _direction;
        public PortalDirection Direction
        {
            get { return _direction; }
            set
            {
                if (Room != null)
                    throw new InvalidOperationException("Portal objects may not change in direction while they are assigned to a room.");
                _direction = value;
            }
        }

        public PortalOpacity Opacity { get; set; } = PortalOpacity.None;
        public PortalEffectType Effect { get; set; } = PortalEffectType.None;
        public PortalProperties Properties { get; set; } = new PortalProperties();

        public bool HasTexturedFaces => Opacity != PortalOpacity.None;
        public bool IsTraversable => Opacity != PortalOpacity.SolidFaces;

        public PortalInstance(RectangleInt2 area, PortalDirection direction, Room adjoiningRoom = null)
            : base(area)
        {
            AdjoiningRoom = adjoiningRoom;
            Direction = direction;
        }

        public override string ToString()
        {
            string text = "Portal ";

			if (Effect == PortalEffectType.ClassicMirror)
				text += "with mirror ";

            switch (Direction)
            {
                case PortalDirection.Ceiling:
                    text += "(On Ceiling) ";
                    break;
                case PortalDirection.Floor:
                    text += "(On Floor) ";
                    break;
                case PortalDirection.WallNegativeX:
                    text += "(Towards -X) ";
                    break;
                case PortalDirection.WallNegativeZ:
                    text += "(Towards -Z) ";
                    break;
                case PortalDirection.WallPositiveX:
                    text += "(Towards +X) ";
                    break;
                case PortalDirection.WallPositiveZ:
                    text += "(Towards +Z) ";
                    break;
            }
            text += "in room '" + (Room?.ToString() ?? "NULL") + "' ";
            text += "on sectors [" + Area.X0 + ", " + Area.Y0 + " to " + Area.X1 + ", " + Area.Y1 + "] ";
            text += "to Room " + AdjoiningRoom;
            return text;
        }

        public static PortalDirection GetDirection(Direction direction)
        {
            switch (direction)
            {
                case SectorEnums.Direction.NegativeX:
                    return PortalDirection.WallNegativeX;
                case SectorEnums.Direction.PositiveX:
                    return PortalDirection.WallPositiveX;
                case SectorEnums.Direction.NegativeZ:
                    return PortalDirection.WallNegativeZ;
                case SectorEnums.Direction.PositiveZ:
                    return PortalDirection.WallPositiveZ;
                default:
                    return PortalDirection.Floor;
            }
        }

        public static Direction? GetDirection(PortalDirection direction)
        {
            switch (direction)
            {
                case PortalDirection.WallNegativeX:
                    return SectorEnums.Direction.NegativeX;
                case PortalDirection.WallPositiveX:
                    return SectorEnums.Direction.PositiveX;
                case PortalDirection.WallNegativeZ:
                    return SectorEnums.Direction.NegativeZ;
                case PortalDirection.WallPositiveZ:
                    return SectorEnums.Direction.PositiveZ;
                default:
                    return null;
            }
        }

        public static PortalDirection GetOppositeDirection(PortalDirection direction)
        {
            switch (direction)
            {
                case PortalDirection.Ceiling:
                    return PortalDirection.Floor;
                case PortalDirection.Floor:
                    return PortalDirection.Ceiling;
                case PortalDirection.WallPositiveX:
                    return PortalDirection.WallNegativeX;
                case PortalDirection.WallNegativeX:
                    return PortalDirection.WallPositiveX;
                case PortalDirection.WallPositiveZ:
                    return PortalDirection.WallNegativeZ;
                case PortalDirection.WallNegativeZ:
                    return PortalDirection.WallPositiveZ;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static RectangleInt2 GetOppositePortalArea(PortalDirection direction, RectangleInt2 area)
        {
            switch (direction)
            {
                case PortalDirection.WallPositiveX:
                    return area + new VectorInt2(-1, 0);
                case PortalDirection.WallPositiveZ:
                    return area + new VectorInt2(0, -1);
                case PortalDirection.WallNegativeZ:
                    return area + new VectorInt2(0, 1);
                case PortalDirection.WallNegativeX:
                    return area + new VectorInt2(1, 0);
                default:
                    return area;
            }
        }

        // Usually this should return a portal, but be prepared for the situation that this returns null because in case of problems this might happen.
        public PortalInstance FindOppositePortal(Room room)
        {
            var adjoiningRoomArea = GetOppositePortalArea(Direction, Area) + (room.SectorPos - AdjoiningRoom.SectorPos);
            if (!new RectangleInt2(0, 0, AdjoiningRoom.NumXSectors, AdjoiningRoom.NumZSectors).Contains(adjoiningRoomArea.Start))
                return null;

            // Check sectors
            var sector = AdjoiningRoom.GetSectorTry(adjoiningRoomArea.Start);
            switch (Direction)
            {
                case PortalDirection.Floor:
                    return sector?.CeilingPortal; // A floor portal in this room is a ceiling portal in the adjoining room.
                case PortalDirection.Ceiling:
                    return sector?.FloorPortal; // A ceiling portal in this room is a floor portal in the adjoining room.
                default:
                    return sector?.WallPortal;
            }
        }

        public PortalInstance FindAlternatePortal(Room alternateRoom)
        {
            var sector = alternateRoom?.GetSectorTry(Area.X0, Area.Y0);
            switch (Direction)
            {
                case PortalDirection.Floor:
                    return sector?.FloorPortal; // A floor portal in this room is a ceiling portal in the adjoining room.
                case PortalDirection.Ceiling:
                    return sector?.CeilingPortal; // A ceiling portal in this room is a floor portal in the adjoining room.
                default:
                    return sector?.WallPortal;
            }
        }

        public bool IsValid(Room room)
        {
            switch (Direction)
            {
                case PortalDirection.Floor:
                    for (int z = Area.Y0; z <= Area.Y1; ++z)
                        for (int x = Area.X0; x <= Area.X1; ++x)
                            if (room.Sectors[x, z].FloorPortal != null)
                                return false;
                    break;

                case PortalDirection.Ceiling:
                    for (int z = Area.Y0; z <= Area.Y1; ++z)
                        for (int x = Area.X0; x <= Area.X1; ++x)
                            if (room.Sectors[x, z].CeilingPortal != null)
                                return false;
                    break;

                default:
                    for (int z = Area.Y0; z <= Area.Y1; ++z)
                        for (int x = Area.X0; x <= Area.X1; ++x)
                            if (room.Sectors[x, z].WallPortal != null)
                                return false;
                    break;
            }

            return true;
        }

        // For compatibility with legacy compiler
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public bool PositionOnPortal(VectorInt3 pos, bool detectInside, bool nonPlaneResult)
        {
            return PositionOnPortalFast(pos, detectInside, nonPlaneResult, Room.GetLowestCorner(), Room.GetHighestCorner());
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public bool PositionOnPortalFast(VectorInt3 pos, bool detectInside, bool nonPlaneResult, int lowestCorner, int highestCorner)
        {
            // Fast exit if we are on horizontal portals, check immediately the Y
            if (Direction <= PortalDirection.Ceiling)
            {
                int planeY = Direction == PortalDirection.Floor
                    ? -(lowestCorner + Room.WorldPos.Y)
                    : -(highestCorner + Room.WorldPos.Y);

                if (pos.Y != planeY)
                    return nonPlaneResult;
            }

            // Otherwise let's continue

            int compX0, compX1, compY0, compY1;
            switch (Direction)
            {
                case (PortalDirection.WallPositiveZ):
                    compX0 = Area.X0;
                    compX1 = Area.X1 + 1;
                    compY0 = Area.Y0;
                    compY1 = Area.Y1;
                    break;
                case (PortalDirection.WallNegativeZ):
                    compX0 = Area.X0;
                    compX1 = Area.X1 + 1;
                    compY0 = Area.Y0 + 1;
                    compY1 = Area.Y1 + 1;
                    break;
                case (PortalDirection.WallPositiveX):
                    compX0 = Area.X0;
                    compX1 = Area.X1;
                    compY0 = Area.Y0;
                    compY1 = Area.Y1 + 1;
                    break;
                case (PortalDirection.WallNegativeX):
                    compX0 = Area.X0 + 1;
                    compX1 = Area.X1 + 1;
                    compY0 = Area.Y0;
                    compY1 = Area.Y1 + 1;
                    break;
                default:
                    compX0 = Area.X0;
                    compX1 = Area.X1 + 1;
                    compY0 = Area.Y0;
                    compY1 = Area.Y1 + 1;
                    break;
            }

            // Shift is faster than multiplication, assuming that sector size is power of two
            int shift = System.Numerics.BitOperations.TrailingZeroCount((uint)Level.SectorSizeUnit);
            compX0 <<= shift;
            compX1 <<= shift;
			compY0 <<= shift;
			compY1 <<= shift;

			if (detectInside)
                return (((pos.X > compX0 && pos.X < compX1) || pos.X == compX0 && compX0 == compX1) &&
                ((pos.Z > compY0 && pos.Z < compY1) || pos.Z == compY0 && compY0 == compY1) &&
                !((pos.X == compX0 || pos.X == compX1) && (pos.Z == compY0 || pos.Z == compY1)));
            else
                return (((pos.X == compX0 || pos.X == compX1) && (pos.Z >= compY0 && pos.Z <= compY1)) ||
                ((pos.X >= compX0 && pos.X <= compX1) && (pos.Z == compY0 || pos.Z == compY1)));
        }

		public override void AddToRoom(Level level, Room room)
		{
			base.AddToRoom(level, room);
			if (!IsValid(room))
				throw new ApplicationException("Portal overlaps another");

			// Set sector information to this portal ...
            switch (Direction)
            {
                case PortalDirection.Floor:
                    for (int z = Area.Y0; z <= Area.Y1; ++z)
                        for (int x = Area.X0; x <= Area.X1; ++x)
                            room.Sectors[x, z].FloorPortal = this;
                    break;

                case PortalDirection.Ceiling:
                    for (int z = Area.Y0; z <= Area.Y1; ++z)
                        for (int x = Area.X0; x <= Area.X1; ++x)
                            room.Sectors[x, z].CeilingPortal = this;
                    break;

                default:
                    for (int z = Area.Y0; z <= Area.Y1; ++z)
                        for (int x = Area.X0; x <= Area.X1; ++x)
                            room.Sectors[x, z].WallPortal = this;
                    break;
            }
        }

        public override void RemoveFromRoom(Level level, Room room)
        {
            base.RemoveFromRoom(level, room);

            // Remove portal reference
            switch (Direction)
            {
                case PortalDirection.Floor:
                    for (int z = Area.Y0; z <= Area.Y1; ++z)
                        for (int x = Area.X0; x <= Area.X1; ++x)
                            room.Sectors[x, z].FloorPortal = null;
                    break;
                case PortalDirection.Ceiling:
                    for (int z = Area.Y0; z <= Area.Y1; ++z)
                        for (int x = Area.X0; x <= Area.X1; ++x)
                            room.Sectors[x, z].CeilingPortal = null;
                    break;
                default:
                    for (int z = Area.Y0; z <= Area.Y1; ++z)
                        for (int x = Area.X0; x <= Area.X1; ++x)
                            room.Sectors[x, z].WallPortal = null;
                    break;
            }
        }

        public override RectangleInt2 GetValidArea(RectangleInt2 newLocalRoomArea)
        {
            switch (Direction) // Special constraints for portals on walls
            {
                case PortalDirection.WallPositiveZ:
                    return newLocalRoomArea.Inflate(-1, 0);
                case PortalDirection.WallNegativeZ:
                    return newLocalRoomArea.Inflate(-1, 0);
                case PortalDirection.WallPositiveX:
                    return newLocalRoomArea.Inflate(0, -1);
                case PortalDirection.WallNegativeX:
                    return newLocalRoomArea.Inflate(0, -1);
                default:
                    return newLocalRoomArea.Inflate(-1, -1);
            }
        }

        public override void Transform(RectTransformation transformation, VectorInt2 oldRoomSize)
        {
            base.Transform(transformation, oldRoomSize);

            if (transformation.MirrorX)
                switch (Direction)
                {
                    case PortalDirection.WallNegativeX:
                        Direction = PortalDirection.WallPositiveX;
                        break;
                    case PortalDirection.WallPositiveX:
                        Direction = PortalDirection.WallNegativeX;
                        break;
                }

            for (int i = 0; i < transformation.QuadrantRotation; ++i)
                switch (Direction)
                {
                    case PortalDirection.WallPositiveX:
                        Direction = PortalDirection.WallPositiveZ;
                        break;
                    case PortalDirection.WallPositiveZ:
                        Direction = PortalDirection.WallNegativeX;
                        break;
                    case PortalDirection.WallNegativeX:
                        Direction = PortalDirection.WallNegativeZ;
                        break;
                    case PortalDirection.WallNegativeZ:
                        Direction = PortalDirection.WallPositiveX;
                        break;
                }
        }

        public override void TransformRoomReferences(Func<Room, Room> transformRoom)
        {
            base.TransformRoomReferences(transformRoom);
            AdjoiningRoom = transformRoom(AdjoiningRoom);
        }
    }
}
