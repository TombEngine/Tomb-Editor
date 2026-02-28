using System;
using System.IO;

namespace TombLib.Graphics.Dx11Toolkit
{
    /// <summary>
    /// Minimal SpriteFont placeholder. The legacy text rendering will be replaced
    /// by the new RenderingFont system in a future phase. For now this provides
    /// the type signatures needed by DeviceManager.
    /// Replaces SharpDX.Toolkit.Graphics.SpriteFont.
    /// </summary>
    public class SpriteFont : IDisposable
    {
        public SpriteFontData FontData { get; }
        public GraphicsDevice Device { get; }

        private SpriteFont(GraphicsDevice device, SpriteFontData data)
        {
            Device = device;
            FontData = data;
        }

        public static SpriteFont New(GraphicsDevice device, SpriteFontData data)
        {
            return new SpriteFont(device, data);
        }

        public void Dispose() { }
    }

    /// <summary>
    /// Minimal SpriteFontData placeholder.
    /// Replaces SharpDX.Toolkit.Graphics.SpriteFontData.
    /// </summary>
    public class SpriteFontData
    {
        public char DefaultCharacter { get; set; }
        public byte[] RawData { get; private set; }

        public static SpriteFontData Load(Stream stream)
        {
            var data = new SpriteFontData();
            if (stream != null)
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                data.RawData = ms.ToArray();
            }
            return data;
        }

        public static SpriteFontData Load(byte[] bytes)
        {
            var data = new SpriteFontData();
            if (bytes != null)
                data.RawData = bytes;
            return data;
        }
    }

    /// <summary>
    /// Minimal BasicEffect placeholder.
    /// In the original code, `new BasicEffect(device)` is called but the result is discarded.
    /// This is likely a side-effect hack. This class does nothing.
    /// </summary>
    public class BasicEffect : IDisposable
    {
        public BasicEffect(GraphicsDevice device) { }
        public void Dispose() { }
    }
}
