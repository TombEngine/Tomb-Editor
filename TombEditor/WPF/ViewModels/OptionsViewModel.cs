using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using TombLib.LevelData;
using TombLib.Rendering;

namespace TombEditor.WPF.ViewModels
{
	public class OptionsViewModel : ViewModelBase
	{
		private readonly Configuration _config;

		#region Properties

		public IEnumerable<TRVersion.Game> GameVersions { get; set; } = Enum.GetValues<TRVersion.Game>().Cast<TRVersion.Game>();

		public int Editor_UndoDepth
		{
			get => _config.Editor_UndoDepth;
			set
			{
				_config.Editor_UndoDepth = value;
				NotifyPropertyChanged(nameof(Editor_UndoDepth));
			}
		}

		public bool Log_WriteToFile
		{
			get => _config.Log_WriteToFile;
			set
			{
				_config.Log_WriteToFile = value;
				NotifyPropertyChanged(nameof(Log_WriteToFile));
			}
		}

		public int Log_ArchiveN
		{
			get => _config.Log_ArchiveN;
			set
			{
				_config.Log_ArchiveN = value;
				NotifyPropertyChanged(nameof(Log_ArchiveN));
			}
		}

		public bool Editor_ReloadFilesAutomaticallyWhenChanged
		{
			get => _config.Editor_ReloadFilesAutomaticallyWhenChanged;
			set
			{
				_config.Editor_ReloadFilesAutomaticallyWhenChanged = value;
				NotifyPropertyChanged(nameof(Editor_ReloadFilesAutomaticallyWhenChanged));
			}
		}

		public bool Editor_OpenLastProjectOnStartup
		{
			get => _config.Editor_OpenLastProjectOnStartup;
			set
			{
				_config.Editor_OpenLastProjectOnStartup = value;
				NotifyPropertyChanged(nameof(Editor_OpenLastProjectOnStartup));
			}
		}

		public bool Editor_AllowMultipleInstances
		{
			get => _config.Editor_AllowMultipleInstances;
			set
			{
				_config.Editor_AllowMultipleInstances = value;
				NotifyPropertyChanged(nameof(Editor_AllowMultipleInstances));
			}
		}

		// Defaults

		public TRVersion.Game Editor_DefaultProjectGameVersion { get; set; } = TRVersion.Game.TRNG;

		public int Editor_DefaultNewRoomSize
		{
			get => _config.Editor_DefaultNewRoomSize;
			set
			{
				_config.Editor_DefaultNewRoomSize = value;
				NotifyPropertyChanged(nameof(Editor_DefaultNewRoomSize));
			}
		}

		public bool Editor_GridNewRoom
		{
			get => _config.Editor_GridNewRoom;
			set
			{
				_config.Editor_GridNewRoom = value;
				NotifyPropertyChanged(nameof(Editor_GridNewRoom));
			}
		}

		public bool Editor_UseHalfPixelCorrectionOnPrjImport
		{
			get => _config.Editor_UseHalfPixelCorrectionOnPrjImport;
			set
			{
				_config.Editor_UseHalfPixelCorrectionOnPrjImport = value;
				NotifyPropertyChanged(nameof(Editor_UseHalfPixelCorrectionOnPrjImport));
			}
		}

		public bool Editor_RespectFlybyPatchOnPrjImport
		{
			get => _config.Editor_RespectFlybyPatchOnPrjImport;
			set
			{
				_config.Editor_RespectFlybyPatchOnPrjImport = value;
				NotifyPropertyChanged(nameof(Editor_RespectFlybyPatchOnPrjImport));
			}
		}

		// Item preview options

		public float RenderingItem_NavigationSpeedMouseWheelZoom
		{
			get => _config.RenderingItem_NavigationSpeedMouseWheelZoom;
			set
			{
				_config.RenderingItem_NavigationSpeedMouseWheelZoom = value;
				NotifyPropertyChanged(nameof(RenderingItem_NavigationSpeedMouseWheelZoom));
			}
		}

		public float RenderingItem_NavigationSpeedMouseZoom
		{
			get => _config.RenderingItem_NavigationSpeedMouseZoom;
			set
			{
				_config.RenderingItem_NavigationSpeedMouseZoom = value;
				NotifyPropertyChanged(nameof(RenderingItem_NavigationSpeedMouseZoom));
			}
		}

		public float RenderingItem_NavigationSpeedMouseTranslate
		{
			get => _config.RenderingItem_NavigationSpeedMouseTranslate;
			set
			{
				_config.RenderingItem_NavigationSpeedMouseTranslate = value;
				NotifyPropertyChanged(nameof(RenderingItem_NavigationSpeedMouseTranslate));
			}
		}

		public float RenderingItem_NavigationSpeedMouseRotate
		{
			get => _config.RenderingItem_NavigationSpeedMouseRotate;
			set
			{
				_config.RenderingItem_NavigationSpeedMouseRotate = value;
				NotifyPropertyChanged(nameof(RenderingItem_NavigationSpeedMouseRotate));
			}
		}

		public float RenderingItem_FieldOfView
		{
			get => _config.RenderingItem_FieldOfView;
			set
			{
				_config.RenderingItem_FieldOfView = value;
				NotifyPropertyChanged(nameof(RenderingItem_FieldOfView));
			}
		}

		public bool RenderingItem_Antialias
		{
			get => _config.RenderingItem_Antialias;
			set
			{
				_config.RenderingItem_Antialias = value;
				NotifyPropertyChanged(nameof(RenderingItem_Antialias));
			}
		}

		public bool RenderingItem_HideInternalObjects
		{
			get => _config.RenderingItem_HideInternalObjects;
			set
			{
				_config.RenderingItem_HideInternalObjects = value;
				NotifyPropertyChanged(nameof(RenderingItem_HideInternalObjects));
			}
		}

		public bool RenderingItem_ShowMultipleWadsPrompt
		{
			get => _config.RenderingItem_ShowMultipleWadsPrompt;
			set
			{
				_config.RenderingItem_ShowMultipleWadsPrompt = value;
				NotifyPropertyChanged(nameof(RenderingItem_ShowMultipleWadsPrompt));
			}
		}

		public bool RenderingItem_Animate
		{
			get => _config.RenderingItem_Animate;
			set
			{
				_config.RenderingItem_Animate = value;
				NotifyPropertyChanged(nameof(RenderingItem_Animate));
			}
		}

		// Main 3D window options

		public int Rendering3D_DrawRoomsMaxDepth
		{
			get => _config.Rendering3D_DrawRoomsMaxDepth;
			set
			{
				_config.Rendering3D_DrawRoomsMaxDepth = value;
				NotifyPropertyChanged(nameof(Rendering3D_DrawRoomsMaxDepth));
			}
		}

		public float Rendering3D_NavigationSpeedKeyRotate
		{
			get => _config.Rendering3D_NavigationSpeedKeyRotate;
			set
			{
				_config.Rendering3D_NavigationSpeedKeyRotate = value;
				NotifyPropertyChanged(nameof(Rendering3D_NavigationSpeedKeyRotate));
			}
		}

		public float Rendering3D_NavigationSpeedKeyZoom
		{
			get => _config.Rendering3D_NavigationSpeedKeyZoom;
			set
			{
				_config.Rendering3D_NavigationSpeedKeyZoom = value;
				NotifyPropertyChanged(nameof(Rendering3D_NavigationSpeedKeyZoom));
			}
		}

		public float Rendering3D_NavigationSpeedMouseWheelZoom
		{
			get => _config.Rendering3D_NavigationSpeedMouseWheelZoom;
			set
			{
				_config.Rendering3D_NavigationSpeedMouseWheelZoom = value;
				NotifyPropertyChanged(nameof(Rendering3D_NavigationSpeedMouseWheelZoom));
			}
		}

		public float Rendering3D_NavigationSpeedMouseZoom
		{
			get => _config.Rendering3D_NavigationSpeedMouseZoom;
			set
			{
				_config.Rendering3D_NavigationSpeedMouseZoom = value;
				NotifyPropertyChanged(nameof(Rendering3D_NavigationSpeedMouseZoom));
			}
		}

		public float Rendering3D_NavigationSpeedMouseTranslate
		{
			get => _config.Rendering3D_NavigationSpeedMouseTranslate;
			set
			{
				_config.Rendering3D_NavigationSpeedMouseTranslate = value;
				NotifyPropertyChanged(nameof(Rendering3D_NavigationSpeedMouseTranslate));
			}
		}

		public float Rendering3D_NavigationSpeedMouseRotate
		{
			get => _config.Rendering3D_NavigationSpeedMouseRotate;
			set
			{
				_config.Rendering3D_NavigationSpeedMouseRotate = value;
				NotifyPropertyChanged(nameof(Rendering3D_NavigationSpeedMouseRotate));
			}
		}

		public float Rendering3D_DragMouseSensitivity
		{
			get => _config.Rendering3D_DragMouseSensitivity;
			set
			{
				_config.Rendering3D_DragMouseSensitivity = value;
				NotifyPropertyChanged(nameof(Rendering3D_DragMouseSensitivity));
			}
		}

		public bool Rendering3D_InvertMouseZoom
		{
			get => _config.Rendering3D_InvertMouseZoom;
			set
			{
				_config.Rendering3D_InvertMouseZoom = value;
				NotifyPropertyChanged(nameof(Rendering3D_InvertMouseZoom));
			}
		}

		public float Rendering3D_LineWidth
		{
			get => _config.Rendering3D_LineWidth;
			set
			{
				_config.Rendering3D_LineWidth = value;
				NotifyPropertyChanged(nameof(Rendering3D_LineWidth));
			}
		}

		public float Rendering3D_FieldOfView
		{
			get => _config.Rendering3D_FieldOfView;
			set
			{
				_config.Rendering3D_FieldOfView = value;
				NotifyPropertyChanged(nameof(Rendering3D_FieldOfView));
			}
		}

		public bool Rendering3D_ToolboxVisible
		{
			get => _config.Rendering3D_ToolboxVisible;
			set
			{
				_config.Rendering3D_ToolboxVisible = value;
				NotifyPropertyChanged(nameof(Rendering3D_ToolboxVisible));
			}
		}

		public Point Rendering3D_ToolboxPosition
		{
			get => _config.Rendering3D_ToolboxPosition;
			set
			{
				_config.Rendering3D_ToolboxPosition = value;
				NotifyPropertyChanged(nameof(Rendering3D_ToolboxPosition));
			}
		}

		public bool Rendering3D_DisablePickingForImportedGeometry
		{
			get => _config.Rendering3D_DisablePickingForImportedGeometry;
			set
			{
				_config.Rendering3D_DisablePickingForImportedGeometry = value;
				NotifyPropertyChanged(nameof(Rendering3D_DisablePickingForImportedGeometry));
			}
		}

		public bool Rendering3D_DisablePickingForHiddenRooms
		{
			get => _config.Rendering3D_DisablePickingForHiddenRooms;
			set
			{
				_config.Rendering3D_DisablePickingForHiddenRooms = value;
				NotifyPropertyChanged(nameof(Rendering3D_DisablePickingForHiddenRooms));
			}
		}

		public bool Rendering3D_ShowPortals
		{
			get => _config.Rendering3D_ShowPortals;
			set
			{
				_config.Rendering3D_ShowPortals = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowPortals));
			}
		}

		public bool Rendering3D_ShowHorizon
		{
			get => _config.Rendering3D_ShowHorizon;
			set
			{
				_config.Rendering3D_ShowHorizon = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowHorizon));
			}
		}

		public bool Rendering3D_ShowAllRooms
		{
			get => _config.Rendering3D_ShowAllRooms;
			set
			{
				_config.Rendering3D_ShowAllRooms = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowAllRooms));
			}
		}

		public bool Rendering3D_ShowIllegalSlopes
		{
			get => _config.Rendering3D_ShowIllegalSlopes;
			set
			{
				_config.Rendering3D_ShowIllegalSlopes = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowIllegalSlopes));
			}
		}

		public bool Rendering3D_ShowMoveables
		{
			get => _config.Rendering3D_ShowMoveables;
			set
			{
				_config.Rendering3D_ShowMoveables = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowMoveables));
			}
		}

		public bool Rendering3D_ShowStatics
		{
			get => _config.Rendering3D_ShowStatics;
			set
			{
				_config.Rendering3D_ShowStatics = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowStatics));
			}
		}

		public bool Rendering3D_ShowImportedGeometry
		{
			get => _config.Rendering3D_ShowImportedGeometry;
			set
			{
				_config.Rendering3D_ShowImportedGeometry = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowImportedGeometry));
			}
		}

		public bool Rendering3D_ShowGhostBlocks
		{
			get => _config.Rendering3D_ShowGhostBlocks;
			set
			{
				_config.Rendering3D_ShowGhostBlocks = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowGhostBlocks));
			}
		}

		public bool Rendering3D_ShowVolumes
		{
			get => _config.Rendering3D_ShowVolumes;
			set
			{
				_config.Rendering3D_ShowVolumes = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowVolumes));
			}
		}

		public bool Rendering3D_ShowBoundingBoxes
		{
			get => _config.Rendering3D_ShowBoundingBoxes;
			set
			{
				_config.Rendering3D_ShowBoundingBoxes = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowBoundingBoxes));
			}
		}

		public bool Rendering3D_ShowOtherObjects
		{
			get => _config.Rendering3D_ShowOtherObjects;
			set
			{
				_config.Rendering3D_ShowOtherObjects = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowOtherObjects));
			}
		}

		public bool Rendering3D_ShowSlideDirections
		{
			get => _config.Rendering3D_ShowSlideDirections;
			set
			{
				_config.Rendering3D_ShowSlideDirections = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowSlideDirections));
			}
		}

		public bool Rendering3D_ShowFPS
		{
			get => _config.Rendering3D_ShowFPS;
			set
			{
				_config.Rendering3D_ShowFPS = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowFPS));
			}
		}

		public bool Rendering3D_ShowRoomNames
		{
			get => _config.Rendering3D_ShowRoomNames;
			set
			{
				_config.Rendering3D_ShowRoomNames = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowRoomNames));
			}
		}

		public bool Rendering3D_ShowCardinalDirections
		{
			get => _config.Rendering3D_ShowCardinalDirections;
			set
			{
				_config.Rendering3D_ShowCardinalDirections = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowCardinalDirections));
			}
		}

		public bool Rendering3D_UseRoomEditorDirections
		{
			get => _config.Rendering3D_UseRoomEditorDirections;
			set
			{
				_config.Rendering3D_UseRoomEditorDirections = value;
				NotifyPropertyChanged(nameof(Rendering3D_UseRoomEditorDirections));
			}
		}

		public bool Rendering3D_ShowExtraBlendingModes
		{
			get => _config.Rendering3D_ShowExtraBlendingModes;
			set
			{
				_config.Rendering3D_ShowExtraBlendingModes = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowExtraBlendingModes));
			}
		}

		public bool Rendering3D_HideTransparentFaces
		{
			get => _config.Rendering3D_HideTransparentFaces;
			set
			{
				_config.Rendering3D_HideTransparentFaces = value;
				NotifyPropertyChanged(nameof(Rendering3D_HideTransparentFaces));
			}
		}

		public bool Rendering3D_BilinearFilter
		{
			get => _config.Rendering3D_BilinearFilter;
			set
			{
				_config.Rendering3D_BilinearFilter = value;
				NotifyPropertyChanged(nameof(Rendering3D_BilinearFilter));
			}
		}

		public bool Rendering3D_ShowLightingWhiteTextureOnly
		{
			get => _config.Rendering3D_ShowLightingWhiteTextureOnly;
			set
			{
				_config.Rendering3D_ShowLightingWhiteTextureOnly = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowLightingWhiteTextureOnly));
			}
		}

		public string Rendering3D_FontName
		{
			get => _config.Rendering3D_FontName;
			set
			{
				_config.Rendering3D_FontName = value;
				NotifyPropertyChanged(nameof(Rendering3D_FontName));
			}
		}

		public int Rendering3D_FontSize
		{
			get => _config.Rendering3D_FontSize;
			set
			{
				_config.Rendering3D_FontSize = value;
				NotifyPropertyChanged(nameof(Rendering3D_FontSize));
			}
		}

		public bool Rendering3D_FontIsBold
		{
			get => _config.Rendering3D_FontIsBold;
			set
			{
				_config.Rendering3D_FontIsBold = value;
				NotifyPropertyChanged(nameof(Rendering3D_FontIsBold));
			}
		}

		public bool Rendering3D_DrawFontOverlays
		{
			get => _config.Rendering3D_DrawFontOverlays;
			set
			{
				_config.Rendering3D_DrawFontOverlays = value;
				NotifyPropertyChanged(nameof(Rendering3D_DrawFontOverlays));
			}
		}

		public bool Rendering3D_Antialias
		{
			get => _config.Rendering3D_Antialias;
			set
			{
				_config.Rendering3D_Antialias = value;
				NotifyPropertyChanged(nameof(Rendering3D_Antialias));
			}
		}

		public bool Rendering3D_SafeMode
		{
			get => _config.Rendering3D_SafeMode;
			set
			{
				_config.Rendering3D_SafeMode = value;
				NotifyPropertyChanged(nameof(Rendering3D_SafeMode));
			}
		}

		public bool Rendering3D_ResetCameraOnRoomSwitch
		{
			get => _config.Rendering3D_ResetCameraOnRoomSwitch;
			set
			{
				_config.Rendering3D_ResetCameraOnRoomSwitch = value;
				NotifyPropertyChanged(nameof(Rendering3D_ResetCameraOnRoomSwitch));
			}
		}

		public bool Rendering3D_AnimateCameraOnDoubleClickRoomSwitch
		{
			get => _config.Rendering3D_AnimateCameraOnDoubleClickRoomSwitch;
			set
			{
				_config.Rendering3D_AnimateCameraOnDoubleClickRoomSwitch = value;
				NotifyPropertyChanged(nameof(Rendering3D_AnimateCameraOnDoubleClickRoomSwitch));
			}
		}

		public bool Rendering3D_AnimateCameraOnRelocation
		{
			get => _config.Rendering3D_AnimateCameraOnRelocation;
			set
			{
				_config.Rendering3D_AnimateCameraOnRelocation = value;
				NotifyPropertyChanged(nameof(Rendering3D_AnimateCameraOnRelocation));
			}
		}

		public bool Rendering3D_AnimateCameraOnReset
		{
			get => _config.Rendering3D_AnimateCameraOnReset;
			set
			{
				_config.Rendering3D_AnimateCameraOnReset = value;
				NotifyPropertyChanged(nameof(Rendering3D_AnimateCameraOnReset));
			}
		}

		public bool Rendering3D_AnimateGhostBlockUnfolding
		{
			get => _config.Rendering3D_AnimateGhostBlockUnfolding;
			set
			{
				_config.Rendering3D_AnimateGhostBlockUnfolding = value;
				NotifyPropertyChanged(nameof(Rendering3D_AnimateGhostBlockUnfolding));
			}
		}

		public bool Rendering3D_AllowTexturingInLightingMode
		{
			get => _config.Rendering3D_AllowTexturingInLightingMode;
			set
			{
				_config.Rendering3D_AllowTexturingInLightingMode = value;
				NotifyPropertyChanged(nameof(Rendering3D_AllowTexturingInLightingMode));
			}
		}

		public bool Rendering3D_AlwaysShowCurrentRoomBounds
		{
			get => _config.Rendering3D_AlwaysShowCurrentRoomBounds;
			set
			{
				_config.Rendering3D_AlwaysShowCurrentRoomBounds = value;
				NotifyPropertyChanged(nameof(Rendering3D_AlwaysShowCurrentRoomBounds));
			}
		}

		public bool Rendering3D_SelectObjectsInAnyRoom
		{
			get => _config.Rendering3D_SelectObjectsInAnyRoom;
			set
			{
				_config.Rendering3D_SelectObjectsInAnyRoom = value;
				NotifyPropertyChanged(nameof(Rendering3D_SelectObjectsInAnyRoom));
			}
		}

		public bool Rendering3D_AutoswitchCurrentRoom
		{
			get => _config.Rendering3D_AutoswitchCurrentRoom;
			set
			{
				_config.Rendering3D_AutoswitchCurrentRoom = value;
				NotifyPropertyChanged(nameof(Rendering3D_AutoswitchCurrentRoom));
			}
		}

		public bool Rendering3D_AutoBookmarkSelectedObject
		{
			get => _config.Rendering3D_AutoBookmarkSelectedObject;
			set
			{
				_config.Rendering3D_AutoBookmarkSelectedObject = value;
				NotifyPropertyChanged(nameof(Rendering3D_AutoBookmarkSelectedObject));
			}
		}

		public bool Rendering3D_CursorWarping
		{
			get => _config.Rendering3D_CursorWarping;
			set
			{
				_config.Rendering3D_CursorWarping = value;
				NotifyPropertyChanged(nameof(Rendering3D_CursorWarping));
			}
		}

		public int Rendering3D_FlyModeMoveSpeed
		{
			get => _config.Rendering3D_FlyModeMoveSpeed;
			set
			{
				_config.Rendering3D_FlyModeMoveSpeed = value;
				NotifyPropertyChanged(nameof(Rendering3D_FlyModeMoveSpeed));
			}
		}

		public bool Rendering3D_ShowLightRadius
		{
			get => _config.Rendering3D_ShowLightRadius;
			set
			{
				_config.Rendering3D_ShowLightRadius = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowLightRadius));
			}
		}

		public bool Rendering3D_HighQualityLightPreview
		{
			get => _config.Rendering3D_HighQualityLightPreview;
			set
			{
				_config.Rendering3D_HighQualityLightPreview = value;
				NotifyPropertyChanged(nameof(Rendering3D_HighQualityLightPreview));
			}
		}

		public bool Rendering3D_ShowRealTintForObjects
		{
			get => _config.Rendering3D_ShowRealTintForObjects;
			set
			{
				_config.Rendering3D_ShowRealTintForObjects = value;
				NotifyPropertyChanged(nameof(Rendering3D_ShowRealTintForObjects));
			}
		}

		public bool Rendering3D_UseSpritesForServiceObjects
		{
			get => _config.Rendering3D_UseSpritesForServiceObjects;
			set
			{
				_config.Rendering3D_UseSpritesForServiceObjects = value;
				NotifyPropertyChanged(nameof(Rendering3D_UseSpritesForServiceObjects));
			}
		}

		// 2D Map options

		public float Map2D_NavigationMinZoom
		{
			get => _config.Map2D_NavigationMinZoom;
			set
			{
				_config.Map2D_NavigationMinZoom = value;
				NotifyPropertyChanged(nameof(Map2D_NavigationMinZoom));
			}
		}

		public float Map2D_NavigationMaxZoom
		{
			get => _config.Map2D_NavigationMaxZoom;
			set
			{
				_config.Map2D_NavigationMaxZoom = value;
				NotifyPropertyChanged(nameof(Map2D_NavigationMaxZoom));
			}
		}

		public float Map2D_NavigationSpeedMouseWheelZoom
		{
			get => _config.Map2D_NavigationSpeedMouseWheelZoom;
			set
			{
				_config.Map2D_NavigationSpeedMouseWheelZoom = value;
				NotifyPropertyChanged(nameof(Map2D_NavigationSpeedMouseWheelZoom));
			}
		}

		public float Map2D_NavigationSpeedMouseZoom
		{
			get => _config.Map2D_NavigationSpeedMouseZoom;
			set
			{
				_config.Map2D_NavigationSpeedMouseZoom = value;
				NotifyPropertyChanged(nameof(Map2D_NavigationSpeedMouseZoom));
			}
		}

		public float Map2D_NavigationSpeedKeyZoom
		{
			get => _config.Map2D_NavigationSpeedKeyZoom;
			set
			{
				_config.Map2D_NavigationSpeedKeyZoom = value;
				NotifyPropertyChanged(nameof(Map2D_NavigationSpeedKeyZoom));
			}
		}

		public float Map2D_NavigationSpeedKeyMove
		{
			get => _config.Map2D_NavigationSpeedKeyMove;
			set
			{
				_config.Map2D_NavigationSpeedKeyMove = value;
				NotifyPropertyChanged(nameof(Map2D_NavigationSpeedKeyMove));
			}
		}

		public int Map2D_ShowTimes
		{
			get => _config.Map2D_ShowTimes;
			set
			{
				_config.Map2D_ShowTimes = value;
				NotifyPropertyChanged(nameof(Map2D_ShowTimes));
			}
		}

		// Texture map options

		public float TextureMap_NavigationMinZoom
		{
			get => _config.TextureMap_NavigationMinZoom;
			set
			{
				_config.TextureMap_NavigationMinZoom = value;
				NotifyPropertyChanged(nameof(TextureMap_NavigationMinZoom));
			}
		}

		public float TextureMap_NavigationMaxZoom
		{
			get => _config.TextureMap_NavigationMaxZoom;
			set
			{
				_config.TextureMap_NavigationMaxZoom = value;
				NotifyPropertyChanged(nameof(TextureMap_NavigationMaxZoom));
			}
		}

		public float TextureMap_NavigationSpeedMouseWheelZoom
		{
			get => _config.TextureMap_NavigationSpeedMouseWheelZoom;
			set
			{
				_config.TextureMap_NavigationSpeedMouseWheelZoom = value;
				NotifyPropertyChanged(nameof(TextureMap_NavigationSpeedMouseWheelZoom));
			}
		}

		public float TextureMap_NavigationSpeedMouseZoom
		{
			get => _config.TextureMap_NavigationSpeedMouseZoom;
			set
			{
				_config.TextureMap_NavigationSpeedMouseZoom = value;
				NotifyPropertyChanged(nameof(TextureMap_NavigationSpeedMouseZoom));
			}
		}

		public float TextureMap_NavigationSpeedKeyZoom
		{
			get => _config.TextureMap_NavigationSpeedKeyZoom;
			set
			{
				_config.TextureMap_NavigationSpeedKeyZoom = value;
				NotifyPropertyChanged(nameof(TextureMap_NavigationSpeedKeyZoom));
			}
		}

		public float TextureMap_NavigationSpeedKeyMove
		{
			get => _config.TextureMap_NavigationSpeedKeyMove;
			set
			{
				_config.TextureMap_NavigationSpeedKeyMove = value;
				NotifyPropertyChanged(nameof(TextureMap_NavigationSpeedKeyMove));
			}
		}

		public bool TextureMap_DrawSelectionDirectionIndicators
		{
			get => _config.TextureMap_DrawSelectionDirectionIndicators;
			set
			{
				_config.TextureMap_DrawSelectionDirectionIndicators = value;
				NotifyPropertyChanged(nameof(TextureMap_DrawSelectionDirectionIndicators));
			}
		}

		public bool TextureMap_MouseWheelMovesTheTextureInsteadOfZooming
		{
			get => _config.TextureMap_MouseWheelMovesTheTextureInsteadOfZooming;
			set
			{
				_config.TextureMap_MouseWheelMovesTheTextureInsteadOfZooming = value;
				NotifyPropertyChanged(nameof(TextureMap_MouseWheelMovesTheTextureInsteadOfZooming));
			}
		}

		public bool TextureMap_PickTextureWithoutAttributes
		{
			get => _config.TextureMap_PickTextureWithoutAttributes;
			set
			{
				_config.TextureMap_PickTextureWithoutAttributes = value;
				NotifyPropertyChanged(nameof(TextureMap_PickTextureWithoutAttributes));
			}
		}

		public float TextureMap_TileSelectionSize
		{
			get => _config.TextureMap_TileSelectionSize;
			set
			{
				_config.TextureMap_TileSelectionSize = value;
				NotifyPropertyChanged(nameof(TextureMap_TileSelectionSize));
			}
		}

		public bool TextureMap_ResetAttributesOnNewSelection
		{
			get => _config.TextureMap_ResetAttributesOnNewSelection;
			set
			{
				_config.TextureMap_ResetAttributesOnNewSelection = value;
				NotifyPropertyChanged(nameof(TextureMap_ResetAttributesOnNewSelection));
			}
		}

		public bool TextureMap_WarnAboutIncorrectAttributes
		{
			get => _config.TextureMap_WarnAboutIncorrectAttributes;
			set
			{
				_config.TextureMap_WarnAboutIncorrectAttributes = value;
				NotifyPropertyChanged(nameof(TextureMap_WarnAboutIncorrectAttributes));
			}
		}

		// Palette options

		public bool Palette_TextureSamplingMode
		{
			get => _config.Palette_TextureSamplingMode;
			set
			{
				_config.Palette_TextureSamplingMode = value;
				NotifyPropertyChanged(nameof(Palette_TextureSamplingMode));
			}
		}

		public bool Palette_PickColorFromSelectedObject
		{
			get => _config.Palette_PickColorFromSelectedObject;
			set
			{
				_config.Palette_PickColorFromSelectedObject = value;
				NotifyPropertyChanged(nameof(Palette_PickColorFromSelectedObject));
			}
		}

		// Node editor options

		public int NodeEditor_Size
		{
			get => _config.NodeEditor_Size;
			set
			{
				_config.NodeEditor_Size = value;
				NotifyPropertyChanged(nameof(NodeEditor_Size));
			}
		}

		public int NodeEditor_DefaultNodeWidth
		{
			get => _config.NodeEditor_DefaultNodeWidth;
			set
			{
				_config.NodeEditor_DefaultNodeWidth = value;
				NotifyPropertyChanged(nameof(NodeEditor_DefaultNodeWidth));
			}
		}

		public int NodeEditor_GridStep
		{
			get => _config.NodeEditor_GridStep;
			set
			{
				_config.NodeEditor_GridStep = value;
				NotifyPropertyChanged(nameof(NodeEditor_GridStep));
			}
		}

		public bool NodeEditor_LinksAsRopes
		{
			get => _config.NodeEditor_LinksAsRopes;
			set
			{
				_config.NodeEditor_LinksAsRopes = value;
				NotifyPropertyChanged(nameof(NodeEditor_LinksAsRopes));
			}
		}

		public bool NodeEditor_ShowGrips
		{
			get => _config.NodeEditor_ShowGrips;
			set
			{
				_config.NodeEditor_ShowGrips = value;
				NotifyPropertyChanged(nameof(NodeEditor_ShowGrips));
			}
		}

		public int NodeEditor_DefaultEventMode
		{
			get => _config.NodeEditor_DefaultEventMode;
			set
			{
				_config.NodeEditor_DefaultEventMode = value;
				NotifyPropertyChanged(nameof(NodeEditor_DefaultEventMode));
			}
		}

		public int NodeEditor_DefaultEventToEdit
		{
			get => _config.NodeEditor_DefaultEventToEdit;
			set
			{
				_config.NodeEditor_DefaultEventToEdit = value;
				NotifyPropertyChanged(nameof(NodeEditor_DefaultEventToEdit));
			}
		}

		// Gizmo options

		public float Gizmo_Size
		{
			get => _config.Gizmo_Size;
			set
			{
				_config.Gizmo_Size = value;
				NotifyPropertyChanged(nameof(Gizmo_Size));
			}
		}

		public float Gizmo_TranslationConeSize
		{
			get => _config.Gizmo_TranslationConeSize;
			set
			{
				_config.Gizmo_TranslationConeSize = value;
				NotifyPropertyChanged(nameof(Gizmo_TranslationConeSize));
			}
		}

		public float Gizmo_CenterCubeSize
		{
			get => _config.Gizmo_CenterCubeSize;
			set
			{
				_config.Gizmo_CenterCubeSize = value;
				NotifyPropertyChanged(nameof(Gizmo_CenterCubeSize));
			}
		}

		public float Gizmo_ScaleCubeSize
		{
			get => _config.Gizmo_ScaleCubeSize;
			set
			{
				_config.Gizmo_ScaleCubeSize = value;
				NotifyPropertyChanged(nameof(Gizmo_ScaleCubeSize));
			}
		}

		public float Gizmo_LineThickness
		{
			get => _config.Gizmo_LineThickness;
			set
			{
				_config.Gizmo_LineThickness = value;
				NotifyPropertyChanged(nameof(Gizmo_LineThickness));
			}
		}

		// Autosave options

		public bool AutoSave_Enable
		{
			get => _config.AutoSave_Enable;
			set
			{
				_config.AutoSave_Enable = value;
				NotifyPropertyChanged(nameof(AutoSave_Enable));
			}
		}

		public int AutoSave_TimeInSeconds
		{
			get => _config.AutoSave_TimeInSeconds;
			set
			{
				_config.AutoSave_TimeInSeconds = value;
				NotifyPropertyChanged(nameof(AutoSave_TimeInSeconds));
			}
		}

		public string AutoSave_DateTimeFormat
		{
			get => _config.AutoSave_DateTimeFormat;
			set
			{
				_config.AutoSave_DateTimeFormat = value;
				NotifyPropertyChanged(nameof(AutoSave_DateTimeFormat));
			}
		}

		public bool AutoSave_CleanupEnable
		{
			get => _config.AutoSave_CleanupEnable;
			set
			{
				_config.AutoSave_CleanupEnable = value;
				NotifyPropertyChanged(nameof(AutoSave_CleanupEnable));
			}
		}

		public int AutoSave_CleanupMaxAutoSaves
		{
			get => _config.AutoSave_CleanupMaxAutoSaves;
			set
			{
				_config.AutoSave_CleanupMaxAutoSaves = value;
				NotifyPropertyChanged(nameof(AutoSave_CleanupMaxAutoSaves));
			}
		}

		public bool AutoSave_NamePutDateFirst
		{
			get => _config.AutoSave_NamePutDateFirst;
			set
			{
				_config.AutoSave_NamePutDateFirst = value;
				NotifyPropertyChanged(nameof(AutoSave_NamePutDateFirst));
			}
		}

		public string AutoSave_NameSeparator
		{
			get => _config.AutoSave_NameSeparator;
			set
			{
				_config.AutoSave_NameSeparator = value;
				NotifyPropertyChanged(nameof(AutoSave_NameSeparator));
			}
		}

		// User interface options

		public bool UI_ShowStats
		{
			get => _config.UI_ShowStats;
			set
			{
				_config.UI_ShowStats = value;
				NotifyPropertyChanged(nameof(UI_ShowStats));
			}
		}

		public bool UI_AutoFillTriggerTypesForSwitchAndKey
		{
			get => _config.UI_AutoFillTriggerTypesForSwitchAndKey;
			set
			{
				_config.UI_AutoFillTriggerTypesForSwitchAndKey = value;
				NotifyPropertyChanged(nameof(UI_AutoFillTriggerTypesForSwitchAndKey));
			}
		}

		public bool UI_AutoSwitchRoomToOutsideOnAppliedInvisibleTexture
		{
			get => _config.UI_AutoSwitchRoomToOutsideOnAppliedInvisibleTexture;
			set
			{
				_config.UI_AutoSwitchRoomToOutsideOnAppliedInvisibleTexture = value;
				NotifyPropertyChanged(nameof(UI_AutoSwitchRoomToOutsideOnAppliedInvisibleTexture));
			}
		}

		public bool UI_DiscardSelectionOnModeSwitch
		{
			get => _config.UI_DiscardSelectionOnModeSwitch;
			set
			{
				_config.UI_DiscardSelectionOnModeSwitch = value;
				NotifyPropertyChanged(nameof(UI_DiscardSelectionOnModeSwitch));
			}
		}

		public bool UI_ProbeAttributesThroughPortals
		{
			get => _config.UI_ProbeAttributesThroughPortals;
			set
			{
				_config.UI_ProbeAttributesThroughPortals = value;
				NotifyPropertyChanged(nameof(UI_ProbeAttributesThroughPortals));
			}
		}

		public bool UI_SetAttributesAtOnce
		{
			get => _config.UI_SetAttributesAtOnce;
			set
			{
				_config.UI_SetAttributesAtOnce = value;
				NotifyPropertyChanged(nameof(UI_SetAttributesAtOnce));
			}
		}

		public bool UI_OnlyShowSmallMessageWhenRoomIsLocked
		{
			get => _config.UI_OnlyShowSmallMessageWhenRoomIsLocked;
			set
			{
				_config.UI_OnlyShowSmallMessageWhenRoomIsLocked = value;
				NotifyPropertyChanged(nameof(UI_OnlyShowSmallMessageWhenRoomIsLocked));
			}
		}

		public bool UI_WarnBeforeDeletingObjects
		{
			get => _config.UI_WarnBeforeDeletingObjects;
			set
			{
				_config.UI_WarnBeforeDeletingObjects = value;
				NotifyPropertyChanged(nameof(UI_WarnBeforeDeletingObjects));
			}
		}

		public bool UI_GenerateRoomDescriptions
		{
			get => _config.UI_GenerateRoomDescriptions;
			set
			{
				_config.UI_GenerateRoomDescriptions = value;
				NotifyPropertyChanged(nameof(UI_GenerateRoomDescriptions));
			}
		}

		public bool UI_AutoSwitchSectorColoringInfo
		{
			get => _config.UI_AutoSwitchSectorColoringInfo;
			set
			{
				_config.UI_AutoSwitchSectorColoringInfo = value;
				NotifyPropertyChanged(nameof(UI_AutoSwitchSectorColoringInfo));
			}
		}

		public float UI_FormColor_Brightness { get; set; } = 100.0f;
		public string UI_FormColor_ButtonHighlight { get; set; } = ColorTranslator.ToHtml(Color.FromArgb(104, 151, 187));

		public ColorScheme UI_ColorScheme
		{
			get => _config.UI_ColorScheme;
			set
			{
				if (value == null)
					return;

				_config.UI_ColorScheme = value;
				Color2DBackground = value.Color2DBackground;
				ColorSelection = value.ColorSelection;
				ColorIllegalSlope = value.ColorIllegalSlope;
				ColorSlideDirection = value.ColorSlideDirection;
				Color3DBackground = value.Color3DBackground;
				ColorFlipRoom = value.ColorFlipRoom;
				ColorPortal = value.ColorPortal;
				ColorPortalFace = value.ColorPortalFace;
				ColorFloor = value.ColorFloor;
				ColorBorderWall = value.ColorBorderWall;
				ColorWall = value.ColorWall;
				ColorWallLower = value.ColorWallLower;
				ColorWallUpper = value.ColorWallUpper;
				ColorTrigger = value.ColorTrigger;
				ColorMonkey = value.ColorMonkey;
				ColorClimb = value.ColorClimb;
				ColorBox = value.ColorBox;
				ColorDeath = value.ColorDeath;
				ColorNotWalkable = value.ColorNotWalkable;
				ColorBeetle = value.ColorBeetle;
				ColorTriggerTriggerer = value.ColorTriggerTriggerer;
				ColorForceSolidFloor = value.ColorForceSolidFloor;
				Color2DRoomsAbove = value.Color2DRoomsAbove;
				Color2DRoomsBelow = value.Color2DRoomsBelow;
				Color2DRoomsMoved = value.Color2DRoomsMoved;
			}
		}

		public HotkeySets UI_Hotkeys
		{
			get => _config.UI_Hotkeys;
			set
			{
				_config.UI_Hotkeys = value;
				NotifyPropertyChanged(nameof(UI_Hotkeys));
			}
		}

		public IEnumerable<KeyValuePair<string, ColorScheme>> ColorSchemePresets
		{
			get => new Dictionary<string, ColorScheme>()
			{
				{"Default",ColorScheme.Default },
				{"Gray",ColorScheme.Gray },
				{"Dark",ColorScheme.Dark },
				{"Pastel",ColorScheme.Pastel },
			}.ToList();
		}

		public IEnumerable<string> NodeEditorDefaultModes => new ReadOnlyCollection<string>(new string[] { "Level script functions", "Node editor" });
		public IEnumerable<string> NodeEditorDefaultEvents => new ReadOnlyCollection<string>(new string[] { "On enter", "On inside", "On leave" });

		#endregion Properties

		#region ColorProperties

		public Vector4 ColorSelection
		{
			get => _config.UI_ColorScheme.ColorSelection;
			set
			{
				_config.UI_ColorScheme.ColorSelection = value;
				NotifyPropertyChanged(nameof(ColorSelection));
			}
		}

		public Vector4 ColorIllegalSlope
		{
			get => _config.UI_ColorScheme.ColorIllegalSlope;
			set
			{
				_config.UI_ColorScheme.ColorIllegalSlope = value;
				NotifyPropertyChanged(nameof(ColorIllegalSlope));
			}
		}

		public Vector4 ColorSlideDirection
		{
			get => _config.UI_ColorScheme.ColorSlideDirection;
			set
			{
				_config.UI_ColorScheme.ColorSlideDirection = value;
				NotifyPropertyChanged(nameof(ColorSlideDirection));
			}
		}

		public Vector4 Color3DBackground
		{
			get => _config.UI_ColorScheme.Color3DBackground;
			set
			{
				_config.UI_ColorScheme.Color3DBackground = value;
				NotifyPropertyChanged(nameof(Color3DBackground));
			}
		}

		public Vector4 Color2DBackground
		{
			get => _config.UI_ColorScheme.Color2DBackground;
			set
			{
				_config.UI_ColorScheme.Color2DBackground = value;
				NotifyPropertyChanged(nameof(Color2DBackground));
			}
		}

		public Vector4 ColorFlipRoom
		{
			get => _config.UI_ColorScheme.ColorFlipRoom;
			set
			{
				_config.UI_ColorScheme.ColorFlipRoom = value;
				NotifyPropertyChanged(nameof(ColorFlipRoom));
			}
		}

		public Vector4 ColorPortal
		{
			get => _config.UI_ColorScheme.ColorPortal;
			set
			{
				_config.UI_ColorScheme.ColorPortal = value;
				NotifyPropertyChanged(nameof(ColorPortal));
			}
		}

		public Vector4 ColorPortalFace
		{
			get => _config.UI_ColorScheme.ColorPortalFace;
			set
			{
				_config.UI_ColorScheme.ColorPortalFace = value;
				NotifyPropertyChanged(nameof(ColorPortalFace));
			}
		}

		public Vector4 ColorFloor
		{
			get => _config.UI_ColorScheme.ColorFloor;
			set
			{
				_config.UI_ColorScheme.ColorFloor = value;
				NotifyPropertyChanged(nameof(ColorFloor));
			}
		}

		public Vector4 ColorBorderWall
		{
			get => _config.UI_ColorScheme.ColorBorderWall;
			set
			{
				_config.UI_ColorScheme.ColorBorderWall = value;
				NotifyPropertyChanged(nameof(ColorBorderWall));
			}
		}

		public Vector4 ColorWall
		{
			get => _config.UI_ColorScheme.ColorWall;
			set
			{
				_config.UI_ColorScheme.ColorWall = value;
				NotifyPropertyChanged(nameof(ColorWall));
			}
		}

		public Vector4 ColorWallLower
		{
			get => _config.UI_ColorScheme.ColorWallLower;
			set
			{
				_config.UI_ColorScheme.ColorWallLower = value;
				NotifyPropertyChanged(nameof(ColorWallLower));
			}
		}

		public Vector4 ColorWallUpper
		{
			get => _config.UI_ColorScheme.ColorWallUpper;
			set
			{
				_config.UI_ColorScheme.ColorWallUpper = value;
				NotifyPropertyChanged(nameof(ColorWallUpper));
			}
		}

		public Vector4 ColorTrigger
		{
			get => _config.UI_ColorScheme.ColorTrigger;
			set
			{
				_config.UI_ColorScheme.ColorTrigger = value;
				NotifyPropertyChanged(nameof(ColorTrigger));
			}
		}

		public Vector4 ColorMonkey
		{
			get => _config.UI_ColorScheme.ColorMonkey;
			set
			{
				_config.UI_ColorScheme.ColorMonkey = value;
				NotifyPropertyChanged(nameof(ColorMonkey));
			}
		}

		public Vector4 ColorClimb
		{
			get => _config.UI_ColorScheme.ColorClimb;
			set
			{
				_config.UI_ColorScheme.ColorClimb = value;
				NotifyPropertyChanged(nameof(ColorClimb));
			}
		}

		public Vector4 ColorBox
		{
			get => _config.UI_ColorScheme.ColorBox;
			set
			{
				_config.UI_ColorScheme.ColorBox = value;
				NotifyPropertyChanged(nameof(ColorBox));
			}
		}

		public Vector4 ColorDeath
		{
			get => _config.UI_ColorScheme.ColorDeath;
			set
			{
				_config.UI_ColorScheme.ColorDeath = value;
				NotifyPropertyChanged(nameof(ColorDeath));
			}
		}

		public Vector4 ColorNotWalkable
		{
			get => _config.UI_ColorScheme.ColorNotWalkable;
			set
			{
				_config.UI_ColorScheme.ColorNotWalkable = value;
				NotifyPropertyChanged(nameof(ColorNotWalkable));
			}
		}

		public Vector4 ColorBeetle
		{
			get => _config.UI_ColorScheme.ColorBeetle;
			set
			{
				_config.UI_ColorScheme.ColorBeetle = value;
				NotifyPropertyChanged(nameof(ColorBeetle));
			}
		}

		public Vector4 ColorTriggerTriggerer
		{
			get => _config.UI_ColorScheme.ColorTriggerTriggerer;
			set
			{
				_config.UI_ColorScheme.ColorTriggerTriggerer = value;
				NotifyPropertyChanged(nameof(ColorTriggerTriggerer));
			}
		}

		public Vector4 ColorForceSolidFloor
		{
			get => _config.UI_ColorScheme.ColorForceSolidFloor;
			set
			{
				_config.UI_ColorScheme.ColorForceSolidFloor = value;
				NotifyPropertyChanged(nameof(ColorForceSolidFloor));
			}
		}

		public Vector4 Color2DRoomsAbove
		{
			get => _config.UI_ColorScheme.Color2DRoomsAbove;
			set
			{
				_config.UI_ColorScheme.Color2DRoomsAbove = value;
				NotifyPropertyChanged(nameof(Color2DRoomsAbove));
			}
		}

		public Vector4 Color2DRoomsBelow
		{
			get => _config.UI_ColorScheme.Color2DRoomsBelow;
			set
			{
				_config.UI_ColorScheme.Color2DRoomsBelow = value;
				NotifyPropertyChanged(nameof(Color2DRoomsBelow));
			}
		}

		public Vector4 Color2DRoomsMoved
		{
			get => _config.UI_ColorScheme.Color2DRoomsMoved;
			set
			{
				_config.UI_ColorScheme.Color2DRoomsMoved = value;
				NotifyPropertyChanged(nameof(Color2DRoomsMoved));
			}
		}

		#endregion ColorProperties

		public OptionsViewModel(Configuration config)
		{
			_config = config;
		}
	}
}
