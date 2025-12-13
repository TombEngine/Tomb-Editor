using System;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using TombLib.LevelData;
using System.Runtime.CompilerServices;

namespace TombLib.Utils
{
    public abstract class Texture : ICloneable
    {
        public static ImageC UnloadedPlaceholder { get; } = ImageC.Black;

        // Do not change the image with this method
        public ImageC Image { get; protected set; } = UnloadedPlaceholder;

		// This helps the texture packer of TombEngine compiler to have ready paths for doing sidecar loading
		public string AbsolutePath { get; set; }

		public abstract Texture Clone();

        object ICloneable.Clone()
        {
            return Clone();
        }

        public bool IsUnavailable => Image == UnloadedPlaceholder;
        public bool IsAvailable => Image != UnloadedPlaceholder;
    }

    public sealed class TextureInvisible : Texture
    {
        public static Texture Instance { get; } = new TextureInvisible();

        private TextureInvisible()
        {
            Image = ImageC.Transparent;
        }

        public override Texture Clone()
        {
            return Instance;
        }
    }

    public interface TextureHashed
    {
        Hash Hash { get; }
    }

    public enum BlendMode : ushort
    {
        Normal = 0,
        AlphaTest = 1,
        Additive = 2,
        NoZTest = 4,
        Subtract = 5,
        Wireframe = 6,
        Exclude = 8,
        Screen = 9,
        Lighten = 10,
        AlphaBlend = 11
    }

    public enum BumpMappingLevel
    {
        None = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3
    }

    public enum TextureShapeType
    {
        XnYnClockwise = 0,
        QuadClockwise = 0,
        XpYnClockwise = 1,
        QuadCounterclockwise = 1,
        XpYpClockwise = 2,
        XnYpClockwise = 3,
        XpYnCounterclockwise = 4,
        XnYnCounterclockwise = 5,
        XnYpCounterclockwise = 6,
        XpYpCounterclockwise = 7
    }

    public enum TextureSearchType
    {
        Empty,
        Invisible,
        Broken,
        ExactMatch,
        PartialMatch,
        TextureSet
    }

    public static class TextureExtensions
    {
         // Helper procedures to convert between blending mode indices available to user
         // and actual internal enumeration of blending modes.

        public static BlendMode ToBlendMode(int userIndex)
        {
            var result = BlendMode.Normal;
            switch (userIndex)
            {
                default:
                case 0: result = BlendMode.Normal; break;
                case 1: result = BlendMode.Additive; break;
                case 2: result = BlendMode.Subtract; break;
                case 3: result = BlendMode.Exclude; break;
                case 4: result = BlendMode.Screen; break;
                case 5: result = BlendMode.Lighten; break;
                case 6: result = BlendMode.AlphaTest; break;
            }
            return result;
        }

        public static int ToUserIndex(this BlendMode mode)
        {
            var result = 0;
            switch (mode)
            {
                default:
                case BlendMode.Normal: result = 0; break;
                case BlendMode.Additive: result = 1; break;
                case BlendMode.Subtract: result = 2; break;
                case BlendMode.Exclude: result = 3; break;
                case BlendMode.Screen: result = 4; break;
                case BlendMode.Lighten: result = 5; break;
                case BlendMode.AlphaTest: result = 6; break;
            }
            return result;
        }

        // Helper UI function which gets the names of all available blending modes
        // according to selected game version.

        public static List<string> BlendModeUserNames(TRVersion.Game version) =>
            BlendModeUserNames(new LevelSettings() { GameVersion = version, GameEnableExtraBlendingModes = true });

        public static List<string> BlendModeUserNames(LevelSettings settings)
        {
            int blendCount;

            // For TR4, TRNG and TombEngine we can add all types (if extra blending modes are enabled)
            if (settings.GameVersion == TRVersion.Game.TombEngine)
            {
                blendCount = 7;
            }
            else if (((settings.GameEnableExtraBlendingModes ?? false) && settings.GameVersion.Native() == TRVersion.Game.TR4))
            {
                blendCount = 6;
            }
            else
            {
                // Additive blending is for TR3-5 only
                if (settings.GameVersion.Native() >= TRVersion.Game.TR3)
                    blendCount = 2;
                else
                    blendCount = 1; // Type 0 exists everywhere
            }

            var result = new List<string>();
            for (int i = 0; i < blendCount; i++)
                result.Add(ToBlendMode(i).ToString().SplitCamelcase());

            return result;
        }

        // Mapping correction compensation coordinate sets.
        // Used to counterbalance TR4/5 internal mapping correction applied in regard to NewFlags
        // value.

        public static readonly Vector2[,] CompensationTris = new Vector2[,]
        {
            { new Vector2( 0.5f,  0.5f), new Vector2(-0.5f,  0.5f), new Vector2( 0.5f, -0.5f) },
            { new Vector2(-0.5f,  0.5f), new Vector2(-0.5f, -0.5f), new Vector2( 0.5f,  0.5f) },
            { new Vector2(-0.5f, -0.5f), new Vector2( 0.5f, -0.5f), new Vector2(-0.5f,  0.5f) },
            { new Vector2( 0.5f, -0.5f), new Vector2( 0.5f,  0.5f), new Vector2(-0.5f, -0.5f) },
            { new Vector2(-0.5f,  0.5f), new Vector2( 0.5f,  0.5f), new Vector2(-0.5f, -0.5f) },
            { new Vector2( 0.5f,  0.5f), new Vector2( 0.5f, -0.5f), new Vector2(-0.5f,  0.5f) },
            { new Vector2( 0.5f, -0.5f), new Vector2(-0.5f, -0.5f), new Vector2( 0.5f,  0.5f) },
            { new Vector2(-0.5f, -0.5f), new Vector2(-0.5f,  0.5f), new Vector2( 0.5f, -0.5f) }
        };

        public static readonly Vector2[,] CompensationQuads = new Vector2[,]
        {
            { new Vector2( 0.5f, 0.5f), new Vector2(-0.5f, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2( 0.5f, -0.5f) },
            { new Vector2(-0.5f, 0.5f), new Vector2( 0.5f, 0.5f), new Vector2( 0.5f, -0.5f), new Vector2(-0.5f, -0.5f) }
        };

        public static TextureShapeType GetTextureShapeType(Vector2[] texCoords, bool isForTriangle)
        {
            bool isClockwise = !(MathC.CalculateArea(texCoords) > 0.0f);

            if (isForTriangle)
            {
                Vector2 midPoint = (texCoords[0] + texCoords[1] + texCoords[2]) * (1.0f / 3.0f);

                // Determine closest edge to the mid
                float distance0 = (texCoords[0] - midPoint).LengthSquared();
                float distance1 = (texCoords[1] - midPoint).LengthSquared();
                float distance2 = (texCoords[2] - midPoint).LengthSquared();

                byte closeEdgeIndex = 0;
                if (distance1 < Math.Min(distance0, distance2))
                    closeEdgeIndex = 1;
                if (distance2 < Math.Min(distance0, distance1))
                    closeEdgeIndex = 2;

                // Determine case
                Vector2 toClosestEdge = texCoords[closeEdgeIndex] - midPoint;
                if (toClosestEdge.X < 0)
                    if (toClosestEdge.Y < 0)
                    { // Negative X, Negative Y
                        // +---+
                        // |  /
                        // | /
                        // |/
                        // +
                        if (isClockwise)
                            return (TextureShapeType)0; //static constexpr Diverse::Vec<2, float> Triangle0[3] = { { 0.5f, 0.5f }, { -0.5f, 0.5f }, { 0.5f, -0.5f } };
                        else
                            return (TextureShapeType)5; //static constexpr Diverse::Vec<2, float> Triangle5[3] = { { 0.5f, 0.5f }, { 0.5f, -0.5f }, { -0.5f, 0.5f } };
                    }
                    else
                    { // Negative X, Postive Y
                        // +
                        // |\
                        // | \
                        // |  \
                        // +---+
                        if (isClockwise)
                            return (TextureShapeType)3; //static constexpr Diverse::Vec<2, float> Triangle3[3] = { { 0.5f, -0.5f }, { 0.5f, 0.5f }, { -0.5f, -0.5f } };
                        else
                            return (TextureShapeType)6; //static constexpr Diverse::Vec<2, float> Triangle6[3] = { { 0.5f, -0.5f }, { -0.5f, -0.5f }, { 0.5f, 0.5f } };
                    }
                else
                    if (toClosestEdge.Y < 0)
                { // Postive X, Negative Y
                  // +---+
                  //  \  |
                  //   \ |
                  //    \|
                  //     +
                    if (isClockwise)
                        return (TextureShapeType)1; //static constexpr Diverse::Vec<2, float> Triangle1[3] = { { -0.5f, 0.5f }, { -0.5f, -0.5f }, { 0.5f, 0.5f } };
                    else
                        return (TextureShapeType)4; //static constexpr Diverse::Vec<2, float> Triangle4[3] = { { -0.5f, 0.5f }, { 0.5f, 0.5f }, { -0.5f, -0.5f } };
                }
                else
                { // Postive X, Postive Y
                  //     +
                  //    /|
                  //   / |
                  //  /  |
                  // +---+
                    if (isClockwise)
                        return (TextureShapeType)2; //static constexpr Diverse::Vec<2, float> Triangle2[3] = { { -0.5f, -0.5f }, { 0.5f, -0.5f }, { -0.5f, 0.5f } };
                    else
                        return (TextureShapeType)7; //static constexpr Diverse::Vec<2, float> Triangle7[3] = { { -0.5f, -0.5f }, { -0.5f, 0.5f }, { 0.5f, -0.5f } };
                }
            }
            else if (!isClockwise)
                return (TextureShapeType)1;
            else
                return (TextureShapeType)0;
        }
    }

    public struct TextureArea : IEquatable<TextureArea>
    {
        public static readonly TextureArea None;
        public static readonly TextureArea Invisible = new TextureArea { Texture = TextureInvisible.Instance };

        public Texture Texture;
        public Rectangle2 ParentArea;
        public Vector2 TexCoord0; // No array for those because:
        public Vector2 TexCoord1; //    - Cache locality
        public Vector2 TexCoord2; //    - No array bounds checks
        public Vector2 TexCoord3; //    - 'Clone', 'GetHashCode' and so on work by default
        public BlendMode BlendMode;
        public bool DoubleSided;

        public static bool operator ==(TextureArea first, TextureArea second)
        {
            return
                first.Texture == second.Texture &&
                first.TexCoord0.Equals(second.TexCoord0) &&
                first.TexCoord1.Equals(second.TexCoord1) &&
                first.TexCoord2.Equals(second.TexCoord2) &&
                first.TexCoord3.Equals(second.TexCoord3) &&
                first.ParentArea.Start.Equals(second.ParentArea.Start) &&
                first.ParentArea.End.Equals(second.ParentArea.End) &&
                first.BlendMode == second.BlendMode &&
                first.DoubleSided == second.DoubleSided;
        }

        public static bool operator !=(TextureArea first, TextureArea second) => !(first == second);
        public bool Equals(TextureArea other) => this == other;
        public override bool Equals(object other) => other is TextureArea && this == (TextureArea)other;

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Texture);
            hash.Add(TexCoord0);
            hash.Add(TexCoord1);
            hash.Add(TexCoord2);
            hash.Add(TexCoord3);
            hash.Add(ParentArea);
            hash.Add(BlendMode);
            hash.Add(DoubleSided);
            return hash.ToHashCode();
        }

        public bool TextureIsUnavailable => Texture == null || Texture.IsUnavailable;
        public bool TextureIsInvisible => Texture == TextureInvisible.Instance || Texture == null;
        public bool TextureIsTriangle => TexCoord2 == TexCoord3;
        public bool TextureIsDegenerate => (!TextureIsTriangle && QuadArea == 0) || (TextureIsTriangle && TriangleArea == 0);

        public bool TriangleCoordsOutOfBounds
        {
            get
            {
                if (TextureIsInvisible || TextureIsUnavailable)
                    return false;

                Vector2 max = Vector2.Max(Vector2.Max(TexCoord0, TexCoord1), TexCoord2);
                Vector2 min = Vector2.Min(Vector2.Min(TexCoord0, TexCoord1), TexCoord2);

                return min.X < 0.0f || min.Y < 0.0f || max.X > Texture.Image.Width || max.Y > Texture.Image.Height ||
                       max.X - min.X > 256.0f || max.Y - min.Y > 256.0f;
            }
        }

        public bool QuadCoordsOutOfBounds
        {
            get
            {
                if (TextureIsInvisible || TextureIsUnavailable)
                    return false;

                Vector2 max = Vector2.Max(Vector2.Max(TexCoord0, TexCoord1), Vector2.Max(TexCoord2, TexCoord3));
                Vector2 min = Vector2.Min(Vector2.Min(TexCoord0, TexCoord1), Vector2.Min(TexCoord2, TexCoord3));

                return min.X < 0.0f || min.Y < 0.0f || max.X > Texture.Image.Width || max.Y > Texture.Image.Height ||
                       max.X - min.X > 256.0f || max.Y - min.Y > 256.0f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public Rectangle2 GetRect(bool? isTriangle = null)
        {
            bool tri = isTriangle ?? TextureIsTriangle;
            return tri
                ? Rectangle2.FromCoordinates(TexCoord0, TexCoord1, TexCoord2)
                : Rectangle2.FromCoordinates(TexCoord0, TexCoord1, TexCoord2, TexCoord3);
        }

        public Vector2[] TexCoords
        {
            get
            {
                return new Vector2[]
                {
                TexCoord0,
                TexCoord1,
                TexCoord2,
                TexCoord3
                };
            }
        }

        // Gets canonical texture area which is compatible with UVRotate routine
        // and also puts rotational difference into Rotation out parameter
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public TextureArea GetCanonicalTexture(bool isTriangle)
        {
            var minY = GetRect(isTriangle).Start.Y;
            var transformedTexture = this;

            while (transformedTexture.TexCoord0.Y != minY)
                transformedTexture.Rotate(1, isTriangle);

            // Perform extra rotation in case it's texture with similar upper coordinates
            if (minY == (isTriangle ? transformedTexture.TexCoord2.Y : transformedTexture.TexCoord3.Y))
                transformedTexture.Rotate(1, isTriangle);

            return transformedTexture;
        }

        public TextureArea RestoreQuad()
        {
            if (!TextureIsTriangle)
                return this;

            var area = GetRect(true);
            var restoredTexture = this;

            Vector2[] restoredCoords = new Vector2[4];
            restoredCoords[0] = new Vector2(area.X0, area.Y0);
            restoredCoords[1] = new Vector2(area.X1, area.Y0);
            restoredCoords[2] = new Vector2(area.X1, area.Y1);
            restoredCoords[3] = new Vector2(area.X0, area.Y1);

            Vector2[] originalCoords = new Vector2[4];
            originalCoords[0] = TexCoord0;
            originalCoords[1] = TexCoord1;
            originalCoords[2] = TexCoord2;
            originalCoords[3] = TexCoord1 + TexCoord2;

            // Get closest vertex to zero coord

            int closest = 0;
            var length = float.MaxValue;

            for (int i = 0; i < 4; i++)
            {
                var newLength = Vector2.Distance(restoredCoords[i], originalCoords[0]);
                if (newLength <= length)
                {
                    closest = i;
                    length = newLength;
                }
            }

            for (int i = closest, j = 0; i <= closest + 3; i++, j++)
                originalCoords[i % 4] = restoredCoords[j];

            restoredTexture.TexCoord0 = originalCoords[0];
            restoredTexture.TexCoord1 = originalCoords[1];
            restoredTexture.TexCoord2 = originalCoords[2];
            restoredTexture.TexCoord3 = originalCoords[3];

            return restoredTexture;
        }

        public TextureArea RestoreQuadWithRotation()
        {
            if (!TextureIsTriangle)
                return this;

            try
            {
                var area = GetRect(true);
                var triangleCoords = TexCoords.ToList();
                var coords = new List<Vector2>
                {
                    area.Start,
                    new Vector2(area.X1, area.Y0),
                    new Vector2(area.X0, area.Y1),
                    area.End
                };

                var newCoord = coords.First(c => !triangleCoords.Contains(c));
                var newTexture = this;
                newTexture.TexCoord3 = newCoord;

                return newTexture;
            }
            catch
            {
                return RestoreQuad();
            }
        }

        public void ClampToBounds()
        {
            TexCoord0.X = Math.Max(0.0f, TexCoord0.X);
            TexCoord0.Y = Math.Max(0.0f, TexCoord0.Y);
            TexCoord1.X = Math.Max(0.0f, TexCoord1.X);
            TexCoord1.Y = Math.Max(0.0f, TexCoord1.Y);
            TexCoord2.X = Math.Max(0.0f, TexCoord2.X);
            TexCoord2.Y = Math.Max(0.0f, TexCoord2.Y);
            TexCoord3.X = Math.Max(0.0f, TexCoord3.X);
            TexCoord3.Y = Math.Max(0.0f, TexCoord3.Y);

            if (!TextureIsInvisible && !TextureIsUnavailable)
            {                                                
                TexCoord0.X = Math.Min(Texture.Image.Width,  TexCoord0.X);
                TexCoord0.Y = Math.Min(Texture.Image.Height, TexCoord0.Y);
                TexCoord1.X = Math.Min(Texture.Image.Width,  TexCoord1.X);
                TexCoord1.Y = Math.Min(Texture.Image.Height, TexCoord1.Y);
                TexCoord2.X = Math.Min(Texture.Image.Width,  TexCoord2.X);
                TexCoord2.Y = Math.Min(Texture.Image.Height, TexCoord2.Y);
                TexCoord3.X = Math.Min(Texture.Image.Width,  TexCoord3.X);
                TexCoord3.Y = Math.Min(Texture.Image.Height, TexCoord3.Y);
            }
        }

        public Vector2 GetTexCoord(int index)
        {
            switch (index)
            {
                case 0:
                    return TexCoord0;
                case 1:
                    return TexCoord1;
                case 2:
                    return TexCoord2;
                case 3:
                    return TexCoord3;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetTexCoord(int index, Vector2 value)
        {
            switch (index)
            {
                case 0:
                    TexCoord0 = value;
                    break;
                case 1:
                    TexCoord1 = value;
                    break;
                case 2:
                    TexCoord2 = value;
                    break;
                case 3:
                    TexCoord3 = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Mirror(bool isTriangle = false)
        {
            if (!isTriangle)
            {
                Swap.Do(ref TexCoord0, ref TexCoord3);
                Swap.Do(ref TexCoord1, ref TexCoord2);
            }
            else
            {
                Swap.Do(ref TexCoord0, ref TexCoord2);
                TexCoord3 = TexCoord2;
            }
        }

		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		public void Rotate(int iter = 1, bool isTriangle = false)
        {
            for (int i = 0; i < iter; i++)
            {
                if (!isTriangle)
                {
                    Vector2 tempTexCoord = TexCoord3;
                    TexCoord3 = TexCoord2;
                    TexCoord2 = TexCoord1;
                    TexCoord1 = TexCoord0;
                    TexCoord0 = tempTexCoord;
                }
                else
                {
                    Vector2 tempTexCoord = TexCoord2;
                    TexCoord2 = TexCoord1;
                    TexCoord1 = TexCoord0;
                    TexCoord0 = tempTexCoord;
                    TexCoord3 = TexCoord2;
                }
            }
        }

        public TextureArea Transform(RectTransformation transformation)
        {
            TextureArea result = this;
            transformation.TransformValueDiagonalQuad(ref result.TexCoord0, ref result.TexCoord1, ref result.TexCoord2, ref result.TexCoord3);
            return result;
        }

        public bool SetParentArea(int maxTextureSize = int.MaxValue)
        {
            if (Texture.Image.Width > maxTextureSize || Texture.Image.Height > maxTextureSize)
                return false;

            ParentArea = Texture.Image.GetRect();
            return true;
        }

        public void ClearParentArea()
        {
            ParentArea = Rectangle2.Zero;
        }

        public float TriangleArea => MathC.CalculateArea(TexCoord0, TexCoord1, TexCoord2);
        public float QuadArea => MathC.CalculateArea(TexCoord0, TexCoord1, TexCoord2, TexCoord3);
    }
}
