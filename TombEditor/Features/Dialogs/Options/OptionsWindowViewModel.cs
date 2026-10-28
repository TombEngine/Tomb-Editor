#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using WpfColor = System.Windows.Media.Color;

namespace TombEditor.Features.Dialogs.Options
{
    /// <summary>
    /// WPF port of <c>FormOptions</c> / <c>FormOptionsBase</c>. The dialog is data-driven: a schema of
    /// tabs -> groups -> <see cref="OptionItem"/>s is rendered by the view, and values are read from /
    /// written to <see cref="Configuration"/> by reflection on each option's <see cref="OptionItem.ConfigName"/>
    /// (the same convention the previous control <c>Tag</c> used), including the <c>UI_ColorScheme</c>
    /// sub-fields for the color options.
    /// </summary>
    public partial class OptionsWindowViewModel : ObservableObject, IModalDialogViewModel
    {
        private const string ColorSchemeProperty = "UI_ColorScheme";

        private readonly Editor _editor;
        private readonly object _config;
        private readonly IColorPickerService _colorPickerService;
        private readonly ILocalizationService _localizationService;

        public ObservableCollection<OptionTab> Tabs { get; } = new();

        [ObservableProperty] private OptionTab? _selectedTab;
        [ObservableProperty] private bool? _dialogResult;
        [ObservableProperty] private string _searchText = string.Empty;

        public OptionsWindowViewModel(Editor editor, IColorPickerService? colorPickerService = null, ILocalizationService? localizationService = null)
        {
            _editor = editor;
            _config = editor.Configuration;
            _colorPickerService = ServiceLocator.ResolveService(colorPickerService);
            _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

            BuildSchema();
            ReadConfigIntoItems(_config);
            SelectedTab = Tabs.FirstOrDefault();
        }

        // Commands.

        [RelayCommand]
        private void Apply()
        {
            WriteConfigFromItems();
            _editor.ConfigurationChange();
        }

        [RelayCommand]
        private void Ok()
        {
            WriteConfigFromItems();
            _editor.ConfigurationChange();
            DialogResult = true;
        }

        [RelayCommand]
        private void Cancel() => DialogResult = false;

        [RelayCommand]
        private void PickColor(OptionItem item)
        {
            var current = item.Value is WpfColor c ? c : WpfColor.FromRgb(0, 0, 0);

            Vector3? picked = _colorPickerService.PickColor(current.ToFloat3Color());
            if (picked.HasValue)
                item.Value = picked.Value.ToWPFColor();
        }

        [RelayCommand]
        private void PageDefaults()
        {
            if (SelectedTab is null)
                return;

            var defaults = Activator.CreateInstance(_config.GetType())!;
            ReadConfigIntoItems(defaults, SelectedTab);
        }

        // Reflection get / set (mirrors FormOptionsBase, with the UI_ColorScheme field fallback).

        private object? GetOption(object config, string name)
        {
            var prop = config.GetType().GetProperty(name);
            if (prop != null)
                return prop.GetValue(config);

            var scheme = config.GetType().GetProperty(ColorSchemeProperty)?.GetValue(config);
            return scheme?.GetType().GetField(name)?.GetValue(scheme);
        }

        private void SetOption(string name, object? value)
        {
            var prop = _config.GetType().GetProperty(name);
            if (prop != null)
            {
                prop.SetValue(_config, value);
                return;
            }

            var scheme = _config.GetType().GetProperty(ColorSchemeProperty)?.GetValue(_config);
            scheme?.GetType().GetField(name)?.SetValue(scheme, value);
        }

        private void ReadConfigIntoItems(object config, OptionTab? onlyTab = null)
        {
            var items = (onlyTab?.AllItems ?? Tabs.SelectMany(t => t.AllItems));

            foreach (var item in items)
            {
                if (item.Kind == OptionKind.ColorSchemePreset)
                    continue;

                var option = GetOption(config, item.ConfigName);
                if (option == null)
                    continue;

                switch (option)
                {
                    case bool b:
                        item.Value = b;
                        break;
                    case string s:
                        item.Value = item.Kind == OptionKind.Color ? (object)ParseHtmlColor(s) : s;
                        break;
                    case TRVersion.Game game:
                        item.Value = game;
                        break;
                    case int i:
                        item.Value = item.Kind == OptionKind.Combo ? ItemAt(item, i) : (object)(double)i;
                        break;
                    case float f:
                        item.Value = (double)f;
                        break;
                    case Vector4 v:
                        item.Value = v.ToWPFColor();
                        break;
                }
            }
        }

        private void WriteConfigFromItems()
        {
            foreach (var item in Tabs.SelectMany(t => t.AllItems))
            {
                if (item.Kind == OptionKind.ColorSchemePreset)
                    continue;

                var option = GetOption(_config, item.ConfigName);
                if (option == null)
                    continue;

                switch (option)
                {
                    case bool when item.Value is bool b:
                        SetOption(item.ConfigName, b);
                        break;
                    case string when item.Kind == OptionKind.Color && item.Value is WpfColor color:
                        SetOption(item.ConfigName, ToHtmlColor(color));
                        break;
                    case string when item.Value is string s:
                        SetOption(item.ConfigName, s);
                        break;
                    case TRVersion.Game when item.Value is TRVersion.Game game:
                        SetOption(item.ConfigName, game);
                        break;
                    case int when item.Kind == OptionKind.Combo:
                        SetOption(item.ConfigName, IndexOf(item, item.Value));
                        break;
                    case int when item.Value is double d:
                        SetOption(item.ConfigName, (int)d);
                        break;
                    case float when item.Value is double d:
                        SetOption(item.ConfigName, (float)d);
                        break;
                    case Vector4 original when item.Value is WpfColor color:
                        SetOption(item.ConfigName, new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, original.W));
                        break;
                }
            }
        }

        // Color scheme presets.

        private void ApplyColorSchemePreset(string presetName)
        {
            var scheme = _config.GetType().GetProperty(ColorSchemeProperty)?.GetValue(_config);
            if (scheme is null)
                return;

            var presetField = scheme.GetType().GetFields()
                .FirstOrDefault(f => f.FieldType == scheme.GetType() && f.Name == presetName);
            if (presetField is null)
                return;

            var preset = presetField.GetValue(null);
            if (preset is null)
                return;

            foreach (var item in Tabs.SelectMany(t => t.AllItems).Where(o => o.Kind == OptionKind.Color))
            {
                var field = preset.GetType().GetField(item.ConfigName);
                if (field != null && field.GetValue(preset) is Vector4 v)
                    item.Value = v.ToWPFColor();
            }
        }

        // Search filtering.

        partial void OnSearchTextChanged(string value) => ApplyFilter();

        private void ApplyFilter()
        {
            string query = SearchText.Trim();

            foreach (var tab in Tabs)
            {
                bool anyTab = false;
                foreach (var group in tab.Groups)
                {
                    bool anyGroup = false;
                    foreach (var item in group.Items)
                    {
                        bool match = query.Length == 0 || item.Label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
                        item.IsVisible = match;
                        anyGroup |= match;
                    }
                    group.IsVisible = anyGroup;
                    anyTab |= anyGroup;
                }
                tab.IsVisible = anyTab;
            }
        }

        private static object? ItemAt(OptionItem item, int index)
            => item.Items != null && index >= 0 && index < item.Items.Count ? item.Items[index] : null;

        private static int IndexOf(OptionItem item, object? value)
        {
            if (item.Items == null || value == null)
                return 0;
            int index = item.Items.ToList().IndexOf(value);
            return index < 0 ? 0 : index;
        }

        private static WpfColor ParseHtmlColor(string html)
        {
            try
            {
                var c = ColorTranslator.FromHtml(html);
                return WpfColor.FromRgb(c.R, c.G, c.B);
            }
            catch
            {
                return WpfColor.FromRgb(0, 0, 0);
            }
        }

        private static string ToHtmlColor(WpfColor color)
            => ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(color.R, color.G, color.B));

        // Schema.

        private void BuildSchema()
        {
            var gameVersions = TRVersion.AllVersions.Cast<object>().ToList();
            var fonts = System.Drawing.FontFamily.Families.Select(f => (object)f.Name).ToList();
            var objectQuality = new List<object> { _localizationService["ComboHigh"], _localizationService["ComboMedium"], _localizationService["ComboLow"] };
            var globalEvents = Event.GlobalEventTypes.Select(e => (object)e.ToString().SplitCamelcase()).ToList();
            var volumeEvents = Event.VolumeEventTypes.Select(e => (object)e.ToString().SplitCamelcase()).ToList();
            var eventMode = new List<object> { _localizationService["ComboLevelScriptFunctions"], _localizationService["ComboNodeEditor"] };

            var presetNames = GetColorSchemePresetNames();

            // General
            Tabs.Add(Tab(_localizationService["Tab_General"],
                Group(_localizationService["Group_Misc"],
                    Bool(_localizationService["Opt_Editor_EnableStepHeightControlsForUnsupportedEngines"], "Editor_EnableStepHeightControlsForUnsupportedEngines"),
                    Bool(_localizationService["Opt_Log_WriteToFile"], "Log_WriteToFile"),
                    Num(_localizationService["Opt_Log_ArchiveN"], "Log_ArchiveN")),
                Group(_localizationService["Group_Defaults"],
                    Bool(_localizationService["Opt_Editor_GridNewRoom"], "Editor_GridNewRoom"),
                    Num(_localizationService["Opt_Editor_DefaultNewRoomSize"], "Editor_DefaultNewRoomSize", 3, 31),
                    Combo(_localizationService["Opt_Editor_DefaultProjectGameVersion"], "Editor_DefaultProjectGameVersion", gameVersions),
                    Bool(_localizationService["Opt_Editor_UseHalfPixelCorrectionOnPrjImport"], "Editor_UseHalfPixelCorrectionOnPrjImport"),
                    Bool(_localizationService["Opt_Editor_RespectFlybyPatchOnPrjImport"], "Editor_RespectFlybyPatchOnPrjImport")),
                Group(_localizationService["Group_System"],
                    Bool(_localizationService["Opt_Editor_AllowMultipleInstances"], "Editor_AllowMultipleInstances"),
                    Bool(_localizationService["Opt_Editor_OpenLastProjectOnStartup"], "Editor_OpenLastProjectOnStartup"),
                    Num(_localizationService["Opt_Editor_UndoDepth"], "Editor_UndoDepth", 1, 1000),
                    Bool(_localizationService["Opt_Editor_ReloadFilesAutomaticallyWhenChanged"], "Editor_ReloadFilesAutomaticallyWhenChanged")),
                Group(_localizationService["Group_Autosave"],
                    Bool(_localizationService["Opt_AutoSave_Enable"], "AutoSave_Enable"),
                    Num(_localizationService["Opt_AutoSave_TimeInSeconds"], "AutoSave_TimeInSeconds", 0, 10000),
                    Bool(_localizationService["Opt_AutoSave_NamePutDateFirst"], "AutoSave_NamePutDateFirst"),
                    Text(_localizationService["Opt_AutoSave_DateTimeFormat"], "AutoSave_DateTimeFormat"),
                    Bool(_localizationService["Opt_AutoSave_CleanupEnable"], "AutoSave_CleanupEnable"),
                    Num(_localizationService["Opt_AutoSave_CleanupMaxAutoSaves"], "AutoSave_CleanupMaxAutoSaves"))));

            // User interface
            Tabs.Add(Tab(_localizationService["Tab_UserInterface"],
                Group(_localizationService["Group_General"],
                    Bool(_localizationService["Opt_Palette_PickColorFromSelectedObject"], "Palette_PickColorFromSelectedObject"),
                    Bool(_localizationService["Opt_UI_GenerateRoomDescriptions"], "UI_GenerateRoomDescriptions"),
                    Bool(_localizationService["Opt_UI_WarnBeforeDeletingObjects"], "UI_WarnBeforeDeletingObjects"),
                    Bool(_localizationService["Opt_UI_AutoSwitchRoomToOutsideOnAppliedInvisibleTexture"], "UI_AutoSwitchRoomToOutsideOnAppliedInvisibleTexture"),
                    Bool(_localizationService["Opt_UI_AutoFillTriggerTypesForSwitchAndKey"], "UI_AutoFillTriggerTypesForSwitchAndKey"),
                    Bool(_localizationService["Opt_UI_SetAttributesAtOnce"], "UI_SetAttributesAtOnce"),
                    Bool(_localizationService["Opt_UI_DiscardSelectionOnModeSwitch"], "UI_DiscardSelectionOnModeSwitch"),
                    Bool(_localizationService["Opt_UI_ProbeAttributesThroughPortals"], "UI_ProbeAttributesThroughPortals"),
                    Bool(_localizationService["Opt_UI_AutoSwitchSectorColoringInfo"], "UI_AutoSwitchSectorColoringInfo"),
                    Bool(_localizationService["Opt_UI_OnlyShowSmallMessageWhenRoomIsLocked"], "UI_OnlyShowSmallMessageWhenRoomIsLocked")),
                Group(_localizationService["Group_ColorScheme"],
                    Preset(_localizationService["Opt_ColorSchemePreset"], presetNames),
                    Num(_localizationService["Opt_UI_FormColor_Brightness"], "UI_FormColor_Brightness", 50, 100, 5, 0, "%"),
                    Color(_localizationService["Opt_UI_FormColor_ButtonHighlight"], "UI_FormColor_ButtonHighlight"),
                    Color(_localizationService["Opt_Color2DRoomsMoved"], "Color2DRoomsMoved"),
                    Color(_localizationService["Opt_Color2DRoomsBelow"], "Color2DRoomsBelow"),
                    Color(_localizationService["Opt_Color2DRoomsAbove"], "Color2DRoomsAbove"),
                    Color(_localizationService["Opt_ColorSlideDirection"], "ColorSlideDirection"),
                    Color(_localizationService["Opt_ColorIllegalSlope"], "ColorIllegalSlope"),
                    Color(_localizationService["Opt_ColorSelection"], "ColorSelection"),
                    Color(_localizationService["Opt_ColorForceSolidFloor"], "ColorForceSolidFloor"),
                    Color(_localizationService["Opt_ColorTriggerTriggerer"], "ColorTriggerTriggerer"),
                    Color(_localizationService["Opt_ColorBeetle"], "ColorBeetle"),
                    Color(_localizationService["Opt_ColorNotWalkable"], "ColorNotWalkable"),
                    Color(_localizationService["Opt_ColorDeath"], "ColorDeath"),
                    Color(_localizationService["Opt_ColorBox"], "ColorBox"),
                    Color(_localizationService["Opt_ColorClimb"], "ColorClimb"),
                    Color(_localizationService["Opt_ColorMonkey"], "ColorMonkey"),
                    Color(_localizationService["Opt_ColorTrigger"], "ColorTrigger"),
                    Color(_localizationService["Opt_ColorWallUpper"], "ColorWallUpper"),
                    Color(_localizationService["Opt_ColorWallLower"], "ColorWallLower"),
                    Color(_localizationService["Opt_ColorWall"], "ColorWall"),
                    Color(_localizationService["Opt_ColorBorderWall"], "ColorBorderWall"),
                    Color(_localizationService["Opt_ColorFloor"], "ColorFloor"),
                    Color(_localizationService["Opt_ColorPortalFace"], "ColorPortalFace"),
                    Color(_localizationService["Opt_ColorPortal"], "ColorPortal"),
                    Color(_localizationService["Opt_ColorPortalEffect"], "ColorPortalEffect"),
                    Color(_localizationService["Opt_ColorFlipRoom"], "ColorFlipRoom"),
                    Color(_localizationService["Opt_Color2DBackground"], "Color2DBackground"),
                    Color(_localizationService["Opt_Color3DBackground"], "Color3DBackground"))));

            // 3D window
            Tabs.Add(Tab(_localizationService["Tab_3DWindow"],
                Group(_localizationService["Group_Text"],
                    Bool(_localizationService["Opt_Rendering3D_DrawFontOverlays"], "Rendering3D_DrawFontOverlays"),
                    Bool(_localizationService["Opt_Rendering3D_FontIsBold"], "Rendering3D_FontIsBold"),
                    Num(_localizationService["Opt_Rendering3D_FontSize"], "Rendering3D_FontSize", 0, 1000, 1, 2),
                    Combo(_localizationService["Opt_Rendering3D_FontName"], "Rendering3D_FontName", fonts)),
                Group(_localizationService["Group_Rendering"],
                    Combo(_localizationService["Opt_Rendering3D_ObjectQuality"], "Rendering3D_ObjectQuality", objectQuality),
                    Bool(_localizationService["Opt_Rendering3D_UseRoomEditorDirections"], "Rendering3D_UseRoomEditorDirections"),
                    Bool(_localizationService["Opt_Rendering3D_AnimateGhostBlockUnfolding"], "Rendering3D_AnimateGhostBlockUnfolding"),
                    Bool(_localizationService["Opt_Rendering3D_HighQualityLightPreview"], "Rendering3D_HighQualityLightPreview"),
                    Bool(_localizationService["Opt_Rendering3D_AutoBookmarkSelectedObject"], "Rendering3D_AutoBookmarkSelectedObject"),
                    Bool(_localizationService["Opt_Rendering3D_AutoswitchCurrentRoom"], "Rendering3D_AutoswitchCurrentRoom"),
                    Bool(_localizationService["Opt_Rendering3D_SelectObjectsInAnyRoom"], "Rendering3D_SelectObjectsInAnyRoom"),
                    Bool(_localizationService["Opt_Rendering3D_AnimateCameraOnReset"], "Rendering3D_AnimateCameraOnReset"),
                    Bool(_localizationService["Opt_Rendering3D_AlwaysShowCurrentRoomBounds"], "Rendering3D_AlwaysShowCurrentRoomBounds"),
                    Bool(_localizationService["Opt_Rendering3D_AllowTexturingInLightingMode"], "Rendering3D_AllowTexturingInLightingMode"),
                    Bool(_localizationService["Opt_Rendering3D_AnimateCameraOnRelocation"], "Rendering3D_AnimateCameraOnRelocation"),
                    Bool(_localizationService["Opt_Rendering3D_AnimateCameraOnDoubleClickRoomSwitch"], "Rendering3D_AnimateCameraOnDoubleClickRoomSwitch"),
                    Bool(_localizationService["Opt_Rendering3D_UseSpritesForServiceObjects"], "Rendering3D_UseSpritesForServiceObjects"),
                    Bool(_localizationService["Opt_Rendering3D_ShowFPS"], "Rendering3D_ShowFPS"),
                    Bool(_localizationService["Opt_Rendering3D_ResetCameraOnRoomSwitch"], "Rendering3D_ResetCameraOnRoomSwitch"),
                    Bool(_localizationService["Opt_Rendering3D_Antialias"], "Rendering3D_Antialias"),
                    Num(_localizationService["Opt_Rendering3D_LineWidth"], "Rendering3D_LineWidth"),
                    Num(_localizationService["Opt_Rendering3D_DrawRoomsMaxDepth"], "Rendering3D_DrawRoomsMaxDepth"),
                    Num(_localizationService["Opt_Rendering3D_FieldOfView"], "Rendering3D_FieldOfView", 10, 179))));

            // 3D controls
            Tabs.Add(Tab(_localizationService["Tab_3DControls"],
                Group(_localizationService["Group_Other"],
                    Num(_localizationService["Opt_Rendering3D_FlyModeMoveSpeed"], "Rendering3D_FlyModeMoveSpeed")),
                Group(_localizationService["Group_MouseControls"],
                    Bool(_localizationService["Opt_Rendering3D_CursorWarping"], "Rendering3D_CursorWarping"),
                    Bool(_localizationService["Opt_Rendering3D_InvertMouseZoom"], "Rendering3D_InvertMouseZoom"),
                    Num(_localizationService["Opt_Rendering3D_DragMouseSensitivity"], "Rendering3D_DragMouseSensitivity", 0, 1, 0.01, 2),
                    Num(_localizationService["Opt_Rendering3D_NavigationSpeedMouseRotate"], "Rendering3D_NavigationSpeedMouseRotate"),
                    Num(_localizationService["Opt_Rendering3D_NavigationSpeedMouseTranslate"], "Rendering3D_NavigationSpeedMouseTranslate", 0, 500000),
                    Num(_localizationService["Opt_Rendering3D_NavigationSpeedMouseZoom"], "Rendering3D_NavigationSpeedMouseZoom", 0, 500000),
                    Num(_localizationService["Opt_Rendering3D_NavigationSpeedMouseWheelZoom"], "Rendering3D_NavigationSpeedMouseWheelZoom")),
                Group(_localizationService["Group_KeyboardControls"],
                    Num(_localizationService["Opt_Rendering3D_NavigationSpeedKeyZoom"], "Rendering3D_NavigationSpeedKeyZoom", 0, 10000),
                    Num(_localizationService["Opt_Rendering3D_NavigationSpeedKeyRotate"], "Rendering3D_NavigationSpeedKeyRotate", 0, 10, 0.01, 2))));

            // Gizmo
            Tabs.Add(Tab(_localizationService["Tab_Gizmo"],
                Group(_localizationService["Group_Gizmo"],
                    Num(_localizationService["Opt_Gizmo_LineThickness"], "Gizmo_LineThickness", 0, 1000),
                    Num(_localizationService["Opt_Gizmo_ScaleCubeSize"], "Gizmo_ScaleCubeSize", 0, 1000),
                    Num(_localizationService["Opt_Gizmo_CenterCubeSize"], "Gizmo_CenterCubeSize", 0, 1000),
                    Num(_localizationService["Opt_Gizmo_TranslationConeSize"], "Gizmo_TranslationConeSize", 0, 1000),
                    Num(_localizationService["Opt_Gizmo_Size"], "Gizmo_Size", 0, 10000))));

            // Item browser
            Tabs.Add(Tab(_localizationService["Tab_ItemBrowser"],
                Group(_localizationService["Group_ItemBrowser"],
                    Bool(_localizationService["Opt_RenderingItem_Animate"], "RenderingItem_Animate"),
                    Bool(_localizationService["Opt_RenderingItem_ShowMultipleWadsPrompt"], "RenderingItem_ShowMultipleWadsPrompt"),
                    Bool(_localizationService["Opt_RenderingItem_HideInternalObjects"], "RenderingItem_HideInternalObjects"),
                    Bool(_localizationService["Opt_RenderingItem_Antialias"], "RenderingItem_Antialias"),
                    Num(_localizationService["Opt_RenderingItem_FieldOfView"], "RenderingItem_FieldOfView", 10, 179),
                    Num(_localizationService["Opt_RenderingItem_NavigationSpeedMouseRotate"], "RenderingItem_NavigationSpeedMouseRotate"),
                    Num(_localizationService["Opt_RenderingItem_NavigationSpeedMouseTranslate"], "RenderingItem_NavigationSpeedMouseTranslate", 0, 2000),
                    Num(_localizationService["Opt_RenderingItem_NavigationSpeedMouseZoom"], "RenderingItem_NavigationSpeedMouseZoom", 0, 2000),
                    Num(_localizationService["Opt_RenderingItem_NavigationSpeedMouseWheelZoom"], "RenderingItem_NavigationSpeedMouseWheelZoom"))));

            // 2D window
            Tabs.Add(Tab(_localizationService["Tab_2DWindow"],
                Group(_localizationService["Group_2DWindow"],
                    Num(_localizationService["Opt_Map2D_NavigationSpeedKeyZoom"], "Map2D_NavigationSpeedKeyZoom", 0.05, 10, 0.05, 2),
                    Num(_localizationService["Opt_Map2D_NavigationSpeedKeyMove"], "Map2D_NavigationSpeedKeyMove", 0, 500),
                    Num(_localizationService["Opt_Map2D_NavigationSpeedMouseZoom"], "Map2D_NavigationSpeedMouseZoom", 0, 1_000_000, 0.05, 2),
                    Num(_localizationService["Opt_Map2D_NavigationMinZoom"], "Map2D_NavigationMinZoom", 0.05, 1_000_000, 0.05, 2),
                    Num(_localizationService["Opt_Map2D_NavigationSpeedMouseWheelZoom"], "Map2D_NavigationSpeedMouseWheelZoom", 0.0001, 1, 0.0001, 4),
                    Num(_localizationService["Opt_Map2D_NavigationMaxZoom"], "Map2D_NavigationMaxZoom", 0, 5000))));

            // Texture map
            Tabs.Add(Tab(_localizationService["Tab_TextureMap"],
                Group(_localizationService["Group_TextureMap"],
                    Bool(_localizationService["Opt_TextureMap_WarnAboutIncorrectAttributes"], "TextureMap_WarnAboutIncorrectAttributes"),
                    Bool(_localizationService["Opt_TextureMap_ResetAttributesOnNewSelection"], "TextureMap_ResetAttributesOnNewSelection"),
                    Bool(_localizationService["Opt_TextureMap_PickTextureWithoutAttributes"], "TextureMap_PickTextureWithoutAttributes"),
                    Bool(_localizationService["Opt_TextureMap_MouseWheelMovesTheTextureInsteadOfZooming"], "TextureMap_MouseWheelMovesTheTextureInsteadOfZooming"),
                    Bool(_localizationService["Opt_TextureMap_DrawSelectionDirectionIndicators"], "TextureMap_DrawSelectionDirectionIndicators"),
                    Num(_localizationService["Opt_TextureMap_NavigationSpeedKeyZoom"], "TextureMap_NavigationSpeedKeyZoom", 0, 1_000_000, 0.05, 2),
                    Num(_localizationService["Opt_TextureMap_NavigationSpeedKeyMove"], "TextureMap_NavigationSpeedKeyMove", 0, 500),
                    Num(_localizationService["Opt_TextureMap_NavigationSpeedMouseZoom"], "TextureMap_NavigationSpeedMouseZoom", 0, 1_000_000, 0.05, 2),
                    Num(_localizationService["Opt_TextureMap_NavigationMinZoom"], "TextureMap_NavigationMinZoom", 0.05, 1_000_000, 0.05, 2),
                    Num(_localizationService["Opt_TextureMap_NavigationSpeedMouseWheelZoom"], "TextureMap_NavigationSpeedMouseWheelZoom", 0, 1_000_000, 0.0005, 4),
                    Num(_localizationService["Opt_TextureMap_NavigationMaxZoom"], "TextureMap_NavigationMaxZoom", 0, 5000))));

            // Node editor
            Tabs.Add(Tab(_localizationService["Tab_NodeEditor"],
                Group(_localizationService["Group_NodeEditor"],
                    Bool(_localizationService["Opt_NodeEditor_ShowGrips"], "NodeEditor_ShowGrips"),
                    Bool(_localizationService["Opt_NodeEditor_LinksAsRopes"], "NodeEditor_LinksAsRopes"),
                    Combo(_localizationService["Opt_NodeEditor_DefaultEventMode"], "NodeEditor_DefaultEventMode", eventMode),
                    Combo(_localizationService["Opt_NodeEditor_DefaultEventToEdit"], "NodeEditor_DefaultEventToEdit", volumeEvents),
                    Combo(_localizationService["Opt_NodeEditor_DefaultGlobalEventToEdit"], "NodeEditor_DefaultGlobalEventToEdit", globalEvents),
                    Num(_localizationService["Opt_NodeEditor_Size"], "NodeEditor_Size", 1, 1024, 0.05, 0),
                    Num(_localizationService["Opt_NodeEditor_GridStep"], "NodeEditor_GridStep", 1, 64, 0.0005, 0),
                    Num(_localizationService["Opt_NodeEditor_DefaultNodeWidth"], "NodeEditor_DefaultNodeWidth", 128, 1024))));

            // Drop options whose exact config property wasn't resolved (placeholders) - only keep ones
            // that actually exist on the configuration so reflection read/write stays correct.
            foreach (var tab in Tabs)
                foreach (var group in tab.Groups)
                    group.Items.RemoveAll(o => o.Kind != OptionKind.ColorSchemePreset && GetOption(_config, o.ConfigName) == null);

            // Apply the preset as soon as one is picked (the view binds the combo's SelectedItem to Value).
            foreach (var item in Tabs.SelectMany(t => t.AllItems).Where(i => i.Kind == OptionKind.ColorSchemePreset))
                item.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(OptionItem.Value) && item.Value is string presetName)
                        ApplyColorSchemePreset(presetName);
                };
        }

        private List<object> GetColorSchemePresetNames()
        {
            var scheme = _config.GetType().GetProperty(ColorSchemeProperty)?.GetValue(_config);
            if (scheme is null)
                return new List<object>();

            return scheme.GetType().GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == scheme.GetType())
                .Select(f => (object)f.Name)
                .ToList();
        }

        // Schema helpers.

        private static OptionTab Tab(string header, params OptionGroup[] groups)
        {
            var tab = new OptionTab { Header = header };
            tab.Groups.AddRange(groups);
            return tab;
        }

        private static OptionGroup Group(string header, params OptionItem?[] items)
        {
            var group = new OptionGroup { Header = header };
            group.Items.AddRange(items.Where(i => i != null)!);
            return group;
        }

        private static OptionItem? Bool(string label, string cfg, bool skip = false)
            => skip ? null : new OptionItem { Kind = OptionKind.Bool, Label = label, ConfigName = cfg };

        private static OptionItem? Num(string label, string cfg, double min = 0, double max = 1_000_000, double inc = 1, int dec = 0, string? suffix = null, bool skip = false)
            => skip ? null : new OptionItem { Kind = OptionKind.Number, Label = label, ConfigName = cfg, Minimum = min, Maximum = max, Increment = inc, DecimalPlaces = dec, Suffix = suffix };

        private static OptionItem Text(string label, string cfg)
            => new() { Kind = OptionKind.Text, Label = label, ConfigName = cfg };

        private static OptionItem Combo(string label, string cfg, IReadOnlyList<object> items)
            => new() { Kind = OptionKind.Combo, Label = label, ConfigName = cfg, Items = items };

        private static OptionItem Color(string label, string cfg)
            => new() { Kind = OptionKind.Color, Label = label, ConfigName = cfg };

        private static OptionItem Preset(string label, IReadOnlyList<object> items)
            => new() { Kind = OptionKind.ColorSchemePreset, Label = label, Items = items };
    }
}
