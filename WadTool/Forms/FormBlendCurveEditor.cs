using System;
using System.Drawing;
using System.Windows.Forms;
using DarkUI.Config;
using TombLib.Types;

namespace WadTool
{
    public partial class FormBlendCurveEditor : DarkUI.Forms.DarkForm
    {
        public BezierCurve2 ResultCurve { get; private set; }

        public FormBlendCurveEditor(BezierCurve2 curve, Point screenPosition)
        {
            InitializeComponent();

            ResultCurve = curve.Clone();
            bezierCurveEditor.Value = ResultCurve;

            // Clamp position to screen bounds with 4px padding.
            var screen = Screen.FromPoint(screenPosition).WorkingArea;
            int x = Math.Min(screenPosition.X, screen.Right - Width);
            int y = Math.Min(screenPosition.Y, screen.Bottom - Height);
            x = Math.Max(x, screen.Left);
            y = Math.Max(y, screen.Top);
            Location = new Point(x, y);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            using (var pen = new Pen(Colors.GreySelection, 1))
                e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }

        private void cbBlendPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbBlendPreset.SelectedIndex)
            {
                case 0:
                    bezierCurveEditor.Value.Set(BezierCurve2.Linear);
                    break;

                case 1:
                    bezierCurveEditor.Value.Set(BezierCurve2.EaseIn);
                    break;

                case 2:
                    bezierCurveEditor.Value.Set(BezierCurve2.EaseOut);
                    break;

                case 3:
                    bezierCurveEditor.Value.Set(BezierCurve2.EaseInOut);
                    break;
            }

            bezierCurveEditor.UpdateUI();
        }

        private void bezierCurveEditor_ValueChanged(object sender, EventArgs e)
        {
            cbBlendPreset.SelectedIndex = -1;
        }
    }
}
