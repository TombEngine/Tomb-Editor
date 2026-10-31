#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TombLib.Graphics;
using TombLib.WPF.Input;
using TombLib.Wad;

namespace TombLib.WPF.CustomControls;

/// <summary>
/// WPF rewrite of the shared <c>AnimationTrackBar</c>: the animation editor's timeline. Renders
/// frame/keyframe ticks, frame number labels, the cursor, the (middle- or shift-drag) selection,
/// the fading highlight, state change ranges and frame-based anim command markers; same public API
/// (Minimum/Maximum/Value/Selection*, Highlight, Animation, ValueChanged/SelectionChanged/
/// MinMaxChanged/AnimCommandDoubleClick) as the legacy control.
/// </summary>
public class AnimationTrackBar : FrameworkElement
{
    // Timeline palette colors.
    private static readonly Color _greyBackground = Color.FromRgb(60, 63, 65);
    private static readonly Color _lightestBackground = Color.FromRgb(178, 178, 178);
    private static readonly Color _lightText = Color.FromRgb(220, 220, 220);
    private static readonly Color _disabledText = Color.FromRgb(153, 153, 153);
    private static readonly Color _blueHighlight = Color.FromRgb(104, 151, 187);
    private static readonly Color _greyHighlight = Color.FromRgb(122, 128, 132);
    private static readonly Color _highlightBase = Color.FromRgb(104, 151, 187);

    private static Color Multiply(Color color, float factor) => Color.FromArgb(color.A,
        (byte)Math.Min(color.R * factor, 255.0f), (byte)Math.Min(color.G * factor, 255.0f), (byte)Math.Min(color.B * factor, 255.0f));
    private static Color MultiplyAlpha(Color color, float factor) => Color.FromArgb((byte)Math.Min(color.A * factor, 255.0f), color.R, color.G, color.B);

    private static SolidColorBrush FrozenBrush(Color color) { var brush = new SolidColorBrush(color); brush.Freeze(); return brush; }
    private static Pen FrozenPen(Color color, double thickness) { var pen = new Pen(FrozenBrush(color), thickness); pen.Freeze(); return pen; }

    private static readonly Brush _backgroundBrush = FrozenBrush(_greyBackground);
    private static readonly Pen _frameBorderPen = FrozenPen(Multiply(_lightestBackground, 0.7f), 1);
    private static readonly Pen _keyFrameBorderPen = FrozenPen(_lightestBackground, 2);
    private static readonly Pen _selectionPen = FrozenPen(MultiplyAlpha(_highlightBase, 0.8f), 1);
    private static readonly Brush _selectionBrush = FrozenBrush(MultiplyAlpha(_highlightBase, 0.4f));
    private static readonly Color _highlightColor = MultiplyAlpha(_greyHighlight, 0.7f);
    private static readonly Brush _cursorBrush = FrozenBrush(MultiplyAlpha(_lightestBackground, 0.6f));
    private static readonly Brush _stateChangeBrush = FrozenBrush(Color.FromArgb(30, 220, 160, 180));
    private static readonly Color _animCommandSoundColor = Color.FromArgb(220, 100, 170, 255);
    private static readonly Color _animCommandFlipeffectColor = Color.FromArgb(220, 230, 110, 110);

    private static readonly Brush _lblEndFrameBrush = FrozenBrush(_blueHighlight);
    private static readonly Brush _lblKeyframeBrush = FrozenBrush(_lightText);
    private static readonly Brush _lblFrameBrush = FrozenBrush(MultiplyAlpha(_lightText, 0.3f));
    private static readonly Brush _disabledTextBrush = FrozenBrush(_disabledText);

    private static readonly Typeface _typeface = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
    private const double _fontSize = 9.0; // Legacy control font was 6.75pt = 9px.

    private const int _paddingLeft = 4, _paddingTop = 4, _paddingBottom = 4;
    private const int _paddingHorizontal = 8;
    private const int _cursorWidth = 6;
    private const int _animCommandMarkerRadius = 14;
    private const int _stateChangeMarkerThicknessDivider = 2;

    private const int _highlightTimerInterval = 30;
    private const float _highlightTime = 1000;
    private const float _highlightStep = _highlightTimerInterval / _highlightTime;

    private readonly DispatcherTimer _highlightTimer = new() { Interval = TimeSpan.FromMilliseconds(_highlightTimerInterval) };
    private float _highlightCounter = 0;
    private int _highlightStart;
    private int _highlightEnd;

    public event EventHandler<WadAnimCommand>? AnimCommandDoubleClick; // Happens when user double-clicks on frame with frame-based animcommands
    public event EventHandler? ValueChanged;
    public event EventHandler? SelectionChanged;
    public event EventHandler? MinMaxChanged;

    public AnimationNode? Animation { get; set; }

    private bool _mouseDown = false;

    public AnimationTrackBar()
    {
        // Some legacy rectangles intentionally overflow the bottom edge and relied on
        // GDI clipping to the control bounds.
        ClipToBounds = true;

        _highlightTimer.Tick += HighlightTimer_Tick;
    }

    /// <summary>Mirrors the previous Invalidate() the view model calls after external changes.</summary>
    public void Invalidate() => InvalidateVisual();

    private void HighlightTimer_Tick(object? sender, EventArgs e)
    {
        _highlightCounter -= _highlightStep;

        if (_highlightCounter <= 0.0f)
        {
            _highlightCounter = 0.0f;
            _highlightTimer.Stop();
        }

        InvalidateVisual();
    }

    private int realFrameCount => Animation!.WadAnimation.FrameRate * (Animation.DirectXAnimation.KeyFrames.Count - 1) + 1;
    private double marginWidth => ActualWidth - _paddingHorizontal - 1;
    private double frameStep => realFrameCount <= 1 ? marginWidth : marginWidth / (realFrameCount - 1);

    private int XtoMinMax(double x, int max, bool interpolate = true) => (int)Math.Round((Minimum + (double)(max - Minimum) * x) / (ActualWidth - _paddingHorizontal), interpolate ? MidpointRounding.ToEven : MidpointRounding.AwayFromZero);
    private int XtoRealFrameNumber(double x) => Math.Max(XtoMinMax(x, realFrameCount - 1, false), 0);
    private int XtoValue(double x) => XtoMinMax(x, Maximum, false);
    private double ValueToX(int value) => Maximum - Minimum == 0 ? 0 : Math.Round((ActualWidth - _paddingHorizontal) * (value - Minimum) / (Maximum - Minimum), MidpointRounding.ToEven);

    private int _minimum;
    public int Minimum
    {
        get { return _minimum; }
        set
        {
            if (_minimum == value)
            {
                InvalidateVisual(); // Invalidate anyway in case other values have changed
                return;
            }

            if (value >= _maximum || value < 0) return;

            _minimum = value;
            if (_minimum > Value) Value = _minimum;

            MinMaxChanged?.Invoke(this, EventArgs.Empty);
            InvalidateVisual();
        }
    }

    private int _maximum;
    public int Maximum
    {
        get { return _maximum; }
        set
        {
            if (_maximum == value)
            {
                InvalidateVisual(); // Invalidate anyway in case other values have changed
                return;
            }

            if (value < _minimum || value < 0) return;

            _maximum = value;
            if (_maximum < Value) Value = _maximum;

            MinMaxChanged?.Invoke(this, EventArgs.Empty);
            InvalidateVisual();
        }
    }

    private int _selectionStart;
    private int _selectionEnd;
    public int SelectionStart
    {
        get { return _selectionStart; }
        set
        {
            if (value == _selectionStart || value < _minimum || value > _maximum)
            {
                InvalidateVisual(); // Invalidate anyway in case other values have changed
                return;
            }

            _selectionStart = value;
            if (_selectionEnd == -1) _selectionEnd = value;
            else if (_selectionStart > _selectionEnd) _selectionEnd = _selectionStart;

            SelectionChanged?.Invoke(this, EventArgs.Empty);
            InvalidateVisual();
        }
    }
    public int SelectionEnd
    {
        get { return _selectionEnd; }
        set
        {
            if (value == _selectionEnd || value < _minimum || value > _maximum)
            {
                InvalidateVisual(); // Invalidate anyway in case other values have changed
                return;
            }

            _selectionEnd = value;
            if (_selectionStart == -1) _selectionStart = value;
            else if (_selectionStart > _selectionEnd) _selectionStart = _selectionEnd;

            SelectionChanged?.Invoke(this, EventArgs.Empty);
            InvalidateVisual();
        }
    }
    public int SelectionStartFrameIndex => Animation!.WadAnimation.FrameRate * SelectionStart;
    public int SelectionEndFrameIndex => Animation!.WadAnimation.FrameRate * SelectionEnd;

    public VectorInt2 Selection => SelectionIsEmpty ? new VectorInt2(-1, -1) : new VectorInt2(Math.Min(SelectionStart, SelectionEnd), Math.Max(SelectionStart, SelectionEnd));
    public bool SelectionIsEmpty => SelectionEnd == -1 && SelectionStart == -1;
    public int SelectionSize => SelectionIsEmpty ? 0 : Selection.Y - Selection.X + 1;

    public void ResetSelection()
    {
        _selectionStart = _selectionEnd = -1;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        InvalidateVisual();
    }

    public void SelectAll()
    {
        _selectionStart = _minimum;
        _selectionEnd = _maximum;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        InvalidateVisual();
    }

    private int _value;
    public int Value
    {
        get { return _value; }
        set
        {
            if (value == _value)
            {
                InvalidateVisual(); // Invalidate anyway in case other values have changed
                return;
            }
            if (value < _minimum) value = _minimum;
            if (value > _maximum) value = _maximum;

            _value = value;

            ValueChanged?.Invoke(this, EventArgs.Empty);
            InvalidateVisual();
        }
    }
    public int FrameIndex => Animation!.WadAnimation.FrameRate * Value;

    public void ValueLoopInc() { if (Value < Maximum) Value++; else Value = 0; }
    public void ValueLoopDec() { if (Value > Minimum) Value--; else Value = Maximum; }

    public void Highlight(int start, int end)
    {
        if (start > end || start < _minimum || end > _maximum) return;
        _highlightStart = start;
        _highlightEnd = end;
        _highlightCounter = 1.0f;
        _highlightTimer.Start();
    }
    public void Highlight() => Highlight(Minimum, Maximum);

    public void SelectCurrentValue()
    {
        _selectionStart = Value;
        _selectionEnd = Value;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool IsSelectingRange(MouseEventArgs e) =>
        e.MiddleButton == MouseButtonState.Pressed ||
        (e.LeftButton == MouseButtonState.Pressed && Keyboard.Modifiers == ModifierKeys.Shift);

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
        {
            OnDoubleClickAt(e.GetPosition(this).X);
            e.Handled = true;
            return;
        }

        if (e.ChangedButton != MouseButton.Left && e.ChangedButton != MouseButton.Middle)
            return;

        _mouseDown = true;
        CaptureMouse();

        double x = e.GetPosition(this).X;

        if (e.ChangedButton == MouseButton.Middle || (e.ChangedButton == MouseButton.Left && Keyboard.Modifiers == ModifierKeys.Shift))
        {
            SelectionStart = XtoValue(x);
            SelectionEnd = SelectionStart;
        }
        else if (e.ChangedButton == MouseButton.Left)
            Value = XtoValue(x);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        _mouseDown = false;
        ReleaseMouseCapture();

        // Fix selection dimensions after possible reverse dragging
        if (_selectionStart > _selectionEnd)
            (_selectionStart, _selectionEnd) = (_selectionEnd, _selectionStart);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_mouseDown) return;

        double x = e.GetPosition(this).X;

        if (IsSelectingRange(e))
        {
            if (SelectionIsEmpty)
                SelectionStart = XtoValue(x);

            // Manually update selection end to allow "reverse dragging"
            int potentialNewSelection = XtoValue(x);
            if (!SelectionIsEmpty && potentialNewSelection != _selectionEnd && potentialNewSelection >= _minimum && potentialNewSelection <= _maximum)
            {
                _selectionEnd = potentialNewSelection;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }

            InvalidateVisual();
        }
        else if (e.LeftButton == MouseButtonState.Pressed)
        {
            // Warp cursor at the edges, like the legacy control
            var cursorPosition = PointerState.ScreenPosition;
            if (x <= 0)
                PointerState.ScreenPosition = new System.Drawing.Point(cursorPosition.X + (int)ActualWidth - 2, cursorPosition.Y);
            else if (x >= ActualWidth - 1)
                PointerState.ScreenPosition = new System.Drawing.Point(cursorPosition.X - (int)ActualWidth + 2, cursorPosition.Y);

            Value = XtoValue(x);
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (e.Delta < 0) ValueLoopInc();
        else if (e.Delta > 0) ValueLoopDec();

        e.Handled = true;
    }

    private void OnDoubleClickAt(double x)
    {
        if (Animation == null || Animation.DirectXAnimation.KeyFrames.Count == 0)
            return;

        int targetFrame = XtoRealFrameNumber(x);
        if (targetFrame < 0) targetFrame = 0;

        // Try to find animcommand under cursor
        foreach (WadAnimCommand ac in Animation.WadAnimation.AnimCommands)
            if (ac.FrameBased && ac.Parameter1 == targetFrame)
            {
                AnimCommandDoubleClick?.Invoke(this, ac);
                return;
            }

        // No animcommand found, try to create new one
        var newCommand = new WadAnimCommand() { Type = WadAnimCommandType.PlaySound, Parameter1 = (short)targetFrame };
        AnimCommandDoubleClick?.Invoke(this, newCommand);
    }

    private FormattedText MakeText(string text, Brush brush) =>
        new(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, _typeface, _fontSize, brush,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

    /// <summary>Counterpart of GDI+ FillPie: a filled circle sector of <paramref name="rect"/>'s inscribed ellipse.</summary>
    private static void DrawPie(DrawingContext dc, Brush brush, Rect rect, double startAngle, double sweepAngle)
    {
        var center = new Point(rect.X + rect.Width / 2.0, rect.Y + rect.Height / 2.0);
        double radiusX = rect.Width / 2.0, radiusY = rect.Height / 2.0;

        Point PointAt(double angleDeg)
        {
            double rad = angleDeg * Math.PI / 180.0;
            return new Point(center.X + radiusX * Math.Cos(rad), center.Y + radiusY * Math.Sin(rad));
        }

        var geometry = new StreamGeometry();
        using (StreamGeometryContext context = geometry.Open())
        {
            context.BeginFigure(center, isFilled: true, isClosed: true);
            context.LineTo(PointAt(startAngle), isStroked: false, isSmoothJoin: false);
            context.ArcTo(PointAt(startAngle + sweepAngle), new Size(radiusX, radiusY), 0.0,
                isLargeArc: sweepAngle > 180.0, SweepDirection.Clockwise, isStroked: false, isSmoothJoin: false);
        }
        geometry.Freeze();
        dc.DrawGeometry(brush, null, geometry);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        double width = ActualWidth, height = ActualHeight;
        if (width <= 0 || height <= 0)
            return;

        dc.DrawRectangle(_backgroundBrush, null, new Rect(0, 0, width, height));

        // Any messages in case of any errors
        string? errorMessage = null;
        if (Animation == null) errorMessage = "No animation! Create or select animation to start editing.";
        else if (Animation.DirectXAnimation.KeyFrames.Count == 0) errorMessage = "No frames! Add some frames to start editing.";

        if (!string.IsNullOrEmpty(errorMessage))
        {
            FormattedText text = MakeText(errorMessage, _disabledTextBrush);
            dc.DrawText(text, new Point((width - text.Width) / 2.0, (height - text.Height) / 2.0));
            return;
        }

        // Draw state change ranges
        foreach (var sch in Animation!.WadAnimation.StateChanges)
            foreach (var disp in sch.Dispatches)
            {
                int realOutFrame = disp.OutFrame >= realFrameCount ? realFrameCount - 1 : disp.OutFrame;
                dc.DrawRectangle(_stateChangeBrush, null, new Rect(_paddingLeft + disp.InFrame * frameStep,
                    _paddingTop,
                    Math.Max((realOutFrame - disp.InFrame) * frameStep, 0.0),
                    Math.Max(height / _stateChangeMarkerThicknessDivider - _paddingBottom - 2, 0.0)));
            }

        int halfCursorWidth = (int)Math.Round(_cursorWidth / 2.0f);

        // Shift the cursor at start/stop positions to prevent clipping
        int addShift = -halfCursorWidth;
        if (Value == 0) addShift += halfCursorWidth;
        else if (Value == Maximum) addShift += -halfCursorWidth;

        // Draw selection and highlight in 2 passes
        for (int passes = 0; passes < 2; passes++)
        {
            int x = passes == 0 ? Selection.X : _highlightStart;
            int y = passes == 0 ? Selection.Y : _highlightEnd;
            double realX = ValueToX(x);
            double realY = ValueToX(y);
            double size = realY - realX;

            if ((passes == 0 && !SelectionIsEmpty) || (passes == 1 && _highlightTimer.IsEnabled))
            {
                double rectWidth = size == 0 ? _cursorWidth : size + _cursorWidth;
                var rect = new Rect(realX + _paddingLeft - halfCursorWidth, _paddingTop, rectWidth, Math.Max(height - _paddingBottom, 0.0));

                if (size == 0)
                {
                    if (y != Minimum && y == Maximum) rect.X -= halfCursorWidth;
                    else if (x == Minimum) rect.X += halfCursorWidth;
                }
                else
                {
                    if (x == Minimum)
                    {
                        rect.X += halfCursorWidth;
                        rect.Width -= halfCursorWidth;
                    }
                    if (y == Maximum)
                    {
                        rect.Width -= halfCursorWidth;
                    }
                }

                if (passes == 0)
                {
                    dc.DrawRectangle(_selectionBrush, null, rect);
                    dc.DrawRectangle(null, _selectionPen, rect);
                }
                else
                {
                    var highlightBrush = new SolidColorBrush(Color.FromArgb((byte)(_highlightColor.A * _highlightCounter),
                        _highlightColor.R, _highlightColor.G, _highlightColor.B));
                    highlightBrush.Freeze();
                    dc.DrawRectangle(highlightBrush, null, rect);
                }
            }
        }

        // Measure maximum label size
        FormattedText maxLabel = MakeText(realFrameCount.ToString(), _lblKeyframeBrush);
        double maxLabelWidth = maxLabel.Width;

        // Precache animcommands so we don't iterate them every drawn frame
        var acList = new List<KeyValuePair<int, WadAnimCommand>>();
        foreach (var ac in Animation.WadAnimation.AnimCommands.Where(ac => ac.FrameBased))
            acList.Add(new KeyValuePair<int, WadAnimCommand>(ac.Parameter1, ac));

        // Precache some variables for speeding up renderer with ultra-long animations (5000+ frames)
        double step = frameStep;
        double drawStepWidth = _keyFrameBorderPen.Thickness * 4;

        // Draw frame-specific animcommands, numericals and dividers
        for (int passes = 0; passes < 2; passes++)
            for (int i = 0; i < realFrameCount; ++i)
            {
                double currX = MathC.Round(step * i) + _paddingLeft;
                bool isKeyFrame = (i % (Animation.WadAnimation.FrameRate == 0 ? 1 : Animation.WadAnimation.FrameRate) == 0);
                bool first = i == 0;
                bool last = i >= realFrameCount - 1;

                if (passes == 0)
                {
                    int count = 0;

                    // Draw animcommands
                    if (acList.Count > 0)
                    {
                        foreach (var acPair in acList)
                        {
                            WadAnimCommand ac = acPair.Value;
                            var currRect = new Rect(currX - _animCommandMarkerRadius / 2.0, _paddingTop - _animCommandMarkerRadius / 2.0 + (_animCommandMarkerRadius / 3.0 * count), _animCommandMarkerRadius, _animCommandMarkerRadius);
                            double startAngle = !first ? (!last ? 0 : 90) : 0;
                            double sweepAngle = !first ? (!last ? 180 : 90) : 90;

                            if (ac.Parameter1 == i)
                            {
                                Color baseColor = ac.Type == WadAnimCommandType.PlaySound ? _animCommandSoundColor : _animCommandFlipeffectColor;
                                var currBrush = new SolidColorBrush(Color.FromArgb((byte)(baseColor.A / (1.0f + (count / 3.0f))), baseColor.R, baseColor.G, baseColor.B));
                                currBrush.Freeze();
                                DrawPie(dc, currBrush, currRect, startAngle, sweepAngle);
                                count++;
                            }
                        }
                        acList.RemoveAll(ac => ac.Key == i); // Remove already drawn animcommands from list
                    }

                    // Determine if current line should be drawn.
                    bool drawCurrentLine = true;
                    if (step < drawStepWidth)
                    {
                        int period = (int)MathC.Round(drawStepWidth / step);
                        if (i % period != 0 && i != realFrameCount - 1) drawCurrentLine = false;
                    }

                    if (drawCurrentLine)
                    {
                        // Draw frame lines
                        double lineHeight = height / (isKeyFrame ? 2 : 3);
                        dc.DrawLine(isKeyFrame ? _keyFrameBorderPen : _frameBorderPen, new Point(currX, _paddingTop), new Point(currX, lineHeight));
                    }
                }

                // Draw cursor on 2nd pass's first occurence (only for real animations, not for single-frame ones)
                if (i == 0 && passes == 1 && realFrameCount > 1)
                    dc.DrawRectangle(_cursorBrush, null, new Rect(ValueToX(Value) + addShift + _paddingLeft, _paddingTop, _cursorWidth, Math.Max(height - _paddingBottom - 2, 0.0)));

                // Draw labels
                bool drawCurrentLabel = true;
                if ((passes == 0 && !isKeyFrame) || (passes != 0 && isKeyFrame))
                {
                    // Determine if labels are overlapping and decide on drawing
                    if (step < maxLabelWidth * 1.25)
                    {
                        int period = (int)MathC.Round(maxLabelWidth * 1.25 / step);
                        if (i % period != 0) drawCurrentLabel = false;
                    }

                    if (drawCurrentLabel || i == Animation.WadAnimation.EndFrame)
                    {
                        Brush brush = (i == Animation.WadAnimation.EndFrame) ? _lblEndFrameBrush : (isKeyFrame ? _lblKeyframeBrush : _lblFrameBrush);
                        FormattedText label = MakeText(i.ToString(), brush);

                        // Align first and last numerical entries so they are not concealed by control border
                        double labelX;
                        if (first)
                            labelX = currX - _paddingLeft;
                        else if (last)
                            labelX = currX + _paddingLeft - label.Width;
                        else
                            labelX = currX - label.Width / 2.0;

                        // Finally draw it after all these tests
                        dc.DrawText(label, new Point(labelX, height - label.Height));
                    }
                }
            }

        // Draw horizontal guide (only for real anims, for single-frame anims we wouldn't wanna show that)
        if (realFrameCount > 1)
            dc.DrawLine(_keyFrameBorderPen, new Point(_paddingLeft, _paddingTop + 1), new Point(width - _paddingLeft, _paddingTop + 1));
    }
}
