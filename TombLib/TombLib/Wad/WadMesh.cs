using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using TombLib.GeometryIO;
using TombLib.IO;
using TombLib.Utils;
using System.Linq;
using TombLib.GeometryIO.Importers;
using NLog;
using TombLib.Graphics;

namespace TombLib.Wad
{
    public struct VertexAttributes
    {
        public int Glow;
        public int Move;
    }

    public struct VertexWeight
    {
        public int[] Index;
        public float[] Weight;

        public VertexWeight()
        {
            Index = new int[4];
            Weight = new float[4];
        }

        public bool Valid()
        {
            return Weight[0] > 0.0f || Weight[1] > 0.0f ||
                   Weight[2] > 0.0f || Weight[3] > 0.0f;
        }

        public static bool operator ==(VertexWeight a, VertexWeight b)
        {
            for (int i = 0; i < 4; i++)
            {
                if (a.Index[i] != b.Index[i] || a.Weight[i] != b.Weight[i])
                    return false;
            }
            return true;
        }

        public static bool operator !=(VertexWeight a, VertexWeight b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is VertexWeight other && this == other;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            for (int i = 0; i < 4; i++)
            {
                hash = hash * 23 + Index[i];
                hash = hash * 23 + Weight[i].GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            var epsilon = 0.001f;
            return (Weight[0] > epsilon ? (Index[0].ToString() + ": " + Weight[0].ToString() + "\n") : string.Empty) +
                   (Weight[1] > epsilon ? (Index[1].ToString() + ": " + Weight[1].ToString() + "\n") : string.Empty) +
                   (Weight[2] > epsilon ? (Index[2].ToString() + ": " + Weight[2].ToString() + "\n") : string.Empty) +
                   (Weight[3] > epsilon ? (Index[3].ToString() + ": " + Weight[3].ToString() + "\n") : string.Empty);
        }
    }

    public class WadMesh : ICloneable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public string Name { get; set; }
        public List<Vector3> VertexPositions { get; set; } = new List<Vector3>();
        public List<Vector3> VertexNormals { get; set; } = new List<Vector3>();
        public List<Vector3> VertexColors { get; set; } = new List<Vector3>();
        public List<VertexWeight> VertexWeights { get; set; } = new List<VertexWeight>();
        public List<VertexAttributes> VertexAttributes { get; set; } = new List<VertexAttributes>();
        public List<WadPolygon> Polys { get; set; } = new List<WadPolygon>();
        public BoundingSphere BoundingSphere { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public WadMeshLightingType LightingType { get; set; }
        public bool Hidden { get; set; }

        public Hash Hash => Hash.FromByteArray(ToByteArray());

        public bool HasColors  => VertexColors.Count == VertexPositions.Count;
        public bool HasNormals => VertexNormals.Count == VertexPositions.Count;
        public bool HasAttributes => VertexAttributes.Count == VertexPositions.Count;
        public bool HasWeights => VertexWeights.Count == VertexPositions.Count;

        public List<TextureArea> TextureAreas => Polys.AsParallel().Select(p => p.Texture.GetCanonicalTexture(p.IsTriangle)).Distinct().ToList();

        public static readonly TextureArea EmptyTextureArea = new TextureArea()
        {
            Texture = new WadTexture(Texture.UnloadedPlaceholder),
            TexCoord0 = Vector2.Zero,
            TexCoord1 = new Vector2(0, Texture.UnloadedPlaceholder.Height),
            TexCoord2 = new Vector2(Texture.UnloadedPlaceholder.Width, Texture.UnloadedPlaceholder.Height),
            TexCoord3 = new Vector2(Texture.UnloadedPlaceholder.Width, 0),
        };

        public WadMesh Clone()
        {
            var mesh = (WadMesh)MemberwiseClone();
            mesh.VertexPositions = new List<Vector3>(VertexPositions);
            mesh.VertexNormals = new List<Vector3>(VertexNormals);
            mesh.VertexColors = new List<Vector3>(VertexColors);
            mesh.VertexAttributes = new List<VertexAttributes>(VertexAttributes);
            mesh.VertexWeights = new List<VertexWeight>(VertexWeights);
            mesh.Polys = new List<WadPolygon>(Polys);
            return mesh;
        }
        object ICloneable.Clone() => Clone();

        public byte[] ToByteArray()
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriterEx(ms);
                writer.Write(BoundingSphere.Center.X);
                writer.Write(BoundingSphere.Center.Y);
                writer.Write(BoundingSphere.Center.Z);
                writer.Write(BoundingSphere.Radius);
                writer.Write((int)LightingType);

                int numVertices = VertexPositions.Count;
                writer.Write(numVertices);

                for (int i = 0; i < VertexPositions.Count; i++)
                    writer.Write(VertexPositions[i]);

                if (VertexNormals.Count > 0)
                    for (int i = 0; i < VertexNormals.Count; i++)
                        writer.Write(VertexNormals[i]);

                if (VertexColors.Count > 0)
                    for (int i = 0; i < VertexColors.Count; i++)
                        writer.Write(VertexColors[i]);

                if (VertexAttributes.Count > 0)
                    for (int i = 0; i < VertexAttributes.Count; i++)
                    {
                        writer.Write(VertexAttributes[i].Glow);
                        writer.Write(VertexAttributes[i].Move);
                    }

                if (VertexWeights.Count > 0)
                    for (int i = 0; i < VertexWeights.Count; i++)
                        for (int j = 0; j < VertexWeights[i].Index.Length; j++)
                        {
                            writer.Write(VertexWeights[i].Index[j]);
                            writer.Write(VertexWeights[i].Weight[j]);
                        }

                int numPolygons = Polys.Count;
                writer.Write(numPolygons);
                for (int i = 0; i < Polys.Count; i++)
                {
                    WadPolygon poly = Polys[i];
                    writer.Write((ushort)Polys[i].Shape);
                    writer.Write(Polys[i].Index0);
                    writer.Write(Polys[i].Index1);
                    writer.Write(Polys[i].Index2);
                    if (Polys[i].Shape == WadPolygonShape.Quad)
                        writer.Write(Polys[i].Index3);
                    writer.Write(((WadTexture)Polys[i].Texture.Texture).Hash);
                    writer.Write(Polys[i].Texture.DoubleSided);
                    writer.Write((short)Polys[i].Texture.BlendMode);
                    writer.Write(Polys[i].Texture.TexCoord0);
                    writer.Write(Polys[i].Texture.TexCoord1);
                    writer.Write(Polys[i].Texture.TexCoord2);
                    if (Polys[i].Shape == WadPolygonShape.Quad)
                        writer.Write(Polys[i].Texture.TexCoord3);
                    writer.Write(Polys[i].ShineStrength);
                }

                return ms.ToArray();
            }
        }

        public BoundingBox CalculateBoundingBox(Matrix4x4 transform)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;
            
            foreach (Vector3 oldVertex in VertexPositions)
            {
                var transformedVertex = MathC.HomogenousTransform(oldVertex, transform);

                minX = (float)Math.Min(transformedVertex.X, minX);
                minY = (float)Math.Min(transformedVertex.Y, minY);
                minZ = (float)Math.Min(transformedVertex.Z, minZ);
                maxX = (float)Math.Max(transformedVertex.X, maxX);
                maxY = (float)Math.Max(transformedVertex.Y, maxY);
                maxZ = (float)Math.Max(transformedVertex.Z, maxZ);
            }

            return new BoundingBox(new Vector3(minX, maxY, minZ), new Vector3(maxX, minY, maxZ));
        }

        public BoundingBox CalculateBoundingBox()
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);
            foreach (Vector3 oldVertex in VertexPositions)
            {
                min = Vector3.Min(oldVertex, min);
                max = Vector3.Max(oldVertex, max);
            }
            return new BoundingBox(min, max);
        }

        public BoundingSphere CalculateBoundingSphere()
        {
            // Gets midpoint between circumscribed and inscribed spheres, since
            // this seems to be optimal radius for most meshes.

            return BoundingSphere.FromBoundingBox(CalculateBoundingBox());
        }

        public void CalculateNormals(bool weighted = true)
        {
            var helper = new VertexNormalAverageHelper(VertexPositions);

            foreach (var poly in Polys)
            {
                if (poly.IsTriangle)
                    helper.AddPolygon(weighted, poly.Index0, poly.Index1, poly.Index2);
                else
                    helper.AddPolygon(weighted, poly.Index0, poly.Index1, poly.Index2, poly.Index3);
            }

            VertexNormals = helper.CalculateNormals();
        }

        public bool GenerateMissingVertexData()
        {
            bool result = false;

            if (!HasColors)
            {
                result = true;
                VertexColors = Enumerable.Repeat(Vector3.One, VertexPositions.Count).ToList();
            }

            if (!HasAttributes)
            {
                result = true;
                VertexAttributes = Enumerable.Repeat(new VertexAttributes(), VertexPositions.Count).ToList();
            }

            if (!HasWeights)
            {
                result = true;
                VertexWeights = Enumerable.Repeat(new VertexWeight(), VertexPositions.Count).ToList();
            }

            return result;
        }

        public static WadMesh ImportFromExternalModel(string fileName, IOGeometrySettings settings, TextureArea placeholderTexture)
        {
            var list = ImportFromExternalModel(fileName, settings, true, placeholderTexture);
            if (list != null && list.Count > 0)
                return list.First();
            else
                return null;
        }

        public static IOModel PrepareForExport(string filePath, IOGeometrySettings settings, WadMesh m)
        {
            var model = new IOModel();
            var mesh = new IOMesh(m.Name);
            model.Meshes.Add(mesh);

            if (settings.PackTextures)
            {
                m = m.Clone();
                for (int i = 0; i < m.Polys.Count; i++)
                {
                    var p = m.Polys[i];

                    var rect = p.Texture.GetRect();
                    var image = ImageC.CreateNew((int)rect.Width, (int)rect.Height);
                    image.CopyFrom(0, 0, p.Texture.Texture.Image, (int)rect.TopLeft.X, (int)rect.TopLeft.Y, (int)rect.Width, (int)rect.Height);

                    var texture = p.Texture;
                    texture.Texture = new WadTexture(image);
                    texture.TexCoord0 -= rect.Start;
                    texture.TexCoord1 -= rect.Start;
                    texture.TexCoord2 -= rect.Start;
                    texture.TexCoord3 -= rect.Start;

                    p.Texture = texture;
                    m.Polys[i] = p;
                }
            }

            // Collect all textures
            var tempTextures = new Dictionary<Hash, WadTexture>();
            for (int i = 0; i < m.Polys.Count; i++)
            {
                var poly = m.Polys[i];

                // Add uniquely the texture to the dictionary
                if (!tempTextures.ContainsKey(((WadTexture)poly.Texture.Texture).Hash))
                    tempTextures.Add(((WadTexture)poly.Texture.Texture).Hash, ((WadTexture)poly.Texture.Texture));
            }

            var textureList = tempTextures.Values.ToList();
            textureList.Sort(delegate (WadTexture x, WadTexture y)
            {
                if (x.Image.Width > y.Image.Width)
                    return -1;
                else if (x.Image.Width < y.Image.Width)
                    return 1;
                return 0;
            });

            var texturePieces = new Dictionary<Hash, WadTexture.AtlasReference>();
            foreach (var texture in textureList)
            {
                texturePieces.Add(texture.Hash, new WadTexture.AtlasReference
                {
                    Texture = texture
                });
            }

            // Only merge textures into pages if all of them are no more than 256px in dimensions,
            // otherwise algorithm may fail.

            // FIXME Monty: modify algorithm in a way that it looks into every poly's texture area zone
            // for comparison, not just bluntly hash whole image. This way algorithm will never get
            // to a point when incoming texture fragment is bigger than 256.

            bool mergeIntoPages = settings.PackTextures &&
                                  m.Polys.All(p => p.Texture.Texture.Image.Size.X <= 256 &&
                                                   p.Texture.Texture.Image.Size.Y <= 256);
            List<WadTexture> pages;

            if (mergeIntoPages)
                pages = Wad2.PackTexturesForExport(texturePieces, settings.PadPackedTextures ? 4 : 0);
            else
                pages = tempTextures.Values.ToList();

            // Clean up incorrect textures
            pages = pages.Where(p => p.Image.Width > 0 && p.Image.Height > 0).ToList();

            var name = string.IsNullOrEmpty(mesh.Name) ? "UntitledMesh" : mesh.Name;

            // Create the materials
            for (int i = 0; i < pages.Count; i++)
            {
                var textureFileName = name + "_" + i + ".png";
                var path = Path.Combine(Path.GetDirectoryName(filePath), textureFileName);

                var matOpaque = new IOMaterial(Material.Material_Opaque + "_" + i, pages[i], path, false, false, 0, i);
                var matOpaqueDoubleSided = new IOMaterial(Material.Material_OpaqueDoubleSided + "_" + i, pages[i], path, false, true, 0, i);
                var matAdditiveBlending = new IOMaterial(Material.Material_AdditiveBlending + "_" + i, pages[i], path, true, false, 0, i);
                var matAdditiveBlendingDoubleSided = new IOMaterial(Material.Material_AdditiveBlendingDoubleSided + "_" + i, pages[i], path, true, true, 0, i);

                model.Materials.Add(matOpaque);
                model.Materials.Add(matOpaqueDoubleSided);
                model.Materials.Add(matAdditiveBlending);
                model.Materials.Add(matAdditiveBlendingDoubleSided);
            }

            int lastIndex = 0;

            foreach (var p in m.Polys)
            {
                var poly = new IOPolygon(p.Shape == WadPolygonShape.Quad ? IOPolygonShape.Quad : IOPolygonShape.Triangle);

                mesh.Positions.Add(m.VertexPositions[p.Index0]);
                mesh.Positions.Add(m.VertexPositions[p.Index1]);
                mesh.Positions.Add(m.VertexPositions[p.Index2]);
                if (p.Shape == WadPolygonShape.Quad)
                {
                    mesh.Positions.Add(m.VertexPositions[p.Index3]);
                }

                if (m.HasNormals)
                {
                    mesh.Normals.Add(m.VertexNormals[p.Index0]);
                    mesh.Normals.Add(m.VertexNormals[p.Index1]);
                    mesh.Normals.Add(m.VertexNormals[p.Index2]);
                    if (p.Shape == WadPolygonShape.Quad)
                    {
                        mesh.Normals.Add(m.VertexNormals[p.Index3]);
                    }
                }

                var texture = texturePieces[((WadTexture)p.Texture.Texture).Hash];

                var offset = mergeIntoPages ?
                    new Vector2
                    (
                        Math.Max(0.0f, texture.Position.X),
                        Math.Max(0.0f, texture.Position.Y)
                    ) : Vector2.Zero;

                var size = mergeIntoPages ? pages[texture.Atlas].Image.Size : p.Texture.Texture.Image.Size;

                mesh.UV.Add((p.Texture.TexCoord0 + offset) / size);
                mesh.UV.Add((p.Texture.TexCoord1 + offset) / size);
                mesh.UV.Add((p.Texture.TexCoord2 + offset) / size);
                if (p.Shape == WadPolygonShape.Quad)
                {
                    mesh.UV.Add((p.Texture.TexCoord3 + offset) / size);
                }

                if (m.HasColors)
                {
                    mesh.Colors.Add(new Vector4(m.VertexColors[p.Index0], 1.0f));
                    mesh.Colors.Add(new Vector4(m.VertexColors[p.Index1], 1.0f));
                    mesh.Colors.Add(new Vector4(m.VertexColors[p.Index2], 1.0f));
                    if (p.Shape == WadPolygonShape.Quad)
                    {
                        mesh.Colors.Add(new Vector4(m.VertexColors[p.Index3], 1.0f));
                    }
                }
                else
                {
                    mesh.Colors.Add(Vector4.One);
                    mesh.Colors.Add(Vector4.One);
                    mesh.Colors.Add(Vector4.One);
                    if (p.Shape == WadPolygonShape.Quad)
                    {
                        mesh.Colors.Add(Vector4.One);
                    }
                }

                var mat = model.Materials[0];
                foreach (var mt in model.Materials)
                    if ((mergeIntoPages && mt.Page == texture.Atlas) ||
                        (!mergeIntoPages && mt.Texture == p.Texture.Texture))
                            if (mt.AdditiveBlending == (p.Texture.BlendMode >= BlendMode.Additive))
                                if (mt.DoubleSided == p.Texture.DoubleSided)
                                    if (mt.Shininess == 0)
                                        mat = mt;

                poly.Indices.Add(lastIndex + 0);
                poly.Indices.Add(lastIndex + 1);
                poly.Indices.Add(lastIndex + 2);
                if (p.Shape == WadPolygonShape.Quad)
                {
                    poly.Indices.Add(lastIndex + 3);
                }

                if (!mesh.Submeshes.ContainsKey(mat))
                    mesh.Submeshes.Add(mat, new IOSubmesh(mat));

                mesh.Submeshes[mat].Polygons.Add(poly);
                lastIndex += (p.Shape == WadPolygonShape.Quad ? 4 : 3);
            }

            for (int i = 0; i < pages.Count; i++)
            {
                var textureFileName = name + "_" + i + ".png";
                var path = Path.Combine(Path.GetDirectoryName(filePath), textureFileName);
                pages[i].Image.SaveToFile(path);
            }

            return model;
        }

        public static List<WadMesh> ImportFromExternalModel(string fileName, IOGeometrySettings settings, bool mergeIntoOne, TextureArea placeholderTexture)
        {
            if (placeholderTexture.TextureIsUnavailable) 
                placeholderTexture = EmptyTextureArea;

            IOModel tmpModel = null;
            var meshList = new List<WadMesh>();

            bool calculateNormals = false;

            // Import the model
            try
            {
                var importer = BaseGeometryImporter.CreateForFile(fileName, settings, absoluteTexturePath =>
                {
					var image = ImageC.FromFile(absoluteTexturePath);
					if (!settings.KeepTexturesExternal)
						image.FileName = string.Empty;
                    
                    var texture = new WadTexture(image);
                    texture.AbsolutePath = image.FileName;
                    return texture;
				});
                tmpModel = importer.ImportFromFile(fileName);

                calculateNormals = importer is MetasequoiaImporter;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Geometry import failed!");
                return null;
            }

            // Create a new mesh (all meshes from model will be joined)
            WadMesh mesh = null;
            for (int i = 0; i < tmpModel.Meshes.Count; i++)
            {
                var tmpMesh = tmpModel.Meshes[i];
                var tmpPos  = new List<Vector3>();
                var tmpCol  = new List<Vector3>();
                var tmpNor  = new List<Vector3>();
                var tmpWgt  = new List<VertexWeight>();

                if (mesh == null || !mergeIntoOne)
                {
                    mesh = new WadMesh();
                    mesh.Name = string.IsNullOrEmpty(tmpMesh.Name) ? "ImportedMesh" + i : tmpMesh.Name;
                }

                // Fix rounding errors in vertex positions
                tmpPos.AddRange(tmpMesh.Positions.Select(v => new Vector3((float)Math.Round(v.X, 3),
                                                                          (float)Math.Round(v.Y, 3),
                                                                          (float)Math.Round(v.Z, 3))));
                // Copy normals as well, if they are consistent
                if (tmpMesh.Normals.Count == tmpMesh.Positions.Count)
                    tmpNor.AddRange(tmpMesh.Normals.Select(v => new Vector3((float)Math.Round(v.X, 3),
                                                                            (float)Math.Round(v.Y, 3),
                                                                            (float)Math.Round(v.Z, 3))));
                
                // Copy vertex colors, if they are consistent and omit alpha if exists
                if (tmpMesh.Colors.Count == tmpMesh.Positions.Count)
                    tmpCol.AddRange(tmpMesh.Colors.Select(v => v.To3()));

                if (tmpMesh.Weights.Count == tmpMesh.Positions.Count)
                    tmpWgt.AddRange(tmpMesh.Weights.Select(v =>
                    {
                        var weight = new VertexWeight();

                        int addedCount = 0;

                        for (int w = 0; w < v.Count; w++)
                        {
                            // Maximum bone weight count reached, exit conversion process.
                            if (addedCount >= weight.Index.Length)
                                break;

                            var value = v.ElementAt(w).Value;

                            // Discard weights below threshold, which may have been added by smooth pass.
                            // Threshold is approximately equal to lowest possible 8-bit value (1 / 255).
                            if (value < 0.004f)
                                continue;

                            weight.Index[addedCount] = v.ElementAt(w).Key;
                            weight.Weight[addedCount] = value;

                            addedCount++;
                        }

                        // Normalize weights.

                        float total = weight.Weight[0] + weight.Weight[1] + weight.Weight[2] + weight.Weight[3];
                        if (total == 0.0f)
                        {
                            for (int w = 0; w < 4; w++)
                                weight.Weight[w] = 0.0f;
                            return weight;
                        }

                        for (int w = 0; w < 4; w++)
                            weight.Weight[w] /= total;

                        return weight;
                    }));

                foreach (var tmpSubmesh in tmpMesh.Submeshes)
                    foreach (var tmpPoly in tmpSubmesh.Value.Polygons)
                    {
                        var vertexCount = (tmpPoly.Shape == IOPolygonShape.Quad ? 4 : 3);

                        var poly = new WadPolygon
                        { Shape = vertexCount == 4 ? WadPolygonShape.Quad : WadPolygonShape.Triangle};

                        // ULTRA HACK for ass-imp disjointment mess!
                        // Ass-imp tends to disjoint shared vertices in case different faces have non-adjacent
                        // texture coordinates. Sadly no ass-imp options can solve this problem, so only way
                        // to do this is to manually rejoint similar ones.

                        for (int j = 0; j < vertexCount; j++)
                        {
                            // Get assimp's own index which is wrong by now
                            var tmpIndex = tmpPoly.Indices[j];

                            // Find first entry in filtered vertex list which is similar
                            // to the one assimp index is refering to in its vertex list and remember it as candidate.

                            int candidate = -1;
                            for (int k = 0; k < mesh.VertexPositions.Count; k++)
                            {
                                if (tmpPos[tmpIndex] == mesh.VertexPositions[k] &&
                                    (mesh.VertexColors.Count  == 0 || tmpCol[tmpIndex] == mesh.VertexColors[k]) &&
                                    (mesh.VertexNormals.Count == 0 || tmpNor[tmpIndex] == mesh.VertexNormals[k]) &&
                                    (mesh.VertexWeights.Count == 0 || tmpWgt[tmpIndex] == mesh.VertexWeights[k]))
                                {
                                    candidate = k;
                                    break;
                                }
                            }

                            // Copy vertex data from assimp lists to mesh lists, also
                            // keeping data index to refer to it later in case similar but 
                            // duplicated entry is found.

                            if (candidate == -1)
                            {
                                candidate = mesh.VertexPositions.Count;
                                mesh.VertexPositions.Add(tmpPos[tmpIndex]);
                                if (tmpCol.Count > 0) mesh.VertexColors.Add(tmpCol[tmpIndex]);
                                if (tmpNor.Count > 0) mesh.VertexNormals.Add(tmpNor[tmpIndex]);
                                if (tmpWgt.Count > 0) mesh.VertexWeights.Add(tmpWgt[tmpIndex]);
                            }

                            switch (j)
                            {
                                case 0: poly.Index0 = candidate; break;
                                case 1: poly.Index1 = candidate; break;
                                case 2: poly.Index2 = candidate; break;
                                case 3: poly.Index3 = candidate; break;
                            }
                        }
                        
                        var area = new TextureArea();

                        if (tmpSubmesh.Value.Material.Texture == null)
                            area = placeholderTexture;
                        else
                        {
                            area.Texture = tmpSubmesh.Value.Material.Texture;

                            if (settings.MappedUV)
                                area.SetParentArea();

                            area.TexCoord0 = tmpMesh.UV[tmpPoly.Indices[0]];
                            area.TexCoord1 = tmpMesh.UV[tmpPoly.Indices[1]];
                            area.TexCoord2 = tmpMesh.UV[tmpPoly.Indices[2]];
                            if (vertexCount == 4)
                                area.TexCoord3 = tmpMesh.UV[tmpPoly.Indices[3]];
                            else
                                area.TexCoord3 = area.TexCoord2;

                            area.ClampToBounds();
                        }

                        area.DoubleSided = tmpSubmesh.Value.Material.DoubleSided;
                        area.BlendMode = tmpSubmesh.Value.Material.AdditiveBlending ? BlendMode.Additive : BlendMode.Normal;

                        poly.Texture = area;
                        poly.ShineStrength = (byte)Math.Min(Math.Round(tmpSubmesh.Value.Material.Shininess / 16.0f, MidpointRounding.ToEven), 63);

                        mesh.Polys.Add(poly);
                    }

                if (!mergeIntoOne || i == tmpModel.Meshes.Count - 1)
                {
                    mesh.BoundingBox = mesh.CalculateBoundingBox();
                    mesh.BoundingSphere = mesh.CalculateBoundingSphere();

                    if (!mesh.HasNormals || calculateNormals)
                        mesh.CalculateNormals(); // MQO files rarely have normals
                    
                    if (!mesh.HasColors)
                        mesh.VertexColors.Clear(); // Reset vertex colors in case they got desynced from vertex count

                    meshList.Add(mesh);
                }
            }

            return meshList;
        }

        public static WadMesh Empty { get; } = new WadMesh() { Name = string.Empty };
    }
}

