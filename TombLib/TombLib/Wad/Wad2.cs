using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad.Tr4Wad;
using TombLib.Wad.TrLevels;

namespace TombLib.Wad
{
    public class ComparerWadTextures : IComparer<WadTexture>
    {
        public int Compare(WadTexture x, WadTexture y)
        {
            if (x == null || y == null)
                return 0;

            return -x.Image.Height.CompareTo(y.Image.Height);
        }
    }

    public class Wad2
    {
        public TRVersion.Game GameVersion { get; set; } = TRVersion.Game.TR4;
        public SortedList<WadMoveableId, WadMoveable> Moveables { get; set; } = new SortedList<WadMoveableId, WadMoveable>();
        public SortedList<WadStaticId, WadStatic> Statics { get; set; } = new SortedList<WadStaticId, WadStatic>();
        public SortedList<WadSpriteSequenceId, WadSpriteSequence> SpriteSequences { get; set; } = new SortedList<WadSpriteSequenceId, WadSpriteSequence>();
        public List<AnimatedTextureSet> AnimatedTextureSets { get; set; } = new List<AnimatedTextureSet>();

        public string FileName { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string UserNotes { get; set; } = string.Empty;
        public bool HasUnknownData { get; set; } = false;

        public WadSounds Sounds { get; set; }

        public Wad2()
        {
            Sounds = new WadSounds();
        }

        public bool WadIsEmpty => String.IsNullOrEmpty(FileName) && Moveables.Count == 0 && Statics.Count == 0 && SpriteSequences.Count == 0;

        public HashSet<WadTexture> MeshTexturesUnique
        {
            get
            {
                var textures = new HashSet<WadTexture>();
                foreach (var moveable in Moveables)
                {
                    foreach (var mesh in moveable.Value.Meshes)
                        if (mesh != null)
                            foreach (WadPolygon polygon in mesh.Polys)
                                textures.Add((WadTexture)polygon.Texture.Texture);

                    if (moveable.Value.Skin != null)
                        foreach (WadPolygon polygon in moveable.Value.Skin.Polys)
                            textures.Add((WadTexture)polygon.Texture.Texture);
                }
                foreach (var stat in Statics)
                    if (stat.Value.Mesh != null)
                        foreach (WadPolygon polygon in stat.Value.Mesh.Polys)
                            textures.Add((WadTexture)polygon.Texture.Texture);
				foreach (var set in AnimatedTextureSets)
                    foreach (var frame in set.AnimationType == AnimatedTextureAnimationType.Video ? new List<AnimatedTextureFrame>() : set.Frames)
                        if (frame.Texture is WadTexture wadTexture)
                            textures.Add(wadTexture);
				
				return textures;
            }
        }

        public List<WadMesh> MeshesUnique
        {
            get
            {
                var meshes = new List<WadMesh>();
                foreach (WadMoveable moveable in Moveables.Values)
                {
                    foreach (WadMesh mesh in moveable.Meshes)
                        meshes.Add(mesh);

                    if (moveable.Skin != null)
                        meshes.Add(moveable.Skin);
                }
                foreach (WadStatic @static in Statics.Values)
                    meshes.Add(@static.Mesh);
                return meshes;
            }
        }

        public List<TextureArea> MeshTexInfosUnique
        {
            get
            {
                var texinfos = new List<TextureArea>();

                foreach (var moveable in Moveables.Values)
                {
                    foreach (var mesh in moveable.Meshes)
                        texinfos.AddRange(mesh.TextureAreas);

                    if (moveable.Skin != null)
                        texinfos.AddRange(moveable.Skin.TextureAreas);
                }
                foreach (var stat in Statics.Values)
                    texinfos.AddRange(stat.Mesh.TextureAreas);

                return texinfos.Distinct().ToList();
            }
        }

        public WadStaticId GetFirstFreeStaticMesh()
        {
            for (int i = 0; i < Statics.Count; i++)
                if (!Statics.ContainsKey(new WadStaticId((uint)i)))
                    return new WadStaticId((uint)i);
            return new WadStaticId();
        }

        public static Wad2 ImportFromFile(string fileName, bool withSounds, IDialogHandler progressReporter, bool allowTRNGDecryption = false)
        {
            if (fileName.EndsWith(".wad2", StringComparison.InvariantCultureIgnoreCase))
                return Wad2Loader.LoadFromFile(fileName, withSounds);
            else if (fileName.EndsWith(".wad", StringComparison.InvariantCultureIgnoreCase) ||
                     fileName.EndsWith(".was", StringComparison.InvariantCultureIgnoreCase) ||
                     fileName.EndsWith(".sam", StringComparison.InvariantCultureIgnoreCase) ||
                     fileName.EndsWith(".sfx", StringComparison.InvariantCultureIgnoreCase) ||
                     fileName.EndsWith(".swd", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!fileName.EndsWith(".wad", StringComparison.InvariantCultureIgnoreCase))
                    fileName = Path.ChangeExtension(fileName, "wad");

                var oldWad = new Tr4Wad.Tr4Wad();
                oldWad.LoadWad(fileName);
                var newWad = Tr4WadOperations.ConvertTr4Wad(oldWad, progressReporter);
                newWad.Timestamp = File.GetLastWriteTime(fileName);
                return newWad;
            }
            else
            {
                var originalLevel = new TrLevel();
                originalLevel.LoadLevel(fileName, allowTRNGDecryption);
                var newWad = TrLevelOperations.ConvertTrLevel(originalLevel);
                newWad.Timestamp = File.GetLastWriteTime(fileName);
                return newWad;
            }
        }

        public List<WadMoveable> GetAllMoveablesReferencingSound(int id)
        {
            var moveables = new List<WadMoveable>();
            foreach (var moveable in Moveables)
                foreach (var animation in moveable.Value.Animations)
                    foreach (var command in animation.AnimCommands)
                        if (command.Type == WadAnimCommandType.PlaySound)
                            if ((command.Parameter2) == id)
                                if (!moveables.Contains(moveable.Value))
                                    moveables.Add(moveable.Value);
            return moveables;
        }

        public IWadObject TryGet(IWadObjectId wadObjectId)
        {
            if (wadObjectId is WadMoveableId)
                return Moveables.TryGetOrDefault((WadMoveableId)wadObjectId);
            else if (wadObjectId is WadStaticId)
                return Statics.TryGetOrDefault((WadStaticId)wadObjectId);
            else if (wadObjectId is WadSpriteSequenceId)
                return SpriteSequences.TryGetOrDefault((WadSpriteSequenceId)wadObjectId);
            else
                throw new ArgumentException("Argument not of a valid type.");
        }

        public void Remove(IWadObjectId wadObjectId)
        {
            if (wadObjectId is WadMoveableId)
                Moveables.Remove((WadMoveableId)wadObjectId);
            else if (wadObjectId is WadStaticId)
                Statics.Remove((WadStaticId)wadObjectId);
            else if (wadObjectId is WadSpriteSequenceId)
                SpriteSequences.Remove((WadSpriteSequenceId)wadObjectId);
            else
                throw new ArgumentException("Argument not of a valid type.");
        }

        public void Add(IWadObjectId newId, IWadObject wadObject)
        {
            // Change id if necessary
            if (newId != wadObject.Id)
            {
                var property = wadObject.GetType().GetProperty("Id");
                property.SetValue(wadObject, newId);
            }

            // Add object
            if (newId is WadMoveableId)
            {
                var mov = ((WadMoveable)wadObject).Clone();
                mov.Meshes.ForEach(m => MergeSimilarTextures(m)); // Find texture duplicates
                Moveables[(WadMoveableId)newId] = mov;
            }
            else if (newId is WadStaticId)
            {
                var st = ((WadStatic)wadObject).Clone();
                MergeSimilarTextures(st.Mesh); // Find texture duplicates
                Statics[(WadStaticId)newId] = st;
            }
            else if (newId is WadSpriteSequenceId)
            {
                var sp = ((WadSpriteSequence)wadObject).Clone();
                SpriteSequences[(WadSpriteSequenceId)newId] = sp;
            }
            else
                throw new ArgumentException("Argument not of a valid type.");
        }

        public bool Contains(IWadObjectId wadObjectId)
        {
            return TryGet(wadObjectId) != null;
        }

        public bool Contains(IWadObject wadObject)
        {
            if (wadObject is WadMoveable)
                return Moveables.Any(obj => obj.Value == (WadMoveable)wadObject);
            else if (wadObject is WadStatic)
                return Statics.Any(obj => obj.Value == (WadStatic)wadObject);
            else if (wadObject is WadSpriteSequence)
                return SpriteSequences.Any(obj => obj.Value == (WadSpriteSequence)wadObject);
            else
                throw new ArgumentException("Argument not of a valid type.");
        }

        public void AssignNewId(IWadObjectId oldId, IWadObjectId newId)
        {
            if (Contains(newId))
                throw new ArgumentException("Id " + newId.ToString(GameVersion) + " already exists.");
            IWadObject @object = TryGet(oldId);
            if (@object == null)
                throw new KeyNotFoundException("Id " + newId.ToString(GameVersion) + " not found.");
            Remove(oldId);
            Add(newId, @object);
        }

        public void MergeSimilarTextures(WadMesh mesh)
        {
            var textures = MeshTexturesUnique;

            for (int i = 0; i < mesh.Polys.Count; i++)
            {
                var poly = mesh.Polys[i];
                if (textures.Contains(poly.Texture.Texture))
                    poly.Texture.Texture = textures.First(t => t.GetHashCode() == poly.Texture.Texture.GetHashCode());
                mesh.Polys[i] = poly;
            }
        }

        public static IReadOnlyList<FileFormat> FileExtensions { get; } = new List<FileFormat>()
        {
            new FileFormat("Winroomedit WAD", "wad"),
            new FileFormat("TombEditor WAD2", "wad2"),
            new FileFormat("Tomb Raider I level", "phd"),
            new FileFormat("Tomb Raider II/III level", "tr2"),
            new FileFormat("Tomb Raider The Last Revelation level", "tr4"),
            new FileFormat("Tomb Raider Chronicles level", "trc")
        };

        private class PackedTexturePlacement
        {
            public WadTexture.AtlasReference TextureReference { get; set; }
            public VectorInt2 Position { get; set; }
            public int PaddingX { get; set; }
            public int PaddingY { get; set; }
        }

        private class PackedTexturePage
        {
            public int Scale { get; set; }
            public List<PackedTexturePlacement> Placements { get; } = new List<PackedTexturePlacement>();
        }

        public static List<WadTexture> PackTexturesForExport(Dictionary<Hash, WadTexture.AtlasReference> texturesToPack, int padding, int texturePageSize)
        {
            var textures = new List<WadTexture>();
            var scale = Math.Clamp(texturePageSize, 1, 2048);

            var remainingTextures = texturesToPack.Values.ToList();

            while (remainingTextures.Count > 0)
            {
                var page = PackTexturePage(remainingTextures, padding, scale);
                if (page == null || page.Placements.Count == 0)
                    throw new InvalidOperationException("Unable to pack textures into the requested export page size.");

                var atlasImage = ImageC.CreateNew(page.Scale, page.Scale);
                atlasImage.Fill(new ColorC(0, 0, 0, 0));

                foreach (var placement in page.Placements)
                {
                    DrawTextureToAtlas(atlasImage, placement);
                    placement.TextureReference.Position = new VectorInt2(placement.Position.X + placement.PaddingX, placement.Position.Y + placement.PaddingY);
                    placement.TextureReference.Atlas = textures.Count;
                }

                var atlas = new WadTexture(atlasImage);
                textures.Add(atlas);
                remainingTextures.RemoveRange(0, page.Placements.Count);
            }

            return textures;
        }

        private static PackedTexturePage PackTexturePage(IReadOnlyList<WadTexture.AtlasReference> texturesToPack, int padding, int maxScale)
        {
            PackedTexturePage bestPage = null;

            foreach (var candidateScale in GetCandidateScales(maxScale))
            {
                var candidatePage = TryPackTexturePage(texturesToPack, padding, candidateScale);
                if (candidatePage.Placements.Count == 0)
                    continue;

                if (bestPage == null ||
                    candidatePage.Placements.Count > bestPage.Placements.Count ||
                    (candidatePage.Placements.Count == bestPage.Placements.Count && candidatePage.Scale < bestPage.Scale))
                    bestPage = candidatePage;
            }

            return bestPage;
        }

        private static PackedTexturePage TryPackTexturePage(IReadOnlyList<WadTexture.AtlasReference> texturesToPack, int padding, int scale)
        {
            var page = new PackedTexturePage { Scale = scale };
            var packer = new RectPackerTree(new VectorInt2(scale, scale));

            for (int i = 0; i < texturesToPack.Count; i++)
            {
                var textureRef = texturesToPack[i];
                var size = GetPaddedTextureSize(textureRef.Texture, padding, scale, out int paddingX, out int paddingY);
                var result = packer.TryAdd(size);

                if (!result.HasValue)
                    break;

                page.Placements.Add(new PackedTexturePlacement
                {
                    TextureReference = textureRef,
                    Position = result.Value,
                    PaddingX = paddingX,
                    PaddingY = paddingY
                });
            }

            return page;
        }

        private static VectorInt2 GetPaddedTextureSize(WadTexture texture, int padding, int scale, out int paddingX, out int paddingY)
        {
            paddingX = padding;
            if (texture.Image.Width + 2 * paddingX >= scale)
                paddingX = Math.Max(0, (int)Math.Floor((float)(scale - texture.Image.Width) / 2));

            paddingY = padding;
            if (texture.Image.Height + 2 * paddingY >= scale)
                paddingY = Math.Max(0, (int)Math.Floor((float)(scale - texture.Image.Height) / 2));

            return texture.Image.Size + new VectorInt2(paddingX * 2, paddingY * 2);
        }

        private static IEnumerable<int> GetCandidateScales(int maxScale)
        {
            yield return maxScale;

            for (int candidateScale = GetPreviousPowerOfTwo(maxScale); candidateScale > 0; candidateScale /= 2)
                if (candidateScale != maxScale)
                    yield return candidateScale;
        }

        private static int GetPreviousPowerOfTwo(int value)
        {
            int result = 1;

            while (result <= value / 2)
                result *= 2;

            return result;
        }

        private static void DrawTextureToAtlas(ImageC atlasImage, PackedTexturePlacement placement)
        {
            var texture = placement.TextureReference.Texture;

            for (int p = 0; p < placement.PaddingX; p++)
                atlasImage.CopyFrom(placement.Position.X + p, placement.Position.Y + placement.PaddingY, texture.Image, 0, 0, 1, texture.Image.Height);

            for (int p = 0; p < placement.PaddingX; p++)
                atlasImage.CopyFrom(placement.Position.X + placement.PaddingX + texture.Image.Width + p, placement.Position.Y + placement.PaddingY, texture.Image, texture.Image.Width - 1, 0, 1, texture.Image.Height);

            for (int p = 0; p < placement.PaddingY; p++)
                atlasImage.CopyFrom(placement.Position.X + placement.PaddingX, placement.Position.Y + p, texture.Image, 0, 0, texture.Image.Width, 1);

            for (int p = 0; p < placement.PaddingY; p++)
                atlasImage.CopyFrom(placement.Position.X + placement.PaddingX, placement.Position.Y + placement.PaddingY + texture.Image.Height + p, texture.Image, 0, texture.Image.Height - 1, texture.Image.Width, 1);

            var color = texture.Image.GetPixel(0, 0);
            for (int px = 0; px < placement.PaddingX; px++)
                for (int py = 0; py < placement.PaddingY; py++)
                    atlasImage.SetPixel(placement.Position.X + px, placement.Position.Y + py, color);

            color = texture.Image.GetPixel(texture.Image.Width - 1, 0);
            for (int px = 0; px < placement.PaddingX; px++)
                for (int py = 0; py < placement.PaddingY; py++)
                    atlasImage.SetPixel(placement.Position.X + texture.Image.Width + placement.PaddingX + px, placement.Position.Y + py, color);

            color = texture.Image.GetPixel(texture.Image.Width - 1, texture.Image.Height - 1);
            for (int px = 0; px < placement.PaddingX; px++)
                for (int py = 0; py < placement.PaddingY; py++)
                    atlasImage.SetPixel(placement.Position.X + texture.Image.Width + placement.PaddingX + px, placement.Position.Y + texture.Image.Height + placement.PaddingY + py, color);

            color = texture.Image.GetPixel(0, texture.Image.Height - 1);
            for (int px = 0; px < placement.PaddingX; px++)
                for (int py = 0; py < placement.PaddingY; py++)
                    atlasImage.SetPixel(placement.Position.X + px, placement.Position.Y + texture.Image.Height + placement.PaddingY + py, color);

            atlasImage.CopyFrom(placement.Position.X + placement.PaddingX, placement.Position.Y + placement.PaddingY, texture.Image);
        }
    }
}
