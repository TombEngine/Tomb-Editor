namespace WadTool
{
    partial class FormMain
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

		#region Codice generato da Progettazione Windows Form

		/// <summary>
		/// Metodo necessario per il supporto della finestra di progettazione. Non modificare
		/// il contenuto del metodo con l'editor di codice.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			statusStrip = new DarkUI.Controls.DarkStatusStrip();
			labelStatistics = new System.Windows.Forms.ToolStripStatusLabel();
			darkMenuStrip1 = new DarkUI.Controls.DarkMenuStrip();
			fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			newWad2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			openSourceWADToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			openDestinationWadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			openRecentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			openReferenceLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			closeReferenceLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			saveWad2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			saveWad2AsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			createToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			newMoveableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			newStaticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			newSpriteSequenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			selectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			lightingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			convertSelectionToStaticLightingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			convertSelectionToDynamicLightingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			texturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			convertToTiledToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			convertToUVMappedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			animatedTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			meshEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			convertDestinationWadToTombEngineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			optionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			aboutWadToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			debugAction0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			debugAction1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			debugAction2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			debugAction3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			debugAction4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			debugAction5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			debugAction6ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			debugAction7ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			debugAction8ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			debugAction9ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			darkToolStrip1 = new DarkUI.Controls.DarkToolStrip();
			labelDest = new System.Windows.Forms.ToolStripLabel();
			butNewWad2 = new System.Windows.Forms.ToolStripButton();
			butOpenDestWad = new System.Windows.Forms.ToolStripButton();
			butSave = new System.Windows.Forms.ToolStripButton();
			butSaveAs = new System.Windows.Forms.ToolStripButton();
			toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			labelSource = new System.Windows.Forms.ToolStripLabel();
			butOpenSourceWad = new System.Windows.Forms.ToolStripButton();
			toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			labelRefProject = new System.Windows.Forms.ToolStripLabel();
			lblRefLevel = new System.Windows.Forms.ToolStripLabel();
			butOpenRefLevel = new System.Windows.Forms.ToolStripButton();
			butCloseRefLevel = new System.Windows.Forms.ToolStripButton();
			tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			panel2 = new System.Windows.Forms.Panel();
			panelSource = new DarkUI.Controls.DarkSectionPanel();
			treeSourceWad = new TombLib.Controls.WadTreeView();
			darkToolStrip4 = new DarkUI.Controls.DarkToolStrip();
			toolStripButton4 = new System.Windows.Forms.ToolStripButton();
			toolStripButton5 = new System.Windows.Forms.ToolStripButton();
			panel1 = new System.Windows.Forms.Panel();
			panelDestination = new DarkUI.Controls.DarkSectionPanel();
			treeDestWad = new TombLib.Controls.WadTreeView();
			darkToolStrip3 = new DarkUI.Controls.DarkToolStrip();
			toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			toolStripButton3 = new System.Windows.Forms.ToolStripButton();
			panel3 = new System.Windows.Forms.Panel();
			darkSectionPanel3 = new DarkUI.Controls.DarkSectionPanel();
			panel3D = new Controls.PanelRenderingMainPreview();
			darkToolStrip2 = new DarkUI.Controls.DarkToolStrip();
			butEditAnimations = new System.Windows.Forms.ToolStripButton();
			butEditSkeleton = new System.Windows.Forms.ToolStripButton();
			butEditStaticModel = new System.Windows.Forms.ToolStripButton();
			butEditSpriteSequence = new System.Windows.Forms.ToolStripButton();
			contextMenuMoveableItem = new DarkUI.Controls.DarkContextMenu();
			editAnimationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			editSkeletonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			editMeshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
			toolStripMenuItemMoveablesChangeSlot = new System.Windows.Forms.ToolStripMenuItem();
			toolStripMenuItemMoveablesDelete = new System.Windows.Forms.ToolStripMenuItem();
			cmStatics = new DarkUI.Controls.DarkContextMenu();
			editObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			editMeshToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			changeSlorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			deleteObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			statusStrip.SuspendLayout();
			darkMenuStrip1.SuspendLayout();
			darkToolStrip1.SuspendLayout();
			tableLayoutPanel1.SuspendLayout();
			panel2.SuspendLayout();
			panelSource.SuspendLayout();
			darkToolStrip4.SuspendLayout();
			panel1.SuspendLayout();
			panelDestination.SuspendLayout();
			darkToolStrip3.SuspendLayout();
			panel3.SuspendLayout();
			darkSectionPanel3.SuspendLayout();
			darkToolStrip2.SuspendLayout();
			contextMenuMoveableItem.SuspendLayout();
			cmStatics.SuspendLayout();
			SuspendLayout();
			// 
			// statusStrip
			// 
			statusStrip.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			statusStrip.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { labelStatistics });
			statusStrip.Location = new System.Drawing.Point(0, 711);
			statusStrip.Name = "statusStrip";
			statusStrip.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
			statusStrip.Size = new System.Drawing.Size(1244, 31);
			statusStrip.TabIndex = 1;
			statusStrip.Text = "darkStatusStrip1";
			// 
			// labelStatistics
			// 
			labelStatistics.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			labelStatistics.ForeColor = System.Drawing.Color.Silver;
			labelStatistics.Name = "labelStatistics";
			labelStatistics.Size = new System.Drawing.Size(70, 18);
			labelStatistics.Text = "                     ";
			// 
			// darkMenuStrip1
			// 
			darkMenuStrip1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			darkMenuStrip1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, createToolStripMenuItem, selectionToolStripMenuItem, optionsToolStripMenuItem, helpToolStripMenuItem, debugToolStripMenuItem });
			darkMenuStrip1.Location = new System.Drawing.Point(0, 0);
			darkMenuStrip1.Name = "darkMenuStrip1";
			darkMenuStrip1.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
			darkMenuStrip1.Size = new System.Drawing.Size(1244, 24);
			darkMenuStrip1.TabIndex = 2;
			darkMenuStrip1.Text = "darkMenuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			fileToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { newWad2ToolStripMenuItem, toolStripMenuItem3, openSourceWADToolStripMenuItem, openDestinationWadToolStripMenuItem, openRecentToolStripMenuItem, openReferenceLevelToolStripMenuItem, closeReferenceLevelToolStripMenuItem, toolStripMenuItem1, saveWad2ToolStripMenuItem, saveWad2AsToolStripMenuItem, toolStripMenuItem2, exitToolStripMenuItem });
			fileToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			fileToolStripMenuItem.Text = "&File";
			// 
			// newWad2ToolStripMenuItem
			// 
			newWad2ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			newWad2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			newWad2ToolStripMenuItem.Name = "newWad2ToolStripMenuItem";
			newWad2ToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N;
			newWad2ToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
			newWad2ToolStripMenuItem.Text = "New Wad2";
			newWad2ToolStripMenuItem.Click += newWad2ToolStripMenuItem_Click;
			// 
			// toolStripMenuItem3
			// 
			toolStripMenuItem3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			toolStripMenuItem3.Name = "toolStripMenuItem3";
			toolStripMenuItem3.Size = new System.Drawing.Size(340, 6);
			// 
			// openSourceWADToolStripMenuItem
			// 
			openSourceWADToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			openSourceWADToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			openSourceWADToolStripMenuItem.Image = Properties.Resources.general_Open_16;
			openSourceWADToolStripMenuItem.Name = "openSourceWADToolStripMenuItem";
			openSourceWADToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
			openSourceWADToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
			openSourceWADToolStripMenuItem.Text = "Open source";
			openSourceWADToolStripMenuItem.Click += openSourceWADToolStripMenuItem_Click;
			// 
			// openDestinationWadToolStripMenuItem
			// 
			openDestinationWadToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			openDestinationWadToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			openDestinationWadToolStripMenuItem.Image = Properties.Resources.opened_folder_16;
			openDestinationWadToolStripMenuItem.Name = "openDestinationWadToolStripMenuItem";
			openDestinationWadToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.O;
			openDestinationWadToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
			openDestinationWadToolStripMenuItem.Text = "Open destination";
			openDestinationWadToolStripMenuItem.Click += openDestinationWad2ToolStripMenuItem_Click;
			// 
			// openRecentToolStripMenuItem
			// 
			openRecentToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			openRecentToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			openRecentToolStripMenuItem.Name = "openRecentToolStripMenuItem";
			openRecentToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
			openRecentToolStripMenuItem.Text = "Open recent...";
			// 
			// openReferenceLevelToolStripMenuItem
			// 
			openReferenceLevelToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			openReferenceLevelToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			openReferenceLevelToolStripMenuItem.Name = "openReferenceLevelToolStripMenuItem";
			openReferenceLevelToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.O;
			openReferenceLevelToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
			openReferenceLevelToolStripMenuItem.Text = "Open reference Tomb Editor project";
			openReferenceLevelToolStripMenuItem.Click += openReferenceLevelToolStripMenuItem_Click;
			// 
			// closeReferenceLevelToolStripMenuItem
			// 
			closeReferenceLevelToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			closeReferenceLevelToolStripMenuItem.Enabled = false;
			closeReferenceLevelToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			closeReferenceLevelToolStripMenuItem.Name = "closeReferenceLevelToolStripMenuItem";
			closeReferenceLevelToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
			closeReferenceLevelToolStripMenuItem.Text = "Close reference Tomb Editor project";
			closeReferenceLevelToolStripMenuItem.Click += closeReferenceLevelToolStripMenuItem_Click;
			// 
			// toolStripMenuItem1
			// 
			toolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			toolStripMenuItem1.Name = "toolStripMenuItem1";
			toolStripMenuItem1.Size = new System.Drawing.Size(340, 6);
			// 
			// saveWad2ToolStripMenuItem
			// 
			saveWad2ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			saveWad2ToolStripMenuItem.Enabled = false;
			saveWad2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			saveWad2ToolStripMenuItem.Image = Properties.Resources.save_16;
			saveWad2ToolStripMenuItem.Name = "saveWad2ToolStripMenuItem";
			saveWad2ToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
			saveWad2ToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
			saveWad2ToolStripMenuItem.Text = "Save Wad2";
			saveWad2ToolStripMenuItem.Click += butSave_Click;
			// 
			// saveWad2AsToolStripMenuItem
			// 
			saveWad2AsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			saveWad2AsToolStripMenuItem.Enabled = false;
			saveWad2AsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			saveWad2AsToolStripMenuItem.Image = Properties.Resources.save_as_16;
			saveWad2AsToolStripMenuItem.Name = "saveWad2AsToolStripMenuItem";
			saveWad2AsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.S;
			saveWad2AsToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
			saveWad2AsToolStripMenuItem.Text = "Save Wad2 as...";
			saveWad2AsToolStripMenuItem.Click += saveWad2AsToolStripMenuItem_Click;
			// 
			// toolStripMenuItem2
			// 
			toolStripMenuItem2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			toolStripMenuItem2.Name = "toolStripMenuItem2";
			toolStripMenuItem2.Size = new System.Drawing.Size(340, 6);
			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			exitToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			exitToolStripMenuItem.Image = Properties.Resources.door_opened_16;
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4;
			exitToolStripMenuItem.Size = new System.Drawing.Size(343, 22);
			exitToolStripMenuItem.Text = "Exit";
			exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
			// 
			// createToolStripMenuItem
			// 
			createToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			createToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { newMoveableToolStripMenuItem, newStaticToolStripMenuItem, newSpriteSequenceToolStripMenuItem });
			createToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			createToolStripMenuItem.Name = "createToolStripMenuItem";
			createToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
			createToolStripMenuItem.Text = "Create";
			// 
			// newMoveableToolStripMenuItem
			// 
			newMoveableToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			newMoveableToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			newMoveableToolStripMenuItem.Name = "newMoveableToolStripMenuItem";
			newMoveableToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
			newMoveableToolStripMenuItem.Text = "New moveable";
			newMoveableToolStripMenuItem.Click += newMoveableToolStripMenuItem_Click;
			// 
			// newStaticToolStripMenuItem
			// 
			newStaticToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			newStaticToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			newStaticToolStripMenuItem.Name = "newStaticToolStripMenuItem";
			newStaticToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
			newStaticToolStripMenuItem.Text = "New static";
			newStaticToolStripMenuItem.Click += newStaticToolStripMenuItem_Click;
			// 
			// newSpriteSequenceToolStripMenuItem
			// 
			newSpriteSequenceToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			newSpriteSequenceToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			newSpriteSequenceToolStripMenuItem.Name = "newSpriteSequenceToolStripMenuItem";
			newSpriteSequenceToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
			newSpriteSequenceToolStripMenuItem.Text = "New sprite sequence";
			newSpriteSequenceToolStripMenuItem.Click += newSpriteSequenceToolStripMenuItem_Click;
			// 
			// selectionToolStripMenuItem
			// 
			selectionToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			selectionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { lightingToolStripMenuItem, texturesToolStripMenuItem });
			selectionToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			selectionToolStripMenuItem.Name = "selectionToolStripMenuItem";
			selectionToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
			selectionToolStripMenuItem.Text = "Selection";
			// 
			// lightingToolStripMenuItem
			// 
			lightingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			lightingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { convertSelectionToStaticLightingToolStripMenuItem, convertSelectionToDynamicLightingToolStripMenuItem });
			lightingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lightingToolStripMenuItem.Name = "lightingToolStripMenuItem";
			lightingToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
			lightingToolStripMenuItem.Text = "Lighting";
			// 
			// convertSelectionToStaticLightingToolStripMenuItem
			// 
			convertSelectionToStaticLightingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			convertSelectionToStaticLightingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			convertSelectionToStaticLightingToolStripMenuItem.Name = "convertSelectionToStaticLightingToolStripMenuItem";
			convertSelectionToStaticLightingToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
			convertSelectionToStaticLightingToolStripMenuItem.Text = "Convert to static";
			convertSelectionToStaticLightingToolStripMenuItem.Click += convertSelectedObjectsLightTypeToStaticToolStripMenuItem_Click;
			// 
			// convertSelectionToDynamicLightingToolStripMenuItem
			// 
			convertSelectionToDynamicLightingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			convertSelectionToDynamicLightingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			convertSelectionToDynamicLightingToolStripMenuItem.Name = "convertSelectionToDynamicLightingToolStripMenuItem";
			convertSelectionToDynamicLightingToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
			convertSelectionToDynamicLightingToolStripMenuItem.Text = "Convert to dynamic";
			convertSelectionToDynamicLightingToolStripMenuItem.Click += convertSelectedObjectsLightTypeToDynamicToolStripMenuItem_Click;
			// 
			// texturesToolStripMenuItem
			// 
			texturesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			texturesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { convertToTiledToolStripMenuItem, convertToUVMappedToolStripMenuItem });
			texturesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			texturesToolStripMenuItem.Name = "texturesToolStripMenuItem";
			texturesToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
			texturesToolStripMenuItem.Text = "Textures";
			// 
			// convertToTiledToolStripMenuItem
			// 
			convertToTiledToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			convertToTiledToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			convertToTiledToolStripMenuItem.Name = "convertToTiledToolStripMenuItem";
			convertToTiledToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			convertToTiledToolStripMenuItem.Text = "Convert to tiled";
			convertToTiledToolStripMenuItem.Click += convertToTiledToolStripMenuItem_Click;
			// 
			// convertToUVMappedToolStripMenuItem
			// 
			convertToUVMappedToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			convertToUVMappedToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			convertToUVMappedToolStripMenuItem.Name = "convertToUVMappedToolStripMenuItem";
			convertToUVMappedToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			convertToUVMappedToolStripMenuItem.Text = "Convert to UV-mapped";
			convertToUVMappedToolStripMenuItem.Click += convertToUVMappedToolStripMenuItem_Click;
			// 
			// optionsToolStripMenuItem
			// 
			optionsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { animatedTexturesToolStripMenuItem, meshEditorToolStripMenuItem, toolStripSeparator4, convertDestinationWadToTombEngineToolStripMenuItem, toolStripSeparator2, optionsToolStripMenuItem1 });
			optionsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			optionsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
			optionsToolStripMenuItem.Text = "Tools";
			// 
			// animatedTexturesToolStripMenuItem
			// 
			animatedTexturesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			animatedTexturesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			animatedTexturesToolStripMenuItem.Name = "animatedTexturesToolStripMenuItem";
			animatedTexturesToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
			animatedTexturesToolStripMenuItem.Text = "Animated textures";
			animatedTexturesToolStripMenuItem.Click += animatedTexturesToolStripMenuItem_Click;
			// 
			// meshEditorToolStripMenuItem
			// 
			meshEditorToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			meshEditorToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			meshEditorToolStripMenuItem.Name = "meshEditorToolStripMenuItem";
			meshEditorToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
			meshEditorToolStripMenuItem.Text = "Mesh editor";
			meshEditorToolStripMenuItem.Click += meshEditorToolStripMenuItem_Click;
			// 
			// toolStripSeparator4
			// 
			toolStripSeparator4.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			toolStripSeparator4.Name = "toolStripSeparator4";
			toolStripSeparator4.Size = new System.Drawing.Size(231, 6);
			// 
			// convertDestinationWadToTombEngineToolStripMenuItem
			// 
			convertDestinationWadToTombEngineToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			convertDestinationWadToTombEngineToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			convertDestinationWadToTombEngineToolStripMenuItem.Name = "convertDestinationWadToTombEngineToolStripMenuItem";
			convertDestinationWadToTombEngineToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
			convertDestinationWadToTombEngineToolStripMenuItem.Text = "Convert wad to TombEngine...";
			convertDestinationWadToTombEngineToolStripMenuItem.Click += convertDestinationToTombEngineToolStripMenuItem_Click;
			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new System.Drawing.Size(231, 6);
			// 
			// optionsToolStripMenuItem1
			// 
			optionsToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			optionsToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			optionsToolStripMenuItem1.Name = "optionsToolStripMenuItem1";
			optionsToolStripMenuItem1.Size = new System.Drawing.Size(234, 22);
			optionsToolStripMenuItem1.Text = "Options...";
			optionsToolStripMenuItem1.Click += optionsToolStripMenuItem_Click;
			// 
			// helpToolStripMenuItem
			// 
			helpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { aboutWadToolToolStripMenuItem });
			helpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			helpToolStripMenuItem.Text = "Help";
			// 
			// aboutWadToolToolStripMenuItem
			// 
			aboutWadToolToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			aboutWadToolToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			aboutWadToolToolStripMenuItem.Name = "aboutWadToolToolStripMenuItem";
			aboutWadToolToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
			aboutWadToolToolStripMenuItem.Text = "About Wad Tool...";
			aboutWadToolToolStripMenuItem.Click += aboutWadToolToolStripMenuItem_Click;
			// 
			// debugToolStripMenuItem
			// 
			debugToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			debugToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { debugAction0ToolStripMenuItem, debugAction1ToolStripMenuItem, debugAction2ToolStripMenuItem, debugAction3ToolStripMenuItem, debugAction4ToolStripMenuItem, debugAction5ToolStripMenuItem, debugAction6ToolStripMenuItem, debugAction7ToolStripMenuItem, debugAction8ToolStripMenuItem, debugAction9ToolStripMenuItem });
			debugToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugToolStripMenuItem.Margin = new System.Windows.Forms.Padding(0, 0, 150, 0);
			debugToolStripMenuItem.Name = "debugToolStripMenuItem";
			debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
			debugToolStripMenuItem.Text = "Debug";
			// 
			// debugAction0ToolStripMenuItem
			// 
			debugAction0ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction0ToolStripMenuItem.Name = "debugAction0ToolStripMenuItem";
			debugAction0ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			debugAction0ToolStripMenuItem.Text = "Debug action 0";
			debugAction0ToolStripMenuItem.Click += debugAction0ToolStripMenuItem_Click;
			// 
			// debugAction1ToolStripMenuItem
			// 
			debugAction1ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction1ToolStripMenuItem.Name = "debugAction1ToolStripMenuItem";
			debugAction1ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			debugAction1ToolStripMenuItem.Text = "Debug action 1";
			debugAction1ToolStripMenuItem.Click += debugAction1ToolStripMenuItem_Click;
			// 
			// debugAction2ToolStripMenuItem
			// 
			debugAction2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction2ToolStripMenuItem.Name = "debugAction2ToolStripMenuItem";
			debugAction2ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			debugAction2ToolStripMenuItem.Text = "Debug action 1";
			debugAction2ToolStripMenuItem.Click += debugAction2ToolStripMenuItem_Click;
			// 
			// debugAction3ToolStripMenuItem
			// 
			debugAction3ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			debugAction3ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction3ToolStripMenuItem.Name = "debugAction3ToolStripMenuItem";
			debugAction3ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			debugAction3ToolStripMenuItem.Text = "Debug action 1";
			debugAction3ToolStripMenuItem.Click += debugAction3ToolStripMenuItem_Click;
			// 
			// debugAction4ToolStripMenuItem
			// 
			debugAction4ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			debugAction4ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction4ToolStripMenuItem.Name = "debugAction4ToolStripMenuItem";
			debugAction4ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			debugAction4ToolStripMenuItem.Text = "Debug action 4";
			debugAction4ToolStripMenuItem.Click += debugAction4ToolStripMenuItem_Click;
			// 
			// debugAction5ToolStripMenuItem
			// 
			debugAction5ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			debugAction5ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction5ToolStripMenuItem.Name = "debugAction5ToolStripMenuItem";
			debugAction5ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			debugAction5ToolStripMenuItem.Text = "Debug action 5";
			debugAction5ToolStripMenuItem.Click += debugAction5ToolStripMenuItem_Click;
			// 
			// debugAction6ToolStripMenuItem
			// 
			debugAction6ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			debugAction6ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction6ToolStripMenuItem.Name = "debugAction6ToolStripMenuItem";
			debugAction6ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			debugAction6ToolStripMenuItem.Text = "Debug action 6";
			debugAction6ToolStripMenuItem.Click += debugAction6ToolStripMenuItem_Click;
			// 
			// debugAction7ToolStripMenuItem
			// 
			debugAction7ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			debugAction7ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction7ToolStripMenuItem.Name = "debugAction7ToolStripMenuItem";
			debugAction7ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			debugAction7ToolStripMenuItem.Text = "Debug action 7";
			debugAction7ToolStripMenuItem.Click += debugAction7ToolStripMenuItem_Click;
			// 
			// debugAction8ToolStripMenuItem
			// 
			debugAction8ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			debugAction8ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction8ToolStripMenuItem.Name = "debugAction8ToolStripMenuItem";
			debugAction8ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			debugAction8ToolStripMenuItem.Text = "Debug action 8";
			debugAction8ToolStripMenuItem.Click += debugAction8ToolStripMenuItem_Click;
			// 
			// debugAction9ToolStripMenuItem
			// 
			debugAction9ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			debugAction9ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction9ToolStripMenuItem.Name = "debugAction9ToolStripMenuItem";
			debugAction9ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
			debugAction9ToolStripMenuItem.Text = "Debug action 9";
			debugAction9ToolStripMenuItem.Click += debugAction9ToolStripMenuItem_Click;
			// 
			// darkToolStrip1
			// 
			darkToolStrip1.AutoSize = false;
			darkToolStrip1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			darkToolStrip1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			darkToolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { labelDest, butNewWad2, butOpenDestWad, butSave, butSaveAs, toolStripSeparator3, labelSource, butOpenSourceWad, toolStripSeparator1, labelRefProject, lblRefLevel, butOpenRefLevel, butCloseRefLevel });
			darkToolStrip1.Location = new System.Drawing.Point(0, 24);
			darkToolStrip1.Name = "darkToolStrip1";
			darkToolStrip1.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			darkToolStrip1.Size = new System.Drawing.Size(1244, 28);
			darkToolStrip1.TabIndex = 3;
			darkToolStrip1.Text = "darkToolStrip1";
			// 
			// labelDest
			// 
			labelDest.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			labelDest.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			labelDest.Name = "labelDest";
			labelDest.Size = new System.Drawing.Size(70, 25);
			labelDest.Text = "Destination:";
			// 
			// butNewWad2
			// 
			butNewWad2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butNewWad2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butNewWad2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butNewWad2.Image = Properties.Resources.general_create_new_16;
			butNewWad2.ImageTransparentColor = System.Drawing.Color.Magenta;
			butNewWad2.Name = "butNewWad2";
			butNewWad2.Size = new System.Drawing.Size(23, 25);
			butNewWad2.Text = "New empty Wad2";
			butNewWad2.Click += butNewWad_Click;
			// 
			// butOpenDestWad
			// 
			butOpenDestWad.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butOpenDestWad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butOpenDestWad.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butOpenDestWad.Image = Properties.Resources.opened_folder_16;
			butOpenDestWad.ImageTransparentColor = System.Drawing.Color.Magenta;
			butOpenDestWad.Name = "butOpenDestWad";
			butOpenDestWad.Size = new System.Drawing.Size(23, 25);
			butOpenDestWad.Text = "Open destination file";
			butOpenDestWad.Click += butOpenDestWad_Click;
			// 
			// butSave
			// 
			butSave.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butSave.Enabled = false;
			butSave.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butSave.Image = Properties.Resources.save_16;
			butSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			butSave.Name = "butSave";
			butSave.Size = new System.Drawing.Size(23, 25);
			butSave.Text = "Save Wad2";
			butSave.Click += butSave_Click;
			// 
			// butSaveAs
			// 
			butSaveAs.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butSaveAs.Enabled = false;
			butSaveAs.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butSaveAs.Image = Properties.Resources.save_as_16;
			butSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
			butSaveAs.Name = "butSaveAs";
			butSaveAs.Size = new System.Drawing.Size(23, 25);
			butSaveAs.Text = "Save Wad2 as...";
			butSaveAs.Click += butSaveAs_Click;
			// 
			// toolStripSeparator3
			// 
			toolStripSeparator3.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			toolStripSeparator3.Name = "toolStripSeparator3";
			toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
			// 
			// labelSource
			// 
			labelSource.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			labelSource.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			labelSource.Name = "labelSource";
			labelSource.Size = new System.Drawing.Size(46, 25);
			labelSource.Text = "Source:";
			// 
			// butOpenSourceWad
			// 
			butOpenSourceWad.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butOpenSourceWad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butOpenSourceWad.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butOpenSourceWad.Image = Properties.Resources.general_Open_16;
			butOpenSourceWad.ImageTransparentColor = System.Drawing.Color.Magenta;
			butOpenSourceWad.Name = "butOpenSourceWad";
			butOpenSourceWad.Size = new System.Drawing.Size(23, 25);
			butOpenSourceWad.Text = "Open source file";
			butOpenSourceWad.Click += butOpenSourceWad_Click;
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
			// 
			// labelRefProject
			// 
			labelRefProject.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			labelRefProject.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			labelRefProject.Name = "labelRefProject";
			labelRefProject.Size = new System.Drawing.Size(102, 25);
			labelRefProject.Text = "Reference project:";
			// 
			// lblRefLevel
			// 
			lblRefLevel.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			lblRefLevel.Enabled = false;
			lblRefLevel.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblRefLevel.Name = "lblRefLevel";
			lblRefLevel.Size = new System.Drawing.Size(112, 25);
			lblRefLevel.Text = "(project not loaded)";
			lblRefLevel.Click += lblRefLevel_Click;
			// 
			// butOpenRefLevel
			// 
			butOpenRefLevel.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butOpenRefLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butOpenRefLevel.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butOpenRefLevel.Image = Properties.Resources.general_Open_16;
			butOpenRefLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
			butOpenRefLevel.Name = "butOpenRefLevel";
			butOpenRefLevel.Size = new System.Drawing.Size(23, 25);
			butOpenRefLevel.Text = "Open source file";
			butOpenRefLevel.ToolTipText = "Open reference project";
			butOpenRefLevel.Click += butOpenRefLevel_Click;
			// 
			// butCloseRefLevel
			// 
			butCloseRefLevel.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butCloseRefLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butCloseRefLevel.Enabled = false;
			butCloseRefLevel.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butCloseRefLevel.Image = Properties.Resources.actions_delete_16;
			butCloseRefLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
			butCloseRefLevel.Name = "butCloseRefLevel";
			butCloseRefLevel.Size = new System.Drawing.Size(23, 25);
			butCloseRefLevel.ToolTipText = "Close reference project";
			butCloseRefLevel.Click += butCloseRefLevel_Click;
			// 
			// tableLayoutPanel1
			// 
			tableLayoutPanel1.ColumnCount = 3;
			tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26F));
			tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48F));
			tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26F));
			tableLayoutPanel1.Controls.Add(panel2, 2, 0);
			tableLayoutPanel1.Controls.Add(panel1, 0, 0);
			tableLayoutPanel1.Controls.Add(panel3, 1, 0);
			tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			tableLayoutPanel1.Location = new System.Drawing.Point(0, 52);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			tableLayoutPanel1.RowCount = 1;
			tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 659F));
			tableLayoutPanel1.Size = new System.Drawing.Size(1244, 659);
			tableLayoutPanel1.TabIndex = 0;
			// 
			// panel2
			// 
			panel2.Controls.Add(panelSource);
			panel2.Controls.Add(darkToolStrip4);
			panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			panel2.Location = new System.Drawing.Point(920, 0);
			panel2.Margin = new System.Windows.Forms.Padding(0);
			panel2.Name = "panel2";
			panel2.Size = new System.Drawing.Size(324, 659);
			panel2.TabIndex = 11;
			// 
			// panelSource
			// 
			panelSource.Controls.Add(treeSourceWad);
			panelSource.Dock = System.Windows.Forms.DockStyle.Fill;
			panelSource.Location = new System.Drawing.Point(0, 0);
			panelSource.Name = "panelSource";
			panelSource.SectionHeader = "Source";
			panelSource.Size = new System.Drawing.Size(324, 631);
			panelSource.TabIndex = 12;
			// 
			// treeSourceWad
			// 
			treeSourceWad.Dock = System.Windows.Forms.DockStyle.Fill;
			treeSourceWad.Location = new System.Drawing.Point(1, 25);
			treeSourceWad.Name = "treeSourceWad";
			treeSourceWad.Padding = new System.Windows.Forms.Padding(3);
			treeSourceWad.ReadOnly = false;
			treeSourceWad.Size = new System.Drawing.Size(322, 605);
			treeSourceWad.TabIndex = 8;
			treeSourceWad.ClickOnEmpty += treeSourceWad_ClickOnEmpty;
			treeSourceWad.SelectedWadObjectIdsChanged += treeSourceWad_SelectedWadObjectIdsChanged;
			treeSourceWad.DoubleClick += treeSourceWad_DoubleClick;
			treeSourceWad.KeyDown += treeSourceWad_KeyDown;
			// 
			// darkToolStrip4
			// 
			darkToolStrip4.AutoSize = false;
			darkToolStrip4.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			darkToolStrip4.Dock = System.Windows.Forms.DockStyle.Bottom;
			darkToolStrip4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			darkToolStrip4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkToolStrip4.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			darkToolStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButton4, toolStripButton5 });
			darkToolStrip4.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			darkToolStrip4.Location = new System.Drawing.Point(0, 631);
			darkToolStrip4.Name = "darkToolStrip4";
			darkToolStrip4.Padding = new System.Windows.Forms.Padding(4, 1, 1, 0);
			darkToolStrip4.Size = new System.Drawing.Size(324, 28);
			darkToolStrip4.TabIndex = 28;
			darkToolStrip4.Text = "darkToolStrip4";
			// 
			// toolStripButton4
			// 
			toolStripButton4.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripButton4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripButton4.Image = Properties.Resources.angle_left_16;
			toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolStripButton4.Name = "toolStripButton4";
			toolStripButton4.Size = new System.Drawing.Size(85, 24);
			toolStripButton4.Text = "Add object";
			toolStripButton4.Click += butAddObject_Click;
			// 
			// toolStripButton5
			// 
			toolStripButton5.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripButton5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripButton5.Image = Properties.Resources.angle_left_16;
			toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolStripButton5.Name = "toolStripButton5";
			toolStripButton5.Size = new System.Drawing.Size(169, 24);
			toolStripButton5.Text = "Add object to different slot";
			toolStripButton5.Click += butAddObjectToDifferentSlot_Click;
			// 
			// panel1
			// 
			panel1.Controls.Add(panelDestination);
			panel1.Controls.Add(darkToolStrip3);
			panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			panel1.Location = new System.Drawing.Point(0, 0);
			panel1.Margin = new System.Windows.Forms.Padding(0);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(323, 659);
			panel1.TabIndex = 10;
			// 
			// panelDestination
			// 
			panelDestination.Controls.Add(treeDestWad);
			panelDestination.Dock = System.Windows.Forms.DockStyle.Fill;
			panelDestination.Location = new System.Drawing.Point(0, 0);
			panelDestination.Name = "panelDestination";
			panelDestination.SectionHeader = "Destination";
			panelDestination.Size = new System.Drawing.Size(323, 631);
			panelDestination.TabIndex = 23;
			// 
			// treeDestWad
			// 
			treeDestWad.Dock = System.Windows.Forms.DockStyle.Fill;
			treeDestWad.Location = new System.Drawing.Point(1, 25);
			treeDestWad.Name = "treeDestWad";
			treeDestWad.Padding = new System.Windows.Forms.Padding(3);
			treeDestWad.ReadOnly = false;
			treeDestWad.Size = new System.Drawing.Size(321, 605);
			treeDestWad.TabIndex = 7;
			treeDestWad.ClickOnEmpty += treeDestWad_ClickOnEmpty;
			treeDestWad.SelectedWadObjectIdsChanged += treeDestWad_SelectedWadObjectIdsChanged;
			treeDestWad.MetadataChanged += treeDestWad_MetadataChanged;
			treeDestWad.DoubleClick += treeDestWad_DoubleClick;
			treeDestWad.KeyDown += treeDestWad_KeyDown;
			// 
			// darkToolStrip3
			// 
			darkToolStrip3.AutoSize = false;
			darkToolStrip3.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			darkToolStrip3.Dock = System.Windows.Forms.DockStyle.Bottom;
			darkToolStrip3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			darkToolStrip3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkToolStrip3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			darkToolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButton1, toolStripButton2, toolStripButton3 });
			darkToolStrip3.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			darkToolStrip3.Location = new System.Drawing.Point(0, 631);
			darkToolStrip3.Name = "darkToolStrip3";
			darkToolStrip3.Padding = new System.Windows.Forms.Padding(4, 1, 1, 0);
			darkToolStrip3.Size = new System.Drawing.Size(323, 28);
			darkToolStrip3.TabIndex = 28;
			darkToolStrip3.Text = "darkToolStrip3";
			// 
			// toolStripButton1
			// 
			toolStripButton1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripButton1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripButton1.Image = Properties.Resources.edit_16;
			toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolStripButton1.Name = "toolStripButton1";
			toolStripButton1.Size = new System.Drawing.Size(74, 24);
			toolStripButton1.Text = "Edit item";
			toolStripButton1.Click += butEditItem_Click;
			// 
			// toolStripButton2
			// 
			toolStripButton2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripButton2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripButton2.Image = Properties.Resources.copy_16;
			toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolStripButton2.Name = "toolStripButton2";
			toolStripButton2.Size = new System.Drawing.Size(90, 24);
			toolStripButton2.Text = "Change slot";
			toolStripButton2.Click += butChangeSlot_Click;
			// 
			// toolStripButton3
			// 
			toolStripButton3.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripButton3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripButton3.Image = Properties.Resources.trash_16;
			toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
			toolStripButton3.Name = "toolStripButton3";
			toolStripButton3.Size = new System.Drawing.Size(60, 24);
			toolStripButton3.Text = "Delete";
			toolStripButton3.Click += butDeleteObject_Click;
			// 
			// panel3
			// 
			panel3.Controls.Add(darkSectionPanel3);
			panel3.Controls.Add(darkToolStrip2);
			panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			panel3.Location = new System.Drawing.Point(323, 0);
			panel3.Margin = new System.Windows.Forms.Padding(0);
			panel3.Name = "panel3";
			panel3.Size = new System.Drawing.Size(597, 659);
			panel3.TabIndex = 12;
			// 
			// darkSectionPanel3
			// 
			darkSectionPanel3.Controls.Add(panel3D);
			darkSectionPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			darkSectionPanel3.Location = new System.Drawing.Point(0, 0);
			darkSectionPanel3.Name = "darkSectionPanel3";
			darkSectionPanel3.SectionHeader = null;
			darkSectionPanel3.Size = new System.Drawing.Size(597, 631);
			darkSectionPanel3.TabIndex = 22;
			// 
			// panel3D
			// 
			panel3D.AllowRendering = true;
			panel3D.AnimatePreview = true;
			panel3D.Dock = System.Windows.Forms.DockStyle.Fill;
			panel3D.DrawTransparency = false;
			panel3D.Location = new System.Drawing.Point(1, 1);
			panel3D.Name = "panel3D";
			panel3D.Padding = new System.Windows.Forms.Padding(3);
			panel3D.Size = new System.Drawing.Size(595, 629);
			panel3D.TabIndex = 9;
			// 
			// darkToolStrip2
			// 
			darkToolStrip2.AutoSize = false;
			darkToolStrip2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			darkToolStrip2.Dock = System.Windows.Forms.DockStyle.Bottom;
			darkToolStrip2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			darkToolStrip2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkToolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			darkToolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { butEditAnimations, butEditSkeleton, butEditStaticModel, butEditSpriteSequence });
			darkToolStrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			darkToolStrip2.Location = new System.Drawing.Point(0, 631);
			darkToolStrip2.Name = "darkToolStrip2";
			darkToolStrip2.Padding = new System.Windows.Forms.Padding(4, 1, 1, 0);
			darkToolStrip2.Size = new System.Drawing.Size(597, 28);
			darkToolStrip2.TabIndex = 27;
			darkToolStrip2.Text = "darkToolStrip2";
			// 
			// butEditAnimations
			// 
			butEditAnimations.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butEditAnimations.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butEditAnimations.Image = Properties.Resources.animations_16;
			butEditAnimations.ImageTransparentColor = System.Drawing.Color.Magenta;
			butEditAnimations.Name = "butEditAnimations";
			butEditAnimations.Size = new System.Drawing.Size(109, 24);
			butEditAnimations.Text = "Edit animations";
			butEditAnimations.Click += butEditAnimations_Click;
			// 
			// butEditSkeleton
			// 
			butEditSkeleton.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butEditSkeleton.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butEditSkeleton.Image = Properties.Resources.skeleton_16;
			butEditSkeleton.ImageTransparentColor = System.Drawing.Color.Magenta;
			butEditSkeleton.Name = "butEditSkeleton";
			butEditSkeleton.Size = new System.Drawing.Size(94, 24);
			butEditSkeleton.Text = "Edit skeleton";
			butEditSkeleton.Click += butEditSkeleton_Click;
			// 
			// butEditStaticModel
			// 
			butEditStaticModel.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butEditStaticModel.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butEditStaticModel.Image = Properties.Resources.edit_16;
			butEditStaticModel.ImageTransparentColor = System.Drawing.Color.Magenta;
			butEditStaticModel.Name = "butEditStaticModel";
			butEditStaticModel.Size = new System.Drawing.Size(115, 24);
			butEditStaticModel.Text = "Edit static model";
			butEditStaticModel.Click += butEditStaticModel_Click;
			// 
			// butEditSpriteSequence
			// 
			butEditSpriteSequence.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butEditSpriteSequence.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butEditSpriteSequence.Image = Properties.Resources.movie_projector_16;
			butEditSpriteSequence.ImageTransparentColor = System.Drawing.Color.Magenta;
			butEditSpriteSequence.Name = "butEditSpriteSequence";
			butEditSpriteSequence.Size = new System.Drawing.Size(132, 24);
			butEditSpriteSequence.Text = "Edit sprite sequence";
			butEditSpriteSequence.Click += butEditSpriteSequence_Click;
			// 
			// contextMenuMoveableItem
			// 
			contextMenuMoveableItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			contextMenuMoveableItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			contextMenuMoveableItem.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { editAnimationsToolStripMenuItem, editSkeletonToolStripMenuItem, editMeshToolStripMenuItem, toolStripMenuItem6, toolStripMenuItemMoveablesChangeSlot, toolStripMenuItemMoveablesDelete });
			contextMenuMoveableItem.Name = "contextMenuMoveableItem";
			contextMenuMoveableItem.Size = new System.Drawing.Size(166, 121);
			// 
			// editAnimationsToolStripMenuItem
			// 
			editAnimationsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			editAnimationsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			editAnimationsToolStripMenuItem.Image = Properties.Resources.animations_16;
			editAnimationsToolStripMenuItem.Name = "editAnimationsToolStripMenuItem";
			editAnimationsToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
			editAnimationsToolStripMenuItem.Text = "Edit animations...";
			editAnimationsToolStripMenuItem.Click += editAnimationsToolStripMenuItem_Click;
			// 
			// editSkeletonToolStripMenuItem
			// 
			editSkeletonToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			editSkeletonToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			editSkeletonToolStripMenuItem.Image = Properties.Resources.skeleton_16;
			editSkeletonToolStripMenuItem.Name = "editSkeletonToolStripMenuItem";
			editSkeletonToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
			editSkeletonToolStripMenuItem.Text = "Edit skeleton...";
			editSkeletonToolStripMenuItem.Click += editSkeletonToolStripMenuItem_Click;
			// 
			// editMeshToolStripMenuItem
			// 
			editMeshToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			editMeshToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			editMeshToolStripMenuItem.Name = "editMeshToolStripMenuItem";
			editMeshToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
			editMeshToolStripMenuItem.Text = "Edit meshes...";
			editMeshToolStripMenuItem.Click += meshEditorToolStripMenuItem_Click;
			// 
			// toolStripMenuItem6
			// 
			toolStripMenuItem6.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItem6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem6.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			toolStripMenuItem6.Name = "toolStripMenuItem6";
			toolStripMenuItem6.Size = new System.Drawing.Size(162, 6);
			// 
			// toolStripMenuItemMoveablesChangeSlot
			// 
			toolStripMenuItemMoveablesChangeSlot.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItemMoveablesChangeSlot.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItemMoveablesChangeSlot.Image = Properties.Resources.replace_16;
			toolStripMenuItemMoveablesChangeSlot.Name = "toolStripMenuItemMoveablesChangeSlot";
			toolStripMenuItemMoveablesChangeSlot.Size = new System.Drawing.Size(165, 22);
			toolStripMenuItemMoveablesChangeSlot.Text = "Change slot...";
			toolStripMenuItemMoveablesChangeSlot.Click += toolStripMenuItemMoveablesChangeSlot_Click;
			// 
			// toolStripMenuItemMoveablesDelete
			// 
			toolStripMenuItemMoveablesDelete.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItemMoveablesDelete.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItemMoveablesDelete.Image = Properties.Resources.trash_16;
			toolStripMenuItemMoveablesDelete.Name = "toolStripMenuItemMoveablesDelete";
			toolStripMenuItemMoveablesDelete.Size = new System.Drawing.Size(165, 22);
			toolStripMenuItemMoveablesDelete.Text = "Delete object";
			toolStripMenuItemMoveablesDelete.Click += toolStripMenuItemMoveablesDelete_Click;
			// 
			// cmStatics
			// 
			cmStatics.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			cmStatics.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			cmStatics.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { editObjectToolStripMenuItem, editMeshToolStripMenuItem1, toolStripMenuItem4, changeSlorToolStripMenuItem, deleteObjectToolStripMenuItem });
			cmStatics.Name = "cmObject";
			cmStatics.Size = new System.Drawing.Size(147, 99);
			// 
			// editObjectToolStripMenuItem
			// 
			editObjectToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			editObjectToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			editObjectToolStripMenuItem.Image = Properties.Resources.edit_16;
			editObjectToolStripMenuItem.Name = "editObjectToolStripMenuItem";
			editObjectToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			editObjectToolStripMenuItem.Text = "Edit object...";
			editObjectToolStripMenuItem.Click += editObjectToolStripMenuItem_Click;
			// 
			// editMeshToolStripMenuItem1
			// 
			editMeshToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			editMeshToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			editMeshToolStripMenuItem1.Name = "editMeshToolStripMenuItem1";
			editMeshToolStripMenuItem1.Size = new System.Drawing.Size(146, 22);
			editMeshToolStripMenuItem1.Text = "Edit mesh...";
			editMeshToolStripMenuItem1.Click += meshEditorToolStripMenuItem_Click;
			// 
			// toolStripMenuItem4
			// 
			toolStripMenuItem4.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItem4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem4.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			toolStripMenuItem4.Name = "toolStripMenuItem4";
			toolStripMenuItem4.Size = new System.Drawing.Size(143, 6);
			// 
			// changeSlorToolStripMenuItem
			// 
			changeSlorToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			changeSlorToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			changeSlorToolStripMenuItem.Image = Properties.Resources.replace_16;
			changeSlorToolStripMenuItem.Name = "changeSlorToolStripMenuItem";
			changeSlorToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			changeSlorToolStripMenuItem.Text = "Change slot...";
			changeSlorToolStripMenuItem.Click += changeSlotToolStripMenuItem_Click;
			// 
			// deleteObjectToolStripMenuItem
			// 
			deleteObjectToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			deleteObjectToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			deleteObjectToolStripMenuItem.Image = Properties.Resources.trash_16;
			deleteObjectToolStripMenuItem.Name = "deleteObjectToolStripMenuItem";
			deleteObjectToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
			deleteObjectToolStripMenuItem.Text = "Delete object";
			deleteObjectToolStripMenuItem.Click += deleteObjectToolStripMenuItem_Click;
			// 
			// FormMain
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(1244, 742);
			Controls.Add(tableLayoutPanel1);
			Controls.Add(statusStrip);
			Controls.Add(darkToolStrip1);
			Controls.Add(darkMenuStrip1);
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MainMenuStrip = darkMenuStrip1;
			MinimumSize = new System.Drawing.Size(800, 600);
			Name = "FormMain";
			StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "WadTool";
			statusStrip.ResumeLayout(false);
			statusStrip.PerformLayout();
			darkMenuStrip1.ResumeLayout(false);
			darkMenuStrip1.PerformLayout();
			darkToolStrip1.ResumeLayout(false);
			darkToolStrip1.PerformLayout();
			tableLayoutPanel1.ResumeLayout(false);
			panel2.ResumeLayout(false);
			panelSource.ResumeLayout(false);
			darkToolStrip4.ResumeLayout(false);
			darkToolStrip4.PerformLayout();
			panel1.ResumeLayout(false);
			panelDestination.ResumeLayout(false);
			darkToolStrip3.ResumeLayout(false);
			darkToolStrip3.PerformLayout();
			panel3.ResumeLayout(false);
			darkSectionPanel3.ResumeLayout(false);
			darkToolStrip2.ResumeLayout(false);
			darkToolStrip2.PerformLayout();
			contextMenuMoveableItem.ResumeLayout(false);
			cmStatics.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private DarkUI.Controls.DarkStatusStrip statusStrip;
        private DarkUI.Controls.DarkMenuStrip darkMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSourceWADToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openDestinationWadToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveWad2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private DarkUI.Controls.DarkToolStrip darkToolStrip1;
        private TombLib.Controls.WadTreeView treeDestWad;
        private TombLib.Controls.WadTreeView treeSourceWad;
        private WadTool.Controls.PanelRenderingMainPreview panel3D;
        private System.Windows.Forms.ToolStripButton butOpenDestWad;
        private System.Windows.Forms.ToolStripButton butOpenSourceWad;
        private System.Windows.Forms.ToolStripButton butSave;
        private System.Windows.Forms.ToolStripMenuItem saveWad2AsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton butSaveAs;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction0ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem debugAction3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction4ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction6ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutWadToolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction7ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction8ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction9ToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton butNewWad2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem newWad2ToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ToolStripMenuItem createToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newMoveableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newStaticToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newSpriteSequenceToolStripMenuItem;
        private DarkUI.Controls.DarkContextMenu contextMenuMoveableItem;
        private System.Windows.Forms.ToolStripMenuItem editSkeletonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editAnimationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel labelStatistics;
        private DarkUI.Controls.DarkContextMenu cmStatics;
        private System.Windows.Forms.ToolStripMenuItem changeSlorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMoveablesChangeSlot;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMoveablesDelete;
        private System.Windows.Forms.ToolStripMenuItem editObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private DarkUI.Controls.DarkSectionPanel panelDestination;
        private DarkUI.Controls.DarkSectionPanel panelSource;
        private DarkUI.Controls.DarkToolStrip darkToolStrip2;
        private System.Windows.Forms.ToolStripButton butEditSkeleton;
        private System.Windows.Forms.ToolStripButton butEditAnimations;
        private System.Windows.Forms.ToolStripButton butEditStaticModel;
        private System.Windows.Forms.ToolStripButton butEditSpriteSequence;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel3;
        private DarkUI.Controls.DarkToolStrip darkToolStrip3;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private DarkUI.Controls.DarkToolStrip darkToolStrip4;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripLabel labelDest;
        private System.Windows.Forms.ToolStripLabel labelSource;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel lblRefLevel;
        private System.Windows.Forms.ToolStripButton butOpenRefLevel;
        private System.Windows.Forms.ToolStripButton butCloseRefLevel;
        private System.Windows.Forms.ToolStripMenuItem closeReferenceLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openReferenceLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripLabel labelRefProject;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openRecentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem meshEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem editMeshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editMeshToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem convertDestinationWadToTombEngineToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem selectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lightingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem convertSelectionToStaticLightingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem convertSelectionToDynamicLightingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem texturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem convertToUVMappedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem convertToTiledToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem animatedTexturesToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
	}
}

