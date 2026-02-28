using NLog;
using TombLib.Graphics.Dx11Toolkit;
using System;
using System.IO;
using TombLib.Utils;
using Texture2D = TombLib.Graphics.Dx11Toolkit.Texture2D;

namespace TombLib.Graphics
{
    public static class TextureLoad
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static Texture2D Load(GraphicsDevice graphicsDevice, ImageC image, ResourceUsage usage = ResourceUsage.Immutable)
        {
            if (graphicsDevice == null)
                return null;

            Texture2D result = null;

            try
            {
                image.GetIntPtr((IntPtr data) =>
                {
                    result = Texture2D.New(graphicsDevice, image.Width, image.Height, 1,
                        DxgiFormat.B8G8R8A8_UNorm,
                        new[] { new DataBox(data, image.Width * ImageC.PixelSize, 0) },
                        TextureFlags.ShaderResource, 1, usage);
                });
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to load texture, falling back to red placeholder.");
                return Load(graphicsDevice, ImageC.Red, usage);
            }

            return result;
        }

        public static Texture2D Load(GraphicsDevice graphicsDevice, Stream stream)
        {
            return Load(graphicsDevice, ImageC.FromStream(stream));
        }

        public static Texture2D Load(GraphicsDevice graphicsDevice, string path)
        {
            return Load(graphicsDevice, ImageC.FromFile(path));
        }

        public static void Update(GraphicsDevice graphicsDevice, Texture2D texture, ImageC image, VectorInt3 position)
        {
            if (image.Width == 0 || image.Height == 0)
                return;

            image.GetIntPtr((IntPtr data) =>
            {
                var region = new ResourceRegion
                {
                    Left = position.X,
                    Right = position.X + image.Width,
                    Top = position.Y,
                    Bottom = position.Y + image.Height,
                    Front = 0,
                    Back = 1,
                };
                texture.SetData(graphicsDevice, new DataPointer(data, image.DataSize), position.Z, 0, region);
            });
        }
    }
}
