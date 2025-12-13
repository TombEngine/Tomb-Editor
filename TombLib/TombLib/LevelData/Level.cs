using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TombLib.LevelData.SectorEnums;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData
{
    public class Level
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const float SectorSizeUnit = 1024.0f;
        public const float HalfSectorSizeUnit = SectorSizeUnit / 2.0f;
        public const int FullClickHeight = 256;

        public const short MaxNumberOfRooms = 1024;

        public Room[] Rooms { get; } = new Room[MaxNumberOfRooms]; // Rooms in level
        public LevelSettings Settings { get; private set; } = new LevelSettings();
        public ScriptIdTable<IHasScriptID> GlobalScriptingIdsTable { get; } = new ScriptIdTable<IHasScriptID>();

        public List<Room> ExistingRooms => Rooms.Where(r => r != null).ToList();
        public List<Room> VerticallyConnectedRooms => ExistingRooms.Where(r => r.Portals.Any(p => p.Direction == PortalDirection.Floor || p.Direction == PortalDirection.Ceiling)).ToList();

        public static Level CreateSimpleLevel(TRVersion.Game version = TRVersion.Game.TRNG)
        {
            logger.Info("Creating new empty level");

            Level result = new Level();
            result.Settings.GameVersion = version;
            result.Settings.ConvertLevelExtension();

            if (result.Rooms[0] == null)
                result.Rooms[0] = new Room(result, Room.DefaultRoomDimensions, Room.DefaultRoomDimensions, result.Settings.DefaultAmbientLight, "Room 0");
            return result;
        }

        public HashSet<Room> GetConnectedRooms(IEnumerable<Room> startingRooms)
        {
            var result = new HashSet<Room>();
            foreach (Room startingRoom in startingRooms)
            {
                GetConnectedRoomsRecursively(result, startingRoom);
                GetConnectedRoomsRecursively(result, startingRoom.AlternateOpposite);
            }
            return result;
        }

        public HashSet<Room> GetConnectedRooms(Room startingRoom)
        {
            var result = new HashSet<Room>();
            GetConnectedRoomsRecursively(result, startingRoom);
            GetConnectedRoomsRecursively(result, startingRoom.AlternateOpposite);
            return result;
        }

        public int GetLowestRoomGroupPoint(HashSet<Room> roomGroup)
        {
            int lowestPoint = int.MaxValue;

            foreach (Room room in roomGroup)
            {
                int current = Clicks.FromWorld(room.Position.Y) + Clicks.FromWorld(room.GetLowestCorner());

                if (lowestPoint > current)
                    lowestPoint = current;
            }

            return lowestPoint;
        }

        public int GetHighestRoomGroupPoint(HashSet<Room> roomGroup)
        {
            int highestPoint = int.MinValue;

            foreach (Room room in roomGroup)
            {
                int current = Clicks.FromWorld(room.Position.Y) + Clicks.FromWorld(room.GetHighestCorner());

                if (highestPoint < current)
                    highestPoint = current;
            }

            return highestPoint;
        }

        public Room GetNearbyRoomBelow(HashSet<Room> roomGroup, HashSet<Room> roomsToCheck, int height, int tolerance)
        {
            var roomsOutsideGroup = roomsToCheck.Where(r => !roomGroup.Contains(r) && r != null).ToList();
            if (roomsOutsideGroup == null || roomsOutsideGroup.Count() == 0)
                return null;
            else
                return roomsOutsideGroup.FirstOrDefault(r
                    => height < Clicks.FromWorld(r.Position.Y) + Clicks.FromWorld(r.GetHighestCorner()) + tolerance
                    && height >= Clicks.FromWorld(r.Position.Y) + Clicks.FromWorld(r.GetHighestCorner()) - tolerance);
        }

        public Room GetNearbyRoomAbove(HashSet<Room> roomGroup, HashSet<Room> roomsToCheck, int height, int tolerance)
        {
            var roomsOutsideGroup = roomsToCheck.Where(r => !roomGroup.Contains(r) && r != null).ToList();
            if (roomsOutsideGroup == null || roomsOutsideGroup.Count() == 0)
                return null;
            else
                return roomsOutsideGroup.FirstOrDefault(r
                    => height < Clicks.FromWorld(r.Position.Y) + Clicks.FromWorld(r.GetLowestCorner()) + tolerance
                    && height >= Clicks.FromWorld(r.Position.Y) + Clicks.FromWorld(r.GetLowestCorner()) - tolerance);
        }

        public List<TriggerInstance> GetAllTriggersPointingToObject(ObjectInstance instance)
        {
            var triggers = new List<TriggerInstance>();
            foreach (var room in ExistingRooms)
                foreach (var trigger in room.Triggers)
                    if (trigger.IsPointingTo(instance))
                        triggers.Add(trigger);
            return triggers;
        }

        private void GetConnectedRoomsRecursively(HashSet<Room> result, Room startingRoom)
        {
            if (startingRoom == null || result.Contains(startingRoom))
                return;

            result.Add(startingRoom);
            foreach (var portal in startingRoom.Portals)
            {
                GetConnectedRoomsRecursively(result, portal.AdjoiningRoom);
                GetConnectedRoomsRecursively(result, portal.AdjoiningRoom?.AlternateOpposite);
            }
        }

        public IEnumerable<Room> GetVerticallyAscendingRoomList(Predicate<Room> checkRoom)
        {
            var roomList = new List<KeyValuePair<float, Room>>();
            foreach (Room room in Rooms)
                if (room != null && checkRoom(room))
                    roomList.Add(new KeyValuePair<float, Room>(room.Position.Y + room.GetHighestCorner(), room));
            var result = roomList
                .OrderBy(roomPair => roomPair.Key) // don't use the Sort member function because it is unstable!
                .ThenBy(roomPair => roomPair.Value.AlternateBaseRoom == null)
                .Select(roomKey => roomKey.Value).ToList();
            return result;
        }

        public int GetFreeRoomIndex()
        {
            // Search the first free room
            for (int i = 0; i < Rooms.Length; i++)
                if (Rooms[i] == null)
                    return i;
            throw new Exception("A maximum number of " + MaxNumberOfRooms + " rooms has been reached. Unable to add room.");
        }

        public int AssignRoomToFree(Room room)
        {
            int result = GetFreeRoomIndex();
            Rooms[result] = room;
            return result;
        }

        public List<Room> DeleteRoom(Room room)
        {
            for (int i = 0; i < Rooms.Length; ++i)
                if (Rooms[i] == room)
                {
                    var result = room.Delete(this);
                    Rooms[i] = null;
                    return result;
                }
            throw new ArgumentException("The room does not belong to the level from which it should be removed.");
        }

        public void DeleteRoomWithAlternate(Room room)
        {
            DeleteRoom(room);
            if (room.AlternateOpposite != null)
                DeleteRoom(room.AlternateOpposite);
        }

        public List<Room> DeleteTriggersForObject(ObjectInstance instance)
        {
            if (instance is ObjectGroup)
                return ((ObjectGroup)instance).SelectMany(DeleteTriggersForObject).Distinct().ToList();

            var result = new List<Room>();

            var isTriggerableObject = instance is MoveableInstance || instance is StaticInstance || instance is CameraInstance ||
                                      instance is FlybyCameraInstance || instance is SinkInstance || instance is SoundSourceInstance;

            if (isTriggerableObject)
            {
                var triggersToRemove = new List<TriggerInstance>();
                foreach (var r in Rooms)
                    if (r != null)
                        foreach (var trigger in r.Triggers)
                        {
                            if (trigger.Target == instance)
                                triggersToRemove.Add(trigger);

                            if (!result.Contains(r))
                                result.Add(r);
                        }

                foreach (var t in triggersToRemove)
                    t.Room.RemoveObject(this, t);
            }

            return result;
        }

        public void MergeFrom(Level otherLevel, bool unifyData, Action<LevelSettings> applyLevelSettings = null)
        {
            IReadOnlyList<ObjectInstance> oldObjects = ExistingRooms.SelectMany(room => room.AnyObjects).ToList();
            IReadOnlyList<Room> otherRooms = otherLevel.ExistingRooms;

            // Merge rooms
            for (int i = 0; i < otherRooms.Count; ++i)
                try
                {
                    AssignRoomToFree(otherRooms[i]);
                }
                catch
                { // If we fail, roll back the changes...
                    while (i > 0)
                        DeleteRoomWithAlternate(otherRooms[--i]);
                    throw;
                }

            // Merge dependencies like imported geometries, textures (and wads in the future?)
            LevelSettings newSettings = applyLevelSettings == null ? Settings : Settings.Clone();
            newSettings.Textures = Settings.Textures.ToList(); // Make sure the same references are used.
            newSettings.ImportedGeometries = Settings.ImportedGeometries.ToList();
            var copyInstance = new Room.CopyDependentLevelSettingsArgs(oldObjects, newSettings, otherLevel.Settings, unifyData);
            foreach (Room room in otherRooms)
                room.CopyDependentLevelSettings(copyInstance);
            applyLevelSettings?.Invoke(newSettings);
            GlobalScriptingIdsTable.MergeFrom(otherLevel.GlobalScriptingIdsTable, @object => @object.ScriptId = null);
            foreach (Room room in otherRooms)
                room.Level = this;
        }

        public IReadOnlyList<Room> TransformRooms(IEnumerable<Room> roomsToRotate, RectTransformation transformation, VectorInt2 center)
        {
            roomsToRotate = roomsToRotate.SelectMany(room => room.Versions).Distinct();
            Room[] oldRooms = roomsToRotate.ToArray();
            var worldCenter = new VectorInt3(center.X, 0, center.Y) * (int)SectorSizeUnit;

            // Copy rooms and sectors
            var newRooms = new Room[oldRooms.Length];
            for (int i = 0; i < oldRooms.Length; ++i)
            {
                var oldRoom = oldRooms[i];

                // Create room
                var newSize = transformation.QuadrantRotation % 2 == 0 ? oldRoom.SectorSize : new VectorInt2(oldRoom.NumZSectors, oldRoom.NumXSectors);
                var newRoom = oldRoom.Clone(this, obj => false); // This is a waste of computing power: All sectors are copied and immediately afterwards thrown away because the room needs to get resized.
                newRoom.Resize(this, new RectangleInt2(0, 0, newSize.X - 1, newSize.Y - 1));

                // Assign position
                var roomCenter = oldRoom.WorldPos + new VectorInt3(oldRoom.NumXSectors, 0, oldRoom.NumZSectors) * 512;
                roomCenter -= worldCenter;
                roomCenter = transformation.TransformVecInt3(roomCenter, oldRoom.NumXSectors, oldRoom.NumZSectors);
                roomCenter += worldCenter;
                newRoom.WorldPos = roomCenter - new VectorInt3(newSize.X, 0, newSize.Y) * 512;

                // Copy sectors
                for (int z = 0; z < oldRoom.NumZSectors; ++z)
                    for (int x = 0; x < oldRoom.NumXSectors; ++x)
                    {
                        VectorInt2 newSectorPosition = transformation.Transform(new VectorInt2(x, z), oldRoom.SectorSize);
                        newRoom.Sectors[newSectorPosition.X, newSectorPosition.Y] = oldRoom.Sectors[x, z].Clone();
                        newRoom.Sectors[newSectorPosition.X, newSectorPosition.Y].Transform(transformation, null,
                            oldFace => oldRoom.GetFaceShape(x, z, oldFace));
                    }
                newRooms[i] = newRoom;
            }

            // Move objects to new rooms
            for (int i = 0; i < oldRooms.Length; ++i)
            {
                Room oldRoom = oldRooms[i];
                Room newRoom = newRooms[i];

                // Copy objects
                List<ObjectInstance> objects = oldRoom.AnyObjects.ToList();
                foreach (ObjectInstance @object in objects)
                {
                    oldRoom.RemoveObjectAndSingularPortalAndKeepAlive(this, @object);
                    @object.Transform(transformation, oldRoom.SectorSize);
                    @object.TransformRoomReferences(room =>
                    {
                        int index = Array.IndexOf(oldRooms, room);
                        if (index == -1)
                            return room;
                        else
                            return newRooms[index];
                    });
                    newRoom.AddObjectAndSingularPortal(this, @object);
                }
            }

            // Assign new rooms
            for (int i = 0; i < oldRooms.Length; ++i)
            {
                if (oldRooms[i].AlternateRoom != null)
                    newRooms[i].AlternateRoom = newRooms[Array.IndexOf(oldRooms, oldRooms[i].AlternateRoom)];
                if (oldRooms[i].AlternateBaseRoom != null)
                    newRooms[i].AlternateBaseRoom = newRooms[Array.IndexOf(oldRooms, oldRooms[i].AlternateBaseRoom)];
            }
            for (int i = 0; i < oldRooms.Length; ++i)
            {
                int roomIndex = Array.IndexOf(Rooms, oldRooms[i]);
                Rooms[roomIndex] = newRooms[i];
            }

            return newRooms;
        }

        public IReadOnlyList<Room> TransformRooms(IEnumerable<Room> roomsToRotate, RectTransformation transformation)
        {
            IReadOnlyList<Room> rooms = roomsToRotate as IReadOnlyList<Room> ?? roomsToRotate.ToList();
            RectangleInt2 coveredArea = new RectangleInt2(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
            foreach (Room room in rooms)
                coveredArea = coveredArea.Union(room.WorldArea);
            return TransformRooms(rooms, transformation, new VectorInt2((coveredArea.X0 + coveredArea.X1) / 2, (coveredArea.Y0 + coveredArea.Y1) / 2));
        }

        public Room[] GetRearrangedRooms(IProgressReporter progressReporter = null)
        {
            var limit = TrCatalog.GetLimit(Settings.GameVersion, Limit.RoomSafeCount);

            // Don't sort anything if option is disabled or room count is less than limit
            if (!Settings.RearrangeVerticalRooms || ExistingRooms.Count < limit)
                return Rooms;

            // Count last index of vertically connected room and sort only if it goes beyond limit
            var lastIndex = ExistingRooms.IndexOf(r => r == ExistingRooms.LastOrDefault(rm => rm.Portals.Any(p => p.Direction == PortalDirection.Floor || p.Direction == PortalDirection.Ceiling)));
            if (lastIndex < limit)
            {
                // If no any room gets beyond the limit, don't sort anything
                return Rooms;
            }
            else
            {
                // Warn user if last vertically connected room index is beyond limit
                progressReporter?.ReportInfo("Level has " + ExistingRooms.Count + " rooms. Vertically connected rooms were sorted.");
            }

            // Warn user if portal count is beyond absolute limit
            if (VerticallyConnectedRooms.Count >= limit)
                progressReporter?.ReportWarn("Level contains " + VerticallyConnectedRooms.Count + " vertically connected rooms, while maximum is " + limit + ". Some vertical portals won't be added.");

            // Put all vertically connected rooms to the beginning of the list
            return Rooms.OrderBy(r => !VerticallyConnectedRooms.Contains(r)).ToArray();
        }

        public void RemoveTextures(Predicate<LevelTexture> askIfTextureToRemove)
        {
            // Clean up default texture first, in case removed texture set contains it
            if (Settings.DefaultTexture.Texture != null && askIfTextureToRemove(Settings.DefaultTexture.Texture as LevelTexture))
                Settings.DefaultTexture = TextureArea.None;

            // Remove all texture occurences from room faces
            Parallel.ForEach(ExistingRooms, room =>
            {
                foreach (Sector sector in room.Sectors)
                    foreach (SectorFace face in sector.GetFaceTextures().Keys)
                    {
                        TextureArea currentTextureArea = sector.GetFaceTexture(face);
                        LevelTexture currentTexture = currentTextureArea.Texture as LevelTexture;
                        if (currentTexture != null && askIfTextureToRemove(currentTexture))
                        {
                            sector.SetFaceTexture(face, TextureArea.None);
                        }
                    }
                room.BuildGeometry();
            });

            foreach (AnimatedTextureSet set in Settings.AnimatedTextureSets)
                set.Frames.RemoveAll(frame => askIfTextureToRemove((LevelTexture)frame.Texture));

            // Clean up empty texture sets as well
            Settings.AnimatedTextureSets.RemoveAll(set => set.Frames.Count == 0);
        }

        public void ApplyNewLevelSettings(LevelSettings newSettings, Action<ObjectInstance> objectChangedNotification)
        {
            LevelSettings oldSettings = Settings;
            Settings = newSettings;

            // Imported geometry
            {
                // Reuse old imported geometry objects to keep references up to date
                var oldLookup = new Dictionary<ImportedGeometry.UniqueIDType, ImportedGeometry>();
                foreach (ImportedGeometry oldImportedGeometry in oldSettings.ImportedGeometries)
                    oldLookup.Add(oldImportedGeometry.UniqueID, oldImportedGeometry);
                for (int i = 0; i < newSettings.ImportedGeometries.Count; ++i)
                {
                    ImportedGeometry newImportedGeometry = newSettings.ImportedGeometries[i];
                    ImportedGeometry oldImportedGeometry;
                    if (oldLookup.TryGetValue(newImportedGeometry.UniqueID, out oldImportedGeometry))
                    {
                        oldImportedGeometry.Assign(newImportedGeometry);
                        newSettings.ImportedGeometries[i] = oldImportedGeometry;
                        oldLookup.Remove(oldImportedGeometry.UniqueID); // The same object shouldn't be matched multiple times.
                    }
                }

                // Reset imported geometry objects if any objects are now missing
                if (oldLookup.Count != 0)
                    foreach (var room in ExistingRooms)
                        foreach (var instance in room.Objects.OfType<ImportedGeometryInstance>())
                            if (instance.Model != null && oldLookup.ContainsKey(instance.Model.UniqueID))
                            {
                                instance.Model = null;
                                objectChangedNotification(instance);
                            }
            }

            // Level texture
            {
                // Reuse old level texture objects to keep references up to date
                var oldLookup = new Dictionary<LevelTexture.UniqueIDType, LevelTexture>();
                foreach (LevelTexture oldLevelTexture in oldSettings.Textures)
                    oldLookup.Add(oldLevelTexture.UniqueID, oldLevelTexture);
                for (int i = 0; i < newSettings.Textures.Count; ++i)
                {
                    LevelTexture newLevelTexture = newSettings.Textures[i];
                    LevelTexture oldLevelTexture;
                    if (oldLookup.TryGetValue(newLevelTexture.UniqueID, out oldLevelTexture))
                    {
                        oldLevelTexture.Assign(newLevelTexture);
                        newSettings.Textures[i] = oldLevelTexture;
                        oldLookup.Remove(oldLevelTexture.UniqueID); // The same object shouldn't be matched multiple times.
                    }
                }

                // Reset level texture objects if any objects are now missing
                if (oldLookup.Count != 0)
                    RemoveTextures(texture => oldLookup.ContainsKey(texture.UniqueID));
            }
        }
        public void ApplyNewLevelSettings(LevelSettings newSettings) => ApplyNewLevelSettings(newSettings, s => { });

        public List<PositionBasedObjectInstance> GetAllObjects()
        {
            var result = new List<PositionBasedObjectInstance>();
            ExistingRooms.ForEach(r => result.AddRange(r.Objects));
            return result;
        }

        public bool IsNG => Settings?.GameVersion == TRVersion.Game.TRNG;

        public bool IsTombEngine => Settings?.GameVersion == TRVersion.Game.TombEngine;
    }
}
