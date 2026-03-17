using System;
using System.Numerics;
using TombLib.Graphics;
using TombLib.Types;

namespace WadTool
{
    // Encapsulates all state and helpers for the animation blending preview.

    public class AnimBlendPreviewState
    {
        public bool IsActive { get; private set; }

        private int _frameNumber;
        private int _frameCount;
        private BezierCurve2 _curve;
        private KeyFrame _sourceFrame;

        // Pending blend params set from an incoming state change or next-anim transition.
        private int _pendingFrameCount;
        private BezierCurve2 _pendingCurve;

        public void SetPendingBlend(int frameCount, BezierCurve2 curve)
        {
            _pendingFrameCount = frameCount;
            _pendingCurve = curve;
        }

        public void ClearPendingBlend()
        {
            _pendingFrameCount = 0;
            _pendingCurve = null;
        }

        // Captures the source frame from the outgoing animation and starts blending.
        // Uses pending blend params if set; falls back to those on the outgoing animation.
        // Clears the pending params afterwards.
        public bool TryBegin(AnimationNode outgoingAnim, int frameCount, bool smoothAnimation)
        {
            int blendFrameCount = _pendingFrameCount > 0 ? _pendingFrameCount : outgoingAnim.WadAnimation.BlendFrameCount;
            var blendCurve = _pendingFrameCount > 0 ? _pendingCurve : outgoingAnim.WadAnimation.BlendCurve;

            ClearPendingBlend();

            if (blendFrameCount <= 0)
                return false;

            var sourceFrame = CaptureCurrentFrame(outgoingAnim, frameCount, smoothAnimation);
            if (sourceFrame == null)
                return false;

            _sourceFrame = sourceFrame;
            _frameCount = blendFrameCount;
            _curve = blendCurve ?? BezierCurve2.Linear;
            _frameNumber = 0;
            IsActive = true;
            return true;
        }

        public void Clear()
        {
            IsActive = false;
            _frameNumber = 0;
            _frameCount = 0;
            _sourceFrame = null;
            _curve = null;
            ClearPendingBlend();
        }

        // Build the blended pose for the current tick, sampling the target from the given animation.
        public void BuildPose(AnimatedModel model, AnimationNode targetAnim, int frameCount, bool smoothAnimation)
        {
            int frameRate = targetAnim.WadAnimation.FrameRate;
            if (frameRate == 0)
                frameRate = 1;

            int keyFrameIndex = frameCount / frameRate;
            int maxKfIndex = targetAnim.DirectXAnimation.KeyFrames.Count - 1;
            keyFrameIndex = Math.Min(keyFrameIndex, maxKfIndex);

            var targetFrame = targetAnim.DirectXAnimation.KeyFrames[keyFrameIndex];

            if (smoothAnimation && keyFrameIndex < maxKfIndex)
            {
                float targetK = ((float)frameCount / (float)frameRate) - (float)keyFrameIndex;
                targetK = Math.Min(targetK, 1.0f);

                var nextFrame = targetAnim.DirectXAnimation.KeyFrames[keyFrameIndex + 1];
                BuildBlendedPose(model, targetFrame, nextFrame, targetK);
            }
            else
            {
                BuildBlendedPose(model, targetFrame);
            }
        }

        // Advance one frame. Returns false when blending is complete.
        public bool Advance()
        {
            _frameNumber++;
            if (_frameNumber >= _frameCount)
            {
                Clear();
                return false;
            }

            return true;
        }

        // Get the current blend alpha (0 = fully source, 1 = fully target).
        private float GetAlpha()
        {
            if (_frameCount == 0)
                return 0.0f;

            float curveX = (float)_frameNumber / (float)_frameCount;
            return _curve.GetY(curveX);
        }

        private void BuildBlendedPose(AnimatedModel model, KeyFrame targetFrame)
        {
            model.BuildAnimationPose(_sourceFrame, targetFrame, GetAlpha());
        }

        private void BuildBlendedPose(AnimatedModel model, KeyFrame targetFrame1, KeyFrame targetFrame2, float targetK)
        {
            if (targetK <= 0 || targetFrame1 == targetFrame2)
            {
                model.BuildAnimationPose(_sourceFrame, targetFrame1, GetAlpha());
                return;
            }

            model.BuildAnimationPose(_sourceFrame, InterpolateKeyFrames(targetFrame1, targetFrame2, targetK), GetAlpha());
        }

        // Capture the current animation frame, optionally accounting for smooth interpolation.
        public static KeyFrame CaptureCurrentFrame(AnimationNode currentAnim, int frameCount, bool smoothAnimation)
        {
            if (currentAnim == null || currentAnim.DirectXAnimation.KeyFrames.Count == 0)
                return null;

            int frameRate = currentAnim.WadAnimation.FrameRate;
            if (frameRate == 0)
                frameRate = 1;

            int realFrame = Math.Max(0, frameCount - 1);
            int keyFrameIndex = realFrame / frameRate;
            int maxKfIndex = currentAnim.DirectXAnimation.KeyFrames.Count - 1;
            keyFrameIndex = Math.Min(keyFrameIndex, maxKfIndex);

            if (smoothAnimation && keyFrameIndex < maxKfIndex)
            {
                float k = (float)realFrame / (float)frameRate - (float)keyFrameIndex;
                k = Math.Min(k, 1.0f);

                return InterpolateKeyFrames(
                    currentAnim.DirectXAnimation.KeyFrames[keyFrameIndex],
                    currentAnim.DirectXAnimation.KeyFrames[keyFrameIndex + 1], k);
            }

            return currentAnim.DirectXAnimation.KeyFrames[keyFrameIndex].Clone();
        }

        // Produce a new KeyFrame by interpolating two source frames.
        public static KeyFrame InterpolateKeyFrames(KeyFrame frame1, KeyFrame frame2, float k)
        {
            var result = new KeyFrame();
            int boneCount = Math.Min(frame1.Quaternions.Count, frame2.Quaternions.Count);

            for (int i = 0; i < boneCount; i++)
            {
                result.Quaternions.Add(Quaternion.Slerp(frame1.Quaternions[i], frame2.Quaternions[i], k));
                result.Rotations.Add(Vector3.Lerp(frame1.Rotations[i], frame2.Rotations[i], k));

                var trans = Vector3.Lerp(frame1.Translations[i], frame2.Translations[i], k);
                result.Translations.Add(trans);
                result.TranslationsMatrices.Add(Matrix4x4.CreateTranslation(trans));
            }

            return result;
        }
    }
}
