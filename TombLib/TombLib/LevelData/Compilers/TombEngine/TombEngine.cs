using Microsoft.IO;
using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.TombEngine
{
    public sealed partial class LevelCompilerTombEngine
    {
        private static readonly RecyclableMemoryStreamManager _streamManager = new();

        private void WriteLevelTombEngine()
        {
            byte[] dynamicDataBuffer;
            using (var dynamicDataStream = _streamManager.GetStream())
            {
                var writer = new BinaryWriterEx(dynamicDataStream); // Don't dispose
                ReportProgress(80, "Writing dynamic data to memory buffer");

                // Write room dynamic data
                writer.Write(_level.ExistingRooms.Count);
                foreach (var r in _level.ExistingRooms)
                    _tempRooms[r].WriteDynamicData(writer);

                // Write items and AI objects
                writer.Write((uint)_items.Count);
                foreach (var item in _items)
                {
                    writer.Write(item.ObjectID);
                    writer.Write(item.Room);
                    writer.Write(item.X);
                    writer.Write(item.Y);
                    writer.Write(item.Z);
                    writer.Write(item.Yaw);
                    writer.Write(item.Pitch);
                    writer.Write(item.Roll);
                    writer.Write(item.Color);
                    writer.Write(item.OCB);
                    writer.Write(item.Flags);
                    writer.Write(item.LuaName);
                }

                writer.Write((uint)_aiItems.Count);
                foreach (var item in _aiItems)
                {
                    writer.Write(item.ObjectID);
                    writer.Write(item.Room);
                    writer.Write(item.X);
                    writer.Write(item.Y);
                    writer.Write(item.Z);
                    writer.Write(item.Yaw);
                    writer.Write(item.Pitch);
                    writer.Write(item.Roll);
                    writer.Write(item.OCB);
                    writer.Write(item.Flags);
                    writer.Write(item.BoxIndex);
                    writer.Write(item.LuaName);
                }

                // Write camera, flyby and sound sources
                writer.Write((uint)_cameras.Count);
                foreach (var camera in _cameras)
                {
                    writer.Write(camera.X);
                    writer.Write(camera.Y);
                    writer.Write(camera.Z);
                    writer.Write(camera.Room);
                    writer.Write(camera.Flags);
                    writer.Write(camera.Speed);
                    writer.Write(camera.LuaName);
                }

                writer.Write((uint)_flyByCameras.Count);
                writer.WriteBlockArray(_flyByCameras);

                writer.Write((uint)_sinks.Count);
                foreach (var sink in _sinks)
                {
                    writer.Write(sink.X);
                    writer.Write(sink.Y);
                    writer.Write(sink.Z);
                    writer.Write(sink.Strength);
                    writer.Write(sink.BoxIndex);
                    writer.Write(sink.LuaName);
                }

                writer.Write((uint)_soundSources.Count);
                foreach (var source in _soundSources)
                {
                    writer.Write(source.X);
                    writer.Write(source.Y);
                    writer.Write(source.Z);
                    writer.Write(source.SoundID);
                    writer.Write(source.Flags);
                    writer.Write(source.LuaName);
                }

                // Write event sets
                int eventSetCount = _level.Settings.GlobalEventSets.Count + _level.Settings.VolumeEventSets.Count;
                writer.Write((uint)eventSetCount);

                if (eventSetCount > 0)
                {
                    writer.Write((uint)_level.Settings.GlobalEventSets.Count);
                    foreach (GlobalEventSet set in _level.Settings.GlobalEventSets)
                        set.Write(writer, _level.Settings.GlobalEventSets);

                    writer.Write((uint)_level.Settings.VolumeEventSets.Count);
                    foreach (VolumeEventSet set in _level.Settings.VolumeEventSets)
                        set.Write(writer, _level.Settings.VolumeEventSets);
                }

                // Write Lua property script blob
                writer.Write(_luaPropertyScriptBlob.Key);
                writer.Write(_luaPropertyScriptBlob.Value);

                dynamicDataBuffer = dynamicDataStream.ToArray();
            }

            // Now begin to compile the geometry block in a MemoryStream
            byte[] geometryDataBuffer;
            using (var geometryDataStream = _streamManager.GetStream())
            {
                var writer = new BinaryWriterEx(geometryDataStream); // Don't dispose
                ReportProgress(85, "Writing geometry data to memory buffer");

                writer.Write(_level.ExistingRooms.Count);
                foreach (var r in _level.ExistingRooms)
                    _tempRooms[r].WriteStaticData(writer);

                // Write floordata
                var numFloorData = (uint)_floorData.Count;
                writer.Write(numFloorData);
                writer.WriteBlockArray(_floorData);

                // Write meshes
                writer.Write(_meshes.Count);
                foreach (var mesh in _meshes)
                {
                    writer.Write(mesh.Hidden);
                    writer.Write((byte)mesh.LightingType);

                    writer.Write( mesh.Sphere.Center.X);
                    writer.Write(-mesh.Sphere.Center.Y);
                    writer.Write( mesh.Sphere.Center.Z);
                    writer.Write( mesh.Sphere.Radius);

                    writer.Write(mesh.Vertices.Count);
                    foreach (var p in mesh.Vertices)
                        writer.Write(p.Position);
                    foreach (var c in mesh.Vertices)
                        writer.Write(c.Color);
                    foreach (var e in mesh.Vertices)
                        writer.Write(new Vector3(e.Glow, e.Move, e.Locked ? 0 : 1));
                    foreach (var b in mesh.Vertices)
                        for (int w = 0; w < b.BoneIndex.Length; w++)
                            writer.Write((byte)b.BoneIndex[w]);
                    foreach (var b in mesh.Vertices)
                        for (int w = 0; w < b.BoneWeight.Length; w++)
                            writer.Write((byte)(b.BoneWeight[w] * byte.MaxValue));

                    writer.Write(mesh.Buckets.Count);
                    foreach (var bucket in mesh.Buckets)
                    {
                        writer.Write(bucket.Material.Texture);
                        writer.Write(bucket.Material.BlendMode);
                        writer.Write(bucket.Material.MaterialIndex);
                        writer.Write(bucket.Material.Animated);

                        writer.Write(bucket.Polygons.Count);
                        foreach (var poly in bucket.Polygons)
                        {
                            writer.Write((int)poly.Shape);

                            writer.Write((int)poly.AnimatedSequence);
                            writer.Write((int)poly.AnimatedFrame);

                            writer.Write((float)poly.ShineStrength);

                            writer.Write(poly.Normal); 

                            foreach (int index in poly.Indices)
                                writer.Write(index);
                            foreach (var uv in poly.TextureCoordinates)
                                writer.Write(uv);
                            foreach (var n in poly.Normals)
                                writer.Write(n);
                            foreach (var t in poly.Tangents)
                                writer.Write(t);
                            foreach (var bt in poly.Binormals)
                                writer.Write(bt);
                        }
                    }
                }

                writer.Write(_meshTrees.Count);
                writer.WriteBlockArray(_meshTrees);

                writer.Write(_moveables.Count);
                foreach (var moveable in _moveables)
                    moveable.Write(writer);

                writer.Write(_staticMeshes.Count);
                writer.WriteBlockArray(_staticMeshes);

                // SPR block
                writer.Write(_spriteTextures.Count);
                writer.WriteBlockArray(_spriteTextures);

                writer.Write(_spriteSequences.Count);
                writer.WriteBlockArray(_spriteSequences);

                // Write pathfinding data
                writer.Write(_boxes.Count);
                writer.WriteBlockArray(_boxes);

                writer.Write(_overlaps.Count);
                writer.WriteBlockArray(_overlaps);

                int zoneCount = Enum.GetValues(typeof(ZoneType)).Length;
                writer.Write(zoneCount);

                foreach (int flipped in new[] { 0, 1 })
                    for (int i = 0; i < zoneCount; i++)
                        _zones.ForEach(z => writer.Write(z.Zones[flipped][i]));

                // Write mirrors
                writer.Write(_mirrors.Count);
                foreach (var mirror in _mirrors)
                {
                    writer.Write(mirror.Room);
                    writer.Write(mirror.Plane.X);
                    writer.Write(mirror.Plane.Y);
                    writer.Write(mirror.Plane.Z);
                    writer.Write(mirror.Plane.W);
                    writer.Write(mirror.ReflectLara);
                    writer.Write(mirror.ReflectMoveables);
                    writer.Write(mirror.ReflectStatics);
                    writer.Write(mirror.ReflectSprites);
                    writer.Write(mirror.ReflectLights);
                }

                // Write animated textures
                _textureInfoManager.WriteAnimatedTextures(writer);

                // Write materials
                writer.Write((uint)_materialDictionary.Count);
                foreach (var material in _materialDictionary)
                {
                    writer.Write(material.Key);
                    writer.Write((int)material.Value.Type);
                    writer.Write(material.Value.Parameters0);
                    writer.Write(material.Value.Parameters1);
                    writer.Write(material.Value.Parameters2);
                    writer.Write(material.Value.Parameters3);
                    writer.Write(material.Value.IsNormalMapFound);
                    writer.Write(material.Value.IsHeightMapFound);
                    writer.Write(material.Value.IsAmbientOcclusionMapFound);
                    writer.Write(material.Value.IsRoughnessMapFound);
                    writer.Write(material.Value.IsSpecularMapFound);
                    writer.Write(material.Value.IsEmissiveMapFound);
                }

                geometryDataBuffer = geometryDataStream.ToArray();
            }

            ReportProgress(95, "Compressing level...");

            using (var fs = new FileStream(_dest, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new BinaryWriter(fs))
            {
                long endPos;

                using (var mediaStream = _streamManager.GetStream())
                {
                    using (var mediaWriter = new BinaryWriterEx(mediaStream, true))
                    {
                        WriteTextureData(mediaWriter);

                        // Write sound metadata.
                        PrepareSoundsData();
                        WriteSoundMetadata(mediaWriter);
                        WriteSoundData(mediaWriter);
                    }

                    // TEN header
                    writer.Write(new byte[] { 0x54, 0x45, 0x4E, 0x00 });

                    // TE compiler version
                    var version = Assembly.GetExecutingAssembly().GetName().Version;
                    writer.Write(new byte[] { (byte)version.Major, (byte)version.Minor, (byte)version.Build, 0x00 });

                    // Hashed system name (reserved for quick start feature).
                    writer.Write(Math.Abs(Environment.MachineName.GetHashCode()));

                    // Checksum to detect incorrect level version on fast reload.
                    int checksum = Checksum.Calculate(mediaStream) ^ Checksum.Calculate(geometryDataBuffer);
                    writer.Write(checksum);

                    // Media data (textures and sounds) - stream compressed directly to file.
                    writer.Write(mediaStream.Length);
                    long mediaCompSizePos = fs.Position;
                    writer.Write((long)0);

                    mediaStream.Position = 0;
                    long mediaCompSize = LZ4.CompressData(mediaStream, fs, System.IO.Compression.CompressionLevel.Fastest);

                    endPos = fs.Position;
                    fs.Position = mediaCompSizePos;
                    writer.Write(mediaCompSize);
                    fs.Position = endPos;

                    ReportProgress(96, $"    Media data size: " + TextExtensions.ToDataSize(mediaCompSize));
                }
                // RecyclableMemoryStream is now disposed, releasing pooled buffers.

                // Geometry data
                writer.Write((long)geometryDataBuffer.Length);
                long geoCompSizePos = fs.Position;
                writer.Write((long)0);

                long geoCompSize = LZ4.CompressData(geometryDataBuffer, fs, System.IO.Compression.CompressionLevel.Fastest);

                endPos = fs.Position;
                fs.Position = geoCompSizePos;
                writer.Write(geoCompSize);
                fs.Position = endPos;

                ReportProgress(96, $"    Geometry data size: " + TextExtensions.ToDataSize(geoCompSize));

                // Dynamic data
                writer.Write((long)dynamicDataBuffer.Length);
                long dynCompSizePos = fs.Position;
                writer.Write((long)0);

                long dynCompSize = LZ4.CompressData(dynamicDataBuffer, fs, System.IO.Compression.CompressionLevel.Fastest);

                endPos = fs.Position;
                fs.Position = dynCompSizePos;
                writer.Write(dynCompSize);
                fs.Position = endPos;

                ReportProgress(96, $"    Dynamic data size: " + TextExtensions.ToDataSize(dynCompSize));
            }

            ReportProgress(100, "Done");
        }
    }
}
