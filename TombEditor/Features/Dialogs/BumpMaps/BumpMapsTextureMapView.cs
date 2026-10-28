#nullable enable

using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using TombEditor.Controls;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.BumpMaps;

/// <summary>
/// Texture map specialization that overlays bump-mapping level tiles on top of the texture
/// and snaps selection to the bump-mapping granularity.
/// </summary>
public sealed class BumpMapsTextureMapView : WpfTextureMapView
{
    private static readonly Brush[] BumpBrushes = CreateBumpBrushes();
    private static readonly Brush CoverBrush = CreateFrozenBrush(Color.FromArgb(128, 15, 15, 200));
    private static readonly Pen TileBorderPen = CreateFrozenPen(Colors.White, 1.0);

    private const double BumpStringSize = 0.4;
    private const double BumpProportion = 1.0 / 4.0;

    protected override SelectionPrecisionType GetSelectionPrecision(bool singleVertexMovement = false)
        => new(LevelTexture.BumpMappingGranularity, true);

    protected override float MaxTextureSize => float.PositiveInfinity;
    protected override bool DrawTriangle => false;

    protected override void OnPaintSelection(DrawingContext drawingContext)
    {
        if (base.VisibleTexture is not LevelTexture texture)
            return;

        // Visible tile range in texture space.
        Vector2 start = FromVisualCoord(new Point(0, 0));
        Vector2 end = FromVisualCoord(new Point(ActualWidth, ActualHeight));

        start = Vector2.Min(texture.Image.Size, Vector2.Max(Vector2.Zero, start));
        end = Vector2.Min(texture.Image.Size, Vector2.Max(Vector2.Zero, end));

        int tileStartX = (int)Math.Floor(start.X / LevelTexture.BumpMappingGranularity);
        int tileStartY = (int)Math.Floor(start.Y / LevelTexture.BumpMappingGranularity);
        int tileEndX = (int)Math.Ceiling(end.X / LevelTexture.BumpMappingGranularity);
        int tileEndY = (int)Math.Ceiling(end.Y / LevelTexture.BumpMappingGranularity);

        double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        double fontSize = Math.Max(6, BumpStringSize * BumpProportion * LevelTexture.BumpMappingGranularity * Math.Min(100, ViewScale));
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

        for (int y = tileStartY; y < tileEndY; y++)
        {
            for (int x = tileStartX; x < tileEndX; x++)
            {
                if (x < 0 || x >= texture.BumpMappingWidth || y < 0 || y >= texture.BumpMappingHeight)
                    continue;

                BumpMappingLevel level = texture.GetBumpMapLevel(x, y);

                Vector2 tileStartTex = new Vector2(x, y) * LevelTexture.BumpMappingGranularity;
                Point tileStart = ToVisualCoord(tileStartTex);
                Point tileEnd = ToVisualCoord(tileStartTex + new Vector2(LevelTexture.BumpMappingGranularity));

                var labelArea = new Rect(
                    Math.Min(tileStart.X, tileEnd.X),
                    tileStart.Y * BumpProportion + tileEnd.Y * (1.0 - BumpProportion),
                    Math.Abs(tileEnd.X - tileStart.X),
                    Math.Abs(tileEnd.Y - (tileStart.Y * BumpProportion + tileEnd.Y * (1.0 - BumpProportion))));

                drawingContext.DrawRectangle(BumpBrushes[(int)level], null, labelArea);

                drawingContext.DrawCenteredText(
                    level.ToString(),
                    labelArea,
                    typeface,
                    fontSize,
                    Brushes.Black,
                    pixelsPerDip);

                var tileArea = new Rect(
                    Math.Min(tileStart.X, tileEnd.X),
                    Math.Min(tileStart.Y, tileEnd.Y),
                    Math.Abs(tileEnd.X - tileStart.X),
                    Math.Abs(tileEnd.Y - tileStart.Y));

                drawingContext.DrawRectangle(null, TileBorderPen, tileArea);
            }
        }

        // Fill covered tiles for the current selection.
        if (SelectedTexture.Texture is not null)
        {
            Vector2 p0 = SelectedTexture.TexCoord0 / LevelTexture.FootStepSoundGranularity;
            Vector2 p1 = SelectedTexture.TexCoord1 / LevelTexture.FootStepSoundGranularity;
            Vector2 p2 = SelectedTexture.TexCoord2 / LevelTexture.FootStepSoundGranularity;
            Vector2 p3 = SelectedTexture.TexCoord3 / LevelTexture.FootStepSoundGranularity;

            int xMin = (int)Math.Min(Math.Min(Math.Min(p0.X, p1.X), p2.X), p3.X);
            int xMax = (int)Math.Max(Math.Max(Math.Max(p0.X, p1.X), p2.X), p3.X);
            int yMin = (int)Math.Min(Math.Min(Math.Min(p0.Y, p1.Y), p2.Y), p3.Y);
            int yMax = (int)Math.Max(Math.Max(Math.Max(p0.Y, p1.Y), p2.Y), p3.Y);

            Point selStart = ToVisualCoord(new Vector2(xMin, yMin) * LevelTexture.FootStepSoundGranularity);
            Point selEnd = ToVisualCoord(new Vector2(xMax, yMax) * LevelTexture.FootStepSoundGranularity);

            var selArea = new Rect(
                Math.Min(selStart.X, selEnd.X),
                Math.Min(selStart.Y, selEnd.Y),
                Math.Abs(selEnd.X - selStart.X),
                Math.Abs(selEnd.Y - selStart.Y));

            drawingContext.DrawRectangle(CoverBrush, null, selArea);
        }

        base.OnPaintSelection(drawingContext);
    }

    private static Brush[] CreateBumpBrushes() => new Brush[]
    {
        CreateFrozenBrush(Color.FromArgb(200, 160, 160, 160)), // 0: None
        CreateFrozenBrush(Color.FromArgb(200, 235, 200, 120)), // 1: Level 1
        CreateFrozenBrush(Color.FromArgb(200, 245, 180, 100)), // 2: Level 2
        CreateFrozenBrush(Color.FromArgb(200, 255, 160, 80))   // 3: Level 3
    };

    private static Brush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static Pen CreateFrozenPen(Color color, double thickness)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        var pen = new Pen(brush, thickness);
        pen.Freeze();
        return pen;
    }
}
