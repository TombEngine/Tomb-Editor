#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TombLib.Icons;
using TombLib.LevelData;

namespace TombEditor.Features.Toolbar;

internal enum ToolbarButtonKind { Button, Toggle, DrawObjectsMenu }

/// <summary>
/// One configurable toolbar entry. <see cref="Token"/> is the short identifier persisted in
/// <c>Configuration.UI_ToolbarButtons</c> (and shown in the Customize dialog); the remaining fields
/// describe how to build the matching WPF control, mirroring the hardcoded buttons that used to live
/// in EditorToolbarView.xaml and the legacy previous <c>but&lt;Token&gt;</c> controls.
/// </summary>
internal sealed record ToolbarButtonSpec(
    string Token,
    string Command,
    string Icon,
    ToolbarButtonKind Kind = ToolbarButtonKind.Button,
    EditorMode? ActiveMode = null,
    string? ConfigFlag = null,
    PortalOpacity? PortalOpacity = null,
    PortalEffectType? PortalEffect = null);

/// <summary>
/// Maps the toolbar tokens stored in <c>Configuration.UI_ToolbarButtons</c> to the controls that
/// the WPF toolbar builds at runtime, so the Customize dialog actually drives the layout.
/// </summary>
internal static class ToolbarButtonRegistry
{
    public const string SeparatorToken = "|";
    public const string DrawObjectsToken = "DrawObjects";

    private static readonly ToolbarButtonSpec[] _specs =
    {
        // Editor modes
        new("2D", "Switch2DMode", "Actions/2DView", ToolbarButtonKind.Toggle, ActiveMode: EditorMode.Map2D),
        new("3D", "SwitchGeometryMode", "Actions/3DView", ToolbarButtonKind.Toggle, ActiveMode: EditorMode.Geometry),
        new("FaceEdit", "SwitchFaceEditMode", "Actions/TextureMode", ToolbarButtonKind.Toggle, ActiveMode: EditorMode.FaceEdit),
        new("ObjectPlacement", "SwitchObjectPlacementMode", "Toolbox/ObjectPlacement", ToolbarButtonKind.Toggle, ActiveMode: EditorMode.ObjectPlacement),
        new("LightingMode", "SwitchLightingMode", "Actions/light_on", ToolbarButtonKind.Toggle, ActiveMode: EditorMode.Lighting),

        // Undo / redo
        new("Undo", "Undo", "General/undo"),
        new("Redo", "Redo", "General/redo"),

        // Camera
        new("CenterCamera", "ResetCamera", "Actions/center_direction"),
        new("ToggleFlyMode", "ToggleFlyMode", "General/airplane"),

        // Draw flags
        new("DrawPortals", "DrawPortals", "Actions/DrawPortals", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_ShowPortals"),
        new("DrawAllRooms", "DrawAllRooms", "Actions/DrawAllRooms", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_ShowAllRooms"),
        new("DrawHorizon", "DrawHorizon", "Actions/horizon", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_ShowHorizon"),
        new("DrawRoomNames", "DrawRoomNames", "Actions/generic_text", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_ShowRoomNames"),
        new("DrawCardinalDirections", "DrawCardinalDirections", "Actions/DrawCardinalDirections", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_ShowCardinalDirections"),
        new("DrawExtraBlendingModes", "DrawExtraBlendingModes", "Texture/Transparent_1", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_ShowExtraBlendingModes"),
        new("HideTransparentFaces", "HideTransparentFaces", "Actions/AlphaTest", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_HideTransparentFaces"),
        new("BilinearFilter", "BilinearFilter", "General/blur", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_BilinearFilter"),
        new("DrawWhiteLighting", "DrawWhiteTextureLightingOnly", "Actions/DrawUntexturedLights", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_ShowLightingWhiteTextureOnly"),
        new("DrawStaticTint", "ShowRealTintForObjects", "Actions/StaticTint", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_ShowRealTintForObjects"),
        new("DrawIllegalSlopes", "DrawIllegalSlopes", "General/Warning", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_ShowIllegalSlopes"),
        new("DrawSlideDirections", "DrawSlideDirections", "Actions/Slide", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_ShowSlideDirections"),
        new("DisableGeometryPicking", "DisableGeometryPicking", "Actions/HideCustomGeometry_1", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_DisablePickingForImportedGeometry"),
        new("DisableHiddenRoomPicking", "DisableHiddenRoomPicking", "Actions/HideHiddenRooms", ToolbarButtonKind.Toggle, ConfigFlag: "Rendering3D_DisablePickingForHiddenRooms"),
        new(DrawObjectsToken, "", "Actions/DrawObjects", ToolbarButtonKind.DrawObjectsMenu),

        // Flipmap + clipboard
        new("FlipMap", "ToggleFlipMap", "General/copy_link"),
        new("Copy", "Copy", "General/copy"),
        new("Paste", "Paste", "General/clipboard"),
        new("Stamp", "StampObject", "Actions/rubber_stamp"),

        // Portal opacity / effect
        new("OpacityNone", "ToggleNoOpacity", "Texture/Solid", ToolbarButtonKind.Toggle, PortalOpacity: TombLib.LevelData.PortalOpacity.None),
        new("OpacitySolidFaces", "ToggleOpacity", "Texture/ToggleOpacity", ToolbarButtonKind.Toggle, PortalOpacity: TombLib.LevelData.PortalOpacity.SolidFaces),
        new("OpacityTraversableFaces", "ToggleOpacity2", "Texture/ToggleOpacity2", ToolbarButtonKind.Toggle, PortalOpacity: TombLib.LevelData.PortalOpacity.TraversableFaces),
        new("Mirror", "ToggleClassicPortalMirror", "Texture/MirrorPortal", ToolbarButtonKind.Toggle, PortalEffect: PortalEffectType.ClassicMirror),

        // Add object
        new("AddCamera", "AddCamera", "Objects/Camera"),
        new("AddSprite", "AddSprite", "Objects/Sprite"),
        new("AddFlybyCamera", "AddFlybyCamera", "Objects/movie_projector"),
        new("AddSink", "AddSink", "Objects/tornado"),
        new("AddSoundSource", "AddSoundSource", "Objects/speaker"),
        new("AddImportedGeometry", "AddImportedGeometry", "Objects/custom-geometry"),
        new("AddGhostBlock", "AddGhostBlock", "Objects/geometry-override"),
        new("AddMemo", "AddMemo", "Objects/Memo"),
        new("AddBoxVolume", "AddBoxVolume", "Objects/volume-box"),
        new("AddSphereVolume", "AddSphereVolume", "Objects/volume-sphere"),

        // Build
        new("CompileLevel", "BuildLevel", "Actions/compile"),
        new("CompileLevelAndPlay", "BuildAndPlay", "Actions/play"),
        new("CompileAndPlayPreview", "BuildAndPlayPreview", "Actions/play_fast"),

        // Quick texture targets
        new("TextureFloor", "TextureFloor", "Texture/Floor2"),
        new("TextureCeiling", "TextureCeiling", "Texture/Ceiling2"),
        new("TextureWalls", "TextureWalls", "Texture/Walls2"),

        // Misc tools
        new("EditLevelSettings", "EditLevelSettings", "General/settings"),
        new("Search", "Search", "General/search"),
        new("SearchAndReplaceObjects", "SearchAndReplaceObjects", "General/Find-and-replace"),
    };

    private static readonly Dictionary<string, ToolbarButtonSpec> _byToken =
        _specs.ToDictionary(s => s.Token);

    /// <summary>All known toolbar tokens, in their canonical order (used as the Customize dialog universe).</summary>
    public static IReadOnlyList<string> AllTokens { get; } = _specs.Select(s => s.Token).ToList();

    public static bool TryGet(string token, out ToolbarButtonSpec spec) => _byToken.TryGetValue(token, out spec!);

    /// <summary>Builds the simple Button / ToggleButton for a token (the DrawObjects dropdown is built by the view).</summary>
    public static FrameworkElement? CreateButton(ToolbarButtonSpec spec)
    {
        if (spec.Kind == ToolbarButtonKind.DrawObjectsMenu)
            return null;

        ButtonBase button = spec.Kind == ToolbarButtonKind.Toggle ? new ToggleButton() : new Button();
        button.Content = new Image { Source = IconSources.Load(spec.Icon) };

        EditorToolButton.SetCommand(button, spec.Command);

        if (spec.ActiveMode is { } mode)
            EditorToolButton.SetActiveMode(button, mode);
        if (spec.ConfigFlag is { } flag)
            EditorToolButton.SetConfigFlag(button, flag);
        if (spec.PortalOpacity is { } opacity)
            EditorToolButton.SetPortalOpacity(button, opacity);
        if (spec.PortalEffect is { } effect)
            EditorToolButton.SetPortalEffect(button, effect);

        return button;
    }
}
