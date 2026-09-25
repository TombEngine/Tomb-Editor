using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.GeometryIO
{
    public class IOModel
    {
        public List<IOMesh> Meshes { get; private set; } = new List<IOMesh>();
        public List<IOMaterial> Materials { get; private set; } = new List<IOMaterial>();
        public List<IOAnimation> Animations { get; private set; } = new List<IOAnimation>();

        // Used only by Tomb Editor for managing the special case of multiple rooms
        public bool HasMultipleRooms { get; set; }

        public BoundingBox BoundingBox
        {
            get
            {
                Vector3 minVertex = new Vector3(float.PositiveInfinity);
                Vector3 maxVertex = new Vector3(float.NegativeInfinity);
                foreach (IOMesh mesh in Meshes)
                {
                    BoundingBox partialBoundingBox = mesh.BoundingBox;
                    minVertex = Vector3.Min(minVertex, partialBoundingBox.Minimum);
                    maxVertex = Vector3.Max(maxVertex, partialBoundingBox.Maximum);
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

        public IOMaterial GetMaterial(Texture texture, BlendMode blendMode, int page, bool doubleSided, int shininess)
        {
            foreach (var mat in Materials)
                if (mat.Page == page)
                    if (mat.Texture.Equals(texture))
                        if (mat.BlendMode == blendMode)
                            if (mat.DoubleSided == doubleSided)
                                if (mat.Shininess == shininess)
                                    return mat;
            return null;
        }

        public List<IOMaterial> UsedMaterials => Materials.Where(mat => Meshes.Any(m => m.Submeshes.Any(s => s.Key == mat && s.Value.Polygons.Count > 0))).ToList();
    }
}
