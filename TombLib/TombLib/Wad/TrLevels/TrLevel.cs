using System;
using System.Collections.Generic;
using System.IO;
using TombLib.IO;
using TombLib.LevelData;
using TombLib.NG;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad.TrLevels
{
    // This is a class for loading objects data from original TR levels.
    // We are interested only in meshes, animations, textures.
    // Everything else will be ignored.
    public class TrLevel
    {
        internal TRVersion.Game Version;
        internal string Name;
        internal bool IsNg;

        internal byte[] TextureMap32;
        internal List<tr_mesh> Meshes = new List<tr_mesh>();
        internal List<uint> MeshPointers = new List<uint>();
        internal List<tr_animation> Animations = new List<tr_animation>();
        internal List<tr_state_change> StateChanges = new List<tr_state_change>();
        internal List<tr_anim_dispatch> AnimDispatches = new List<tr_anim_dispatch>();
        internal List<short> AnimCommands = new List<short>();
        internal List<int> MeshTrees = new List<int>();
        internal List<short> Frames = new List<short>();
        internal List<tr_moveable> Moveables = new List<tr_moveable>();
        internal List<tr_staticmesh> StaticMeshes = new List<tr_staticmesh>();
        internal List<tr_object_texture> ObjectTextures = new List<tr_object_texture>();
        internal List<tr_sprite_texture> SpriteTextures = new List<tr_sprite_texture>();
        internal List<tr_sprite_sequence> SpriteSequences = new List<tr_sprite_sequence>();
        internal List<short> SoundMap = new List<short>();
        internal List<tr_sound_details> SoundDetails = new List<tr_sound_details>();
        internal List<uint> SamplesIndices = new List<uint>();
        internal List<tr_sample> Samples = new List<tr_sample>();
        internal List<tr_color> Palette8 = new List<tr_color>();
        internal List<tr_color4> Palette16 = new List<tr_color4>();

        // Helper mesh pointers
        internal List<uint> RealPointers = new List<uint>();
        internal List<uint> HelperPointers = new List<uint>();

        public void LoadLevel(string fileName, bool allowTRNGDecryption)
        {
            bool needsDecrypt = false;
            string levelName = fileName;

            uint version;
            using (var reader = new BinaryReaderEx(File.OpenRead(fileName)))
            {
                version = reader.ReadUInt32();
                if (version == 0x00000020)
                    Version = TRVersion.Game.TR1;
                else if (version == 0x0000002D)
                    Version = TRVersion.Game.TR2;
                else if (version == 0xFF180038)
                    Version = TRVersion.Game.TR3;
                else if (version == 0xFF080038)
                    Version = TRVersion.Game.TR3;
                else if (version == 0xFF180034)
                    Version = TRVersion.Game.TR3;
                else if (version == 0x00345254)
                    Version = TRVersion.Game.TR4;
                else if (version == 0x63345254 && allowTRNGDecryption) // Encrypted TRNG level
                {
                    Version = TRVersion.Game.TR4;
                    needsDecrypt = true;
                }
                else
                    throw new Exception("Unknown game version 0x" + version.ToString("X") + ".");
            }

            // If encrypted TRNG level, make temporary copy of it and decrypt it.
            // Copy will be deleted after parsing.
            if (needsDecrypt)
            {
                levelName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_decrypted" + Path.GetExtension(fileName));
                if (!NgEncryption.DecryptLevel(fileName, levelName))
                    throw new Exception("Can't decrypt TRNG level " + fileName + ".");
            }

            using (var reader = new BinaryReaderEx(File.OpenRead(levelName)))
            {
                Name = Path.GetFileNameWithoutExtension(levelName);

                version = reader.ReadUInt32();

                if (Version == TRVersion.Game.TR4 && fileName.ToLower().Trim().EndsWith(".trc"))
                    Version = TRVersion.Game.TR5;

                // Check for NG header
                IsNg = false;
                if (Version == TRVersion.Game.TR4)
                {
                    var offset = reader.BaseStream.Position;
                    reader.BaseStream.Seek(reader.BaseStream.Length - 8, SeekOrigin.Begin);
                    var ngBuffer = reader.ReadBytes(4);
                    if (ngBuffer[0] == 0x4E && ngBuffer[1] == 0x47 && ngBuffer[2] == 0x4C && ngBuffer[3] == 0x45)
                        IsNg = true;
                    reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                }

                // Read the palette only for TR2 and TR3, TR1 has the palette near the end of the file
                if (Version is TRVersion.Game.TR2 or TRVersion.Game.TR3)
                {
                    for (int i = 0; i < 256; i++)
                    {
                        var color = new tr_color();
                        color.Red = reader.ReadByte();
                        color.Green = reader.ReadByte();
                        color.Blue = reader.ReadByte();
                        Palette8.Add(color);
                    }

                    for (int i = 0; i < 256; i++)
                    {
                        var color = new tr_color4();
                        color.Red = reader.ReadByte();
                        color.Green = reader.ReadByte();
                        color.Blue = reader.ReadByte();
                        color.Alpha = reader.ReadByte();
                        Palette16.Add(color);
                    }
                }

                byte[] texture8 = new byte[0];
                byte[] texture16 = new byte[0];
                byte[] texture32 = new byte[0];

                // Read 8 bit and 16 bit textures if version is <= TR3
                if (Version is TRVersion.Game.TR1 or TRVersion.Game.TR2 or TRVersion.Game.TR3)
                {
                    uint numTextureTiles = reader.ReadUInt32();
                    texture8 = reader.ReadBytes((int)numTextureTiles * 65536);
                    if (Version != TRVersion.Game.TR1)
                        texture16 = reader.ReadBytes((int)numTextureTiles * 131072);

                    // Later I will convert textures to 32 bit format
                }

                byte[] levelData;

                // Read 16 and 32 bit textures and uncompress them if TR4 and TR5
                if (Version is TRVersion.Game.TR4 or TRVersion.Game.TR5)
                {
                    var numRoomTiles = reader.ReadUInt16();
                    var numObjectTiles = reader.ReadUInt16();
                    var numBumpTiles = reader.ReadUInt16();

                    // 32 bit textures
                    var uncompressedSize = reader.ReadUInt32();
                    var compressedSize = reader.ReadUInt32();
                    TextureMap32 = reader.ReadBytes((int)compressedSize);
                    TextureMap32 = ZLib.DecompressData(TextureMap32);

                    // 16 bit textures (not needed)
                    uncompressedSize = reader.ReadUInt32();
                    compressedSize = reader.ReadUInt32();
                    reader.ReadBytes((int)compressedSize);

                    // Misc textures (not needed?)
                    uncompressedSize = reader.ReadUInt32();
                    compressedSize = reader.ReadUInt32();
                    reader.ReadBytes((int)compressedSize);
                }

                // Put the level geometry into a byte array
                if (Version is TRVersion.Game.TR1 or TRVersion.Game.TR2 or TRVersion.Game.TR3)
                {
                    levelData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
                }
                else if (Version == TRVersion.Game.TR4)
                {
                    var uncompressedSize = reader.ReadUInt32();
                    var compressedSize = reader.ReadUInt32();
                    levelData = reader.ReadBytes((int)compressedSize);
                    levelData = ZLib.DecompressData(levelData);
                }
                else
                {
                    reader.ReadBytes(32);

                    var uncompressedSize = reader.ReadUInt32();
                    var compressedSize = reader.ReadUInt32();
                    levelData = reader.ReadBytes((int)compressedSize);
                }

                // Now store the level data in a new stream
                using (var stream = new MemoryStream(levelData))
                {
                    using (var levelReader = new BinaryReaderEx(stream))
                    {
                        var unused = levelReader.ReadUInt32();
                        var numRooms = Version != TRVersion.Game.TR5 ? levelReader.ReadUInt16() : levelReader.ReadUInt32();

                        for (var i = 0; i < numRooms; i++)
                        {
                            // We'll skip quickly this section
                            if (Version != TRVersion.Game.TR5)
                            {
                                // Room info
                                levelReader.ReadBytes(16);

                                var numDataWords = levelReader.ReadUInt32();
                                levelReader.ReadBytes((int)numDataWords * 2);

                                var numPortals = levelReader.ReadUInt16();
                                levelReader.ReadBytes(numPortals * 32);

                                var numXsectors = levelReader.ReadUInt16();
                                var numZsectors = levelReader.ReadUInt16();
                                levelReader.ReadBytes(numXsectors * numZsectors * 8);

                                // Ambient intensity 1 & 2
                                levelReader.ReadUInt16();
                                if (Version != TRVersion.Game.TR1)
                                    levelReader.ReadUInt16();

                                // Lightmode
                                if (Version == TRVersion.Game.TR2)
                                    levelReader.ReadUInt16();

                                var numLights = levelReader.ReadUInt16();
                                if (Version == TRVersion.Game.TR1)
                                    levelReader.ReadBytes(numLights * 18);
                                if (Version == TRVersion.Game.TR2)
                                    levelReader.ReadBytes(numLights * 24);
                                if (Version == TRVersion.Game.TR3)
                                    levelReader.ReadBytes(numLights * 24);
                                if (Version == TRVersion.Game.TR4)
                                    levelReader.ReadBytes(numLights * 46);

                                var numRoomStaticMeshes = levelReader.ReadUInt16();
                                if (Version == TRVersion.Game.TR1)
                                    levelReader.ReadBytes(numRoomStaticMeshes * 18);
                                else
                                    levelReader.ReadBytes(numRoomStaticMeshes * 20);

                                // Various flags and alternate room
                                if (Version == TRVersion.Game.TR1)
                                    levelReader.ReadBytes(4);
                                if (Version == TRVersion.Game.TR2)
                                    levelReader.ReadBytes(4);
                                if (Version == TRVersion.Game.TR3)
                                    levelReader.ReadBytes(7);
                                if (Version == TRVersion.Game.TR4)
                                    levelReader.ReadBytes(7);
                            }
                            else
                            {
                                // TR5 is very different, but luckly we have a field with the total size
                                // But I read everything for debugging the new TR5 compiler
                                // XELA
                                var xela = System.Text.Encoding.ASCII.GetString(levelReader.ReadBytes(4));
                                var roomDataSize = levelReader.ReadUInt32();
                                int startPos = (int)levelReader.BaseStream.Position;

                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                var roomX = levelReader.ReadInt32();
                                var roomY = levelReader.ReadInt32();
                                var roomZ = levelReader.ReadInt32();
                                var roomYbottom = levelReader.ReadInt32();
                                var roomYtop = levelReader.ReadInt32();
                                var numZsectors = levelReader.ReadUInt16();
                                var numXsectors = levelReader.ReadUInt16();
                                var roomColor = levelReader.ReadUInt32();
                                var numLights = levelReader.ReadUInt16();
                                var numStatics = levelReader.ReadUInt16();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                var alternateRoom = levelReader.ReadUInt16();
                                var flags = levelReader.ReadUInt16();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                var roomXfloat = levelReader.ReadSingle();
                                var roomYfloat = levelReader.ReadSingle();
                                var roomZfloat = levelReader.ReadSingle();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                var numQuads = levelReader.ReadUInt32();
                                var numTriangles = levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                var lightSize = levelReader.ReadUInt32();
                                var numLights2 = levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                var numLayers = levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                var numVertices = levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadUInt32();
                                levelReader.ReadBytes(88 * numLights);
                                levelReader.ReadBytes(8 * numXsectors * numZsectors);
                                var numPortals = levelReader.ReadUInt16();
                                byte[] portals = levelReader.ReadBytes(32 * numPortals);

                                levelReader.BaseStream.Seek(startPos, SeekOrigin.Begin);
                                levelReader.BaseStream.Seek(roomDataSize, SeekOrigin.Current);

                                continue;
                            }
                        }

                        // Floordata
                        var numFloorData = levelReader.ReadUInt32();
                        levelReader.ReadBytes((int)numFloorData * 2);

                        var numMeshData = levelReader.ReadUInt32();
                        var numBytes = 0;
                        var totalBytes = 0;
                        var l = 0;

                        Meshes = new List<tr_mesh>();

                        while (totalBytes < numMeshData * 2)
                        {
                            long offset1 = levelReader.BaseStream.Position;

                            var mesh = new tr_mesh();

                            mesh.Center = new tr_vertex(levelReader.ReadInt16(), levelReader.ReadInt16(), levelReader.ReadInt16());
                            mesh.Radius = levelReader.ReadInt16();
                            levelReader.ReadInt16(); // Unknown int16, used for some kind of flags?
                            numBytes += 10;

                            mesh.NumVertices = levelReader.ReadInt16();
                            levelReader.ReadBlockArray(out mesh.Vertices, mesh.NumVertices);
                            numBytes += 2 + 6 * mesh.NumVertices;

                            mesh.Normals = new tr_vertex[0];
                            mesh.Lights = new short[0];

                            mesh.NumNormals = levelReader.ReadInt16();
                            if (mesh.NumNormals > 0)
                            {
                                levelReader.ReadBlockArray(out mesh.Normals, mesh.NumNormals);
                                numBytes += 2 + 6 * mesh.NumNormals;
                            }
                            else
                            {
                                levelReader.ReadBlockArray(out mesh.Lights, -mesh.NumNormals);
                                numBytes += 2 - 2 * mesh.NumNormals;
                            }

                            mesh.NumTexturedQuads = levelReader.ReadInt16();
                            mesh.TexturedQuads = new tr_face4[mesh.NumTexturedQuads];
                            for (var i = 0; i < mesh.NumTexturedQuads; i++)
                            {
                                var poly = new tr_face4();
                                poly.Index0 = levelReader.ReadUInt16();
                                poly.Index1 = levelReader.ReadUInt16();
                                poly.Index2 = levelReader.ReadUInt16();
                                poly.Index3 = levelReader.ReadUInt16();
                                poly.Texture = levelReader.ReadUInt16();
                                if (Version is TRVersion.Game.TR4 or TRVersion.Game.TR5)
                                    poly.LightingEffect = levelReader.ReadUInt16();
                                mesh.TexturedQuads[i] = poly;
                            }

                            mesh.NumTexturedTriangles = levelReader.ReadInt16();
                            mesh.TexturedTriangles = new tr_face3[mesh.NumTexturedTriangles];
                            for (var i = 0; i < mesh.NumTexturedTriangles; i++)
                            {
                                var poly = new tr_face3();
                                poly.Index0 = levelReader.ReadUInt16();
                                poly.Index1 = levelReader.ReadUInt16();
                                poly.Index2 = levelReader.ReadUInt16();
                                poly.Texture = levelReader.ReadUInt16();
                                if (Version is TRVersion.Game.TR4 or TRVersion.Game.TR5)
                                    poly.LightingEffect = levelReader.ReadUInt16();
                                mesh.TexturedTriangles[i] = poly;
                            }

                            if (Version is TRVersion.Game.TR1 or TRVersion.Game.TR2 or TRVersion.Game.TR3)
                            {
                                mesh.NumColoredQuads = levelReader.ReadInt16();
                                mesh.ColoredQuads = new tr_face4[mesh.NumColoredQuads];
                                for (var i = 0; i < mesh.NumColoredQuads; i++)
                                {
                                    var poly = new tr_face4();
                                    poly.Index0 = levelReader.ReadUInt16();
                                    poly.Index1 = levelReader.ReadUInt16();
                                    poly.Index2 = levelReader.ReadUInt16();
                                    poly.Index3 = levelReader.ReadUInt16();
                                    poly.Texture = levelReader.ReadUInt16();
                                    mesh.ColoredQuads[i] = poly;
                                }

                                mesh.NumColoredTriangles = levelReader.ReadInt16();
                                mesh.ColoredTriangles = new tr_face3[mesh.NumColoredTriangles];
                                for (var i = 0; i < mesh.NumColoredTriangles; i++)
                                {
                                    var poly = new tr_face3();
                                    poly.Index0 = levelReader.ReadUInt16();
                                    poly.Index1 = levelReader.ReadUInt16();
                                    poly.Index2 = levelReader.ReadUInt16();
                                    poly.Texture = levelReader.ReadUInt16();
                                    mesh.ColoredTriangles[i] = poly;
                                }

                                numBytes += 2 + mesh.NumTexturedQuads * 10;
                                numBytes += 2 + mesh.NumTexturedTriangles * 8;
                                numBytes += 2 + mesh.NumColoredQuads * 10;
                                numBytes += 2 + mesh.NumColoredTriangles * 8;
                            }
                            else
                            {
                                mesh.NumColoredQuads = 0;
                                mesh.NumColoredTriangles  = 0;
                                mesh.ColoredQuads = new tr_face4[0];
                                mesh.ColoredTriangles  = new tr_face3[0];

                                numBytes += 2 + mesh.NumTexturedQuads * 12;
                                numBytes += 2 + mesh.NumTexturedTriangles * 10;
                            }

                            long offset2 = levelReader.BaseStream.Position;
                            int diff = (int)(offset2 - offset1);
                            if (diff % 4 != 0)
                            { levelReader.ReadUInt16(); diff += 2; }

                            mesh.MeshSize = numBytes;
                            mesh.MeshPointer = totalBytes;

                            totalBytes += diff;

                            mesh.TotalBytesReadUntilThisMesh = totalBytes;

                            numBytes = 0;
                            l++;

                            Meshes.Add(mesh);
                        }

                        var numMeshPointers = levelReader.ReadUInt32();
                        MeshPointers = new List<uint>();
                        for (var i = 0; i < numMeshPointers; i++)
                        {
                            MeshPointers.Add(levelReader.ReadUInt32());
                            HelperPointers.Add(0);
                            RealPointers.Add(0);
                        }

                        // Update the real pointers
                        for (var i = 0; i < Meshes.Count; i++)
                        {
                            for (int k = 0; k < MeshPointers.Count; k++)
                            {
                                if (MeshPointers[k] == Meshes[i].MeshPointer)
                                {
                                    RealPointers[k] = (uint)i;
                                    HelperPointers[k] = (uint)i;
                                }
                            }
                        }

                        // Animations
                        var numAnimations = levelReader.ReadUInt32();
                        Animations = new List<tr_animation>();
                        for (var i = 0; i < numAnimations; i++)
                        {
                            var animation = new tr_animation();
                            animation.FrameOffset = levelReader.ReadUInt32();
                            animation.FrameRate = levelReader.ReadByte();
                            animation.FrameSize = levelReader.ReadByte();
                            animation.StateID = levelReader.ReadUInt16();
                            animation.Speed = levelReader.ReadInt32();
                            animation.Accel = levelReader.ReadInt32();
                            if (Version is TRVersion.Game.TR4 or TRVersion.Game.TR5)
                            {
                                animation.SpeedLateral = levelReader.ReadInt32();
                                animation.AccelLateral = levelReader.ReadInt32();
                            }
                            animation.FrameStart = levelReader.ReadUInt16();
                            animation.FrameEnd = levelReader.ReadUInt16();
                            animation.NextAnimation = levelReader.ReadUInt16();
                            animation.NextFrame = levelReader.ReadUInt16();
                            animation.NumStateChanges = levelReader.ReadUInt16();
                            animation.StateChangeOffset = levelReader.ReadUInt16();
                            animation.NumAnimCommands = levelReader.ReadUInt16();
                            animation.AnimCommand = levelReader.ReadUInt16();
                            Animations.Add(animation);
                        }

                        // State changes
                        var numStateChanges = levelReader.ReadUInt32();
                        StateChanges = new List<tr_state_change>();
                        for (var i = 0; i < numStateChanges; i++)
                        {
                            var stateChange = new tr_state_change();
                            stateChange.StateID = levelReader.ReadUInt16();
                            stateChange.NumAnimDispatches = levelReader.ReadUInt16();
                            stateChange.AnimDispatch = levelReader.ReadUInt16();
                            StateChanges.Add(stateChange);
                        }

                        // Anim dispatches
                        var numDispatches = levelReader.ReadUInt32();
                        AnimDispatches = new List<tr_anim_dispatch>();
                        for (var i = 0; i < numDispatches; i++)
                        {
                            var dispatch = new tr_anim_dispatch();
                            dispatch.Low = levelReader.ReadUInt16();
                            dispatch.High = levelReader.ReadUInt16();
                            dispatch.NextAnimation = levelReader.ReadUInt16();
                            dispatch.NextFrame = levelReader.ReadUInt16();
                            AnimDispatches.Add(dispatch);
                        }

                        // Anim commands
                        var numAnimCommands = levelReader.ReadUInt32();
                        AnimCommands = new List<short>();
                        for (var i = 0; i < numAnimCommands; i++)
                            AnimCommands.Add(levelReader.ReadInt16());

                        // Mesh trees
                        var numMeshTrees = levelReader.ReadUInt32();
                        MeshTrees = new List<int>();
                        for (var i = 0; i < numMeshTrees; i++)
                            MeshTrees.Add(levelReader.ReadInt32());

                        // Keyframes
                        var numFrames = levelReader.ReadUInt32();
                        Frames = new List<short>();
                        for (var i = 0; i < numFrames; i++)
                            Frames.Add(levelReader.ReadInt16());

                        // Moveables
                        var numMoveables = levelReader.ReadUInt32();
                        Moveables = new List<tr_moveable>();
                        for (var i = 0; i < numMoveables; i++)
                        {
                            var moveable = new tr_moveable();
                            moveable.ObjectID = levelReader.ReadUInt32();
                            moveable.NumMeshes = levelReader.ReadUInt16();
                            moveable.StartingMesh = levelReader.ReadUInt16();
                            moveable.MeshTree = levelReader.ReadUInt32();
                            moveable.FrameOffset = levelReader.ReadUInt32();
                            moveable.Animation = levelReader.ReadInt16();

                            if (Version == TRVersion.Game.TR5)
                                levelReader.ReadUInt16();

                            Moveables.Add(moveable);
                        }

                        // Static meshes
                        var numStaticMeshes = levelReader.ReadUInt32();
                        StaticMeshes = new List<tr_staticmesh>();
                        for (var i = 0; i < numStaticMeshes; i++)
                        {
                            var staticMesh = new tr_staticmesh();
                            staticMesh.ObjectID = levelReader.ReadUInt32();
                            staticMesh.Mesh = levelReader.ReadUInt16();

                            var visibilityBox = new tr_bounding_box();
                            visibilityBox.X1 = levelReader.ReadInt16();
                            visibilityBox.X2 = levelReader.ReadInt16();
                            visibilityBox.Y1 = levelReader.ReadInt16();
                            visibilityBox.Y2 = levelReader.ReadInt16();
                            visibilityBox.Z1 = levelReader.ReadInt16();
                            visibilityBox.Z2 = levelReader.ReadInt16();

                            var collisionBox = new tr_bounding_box();
                            collisionBox.X1 = levelReader.ReadInt16();
                            collisionBox.X2 = levelReader.ReadInt16();
                            collisionBox.Y1 = levelReader.ReadInt16();
                            collisionBox.Y2 = levelReader.ReadInt16();
                            collisionBox.Z1 = levelReader.ReadInt16();
                            collisionBox.Z2 = levelReader.ReadInt16();

                            staticMesh.VisibilityBox = visibilityBox;
                            staticMesh.CollisionBox = collisionBox;

                            staticMesh.Flags = levelReader.ReadUInt16();

                            StaticMeshes.Add(staticMesh);
                        }

                        // If version <= TR2 object textures are here
                        if (Version is TRVersion.Game.TR1 or TRVersion.Game.TR2)
                        {
                            var numObjectTextures = levelReader.ReadUInt32();
                            for (var i = 0; i < numObjectTextures; i++)
                            {
                                var objectTexture = new tr_object_texture();
                                objectTexture.Attributes = levelReader.ReadUInt16();
                                objectTexture.TileAndFlags = levelReader.ReadUInt16();
                                objectTexture.Vertices = new tr_object_texture_vert[4];
                                for (int j = 0; j < 4; j++)
                                {
                                    var vert = new tr_object_texture_vert();
                                    vert.X = levelReader.ReadUInt16();
                                    vert.Y = levelReader.ReadUInt16();
                                    objectTexture.Vertices[j] = vert;
                                }
                                ObjectTextures.Add(objectTexture);
                            }
                        }

                        // SPR marker
                        var marker = "";
                        if (Version == TRVersion.Game.TR4)
                            marker = System.Text.Encoding.ASCII.GetString(levelReader.ReadBytes(3));
                        if (Version == TRVersion.Game.TR5)
                            marker = System.Text.Encoding.ASCII.GetString(levelReader.ReadBytes(4));

                        // Sprite textures
                        var numSpriteTextures = levelReader.ReadUInt32();
                        for (var i = 0; i < numSpriteTextures; i++)
                        {
                            var sprite = new tr_sprite_texture();
                            sprite.Tile = levelReader.ReadUInt16();
                            sprite.X = levelReader.ReadByte();
                            sprite.Y = levelReader.ReadByte();
                            sprite.Width = levelReader.ReadUInt16();
                            sprite.Height = levelReader.ReadUInt16();
                            sprite.LeftSide = levelReader.ReadInt16();
                            sprite.TopSide = levelReader.ReadInt16();
                            sprite.RightSide = levelReader.ReadInt16();
                            sprite.BottomSide = levelReader.ReadInt16();
                            SpriteTextures.Add(sprite);
                        }

                        // Sprite sequences
                        var numSpriteSequences = levelReader.ReadUInt32();
                        for (var i = 0; i < numSpriteSequences; i++)
                        {
                            var sequence = new tr_sprite_sequence();
                            sequence.ObjectID = levelReader.ReadInt32();
                            sequence.NegativeLength = levelReader.ReadInt16();
                            sequence.Offset = levelReader.ReadInt16();
                            SpriteSequences.Add(sequence);
                        }

                        // Cameras
                        var numCameras = levelReader.ReadUInt32();
                        levelReader.ReadBytes((int)numCameras * 16);

                        // Flyby cameras
                        if (Version is TRVersion.Game.TR4 or TRVersion.Game.TR5)
                        {
                            var numFlybyCameras = levelReader.ReadUInt32();
                            levelReader.ReadBytes((int)numFlybyCameras * 40);
                        }

                        // Sound sources
                        var numSoundSources = levelReader.ReadUInt32();
                        levelReader.ReadBytes((int)numSoundSources * 16);

                        // Boxes
                        var numBoxes = levelReader.ReadUInt32();
                        levelReader.ReadBytes((int)numBoxes * (Version == TRVersion.Game.TR1 ? 20 : 8));

                        // Overlaps
                        var numOverlaps = levelReader.ReadUInt32();
                        levelReader.ReadBytes((int)numOverlaps * 2);

                        // Zones
                        levelReader.ReadBytes((int)numBoxes * (Version == TRVersion.Game.TR1 ? 12 : 20));

                        // Animated textures
                        var numAnimatedTextures = levelReader.ReadUInt32();
                        levelReader.ReadBytes((int)numAnimatedTextures * 2);

                        // If version >= TR3, object textures are here
                        if (Version is TRVersion.Game.TR3 or TRVersion.Game.TR4 or TRVersion.Game.TR5)
                        {
                            if (Version == TRVersion.Game.TR4)
                                marker = System.Text.Encoding.ASCII.GetString(levelReader.ReadBytes(4));
                            if (Version == TRVersion.Game.TR5)
                                marker = System.Text.Encoding.ASCII.GetString(levelReader.ReadBytes(5));

                            var numObjectTextures = levelReader.ReadUInt32();
                            for (var i = 0; i < numObjectTextures; i++)
                            {
                                var objectTexture = new tr_object_texture();
                                objectTexture.Attributes = levelReader.ReadUInt16();
                                objectTexture.TileAndFlags = levelReader.ReadUInt16();
                                if (Version is TRVersion.Game.TR4 or TRVersion.Game.TR5)
                                    objectTexture.NewFlags = levelReader.ReadUInt16();
                                objectTexture.Vertices = new tr_object_texture_vert[4];
                                for (int j = 0; j < 4; j++)
                                {
                                    var vert = new tr_object_texture_vert();
                                    vert.X = levelReader.ReadUInt16();
                                    vert.Y = levelReader.ReadUInt16();
                                    objectTexture.Vertices[j] = vert;
                                }
                                if (Version == TRVersion.Game.TR4)
                                    levelReader.ReadBytes(16);
                                if (Version == TRVersion.Game.TR5)
                                    levelReader.ReadBytes(18);
                                ObjectTextures.Add(objectTexture);
                            }
                        }

                        // Items
                        var numEntities = levelReader.ReadUInt32();
                        levelReader.ReadBytes((int)numEntities * (Version == TRVersion.Game.TR1 ? 22 : 24));

                        if (Version is TRVersion.Game.TR1 or TRVersion.Game.TR2 or TRVersion.Game.TR3)
                        {
                            // Lightmap
                            levelReader.ReadBytes(8192);

                            // Palette
                            if (Version == TRVersion.Game.TR1)
                            {
                                for (var i = 0; i < 256; i++)
                                {
                                    var color = new tr_color();
                                    color.Red = levelReader.ReadByte();
                                    color.Green = levelReader.ReadByte();
                                    color.Blue = levelReader.ReadByte();
                                    Palette8.Add(color);
                                }
                            }

                            // Cinematic frames
                            var numCinematicFrames = levelReader.ReadUInt16();
                            levelReader.ReadBytes(numCinematicFrames * 16);
                        }

                        // AI items
                        if (Version is TRVersion.Game.TR4 or TRVersion.Game.TR5)
                        {
                            var numAI = levelReader.ReadUInt32();
                            levelReader.ReadBytes((int)numAI * 24);
                        }

                        // Demo data
                        var numDemoData = levelReader.ReadUInt16();
                        // NumDemoData in TRNG is used for soundmap length
                        if (!IsNg)
                            levelReader.ReadBytes(numDemoData);

                        // Sound map
                        var soundMapSize = TrCatalog.PredictSoundMapSize(Version, IsNg, numDemoData);
                        for (var i = 0; i < soundMapSize; i++)
                            SoundMap.Add(levelReader.ReadInt16());

                        // Sound details
                        var numSoundDetails = levelReader.ReadUInt32();
                        for (var i = 0; i < numSoundDetails; i++)
                        {
                            var soundDetails = new tr_sound_details();
                            if (Version is TRVersion.Game.TR1 or TRVersion.Game.TR2)
                            {
                                soundDetails.Sample = levelReader.ReadInt16();
                                soundDetails.Volume = levelReader.ReadUInt16();
                                soundDetails.Range = 8;
                                soundDetails.Chance = levelReader.ReadUInt16();
                                soundDetails.Pitch = 128;
                                soundDetails.Characteristics = levelReader.ReadUInt16();
                            }
                            else
                            {
                                soundDetails.Sample = levelReader.ReadInt16();
                                soundDetails.Volume = levelReader.ReadByte();
                                soundDetails.Range = levelReader.ReadByte();
                                soundDetails.Chance = levelReader.ReadByte();
                                soundDetails.Pitch = levelReader.ReadByte();
                                soundDetails.Characteristics = levelReader.ReadUInt16();
                            }
                            SoundDetails.Add(soundDetails);
                        }

#if DUMP_SOUND_INFOS
                        using (var writer = new StreamWriter(File.OpenWrite("sounds_" + Path.GetFileName(fileName) + ".txt")))
                        {
                            writer.WriteLine("");
                            writer.WriteLine("-------------------" + Path.GetFileName(fileName) + "-------------------");
                            writer.WriteLine("");

                            var index = 0;
                            for (int r = 0; r < SoundMap.Count; r++)
                            {
                                if (SoundMap[r] == -1)
                                {
                                    writer.WriteLine("Empty:");
                                    continue;
                                }

                                var sd = SoundDetails[index];

                                float divider = (Version < TRVersion.Game.TR3 ? 32767.0f : 255.0f);
                                var vol = (int)Math.Round(((float)sd.Volume / divider) * 99.0f);
                                var rad = sd.Range;
                                var ch  = (int)Math.Round(((float)sd.Chance / divider) * 99.0f);

                                var RandomizePitch =  ((sd.Characteristics & 0x2000) != 0)  ? "P" : " ";
                                var RandomizeVolume = ((sd.Characteristics & 0x4000) != 0)  ? "V" : " ";
                                var DisablePanning =  ((sd.Characteristics & 0x1000) != 0)  ? "N" : " ";

                                string loop;
                                var LoopBehaviour = (sd.Characteristics & 0x03);

                                if (Version == TRVersion.Game.TR1)
                                {
                                    switch(LoopBehaviour)
                                    {
                                        case 0:
                                            loop = "W";
                                            break;
                                        case 1:
                                        default:
                                            loop = " ";
                                            break;
                                        case 2:
                                            loop = "L";
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (LoopBehaviour)
                                    {
                                        case 0:
                                        default:
                                            loop = " ";
                                            break;
                                        case 1:
                                            loop = "w";
                                            break;
                                        case 2:
                                            loop = "R";
                                            break;
                                        case 3:
                                            loop = "L";
                                            break;
                                    }
                                }

                                writer.WriteLine((TrCatalog.GetOriginalSoundName(Version, (uint)r) + ":").PadRight(24) + "sample  " + "VOL" + vol.ToString("D2") + "   " + 
                                                 RandomizePitch + " " + RandomizeVolume + " " + DisablePanning + " " + loop + (ch == 0 ? "      " : "  CH" + ch.ToString("D2")) + "  RAD" + rad.ToString("D2"));
                                index++;
                            }
                        }
#endif

                        // In TR1 waves are here
                        if (Version == TRVersion.Game.TR1)
                        {
                            var numSamples = levelReader.ReadUInt32();
                            var samplesBytesRead = 0;
                            while (samplesBytesRead < numSamples)
                            {
                                var sample = new tr_sample();

                                // Check for RIFF header
                                var riff = System.Text.Encoding.ASCII.GetString(levelReader.ReadBytes(4));
                                samplesBytesRead += 4;
                                if (riff != "RIFF")
                                    continue;

                                // Read the chunk size (in this case, the entire file size)
                                var fileSize = levelReader.ReadInt32();
                                samplesBytesRead += 4;

                                // Write to a MemoryStream
                                using (var ms = new MemoryStream())
                                {
                                    using (var writerSample = new BinaryWriterEx(ms))
                                    {
                                        writerSample.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                                        writerSample.Write(fileSize);
                                        writerSample.Write(levelReader.ReadBytes(fileSize));
                                        samplesBytesRead += fileSize;
                                    }

                                    sample.Data = ms.ToArray();
                                }

                                Samples.Add(sample);
                            }
                        }

                        // Samples indices
                        var numSamplesIndices = levelReader.ReadUInt32();
                        for (var i = 0; i < numSamplesIndices; i++)
                            SamplesIndices.Add(levelReader.ReadUInt32());
                    }
                }

                // Now for TR4 and TR5 there are sounds
                if (Version is TRVersion.Game.TR4 or TRVersion.Game.TR5)
                {
                    var numSamples = reader.ReadUInt32();
                    for (var i = 0; i < numSamples; i++)
                    {
                        var uncompressedWaveSize = reader.ReadUInt32();
                        var compressedWaveSize = reader.ReadUInt32();
                        var waveData = reader.ReadBytes((int)compressedWaveSize);
                        var sample = new tr_sample();
                        sample.Data = waveData;
                        Samples.Add(sample);
                    }
                }

                // Convert textures to 32 bit format if needed
                if (Version == TRVersion.Game.TR1)
                {
                    // We'll use palette8 and texture8
                    var numPages = texture8.Length / 65536;
                    var width = 256;
                    var height = numPages * 256;
                    TextureMap32 = new byte[numPages * 65536 * 4];

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var index = texture8[y * 256 + x];
                            var r = Palette8[index].Red * 4;
                            var g = Palette8[index].Green * 4;
                            var b = Palette8[index].Blue * 4;
                            var a = (byte)(index == 0 ? 0 : 255);

                            TextureMap32[y * 1024 + x * 4 + 0] = (byte)b;
                            TextureMap32[y * 1024 + x * 4 + 1] = (byte)g;
                            TextureMap32[y * 1024 + x * 4 + 2] = (byte)r;
                            TextureMap32[y * 1024 + x * 4 + 3] = a;
                        }
                    }
                }

                if (Version is TRVersion.Game.TR2 or TRVersion.Game.TR3)
                {
                    // We'll use texture16
                    var numPages = texture16.Length / 131072;
                    var width = 256;
                    var height = numPages * 256;
                    TextureMap32 = new byte[numPages * 65536 * 4];

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var color = (texture16[y * 512 + 2 * x + 1] << 8) + texture16[y * 512 + 2 * x];

                            var r = ((color & 0x7c00) >> 10) * 8;
                            var g = ((color & 0x03e0) >> 5) * 8;
                            var b = ((color & 0x001f) >> 0) * 8;
                            var a = (color & 0x8000) != 0 ? 255 : 0;

                            TextureMap32[y * 1024 + x * 4 + 0] = (byte)b;
                            TextureMap32[y * 1024 + x * 4 + 1] = (byte)g;
                            TextureMap32[y * 1024 + x * 4 + 2] = (byte)r;
                            TextureMap32[y * 1024 + x * 4 + 3] = (byte)a;
                        }
                    }
                }
            }

            // Delete decrypted level after processing
            if (needsDecrypt && levelName != fileName && File.Exists(levelName))
                File.Delete(levelName);

            // If TR2 or TR3, read samples from SFX files
            if (Version is TRVersion.Game.TR2 or TRVersion.Game.TR3)
            {
                string path = PathC.TryFindFile(Path.GetDirectoryName(fileName), "data/main.sfx", 4, 4);
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    using (var sampleReader = new BinaryReaderEx(File.OpenRead(path)))
                    {
                        while (sampleReader.BaseStream.Position < sampleReader.BaseStream.Length)
                        {
                            var sample = new tr_sample();

                            // Check for RIFF header
                            var riff = System.Text.Encoding.ASCII.GetString(sampleReader.ReadBytes(4));
                            if (riff != "RIFF")
                                continue;

                            // Read the chunk size (in this case, the entire file size)
                            var fileSize = sampleReader.ReadInt32();

                            // Write to a MemoryStream
                            using (var ms = new MemoryStream())
                            {
                                using (var writerSample = new BinaryWriterEx(ms))
                                {
                                    writerSample.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                                    writerSample.Write(fileSize);
                                    writerSample.Write(sampleReader.ReadBytes(fileSize));
                                }

                                sample.Data = ms.ToArray();
                            }

                            Samples.Add(sample);
                        }
                    }
                }
            }
        }

        internal int GetNextMoveableWithAnimations(int current)
        {
            for (int i = current + 1; i < Moveables.Count; i++)
                if (Moveables[i].Animation != -1)
                    return i;
            return -1;
        }

        internal tr_mesh GetMeshFromPointer(int pointer)
        {
            return Meshes[(int)RealPointers[pointer]];
        }
    }
}
