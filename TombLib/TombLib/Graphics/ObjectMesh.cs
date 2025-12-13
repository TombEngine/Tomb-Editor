using NLog;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.Utils;
using TombLib.Wad;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;

namespace TombLib.Graphics
{
    public class ObjectMesh : Mesh<ObjectVertex>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public ObjectMesh(GraphicsDevice device, string name)
            : base(device, name)
        { }

        public void UpdateBuffers(Vector3? position = null)
        {
            if (Vertices.Count == 0)
                return;

            DepthSort(position);
            UpdateBoundingBox();

            if (VertexBuffer != null)
                VertexBuffer.Dispose();
            if (IndexBuffer != null)
                IndexBuffer.Dispose();

            VertexBuffer = Buffer.Vertex.New(GraphicsDevice, Vertices.ToArray<ObjectVertex>(), SharpDX.Direct3D11.ResourceUsage.Immutable);
            InputLayout  = VertexInputLayout.FromBuffer(0, VertexBuffer);
            IndexBuffer  = Buffer.Index.New(GraphicsDevice, Indices.ToArray(), SharpDX.Direct3D11.ResourceUsage.Immutable);
            if (VertexBuffer == null)
                logger.Error("Vertex Buffer of Mesh " + Name + " could not be created!");
            if (InputLayout == null)
                logger.Error("Input Layout of Mesh " + Name + " could not be created!");
            if (IndexBuffer == null)
                logger.Error("Index Buffer of Mesh " + Name + " could not be created!");
        }

        private static void PutObjectVertexAndIndex(Vector3 v, Vector3 n, VertexWeight w,
                                                    ObjectMesh mesh, Submesh submesh, Vector2 pixelCoord, int submeshIndex,
                                                    Vector3 color, in WadRenderer.AllocationResult allocation)
        {
            var uFactor = allocation.AllocatedSize.X / (float)allocation.OriginalSize.X;
            var vFactor = allocation.AllocatedSize.Y / (float)allocation.OriginalSize.Y;
            var newVertex = new ObjectVertex();

            newVertex.Position = new Vector3(v.X, v.Y, v.Z);
            newVertex.UVW = new Vector3(((allocation.Position.X + (pixelCoord.X * uFactor)) ) / allocation.AtlasDimension.X ,
                                       ((allocation.Position.Y + (pixelCoord.Y * vFactor)) ) / allocation.AtlasDimension.Y ,
                                       allocation.Position.Z);
            newVertex.Normal = n / n.Length();
            newVertex.Color = color;

            newVertex.Indices = new Vector4(w.Index[0], w.Index[1], w.Index[2], w.Index[3]);
            newVertex.Weights = new Vector4(w.Weight[0], w.Weight[1], w.Weight[2], w.Weight[3]);

            mesh.Vertices.Add(newVertex);
            submesh.Indices.Add(mesh.Vertices.Count - 1);
        }

        public static ObjectMesh FromWad2(GraphicsDevice device, WadMesh msh, Func<WadTexture, WadRenderer.AllocationResult> allocateTexture, bool correct)
        {
            // Initialize the mesh
            var mesh = new ObjectMesh(device, msh.Name);

            // Prepare materials
            var materialOpaque = new Material(Material.Material_Opaque + "_0_0_0_0", null, false, false, 0);
            var materialOpaqueDoubleSided = new Material(Material.Material_OpaqueDoubleSided + "_0_0_1_0", null, false, true, 0);
            var materialAdditiveBlending = new Material(Material.Material_AdditiveBlending + "_0_1_0_0", null, true, false, 0);
            var materialAdditiveBlendingDoubleSided = new Material(Material.Material_AdditiveBlendingDoubleSided + "_0_1_1_0", null, true, true, 0);

            mesh.Materials = new List<Material>();
            mesh.Materials.Add(materialOpaque);
            mesh.Materials.Add(materialOpaqueDoubleSided);
            mesh.Materials.Add(materialAdditiveBlending);
            mesh.Materials.Add(materialAdditiveBlendingDoubleSided);

            mesh.Submeshes.Add(materialOpaque, new Submesh(materialOpaque));
            mesh.Submeshes.Add(materialOpaqueDoubleSided, new Submesh(materialOpaqueDoubleSided));
            mesh.Submeshes.Add(materialAdditiveBlending, new Submesh(materialAdditiveBlending));
            mesh.Submeshes.Add(materialAdditiveBlendingDoubleSided, new Submesh(materialAdditiveBlendingDoubleSided));

            mesh.BoundingBox = msh.BoundingBox;
            mesh.BoundingSphere = msh.BoundingSphere;
            mesh.Hidden = msh.Hidden;

            // For some reason, wad meshes sometimes may have position count desynced from color count, so we check that too.
            var hasShades = msh.VertexColors.Count != 0 && msh.VertexPositions.Count == msh.VertexColors.Count;
            var hasWeights = msh.VertexWeights.Count != 0 && msh.VertexPositions.Count == msh.VertexWeights.Count;

            for (int j = 0; j < msh.Polys.Count; j++)
            {
                WadPolygon poly = msh.Polys[j];
                WadRenderer.AllocationResult positionInPackedTexture = allocateTexture((WadTexture)poly.Texture.Texture);

                // Get the right submesh
                var submesh = mesh.Submeshes[materialOpaque];
                if (poly.Texture.BlendMode == BlendMode.Additive)
                {
                    if (poly.Texture.DoubleSided)
                        submesh = mesh.Submeshes[materialAdditiveBlendingDoubleSided];
                    else
                        submesh = mesh.Submeshes[materialAdditiveBlending];
                }
                else
                {
                    if (poly.Texture.DoubleSided)
                        submesh = mesh.Submeshes[materialOpaqueDoubleSided];
                }

                // Do half-pixel correction on texture to prevent bleeding.
                // Chop 0.15th of a pixel even if non-corrected mode to prevent issues with AA.
                var coords = poly.CorrectTexCoords(correct ? 0.5f : 0.15f);

                if (poly.IsTriangle)
                {
                    int v1 = poly.Index0;
                    int v2 = poly.Index1;
                    int v3 = poly.Index2;

                    PutObjectVertexAndIndex(msh.VertexPositions[v1], msh.VertexNormals[v1], hasWeights ? msh.VertexWeights[v1] : new VertexWeight(), mesh, submesh,
                                            coords[0], 0, (hasShades ? msh.VertexColors[v1] : Vector3.One),
                                            positionInPackedTexture);
                    PutObjectVertexAndIndex(msh.VertexPositions[v2], msh.VertexNormals[v2], hasWeights ? msh.VertexWeights[v2] : new VertexWeight(), mesh, submesh,
                                            coords[1], 0, (hasShades ? msh.VertexColors[v2] : Vector3.One),
                                            positionInPackedTexture);
                    PutObjectVertexAndIndex(msh.VertexPositions[v3], msh.VertexNormals[v3], hasWeights ? msh.VertexWeights[v3] : new VertexWeight(), mesh, submesh,
                                            coords[2], 0, (hasShades ? msh.VertexColors[v3] : Vector3.One),
                                            positionInPackedTexture);
                }
                else
                {
                    int v1 = poly.Index0;
                    int v2 = poly.Index1;
                    int v3 = poly.Index2;
                    int v4 = poly.Index3;

                    PutObjectVertexAndIndex(msh.VertexPositions[v1], msh.VertexNormals[v1], hasWeights ? msh.VertexWeights[v1] : new VertexWeight(), mesh, submesh,
                                            coords[0], 0, (hasShades ? msh.VertexColors[v1] : Vector3.One),
                                            positionInPackedTexture);
                    PutObjectVertexAndIndex(msh.VertexPositions[v2], msh.VertexNormals[v2], hasWeights ? msh.VertexWeights[v2] : new VertexWeight(), mesh, submesh,
                                            coords[1], 0, (hasShades ? msh.VertexColors[v2] : Vector3.One),
                                            positionInPackedTexture);
                    PutObjectVertexAndIndex(msh.VertexPositions[v4], msh.VertexNormals[v4], hasWeights ? msh.VertexWeights[v4] : new VertexWeight(), mesh, submesh,
                                            coords[3], 0, (hasShades ? msh.VertexColors[v4] : Vector3.One),
                                            positionInPackedTexture);

                    PutObjectVertexAndIndex(msh.VertexPositions[v4], msh.VertexNormals[v4], hasWeights ? msh.VertexWeights[v4] : new VertexWeight(), mesh, submesh,
                                            coords[3], 0, (hasShades ? msh.VertexColors[v4] : Vector3.One),
                                            positionInPackedTexture);
                    PutObjectVertexAndIndex(msh.VertexPositions[v2], msh.VertexNormals[v2], hasWeights ? msh.VertexWeights[v2] : new VertexWeight(), mesh, submesh,
                                            coords[1], 0, (hasShades ? msh.VertexColors[v2] : Vector3.One),
                                            positionInPackedTexture);
                    PutObjectVertexAndIndex(msh.VertexPositions[v3], msh.VertexNormals[v3], hasWeights ? msh.VertexWeights[v3] : new VertexWeight(), mesh, submesh,
                                            coords[2], 0, (hasShades ? msh.VertexColors[v3] : Vector3.One),
                                            positionInPackedTexture);
                }
            }

            mesh.UpdateBuffers();
            
            return mesh;
        }
    }
}
