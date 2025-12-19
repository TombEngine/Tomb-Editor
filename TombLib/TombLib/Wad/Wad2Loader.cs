using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib.IO;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public static class Wad2Loader
    {
        public static Wad2 LoadFromFile(string fileName, bool withSounds)
        {
            Wad2 result;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                result = LoadFromStream(fileStream, fileName);
            result.FileName = fileName;

            // Load additional XML file if it exists
            if (withSounds)
            {
                var xmlFile = Path.ChangeExtension(fileName, "xml");
                if (File.Exists(xmlFile))
                {
                    result.Sounds = WadSounds.ReadFromFile(xmlFile);
                }
            }

            // Set timestamp as file creation time, if metadata is missing
            if (result.Timestamp == DateTime.MinValue)
                result.Timestamp = File.GetLastWriteTime(fileName);

            return result;
        }

        public static Wad2 LoadFromStream(Stream stream, string fileName)
        {
            byte[] magicNumber = new byte[4];

            stream.Read(magicNumber, 0, 4);
            stream.Seek(-4, SeekOrigin.Current);

            if (magicNumber.SequenceEqual(Wad2Chunks.MagicNumberObsolete))
                throw new NotImplementedException("Loaded wad2 version is deprecated and not supported. Please use version 1.3.15 or earlier.");
            else
                using (var chunkIO = new ChunkReader(Wad2Chunks.MagicNumber, stream, Wad2Chunks.ChunkList))
                    return LoadWad2(chunkIO, fileName);
        }

        private static Wad2 LoadWad2(ChunkReader chunkIO, string fileName)
        {
            var wad = new Wad2() { Timestamp = DateTime.MinValue };
            wad.FileName = fileName;

            Dictionary<long, WadTexture> textures = null;
            Dictionary<long, WadSprite> sprites = null;

            bool incompatible = true;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id == Wad2Chunks.GameVersion)
                {
                    wad.GameVersion = (TRVersion.Game)chunkIO.ReadChunkLong(chunkSize);
                    return true;
                }
                else if (id == Wad2Chunks.SoundSystem)
                {
                    // This scenario should never happen because we always write XML sound system as of now.
                    incompatible = ((SoundSystem)chunkIO.ReadChunkLong(chunkSize)) == SoundSystem.None;
                    return true;
                }
                else if (LoadMetadata(chunkIO, id, wad))
                    return true;
                else if (LoadTextures(chunkIO, id, wad, ref textures))
                    return true;
                else if (LoadSprites(chunkIO, id, wad, ref sprites))
                    return true;
                else if (LoadSpriteSequences(chunkIO, id, wad, sprites))
                    return true;
                else if (LoadMoveables(chunkIO, id, wad, textures))
                    return true;
                else if (LoadStatics(chunkIO, id, wad, textures))
                    return true;
                else if (LoadAnimatedTextures(chunkIO, id, wad, textures))
                    return true;
                return false;
            });

            if (incompatible)
                throw new NotImplementedException("Loaded wad2 version is deprecated and not supported. Please use version 1.3.15 or earlier.");

            wad.HasUnknownData = chunkIO.UnknownChunksFound;
            return wad;
        }

        private static bool LoadAnimatedTextures(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, Dictionary<long, WadTexture> textures)
        {
            if (idOuter != Wad2Chunks.AnimatedTextureSets)
                return false;

                var animatedTextureSets = new List<AnimatedTextureSet>();
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 != Wad2Chunks.AnimatedTextureSet)
                        return false;

                    var set = new AnimatedTextureSet();
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.AnimatedTextureSetName)
                            set.Name = chunkIO.ReadChunkString(chunkSize3);
                        else if (id3 == Wad2Chunks.AnimatedTextureSetExtraInfo) // Legacy!
                        {
                            set.AnimationType = (AnimatedTextureAnimationType)LEB128.ReadByte(chunkIO.Raw);
                            set.Fps = LEB128.ReadSByte(chunkIO.Raw);
                            if (set.Fps == 0.0f)
                                set.Fps = 15.0f;
                            set.UvRotate = LEB128.ReadSByte(chunkIO.Raw);
                        }
                        else if (id3 == Wad2Chunks.AnimatedTextureSetType)
                        {
                            set.AnimationType = (AnimatedTextureAnimationType)chunkIO.ReadChunkInt(chunkSize3);
                        }
                        else if (id3 == Wad2Chunks.AnimatedTextureSetFps)
                        {
                            set.Fps = chunkIO.ReadChunkFloat(chunkSize3);
                        }
                        else if (id3 == Wad2Chunks.AnimatedTextureSetUvRotate)
                        {
                            set.UvRotate = chunkIO.ReadChunkInt(chunkSize3);
                        }
						else if (id3 == Wad2Chunks.AnimatedTextureSetTenUvRotateDirection)
						{
							set.TenUvRotateDirection = chunkIO.ReadChunkFloat(chunkSize3);
						}
						else if (id3 == Wad2Chunks.AnimatedTextureSetTenUvRotateSpeed)
						{
							set.TenUvRotateSpeed = chunkIO.ReadChunkFloat(chunkSize3);
						}
						else if (id3 == Wad2Chunks.AnimatedTextureFrames)
                        {
                            var frames = new List<AnimatedTextureFrame>();
                            chunkIO.ReadChunks((id4, chunkSize4) =>
                            {
                                if (id4 != Wad2Chunks.AnimatedTextureFrame)
                                    return false;

                                frames.Add(new AnimatedTextureFrame
                                {
                                    Texture = textures[LEB128.ReadLong(chunkIO.Raw)],
                                    TexCoord0 = chunkIO.Raw.ReadVector2(),
                                    TexCoord1 = chunkIO.Raw.ReadVector2(),
                                    TexCoord2 = chunkIO.Raw.ReadVector2(),
                                    TexCoord3 = chunkIO.Raw.ReadVector2(),
                                    Repeat = Math.Max(LEB128.ReadInt(chunkIO.Raw), 1)
                                });
                                return true;
                            });

                            set.Frames = frames;
                        }
                        else
                            return false;
                        return true;
                    });
                    animatedTextureSets.Add(set);
                    return true;
                });

            wad.AnimatedTextureSets = animatedTextureSets;

            return true;
        }

        private static bool LoadMetadata(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad)
        {
            if (idOuter != Wad2Chunks.Metadata)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id == Wad2Chunks.Timestamp)
                {
                    wad.Timestamp = new DateTime(LEB128.ReadInt(chunkIO.Raw),   // Year
                                                 LEB128.ReadInt(chunkIO.Raw),   // Month
                                                 LEB128.ReadInt(chunkIO.Raw),   // Day
                                                 LEB128.ReadInt(chunkIO.Raw),   // Hours
                                                 LEB128.ReadInt(chunkIO.Raw),   // Minutes
                                                 LEB128.ReadInt(chunkIO.Raw));  // Seconds

                    chunkIO.ReadChunks((id2, chunkSize2) => { return false; });
                    return true;
                }
                else if (id == Wad2Chunks.UserNotes)
                {
                    wad.UserNotes = chunkIO.ReadChunkString(chunkSize);
                    return true;
                }
                else
                    return false;
            });
            return true;
        }

        private static bool LoadTextures(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, ref Dictionary<long, WadTexture> outTextures)
        {
            if (idOuter != Wad2Chunks.Textures)
                return false;

            Dictionary<long, WadTexture> textures = new Dictionary<long,  WadTexture>();
            long obsoleteIndex = 0; // Move this into each chunk once we got rid of old style *.wad2 files.

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Texture)
                    return false;

                var width = LEB128.ReadInt(chunkIO.Raw);
                var height = LEB128.ReadInt(chunkIO.Raw);
                var name = string.Empty;
                var relativePath = string.Empty;

                byte[] textureData = null;
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.TextureIndex)
                        obsoleteIndex = chunkIO.ReadChunkLong(chunkSize2);
                    else if (id2 == Wad2Chunks.TextureName)
                        name = chunkIO.ReadChunkString(chunkSize2);
					else if (id2 == Wad2Chunks.TextureRelativePath)
						relativePath = chunkIO.ReadChunkString(chunkSize2);
					else if (id2 == Wad2Chunks.TextureData)
                        textureData = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                    else
                        return false;
                    return true;
                });

				// NOTE: we'll always have data there, but it should be loaded 
				// only if RelativePath is null or empty, meaning that this is 
				// an embedded texture.

				var texture = ImageC.Magenta;
				string absolutePath = null;
                bool textureLoaded = false;

                if (!string.IsNullOrEmpty(relativePath))
                {
                    absolutePath = Path.GetFullPath(PathC.IsTrulyAbsolutePath(name) ? name : Path.Combine(Path.GetDirectoryName(wad.FileName), name));
                    try
                    {
                        texture = ImageC.FromFile(absolutePath);
						textureLoaded = true;
					}
                    catch (Exception ex)
                    {
					}
				}

                // At this point, if the texture is embedded or if external but an error occurred,
                // we fallback using the data stored inside the Wad2 file.

                if (!textureLoaded && textureData is not null)
                {
                    texture = ImageC.FromByteArray(textureData, width, height);
                    name = null;
                    textureLoaded = true;
                }

				texture.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));
				texture.FileName = name;

				var wadTexture = new WadTexture(texture);
				wadTexture.AbsolutePath = absolutePath;
                textures.Add(obsoleteIndex++, wadTexture);

                return true;
            });

            outTextures = textures;
            return true;
        }

        private static WadMesh LoadMesh(ChunkReader chunkIO, long chunkSize, Dictionary<long, WadTexture> textures)
        {
            var mesh = new WadMesh();
            long obsoleteIndex = 0;

            chunkIO.ReadChunks((id2, chunkSize2) =>
            {
                if (id2 == Wad2Chunks.MeshIndex)
                    obsoleteIndex = chunkIO.ReadChunkLong(chunkSize2);
                else if (id2 == Wad2Chunks.MeshName)
                    mesh.Name = chunkIO.ReadChunkString(chunkSize2);
                else if (id2 == Wad2Chunks.MeshSphere)
                {
                    // Read bounding sphere
                    float radius = 0;
                    Vector3 center = Vector3.Zero;
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshSphereCenter)
                            center = chunkIO.ReadChunkVector3(chunkSize3);
                        else if (id3 == Wad2Chunks.MeshSphereRadius)
                            radius = chunkIO.ReadChunkFloat(chunkSize3);
                        else
                            return false;
                        return true;
                    });
                    mesh.BoundingSphere = new BoundingSphere(center, radius);
                }
                else if (id2 == Wad2Chunks.MeshBoundingBox)
                {
                    // Read bounding box
                    Vector3 min = Vector3.Zero;
                    Vector3 max = Vector3.Zero;
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshBoundingBoxMin)
                            min = chunkIO.ReadChunkVector3(chunkSize3);
                        else if (id3 == Wad2Chunks.MeshBoundingBoxMax)
                            max = chunkIO.ReadChunkVector3(chunkSize3);
                        else
                            return false;
                        return true;
                    });
                    mesh.BoundingBox = new BoundingBox(min, max);
                }
                else if (id2 == Wad2Chunks.MeshVertexPositions)
                {
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshVertexPosition)
                            mesh.VertexPositions.Add(chunkIO.ReadChunkVector3(chunkSize3));
                        else
                            return false;
                        return true;
                    });
                }
                else if (id2 == Wad2Chunks.MeshVertexNormals)
                {
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshVertexNormal)
                            mesh.VertexNormals.Add(chunkIO.ReadChunkVector3(chunkSize3));
                        else
                            return false;
                        return true;
                    });
                }
                else if (id2 == Wad2Chunks.MeshVertexShades)
                {
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshVertexShade)
                            mesh.VertexColors.Add(new Vector3((8191.0f - chunkIO.ReadChunkShort(chunkSize3)) / 8191.0f));
                        else if (id3 == Wad2Chunks.MeshVertexColor)
                            mesh.VertexColors.Add(chunkIO.ReadChunkVector3(chunkSize3));
                        else
                            return false;
                        return true;
                    });
                }
                else if (id2 == Wad2Chunks.MeshVertexAttributes)
                {
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshVertexAttribute)
                        {
                            var attr = new VertexAttributes();
                            attr.Glow = LEB128.ReadInt(chunkIO.Raw);
                            attr.Move = LEB128.ReadInt(chunkIO.Raw);
                            mesh.VertexAttributes.Add(attr);

                            chunkIO.ReadChunks((id4, chunkSize4) =>
                            {
                                return false;
                            });
                        }
                        else
                            return false;
                        return true;
                    });
                }
                else if (id2 == Wad2Chunks.MeshVertexWeights)
                {
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshVertexWeight)
                        {
                            var weight = new VertexWeight();
                            for (int w = 0; w < weight.Index.Length; w++)
                            {
                                weight.Index[w] = chunkIO.Raw.ReadInt32();
                                weight.Weight[w] = chunkIO.Raw.ReadSingle();
                            }

                            mesh.VertexWeights.Add(weight);

                            chunkIO.ReadChunks((id4, chunkSize4) =>
                            {
                                return false;
                            });
                        }
                        else
                            return false;
                        return true;
                    });
                }
                else if (id2 == Wad2Chunks.MeshLightingType)
                {
                    mesh.LightingType = (WadMeshLightingType)chunkIO.ReadChunkInt(chunkSize2);
                }
                else if (id2 == Wad2Chunks.MeshVisibility)
                {
                    mesh.Hidden = chunkIO.ReadChunkBool(chunkSize2);
                }
                else if (id2 == Wad2Chunks.MeshPolygons)
                {
                    chunkIO.ReadChunks((id3, chunkSize3) =>
                    {
                        if (id3 == Wad2Chunks.MeshQuad  || id3 == Wad2Chunks.MeshTriangle ||
                            id3 == Wad2Chunks.MeshQuad2 || id3 == Wad2Chunks.MeshTriangle2)
                        {
                            bool isQuad = (id3 == Wad2Chunks.MeshQuad || id3 == Wad2Chunks.MeshQuad2);

                            var polygon = new WadPolygon();
                            polygon.Shape = isQuad ? WadPolygonShape.Quad : WadPolygonShape.Triangle;
                            polygon.Index0 = LEB128.ReadInt(chunkIO.Raw);
                            polygon.Index1 = LEB128.ReadInt(chunkIO.Raw);
                            polygon.Index2 = LEB128.ReadInt(chunkIO.Raw);
                            if (isQuad)
                                polygon.Index3 = LEB128.ReadInt(chunkIO.Raw);
                            polygon.ShineStrength = LEB128.ReadByte(chunkIO.Raw);

                            TextureArea textureArea = new TextureArea();
                            textureArea.Texture = textures[LEB128.ReadInt(chunkIO.Raw)];

                            if (id3 == Wad2Chunks.MeshQuad2 || id3 == Wad2Chunks.MeshTriangle2)
                            {
                                textureArea.ParentArea.Start = chunkIO.Raw.ReadVector2();
                                textureArea.ParentArea.End = chunkIO.Raw.ReadVector2();
                            }

                            textureArea.TexCoord0 = chunkIO.Raw.ReadVector2();
                            textureArea.TexCoord1 = chunkIO.Raw.ReadVector2();
                            textureArea.TexCoord2 = chunkIO.Raw.ReadVector2();
                            if (isQuad)
                                textureArea.TexCoord3 = chunkIO.Raw.ReadVector2();
                            else
                                textureArea.TexCoord3 = textureArea.TexCoord2;
                            textureArea.BlendMode = (BlendMode)LEB128.ReadLong(chunkIO.Raw);
                            textureArea.DoubleSided = chunkIO.Raw.ReadBoolean();
                            polygon.Texture = textureArea;

                            chunkIO.ReadChunks((id4, chunkSize4) =>
                            {
                                return false;
                            });

                            mesh.Polys.Add(polygon);
                        }
                        else
                        {
                            return false;
                        }
                        return true;
                    });
                }
                else
                {
                    return false;
                }

                return true;
            });

            return mesh;
        }

        private static bool LoadSprites(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, ref Dictionary<long, WadSprite> outSprites)
        {
            if (idOuter != Wad2Chunks.Sprites)
                return false;

            var sprites = new Dictionary<long, WadSprite>();
            long obsoleteIndex = 0; // Move this into each chunk once we got rid of old style *.wad2 files.
            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Sprite)
                    return false;

                int width = LEB128.ReadInt(chunkIO.Raw);
                int height = LEB128.ReadInt(chunkIO.Raw);
                byte[] imageData = null;
                RectangleInt2 rect = new RectangleInt2();

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.SpriteIndex)
                        obsoleteIndex = chunkIO.ReadChunkLong(chunkSize2);
                    else if (id2 == Wad2Chunks.SpriteData)
                        imageData = chunkIO.ReadChunkArrayOfBytes(chunkSize2);
                    else if (id2 == Wad2Chunks.SpriteSides) {
                        rect.X0 = chunkIO.Raw.ReadInt32();
                        rect.Y0 = chunkIO.Raw.ReadInt32();
                        rect.X1 = chunkIO.Raw.ReadInt32();
                        rect.Y1 = chunkIO.Raw.ReadInt32();
                    }else return false;
                    return true;
                });

                sprites.Add(obsoleteIndex++, new WadSprite {
                    Texture = new WadTexture(ImageC.FromByteArray(imageData, width, height)),
                    Alignment = rect
                })
                ;
                return true;
            });

            outSprites = sprites;
            return true;
        }

        private static bool LoadSpriteSequences(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, Dictionary<long, WadSprite> sprites)
        {
            if (idOuter != Wad2Chunks.SpriteSequences)
                return false;

            var spriteSequences = new SortedList<WadSpriteSequenceId, WadSpriteSequence>();
            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.SpriteSequence)
                    return false;

                var s = new WadSpriteSequence(new WadSpriteSequenceId(LEB128.ReadUInt(chunkIO.Raw)));
                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.SpriteSequenceSpriteIndex)
                        s.Sprites.Add(sprites[chunkIO.ReadChunkInt(chunkSize2)]);
                    else
                        return false;
                    return true;
                });

                spriteSequences.Add(s.Id, s);
                return true;
            });

            wad.SpriteSequences = spriteSequences;
            return true;
        }

        private static WadBone LoadBone(ChunkReader chunkIO, WadMoveable mov, List<WadMesh> meshes)
        {
            WadBone bone = new WadBone();

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id == Wad2Chunks.MoveableBoneName)
                {
                    bone.Name = chunkIO.ReadChunkString(chunkSize);
                    //Console.WriteLine("Processing " + bone.Name);
                }
                else if (id == Wad2Chunks.MoveableBoneTranslation)
                    bone.Translation = chunkIO.ReadChunkVector3(chunkSize);
                else if (id == Wad2Chunks.MoveableBoneMeshPointer)
                    bone.Mesh = meshes[chunkIO.ReadChunkInt(chunkSize)];
                else if (id == Wad2Chunks.MoveableBone)
                    bone.Children.Add(LoadBone(chunkIO, mov, meshes));
                else
                    return false;
                return true;
            });

            foreach (var childBone in bone.Children)
                childBone.Parent = bone;

            return bone;
        }

        private static void BuildNewMeshTree(WadBone bone, List<WadBone> meshTrees)
        {
            var newBone = new WadBone();
            newBone.Translation = bone.Translation;
            newBone.Mesh = bone.Mesh;
            newBone.Name = bone.Name;

            if (bone.Parent == null)
                newBone.OpCode = WadLinkOpcode.Push;
            else
            {
                if (bone.Parent.Children.Count == 1)
                    newBone.OpCode = WadLinkOpcode.NotUseStack;
                else
                {
                    int childrenCount = bone.Parent.Children.Count;
                    if (bone.Parent.Children.IndexOf(bone) == 0)
                        newBone.OpCode = WadLinkOpcode.Push;
                    else if (bone.Parent.Children.IndexOf(bone) == childrenCount - 1)
                        newBone.OpCode = WadLinkOpcode.Pop;
                    else
                        newBone.OpCode = WadLinkOpcode.Read;
                }
            }

            if (bone.Parent != null)
                meshTrees.Add(newBone);

            for (int i = 0; i < bone.Children.Count; i++)
                BuildNewMeshTree(bone.Children[i], meshTrees);
        }

        private static bool LoadMoveables(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad,
                                          Dictionary<long, WadTexture> textures)
        {
            if (idOuter != Wad2Chunks.Moveables)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Moveable)
                    return false;

                uint objTypeId = LEB128.ReadUInt(chunkIO.Raw);
                var mov = new WadMoveable(new WadMoveableId(objTypeId));
                var meshes = new List<WadMesh>();

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.Mesh)
                    {
                        var mesh = LoadMesh(chunkIO, chunkSize2, textures);
                        meshes.Add(mesh);
                    }
                    else if (id2 == Wad2Chunks.MoveableSkin)
                    {
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.Mesh)
                            {
                                var mesh = LoadMesh(chunkIO, chunkSize3, textures);
                                mov.Skin = mesh;
                                return true;
                            }
                            else
                                return false;
                        });
                    }
                    else if (id2 == Wad2Chunks.MoveableBone)
                    {
                        var skeleton = LoadBone(chunkIO, mov, meshes);

                        // Now convert the skeleton in the new (?) format. Ugly system but this is required for 
                        // setting exact hardcoded ID for some moveables (i.e. gun mesh of an enemy, the engine 
                        // has hardcoded mesh indices for effects)
                        var bones = new List<WadBone>();

                        var root = new WadBone();
                        root.Name = skeleton.Name;
                        root.Translation = Vector3.Zero;
                        root.Mesh = skeleton.Mesh;
                        bones.Add(root);

                        BuildNewMeshTree(skeleton, bones);
                        mov.Bones.AddRange(bones);
                    }
                    else if (id2 == Wad2Chunks.MoveableBoneNew)
                    {
                        var bone = new WadBone();

                        bone.OpCode = (WadLinkOpcode)LEB128.ReadByte(chunkIO.Raw);
                        bone.Name = chunkIO.Raw.ReadStringUTF8();

                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {                            
                            if (id3 == Wad2Chunks.MoveableBoneTranslation)
                                bone.Translation = chunkIO.ReadChunkVector3(chunkSize);
                            else if (id3 == Wad2Chunks.MoveableBoneMeshPointer)
                                bone.Mesh = meshes[chunkIO.ReadChunkInt(chunkSize)];
                            else
                                return false;
                            return true;
                        });

                        mov.Bones.Add(bone);
                    }
                    else if (id2 == Wad2Chunks.AnimationObsolete || 
                             id2 == Wad2Chunks.Animation ||
                             id2 == Wad2Chunks.Animation2 ||
                             id2 == Wad2Chunks.Animation3)
                    {
                        var animation = new WadAnimation();

                        animation.StateId = LEB128.ReadUShort(chunkIO.Raw);
                        animation.EndFrame = LEB128.ReadUShort(chunkIO.Raw);
                        animation.FrameRate = LEB128.ReadByte(chunkIO.Raw);

                        if (id2 == Wad2Chunks.AnimationObsolete)
                        {
                            LEB128.ReadUShort(chunkIO.Raw);
                            LEB128.ReadUShort(chunkIO.Raw);
                        }

                        int oldSpeed, oldAccel, oldLatSpeed, oldLatAccel;
                        oldSpeed = oldAccel = oldLatSpeed = oldLatAccel = 0;

                        if (id2 != Wad2Chunks.Animation2 && id2 != Wad2Chunks.Animation3)
                        {
                            // Use old speeds/accels for legacy chunk versions
                            oldSpeed    = LEB128.ReadInt(chunkIO.Raw);
                            oldAccel    = LEB128.ReadInt(chunkIO.Raw);
                            oldLatSpeed = LEB128.ReadInt(chunkIO.Raw);
                            oldLatAccel = LEB128.ReadInt(chunkIO.Raw);

                            // Correct EndFrame for legacy chunk versions
                            if (animation.EndFrame > 0)
                                animation.EndFrame--;
                        }

                        // Fix possibly corrupted EndFrame value which was caused by bug introduced in 1.2.9
                        if (animation.EndFrame == ushort.MaxValue)
                            animation.EndFrame = 0;

                        animation.NextAnimation = LEB128.ReadUShort(chunkIO.Raw);
                        animation.NextFrame = LEB128.ReadUShort(chunkIO.Raw);

                        if (id2 == Wad2Chunks.Animation3)
                            animation.BlendFrameCount = LEB128.ReadUShort(chunkIO.Raw);

                        bool foundNewVelocitiesChunk = false;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.CurveStart)
                            {
                                animation.BlendCurve.Start = chunkIO.ReadChunkVector2(chunkSize3);
                            }
                            else if (id3 == Wad2Chunks.CurveEnd)
                            {
                                animation.BlendCurve.End = chunkIO.ReadChunkVector2(chunkSize3);
                            }
                            else if (id3 == Wad2Chunks.CurveStartHandle)
                            {
                                animation.BlendCurve.StartHandle = chunkIO.ReadChunkVector2(chunkSize3);
                            }
                            else if (id3 == Wad2Chunks.CurveEndHandle)
                            {
                                animation.BlendCurve.EndHandle = chunkIO.ReadChunkVector2(chunkSize3);
                            }
                            else if (id3 == Wad2Chunks.AnimationName)
                            {
                                animation.Name = chunkIO.ReadChunkString(chunkSize3);
                            }
                            else if (id3 == Wad2Chunks.AnimationVelocities)
                            {
                                foundNewVelocitiesChunk = true;
                                var velocities = chunkIO.ReadChunkVector4(chunkSize);
                                animation.StartVelocity = velocities.X;
                                animation.EndVelocity = velocities.Y;
                                animation.StartLateralVelocity = velocities.Z;
                                animation.EndLateralVelocity = velocities.W;
                            }
                            else if (id3 == Wad2Chunks.KeyFrame)
                            {
                                var keyframe = new WadKeyFrame();
                                chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 == Wad2Chunks.KeyFrameOffset)
                                    {
                                        keyframe.Offset = chunkIO.ReadChunkVector3(chunkSize4);
                                    }
                                    else if (id4 == Wad2Chunks.KeyFrameBoundingBox)
                                    {
                                        var kfMin = Vector3.Zero;
                                        var kfMax = Vector3.Zero;
                                        chunkIO.ReadChunks((id5, chunkSize5) =>
                                        {
                                            if (id5 == Wad2Chunks.MeshBoundingBoxMin)
                                                kfMin = chunkIO.ReadChunkVector3(chunkSize5);
                                            else if (id5 == Wad2Chunks.MeshBoundingBoxMax)
                                                kfMax = chunkIO.ReadChunkVector3(chunkSize5);
                                            else
                                                return false;
                                            return true;
                                        });
                                        keyframe.BoundingBox = new BoundingBox(kfMin, kfMax);
                                    }
                                    else if (id4 == Wad2Chunks.KeyFrameAngle)
                                    {
                                        var angle = new WadKeyFrameRotation();
                                        angle.Rotations = chunkIO.ReadChunkVector3(chunkSize4);
                                        keyframe.Angles.Add(angle);
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    return true;
                                });
                                animation.KeyFrames.Add(keyframe);
                            }
                            else if (id3 == Wad2Chunks.StateChange)
                            {
                                var stateChange = new WadStateChange();
                                stateChange.StateId = LEB128.ReadUShort(chunkIO.Raw);
                                chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 == Wad2Chunks.Dispatch || id4 == Wad2Chunks.Dispatch2)
                                    {
                                        var dispatch = new WadAnimDispatch();
                                        dispatch.InFrame = LEB128.ReadUShort(chunkIO.Raw);
                                        dispatch.OutFrame = LEB128.ReadUShort(chunkIO.Raw);
                                        dispatch.NextAnimation = LEB128.ReadUShort(chunkIO.Raw);
                                        dispatch.NextFrameLow = LEB128.ReadUShort(chunkIO.Raw);

                                        if (id4 == Wad2Chunks.Dispatch2)
                                        {
                                            dispatch.NextFrameHigh = LEB128.ReadUShort(chunkIO.Raw);
                                            dispatch.BlendFrameCount = LEB128.ReadUShort(chunkIO.Raw);

                                            chunkIO.ReadChunks((id5, chunkSize5) =>
                                            {
                                                if (id5 == Wad2Chunks.CurveStart)
                                                    dispatch.BlendCurve.Start = chunkIO.ReadChunkVector2(chunkSize5);
                                                else if (id5 == Wad2Chunks.CurveEnd)
                                                    dispatch.BlendCurve.End = chunkIO.ReadChunkVector2(chunkSize5);
                                                else if (id5 == Wad2Chunks.CurveStartHandle)
                                                    dispatch.BlendCurve.StartHandle = chunkIO.ReadChunkVector2(chunkSize5);
                                                else if (id5 == Wad2Chunks.CurveEndHandle)
                                                    dispatch.BlendCurve.EndHandle = chunkIO.ReadChunkVector2(chunkSize5);
                                                else
                                                    return false;
                                                return true;
                                            });
                                        }

                                        stateChange.Dispatches.Add(dispatch);
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    return true;
                                });
                                animation.StateChanges.Add(stateChange);
                            }
                            else if (id3 == Wad2Chunks.AnimCommand ||
                                     id3 == Wad2Chunks.AnimCommand2)
                            {
                                var command = new WadAnimCommand();
                                long offset = chunkIO.Raw.BaseStream.Position;
                                command.Type = (WadAnimCommandType)LEB128.ReadUShort(chunkIO.Raw);
                                command.Parameter1 = LEB128.ReadShort(chunkIO.Raw);
                                command.Parameter2 = LEB128.ReadShort(chunkIO.Raw);
                                command.Parameter3 = LEB128.ReadShort(chunkIO.Raw);

                                // Convert legacy sound and flipeffect condition bitpacking to separate field to free up space
                                // and account for TEN's new sound condition types.
                                if (id3 == Wad2Chunks.AnimCommand &&
                                    (command.Type == WadAnimCommandType.FlipEffect || command.Type == WadAnimCommandType.PlaySound))
                                {
                                    command.ConvertLegacyConditions();
                                }

                                chunkIO.ReadChunks((id4, chunkSize4) =>
                                {
                                    if (id4 == Wad2Chunks.AnimCommandSoundInfo) // Deprecated
                                    {
                                        chunkIO.ReadChunkInt(chunkSize4);
                                        return true;
                                    }
                                    else
                                        return false;
                                });

                                animation.AnimCommands.Add(command);
                            }
                            else
                            {
                                return false;
                            }
                            return true;
                        });

                        // Legacy code for calculating start and end velocities
                        if (!foundNewVelocitiesChunk)
                        {
                            float acceleration = oldAccel / 65536.0f;
                            animation.StartVelocity = oldSpeed / 65536.0f;
                            animation.EndVelocity = animation.StartVelocity + acceleration *
                                                        (animation.KeyFrames.Count - 1) * animation.FrameRate;

                            float lateralAcceleration = oldLatAccel / 65536.0f;
                            animation.StartLateralVelocity = oldLatSpeed / 65536.0f;
                            animation.EndLateralVelocity = animation.StartLateralVelocity + lateralAcceleration *
                                                                (animation.KeyFrames.Count - 1) * animation.FrameRate;
                        }

                        mov.Animations.Add(animation);
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                });

                wad.Moveables.Add(mov.Id, mov);

                return true;
            });

            return true;
        }

        private static bool LoadStatics(ChunkReader chunkIO, ChunkId idOuter, Wad2 wad, /*Dictionary<long, WadMesh> meshes*/
                                        Dictionary<long, WadTexture> textures)
        {
            if (idOuter != Wad2Chunks.Statics)
                return false;

            chunkIO.ReadChunks((id, chunkSize) =>
            {
                if (id != Wad2Chunks.Static && id != Wad2Chunks.Static2)
                    return false;

                var s = new WadStatic(new WadStaticId(LEB128.ReadUInt(chunkIO.Raw)));
                s.Flags = LEB128.ReadShort(chunkIO.Raw);

                // HACK: historically (pre-1.3.12) lighting type
                short legacyLightingType = -1;
                if (id == Wad2Chunks.Static)
                    legacyLightingType = LEB128.ReadShort(chunkIO.Raw);

                // HACK: shatter options (pre-1.7.3)
                bool shatterSoundSet = false;

                chunkIO.ReadChunks((id2, chunkSize2) =>
                {
                    if (id2 == Wad2Chunks.StaticVisibilityBox)
                    {
                        var min = Vector3.Zero;
                        var max = Vector3.Zero;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.MeshBoundingBoxMin)
                                min = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.MeshBoundingBoxMax)
                                max = chunkIO.ReadChunkVector3(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        s.VisibilityBox = new BoundingBox(min, max);
                    }
                    else if (id2 == Wad2Chunks.StaticCollisionBox)
                    {
                        var min = Vector3.Zero;
                        var max = Vector3.Zero;
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.MeshBoundingBoxMin)
                                min = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.MeshBoundingBoxMax)
                                max = chunkIO.ReadChunkVector3(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        s.CollisionBox = new BoundingBox(min, max);
                    }
                    else if (id2 == Wad2Chunks.Mesh)
                        s.Mesh = LoadMesh(chunkIO, chunkSize2, textures);
                    else if (id2 == Wad2Chunks.StaticAmbientLight)
                        s.AmbientLight = chunkIO.ReadChunkShort(chunkSize2);
                    else if (id2 == Wad2Chunks.StaticShatterSound)
                        s.ShatterSoundID = chunkIO.ReadChunkInt(chunkSize2);
                    else if (id2 == Wad2Chunks.StaticShatter)
                    {
                        s.Shatter = chunkIO.ReadChunkBool(chunkSize2);
                        shatterSoundSet = true;
                    }
                    else if (id2 == Wad2Chunks.StaticLight)
                    {
                        var light = new WadLight();
                        chunkIO.ReadChunks((id3, chunkSize3) =>
                        {
                            if (id3 == Wad2Chunks.StaticLightPosition)
                                light.Position = chunkIO.ReadChunkVector3(chunkSize3);
                            else if (id3 == Wad2Chunks.StaticLightRadius)
                                light.Radius = chunkIO.ReadChunkFloat(chunkSize3);
                            else if (id3 == Wad2Chunks.StaticLightIntensity)
                                light.Intensity = chunkIO.ReadChunkFloat(chunkSize3);
                            else
                                return false;
                            return true;
                        });
                        s.Lights.Add(light);
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                });

                // HACK: Restore legacy pre-1.3.12 lighting type if needed
                if (legacyLightingType != -1)
                    s.Mesh.LightingType = (WadMeshLightingType)legacyLightingType;

                // HACK: Set default shatter parameters, if they were not set
                if (!shatterSoundSet)
                    s.Shatter = TrCatalog.IsStaticShatterable(wad.GameVersion, s.Id.TypeId);

                wad.Statics.Add(s.Id, s);
                return true;
            });

            return true;
        }
    }
}
