using TombLib.Utils;

namespace TombLib.GeometryIO
{
    public class IOMaterial
    {
        public string Name { get; private set; }
        public Texture Texture { get; set; }
        public BlendMode BlendMode { get; set; }
        public bool AdditiveBlending => BlendMode >= BlendMode.Additive;
        public bool DoubleSided { get; set; }
        public int Shininess { get; set; }

        // These 2 fields are used for exporting material texture to split texture pages.

        public string Path { get; set; }
        public int Page { get; set; }

        public IOMaterial(string name)
        {
            Name = name;
        }

        public IOMaterial(string name, Texture texture, string texturePath, BlendMode blendMode, bool doubleSided, int shininess, int page)
        {
            Name = name;
            Texture = texture;
            BlendMode = blendMode;
            DoubleSided = doubleSided;
            Shininess = shininess;
            Path = texturePath;
            Page = page;
        }
    }
}
