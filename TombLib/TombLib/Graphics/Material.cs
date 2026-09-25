using TombLib.Utils;

namespace TombLib.Graphics
{
    public class Material
    {
        public const string DoubleSidedSuffix = "DS";

        // Opaque (normal) prefix.
        public const string Material_Opaque = "TeOp";
        public const string Material_OpaqueDoubleSided = "TeOpDS";

        // Blend mode prefixes. "TeBl" is kept for backward compatibility with Additive.
        public const string Material_AdditiveBlending            = "TeBl";
        public const string Material_AdditiveBlendingDoubleSided = "TeBlDS";
        public const string Material_AlphaTest                   = "TeBlAT";
        public const string Material_AlphaTestDoubleSided        = "TeBlATDS";
        public const string Material_Distortion                  = "TeBlDis";
        public const string Material_DistortionDoubleSided       = "TeBlDisDS";
        public const string Material_NoZTest                     = "TeBlNZ";
        public const string Material_NoZTestDoubleSided          = "TeBlNZDS";
        public const string Material_Subtract                    = "TeBlSub";
        public const string Material_SubtractDoubleSided         = "TeBlSubDS";
        public const string Material_Wireframe                   = "TeBlWf";
        public const string Material_WireframeDoubleSided        = "TeBlWfDS";
        public const string Material_Exclude                     = "TeBlExc";
        public const string Material_ExcludeDoubleSided          = "TeBlExcDS";
        public const string Material_Screen                      = "TeBlScr";
        public const string Material_ScreenDoubleSided           = "TeBlScrDS";
        public const string Material_Lighten                     = "TeBlLig";
        public const string Material_LightenDoubleSided          = "TeBlLigDS";
        public const string Material_AlphaBlend                  = "TeBlAB";
        public const string Material_AlphaBlendDoubleSided       = "TeBlABDS";

        // Lookup from prefix to blend mode.
        // Longer (more specific) entries must precede shorter generic ones to prevent
        // partial matches (e.g., "TeBlSub" must come before "TeBl").
        private static readonly (string Prefix, BlendMode Mode)[] BlendModePrefixLookup =
        {
            (Material_AlphaTest,        BlendMode.AlphaTest),
            (Material_Distortion,       BlendMode.Distortion),
            (Material_NoZTest,          BlendMode.NoZTest),
            (Material_Subtract,         BlendMode.Subtract),
            (Material_Wireframe,        BlendMode.Wireframe),
            (Material_Exclude,          BlendMode.Exclude),
            (Material_Screen,           BlendMode.Screen),
            (Material_Lighten,          BlendMode.Lighten),
            (Material_AlphaBlend,       BlendMode.AlphaBlend),
            (Material_AdditiveBlending, BlendMode.Additive),
            (Material_Opaque,           BlendMode.Normal),
        };

        public string Name { get; private set; }
        public Texture Texture { get; set; }
        public bool AdditiveBlending { get; set; }
        public bool DoubleSided { get; set; }
        public int Shininess { get; set; }

        public Material(string name)
        {
            Name = name;
        }

        public Material(string name, Texture texture, bool additiveBlending, bool doubleSided, int shininess)
        {
            Name = name;
            Texture = texture;
            AdditiveBlending = additiveBlending;
            DoubleSided = doubleSided;
            Shininess = shininess;
        }

        public void SetStates(SharpDX.Toolkit.Graphics.GraphicsDevice device, bool transparent)
        {
            if (transparent && AdditiveBlending)
                device.SetBlendState(device.BlendStates.Additive);
            else if (transparent)
                device.SetBlendState(device.BlendStates.NonPremultiplied);
            else
                device.SetBlendState(device.BlendStates.Opaque);

            if (DoubleSided)
                device.SetRasterizerState(device.RasterizerStates.CullNone);
            else
                device.SetRasterizerState(device.RasterizerStates.CullBack);
        }

        // Returns the material name prefix for the given blend mode.
        public static string GetPrefixForBlendMode(BlendMode mode)
        {
            foreach (var (prefix, blendMode) in BlendModePrefixLookup)
                if (blendMode == mode)
                    return prefix;
            return Material_Opaque;
        }

        // Parses the blend mode from a material name using the Te prefix convention.
        public static BlendMode GetBlendModeFromName(string name)
        {
            foreach (var (prefix, mode) in BlendModePrefixLookup)
                if (MatchesMaterialPrefix(name, prefix))
                    return mode;
            return BlendMode.Normal;
        }

        // Returns true if the material name indicates a double-sided surface.
        public static bool GetDoubleSidedFromName(string name)
        {
            foreach (var (prefix, _) in BlendModePrefixLookup)
                if (MatchesMaterialPrefix(name, prefix + DoubleSidedSuffix))
                    return true;
            return false;
        }

        // Returns true if name starts with prefix and is immediately followed by "DS", "_", or end of string.
        private static bool MatchesMaterialPrefix(string name, string prefix)
        {
            if (!name.StartsWith(prefix))
                return false;
            int after = prefix.Length;
            if (after >= name.Length || name[after] == '_')
                return true;
            if (after + 1 < name.Length && name[after] == 'D' && name[after + 1] == 'S')
                return true;
            return false;
        }
    }
}
