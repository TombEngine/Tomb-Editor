using System;
using System.Collections.Generic;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.WPF.Features.AnimatedTextures
{
    /// <summary>
    /// Host context for the WPF animated-textures editor. Mirrors the shared
    /// <c>FormAnimatedTextures.IAnimatedTexturesContext</c> so each app (Tomb Editor, Wad Tool) can
    /// supply its own textures and live <see cref="AnimatedTextureSet"/> list.
    /// </summary>
    public interface IAnimatedTexturesContext
    {
        TextureArea SelectedTexture { get; set; }
        List<Texture> AvailableTextures { get; }
        List<AnimatedTextureSet> AnimatedTextureSets { get; }
        Action OnAnimatedTexturesChanged { get; set; }
        Action OnContextInvalidated { get; set; }
        TRVersion.Game Version { get; }
    }
}
