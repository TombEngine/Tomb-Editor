using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib.LevelData.SectorEnums;
using TombLib.Utils;
using TombLib.Utils.ImageQuantizer;

namespace TombLib.LevelData.Compilers
{
    public sealed partial class LevelCompilerClassicTR
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private int PrepareTextures()
        {
            // It's fine using the old unpadded way for sprites, they are quad
            List<ImageC> spritePages = BuildSprites(_textureInfoManager.NumRoomPages + _textureInfoManager.NumObjectsPages);

            // Get the final number of pages
            int numPages = _textureInfoManager.NumRoomPages + _textureInfoManager.NumObjectsPages +
                spritePages.Count + _textureInfoManager.NumBumpPages * 2;

            ReportProgress(60, "Building final texture map");

            byte[] texture32Data = new byte[numPages * 256 * 256 * 4];
            int totalPages = 0;

            _textureInfoManager.RoomPages.RawCopyTo(texture32Data, totalPages * 256 * 256 * 4);
            totalPages += _textureInfoManager.NumRoomPages;

            _textureInfoManager.ObjectsPages.RawCopyTo(texture32Data, totalPages * 256 * 256 * 4);
            totalPages += _textureInfoManager.NumObjectsPages;

            for (int i = 0; i < spritePages.Count; i++)
                spritePages[i].RawCopyTo(texture32Data, (totalPages + i) * 256 * 256 * 4);
            totalPages += spritePages.Count;

            _textureInfoManager.BumpPages.RawCopyTo(texture32Data, totalPages * 256 * 256 * 4);

            _texture32Data = texture32Data;

            // I need to update the bumped tiles
            _textureInfoManager.UpdateTiles(spritePages.Count);

            // DEBUG: dump the texture map
            //var tempImage = ImageC.FromByteArray(texture32Data, 256, numPages * 256);
            //tempImage.Save("H:\\testpack.jpg");

            ReportProgress(70, "    Num room pages: " + _textureInfoManager.NumRoomPages);
            ReportProgress(70, "    Num objects pages: " + _textureInfoManager.NumObjectsPages);
            ReportProgress(70, "    Num bumpmap pages: " + _textureInfoManager.NumBumpPages);

            return numPages;
        }

        private TextureFootStep.Type? GetTextureSound(bool isTriangle, TextureArea area)
        {
            LevelTexture texture = area.Texture as LevelTexture;
            if (area.TextureIsInvisible || area.TextureIsUnavailable || area.TextureIsDegenerate || texture == null)
                return null;

            // Top right position for now
            Vector2 topRight = Vector2.Min(Vector2.Min(area.TexCoord0, area.TexCoord1), isTriangle ? area.TexCoord2 : Vector2.Min(area.TexCoord2, area.TexCoord3));
            return texture.GetTextureSoundFromTexCoord(topRight);
        }

        private TextureFootStep.Type GetTextureSound(Room room, int x, int z)
        {
            Sector sector = room.Sectors[x, z];

            TextureFootStep.Type? result0 = GetTextureSound(!sector.Floor.IsQuad, sector.GetFaceTexture(SectorFace.Floor));
            if (result0.HasValue)
                return result0.Value;

            TextureFootStep.Type? result1 = GetTextureSound(!sector.Floor.IsQuad, sector.GetFaceTexture(SectorFace.Floor_Triangle2));
            if (result1.HasValue)
                return result1.Value;

            return TextureFootStep.Type.Stone;
        }

        private Stream PrepareFontAndSkyTexture()
        {
            ReportProgress(96, "Building font & sky textures");

            var image = ImageC.CreateNew(256, _level.Settings.GameVersion == TRVersion.Game.TR5 ? 768 : 512);
            int toY = 0;

            // Read extra textures
            if (_level.Settings.GameVersion == TRVersion.Game.TR5)
            {
                string extraFileName = _level.Settings.MakeAbsolute(_level.Settings.Tr5ExtraSpritesFilePath);
                if (!string.IsNullOrEmpty(extraFileName) && !File.Exists(extraFileName))
                {
                    _progressReporter.ReportWarn("Specified extra TR5 texture not found, using default.");
                    extraFileName = null;
                }
                else
                    ReportProgress(96, "Reading extra TR5 texture: " + extraFileName);
                image.CopyFrom(0, toY, _level.Settings.LoadTr5ExtraSprites(extraFileName));
                toY += 256;
            }

            // Read font texture
            string fontFileName = _level.Settings.MakeAbsolute(_level.Settings.FontTextureFilePath);
            if (!string.IsNullOrEmpty(fontFileName) && !File.Exists(fontFileName))
            {
                _progressReporter.ReportWarn("Specified font texture not found, using default.");
                fontFileName = null;
            }
            else
                ReportProgress(96, "Reading font texture: " + fontFileName);
            image.CopyFrom(0, toY, _level.Settings.LoadFontTexture(fontFileName));
            toY += 256;


            // Read sky texture
            string skyFileName = _level.Settings.MakeAbsolute(_level.Settings.SkyTextureFilePath);
            if (!string.IsNullOrEmpty(skyFileName) && !File.Exists(skyFileName))
            {
                _progressReporter.ReportWarn("Specified sky texture not found, using default.");
                skyFileName = null;
            }
            else
                ReportProgress(96, "Reading sky texture: " + skyFileName);
            image.CopyFrom(0, toY, _level.Settings.LoadSkyTexture(skyFileName));

            return image.ToRawStream();
        }

        private List<ImageC> BuildSprites(int pagesBeforeSprites)
        {
            ReportProgress(59, "Building sprites");
            var spriteSequences = _level.Settings.WadGetAllSpriteSequences();

            // Add all sprites to the texture packer
            var spriteAllocator = new Util.SpriteAllocator();
            var spriteTextureIDs = new Dictionary<Hash, int>();
            foreach (var sprite in spriteSequences.Values.SelectMany(sequence => sequence.Sprites))
                if (!spriteTextureIDs.ContainsKey(sprite.Texture.Hash))
                    spriteTextureIDs.Add(sprite.Texture.Hash, spriteAllocator.GetOrAllocateTextureID(sprite.Texture));

            // Pack textures
            List<ImageC> texturePages = spriteAllocator.PackTextures();

            // Now build data structures
            var tempSequences = new List<tr_sprite_sequence>();
            var tempSprites = new List<tr_sprite_texture>();
            var lastOffset = 0;

            foreach (var sequence in spriteSequences.Values)
            {
                var newSequence = new tr_sprite_sequence();
                newSequence.NegativeLength = (short)-sequence.Sprites.Count;
                newSequence.ObjectID = (int)sequence.Id.TypeId;
                newSequence.Offset = (short)lastOffset;

                foreach (var sprite in sequence.Sprites)
                {
                    var id = spriteTextureIDs[sprite.Texture.Hash];
                    if (id == -1)
                    {
                        _progressReporter.ReportWarn("Sprite #" + sequence.Sprites.IndexOf(sprite) +
                            " in sequence #" + sequence.Id.TypeId + " wasn't added: size is too big or coordinates are invalid.");
                        continue;
                    }

                    var packInfo = spriteAllocator.GetPackInfo(id);
                    var newTexture = new tr_sprite_texture();

                    if (_level.Settings.GameVersion.Native() <= TRVersion.Game.TR3)
                    {
                        ushort texW = (ushort)sprite.Texture.Image.Width;
                        ushort texH = (ushort)sprite.Texture.Image.Height;
                        ushort SpriteW = (ushort)((texW - 1) * 256 + 255);
                        ushort SpriteH = (ushort)((texH - 1) * 256 + 255);
                        newTexture.X = (byte)packInfo.Pos.X;
                        newTexture.Y = (byte)packInfo.Pos.Y;
                        newTexture.Width = SpriteW;
                        newTexture.Height = SpriteH;
                        newTexture.TopSide = (short)sprite.Alignment.Y0;
                        newTexture.LeftSide = (short)sprite.Alignment.X0;
                        newTexture.RightSide = (short)sprite.Alignment.X1;
                        newTexture.BottomSide = (short)sprite.Alignment.Y1;
                    }
                    else
                    {
                        newTexture.TopSide = (short)packInfo.Pos.Y;
                        newTexture.LeftSide = (short)packInfo.Pos.X;
                        newTexture.X = (byte)newTexture.LeftSide;
                        newTexture.Y = (byte)newTexture.TopSide;
                        newTexture.Width = (ushort)((sprite.Texture.Image.Width - 1) * 256);
                        newTexture.Height = (ushort)((sprite.Texture.Image.Height - 1) * 256);
                        newTexture.RightSide = (short)(newTexture.LeftSide + sprite.Texture.Image.Width);
                        newTexture.BottomSide = (short)(newTexture.TopSide + sprite.Texture.Image.Height);
                    }
                    newTexture.Tile = (ushort)(pagesBeforeSprites + packInfo.OutputTextureID);

                    tempSprites.Add(newTexture);
                }

                tempSequences.Add(newSequence);
                lastOffset += sequence.Sprites.Count;
            }

            _spriteSequences = tempSequences;
            _spriteTextures = tempSprites;

            return texturePages;
        }

        private static byte[] PackTextureMap32To16Bit(byte[] textureData, LevelSettings settings)
        {
            if (settings.Dither16BitTextures && !settings.FastMode)
                return PackTextureMap32To16BitDithered(textureData, 256);
            else
                return PackTextureMap32To16Bit(textureData);
        }

        private static byte[] PackTextureMap32To16Bit(byte[] textureData)
        {
            int pixelCount = textureData.Length / 4;
            byte[] newTextureData = new byte[pixelCount * 2];
            for (int i = 0; i < pixelCount; i++)
            {
                ushort a1 = textureData[i * 4 + 3];
                ushort r1 = textureData[i * 4 + 2];
                ushort g1 = textureData[i * 4 + 1];
                ushort b1 = textureData[i * 4 + 0];

                var r = (ushort)(r1 / 8);
                var g = (ushort)(g1 / 8);
                var b = (ushort)(b1 / 8);
                ushort a;

                if (a1 == 0)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                    a = 0;
                }
                else
                {
                    if (a1 > 0 && a1 < 255)
                    {
                        r = (byte)(r * (a1 / 255.0f));
                        g = (byte)(g * (a1 / 255.0f));
                        b = (byte)(b * (a1 / 255.0f));
                    }
                    a = 0x8000;
                }

                if (r1 < 8) r = 0;
                if (g1 < 8) g = 0;
                if (b1 < 8) b = 0;

                ushort tmp = 0;

                if (r1 == 255 && g1 == 255 && b1 == 255)
                {
                    tmp = 0xffff;
                }
                else
                {
                    tmp |= a;
                    tmp |= (ushort)(r << 10);
                    tmp |= (ushort)(g << 5);
                    tmp |= b;
                }

                newTextureData[i * 2] = (byte)(tmp & 0xff);
                newTextureData[i * 2 + 1] = (byte)((tmp & 0xff00) >> 8);
            }
            return newTextureData;
        }

        private static byte[] PackTextureMap32To16BitDithered(byte[] textureData, int pageSize)
        {
            // Stucki dithering matrix, it produces better result than Floyd-Steinberg
            // with gradients
            var ditherMatrix = new byte[,]
            { { 0, 0, 0, 8, 4 },
              { 2, 4, 8, 4, 2 },
              { 1, 2, 4, 2, 1 } };

            var seed = new Random(31459);
            int pixelCount = textureData.Length / 4;
            var height = pixelCount / pageSize;
            byte[] newTextureData = new byte[pixelCount * 2];

            for (int i = 0; i < pixelCount; i++)
            {
                int r1 = textureData[i * 4 + 2];
                int g1 = textureData[i * 4 + 1];
                int b1 = textureData[i * 4 + 0];
                int r2 = (byte)(r1 >> 3) << 3;
                int g2 = (byte)(g1 >> 3) << 3;
                int b2 = (byte)(b1 >> 3) << 3;
                int rE = r1 - r2;
                int bE = g1 - g2;
                int gE = b1 - b2;

                for (int row = 0; row < 3; row++)
                {
                    int offsetY = (i / pageSize) + row;

                    for (int col = 0; col < 5; col++)
                    {
                        int coefficient = ditherMatrix[row, col];
                        int offsetX = (i % pageSize) + (col - 4);

                        if (coefficient != 0 && offsetX >= 0 && offsetX < pageSize && offsetY >= 0 && offsetY < height)
                        {
                            // Add some noise to coefficient to reduce banding
                            float finalCoeff = 42 - (seed.Next(0, 15));

                            int offsetIndex = (offsetY * pageSize + offsetX) * 4;
                            int newR = (int)((rE * coefficient) / finalCoeff);
                            int newG = (int)((gE * coefficient) / finalCoeff);
                            int newB = (int)((bE * coefficient) / finalCoeff);

                            byte a = (byte)MathC.Clamp((textureData[offsetIndex + 3]), 0, 255);
                            byte r = (byte)MathC.Clamp((textureData[offsetIndex + 2] + newR), 0, 255);
                            byte g = (byte)MathC.Clamp((textureData[offsetIndex + 1] + newG), 0, 255);
                            byte b = (byte)MathC.Clamp((textureData[offsetIndex + 0] + newB), 0, 255);

                            if (r < 8 || a == 0) r = 0;
                            if (g < 8 || a == 0) g = 0;
                            if (b < 8 || a == 0) b = 0;

                            if (a > 0 && a < 255)
                            {
                                // Convert true alpha to brightness with slight noise to prevent banding
                                a -= (byte)seed.Next(0, MathC.Clamp((255 - a) / 20, 0, a));
                                r = (byte)(r * (a / 255.0f));
                                g = (byte)(g * (a / 255.0f));
                                b = (byte)(b * (a / 255.0f));
                            }

                            r /= 8;
                            g /= 8;
                            b /= 8;

                            ushort tmp = 0;

                            if (r == 255 && g == 255 && b == 255)
                                tmp = 0xffff;
                            else
                            {
                                tmp |= (ushort)(a == 0 ? 0 : 0x8000);
                                tmp |= (ushort)(r << 10);
                                tmp |= (ushort)(g << 5);
                                tmp |= (ushort)b;
                            }

                            newTextureData[offsetIndex / 2] = (byte)((tmp & 0x00ff));
                            newTextureData[offsetIndex / 2 + 1] = (byte)((tmp & 0xff00) >> 8);
                        }
                    }
                }
            }
            return newTextureData;
        }

        private static byte[] PackTextureMap32To8Bit(byte[] textureData, out tr_color[] palette) =>
            PackTextureMap32To8Bit(textureData, new List<Color>(), out palette);
        private static byte[] PackTextureMap32To8Bit(byte[] textureData, List<Color> predefinedColors, out tr_color[] palette)
        {
            int offset = predefinedColors.Count;
            int colorCount = 256 - offset;
            int pixelCount = textureData.Length / 4;

            // Initialize quantizer
            var quantizer = new OctreeQuantizer();
            quantizer.Clear();

            // Put palette indices into texture data array
            var result = new byte[textureData.Length / 4];

            // Create temporary bitmap out of texture data
            using (var bitmap = ImageC.FromByteArray(textureData, 256, textureData.Length / 4 / 256).ToBitmap())
            {
                ((Image)bitmap).AddColorsToQuantizer(quantizer);

                // Get palette and merge with predefined colors
                var newPalette = predefinedColors.Concat(quantizer.GetPalette(colorCount)).ToList();

                for (int x = 0; x < 256; x++)
                    for (int y = 0; y < pixelCount / 256; y++)
                    {
                        var current = bitmap.GetPixel(x, y);
                        int paletteIndex;

                        if (current.A == 0)
                            paletteIndex = 0;
                        else
                            paletteIndex = quantizer.GetPaletteIndex(QuantizationHelper.ConvertAlpha(current)) + offset;

                        result[y * 256 + x] = (byte)paletteIndex;
                    }

                // Convert system palette to TR palette
                palette = new tr_color[256];
                for (int i = 0; i < palette.Length; i++)
                {
                    if (i < offset)
                    {
                        palette[i].Red = newPalette[i].R;
                        palette[i].Green = newPalette[i].G;
                        palette[i].Blue = newPalette[i].B;
                    }
                    else if (i < newPalette.Count)
                    {
                        palette[i].Red = (byte)(newPalette[i].R / 4);
                        palette[i].Green = (byte)(newPalette[i].G / 4);
                        palette[i].Blue = (byte)(newPalette[i].B / 4);
                    }
                }
            }

            return result;
        }

        private byte[] CalculateLightmap(tr_color[] palette)
        {
            var result = new byte[palette.Length * 32];
            for (int k = 0; k < 32; k++)
            {
                var bias  = _level.Settings.GameVersion.Native() == TRVersion.Game.TR1 ? k + 1 : k;
                var coeff = Math.Min(Math.Max(2.0f - bias / 16.0f, 0), 1.5f);

                for (int i = 0; i < palette.Length; i++)
                {
                    if (_level.Settings.FastMode)
                        result[palette.Length * k + i] = (byte)i;
                    else
                    {
                        var newR = (byte)Math.Min(palette[i].Red * coeff, byte.MaxValue);
                        var newG = (byte)Math.Min(palette[i].Green * coeff, byte.MaxValue);
                        var newB = (byte)Math.Min(palette[i].Blue * coeff, byte.MaxValue);
                        var candidate = new tr_color() { Red = newR, Green = newG, Blue = newB };
                        result[palette.Length * k + i] = (byte)FindClosestColor(candidate, palette);
                    }
                }
            }
            return result;
        }

        private static int FindClosestColor(tr_color color, tr_color[] palette)
        {
            int colorReturn = 0;

            int biggestDifference = 1000;
            for (int i = 0; i < palette.Length; i++)
            {
                var match = Math.Sqrt(Math.Pow(color.Red   - palette[i].Red,   2) + 
                                      Math.Pow(color.Green - palette[i].Green, 2) + 
                                      Math.Pow(color.Blue  - palette[i].Blue,  2));

                if (match < biggestDifference)
                {
                    colorReturn = i;
                    biggestDifference = (int)match;
                }
            }

            return colorReturn;
        }
    }
}
