#nullable enable

using System.Windows.Media;
using TombLib.LevelData;
using TombLib.WPF.Features.AnimatedTextures;

namespace WadTool.Controls;

/// <summary>
/// Wad Tool flavour of the WPF texture-map view for the animated-textures editor: same
/// configuration hooks as the shared <see cref="PanelTextureMap"/>, plus the animated-texture
/// set outlines (WPF counterpart of <c>PanelTextureMapForAnimations</c>).
/// </summary>
public class WpfAnimatedTextureMapView : PanelTextureMap, IAnimatedTextureMap
{
    private readonly WadToolClass _tool;

    public WpfAnimatedTextureMapView(WadToolClass tool)
    {
        _tool = tool;
        Initialize(tool);
    }

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
        if (_tool.DestinationWad is not null)
            AnimatedTextureSetsPainter.Paint(drawingContext, _tool.DestinationWad.AnimatedTextureSets, _selectedSet,
                VisibleTexture, ToVisualCoord, VisualTreeHelper.GetDpi(this).PixelsPerDip);

        base.OnPaintSelection(drawingContext);
    }
}
