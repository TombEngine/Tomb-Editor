using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public class LevelTexture : Texture, IReloadableResource, IEquatable<LevelTexture>
    {
        public class UniqueIDType { }
        public UniqueIDType UniqueID { get; private set; } = new UniqueIDType();

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public const float FootStepSoundGranularity = 64.0f;
        public const float BumpMappingGranularity = 64.0f;

        public string Path { get; private set; }

        public bool Convert512PixelsToDoubleRows { get; private set; }
        public bool ReplaceMagentaWithTransparency { get; private set; }

        private TextureFootStep.Type[,] _footStepSounds = new TextureFootStep.Type[0, 0];

        public string BumpPath;
        private BumpMappingLevel[,] _bumpMappingLevel = new BumpMappingLevel[0, 0];

        public LevelTexture()
        { }

        public LevelTexture(LevelSettings settings, string path, bool convert512PixelsToDoubleRows = false, bool replaceWithTransparency = true)
        {
            Convert512PixelsToDoubleRows = convert512PixelsToDoubleRows;
            ReplaceMagentaWithTransparency = replaceWithTransparency;
            SetPath(settings, path);
            BumpPath = "";
        }

        public override Texture Clone()
        {
            return new LevelTexture
            {
                UniqueID = UniqueID,
                Image = Image,
                Path = Path,
                BumpPath = BumpPath,
                LoadException = LoadException,
                Convert512PixelsToDoubleRows = Convert512PixelsToDoubleRows,
                ReplaceMagentaWithTransparency = ReplaceMagentaWithTransparency,
                _footStepSounds = _footStepSounds,
                _bumpMappingLevel = _bumpMappingLevel
            };
        }

        public void Assign(LevelTexture other)
        {
            Image = other.Image;
            Path = other.Path;
            BumpPath = other.BumpPath;
            LoadException = other.LoadException;
            Convert512PixelsToDoubleRows = other.Convert512PixelsToDoubleRows;
            ReplaceMagentaWithTransparency = other.ReplaceMagentaWithTransparency;
            _footStepSounds = other._footStepSounds;
            _bumpMappingLevel = other._bumpMappingLevel;
        }

        public bool Equals(LevelTexture other)
        {
            if ((Path == null) != (other.Path == null))
                return false;
            if (!(Path == null && other.Path == null) && !Path.Equals(other?.Path, StringComparison.InvariantCultureIgnoreCase))
                return false;
            if (!(BumpPath == null && other.BumpPath == null) && !BumpPath.Equals(other?.BumpPath, StringComparison.InvariantCultureIgnoreCase))
                return false;
            if (Convert512PixelsToDoubleRows != other.Convert512PixelsToDoubleRows)
                return false;
            if (ReplaceMagentaWithTransparency != other.ReplaceMagentaWithTransparency)
                return false;
            if (FootStepSoundWidth != other.FootStepSoundWidth || FootStepSoundHeight != other.FootStepSoundHeight)
                return false;
            for (int x = 0; x < FootStepSoundWidth; ++x)
                for (int y = 0; y < FootStepSoundHeight; ++y)
                    if (_footStepSounds[x, y] != other._footStepSounds[x, y])
                        return false;
            if (BumpMappingWidth != other.BumpMappingWidth || BumpMappingHeight != other.BumpMappingHeight)
                return false;
            for (int x = 0; x < BumpMappingWidth; ++x)
                for (int y = 0; y < BumpMappingHeight; ++y)
                    if (_bumpMappingLevel[x, y] != other._bumpMappingLevel[x, y])
                        return false;
            return true;
        }

        public override bool Equals(object other) => other is LevelTexture && Equals((LevelTexture)other);

        public override int GetHashCode() => Path == null ? 0 : (Path + BumpPath).GetHashCode();

        public override string ToString()
        {
            string Filename = System.IO.Path.GetFileNameWithoutExtension(Path);

            if (String.IsNullOrEmpty(Filename) && String.IsNullOrEmpty(Path))
                return "<Unloaded placeholder>";
            else
                return Filename + " (at " + Path + ")";
        }

        public void Reload(LevelSettings settings)
        {
            LoadException = null;
            if (string.IsNullOrEmpty(Path))
            {
                Image = UnloadedPlaceholder;
                return;
            }

            // Load image
            try
            {
                ImageC image = ImageC.FromFile(settings.MakeAbsolute(Path));

                if (Convert512PixelsToDoubleRows && image.Width == 512)
                {
                    ImageC newImage = ImageC.CreateNew(256, image.Height * 2);
                    newImage.FileName = image.FileName;
                    for (int oldY = 0; oldY < image.Height; oldY += 64)
                    {
                        newImage.CopyFrom(0, oldY * 2, image, 0, oldY, 256, 64);
                        newImage.CopyFrom(0, oldY * 2 + 64, image, 256, oldY, 256, 64);
                    }
                    image = newImage;
                }

                if (ReplaceMagentaWithTransparency)
                    image.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));

                image.CalculatePalette();
                Image = image;

                // Resize sound array
                ResizeFootStepSounds(
                    (int)Math.Ceiling(Image.Width / FootStepSoundGranularity),
                    (int)Math.Ceiling(Image.Height / FootStepSoundGranularity));

                // Resize bump maps
                 ResizeBumpMappingInfos(
                    (int)Math.Ceiling(Image.Width / BumpMappingGranularity),
                    (int)Math.Ceiling(Image.Height / BumpMappingGranularity));
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "Unable to load texture '" + Path + "'.");
                Image = UnloadedPlaceholder;
                LoadException = exc;
            }
        }

        public ReloadableResourceType ResourceType { get { return ReloadableResourceType.Texture; } }
        public Exception LoadException { get; set; }
        public IEnumerable<FileFormat> FileExtensions => ImageC.FileExtensions;
        public List<IReloadableResource> GetResourceList(LevelSettings settings) => settings.Textures.Select(i => i as IReloadableResource).ToList();

        public string GetPath() => Path;
        public void SetPath(LevelSettings settings, string path)
        {
            Path = path;
            AbsolutePath = settings.MakeAbsolute(path);
            Reload(settings);
        }

        public void SetConvert512PixelsToDoubleRows(LevelSettings settings, bool value)
        {
            if (Convert512PixelsToDoubleRows == value)
                return;
            Convert512PixelsToDoubleRows = value;
            Reload(settings);
        }

        public void SetReplaceMagentaWithTransparency(LevelSettings settings, bool value)
        {
            if (ReplaceMagentaWithTransparency == value)
                return;
            ReplaceMagentaWithTransparency = value;
            Reload(settings);
        }

        public TextureFootStep.Type GetFootStepSound(int x, int y)
        {
            return _footStepSounds[x, y];
        }

        public void SetFootStepSound(int x, int y, TextureFootStep.Type value)
        {
            _footStepSounds[x, y] = value;
        }

        public TextureFootStep.Type? GetTextureSoundFromTexCoord(Vector2 coord)
        {
            coord /= FootStepSoundGranularity;
            if (!(coord.X >= 0 && coord.Y >= 0 && coord.X < FootStepSoundWidth && coord.Y < FootStepSoundHeight))
                return null;
            return _footStepSounds[(int)coord.X, (int)coord.Y];
        }

        public int FootStepSoundWidth => _footStepSounds.GetLength(0);
        public int FootStepSoundHeight => _footStepSounds.GetLength(1);

        public void ResizeFootStepSounds(int width, int height)
        {
            int requiredTextureSoundWidth = Math.Max(FootStepSoundWidth, width);
            int requiredTextureSoundHeight = Math.Max(FootStepSoundHeight, height);
            if (FootStepSoundWidth < requiredTextureSoundWidth ||
                FootStepSoundHeight < requiredTextureSoundHeight)
            {
                var newTextureSounds = new TextureFootStep.Type[requiredTextureSoundWidth, requiredTextureSoundHeight];
                for (int y = 0; y < requiredTextureSoundHeight; ++y)
                    for (int x = 0; x < requiredTextureSoundWidth; ++x)
                        newTextureSounds[x, y] = TextureFootStep.Type.Stone;
                for (int y = 0; y < FootStepSoundHeight; ++y)
                    for (int x = 0; x < FootStepSoundWidth; ++x)
                        newTextureSounds[x, y] = _footStepSounds[x, y];
                _footStepSounds = newTextureSounds;
            }
        }

        public BumpMappingLevel GetBumpMapLevel(int x, int y)
        {
            return _bumpMappingLevel[x, y];
        }

        public void SetBumpMappingLevel(int x, int y, BumpMappingLevel info)
        {
            _bumpMappingLevel[x, y] = info;
        }

        public BumpMappingLevel? GetBumpMappingLevelFromTexCoord(Vector2 coord)
        {
            coord /= BumpMappingGranularity;
            if (!(coord.X >= 0 && coord.Y >= 0 && coord.X < BumpMappingWidth && coord.Y < BumpMappingHeight))
                return null;
            return _bumpMappingLevel[(int)coord.X, (int)coord.Y];
        }

        public int BumpMappingWidth => _bumpMappingLevel.GetLength(0);
        public int BumpMappingHeight => _bumpMappingLevel.GetLength(1);

        public void ResizeBumpMappingInfos(int width, int height)
        {
            int requiredBumpMappingWidth = Math.Max(BumpMappingWidth, width);
            int requiredBumpMappingHeight = Math.Max(BumpMappingHeight, height);
            if (BumpMappingWidth < requiredBumpMappingWidth ||
                BumpMappingHeight < requiredBumpMappingHeight)
            {
                var newBumpMappingInfos = new BumpMappingLevel[requiredBumpMappingWidth, requiredBumpMappingHeight];
                for (int y = 0; y < requiredBumpMappingHeight; ++y)
                    for (int x = 0; x < requiredBumpMappingWidth; ++x)
                        newBumpMappingInfos[x, y] = BumpMappingLevel.None;
                for (int y = 0; y < BumpMappingHeight; ++y)
                    for (int x = 0; x < BumpMappingWidth; ++x)
                        newBumpMappingInfos[x, y] = _bumpMappingLevel[x, y];
                _bumpMappingLevel = newBumpMappingInfos;
            }
        }

    }
}
