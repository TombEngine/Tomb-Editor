using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.Graphics
{
    public class AnimatedModel : Model<ObjectMesh, ObjectVertex>
    {
        private struct BoneSignature
        {
            public int Index;
            public int Depth;
            public int ParentIndex;
            public Vector3 Pivot;
            public List<int> ChildIndices;
        }

        public Vector3 Offset;
        public Bone Root { get; set; }
        public List<Animation> Animations { get; set; } = new List<Animation>();
        public List<Bone> Bones { get; set; } = new List<Bone>();
        public List<Matrix4x4> BindPoseTransforms { get; set; } = new List<Matrix4x4>();
        public List<Matrix4x4> AnimationTransforms { get; set; } = new List<Matrix4x4>();

        public AnimatedModel(GraphicsDevice device)
            : base(device, ModelType.Skinned)
        {
            UpdateBuffers();
        }

        public override void UpdateBuffers(Vector3? position = null)
        {
            foreach (var mesh in Meshes)
            {
                mesh.UpdateBoundingBox();
                mesh.UpdateBuffers(position);
            }
        }

        public void BuildHierarchy()
        {
            Root.GlobalTransform = Root.Transform;
            BindPoseTransforms[Root.Index] = Root.GlobalTransform;

            foreach (var node in Root.Children)
            {
                BuildHierarchy(node, Root.GlobalTransform);
            }
        }

        private void BuildHierarchy(Bone node, Matrix4x4 parentTransform)
        {
            node.GlobalTransform = node.Transform * parentTransform;
            BindPoseTransforms[node.Index] = node.GlobalTransform;

            foreach (var child in node.Children)
            {
                BuildHierarchy(child, node.GlobalTransform);
            }
        }

        public void UpdateAnimation(int animationIndex, int frameIndex)
        {
            if (animationIndex >= Animations.Count)
                return;
            var animation = Animations[animationIndex];
            int keyFrameIndex1 = frameIndex;
            int keyFrameIndex2 = keyFrameIndex1 + 1;
            if (keyFrameIndex1 >= animation.KeyFrames.Count || keyFrameIndex2 >= animation.KeyFrames.Count)
                return;
            BuildAnimationPose(animation.KeyFrames[keyFrameIndex1], animation.KeyFrames[keyFrameIndex2], 1);
        }

        public void BuildAnimationPose(KeyFrame frame)
        {
            var globalScale = Matrix4x4.CreateTranslation(Offset) * frame.TranslationsMatrices[0];
            AnimationTransforms[0] = Matrix4x4.CreateFromQuaternion(frame.Quaternions[0]) * globalScale;

            foreach (var node in Root.Children)
                BuildAnimationPose(node, AnimationTransforms[0], frame);
        }

        private void BuildAnimationPose(Bone node, Matrix4x4 parentTransform, KeyFrame frame)
        {
            AnimationTransforms[node.Index] = Matrix4x4.CreateFromQuaternion(frame.Quaternions[node.Index]) * node.Transform * parentTransform;

            foreach (Bone child in node.Children)
                BuildAnimationPose(child, AnimationTransforms[node.Index], frame);
        }

        public void BuildAnimationPose(KeyFrame frame1, KeyFrame frame2, float k)
        {
            Matrix4x4 translation = Matrix4x4.Lerp(frame1.TranslationsMatrices[0], frame2.TranslationsMatrices[0], k);
            Matrix4x4 rotation = Matrix4x4.CreateFromQuaternion(Quaternion.Slerp(frame1.Quaternions[0], frame2.Quaternions[0], k));

            var globalScale = Matrix4x4.CreateTranslation(Offset) * translation;
            AnimationTransforms[0] = rotation * globalScale;

            foreach (var node in Root.Children)
                BuildAnimationPose(node, AnimationTransforms[0], frame1, frame2, k);
        }

        private void BuildAnimationPose(Bone node, Matrix4x4 parentTransform, KeyFrame frame1, KeyFrame frame2, float k)
        {
            Matrix4x4 rotation = Matrix4x4.CreateFromQuaternion(Quaternion.Slerp(frame1.Quaternions[node.Index], frame2.Quaternions[node.Index], k));

            AnimationTransforms[node.Index] = rotation * node.Transform * parentTransform;

            foreach (Bone child in node.Children)
                BuildAnimationPose(child, AnimationTransforms[node.Index], frame1, frame2, k);
        }

        public static AnimatedModel FromWadMoveable(GraphicsDevice device, WadMoveable mov, Func<WadTexture, WadRenderer.AllocationResult> allocateTexture, bool correctTexture, bool loadAnimations)
        {
            AnimatedModel model = new AnimatedModel(device);
            List<WadBone> bones = mov.Bones;  

            // Create meshes
            for (int m = 0; m < bones.Count; m++)
                model.Meshes.Add(ObjectMesh.FromWad2(device, bones[m].Mesh, allocateTexture, correctTexture));

            if (mov.Skin != null)
                model.Skin = ObjectMesh.FromWad2(device, mov.Skin, allocateTexture, correctTexture);

            // HACK: Add matrices here because if original WAD stack was corrupted, we could have broken parent - children
            // relations and so we could have meshes count different from matrices count
            for (int j = 0; j < bones.Count; j++)
            {
                model.BindPoseTransforms.Add(Matrix4x4.Identity);
                model.AnimationTransforms.Add(Matrix4x4.Identity);
            }

            // Build the skeleton
            model.Root = BuildSkeleton(model, null, bones);

            if (loadAnimations)
            {
                // Prepare animations
                for (int j = 0; j < mov.Animations.Count; j++)
                    model.Animations.Add(Animation.FromWad2(bones, mov.Animations[j]));

                // Prepare data by loading the first valid animation and uploading data to the GPU
                model.BuildHierarchy();

                if (model.Animations.Count > 0 && model.Animations.Any(a => a.KeyFrames.Count > 0))
                    model.BuildAnimationPose(model.Animations.FirstOrDefault(a => a.KeyFrames.Count > 0)?.KeyFrames[0]);
            }
            else 
            {
                // We do not need whole animation, just load the first valid animation and first keyframe
                var anim = mov.Animations.FirstOrDefault(a => a.KeyFrames.Count > 0);
                if (anim is not null)
                {
                    var modelAnim = Animation.FromWad2(bones, anim);
                    if (modelAnim.KeyFrames.Count > 1)
                        modelAnim.KeyFrames.RemoveRange(1, modelAnim.KeyFrames.Count - 1);

                    model.Animations.Add(modelAnim);
                    model.BuildHierarchy();
                    model.BuildAnimationPose(modelAnim.KeyFrames[0]);
                }
            }
            
            model.UpdateBuffers();

            return model;
        }

        private static Bone BuildSkeleton(AnimatedModel model, Bone parentBone, List<WadBone> bones)
        {
            model.Bones = new List<Bone>();
            var stack = new Stack<Bone>();

            for (int i = 0; i < bones.Count; i++)
            {
                var nb = new Bone();
                nb.Name = bones[i].Name;
                nb.Pivot = bones[i].Translation;
                nb.Index = i;
                model.Bones.Add(nb);
            }

            var currentBone = model.Bones[0];
            currentBone.Transform = Matrix4x4.Identity;
            currentBone.GlobalTransform = Matrix4x4.Identity;

            for (int j = 1; j < model.Bones.Count; j++)
            {
                int linkX = (int)bones[j].Translation.X;
                int linkY = (int)bones[j].Translation.Y;
                int linkZ = (int)bones[j].Translation.Z;

                switch (bones[j].OpCode)
                {
                    case WadLinkOpcode.NotUseStack:
                        model.Bones[j].Transform = Matrix4x4.CreateTranslation(linkX, linkY, linkZ);
                        model.Bones[j].Parent = currentBone;
                        currentBone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];

                        break;
                    case WadLinkOpcode.Pop:
                        if (stack.Count <= 0)
                            continue;
                        currentBone = stack.Pop();

                        model.Bones[j].Transform = Matrix4x4.CreateTranslation(linkX, linkY, linkZ);
                        model.Bones[j].Parent = currentBone;
                        currentBone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];

                        break;
                    case WadLinkOpcode.Push:
                        stack.Push(currentBone);

                        model.Bones[j].Transform = Matrix4x4.CreateTranslation(linkX, linkY, linkZ);
                        model.Bones[j].Parent = currentBone;
                        currentBone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];

                        break;
                    case WadLinkOpcode.Read:
                        if (stack.Count <= 0)
                            continue;
                        var bone = stack.Pop();
                        model.Bones[j].Transform = Matrix4x4.CreateTranslation(linkX, linkY, linkZ);
                        model.Bones[j].Parent = bone;
                        bone.Children.Add(model.Bones[j]);
                        currentBone = model.Bones[j];
                        stack.Push(bone);

                        break;
                }
            }
            
            return model.Bones[0];
        }

        public void RenderSkin(GraphicsDevice device, Effect effect, SharpDX.Matrix world, AnimatedModel animSource = null)
        {
            if (Skin == null)
                return;

            device.SetVertexBuffer(0, Skin.VertexBuffer);
            device.SetIndexBuffer(Skin.IndexBuffer, true);
            device.SetVertexInputLayout(VertexInputLayout.FromBuffer(0, Skin.VertexBuffer));

            var model = animSource == null ? this : animSource;

            var dxMatrices = model.AnimationTransforms.Select(m =>
            {
                if (!Matrix4x4.Invert(model.BindPoseTransforms[model.AnimationTransforms.IndexOf(m)], out Matrix4x4 invBindPose))
                    return Matrix4x4.Identity;

                return Matrix4x4.Transpose(invBindPose * m);
            }).ToArray();

            effect.Parameters["Skinned"].SetValue(true);
            effect.Parameters["Bones"].SetValue(dxMatrices);

            effect.Parameters["ModelViewProjection"].SetValue(world);
            effect.Techniques[0].Passes[0].Apply();

            foreach (var submesh in Skin.Submeshes)
            {
                if (submesh.Value.Material.AdditiveBlending)
                    device.SetBlendState(device.BlendStates.Additive);
                else
                    device.SetBlendState(device.BlendStates.Opaque);

                if (submesh.Value.Material.DoubleSided)
                    device.SetRasterizerState(device.RasterizerStates.CullNone);
                else
                    device.SetRasterizerState(device.RasterizerStates.CullBack);

                device.DrawIndexed(PrimitiveType.TriangleList, submesh.Value.NumIndices, submesh.Value.BaseIndex);
            }

            effect.Parameters["Skinned"].SetValue(false);
        }

        public List<int[]> GetBonePairs(bool flipZ = false, int symmetryMargin = 16)
        {
            var sigList = new List<BoneSignature>();
            sigList = CollectBoneSignatures(Bones[0], 0, -1, sigList).OrderBy(item => item.Index).ToList();
            var result = new List<int[]>();

            // Traverse through collected signatures in 2 passes
            for (int i = 0; i <= symmetryMargin; i++)
            {
                foreach (var sig in sigList)
                {
                    // Entry already was paired, bypass it
                    if (result.Any(pair => (pair[0] != -1 && pair[1] == sig.Index) || (pair[1] != -1 && pair[0] == sig.Index)))
                        continue;

                    // Find out potentially matching bone signatures
                    var others = sigList.Where(item =>
                        sig.Index != item.Index &&
                        sig.ChildIndices.Count == item.ChildIndices.Count && // Child count should always match!
                        sig.Depth == item.Depth && // Own depth should always be the same!
                        sigList[sig.ParentIndex].Depth == sigList[item.ParentIndex].Depth).ToList(); // Parent depth should always be the same!

                    bool boneAdded = false;
                    foreach (var other in others)
                    {
                        float compX1 = !flipZ ? sig.Pivot.X   :   sig.Pivot.Z;
                        float compX2 = !flipZ ? other.Pivot.X : other.Pivot.Z;
                        float compZ1 = !flipZ ? sig.Pivot.Z   :   sig.Pivot.X;
                        float compZ2 = !flipZ ? other.Pivot.Z : other.Pivot.X;

                        if ((i != symmetryMargin &&

                            // Prioritize bones with same parent index and pivot symmetry.
                            // Pivot symmetry is declared if X pivot position is mirrored for both bones
                            // and bones are well apart on Z axis (to correctly identify models like scorpion or crocodile).
                            // Unfortunately, there's a case when both bones are symmetrical but are pivoted
                            // from single point (e.g. DEMIGOD3 spear), in this case we can't predict symmetry correctly,
                            // so such cases are bypassed.

                            sig.ParentIndex == other.ParentIndex &&
                            Math.Abs(Math.Abs(compX1) - Math.Abs(compX2)) <= i &&
                            Math.Abs(compX1 - compX2) >= i &&
                            MathC.WithinEpsilon(compZ1, compZ2, symmetryMargin)) ||

                            // On last pass, prioritize bones with already found matching parent mesh pairs.

                            (i == symmetryMargin && 
                            ((result.Any(pair => (pair[0] == sig.ParentIndex && pair[1] == other.ParentIndex) || 
                                                 (pair[0] == other.ParentIndex && pair[1] == sig.ParentIndex)))) ||
                                                
                            // Additionally do final exact comparison of Y/Z values and X distance (fixes TR3 Shiva without breaking anything else)

                            (MathC.WithinEpsilon(compZ1, compZ2, symmetryMargin) && 
                             MathC.WithinEpsilon(sig.Pivot.Y, other.Pivot.Y, symmetryMargin) && 
                             Math.Abs(compX1 - compX2) >= symmetryMargin)))
                        {
                            int p0 = result.IndexOf(pair => pair[0] == sig.Index);
                            int p1 = result.IndexOf(pair => pair[0] == other.Index);

                            if (p0 == -1 && p1 == -1)
                                result.Add(new int[2] { sig.Index, other.Index });  // No entry in pair list
                            else if (p0 == -1)
                                result[p1] = new int[2] { sig.Index, other.Index }; // Entry was already in list
                            else if (p1 == -1)
                                result[p0] = new int[2] { sig.Index, other.Index }; // Found match was already in list

                            boneAdded = true;
                            break;
                        }
                    }

                    if (!boneAdded && i == symmetryMargin)
                        result.Add(new int[2] { sig.Index, -1 }); // Entry isn't found, create new one on last pass
                }
            }

            // PARANOIA: try to flter out stray decoupled pairs
            result = result.Where(item => !(item[1] == -1 && 
                     result.Any(item2 => (item2[0] == item[0] || item2[1] == item[0]) && item2[1] != -1))).ToList();

            return result;
        }

        private List<BoneSignature> CollectBoneSignatures(Bone bone, int depth, int parentIndex, List<BoneSignature> list)
        {
            var childIndices = new List<int>();

            foreach (var child in bone.Children)
            {
                // Recursively scan through all bones
                CollectBoneSignatures(child, depth + 1, Bones.IndexOf(bone), list);

                // Add child to own bone signature
                if (Bones.IndexOf(child) != parentIndex)
                    childIndices.Add(Bones.IndexOf(child));
            }

            list.Add(new BoneSignature() { Index = Bones.IndexOf(bone), Depth = depth, ParentIndex = parentIndex, Pivot = bone.Pivot, ChildIndices = childIndices });
            return list;
        }
    }
}
