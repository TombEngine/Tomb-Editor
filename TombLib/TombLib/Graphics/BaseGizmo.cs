using SharpDX.Toolkit.Graphics;
using System;
using System.Numerics;
using TombLib.Graphics.Primitives;
using TombLib.Utils;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public enum GizmoOrientation : byte
    {
        Normal,
        UpsideDown
    }

    public enum GizmoMode : byte
    {
        None,
        TranslateX,
        TranslateY,
        TranslateZ,
        RotateZ,
        RotateX,
        RotateY,
        ScaleX,
        ScaleY,
        ScaleZ
    }

    public class PickingResultGizmo : PickingResult
    {
        internal float RotationPickAngle { get; set; }
        internal float RotationPickDistance { get; set; }
        internal GizmoMode Mode { get; set; }

        internal PickingResultGizmo(GizmoMode mode)
        {
            Mode = mode;
        }
        internal PickingResultGizmo(GizmoMode mode, float rotationPickAngle, float rotationPickDistance)
        {
            Mode = mode;
            RotationPickAngle = rotationPickAngle;
            RotationPickDistance = rotationPickDistance;
        }
    }

    public abstract class BaseGizmo : IDisposable
    {
        private const int _rotationTrianglesCount = 64;
        private const int _lineRadiusTesselation = 8;
        private const float _rotationAlpha = 0.58f;
        private const float _scaleSpeed = 0.0004f;

        private readonly RasterizerState _rasterizerWireframe;

        private readonly Effect _effect;

        // Geometry of the gizmo
        private readonly GraphicsDevice _device;
        private readonly Buffer<SolidVertex> _rotationHelperGeometry;
        private readonly GeometricPrimitive _cylinder;
        private readonly GeometricPrimitive _cube;
        private readonly GeometricPrimitive _cone;
        private GeometricPrimitive _torus;
        private float _torusRadius = float.MinValue;
        private static readonly Vector4 _xAxisColor = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        private static readonly Vector4 _yAxisColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        private static readonly Vector4 _zAxisColor = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
        private static readonly Vector4 _centerColor = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
        private static readonly Vector4 _hoveredAddition = new Vector4(0.6f, 0.6f, 0.6f, 1.0f);
        private static readonly float _arrowHeadOffsetMultiplier = 1.13f;

        private GizmoMode _mode;
        private Vector3 _scaleBase;
        private Vector3 _initialPosition;
        private float _rotationLastMouseAngle;
        private float _rotationLastMouseRadius;
        private float _rotationPickAngle;
        private float _rotationPickAngleOffset;

        // Frozen rotation matrices captured at gizmo activation time.
        // During a rotation drag, the rotation plane must remain fixed.
        // If recomputed from live Euler angles, the plane flips near ±90° pitch
        // (gimbal lock), corrupting the mouse-angle computation and causing
        // wild model spinning.
        private Matrix4x4 _frozenRotateMatrixY;
        private Matrix4x4 _frozenRotateMatrixX;
        private Matrix4x4 _frozenRotateMatrixZ;

        private GizmoMode _hoveredMode;

        public BaseGizmo(GraphicsDevice device, Effect effect)
        {
            _effect = effect;
            _device = device;

            // Create the gizmo geometry
            _rotationHelperGeometry = Buffer.Vertex.New<SolidVertex>(device, _rotationTrianglesCount * 3 + 2);
            _cylinder = GeometricPrimitive.Cylinder.New(_device, 1.0f, 1.0f, _lineRadiusTesselation);
            _cube = GeometricPrimitive.Cube.New(_device, 1.0f);
            _cone = GeometricPrimitive.Cone.New(_device, 1.0f, 1.3f, 16);

            // Create the rasterizer state for wireframe drawing
            var renderStateDesc = new SharpDX.Direct3D11.RasterizerStateDescription
            {
                CullMode = SharpDX.Direct3D11.CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 0,
                FillMode = SharpDX.Direct3D11.FillMode.Wireframe,
                IsAntialiasedLineEnabled = true,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0
            };
            _rasterizerWireframe = RasterizerState.New(_device, renderStateDesc);
        }

        public void Dispose()
        {
            _rasterizerWireframe?.Dispose();
            _rotationHelperGeometry?.Dispose();
            _cylinder?.Dispose();
            _cube?.Dispose();
            _cone?.Dispose();
            _torus?.Dispose();
        }

        private static bool ConstructPlaneIntersection(Vector3 Position, Matrix4x4 viewProjection, Ray ray, Vector3 perpendicularVector0, Vector3 perpendicularVector1, out Vector3 intersection)
        {
            // Choose the perpendicular plane that is more parallel to the camera plane to
            // maximize the available accuracy in the view space.
            Vector3 viewDirection = new Vector3(viewProjection.M31, viewProjection.M32, viewProjection.M33);
            float perpendicularVector0Dot = Math.Abs(Vector3.Dot(viewDirection, perpendicularVector0));
            float perpendicularVector1Dot = Math.Abs(Vector3.Dot(viewDirection, perpendicularVector1));
            Plane plane = MathC.CreatePlaneAtPoint(Position, perpendicularVector0Dot > perpendicularVector1Dot ? perpendicularVector0 : perpendicularVector1);

            // Construct intersection
            return Collision.RayIntersectsPlane(ray, plane, out intersection);
        }

        private static float SimplifyAngle(float angle)
        {
            return (float)(angle - Math.Round(angle / (2 * Math.PI)) * (2 * Math.PI));
        }

        /// <returns>true, if an iteraction with the gizmo is happening</returns>
        public bool MouseMoved(Matrix4x4 viewProjection, Ray ray)
        {
            if (!DrawGizmo || _mode == GizmoMode.None)
                return false;

            // Init movement variables
            bool upside = Orientation == GizmoOrientation.UpsideDown;
            var arrowShift = (upside ? -Size : Size) * _arrowHeadOffsetMultiplier;

            // Flip sizing dimensions if object is rotateable on Y axis
            bool flippedScale = false;
            if ((_mode == GizmoMode.ScaleX || _mode == GizmoMode.ScaleZ) && SupportRotationY)
                flippedScale = MathC.RadToDeg(RotationY) % 180.0f >= 45.0f;

            // First get the ray in 3D space from X, Y mouse coordinates
            switch (_mode)
            {
                case GizmoMode.TranslateX:
                    {
                        Vector3 intersection;
                        if (ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitY, Vector3.UnitZ, out intersection))
                        {
                            var delta = _initialPosition.X - arrowShift;
                            GizmoMove(new Vector3(intersection.X - arrowShift - delta, Position.Y, Position.Z));
                            GizmoMoveDelta(new Vector3(intersection.X - arrowShift - delta - Position.X, 0.0f, 0.0f));
                        }
                    }
                    break;
                case GizmoMode.TranslateY:
                    {
                        Vector3 intersection;
                        if (ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitZ, out intersection))
                        {
                            var delta = _initialPosition.Y - arrowShift;
                            GizmoMove(new Vector3(Position.X, intersection.Y - arrowShift - delta, Position.Z));
                            GizmoMoveDelta(new Vector3(0.0f, intersection.Y - arrowShift - delta - Position.Y, 0.0f));
                        }
                    }
                    break;
                case GizmoMode.TranslateZ:
                    {
                        Vector3 intersection;
                        if (ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitY, out intersection))
                        {
                            var delta = _initialPosition.Z - arrowShift;
                            GizmoMove(new Vector3(Position.X, Position.Y, intersection.Z - arrowShift - delta));
                            GizmoMoveDelta(new Vector3(0.0f, 0.0f, intersection.Z - arrowShift - delta - Position.Z));
                        }
                    }
                    break;
                case GizmoMode.ScaleX:
                    {
                        Vector3 intersection;
                        if (ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitY, Vector3.UnitZ, out intersection))
                        {
                            var delta = _initialPosition.X - arrowShift;
                            var offset = intersection.X - arrowShift - delta - Position.X;

                            if (flippedScale)
                                GizmoScaleZ(_scaleBase.Z * (float)Math.Exp(_scaleSpeed * offset));
                            else
                                GizmoScaleX(_scaleBase.X * (float)Math.Exp(_scaleSpeed * offset));
                        }
                    }
                    break;
                case GizmoMode.ScaleY:
                    {
                        Vector3 intersection;
                        if (ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitZ, out intersection))
                        {
                            var delta = _initialPosition.Y - arrowShift;
                            var offset = intersection.Y - arrowShift - delta - Position.Y;

                            GizmoScaleY(_scaleBase.Y * (float)Math.Exp(_scaleSpeed * offset));
                        }
                    }
                    break;
                case GizmoMode.ScaleZ:
                    {
                        Vector3 intersection;
                        if (ConstructPlaneIntersection(Position, viewProjection, ray, Vector3.UnitX, Vector3.UnitY, out intersection))
                        {
                            var delta = _initialPosition.Z - arrowShift;
                            var offset = -(intersection.Z - arrowShift - delta - Position.Z);

                            if (flippedScale)
                                GizmoScaleX(_scaleBase.X * (float)Math.Exp(_scaleSpeed * offset));
                            else
                                GizmoScaleZ(_scaleBase.Z * (float)Math.Exp(_scaleSpeed * offset));
                        }
                    }
                    break;
                case GizmoMode.RotateY:
                    {
                        Plane rotationPlane = MathC.CreatePlaneAtPoint(Position, MathC.HomogenousTransform(Vector3.UnitY, _frozenRotateMatrixY));
                        Vector3 rotationIntersection;
                        if (Collision.RayIntersectsPlane(ray, rotationPlane, out rotationIntersection))
                        {
                            Vector3 direction = rotationIntersection - Position;
                            _rotationLastMouseRadius = direction.Length();
                            direction = Vector3.Normalize(direction);

                            float sin = Vector3.Dot(Vector3.UnitZ, direction);
                            float cos = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitZ, direction));
                            _rotationLastMouseAngle = (float)Math.Atan2(-sin, cos);
                            GizmoRotateY(SimplifyAngle(_rotationPickAngleOffset + _rotationLastMouseAngle));
                        }
                    }
                    break;
                case GizmoMode.RotateX:
                    {
                        Plane rotationPlane = MathC.CreatePlaneAtPoint(Position, MathC.HomogenousTransform(Vector3.UnitX, _frozenRotateMatrixX));
                        Vector3 rotationIntersection;
                        if (Collision.RayIntersectsPlane(ray, rotationPlane, out rotationIntersection))
                        {
                            Vector3 direction = rotationIntersection - Position;
                            _rotationLastMouseRadius = direction.Length();
                            direction = Vector3.Normalize(direction);

                            float sin = Vector3.Dot(Vector3.UnitY, direction);
                            float cos = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitY, direction));
                            _rotationLastMouseAngle = (float)Math.Atan2(-sin, cos);
                            GizmoRotateX(SimplifyAngle(_rotationPickAngleOffset + _rotationLastMouseAngle));
                        }
                    }
                    break;
                case GizmoMode.RotateZ:
                    {
                        Plane rotationPlane = MathC.CreatePlaneAtPoint(Position, MathC.HomogenousTransform(Vector3.UnitZ, _frozenRotateMatrixZ));
                        Vector3 rotationIntersection;
                        if (Collision.RayIntersectsPlane(ray, rotationPlane, out rotationIntersection))
                        {

                            Vector3 direction = rotationIntersection - Position;
                            _rotationLastMouseRadius = direction.Length();
                            direction = Vector3.Normalize(direction);

                            float sin = Vector3.Dot(Vector3.UnitY, direction);
                            float cos = Vector3.Dot(rotationPlane.Normal, Vector3.Cross(Vector3.UnitY, direction));
                            _rotationLastMouseAngle = (float)Math.Atan2(-sin, cos);
                            GizmoRotateZ(SimplifyAngle(_rotationPickAngleOffset + _rotationLastMouseAngle));
                        }
                    }
                    break;
            }

            return true;
        }

        /// <returns>If the parent should be redrawn</returns>
        public bool MouseUp()
        {
            GizmoMode oldMode = _mode;
            _mode = GizmoMode.None;
            return oldMode != GizmoMode.None;
        }

        private void CalculateInitialPosition(Vector3 pickPosition)
        { 
            if (_mode == GizmoMode.None) 
                _initialPosition = pickPosition - Position; 
        }

        public PickingResultGizmo DoPicking(Ray ray)
        {
            if (!DrawGizmo)
                return null;

            // Init movement variables
            var pickPos = Vector3.Zero;
            var upsideSize = Orientation == GizmoOrientation.UpsideDown ? -Size : Size;
            var arrowShift = upsideSize * _arrowHeadOffsetMultiplier;
            var scaleShift = upsideSize  / 2.0f;

            // Check for translation
            if (SupportTranslateX)
            {
                BoundingSphere sphereX = new BoundingSphere(Position + Vector3.UnitX * arrowShift, TranslationConeSize / 1.5f);
                if (Collision.RayIntersectsSphere(ray, sphereX, out pickPos))
                {
                    CalculateInitialPosition(pickPos);
                    return new PickingResultGizmo(GizmoMode.TranslateX);
                }
            }
            if (SupportTranslateY)
            {
                BoundingSphere sphereY = new BoundingSphere(Position + Vector3.UnitY * arrowShift, TranslationConeSize / 1.5f);
                if (Collision.RayIntersectsSphere(ray, sphereY, out pickPos))
                {
                    CalculateInitialPosition(pickPos);
                    return new PickingResultGizmo(GizmoMode.TranslateY);
                }
            }
            if (SupportTranslateZ)
            {
                BoundingSphere sphereZ = new BoundingSphere(Position - Vector3.UnitZ * arrowShift, TranslationConeSize / 1.5f);
                if (Collision.RayIntersectsSphere(ray, sphereZ, out pickPos))
                {
                    CalculateInitialPosition(pickPos);
                    return new PickingResultGizmo(GizmoMode.TranslateZ);
                }
            }

            // Check for scale
            if (SupportScale)
            {
                BoundingBox scaleX = new BoundingBox(Position + Vector3.UnitX * scaleShift - new Vector3(ScaleCubeSize / 2.0f),
                                                     Position + Vector3.UnitX * scaleShift + new Vector3(ScaleCubeSize / 2.0f));
                if (Collision.RayIntersectsBox(ray, scaleX, out pickPos))
                {
                    CalculateInitialPosition(pickPos);
                    return new PickingResultGizmo(GizmoMode.ScaleX);
                }

                BoundingBox scaleY = new BoundingBox(Position + Vector3.UnitY * scaleShift - new Vector3(ScaleCubeSize / 2.0f),
                                                     Position + Vector3.UnitY * scaleShift + new Vector3(ScaleCubeSize / 2.0f));
                if (Collision.RayIntersectsBox(ray, scaleY, out pickPos))
                {
                    CalculateInitialPosition(pickPos);
                    return new PickingResultGizmo(GizmoMode.ScaleY);
                }

                BoundingBox scaleZ = new BoundingBox(Position - Vector3.UnitZ * scaleShift - new Vector3(ScaleCubeSize / 2.0f),
                                                     Position - Vector3.UnitZ * scaleShift + new Vector3(ScaleCubeSize / 2.0f));
                if (Collision.RayIntersectsBox(ray, scaleZ, out pickPos))
                {
                    CalculateInitialPosition(pickPos);
                    return new PickingResultGizmo(GizmoMode.ScaleZ);
                }
            }

            // Check for rotation
            float pickRadius = LineThickness / 2 + (Size * 0.045f);

            if (SupportRotationZ)
            {
                Plane planeZ = MathC.CreatePlaneAtPoint(Position, MathC.HomogenousTransform(Vector3.UnitZ, RotateMatrixZ));
                Vector3 intersectionPoint;
                if (Collision.RayIntersectsPlane(ray, planeZ, out intersectionPoint))
                {
                    var distance = (intersectionPoint - Position).Length();
                    if (distance >= Size - pickRadius && distance <= Size + pickRadius)
                    {
                        Vector3 startDirection = Vector3.Normalize(intersectionPoint - Position);

                        float sin = Vector3.Dot(Vector3.UnitY, startDirection);
                        float cos = Vector3.Dot(planeZ.Normal, Vector3.Cross(Vector3.UnitY, startDirection));
                        return new PickingResultGizmo(GizmoMode.RotateZ, (float)Math.Atan2(-sin, cos), distance);
                    }
                }
            }

            if (SupportRotationX)
            {
                Plane planeX = MathC.CreatePlaneAtPoint(Position, MathC.HomogenousTransform(Vector3.UnitX, RotateMatrixX));
                Vector3 intersectionPoint;
                if (Collision.RayIntersectsPlane(ray, planeX, out intersectionPoint))
                {
                    var distance = (intersectionPoint - Position).Length();
                    if (distance >= Size - pickRadius && distance <= Size + pickRadius)
                    {
                        Vector3 startDirection = Vector3.Normalize(intersectionPoint - Position);

                        float sin = Vector3.Dot(Vector3.UnitY, startDirection);
                        float cos = Vector3.Dot(planeX.Normal, Vector3.Cross(Vector3.UnitY, startDirection));
                        return new PickingResultGizmo(GizmoMode.RotateX, (float)Math.Atan2(-sin, cos), distance);
                    }
                }
            }

            if (SupportRotationY)
            {
                Plane planeY = MathC.CreatePlaneAtPoint(Position, MathC.HomogenousTransform(Vector3.UnitY, RotateMatrixY));
                Vector3 intersectionPoint;
                if (Collision.RayIntersectsPlane(ray, planeY, out intersectionPoint))
                {
                    var distance = (intersectionPoint - Position).Length();
                    if (distance >= Size - pickRadius && distance <= Size + pickRadius)
                    {
                        Vector3 startDirection = Vector3.Normalize(intersectionPoint - Position);

                        float sin = Vector3.Dot(Vector3.UnitZ, startDirection);
                        float cos = Vector3.Dot(planeY.Normal, Vector3.Cross(Vector3.UnitZ, startDirection));
                        return new PickingResultGizmo(GizmoMode.RotateY, (float)Math.Atan2(-sin, cos), distance);
                    }
                }
            }

            return null;
        }

        public void ActivateGizmo(PickingResultGizmo pickingResult)
        {
            _mode = pickingResult.Mode;

            _scaleBase = SupportScale ? Scale : Vector3.One;
            _rotationPickAngle = SimplifyAngle(pickingResult.RotationPickAngle);
            _rotationPickAngleOffset = SimplifyAngle(
                (pickingResult.Mode == GizmoMode.RotateY ? RotationY :
                 pickingResult.Mode == GizmoMode.RotateX ? RotationX :
                 pickingResult.Mode == GizmoMode.RotateZ ? RotationZ : 0.0f) - pickingResult.RotationPickAngle);
            _rotationLastMouseAngle = SimplifyAngle(pickingResult.RotationPickAngle);
            _rotationLastMouseRadius = pickingResult.Distance;

            // Freeze rotation matrices at pick time so the rotation plane
            // stays stable for the entire drag, immune to Euler gimbal lock.
            _frozenRotateMatrixY = RotateMatrixY;
            _frozenRotateMatrixX = RotateMatrixX;
            _frozenRotateMatrixZ = RotateMatrixZ;
        }

        /// <returns>If the parent should be redrawn</returns>
        public bool GizmoUpdateHoverEffect(PickingResultGizmo pickingResult)
        {
            GizmoMode oldHoveredMode = _hoveredMode;

            // Disable hover mode
            if (pickingResult == null)
            {
                _hoveredMode = GizmoMode.None;
                return oldHoveredMode != _hoveredMode;
            }

            // Set hover mode
            _hoveredMode = pickingResult.Mode;
            return oldHoveredMode != _hoveredMode;
        }

        private Matrix4x4 RotateMatrixY => Matrix4x4.Identity;
        private Matrix4x4 RotateMatrixX => RotateMatrixY * Matrix4x4.CreateRotationY(SupportRotationY ? RotationY : 0.0f);
        private Matrix4x4 RotateMatrixZ => RotateMatrixX * Matrix4x4.CreateRotationX(SupportRotationX ? RotationX : 0.0f);

        public void Draw(Matrix4x4 viewProjection)
        {
            if (!DrawGizmo)
                return;

            bool upside = Orientation == GizmoOrientation.UpsideDown;

            _device.SetRasterizerState(_device.RasterizerStates.CullNone);

            var solidEffect = _effect;
            GizmoMode highlight = _mode == GizmoMode.None ? _hoveredMode : _mode;

            // Use frozen matrices during an active rotation drag to prevent
            // torus rings from jumping due to Euler gimbal lock.
            bool dragging = _mode == GizmoMode.RotateX || _mode == GizmoMode.RotateY || _mode == GizmoMode.RotateZ;
            Matrix4x4 drawRotateMatrixY = dragging ? _frozenRotateMatrixY : RotateMatrixY;
            Matrix4x4 drawRotateMatrixX = dragging ? _frozenRotateMatrixX : RotateMatrixX;
            Matrix4x4 drawRotateMatrixZ = dragging ? _frozenRotateMatrixZ : RotateMatrixZ;

            // Rotation
            if (SupportRotationX | SupportRotationY | SupportRotationZ)
            {
                // Setup torus model
                float requiredTorusRadius = LineThickness * 0.5f / Size;
                if (_torusRadius != requiredTorusRadius)
                {
                    _torus?.Dispose();
                    _torus = GeometricPrimitive.Torus.New(_device, 1.0f, requiredTorusRadius, 48, _lineRadiusTesselation);
                    _torusRadius = requiredTorusRadius;
                }
                _torus.SetupForRendering(_device);

                // Rotation Y
                if (SupportRotationY)
                {
                    var model = Matrix4x4.CreateScale(Size * 2.0f) *
                        drawRotateMatrixY *
                        Matrix4x4.CreateTranslation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_yAxisColor + (highlight == GizmoMode.RotateY ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _torus.IndexBuffer.ElementCount);
                }

                // Rotation X
                if (SupportRotationX)
                {
                    var model = Matrix4x4.CreateScale(Size * 2.0f) *
                        Matrix4x4.CreateRotationZ((float)Math.PI / 2.0f) *
                        drawRotateMatrixX *
                        Matrix4x4.CreateTranslation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_xAxisColor + (highlight == GizmoMode.RotateX ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _torus.IndexBuffer.ElementCount);
                }

                // Rotation Z
                if (SupportRotationZ)
                {
                    var model = Matrix4x4.CreateScale(Size * 2.0f) *
                        Matrix4x4.CreateRotationX((float)Math.PI / 2.0f) *
                        drawRotateMatrixZ *
                        Matrix4x4.CreateTranslation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_zAxisColor + (highlight == GizmoMode.RotateZ ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, _torus.IndexBuffer.ElementCount);
                }
            }

            // Scale
            if (SupportScale)
            {
                _cylinder.SetupForRendering(_device);

                // X axis
                {
                    var model = Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.5f, 0.0f)) *
                        Matrix4x4.CreateScale(new Vector3(LineThickness * 1.1f, (upside ? -Size : Size) / 2.0f, LineThickness * 1.1f)) *
                        Matrix4x4.CreateRotationZ(-(float)Math.PI / 2.0f) *
                        Matrix4x4.CreateTranslation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_xAxisColor + (highlight == GizmoMode.ScaleX ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                // Y axis
                {
                    var model = Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.5f, 0.0f)) *
                        Matrix4x4.CreateScale(new Vector3(LineThickness * 1.1f, (upside ? -Size : Size) / 2.0f, LineThickness * 1.1f)) *
                        Matrix4x4.CreateTranslation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_yAxisColor + (highlight == GizmoMode.ScaleY ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                // Z axis
                {
                    var model = Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.5f, 0.0f)) *
                        Matrix4x4.CreateScale(new Vector3(LineThickness * 1.1f, (upside ? -Size : Size) / 2.0f, LineThickness * 1.1f)) *
                        Matrix4x4.CreateRotationX(-(float)Math.PI / 2.0f) *
                        Matrix4x4.CreateTranslation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_zAxisColor + (highlight == GizmoMode.ScaleZ ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                _cube.SetupForRendering(_device);

                // X axis scale
                {
                    var model = Matrix4x4.CreateScale(ScaleCubeSize) *
                        Matrix4x4.CreateTranslation(Position + Vector3.UnitX * (upside ? -Size : Size) / 2.0f);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_xAxisColor + (highlight == GizmoMode.ScaleX ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }

                // Y axis scale
                {
                    var model = Matrix4x4.CreateScale(ScaleCubeSize) *
                        Matrix4x4.CreateTranslation(Position + Vector3.UnitY * (upside ? -Size : Size) / 2.0f);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_yAxisColor + (highlight == GizmoMode.ScaleY ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }

                // Z axis scale
                {
                    var model = Matrix4x4.CreateScale(ScaleCubeSize) *
                        Matrix4x4.CreateTranslation(Position - Vector3.UnitZ * (upside ? -Size : Size) / 2.0f);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_zAxisColor + (highlight == GizmoMode.ScaleZ ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }
            }


            // Translation
            {
                _cylinder.SetupForRendering(_device);

                // X axis
                if (SupportTranslateX)
                {
                    var model = Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.5f, 0.0f)) *
                        Matrix4x4.CreateScale(new Vector3(LineThickness, (upside ? -Size : Size) * _arrowHeadOffsetMultiplier, LineThickness)) *
                        Matrix4x4.CreateRotationZ(-(float)Math.PI / 2.0f) *
                        Matrix4x4.CreateTranslation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_xAxisColor + (highlight == GizmoMode.TranslateX ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                // Y axis
                if (SupportTranslateY)
                {
                    var model = Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.5f, 0.0f)) *
                        Matrix4x4.CreateScale(new Vector3(LineThickness, (upside ? -Size : Size) * _arrowHeadOffsetMultiplier, LineThickness)) *
                        Matrix4x4.CreateTranslation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_yAxisColor + (highlight == GizmoMode.TranslateY ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                // Z axis
                if (SupportTranslateZ)
                {
                    var model = Matrix4x4.CreateTranslation(new Vector3(0.0f, 0.5f, 0.0f)) *
                        Matrix4x4.CreateScale(new Vector3(LineThickness, (upside ? -Size : Size) * _arrowHeadOffsetMultiplier, LineThickness)) *
                        Matrix4x4.CreateRotationX(-(float)Math.PI / 2.0f) *
                        Matrix4x4.CreateTranslation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_zAxisColor + (highlight == GizmoMode.TranslateZ ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cylinder.IndexBuffer.ElementCount);
                }

                _cone.SetupForRendering(_device);

                // X axis translation
                if (SupportTranslateX)
                {
                    var model = Matrix4x4.CreateRotationY((float)(upside ? -Math.PI * 1.5f : -Math.PI * 0.5f)) *
                        Matrix4x4.CreateScale(TranslationConeSize) *
                        Matrix4x4.CreateTranslation(Position + (Vector3.UnitX + new Vector3(0.1f, 0, 0)) * ((upside ? -Size : Size) * _arrowHeadOffsetMultiplier));
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_xAxisColor + (highlight == GizmoMode.TranslateX ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }

                // Y axis translation
                if (SupportTranslateY)
                {
                    var model = Matrix4x4.CreateRotationX((float)(upside ? Math.PI * 1.5f : Math.PI * 0.5f)) *
                        Matrix4x4.CreateScale(TranslationConeSize) *
                        Matrix4x4.CreateTranslation(Position + (Vector3.UnitY + new Vector3(0, 0.1f, 0)) * ((upside ? -Size : Size) * _arrowHeadOffsetMultiplier));
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_yAxisColor + (highlight == GizmoMode.TranslateY ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }

                // Z axis translation
                if (SupportTranslateZ)
                {
                    var model = Matrix4x4.CreateRotationY((float)(upside ? Math.PI : 0.0f)) *
                        Matrix4x4.CreateScale(TranslationConeSize) *
                        Matrix4x4.CreateTranslation(Position - (Vector3.UnitZ + new Vector3(0, 0, 0.1f)) * ((upside ? -Size : Size) * _arrowHeadOffsetMultiplier));
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_zAxisColor + (highlight == GizmoMode.TranslateZ ? _hoveredAddition : new Vector4()));
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.DrawIndexed(PrimitiveType.TriangleList, _cone.IndexBuffer.ElementCount);
                }
            }

            // All time geometry
            {
                /*_device.SetVertexBuffer(_cube.VertexBuffer);
                _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _cube.VertexBuffer));
                _device.SetIndexBuffer(_cube.IndexBuffer, _cube.IsIndex32Bits);

                // center cube
                {
                    var model = Matrix4x4.CreateScale(CentreCubeSize) * Matrix4x4.CreateTranslation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(_centerColor);
                    solidEffect.CurrentTechnique.Passes[0].Apply();

                    _device.DrawIndexed(PrimitiveType.TriangleList, _cube.IndexBuffer.ElementCount);
                }*/
            }

            _device.SetRasterizerState(_device.RasterizerStates.CullBack);

            // Rotation display vertices
            switch (_mode)
            {
                case GizmoMode.RotateY:
                case GizmoMode.RotateX:
                case GizmoMode.RotateZ:

                    // Figure out relevant angle
                    float startAngle;
                    float endAngle;
                    float lastMouseAngle;
                    Matrix4x4 baseMatrix;
                    Vector4 color;
                    switch (_mode)
                    {
                        case GizmoMode.RotateY:
                            startAngle = _rotationPickAngle;
                            endAngle = _rotationLastMouseAngle;
                            lastMouseAngle = _rotationLastMouseAngle;
                            baseMatrix = _frozenRotateMatrixY;
                            color = _yAxisColor;
                            break;
                        case GizmoMode.RotateX:
                            startAngle = -((float)Math.PI * 0.5f) - _rotationPickAngle;
                            endAngle = -((float)Math.PI * 0.5f) - _rotationLastMouseAngle;
                            lastMouseAngle = -((float)Math.PI * 0.5f) - _rotationLastMouseAngle;
                            baseMatrix = Matrix4x4.CreateRotationZ((float)Math.PI / 2.0f) * _frozenRotateMatrixX;
                            color = _xAxisColor;
                            break;
                        case GizmoMode.RotateZ:
                            startAngle = (float)Math.PI + _rotationPickAngle;
                            endAngle = (float)Math.PI + _rotationLastMouseAngle;
                            lastMouseAngle = (float)Math.PI + _rotationLastMouseAngle;
                            baseMatrix = Matrix4x4.CreateRotationX((float)Math.PI / 2.0f) * _frozenRotateMatrixZ;
                            color = _zAxisColor;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    { // Choose shortest path
                        float shortestAngle = endAngle - startAngle;
                        shortestAngle = (float)(shortestAngle - Math.Round(shortestAngle / (Math.PI * 2)) * (Math.PI * 2));
                        endAngle = startAngle + shortestAngle;
                    }
                    if (startAngle > endAngle)
                    {
                        float temp = startAngle;
                        startAngle = endAngle;
                        endAngle = temp;
                    }

                    // Build rotation geometry
                    var rotationHelperGeometry = new SolidVertex[_rotationTrianglesCount * 3 + 2];
                    float angleStep = (endAngle - startAngle) / _rotationTrianglesCount;
                    var middleVertex = new SolidVertex(new Vector3());
                    var lastVertex = new SolidVertex(new Vector3((float)Math.Cos(startAngle), 0, (float)-Math.Sin(startAngle)));
                    for (int i = 0; i < _rotationTrianglesCount; ++i)
                    {
                        float currentAngle = startAngle + (i + 1) * angleStep;
                        var currentVertex = new SolidVertex(new Vector3((float)Math.Cos(currentAngle), 0, (float)-Math.Sin(currentAngle)));
                        rotationHelperGeometry[i * 3 + 0] = middleVertex;
                        rotationHelperGeometry[i * 3 + 1] = lastVertex;
                        rotationHelperGeometry[i * 3 + 2] = currentVertex;
                        lastVertex = currentVertex;
                    }

                    rotationHelperGeometry[_rotationTrianglesCount * 3] = new SolidVertex(new Vector3(
                        _rotationLastMouseRadius / Size * (float)Math.Cos(lastMouseAngle), 0,
                        _rotationLastMouseRadius / Size * (float)-Math.Sin(lastMouseAngle)));
                    rotationHelperGeometry[_rotationTrianglesCount * 3 + 1] = middleVertex;
                    _rotationHelperGeometry.SetData(rotationHelperGeometry);

                    // Draw
                    _device.SetRasterizerState(_device.RasterizerStates.CullNone);
                    _device.SetVertexBuffer(_rotationHelperGeometry);
                    _device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, _rotationHelperGeometry));
                    var model = Matrix4x4.CreateScale(Size) * baseMatrix * Matrix4x4.CreateTranslation(Position);
                    solidEffect.Parameters["ModelViewProjection"].SetValue((model * viewProjection).ToSharpDX());
                    solidEffect.Parameters["Color"].SetValue(color * _rotationAlpha);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.Draw(PrimitiveType.TriangleList, _rotationTrianglesCount * 3);

                    solidEffect.Parameters["Color"].SetValue(Vector4.One);
                    solidEffect.CurrentTechnique.Passes[0].Apply();
                    _device.SetRasterizerState(_rasterizerWireframe);
                    _device.Draw(PrimitiveType.LineList, 2, _rotationTrianglesCount * 3);

                    // FIXME: Temporary fix for text corruption problem! Remove when SharpDX is not used.
                    _device.SetRasterizerState(_device.RasterizerStates.Default);
                    break;
            }
        }

        // They are called both, just leave the implementation empty if not needed, don't use exceptions
        protected abstract void GizmoMove(Vector3 newPos);
        protected abstract void GizmoMoveDelta(Vector3 delta);
        protected abstract void GizmoScaleY(float newScale);
        protected abstract void GizmoScaleX(float newScale);
        protected abstract void GizmoScaleZ(float newScale);
        protected abstract void GizmoRotateY(float newAngle);
        protected abstract void GizmoRotateX(float newAngle);
        protected abstract void GizmoRotateZ(float newAngle);

        protected abstract Vector3 Position { get; }
        protected abstract float RotationY { get; }
        protected abstract float RotationX { get; }
        protected abstract float RotationZ { get; }
        protected abstract Vector3 Scale { get; }
        protected abstract float CentreCubeSize { get; }
        protected abstract float TranslationConeSize { get; }
        protected abstract float ScaleCubeSize { get; }
        protected abstract float Size { get; }
        protected abstract float LineThickness { get; }
        protected abstract GizmoOrientation Orientation { get; }
        protected abstract bool SupportTranslateX { get; }
        protected abstract bool SupportTranslateY { get; }
        protected abstract bool SupportTranslateZ { get; }
        protected abstract bool SupportScale { get; }
        protected abstract bool SupportRotationY { get; }
        protected abstract bool SupportRotationX { get; }
        protected abstract bool SupportRotationZ { get; }
        protected virtual bool DrawGizmo => SupportTranslateX || SupportTranslateY || SupportTranslateZ || SupportScale || SupportRotationY || SupportRotationX || SupportRotationZ;
    }
}
