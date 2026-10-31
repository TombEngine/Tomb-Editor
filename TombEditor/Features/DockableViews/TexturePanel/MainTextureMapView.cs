#nullable enable

using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TombEditor.Controls;
using TombLib.Utils;
using TombLib.WPF;

namespace TombEditor.Features.DockableViews.TexturePanel;

/// <summary>
/// Main-panel specialization that paints the level-wide "default texture" hint on top of the
/// regular selection overlay.
/// </summary>
public sealed class MainTextureMapView : WpfTextureMapView
{
    private const float ContextMenuMaxTravelSquared = 16.0f;

    private static readonly Pen DefaultTexturePen = CreateFrozenPen(Color.FromArgb(230, 238, 150, 238), 2.0);

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        // Right-button-up with negligible travel opens the context menu instead of
        // committing a pan, matching the previous TextureMapContextMenu behaviour.
        if (e.ChangedButton == MouseButton.Right && _startPos.HasValue)
        {
            Point current = e.GetPosition(this);
            var travel = new Vector2((float)current.X, (float)current.Y) - _startPos.Value;

            if (travel.LengthSquared() <= ContextMenuMaxTravelSquared)
                ShowContextMenu();
        }

        base.OnMouseUp(e);
    }

    private void ShowContextMenu()
    {
        if (_editor?.Level?.Settings is not { } settings)
            return;

        string setLabel = Localizer.Instance["TombEditor.MainTextureMap.SetAsDefaultTexture"];
        string clearLabel = Localizer.Instance["TombEditor.MainTextureMap.ClearDefaultTexture"];

        var menu = new ContextMenu { PlacementTarget = this };

        if (_editor.SelectedTexture != TextureArea.None)
        {
            var setDefault = new MenuItem { Header = setLabel };
            setDefault.Click += (_, _) =>
            {
                settings.DefaultTexture = _editor.SelectedTexture;
                InvalidateVisual();
            };
            menu.Items.Add(setDefault);
        }

        if (settings.DefaultTexture != TextureArea.None)
        {
            var clearDefault = new MenuItem { Header = clearLabel };
            clearDefault.Click += (_, _) =>
            {
                settings.DefaultTexture = TextureArea.None;
                InvalidateVisual();
            };
            menu.Items.Add(clearDefault);
        }

        if (menu.Items.Count > 0)
            menu.IsOpen = true;
    }

    protected override void OnPaintSelection(DrawingContext drawingContext)
    {
        base.OnPaintSelection(drawingContext);

        if (_editor?.Level is null)
            return;

        TextureArea defaultTexture = _editor.Level.Settings.DefaultTexture;

        if (defaultTexture == TextureArea.None || defaultTexture.Texture is null)
            return;

        if (defaultTexture.Texture != base.VisibleTexture)
            return;

        Point p0 = ToVisualCoord(defaultTexture.TexCoord0);
        Point p1 = ToVisualCoord(defaultTexture.TexCoord1);
        Point p2 = ToVisualCoord(defaultTexture.TexCoord2);
        Point p3 = ToVisualCoord(defaultTexture.TexCoord3);

        var geometry = new StreamGeometry();

        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(p0, false, true);
            ctx.LineTo(p1, true, false);
            ctx.LineTo(p2, true, false);
            ctx.LineTo(p3, true, false);
        }

        geometry.Freeze();
        drawingContext.DrawGeometry(null, DefaultTexturePen, geometry);
    }

    private static Pen CreateFrozenPen(Color color, double thickness)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();

        var pen = new Pen(brush, thickness)
        {
            LineJoin = PenLineJoin.Round
        };

        pen.Freeze();
        return pen;
    }
}
