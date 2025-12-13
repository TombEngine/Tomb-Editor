using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Wad.Catalog;
using TombLib.Wad;
using TombLib.Utils;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorEnums.Extensions;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private class PickingResultSector : PickingResult
        {
            public float VerticalCoord { get; set; }
            public VectorInt2 Pos { get; set; }
            public Room Room { get; set; }
            public SectorFace Face { get; set; }

            public bool IsFloorHorizontalPlane => Face.IsFloor();
            public bool IsCeilingHorizontalPlane => Face.IsCeiling();
            public bool IsVerticalPlane => !IsFloorHorizontalPlane && !IsCeilingHorizontalPlane;
            public bool BelongsToFloor => IsFloorHorizontalPlane || Face.IsFloorWall();
            public bool BelongsToCeiling => IsCeilingHorizontalPlane || Face.IsCeilingWall();

            public PickingResultSector(float distance, float verticalCoord, VectorInt2 pos, Room room, SectorFace face)
            {
                Distance = distance;
                VerticalCoord = verticalCoord;
                Pos = pos;
                Room = room;
                Face = face;
            }
        }

        private class PickingResultObject : PickingResult
        {
            public ObjectInstance ObjectInstance { get; set; }
            public PickingResultObject(float distance, ObjectInstance objectPtr)
            {
                Distance = distance;
                ObjectInstance = objectPtr;
            }
        }

        private Ray GetRay(float x, float y)
        {
            return Ray.GetPickRay(new Vector2(x, y), _viewProjection, ClientSize.Width, ClientSize.Height);
        }

        private float TransformRayDistance(ref Ray sourceRay, ref Matrix4x4 transform, ref Ray destinationRay, float sourceDistance)
        {
            Vector3 sourcePos = sourceRay.Position + sourceDistance * sourceRay.Direction;
            Vector3 destinationPos = MathC.HomogenousTransform(sourcePos, transform);
            float destinationDistance = (destinationPos - destinationRay.Position).Length();
            return destinationDistance;
        }

        private void DoMeshPicking<T>(ref PickingResult result, Ray ray, ObjectInstance objectPtr, Mesh<T> mesh, Matrix4x4 objectMatrix) where T : struct, IVertex
        {
            // Transform view ray to object space space
            Matrix4x4 inverseObjectMatrix;
            if (!Matrix4x4.Invert(objectMatrix, out inverseObjectMatrix))
                return;
            Vector3 transformedRayPos = MathC.HomogenousTransform(ray.Position, inverseObjectMatrix);
            Vector3 transformedRayDestination = MathC.HomogenousTransform(ray.Position + ray.Direction, inverseObjectMatrix);
            Ray transformedRay = new Ray(transformedRayPos, transformedRayDestination - transformedRayPos);
            transformedRay.Direction = Vector3.Normalize(transformedRay.Direction);

            // Do a fast bounding box check
            float minDistance;
            {
                BoundingBox box = mesh.BoundingBox;
                float distance;
                if (!Collision.RayIntersectsBox(transformedRay, box, out distance))
                    return;

                minDistance = result == null ? float.PositiveInfinity : TransformRayDistance(ref ray, ref inverseObjectMatrix, ref transformedRay, result.Distance);
                if (!(distance < minDistance))
                    return;
            }

            // Now do a ray - triangle intersection test
            bool hit = false;
            foreach (var submesh in mesh.Submeshes)
                for (int k = 0; k < submesh.Value.Indices.Count; k += 3)
                {
                    Vector3 p1 = mesh.Vertices[submesh.Value.Indices[k]].Position;
                    Vector3 p2 = mesh.Vertices[submesh.Value.Indices[k + 1]].Position;
                    Vector3 p3 = mesh.Vertices[submesh.Value.Indices[k + 2]].Position;

                    float distance;
                    if (Collision.RayIntersectsTriangle(transformedRay, p1, p2, p3, true, out distance) && distance < minDistance)
                    {
                        minDistance = distance;
                        hit = true;
                    }
                }

            if (hit)
                result = new PickingResultObject(TransformRayDistance(ref transformedRay, ref objectMatrix, ref ray, minDistance), objectPtr);
        }

        private PickingResult DoPicking(Ray ray, bool pickAnyRoom = false)
        {
            // The gizmo has the priority because it always drawn on top
            PickingResult result = _gizmo.DoPicking(ray);
            if (result != null)
                return result;

            List<Room> rooms = pickAnyRoom ? CollectRoomsToDraw(_editor.SelectedRoom) : new List<Room> { _editor.SelectedRoom };

            foreach (var room in rooms)
            {
                float distance;

                // First check for all objects in the room
                foreach (var instance in room.Objects)
                    if (instance is MoveableInstance)
                    {
                        if (ShowMoveables)
                        {
                            var modelInfo = (MoveableInstance)instance;
                            var moveable = _editor?.Level?.Settings?.WadTryGetMoveable(modelInfo.WadObjectId);
                            if (moveable != null)
                            {
                                // TODO Make picking independent of the rendering data.
                                var model = _wadRenderer.GetMoveable(moveable);
                                var skin = model;
                                if (moveable.Id == WadMoveableId.Lara)
                                {
                                    var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(_editor.Level.Settings.GameVersion, moveable.Id.TypeId));
                                    var moveableSkin = _editor.Level.Settings.WadTryGetMoveable(skinId);
                                    if (moveableSkin != null && moveableSkin.Meshes.Count == model.Meshes.Count)
                                        skin = _wadRenderer.GetMoveable(moveableSkin);
                                }

                                for (int j = 0; j < model.Meshes.Count; j++)
                                {
                                    var mesh = skin.Meshes[j];
                                    DoMeshPicking(ref result, ray, instance, mesh, model.AnimationTransforms[j] * instance.ObjectMatrix);
                                }
                            }
                            else
                                result = TryPickServiceObject(instance, ray, result, out distance);
                        }
                    }
                    else if (instance is StaticInstance)
                    {
                        if (ShowStatics)
                        {
                            StaticInstance modelInfo = (StaticInstance)instance;
                            WadStatic @static = _editor?.Level?.Settings?.WadTryGetStatic(modelInfo.WadObjectId);
                            if (@static != null)
                            {
                                // TODO Make picking independent of the rendering data.
                                StaticModel model = _wadRenderer.GetStatic(@static);
                                var mesh = model.Meshes[0];
                                DoMeshPicking(ref result, ray, instance, mesh, instance.ObjectMatrix);
                            }
                            else
                                result = TryPickServiceObject(instance, ray, result, out distance);
                        }
                    }
                    else if (instance is ImportedGeometryInstance)
                    {
                        if (ShowImportedGeometry && !DisablePickingForImportedGeometry)
                        {
                            var geometry = (ImportedGeometryInstance)instance;
                            if (geometry.Hidden || !(geometry?.Model?.DirectXModel?.Meshes.Count > 0))
                                result = TryPickServiceObject(instance, ray, result, out distance);
                            else
                                foreach (ImportedGeometryMesh mesh in geometry?.Model?.DirectXModel?.Meshes ?? Enumerable.Empty<ImportedGeometryMesh>())
                                    DoMeshPicking(ref result, ray, instance, mesh, geometry.ObjectMatrix);
                        }
                    }
                    else if (instance is VolumeInstance)
                    {
                        if (ShowVolumes)
                            result = TryPickServiceObject(instance, ray, result, out distance);
                    }
                    else if (ShowOtherObjects)
                        result = TryPickServiceObject(instance, ray, result, out distance);

                if (ShowGhostBlocks)
                    foreach (var ghost in room.GhostBlocks)
                    {
                        if (_editor.SelectedObject == ghost)
                        {
                            for (int f = 0; f < 2; f++)
                            {
                                bool floor = f == 0;
                                var pos = ghost.ControlPositions(floor);

                                for (int i = 0; i < 4; i++)
                                {
                                    BoundingBox nodeBox = new BoundingBox(
                                        pos[i] - new Vector3(_littleCubeRadius),
                                        pos[i] + new Vector3(_littleCubeRadius));

                                    if (Collision.RayIntersectsBox(ray, nodeBox, out distance) && (result == null || distance < result.Distance))
                                    {
                                        ghost.SelectedFloor = floor;
                                        switch (i)
                                        {
                                            case 0: ghost.SelectedCorner = SectorEdge.XnZp; break;
                                            case 1: ghost.SelectedCorner = SectorEdge.XpZp; break;
                                            case 2: ghost.SelectedCorner = SectorEdge.XpZn; break;
                                            case 3: ghost.SelectedCorner = SectorEdge.XnZn; break;
                                        }

                                        result = new PickingResultObject(distance, ghost);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // FIXME: For now, ghost blocks don't differentiate sprite mode and 3D mode picking.
                            // It may need a huge refactoring. Until now, there could be misfiring on higher FOVs.

                            BoundingBox box = new BoundingBox(
                                ghost.Center(true) - new Vector3(_littleCubeRadius),
                                ghost.Center(true) + new Vector3(_littleCubeRadius));

                            if (Collision.RayIntersectsBox(ray, box, out distance) && (result == null || distance < result.Distance))
                            {
                                result = new PickingResultObject(distance, ghost);
                                ghost.SelectedCorner = null;
                            }
                        }
                    }

                // Pick hidden rooms only for place action, if they are not selected or if global picking setting is off.

                if (!DisablePickingForHiddenRooms ||
                    !room.Properties.Hidden || room != _editor.SelectedRoom || _editor.Action is IEditorActionPlace)
                {
                    // Check room geometry
                    var roomIntersectInfo = room.RoomGeometry?.RayIntersectsGeometry(new Ray(ray.Position - room.WorldPos, ray.Direction));
                    if (roomIntersectInfo != null && (result == null || roomIntersectInfo.Value.Distance < result.Distance))
                        result = new PickingResultSector(roomIntersectInfo.Value.Distance, roomIntersectInfo.Value.VerticalCoord, roomIntersectInfo.Value.Pos, room, roomIntersectInfo.Value.Face);
                }
            }

            return result;
        }

        private PickingResult TryPickServiceObject(PositionBasedObjectInstance instance, Ray ray, PickingResult result, out float distance)
        {
            if (_editor.Configuration.Rendering3D_UseSpritesForServiceObjects || instance is SpriteInstance)
            {
                RectangleInt2 bounds;

                if (instance is SpriteInstance && _editor.Level.Settings.GameVersion.Native() < TRVersion.Game.TR3)
                {
                    var sprite = instance as SpriteInstance;
                    var sequence = _editor.Level.Settings.WadGetAllSpriteSequences()
                        .FirstOrDefault(s => s.Key.TypeId == sprite.Sequence && s.Value.Sprites.Count > sprite.Frame).Value;
                    if (sequence != null)
                        bounds = sequence.Sprites[sprite.Frame].Alignment;
                    else
                        bounds = ServiceObjectTextures.GetBounds(instance);
                }
                else
                    bounds = ServiceObjectTextures.GetBounds(instance);

                var matrix = Matrix4x4.CreateTranslation(ray.Position) * _viewProjection;
                var rayPos = matrix.TransformPerspectively(new Vector3()).To2();

                float dist;
                var rect = instance.GetViewportRect(bounds, Camera.GetPosition(), _viewProjection, ClientSize, out dist);
                distance = Vector3.Distance(Camera.GetPosition(), instance.Position + instance.Room.WorldPos);

                // dist < 1.0f discards offscreen sprites which may occasionally pop up from other side
                // due to sign overflow.

                if (dist < 1.0f && rect.Contains(rayPos) && (result == null || distance < result.Distance))
                    return new PickingResultObject(distance, instance);
            }
            else if (instance is LightInstance)
            {
                BoundingSphere sphere = new BoundingSphere(instance.Room.WorldPos + instance.Position, _littleSphereRadius);

                if (Collision.RayIntersectsSphere(ray, sphere, out distance) && (result == null || distance < result.Distance))
                    return new PickingResultObject(distance, instance);
            }
            else
            {
                BoundingBox box = new BoundingBox(instance.Room.WorldPos + instance.Position - new Vector3(_littleCubeRadius),
                                                  instance.Room.WorldPos + instance.Position + new Vector3(_littleCubeRadius));

                if (Collision.RayIntersectsBox(ray, box, out distance) && (result == null || distance < result.Distance))
                    return new PickingResultObject(distance, instance);
            }

            return result;
        }
    }
}
