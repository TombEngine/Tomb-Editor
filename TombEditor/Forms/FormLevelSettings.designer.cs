namespace TombEditor.Forms
{
    partial class FormLevelSettings
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLevelSettings));
            this.pathVariablesDataGridViewContextMenu = new DarkUI.Controls.DarkContextMenu();
            this.pathVariablesDataGridViewContextMenuCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.pathToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.GameEnableExtraReverbPresetsCheckBox = new DarkUI.Controls.DarkCheckBox();
            this.cbEnableExtraBlendingModes = new DarkUI.Controls.DarkCheckBox();
            this.butAssignFromWads = new DarkUI.Controls.DarkButton();
            this.butRemoveMissing = new DarkUI.Controls.DarkButton();
            this.butAssignFromSoundSources = new DarkUI.Controls.DarkButton();
            this.butAssignSoundsFromSelectedCatalogs = new DarkUI.Controls.DarkButton();
            this.butAssignHardcodedSounds = new DarkUI.Controls.DarkButton();
            this.butAutodetectSoundsAndAssign = new DarkUI.Controls.DarkButton();
            this.butDeselectAllSounds = new DarkUI.Controls.DarkButton();
            this.butSelectAllSounds = new DarkUI.Controls.DarkButton();
            this.cbKeepSampleRate = new DarkUI.Controls.DarkCheckBox();
            this.cbRemoveObjects = new DarkUI.Controls.DarkCheckBox();
            this.cbRearrangeRooms = new DarkUI.Controls.DarkCheckBox();
            this.cbRemapAnimTextures = new DarkUI.Controls.DarkCheckBox();
            this.cbDither16BitTextures = new DarkUI.Controls.DarkCheckBox();
            this.cbOverrideAllLightQuality = new DarkUI.Controls.DarkCheckBox();
            this.cmbDefaultLightQuality = new DarkUI.Controls.DarkComboBox();
            this.cbAgressiveFloordataPacking = new DarkUI.Controls.DarkCheckBox();
            this.cbAgressiveTexturePacking = new DarkUI.Controls.DarkCheckBox();
            this.numPadding = new DarkUI.Controls.DarkNumericUpDown();
            this.cbUse32BitLighting = new DarkUI.Controls.DarkCheckBox();
            this.cbCompressTextures = new DarkUI.Controls.DarkCheckBox();
            this.optionsList = new DarkUI.Controls.DarkListView();
            this.butApply = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.tabbedContainer = new TombLib.Controls.DarkTabbedContainer();
            this.tabGame = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.GameEnableQuickStartFeatureCheckBox = new DarkUI.Controls.DarkCheckBox();
            this.gameExecutableFilePathBut = new DarkUI.Controls.DarkButton();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.gameExecutableFilePathTxt = new DarkUI.Controls.DarkTextBox();
            this.panelScripts = new System.Windows.Forms.Panel();
            this.scriptPathBut = new DarkUI.Controls.DarkButton();
            this.darkLabel15 = new DarkUI.Controls.DarkLabel();
            this.tbScriptPath = new DarkUI.Controls.DarkTextBox();
            this.panelLuaPath = new System.Windows.Forms.Panel();
            this.butBrowseLuaPath = new DarkUI.Controls.DarkButton();
            this.darkLabel10 = new DarkUI.Controls.DarkLabel();
            this.tbLuaPath = new DarkUI.Controls.DarkTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.gameLevelFilePathBut = new DarkUI.Controls.DarkButton();
            this.gameLevelFilePathTxt = new DarkUI.Controls.DarkTextBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.gameDirectoryBut = new DarkUI.Controls.DarkButton();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.gameDirectoryTxt = new DarkUI.Controls.DarkTextBox();
            this.panel7 = new System.Windows.Forms.Panel();
            this.comboGameVersion = new DarkUI.Controls.DarkComboBox();
            this.darkLabel14 = new DarkUI.Controls.DarkLabel();
            this.tabTextures = new System.Windows.Forms.TabPage();
            this.textureFileDataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
            this.textureFileDataGridView = new DarkUI.Controls.DarkDataGridView();
            this.textureFileDataGridViewPreviewColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.textureFileDataGridViewPathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textureFileDataGridViewSearchColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.textureFileDataGridViewShowColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.textureFileDataGridViewMessageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textureFileDataGridViewSizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textureFileDataGridViewReplaceMagentaWithTransparencyColumn = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.textureFileDataGridViewConvert512PixelsToDoubleRowsColumn = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.tabObjects = new System.Windows.Forms.TabPage();
            this.objectFileDataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
            this.objectFileDataGridView = new DarkUI.Controls.DarkDataGridView();
            this.objectFileDataGridViewPathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.objectFileDataGridViewSearchColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.objectFileDataGridViewShowColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.objectFileDataGridViewMessageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.tabImportedGeometry = new System.Windows.Forms.TabPage();
            this.importedGeometryManager = new TombEditor.Controls.ImportedGeometryManager();
            this.darkLabel11 = new DarkUI.Controls.DarkLabel();
            this.tabStaticMeshes = new System.Windows.Forms.TabPage();
            this.butSelectAllButShatterStatics = new DarkUI.Controls.DarkButton();
            this.butDeselectAllStatics = new DarkUI.Controls.DarkButton();
            this.butSelectAllStatics = new DarkUI.Controls.DarkButton();
            this.darkLabel19 = new DarkUI.Controls.DarkLabel();
            this.staticMeshMergeDataGridView = new DarkUI.Controls.DarkDataGridView();
            this.colMeshName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMergeStatics = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.colInterpretShadesAsEffect = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.colTintAsAmbient = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.colClearShades = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.darkLabel18 = new DarkUI.Controls.DarkLabel();
            this.darkLabel17 = new DarkUI.Controls.DarkLabel();
            this.tabSkyAndFont = new System.Windows.Forms.TabPage();
            this.panelTr5Sprites = new System.Windows.Forms.Panel();
            this.tr5SpritesTextureFilePathPicPreview = new System.Windows.Forms.PictureBox();
            this.tr5SpritesTextureFilePathBut = new DarkUI.Controls.DarkButton();
            this.tr5SpritesFilePathOptCustom = new DarkUI.Controls.DarkRadioButton();
            this.lblTr5ExtraSprites = new DarkUI.Controls.DarkLabel();
            this.tr5SpritesFilePathOptAuto = new DarkUI.Controls.DarkRadioButton();
            this.tr5SpritesTextureFilePathTxt = new DarkUI.Controls.DarkTextBox();
            this.panelFont = new System.Windows.Forms.Panel();
            this.fontTextureFilePathPicPreview = new System.Windows.Forms.PictureBox();
            this.fontTextureFilePathBut = new DarkUI.Controls.DarkButton();
            this.fontTextureFilePathOptCustom = new DarkUI.Controls.DarkRadioButton();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.fontTextureFilePathOptAuto = new DarkUI.Controls.DarkRadioButton();
            this.fontTextureFilePathTxt = new DarkUI.Controls.DarkTextBox();
            this.panelSky = new System.Windows.Forms.Panel();
            this.skyTextureFilePathPicPreview = new System.Windows.Forms.PictureBox();
            this.skyTextureFilePathBut = new DarkUI.Controls.DarkButton();
            this.skyTextureFilePathOptCustom = new DarkUI.Controls.DarkRadioButton();
            this.darkLabel9 = new DarkUI.Controls.DarkLabel();
            this.skyTextureFilePathOptAuto = new DarkUI.Controls.DarkRadioButton();
            this.skyTextureFilePathTxt = new DarkUI.Controls.DarkTextBox();
            this.tabSoundsCatalogs = new System.Windows.Forms.TabPage();
            this.cbAutodetectIfNoneSelected = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel20 = new DarkUI.Controls.DarkLabel();
            this.labelSoundsCatalogsStatistics = new DarkUI.Controls.DarkLabel();
            this.butSearchSounds = new DarkUI.Controls.DarkButton();
            this.tbFilterSounds = new DarkUI.Controls.DarkTextBox();
            this.lblCatalogsPrompt = new DarkUI.Controls.DarkLabel();
            this.soundsCatalogsDataGridView = new DarkUI.Controls.DarkDataGridView();
            this.SoundsCatalogPathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SoundsCatalogSearchColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.SoundsCatalogReloadButton = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.SoundsCatalogEditColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.SoundsCatalogsAssignColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.SoundsCatalogsSoundCountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SoundsCatalogMessageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.soundsCatalogsDataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
            this.darkLabel50 = new DarkUI.Controls.DarkLabel();
            this.selectedSoundsDataGridView = new DarkUI.Controls.DarkDataGridView();
            this.colSoundsEnabled = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
            this.colSoundID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabSamples = new System.Windows.Forms.TabPage();
            this.soundDataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
            this.soundDataGridView = new DarkUI.Controls.DarkDataGridView();
            this.soundDataGridViewColumnPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.soundDataGridViewColumnSearch = new DarkUI.Controls.DarkDataGridViewButtonColumn();
            this.lblPathsPrompt = new DarkUI.Controls.DarkLabel();
            this.tabMisc = new System.Windows.Forms.TabPage();
            this.panelTr5Weather = new System.Windows.Forms.Panel();
            this.comboTr5Weather = new DarkUI.Controls.DarkComboBox();
            this.lblTr5Weather = new DarkUI.Controls.DarkLabel();
            this.panelTr5LaraType = new System.Windows.Forms.Panel();
            this.comboLaraType = new DarkUI.Controls.DarkComboBox();
            this.lblLaraType = new DarkUI.Controls.DarkLabel();
            this.tabTrx = new System.Windows.Forms.TabPage();
            this.comboTrxTextureDepth = new DarkUI.Controls.DarkComboBox();
            this.lblTrxTextureDepth = new DarkUI.Controls.DarkLabel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.levelFilePathBut = new DarkUI.Controls.DarkButton();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.levelFilePathTxt = new DarkUI.Controls.DarkTextBox();
            this.panel12 = new System.Windows.Forms.Panel();
            this.cmbSampleRate = new DarkUI.Controls.DarkComboBox();
            this.darkLabel22 = new DarkUI.Controls.DarkLabel();
            this.darkLabel13 = new DarkUI.Controls.DarkLabel();
            this.darkLabel16 = new DarkUI.Controls.DarkLabel();
            this.panelRoomAmbientLight = new DarkUI.Controls.DarkPanel();
            this.darkLabel12 = new DarkUI.Controls.DarkLabel();
            this.tabPaths = new System.Windows.Forms.TabPage();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.pathVariablesDataGridView = new DarkUI.Controls.DarkDataGridView();
            this.pathVariablesDataGridViewNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pathVariablesDataGridViewValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSoundsId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSoundsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SelectedSoundsCatalogColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SelectedSoundsGameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SelectedSoundsOriginalIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pathVariablesDataGridViewContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPadding)).BeginInit();
            this.darkSectionPanel1.SuspendLayout();
            this.tabbedContainer.SuspendLayout();
            this.tabGame.SuspendLayout();
            this.panelLuaPath.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panelScripts.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel7.SuspendLayout();
            this.tabTextures.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textureFileDataGridView)).BeginInit();
            this.tabObjects.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.objectFileDataGridView)).BeginInit();
            this.tabImportedGeometry.SuspendLayout();
            this.tabStaticMeshes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.staticMeshMergeDataGridView)).BeginInit();
            this.tabSkyAndFont.SuspendLayout();
            this.panelTr5Sprites.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tr5SpritesTextureFilePathPicPreview)).BeginInit();
            this.panelFont.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fontTextureFilePathPicPreview)).BeginInit();
            this.panelSky.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.skyTextureFilePathPicPreview)).BeginInit();
            this.tabSoundsCatalogs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.soundsCatalogsDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.selectedSoundsDataGridView)).BeginInit();
            this.tabSamples.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.soundDataGridView)).BeginInit();
            this.tabMisc.SuspendLayout();
            this.panelTr5Weather.SuspendLayout();
            this.panelTr5LaraType.SuspendLayout();
            this.tabTrx.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel12.SuspendLayout();
            this.tabPaths.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pathVariablesDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // pathVariablesDataGridViewContextMenu
            // 
            this.pathVariablesDataGridViewContextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pathVariablesDataGridViewContextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pathVariablesDataGridViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pathVariablesDataGridViewContextMenuCopy});
            this.pathVariablesDataGridViewContextMenu.Name = "variablesListContextMenu";
            this.pathVariablesDataGridViewContextMenu.Size = new System.Drawing.Size(102, 26);
            // 
            // pathVariablesDataGridViewContextMenuCopy
            // 
            this.pathVariablesDataGridViewContextMenuCopy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pathVariablesDataGridViewContextMenuCopy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pathVariablesDataGridViewContextMenuCopy.Name = "pathVariablesDataGridViewContextMenuCopy";
            this.pathVariablesDataGridViewContextMenuCopy.Size = new System.Drawing.Size(101, 22);
            this.pathVariablesDataGridViewContextMenuCopy.Text = "Copy";
            this.pathVariablesDataGridViewContextMenuCopy.Click += new System.EventHandler(this.pathVariablesDataGridViewContextMenuCopy_Click);
            // 
            // pathToolTip
            // 
            this.pathToolTip.AutoPopDelay = 32000;
            this.pathToolTip.InitialDelay = 300;
            this.pathToolTip.ReshowDelay = 100;
            this.pathToolTip.ShowAlways = true;
            // 
            // GameEnableExtraReverbPresetsCheckBox
            // 
            this.GameEnableExtraReverbPresetsCheckBox.AutoSize = true;
            this.GameEnableExtraReverbPresetsCheckBox.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.GameEnableExtraReverbPresetsCheckBox.Location = new System.Drawing.Point(19, 76);
            this.GameEnableExtraReverbPresetsCheckBox.Name = "GameEnableExtraReverbPresetsCheckBox";
            this.GameEnableExtraReverbPresetsCheckBox.Size = new System.Drawing.Size(155, 17);
            this.GameEnableExtraReverbPresetsCheckBox.TabIndex = 7;
            this.GameEnableExtraReverbPresetsCheckBox.Text = "Show FLEP reverb presets";
            this.pathToolTip.SetToolTip(this.GameEnableExtraReverbPresetsCheckBox, "Adds extra reverb presets to UI which are enabled by corresponding FLEP patch.\r\n");
            this.GameEnableExtraReverbPresetsCheckBox.CheckedChanged += new System.EventHandler(this.GameEnableExtraReverbPresetsCheckBox_CheckedChanged);
            // 
            // cbEnableExtraBlendingModes
            // 
            this.cbEnableExtraBlendingModes.AutoSize = true;
            this.cbEnableExtraBlendingModes.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbEnableExtraBlendingModes.Location = new System.Drawing.Point(19, 53);
            this.cbEnableExtraBlendingModes.Name = "cbEnableExtraBlendingModes";
            this.cbEnableExtraBlendingModes.Size = new System.Drawing.Size(168, 17);
            this.cbEnableExtraBlendingModes.TabIndex = 6;
            this.cbEnableExtraBlendingModes.Text = "Show extra blending modes";
            this.pathToolTip.SetToolTip(this.cbEnableExtraBlendingModes, "Adds extra blending modes to UI which are enabled by corresponding FLEP patch.");
            this.cbEnableExtraBlendingModes.CheckedChanged += new System.EventHandler(this.cbEnableExtraBlendingModes_CheckedChanged);
            // 
            // butAssignFromWads
            // 
            this.butAssignFromWads.Checked = false;
            this.butAssignFromWads.Location = new System.Drawing.Point(495, 245);
            this.butAssignFromWads.Name = "butAssignFromWads";
            this.butAssignFromWads.Size = new System.Drawing.Size(72, 22);
            this.butAssignFromWads.TabIndex = 115;
            this.butAssignFromWads.Text = "From wads";
            this.butAssignFromWads.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.pathToolTip.SetToolTip(this.butAssignFromWads, "Select sounds from wads");
            this.butAssignFromWads.Click += new System.EventHandler(this.butAssignFromWads_Click);
            // 
            // butRemoveMissing
            // 
            this.butRemoveMissing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butRemoveMissing.Checked = false;
            this.butRemoveMissing.Location = new System.Drawing.Point(684, 480);
            this.butRemoveMissing.Name = "butRemoveMissing";
            this.butRemoveMissing.Size = new System.Drawing.Size(91, 22);
            this.butRemoveMissing.TabIndex = 114;
            this.butRemoveMissing.Text = "Clear missing";
            this.pathToolTip.SetToolTip(this.butRemoveMissing, "Hide sounds which aren\'t present in any of the catalogs");
            this.butRemoveMissing.Click += new System.EventHandler(this.butRemoveMissing_Click);
            // 
            // butAssignFromSoundSources
            // 
            this.butAssignFromSoundSources.Checked = false;
            this.butAssignFromSoundSources.Location = new System.Drawing.Point(373, 245);
            this.butAssignFromSoundSources.Name = "butAssignFromSoundSources";
            this.butAssignFromSoundSources.Size = new System.Drawing.Size(116, 22);
            this.butAssignFromSoundSources.TabIndex = 113;
            this.butAssignFromSoundSources.Text = "From sound sources";
            this.butAssignFromSoundSources.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.pathToolTip.SetToolTip(this.butAssignFromSoundSources, "Select sounds from sound sources placed in level");
            this.butAssignFromSoundSources.Click += new System.EventHandler(this.ButAssignFromSoundSources_Click);
            // 
            // butAssignSoundsFromSelectedCatalogs
            // 
            this.butAssignSoundsFromSelectedCatalogs.Checked = false;
            this.butAssignSoundsFromSelectedCatalogs.Location = new System.Drawing.Point(236, 245);
            this.butAssignSoundsFromSelectedCatalogs.Name = "butAssignSoundsFromSelectedCatalogs";
            this.butAssignSoundsFromSelectedCatalogs.Size = new System.Drawing.Size(131, 22);
            this.butAssignSoundsFromSelectedCatalogs.TabIndex = 112;
            this.butAssignSoundsFromSelectedCatalogs.Text = "From selected catalogs";
            this.butAssignSoundsFromSelectedCatalogs.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.pathToolTip.SetToolTip(this.butAssignSoundsFromSelectedCatalogs, "Select sounds from selected catalogs");
            this.butAssignSoundsFromSelectedCatalogs.Click += new System.EventHandler(this.ButAssignSoundsFromSelectedCatalogs_Click);
            // 
            // butAssignHardcodedSounds
            // 
            this.butAssignHardcodedSounds.Checked = false;
            this.butAssignHardcodedSounds.Location = new System.Drawing.Point(105, 245);
            this.butAssignHardcodedSounds.Name = "butAssignHardcodedSounds";
            this.butAssignHardcodedSounds.Size = new System.Drawing.Size(125, 22);
            this.butAssignHardcodedSounds.TabIndex = 111;
            this.butAssignHardcodedSounds.Text = "Hardcoded & global";
            this.butAssignHardcodedSounds.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.pathToolTip.SetToolTip(this.butAssignHardcodedSounds, "Select hardcoded sounds and all sounds from all catalogs which are marked as glob" +
        "al");
            this.butAssignHardcodedSounds.Click += new System.EventHandler(this.ButAssignHardcodedSounds_Click);
            // 
            // butAutodetectSoundsAndAssign
            // 
            this.butAutodetectSoundsAndAssign.BackColor = System.Drawing.Color.DarkGreen;
            this.butAutodetectSoundsAndAssign.BackColorUseGeneric = false;
            this.butAutodetectSoundsAndAssign.Checked = false;
            this.butAutodetectSoundsAndAssign.Image = global::TombEditor.Properties.Resources.actions_light_on_16;
            this.butAutodetectSoundsAndAssign.Location = new System.Drawing.Point(6, 245);
            this.butAutodetectSoundsAndAssign.Name = "butAutodetectSoundsAndAssign";
            this.butAutodetectSoundsAndAssign.Size = new System.Drawing.Size(93, 22);
            this.butAutodetectSoundsAndAssign.TabIndex = 110;
            this.butAutodetectSoundsAndAssign.Text = "Autodetect";
            this.butAutodetectSoundsAndAssign.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.pathToolTip.SetToolTip(this.butAutodetectSoundsAndAssign, "Find and select all sounds used in a level.\r\nThis includes hardcoded and global s" +
        "ounds, sounds used in animations, sound sources and flipeffect sounds.\r\nScripted" +
        " sounds are not included.");
            this.butAutodetectSoundsAndAssign.Click += new System.EventHandler(this.ButAutodetectSoundsAndAssign_Click);
            // 
            // butDeselectAllSounds
            // 
            this.butDeselectAllSounds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeselectAllSounds.Checked = false;
            this.butDeselectAllSounds.Location = new System.Drawing.Point(695, 245);
            this.butDeselectAllSounds.Name = "butDeselectAllSounds";
            this.butDeselectAllSounds.Size = new System.Drawing.Size(80, 22);
            this.butDeselectAllSounds.TabIndex = 108;
            this.butDeselectAllSounds.Text = "Deselect all";
            this.pathToolTip.SetToolTip(this.butDeselectAllSounds, "Deselect all sounds");
            this.butDeselectAllSounds.Click += new System.EventHandler(this.butDeselectAllSounds_Click);
            // 
            // butSelectAllSounds
            // 
            this.butSelectAllSounds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSelectAllSounds.Checked = false;
            this.butSelectAllSounds.Location = new System.Drawing.Point(609, 245);
            this.butSelectAllSounds.Name = "butSelectAllSounds";
            this.butSelectAllSounds.Size = new System.Drawing.Size(80, 22);
            this.butSelectAllSounds.TabIndex = 107;
            this.butSelectAllSounds.Text = "Select all";
            this.pathToolTip.SetToolTip(this.butSelectAllSounds, "Select all available sounds");
            this.butSelectAllSounds.Click += new System.EventHandler(this.butSelectAllSounds_Click);
            // 
            // cbKeepSampleRate
            // 
            this.cbKeepSampleRate.AutoSize = true;
            this.cbKeepSampleRate.Location = new System.Drawing.Point(3, 281);
            this.cbKeepSampleRate.Name = "cbKeepSampleRate";
            this.cbKeepSampleRate.Size = new System.Drawing.Size(208, 17);
            this.cbKeepSampleRate.TabIndex = 114;
            this.cbKeepSampleRate.Tag = "";
            this.cbKeepSampleRate.Text = "Use custom sample rate for sounds:";
            this.pathToolTip.SetToolTip(this.cbKeepSampleRate, "Override original game sample rate with custom one.\r\nMay be necessary if level bu" +
        "ilder uses samples with custom sample rate.");
            this.cbKeepSampleRate.CheckedChanged += new System.EventHandler(this.cbKeepSampleRate_CheckedChanged);
            // 
            // cbRemoveObjects
            // 
            this.cbRemoveObjects.AutoSize = true;
            this.cbRemoveObjects.Location = new System.Drawing.Point(3, 258);
            this.cbRemoveObjects.Name = "cbRemoveObjects";
            this.cbRemoveObjects.Size = new System.Drawing.Size(253, 17);
            this.cbRemoveObjects.TabIndex = 112;
            this.cbRemoveObjects.Tag = "";
            this.cbRemoveObjects.Text = "Remove unused objects from compiled level";
            this.pathToolTip.SetToolTip(this.cbRemoveObjects, "Removes moveables and statics that are in the WADs, but not placed in the level.\r" +
        "\nUse with caution in the case of dynamic object creation with scripts!");
            this.cbRemoveObjects.CheckedChanged += new System.EventHandler(this.cbRemoveObjects_CheckedChanged);
            // 
            // cbRearrangeRooms
            // 
            this.cbRearrangeRooms.AutoSize = true;
            this.cbRearrangeRooms.Location = new System.Drawing.Point(3, 235);
            this.cbRearrangeRooms.Name = "cbRearrangeRooms";
            this.cbRearrangeRooms.Size = new System.Drawing.Size(280, 17);
            this.cbRearrangeRooms.TabIndex = 111;
            this.cbRearrangeRooms.Tag = "";
            this.cbRearrangeRooms.Text = "Rearrange vertically connected rooms if necessary";
            this.pathToolTip.SetToolTip(this.cbRearrangeRooms, "Prioritize vertically connected rooms during compilation.\r\nAllows to safely make " +
        "more rooms until overall amount of vertically connected ones won\'t hit absolute " +
        "limit.");
            this.cbRearrangeRooms.CheckedChanged += new System.EventHandler(this.cbRearrangeRooms_CheckedChanged);
            // 
            // cbRemapAnimTextures
            // 
            this.cbRemapAnimTextures.AutoSize = true;
            this.cbRemapAnimTextures.Location = new System.Drawing.Point(3, 212);
            this.cbRemapAnimTextures.Name = "cbRemapAnimTextures";
            this.cbRemapAnimTextures.Size = new System.Drawing.Size(397, 17);
            this.cbRemapAnimTextures.TabIndex = 110;
            this.cbRemapAnimTextures.Tag = "";
            this.cbRemapAnimTextures.Text = "Map animated textures to imported geometry, objects and static meshes";
            this.pathToolTip.SetToolTip(this.cbRemapAnimTextures, "Scan all objects for textures which are similar to any animated frame and apply a" +
        "nimations to them.");
            this.cbRemapAnimTextures.CheckedChanged += new System.EventHandler(this.cbRemapAnimTextures_CheckedChanged);
            // 
            // cbDither16BitTextures
            // 
            this.cbDither16BitTextures.AutoSize = true;
            this.cbDither16BitTextures.Location = new System.Drawing.Point(3, 166);
            this.cbDither16BitTextures.Name = "cbDither16BitTextures";
            this.cbDither16BitTextures.Size = new System.Drawing.Size(132, 17);
            this.cbDither16BitTextures.TabIndex = 109;
            this.cbDither16BitTextures.Tag = "";
            this.cbDither16BitTextures.Text = "Dither 16-bit textures";
            this.pathToolTip.SetToolTip(this.cbDither16BitTextures, "Apply dithering and premultiply alpha channel to brightness for 16-bit textures.");
            this.cbDither16BitTextures.CheckedChanged += new System.EventHandler(this.cbDither16BitTextures_CheckedChanged);
            // 
            // cbOverrideAllLightQuality
            // 
            this.cbOverrideAllLightQuality.AutoSize = true;
            this.cbOverrideAllLightQuality.Location = new System.Drawing.Point(216, 42);
            this.cbOverrideAllLightQuality.Name = "cbOverrideAllLightQuality";
            this.cbOverrideAllLightQuality.Size = new System.Drawing.Size(166, 17);
            this.cbOverrideAllLightQuality.TabIndex = 108;
            this.cbOverrideAllLightQuality.Tag = "";
            this.cbOverrideAllLightQuality.Text = "Override individual settings";
            this.pathToolTip.SetToolTip(this.cbOverrideAllLightQuality, "Override raytracing quality for all lights with specified one");
            this.cbOverrideAllLightQuality.Visible = false;
            this.cbOverrideAllLightQuality.CheckedChanged += new System.EventHandler(this.cbOverrideAllLightQuality_CheckedChanged);
            // 
            // cmbDefaultLightQuality
            // 
            this.cmbDefaultLightQuality.FormattingEnabled = true;
            this.cmbDefaultLightQuality.Items.AddRange(new object[] {
            "Low",
            "Medium",
            "High"});
            this.cmbDefaultLightQuality.Location = new System.Drawing.Point(129, 40);
            this.cmbDefaultLightQuality.Name = "cmbDefaultLightQuality";
            this.cmbDefaultLightQuality.Size = new System.Drawing.Size(81, 23);
            this.cmbDefaultLightQuality.TabIndex = 107;
            this.pathToolTip.SetToolTip(this.cmbDefaultLightQuality, "Raytracing quality for all lights with \'Default\' light quality setting");
            this.cmbDefaultLightQuality.SelectedIndexChanged += new System.EventHandler(this.cmbDefaultLightQuality_SelectedIndexChanged);
            // 
            // cbAgressiveFloordataPacking
            // 
            this.cbAgressiveFloordataPacking.AutoSize = true;
            this.cbAgressiveFloordataPacking.Location = new System.Drawing.Point(3, 120);
            this.cbAgressiveFloordataPacking.Name = "cbAgressiveFloordataPacking";
            this.cbAgressiveFloordataPacking.Size = new System.Drawing.Size(176, 17);
            this.cbAgressiveFloordataPacking.TabIndex = 105;
            this.cbAgressiveFloordataPacking.Tag = "";
            this.cbAgressiveFloordataPacking.Text = "Aggressive floordata packing";
            this.pathToolTip.SetToolTip(this.cbAgressiveFloordataPacking, "Scan and merge similar floordata sequences.\r\nRecommended mode for TR1-3 targets.");
            this.cbAgressiveFloordataPacking.CheckedChanged += new System.EventHandler(this.cbAgressiveFloordataPacking_CheckedChanged);
            // 
            // cbAgressiveTexturePacking
            // 
            this.cbAgressiveTexturePacking.AutoSize = true;
            this.cbAgressiveTexturePacking.Location = new System.Drawing.Point(3, 97);
            this.cbAgressiveTexturePacking.Name = "cbAgressiveTexturePacking";
            this.cbAgressiveTexturePacking.Size = new System.Drawing.Size(340, 17);
            this.cbAgressiveTexturePacking.TabIndex = 104;
            this.cbAgressiveTexturePacking.Tag = "";
            this.cbAgressiveTexturePacking.Text = "Aggressive texture packing (merge object and room textures)";
            this.pathToolTip.SetToolTip(this.cbAgressiveTexturePacking, "Pack both object and room textures in same texture pages.\r\nRecommended mode for T" +
        "R1-3 targets.");
            this.cbAgressiveTexturePacking.CheckedChanged += new System.EventHandler(this.cbAgressiveTexturePacking_CheckedChanged);
            // 
            // numPadding
            // 
            this.numPadding.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.numPadding.Location = new System.Drawing.Point(129, 69);
            this.numPadding.LoopValues = false;
            this.numPadding.Name = "numPadding";
            this.numPadding.Size = new System.Drawing.Size(81, 22);
            this.numPadding.TabIndex = 102;
            this.pathToolTip.SetToolTip(this.numPadding, "Edge pixel padding to prevent texture border bleeding.\r\nIf set to 0, seams betwee" +
        "n textures may become visible.");
            this.numPadding.ValueChanged += new System.EventHandler(this.numPadding_ValueChanged);
            // 
            // cbUse32BitLighting
            // 
            this.cbUse32BitLighting.AutoSize = true;
            this.cbUse32BitLighting.Location = new System.Drawing.Point(3, 143);
            this.cbUse32BitLighting.Name = "cbUse32BitLighting";
            this.cbUse32BitLighting.Size = new System.Drawing.Size(231, 17);
            this.cbUse32BitLighting.TabIndex = 116;
            this.cbUse32BitLighting.Tag = "";
            this.cbUse32BitLighting.Text = "Use 32-bit lighting (requires FLEP patch)";
            this.pathToolTip.SetToolTip(this.cbUse32BitLighting, "Changes room lighting format from 16-bit to 32-bit, which fixes \"rainbow bug\".\r\nR" +
        "equires specific FLEP patch to be activated, otherwise lighting will not look co" +
        "rrect.");
            this.cbUse32BitLighting.CheckedChanged += new System.EventHandler(this.cbUse32BitLighting_CheckedChanged);
            // 
            // cbCompressTextures
            // 
            this.cbCompressTextures.AutoSize = true;
            this.cbCompressTextures.Location = new System.Drawing.Point(3, 189);
            this.cbCompressTextures.Name = "cbCompressTextures";
            this.cbCompressTextures.Size = new System.Drawing.Size(120, 17);
            this.cbCompressTextures.TabIndex = 117;
            this.cbCompressTextures.Tag = "";
            this.cbCompressTextures.Text = "Compress textures";
            this.pathToolTip.SetToolTip(this.cbCompressTextures, "Use DirectX texture compression for all level textures.\r\nUse with caution, as it " +
        "may cause artifacts on a legacy small texture fragments.");
            this.cbCompressTextures.CheckedChanged += new System.EventHandler(this.cbCompressTextures_CheckedChanged);
            // 
            // optionsList
            // 
            this.optionsList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.optionsList.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.optionsList.Location = new System.Drawing.Point(5, 5);
            this.optionsList.Name = "optionsList";
            this.optionsList.Size = new System.Drawing.Size(198, 533);
            this.optionsList.TabIndex = 6;
            // 
            // butApply
            // 
            this.butApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butApply.Checked = false;
            this.butApply.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butApply.Location = new System.Drawing.Point(743, 544);
            this.butApply.Name = "butApply";
            this.butApply.Size = new System.Drawing.Size(80, 24);
            this.butApply.TabIndex = 3;
            this.butApply.Text = "Apply";
            this.butApply.Click += new System.EventHandler(this.butApply_Click);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Checked = false;
            this.butOk.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butOk.Location = new System.Drawing.Point(829, 544);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 24);
            this.butOk.TabIndex = 3;
            this.butOk.Text = "OK";
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.butCancel.Location = new System.Drawing.Point(915, 544);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 24);
            this.butCancel.TabIndex = 3;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // colorDialog
            // 
            this.colorDialog.AnyColor = true;
            this.colorDialog.FullOpen = true;
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkSectionPanel1.Controls.Add(this.tabbedContainer);
            this.darkSectionPanel1.Location = new System.Drawing.Point(208, 5);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = null;
            this.darkSectionPanel1.Size = new System.Drawing.Size(788, 533);
            this.darkSectionPanel1.TabIndex = 7;
            // 
            // tabbedContainer
            // 
            this.tabbedContainer.Controls.Add(this.tabGame);
            this.tabbedContainer.Controls.Add(this.tabTextures);
            this.tabbedContainer.Controls.Add(this.tabObjects);
            this.tabbedContainer.Controls.Add(this.tabImportedGeometry);
            this.tabbedContainer.Controls.Add(this.tabStaticMeshes);
            this.tabbedContainer.Controls.Add(this.tabSkyAndFont);
            this.tabbedContainer.Controls.Add(this.tabSoundsCatalogs);
            this.tabbedContainer.Controls.Add(this.tabSamples);
            this.tabbedContainer.Controls.Add(this.tabMisc);
            this.tabbedContainer.Controls.Add(this.tabPaths);
            this.tabbedContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabbedContainer.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabbedContainer.Location = new System.Drawing.Point(1, 1);
            this.tabbedContainer.Name = "tabbedContainer";
            this.tabbedContainer.SelectedIndex = 0;
            this.tabbedContainer.Size = new System.Drawing.Size(786, 531);
            this.tabbedContainer.TabIndex = 2;
            // 
            // tabGame
            // 
            this.tabGame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabGame.Controls.Add(this.panel3);
            this.tabGame.Controls.Add(this.panelLuaPath);
            this.tabGame.Controls.Add(this.panelScripts);
            this.tabGame.Controls.Add(this.panel1);
            this.tabGame.Controls.Add(this.panel2);
            this.tabGame.Controls.Add(this.panel7);
            this.tabGame.Location = new System.Drawing.Point(4, 22);
            this.tabGame.Name = "tabGame";
            this.tabGame.Padding = new System.Windows.Forms.Padding(3);
            this.tabGame.Size = new System.Drawing.Size(778, 505);
            this.tabGame.TabIndex = 4;
            this.tabGame.Text = "Game";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.GameEnableExtraReverbPresetsCheckBox);
            this.panel3.Controls.Add(this.cbEnableExtraBlendingModes);
            this.panel3.Controls.Add(this.GameEnableQuickStartFeatureCheckBox);
            this.panel3.Controls.Add(this.gameExecutableFilePathBut);
            this.panel3.Controls.Add(this.darkLabel3);
            this.panel3.Controls.Add(this.gameExecutableFilePathTxt);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 253);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(772, 249);
            this.panel3.TabIndex = 3;
            // 
            // GameEnableQuickStartFeatureCheckBox
            // 
            this.GameEnableQuickStartFeatureCheckBox.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.GameEnableQuickStartFeatureCheckBox.Location = new System.Drawing.Point(19, 99);
            this.GameEnableQuickStartFeatureCheckBox.Name = "GameEnableQuickStartFeatureCheckBox";
            this.GameEnableQuickStartFeatureCheckBox.Size = new System.Drawing.Size(420, 16);
            this.GameEnableQuickStartFeatureCheckBox.TabIndex = 4;
            this.GameEnableQuickStartFeatureCheckBox.Text = "Enable Tomb4.exe quick start feature";
            this.GameEnableQuickStartFeatureCheckBox.CheckedChanged += new System.EventHandler(this.GameEnableQuickStartFeatureCheckBox_CheckedChanged);
            // 
            // gameExecutableFilePathBut
            // 
            this.gameExecutableFilePathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gameExecutableFilePathBut.Checked = false;
            this.gameExecutableFilePathBut.Location = new System.Drawing.Point(677, 23);
            this.gameExecutableFilePathBut.Name = "gameExecutableFilePathBut";
            this.gameExecutableFilePathBut.Size = new System.Drawing.Size(92, 22);
            this.gameExecutableFilePathBut.TabIndex = 3;
            this.gameExecutableFilePathBut.Text = "Browse";
            this.gameExecutableFilePathBut.Click += new System.EventHandler(this.gameExecutableFilePathBut_Click);
            // 
            // darkLabel3
            // 
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(0, 2);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(439, 17);
            this.darkLabel3.TabIndex = 1;
            this.darkLabel3.Text = "Target executable that is started with the \'Build and Play\' button";
            // 
            // gameExecutableFilePathTxt
            // 
            this.gameExecutableFilePathTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameExecutableFilePathTxt.Location = new System.Drawing.Point(19, 23);
            this.gameExecutableFilePathTxt.Name = "gameExecutableFilePathTxt";
            this.gameExecutableFilePathTxt.Size = new System.Drawing.Size(652, 22);
            this.gameExecutableFilePathTxt.TabIndex = 2;
            this.gameExecutableFilePathTxt.TextChanged += new System.EventHandler(this.gameExecutableFilePathTxt_TextChanged);
            // 
            // panelLuaPath
            // 
            this.panelLuaPath.Controls.Add(this.butBrowseLuaPath);
            this.panelLuaPath.Controls.Add(this.darkLabel10);
            this.panelLuaPath.Controls.Add(this.tbLuaPath);
            this.panelLuaPath.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLuaPath.Location = new System.Drawing.Point(3, 203);
            this.panelLuaPath.Name = "panelLuaPath";
            this.panelLuaPath.Size = new System.Drawing.Size(772, 50);
            this.panelLuaPath.TabIndex = 100;
            // 
            // butBrowseLuaPath
            // 
            this.butBrowseLuaPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butBrowseLuaPath.Checked = false;
            this.butBrowseLuaPath.Location = new System.Drawing.Point(677, 23);
            this.butBrowseLuaPath.Name = "butBrowseLuaPath";
            this.butBrowseLuaPath.Size = new System.Drawing.Size(92, 22);
            this.butBrowseLuaPath.TabIndex = 3;
            this.butBrowseLuaPath.Text = "Browse";
            this.butBrowseLuaPath.Click += new System.EventHandler(this.butBrowseLuaPath_Click);
            // 
            // darkLabel10
            // 
            this.darkLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel10.Location = new System.Drawing.Point(0, 3);
            this.darkLabel10.Name = "darkLabel10";
            this.darkLabel10.Size = new System.Drawing.Size(439, 17);
            this.darkLabel10.TabIndex = 1;
            this.darkLabel10.Text = "Path of Lua level script file:";
            // 
            // tbLuaPath
            // 
            this.tbLuaPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLuaPath.Location = new System.Drawing.Point(19, 23);
            this.tbLuaPath.Name = "tbLuaPath";
            this.tbLuaPath.Size = new System.Drawing.Size(652, 22);
            this.tbLuaPath.TabIndex = 2;
            // 
            // panelScripts
            // 
            this.panelScripts.Controls.Add(this.scriptPathBut);
            this.panelScripts.Controls.Add(this.darkLabel15);
            this.panelScripts.Controls.Add(this.tbScriptPath);
            this.panelScripts.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelScripts.Location = new System.Drawing.Point(3, 153);
            this.panelScripts.Name = "panelScripts";
            this.panelScripts.Size = new System.Drawing.Size(772, 50);
            this.panelScripts.TabIndex = 99;
            // 
            // scriptPathBut
            // 
            this.scriptPathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.scriptPathBut.Checked = false;
            this.scriptPathBut.Location = new System.Drawing.Point(677, 23);
            this.scriptPathBut.Name = "scriptPathBut";
            this.scriptPathBut.Size = new System.Drawing.Size(92, 22);
            this.scriptPathBut.TabIndex = 3;
            this.scriptPathBut.Text = "Browse";
            this.scriptPathBut.Click += new System.EventHandler(this.scriptPathBut_Click);
            // 
            // darkLabel15
            // 
            this.darkLabel15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel15.Location = new System.Drawing.Point(0, 3);
            this.darkLabel15.Name = "darkLabel15";
            this.darkLabel15.Size = new System.Drawing.Size(439, 17);
            this.darkLabel15.TabIndex = 1;
            this.darkLabel15.Text = "Path of TXT files for script (Optional):";
            // 
            // tbScriptPath
            // 
            this.tbScriptPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbScriptPath.Location = new System.Drawing.Point(19, 23);
            this.tbScriptPath.Name = "tbScriptPath";
            this.tbScriptPath.Size = new System.Drawing.Size(652, 22);
            this.tbScriptPath.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.gameLevelFilePathBut);
            this.panel1.Controls.Add(this.gameLevelFilePathTxt);
            this.panel1.Controls.Add(this.darkLabel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 103);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(772, 50);
            this.panel1.TabIndex = 3;
            // 
            // gameLevelFilePathBut
            // 
            this.gameLevelFilePathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gameLevelFilePathBut.Checked = false;
            this.gameLevelFilePathBut.Location = new System.Drawing.Point(677, 23);
            this.gameLevelFilePathBut.Name = "gameLevelFilePathBut";
            this.gameLevelFilePathBut.Size = new System.Drawing.Size(92, 22);
            this.gameLevelFilePathBut.TabIndex = 3;
            this.gameLevelFilePathBut.Text = "Browse";
            this.gameLevelFilePathBut.Click += new System.EventHandler(this.gameLevelFilePathBut_Click);
            // 
            // gameLevelFilePathTxt
            // 
            this.gameLevelFilePathTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameLevelFilePathTxt.Location = new System.Drawing.Point(19, 23);
            this.gameLevelFilePathTxt.Name = "gameLevelFilePathTxt";
            this.gameLevelFilePathTxt.Size = new System.Drawing.Size(652, 22);
            this.gameLevelFilePathTxt.TabIndex = 2;
            this.gameLevelFilePathTxt.TextChanged += new System.EventHandler(this.gameLevelFilePathTxt_TextChanged);
            // 
            // darkLabel2
            // 
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(0, 3);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(439, 17);
            this.darkLabel2.TabIndex = 1;
            this.darkLabel2.Text = "Target folder and filename for level file:";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.gameDirectoryBut);
            this.panel2.Controls.Add(this.darkLabel7);
            this.panel2.Controls.Add(this.gameDirectoryTxt);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 53);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(772, 50);
            this.panel2.TabIndex = 2;
            // 
            // gameDirectoryBut
            // 
            this.gameDirectoryBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gameDirectoryBut.Checked = false;
            this.gameDirectoryBut.Location = new System.Drawing.Point(677, 23);
            this.gameDirectoryBut.Name = "gameDirectoryBut";
            this.gameDirectoryBut.Size = new System.Drawing.Size(92, 22);
            this.gameDirectoryBut.TabIndex = 3;
            this.gameDirectoryBut.Text = "Browse";
            this.gameDirectoryBut.Click += new System.EventHandler(this.GameDirectoryBut_Click);
            // 
            // darkLabel7
            // 
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(0, 3);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(439, 17);
            this.darkLabel7.TabIndex = 1;
            this.darkLabel7.Text = "Folder in which all runtime game components reside:";
            // 
            // gameDirectoryTxt
            // 
            this.gameDirectoryTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameDirectoryTxt.Location = new System.Drawing.Point(19, 23);
            this.gameDirectoryTxt.Name = "gameDirectoryTxt";
            this.gameDirectoryTxt.Size = new System.Drawing.Size(652, 22);
            this.gameDirectoryTxt.TabIndex = 2;
            this.gameDirectoryTxt.TextChanged += new System.EventHandler(this.gameDirectoryTxt_TextChanged);
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.comboGameVersion);
            this.panel7.Controls.Add(this.darkLabel14);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel7.Location = new System.Drawing.Point(3, 3);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(772, 50);
            this.panel7.TabIndex = 2;
            // 
            // comboGameVersion
            // 
            this.comboGameVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboGameVersion.FormattingEnabled = true;
            this.comboGameVersion.Location = new System.Drawing.Point(19, 20);
            this.comboGameVersion.Name = "comboGameVersion";
            this.comboGameVersion.Size = new System.Drawing.Size(652, 23);
            this.comboGameVersion.TabIndex = 2;
            this.comboGameVersion.SelectedIndexChanged += new System.EventHandler(this.comboGameVersion_SelectedIndexChanged);
            // 
            // darkLabel14
            // 
            this.darkLabel14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel14.Location = new System.Drawing.Point(2, 0);
            this.darkLabel14.Name = "darkLabel14";
            this.darkLabel14.Size = new System.Drawing.Size(439, 17);
            this.darkLabel14.TabIndex = 1;
            this.darkLabel14.Text = "Game version to target:";
            // 
            // tabTextures
            // 
            this.tabTextures.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabTextures.Controls.Add(this.textureFileDataGridViewControls);
            this.tabTextures.Controls.Add(this.textureFileDataGridView);
            this.tabTextures.Controls.Add(this.darkLabel4);
            this.tabTextures.Location = new System.Drawing.Point(4, 22);
            this.tabTextures.Name = "tabTextures";
            this.tabTextures.Padding = new System.Windows.Forms.Padding(3);
            this.tabTextures.Size = new System.Drawing.Size(778, 505);
            this.tabTextures.TabIndex = 7;
            this.tabTextures.Text = "Texture Files";
            // 
            // textureFileDataGridViewControls
            // 
            this.textureFileDataGridViewControls.AllowUserDelete = false;
            this.textureFileDataGridViewControls.AllowUserMove = false;
            this.textureFileDataGridViewControls.AllowUserNew = false;
            this.textureFileDataGridViewControls.AlwaysInsertAtZero = false;
            this.textureFileDataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textureFileDataGridViewControls.Enabled = false;
            this.textureFileDataGridViewControls.Location = new System.Drawing.Point(751, 32);
            this.textureFileDataGridViewControls.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textureFileDataGridViewControls.MinimumSize = new System.Drawing.Size(24, 100);
            this.textureFileDataGridViewControls.Name = "textureFileDataGridViewControls";
            this.textureFileDataGridViewControls.Size = new System.Drawing.Size(24, 467);
            this.textureFileDataGridViewControls.TabIndex = 7;
            // 
            // textureFileDataGridView
            // 
            this.textureFileDataGridView.AllowUserToAddRows = false;
            this.textureFileDataGridView.AllowUserToDragDropRows = false;
            this.textureFileDataGridView.AllowUserToResizeRows = true;
            this.textureFileDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textureFileDataGridView.AutoGenerateColumns = false;
            this.textureFileDataGridView.ColumnHeadersHeight = 17;
            this.textureFileDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.textureFileDataGridViewPreviewColumn,
            this.textureFileDataGridViewPathColumn,
            this.textureFileDataGridViewSearchColumn,
            this.textureFileDataGridViewShowColumn,
            this.textureFileDataGridViewMessageColumn,
            this.textureFileDataGridViewSizeColumn,
            this.textureFileDataGridViewReplaceMagentaWithTransparencyColumn,
            this.textureFileDataGridViewConvert512PixelsToDoubleRowsColumn});
            this.textureFileDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.textureFileDataGridView.Location = new System.Drawing.Point(6, 32);
            this.textureFileDataGridView.Name = "textureFileDataGridView";
            this.textureFileDataGridView.RowHeadersWidth = 41;
            this.textureFileDataGridView.RowTemplate.Height = 40;
            this.textureFileDataGridView.Size = new System.Drawing.Size(739, 467);
            this.textureFileDataGridView.TabIndex = 6;
            this.textureFileDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.textureFileDataGridView_CellContentClick);
            this.textureFileDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.textureFileDataGridView_CellFormatting);
            // 
            // textureFileDataGridViewPreviewColumn
            // 
            this.textureFileDataGridViewPreviewColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.textureFileDataGridViewPreviewColumn.HeaderText = "Preview";
            this.textureFileDataGridViewPreviewColumn.Name = "textureFileDataGridViewPreviewColumn";
            this.textureFileDataGridViewPreviewColumn.ReadOnly = true;
            this.textureFileDataGridViewPreviewColumn.Width = 60;
            // 
            // textureFileDataGridViewPathColumn
            // 
            this.textureFileDataGridViewPathColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.textureFileDataGridViewPathColumn.DataPropertyName = "Path";
            this.textureFileDataGridViewPathColumn.HeaderText = "Path";
            this.textureFileDataGridViewPathColumn.Name = "textureFileDataGridViewPathColumn";
            // 
            // textureFileDataGridViewSearchColumn
            // 
            this.textureFileDataGridViewSearchColumn.HeaderText = "";
            this.textureFileDataGridViewSearchColumn.Name = "textureFileDataGridViewSearchColumn";
            this.textureFileDataGridViewSearchColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.textureFileDataGridViewSearchColumn.Text = "Browse";
            this.textureFileDataGridViewSearchColumn.Width = 60;
            // 
            // textureFileDataGridViewShowColumn
            // 
            this.textureFileDataGridViewShowColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.textureFileDataGridViewShowColumn.FillWeight = 45F;
            this.textureFileDataGridViewShowColumn.HeaderText = "Show";
            this.textureFileDataGridViewShowColumn.Name = "textureFileDataGridViewShowColumn";
            this.textureFileDataGridViewShowColumn.Text = "◀";
            this.textureFileDataGridViewShowColumn.Width = 45;
            // 
            // textureFileDataGridViewMessageColumn
            // 
            this.textureFileDataGridViewMessageColumn.DataPropertyName = "Message";
            this.textureFileDataGridViewMessageColumn.HeaderText = "Message";
            this.textureFileDataGridViewMessageColumn.Name = "textureFileDataGridViewMessageColumn";
            this.textureFileDataGridViewMessageColumn.ReadOnly = true;
            this.textureFileDataGridViewMessageColumn.Width = 150;
            // 
            // textureFileDataGridViewSizeColumn
            // 
            this.textureFileDataGridViewSizeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.textureFileDataGridViewSizeColumn.DataPropertyName = "Size";
            this.textureFileDataGridViewSizeColumn.HeaderText = "Size";
            this.textureFileDataGridViewSizeColumn.Name = "textureFileDataGridViewSizeColumn";
            this.textureFileDataGridViewSizeColumn.ReadOnly = true;
            this.textureFileDataGridViewSizeColumn.Width = 51;
            // 
            // textureFileDataGridViewReplaceMagentaWithTransparencyColumn
            // 
            this.textureFileDataGridViewReplaceMagentaWithTransparencyColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.textureFileDataGridViewReplaceMagentaWithTransparencyColumn.DataPropertyName = "ReplaceMagentaWithTransparency";
            this.textureFileDataGridViewReplaceMagentaWithTransparencyColumn.HeaderText = "Magenta to alpha";
            this.textureFileDataGridViewReplaceMagentaWithTransparencyColumn.Name = "textureFileDataGridViewReplaceMagentaWithTransparencyColumn";
            this.textureFileDataGridViewReplaceMagentaWithTransparencyColumn.ReadOnly = true;
            this.textureFileDataGridViewReplaceMagentaWithTransparencyColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.textureFileDataGridViewReplaceMagentaWithTransparencyColumn.Width = 102;
            // 
            // textureFileDataGridViewConvert512PixelsToDoubleRowsColumn
            // 
            this.textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.DataPropertyName = "Convert512PixelsToDoubleRows";
            this.textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.HeaderText = "Set width of 512 to 256";
            this.textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.Name = "textureFileDataGridViewConvert512PixelsToDoubleRowsColumn";
            this.textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.ReadOnly = true;
            this.textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.Width = 127;
            // 
            // darkLabel4
            // 
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(3, 3);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(406, 26);
            this.darkLabel4.TabIndex = 5;
            this.darkLabel4.Text = "List of texture resources (eg *.png, *.tga, *.bmp).\r\nOnly the areas used in the t" +
    "extures will be compiled into the playable level file.";
            // 
            // tabObjects
            // 
            this.tabObjects.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabObjects.Controls.Add(this.objectFileDataGridViewControls);
            this.tabObjects.Controls.Add(this.objectFileDataGridView);
            this.tabObjects.Controls.Add(this.darkLabel5);
            this.tabObjects.Location = new System.Drawing.Point(4, 22);
            this.tabObjects.Name = "tabObjects";
            this.tabObjects.Padding = new System.Windows.Forms.Padding(3);
            this.tabObjects.Size = new System.Drawing.Size(778, 505);
            this.tabObjects.TabIndex = 0;
            this.tabObjects.Text = "Object Files";
            // 
            // objectFileDataGridViewControls
            // 
            this.objectFileDataGridViewControls.AllowUserDelete = false;
            this.objectFileDataGridViewControls.AllowUserMove = false;
            this.objectFileDataGridViewControls.AllowUserNew = false;
            this.objectFileDataGridViewControls.AlwaysInsertAtZero = true;
            this.objectFileDataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectFileDataGridViewControls.Enabled = false;
            this.objectFileDataGridViewControls.Location = new System.Drawing.Point(751, 32);
            this.objectFileDataGridViewControls.MinimumSize = new System.Drawing.Size(24, 100);
            this.objectFileDataGridViewControls.Name = "objectFileDataGridViewControls";
            this.objectFileDataGridViewControls.Size = new System.Drawing.Size(24, 467);
            this.objectFileDataGridViewControls.TabIndex = 5;
            // 
            // objectFileDataGridView
            // 
            this.objectFileDataGridView.AllowUserToAddRows = false;
            this.objectFileDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectFileDataGridView.AutoGenerateColumns = false;
            this.objectFileDataGridView.ColumnHeadersHeight = 17;
            this.objectFileDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.objectFileDataGridViewPathColumn,
            this.objectFileDataGridViewSearchColumn,
            this.objectFileDataGridViewShowColumn,
            this.objectFileDataGridViewMessageColumn});
            this.objectFileDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.objectFileDataGridView.Location = new System.Drawing.Point(6, 32);
            this.objectFileDataGridView.Name = "objectFileDataGridView";
            this.objectFileDataGridView.RowHeadersWidth = 41;
            this.objectFileDataGridView.Size = new System.Drawing.Size(739, 467);
            this.objectFileDataGridView.TabIndex = 4;
            this.objectFileDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.objectFileDataGridView_CellContentClick);
            this.objectFileDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.objectFileDataGridView_CellFormatting);
            // 
            // objectFileDataGridViewPathColumn
            // 
            this.objectFileDataGridViewPathColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.objectFileDataGridViewPathColumn.DataPropertyName = "Path";
            this.objectFileDataGridViewPathColumn.FillWeight = 60F;
            this.objectFileDataGridViewPathColumn.HeaderText = "Path";
            this.objectFileDataGridViewPathColumn.Name = "objectFileDataGridViewPathColumn";
            // 
            // objectFileDataGridViewSearchColumn
            // 
            this.objectFileDataGridViewSearchColumn.HeaderText = "";
            this.objectFileDataGridViewSearchColumn.Name = "objectFileDataGridViewSearchColumn";
            this.objectFileDataGridViewSearchColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.objectFileDataGridViewSearchColumn.Text = "Browse";
            this.objectFileDataGridViewSearchColumn.Width = 80;
            // 
            // objectFileDataGridViewShowColumn
            // 
            this.objectFileDataGridViewShowColumn.HeaderText = "Show";
            this.objectFileDataGridViewShowColumn.Name = "objectFileDataGridViewShowColumn";
            this.objectFileDataGridViewShowColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.objectFileDataGridViewShowColumn.Text = "◀";
            this.objectFileDataGridViewShowColumn.Width = 45;
            // 
            // objectFileDataGridViewMessageColumn
            // 
            this.objectFileDataGridViewMessageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.objectFileDataGridViewMessageColumn.DataPropertyName = "Message";
            this.objectFileDataGridViewMessageColumn.FillWeight = 40F;
            this.objectFileDataGridViewMessageColumn.HeaderText = "Message";
            this.objectFileDataGridViewMessageColumn.Name = "objectFileDataGridViewMessageColumn";
            this.objectFileDataGridViewMessageColumn.ReadOnly = true;
            this.objectFileDataGridViewMessageColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // darkLabel5
            // 
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(3, 3);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(408, 26);
            this.darkLabel5.TabIndex = 1;
            this.darkLabel5.Text = "List of object resources (eg *.wad2, *.wad).\r\nObjects inside the files mentioned " +
    "earlier in the list take priority over later files.";
            // 
            // tabImportedGeometry
            // 
            this.tabImportedGeometry.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabImportedGeometry.Controls.Add(this.importedGeometryManager);
            this.tabImportedGeometry.Controls.Add(this.darkLabel11);
            this.tabImportedGeometry.Location = new System.Drawing.Point(4, 22);
            this.tabImportedGeometry.Name = "tabImportedGeometry";
            this.tabImportedGeometry.Padding = new System.Windows.Forms.Padding(3);
            this.tabImportedGeometry.Size = new System.Drawing.Size(778, 505);
            this.tabImportedGeometry.TabIndex = 3;
            this.tabImportedGeometry.Text = "Imported Geometry";
            // 
            // importedGeometryManager
            // 
            this.importedGeometryManager.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.importedGeometryManager.LevelSettings = null;
            this.importedGeometryManager.Location = new System.Drawing.Point(6, 19);
            this.importedGeometryManager.Name = "importedGeometryManager";
            this.importedGeometryManager.SelectedImportedGeometry = null;
            this.importedGeometryManager.Size = new System.Drawing.Size(772, 480);
            this.importedGeometryManager.TabIndex = 2;
            // 
            // darkLabel11
            // 
            this.darkLabel11.AutoSize = true;
            this.darkLabel11.Dock = System.Windows.Forms.DockStyle.Top;
            this.darkLabel11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel11.Location = new System.Drawing.Point(3, 3);
            this.darkLabel11.Name = "darkLabel11";
            this.darkLabel11.Size = new System.Drawing.Size(276, 13);
            this.darkLabel11.TabIndex = 1;
            this.darkLabel11.Text = "All imported geometries associated with this project:";
            // 
            // tabStaticMeshes
            // 
            this.tabStaticMeshes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabStaticMeshes.Controls.Add(this.butSelectAllButShatterStatics);
            this.tabStaticMeshes.Controls.Add(this.butDeselectAllStatics);
            this.tabStaticMeshes.Controls.Add(this.butSelectAllStatics);
            this.tabStaticMeshes.Controls.Add(this.darkLabel19);
            this.tabStaticMeshes.Controls.Add(this.staticMeshMergeDataGridView);
            this.tabStaticMeshes.Controls.Add(this.darkLabel18);
            this.tabStaticMeshes.Controls.Add(this.darkLabel17);
            this.tabStaticMeshes.Location = new System.Drawing.Point(4, 22);
            this.tabStaticMeshes.Name = "tabStaticMeshes";
            this.tabStaticMeshes.Padding = new System.Windows.Forms.Padding(3);
            this.tabStaticMeshes.Size = new System.Drawing.Size(778, 505);
            this.tabStaticMeshes.TabIndex = 8;
            this.tabStaticMeshes.Text = "Static Meshes";
            // 
            // butSelectAllButShatterStatics
            // 
            this.butSelectAllButShatterStatics.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSelectAllButShatterStatics.Checked = false;
            this.butSelectAllButShatterStatics.Location = new System.Drawing.Point(469, 408);
            this.butSelectAllButShatterStatics.Name = "butSelectAllButShatterStatics";
            this.butSelectAllButShatterStatics.Size = new System.Drawing.Size(134, 22);
            this.butSelectAllButShatterStatics.TabIndex = 111;
            this.butSelectAllButShatterStatics.Text = "Select all but shatters";
            this.butSelectAllButShatterStatics.Click += new System.EventHandler(this.butSelectAllButShatterStatics_Click);
            // 
            // butDeselectAllStatics
            // 
            this.butDeselectAllStatics.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butDeselectAllStatics.Checked = false;
            this.butDeselectAllStatics.Location = new System.Drawing.Point(695, 408);
            this.butDeselectAllStatics.Name = "butDeselectAllStatics";
            this.butDeselectAllStatics.Size = new System.Drawing.Size(80, 22);
            this.butDeselectAllStatics.TabIndex = 110;
            this.butDeselectAllStatics.Text = "Deselect all";
            this.butDeselectAllStatics.Click += new System.EventHandler(this.butDeselectAllStatics_Click);
            // 
            // butSelectAllStatics
            // 
            this.butSelectAllStatics.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSelectAllStatics.Checked = false;
            this.butSelectAllStatics.Location = new System.Drawing.Point(609, 408);
            this.butSelectAllStatics.Name = "butSelectAllStatics";
            this.butSelectAllStatics.Size = new System.Drawing.Size(80, 22);
            this.butSelectAllStatics.TabIndex = 109;
            this.butSelectAllStatics.Text = "Select all";
            this.butSelectAllStatics.Click += new System.EventHandler(this.butSelectAllStatics_Click);
            // 
            // darkLabel19
            // 
            this.darkLabel19.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.darkLabel19.ForeColor = System.Drawing.Color.Silver;
            this.darkLabel19.Location = new System.Drawing.Point(3, 438);
            this.darkLabel19.Name = "darkLabel19";
            this.darkLabel19.Size = new System.Drawing.Size(772, 32);
            this.darkLabel19.TabIndex = 4;
            this.darkLabel19.Text = "If \'Interpret shades as effect\' value is set, static mesh vertex shades are inter" +
    "preted as follows:\r\n0 = No Effect, 1-14 = Glow, 15-31 = Movement/Glow.";
            // 
            // staticMeshMergeDataGridView
            // 
            this.staticMeshMergeDataGridView.AllowUserToAddRows = false;
            this.staticMeshMergeDataGridView.AllowUserToDeleteRows = false;
            this.staticMeshMergeDataGridView.AllowUserToDragDropRows = false;
            this.staticMeshMergeDataGridView.AllowUserToPasteCells = false;
            this.staticMeshMergeDataGridView.AllowUserToResizeColumns = false;
            this.staticMeshMergeDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.staticMeshMergeDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.staticMeshMergeDataGridView.ColumnHeadersHeight = 17;
            this.staticMeshMergeDataGridView.ColumnHeadersHeight = 19;
            this.staticMeshMergeDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMeshName,
            this.colMergeStatics,
            this.colInterpretShadesAsEffect,
            this.colTintAsAmbient,
            this.colClearShades});
            this.staticMeshMergeDataGridView.DisableSelection = true;
            this.staticMeshMergeDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.staticMeshMergeDataGridView.Location = new System.Drawing.Point(6, 19);
            this.staticMeshMergeDataGridView.MultiSelect = false;
            this.staticMeshMergeDataGridView.Name = "staticMeshMergeDataGridView";
            this.staticMeshMergeDataGridView.RowHeadersWidth = 41;
            this.staticMeshMergeDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.staticMeshMergeDataGridView.Size = new System.Drawing.Size(769, 383);
            this.staticMeshMergeDataGridView.TabIndex = 2;
            // 
            // colMeshName
            // 
            this.colMeshName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colMeshName.DataPropertyName = "StaticMesh";
            this.colMeshName.FillWeight = 40F;
            this.colMeshName.HeaderText = "Mesh name";
            this.colMeshName.Name = "colMeshName";
            this.colMeshName.ReadOnly = true;
            // 
            // colMergeStatics
            // 
            this.colMergeStatics.DataPropertyName = "Merge";
            this.colMergeStatics.FillWeight = 20F;
            this.colMergeStatics.HeaderText = "Merge";
            this.colMergeStatics.Name = "colMergeStatics";
            this.colMergeStatics.ReadOnly = true;
            this.colMergeStatics.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colMergeStatics.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colMergeStatics.ToolTipText = "Whether to merge the Mesh";
            // 
            // colInterpretShadesAsEffect
            // 
            this.colInterpretShadesAsEffect.DataPropertyName = "InterpretShadesAsEffect";
            this.colInterpretShadesAsEffect.FillWeight = 50F;
            this.colInterpretShadesAsEffect.HeaderText = "Interpret shades as effect";
            this.colInterpretShadesAsEffect.Name = "colInterpretShadesAsEffect";
            this.colInterpretShadesAsEffect.ReadOnly = true;
            this.colInterpretShadesAsEffect.ToolTipText = "See description below";
            // 
            // colTintAsAmbient
            // 
            this.colTintAsAmbient.DataPropertyName = "TintAsAmbient";
            this.colTintAsAmbient.FillWeight = 60F;
            this.colTintAsAmbient.HeaderText = "Use tint as ambient color";
            this.colTintAsAmbient.Name = "colTintAsAmbient";
            this.colTintAsAmbient.ReadOnly = true;
            this.colTintAsAmbient.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colTintAsAmbient.ToolTipText = "Uses the Static Mesh\'s Tint and uses it as the base for lighting";
            // 
            // colClearShades
            // 
            this.colClearShades.DataPropertyName = "ClearShades";
            this.colClearShades.FillWeight = 50F;
            this.colClearShades.HeaderText = "Clear vertex shades";
            this.colClearShades.Name = "colClearShades";
            this.colClearShades.ReadOnly = true;
            this.colClearShades.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colClearShades.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colClearShades.ToolTipText = "The shades of the mesh are ignored for lighting calculation";
            // 
            // darkLabel18
            // 
            this.darkLabel18.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.darkLabel18.ForeColor = System.Drawing.Color.Silver;
            this.darkLabel18.Location = new System.Drawing.Point(3, 470);
            this.darkLabel18.Name = "darkLabel18";
            this.darkLabel18.Size = new System.Drawing.Size(772, 32);
            this.darkLabel18.TabIndex = 1;
            this.darkLabel18.Text = resources.GetString("darkLabel18.Text");
            // 
            // darkLabel17
            // 
            this.darkLabel17.AutoSize = true;
            this.darkLabel17.Dock = System.Windows.Forms.DockStyle.Top;
            this.darkLabel17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel17.Location = new System.Drawing.Point(3, 3);
            this.darkLabel17.Name = "darkLabel17";
            this.darkLabel17.Size = new System.Drawing.Size(386, 13);
            this.darkLabel17.Size = new System.Drawing.Size(388, 13);
            this.darkLabel17.TabIndex = 0;
            this.darkLabel17.Text = "Static meshes which should be automatically merged with room geometry:";
            // 
            // tabSkyAndFont
            // 
            this.tabSkyAndFont.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabSkyAndFont.Controls.Add(this.panelTr5Sprites);
            this.tabSkyAndFont.Controls.Add(this.panelFont);
            this.tabSkyAndFont.Controls.Add(this.panelSky);
            this.tabSkyAndFont.Location = new System.Drawing.Point(4, 22);
            this.tabSkyAndFont.Name = "tabSkyAndFont";
            this.tabSkyAndFont.Padding = new System.Windows.Forms.Padding(3);
            this.tabSkyAndFont.Size = new System.Drawing.Size(778, 505);
            this.tabSkyAndFont.TabIndex = 1;
            this.tabSkyAndFont.Text = "Sky & Font";
            // 
            // panelTr5Sprites
            // 
            this.panelTr5Sprites.Controls.Add(this.tr5SpritesTextureFilePathPicPreview);
            this.panelTr5Sprites.Controls.Add(this.tr5SpritesTextureFilePathBut);
            this.panelTr5Sprites.Controls.Add(this.tr5SpritesFilePathOptCustom);
            this.panelTr5Sprites.Controls.Add(this.lblTr5ExtraSprites);
            this.panelTr5Sprites.Controls.Add(this.tr5SpritesFilePathOptAuto);
            this.panelTr5Sprites.Controls.Add(this.tr5SpritesTextureFilePathTxt);
            this.panelTr5Sprites.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTr5Sprites.Location = new System.Drawing.Point(3, 142);
            this.panelTr5Sprites.Name = "panelTr5Sprites";
            this.panelTr5Sprites.Size = new System.Drawing.Size(772, 68);
            this.panelTr5Sprites.TabIndex = 3;
            // 
            // tr5SpritesTextureFilePathPicPreview
            // 
            this.tr5SpritesTextureFilePathPicPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tr5SpritesTextureFilePathPicPreview.BackColor = System.Drawing.Color.Gray;
            this.tr5SpritesTextureFilePathPicPreview.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tr5SpritesTextureFilePathPicPreview.BackgroundImage")));
            this.tr5SpritesTextureFilePathPicPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tr5SpritesTextureFilePathPicPreview.Location = new System.Drawing.Point(639, 2);
            this.tr5SpritesTextureFilePathPicPreview.Name = "tr5SpritesTextureFilePathPicPreview";
            this.tr5SpritesTextureFilePathPicPreview.Size = new System.Drawing.Size(32, 32);
            this.tr5SpritesTextureFilePathPicPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.tr5SpritesTextureFilePathPicPreview.TabIndex = 6;
            this.tr5SpritesTextureFilePathPicPreview.TabStop = false;
            // 
            // tr5SpritesTextureFilePathBut
            // 
            this.tr5SpritesTextureFilePathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tr5SpritesTextureFilePathBut.Checked = false;
            this.tr5SpritesTextureFilePathBut.Location = new System.Drawing.Point(677, 40);
            this.tr5SpritesTextureFilePathBut.Name = "tr5SpritesTextureFilePathBut";
            this.tr5SpritesTextureFilePathBut.Size = new System.Drawing.Size(92, 22);
            this.tr5SpritesTextureFilePathBut.TabIndex = 3;
            this.tr5SpritesTextureFilePathBut.Text = "Browse";
            this.tr5SpritesTextureFilePathBut.Click += new System.EventHandler(this.tr5SpritesTextureFilePathBut_Click);
            // 
            // tr5SpritesFilePathOptCustom
            // 
            this.tr5SpritesFilePathOptCustom.Location = new System.Drawing.Point(19, 42);
            this.tr5SpritesFilePathOptCustom.Name = "tr5SpritesFilePathOptCustom";
            this.tr5SpritesFilePathOptCustom.Size = new System.Drawing.Size(162, 17);
            this.tr5SpritesFilePathOptCustom.TabIndex = 5;
            this.tr5SpritesFilePathOptCustom.TabStop = true;
            this.tr5SpritesFilePathOptCustom.Text = "Custom file (has to be 256²)";
            // 
            // lblTr5ExtraSprites
            // 
            this.lblTr5ExtraSprites.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblTr5ExtraSprites.Location = new System.Drawing.Point(0, 0);
            this.lblTr5ExtraSprites.Name = "lblTr5ExtraSprites";
            this.lblTr5ExtraSprites.Size = new System.Drawing.Size(381, 17);
            this.lblTr5ExtraSprites.TabIndex = 1;
            this.lblTr5ExtraSprites.Text = "TR5 extra sprites texture";
            // 
            // tr5SpritesFilePathOptAuto
            // 
            this.tr5SpritesFilePathOptAuto.Checked = true;
            this.tr5SpritesFilePathOptAuto.Location = new System.Drawing.Point(19, 19);
            this.tr5SpritesFilePathOptAuto.Name = "tr5SpritesFilePathOptAuto";
            this.tr5SpritesFilePathOptAuto.Size = new System.Drawing.Size(450, 17);
            this.tr5SpritesFilePathOptAuto.TabIndex = 4;
            this.tr5SpritesFilePathOptAuto.TabStop = true;
            this.tr5SpritesFilePathOptAuto.Text = "Use default \'Extra.Tr5.pc\' file";
            this.tr5SpritesFilePathOptAuto.CheckedChanged += new System.EventHandler(this.tr5SpritesFilePathOptAuto_CheckedChanged);
            // 
            // tr5SpritesTextureFilePathTxt
            // 
            this.tr5SpritesTextureFilePathTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tr5SpritesTextureFilePathTxt.Enabled = false;
            this.tr5SpritesTextureFilePathTxt.Location = new System.Drawing.Point(187, 40);
            this.tr5SpritesTextureFilePathTxt.Name = "tr5SpritesTextureFilePathTxt";
            this.tr5SpritesTextureFilePathTxt.Size = new System.Drawing.Size(484, 22);
            this.tr5SpritesTextureFilePathTxt.TabIndex = 2;
            this.tr5SpritesTextureFilePathTxt.TextChanged += new System.EventHandler(this.tr5SpritesTextureFilePathTxt_TextChanged);
            // 
            // panelFont
            // 
            this.panelFont.Controls.Add(this.fontTextureFilePathPicPreview);
            this.panelFont.Controls.Add(this.fontTextureFilePathBut);
            this.panelFont.Controls.Add(this.fontTextureFilePathOptCustom);
            this.panelFont.Controls.Add(this.darkLabel8);
            this.panelFont.Controls.Add(this.fontTextureFilePathOptAuto);
            this.panelFont.Controls.Add(this.fontTextureFilePathTxt);
            this.panelFont.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFont.Location = new System.Drawing.Point(3, 74);
            this.panelFont.Name = "panelFont";
            this.panelFont.Size = new System.Drawing.Size(772, 68);
            this.panelFont.TabIndex = 2;
            // 
            // fontTextureFilePathPicPreview
            // 
            this.fontTextureFilePathPicPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fontTextureFilePathPicPreview.BackColor = System.Drawing.Color.Gray;
            this.fontTextureFilePathPicPreview.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("fontTextureFilePathPicPreview.BackgroundImage")));
            this.fontTextureFilePathPicPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fontTextureFilePathPicPreview.Location = new System.Drawing.Point(639, 2);
            this.fontTextureFilePathPicPreview.Name = "fontTextureFilePathPicPreview";
            this.fontTextureFilePathPicPreview.Size = new System.Drawing.Size(32, 32);
            this.fontTextureFilePathPicPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.fontTextureFilePathPicPreview.TabIndex = 6;
            this.fontTextureFilePathPicPreview.TabStop = false;
            // 
            // fontTextureFilePathBut
            // 
            this.fontTextureFilePathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fontTextureFilePathBut.Checked = false;
            this.fontTextureFilePathBut.Location = new System.Drawing.Point(677, 40);
            this.fontTextureFilePathBut.Name = "fontTextureFilePathBut";
            this.fontTextureFilePathBut.Size = new System.Drawing.Size(92, 22);
            this.fontTextureFilePathBut.TabIndex = 3;
            this.fontTextureFilePathBut.Text = "Browse";
            this.fontTextureFilePathBut.Click += new System.EventHandler(this.fontTextureFilePathBut_Click);
            // 
            // fontTextureFilePathOptCustom
            // 
            this.fontTextureFilePathOptCustom.Location = new System.Drawing.Point(19, 42);
            this.fontTextureFilePathOptCustom.Name = "fontTextureFilePathOptCustom";
            this.fontTextureFilePathOptCustom.Size = new System.Drawing.Size(162, 17);
            this.fontTextureFilePathOptCustom.TabIndex = 5;
            this.fontTextureFilePathOptCustom.TabStop = true;
            this.fontTextureFilePathOptCustom.Text = "Custom file (has to be 256²)";
            // 
            // darkLabel8
            // 
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(0, 0);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(381, 17);
            this.darkLabel8.TabIndex = 1;
            this.darkLabel8.Text = "Font texture (\'Font.pc\' in the official editor):";
            // 
            // fontTextureFilePathOptAuto
            // 
            this.fontTextureFilePathOptAuto.Checked = true;
            this.fontTextureFilePathOptAuto.Location = new System.Drawing.Point(19, 19);
            this.fontTextureFilePathOptAuto.Name = "fontTextureFilePathOptAuto";
            this.fontTextureFilePathOptAuto.Size = new System.Drawing.Size(450, 17);
            this.fontTextureFilePathOptAuto.TabIndex = 4;
            this.fontTextureFilePathOptAuto.TabStop = true;
            this.fontTextureFilePathOptAuto.Text = "Use default \'Font.pc\' file";
            this.fontTextureFilePathOptAuto.CheckedChanged += new System.EventHandler(this.fontTextureFilePathOptAuto_CheckedChanged);
            // 
            // fontTextureFilePathTxt
            // 
            this.fontTextureFilePathTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fontTextureFilePathTxt.Enabled = false;
            this.fontTextureFilePathTxt.Location = new System.Drawing.Point(187, 40);
            this.fontTextureFilePathTxt.Name = "fontTextureFilePathTxt";
            this.fontTextureFilePathTxt.Size = new System.Drawing.Size(484, 22);
            this.fontTextureFilePathTxt.TabIndex = 2;
            this.fontTextureFilePathTxt.TextChanged += new System.EventHandler(this.fontTextureFilePathTxt_TextChanged);
            // 
            // panelSky
            // 
            this.panelSky.Controls.Add(this.skyTextureFilePathPicPreview);
            this.panelSky.Controls.Add(this.skyTextureFilePathBut);
            this.panelSky.Controls.Add(this.skyTextureFilePathOptCustom);
            this.panelSky.Controls.Add(this.darkLabel9);
            this.panelSky.Controls.Add(this.skyTextureFilePathOptAuto);
            this.panelSky.Controls.Add(this.skyTextureFilePathTxt);
            this.panelSky.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSky.Location = new System.Drawing.Point(3, 3);
            this.panelSky.Name = "panelSky";
            this.panelSky.Size = new System.Drawing.Size(772, 71);
            this.panelSky.TabIndex = 2;
            // 
            // skyTextureFilePathPicPreview
            // 
            this.skyTextureFilePathPicPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.skyTextureFilePathPicPreview.BackColor = System.Drawing.Color.Gray;
            this.skyTextureFilePathPicPreview.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("skyTextureFilePathPicPreview.BackgroundImage")));
            this.skyTextureFilePathPicPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.skyTextureFilePathPicPreview.Location = new System.Drawing.Point(639, 2);
            this.skyTextureFilePathPicPreview.Name = "skyTextureFilePathPicPreview";
            this.skyTextureFilePathPicPreview.Size = new System.Drawing.Size(32, 32);
            this.skyTextureFilePathPicPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.skyTextureFilePathPicPreview.TabIndex = 6;
            this.skyTextureFilePathPicPreview.TabStop = false;
            // 
            // skyTextureFilePathBut
            // 
            this.skyTextureFilePathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.skyTextureFilePathBut.Checked = false;
            this.skyTextureFilePathBut.Location = new System.Drawing.Point(677, 40);
            this.skyTextureFilePathBut.Name = "skyTextureFilePathBut";
            this.skyTextureFilePathBut.Size = new System.Drawing.Size(92, 22);
            this.skyTextureFilePathBut.TabIndex = 3;
            this.skyTextureFilePathBut.Text = "Browse";
            this.skyTextureFilePathBut.Click += new System.EventHandler(this.skyTextureFilePathBut_Click);
            // 
            // skyTextureFilePathOptCustom
            // 
            this.skyTextureFilePathOptCustom.Location = new System.Drawing.Point(19, 42);
            this.skyTextureFilePathOptCustom.Name = "skyTextureFilePathOptCustom";
            this.skyTextureFilePathOptCustom.Size = new System.Drawing.Size(162, 17);
            this.skyTextureFilePathOptCustom.TabIndex = 5;
            this.skyTextureFilePathOptCustom.TabStop = true;
            this.skyTextureFilePathOptCustom.Text = "Custom file (has to be 256²)";
            // 
            // darkLabel9
            // 
            this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel9.Location = new System.Drawing.Point(0, 0);
            this.darkLabel9.Name = "darkLabel9";
            this.darkLabel9.Size = new System.Drawing.Size(381, 17);
            this.darkLabel9.TabIndex = 1;
            this.darkLabel9.Text = "Sky texture (\'pcsky.raw\' in the official editor):     ";
            // 
            // skyTextureFilePathOptAuto
            // 
            this.skyTextureFilePathOptAuto.Checked = true;
            this.skyTextureFilePathOptAuto.Location = new System.Drawing.Point(19, 19);
            this.skyTextureFilePathOptAuto.Name = "skyTextureFilePathOptAuto";
            this.skyTextureFilePathOptAuto.Size = new System.Drawing.Size(450, 17);
            this.skyTextureFilePathOptAuto.TabIndex = 4;
            this.skyTextureFilePathOptAuto.TabStop = true;
            this.skyTextureFilePathOptAuto.Text = "Use default \'pcsky.raw\' file";
            this.skyTextureFilePathOptAuto.CheckedChanged += new System.EventHandler(this.skyTextureFilePathOptAuto_CheckedChanged);
            // 
            // skyTextureFilePathTxt
            // 
            this.skyTextureFilePathTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.skyTextureFilePathTxt.Enabled = false;
            this.skyTextureFilePathTxt.Location = new System.Drawing.Point(187, 40);
            this.skyTextureFilePathTxt.Name = "skyTextureFilePathTxt";
            this.skyTextureFilePathTxt.Size = new System.Drawing.Size(484, 22);
            this.skyTextureFilePathTxt.TabIndex = 2;
            this.skyTextureFilePathTxt.TextChanged += new System.EventHandler(this.skyTextureFilePathTxt_TextChanged);
            // 
            // tabSoundsCatalogs
            // 
            this.tabSoundsCatalogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabSoundsCatalogs.Controls.Add(this.cbAutodetectIfNoneSelected);
            this.tabSoundsCatalogs.Controls.Add(this.butAssignFromWads);
            this.tabSoundsCatalogs.Controls.Add(this.butRemoveMissing);
            this.tabSoundsCatalogs.Controls.Add(this.darkLabel20);
            this.tabSoundsCatalogs.Controls.Add(this.labelSoundsCatalogsStatistics);
            this.tabSoundsCatalogs.Controls.Add(this.butSearchSounds);
            this.tabSoundsCatalogs.Controls.Add(this.butAssignFromSoundSources);
            this.tabSoundsCatalogs.Controls.Add(this.tbFilterSounds);
            this.tabSoundsCatalogs.Controls.Add(this.butAssignSoundsFromSelectedCatalogs);
            this.tabSoundsCatalogs.Controls.Add(this.butAssignHardcodedSounds);
            this.tabSoundsCatalogs.Controls.Add(this.butAutodetectSoundsAndAssign);
            this.tabSoundsCatalogs.Controls.Add(this.butDeselectAllSounds);
            this.tabSoundsCatalogs.Controls.Add(this.butSelectAllSounds);
            this.tabSoundsCatalogs.Controls.Add(this.lblCatalogsPrompt);
            this.tabSoundsCatalogs.Controls.Add(this.soundsCatalogsDataGridView);
            this.tabSoundsCatalogs.Controls.Add(this.soundsCatalogsDataGridViewControls);
            this.tabSoundsCatalogs.Controls.Add(this.darkLabel50);
            this.tabSoundsCatalogs.Controls.Add(this.selectedSoundsDataGridView);
            this.tabSoundsCatalogs.Location = new System.Drawing.Point(4, 22);
            this.tabSoundsCatalogs.Name = "tabSoundsCatalogs";
            this.tabSoundsCatalogs.Padding = new System.Windows.Forms.Padding(3);
            this.tabSoundsCatalogs.Size = new System.Drawing.Size(778, 505);
            this.tabSoundsCatalogs.TabIndex = 8;
            this.tabSoundsCatalogs.Text = "Sound Infos";
            // 
            // cbAutodetectIfNoneSelected
            // 
            this.cbAutodetectIfNoneSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAutodetectIfNoneSelected.AutoSize = true;
            this.cbAutodetectIfNoneSelected.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbAutodetectIfNoneSelected.Location = new System.Drawing.Point(492, 222);
            this.cbAutodetectIfNoneSelected.Name = "cbAutodetectIfNoneSelected";
            this.cbAutodetectIfNoneSelected.Size = new System.Drawing.Size(284, 17);
            this.cbAutodetectIfNoneSelected.TabIndex = 117;
            this.cbAutodetectIfNoneSelected.Text = "Autodetect sounds on compilation if none selected";
            this.cbAutodetectIfNoneSelected.CheckedChanged += new System.EventHandler(this.cbAutodetectIfNoneSelected_CheckedChanged);
            // 
            // darkLabel20
            // 
            this.darkLabel20.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel20.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel20.Location = new System.Drawing.Point(370, 482);
            this.darkLabel20.Name = "darkLabel20";
            this.darkLabel20.Size = new System.Drawing.Size(83, 19);
            this.darkLabel20.TabIndex = 110;
            this.darkLabel20.Text = "Filter by name:";
            // 
            // labelSoundsCatalogsStatistics
            // 
            this.labelSoundsCatalogsStatistics.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelSoundsCatalogsStatistics.AutoSize = true;
            this.labelSoundsCatalogsStatistics.ForeColor = System.Drawing.Color.DarkGray;
            this.labelSoundsCatalogsStatistics.Location = new System.Drawing.Point(6, 482);
            this.labelSoundsCatalogsStatistics.Name = "labelSoundsCatalogsStatistics";
            this.labelSoundsCatalogsStatistics.Size = new System.Drawing.Size(52, 13);
            this.labelSoundsCatalogsStatistics.TabIndex = 105;
            this.labelSoundsCatalogsStatistics.Text = "               ";
            // 
            // butSearchSounds
            // 
            this.butSearchSounds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSearchSounds.Checked = false;
            this.butSearchSounds.Image = global::TombEditor.Properties.Resources.general_filter_16;
            this.butSearchSounds.Location = new System.Drawing.Point(654, 480);
            this.butSearchSounds.Name = "butSearchSounds";
            this.butSearchSounds.Selectable = false;
            this.butSearchSounds.Size = new System.Drawing.Size(24, 22);
            this.butSearchSounds.TabIndex = 109;
            this.butSearchSounds.Click += new System.EventHandler(this.butFilterSounds_Click);
            // 
            // tbFilterSounds
            // 
            this.tbFilterSounds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilterSounds.Location = new System.Drawing.Point(457, 480);
            this.tbFilterSounds.Name = "tbFilterSounds";
            this.tbFilterSounds.Size = new System.Drawing.Size(198, 22);
            this.tbFilterSounds.TabIndex = 100;
            this.tbFilterSounds.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbFilterSounds_KeyDown);
            // 
            // lblCatalogsPrompt
            // 
            this.lblCatalogsPrompt.AutoSize = true;
            this.lblCatalogsPrompt.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCatalogsPrompt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblCatalogsPrompt.Location = new System.Drawing.Point(3, 3);
            this.lblCatalogsPrompt.Name = "lblCatalogsPrompt";
            this.lblCatalogsPrompt.Size = new System.Drawing.Size(460, 26);
            this.lblCatalogsPrompt.TabIndex = 106;
            this.lblCatalogsPrompt.Text = "Sound catalogs (eg *.xml, sounds.txt, *.sfx/*.sam) from which sound infos will be" +
    " loaded.\r\nIf any sound info ID is duplicated in any of catalog, first one will b" +
    "e used.";
            // 
            // soundsCatalogsDataGridView
            // 
            this.soundsCatalogsDataGridView.AllowUserToAddRows = false;
            this.soundsCatalogsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.soundsCatalogsDataGridView.AutoGenerateColumns = false;
            this.soundsCatalogsDataGridView.ColumnHeadersHeight = 17;
            this.soundsCatalogsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SoundsCatalogPathColumn,
            this.SoundsCatalogSearchColumn,
            this.SoundsCatalogReloadButton,
            this.SoundsCatalogEditColumn,
            this.SoundsCatalogsAssignColumn,
            this.SoundsCatalogsSoundCountColumn,
            this.SoundsCatalogMessageColumn});
            this.soundsCatalogsDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.soundsCatalogsDataGridView.Location = new System.Drawing.Point(6, 32);
            this.soundsCatalogsDataGridView.Name = "soundsCatalogsDataGridView";
            this.soundsCatalogsDataGridView.RowHeadersWidth = 41;
            this.soundsCatalogsDataGridView.Size = new System.Drawing.Size(739, 180);
            this.soundsCatalogsDataGridView.TabIndex = 104;
            this.soundsCatalogsDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.soundsCatalogsDataGridView_CellContentClick);
            this.soundsCatalogsDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.soundsCatalogsDataGridView_CellFormatting);
            this.soundsCatalogsDataGridView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.soundsCatalogsDataGridView_CellPainting);
            this.soundsCatalogsDataGridView.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.soundsCatalogsDataGridView_RowsRemoved);
            this.soundsCatalogsDataGridView.Sorted += new System.EventHandler(this.soundsCatalogsDataGridView_Sorted);
            // 
            // SoundsCatalogPathColumn
            // 
            this.SoundsCatalogPathColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SoundsCatalogPathColumn.DataPropertyName = "Path";
            this.SoundsCatalogPathColumn.FillWeight = 60F;
            this.SoundsCatalogPathColumn.HeaderText = "Path";
            this.SoundsCatalogPathColumn.Name = "SoundsCatalogPathColumn";
            this.SoundsCatalogPathColumn.ReadOnly = true;
            // 
            // SoundsCatalogSearchColumn
            // 
            this.SoundsCatalogSearchColumn.HeaderText = "";
            this.SoundsCatalogSearchColumn.Name = "SoundsCatalogSearchColumn";
            this.SoundsCatalogSearchColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.SoundsCatalogSearchColumn.Text = "";
            this.SoundsCatalogSearchColumn.ToolTipText = "Browse";
            this.SoundsCatalogSearchColumn.Width = 24;
            // 
            // SoundsCatalogReloadButton
            // 
            this.SoundsCatalogReloadButton.HeaderText = "";
            this.SoundsCatalogReloadButton.Name = "SoundsCatalogReloadButton";
            this.SoundsCatalogReloadButton.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.SoundsCatalogReloadButton.Text = "";
            this.SoundsCatalogReloadButton.ToolTipText = "Reload this catalog from disk";
            this.SoundsCatalogReloadButton.Width = 24;
            // 
            // SoundsCatalogEditColumn
            // 
            this.SoundsCatalogEditColumn.HeaderText = "";
            this.SoundsCatalogEditColumn.Name = "SoundsCatalogEditColumn";
            this.SoundsCatalogEditColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.SoundsCatalogEditColumn.Text = "";
            this.SoundsCatalogEditColumn.ToolTipText = "Edit in SoundTool";
            this.SoundsCatalogEditColumn.Width = 24;
            // 
            // SoundsCatalogsAssignColumn
            // 
            this.SoundsCatalogsAssignColumn.HeaderText = "";
            this.SoundsCatalogsAssignColumn.Name = "SoundsCatalogsAssignColumn";
            this.SoundsCatalogsAssignColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.SoundsCatalogsAssignColumn.Text = "Include all";
            this.SoundsCatalogsAssignColumn.ToolTipText = "Include all sounds from this catalog";
            this.SoundsCatalogsAssignColumn.Width = 60;
            // 
            // SoundsCatalogsSoundCountColumn
            // 
            this.SoundsCatalogsSoundCountColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SoundsCatalogsSoundCountColumn.DataPropertyName = "SoundsCount";
            this.SoundsCatalogsSoundCountColumn.FillWeight = 20F;
            this.SoundsCatalogsSoundCountColumn.HeaderText = "Num. sounds";
            this.SoundsCatalogsSoundCountColumn.Name = "SoundsCatalogsSoundCountColumn";
            this.SoundsCatalogsSoundCountColumn.ReadOnly = true;
            this.SoundsCatalogsSoundCountColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // SoundsCatalogMessageColumn
            // 
            this.SoundsCatalogMessageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SoundsCatalogMessageColumn.DataPropertyName = "Message";
            this.SoundsCatalogMessageColumn.FillWeight = 40F;
            this.SoundsCatalogMessageColumn.HeaderText = "Message";
            this.SoundsCatalogMessageColumn.Name = "SoundsCatalogMessageColumn";
            this.SoundsCatalogMessageColumn.ReadOnly = true;
            this.SoundsCatalogMessageColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // soundsCatalogsDataGridViewControls
            // 
            this.soundsCatalogsDataGridViewControls.AllowUserDelete = false;
            this.soundsCatalogsDataGridViewControls.AllowUserMove = false;
            this.soundsCatalogsDataGridViewControls.AllowUserNew = false;
            this.soundsCatalogsDataGridViewControls.AlwaysInsertAtZero = false;
            this.soundsCatalogsDataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.soundsCatalogsDataGridViewControls.Enabled = false;
            this.soundsCatalogsDataGridViewControls.Location = new System.Drawing.Point(751, 32);
            this.soundsCatalogsDataGridViewControls.MinimumSize = new System.Drawing.Size(24, 100);
            this.soundsCatalogsDataGridViewControls.Name = "soundsCatalogsDataGridViewControls";
            this.soundsCatalogsDataGridViewControls.Size = new System.Drawing.Size(24, 180);
            this.soundsCatalogsDataGridViewControls.TabIndex = 103;
            // 
            // darkLabel50
            // 
            this.darkLabel50.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel50.Location = new System.Drawing.Point(3, 223);
            this.darkLabel50.Name = "darkLabel50";
            this.darkLabel50.Size = new System.Drawing.Size(196, 19);
            this.darkLabel50.TabIndex = 97;
            this.darkLabel50.Text = "Sound infos to include in level:";
            // 
            // selectedSoundsDataGridView
            // 
            this.selectedSoundsDataGridView.AllowUserToAddRows = false;
            this.selectedSoundsDataGridView.AllowUserToDeleteRows = false;
            this.selectedSoundsDataGridView.AllowUserToDragDropRows = false;
            this.selectedSoundsDataGridView.AllowUserToOrderColumns = true;
            this.selectedSoundsDataGridView.AllowUserToPasteCells = false;
            this.selectedSoundsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectedSoundsDataGridView.ColumnHeadersHeight = 17;
            this.selectedSoundsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSoundsEnabled,
            this.colSoundID,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5});
            this.selectedSoundsDataGridView.DisableSelection = true;
            this.selectedSoundsDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.selectedSoundsDataGridView.Location = new System.Drawing.Point(6, 273);
            this.selectedSoundsDataGridView.MultiSelect = false;
            this.selectedSoundsDataGridView.Name = "selectedSoundsDataGridView";
            this.selectedSoundsDataGridView.RowHeadersWidth = 41;
            this.selectedSoundsDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.selectedSoundsDataGridView.ShowCellErrors = false;
            this.selectedSoundsDataGridView.ShowCellToolTips = false;
            this.selectedSoundsDataGridView.ShowEditingIcon = false;
            this.selectedSoundsDataGridView.ShowRowErrors = false;
            this.selectedSoundsDataGridView.Size = new System.Drawing.Size(769, 201);
            this.selectedSoundsDataGridView.TabIndex = 116;
            this.selectedSoundsDataGridView.ToggleCheckBoxOnClick = true;
            this.selectedSoundsDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.selectedSoundsDataGridView_CellValueChanged);
            // 
            // colSoundsEnabled
            // 
            this.colSoundsEnabled.HeaderText = "";
            this.colSoundsEnabled.Name = "colSoundsEnabled";
            this.colSoundsEnabled.ReadOnly = true;
            this.colSoundsEnabled.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.colSoundsEnabled.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colSoundsEnabled.Width = 40;
            // 
            // colSoundID
            // 
            this.colSoundID.HeaderText = "ID";
            this.colSoundID.Name = "colSoundID";
            this.colSoundID.ReadOnly = true;
            this.colSoundID.Width = 40;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.HeaderText = "Name";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            this.dataGridViewTextBoxColumn2.Width = 180;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn3.HeaderText = "From catalog";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.HeaderText = "Samples";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.ReadOnly = true;
            this.dataGridViewTextBoxColumn6.Width = 70;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "Range";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.ToolTipText = "Range name of TRNG extended soundmap";
            this.dataGridViewTextBoxColumn4.Width = 80;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.HeaderText = "Orig. ID";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            this.dataGridViewTextBoxColumn5.ToolTipText = "Original sound ID derived from TRNG extended soundmap";
            this.dataGridViewTextBoxColumn5.Width = 80;
            // 
            // tabSamples
            // 
            this.tabSamples.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabSamples.Controls.Add(this.soundDataGridViewControls);
            this.tabSamples.Controls.Add(this.soundDataGridView);
            this.tabSamples.Controls.Add(this.lblPathsPrompt);
            this.tabSamples.Location = new System.Drawing.Point(4, 22);
            this.tabSamples.Name = "tabSamples";
            this.tabSamples.Padding = new System.Windows.Forms.Padding(3);
            this.tabSamples.Size = new System.Drawing.Size(778, 505);
            this.tabSamples.TabIndex = 2;
            this.tabSamples.Text = "Sound Sample Paths";
            // 
            // soundDataGridViewControls
            // 
            this.soundDataGridViewControls.AllowUserDelete = false;
            this.soundDataGridViewControls.AllowUserMove = false;
            this.soundDataGridViewControls.AllowUserNew = false;
            this.soundDataGridViewControls.AlwaysInsertAtZero = false;
            this.soundDataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.soundDataGridViewControls.Enabled = false;
            this.soundDataGridViewControls.Location = new System.Drawing.Point(751, 32);
            this.soundDataGridViewControls.MinimumSize = new System.Drawing.Size(24, 100);
            this.soundDataGridViewControls.Name = "soundDataGridViewControls";
            this.soundDataGridViewControls.Size = new System.Drawing.Size(24, 467);
            this.soundDataGridViewControls.TabIndex = 3;
            // 
            // soundDataGridView
            // 
            this.soundDataGridView.AllowUserToAddRows = false;
            this.soundDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.soundDataGridView.AutoGenerateColumns = false;
            this.soundDataGridView.ColumnHeadersHeight = 17;
            this.soundDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.soundDataGridViewColumnPath,
            this.soundDataGridViewColumnSearch});
            this.soundDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.soundDataGridView.Location = new System.Drawing.Point(6, 32);
            this.soundDataGridView.Name = "soundDataGridView";
            this.soundDataGridView.RowHeadersWidth = 41;
            this.soundDataGridView.Size = new System.Drawing.Size(739, 467);
            this.soundDataGridView.TabIndex = 2;
            this.soundDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.soundDataGridView_CellContentClick);
            this.soundDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.soundDataGridView_CellFormatting);
            // 
            // soundDataGridViewColumnPath
            // 
            this.soundDataGridViewColumnPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.soundDataGridViewColumnPath.DataPropertyName = "Path";
            this.soundDataGridViewColumnPath.HeaderText = "Path";
            this.soundDataGridViewColumnPath.Name = "soundDataGridViewColumnPath";
            this.soundDataGridViewColumnPath.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // soundDataGridViewColumnSearch
            // 
            this.soundDataGridViewColumnSearch.HeaderText = "";
            this.soundDataGridViewColumnSearch.Name = "soundDataGridViewColumnSearch";
            this.soundDataGridViewColumnSearch.Text = "Browse";
            // 
            // lblPathsPrompt
            // 
            this.lblPathsPrompt.AutoSize = true;
            this.lblPathsPrompt.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPathsPrompt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblPathsPrompt.Location = new System.Drawing.Point(3, 3);
            this.lblPathsPrompt.Name = "lblPathsPrompt";
            this.lblPathsPrompt.Size = new System.Drawing.Size(545, 26);
            this.lblPathsPrompt.TabIndex = 1;
            this.lblPathsPrompt.Text = "Locations from which sound samples will be loaded.\r\nEach required sample will be " +
    "searched in folders in top to bottom order. If not found, sound is not played.";
            // 
            // tabMisc
            // 
            this.tabMisc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabMisc.Controls.Add(this.panelTr5Weather);
            this.tabMisc.Controls.Add(this.panelTr5LaraType);
            this.tabMisc.Controls.Add(this.panel6);
            this.tabMisc.Controls.Add(this.panel12);
            this.tabMisc.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabMisc.Location = new System.Drawing.Point(4, 22);
            this.tabMisc.Name = "tabMisc";
            this.tabMisc.Size = new System.Drawing.Size(778, 505);
            this.tabMisc.TabIndex = 6;
            this.tabMisc.Text = "Misc";
            // 
            // panelTr5Weather
            // 
            this.panelTr5Weather.Controls.Add(this.comboTr5Weather);
            this.panelTr5Weather.Controls.Add(this.lblTr5Weather);
            this.panelTr5Weather.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTr5Weather.Location = new System.Drawing.Point(0, 415);
            this.panelTr5Weather.Name = "panelTr5Weather";
            this.panelTr5Weather.Size = new System.Drawing.Size(778, 51);
            this.panelTr5Weather.TabIndex = 97;
            // 
            // comboTr5Weather
            // 
            this.comboTr5Weather.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboTr5Weather.FormattingEnabled = true;
            this.comboTr5Weather.Location = new System.Drawing.Point(19, 23);
            this.comboTr5Weather.Name = "comboTr5Weather";
            this.comboTr5Weather.Size = new System.Drawing.Size(658, 23);
            this.comboTr5Weather.TabIndex = 4;
            this.comboTr5Weather.SelectedIndexChanged += new System.EventHandler(this.comboTr5Weather_SelectedIndexChanged);
            // 
            // lblTr5Weather
            // 
            this.lblTr5Weather.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblTr5Weather.Location = new System.Drawing.Point(0, 3);
            this.lblTr5Weather.Name = "lblTr5Weather";
            this.lblTr5Weather.Size = new System.Drawing.Size(439, 17);
            this.lblTr5Weather.TabIndex = 3;
            this.lblTr5Weather.Text = "TR5 weather:";
            // 
            // panelTr5LaraType
            // 
            this.panelTr5LaraType.Controls.Add(this.comboLaraType);
            this.panelTr5LaraType.Controls.Add(this.lblLaraType);
            this.panelTr5LaraType.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTr5LaraType.Location = new System.Drawing.Point(0, 364);
            this.panelTr5LaraType.Name = "panelTr5LaraType";
            this.panelTr5LaraType.Size = new System.Drawing.Size(778, 51);
            this.panelTr5LaraType.TabIndex = 96;
            // 
            // comboLaraType
            // 
            this.comboLaraType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboLaraType.FormattingEnabled = true;
            this.comboLaraType.Location = new System.Drawing.Point(19, 23);
            this.comboLaraType.Name = "comboLaraType";
            this.comboLaraType.Size = new System.Drawing.Size(658, 23);
            this.comboLaraType.TabIndex = 4;
            this.comboLaraType.SelectedIndexChanged += new System.EventHandler(this.comboLaraType_SelectedIndexChanged);
            // 
            // lblLaraType
            // 
            this.lblLaraType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblLaraType.Location = new System.Drawing.Point(0, 3);
            this.lblLaraType.Name = "lblLaraType";
            this.lblLaraType.Size = new System.Drawing.Size(439, 17);
            this.lblLaraType.TabIndex = 3;
            this.lblLaraType.Text = "TR5 Lara type:";
            // 
            // tabTrx
            // 
            this.tabTrx.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabTrx.Controls.Add(this.comboTrxTextureDepth);
            this.tabTrx.Controls.Add(this.lblTrxTextureDepth);
            this.tabTrx.Location = new System.Drawing.Point(4, 22);
            this.tabTrx.Name = "tabTrx";
            this.tabTrx.Size = new System.Drawing.Size(778, 505);
            this.tabTrx.TabIndex = 11;
            this.tabTrx.Text = "TRX";
            // 
            // comboTrxTextureDepth
            // 
            this.comboTrxTextureDepth.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboTrxTextureDepth.FormattingEnabled = true;
            this.comboTrxTextureDepth.Location = new System.Drawing.Point(129, 10);
            this.comboTrxTextureDepth.Name = "comboTrxTextureDepth";
            this.comboTrxTextureDepth.Size = new System.Drawing.Size(81, 23);
            this.comboTrxTextureDepth.TabIndex = 4;
            this.comboTrxTextureDepth.SelectedIndexChanged += new System.EventHandler(this.comboTrxTextureDepth_SelectedIndexChanged);
            // 
            // lblTrxTextureDepth
            // 
            this.lblTrxTextureDepth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblTrxTextureDepth.Location = new System.Drawing.Point(0, 15);
            this.lblTrxTextureDepth.Name = "lblTrxTextureDepth";
            this.lblTrxTextureDepth.Size = new System.Drawing.Size(123, 17);
            this.lblTrxTextureDepth.TabIndex = 3;
            this.lblTrxTextureDepth.Text = "Texture depth:";
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.levelFilePathBut);
            this.panel6.Controls.Add(this.darkLabel6);
            this.panel6.Controls.Add(this.levelFilePathTxt);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 312);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(778, 52);
            this.panel6.TabIndex = 94;
            // 
            // levelFilePathBut
            // 
            this.levelFilePathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.levelFilePathBut.Checked = false;
            this.levelFilePathBut.Location = new System.Drawing.Point(683, 20);
            this.levelFilePathBut.Name = "levelFilePathBut";
            this.levelFilePathBut.Size = new System.Drawing.Size(92, 22);
            this.levelFilePathBut.TabIndex = 3;
            this.levelFilePathBut.Text = "Browse";
            this.levelFilePathBut.Click += new System.EventHandler(this.levelFilePathBut_Click);
            // 
            // darkLabel6
            // 
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(0, 0);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(384, 17);
            this.darkLabel6.TabIndex = 1;
            this.darkLabel6.Text = "Full file path for the currently open level:";
            // 
            // levelFilePathTxt
            // 
            this.levelFilePathTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.levelFilePathTxt.Location = new System.Drawing.Point(19, 20);
            this.levelFilePathTxt.Name = "levelFilePathTxt";
            this.levelFilePathTxt.Size = new System.Drawing.Size(658, 22);
            this.levelFilePathTxt.TabIndex = 2;
            this.levelFilePathTxt.TextChanged += new System.EventHandler(this.levelFilePathTxt_TextChanged);
            // 
            // panel12
            // 
            this.panel12.Controls.Add(this.cbCompressTextures);
            this.panel12.Controls.Add(this.cbUse32BitLighting);
            this.panel12.Controls.Add(this.cmbSampleRate);
            this.panel12.Controls.Add(this.cbKeepSampleRate);
            this.panel12.Controls.Add(this.cbRemoveObjects);
            this.panel12.Controls.Add(this.cbRearrangeRooms);
            this.panel12.Controls.Add(this.cbRemapAnimTextures);
            this.panel12.Controls.Add(this.cbDither16BitTextures);
            this.panel12.Controls.Add(this.cbOverrideAllLightQuality);
            this.panel12.Controls.Add(this.cmbDefaultLightQuality);
            this.panel12.Controls.Add(this.darkLabel22);
            this.panel12.Controls.Add(this.cbAgressiveFloordataPacking);
            this.panel12.Controls.Add(this.cbAgressiveTexturePacking);
            this.panel12.Controls.Add(this.darkLabel13);
            this.panel12.Controls.Add(this.darkLabel16);
            this.panel12.Controls.Add(this.numPadding);
            this.panel12.Controls.Add(this.panelRoomAmbientLight);
            this.panel12.Controls.Add(this.darkLabel12);
            this.panel12.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel12.Location = new System.Drawing.Point(0, 0);
            this.panel12.Name = "panel12";
            this.panel12.Size = new System.Drawing.Size(778, 312);
            this.panel12.TabIndex = 91;
            // 
            // cmbSampleRate
            // 
            this.cmbSampleRate.FormattingEnabled = true;
            this.cmbSampleRate.Items.AddRange(new object[] {
            "11025",
            "22050",
            "44100",
            "48000"});
            this.cmbSampleRate.Location = new System.Drawing.Point(215, 279);
            this.cmbSampleRate.Name = "cmbSampleRate";
            this.cmbSampleRate.Size = new System.Drawing.Size(81, 23);
            this.cmbSampleRate.TabIndex = 115;
            this.cmbSampleRate.SelectedIndexChanged += new System.EventHandler(this.cmbSampleRate_SelectedIndexChanged);
            // 
            // darkLabel22
            // 
            this.darkLabel22.AutoSize = true;
            this.darkLabel22.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel22.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel22.Location = new System.Drawing.Point(0, 43);
            this.darkLabel22.Name = "darkLabel22";
            this.darkLabel22.Size = new System.Drawing.Size(111, 13);
            this.darkLabel22.TabIndex = 106;
            this.darkLabel22.Text = "Default light quality:";
            // 
            // darkLabel13
            // 
            this.darkLabel13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel13.Location = new System.Drawing.Point(213, 71);
            this.darkLabel13.Name = "darkLabel13";
            this.darkLabel13.Size = new System.Drawing.Size(45, 17);
            this.darkLabel13.TabIndex = 103;
            this.darkLabel13.Text = "pixels";
            // 
            // darkLabel16
            // 
            this.darkLabel16.AutoSize = true;
            this.darkLabel16.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel16.Location = new System.Drawing.Point(0, 15);
            this.darkLabel16.Name = "darkLabel16";
            this.darkLabel16.Size = new System.Drawing.Size(118, 13);
            this.darkLabel16.TabIndex = 90;
            this.darkLabel16.Text = "Default ambient light:";
            // 
            // panelRoomAmbientLight
            // 
            this.panelRoomAmbientLight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRoomAmbientLight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelRoomAmbientLight.Location = new System.Drawing.Point(129, 10);
            this.panelRoomAmbientLight.Name = "panelRoomAmbientLight";
            this.panelRoomAmbientLight.Size = new System.Drawing.Size(81, 24);
            this.panelRoomAmbientLight.TabIndex = 89;
            this.panelRoomAmbientLight.Click += new System.EventHandler(this.panelRoomAmbientLight_Click);
            // 
            // darkLabel12
            // 
            this.darkLabel12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel12.Location = new System.Drawing.Point(0, 71);
            this.darkLabel12.Name = "darkLabel12";
            this.darkLabel12.Size = new System.Drawing.Size(123, 17);
            this.darkLabel12.TabIndex = 101;
            this.darkLabel12.Text = "Texture tile padding:";
            // 
            // tabPaths
            // 
            this.tabPaths.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabPaths.Controls.Add(this.darkLabel1);
            this.tabPaths.Controls.Add(this.pathVariablesDataGridView);
            this.tabPaths.Location = new System.Drawing.Point(4, 22);
            this.tabPaths.Name = "tabPaths";
            this.tabPaths.Padding = new System.Windows.Forms.Padding(3);
            this.tabPaths.Size = new System.Drawing.Size(778, 505);
            this.tabPaths.TabIndex = 5;
            this.tabPaths.Text = "Path Placeholders";
            // 
            // darkLabel1
            // 
            this.darkLabel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(3, 3);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(772, 12);
            this.darkLabel1.TabIndex = 1;
            this.darkLabel1.Text = "Available dynamic place holders that can be used inside paths: ";
            // 
            // pathVariablesDataGridView
            // 
            this.pathVariablesDataGridView.AllowUserToAddRows = false;
            this.pathVariablesDataGridView.AllowUserToDeleteRows = false;
            this.pathVariablesDataGridView.AllowUserToDragDropRows = false;
            this.pathVariablesDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pathVariablesDataGridView.ColumnHeadersHeight = 19;
            this.pathVariablesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pathVariablesDataGridViewNameColumn,
            this.pathVariablesDataGridViewValueColumn});
            this.pathVariablesDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pathVariablesDataGridView.Location = new System.Drawing.Point(6, 19);
            this.pathVariablesDataGridView.Name = "pathVariablesDataGridView";
            this.pathVariablesDataGridView.RowHeadersWidth = 41;
            this.pathVariablesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.pathVariablesDataGridView.Size = new System.Drawing.Size(769, 480);
            this.pathVariablesDataGridView.TabIndex = 2;
            this.pathVariablesDataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.pathVariablesDataGridView_CellMouseDown);
            // 
            // pathVariablesDataGridViewNameColumn
            // 
            this.pathVariablesDataGridViewNameColumn.ContextMenuStrip = this.pathVariablesDataGridViewContextMenu;
            this.pathVariablesDataGridViewNameColumn.HeaderText = "Placeholder";
            this.pathVariablesDataGridViewNameColumn.MinimumWidth = 50;
            this.pathVariablesDataGridViewNameColumn.Name = "pathVariablesDataGridViewNameColumn";
            this.pathVariablesDataGridViewNameColumn.ReadOnly = true;
            this.pathVariablesDataGridViewNameColumn.Width = 120;
            // 
            // pathVariablesDataGridViewValueColumn
            // 
            this.pathVariablesDataGridViewValueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.pathVariablesDataGridViewValueColumn.ContextMenuStrip = this.pathVariablesDataGridViewContextMenu;
            this.pathVariablesDataGridViewValueColumn.HeaderText = "Current Value";
            this.pathVariablesDataGridViewValueColumn.Name = "pathVariablesDataGridViewValueColumn";
            this.pathVariablesDataGridViewValueColumn.ReadOnly = true;
            // 
            // colSoundsId
            // 
            this.colSoundsId.HeaderText = "ID";
            this.colSoundsId.Name = "colSoundsId";
            this.colSoundsId.ReadOnly = true;
            this.colSoundsId.Width = 40;
            // 
            // colSoundsName
            // 
            this.colSoundsName.HeaderText = "Name";
            this.colSoundsName.Name = "colSoundsName";
            this.colSoundsName.Width = 200;
            // 
            // SelectedSoundsCatalogColumn
            // 
            this.SelectedSoundsCatalogColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SelectedSoundsCatalogColumn.HeaderText = "From catalog";
            this.SelectedSoundsCatalogColumn.Name = "SelectedSoundsCatalogColumn";
            this.SelectedSoundsCatalogColumn.ReadOnly = true;
            // 
            // SelectedSoundsGameColumn
            // 
            this.SelectedSoundsGameColumn.HeaderText = "Range";
            this.SelectedSoundsGameColumn.Name = "SelectedSoundsGameColumn";
            this.SelectedSoundsGameColumn.ReadOnly = true;
            this.SelectedSoundsGameColumn.ToolTipText = "Range name of TRNG extended soundmap";
            this.SelectedSoundsGameColumn.Width = 80;
            // 
            // SelectedSoundsOriginalIdColumn
            // 
            this.SelectedSoundsOriginalIdColumn.HeaderText = "Orig. ID";
            this.SelectedSoundsOriginalIdColumn.Name = "SelectedSoundsOriginalIdColumn";
            this.SelectedSoundsOriginalIdColumn.ReadOnly = true;
            this.SelectedSoundsOriginalIdColumn.ToolTipText = "Original sound ID derived from TRNG extended soundmap";
            this.SelectedSoundsOriginalIdColumn.Width = 80;
            // 
            // FormLevelSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(1001, 573);
            this.Controls.Add(this.butApply);
            this.Controls.Add(this.butOk);
            this.Controls.Add(this.darkSectionPanel1);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.optionsList);
            this.MinimizeBox = false;
            this.Name = "FormLevelSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Level Settings";
            this.pathVariablesDataGridViewContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numPadding)).EndInit();
            this.darkSectionPanel1.ResumeLayout(false);
            this.tabbedContainer.ResumeLayout(false);
            this.tabGame.ResumeLayout(false);
            this.panelLuaPath.ResumeLayout(false);
            this.panelLuaPath.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panelScripts.ResumeLayout(false);
            this.panelScripts.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.tabTextures.ResumeLayout(false);
            this.tabTextures.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textureFileDataGridView)).EndInit();
            this.tabObjects.ResumeLayout(false);
            this.tabObjects.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.objectFileDataGridView)).EndInit();
            this.tabImportedGeometry.ResumeLayout(false);
            this.tabImportedGeometry.PerformLayout();
            this.tabStaticMeshes.ResumeLayout(false);
            this.tabStaticMeshes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.staticMeshMergeDataGridView)).EndInit();
            this.tabSkyAndFont.ResumeLayout(false);
            this.panelTr5Sprites.ResumeLayout(false);
            this.panelTr5Sprites.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tr5SpritesTextureFilePathPicPreview)).EndInit();
            this.panelFont.ResumeLayout(false);
            this.panelFont.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fontTextureFilePathPicPreview)).EndInit();
            this.panelSky.ResumeLayout(false);
            this.panelSky.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.skyTextureFilePathPicPreview)).EndInit();
            this.tabSoundsCatalogs.ResumeLayout(false);
            this.tabSoundsCatalogs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.soundsCatalogsDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.selectedSoundsDataGridView)).EndInit();
            this.tabSamples.ResumeLayout(false);
            this.tabSamples.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.soundDataGridView)).EndInit();
            this.tabMisc.ResumeLayout(false);
            this.panelTr5Weather.ResumeLayout(false);
            this.panelTr5LaraType.ResumeLayout(false);
            this.tabTrx.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel12.ResumeLayout(false);
            this.panel12.PerformLayout();
            this.tabPaths.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pathVariablesDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private DarkUI.Controls.DarkButton gameLevelFilePathBut;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkTextBox gameLevelFilePathTxt;
        private System.Windows.Forms.Panel panel3;
        private DarkUI.Controls.DarkButton gameExecutableFilePathBut;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkTextBox gameExecutableFilePathTxt;
        private DarkUI.Controls.DarkButton fontTextureFilePathBut;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private System.Windows.Forms.Panel panel7;
        private DarkUI.Controls.DarkButton gameDirectoryBut;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkTextBox gameDirectoryTxt;
        private System.Windows.Forms.Panel panelFont;
        private DarkUI.Controls.DarkRadioButton fontTextureFilePathOptCustom;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkRadioButton fontTextureFilePathOptAuto;
        private DarkUI.Controls.DarkTextBox fontTextureFilePathTxt;
        private System.Windows.Forms.Panel panelSky;
        private DarkUI.Controls.DarkButton skyTextureFilePathBut;
        private DarkUI.Controls.DarkRadioButton skyTextureFilePathOptCustom;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkRadioButton skyTextureFilePathOptAuto;
        private DarkUI.Controls.DarkTextBox skyTextureFilePathTxt;
        private System.Windows.Forms.PictureBox fontTextureFilePathPicPreview;
        private System.Windows.Forms.PictureBox skyTextureFilePathPicPreview;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkContextMenu pathVariablesDataGridViewContextMenu;
        private System.Windows.Forms.ToolStripMenuItem pathVariablesDataGridViewContextMenuCopy;
        private System.Windows.Forms.ToolTip pathToolTip;
        private DarkUI.Controls.DarkDataGridView pathVariablesDataGridView;
        private DarkUI.Controls.DarkCheckBox GameEnableQuickStartFeatureCheckBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathVariablesDataGridViewNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathVariablesDataGridViewValueColumn;
        private DarkUI.Controls.DarkLabel darkLabel11;
        private Controls.ImportedGeometryManager importedGeometryManager;
        private TombLib.Controls.DarkTabbedContainer tabbedContainer;
        private System.Windows.Forms.TabPage tabObjects;
        private System.Windows.Forms.TabPage tabSkyAndFont;
        private System.Windows.Forms.TabPage tabSamples;
        private System.Windows.Forms.TabPage tabImportedGeometry;
        private System.Windows.Forms.TabPage tabGame;
        private System.Windows.Forms.TabPage tabPaths;
        private DarkUI.Controls.DarkButton butApply;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkListView optionsList;
        private System.Windows.Forms.Panel panel1;
        private DarkUI.Controls.DarkComboBox comboGameVersion;
        private DarkUI.Controls.DarkLabel darkLabel14;
        private System.Windows.Forms.TabPage tabMisc;
        private System.Windows.Forms.Panel panel12;
        private DarkUI.Controls.DarkLabel darkLabel16;
        private DarkUI.Controls.DarkPanel panelRoomAmbientLight;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Panel panelTr5Sprites;
        private System.Windows.Forms.PictureBox tr5SpritesTextureFilePathPicPreview;
        private DarkUI.Controls.DarkButton tr5SpritesTextureFilePathBut;
        private DarkUI.Controls.DarkRadioButton tr5SpritesFilePathOptCustom;
        private DarkUI.Controls.DarkLabel lblTr5ExtraSprites;
        private DarkUI.Controls.DarkRadioButton tr5SpritesFilePathOptAuto;
        private DarkUI.Controls.DarkTextBox tr5SpritesTextureFilePathTxt;
        private TombLib.Controls.DarkDataGridViewControls objectFileDataGridViewControls;
        private DarkUI.Controls.DarkDataGridView objectFileDataGridView;
        private System.Windows.Forms.TabPage tabTextures;
        private System.Windows.Forms.Panel panelTr5Weather;
        private DarkUI.Controls.DarkComboBox comboTr5Weather;
        private DarkUI.Controls.DarkLabel lblTr5Weather;
        private System.Windows.Forms.Panel panelTr5LaraType;
        private DarkUI.Controls.DarkComboBox comboLaraType;
        private DarkUI.Controls.DarkLabel lblLaraType;
        private System.Windows.Forms.Panel panel6;
        private DarkUI.Controls.DarkButton levelFilePathBut;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkTextBox levelFilePathTxt;
        private DarkUI.Controls.DarkDataGridView textureFileDataGridView;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private System.Windows.Forms.TabPage tabTrx;
        private DarkUI.Controls.DarkComboBox comboTrxTextureDepth;
        private DarkUI.Controls.DarkLabel lblTrxTextureDepth;
        private TombLib.Controls.DarkDataGridViewControls textureFileDataGridViewControls;
        private System.Windows.Forms.DataGridViewTextBoxColumn objectFileDataGridViewPathColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn objectFileDataGridViewSearchColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn objectFileDataGridViewShowColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn objectFileDataGridViewMessageColumn;
        private DarkUI.Controls.DarkLabel darkLabel13;
        private DarkUI.Controls.DarkNumericUpDown numPadding;
        private DarkUI.Controls.DarkLabel darkLabel12;
        private DarkUI.Controls.DarkCheckBox cbAgressiveTexturePacking;
        private DarkUI.Controls.DarkCheckBox cbAgressiveFloordataPacking;
        private System.Windows.Forms.TabPage tabStaticMeshes;
        private System.Windows.Forms.TabPage tabSoundsCatalogs;
        private DarkUI.Controls.DarkLabel darkLabel18;
        private DarkUI.Controls.DarkDataGridView selectedSoundsDataGridView;
        private DarkUI.Controls.DarkTextBox tbFilterSounds;
        private TombLib.Controls.DarkDataGridViewControls soundsCatalogsDataGridViewControls;
        private DarkUI.Controls.DarkDataGridView soundsCatalogsDataGridView;
        private DarkUI.Controls.DarkLabel labelSoundsCatalogsStatistics;
        private DarkUI.Controls.DarkLabel darkLabel17;
        private DarkUI.Controls.DarkDataGridView staticMeshMergeDataGridView;
        private DarkUI.Controls.DarkLabel darkLabel50;
        private DarkUI.Controls.DarkLabel darkLabel19;
        private DarkUI.Controls.DarkLabel lblCatalogsPrompt;
        private TombLib.Controls.DarkDataGridViewControls soundDataGridViewControls;
        private DarkUI.Controls.DarkDataGridView soundDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn soundDataGridViewColumnPath;
        private DarkUI.Controls.DarkDataGridViewButtonColumn soundDataGridViewColumnSearch;
        private DarkUI.Controls.DarkLabel lblPathsPrompt;
        private DarkUI.Controls.DarkButton butDeselectAllSounds;
        private DarkUI.Controls.DarkButton butSelectAllSounds;
        private DarkUI.Controls.DarkButton butSearchSounds;
        private DarkUI.Controls.DarkButton butAssignSoundsFromSelectedCatalogs;
        private DarkUI.Controls.DarkButton butAssignHardcodedSounds;
        private DarkUI.Controls.DarkButton butAutodetectSoundsAndAssign;
        private DarkUI.Controls.DarkButton butAssignFromSoundSources;
        private DarkUI.Controls.DarkLabel darkLabel20;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private DarkUI.Controls.DarkButton butRemoveMissing;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSoundsId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSoundsName;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelectedSoundsCatalogColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelectedSoundsGameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelectedSoundsOriginalIdColumn;
        private DarkUI.Controls.DarkButton butAssignFromWads;
        private DarkUI.Controls.DarkButton butSelectAllButShatterStatics;
        private DarkUI.Controls.DarkButton butDeselectAllStatics;
        private DarkUI.Controls.DarkButton butSelectAllStatics;
        private System.Windows.Forms.DataGridViewTextBoxColumn SoundsCatalogPathColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn SoundsCatalogSearchColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn SoundsCatalogReloadButton;
        private DarkUI.Controls.DarkDataGridViewButtonColumn SoundsCatalogEditColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn SoundsCatalogsAssignColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SoundsCatalogsSoundCountColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SoundsCatalogMessageColumn;
        private DarkUI.Controls.DarkCheckBox cbAutodetectIfNoneSelected;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn colSoundsEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSoundID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMeshName;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn colMergeStatics;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn colInterpretShadesAsEffect;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn colTintAsAmbient;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn colClearShades;
        private System.Windows.Forms.DataGridViewImageColumn textureFileDataGridViewPreviewColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn textureFileDataGridViewPathColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn textureFileDataGridViewSearchColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn textureFileDataGridViewShowColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn textureFileDataGridViewMessageColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn textureFileDataGridViewSizeColumn;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn textureFileDataGridViewReplaceMagentaWithTransparencyColumn;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn textureFileDataGridViewConvert512PixelsToDoubleRowsColumn;
        private DarkUI.Controls.DarkLabel darkLabel22;
        private DarkUI.Controls.DarkCheckBox cbOverrideAllLightQuality;
        private DarkUI.Controls.DarkComboBox cmbDefaultLightQuality;
        private DarkUI.Controls.DarkCheckBox cbDither16BitTextures;
        private DarkUI.Controls.DarkCheckBox cbRemapAnimTextures;
        private DarkUI.Controls.DarkCheckBox cbRearrangeRooms;
		private DarkUI.Controls.DarkCheckBox cbRemoveObjects;
        private DarkUI.Controls.DarkCheckBox GameEnableExtraReverbPresetsCheckBox;
        private DarkUI.Controls.DarkCheckBox cbEnableExtraBlendingModes;
        private DarkUI.Controls.DarkCheckBox cbKeepSampleRate;
        private DarkUI.Controls.DarkComboBox cmbSampleRate;
        private System.Windows.Forms.Panel panelLuaPath;
        private DarkUI.Controls.DarkButton butBrowseLuaPath;
        private DarkUI.Controls.DarkLabel darkLabel10;
        private DarkUI.Controls.DarkTextBox tbLuaPath;
        private System.Windows.Forms.Panel panelScripts;
        private DarkUI.Controls.DarkButton scriptPathBut;
        private DarkUI.Controls.DarkLabel darkLabel15;
        private DarkUI.Controls.DarkTextBox tbScriptPath;
        private DarkUI.Controls.DarkCheckBox cbUse32BitLighting;
        private DarkUI.Controls.DarkCheckBox cbCompressTextures;
    }
}