using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    public enum AnimTransformMode
    {
        Simple,         // Brutally overwrite all properties for all frames
        Smooth,         // Smootherstep interpolation
        SmoothReverse,
        Linear,         // Do simple linear interpolation, a-la WadMerger
        LinearReverse,
        Symmetric,      // Nifty symmetric smoothstep interpolation
        SymmetricLinear // Linear symmetric interpolation
    }

    public class AnimationEditor
    {
        public List<AnimationNode> Animations;
        public AnimationNode CurrentAnim;
        public int CurrentFrameIndex;
        public VectorInt2 Selection;

        public WadToolClass Tool { get; private set; }
        public Wad2 Wad { get; private set; }
        public WadMoveable Moveable { get; private set; }

        // Clipboard
        public List<KeyFrame> ClipboardKeyFrames = new List<KeyFrame>();
        public AnimationNode ClipboardNode = null;

        // General state
        public bool Saved;
        public bool MadeChanges = false;

        // Editing state
        public AnimTransformMode TransformMode;

        // Internal state, used for transform functions
        private List<Vector3> _backupRot = new List<Vector3>();
        private List<Quaternion> _backupQuat = new List<Quaternion>();
        private List<Vector3> _backupPos = new List<Vector3>();
        private Vector3 _initialPos;
        private Vector3 _initialRot;
        private Quaternion _initialQuat;

        // Helpers
        public bool SelectionIsEmpty => Selection.X == -1 || Selection.Y == -1;
        public bool ValidAnimationAndFrames => CurrentAnim != null && CurrentAnim.DirectXAnimation.KeyFrames.Count > 0;

        public int GetRealNumberOfFrames(int animNumber = -1)
        {
            if (animNumber < 0)
            {
                if (CurrentAnim == null) return 0;
                animNumber = CurrentAnim.Index;
            }

            if (Animations.Count <= animNumber)
                return -1;
            else
                return Animations[animNumber].WadAnimation.GetRealNumberOfFrames(Animations[animNumber].DirectXAnimation.KeyFrames.Count);
        }

        public int GetRealFrameNumber(int keyFrameNumber = -1, int animNumber = -1)
        {
            if (CurrentAnim == null) return 0;

            if (animNumber < 0) animNumber = CurrentAnim.Index;
            if (keyFrameNumber < 0) keyFrameNumber = CurrentFrameIndex;

            if (Animations.Count <= animNumber)
                return -1;
            else
                return Animations[animNumber].WadAnimation.FrameRate * keyFrameNumber;
        }

        public KeyFrame CurrentKeyFrame
        {
            get
            {
                if (CurrentAnim == null || CurrentAnim.DirectXAnimation.KeyFrames.Count <= 0)
                    return null;

                if (CurrentFrameIndex >= CurrentAnim.DirectXAnimation.KeyFrames.Count)
                    CurrentFrameIndex = 0;
                return CurrentAnim.DirectXAnimation.KeyFrames[CurrentFrameIndex];
            }
        }

        public List<KeyFrame> ActiveFrames
        {
            get
            {
                if (SelectionIsEmpty)
                    return new List<KeyFrame>() { CurrentKeyFrame };
                else
                    return CurrentAnim.DirectXAnimation.KeyFrames.GetRange(Selection.X, Selection.Y - Selection.X + 1);
            }
        }

        public AnimationEditor(WadToolClass tool, Wad2 wad, WadMoveableId idToEdit)
        {
            Tool = tool;
            Wad = wad;
            Moveable = Wad.Moveables.ContainsKey(idToEdit) ? Wad.Moveables[idToEdit] : null;

            // NOTE: we work with a pair WadAnimation - Animation. All changes to animation data like name,
            // framerate, next animation, state changes... will be saved directly to WadAnimation.
            // All changes to keyframes will be instead stored directly in the renderer's Animation class.
            // While saving, WadAnimation and Animation will be combined and original animations will be overwritten.

            Animations = new List<AnimationNode>();
            for (int i = 0; i < Moveable.Animations.Count; i++)
            {
                var animation = Moveable.Animations[i].Clone(); ;
                Animations.Add(new AnimationNode(animation, Animation.FromWad2(Moveable.Bones, animation), i));
            }
        }

        public WadAnimation GetSavedAnimation(AnimationNode animation)
        {
            var wadAnim = animation.WadAnimation.Clone();
            var directxAnim = animation.DirectXAnimation;

            // I need only to convert DX keyframes to Wad2 keyframes
            wadAnim.KeyFrames.Clear();
            foreach (var directxKeyframe in directxAnim.KeyFrames)
            {
                var keyframe = new WadKeyFrame();

                // Create the new bounding box
                keyframe.BoundingBox = new TombLib.BoundingBox(directxKeyframe.BoundingBox.Minimum,
                                                               directxKeyframe.BoundingBox.Maximum);

                // For now we take the first translation as offset
                keyframe.Offset = new System.Numerics.Vector3(directxKeyframe.Translations[0].X,
                                                              directxKeyframe.Translations[0].Y,
                                                              directxKeyframe.Translations[0].Z);

                // Convert angles from radians to degrees and save them
                foreach (var rot in directxKeyframe.Rotations)
                {
                    var angle = new WadKeyFrameRotation();
                    angle.Rotations = new System.Numerics.Vector3(rot.X * 180.0f / (float)Math.PI,
                                                                  rot.Y * 180.0f / (float)Math.PI,
                                                                  rot.Z * 180.0f / (float)Math.PI);
                    keyframe.Angles.Add(angle);
                }

                wadAnim.KeyFrames.Add(keyframe);
            }

            return wadAnim;
        }

        public bool CheckAnimationIntegrity(WadAnimation anim)
        {
            if (anim == null)
                return true;

            if (anim.StateChanges.Any(sc => sc.Dispatches.Any(d => d.NextAnimation >= Animations.Count)))
            {
                Tool.SendMessage("Some state changes refer to non-existent animations.", TombLib.Forms.PopupType.Warning);
                return false;
            }
            else if (anim.NextAnimation >= Animations.Count)
            {
                Tool.SendMessage("Next animation field refers to non-existent animation.", TombLib.Forms.PopupType.Warning);
                return false;
            }
            else if (anim.NextFrame > Animations[anim.NextAnimation].WadAnimation.GetRealNumberOfFrames())
            {
                Tool.SendMessage("Next frame field refers to non-existent frame.", TombLib.Forms.PopupType.Warning);
                return false;
            }

            return true;
        }

        public bool SaveChanges(IWin32Window owner = null)  // No owner - no confirmation!
        {
            if (owner != null && DarkMessageBox.Show(owner, "Do you really want to save changes to animations?", "Save changes",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return false;

            // Clear the old animations
            Moveable.Animations.Clear();

            // Combine WadAnimation and Animation classes
            foreach (var animation in Animations)
                Moveable.Animations.Add(GetSavedAnimation(animation));

            Moveable.Version = DataVersion.GetNext();
            Tool.ToggleUnsavedChanges();
            return true;
        }

        public void UpdateTransform(int meshIndex, Vector3 newRot, Vector3 newPos)
        {
            if (CurrentAnim == null || CurrentKeyFrame == null) return;

            // Backup everything and push undo on first occurence of editing
            if (!MadeChanges || _backupPos.Count == 0 || _backupRot.Count == 0)
            {
                _initialPos = CurrentKeyFrame.Translations[0];
                _initialRot = CurrentKeyFrame.Rotations[meshIndex];
                _initialQuat = CurrentKeyFrame.Quaternions[meshIndex];

                _backupPos.Clear();
                _backupRot.Clear();
                _backupQuat.Clear();

                ActiveFrames.ForEach(f => {
                    _backupRot.Add(f.Rotations[meshIndex]);
                    _backupQuat.Add(f.Quaternions[meshIndex]);
                    _backupPos.Add(f.Translations[0]);
                });

                Tool.UndoManager.PushAnimationChanged(this, CurrentAnim);
                MadeChanges = true;
            }

            // Calculate deltas for other frames processing
            var deltaPos = newPos - _initialPos;
            var deltaRot = newRot - _initialRot;

            // Define animation properties
            bool evolve  = TransformMode != AnimTransformMode.Simple && ActiveFrames.Count > 1;
            bool smooth  = TransformMode == AnimTransformMode.Smooth || TransformMode == AnimTransformMode.SmoothReverse || TransformMode == AnimTransformMode.Symmetric;
            bool reverse = TransformMode == AnimTransformMode.LinearReverse || TransformMode == AnimTransformMode.SmoothReverse;
            bool loop    = TransformMode == AnimTransformMode.Symmetric || TransformMode == AnimTransformMode.SymmetricLinear;

            // Calculate evolution
            float frameCount = loop || reverse ? Selection.Y - Selection.X : CurrentFrameIndex - Selection.X;
            float currentStep = 0;

            int index = 0;
            foreach (var keyframe in ActiveFrames)
            {
                float midFrame = loop ? CurrentFrameIndex - Selection.X : (reverse ? CurrentFrameIndex : frameCount);
                float bias = (currentStep <= midFrame) ? (reverse ? 1.0f : currentStep / midFrame) : (frameCount - currentStep) / (frameCount - midFrame);

                // Single-pass smoothstep doesn't look organic on fast animations, hence we're using 2-pass smootherstep here.
                float weight = smooth ? (float)MathC.SmoothStep(0, 1, MathC.SmoothStep(0, 1, bias)) : bias;

                // Apply deltas to backed-up transforms
                var currPos = _backupPos[index] + deltaPos;
                var currRot = _backupRot[index] + deltaRot;

                var translationVector = currPos;

                if (evolve)
                    translationVector = Vector3.Lerp(_backupPos[index], translationVector, weight);

                // Foolproof stuff in case user hardly messes with transform during playback...
                if (float.IsNaN(translationVector.X)) translationVector.X = _backupPos[index].X;
                if (float.IsNaN(translationVector.Y)) translationVector.Y = _backupPos[index].Y;
                if (float.IsNaN(translationVector.Z)) translationVector.Z = _backupPos[index].Z;

                keyframe.Translations[0] = translationVector;
                keyframe.TranslationsMatrices[0] = Matrix4x4.CreateTranslation(translationVector);

                var rotVector = currRot;
                Quaternion finalQuat;
                if (evolve)
                {
                    // Calculate source and destination quats and decide on direction based on dot product.
                    var srcQuat = Quaternion.CreateFromYawPitchRoll(_backupRot[index].Y, _backupRot[index].X, _backupRot[index].Z);
                    var destQuat = Quaternion.CreateFromYawPitchRoll(currRot.Y, currRot.X, currRot.Z);
                    if (Quaternion.Dot(srcQuat, destQuat) < 0) srcQuat = Quaternion.Negate(srcQuat);
                    finalQuat = Quaternion.Lerp(srcQuat, destQuat, weight);
                }
                else
                    finalQuat = Quaternion.CreateFromYawPitchRoll(rotVector.Y, rotVector.X, rotVector.Z);

                // Derive Euler angles for display/UI
                var rotationVector = MathC.QuaternionToEuler(finalQuat);

                // Foolproof stuff in case user hardly messes with transform during playback...
                bool hasNaN = float.IsNaN(rotationVector.X) || float.IsNaN(rotationVector.Y) || float.IsNaN(rotationVector.Z);
                if (float.IsNaN(rotationVector.X)) rotationVector.X = _backupRot[index].X;
                if (float.IsNaN(rotationVector.Y)) rotationVector.Y = _backupRot[index].Y;
                if (float.IsNaN(rotationVector.Z)) rotationVector.Z = _backupRot[index].Z;

                // Store quaternion directly to avoid precision loss from Euler round-trip.
                // Only reconstruct from Euler if NaN filtering was needed.
                keyframe.Quaternions[meshIndex] = hasNaN
                    ? Quaternion.CreateFromYawPitchRoll(rotationVector.Y, rotationVector.X, rotationVector.Z)
                    : finalQuat;
                keyframe.Rotations[meshIndex] = rotationVector;

                index++;
                currentStep++;

                if (currentStep > frameCount)
                    currentStep = frameCount;
            }
        }

        /// <summary>
        /// Quaternion-based transform update that avoids the Euler angle round-trip
        /// which causes gimbal lock jitter during interactive gizmo manipulation.
        /// </summary>
        public void UpdateTransformQuaternion(int meshIndex, Quaternion targetQuat, Vector3 newPos)
        {
            if (CurrentAnim == null || CurrentKeyFrame == null) return;

            // Backup everything and push undo on first occurrence of editing
            if (!MadeChanges || _backupPos.Count == 0 || _backupQuat.Count == 0)
            {
                _initialPos = CurrentKeyFrame.Translations[0];
                _initialRot = CurrentKeyFrame.Rotations[meshIndex];
                _initialQuat = CurrentKeyFrame.Quaternions[meshIndex];

                _backupPos.Clear();
                _backupRot.Clear();
                _backupQuat.Clear();

                ActiveFrames.ForEach(f => {
                    _backupRot.Add(f.Rotations[meshIndex]);
                    _backupQuat.Add(f.Quaternions[meshIndex]);
                    _backupPos.Add(f.Translations[0]);
                });

                Tool.UndoManager.PushAnimationChanged(this, CurrentAnim);
                MadeChanges = true;
            }

            // Calculate deltas
            var deltaPos = newPos - _initialPos;
            // Quaternion delta from initial to target, applied in local bone space
            var deltaQuat = Quaternion.Inverse(_initialQuat) * targetQuat;

            // Define animation properties
            bool evolve  = TransformMode != AnimTransformMode.Simple && ActiveFrames.Count > 1;
            bool smooth  = TransformMode == AnimTransformMode.Smooth || TransformMode == AnimTransformMode.SmoothReverse || TransformMode == AnimTransformMode.Symmetric;
            bool reverse = TransformMode == AnimTransformMode.LinearReverse || TransformMode == AnimTransformMode.SmoothReverse;
            bool loop    = TransformMode == AnimTransformMode.Symmetric || TransformMode == AnimTransformMode.SymmetricLinear;

            // Calculate evolution
            float frameCount = loop || reverse ? Selection.Y - Selection.X : CurrentFrameIndex - Selection.X;
            float currentStep = 0;

            int index = 0;
            foreach (var keyframe in ActiveFrames)
            {
                float midFrame = loop ? CurrentFrameIndex - Selection.X : (reverse ? CurrentFrameIndex : frameCount);
                float bias = (currentStep <= midFrame) ? (reverse ? 1.0f : currentStep / midFrame) : (frameCount - currentStep) / (frameCount - midFrame);

                // Single-pass smoothstep doesn't look organic on fast animations, hence we're using 2-pass smootherstep here.
                float weight = smooth ? (float)MathC.SmoothStep(0, 1, MathC.SmoothStep(0, 1, bias)) : bias;

                // Apply position delta
                var translationVector = _backupPos[index] + deltaPos;
                if (evolve)
                    translationVector = Vector3.Lerp(_backupPos[index], translationVector, weight);

                if (float.IsNaN(translationVector.X)) translationVector.X = _backupPos[index].X;
                if (float.IsNaN(translationVector.Y)) translationVector.Y = _backupPos[index].Y;
                if (float.IsNaN(translationVector.Z)) translationVector.Z = _backupPos[index].Z;

                keyframe.Translations[0] = translationVector;
                keyframe.TranslationsMatrices[0] = Matrix4x4.CreateTranslation(translationVector);

                // Apply rotation delta entirely in quaternion space
                Quaternion finalQuat;
                if (evolve)
                {
                    var destQuat = _backupQuat[index] * deltaQuat;
                    if (Quaternion.Dot(_backupQuat[index], destQuat) < 0)
                        destQuat = Quaternion.Negate(destQuat);
                    finalQuat = Quaternion.Slerp(_backupQuat[index], destQuat, weight);
                }
                else
                {
                    finalQuat = _backupQuat[index] * deltaQuat;
                }

                // Normalize to prevent floating-point drift
                finalQuat = Quaternion.Normalize(finalQuat);

                // Derive Euler angles for display/UI only
                var rotationVector = MathC.QuaternionToEuler(finalQuat);
                if (float.IsNaN(rotationVector.X)) rotationVector.X = _backupRot[index].X;
                if (float.IsNaN(rotationVector.Y)) rotationVector.Y = _backupRot[index].Y;
                if (float.IsNaN(rotationVector.Z)) rotationVector.Z = _backupRot[index].Z;

                // Store quaternion directly — no Euler round-trip
                keyframe.Quaternions[meshIndex] = finalQuat;
                keyframe.Rotations[meshIndex] = rotationVector;

                index++;
                currentStep++; if (currentStep > frameCount) currentStep = frameCount;
            }
        }

        public void ReplaceAnimCommands(WadAnimCommand oldCommand, WadAnimCommand newCommand) => 
            Animations.ForEach(a => a.WadAnimation.AnimCommands.ForEach(ac => { if (ac == oldCommand) ac = newCommand.Clone(); }));
    }
}
