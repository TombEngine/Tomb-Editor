#nullable enable

using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using TombEditor.Controls;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.FootStepSounds;

/// <summary>
/// Texture map specialization that overlays foot-step sound tiles on top of the texture and
/// snaps selection to the foot-step-sound granularity. Free-corner editing is disabled.
/// </summary>
public sealed class FootStepSoundsTextureMapView : WpfTextureMapView
{
    private const byte BrushAlpha = 212;
    private static readonly Brush[] SoundBrushes = CreateSoundBrushes();
    private static readonly Brush CoverBrush = CreateFrozenBrush(Color.FromArgb(128, 15, 15, 200));
    private static readonly Pen TileBorderPen = CreateFrozenPen(Colors.White, 1.0);

    private const double LabelTextSize = 0.4;
    private const double LabelProportion = 1.0 / 4.0;

    public FootStepSoundsTextureMapView()
    {
        _allowFreeCornerEdit = false;
    }

    protected override SelectionPrecisionType GetSelectionPrecision(bool singleVertexMovement = false)
        => new(LevelTexture.FootStepSoundGranularity, true);

    protected override float MaxTextureSize => float.PositiveInfinity;
    protected override bool DrawTriangle => false;

    protected override void OnPaintSelection(DrawingContext drawingContext)
    {
        if (base.VisibleTexture is not LevelTexture texture)
            return;

        Vector2 start = FromVisualCoord(new Point(0, 0));
        Vector2 end = FromVisualCoord(new Point(ActualWidth, ActualHeight));

        start = Vector2.Min(texture.Image.Size, Vector2.Max(Vector2.Zero, start));
        end = Vector2.Min(texture.Image.Size, Vector2.Max(Vector2.Zero, end));

        int tileStartX = (int)Math.Floor(start.X / LevelTexture.FootStepSoundGranularity);
        int tileStartY = (int)Math.Floor(start.Y / LevelTexture.FootStepSoundGranularity);
        int tileEndX = (int)Math.Ceiling(end.X / LevelTexture.FootStepSoundGranularity);
        int tileEndY = (int)Math.Ceiling(end.Y / LevelTexture.FootStepSoundGranularity);

        double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        double fontSize = Math.Max(6, LabelTextSize * LabelProportion * LevelTexture.FootStepSoundGranularity * Math.Min(100, ViewScale));
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

        for (int y = tileStartY; y < tileEndY; y++)
        {
            for (int x = tileStartX; x < tileEndX; x++)
            {
                if (x < 0 || x >= texture.FootStepSoundWidth || y < 0 || y >= texture.FootStepSoundHeight)
                    continue;

                TextureFootStep.Type sound = texture.GetFootStepSound(x, y);
                Brush brush = SoundBrushes[(int)sound];

                Vector2 tileStartTex = new Vector2(x, y) * LevelTexture.FootStepSoundGranularity;
                Point tileStart = ToVisualCoord(tileStartTex);
                Point tileEnd = ToVisualCoord(tileStartTex + new Vector2(LevelTexture.FootStepSoundGranularity));

                double labelTop = tileStart.Y * LabelProportion + tileEnd.Y * (1.0 - LabelProportion);
                var labelArea = new Rect(
                    Math.Min(tileStart.X, tileEnd.X),
                    labelTop,
                    Math.Abs(tileEnd.X - tileStart.X),
                    Math.Abs(tileEnd.Y - labelTop));

                drawingContext.DrawRectangle(brush, null, labelArea);

                drawingContext.DrawCenteredText(
                    sound.ToString().SplitCamelcase(),
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

        // Highlight the selection covered tiles.
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

    private static Brush[] CreateSoundBrushes() => new Brush[]
    {
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 255, 188, 143)), // Mud
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 220, 224, 250)), // Snow
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 190, 190, 10)),  // Sand
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 128, 128, 128)), // Gravel
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 140, 170, 250)), // Ice
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 40, 80, 230)),   // Water
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 160, 160, 170)), // Stone
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 222, 184, 135)), // Wood
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 190, 180, 180)), // Metal
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 244, 164, 96)),  // Marble
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 34, 139, 34)),   // Grass
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 112, 128, 144)), // Concrete
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 111, 92, 67)),   // Old Wood
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 205, 133, 63)),  // Old Metal
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 114, 222, 231)), // Custom 1
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 139, 113, 255)), // Custom 2
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 240, 128, 164)), // Custom 3
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 249, 74, 92)),   // Custom 4
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 238, 139, 91)),  // Custom 5
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 114, 216, 129)), // Custom 6
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 88, 241, 169)),  // Custom 7
        CreateFrozenBrush(Color.FromArgb(BrushAlpha, 170, 80, 169))   // Custom 8
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
