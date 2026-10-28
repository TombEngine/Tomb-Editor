#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.WPF.Controls;

/// <summary>
/// WPF port of the previous <c>TombLib.Controls.TextureMapBase</c>.
/// </summary>
/// <remarks>
/// Renders a <see cref="Texture"/> with a draggable rectangular selection (and free-corner refinement)
/// using <see cref="DrawingContext"/>. Mouse and keyboard input mirror the shared control:
/// left-drag selects, right-drag pans, Ctrl + right-drag zooms, wheel zooms (or pans when configured),
/// arrow keys pan and PageUp/PageDown zoom while focused.
/// </remarks>
public abstract class TextureMapBase : Control
{
    protected abstract float TileSelectionSize { get; }
    protected abstract bool ResetAttributesOnNewSelection { get; }
    protected abstract bool MouseWheelMovesTheTextureInsteadOfZooming { get; }
    protected abstract float NavigationSpeedKeyMove { get; }
    protected abstract float NavigationSpeedKeyZoom { get; }
    protected abstract float NavigationSpeedMouseZoom { get; }
    protected abstract float NavigationSpeedMouseWheelZoom { get; }
    protected abstract float NavigationMaxZoom { get; }
    protected abstract float NavigationMinZoom { get; }
    protected abstract bool DrawSelectionDirectionIndicators { get; }

    protected virtual bool DrawTriangle => true;
    protected virtual float MaxTextureSize => 1024.0f;

    private static readonly Pen SelectionPen = CreateFrozenPen(Colors.Yellow, 2.0);
    private static readonly Pen SelectionPenTriangle = CreateFrozenPen(Colors.Red, 2.0);
    private static readonly Brush SelectionFill = CreateFrozenBrush(Color.FromArgb(21, 255, 255, 0));
    private static readonly Brush SelectionFillTriangle = CreateFrozenBrush(Color.FromArgb(33, 255, 0, 0));
    private static readonly Brush SelectionCornerBrush = Brushes.Yellow;
    private static readonly Brush SelectionCornerHighlight = Brushes.DeepSkyBlue;
    private const double SelectionCornerSize = 6.0;
    private const double ViewMargin = 10.0;

    private static readonly Brush CheckerBackground = CreateCheckerBrush();

    private readonly Dictionary<Texture, BitmapSource> _bitmapCache = new();

    private Texture? _visibleTexture;
    private TextureArea _selectedTexture;

    private Vector2 _viewPosition;
    private float _viewScale = 1.0f;

    protected Vector2? _startPos;
    private int? _selectedTexCoordIndex;
    private Vector2? _viewMoveMouseTexCoord;
    private Point _lastMousePosition;

    private readonly DispatcherTimer _keyboardTimer;
    private Key _pressedKey = Key.None;
    private DateTime _keyboardEngagedAt;

    protected bool _allowFreeCornerEdit = true;

    public event EventHandler? SelectedTextureChanged;

    protected TextureMapBase()
    {
        Focusable = true;
        ClipToBounds = true;
        SnapsToDevicePixels = true;
        UseLayoutRounding = true;
        Background = Brushes.Transparent;

        _keyboardTimer = new DispatcherTimer(DispatcherPriority.Input)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _keyboardTimer.Tick += OnKeyboardTick;
    }

    public Texture? VisibleTexture
    {
        get => _visibleTexture;
        set
        {
            if (_visibleTexture == value)
                return;

            ResetVisibleTexture(value);
        }
    }

    public TextureArea SelectedTexture
    {
        get => _selectedTexture;
        set
        {
            if (!(VisibleTexture?.IsAvailable ?? false))
                return;

            if (_selectedTexture == value)
                return;

            _selectedTexture = value;
            SelectedTextureChanged?.Invoke(this, EventArgs.Empty);
            InvalidateVisual();
        }
    }

    public Vector2 ViewPosition
    {
        get => _viewPosition;
        set
        {
            if (_viewPosition == value)
                return;

            _viewPosition = value;
            InvalidateVisual();
        }
    }

    public float ViewScale
    {
        get => _viewScale;
        set
        {
            value = Math.Min(value, NavigationMaxZoom);
            value = Math.Max(value, NavigationMinZoom);

            if (Math.Abs(_viewScale - value) < float.Epsilon)
                return;

            _viewScale = value;
            InvalidateVisual();
        }
    }

    /// <summary>
    /// Centres the view on the given texture area and selects it.
    /// </summary>
    public void ShowTexture(TextureArea area)
    {
        VisibleTexture = area.Texture;
        SelectedTexture = area;

        Vector2 min = Vector2.Min(Vector2.Min(area.TexCoord0, area.TexCoord1), Vector2.Min(area.TexCoord2, area.TexCoord3));
        Vector2 max = Vector2.Max(Vector2.Max(area.TexCoord0, area.TexCoord1), Vector2.Max(area.TexCoord2, area.TexCoord3));

        ViewPosition = (min + max) * 0.5f;
        LimitPosition();
    }

    /// <summary>
    /// Replaces the currently visible texture, optionally fitting the view to the new image.
    /// </summary>
    public void ResetVisibleTexture(Texture? texture, bool fit = false)
    {
        _visibleTexture = texture;

        bool available = texture is { IsAvailable: true };

        float scale = 1.0f;
        float centerX = available ? texture!.Image.Width * 0.5f : 128f;
        float centerY = (float)(ActualHeight * 0.5);

        if (fit && available)
        {
            double widthScale = ActualWidth / texture!.Image.Width;
            double heightScale = ActualHeight / texture.Image.Height;
            scale = (float)(Math.Min(widthScale, heightScale) * 0.8);

            if (texture.Image.Height <= ActualHeight)
                centerY = texture.Image.Height / 2f;
        }

        ViewPosition = new Vector2(centerX, centerY);
        ViewScale = scale;
        InvalidateVisual();
    }

    public Vector2 FromVisualCoord(Point pos, bool limited = true)
    {
        var textureCoord = new Vector2(
            (float)((pos.X - ActualWidth * 0.5) / ViewScale + ViewPosition.X),
            (float)((pos.Y - ActualHeight * 0.5) / ViewScale + ViewPosition.Y));

        if (limited && (VisibleTexture?.IsAvailable ?? false))
            textureCoord = Vector2.Min(VisibleTexture.Image.Size, Vector2.Max(Vector2.Zero, textureCoord));

        return textureCoord;
    }

    public Point ToVisualCoord(Vector2 texCoord)
    {
        return new Point(
            (texCoord.X - ViewPosition.X) * ViewScale + ActualWidth * 0.5,
            (texCoord.Y - ViewPosition.Y) * ViewScale + ActualHeight * 0.5);
    }

    private void MoveToFixedPoint(Point visualPoint, Vector2 worldPoint)
    {
        ViewPosition = -worldPoint;
        ViewPosition = -FromVisualCoord(visualPoint, false);
    }

    private void LimitPosition()
    {
        bool hasTexture = VisibleTexture?.IsAvailable ?? false;
        var minimum = new Vector2(-(float)(ViewMargin / ViewScale));
        var maximum = (hasTexture ? VisibleTexture!.Image.Size : new Vector2(1)) + new Vector2((float)(ViewMargin / ViewScale));
        ViewPosition = Vector2.Min(maximum, Vector2.Max(minimum, ViewPosition));
    }

    protected struct SelectionPrecisionType
    {
        public float Precision { get; set; }
        public bool SelectFullTileAutomatically { get; set; }

        public SelectionPrecisionType(float precision, bool selectFullTileAutomatically)
        {
            Precision = precision;
            SelectFullTileAutomatically = selectFullTileAutomatically;
        }
    }

    protected virtual SelectionPrecisionType GetSelectionPrecision(bool singleVertexMovement = false)
    {
        var modifiers = Keyboard.Modifiers;

        if ((modifiers & ModifierKeys.Alt) != 0)
            return new SelectionPrecisionType(0.0f, false);

        if ((modifiers & ModifierKeys.Control) != 0)
            return new SelectionPrecisionType(1.0f, false);

        if ((modifiers & ModifierKeys.Shift) != 0 || (singleVertexMovement && TileSelectionSize >= 16.0f))
            return new SelectionPrecisionType(16.0f, false);

        return new SelectionPrecisionType(TileSelectionSize, true);
    }

    private double TextureSelectionPointSelectionRadius => TileSelectionSize switch
    {
        <= 2.0f => 1.0,
        <= 4.0f => 2.0,
        <= 8.0f => 4.0,
        <= 16.0f => 8.0,
        _ => 12.0
    };

    private Vector2 Quantize(Vector2 texCoord, bool endX, bool endY, bool singleVertexMovement = false)
    {
        var precision = GetSelectionPrecision(singleVertexMovement);

        if (precision.Precision == 0.0f)
            return texCoord;

        texCoord /= precision.Precision;

        if (singleVertexMovement)
        {
            texCoord = new Vector2((float)Math.Round(texCoord.X), (float)Math.Round(texCoord.Y));
        }
        else
        {
            texCoord = new Vector2(
                endX ? (float)Math.Ceiling(texCoord.X) : (float)Math.Floor(texCoord.X),
                endY ? (float)Math.Ceiling(texCoord.Y) : (float)Math.Floor(texCoord.Y));
        }

        texCoord *= precision.Precision;
        texCoord = Vector2.Min(texCoord, VisibleTexture!.Image.Size);

        return texCoord;
    }

    private void SetRectangularTextureWithMouse(Vector2 texCoordStart, Vector2 texCoordEnd)
    {
        Vector2 startQ = Quantize(texCoordStart, texCoordStart.X > texCoordEnd.X, texCoordStart.Y > texCoordEnd.Y);
        Vector2 endQ = Quantize(texCoordEnd, !(texCoordStart.X > texCoordEnd.X), !(texCoordStart.Y > texCoordEnd.Y));

        endQ = Vector2.Min(startQ + new Vector2(MaxTextureSize),
            Vector2.Max(startQ - new Vector2(MaxTextureSize), endQ));

        var area = SelectedTexture;
        area.TexCoord0 = new Vector2(startQ.X, endQ.Y);
        area.TexCoord1 = startQ;
        area.TexCoord2 = new Vector2(endQ.X, startQ.Y);
        area.TexCoord3 = endQ;

        if ((startQ.X > endQ.X) != (startQ.Y > endQ.Y))
            Swap.Do(ref area.TexCoord0, ref area.TexCoord2);

        area.Texture = VisibleTexture;
        area.ClampToBounds();

        if (ResetAttributesOnNewSelection)
        {
            area.DoubleSided = false;
            area.BlendMode = BlendMode.Normal;
        }

        if (area.TriangleArea != 0 || area.QuadArea != 0)
            SelectedTexture = area;
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        if (!IsKeyboardFocused && Window.GetWindow(this)?.IsActive == true)
            Focus();
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (!IsKeyboardFocused)
            Focus();

        _lastMousePosition = e.GetPosition(this);
        _startPos = null;

        CaptureMouse();

        if (e.ChangedButton == MouseButton.Left)
        {
            var mousePos = FromVisualCoord(_lastMousePosition);

            if (SelectedTexture.Texture == VisibleTexture && _allowFreeCornerEdit)
            {
                var coords = SelectedTexture.TexCoords;

                var sorted = coords
                    .Where(c => Vector2.Distance(c, mousePos) < TextureSelectionPointSelectionRadius)
                    .OrderBy(c => Vector2.Distance(c, mousePos))
                    .ToList();

                if (sorted.Count != 0)
                {
                    _selectedTexCoordIndex = Array.FindIndex(coords, c => c == sorted[0]);
                    InvalidateVisual();
                    return;
                }
            }

            if (_selectedTexCoordIndex != null)
            {
                _selectedTexCoordIndex = null;
                InvalidateVisual();
            }

            _startPos = mousePos;
        }
        else if (e.ChangedButton == MouseButton.Right)
        {
            _viewMoveMouseTexCoord = FromVisualCoord(_lastMousePosition);
            _startPos = new Vector2((float)_lastMousePosition.X, (float)_lastMousePosition.Y);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!(VisibleTexture?.IsAvailable ?? false))
            return;

        var pos = e.GetPosition(this);

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (_selectedTexCoordIndex.HasValue)
            {
                TextureArea current = SelectedTexture;

                Vector2 texMin = new(float.PositiveInfinity);
                Vector2 texMax = new(float.NegativeInfinity);

                for (int i = 0; i < 4; i++)
                {
                    if (i == _selectedTexCoordIndex)
                        continue;

                    texMin = Vector2.Min(texMin, current.GetTexCoord(i));
                    texMax = Vector2.Max(texMax, current.GetTexCoord(i));
                }

                Vector2 texMinBounds = texMax - new Vector2(MaxTextureSize);
                Vector2 texMaxBounds = texMin + new Vector2(MaxTextureSize);

                Vector2 newCoord = FromVisualCoord(pos);

                float minArea = float.PositiveInfinity;
                TextureArea bestArea = current;

                for (int i = 0; i < 4; i++)
                {
                    current.SetTexCoord(_selectedTexCoordIndex.Value,
                        Vector2.Min(texMaxBounds, Vector2.Max(texMinBounds,
                            Quantize(newCoord, (i & 1) != 0, (i & 2) != 0, true))));

                    float area = Math.Abs(current.QuadArea);

                    if (area < minArea)
                    {
                        bestArea = current;
                        minArea = area;
                    }
                }

                SelectedTexture = bestArea;
            }

            if (_startPos.HasValue)
                SetRectangularTextureWithMouse(_startPos.Value, FromVisualCoord(pos));
        }
        else if (e.RightButton == MouseButtonState.Pressed && _viewMoveMouseTexCoord.HasValue)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                double relativeDeltaY = (pos.Y - _lastMousePosition.Y) / Math.Max(1.0, ActualHeight);
                ViewScale *= (float)Math.Exp(NavigationSpeedMouseZoom * relativeDeltaY);
            }
            else
            {
                MoveToFixedPoint(pos, _viewMoveMouseTexCoord.Value);
                LimitPosition();
            }
        }

        _lastMousePosition = pos;
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (!(VisibleTexture?.IsAvailable ?? false))
        {
            ReleaseMouseCapture();
            return;
        }

        if (e.ChangedButton == MouseButton.Left && _startPos.HasValue)
        {
            if (GetSelectionPrecision().SelectFullTileAutomatically)
                SetRectangularTextureWithMouse(_startPos.Value, FromVisualCoord(e.GetPosition(this)));
        }

        _startPos = null;
        _viewMoveMouseTexCoord = null;
        ReleaseMouseCapture();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (!(VisibleTexture?.IsAvailable ?? false))
            return;

        if (MouseWheelMovesTheTextureInsteadOfZooming)
        {
            ViewPosition -= 440.0f * new Vector2(0.0f, e.Delta * NavigationSpeedMouseWheelZoom);
            LimitPosition();
            InvalidateVisual();
        }
        else
        {
            Vector2 fixedPointInWorld = FromVisualCoord(e.GetPosition(this));
            ViewScale *= (float)Math.Exp(e.Delta * NavigationSpeedMouseWheelZoom);
            MoveToFixedPoint(e.GetPosition(this), fixedPointInWorld);
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (Keyboard.Modifiers != ModifierKeys.None)
            return;

        if (e.Key is Key.Up or Key.Down or Key.Left or Key.Right or Key.PageUp or Key.PageDown)
        {
            if (_pressedKey != e.Key)
            {
                _pressedKey = e.Key;
                _keyboardEngagedAt = DateTime.UtcNow;

                if (!_keyboardTimer.IsEnabled)
                    _keyboardTimer.Start();
            }

            e.Handled = true;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        StopKeyboard();
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        StopKeyboard();
    }

    private void StopKeyboard()
    {
        _pressedKey = Key.None;
        _keyboardTimer.Stop();
    }

    private void OnKeyboardTick(object? sender, EventArgs e)
    {
        if (_pressedKey == Key.None)
        {
            _keyboardTimer.Stop();
            return;
        }

        double elapsedSeconds = (DateTime.UtcNow - _keyboardEngagedAt).TotalSeconds;
        float multiplier = (float)Math.Min(4.0, 1.0 + elapsedSeconds);

        switch (_pressedKey)
        {
            case Key.Up:
                ViewPosition += new Vector2(0.0f, -NavigationSpeedKeyMove / ViewScale * multiplier);
                break;
            case Key.Down:
                ViewPosition += new Vector2(0.0f, NavigationSpeedKeyMove / ViewScale * multiplier);
                break;
            case Key.Left:
                ViewPosition += new Vector2(-NavigationSpeedKeyMove / ViewScale * multiplier, 0.0f);
                break;
            case Key.Right:
                ViewPosition += new Vector2(NavigationSpeedKeyMove / ViewScale * multiplier, 0.0f);
                break;
            case Key.PageUp:
                ViewScale *= (float)Math.Exp(NavigationSpeedKeyZoom * multiplier);
                break;
            case Key.PageDown:
                ViewScale *= (float)Math.Exp(-NavigationSpeedKeyZoom * multiplier);
                break;
        }

        LimitPosition();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        var clip = new Rect(0, 0, ActualWidth, ActualHeight);
        drawingContext.PushClip(new RectangleGeometry(clip));

        try
        {
            // Fill the whole surface so the entire control is hit-testable. A templateless Control
            // does not render its Background, so without this only the drawn texture / message glyphs
            // would receive mouse input - meaning the empty "click here to load" hint area, and the
            // margins around a loaded texture, would silently swallow clicks.
            drawingContext.DrawRectangle(Background ?? Brushes.Transparent, null, clip);

            if (VisibleTexture?.IsAvailable ?? false)
            {
                RenderTexture(drawingContext);
                OnPaintSelection(drawingContext);
            }
            else
            {
                RenderMissingTexture(drawingContext);
            }
        }
        finally
        {
            drawingContext.Pop();
        }
    }

    private void RenderTexture(DrawingContext drawingContext)
    {
        Texture texture = VisibleTexture!;

        Point drawStart = ToVisualCoord(Vector2.Zero);
        Point drawEnd = ToVisualCoord(new Vector2(texture.Image.Width, texture.Image.Height));

        var drawArea = new Rect(
            Math.Min(drawStart.X, drawEnd.X),
            Math.Min(drawStart.Y, drawEnd.Y),
            Math.Abs(drawEnd.X - drawStart.X),
            Math.Abs(drawEnd.Y - drawStart.Y));

        if (drawArea.Width <= 0 || drawArea.Height <= 0)
            return;

        drawingContext.DrawRectangle(CheckerBackground, null, drawArea);

        BitmapSource bitmap = GetOrCreateBitmap(texture);
        drawingContext.DrawImage(bitmap, drawArea);
    }

    private void RenderMissingTexture(DrawingContext drawingContext)
    {
        string message = GetMissingTextureMessage();

        if (string.IsNullOrEmpty(message))
            return;

        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        var foreground = Foreground ?? Brushes.Gray;
        var bounds = new Rect(0, 0, ActualWidth, ActualHeight);

        drawingContext.DrawCenteredText(
            message,
            bounds,
            typeface,
            FontSize,
            foreground,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }

    /// <summary>
    /// Overridable text shown when the texture cannot be displayed.
    /// </summary>
    protected virtual string GetMissingTextureMessage() => string.Empty;

    protected virtual void OnPaintSelection(DrawingContext drawingContext)
    {
        if (SelectedTexture.Texture is null || VisibleTexture is null)
            return;

        if (!SelectedTexture.Texture.Equals(VisibleTexture))
            return;

        TextureArea area = SelectedTexture;

        Point p0 = ToVisualCoord(area.TexCoord0);
        Point p1 = ToVisualCoord(area.TexCoord1);
        Point p2 = ToVisualCoord(area.TexCoord2);
        Point p3 = ToVisualCoord(area.TexCoord3);

        // Quad fill (P0, P2, P3).
        DrawPolygon(drawingContext, SelectionFill, null, p0, p2, p3);

        // Triangle fill (P0, P1, P2).
        if (DrawTriangle)
            DrawPolygon(drawingContext, SelectionFillTriangle, null, p0, p1, p2);

        // Quad outline (P0, P1, P2, P3).
        DrawPolygon(drawingContext, null, SelectionPen, p0, p1, p2, p3);

        // Triangle outline (P0, P1, P2).
        if (DrawTriangle)
            DrawPolygon(drawingContext, null, SelectionPenTriangle, p0, p1, p2);

        if (DrawSelectionDirectionIndicators)
            DrawDirectionArrows(drawingContext, p0, p1, p2, p3);

        // Corner handles.
        var points = new[] { p0, p1, p2, p3 };

        for (int i = 0; i < points.Length; i++)
        {
            Brush brush = _selectedTexCoordIndex == i ? SelectionCornerHighlight : SelectionCornerBrush;
            var corner = new Rect(
                points[i].X - SelectionCornerSize * 0.5,
                points[i].Y - SelectionCornerSize * 0.5,
                SelectionCornerSize,
                SelectionCornerSize);
            drawingContext.DrawRectangle(brush, null, corner);
        }
    }

    private void DrawDirectionArrows(DrawingContext drawingContext, Point p0, Point p1, Point p2, Point p3)
    {
        var points = new[] { p0, p1, p2, p3 };

        for (int i = 0; i < 4; i++)
        {
            Point fromP = points[i];
            Point toP = points[(i + 1) % 4];

            var from = new Vector2((float)fromP.X, (float)fromP.Y);
            var to = new Vector2((float)toP.X, (float)toP.Y);
            Vector2 center = (from + to) * 0.5f;
            Vector2 direction = Vector2.Normalize(to - from);
            var perpendicular = new Vector2(direction.Y, -direction.X);

            var arrowEdges = new[]
            {
                new Vector2(-6, 4),
                new Vector2(-6, -4),
                new Vector2(8, 0)
            };

            Point Transform(Vector2 v) => new(
                center.X + Vector2.Dot(v, direction),
                center.Y + Vector2.Dot(v, perpendicular));

            Brush brush = i == 0 || i == 1
                ? SelectionPenTriangle.Brush
                : SelectionPen.Brush;

            DrawPolygon(drawingContext, brush, null, Transform(arrowEdges[0]), Transform(arrowEdges[1]), Transform(arrowEdges[2]));
        }
    }

    private static void DrawPolygon(DrawingContext drawingContext, Brush? fill, Pen? pen, params Point[] points)
    {
        if (points.Length == 0)
            return;

        var geometry = new StreamGeometry();

        using (StreamGeometryContext context = geometry.Open())
        {
            context.BeginFigure(points[0], fill is not null, true);

            for (int i = 1; i < points.Length; i++)
                context.LineTo(points[i], pen is not null, false);
        }

        geometry.Freeze();
        drawingContext.DrawGeometry(fill, pen, geometry);
    }

    /// <summary>
    /// Lazily creates a frozen WPF bitmap mirroring the texture's BGRA pixel data.
    /// </summary>
    private BitmapSource GetOrCreateBitmap(Texture texture)
    {
        if (_bitmapCache.TryGetValue(texture, out BitmapSource? cached))
            return cached;

        ImageC image = texture.Image;
        int width = image.Width;
        int height = image.Height;

        var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        bitmap.WritePixels(new Int32Rect(0, 0, width, height), image.ToByteArray(), width * 4, 0);
        bitmap.Freeze();

        _bitmapCache[texture] = bitmap;
        return bitmap;
    }

    /// <summary>
    /// Drops the cached WPF bitmap for the given texture; call this when the texture's pixel data
    /// changes outside the control.
    /// </summary>
    public void InvalidateBitmapCache(Texture texture)
    {
        _bitmapCache.Remove(texture);
        InvalidateVisual();
    }

    public void InvalidateBitmapCache()
    {
        _bitmapCache.Clear();
        InvalidateVisual();
    }

    private static Pen CreateFrozenPen(Color color, double thickness)
    {
        var pen = new Pen(new SolidColorBrush(color), thickness)
        {
            LineJoin = PenLineJoin.Round
        };
        pen.Freeze();
        return pen;
    }

    private static Brush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static DrawingBrush CreateCheckerBrush()
    {
        // Classic 8x8 alpha-checker pattern used as the texture-map background.
        var dark = new SolidColorBrush(Color.FromRgb(102, 102, 102));
        var light = new SolidColorBrush(Color.FromRgb(153, 153, 153));
        dark.Freeze();
        light.Freeze();

        var group = new DrawingGroup();

        group.Children.Add(new GeometryDrawing
        {
            Brush = dark,
            Geometry = new RectangleGeometry(new Rect(0, 0, 16, 16))
        });

        var lightGroup = new GeometryGroup();
        lightGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 8, 8)));
        lightGroup.Children.Add(new RectangleGeometry(new Rect(8, 8, 8, 8)));

        group.Children.Add(new GeometryDrawing
        {
            Brush = light,
            Geometry = lightGroup
        });

        group.Freeze();

        var brush = new DrawingBrush(group)
        {
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, 16, 16),
            ViewportUnits = BrushMappingMode.Absolute,
            Stretch = Stretch.None
        };

        brush.Freeze();
        return brush;
    }
}
