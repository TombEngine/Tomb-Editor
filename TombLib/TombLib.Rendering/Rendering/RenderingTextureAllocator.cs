using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using TombLib.Utils;

namespace TombLib.Rendering
{
    public abstract class RenderingTextureAllocator : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const int MinimumPageCount = 4;
        public const int MaximumPageCount = 32;
        public const int SafePageCount = 8;
        public const int PageSize = 2048;

        public class Description
        {
            public VectorInt3 Size { get; set; } = new VectorInt3(PageSize, PageSize, MaximumPageCount);
            public Func<VectorInt2, RectPacker> CreateRectPacker = size => new RectPackerTree(size);
        }

        private class TexturePage
        {
            public RectPacker Packer;
            public TexturePage(VectorInt2 Size)
            {
                Packer = new RectPackerTree(Size);
            }
        }

        // Texture accessing state
        private KeyValuePair<RenderingTexture, VectorInt3> QuickAccessTexture0;
        private KeyValuePair<RenderingTexture, VectorInt3> QuickAccessTexture1;
        private int LastUsedTextureIndex = 0;
        private readonly TexturePage[] Pages;
        private readonly Dictionary<RenderingTexture, VectorInt3> AvailableTextures = new Dictionary<RenderingTexture, VectorInt3>();
        public VectorInt3 Size { get; }

        // Garbage collection state
        public delegate void GarbageCollectionAdjustDelegate(RenderingTextureAllocator allocator, Map map);
        public delegate GarbageCollectionAdjustDelegate GarbageCollectionCollectDelegate(RenderingTextureAllocator allocator, Map map, HashSet<Map.Entry> inOutUsedTextures);
        private const float GarbageCollectionWaitSeconds = 0.6f;
        private long GarbageCollectionLastTimestamp = Stopwatch.GetTimestamp() - (long)(GarbageCollectionWaitSeconds * Stopwatch.Frequency);
        private bool GarbageCollectionInProgress = false;
        public List<GarbageCollectionCollectDelegate> GarbageCollectionCollectEvent = new List<GarbageCollectionCollectDelegate>();

        public RenderingTextureAllocator(RenderingDevice device, Description description)
        {
            logger.Info("Rendering texture allocated of size \"" + description.Size + "\".");

            Pages = new TexturePage[description.Size.Z];
            for (int i = 0; i < description.Size.Z; ++i)
                Pages[i] = new TexturePage(new VectorInt2(description.Size.X, description.Size.Y));
            Size = description.Size;
        }

        public abstract void Dispose();
        public abstract ImageC RetrieveTestImage();
        protected abstract void UploadTexture(RenderingTexture Texture, VectorInt3 pos);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private VectorInt3 UpdateQuickAccess(ref RenderingTexture texture, VectorInt3 result)
        {
            if (LastUsedTextureIndex == 0)
            {
                QuickAccessTexture1 = new KeyValuePair<RenderingTexture, VectorInt3>(texture, result);
                LastUsedTextureIndex = 1;
            }
            else
            {
                QuickAccessTexture0 = new KeyValuePair<RenderingTexture, VectorInt3>(texture, result);
                LastUsedTextureIndex = 0;
            }
            return result;
        }

        private VectorInt3? AllocateTexture(RenderingTexture texture)
        {
            if (texture.To.X < texture.From.X || texture.To.Y < texture.From.Y)
                throw new ArgumentOutOfRangeException(); // Avoid totally currupted texture allocator state.
            for (int i = 0; i < Pages.Length; ++i)
            {
                VectorInt2? allocatedPos = Pages[i].Packer.TryAdd(VectorInt2.Max((texture.To - texture.From) + new VectorInt2(2), VectorInt2.One));
                if (allocatedPos != null)
                {
                    VectorInt3 pos = new VectorInt3(allocatedPos.Value.X, allocatedPos.Value.Y, i);
                    if (texture.To.X == texture.From.X || texture.To.Y == texture.From.Y)
                        UploadTexture(ImageC.CreateNew(1, 1), pos); // Attempt to handle this situation
                    else
                        UploadTexture(texture, pos);
                    AvailableTextures.Add(texture, pos + new VectorInt3(1, 1, 0));
                    return pos + new VectorInt3(1, 1, 0);
                }
            }
            return null;
        }

        /// <summary>
        /// RemoveUser must be called after the rendering texture is no longer used.
        /// </summary>
        public VectorInt3 Get(RenderingTexture texture)
        {
            // Check if it's the last used texture.
            // This speeds up the common case because oftentimes similar textures are used often
            if (texture == QuickAccessTexture0.Key)
            {
                LastUsedTextureIndex = 0;
                return QuickAccessTexture0.Value;
            }
            if (texture == QuickAccessTexture1.Key)
            {
                LastUsedTextureIndex = 1;
                return QuickAccessTexture1.Value;
            }

            // Look texture up in dictionary
            VectorInt3 result;
            if (AvailableTextures.TryGetValue(texture, out result))
                return UpdateQuickAccess(ref texture, result);

            // Add texture
            VectorInt3? result2 = AllocateTexture(texture);
            if (result2.HasValue)
                return UpdateQuickAccess(ref texture, result2.Value);
            if (!GarbageCollect())
                return new VectorInt3(); // Garbage collection failed and we have already established that there is not enough space as it is.
            VectorInt3? result3 = AllocateTexture(texture); // Garbage collection succeeded, let's try again
            if (result3.HasValue)
                return UpdateQuickAccess(ref texture, result3.Value);
            return new VectorInt3(); // Still not enough space but there is nothing we can do about it.
        }

        public VectorInt3 GetForTriangle(TextureArea texture)
        {
            const int MaxDirectImageArea = 1024 * 1024;

            // @FIXME: MaxDirectImageArea GREATER comparison against image size area made no sense,
            // so I changed it to LESS-OR-EQUAL. Addressed to TRTomb. -- Lwmte

            VectorInt2 imageSize = texture.Texture.Image.Size;
            if ((imageSize.X * imageSize.Y) <= MaxDirectImageArea &&
                Size.X >= imageSize.X && Size.Y >= imageSize.Y)
            { // If the image is small enough, allocate the entire image...
                VectorInt3 allocatedTexture = Get(new RenderingTexture
                {
                    From = new VectorInt2(),
                    To = imageSize,
                    Image = texture.Texture.Image
                });
                return allocatedTexture;
            }
            else
            { // Allocate a part of the image...

                // @FIXME: replace origRect calculation with commented line when bug with texture allocator atlas
                // corruption is fixed. Addressed to TRTomb. -- Lwmte

                // var origRect = texture.ParentArea.IsZero ? texture.GetRect(true).Round() : texture.ParentArea.Round();
                var origRect = texture.GetRect(true).Round();

                VectorInt3 allocatedTexture = Get(new RenderingTexture
                {
                    From = VectorInt2.FromRounded(origRect.Start),
                    To = VectorInt2.FromRounded(origRect.End),
                    Image = texture.Texture.Image
                });

                return allocatedTexture - new VectorInt3((int)origRect.Start.X, (int)origRect.Start.Y, 0);
            }
        }

        /// <summary> Removes and compresses texture space. Updates all buffers with texture coordinates.</summary>
        public bool GarbageCollect(bool checkTime = true)
        {
            if (GarbageCollectionInProgress)
                return false;

            // We don't want to garbage collect the texture all the time because it will make the program very slow.
            // So if we just collected it and only a bit of time elapsed, if it's already full again we must give up.
            long elapsedTicks = Stopwatch.GetTimestamp() - GarbageCollectionLastTimestamp;
            float elapsedSeconds = (float)elapsedTicks / Stopwatch.Frequency;
            if (checkTime && elapsedSeconds < GarbageCollectionWaitSeconds)
            {
                logger.Warn("Garbage collection of texture (size " + Size + ") was aborted because only " + elapsedSeconds + " seconds elapsed " +
                    "since the last collection. (Minimum time: " + GarbageCollectionWaitSeconds + " seconds)");
                return false;
            }

            // Start garbage collection
            try
            {
                logger.Info("Started garbage collecting of texture (size " + Size + ") after " + elapsedSeconds + " seconds.");
                GarbageCollectionInProgress = true;

                // Collect
                var map = new Map(this);
                var usedTextures = new HashSet<Map.Entry>();
                var adjustEvent = new List<GarbageCollectionAdjustDelegate>();
                foreach (var collectDelegate in GarbageCollectionCollectEvent)
                {
                    var adjustDelegate = collectDelegate(this, map, usedTextures);
                    if (adjustDelegate != null)
                        adjustEvent.Add(adjustDelegate);
                }
                logger.Error("Removed " + (AvailableTextures.Count - usedTextures.Count) + " textures of previously " + AvailableTextures.Count + ".");

                // Reset
                for (int i = 0; i < Size.Z; ++i)
                    Pages[i] = new TexturePage(new VectorInt2(Size.X, Size.Y));
                AvailableTextures.Clear();

                // Read textures that were still used
                var usedTexturesSorted = usedTextures.ToArray();
                Array.Sort(usedTexturesSorted, new GarbageCollectionTextureOrder());
                foreach (Map.Entry usedTexture in usedTexturesSorted)
                    if (AllocateTexture(usedTexture.Texture) == null)
                        logger.Error("Unable to add texture after garbage collecting texture!");

                // Adjust
                foreach (var adjustDelegate in adjustEvent)
                    adjustDelegate(this, map);

                // Finished
                return true;
            }
            finally
            {
                logger.Info("Garbage collection of texture (size " + Size + ") finished.");
                GarbageCollectionLastTimestamp = Stopwatch.GetTimestamp();
                GarbageCollectionInProgress = false;
            }
        }

        private class GarbageCollectionTextureOrder : IComparer<Map.Entry>
        {
            public int Compare(Map.Entry first, Map.Entry second)
            {
                VectorInt2 firstSize = first.Texture.To - first.Texture.From;
                VectorInt2 secondSize = second.Texture.To - second.Texture.From;

                // Return 0 if sizes are completely equal
                if (firstSize == secondSize)
                    return 0;

                // Compare height
                if (firstSize.Y != secondSize.Y)
                    return firstSize.Y > secondSize.Y ? -1 : 1; // Higher textures first!

                // Compare area
                int firstArea = firstSize.X * firstSize.Y;
                int secondArea = secondSize.X * secondSize.Y;
                if (firstArea != secondArea)
                    return firstArea > secondArea ? -1 : 1; // Bigger textures first!

                return 0;
            }
        }

        /// <summary>Datastructure to quickly lookup which texture can be found at a certain position.</summary>
        public class Map
        {
            public class Entry
            {
                public readonly RenderingTexture Texture;
                public readonly VectorInt3 Pos;
                internal Entry(KeyValuePair<RenderingTexture, VectorInt3> availableTexture)
                {
                    Texture = availableTexture.Key;
                    Pos = availableTexture.Value;
                }
            }
            private const int _mapGranularity = 64;
            private List<Entry>[,,] _map;

            public Map(RenderingTextureAllocator Allocator)
            {
                // Create map
                _map = new List<Entry>[Allocator.Size.Z, Allocator.Size.Y / _mapGranularity, Allocator.Size.X / _mapGranularity];
                for (int z = 0; z < _map.GetLength(0); ++z)
                    for (int y = 0; y < _map.GetLength(1); ++y)
                        for (int x = 0; x < _map.GetLength(2); ++x)
                            _map[z, y, x] = new List<Entry>();

                // Fill map
                foreach (KeyValuePair<RenderingTexture, VectorInt3> availableTexture in Allocator.AvailableTextures)
                {
                    Entry @ref = new Entry(availableTexture);
                    VectorInt2 startPixel = new VectorInt2(availableTexture.Value.X, availableTexture.Value.Y);
                    VectorInt2 endPixel = startPixel + (availableTexture.Key.To - availableTexture.Key.From);
                    VectorInt2 startBlock = startPixel / _mapGranularity;
                    VectorInt2 endBlock = (endPixel + new VectorInt2(_mapGranularity - 1, _mapGranularity - 1)) / _mapGranularity;
                    for (int x = startBlock.X; x < endBlock.X; ++x)
                        for (int y = startBlock.Y; y < endBlock.Y; ++y)
                            _map[availableTexture.Value.Z, y, x].Add(@ref);
                }
            }

            public Entry Lookup(VectorInt3 pos)
            {
                int mapX = pos.X / _mapGranularity;
                int mapY = pos.Y / _mapGranularity;
                if (mapX < 0 || mapY < 0 || mapX >= _map.GetLength(2) || mapY >= _map.GetLength(1))
                    return null;

                List<Entry> candidates = _map[pos.Z, mapY, mapX];
                foreach (Entry candidate in candidates)
                    if ((pos.X >= candidate.Pos.X) && (pos.Y >= candidate.Pos.Y))
                    {
                        VectorInt2 size = candidate.Texture.To - candidate.Texture.From;
                        if ((pos.X < (candidate.Pos.X + size.X)) && (pos.Y < (candidate.Pos.Y + size.Y)))
                            return candidate;
                    }
                return null;
            }
        };
    }
}
