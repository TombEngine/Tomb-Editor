using DarkUI.Controls;

namespace TombEditor.Forms
{
    partial class FormFlybyCamera
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            toolTip = new System.Windows.Forms.ToolTip(components);
            butPrevious = new DarkButton();
            butNext = new DarkButton();
            butCancel = new DarkButton();
            butOK = new DarkButton();
            cbBit0 = new DarkCheckBox();
            cbBit1 = new DarkCheckBox();
            cbBit2 = new DarkCheckBox();
            cbBit3 = new DarkCheckBox();
            cbBit4 = new DarkCheckBox();
            cbBit5 = new DarkCheckBox();
            cbBit6 = new DarkCheckBox();
            cbBit13 = new DarkCheckBox();
            cbBit12 = new DarkCheckBox();
            cbBit11 = new DarkCheckBox();
            cbBit10 = new DarkCheckBox();
            cbBit9 = new DarkCheckBox();
            cbBit8 = new DarkCheckBox();
            cbBit7 = new DarkCheckBox();
            cbBit15 = new DarkCheckBox();
            cbBit14 = new DarkCheckBox();
            label1 = new DarkLabel();
            label2 = new DarkLabel();
            label3 = new DarkLabel();
            label4 = new DarkLabel();
            label5 = new DarkLabel();
            label6 = new DarkLabel();
            darkLabel1 = new DarkLabel();
            darkLabel2 = new DarkLabel();
            labelDofMode = new DarkLabel();
            labelDofDistance = new DarkLabel();
            labelDofRange = new DarkLabel();
            labelDofStrength = new DarkLabel();
            numSequence = new DarkNumericUpDown();
            numNumber = new DarkNumericUpDown();
            numTimer = new DarkNumericUpDown();
            numSpeed = new DarkNumericUpDown();
            numFOV = new DarkNumericUpDown();
            numRoll = new DarkNumericUpDown();
            numRotationX = new DarkNumericUpDown();
            numRotationY = new DarkNumericUpDown();
            comboDofMode = new DarkComboBox();
            numDofDistance = new DarkNumericUpDown();
            numDofRange = new DarkNumericUpDown();
            numDofStrength = new DarkNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)numSequence).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numNumber).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTimer).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSpeed).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numFOV).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRoll).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRotationX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRotationY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDofDistance).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDofRange).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDofStrength).BeginInit();
            SuspendLayout();
            // 
            // butPrevious
            // 
            butPrevious.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            butPrevious.Checked = false;
            butPrevious.Image = Properties.Resources.general_angle_left_16;
            butPrevious.Location = new System.Drawing.Point(16, 493);
            butPrevious.Margin = new System.Windows.Forms.Padding(4);
            butPrevious.Name = "butPrevious";
            butPrevious.Size = new System.Drawing.Size(90, 42);
            butPrevious.TabIndex = 2;
            butPrevious.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            toolTip.SetToolTip(butPrevious, "Navigate to the previous camera in this sequence");
            butPrevious.Click += butPrevious_Click;
            // 
            // butNext
            // 
            butNext.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            butNext.Checked = false;
            butNext.Image = Properties.Resources.general_angle_right_16;
            butNext.Location = new System.Drawing.Point(114, 493);
            butNext.Margin = new System.Windows.Forms.Padding(4);
            butNext.Name = "butNext";
            butNext.Size = new System.Drawing.Size(90, 42);
            butNext.TabIndex = 3;
            butNext.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            toolTip.SetToolTip(butNext, "Navigate to the next camera in this sequence");
            butNext.Click += butNext_Click;
            // 
            // butCancel
            // 
            butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butCancel.Checked = false;
            butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            butCancel.Location = new System.Drawing.Point(608, 569);
            butCancel.Margin = new System.Windows.Forms.Padding(4);
            butCancel.Name = "butCancel";
            butCancel.Size = new System.Drawing.Size(107, 34);
            butCancel.TabIndex = 1;
            butCancel.Text = "Cancel";
            butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butCancel.Click += butCancel_Click;
            // 
            // butOK
            // 
            butOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            butOK.Checked = false;
            butOK.Location = new System.Drawing.Point(493, 569);
            butOK.Margin = new System.Windows.Forms.Padding(4);
            butOK.Name = "butOK";
            butOK.Size = new System.Drawing.Size(107, 34);
            butOK.TabIndex = 0;
            butOK.Text = "OK";
            butOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            butOK.Click += butOK_Click;
            // 
            // cbBit0
            // 
            cbBit0.Location = new System.Drawing.Point(277, 18);
            cbBit0.Margin = new System.Windows.Forms.Padding(4);
            cbBit0.Name = "cbBit0";
            cbBit0.Size = new System.Drawing.Size(400, 47);
            cbBit0.TabIndex = 8;
            cbBit0.Text = "Make a cut to flyby from Lara camera position";
            // 
            // cbBit1
            // 
            cbBit1.Location = new System.Drawing.Point(277, 51);
            cbBit1.Margin = new System.Windows.Forms.Padding(4);
            cbBit1.Name = "cbBit1";
            cbBit1.Size = new System.Drawing.Size(400, 47);
            cbBit1.TabIndex = 9;
            cbBit1.Text = "Track entity position";
            // 
            // cbBit2
            // 
            cbBit2.Location = new System.Drawing.Point(277, 85);
            cbBit2.Margin = new System.Windows.Forms.Padding(4);
            cbBit2.Name = "cbBit2";
            cbBit2.Size = new System.Drawing.Size(400, 47);
            cbBit2.TabIndex = 10;
            cbBit2.Text = "Infinite loop";
            // 
            // cbBit3
            // 
            cbBit3.Location = new System.Drawing.Point(277, 118);
            cbBit3.Margin = new System.Windows.Forms.Padding(4);
            cbBit3.Name = "cbBit3";
            cbBit3.Size = new System.Drawing.Size(400, 47);
            cbBit3.TabIndex = 11;
            cbBit3.Text = "Create tracking camera";
            // 
            // cbBit4
            // 
            cbBit4.Location = new System.Drawing.Point(277, 152);
            cbBit4.Margin = new System.Windows.Forms.Padding(4);
            cbBit4.Name = "cbBit4";
            cbBit4.Size = new System.Drawing.Size(400, 47);
            cbBit4.TabIndex = 12;
            cbBit4.Text = "Focus on Lara's last head position";
            // 
            // cbBit5
            // 
            cbBit5.Location = new System.Drawing.Point(277, 186);
            cbBit5.Margin = new System.Windows.Forms.Padding(4);
            cbBit5.Name = "cbBit5";
            cbBit5.Size = new System.Drawing.Size(400, 47);
            cbBit5.TabIndex = 13;
            cbBit5.Text = "Focus on Lara's head";
            // 
            // cbBit6
            // 
            cbBit6.Location = new System.Drawing.Point(277, 219);
            cbBit6.Margin = new System.Windows.Forms.Padding(4);
            cbBit6.Name = "cbBit6";
            cbBit6.Size = new System.Drawing.Size(400, 47);
            cbBit6.TabIndex = 14;
            cbBit6.Text = "Snap back to Lara at the end of sequence";
            // 
            // cbBit13
            // 
            cbBit13.Location = new System.Drawing.Point(277, 455);
            cbBit13.Margin = new System.Windows.Forms.Padding(4);
            cbBit13.Name = "cbBit13";
            cbBit13.Size = new System.Drawing.Size(400, 47);
            cbBit13.TabIndex = 21;
            cbBit13.Text = "Unused";
            // 
            // cbBit12
            // 
            cbBit12.Location = new System.Drawing.Point(277, 421);
            cbBit12.Margin = new System.Windows.Forms.Padding(4);
            cbBit12.Name = "cbBit12";
            cbBit12.Size = new System.Drawing.Size(400, 47);
            cbBit12.TabIndex = 20;
            cbBit12.Text = "Unused";
            // 
            // cbBit11
            // 
            cbBit11.Location = new System.Drawing.Point(277, 387);
            cbBit11.Margin = new System.Windows.Forms.Padding(4);
            cbBit11.Name = "cbBit11";
            cbBit11.Size = new System.Drawing.Size(400, 47);
            cbBit11.TabIndex = 19;
            cbBit11.Text = "Let Lara move in cinematic mode";
            // 
            // cbBit10
            // 
            cbBit10.Location = new System.Drawing.Point(277, 354);
            cbBit10.Margin = new System.Windows.Forms.Padding(4);
            cbBit10.Name = "cbBit10";
            cbBit10.Size = new System.Drawing.Size(400, 47);
            cbBit10.TabIndex = 18;
            cbBit10.Text = "Cinematic mode";
            // 
            // cbBit9
            // 
            cbBit9.Location = new System.Drawing.Point(277, 320);
            cbBit9.Margin = new System.Windows.Forms.Padding(4);
            cbBit9.Name = "cbBit9";
            cbBit9.Size = new System.Drawing.Size(400, 47);
            cbBit9.TabIndex = 17;
            cbBit9.Text = "Disable exit from sequence with \"Look At\" key";
            // 
            // cbBit8
            // 
            cbBit8.Location = new System.Drawing.Point(277, 286);
            cbBit8.Margin = new System.Windows.Forms.Padding(4);
            cbBit8.Name = "cbBit8";
            cbBit8.Size = new System.Drawing.Size(400, 47);
            cbBit8.TabIndex = 16;
            cbBit8.Text = "Freeze camera (Timer field)";
            // 
            // cbBit7
            // 
            cbBit7.Location = new System.Drawing.Point(277, 253);
            cbBit7.Margin = new System.Windows.Forms.Padding(4);
            cbBit7.Name = "cbBit7";
            cbBit7.Size = new System.Drawing.Size(423, 47);
            cbBit7.TabIndex = 15;
            cbBit7.Text = "Jump to another camera in sequence (Timer field)";
            // 
            // cbBit15
            // 
            cbBit15.Location = new System.Drawing.Point(277, 522);
            cbBit15.Margin = new System.Windows.Forms.Padding(4);
            cbBit15.Name = "cbBit15";
            cbBit15.Size = new System.Drawing.Size(400, 47);
            cbBit15.TabIndex = 23;
            cbBit15.Text = "Unused";
            // 
            // cbBit14
            // 
            cbBit14.Location = new System.Drawing.Point(277, 488);
            cbBit14.Margin = new System.Windows.Forms.Padding(4);
            cbBit14.Name = "cbBit14";
            cbBit14.Size = new System.Drawing.Size(400, 47);
            cbBit14.TabIndex = 22;
            cbBit14.Text = "Activate heavy trigger";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            label1.Location = new System.Drawing.Point(16, 32);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(70, 19);
            label1.TabIndex = 18;
            label1.Text = "Sequence:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            label2.Location = new System.Drawing.Point(16, 70);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(62, 19);
            label2.TabIndex = 20;
            label2.Text = "Number:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            label3.Location = new System.Drawing.Point(16, 184);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(39, 19);
            label3.TabIndex = 22;
            label3.Text = "FOV:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            label4.Location = new System.Drawing.Point(16, 222);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(34, 19);
            label4.TabIndex = 24;
            label4.Text = "Roll:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            label5.Location = new System.Drawing.Point(16, 108);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(46, 19);
            label5.TabIndex = 26;
            label5.Text = "Timer:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            label6.Location = new System.Drawing.Point(16, 146);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(49, 19);
            label6.TabIndex = 28;
            label6.Text = "Speed:";
            // 
            // darkLabel1
            // 
            darkLabel1.AutoSize = true;
            darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel1.Location = new System.Drawing.Point(16, 260);
            darkLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new System.Drawing.Size(76, 19);
            darkLabel1.TabIndex = 22;
            darkLabel1.Text = "Rotation X:";
            // 
            // darkLabel2
            // 
            darkLabel2.AutoSize = true;
            darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            darkLabel2.Location = new System.Drawing.Point(16, 298);
            darkLabel2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new System.Drawing.Size(76, 19);
            darkLabel2.TabIndex = 24;
            darkLabel2.Text = "Rotation Y:";
            // 
            // labelDofMode
            // 
            labelDofMode.AutoSize = true;
            labelDofMode.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            labelDofMode.Location = new System.Drawing.Point(16, 336);
            labelDofMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelDofMode.Name = "labelDofMode";
            labelDofMode.Size = new System.Drawing.Size(79, 19);
            labelDofMode.TabIndex = 30;
            labelDofMode.Text = "DOF mode:";
            // 
            // labelDofDistance
            // 
            labelDofDistance.AutoSize = true;
            labelDofDistance.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            labelDofDistance.Location = new System.Drawing.Point(16, 374);
            labelDofDistance.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelDofDistance.Name = "labelDofDistance";
            labelDofDistance.Size = new System.Drawing.Size(64, 19);
            labelDofDistance.TabIndex = 31;
            labelDofDistance.Text = "Distance:";
            // 
            // labelDofRange
            // 
            labelDofRange.AutoSize = true;
            labelDofRange.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            labelDofRange.Location = new System.Drawing.Point(16, 412);
            labelDofRange.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelDofRange.Name = "labelDofRange";
            labelDofRange.Size = new System.Drawing.Size(50, 19);
            labelDofRange.TabIndex = 32;
            labelDofRange.Text = "Range:";
            // 
            // labelDofStrength
            // 
            labelDofStrength.AutoSize = true;
            labelDofStrength.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            labelDofStrength.Location = new System.Drawing.Point(16, 450);
            labelDofStrength.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelDofStrength.Name = "labelDofStrength";
            labelDofStrength.Size = new System.Drawing.Size(65, 19);
            labelDofStrength.TabIndex = 33;
            labelDofStrength.Text = "Strength:";
            // 
            // numSequence
            // 
            numSequence.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numSequence.Location = new System.Drawing.Point(109, 29);
            numSequence.LoopValues = false;
            numSequence.Margin = new System.Windows.Forms.Padding(4);
            numSequence.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numSequence.Name = "numSequence";
            numSequence.Size = new System.Drawing.Size(95, 26);
            numSequence.TabIndex = 0;
            // 
            // numNumber
            // 
            numNumber.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numNumber.Location = new System.Drawing.Point(109, 67);
            numNumber.LoopValues = false;
            numNumber.Margin = new System.Windows.Forms.Padding(4);
            numNumber.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numNumber.Name = "numNumber";
            numNumber.Size = new System.Drawing.Size(95, 26);
            numNumber.TabIndex = 1;
            // 
            // numTimer
            // 
            numTimer.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numTimer.Location = new System.Drawing.Point(109, 105);
            numTimer.LoopValues = false;
            numTimer.Margin = new System.Windows.Forms.Padding(4);
            numTimer.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            numTimer.Minimum = new decimal(new int[] { 32768, 0, 0, int.MinValue });
            numTimer.Name = "numTimer";
            numTimer.Size = new System.Drawing.Size(95, 26);
            numTimer.TabIndex = 2;
            // 
            // numSpeed
            // 
            numSpeed.DecimalPlaces = 2;
            numSpeed.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numSpeed.Location = new System.Drawing.Point(109, 143);
            numSpeed.LoopValues = false;
            numSpeed.Margin = new System.Windows.Forms.Padding(4);
            numSpeed.Name = "numSpeed";
            numSpeed.Size = new System.Drawing.Size(95, 26);
            numSpeed.TabIndex = 3;
            // 
            // numFOV
            // 
            numFOV.DecimalPlaces = 2;
            numFOV.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numFOV.Location = new System.Drawing.Point(109, 181);
            numFOV.LoopValues = false;
            numFOV.Margin = new System.Windows.Forms.Padding(4);
            numFOV.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            numFOV.Name = "numFOV";
            numFOV.Size = new System.Drawing.Size(95, 26);
            numFOV.TabIndex = 4;
            // 
            // numRoll
            // 
            numRoll.DecimalPlaces = 2;
            numRoll.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numRoll.Location = new System.Drawing.Point(109, 219);
            numRoll.LoopValues = true;
            numRoll.Margin = new System.Windows.Forms.Padding(4);
            numRoll.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            numRoll.Name = "numRoll";
            numRoll.Size = new System.Drawing.Size(95, 26);
            numRoll.TabIndex = 5;
            // 
            // numRotationX
            // 
            numRotationX.DecimalPlaces = 2;
            numRotationX.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numRotationX.Location = new System.Drawing.Point(109, 257);
            numRotationX.LoopValues = true;
            numRotationX.Margin = new System.Windows.Forms.Padding(4);
            numRotationX.Maximum = new decimal(new int[] { 90, 0, 0, 0 });
            numRotationX.Minimum = new decimal(new int[] { 90, 0, 0, int.MinValue });
            numRotationX.Name = "numRotationX";
            numRotationX.Size = new System.Drawing.Size(95, 26);
            numRotationX.TabIndex = 6;
            // 
            // numRotationY
            // 
            numRotationY.DecimalPlaces = 2;
            numRotationY.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
            numRotationY.Location = new System.Drawing.Point(109, 295);
            numRotationY.LoopValues = true;
            numRotationY.Margin = new System.Windows.Forms.Padding(4);
            numRotationY.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            numRotationY.Name = "numRotationY";
            numRotationY.Size = new System.Drawing.Size(95, 26);
            numRotationY.TabIndex = 7;
            // 
            // comboDofMode
            // 
            comboDofMode.FormattingEnabled = true;
            comboDofMode.Items.AddRange(new object[] { "None", "Full", "Front", "Back" });
            comboDofMode.Location = new System.Drawing.Point(109, 333);
            comboDofMode.Margin = new System.Windows.Forms.Padding(4);
            comboDofMode.Name = "comboDofMode";
            comboDofMode.Size = new System.Drawing.Size(93, 27);
            comboDofMode.TabIndex = 8;
            // 
            // numDofDistance
            // 
            numDofDistance.DecimalPlaces = 2;
            numDofDistance.Increment = new decimal(new int[] { 128, 0, 0, 0 });
            numDofDistance.IncrementAlternate = new decimal(new int[] { 128, 0, 0, 0 });
            numDofDistance.Location = new System.Drawing.Point(109, 371);
            numDofDistance.LoopValues = false;
            numDofDistance.Margin = new System.Windows.Forms.Padding(4);
            numDofDistance.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numDofDistance.Name = "numDofDistance";
            numDofDistance.Size = new System.Drawing.Size(95, 26);
            numDofDistance.TabIndex = 9;
            // 
            // numDofRange
            // 
            numDofRange.DecimalPlaces = 2;
            numDofRange.Increment = new decimal(new int[] { 128, 0, 0, 0 });
            numDofRange.IncrementAlternate = new decimal(new int[] { 128, 0, 0, 0 });
            numDofRange.Location = new System.Drawing.Point(109, 409);
            numDofRange.LoopValues = false;
            numDofRange.Margin = new System.Windows.Forms.Padding(4);
            numDofRange.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numDofRange.Name = "numDofRange";
            numDofRange.Size = new System.Drawing.Size(95, 26);
            numDofRange.TabIndex = 10;
            // 
            // numDofStrength
            // 
            numDofStrength.DecimalPlaces = 2;
            numDofStrength.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            numDofStrength.IncrementAlternate = new decimal(new int[] { 1, 0, 0, 65536 });
            numDofStrength.Location = new System.Drawing.Point(109, 447);
            numDofStrength.LoopValues = false;
            numDofStrength.Margin = new System.Windows.Forms.Padding(4);
            numDofStrength.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            numDofStrength.Name = "numDofStrength";
            numDofStrength.Size = new System.Drawing.Size(95, 26);
            numDofStrength.TabIndex = 11;
            // 
            // FormFlybyCamera
            // 
            AcceptButton = butOK;
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = butCancel;
            ClientSize = new System.Drawing.Size(721, 611);
            Controls.Add(numDofStrength);
            Controls.Add(numDofRange);
            Controls.Add(numDofDistance);
            Controls.Add(comboDofMode);
            Controls.Add(numRotationY);
            Controls.Add(numRotationX);
            Controls.Add(numRoll);
            Controls.Add(numFOV);
            Controls.Add(numSpeed);
            Controls.Add(numTimer);
            Controls.Add(numNumber);
            Controls.Add(numSequence);
            Controls.Add(labelDofStrength);
            Controls.Add(labelDofRange);
            Controls.Add(labelDofDistance);
            Controls.Add(labelDofMode);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(darkLabel2);
            Controls.Add(label4);
            Controls.Add(darkLabel1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(cbBit15);
            Controls.Add(cbBit14);
            Controls.Add(cbBit13);
            Controls.Add(cbBit12);
            Controls.Add(cbBit11);
            Controls.Add(cbBit10);
            Controls.Add(cbBit9);
            Controls.Add(cbBit8);
            Controls.Add(cbBit7);
            Controls.Add(cbBit6);
            Controls.Add(cbBit5);
            Controls.Add(cbBit4);
            Controls.Add(cbBit3);
            Controls.Add(cbBit2);
            Controls.Add(cbBit1);
            Controls.Add(cbBit0);
            Controls.Add(butNext);
            Controls.Add(butPrevious);
            Controls.Add(butCancel);
            Controls.Add(butOK);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            Margin = new System.Windows.Forms.Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormFlybyCamera";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Flyby camera";
            Load += FormFlybyCamera_Load;
            ((System.ComponentModel.ISupportInitialize)numSequence).EndInit();
            ((System.ComponentModel.ISupportInitialize)numNumber).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTimer).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSpeed).EndInit();
            ((System.ComponentModel.ISupportInitialize)numFOV).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRoll).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRotationX).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRotationY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDofDistance).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDofRange).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDofStrength).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private DarkButton butOK;
        private DarkButton butCancel;
        private DarkButton butPrevious;
        private DarkButton butNext;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkCheckBox cbBit0;
        private DarkCheckBox cbBit1;
        private DarkCheckBox cbBit2;
        private DarkCheckBox cbBit3;
        private DarkCheckBox cbBit4;
        private DarkCheckBox cbBit5;
        private DarkCheckBox cbBit6;
        private DarkCheckBox cbBit13;
        private DarkCheckBox cbBit12;
        private DarkCheckBox cbBit11;
        private DarkCheckBox cbBit10;
        private DarkCheckBox cbBit9;
        private DarkCheckBox cbBit8;
        private DarkCheckBox cbBit7;
        private DarkCheckBox cbBit15;
        private DarkCheckBox cbBit14;
        private DarkLabel label1;
        private DarkLabel label2;
        private DarkLabel label3;
        private DarkLabel label4;
        private DarkLabel label5;
        private DarkLabel label6;
        private DarkLabel darkLabel1;
        private DarkLabel darkLabel2;
        private DarkLabel labelDofMode;
        private DarkLabel labelDofDistance;
        private DarkLabel labelDofRange;
        private DarkLabel labelDofStrength;
        private DarkNumericUpDown numSequence;
        private DarkNumericUpDown numNumber;
        private DarkNumericUpDown numTimer;
        private DarkNumericUpDown numSpeed;
        private DarkNumericUpDown numFOV;
        private DarkNumericUpDown numRoll;
        private DarkNumericUpDown numRotationX;
        private DarkNumericUpDown numRotationY;
        private DarkComboBox comboDofMode;
        private DarkNumericUpDown numDofDistance;
        private DarkNumericUpDown numDofRange;
        private DarkNumericUpDown numDofStrength;
    }
}
