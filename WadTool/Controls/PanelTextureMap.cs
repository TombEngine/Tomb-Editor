#nullable enable

using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using TombLib.WPF;
using TombLib.WPF.Controls;

namespace WadTool.Controls;

/// <summary>
/// Displays mesh textures and edits UV selections using the shared WPF texture map.
/// </summary>
public class PanelTextureMap : TextureMapBase
{
    private WadToolClass? _tool;

    protected override float TileSelectionSize => 32.0f;
    protected override bool ResetAttributesOnNewSelection => false;
    protected override bool MouseWheelMovesTheTextureInsteadOfZooming => _tool?.Configuration.MeshEditor_MouseWheelMovesTheTextureInsteadOfZooming ?? false;
    protected override float NavigationSpeedKeyMove => 100.0f;
    protected override float NavigationSpeedKeyZoom => 0.15f;
    protected override float NavigationSpeedMouseZoom => (_tool?.Configuration.RenderingItem_NavigationSpeedMouseZoom ?? 1.0f) * 0.00225f;
    protected override float NavigationSpeedMouseWheelZoom => (_tool?.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom ?? 1.0f) * 0.00025f;
    protected override float NavigationMaxZoom => 2000.0f;
    protected override float NavigationMinZoom => 0.5f;
    protected override bool DrawSelectionDirectionIndicators => true;

    /// <summary>Applies navigation preferences for the current editor.</summary>
    public void Initialize(WadToolClass tool) => _tool = tool;

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        var path = VisibleTexture?.AbsolutePath;
        if (string.IsNullOrEmpty(path) || File.Exists(path) || ActualWidth <= 0.0 || ActualHeight <= 0.0)
            return;

        var foreground = TryFindResource("Brush_Text") as Brush ?? Brushes.White;
        var text = new FormattedText(Localizer.Instance["WadTool.TextureMap.ExternalTextureMissing"] + Environment.NewLine + path,
            CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
            FontSize, foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip)
        {
            MaxTextWidth = Math.Max(1.0, ActualWidth - 12.0),
            TextAlignment = TextAlignment.Center,
            Trimming = TextTrimming.CharacterEllipsis
        };
        var origin = new Point(6.0, Math.Max(0.0, ActualHeight - text.Height - 6.0));
        drawingContext.DrawRectangle(Brushes.DarkRed, null, new Rect(0.0, origin.Y - 6.0, ActualWidth, text.Height + 12.0));
        drawingContext.DrawText(text, origin);
    }
}
