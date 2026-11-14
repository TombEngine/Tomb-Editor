#nullable enable

using System.Windows.Input;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF;
using TombLib.WPF.Controls;

namespace TombEditor.Controls;

/// <summary>
/// WPF texture-map view backed by the global editor's configuration. Mirrors the previous
/// <c>PanelTextureMap</c> contract so per-dialog subclasses only override behavioural hooks.
/// </summary>
public class WpfTextureMapView : TextureMapBase
{
    protected readonly Editor _editor;

    public WpfTextureMapView()
    {
        _editor = Editor.Instance;
        _editor.EditorEventRaised += OnEditorEventRaised;
        Unloaded += (_, _) => _editor.EditorEventRaised -= OnEditorEventRaised;
    }

    public new LevelTexture? VisibleTexture
    {
        get => base.VisibleTexture as LevelTexture;
        set => base.VisibleTexture = value;
    }

    protected override float TileSelectionSize => _editor.Configuration.TextureMap_TileSelectionSize;
    protected override bool ResetAttributesOnNewSelection => _editor.Configuration.TextureMap_ResetAttributesOnNewSelection;
    protected override bool MouseWheelMovesTheTextureInsteadOfZooming => _editor.Configuration.TextureMap_MouseWheelMovesTheTextureInsteadOfZooming;
    protected override float NavigationSpeedKeyMove => _editor.Configuration.TextureMap_NavigationSpeedKeyMove;
    protected override float NavigationSpeedKeyZoom => _editor.Configuration.TextureMap_NavigationSpeedKeyZoom;
    protected override float NavigationSpeedMouseZoom => _editor.Configuration.TextureMap_NavigationSpeedMouseZoom;
    protected override float NavigationSpeedMouseWheelZoom => _editor.Configuration.TextureMap_NavigationSpeedMouseWheelZoom;
    protected override float NavigationMaxZoom => _editor.Configuration.TextureMap_NavigationMaxZoom;
    protected override float NavigationMinZoom => _editor.Configuration.TextureMap_NavigationMinZoom;
    protected override bool DrawSelectionDirectionIndicators => _editor.Configuration.TextureMap_DrawSelectionDirectionIndicators;

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        if (obj is Editor.ConfigurationChangedEvent)
            InvalidateVisual();
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        // Mirror the previous PanelTextureMap "click to load" behaviour: when there is no
        // valid texture, a left-click triggers the same load/reload dialog the toolbar uses
        // instead of falling through to selection input.
        if (!(base.VisibleTexture is { IsAvailable: true }))
        {
            var owner = WPFUtils.GetWindowOwner();
            var current = base.VisibleTexture as LevelTexture;

            if (current is not null)
                EditorActions.ReloadResource(owner, _editor.Level.Settings, current);
            else
                EditorActions.AddTexture(owner);

            return;
        }

        base.OnMouseDown(e);
    }

    protected override string GetMissingTextureMessage()
    {
        var current = base.VisibleTexture as LevelTexture;

        if (current is null || string.IsNullOrEmpty(current.Path))
            return "Click here to load new texture file.";

        string fileName = PathC.GetFileNameWithoutExtensionTry(current.Path) ?? string.Empty;
        string head = PathC.IsFileNotFoundException(current.LoadException)
            ? $"Texture file '{fileName}' was not found!\n"
            : $"Unable to load texture from file '{fileName}'.\n";

        return head
            + "Click here to choose a replacement.\n\n"
            + "Path: " + (_editor.Level.Settings.MakeAbsolute(current.Path) ?? string.Empty);
    }
}
