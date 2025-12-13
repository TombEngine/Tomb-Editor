namespace TombLib.Forms
{
    partial class FormMaterialEditor
	{
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMaterialEditor));
			butOK = new DarkUI.Controls.DarkButton();
			butCancel = new DarkUI.Controls.DarkButton();
			panelSky = new System.Windows.Forms.Panel();
			tbColorMapPath = new DarkUI.Controls.DarkTextBox();
			picPreviewColorMap = new System.Windows.Forms.PictureBox();
			darkLabel9 = new DarkUI.Controls.DarkLabel();
			panel1 = new System.Windows.Forms.Panel();
			butClearSpecularMap = new DarkUI.Controls.DarkButton();
			butBrowseSpecularMap = new DarkUI.Controls.DarkButton();
			tbSpecularMapPath = new DarkUI.Controls.DarkTextBox();
			picPreviewSpecularMap = new System.Windows.Forms.PictureBox();
			darkLabel1 = new DarkUI.Controls.DarkLabel();
			panel2 = new System.Windows.Forms.Panel();
			butClearNormalMap = new DarkUI.Controls.DarkButton();
			butBrowseNormalMap = new DarkUI.Controls.DarkButton();
			tbNormalMapPath = new DarkUI.Controls.DarkTextBox();
			picPreviewNormalMap = new System.Windows.Forms.PictureBox();
			darkLabel2 = new DarkUI.Controls.DarkLabel();
			panel3 = new System.Windows.Forms.Panel();
			butClearAmbientOcclusionMap = new DarkUI.Controls.DarkButton();
			butBrowseAmbientOcclusionMap = new DarkUI.Controls.DarkButton();
			tbAmbientOcclusionMapPath = new DarkUI.Controls.DarkTextBox();
			picPreviewAmbientOcclusionMap = new System.Windows.Forms.PictureBox();
			darkLabel3 = new DarkUI.Controls.DarkLabel();
			panel4 = new System.Windows.Forms.Panel();
			butClearEmissiveMap = new DarkUI.Controls.DarkButton();
			butBrowseEmissiveMap = new DarkUI.Controls.DarkButton();
			tbEmissiveMapPath = new DarkUI.Controls.DarkTextBox();
			picPreviewEmissiveMap = new System.Windows.Forms.PictureBox();
			darkLabel4 = new DarkUI.Controls.DarkLabel();
			darkLabel5 = new DarkUI.Controls.DarkLabel();
			tabcontainerParameters = new Controls.DarkTabbedContainer();
			tabPage1 = new System.Windows.Forms.TabPage();
			nmSpecularIntensity = new DarkUI.Controls.DarkNumericUpDown();
			darkLabel6 = new DarkUI.Controls.DarkLabel();
			nmNormalMapStrength = new DarkUI.Controls.DarkNumericUpDown();
			lblScale = new DarkUI.Controls.DarkLabel();
			tabPage2 = new System.Windows.Forms.TabPage();
			statusStrip = new DarkUI.Controls.DarkStatusStrip();
			lblResult = new System.Windows.Forms.ToolStripStatusLabel();
			lblXmlMaterialFile = new System.Windows.Forms.ToolStripStatusLabel();
			panel5 = new System.Windows.Forms.Panel();
			butClearRoughnessMap = new DarkUI.Controls.DarkButton();
			butBrowseRoughnessMap = new DarkUI.Controls.DarkButton();
			tbRoughnessMapPath = new DarkUI.Controls.DarkTextBox();
			picPreviewRoughnessMap = new System.Windows.Forms.PictureBox();
			darkLabel7 = new DarkUI.Controls.DarkLabel();
			panelTextureSelect = new DarkUI.Controls.DarkPanel();
			comboTexture = new Controls.DarkSearchableComboBox();
			darkLabel8 = new DarkUI.Controls.DarkLabel();
			comboMaterialType = new DarkUI.Controls.DarkComboBox();
			panel6 = new System.Windows.Forms.Panel();
			butClearHeightMap = new DarkUI.Controls.DarkButton();
			butBrowseHeightMap = new DarkUI.Controls.DarkButton();
			tbHeightMapPath = new DarkUI.Controls.DarkTextBox();
			picPreviewHeightMap = new System.Windows.Forms.PictureBox();
			darkLabel10 = new DarkUI.Controls.DarkLabel();
			panelSky.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewColorMap).BeginInit();
			panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewSpecularMap).BeginInit();
			panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewNormalMap).BeginInit();
			panel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewAmbientOcclusionMap).BeginInit();
			panel4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewEmissiveMap).BeginInit();
			tabcontainerParameters.SuspendLayout();
			tabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)nmSpecularIntensity).BeginInit();
			((System.ComponentModel.ISupportInitialize)nmNormalMapStrength).BeginInit();
			statusStrip.SuspendLayout();
			panel5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewRoughnessMap).BeginInit();
			panelTextureSelect.SuspendLayout();
			panel6.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewHeightMap).BeginInit();
			SuspendLayout();
			// 
			// butOK
			// 
			butOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butOK.Checked = false;
			butOK.Location = new System.Drawing.Point(349, 458);
			butOK.Name = "butOK";
			butOK.Size = new System.Drawing.Size(80, 23);
			butOK.TabIndex = 16;
			butOK.Text = "OK";
			butOK.Click += butOK_Click;
			// 
			// butCancel
			// 
			butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butCancel.Checked = false;
			butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			butCancel.Location = new System.Drawing.Point(435, 458);
			butCancel.Name = "butCancel";
			butCancel.Size = new System.Drawing.Size(80, 23);
			butCancel.TabIndex = 17;
			butCancel.Text = "Cancel";
			butCancel.Click += butCancel_Click;
			// 
			// panelSky
			// 
			panelSky.Controls.Add(tbColorMapPath);
			panelSky.Controls.Add(picPreviewColorMap);
			panelSky.Controls.Add(darkLabel9);
			panelSky.Dock = System.Windows.Forms.DockStyle.Top;
			panelSky.Location = new System.Drawing.Point(6, 57);
			panelSky.Name = "panelSky";
			panelSky.Size = new System.Drawing.Size(509, 51);
			panelSky.TabIndex = 18;
			// 
			// tbColorMapPath
			// 
			tbColorMapPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbColorMapPath.Location = new System.Drawing.Point(0, 20);
			tbColorMapPath.Name = "tbColorMapPath";
			tbColorMapPath.ReadOnly = true;
			tbColorMapPath.Size = new System.Drawing.Size(462, 22);
			tbColorMapPath.TabIndex = 8;
			// 
			// picPreviewColorMap
			// 
			picPreviewColorMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			picPreviewColorMap.BackColor = System.Drawing.Color.Gray;
			picPreviewColorMap.BackgroundImage = (System.Drawing.Image)resources.GetObject("picPreviewColorMap.BackgroundImage");
			picPreviewColorMap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			picPreviewColorMap.Location = new System.Drawing.Point(468, 3);
			picPreviewColorMap.Name = "picPreviewColorMap";
			picPreviewColorMap.Size = new System.Drawing.Size(41, 39);
			picPreviewColorMap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			picPreviewColorMap.TabIndex = 7;
			picPreviewColorMap.TabStop = false;
			// 
			// darkLabel9
			// 
			darkLabel9.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel9.Location = new System.Drawing.Point(0, 0);
			darkLabel9.Name = "darkLabel9";
			darkLabel9.Size = new System.Drawing.Size(381, 17);
			darkLabel9.TabIndex = 1;
			darkLabel9.Text = "Color map:";
			// 
			// panel1
			// 
			panel1.Controls.Add(butClearSpecularMap);
			panel1.Controls.Add(butBrowseSpecularMap);
			panel1.Controls.Add(tbSpecularMapPath);
			panel1.Controls.Add(picPreviewSpecularMap);
			panel1.Controls.Add(darkLabel1);
			panel1.Dock = System.Windows.Forms.DockStyle.Top;
			panel1.Location = new System.Drawing.Point(6, 312);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(509, 51);
			panel1.TabIndex = 19;
			// 
			// butClearSpecularMap
			// 
			butClearSpecularMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butClearSpecularMap.Checked = false;
			butClearSpecularMap.Location = new System.Drawing.Point(408, 20);
			butClearSpecularMap.Name = "butClearSpecularMap";
			butClearSpecularMap.Size = new System.Drawing.Size(54, 22);
			butClearSpecularMap.TabIndex = 11;
			butClearSpecularMap.Text = "Clear";
			butClearSpecularMap.Click += butClearSpecularMap_Click;
			// 
			// butBrowseSpecularMap
			// 
			butBrowseSpecularMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butBrowseSpecularMap.Checked = false;
			butBrowseSpecularMap.Location = new System.Drawing.Point(332, 20);
			butBrowseSpecularMap.Name = "butBrowseSpecularMap";
			butBrowseSpecularMap.Size = new System.Drawing.Size(70, 22);
			butBrowseSpecularMap.TabIndex = 9;
			butBrowseSpecularMap.Text = "Browse";
			butBrowseSpecularMap.Click += butBrowseSpecularMap_Click;
			// 
			// tbSpecularMapPath
			// 
			tbSpecularMapPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbSpecularMapPath.Location = new System.Drawing.Point(0, 20);
			tbSpecularMapPath.Name = "tbSpecularMapPath";
			tbSpecularMapPath.ReadOnly = true;
			tbSpecularMapPath.Size = new System.Drawing.Size(326, 22);
			tbSpecularMapPath.TabIndex = 8;
			// 
			// picPreviewSpecularMap
			// 
			picPreviewSpecularMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			picPreviewSpecularMap.BackColor = System.Drawing.Color.Gray;
			picPreviewSpecularMap.BackgroundImage = (System.Drawing.Image)resources.GetObject("picPreviewSpecularMap.BackgroundImage");
			picPreviewSpecularMap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			picPreviewSpecularMap.Location = new System.Drawing.Point(468, 3);
			picPreviewSpecularMap.Name = "picPreviewSpecularMap";
			picPreviewSpecularMap.Size = new System.Drawing.Size(41, 39);
			picPreviewSpecularMap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			picPreviewSpecularMap.TabIndex = 7;
			picPreviewSpecularMap.TabStop = false;
			// 
			// darkLabel1
			// 
			darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel1.Location = new System.Drawing.Point(0, 0);
			darkLabel1.Name = "darkLabel1";
			darkLabel1.Size = new System.Drawing.Size(381, 17);
			darkLabel1.TabIndex = 1;
			darkLabel1.Text = "Specular map:";
			// 
			// panel2
			// 
			panel2.Controls.Add(butClearNormalMap);
			panel2.Controls.Add(butBrowseNormalMap);
			panel2.Controls.Add(tbNormalMapPath);
			panel2.Controls.Add(picPreviewNormalMap);
			panel2.Controls.Add(darkLabel2);
			panel2.Dock = System.Windows.Forms.DockStyle.Top;
			panel2.Location = new System.Drawing.Point(6, 108);
			panel2.Name = "panel2";
			panel2.Size = new System.Drawing.Size(509, 51);
			panel2.TabIndex = 20;
			// 
			// butClearNormalMap
			// 
			butClearNormalMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butClearNormalMap.Checked = false;
			butClearNormalMap.Location = new System.Drawing.Point(408, 20);
			butClearNormalMap.Name = "butClearNormalMap";
			butClearNormalMap.Size = new System.Drawing.Size(54, 22);
			butClearNormalMap.TabIndex = 10;
			butClearNormalMap.Text = "Clear";
			butClearNormalMap.Click += butClearNormalMap_Click;
			// 
			// butBrowseNormalMap
			// 
			butBrowseNormalMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butBrowseNormalMap.Checked = false;
			butBrowseNormalMap.Location = new System.Drawing.Point(332, 20);
			butBrowseNormalMap.Name = "butBrowseNormalMap";
			butBrowseNormalMap.Size = new System.Drawing.Size(70, 22);
			butBrowseNormalMap.TabIndex = 9;
			butBrowseNormalMap.Text = "Browse";
			butBrowseNormalMap.Click += butBrowseNormalMap_Click;
			// 
			// tbNormalMapPath
			// 
			tbNormalMapPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbNormalMapPath.Location = new System.Drawing.Point(0, 20);
			tbNormalMapPath.Name = "tbNormalMapPath";
			tbNormalMapPath.ReadOnly = true;
			tbNormalMapPath.Size = new System.Drawing.Size(326, 22);
			tbNormalMapPath.TabIndex = 8;
			// 
			// picPreviewNormalMap
			// 
			picPreviewNormalMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			picPreviewNormalMap.BackColor = System.Drawing.Color.Gray;
			picPreviewNormalMap.BackgroundImage = (System.Drawing.Image)resources.GetObject("picPreviewNormalMap.BackgroundImage");
			picPreviewNormalMap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			picPreviewNormalMap.Location = new System.Drawing.Point(468, 3);
			picPreviewNormalMap.Name = "picPreviewNormalMap";
			picPreviewNormalMap.Size = new System.Drawing.Size(41, 39);
			picPreviewNormalMap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			picPreviewNormalMap.TabIndex = 7;
			picPreviewNormalMap.TabStop = false;
			// 
			// darkLabel2
			// 
			darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel2.Location = new System.Drawing.Point(0, 0);
			darkLabel2.Name = "darkLabel2";
			darkLabel2.Size = new System.Drawing.Size(381, 17);
			darkLabel2.TabIndex = 1;
			darkLabel2.Text = "Normal map:";
			// 
			// panel3
			// 
			panel3.Controls.Add(butClearAmbientOcclusionMap);
			panel3.Controls.Add(butBrowseAmbientOcclusionMap);
			panel3.Controls.Add(tbAmbientOcclusionMapPath);
			panel3.Controls.Add(picPreviewAmbientOcclusionMap);
			panel3.Controls.Add(darkLabel3);
			panel3.Dock = System.Windows.Forms.DockStyle.Top;
			panel3.Location = new System.Drawing.Point(6, 210);
			panel3.Name = "panel3";
			panel3.Size = new System.Drawing.Size(509, 51);
			panel3.TabIndex = 21;
			// 
			// butClearAmbientOcclusionMap
			// 
			butClearAmbientOcclusionMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butClearAmbientOcclusionMap.Checked = false;
			butClearAmbientOcclusionMap.Location = new System.Drawing.Point(408, 20);
			butClearAmbientOcclusionMap.Name = "butClearAmbientOcclusionMap";
			butClearAmbientOcclusionMap.Size = new System.Drawing.Size(54, 22);
			butClearAmbientOcclusionMap.TabIndex = 11;
			butClearAmbientOcclusionMap.Text = "Clear";
			butClearAmbientOcclusionMap.Click += butClearAmbientOcclusionMap_Click;
			// 
			// butBrowseAmbientOcclusionMap
			// 
			butBrowseAmbientOcclusionMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butBrowseAmbientOcclusionMap.Checked = false;
			butBrowseAmbientOcclusionMap.Location = new System.Drawing.Point(332, 20);
			butBrowseAmbientOcclusionMap.Name = "butBrowseAmbientOcclusionMap";
			butBrowseAmbientOcclusionMap.Size = new System.Drawing.Size(70, 22);
			butBrowseAmbientOcclusionMap.TabIndex = 9;
			butBrowseAmbientOcclusionMap.Text = "Browse";
			butBrowseAmbientOcclusionMap.Click += butBrowseAmbientOcclusionMap_Click;
			// 
			// tbAmbientOcclusionMapPath
			// 
			tbAmbientOcclusionMapPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbAmbientOcclusionMapPath.Location = new System.Drawing.Point(0, 20);
			tbAmbientOcclusionMapPath.Name = "tbAmbientOcclusionMapPath";
			tbAmbientOcclusionMapPath.ReadOnly = true;
			tbAmbientOcclusionMapPath.Size = new System.Drawing.Size(326, 22);
			tbAmbientOcclusionMapPath.TabIndex = 8;
			// 
			// picPreviewAmbientOcclusionMap
			// 
			picPreviewAmbientOcclusionMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			picPreviewAmbientOcclusionMap.BackColor = System.Drawing.Color.Gray;
			picPreviewAmbientOcclusionMap.BackgroundImage = (System.Drawing.Image)resources.GetObject("picPreviewAmbientOcclusionMap.BackgroundImage");
			picPreviewAmbientOcclusionMap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			picPreviewAmbientOcclusionMap.Location = new System.Drawing.Point(468, 3);
			picPreviewAmbientOcclusionMap.Name = "picPreviewAmbientOcclusionMap";
			picPreviewAmbientOcclusionMap.Size = new System.Drawing.Size(41, 39);
			picPreviewAmbientOcclusionMap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			picPreviewAmbientOcclusionMap.TabIndex = 7;
			picPreviewAmbientOcclusionMap.TabStop = false;
			// 
			// darkLabel3
			// 
			darkLabel3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel3.Location = new System.Drawing.Point(0, 0);
			darkLabel3.Name = "darkLabel3";
			darkLabel3.Size = new System.Drawing.Size(381, 17);
			darkLabel3.TabIndex = 1;
			darkLabel3.Text = "Ambient occlusion map:";
			// 
			// panel4
			// 
			panel4.Controls.Add(butClearEmissiveMap);
			panel4.Controls.Add(butBrowseEmissiveMap);
			panel4.Controls.Add(tbEmissiveMapPath);
			panel4.Controls.Add(picPreviewEmissiveMap);
			panel4.Controls.Add(darkLabel4);
			panel4.Dock = System.Windows.Forms.DockStyle.Top;
			panel4.Location = new System.Drawing.Point(6, 261);
			panel4.Name = "panel4";
			panel4.Size = new System.Drawing.Size(509, 51);
			panel4.TabIndex = 22;
			// 
			// butClearEmissiveMap
			// 
			butClearEmissiveMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butClearEmissiveMap.Checked = false;
			butClearEmissiveMap.Location = new System.Drawing.Point(408, 20);
			butClearEmissiveMap.Name = "butClearEmissiveMap";
			butClearEmissiveMap.Size = new System.Drawing.Size(54, 22);
			butClearEmissiveMap.TabIndex = 11;
			butClearEmissiveMap.Text = "Clear";
			butClearEmissiveMap.Click += butClearEmissiveMap_Click;
			// 
			// butBrowseEmissiveMap
			// 
			butBrowseEmissiveMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butBrowseEmissiveMap.Checked = false;
			butBrowseEmissiveMap.Location = new System.Drawing.Point(332, 20);
			butBrowseEmissiveMap.Name = "butBrowseEmissiveMap";
			butBrowseEmissiveMap.Size = new System.Drawing.Size(70, 22);
			butBrowseEmissiveMap.TabIndex = 9;
			butBrowseEmissiveMap.Text = "Browse";
			butBrowseEmissiveMap.Click += butBrowseEmissiveMap_Click;
			// 
			// tbEmissiveMapPath
			// 
			tbEmissiveMapPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbEmissiveMapPath.Location = new System.Drawing.Point(0, 20);
			tbEmissiveMapPath.Name = "tbEmissiveMapPath";
			tbEmissiveMapPath.ReadOnly = true;
			tbEmissiveMapPath.Size = new System.Drawing.Size(326, 22);
			tbEmissiveMapPath.TabIndex = 8;
			// 
			// picPreviewEmissiveMap
			// 
			picPreviewEmissiveMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			picPreviewEmissiveMap.BackColor = System.Drawing.Color.Gray;
			picPreviewEmissiveMap.BackgroundImage = (System.Drawing.Image)resources.GetObject("picPreviewEmissiveMap.BackgroundImage");
			picPreviewEmissiveMap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			picPreviewEmissiveMap.Location = new System.Drawing.Point(468, 3);
			picPreviewEmissiveMap.Name = "picPreviewEmissiveMap";
			picPreviewEmissiveMap.Size = new System.Drawing.Size(41, 39);
			picPreviewEmissiveMap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			picPreviewEmissiveMap.TabIndex = 7;
			picPreviewEmissiveMap.TabStop = false;
			// 
			// darkLabel4
			// 
			darkLabel4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel4.Location = new System.Drawing.Point(0, 0);
			darkLabel4.Name = "darkLabel4";
			darkLabel4.Size = new System.Drawing.Size(381, 17);
			darkLabel4.TabIndex = 1;
			darkLabel4.Text = "Emissive map:";
			// 
			// darkLabel5
			// 
			darkLabel5.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			darkLabel5.AutoSize = true;
			darkLabel5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel5.Location = new System.Drawing.Point(6, 425);
			darkLabel5.Name = "darkLabel5";
			darkLabel5.Size = new System.Drawing.Size(77, 13);
			darkLabel5.TabIndex = 25;
			darkLabel5.Text = "Material type:";
			// 
			// tabcontainerParameters
			// 
			tabcontainerParameters.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tabcontainerParameters.Controls.Add(tabPage1);
			tabcontainerParameters.Controls.Add(tabPage2);
			tabcontainerParameters.Location = new System.Drawing.Point(9, 486);
			tabcontainerParameters.Name = "tabcontainerParameters";
			tabcontainerParameters.SelectedIndex = 0;
			tabcontainerParameters.Size = new System.Drawing.Size(506, 53);
			tabcontainerParameters.TabIndex = 26;
			tabcontainerParameters.Visible = false;
			// 
			// tabPage1
			// 
			tabPage1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabPage1.Controls.Add(nmSpecularIntensity);
			tabPage1.Controls.Add(darkLabel6);
			tabPage1.Controls.Add(nmNormalMapStrength);
			tabPage1.Controls.Add(lblScale);
			tabPage1.Location = new System.Drawing.Point(4, 22);
			tabPage1.Name = "tabPage1";
			tabPage1.Padding = new System.Windows.Forms.Padding(3);
			tabPage1.Size = new System.Drawing.Size(498, 27);
			tabPage1.TabIndex = 0;
			tabPage1.Text = "tabPage1";
			// 
			// nmSpecularIntensity
			// 
			nmSpecularIntensity.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			nmSpecularIntensity.DecimalPlaces = 4;
			nmSpecularIntensity.Increment = new decimal(new int[] { 25, 0, 0, 131072 });
			nmSpecularIntensity.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 65536 });
			nmSpecularIntensity.Location = new System.Drawing.Point(135, 31);
			nmSpecularIntensity.LoopValues = false;
			nmSpecularIntensity.Maximum = new decimal(new int[] { 2048, 0, 0, 0 });
			nmSpecularIntensity.Minimum = new decimal(new int[] { 1, 0, 0, 262144 });
			nmSpecularIntensity.Name = "nmSpecularIntensity";
			nmSpecularIntensity.Size = new System.Drawing.Size(360, 22);
			nmSpecularIntensity.TabIndex = 11;
			nmSpecularIntensity.Value = new decimal(new int[] { 1, 0, 0, 0 });
			nmSpecularIntensity.ValueChanged += nmSpecularIntensity_ValueChanged;
			// 
			// darkLabel6
			// 
			darkLabel6.AutoSize = true;
			darkLabel6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel6.Location = new System.Drawing.Point(6, 34);
			darkLabel6.Name = "darkLabel6";
			darkLabel6.Size = new System.Drawing.Size(101, 13);
			darkLabel6.TabIndex = 10;
			darkLabel6.Text = "Specular intensity:";
			// 
			// nmNormalMapStrength
			// 
			nmNormalMapStrength.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			nmNormalMapStrength.DecimalPlaces = 4;
			nmNormalMapStrength.Increment = new decimal(new int[] { 25, 0, 0, 131072 });
			nmNormalMapStrength.IncrementAlternate = new decimal(new int[] { 5, 0, 0, 65536 });
			nmNormalMapStrength.Location = new System.Drawing.Point(135, 3);
			nmNormalMapStrength.LoopValues = false;
			nmNormalMapStrength.Maximum = new decimal(new int[] { 2048, 0, 0, 0 });
			nmNormalMapStrength.Minimum = new decimal(new int[] { 1, 0, 0, 262144 });
			nmNormalMapStrength.Name = "nmNormalMapStrength";
			nmNormalMapStrength.Size = new System.Drawing.Size(360, 22);
			nmNormalMapStrength.TabIndex = 9;
			nmNormalMapStrength.Value = new decimal(new int[] { 1, 0, 0, 0 });
			nmNormalMapStrength.ValueChanged += nmNormalMapStrength_ValueChanged;
			// 
			// lblScale
			// 
			lblScale.AutoSize = true;
			lblScale.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblScale.Location = new System.Drawing.Point(6, 6);
			lblScale.Name = "lblScale";
			lblScale.Size = new System.Drawing.Size(123, 13);
			lblScale.TabIndex = 1;
			lblScale.Text = "Normap map strength:";
			// 
			// tabPage2
			// 
			tabPage2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabPage2.Location = new System.Drawing.Point(4, 22);
			tabPage2.Name = "tabPage2";
			tabPage2.Padding = new System.Windows.Forms.Padding(3);
			tabPage2.Size = new System.Drawing.Size(498, 27);
			tabPage2.TabIndex = 1;
			tabPage2.Text = "tabPage2";
			// 
			// statusStrip
			// 
			statusStrip.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			statusStrip.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblResult, lblXmlMaterialFile });
			statusStrip.Location = new System.Drawing.Point(6, 487);
			statusStrip.Name = "statusStrip";
			statusStrip.Padding = new System.Windows.Forms.Padding(2, 5, 0, 3);
			statusStrip.Size = new System.Drawing.Size(509, 28);
			statusStrip.TabIndex = 29;
			// 
			// lblResult
			// 
			lblResult.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			lblResult.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			lblResult.ForeColor = System.Drawing.Color.Silver;
			lblResult.Name = "lblResult";
			lblResult.Size = new System.Drawing.Size(0, 15);
			lblResult.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblXmlMaterialFile
			// 
			lblXmlMaterialFile.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			lblXmlMaterialFile.ForeColor = System.Drawing.Color.Silver;
			lblXmlMaterialFile.Name = "lblXmlMaterialFile";
			lblXmlMaterialFile.Size = new System.Drawing.Size(0, 15);
			// 
			// panel5
			// 
			panel5.Controls.Add(butClearRoughnessMap);
			panel5.Controls.Add(butBrowseRoughnessMap);
			panel5.Controls.Add(tbRoughnessMapPath);
			panel5.Controls.Add(picPreviewRoughnessMap);
			panel5.Controls.Add(darkLabel7);
			panel5.Dock = System.Windows.Forms.DockStyle.Top;
			panel5.Location = new System.Drawing.Point(6, 363);
			panel5.Name = "panel5";
			panel5.Size = new System.Drawing.Size(509, 51);
			panel5.TabIndex = 30;
			// 
			// butClearRoughnessMap
			// 
			butClearRoughnessMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butClearRoughnessMap.Checked = false;
			butClearRoughnessMap.Location = new System.Drawing.Point(408, 20);
			butClearRoughnessMap.Name = "butClearRoughnessMap";
			butClearRoughnessMap.Size = new System.Drawing.Size(54, 22);
			butClearRoughnessMap.TabIndex = 11;
			butClearRoughnessMap.Text = "Clear";
			butClearRoughnessMap.Click += butClearRoughnessMap_Click;
			// 
			// butBrowseRoughnessMap
			// 
			butBrowseRoughnessMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butBrowseRoughnessMap.Checked = false;
			butBrowseRoughnessMap.Location = new System.Drawing.Point(332, 20);
			butBrowseRoughnessMap.Name = "butBrowseRoughnessMap";
			butBrowseRoughnessMap.Size = new System.Drawing.Size(70, 22);
			butBrowseRoughnessMap.TabIndex = 9;
			butBrowseRoughnessMap.Text = "Browse";
			butBrowseRoughnessMap.Click += butBrowseRoughnessMap_Click;
			// 
			// tbRoughnessMapPath
			// 
			tbRoughnessMapPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbRoughnessMapPath.Location = new System.Drawing.Point(0, 20);
			tbRoughnessMapPath.Name = "tbRoughnessMapPath";
			tbRoughnessMapPath.ReadOnly = true;
			tbRoughnessMapPath.Size = new System.Drawing.Size(326, 22);
			tbRoughnessMapPath.TabIndex = 8;
			// 
			// picPreviewRoughnessMap
			// 
			picPreviewRoughnessMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			picPreviewRoughnessMap.BackColor = System.Drawing.Color.Gray;
			picPreviewRoughnessMap.BackgroundImage = (System.Drawing.Image)resources.GetObject("picPreviewRoughnessMap.BackgroundImage");
			picPreviewRoughnessMap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			picPreviewRoughnessMap.Location = new System.Drawing.Point(468, 3);
			picPreviewRoughnessMap.Name = "picPreviewRoughnessMap";
			picPreviewRoughnessMap.Size = new System.Drawing.Size(41, 39);
			picPreviewRoughnessMap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			picPreviewRoughnessMap.TabIndex = 7;
			picPreviewRoughnessMap.TabStop = false;
			// 
			// darkLabel7
			// 
			darkLabel7.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel7.Location = new System.Drawing.Point(0, 0);
			darkLabel7.Name = "darkLabel7";
			darkLabel7.Size = new System.Drawing.Size(381, 17);
			darkLabel7.TabIndex = 1;
			darkLabel7.Text = "Roughness map:";
			// 
			// panelTextureSelect
			// 
			panelTextureSelect.Controls.Add(comboTexture);
			panelTextureSelect.Controls.Add(darkLabel8);
			panelTextureSelect.Dock = System.Windows.Forms.DockStyle.Top;
			panelTextureSelect.Location = new System.Drawing.Point(6, 6);
			panelTextureSelect.Name = "panelTextureSelect";
			panelTextureSelect.Size = new System.Drawing.Size(509, 51);
			panelTextureSelect.TabIndex = 9;
			// 
			// comboTexture
			// 
			comboTexture.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			comboTexture.Location = new System.Drawing.Point(0, 23);
			comboTexture.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
			comboTexture.Name = "comboTexture";
			comboTexture.Size = new System.Drawing.Size(509, 23);
			comboTexture.TabIndex = 3;
			comboTexture.SelectedIndexChanged += comboTexture_SelectedIndexChanged;
			// 
			// darkLabel8
			// 
			darkLabel8.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel8.Location = new System.Drawing.Point(0, 0);
			darkLabel8.Name = "darkLabel8";
			darkLabel8.Size = new System.Drawing.Size(51, 17);
			darkLabel8.TabIndex = 2;
			darkLabel8.Text = "Texture:";
			// 
			// comboMaterialType
			// 
			comboMaterialType.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			comboMaterialType.FormattingEnabled = true;
			comboMaterialType.Location = new System.Drawing.Point(89, 422);
			comboMaterialType.Name = "comboMaterialType";
			comboMaterialType.Size = new System.Drawing.Size(426, 23);
			comboMaterialType.TabIndex = 31;
			comboMaterialType.SelectedIndexChanged += comboMaterialType_SelectedIndexChanged;
			// 
			// panel6
			// 
			panel6.Controls.Add(butClearHeightMap);
			panel6.Controls.Add(butBrowseHeightMap);
			panel6.Controls.Add(tbHeightMapPath);
			panel6.Controls.Add(picPreviewHeightMap);
			panel6.Controls.Add(darkLabel10);
			panel6.Dock = System.Windows.Forms.DockStyle.Top;
			panel6.Location = new System.Drawing.Point(6, 159);
			panel6.Name = "panel6";
			panel6.Size = new System.Drawing.Size(509, 51);
			panel6.TabIndex = 32;
			// 
			// butClearHeightMap
			// 
			butClearHeightMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butClearHeightMap.Checked = false;
			butClearHeightMap.Location = new System.Drawing.Point(408, 20);
			butClearHeightMap.Name = "butClearHeightMap";
			butClearHeightMap.Size = new System.Drawing.Size(54, 22);
			butClearHeightMap.TabIndex = 11;
			butClearHeightMap.Text = "Clear";
			butClearHeightMap.Click += butClearHeightMap_Click;
			// 
			// butBrowseHeightMap
			// 
			butBrowseHeightMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butBrowseHeightMap.Checked = false;
			butBrowseHeightMap.Location = new System.Drawing.Point(332, 20);
			butBrowseHeightMap.Name = "butBrowseHeightMap";
			butBrowseHeightMap.Size = new System.Drawing.Size(70, 22);
			butBrowseHeightMap.TabIndex = 9;
			butBrowseHeightMap.Text = "Browse";
			butBrowseHeightMap.Click += butBrowseHeightMap_Click;
			// 
			// tbHeightMapPath
			// 
			tbHeightMapPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbHeightMapPath.Location = new System.Drawing.Point(0, 20);
			tbHeightMapPath.Name = "tbHeightMapPath";
			tbHeightMapPath.ReadOnly = true;
			tbHeightMapPath.Size = new System.Drawing.Size(326, 22);
			tbHeightMapPath.TabIndex = 8;
			// 
			// picPreviewHeightMap
			// 
			picPreviewHeightMap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			picPreviewHeightMap.BackColor = System.Drawing.Color.Gray;
			picPreviewHeightMap.BackgroundImage = (System.Drawing.Image)resources.GetObject("picPreviewHeightMap.BackgroundImage");
			picPreviewHeightMap.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			picPreviewHeightMap.Location = new System.Drawing.Point(468, 3);
			picPreviewHeightMap.Name = "picPreviewHeightMap";
			picPreviewHeightMap.Size = new System.Drawing.Size(41, 39);
			picPreviewHeightMap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			picPreviewHeightMap.TabIndex = 7;
			picPreviewHeightMap.TabStop = false;
			// 
			// darkLabel10
			// 
			darkLabel10.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel10.Location = new System.Drawing.Point(0, 0);
			darkLabel10.Name = "darkLabel10";
			darkLabel10.Size = new System.Drawing.Size(381, 17);
			darkLabel10.TabIndex = 1;
			darkLabel10.Text = "Height map:";
			// 
			// FormMaterialEditor
			// 
			AcceptButton = butOK;
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			CancelButton = butCancel;
			ClientSize = new System.Drawing.Size(521, 521);
			Controls.Add(comboMaterialType);
			Controls.Add(panel5);
			Controls.Add(butCancel);
			Controls.Add(butOK);
			Controls.Add(panel1);
			Controls.Add(statusStrip);
			Controls.Add(tabcontainerParameters);
			Controls.Add(darkLabel5);
			Controls.Add(panel4);
			Controls.Add(panel3);
			Controls.Add(panel6);
			Controls.Add(panel2);
			Controls.Add(panelSky);
			Controls.Add(panelTextureSelect);
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			MaximizeBox = false;
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(537, 560);
			Name = "FormMaterialEditor";
			Padding = new System.Windows.Forms.Padding(6);
			ShowIcon = false;
			ShowInTaskbar = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Material editor";
			panelSky.ResumeLayout(false);
			panelSky.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewColorMap).EndInit();
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewSpecularMap).EndInit();
			panel2.ResumeLayout(false);
			panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewNormalMap).EndInit();
			panel3.ResumeLayout(false);
			panel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewAmbientOcclusionMap).EndInit();
			panel4.ResumeLayout(false);
			panel4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewEmissiveMap).EndInit();
			tabcontainerParameters.ResumeLayout(false);
			tabPage1.ResumeLayout(false);
			tabPage1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)nmSpecularIntensity).EndInit();
			((System.ComponentModel.ISupportInitialize)nmNormalMapStrength).EndInit();
			statusStrip.ResumeLayout(false);
			statusStrip.PerformLayout();
			panel5.ResumeLayout(false);
			panel5.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewRoughnessMap).EndInit();
			panelTextureSelect.ResumeLayout(false);
			panel6.ResumeLayout(false);
			panel6.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picPreviewHeightMap).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private DarkUI.Controls.DarkButton butOK;
        private DarkUI.Controls.DarkButton butCancel;
		private System.Windows.Forms.Panel panelSky;
		private DarkUI.Controls.DarkLabel darkLabel9;
		private System.Windows.Forms.PictureBox picPreviewColorMap;
		private DarkUI.Controls.DarkTextBox tbColorMapPath;
		private System.Windows.Forms.Panel panel1;
		private DarkUI.Controls.DarkButton butBrowseSpecularMap;
		private DarkUI.Controls.DarkTextBox tbSpecularMapPath;
		private System.Windows.Forms.PictureBox picPreviewSpecularMap;
		private DarkUI.Controls.DarkLabel darkLabel1;
		private System.Windows.Forms.Panel panel2;
		private DarkUI.Controls.DarkButton butBrowseNormalMap;
		private DarkUI.Controls.DarkTextBox tbNormalMapPath;
		private System.Windows.Forms.PictureBox picPreviewNormalMap;
		private DarkUI.Controls.DarkLabel darkLabel2;
		private System.Windows.Forms.Panel panel3;
		private DarkUI.Controls.DarkButton butBrowseAmbientOcclusionMap;
		private DarkUI.Controls.DarkTextBox tbAmbientOcclusionMapPath;
		private System.Windows.Forms.PictureBox picPreviewAmbientOcclusionMap;
		private DarkUI.Controls.DarkLabel darkLabel3;
		private System.Windows.Forms.Panel panel4;
		private DarkUI.Controls.DarkButton butBrowseEmissiveMap;
		private DarkUI.Controls.DarkTextBox tbEmissiveMapPath;
		private System.Windows.Forms.PictureBox picPreviewEmissiveMap;
		private DarkUI.Controls.DarkLabel darkLabel4;
		private DarkUI.Controls.DarkLabel darkLabel5;
		private Controls.DarkTabbedContainer tabcontainerParameters;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private DarkUI.Controls.DarkLabel lblScale;
		private DarkUI.Controls.DarkNumericUpDown nmSpecularIntensity;
		private DarkUI.Controls.DarkLabel darkLabel6;
		private DarkUI.Controls.DarkNumericUpDown nmNormalMapStrength;
		private DarkUI.Controls.DarkLabel AAA;
		private DarkUI.Controls.DarkButton butClearSpecularMap;
		private DarkUI.Controls.DarkButton butClearNormalMap;
		private DarkUI.Controls.DarkButton butClearAmbientOcclusionMap;
		private DarkUI.Controls.DarkButton butClearEmissiveMap;
        private DarkUI.Controls.DarkStatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblResult;
        private System.Windows.Forms.ToolStripStatusLabel lblXmlMaterialFile;
		private System.Windows.Forms.Panel panel5;
		private DarkUI.Controls.DarkButton butClearRoughnessMap;
		private DarkUI.Controls.DarkButton butBrowseRoughnessMap;
		private DarkUI.Controls.DarkTextBox tbRoughnessMapPath;
		private System.Windows.Forms.PictureBox picPreviewRoughnessMap;
		private DarkUI.Controls.DarkLabel darkLabel7;
		private DarkUI.Controls.DarkPanel panelTextureSelect;
		private DarkUI.Controls.DarkLabel darkLabel8;
		private TombLib.Controls.DarkSearchableComboBox comboTexture;
        private DarkUI.Controls.DarkComboBox comboMaterialType;
        private System.Windows.Forms.Panel panel6;
        private DarkUI.Controls.DarkButton butClearHeightMap;
        private DarkUI.Controls.DarkButton butBrowseHeightMap;
        private DarkUI.Controls.DarkTextBox tbHeightMapPath;
        private System.Windows.Forms.PictureBox picPreviewHeightMap;
        private DarkUI.Controls.DarkLabel darkLabel10;
    }
}