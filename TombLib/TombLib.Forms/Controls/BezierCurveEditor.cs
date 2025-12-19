using DarkUI.Config;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Types;

namespace WadTool.Controls
{
    public partial class BezierCurveEditor : UserControl
    {
        private const float HandleRadius = 6.0f;
        private const float HandleOutlineRadius = 9.0f;

        private Vector2[] _controlPoints = new Vector2[4];
        private int _selectedPoint = -1;

        public event EventHandler ValueChanged;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BezierCurve2 Value
        {
            get => _bezierCurve;
            set
            {
                _bezierCurve = value ?? BezierCurve2.Linear.Clone();

                if (!DesignMode)
                    UpdateUI();
            }
        }

        private BezierCurve2 _bezierCurve = BezierCurve2.Linear;

        public BezierCurveEditor()
        {
            if (DesignMode)
                return;

            DoubleBuffered = true;
            ResizeRedraw = true;
            InitializeComponent();
            InitializeControlPoints();
        }

        private void InitializeControlPoints()
        {
            if (DesignMode)
                return;

            _controlPoints[0] = new Vector2(0, Height);
            _controlPoints[1] = new Vector2(Width / 3.0f, Height * 2.0f / 3.0f);
            _controlPoints[2] = new Vector2(2 * Width / 3.0f, Height / 3.0f);
            _controlPoints[3] = new Vector2(Width, 0);
        }

        public void UpdateUI()
        {
            if (DesignMode || _bezierCurve == null)
                return;

            _controlPoints[0] = TransformToScreen(_bezierCurve.Start);
            _controlPoints[1] = TransformToScreen(_bezierCurve.StartHandle);
            _controlPoints[2] = TransformToScreen(_bezierCurve.EndHandle);
            _controlPoints[3] = TransformToScreen(_bezierCurve.End);

            AdjustHandlesForLinearCurve();
            Invalidate();
        }

        private void AdjustHandlesForLinearCurve()
        {
            if (_bezierCurve.StartHandle == _bezierCurve.Start)
            {

                _controlPoints[0] = new Vector2(0, Height);
                _controlPoints[1] = new Vector2(Width / 3.0f, Height * 2.0f / 3.0f);
            }

            if (_bezierCurve.EndHandle == _bezierCurve.End)
            {
                _controlPoints[2] = new Vector2(2 * Width / 3.0f, Height / 3.0f);
                _controlPoints[3] = new Vector2(Width, 0);
            }
        }

        private Vector2 TransformToBezier(Vector2 point)
        {
            return new Vector2(point.X / Width, 1.0f - (point.Y / Height));
        }

        private Vector2 TransformToScreen(Vector2 point)
        {
            return new Vector2(point.X * Width, (1.0f - point.Y) * Height);
        }

        private bool IsPointNear(Vector2 p1, Vector2 p2)
        {
            return Math.Abs(p1.X - p2.X) < HandleOutlineRadius * 2 && Math.Abs(p1.Y - p2.Y) < HandleOutlineRadius * 2;
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            if (DesignMode)
            {
                using (var borderPen = new Pen(Colors.LightBorder, 1))
                {
                    g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
                }

                g.DrawString("Rendering: Not Available in form designer!", Font, Brushes.DarkGray, ClientRectangle,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                return;
            }

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (var pen = new Pen(!Enabled ? Colors.DarkGreySelection : Colors.LightestBackground, 2))
            {
                g.DrawBezier(pen, new PointF(_controlPoints[0]), new PointF(_controlPoints[1]), 
                                  new PointF(_controlPoints[2]), new PointF(_controlPoints[3]));
            }

            using (var handlePen = new Pen(!Enabled ? Colors.DarkGreySelection : Colors.GreyHighlight, 1))
            {
                g.DrawLine(handlePen, new PointF(_controlPoints[0]), new PointF(_controlPoints[1]));
                g.DrawLine(handlePen, new PointF(_controlPoints[3]), new PointF(_controlPoints[2]));
            }

            for (int i = 1; i < 3; i++)
            {
                using (var brush = new SolidBrush(Colors.GreyBackground))
                {
                    g.FillEllipse(brush, _controlPoints[i].X - HandleOutlineRadius, _controlPoints[i].Y - HandleOutlineRadius,
                                  HandleOutlineRadius * 2, HandleOutlineRadius * 2);
                }

                using (var brush = new SolidBrush(!Enabled ? Colors.DarkGreySelection : (i == _selectedPoint ? Colors.LightestBackground : Colors.GreyHighlight)))
                {
                    g.FillEllipse(brush, _controlPoints[i].X - HandleRadius, _controlPoints[i].Y - HandleRadius,
                                  HandleRadius * 2, HandleRadius * 2);
                }
            }

            using (var borderPen = new Pen(Colors.GreyBackground, 3))
            {
                g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
            }

            using (var borderPen = new Pen(Colors.LightBorder, 1))
            {
                g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (DesignMode)
                return;

            for (int i = 1; i < 3; i++)
            {
                if (IsPointNear(new Vector2(e.Location.X, e.Location.Y), _controlPoints[i]))
                {
                    _selectedPoint = i;
                    break;
                }
            }

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (DesignMode || _selectedPoint == -1 || e.Button != MouseButtons.Left)
                return;

            _controlPoints[_selectedPoint] = new Vector2(
                Math.Max(HandleRadius / 2, Math.Min(e.X, Width - HandleRadius / 2 - 1)),
                Math.Max(HandleRadius / 2, Math.Min(e.Y, Height - HandleRadius / 2 - 1))
            );

            if (_bezierCurve != null)
            {
                if (_selectedPoint == 1)
                    _bezierCurve.StartHandle = TransformToBezier(_controlPoints[1]);
                else if (_selectedPoint == 2)
                    _bezierCurve.EndHandle = TransformToBezier(_controlPoints[2]);

                OnValueChanged(EventArgs.Empty);
            }

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (DesignMode)
                return;

            _selectedPoint = -1;
            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (DesignMode)
                return;

            if (_selectedPoint == -1)
            {
                Value.Set(BezierCurve2.Linear);
                InitializeControlPoints();
                ValueChanged?.Invoke(this, e);
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (DesignMode)
                return;

            UpdateUI();
        }
    }
}