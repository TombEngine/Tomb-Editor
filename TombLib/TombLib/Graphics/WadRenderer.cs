using NLog;
using TombLib.Graphics.Dx11Toolkit;
using System;
using System.Collections.Generic;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.Graphics
{
    public class WadRenderer : IDisposable
    {
        public record struct AllocationResult(VectorInt3 Position, VectorInt2 OriginalSize, VectorInt2 AllocatedSize, VectorInt2 AtlasDimension);

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public GraphicsDevice GraphicsDevice { get; }
        public Texture2D Texture { get; private set; } = null;

        private IDictionary<WadMoveable, AnimatedModel> Moveables { get; } = new Dictionary<WadMoveable, AnimatedModel>();
        private IDictionary<WadStatic, StaticModel> Statics { get; } = new Dictionary<WadStatic, StaticModel>();

        private IList<RectPacker> TexturePackers { get; } = new List<RectPacker>();
        private IDictionary<WadTexture, AllocationResult> PackedTextures { get; } = new Dictionary<WadTexture, WadRenderer.AllocationResult>();
        private bool _compactTexture;
        private bool _correctTexture;
        private bool _disposing = false;
        private int _textureAtlasSize { get; }
        private int _maxTextureAllocationSize { get; }
        public int TextureAtlasSize { get => _textureAtlasSize; }
        private bool _loadAnimations { get; }

        public WadRenderer(GraphicsDevice graphicsDevice, bool compactTexture, bool correctTexture, int atlasSize, int maxAllocationSize, bool loadAnimations)
        {
            GraphicsDevice = graphicsDevice;
            _compactTexture = compactTexture;
            _correctTexture = correctTexture;
            _textureAtlasSize = atlasSize;
            _maxTextureAllocationSize = maxAllocationSize;
            _loadAnimations = loadAnimations;
            AddPacker();
        }

        private void AddPacker()
        {
            var size = new VectorInt2(_textureAtlasSize, _textureAtlasSize);
            TexturePackers.Add(_compactTexture ? new RectPackerTree(size) : new RectPackerSimpleStack(size));
        }

        public void Dispose()
        {
            // Avoid multiple attempts at cleanup
            if (_disposing)
                return;

            _disposing = true;
            Texture?.Dispose();
            Texture = null;

            foreach (var obj in Moveables.Values)
                obj.Dispose();
            Moveables.Clear();

            foreach (var obj in Statics.Values)
                obj.Dispose();
            Statics.Clear();

            PackedTextures.Clear();
            TexturePackers.Clear();
            AddPacker();

            _disposing = false;
        }

        public void GarbageCollect()
        {
            Dispose();
            InitializeTexture();
        }

        private void ReclaimTextureSpace<T, U>(Model<T, U> model) where U : unmanaged where T: IDisposable
        {
            // TODO Some mechanism to reclaim texture space without rebuilding the atlas would be good.
            // Currently texture space is only freed when the atlas fills up completely and must be rebuilt.
        }

        public AnimatedModel GetMoveable(WadMoveable moveable, bool maybeRebuildAll = true)
        {
            // Check if the data is already loaded
            // If yes attempt to use that one
            AnimatedModel model;
            if (Moveables.TryGetValue(moveable, out model))
            {
                if (model.Version >= moveable.Version)
                    return model;
                ReclaimTextureSpace(model);
                model.Dispose();
                Moveables.Remove(moveable);
            }

            // The data is either new or has changed, unfortunately we need to reload it
            try
            {
                model = AnimatedModel.FromWadMoveable(GraphicsDevice, moveable, AllocateTexture, _correctTexture, _loadAnimations);
            }
            catch (TextureAtlasFullException exc)
            {
                logger.Info(exc.Message);

                if (maybeRebuildAll)
                {
                    logger.Info("Starting to rebuild the entire atlas.");
                    Dispose();
                    return GetMoveable(moveable, false);
                }
            }
            Moveables.Add(moveable, model);
            return model;
        }

        public StaticModel GetStatic(WadStatic @static, bool maybeRebuildAll = true)
        {
            // Check if the data is already loaded
            // If yes attempt to use that one
            StaticModel model;
            if (Statics.TryGetValue(@static, out model))
            {
                if (model.Version >= @static.Version)
                    return model;
                ReclaimTextureSpace(model);
                model.Dispose();
                Statics.Remove(@static);
            }

            // The data is either new or has changed, unfortunately we need to reload it
            try
            {
                model = new StaticModel(GraphicsDevice);
                model.Meshes.Add(ObjectMesh.FromWad2(GraphicsDevice, @static.Mesh, AllocateTexture, _correctTexture));
                model.UpdateBuffers();
            }
            catch (TextureAtlasFullException exc)
            {
                logger.Error(exc.Message);

                if (maybeRebuildAll)
                {
                    logger.Info("Starting to rebuild the entire atlas.");
                    Dispose();
                    return GetStatic(@static, false);
                }
            }

            Statics.Add(@static, model);
            return model;
        }

        private AllocationResult AllocateTexture(WadTexture texture)
        {
            // Check if the texture is already loaded
            {
                if (PackedTextures.TryGetValue(texture, out AllocationResult position))
                    return position;
            }

            // Allocate texture
            {
                ImageC imageToPack = texture.Image;
                int originalWidth = texture.Image.Width;
                int originalHeight = texture.Image.Height;
                int biggestDimension = Math.Max(imageToPack.Width, imageToPack.Height);

                if (biggestDimension > _maxTextureAllocationSize)
                {
                    float scaleFactor = _maxTextureAllocationSize / (float)biggestDimension;
                    int newWidth = (int)(imageToPack.Width * scaleFactor);
                    int newHeight = (int)(imageToPack.Height * scaleFactor);
                    newWidth = Math.Max(newWidth, 4);
                    newHeight = Math.Max(newHeight, 4);
                    imageToPack = ImageC.Resize(imageToPack, newWidth, newHeight);
                }

                VectorInt2 sizeToPack = imageToPack.Size;
                VectorInt3? position = null;

                for (int packerIndex = 0; position is null; packerIndex++)
                {
                    bool addedPacker = false;
                    if (packerIndex == TexturePackers.Count)
                    {
                        AddPacker();
                        addedPacker = true;
                    }

                    VectorInt2? pos = TexturePackers[packerIndex].TryAdd(sizeToPack);

                    if (pos is not null)
                    {
                        position = new VectorInt3(pos.Value.X, pos.Value.Y, packerIndex);
                        break;
                    }
                    else if (pos is null && addedPacker) /* We added a new empty atlas and the texture did not fit at all*/
                        throw new TextureAtlasFullException();
                }

                // Upload texture
                InitializeTexture();
                EnsureTextureCapacity();
                TextureLoad.Update(GraphicsDevice, Texture, imageToPack, position.Value);
                var result = new AllocationResult(position.Value, new VectorInt2(originalWidth, originalHeight), new VectorInt2(imageToPack.Width, imageToPack.Height), new VectorInt2(_textureAtlasSize, _textureAtlasSize));
                PackedTextures.Add(texture, result);
                return result;
            }
        }

        // At runtime we can't be sure how many textures the array contains,
        // so we create a new texture array on demand and copy the old content over
        private void EnsureTextureCapacity()
        {
            var arraySize = Texture.ArraySize;

            if (TexturePackers.Count > arraySize)
            {
                var newTexture = Texture2D.New(GraphicsDevice, _textureAtlasSize, _textureAtlasSize, DxgiFormat.B8G8R8A8_UNorm, TextureFlags.ShaderResource, TexturePackers.Count, ResourceUsage.Default);

                for (int i = 0; i < arraySize; i++)
                {
                    var fromSubresource = Texture.GetSubResourceIndex(i, 0);
                    var toSubresource = newTexture.GetSubResourceIndex(i, 0);
                    GraphicsDevice.Copy(Texture, fromSubresource, newTexture, toSubresource);
                }
                Texture?.Dispose();
                Texture = newTexture;
            }
        }

        private void InitializeTexture()
        {
            if (Texture is null)
                Texture = Texture2D.New(GraphicsDevice, _textureAtlasSize, _textureAtlasSize, DxgiFormat.B8G8R8A8_UNorm, TextureFlags.ShaderResource, TexturePackers.Count, ResourceUsage.Default);
        }

        public class TextureAtlasFullException : Exception
        {
            public TextureAtlasFullException()
                : base("Texture atlas is full. Unable to add a wad texture.")
            { }
        }
    }
}
