using System.Windows.Forms;
using DarkUI.Controls;

namespace TombLib.Forms
{
    partial class FormAnimatedTextures
    {
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			darkLabel1 = new DarkLabel();
			tableLayoutPanel1 = new TableLayoutPanel();
			panel2 = new Panel();
			panelTextureMapContainer = new Panel();
			comboCurrentTexture = new Controls.DarkSearchableComboBox();
			panel1 = new Panel();
			darkPanel1 = new DarkPanel();
			previewImage = new PictureBox();
			panel4 = new DarkPanel();
			settingsPanel = new Panel();
			numericUpDownUvRotateDirection = new DarkNumericUpDown();
			comboUvRotate = new DarkComboBox();
			comboEffect = new DarkComboBox();
			numericUpDownFPS = new DarkNumericUpDown();
			comboFps = new DarkComboBox();
			lblUvRotate = new DarkLabel();
			lblEffect = new DarkLabel();
			lblFps = new DarkLabel();
			butEditSetName = new DarkButton();
			lblProcAnim = new DarkLabel();
			panel3 = new DarkPanel();
			butAddProcAnim = new DarkButton();
			butReplaceProcAnim = new DarkButton();
			butMergeProcAnim = new DarkButton();
			butCloneProcAnim = new DarkButton();
			cbSmooth = new DarkCheckBox();
			cbLoop = new DarkCheckBox();
			numFrames = new DarkNumericUpDown();
			lblFrames = new DarkLabel();
			lblProgress = new DarkLabel();
			lblAnimatorName = new DarkLabel();
			numStrength = new DarkNumericUpDown();
			butGenerateProcAnim = new DarkButton();
			comboProcPresets = new DarkComboBox();
			butUpdate = new DarkButton();
			label1 = new DarkLabel();
			tooManyFramesWarning = new DarkLabel();
			previewProgressBar = new DarkProgressBar();
			texturesDataGridView = new DarkDataGridView();
			texturesDataGridViewColumnImage = new DataGridViewImageColumn();
			texturesDataGridViewColumnRepeat = new DataGridViewTextBoxColumn();
			texturesDataGridViewColumnTexture = new DarkDataGridViewComboBoxColumn();
			texturesDataGridViewColumnArea = new DataGridViewTextBoxColumn();
			texturesDataGridViewColumnTexCoord0 = new DataGridViewTextBoxColumn();
			texturesDataGridViewColumnTexCoord1 = new DataGridViewTextBoxColumn();
			texturesDataGridViewColumnTexCoord2 = new DataGridViewTextBoxColumn();
			texturesDataGridViewColumnTexCoord3 = new DataGridViewTextBoxColumn();
			lblHeaderNgSettings = new DarkLabel();
			lblPreview = new DarkLabel();
			texturesDataGridViewControls = new Controls.DarkDataGridViewControls();
			butAnimatedTextureSetDelete = new DarkButton();
			butAnimatedTextureSetNew = new DarkButton();
			comboAnimatedTextureSets = new DarkComboBox();
			butOk = new DarkButton();
			toolTip = new ToolTip(components);
			butCancel = new DarkButton();
			tableLayoutPanel1.SuspendLayout();
			panel2.SuspendLayout();
			panel1.SuspendLayout();
			darkPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)previewImage).BeginInit();
			panel4.SuspendLayout();
			settingsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)numericUpDownUvRotateDirection).BeginInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownFPS).BeginInit();
			panel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)numFrames).BeginInit();
			((System.ComponentModel.ISupportInitialize)numStrength).BeginInit();
			((System.ComponentModel.ISupportInitialize)texturesDataGridView).BeginInit();
			SuspendLayout();
			// 
			// darkLabel1
			// 
			darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel1.Location = new System.Drawing.Point(2, 5);
			darkLabel1.Name = "darkLabel1";
			darkLabel1.Size = new System.Drawing.Size(82, 17);
			darkLabel1.TabIndex = 0;
			darkLabel1.Text = "Animation set:";
			darkLabel1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// tableLayoutPanel1
			// 
			tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			tableLayoutPanel1.ColumnCount = 2;
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56.49652F));
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43.50348F));
			tableLayoutPanel1.Controls.Add(panel2, 0, 0);
			tableLayoutPanel1.Controls.Add(panel1, 0, 0);
			tableLayoutPanel1.Location = new System.Drawing.Point(4, 4);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			tableLayoutPanel1.RowCount = 1;
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 618F));
			tableLayoutPanel1.Size = new System.Drawing.Size(862, 618);
			tableLayoutPanel1.TabIndex = 43;
			// 
			// panel2
			// 
			panel2.Controls.Add(panelTextureMapContainer);
			panel2.Controls.Add(comboCurrentTexture);
			panel2.Dock = DockStyle.Fill;
			panel2.Location = new System.Drawing.Point(490, 3);
			panel2.Name = "panel2";
			panel2.Size = new System.Drawing.Size(369, 612);
			panel2.TabIndex = 21;
			// 
			// panelTextureMapContainer
			// 
			panelTextureMapContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			panelTextureMapContainer.Location = new System.Drawing.Point(4, 33);
			panelTextureMapContainer.Name = "panelTextureMapContainer";
			panelTextureMapContainer.Size = new System.Drawing.Size(365, 579);
			panelTextureMapContainer.TabIndex = 21;
			// 
			// comboCurrentTexture
			// 
			comboCurrentTexture.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			comboCurrentTexture.Location = new System.Drawing.Point(4, 5);
			comboCurrentTexture.Margin = new Padding(4, 3, 4, 3);
			comboCurrentTexture.Name = "comboCurrentTexture";
			comboCurrentTexture.Size = new System.Drawing.Size(359, 23);
			comboCurrentTexture.TabIndex = 20;
			comboCurrentTexture.SelectedValueChanged += comboCurrentTexture_SelectedValueChanged;
			// 
			// panel1
			// 
			panel1.Controls.Add(darkPanel1);
			panel1.Controls.Add(panel4);
			panel1.Controls.Add(butEditSetName);
			panel1.Controls.Add(lblProcAnim);
			panel1.Controls.Add(panel3);
			panel1.Controls.Add(butUpdate);
			panel1.Controls.Add(label1);
			panel1.Controls.Add(tooManyFramesWarning);
			panel1.Controls.Add(previewProgressBar);
			panel1.Controls.Add(texturesDataGridView);
			panel1.Controls.Add(lblHeaderNgSettings);
			panel1.Controls.Add(lblPreview);
			panel1.Controls.Add(texturesDataGridViewControls);
			panel1.Controls.Add(butAnimatedTextureSetDelete);
			panel1.Controls.Add(butAnimatedTextureSetNew);
			panel1.Controls.Add(darkLabel1);
			panel1.Controls.Add(comboAnimatedTextureSets);
			panel1.Dock = DockStyle.Fill;
			panel1.Location = new System.Drawing.Point(3, 3);
			panel1.Margin = new Padding(3, 3, 3, 2);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(481, 613);
			panel1.TabIndex = 0;
			// 
			// darkPanel1
			// 
			darkPanel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			darkPanel1.BorderStyle = BorderStyle.FixedSingle;
			darkPanel1.Controls.Add(previewImage);
			darkPanel1.Location = new System.Drawing.Point(346, 452);
			darkPanel1.Name = "darkPanel1";
			darkPanel1.Size = new System.Drawing.Size(135, 135);
			darkPanel1.TabIndex = 24;
			// 
			// previewImage
			// 
			previewImage.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			previewImage.Location = new System.Drawing.Point(1, 1);
			previewImage.Name = "previewImage";
			previewImage.Size = new System.Drawing.Size(133, 133);
			previewImage.TabIndex = 13;
			previewImage.TabStop = false;
			// 
			// panel4
			// 
			panel4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			panel4.BorderStyle = BorderStyle.FixedSingle;
			panel4.Controls.Add(settingsPanel);
			panel4.Controls.Add(lblUvRotate);
			panel4.Controls.Add(lblEffect);
			panel4.Controls.Add(lblFps);
			panel4.Location = new System.Drawing.Point(5, 558);
			panel4.Name = "panel4";
			panel4.Size = new System.Drawing.Size(335, 52);
			panel4.TabIndex = 23;
			// 
			// settingsPanel
			// 
			settingsPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			settingsPanel.Controls.Add(numericUpDownUvRotateDirection);
			settingsPanel.Controls.Add(comboUvRotate);
			settingsPanel.Controls.Add(comboEffect);
			settingsPanel.Controls.Add(numericUpDownFPS);
			settingsPanel.Controls.Add(comboFps);
			settingsPanel.Location = new System.Drawing.Point(5, 22);
			settingsPanel.Name = "settingsPanel";
			settingsPanel.Size = new System.Drawing.Size(323, 24);
			settingsPanel.TabIndex = 12;
			// 
			// numericUpDownUvRotateDirection
			// 
			numericUpDownUvRotateDirection.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			numericUpDownUvRotateDirection.DecimalPlaces = 2;
			numericUpDownUvRotateDirection.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			numericUpDownUvRotateDirection.IncrementAlternate = new decimal(new int[] { 25, 0, 0, 131072 });
			numericUpDownUvRotateDirection.Location = new System.Drawing.Point(184, 0);
			numericUpDownUvRotateDirection.LoopValues = false;
			numericUpDownUvRotateDirection.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
			numericUpDownUvRotateDirection.Name = "numericUpDownUvRotateDirection";
			numericUpDownUvRotateDirection.Size = new System.Drawing.Size(139, 23);
			numericUpDownUvRotateDirection.TabIndex = 20;
			numericUpDownUvRotateDirection.Visible = false;
			numericUpDownUvRotateDirection.ValueChanged += numericUpDownUvRotateDirection_ValueChanged;
			// 
			// comboUvRotate
			// 
			comboUvRotate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			comboUvRotate.Location = new System.Drawing.Point(184, 0);
			comboUvRotate.Name = "comboUvRotate";
			comboUvRotate.Size = new System.Drawing.Size(139, 23);
			comboUvRotate.TabIndex = 19;
			comboUvRotate.SelectionChangeCommitted += comboUvRotate_SelectionChangeCommitted;
			// 
			// comboEffect
			// 
			comboEffect.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			comboEffect.Location = new System.Drawing.Point(0, 0);
			comboEffect.Name = "comboEffect";
			comboEffect.Size = new System.Drawing.Size(101, 23);
			comboEffect.TabIndex = 17;
			comboEffect.SelectedIndexChanged += comboEffect_SelectedIndexChanged;
			comboEffect.SelectionChangeCommitted += comboEffect_SelectionChangeCommitted;
			// 
			// numericUpDownFPS
			// 
			numericUpDownFPS.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			numericUpDownFPS.DecimalPlaces = 2;
			numericUpDownFPS.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			numericUpDownFPS.IncrementAlternate = new decimal(new int[] { 25, 0, 0, 131072 });
			numericUpDownFPS.Location = new System.Drawing.Point(107, 0);
			numericUpDownFPS.LoopValues = false;
			numericUpDownFPS.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
			numericUpDownFPS.Name = "numericUpDownFPS";
			numericUpDownFPS.Size = new System.Drawing.Size(71, 23);
			numericUpDownFPS.TabIndex = 18;
			numericUpDownFPS.Value = new decimal(new int[] { 16, 0, 0, 0 });
			numericUpDownFPS.ValueChanged += numericUpDownFPS_ValueChanged;
			// 
			// comboFps
			// 
			comboFps.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			comboFps.Location = new System.Drawing.Point(107, 0);
			comboFps.Name = "comboFps";
			comboFps.Size = new System.Drawing.Size(71, 23);
			comboFps.TabIndex = 9;
			comboFps.SelectionChangeCommitted += comboFps_SelectionChangeCommitted;
			// 
			// lblUvRotate
			// 
			lblUvRotate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblUvRotate.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblUvRotate.Location = new System.Drawing.Point(186, 6);
			lblUvRotate.Name = "lblUvRotate";
			lblUvRotate.Size = new System.Drawing.Size(142, 13);
			lblUvRotate.TabIndex = 10;
			lblUvRotate.Text = "UvRotate:";
			// 
			// lblEffect
			// 
			lblEffect.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblEffect.Location = new System.Drawing.Point(2, 6);
			lblEffect.Name = "lblEffect";
			lblEffect.Size = new System.Drawing.Size(41, 14);
			lblEffect.TabIndex = 6;
			lblEffect.Text = "Type:";
			// 
			// lblFps
			// 
			lblFps.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblFps.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblFps.Location = new System.Drawing.Point(109, 6);
			lblFps.Name = "lblFps";
			lblFps.Size = new System.Drawing.Size(71, 14);
			lblFps.TabIndex = 8;
			lblFps.Text = "FPS:";
			// 
			// butEditSetName
			// 
			butEditSetName.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			butEditSetName.Checked = false;
			butEditSetName.Image = Properties.Resources.general_edit_16;
			butEditSetName.Location = new System.Drawing.Point(399, 5);
			butEditSetName.Name = "butEditSetName";
			butEditSetName.Size = new System.Drawing.Size(24, 24);
			butEditSetName.TabIndex = 2;
			butEditSetName.Tag = "EditRoomName";
			butEditSetName.Click += butEditSetName_Click;
			// 
			// lblProcAnim
			// 
			lblProcAnim.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			lblProcAnim.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblProcAnim.Location = new System.Drawing.Point(1, 431);
			lblProcAnim.Name = "lblProcAnim";
			lblProcAnim.Size = new System.Drawing.Size(246, 18);
			lblProcAnim.TabIndex = 21;
			lblProcAnim.Text = "Procedural animation tool";
			lblProcAnim.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// panel3
			// 
			panel3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			panel3.BorderStyle = BorderStyle.FixedSingle;
			panel3.Controls.Add(butAddProcAnim);
			panel3.Controls.Add(butReplaceProcAnim);
			panel3.Controls.Add(butMergeProcAnim);
			panel3.Controls.Add(butCloneProcAnim);
			panel3.Controls.Add(cbSmooth);
			panel3.Controls.Add(cbLoop);
			panel3.Controls.Add(numFrames);
			panel3.Controls.Add(lblFrames);
			panel3.Controls.Add(lblProgress);
			panel3.Controls.Add(lblAnimatorName);
			panel3.Controls.Add(numStrength);
			panel3.Controls.Add(butGenerateProcAnim);
			panel3.Controls.Add(comboProcPresets);
			panel3.Location = new System.Drawing.Point(5, 452);
			panel3.Name = "panel3";
			panel3.Size = new System.Drawing.Size(335, 82);
			panel3.TabIndex = 20;
			// 
			// butAddProcAnim
			// 
			butAddProcAnim.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			butAddProcAnim.Checked = false;
			butAddProcAnim.Image = Properties.Resources.general_plus_math_16;
			butAddProcAnim.Location = new System.Drawing.Point(276, 52);
			butAddProcAnim.Name = "butAddProcAnim";
			butAddProcAnim.Size = new System.Drawing.Size(23, 23);
			butAddProcAnim.TabIndex = 15;
			butAddProcAnim.Tag = "";
			toolTip.SetToolTip(butAddProcAnim, "Generate and add frames after current sequence frame");
			butAddProcAnim.Click += butAddProcAnim_Click;
			// 
			// butReplaceProcAnim
			// 
			butReplaceProcAnim.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			butReplaceProcAnim.Checked = false;
			butReplaceProcAnim.Image = Properties.Resources.actions_refresh_16;
			butReplaceProcAnim.Location = new System.Drawing.Point(305, 52);
			butReplaceProcAnim.Name = "butReplaceProcAnim";
			butReplaceProcAnim.Size = new System.Drawing.Size(23, 23);
			butReplaceProcAnim.TabIndex = 16;
			butReplaceProcAnim.Tag = "";
			toolTip.SetToolTip(butReplaceProcAnim, "Generate and replace current animation");
			butReplaceProcAnim.Click += butReplaceProcAnim_Click;
			// 
			// butMergeProcAnim
			// 
			butMergeProcAnim.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			butMergeProcAnim.Checked = false;
			butMergeProcAnim.Image = Properties.Resources.actions_Merge_16;
			butMergeProcAnim.Location = new System.Drawing.Point(247, 52);
			butMergeProcAnim.Name = "butMergeProcAnim";
			butMergeProcAnim.Size = new System.Drawing.Size(23, 23);
			butMergeProcAnim.TabIndex = 14;
			butMergeProcAnim.Tag = "";
			toolTip.SetToolTip(butMergeProcAnim, "Generate and merge into current animation");
			butMergeProcAnim.Click += butMergeProcAnim_Click;
			// 
			// butCloneProcAnim
			// 
			butCloneProcAnim.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			butCloneProcAnim.Checked = false;
			butCloneProcAnim.Image = Properties.Resources.general_copy_16;
			butCloneProcAnim.Location = new System.Drawing.Point(218, 52);
			butCloneProcAnim.Name = "butCloneProcAnim";
			butCloneProcAnim.Size = new System.Drawing.Size(23, 23);
			butCloneProcAnim.TabIndex = 13;
			butCloneProcAnim.Tag = "";
			toolTip.SetToolTip(butCloneProcAnim, "Generate and merge into copy of current animation");
			butCloneProcAnim.Click += butCloneProcAnim_Click;
			// 
			// cbSmooth
			// 
			cbSmooth.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cbSmooth.Location = new System.Drawing.Point(87, 56);
			cbSmooth.Name = "cbSmooth";
			cbSmooth.Size = new System.Drawing.Size(91, 17);
			cbSmooth.TabIndex = 11;
			cbSmooth.Text = "Smooth";
			toolTip.SetToolTip(cbSmooth, "Ease-in and ease-out animation");
			// 
			// cbLoop
			// 
			cbLoop.Location = new System.Drawing.Point(5, 56);
			cbLoop.Name = "cbLoop";
			cbLoop.Size = new System.Drawing.Size(72, 17);
			cbLoop.TabIndex = 10;
			cbLoop.Text = "Symmetric";
			toolTip.SetToolTip(cbLoop, "Make animation symmetric");
			// 
			// numFrames
			// 
			numFrames.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			numFrames.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			numFrames.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			numFrames.Location = new System.Drawing.Point(261, 23);
			numFrames.LoopValues = false;
			numFrames.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
			numFrames.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			numFrames.Name = "numFrames";
			numFrames.Size = new System.Drawing.Size(67, 23);
			numFrames.TabIndex = 9;
			toolTip.SetToolTip(numFrames, "Amount of resulting animation frames");
			numFrames.Value = new decimal(new int[] { 1, 0, 0, 0 });
			// 
			// lblFrames
			// 
			lblFrames.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblFrames.AutoSize = true;
			lblFrames.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblFrames.Location = new System.Drawing.Point(258, 7);
			lblFrames.Name = "lblFrames";
			lblFrames.Size = new System.Drawing.Size(46, 13);
			lblFrames.TabIndex = 14;
			lblFrames.Text = "Frames:";
			// 
			// lblProgress
			// 
			lblProgress.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lblProgress.AutoSize = true;
			lblProgress.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblProgress.Location = new System.Drawing.Point(186, 6);
			lblProgress.Name = "lblProgress";
			lblProgress.Size = new System.Drawing.Size(66, 13);
			lblProgress.TabIndex = 12;
			lblProgress.Text = "Progress %:";
			// 
			// lblAnimatorName
			// 
			lblAnimatorName.AutoSize = true;
			lblAnimatorName.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblAnimatorName.Location = new System.Drawing.Point(3, 7);
			lblAnimatorName.Name = "lblAnimatorName";
			lblAnimatorName.Size = new System.Drawing.Size(57, 13);
			lblAnimatorName.TabIndex = 11;
			lblAnimatorName.Text = "Animator:";
			// 
			// numStrength
			// 
			numStrength.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			numStrength.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			numStrength.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			numStrength.Location = new System.Drawing.Point(189, 23);
			numStrength.LoopValues = false;
			numStrength.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
			numStrength.Name = "numStrength";
			numStrength.Size = new System.Drawing.Size(66, 23);
			numStrength.TabIndex = 8;
			toolTip.SetToolTip(numStrength, "Effect progress or strength");
			numStrength.Value = new decimal(new int[] { 100, 0, 0, 0 });
			// 
			// butGenerateProcAnim
			// 
			butGenerateProcAnim.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			butGenerateProcAnim.Checked = false;
			butGenerateProcAnim.Image = Properties.Resources.general_create_new_16;
			butGenerateProcAnim.Location = new System.Drawing.Point(189, 52);
			butGenerateProcAnim.Name = "butGenerateProcAnim";
			butGenerateProcAnim.Size = new System.Drawing.Size(23, 23);
			butGenerateProcAnim.TabIndex = 12;
			butGenerateProcAnim.Tag = "";
			toolTip.SetToolTip(butGenerateProcAnim, "Generate new animation");
			butGenerateProcAnim.Click += butGenerateProcAnim_Click;
			// 
			// comboProcPresets
			// 
			comboProcPresets.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			comboProcPresets.Items.AddRange(new object[] { "Stretch horizontal", "Stretch vertical", "Scale", "Skew horizontal ↖", "Skew horizontal ↗", "Skew vertical ↖", "Skew vertical ↗", "Spin", "Pan horizontal", "Pan vertical", "Shake" });
			comboProcPresets.Location = new System.Drawing.Point(5, 23);
			comboProcPresets.Name = "comboProcPresets";
			comboProcPresets.Size = new System.Drawing.Size(178, 23);
			comboProcPresets.TabIndex = 7;
			toolTip.SetToolTip(comboProcPresets, "Animation type");
			// 
			// butUpdate
			// 
			butUpdate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			butUpdate.Checked = false;
			butUpdate.Enabled = false;
			butUpdate.Image = Properties.Resources.actions_refresh_16;
			butUpdate.ImagePadding = 4;
			butUpdate.Location = new System.Drawing.Point(457, 115);
			butUpdate.Margin = new Padding(0, 0, 2, 0);
			butUpdate.Name = "butUpdate";
			butUpdate.Size = new System.Drawing.Size(24, 24);
			butUpdate.TabIndex = 2;
			toolTip.SetToolTip(butUpdate, "Replace frame with selected texture");
			butUpdate.Click += butUpdate_Click;
			// 
			// label1
			// 
			label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			label1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			label1.Location = new System.Drawing.Point(1, 37);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(473, 15);
			label1.TabIndex = 17;
			label1.Text = "Frames";
			label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// tooManyFramesWarning
			// 
			tooManyFramesWarning.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
			tooManyFramesWarning.BackColor = System.Drawing.Color.Firebrick;
			tooManyFramesWarning.BorderStyle = BorderStyle.FixedSingle;
			tooManyFramesWarning.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			tooManyFramesWarning.Image = Properties.Resources.general_Warning_16;
			tooManyFramesWarning.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			tooManyFramesWarning.Location = new System.Drawing.Point(457, 145);
			tooManyFramesWarning.Name = "tooManyFramesWarning";
			tooManyFramesWarning.Size = new System.Drawing.Size(24, 223);
			tooManyFramesWarning.TabIndex = 19;
			tooManyFramesWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			tooManyFramesWarning.Visible = false;
			// 
			// previewProgressBar
			// 
			previewProgressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			previewProgressBar.Location = new System.Drawing.Point(346, 587);
			previewProgressBar.Maximum = 0;
			previewProgressBar.Name = "previewProgressBar";
			previewProgressBar.Size = new System.Drawing.Size(135, 23);
			previewProgressBar.Style = ProgressBarStyle.Continuous;
			previewProgressBar.TabIndex = 18;
			previewProgressBar.TextMode = DarkProgressBarMode.XOfN;
			// 
			// texturesDataGridView
			// 
			texturesDataGridView.AllowUserToAddRows = false;
			texturesDataGridView.AllowUserToPasteCells = false;
			texturesDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			texturesDataGridView.AutoGenerateColumns = false;
			texturesDataGridView.ColumnHeadersHeight = 17;
			texturesDataGridView.Columns.AddRange(new DataGridViewColumn[] { texturesDataGridViewColumnImage, texturesDataGridViewColumnRepeat, texturesDataGridViewColumnTexture, texturesDataGridViewColumnArea, texturesDataGridViewColumnTexCoord0, texturesDataGridViewColumnTexCoord1, texturesDataGridViewColumnTexCoord2, texturesDataGridViewColumnTexCoord3 });
			texturesDataGridView.Enabled = false;
			texturesDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(220, 220, 220);
			texturesDataGridView.Location = new System.Drawing.Point(4, 55);
			texturesDataGridView.Name = "texturesDataGridView";
			texturesDataGridView.RowHeadersWidth = 41;
			texturesDataGridView.RowTemplate.Height = 48;
			texturesDataGridView.Size = new System.Drawing.Size(447, 373);
			texturesDataGridView.TabIndex = 5;
			texturesDataGridView.CellDoubleClick += texturesDataGridView_CellDoubleClick;
			texturesDataGridView.CellFormatting += texturesDataGridView_CellFormatting;
			texturesDataGridView.CellParsing += texturesDataGridView_CellParsing;
			texturesDataGridView.CellValidating += texturesDataGridView_CellValidating;
			texturesDataGridView.SelectionChanged += texturesDataGridView_SelectionChanged;
			// 
			// texturesDataGridViewColumnImage
			// 
			texturesDataGridViewColumnImage.HeaderText = "Image";
			texturesDataGridViewColumnImage.ImageLayout = DataGridViewImageCellLayout.Zoom;
			texturesDataGridViewColumnImage.Name = "texturesDataGridViewColumnImage";
			texturesDataGridViewColumnImage.ReadOnly = true;
			texturesDataGridViewColumnImage.Width = 48;
			// 
			// texturesDataGridViewColumnRepeat
			// 
			texturesDataGridViewColumnRepeat.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
			texturesDataGridViewColumnRepeat.DataPropertyName = "Repeat";
			texturesDataGridViewColumnRepeat.HeaderText = "Repeat";
			texturesDataGridViewColumnRepeat.Name = "texturesDataGridViewColumnRepeat";
			texturesDataGridViewColumnRepeat.Width = 67;
			// 
			// texturesDataGridViewColumnTexture
			// 
			texturesDataGridViewColumnTexture.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			texturesDataGridViewColumnTexture.DataPropertyName = "Texture";
			texturesDataGridViewColumnTexture.HeaderText = "Texture";
			texturesDataGridViewColumnTexture.Name = "texturesDataGridViewColumnTexture";
			texturesDataGridViewColumnTexture.Resizable = DataGridViewTriState.True;
			// 
			// texturesDataGridViewColumnArea
			// 
			texturesDataGridViewColumnArea.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			texturesDataGridViewColumnArea.HeaderText = "Area";
			texturesDataGridViewColumnArea.Name = "texturesDataGridViewColumnArea";
			texturesDataGridViewColumnArea.ReadOnly = true;
			texturesDataGridViewColumnArea.Width = 54;
			// 
			// texturesDataGridViewColumnTexCoord0
			// 
			texturesDataGridViewColumnTexCoord0.DataPropertyName = "TexCoord0";
			texturesDataGridViewColumnTexCoord0.HeaderText = "Edge 0";
			texturesDataGridViewColumnTexCoord0.Name = "texturesDataGridViewColumnTexCoord0";
			texturesDataGridViewColumnTexCoord0.ReadOnly = true;
			texturesDataGridViewColumnTexCoord0.Width = 70;
			// 
			// texturesDataGridViewColumnTexCoord1
			// 
			texturesDataGridViewColumnTexCoord1.DataPropertyName = "TexCoord1";
			texturesDataGridViewColumnTexCoord1.HeaderText = "Edge 1";
			texturesDataGridViewColumnTexCoord1.Name = "texturesDataGridViewColumnTexCoord1";
			texturesDataGridViewColumnTexCoord1.ReadOnly = true;
			texturesDataGridViewColumnTexCoord1.Width = 70;
			// 
			// texturesDataGridViewColumnTexCoord2
			// 
			texturesDataGridViewColumnTexCoord2.DataPropertyName = "TexCoord2";
			texturesDataGridViewColumnTexCoord2.HeaderText = "Edge 2";
			texturesDataGridViewColumnTexCoord2.Name = "texturesDataGridViewColumnTexCoord2";
			texturesDataGridViewColumnTexCoord2.ReadOnly = true;
			texturesDataGridViewColumnTexCoord2.Width = 70;
			// 
			// texturesDataGridViewColumnTexCoord3
			// 
			texturesDataGridViewColumnTexCoord3.DataPropertyName = "TexCoord3";
			texturesDataGridViewColumnTexCoord3.HeaderText = "Edge 3";
			texturesDataGridViewColumnTexCoord3.Name = "texturesDataGridViewColumnTexCoord3";
			texturesDataGridViewColumnTexCoord3.ReadOnly = true;
			texturesDataGridViewColumnTexCoord3.Width = 70;
			// 
			// lblHeaderNgSettings
			// 
			lblHeaderNgSettings.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			lblHeaderNgSettings.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblHeaderNgSettings.Location = new System.Drawing.Point(1, 537);
			lblHeaderNgSettings.Name = "lblHeaderNgSettings";
			lblHeaderNgSettings.Size = new System.Drawing.Size(167, 18);
			lblHeaderNgSettings.TabIndex = 10;
			lblHeaderNgSettings.Text = "Settings";
			lblHeaderNgSettings.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// lblPreview
			// 
			lblPreview.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			lblPreview.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblPreview.Location = new System.Drawing.Point(343, 431);
			lblPreview.Name = "lblPreview";
			lblPreview.Size = new System.Drawing.Size(133, 18);
			lblPreview.TabIndex = 11;
			lblPreview.Text = "Preview";
			lblPreview.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// texturesDataGridViewControls
			// 
			texturesDataGridViewControls.AlwaysInsertAtZero = false;
			texturesDataGridViewControls.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
			texturesDataGridViewControls.Enabled = false;
			texturesDataGridViewControls.Location = new System.Drawing.Point(457, 55);
			texturesDataGridViewControls.Margin = new Padding(4, 3, 4, 3);
			texturesDataGridViewControls.MinimumSize = new System.Drawing.Size(24, 100);
			texturesDataGridViewControls.Name = "texturesDataGridViewControls";
			texturesDataGridViewControls.Size = new System.Drawing.Size(24, 373);
			texturesDataGridViewControls.TabIndex = 6;
			// 
			// butAnimatedTextureSetDelete
			// 
			butAnimatedTextureSetDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			butAnimatedTextureSetDelete.Checked = false;
			butAnimatedTextureSetDelete.Enabled = false;
			butAnimatedTextureSetDelete.Image = Properties.Resources.general_trash_16;
			butAnimatedTextureSetDelete.ImagePadding = 3;
			butAnimatedTextureSetDelete.Location = new System.Drawing.Point(457, 5);
			butAnimatedTextureSetDelete.Margin = new Padding(3, 0, 0, 0);
			butAnimatedTextureSetDelete.Name = "butAnimatedTextureSetDelete";
			butAnimatedTextureSetDelete.Size = new System.Drawing.Size(24, 24);
			butAnimatedTextureSetDelete.TabIndex = 4;
			butAnimatedTextureSetDelete.Click += butAnimatedTextureSetDelete_Click;
			// 
			// butAnimatedTextureSetNew
			// 
			butAnimatedTextureSetNew.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			butAnimatedTextureSetNew.Checked = false;
			butAnimatedTextureSetNew.Image = Properties.Resources.general_plus_math_16;
			butAnimatedTextureSetNew.ImagePadding = 4;
			butAnimatedTextureSetNew.Location = new System.Drawing.Point(428, 5);
			butAnimatedTextureSetNew.Margin = new Padding(0, 0, 2, 0);
			butAnimatedTextureSetNew.Name = "butAnimatedTextureSetNew";
			butAnimatedTextureSetNew.Size = new System.Drawing.Size(24, 24);
			butAnimatedTextureSetNew.TabIndex = 3;
			butAnimatedTextureSetNew.Click += butAnimatedTextureSetNew_Click;
			// 
			// comboAnimatedTextureSets
			// 
			comboAnimatedTextureSets.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			comboAnimatedTextureSets.ItemHeight = 18;
			comboAnimatedTextureSets.Location = new System.Drawing.Point(88, 5);
			comboAnimatedTextureSets.Name = "comboAnimatedTextureSets";
			comboAnimatedTextureSets.Size = new System.Drawing.Size(305, 24);
			comboAnimatedTextureSets.TabIndex = 1;
			comboAnimatedTextureSets.SelectedIndexChanged += comboAnimatedTextureSets_SelectedIndexChanged;
			// 
			// butOk
			// 
			butOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			butOk.Checked = false;
			butOk.DialogResult = DialogResult.Cancel;
			butOk.Location = new System.Drawing.Point(691, 625);
			butOk.Name = "butOk";
			butOk.Size = new System.Drawing.Size(80, 23);
			butOk.TabIndex = 8;
			butOk.Text = "OK";
			butOk.TextImageRelation = TextImageRelation.ImageBeforeText;
			butOk.Click += butOk_Click;
			// 
			// toolTip
			// 
			toolTip.AutomaticDelay = 100;
			toolTip.AutoPopDelay = 30000;
			toolTip.InitialDelay = 100;
			toolTip.ReshowDelay = 20;
			// 
			// butCancel
			// 
			butCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			butCancel.Checked = false;
			butCancel.DialogResult = DialogResult.Cancel;
			butCancel.Location = new System.Drawing.Point(777, 625);
			butCancel.Name = "butCancel";
			butCancel.Size = new System.Drawing.Size(80, 23);
			butCancel.TabIndex = 44;
			butCancel.Text = "Cancel";
			butCancel.TextImageRelation = TextImageRelation.ImageBeforeText;
			butCancel.Click += butCancel_Click;
			// 
			// FormAnimatedTextures
			// 
			AcceptButton = butOk;
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = butCancel;
			ClientSize = new System.Drawing.Size(869, 656);
			Controls.Add(butCancel);
			Controls.Add(tableLayoutPanel1);
			Controls.Add(butOk);
			DoubleBuffered = true;
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(691, 500);
			Name = "FormAnimatedTextures";
			ShowIcon = false;
			ShowInTaskbar = false;
			SizeGripStyle = SizeGripStyle.Hide;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Animated textures";
			tableLayoutPanel1.ResumeLayout(false);
			panel2.ResumeLayout(false);
			panel1.ResumeLayout(false);
			darkPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)previewImage).EndInit();
			panel4.ResumeLayout(false);
			settingsPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)numericUpDownUvRotateDirection).EndInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownFPS).EndInit();
			panel3.ResumeLayout(false);
			panel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)numFrames).EndInit();
			((System.ComponentModel.ISupportInitialize)numStrength).EndInit();
			((System.ComponentModel.ISupportInitialize)texturesDataGridView).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private DarkLabel darkLabel1;
        private DarkComboBox comboAnimatedTextureSets;
        private DarkButton butAnimatedTextureSetDelete;
        private DarkButton butAnimatedTextureSetNew;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private DarkButton butOk;
        private DarkLabel tooManyFramesWarning;
        private DarkProgressBar previewProgressBar;
        private DarkDataGridView texturesDataGridView;
        private DarkLabel lblHeaderNgSettings;
        private DarkLabel lblPreview;
        private Panel settingsPanel;
        private DarkLabel lblEffect;
        private DarkComboBox comboEffect;
        private PictureBox previewImage;
        private TombLib.Controls.DarkDataGridViewControls texturesDataGridViewControls;
        private DarkButton butUpdate;
        private ToolTip toolTip;
        private System.ComponentModel.IContainer components;
        private DarkLabel lblUvRotate;
        private DarkComboBox comboUvRotate;
        private DarkLabel lblFps;
        private DarkComboBox comboFps;
        private Panel panel2;
        private TombLib.Controls.DarkSearchableComboBox comboCurrentTexture;
        private DarkLabel lblProcAnim;
        private DarkPanel panel3;
        private DarkComboBox comboProcPresets;
        private DarkButton butGenerateProcAnim;
        private DarkNumericUpDown numStrength;
        private DarkLabel lblAnimatorName;
        private DarkLabel lblProgress;
        private DarkNumericUpDown numFrames;
        private DarkLabel lblFrames;
        private DarkCheckBox cbSmooth;
        private DarkCheckBox cbLoop;
        private DarkButton butEditSetName;
        private DarkButton butMergeProcAnim;
        private DarkButton butCloneProcAnim;
        private DarkLabel label1;
        private DarkButton butReplaceProcAnim;
        private DarkButton butAddProcAnim;
        private DarkNumericUpDown numericUpDownFPS;
        private DarkPanel panel4;
        private DataGridViewImageColumn texturesDataGridViewColumnImage;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnRepeat;
        private DarkDataGridViewComboBoxColumn texturesDataGridViewColumnTexture;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnArea;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnTexCoord0;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnTexCoord1;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnTexCoord2;
        private DataGridViewTextBoxColumn texturesDataGridViewColumnTexCoord3;
        private DarkButton butCancel;
        private DarkPanel darkPanel1;
		private Panel panelTextureMapContainer;
		private DarkNumericUpDown numericUpDownUvRotateDirection;
	}
}