using TombLib.Graphics.Dx11Toolkit;

namespace TombLib.Graphics
{
    public static class InputLayouts
    {
        public static VertexInputLayout SkinnedVertexLayout { get; } =
            VertexInputLayout.New(0, new VertexElement("POSITION0", 0, DxgiFormat.R32G32B32A32_Float),
                new VertexElement("TEXCOORD0", 0, DxgiFormat.R32G32_Float),
                    new VertexElement("NORMAL0", 0, DxgiFormat.R32G32B32_Float),
                    new VertexElement("TANGENT0", 0, DxgiFormat.R32G32B32_Float),
                    new VertexElement("BINORMAL0", 0, DxgiFormat.R32G32B32_Float),
                    new VertexElement("BLENDWEIGTH0", 0, DxgiFormat.R32G32B32A32_Float),
                    new VertexElement("BLENDINDICES0", 0, DxgiFormat.R32G32B32A32_Float));

        public static VertexInputLayout StaticVertexLayout { get; } =
            VertexInputLayout.New(0, new VertexElement("POSITION0", 0, DxgiFormat.R32G32B32A32_Float),
                new VertexElement("TEXCOORD0", 0, DxgiFormat.R32G32_Float),
                    new VertexElement("NORMAL0", 0, DxgiFormat.R32G32B32_Float),
                    new VertexElement("TANGENT0", 0, DxgiFormat.R32G32B32_Float),
                    new VertexElement("BINORMAL0", 0, DxgiFormat.R32G32B32_Float));
    }
}
