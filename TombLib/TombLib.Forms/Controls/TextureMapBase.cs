using DarkUI.Config;
using DarkUI.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using Color = System.Drawing.Color;
using RectangleF = System.Drawing.RectangleF;

namespace TombLib.Controls
{
    public abstract class TextureMapBase : DarkPanel
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


        private Texture _visibleTexture;
        private TextureArea _selectedTexture;

        private Vector2 _viewPosition;
        private float _viewScale = 1.0f;

        protected Vector2? _startPos;
        private int? _selectedTexCoordIndex;
        private Vector2? _viewMoveMouseTexCoord;
        private Point _lastMousePosition;
        private readonly MovementTimer _movementTimer;

        private static readonly Pen textureSelectionPen = new Pen(Brushes.Yellow, 2.0f) { LineJoin = LineJoin.Round };
        private static readonly Pen textureSelectionPenTriangle = new Pen(Brushes.Red, 2.0f) { LineJoin = LineJoin.Round };
        private static readonly Brush textureSelectionBrush = new SolidBrush(Color.FromArgb(21, textureSelectionPen.Color.R, textureSelectionPen.Color.G, textureSelectionPen.Color.B));
        private static readonly Brush textureSelectionBrushTriangle = new SolidBrush(Color.FromArgb(33, textureSelectionPenTriangle.Color.R, textureSelectionPenTriangle.Color.G, textureSelectionPenTriangle.Color.B));
        private static readonly Brush textureSelectionBrushSelection = Brushes.DeepSkyBlue;
        private const float textureSelectionPointWidth = 6.0f;
        private const float viewMargin = 10;

        private float TextureSelectionPointSelectionRadius => TileSelectionSize switch
        {
            <= 2.0f => 1.0f,
            <= 4.0f => 2.0f,
            <= 8.0f => 4.0f,
            <= 16.0f => 8.0f,
            _ => 12.0f
        };

        private readonly DarkScrollBarC _hScrollBar = new DarkScrollBarC { ScrollOrientation = DarkScrollOrientation.Horizontal };
        private readonly DarkScrollBarC _vScrollBar = new DarkScrollBarC { ScrollOrientation = DarkScrollOrientation.Vertical };

        protected int _scrollSize => Consts.ScrollBarSize;
        protected int _scrollSizeTotal => _scrollSize + 1;

        protected bool _allowFreeCornerEdit = true;

        public TextureMapBase()
        {
            // Change default state
            SetStyle(ControlStyles.Selectable, true);
            BorderStyle = BorderStyle.FixedSingle;
            DoubleBuffered = true;

            // Scroll bars
            _hScrollBar.Size = new Size(Width - _scrollSize - 2, _scrollSize);
            _hScrollBar.Location = new Point(1, Height - _scrollSize - 1);
            _hScrollBar.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
            _hScrollBar.ValueChanged += (sender, e) => { ViewPosition = new Vector2((float)_hScrollBar.ValueCentered, ViewPosition.Y); };

            _vScrollBar.Size = new Size(_scrollSize, Height - _scrollSize - 2);
            _vScrollBar.Location = new Point(Width - _scrollSize - 1, 1);
            _vScrollBar.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            _vScrollBar.ValueChanged += (sender, e) => { ViewPosition = new Vector2(ViewPosition.X, (float)_vScrollBar.ValueCentered); };

            Controls.Add(_vScrollBar);
            Controls.Add(_hScrollBar);

            _movementTimer = new MovementTimer(MoveTimerTick);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hScrollBar.Dispose();
                _vScrollBar.Dispose();
                _movementTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        public void ShowTexture(TextureArea area)
        {
            VisibleTexture = area.Texture;
            SelectedTexture = area;

            Vector2 min = Vector2.Min(Vector2.Min(area.TexCoord0, area.TexCoord1), Vector2.Min(area.TexCoord2, area.TexCoord3));
            Vector2 max = Vector2.Max(Vector2.Max(area.TexCoord0, area.TexCoord1), Vector2.Max(area.TexCoord2, area.TexCoord3));

            ViewPosition = (min + max) * 0.5f;

            LimitPosition();
        }

        private void UpdateScrollBars()
        {
            bool hasTexture = VisibleTexture?.IsAvailable ?? false;

            _vScrollBar.SetViewCentered(
                -viewMargin,
                (hasTexture ? VisibleTexture.Image.Height : 1) + viewMargin * 2,
                (ClientSize.Height - _scrollSizeTotal) / ViewScale,
                ViewPosition.Y, hasTexture);
            _hScrollBar.SetViewCentered(
                -viewMargin,
                (hasTexture ? VisibleTexture.Image.Width : 1) + viewMargin * 2,
                (ClientSize.Width - _scrollSizeTotal) / ViewScale,
                ViewPosition.X, hasTexture);
        }

        public Vector2 FromVisualCoord(PointF pos, bool limited = true)
        {
            Vector2 textureCoord = new Vector2(
               (pos.X - (ClientSize.Width - _scrollSizeTotal) * 0.5f) / ViewScale + ViewPosition.X,
               (pos.Y - (ClientSize.Height - _scrollSizeTotal) * 0.5f) / ViewScale + ViewPosition.Y);

            if (limited && (VisibleTexture?.IsAvailable ?? false))
                textureCoord = Vector2.Min(VisibleTexture.Image.Size, Vector2.Max(Vector2.Zero, textureCoord));

            return textureCoord;
        }

        public PointF ToVisualCoord(Vector2 texCoord)
        {
            return new PointF(
                (texCoord.X - ViewPosition.X) * ViewScale + (ClientSize.Width - _scrollSizeTotal) * 0.5f,
                (texCoord.Y - ViewPosition.Y) * ViewScale + (ClientSize.Height - _scrollSizeTotal) * 0.5f);
        }

        private void MoveToFixedPoint(PointF visualPoint, Vector2 worldPoint)
        {
            //Adjust ViewPosition in such a way, that the FixedPoint does not move visually
            ViewPosition = -worldPoint;
            ViewPosition = -FromVisualCoord(visualPoint, false);
        }

        private void LimitPosition()
        {
            bool hasTexture = VisibleTexture?.IsAvailable ?? false;
            Vector2 minimum = new Vector2(-viewMargin / ViewScale);
            Vector2 maximum = (hasTexture ? VisibleTexture.Image.Size : new Vector2(1)) + new Vector2(viewMargin / ViewScale);
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
            if (ModifierKeys.HasFlag(Keys.Alt))
                return new SelectionPrecisionType(0.0f, false);
            else if (ModifierKeys.HasFlag(Keys.Control))
                return new SelectionPrecisionType(1.0f, false);
            else if (ModifierKeys.HasFlag(Keys.Shift) || (singleVertexMovement && TileSelectionSize >= 16.0f))
                return new SelectionPrecisionType(16.0f, false);
            else
                return new SelectionPrecisionType(TileSelectionSize, true);
        }

        protected virtual float MaxTextureSize { get; } = 1024;

        private Vector2 Quantize(Vector2 texCoord, bool endX, bool endY, bool singleVertexMovement = false)
        {
            var selectionPrecision = GetSelectionPrecision(singleVertexMovement);

            if (selectionPrecision.Precision == 0.0f)
                return texCoord;

            texCoord /= selectionPrecision.Precision;

            if (singleVertexMovement)
                texCoord = new Vector2((float)Math.Round(texCoord.X), (float)Math.Round(texCoord.Y));
            else
            {
                texCoord = new Vector2(
                    endX ? (float)Math.Ceiling(texCoord.X) : (float)Math.Floor(texCoord.X),
                    endY ? (float)Math.Ceiling(texCoord.Y) : (float)Math.Floor(texCoord.Y));
            }

            texCoord *= selectionPrecision.Precision;

            // Clamp texture coordinate to image bounds
            texCoord = Vector2.Min(texCoord, VisibleTexture.Image.Size);

            return texCoord;
        }

        private void SetRectangularTextureWithMouse(Vector2 texCoordStart, Vector2 texCoordEnd)
        {
            Vector2 texCoordStartQuantized = Quantize(texCoordStart, texCoordStart.X > texCoordEnd.X, texCoordStart.Y > texCoordEnd.Y);
            Vector2 texCoordEndQuantized = Quantize(texCoordEnd, !(texCoordStart.X > texCoordEnd.X), !(texCoordStart.Y > texCoordEnd.Y));

            texCoordEndQuantized = Vector2.Min(texCoordStartQuantized + new Vector2(MaxTextureSize),
                Vector2.Max(texCoordStartQuantized - new Vector2(MaxTextureSize), texCoordEndQuantized));

            var selectedTexture = SelectedTexture;
            selectedTexture.TexCoord0 = new Vector2(texCoordStartQuantized.X, texCoordEndQuantized.Y);
            selectedTexture.TexCoord1 = texCoordStartQuantized;
            selectedTexture.TexCoord2 = new Vector2(texCoordEndQuantized.X, texCoordStartQuantized.Y);
            selectedTexture.TexCoord3 = texCoordEndQuantized;

            // Avoid mirroring the texture by rectangular selections
            if ((texCoordStartQuantized.X > texCoordEndQuantized.X) != (texCoordStartQuantized.Y > texCoordEndQuantized.Y))
                Swap.Do(ref selectedTexture.TexCoord0, ref selectedTexture.TexCoord2);

            selectedTexture.Texture = VisibleTexture;

            selectedTexture.ClampToBounds();

            // Reset double-sided and blending mode attrib on new picking, if needed
            if (ResetAttributesOnNewSelection)
            {
                selectedTexture.DoubleSided = false;
                selectedTexture.BlendMode = BlendMode.Normal;
            }

            if (selectedTexture.TriangleArea != 0 || selectedTexture.QuadArea != 0)
                SelectedTexture = selectedTexture;
        }

        private void MoveTimerTick(object sender, EventArgs e)
        {
            switch (_movementTimer.MoveKey)
            {
                case Keys.Down:
                    ViewPosition += new Vector2(0.0f, NavigationSpeedKeyMove / ViewScale * _movementTimer.MoveMultiplier);
                    Invalidate();
                    break;
                case Keys.Up:
                    ViewPosition += new Vector2(0.0f, -NavigationSpeedKeyMove / ViewScale * _movementTimer.MoveMultiplier);
                    Invalidate();
                    break;
                case Keys.Left:
                    ViewPosition += new Vector2(-NavigationSpeedKeyMove / ViewScale * _movementTimer.MoveMultiplier, 0.0f);
                    Invalidate();
                    break;
                case Keys.Right:
                    ViewPosition += new Vector2(NavigationSpeedKeyMove / ViewScale * _movementTimer.MoveMultiplier, 0.0f);
                    Invalidate();
                    break;
                case Keys.PageDown:
                    ViewScale *= (float)Math.Exp(-NavigationSpeedKeyZoom * _movementTimer.MoveMultiplier);
                    Invalidate();
                    break;
                case Keys.PageUp:
                    ViewScale *= (float)Math.Exp(NavigationSpeedKeyZoom * _movementTimer.MoveMultiplier);
                    Invalidate();
                    break;
            }
            LimitPosition();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (!Focused && Form.ActiveForm == FindForm())
                Focus(); // Enable keyboard interaction
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!Focused)
                Focus(); // Enable keyboard interaction

            _lastMousePosition = e.Location;
            _startPos = null;

            //https://stackoverflow.com/questions/14191219/receive-mouse-move-even-cursor-is-outside-control
            Capture = true; // Capture mouse for zoom and panning

            switch (e.Button)
            {
                case MouseButtons.Left:
                    var mousePos = FromVisualCoord(e.Location);

                    // Check if mouse was on existing texture
                    if (SelectedTexture.Texture == VisibleTexture)
                    {
                        if (_allowFreeCornerEdit)
                        {
                            var coords = SelectedTexture.TexCoords;

                            var sortedCoords = coords
                                .Where(coord => Vector2.Distance(coord, mousePos) < TextureSelectionPointSelectionRadius)
                                .OrderBy(coord => Vector2.Distance(coord, mousePos))
                                .ToList();

                            if (sortedCoords.Count != 0)
                            {
                                // Select texture coords
                                _selectedTexCoordIndex = Array.FindIndex(coords, coord => coord == sortedCoords.First());
                                Invalidate();
                                break;
                            }
                        }
                    }
                    if (_selectedTexCoordIndex != null)
                    {
                        _selectedTexCoordIndex = null;
                        Invalidate();
                    }

                    // Start selection ...
                    _startPos = mousePos;
                    break;

                case MouseButtons.Right:
                    // Move view with mouse cursor
                    // Mouse cursor is a fixed point
                    _viewMoveMouseTexCoord = FromVisualCoord(e.Location);
                    _startPos = new Vector2(e.Location.X, e.Location.Y);
                    break;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!(VisibleTexture?.IsAvailable ?? false))
                return;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (_selectedTexCoordIndex.HasValue)
                    {
                        TextureArea currentTexture = SelectedTexture;

                        // Determine bounds
                        Vector2 texCoordMin = new Vector2(float.PositiveInfinity);
                        Vector2 texCoordMax = new Vector2(float.NegativeInfinity);
                        for (int i = 0; i < 4; ++i)
                        {
                            if (i == _selectedTexCoordIndex)
                                continue;
                            texCoordMin = Vector2.Min(texCoordMin, currentTexture.GetTexCoord(i));
                            texCoordMax = Vector2.Max(texCoordMax, currentTexture.GetTexCoord(i));
                        }
                        Vector2 texCoordMinBounds = texCoordMax - new Vector2(MaxTextureSize);
                        Vector2 texCoordMaxBounds = texCoordMin + new Vector2(MaxTextureSize);

                        //Move texture coord
                        Vector2 newTextureCoord = FromVisualCoord(e.Location);

                        float minArea = float.PositiveInfinity;
                        TextureArea minAreaTextureArea = currentTexture;
                        for (int i = 0; i < 4; ++i)
                        {
                            currentTexture.SetTexCoord(_selectedTexCoordIndex.Value,
                                Vector2.Min(texCoordMaxBounds, Vector2.Max(texCoordMinBounds,
                                    Quantize(newTextureCoord, (i & 1) != 0, (i & 2) != 0, true))));

                            float area = Math.Abs(currentTexture.QuadArea);
                            if (area < minArea)
                            {
                                minAreaTextureArea = currentTexture;
                                minArea = area;
                            }
                        }

                        // Use the configuration that covers the most area
                        SelectedTexture = minAreaTextureArea;
                    }

                    if (_startPos.HasValue)
                        SetRectangularTextureWithMouse(_startPos.Value, FromVisualCoord(e.Location));
                    break;

                case MouseButtons.Right:
                    // Move view with mouse cursor
                    // Mouse cursor is a fixed point
                    if (_viewMoveMouseTexCoord.HasValue)
                        if (ModifierKeys.HasFlag(Keys.Control))
                        { // Zoom
                            float relativeDeltaY = (e.Location.Y - _lastMousePosition.Y) / (float)Height;
                            ViewScale *= (float)Math.Exp(NavigationSpeedMouseZoom * relativeDeltaY);
                        }
                        else
                        { // Movement
                            MoveToFixedPoint(e.Location, _viewMoveMouseTexCoord.Value);
                            LimitPosition();
                        }
                    break;
            }
            _lastMousePosition = e.Location;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!(VisibleTexture?.IsAvailable ?? false))
                return;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (_startPos.HasValue)
                        if (GetSelectionPrecision().SelectFullTileAutomatically)
                            SetRectangularTextureWithMouse(_startPos.Value, FromVisualCoord(e.Location));
                    break;
            }

            _startPos = null;
            _viewMoveMouseTexCoord = null;
            Capture = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!(VisibleTexture?.IsAvailable ?? false))
                return;

            if (MouseWheelMovesTheTextureInsteadOfZooming)
            {
                ViewPosition -= 440.0f * new Vector2(0.0f, e.Delta * NavigationSpeedMouseWheelZoom);
                LimitPosition();
                Invalidate();
            }
            else
            {
                Vector2 FixedPointInWorld = FromVisualCoord(e.Location);
                ViewScale *= (float)Math.Exp(e.Delta * NavigationSpeedMouseWheelZoom);
                MoveToFixedPoint(e.Location, FixedPointInWorld);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.IntersectClip(new RectangleF(new PointF(), ClientSize - new SizeF(_scrollSizeTotal, _scrollSizeTotal)));

            // Only proceed if texture is actually available
            if (VisibleTexture?.IsAvailable ?? false)
            {
                PointF drawStart = ToVisualCoord(new Vector2(0.0f, 0.0f));
                PointF drawEnd = ToVisualCoord(new Vector2(VisibleTexture.Image.Width, VisibleTexture.Image.Height));
                RectangleF drawArea = RectangleF.FromLTRB(drawStart.X, drawStart.Y, drawEnd.X, drawEnd.Y);

                // Draw background
                using (var textureBrush = new TextureBrush(Properties.Resources.misc_TransparentBackground))
                    e.Graphics.FillRectangle(textureBrush, drawArea);

                // Switch interpolation based on current view scale
                if (ViewScale >= 1.0)
                    e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                else
                    e.Graphics.InterpolationMode = InterpolationMode.Bicubic;

                // Draw image
                VisibleTexture.Image.GetTempSystemDrawingBitmap(tempBitmap =>
                    {
                        // System.Drawing being silly, it draws the first row of pixels only half, so everything would be shifted
                        // To work around it, we have to do some silly coodinate changes :/
                        e.Graphics.DrawImage(tempBitmap,
                            new RectangleF(drawArea.X, drawArea.Y, drawArea.Width + 0.5f * ViewScale, drawArea.Height + 0.5f * ViewScale),
                            new RectangleF(-0.5f, -0.5f, tempBitmap.Width + 0.5f, tempBitmap.Height + 0.5f),
                            GraphicsUnit.Pixel);
                    });

                OnPaintSelection(e);
            }

            // Draw borders
            using (Pen pen = new Pen(Colors.GreySelection, 1.0f))
                e.Graphics.DrawRectangle(pen, new RectangleF(0, 0, ClientSize.Width - _scrollSizeTotal - 1, ClientSize.Height - _scrollSizeTotal - 1));

        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if ((ModifierKeys & (Keys.Control | Keys.Alt | Keys.Shift)) == Keys.None)
                _movementTimer.Engage(e.KeyCode);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _movementTimer.Stop();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _movementTimer.Stop();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UpdateScrollBars();
            Invalidate();
        }

        protected virtual bool DrawTriangle => true;

        protected virtual void OnPaintSelection(PaintEventArgs e)
        {
            if (SelectedTexture.Texture == null || VisibleTexture == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            // Draw selection
            var selectedTexture = SelectedTexture;
            if (selectedTexture.Texture.Equals(VisibleTexture))
            {
                // This texture is currently selected
                PointF[] points = new[]
                {
                    ToVisualCoord(selectedTexture.TexCoord0),
                    ToVisualCoord(selectedTexture.TexCoord1),
                    ToVisualCoord(selectedTexture.TexCoord2),
                    ToVisualCoord(selectedTexture.TexCoord3)
                };

                // Draw fill color
                e.Graphics.FillPolygon(textureSelectionBrush, new[] { points[0], points[2], points[3] });
                if (DrawTriangle)
                    e.Graphics.FillPolygon(textureSelectionBrushTriangle, new[] { points[0], points[1], points[2] });

                // Draw outlines
                e.Graphics.DrawPolygon(textureSelectionPen, points);
                if (DrawTriangle)
                    e.Graphics.DrawPolygon(textureSelectionPenTriangle, new[] { points[0], points[1], points[2] });

                // Draw arrows on quad outlines
                if (DrawSelectionDirectionIndicators)
                    for (int i = 0; i < 4; ++i)
                    {
                        Vector2 from = new Vector2(points[i].X, points[i].Y);
                        Vector2 to = new Vector2(points[(i + 1) % 4].X, points[(i + 1) % 4].Y);
                        Vector2 center = (from + to) * 0.5f;
                        Vector2 direction = Vector2.Normalize(to - from);
                        Vector2 directionPerpendicular = new Vector2(direction.Y, -direction.X);

                        Vector2[] arrowEdges = new[] {
                            new Vector2(-6, 4),
                            new Vector2(-6, -4),
                            new Vector2(8, 0)
                        };

                        // Transform edges into direction (Unfortunately there is no Matrix2x2 type)
                        PointF[] arrowEdgesTransformed = new PointF[arrowEdges.Length];
                        for (int j = 0; j < arrowEdges.Length; ++j)
                            arrowEdgesTransformed[j] = new PointF(
                                center.X + Vector2.Dot(arrowEdges[j], direction),
                                center.Y + Vector2.Dot(arrowEdges[j], directionPerpendicular));

                        Brush brush = i == 0 || i == 1 ? textureSelectionPenTriangle.Brush : textureSelectionPen.Brush;
                        e.Graphics.FillPolygon(brush, arrowEdgesTransformed);
                    }

                // Draw edges as squares
                for (int i = 0; i < 4; ++i)
                {
                    Brush brush = _selectedTexCoordIndex == i ? textureSelectionBrushSelection : textureSelectionPen.Brush;
                    e.Graphics.FillRectangle(brush,
                        points[i].X - textureSelectionPointWidth * 0.5f, points[i].Y - textureSelectionPointWidth * 0.5f,
                        textureSelectionPointWidth, textureSelectionPointWidth);
                }
            }
        }

        [DefaultValue(BorderStyle.FixedSingle)]
        public new BorderStyle BorderStyle
        {
            get { return base.BorderStyle; }
            set { base.BorderStyle = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public Vector2 ViewPosition
        {
            get { return _viewPosition; }
            set
            {
                if (_viewPosition == value)
                    return;
                _viewPosition = value;
                UpdateScrollBars();
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public float ViewScale
        {
            get { return _viewScale; }
            set
            {
                value = Math.Min(value, NavigationMaxZoom);
                value = Math.Max(value, NavigationMinZoom);
                if (_viewScale == value)
                    return;
                _viewScale = value;
                UpdateScrollBars();
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public Texture VisibleTexture
        {
            get { return _visibleTexture; }
            set
            {
                if (_visibleTexture == value)
                    return;
                ResetVisibleTexture(value);
            }
        }

        public void ResetVisibleTexture(Texture texture, bool fit = false)
        {
            _visibleTexture = texture;

            var available = VisibleTexture != null && VisibleTexture.IsAvailable;

            var x = available ? VisibleTexture.Image.Width * 0.5f : 128;
            float y = (ClientSize.Height - _scrollSizeTotal) * 0.5f;

            var scale = 1.0f;

            if (fit & available)
            {
                if (ClientSize.Width > ClientSize.Height)
                {
                    scale = (float)ClientSize.Height / (float)VisibleTexture.Image.Height;

                    if (VisibleTexture.Image.Height >= ClientSize.Height * scale)
                        scale = (float)ClientSize.Width / (float)VisibleTexture.Image.Width;
                }
                else
                {
                    scale = (float)ClientSize.Width / (float)VisibleTexture.Image.Width;

                    if (VisibleTexture.Image.Width >= ClientSize.Width * scale)
                        scale = (float)ClientSize.Height / (float)VisibleTexture.Image.Height;
                }

                scale *= 0.8f;

                if (VisibleTexture.Image.Height <= ClientSize.Height)
                    y = (float)VisibleTexture.Image.Height / 2;
            }


            ViewPosition = new Vector2(x, y);
            ViewScale = scale;
            Invalidate();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public TextureArea SelectedTexture
        {
            get { return _selectedTexture; }
            set
            {
                if (!(VisibleTexture?.IsAvailable ?? false))
                    return;

                if (_selectedTexture == value)
                    return;
                _selectedTexture = value;
                SelectedTextureChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        public event EventHandler<EventArgs> SelectedTextureChanged;
    }
}
