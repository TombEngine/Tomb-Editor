#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers
{
    internal static class AnimatedTextureLookupUtility
    {
        private static float NormalizeLookupCoordinate(float value, float margin)
            => (float)(Math.Round(value / margin) * margin);

        /// <summary>
        /// Quantizes a rectangle to the lookup margin so equivalent UV bounds collapse to a stable cache key.
        /// </summary>
        /// <param name="rect">The rectangle to normalize.</param>
        /// <param name="margin">The quantization step used by animated texture lookup comparisons.</param>
        /// <returns>A rectangle snapped to the lookup grid defined by <paramref name="margin"/>.</returns>
        public static Rectangle2 NormalizeLookupRectangle(Rectangle2 rect, float margin) => new(
            NormalizeLookupCoordinate(rect.Start.X, margin),
            NormalizeLookupCoordinate(rect.Start.Y, margin),
            NormalizeLookupCoordinate(rect.End.X, margin),
            NormalizeLookupCoordinate(rect.End.Y, margin)
        );

        /// <summary>
        /// Determines whether two textures should be treated as the same logical texture for animated lookup purposes.
        /// </summary>
        /// <param name="first">The first texture to compare.</param>
        /// <param name="second">The second texture to compare.</param>
        /// <returns><see langword="true"/> when both textures resolve to the same identity; otherwise <see langword="false"/>.</returns>
        public static bool AreEquivalentTextures(Texture first, Texture second)
        {
            if (ReferenceEquals(first, second))
                return true;

            if (!string.IsNullOrEmpty(first.AbsolutePath) && !string.IsNullOrEmpty(second.AbsolutePath))
                return first.AbsolutePath.Equals(second.AbsolutePath, StringComparison.OrdinalIgnoreCase);

            if (first is TextureHashed firstHashed)
                return second is TextureHashed secondHashed && firstHashed.Hash == secondHashed.Hash;

            if (second is TextureHashed)
                return false;

            return first.Equals(second);
        }

        /// <summary>
        /// Builds a stable hash for a texture identity using the same precedence as <see cref="AreEquivalentTextures"/>.
        /// </summary>
        /// <param name="texture">The texture whose identity hash should be computed.</param>
        /// <returns>A hash code suitable for deduplication keys.</returns>
        public static int GetTextureIdentityHash(Texture texture)
        {
            if (!string.IsNullOrEmpty(texture.AbsolutePath))
                return StringComparer.OrdinalIgnoreCase.GetHashCode(texture.AbsolutePath);

            if (texture is TextureHashed hashed)
                return hashed.Hash.GetHashCode();

            return texture.GetHashCode();
        }

        /// <summary>
        /// Checks whether two rectangles match within the specified per-edge tolerance.
        /// </summary>
        /// <param name="first">The first rectangle.</param>
        /// <param name="second">The second rectangle.</param>
        /// <param name="margin">The allowed epsilon for each rectangle edge.</param>
        /// <returns><see langword="true"/> when all corresponding edges are within <paramref name="margin"/>.</returns>
        public static bool RectanglesMatch(Rectangle2 first, Rectangle2 second, float margin)
            => MathC.WithinEpsilon(first.X0, second.X0, margin) &&
               MathC.WithinEpsilon(first.Y0, second.Y0, margin) &&
               MathC.WithinEpsilon(first.X1, second.X1, margin) &&
               MathC.WithinEpsilon(first.Y1, second.Y1, margin);

        private static float GetRectangleMatchScore(Rectangle2 first, Rectangle2 second)
            => Math.Abs(first.X0 - second.X0) +
               Math.Abs(first.Y0 - second.Y0) +
               Math.Abs(first.X1 - second.X1) +
               Math.Abs(first.Y1 - second.Y1);

        /// <summary>
        /// Finds the closest frame in an animated set whose texture identity and bounds match the requested parent area.
        /// </summary>
        /// <param name="set">The animated texture set to scan.</param>
        /// <param name="texture">The texture area whose source frame is being resolved.</param>
        /// <param name="parentRect">The full parent rectangle that should match one frame in the set.</param>
        /// <param name="margin">The matching tolerance for rectangle comparison.</param>
        /// <returns>The best matching frame, or <see langword="null"/> when no acceptable match exists.</returns>
        public static AnimatedTextureFrame? FindBestMatchingAnimatedFrame(AnimatedTextureSet set, TextureArea texture, Rectangle2 parentRect, float margin)
        {
            AnimatedTextureFrame? bestFrame = null;
            float bestScore = float.MaxValue;

            foreach (var frame in set.Frames)
            {
                if (!AreEquivalentTextures(frame.Texture, texture.Texture))
                    continue;

                var frameRect = Rectangle2.FromCoordinates(frame.TexCoord0, frame.TexCoord1, frame.TexCoord2, frame.TexCoord3);

                if (!RectanglesMatch(frameRect, parentRect, margin))
                    continue;

                var score = GetRectangleMatchScore(frameRect, parentRect);

                if (score >= bestScore)
                    continue;

                bestScore = score;
                bestFrame = frame;

                if (score == 0.0f)
                    break;
            }

            return bestFrame;
        }

        /// <summary>
        /// Rebuilds a texture area so its UVs cover the full stored parent area instead of the current sub-area.
        /// </summary>
        /// <param name="texture">The texture area whose parent bounds should become the full UV rectangle.</param>
        /// <returns>A copy of <paramref name="texture"/> expanded to its full parent area.</returns>
        public static TextureArea CreateFullParentAreaTexture(TextureArea texture)
        {
            TextureArea fullTexture = texture;
            fullTexture.TexCoord0 = new Vector2(texture.ParentArea.X0, texture.ParentArea.Y0);
            fullTexture.TexCoord1 = new Vector2(texture.ParentArea.X0, texture.ParentArea.Y1);
            fullTexture.TexCoord2 = new Vector2(texture.ParentArea.X1, texture.ParentArea.Y1);
            fullTexture.TexCoord3 = new Vector2(texture.ParentArea.X1, texture.ParentArea.Y0);
            fullTexture.ParentArea = Rectangle2.Zero;
            return fullTexture;
        }

        /// <summary>
        /// Creates a synthetic animated texture set whose frames are cropped to the same relative sub-area as the input texture.
        /// </summary>
        /// <param name="originalSet">The source animated texture set.</param>
        /// <param name="texture">The texture area that defines the desired sub-area.</param>
        /// <param name="parentRect">The full parent rectangle expected to match a frame in <paramref name="originalSet"/>.</param>
        /// <param name="subRect">The actual sub-rectangle that should be projected onto every frame.</param>
        /// <param name="margin">The matching tolerance for resolving the source frame.</param>
        /// <param name="subSet">Receives the generated sub-area animation set when the method succeeds.</param>
        /// <returns><see langword="true"/> when a valid sub-area animation set was generated; otherwise <see langword="false"/>.</returns>
        public static bool TryCreateSubAreaAnimationSet(
            AnimatedTextureSet originalSet,
            TextureArea texture,
            Rectangle2 parentRect,
            Rectangle2 subRect,
            float margin,
            [NotNullWhen(true)] out AnimatedTextureSet? subSet)
        {
            subSet = null;

            AnimatedTextureFrame? matchedFrame = FindBestMatchingAnimatedFrame(originalSet, texture, parentRect, margin);

            if (matchedFrame is null)
                return false;

            var matchedFrameRect = Rectangle2.FromCoordinates(matchedFrame.TexCoord0, matchedFrame.TexCoord1, matchedFrame.TexCoord2, matchedFrame.TexCoord3);

            if (matchedFrameRect.Width == 0 || matchedFrameRect.Height == 0)
                return false;

            float relX0 = (subRect.X0 - matchedFrameRect.X0) / matchedFrameRect.Width;
            float relY0 = (subRect.Y0 - matchedFrameRect.Y0) / matchedFrameRect.Height;
            float relX1 = (subRect.X1 - matchedFrameRect.X0) / matchedFrameRect.Width;
            float relY1 = (subRect.Y1 - matchedFrameRect.Y0) / matchedFrameRect.Height;

            subSet = originalSet.Clone();

            foreach (var subFrame in subSet.Frames)
            {
                var frameRect = Rectangle2.FromCoordinates(subFrame.TexCoord0, subFrame.TexCoord1, subFrame.TexCoord2, subFrame.TexCoord3);

                float frameWidth = frameRect.Width;
                float frameHeight = frameRect.Height;

                subFrame.TexCoord0 = new Vector2(frameRect.X0 + relX0 * frameWidth, frameRect.Y0 + relY0 * frameHeight);
                subFrame.TexCoord1 = new Vector2(frameRect.X0 + relX0 * frameWidth, frameRect.Y0 + relY1 * frameHeight);
                subFrame.TexCoord2 = new Vector2(frameRect.X0 + relX1 * frameWidth, frameRect.Y0 + relY1 * frameHeight);
                subFrame.TexCoord3 = new Vector2(frameRect.X0 + relX1 * frameWidth, frameRect.Y0 + relY0 * frameHeight);
            }

            return true;
        }
    }

    internal readonly struct SubAreaKey : IEquatable<SubAreaKey>
    {
        private readonly int _destinationKey;

        public readonly Texture Texture;
        public readonly Rectangle2 ParentRect;
        public readonly Rectangle2 SubRect;

        /// <summary>
        /// Creates a cache key for a sub-area lookup that does not vary by texture destination.
        /// </summary>
        /// <param name="texture">The texture identity to track.</param>
        /// <param name="parentRect">The full parent rectangle of the animated frame.</param>
        /// <param name="subRect">The cropped sub-area rectangle.</param>
        /// <param name="margin">The rectangle quantization step used for lookup deduplication.</param>
        public SubAreaKey(Texture texture, Rectangle2 parentRect, Rectangle2 subRect, float margin)
            : this(texture, 0, parentRect, subRect, margin)
        { }

        /// <summary>
        /// Creates a cache key for a sub-area lookup scoped to the classic room versus object destination split.
        /// </summary>
        /// <param name="texture">The texture identity to track.</param>
        /// <param name="isForRoom"><see langword="true"/> for room textures; <see langword="false"/> for object textures.</param>
        /// <param name="parentRect">The full parent rectangle of the animated frame.</param>
        /// <param name="subRect">The cropped sub-area rectangle.</param>
        /// <param name="margin">The rectangle quantization step used for lookup deduplication.</param>
        public SubAreaKey(Texture texture, bool isForRoom, Rectangle2 parentRect, Rectangle2 subRect, float margin)
            : this(texture, isForRoom ? 1 : 2, parentRect, subRect, margin)
        { }

        /// <summary>
        /// Creates a cache key for a sub-area lookup scoped to a specific texture destination.
        /// </summary>
        /// <param name="texture">The texture identity to track.</param>
        /// <param name="destination">The destination bucket that the generated lookup belongs to.</param>
        /// <param name="parentRect">The full parent rectangle of the animated frame.</param>
        /// <param name="subRect">The cropped sub-area rectangle.</param>
        /// <param name="margin">The rectangle quantization step used for lookup deduplication.</param>
        public SubAreaKey(Texture texture, TextureDestination destination, Rectangle2 parentRect, Rectangle2 subRect, float margin)
            : this(texture, (int)destination + 1, parentRect, subRect, margin)
        { }

        private SubAreaKey(Texture texture, int destinationKey, Rectangle2 parentRect, Rectangle2 subRect, float margin)
        {
            Texture = texture;
            _destinationKey = destinationKey;
            ParentRect = AnimatedTextureLookupUtility.NormalizeLookupRectangle(parentRect, margin);
            SubRect = AnimatedTextureLookupUtility.NormalizeLookupRectangle(subRect, margin);
        }

        /// <summary>
        /// Compares two sub-area keys using normalized bounds, destination scope, and logical texture identity.
        /// </summary>
        /// <param name="other">The key to compare against.</param>
        /// <returns><see langword="true"/> when both keys refer to the same deduplicated sub-area lookup.</returns>
        public bool Equals(SubAreaKey other)
        {
            if (_destinationKey != other._destinationKey || ParentRect != other.ParentRect || SubRect != other.SubRect)
                return false;

            return AnimatedTextureLookupUtility.AreEquivalentTextures(Texture, other.Texture);
        }

        public override bool Equals(object? obj) => obj is SubAreaKey key && Equals(key);

        public override int GetHashCode()
            => HashCode.Combine(_destinationKey, ParentRect, SubRect, AnimatedTextureLookupUtility.GetTextureIdentityHash(Texture));
    }
}
