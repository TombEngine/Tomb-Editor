using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib.IO;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.LevelData.Compilers.TombEngine
{
    public sealed partial class LevelCompilerTombEngine
    {
        private static readonly bool _writeDbgWadTxt = false;
        private List<int> _finalSelectedSoundsList;
        private List<WadSoundInfo> _finalSoundInfosList;
        private List<WadSample> _finalSamplesList;
        private int _soundMapSize = 0;
        private short[] _finalSoundMap;

        private TombEngineMesh ConvertWadMesh(WadMesh oldMesh, bool isStatic, string objectName, int meshIndex = 0)
        {
            var newMesh = new TombEngineMesh
            {
                Sphere = oldMesh.BoundingSphere,
                LightingType = oldMesh.LightingType,
                Hidden = oldMesh.Hidden
            };

            var objectString = isStatic ? "Static" : "Moveable";

            if (!oldMesh.HasNormals)
            {
                _progressReporter.ReportWarn(string.Format(objectString + " {0} has a mesh with invalid lighting data. Normals will be recalculated on the fly.", objectName));
                oldMesh.CalculateNormals();
            }

            for (int i = 0; i < oldMesh.VertexPositions.Count; i++)
            {
                var pos    = oldMesh.VertexPositions[i];
                var normal = oldMesh.VertexNormals[i];
                var color  = (oldMesh.HasColors) ? oldMesh.VertexColors[i] : new Vector3(1.0f);

                var v = new TombEngineVertex() 
                { 
                    Position   = new Vector3(pos.X, -pos.Y, pos.Z),
                    Normal     = Vector3.Normalize(new Vector3(normal.X, -normal.Y, normal.Z)),
                    Color      = color,
                    BoneIndex  = oldMesh.HasWeights && oldMesh.VertexWeights[i].Valid() ? oldMesh.VertexWeights[i].Index : new int[4] { meshIndex, 0, 0, 0 },
                    BoneWeight = oldMesh.HasWeights && oldMesh.VertexWeights[i].Valid() ? oldMesh.VertexWeights[i].Weight : new float[4] { 1, 0, 0, 0 },
                    Glow       = oldMesh.HasAttributes ? (float)oldMesh.VertexAttributes[i].Glow / 64.0f : 0.0f,
                    Move       = oldMesh.HasAttributes ? (float)oldMesh.VertexAttributes[i].Move / 64.0f : 0.0f
            };

                newMesh.Vertices.Add(v);
            }

            for (int j = 0; j < oldMesh.Polys.Count; j++)
            {
                var poly = oldMesh.Polys[j];

                // Check if we should merge object and room textures in same texture tiles.
                TextureDestination destination = isStatic ? TextureDestination.Static : TextureDestination.Moveable;
                
                var texture = poly.Texture;
                texture.ClampToBounds();

                foreach (bool doubleSided in new[] { false, true })
                {
                    if (doubleSided && !texture.DoubleSided)
                        break;

					if (doubleSided)
                        texture.Mirror(poly.IsTriangle);
                    var result = _textureInfoManager.AddTexture(texture, destination, poly.IsTriangle, texture.BlendMode);

                    int[] indices = poly.IsTriangle ? new int[] { poly.Index0, poly.Index1, poly.Index2 } : 
                                                      new int[] { poly.Index0, poly.Index1, poly.Index2, poly.Index3 };

                    if (doubleSided)
                        Array.Reverse(indices);

                    var realBlendMode = texture.BlendMode;
                    if (texture.BlendMode == BlendMode.Normal)
                        realBlendMode = texture.Texture.Image.HasAlpha(TRVersion.Game.TombEngine, texture.GetRect());
					
					var textureAbsolutePath = ((WadTexture)texture.Texture).AbsolutePath;
                    int materialIndex = -1;
                    if (!string.IsNullOrEmpty(textureAbsolutePath))
                        materialIndex = _materialNames.IndexOf(textureAbsolutePath);
                    if (materialIndex == -1)
                        materialIndex = 0;

                    TombEnginePolygon newPoly;
                    if (poly.IsTriangle)
                        newPoly = result.CreateTombEnginePolygon3(indices, realBlendMode, materialIndex, newMesh.Vertices);
                    else
                        newPoly = result.CreateTombEnginePolygon4(indices, realBlendMode, materialIndex, newMesh.Vertices);

                    newPoly.ShineStrength = (float)poly.ShineStrength / 63.0f;

                    newMesh.Polygons.Add(newPoly);

                    newMesh.Vertices[indices[0]].NormalHelpers.Add(new NormalHelper(newPoly));
                    newMesh.Vertices[indices[1]].NormalHelpers.Add(new NormalHelper(newPoly));
                    newMesh.Vertices[indices[2]].NormalHelpers.Add(new NormalHelper(newPoly));
                    if (!poly.IsTriangle)
                        newMesh.Vertices[indices[3]].NormalHelpers.Add(new NormalHelper(newPoly));

                    newPoly.Normals.Add(newMesh.Vertices[newPoly.Indices[0]].Normal);
                    newPoly.Normals.Add(newMesh.Vertices[newPoly.Indices[1]].Normal);
                    newPoly.Normals.Add(newMesh.Vertices[newPoly.Indices[2]].Normal);
                    if (!poly.IsTriangle)
                        newPoly.Normals.Add(newMesh.Vertices[newPoly.Indices[3]].Normal);
                }
            }

            _meshes.Add(newMesh);

            return newMesh;
        }

        private void PrepareMeshBuckets()
        {
            for (int i = 0; i < _meshes.Count; i++)
                PrepareMeshBuckets(_meshes[i]);
        }

        private void PrepareMeshBuckets(TombEngineMesh mesh)
        {
            var textures = _textureInfoManager.GetObjectTextures();
            var buckets = new Dictionary<TombEngineMaterial, TombEngineBucket>(new TombEngineMaterial.TombEngineMaterialComparer());
            
            foreach (var poly in mesh.Polygons)
            {
                poly.AnimatedSequence = -1;
                poly.AnimatedFrame = -1;

                if (poly.Animated)
                {
                    var animInfo = _textureInfoManager.GetAnimatedTexture(poly.TextureId);
                    if (animInfo != null)
                    {
                        poly.AnimatedSequence = animInfo.Item1;
                        poly.AnimatedFrame = animInfo.Item2;
                    }
                }

                var bucket = GetOrAddBucket(textures[poly.TextureId].AtlasIndex, poly.BlendMode, poly.MaterialIndex, poly.AnimatedSequence, buckets);

                var texture = textures[poly.TextureId];

                // We output only triangles, no quads anymore
                if (poly.Shape == TombEnginePolygonShape.Quad)
                {
                    for (int n = 0; n < 4; n++)
                    {
                        poly.TextureCoordinates.Add(texture.TexCoordFloat[n]);
                        poly.Tangents.Add(Vector3.Zero);
                        poly.Binormals.Add(Vector3.Zero);
                    }

                    bucket.Polygons.Add(poly);
                }
                else
                {
                    for (int n = 0; n < 3; n++)
                    {
                        poly.TextureCoordinates.Add(texture.TexCoordFloat[n]);
                        poly.Tangents.Add(poly.Tangent);
                        poly.Binormals.Add(poly.Binormal);
                    }

                    bucket.Polygons.Add(poly);
                }
            }

            mesh.Buckets = buckets.Values.ToList();
            mesh.Buckets.Sort(TombEngineBucketComparer.Instance);

            // Calculate tangents and bitangents
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var vertex = mesh.Vertices[i];
                var normalHelpers = vertex.NormalHelpers;

                for (int j = 0; j < normalHelpers.Count; j++)
                {
                    var normalHelper = normalHelpers[j];

                    var e1 = mesh.Vertices[normalHelper.Polygon.Indices[1]].Position - mesh.Vertices[normalHelper.Polygon.Indices[0]].Position;
                    var e2 = mesh.Vertices[normalHelper.Polygon.Indices[2]].Position - mesh.Vertices[normalHelper.Polygon.Indices[0]].Position;

                    var uv1 = normalHelper.Polygon.TextureCoordinates[1] - normalHelper.Polygon.TextureCoordinates[0];
                    var uv2 = normalHelper.Polygon.TextureCoordinates[2] - normalHelper.Polygon.TextureCoordinates[0];

                    float r = 1.0f / (uv1.X * uv2.Y - uv1.Y * uv2.X);
                    normalHelper.Polygon.Tangent = Vector3.Normalize((e1 * uv2.Y - e2 * uv1.Y) * r);
                    normalHelper.Polygon.Binormal = Vector3.Normalize((e2 * uv1.X - e1 * uv2.X) * r);
                }
            }

            // Average everything
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var vertex = mesh.Vertices[i];
                var normalHelpers = vertex.NormalHelpers;

                for (int j = 0; j < normalHelpers.Count; j++)
                {
                    var normalHelper = normalHelpers[j];

                    for (int k = 0; k < normalHelper.Polygon.Indices.Count; k++)
                    {
                        int index = normalHelper.Polygon.Indices[k];
                        if (index == i)
                        {
                            normalHelper.Polygon.Tangents[k] = normalHelper.Polygon.Tangent;
                            normalHelper.Polygon.Binormals[k] = normalHelper.Polygon.Binormal;
                            break;
                        }
                    }
                }
            }
        }

        private class AnimationTr4HelperData
        {
            public int KeyFrameOffset { get; set; }
            public int KeyFrameSize { get; set; }
        }

        public void ConvertWad2DataToTombEngine()
        {
            ReportProgress(0, "Preparing WAD data");

            SortedList<WadMoveableId, WadMoveable> moveables = _level.Settings.WadGetAllMoveables();
            SortedList<WadStaticId, WadStatic> statics = _level.Settings.WadGetAllStatics();

            // First thing build frames
            ReportProgress(1, "Building animations");
            var animationDictionary = new Dictionary<WadAnimation, int>(new ReferenceEqualityComparer<WadAnimation>());
            foreach (WadMoveable moveable in moveables.Values)
                foreach (var animation in moveable.Animations)
                { 
                    animationDictionary.Add(animation, _frames.Count);

                    foreach (var wadFrame in animation.KeyFrames)
                    {
                        var newFrame = new TombEngineKeyFrame
                        {
                            Angles = new List<Quaternion>()
                        };

                        newFrame.BoundingBox.X1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Minimum.X));
                        newFrame.BoundingBox.Y1 = (short)-Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Minimum.Y));
                        newFrame.BoundingBox.Z1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Minimum.Z));
                        newFrame.BoundingBox.X2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Maximum.X));
                        newFrame.BoundingBox.Y2 = (short)-Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Maximum.Y));
                        newFrame.BoundingBox.Z2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, wadFrame.BoundingBox.Maximum.Z));
                        newFrame.Offset = new Vector3(wadFrame.Offset.X, -wadFrame.Offset.Y, wadFrame.Offset.Z);

                        foreach (var oldRot in wadFrame.Angles)
                        {
                            newFrame.Angles.Add(oldRot.Quaternion);
                        }

                        _frames.Add(newFrame);
                    }
                }

			ReportProgress(3, "Building moveables");
			
            int lastAnimation = 0;
            int lastAnimDispatch = 0;
            foreach (WadMoveable oldMoveable in moveables.Values)
            {
                var newMoveable = new TombEngineMoveable();
                newMoveable.Animation = oldMoveable.Animations.Count != 0 ? lastAnimation : -1;
                newMoveable.NumMeshes = oldMoveable.Meshes.Count();
                newMoveable.ObjectID = checked((int)oldMoveable.Id.TypeId);
                newMoveable.FrameOffset = 0;

                // Determine possible skin object to shift bone offsets.
                var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(_level.Settings.GameVersion, oldMoveable.Id.TypeId));
                var skin = _level.Settings.WadTryGetMoveable(skinId);

                // Add animations
                int realFrameBase = 0;
                for (int j = 0; j < oldMoveable.Animations.Count; ++j)
                {
                    var oldAnimation = oldMoveable.Animations[j];
                    var newAnimation = new TombEngineAnimation();
                    var offset = animationDictionary[oldAnimation];

                    // Clamp EndFrame to max. frame count as a last resort to prevent glitching animations.

                    var frameCount = oldAnimation.EndFrame + 1;
                    var maxFrame   = oldAnimation.GetRealNumberOfFrames(oldAnimation.KeyFrames.Count);
                    if (frameCount > maxFrame)
                        frameCount = maxFrame;

                    // Setup the final animation
                    newAnimation.FrameOffset = offset;
                    newAnimation.FrameRate = oldAnimation.FrameRate;
                    newAnimation.VelocityStart = new Vector3(oldAnimation.StartLateralVelocity, 0, oldAnimation.StartVelocity);
                    newAnimation.VelocityEnd = new Vector3(oldAnimation.EndLateralVelocity, 0, oldAnimation.EndVelocity);
                    newAnimation.FrameStart = realFrameBase;
                    newAnimation.FrameEnd = realFrameBase + (frameCount == 0 ? 0 : frameCount - 1);
                    newAnimation.AnimCommand = _animCommands.Count;
                    newAnimation.StateChangeOffset = _stateChanges.Count;
                    newAnimation.NumAnimCommands = oldAnimation.AnimCommands.Count;
                    newAnimation.NumStateChanges = oldAnimation.StateChanges.Count;
                    newAnimation.NextFrame = oldAnimation.NextFrame;
                    newAnimation.StateID = oldAnimation.StateId;

                    // Check if next animation contains valid value. If not, set to zero and throw a warning.
                    if (oldAnimation.NextAnimation >= oldMoveable.Animations.Count)
                    {
                        _progressReporter.ReportWarn("Object '" + oldMoveable.Id.ShortName(_level.Settings.GameVersion) +
                                                     "' refers to incorrect next animation " + oldAnimation.NextAnimation + " in animation " + j + ". It will be set to 0.");
                        newAnimation.NextAnimation = lastAnimation;
                    }
                    else
                        newAnimation.NextAnimation = oldAnimation.NextAnimation + lastAnimation;

                    // Add anim commands
                    foreach (var command in oldAnimation.AnimCommands)
                    {
                        _animCommands.Add((int)command.Type);

                        switch (command.Type)
                        {
                            case WadAnimCommandType.SetPosition:
                                _animCommands.Add(command.Parameter1);
                                _animCommands.Add(command.Parameter2);
                                _animCommands.Add(command.Parameter3);
                                break;
								
                            case WadAnimCommandType.SetJumpDistance:
                                _animCommands.Add(command.Parameter1);
                                _animCommands.Add(command.Parameter2);
                                break;

                            case WadAnimCommandType.EmptyHands:
                                break;

                            case WadAnimCommandType.KillEntity:
                                break;

                            case WadAnimCommandType.PlaySound:
                                _animCommands.Add(command.Parameter1 + newAnimation.FrameStart);
                                _animCommands.Add(command.Parameter2);
                                _animCommands.Add(command.Parameter3);
                                break;

                            case WadAnimCommandType.FlipEffect:
                                _animCommands.Add(command.Parameter1 + newAnimation.FrameStart);
                                _animCommands.Add(command.Parameter2);
                                break;

                            case WadAnimCommandType.DisableInterpolation:
                                _animCommands.Add(command.Parameter1 + newAnimation.FrameStart);
                                break;
                        }
                    }

                    // Add state changes
                    foreach (var stateChange in oldAnimation.StateChanges)
                    {
                        var newStateChange = new TombEngineStateChange();

                        newStateChange.AnimDispatch = lastAnimDispatch;
                        newStateChange.StateID = (int)stateChange.StateId;
                        newStateChange.NumAnimDispatches = stateChange.Dispatches.Count;

                        foreach (var dispatch in stateChange.Dispatches)
                        {
                            // If dispatch refers to nonexistent animation, ignore it.
                            if (dispatch.NextAnimation >= oldMoveable.Animations.Count)
                            {
                                _progressReporter.ReportWarn("Object '" + oldMoveable.Id.ShortName(_level.Settings.GameVersion) +
                                                             "' has wrong anim dispatch in animation " + j +
                                                             " which refers to nonexistent animation " + dispatch.NextAnimation + ". It will be removed.");
                                continue;
                            }

                            var newAnimDispatch = new TombEngineAnimDispatch();

                            newAnimDispatch.Low = unchecked((int)(dispatch.InFrame + newAnimation.FrameStart));
                            newAnimDispatch.High = unchecked((int)(dispatch.OutFrame + newAnimation.FrameStart));
                            newAnimDispatch.NextAnimation = checked((int)(dispatch.NextAnimation + lastAnimation));
                            newAnimDispatch.NextFrame = (int)dispatch.NextFrame;

                            _animDispatches.Add(newAnimDispatch);
                            lastAnimDispatch++;
                        }

                        _stateChanges.Add(newStateChange);
                    }

                    _animations.Add(newAnimation);

                    realFrameBase += frameCount < 0 ? 0 : frameCount; // FIXME: Not really needed?
                }
                lastAnimation += oldMoveable.Animations.Count;

                newMoveable.MeshTree = _meshTrees.Count;
                newMoveable.StartingMesh = _meshes.Count;

                for (int i = 0; i < oldMoveable.Meshes.Count; i++) {
                    var wadMesh = oldMoveable.Meshes[i];
                    ConvertWadMesh(
                        wadMesh, 
                        false, 
                        oldMoveable.Id.ShortName(_level.Settings.GameVersion), 
                        i);
                }

                newMoveable.Skin = -1;
                if (oldMoveable.Skin != null)
                {
                    newMoveable.Skin = _meshes.Count;
                    var wadSkin = oldMoveable.Skin;
                    ConvertWadMesh(
                        wadSkin,
                        false,
                        oldMoveable.Id.ShortName(_level.Settings.GameVersion));
                }

                var meshTrees = new List<tr_meshtree>();

                for (int b = 1; b < oldMoveable.Bones.Count; b++)
                {
                    var tree = new tr_meshtree();
                    var bone = oldMoveable.Bones[b];

                    if (skin != null && skin.Bones.Count == oldMoveable.Bones.Count)
                        bone = skin.Bones[b];

                    tree.Opcode = (int)oldMoveable.Bones[b].OpCode;
                    tree.X = (int) bone.Translation.X;
                    tree.Y = (int)-bone.Translation.Y;
                    tree.Z = (int) bone.Translation.Z;

                    meshTrees.Add(tree);
                }

                foreach (var meshTree in meshTrees)
                {
                    _meshTrees.Add(meshTree.Opcode);
                    _meshTrees.Add(meshTree.X);
                    _meshTrees.Add(meshTree.Y);
                    _meshTrees.Add(meshTree.Z);
                }

                _moveables.Add(newMoveable);
            }

            // Adjust NextFrame of each Animation
            for (int i = 0; i < _animations.Count; i++)
            {
                var animation = _animations[i];
                animation.NextFrame += _animations[animation.NextAnimation].FrameStart;
                _animations[i] = animation;
            }

            // Adjust NextFrame of each AnimDispatch
            for (int i = 0; i < _animDispatches.Count; i++)
            {
                var dispatch = _animDispatches[i];
                dispatch.NextFrame += _animations[dispatch.NextAnimation].FrameStart;
                _animDispatches[i] = dispatch;
            }

			// Convert static meshes

			ReportProgress(4, "Building statics");

			int convertedStaticsCount = 0;
            ReportProgress(10, "Converting static meshes");
            foreach (WadStatic oldStaticMesh in statics.Values)
            {
                var newStaticMesh = new TombEngineStaticMesh();

                newStaticMesh.ObjectID = checked((int)oldStaticMesh.Id.TypeId);

                newStaticMesh.CollisionBox = new TombEngineBoundingBox
                {
                    X1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.CollisionBox.Minimum.X)),
                    X2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.CollisionBox.Maximum.X)),
                    Y1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -oldStaticMesh.CollisionBox.Minimum.Y)),
                    Y2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -oldStaticMesh.CollisionBox.Maximum.Y)),
                    Z1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.CollisionBox.Minimum.Z)),
                    Z2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.CollisionBox.Maximum.Z))
                };

                newStaticMesh.VisibilityBox = new TombEngineBoundingBox
                {
                    X1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.VisibilityBox.Minimum.X)),
                    X2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.VisibilityBox.Maximum.X)),
                    Y1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -oldStaticMesh.VisibilityBox.Minimum.Y)),
                    Y2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, -oldStaticMesh.VisibilityBox.Maximum.Y)),
                    Z1 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.VisibilityBox.Minimum.Z)),
                    Z2 = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, oldStaticMesh.VisibilityBox.Maximum.Z))
                };

                newStaticMesh.Flags = (ushort)oldStaticMesh.Flags;
                newStaticMesh.Mesh = (short)_meshes.Count;

                newStaticMesh.ShatterType = oldStaticMesh.Shatter ? (short)ShatterType.Fragment : (short)ShatterType.None;
                newStaticMesh.ShatterSound = (short)oldStaticMesh.ShatterSoundID;

                // Do not add faces and vertices to the wad, instead keep only the bounding boxes when we automatically merge the Mesh
                if (_level.Settings.FastMode || !_level.Settings.AutoStaticMeshMergeContainsStaticMesh(oldStaticMesh))
                {
                    ConvertWadMesh(
                        oldStaticMesh.Mesh, 
                        true,
                        oldStaticMesh.Id.ShortName(_level.Settings.GameVersion));
                }
                else
                {
                    convertedStaticsCount++;
                    logger.Info("Creating Dummy Mesh for automatically Merged Mesh: " + oldStaticMesh.ToString(_level.Settings.GameVersion));
                    CreateDummyWadMesh(oldStaticMesh.Mesh);
                }
                _staticMeshes.Add(newStaticMesh);
            }
            
            if (convertedStaticsCount > 0)
                _progressReporter.ReportInfo("    Number of statics merged with room geometry: " + convertedStaticsCount);
            else
                _progressReporter.ReportInfo("    No statics to merge into room geometry.");

            if (_writeDbgWadTxt)
                using (var fileStream = new FileStream("Wad.txt", FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(fileStream))
                {
                    int n = 0;
                    foreach (var anim in _animations)
                    {
                        writer.WriteLine("Anim #" + n);
                        writer.WriteLine("    KeyframeOffset: " + anim.FrameOffset);
                        writer.WriteLine("    FrameRate: " + anim.FrameRate);
                        writer.WriteLine("    FrameStart: " + anim.FrameStart);
                        writer.WriteLine("    FrameEnd: " + anim.FrameEnd);
                        writer.WriteLine("    StateChangeOffset: " + anim.StateChangeOffset);
                        writer.WriteLine("    NumStateChanges: " + anim.NumStateChanges);
                        writer.WriteLine("    AnimCommand: " + anim.AnimCommand);
                        writer.WriteLine("    NumAnimCommands: " + anim.NumAnimCommands);
                        writer.WriteLine("    NextAnimation: " + anim.NextAnimation);
                        writer.WriteLine("    NextFrame: " + anim.NextFrame);
                        writer.WriteLine("    StateID: " + anim.StateID);
                        writer.WriteLine("    VelStart: " + anim.VelocityStart.Z.ToString("X"));
                        writer.WriteLine("    VelEnd: " + anim.VelocityEnd.Z.ToString("X"));
                        writer.WriteLine("    VelLateralStart: " + anim.VelocityStart.X.ToString("X"));
                        writer.WriteLine("    VelLateralEnd: " + anim.VelocityEnd.X.ToString("X"));
                        writer.WriteLine();

                        n++;
                    }

                    n = 0;
                    foreach (var dispatch in _animDispatches)
                    {
                        writer.WriteLine("AnimDispatch #" + n);
                        writer.WriteLine("    In: " + dispatch.Low);
                        writer.WriteLine("    Out: " + dispatch.High);
                        writer.WriteLine("    NextAnimation: " + dispatch.NextAnimation);
                        writer.WriteLine("    NextFrame: " + dispatch.NextFrame);
                        writer.WriteLine();

                        n++;
                    }

                    n = 0;
                    for (int jj = 0; jj < _meshTrees.Count; jj += 4)
                    {
                        writer.WriteLine("MeshTree #" + jj);
                        writer.WriteLine("    Op: " + _meshTrees[jj + 0]);
                        writer.WriteLine("    X: " + _meshTrees[jj + 1]);
                        writer.WriteLine("    Y: " + _meshTrees[jj + 2]);
                        writer.WriteLine("    Z: " + _meshTrees[jj + 3]);
                        writer.WriteLine();

                        n++;
                    }

                    n = 0;
                    foreach (var mov in _moveables)
                    {
                        writer.WriteLine("Moveable #" + n);
                        writer.WriteLine("    MeshTree: " + mov.MeshTree);
                        writer.WriteLine("    MeshPointer: " + mov.StartingMesh);
                        writer.WriteLine("    AnimationIndex: " + mov.Animation);
                        writer.WriteLine("    NumMeshes: " + mov.NumMeshes);
                        writer.WriteLine();

                        n++;
                    }

                    n = 0;
                    foreach (var sc in _stateChanges)
                    {
                        writer.WriteLine("StateChange #" + n);
                        writer.WriteLine("    StateID: " + sc.StateID);
                        writer.WriteLine("    NumAnimDispatches: " + sc.NumAnimDispatches);
                        writer.WriteLine("    AnimDispatch: " + sc.AnimDispatch);
                        writer.WriteLine();

                        n++;
                    }
                }
        }

        public static short ToTrAngle(float angle)
        {
            double result = Math.Round(angle * (65536.0f / 360.0f));
            return unchecked((short)result);
        }

        private void BuildMeshTree(WadBone bone, List<tr_meshtree> meshTrees, List<WadMesh> usedMeshes)
        {
            tr_meshtree tree = new tr_meshtree();
            tree.X = (int)bone.Translation.X;
            tree.Y = (int)-bone.Translation.Y;
            tree.Z = (int)bone.Translation.Z;
            

            if (bone.Parent == null)
                tree.Opcode = 2;
            else
            {
                if (bone.Parent.Children.Count == 1)
                    tree.Opcode = 0;
                else
                {
                    int childrenCount = bone.Parent.Children.Count;
                    if (bone.Parent.Children.IndexOf(bone) == 0)
                        tree.Opcode = 2;
                    else if (bone.Parent.Children.IndexOf(bone) == childrenCount - 1)
                        tree.Opcode = 1;
                    else
                        tree.Opcode = 3;
                }
            }

            if (bone.Parent != null)
                meshTrees.Add(tree);

            usedMeshes.Add(bone.Mesh);

            for (int i = 0; i < bone.Children.Count; i++)
                BuildMeshTree(bone.Children[i], meshTrees, usedMeshes);
        }

        private void PrepareSoundsData()
        {
            // Step 1: create the real list of sounds and indices to compile

            _finalSoundInfosList = new List<WadSoundInfo>();

            if (_level.Settings.GameVersion.UsesMainSfx())
                _finalSelectedSoundsList = new List<int>(_level.Settings.SelectedSounds);
            else
                _finalSelectedSoundsList = new List<int>(_level.Settings.SelectedAndAvailableSounds);

            foreach (var soundInfo in _level.Settings.GlobalSoundMap)
                if (_finalSelectedSoundsList.Contains(soundInfo.Id))
                {
                    // Check if sound is marked as indexed. If not, exclude it from processing.
                    if (_level.Settings.GameVersion.UsesMainSfx() && !soundInfo.Indexed)
                    {
                        _progressReporter.ReportWarn("Sound info #" + soundInfo.Id +
                            " was selected but was not set as included for TR2-3 MAIN.SFX in SoundTool. It was removed.");
                        _finalSelectedSoundsList.Remove(soundInfo.Id);
                    }
                    else
                        _finalSoundInfosList.Add(soundInfo);
                }

            // Step 2: create the sound map

             _soundMapSize = _limits[Limit.SoundMapSize];
            _finalSoundMap = Enumerable.Repeat((short)-1, _soundMapSize).ToArray<short>();
            foreach (var sound in _finalSoundInfosList)
            {
                if (sound.Id >= _soundMapSize)
                {
                    _progressReporter.ReportWarn("Sound info #" + sound.Id + " wasn't included because max. ID for this game version is " + _soundMapSize + ".");
                    continue;
                }
                _finalSoundMap[sound.Id] = (short)_finalSoundInfosList.IndexOf(sound);
            }

            // Step 3: load samples
            var loadedSamples = WadSample.CompileSamples(_finalSoundInfosList, _level.Settings, false, _progressReporter);
            _finalSamplesList = loadedSamples.Values.ToList();
        }

        private void WriteSoundMetadata(BinaryWriter writer)
        {
            writer.Write((ushort)_soundMapSize);

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriterEx(ms))
                {
                    // Write soundmap to level file
                    for (int i = 0; i < _finalSoundMap.Length; i++)
                        bw.Write(_finalSoundMap[i]);

                    // Write sound details
                    int lastSampleIndex = 0;
                    bw.Write((uint)_finalSoundInfosList.Count);
                    for (int i = 0; i < _finalSoundInfosList.Count; i++)
                    {
                        var soundDetail = _finalSoundInfosList[i];

                        if (soundDetail.Samples.Count > 0x0F)
                            throw new Exception("Too many sound effects for sound info '" + soundDetail.Name + "'.");

                        ushort characteristics = (ushort)(3 & (int)soundDetail.LoopBehaviour);
                        characteristics |= (ushort)(soundDetail.Samples.Count << 2);
                        if (soundDetail.DisablePanning)
                            characteristics |= 0x1000;
                        if (soundDetail.RandomizePitch)
                            characteristics |= 0x2000;
                        if (soundDetail.RandomizeVolume)
                            characteristics |= 0x4000;

                        var newSoundDetail = new tr3_sound_details();
                        newSoundDetail.Sample = (ushort)lastSampleIndex;
                        newSoundDetail.Volume = (byte)Math.Round(soundDetail.Volume / WadSoundInfo.MaxAttribValue * byte.MaxValue);
                        newSoundDetail.Chance = (byte)Math.Floor((soundDetail.Chance == WadSoundInfo.MaxAttribValue ? 0 : soundDetail.Chance) / WadSoundInfo.MaxAttribValue * byte.MaxValue);
                        newSoundDetail.Range  = (byte)soundDetail.RangeInSectors;
                        newSoundDetail.Pitch  = (byte)Math.Round(soundDetail.PitchFactor / WadSoundInfo.MaxAttribValue * sbyte.MaxValue + (soundDetail.PitchFactor < 0 ? (byte.MaxValue + 1) : 0));
                        newSoundDetail.Characteristics = characteristics;
                        bw.WriteBlock(newSoundDetail);
                        lastSampleIndex += soundDetail.Samples.Count;
                    }
                }

                writer.Write(ms.ToArray());
            }
        }

        private void WriteSoundData(BinaryWriter writer)
        {
            var sampleRate = _limits[Limit.SoundSampleRate];

            writer.Write((uint)_finalSamplesList.Count); // Write sample count

            for (int i = 0; i < _finalSamplesList.Count; ++i)
            {
                writer.Write((uint)_finalSamplesList[i].Data.Length);
                writer.Write((uint)_finalSamplesList[i].Data.Length);
                writer.Write(_finalSamplesList[i].Data);
            }
        }

        private TombEngineMesh CreateDummyWadMesh(WadMesh oldMesh)
        {
            var newMesh = new TombEngineMesh
            {
                Sphere = oldMesh.BoundingSphere
            };

            _meshes.Add(newMesh);

            return newMesh;
        }
    }
}
