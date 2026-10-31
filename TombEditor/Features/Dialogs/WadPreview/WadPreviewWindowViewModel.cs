#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombEditor.Features.Dialogs.WadPreview;

/// <summary>
/// View-model for <see cref="WadPreviewWindow"/>: holds the previewed <see cref="Wad2"/>,
/// the currently selected wad object and the moveable skin-substitution logic deciding
/// which object the 3D panel should actually render.
/// </summary>
public partial class WadPreviewWindowViewModel : ObservableObject
{
    private readonly Editor _editor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ObjectToRender))]
    private IWadObject? _selectedObject;

    public Wad2 Wad { get; }

    /// <summary>
    /// The object the preview panel should render: for moveables, substitutes the
    /// game-version skin's meshes into the dummy meshes (mirroring the previous
    /// <c>FormPreviewWad</c> behaviour); all other objects pass through unchanged.
    /// </summary>
    public IWadObject? ObjectToRender
    {
        get
        {
            if (SelectedObject is not WadMoveable moveable)
                return SelectedObject;

            var skinId = new WadMoveableId(
                TrCatalog.GetMoveableSkin(_editor.Level.Settings.GameVersion, moveable.Id.TypeId));
            var skin = _editor.Level.Settings.WadTryGetMoveable(skinId);

            return skin != null && skin != moveable
                ? moveable.ReplaceDummyMeshes(skin)
                : moveable;
        }
    }

    public WadPreviewWindowViewModel(Wad2 wad, Editor? editor = null)
    {
        Wad = wad;
        _editor = editor ?? Editor.Instance;
    }

    /// <summary>Sets the selection from a wad-tree object id, or clears it for <see langword="null"/>.</summary>
    public void SelectObject(IWadObjectId? id)
        => SelectedObject = id == null ? null : Wad.TryGet(id);
}
