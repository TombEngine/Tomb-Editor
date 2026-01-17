using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad.Tr4Wad
{
    internal static class Tr4WadOperations
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        public static Wad2 ConvertTr4Wad(Tr4Wad oldWad, IDialogHandler progressReporter)
        {
            logger.Info("Converting TR4 WAD to Wad2");

            var wad = new Wad2() { GameVersion = TRVersion.Game.TR4 };

            // Convert all textures
            Dictionary<int, TextureArea> textures = ConvertTr4TexturesToWadTexture(oldWad, wad);
            logger.Info("Textures read.");

            // Convert sounds
            var sfxPath = Path.GetDirectoryName(oldWad.FileName) + "\\" + Path.GetFileNameWithoutExtension(oldWad.FileName) + ".sfx";
            if (File.Exists(sfxPath))
            {
                wad.Sounds = WadSounds.ReadFromFile(sfxPath);
                logger.Info("Sounds read.");
            }

            // Convert moveables
            for (int i = 0; i < oldWad.Moveables.Count; i++)
                ConvertTr4MoveableToWadMoveable(wad, oldWad, i, textures);
            logger.Info("Moveables read.");

            // Convert statics
            for (int i = 0; i < oldWad.Statics.Count; i++)
                ConvertTr4StaticMeshToWadStatic(wad, oldWad, i, textures);
            logger.Info("Statics read.");

            // Convert sprites
            ConvertTr4Sprites(wad, oldWad);
            logger.Info("Sprites read.");

            return wad;
        }

        private static Dictionary<int, TextureArea> ConvertTr4TexturesToWadTexture(Tr4Wad oldWad, Wad2 wad)
        {
            var texInfos = new ConcurrentDictionary<int, TextureArea>();
            var textures = new List<WadTexture>();

            // Generate pages

            for (int i = 0; i < oldWad.NumTexturePages; i++)
            {
                var image = ImageC.CreateNew(256, 256);

                for (var y = 0; y < 256; y++)
                {
                    for (var x = 0; x < 256; x++)
                    {
                        var baseIndex = (i * 256 + y) * 768 + x * 3;
                        var r = oldWad.TexturePages[baseIndex];
                        var g = oldWad.TexturePages[baseIndex + 1];
                        var b = oldWad.TexturePages[baseIndex + 2];
                        var a = (byte)255;

                        image.SetPixel(x, y, r, g, b, a);
                    }
                }

                // Set filename
                image.FileName = Path.GetFileNameWithoutExtension(oldWad.FileName) + "_Page_" + i;

                // Replace magenta color with alpha transparent black
                image.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));

                var newPage = new WadTexture(image);
                textures.Add(newPage);
            }

            // Generate dummy texture areas

            Parallel.For(0, oldWad.Textures.Count, i =>
            {
                var oldTexture = oldWad.Textures[i];
                var start = new Vector2(oldTexture.X, oldTexture.Y);

                var area = new TextureArea()
                {
                    TexCoord0 = start + new Vector2(0, 0),
                    TexCoord1 = start + new Vector2(oldTexture.Width + 1.0f, 0),
                    TexCoord2 = start + new Vector2(oldTexture.Width + 1.0f, oldTexture.Height + 1.0f),
                    TexCoord3 = start + new Vector2(0, oldTexture.Height + 1.0f),
                    Texture = textures[oldTexture.Page]
                };

                texInfos.TryAdd(i, area);
              });

            return new Dictionary<int, TextureArea>(texInfos);
        }

        private static int GetTr4TextureIdFromPolygon(wad_polygon polygon)
        {
            short textureId = (short)(polygon.Texture);
            if (polygon.Shape == 8)
            {
                textureId = (short)(polygon.Texture & 0xFFF);
                if ((polygon.Texture & 0x8000) != 0)
                    textureId = (short)(-textureId);
            }
            else
            {
                // HACK: for taking in account sign
                if (textureId > 0x7FFF)
                {
                    textureId = (short)(textureId - 0xFFFF);
                }
            }

            textureId = Math.Abs(textureId);

            return textureId;
        }

        private static WadMesh ConvertTr4MeshToWadMesh(Wad2 wad, Tr4Wad oldWad, Dictionary<int, TextureArea> textures,
                                                       wad_mesh oldMesh, int objectID)
        {
            WadMesh mesh = new WadMesh();
            var meshIndex = oldWad.Meshes.IndexOf(oldMesh);
            mesh.Name = "Mesh-" + objectID + "-" + meshIndex;

            // Create the bounding sphere
            mesh.BoundingSphere = new BoundingSphere(new Vector3(oldMesh.SphereX, -oldMesh.SphereY, oldMesh.SphereZ), Math.Abs(oldMesh.Radius));

            // Add positions
            foreach (var oldVertex in oldMesh.Vertices)
                mesh.VertexPositions.Add(new Vector3(oldVertex.X, -oldVertex.Y, oldVertex.Z));

            // Add normals
            foreach (var oldNormal in oldMesh.Normals)
                mesh.VertexNormals.Add(new Vector3(oldNormal.X, -oldNormal.Y, oldNormal.Z));

            // Add shades
            foreach (var oldShade in oldMesh.Shades)
                mesh.VertexColors.Add(new Vector3((8191.0f - oldShade) / 8191.0f));

            // Add polygons
            foreach (var oldPoly in oldMesh.Polygons)
            {
                WadPolygon poly = new WadPolygon();
                poly.Shape = oldPoly.Shape == 8 ? WadPolygonShape.Triangle : WadPolygonShape.Quad;

                // Polygon indices
                poly.Index0 = oldPoly.V1;
                poly.Index1 = oldPoly.V2;
                poly.Index2 = oldPoly.V3;
                if (poly.Shape == WadPolygonShape.Quad)
                    poly.Index3 = oldPoly.V4;
                
                // Polygon special effects
                poly.ShineStrength = (byte)((oldPoly.Attributes & 0x7c) >> 2);

                // Add the texture
                poly.Texture = CalculateTr4UVCoordinates(wad, oldWad, oldPoly, textures);

                mesh.Polys.Add(poly);
            }

            mesh.BoundingBox = new BoundingBox(oldMesh.Minimum, oldMesh.Maximum);

            // In original wad/tr formats, negative normals count means that light type is static
            if (oldMesh.NumNormals <= 0)
                mesh.LightingType = WadMeshLightingType.VertexColors;
            else
                mesh.LightingType = WadMeshLightingType.Normals;

            // Usually only for static meshes
            if (mesh.VertexNormals.Count == 0)
                mesh.CalculateNormals();

            return mesh;
        }

        internal static void ConvertTr4Sprites(Wad2 wad, Tr4Wad oldWad)
        {
            if (oldWad.SpriteData == null || oldWad.SpriteSequences.Count == 0)
                return;

            int spriteDataSize = oldWad.SpriteData.Length;

            // Load the real sprite texture data
            int numSpriteTexturePages = spriteDataSize / 196608;
            if ((spriteDataSize % 196608) != 0)
                numSpriteTexturePages++;

            foreach (var oldSequence in oldWad.SpriteSequences)
            {
                int lengthOfSequence = -oldSequence.NegativeLength;
                int startIndex = oldSequence.Offset;

                var newSequence = new WadSpriteSequence(new WadSpriteSequenceId((uint)oldSequence.ObjectID));

                for (int i = startIndex; i < startIndex + lengthOfSequence; i++)
                {
                    var oldSpriteTexture = oldWad.SpriteTextures[i];

                    int spriteWidth = oldSpriteTexture.Width + 1;
                    int spriteHeight = oldSpriteTexture.Height + 1;
                    int spriteX = oldSpriteTexture.X;
                    int spriteY = oldSpriteTexture.Y;
                    var spriteImage = ImageC.CreateNew(spriteWidth, spriteHeight);

                    for (int y = 0; y < spriteHeight; y++)
                        for (int x = 0; x < spriteWidth; x++)
                        {
                            int baseIndex = oldSpriteTexture.Tile * 196608 + 768 * (y + spriteY) + 3 * (x + spriteX);

                            byte b = oldWad.SpriteData[baseIndex + 0];
                            byte g = oldWad.SpriteData[baseIndex + 1];
                            byte r = oldWad.SpriteData[baseIndex + 2];

                            if (r == 255 && g == 0 && b == 255)
                                spriteImage.SetPixel(x, y, 0, 0, 0, 0);
                            else
                                spriteImage.SetPixel(x, y, b, g, r, 255);
                        }

                    // Make new sprite and recalculate alignment, if needed
                    var sprite = new WadSprite { Texture = new WadTexture(spriteImage) };
                    if (sprite.Alignment == RectangleInt2.Zero)
                        sprite.RecalculateAlignment();

                    // Add current sprite to the sequence
                    newSequence.Sprites.Add(sprite);
                }

                wad.SpriteSequences.Add(newSequence.Id, newSequence);
            }
        }

        internal static WadMoveable ConvertTr4MoveableToWadMoveable(Wad2 wad, Tr4Wad oldWad, int moveableIndex,
                                                                    Dictionary<int, TextureArea> textures)
        {
            wad_moveable oldMoveable = oldWad.Moveables[moveableIndex];
            var newId = new WadMoveableId(oldMoveable.ObjectID);

            // A workaround to find out duplicated item IDs produced by StrPix unpatched for v130 wads.
            // In such case, a legacy name is used to guess real item ID, if this fails - broken item is filtered out.
            if (wad.Moveables.ContainsKey(newId))
            {
                var message = "Duplicated moveable ID " + oldMoveable.ObjectID + " was identified while loading " + oldWad.BaseName + ". ";
                if (oldWad.LegacyNames.Count - oldWad.Statics.Count < moveableIndex)
                {
                    logger.Warn(message + "Can't restore real ID by name, name table is too short. Ignoring moveable.");
                    return null;
                }

                bool isMoveable;
                var guessedId = TrCatalog.GetItemIndex(TRVersion.Game.TR4, oldWad.LegacyNames[moveableIndex], out isMoveable);

                if (isMoveable && guessedId.HasValue)
                {
                    newId = new WadMoveableId(guessedId.Value);
                    if (wad.Moveables.ContainsKey(newId))
                    {
                        logger.Warn(message + "Can't restore real ID by name, name table is inconsistent. Ignoring moveable.");
                        return null;
                    }
                    else
                        logger.Warn(message + "Successfully restored real ID by name.");
                }
                else
                {
                    logger.Warn(message + "Can't find provided name in catalog. Ignoring moveable.");
                    return null;
                }
            }

            WadMoveable newMoveable = new WadMoveable(newId);
            var frameBases = new Dictionary<WadAnimation, ushort[]>();

            // Load meshes
            var meshes = new List<WadMesh>();
            for (int j = 0; j < oldMoveable.NumPointers; j++)
            {
                meshes.Add(ConvertTr4MeshToWadMesh(wad, oldWad, textures,
                                                               oldWad.Meshes[(int)oldWad.RealPointers[oldMoveable.PointerIndex + j]],
                                                               (int)oldMoveable.ObjectID));
            }

            // Build the skeleton
            var root = new WadBone();
            root.Name = "bone_0_root";
            root.Parent = null;
            root.Translation = Vector3.Zero;
            root.Mesh = meshes[0];

            newMoveable.Bones.Add(root);

            for (int j = 0; j < oldMoveable.NumPointers - 1; j++)
            {
                WadBone bone = new WadBone();
                bone.Name = "bone_" + (j + 1).ToString();
                bone.Parent = null;
                bone.Translation = Vector3.Zero;
                bone.Mesh = meshes[j + 1];
                newMoveable.Bones.Add(bone);
            }

            for (int mi = 0; mi < (oldMoveable.NumPointers - 1); mi++)
            {
                int j = mi + 1;

                var opcode = (WadLinkOpcode)oldWad.Links[(int)(oldMoveable.LinksIndex + mi * 4)];
                int linkX = oldWad.Links[(int)(oldMoveable.LinksIndex + mi * 4) + 1];
                int linkY = -oldWad.Links[(int)(oldMoveable.LinksIndex + mi * 4) + 2];
                int linkZ = oldWad.Links[(int)(oldMoveable.LinksIndex + mi * 4) + 3];

                newMoveable.Bones[j].OpCode = opcode;
                newMoveable.Bones[j].Translation = new Vector3(linkX, linkY, linkZ);
            }

            // Convert animations
            int numAnimations = 0;
            int nextMoveable = oldWad.GetNextMoveableWithAnimations(moveableIndex);

            if (nextMoveable == -1)
                numAnimations = oldWad.Animations.Count - oldMoveable.AnimationIndex;
            else
                numAnimations = oldWad.Moveables[nextMoveable].AnimationIndex - oldMoveable.AnimationIndex;

            for (int j = 0; j < numAnimations; j++)
            {
                if (oldMoveable.AnimationIndex == -1)
                    break;

                wad_animation oldAnimation = oldWad.Animations[j + oldMoveable.AnimationIndex];
                WadAnimation newAnimation = new WadAnimation();
                newAnimation.StateId = oldAnimation.StateId;
                newAnimation.FrameRate = oldAnimation.FrameDuration;
                newAnimation.NextAnimation = (ushort)(oldAnimation.NextAnimation - oldMoveable.AnimationIndex);
                newAnimation.NextFrame = oldAnimation.NextFrame;
                newAnimation.Name = TrCatalog.GetAnimationName(TRVersion.Game.TR4, oldMoveable.ObjectID, (uint)j);

                // Fix wadmerger/wad format bug with inverted frame start/end on single-frame anims
                ushort newFrameStart = oldAnimation.FrameStart < oldAnimation.FrameEnd ? oldAnimation.FrameStart : oldAnimation.FrameEnd;
                ushort newFrameEnd   = oldAnimation.FrameStart < oldAnimation.FrameEnd ? oldAnimation.FrameEnd : newFrameStart;
                newAnimation.EndFrame = (ushort)(newFrameEnd - newFrameStart);

                for (int k = 0; k < oldAnimation.NumStateChanges; k++)
                {
                    WadStateChange sc = new WadStateChange();
                    wad_state_change wadSc = oldWad.Changes[(int)oldAnimation.ChangesIndex + k];
                    sc.StateId = (ushort)wadSc.StateId;

                    for (int n = 0; n < wadSc.NumDispatches; n++)
                    {
                        WadAnimDispatch ad = new WadAnimDispatch();
                        wad_anim_dispatch wadAd = oldWad.Dispatches[(int)wadSc.DispatchesIndex + n];

                        ad.InFrame = (ushort)(wadAd.Low - newFrameStart);
                        ad.OutFrame = (ushort)(wadAd.High - newFrameStart);
                        ad.NextAnimation = (ushort)((wadAd.NextAnimation - oldMoveable.AnimationIndex) % numAnimations);
                        ad.NextFrameLow = (ushort)wadAd.NextFrame;

                        sc.Dispatches.Add(ad);
                    }

                    newAnimation.StateChanges.Add(sc);
                }

                if (oldAnimation.NumCommands < oldWad.Commands.Count)
                {
                    int lastCommand = oldAnimation.CommandOffset;

                    for (int k = 0; k < oldAnimation.NumCommands; k++)
                    {
                        short commandType = oldWad.Commands[lastCommand];

                        WadAnimCommand command = new WadAnimCommand();
                        command.Type = (WadAnimCommandType)commandType;
                        switch (command.Type)
                        {
                            case WadAnimCommandType.SetPosition:
                                command.Parameter1 = (short)oldWad.Commands[lastCommand + 1];
                                command.Parameter2 = (short)oldWad.Commands[lastCommand + 2];
                                command.Parameter3 = (short)oldWad.Commands[lastCommand + 3];
                                lastCommand += 4;
                                break;

                            case WadAnimCommandType.SetJumpDistance:
                                command.Parameter1 = (short)oldWad.Commands[lastCommand + 1];
                                command.Parameter2 = (short)oldWad.Commands[lastCommand + 2];
                                lastCommand += 3;
                                break;

                            case WadAnimCommandType.EmptyHands:
                            case WadAnimCommandType.KillEntity:
                                lastCommand += 1;
                                break;

                            case WadAnimCommandType.PlaySound:
                            case WadAnimCommandType.FlipEffect:
                                command.Parameter1 = (short)(oldWad.Commands[lastCommand + 1] - newFrameStart);
                                command.Parameter2 = (short)oldWad.Commands[lastCommand + 2];

                                // For single-frame anims, clamp frame number to first frame (another fix for WM/wad format range inversion bug)
                                if (newAnimation.EndFrame == 0 && command.Parameter1 > 0)
                                    command.Parameter1 = 0;

                                // Convert animcommand conditions to a separate field.
                                command.ConvertLegacyConditions();

                                lastCommand += 3;
                                break;

                            default: // Ignore invalid anim commands (see for example karnak.wad)
                                logger.Warn("Invalid anim command " + commandType);
                                goto ExitForLoop;
                        }

                        newAnimation.AnimCommands.Add(command);
                    }
                    ExitForLoop:
                    ;
                }

                int frames = (int)oldAnimation.KeyFrameOffset / 2;
                uint numFrames = 0;
                if (oldAnimation.KeyFrameSize != 0)
                    if ((j + oldMoveable.AnimationIndex) == (oldWad.Animations.Count - 1))
                        numFrames = ((uint)(2 * oldWad.KeyFrames.Count) - oldAnimation.KeyFrameOffset) / (uint)(2 * oldAnimation.KeyFrameSize);
                    else
                        numFrames = (oldWad.Animations[oldMoveable.AnimationIndex + j + 1].KeyFrameOffset - oldAnimation.KeyFrameOffset) / (uint)(2 * oldAnimation.KeyFrameSize);

                for (int f = 0; f < numFrames; f++)
                {
                    WadKeyFrame frame = new WadKeyFrame();
                    int startOfFrame = frames;

                    frame.BoundingBox = new BoundingBox(new Vector3(oldWad.KeyFrames[frames],
                                                                    -oldWad.KeyFrames[frames + 2],
                                                                    oldWad.KeyFrames[frames + 4]),
                                                        new Vector3(oldWad.KeyFrames[frames + 1],
                                                                    -oldWad.KeyFrames[frames + 3],
                                                                    oldWad.KeyFrames[frames + 5]));
                    frames += 6;

                    frame.Offset = new Vector3(oldWad.KeyFrames[frames],
                                               (short)(-oldWad.KeyFrames[frames + 1]),
                                               oldWad.KeyFrames[frames + 2]);
                    frames += 3;

                    for (int n = 0; n < oldMoveable.NumPointers; n++)
                        frame.Angles.Add(WadKeyFrameRotation.FromTrAngle(ref frames, oldWad.KeyFrames, false, true));
                    if ((frames - startOfFrame) < oldAnimation.KeyFrameSize)
                        frames += ((int)oldAnimation.KeyFrameSize - (frames - startOfFrame));

                    newAnimation.KeyFrames.Add(frame);
                }

                // New velocities
                float acceleration = oldAnimation.Accel / 65536.0f;
                newAnimation.StartVelocity = oldAnimation.Speed / 65536.0f;

                float lateralAcceleration = oldAnimation.AccelLateral / 65536.0f;
                newAnimation.StartLateralVelocity = oldAnimation.SpeedLateral / 65536.0f;

                if (newAnimation.KeyFrames.Count != 0 && newAnimation.FrameRate != 0)
                {
                    newAnimation.EndVelocity = newAnimation.StartVelocity + acceleration *
                                                    (newAnimation.KeyFrames.Count - 1) * newAnimation.FrameRate;
                    newAnimation.EndLateralVelocity = newAnimation.StartLateralVelocity + lateralAcceleration *
                                                        (newAnimation.KeyFrames.Count - 1) * newAnimation.FrameRate;
                }
                else
                {
                    // Basic foolproofness for potentially broken animations
                    newAnimation.EndVelocity = newAnimation.StartVelocity;
                    newAnimation.EndLateralVelocity = newAnimation.StartLateralVelocity;
                }

                // Deduce real maximum frame number, based on interpolation and keyframes.
                // We need to refer this value in NextFrame-related fixes (below) because of epic WadMerger bug,
                // which incorrectly calculates NextFrame and "steals" last frame from every custom animation.
                ushort maxFrameCount = (ushort)((newAnimation.FrameRate == 1 || numFrames <= 2) ? numFrames : ((numFrames - 1) * newAnimation.FrameRate) + 1);

                // Also correct animation out-point
                if (newAnimation.EndFrame >= maxFrameCount)
                    newAnimation.EndFrame = (ushort)(maxFrameCount - 1);

                frameBases.Add(newAnimation, new ushort[] { newFrameStart, (ushort)(maxFrameCount - 1) });
                newMoveable.Animations.Add(newAnimation);
            }

            for (int i = 0; i < newMoveable.Animations.Count; i++)
            {
                var animation = newMoveable.Animations[i];

                if (animation.KeyFrames.Count == 0)
                    animation.EndFrame = 0;

                // HACK: this fixes some invalid NextAnimations values
                animation.NextAnimation %= (ushort)newMoveable.Animations.Count;

                newMoveable.Animations[i] = animation;
            }

            for (int i = 0; i < newMoveable.Animations.Count; i++)
            {
                var animation = newMoveable.Animations[i];

                // HACK: this fixes some invalid NextFrame values
                if (frameBases[newMoveable.Animations[animation.NextAnimation]][0] != 0)
                {
                    animation.NextFrame -= frameBases[newMoveable.Animations[animation.NextAnimation]][0];
                    if (animation.NextFrame > frameBases[newMoveable.Animations[animation.NextAnimation]][1])
                        animation.NextFrame = frameBases[newMoveable.Animations[animation.NextAnimation]][1];
                }

                foreach (var stateChange in animation.StateChanges)
                    for (int j = 0; j < stateChange.Dispatches.Count; ++j)
                    {
                        WadAnimDispatch animDispatch = stateChange.Dispatches[j];

                        // HACK: Probably WadMerger's bug
                        if (animDispatch.NextAnimation > short.MaxValue)
                        {
                            animDispatch.NextAnimation = 0;
                            animDispatch.NextFrameLow = 0;
                            continue;
                        }

                        if (frameBases[newMoveable.Animations[animDispatch.NextAnimation]][0] != 0)
                        {
                            // HACK: In some cases dispatches have invalid NextFrame.
                            // From tests it seems that's ok to make NextFrame equal to max frame number.
                            animDispatch.NextFrameLow -= frameBases[newMoveable.Animations[animDispatch.NextAnimation]][0];
                            if (animDispatch.NextFrameLow > frameBases[newMoveable.Animations[animDispatch.NextAnimation]][1])
                                animDispatch.NextFrameLow = frameBases[newMoveable.Animations[animDispatch.NextAnimation]][1];
                        }
                        stateChange.Dispatches[j] = animDispatch;
                    }
            }
            
            wad.Moveables.Add(newMoveable.Id, newMoveable);
            return newMoveable;
        }

        internal static WadStatic ConvertTr4StaticMeshToWadStatic(Wad2 wad, Tr4Wad oldWad, int staticIndex,
                                                                  Dictionary<int, TextureArea> textures)
        {
            var oldStaticMesh = oldWad.Statics[staticIndex];
            var newId = new WadStaticId(oldStaticMesh.ObjectId);

            // A workaround to find out duplicated item IDs produced by StrPix unpatched for v130 wads.
            // In such case, a legacy name is used to guess real item ID, if this fails - broken item is filtered out.
            if (wad.Statics.ContainsKey(newId))
            {
                var message = "Duplicated static ID " + oldStaticMesh.ObjectId + " was identified while loading " + oldWad.BaseName + ". ";
                if (oldWad.LegacyNames.Count - oldWad.Moveables.Count < staticIndex)
                {
                    logger.Warn(message + "Can't restore real ID by name, name table is too short. Ignoring static.");
                    return null;
                }

                bool isMoveable;
                var guessedId = TrCatalog.GetItemIndex(TRVersion.Game.TR4, oldWad.LegacyNames[oldWad.Moveables.Count + staticIndex], out isMoveable);

                if (!isMoveable && guessedId.HasValue)
                {
                    newId = new WadStaticId(guessedId.Value);
                    if (wad.Statics.ContainsKey(newId))
                    {
                        logger.Warn(message + "Can't restore real ID by name, name table is inconsistent. Ignoring static.");
                        return null;
                    }
                    else
                        logger.Warn(message + "Successfully restored real ID by name.");
                }
                else
                {
                    logger.Warn(message + "Can't find provided name in catalog. Ignoring static.");
                    return null;
                }
            }

            var staticMesh = new WadStatic(newId);
            //staticMesh.Name = TrCatalog.GetStaticName(WadTombRaiderVersion.TR4, oldStaticMesh.ObjectId);

            // First setup collisional and visibility bounding boxes
            staticMesh.CollisionBox = new BoundingBox(new Vector3(oldStaticMesh.CollisionX1,
                                                                  -oldStaticMesh.CollisionY1,
                                                                  oldStaticMesh.CollisionZ1),
                                                      new Vector3(oldStaticMesh.CollisionX2,
                                                                  -oldStaticMesh.CollisionY2,
                                                                  oldStaticMesh.CollisionZ2));

            staticMesh.VisibilityBox = new BoundingBox(new Vector3(oldStaticMesh.VisibilityX1,
                                                                   -oldStaticMesh.VisibilityY1,
                                                                   oldStaticMesh.VisibilityZ1),
                                                       new Vector3(oldStaticMesh.VisibilityX2,
                                                                   -oldStaticMesh.VisibilityY2,
                                                                   oldStaticMesh.VisibilityZ2));

            staticMesh.Mesh = ConvertTr4MeshToWadMesh(wad, oldWad, textures,
                                                      oldWad.Meshes[(int)oldWad.RealPointers[oldStaticMesh.PointersIndex]],
                                                      (int)oldStaticMesh.ObjectId);

            staticMesh.Shatter = TrCatalog.IsStaticShatterable(wad.GameVersion, staticMesh.Id.TypeId);
            wad.Statics.Add(staticMesh.Id, staticMesh);

            return staticMesh;
        }

        private static TextureArea CalculateTr4UVCoordinates(Wad2 wad, Tr4Wad oldWad, wad_polygon poly, Dictionary<int, TextureArea> textures)
        {
            int textureId = GetTr4TextureIdFromPolygon(poly);
            var textureArea = textures[textureId];

            textureArea.BlendMode = (poly.Attributes & 0x01) != 0 ? BlendMode.Additive : BlendMode.Normal;
            textureArea.DoubleSided = false;

            // Add the UV coordinates
            int shape = (poly.Texture & 0x7000) >> 12;
            int flipped = (poly.Texture & 0x8000) >> 15;

            Vector2 nw = textureArea.TexCoord0;
            Vector2 ne = textureArea.TexCoord1;
            Vector2 se = textureArea.TexCoord2;
            Vector2 sw = textureArea.TexCoord3;

            if (poly.Shape == 9)
            {
                if (flipped == 1)
                {
                    textureArea.TexCoord0 = ne;
                    textureArea.TexCoord1 = nw;
                    textureArea.TexCoord2 = sw;
                    textureArea.TexCoord3 = se;
                }
                else
                {
                    textureArea.TexCoord0 = nw;
                    textureArea.TexCoord1 = ne;
                    textureArea.TexCoord2 = se;
                    textureArea.TexCoord3 = sw;
                }
            }
            else
            {
                switch (shape)
                {
                    case 0:
                        if (flipped == 1)
                        {
                            textureArea.TexCoord0 = ne;
                            textureArea.TexCoord1 = nw;
                            textureArea.TexCoord2 = se;
                            textureArea.TexCoord3 = textureArea.TexCoord2;
                        }
                        else
                        {
                            textureArea.TexCoord0 = nw;
                            textureArea.TexCoord1 = ne;
                            textureArea.TexCoord2 = sw;
                            textureArea.TexCoord3 = textureArea.TexCoord2;
                        }
                        break;

                    case 2:
                        if (flipped == 1)
                        {
                            textureArea.TexCoord0 = nw;
                            textureArea.TexCoord1 = sw;
                            textureArea.TexCoord2 = ne;
                            textureArea.TexCoord3 = textureArea.TexCoord2;
                        }
                        else
                        {
                            textureArea.TexCoord0 = ne;
                            textureArea.TexCoord1 = se;
                            textureArea.TexCoord2 = nw;
                            textureArea.TexCoord3 = textureArea.TexCoord2;
                        }
                        break;

                    case 4:
                        if (flipped == 1)
                        {
                            textureArea.TexCoord0 = sw;
                            textureArea.TexCoord1 = se;
                            textureArea.TexCoord2 = nw;
                            textureArea.TexCoord3 = textureArea.TexCoord2;
                        }
                        else
                        {
                            textureArea.TexCoord0 = se;
                            textureArea.TexCoord1 = sw;
                            textureArea.TexCoord2 = ne;
                            textureArea.TexCoord3 = textureArea.TexCoord2;
                        }
                        break;

                    case 6:
                        if (flipped == 1)
                        {
                            textureArea.TexCoord0 = se;
                            textureArea.TexCoord1 = ne;
                            textureArea.TexCoord2 = sw;
                            textureArea.TexCoord3 = textureArea.TexCoord2;
                        }
                        else
                        {
                            textureArea.TexCoord0 = sw;
                            textureArea.TexCoord1 = nw;
                            textureArea.TexCoord2 = se;
                            textureArea.TexCoord3 = textureArea.TexCoord2;
                        }
                        break;
                    default:
                        throw new NotImplementedException("Unknown texture shape " + shape + " found in the wad.");
                }
            }

            return textureArea;
        }
    }
}
