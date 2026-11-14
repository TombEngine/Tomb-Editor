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
    public partial class OptionsWindowViewModel : ObservableObject, IModalDialogViewModel, IDisposable
    {
        private const string ColorSchemeProperty = "UI_ColorScheme";

        private bool _isApplyingPreset;

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

            _editor.EditorEventRaised += OnEditorEventRaised;
        }

        /// <summary>Stops listening for configuration changes made elsewhere while the dialog is open.</summary>
        public void Dispose() => _editor.EditorEventRaised -= OnEditorEventRaised;

        // Keeps the displayed values in sync when the configuration changes from outside (Apply, other windows).
        private void OnEditorEventRaised(IEditorEvent editorEvent)
        {
            if (editorEvent is Editor.ConfigurationChangedEvent)
                ReadConfigIntoItems(_config);
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
                if (item.Kind is OptionKind.ColorSchemePreset or OptionKind.BoolRow)
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
                if (item.Kind is OptionKind.ColorSchemePreset or OptionKind.BoolRow)
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

            _isApplyingPreset = true;

            foreach (var item in Tabs.SelectMany(t => t.AllItems).Where(o => o.Kind == OptionKind.Color))
            {
                var field = preset.GetType().GetField(item.ConfigName);
                if (field != null && field.GetValue(preset) is Vector4 v)
                    item.Value = v.ToWPFColor();
            }

            _isApplyingPreset = false;
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
                        bool match = item.MatchesQuery(query);
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
            Tabs.Add(Tab(Loc("Tab_General"),
                Group(Loc("Group_Autosave"),
                    Bool("Opt_AutoSave_Enable", "AutoSave_Enable"),
                    Num("Opt_AutoSave_TimeInSeconds", "AutoSave_TimeInSeconds", 0, 10000),
                    Text("Opt_AutoSave_DateTimeFormat", "AutoSave_DateTimeFormat"),
                    Bool("Opt_AutoSave_NamePutDateFirst", "AutoSave_NamePutDateFirst"),
                    BoolWithNumber("Opt_AutoSave_CleanupEnable", "AutoSave_CleanupEnable", "AutoSave_CleanupMaxAutoSaves")),
                Group(Loc("Group_System"),
                    Bool("Opt_Editor_AllowMultipleInstances", "Editor_AllowMultipleInstances"),
                    Bool("Opt_Editor_OpenLastProjectOnStartup", "Editor_OpenLastProjectOnStartup"),
                    Bool("Opt_Editor_ReloadFilesAutomaticallyWhenChanged", "Editor_ReloadFilesAutomaticallyWhenChanged"),
                    Num("Opt_Editor_UndoDepth", "Editor_UndoDepth", 1, 1000)),
                Group(Loc("Group_Defaults"),
                    Combo("Opt_Editor_DefaultProjectGameVersion", "Editor_DefaultProjectGameVersion", gameVersions),
                    Num("Opt_Editor_DefaultNewRoomSize", "Editor_DefaultNewRoomSize", 3, 31),
                    Bool("Opt_Editor_GridNewRoom", "Editor_GridNewRoom"),
                    Bool("Opt_Editor_RespectFlybyPatchOnPrjImport", "Editor_RespectFlybyPatchOnPrjImport"),
                    Bool("Opt_Editor_UseHalfPixelCorrectionOnPrjImport", "Editor_UseHalfPixelCorrectionOnPrjImport")),
                Group(Loc("Group_Misc"),
                    Bool("Opt_Log_WriteToFile", "Log_WriteToFile"),
                    Num("Opt_Log_ArchiveN", "Log_ArchiveN"),
                    Bool("Opt_Editor_EnableStepHeightControlsForUnsupportedEngines", "Editor_EnableStepHeightControlsForUnsupportedEngines"))));

            // User interface
            Tabs.Add(Tab(Loc("Tab_UserInterface"),
                Group(Loc("Group_General"),
                    Bool("Opt_UI_WarnBeforeDeletingObjects", "UI_WarnBeforeDeletingObjects"),
                    Bool("Opt_UI_OnlyShowSmallMessageWhenRoomIsLocked", "UI_OnlyShowSmallMessageWhenRoomIsLocked"),
                    Bool("Opt_UI_DiscardSelectionOnModeSwitch", "UI_DiscardSelectionOnModeSwitch"),
                    Bool("Opt_UI_AutoSwitchSectorColoringInfo", "UI_AutoSwitchSectorColoringInfo"),
                    Bool("Opt_UI_ProbeAttributesThroughPortals", "UI_ProbeAttributesThroughPortals"),
                    Bool("Opt_UI_SetAttributesAtOnce", "UI_SetAttributesAtOnce"),
                    Bool("Opt_UI_AutoFillTriggerTypesForSwitchAndKey", "UI_AutoFillTriggerTypesForSwitchAndKey"),
                    Bool("Opt_UI_AutoSwitchRoomToOutsideOnAppliedInvisibleTexture", "UI_AutoSwitchRoomToOutsideOnAppliedInvisibleTexture"),
                    Bool("Opt_UI_GenerateRoomDescriptions", "UI_GenerateRoomDescriptions"),
                    Bool("Opt_Palette_PickColorFromSelectedObject", "Palette_PickColorFromSelectedObject")),
                Group(Loc("Group_ColorScheme"),
                    Num("Opt_UI_FormColor_Brightness", "UI_FormColor_Brightness", 50, 100, 1, 0, "%"),
                    Preset("Opt_ColorSchemePreset", presetNames),
                    Color("Opt_UI_FormColor_ButtonHighlight", "UI_FormColor_ButtonHighlight"),
                    Color("Opt_ColorSelection", "ColorSelection"),
                    Color("Opt_Color3DBackground", "Color3DBackground"),
                    Color("Opt_Color2DBackground", "Color2DBackground"),
                    Color("Opt_ColorFlipRoom", "ColorFlipRoom"),
                    Color("Opt_ColorPortal", "ColorPortal"),
                    Color("Opt_ColorPortalEffect", "ColorPortalEffect"),
                    Color("Opt_ColorPortalFace", "ColorPortalFace"),
                    Color("Opt_ColorFloor", "ColorFloor"),
                    Color("Opt_ColorBorderWall", "ColorBorderWall"),
                    Color("Opt_ColorWall", "ColorWall"),
                    Color("Opt_ColorWallLower", "ColorWallLower"),
                    Color("Opt_ColorWallUpper", "ColorWallUpper"),
                    Color("Opt_ColorTrigger", "ColorTrigger"),
                    Color("Opt_ColorMonkey", "ColorMonkey"),
                    Color("Opt_ColorClimb", "ColorClimb"),
                    Color("Opt_ColorBox", "ColorBox"),
                    Color("Opt_ColorDeath", "ColorDeath"),
                    Color("Opt_ColorNotWalkable", "ColorNotWalkable"),
                    Color("Opt_ColorBeetle", "ColorBeetle"),
                    Color("Opt_ColorTriggerTriggerer", "ColorTriggerTriggerer"),
                    Color("Opt_ColorForceSolidFloor", "ColorForceSolidFloor"),
                    Color("Opt_ColorIllegalSlope", "ColorIllegalSlope"),
                    Color("Opt_ColorSlideDirection", "ColorSlideDirection"),
                    Color("Opt_Color2DRoomsAbove", "Color2DRoomsAbove"),
                    Color("Opt_Color2DRoomsBelow", "Color2DRoomsBelow"),
                    Color("Opt_Color2DRoomsMoved", "Color2DRoomsMoved"))));

            // 3D window
            Tabs.Add(Tab(Loc("Tab_3DWindow"),
                Group(Loc("Group_Rendering"),
                    Bool("Opt_Rendering3D_Antialias", "Rendering3D_Antialias"),
                    Bool("Opt_Rendering3D_HighQualityLightPreview", "Rendering3D_HighQualityLightPreview"),
                    Bool("Opt_Rendering3D_AllowTexturingInLightingMode", "Rendering3D_AllowTexturingInLightingMode"),
                    Bool("Opt_Rendering3D_AlwaysShowCurrentRoomBounds", "Rendering3D_AlwaysShowCurrentRoomBounds"),
                    Bool("Opt_Rendering3D_SelectObjectsInAnyRoom", "Rendering3D_SelectObjectsInAnyRoom"),
                    Bool("Opt_Rendering3D_AutoswitchCurrentRoom", "Rendering3D_AutoswitchCurrentRoom"),
                    Bool("Opt_Rendering3D_AutoBookmarkSelectedObject", "Rendering3D_AutoBookmarkSelectedObject"),
                    Bool("Opt_Rendering3D_UseRoomEditorDirections", "Rendering3D_UseRoomEditorDirections"),
                    Bool("Opt_Rendering3D_UseSpritesForServiceObjects", "Rendering3D_UseSpritesForServiceObjects"),
                    Bool("Opt_Rendering3D_ShowFPS", "Rendering3D_ShowFPS"),
                    Bool("Opt_Rendering3D_ResetCameraOnRoomSwitch", "Rendering3D_ResetCameraOnRoomSwitch"),
                    Bool("Opt_Rendering3D_AnimateGhostBlockUnfolding", "Rendering3D_AnimateGhostBlockUnfolding"),
                    BoolRow("Opt_Rendering3D_AnimateCameraOn",
                        ("Opt_Rendering3D_AnimateCameraOnDoubleClickRoomSwitch", "Rendering3D_AnimateCameraOnDoubleClickRoomSwitch"),
                        ("Opt_Rendering3D_AnimateCameraOnRelocation", "Rendering3D_AnimateCameraOnRelocation"),
                        ("Opt_Rendering3D_AnimateCameraOnReset", "Rendering3D_AnimateCameraOnReset")),
                    Num("Opt_Rendering3D_FieldOfView", "Rendering3D_FieldOfView", 10, 179),
                    Num("Opt_Rendering3D_DrawRoomsMaxDepth", "Rendering3D_DrawRoomsMaxDepth"),
                    Num("Opt_Rendering3D_LineWidth", "Rendering3D_LineWidth"),
                    Combo("Opt_Rendering3D_ObjectQuality", "Rendering3D_ObjectQuality", objectQuality)),
                Group(Loc("Group_Text"),
                    ComboWithBool("Opt_Rendering3D_FontName", "Rendering3D_FontName", fonts, "Opt_Rendering3D_FontIsBold", "Rendering3D_FontIsBold"),
                    Num("Opt_Rendering3D_FontSize", "Rendering3D_FontSize", 0, 1000, 1, 2),
                    Bool("Opt_Rendering3D_DrawFontOverlays", "Rendering3D_DrawFontOverlays"))));

            // 3D controls
            Tabs.Add(Tab(Loc("Tab_3DControls"),
                Group(Loc("Group_KeyboardControls"),
                    Num("Opt_Rendering3D_NavigationSpeedKeyZoom", "Rendering3D_NavigationSpeedKeyZoom", 0, 10000),
                    Num("Opt_Rendering3D_NavigationSpeedKeyRotate", "Rendering3D_NavigationSpeedKeyRotate", 0, 10, 0.1, 2)),
                Group(Loc("Group_MouseControls"),
                    Num("Opt_Rendering3D_NavigationSpeedMouseWheelZoom", "Rendering3D_NavigationSpeedMouseWheelZoom"),
                    Num("Opt_Rendering3D_NavigationSpeedMouseZoom", "Rendering3D_NavigationSpeedMouseZoom", 0, 500000),
                    Num("Opt_Rendering3D_NavigationSpeedMouseTranslate", "Rendering3D_NavigationSpeedMouseTranslate", 0, 500000),
                    Num("Opt_Rendering3D_NavigationSpeedMouseRotate", "Rendering3D_NavigationSpeedMouseRotate"),
                    Num("Opt_Rendering3D_DragMouseSensitivity", "Rendering3D_DragMouseSensitivity", 0, 1, 0.01, 2),
                    Bool("Opt_Rendering3D_InvertMouseZoom", "Rendering3D_InvertMouseZoom"),
                    Bool("Opt_Rendering3D_CursorWarping", "Rendering3D_CursorWarping")),
                Group(Loc("Group_Other"),
                    Num("Opt_Rendering3D_FlyModeMoveSpeed", "Rendering3D_FlyModeMoveSpeed"))));

            // Gizmo
            Tabs.Add(Tab(Loc("Tab_Gizmo"),
                Group(string.Empty,
                    Num("Opt_Gizmo_Size", "Gizmo_Size", 0, 10000),
                    Num("Opt_Gizmo_TranslationConeSize", "Gizmo_TranslationConeSize", 0, 1000),
                    Num("Opt_Gizmo_CenterCubeSize", "Gizmo_CenterCubeSize", 0, 1000),
                    Num("Opt_Gizmo_ScaleCubeSize", "Gizmo_ScaleCubeSize", 0, 1000),
                    Num("Opt_Gizmo_LineThickness", "Gizmo_LineThickness", 0, 1000))));

            // Item browser
            Tabs.Add(Tab(Loc("Tab_ItemBrowser"),
                Group(string.Empty,
                    Num("Opt_RenderingItem_NavigationSpeedMouseWheelZoom", "RenderingItem_NavigationSpeedMouseWheelZoom"),
                    Num("Opt_RenderingItem_NavigationSpeedMouseZoom", "RenderingItem_NavigationSpeedMouseZoom", 0, 2000),
                    Num("Opt_RenderingItem_NavigationSpeedMouseTranslate", "RenderingItem_NavigationSpeedMouseTranslate", 0, 2000),
                    Num("Opt_RenderingItem_NavigationSpeedMouseRotate", "RenderingItem_NavigationSpeedMouseRotate"),
                    Num("Opt_RenderingItem_FieldOfView", "RenderingItem_FieldOfView", 10, 179),
                    Bool("Opt_RenderingItem_Antialias", "RenderingItem_Antialias"),
                    Bool("Opt_RenderingItem_HideInternalObjects", "RenderingItem_HideInternalObjects"),
                    Bool("Opt_RenderingItem_ShowMultipleWadsPrompt", "RenderingItem_ShowMultipleWadsPrompt"),
                    Bool("Opt_RenderingItem_Animate", "RenderingItem_Animate"))));

            // 2D window
            Tabs.Add(Tab(Loc("Tab_2DWindow"),
                Group(string.Empty,
                    Num("Opt_Map2D_NavigationMinZoom", "Map2D_NavigationMinZoom", 0.01, 1_000_000, 0.05, 2),
                    Num("Opt_Map2D_NavigationMaxZoom", "Map2D_NavigationMaxZoom", 0, 5000),
                    Num("Opt_Map2D_NavigationSpeedMouseWheelZoom", "Map2D_NavigationSpeedMouseWheelZoom", 0.001, 1, 0.001, 4),
                    Num("Opt_Map2D_NavigationSpeedMouseZoom", "Map2D_NavigationSpeedMouseZoom", 0, 1_000_000, 0.5, 2),
                    Num("Opt_Map2D_NavigationSpeedKeyZoom", "Map2D_NavigationSpeedKeyZoom", 0.01, 10, 0.05, 2),
                    Num("Opt_Map2D_NavigationSpeedKeyMove", "Map2D_NavigationSpeedKeyMove", 0, 500))));

            // Texture map
            Tabs.Add(Tab(Loc("Tab_TextureMap"),
                Group(string.Empty,
                    Num("Opt_TextureMap_NavigationMinZoom", "TextureMap_NavigationMinZoom", 0.01, 1_000_000, 0.05, 2),
                    Num("Opt_TextureMap_NavigationMaxZoom", "TextureMap_NavigationMaxZoom", 0, 5000),
                    Num("Opt_TextureMap_NavigationSpeedMouseWheelZoom", "TextureMap_NavigationSpeedMouseWheelZoom", 0, 1_000_000, 0.0005, 4),
                    Num("Opt_TextureMap_NavigationSpeedMouseZoom", "TextureMap_NavigationSpeedMouseZoom", 0, 1_000_000, 0.5, 2),
                    Num("Opt_TextureMap_NavigationSpeedKeyZoom", "TextureMap_NavigationSpeedKeyZoom", 0, 1_000_000, 0.05, 2),
                    Num("Opt_TextureMap_NavigationSpeedKeyMove", "TextureMap_NavigationSpeedKeyMove", 0, 500),
                    Bool("Opt_TextureMap_DrawSelectionDirectionIndicators", "TextureMap_DrawSelectionDirectionIndicators"),
                    Bool("Opt_TextureMap_MouseWheelMovesTheTextureInsteadOfZooming", "TextureMap_MouseWheelMovesTheTextureInsteadOfZooming"),
                    Bool("Opt_TextureMap_PickTextureWithoutAttributes", "TextureMap_PickTextureWithoutAttributes"),
                    Bool("Opt_TextureMap_ResetAttributesOnNewSelection", "TextureMap_ResetAttributesOnNewSelection"),
                    Bool("Opt_TextureMap_WarnAboutIncorrectAttributes", "TextureMap_WarnAboutIncorrectAttributes"))));

            // Node editor
            Tabs.Add(Tab(Loc("Tab_NodeEditor"),
                Group(string.Empty,
                    Num("Opt_NodeEditor_Size", "NodeEditor_Size", 1, 1024, 0.05, 0),
                    Num("Opt_NodeEditor_GridStep", "NodeEditor_GridStep", 1, 64, 0.0005, 0),
                    Num("Opt_NodeEditor_DefaultNodeWidth", "NodeEditor_DefaultNodeWidth", 128, 1024),
                    Combo("Opt_NodeEditor_DefaultEventMode", "NodeEditor_DefaultEventMode", eventMode),
                    Combo("Opt_NodeEditor_DefaultEventToEdit", "NodeEditor_DefaultEventToEdit", volumeEvents),
                    Combo("Opt_NodeEditor_DefaultGlobalEventToEdit", "NodeEditor_DefaultGlobalEventToEdit", globalEvents),
                    Bool("Opt_NodeEditor_LinksAsRopes", "NodeEditor_LinksAsRopes"),
                    Bool("Opt_NodeEditor_ShowGrips", "NodeEditor_ShowGrips"))));

            // Drop options whose exact config property wasn't resolved (placeholders) - only keep ones
            // that actually exist on the configuration so reflection read/write stays correct.
            foreach (var tab in Tabs)
                foreach (var group in tab.Groups)
                    group.Items.RemoveAll(o => o.Kind is not (OptionKind.ColorSchemePreset or OptionKind.BoolRow) && GetOption(_config, o.ConfigName) == null);

            // Apply the preset as soon as one is picked (the view binds the combo's SelectedItem to Value).
            foreach (var item in Tabs.SelectMany(t => t.AllItems).Where(i => i.Kind == OptionKind.ColorSchemePreset))
                item.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(OptionItem.Value) && item.Value is string presetName)
                        ApplyColorSchemePreset(presetName);
                };

            // Changing any color by hand means the colors no longer match the picked preset.
            foreach (var item in Tabs.SelectMany(t => t.AllItems).Where(i => i.Kind == OptionKind.Color))
                item.PropertyChanged += (_, e) =>
                {
                    if (!_isApplyingPreset && e.PropertyName == nameof(OptionItem.Value))
                        ClearColorSchemePreset();
                };
        }

        private void ClearColorSchemePreset()
        {
            foreach (var preset in Tabs.SelectMany(t => t.AllItems).Where(i => i.Kind == OptionKind.ColorSchemePreset))
                preset.Value = null;
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

        private string Loc(string key) => _localizationService[key];

        private static OptionTab Tab(string header, params OptionGroup[] groups)
        {
            var tab = new OptionTab { Header = header };
            tab.Groups.AddRange(groups);
            return tab;
        }

        private static OptionGroup Group(string header, params OptionItem[] items)
        {
            var group = new OptionGroup { Header = header };
            group.Items.AddRange(items);
            return group;
        }

        private OptionItem Bool(string labelKey, string cfg)
            => new() { Kind = OptionKind.Bool, Label = Loc(labelKey), ConfigName = cfg };

        private OptionItem Num(string labelKey, string cfg, double min = 0, double max = 100, double inc = 1, int dec = 0, string? suffix = null)
            => new() { Kind = OptionKind.Number, Label = Loc(labelKey), ConfigName = cfg, Minimum = min, Maximum = max, Increment = inc, DecimalPlaces = dec, Suffix = suffix };

        private OptionItem Text(string labelKey, string cfg)
            => new() { Kind = OptionKind.Text, Label = Loc(labelKey), ConfigName = cfg };

        private OptionItem Combo(string labelKey, string cfg, IReadOnlyList<object> items)
            => new() { Kind = OptionKind.Combo, Label = Loc(labelKey), ConfigName = cfg, Items = items };

        private OptionItem Color(string labelKey, string cfg)
            => new() { Kind = OptionKind.Color, Label = Loc(labelKey), ConfigName = cfg };

        private OptionItem Preset(string labelKey, IReadOnlyList<object> items)
            => new() { Kind = OptionKind.ColorSchemePreset, Label = Loc(labelKey), Items = items };

        // A checkbox followed by a numeric box on the same row.
        private OptionItem BoolWithNumber(string labelKey, string boolCfg, string numberCfg)
        {
            var item = new OptionItem { Kind = OptionKind.BoolWithNumber, Label = Loc(labelKey), ConfigName = boolCfg };
            item.Children.Add(new OptionItem { Kind = OptionKind.Number, ConfigName = numberCfg });
            return item;
        }

        // A combo box followed by a checkbox on the same row.
        private OptionItem ComboWithBool(string labelKey, string comboCfg, IReadOnlyList<object> items, string boolLabelKey, string boolCfg)
        {
            var item = new OptionItem { Kind = OptionKind.ComboWithBool, Label = Loc(labelKey), ConfigName = comboCfg, Items = items };
            item.Children.Add(new OptionItem { Kind = OptionKind.Bool, Label = Loc(boolLabelKey), ConfigName = boolCfg });
            return item;
        }

        // A caption followed by several checkboxes on the same row.
        private OptionItem BoolRow(string labelKey, params (string LabelKey, string Cfg)[] checkboxes)
        {
            var item = new OptionItem { Kind = OptionKind.BoolRow, Label = Loc(labelKey) };
            item.Children.AddRange(checkboxes.Select(c => new OptionItem { Kind = OptionKind.Bool, Label = Loc(c.LabelKey), ConfigName = c.Cfg }));
            return item;
        }
    }
}
