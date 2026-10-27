using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.Controls
{
    public static class WadObjectRenderHelper
    {
        /// <summary>
        /// Applies optional skin substitute for moveables that need it.
        /// If the object is a WadMoveable and has a skin defined in TrCatalog,
        /// replaces dummy meshes with the skin's meshes.
        /// Returns the original object unchanged for non-moveables or when no skin is found.
        /// </summary>
        public static IWadObject GetRenderObject(IWadObject wadObject, LevelSettings settings)
        {
            if (wadObject is WadMoveable moveable)
            {
                var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(settings.GameVersion, moveable.Id.TypeId));
                var skin = settings.WadTryGetMoveable(skinId);

                if (skin != null && skin != moveable)
                    return moveable.ReplaceDummyMeshes(skin);
            }
            return wadObject;
        }

        /// <summary>
        /// Computes a bounding sphere for the given wad object, suitable for camera framing.
        /// </summary>
        public static BoundingSphere ComputeBoundingSphere(IWadObject wadObject, WadRenderer wadRenderer)
        {
            var bs = new BoundingSphere(new Vector3(0.0f, 256.0f, 0.0f), 640.0f);

            if (wadObject is WadMoveable moveable)
            {
                if (moveable.Meshes.Count == 0 || (moveable.Meshes.Count == 1 && moveable.Meshes[0] == null))
                    return bs;

                var model = wadRenderer.GetMoveable(moveable);
                if (model.Animations.Count > 0 && model.Animations[0].KeyFrames.Count > 0)
                {
                    model.UpdateAnimation(0, 0);
                    var bb = model.Animations[0].KeyFrames[0].CalculateBoundingBox(model, model);
                    bs = BoundingSphere.FromBoundingBox(bb);
                }
            }
            else if (wadObject is WadStatic staticObj)
            {
                if (staticObj.Mesh != null)
                    bs = staticObj.Mesh.CalculateBoundingSphere();
            }
            else if (wadObject is ImportedGeometry impGeo)
            {
                if (impGeo.DirectXModel != null && impGeo.DirectXModel.Meshes != null)
                {
                    var bb = new BoundingBox();
                    foreach (var mesh in impGeo.DirectXModel.Meshes)
                        bb = bb.Union(mesh.BoundingBox);
                    bs = BoundingSphere.FromBoundingBox(bb);
                }
            }

            return bs;
        }

        /// <summary>
        /// Creates a camera positioned to frame the given WAD object.
        /// Returns null if the object type is unsupported or has no renderable content.
        /// </summary>
        public static ArcBallCamera CreateCameraForObject(IWadObject wadObject, WadRenderer wadRenderer, float fieldOfView)
        {
            if (wadObject is WadMoveable moveable)
            {
                if (moveable.Meshes.Count == 0 || (moveable.Meshes.Count == 1 && moveable.Meshes[0] == null))
                    return null;
            }
            else if (wadObject is WadStatic staticObj)
            {
                if (staticObj.Mesh == null || staticObj.Mesh.VertexPositions.Count == 0)
                    return null;
            }
            else if (!(wadObject is WadStatic) && !(wadObject is ImportedGeometry))
            {
                return null;
            }

            var bs = ComputeBoundingSphere(wadObject, wadRenderer);
            var center = bs.Center;
            var radius = bs.Radius * 1.15f;

            return new ArcBallCamera(center, MathC.DegToRad(35), MathC.DegToRad(35),
                -(float)Math.PI / 2, (float)Math.PI / 2, radius * 3, 50, 1000000, fieldOfView * (float)(Math.PI / 180));
        }

        public static void RenderObject(IWadObject wadObject, WadRenderer wadRenderer,
            GraphicsDevice legacyDevice, Matrix4x4 viewProjection, Vector3 cameraPosition, bool drawTransparency)
        {
            if (wadObject is WadMoveable moveable)
                RenderMoveable(moveable, wadRenderer, legacyDevice, viewProjection, cameraPosition, drawTransparency);
            else if (wadObject is WadStatic staticObj)
                RenderStatic(staticObj, wadRenderer, legacyDevice, viewProjection, cameraPosition, drawTransparency);
            else if (wadObject is ImportedGeometry impGeo)
                RenderImportedGeometry(impGeo, legacyDevice, viewProjection, cameraPosition, drawTransparency);
        }

        public static void RenderMoveable(WadMoveable moveable, WadRenderer wadRenderer,
            GraphicsDevice legacyDevice, Matrix4x4 viewProjection, Vector3 cameraPosition, bool drawTransparency)
        {
            if (moveable.Meshes.Count == 0 || (moveable.Meshes.Count == 1 && moveable.Meshes[0] == null))
                return;

            var model = wadRenderer.GetMoveable(moveable);
            model.UpdateAnimation(0, 0);

            var effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];

            effect.Parameters["AlphaTest"].SetValue(drawTransparency);
            effect.Parameters["Color"].SetValue(Vector4.One);
            effect.Parameters["StaticLighting"].SetValue(false);
            effect.Parameters["ColoredVertices"].SetValue(false);
            effect.Parameters["Texture"].SetResource(wadRenderer.Texture);
            effect.Parameters["TextureSampler"].SetResource(legacyDevice.SamplerStates.Default);

            var matrices = new List<Matrix4x4>();
            if (model.Animations.Count != 0)
            {
                for (var b = 0; b < model.Meshes.Count; b++)
                    matrices.Add(model.AnimationTransforms[b]);
            }
            else
            {
                foreach (var bone in model.Bones)
                    matrices.Add(bone.GlobalTransform);
            }

            if (model.Skin != null)
                model.RenderSkin(legacyDevice, effect, viewProjection.ToSharpDX());

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];
                if (mesh.Vertices.Count == 0)
                    continue;

                if (model.Skin != null && mesh.Hidden)
                    continue;

                mesh.UpdateBuffers(cameraPosition);

                legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);
                legacyDevice.SetVertexInputLayout(mesh.InputLayout);

                effect.Parameters["ModelViewProjection"].SetValue((matrices[i] * viewProjection).ToSharpDX());
                effect.Techniques[0].Passes[0].Apply();

                foreach (var submesh in mesh.Submeshes)
                {
                    submesh.Value.Material.SetStates(legacyDevice, drawTransparency);
                    legacyDevice.Draw(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                }
            }
        }

        public static void RenderStatic(WadStatic staticObj, WadRenderer wadRenderer,
            GraphicsDevice legacyDevice, Matrix4x4 viewProjection, Vector3 cameraPosition, bool drawTransparency)
        {
            var model = wadRenderer.GetStatic(staticObj);

            var effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["Model"];

            effect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
            effect.Parameters["AlphaTest"].SetValue(drawTransparency);
            effect.Parameters["Color"].SetValue(Vector4.One);
            effect.Parameters["StaticLighting"].SetValue(false);
            effect.Parameters["ColoredVertices"].SetValue(false);
            effect.Parameters["Texture"].SetResource(wadRenderer.Texture);
            effect.Parameters["TextureSampler"].SetResource(legacyDevice.SamplerStates.Default);

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];
                if (mesh.Vertices.Count == 0)
                    continue;

                mesh.UpdateBuffers(cameraPosition);

                legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);
                legacyDevice.SetVertexInputLayout(mesh.InputLayout);

                effect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());
                effect.Techniques[0].Passes[0].Apply();

                foreach (var submesh in mesh.Submeshes)
                {
                    submesh.Value.Material.SetStates(legacyDevice, drawTransparency);
                    legacyDevice.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                }
            }
        }

        public static void RenderImportedGeometry(ImportedGeometry geo,
            GraphicsDevice legacyDevice, Matrix4x4 viewProjection, Vector3 cameraPosition, bool drawTransparency)
        {
            var model = geo.DirectXModel;
            if (model == null || model.Meshes == null || model.Meshes.Count == 0)
                return;

            var effect = DeviceManager.DefaultDeviceManager.___LegacyEffects["RoomGeometry"];

            effect.Parameters["UseVertexColors"].SetValue(true);
            effect.Parameters["AlphaTest"].SetValue(drawTransparency);
            effect.Parameters["Color"].SetValue(Vector4.One);
            effect.Parameters["TextureSampler"].SetResource(legacyDevice.SamplerStates.AnisotropicWrap);

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];
                if (mesh.Vertices.Count == 0)
                    continue;

                mesh.UpdateBuffers(cameraPosition);

                legacyDevice.SetVertexBuffer(0, mesh.VertexBuffer);
                legacyDevice.SetIndexBuffer(mesh.IndexBuffer, true);
                legacyDevice.SetVertexInputLayout(mesh.InputLayout);

                effect.Parameters["ModelViewProjection"].SetValue(viewProjection.ToSharpDX());

                foreach (var submesh in mesh.Submeshes)
                {
                    var texture = submesh.Value.Material.Texture;
                    if (texture != null && texture is ImportedGeometryTexture)
                    {
                        effect.Parameters["TextureEnabled"].SetValue(true);
                        effect.Parameters["Texture"].SetResource(((ImportedGeometryTexture)texture).DirectXTexture);
                        effect.Parameters["ReciprocalTextureSize"].SetValue(new Vector2(1.0f / texture.Image.Width, 1.0f / texture.Image.Height));
                    }
                    else
                        effect.Parameters["TextureEnabled"].SetValue(false);

                    effect.Techniques[0].Passes[0].Apply();
                    submesh.Value.Material.SetStates(legacyDevice, drawTransparency);
                    legacyDevice.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
                }
            }
        }
    }
}
