namespace WadTool
{
    partial class AnimCommandEditor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			darkLabel8 = new DarkUI.Controls.DarkLabel();
			comboCommandType = new DarkUI.Controls.DarkComboBox();
			commandControls = new TombLib.Controls.DarkTabbedContainer();
			tabSetPosition = new System.Windows.Forms.TabPage();
			darkLabel3 = new DarkUI.Controls.DarkLabel();
			tbPosX = new DarkUI.Controls.DarkNumericUpDown();
			tbPosZ = new DarkUI.Controls.DarkNumericUpDown();
			darkLabel1 = new DarkUI.Controls.DarkLabel();
			darkLabel2 = new DarkUI.Controls.DarkLabel();
			tbPosY = new DarkUI.Controls.DarkNumericUpDown();
			tabSetJumpVelocity = new System.Windows.Forms.TabPage();
			darkLabel5 = new DarkUI.Controls.DarkLabel();
			darkLabel6 = new DarkUI.Controls.DarkLabel();
			tbVertical = new DarkUI.Controls.DarkNumericUpDown();
			tbHorizontal = new DarkUI.Controls.DarkNumericUpDown();
			tabFlipeffect = new System.Windows.Forms.TabPage();
			lblLaraFoot = new DarkUI.Controls.DarkLabel();
			comboFlipeffectConditions = new DarkUI.Controls.DarkComboBox();
			darkLabel4 = new DarkUI.Controls.DarkLabel();
			darkLabel7 = new DarkUI.Controls.DarkLabel();
			tbFlipEffectFrame = new DarkUI.Controls.DarkNumericUpDown();
			tbFlipEffect = new DarkUI.Controls.DarkNumericUpDown();
			tabPlaySound = new System.Windows.Forms.TabPage();
			nudSoundId = new DarkUI.Controls.DarkNumericUpDown();
			butPlaySound = new DarkUI.Controls.DarkButton();
			darkLabel11 = new DarkUI.Controls.DarkLabel();
			darkLabel10 = new DarkUI.Controls.DarkLabel();
			comboSound = new TombLib.Controls.DarkSearchableComboBox();
			tbPlaySoundFrame = new DarkUI.Controls.DarkNumericUpDown();
			darkLabel9 = new DarkUI.Controls.DarkLabel();
			comboPlaySoundConditions = new DarkUI.Controls.DarkComboBox();
			tabDisableInterpolation = new System.Windows.Forms.TabPage();
			darkLabel12 = new DarkUI.Controls.DarkLabel();
			tbFrameDisableInterpolation = new DarkUI.Controls.DarkNumericUpDown();
			tabDetachFromParent = new System.Windows.Forms.TabPage();
			darkLabel13 = new DarkUI.Controls.DarkLabel();
			tbFrameDetachFromParent = new DarkUI.Controls.DarkNumericUpDown();
			panelView = new System.Windows.Forms.Panel();
			commandControls.SuspendLayout();
			tabSetPosition.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)tbPosX).BeginInit();
			((System.ComponentModel.ISupportInitialize)tbPosZ).BeginInit();
			((System.ComponentModel.ISupportInitialize)tbPosY).BeginInit();
			tabSetJumpVelocity.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)tbVertical).BeginInit();
			((System.ComponentModel.ISupportInitialize)tbHorizontal).BeginInit();
			tabFlipeffect.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)tbFlipEffectFrame).BeginInit();
			((System.ComponentModel.ISupportInitialize)tbFlipEffect).BeginInit();
			tabPlaySound.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)nudSoundId).BeginInit();
			((System.ComponentModel.ISupportInitialize)tbPlaySoundFrame).BeginInit();
			tabDisableInterpolation.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)tbFrameDisableInterpolation).BeginInit();
			tabDetachFromParent.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)tbFrameDetachFromParent).BeginInit();
			panelView.SuspendLayout();
			SuspendLayout();
			// 
			// darkLabel8
			// 
			darkLabel8.AutoSize = true;
			darkLabel8.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel8.Location = new System.Drawing.Point(4, 6);
			darkLabel8.Name = "darkLabel8";
			darkLabel8.Size = new System.Drawing.Size(33, 13);
			darkLabel8.TabIndex = 1;
			darkLabel8.Text = "Type:";
			// 
			// comboCommandType
			// 
			comboCommandType.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			comboCommandType.Enabled = false;
			comboCommandType.FormattingEnabled = true;
			comboCommandType.Location = new System.Drawing.Point(42, 3);
			comboCommandType.Name = "comboCommandType";
			comboCommandType.Size = new System.Drawing.Size(328, 23);
			comboCommandType.TabIndex = 53;
			comboCommandType.SelectedIndexChanged += comboCommandType_SelectedIndexChanged;
			// 
			// commandControls
			// 
			commandControls.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			commandControls.Controls.Add(tabSetPosition);
			commandControls.Controls.Add(tabSetJumpVelocity);
			commandControls.Controls.Add(tabFlipeffect);
			commandControls.Controls.Add(tabPlaySound);
			commandControls.Controls.Add(tabDisableInterpolation);
			commandControls.Controls.Add(tabDetachFromParent);
			commandControls.Location = new System.Drawing.Point(0, 29);
			commandControls.Name = "commandControls";
			commandControls.SelectedIndex = 0;
			commandControls.Size = new System.Drawing.Size(370, 128);
			commandControls.TabIndex = 0;
			commandControls.Visible = false;
			// 
			// tabSetPosition
			// 
			tabSetPosition.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabSetPosition.Controls.Add(darkLabel3);
			tabSetPosition.Controls.Add(tbPosX);
			tabSetPosition.Controls.Add(tbPosZ);
			tabSetPosition.Controls.Add(darkLabel1);
			tabSetPosition.Controls.Add(darkLabel2);
			tabSetPosition.Controls.Add(tbPosY);
			tabSetPosition.Location = new System.Drawing.Point(4, 22);
			tabSetPosition.Name = "tabSetPosition";
			tabSetPosition.Padding = new System.Windows.Forms.Padding(3);
			tabSetPosition.Size = new System.Drawing.Size(362, 102);
			tabSetPosition.TabIndex = 0;
			tabSetPosition.Text = "setPosition";
			// 
			// darkLabel3
			// 
			darkLabel3.AutoSize = true;
			darkLabel3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel3.Location = new System.Drawing.Point(164, 8);
			darkLabel3.Name = "darkLabel3";
			darkLabel3.Size = new System.Drawing.Size(16, 13);
			darkLabel3.TabIndex = 5;
			darkLabel3.Text = "Z:";
			// 
			// tbPosX
			// 
			tbPosX.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			tbPosX.Location = new System.Drawing.Point(9, 24);
			tbPosX.LoopValues = false;
			tbPosX.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
			tbPosX.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
			tbPosX.Name = "tbPosX";
			tbPosX.Size = new System.Drawing.Size(73, 22);
			tbPosX.TabIndex = 0;
			tbPosX.ValueChanged += tbPosX_ValueChanged;
			tbPosX.Click += tbPosX_ValueChanged;
			// 
			// tbPosZ
			// 
			tbPosZ.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			tbPosZ.Location = new System.Drawing.Point(167, 24);
			tbPosZ.LoopValues = false;
			tbPosZ.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
			tbPosZ.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
			tbPosZ.Name = "tbPosZ";
			tbPosZ.Size = new System.Drawing.Size(73, 22);
			tbPosZ.TabIndex = 4;
			tbPosZ.ValueChanged += tbPosZ_ValueChanged;
			tbPosZ.Click += tbPosZ_ValueChanged;
			// 
			// darkLabel1
			// 
			darkLabel1.AutoSize = true;
			darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel1.Location = new System.Drawing.Point(6, 8);
			darkLabel1.Name = "darkLabel1";
			darkLabel1.Size = new System.Drawing.Size(16, 13);
			darkLabel1.TabIndex = 1;
			darkLabel1.Text = "X:";
			// 
			// darkLabel2
			// 
			darkLabel2.AutoSize = true;
			darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel2.Location = new System.Drawing.Point(85, 8);
			darkLabel2.Name = "darkLabel2";
			darkLabel2.Size = new System.Drawing.Size(15, 13);
			darkLabel2.TabIndex = 3;
			darkLabel2.Text = "Y:";
			// 
			// tbPosY
			// 
			tbPosY.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			tbPosY.Location = new System.Drawing.Point(88, 24);
			tbPosY.LoopValues = false;
			tbPosY.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
			tbPosY.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
			tbPosY.Name = "tbPosY";
			tbPosY.Size = new System.Drawing.Size(73, 22);
			tbPosY.TabIndex = 2;
			tbPosY.ValueChanged += tbPosY_ValueChanged;
			tbPosY.Click += tbPosY_ValueChanged;
			// 
			// tabSetJumpVelocity
			// 
			tabSetJumpVelocity.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabSetJumpVelocity.Controls.Add(darkLabel5);
			tabSetJumpVelocity.Controls.Add(darkLabel6);
			tabSetJumpVelocity.Controls.Add(tbVertical);
			tabSetJumpVelocity.Controls.Add(tbHorizontal);
			tabSetJumpVelocity.Location = new System.Drawing.Point(4, 24);
			tabSetJumpVelocity.Name = "tabSetJumpVelocity";
			tabSetJumpVelocity.Padding = new System.Windows.Forms.Padding(3);
			tabSetJumpVelocity.Size = new System.Drawing.Size(362, 100);
			tabSetJumpVelocity.TabIndex = 1;
			tabSetJumpVelocity.Text = "SetJumpVelocity";
			// 
			// darkLabel5
			// 
			darkLabel5.AutoSize = true;
			darkLabel5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel5.Location = new System.Drawing.Point(85, 8);
			darkLabel5.Name = "darkLabel5";
			darkLabel5.Size = new System.Drawing.Size(47, 13);
			darkLabel5.TabIndex = 3;
			darkLabel5.Text = "Vertical:";
			// 
			// darkLabel6
			// 
			darkLabel6.AutoSize = true;
			darkLabel6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel6.Location = new System.Drawing.Point(6, 8);
			darkLabel6.Name = "darkLabel6";
			darkLabel6.Size = new System.Drawing.Size(64, 13);
			darkLabel6.TabIndex = 1;
			darkLabel6.Text = "Horizontal:";
			// 
			// tbVertical
			// 
			tbVertical.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			tbVertical.Location = new System.Drawing.Point(88, 24);
			tbVertical.LoopValues = false;
			tbVertical.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
			tbVertical.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
			tbVertical.Name = "tbVertical";
			tbVertical.Size = new System.Drawing.Size(73, 22);
			tbVertical.TabIndex = 2;
			tbVertical.ValueChanged += tbVertical_ValueChanged;
			// 
			// tbHorizontal
			// 
			tbHorizontal.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			tbHorizontal.Location = new System.Drawing.Point(9, 24);
			tbHorizontal.LoopValues = false;
			tbHorizontal.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
			tbHorizontal.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
			tbHorizontal.Name = "tbHorizontal";
			tbHorizontal.Size = new System.Drawing.Size(73, 22);
			tbHorizontal.TabIndex = 0;
			tbHorizontal.ValueChanged += tbHorizontal_ValueChanged;
			// 
			// tabFlipeffect
			// 
			tabFlipeffect.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabFlipeffect.Controls.Add(lblLaraFoot);
			tabFlipeffect.Controls.Add(comboFlipeffectConditions);
			tabFlipeffect.Controls.Add(darkLabel4);
			tabFlipeffect.Controls.Add(darkLabel7);
			tabFlipeffect.Controls.Add(tbFlipEffectFrame);
			tabFlipeffect.Controls.Add(tbFlipEffect);
			tabFlipeffect.Location = new System.Drawing.Point(4, 24);
			tabFlipeffect.Name = "tabFlipeffect";
			tabFlipeffect.Padding = new System.Windows.Forms.Padding(3);
			tabFlipeffect.Size = new System.Drawing.Size(362, 100);
			tabFlipeffect.TabIndex = 2;
			tabFlipeffect.Text = "flipeffect";
			// 
			// lblLaraFoot
			// 
			lblLaraFoot.AutoSize = true;
			lblLaraFoot.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblLaraFoot.Location = new System.Drawing.Point(164, 8);
			lblLaraFoot.Name = "lblLaraFoot";
			lblLaraFoot.Size = new System.Drawing.Size(62, 13);
			lblLaraFoot.TabIndex = 104;
			lblLaraFoot.Text = "Condition:";
			// 
			// comboFlipeffectConditions
			// 
			comboFlipeffectConditions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			comboFlipeffectConditions.FormattingEnabled = true;
			comboFlipeffectConditions.Location = new System.Drawing.Point(167, 24);
			comboFlipeffectConditions.Name = "comboFlipeffectConditions";
			comboFlipeffectConditions.Size = new System.Drawing.Size(189, 23);
			comboFlipeffectConditions.TabIndex = 103;
			comboFlipeffectConditions.SelectedIndexChanged += comboFlipeffectConditions_SelectedIndexChanged;
			// 
			// darkLabel4
			// 
			darkLabel4.AutoSize = true;
			darkLabel4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel4.Location = new System.Drawing.Point(88, 8);
			darkLabel4.Name = "darkLabel4";
			darkLabel4.Size = new System.Drawing.Size(39, 13);
			darkLabel4.TabIndex = 5;
			darkLabel4.Text = "Effect:";
			// 
			// darkLabel7
			// 
			darkLabel7.AutoSize = true;
			darkLabel7.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel7.Location = new System.Drawing.Point(6, 8);
			darkLabel7.Name = "darkLabel7";
			darkLabel7.Size = new System.Drawing.Size(41, 13);
			darkLabel7.TabIndex = 4;
			darkLabel7.Text = "Frame:";
			// 
			// tbFlipEffectFrame
			// 
			tbFlipEffectFrame.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
			tbFlipEffectFrame.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			tbFlipEffectFrame.Location = new System.Drawing.Point(9, 24);
			tbFlipEffectFrame.LoopValues = false;
			tbFlipEffectFrame.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
			tbFlipEffectFrame.Name = "tbFlipEffectFrame";
			tbFlipEffectFrame.Size = new System.Drawing.Size(73, 23);
			tbFlipEffectFrame.TabIndex = 0;
			tbFlipEffectFrame.ValueChanged += tbFlipEffectFrame_ValueChanged;
			// 
			// tbFlipEffect
			// 
			tbFlipEffect.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
			tbFlipEffect.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			tbFlipEffect.Location = new System.Drawing.Point(88, 24);
			tbFlipEffect.LoopValues = false;
			tbFlipEffect.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
			tbFlipEffect.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
			tbFlipEffect.Name = "tbFlipEffect";
			tbFlipEffect.Size = new System.Drawing.Size(73, 23);
			tbFlipEffect.TabIndex = 2;
			tbFlipEffect.ValueChanged += tbFlipEffect_ValueChanged;
			// 
			// tabPlaySound
			// 
			tabPlaySound.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabPlaySound.Controls.Add(nudSoundId);
			tabPlaySound.Controls.Add(butPlaySound);
			tabPlaySound.Controls.Add(darkLabel11);
			tabPlaySound.Controls.Add(darkLabel10);
			tabPlaySound.Controls.Add(comboSound);
			tabPlaySound.Controls.Add(tbPlaySoundFrame);
			tabPlaySound.Controls.Add(darkLabel9);
			tabPlaySound.Controls.Add(comboPlaySoundConditions);
			tabPlaySound.Location = new System.Drawing.Point(4, 24);
			tabPlaySound.Name = "tabPlaySound";
			tabPlaySound.Padding = new System.Windows.Forms.Padding(3);
			tabPlaySound.Size = new System.Drawing.Size(362, 100);
			tabPlaySound.TabIndex = 3;
			tabPlaySound.Text = "playSound";
			// 
			// nudSoundId
			// 
			nudSoundId.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			nudSoundId.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
			nudSoundId.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			nudSoundId.Location = new System.Drawing.Point(268, 53);
			nudSoundId.LoopValues = false;
			nudSoundId.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
			nudSoundId.Name = "nudSoundId";
			nudSoundId.Size = new System.Drawing.Size(59, 23);
			nudSoundId.TabIndex = 105;
			nudSoundId.ValueChanged += nudSoundId_ValueChanged;
			// 
			// butPlaySound
			// 
			butPlaySound.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butPlaySound.Checked = false;
			butPlaySound.Image = Properties.Resources.actions_play_16;
			butPlaySound.Location = new System.Drawing.Point(333, 53);
			butPlaySound.Name = "butPlaySound";
			butPlaySound.Size = new System.Drawing.Size(23, 23);
			butPlaySound.TabIndex = 104;
			butPlaySound.Click += butPlaySound_Click;
			// 
			// darkLabel11
			// 
			darkLabel11.AutoSize = true;
			darkLabel11.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel11.Location = new System.Drawing.Point(85, 8);
			darkLabel11.Name = "darkLabel11";
			darkLabel11.Size = new System.Drawing.Size(62, 13);
			darkLabel11.TabIndex = 102;
			darkLabel11.Text = "Condition:";
			// 
			// darkLabel10
			// 
			darkLabel10.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel10.Location = new System.Drawing.Point(6, 56);
			darkLabel10.Name = "darkLabel10";
			darkLabel10.Size = new System.Drawing.Size(46, 20);
			darkLabel10.TabIndex = 101;
			darkLabel10.Text = "Sound:";
			// 
			// comboSound
			// 
			comboSound.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			comboSound.Location = new System.Drawing.Point(53, 53);
			comboSound.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			comboSound.Name = "comboSound";
			comboSound.Size = new System.Drawing.Size(209, 23);
			comboSound.TabIndex = 100;
			comboSound.SelectedIndexChanged += comboSound_SelectedIndexChanged;
			// 
			// tbPlaySoundFrame
			// 
			tbPlaySoundFrame.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
			tbPlaySoundFrame.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			tbPlaySoundFrame.Location = new System.Drawing.Point(9, 24);
			tbPlaySoundFrame.LoopValues = false;
			tbPlaySoundFrame.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
			tbPlaySoundFrame.Name = "tbPlaySoundFrame";
			tbPlaySoundFrame.Size = new System.Drawing.Size(73, 23);
			tbPlaySoundFrame.TabIndex = 0;
			tbPlaySoundFrame.ValueChanged += tbPlaySoundFrame_ValueChanged;
			// 
			// darkLabel9
			// 
			darkLabel9.AutoSize = true;
			darkLabel9.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel9.Location = new System.Drawing.Point(6, 8);
			darkLabel9.Name = "darkLabel9";
			darkLabel9.Size = new System.Drawing.Size(41, 13);
			darkLabel9.TabIndex = 1;
			darkLabel9.Text = "Frame:";
			// 
			// comboPlaySoundConditions
			// 
			comboPlaySoundConditions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			comboPlaySoundConditions.FormattingEnabled = true;
			comboPlaySoundConditions.Location = new System.Drawing.Point(88, 24);
			comboPlaySoundConditions.Name = "comboPlaySoundConditions";
			comboPlaySoundConditions.Size = new System.Drawing.Size(268, 23);
			comboPlaySoundConditions.TabIndex = 53;
			comboPlaySoundConditions.SelectedIndexChanged += comboPlaySoundConditions_SelectedIndexChanged;
			// 
			// tabDisableInterpolation
			// 
			tabDisableInterpolation.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabDisableInterpolation.Controls.Add(darkLabel12);
			tabDisableInterpolation.Controls.Add(tbFrameDisableInterpolation);
			tabDisableInterpolation.Location = new System.Drawing.Point(4, 22);
			tabDisableInterpolation.Name = "tabDisableInterpolation";
			tabDisableInterpolation.Padding = new System.Windows.Forms.Padding(3);
			tabDisableInterpolation.Size = new System.Drawing.Size(362, 102);
			tabDisableInterpolation.TabIndex = 4;
			tabDisableInterpolation.Text = "disableInterpolation";
			// 
			// darkLabel12
			// 
			darkLabel12.AutoSize = true;
			darkLabel12.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel12.Location = new System.Drawing.Point(6, 8);
			darkLabel12.Name = "darkLabel12";
			darkLabel12.Size = new System.Drawing.Size(41, 13);
			darkLabel12.TabIndex = 6;
			darkLabel12.Text = "Frame:";
			// 
			// tbFrameDisableInterpolation
			// 
			tbFrameDisableInterpolation.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
			tbFrameDisableInterpolation.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			tbFrameDisableInterpolation.Location = new System.Drawing.Point(9, 24);
			tbFrameDisableInterpolation.LoopValues = false;
			tbFrameDisableInterpolation.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
			tbFrameDisableInterpolation.Name = "tbFrameDisableInterpolation";
			tbFrameDisableInterpolation.Size = new System.Drawing.Size(73, 23);
			tbFrameDisableInterpolation.TabIndex = 5;
			tbFrameDisableInterpolation.ValueChanged += tbFrameDisableInterpolation_ValueChanged;
			// 
			// tabDetachFromParent
			// 
			tabDetachFromParent.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabDetachFromParent.Controls.Add(darkLabel13);
			tabDetachFromParent.Controls.Add(tbFrameDetachFromParent);
			tabDetachFromParent.Location = new System.Drawing.Point(4, 22);
			tabDetachFromParent.Name = "tabDetachFromParent";
			tabDetachFromParent.Padding = new System.Windows.Forms.Padding(3);
			tabDetachFromParent.Size = new System.Drawing.Size(362, 102);
			tabDetachFromParent.TabIndex = 5;
			tabDetachFromParent.Text = "detachFromParent";
			// 
			// darkLabel13
			// 
			darkLabel13.AutoSize = true;
			darkLabel13.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel13.Location = new System.Drawing.Point(6, 8);
			darkLabel13.Name = "darkLabel13";
			darkLabel13.Size = new System.Drawing.Size(41, 13);
			darkLabel13.TabIndex = 6;
			darkLabel13.Text = "Frame:";
			// 
			// tbFrameDetachFromParent
			// 
			tbFrameDetachFromParent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
			tbFrameDetachFromParent.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			tbFrameDetachFromParent.Location = new System.Drawing.Point(9, 24);
			tbFrameDetachFromParent.LoopValues = false;
			tbFrameDetachFromParent.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
			tbFrameDetachFromParent.Name = "tbFrameDetachFromParent";
			tbFrameDetachFromParent.Size = new System.Drawing.Size(73, 23);
			tbFrameDetachFromParent.TabIndex = 5;
			tbFrameDetachFromParent.ValueChanged += tbFrameDetachFromParent_ValueChanged;
			// 
			// panelView
			// 
			panelView.Controls.Add(commandControls);
			panelView.Controls.Add(darkLabel8);
			panelView.Controls.Add(comboCommandType);
			panelView.Dock = System.Windows.Forms.DockStyle.Fill;
			panelView.Location = new System.Drawing.Point(0, 0);
			panelView.Name = "panelView";
			panelView.Size = new System.Drawing.Size(370, 157);
			panelView.TabIndex = 54;
			// 
			// AnimCommandEditor
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			Controls.Add(panelView);
			Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
			Name = "AnimCommandEditor";
			Size = new System.Drawing.Size(370, 157);
			commandControls.ResumeLayout(false);
			tabSetPosition.ResumeLayout(false);
			tabSetPosition.PerformLayout();
			((System.ComponentModel.ISupportInitialize)tbPosX).EndInit();
			((System.ComponentModel.ISupportInitialize)tbPosZ).EndInit();
			((System.ComponentModel.ISupportInitialize)tbPosY).EndInit();
			tabSetJumpVelocity.ResumeLayout(false);
			tabSetJumpVelocity.PerformLayout();
			((System.ComponentModel.ISupportInitialize)tbVertical).EndInit();
			((System.ComponentModel.ISupportInitialize)tbHorizontal).EndInit();
			tabFlipeffect.ResumeLayout(false);
			tabFlipeffect.PerformLayout();
			((System.ComponentModel.ISupportInitialize)tbFlipEffectFrame).EndInit();
			((System.ComponentModel.ISupportInitialize)tbFlipEffect).EndInit();
			tabPlaySound.ResumeLayout(false);
			tabPlaySound.PerformLayout();
			((System.ComponentModel.ISupportInitialize)nudSoundId).EndInit();
			((System.ComponentModel.ISupportInitialize)tbPlaySoundFrame).EndInit();
			tabDisableInterpolation.ResumeLayout(false);
			tabDisableInterpolation.PerformLayout();
			((System.ComponentModel.ISupportInitialize)tbFrameDisableInterpolation).EndInit();
			tabDetachFromParent.ResumeLayout(false);
			tabDetachFromParent.PerformLayout();
			((System.ComponentModel.ISupportInitialize)tbFrameDetachFromParent).EndInit();
			panelView.ResumeLayout(false);
			panelView.PerformLayout();
			ResumeLayout(false);
		}

		#endregion
		private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkComboBox comboCommandType;
        private TombLib.Controls.DarkTabbedContainer commandControls;
        private System.Windows.Forms.TabPage tabSetPosition;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkNumericUpDown tbPosX;
        private DarkUI.Controls.DarkNumericUpDown tbPosZ;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkNumericUpDown tbPosY;
        private System.Windows.Forms.TabPage tabSetJumpVelocity;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkNumericUpDown tbVertical;
        private DarkUI.Controls.DarkNumericUpDown tbHorizontal;
        private System.Windows.Forms.TabPage tabFlipeffect;
        private DarkUI.Controls.DarkLabel lblLaraFoot;
        private DarkUI.Controls.DarkComboBox comboFlipeffectConditions;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkNumericUpDown tbFlipEffectFrame;
        private DarkUI.Controls.DarkNumericUpDown tbFlipEffect;
        private System.Windows.Forms.TabPage tabPlaySound;
        private DarkUI.Controls.DarkNumericUpDown nudSoundId;
        private DarkUI.Controls.DarkButton butPlaySound;
        private DarkUI.Controls.DarkLabel darkLabel11;
        private DarkUI.Controls.DarkLabel darkLabel10;
        private TombLib.Controls.DarkSearchableComboBox comboSound;
        private DarkUI.Controls.DarkNumericUpDown tbPlaySoundFrame;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkComboBox comboPlaySoundConditions;
        private System.Windows.Forms.Panel panelView;
		private System.Windows.Forms.TabPage tabDisableInterpolation;
		private DarkUI.Controls.DarkLabel darkLabel12;
		private DarkUI.Controls.DarkNumericUpDown tbFrameDisableInterpolation;
		private System.Windows.Forms.TabPage tabDetachFromParent;
		private DarkUI.Controls.DarkLabel darkLabel13;
		private DarkUI.Controls.DarkNumericUpDown tbFrameDetachFromParent;
	}
}
