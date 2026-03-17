using TombEditor.Controls.Panel3D;

namespace TombEditor.ToolWindows
{
    partial class MainView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			toolStrip = new DarkUI.Controls.DarkToolStrip();
			panel2DMap = new TombEditor.Controls.Panel2DMap();
			panel3D = new TombEditor.Controls.Panel3D.Panel3D();
			but2D = new System.Windows.Forms.ToolStripButton();
			but3D = new System.Windows.Forms.ToolStripButton();
			butObjectPlacement = new System.Windows.Forms.ToolStripButton();
			butFaceEdit = new System.Windows.Forms.ToolStripButton();
			butLightingMode = new System.Windows.Forms.ToolStripButton();
			butUndo = new System.Windows.Forms.ToolStripButton();
			butRedo = new System.Windows.Forms.ToolStripButton();
			butCenterCamera = new System.Windows.Forms.ToolStripButton();
			butDrawPortals = new System.Windows.Forms.ToolStripButton();
			butDrawAllRooms = new System.Windows.Forms.ToolStripButton();
			butDrawHorizon = new System.Windows.Forms.ToolStripButton();
			butDrawRoomNames = new System.Windows.Forms.ToolStripButton();
			butDrawCardinalDirections = new System.Windows.Forms.ToolStripButton();
			butDrawExtraBlendingModes = new System.Windows.Forms.ToolStripButton();
			butHideTransparentFaces = new System.Windows.Forms.ToolStripButton();
			butBilinearFilter = new System.Windows.Forms.ToolStripButton();
			butDrawWhiteLighting = new System.Windows.Forms.ToolStripButton();
			butDrawStaticTint = new System.Windows.Forms.ToolStripButton();
			butDrawIllegalSlopes = new System.Windows.Forms.ToolStripButton();
			butDrawSlideDirections = new System.Windows.Forms.ToolStripButton();
			butDisableGeometryPicking = new System.Windows.Forms.ToolStripButton();
			butDisableHiddenRoomPicking = new System.Windows.Forms.ToolStripButton();
			butDrawObjects = new System.Windows.Forms.ToolStripDropDownButton();
			butDrawMoveables = new System.Windows.Forms.ToolStripMenuItem();
			butDrawStatics = new System.Windows.Forms.ToolStripMenuItem();
			butDrawImportedGeometry = new System.Windows.Forms.ToolStripMenuItem();
			butDrawGhostBlocks = new System.Windows.Forms.ToolStripMenuItem();
			butDrawVolumes = new System.Windows.Forms.ToolStripMenuItem();
			butDrawBoundingBoxes = new System.Windows.Forms.ToolStripMenuItem();
			butDrawOther = new System.Windows.Forms.ToolStripMenuItem();
			butDrawLightRadius = new System.Windows.Forms.ToolStripMenuItem();
			butFlipMap = new System.Windows.Forms.ToolStripButton();
			butCopy = new System.Windows.Forms.ToolStripButton();
			butPaste = new System.Windows.Forms.ToolStripButton();
			butStamp = new System.Windows.Forms.ToolStripButton();
			butOpacityNone = new System.Windows.Forms.ToolStripButton();
			butOpacitySolidFaces = new System.Windows.Forms.ToolStripButton();
			butOpacityTraversableFaces = new System.Windows.Forms.ToolStripButton();
			butAddCamera = new System.Windows.Forms.ToolStripButton();
			butAddSprite = new System.Windows.Forms.ToolStripButton();
			butAddFlybyCamera = new System.Windows.Forms.ToolStripButton();
			butAddSink = new System.Windows.Forms.ToolStripButton();
			butAddSoundSource = new System.Windows.Forms.ToolStripButton();
			butAddImportedGeometry = new System.Windows.Forms.ToolStripButton();
			butAddGhostBlock = new System.Windows.Forms.ToolStripButton();
			butAddMemo = new System.Windows.Forms.ToolStripButton();
			butCompileLevel = new System.Windows.Forms.ToolStripButton();
			butCompileLevelAndPlay = new System.Windows.Forms.ToolStripButton();
			butCompileAndPlayPreview = new System.Windows.Forms.ToolStripButton();
			butAddBoxVolume = new System.Windows.Forms.ToolStripButton();
			butAddSphereVolume = new System.Windows.Forms.ToolStripButton();
			butTextureFloor = new System.Windows.Forms.ToolStripButton();
			butTextureCeiling = new System.Windows.Forms.ToolStripButton();
			butTextureWalls = new System.Windows.Forms.ToolStripButton();
			butEditLevelSettings = new System.Windows.Forms.ToolStripButton();
			butToggleFlyMode = new System.Windows.Forms.ToolStripButton();
			butSearch = new System.Windows.Forms.ToolStripButton();
			butSearchAndReplaceObjects = new System.Windows.Forms.ToolStripButton();
			panelStats = new System.Windows.Forms.Panel();
			tbStats = new Controls.RichTextLabel();
			lblStepHeight = new DarkUI.Controls.DarkLabel();
			panelStepHeightOptions = new System.Windows.Forms.Panel();
			comboStepHeight = new DarkUI.Controls.DarkComboBox();
			panelMainView = new System.Windows.Forms.Panel();
			toolTip = new System.Windows.Forms.ToolTip(components);
			butMirror = new System.Windows.Forms.ToolStripButton();
			toolStrip.SuspendLayout();
			panelStats.SuspendLayout();
			panelStepHeightOptions.SuspendLayout();
			SuspendLayout();
			// 
			// toolStrip
			// 
			toolStrip.AutoSize = false;
			toolStrip.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			toolStrip.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { but2D, but3D, butFaceEdit, butObjectPlacement, butLightingMode, butUndo, butRedo, butCenterCamera, butDrawPortals, butDrawAllRooms, butDrawHorizon, butDrawRoomNames, butDrawCardinalDirections, butDrawExtraBlendingModes, butHideTransparentFaces, butBilinearFilter, butDrawWhiteLighting, butDrawStaticTint, butDrawIllegalSlopes, butDrawSlideDirections, butDisableGeometryPicking, butDisableHiddenRoomPicking, butDrawObjects, butFlipMap, butCopy, butPaste, butStamp, butOpacityNone, butOpacitySolidFaces, butOpacityTraversableFaces, butMirror, butAddCamera, butAddSprite, butAddFlybyCamera, butAddSink, butAddSoundSource, butAddImportedGeometry, butAddGhostBlock, butAddMemo, butCompileLevel, butCompileLevelAndPlay, butCompileAndPlayPreview, butAddBoxVolume, butAddSphereVolume, butTextureFloor, butTextureCeiling, butTextureWalls, butEditLevelSettings, butToggleFlyMode, butSearch, butSearchAndReplaceObjects });
			toolStrip.Location = new System.Drawing.Point(0, 0);
			toolStrip.Name = "toolStrip";
			toolStrip.Padding = new System.Windows.Forms.Padding(6, 0, 1, 0);
			toolStrip.Size = new System.Drawing.Size(1505, 32);
			toolStrip.TabIndex = 12;
			toolStrip.Text = "darkToolStrip1";
			toolStrip.MouseClick += toolStrip_MouseClick;
			// 
			// but2D
			// 
			but2D.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			but2D.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			but2D.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			but2D.Image = Properties.Resources.actions_2DView_16;
			but2D.ImageTransparentColor = System.Drawing.Color.Magenta;
			but2D.Name = "but2D";
			but2D.Size = new System.Drawing.Size(23, 29);
			but2D.Tag = "Switch2DMode";
			// 
			// but3D
			// 
			but3D.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			but3D.Checked = true;
			but3D.CheckState = System.Windows.Forms.CheckState.Checked;
			but3D.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			but3D.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			but3D.Image = Properties.Resources.actions_3DView_16;
			but3D.ImageTransparentColor = System.Drawing.Color.Magenta;
			but3D.Name = "but3D";
			but3D.Size = new System.Drawing.Size(23, 29);
			but3D.Tag = "SwitchGeometryMode";
			// 
			// butObjectPlacement
			// 
			butObjectPlacement.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butObjectPlacement.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butObjectPlacement.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butObjectPlacement.Image = Properties.Resources.toolbox_ObjectPlacement_16;
			butObjectPlacement.ImageTransparentColor = System.Drawing.Color.Magenta;
			butObjectPlacement.Name = "butObjectPlacement";
			butObjectPlacement.Size = new System.Drawing.Size(23, 29);
			butObjectPlacement.Tag = "SwitchObjectPlacementMode";
			// 
			// butFaceEdit
			// 
			butFaceEdit.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butFaceEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butFaceEdit.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butFaceEdit.Image = Properties.Resources.actions_TextureMode_16;
			butFaceEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
			butFaceEdit.Name = "butFaceEdit";
			butFaceEdit.Size = new System.Drawing.Size(23, 29);
			butFaceEdit.Tag = "SwitchFaceEditMode";
			// 
			// butLightingMode
			// 
			butLightingMode.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butLightingMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butLightingMode.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butLightingMode.Image = Properties.Resources.actions_light_on_16;
			butLightingMode.ImageTransparentColor = System.Drawing.Color.Magenta;
			butLightingMode.Name = "butLightingMode";
			butLightingMode.Size = new System.Drawing.Size(23, 29);
			butLightingMode.Tag = "SwitchLightingMode";
			// 
			// butUndo
			// 
			butUndo.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butUndo.Enabled = false;
			butUndo.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butUndo.Image = Properties.Resources.general_undo_16;
			butUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
			butUndo.Name = "butUndo";
			butUndo.Size = new System.Drawing.Size(23, 29);
			butUndo.Tag = "Undo";
			// 
			// butRedo
			// 
			butRedo.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butRedo.Enabled = false;
			butRedo.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butRedo.Image = Properties.Resources.general_redo_16;
			butRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
			butRedo.Name = "butRedo";
			butRedo.Size = new System.Drawing.Size(23, 29);
			butRedo.Tag = "Redo";
			// 
			// butCenterCamera
			// 
			butCenterCamera.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butCenterCamera.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butCenterCamera.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butCenterCamera.Image = Properties.Resources.actions_center_direction_16;
			butCenterCamera.ImageTransparentColor = System.Drawing.Color.Magenta;
			butCenterCamera.Name = "butCenterCamera";
			butCenterCamera.Size = new System.Drawing.Size(23, 29);
			butCenterCamera.Tag = "ResetCamera";
			// 
			// butDrawPortals
			// 
			butDrawPortals.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawPortals.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butDrawPortals.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawPortals.Image = Properties.Resources.actions_DrawPortals_16;
			butDrawPortals.ImageTransparentColor = System.Drawing.Color.Magenta;
			butDrawPortals.Name = "butDrawPortals";
			butDrawPortals.Size = new System.Drawing.Size(23, 29);
			butDrawPortals.Tag = "DrawPortals";
			// 
			// butDrawAllRooms
			// 
			butDrawAllRooms.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawAllRooms.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butDrawAllRooms.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawAllRooms.Image = Properties.Resources.actions_DrawAllRooms_16;
			butDrawAllRooms.ImageTransparentColor = System.Drawing.Color.Magenta;
			butDrawAllRooms.Name = "butDrawAllRooms";
			butDrawAllRooms.Size = new System.Drawing.Size(23, 29);
			butDrawAllRooms.Tag = "DrawAllRooms";
			// 
			// butDrawHorizon
			// 
			butDrawHorizon.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawHorizon.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butDrawHorizon.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawHorizon.Image = Properties.Resources.actions_horizon_16;
			butDrawHorizon.ImageTransparentColor = System.Drawing.Color.Magenta;
			butDrawHorizon.Name = "butDrawHorizon";
			butDrawHorizon.Size = new System.Drawing.Size(23, 29);
			butDrawHorizon.Tag = "DrawHorizon";
			// 
			// butDrawRoomNames
			// 
			butDrawRoomNames.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawRoomNames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butDrawRoomNames.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawRoomNames.Image = Properties.Resources.actions_generic_text_16;
			butDrawRoomNames.ImageTransparentColor = System.Drawing.Color.Magenta;
			butDrawRoomNames.Name = "butDrawRoomNames";
			butDrawRoomNames.Size = new System.Drawing.Size(23, 29);
			butDrawRoomNames.Tag = "DrawRoomNames";
			// 
			// butDrawCardinalDirections
			// 
			butDrawCardinalDirections.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawCardinalDirections.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butDrawCardinalDirections.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawCardinalDirections.Image = Properties.Resources.actions_DrawCardinalDirections_16;
			butDrawCardinalDirections.ImageTransparentColor = System.Drawing.Color.Magenta;
			butDrawCardinalDirections.Name = "butDrawCardinalDirections";
			butDrawCardinalDirections.Size = new System.Drawing.Size(23, 29);
			butDrawCardinalDirections.Tag = "DrawCardinalDirections";
			// 
			// butDrawExtraBlendingModes
			// 
			butDrawExtraBlendingModes.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawExtraBlendingModes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butDrawExtraBlendingModes.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawExtraBlendingModes.Image = Properties.Resources.texture_Transparent_1_16;
			butDrawExtraBlendingModes.ImageTransparentColor = System.Drawing.Color.Magenta;
			butDrawExtraBlendingModes.Name = "butDrawExtraBlendingModes";
			butDrawExtraBlendingModes.Size = new System.Drawing.Size(23, 29);
			butDrawExtraBlendingModes.Tag = "DrawExtraBlendingModes";
			// 
			// butHideTransparentFaces
			// 
			butHideTransparentFaces.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butHideTransparentFaces.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butHideTransparentFaces.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butHideTransparentFaces.Image = Properties.Resources.actions_AlphaTest_16;
			butHideTransparentFaces.ImageTransparentColor = System.Drawing.Color.Magenta;
			butHideTransparentFaces.Name = "butHideTransparentFaces";
			butHideTransparentFaces.Size = new System.Drawing.Size(23, 29);
			butHideTransparentFaces.Tag = "HideTransparentFaces";
			// 
			// butBilinearFilter
			// 
			butBilinearFilter.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butBilinearFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butBilinearFilter.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butBilinearFilter.Image = Properties.Resources.general_blur_16;
			butBilinearFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
			butBilinearFilter.Name = "butBilinearFilter";
			butBilinearFilter.Size = new System.Drawing.Size(23, 29);
			butBilinearFilter.Tag = "BilinearFilter";
			// 
			// butDrawWhiteLighting
			// 
			butDrawWhiteLighting.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawWhiteLighting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butDrawWhiteLighting.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawWhiteLighting.Image = Properties.Resources.actions_DrawUntexturedLights_16;
			butDrawWhiteLighting.ImageTransparentColor = System.Drawing.Color.Magenta;
			butDrawWhiteLighting.Name = "butDrawWhiteLighting";
			butDrawWhiteLighting.Size = new System.Drawing.Size(23, 29);
			butDrawWhiteLighting.Tag = "DrawWhiteTextureLightingOnly";
			// 
			// butDrawStaticTint
			// 
			butDrawStaticTint.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawStaticTint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butDrawStaticTint.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawStaticTint.Image = Properties.Resources.actions_StaticTint_16;
			butDrawStaticTint.ImageTransparentColor = System.Drawing.Color.Magenta;
			butDrawStaticTint.Name = "butDrawStaticTint";
			butDrawStaticTint.Size = new System.Drawing.Size(23, 29);
			butDrawStaticTint.Tag = "ShowRealTintForObjects";
			// 
			// butDrawIllegalSlopes
			// 
			butDrawIllegalSlopes.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawIllegalSlopes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butDrawIllegalSlopes.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawIllegalSlopes.Image = Properties.Resources.general_Warning_16;
			butDrawIllegalSlopes.ImageTransparentColor = System.Drawing.Color.Magenta;
			butDrawIllegalSlopes.Name = "butDrawIllegalSlopes";
			butDrawIllegalSlopes.Size = new System.Drawing.Size(23, 29);
			butDrawIllegalSlopes.Tag = "DrawIllegalSlopes";
			// 
			// butDrawSlideDirections
			// 
			butDrawSlideDirections.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawSlideDirections.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butDrawSlideDirections.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawSlideDirections.Image = Properties.Resources.actions_Slide_16;
			butDrawSlideDirections.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			butDrawSlideDirections.ImageTransparentColor = System.Drawing.Color.Magenta;
			butDrawSlideDirections.Name = "butDrawSlideDirections";
			butDrawSlideDirections.Size = new System.Drawing.Size(23, 29);
			butDrawSlideDirections.Tag = "DrawSlideDirections";
			// 
			// butDisableGeometryPicking
			// 
			butDisableGeometryPicking.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDisableGeometryPicking.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butDisableGeometryPicking.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDisableGeometryPicking.Image = Properties.Resources.actions_HideCustomGeometry_1_16;
			butDisableGeometryPicking.ImageTransparentColor = System.Drawing.Color.Magenta;
			butDisableGeometryPicking.Name = "butDisableGeometryPicking";
			butDisableGeometryPicking.Size = new System.Drawing.Size(23, 29);
			butDisableGeometryPicking.Tag = "DisableGeometryPicking";
			// 
			// butDisableHiddenRoomPicking
			// 
			butDisableHiddenRoomPicking.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDisableHiddenRoomPicking.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butDisableHiddenRoomPicking.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDisableHiddenRoomPicking.Image = Properties.Resources.actions_HideHiddenRooms_16;
			butDisableHiddenRoomPicking.ImageTransparentColor = System.Drawing.Color.Magenta;
			butDisableHiddenRoomPicking.Name = "butDisableHiddenRoomPicking";
			butDisableHiddenRoomPicking.Size = new System.Drawing.Size(23, 29);
			butDisableHiddenRoomPicking.Tag = "DisableHiddenRoomPicking";
			// 
			// butDrawObjects
			// 
			butDrawObjects.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawObjects.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butDrawObjects.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { butDrawMoveables, butDrawStatics, butDrawImportedGeometry, butDrawGhostBlocks, butDrawVolumes, butDrawBoundingBoxes, butDrawOther, butDrawLightRadius });
			butDrawObjects.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawObjects.Image = Properties.Resources.actions_DrawObjects_16;
			butDrawObjects.ImageTransparentColor = System.Drawing.Color.Magenta;
			butDrawObjects.Name = "butDrawObjects";
			butDrawObjects.Size = new System.Drawing.Size(29, 29);
			// 
			// butDrawMoveables
			// 
			butDrawMoveables.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawMoveables.Checked = true;
			butDrawMoveables.CheckOnClick = true;
			butDrawMoveables.CheckState = System.Windows.Forms.CheckState.Checked;
			butDrawMoveables.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawMoveables.Name = "butDrawMoveables";
			butDrawMoveables.Size = new System.Drawing.Size(202, 22);
			butDrawMoveables.Tag = "DrawMoveables";
			butDrawMoveables.Text = "DrawMoveables";
			// 
			// butDrawStatics
			// 
			butDrawStatics.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawStatics.Checked = true;
			butDrawStatics.CheckOnClick = true;
			butDrawStatics.CheckState = System.Windows.Forms.CheckState.Checked;
			butDrawStatics.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawStatics.Name = "butDrawStatics";
			butDrawStatics.Size = new System.Drawing.Size(202, 22);
			butDrawStatics.Tag = "DrawStatics";
			butDrawStatics.Text = "DrawStatics";
			// 
			// butDrawImportedGeometry
			// 
			butDrawImportedGeometry.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawImportedGeometry.Checked = true;
			butDrawImportedGeometry.CheckOnClick = true;
			butDrawImportedGeometry.CheckState = System.Windows.Forms.CheckState.Checked;
			butDrawImportedGeometry.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawImportedGeometry.Name = "butDrawImportedGeometry";
			butDrawImportedGeometry.Size = new System.Drawing.Size(202, 22);
			butDrawImportedGeometry.Tag = "DrawImportedGeometry";
			butDrawImportedGeometry.Text = "DrawImportedGeometry";
			// 
			// butDrawGhostBlocks
			// 
			butDrawGhostBlocks.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawGhostBlocks.Checked = true;
			butDrawGhostBlocks.CheckOnClick = true;
			butDrawGhostBlocks.CheckState = System.Windows.Forms.CheckState.Checked;
			butDrawGhostBlocks.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawGhostBlocks.Name = "butDrawGhostBlocks";
			butDrawGhostBlocks.Size = new System.Drawing.Size(202, 22);
			butDrawGhostBlocks.Tag = "DrawGhostBlocks";
			butDrawGhostBlocks.Text = "DrawGhostBlocks";
			// 
			// butDrawVolumes
			// 
			butDrawVolumes.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawVolumes.Checked = true;
			butDrawVolumes.CheckState = System.Windows.Forms.CheckState.Checked;
			butDrawVolumes.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawVolumes.Name = "butDrawVolumes";
			butDrawVolumes.Size = new System.Drawing.Size(202, 22);
			butDrawVolumes.Tag = "DrawVolumes";
			butDrawVolumes.Text = "DrawVolumes";
			// 
			// butDrawBoundingBoxes
			// 
			butDrawBoundingBoxes.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawBoundingBoxes.Checked = true;
			butDrawBoundingBoxes.CheckState = System.Windows.Forms.CheckState.Checked;
			butDrawBoundingBoxes.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawBoundingBoxes.Name = "butDrawBoundingBoxes";
			butDrawBoundingBoxes.Size = new System.Drawing.Size(202, 22);
			butDrawBoundingBoxes.Tag = "DrawBoundingBoxes";
			butDrawBoundingBoxes.Text = "DrawBoundingBoxes";
			// 
			// butDrawOther
			// 
			butDrawOther.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawOther.Checked = true;
			butDrawOther.CheckOnClick = true;
			butDrawOther.CheckState = System.Windows.Forms.CheckState.Checked;
			butDrawOther.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawOther.Name = "butDrawOther";
			butDrawOther.Size = new System.Drawing.Size(202, 22);
			butDrawOther.Tag = "DrawOtherObjects";
			butDrawOther.Text = "DrawOtherObjects";
			// 
			// butDrawLightRadius
			// 
			butDrawLightRadius.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butDrawLightRadius.Checked = true;
			butDrawLightRadius.CheckOnClick = true;
			butDrawLightRadius.CheckState = System.Windows.Forms.CheckState.Checked;
			butDrawLightRadius.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butDrawLightRadius.Name = "butDrawLightRadius";
			butDrawLightRadius.Size = new System.Drawing.Size(202, 22);
			butDrawLightRadius.Tag = "DrawLightRadius";
			butDrawLightRadius.Text = "DrawLightRadius";
			// 
			// butFlipMap
			// 
			butFlipMap.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butFlipMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butFlipMap.Enabled = false;
			butFlipMap.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butFlipMap.Image = Properties.Resources.general_copy_link_16;
			butFlipMap.ImageTransparentColor = System.Drawing.Color.Magenta;
			butFlipMap.Name = "butFlipMap";
			butFlipMap.Size = new System.Drawing.Size(23, 29);
			butFlipMap.Tag = "ToggleFlipMap";
			// 
			// butCopy
			// 
			butCopy.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butCopy.Enabled = false;
			butCopy.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butCopy.Image = Properties.Resources.general_copy_16;
			butCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
			butCopy.Name = "butCopy";
			butCopy.Size = new System.Drawing.Size(23, 29);
			butCopy.Tag = "Copy";
			// 
			// butPaste
			// 
			butPaste.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butPaste.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butPaste.Image = Properties.Resources.general_clipboard_16;
			butPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
			butPaste.Name = "butPaste";
			butPaste.Size = new System.Drawing.Size(23, 29);
			butPaste.Tag = "Paste";
			// 
			// butStamp
			// 
			butStamp.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butStamp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butStamp.Enabled = false;
			butStamp.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butStamp.Image = Properties.Resources.actions_rubber_stamp_16;
			butStamp.ImageTransparentColor = System.Drawing.Color.Magenta;
			butStamp.Name = "butStamp";
			butStamp.Size = new System.Drawing.Size(23, 29);
			butStamp.Tag = "StampObject";
			// 
			// butOpacityNone
			// 
			butOpacityNone.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butOpacityNone.CheckOnClick = true;
			butOpacityNone.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butOpacityNone.Enabled = false;
			butOpacityNone.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butOpacityNone.Image = Properties.Resources.texture_Solid_16;
			butOpacityNone.ImageTransparentColor = System.Drawing.Color.Magenta;
			butOpacityNone.Name = "butOpacityNone";
			butOpacityNone.Size = new System.Drawing.Size(23, 29);
			butOpacityNone.Tag = "ToggleNoOpacity";
			// 
			// butOpacitySolidFaces
			// 
			butOpacitySolidFaces.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butOpacitySolidFaces.CheckOnClick = true;
			butOpacitySolidFaces.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butOpacitySolidFaces.Enabled = false;
			butOpacitySolidFaces.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butOpacitySolidFaces.Image = Properties.Resources.texture_ToggleOpacity_16;
			butOpacitySolidFaces.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			butOpacitySolidFaces.ImageTransparentColor = System.Drawing.Color.Magenta;
			butOpacitySolidFaces.Name = "butOpacitySolidFaces";
			butOpacitySolidFaces.Size = new System.Drawing.Size(23, 29);
			butOpacitySolidFaces.Tag = "ToggleOpacity";
			// 
			// butOpacityTraversableFaces
			// 
			butOpacityTraversableFaces.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butOpacityTraversableFaces.CheckOnClick = true;
			butOpacityTraversableFaces.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butOpacityTraversableFaces.Enabled = false;
			butOpacityTraversableFaces.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butOpacityTraversableFaces.Image = Properties.Resources.texture_ToggleOpacity2_16;
			butOpacityTraversableFaces.ImageTransparentColor = System.Drawing.Color.Magenta;
			butOpacityTraversableFaces.Name = "butOpacityTraversableFaces";
			butOpacityTraversableFaces.Size = new System.Drawing.Size(23, 29);
			butOpacityTraversableFaces.Tag = "ToggleOpacity2";
			// 
			// butAddCamera
			// 
			butAddCamera.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butAddCamera.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butAddCamera.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butAddCamera.Image = Properties.Resources.objects_Camera_16;
			butAddCamera.ImageTransparentColor = System.Drawing.Color.Magenta;
			butAddCamera.Name = "butAddCamera";
			butAddCamera.Size = new System.Drawing.Size(23, 29);
			butAddCamera.Tag = "AddCamera";
			// 
			// butAddSprite
			// 
			butAddSprite.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butAddSprite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butAddSprite.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butAddSprite.Image = Properties.Resources.objects_Sprite_16;
			butAddSprite.ImageTransparentColor = System.Drawing.Color.Magenta;
			butAddSprite.Name = "butAddSprite";
			butAddSprite.Size = new System.Drawing.Size(23, 29);
			butAddSprite.Tag = "AddSprite";
			// 
			// butAddFlybyCamera
			// 
			butAddFlybyCamera.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butAddFlybyCamera.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butAddFlybyCamera.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butAddFlybyCamera.Image = Properties.Resources.objects_movie_projector_16;
			butAddFlybyCamera.ImageTransparentColor = System.Drawing.Color.Magenta;
			butAddFlybyCamera.Name = "butAddFlybyCamera";
			butAddFlybyCamera.Size = new System.Drawing.Size(23, 29);
			butAddFlybyCamera.Tag = "AddFlybyCamera";
			// 
			// butAddSink
			// 
			butAddSink.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butAddSink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butAddSink.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butAddSink.Image = Properties.Resources.objects_tornado_16;
			butAddSink.ImageTransparentColor = System.Drawing.Color.Magenta;
			butAddSink.Name = "butAddSink";
			butAddSink.Size = new System.Drawing.Size(23, 29);
			butAddSink.Tag = "AddSink";
			// 
			// butAddSoundSource
			// 
			butAddSoundSource.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butAddSoundSource.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butAddSoundSource.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butAddSoundSource.Image = Properties.Resources.objects_speaker_16;
			butAddSoundSource.ImageTransparentColor = System.Drawing.Color.Magenta;
			butAddSoundSource.Name = "butAddSoundSource";
			butAddSoundSource.Size = new System.Drawing.Size(23, 29);
			butAddSoundSource.Tag = "AddSoundSource";
			// 
			// butAddImportedGeometry
			// 
			butAddImportedGeometry.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butAddImportedGeometry.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butAddImportedGeometry.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butAddImportedGeometry.Image = Properties.Resources.objects_custom_geometry;
			butAddImportedGeometry.ImageTransparentColor = System.Drawing.Color.Magenta;
			butAddImportedGeometry.Name = "butAddImportedGeometry";
			butAddImportedGeometry.Size = new System.Drawing.Size(23, 29);
			butAddImportedGeometry.Tag = "AddImportedGeometry";
			// 
			// butAddGhostBlock
			// 
			butAddGhostBlock.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butAddGhostBlock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butAddGhostBlock.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butAddGhostBlock.Image = Properties.Resources.objects_geometry_override_16;
			butAddGhostBlock.ImageTransparentColor = System.Drawing.Color.Magenta;
			butAddGhostBlock.Name = "butAddGhostBlock";
			butAddGhostBlock.Size = new System.Drawing.Size(23, 29);
			butAddGhostBlock.Tag = "AddGhostBlock";
			// 
			// butAddMemo
			// 
			butAddMemo.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butAddMemo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butAddMemo.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butAddMemo.Image = Properties.Resources.objects_Memo_16;
			butAddMemo.ImageTransparentColor = System.Drawing.Color.Magenta;
			butAddMemo.Name = "butAddMemo";
			butAddMemo.Size = new System.Drawing.Size(23, 29);
			butAddMemo.Tag = "AddMemo";
			// 
			// butCompileLevel
			// 
			butCompileLevel.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butCompileLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butCompileLevel.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butCompileLevel.Image = Properties.Resources.actions_compile_16;
			butCompileLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
			butCompileLevel.Name = "butCompileLevel";
			butCompileLevel.Size = new System.Drawing.Size(23, 29);
			butCompileLevel.Tag = "BuildLevel";
			// 
			// butCompileLevelAndPlay
			// 
			butCompileLevelAndPlay.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butCompileLevelAndPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butCompileLevelAndPlay.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butCompileLevelAndPlay.Image = Properties.Resources.actions_play_16;
			butCompileLevelAndPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
			butCompileLevelAndPlay.Name = "butCompileLevelAndPlay";
			butCompileLevelAndPlay.Size = new System.Drawing.Size(23, 29);
			butCompileLevelAndPlay.Tag = "BuildAndPlay";
			// 
			// butCompileAndPlayPreview
			// 
			butCompileAndPlayPreview.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butCompileAndPlayPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butCompileAndPlayPreview.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butCompileAndPlayPreview.Image = Properties.Resources.actions_play_fast_16;
			butCompileAndPlayPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
			butCompileAndPlayPreview.Name = "butCompileAndPlayPreview";
			butCompileAndPlayPreview.Size = new System.Drawing.Size(23, 29);
			butCompileAndPlayPreview.Tag = "BuildAndPlayPreview";
			// 
			// butAddBoxVolume
			// 
			butAddBoxVolume.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butAddBoxVolume.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butAddBoxVolume.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butAddBoxVolume.Image = Properties.Resources.objects_volume_box_16;
			butAddBoxVolume.ImageTransparentColor = System.Drawing.Color.Magenta;
			butAddBoxVolume.Name = "butAddBoxVolume";
			butAddBoxVolume.Size = new System.Drawing.Size(23, 29);
			butAddBoxVolume.Tag = "AddBoxVolume";
			// 
			// butAddSphereVolume
			// 
			butAddSphereVolume.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butAddSphereVolume.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butAddSphereVolume.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butAddSphereVolume.Image = Properties.Resources.objects_volume_sphere_16;
			butAddSphereVolume.ImageTransparentColor = System.Drawing.Color.Magenta;
			butAddSphereVolume.Name = "butAddSphereVolume";
			butAddSphereVolume.Size = new System.Drawing.Size(23, 29);
			butAddSphereVolume.Tag = "AddSphereVolume";
			// 
			// butTextureFloor
			// 
			butTextureFloor.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butTextureFloor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butTextureFloor.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butTextureFloor.Image = Properties.Resources.texture_Floor2_16;
			butTextureFloor.ImageTransparentColor = System.Drawing.Color.Magenta;
			butTextureFloor.Name = "butTextureFloor";
			butTextureFloor.Size = new System.Drawing.Size(23, 29);
			butTextureFloor.Tag = "TextureFloor";
			// 
			// butTextureCeiling
			// 
			butTextureCeiling.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butTextureCeiling.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butTextureCeiling.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butTextureCeiling.Image = Properties.Resources.texture_Ceiling2_16;
			butTextureCeiling.ImageTransparentColor = System.Drawing.Color.Magenta;
			butTextureCeiling.Name = "butTextureCeiling";
			butTextureCeiling.Size = new System.Drawing.Size(23, 29);
			butTextureCeiling.Tag = "TextureCeiling";
			// 
			// butTextureWalls
			// 
			butTextureWalls.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butTextureWalls.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butTextureWalls.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butTextureWalls.Image = Properties.Resources.texture_Walls2_16;
			butTextureWalls.ImageTransparentColor = System.Drawing.Color.Magenta;
			butTextureWalls.Name = "butTextureWalls";
			butTextureWalls.Size = new System.Drawing.Size(23, 29);
			butTextureWalls.Tag = "TextureWalls";
			// 
			// butEditLevelSettings
			// 
			butEditLevelSettings.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butEditLevelSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butEditLevelSettings.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butEditLevelSettings.Image = Properties.Resources.general_settings_16;
			butEditLevelSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
			butEditLevelSettings.Name = "butEditLevelSettings";
			butEditLevelSettings.Size = new System.Drawing.Size(23, 29);
			butEditLevelSettings.Tag = "EditLevelSettings";
			// 
			// butToggleFlyMode
			// 
			butToggleFlyMode.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butToggleFlyMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butToggleFlyMode.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butToggleFlyMode.Image = Properties.Resources.general_airplane_16;
			butToggleFlyMode.ImageTransparentColor = System.Drawing.Color.Magenta;
			butToggleFlyMode.Name = "butToggleFlyMode";
			butToggleFlyMode.Size = new System.Drawing.Size(23, 29);
			butToggleFlyMode.Tag = "ToggleFlyMode";
			// 
			// butSearch
			// 
			butSearch.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butSearch.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butSearch.Image = Properties.Resources.general_search_16;
			butSearch.ImageTransparentColor = System.Drawing.Color.Magenta;
			butSearch.Name = "butSearch";
			butSearch.Size = new System.Drawing.Size(23, 29);
			butSearch.Tag = "Search";
			// 
			// butSearchAndReplaceObjects
			// 
			butSearchAndReplaceObjects.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butSearchAndReplaceObjects.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butSearchAndReplaceObjects.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butSearchAndReplaceObjects.Image = Properties.Resources.general_search_and_replace_16;
			butSearchAndReplaceObjects.ImageTransparentColor = System.Drawing.Color.Magenta;
			butSearchAndReplaceObjects.Name = "butSearchAndReplaceObjects";
			butSearchAndReplaceObjects.Size = new System.Drawing.Size(23, 29);
			butSearchAndReplaceObjects.Tag = "SearchAndReplaceObjects";
			// 
			// panel3D
			// 
			panel3D.AllowDrop = true;
			panel3D.Dock = System.Windows.Forms.DockStyle.Fill;
			panel3D.Location = new System.Drawing.Point(0, 0);
			panel3D.Name = "panel3D";
			panel3D.Size = new System.Drawing.Size(1290, 239);
			panel3D.TabIndex = 13;
			// 
			// panel2DMap
			// 
			panel2DMap.Dock = System.Windows.Forms.DockStyle.Fill;
			panel2DMap.Location = new System.Drawing.Point(0, 0);
			panel2DMap.Name = "panel2DMap";
			panel2DMap.Size = new System.Drawing.Size(1290, 239);
			panel2DMap.TabIndex = 14;
			panel2DMap.Visible = false;
			panel2DMap.DragDrop += new System.Windows.Forms.DragEventHandler(this.panel2DMap_DragDrop);
			panel2DMap.DragEnter += new System.Windows.Forms.DragEventHandler(this.panel2DMap_DragEnter);
			// 
			// panelStats
			// 
			panelStats.AutoSize = true;
			panelStats.Controls.Add(tbStats);
			panelStats.Controls.Add(panelStepHeightOptions);
			panelStats.Dock = System.Windows.Forms.DockStyle.Bottom;
			panelStats.Location = new System.Drawing.Point(0, 307);
			panelStats.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			panelStats.Name = "panelStats";
			panelStats.Padding = new System.Windows.Forms.Padding(5, 5, 5, 0);
			panelStats.Size = new System.Drawing.Size(1505, 26);
			panelStats.TabIndex = 15;
			// 
			// tbStats
			// 
			tbStats.AutoSize = true;
			tbStats.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tbStats.BorderStyle = System.Windows.Forms.BorderStyle.None;
			tbStats.Dock = System.Windows.Forms.DockStyle.Top;
			tbStats.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			tbStats.Location = new System.Drawing.Point(5, 5);
			tbStats.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			tbStats.Name = "tbStats";
			tbStats.ReadOnly = true;
			tbStats.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			tbStats.ShortcutsEnabled = false;
			tbStats.Size = new System.Drawing.Size(1303, 21);
			tbStats.TabIndex = 0;
			tbStats.TabStop = false;
			tbStats.Text = "";
			tbStats.WordWrap = false;
			// 
			// lblStepHeight
			// 
			lblStepHeight.Dock = System.Windows.Forms.DockStyle.Left;
			lblStepHeight.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblStepHeight.Location = new System.Drawing.Point(0, 5);
			lblStepHeight.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			lblStepHeight.Name = "lblStepHeight";
			lblStepHeight.Size = new System.Drawing.Size(88, 11);
			lblStepHeight.TabIndex = 0;
			lblStepHeight.Text = "Step height:";
			lblStepHeight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panelStepHeightOptions
			// 
			panelStepHeightOptions.AutoSize = true;
			panelStepHeightOptions.Controls.Add(comboStepHeight);
			panelStepHeightOptions.Controls.Add(lblStepHeight);
			panelStepHeightOptions.Dock = System.Windows.Forms.DockStyle.Right;
			panelStepHeightOptions.Location = new System.Drawing.Point(1308, 5);
			panelStepHeightOptions.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			panelStepHeightOptions.Name = "panelStepHeightOptions";
			panelStepHeightOptions.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
			panelStepHeightOptions.Size = new System.Drawing.Size(192, 21);
			panelStepHeightOptions.TabIndex = 16;
			// 
			// comboStepHeight
			// 
			comboStepHeight.Dock = System.Windows.Forms.DockStyle.Right;
			comboStepHeight.Items.AddRange(new object[] { "32 (Eighth)", "64 (Quarter)", "128 (Half)", "256 (Full)" });
			comboStepHeight.Location = new System.Drawing.Point(88, 5);
			comboStepHeight.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			comboStepHeight.Name = "comboStepHeight";
			comboStepHeight.Size = new System.Drawing.Size(104, 24);
			comboStepHeight.TabIndex = 1;
			toolTip.SetToolTip(comboStepHeight, "Set minimum step height for room geometry actions");
			comboStepHeight.SelectedIndexChanged += comboStepHeight_SelectedIndexChanged;
			// 
			// panelMainView
			// 
			panelMainView.Controls.Add(this.panel3D);
			panelMainView.Controls.Add(this.panel2DMap);
			panelMainView.Dock = System.Windows.Forms.DockStyle.Fill;
			panelMainView.Location = new System.Drawing.Point(0, 32);
			panelMainView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			panelMainView.Name = "panelMainView";
			panelMainView.Size = new System.Drawing.Size(1505, 275);
			panelMainView.TabIndex = 16;
			// 
			// butMirror
			// 
			butMirror.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			butMirror.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			butMirror.Enabled = false;
			butMirror.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			butMirror.Image = Properties.Resources.texture_MirrorPortal_16;
			butMirror.ImageTransparentColor = System.Drawing.Color.Magenta;
			butMirror.Name = "butMirror";
			butMirror.Size = new System.Drawing.Size(23, 29);
			butMirror.Tag = "ToggleClassicPortalMirror";
			// 
			// MainView
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			Controls.Add(panelMainView);
			Controls.Add(panelStats);
			Controls.Add(toolStrip);
			DockText = "";
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Name = "MainView";
			SerializationKey = "MainView";
			Size = new System.Drawing.Size(1505, 333);
			toolStrip.ResumeLayout(false);
			toolStrip.PerformLayout();
			panelStats.ResumeLayout(false);
			panelStats.PerformLayout();
			panelStepHeightOptions.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private DarkUI.Controls.DarkToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton but2D;
        private System.Windows.Forms.ToolStripButton but3D;
        private System.Windows.Forms.ToolStripButton butObjectPlacement;
        private System.Windows.Forms.ToolStripButton butFaceEdit;
        private System.Windows.Forms.ToolStripButton butLightingMode;
        private System.Windows.Forms.ToolStripButton butFlipMap;
        private System.Windows.Forms.ToolStripButton butCopy;
        private System.Windows.Forms.ToolStripButton butPaste;
        private System.Windows.Forms.ToolStripButton butStamp;
        private System.Windows.Forms.ToolStripButton butOpacityNone;
        private System.Windows.Forms.ToolStripButton butOpacitySolidFaces;
        private System.Windows.Forms.ToolStripButton butOpacityTraversableFaces;
        private System.Windows.Forms.ToolStripButton butAddCamera;
        private System.Windows.Forms.ToolStripButton butAddFlybyCamera;
        private System.Windows.Forms.ToolStripButton butAddSoundSource;
        private System.Windows.Forms.ToolStripButton butAddSink;
        private System.Windows.Forms.ToolStripButton butAddGhostBlock;
        private System.Windows.Forms.ToolStripButton butCompileLevel;
        private System.Windows.Forms.ToolStripButton butCompileLevelAndPlay;
        private TombEditor.Controls.Panel3D.Panel3D panel3D;
        private TombEditor.Controls.Panel2DMap panel2DMap;
        private System.Windows.Forms.ToolStripButton butCenterCamera;
        private System.Windows.Forms.ToolStripDropDownButton butDrawObjects;
        private System.Windows.Forms.ToolStripMenuItem butDrawMoveables;
        private System.Windows.Forms.ToolStripMenuItem butDrawStatics;
        private System.Windows.Forms.ToolStripButton butDrawPortals;
        private System.Windows.Forms.ToolStripButton butDrawHorizon;
        private System.Windows.Forms.ToolStripButton butDrawIllegalSlopes;
        private System.Windows.Forms.ToolStripButton butDrawRoomNames;
        private System.Windows.Forms.ToolStripMenuItem butDrawImportedGeometry;
        private System.Windows.Forms.ToolStripMenuItem butDrawOther;
        private System.Windows.Forms.ToolStripButton butDrawSlideDirections;
        private System.Windows.Forms.ToolStripButton butDisableGeometryPicking;
        private System.Windows.Forms.ToolStripButton butDrawCardinalDirections;
        private System.Windows.Forms.ToolStripButton butDrawExtraBlendingModes;
        private System.Windows.Forms.ToolStripButton butUndo;
        private System.Windows.Forms.ToolStripButton butRedo;
        private System.Windows.Forms.ToolStripButton butDrawAllRooms;
        private System.Windows.Forms.ToolStripMenuItem butDrawGhostBlocks;
        private System.Windows.Forms.ToolStripButton butAddImportedGeometry;
        private System.Windows.Forms.ToolStripButton butTextureFloor;
        private System.Windows.Forms.ToolStripButton butTextureCeiling;
        private System.Windows.Forms.ToolStripButton butTextureWalls;
        private System.Windows.Forms.ToolStripButton butEditLevelSettings;
        private System.Windows.Forms.ToolStripButton butToggleFlyMode;
        private System.Windows.Forms.ToolStripButton butSearch;
        private System.Windows.Forms.ToolStripButton butSearchAndReplaceObjects;
        private System.Windows.Forms.ToolStripMenuItem butDrawLightRadius;
        private System.Windows.Forms.ToolStripButton butDrawWhiteLighting;
        private System.Windows.Forms.ToolStripMenuItem butDrawVolumes;
        private System.Windows.Forms.ToolStripMenuItem butDrawBoundingBoxes;
        private System.Windows.Forms.ToolStripButton butAddBoxVolume;
        private System.Windows.Forms.ToolStripButton butAddSphereVolume;
        private System.Windows.Forms.ToolStripButton butDrawStaticTint;
        private System.Windows.Forms.ToolStripButton butDisableHiddenRoomPicking;
        private System.Windows.Forms.ToolStripButton butHideTransparentFaces;
        private System.Windows.Forms.ToolStripButton butCompileAndPlayPreview;
        private System.Windows.Forms.ToolStripButton butAddSprite;
        private System.Windows.Forms.ToolStripButton butAddMemo;
        private System.Windows.Forms.Panel panelStats;
        private System.Windows.Forms.Panel panelMainView;
        private TombEditor.Controls.RichTextLabel tbStats;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolStripButton butBilinearFilter;
        private System.Windows.Forms.Panel panelStepHeightOptions;
        private DarkUI.Controls.DarkLabel lblStepHeight;
        private DarkUI.Controls.DarkComboBox comboStepHeight;
		private System.Windows.Forms.ToolStripButton butMirror;
	}
}
