#nullable enable

using System.Windows.Media;
using TombLib.LevelData;
using TombLib.WPF.Controls;
using TombLib.WPF.Features.AnimatedTextures;

namespace TombEditor.Controls;

/// <summary>
/// Pure-WPF clone of the previous <c>PanelTextureMapForAnimations</c>: draws every animated-texture
/// set's frame outlines on the atlas, highlighting the selected set and numbering its frames.
/// </summary>
public class WpfAnimatedTextureMapView : WpfTextureMapView, IAnimatedTextureMap
{
    protected override float MaxTextureSize => float.PositiveInfinity;
    protected override bool DrawTriangle => false;

    private AnimatedTextureSet? _selectedSet;
    public AnimatedTextureSet? SelectedSet
    {
        get => _selectedSet;
        set
        {
            _selectedSet = value;
            InvalidateVisual();
        }
    }

    protected override void OnPaintSelection(DrawingContext drawingContext)
    {
        AnimatedTextureSetsPainter.Paint(drawingContext, _editor.Level.Settings.AnimatedTextureSets, _selectedSet,
            ((TextureMapBase)this).VisibleTexture, ToVisualCoord, VisualTreeHelper.GetDpi(this).PixelsPerDip);

        // Current selection (handles, quad) on top.
        base.OnPaintSelection(drawingContext);
    }
}
