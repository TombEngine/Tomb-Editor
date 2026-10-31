using TombLib.LevelData;

namespace TombLib.WPF.Features.AnimatedTextures
{
    /// <summary>
    /// Implemented by the WPF texture-map control used in the animated-textures editor so the view-model
    /// can tell it which set to highlight (the control draws every set's frame outlines on the atlas).
    /// </summary>
    public interface IAnimatedTextureMap
    {
        AnimatedTextureSet? SelectedSet { get; set; }
    }
}
