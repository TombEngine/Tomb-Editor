using System;
using TombLib.Types;

namespace WadTool
{
    public partial class FormBlendCurveEditor : DarkUI.Forms.DarkForm
    {
        public BezierCurve2 ResultCurve { get; private set; }

        public FormBlendCurveEditor(BezierCurve2 curve)
        {
            InitializeComponent();

            ResultCurve = curve.Clone();
            bezierCurveEditor.Value = ResultCurve;
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
