using TombLib.Graphics.Dx11Toolkit;
using System.Numerics;
using TombLib.Graphics;
using Buffer = TombLib.Graphics.Dx11Toolkit.Buffer;

namespace TombLib.Rendering
{
    public static class BoundingBoxExtensions
    {
        public static Buffer<SolidVertex> GetVertexBuffer(this BoundingBox box, GraphicsDevice device)
        {
            var p0 = new SolidVertex(new Vector3(box.Minimum.X, box.Minimum.Y, box.Minimum.Z));
            var p1 = new SolidVertex(new Vector3(box.Maximum.X, box.Minimum.Y, box.Minimum.Z));
            var p2 = new SolidVertex(new Vector3(box.Maximum.X, box.Minimum.Y, box.Maximum.Z));
            var p3 = new SolidVertex(new Vector3(box.Minimum.X, box.Minimum.Y, box.Maximum.Z));
            var p4 = new SolidVertex(new Vector3(box.Minimum.X, box.Maximum.Y, box.Minimum.Z));
            var p5 = new SolidVertex(new Vector3(box.Maximum.X, box.Maximum.Y, box.Minimum.Z));
            var p6 = new SolidVertex(new Vector3(box.Maximum.X, box.Maximum.Y, box.Maximum.Z));
            var p7 = new SolidVertex(new Vector3(box.Minimum.X, box.Maximum.Y, box.Maximum.Z));

            var vertices = new[]
            {
                p4, p5, p5, p1, p1, p0, p0, p4,
                p5, p6, p6, p2, p2, p1, p1, p5,
                p2, p6, p6, p7, p7, p3, p3, p2,
                p7, p4, p4, p0, p0, p3, p3, p7,
                p7, p6, p6, p5, p5, p4, p4, p7,
                p0, p1, p1, p2, p2, p3, p3, p0
            };

            return Buffer.New(device, vertices, BufferFlags.VertexBuffer, ResourceUsage.Default);
        }
    }
}
