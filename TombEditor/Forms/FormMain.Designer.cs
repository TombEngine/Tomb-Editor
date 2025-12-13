using System.Windows.Forms;

namespace TombEditor.Forms
{
    partial class FormMain : DarkUI.Forms.DarkForm
    {
        #region Windows Form Designer generated code

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			menuStrip = new DarkUI.Controls.DarkMenuStrip();
			fileToolStripMenuItem = new ToolStripMenuItem();
			newLevelToolStripMenuItem = new ToolStripMenuItem();
			openLevelToolStripMenuItem = new ToolStripMenuItem();
			openRecentToolStripMenuItem = new ToolStripMenuItem();
			saveLevelToolStripMenuItem = new ToolStripMenuItem();
			saveAsToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuSeparator1 = new ToolStripSeparator();
			importTRLEPRJToolStripMenuItem = new ToolStripMenuItem();
			convertToTENToolstripMenuItem = new ToolStripMenuItem();
			toolStripMenuSeparator2 = new ToolStripSeparator();
			buildLevelPlayToolStripMenuItem = new ToolStripMenuItem();
			buildLevelToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuSeparator3 = new ToolStripSeparator();
			exitToolStripMenuItem = new ToolStripMenuItem();
			editToolStripMenuItem = new ToolStripMenuItem();
			undoToolStripMenuItem = new ToolStripMenuItem();
			redoToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator2 = new ToolStripSeparator();
			cutToolStripMenuItem = new ToolStripMenuItem();
			copyToolStripMenuItem = new ToolStripMenuItem();
			pasteToolStripMenuItem = new ToolStripMenuItem();
			stampToolStripMenuItem = new ToolStripMenuItem();
			deleteToolStripMenuItem = new ToolStripMenuItem();
			selectAllToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator4 = new ToolStripSeparator();
			bookmarkObjectToolStripMenuItem = new ToolStripMenuItem();
			bookmarkRestoreObjectToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator1 = new ToolStripSeparator();
			editObjectToolStripMenuItem = new ToolStripMenuItem();
			editEventSetsToolStripMenuItem = new ToolStripMenuItem();
			editGlobalEventSetsToolStripMenuItem = new ToolStripMenuItem();
			searchToolStripMenuItem = new ToolStripMenuItem();
			searchAndReplaceToolStripMenuItem = new ToolStripMenuItem();
			viewToolStripMenuItem = new ToolStripMenuItem();
			resetCameraToolStripMenuItem = new ToolStripMenuItem();
			relocateCameraToolStripMenuItem = new ToolStripMenuItem();
			toggleFlyModeToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator8 = new ToolStripSeparator();
			drawWhiteTextureLightingOnlyToolStripMenuItem = new ToolStripMenuItem();
			ShowRealTintForObjectsToolStripMenuItem = new ToolStripMenuItem();
			roomsToolStripMenuItem = new ToolStripMenuItem();
			newRoomToolStripMenuItem = new ToolStripMenuItem();
			newRoomUpToolStripMenuItem = new ToolStripMenuItem();
			newRoomDownToolStripMenuItem = new ToolStripMenuItem();
			newRoomLeftToolStripMenuItem = new ToolStripMenuItem();
			newRoomRightToolStripMenuItem = new ToolStripMenuItem();
			newRoomFrontToolStripMenuItem = new ToolStripMenuItem();
			newRoomBackToolStripMenuItem = new ToolStripMenuItem();
			duplicateRoomToolStripMenuItem = new ToolStripMenuItem();
			cropRoomToolStripMenuItem = new ToolStripMenuItem();
			splitRoomToolStripMenuItem = new ToolStripMenuItem();
			mergeRoomsHorizontallyToolStripMenuItem = new ToolStripMenuItem();
			deleteRoomsToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuSeparator5 = new ToolStripSeparator();
			moveRoomsToolStripMenuItem = new ToolStripMenuItem();
			wholeRoomUpToolStripMenuItem = new ToolStripMenuItem();
			moveRoomUp4ClicksToolStripMenuItem = new ToolStripMenuItem();
			wholeRoomDownToolStripMenuItem = new ToolStripMenuItem();
			moveRoomDown4ClicksToolStripMenuItem = new ToolStripMenuItem();
			moveRoomLeftToolStripMenuItem = new ToolStripMenuItem();
			moveRoomRightToolStripMenuItem = new ToolStripMenuItem();
			moveRoomForwardToolStripMenuItem = new ToolStripMenuItem();
			moveRoomBackToolStripMenuItem = new ToolStripMenuItem();
			transformRoomsToolStripMenuItem = new ToolStripMenuItem();
			rotateRoomsToolStripMenuItem = new ToolStripMenuItem();
			rotateRoomsCountercockwiseToolStripMenuItem = new ToolStripMenuItem();
			mirrorRoomsOnXAxisToolStripMenuItem = new ToolStripMenuItem();
			mirrorRoomsOnZAxisToolStripMenuItem = new ToolStripMenuItem();
			selectRoomsToolStripMenuItem = new ToolStripMenuItem();
			selectWaterRoomsToolStripMenuItem = new ToolStripMenuItem();
			selectSkyRoomsToolStripMenuItem = new ToolStripMenuItem();
			selectOutsideRoomsToolStripMenuItem = new ToolStripMenuItem();
			selectToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator6 = new ToolStripSeparator();
			selectConnectedRoomsToolStripMenuItem = new ToolStripMenuItem();
			selectRoomsByTagsToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem1 = new ToolStripSeparator();
			exportRoomToolStripMenuItem = new ToolStripMenuItem();
			importRoomsToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem9 = new ToolStripMenuItem();
			itemsToolStripMenuItem = new ToolStripMenuItem();
			addWadToolStripMenuItem = new ToolStripMenuItem();
			removeWadsToolStripMenuItem = new ToolStripMenuItem();
			reloadWadsToolStripMenuItem = new ToolStripMenuItem();
			reloadSoundsToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuSeparator6 = new ToolStripSeparator();
			toolStripMenuItem8 = new ToolStripMenuItem();
			addCameraToolStripMenuItem = new ToolStripMenuItem();
			addFlybyCameraToolStripMenuItem = new ToolStripMenuItem();
			addSpriteToolStripMenuItem = new ToolStripMenuItem();
			addSinkToolStripMenuItem = new ToolStripMenuItem();
			addSoundSourceToolStripMenuItem = new ToolStripMenuItem();
			addImportedGeometryToolStripMenuItem = new ToolStripMenuItem();
			addGhostBlockToolStripMenuItem = new ToolStripMenuItem();
			addMemoToolStripMenuItem = new ToolStripMenuItem();
			addPortalToolStripMenuItem = new ToolStripMenuItem();
			addTriggerToolStripMenuItem = new ToolStripMenuItem();
			addBoxVolumeToolStripMenuItem = new ToolStripMenuItem();
			addSphereVolumeToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator7 = new ToolStripSeparator();
			deleteAllToolStripMenuItem = new ToolStripMenuItem();
			deleteAllLightsToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem2 = new ToolStripMenuItem();
			deleteAllTriggersToolStripMenuItem = new ToolStripMenuItem();
			deleteMissingObjectsToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuSeparator8 = new ToolStripSeparator();
			findObjectToolStripMenuItem = new ToolStripMenuItem();
			moveLaraToolStripMenuItem = new ToolStripMenuItem();
			selectItemsInSelectedAreaToolStripMenuItem = new ToolStripMenuItem();
			selectFloorBelowObjectToolStripMenuItem = new ToolStripMenuItem();
			splitSectorObjectOnSelectionToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator3 = new ToolStripSeparator();
			setStaticMeshColorToRoomLightToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem10 = new ToolStripMenuItem();
			toolStripSeparator9 = new ToolStripSeparator();
			makeQuickItemGroupToolStripMenuItem = new ToolStripMenuItem();
			getObjectStatisticsToolStripMenuItem = new ToolStripMenuItem();
			generateObjectNamesToolStripMenuItem = new ToolStripMenuItem();
			texturesToolStripMenuItem = new ToolStripMenuItem();
			loadTextureToolStripMenuItem = new ToolStripMenuItem();
			removeTexturesToolStripMenuItem = new ToolStripMenuItem();
			unloadTexturesToolStripMenuItem = new ToolStripMenuItem();
			reloadTexturesToolStripMenuItem = new ToolStripMenuItem();
			importConvertTexturesToPng = new ToolStripMenuItem();
			remapTextureToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuSeparator9 = new ToolStripSeparator();
			textureFloorToolStripMenuItem = new ToolStripMenuItem();
			textureWallsToolStripMenuItem = new ToolStripMenuItem();
			textureCeilingToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem3 = new ToolStripSeparator();
			clearAllTexturesInRoomToolStripMenuItem = new ToolStripMenuItem();
			clearAllTexturesInRoomToolStripMenuItem1 = new ToolStripMenuItem();
			toolStripMenuSeparator10 = new ToolStripSeparator();
			findTexturesToolStripMenuItem = new ToolStripMenuItem();
			animationRangesToolStripMenuItem = new ToolStripMenuItem();
			transformToolStripMenuItem = new ToolStripMenuItem();
			increaseStepHeightToolStripMenuItem = new ToolStripMenuItem();
			decreaseStepHeightToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator10 = new ToolStripSeparator();
			smoothRandomFloorUpToolStripMenuItem = new ToolStripMenuItem();
			smoothRandomFloorDownToolStripMenuItem = new ToolStripMenuItem();
			smoothRandomCeilingUpToolStripMenuItem = new ToolStripMenuItem();
			smoothRandomCeilingDownToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuSeparator11 = new ToolStripSeparator();
			sharpRandomFloorUpToolStripMenuItem = new ToolStripMenuItem();
			sharpRandomFloorDownToolStripMenuItem = new ToolStripMenuItem();
			sharpRandomCeilingUpToolStripMenuItem = new ToolStripMenuItem();
			sharpRandomCeilingDownToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuSeparator12 = new ToolStripSeparator();
			realignFloorToStepHeightToolStripMenuItem = new ToolStripMenuItem();
			realignCeilingToStepHeightToolStripMenuItem = new ToolStripMenuItem();
			smoothFloorToolStripMenuItem = new ToolStripMenuItem();
			smoothCeilingToolStripMenuItem = new ToolStripMenuItem();
			averageFloorToolStripMenuItem = new ToolStripMenuItem();
			averageCeilingToolStripMenuItem = new ToolStripMenuItem();
			flattenFloorToolStripMenuItem = new ToolStripMenuItem();
			flattenCeilingToolStripMenuItem = new ToolStripMenuItem();
			resetGeometryToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuSeparator13 = new ToolStripSeparator();
			gridWallsIn3ToolStripMenuItem = new ToolStripMenuItem();
			gridWallsIn5ToolStripMenuItem = new ToolStripMenuItem();
			gridWallsIn3SquaresToolStripMenuItem = new ToolStripMenuItem();
			gridWallsIn5SquaresToolStripMenuItem = new ToolStripMenuItem();
			convertFloorToQuadsToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem4 = new ToolStripMenuItem();
			toolStripMenuItem5 = new ToolStripMenuItem();
			editOptionsToolStripMenuItem = new ToolStripMenuItem();
			keyboardLayoutToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator5 = new ToolStripSeparator();
			toolStripMenuItem7 = new ToolStripMenuItem();
			toolStripMenuItem6 = new ToolStripMenuItem();
			butFindMenu = new ToolStripMenuItem();
			windowToolStripMenuItem = new ToolStripMenuItem();
			restoreDefaultLayoutToolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuSeparator14 = new ToolStripSeparator();
			sectorOptionsToolStripMenuItem = new ToolStripMenuItem();
			roomOptionsToolStripMenuItem = new ToolStripMenuItem();
			itemBrowserToolStripMenuItem = new ToolStripMenuItem();
			importedGeometryBrowserToolstripMenuItem = new ToolStripMenuItem();
			triggerListToolStripMenuItem = new ToolStripMenuItem();
			lightingToolStripMenuItem = new ToolStripMenuItem();
			paletteToolStripMenuItem = new ToolStripMenuItem();
			texturePanelToolStripMenuItem = new ToolStripMenuItem();
			objectListToolStripMenuItem = new ToolStripMenuItem();
			statisticsToolStripMenuItem = new ToolStripMenuItem();
			dockableToolStripMenuItem = new ToolStripMenuItem();
			floatingToolStripMenuItem = new ToolStripMenuItem();
			helpToolStripMenuItem = new ToolStripMenuItem();
			aboutToolStripMenuItem = new ToolStripMenuItem();
			tbSearchMenu = new ToolStripTextBox();
			debugToolStripMenuItem = new ToolStripMenuItem();
			debugAction0ToolStripMenuItem = new ToolStripMenuItem();
			debugAction1ToolStripMenuItem = new ToolStripMenuItem();
			debugAction2ToolStripMenuItem = new ToolStripMenuItem();
			debugAction3ToolStripMenuItem = new ToolStripMenuItem();
			debugAction4ToolStripMenuItem = new ToolStripMenuItem();
			debugAction5ToolStripMenuItem = new ToolStripMenuItem();
			debugScriptToolStripMenuItem = new ToolStripMenuItem();
			butRoomDown = new DarkUI.Controls.DarkButton();
			butEditRoomName = new DarkUI.Controls.DarkButton();
			butDeleteRoom = new DarkUI.Controls.DarkButton();
			statusStrip = new DarkUI.Controls.DarkStatusStrip();
			statusStripSelectedRoom = new ToolStripStatusLabel();
			statusStripGlobalSelectionArea = new ToolStripStatusLabel();
			statusStripLocalSelectionArea = new ToolStripStatusLabel();
			statusAutosave = new ToolStripStatusLabel();
			dockArea = new DarkUI.Docking.DarkDockPanel();
			panelDockArea = new Panel();
			assToolStripMenuItem = new ToolStripMenuItem();
			convertCeilingToQuadsToolStripMenuItem = new ToolStripMenuItem();
			menuStrip.SuspendLayout();
			statusStrip.SuspendLayout();
			panelDockArea.SuspendLayout();
			SuspendLayout();
			// 
			// menuStrip
			// 
			menuStrip.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			menuStrip.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, viewToolStripMenuItem, roomsToolStripMenuItem, itemsToolStripMenuItem, texturesToolStripMenuItem, transformToolStripMenuItem, toolStripMenuItem4, butFindMenu, windowToolStripMenuItem, helpToolStripMenuItem, tbSearchMenu, debugToolStripMenuItem });
			menuStrip.Location = new System.Drawing.Point(0, 0);
			menuStrip.Name = "menuStrip";
			menuStrip.Padding = new Padding(3, 2, 0, 2);
			menuStrip.Size = new System.Drawing.Size(913, 29);
			menuStrip.TabIndex = 0;
			menuStrip.Text = "darkMenuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			fileToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newLevelToolStripMenuItem, openLevelToolStripMenuItem, openRecentToolStripMenuItem, saveLevelToolStripMenuItem, saveAsToolStripMenuItem, toolStripMenuSeparator1, importTRLEPRJToolStripMenuItem, convertToTENToolstripMenuItem, toolStripMenuSeparator2, buildLevelPlayToolStripMenuItem, buildLevelToolStripMenuItem, toolStripMenuSeparator3, exitToolStripMenuItem });
			fileToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			fileToolStripMenuItem.Size = new System.Drawing.Size(37, 25);
			fileToolStripMenuItem.Text = "File";
			// 
			// newLevelToolStripMenuItem
			// 
			newLevelToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			newLevelToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			newLevelToolStripMenuItem.Image = Properties.Resources.general_create_new_16;
			newLevelToolStripMenuItem.Name = "newLevelToolStripMenuItem";
			newLevelToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
			newLevelToolStripMenuItem.Tag = "NewLevel";
			newLevelToolStripMenuItem.Text = "NewLevel";
			// 
			// openLevelToolStripMenuItem
			// 
			openLevelToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			openLevelToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			openLevelToolStripMenuItem.Image = Properties.Resources.general_Open_16;
			openLevelToolStripMenuItem.Name = "openLevelToolStripMenuItem";
			openLevelToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
			openLevelToolStripMenuItem.Tag = "OpenLevel";
			openLevelToolStripMenuItem.Text = "OpenLevel";
			// 
			// openRecentToolStripMenuItem
			// 
			openRecentToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			openRecentToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			openRecentToolStripMenuItem.Name = "openRecentToolStripMenuItem";
			openRecentToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
			openRecentToolStripMenuItem.Text = "Open recent";
			// 
			// saveLevelToolStripMenuItem
			// 
			saveLevelToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			saveLevelToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			saveLevelToolStripMenuItem.Image = Properties.Resources.general_Save_16;
			saveLevelToolStripMenuItem.Name = "saveLevelToolStripMenuItem";
			saveLevelToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
			saveLevelToolStripMenuItem.Tag = "SaveLevel";
			saveLevelToolStripMenuItem.Text = "SaveLevel";
			// 
			// saveAsToolStripMenuItem
			// 
			saveAsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			saveAsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			saveAsToolStripMenuItem.Image = Properties.Resources.general_Save_As_16;
			saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			saveAsToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
			saveAsToolStripMenuItem.Tag = "SaveLevelAs";
			saveAsToolStripMenuItem.Text = "SaveLevelAs";
			// 
			// toolStripMenuSeparator1
			// 
			toolStripMenuSeparator1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuSeparator1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuSeparator1.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuSeparator1.Name = "toolStripMenuSeparator1";
			toolStripMenuSeparator1.Size = new System.Drawing.Size(218, 6);
			// 
			// importTRLEPRJToolStripMenuItem
			// 
			importTRLEPRJToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			importTRLEPRJToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			importTRLEPRJToolStripMenuItem.Image = Properties.Resources.general_Import_16;
			importTRLEPRJToolStripMenuItem.Name = "importTRLEPRJToolStripMenuItem";
			importTRLEPRJToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
			importTRLEPRJToolStripMenuItem.Tag = "ImportPrj";
			importTRLEPRJToolStripMenuItem.Text = "ImportPrj";
			// 
			// convertToTENToolstripMenuItem
			// 
			convertToTENToolstripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			convertToTENToolstripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			convertToTENToolstripMenuItem.Image = Properties.Resources.actions_TEN_16;
			convertToTENToolstripMenuItem.Name = "convertToTENToolstripMenuItem";
			convertToTENToolstripMenuItem.Size = new System.Drawing.Size(221, 22);
			convertToTENToolstripMenuItem.Tag = "ConvertLevelToTombEngine";
			convertToTENToolstripMenuItem.Text = "ConvertLevelToTombEngine";
			// 
			// toolStripMenuSeparator2
			// 
			toolStripMenuSeparator2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuSeparator2.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuSeparator2.Name = "toolStripMenuSeparator2";
			toolStripMenuSeparator2.Size = new System.Drawing.Size(218, 6);
			// 
			// buildLevelPlayToolStripMenuItem
			// 
			buildLevelPlayToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			buildLevelPlayToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			buildLevelPlayToolStripMenuItem.Image = Properties.Resources.actions_play_16;
			buildLevelPlayToolStripMenuItem.Name = "buildLevelPlayToolStripMenuItem";
			buildLevelPlayToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
			buildLevelPlayToolStripMenuItem.Tag = "BuildAndPlay";
			buildLevelPlayToolStripMenuItem.Text = "BuildAndPlay";
			// 
			// buildLevelToolStripMenuItem
			// 
			buildLevelToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			buildLevelToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			buildLevelToolStripMenuItem.Image = Properties.Resources.actions_compile_16;
			buildLevelToolStripMenuItem.Name = "buildLevelToolStripMenuItem";
			buildLevelToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
			buildLevelToolStripMenuItem.Tag = "BuildLevel";
			buildLevelToolStripMenuItem.Text = "BuildLevel";
			// 
			// toolStripMenuSeparator3
			// 
			toolStripMenuSeparator3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuSeparator3.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuSeparator3.Name = "toolStripMenuSeparator3";
			toolStripMenuSeparator3.Size = new System.Drawing.Size(218, 6);
			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			exitToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
			exitToolStripMenuItem.Tag = "QuitEditor";
			exitToolStripMenuItem.Text = "QuitEditor";
			// 
			// editToolStripMenuItem
			// 
			editToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { undoToolStripMenuItem, redoToolStripMenuItem, toolStripSeparator2, cutToolStripMenuItem, copyToolStripMenuItem, pasteToolStripMenuItem, stampToolStripMenuItem, deleteToolStripMenuItem, selectAllToolStripMenuItem, toolStripSeparator4, bookmarkObjectToolStripMenuItem, bookmarkRestoreObjectToolStripMenuItem, toolStripSeparator1, editObjectToolStripMenuItem, editEventSetsToolStripMenuItem, editGlobalEventSetsToolStripMenuItem, searchToolStripMenuItem, searchAndReplaceToolStripMenuItem });
			editToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			editToolStripMenuItem.Name = "editToolStripMenuItem";
			editToolStripMenuItem.Size = new System.Drawing.Size(39, 25);
			editToolStripMenuItem.Text = "Edit";
			// 
			// undoToolStripMenuItem
			// 
			undoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			undoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			undoToolStripMenuItem.Image = Properties.Resources.general_undo_16;
			undoToolStripMenuItem.Name = "undoToolStripMenuItem";
			undoToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			undoToolStripMenuItem.Tag = "Undo";
			undoToolStripMenuItem.Text = "Undo";
			// 
			// redoToolStripMenuItem
			// 
			redoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			redoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			redoToolStripMenuItem.Image = Properties.Resources.general_redo_16;
			redoToolStripMenuItem.Name = "redoToolStripMenuItem";
			redoToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			redoToolStripMenuItem.Tag = "Redo";
			redoToolStripMenuItem.Text = "Redo";
			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator2.Margin = new Padding(0, 0, 0, 1);
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new System.Drawing.Size(209, 6);
			// 
			// cutToolStripMenuItem
			// 
			cutToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			cutToolStripMenuItem.Enabled = false;
			cutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			cutToolStripMenuItem.Name = "cutToolStripMenuItem";
			cutToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			cutToolStripMenuItem.Tag = "Cut";
			cutToolStripMenuItem.Text = "Cut";
			// 
			// copyToolStripMenuItem
			// 
			copyToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			copyToolStripMenuItem.Enabled = false;
			copyToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			copyToolStripMenuItem.Image = Properties.Resources.general_copy_16;
			copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			copyToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			copyToolStripMenuItem.Tag = "Copy";
			copyToolStripMenuItem.Text = "Copy";
			// 
			// pasteToolStripMenuItem
			// 
			pasteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			pasteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			pasteToolStripMenuItem.Image = Properties.Resources.general_clipboard_16;
			pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
			pasteToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			pasteToolStripMenuItem.Tag = "Paste";
			pasteToolStripMenuItem.Text = "Paste";
			// 
			// stampToolStripMenuItem
			// 
			stampToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			stampToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			stampToolStripMenuItem.Image = Properties.Resources.actions_rubber_stamp_16;
			stampToolStripMenuItem.Name = "stampToolStripMenuItem";
			stampToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			stampToolStripMenuItem.Tag = "StampObject";
			stampToolStripMenuItem.Text = "StampObject";
			// 
			// deleteToolStripMenuItem
			// 
			deleteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			deleteToolStripMenuItem.Enabled = false;
			deleteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			deleteToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			deleteToolStripMenuItem.Tag = "Delete";
			deleteToolStripMenuItem.Text = "Delete";
			// 
			// selectAllToolStripMenuItem
			// 
			selectAllToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			selectAllToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
			selectAllToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			selectAllToolStripMenuItem.Tag = "SelectAll";
			selectAllToolStripMenuItem.Text = "SelectAll";
			// 
			// toolStripSeparator4
			// 
			toolStripSeparator4.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator4.Margin = new Padding(0, 0, 0, 1);
			toolStripSeparator4.Name = "toolStripSeparator4";
			toolStripSeparator4.Size = new System.Drawing.Size(209, 6);
			// 
			// bookmarkObjectToolStripMenuItem
			// 
			bookmarkObjectToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			bookmarkObjectToolStripMenuItem.Enabled = false;
			bookmarkObjectToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			bookmarkObjectToolStripMenuItem.Name = "bookmarkObjectToolStripMenuItem";
			bookmarkObjectToolStripMenuItem.ShortcutKeyDisplayString = "";
			bookmarkObjectToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			bookmarkObjectToolStripMenuItem.Tag = "BookmarkObject";
			bookmarkObjectToolStripMenuItem.Text = "BookmarkObject";
			// 
			// bookmarkRestoreObjectToolStripMenuItem
			// 
			bookmarkRestoreObjectToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			bookmarkRestoreObjectToolStripMenuItem.Enabled = false;
			bookmarkRestoreObjectToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			bookmarkRestoreObjectToolStripMenuItem.Name = "bookmarkRestoreObjectToolStripMenuItem";
			bookmarkRestoreObjectToolStripMenuItem.ShortcutKeyDisplayString = "";
			bookmarkRestoreObjectToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			bookmarkRestoreObjectToolStripMenuItem.Tag = "SelectBookmarkedObject";
			bookmarkRestoreObjectToolStripMenuItem.Text = "SelectBookmarkedObject";
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator1.Margin = new Padding(0, 0, 0, 1);
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new System.Drawing.Size(209, 6);
			// 
			// editObjectToolStripMenuItem
			// 
			editObjectToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			editObjectToolStripMenuItem.Enabled = false;
			editObjectToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			editObjectToolStripMenuItem.Name = "editObjectToolStripMenuItem";
			editObjectToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			editObjectToolStripMenuItem.Tag = "EditObject";
			editObjectToolStripMenuItem.Text = "EditObject";
			// 
			// editEventSetsToolStripMenuItem
			// 
			editEventSetsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			editEventSetsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			editEventSetsToolStripMenuItem.Name = "editEventSetsToolStripMenuItem";
			editEventSetsToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			editEventSetsToolStripMenuItem.Tag = "EditVolumeEventSets";
			editEventSetsToolStripMenuItem.Text = "EditVolumeEventSets";
			// 
			// editGlobalEventSetsToolStripMenuItem
			// 
			editGlobalEventSetsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			editGlobalEventSetsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			editGlobalEventSetsToolStripMenuItem.Name = "editGlobalEventSetsToolStripMenuItem";
			editGlobalEventSetsToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			editGlobalEventSetsToolStripMenuItem.Tag = "EditGlobalEventSets";
			editGlobalEventSetsToolStripMenuItem.Text = "EditGlobalEventSets";
			// 
			// searchToolStripMenuItem
			// 
			searchToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			searchToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			searchToolStripMenuItem.Image = Properties.Resources.general_search_16;
			searchToolStripMenuItem.Name = "searchToolStripMenuItem";
			searchToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			searchToolStripMenuItem.Tag = "Search";
			searchToolStripMenuItem.Text = "Search";
			// 
			// searchAndReplaceToolStripMenuItem
			// 
			searchAndReplaceToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			searchAndReplaceToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			searchAndReplaceToolStripMenuItem.Image = Properties.Resources.general_search_and_replace_16;
			searchAndReplaceToolStripMenuItem.Name = "searchAndReplaceToolStripMenuItem";
			searchAndReplaceToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
			searchAndReplaceToolStripMenuItem.Tag = "SearchAndReplaceObjects";
			searchAndReplaceToolStripMenuItem.Text = "SearchAndReplaceObjects";
			// 
			// viewToolStripMenuItem
			// 
			viewToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { resetCameraToolStripMenuItem, relocateCameraToolStripMenuItem, toggleFlyModeToolStripMenuItem, toolStripSeparator8, drawWhiteTextureLightingOnlyToolStripMenuItem, ShowRealTintForObjectsToolStripMenuItem });
			viewToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			viewToolStripMenuItem.Name = "viewToolStripMenuItem";
			viewToolStripMenuItem.Size = new System.Drawing.Size(44, 25);
			viewToolStripMenuItem.Text = "View";
			// 
			// resetCameraToolStripMenuItem
			// 
			resetCameraToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			resetCameraToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			resetCameraToolStripMenuItem.Image = Properties.Resources.actions_center_direction_16;
			resetCameraToolStripMenuItem.Name = "resetCameraToolStripMenuItem";
			resetCameraToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
			resetCameraToolStripMenuItem.Tag = "ResetCamera";
			resetCameraToolStripMenuItem.Text = "ResetCamera";
			// 
			// relocateCameraToolStripMenuItem
			// 
			relocateCameraToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			relocateCameraToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			relocateCameraToolStripMenuItem.Name = "relocateCameraToolStripMenuItem";
			relocateCameraToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
			relocateCameraToolStripMenuItem.Tag = "RelocateCamera";
			relocateCameraToolStripMenuItem.Text = "RelocateCamera";
			// 
			// toggleFlyModeToolStripMenuItem
			// 
			toggleFlyModeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toggleFlyModeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toggleFlyModeToolStripMenuItem.Image = Properties.Resources.general_airplane_16;
			toggleFlyModeToolStripMenuItem.Name = "toggleFlyModeToolStripMenuItem";
			toggleFlyModeToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
			toggleFlyModeToolStripMenuItem.Tag = "ToggleFlyMode";
			toggleFlyModeToolStripMenuItem.Text = "ToggleFlyMode";
			// 
			// toolStripSeparator8
			// 
			toolStripSeparator8.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator8.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator8.Margin = new Padding(0, 0, 0, 1);
			toolStripSeparator8.Name = "toolStripSeparator8";
			toolStripSeparator8.Size = new System.Drawing.Size(236, 6);
			// 
			// drawWhiteTextureLightingOnlyToolStripMenuItem
			// 
			drawWhiteTextureLightingOnlyToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			drawWhiteTextureLightingOnlyToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			drawWhiteTextureLightingOnlyToolStripMenuItem.Image = Properties.Resources.actions_DrawUntexturedLights_16;
			drawWhiteTextureLightingOnlyToolStripMenuItem.Name = "drawWhiteTextureLightingOnlyToolStripMenuItem";
			drawWhiteTextureLightingOnlyToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
			drawWhiteTextureLightingOnlyToolStripMenuItem.Tag = "DrawWhiteTextureLightingOnly";
			drawWhiteTextureLightingOnlyToolStripMenuItem.Text = "DrawWhiteTextureLightingOnly";
			// 
			// ShowRealTintForObjectsToolStripMenuItem
			// 
			ShowRealTintForObjectsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			ShowRealTintForObjectsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			ShowRealTintForObjectsToolStripMenuItem.Image = Properties.Resources.actions_StaticTint_16;
			ShowRealTintForObjectsToolStripMenuItem.Name = "ShowRealTintForObjectsToolStripMenuItem";
			ShowRealTintForObjectsToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
			ShowRealTintForObjectsToolStripMenuItem.Tag = "ShowRealTintForObjects";
			ShowRealTintForObjectsToolStripMenuItem.Text = "ShowRealTintForObjects";
			// 
			// roomsToolStripMenuItem
			// 
			roomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			roomsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newRoomToolStripMenuItem, duplicateRoomToolStripMenuItem, cropRoomToolStripMenuItem, splitRoomToolStripMenuItem, mergeRoomsHorizontallyToolStripMenuItem, deleteRoomsToolStripMenuItem, toolStripMenuSeparator5, moveRoomsToolStripMenuItem, transformRoomsToolStripMenuItem, selectRoomsToolStripMenuItem, toolStripMenuItem1, exportRoomToolStripMenuItem, importRoomsToolStripMenuItem, toolStripMenuItem9 });
			roomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			roomsToolStripMenuItem.Name = "roomsToolStripMenuItem";
			roomsToolStripMenuItem.Size = new System.Drawing.Size(56, 25);
			roomsToolStripMenuItem.Text = "Rooms";
			// 
			// newRoomToolStripMenuItem
			// 
			newRoomToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			newRoomToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newRoomUpToolStripMenuItem, newRoomDownToolStripMenuItem, newRoomLeftToolStripMenuItem, newRoomRightToolStripMenuItem, newRoomFrontToolStripMenuItem, newRoomBackToolStripMenuItem });
			newRoomToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			newRoomToolStripMenuItem.Name = "newRoomToolStripMenuItem";
			newRoomToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			newRoomToolStripMenuItem.Text = "New room";
			// 
			// newRoomUpToolStripMenuItem
			// 
			newRoomUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			newRoomUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			newRoomUpToolStripMenuItem.Name = "newRoomUpToolStripMenuItem";
			newRoomUpToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			newRoomUpToolStripMenuItem.Tag = "NewRoomUp";
			newRoomUpToolStripMenuItem.Text = "NewRoomUp";
			// 
			// newRoomDownToolStripMenuItem
			// 
			newRoomDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			newRoomDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			newRoomDownToolStripMenuItem.Name = "newRoomDownToolStripMenuItem";
			newRoomDownToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			newRoomDownToolStripMenuItem.Tag = "NewRoomDown";
			newRoomDownToolStripMenuItem.Text = "NewRoomDown";
			// 
			// newRoomLeftToolStripMenuItem
			// 
			newRoomLeftToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			newRoomLeftToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			newRoomLeftToolStripMenuItem.Name = "newRoomLeftToolStripMenuItem";
			newRoomLeftToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			newRoomLeftToolStripMenuItem.Tag = "NewRoomLeft";
			newRoomLeftToolStripMenuItem.Text = "NewRoomLeft";
			// 
			// newRoomRightToolStripMenuItem
			// 
			newRoomRightToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			newRoomRightToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			newRoomRightToolStripMenuItem.Name = "newRoomRightToolStripMenuItem";
			newRoomRightToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			newRoomRightToolStripMenuItem.Tag = "NewRoomRight";
			newRoomRightToolStripMenuItem.Text = "NewRoomRight";
			// 
			// newRoomFrontToolStripMenuItem
			// 
			newRoomFrontToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			newRoomFrontToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			newRoomFrontToolStripMenuItem.Name = "newRoomFrontToolStripMenuItem";
			newRoomFrontToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			newRoomFrontToolStripMenuItem.Tag = "NewRoomFront";
			newRoomFrontToolStripMenuItem.Text = "NewRoomFront";
			// 
			// newRoomBackToolStripMenuItem
			// 
			newRoomBackToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			newRoomBackToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			newRoomBackToolStripMenuItem.Name = "newRoomBackToolStripMenuItem";
			newRoomBackToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			newRoomBackToolStripMenuItem.Tag = "NewRoomBack";
			newRoomBackToolStripMenuItem.Text = "NewRoomBack";
			// 
			// duplicateRoomToolStripMenuItem
			// 
			duplicateRoomToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			duplicateRoomToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			duplicateRoomToolStripMenuItem.Image = Properties.Resources.general_copy_16;
			duplicateRoomToolStripMenuItem.Name = "duplicateRoomToolStripMenuItem";
			duplicateRoomToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			duplicateRoomToolStripMenuItem.Tag = "DuplicateRoom";
			duplicateRoomToolStripMenuItem.Text = "DuplicateRoom";
			// 
			// cropRoomToolStripMenuItem
			// 
			cropRoomToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			cropRoomToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			cropRoomToolStripMenuItem.Image = Properties.Resources.general_crop_16;
			cropRoomToolStripMenuItem.Name = "cropRoomToolStripMenuItem";
			cropRoomToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			cropRoomToolStripMenuItem.Tag = "CropRoom";
			cropRoomToolStripMenuItem.Text = "CropRoom";
			// 
			// splitRoomToolStripMenuItem
			// 
			splitRoomToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			splitRoomToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			splitRoomToolStripMenuItem.Image = Properties.Resources.actions_Split_16;
			splitRoomToolStripMenuItem.Name = "splitRoomToolStripMenuItem";
			splitRoomToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			splitRoomToolStripMenuItem.Tag = "SplitRoom";
			splitRoomToolStripMenuItem.Text = "SplitRoom";
			// 
			// mergeRoomsHorizontallyToolStripMenuItem
			// 
			mergeRoomsHorizontallyToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			mergeRoomsHorizontallyToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			mergeRoomsHorizontallyToolStripMenuItem.Image = Properties.Resources.actions_Merge_16;
			mergeRoomsHorizontallyToolStripMenuItem.Name = "mergeRoomsHorizontallyToolStripMenuItem";
			mergeRoomsHorizontallyToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			mergeRoomsHorizontallyToolStripMenuItem.Tag = "MergeRoomsHorizontally";
			mergeRoomsHorizontallyToolStripMenuItem.Text = "MergeRoomsHorizontally";
			// 
			// deleteRoomsToolStripMenuItem
			// 
			deleteRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			deleteRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			deleteRoomsToolStripMenuItem.Image = Properties.Resources.general_trash_16;
			deleteRoomsToolStripMenuItem.Name = "deleteRoomsToolStripMenuItem";
			deleteRoomsToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			deleteRoomsToolStripMenuItem.Tag = "DeleteRooms";
			deleteRoomsToolStripMenuItem.Text = "DeleteRooms";
			// 
			// toolStripMenuSeparator5
			// 
			toolStripMenuSeparator5.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuSeparator5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuSeparator5.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuSeparator5.Name = "toolStripMenuSeparator5";
			toolStripMenuSeparator5.Size = new System.Drawing.Size(206, 6);
			// 
			// moveRoomsToolStripMenuItem
			// 
			moveRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			moveRoomsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { wholeRoomUpToolStripMenuItem, moveRoomUp4ClicksToolStripMenuItem, wholeRoomDownToolStripMenuItem, moveRoomDown4ClicksToolStripMenuItem, moveRoomLeftToolStripMenuItem, moveRoomRightToolStripMenuItem, moveRoomForwardToolStripMenuItem, moveRoomBackToolStripMenuItem });
			moveRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			moveRoomsToolStripMenuItem.Name = "moveRoomsToolStripMenuItem";
			moveRoomsToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			moveRoomsToolStripMenuItem.Text = "Move rooms";
			// 
			// wholeRoomUpToolStripMenuItem
			// 
			wholeRoomUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			wholeRoomUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			wholeRoomUpToolStripMenuItem.Name = "wholeRoomUpToolStripMenuItem";
			wholeRoomUpToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			wholeRoomUpToolStripMenuItem.Tag = "MoveRoomUp";
			wholeRoomUpToolStripMenuItem.Text = "MoveRoomUp";
			// 
			// moveRoomUp4ClicksToolStripMenuItem
			// 
			moveRoomUp4ClicksToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			moveRoomUp4ClicksToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			moveRoomUp4ClicksToolStripMenuItem.Name = "moveRoomUp4ClicksToolStripMenuItem";
			moveRoomUp4ClicksToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			moveRoomUp4ClicksToolStripMenuItem.Tag = "MoveRoomUp4Clicks";
			moveRoomUp4ClicksToolStripMenuItem.Text = "MoveRoomUp4Clicks";
			// 
			// wholeRoomDownToolStripMenuItem
			// 
			wholeRoomDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			wholeRoomDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			wholeRoomDownToolStripMenuItem.Name = "wholeRoomDownToolStripMenuItem";
			wholeRoomDownToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			wholeRoomDownToolStripMenuItem.Tag = "MoveRoomDown";
			wholeRoomDownToolStripMenuItem.Text = "MoveRoomDown";
			// 
			// moveRoomDown4ClicksToolStripMenuItem
			// 
			moveRoomDown4ClicksToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			moveRoomDown4ClicksToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			moveRoomDown4ClicksToolStripMenuItem.Name = "moveRoomDown4ClicksToolStripMenuItem";
			moveRoomDown4ClicksToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			moveRoomDown4ClicksToolStripMenuItem.Tag = "MoveRoomDown4Clicks";
			moveRoomDown4ClicksToolStripMenuItem.Text = "MoveRoomDown4Clicks";
			// 
			// moveRoomLeftToolStripMenuItem
			// 
			moveRoomLeftToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			moveRoomLeftToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			moveRoomLeftToolStripMenuItem.Name = "moveRoomLeftToolStripMenuItem";
			moveRoomLeftToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			moveRoomLeftToolStripMenuItem.Tag = "MoveRoomLeft";
			moveRoomLeftToolStripMenuItem.Text = "MoveRoomLeft";
			// 
			// moveRoomRightToolStripMenuItem
			// 
			moveRoomRightToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			moveRoomRightToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			moveRoomRightToolStripMenuItem.Name = "moveRoomRightToolStripMenuItem";
			moveRoomRightToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			moveRoomRightToolStripMenuItem.Tag = "MoveRoomRight";
			moveRoomRightToolStripMenuItem.Text = "MoveRoomRight";
			// 
			// moveRoomForwardToolStripMenuItem
			// 
			moveRoomForwardToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			moveRoomForwardToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			moveRoomForwardToolStripMenuItem.Name = "moveRoomForwardToolStripMenuItem";
			moveRoomForwardToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			moveRoomForwardToolStripMenuItem.Tag = "MoveRoomForward";
			moveRoomForwardToolStripMenuItem.Text = "MoveRoomForward";
			// 
			// moveRoomBackToolStripMenuItem
			// 
			moveRoomBackToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			moveRoomBackToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			moveRoomBackToolStripMenuItem.Name = "moveRoomBackToolStripMenuItem";
			moveRoomBackToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			moveRoomBackToolStripMenuItem.Tag = "MoveRoomBack";
			moveRoomBackToolStripMenuItem.Text = "MoveRoomBack";
			// 
			// transformRoomsToolStripMenuItem
			// 
			transformRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			transformRoomsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { rotateRoomsToolStripMenuItem, rotateRoomsCountercockwiseToolStripMenuItem, mirrorRoomsOnXAxisToolStripMenuItem, mirrorRoomsOnZAxisToolStripMenuItem });
			transformRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			transformRoomsToolStripMenuItem.Name = "transformRoomsToolStripMenuItem";
			transformRoomsToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			transformRoomsToolStripMenuItem.Text = "Transform rooms";
			// 
			// rotateRoomsToolStripMenuItem
			// 
			rotateRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			rotateRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			rotateRoomsToolStripMenuItem.Name = "rotateRoomsToolStripMenuItem";
			rotateRoomsToolStripMenuItem.Size = new System.Drawing.Size(241, 22);
			rotateRoomsToolStripMenuItem.Tag = "RotateRoomsClockwise";
			rotateRoomsToolStripMenuItem.Text = "RotateRoomsClockwise";
			// 
			// rotateRoomsCountercockwiseToolStripMenuItem
			// 
			rotateRoomsCountercockwiseToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			rotateRoomsCountercockwiseToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			rotateRoomsCountercockwiseToolStripMenuItem.Name = "rotateRoomsCountercockwiseToolStripMenuItem";
			rotateRoomsCountercockwiseToolStripMenuItem.Size = new System.Drawing.Size(241, 22);
			rotateRoomsCountercockwiseToolStripMenuItem.Tag = "RotateRoomsCounterClockwise";
			rotateRoomsCountercockwiseToolStripMenuItem.Text = "RotateRoomsCounterClockwise";
			// 
			// mirrorRoomsOnXAxisToolStripMenuItem
			// 
			mirrorRoomsOnXAxisToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			mirrorRoomsOnXAxisToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			mirrorRoomsOnXAxisToolStripMenuItem.Name = "mirrorRoomsOnXAxisToolStripMenuItem";
			mirrorRoomsOnXAxisToolStripMenuItem.Size = new System.Drawing.Size(241, 22);
			mirrorRoomsOnXAxisToolStripMenuItem.Tag = "MirrorRoomsX";
			mirrorRoomsOnXAxisToolStripMenuItem.Text = "MirrorRoomsX";
			// 
			// mirrorRoomsOnZAxisToolStripMenuItem
			// 
			mirrorRoomsOnZAxisToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			mirrorRoomsOnZAxisToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			mirrorRoomsOnZAxisToolStripMenuItem.Name = "mirrorRoomsOnZAxisToolStripMenuItem";
			mirrorRoomsOnZAxisToolStripMenuItem.Size = new System.Drawing.Size(241, 22);
			mirrorRoomsOnZAxisToolStripMenuItem.Tag = "MirrorRoomsZ";
			mirrorRoomsOnZAxisToolStripMenuItem.Text = "MirrorRoomsZ";
			// 
			// selectRoomsToolStripMenuItem
			// 
			selectRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			selectRoomsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { selectWaterRoomsToolStripMenuItem, selectSkyRoomsToolStripMenuItem, selectOutsideRoomsToolStripMenuItem, selectToolStripMenuItem, toolStripSeparator6, selectConnectedRoomsToolStripMenuItem, selectRoomsByTagsToolStripMenuItem });
			selectRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			selectRoomsToolStripMenuItem.Name = "selectRoomsToolStripMenuItem";
			selectRoomsToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			selectRoomsToolStripMenuItem.Text = "Select rooms";
			// 
			// selectWaterRoomsToolStripMenuItem
			// 
			selectWaterRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			selectWaterRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			selectWaterRoomsToolStripMenuItem.Name = "selectWaterRoomsToolStripMenuItem";
			selectWaterRoomsToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			selectWaterRoomsToolStripMenuItem.Tag = "SelectWaterRooms";
			selectWaterRoomsToolStripMenuItem.Text = "SelectWaterRooms";
			// 
			// selectSkyRoomsToolStripMenuItem
			// 
			selectSkyRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			selectSkyRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			selectSkyRoomsToolStripMenuItem.Name = "selectSkyRoomsToolStripMenuItem";
			selectSkyRoomsToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			selectSkyRoomsToolStripMenuItem.Tag = "SelectSkyRooms";
			selectSkyRoomsToolStripMenuItem.Text = "SelectSkyRooms";
			// 
			// selectOutsideRoomsToolStripMenuItem
			// 
			selectOutsideRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			selectOutsideRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			selectOutsideRoomsToolStripMenuItem.Name = "selectOutsideRoomsToolStripMenuItem";
			selectOutsideRoomsToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			selectOutsideRoomsToolStripMenuItem.Tag = "SelectOutsideRooms";
			selectOutsideRoomsToolStripMenuItem.Text = "SelectOutsideRooms";
			// 
			// selectToolStripMenuItem
			// 
			selectToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			selectToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			selectToolStripMenuItem.Name = "selectToolStripMenuItem";
			selectToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			selectToolStripMenuItem.Tag = "SelectQuicksandRooms";
			selectToolStripMenuItem.Text = "SelectQuicksandRooms";
			// 
			// toolStripSeparator6
			// 
			toolStripSeparator6.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator6.Margin = new Padding(0, 0, 0, 1);
			toolStripSeparator6.Name = "toolStripSeparator6";
			toolStripSeparator6.Size = new System.Drawing.Size(197, 6);
			// 
			// selectConnectedRoomsToolStripMenuItem
			// 
			selectConnectedRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			selectConnectedRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			selectConnectedRoomsToolStripMenuItem.Name = "selectConnectedRoomsToolStripMenuItem";
			selectConnectedRoomsToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			selectConnectedRoomsToolStripMenuItem.Tag = "SelectConnectedRooms";
			selectConnectedRoomsToolStripMenuItem.Text = "SelectConnectedRooms";
			// 
			// selectRoomsByTagsToolStripMenuItem
			// 
			selectRoomsByTagsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			selectRoomsByTagsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			selectRoomsByTagsToolStripMenuItem.Name = "selectRoomsByTagsToolStripMenuItem";
			selectRoomsByTagsToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			selectRoomsByTagsToolStripMenuItem.Tag = "SelectRoomsByTags";
			selectRoomsByTagsToolStripMenuItem.Text = "SelectRoomsByTags";
			// 
			// toolStripMenuItem1
			// 
			toolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem1.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuItem1.Name = "toolStripMenuItem1";
			toolStripMenuItem1.Size = new System.Drawing.Size(206, 6);
			// 
			// exportRoomToolStripMenuItem
			// 
			exportRoomToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			exportRoomToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			exportRoomToolStripMenuItem.Image = Properties.Resources.general_Export_16;
			exportRoomToolStripMenuItem.Name = "exportRoomToolStripMenuItem";
			exportRoomToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			exportRoomToolStripMenuItem.Tag = "ExportRooms";
			exportRoomToolStripMenuItem.Text = "ExportRooms";
			// 
			// importRoomsToolStripMenuItem
			// 
			importRoomsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			importRoomsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			importRoomsToolStripMenuItem.Image = Properties.Resources.general_Import_16;
			importRoomsToolStripMenuItem.Name = "importRoomsToolStripMenuItem";
			importRoomsToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			importRoomsToolStripMenuItem.Tag = "ImportRooms";
			importRoomsToolStripMenuItem.Text = "ImportRooms";
			importRoomsToolStripMenuItem.Visible = false;
			// 
			// toolStripMenuItem9
			// 
			toolStripMenuItem9.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItem9.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem9.Name = "toolStripMenuItem9";
			toolStripMenuItem9.Size = new System.Drawing.Size(209, 22);
			toolStripMenuItem9.Tag = "ApplyRoomProperties";
			toolStripMenuItem9.Text = "ApplyRoomProperties";
			// 
			// itemsToolStripMenuItem
			// 
			itemsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			itemsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { addWadToolStripMenuItem, removeWadsToolStripMenuItem, reloadWadsToolStripMenuItem, reloadSoundsToolStripMenuItem, toolStripMenuSeparator6, toolStripMenuItem8, addPortalToolStripMenuItem, addTriggerToolStripMenuItem, addBoxVolumeToolStripMenuItem, addSphereVolumeToolStripMenuItem, toolStripSeparator7, deleteAllToolStripMenuItem, toolStripMenuSeparator8, findObjectToolStripMenuItem, moveLaraToolStripMenuItem, selectItemsInSelectedAreaToolStripMenuItem, selectFloorBelowObjectToolStripMenuItem, splitSectorObjectOnSelectionToolStripMenuItem, toolStripSeparator3, setStaticMeshColorToRoomLightToolStripMenuItem, toolStripMenuItem10, toolStripSeparator9, makeQuickItemGroupToolStripMenuItem, getObjectStatisticsToolStripMenuItem, generateObjectNamesToolStripMenuItem });
			itemsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			itemsToolStripMenuItem.Name = "itemsToolStripMenuItem";
			itemsToolStripMenuItem.Size = new System.Drawing.Size(48, 25);
			itemsToolStripMenuItem.Text = "Items";
			// 
			// addWadToolStripMenuItem
			// 
			addWadToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			addWadToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			addWadToolStripMenuItem.Image = Properties.Resources.general_plus_math_16;
			addWadToolStripMenuItem.Name = "addWadToolStripMenuItem";
			addWadToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			addWadToolStripMenuItem.Tag = "AddWad";
			addWadToolStripMenuItem.Text = "AddWad";
			// 
			// removeWadsToolStripMenuItem
			// 
			removeWadsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			removeWadsToolStripMenuItem.Enabled = false;
			removeWadsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			removeWadsToolStripMenuItem.Image = Properties.Resources.general_trash_16;
			removeWadsToolStripMenuItem.Name = "removeWadsToolStripMenuItem";
			removeWadsToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			removeWadsToolStripMenuItem.Tag = "RemoveWads";
			removeWadsToolStripMenuItem.Text = "RemoveWads";
			// 
			// reloadWadsToolStripMenuItem
			// 
			reloadWadsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			reloadWadsToolStripMenuItem.Enabled = false;
			reloadWadsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			reloadWadsToolStripMenuItem.Name = "reloadWadsToolStripMenuItem";
			reloadWadsToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			reloadWadsToolStripMenuItem.Tag = "ReloadWads";
			reloadWadsToolStripMenuItem.Text = "ReloadWads";
			// 
			// reloadSoundsToolStripMenuItem
			// 
			reloadSoundsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			reloadSoundsToolStripMenuItem.Enabled = false;
			reloadSoundsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			reloadSoundsToolStripMenuItem.Name = "reloadSoundsToolStripMenuItem";
			reloadSoundsToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			reloadSoundsToolStripMenuItem.Tag = "ReloadSounds";
			reloadSoundsToolStripMenuItem.Text = "ReloadSounds";
			// 
			// toolStripMenuSeparator6
			// 
			toolStripMenuSeparator6.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuSeparator6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuSeparator6.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuSeparator6.Name = "toolStripMenuSeparator6";
			toolStripMenuSeparator6.Size = new System.Drawing.Size(256, 6);
			// 
			// toolStripMenuItem8
			// 
			toolStripMenuItem8.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItem8.DropDownItems.AddRange(new ToolStripItem[] { addCameraToolStripMenuItem, addFlybyCameraToolStripMenuItem, addSpriteToolStripMenuItem, addSinkToolStripMenuItem, addSoundSourceToolStripMenuItem, addImportedGeometryToolStripMenuItem, addGhostBlockToolStripMenuItem, addMemoToolStripMenuItem });
			toolStripMenuItem8.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem8.Name = "toolStripMenuItem8";
			toolStripMenuItem8.Size = new System.Drawing.Size(259, 22);
			toolStripMenuItem8.Text = "Add item";
			// 
			// addCameraToolStripMenuItem
			// 
			addCameraToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			addCameraToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			addCameraToolStripMenuItem.Image = Properties.Resources.objects_Camera_16;
			addCameraToolStripMenuItem.Name = "addCameraToolStripMenuItem";
			addCameraToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			addCameraToolStripMenuItem.Tag = "AddCamera";
			addCameraToolStripMenuItem.Text = "AddCamera";
			// 
			// addFlybyCameraToolStripMenuItem
			// 
			addFlybyCameraToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			addFlybyCameraToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			addFlybyCameraToolStripMenuItem.Image = Properties.Resources.objects_movie_projector_16;
			addFlybyCameraToolStripMenuItem.Name = "addFlybyCameraToolStripMenuItem";
			addFlybyCameraToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			addFlybyCameraToolStripMenuItem.Tag = "AddFlybyCamera";
			addFlybyCameraToolStripMenuItem.Text = "AddFlybyCamera";
			// 
			// addSpriteToolStripMenuItem
			// 
			addSpriteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			addSpriteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			addSpriteToolStripMenuItem.Image = Properties.Resources.objects_Sprite_16;
			addSpriteToolStripMenuItem.Name = "addSpriteToolStripMenuItem";
			addSpriteToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			addSpriteToolStripMenuItem.Tag = "AddSprite";
			addSpriteToolStripMenuItem.Text = "AddSprite";
			// 
			// addSinkToolStripMenuItem
			// 
			addSinkToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			addSinkToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			addSinkToolStripMenuItem.Image = Properties.Resources.objects_tornado_16;
			addSinkToolStripMenuItem.Name = "addSinkToolStripMenuItem";
			addSinkToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			addSinkToolStripMenuItem.Tag = "AddSink";
			addSinkToolStripMenuItem.Text = "AddSink";
			// 
			// addSoundSourceToolStripMenuItem
			// 
			addSoundSourceToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			addSoundSourceToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			addSoundSourceToolStripMenuItem.Image = Properties.Resources.objects_speaker_16;
			addSoundSourceToolStripMenuItem.Name = "addSoundSourceToolStripMenuItem";
			addSoundSourceToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			addSoundSourceToolStripMenuItem.Tag = "AddSoundSource";
			addSoundSourceToolStripMenuItem.Text = "AddSoundSource";
			// 
			// addImportedGeometryToolStripMenuItem
			// 
			addImportedGeometryToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			addImportedGeometryToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			addImportedGeometryToolStripMenuItem.Image = Properties.Resources.objects_custom_geometry;
			addImportedGeometryToolStripMenuItem.Name = "addImportedGeometryToolStripMenuItem";
			addImportedGeometryToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			addImportedGeometryToolStripMenuItem.Tag = "AddImportedGeometry";
			addImportedGeometryToolStripMenuItem.Text = "AddImportedGeometry";
			// 
			// addGhostBlockToolStripMenuItem
			// 
			addGhostBlockToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			addGhostBlockToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			addGhostBlockToolStripMenuItem.Image = Properties.Resources.objects_geometry_override_16;
			addGhostBlockToolStripMenuItem.Name = "addGhostBlockToolStripMenuItem";
			addGhostBlockToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			addGhostBlockToolStripMenuItem.Tag = "AddGhostBlock";
			addGhostBlockToolStripMenuItem.Text = "AddGhostBlock";
			// 
			// addMemoToolStripMenuItem
			// 
			addMemoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			addMemoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			addMemoToolStripMenuItem.Image = Properties.Resources.objects_Memo_16;
			addMemoToolStripMenuItem.Name = "addMemoToolStripMenuItem";
			addMemoToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
			addMemoToolStripMenuItem.Tag = "AddMemo";
			addMemoToolStripMenuItem.Text = "AddMemo";
			// 
			// addPortalToolStripMenuItem
			// 
			addPortalToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			addPortalToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			addPortalToolStripMenuItem.Name = "addPortalToolStripMenuItem";
			addPortalToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			addPortalToolStripMenuItem.Tag = "AddPortal";
			addPortalToolStripMenuItem.Text = "AddPortal";
			// 
			// addTriggerToolStripMenuItem
			// 
			addTriggerToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			addTriggerToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			addTriggerToolStripMenuItem.Name = "addTriggerToolStripMenuItem";
			addTriggerToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			addTriggerToolStripMenuItem.Tag = "AddTrigger";
			addTriggerToolStripMenuItem.Text = "AddTrigger";
			// 
			// addBoxVolumeToolStripMenuItem
			// 
			addBoxVolumeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			addBoxVolumeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			addBoxVolumeToolStripMenuItem.Image = Properties.Resources.objects_volume_box_16;
			addBoxVolumeToolStripMenuItem.Name = "addBoxVolumeToolStripMenuItem";
			addBoxVolumeToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			addBoxVolumeToolStripMenuItem.Tag = "AddBoxVolume";
			addBoxVolumeToolStripMenuItem.Text = "AddBoxVolume";
			addBoxVolumeToolStripMenuItem.Visible = false;
			// 
			// addSphereVolumeToolStripMenuItem
			// 
			addSphereVolumeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			addSphereVolumeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			addSphereVolumeToolStripMenuItem.Image = Properties.Resources.objects_volume_sphere_16;
			addSphereVolumeToolStripMenuItem.Name = "addSphereVolumeToolStripMenuItem";
			addSphereVolumeToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			addSphereVolumeToolStripMenuItem.Tag = "AddSphereVolume";
			addSphereVolumeToolStripMenuItem.Text = "AddSphereVolume";
			addSphereVolumeToolStripMenuItem.Visible = false;
			// 
			// toolStripSeparator7
			// 
			toolStripSeparator7.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator7.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator7.Margin = new Padding(0, 0, 0, 1);
			toolStripSeparator7.Name = "toolStripSeparator7";
			toolStripSeparator7.Size = new System.Drawing.Size(256, 6);
			// 
			// deleteAllToolStripMenuItem
			// 
			deleteAllToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			deleteAllToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { deleteAllLightsToolStripMenuItem, toolStripMenuItem2, deleteAllTriggersToolStripMenuItem, deleteMissingObjectsToolStripMenuItem });
			deleteAllToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			deleteAllToolStripMenuItem.Name = "deleteAllToolStripMenuItem";
			deleteAllToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			deleteAllToolStripMenuItem.Text = "Delete...";
			// 
			// deleteAllLightsToolStripMenuItem
			// 
			deleteAllLightsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			deleteAllLightsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			deleteAllLightsToolStripMenuItem.Name = "deleteAllLightsToolStripMenuItem";
			deleteAllLightsToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
			deleteAllLightsToolStripMenuItem.Tag = "DeleteAllLights";
			deleteAllLightsToolStripMenuItem.Text = "DeleteAllLights";
			// 
			// toolStripMenuItem2
			// 
			toolStripMenuItem2.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItem2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem2.Name = "toolStripMenuItem2";
			toolStripMenuItem2.Size = new System.Drawing.Size(188, 22);
			toolStripMenuItem2.Tag = "DeleteAllObjects";
			toolStripMenuItem2.Text = "DeleteAllObjects";
			// 
			// deleteAllTriggersToolStripMenuItem
			// 
			deleteAllTriggersToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			deleteAllTriggersToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			deleteAllTriggersToolStripMenuItem.Name = "deleteAllTriggersToolStripMenuItem";
			deleteAllTriggersToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
			deleteAllTriggersToolStripMenuItem.Tag = "DeleteAllTriggers";
			deleteAllTriggersToolStripMenuItem.Text = "DeleteAllTriggers";
			// 
			// deleteMissingObjectsToolStripMenuItem
			// 
			deleteMissingObjectsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			deleteMissingObjectsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			deleteMissingObjectsToolStripMenuItem.Name = "deleteMissingObjectsToolStripMenuItem";
			deleteMissingObjectsToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
			deleteMissingObjectsToolStripMenuItem.Tag = "DeleteMissingObjects";
			deleteMissingObjectsToolStripMenuItem.Text = "DeleteMissingObjects";
			// 
			// toolStripMenuSeparator8
			// 
			toolStripMenuSeparator8.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuSeparator8.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuSeparator8.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuSeparator8.Name = "toolStripMenuSeparator8";
			toolStripMenuSeparator8.Size = new System.Drawing.Size(256, 6);
			toolStripMenuSeparator8.Visible = false;
			// 
			// findObjectToolStripMenuItem
			// 
			findObjectToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			findObjectToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			findObjectToolStripMenuItem.Image = Properties.Resources.general_target_16;
			findObjectToolStripMenuItem.Name = "findObjectToolStripMenuItem";
			findObjectToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			findObjectToolStripMenuItem.Tag = "LocateItem";
			findObjectToolStripMenuItem.Text = "LocateItem";
			// 
			// moveLaraToolStripMenuItem
			// 
			moveLaraToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			moveLaraToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			moveLaraToolStripMenuItem.Name = "moveLaraToolStripMenuItem";
			moveLaraToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			moveLaraToolStripMenuItem.Tag = "MoveLara";
			moveLaraToolStripMenuItem.Text = "MoveLara";
			// 
			// selectItemsInSelectedAreaToolStripMenuItem
			// 
			selectItemsInSelectedAreaToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			selectItemsInSelectedAreaToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			selectItemsInSelectedAreaToolStripMenuItem.Name = "selectItemsInSelectedAreaToolStripMenuItem";
			selectItemsInSelectedAreaToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			selectItemsInSelectedAreaToolStripMenuItem.Tag = "SelectAllObjectsInArea";
			selectItemsInSelectedAreaToolStripMenuItem.Text = "SelectAllObjectsInArea";
			// 
			// selectFloorBelowObjectToolStripMenuItem
			// 
			selectFloorBelowObjectToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			selectFloorBelowObjectToolStripMenuItem.Enabled = false;
			selectFloorBelowObjectToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			selectFloorBelowObjectToolStripMenuItem.Name = "selectFloorBelowObjectToolStripMenuItem";
			selectFloorBelowObjectToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			selectFloorBelowObjectToolStripMenuItem.Tag = "SelectFloorBelowObject";
			selectFloorBelowObjectToolStripMenuItem.Text = "SelectFloorBelowObject";
			// 
			// splitSectorObjectOnSelectionToolStripMenuItem
			// 
			splitSectorObjectOnSelectionToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			splitSectorObjectOnSelectionToolStripMenuItem.Enabled = false;
			splitSectorObjectOnSelectionToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			splitSectorObjectOnSelectionToolStripMenuItem.Name = "splitSectorObjectOnSelectionToolStripMenuItem";
			splitSectorObjectOnSelectionToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			splitSectorObjectOnSelectionToolStripMenuItem.Tag = "SplitSectorObjectOnSelection";
			splitSectorObjectOnSelectionToolStripMenuItem.Text = "SplitSectorObjectOnSelection";
			// 
			// toolStripSeparator3
			// 
			toolStripSeparator3.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator3.Margin = new Padding(0, 0, 0, 1);
			toolStripSeparator3.Name = "toolStripSeparator3";
			toolStripSeparator3.Size = new System.Drawing.Size(256, 6);
			// 
			// setStaticMeshColorToRoomLightToolStripMenuItem
			// 
			setStaticMeshColorToRoomLightToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			setStaticMeshColorToRoomLightToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			setStaticMeshColorToRoomLightToolStripMenuItem.Name = "setStaticMeshColorToRoomLightToolStripMenuItem";
			setStaticMeshColorToRoomLightToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			setStaticMeshColorToRoomLightToolStripMenuItem.Tag = "SetStaticMeshesColorToRoomLight";
			setStaticMeshColorToRoomLightToolStripMenuItem.Text = "SetStaticMeshesColorToRoomLight";
			// 
			// toolStripMenuItem10
			// 
			toolStripMenuItem10.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItem10.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem10.Name = "toolStripMenuItem10";
			toolStripMenuItem10.Size = new System.Drawing.Size(259, 22);
			toolStripMenuItem10.Tag = "SetStaticMeshesColor";
			toolStripMenuItem10.Text = "SetStaticMeshesColor";
			// 
			// toolStripSeparator9
			// 
			toolStripSeparator9.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator9.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator9.Margin = new Padding(0, 0, 0, 1);
			toolStripSeparator9.Name = "toolStripSeparator9";
			toolStripSeparator9.Size = new System.Drawing.Size(256, 6);
			// 
			// makeQuickItemGroupToolStripMenuItem
			// 
			makeQuickItemGroupToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			makeQuickItemGroupToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			makeQuickItemGroupToolStripMenuItem.Name = "makeQuickItemGroupToolStripMenuItem";
			makeQuickItemGroupToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			makeQuickItemGroupToolStripMenuItem.Tag = "MakeQuickItemGroup";
			makeQuickItemGroupToolStripMenuItem.Text = "MakeQuickItemGroup";
			// 
			// getObjectStatisticsToolStripMenuItem
			// 
			getObjectStatisticsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			getObjectStatisticsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			getObjectStatisticsToolStripMenuItem.Name = "getObjectStatisticsToolStripMenuItem";
			getObjectStatisticsToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			getObjectStatisticsToolStripMenuItem.Tag = "GetObjectStatistics";
			getObjectStatisticsToolStripMenuItem.Text = "GetObjectStatistics";
			// 
			// generateObjectNamesToolStripMenuItem
			// 
			generateObjectNamesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			generateObjectNamesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			generateObjectNamesToolStripMenuItem.Name = "generateObjectNamesToolStripMenuItem";
			generateObjectNamesToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			generateObjectNamesToolStripMenuItem.Tag = "GenerateObjectNames";
			generateObjectNamesToolStripMenuItem.Text = "GenerateObjectNames";
			// 
			// texturesToolStripMenuItem
			// 
			texturesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			texturesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { loadTextureToolStripMenuItem, removeTexturesToolStripMenuItem, unloadTexturesToolStripMenuItem, reloadTexturesToolStripMenuItem, importConvertTexturesToPng, remapTextureToolStripMenuItem, toolStripMenuSeparator9, textureFloorToolStripMenuItem, textureWallsToolStripMenuItem, textureCeilingToolStripMenuItem, toolStripMenuItem3, clearAllTexturesInRoomToolStripMenuItem, clearAllTexturesInRoomToolStripMenuItem1, toolStripMenuSeparator10, findTexturesToolStripMenuItem, animationRangesToolStripMenuItem });
			texturesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			texturesToolStripMenuItem.Name = "texturesToolStripMenuItem";
			texturesToolStripMenuItem.Size = new System.Drawing.Size(62, 25);
			texturesToolStripMenuItem.Text = "Textures";
			// 
			// loadTextureToolStripMenuItem
			// 
			loadTextureToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			loadTextureToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			loadTextureToolStripMenuItem.Image = Properties.Resources.general_plus_math_16;
			loadTextureToolStripMenuItem.Name = "loadTextureToolStripMenuItem";
			loadTextureToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			loadTextureToolStripMenuItem.Tag = "AddTexture";
			loadTextureToolStripMenuItem.Text = "AddTexture";
			// 
			// removeTexturesToolStripMenuItem
			// 
			removeTexturesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			removeTexturesToolStripMenuItem.Enabled = false;
			removeTexturesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			removeTexturesToolStripMenuItem.Image = Properties.Resources.general_trash_16;
			removeTexturesToolStripMenuItem.Name = "removeTexturesToolStripMenuItem";
			removeTexturesToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			removeTexturesToolStripMenuItem.Tag = "RemoveTextures";
			removeTexturesToolStripMenuItem.Text = "RemoveTextures";
			// 
			// unloadTexturesToolStripMenuItem
			// 
			unloadTexturesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			unloadTexturesToolStripMenuItem.Enabled = false;
			unloadTexturesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			unloadTexturesToolStripMenuItem.Name = "unloadTexturesToolStripMenuItem";
			unloadTexturesToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			unloadTexturesToolStripMenuItem.Tag = "UnloadTextures";
			unloadTexturesToolStripMenuItem.Text = "UnloadTextures";
			// 
			// reloadTexturesToolStripMenuItem
			// 
			reloadTexturesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			reloadTexturesToolStripMenuItem.Enabled = false;
			reloadTexturesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			reloadTexturesToolStripMenuItem.Name = "reloadTexturesToolStripMenuItem";
			reloadTexturesToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			reloadTexturesToolStripMenuItem.Tag = "ReloadTextures";
			reloadTexturesToolStripMenuItem.Text = "ReloadTextures";
			// 
			// importConvertTexturesToPng
			// 
			importConvertTexturesToPng.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			importConvertTexturesToPng.Enabled = false;
			importConvertTexturesToPng.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			importConvertTexturesToPng.Image = Properties.Resources.general_Import_16;
			importConvertTexturesToPng.Name = "importConvertTexturesToPng";
			importConvertTexturesToPng.Size = new System.Drawing.Size(200, 22);
			importConvertTexturesToPng.Tag = "ConvertTexturesToPNG";
			importConvertTexturesToPng.Text = "ConvertTexturesToPNG";
			// 
			// remapTextureToolStripMenuItem
			// 
			remapTextureToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			remapTextureToolStripMenuItem.Enabled = false;
			remapTextureToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			remapTextureToolStripMenuItem.Image = Properties.Resources.general_crop_16;
			remapTextureToolStripMenuItem.Name = "remapTextureToolStripMenuItem";
			remapTextureToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			remapTextureToolStripMenuItem.Tag = "RemapTexture";
			remapTextureToolStripMenuItem.Text = "RemapTexture";
			// 
			// toolStripMenuSeparator9
			// 
			toolStripMenuSeparator9.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuSeparator9.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuSeparator9.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuSeparator9.Name = "toolStripMenuSeparator9";
			toolStripMenuSeparator9.Size = new System.Drawing.Size(197, 6);
			// 
			// textureFloorToolStripMenuItem
			// 
			textureFloorToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			textureFloorToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			textureFloorToolStripMenuItem.Image = Properties.Resources.texture_Floor2_16;
			textureFloorToolStripMenuItem.Name = "textureFloorToolStripMenuItem";
			textureFloorToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			textureFloorToolStripMenuItem.Tag = "TextureFloor";
			textureFloorToolStripMenuItem.Text = "TextureFloor";
			// 
			// textureWallsToolStripMenuItem
			// 
			textureWallsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			textureWallsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			textureWallsToolStripMenuItem.Image = Properties.Resources.texture_Walls2_16;
			textureWallsToolStripMenuItem.Name = "textureWallsToolStripMenuItem";
			textureWallsToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			textureWallsToolStripMenuItem.Tag = "TextureWalls";
			textureWallsToolStripMenuItem.Text = "TextureWalls";
			// 
			// textureCeilingToolStripMenuItem
			// 
			textureCeilingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			textureCeilingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			textureCeilingToolStripMenuItem.Image = Properties.Resources.texture_Ceiling2_16;
			textureCeilingToolStripMenuItem.Name = "textureCeilingToolStripMenuItem";
			textureCeilingToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			textureCeilingToolStripMenuItem.Tag = "TextureCeiling";
			textureCeilingToolStripMenuItem.Text = "TextureCeiling";
			// 
			// toolStripMenuItem3
			// 
			toolStripMenuItem3.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItem3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem3.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuItem3.Name = "toolStripMenuItem3";
			toolStripMenuItem3.Size = new System.Drawing.Size(197, 6);
			// 
			// clearAllTexturesInRoomToolStripMenuItem
			// 
			clearAllTexturesInRoomToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			clearAllTexturesInRoomToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			clearAllTexturesInRoomToolStripMenuItem.Image = Properties.Resources.toolbox_Eraser_16;
			clearAllTexturesInRoomToolStripMenuItem.Name = "clearAllTexturesInRoomToolStripMenuItem";
			clearAllTexturesInRoomToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			clearAllTexturesInRoomToolStripMenuItem.Tag = "ClearAllTexturesInRoom";
			clearAllTexturesInRoomToolStripMenuItem.Text = "ClearAllTexturesInRoom";
			// 
			// clearAllTexturesInRoomToolStripMenuItem1
			// 
			clearAllTexturesInRoomToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			clearAllTexturesInRoomToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			clearAllTexturesInRoomToolStripMenuItem1.Name = "clearAllTexturesInRoomToolStripMenuItem1";
			clearAllTexturesInRoomToolStripMenuItem1.Size = new System.Drawing.Size(200, 22);
			clearAllTexturesInRoomToolStripMenuItem1.Tag = "ClearAllTexturesInLevel";
			clearAllTexturesInRoomToolStripMenuItem1.Text = "ClearAllTexturesInLevel";
			// 
			// toolStripMenuSeparator10
			// 
			toolStripMenuSeparator10.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuSeparator10.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuSeparator10.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuSeparator10.Name = "toolStripMenuSeparator10";
			toolStripMenuSeparator10.Size = new System.Drawing.Size(197, 6);
			// 
			// findTexturesToolStripMenuItem
			// 
			findTexturesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			findTexturesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			findTexturesToolStripMenuItem.Image = Properties.Resources.general_search_16;
			findTexturesToolStripMenuItem.Name = "findTexturesToolStripMenuItem";
			findTexturesToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			findTexturesToolStripMenuItem.Tag = "SearchTextures";
			findTexturesToolStripMenuItem.Text = "SearchTextures";
			// 
			// animationRangesToolStripMenuItem
			// 
			animationRangesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			animationRangesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			animationRangesToolStripMenuItem.Image = Properties.Resources.texture_anim_ranges;
			animationRangesToolStripMenuItem.Name = "animationRangesToolStripMenuItem";
			animationRangesToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
			animationRangesToolStripMenuItem.Tag = "EditAnimationRanges";
			animationRangesToolStripMenuItem.Text = "EditAnimationRanges";
			// 
			// transformToolStripMenuItem
			// 
			transformToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			transformToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { increaseStepHeightToolStripMenuItem, decreaseStepHeightToolStripMenuItem, toolStripSeparator10, smoothRandomFloorUpToolStripMenuItem, smoothRandomFloorDownToolStripMenuItem, smoothRandomCeilingUpToolStripMenuItem, smoothRandomCeilingDownToolStripMenuItem, toolStripMenuSeparator11, sharpRandomFloorUpToolStripMenuItem, sharpRandomFloorDownToolStripMenuItem, sharpRandomCeilingUpToolStripMenuItem, sharpRandomCeilingDownToolStripMenuItem, toolStripMenuSeparator12, realignFloorToStepHeightToolStripMenuItem, realignCeilingToStepHeightToolStripMenuItem, convertFloorToQuadsToolStripMenuItem, convertCeilingToQuadsToolStripMenuItem, smoothFloorToolStripMenuItem, smoothCeilingToolStripMenuItem, averageFloorToolStripMenuItem, averageCeilingToolStripMenuItem, flattenFloorToolStripMenuItem, flattenCeilingToolStripMenuItem, resetGeometryToolStripMenuItem, toolStripMenuSeparator13, gridWallsIn3ToolStripMenuItem, gridWallsIn5ToolStripMenuItem, gridWallsIn3SquaresToolStripMenuItem, gridWallsIn5SquaresToolStripMenuItem });
			transformToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			transformToolStripMenuItem.Name = "transformToolStripMenuItem";
			transformToolStripMenuItem.Size = new System.Drawing.Size(72, 25);
			transformToolStripMenuItem.Text = "Transform";
			// 
			// increaseStepHeightToolStripMenuItem
			// 
			increaseStepHeightToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			increaseStepHeightToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			increaseStepHeightToolStripMenuItem.Name = "increaseStepHeightToolStripMenuItem";
			increaseStepHeightToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			increaseStepHeightToolStripMenuItem.Tag = "IncreaseStepHeight";
			increaseStepHeightToolStripMenuItem.Text = "IncreaseStepHeight";
			// 
			// decreaseStepHeightToolStripMenuItem
			// 
			decreaseStepHeightToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			decreaseStepHeightToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			decreaseStepHeightToolStripMenuItem.Name = "decreaseStepHeightToolStripMenuItem";
			decreaseStepHeightToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			decreaseStepHeightToolStripMenuItem.Tag = "DecreaseStepHeight";
			decreaseStepHeightToolStripMenuItem.Text = "DecreaseStepHeight";
			// 
			// toolStripSeparator10
			// 
			toolStripSeparator10.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator10.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator10.Margin = new Padding(0, 0, 0, 1);
			toolStripSeparator10.Name = "toolStripSeparator10";
			toolStripSeparator10.Size = new System.Drawing.Size(226, 6);
			// 
			// smoothRandomFloorUpToolStripMenuItem
			// 
			smoothRandomFloorUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			smoothRandomFloorUpToolStripMenuItem.Enabled = false;
			smoothRandomFloorUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			smoothRandomFloorUpToolStripMenuItem.Name = "smoothRandomFloorUpToolStripMenuItem";
			smoothRandomFloorUpToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			smoothRandomFloorUpToolStripMenuItem.Tag = "SmoothRandomFloorUp";
			smoothRandomFloorUpToolStripMenuItem.Text = "SmoothRandomFloorUp";
			// 
			// smoothRandomFloorDownToolStripMenuItem
			// 
			smoothRandomFloorDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			smoothRandomFloorDownToolStripMenuItem.Enabled = false;
			smoothRandomFloorDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			smoothRandomFloorDownToolStripMenuItem.Name = "smoothRandomFloorDownToolStripMenuItem";
			smoothRandomFloorDownToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			smoothRandomFloorDownToolStripMenuItem.Tag = "SmoothRandomFloorDown";
			smoothRandomFloorDownToolStripMenuItem.Text = "SmoothRandomFloorDown";
			// 
			// smoothRandomCeilingUpToolStripMenuItem
			// 
			smoothRandomCeilingUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			smoothRandomCeilingUpToolStripMenuItem.Enabled = false;
			smoothRandomCeilingUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			smoothRandomCeilingUpToolStripMenuItem.Name = "smoothRandomCeilingUpToolStripMenuItem";
			smoothRandomCeilingUpToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			smoothRandomCeilingUpToolStripMenuItem.Tag = "SmoothRandomCeilingUp";
			smoothRandomCeilingUpToolStripMenuItem.Text = "SmoothRandomCeilingUp";
			// 
			// smoothRandomCeilingDownToolStripMenuItem
			// 
			smoothRandomCeilingDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			smoothRandomCeilingDownToolStripMenuItem.Enabled = false;
			smoothRandomCeilingDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			smoothRandomCeilingDownToolStripMenuItem.Name = "smoothRandomCeilingDownToolStripMenuItem";
			smoothRandomCeilingDownToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			smoothRandomCeilingDownToolStripMenuItem.Tag = "SmoothRandomCeilingDown";
			smoothRandomCeilingDownToolStripMenuItem.Text = "SmoothRandomCeilingDown";
			// 
			// toolStripMenuSeparator11
			// 
			toolStripMenuSeparator11.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuSeparator11.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuSeparator11.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuSeparator11.Name = "toolStripMenuSeparator11";
			toolStripMenuSeparator11.Size = new System.Drawing.Size(226, 6);
			// 
			// sharpRandomFloorUpToolStripMenuItem
			// 
			sharpRandomFloorUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			sharpRandomFloorUpToolStripMenuItem.Enabled = false;
			sharpRandomFloorUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			sharpRandomFloorUpToolStripMenuItem.Name = "sharpRandomFloorUpToolStripMenuItem";
			sharpRandomFloorUpToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			sharpRandomFloorUpToolStripMenuItem.Tag = "SharpRandomFloorUp";
			sharpRandomFloorUpToolStripMenuItem.Text = "SharpRandomFloorUp";
			// 
			// sharpRandomFloorDownToolStripMenuItem
			// 
			sharpRandomFloorDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			sharpRandomFloorDownToolStripMenuItem.Enabled = false;
			sharpRandomFloorDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			sharpRandomFloorDownToolStripMenuItem.Name = "sharpRandomFloorDownToolStripMenuItem";
			sharpRandomFloorDownToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			sharpRandomFloorDownToolStripMenuItem.Tag = "SharpRandomFloorDown";
			sharpRandomFloorDownToolStripMenuItem.Text = "SharpRandomFloorDown";
			// 
			// sharpRandomCeilingUpToolStripMenuItem
			// 
			sharpRandomCeilingUpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			sharpRandomCeilingUpToolStripMenuItem.Enabled = false;
			sharpRandomCeilingUpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			sharpRandomCeilingUpToolStripMenuItem.Name = "sharpRandomCeilingUpToolStripMenuItem";
			sharpRandomCeilingUpToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			sharpRandomCeilingUpToolStripMenuItem.Tag = "SharpRandomCeilingUp";
			sharpRandomCeilingUpToolStripMenuItem.Text = "SharpRandomCeilingUp";
			// 
			// sharpRandomCeilingDownToolStripMenuItem
			// 
			sharpRandomCeilingDownToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			sharpRandomCeilingDownToolStripMenuItem.Enabled = false;
			sharpRandomCeilingDownToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			sharpRandomCeilingDownToolStripMenuItem.Name = "sharpRandomCeilingDownToolStripMenuItem";
			sharpRandomCeilingDownToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			sharpRandomCeilingDownToolStripMenuItem.Tag = "SharpRandomCeilingDown";
			sharpRandomCeilingDownToolStripMenuItem.Text = "SharpRandomCeilingDown";
			// 
			// toolStripMenuSeparator12
			// 
			toolStripMenuSeparator12.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuSeparator12.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuSeparator12.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuSeparator12.Name = "toolStripMenuSeparator12";
			toolStripMenuSeparator12.Size = new System.Drawing.Size(226, 6);
			// 
			// realignFloorToStepHeightToolStripMenuItem
			// 
			realignFloorToStepHeightToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			realignFloorToStepHeightToolStripMenuItem.Enabled = false;
			realignFloorToStepHeightToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			realignFloorToStepHeightToolStripMenuItem.Name = "realignFloorToStepHeightToolStripMenuItem";
			realignFloorToStepHeightToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			realignFloorToStepHeightToolStripMenuItem.Tag = "RealignFloorToStepHeight";
			realignFloorToStepHeightToolStripMenuItem.Text = "RealignFloorToStepHeight";
			// 
			// realignCeilingToStepHeightToolStripMenuItem
			// 
			realignCeilingToStepHeightToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			realignCeilingToStepHeightToolStripMenuItem.Enabled = false;
			realignCeilingToStepHeightToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			realignCeilingToStepHeightToolStripMenuItem.Name = "realignCeilingToStepHeightToolStripMenuItem";
			realignCeilingToStepHeightToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			realignCeilingToStepHeightToolStripMenuItem.Tag = "RealignCeilingToStepHeight";
			realignCeilingToStepHeightToolStripMenuItem.Text = "RealignCeilingToStepHeight";
			// 
			// smoothFloorToolStripMenuItem
			// 
			smoothFloorToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			smoothFloorToolStripMenuItem.Enabled = false;
			smoothFloorToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			smoothFloorToolStripMenuItem.Name = "smoothFloorToolStripMenuItem";
			smoothFloorToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			smoothFloorToolStripMenuItem.Tag = "SmoothFloor";
			smoothFloorToolStripMenuItem.Text = "SmoothFloor";
			// 
			// smoothCeilingToolStripMenuItem
			// 
			smoothCeilingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			smoothCeilingToolStripMenuItem.Enabled = false;
			smoothCeilingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			smoothCeilingToolStripMenuItem.Name = "smoothCeilingToolStripMenuItem";
			smoothCeilingToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			smoothCeilingToolStripMenuItem.Tag = "SmoothCeiling";
			smoothCeilingToolStripMenuItem.Text = "SmoothCeiling";
			// 
			// averageFloorToolStripMenuItem
			// 
			averageFloorToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			averageFloorToolStripMenuItem.Enabled = false;
			averageFloorToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			averageFloorToolStripMenuItem.Name = "averageFloorToolStripMenuItem";
			averageFloorToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			averageFloorToolStripMenuItem.Tag = "AverageFloor";
			averageFloorToolStripMenuItem.Text = "AverageFloor";
			// 
			// averageCeilingToolStripMenuItem
			// 
			averageCeilingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			averageCeilingToolStripMenuItem.Enabled = false;
			averageCeilingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			averageCeilingToolStripMenuItem.Name = "averageCeilingToolStripMenuItem";
			averageCeilingToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			averageCeilingToolStripMenuItem.Tag = "AverageCeiling";
			averageCeilingToolStripMenuItem.Text = "AverageCeiling";
			// 
			// flattenFloorToolStripMenuItem
			// 
			flattenFloorToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			flattenFloorToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			flattenFloorToolStripMenuItem.Name = "flattenFloorToolStripMenuItem";
			flattenFloorToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			flattenFloorToolStripMenuItem.Tag = "FlattenFloor";
			flattenFloorToolStripMenuItem.Text = "FlattenFloor";
			// 
			// flattenCeilingToolStripMenuItem
			// 
			flattenCeilingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			flattenCeilingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			flattenCeilingToolStripMenuItem.Name = "flattenCeilingToolStripMenuItem";
			flattenCeilingToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			flattenCeilingToolStripMenuItem.Tag = "FlattenCeiling";
			flattenCeilingToolStripMenuItem.Text = "FlattenCeiling";
			// 
			// resetGeometryToolStripMenuItem
			// 
			resetGeometryToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			resetGeometryToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			resetGeometryToolStripMenuItem.Name = "resetGeometryToolStripMenuItem";
			resetGeometryToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			resetGeometryToolStripMenuItem.Tag = "ResetGeometry";
			resetGeometryToolStripMenuItem.Text = "ResetGeometry";
			// 
			// toolStripMenuSeparator13
			// 
			toolStripMenuSeparator13.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuSeparator13.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuSeparator13.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuSeparator13.Name = "toolStripMenuSeparator13";
			toolStripMenuSeparator13.Size = new System.Drawing.Size(226, 6);
			// 
			// gridWallsIn3ToolStripMenuItem
			// 
			gridWallsIn3ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			gridWallsIn3ToolStripMenuItem.Enabled = false;
			gridWallsIn3ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			gridWallsIn3ToolStripMenuItem.Name = "gridWallsIn3ToolStripMenuItem";
			gridWallsIn3ToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			gridWallsIn3ToolStripMenuItem.Tag = "GridWallsIn3";
			gridWallsIn3ToolStripMenuItem.Text = "GridWallsIn3";
			// 
			// gridWallsIn5ToolStripMenuItem
			// 
			gridWallsIn5ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			gridWallsIn5ToolStripMenuItem.Enabled = false;
			gridWallsIn5ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			gridWallsIn5ToolStripMenuItem.Name = "gridWallsIn5ToolStripMenuItem";
			gridWallsIn5ToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			gridWallsIn5ToolStripMenuItem.Tag = "GridWallsIn5";
			gridWallsIn5ToolStripMenuItem.Text = "GridWallsIn5";
			// 
			// gridWallsIn3SquaresToolStripMenuItem
			// 
			gridWallsIn3SquaresToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			gridWallsIn3SquaresToolStripMenuItem.Enabled = false;
			gridWallsIn3SquaresToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			gridWallsIn3SquaresToolStripMenuItem.Name = "gridWallsIn3SquaresToolStripMenuItem";
			gridWallsIn3SquaresToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			gridWallsIn3SquaresToolStripMenuItem.Tag = "GridWallsIn3Squares";
			gridWallsIn3SquaresToolStripMenuItem.Text = "GridWallsIn3Squares";
			// 
			// gridWallsIn5SquaresToolStripMenuItem
			// 
			gridWallsIn5SquaresToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			gridWallsIn5SquaresToolStripMenuItem.Enabled = false;
			gridWallsIn5SquaresToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			gridWallsIn5SquaresToolStripMenuItem.Name = "gridWallsIn5SquaresToolStripMenuItem";
			gridWallsIn5SquaresToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			gridWallsIn5SquaresToolStripMenuItem.Tag = "GridWallsIn5Squares";
			gridWallsIn5SquaresToolStripMenuItem.Text = "GridWallsIn5Squares";
			// 
			// convertFloorToQuadsToolStripMenuItem
			// 
			convertFloorToQuadsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			convertFloorToQuadsToolStripMenuItem.Enabled = false;
			convertFloorToQuadsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			convertFloorToQuadsToolStripMenuItem.Name = "convertFloorToQuadsToolStripMenuItem";
			convertFloorToQuadsToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			convertFloorToQuadsToolStripMenuItem.Tag = "ConvertFloorToQuads";
			convertFloorToQuadsToolStripMenuItem.Text = "ConvertFloorToQuads";
			// 
			// toolStripMenuItem4
			// 
			toolStripMenuItem4.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItem4.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItem5, editOptionsToolStripMenuItem, keyboardLayoutToolStripMenuItem, toolStripSeparator5, toolStripMenuItem7, toolStripMenuItem6 });
			toolStripMenuItem4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem4.Name = "toolStripMenuItem4";
			toolStripMenuItem4.Size = new System.Drawing.Size(46, 25);
			toolStripMenuItem4.Text = "Tools";
			// 
			// toolStripMenuItem5
			// 
			toolStripMenuItem5.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItem5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem5.Image = Properties.Resources.general_settings_16;
			toolStripMenuItem5.Name = "toolStripMenuItem5";
			toolStripMenuItem5.Size = new System.Drawing.Size(180, 22);
			toolStripMenuItem5.Tag = "EditLevelSettings";
			toolStripMenuItem5.Text = "EditLevelSettings";
			// 
			// editOptionsToolStripMenuItem
			// 
			editOptionsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			editOptionsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			editOptionsToolStripMenuItem.Image = Properties.Resources.general_options_16;
			editOptionsToolStripMenuItem.Name = "editOptionsToolStripMenuItem";
			editOptionsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			editOptionsToolStripMenuItem.Tag = "EditOptions";
			editOptionsToolStripMenuItem.Text = "EditOptions";
			// 
			// keyboardLayoutToolStripMenuItem
			// 
			keyboardLayoutToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			keyboardLayoutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			keyboardLayoutToolStripMenuItem.Name = "keyboardLayoutToolStripMenuItem";
			keyboardLayoutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			keyboardLayoutToolStripMenuItem.Tag = "EditKeyboardLayout";
			keyboardLayoutToolStripMenuItem.Text = "EditKeyboardLayout";
			// 
			// toolStripSeparator5
			// 
			toolStripSeparator5.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripSeparator5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripSeparator5.Margin = new Padding(0, 0, 0, 1);
			toolStripSeparator5.Name = "toolStripSeparator5";
			toolStripSeparator5.Size = new System.Drawing.Size(177, 6);
			// 
			// toolStripMenuItem7
			// 
			toolStripMenuItem7.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItem7.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem7.Name = "toolStripMenuItem7";
			toolStripMenuItem7.Size = new System.Drawing.Size(180, 22);
			toolStripMenuItem7.Tag = "StartWadTool";
			toolStripMenuItem7.Text = "StartWadTool";
			// 
			// toolStripMenuItem6
			// 
			toolStripMenuItem6.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuItem6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuItem6.Name = "toolStripMenuItem6";
			toolStripMenuItem6.Size = new System.Drawing.Size(180, 22);
			toolStripMenuItem6.Tag = "StartSoundTool";
			toolStripMenuItem6.Text = "StartSoundTool";
			// 
			// butFindMenu
			// 
			butFindMenu.Alignment = ToolStripItemAlignment.Right;
			butFindMenu.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butFindMenu.DisplayStyle = ToolStripItemDisplayStyle.Image;
			butFindMenu.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butFindMenu.Image = Properties.Resources.general_search_16;
			butFindMenu.Margin = new Padding(1, 2, 1, 0);
			butFindMenu.Name = "butFindMenu";
			butFindMenu.Size = new System.Drawing.Size(28, 23);
			butFindMenu.ToolTipText = "Search for menu entry";
			butFindMenu.Click += butFindMenu_Click;
			// 
			// windowToolStripMenuItem
			// 
			windowToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			windowToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { restoreDefaultLayoutToolStripMenuItem, toolStripMenuSeparator14, sectorOptionsToolStripMenuItem, roomOptionsToolStripMenuItem, itemBrowserToolStripMenuItem, importedGeometryBrowserToolstripMenuItem, triggerListToolStripMenuItem, lightingToolStripMenuItem, paletteToolStripMenuItem, texturePanelToolStripMenuItem, objectListToolStripMenuItem, statisticsToolStripMenuItem, dockableToolStripMenuItem, floatingToolStripMenuItem });
			windowToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			windowToolStripMenuItem.Name = "windowToolStripMenuItem";
			windowToolStripMenuItem.Size = new System.Drawing.Size(63, 25);
			windowToolStripMenuItem.Text = "Window";
			// 
			// restoreDefaultLayoutToolStripMenuItem
			// 
			restoreDefaultLayoutToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			restoreDefaultLayoutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			restoreDefaultLayoutToolStripMenuItem.Name = "restoreDefaultLayoutToolStripMenuItem";
			restoreDefaultLayoutToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			restoreDefaultLayoutToolStripMenuItem.Text = "Restore default layout";
			restoreDefaultLayoutToolStripMenuItem.Click += restoreDefaultLayoutToolStripMenuItem_Click;
			// 
			// toolStripMenuSeparator14
			// 
			toolStripMenuSeparator14.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStripMenuSeparator14.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStripMenuSeparator14.Margin = new Padding(0, 0, 0, 1);
			toolStripMenuSeparator14.Name = "toolStripMenuSeparator14";
			toolStripMenuSeparator14.Size = new System.Drawing.Size(243, 6);
			// 
			// sectorOptionsToolStripMenuItem
			// 
			sectorOptionsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			sectorOptionsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			sectorOptionsToolStripMenuItem.Name = "sectorOptionsToolStripMenuItem";
			sectorOptionsToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			sectorOptionsToolStripMenuItem.Tag = "ShowSectorOptions";
			sectorOptionsToolStripMenuItem.Text = "ShowSectorOptions";
			// 
			// roomOptionsToolStripMenuItem
			// 
			roomOptionsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			roomOptionsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			roomOptionsToolStripMenuItem.Name = "roomOptionsToolStripMenuItem";
			roomOptionsToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			roomOptionsToolStripMenuItem.Tag = "ShowRoomOptions";
			roomOptionsToolStripMenuItem.Text = "ShowRoomOptions";
			// 
			// itemBrowserToolStripMenuItem
			// 
			itemBrowserToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			itemBrowserToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			itemBrowserToolStripMenuItem.Name = "itemBrowserToolStripMenuItem";
			itemBrowserToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			itemBrowserToolStripMenuItem.Tag = "ShowItemBrowser";
			itemBrowserToolStripMenuItem.Text = "ShowItemBrowser";
			// 
			// importedGeometryBrowserToolstripMenuItem
			// 
			importedGeometryBrowserToolstripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			importedGeometryBrowserToolstripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			importedGeometryBrowserToolstripMenuItem.Name = "importedGeometryBrowserToolstripMenuItem";
			importedGeometryBrowserToolstripMenuItem.Size = new System.Drawing.Size(246, 22);
			importedGeometryBrowserToolstripMenuItem.Tag = "ShowImportedGeometryBrowser";
			importedGeometryBrowserToolstripMenuItem.Text = "ShowImportedGeometryBrowser";
			// 
			// triggerListToolStripMenuItem
			// 
			triggerListToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			triggerListToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			triggerListToolStripMenuItem.Name = "triggerListToolStripMenuItem";
			triggerListToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			triggerListToolStripMenuItem.Tag = "ShowTriggerList";
			triggerListToolStripMenuItem.Text = "ShowTriggerList";
			// 
			// lightingToolStripMenuItem
			// 
			lightingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			lightingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lightingToolStripMenuItem.Name = "lightingToolStripMenuItem";
			lightingToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			lightingToolStripMenuItem.Tag = "ShowLighting";
			lightingToolStripMenuItem.Text = "ShowLighting";
			// 
			// paletteToolStripMenuItem
			// 
			paletteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			paletteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			paletteToolStripMenuItem.Name = "paletteToolStripMenuItem";
			paletteToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			paletteToolStripMenuItem.Tag = "ShowPalette";
			paletteToolStripMenuItem.Text = "ShowPalette";
			// 
			// texturePanelToolStripMenuItem
			// 
			texturePanelToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			texturePanelToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			texturePanelToolStripMenuItem.Name = "texturePanelToolStripMenuItem";
			texturePanelToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			texturePanelToolStripMenuItem.Tag = "ShowTexturePanel";
			texturePanelToolStripMenuItem.Text = "ShowTexturePanel";
			// 
			// objectListToolStripMenuItem
			// 
			objectListToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			objectListToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			objectListToolStripMenuItem.Name = "objectListToolStripMenuItem";
			objectListToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			objectListToolStripMenuItem.Tag = "ShowObjectList";
			objectListToolStripMenuItem.Text = "ShowObjectList";
			// 
			// statisticsToolStripMenuItem
			// 
			statisticsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			statisticsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			statisticsToolStripMenuItem.Name = "statisticsToolStripMenuItem";
			statisticsToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			statisticsToolStripMenuItem.Tag = "ShowStatistics";
			statisticsToolStripMenuItem.Text = "ShowStatistics";
			// 
			// dockableToolStripMenuItem
			// 
			dockableToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			dockableToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			dockableToolStripMenuItem.Name = "dockableToolStripMenuItem";
			dockableToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			dockableToolStripMenuItem.Tag = "ShowToolPalette";
			dockableToolStripMenuItem.Text = "ShowToolPalette";
			// 
			// floatingToolStripMenuItem
			// 
			floatingToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			floatingToolStripMenuItem.CheckOnClick = true;
			floatingToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			floatingToolStripMenuItem.Name = "floatingToolStripMenuItem";
			floatingToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			floatingToolStripMenuItem.Text = "Tool Palette (floating)";
			floatingToolStripMenuItem.CheckedChanged += floatingToolStripMenuItem_CheckedChanged;
			// 
			// helpToolStripMenuItem
			// 
			helpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
			helpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			helpToolStripMenuItem.Size = new System.Drawing.Size(44, 25);
			helpToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem
			// 
			aboutToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			aboutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			aboutToolStripMenuItem.Image = Properties.Resources.general_AboutIcon_16;
			aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			aboutToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
			aboutToolStripMenuItem.Text = "About Tomb Editor...";
			aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
			// 
			// tbSearchMenu
			// 
			tbSearchMenu.Alignment = ToolStripItemAlignment.Right;
			tbSearchMenu.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tbSearchMenu.BorderStyle = BorderStyle.FixedSingle;
			tbSearchMenu.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			tbSearchMenu.Margin = new Padding(0, 2, 0, 0);
			tbSearchMenu.Name = "tbSearchMenu";
			tbSearchMenu.Size = new System.Drawing.Size(200, 23);
			tbSearchMenu.ToolTipText = "Search for menu entry";
			tbSearchMenu.KeyDown += tbSearchMenu_KeyDown;
			// 
			// debugToolStripMenuItem
			// 
			debugToolStripMenuItem.Alignment = ToolStripItemAlignment.Right;
			debugToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			debugToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { debugAction0ToolStripMenuItem, debugAction1ToolStripMenuItem, debugAction2ToolStripMenuItem, debugAction3ToolStripMenuItem, debugAction4ToolStripMenuItem, debugAction5ToolStripMenuItem, debugScriptToolStripMenuItem });
			debugToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugToolStripMenuItem.Name = "debugToolStripMenuItem";
			debugToolStripMenuItem.Size = new System.Drawing.Size(54, 25);
			debugToolStripMenuItem.Text = "Debug";
			debugToolStripMenuItem.Visible = false;
			// 
			// debugAction0ToolStripMenuItem
			// 
			debugAction0ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction0ToolStripMenuItem.Name = "debugAction0ToolStripMenuItem";
			debugAction0ToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
			debugAction0ToolStripMenuItem.Text = "Debug Action 0";
			debugAction0ToolStripMenuItem.Click += debugAction0ToolStripMenuItem_Click;
			// 
			// debugAction1ToolStripMenuItem
			// 
			debugAction1ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction1ToolStripMenuItem.Name = "debugAction1ToolStripMenuItem";
			debugAction1ToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
			debugAction1ToolStripMenuItem.Text = "Debug Action 1";
			debugAction1ToolStripMenuItem.Click += debugAction1ToolStripMenuItem_Click;
			// 
			// debugAction2ToolStripMenuItem
			// 
			debugAction2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction2ToolStripMenuItem.Name = "debugAction2ToolStripMenuItem";
			debugAction2ToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
			debugAction2ToolStripMenuItem.Text = "Debug Action 2";
			debugAction2ToolStripMenuItem.Click += debugAction2ToolStripMenuItem_Click;
			// 
			// debugAction3ToolStripMenuItem
			// 
			debugAction3ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction3ToolStripMenuItem.Name = "debugAction3ToolStripMenuItem";
			debugAction3ToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
			debugAction3ToolStripMenuItem.Text = "Debug Action 3";
			debugAction3ToolStripMenuItem.Click += debugAction3ToolStripMenuItem_Click;
			// 
			// debugAction4ToolStripMenuItem
			// 
			debugAction4ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction4ToolStripMenuItem.Name = "debugAction4ToolStripMenuItem";
			debugAction4ToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
			debugAction4ToolStripMenuItem.Text = "Debug Action 4";
			debugAction4ToolStripMenuItem.Click += debugAction4ToolStripMenuItem_Click;
			// 
			// debugAction5ToolStripMenuItem
			// 
			debugAction5ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugAction5ToolStripMenuItem.Name = "debugAction5ToolStripMenuItem";
			debugAction5ToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
			debugAction5ToolStripMenuItem.Text = "Debug Action 5";
			debugAction5ToolStripMenuItem.Click += debugAction5ToolStripMenuItem_Click;
			// 
			// debugScriptToolStripMenuItem
			// 
			debugScriptToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			debugScriptToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			debugScriptToolStripMenuItem.Name = "debugScriptToolStripMenuItem";
			debugScriptToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
			debugScriptToolStripMenuItem.Text = "Debug script";
			debugScriptToolStripMenuItem.Click += debugScriptToolStripMenuItem_Click;
			// 
			// butRoomDown
			// 
			butRoomDown.Checked = false;
			butRoomDown.Location = new System.Drawing.Point(0, 0);
			butRoomDown.Name = "butRoomDown";
			butRoomDown.Size = new System.Drawing.Size(75, 23);
			butRoomDown.TabIndex = 0;
			// 
			// butEditRoomName
			// 
			butEditRoomName.Checked = false;
			butEditRoomName.Location = new System.Drawing.Point(0, 0);
			butEditRoomName.Name = "butEditRoomName";
			butEditRoomName.Size = new System.Drawing.Size(75, 23);
			butEditRoomName.TabIndex = 0;
			// 
			// butDeleteRoom
			// 
			butDeleteRoom.Checked = false;
			butDeleteRoom.Location = new System.Drawing.Point(0, 0);
			butDeleteRoom.Name = "butDeleteRoom";
			butDeleteRoom.Size = new System.Drawing.Size(75, 23);
			butDeleteRoom.TabIndex = 0;
			// 
			// statusStrip
			// 
			statusStrip.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			statusStrip.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			statusStrip.Items.AddRange(new ToolStripItem[] { statusStripSelectedRoom, statusStripGlobalSelectionArea, statusStripLocalSelectionArea, statusAutosave });
			statusStrip.Location = new System.Drawing.Point(0, 440);
			statusStrip.Name = "statusStrip";
			statusStrip.Padding = new Padding(0, 5, 0, 3);
			statusStrip.Size = new System.Drawing.Size(913, 29);
			statusStrip.TabIndex = 29;
			statusStrip.Text = "statusStrip";
			// 
			// statusStripSelectedRoom
			// 
			statusStripSelectedRoom.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			statusStripSelectedRoom.BorderStyle = Border3DStyle.RaisedOuter;
			statusStripSelectedRoom.ForeColor = System.Drawing.Color.Silver;
			statusStripSelectedRoom.Margin = new Padding(4, 0, 4, 0);
			statusStripSelectedRoom.Name = "statusStripSelectedRoom";
			statusStripSelectedRoom.Size = new System.Drawing.Size(0, 21);
			statusStripSelectedRoom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// statusStripGlobalSelectionArea
			// 
			statusStripGlobalSelectionArea.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			statusStripGlobalSelectionArea.BorderStyle = Border3DStyle.RaisedOuter;
			statusStripGlobalSelectionArea.ForeColor = System.Drawing.Color.Silver;
			statusStripGlobalSelectionArea.Margin = new Padding(4, 0, 4, 0);
			statusStripGlobalSelectionArea.Name = "statusStripGlobalSelectionArea";
			statusStripGlobalSelectionArea.Size = new System.Drawing.Size(0, 21);
			statusStripGlobalSelectionArea.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// statusStripLocalSelectionArea
			// 
			statusStripLocalSelectionArea.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			statusStripLocalSelectionArea.BorderStyle = Border3DStyle.RaisedOuter;
			statusStripLocalSelectionArea.ForeColor = System.Drawing.Color.Silver;
			statusStripLocalSelectionArea.Margin = new Padding(4, 0, 4, 0);
			statusStripLocalSelectionArea.Name = "statusStripLocalSelectionArea";
			statusStripLocalSelectionArea.Size = new System.Drawing.Size(0, 21);
			statusStripLocalSelectionArea.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// statusAutosave
			// 
			statusAutosave.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			statusAutosave.ForeColor = System.Drawing.Color.Silver;
			statusAutosave.Margin = new Padding(4, 0, 4, 0);
			statusAutosave.Name = "statusAutosave";
			statusAutosave.Size = new System.Drawing.Size(0, 21);
			// 
			// dockArea
			// 
			dockArea.Dock = DockStyle.Fill;
			dockArea.Location = new System.Drawing.Point(0, 0);
			dockArea.MinimumSize = new System.Drawing.Size(274, 274);
			dockArea.Name = "dockArea";
			dockArea.Padding = new Padding(2);
			dockArea.Size = new System.Drawing.Size(913, 411);
			dockArea.TabIndex = 90;
			dockArea.ContentAdded += ToolWindow_Added;
			dockArea.ContentRemoved += ToolWindow_Removed;
			// 
			// panelDockArea
			// 
			panelDockArea.Controls.Add(dockArea);
			panelDockArea.Dock = DockStyle.Fill;
			panelDockArea.Location = new System.Drawing.Point(0, 29);
			panelDockArea.Name = "panelDockArea";
			panelDockArea.Size = new System.Drawing.Size(913, 411);
			panelDockArea.TabIndex = 26;
			// 
			// assToolStripMenuItem
			// 
			assToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			assToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			assToolStripMenuItem.Name = "assToolStripMenuItem";
			assToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			assToolStripMenuItem.Text = "ass";
			// 
			// convertCeilingToQuadsToolStripMenuItem
			// 
			convertCeilingToQuadsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			convertCeilingToQuadsToolStripMenuItem.Enabled = false;
			convertCeilingToQuadsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(153, 153, 153);
			convertCeilingToQuadsToolStripMenuItem.Name = "convertCeilingToQuadsToolStripMenuItem";
			convertCeilingToQuadsToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
			convertCeilingToQuadsToolStripMenuItem.Tag = "ConvertCeilingToQuads";
			convertCeilingToQuadsToolStripMenuItem.Text = "ConvertCeilingToQuads";
			// 
			// FormMain
			// 
			AllowDrop = true;
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(913, 469);
			Controls.Add(panelDockArea);
			Controls.Add(statusStrip);
			Controls.Add(menuStrip);
			KeyPreview = true;
			MainMenuStrip = menuStrip;
			Name = "FormMain";
			StartPosition = FormStartPosition.Manual;
			Text = "Tomb Editor";
			menuStrip.ResumeLayout(false);
			menuStrip.PerformLayout();
			statusStrip.ResumeLayout(false);
			statusStrip.PerformLayout();
			panelDockArea.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private DarkUI.Controls.DarkMenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem roomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem texturesToolStripMenuItem;
        private DarkUI.Controls.DarkButton butDeleteRoom;
        private DarkUI.Controls.DarkButton butEditRoomName;
        private DarkUI.Controls.DarkButton butRoomDown;
        private DarkUI.Controls.DarkStatusStrip statusStrip;
        private System.Windows.Forms.ToolStripMenuItem newLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem importTRLEPRJToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator2;
        private System.Windows.Forms.ToolStripMenuItem buildLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem buildLevelPlayToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator3;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smoothRandomFloorUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smoothRandomFloorDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem averageFloorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem averageCeilingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addFlybyCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSinkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSoundSourceToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator8;
        private System.Windows.Forms.ToolStripMenuItem findObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator9;
        private System.Windows.Forms.ToolStripMenuItem textureFloorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textureCeilingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textureWallsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator10;
        private System.Windows.Forms.ToolStripMenuItem animationRangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importConvertTexturesToPng;
        private System.Windows.Forms.ToolStripMenuItem addWadToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator6;
        private System.Windows.Forms.ToolStripStatusLabel statusStripSelectedRoom;
        private System.Windows.Forms.ToolStripMenuItem smoothRandomCeilingUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smoothRandomCeilingDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator12;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator5;
        private System.Windows.Forms.ToolStripMenuItem duplicateRoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem splitRoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cropRoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction0ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction4ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator13;
        private System.Windows.Forms.ToolStripMenuItem gridWallsIn3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gridWallsIn5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator11;
        private System.Windows.Forms.ToolStripMenuItem sharpRandomFloorUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sharpRandomFloorDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sharpRandomCeilingUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sharpRandomCeilingDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel statusStripGlobalSelectionArea;
        private System.Windows.Forms.ToolStripStatusLabel statusStripLocalSelectionArea;
        private System.Windows.Forms.ToolStripMenuItem removeTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeWadsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadWadsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveLaraToolStripMenuItem;
        private DarkUI.Docking.DarkDockPanel dockArea;
        private System.Windows.Forms.Panel panelDockArea;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreDefaultLayoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuSeparator14;
        private System.Windows.Forms.ToolStripMenuItem sectorOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem roomOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemBrowserToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem triggerListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lightingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem paletteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem texturePanelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addGhostBlockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addPortalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addTriggerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem transformToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exportRoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel statusAutosave;
        private System.Windows.Forms.ToolStripMenuItem unloadTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem remapTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stampToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem bookmarkObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bookmarkRestoreObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem editObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem keyboardLayoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeRoomsHorizontallyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem splitSectorObjectOnSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearAllTexturesInRoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openRecentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flattenFloorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flattenCeilingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wholeRoomUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wholeRoomDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomLeftToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomRightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomForwardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomBackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomLeftToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomRightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomFrontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRoomBackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem clearAllTexturesInRoomToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem gridWallsIn3SquaresToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gridWallsIn5SquaresToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetGeometryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomUp4ClicksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveRoomDown4ClicksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleFlyModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem relocateCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem transformRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotateRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotateRoomsCountercockwiseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mirrorRoomsOnXAxisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mirrorRoomsOnZAxisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectWaterRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectSkyRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectOutsideRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem selectConnectedRoomsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectRoomsByTagsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem setStaticMeshColorToRoomLightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getObjectStatisticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem reloadSoundsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem makeQuickItemGroupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addImportedGeometryToolStripMenuItem;
        private ToolStripMenuItem searchAndReplaceToolStripMenuItem;
        private ToolStripMenuItem findTexturesToolStripMenuItem;
        private ToolStripMenuItem addSphereVolumeToolStripMenuItem;
        private ToolStripMenuItem addBoxVolumeToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem8;
        private ToolStripMenuItem assToolStripMenuItem;
        private ToolStripMenuItem drawWhiteTextureLightingOnlyToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem ShowRealTintForObjectsToolStripMenuItem;
        private ToolStripMenuItem importedGeometryBrowserToolstripMenuItem;
        private ToolStripMenuItem addSpriteToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem9;
        private ToolStripMenuItem addMemoToolStripMenuItem;
        private ToolStripMenuItem statisticsToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator9;
		private ToolStripMenuItem toolStripMenuItem10;
		private ToolStripMenuItem deleteAllToolStripMenuItem;
		private ToolStripMenuItem deleteAllLightsToolStripMenuItem;
		private ToolStripMenuItem toolStripMenuItem2;
		private ToolStripMenuItem deleteAllTriggersToolStripMenuItem;
        private ToolStripMenuItem selectFloorBelowObjectToolStripMenuItem;
        private ToolStripMenuItem deleteMissingObjectsToolStripMenuItem;
        private ToolStripMenuItem generateObjectNamesToolStripMenuItem;
        private ToolStripMenuItem selectItemsInSelectedAreaToolStripMenuItem;
        private ToolStripMenuItem convertToTENToolstripMenuItem;
        private ToolStripMenuItem butFindMenu;
        private ToolStripTextBox tbSearchMenu;
        private ToolStripMenuItem dockableToolStripMenuItem;
        private ToolStripMenuItem floatingToolStripMenuItem;
        private ToolStripMenuItem editEventSetsToolStripMenuItem;
        private ToolStripMenuItem editGlobalEventSetsToolStripMenuItem;
		private ToolStripMenuItem increaseStepHeightToolStripMenuItem;
		private ToolStripMenuItem decreaseStepHeightToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator10;
		private ToolStripMenuItem smoothCeilingToolStripMenuItem;
		private ToolStripMenuItem smoothFloorToolStripMenuItem;
		private ToolStripMenuItem realignFloorToStepHeightToolStripMenuItem;
		private ToolStripMenuItem realignCeilingToStepHeightToolStripMenuItem;
		private ToolStripMenuItem convertFloorToQuadsToolStripMenuItem;
		private ToolStripMenuItem convertCeilingToQuadsToolStripMenuItem;
	}
}