using NLog;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.LevelData.Compilers.TombEngine;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.LevelData.Compilers
{
	public enum TextureDestination
    {
        RoomOrAggressive,
        Moveable,
        Static
    }

    public class TombEngineTexInfoManager
    {
        [ThreadStatic]
        private static HashSet<int>? _candidatePool;

        private static HashSet<int> GetCandidateSet()
        {
            if (_candidatePool == null)
                _candidatePool = new HashSet<int>(128);
            else
                _candidatePool.Clear();

            return _candidatePool;
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const int _noTexInfo = -1;
        private const int _dummyTexInfo = -2;
        private const int _minimumPadding = 1;
        private const int _minimumTileSize = 4096;
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

        private List<ParentTextureArea> _parentRoomTextureAreas = new List<ParentTextureArea>();
        private List<ParentTextureArea> _parentMoveableTextureAreas = new List<ParentTextureArea>();
        private List<ParentTextureArea> _parentStaticTextureAreas = new List<ParentTextureArea>();

        // MaxTileSize defines maximum size to which parent can be inflated by incoming child, if
        // inflation is allowed.

        private ushort MaxTileSize = _minimumTileSize;

        // If padding value is 0, 1 px padding will be forced on object textures anyway,
        // because yet we don't have a mechanism to specify UV adjustment in converted WADs.

        private ushort _padding = 8;

        // TexInfoCount is internally a "reference counter" which is also used to get new TexInfo IDs.
        // Since generation of TexInfos is an one-off serialized process, we can safely use it in
        // serial manner as well.

        public int TexturesCount { get; private set; } = 0;

        // Final texture pages 
        public List<TombEngineAtlas> RoomsAtlas { get; private set; }
        public List<TombEngineAtlas> MoveablesAtlas { get; private set; }
        public List<TombEngineAtlas> StaticsAtlas { get; private set; }
        public List<TombEngineAtlas> AnimatedAtlas { get; private set; }


        // Precompiled object textures are kept in this dictionary.
        private SortedDictionary<int, ObjectTexture> _objectTextures;

        // Precompiled anim texture indices are kept separately to avoid
        // messing up after texture page cleanup.
        private List<List<int>> _animTextureIndices;

        // Expose the latter publicly, because TRNG compiler needs it
        public ReadOnlyCollection<KeyValuePair<AnimatedTextureSet, ReadOnlyCollection<int>>> AnimatedTextures
        {
            get
            {
                var result = new List<KeyValuePair<AnimatedTextureSet, ReadOnlyCollection<int>>>();
                for (int i = 0; i < _animTextureIndices.Count; i++)
                    result.Add(new KeyValuePair<AnimatedTextureSet, ReadOnlyCollection<int>>
                        (_actualAnimTextures[i].Origin, _animTextureIndices[i].AsReadOnly()));
                return result.AsReadOnly();
            }
        }

        public class TexturePage
        {
            public int Atlas { get; set; }
            public VectorInt2 Position { get; set; }
            public ImageC ColorMap { get; set; }
            public ImageC NormalMap { get; set; }
            public bool HasNormalMap;
        }

        // ChildTextureArea is a simple enclosed relative texture area with stripped down parameters
        // which should be the same among all children of same parent.
        // Stripped down parameters include BumpLevel and IsForTriangle, because if these are
        // different, it automatically means we should assign new parent for this child.

        public class ChildTextureArea
        {
            public int TextureId;
            public Vector2[] RelCoord;  // Relative to parent!
            public Vector2[] AbsCoord; // Absolute

            public BlendMode BlendMode;
            public bool IsForTriangle;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            public int AtlasIndex { get; set; }
            public VectorInt2 AtlasDimensions { get; set; }
            public int[] Padding { get; set; } = new int[4]; // LTRB
            public Texture Texture { get; private set; }
            public TextureDestination Destination { get; set; }
            public BlendMode BlendMode { get; set; }


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

            private const int DefaultCellSize = 64;
            private int _cellSize = DefaultCellSize;

            private readonly Dictionary<int, HashSet<int>> _grid = new Dictionary<int, HashSet<int>>(256);
            private readonly Dictionary<int, HashSet<int>> _childCells = new Dictionary<int, HashSet<int>>(256);
            
            private static readonly ArrayPool<int> _childPool = ArrayPool<int>.Shared;

            public int Version { get; set; }

            // Generates new ParentTextureArea from raw texture coordinates.
            public ParentTextureArea(TextureArea texture, TextureDestination destination)
            {
                // Round area to nearest pixel to prevent rounding errors further down the line.
                // Use ParentArea to create a parent for textures which were applied with group texturing tools.
                _area = texture.ParentArea.IsZero ? texture.GetRect().Round() : texture.ParentArea.Round();
                Initialize(texture.Texture, destination);
            }

            // Pack cell key (cx,cy) -> int
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int PackCellKey(int cx, int cy) => (cx << 16) ^ (cy & 0xFFFF);

            // Bounding box from the child
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static Rectangle2 GetChildRect(ChildTextureArea c) => c.GetRect();

            // Get the cells touched by a rectangle
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void GetCellRange(Rectangle2 r, out int cx0, out int cx1, out int cy0, out int cy1)
            {
                // Pixel-aligned coordinates
                int x0 = (int)MathF.Floor(r.X0);
                int y0 = (int)MathF.Floor(r.Y0);
                int x1 = (int)MathF.Ceiling(r.X1) - 1; // inclusive
                int y1 = (int)MathF.Ceiling(r.Y1) - 1;

                cx0 = x0 / _cellSize;
                cy0 = y0 / _cellSize;
                cx1 = x1 / _cellSize;
                cy1 = y1 / _cellSize;
            }

            private void IndexChild(int childIndex)
            {
                var rect = GetChildRect(Children[childIndex]);
                GetCellRange(rect, out int cx0, out int cx1, out int cy0, out int cy1);

                // Create an HashSet with touched cells
                var cells = _childPool.Rent(Math.Max(1, (cx1 - cx0 + 1) * (cy1 - cy0 + 1)));
                int count = 0;

                for (int cy = cy0; cy <= cy1; cy++)
                {
                    for (int cx = cx0; cx <= cx1; cx++)
                    {
                        int key = PackCellKey(cx, cy);
                        if (!_grid.TryGetValue(key, out var list))
                        {
                            list = new HashSet<int>();
                            _grid[key] = list;
                        }

                        // Add index's child to the cell
                        list.Add(childIndex);
                        cells[count++] = key;
                    }
                }

                _childCells[childIndex] = new HashSet<int>(cells.Take(count));

                _childPool.Return(cells);
            }

            private void UnindexChild(int childIndex)
            {
                if (!_childCells.TryGetValue(childIndex, out var cells))
                    return;

                // Use ArrayPool for avoiding allocations
                var cellsArray = cells.ToArray();
                foreach (var key in cellsArray)
                {
                    if (_grid.TryGetValue(key, out var list))
                    {
                        // Remove the child from the list
                        list.Remove(childIndex);
                        if (list.Count == 0)
                            _grid.Remove(key);
                    }
                }
                _childCells.Remove(childIndex);
            }

            private void ResetGridIndex()
            {
                _grid.Clear();
                _childCells.Clear();
                Version++;
            }

            // Generates new ParentTextureArea from given area in texture.
            public ParentTextureArea(Rectangle2 area, Texture texture, TextureDestination destination)
            {
                _area = area;
                Initialize(texture, destination);
            }

            private void Initialize(Texture texture, TextureDestination destination)
            {
                Children = new List<ChildTextureArea>();

                Texture = texture;
                Destination = destination;
            }

            // Compare parent's properties with incoming texture properties.
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public bool ParametersSimilar(TextureArea incomingTexture, TextureDestination destination)
            {
                // Are textures for the same destination?
                if (Destination != destination) return false;

                var t1 = incomingTexture.Texture;
                var t2 = Texture;

                // Are textures the same .NET instance?
                if (ReferenceEquals(t1, t2))
                    return true;

                // Have textures the same absolute path if present?
                if (!string.IsNullOrEmpty(t1.AbsolutePath) && !string.IsNullOrEmpty(t2.AbsolutePath))
                    return string.Equals(t1.AbsolutePath, t2.AbsolutePath, StringComparison.OrdinalIgnoreCase);

                // Are textures hashed and have same hash? (For embedded WadTexture only)
                if (t1 is TextureHashed h1)
                    return t2 is TextureHashed h2 && h1.Hash == h2.Hash;

                if (t2 is TextureHashed) 
                    return false;

                // Fallback to GetHashCode comparison (it shouldn't happen for most of the situations however)
                return t1.GetHashCode() == t2.GetHashCode();
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

                    // Precompute hash
                    var hash1 = texture.Texture.Image.GetHashOfAreaFast(rr);

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
                        var hash2 = Texture.Image.GetHashOfAreaFast(r);

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
            public BumpMappingLevel BumpLevel()
            {
                if (Texture is LevelTexture)
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
            public bool IsPotentialParent(TextureArea texture, TextureDestination destination, bool allowOverlaps, uint maxOverlappedSize)
            {
                var rect = texture.GetRect();

                if (ParametersSimilar(texture, destination))
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
            public bool IsPotentialChild(TextureArea texture, TextureDestination destination)
                => (ParametersSimilar(texture, destination) && texture.GetRect().Round().Contains(_area));

            // Adds texture as a child to existing parent, with recalculating coordinates to relative.
            public void AddChild(TextureArea texture, int newTextureID, bool isForTriangle, BlendMode blendMode)
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
                    TextureId = newTextureID,
                    BlendMode = texture.BlendMode,
                    IsForTriangle = isForTriangle,
                    RelCoord = relative,
                    AbsCoord = absolute
                });

                IndexChild(Children.Count - 1);

                // Expand parent area, if needed
                var rect = texture.GetRect();
                if (!Area.Contains(rect))
                    Area = Area.Union(rect).Round();
            }

            // Moves child to another parent. This is intended to work only within same texture set.
            public void MoveChildByIndex(int childIndex, ParentTextureArea newParent)
            {
                var child = Children[childIndex];

                UnindexChild(childIndex);
                int last = Children.Count - 1;
                if (childIndex != last) Children[childIndex] = Children[last];
                Children.RemoveAt(last);

                var newRel = new Vector2[child.AbsCoord.Length];
                for (int i = 0; i < newRel.Length; i++)
                    newRel[i] = child.AbsCoord[i] - newParent.Area.Start;

                newParent.Children.Add(new ChildTextureArea
                {
                    TextureId = child.TextureId,
                    BlendMode = child.BlendMode,
                    IsForTriangle = child.IsForTriangle,
                    RelCoord = newRel,
                    AbsCoord = child.AbsCoord
                });
                newParent.IndexChild(newParent.Children.Count - 1);
                newParent.Version++;
                Version++;
            }

            public void MoveChild(ChildTextureArea child, ParentTextureArea newParent)
            {
                // se preferisci questo overload: trova index, poi delega
                int idx = Children.IndexOf(child);
                if (idx >= 0) MoveChildByIndex(idx, newParent);
            }

            // Moves child to another parent by absolute coordinate. This is intended to work between texture sets.
            public void MoveChildWithoutRepositioning(ChildTextureArea child, ParentTextureArea newParent)
            {
                var newAbsCoord = new Vector2[child.AbsCoord.Length];
                for (int i = 0; i < newAbsCoord.Length; i++)
                    newAbsCoord[i] = child.RelCoord[i] + newParent.Area.Start;

                newParent.Children.Add(new ChildTextureArea()
                {
                    TextureId = child.TextureId,
                    BlendMode = child.BlendMode,
                    IsForTriangle = child.IsForTriangle,
                    RelCoord = child.RelCoord,
                    AbsCoord = newAbsCoord
                });

                newParent.IndexChild(newParent.Children.Count - 1);
                newParent.Version++;
            }

            public void MergeParents(List<ParentTextureArea> parentList, List<ParentTextureArea> parents)
            {
                foreach (var parent in parents)
                {
                    Area = Area.Union(parent.Area);
                    BlendMode = parent.BlendMode;

                    for (int i = parent.Children.Count - 1; i >= 0; i--)
                        parent.MoveChildByIndex(i, this);

                    parent.ResetGridIndex();
                }
                parents.ForEach(item => parentList.Remove(item));
                Version++;
            }

            public int CollectCandidates(Rectangle2 rect, HashSet<int> outIndices)
            {
                GetCellRange(rect, out int cx0, out int cx1, out int cy0, out int cy1);

                // Usa HashSet per raccogliere gli indici senza duplicati
                HashSet<int> seen = new HashSet<int>();

                // Loop attraverso tutte le celle che potrebbero essere toccate dal rettangolo
                for (int cy = cy0; cy <= cy1; cy++)
                {
                    for (int cx = cx0; cx <= cx1; cx++)
                    {
                        int key = PackCellKey(cx, cy);
                        if (!_grid.TryGetValue(key, out var list)) continue;

                        // Aggiungi ogni indice alla lista di candidati se non è già stato aggiunto
                        foreach (var index in list)
                        {
                            if (seen.Add(index)) // Aggiungi e verifica se è stato aggiunto
                            {
                                outIndices.Add(index);
                            }
                        }
                    }
                }

                return outIndices.Count;
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
                    var newParent = new ParentTextureArea(parent.Area, parent.Texture, parent.Destination);

                    foreach (var child in parent.Children)
                    {
                        var newChild = new ChildTextureArea()
                        {
                            BlendMode = child.BlendMode,
                            IsForTriangle = child.IsForTriangle,
                            TextureId = child.TextureId
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

        public class ObjectTexture
        {
            public VectorInt2[] TexCoord = new VectorInt2[4];
            public Vector2[] TexCoordFloat = new Vector2[4];

            public bool IsForTriangle;
            public TextureDestination Destination;
            public BlendMode BlendMode;
            public BumpMappingLevel BumpLevel;

            public int AtlasIndex;
            public VectorInt2 AtlasDimensions;

            public ObjectTexture(ParentTextureArea parent, ChildTextureArea child, float maxTextureSize)
            {
                BlendMode = child.BlendMode;
                BumpLevel = parent.BumpLevel();
                Destination = parent.Destination;
                IsForTriangle = child.IsForTriangle;

                for (int i = 0; i < child.RelCoord.Length; i++)
                {
                    var coord = new Vector2(child.RelCoord[i].X + (float)(parent.PositionInPage.X + parent.Padding[0]),
                                            child.RelCoord[i].Y + (float)(parent.PositionInPage.Y + parent.Padding[1]));

                    // Clamp coordinates that are possibly out of bounds
                    coord.X = (float)MathC.Clamp(coord.X, 0, maxTextureSize);
                    coord.Y = (float)MathC.Clamp(coord.Y, 0, maxTextureSize);

                    AtlasIndex = parent.AtlasIndex;
                    AtlasDimensions = parent.AtlasDimensions;

                    // Float coordinates must be in 0.0f ... 1.0f range
                    TexCoordFloat[i] = coord;
                    TexCoordFloat[i].X /= (float)AtlasDimensions.X;
                    TexCoordFloat[i].Y /= (float)AtlasDimensions.Y;

                    // Pack coordinates into 2-byte set (whole and frac parts)
                    TexCoord[i] = new VectorInt2((((int)Math.Truncate(coord.X)) << 8) + (int)(Math.Floor(coord.X % 1.0f * (float)(maxTextureSize - 1))),
                                                 (((int)Math.Truncate(coord.Y)) << 8) + (int)(Math.Floor(coord.Y % 1.0f * (float)(maxTextureSize - 1))));
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

        public TombEngineTexInfoManager(Level level, IProgressReporter progressReporter, int maxTileSize = -1)
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
                MaxTileSize = (ushort)_minimumTileSize;
            }

            GenerateAnimLookups(_level.Settings.AnimatedTextureSets, TextureDestination.RoomOrAggressive);  // Generate anim texture lookup table

            foreach (var wad in _level.Settings.Wads)
            {
                var usedMoveablesTextures = wad.Wad.Moveables
                    .SelectMany(m => m.Value.Meshes)
                    .SelectMany(msh => msh.Polys)
                    .Select(p => p.Texture.Texture)
                    .Distinct()
                    .ToList();

                GenerateAnimLookups(wad.Wad.AnimatedTextureSets
                    .Where(s => s.Frames.Any(f => usedMoveablesTextures.Contains(f.Texture)))
                    .ToList(), TextureDestination.Moveable);

                var usedStaticsTextures = wad.Wad.Statics
                   .Select(m => m.Value.Mesh)
                   .SelectMany(m => m.Polys)
                   .Select(p => p.Texture.Texture)
                   .Distinct()
                   .ToList();

                GenerateAnimLookups(wad.Wad.AnimatedTextureSets
                    .Where(s => s.Frames.Any(f => usedStaticsTextures.Contains(f.Texture)))
                    .ToList(), TextureDestination.Static);
            }

            _generateTexInfos = true;    // Set manager ready state 
        }

        // Gets free TexInfo index
        private int GetNewTextureId()
        {
            if (_generateTexInfos)
            {
                int result = TexturesCount;
                TexturesCount++;
                return result;
            }
            else
                return _dummyTexInfo;
        }

        // Try to add texture to existing parent(s) either as a child of one, or as a parent, merging
        // enclosed parents.

        private bool TryToAddToExisting(TextureArea texture, List<ParentTextureArea> parentList, TextureDestination destination, bool isForTriangle, BlendMode blendMode, int animFrameIndex = -1)
        {
            // Try to find potential parent (larger texture) and add itself to children
            foreach (var parent in parentList)
            {
                if (!parent.IsPotentialParent(texture, destination, animFrameIndex >= 0, MaxTileSize))
                    continue;

                parent.AddChild(texture, animFrameIndex >= 0 ? animFrameIndex : GetNewTextureId(), isForTriangle, blendMode);
                return true;
            }

            // Try to find and merge parents which are enclosed in incoming texture area
            var childrenWannabes = parentList.Where(item => item.IsPotentialChild(texture, destination)).ToList();
            if (childrenWannabes.Count > 0)
            {
                var newParent = new ParentTextureArea(texture, destination);
                newParent.AddChild(texture, animFrameIndex >= 0 ? animFrameIndex : GetNewTextureId(), isForTriangle, blendMode);
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

            public bool Animated;
            public int AnimatedSequence;
            public int AnimatedFrame;

            // This value indicates that if used on triangle, it must be converted to
            // degenerate quad. It's needed to fake UVRotate application to triangular areas.
            public bool ConvertToQuad;

            public TombEnginePolygon CreateTombEnginePolygon3(int[] indices, BlendMode blendMode, int materialIndex, List<TombEngineVertex> vertices)
            {
                if (indices.Length != 3)
                    throw new ArgumentOutOfRangeException(nameof(indices.Length));

                int objectTextureIndex = TexInfoIndex;
                int[] transformedIndices = new int[3] { indices[0], indices[1], indices[2] };

                if (Rotation > 0)
                {
                    for (int i = 0; i < Rotation; i++)
                    {
                        int tempIndex = transformedIndices[0];
                        transformedIndices[0] = transformedIndices[2];
                        transformedIndices[2] = transformedIndices[1];
                        transformedIndices[1] = tempIndex;
                    }
                }

                var polygon = new TombEnginePolygon();
                polygon.Shape = TombEnginePolygonShape.Triangle;
                polygon.Indices.AddRange(transformedIndices);
                polygon.TextureId = objectTextureIndex;
                polygon.MaterialIndex = materialIndex;
                polygon.BlendMode = (byte)blendMode;
                polygon.Animated = Animated;

                if (vertices != null)
                {
                    // Calculate the normal
                    var e1 = vertices[polygon.Indices[1]].Position - vertices[polygon.Indices[0]].Position;
                    var e2 = vertices[polygon.Indices[2]].Position - vertices[polygon.Indices[0]].Position;
                    polygon.Normal = Vector3.Normalize(Vector3.Cross(e1, e2));
                }

                return polygon;
            }

            public TombEnginePolygon CreateTombEnginePolygon4(int[] indices, BlendMode blendMode, int materialIndex, List<TombEngineVertex> vertices)
            {
                if (indices.Length != 4)
                    throw new ArgumentOutOfRangeException(nameof(indices.Length));

                int objectTextureIndex = TexInfoIndex;
                int[] transformedIndices = new int[4] { indices[0], indices[1], indices[2], indices[3] };

                if (Rotation > 0)
                {
                    for (int i = 0; i < Rotation; i++)
                    {
                        int tempIndex = transformedIndices[0];
                        transformedIndices[0] = transformedIndices[3];
                        transformedIndices[3] = transformedIndices[2];
                        transformedIndices[2] = transformedIndices[1];
                        transformedIndices[1] = tempIndex;
                    }
                }

                var polygon = new TombEnginePolygon();
                polygon.Shape = TombEnginePolygonShape.Quad;
                polygon.Indices.AddRange(transformedIndices);
                polygon.TextureId = objectTextureIndex;
                polygon.MaterialIndex = materialIndex;
                polygon.BlendMode = (byte)blendMode;
                polygon.Animated = Animated;

                if (vertices != null)
                {
                    // Calculate the normal
                    Vector3 e1 = vertices[polygon.Indices[1]].Position - vertices[polygon.Indices[0]].Position;
                    Vector3 e2 = vertices[polygon.Indices[2]].Position - vertices[polygon.Indices[0]].Position;
                    polygon.Normal = Vector3.Normalize(Vector3.Cross(e1, e2));
                }

                return polygon;
            }
        }

        /// <summary>
        /// Tries to find an existing texture info (TexInfo) that matches the given texture area,
        /// optionally taking into account whether the area is a triangle or a quad, blend mode,
        /// and a UV similarity margin.
        /// 
        /// The search is performed across a list of parent texture areas, each of which can hold
        /// multiple children (candidate TexInfos). The method:
        /// 1. Normalizes the UV coordinates for the incoming area.
        /// 2. For each parent whose parameters are compatible:
        ///    - Builds a bounding rectangle from the area UVs.
        ///    - Queries a spatial index (grid) to quickly find candidate children.
        ///    - For those candidates, compares UVs using <see cref="TestUVSimilarity"/>.
        ///    - If no candidate from the grid matches, performs a full scan over all children.
        /// 3. If a match is found, it updates some bookkeeping info on the parent and returns
        ///    a <see cref="Result"/> containing the TexInfo index and the rotation needed
        ///    to align the coordinates.
        /// 4. If no match is found in any parent, returns null.
        /// </summary>
        /// <param name="areaToLook">
        /// The texture area we are trying to match against existing TexInfos.
        /// Contains UVs, texture reference, blend mode, etc.
        /// </param>
        /// <param name="parentList">
        /// List of parent texture areas. Each parent groups several children (TexInfos)
        /// and may also maintain a spatial grid index for fast lookup.
        /// </param>
        /// <param name="destination">
        /// The texture destination (atlas / target texture) where the area will be placed.
        /// Used to compare parameters between the incoming area and a parent.
        /// </param>
        /// <param name="isForTriangle">
        /// If true, the area is treated as a triangle (3 UV coordinates).
        /// If false, it is treated as a quad (4 UV coordinates).
        /// </param>
        /// <param name="blendMode">
        /// Blend mode that will be applied to the parent if a matching child is found.
        /// Also used as a filtering criterion when <paramref name="checkParameters"/> is true.
        /// </param>
        /// <param name="checkParameters">
        /// If true, candidate children must match the blend mode (and possibly other parameters)
        /// of <paramref name="areaToLook"/> before being considered as UV matches.
        /// If false, only UV similarity is checked.
        /// </param>
        /// <param name="lookupMargin">
        /// Allowed margin for UV similarity. Passed to <see cref="TestUVSimilarity"/> as the
        /// maximum squared distance threshold (after squaring inside that method).
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> containing the matched TexInfo index and rotation if a match is found,
        /// or null if no compatible TexInfo exists in the provided parents.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private Result? GetTexInfo(
            TextureArea areaToLook,
            List<ParentTextureArea> parentList,
            TextureDestination destination,
            bool isForTriangle,
            BlendMode blendMode,
            bool checkParameters = true,
            float lookupMargin = 0.0f)
        {
            // Decide how many UV coordinates we expect based on primitive type:
            //  - 3 for triangles
            //  - 4 for quads
            int coordCount = isForTriangle ? 3 : 4;

            // Stackallocate a small buffer for the UVs of the area we are trying to match.
            // Using Span + stackalloc avoids heap allocations in this hot path.
            Span<Vector2> lookupCoordinates = stackalloc Vector2[coordCount];
            for (int i = 0; i < coordCount; i++)
                lookupCoordinates[i] = areaToLook.GetTexCoord(i);

            // Build a bounding rectangle in UV space for the lookup area.
            // For triangles, we compute a rectangle that encloses the 3 UVs.
            // For quads, the rectangle encloses all 4 UVs.
            Rectangle2 rect = isForTriangle
                ? Rectangle2.FromCoordinates(lookupCoordinates[0], lookupCoordinates[1], lookupCoordinates[2])
                : Rectangle2.FromCoordinates(lookupCoordinates[0], lookupCoordinates[1], lookupCoordinates[2], lookupCoordinates[3]);

            // Preallocate this
            var candidates = GetCandidateSet();

            // Iterate over all parents by reference (CollectionsMarshal.AsSpan avoids
            // Enumerator overhead and allows direct ref access to list elements).
            foreach (ref var parent in CollectionsMarshal.AsSpan(parentList))
            {
                // Quickly skip parents whose parameters are not compatible
                // with the target area and destination. 
                if (!parent.ParametersSimilar(areaToLook, destination))
                    continue;

                // --- Phase 1: use the grid / spatial index to get a reduced candidate set ---

                // Collect candidate child indices whose bounding rectangles overlap 'rect'.
                // This is a fast pre-filter: it reduces the number of expensive UV comparisons.
                candidates.Clear();
                parent.CollectCandidates(rect, candidates);

                if (candidates.Count > 0)
                {
                    var children = parent.Children;

                    // Check each candidate from the spatial index.
                    foreach (var candidateIndex in candidates)
                    {
                        var child = children[candidateIndex];

                        // Skip if the primitive type (triangle vs quad) does not match.
                        if (child.IsForTriangle != isForTriangle)
                            continue;

                        // Optionally require a blend mode match (and other parameters) before checking UVs.
                        if (checkParameters && areaToLook.BlendMode != child.BlendMode)
                            continue;

                        // Compare UVs with optional rotation using the SIMD-optimized UV similarity test.
                        int rot = TestUVSimilarity(child.AbsCoord, lookupCoordinates, lookupMargin);
                        if (rot != _noTexInfo)
                        {
                            // We found a matching TexInfo.
                            // Update the parent's blend mode to the requested one.
                            parent.BlendMode = blendMode;

                            // If the texture is the same and the incoming area has a non-zero parent area,
                            // propagate that parent area to the parent. This can be used to track
                            // hierarchy or aggregated area information.
                            if (areaToLook.Texture == parent.Texture && !areaToLook.ParentArea.IsZero)
                                parent.Area = areaToLook.ParentArea;

                            // Return the matched TexInfo index and the rotation needed to align UVs.
                            return new Result
                            {
                                TexInfoIndex = child.TextureId,
                                Rotation = (byte)rot
                            };
                        }
                    }
                }

                // --- Phase 2: fallback – linear scan over all children ---
                // This covers cases where the grid did not return any candidates
                // (e.g., small or misaligned rectangles), or where all candidates failed.
                for (int i = 0; i < parent.Children.Count; i++)
                {
                    var child = parent.Children[i];

                    // Again, ensure primitive type compatibility.
                    if (child.IsForTriangle != isForTriangle)
                        continue;

                    // Optional parameter check (e.g. matching blend mode).
                    if (checkParameters && areaToLook.BlendMode != child.BlendMode)
                        continue;

                    // Test UV similarity for this child.
                    int rot = TestUVSimilarity(child.AbsCoord, lookupCoordinates, lookupMargin);
                    if (rot != _noTexInfo)
                    {
                        // Matching TexInfo found in the fallback path.
                        parent.BlendMode = blendMode;

                        if (areaToLook.Texture == parent.Texture && !areaToLook.ParentArea.IsZero)
                            parent.Area = areaToLook.ParentArea;

                        return new Result
                        {
                            TexInfoIndex = child.TextureId,
                            Rotation = (byte)rot
                        };
                    }
                }
            }

            // No matching TexInfo found among any parent.
            return null;
        }

		/// </returns>
		/// <summary>
		/// Tests whether two small UV sets (triangles or quads) are similar within a given margin,
		/// optionally allowing for cyclic rotation of the vertices.
		/// 
		/// Returns:
		/// - 0  -> UVs are similar with no rotation.
		/// - N  -> UVs are similar, but the 'second' set is rotated N steps relative to 'first'
		///         (encoded as 4 - rotation for quads, 3 - rotation for triangles).
		/// - _noTexInfo -> UVs are not similar (different size, too far apart, or no rotation matches).
		/// </summary>
		/// <param name="first">
		/// First UV set, as a plain array (must contain either 3 or 4 elements).
		/// This set is treated as the canonical orientation.
		/// </param>
		/// <param name="second">
		/// Second UV set, as a read-only span (must have the same length as <paramref name="first"/>).
		/// The method will try all cyclic rotations of this set.
		/// </param>
		/// <param name="lookupMargin">
		/// Maximum allowed Euclidean distance between corresponding UVs, *summed* over all vertices.
		/// The comparison is done on squared distances, so this value is squared internally.
		/// </param>
		/// <returns>
		/// An integer encoding the rotation needed to align the second UV set to the first,
		/// or _noTexInfo if no match is found.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private int TestUVSimilarity(Vector2[] first, ReadOnlySpan<Vector2> second, float lookupMargin)
		{
			int len = first.Length;

			// Only 3 or 4 vertices are supported.
			// Conditions:
			//  - lengths must match
			//  - (uint)(len - 3) > 1 is a compact range check:
			//      len == 3 -> len - 3 = 0  -> (uint)0 > 1?  false
			//      len == 4 -> len - 3 = 1  -> (uint)1 > 1?  false
			//      any other len -> result is either very large (for len < 3) or >= 2 (for len > 4),
			//                       so the condition becomes true.
			if (len != second.Length || (uint)(len - 3) > 1)
				return _noTexInfo;

			// Work with squared distances to avoid the cost of square roots.
			// The caller specifies a distance in UV units, but internally we compare
			// against lookupMargin^2.
			float marginSquared = lookupMargin * lookupMargin;

			// --- Quad path (4 vertices) -------------------------------------------------
			if (len == 4)
			{
				// Try all 4 cyclic rotations of the "second" set.
				// rotation = 0 -> (0,1,2,3)
				// rotation = 1 -> (1,2,3,0)
				// rotation = 2 -> (2,3,0,1)
				// rotation = 3 -> (3,0,1,2)
				for (int rotation = 0; rotation < 4; rotation++)
				{
					float sum = 0.0f;

					// Accumulate the squared distance for all 4 vertices
					// under the current rotation.
					for (int i = 0; i < 4; i++)
					{
						// (i + rotation) & 3 is equivalent to (i + rotation) % 4,
						// but avoids the modulo operator.
						int idx = (i + rotation) & 3;

						var f = first[i];
						var s = second[idx];

						float dx = f.X - s.X;
						float dy = f.Y - s.Y;

						sum += dx * dx + dy * dy;
					}

					// If the total squared distance is within the allowed margin,
					// the two UV sets are considered equivalent under this rotation.
					if (sum <= marginSquared)
					{
						// Encoding:
						//  - rotation == 0 -> return 0 (no rotation needed)
						//  - rotation == 1 -> return 3
						//  - rotation == 2 -> return 2
						//  - rotation == 3 -> return 1
						//
						// This "4 - rotation" encoding corresponds to how many steps are needed
						// to rotate back to the canonical orientation and is used by the
						// polygon-building code to reindex the vertices.
						return rotation == 0 ? 0 : 4 - rotation;
					}
				}

				// None of the 4 rotations produced a match.
				return _noTexInfo;
			}

			// --- Triangle path (3 vertices) --------------------------------------------
			// For triangles, we reuse the same idea as in the original version:
			// test each of the 3 cyclic rotations and ensure that *each* corresponding
			// vertex pair is within the allowed margin.

			for (int rotation = 0; rotation < 3; rotation++)
			{
				bool allEqual = true;

				// Test all 3 vertices for the current rotation.
				for (int j = 0; j < 3; j++)
				{
					int idx = j + rotation;
					if (idx >= 3)
						idx -= 3; // Equivalent to (j + rotation) % 3.

					var f = first[j];
					var s = second[idx];

					float dx = f.X - s.X;
					float dy = f.Y - s.Y;

					// If any vertex is further than allowed, this rotation fails.
					if ((dx * dx + dy * dy) > marginSquared)
					{
						allEqual = false;
						break;
					}
				}

				// All vertices are within the margin for this rotation.
				if (allEqual)
				{
					// Encoding mirrors the quad case:
					//  - rotation == 0 -> return 0 (no rotation)
					//  - rotation == 1 -> return 2
					//  - rotation == 2 -> return 1
					//
					// Again, the return value represents how many steps you'd need
					// to rotate back to the canonical ordering.
					return rotation == 0 ? 0 : 3 - rotation;
				}
			}

			// No rotation (for triangle) produced a match either.
			return _noTexInfo;
		}


		// Generate new parent with incoming texture and immediately add incoming texture as a child

		private void AddParent(TextureArea texture, List<ParentTextureArea> parentList, TextureDestination destination, bool isForTriangle, BlendMode blendMode, int frameIndex = -1)
        {
            var newParent = new ParentTextureArea(texture, destination);
            parentList.Add(newParent);
            newParent.AddChild(texture, frameIndex >= 0 ? frameIndex : GetNewTextureId(), isForTriangle, blendMode);
        }

        // Only exposed variation of AddTexture that should be used outside of TexInfoManager itself

        public Result AddTexture(TextureArea texture, TextureDestination destination, bool isForTriangle, BlendMode blendMode)
        {
            if (_dataHasBeenLaidOut)
                throw new InvalidOperationException("Data has been already laid out for this TexInfoManager. Reinitialize it if you want to restart texture collection.");

            // If UVRotate hack is needed and texture is triangle, prepare a quad substitute reference for animation lookup.
            var refQuad = texture;

            // Try to compare incoming texture with existing anims and return animation frame
            if (_actualAnimTextures.Count > 0)
                foreach (var actualTex in _actualAnimTextures)
                {
                    // If current animation set is UVRotate set and UVRotate hack is needed, pass the texture as quad
                    var asTriangle = isForTriangle;
                    var reference = texture;

                    var existing = GetTexInfo(reference, actualTex.CompiledAnimation, destination, asTriangle, blendMode, true, _animTextureLookupMargin);
                    if (existing.HasValue)
                    {
                        var result = new Result { ConvertToQuad = false, Rotation = existing.Value.Rotation, TexInfoIndex = existing.Value.TexInfoIndex, Animated = true };
                        return result;
                    }
                }

            // Now try to compare incoming texture with lookup anim seq table
            if (_referenceAnimTextures.Count > 0)
                foreach (var refTex in _referenceAnimTextures)
                {
                    // If current animation set is UVRotate set and UVRotate hack is needed, pass the texture as quad
                    var asTriangle = isForTriangle;
                    var reference = texture;

                    // If reference set found, generate actual one and immediately return fresh result
                    if (GetTexInfo(reference, refTex.CompiledAnimation, destination, asTriangle, blendMode, false, _animTextureLookupMargin).HasValue)
                    {
                        GenerateAnimTexture(refTex, refQuad, destination, isForTriangle);
                        var result = AddTexture(texture, destination, isForTriangle, blendMode);
                        {
                            result.Animated = true;
                        }
                        return new Result() { ConvertToQuad = false, Rotation = result.Rotation, TexInfoIndex = result.TexInfoIndex, Animated = true };
                    }
                }

            var parentTextures = _parentRoomTextureAreas;
            if (destination == TextureDestination.Moveable)
                parentTextures = _parentMoveableTextureAreas;
            else if (destination == TextureDestination.Static)
                parentTextures = _parentStaticTextureAreas;

            // No animated textures identified, add texture as ordinary one
            return AddTexture(texture, parentTextures, destination, isForTriangle, blendMode);
        }

        // Internal AddTexture variation which is capable of adding texture to various ParentTextureArea lists
        // with customizable parameters.
        // If animFrameIndex == -1, it means that ordinary texture is added, otherwise it indicates that specific anim
        // texture frame is being processed. If so, frame index is saved into TexInfoIndex field of resulting child.
        // Later on, on real anim texture creation, this index is used to sort frames in proper order.

        private Result AddTexture(TextureArea texture, List<ParentTextureArea> parentList, TextureDestination destination, bool isForTriangle, BlendMode blendMode, int animFrameIndex = -1, bool makeCanonical = true)
        {
            // In case AddTexture is used with animated seq packing, we don't check frames for full similarity, because
            // frames can be duplicated with Repeat function or simply because of complex animator functions applied.
            var result = animFrameIndex >= 0 ? null : GetTexInfo(texture, parentList, destination, isForTriangle, blendMode);

            if (!result.HasValue)
            {
                // Try to create new canonical (top-left-based) texture as child or parent.
                // makeCanonical parameter is necessary for animated textures, because animators may produce frames
                // with non-canonically rotated coordinates (e.g. spin animator).
                var canonicalTexture = makeCanonical ? texture.GetCanonicalTexture(isForTriangle) : texture;

                // If no any potential parents or children, create as new parent
                if (!TryToAddToExisting(canonicalTexture, parentList, destination, isForTriangle, blendMode, animFrameIndex))
                    AddParent(canonicalTexture, parentList, destination, isForTriangle, blendMode, animFrameIndex);

                // Try again to get texinfo
                if (animFrameIndex >= 0)
                    result = new Result { TexInfoIndex = _dummyTexInfo, Rotation = 0 };
                else
                    result = GetTexInfo(texture, parentList, destination, isForTriangle, blendMode);
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
        private void SortOutAlpha(List<ParentTextureArea> parentList)
        {
            Parallel.For(0, parentList.Count, i =>
            {
                var opaqueChildren = parentList[i].Children.Where(child => child.BlendMode < BlendMode.Additive);
                if (opaqueChildren.Count() > 0)
                {
                    var realBlendMode = parentList[i].Texture.Image.HasAlpha(TRVersion.Game.TombEngine,
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

        private void GenerateAnimLookups(List<AnimatedTextureSet> sets, TextureDestination destination)
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
                            AddTexture(newFrame, refAnim.CompiledAnimation, destination, (triangleVariation > 0), newFrame.BlendMode, index, set.IsUvRotate);
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

        private void GenerateAnimTexture(ParentAnimatedTexture reference, TextureArea origin, TextureDestination destination, bool isForTriangle)
        {
            var refCopy = reference.Clone();
            foreach (var parent in refCopy.CompiledAnimation)
            {
                parent.Destination = destination;

                foreach (var child in parent.Children)
                    child.BlendMode = origin.BlendMode;
            }

            // Sort and assign TexInfo indices for frames by the order they were created in reference animation
            var orderedFrameList = refCopy.CompiledAnimation.SelectMany(x => x.Children).OrderBy(c => c.TextureId);
            foreach (var frame in orderedFrameList)
            {
                frame.TextureId = GetNewTextureId();
            }

            _actualAnimTextures.Add(refCopy);
        }

        // Finds visually similar image areas across all parents and unifies them. This step drastically
        // reduces texture page usage if lots of objects with duplicated textures are used. This process is lossy.

        private void CleanUp(ref List<ParentTextureArea> textures)
        {
            var result = new Dictionary<Hash, ParentTextureArea>();

            foreach (var tex in textures) // Do not parallelize this. For some reason it breaks everything.
            {
                var bmpHash = tex.Texture.Image.GetHashOfAreaFast(tex.Area);

                if (result.ContainsKey(bmpHash))
                {
                    tex.Children.ForEach(child => tex.MoveChildWithoutRepositioning(child, result[bmpHash]));
                    result[bmpHash].BlendMode = tex.BlendMode;
                }
                else
                    result.TryAdd(bmpHash, tex);
            }

            textures = result.Select(entry => entry.Value).ToList();
        }

        // Maps parent texture areas on the proposed texture map.
        // This step only prepares for actual image data layout, actual layout is done in BuildTextureMap.

        private int PlaceTexturesInMap(ref List<ParentTextureArea> textures)
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
                int padding = _padding == 0 ? _minimumPadding : _padding;

                int tP = padding;
                int bP = padding;
                int lP = padding;
                int rP = padding;

                int horizontalPaddingSpace = MaxTileSize - w;
                int verticalPaddingSpace = MaxTileSize - h;

                // If hor/ver padding won't fully fit, get existing space and calculate padding out of it
                if (verticalPaddingSpace < tP + bP)
                {
                    tP = verticalPaddingSpace / 2;
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
                texPackers.Add(new RectPackerTree(new VectorInt2(MaxTileSize, MaxTileSize)));
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

        private VectorInt2 PlaceAnimatedTexturesInMap(ref List<ParentTextureArea> textures, bool uvrotate)
        {
            if (textures.Count == 0)
            {
                return VectorInt2.Zero;
            }

            bool done;
            int atlasWidth = uvrotate ? (int)textures[0].Area.Width : 256;
            int atlasHeight = uvrotate ? (int)textures[0].Area.Height : 256;

            do
            {
                RectPacker texPacker = new RectPackerTree(new VectorInt2(atlasWidth, atlasHeight));

                done = true;

                for (int i = 0; i < textures.Count; i++)
                {
                    // Get the size of the quad surrounding texture area, typically should be the texture area itself
                    int w = (int)(textures[i].Area.Width);
                    int h = (int)(textures[i].Area.Height);

                    // Calculate adaptive padding at all sides
                    int padding = 0;
                    if (!uvrotate)
                    {
                        padding = _padding == 0 ? _minimumPadding : _padding;
                    }

                    int tP = padding;
                    int bP = padding;
                    int lP = padding;
                    int rP = padding;

                    int horizontalPaddingSpace = MaxTileSize - w;
                    int verticalPaddingSpace = MaxTileSize - h;

                    // If hor/ver padding won't fully fit, get existing space and calculate padding out of it
                    if (verticalPaddingSpace < tP + bP)
                    {
                        tP = verticalPaddingSpace / 2;
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
                    VectorInt2? pos = texPacker.TryAdd(new VectorInt2(w, h));
                    if (pos.HasValue)
                    {
                        textures[i].Padding[0] = lP;
                        textures[i].Padding[1] = tP;
                        textures[i].Padding[2] = rP;
                        textures[i].Padding[3] = bP;
                        textures[i].Page = 0;
                        textures[i].PositionInPage = pos.Value;
                    }
                    else
                    {
                        done = false;
                        atlasWidth += 256;
                        atlasHeight += 256;
                        break;
                    }
                }
            } while (!done);


            return new VectorInt2(atlasWidth, atlasHeight);
        }

        private class SidecarLoadingCacheEntry
        {
            public MaterialType MaterialType { get; set; } = MaterialType.Default;
            public string NormalMapPath { get; set; }
            public string HeightMapPath { get; set; }
            public string SpecularMapPath { get; set; }
            public string AmbientOcclusionMapPath { get; set; }
            public string RoughnessMapPath { get; set; }
            public string EmissiveMapPath { get; set; }
            public ImageC? NormalMap { get; set; }
            public ImageC? HeightMap { get; set; }
            public ImageC? SpecularMap { get; set; }
            public ImageC? AmbientOcclusionMap { get; set; }
            public ImageC? RoughnessMap { get; set; }
            public ImageC? EmissiveMap { get; set; }
        }

        private List<TombEngineAtlas> CreateAtlas(ref List<ParentTextureArea> textures, int numPages, bool forceMinimumPadding, int baseIndex, VectorInt2 atlasSize, bool uvrotate)
        {
            var sidecarLoadingCache = new Dictionary<Texture, SidecarLoadingCacheEntry>();

            var atlasList = new List<TombEngineAtlas>();
            for (int i = 0; i < numPages; i++)
            {
                atlasList.Add(new TombEngineAtlas { ColorMap = ImageC.CreateNew(atlasSize.X, atlasSize.Y) });
            }

            var actualPadding = 0;
            if (!uvrotate)
            {
                actualPadding = (_padding == 0 && forceMinimumPadding) ? _minimumPadding : _padding;
            }

            void EnsureMap(ref ImageC? image, ColorC value)
            {
                if (image is null)
                {
                    image = ImageC.CreateNew(atlasSize.X, atlasSize.Y);
                    image.Value.Fill(value);
                }
            }

            int x, y;

            for (int i = 0; i < textures.Count; i++)
            {
                var p = textures[i];

                if (p.Texture == null || p.Texture.Image == null)
                {
                    _progressReporter.ReportWarn("Texture null: " + i);
                    continue;
                }

                x = (int)p.Area.Start.X;
                y = (int)p.Area.Start.Y;
                var width = (int)p.Area.Width;
                var height = (int)p.Area.Height;

                var image = atlasList[p.Page];

                var destX = p.PositionInPage.X + p.Padding[0];
                var destY = p.PositionInPage.Y + p.Padding[1];

                // Always copy color data
                image.ColorMap.CopyFrom(destX, destY, p.Texture.Image, x, y, width, height);
                AddPadding(p, p.Texture.Image, image.ColorMap, 0, actualPadding);

                // Try sidecar loading
                if (!sidecarLoadingCache.ContainsKey(p.Texture))
                {
                    var textureAbsolutePath = string.Empty;

                    if (p.Texture is LevelTexture)
                        textureAbsolutePath = p.Texture.Image.FileName;
                    else if (p.Texture is ImportedGeometryTexture)
                        textureAbsolutePath = ((ImportedGeometryTexture)p.Texture).AbsolutePath;
                    else if (p.Texture is WadTexture)
                        textureAbsolutePath = ((WadTexture)p.Texture).AbsolutePath;

                    var cacheEntry = new SidecarLoadingCacheEntry();
                    var materialData = MaterialData.TrySidecarLoadOrLoadExisting(textureAbsolutePath);

                    void TryLoadMap(string mapName, bool isFound, Func<MaterialData, string> pathSelector, Action<ImageC> assignImage, Action<string> assignPath)
                    {
                        var mapPath = pathSelector(materialData);
                        if (string.IsNullOrEmpty(mapPath))
                            return;

                        if (!isFound)
                        {
                            _progressReporter.ReportWarn($"{mapName} not found: {mapPath}");
                            return;
                        }

                        try
                        {
                            var potentialImage = ImageC.FromFile(mapPath);
                            if (potentialImage.Size == p.Texture.Image.Size)
                            {
                                assignImage(potentialImage);
                                assignPath(mapPath);
                                logger.Info($"{mapName} found: {mapPath}");
                            }
                            else
                            {
                                _progressReporter.ReportWarn(
                                    $"Texture file '{p.Texture}' has a {mapName.ToLower()} with a different size and was ignored.");
                            }
                        }
                        catch (Exception)
                        {
                            _progressReporter.ReportWarn($"Error while loading {mapName.ToLower()}: {mapPath}");
                        }
                    }

                    if (!string.IsNullOrEmpty(textureAbsolutePath))
                    {
                        cacheEntry.MaterialType = materialData.Type;

                        TryLoadMap("Normal map", materialData.IsNormalMapFound, m => m.NormalMap,
                            img => cacheEntry.NormalMap = img, path => cacheEntry.NormalMapPath = path );

                        // LEGACY: for legacy compatibility, try to load external bump map textures or generate automatically,
                        // if no normal was provided with sidecar loading, and fill the cache.
                        if (cacheEntry.NormalMap is null && p.Texture is LevelTexture tex)
                        {
                            if (!string.IsNullOrEmpty(tex.BumpPath))
                            {
                                // Load external custom bump map
                                try
                                {
                                    var potentialImage = ImageC.FromFile(materialData.NormalMap);
                                    if (potentialImage.Size == p.Texture.Image.Size)
                                    {
                                        cacheEntry.NormalMap = potentialImage;
                                        cacheEntry.NormalMapPath = materialData.NormalMap;
                                    }
                                    else
                                        _progressReporter.ReportWarn($"Texture file '{p.Texture}' has a normal map with a different size and was ignored.");
                                }
                                catch (Exception)
                                {
                                    _progressReporter.ReportWarn($"Error while loading custom external bump map: {tex.BumpPath}");
                                }
                            }

                            // If also no custom bump path was found, we'll automatically generate the normal map later
                        }

                        TryLoadMap("Height map", materialData.IsHeightMapFound, m => m.HeightMap,
                            img => cacheEntry.HeightMap = img, path => cacheEntry.HeightMapPath = path);

                        // TODO: Copy R channel of height map to A channel. Must be later done in CopySingleChannelFrom.
                        if (cacheEntry.HeightMap is not null)
                        {
                            var heightMap = cacheEntry.HeightMap.Value;
                            Parallel.For(0, heightMap.Height, yy =>
                            {
                                for (int xx = 0; xx < heightMap.Width; xx++)
                                {
                                    var heightPixel = heightMap.GetPixel(xx, yy);
                                    heightPixel.A = heightPixel.R;
                                    heightMap.SetPixel(xx, yy, heightPixel);
                                }
                            });
                        }

                        TryLoadMap("Specular map", materialData.IsSpecularMapFound, m => m.SpecularMap,
                            img => cacheEntry.SpecularMap = img, path => cacheEntry.SpecularMapPath = path);

                        TryLoadMap("Ambient occlusion map", materialData.IsAmbientOcclusionMapFound, m => m.AmbientOcclusionMap,
                            img => cacheEntry.AmbientOcclusionMap = img, path => cacheEntry.AmbientOcclusionMapPath = path);

                        TryLoadMap("Roughness map", materialData.IsRoughnessMapFound,  m => m.RoughnessMap,
                            img => cacheEntry.RoughnessMap = img, path => cacheEntry.RoughnessMapPath = path);

                        TryLoadMap("Emissive map", materialData.IsEmissiveMapFound, m => m.EmissiveMap,
                            img => cacheEntry.EmissiveMap = img, path => cacheEntry.EmissiveMapPath = path);
                    }

                    sidecarLoadingCache.Add(p.Texture, cacheEntry);
                }

                var currentCacheEntry = sidecarLoadingCache[p.Texture];

                // Normal map processing
                if (currentCacheEntry.NormalMap is not null)
                {
                    EnsureMap(ref image.NormalMap, new ColorC(128, 128, 255));
                    image.NormalMap.Value.CopyFrom(destX, destY, currentCacheEntry.NormalMap.Value, x, y, width, height);
                }
                else if (p.Texture is LevelTexture)
                {
                    // LEGACY: generate normal maps programmatically if no sidecar normal or bump map was provided
                    // and bump mapping is requested for this texture

                    var tex = p.Texture as LevelTexture;

                    // Only for level textures, try to generate normal maps
                    var level = tex.GetBumpMappingLevelFromTexCoord(p.Area.GetMid());

                    var bumpImage = ImageC.CreateNew(width, height);
                    bumpImage.CopyFrom(0, 0, p.Texture.Image, x, y, width, height);

                    float sobelLevel = 0;
                    float sobelStrength = 0;

                    switch (level)
                    {
                        case BumpMappingLevel.Level1:
                            sobelLevel = 0.004f;
                            sobelStrength = 0.003f;
                            break;
                        case BumpMappingLevel.Level2:
                            sobelLevel = 0.007f;
                            sobelStrength = 0.004f;
                            break;
                        case BumpMappingLevel.Level3:
                            sobelLevel = 0.009f;
                            sobelStrength = 0.008f;
                            break;
                    }

                    // Init the normal map if not done yet
                    EnsureMap(ref image.NormalMap, new ColorC(128, 128, 255));

                    if (level != BumpMappingLevel.None)
                    {
                        // TODO: disabled for now
                        // bumpImage = ImageC.GrayScaleFilter(bumpImage, true, 0, 0, bumpImage.Width, bumpImage.Height);
                        // bumpImage = ImageC.GaussianBlur(bumpImage, radius: 1.0f);
                        // bumpImage = ImageC.NormalizeContrast(bumpImage);
                        // bumpImage = ImageC.SobelFilter(bumpImage, sobelStrength, sobelLevel, SobelFilterType.Scharr, 0, 0, bumpImage.Width, bumpImage.Height);

                        // The old way of doing normal maps
                        bumpImage = ImageC.GrayScaleFilter(bumpImage, true, 0, 0, bumpImage.Width, bumpImage.Height);
                        bumpImage = ImageC.SobelFilter(bumpImage, sobelStrength, sobelLevel, SobelFilterType.Sobel, 0, 0, bumpImage.Width, bumpImage.Height);
                    }
                    else
                        // Neutral Bump
                        bumpImage.Fill(new ColorC(128, 128, 255));

                    image.NormalMap.Value.CopyFrom(destX, destY, bumpImage, 0, 0, width, height);
                }

                // Add padding for normal map
                if (image.NormalMap is not null)
                    AddPadding(p, image.NormalMap.Value, image.NormalMap.Value, 0, actualPadding, destX, destY);

                // Ambient occlusion map
                if (currentCacheEntry.AmbientOcclusionMap is not null)
                {
                    EnsureMap(ref image.ORSHMap, new ColorC(255, 255, 0));
                    image.ORSHMap.Value.CopySingleChannelFrom(destX, destY, currentCacheEntry.AmbientOcclusionMap.Value, x, y, width, height, ImageChannel.R);
                }

                // Roughness map
                if (currentCacheEntry.RoughnessMap is not null)
                {
                    EnsureMap(ref image.ORSHMap, new ColorC(255, 255, 0));
                    image.ORSHMap.Value.CopySingleChannelFrom(destX, destY, currentCacheEntry.RoughnessMap.Value, x, y, width, height, ImageChannel.G);
                }

                // Specular map
                if (currentCacheEntry.SpecularMap is not null)
                {
                    EnsureMap(ref image.ORSHMap, new ColorC(255, 255, 0));
                    image.ORSHMap.Value.CopySingleChannelFrom(destX, destY, currentCacheEntry.SpecularMap.Value, x, y, width, height, ImageChannel.B);
                }
                else
                {
                    // In this case, for reflective materials, let's create a dummy white specular map on the fly
                    if (currentCacheEntry.MaterialType == MaterialType.Reflective || 
                        currentCacheEntry.MaterialType == MaterialType.SkyboxReflective)
                    {
                        var dummySpecularMap = ImageC.CreateNew(width, height);
                        dummySpecularMap.Fill(new ColorC(128, 128, 128));

                        EnsureMap(ref image.ORSHMap, new ColorC(255, 255, 0));
                        image.ORSHMap.Value.CopySingleChannelFrom(destX, destY, dummySpecularMap, 0, 0, width, height, ImageChannel.B);
                    }
                }

                // Height map
                if (currentCacheEntry.HeightMap is not null)
                {
                    EnsureMap(ref image.ORSHMap, new ColorC(255, 255, 0));
                    image.ORSHMap.Value.CopySingleChannelFrom(destX, destY, currentCacheEntry.HeightMap.Value, x, y, width, height, ImageChannel.A);
                }

                // Add padding here for AmbientOcclusionRoughnessSpecularMap because we have packed three textures in RGB channels
                if (image.ORSHMap is not null)
                    AddPadding(p, image.ORSHMap.Value, image.ORSHMap.Value, 0, actualPadding, destX, destY);

                // Emissive map
                if (currentCacheEntry.EmissiveMap is not null)
                {
                    EnsureMap(ref image.EmissiveMap, new ColorC(0, 0, 0, 255));
                    image.EmissiveMap.Value.CopyFrom(destX, destY, currentCacheEntry.EmissiveMap.Value, x, y, width, height);
                    AddPadding(p, image.EmissiveMap.Value, image.EmissiveMap.Value, 0, actualPadding, destX, destY);
                }
            }

            for (int i = 0; i < textures.Count; i++)
            {
                textures[i].AtlasDimensions = atlasList[textures[i].Page].ColorMap.Size;
                textures[i].AtlasIndex = textures[i].Page + baseIndex;
            }

            return atlasList;
        }

        // Expands edge pixels to create padding which prevents border bleeding problems.

        private void AddPadding(ParentTextureArea texture, ImageC from, ImageC to, int pageOffset, int padding, int? customX = null, int? customY = null)
        {
            var p = texture;
            var x = customX.HasValue ? customX.Value : (int)p.Area.Start.X;
            var y = customY.HasValue ? customY.Value : (int)p.Area.Start.Y;
            var width = (int)p.Area.Width;
            var height = (int)p.Area.Height;
            var dataOffset = 0;

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

        public void LayOutAllData()
        {
            if (_dataHasBeenLaidOut) return;
            _dataHasBeenLaidOut = true;

            // Before any other action, lay out animated textures
            PrepareAnimatedTextures();

            // Subdivide textures in 3 blocks: room, objects, bump
            var roomTextures = _parentRoomTextureAreas;
            var moveablesTextures = _parentMoveableTextureAreas;
            var staticsTextures = _parentStaticTextureAreas;
            var animatedTextures = new List<List<ParentTextureArea>>();

            for (int n = 0; n < _actualAnimTextures.Count; n++)
            {
                var parentTextures = _actualAnimTextures[n].CompiledAnimation;

                animatedTextures.Add(new List<ParentTextureArea>());

                for (int i = 0; i < parentTextures.Count; i++)
                {
                    animatedTextures[n].Add(parentTextures[i]);
                }
            }

            // Cleanup duplicated parent areas.
            if (!_level.Settings.FastMode)
            {
                CleanUp(ref roomTextures);
                CleanUp(ref moveablesTextures);
                CleanUp(ref staticsTextures);
                for (int n = 0; n < _actualAnimTextures.Count; n++)
                {
                    var textures = animatedTextures[n];
                    CleanUp(ref textures);
                }
            }

            // Sort textures by their TopmostAndUnpadded property (waterfalls first!)
            roomTextures = roomTextures
                .OrderBy(item => item.BlendMode)
                .ThenByDescending(item => item.Area.Size.X * item.Area.Size.Y)
                .ToList();

            moveablesTextures = moveablesTextures
               .OrderBy(item => item.BlendMode)
               .ThenByDescending(item => item.Area.Size.X * item.Area.Size.Y)
               .ToList();

            staticsTextures = staticsTextures
               .OrderBy(item => item.BlendMode)
               .ThenByDescending(item => item.Area.Size.X * item.Area.Size.Y)
               .ToList();

            // Calculate new X, Y of each texture area
            int numRoomsAtlases = PlaceTexturesInMap(ref roomTextures);
            int numMoveablesAtlases = PlaceTexturesInMap(ref moveablesTextures);
            int numStaticsAtlases = PlaceTexturesInMap(ref staticsTextures);

            ICollection<VectorInt2> animatedAtlasSizes = new List<VectorInt2>();
            for (int n = 0; n < _actualAnimTextures.Count; n++)
            {
                var textures = animatedTextures[n];
                animatedAtlasSizes.Add(PlaceAnimatedTexturesInMap(ref textures, _actualAnimTextures[n].Origin.AnimationType == AnimatedTextureAnimationType.UVRotate));
            }

            // In TombEngine, we pack textures in 4K pages and we can use big textures up to 256 pixels without bleeding
            VectorInt2 atlasSize = new VectorInt2(MaxTileSize, MaxTileSize);

            RoomsAtlas = CreateAtlas(ref roomTextures, numRoomsAtlases, true, 0, atlasSize, false);
            MoveablesAtlas = CreateAtlas(ref moveablesTextures, numMoveablesAtlases, true, 0, atlasSize, false);
            StaticsAtlas = CreateAtlas(ref staticsTextures, numStaticsAtlases, true, 0, atlasSize, false);

            AnimatedAtlas = new List<TombEngineAtlas>();
            for (int n = 0; n < _actualAnimTextures.Count; n++)
            {
                var textures = animatedTextures[n];
                AnimatedAtlas.AddRange(CreateAtlas(ref textures, 1, false, AnimatedAtlas.Count,
                    animatedAtlasSizes.ElementAt(n),
                    _actualAnimTextures[n].Origin.AnimationType == AnimatedTextureAnimationType.UVRotate));
            }

#if DEBUG
            try
            {
                Directory.CreateDirectory("OutputDebug");

                for (int n = 0; n < RoomsAtlas.Count; n++)
                {
                    RoomsAtlas[n].ColorMap.SaveToFile("OutputDebug\\RoomsAtlas" + n + ".png");
                    RoomsAtlas[n].NormalMap?.SaveToFile("OutputDebug\\RoomsAtlas" + n + "_N.png");
                    RoomsAtlas[n].ORSHMap?.SaveToFile("OutputDebug\\RoomsAtlas" + n + "_AOS.png");
                    RoomsAtlas[n].EmissiveMap?.SaveToFile("OutputDebug\\RoomsAtlas" + n + "_E.png");
                }

                for (int n = 0; n < MoveablesAtlas.Count; n++)
                {
                    MoveablesAtlas[n].ColorMap.SaveToFile("OutputDebug\\MoveablesAtlas" + n + ".png");
                    MoveablesAtlas[n].NormalMap?.SaveToFile("OutputDebug\\MoveablesAtlas" + n + "_N.png");
                    MoveablesAtlas[n].ORSHMap?.SaveToFile("OutputDebug\\MoveablesAtlas" + n + "_AOS.png");
                    MoveablesAtlas[n].EmissiveMap?.SaveToFile("OutputDebug\\MoveablesAtlas" + n + "_E.png");
                }

                for (int n = 0; n < StaticsAtlas.Count; n++)
                {
                    StaticsAtlas[n].ColorMap.SaveToFile("OutputDebug\\StaticsAtlas" + n + ".png");
                    StaticsAtlas[n].NormalMap?.SaveToFile("OutputDebug\\StaticsAtlas" + n + "_N.png");
                    StaticsAtlas[n].ORSHMap?.SaveToFile("OutputDebug\\StaticsAtlas" + n + "_AOS.png");
                    StaticsAtlas[n].EmissiveMap?.SaveToFile("OutputDebug\\StaticsAtlas" + n + "_E.png");
                }

                for (int n = 0; n < AnimatedAtlas.Count; n++)
                {
                    AnimatedAtlas[n].ColorMap.SaveToFile("OutputDebug\\AnimatedAtlas" + n + ".png");
                    AnimatedAtlas[n].NormalMap?.SaveToFile("OutputDebug\\AnimatedAtlas" + n + "_N.png");
                    AnimatedAtlas[n].ORSHMap?.SaveToFile("OutputDebug\\AnimatedAtlas" + n + "_AOS.png");
                    AnimatedAtlas[n].EmissiveMap?.SaveToFile("OutputDebug\\AnimatedAtlas" + n + "_E.png");
                }
            }
            catch { }   
#endif

            // Finally compile all texinfos
            BuildTextureInfos();
        }

		private void BuildTextureInfos()
		{
			float maxSize = (float)MaxTileSize - (1.0f / (float)(MaxTileSize - 1));

			_objectTextures = new SortedDictionary<int, ObjectTexture>();

			// Helper local function for repetitive pattern
			void ProcessAreas(List<ParentTextureArea> parents)
			{
				SortOutAlpha(parents);
				foreach (var parent in parents)
				{
					foreach (var child in parent.Children)
					{
						if (_objectTextures.ContainsKey(child.TextureId))
							continue;

						var newObjectTexture = new ObjectTexture(parent, child, maxSize);
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
							_progressReporter.ReportWarn(
								"Compiled TexInfo " + child.TextureId + " is broken, coordinates are invalid.");
						}
#endif
						_objectTextures.Add(child.TextureId, newObjectTexture);
					}
				}
			}

			// Process texture sets
			ProcessAreas(_parentRoomTextureAreas);
			ProcessAreas(_parentMoveableTextureAreas);
			ProcessAreas(_parentStaticTextureAreas);

			// Process animated textures
			foreach (var animTexture in _actualAnimTextures)
			{
				SortOutAlpha(animTexture.CompiledAnimation);
				foreach (var parent in animTexture.CompiledAnimation)
				{
					foreach (var child in parent.Children)
					{
						if (!_objectTextures.ContainsKey(child.TextureId))
							_objectTextures.Add(child.TextureId, new ObjectTexture(parent, child, maxSize));
					}
				}
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
            _animTextureIndices = new List<List<int>>();
            foreach (var compiledAnimatedTexture in _actualAnimTextures)
            {
                var list = new List<int>();
                var orderedFrameList = compiledAnimatedTexture.CompiledAnimation.SelectMany(x => x.Children).OrderBy(c => c.TextureId).ToList();
                foreach (var frame in orderedFrameList)
                    list.Add(frame.TextureId);

                _animTextureIndices.Add(list);
            }
        }

        public void WriteAnimatedTextures(BinaryWriterEx writer)
        {
            writer.Write((int)_actualAnimTextures.Count);
            for (int i = 0; i < _actualAnimTextures.Count; i++)
            {
                byte animType = 0;

                switch (_actualAnimTextures[i].Origin.AnimationType)
                {
                    default:
                    case AnimatedTextureAnimationType.Frames:
                        animType = 0;
                        break;

                    case AnimatedTextureAnimationType.UVRotate:
                        animType = 1;
                        break;

                    case AnimatedTextureAnimationType.Video:
                        animType = 2;
                        break;
                }

                writer.Write(i);
                writer.Write((byte)_actualAnimTextures[i].Origin.Fps);
                writer.Write((byte)animType);
                writer.Write((float)(animType == 1 ? _actualAnimTextures[i].Origin.TenUvRotateDirection : 0));
                writer.Write((float)(animType == 1 ? _actualAnimTextures[i].Origin.TenUvRotateSpeed : 0.0f));
                writer.Write(_animTextureIndices[i].Count); // Number of frames

                foreach (var frame in _animTextureIndices[i])
                {
                    var texture = _objectTextures[(int)frame];

                    // Coordinates of each frame
                    writer.Write(texture.TexCoordFloat[0].X);
                    writer.Write(texture.TexCoordFloat[0].Y);
                    writer.Write(texture.TexCoordFloat[1].X);
                    writer.Write(texture.TexCoordFloat[1].Y);
                    writer.Write(texture.TexCoordFloat[2].X);
                    writer.Write(texture.TexCoordFloat[2].Y);
                    writer.Write(texture.TexCoordFloat[3].X);
                    writer.Write(texture.TexCoordFloat[3].Y);
                }
            }
        }

        public List<ObjectTexture> GetObjectTextures()
        {
            return _objectTextures.Values.ToList();
        }

        public Tuple<int, int> GetAnimatedTexture(int tid)
        {
            for (int i = 0; i < _animTextureIndices.Count; i++)
                for (int j = 0; j < _animTextureIndices[i].Count; j++)
                    if (_animTextureIndices[i][j] == tid)
                        return new Tuple<int, int>(i, j);
            return null;
        }

    }
}