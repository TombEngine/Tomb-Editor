using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.Graphics
{
    public enum ModelType : short
    {
        Static,
        Skinned,
        RoomGeometry,
        Room
    }

    public abstract class Model<T, U> : IRenderableObject, IDisposable where U : unmanaged where T : IDisposable
    {
        public BoundingBox BoundingBox { get; set; }
        public List<T> Meshes { get; set; }
        public T Skin { get; set; }
        public GraphicsDevice GraphicsDevice { get; set; }
        public ModelType Type { get; set; }
        public string Name { get; set; }
        public List<Material> Materials { get; private set; } = new List<Material>();
        public DataVersion Version { get; set; } = DataVersion.GetNext();

        public Model(GraphicsDevice device, ModelType type)
        {
            GraphicsDevice = device;
            Type = type;
            Meshes = new List<T>();
        }

        public abstract void UpdateBuffers(Vector3? position = null);

        public void Dispose()
        {
            foreach (var m in Meshes)
                m.Dispose();
        }
    }
}
