using TombLib.Graphics.Dx11Toolkit;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.Utils;
using TombLib.Wad;
using Buffer = TombLib.Graphics.Dx11Toolkit.Buffer;

namespace TombLib.Graphics
{
    public class StaticModel : Model<ObjectMesh, ObjectVertex>
    {
        public StaticModel(GraphicsDevice device)
            : base(device, ModelType.Static)
        {}

        public override void UpdateBuffers(Vector3? position = null)
        {
            foreach (var mesh in Meshes)
            {
                mesh.UpdateBoundingBox();
                mesh.UpdateBuffers(position);
            }
        }
    }
}
