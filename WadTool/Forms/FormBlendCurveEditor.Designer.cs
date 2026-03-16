namespace WadTool
{
    partial class FormBlendCurveEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            bezierCurveEditor = new Controls.BezierCurveEditor();
            cbBlendPreset = new DarkUI.Controls.DarkComboBox();
            darkLabel1 = new DarkUI.Controls.DarkLabel();
            SuspendLayout();
            //
            // bezierCurveEditor
            //
            bezierCurveEditor.Location = new System.Drawing.Point(4, 4);
            bezierCurveEditor.Name = "bezierCurveEditor";
            bezierCurveEditor.Size = new System.Drawing.Size(248, 230);
            bezierCurveEditor.TabIndex = 0;
            bezierCurveEditor.ValueChanged += bezierCurveEditor_ValueChanged;
            //
            // cbBlendPreset
            //
            cbBlendPreset.FormattingEnabled = true;
            cbBlendPreset.Items.AddRange(new object[] { "Linear", "Ease In", "Ease Out", "Ease In and Out" });
            cbBlendPreset.Location = new System.Drawing.Point(49, 240);
            cbBlendPreset.Name = "cbBlendPreset";
            cbBlendPreset.Size = new System.Drawing.Size(203, 23);
            cbBlendPreset.TabIndex = 1;
            cbBlendPreset.SelectedIndexChanged += cbBlendPreset_SelectedIndexChanged;
            //
            // darkLabel1
            //
            darkLabel1.AutoSize = true;
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel1.Location = new System.Drawing.Point(4, 244);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(41, 13);
            darkLabel1.TabIndex = 2;
            darkLabel1.Text = "Preset:";
            //
            // FormBlendCurveEditor
            //
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(256, 269);
            ControlBox = false;
            Controls.Add(bezierCurveEditor);
            Controls.Add(darkLabel1);
            Controls.Add(cbBlendPreset);
            FlatBorder = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormBlendCurveEditor";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Controls.BezierCurveEditor bezierCurveEditor;
        private DarkUI.Controls.DarkComboBox cbBlendPreset;
        private DarkUI.Controls.DarkLabel darkLabel1;
    }
}
