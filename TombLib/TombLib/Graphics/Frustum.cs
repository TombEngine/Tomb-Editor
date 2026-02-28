using System;
using System.Drawing;
using System.Numerics;

namespace TombLib.Graphics
{
    /// <summary>
    /// View frustum for culling. Uses System.Numerics planes instead of SharpDX.BoundingFrustum.
    /// Planes are extracted via the Gribb/Hartmann method from a view-projection matrix.
    /// </summary>
    public class Frustum
    {
        private const float _frustum_divisor = 1024.0f;

        // 6 frustum planes: Left, Right, Bottom, Top, Near, Far
        private Plane[] _planes;
        private FrustumCameraParams? _frustumParams;

        public void Update(Camera camera, Size viewportSize)
        {
            var pos = camera.GetPosition() / _frustum_divisor;
            var target = camera.Target / _frustum_divisor;
            var dir = Vector3.Normalize(target - pos);

            Matrix4x4 rotMatrix = camera.GetRotationMatrix();
            Vector3 up = new Vector3(rotMatrix.M21, rotMatrix.M22, rotMatrix.M23);

            var frustumParams = new FrustumCameraParams
            {
                Position = pos,
                LookAtDir = dir,
                UpDir = up,
                AspectRatio = viewportSize.Width / (float)viewportSize.Height,
                ZFar = _frustum_divisor * 200,
                ZNear = 1 / _frustum_divisor,
                FOV = camera.FieldOfView * 1.2f,
            };

            if (!_frustumParams.HasValue ||
                 _frustumParams.Value.Position != frustumParams.Position ||
                 _frustumParams.Value.LookAtDir != frustumParams.LookAtDir ||
                 _frustumParams.Value.UpDir != frustumParams.UpDir ||
                 _frustumParams.Value.FOV != frustumParams.FOV)
            {
                _frustumParams = frustumParams;
                _planes = BuildFrustumPlanes(frustumParams);
            }
        }

        public bool Contains(BoundingBox box)
        {
            var min = box.Minimum / _frustum_divisor;
            var max = box.Maximum / _frustum_divisor;
            return TestAABB(min, max);
        }

        public bool Contains(Vector3 point)
        {
            var p = point / _frustum_divisor;
            return TestPoint(p);
        }

        public bool Contains(BoundingSphere sphere)
        {
            var center = sphere.Center / _frustum_divisor;
            float radius = sphere.Radius / _frustum_divisor;
            return TestSphere(center, radius);
        }

        // ── Plane extraction (Gribb/Hartmann) ───────────────────────

        private static Plane[] BuildFrustumPlanes(FrustumCameraParams p)
        {
            // Build LH view + perspective projection matrices
            var view = CreateLookAtLH(p.Position, p.Position + p.LookAtDir, p.UpDir);
            var proj = CreatePerspectiveFovLH(p.FOV, p.AspectRatio, p.ZNear, p.ZFar);
            var vp = view * proj; // row-major: view then proj

            var planes = new Plane[6];
            // Left
            planes[0] = NormalizePlane(new Plane(
                vp.M14 + vp.M11, vp.M24 + vp.M21, vp.M34 + vp.M31, vp.M44 + vp.M41));
            // Right
            planes[1] = NormalizePlane(new Plane(
                vp.M14 - vp.M11, vp.M24 - vp.M21, vp.M34 - vp.M31, vp.M44 - vp.M41));
            // Bottom
            planes[2] = NormalizePlane(new Plane(
                vp.M14 + vp.M12, vp.M24 + vp.M22, vp.M34 + vp.M32, vp.M44 + vp.M42));
            // Top
            planes[3] = NormalizePlane(new Plane(
                vp.M14 - vp.M12, vp.M24 - vp.M22, vp.M34 - vp.M32, vp.M44 - vp.M42));
            // Near
            planes[4] = NormalizePlane(new Plane(
                vp.M13, vp.M23, vp.M33, vp.M43));
            // Far
            planes[5] = NormalizePlane(new Plane(
                vp.M14 - vp.M13, vp.M24 - vp.M23, vp.M34 - vp.M33, vp.M44 - vp.M43));
            return planes;
        }

        private static Plane NormalizePlane(Plane p)
        {
            float len = new Vector3(p.Normal.X, p.Normal.Y, p.Normal.Z).Length();
            if (len < 1e-12f) return p;
            return new Plane(p.Normal / len, p.D / len);
        }

        // ── Containment tests ────────────────────────────────────────

        private bool TestPoint(Vector3 p)
        {
            if (_planes == null) return true;
            for (int i = 0; i < 6; i++)
            {
                if (DistanceToPlane(_planes[i], p) < 0)
                    return false;
            }
            return true;
        }

        private bool TestSphere(Vector3 center, float radius)
        {
            if (_planes == null) return true;
            for (int i = 0; i < 6; i++)
            {
                if (DistanceToPlane(_planes[i], center) < -radius)
                    return false;
            }
            return true;
        }

        private bool TestAABB(Vector3 min, Vector3 max)
        {
            if (_planes == null) return true;
            for (int i = 0; i < 6; i++)
            {
                // "P-vertex" — the AABB corner most in the direction of the plane normal
                var pVertex = new Vector3(
                    _planes[i].Normal.X >= 0 ? max.X : min.X,
                    _planes[i].Normal.Y >= 0 ? max.Y : min.Y,
                    _planes[i].Normal.Z >= 0 ? max.Z : min.Z);

                if (DistanceToPlane(_planes[i], pVertex) < 0)
                    return false;
            }
            return true;
        }

        private static float DistanceToPlane(Plane plane, Vector3 point)
        {
            return Vector3.Dot(plane.Normal, point) + plane.D;
        }

        // ── Matrix helpers (LH) ──────────────────────────────────────

        private static Matrix4x4 CreateLookAtLH(Vector3 eye, Vector3 target, Vector3 up)
        {
            var zaxis = Vector3.Normalize(target - eye);
            var xaxis = Vector3.Normalize(Vector3.Cross(up, zaxis));
            var yaxis = Vector3.Cross(zaxis, xaxis);

            return new Matrix4x4(
                xaxis.X, yaxis.X, zaxis.X, 0,
                xaxis.Y, yaxis.Y, zaxis.Y, 0,
                xaxis.Z, yaxis.Z, zaxis.Z, 0,
                -Vector3.Dot(xaxis, eye), -Vector3.Dot(yaxis, eye), -Vector3.Dot(zaxis, eye), 1);
        }

        private static Matrix4x4 CreatePerspectiveFovLH(float fov, float aspect, float znear, float zfar)
        {
            float h = 1.0f / MathF.Tan(fov * 0.5f);
            float w = h / aspect;
            float range = zfar / (zfar - znear);

            return new Matrix4x4(
                w, 0, 0, 0,
                0, h, 0, 0,
                0, 0, range, 1,
                0, 0, -range * znear, 0);
        }
    }

    /// <summary>Camera parameters for frustum construction.</summary>
    internal struct FrustumCameraParams
    {
        public Vector3 Position;
        public Vector3 LookAtDir;
        public Vector3 UpDir;
        public float FOV;
        public float AspectRatio;
        public float ZNear;
        public float ZFar;
    }
}
