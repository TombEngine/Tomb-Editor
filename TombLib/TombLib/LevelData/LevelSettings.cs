using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using TombLib.LevelData.IO;
using TombLib.NG;
using TombLib.Utils;
using TombLib.Wad;
using ImportedGeometryUpdateInfo = System.Collections.Generic.KeyValuePair<TombLib.LevelData.ImportedGeometry, TombLib.LevelData.ImportedGeometryInfo>;

namespace TombLib.LevelData
{
    public enum VariableType
    {
        [Description("The directory of the main level file (*.prj2).")]
        LevelDirectory,
        [Description("The directory of the .txt files for script")]
        ScriptDirectory,
        [Description("The directory in which all game components reside.")]
        GameDirectory,
        [Description("The directory of the editor application.")]
        EditorDirectory,
        [Description("The name of the main level file (*.prj2).")]
        LevelName,
        [Description("The engine.")]
        EngineVersion,
        [Description("The sound engine (Outputs TR4 for the 'TRNG' engine).")]
        SoundEngineVersion
    }

    public class AutoStaticMeshMergeEntry : ICloneable, IEquatable<AutoStaticMeshMergeEntry>
    {
        public string StaticMesh => parent.WadTryGetStatic(new WadStaticId(meshId)).ToString(parent.GameVersion);

        private readonly LevelSettings parent;
        public uint meshId;
        public bool Merge { get; set; }
        public bool InterpretShadesAsEffect { get; set; }
        public bool TintAsAmbient { get; set; }
        public bool ClearShades { get; set; }
        public AutoStaticMeshMergeEntry(uint staticMesh, bool merge, bool interpretShadesAsEffect, bool tintAsAmbient, bool clearShades, LevelSettings parent)
        {
            meshId = staticMesh;
            this.parent = parent;
            Merge = merge;
            TintAsAmbient = tintAsAmbient;
            InterpretShadesAsEffect = interpretShadesAsEffect;
            ClearShades = clearShades;
        }

        public AutoStaticMeshMergeEntry Clone()
        {
            return (AutoStaticMeshMergeEntry)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public override int GetHashCode()
        {
            return ("merged" + meshId + ClearShades + InterpretShadesAsEffect + Merge + TintAsAmbient).GetHashCode();
        }

        public override bool Equals(object other)
        {
            return Equals(other as AutoStaticMeshMergeEntry);
        }

        public bool Equals(AutoStaticMeshMergeEntry other)
        {
            if (other == null)
            {
                return false;
            }

            return (other.meshId == meshId &&
                    other.ClearShades == ClearShades &&
                    other.InterpretShadesAsEffect == InterpretShadesAsEffect &&
                    other.Merge == Merge &&
                    other.TintAsAmbient == TintAsAmbient);
        }
    }

    public class WadSoundPath : ICloneable
    {
        public string Path { get; set; }

        public WadSoundPath(string path)
        {
            Path = path;
        }

        public WadSoundPath Clone()
        {
            return (WadSoundPath)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }

    public class LevelSettings : ICloneable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const string VariableBegin = "$(";
        public const string VariableEnd = ")";
        public static readonly char Dir = Path.DirectorySeparatorChar;

        // Identify if current level was loaded from prj2 with unknown chunks, possibly
        // indicating that data loss occured.
        public bool HasUnknownData { get; set; } = false;

        // Last used room
        public int LastSelectedRoom { get; set; } = 0;

        // Default texture to be used for new rooms
        public TextureArea DefaultTexture { get; set; } = TextureArea.None;

        // Sound system
        public SoundSystem SoundSystem { get; set; } = SoundSystem.Xml;
        public List<int> SelectedSounds { get; set; } = new List<int>();
        public List<WadSoundInfo> GlobalSoundMap
        {
            get
            {
                SortedDictionary<int, WadSoundInfo> soundmap = new SortedDictionary<int, WadSoundInfo>();

                // Loop through all reference classes, collecting sounds
                foreach (ReferencedSoundCatalog soundsRef in SoundCatalogs)
                {
                    if (soundsRef.LoadException == null)
                    {
                        foreach (WadSoundInfo sound in soundsRef.Sounds.SoundInfos)
                        {
                            if (!soundmap.ContainsKey(sound.Id))
                            {
                                soundmap.Add(sound.Id, sound);
                            }
                        }
                    }
                }

                return soundmap.Values.ToList();
            }
        }
        public bool AutoAssignSoundsIfNoSelection { get; set; } = true; // Autodetect and assign sounds if none selected

        // Default sound paths
        public List<WadSoundPath> WadSoundPaths { get; set; } = new List<WadSoundPath>
        {
            new WadSoundPath("Sounds"), // For directly loading wad files.
            new WadSoundPath("Sound"),
            new WadSoundPath("Sounds" + Dir + "Samples"),
            new WadSoundPath("Sound" + Dir + "Samples"),
            new WadSoundPath(".." + Dir + "Sounds"), // For directly loading wad files.
            new WadSoundPath(".." + Dir + "Sound"),
            new WadSoundPath(".." + Dir + "Sounds" + Dir + "Samples"),
            new WadSoundPath(".." + Dir + "Sound" + Dir + "Samples"),
            new WadSoundPath(".." + Dir + ".." + Dir + "Sounds"), // For directly loading wad files.
            new WadSoundPath(".." + Dir + ".." + Dir + "Sound"),
            new WadSoundPath(".." + Dir + ".." + Dir + "Sounds" + Dir + "Samples"),
            new WadSoundPath(".." + Dir + ".." + Dir + "Sound" + Dir + "Samples"),
            new WadSoundPath(VariableCreate(VariableType.LevelDirectory) + Dir + "Sound" + Dir + "Samples"),
            new WadSoundPath(VariableCreate(VariableType.LevelDirectory) + Dir + ".." + Dir + "Sound" + Dir + "Samples"),
            new WadSoundPath(VariableCreate(VariableType.LevelDirectory) + Dir + ".." + Dir + ".." + Dir + "Sound" + Dir + "Samples"),
            new WadSoundPath(VariableCreate(VariableType.GameDirectory) + Dir + "Sound" + Dir + "Samples"),
            new WadSoundPath(VariableCreate(VariableType.EditorDirectory) + Dir + "Sounds" + Dir + VariableCreate(VariableType.SoundEngineVersion) + Dir + "Samples"),
            new WadSoundPath(VariableCreate(VariableType.EditorDirectory) + Dir + "Sounds" + Dir + "Samples")
        };

        // Game version and version-specific settings
        public TRVersion.Game GameVersion { get; set; } = TRVersion.Game.TR4;
        public bool GameEnableQuickStartFeature { get; set; } = true;
        public bool? GameEnableExtraBlendingModes { get; set; } = null;
        public bool GameEnableExtraReverbPresets { get; set; } = false;

        // Paths
        public string LevelFilePath { get; set; } = null; // Can be null if the level has not been loaded from / saved to disk yet.
        public string FontTextureFilePath { get; set; } = null; // Can be null if the default should be used.
        public string SkyTextureFilePath { get; set; } = null; // Can be null if the default should be used.
        public string Tr5ExtraSpritesFilePath { get; set; } = null; // Can be null if the default should be used.
        public string ScriptDirectory { get; set; } = VariableCreate(VariableType.ScriptDirectory);
        public string GameDirectory { get; set; } = VariableCreate(VariableType.EditorDirectory) + Dir + "Game";
        public string GameLevelFilePath { get; set; } = VariableCreate(VariableType.GameDirectory) + Dir + "data" + Dir + VariableCreate(VariableType.LevelName) + ".tr4"; // Relative to "GameDirectory"
        public string GameExecutableFilePath { get; set; } = VariableCreate(VariableType.GameDirectory) + Dir + "Tomb4.exe"; // Relative to "GameDirectory"
        public string TenLuaScriptFile { get; set; } = VariableCreate(VariableType.ScriptDirectory) + Dir + VariableCreate(VariableType.LevelName) + ".lua";

        // All data lists
        public List<ReferencedWad> Wads { get; set; } = new List<ReferencedWad>();
        public List<LevelTexture> Textures { get; set; } = new List<LevelTexture>();
        public List<ReferencedSoundCatalog> SoundCatalogs { get; set; } = new List<ReferencedSoundCatalog>();
        public List<ImportedGeometry> ImportedGeometries { get; set; } = new List<ImportedGeometry>();
        public List<AutoStaticMeshMergeEntry> AutoStaticMeshMerges { get; set; } = new List<AutoStaticMeshMergeEntry>();
        public List<AnimatedTextureSet> AnimatedTextureSets { get; set; } = new List<AnimatedTextureSet>();
        public List<EventSet> GlobalEventSets { get; set; } = new List<EventSet>();
        public List<EventSet> VolumeEventSets { get; set; } = new List<EventSet>();
        public List<ColorC> Palette { get; set; } = LoadPalette(ResourcesC.ResourcesC.palette);

        // Light options
        public Vector3 DefaultAmbientLight { get; set; } = new Vector3(0.25f, 0.25f, 0.25f);
        public LightQuality DefaultLightQuality { get; set; } = LightQuality.Low;
        public bool OverrideIndividualLightQualitySettings { get; set; } = false;
        public bool Room32BitLighting { get; set; } = false;

        // Compiler options
        public bool RearrangeVerticalRooms { get; set; } = true;
        public bool AgressiveFloordataPacking { get; set; } = false;
        public bool AgressiveTexturePacking { get; set; } = false;
        public bool CompressTextures { get; set; } = false;
        public bool Dither16BitTextures { get; set; } = true;
        public bool RemapAnimatedTextures { get; set; } = true;
        public int TexturePadding { get; set; } = 8;
        public bool FastMode { get; set; } = false;
        public bool RemoveUnusedObjects { get; set; } = false;
        public bool EnableCustomSampleRate { get; set; } = false;
        public int CustomSampleRate { get; set; } = 44100;

        // For TR5 only
        public Tr5LaraType Tr5LaraType { get; set; } = Tr5LaraType.Normal;
        public Tr5WeatherType Tr5WeatherType { get; set; } = Tr5WeatherType.Normal;

        public LevelSettings Clone()
        {
            LevelSettings result = (LevelSettings)MemberwiseClone();
            result.Wads = Wads.ConvertAll(wad => wad.Clone());
            result.WadSoundPaths = WadSoundPaths.ConvertAll(soundPath => soundPath.Clone());
            result.SoundCatalogs = SoundCatalogs.ConvertAll(catalog => catalog.Clone());
            result.Textures = Textures.ConvertAll(texture => (LevelTexture)texture.Clone());
            result.AnimatedTextureSets = AnimatedTextureSets.ConvertAll(set => set.Clone());
            result.ImportedGeometries = ImportedGeometries.ConvertAll(geometry => geometry.Clone());
            result.AutoStaticMeshMerges = AutoStaticMeshMerges.ConvertAll(entry => entry.Clone());
            return result;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public static string VariableCreate(VariableType type)
        {
            return VariableBegin + type + VariableEnd;
        }

        public void ConvertLevelExtension()
        {
            string result = string.Empty;
            switch (GameVersion.Native())
            {
                case TRVersion.Game.TR1:
                    result = ".phd";
                    break;
                case TRVersion.Game.TR2:
                case TRVersion.Game.TR3:
                    result = ".tr2";
                    break;
                case TRVersion.Game.TR4:
                case TRVersion.Game.TRNG:
                default:
                    result = ".tr4";
                    break;
                case TRVersion.Game.TR5:
                    result = ".trc";
                    break;
                case TRVersion.Game.TombEngine:
                    result = ".ten";
                    break;
            }
            GameLevelFilePath = Path.ChangeExtension(GameLevelFilePath, result);
        }

        public List<string> GetRecursiveListOfSoundPaths()
        {
            var result = new List<string>();

            foreach (var path in WadSoundPaths)
            {
                string newPath = MakeAbsolute(path.Path);

                if ((newPath == null) || !Directory.Exists(newPath))
                    continue; // Avoid non-existing

                if (result.Any(s => s.Equals(newPath)))
                    continue; // Avoid duplicates

                result.AddRange(PathC.GetDirectories(newPath));
            }

            return result;
        }

        public List<string> GetListOfSoundtracks()
        {
            var path = Path.Combine(MakeAbsolute(GameDirectory), WadSounds.AudioFolder);

            if (!Directory.Exists(path))
                return new List<string>();

            var extList = Enum.GetValues(typeof(WadSounds.SoundtrackFormat)).Cast<object>().Select(s => s.ToString().ToLowerInvariant());

            return Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                            .Where(s => extList.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()))
                            .Select(s => TextExtensions.ToLinuxPath(Path.Combine(Path.GetDirectoryName(Path.GetRelativePath(path, s)), Path.GetFileNameWithoutExtension(s))))
                            .Distinct()
                            .ToList();
        }

        public List<string> GetListOfVideos()
        {
            var path = Path.Combine(MakeAbsolute(GameDirectory), WadVideos.VideoFolder);

            if (!Directory.Exists(path))
                return new List<string>();

            var extList = Enum.GetValues(typeof(WadVideos.VideoFormat)).Cast<object>().Select(s => s.ToString().ToLowerInvariant());

            return Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                            .Where(s => extList.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()))
                            .Select(s => TextExtensions.ToLinuxPath(Path.Combine(Path.GetDirectoryName(Path.GetRelativePath(path, s)), Path.GetFileNameWithoutExtension(s))))
                            .Distinct()
                            .ToList();
        }

        public string GetVariable(VariableType type)
        {
            string result;

            switch (type)
            {
                case VariableType.EditorDirectory:
                    result = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    break;
                case VariableType.ScriptDirectory:
                    result = GetVariable(VariableType.GameDirectory) + Dir + "Scripts";
                    break;
                case VariableType.LevelDirectory:
                    if (!string.IsNullOrEmpty(LevelFilePath))
                    {
                        result = Path.GetDirectoryName(LevelFilePath);
                    }
                    else
                    {
                        result = GetVariable(VariableType.EditorDirectory);
                    }

                    break;
                case VariableType.GameDirectory:
                    result = MakeAbsolute(GameDirectory ?? VariableCreate(VariableType.LevelDirectory), VariableType.GameDirectory);
                    break;
                case VariableType.LevelName:
                    if (!string.IsNullOrEmpty(LevelFilePath))
                    {
                        result = PathC.GetFileNameWithoutExtensionTry(LevelFilePath);
                    }
                    else if (Wads.Count > 0 && !string.IsNullOrEmpty(Wads[0].Path))
                    {
                        result = PathC.GetFileNameWithoutExtensionTry(Wads[0].Path);
                    }
                    else
                    {
                        result = "Default";
                    }

                    break;
                case VariableType.EngineVersion:
                    result = GameVersion.ToString();
                    break;
                case VariableType.SoundEngineVersion:
                    result = (GameVersion.Native()).ToString();
                    break;
                default:
                    throw new ArgumentException();
            }
            if (result == null)
            {
                result = "";
            }

            return result;
        }

        public string ParseVariables(string path, params VariableType[] excluded)
        {
            int startIndex = 0;
            do
            {
                // Find variable
                startIndex = path.IndexOf(VariableBegin, startIndex);
                if (startIndex == -1)
                {
                    break;
                }

                int afterStartIndex = startIndex + VariableBegin.Length;
                int endIndex = path.IndexOf(VariableEnd, afterStartIndex);
                if (endIndex == -1)
                {
                    break;
                }

                string variableName = path.Substring(afterStartIndex, endIndex - afterStartIndex);

                // Parse variable
                VariableType variableType;
                if (!Enum.TryParse(variableName, out variableType) ||
                    excluded.Contains(variableType))
                {
                    startIndex = endIndex + VariableEnd.Length;
                    continue;
                }
                string variableContent = GetVariable(variableType);
                path = path.Remove(startIndex, endIndex + VariableEnd.Length - startIndex);
                path = path.Insert(startIndex, variableContent);
                startIndex += variableContent.Length;
            } while (true);

            return path;
        }

        public string MakeAbsolute(string path, params VariableType[] excluded)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            path = ParseVariables(path, excluded);

            try
            {
                return Path.GetFullPath(path);
            }
            catch
            {
                return path;
            }
        }

        public string MakeRelative(string path, VariableType baseDirType)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            try
            {
                if (string.IsNullOrEmpty(LevelFilePath))
                {
                    return Path.GetFullPath(path);
                }

                switch (baseDirType)
                {
                    case VariableType.EditorDirectory:
                    case VariableType.GameDirectory:
                    case VariableType.LevelDirectory:
                    case VariableType.ScriptDirectory:
                        string relativePath = PathC.GetRelativePath(GetVariable(baseDirType), path);
                        if (relativePath == null)
                        {
                            return Path.GetFullPath(path);
                        }

                        return VariableCreate(baseDirType) + Path.DirectorySeparatorChar + relativePath;
                    default:
                        return path;
                }
            }
            catch
            {
                return path;
            }
        }

        public string GetPluginsDirectory()
        {
            string gameDirectory = MakeAbsolute(GameDirectory);
            string potentialDirectory = Path.Combine(gameDirectory, "Plugins");

            if (Directory.Exists(potentialDirectory))
                return potentialDirectory;

            potentialDirectory = Path.Combine(Path.GetDirectoryName(gameDirectory), "Plugins");

            return Directory.Exists(potentialDirectory) ? potentialDirectory : null;
        }

        public IDictionary<ushort, TriggerParameterUshort> GetPluginRange()
        {
            var result = new SortedDictionary<ushort, TriggerParameterUshort>
            {
                { 0, new TriggerParameterUshort(0, "Tomb_NextGeneration") }
            };

            string scriptDirectory = MakeAbsolute(ScriptDirectory);
            string scriptFilePath = Path.Combine(scriptDirectory, "Script.txt");

            if (!File.Exists(scriptFilePath))
                return result;

            string[] trgFiles = TryGetTRGFiles();

            if (trgFiles.Length == 0)
                return result;

            string[] scriptFileLines = File.ReadAllLines(scriptFilePath);
            string[] pluginCommands = scriptFileLines
                .Where(line => Regex.IsMatch(line, @"^Plugin\s*=", RegexOptions.IgnoreCase))
                .ToArray();

            if (pluginCommands.Length == 0)
                return result;

            foreach (string pluginCommand in pluginCommands)
            {
                string[] commandParts = pluginCommand
                    .Split(';')[0] // Part before the comment
                    .Split('=');

                if (commandParts.Length != 2)
                    continue;

                commandParts = commandParts[1].Split(',');

                if (commandParts.Length is not 2 and not 3)
                    continue;

                if (!ushort.TryParse(commandParts[0], out ushort id) || id < 1) // ID 0 is reserved for Tomb_NextGeneration
                    continue;

                string pluginName = commandParts[1].Trim();

                if (string.IsNullOrEmpty(pluginName))
                    continue;

                if (trgFiles.Any(file => Path.GetFileNameWithoutExtension(file).Equals(pluginName, StringComparison.OrdinalIgnoreCase)))
                    result[id] = new TriggerParameterUshort(id, pluginName);
            }

            return result;
        }

        public string[] TryGetTRGFiles()
        {
            string pluginsDirectory = GetPluginsDirectory();

            return pluginsDirectory is null
                ? Array.Empty<string>()
                : Directory.GetFiles(pluginsDirectory, "Plugin_*.trg", SearchOption.AllDirectories);
        }

        public ImageC LoadFontTexture(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return ImageC.FromSystemDrawingImage(ResourcesC.ResourcesC.Font_pc);
            }

            return LoadRawExtraTexture(path);
        }

        public ImageC LoadSkyTexture(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return ImageC.FromSystemDrawingImage(ResourcesC.ResourcesC.pcsky_raw);
            }

            return LoadRawExtraTexture(path);
        }

        public ImageC LoadTr5ExtraSprites(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return ImageC.FromSystemDrawingImage(ResourcesC.ResourcesC.Extra_Tr5_pc);
            }

            return LoadRawExtraTexture(path);
        }

        public static List<ColorC> LoadPalette(byte[] buffer)
        {
            List<ColorC> result = new List<ColorC>();
            if (buffer.Length < 3)
            {
                return result; // No suitable color data found
            }

            using (MemoryStream stream = new MemoryStream(buffer, false))
            using (BinaryReader readerPalette = new BinaryReader(stream))
            {
                while (readerPalette.BaseStream.Position < readerPalette.BaseStream.Length)
                {
                    result.Add(new ColorC(readerPalette.ReadByte(), readerPalette.ReadByte(), readerPalette.ReadByte()));
                }
            }

            return result;
        }
        public static List<ColorC> LoadPalette()
        {
            return LoadPalette(ResourcesC.ResourcesC.palette);
        }

        public static ImageC LoadRawExtraTexture(string path)
        {
            using (FileStream reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                ImageC image;

                if (path.EndsWith(".raw"))
                {
                    // Raw file: 256² pixels with 24 bpp
                    byte[] data = new byte[256 * 256 * 3];
                    reader.Read(data, 0, data.Length);

                    image = ImageC.CreateNew(256, 256);
                    for (int i = 0; i < 256 * 256; ++i)
                    {
                        image.Set(i, data[i * 3], data[i * 3 + 1], data[i * 3 + 2]);
                    }
                }
                else if (path.EndsWith(".pc"))
                {
                    // Raw file: 256² pixels with 32 bpp
                    byte[] data = new byte[256 * 256 * 4];
                    reader.Read(data, 0, data.Length);

                    image = ImageC.CreateNew(256, 256);
                    for (int i = 0; i < 256 * 256; ++i)
                    {
                        image.Set(i, data[i * 4 + 2], data[i * 4 + 1], data[i * 4], data[i * 4 + 3]);
                    }
                }
                else
                {
                    image = ImageC.FromStream(reader);
                }

                if (image.Width != 256 || image.Height != 256)
                {
                    throw new NotSupportedException("The texture's size must be 256 by 256 pixels. " +
                        "(The current texture '" + path + "' is " + image.Width + " by " + image.Height + " pixels)");
                }

                return image;
            }
        }

        public bool LoadWad(string wadFilePath, VariableType variableType = VariableType.GameDirectory)
        {
            try
            {
                Wads.Add(new ReferencedWad(this, MakeRelative(wadFilePath, variableType)));
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Unable to load wad! Exception: " + ex);
            }

            return false;
        }

        public bool ConvertLegacyTombEngineExecutablePath()
        {
            if (GameVersion != TRVersion.Game.TombEngine)
                return false;

            var exePath = Path.GetDirectoryName(MakeAbsolute(GameExecutableFilePath));
            var exeName = Path.GetFileName(MakeAbsolute(GameExecutableFilePath));

            if (!Directory.Exists(Path.Combine(exePath, "Bin")))
                return false;

            if (Environment.Is64BitOperatingSystem)
                exePath = Path.Combine(exePath, "Bin", "x64");
            else
                exePath = Path.Combine(exePath, "Bin", "x86");

            if (!Directory.Exists(Path.Combine(exePath)))
                return false;

            exePath = Path.Combine(exePath, exeName);

            if (!File.Exists(exePath))
                return false;

            GameExecutableFilePath = MakeRelative(exePath, VariableType.GameDirectory);
            return true;
        }

        public bool LoadDefaultSoundCatalog()
        {
            string catalogName = string.Empty;

            switch (GameVersion.Native())
            {
                case TRVersion.Game.TR1:
                    catalogName = Path.Combine(DefaultPaths.CatalogsDirectory, "Sounds.tr1.xml");
                    break;

                case TRVersion.Game.TR2:
                    catalogName = Path.Combine(DefaultPaths.CatalogsDirectory, "Sounds.tr2.xml");
                    break;

                case TRVersion.Game.TR3:
                    catalogName = Path.Combine(DefaultPaths.CatalogsDirectory, "Sounds.tr3.xml");
                    break;

                case TRVersion.Game.TR4:
                    catalogName = Path.Combine(DefaultPaths.CatalogsDirectory, "Sounds.tr4.xml");
                    break;

                case TRVersion.Game.TR5:
                    catalogName = Path.Combine(DefaultPaths.CatalogsDirectory, "Sounds.tr5.xml");
                    break;

                case TRVersion.Game.TombEngine:
                    catalogName = Path.Combine(DefaultPaths.CatalogsDirectory, "TEN Sound Catalogs", "TEN_ALL_SOUNDS.xml");
                    break;
            }

            if (!string.IsNullOrEmpty(catalogName) && File.Exists(catalogName))
            {
                try
                {
                    SoundCatalogs.Add(new ReferencedSoundCatalog(this, MakeRelative(catalogName, VariableType.EditorDirectory)));
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("Unable to load default sound catalog! Exception: " + ex);
                }
            }

            return false;
        }

        public void ImportedGeometryUpdate(IEnumerable<ImportedGeometryUpdateInfo> geometriesToUpdate)
        {
            Dictionary<string, Texture> absolutePathTextureLookup = new Dictionary<string, Texture>();

            // Add other imported geometry textures to lookup
            foreach (ImportedGeometry importedGeometry in ImportedGeometries)
            {
                foreach (ImportedGeometryTexture importedGeometryTexture in importedGeometry.Textures)
                {
                    if (!absolutePathTextureLookup.ContainsKey(importedGeometryTexture.AbsolutePath))
                    {
                        absolutePathTextureLookup.Add(importedGeometryTexture.AbsolutePath, importedGeometryTexture);
                    }
                }
            }

            // TODO Ideally we could load these concurrently
            // Load geometries
            foreach (ImportedGeometryUpdateInfo geometryToUpdate in geometriesToUpdate)
            {
                geometryToUpdate.Key.Update(this, absolutePathTextureLookup, geometryToUpdate.Value);
            }
        }

        public void ImportedGeometryUpdate(ImportedGeometry geometry, ImportedGeometryInfo info)
        {
            ImportedGeometryUpdate(new[] { new ImportedGeometryUpdateInfo(geometry, info) });
        }

        public ImportedGeometry ImportedGeometryFromID(ImportedGeometry.UniqueIDType uniqueID)
        {
            foreach (ImportedGeometry importedGeometry in ImportedGeometries)
            {
                if (importedGeometry.UniqueID == uniqueID)
                {
                    return importedGeometry;
                }
            }

            return null;
        }

        public WadMoveable WadTryGetMoveable(WadMoveableId id)
        {
            WadMoveable result;
            foreach (ReferencedWad wad in Wads)
            {
                if (wad.Wad != null && wad.Wad.Moveables.TryGetValue(id, out result))
                {
                    return result;
                }
            }

            return null;
        }

        public WadStatic WadTryGetStatic(WadStaticId id)
        {
            WadStatic result;
            foreach (ReferencedWad wad in Wads)
            {
                if (wad.Wad != null && wad.Wad.Statics.TryGetValue(id, out result))
                {
                    return result;
                }
            }

            return null;
        }

        public ReferencedWad WadTryGetWad(ItemType item, out bool multiple)
        {
            ReferencedWad result = null;
            multiple = false;

            foreach (ReferencedWad wad in Wads)
            {
                if (wad.Wad == null)
                {
                    continue;
                }

                if ((item.IsStatic && wad.Wad.Statics.ContainsKey(item.StaticId)) ||
                    (!item.IsStatic && wad.Wad.Moveables.ContainsKey(item.MoveableId)))
                {
                    if (result == null)
                    {
                        result = wad;
                    }
                    else
                    {
                        multiple = true;
                    }
                }
            }

            return result;
        }

        public SortedList<WadMoveableId, WadMoveable> WadGetAllMoveables()
        {
            SortedList<WadMoveableId, WadMoveable> result = new SortedList<WadMoveableId, WadMoveable>();
            foreach (ReferencedWad wad in Wads)
            {
                if (wad.Wad != null)
                {
                    foreach (KeyValuePair<WadMoveableId, WadMoveable> moveable in wad.Wad.Moveables)
                    {
                        if (!result.ContainsKey(moveable.Key))
                        {
                            result.Add(moveable.Key, moveable.Value);
                        }
                    }
                }
            }

            return result;
        }

        public SortedList<WadStaticId, WadStatic> WadGetAllStatics()
        {
            SortedList<WadStaticId, WadStatic> result = new SortedList<WadStaticId, WadStatic>();
            foreach (ReferencedWad wad in Wads)
            {
                if (wad.Wad != null)
                {
                    foreach (KeyValuePair<WadStaticId, WadStatic> @static in wad.Wad.Statics)
                    {
                        if (!result.ContainsKey(@static.Key))
                        {
                            result.Add(@static.Key, @static.Value);
                        }
                    }
                }
            }

            return result;
        }

        public SortedList<WadSpriteSequenceId, WadSpriteSequence> WadGetAllSpriteSequences()
        {
            SortedList<WadSpriteSequenceId, WadSpriteSequence> result = new SortedList<WadSpriteSequenceId, WadSpriteSequence>();
            foreach (ReferencedWad wad in Wads)
            {
                if (wad.Wad != null)
                {
                    foreach (KeyValuePair<WadSpriteSequenceId, WadSpriteSequence> sprite in wad.Wad.SpriteSequences)
                    {
                        if (!result.ContainsKey(sprite.Key))
                        {
                            result.Add(sprite.Key, sprite.Value);
                        }
                    }
                }
            }

            return result;
        }

        public List<WadSprite> WadGetAllSprites()
        {
            return WadGetAllSpriteSequences().Values.SelectMany(s => s.Sprites).ToList();
        }

        public static IEnumerable<FileFormat> FileFormatsLoadRawExtraTexture =>
            new[] { new FileFormat("Raw sky/font image", "raw", "pc") }.Concat(ImageC.FileExtensions);
        public static readonly IReadOnlyCollection<FileFormat> FileFormatsLevel = new[] { new FileFormat("Tomb Editor Level", "prj2") };
        public static readonly IReadOnlyCollection<FileFormat> FileFormatsLevelPrj = new[] { new FileFormat("Tomb Editor Level", "prj") };
        public static readonly IReadOnlyCollection<FileFormat> FileFormatsLevelCompiled = new[]
        {
            new FileFormat("Tomb Raider I level", "phd"),
            new FileFormat("Tomb Raider II/III level", "tr2"),
            new FileFormat("Tomb Raider The Last Revelation level", "tr4"),
            new FileFormat("Tomb Raider Chronicles level", "trc"),
            new FileFormat("TombEngine level", "ten")
        };

        public static readonly IReadOnlyCollection<FileFormat> FileFormatsSoundsCatalogs = new[]
        {
            new FileFormat("TRLE sound catalog", "txt"),
            new FileFormat("Tomb Editor sound catalog", "xml")
        };
        public static readonly IReadOnlyCollection<FileFormat> FileFormatsSoundsXmlFiles = new[] { new FileFormat("XML file", "xml") };

        public WadSoundInfo WadTryGetSoundInfo(int id)
        {
            foreach (WadSoundInfo soundInfo in GlobalSoundMap)
            {
                if (soundInfo.Id == id)
                {
                    return soundInfo;
                }
            }

            return null;
        }

        public bool AutoStaticMeshMergeContainsStaticMesh(WadStatic staticMesh)
        {
            return AutoStaticMeshMerges.Where(e => staticMesh != null && e.meshId == staticMesh.Id.TypeId).Any();
        }

        public AutoStaticMeshMergeEntry GetStaticMergeEntry(WadStaticId staticMeshId)
        {
            return AutoStaticMeshMerges.FirstOrDefault(e => e.meshId == staticMeshId.TypeId);
        }

        public List<int> SelectedAndMissingSounds => SelectedSounds.Where(item => !GlobalSoundMap.Any(entry => entry.Id == item)).ToList();

        public List<int> SelectedAndAvailableSounds
        {
            get
            {
                var paths = GetRecursiveListOfSoundPaths();
                return SelectedSounds.Where(item => GlobalSoundMap
                                     .Any(entry => entry.Id == item && entry.Samples.Count > 0 && entry.SampleCount(this, paths) > 0)).ToList();
            }
        }

        public bool AllSoundSamplesAvailable
        {
            get
            {
                var paths = GetRecursiveListOfSoundPaths();
                return GlobalSoundMap.All(s => s.Samples.Count == s.SampleCount(this, paths));
            }
        }

        public List<EventSet> AllEventSets
        {
            get
            {
                return GlobalEventSets.Concat(VolumeEventSets).ToList();
            }
        }
    }
}
