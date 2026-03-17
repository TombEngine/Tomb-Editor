using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using TombLib;
using TombLib.LevelData;
using TombLib.LevelData.SectorStructs;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor
{
    public abstract class EditorUndoRedoInstance : UndoRedoInstance
    {
        public EditorUndoManager Parent;

        public Room Room { get; internal set; }
        protected EditorUndoRedoInstance(EditorUndoManager parent, Room room) { Parent = parent; Room = room; }
    }

    public class AddSectorBasedObjectUndoInstance : EditorUndoRedoInstance
    {
        private SectorBasedObjectInstance SectorObject;

        public AddSectorBasedObjectUndoInstance(EditorUndoManager parent, SectorBasedObjectInstance obj) : base(parent, obj.Room)
        {
            SectorObject = obj;
            Valid = () => SectorObject != null && SectorObject.Room != null && Room != null && SectorObject.Room == Room && Room.ExistsInLevel;
            UndoAction = () => EditorActions.DeleteObjectWithoutUpdate(SectorObject);
        }
    }

    public class AddRoomUndoInstance : EditorUndoRedoInstance
    {
        public AddRoomUndoInstance(EditorUndoManager parent, Room room) : base(parent, room)
        {
            Valid = () => Room != null && Room.ExistsInLevel && Room.AnyObjects.Count() == 0;
            UndoAction = () => EditorActions.DeleteRooms(new[] { Room });
        }
    }

    public class AddAdjoiningRoomUndoInstance : EditorUndoRedoInstance
    {
        private Room ParentRoom;

        public AddAdjoiningRoomUndoInstance(EditorUndoManager parent, Room room) : base(parent, room)
        {
            ParentRoom = room.Portals.Count() == 1 ? room.Portals.First().AdjoiningRoom : null;

            Valid = () => Room != null && ParentRoom != null &&
                         Room.ExistsInLevel && ParentRoom.ExistsInLevel &&
                         Room.Portals.Count(p => p.AdjoiningRoom == ParentRoom) == 1 && Room.AnyObjects.Count(obj => (obj is PortalInstance)) == 1 &&
                         Room.AnyObjects.Count(obj => !(obj is PortalInstance)) == 0;

            UndoAction = () =>
            {
                Parent.Editor.SelectedRoom = ParentRoom;
                EditorActions.DeleteRooms(new[] { Room });
            };
        }
    }

    public class MoveRoomsUndoInstance : EditorUndoRedoInstance
    {
        VectorInt3 Delta;
        List<Room> Rooms;
        Dictionary<Room, VectorInt2> Sizes = new Dictionary<Room, VectorInt2>();

        public MoveRoomsUndoInstance(EditorUndoManager parent, List<Room> rooms, VectorInt3 delta) : base(parent, null)
        {
            Delta = -delta;
            Rooms = rooms;
            Rooms.ForEach(room => Sizes.Add(room, room.SectorSize));

            Valid = () => Rooms != null && Rooms.All(room => room != null && room.ExistsInLevel && !room.Properties.Locked) &&
                                           Rooms.All(room => !Parent.Editor.Level.GetConnectedRooms(room).Except(Rooms).Any()) &&
                                           Rooms.All(room => Sizes.ContainsKey(room) && room.SectorSize == Sizes[room]);
            UndoAction = () => EditorActions.MoveRooms(Delta, Rooms, true);
            RedoInstance = () => new MoveRoomsUndoInstance(Parent, Rooms, Delta);
        }
    }

    public class AddRemoveObjectUndoInstance : EditorUndoRedoInstance
    {
        private PositionBasedObjectInstance UndoObject;
        private bool Created;

        public AddRemoveObjectUndoInstance(EditorUndoManager parent, PositionBasedObjectInstance obj, bool created, Room room = null) : base(parent, room ?? obj.Room)
        {
            Created = created;
            UndoObject = obj;

            Valid = () =>
            {
                var result = UndoObject != null && ((Created && UndoObject.Room != null) ||
                (!Created && Room.ExistsInLevel && Room.LocalArea.Width > UndoObject.SectorPosition.X && Room.LocalArea.Height > UndoObject.SectorPosition.Y));

                if (!result) return result;

                // Special case for imported geometry: in case user deletes model from geometry list, we should prevent it from reappearing on the map.

                if (UndoObject is ImportedGeometryInstance)
                {
                    var geo = UndoObject as ImportedGeometryInstance;
                    if (geo.Model != null && !Room.Level.Settings.ImportedGeometries.Contains(geo.Model))
                        return false;
                }
                return result;
            };            

            UndoAction = () =>
            {
                if (Created)
                {
                    if (Parent.Editor.SelectedObject is ObjectGroup &&
                        ((ObjectGroup)Parent.Editor.SelectedObject).Contains(UndoObject))
                        Parent.Editor.SelectedObject = null;

                    EditorActions.DeleteObjectWithoutUpdate(UndoObject);
                }
                else
                {
                    var backupPos = obj.Position; // Preserve original position and reassign it after placement
                    EditorActions.PlaceObjectWithoutUpdate(Room, obj.SectorPosition, UndoObject);
                    EditorActions.MoveObject(UndoObject, backupPos);
                }
            };

            RedoInstance = () =>
            {
                var result = new AddRemoveObjectUndoInstance(Parent, UndoObject, !Created);
                result.Room = Room; // Relink parent room
                return result;
            };
        }
    }

    public class TransformObjectUndoInstance : EditorUndoRedoInstance
    {
        private PositionBasedObjectInstance UndoObject;
        private Vector3 Position;

        // Optional fields for various interface types

        private Vector3? Size = null;
        private float? Scale = null;
        private float? RotationY = null;
        private float? RotationX = null;
        private float? Roll = null;

        public TransformObjectUndoInstance(EditorUndoManager parent, PositionBasedObjectInstance obj) : base(parent, obj.Room)
        {
            UndoObject = obj;
            Position = obj.Position;

            if (obj is ISizeable) Size = ((ISizeable)obj).Size;
            if (obj is IScaleable) Scale = ((IScaleable)obj).Scale;
            if (obj is IRotateableY) RotationY = ((IRotateableY)obj).RotationY;
            if (obj is IRotateableYX) RotationX = ((IRotateableYX)obj).RotationX;
            if (obj is IRotateableYXRoll) Roll = ((IRotateableYXRoll)obj).Roll;

            Valid = () => UndoObject != null && UndoObject.Room != null && Room.ExistsInLevel;

            UndoAction = () =>
            {
                bool roomChanged = false;

                if (UndoObject.Room != Room)
                {
                    var oldRoom = UndoObject.Room;
                    oldRoom.RemoveObject(Parent.Editor.Level, UndoObject);
                    Parent.Editor.ObjectChange(UndoObject, ObjectChangeType.Remove, oldRoom);

                    Room.AddObject(Parent.Editor.Level, UndoObject);
                    Parent.Editor.ObjectChange(UndoObject, ObjectChangeType.Add);

                    roomChanged = true;
                }

                UndoObject.Position = Position;
                if (UndoObject is ISizeable && Size.HasValue) ((ISizeable)obj).Size = Size.Value;
                if (UndoObject is IScaleable && Scale.HasValue) ((IScaleable)obj).Scale = Scale.Value;
                if (UndoObject is IRotateableY && RotationY.HasValue) ((IRotateableY)obj).RotationY = RotationY.Value;
                if (UndoObject is IRotateableYX && RotationX.HasValue) ((IRotateableYX)obj).RotationX = RotationX.Value;
                if (UndoObject is IRotateableYXRoll && Roll.HasValue) ((IRotateableYXRoll)obj).Roll = Roll.Value;

                // Rebuild lighting!
                if (UndoObject is LightInstance)
                {
                    if (Parent.Editor.ShouldRelight)
                        Room.RebuildLighting(Parent.Editor.Configuration.Rendering3D_HighQualityLightPreview);
                    else
                        Room.PendingRelight = true;
                }

                // Move origin of object group, if it contains object
                if (Parent.Editor.SelectedObject is ObjectGroup)
                {
                    var group = ((ObjectGroup)Parent.Editor.SelectedObject);
                    if (group.RootObject == UndoObject)
                        group.SetOrigin(group.RootObject.Position);
                }

                if (!roomChanged)
                    Parent.Editor.ObjectChange(UndoObject, ObjectChangeType.Change);
            };
            RedoInstance = () => new TransformObjectUndoInstance(Parent, UndoObject);
        }
    }

    public class ChangeObjectPropertyUndoInstance : EditorUndoRedoInstance
    {
        private PositionBasedObjectInstance UndoObject;
        private object Base;
        private List<object> Properties;

        public ChangeObjectPropertyUndoInstance(EditorUndoManager parent, PositionBasedObjectInstance obj) : base(parent, obj.Room)
        {
            UndoObject = obj;

            if (UndoObject is MoveableInstance)
            {
                var uo = (MoveableInstance)UndoObject;
                Base = uo.WadObjectId;
                Properties = new List<object> { uo.Ocb, uo.Invisible, uo.ClearBody, uo.CodeBits, uo.Color };
            }
            else if (UndoObject is StaticInstance)
            {
                var uo = (StaticInstance)UndoObject;
                Base = uo.WadObjectId;
                Properties = new List<object> { uo.Ocb, uo.Color };
            }
            else if (UndoObject is ImportedGeometryInstance)
            {
                var uo = (ImportedGeometryInstance)UndoObject;
                Base = uo.Model;
                Properties = new List<object> { uo.Scale, uo.Color };
            }
            else if (UndoObject is SpriteInstance)
            {
                var uo = (SpriteInstance)UndoObject;
                Properties = new List<object> { uo.Sequence, uo.Frame };
            }
            else if (UndoObject is LightInstance)
                Properties = new List<object> { ((LightInstance)UndoObject).Color };
            else if (UndoObject is SinkInstance)
                Properties = new List<object> { ((SinkInstance)UndoObject).Strength };
            else if (UndoObject is SoundSourceInstance)
                Properties = new List<object> { ((SoundSourceInstance)UndoObject).SoundId };

            Valid = () => UndoObject != null && UndoObject.Room != null && Room.ExistsInLevel;

            UndoAction = () =>
            {
                if (UndoObject is MoveableInstance)
                {
                    var uo = ((MoveableInstance)UndoObject);
                    uo.WadObjectId = (WadMoveableId)Base;
                    uo.Ocb = (short)Properties[0];
                    uo.Invisible = (bool)Properties[1];
                    uo.ClearBody = (bool)Properties[2];
                    uo.CodeBits = (byte)Properties[3];
                    uo.Color = (Vector3)Properties[4];
                }
                else if (UndoObject is StaticInstance)
                {
                    var uo = ((StaticInstance)UndoObject);
                    uo.WadObjectId = (WadStaticId)Base;
                    uo.Ocb = (short)Properties[0];
                    uo.Color = (Vector3)Properties[1];
                }
                else if (UndoObject is ImportedGeometryInstance)
                {
                    var uo = ((ImportedGeometryInstance)UndoObject);
                    uo.Model = (ImportedGeometry)Base;
                    uo.Scale = (float)Properties[0];
                    uo.Color = (Vector3)Properties[1];
                }
                else if (UndoObject is SpriteInstance)
                {
                    var uo = ((SpriteInstance)UndoObject);
                    uo.Sequence = (int)Properties[0];
                    uo.Frame    = (int)Properties[1];
                }
                else if (UndoObject is LightInstance)
                {
                    ((LightInstance)UndoObject).Color = (Vector3)Properties[0];
                    UndoObject.Room.RebuildLighting(parent.Editor.Configuration.Rendering3D_HighQualityLightPreview);
                }
                else if (UndoObject is SinkInstance)
                    ((SinkInstance)UndoObject).Strength = (short)Properties[0];
                else if (UndoObject is SoundSourceInstance)
                    ((SoundSourceInstance)UndoObject).SoundId = (short)Properties[0];

                parent.Editor.ObjectChange(UndoObject, ObjectChangeType.Change);
            };

            RedoInstance = () => new ChangeObjectPropertyUndoInstance(Parent, UndoObject);
        }
    }

    public class AddRemoveGhostBlockUndoInstance : EditorUndoRedoInstance
    {
        private GhostBlockInstance UndoObject;
        private bool Created;

        public AddRemoveGhostBlockUndoInstance(EditorUndoManager parent, GhostBlockInstance obj, bool created) : base(parent, obj.Room)
        {
            Created = created;
            UndoObject = obj;

            Valid = () => UndoObject != null && ((Created && UndoObject.Room != null) ||
                        (!Created && Room.ExistsInLevel)) && !Room.CoordinateInvalid(UndoObject.SectorPosition);

            UndoAction = () =>
            {
                if (Created)
                    EditorActions.DeleteObjectWithoutUpdate(UndoObject);
                else
                {
                    var backupPos = obj.SectorPosition; // Preserve original position and reassign it after placement
                    EditorActions.PlaceGhostBlockWithoutUpdate(Room, obj.SectorPosition, UndoObject);
                }
            };

            RedoInstance = () =>
            {
                var result = new AddRemoveGhostBlockUndoInstance(Parent, UndoObject, !Created);
                result.Room = Room; // Relink parent room
                return result;
            };
        }
    }

    public class TransformGhostBlockUndoInstance : EditorUndoRedoInstance
    {
        private GhostBlockInstance UndoObject;
        private SectorSurface Floor;
        private SectorSurface Ceiling;

        public TransformGhostBlockUndoInstance(EditorUndoManager parent, GhostBlockInstance obj) : base(parent, obj.Room)
        {
            UndoObject = obj;
            Floor = obj.Floor;
            Ceiling = obj.Ceiling;

            Valid = () => UndoObject != null && UndoObject.Room != null && Room.ExistsInLevel &&
                          !Room.CoordinateInvalid(UndoObject.SectorPosition);

            UndoAction = () =>
            {
                UndoObject.Floor = Floor;
                UndoObject.Ceiling = Ceiling;
                Parent.Editor.ObjectChange(UndoObject, ObjectChangeType.Change);
            };
            RedoInstance = () => new TransformGhostBlockUndoInstance(Parent, UndoObject);
        }
    }

    public class RoomPropertyUndoInstance : EditorUndoRedoInstance
    {
        private RoomProperties Properties;

        public RoomPropertyUndoInstance(EditorUndoManager parent, Room room) : base(parent, room)
        {
            Properties = room.Properties.Clone();

            Valid = () => Room != null && Room.ExistsInLevel;

            UndoAction = () =>
            {
                bool rebuildLighting = Room.Properties.AmbientLight != Properties.AmbientLight;
                Room.Properties = Properties;
                if (rebuildLighting)
                    Room.RebuildLighting(parent.Editor.Configuration.Rendering3D_HighQualityLightPreview);
                Parent.Editor.RoomPropertiesChange(Room);
            };
            RedoInstance = () => new RoomPropertyUndoInstance(Parent, Room);
        }
    }

    public class GeometryUndoInstance : EditorUndoRedoInstance
    {
        private RectangleInt2 Area;
        private Sector[,] Sectors;

        public GeometryUndoInstance(EditorUndoManager parent, Room room) : base(parent, room)
        {
            Area.Start = VectorInt2.Zero;
            Area.End = new VectorInt2(room.NumXSectors, room.NumZSectors);

            Sectors = new Sector[Area.Size.X + 1, Area.Size.Y + 1];

            for (int x = Area.X0, i = 0; x < Area.X1; x++, i++)
                for (int z = Area.Y0, j = 0; z < Area.Y1; z++, j++)
                    Sectors[i, j] = Room.Sectors[x, z].Clone();

            Valid = () => (Room != null && Room.ExistsInLevel && Room.SectorSize == Area.Size);

            UndoAction = () =>
            {
                Parent.Editor.RaiseEvent(new Editor.SuspendRenderingEvent());

                for (int x = Area.X0, i = 0; x < Area.X1; x++, i++)
                    for (int z = Area.Y0, j = 0; z < Area.Y1; z++, j++)
                        Room.Sectors[x, z].ReplaceGeometry(Parent.Editor.Level, Sectors[i, j]);

                Room.Rebuild(parent.Editor.ShouldRelight, parent.Editor.Configuration.Rendering3D_HighQualityLightPreview);
                Parent.Editor.RoomGeometryChange(Room);
                Parent.Editor.RoomSectorPropertiesChange(Room);
                var relevantRooms = room.Portals.Select(p => p.AdjoiningRoom).Distinct();
                Parallel.ForEach(relevantRooms, r => r.Rebuild(parent.Editor.ShouldRelight, parent.Editor.Configuration.Rendering3D_HighQualityLightPreview));


                foreach (Room relevantRoom in relevantRooms)
                    Parent.Editor.RoomGeometryChange(relevantRoom);

                Parent.Editor.RaiseEvent(new Editor.ResumeRenderingEvent());
            };
            RedoInstance = () => new GeometryUndoInstance(Parent, Room);
        }
    }

    public class EditorUndoManager : UndoManager
    {
        public Editor Editor;

        public EditorUndoManager(Editor editor, int undoDepth) : base(undoDepth)
        {
            Editor = editor;

            UndoStackChanged += (s, e) => Editor.UndoStackChanged();
            MessageSent += (s, e) => Editor.SendMessage(e.Message, TombLib.Forms.PopupType.Warning);
        }

        public void PushRoomCreated(Room room) => Push(new AddRoomUndoInstance(this, room));
        public void PushAdjoiningRoomCreated(Room room) => Push(new AddAdjoiningRoomUndoInstance(this, room));
        public void PushRoomsMoved(List<Room> rooms, VectorInt3 delta) { if (delta != VectorInt3.Zero) Push(new MoveRoomsUndoInstance(this, rooms, delta)); }
        public void PushSectorObjectCreated(SectorBasedObjectInstance obj) => Push(new AddSectorBasedObjectUndoInstance(this, obj));
        public void PushSectorObjectCreated(List<SectorBasedObjectInstance> objs) => Push(objs.Select(obj => (new AddSectorBasedObjectUndoInstance(this, obj)) as UndoRedoInstance).ToList());
        public void PushGeometryChanged(Room room) => Push(new GeometryUndoInstance(this, room));
        public void PushGeometryChanged(IEnumerable<Room> rooms) => Push(rooms.Select(room => (new GeometryUndoInstance(this, room)) as UndoRedoInstance).ToList());
        public void PushGhostBlockCreated(GhostBlockInstance obj) => Push(new AddRemoveGhostBlockUndoInstance(this, obj, true));
        public void PushGhostBlockCreated(List<GhostBlockInstance> objs) => Push(objs.Select(obj => (new AddRemoveGhostBlockUndoInstance(this, obj, true)) as UndoRedoInstance).ToList());
        public void PushGhostBlockDeleted(GhostBlockInstance obj) => Push(new AddRemoveGhostBlockUndoInstance(this, obj, false));
        public void PushGhostBlockTransformed(GhostBlockInstance obj) => Push(new TransformGhostBlockUndoInstance(this, obj));

        public void PushObjectCreated(PositionBasedObjectInstance obj) => Push(new AddRemoveObjectUndoInstance(this, obj, true));
        public void PushObjectDeleted(PositionBasedObjectInstance obj) => Push(new AddRemoveObjectUndoInstance(this, obj, false));

        public void PushObjectPropertyChanged(PositionBasedObjectInstance obj)
        {
            if (obj is ObjectGroup)
                Push(((ObjectGroup)obj).Select(o => new ChangeObjectPropertyUndoInstance(this, o)).Cast<UndoRedoInstance>().ToList());
            else
                Push(new ChangeObjectPropertyUndoInstance(this, obj));
        }

        public void PushObjectTransformed(PositionBasedObjectInstance obj)
        {
            if (obj is ObjectGroup)
                Push(((ObjectGroup)obj).Select(o => new TransformObjectUndoInstance(this, o)).Cast<UndoRedoInstance>().ToList());
            else
                Push(new TransformObjectUndoInstance(this, obj));
        }
    }
}
