using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData
{
    public enum RotationAxis
    {
        Y,
        X,
        Roll,
        None
    }

    public interface ISpatial
    {
        VectorInt2 SectorPosition { get; set; }
    }

    public interface IColorable
    {
        Vector3 Color { get; set; }
    }

    public interface IScaleable
    {
        float DefaultScale { get; }
        float Scale { get; set; }
    }

    public interface ISizeable
    {
        Vector3 DefaultSize { get; }
        Vector3 Size { get; set; }
    }

    public interface IRotateableY
    {
        float RotationY { get; set; }
    }

    public interface IRotateableYX : IRotateableY
    {
        float RotationX { get; set; }
    }

    public interface IRotateableYXRoll : IRotateableYX
    {
        float Roll { get; set; }
    }

    public interface IReplaceable
    {
        string PrimaryAttribDesc { get; }
        string SecondaryAttribDesc { get; }

        bool ReplaceableEquals(IReplaceable other, bool withProperties = false);
        bool Replace(IReplaceable other, bool withProperties);
    }

    public interface ICopyable { }

    public abstract class ObjectInstance : ICloneable, ITriggerParameter
    {
        public delegate void RemovedFromRoomDelegate(ObjectInstance instance);
        public event RemovedFromRoomDelegate DeletedEvent;
        public Room Room { get; protected set; }

        public virtual ObjectInstance Clone()
        {
            ObjectInstance result = (ObjectInstance)MemberwiseClone();
            result.Room = null;
            return result;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public virtual void AddToRoom(Level level, Room room)
        {
            if (room == null)
                throw new NullReferenceException("room was null");
            if (Room != null)
                throw new ArgumentException("An object can only be part of 1 room. The object '" + this + "' already belongs to room '" +
                    Room + "', it can not be added again to room '" + room + "'");
            Room = room;
        }

        public virtual void RemoveFromRoom(Level level, Room room)
        {
            if (room == null)
                throw new NullReferenceException("room was null");
            if (Room != room)
                throw new ArgumentException("An object can't be removed from a room it not part of. The object '" + this + "' belongs to room '" +
                    (Room == null ? "<none>" : Room.ToString()) + "', it can not be removed from room '" + room + "'");
            Room = null;
        }

        /// <summary>
        /// This methode should be invoked after the object was added to a room and the object will go out of scope.
        /// </summary>
        public virtual void Delete()
        {
            if (Room != null)
                throw new NullReferenceException("An object can't be deleted if it's still part of a room.");
            DeletedEvent?.Invoke(this);
        }

        public virtual void Transform(RectTransformation transformation, VectorInt2 oldRoomSize)
        {
            if (this is IRotateableY rotateableObject)
            {
                float newRotation = rotateableObject.RotationY;
                if (transformation.MirrorX)
                    newRotation = -newRotation;
                newRotation -= transformation.QuadrantRotation * 90;
                rotateableObject.RotationY = newRotation;
            }
            else if (this is GhostBlockInstance ghostBlock) // TODO: Replace this with an interface
            {
                transformation.TransformValueDiagonalQuad(ref ghostBlock.Floor.XpZp, ref ghostBlock.Floor.XnZp, ref ghostBlock.Floor.XnZn, ref ghostBlock.Floor.XpZn);
                transformation.TransformValueDiagonalQuad(ref ghostBlock.Ceiling.XpZp, ref ghostBlock.Ceiling.XnZp, ref ghostBlock.Ceiling.XnZn, ref ghostBlock.Ceiling.XpZn);
            }
        }

        public virtual void TransformRoomReferences(Func<Room, Room> transformRoom)
        { }

        public virtual bool CopyToAlternateRooms => true;

        public virtual void CopyDependentLevelSettings(Room.CopyDependentLevelSettingsArgs args)
        { }

        bool IEquatable<ITriggerParameter>.Equals(ITriggerParameter other) => this == other;

        public virtual string ToShortString()
        {
            var shortName = GetType().GetMethod("ShortName");

            if (shortName != null)
                return shortName.Invoke(this, null).ToString();
            else
                return ToString();
        }
    }

    public abstract class SectorBasedObjectInstance : ObjectInstance
    {
        private RectangleInt2 _area;
        public RectangleInt2 Area
        {
            get { return _area; }
            set
            {
                if (Room != null)
                    throw new InvalidOperationException("Sector based objects may not change in size while they are assigned to a room.");
                _area = value;
            }
        }

        public SectorBasedObjectInstance(RectangleInt2 area)
        {
            Area = area;
        }

        public SectorBasedObjectInstance Clone(RectangleInt2 area)
        {
            var result = (SectorBasedObjectInstance)Clone();
            result.Area = area;
            return result;
        }

        public virtual RectangleInt2 GetValidArea(RectangleInt2 newLocalRoomArea)
        {
            return newLocalRoomArea.Inflate(-1); // Not on room walls
        }

        public override void Transform(RectTransformation transformation, VectorInt2 oldRoomSize)
        {
            base.Transform(transformation, oldRoomSize);
            _area = transformation.TransformRect(_area, oldRoomSize);
        }

        public static List<RectangleInt2> SplitIntoRectangles(RectangleInt2 area, Func<VectorInt2, bool> sectorToBeCovered)
        {
            // Check were the map must be covered
            bool[,] toBeCoveredMap = new bool[area.Width + 1, area.Height + 1];
            for (int x = 0; x <= area.Width; ++x)
                for (int z = 0; z <= area.Height; ++z)
                    toBeCoveredMap[x, z] = sectorToBeCovered(new VectorInt2(x, z) + area.Start);

            // Add rectanges greedily to cover space
            List<RectangleInt2> result = new List<RectangleInt2>();
            for (int currentX = 0; currentX <= area.Width; ++currentX)
                for (int currentZ = 0; currentZ <= area.Height; ++currentZ)
                    if (toBeCoveredMap[currentX, currentZ])
                    { // Needs to be covered with a rectangles.
                        // Create a rectangle and cover as much space as possible
                        int endX = currentX;
                        while (endX < area.Width)
                        {
                            if (!toBeCoveredMap[endX + 1, currentZ])
                                break;
                            ++endX;
                        }
                        int endZ = currentZ;
                        while (endZ < area.Height)
                        {
                            for (int x = currentX; x <= endX; ++x)
                                if (!toBeCoveredMap[x, endZ + 1])
                                    goto ExitEndZSearch;
                            ++endZ;
                        }
                        ExitEndZSearch:

                        // Update map and create actual rectangle
                        for (int x = currentX; x <= endX; ++x)
                            for (int z = currentZ; z <= endZ; ++z)
                                toBeCoveredMap[x, z] = false;
                        result.Add(new RectangleInt2(currentX, currentZ, endX, endZ) + area.Start);
                    }
            return result;
        }

        public IEnumerable<SectorBasedObjectInstance> SplitIntoRectangles(Func<VectorInt2, bool> sectorToBeCovered, VectorInt2 offset)
        {
            return SplitIntoRectangles(Area, sectorToBeCovered).Select(area => Clone(area + offset));
        }
    }

    public abstract class PositionBasedObjectInstance : ObjectInstance, ISpatial, ICopyable
    {
        private Vector3 _position;
        public Vector3 Position
        {
            get { return _position; }
            set { SetPosition(value); }
        }

        protected virtual void SetPosition(Vector3 position)
        {
            _position = position;
        }

        public VectorInt2 SectorPosition
        {
            get { return new VectorInt2((int)(Position.X / Level.SectorSizeUnit), (int)(Position.Z / Level.SectorSizeUnit)); }
            set { Position = new Vector3(value.X * Level.SectorSizeUnit, Position.Y, value.Y * Level.SectorSizeUnit); }
        }

        public Vector3 WorldPosition => Room != null ? Room.WorldPos + Position : Position;

        // All object matrices are precached on any transform, which allows to save some CPU time
        // with big level previews.

        public Matrix4x4 WorldPositionMatrix
        { get { UpdateMatrices(); return _lastWorldPositionMatrix; } }
        private Matrix4x4 _lastWorldPositionMatrix;

        public Matrix4x4 LocalPositionMatrix
        { get { UpdateMatrices(); return _lastLocalPositionMatrix; } }
        private Matrix4x4 _lastLocalPositionMatrix;

        public Matrix4x4 RotationMatrix
        { get { UpdateMatrices(); return _lastRotationMatrix; } }
        private Matrix4x4 _lastRotationMatrix;

        public Matrix4x4 ScaleMatrix
        { get { UpdateMatrices(); return _lastScaleMatrix; } }
        private Matrix4x4 _lastScaleMatrix;

        public Matrix4x4 RotationPositionMatrix
        { get { UpdateMatrices(); return _lastRotPosMatrix; } }
        private Matrix4x4 _lastRotPosMatrix;

        public Matrix4x4 ObjectMatrix
        { get { UpdateMatrices(); return _lastObjectMatrix; } }
        private Matrix4x4 _lastObjectMatrix;

        public Matrix4x4 LocalObjectMatrix
        { get { UpdateMatrices(); return _lastLocalObjectMatrix; } }
        private Matrix4x4 _lastLocalObjectMatrix;

        private bool UpdateMatrices()
        {
            bool result = false;

            if (Room.Position != _lastRoomPosition)
            {
                result = true;
                _lastRoomPosition = Room.Position;
            }

            if (Position != _lastPosition)
            {
                result = true;
                _lastPosition = Position;
            }

            var currentRotation = new Vector3()
            {
                X = (this as IRotateableY)?.RotationY ?? 0.0f,
                Y = (this as IRotateableYX)?.RotationX ?? 0.0f,
                Z = (this as IRotateableYXRoll)?.Roll ?? 0.0f
            };
            if (currentRotation != _lastRotation)
            {
                result = true;
                _lastRotation = currentRotation;
            }

            var currentScale = ((this as ISizeable)?.Size ?? new Vector3(1.0f)) * ((this as IScaleable)?.Scale ?? 1.0f);
            if (currentScale != _lastScale)
            {
                result = true;
                _lastScale = currentScale;
            }

            if (result)
            {
                _lastLocalPositionMatrix = Matrix4x4.CreateTranslation(Position);
                _lastWorldPositionMatrix = _lastLocalPositionMatrix * Matrix4x4.CreateTranslation(Room?.WorldPos ?? new Vector3());
                _lastRotationMatrix = Matrix4x4.CreateFromYawPitchRoll(
                                       (this as IRotateableY)?.GetRotationYRadians() ?? 0.0f,
                                      -(this as IRotateableYX)?.GetRotationXRadians() ?? 0.0f,
                                       (this as IRotateableYXRoll)?.GetRotationRollRadians() ?? 0.0f);
                _lastRotPosMatrix = _lastRotationMatrix * _lastWorldPositionMatrix;
                _lastScaleMatrix = Matrix4x4.CreateScale(_lastScale);
                _lastObjectMatrix = _lastRotationMatrix * _lastScaleMatrix * _lastWorldPositionMatrix;
                _lastLocalObjectMatrix = _lastRotationMatrix * _lastScaleMatrix * _lastLocalPositionMatrix;
            }

            return result;
        }
        private VectorInt3 _lastRoomPosition = new VectorInt3(int.MinValue);
        private Vector3 _lastPosition = new Vector3(float.MinValue);
        private Vector3 _lastRotation = new Vector3(float.MinValue);
        private Vector3 _lastScale = new Vector3(float.MinValue);

        public void Move(int deltaX, int deltaY, int deltaZ)
        {
            Position = Position + new Vector3(deltaX, deltaY, deltaZ);
        }

        public Rectangle2 GetViewportRect(RectangleInt2 bounds, Vector3 camPos, Matrix4x4 camViewProjection, Size viewportSize, out float depth)
        {
            var heightRatio = ((float)viewportSize.Width / viewportSize.Height) * Level.SectorSizeUnit;
            var distance = Vector3.Distance(Position + Room.WorldPos, camPos);
            var scale = (Level.SectorSizeUnit * 2.0f) / (distance != 0 ? distance : 1.0f);
            var pos = (WorldPositionMatrix * camViewProjection).TransformPerspectively(new Vector3());
            var screenPos = pos.To2();
            var start = scale * new Vector2(bounds.Start.X / heightRatio, bounds.Start.Y / Level.SectorSizeUnit);
            var end = scale * new Vector2(bounds.End.X / heightRatio, bounds.End.Y / Level.SectorSizeUnit);

            depth = pos.Z;
            return new Rectangle2(screenPos - end, screenPos - start);
        }

        public override void Transform(RectTransformation transformation, VectorInt2 oldRoomSize)
        {
            base.Transform(transformation, oldRoomSize);
            Position = transformation.TransformVec3(Position, oldRoomSize.X * Level.SectorSizeUnit, oldRoomSize.Y * Level.SectorSizeUnit);
        }
    }

    public abstract class PositionAndScriptBasedObjectInstance : PositionBasedObjectInstance, IHasScriptID, IHasLuaName
    {
        private ScriptIdTable<IHasScriptID> _scriptTable;
        private uint? _scriptId;
        public uint? ScriptId
        {
            get
            {
                return _scriptId;
            }
            set
            {
                _scriptTable?.Update(this, _scriptId, value);
                _scriptId = value;
            }
        }
        public string LuaName { get; set; }

        public void AllocateNewScriptId()
        {
            _scriptTable = Room.Level.GlobalScriptingIdsTable; // HACK: Prevent exception on TRTombalized room copy-paste events
            _scriptId = _scriptTable?.UpdateWithNewId(this, _scriptId);
        }

        public bool TrySetScriptId(uint? newScriptId)
        {
            if (_scriptTable != null && !_scriptTable.TryUpdate(this, _scriptId, newScriptId))
                return false;
            _scriptId = newScriptId;
            return true;
        }

        public override ObjectInstance Clone()
        {
            ObjectInstance result = base.Clone();
            ((PositionAndScriptBasedObjectInstance)result)._scriptId = null;
            ((PositionAndScriptBasedObjectInstance)result).LuaName = string.Empty;
            return result;
        }

        public override void AddToRoom(Level level, Room room)
        {
            base.AddToRoom(level, room);
            try
            {
                level.GlobalScriptingIdsTable.Update(this, null, ScriptId);
            }
            catch (Exception)
            {
                base.RemoveFromRoom(level, room);
                throw;
            }
            _scriptTable = level.GlobalScriptingIdsTable;
        }

        public override void RemoveFromRoom(Level level, Room room)
        {
            base.RemoveFromRoom(level, room);
            try
            {
                level.GlobalScriptingIdsTable.Update(this, ScriptId, null);
            }
            catch (Exception)
            {
                base.AddToRoom(level, room);
                throw;
            }
            _scriptTable = null;
        }

        public void AllocateNewLuaName()
        {
            var objects = Room.Level.GetAllObjects().Where(o => o is IHasLuaName);
            int numObjects = objects.Count();

            while (true)
            {
                string luaId = "";
                if (this is MoveableInstance)
                    luaId = (this as MoveableInstance).WadObjectId.ShortName(TRVersion.Game.TombEngine).ToLower() + "_" + numObjects;
                else if (this is StaticInstance)
                    luaId = "static_mesh_" + numObjects;
                else if (this is SinkInstance)
                    luaId = "sink_" + numObjects;
                else if (this is SoundSourceInstance)
                    luaId = "sound_source_" + numObjects;
                else if (this is CameraInstance)
                    luaId = "camera_" + numObjects;
                else if (this is VolumeInstance)
                    luaId = "volume_" + numObjects;
                else
                    return;

                if (objects.Where(o => o is IHasLuaName && (o as IHasLuaName).LuaName == luaId).Count() == 0)
                {
                    LuaName = luaId;
                    return;
                }
                else
                {
                    numObjects++;
                }
            }
        }

        public bool CanSetLuaName(string newName)
        {
            return (string.IsNullOrEmpty(newName) ||
                          Room.Level.GetAllObjects().Where(o => o is IHasLuaName &&
                                                          (o as IHasLuaName).LuaName == newName &&
                                                           o != this).Count() == 0);
        }

        public string GetScriptIDOrName(bool shortened = true)
        {
            if (Room == null)
                return " <NO ROOM>";

            if (shortened)
                return (Room.Level.IsNG ? (ScriptId.HasValue ? " <" + ScriptId.Value + ">" : "") : "") +
                       (Room.Level.IsTombEngine ? (!string.IsNullOrEmpty(LuaName) ? " [" + LuaName + "]" : "") : "");
            else
                return (Room.Level.IsNG ? (ScriptId.HasValue ? ", Script ID = " + ScriptId.Value : "") : "") +
                       (Room.Level.IsTombEngine ? (!string.IsNullOrEmpty(LuaName) ? ", Lua name = " + LuaName : "") : "");
        }
    }

    public static class ColorableExtensions
    {
        public static bool CanBeColored(this ObjectInstance obj)
        {
            if (obj is IColorable)
            {
                bool changeColor = true;

                // Discard color editing if conditions aren't met
                // FIXME: For TombEngine, it may be considered to apply dynamic lighting in addition to tint, so those conditions can be then changed.

                if (obj is MoveableInstance)
                {
                    // TEN moveables can be always colored
                    if (obj.Room.Level.IsTombEngine)
                        return true;

                    var model = obj.Room.Level.Settings.WadTryGetMoveable((obj as MoveableInstance).WadObjectId);
                    if (model == null || !model.Meshes.Any(m => m.LightingType != Wad.WadMeshLightingType.Normals))
                        changeColor = false;

                    // HACK: TR1-2 pickups can be colored because they are sprites.
                    // Guess them by getting substitute ID.

                    if (obj.Room.Level.Settings.GameVersion.Native() <= TRVersion.Game.TR2)
                    {
                        var id = (obj as MoveableInstance).WadObjectId.TypeId;
                        var subId = TrCatalog.GetSubstituteID(obj.Room.Level.Settings.GameVersion, id);
                        if (id != subId)
                            changeColor = true;
                    }
                }
                else if (obj is StaticInstance)
                {
                    // TEN statics can be always colored
                    if (obj.Room.Level.IsTombEngine)
                        return true;

                    var mesh = obj.Room.Level.Settings.WadTryGetStatic((obj as StaticInstance).WadObjectId);
                    if (mesh == null || mesh.Mesh.LightingType == Wad.WadMeshLightingType.Normals)
                        changeColor = false;
                }
                else if (obj is LightInstance && (obj as LightInstance).Type == LightType.FogBulb && obj.Room.Level.Settings.GameVersion.Native() <= TRVersion.Game.TR4)
                {
                    changeColor = false;
                }
                else if (obj is ObjectGroup)
                {
                    changeColor = (obj as ObjectGroup).Any(o => o.CanBeColored());
                }

                return changeColor;
            }
            else
                return false;
        }
    }

    public static class RotatableExtensions
    {
        public static float GetRotationYRadians(this IRotateableY obj)
        {
            return obj.RotationY * (float)(Math.PI / 180.0);
        }
        public static void SetRotationYRadians(this IRotateableY obj, float value)
        {
            obj.RotationY = value * (float)(180.0 / Math.PI);
        }

        public static float GetRotationXRadians(this IRotateableYX obj)
        {
            return obj.RotationX * (float)(Math.PI / 180.0);
        }
        public static void SetRotationXRadians(this IRotateableYX obj, float value)
        {
            obj.RotationX = value * (float)(180.0 / Math.PI);
        }

        public static float GetRotationRollRadians(this IRotateableYXRoll obj)
        {
            return obj.Roll * (float)(Math.PI / 180.0);
        }
        public static void SetRotationRollRadians(this IRotateableYXRoll obj, float value)
        {
            obj.Roll = value * (float)(180.0 / Math.PI);
        }

        public static Vector3 GetDirection(this IRotateableYX obj)
        {
            float RadiansX = GetRotationXRadians(obj);
            float RadiansY = GetRotationYRadians(obj);
            return new Vector3(
                (float)(Math.Cos(RadiansX) * Math.Sin(RadiansY)),
                (float)Math.Sin(RadiansX),
                (float)(Math.Cos(RadiansX) * Math.Cos(RadiansY)));
        }

        public static void SetArbitaryRotationsYX(this IRotateableYX obj, float rotationY, float rotationX)
        {
            rotationX -= (float)(360 * Math.Floor(rotationX / 360));
            if (rotationX > 270)
                rotationX -= 360;
            else if (rotationX > 90)
            {
                rotationX = 180 - rotationX;
                rotationY += 180;
            }

            obj.RotationX = rotationX;
            obj.RotationY = rotationY;
        }
    }
}
