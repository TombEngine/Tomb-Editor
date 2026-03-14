using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Forms;
using System.Windows.Input;
using TombLib.Forms;

namespace TombEditor.WPF.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
	[ObservableProperty] private bool _isSectorOptionsPanelVisible;
	[ObservableProperty] private bool _isRoomOptionsPanelVisible;
	[ObservableProperty] private bool _isItemBrowserVisible;
	[ObservableProperty] private bool _isImportedGeometryBrowserVisible;
	[ObservableProperty] private bool _isTriggerListVisible;
	[ObservableProperty] private bool _isLightingPanelVisible;
	[ObservableProperty] private bool _isPalettePanelVisible;
	[ObservableProperty] private bool _isTexturePanelVisible;
	[ObservableProperty] private bool _isObjectListVisible;
	[ObservableProperty] private bool _isStatisticsPanelVisible;
	[ObservableProperty] private bool _isToolPaletteVisible;
	[ObservableProperty] private bool _isToolPaletteFloating;

	// File Menu

	public ICommand NewLevelCommand { get; }
	public ICommand OpenLevelCommand { get; }
	public ICommand SaveLevelCommand { get; }
	public ICommand SaveLevelAsCommand { get; }
	public ICommand ImportPrjCommand { get; }
	public ICommand ConvertLevelToTombEngineCommand { get; }
	public ICommand BuildAndPlayCommand { get; }
	public ICommand BuildLevelCommand { get; }
	public ICommand QuitEditorCommand { get; }

	// Edit Menu

	public ICommand UndoCommand { get; }
	public ICommand RedoCommand { get; }
	public ICommand CutCommand { get; }
	public ICommand CopyCommand { get; }
	public ICommand PasteCommand { get; }
	public ICommand StampObjectCommand { get; }
	public ICommand DeleteCommand { get; }
	public ICommand SelectAllCommand { get; }
	public ICommand BookmarkObjectCommand { get; }
	public ICommand SelectBookmarkedObject { get; }
	public ICommand EditObjectCommand { get; }
	public ICommand EditVolumeEventSetsCommand { get; }
	public ICommand EditGlobalEventSetsCommand { get; }
	public ICommand SearchCommand { get; }
	public ICommand SearchAndReplaceObjectsCommand { get; }

	// View Menu

	public ICommand ResetCameraCommand { get; }
	public ICommand RelocateCameraCommand { get; }
	public ICommand ToggleFlyModeCommand { get; }
	public ICommand DrawWhiteTextureLightingOnlyCommand { get; }
	public ICommand ShowRealTintForObjectsCommand { get; }

	// Rooms Menu

	public ICommand NewRoomUpCommand { get; }
	public ICommand NewRoomDownCommand { get; }
	public ICommand NewRoomLeftCommand { get; }
	public ICommand NewRoomRightCommand { get; }
	public ICommand NewRoomFrontCommand { get; }
	public ICommand NewRoomBackCommand { get; }
	public ICommand DuplicateRoomCommand { get; }
	public ICommand CropRoomCommand { get; }
	public ICommand SplitRoomCommand { get; }
	public ICommand MergeRoomsHorizontallyCommand { get; }
	public ICommand DeleteRoomsCommand { get; }
	public ICommand MoveRoomUpCommand { get; }
	public ICommand MoveRoomUp4ClicksCommand { get; }
	public ICommand MoveRoomDownCommand { get; }
	public ICommand MoveRoomDown4ClicksCommand { get; }
	public ICommand MoveRoomLeftCommand { get; }
	public ICommand MoveRoomRightCommand { get; }
	public ICommand MoveRoomForwardCommand { get; }
	public ICommand MoveRoomBackCommand { get; }
	public ICommand RotateRoomsClockwiseCommand { get; }
	public ICommand RotateRoomsCounterClockwiseCommand { get; }
	public ICommand MirrorRoomsXCommand { get; }
	public ICommand MirrorRoomsZCommand { get; }
	public ICommand SelectWaterRoomsCommand { get; }
	public ICommand SelectSkyRoomsCommand { get; }
	public ICommand SelectOutsideRoomsCommand { get; }
	public ICommand SelectQuicksandRoomsCommand { get; }
	public ICommand SelectConnectedRoomsCommand { get; }
	public ICommand SelectRoomsByTagsCommand { get; }
	public ICommand ExportRoomsCommand { get; }
	public ICommand ImportRoomsCommand { get; }
	public ICommand ApplyRoomPropertiesCommand { get; }

	// Items Menu

	public ICommand AddWadCommand { get; }
	public ICommand RemoveWadsCommand { get; }
	public ICommand ReloadWadsCommand { get; }
	public ICommand ReloadSoundsCommand { get; }
	public ICommand AddCameraCommand { get; }
	public ICommand AddFlybyCameraCommand { get; }
	public ICommand AddSpriteCommand { get; }
	public ICommand AddSinkCommand { get; }
	public ICommand AddSoundSourceCommand { get; }
	public ICommand AddImportedGeometryCommand { get; }
	public ICommand AddGhostBlockCommand { get; }
	public ICommand AddMemoCommand { get; }
	public ICommand AddPortalCommand { get; }
	public ICommand AddTriggerCommand { get; }
	public ICommand AddBoxVolumeCommand { get; }
	public ICommand AddSphereVolumeCommand { get; }
	public ICommand DeleteAllLightsCommand { get; }
	public ICommand DeleteAllObjectsCommand { get; }
	public ICommand DeleteAllTriggersCommand { get; }
	public ICommand DeleteMissingObjectsCommand { get; }
	public ICommand LocateItemCommand { get; }
	public ICommand MoveLaraCommand { get; }
	public ICommand SelectAllObjectsInAreaCommand { get; }
	public ICommand SelectFloorBelowObjectCommand { get; }
	public ICommand SplitSectorObjectOnSelectionCommand { get; }
	public ICommand SetStaticMeshesColorToRoomLightCommand { get; }
	public ICommand SetStaticMeshesColorCommand { get; }
	public ICommand MakeQuickItemGroupCommand { get; }
	public ICommand GetObjectStatisticsCommand { get; }
	public ICommand GenerateObjectNamesCommand { get; }

	// Textures Menu

	public ICommand AddTextureCommand { get; }
	public ICommand RemoveTexturesCommand { get; }
	public ICommand UnloadTexturesCommand { get; }
	public ICommand ReloadTexturesCommand { get; }
	public ICommand ConvertTexturesToPNGCommand { get; }
	public ICommand RemapTextureCommand { get; }
	public ICommand TextureFloorCommand { get; }
	public ICommand TextureWallsCommand { get; }
	public ICommand TextureCeilingCommand { get; }
	public ICommand ClearAllTexturesInRoomCommand { get; }
	public ICommand ClearAllTexturesInLevelCommand { get; }
	public ICommand SearchTexturesCommand { get; }
	public ICommand EditAnimationRangesCommand { get; }

	// Transform Menu

	public ICommand IncreaseStepHeightCommand { get; }
	public ICommand DecreaseStepHeightCommand { get; }
	public ICommand SmoothRandomFloorUpCommand { get; }
	public ICommand SmoothRandomFloorDownCommand { get; }
	public ICommand SmoothRandomCeilingUpCommand { get; }
	public ICommand SmoothRandomCeilingDownCommand { get; }
	public ICommand SharpRandomFloorUpCommand { get; }
	public ICommand SharpRandomFloorDownCommand { get; }
	public ICommand SharpRandomCeilingUpCommand { get; }
	public ICommand SharpRandomCeilingDownCommand { get; }
	public ICommand AverageFloorCommand { get; }
	public ICommand AverageCeilingCommand { get; }
	public ICommand FlattenFloorCommand { get; }
	public ICommand FlattenCeilingCommand { get; }
	public ICommand ResetGeometryCommand { get; }
	public ICommand GridWallsIn3Command { get; }
	public ICommand GridWallsIn5Command { get; }
	public ICommand GridWallsIn3SquaresCommand { get; }
	public ICommand GridWallsIn5SquaresCommand { get; }

	// Tools Menu

	public ICommand EditLevelSettingsCommand { get; }
	public ICommand EditOptionsCommand { get; }
	public ICommand EditKeyboardLayoutCommand { get; }
	public ICommand StartWadToolCommand { get; }
	public ICommand StartSoundToolCommand { get; }

	// Window Menu

	public ICommand ShowSectorOptionsCommand { get; }
	public ICommand ShowRoomOptionsCommand { get; }
	public ICommand ShowItemBrowserCommand { get; }
	public ICommand ShowImportedGeometryBrowserCommand { get; }
	public ICommand ShowTriggerListCommand { get; }
	public ICommand ShowLightingCommand { get; }
	public ICommand ShowPaletteCommand { get; }
	public ICommand ShowTexturePanelCommand { get; }
	public ICommand ShowObjectListCommand { get; }
	public ICommand ShowStatisticsCommand { get; }
	public ICommand ShowToolPaletteCommand { get; }
	public ICommand ShowToolPaletteFloatingCommand { get; }

	// Tool Bar Commands

	public ICommand Switch2DModeCommand { get; }
	public ICommand SwitchGeometryModeCommand { get; }
	public ICommand SwitchFaceEditModeCommand { get; }
	public ICommand SwitchLightingModeCommand { get; }

	// Non-menu Commands

	public ICommand CancelAnyActionCommand { get; }

	public ICommand RaiseQA1ClickCommand { get; }
	public ICommand RaiseQA4ClickCommand { get; }
	public ICommand LowerQA1ClickCommand { get; }
	public ICommand LowerQA4ClickCommand { get; }

	public ICommand RaiseWS1ClickCommand { get; }
	public ICommand RaiseWS4ClickCommand { get; }
	public ICommand LowerWS1ClickCommand { get; }
	public ICommand LowerWS4ClickCommand { get; }

	public ICommand RaiseED1ClickCommand { get; }
	public ICommand RaiseED4ClickCommand { get; }
	public ICommand LowerED1ClickCommand { get; }
	public ICommand LowerED4ClickCommand { get; }

	public ICommand RaiseRF1ClickCommand { get; }
	public ICommand RaiseRF4ClickCommand { get; }
	public ICommand LowerRF1ClickCommand { get; }
	public ICommand LowerRF4ClickCommand { get; }

	public ICommand RaiseQA1ClickSmoothCommand { get; }
	public ICommand RaiseQA4ClickSmoothCommand { get; }
	public ICommand LowerQA1ClickSmoothCommand { get; }
	public ICommand LowerQA4ClickSmoothCommand { get; }

	public ICommand RaiseWS1ClickSmoothCommand { get; }
	public ICommand RaiseWS4ClickSmoothCommand { get; }
	public ICommand LowerWS1ClickSmoothCommand { get; }
	public ICommand LowerWS4ClickSmoothCommand { get; }

	public ICommand RaiseED1ClickSmoothCommand { get; }
	public ICommand RaiseED4ClickSmoothCommand { get; }
	public ICommand LowerED1ClickSmoothCommand { get; }
	public ICommand LowerED4ClickSmoothCommand { get; }

	public ICommand RaiseRF1ClickSmoothCommand { get; }
	public ICommand RaiseRF4ClickSmoothCommand { get; }
	public ICommand LowerRF1ClickSmoothCommand { get; }
	public ICommand LowerRF4ClickSmoothCommand { get; }

	public ICommand RaiseYH1ClickCommand { get; }
	public ICommand RaiseYH4ClickCommand { get; }
	public ICommand LowerYH1ClickCommand { get; }
	public ICommand LowerYH4ClickCommand { get; }

	public ICommand RaiseUJ1ClickCommand { get; }
	public ICommand RaiseUJ4ClickCommand { get; }
	public ICommand LowerUJ1ClickCommand { get; }
	public ICommand LowerUJ4ClickCommand { get; }

	// Blend Mode Commands

	public ICommand SwitchBlendModeCommand { get; }
	public ICommand SetBlendModeNormalCommand { get; }
	public ICommand SetBlendModeAddCommand { get; }
	public ICommand SetBlendModeSubtractCommand { get; }
	public ICommand SetBlendModeExcludeCommand { get; }
	public ICommand SetBlendModeScreenCommand { get; }
	public ICommand SetBlendModeLightenCommand { get; }

	// Uncategorized Commands

	public ICommand AddTriggerWithBookmarkCommand { get; }
	public ICommand RenameObjectCommand { get; }
	public ICommand EditObjectColorCommand { get; }

	public ICommand SetTextureDoubleSidedCommand { get; }
	public ICommand SetTextureInvisibleCommand { get; }
	public ICommand ChangeTextureSelectionTileSizeCommand { get; }

	public ICommand RotateObjectLeftCommand { get; }
	public ICommand RotateObjectRightCommand { get; }
	public ICommand RotateObjectUpCommand { get; }
	public ICommand RotateObjectDownCommand { get; }
	public ICommand MoveObjectLeftCommand { get; }
	public ICommand MoveObjectRightCommand { get; }
	public ICommand MoveObjectForwardCommand { get; }
	public ICommand MoveObjectBackCommand { get; }
	public ICommand MoveObjectUpCommand { get; }
	public ICommand MoveObjectDownCommand { get; }

	public ICommand SelectPreviousRoomCommand { get; }
	public ICommand RotateObject5Command { get; }
	public ICommand RotateObject45Command { get; }
	public ICommand MoveObjectToCurrentRoomCommand { get; }
	public ICommand RotateTextureCommand { get; }
	public ICommand MirrorTextureCommand { get; }
	public ICommand BuildAndPlayPreviewCommand { get; }
	public ICommand SelectBookmarkedObjectCommand { get; }
	public ICommand AddNewRoomCommand { get; }
	public ICommand LockRoomCommand { get; }
	public ICommand HideRoomCommand { get; }
	public ICommand EditAmbientLightCommand { get; }
	public ICommand AddBoxVolumeInSelectedAreaCommand { get; }
	public ICommand AddItemCommand { get; }
	public ICommand AssignAndClipboardScriptIdCommand { get; }
	public ICommand FlipFloorSplitCommand { get; }
	public ICommand FlipCeilingSplitCommand { get; }
	public ICommand SwitchTool1Command { get; }
	public ICommand SwitchTool2Command { get; }
	public ICommand SwitchTool3Command { get; }
	public ICommand SwitchTool4Command { get; }
	public ICommand SwitchTool5Command { get; }
	public ICommand SwitchTool6Command { get; }
	public ICommand SwitchTool7Command { get; }
	public ICommand SwitchTool8Command { get; }
	public ICommand SwitchTool9Command { get; }
	public ICommand SwitchTool10Command { get; }
	public ICommand SwitchTool11Command { get; }
	public ICommand SwitchTool12Command { get; }
	public ICommand SwitchTool13Command { get; }
	public ICommand SwitchTool14Command { get; }
	public ICommand DrawPortalsCommand { get; }
	public ICommand DrawHorizonCommand { get; }
	public ICommand DrawRoomNamesCommand { get; }
	public ICommand DrawIllegalSlopesCommand { get; }
	public ICommand DrawMoveablesCommand { get; }
	public ICommand DrawStaticsCommand { get; }
	public ICommand DrawImportedGeometryCommand { get; }
	public ICommand DrawGhostBlocksCommand { get; }
	public ICommand DrawVolumesCommand { get; }
	public ICommand DrawBoundingBoxesCommand { get; }
	public ICommand DrawOtherObjectsCommand { get; }
	public ICommand DrawLightRadiusCommand { get; }
	public ICommand DrawSlideDirectionsCommand { get; }
	public ICommand DrawExtraBlendingModesCommand { get; }
	public ICommand HideTransparentFacesCommand { get; }
	public ICommand BilinearFilterCommand { get; }
	public ICommand DisableGeometryPickingCommand { get; }
	public ICommand DisableHiddenRoomPickingCommand { get; }
	public ICommand DrawAllRoomsCommand { get; }
	public ICommand DrawCardinalDirectionsCommand { get; }
	public ICommand SamplePaletteFromTexturesCommand { get; }
	public ICommand ResetPaletteCommand { get; }
	public ICommand ToggleFlipMapCommand { get; }
	public ICommand ToggleNoOpacityCommand { get; }
	public ICommand ToggleOpacityCommand { get; }
	public ICommand ToggleOpacity2Command { get; }
	public ICommand AddPointLightCommand { get; }
	public ICommand AddShadowCommand { get; }
	public ICommand AddSunLightCommand { get; }
	public ICommand AddSpotLightCommand { get; }
	public ICommand AddEffectLightCommand { get; }
	public ICommand AddFogBulbCommand { get; }
	public ICommand EditRoomNameCommand { get; }
	public ICommand SetFloorCommand { get; }
	public ICommand SetCeilingCommand { get; }
	public ICommand SetWallCommand { get; }
	public ICommand SetBoxCommand { get; }
	public ICommand SetDeathCommand { get; }
	public ICommand SetMonkeyswingCommand { get; }
	public ICommand SetClimbPositiveZCommand { get; }
	public ICommand SetClimbPositiveXCommand { get; }
	public ICommand SetClimbNegativeZCommand { get; }
	public ICommand SetClimbNegativeXCommand { get; }
	public ICommand SetNotWalkableCommand { get; }
	public ICommand SetDiagonalFloorStepCommand { get; }
	public ICommand SetDiagonalCeilingStepCommand { get; }
	public ICommand SetDiagonalWallCommand { get; }
	public ICommand SetBeetleCheckpointCommand { get; }
	public ICommand SetTriggerTriggererCommand { get; }
	public ICommand ToggleForceFloorSolidCommand { get; }
	public ICommand AddGhostBlocksToSelectionCommand { get; }
	public ICommand SetRoomOutsideCommand { get; }
	public ICommand SetRoomSkyboxCommand { get; }
	public ICommand SetRoomNoLensflareCommand { get; }
	public ICommand SetRoomNoPathfindingCommand { get; }
	public ICommand SetRoomColdCommand { get; }
	public ICommand SetRoomDamageCommand { get; }
	public ICommand InPlaceSearchRoomsCommand { get; }
	public ICommand InPlaceSearchItemsCommand { get; }
	public ICommand InPlaceSearchTexturesCommand { get; }
	public ICommand InPlaceSearchImportedGeometryCommand { get; }
	public ICommand SearchMenusCommand { get; }
	public ICommand MoveObjectLeftPreciseCommand { get; }
	public ICommand MoveObjectRightPreciseCommand { get; }
	public ICommand MoveObjectForwardPreciseCommand { get; }
	public ICommand MoveObjectBackPreciseCommand { get; }
	public ICommand MoveObjectUpPreciseCommand { get; }
	public ICommand MoveObjectDownPreciseCommand { get; }
	public ICommand HighlightSplit1Command { get; }
	public ICommand HighlightSplit2Command { get; }
	public ICommand HighlightSplit3Command { get; }
	public ICommand HighlightSplit4Command { get; }
	public ICommand HighlightSplit5Command { get; }
	public ICommand HighlightSplit6Command { get; }
	public ICommand HighlightSplit7Command { get; }
	public ICommand HighlightSplit8Command { get; }
	public ICommand HighlightSplit9Command { get; }

	private readonly Editor _editor;

	public MainWindowViewModel(Editor editor)
	{
		_editor = editor;
		_editor.EditorEventRaised += EditorEventRaised;

		var commandArgs = new CommandArgs(WPFUtils.GetWin32WindowFromCaller(this), _editor);

		// File Menu

		NewLevelCommand = CommandHandler.GetCommand("NewLevel", commandArgs);
		OpenLevelCommand = CommandHandler.GetCommand("OpenLevel", commandArgs);
		SaveLevelCommand = CommandHandler.GetCommand("SaveLevel", commandArgs);
		SaveLevelAsCommand = CommandHandler.GetCommand("SaveLevelAs", commandArgs);
		ImportPrjCommand = CommandHandler.GetCommand("ImportPrj", commandArgs);
		ConvertLevelToTombEngineCommand = CommandHandler.GetCommand("ConvertLevelToTombEngine", commandArgs);
		BuildAndPlayCommand = CommandHandler.GetCommand("BuildAndPlay", commandArgs);
		BuildLevelCommand = CommandHandler.GetCommand("BuildLevel", commandArgs);
		QuitEditorCommand = CommandHandler.GetCommand("QuitEditor", commandArgs);

		// Edit Menu

		UndoCommand = CommandHandler.GetCommand("Undo", commandArgs);
		RedoCommand = CommandHandler.GetCommand("Redo", commandArgs);
		CutCommand = CommandHandler.GetCommand("Cut", commandArgs);
		CopyCommand = CommandHandler.GetCommand("Copy", commandArgs);
		PasteCommand = CommandHandler.GetCommand("Paste", commandArgs);
		StampObjectCommand = CommandHandler.GetCommand("StampObject", commandArgs);
		DeleteCommand = CommandHandler.GetCommand("Delete", commandArgs);
		SelectAllCommand = CommandHandler.GetCommand("SelectAll", commandArgs);
		BookmarkObjectCommand = CommandHandler.GetCommand("BookmarkObject", commandArgs);
		SelectBookmarkedObject = CommandHandler.GetCommand("SelectBookmarkedObject", commandArgs);
		EditObjectCommand = CommandHandler.GetCommand("EditObject", commandArgs);
		EditVolumeEventSetsCommand = CommandHandler.GetCommand("EditVolumeEventSets", commandArgs);
		EditGlobalEventSetsCommand = CommandHandler.GetCommand("EditGlobalEventSets", commandArgs);
		SearchCommand = CommandHandler.GetCommand("Search", commandArgs);
		SearchAndReplaceObjectsCommand = CommandHandler.GetCommand("SearchAndReplaceObjects", commandArgs);

		// View Menu

		ResetCameraCommand = CommandHandler.GetCommand("ResetCamera", commandArgs);
		RelocateCameraCommand = CommandHandler.GetCommand("RelocateCamera", commandArgs);
		ToggleFlyModeCommand = CommandHandler.GetCommand("ToggleFlyMode", commandArgs);
		DrawWhiteTextureLightingOnlyCommand = CommandHandler.GetCommand("DrawWhiteTextureLightingOnly", commandArgs);
		ShowRealTintForObjectsCommand = CommandHandler.GetCommand("ShowRealTintForObjects", commandArgs);

		// Rooms Menu

		NewRoomUpCommand = CommandHandler.GetCommand("NewRoomUp", commandArgs);
		NewRoomDownCommand = CommandHandler.GetCommand("NewRoomDown", commandArgs);
		NewRoomLeftCommand = CommandHandler.GetCommand("NewRoomLeft", commandArgs);
		NewRoomRightCommand = CommandHandler.GetCommand("NewRoomRight", commandArgs);
		NewRoomFrontCommand = CommandHandler.GetCommand("NewRoomFront", commandArgs);
		NewRoomBackCommand = CommandHandler.GetCommand("NewRoomBack", commandArgs);
		DuplicateRoomCommand = CommandHandler.GetCommand("DuplicateRoom", commandArgs);
		CropRoomCommand = CommandHandler.GetCommand("CropRoom", commandArgs);
		SplitRoomCommand = CommandHandler.GetCommand("SplitRoom", commandArgs);
		MergeRoomsHorizontallyCommand = CommandHandler.GetCommand("MergeRoomsHorizontally", commandArgs);
		DeleteRoomsCommand = CommandHandler.GetCommand("DeleteRooms", commandArgs);
		MoveRoomUpCommand = CommandHandler.GetCommand("MoveRoomUp", commandArgs);
		MoveRoomUp4ClicksCommand = CommandHandler.GetCommand("MoveRoomUp4Clicks", commandArgs);
		MoveRoomDownCommand = CommandHandler.GetCommand("MoveRoomDown", commandArgs);
		MoveRoomDown4ClicksCommand = CommandHandler.GetCommand("MoveRoomDown4Clicks", commandArgs);
		MoveRoomLeftCommand = CommandHandler.GetCommand("MoveRoomLeft", commandArgs);
		MoveRoomRightCommand = CommandHandler.GetCommand("MoveRoomRight", commandArgs);
		MoveRoomForwardCommand = CommandHandler.GetCommand("MoveRoomForward", commandArgs);
		MoveRoomBackCommand = CommandHandler.GetCommand("MoveRoomBack", commandArgs);
		RotateRoomsClockwiseCommand = CommandHandler.GetCommand("RotateRoomsClockwise", commandArgs);
		RotateRoomsCounterClockwiseCommand = CommandHandler.GetCommand("RotateRoomsCounterClockwise", commandArgs);
		MirrorRoomsXCommand = CommandHandler.GetCommand("MirrorRoomsX", commandArgs);
		MirrorRoomsZCommand = CommandHandler.GetCommand("MirrorRoomsZ", commandArgs);
		SelectWaterRoomsCommand = CommandHandler.GetCommand("SelectWaterRooms", commandArgs);
		SelectSkyRoomsCommand = CommandHandler.GetCommand("SelectSkyRooms", commandArgs);
		SelectOutsideRoomsCommand = CommandHandler.GetCommand("SelectOutsideRooms", commandArgs);
		SelectQuicksandRoomsCommand = CommandHandler.GetCommand("SelectQuicksandRooms", commandArgs);
		SelectConnectedRoomsCommand = CommandHandler.GetCommand("SelectConnectedRooms", commandArgs);
		SelectRoomsByTagsCommand = CommandHandler.GetCommand("SelectRoomsByTags", commandArgs);
		ExportRoomsCommand = CommandHandler.GetCommand("ExportRooms", commandArgs);
		ImportRoomsCommand = CommandHandler.GetCommand("ImportRooms", commandArgs);
		ApplyRoomPropertiesCommand = CommandHandler.GetCommand("ApplyRoomProperties", commandArgs);

		// Items Menu

		AddWadCommand = CommandHandler.GetCommand("AddWad", commandArgs);
		RemoveWadsCommand = CommandHandler.GetCommand("RemoveWads", commandArgs);
		ReloadWadsCommand = CommandHandler.GetCommand("ReloadWads", commandArgs);
		ReloadSoundsCommand = CommandHandler.GetCommand("ReloadSounds", commandArgs);
		AddCameraCommand = CommandHandler.GetCommand("AddCamera", commandArgs);
		AddFlybyCameraCommand = CommandHandler.GetCommand("AddFlybyCamera", commandArgs);
		AddSpriteCommand = CommandHandler.GetCommand("AddSprite", commandArgs);
		AddSinkCommand = CommandHandler.GetCommand("AddSink", commandArgs);
		AddSoundSourceCommand = CommandHandler.GetCommand("AddSoundSource", commandArgs);
		AddImportedGeometryCommand = CommandHandler.GetCommand("AddImportedGeometry", commandArgs);
		AddGhostBlockCommand = CommandHandler.GetCommand("AddGhostBlock", commandArgs);
		AddMemoCommand = CommandHandler.GetCommand("AddMemo", commandArgs);
		AddPortalCommand = CommandHandler.GetCommand("AddPortal", commandArgs);
		AddTriggerCommand = CommandHandler.GetCommand("AddTrigger", commandArgs);
		AddBoxVolumeCommand = CommandHandler.GetCommand("AddBoxVolume", commandArgs);
		AddSphereVolumeCommand = CommandHandler.GetCommand("AddSphereVolume", commandArgs);
		DeleteAllLightsCommand = CommandHandler.GetCommand("DeleteAllLights", commandArgs);
		DeleteAllObjectsCommand = CommandHandler.GetCommand("DeleteAllObjects", commandArgs);
		DeleteAllTriggersCommand = CommandHandler.GetCommand("DeleteAllTriggers", commandArgs);
		DeleteMissingObjectsCommand = CommandHandler.GetCommand("DeleteMissingObjects", commandArgs);
		LocateItemCommand = CommandHandler.GetCommand("LocateItem", commandArgs);
		MoveLaraCommand = CommandHandler.GetCommand("MoveLara", commandArgs);
		SelectAllObjectsInAreaCommand = CommandHandler.GetCommand("SelectAllObjectsInArea", commandArgs);
		SelectFloorBelowObjectCommand = CommandHandler.GetCommand("SelectFloorBelowObject", commandArgs);
		SplitSectorObjectOnSelectionCommand = CommandHandler.GetCommand("SplitSectorObjectOnSelection", commandArgs);
		SetStaticMeshesColorToRoomLightCommand = CommandHandler.GetCommand("SetStaticMeshesColorToRoomLight", commandArgs);
		SetStaticMeshesColorCommand = CommandHandler.GetCommand("SetStaticMeshesColor", commandArgs);
		MakeQuickItemGroupCommand = CommandHandler.GetCommand("MakeQuickItemGroup", commandArgs);
		GetObjectStatisticsCommand = CommandHandler.GetCommand("GetObjectStatistics", commandArgs);
		GenerateObjectNamesCommand = CommandHandler.GetCommand("GenerateObjectNames", commandArgs);

		// Textures Menu

		AddTextureCommand = CommandHandler.GetCommand("AddTexture", commandArgs);
		RemoveTexturesCommand = CommandHandler.GetCommand("RemoveTextures", commandArgs);
		UnloadTexturesCommand = CommandHandler.GetCommand("UnloadTextures", commandArgs);
		ReloadTexturesCommand = CommandHandler.GetCommand("ReloadTextures", commandArgs);
		ConvertTexturesToPNGCommand = CommandHandler.GetCommand("ConvertTexturesToPNG", commandArgs);
		RemapTextureCommand = CommandHandler.GetCommand("RemapTexture", commandArgs);
		TextureFloorCommand = CommandHandler.GetCommand("TextureFloor", commandArgs);
		TextureWallsCommand = CommandHandler.GetCommand("TextureWalls", commandArgs);
		TextureCeilingCommand = CommandHandler.GetCommand("TextureCeiling", commandArgs);
		ClearAllTexturesInRoomCommand = CommandHandler.GetCommand("ClearAllTexturesInRoom", commandArgs);
		ClearAllTexturesInLevelCommand = CommandHandler.GetCommand("ClearAllTexturesInLevel", commandArgs);
		SearchTexturesCommand = CommandHandler.GetCommand("SearchTextures", commandArgs);
		EditAnimationRangesCommand = CommandHandler.GetCommand("EditAnimationRanges", commandArgs);

		// Transform Menu

		IncreaseStepHeightCommand = CommandHandler.GetCommand("IncreaseStepHeight", commandArgs);
		DecreaseStepHeightCommand = CommandHandler.GetCommand("DecreaseStepHeight", commandArgs);
		SmoothRandomFloorUpCommand = CommandHandler.GetCommand("SmoothRandomFloorUp", commandArgs);
		SmoothRandomFloorDownCommand = CommandHandler.GetCommand("SmoothRandomFloorDown", commandArgs);
		SmoothRandomCeilingUpCommand = CommandHandler.GetCommand("SmoothRandomCeilingUp", commandArgs);
		SmoothRandomCeilingDownCommand = CommandHandler.GetCommand("SmoothRandomCeilingDown", commandArgs);
		SharpRandomFloorUpCommand = CommandHandler.GetCommand("SharpRandomFloorUp", commandArgs);
		SharpRandomFloorDownCommand = CommandHandler.GetCommand("SharpRandomFloorDown", commandArgs);
		SharpRandomCeilingUpCommand = CommandHandler.GetCommand("SharpRandomCeilingUp", commandArgs);
		SharpRandomCeilingDownCommand = CommandHandler.GetCommand("SharpRandomCeilingDown", commandArgs);
		AverageFloorCommand = CommandHandler.GetCommand("AverageFloor", commandArgs);
		AverageCeilingCommand = CommandHandler.GetCommand("AverageCeiling", commandArgs);
		FlattenFloorCommand = CommandHandler.GetCommand("FlattenFloor", commandArgs);
		FlattenCeilingCommand = CommandHandler.GetCommand("FlattenCeiling", commandArgs);
		ResetGeometryCommand = CommandHandler.GetCommand("ResetGeometry", commandArgs);
		GridWallsIn3Command = CommandHandler.GetCommand("GridWallsIn3", commandArgs);
		GridWallsIn5Command = CommandHandler.GetCommand("GridWallsIn5", commandArgs);
		GridWallsIn3SquaresCommand = CommandHandler.GetCommand("GridWallsIn3Squares", commandArgs);
		GridWallsIn5SquaresCommand = CommandHandler.GetCommand("GridWallsIn5Squares", commandArgs);

		// Tools Menu

		EditLevelSettingsCommand = CommandHandler.GetCommand("EditLevelSettings", commandArgs);
		EditOptionsCommand = CommandHandler.GetCommand("EditOptions", commandArgs);
		EditKeyboardLayoutCommand = CommandHandler.GetCommand("EditKeyboardLayout", commandArgs);
		StartWadToolCommand = CommandHandler.GetCommand("StartWadTool", commandArgs);
		StartSoundToolCommand = CommandHandler.GetCommand("StartSoundTool", commandArgs);

		// Tool Bar Commands

		Switch2DModeCommand = CommandHandler.GetCommand("Switch2DMode", commandArgs);
		SwitchGeometryModeCommand = CommandHandler.GetCommand("SwitchGeometryMode", commandArgs);
		SwitchFaceEditModeCommand = CommandHandler.GetCommand("SwitchFaceEditMode", commandArgs);
		SwitchLightingModeCommand = CommandHandler.GetCommand("SwitchLightingMode", commandArgs);

		// Non-menu Commands

		CancelAnyActionCommand = CommandHandler.GetCommand("CancelAnyAction", commandArgs);

		RaiseQA1ClickCommand = CommandHandler.GetCommand("RaiseQA1Click", commandArgs);
		RaiseQA4ClickCommand = CommandHandler.GetCommand("RaiseQA4Click", commandArgs);
		LowerQA1ClickCommand = CommandHandler.GetCommand("LowerQA1Click", commandArgs);
		LowerQA4ClickCommand = CommandHandler.GetCommand("LowerQA4Click", commandArgs);

		RaiseWS1ClickCommand = CommandHandler.GetCommand("RaiseWS1Click", commandArgs);
		RaiseWS4ClickCommand = CommandHandler.GetCommand("RaiseWS4Click", commandArgs);
		LowerWS1ClickCommand = CommandHandler.GetCommand("LowerWS1Click", commandArgs);
		LowerWS4ClickCommand = CommandHandler.GetCommand("LowerWS4Click", commandArgs);

		RaiseED1ClickCommand = CommandHandler.GetCommand("RaiseED1Click", commandArgs);
		RaiseED4ClickCommand = CommandHandler.GetCommand("RaiseED4Click", commandArgs);
		LowerED1ClickCommand = CommandHandler.GetCommand("LowerED1Click", commandArgs);
		LowerED4ClickCommand = CommandHandler.GetCommand("LowerED4Click", commandArgs);

		RaiseRF1ClickCommand = CommandHandler.GetCommand("RaiseRF1Click", commandArgs);
		RaiseRF4ClickCommand = CommandHandler.GetCommand("RaiseRF4Click", commandArgs);
		LowerRF1ClickCommand = CommandHandler.GetCommand("LowerRF1Click", commandArgs);
		LowerRF4ClickCommand = CommandHandler.GetCommand("LowerRF4Click", commandArgs);

		RaiseQA1ClickSmoothCommand = CommandHandler.GetCommand("RaiseQA1ClickSmooth", commandArgs);
		RaiseQA4ClickSmoothCommand = CommandHandler.GetCommand("RaiseQA4ClickSmooth", commandArgs);
		LowerQA1ClickSmoothCommand = CommandHandler.GetCommand("LowerQA1ClickSmooth", commandArgs);
		LowerQA4ClickSmoothCommand = CommandHandler.GetCommand("LowerQA4ClickSmooth", commandArgs);

		RaiseWS1ClickSmoothCommand = CommandHandler.GetCommand("RaiseWS1ClickSmooth", commandArgs);
		RaiseWS4ClickSmoothCommand = CommandHandler.GetCommand("RaiseWS4ClickSmooth", commandArgs);
		LowerWS1ClickSmoothCommand = CommandHandler.GetCommand("LowerWS1ClickSmooth", commandArgs);
		LowerWS4ClickSmoothCommand = CommandHandler.GetCommand("LowerWS4ClickSmooth", commandArgs);

		RaiseED1ClickSmoothCommand = CommandHandler.GetCommand("RaiseED1ClickSmooth", commandArgs);
		RaiseED4ClickSmoothCommand = CommandHandler.GetCommand("RaiseED4ClickSmooth", commandArgs);
		LowerED1ClickSmoothCommand = CommandHandler.GetCommand("LowerED1ClickSmooth", commandArgs);
		LowerED4ClickSmoothCommand = CommandHandler.GetCommand("LowerED4ClickSmooth", commandArgs);

		RaiseRF1ClickSmoothCommand = CommandHandler.GetCommand("RaiseRF1ClickSmooth", commandArgs);
		RaiseRF4ClickSmoothCommand = CommandHandler.GetCommand("RaiseRF4ClickSmooth", commandArgs);
		LowerRF1ClickSmoothCommand = CommandHandler.GetCommand("LowerRF1ClickSmooth", commandArgs);
		LowerRF4ClickSmoothCommand = CommandHandler.GetCommand("LowerRF4ClickSmooth", commandArgs);

		RaiseYH1ClickCommand = CommandHandler.GetCommand("RaiseYH1Click", commandArgs);
		RaiseYH4ClickCommand = CommandHandler.GetCommand("RaiseYH4Click", commandArgs);
		LowerYH1ClickCommand = CommandHandler.GetCommand("LowerYH1Click", commandArgs);
		LowerYH4ClickCommand = CommandHandler.GetCommand("LowerYH4Click", commandArgs);

		RaiseUJ1ClickCommand = CommandHandler.GetCommand("RaiseUJ1Click", commandArgs);
		RaiseUJ4ClickCommand = CommandHandler.GetCommand("RaiseUJ4Click", commandArgs);
		LowerUJ1ClickCommand = CommandHandler.GetCommand("LowerUJ1Click", commandArgs);
		LowerUJ4ClickCommand = CommandHandler.GetCommand("LowerUJ4Click", commandArgs);

		// Blend Mode Commands

		SwitchBlendModeCommand = CommandHandler.GetCommand("SwitchBlendMode", commandArgs);
		SetBlendModeNormalCommand = CommandHandler.GetCommand("SetBlendModeNormal", commandArgs);
		SetBlendModeAddCommand = CommandHandler.GetCommand("SetBlendModeAdd", commandArgs);
		SetBlendModeSubtractCommand = CommandHandler.GetCommand("SetBlendModeSubtract", commandArgs);
		SetBlendModeExcludeCommand = CommandHandler.GetCommand("SetBlendModeExclude", commandArgs);
		SetBlendModeScreenCommand = CommandHandler.GetCommand("SetBlendModeScreen", commandArgs);
		SetBlendModeLightenCommand = CommandHandler.GetCommand("SetBlendModeLighten", commandArgs);

		// Uncategorized Commands

		AddTriggerWithBookmarkCommand = CommandHandler.GetCommand("AddTriggerWithBookmark", commandArgs);
		RenameObjectCommand = CommandHandler.GetCommand("RenameObject", commandArgs);
		EditObjectColorCommand = CommandHandler.GetCommand("EditObjectColor", commandArgs);

		SetTextureDoubleSidedCommand = CommandHandler.GetCommand("SetTextureDoubleSided", commandArgs);
		SetTextureInvisibleCommand = CommandHandler.GetCommand("SetTextureInvisible", commandArgs);
		ChangeTextureSelectionTileSizeCommand = CommandHandler.GetCommand("ChangeTextureSelectionTileSize", commandArgs);

		RotateObjectLeftCommand = CommandHandler.GetCommand("RotateObjectLeft", commandArgs);
		RotateObjectRightCommand = CommandHandler.GetCommand("RotateObjectRight", commandArgs);
		RotateObjectUpCommand = CommandHandler.GetCommand("RotateObjectUp", commandArgs);
		RotateObjectDownCommand = CommandHandler.GetCommand("RotateObjectDown", commandArgs);
		MoveObjectLeftCommand = CommandHandler.GetCommand("MoveObjectLeft", commandArgs);
		MoveObjectRightCommand = CommandHandler.GetCommand("MoveObjectRight", commandArgs);
		MoveObjectForwardCommand = CommandHandler.GetCommand("MoveObjectForward", commandArgs);
		MoveObjectBackCommand = CommandHandler.GetCommand("MoveObjectBack", commandArgs);
		MoveObjectUpCommand = CommandHandler.GetCommand("MoveObjectUp", commandArgs);
		MoveObjectDownCommand = CommandHandler.GetCommand("MoveObjectDown", commandArgs);

		SelectPreviousRoomCommand = CommandHandler.GetCommand("SelectPreviousRoom", commandArgs);
		RotateObject5Command = CommandHandler.GetCommand("RotateObject5", commandArgs);
		RotateObject45Command = CommandHandler.GetCommand("RotateObject45", commandArgs);
		MoveObjectToCurrentRoomCommand = CommandHandler.GetCommand("MoveObjectToCurrentRoom", commandArgs);
		RotateTextureCommand = CommandHandler.GetCommand("RotateTexture", commandArgs);
		MirrorTextureCommand = CommandHandler.GetCommand("MirrorTexture", commandArgs);
		BuildAndPlayPreviewCommand = CommandHandler.GetCommand("BuildAndPlayPreview", commandArgs);
		SelectBookmarkedObjectCommand = CommandHandler.GetCommand("SelectBookmarkedObject", commandArgs);
		AddNewRoomCommand = CommandHandler.GetCommand("AddNewRoom", commandArgs);
		LockRoomCommand = CommandHandler.GetCommand("LockRoom", commandArgs);
		HideRoomCommand = CommandHandler.GetCommand("HideRoom", commandArgs);
		EditAmbientLightCommand = CommandHandler.GetCommand("EditAmbientLight", commandArgs);
		AddBoxVolumeInSelectedAreaCommand = CommandHandler.GetCommand("AddBoxVolumeInSelectedArea", commandArgs);
		AddItemCommand = CommandHandler.GetCommand("AddItem", commandArgs);
		AssignAndClipboardScriptIdCommand = CommandHandler.GetCommand("AssignAndClipboardScriptId", commandArgs);
		FlipFloorSplitCommand = CommandHandler.GetCommand("FlipFloorSplit", commandArgs);
		FlipCeilingSplitCommand = CommandHandler.GetCommand("FlipCeilingSplit", commandArgs);
		SwitchTool1Command = CommandHandler.GetCommand("SwitchTool1", commandArgs);
		SwitchTool2Command = CommandHandler.GetCommand("SwitchTool2", commandArgs);
		SwitchTool3Command = CommandHandler.GetCommand("SwitchTool3", commandArgs);
		SwitchTool4Command = CommandHandler.GetCommand("SwitchTool4", commandArgs);
		SwitchTool5Command = CommandHandler.GetCommand("SwitchTool5", commandArgs);
		SwitchTool6Command = CommandHandler.GetCommand("SwitchTool6", commandArgs);
		SwitchTool7Command = CommandHandler.GetCommand("SwitchTool7", commandArgs);
		SwitchTool8Command = CommandHandler.GetCommand("SwitchTool8", commandArgs);
		SwitchTool9Command = CommandHandler.GetCommand("SwitchTool9", commandArgs);
		SwitchTool10Command = CommandHandler.GetCommand("SwitchTool10", commandArgs);
		SwitchTool11Command = CommandHandler.GetCommand("SwitchTool11", commandArgs);
		SwitchTool12Command = CommandHandler.GetCommand("SwitchTool12", commandArgs);
		SwitchTool13Command = CommandHandler.GetCommand("SwitchTool13", commandArgs);
		SwitchTool14Command = CommandHandler.GetCommand("SwitchTool14", commandArgs);
		DrawPortalsCommand = CommandHandler.GetCommand("DrawPortals", commandArgs);
		DrawHorizonCommand = CommandHandler.GetCommand("DrawHorizon", commandArgs);
		DrawRoomNamesCommand = CommandHandler.GetCommand("DrawRoomNames", commandArgs);
		DrawIllegalSlopesCommand = CommandHandler.GetCommand("DrawIllegalSlopes", commandArgs);
		DrawMoveablesCommand = CommandHandler.GetCommand("DrawMoveables", commandArgs);
		DrawStaticsCommand = CommandHandler.GetCommand("DrawStatics", commandArgs);
		DrawImportedGeometryCommand = CommandHandler.GetCommand("DrawImportedGeometry", commandArgs);
		DrawGhostBlocksCommand = CommandHandler.GetCommand("DrawGhostBlocks", commandArgs);
		DrawVolumesCommand = CommandHandler.GetCommand("DrawVolumes", commandArgs);
		DrawBoundingBoxesCommand = CommandHandler.GetCommand("DrawBoundingBoxes", commandArgs);
		DrawOtherObjectsCommand = CommandHandler.GetCommand("DrawOtherObjects", commandArgs);
		DrawLightRadiusCommand = CommandHandler.GetCommand("DrawLightRadius", commandArgs);
		DrawSlideDirectionsCommand = CommandHandler.GetCommand("DrawSlideDirections", commandArgs);
		DrawExtraBlendingModesCommand = CommandHandler.GetCommand("DrawExtraBlendingModes", commandArgs);
		HideTransparentFacesCommand = CommandHandler.GetCommand("HideTransparentFaces", commandArgs);
		BilinearFilterCommand = CommandHandler.GetCommand("BilinearFilter", commandArgs);
		DisableGeometryPickingCommand = CommandHandler.GetCommand("DisableGeometryPicking", commandArgs);
		DisableHiddenRoomPickingCommand = CommandHandler.GetCommand("DisableHiddenRoomPicking", commandArgs);
		DrawAllRoomsCommand = CommandHandler.GetCommand("DrawAllRooms", commandArgs);
		DrawCardinalDirectionsCommand = CommandHandler.GetCommand("DrawCardinalDirections", commandArgs);
		SamplePaletteFromTexturesCommand = CommandHandler.GetCommand("SamplePaletteFromTextures", commandArgs);
		ResetPaletteCommand = CommandHandler.GetCommand("ResetPalette", commandArgs);
		ToggleFlipMapCommand = CommandHandler.GetCommand("ToggleFlipMap", commandArgs);
		ToggleNoOpacityCommand = CommandHandler.GetCommand("ToggleNoOpacity", commandArgs);
		ToggleOpacityCommand = CommandHandler.GetCommand("ToggleOpacity", commandArgs);
		ToggleOpacity2Command = CommandHandler.GetCommand("ToggleOpacity2", commandArgs);
		AddPointLightCommand = CommandHandler.GetCommand("AddPointLight", commandArgs);
		AddShadowCommand = CommandHandler.GetCommand("AddShadow", commandArgs);
		AddSunLightCommand = CommandHandler.GetCommand("AddSunLight", commandArgs);
		AddSpotLightCommand = CommandHandler.GetCommand("AddSpotLight", commandArgs);
		AddEffectLightCommand = CommandHandler.GetCommand("AddEffectLight", commandArgs);
		AddFogBulbCommand = CommandHandler.GetCommand("AddFogBulb", commandArgs);
		EditRoomNameCommand = CommandHandler.GetCommand("EditRoomName", commandArgs);
		SetFloorCommand = CommandHandler.GetCommand("SetFloor", commandArgs);
		SetCeilingCommand = CommandHandler.GetCommand("SetCeiling", commandArgs);
		SetWallCommand = CommandHandler.GetCommand("SetWall", commandArgs);
		SetBoxCommand = CommandHandler.GetCommand("SetBox", commandArgs);
		SetDeathCommand = CommandHandler.GetCommand("SetDeath", commandArgs);
		SetMonkeyswingCommand = CommandHandler.GetCommand("SetMonkeyswing", commandArgs);
		SetClimbPositiveZCommand = CommandHandler.GetCommand("SetClimbPositiveZ", commandArgs);
		SetClimbPositiveXCommand = CommandHandler.GetCommand("SetClimbPositiveX", commandArgs);
		SetClimbNegativeZCommand = CommandHandler.GetCommand("SetClimbNegativeZ", commandArgs);
		SetClimbNegativeXCommand = CommandHandler.GetCommand("SetClimbNegativeX", commandArgs);
		SetNotWalkableCommand = CommandHandler.GetCommand("SetNotWalkable", commandArgs);
		SetDiagonalFloorStepCommand = CommandHandler.GetCommand("SetDiagonalFloorStep", commandArgs);
		SetDiagonalCeilingStepCommand = CommandHandler.GetCommand("SetDiagonalCeilingStep", commandArgs);
		SetDiagonalWallCommand = CommandHandler.GetCommand("SetDiagonalWall", commandArgs);
		SetBeetleCheckpointCommand = CommandHandler.GetCommand("SetBeetleCheckpoint", commandArgs);
		SetTriggerTriggererCommand = CommandHandler.GetCommand("SetTriggerTriggerer", commandArgs);
		ToggleForceFloorSolidCommand = CommandHandler.GetCommand("ToggleForceFloorSolid", commandArgs);
		AddGhostBlocksToSelectionCommand = CommandHandler.GetCommand("AddGhostBlocksToSelection", commandArgs);
		SetRoomOutsideCommand = CommandHandler.GetCommand("SetRoomOutside", commandArgs);
		SetRoomSkyboxCommand = CommandHandler.GetCommand("SetRoomSkybox", commandArgs);
		SetRoomNoLensflareCommand = CommandHandler.GetCommand("SetRoomNoLensflare", commandArgs);
		SetRoomNoPathfindingCommand = CommandHandler.GetCommand("SetRoomNoPathfinding", commandArgs);
		SetRoomColdCommand = CommandHandler.GetCommand("SetRoomCold", commandArgs);
		SetRoomDamageCommand = CommandHandler.GetCommand("SetRoomDamage", commandArgs);
		InPlaceSearchRoomsCommand = CommandHandler.GetCommand("InPlaceSearchRooms", commandArgs);
		InPlaceSearchItemsCommand = CommandHandler.GetCommand("InPlaceSearchItems", commandArgs);
		InPlaceSearchTexturesCommand = CommandHandler.GetCommand("InPlaceSearchTextures", commandArgs);
		InPlaceSearchImportedGeometryCommand = CommandHandler.GetCommand("InPlaceSearchImportedGeometry", commandArgs);
		SearchMenusCommand = CommandHandler.GetCommand("SearchMenus", commandArgs);
		MoveObjectLeftPreciseCommand = CommandHandler.GetCommand("MoveObjectLeftPrecise", commandArgs);
		MoveObjectRightPreciseCommand = CommandHandler.GetCommand("MoveObjectRightPrecise", commandArgs);
		MoveObjectForwardPreciseCommand = CommandHandler.GetCommand("MoveObjectForwardPrecise", commandArgs);
		MoveObjectBackPreciseCommand = CommandHandler.GetCommand("MoveObjectBackPrecise", commandArgs);
		MoveObjectUpPreciseCommand = CommandHandler.GetCommand("MoveObjectUpPrecise", commandArgs);
		MoveObjectDownPreciseCommand = CommandHandler.GetCommand("MoveObjectDownPrecise", commandArgs);
		HighlightSplit1Command = CommandHandler.GetCommand("HighlightSplit1", commandArgs);
		HighlightSplit2Command = CommandHandler.GetCommand("HighlightSplit2", commandArgs);
		HighlightSplit3Command = CommandHandler.GetCommand("HighlightSplit3", commandArgs);
		HighlightSplit4Command = CommandHandler.GetCommand("HighlightSplit4", commandArgs);
		HighlightSplit5Command = CommandHandler.GetCommand("HighlightSplit5", commandArgs);
		HighlightSplit6Command = CommandHandler.GetCommand("HighlightSplit6", commandArgs);
		HighlightSplit7Command = CommandHandler.GetCommand("HighlightSplit7", commandArgs);
		HighlightSplit8Command = CommandHandler.GetCommand("HighlightSplit8", commandArgs);
		HighlightSplit9Command = CommandHandler.GetCommand("HighlightSplit9", commandArgs);
	}

	private void EditorEventRaised(IEditorEvent obj)
	{
		if (obj is Editor.ConfigurationChangedEvent)
		{
			KeyBindingsWrapper.Instance.Invalidate();
		}
		else if (obj is Editor.ModeChangedEvent modeChanged)
		{
			OnPropertyChanged(nameof(IsMap2DMode));
			OnPropertyChanged(nameof(IsGeometryMode));
			OnPropertyChanged(nameof(IsFaceEditMode));
			OnPropertyChanged(nameof(IsLightingMode));
		}
	}

	[RelayCommand]
	public void About()
	{
		using var form = new FormAbout(Properties.Resources.misc_AboutScreen_800) { StartPosition = FormStartPosition.CenterScreen };
		form.ShowDialog(WPFUtils.GetWin32WindowFromCaller(this));
	}

	[RelayCommand]
	public void RestoreDefaultLayout()
	{
		Console.WriteLine("DEBUG");
	}

	[RelayCommand]
	public void SetEditorMode(EditorMode mode)
	{
		_editor.Mode = mode;
	}

	public bool IsMap2DMode => _editor.Mode == EditorMode.Map2D;
	public bool IsGeometryMode => _editor.Mode == EditorMode.Geometry;
	public bool IsFaceEditMode => _editor.Mode == EditorMode.FaceEdit;
	public bool IsLightingMode => _editor.Mode == EditorMode.Lighting;
}
