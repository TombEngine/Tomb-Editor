using System.Collections.Generic;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.GeometryIO
{
    public class IOMesh
    {
        public string Name { get; private set; }
        public List<Vector3> Positions { get; private set; } = new List<Vector3>();
        public List<List<KeyValuePair<int, float>>> Weights { get; private set; } = new List<List<KeyValuePair<int, float>>>();
        public List<Vector3> Normals { get; private set; } = new List<Vector3>();
        public List<Vector2> UV { get; private set; } = new List<Vector2>();
        public List<Vector4> Colors { get; private set; } = new List<Vector4>();
        public Dictionary<IOMaterial, IOSubmesh> Submeshes { get; private set; } = new Dictionary<IOMaterial, IOSubmesh>();
        public Vector3 Position { get; set; }
        public Vector3 Origin { get; set; }

        public IOMesh(string name)
        {
            Name = name;
            Position = Vector3.Zero;
        }

        public int NumQuads
        {
            get
            {
                var numQuads = 0;
                foreach (var submesh in Submeshes)
                    foreach (var poly in submesh.Value.Polygons)
                        if (poly.Shape == IOPolygonShape.Quad)
                        numQuads++;
                return numQuads;
            }
        }

        public int NumTriangles
        {
            get
            {
                var numTriangles = 0;
                foreach (var submesh in Submeshes)
                    foreach (var poly in submesh.Value.Polygons)
                        if (poly.Shape == IOPolygonShape.Triangle)
                            numTriangles++;
                return numTriangles;
            }
        }

        public int NumIndices
        {
            get
            {
                var numIndieces = 0;
                foreach (var submesh in Submeshes)
                    foreach (var poly in submesh.Value.Polygons)
                    {
                        if (poly.Shape == IOPolygonShape.Triangle)
                            numIndieces += 3;
                        if (poly.Shape == IOPolygonShape.Quad)
                            numIndieces += 4;
                    }
                return numIndieces;
            }
        }

        public List<uint> Indices
        {
            get
            {
                List<uint> indices = new List<uint>();
                foreach (var submesh in Submeshes)
                    foreach (var poly in submesh.Value.Polygons)
                        if(poly.Shape == IOPolygonShape.Quad)
                        {
                            indices.Add((uint)poly.Indices[0]);
                            indices.Add((uint)poly.Indices[1]);
                            indices.Add((uint)poly.Indices[3]);
                            indices.Add((uint)poly.Indices[1]);
                            indices.Add((uint)poly.Indices[2]);
                            indices.Add((uint)poly.Indices[3]);
                            
                        }
                        else if(poly.Shape == IOPolygonShape.Triangle)
                        {
                            indices.Add((uint)poly.Indices[0]);
                            indices.Add((uint)poly.Indices[1]);
                            indices.Add((uint)poly.Indices[2]);
                        }
                return indices;
            }
        }

        public int NumPolygons
        {
            get
            {
                return NumQuads + NumTriangles;
            }
        }

        public BoundingBox BoundingBox
        {
            get
            {
                Vector3 minVertex = new Vector3(float.PositiveInfinity);
                Vector3 maxVertex = new Vector3(float.NegativeInfinity);
                foreach (Vector3 position in Positions)
                {
                    minVertex = Vector3.Min(minVertex, position);
                    maxVertex = Vector3.Max(maxVertex, position);
                }
                return new BoundingBox(minVertex, maxVertex);
            }
        }

        public BoundingSphere BoundingSphere
        {
            get
            {
                BoundingBox boundingBox = BoundingBox;
                Vector3 center = (boundingBox.Minimum + boundingBox.Maximum) * 0.5f;
                float radius = (boundingBox.Maximum - center).Length();
                return new BoundingSphere(center, radius);
            }
        }

        public void CalculateNormals(bool weighted = true)
        {
            var helper = new VertexNormalAverageHelper(Positions);

            foreach (var submesh in Submeshes)
                foreach (var poly in submesh.Value.Polygons)
                {
                    if (poly.Shape == IOPolygonShape.Triangle)
                        helper.AddPolygon(weighted, poly.Indices[0], poly.Indices[1], poly.Indices[2]);
                    else
                        helper.AddPolygon(weighted, poly.Indices[0], poly.Indices[1], poly.Indices[2], poly.Indices[3]);
                }

            Normals = helper.CalculateNormals();
        }
    }
}
