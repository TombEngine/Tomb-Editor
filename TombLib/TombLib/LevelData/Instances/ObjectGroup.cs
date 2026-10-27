#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace TombLib.LevelData
{
    /// <summary>
    /// Represents a group of objects multi-selected by ctrl-clicking.
    /// </summary>
    public class ObjectGroup : PositionBasedObjectInstance, IRotateableY, IColorable, IEnumerable<PositionBasedObjectInstance>
    {
        private readonly HashSet<PositionBasedObjectInstance> _objects = new();
        private PositionBasedObjectInstance? _rootObject;

        public ObjectGroup(PositionBasedObjectInstance initialObject)
        {
            Room = initialObject.Room;
            Position = initialObject.Position;
            _rootObject = initialObject;

            _objects.Add(initialObject);
        }

        public ObjectGroup(IReadOnlyList<PositionBasedObjectInstance> objects)
            : this(objects, null, 0.0f)
        { }

        private ObjectGroup(IReadOnlyList<PositionBasedObjectInstance> objects, PositionBasedObjectInstance? rootObject, float rotationY)
        {
            if (objects is null || objects.Count == 0)
                throw new ArgumentException("The collection of objects must not be null or empty.", nameof(objects));

            // Ensure the provided root belongs to the collection; otherwise fall back to the first element.
            var initialObject = rootObject is not null && objects.Contains(rootObject) ? rootObject : objects[0];

            Room = initialObject.Room;
            Position = initialObject.Position;
            _rootObject = initialObject;
            _rotationY = rotationY;

            foreach (var obj in objects)
                _objects.Add(obj);
        }

        public ObjectGroup(ObjectGroup other)
        {
            Room = other.Room;
            Position = other.Position;
            _rotationY = other._rotationY;
            _rootObject = other.RootObject;

            foreach (var obj in other)
                _objects.Add(obj);
        }

        public override ObjectInstance Clone()
        {
            var clonedObjects = new List<PositionBasedObjectInstance>(_objects.Count);
            PositionBasedObjectInstance? clonedRootObject = null;

            foreach (var obj in _objects)
            {
                var clonedObject = (PositionBasedObjectInstance)obj.Clone();
                clonedObjects.Add(clonedObject);

                if (obj == _rootObject)
                    clonedRootObject = clonedObject;
            }

            var rootToUse = clonedRootObject ?? clonedObjects.FirstOrDefault();
            return new ObjectGroup(clonedObjects, rootToUse, _rotationY);
        }

        public void Add(PositionBasedObjectInstance objectInstance)
        {
            _objects.Add(objectInstance);
            _rootObject ??= objectInstance;
        }

        public void Remove(PositionBasedObjectInstance objectInstance)
        {
            if (!_objects.Remove(objectInstance))
                return;

            if (_rootObject == objectInstance)
                _rootObject = _objects.FirstOrDefault();
        }

        public bool Contains(PositionBasedObjectInstance obInstance) => _objects.Contains(obInstance);
        public bool Any() => _objects.Any();
        public PositionBasedObjectInstance? RootObject => _rootObject;

        public void AddOrRemove(PositionBasedObjectInstance objectInstance)
        {
            if (Contains(objectInstance))
                Remove(objectInstance);
            else
                Add(objectInstance);
        }

        protected override void SetPosition(Vector3 position)
        {
            var difference = position - Position;
            base.SetPosition(position);

            foreach (var i in _objects)
                i.Position += difference;
        }

        private float _rotationY;

        public float RotationY
        {
            get => _rotationY;
            set
            {
                var difference = value - _rotationY;

                _rotationY = value;

                foreach (var i in _objects.OfType<IRotateableY>())
                    i.RotationY += difference;
            }
        }

        public Vector3 Color
        {
            get
            {
                if (RootObject?.CanBeColored() == true)
                    return ((IColorable)RootObject).Color; // Prioritize root object for picking color

                var coloredObject = this.FirstOrDefault(o => o.CanBeColored());

                if (coloredObject is not null)
                    return ((IColorable)coloredObject).Color;

                return Vector3.Zero;
            }
            set
            {
                foreach (var o in this.Where(o => o.CanBeColored()))
                    ((IColorable)o).Color = value;
            }
        }

        public void RotateAsGroup(float targetRotationDeg)
        {
            var rotationDifferenceRad = (targetRotationDeg - RotationY) * Math.PI / 180.0f;

            RotationY = targetRotationDeg;

            var sin = (float)Math.Sin(-rotationDifferenceRad);
            var cos = (float)Math.Cos(-rotationDifferenceRad);

            foreach (var i in _objects)
            {
                var distance = i.WorldPosition - WorldPosition;

                var x = distance.X * cos - distance.Z * sin + WorldPosition.X;
                var z = distance.X * sin + distance.Z * cos + WorldPosition.Z;

                i.Position = new Vector3(x - i.Room.WorldPos.X, i.Position.Y, z - i.Room.WorldPos.Z);
            }
        }

        public ObjectGroup SetRoom(Room room)
        {
            Room = room;
            return this;
        }

        public void SetOrigin(Vector3 position)
        {
            base.SetPosition(position);
        }

        public string ShortName() => $"Group of {_objects.Count} objects";
        public override string ToString() => ShortName();

        public IEnumerator<PositionBasedObjectInstance> GetEnumerator() => _objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _objects.GetEnumerator();
    }
}