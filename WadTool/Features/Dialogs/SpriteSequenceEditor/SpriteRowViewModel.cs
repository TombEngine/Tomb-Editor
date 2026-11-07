#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Media;
using TombLib;
using TombLib.Wad;

namespace WadTool.Features.Dialogs.SpriteSequenceEditor;

/// <summary>
/// Row wrapper for a <see cref="WadSprite"/> in the sprites grid. The model is a plain struct,
/// so alignment edits (numeric fields, recalculate, replace) write through to the struct here
/// and raise the change notifications for the dependent read-only columns (preview, size).
/// </summary>
public sealed class SpriteRowViewModel : ObservableObject
{
    private readonly Func<WadTexture, ImageSource> _imageProvider;
    private WadSprite _sprite;
    private int _index;

    public SpriteRowViewModel(WadSprite sprite, int index, Func<WadTexture, ImageSource> imageProvider)
    {
        _sprite = sprite;
        _index = index;
        _imageProvider = imageProvider;
    }

    /// <summary>The current state of the wrapped sprite, including all alignment edits.</summary>
    public WadSprite Sprite => _sprite;

    /// <summary>Position in the sequence; shown in the legacy "ID" column.</summary>
    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }

    public ImageSource Image => _imageProvider(_sprite.Texture);

    public string Size => _sprite.Texture.Image.Size.ToString();

    public int AlignmentLeft
    {
        get => _sprite.Alignment.X0;
        set => SetAlignment(new RectangleInt2(value, _sprite.Alignment.Y0, _sprite.Alignment.X1, _sprite.Alignment.Y1), nameof(AlignmentLeft));
    }

    public int AlignmentTop
    {
        get => _sprite.Alignment.Y0;
        set => SetAlignment(new RectangleInt2(_sprite.Alignment.X0, value, _sprite.Alignment.X1, _sprite.Alignment.Y1), nameof(AlignmentTop));
    }

    public int AlignmentRight
    {
        get => _sprite.Alignment.X1;
        set => SetAlignment(new RectangleInt2(_sprite.Alignment.X0, _sprite.Alignment.Y0, value, _sprite.Alignment.Y1), nameof(AlignmentRight));
    }

    public int AlignmentBottom
    {
        get => _sprite.Alignment.Y1;
        set => SetAlignment(new RectangleInt2(_sprite.Alignment.X0, _sprite.Alignment.Y0, _sprite.Alignment.X1, value), nameof(AlignmentBottom));
    }

    /// <summary>Swaps the wrapped sprite for a new one (legacy "Replace sprite").</summary>
    public void Replace(WadSprite sprite)
    {
        _sprite = sprite;

        // Everything is potentially stale: preview, size and all four alignment fields.
        OnPropertyChanged(string.Empty);
    }

    /// <summary>Mirrors the legacy "Recalculate adjustment" button.</summary>
    public void RecalculateAlignment(WadSprite.HorizontalAlignment horAdj, WadSprite.VerticalAlignment verAdj, float scale)
    {
        _sprite.RecalculateAlignment(horAdj, verAdj, scale);
        RaiseAlignmentChanged();
    }

    private void SetAlignment(RectangleInt2 alignment, string propertyName)
    {
        if (_sprite.Alignment == alignment)
            return;

        _sprite.Alignment = alignment;
        OnPropertyChanged(propertyName);
    }

    private void RaiseAlignmentChanged()
    {
        OnPropertyChanged(nameof(AlignmentLeft));
        OnPropertyChanged(nameof(AlignmentTop));
        OnPropertyChanged(nameof(AlignmentRight));
        OnPropertyChanged(nameof(AlignmentBottom));
    }
}
