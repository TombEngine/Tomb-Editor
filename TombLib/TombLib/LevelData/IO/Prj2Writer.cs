using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorStructs;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombLib.LevelData.IO
{
    public static class Prj2Writer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static Level _level;

        public class Filter
        {
            public Predicate<Room> RoomPredicate;
            public bool FilterLevelSettings;
        }

        public static void SaveToPrj2(string filename, Level level, Filter filter = null)
        {
            // We save first to a temporary memory stream
            using (var stream = new MemoryStream())
            {
                SaveToPrj2(stream, level);

                // Save to temporary file as well, so original prj2 won't vanish in case of crash
                var tempName = filename + ".tmp";
                if (File.Exists(tempName)) File.Delete(tempName);

                stream.Seek(0, SeekOrigin.Begin);
                using (var writer = new BinaryWriter(new FileStream(tempName, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    var buffer = stream.ToArray();
                    writer.Write(buffer, 0, buffer.Length);
                }

                // Save successful, write temp file over original (if exists)
                if (File.Exists(filename)) File.Delete(filename);
                File.Move(tempName, filename);
            }
        }

        public static void SaveToPrj2(Stream stream, Level level, Filter filter = null)
        {
            _level = level;

            using (var chunkIO = new ChunkWriter(Prj2Chunks.MagicNumber, stream, ChunkWriter.Compression.None))
            {
                // Index rooms
                var rooms = new Dictionary<Room, int>();
                for (int i = 0; i < level.Rooms.Length; ++i)
                    if (level.Rooms[i] != null && (filter?.RoomPredicate(level.Rooms[i]) ?? true))
                        rooms.Add(level.Rooms[i], i);

                LevelSettings settingsToSave = level.Settings;
                if (filter != null && filter.FilterLevelSettings)
                {
                    settingsToSave = new LevelSettings();
                    var copyInstance = new Room.CopyDependentLevelSettingsArgs(null, settingsToSave, level.Settings, false);
                    foreach (Room room in rooms.Keys)
                        room.CopyDependentLevelSettings(copyInstance);
                }

                // Write settings
                LevelSettingsIds levelSettingIds = WriteLevelSettings(chunkIO, settingsToSave);

                // Write rooms
                WriteRooms(chunkIO, rooms, levelSettingIds);
                chunkIO.WriteChunkEnd();
            }
        }

        public static void SaveToPrj2OnlyObjects(Stream stream, Level level, IEnumerable<ObjectInstance> objects)
        {
            using (var chunkIO = new ChunkWriter(Prj2Chunks.MagicNumber, stream, ChunkWriter.Compression.None))
            {
                // Index objects
                var objectInstanceLookup = new ConcurrentDictionary<ObjectInstance, int>();
                Parallel.For(0, level.Rooms.Length, i =>
                {
                    if (level.Rooms[i] == null) return;
                    var objList = level.Rooms[i].Objects;
                    for (int j = 0; j < objList.Count; j++)
                    {
                        // Use room number and a hashcode truncated to 22 bits as a lookup ID.
                        // Truncating hashcode is risky, but since we also use room number as a part
                        // of an ID, there's a slim chance that ID will misfire.

                        var encodedID = ((objList[j].GetHashCode() & 0x3FFFFF) << 10) | i;
                        objectInstanceLookup.TryAdd(objList[j], encodedID);
                    }
                });

                // Index rooms
                var roomLookup = new Dictionary<Room, int>();
                for (int i = 0; i < level.Rooms.Length; ++i)
                    if (level.Rooms[i] != null)
                        roomLookup.Add(level.Rooms[i], i);

                // Write settings
                LevelSettings settingsToSave = new LevelSettings();
                Room.CopyDependentLevelSettingsArgs copyInstance = new Room.CopyDependentLevelSettingsArgs(null, settingsToSave, level.Settings, false);
                foreach (ObjectInstance instance in objects)
                    instance.CopyDependentLevelSettings(copyInstance);
                LevelSettingsIds levelSettingIds = WriteLevelSettings(chunkIO, settingsToSave);

                // Write objects
                WriteObjects(chunkIO, objects, roomLookup, levelSettingIds, objectInstanceLookup, level);
                chunkIO.WriteChunkEnd();
            }
        }

        private class LevelSettingsIds
        {
            private LevelSettings _settings;

            public LevelSettingsIds(LevelSettings settings)
            {
                _settings = settings;
                ImportedGeometries = new Dictionary<ImportedGeometry, int>(new ImportedGeometryComparer(_settings));
                LevelTextures = new Dictionary<LevelTexture, int>(new ReferenceEqualityComparer<LevelTexture>());
                VolumeEventSets = new Dictionary<VolumeEventSet, int>(new ReferenceEqualityComparer<VolumeEventSet>());
            }

            public Dictionary<ImportedGeometry, int> ImportedGeometries { get; private set; }
            public Dictionary<LevelTexture, int> LevelTextures { get; private set; }
            public Dictionary<VolumeEventSet, int> VolumeEventSets { get; private set; }
        }

        private static LevelSettingsIds WriteLevelSettings(ChunkWriter chunkIO, LevelSettings settings)
        {
            var levelSettingIds = new LevelSettingsIds(settings);
            using (var chunkSettings = chunkIO.WriteChunk(Prj2Chunks.Settings, long.MaxValue))
            {
                chunkIO.WriteChunkString(Prj2Chunks.FontTextureFilePath, settings.FontTextureFilePath ?? string.Empty);
                chunkIO.WriteChunkString(Prj2Chunks.SkyTextureFilePath, settings.SkyTextureFilePath ?? string.Empty);
                chunkIO.WriteChunkString(Prj2Chunks.Tr5ExtraSpritesFilePath, settings.Tr5ExtraSpritesFilePath ?? string.Empty);
                using (var chunkOldWadSoundPaths = chunkIO.WriteChunk(Prj2Chunks.OldWadSoundPaths))
                {
                    chunkIO.WriteChunkEmpty(Prj2Chunks.OldWadSoundUpdateTag1_0_8);
                    foreach (WadSoundPath soundPath in settings.WadSoundPaths)
                        using (var chunkOldWadSoundPath = chunkIO.WriteChunk(Prj2Chunks.OldWadSoundPath))
                        {
                            chunkIO.WriteChunkString(Prj2Chunks.OldWadSoundPathPath, soundPath.Path);
                            chunkIO.WriteChunkEnd();
                        }
                    chunkIO.WriteChunkEnd();
                }
                chunkIO.WriteChunkString(Prj2Chunks.GameDirectory, settings.GameDirectory ?? "");
                chunkIO.WriteChunkString(Prj2Chunks.GameLevelFilePath, settings.GameLevelFilePath ?? "");
                chunkIO.WriteChunkString(Prj2Chunks.GameExecutableFilePath, settings.GameExecutableFilePath ?? "");
                chunkIO.WriteChunkBool(Prj2Chunks.GameEnableQuickStartFeature, settings.GameEnableQuickStartFeature);
                chunkIO.WriteChunkBool(Prj2Chunks.GameEnableExtraBlendingModes, settings.GameEnableExtraBlendingModes ?? false);
                chunkIO.WriteChunkBool(Prj2Chunks.GameEnableExtraReverbPresets, settings.GameEnableExtraReverbPresets);
                chunkIO.WriteChunkInt(Prj2Chunks.GameVersion, (long)settings.GameVersion);
                chunkIO.WriteChunkInt(Prj2Chunks.Tr5LaraType, (long)settings.Tr5LaraType);
                chunkIO.WriteChunkInt(Prj2Chunks.Tr5Weather, (long)settings.Tr5WeatherType);
                chunkIO.WriteChunkInt(Prj2Chunks.TexturePadding, (long)settings.TexturePadding);
                chunkIO.WriteChunkBool(Prj2Chunks.RemapAnimatedTextures, settings.RemapAnimatedTextures);
                chunkIO.WriteChunkBool(Prj2Chunks.RearrangeRooms, settings.RearrangeVerticalRooms);
                chunkIO.WriteChunkBool(Prj2Chunks.RemoveUnusedObjects, settings.RemoveUnusedObjects);
                chunkIO.WriteChunkBool(Prj2Chunks.EnableCustomSampleRate, settings.EnableCustomSampleRate);
                chunkIO.WriteChunkInt(Prj2Chunks.CustomSampleRate, settings.CustomSampleRate);
                chunkIO.WriteChunkBool(Prj2Chunks.Room32BitLighting, settings.Room32BitLighting);
                chunkIO.WriteChunkBool(Prj2Chunks.Dither16BitTextures, settings.Dither16BitTextures);
                chunkIO.WriteChunkBool(Prj2Chunks.AgressiveTexturePacking, settings.AgressiveTexturePacking);
                chunkIO.WriteChunkBool(Prj2Chunks.TextureCompression, settings.CompressTextures);
                chunkIO.WriteChunkBool(Prj2Chunks.AgressiveFloordataPacking, settings.AgressiveFloordataPacking);
                chunkIO.WriteChunkVector3(Prj2Chunks.DefaultAmbientLight, settings.DefaultAmbientLight);
                chunkIO.WriteChunkInt(Prj2Chunks.DefaultLightQuality, (long)settings.DefaultLightQuality);
                chunkIO.WriteChunkBool(Prj2Chunks.OverrideLightQuality, settings.OverrideIndividualLightQualitySettings);
                chunkIO.WriteChunkString(Prj2Chunks.ScriptDirectory, settings.ScriptDirectory ?? string.Empty);
                chunkIO.WriteChunkInt(Prj2Chunks.SoundSystem, (int)settings.SoundSystem);
                chunkIO.WriteChunkInt(Prj2Chunks.LastRoom, settings.LastSelectedRoom);
                chunkIO.WriteChunkString(Prj2Chunks.TenLuaScriptFile, settings.TenLuaScriptFile ?? string.Empty);

                using (var chunkWads = chunkIO.WriteChunk(Prj2Chunks.Wads, long.MaxValue))
                {
                    foreach (ReferencedWad wad in settings.Wads)
                        using (var chunkWad = chunkIO.WriteChunk(Prj2Chunks.Wad))
                        {
                            chunkIO.WriteChunkString(Prj2Chunks.WadPath, wad.Path ?? string.Empty);
                            chunkIO.WriteChunkEnd();
                        }
                    chunkIO.WriteChunkEnd();
                }
                using (var chunkSounds = chunkIO.WriteChunk(Prj2Chunks.SoundsCatalogs, long.MaxValue))
                {
                    foreach (ReferencedSoundCatalog soundRef in settings.SoundCatalogs)
                        using (var chunkSound = chunkIO.WriteChunk(Prj2Chunks.SoundsCatalog))
                        {
                            chunkIO.WriteChunkString(Prj2Chunks.SoundsCatalogPath, soundRef.Path ?? string.Empty);
                            chunkIO.WriteChunkEnd();
                        }
                    chunkIO.WriteChunkEnd();
                }
                using (var chunkSelectedSounds = chunkIO.WriteChunk(Prj2Chunks.SelectedSounds, long.MaxValue))
                {
                    foreach (int selectedSound in settings.SelectedSounds)
                        chunkIO.WriteChunkInt(Prj2Chunks.SelectedSound, selectedSound);
                    chunkIO.WriteChunkEnd();
                }
                using (var chunkTextures = chunkIO.WriteChunk(Prj2Chunks.Textures, long.MaxValue))
                {
                    int index = 0;
                    foreach (LevelTexture texture in settings.Textures)
                    {
                        using (var chunkLevelTexture = chunkIO.WriteChunk(Prj2Chunks.LevelTexture))
                        {
                            chunkIO.WriteChunkInt(Prj2Chunks.LevelTextureIndex, index);
                            chunkIO.WriteChunkString(Prj2Chunks.LevelTexturePath, texture.Path ?? string.Empty);
                            chunkIO.WriteChunkString(Prj2Chunks.LevelTextureCustomBumpmapPath, texture.BumpPath ?? string.Empty);
                            chunkIO.WriteChunkBool(Prj2Chunks.LevelTextureConvert512PixelsToDoubleRows, texture.Convert512PixelsToDoubleRows);
                            chunkIO.WriteChunkBool(Prj2Chunks.LevelTextureReplaceMagentaWithTransparency, texture.ReplaceMagentaWithTransparency);
                            using (var chunkLevelTextureFootStepSounds = chunkIO.WriteChunk(Prj2Chunks.LevelTextureFootStepSounds))
                            {
                                chunkIO.Raw.Write(texture.FootStepSoundWidth);
                                chunkIO.Raw.Write(texture.FootStepSoundHeight);
                                for (int y = 0; y < texture.FootStepSoundHeight; ++y)
                                    for (int x = 0; x < texture.FootStepSoundWidth; ++x)
                                        chunkIO.Raw.Write((byte)texture.GetFootStepSound(x, y));
                            }
                            using (var chunkLevelTextureBumpmaps = chunkIO.WriteChunk(Prj2Chunks.LevelTextureBumpmaps))
                            {
                                chunkIO.Raw.Write(texture.BumpMappingWidth);
                                chunkIO.Raw.Write(texture.BumpMappingHeight);
                                for (int y = 0; y < texture.BumpMappingHeight; ++y)
                                    for (int x = 0; x < texture.BumpMappingWidth; ++x)
                                        chunkIO.Raw.Write((byte)texture.GetBumpMapLevel(x, y));
                            }
                            chunkIO.WriteChunkEnd();
                        }
                        levelSettingIds.LevelTextures.Add(texture, index++);
                    }
                    chunkIO.WriteChunkEnd();
                }
                using (var chunkTextures = chunkIO.WriteChunk(Prj2Chunks.DefaultTexture, long.MaxValue))
                {
                    chunkIO.Raw.Write(settings.DefaultTexture.TexCoord0);
                    chunkIO.Raw.Write(settings.DefaultTexture.TexCoord1);
                    chunkIO.Raw.Write(settings.DefaultTexture.TexCoord2);
                    chunkIO.Raw.Write(settings.DefaultTexture.TexCoord3);

                    int textureIndex = -1;

                    if (settings.DefaultTexture.Texture is LevelTexture t)
                    {
                        if (t != null && levelSettingIds.LevelTextures.ContainsKey(t))
                            textureIndex = levelSettingIds.LevelTextures[t];
                    }

                    LEB128.Write(chunkIO.Raw, textureIndex);
                }
                using (var chunkImportedGeometries = chunkIO.WriteChunk(Prj2Chunks.ImportedGeometries, long.MaxValue))
                {
                    int index = 0;
                    foreach (ImportedGeometry importedGeometry in settings.ImportedGeometries)
                    {
                        using (var chunkImportedGeometry = chunkIO.WriteChunk(Prj2Chunks.ImportedGeometry))
                        {
                            chunkIO.WriteChunkInt(Prj2Chunks.ImportedGeometryIndex, index);
                            chunkIO.WriteChunkString(Prj2Chunks.ImportedGeometryName, importedGeometry.Info.Name);
                            chunkIO.WriteChunkString(Prj2Chunks.ImportedGeometryPath, importedGeometry.Info.Path);
                            chunkIO.WriteChunkFloat(Prj2Chunks.ImportedGeometryScale, importedGeometry.Info.Scale);
                            chunkIO.WriteChunkInt(Prj2Chunks.ImportedGeometryPosAxisFlags,
                                (importedGeometry.Info.SwapXY ? 1 : 0) |
                                (importedGeometry.Info.SwapXZ ? 2 : 0) |
                                (importedGeometry.Info.SwapYZ ? 4 : 0) |
                                (importedGeometry.Info.FlipX ? 8 : 0) |
                                (importedGeometry.Info.FlipY ? 16 : 0) |
                                (importedGeometry.Info.FlipZ ? 32 : 0));
                            chunkIO.WriteChunkInt(Prj2Chunks.ImportedGeometryTexAxisFlags, importedGeometry.Info.FlipUV_V ? 4 : 0);
                            chunkIO.WriteChunkBool(Prj2Chunks.ImportedGeometryMappedUV, importedGeometry.Info.MappedUV);
                            chunkIO.WriteChunkBool(Prj2Chunks.ImportedGeometryInvertFaces, importedGeometry.Info.InvertFaces);
                            chunkIO.WriteChunkEnd();
                        }
                        levelSettingIds.ImportedGeometries.TryAdd(importedGeometry, index++);
                    }
                    chunkIO.WriteChunkEnd();
                }
                using (var chunkAnimatedTextureSets = chunkIO.WriteChunk(Prj2Chunks.AnimatedTextureSets, long.MaxValue))
                {
                    foreach (AnimatedTextureSet set in settings.AnimatedTextureSets)
                        using (var chunkAnimatedTextureSet = chunkIO.WriteChunk(Prj2Chunks.AnimatedTextureSet))
                        {
                            chunkIO.WriteChunkString(Prj2Chunks.AnimatedTextureSetName, set.Name ?? string.Empty);
                            chunkIO.WriteChunkInt(Prj2Chunks.AnimatedTextureSetType, (int)set.AnimationType);
                            chunkIO.WriteChunkFloat(Prj2Chunks.AnimatedTextureSetFps, set.Fps);
                            chunkIO.WriteChunkInt(Prj2Chunks.AnimatedTextureSetUvRotate, set.UvRotate);
                            chunkIO.WriteChunkFloat(Prj2Chunks.AnimatedTextureSetTenUvRotateDirection, set.TenUvRotateDirection);
							chunkIO.WriteChunkFloat(Prj2Chunks.AnimatedTextureSetTenUvRotateSpeed, set.TenUvRotateSpeed);
							
                            using (var chunkAnimatedTextureFrames = chunkIO.WriteChunk(Prj2Chunks.AnimatedTextureFrames))
                            {
                                foreach (AnimatedTextureFrame frame in set.Frames)
                                {
                                    if (frame.Texture != null && levelSettingIds.LevelTextures.ContainsKey((LevelTexture)frame.Texture))
                                        using (var chunkAnimatedTextureFrame = chunkIO.WriteChunk(Prj2Chunks.AnimatedTextureFrame, 120))
                                        {
                                            LEB128.Write(chunkIO.Raw, levelSettingIds.LevelTextures[(LevelTexture)frame.Texture]);
                                            chunkIO.Raw.Write(frame.TexCoord0);
                                            chunkIO.Raw.Write(frame.TexCoord1);
                                            chunkIO.Raw.Write(frame.TexCoord2);
                                            chunkIO.Raw.Write(frame.TexCoord3);
                                            LEB128.Write(chunkIO.Raw, frame.Repeat);
                                        }
                                    else
                                        logger.Warn("Animated sequence " + set.Name + " has a frame refering to a texture file which is missing from project.");
                                }

                                chunkIO.WriteChunkEnd();
                            }
                            chunkIO.WriteChunkEnd();
                        }
                    chunkIO.WriteChunkEnd();
                }

                foreach (bool global in new[] { true, false })
                {
                    using (var chunkEventSets = chunkIO.WriteChunk(global ? Prj2Chunks.GlobalEventSets : Prj2Chunks.VolumeEventSets, long.MaxValue))
                    {
                        int index = 0;
                        foreach (var set in (global ? settings.GlobalEventSets : settings.VolumeEventSets))
                        {
                            using (var chunkEventSet = chunkIO.WriteChunk(Prj2Chunks.EventSet))
                            {
                                chunkIO.WriteChunkInt(Prj2Chunks.EventSetIndex, index);
                                chunkIO.WriteChunkString(Prj2Chunks.EventSetName, set.Name ?? string.Empty);
                                chunkIO.WriteChunkInt(Prj2Chunks.EventSetLastUsedEventIndex, (int)set.LastUsedEvent);

                                if (!global)
                                    chunkIO.WriteChunkInt(Prj2Chunks.EventSetActivators, (int)(set as VolumeEventSet).Activators);

                                foreach (var evt in set.Events)
                                {
                                    using (var chunkEvent = chunkIO.WriteChunk(Prj2Chunks.Event))
                                    {
                                        chunkIO.WriteChunkInt(Prj2Chunks.EventType, (int)evt.Key);
                                        chunkIO.WriteChunkInt(Prj2Chunks.EventMode, (int)evt.Value.Mode);
                                        chunkIO.WriteChunkString(Prj2Chunks.EventFunction, evt.Value.Function ?? string.Empty);
                                        chunkIO.WriteChunkString(Prj2Chunks.EventArgument, evt.Value.Argument ?? string.Empty);
                                        chunkIO.WriteChunkInt(Prj2Chunks.EventCallCounter, evt.Value.CallCounter);
                                        chunkIO.WriteChunkBool(Prj2Chunks.EventEnabled, evt.Value.Enabled);
                                        chunkIO.WriteChunkVector2(Prj2Chunks.EventNodePosition, evt.Value.NodePosition);
                                        evt.Value.Nodes.ForEach(n => WriteNode(chunkIO, n, Prj2Chunks.EventNodeNext));

                                        chunkIO.WriteChunkEnd();
                                    }
                                }

                                chunkIO.WriteChunkEnd();
                            }

                            if (!global)
                                levelSettingIds.VolumeEventSets.TryAdd((set as VolumeEventSet), index++);
                            else
                                index++;
                        }

                        chunkIO.WriteChunkEnd();
                    }
                }

                using (var chunkAutoMergeStatics = chunkIO.WriteChunk(Prj2Chunks.AutoMergeStaticMeshes, UInt16.MaxValue))
                {
                    foreach (var entry in settings.AutoStaticMeshMerges)
                        using (var chunkAutoMergeEntry = chunkIO.WriteChunk(Prj2Chunks.AutoMergeStaticMeshEntry3))
                        {
                            chunkIO.Raw.Write(entry.meshId);
                            chunkIO.Raw.Write(entry.InterpretShadesAsEffect);
                            chunkIO.Raw.Write(entry.TintAsAmbient);
                            chunkIO.Raw.Write(entry.ClearShades);
                        }
                    chunkIO.WriteChunkEnd();
                }
                using (var chunkPalette = chunkIO.WriteChunk(Prj2Chunks.Palette, UInt16.MaxValue))
                {
                    chunkIO.Raw.Write((ushort)settings.Palette.Count);
                    foreach (var color in settings.Palette)
                    {
                        chunkIO.Raw.Write(color.R);
                        chunkIO.Raw.Write(color.G);
                        chunkIO.Raw.Write(color.B);
                    }
                }
                chunkIO.WriteChunkEnd();
            }

            return levelSettingIds;
        }

        private static void WriteRooms(ChunkWriter chunkIO, Dictionary<Room, int> rooms, LevelSettingsIds levelSettingIds)
        {
            // Allocate object indices
            var objectInstanceLookup = new Dictionary<ObjectInstance, int>();
            foreach (Room room in rooms.Keys)
                foreach (ObjectInstance objectInstance in room.AnyObjects)
                    objectInstanceLookup.Add(objectInstance, objectInstanceLookup.Count);

            // Save
            using (var chunkRooms = chunkIO.WriteChunk(Prj2Chunks.Rooms, long.MaxValue))
            {
                foreach (Room room in rooms.Keys)
                    using (var chunkRoom = chunkIO.WriteChunk(Prj2Chunks.Room, long.MaxValue))
                    {
                        LEB128.Write(chunkIO.Raw, room.NumXSectors);
                        LEB128.Write(chunkIO.Raw, room.NumZSectors);

                        // Write basic properties
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomIndex, rooms.TryGetOrDefault(room, -1));
                        chunkIO.WriteChunkString(Prj2Chunks.RoomName, room.Name);
                        chunkIO.WriteChunkVector3(Prj2Chunks.RoomPosition2, room.Position);
                        chunkIO.WriteChunkArrayOfBytes(Prj2Chunks.RoomTags, System.Text.Encoding.UTF8.GetBytes(string.Join(" ", room.Properties.Tags)));

                        // Write sectors
                        using (var chunkRoomSectors = chunkIO.WriteChunk(Prj2Chunks.RoomSectors))
                        {
                            for (int z = 0; z < room.NumZSectors; z++)
                                for (int x = 0; x < room.NumXSectors; x++)
                                    using (var chunkSector = chunkIO.WriteChunk(Prj2Chunks.Sector, LEB128.MaximumSize2Byte))
                                    {
                                        chunkIO.Raw.Write(x + z * room.NumXSectors);
                                        var b = room.Sectors[x, z];

                                        long combinedFlag = (b.IsAnyWall ? 1L : 0) | (b.ForceFloorSolid ? 2L : 0) | ((long)b.Flags << 2);
                                        chunkIO.WriteChunkInt(Prj2Chunks.SectorProperties, combinedFlag);
                                        using (var chunkSectorFloor = chunkIO.WriteChunk(Prj2Chunks.SectorFloorOnly2, LEB128.MaximumSize1Byte))
                                        {
                                            long flag = (b.Floor.SplitDirectionIsXEqualsZ ? 1L : 0) | ((long)b.Floor.DiagonalSplit << 1);
                                            LEB128.Write(chunkIO.Raw, flag);
                                            for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                                                LEB128.Write(chunkIO.Raw, b.Floor.GetHeight(edge));
                                        }
                                        using (var chunkSectorCeiling = chunkIO.WriteChunk(Prj2Chunks.SectorCeilingOnly2, LEB128.MaximumSize1Byte))
                                        {
                                            long flag = (b.Ceiling.SplitDirectionIsXEqualsZ ? 1L : 0) | ((long)b.Ceiling.DiagonalSplit << 1);
                                            LEB128.Write(chunkIO.Raw, flag);
                                            for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                                                LEB128.Write(chunkIO.Raw, b.Ceiling.GetHeight(edge));
                                        }

                                        var validFloorSplits = room.GetValidFloorSplitsForSector(x, z).ToArray();
                                        var validCeilingSplits = room.GetValidCeilingSplitsForSector(x, z).ToArray();

                                        using (var chunkSectorExtraFloorSplits = chunkIO.WriteChunk(Prj2Chunks.SectorFloorSubdivisions2, LEB128.MaximumSize2Byte))
                                        {
                                            LEB128.Write(chunkIO.Raw, validFloorSplits.Length);

                                            foreach (SectorVerticalPart splitVertical in validFloorSplits)
                                                for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                                                    LEB128.Write(chunkIO.Raw, b.GetHeight(splitVertical, edge));
                                        }
                                        using (var chunkSectorExtraCeilingSplits = chunkIO.WriteChunk(Prj2Chunks.SectorCeilingSubdivisions2, LEB128.MaximumSize2Byte))
                                        {
                                            LEB128.Write(chunkIO.Raw, validCeilingSplits.Length);

                                            foreach (SectorVerticalPart splitVertical in validCeilingSplits)
                                                for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                                                    LEB128.Write(chunkIO.Raw, b.GetHeight(splitVertical, edge));
                                        }
                                        foreach (SectorFace face in b.GetFaceTextures().Where(texture => room.RoomGeometry.VertexRangeLookup.ContainsKey(new SectorFaceIdentity(x, z, texture.Key))).Select(x => x.Key))
                                        {
                                            TextureArea texture = b.GetFaceTexture(face);

                                            if (texture.Texture == null)
                                                continue;

                                            if (texture.Texture is LevelTexture t)
                                            {
                                                if (t != null && levelSettingIds.LevelTextures.ContainsKey(t))
                                                    using (var chunkTextureLevelTexture = chunkIO.WriteChunk(Prj2Chunks.TextureLevelTexture2, LEB128.MaximumSize1Byte))
                                                    {
                                                        int textureIndex = levelSettingIds.LevelTextures[t];

                                                        LEB128.Write(chunkIO.Raw, (long)face);
                                                        chunkIO.Raw.Write(texture.TexCoord0);
                                                        chunkIO.Raw.Write(texture.TexCoord1);
                                                        chunkIO.Raw.Write(texture.TexCoord2);
                                                        chunkIO.Raw.Write(texture.TexCoord3);
                                                        chunkIO.Raw.Write(texture.ParentArea.Start);
                                                        chunkIO.Raw.Write(texture.ParentArea.End);
                                                        LEB128.Write(chunkIO.Raw, (texture.DoubleSided ? 1L : 0) | ((long)texture.BlendMode << 1));
                                                        LEB128.Write(chunkIO.Raw, textureIndex);
                                                    }
                                                else
                                                    logger.Warn("Room " + room.Name + " has a texture referring to a texture file " + t.Path + " which is missing from project.");
                                            }
                                            else if (texture.Texture == TextureInvisible.Instance)
                                                chunkIO.WriteChunkInt(Prj2Chunks.TextureInvisible, (long)face);
                                            else
                                                throw new NotSupportedException("Unsupported texture type " + texture.Texture.GetType().Name);
                                        }
                                        chunkIO.WriteChunkEnd();
                                    }
                            chunkIO.WriteChunkEnd();
                        }

                        // Write room properties
                        chunkIO.WriteChunkVector3(Prj2Chunks.RoomAmbientLight, room.Properties.AmbientLight);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagCold, room.Properties.FlagCold);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagDamage, room.Properties.FlagDamage);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagHorizon, room.Properties.FlagHorizon);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagOutside, room.Properties.FlagOutside);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagNoLensflare, room.Properties.FlagNoLensflare);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomFlagExcludeFromPathFinding, room.Properties.FlagExcludeFromPathFinding);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomType, (int)room.Properties.Type);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomTypeStrength, room.Properties.TypeStrength);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomLightEffect, (int)room.Properties.LightEffect);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomLightEffectStrength2, room.Properties.LightEffectStrength);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomReverberation, (int)room.Properties.Reverberation);
                        chunkIO.WriteChunkInt(Prj2Chunks.RoomLightInterpolationMode, (int)room.Properties.LightInterpolationMode);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomLocked, room.Properties.Locked);
                        chunkIO.WriteChunkBool(Prj2Chunks.RoomHidden, room.Properties.Hidden);
                        if (room.AlternateRoom != null && rooms.ContainsKey(room.AlternateRoom))
                            using (var chunkRoomAlternate = chunkIO.WriteChunk(Prj2Chunks.RoomAlternate, LEB128.MaximumSize1Byte))
                            {
                                chunkIO.WriteChunkInt(Prj2Chunks.AlternateGroup, room.AlternateGroup);
                                chunkIO.WriteChunkInt(Prj2Chunks.AlternateRoom, rooms[room.AlternateRoom]);
                                chunkIO.WriteChunkEnd();
                            }

                        // Write room objects
                        WriteObjects(chunkIO, room.AnyObjects, rooms, levelSettingIds, objectInstanceLookup, _level);
                        chunkIO.WriteChunkEnd();
                    }
                chunkIO.WriteChunkEnd();
            }
        }

        private static void WriteObjects(ChunkWriter chunkIO, IEnumerable<ObjectInstance> objects,
            IDictionary<Room, int> rooms, LevelSettingsIds levelSettingIds,
            IDictionary<ObjectInstance, int> objectInstanceLookup, Level level)
        {
            using (var chunkObjects = chunkIO.WriteChunk(Prj2Chunks.Objects, long.MaxValue))
            {
                // FIXME: Null level is used when prj2writer is called with clipboard events.
                // It's not clear why Monty decided to deviate for 2 different chunk versions.

                var isTEN = level == null || level.IsTombEngine;

                foreach (var o in objects)
                {
                    if (o is MoveableInstance)
                    {
                        if (isTEN)
                        {
                            using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectMovableTombEngine2, LEB128.MaximumSize2Byte))
                            {
                                var instance = (MoveableInstance)o;
                                LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                chunkIO.Raw.Write(instance.Position);
                                chunkIO.Raw.Write(instance.RotationY);
                                chunkIO.Raw.Write(instance.RotationX);
                                chunkIO.Raw.Write(instance.Roll);
                                LEB128.Write(chunkIO.Raw, (long)-1);
                                chunkIO.Raw.Write(instance.WadObjectId.TypeId);
                                chunkIO.Raw.Write(instance.Ocb);
                                chunkIO.Raw.Write(instance.Invisible);
                                chunkIO.Raw.Write(instance.ClearBody);
                                chunkIO.Raw.Write(instance.CodeBits);
                                chunkIO.Raw.Write(instance.Color);
                                chunkIO.Raw.WriteStringUTF8(instance.LuaName != null ? instance.LuaName : "");
                            }
                        }
                        else
                        {
                            using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectMovable4, LEB128.MaximumSize2Byte))
                            {
                                var instance = (MoveableInstance)o;
                                LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                chunkIO.Raw.Write(instance.Position);
                                chunkIO.Raw.Write(instance.RotationY);
                                LEB128.Write(chunkIO.Raw, (long?)instance.ScriptId ?? -1);
                                chunkIO.Raw.Write(instance.WadObjectId.TypeId);
                                chunkIO.Raw.Write(instance.Ocb);
                                chunkIO.Raw.Write(instance.Invisible);
                                chunkIO.Raw.Write(instance.ClearBody);
                                chunkIO.Raw.Write(instance.CodeBits);
                                chunkIO.Raw.Write(instance.Color);
                            }
                        }
                    }
                    else if (o is StaticInstance)
                    {
                        if (isTEN)
                        {
                            using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectStaticTombEngine2, LEB128.MaximumSize2Byte))
                            {
                                var instance = (StaticInstance)o;
                                LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                chunkIO.Raw.Write(instance.Position);
                                chunkIO.Raw.Write(instance.RotationY);
                                chunkIO.Raw.Write(0.0f); // Reserved: instance.RotationX
                                chunkIO.Raw.Write(0.0f); // Reserved: instance.Roll
                                chunkIO.Raw.Write(instance.Scale);
                                LEB128.Write(chunkIO.Raw, (long)-1);
                                chunkIO.Raw.Write(instance.WadObjectId.TypeId);
                                chunkIO.Raw.Write(instance.Color);
                                chunkIO.Raw.Write(instance.Ocb);
                                chunkIO.Raw.WriteStringUTF8(instance.LuaName != null ? instance.LuaName : string.Empty);
                            }
                        }
                        else
                        {
                            using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectStatic3, LEB128.MaximumSize2Byte))
                            {
                                var instance = (StaticInstance)o;
                                LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                chunkIO.Raw.Write(instance.Position);
                                chunkIO.Raw.Write(instance.RotationY);
                                LEB128.Write(chunkIO.Raw, (long?)instance.ScriptId ?? -1);
                                chunkIO.Raw.Write(instance.WadObjectId.TypeId);
                                chunkIO.Raw.Write(instance.Color);
                                chunkIO.Raw.Write(instance.Ocb);
                            }
                        }
                    }
                    else if (o is CameraInstance)
                    {
                        if (isTEN)
                        {
                            using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectCameraTombEngine, LEB128.MaximumSize1Byte))
                            {
                                var instance = (CameraInstance)o;
                                LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                chunkIO.Raw.Write(instance.Position);
                                LEB128.Write(chunkIO.Raw, (long)-1);
                                chunkIO.Raw.Write(instance.CameraMode == CameraInstanceMode.Locked);
                                chunkIO.Raw.Write(instance.MoveTimer);
                                chunkIO.Raw.WriteStringUTF8(instance.LuaName != null ? instance.LuaName : string.Empty);
                            }
                        }
                        else
                        {
                            using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectCamera3, LEB128.MaximumSize1Byte))
                            {
                                var instance = (CameraInstance)o;
                                LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                chunkIO.Raw.Write(instance.Position);
                                LEB128.Write(chunkIO.Raw, (long?)instance.ScriptId ?? -1);
                                chunkIO.Raw.Write((byte)instance.CameraMode);
                                chunkIO.Raw.Write(instance.MoveTimer);
                                chunkIO.Raw.Write(instance.GlideOut);
                            }
                        }
                    }
                    else if (o is SpriteInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectSprite3, LEB128.MaximumSize1Byte))
                        {
                            var instance = (SpriteInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            chunkIO.Raw.Write(instance.Position);
                            chunkIO.Raw.Write(instance.Sequence);
                            chunkIO.Raw.Write(instance.Frame);
                            chunkIO.Raw.Write(instance.Color);
                        }
                    else if (o is FlybyCameraInstance)
                        chunkIO.WriteChunk(Prj2Chunks.ObjectFlyBy, () =>
                        {
                            var instance = (FlybyCameraInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            chunkIO.Raw.Write(instance.Position);
                            chunkIO.Raw.Write(instance.RotationY);
                            chunkIO.Raw.Write(instance.RotationX);
                            chunkIO.Raw.Write(instance.Roll);
                            LEB128.Write(chunkIO.Raw, ((long?)instance.ScriptId ?? -1));
                            chunkIO.Raw.Write(instance.Speed);
                            chunkIO.Raw.Write(instance.Fov);
                            LEB128.Write(chunkIO.Raw, instance.Flags);
                            LEB128.Write(chunkIO.Raw, instance.Number);
                            LEB128.Write(chunkIO.Raw, instance.Sequence);
                            LEB128.Write(chunkIO.Raw, instance.Timer);
                        });
                    else if (o is MemoInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectMemo2, LEB128.MaximumSize3Byte))
                        {
                            var instance = (MemoInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            chunkIO.Raw.Write(instance.Position);
                            chunkIO.Raw.WriteStringUTF8(instance.Text);
                            chunkIO.Raw.Write(instance.AlwaysDisplay);
                        }
                    else if (o is SinkInstance)
                    {
                        if (isTEN)
                        {
                            using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectSinkTombEngine, LEB128.MaximumSize1Byte))
                            {
                                var instance = (SinkInstance)o;
                                LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                chunkIO.Raw.Write(instance.Position);
                                LEB128.Write(chunkIO.Raw, (long)-1);
                                chunkIO.Raw.Write(instance.Strength);
                                chunkIO.Raw.WriteStringUTF8(instance.LuaName != null ? instance.LuaName : string.Empty);
                            }
                        }
                        else
                        {
                            using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectSink, LEB128.MaximumSize1Byte))
                            {
                                var instance = (SinkInstance)o;
                                LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                chunkIO.Raw.Write(instance.Position);
                                LEB128.Write(chunkIO.Raw, (long?)instance.ScriptId ?? -1);
                                chunkIO.Raw.Write(instance.Strength);
                            }
                        }
                    }
                    else if (o is SoundSourceInstance)
                    {
                        if (isTEN)
                        {
                            using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectSoundSourceTombEngine, LEB128.MaximumSize1Byte))
                            {
                                var instance = (SoundSourceInstance)o;
                                LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                chunkIO.Raw.Write(instance.Position);
                                chunkIO.Raw.Write(instance.SoundId);
                                chunkIO.Raw.Write((int)instance.PlayMode);
                                chunkIO.Raw.WriteStringUTF8(instance.LuaName != null ? instance.LuaName : string.Empty);
                            }
                        }
                        else
                        {
                            using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectSoundSourceReallyFinal, LEB128.MaximumSize1Byte))
                            {
                                var instance = (SoundSourceInstance)o;
                                LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                chunkIO.Raw.Write(instance.Position);
                                chunkIO.Raw.Write(instance.SoundId);
                                chunkIO.Raw.Write((int)instance.PlayMode);
                                LEB128.Write(chunkIO.Raw, ((long?)instance.ScriptId ?? -1));
                            }
                        }
                    }
                    else if (o is LightInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectLight5, LEB128.MaximumSize2Byte))
                        {
                            var instance = (LightInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            LEB128.Write(chunkIO.Raw, (long)instance.Type);
                            chunkIO.Raw.Write(instance.Position);
                            chunkIO.Raw.Write(instance.RotationY);
                            chunkIO.Raw.Write(instance.RotationX);
                            chunkIO.Raw.Write(instance.Intensity);
                            chunkIO.Raw.Write(instance.Color);
                            chunkIO.Raw.Write(instance.InnerRange);
                            chunkIO.Raw.Write(instance.OuterRange);
                            chunkIO.Raw.Write(instance.InnerAngle);
                            chunkIO.Raw.Write(instance.OuterAngle);
                            chunkIO.Raw.Write(instance.Enabled);
                            chunkIO.Raw.Write(instance.IsObstructedByRoomGeometry);
                            chunkIO.Raw.Write(instance.IsDynamicallyUsed);
                            chunkIO.Raw.Write(instance.IsStaticallyUsed);
                            chunkIO.Raw.Write(instance.IsUsedForImportedGeometry);
                            chunkIO.Raw.Write((byte)instance.Quality);
                            chunkIO.Raw.Write(instance.CastDynamicShadows);
                        }
                    else if (o is PortalInstance && rooms.ContainsKey(((PortalInstance)o).AdjoiningRoom))
                        chunkIO.WriteChunkWithChildren(Prj2Chunks.ObjectPortal2, () =>
                        {
                            var instance = (PortalInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            LEB128.Write(chunkIO.Raw, instance.Area.X0);
                            LEB128.Write(chunkIO.Raw, instance.Area.Y0);
                            LEB128.Write(chunkIO.Raw, instance.Area.X1);
                            LEB128.Write(chunkIO.Raw, instance.Area.Y1);
                            LEB128.Write(chunkIO.Raw, rooms[instance.AdjoiningRoom]);
                            chunkIO.Raw.Write((byte)instance.Direction);
                            chunkIO.Raw.Write((byte)instance.Opacity);
                            chunkIO.Raw.Write((byte)instance.Effect);

                            using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectPortalMirrorProperties, LEB128.MaximumSize1Byte))
                            {
                                chunkIO.Raw.Write(instance.Properties.ReflectLara);
                                chunkIO.Raw.Write(instance.Properties.ReflectMoveables);
                                chunkIO.Raw.Write(instance.Properties.ReflectStatics);
                                chunkIO.Raw.Write(instance.Properties.ReflectSprites);
                                chunkIO.Raw.Write(instance.Properties.ReflectLights);
                            }
                        });
                    else if (o is GhostBlockInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectGhostBlock2, LEB128.MaximumSize2Byte))
                        {
                            var instance = (GhostBlockInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            LEB128.Write(chunkIO.Raw, instance.Area.X0);
                            LEB128.Write(chunkIO.Raw, instance.Area.Y0);
                            LEB128.Write(chunkIO.Raw, instance.Floor.XnZn);
                            LEB128.Write(chunkIO.Raw, instance.Floor.XnZp);
                            LEB128.Write(chunkIO.Raw, instance.Floor.XpZn);
                            LEB128.Write(chunkIO.Raw, instance.Floor.XpZp);
                            LEB128.Write(chunkIO.Raw, instance.Ceiling.XnZn);
                            LEB128.Write(chunkIO.Raw, instance.Ceiling.XnZp);
                            LEB128.Write(chunkIO.Raw, instance.Ceiling.XpZn);
                            LEB128.Write(chunkIO.Raw, instance.Ceiling.XpZp);
                        }
                    else if (o is VolumeInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectTriggerVolume4, LEB128.MaximumSize2Byte))
                        {
                            var instance = (VolumeInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));

                            var shape = instance.Shape();
                            chunkIO.Raw.Write((byte)shape);

                            switch (shape)
                            {
                                case VolumeShape.Box:
                                    {
                                        var bv = instance as BoxVolumeInstance;
                                        chunkIO.Raw.Write(bv.Size);
                                        chunkIO.Raw.Write(bv.RotationY);
                                        chunkIO.Raw.Write(bv.RotationX);
                                        chunkIO.Raw.Write(bv.Roll);
                                    }
                                    break;
                                case VolumeShape.Sphere:
                                    chunkIO.Raw.Write((instance as SphereVolumeInstance).Scale);
                                    break;
                            }
                            chunkIO.Raw.Write(instance.Position);

                            chunkIO.Raw.Write(instance.Enabled);
                            chunkIO.Raw.Write(instance.DetectInAdjacentRooms);

                            int eventSetIndex = -1;
                            if (instance.EventSet != null)
                                levelSettingIds.VolumeEventSets.TryGetValue(instance.EventSet as VolumeEventSet, out eventSetIndex);
                            chunkIO.Raw.Write(eventSetIndex);

                            chunkIO.Raw.WriteStringUTF8(instance.LuaName != null ? instance.LuaName : string.Empty);
                        }
                    else if (o is TriggerInstance)
                        using (var chunk = chunkIO.WriteChunk(Prj2Chunks.ObjectTrigger3, LEB128.MaximumSize2Byte))
                        {
                            Action<ITriggerParameter> writeTriggerParameter = (ITriggerParameter parameter) =>
                            {
                                if (parameter is TriggerParameterUshort)
                                {
                                    LEB128.Write(chunkIO.Raw, 0);
                                    LEB128.Write(chunkIO.Raw, ((TriggerParameterUshort)parameter).Key);
                                }
                                else if (parameter is ObjectInstance)
                                {
                                    LEB128.Write(chunkIO.Raw, 1);
                                    LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault((ObjectInstance)parameter));
                                }
                                else if (parameter is Room)
                                {
                                    LEB128.Write(chunkIO.Raw, 2);
                                    LEB128.Write(chunkIO.Raw, rooms.TryGetOrDefault((Room)parameter));
                                }
                                else if (parameter is TriggerParameterString)
                                {
                                    LEB128.Write(chunkIO.Raw, 3);
                                    chunkIO.Raw.WriteStringUTF8(((TriggerParameterString)parameter).Value);
                                }
                                else
                                {
                                    LEB128.Write(chunkIO.Raw, -1);
                                }
                            };

                            var instance = (TriggerInstance)o;
                            LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                            LEB128.Write(chunkIO.Raw, instance.Area.X0);
                            LEB128.Write(chunkIO.Raw, instance.Area.Y0);
                            LEB128.Write(chunkIO.Raw, instance.Area.X1);
                            LEB128.Write(chunkIO.Raw, instance.Area.Y1);
                            chunkIO.WriteChunkInt(Prj2Chunks.ObjectTrigger2Type, (long)instance.TriggerType);
                            chunkIO.WriteChunkInt(Prj2Chunks.ObjectTrigger2TargetType, (long)instance.TargetType);
                            chunkIO.WriteChunk(Prj2Chunks.ObjectTrigger2Target, () => writeTriggerParameter(instance.Target), 32);
                            chunkIO.WriteChunk(Prj2Chunks.ObjectTrigger2Timer, () => writeTriggerParameter(instance.Timer), 32);
                            chunkIO.WriteChunk(Prj2Chunks.ObjectTrigger2Extra, () => writeTriggerParameter(instance.Extra), 32);
                            chunkIO.WriteChunkInt(Prj2Chunks.ObjectTrigger2CodeBits, instance.CodeBits);
                            chunkIO.WriteChunkBool(Prj2Chunks.ObjectTrigger2OneShot, instance.OneShot);
                            chunkIO.WriteChunk(Prj2Chunks.ObjectTrigger3Plugin, () => writeTriggerParameter(instance.Plugin), 32);
                            chunkIO.WriteChunkEnd();
                        }
                    else if (o is ImportedGeometryInstance)
                    {
                        var instance = (ImportedGeometryInstance)o;
                        if (instance.Model == null || levelSettingIds.ImportedGeometries.ContainsKey(instance.Model))
                            chunkIO.WriteChunkWithChildren(Prj2Chunks.ObjectImportedGeometry4, () =>
                            {
                                LEB128.Write(chunkIO.Raw, objectInstanceLookup.TryGetOrDefault(instance, -1));
                                chunkIO.Raw.Write(instance.Position);
                                chunkIO.Raw.Write(instance.RotationY);
                                chunkIO.Raw.Write(instance.RotationX);
                                chunkIO.Raw.Write(instance.Roll);
                                chunkIO.Raw.Write(instance.Scale);
                                chunkIO.Raw.Write(instance.Color);

                                LEB128.Write(chunkIO.Raw, instance.Model == null ?
                                    -1 :
                                    levelSettingIds.ImportedGeometries[instance.Model]);

                                chunkIO.WriteChunkInt(Prj2Chunks.ObjectImportedGeometryLightingModel, (int)instance.LightingModel);
                                chunkIO.WriteChunkBool(Prj2Chunks.ObjectImportedGeometrySharpEdges, instance.SharpEdges);
                                chunkIO.WriteChunkBool(Prj2Chunks.ObjectImportedGeometryHidden, instance.Hidden);
                                chunkIO.WriteChunkBool(Prj2Chunks.ObjectImportedGeometryUseAlphaTest, instance.UseAlphaTestInsteadOfAlphaBlend);
                            });
                        else
                            logger.Warn("Imported geometry in room " + instance.Room + " refers to a model which is missing from project.");
                    }
                    else
                        logger.Warn("Object " + o + " not supported.");
                }
                chunkIO.WriteChunkEnd();
            }
        }

        private static void WriteNode(ChunkWriter chunkIO, TriggerNode node, ChunkId type)
        {
            chunkIO.WriteChunkWithChildren(type, () =>
            {
                chunkIO.WriteChunkInt(Prj2Chunks.NodeType, (int)((node is TriggerNodeCondition) ? NodeType.Condition : NodeType.Action));
                chunkIO.WriteChunkString(Prj2Chunks.NodeName, node.Name);
                chunkIO.WriteChunkInt(Prj2Chunks.NodeSize, node.Size);
                chunkIO.WriteChunkVector2(Prj2Chunks.NodeScreenPosition, node.ScreenPosition);
                chunkIO.WriteChunkVector3(Prj2Chunks.NodeColor, node.Color);
                chunkIO.WriteChunkBool(Prj2Chunks.NodeLocked, node.Locked);
                chunkIO.WriteChunkString(Prj2Chunks.NodeFunction, node.Function);

                foreach (var arg in node.Arguments)
                {
                    chunkIO.WriteChunkWithChildren(Prj2Chunks.NodeArgument2, () =>
                    {
                        chunkIO.WriteChunkString(Prj2Chunks.NodeArgumentName, arg.Name);
                        chunkIO.WriteChunkString(Prj2Chunks.NodeArgumentValue, arg.Value);
                    });
                }

                if (node.Next != null)
                    WriteNode(chunkIO, node.Next, Prj2Chunks.EventNodeNext);

                if ((node as TriggerNodeCondition)?.Else != null)
                    WriteNode(chunkIO, (node as TriggerNodeCondition).Else, Prj2Chunks.EventNodeElse);
            });
        }
    }
}
