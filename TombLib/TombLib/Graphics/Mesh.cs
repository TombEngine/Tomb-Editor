using SharpDX.Toolkit.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace TombLib.Graphics
{
    public abstract class Mesh<T> : GraphicsResource, IRenderableObject where T : struct, IVertex
    {
        public Buffer<T> VertexBuffer { get; protected set; }
        public SharpDX.Toolkit.Graphics.Buffer IndexBuffer { get; protected set; }
        public VertexInputLayout InputLayout { get; protected set; }
        public List<Material> Materials { get; protected set; }
        public List<T> Vertices { get; protected set; } = new List<T>();
        public List<int> Indices { get; protected set; } = new List<int>();
        public Dictionary<Material, Submesh> Submeshes { get; private set; } = new Dictionary<Material, Submesh>();
        public bool Hidden { get; set; } = false;

        public BoundingSphere BoundingSphere { get; set; }
        public BoundingBox BoundingBox { get; set; }

        public Mesh(GraphicsDevice device, string name)
            : base(device, name)
        { }

        public void UpdateBoundingBox()
        {
            // Calculate bounding box
            Vector3 minVertex = new Vector3(float.MaxValue);
            Vector3 maxVertex = new Vector3(float.MinValue);
            foreach (var vertex in Vertices)
            {
                minVertex = Vector3.Min(minVertex, vertex.Position);
                maxVertex = Vector3.Max(maxVertex, vertex.Position);
            }
            BoundingBox = new BoundingBox(minVertex, maxVertex);
        }

        public void DepthSort(Vector3? position)
        {
            int lastBaseIndex = 0;
            Indices.Clear();

            foreach (var submesh in Submeshes)
            {
                submesh.Value.BaseIndex = lastBaseIndex;
                if (submesh.Value.NumIndices != 0)
                {
                    var indexList = new List<int[]>();

                    // Collect triangles

                    for (int i = 0; i < submesh.Value.NumIndices; i += 3)
                    {
                        int[] tri = new int[3] { submesh.Value.Indices[i], submesh.Value.Indices[i + 1], submesh.Value.Indices[i + 2] };
                        indexList.Add(tri);
                    }

                    // Sort triangles

                    if (position != null)
                        indexList = indexList.OrderByDescending(p => Vector3.Distance(position.Value,
                        (Vertices[p[0]].Position + Vertices[p[1]].Position + Vertices[p[2]].Position) / 3.0f)).ToList();

                    // Rebuild index list

                    foreach (var tri in indexList)
                        Indices.AddRange(tri);
                }
                lastBaseIndex += submesh.Value.NumIndices;
            }
        }

        public void Dispose()
        {
            base.Dispose();
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }
    }
}
