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
			btOk = new DarkUI.Controls.DarkButton();
			btCancel = new DarkUI.Controls.DarkButton();
			SuspendLayout();
			// 
			// bezierCurveEditor
			// 
			bezierCurveEditor.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			bezierCurveEditor.Location = new System.Drawing.Point(8, 8);
			bezierCurveEditor.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
			bezierCurveEditor.Name = "bezierCurveEditor";
			bezierCurveEditor.Size = new System.Drawing.Size(248, 230);
			bezierCurveEditor.TabIndex = 0;
			bezierCurveEditor.ValueChanged += bezierCurveEditor_ValueChanged;
			// 
			// cbBlendPreset
			// 
			cbBlendPreset.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			cbBlendPreset.FormattingEnabled = true;
			cbBlendPreset.Items.AddRange(new object[] { "Linear", "Ease In", "Ease Out", "Ease In and Out" });
			cbBlendPreset.Location = new System.Drawing.Point(53, 247);
			cbBlendPreset.Name = "cbBlendPreset";
			cbBlendPreset.Size = new System.Drawing.Size(203, 23);
			cbBlendPreset.TabIndex = 1;
			cbBlendPreset.SelectedIndexChanged += cbBlendPreset_SelectedIndexChanged;
			// 
			// darkLabel1
			// 
			darkLabel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			darkLabel1.AutoSize = true;
			darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel1.Location = new System.Drawing.Point(8, 251);
			darkLabel1.Name = "darkLabel1";
			darkLabel1.Size = new System.Drawing.Size(41, 13);
			darkLabel1.TabIndex = 2;
			darkLabel1.Text = "Preset:";
			// 
			// btOk
			// 
			btOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			btOk.Checked = false;
			btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			btOk.Location = new System.Drawing.Point(101, 279);
			btOk.Name = "btOk";
			btOk.Size = new System.Drawing.Size(75, 23);
			btOk.TabIndex = 3;
			btOk.Text = "OK";
			// 
			// btCancel
			// 
			btCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			btCancel.Checked = false;
			btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			btCancel.Location = new System.Drawing.Point(182, 279);
			btCancel.Name = "btCancel";
			btCancel.Size = new System.Drawing.Size(75, 23);
			btCancel.TabIndex = 4;
			btCancel.Text = "Cancel";
			// 
			// FormBlendCurveEditor
			// 
			AcceptButton = btOk;
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			CancelButton = btCancel;
			ClientSize = new System.Drawing.Size(264, 310);
			Controls.Add(bezierCurveEditor);
			Controls.Add(darkLabel1);
			Controls.Add(cbBlendPreset);
			Controls.Add(btOk);
			Controls.Add(btCancel);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "FormBlendCurveEditor";
			ShowIcon = false;
			ShowInTaskbar = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Blend curve";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Controls.BezierCurveEditor bezierCurveEditor;
        private DarkUI.Controls.DarkComboBox cbBlendPreset;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkButton btOk;
        private DarkUI.Controls.DarkButton btCancel;
    }
}
