#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using TombLib.WPF;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;
using WpfColor = System.Windows.Media.Color;

namespace WadTool.Features.Dialogs.Options
{
    /// <summary>
    /// WPF port of WadTool's <c>FormOptions</c> / <c>FormOptionsBase</c>. The dialog is data-driven:
    /// a schema of tabs -> groups -> <see cref="OptionItem"/>s is rendered by the view, and values are
    /// read from / written to <see cref="Configuration"/> by reflection on each option's
    /// <see cref="OptionItem.ConfigName"/> (the same convention the control <c>Tag</c> used).
    /// </summary>
    public partial class OptionsWindowViewModel : ObservableObject, IModalDialogViewModel
    {
        private readonly Configuration _config;
        private readonly IColorPickerService _colorPickerService;
        private readonly ILocalizationService _localizationService;

        public ObservableCollection<OptionTab> Tabs { get; } = new();

        [ObservableProperty] private OptionTab? _selectedTab;
        [ObservableProperty] private bool? _dialogResult;
        [ObservableProperty] private string _searchText = string.Empty;

        public OptionsWindowViewModel(WadToolClass tool, IColorPickerService? colorPickerService = null, ILocalizationService? localizationService = null)
        {
            _config = tool.Configuration;
            _colorPickerService = colorPickerService ?? new ColorPickerService();
            _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

            BuildSchema();
            ReadConfigIntoItems(_config);
            SelectedTab = Tabs.FirstOrDefault();
        }

        // Commands.

        [RelayCommand]
        private void Apply() => WriteConfigFromItems();

        [RelayCommand]
        private void Ok()
        {
            WriteConfigFromItems();
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

        // Reflection get / set (mirrors FormOptionsBase).

        private static object? GetOption(object config, string name)
            => config.GetType().GetProperty(name)?.GetValue(config);

        private void SetOption(string name, object? value)
            => _config.GetType().GetProperty(name)?.SetValue(_config, value);

        private void ReadConfigIntoItems(object config, OptionTab? onlyTab = null)
        {
            var items = onlyTab?.AllItems ?? Tabs.SelectMany(t => t.AllItems);

            foreach (var item in items)
            {
                var option = GetOption(config, item.ConfigName);
                if (option is null)
                    continue;

                switch (option)
                {
                    case bool b:
                        item.Value = b;
                        break;
                    case string s:
                        item.Value = s;
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
                var option = GetOption(_config, item.ConfigName);
                if (option is null)
                    continue;

                switch (option)
                {
                    case bool when item.Value is bool b:
                        SetOption(item.ConfigName, b);
                        break;
                    case string when item.Value is string s:
                        SetOption(item.ConfigName, s);
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

            // Mirrors the legacy WriteConfigFromControls override.
            _config?.SaveTry();
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
            => item.Items is not null && index >= 0 && index < item.Items.Count ? item.Items[index] : null;

        private static int IndexOf(OptionItem item, object? value)
        {
            if (item.Items is null || value is null)
                return 0;
            int index = item.Items.ToList().IndexOf(value);
            return index < 0 ? 0 : index;
        }

        // Schema. Tabs / groups / options replicate the legacy FormOptions designer
        // (every control whose Tag named a Configuration property), in the same visual order.

        private void BuildSchema()
        {
            // General
            Tabs.Add(Tab(_localizationService["Tab_General"],
                Group(_localizationService["Group_Editing"],
                    Bool(_localizationService["Opt_MeshEditor_MouseWheelMovesTheTextureInsteadOfZooming"], "MeshEditor_MouseWheelMovesTheTextureInsteadOfZooming"),
                    Bool(_localizationService["Opt_AnimationEditor_RewindAfterChainPlayback"], "AnimationEditor_RewindAfterChainPlayback"),
                    Bool(_localizationService["Opt_AnimationEditor_ClampStateChangeValues"], "AnimationEditor_ClampStateChangeValues"),
                    Num(_localizationService["Opt_AnimationEditor_UndoDepth"], "AnimationEditor_UndoDepth", 1, 1000)),
                Group(_localizationService["Group_UserInterface"],
                    Bool(_localizationService["Opt_RenderingItem_Antialias"], "RenderingItem_Antialias"),
                    Bool(_localizationService["Opt_RenderingItem_ShowDebugInfo"], "RenderingItem_ShowDebugInfo"),
                    Bool(_localizationService["Opt_RenderingItem_Animate"], "RenderingItem_Animate"),
                    Num(_localizationService["Opt_UI_FormColor_Brightness"], "UI_FormColor_Brightness", 50, 100, 5, 0, "%"),
                    Color(_localizationService["Opt_RenderingItem_BackgroundColor"], "RenderingItem_BackgroundColor"),
                    Num(_localizationService["Opt_RenderingItem_FieldOfView"], "RenderingItem_FieldOfView", 10, 179),
                    Num(_localizationService["Opt_RenderingItem_NavigationSpeedMouseWheelZoom"], "RenderingItem_NavigationSpeedMouseWheelZoom", 0, 10000),
                    Num(_localizationService["Opt_RenderingItem_NavigationSpeedMouseZoom"], "RenderingItem_NavigationSpeedMouseZoom", 0, 10000),
                    Num(_localizationService["Opt_RenderingItem_NavigationSpeedMouseTranslate"], "RenderingItem_NavigationSpeedMouseTranslate", 0, 10000),
                    Num(_localizationService["Opt_RenderingItem_NavigationSpeedMouseRotate"], "RenderingItem_NavigationSpeedMouseRotate", 0, 10000)),
                Group(_localizationService["Group_System"],
                    Bool(_localizationService["Opt_Tool_MakeEmptyWadAtStartup"], "Tool_MakeEmptyWadAtStartup"),
                    Bool(_localizationService["Opt_Log_WriteToFile"], "Log_WriteToFile"),
                    Num(_localizationService["Opt_Log_ArchiveN"], "Log_ArchiveN"))));

            // Gizmo
            Tabs.Add(Tab(_localizationService["Tab_Gizmo"],
                Group(_localizationService["Group_AnimationEditor"],
                    Num(_localizationService["Opt_Gizmo_Size"], "GizmoAnimationEditor_Size", 0, 10000),
                    Num(_localizationService["Opt_Gizmo_TranslationConeSize"], "GizmoAnimationEditor_TranslationConeSize", 0, 1000),
                    Num(_localizationService["Opt_Gizmo_CenterCubeSize"], "GizmoAnimationEditor_CenterCubeSize", 0, 1000),
                    Num(_localizationService["Opt_Gizmo_ScaleCubeSize"], "GizmoAnimationEditor_ScaleCubeSize", 0, 1000),
                    Num(_localizationService["Opt_Gizmo_LineThickness"], "GizmoAnimationEditor_LineThickness", 0, 1000)),
                Group(_localizationService["Group_SkeletonEditor"],
                    Num(_localizationService["Opt_Gizmo_Size"], "GizmoSkeleton_Size", 0, 10000),
                    Num(_localizationService["Opt_Gizmo_TranslationConeSize"], "GizmoSkeleton_TranslationConeSize", 0, 1000),
                    Num(_localizationService["Opt_Gizmo_CenterCubeSize"], "GizmoSkeleton_CenterCubeSize", 0, 1000),
                    Num(_localizationService["Opt_Gizmo_ScaleCubeSize"], "GizmoSkeleton_ScaleCubeSize", 0, 1000),
                    Num(_localizationService["Opt_Gizmo_LineThickness"], "GizmoSkeleton_LineThickness", 0, 1000)),
                Group(_localizationService["Group_StaticEditor"],
                    Num(_localizationService["Opt_Gizmo_Size"], "GizmoStatic_Size", 0, 10000),
                    Num(_localizationService["Opt_Gizmo_TranslationConeSize"], "GizmoStatic_TranslationConeSize", 0, 1000),
                    Num(_localizationService["Opt_Gizmo_CenterCubeSize"], "GizmoStatic_CenterCubeSize", 0, 1000),
                    Num(_localizationService["Opt_Gizmo_ScaleCubeSize"], "GizmoStatic_ScaleCubeSize", 0, 1000),
                    Num(_localizationService["Opt_Gizmo_LineThickness"], "GizmoStatic_LineThickness", 0, 1000))));

            // Only keep options that actually exist on the configuration so reflection
            // read / write stays correct.
            foreach (var tab in Tabs)
                foreach (var group in tab.Groups)
                    group.Items.RemoveAll(o => GetOption(_config, o.ConfigName) is null);
        }

        // Schema helpers.

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

        private static OptionItem Bool(string label, string cfg)
            => new() { Kind = OptionKind.Bool, Label = label, ConfigName = cfg };

        private static OptionItem Num(string label, string cfg, double min = 0, double max = 1_000_000, double inc = 1, int dec = 0, string? suffix = null)
            => new() { Kind = OptionKind.Number, Label = label, ConfigName = cfg, Minimum = min, Maximum = max, Increment = inc, DecimalPlaces = dec, Suffix = suffix };

        private static OptionItem Color(string label, string cfg)
            => new() { Kind = OptionKind.Color, Label = label, ConfigName = cfg };
    }
}
