using Assimp;
using Assimp.Configs;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.GeometryIO.Importers
{
    public class AssimpImporter : BaseGeometryImporter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AssimpImporter(IOGeometrySettings settings, GetTextureDelegate getTextureCallback)
            : base(settings, getTextureCallback)
        {

        }

        private static List<string> CollectMeshNodeNames(Node node, List<string> list = null)
        {
            if (list == null)
                list = new List<string>();

            if (node.HasMeshes && !list.Any(entry => entry.Equals(node.Name)))
                list.Add(node.Name);

            if (node.HasChildren && node.ChildCount > 0)
                foreach (var child in node.Children)
                    CollectMeshNodeNames(child, list);

            return list;
        }

        private static void TraverseBoneNodes(Node node, HashSet<string> boneNames, List<string> output)
        {
            if (boneNames.Contains(node.Name))
            {
                output.Add(node.Name);
            }

            foreach (var child in node.Children)
            {
                TraverseBoneNodes(child, boneNames, output);
            }
        }

        public static List<string> GetLinearizedBoneList(Scene scene)
        {
            var boneNames = new HashSet<string>();

            // Collect all unique bone names from all meshes
            foreach (var mesh in scene.Meshes)
            {
                foreach (var bone in mesh.Bones)
                {
                    boneNames.Add(bone.Name);
                }
            }

            var orderedBones = new List<string>();

            // Traverse hierarchy and add only nodes that are bones
            TraverseBoneNodes(scene.RootNode, boneNames, orderedBones);

            // Sort bones by name
            orderedBones = orderedBones.OrderBy(m => m, new CustomComparer<string>(NaturalComparer.Do)).ToList();
            return orderedBones;
        }

        public override IOModel ImportFromFile(string filename)
        {
            string path = Path.GetDirectoryName(filename);

            // Use Assimp.NET for importing model
            AssimpContext context = new AssimpContext();
            context.SetConfig(new NormalSmoothingAngleConfig(90.0f));

            // Disable merging similar materials because we encode double-sided attrib in the material name.
            // Also we disable triangulation because legacy meshes still need quads.
            Scene scene = context.ImportFile(filename,
                PostProcessPreset.TargetRealTimeFast ^ 
                PostProcessSteps.Triangulate);

            var newModel = new IOModel();
            var textures = new Dictionary<int, Texture>();

            if (_settings.ProcessGeometry)
            {
                var tmpList = new List<IOMesh>();

                // Create the list of materials to load
                for (int i = 0; i < scene.Materials.Count; i++)
                {
                    var mat = scene.Materials[i];
                    var material = new IOMaterial(mat.HasName ? mat.Name : "Material_" + i);

                    var diffusePath = mat.HasTextureDiffuse ? mat.TextureDiffuse.FilePath : null;
                    if (string.IsNullOrWhiteSpace(diffusePath))
                        continue;

                    // Don't add materials with missing textures
                    var texture = GetTexture(path, diffusePath);

                    // Try to get embedded texture, if it exists
                    if (texture == null && scene.TextureCount > 0 && mat.HasTextureDiffuse)
                    {
                        var matchingTextures = scene.Textures.Where(t => t.Filename == diffusePath).ToList();
                        if (matchingTextures.Count > 0)
                        {
                            var matchingTexture = matchingTextures.First();
                            if (matchingTexture.CompressedData != null)
                            {
                                using (var stream = new MemoryStream(matchingTexture.CompressedData))
                                    texture = new WadTexture(ImageC.FromStream(stream));
                            }
                            else if (matchingTexture.NonCompressedData != null)
                            {
                                var buffer = new byte[matchingTexture.NonCompressedData.Length * 4];
                                for (int t = 0; t < matchingTexture.NonCompressedData.Length; t++)
                                {
                                    int offset = t * 4;
                                    buffer[offset + 0] = matchingTexture.NonCompressedData[i].B;
                                    buffer[offset + 1] = matchingTexture.NonCompressedData[i].G;
                                    buffer[offset + 2] = matchingTexture.NonCompressedData[i].R;
                                    buffer[offset + 3] = matchingTexture.NonCompressedData[i].A;
                                }

                                using (var stream = new MemoryStream(buffer))
                                    texture = new WadTexture(ImageC.FromStreamRaw(stream, matchingTexture.Width, matchingTexture.Height));
                            }
                        }
                    }

                    if (texture == null)
                    {
                        logger.Warn("Texture for material " + mat.Name + " is missing. Meshes referencing this material won't be imported.");
                        continue;
                    }

                     textures.Add(i, texture);

                    // Create the new material
                    material.Texture = textures[i];
                    material.AdditiveBlending = (mat.HasBlendMode && mat.BlendMode == Assimp.BlendMode.Additive) || mat.Opacity < 1.0f 
                        || mat.Name.StartsWith(Graphics.Material.Material_AdditiveBlending)
                        || mat.Name.StartsWith(Graphics.Material.Material_AdditiveBlendingDoubleSided);
                    material.DoubleSided = (mat.HasTwoSided && mat.IsTwoSided) 
                        || mat.Name.StartsWith(Graphics.Material.Material_OpaqueDoubleSided)
                        || mat.Name.StartsWith(Graphics.Material.Material_AdditiveBlendingDoubleSided);

                    // HACK: Ass-imp uses different numbering for shininess in different formats!

                    if (mat.HasShininess)
                    {
                        var extension = Path.GetExtension(filename).ToLower();

                        if (extension == ".obj")
                            material.Shininess = (int)(mat.Shininess * 1023.0f);
                        else
                            material.Shininess = (int)mat.Shininess;
                    }
                    else
                        material.Shininess = 0;

                    newModel.Materials.Add(material);
                }

                var boneList = GetLinearizedBoneList(scene);
                var lastBaseVertex = 0;

                // Loop for each mesh loaded in scene
                foreach (var mesh in scene.Meshes)
                {
                    // Discard nullmeshes
                    if (!mesh.HasFaces || !mesh.HasVertices || mesh.VertexCount < 3)
                    {
                        logger.Warn("Mesh \"" + (mesh.Name ?? "") + "\" has no faces or wrong vertex count.");
                        continue;
                    }

                    // Discard untextured meshes
                    if (mesh.TextureCoordinateChannelCount == 0 || !mesh.HasTextureCoords(0))
                    {
                        logger.Warn("Mesh \"" + (mesh.Name ?? "") + "\" has no texture assigned.");

                        if (!_settings.ProcessUntexturedGeometry)
                            continue;
                    }

                    // Import only textured meshes with valid materials
                    Texture faceTexture = null;
                    if (!textures.TryGetValue(mesh.MaterialIndex, out faceTexture))
                    {
                        logger.Warn("Mesh \"" + (mesh.Name ?? "") + "\" does have material index " + mesh.MaterialIndex + " which is unsupported or can't be found.");

                        if (!_settings.ProcessUntexturedGeometry)
                            continue;
                    }

                    // Make sure we have appropriate material in list. If not, skip mesh and warn user.
                    var material = newModel.Materials.FirstOrDefault(mat => mat.Name.Equals(scene.Materials[mesh.MaterialIndex].Name));
                    if (material == null)
                    {
                        logger.Warn("Can't find material with specified index (" + mesh.MaterialIndex + "). Probably you're missing textures or using non-diffuse materials only for this mesh.");

                        if (_settings.ProcessUntexturedGeometry)
                            material = new IOMaterial(mesh.Name);
                        else
                            continue;
                    }

                    // Assimp's mesh is our IOSubmesh so we import meshes with just one submesh
                    var newMesh = new IOMesh(mesh.Name);
                    var newSubmesh = new IOSubmesh(material);
                    newMesh.Submeshes.Add(material, newSubmesh);

                    bool hasColors   = _settings.UseVertexColor && mesh.VertexColorChannelCount > 0 && mesh.HasVertexColors(0);
                    bool hasNormals  = mesh.HasNormals;
                    bool hasWeights  = mesh.HasBones && mesh.Bones.Any(b => b.HasVertexWeights);
                    bool hasTextures = mesh.HasTextureCoords(0) && (mesh.VertexCount == mesh.TextureCoordinateChannels[0].Count);

                    // Additional integrity checks
                    if ((hasColors  && mesh.VertexCount != mesh.VertexColorChannels[0].Count) ||
                        (hasNormals && mesh.VertexCount != mesh.Normals.Count))
                    {
                        logger.Warn("Mesh \"" + (mesh.Name ?? "") + "\" data structure is inconsistent.");
                        continue;
                    }

                    // Source data
                    var positions = mesh.Vertices;
                    var normals = mesh.Normals;
                    var texCoords = hasTextures ? mesh.TextureCoordinateChannels[0] : null;
                    var colors = mesh.VertexColorChannels[0];

                    for (int i = 0; i < mesh.VertexCount; i++)
                    {
                        // Create position
                        var position = new Vector3(positions[i].X, positions[i].Y, positions[i].Z);

                        position = ApplyAxesTransforms(position);
                        newMesh.Positions.Add(position);

                        // Create normal
                        if (hasNormals)
                        {
                            var normal = new Vector3(normals[i].X, normals[i].Y, normals[i].Z);
                            normal = ApplyAxesTransforms(normal);
                            newMesh.Normals.Add(normal);
                        }
                        else
                            newMesh.CalculateNormals();

                        // Create and scale up UV
                        var currentUV = texCoords == null ? Vector2.Zero : new Vector2(texCoords[i].X, texCoords[i].Y);
                        if (faceTexture != null)
                            currentUV = ApplyUVTransform(currentUV, faceTexture.Image.Width, faceTexture.Image.Height);
                        newMesh.UV.Add(currentUV);

                        // Create colors
                        if (hasColors)
                        {
                            var color = ApplyColorTransform(new Vector4(colors[i].R, colors[i].G, colors[i].B, colors[i].A));
                            newMesh.Colors.Add(color);
                        }

                        // Create weights
                        if (hasWeights)
                        {
                            newMesh.Weights.Add(new List<KeyValuePair<int, float>>());
                        }
                    }

                    // Add vertex weights
                    if (hasWeights)
                    {
                        foreach (var bone in mesh.Bones)
                        {
                            if (!bone.HasVertexWeights)
                                continue;

                            var boneIndex = boneList.IndexOf(bone.Name);
                            
                            foreach (var weight in bone.VertexWeights)
                            {
                                if (weight.VertexID >= newMesh.Weights.Count)
                                {
                                    logger.Warn("Bone \"" + (bone.Name ?? "") + "\" has incorrect vertex ID " + weight.VertexID.ToString() + " for mesh \"" + (mesh.Name ?? "") + ".");
                                    continue;
                                }

                                newMesh.Weights[weight.VertexID].Add(new KeyValuePair<int, float>(boneIndex, weight.Weight));
                            }
                        }

                        for (int i = 0; i < newMesh.Weights.Count; i++)
                            newMesh.Weights[i] = newMesh.Weights[i].OrderByDescending(kvp => kvp.Key).ToList();
                    }

                    // Add polygons
                    foreach (var face in mesh.Faces)
                    {
                        if (face.IndexCount == 3)
                        {
                            var poly = new IOPolygon(IOPolygonShape.Triangle);

                            poly.Indices.Add(lastBaseVertex + face.Indices[0]);
                            poly.Indices.Add(lastBaseVertex + face.Indices[1]);
                            poly.Indices.Add(lastBaseVertex + face.Indices[2]);

                            if (_settings.InvertFaces)
                                poly.Indices.Reverse();

                            newSubmesh.Polygons.Add(poly);
                        }
                        else if (face.IndexCount == 4)
                        {
                            var poly = new IOPolygon(IOPolygonShape.Quad);

                            poly.Indices.Add(lastBaseVertex + face.Indices[0]);
                            poly.Indices.Add(lastBaseVertex + face.Indices[1]);
                            poly.Indices.Add(lastBaseVertex + face.Indices[2]);
                            poly.Indices.Add(lastBaseVertex + face.Indices[3]);

                            if (_settings.InvertFaces)
                                poly.Indices.Reverse();

                            newSubmesh.Polygons.Add(poly);
                        }
                    }

                    tmpList.Add(newMesh);
                }

                // Sort meshes by name, if specified
                if (_settings.SortByName)
                    tmpList = tmpList.OrderBy(m => m.Name, new CustomComparer<string>(NaturalComparer.Do)).ToList();

                foreach (var mesh in tmpList)
                    newModel.Meshes.Add(mesh);
            }

            if (_settings.ProcessAnimations && 
                scene.HasAnimations && scene.AnimationCount > 0)
            {
                // Find all mesh nodes to count against animation nodes
                var meshNameList = CollectMeshNodeNames(scene.RootNode);

                // Sort animations by name, if specified
                if (_settings.SortByName)
                    meshNameList = meshNameList.OrderBy(s => s, new CustomComparer<string>(NaturalComparer.Do)).ToList();

                // Determine amount of animation channels
                var maxChannelCount = scene.MeshCount; 

				// If amount of animation channels does not correspond to mesh count, try to guess valid channels
				// by comparing their names.
				if (maxChannelCount != meshNameList.Count)
				{
					maxChannelCount = scene.Animations
						.Max(a => a.NodeAnimationChannels.Where(c => meshNameList.Contains(c.NodeName)).Count());
				}

				// Loop through all animations and add appropriate ones.
				// Integrity check: there should be meshes and mesh count should be equal to unique mesh name count.
				if (maxChannelCount <= 0 || maxChannelCount != meshNameList.Count)
                    logger.Warn("Actual number of meshes doesn't correspond to mesh list. Animations won't be imported.");
                else
                {
                    for (int i = 0; i < scene.AnimationCount; i++)
                    {
                        var anim = scene.Animations[i];

                        // Integrity check: support only time-based node animations
                        if (!anim.HasNodeAnimations || anim.DurationInTicks <= 0)
                        {
                            logger.Warn("Anim " + i + " isn't a valid type of animation for TR formats.");
                            continue;
                        }

                        // Guess possible maximum frame and time
                        var frameCount = 0;
                        double maximumTime = 0;
                        foreach (var node in anim.NodeAnimationChannels)
                        {
                            if (node.HasPositionKeys)
                            {
                                var maxNodeTime = node.PositionKeys.Max(key => key.Time);
                                maximumTime = maximumTime >= maxNodeTime ? maximumTime : maxNodeTime;
                                frameCount = frameCount >= node.PositionKeyCount ? frameCount : node.PositionKeyCount;
                            }
                            if (node.HasRotationKeys)
                            {
                                var maxNodeTime = node.RotationKeys.Max(key => key.Time);
                                maximumTime = maximumTime >= maxNodeTime ? maximumTime : maxNodeTime;
                                frameCount = frameCount >= node.RotationKeyCount ? frameCount : node.RotationKeyCount;
                            }
                        }

                        // Calculate time multiplier
                        var timeMult = (double)(frameCount - 1) / anim.DurationInTicks;

                        // Integrity check: maximum frame time shouldn't excess duration
                        if (timeMult * maximumTime >= frameCount)
                        {
                            logger.Warn("Anim " + i + " has frames outside of time limits and won't be imported.");
                            continue;
                        }

                        IOAnimation ioAnim = new IOAnimation(string.IsNullOrEmpty(anim.Name) ? "Imported animation " + i : anim.Name,
                                                             maxChannelCount);

                        // Precreate frames and set them to identity
                        for (int j = 0; j < frameCount; j++)
                            ioAnim.Frames.Add(new IOFrame());

                        // Precreate rotations and set them to identity
                        // I am using generic foreach here instead of linq foreach because for some reason it
                        // returns wrong amount of angles during enumeration with Enumerable.Repeat.
                        foreach (var frame in ioAnim.Frames)
                        {
                            var angleList = Enumerable.Repeat(Vector3.Zero, maxChannelCount);
                            frame.Angles.AddRange(angleList);
                        }

                        // Search through all nodes and put data into corresponding frames.
                        // It's not clear what should we do in case if multiple nodes refer to same mesh, but sometimes
                        // it happens, e. g. in case of fbx format. In this case, we'll just add to existing values for now.

                        foreach (var chan in anim.NodeAnimationChannels)
                        {
                            // Look if this channel belongs to any mesh in list. 
                            // If so, attribute it to appropriate frame.
                            var chanIndex = meshNameList.IndexOf(item => chan.NodeName.Contains(item));

                            // Integrity check: no appropriate mesh found
                            if (chanIndex < 0)
                            {
                                logger.Warn("Anim " + i + " channel " + chan.NodeName + " has no corresponding mesh in meshtree and will be ignored");
                                continue;
                            }

                            // Apply translation only if found channel belongs to root mesh.
                            if (chanIndex == 0 && chan.HasPositionKeys && chan.PositionKeyCount > 0)
                                foreach (var key in chan.PositionKeys)
                                {
                                    // Integrity check: frame shouldn't fall out of keyframe array bounds.
                                    var frameIndex = (int)Math.Round(key.Time * timeMult, MidpointRounding.AwayFromZero);
                                    if (frameIndex >= frameCount)
                                    {
                                        logger.Warn("Anim " + i + " channel " + chan.NodeName + " has a key outside of time limits and will be ignored.");
                                        continue;
                                    }

                                    float rX = key.Value.X;
                                    float rY = key.Value.Y;
                                    float rZ = key.Value.Z;

                                    if (_settings.SwapXY) { var temp = rX; rX = rY; rY = temp; }
                                    if (_settings.SwapXZ) { var temp = rX; rX = rZ; rZ = temp; }
                                    if (_settings.SwapYZ) { var temp = rY; rY = rZ; rZ = temp; }

                                    if (_settings.FlipX) { rX = -rX; }
                                    if (_settings.FlipY) { rY = -rY; }
                                    if (_settings.FlipZ) { rZ = -rZ; }

                                    ioAnim.Frames[frameIndex].Offset += new Vector3(rX, rY, rZ);
                                }

                            if (chan.HasRotationKeys && chan.RotationKeyCount > 0)
                                foreach (var key in chan.RotationKeys)
                                {
                                    // Integrity check: frame shouldn't fall out of keyframe array bounds.
                                    var frameIndex = (int)Math.Round(key.Time * timeMult, MidpointRounding.AwayFromZero);
                                    if (frameIndex >= frameCount)
                                    {
                                        logger.Warn("Anim " + i + " channel " + chan.NodeName + " has a key outside of time limits and will be ignored.");
                                        continue;
                                    }

                                    // Convert quaternions back to rotations.
                                    // This is similar to TRViewer's conversion routine.

                                    var quatI = System.Numerics.Quaternion.Identity;
                                    var quat = new System.Numerics.Quaternion(key.Value.X,
                                                                              key.Value.Z,
                                                                              key.Value.Y,
                                                                             -key.Value.W);
                                    quatI *= quat;

                                    var eulers = MathC.QuaternionToEuler(quatI);
                                    var rotation = new Vector3(eulers.X * 180.0f / (float)Math.PI,
                                                               eulers.Y * 180.0f / (float)Math.PI,
                                                               eulers.Z * 180.0f / (float)Math.PI);

                                    ioAnim.Frames[frameIndex].Angles[chanIndex] += MathC.NormalizeAngle(rotation);
                                }
                        }

                        newModel.Animations.Add(ioAnim);
                    }
                }
            }

            return newModel;
        }
    }
}
