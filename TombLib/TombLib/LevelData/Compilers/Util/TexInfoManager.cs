using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.LevelData.Compilers.Util
{
    public class TexInfoManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const int _noTexInfo = -1;
        private const int _dummyTexInfo = -2;
        private const int _minimumPadding = 1;
        private const int _minimumTileSize = 256;
        private const float _animTextureLookupMargin = 5.0f;

        // We need to keep level reference for padding and bumpmap references.

        protected readonly Level _level;
        protected readonly IProgressReporter _progressReporter;

        // Defines if texinfo manager should actually start generating texinfo indexes.
        // Needed for anim lookup table generation.

        private bool _generateTexInfos = false;

        // Defines if all data has been laid out and texture manager is in finalized state.
        // If user tries to add textures after that, texture manager will throw an exception.

        private bool _dataHasBeenLaidOut = false;

        // Two lists of animated textures contain reference animation versions for each sequence
        // and actual found animated texture sequences in rooms. When compiler encounters a tile
        // which is similar to one of the reference animation versions, it copies it into actual
        // animations list with all respective frames. Any new comparison is made with actual
        // animation sequences at first, so next found similar tile will refer to already existing
        // animation version.
        // On packing, list of actual anim textures is added to main ParentTextures list, so only
        // versions existing in level file are added to texinfo list. Rest from reference list
        // is ignored.

        private List<ParentAnimatedTexture> _referenceAnimTextures = new List<ParentAnimatedTexture>();
        private List<ParentAnimatedTexture> _actualAnimTextures = new List<ParentAnimatedTexture>();

        // UVRotate count should be placed after anim texture data to identify how many first anim seqs
        // should be processed using UVRotate engine function

        public int UvRotateCount => _actualAnimTextures.Count(seq => seq.Origin.IsUvRotate);

        // List of parent textures should contain all "ancestor" texture areas in which all variations
        // are placed, including mirrored and rotated ones.

        private List<ParentTextureArea> _parentTextures = new List<ParentTextureArea>();

        // MaxTileSize defines maximum size to which parent can be inflated by incoming child, if
        // inflation is allowed.

        private ushort MaxTileSize = _minimumTileSize;

        // If padding value is 0, 1 px padding will be forced on object textures anyway,
        // because yet we don't have a mechanism to specify UV adjustment in converted WADs.

        private ushort _padding = 8;

        // TexInfoCount is internally a "reference counter" which is also used to get new TexInfo IDs.
        // Since generation of TexInfos is an one-off serialized process, we can safely use it in
        // serial manner as well.

        public int TexInfoCount { get; private set; } = 0;

        // Final texture pages and its counters

        public int NumRoomPages { get; private set; }
        public ImageC RoomPages { get; private set; }

        public int NumObjectsPages { get; private set; }
        public ImageC ObjectsPages { get; private set; }

        public int NumBumpPages { get; private set; }
        public ImageC BumpPages { get; private set; }

        // Precompiled object textures are kept in this dictionary.

        private SortedDictionary<int, ObjectTexture> _objectTextures;

        // Precompiled anim texture indices are kept separately to avoid
        // messing up after texture page cleanup.

        private List<List<ushort>> _animTextureIndices;

        // Expose the latter publicly, because TRNG compiler needs it
        public ReadOnlyCollection<KeyValuePair<AnimatedTextureSet, ReadOnlyCollection<ushort>>> AnimatedTextures
        {
            get
            {
                var result = new List<KeyValuePair<AnimatedTextureSet, ReadOnlyCollection<ushort>>>();
                for (int i = 0; i < _animTextureIndices.Count; i++)
                    result.Add(new KeyValuePair<AnimatedTextureSet, ReadOnlyCollection<ushort>>
                        (_actualAnimTextures[i].Origin, _animTextureIndices[i].AsReadOnly()));
                return result.AsReadOnly();
            }
        }

        // ChildTextureArea is a simple enclosed relative texture area with stripped down parameters
        // which should be the same among all children of same parent.
        // Stripped down parameters include BumpLevel and IsForTriangle, because if these are
        // different, it automatically means we should assign new parent for this child.

        public class ChildTextureArea
        {
            public int TexInfoIndex;
            public Vector2[] RelCoord;  // Relative to parent!
            public Vector2[] AbsCoord; // Absolute

            public BlendMode BlendMode;
            public bool IsForTriangle;

            public Rectangle2 GetRect()
            {
                if (AbsCoord.Length == 3)
                    return Rectangle2.FromCoordinates(AbsCoord[0], AbsCoord[1], AbsCoord[2]);
                else
                    return Rectangle2.FromCoordinates(AbsCoord[0], AbsCoord[1], AbsCoord[2], AbsCoord[3]);
            }
        }

        // ParentTextureArea is a texture area which contains all other texture areas which are
        // completely inside current one. Bumpmapping parameter define that parent is different,
        // hence two TextureAreas with same UV coordinates but with different BumpLevel will be 
        // saved as different parents.

        public class ParentTextureArea
        {
            public VectorInt2 PositionInPage { get; set; }
            public int Page { get; set; }
            public int[] Padding { get; set; } = new int[4]; // LTRB
            public bool SqueezeAndDuplicate { get; set; } // Needed for UVRotate

            public Texture Texture { get; private set; }
            public bool IsForRoom { get; set; }

            // Waterfall textures need to stay on top of texture page without
            // padding, because of extremely ugly Core Design waterfall UVRotate hack.
            private bool _topmostAndUnpadded;
            public bool TopmostAndUnpadded
            {
                get { return _topmostAndUnpadded; }
                set { if (value) _topmostAndUnpadded = value; }
            }

            private Rectangle2 _area;
            public Rectangle2 Area
            {
                get { return _area; }
                set
                {
                    // Disallow reducing area size, cause it corrupts children.
                    if (value == _area || !value.Contains(_area))
                        return;

                    // Calculate startpoint delta and fix up children to comply with new parent area.
                    if (Children != null && Children.Count > 0)
                    {
                        var delta = _area.Start - value.Start;
                        foreach (var child in Children)
                            for (int i = 0; i < child.RelCoord.Length; i++)
                                child.RelCoord[i] += delta;
                    }

                    _area = value;
                }
            }

            public List<ChildTextureArea> Children;

            // Generates new ParentTextureArea from raw texture coordinates.
            public ParentTextureArea(TextureArea texture, bool isForRoom)
            {
                // Round area to nearest pixel to prevent rounding errors further down the line.
                // Use ParentArea to create a parent for textures which were applied with group texturing tools.
                _area = texture.ParentArea.IsZero ? texture.GetRect().Round() : texture.ParentArea.Round();
                Initialize(texture.Texture, isForRoom);
            }

            // Generates new ParentTextureArea from given area in texture.
            public ParentTextureArea(Rectangle2 area, Texture texture, bool isForRoom)
            {
                _area = area;
                Initialize(texture, isForRoom);
            }

            private void Initialize(Texture texture, bool isForRoom)
            {
                Children = new List<ChildTextureArea>();

                Texture = texture;
                IsForRoom = isForRoom;
                SqueezeAndDuplicate = false;
            }

            // Compare parent's properties with incoming texture properties.
            public bool ParametersSimilar(TextureArea incomingTexture, bool isForRoom)
            {
                if (IsForRoom != isForRoom)
                    return false;

                // See if texture is the same
                TextureHashed incoming = incomingTexture.Texture as TextureHashed;
                TextureHashed current = Texture as TextureHashed;

                // First case here should never happen, unless we find a way to texture rooms
                // with WAD textures or vice versa.
                if ((incoming == null) != (current == null))
                    return false;
                else if (incoming != null)
                    return incoming.Hash == current.Hash;
                else
                    return incomingTexture.Texture.GetHashCode() == Texture.GetHashCode();
            }

            // Compare raw bitmap data of given area with incoming texture
            public TextureArea? TextureSimilar(TextureArea texture)
            {
                // Only scan if:
                //  - Parent is room texture
                //  - Parent's texture isn't the same as incoming texture
                //  - Incoming texture is either from imported geometry or wad

                if (Texture != texture.Texture && Texture is LevelTexture &&
                   (texture.Texture is ImportedGeometryTexture || texture.Texture is WadTexture))
                {
                    var rr = texture.GetRect();
                    var pp0 = texture.Texture.Image.GetPixel((int)rr.TopLeft.X, (int)rr.TopLeft.Y);
                    var pp1 = texture.Texture.Image.GetPixel((int)rr.TopRight.X - 1, (int)rr.TopRight.Y);
                    var pp2 = texture.Texture.Image.GetPixel((int)rr.BottomRight.X - 1, (int)rr.BottomRight.Y - 1);
                    var pp3 = texture.Texture.Image.GetPixel((int)rr.BottomLeft.X, (int)rr.BottomLeft.Y - 1);
                    var pp4 = texture.Texture.Image.GetPixel((int)rr.Width / 2, (int)rr.Height / 2);

                    foreach (var child in Children)
                    {
                        // Compare size. If no match, it's not similar texture
                        var r = child.GetRect();
                        if (r.Width != rr.Width || r.Height != rr.Height)
                            continue;

                        // Compare 4 corner pixels and center to quickly filter out wrong results
                        var p0 = Texture.Image.GetPixel((int)r.TopLeft.X, (int)r.TopLeft.Y);
                        var p1 = Texture.Image.GetPixel((int)r.TopRight.X - 1, (int)r.TopRight.Y);
                        var p2 = Texture.Image.GetPixel((int)r.BottomRight.X - 1, (int)r.BottomRight.Y - 1);
                        var p3 = Texture.Image.GetPixel((int)r.BottomLeft.X, (int)r.BottomLeft.Y - 1);
                        var p4 = texture.Texture.Image.GetPixel((int)r.Width / 2, (int)r.Height / 2);
                        if (p0 != pp0 || p1 != pp1 || p2 != pp2 || p3 != pp3 || p4 != pp4)
                            continue;

                        // All pixels match. Now compare all raw data
                        var hash1 = Hash.FromByteArray(texture.Texture.Image.ToByteArray(rr));
                        var hash2 = Hash.FromByteArray(Texture.Image.ToByteArray(r));

                        if (hash1 == hash2)
                        {
                            // Replace texture set with found one and substitute texture coordinates
                            var newtex = texture;
                            var shift = r.TopLeft - rr.TopLeft;
                            newtex.Texture = Texture;
                            newtex.TexCoord0 += shift;
                            newtex.TexCoord1 += shift;
                            newtex.TexCoord2 += shift;
                            if (child.AbsCoord.Length > 3)
                                newtex.TexCoord3 += shift;
                            else
                                newtex.TexCoord3 = newtex.TexCoord2;

                            return newtex;
                        }
                    }
                }

                return null;
            }

            // Check if bumpmapping could be assigned to parent.
            // NOTE: This function is only used to check if bumpmap is possible, DO NOT use it to check ACTUAL bumpmap level!
            public BumpMappingLevel BumpLevel(TRVersion.Game version)
            {
                if (Texture is LevelTexture && version.Native() > TRVersion.Game.TR3)
                {
                    var tex = Texture as LevelTexture;
                    if (!String.IsNullOrEmpty(tex.BumpPath))
                        return BumpMappingLevel.Level1; // tomb4 doesn't care about specific flag value
                    else
                    {
                        var bumpLevel = tex.GetBumpMappingLevelFromTexCoord(Area.GetMid());
                        return bumpLevel.HasValue ? bumpLevel.Value : BumpMappingLevel.None;
                    }
                }
                return BumpMappingLevel.None;
            }

            // Checks if parameters are similar to another texture area, and if so,
            // also checks if texture area is enclosed in parent's area.
            public bool IsPotentialParent(TextureArea texture, bool isForRoom, bool allowOverlaps, uint maxOverlappedSize)
            {
                var rect = texture.GetRect();

                if (ParametersSimilar(texture, isForRoom))
                {
                    if (_area.Contains(rect))
                        return true;
                    else if (allowOverlaps)
                    {
                        var intersection = rect.Intersect(_area);
                        if (!_area.Contains(rect) && intersection.Width > 0 && intersection.Height > 0)
                        {
                            var potentialNewArea = rect.Union(_area);
                            return ((potentialNewArea.Width <= maxOverlappedSize) &&
                                    (potentialNewArea.Height <= maxOverlappedSize));
                        }
                    }
                }
                return false;
            }

            // Checks if incoming texture is similar in parameters and encloses parent area.
            public bool IsPotentialChild(TextureArea texture, bool isForRoom)
                => (ParametersSimilar(texture, isForRoom) && texture.GetRect().Round().Contains(_area));

            // Adds texture as a child to existing parent, with recalculating coordinates to relative.
            public void AddChild(TextureArea texture, int newTextureID, bool isForTriangle, bool topmostAndUnpadded)
            {
                var relative = new Vector2[isForTriangle ? 3 : 4];
                var absolute = new Vector2[isForTriangle ? 3 : 4];

                for (int i = 0; i < relative.Length; i++)
                {
                    absolute[i] = texture.GetTexCoord(i);
                    relative[i] = absolute[i] - Area.Start;
                }

                Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = newTextureID,
                    BlendMode = texture.BlendMode,
                    IsForTriangle = isForTriangle,
                    RelCoord = relative,
                    AbsCoord = absolute
                });

                // Refresh topmost flag
                TopmostAndUnpadded |= topmostAndUnpadded;

                // Expand parent area, if needed
                var rect = texture.GetRect();
                if (!Area.Contains(rect))
                    Area = Area.Union(rect).Round();
            }

            // Moves child to another parent. This is intended to work only within same texture set.
            public void MoveChild(ChildTextureArea child, ParentTextureArea newParent)
            {
                var newRelCoord = new Vector2[child.AbsCoord.Length];
                for (int i = 0; i < newRelCoord.Length; i++)
                    newRelCoord[i] = child.AbsCoord[i] - newParent.Area.Start;

                newParent.Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = child.TexInfoIndex,
                    BlendMode = child.BlendMode,
                    IsForTriangle = child.IsForTriangle,
                    RelCoord = newRelCoord,
                    AbsCoord = child.AbsCoord
                });
            }

            // Moves child to another parent by absolute coordinate. This is intended to work between texture sets.
            public void MoveChildWithoutRepositioning(ChildTextureArea child, ParentTextureArea newParent)
            {
                var newAbsCoord = new Vector2[child.AbsCoord.Length];
                for (int i = 0; i < newAbsCoord.Length; i++)
                    newAbsCoord[i] = child.RelCoord[i] + newParent.Area.Start;

                newParent.Children.Add(new ChildTextureArea()
                {
                    TexInfoIndex = child.TexInfoIndex,
                    BlendMode = child.BlendMode,
                    IsForTriangle = child.IsForTriangle,
                    RelCoord = child.RelCoord,
                    AbsCoord = newAbsCoord
                });
            }

            public void MergeParents(List<ParentTextureArea> parentList, List<ParentTextureArea> parents)
            {
                foreach (var parent in parents)
                {
                    Area = Area.Union(parent.Area);
                    TopmostAndUnpadded |= parent.TopmostAndUnpadded; // Refresh topmost flag

                    foreach (var child in parent.Children)
                        parent.MoveChild(child, this);

                    parent.Children.Clear();
                }

                parents.ForEach(item => parentList.Remove(item));
            }
        }

        // ParentAnimatedTexture contains all precompiled frames of a specific anim texture set.
        // Since animated textures can overlap (especially with animators), we store list of frames
        // as a list of parents. Children and parents are added in sequential order, so no sorting
        // must be made on CompiledAnimation.

        public class ParentAnimatedTexture : ICloneable
        {
            public ParentAnimatedTexture Clone()
            {
                var result = new ParentAnimatedTexture(Origin);
                result.CompiledAnimation = new List<ParentTextureArea>();

                foreach (var parent in CompiledAnimation)
                {
                    var newParent = new ParentTextureArea(parent.Area, parent.Texture, parent.IsForRoom);

                    // Squeeze and duplicate bitmap data for UVRotate texture sets
                    newParent.SqueezeAndDuplicate = Origin.IsUvRotate;

                    foreach (var child in parent.Children)
                    {
                        var newChild = new ChildTextureArea()
                        {
                            BlendMode = child.BlendMode,
                            IsForTriangle = child.IsForTriangle,
                            TexInfoIndex = child.TexInfoIndex
                        };

                        newChild.RelCoord = new Vector2[child.RelCoord.Length];
                        newChild.AbsCoord = new Vector2[child.AbsCoord.Length];

                        for (int i = 0; i < child.RelCoord.Length; i++)
                        {
                            newChild.RelCoord[i] = child.RelCoord[i];
                            newChild.AbsCoord[i] = child.AbsCoord[i];
                        }

                        newParent.Children.Add(newChild);
                    }
                    result.CompiledAnimation.Add(newParent);
                }
                return result;
            }
            object ICloneable.Clone() => Clone();

            public AnimatedTextureSet Origin;
            public List<ParentTextureArea> CompiledAnimation = new List<ParentTextureArea>();

            public ParentAnimatedTexture(AnimatedTextureSet origin)
            {
                Origin = origin;
            }

            public int FrameCount()
            {
                int result = 0;
                foreach (var parent in CompiledAnimation)
                    result += parent.Children.Count;
                return result;
            }
        }

        private class ObjectTexture
        {
            public int Tile;
            public VectorInt2[] TexCoord = new VectorInt2[4];
            public ushort UVAdjustmentFlag;

            public bool IsForTriangle;
            public bool IsForRoom;
            public BlendMode BlendMode;
            public BumpMappingLevel BumpLevel;

            public ObjectTexture(ParentTextureArea parent, ChildTextureArea child, TRVersion.Game version, float maxTextureSize)
            {
                BlendMode = child.BlendMode;
                BumpLevel = parent.BumpLevel(version);
                IsForRoom = parent.IsForRoom;
                IsForTriangle = child.IsForTriangle;
                Tile = parent.Page;
                UVAdjustmentFlag = (ushort)TextureExtensions.GetTextureShapeType(child.RelCoord, IsForTriangle);

                // At first, collect absolute coords
                var coords = new Vector2[4];
                for (int i = 0; i < child.RelCoord.Length; i++)
                {
                    coords[i] = new Vector2(child.RelCoord[i].X + (float)(parent.PositionInPage.X + parent.Padding[0]),
                                            child.RelCoord[i].Y + (float)(parent.PositionInPage.Y + parent.Padding[1]));
                    
                    // If padding exists, apply half-pixel blow-up as countermeasure for hardcoded TR4-5 AdjustUV mapping correction.
                    if (version.Native() >= TRVersion.Game.TR4)
                        coords[i] -= IsForTriangle ? TextureExtensions.CompensationTris[UVAdjustmentFlag, i] :
                                                     TextureExtensions.CompensationQuads[UVAdjustmentFlag, i];
                }

                // TR3 uses unique texel alignment algorithm without which all textures tend to break.
                // But for some reason texel alignment without padding breaks adjacent textures
                // in TR3, so bypass it for such cases.

                if (version == TRVersion.Game.TR3)
                {
                    for (int i = 0; i < child.RelCoord.Length; i++)
                        coords[i] -= new Vector2(0.5f);
                }
                else
                {
                    // If no padding exists, natively chop 0.5px from every side.
                    // This works better than Core's clunky AdjustUV method.

                    if (parent.Padding.All(p => p == 0))
                        coords = MathC.CorrectTexCoords(coords, IsForTriangle, 0.5f);
                }

                for (int i = 0; i < child.RelCoord.Length; i++)
                {
                    // Clamp coordinates that are possibly out of bounds
                    coords[i].X = (float)MathC.Clamp(coords[i].X, 0, maxTextureSize);
                    coords[i].Y = (float)MathC.Clamp(coords[i].Y, 0, maxTextureSize);

                    // Pack coordinates into 2-byte set (whole and frac parts)
                    TexCoord[i] = new VectorInt2((((int)Math.Truncate(coords[i].X)) << 8) + (int)(Math.Floor(coords[i].X % 1.0f * 255.0f)),
                                                 (((int)Math.Truncate(coords[i].Y)) << 8) + (int)(Math.Floor(coords[i].Y % 1.0f * 255.0f)));
                }

                if (child.IsForTriangle)
                    TexCoord[3] = TexCoord[2];
            }

            public Rectangle2 GetRect()
            {
                if (IsForTriangle)
                    return Rectangle2.FromCoordinates(TexCoord[0], TexCoord[1], TexCoord[2]);
                else
                    return Rectangle2.FromCoordinates(TexCoord[0], TexCoord[1], TexCoord[2], TexCoord[3]);
            }
        }

        public TexInfoManager(Level level, IProgressReporter progressReporter, int maxTileSize = -1)
        {
            _level = level;
            _padding = (ushort)level.Settings.TexturePadding;
            _progressReporter = progressReporter;

            if (maxTileSize > 0 && MathC.IsPowerOf2(maxTileSize))
            {
                MaxTileSize = (ushort)maxTileSize;
            }
            else
            {
                MaxTileSize = _minimumTileSize;
            }

            GenerateAnimLookups(_level.Settings.AnimatedTextureSets);  // Generate anim texture lookup table
            _generateTexInfos = true;    // Set manager ready state 
        }

        // Gets free TexInfo index
        private int GetNewTexInfoIndex()
        {
            if (_generateTexInfos)
            {
                int result = TexInfoCount;
                TexInfoCount++;
                return result;
            }
            else
                return _dummyTexInfo;
        }

        // Try to add texture to existing parent(s) either as a child of one, or as a parent, merging
        // enclosed parents.

        private bool TryToAddToExisting(TextureArea texture, List<ParentTextureArea> parentList, bool isForRoom, bool isForTriangle, bool topmostAndUnpadded = false, int animFrameIndex = -1)
        {
            // Try to find potential parent (larger texture) and add itself to children
            foreach (var parent in parentList)
            {
                if (!parent.IsPotentialParent(texture, isForRoom, animFrameIndex >= 0, MaxTileSize))
                    continue;

                parent.AddChild(texture, animFrameIndex >= 0 ? animFrameIndex : GetNewTexInfoIndex(), isForTriangle, topmostAndUnpadded);
                return true;
            }

            // Try to find and merge parents which are enclosed in incoming texture area
            var childrenWannabes = parentList.Where(item => item.IsPotentialChild(texture, isForRoom)).ToList();
            if (childrenWannabes.Count > 0)
            {
                var newParent = new ParentTextureArea(texture, isForRoom);
                newParent.AddChild(texture, animFrameIndex >= 0 ? animFrameIndex : GetNewTexInfoIndex(), isForTriangle, topmostAndUnpadded);
                newParent.MergeParents(parentList, childrenWannabes);
                parentList.Add(newParent);
                return true;
            }

            // No success
            return false;
        }

        public struct Result
        {
            // TexInfoIndex is saved as int for forward compatibility with engines such as TombEngine.
            public int TexInfoIndex;

            // Rotation value indicate that incoming TextureArea should be rotated N times. 
            // This approach allows to tightly pack TexInfos in same manner as tom2pc does.
            // As result, CreateFace3/4 should return a face with changed index order.
            public byte Rotation;

            // This value indicates that if used on triangle, it must be converted to
            // degenerate quad. It's needed to fake UVRotate application to triangular areas.
            public bool ConvertToQuad;

            public tr_face3 CreateFace3(ushort[] indices, bool doubleSided, ushort lightingEffect)
            {
                if (indices.Length != 3)
                    throw new ArgumentOutOfRangeException(nameof(indices.Length));

                ushort objectTextureIndex = (ushort)(TexInfoIndex | (doubleSided ? 0x8000 : 0));
                ushort[] transformedIndices = new ushort[3] { indices[0], indices[1], indices[2] };

                if (Rotation > 0)
                {
                    for (int i = 0; i < Rotation; i++)
                    {
                        ushort tempIndex = transformedIndices[0];
                        transformedIndices[0] = transformedIndices[2];
                        transformedIndices[2] = transformedIndices[1];
                        transformedIndices[1] = tempIndex;
                    }
                }

                return new tr_face3 { Vertices = new ushort[3] { transformedIndices[0], transformedIndices[1], transformedIndices[2] }, Texture = objectTextureIndex, LightingEffect = lightingEffect };
            }

            public tr_face4 CreateFace4(ushort[] indices, bool doubleSided, ushort lightingEffect)
            {
                if (indices.Length != 4)
                    throw new ArgumentOutOfRangeException(nameof(indices.Length));

                ushort objectTextureIndex = (ushort)(TexInfoIndex | (doubleSided ? 0x8000 : 0));
                ushort[] transformedIndices = new ushort[4] { indices[0], indices[1], indices[2], indices[3] };

                if (Rotation > 0)
                {
                    for (int i = 0; i < Rotation; i++)
                    {
                        ushort tempIndex = transformedIndices[0];
                        transformedIndices[0] = transformedIndices[3];
                        transformedIndices[3] = transformedIndices[2];
                        transformedIndices[2] = transformedIndices[1];
                        transformedIndices[1] = tempIndex;
                    }
                }

                return new tr_face4 { Vertices = new ushort[4] { transformedIndices[0], transformedIndices[1], transformedIndices[2], transformedIndices[3] }, Texture = objectTextureIndex, LightingEffect = lightingEffect };
            }
        }

        // Gets existing TexInfo child index if there is similar one in parent textures list.

        private Result? GetTexInfo(TextureArea areaToLook, List<ParentTextureArea> parentList, 
                                   bool isForRoom, bool isForTriangle, bool topmostAndUnpadded,
                                   bool checkParameters = true, bool scanOtherSets = false, float lookupMargin = 0.0f)
        {
            var lookupCoordinates = new Vector2[isForTriangle ? 3 : 4];
            for (int i = 0; i < lookupCoordinates.Length; i++)
                lookupCoordinates[i] = areaToLook.GetTexCoord(i);

            foreach (var parent in parentList)
            {
                // Parents with different attributes are quickly discarded
                if (!parent.ParametersSimilar(areaToLook, isForRoom))
                {
                    // Try to identify if similar texture info from another texture set is present 
                    // by checking hash of the image area. If match is found, substitute lookup coordinates.

                    if (!scanOtherSets) continue;

                    var sr = parent.TextureSimilar(areaToLook);
                    if (!sr.HasValue) continue;

                    for (int i = 0; i < lookupCoordinates.Length; i++)
                        lookupCoordinates[i] = sr.Value.GetTexCoord(i);
                }

                // Extract each children's absolute coordinates and compare them to incoming texture coordinates.
                foreach (var child in parent.Children)
                {
                    // If parameters are different, children is quickly discarded from comparison.
                    if ((checkParameters && areaToLook.BlendMode != child.BlendMode) || child.IsForTriangle != isForTriangle)
                        continue;

                    // Test if coordinates are mutually equal and return resulting rotation if they are
                    var result = TestUVSimilarity(child.AbsCoord, lookupCoordinates, lookupMargin);
                    if (result != _noTexInfo)
                    {
                        // Refresh topmost flag, as same texture may be applied to faces with different topmost priority
                        parent.TopmostAndUnpadded |= topmostAndUnpadded;

                        // Refresh parent area (only in case it's from the same texture set, otherwise clashes are possible)
                        if (areaToLook.Texture == parent.Texture && !areaToLook.ParentArea.IsZero)
                            parent.Area = areaToLook.ParentArea;

                        // Child is rotation-wise equal to incoming area
                        return new Result() { TexInfoIndex = child.TexInfoIndex, Rotation = (byte)result };
                    }
                }
            }

            return null; // No equal entry, new should be created
        }

        // Tests if all UV coordinates are similar with different rotations.
        // If all coordinates are equal for one of the rotation factors, rotation factor is returned,
        // otherwise NoTexInfo is returned (not similar). If coordinates are 100% equal, 0 is returned.

        private int TestUVSimilarity(Vector2[] first, Vector2[] second, float lookupMargin)
        {
            // If first/second coordinates are not mutually quads/tris, quickly return NoTexInfo.
            // Also discard out of bounds cases without exception.
            if (first.Length == second.Length && first.Length >= 3 && first.Length <= 4)
            {
                for (int i = 0; i < first.Length; i++)
                    for (int j = 0; j < second.Length; j++)
                    {
                        var shift = (j + i) % second.Length;

                        if (!MathC.WithinEpsilon(first[j].X, second[shift].X, lookupMargin) ||
                            !MathC.WithinEpsilon(first[j].Y, second[shift].Y, lookupMargin))
                            break;

                        //Comparison was successful
                        if (j == second.Length - 1)
                            return i == 0 ? 0 : second.Length - i;
                    }
            }
            return _noTexInfo;
        }

        // Generate new parent with incoming texture and immediately add incoming texture as a child

        private void AddParent(TextureArea texture, List<ParentTextureArea> parentList, bool isForRoom, bool isForTriangle, bool topmostAndUnpadded, int frameIndex = -1)
        {
            var newParent = new ParentTextureArea(texture, isForRoom);
            parentList.Add(newParent);
            newParent.AddChild(texture, frameIndex >= 0 ? frameIndex : GetNewTexInfoIndex(), isForTriangle, topmostAndUnpadded);
        }

        // Only exposed variation of AddTexture that should be used outside of TexInfoManager itself

        public Result AddTexture(TextureArea texture, bool isForRoom, bool isForTriangle, bool topmostAndUnpadded = false)
        {
            if (_dataHasBeenLaidOut)
                throw new InvalidOperationException("Data has been already laid out for this TexInfoManager. Reinitialize it if you want to restart texture collection.");

            if ((isForTriangle && texture.TriangleCoordsOutOfBounds) || (!isForTriangle && texture.QuadCoordsOutOfBounds))
            {
                _progressReporter.ReportWarn("Texture (" + texture.TexCoord0 + ", " + texture.TexCoord1 + ", " + 
                    texture.TexCoord2 + ", " + texture.TexCoord3 + ") is out of bounds and will be ignored.");
                return new Result();
            }

            if (texture.ParentArea.Width > MaxTileSize || texture.ParentArea.Height > MaxTileSize)
            {
                _progressReporter.ReportWarn("Texture (" + texture.TexCoord0 + ", " + texture.TexCoord1 + ", " + 
                    texture.TexCoord2 + ", " + texture.TexCoord3 + ") has incorrect parent area which has been reset. " + 
                    "Possibly UV-mapped mesh with texture bigger than " + MaxTileSize + " in size.");
                texture.ParentArea = Rectangle2.Zero;
            }

            // Only try to remap animated textures if fast mode is disabled
            bool remapAnimatedTextures = _level.Settings.RemapAnimatedTextures && !_level.Settings.FastMode;

            // UVRotate hack is needed for TR4-5, because we couldn't figure real Core's UVRotate approach. 
            // For TombEngine, hopefully no such hack will be needed.
            var uvRotateHack = _level.Settings.GameVersion.Native() > TRVersion.Game.TR3;

            // If UVRotate hack is needed and texture is triangle, prepare a quad substitute reference for animation lookup.
            var refQuad = uvRotateHack && isForTriangle ? texture.RestoreQuadWithRotation() : texture;

            // Try to compare incoming texture with existing anims and return animation frame
            if (_actualAnimTextures.Count > 0)
                foreach (var actualTex in _actualAnimTextures)
                {
                    // If current animation set is UVRotate set and UVRotate hack is needed, pass the texture as quad
                    var toQuad     = actualTex.Origin.IsUvRotate && uvRotateHack;
                    var asTriangle = toQuad ? false : isForTriangle;
                    var reference  = toQuad ? refQuad : texture;

                    var existing = GetTexInfo(reference, actualTex.CompiledAnimation, isForRoom, asTriangle, false, true, remapAnimatedTextures, _animTextureLookupMargin);
                    if (existing.HasValue)
                        return new Result() { ConvertToQuad = toQuad, Rotation = existing.Value.Rotation, TexInfoIndex = existing.Value.TexInfoIndex };

                }

            // Now try to compare incoming texture with lookup anim seq table
            if (_referenceAnimTextures.Count > 0)
                foreach (var refTex in _referenceAnimTextures)
                {
                    // If current animation set is UVRotate set and UVRotate hack is needed, pass the texture as quad
                    var toQuad     = refTex.Origin.IsUvRotate && uvRotateHack;
                    var asTriangle = toQuad ? false : isForTriangle;
                    var reference  = toQuad ? refQuad : texture;

                    // If reference set found, generate actual one and immediately return fresh result
                    if (GetTexInfo(reference, refTex.CompiledAnimation, isForRoom, asTriangle, false, false, remapAnimatedTextures, _animTextureLookupMargin).HasValue)
                    {
                        GenerateAnimTexture(refTex, refQuad, isForRoom, isForTriangle);
                        var result = AddTexture(texture, isForRoom, isForTriangle);
                        return new Result() { ConvertToQuad = toQuad, Rotation = result.Rotation, TexInfoIndex = result.TexInfoIndex };
                    }
                }

            // No animated textures identified, add texture as ordinary one
            return AddTexture(texture, _parentTextures, isForRoom, isForTriangle, topmostAndUnpadded);
        }

        // Internal AddTexture variation which is capable of adding texture to various ParentTextureArea lists
        // with customizable parameters.
        // If animFrameIndex == -1, it means that ordinary texture is added, otherwise it indicates that specific anim
        // texture frame is being processed. If so, frame index is saved into TexInfoIndex field of resulting child.
        // Later on, on real anim texture creation, this index is used to sort frames in proper order.

        private Result AddTexture(TextureArea texture, List<ParentTextureArea> parentList, bool isForRoom, bool isForTriangle, bool topmostAndUnpadded = false, int animFrameIndex = -1, bool makeCanonical = true)
        {
            // In case AddTexture is used with animated seq packing, we don't check frames for full similarity, because
            // frames can be duplicated with Repeat function or simply because of complex animator functions applied.
            var result = animFrameIndex >= 0 ? null : GetTexInfo(texture, parentList, isForRoom, isForTriangle, topmostAndUnpadded);

            if (!result.HasValue)
            {
                // Try to create new canonical (top-left-based) texture as child or parent.
                // makeCanonical parameter is necessary for animated textures, because animators may produce frames
                // with non-canonically rotated coordinates (e.g. spin animator).
                var canonicalTexture = makeCanonical ? texture.GetCanonicalTexture(isForTriangle) : texture;

                // If no any potential parents or children, create as new parent
                if (!TryToAddToExisting(canonicalTexture, parentList, isForRoom, isForTriangle, topmostAndUnpadded, animFrameIndex))
                    AddParent(canonicalTexture, parentList, isForRoom, isForTriangle, topmostAndUnpadded, animFrameIndex);

                // Try again to get texinfo
                if (animFrameIndex >= 0)
                    result = new Result { TexInfoIndex = _dummyTexInfo, Rotation = 0 };
                else
                    result = GetTexInfo(texture, parentList, isForRoom, isForTriangle, topmostAndUnpadded);
            }
            
            if (!result.HasValue)
            {
                _progressReporter.ReportWarn("Texture info manager couldn't fit texture into parent list. Please send your project to developers.");
                return new Result() { TexInfoIndex = _dummyTexInfo, Rotation = 0 };
            }
            else
                return result.Value;
        }

        // Scan and set alpha-test blending mode for opaque textures.
        // To speed up the process, all children whose parent region contains alpha, is also marked as
        // alpha.

        private void SortOutAlpha(TRVersion.Game version, List<ParentTextureArea> parentList)
        {
            Parallel.For(0, parentList.Count, i =>
            {
                var opaqueChildren = parentList[i].Children.Where(child => child.BlendMode < BlendMode.Additive);
                if (opaqueChildren.Count() > 0)
                {
                    var realBlendMode = parentList[i].Texture.Image.HasAlpha(version,
                                        (int)parentList[i].Area.X0,
                                        (int)parentList[i].Area.Y0,
                                        (int)parentList[i].Area.Width,
                                        (int)parentList[i].Area.Height);

                    if (realBlendMode != BlendMode.Normal)
                        foreach (var children in opaqueChildren)
                            children.BlendMode = realBlendMode;
                }
            });
        }

        // Generates list of dummy lookup animated textures.

        private void GenerateAnimLookups(List<AnimatedTextureSet> sets)
        {
            foreach (var set in sets)
            {
                // Ignore trivial (single-frame non-UVRotated) anims
                if (set.AnimationIsTrivial)
                    continue;

                int triangleVariation = 0;
                bool mirroredVariation = false;

                // Create all possible versions of current animation, including
                // mirrored and rotated ones. Later on, when parsing actual TextureAreas
                // from faces, we will compare them with this "lookup table" and will be
                // able to quickly return desired variation ID without complicated in-place
                // calculations.

                while (true)
                {
                    var refAnim = new ParentAnimatedTexture(set);
                    int index = 0;

                    foreach (var frame in set.Frames)
                    {
                        // Create base frame
                        TextureArea newFrame = new TextureArea() { Texture = frame.Texture };

                        // Rotate or cut 4nd coordinate if needed
                        switch (triangleVariation)
                        {
                            case 0:
                                newFrame.TexCoord0 = frame.TexCoord0;
                                newFrame.TexCoord1 = frame.TexCoord1;
                                newFrame.TexCoord2 = frame.TexCoord2;
                                newFrame.TexCoord3 = frame.TexCoord3;
                                break;
                            case 1:
                                newFrame.TexCoord0 = frame.TexCoord0;
                                newFrame.TexCoord1 = frame.TexCoord1;
                                newFrame.TexCoord2 = frame.TexCoord2;
                                break;
                            case 2:
                                newFrame.TexCoord0 = frame.TexCoord0;
                                newFrame.TexCoord1 = frame.TexCoord2;
                                newFrame.TexCoord2 = frame.TexCoord3;
                                break;
                            case 3:
                                newFrame.TexCoord0 = frame.TexCoord0;
                                newFrame.TexCoord1 = frame.TexCoord1;
                                newFrame.TexCoord2 = frame.TexCoord3;
                                break;
                            case 4:
                                newFrame.TexCoord0 = frame.TexCoord1;
                                newFrame.TexCoord1 = frame.TexCoord2;
                                newFrame.TexCoord2 = frame.TexCoord3;
                                break;
                        }
                        if (triangleVariation > 0)
                            newFrame.TexCoord3 = newFrame.TexCoord2;

                        // Mirror if needed
                        if (mirroredVariation)
                            newFrame.Mirror(triangleVariation > 0);

                        // Make frame, including repeat versions
                        for (int i = 0; i < frame.Repeat; i++)
                        {
                            AddTexture(newFrame, refAnim.CompiledAnimation, true, (triangleVariation > 0), set.AnimationType == AnimatedTextureAnimationType.UVRotate, index, set.IsUvRotate);
                            index++;
                        }
                    }

                    _referenceAnimTextures.Add(refAnim);

                    triangleVariation++;
                    if (triangleVariation > 4)
                    {
                        if (!mirroredVariation)
                        {
                            triangleVariation = 0;
                            mirroredVariation = true;
                        }
                        else
                            break;
                    }
                }
            }
        }

        // Generates real animated texture sequence from reference lookup.

        private void GenerateAnimTexture(ParentAnimatedTexture reference, TextureArea origin, bool isForRoom, bool isForTriangle)
        {
            var refCopy = reference.Clone();
            foreach (var parent in refCopy.CompiledAnimation)
            {
                parent.IsForRoom = isForRoom;

                foreach (var child in parent.Children)
                    child.BlendMode = origin.BlendMode;
            }

            // Sort and assign TexInfo indices for frames by the order they were created in reference animation
            var orderedFrameList = refCopy.CompiledAnimation.SelectMany(x => x.Children).OrderBy(c => c.TexInfoIndex);
            foreach (var frame in orderedFrameList)
                frame.TexInfoIndex = GetNewTexInfoIndex();

            _actualAnimTextures.Add(refCopy);
        }

        // Finds visually similar image areas across all parents and unifies them. This step drastically
        // reduces texture page usage if lots of objects with duplicated textures are used. This process is lossy.

        private void CleanUp(ref List<ParentTextureArea> textures)
        {
            var result = new Dictionary<Hash, ParentTextureArea>();

            foreach (var tex in textures) // Do not parallelize this. For some reason it breaks everything.
            {
                var bmpHash = Hash.FromByteArray(tex.Texture.Image.ToByteArray(tex.Area));

                if (result.ContainsKey(bmpHash))
                {
                    tex.Children.ForEach(child => tex.MoveChildWithoutRepositioning(child, result[bmpHash]));
                    if (tex.TopmostAndUnpadded) result[bmpHash].TopmostAndUnpadded = true;
                }
                else
                    result.TryAdd(bmpHash, tex);
            }

            textures = result.Select(entry => entry.Value).ToList();
        }

        // Maps parent texture areas on the proposed texture map.
        // This step only prepares for actual image data layout, actual layout is done in BuildTextureMap.

        private int PlaceTexturesInMap(ref List<ParentTextureArea> textures, bool forceMinimumPadding = false)
        {
            if (textures.Count == 0)
                return 0;

            int currentPage = -1;
            List<RectPacker> texPackers = new List<RectPacker>();

            for (int i = 0; i < textures.Count; i++)
            {
                // Get the size of the quad surrounding texture area, typically should be the texture area itself
                int w = (int)(textures[i].Area.Width);
                int h = (int)(textures[i].Area.Height);

                // Calculate adaptive padding at all sides
                int padding = (_padding == 0 && forceMinimumPadding) ? _minimumPadding : _padding;

                int tP = textures[i].TopmostAndUnpadded ? 0 : padding; // Ugly, but needed for tomb4 UVRotate
                int bP = padding;
                int lP = padding;
                int rP = padding;

                int horizontalPaddingSpace = MaxTileSize - w;
                int verticalPaddingSpace = MaxTileSize - h;

                // If hor/ver padding won't fully fit, get existing space and calculate padding out of it
                if (verticalPaddingSpace < tP + bP)
                {
                    tP = (tP == 0) ? tP : verticalPaddingSpace / 2; // Ugly, but needed for tomb4 UVRotate
                    bP = verticalPaddingSpace - tP;
                }
                if (horizontalPaddingSpace < padding * 2)
                {
                    lP = horizontalPaddingSpace / 2;
                    rP = horizontalPaddingSpace - lP;
                }

                w += lP + rP;
                h += tP + bP;

                // Pack texture
                int fitPage;
                VectorInt2? pos;

                for (ushort j = 0; j <= currentPage; ++j)
                {
                    pos = texPackers[j].TryAdd(new VectorInt2(w, h));
                    if (pos.HasValue)
                    {
                        fitPage = j;
                        goto PackNextUsedTexture;
                    }
                }

                currentPage++;
                fitPage = currentPage;
                texPackers.Add(new RectPackerTree(new VectorInt2(256, 256)));
                pos = texPackers.Last().TryAdd(new VectorInt2(w, h));

            PackNextUsedTexture:
                textures[i].Padding[0] = lP;
                textures[i].Padding[1] = tP;
                textures[i].Padding[2] = rP;
                textures[i].Padding[3] = bP;
                textures[i].Page = fitPage;
                textures[i].PositionInPage = pos.Value;
            }

            return (currentPage + 1);
        }

        // Actually builds image of the texture map for the given list of parents.

        private ImageC BuildTextureMap(ref List<ParentTextureArea> textures, int numPages, bool bump, bool forceMinimumPadding = false)
        {
            var customBumpmaps = new Dictionary<string, ImageC>();
            var image = ImageC.CreateNew(256, numPages * 256 * (bump ? 2 : 1));

            var actualPadding = (_padding == 0 && forceMinimumPadding) ? _minimumPadding : _padding;

            for (int i = 0; i < textures.Count; i++)
            {
                var p = textures[i];
                var x = (int)p.Area.Start.X;
                var y = (int)p.Area.Start.Y;
                var width = (int)p.Area.Width;
                var height = (int)p.Area.Height;

                var destX = p.PositionInPage.X + p.Padding[0];
                var destY = p.Page * 256 + p.PositionInPage.Y + p.Padding[1];

                if (p.SqueezeAndDuplicate)
                {
                    // If squeeze-and-duplicate approach is needed (UVRotate), use system drawing routines
                    // to do high-quality bicubic resampling.

                    // Copy original region to new image
                    var originalImage = ImageC.CreateNew(width, height);
                    originalImage.CopyFrom(0, 0, p.Texture.Image, x, y, width, height);

                    // Make squeezed bitmap and put original one into it using bicubic resampling
                    var destBitmap = new Bitmap(width, height / 2);
                    using (var graphics = System.Drawing.Graphics.FromImage(destBitmap))
                    {
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        destBitmap.MakeTransparent();

                        using (var ia = new ImageAttributes())
                        {
                            ia.SetWrapMode(WrapMode.TileFlipXY);
                            var rect = new Rectangle(0, 0, destBitmap.Width, destBitmap.Height);
                            
                            using (var bmp = originalImage.ToBitmap())
                                graphics.DrawImage(bmp, rect, 0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, ia);
                        }
                    }

                    // Twice copy squeezed image to original image
                    var squeezedImage = ImageC.FromSystemDrawingImage(destBitmap);
                    originalImage.CopyFrom(0, 0, squeezedImage, 0, 0, width, height / 2);
                    originalImage.CopyFrom(0, height / 2, squeezedImage, 0, 0, width, height / 2);

                    // Copy squeezed-and-duplicated image to texture map and add padding
                    image.CopyFrom(destX, destY, originalImage, 0, 0, width, height);
                    AddPadding(p, originalImage, image, 0, actualPadding, 0, 0);
                }
                else
                {
                    image.CopyFrom(destX, destY, p.Texture.Image, x, y, width, height);
                    AddPadding(p, p.Texture.Image, image, 0, actualPadding);
                }

                // Do the bump map if needed

                if (p.Texture is LevelTexture)
                {
                    var tex = (p.Texture as LevelTexture);
                    var bumpX = destX;
                    var bumpY = destY + (numPages * 256);

                    // Try to copy custom bumpmaps
                    if (!string.IsNullOrEmpty(tex.BumpPath))
                    {
                        if (!customBumpmaps.ContainsKey(tex.BumpPath))
                        {
                            var path = _level.Settings.MakeAbsolute(tex.BumpPath);

                            if (!File.Exists(path))
                            {
                                _progressReporter.ReportWarn("External bumpmap file " + path + " was not found!");
                                customBumpmaps.Add(tex.BumpPath, ImageC.Black);
                            }
                            else
                            {
                                ImageC potentialBumpImage = ImageC.FromFile(path);

                                // Only assign bumpmap image if size is equal to texture image size, otherwise use dummy

                                if (potentialBumpImage.Size == tex.Image.Size)
                                    customBumpmaps.Add(tex.BumpPath, potentialBumpImage);
                                else
                                {
                                    _progressReporter.ReportWarn("Texture file '" + tex + "' has external bumpmap assigned which has different size and was ignored.");
                                    customBumpmaps.Add(tex.BumpPath, ImageC.Black);
                                }
                            }
                        }

                        image.CopyFrom(bumpX, bumpY, customBumpmaps[tex.BumpPath], x, y, width, height);
                        AddPadding(p, image, image, numPages, actualPadding, bumpX, bumpY);
                    }
                    else
                    {
                        var level = tex.GetBumpMappingLevelFromTexCoord(p.Area.GetMid());

                        if (level != BumpMappingLevel.None)
                        {
                            var bumpImage = ImageC.CreateNew(width, height);
                            bumpImage.CopyFrom(0, 0, image, destX, destY, width, height);

                            int effectWeight = 0;
                            int effectSize = 0;

                            switch (level)
                            {
                                case BumpMappingLevel.Level1:
                                    effectWeight = -2;
                                    effectSize = 2;
                                    break;
                                case BumpMappingLevel.Level2:
                                    effectWeight = -2;
                                    effectSize = 3;
                                    break;
                                case BumpMappingLevel.Level3:
                                    effectWeight = -1;
                                    effectSize = 2;
                                    break;
                            }

                            bumpImage.Emboss(0, 0, bumpImage.Width, bumpImage.Height, effectWeight, effectSize);
                            image.CopyFrom(bumpX, bumpY, bumpImage, 0, 0, width, height);
                            AddPadding(p, image, image, numPages, actualPadding, bumpX, bumpY);
                        }
                    }
                }
            }

            return image;
        }

        // Expands edge pixels to create padding which prevents border bleeding problems.

        private void AddPadding(ParentTextureArea texture, ImageC from, ImageC to, int pageOffset, int padding, int? customX = null, int? customY = null)
        {
            var p = texture;
            var x = customX.HasValue ? customX.Value : (int)p.Area.Start.X;
            var y = customY.HasValue ? customY.Value : (int)p.Area.Start.Y;
            var width = (int)p.Area.Width;
            var height = (int)p.Area.Height;
            var dataOffset = (p.Page + pageOffset) * 256;

            // Add actual padding (ported code from OT bordered_texture_atlas.cpp)

            var topLeft = from.GetPixel(x, y);
            var topRight = from.GetPixel(x + width - 1, y);
            var bottomLeft = from.GetPixel(x, y + height - 1);
            var bottomRight = from.GetPixel(x + width - 1, y + height - 1);

            for (int xP = 0; xP < padding; xP++)
            {
                // copy left line
                if (xP < p.Padding[0])
                    to.CopyFrom(p.PositionInPage.X + xP, dataOffset + p.PositionInPage.Y + p.Padding[1], from,
                               x, y, 1, height);

                // copy right line
                if (xP < p.Padding[2])
                    to.CopyFrom(p.PositionInPage.X + xP + width + p.Padding[0], dataOffset + p.PositionInPage.Y + p.Padding[1], from,
                               x + width - 1, y, 1, height);

                for (int yP = 0; yP < padding; yP++)
                {
                    // copy top line
                    if (yP < p.Padding[1])
                        to.CopyFrom(p.PositionInPage.X + p.Padding[0], dataOffset + p.PositionInPage.Y + yP, from,
                                   x, y, width, 1);
                    // copy bottom line
                    if (yP < p.Padding[3])
                        to.CopyFrom(p.PositionInPage.X + p.Padding[0], dataOffset + p.PositionInPage.Y + yP + height + p.Padding[1], from,
                                   x, y + height - 1, width, 1);

                    // expand top-left pixel
                    if (xP < p.Padding[0] && yP < p.Padding[1])
                        to.SetPixel(p.PositionInPage.X + xP, dataOffset + p.PositionInPage.Y + yP, topLeft);
                    // expand top-right pixel
                    if (xP < p.Padding[2] && yP < p.Padding[1])
                        to.SetPixel(p.PositionInPage.X + xP + width + p.Padding[0], dataOffset + p.PositionInPage.Y + yP, topRight);
                    // expand bottom-left pixel
                    if (xP < p.Padding[0] && yP < p.Padding[3])
                        to.SetPixel(p.PositionInPage.X + xP, dataOffset + p.PositionInPage.Y + yP + height + p.Padding[1], bottomLeft);
                    // expand bottom-right pixel
                    if (xP < p.Padding[2] && yP < p.Padding[3])
                        to.SetPixel(p.PositionInPage.X + xP + width + p.Padding[0], dataOffset + p.PositionInPage.Y + yP + height + p.Padding[1], bottomRight);
                }
            }
        }

        // Groups all textures by their attributes, cleans them up and builds actual texture data.

        public void LayOutAllData(TRVersion.Game version)
        {
            if (_dataHasBeenLaidOut) return;
            _dataHasBeenLaidOut = true;

            // Before any other action, lay out animated textures
            PrepareAnimatedTextures();

            // Subdivide textures in 3 blocks: room, objects, bump
            var roomTextures = new List<ParentTextureArea>();
            var objectsTextures = new List<ParentTextureArea>();
            var bumpedTextures = new List<ParentTextureArea>();

            for (int i = 0; i < _parentTextures.Count; i++)
            {
                if (_parentTextures[i].IsForRoom)
                {
                    if (_parentTextures[i].BumpLevel(_level.Settings.GameVersion) != BumpMappingLevel.None)
                        bumpedTextures.Add(_parentTextures[i]);
                    else
                        roomTextures.Add(_parentTextures[i]);
                }
                else
                    objectsTextures.Add(_parentTextures[i]);
            }

            for (int n = 0; n < _actualAnimTextures.Count; n++)
            {
                var parentTextures = _actualAnimTextures[n].CompiledAnimation;
                for (int i = 0; i < parentTextures.Count; i++)
                {
                    if (parentTextures[i].IsForRoom)
                    {
                        if (parentTextures[i].BumpLevel(_level.Settings.GameVersion) != BumpMappingLevel.None)
                            bumpedTextures.Add(parentTextures[i]);
                        else
                            roomTextures.Add(parentTextures[i]);
                    }
                    else
                        objectsTextures.Add(parentTextures[i]);
                }
            }

            // Cleanup duplicated parent areas.
            if (!_level.Settings.FastMode)
            {
                CleanUp(ref roomTextures);
                CleanUp(ref objectsTextures);
                CleanUp(ref bumpedTextures);
            }

            // Sort textures by their TopmostAndUnpadded property (waterfalls first!)
            if (_level.Settings.AgressiveTexturePacking)
                roomTextures = roomTextures.OrderBy(item => !item.TopmostAndUnpadded).ThenByDescending(item => item.Area.Size.X * item.Area.Size.Y).ToList();
            else
                objectsTextures = objectsTextures.OrderBy(item => !item.TopmostAndUnpadded).ThenByDescending(item => item.Area.Size.X * item.Area.Size.Y).ToList();

            // Calculate new X, Y of each texture area
            NumRoomPages = PlaceTexturesInMap(ref roomTextures);
            NumObjectsPages = PlaceTexturesInMap(ref objectsTextures, true);
            NumBumpPages = PlaceTexturesInMap(ref bumpedTextures);

            // Place all the textures areas in the maps
            RoomPages = BuildTextureMap(ref roomTextures, NumRoomPages, false);
            ObjectsPages = BuildTextureMap(ref objectsTextures, NumObjectsPages, false, true);
            BumpPages = BuildTextureMap(ref bumpedTextures, NumBumpPages, true);

            // Finally compile all texinfos
            BuildTextureInfos(version);
        }

        // Compiles all final texture infos into final list to be written into level file.

        private void BuildTextureInfos(TRVersion.Game version)
        {
            float maxSize = (float)MaxTileSize - (1.0f / 255.0f);

            _objectTextures = new SortedDictionary<int, ObjectTexture>();

            SortOutAlpha(version, _parentTextures);
            foreach (var parent in _parentTextures)
                foreach (var child in parent.Children)
                    if (!_objectTextures.ContainsKey(child.TexInfoIndex))
                    {
                        var newObjectTexture = new ObjectTexture(parent, child, version, maxSize);
#if DEBUG
                        if (newObjectTexture.TexCoord[0] == newObjectTexture.TexCoord[1] ||
                            newObjectTexture.TexCoord[0] == newObjectTexture.TexCoord[2] ||
                            newObjectTexture.TexCoord[1] == newObjectTexture.TexCoord[2] ||
                            newObjectTexture.TexCoord[0].X < 0 || newObjectTexture.TexCoord[0].Y < 0 ||
                            newObjectTexture.TexCoord[1].X < 0 || newObjectTexture.TexCoord[1].Y < 0 ||
                            newObjectTexture.TexCoord[2].X < 0 || newObjectTexture.TexCoord[2].Y < 0 ||
                            (!child.IsForTriangle && newObjectTexture.TexCoord[0] == newObjectTexture.TexCoord[3]) ||
                            (!child.IsForTriangle && newObjectTexture.TexCoord[1] == newObjectTexture.TexCoord[3]) ||
                            (!child.IsForTriangle && newObjectTexture.TexCoord[2] == newObjectTexture.TexCoord[3]) ||
                            (!child.IsForTriangle && (newObjectTexture.TexCoord[3].X < 0 || newObjectTexture.TexCoord[3].Y < 0)))
                        {
                            _progressReporter.ReportWarn("Compiled TexInfo " + child.TexInfoIndex + " is broken, coordinates are invalid.");
                        }
#endif
                        _objectTextures.Add(child.TexInfoIndex, newObjectTexture);
                    }

            foreach (var animTexture in _actualAnimTextures)
            {
                SortOutAlpha(version, animTexture.CompiledAnimation);
                foreach (var parent in animTexture.CompiledAnimation)
                    foreach (var child in parent.Children)
                        if (!_objectTextures.ContainsKey(child.TexInfoIndex))
                            _objectTextures.Add(child.TexInfoIndex, new ObjectTexture(parent, child, version, maxSize));
            }
        }

        // Sorts animated textures and prepares index table to layout them later.
        // We need to build index table in advance of CleanUp() call, because after that step animated texture order
        // gets shuffled.

        private void PrepareAnimatedTextures()
        {
            // Put UVRotate sequences first
            _actualAnimTextures = _actualAnimTextures.OrderBy(item => !item.Origin.IsUvRotate).ToList();

            // Build index table
            _animTextureIndices = new List<List<ushort>>();
            foreach (var compiledAnimatedTexture in _actualAnimTextures)
            {
                var list = new List<ushort>();
                var orderedFrameList = compiledAnimatedTexture.CompiledAnimation.SelectMany(x => x.Children).OrderBy(c => c.TexInfoIndex).ToList();
                foreach (var frame in orderedFrameList)
                    list.Add((ushort)frame.TexInfoIndex);

                _animTextureIndices.Add(list);
            }
        }

        public void WriteAnimatedTextures(BinaryWriterEx writer)
        {
            int numAnimatedTextures = 1;
            foreach (var list in _animTextureIndices)
                numAnimatedTextures += list.Count + 1;
            writer.Write((uint)numAnimatedTextures);

            writer.Write((ushort)_animTextureIndices.Count);
            foreach (var list in _animTextureIndices)
            {
                writer.Write((ushort)(list.Count - 1));
                foreach (var frame in list)
                    writer.Write((ushort)frame);
            }
        }

        public void WriteTextureInfos(BinaryWriterEx writer, Level level)
        {
            writer.Write((int)_objectTextures.Count);
            for (int i = 0; i < _objectTextures.Count; i++)
            {
                var texture = _objectTextures.ElementAt(i).Value;

                // Tile and flags
                ushort tile = (ushort)texture.Tile;
                if (texture.IsForTriangle && level.Settings.GameVersion.Native() > TRVersion.Game.TR3) tile |= 0x8000;

                // Blend mode
                ushort attribute = (ushort)texture.BlendMode;

                // Clamp blend modes according to game version
                if (level.Settings.GameVersion <= TRVersion.Game.TR2 && attribute > 1)
                    attribute = 1;
                if ((level.Settings.GameVersion == TRVersion.Game.TR3 || level.Settings.GameVersion == TRVersion.Game.TR5
                    || level.Settings.GameVersion == TRVersion.Game.TR1X || level.Settings.GameVersion == TRVersion.Game.TR2X) && attribute > 2)
                    attribute = 2;

                // Now write the texture
                writer.Write(attribute);
                writer.Write(tile);

                // New flags from >= TR4
                if (level.Settings.GameVersion.Native() >= TRVersion.Game.TR4)
                {
                    // Built-in TR4-5 mapping correction is not used. Dummy mapping type is used
                    // together with compensation coordinate distortion.
                    ushort newFlags = texture.UVAdjustmentFlag;

                    if (texture.IsForRoom) newFlags |= 0x8000;

                    if      (texture.BumpLevel == BumpMappingLevel.Level1) newFlags |= (1 << 9);
                    else if (texture.BumpLevel == BumpMappingLevel.Level2) newFlags |= (2 << 9);
                    else if (texture.BumpLevel == BumpMappingLevel.Level3) newFlags |= (3 << 9);

                    writer.Write(newFlags);
                }

                for (int j = 0; j < 4; j++)
                {
                    if (texture.IsForTriangle && j == 3)
                    {
                        writer.Write((ushort)0);
                        writer.Write((ushort)0);
                    }
                    else
                    {
                        writer.Write((ushort)(texture.TexCoord[j].X));
                        writer.Write((ushort)(texture.TexCoord[j].Y));
                    }
                }

                if (level.Settings.GameVersion.Native() >= TRVersion.Game.TR4)
                {
                    var rect = texture.GetRect();
                    writer.Write((int)0);
                    writer.Write((int)0);
                    writer.Write(rect.Width - 1);
                    writer.Write(rect.Height - 1);
                }

                if (level.Settings.GameVersion == TRVersion.Game.TR5)
                    writer.Write((ushort)0);
            }
        }

        public void UpdateTiles(int numSpritesPages)
        {
            Parallel.For(0, _objectTextures.Count, i =>
            {
                var texture = _objectTextures.ElementAt(i).Value;
                if (texture.IsForRoom && texture.BumpLevel == BumpMappingLevel.None)
                {
                    // Tile is OK
                }
                else if (!texture.IsForRoom)
                {
                    texture.Tile += NumRoomPages;
                }
                else if (texture.IsForRoom && texture.BumpLevel != BumpMappingLevel.None)
                {
                    texture.Tile += NumRoomPages + NumObjectsPages + numSpritesPages;
                }
            });

            NumObjectsPages += numSpritesPages;
        }
    }
}
