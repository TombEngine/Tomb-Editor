namespace TombLib.Graphics.Dx11Toolkit
{
    /// <summary>
    /// Base class for GPU resources associated with a <see cref="GraphicsDevice"/>.
    /// </summary>
    public class GraphicsResource : Component
    {
        public GraphicsDevice GraphicsDevice { get; }

        protected GraphicsResource(GraphicsDevice device, string name = null)
        {
            GraphicsDevice = device;
            Name = name;
        }
    }
}
