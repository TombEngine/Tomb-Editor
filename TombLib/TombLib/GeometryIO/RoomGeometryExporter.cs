using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorStructs;
using TombLib.Utils;

namespace TombLib.GeometryIO
{
    sealed public class RoomExportResult
    {
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public IOModel Model { get; set; }

        public bool Valid()
        {
            if (Model.Materials.Count == 0 || Model.Meshes.Count == 0)
                return false;

            int vertexCount = 0;
            foreach (var mesh in Model.Meshes)
                vertexCount += mesh.Indices.Count;

            return vertexCount > 0;
        }
    }

    public class SplitPageReference
    {
        public int Index { get; }
        public string Path { get; }

        private int _startX;
        private int _startY;
        private int _width;
        private int _height;
        private Texture _texture;

        public SplitPageReference(Texture texture, int index, int x, int y, string path)
        {
            var splitSize = RoomGeometryExporter.SplitPageSize;

            Index = index;
            Path = path;
            _startX = x * splitSize;
            _startY = y * splitSize;
            _width = (x * splitSize + splitSize > texture.Image.Width ? texture.Image.Width - x * splitSize : splitSize);
            _height = (y * splitSize + splitSize > texture.Image.Height ? texture.Image.Height - y * splitSize : splitSize);
            _texture = texture;
        }

        public void SaveTexture()
        {
            ImageC newImage = ImageC.CreateNew(RoomGeometryExporter.SplitPageSize, RoomGeometryExporter.SplitPageSize);
            newImage.CopyFrom(0, 0, _texture.Image, _startX, _startY, _width, _height);
            newImage.SaveToFile(Path);
        }
    }

    public static class RoomGeometryExporter
    {
        public const string RoomIdentifier = "TeRoom_";
        public const int SplitPageSize = 256;

        private static Vector2 GetNormalizedPageUVs(Vector2 uv, int textureWidth, int textureHeight, int page)
        {
            int numXPages = getNumXPages(textureWidth);
            int numYPages = getNumYPages(textureHeight);
            int yPage = page / numXPages;
            int xPage = page % numXPages;
            int uOffset = xPage * SplitPageSize;
            int vOffset = yPage * SplitPageSize;
            return new Vector2((uv.X - uOffset) / SplitPageSize, (uv.Y - vOffset) / SplitPageSize);
        }

        private static int getNumXPages(int width)
        {
            return (int)Math.Ceiling((float)width / SplitPageSize);
        }

        private static int getNumYPages(int height)
        {
            return (int)Math.Ceiling((float)height / SplitPageSize);
        }

        public static RoomExportResult ExportRooms(IEnumerable<Room> roomsToExport, string filePath, Level level, bool exportInWorldCoordinates = false)
        {
            RoomExportResult result = new RoomExportResult();
            try
            {
                //Prepare data for export
                var model = new IOModel();
                var usedTextures = new List<Texture>();
                var splitPages = new List<SplitPageReference>();

                foreach (var room in roomsToExport)
                {
                    for (int x = 0; x < room.NumXSectors; x++)
                    {
                        for (int z = 0; z < room.NumZSectors; z++)
                        {
                            var sector = room.GetSector(new VectorInt2(x, z));
                            foreach (SectorFace face in sector.GetFaceTextures().Keys)
                            {
                                var faceTexture = sector.GetFaceTexture(face);
                                if (faceTexture.TextureIsInvisible || faceTexture.TextureIsUnavailable || faceTexture.Texture == null)
                                    continue;
                                if (!usedTextures.Contains(faceTexture.Texture))
                                    usedTextures.Add(faceTexture.Texture);
                            }
                        }
                    }
                }

                for (int j = 0; j < usedTextures.Count; j++)
                {
                    var tex = usedTextures[j];
                    int numXPages = getNumXPages(tex.Image.Width);
                    int numYPages = getNumYPages(tex.Image.Height);
                    string baseName = Path.GetFileNameWithoutExtension(tex.Image.FileName);

                    for (int y = 0; y < numYPages; y++)
                    {
                        for (int x  = 0; x < numXPages; x++)
                        {
                            // Generate future page reference but postpone actual file creation until model is made.
                            string textureFileName = baseName + "_" + x + "_" + y + ".png";
                            var page = new SplitPageReference(tex, y * numXPages + x, x, y, Path.Combine(Path.GetDirectoryName(filePath), textureFileName));
                            splitPages.Add(page);

                            var matOpaque = new IOMaterial(Material.Material_Opaque + "_" + j + "_" + page.Index, tex, page.Path, false, false, 0, page.Index);
                            var matOpaqueDoubleSided = new IOMaterial(Material.Material_OpaqueDoubleSided + "_" + j + "_" + page.Index, tex, page.Path, false, true, 0, page.Index);
                            var matAdditiveBlending = new IOMaterial(Material.Material_AdditiveBlending + "_" + j + "_" + page.Index, tex, page.Path, true, false, 0, page.Index);
                            var matAdditiveBlendingDoubleSided = new IOMaterial(Material.Material_AdditiveBlendingDoubleSided + "_" + j + "_" + page.Index, tex, page.Path, true, true, 0, page.Index);

                            model.Materials.Add(matOpaque);
                            model.Materials.Add(matOpaqueDoubleSided);
                            model.Materials.Add(matAdditiveBlending);
                            model.Materials.Add(matAdditiveBlendingDoubleSided);
                        }
                    }
                }

                foreach (var room in roomsToExport)
                {
                    if (room.RoomGeometry == null)
                        continue;

                    Vector3 offset;
                    if (exportInWorldCoordinates)
                        // If we're in multi-room export, use world position
                        offset = room.WorldPos;
                    else
                    {
                        // Make room center pivot if we're in single-room export mode
                        offset = -room.GetLocalCenter();
                        int x = (int)-offset.X / (int)Level.SectorSizeUnit;
                        int z = (int)-offset.Z / (int)Level.SectorSizeUnit;
                        offset.Y = -room.GetLowestCorner(new RectangleInt2(x, z, x, z));
                    }

                    int index = level.Rooms.ReferenceIndexOf(room);
                    int xOff = room.Position.X;
                    int yOff = room.Position.Y;
                    int zOff = room.Position.Z;

                    // Append the Offset to the Mesh name, we can later calculate the correct position
                    string meshFormat = "TeRoom_{0}_{1}_{2}_{3}";
                    var mesh = new IOMesh(string.Format(meshFormat, index, xOff, yOff, zOff));

                    int lastIndex = 0;
                    for (int x = 0; x < room.NumXSectors; x++)
                    for (int z = 0; z < room.NumZSectors; z++)
                    {
                        var sector = room.GetSector(new VectorInt2(x, z));

                        foreach (SectorFace face in sector.GetFaceTextures().Keys)
                        {
                            var faceTexture = sector.GetFaceTexture(face);

                            if (faceTexture.TextureIsInvisible || faceTexture.TextureIsUnavailable)
                                continue;

                            var range = room.RoomGeometry.VertexRangeLookup.TryGetOrDefault(new SectorFaceIdentity(x, z, face));
                            var shape = room.GetFaceShape(x, z, face);

                            if (shape == FaceShape.Quad)
                            {
                                int i = range.Start;

                                var textureArea1 = room.RoomGeometry.TriangleTextureAreas[i / 3];
                                var textureArea2 = room.RoomGeometry.TriangleTextureAreas[(i + 3) / 3];

                                if (textureArea1.TextureIsUnavailable || textureArea1.TextureIsInvisible)
                                    continue;
                                if (textureArea2.TextureIsUnavailable || textureArea2.TextureIsInvisible)
                                    continue;

                                int textureAreaPage = GetTextureAreaPage(textureArea1, textureArea2);
                                if (textureAreaPage < 0)
                                {
                                    result.Warnings.Add(string.Format("Quad at ({0},{1}) in Room {2} has a texture that is beyond the 256px boundary. TexturePage is set to 0", x, z, room));
                                    textureAreaPage = 1;
                                }

                                var poly = new IOPolygon(IOPolygonShape.Quad);
                                poly.Indices.Add(lastIndex + 0);
                                poly.Indices.Add(lastIndex + 1);
                                poly.Indices.Add(lastIndex + 2);
                                poly.Indices.Add(lastIndex + 3);

                                var uvFactor = new Vector2(0.5f / (float)textureArea1.Texture.Image.Width, 0.5f / (float)textureArea1.Texture.Image.Height);
                                int textureWidth = textureArea1.Texture.Image.Width;
                                int textureHeight = textureArea1.Texture.Image.Height;

                                if (face != SectorFace.Ceiling)
                                {
                                    mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 3] + offset);
                                    mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 2] + offset);
                                    mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 0] + offset);
                                    mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 1] + offset);
                                    mesh.UV.Add(GetNormalizedPageUVs(textureArea2.TexCoord0, textureWidth, textureHeight, textureAreaPage));
                                    mesh.UV.Add(GetNormalizedPageUVs(textureArea1.TexCoord2, textureWidth, textureHeight, textureAreaPage));
                                    mesh.UV.Add(GetNormalizedPageUVs(textureArea1.TexCoord0, textureWidth, textureHeight, textureAreaPage));
                                    mesh.UV.Add(GetNormalizedPageUVs(textureArea1.TexCoord1, textureWidth, textureHeight, textureAreaPage));
                                    mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 3], 1.0f));
                                    mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 2], 1.0f));
                                    mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 0], 1.0f));
                                    mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 1], 1.0f));
                                }
                                else
                                {
                                    mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 1] + offset);
                                    mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 2] + offset);
                                    mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 0] + offset);
                                    mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 5] + offset);
                                    mesh.UV.Add(GetNormalizedPageUVs(textureArea1.TexCoord1, textureWidth, textureHeight, textureAreaPage));
                                    mesh.UV.Add(GetNormalizedPageUVs(textureArea1.TexCoord2, textureWidth, textureHeight, textureAreaPage));
                                    mesh.UV.Add(GetNormalizedPageUVs(textureArea1.TexCoord0, textureWidth, textureHeight, textureAreaPage));
                                    mesh.UV.Add(GetNormalizedPageUVs(textureArea2.TexCoord2, textureWidth, textureHeight, textureAreaPage));
                                    mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 1], 1.0f));
                                    mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 2], 1.0f));
                                    mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 0], 1.0f));
                                    mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 5], 1.0f));
                                }

                                var mat = model.GetMaterial(textureArea1.Texture,
                                                            textureArea1.BlendMode >= BlendMode.Additive,
                                                            textureAreaPage,
                                                            textureArea1.DoubleSided,
                                                            0);

                                if (!mesh.Submeshes.ContainsKey(mat))
                                    mesh.Submeshes.Add(mat, new IOSubmesh(mat));

                                mesh.Submeshes[mat].Polygons.Add(poly);
                                lastIndex += 4;
                            }
                            else if (shape == FaceShape.Triangle)
                            {
                                int i = range.Start;

                                var textureArea = room.RoomGeometry.TriangleTextureAreas[i / 3];
                                if (textureArea.TextureIsUnavailable || textureArea.TextureIsInvisible || textureArea.Texture == null)
                                    continue;
                                int textureAreaPage = GetTextureAreaPage(textureArea, null);
                                if (textureAreaPage < 0)
                                {
                                    result.Warnings.Add(string.Format("Triangle at ({0},{1}) in Room {2} has a texture that is beyond the 256px boundary. TexturePage is set to 0", x, z, room));
                                    textureAreaPage = 1;
                                }
                                var poly = new IOPolygon(IOPolygonShape.Triangle);
                                poly.Indices.Add(lastIndex);
                                poly.Indices.Add(lastIndex + 1);
                                poly.Indices.Add(lastIndex + 2);

                                mesh.Positions.Add(room.RoomGeometry.VertexPositions[i]     + offset);
                                mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 1] + offset);
                                mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 2] + offset);

                                var uvFactor = new Vector2(0.5f / (float)textureArea.Texture.Image.Width, 0.5f / (float)textureArea.Texture.Image.Height);
                                int textureWidth = textureArea.Texture.Image.Width;
                                int textureHeight = textureArea.Texture.Image.Height;
                                mesh.UV.Add(GetNormalizedPageUVs(textureArea.TexCoord0, textureWidth, textureHeight, textureAreaPage));
                                mesh.UV.Add(GetNormalizedPageUVs(textureArea.TexCoord1, textureWidth, textureHeight, textureAreaPage));
                                mesh.UV.Add(GetNormalizedPageUVs(textureArea.TexCoord2, textureWidth, textureHeight, textureAreaPage));

                                mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i], 1.0f));
                                mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 1], 1.0f));
                                mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 2], 1.0f));

                                var mat = model.GetMaterial(textureArea.Texture,
                                                            textureArea.BlendMode >= BlendMode.Additive,
                                                            textureAreaPage,
                                                            textureArea.DoubleSided,
                                                            0);

                                if (!mesh.Submeshes.ContainsKey(mat))
                                    mesh.Submeshes.Add(mat, new IOSubmesh(mat));

                                mesh.Submeshes[mat].Polygons.Add(poly);
                                lastIndex += 3;
                            }
                        }
                    }
                    model.Meshes.Add(mesh);
                }

                // Save only texture pages which are actually used in model.
                foreach (var page in splitPages)
                    if (model.UsedMaterials.Any(mat => mat.Path == page.Path))
                        page.SaveTexture();

                result.Model = model;

                if (!result.Valid())
                    result.Errors.Add("Nothing was exported. Check if rooms selected for export are textured and has any geometry.");
            }
            catch (Exception e)
            {
                result.Errors.Add(e.Message);
            }
            return result;
        }

        private static int GetTextureAreaPage(TextureArea textureArea1, TextureArea? textureArea2)
        {
            int width = textureArea1.Texture.Image.Width;
            int height = textureArea1.Texture.Image.Height;
            int numXPages = (int)Math.Ceiling((float)width / SplitPageSize);
            int numYPages = (int)Math.Ceiling((float)height / SplitPageSize);

            Rectangle2 textureRect = textureArea2 != null ? textureArea1.GetRect().Union(textureArea2.Value.GetRect()) : textureArea1.GetRect();

            for (int yPage = 0; yPage < numYPages; yPage++)
                for (int xPage = 0; xPage < numXPages; xPage++)
                {
                    Rectangle2 pageRect = new RectangleInt2(xPage * SplitPageSize, yPage * SplitPageSize, (xPage + 1) * SplitPageSize, (yPage + 1) * SplitPageSize);
                    if(pageRect.Contains(textureRect))
                    {
                        return yPage * numXPages + xPage;
                    }
                }
            return -1;
        }
    }
}
