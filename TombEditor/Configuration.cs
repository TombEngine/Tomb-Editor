using DarkUI.Docking;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using TombLib;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Rendering;

namespace TombEditor
{
    // Just add properties to this class to add now configuration options.
    // They will be loaded and saved automatically.
    public class Configuration : ConfigurationBase
    {
        public override string ConfigName { get { return "TombEditorConfiguration.xml"; } }

        // Global editor options

        public int Editor_UndoDepth { get; set; } = 20;
        public bool Editor_ReloadFilesAutomaticallyWhenChanged { get; set; } = true;
        public bool Editor_OpenLastProjectOnStartup { get; set; } = false;
        public bool Editor_AllowMultipleInstances { get; set; } = false;

        // Defaults

        public TRVersion.Game Editor_DefaultProjectGameVersion { get; set; } = TRVersion.Game.TRNG;
        public int Editor_DefaultNewRoomSize { get; set; } = 20;
        public bool Editor_GridNewRoom { get; set; } = false;
        public bool Editor_UseHalfPixelCorrectionOnPrjImport { get; set; } = false;
        public bool Editor_RespectFlybyPatchOnPrjImport { get; set; } = true;
        public bool Editor_EnableStepHeightControlsForUnsupportedEngines { get; set; } = false;
        public int Editor_StepHeight { get; set; } = 256;

        // Item preview options

        public float RenderingItem_NavigationSpeedMouseWheelZoom { get; set; } = 6.0f;
        public float RenderingItem_NavigationSpeedMouseZoom { get; set; } = 300.0f;
        public float RenderingItem_NavigationSpeedMouseTranslate { get; set; } = 200.0f;
        public float RenderingItem_NavigationSpeedMouseRotate { get; set; } = 4.0f;
        public float RenderingItem_FieldOfView { get; set; } = 50.0f;
        public bool RenderingItem_Antialias { get; set; } = false;
        public bool RenderingItem_HideInternalObjects { get; set; } = false;
        public bool RenderingItem_ShowMultipleWadsPrompt { get; set; } = true;
        public bool RenderingItem_Animate { get; set; } = false;

        // Main 3D window options

        public int Rendering3D_DrawRoomsMaxDepth { get; set; } = 6;
        public float Rendering3D_NavigationSpeedKeyRotate { get; set; } = 0.30f;
        public float Rendering3D_NavigationSpeedKeyZoom { get; set; } = 3000.0f;
        public float Rendering3D_NavigationSpeedMouseWheelZoom { get; set; } = 30.0f;
        public float Rendering3D_NavigationSpeedMouseZoom { get; set; } = 72000.0f;
        public float Rendering3D_NavigationSpeedMouseTranslate { get; set; } = 22000.0f;
        public float Rendering3D_NavigationSpeedMouseRotate { get; set; } = 3.0f;
        public float Rendering3D_DragMouseSensitivity { get; set; } = 0.05f;
        public bool Rendering3D_InvertMouseZoom { get; set; } = false;
        public float Rendering3D_LineWidth { get; set; } = 10.0f;
        public float Rendering3D_FieldOfView { get; set; } = 50.0f;
        public bool Rendering3D_ToolboxVisible { get; set; } = true;
        public Point Rendering3D_ToolboxPosition { get; set; } = new Point(15, 45);
        public bool Rendering3D_DisablePickingForImportedGeometry { get; set; } = false;
        public bool Rendering3D_DisablePickingForHiddenRooms { get; set; } = false;
        public bool Rendering3D_ShowPortals { get; set; } = false;
        public bool Rendering3D_ShowHorizon { get; set; } = false;
        public bool Rendering3D_ShowAllRooms { get; set; } = false;
        public bool Rendering3D_ShowIllegalSlopes { get; set; } = false;
        public bool Rendering3D_ShowMoveables { get; set; } = true;
        public bool Rendering3D_ShowStatics { get; set; } = true;
        public bool Rendering3D_ShowImportedGeometry { get; set; } = true;
        public bool Rendering3D_ShowGhostBlocks { get; set; } = true;
        public bool Rendering3D_ShowVolumes { get; set; } = true;
        public bool Rendering3D_ShowBoundingBoxes { get; set; } = false;
        public bool Rendering3D_ShowOtherObjects { get; set; } = true;
        public bool Rendering3D_ShowSlideDirections { get; set; } = false;
        public bool Rendering3D_ShowFPS { get; set; } = false;
        public bool Rendering3D_ShowRoomNames { get; set; } = false;
        public bool Rendering3D_ShowCardinalDirections { get; set; } = false;
        public bool Rendering3D_UseRoomEditorDirections { get; set; } = false;
        public bool Rendering3D_ShowExtraBlendingModes { get; set; } = true;
        public bool Rendering3D_HideTransparentFaces { get; set; } = true;
        public bool Rendering3D_BilinearFilter { get; set; } = true;
        public bool Rendering3D_ShowLightingWhiteTextureOnly { get; set; } = false;
        public string Rendering3D_FontName { get; set; } = "Segoe UI";
        public int Rendering3D_FontSize { get; set; } = 16;
        public bool Rendering3D_FontIsBold { get; set; } = true;
        public bool Rendering3D_DrawFontOverlays { get; set; } = true;
        public bool Rendering3D_Antialias { get; set; } = true;
        public bool Rendering3D_SafeMode { get; set; } = false;
        public bool Rendering3D_ResetCameraOnRoomSwitch { get; set; } = true;
        public bool Rendering3D_AnimateCameraOnDoubleClickRoomSwitch { get; set; } = true;
        public bool Rendering3D_AnimateCameraOnRelocation { get; set; } = true;
        public bool Rendering3D_AnimateCameraOnReset { get; set; } = true;
        public bool Rendering3D_AnimateGhostBlockUnfolding { get; set; } = true;
        public bool Rendering3D_AllowTexturingInLightingMode { get; set; } = true;
        public bool Rendering3D_AlwaysShowCurrentRoomBounds { get; set; } = false;
        public bool Rendering3D_SelectObjectsInAnyRoom { get; set; } = true;
        public bool Rendering3D_AutoswitchCurrentRoom { get; set; } = false;
        public bool Rendering3D_AutoBookmarkSelectedObject { get; set; } = false;
        public bool Rendering3D_CursorWarping { get; set; } = true;
        public int Rendering3D_FlyModeMoveSpeed { get; set; } = 5;
        public bool Rendering3D_ShowLightRadius { get; set; } = true;
        public bool Rendering3D_HighQualityLightPreview { get; set; } = false;
        public bool Rendering3D_ShowRealTintForObjects { get; set; } = true;
        public bool Rendering3D_UseSpritesForServiceObjects { get; set; } = true;
        public int Rendering3D_ObjectQuality { get; set; } = 0; // 0 = High, 1 = Medium, 2 = Low

        // 2D Map options

        public float Map2D_NavigationMinZoom { get; set; } = 0.04f;
        public float Map2D_NavigationMaxZoom { get; set; } = 500.0f;
        public float Map2D_NavigationSpeedMouseWheelZoom { get; set; } = 0.001f;
        public float Map2D_NavigationSpeedMouseZoom { get; set; } = 7.5f;
        public float Map2D_NavigationSpeedKeyZoom { get; set; } = 0.17f;
        public float Map2D_NavigationSpeedKeyMove { get; set; } = 107.0f;
        public int Map2D_ShowTimes { get; set; } = 0;

        // Texture map options

        public float TextureMap_NavigationMinZoom { get; set; } = 0.02f;
        public float TextureMap_NavigationMaxZoom { get; set; } = 2000.0f;
        public float TextureMap_NavigationSpeedMouseWheelZoom { get; set; } = 0.0015f;
        public float TextureMap_NavigationSpeedMouseZoom { get; set; } = 7.5f;
        public float TextureMap_NavigationSpeedKeyZoom { get; set; } = 0.17f;
        public float TextureMap_NavigationSpeedKeyMove { get; set; } = 107.0f;
        public bool TextureMap_DrawSelectionDirectionIndicators { get; set; } = true;
        public bool TextureMap_MouseWheelMovesTheTextureInsteadOfZooming { get; set; } = false;
        public bool TextureMap_PickTextureWithoutAttributes { get; set; } = false;
        public float TextureMap_TileSelectionSize { get; set; } = 64.0f;
        public bool TextureMap_ResetAttributesOnNewSelection { get; set; } = false;
        public bool TextureMap_WarnAboutIncorrectAttributes { get; set; } = true;

        // Palette options

        public bool Palette_TextureSamplingMode { get; set; } = false;
        public bool Palette_PickColorFromSelectedObject { get; set; } = false;

        // Node editor options

        public int NodeEditor_Size { get; set; } = 256;
        public int NodeEditor_DefaultNodeWidth { get; set; } = TriggerNode.DefaultSize;
        public int NodeEditor_GridStep { get; set; } = 8;
        public bool NodeEditor_LinksAsRopes { get; set; } = true;
        public bool NodeEditor_ShowGrips { get; set; } = true;
        public int NodeEditor_DefaultEventMode { get; set; } = 1;
        public int NodeEditor_DefaultEventToEdit { get; set; } = 0;
        public int NodeEditor_DefaultGlobalEventToEdit { get; set; } = 0;

        // Gizmo options

        public float Gizmo_Size { get; set; } = 1536.0f;
        public float Gizmo_TranslationConeSize { get; set; } = 220.0f;
        public float Gizmo_CenterCubeSize { get; set; } = 128.0f;
        public float Gizmo_ScaleCubeSize { get; set; } = 128.0f;
        public float Gizmo_LineThickness { get; set; } = 45.0f;

        // Autosave options

        public bool AutoSave_Enable { get; set; } = true;
        public int AutoSave_TimeInSeconds { get; set; } = 500;
        public string AutoSave_DateTimeFormat { get; set; } = "yyyy-MM-dd HH-mm";
        public bool AutoSave_CleanupEnable { get; set; } = true;
        public int AutoSave_CleanupMaxAutoSaves { get; set; } = 10;
        public bool AutoSave_NamePutDateFirst { get; set; } = true;
        public string AutoSave_NameSeparator { get; set; } = " ";

        // User interface options

        public bool UI_ShowStats { get; set; } = true;
        public bool UI_AutoFillTriggerTypesForSwitchAndKey { get; set; } = true;
        public bool UI_AutoSwitchRoomToOutsideOnAppliedInvisibleTexture { get; set; } = false;
        public bool UI_DiscardSelectionOnModeSwitch { get; set; } = false;
        public bool UI_ProbeAttributesThroughPortals { get; set; } = true;
        public bool UI_SetAttributesAtOnce { get; set; } = true;
        public bool UI_OnlyShowSmallMessageWhenRoomIsLocked { get; set; } = false;
        public bool UI_WarnBeforeDeletingObjects { get; set; } = true;
        public bool UI_GenerateRoomDescriptions { get; set; } = true;
        public bool UI_AutoSwitchSectorColoringInfo { get; set; } = true;
        public float UI_FormColor_Brightness { get; set; } = 100.0f;
        public string UI_FormColor_ButtonHighlight { get; set; } = ColorTranslator.ToHtml(Color.FromArgb(104, 151, 187));
        public ColorScheme UI_ColorScheme { get; set; } = ColorScheme.Default;
        public HotkeySets UI_Hotkeys { get; set; } = new HotkeySets();

        // Toolbar button order

        public string[] UI_ToolbarButtons { get; set; } = new string[]
        {
            "2D", "3D", "FaceEdit", "LightingMode", "DrawWhiteLighting", "|",
            "Undo", "Redo", "|",
            "CenterCamera", "ToggleFlyMode", "|",
            "DrawPortals", "DrawAllRooms", "DrawHorizon",
            "DrawRoomNames", "DrawCardinalDirections",
            "DrawExtraBlendingModes", "HideTransparentFaces", "BilinearFilter", "DrawStaticTint",
            "DrawIllegalSlopes", "DrawSlideDirections", "DisableHiddenRoomPicking", "DisableGeometryPicking", "DrawObjects", "|",
            "FlipMap", "|",
            "Copy", "Paste", "Stamp", "|",
            "OpacityNone", "OpacitySolidFaces", "OpacityTraversableFaces", "Mirror","|",
            "AddCamera", "AddFlybyCamera", "AddSink", "AddSoundSource", "AddImportedGeometry", "AddGhostBlock", "AddMemo", "|",
            "CompileLevel", "CompileLevelAndPlay", "CompileAndPlayPreview"
        };

        // Last used tools

        public EditorTool UI_LastGeometryTool { get; set; } = new EditorTool();
        public EditorTool UI_LastTexturingTool { get; set; } = new EditorTool() { Tool = EditorToolType.Brush };

        // Geometry IO Window

        public string GeometryIO_LastUsedGeometryImportPresetName { get; set; }
        public string GeometryIO_LastUsedGeometryExportPresetName { get; set; }
        public string GeometryIO_LastUsedAnimationPresetName { get; set; }

        // Window options

        public Point ColorDialog_Position { get; set; } = new Point(-1); // Center by default
        public Point Window_FormMain_Position { get; set; } = new Point(-1); // Center by default
        public Size  Window_FormMain_Size { get; set; } = Window_SizeDefault;
        public bool  Window_FormMain_Maximized { get; set; } = true;
        public Point Window_FormAnimatedTextures_Position { get; set; } = new Point(-1); // Center by default
        public Size  Window_FormAnimatedTextures_Size { get; set; } = new Size(885, 694);
        public bool  Window_FormAnimatedTextures_Maximized { get; set; } = false;
        public Point Window_FormBumpMaps_Position { get; set; } = new Point(-1); // Center by default
        public Size  Window_FormBumpMaps_Size { get; set; } = new Size(440, 600);
        public bool  Window_FormBumpMaps_Maximized { get; set; } = false;
        public Point Window_FormFootStepSounds_Position { get; set; } = new Point(-1); // Center by default
        public Size  Window_FormFootStepSounds_Size { get; set; } = new Size(440, 600);
        public bool  Window_FormFootStepSounds_Maximized { get; set; } = false;
        public Point Window_FormImportedGeometry_Position { get; set; } = new Point(-1); // Center by default
        public Size  Window_FormImportedGeometry_Size { get; set; } = new Size(830, 488);
        public bool  Window_FormImportedGeometry_Maximized { get; set; } = false;
        public Point Window_FormLevelSettings_Position { get; set; } = new Point(-1); // Center by default
        public Size  Window_FormLevelSettings_Size { get; set; } = new Size(800, 540);
        public bool  Window_FormLevelSettings_Maximized { get; set; } = false;
        public Point Window_FormSearch_Position { get; set; } = new Point(-1); // Center by default
        public Size  Window_FormSearch_Size { get; set; } = new Size(650, 350);
        public bool  Window_FormSearch_Maximized { get; set; } = false;
        public Point Window_FormSprite_Position { get; set; } = new Point(-1); // Center by default
        public Size Window_FormSprite_Size { get; set; } = new Size(350, 370);
        public bool Window_FormSprite_Maximized { get; set; } = false;
        public Point Window_FormMoveable_Position { get; set; } = new Point(-1); // Center by default
        public Size Window_FormMoveable_Size { get; set; } = new Size(232, 254);
        public bool Window_FormMoveable_Maximized { get; set; } = false;
        public Point Window_FormStatic_Position { get; set; } = new Point(-1); // Center by default
        public Size Window_FormStatic_Size { get; set; } = new Size(237, 418);
        public bool Window_FormStatic_Maximized { get; set; } = false;
        public Point Window_FormReplaceObject_Position { get; set; } = new Point(-1); // Center by default
        public Size  Window_FormReplaceObject_Size { get; set; } = new Size(600, 264);
        public bool Window_FormReplaceObject_Maximized { get; set; } = false;
        public Point Window_FormRoomProperties_Position { get; set; } = new Point(-1); // Center by default
        public Size Window_FormRoomProperties_Size { get; set; } = new Size(280, 450);
        public bool Window_FormRoomProperties_Maximized { get; set; } = false;
        public Point Window_FormTextureRemap_Position { get; set; } = new Point(-1); // Center by default
        public Size Window_FormTextureRemap_Size { get; set; } = new Size(710, 730);
        public bool Window_FormTextureRemap_Maximized { get; set; } = false;
        public Point Window_FormTrigger_Position { get; set; } = new Point(-1); // Center by default
        public Size  Window_FormTrigger_Size { get; set; } = new Size(); // Depends on target
        public bool  Window_FormTrigger_Maximized { get; set; } = false;
        public Point Window_FormFindTextures_Position { get; set; } = new Point(-1); // Center by default
        public Size Window_FormFindTextures_Size { get; set; } = new Size(330, 450);
        public bool Window_FormFindTextures_Maximized { get; set; } = false;
        public Point Window_FormMemo_Position { get; set; } = new Point(-1); // Center by default
        public Size Window_FormMemo_Size { get; set; } = new Size(350, 200);
        public bool Window_FormMemo_Maximized { get; set; } = false;
        public Point Window_FormEventSetEditor_Position { get; set; } = new Point(-1); // Center by default
        public Size Window_FormEventSetEditor_Size { get; set; } = new Size(930, 460);
        public bool Window_FormEventSetEditor_Maximized { get; set; } = false;
        public int Window_FormEventSetEditor_SplitterDistance { get; set; } = 250;
        public Point Window_FormTransform_Position { get; set; } = new Point(-1); // Center by default
        public Size Window_FormTransform_Size { get; set; } = new Size(345, 171);
        public bool Window_FormTransform_Maximized { get; set; } = false;
        public Point Window_FormMaterialEditor_Position { get; set; } = new Point(-1);
        public Size Window_FormMaterialEditor_Size { get; set; } = new Size(537, 560);
        public bool Window_FormMaterialEditor_Maximized { get; set; } = false;

        public DockPanelState Window_Layout { get; set; } = Window_LayoutDefault;

        public void EnsureDefaults()
        {
            foreach (var field in UI_ColorScheme.GetType().GetFields())
            {
                if (field.FieldType != typeof(Vector4))
                    continue;

                var value = (Vector4)field.GetValue(UI_ColorScheme);
                if (value.W != 0)
                    continue;

                var defaultValue = (Vector4)field.GetValue(ColorScheme.Default);
                field.SetValue(UI_ColorScheme, defaultValue);
            }
        }

        public static readonly Size Window_SizeDefault = new Size(1212, 763);
        public static readonly DockPanelState Window_LayoutDefault = new DockPanelState
        {
            Regions = new List<DockRegionState>
            {
                new DockRegionState
                {
                    Area = DarkDockArea.Document,
                    Size = new Size(0, 0),
                    Groups = new List<DockGroupState>
                    {
                        new DockGroupState
                        {
                            Contents = new List<string> { "MainView" },
                            VisibleContent = "MainView",
                            Order = 0,
                            Size = new Size(0 ,0)
                        }
                    }
                },
                new DockRegionState
                {
                    Area = DarkDockArea.Left,
                    Size = new Size(286, 893),
                    Groups = new List<DockGroupState>
                    {
                        new DockGroupState
                        {
                            Contents = new List<string> { "SectorOptions" },
                            VisibleContent = "SectorOptions",
                            Order = 0,
                            Size = new Size(284,280)
                        },
                        //new DockGroupState
                        //{
                        //    Contents = new List<string> { "ToolPalette" },
                        //    VisibleContent = "ToolPalette",
                        //    Order = 0,
                        //    Size = new Size(284,52)
                        //},
                        new DockGroupState
                        {
                            Contents = new List<string> { "RoomOptions" },
                            VisibleContent = "RoomOptions",
                            Order = 1,
                            Size = new Size(284,226)
                        },
                        new DockGroupState
                        {
                            Contents = new List<string> { "ItemBrowser", "ImportedGeometryBrowser" },
                            VisibleContent = "ItemBrowser",
                            Order = 2,
                            Size = new Size(284,259)
                        },
                        new DockGroupState
                        {
                            Contents = new List<string> { "TriggerList", "ObjectList" },
                            VisibleContent = "TriggerList",
                            Order = 3,
                            Size = new Size(284,174)
                        }
                    }
                },
                new DockRegionState
                {
                    Area = DarkDockArea.Right,
                    Size = new Size(286, 0),
                    Groups = new List<DockGroupState>
                    {
                        new DockGroupState
                        {
                            Contents = new List<string> { "TexturePanel" },
                            VisibleContent = "TexturePanel",
                            Order = 0,
                            Size = new Size(286,700)
                        }
                    }
                },
                new DockRegionState
                {
                    Area = DarkDockArea.Bottom,
                    Size = new Size(1007, 128),
                    Groups = new List<DockGroupState>
                    {
                        new DockGroupState
                        {
                            Contents = new List<string> { "Lighting" },
                            VisibleContent = "Lighting",
                            Order = 0,
                            Size = new Size(371,141)
                        },
                        new DockGroupState
                        {
                            Contents = new List<string> { "Palette" },
                            VisibleContent = "Palette",
                            Order = 1,
                            Size = new Size(645,141)
                        }
                    }
                }
            }
        };
    }
}
